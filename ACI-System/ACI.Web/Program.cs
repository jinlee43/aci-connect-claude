using ACI.Web.Data;
using ACI.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

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

builder.Services.AddAuthorization();

// ─── Razor Pages + MVC ────────────────────────────────────────────────────
builder.Services.AddRazorPages()
    .AddRazorRuntimeCompilation();

builder.Services.AddControllers();

// ─── Application Services ─────────────────────────────────────────────────
builder.Services.AddScoped<IGanttDataService, GanttDataService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ILookaheadService, LookaheadService>();
builder.Services.AddScoped<IWeeklyPlanService, WeeklyPlanService>();

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
