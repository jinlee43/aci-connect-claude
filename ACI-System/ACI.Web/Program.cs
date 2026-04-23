using ACI.Web.Data;
using ACI.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Kestrel 요청 크기 제한 (기본 30 MB → 200 MB) ─────────────────────────
builder.WebHost.ConfigureKestrel(options =>
    options.Limits.MaxRequestBodySize = 2002L * 1024 * 1024);  // 2000 MB + 오버헤드

// ─── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(
               Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));

// ─── Custom Cookie Auth (NOT ASP.NET Identity) ────────────────────────────
builder.Services.AddAuthentication("AciCookies")
    .AddCookie("AciCookies", options =>
    {
        options.LoginPath         = "/Account/Login";
        options.LogoutPath        = "/Account/Logout";
        options.AccessDeniedPath  = "/Account/AccessDenied";
        options.ExpireTimeSpan    = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly   = true;
        options.Cookie.SameSite   = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
    });

// AddAuthorization은 아래 정책 설정에서 처리함

// ─── Multipart form body 제한 (기본 128 MB → 2000 MB) ────────────────────
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    options.MultipartBodyLengthLimit = 2002L * 1024 * 1024);

// ─── Razor Pages + MVC ────────────────────────────────────────────────────
builder.Services.AddRazorPages(options =>
{
    // ─ HR 영역(/Hr/*): HR 관련 권한자(HrUser / HrManager / HrAdmin / Admin)만 접근 ──
    //   Employees / OrgUnits / JobPositions / Users 기본 조회 및 일반 편집(Detail)
    //   민감 정보(AdminDetail) 및 Role 부여는 각 PageModel에서 [Authorize(Policy="HrAdmin")]로 추가 보호.
    options.Conventions.AuthorizeFolder("/Hr", "Hr");

    // ─ 시스템 Admin 영역(/SystemAdmin/*): Admin만 접근 ──────────────────
    //   순수 시스템 설정(감사 로그, 메일/SMTP 등) 용도로 향후 사용
    options.Conventions.AuthorizeFolder("/SystemAdmin", "SystemAdmin");

    // ─ 프로젝트 마스터 영역(/Trades/*): 인증된 사용자 전체 조회 가능 ────
    //   편집(Save/Toggle) 핸들러는 각 PageModel에서 [Authorize(Policy="ProjectAdmin")]로 추가 보호
    options.Conventions.AuthorizeFolder("/Trades");

    // ─ Safety 영역(/Safety/*): 인증된 사용자 접근 가능 ──────────────────
    //   Reports: [Authorize(Policy="SafetyUser")], Settings: [Authorize(Policy="SafetyAdmin")],
    //   MyReports: [Authorize] (PM/Superintendent 포함 모든 인증 사용자)
    options.Conventions.AuthorizeFolder("/Safety");
})
.AddRazorRuntimeCompilation();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // SVAR Gantt 클라이언트가 숫자를 문자열로 보낼 수 있음 → 허용
        // (ID/날짜 필드가 문자열로 직렬화돼도 int/decimal로 역직렬화)
        o.JsonSerializerOptions.NumberHandling =
            System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
    });

// ─── Authorization Policies ───────────────────────────────────────────────
builder.Services.AddAuthorization(options =>
{
    // ── Priv 체크 주의사항 ──────────────────────────────────────────────
    //   로그인 시 PrivilegeExpander 가 상위 priv 의 하위들을 전부 Role 클레임으로 심는다.
    //   즉 한 사용자가 여러 Role 클레임을 가질 수 있으므로 반드시 User.IsInRole(...)
    //   (= 모든 Role 클레임 순회)로 체크해야 한다. FindFirst(ClaimTypes.Role) 는 첫 번째
    //   claim 만 반환하므로 다중 priv 사용자에서 false negative 가 난다.

    // HR 정책(느슨): /Hr/* 폴더 전체 접근 가능
    //   - HrUser, HrManager, HrAdmin, Admin
    //   - Detail 편집(HrUser 이상) 및 OrgUnits/JobPositions/Users 조회
    options.AddPolicy("Hr", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(PrivilegeCodes.Admin)
            || ctx.User.IsInRole(PrivilegeCodes.HrAdmin)
            || ctx.User.IsInRole(PrivilegeCodes.HrManager)
            || ctx.User.IsInRole(PrivilegeCodes.HrUser)));

    // HR Admin 정책(엄격): AdminDetail / Role 부여 / 민감 데이터 전용
    //   - Admin 또는 HrAdmin만 접근 가능
    //   - 대상: /Hr/Employees/AdminDetail, EmpRole 관리 핸들러, EmployeeDocumentController,
    //          Users 페이지의 권한 부여 핸들러
    // Admin은 시스템 전체 관리자로서 HrAdmin 권한을 포함 (Expander 가 자동 전개).
    options.AddPolicy("HrAdmin", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(PrivilegeCodes.Admin)
            || ctx.User.IsInRole(PrivilegeCodes.HrAdmin)));

    // 시스템 Admin 전용 정책: Admin만 접근 가능
    //   - 대상 페이지: /SystemAdmin/* (감사 로그, 메일/SMTP 설정 등 — 향후)
    options.AddPolicy("SystemAdmin", policy =>
        policy.RequireAssertion(ctx => ctx.User.IsInRole(PrivilegeCodes.Admin)));

    // Project Admin 정책: Trades & Subs 등 프로젝트 마스터 편집 권한.
    //   - 조회는 누구나, 편집(Save/Toggle/Delete)은 Admin ∪ LsProjAdmin ∪ JocProjAdmin
    //   - 사용처: /Trades/* 페이지의 OnPost* 핸들러, 관련 API POST/PUT/DELETE
    options.AddPolicy("ProjectAdmin", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(PrivilegeCodes.Admin)
            || ctx.User.IsInRole(PrivilegeCodes.LsProjAdmin)
            || ctx.User.IsInRole(PrivilegeCodes.JocProjAdmin)));

    // ── Safety 정책 ──────────────────────────────────────────────────────────

    // SafetyUser 이상: 보고서 업로드·편집·NoWork 표시 가능 (전체 프로젝트)
    //   - SafetyUser, SafetyManager, SafetyAdmin, Admin
    options.AddPolicy("SafetyUser", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(PrivilegeCodes.Admin)
            || ctx.User.IsInRole(PrivilegeCodes.SafetyAdmin)
            || ctx.User.IsInRole(PrivilegeCodes.SafetyManager)
            || ctx.User.IsInRole(PrivilegeCodes.SafetyUser)));

    // SafetyManager 이상: 보고서 승인 및 승인 취소 가능
    //   - SafetyManager, SafetyAdmin, Admin
    options.AddPolicy("SafetyManager", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(PrivilegeCodes.Admin)
            || ctx.User.IsInRole(PrivilegeCodes.SafetyAdmin)
            || ctx.User.IsInRole(PrivilegeCodes.SafetyManager)));

    // SafetyAdmin 전용: 시스템 전체 Safety 관리 (향후 확장용)
    options.AddPolicy("SafetyAdmin", policy =>
        policy.RequireAssertion(ctx =>
            ctx.User.IsInRole(PrivilegeCodes.Admin)
            || ctx.User.IsInRole(PrivilegeCodes.SafetyAdmin)));
});

// ─── File Storage Options ─────────────────────────────────────────────────
builder.Services.Configure<ACI.Web.Controllers.FileStorageOptions>(
    builder.Configuration.GetSection(ACI.Web.Controllers.FileStorageOptions.Section));

// ─── Application Services ─────────────────────────────────────────────────
builder.Services.AddSingleton<ACI.Web.Services.IEncryptionService, ACI.Web.Services.EncryptionService>();
builder.Services.AddScoped<IGanttDataService, GanttDataService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ILookaheadService, LookaheadService>();
builder.Services.AddScoped<IWeeklyPlanService, WeeklyPlanService>();
builder.Services.AddScoped<IProgressScheduleService, ProgressScheduleService>();
builder.Services.AddScoped<IBaselineService, BaselineService>();
builder.Services.AddScoped<ISimulationService, SimulationService>();
builder.Services.AddScoped<ISafetyWkRepService, SafetyWkRepService>();
builder.Services.AddScoped<IUserIdGenerator, UserIdGenerator>();

// ─── HTTP Context Accessor (for auth helper) ──────────────────────────────
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ─── Middleware Pipeline ───────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// ─── DB 초기화 (개발환경) ──────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

app.Run();
