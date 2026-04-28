using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Daily;

/// <summary>
/// Daily Report 목록 (복수 프로젝트 지원 + 페이징).
/// 작성 권한: Superintendent
/// 검토 권한: ProjectEngineer / ProjectManager
/// 승인 권한: ProjectManager
/// 읽기 전용: HrAdmin (모든 프로젝트)
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly IDailyReportService _svc;
    private readonly AppDbContext        _db;

    public IndexModel(IDailyReportService svc, AppDbContext db)
    {
        _svc = svc;
        _db  = db;
    }

    // ── 파라미터 ─────────────────────────────────────────────────────────────
    /// <summary>null = All projects.</summary>
    [BindProperty(SupportsGet = true)] public int?    FilterProjectId { get; set; }
    /// <summary>3m / 6m / 9m / 1yr / all. null → custom From/To.</summary>
    [BindProperty(SupportsGet = true)] public string? Period          { get; set; }
    [BindProperty(SupportsGet = true)] public string? From            { get; set; }
    [BindProperty(SupportsGet = true)] public string? To              { get; set; }
    [BindProperty(SupportsGet = true)] public int     PageNum         { get; set; } = 1;
    [BindProperty(SupportsGet = true)] public int     PageSize        { get; set; } = 25;

    // ── 뷰 데이터 ─────────────────────────────────────────────────────────────
    public bool CanWrite   { get; set; }
    public bool CanReview  { get; set; }
    public bool CanApprove { get; set; }

    public List<ProjectItem>  Projects   { get; set; } = [];
    public List<DailyReport>  Reports    { get; set; } = [];
    public int                TotalCount { get; set; }
    public int                TotalPages => (int)Math.Ceiling((double)TotalCount / Math.Max(1, PageSize));

    public int[] PageSizeOptions { get; } = [15, 25, 50, 100];

    // 선택된 단일 프로젝트 정보 (Today's Report 버튼용)
    public string SelectedProjectCode { get; set; } = "";
    public string SelectedProjectName { get; set; } = "";

    // ── GET ───────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync()
    {
        var (userId, _) = GetUser();
        if (userId <= 0) return Forbid();

        CanWrite   = User.IsInRole(PrivilegeCodes.Superintendent);
        CanReview  = User.IsInRole(PrivilegeCodes.ProjectEngineer)
                  || User.IsInRole(PrivilegeCodes.ProjectManager);
        CanApprove = User.IsInRole(PrivilegeCodes.ProjectManager);

        // ── 접근 가능한 프로젝트 ID 목록 ─────────────────────────────────────
        var canReadAll = User.IsInRole(PrivilegeCodes.Admin)
                      || User.IsInRole(PrivilegeCodes.HrAdmin);

        List<int> accessibleIds;
        if (canReadAll)
            accessibleIds = await _db.Projects
                .Where(p => p.IsActive)
                .Select(p => p.Id)
                .ToListAsync();
        else
            accessibleIds = await _svc.GetAssignedProjectIdsAsync(userId);

        // 담당 프로젝트 없음: Projects = [] 그대로, 콤보는 "All Projects" 선택 유지
        if (accessibleIds.Count == 0) return Page();

        // ── 콤보박스용 프로젝트 목록 ─────────────────────────────────────────
        Projects = await _db.Projects
            .Where(p => p.IsActive && accessibleIds.Contains(p.Id))
            .OrderBy(p => p.ProjectCode)
            .Select(p => new ProjectItem(p.Id, p.ProjectCode, p.Name))
            .ToListAsync();

        // ── 필터 프로젝트 유효성 검사 ─────────────────────────────────────────
        if (FilterProjectId.HasValue && !accessibleIds.Contains(FilterProjectId.Value))
            return Forbid();

        var queryProjectIds = FilterProjectId.HasValue
            ? [FilterProjectId.Value]
            : accessibleIds;

        // 선택된 단일 프로젝트 정보
        if (FilterProjectId.HasValue)
        {
            var proj = Projects.FirstOrDefault(p => p.Id == FilterProjectId.Value);
            SelectedProjectCode = proj?.Code ?? "";
            SelectedProjectName = proj?.Name ?? "";
        }

        // ── 날짜 범위 ─────────────────────────────────────────────────────────
        var (fromDate, toDate) = ResolveDateRange();

        // ── 페이징 ────────────────────────────────────────────────────────────
        if (!PageSizeOptions.Contains(PageSize)) PageSize = 25;
        if (PageNum < 1) PageNum = 1;

        var (items, total) = await _svc.GetReportsPagedAsync(
            queryProjectIds, fromDate, toDate,
            (PageNum - 1) * PageSize, PageSize);

        Reports    = items;
        TotalCount = total;

        return Page();
    }

    // ── POST: Delete Draft ────────────────────────────────────────────────────
    [BindProperty] public int DeleteReportId { get; set; }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var (userId, _) = GetUser();
        if (userId <= 0) return Forbid();
        if (!User.IsInRole(PrivilegeCodes.Superintendent))
            return Forbid();

        if (DeleteReportId <= 0) return BadRequest("Invalid report ID.");

        // 담당 프로젝트만 삭제 가능
        var report = await _db.DailyReports.FindAsync(DeleteReportId);
        if (report == null) return NotFound();

        var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
        if (!assigned.Contains(report.ProjectId)) return Forbid();

        try
        {
            await _svc.DeleteDraftAsync(DeleteReportId);
            TempData["Success"] = "Draft report deleted.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        // 현재 필터 상태를 유지하며 목록으로 돌아감
        return RedirectToPage(new
        {
            filterProjectId = FilterProjectId,
            period          = Period,
            from            = From,
            to              = To,
            pageNum         = PageNum,
            pageSize        = PageSize,
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    /// <summary>Period 쇼트컷 또는 From/To를 DateOnly? 쌍으로 변환.</summary>
    private (DateOnly? from, DateOnly? to) ResolveDateRange()
    {
        var today  = DateOnly.FromDateTime(DateTime.Today);
        var period = (Period ?? "").ToLowerInvariant().Trim();

        // All: 날짜 제한 없음
        if (period == "all") { From = null; To = null; return (null, null); }

        // Period 쇼트컷 → From/To 설정
        int? periodDays = period switch
        {
            "1yr" => 365,
            "9m"  => 270,
            "6m"  => 180,
            "3m"  => 90,
            _     => null
        };
        if (periodDays.HasValue)
        {
            var f = today.AddDays(-periodDays.Value);
            From = f.ToString("yyyy-MM-dd");
            To   = today.ToString("yyyy-MM-dd");
            return (f, today);
        }

        // Custom: 사용자가 직접 From/To 입력
        if (DateOnly.TryParse(From, out var cf) && DateOnly.TryParse(To, out var ct))
            return (cf, ct);

        // 기본값: 3m
        var def = today.AddDays(-90);
        From   = def.ToString("yyyy-MM-dd");
        To     = today.ToString("yyyy-MM-dd");
        Period = "3m";
        return (def, today);
    }

    private (int userId, string userName) GetUser()
    {
        var idStr = User.FindFirst("UserId")?.Value
                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id) || id <= 0) return (0, "");
        return (id, User.Identity?.Name ?? "Unknown");
    }

    public static (string css, string icon, string label) StatusBadge(DailyReportStatus s) => s switch
    {
        DailyReportStatus.Draft          => ("bg-secondary",         "bi-pencil",           "Draft"),
        DailyReportStatus.Submitted      => ("bg-primary",           "bi-send",             "Submitted"),
        DailyReportStatus.Reviewed       => ("bg-info text-dark",    "bi-eye-fill",         "Reviewed"),
        DailyReportStatus.Approved       => ("bg-success",           "bi-shield-check",     "Approved"),
        DailyReportStatus.Voided         => ("bg-danger",            "bi-x-circle-fill",    "Voided"),
        DailyReportStatus.NoWork         => ("bg-warning text-dark", "bi-dash-circle",      "No Work"),
        DailyReportStatus.NoWorkApproved => ("bg-success",           "bi-dash-circle-fill", "No Work (Apv)"),
        _                                => ("bg-secondary",         "bi-question",         "Unknown"),
    };
}

public record ProjectItem(int Id, string Code, string Name);
