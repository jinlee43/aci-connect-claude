using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Safety;

/// <summary>
/// 주간 안전 보고서 상세 페이지.
///
/// 접근 방법:
///   신규: /Safety/Report?projectId=X&amp;weekStart=YYYY-MM-DD
///   기존: /Safety/Report?reportId=X
///
/// 상태별 동작:
///   없음(Missing)     → 파일 업로드 + No Work 버튼
///   Draft(파일 있음)  → 파일 정보 표시 + [검토완료] (PM/SPM)
///   NoWork            → No Work 표시 + [검토완료] (PM/SPM)
///   Reviewed          → 파일/NoWork 정보 + [승인] (SafetyManager/Admin)
///   NoWorkReviewed    → No Work + [승인] (SafetyManager/Admin)
///   Approved / NoWorkApproved → 읽기 전용
/// </summary>
[Authorize]
public class ReportModel : PageModel
{
    private readonly ISafetyWkRepService _svc;
    private readonly AppDbContext        _db;

    public ReportModel(ISafetyWkRepService svc, AppDbContext db)
    {
        _svc = svc;
        _db  = db;
    }

    // ── Route / query params ──────────────────────────────────────────────────
    [BindProperty(SupportsGet = true)] public int?    ReportId  { get; set; }
    [BindProperty(SupportsGet = true)] public int?    ProjectId { get; set; }
    [BindProperty(SupportsGet = true)] public string? WeekStart { get; set; }

    // 돌아갈 그리드 필터 파라미터
    [BindProperty(SupportsGet = true)] public string? From { get; set; }
    [BindProperty(SupportsGet = true)] public string? To   { get; set; }

    // ── View data ─────────────────────────────────────────────────────────────
    public SafetyWkRep?         Report   { get; set; }
    public Project?             Project  { get; set; }
    public DateOnly             Week     { get; set; }
    public SafetyWkRepSettings? Settings { get; set; }

    /// <summary>해당 주의 기본 보고 날짜 (DefaultSubmitDay 기준)</summary>
    public DateOnly DefaultReportDate { get; set; }

    public bool IsNew        => ReportId is null or 0;
    public bool CanUpload    { get; set; }   // 파일 업로드 가능
    public bool CanMarkNoWork{ get; set; }   // No Work 마킹 가능
    public bool CanReview    { get; set; }   // 검토 완료 가능 (PM/SPM)
    public bool CanApprove   { get; set; }   // 승인 가능 (SafetyManager/Admin)

    // ── GET ───────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync()
    {
        if (IsNew)
        {
            if (ProjectId is null || !DateOnly.TryParse(WeekStart, out var weekDate))
                return BadRequest("projectId and weekStart are required for new reports.");

            var project = await _db.Projects.FindAsync(ProjectId.Value);
            if (project == null) return NotFound("Project not found.");
            Project = project;
            Week = SafetyWkRepService.GetWeekMonday(weekDate);

            // 이미 레코드가 있으면 상세로 redirect
            var existing = await _svc.GetReportByWeekAsync(ProjectId.Value, Week);
            if (existing != null)
                return RedirectToPage(new { reportId = existing.Id, from = From, to = To });
        }
        else
        {
            var report = await _svc.GetReportAsync(ReportId!.Value);
            if (report == null) return NotFound("Report not found.");
            Report  = report;
            Project = report.Project;
            Week    = report.WeekStartDate;
        }

        // 프로젝트 설정 로드 → 기본 보고 날짜 계산
        if (Project != null)
        {
            Settings = await _svc.GetSettingsAsync(Project.Id);
            var submitDay = Settings?.DefaultSubmitDay ?? DayOfWeek.Friday;
            DefaultReportDate = ComputeDefaultReportDate(Week, submitDay);
        }

        await ComputePermissionsAsync();
        return Page();
    }

    // ── POST: Mark No Work ────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostMarkNoWorkAsync(
        int projectId, string weekStart, string? reportDate)
    {
        if (!CanMarkNoWorkAllowed()) return Forbid();
        if (!DateOnly.TryParse(weekStart, out var weekDate))
            return BadRequest("Invalid week date.");

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        DateOnly? parsedReportDate = DateOnly.TryParse(reportDate, out var rd) ? rd : null;

        try
        {
            var report = await _svc.MarkNoWorkAsync(projectId, weekDate, userId, userName, parsedReportDate);
            TempData["Success"] = "Marked as No Work.";
            return RedirectToPage(new { reportId = report.Id, from = From, to = To });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage(new { projectId, weekStart, from = From, to = To });
        }
    }

    // ── POST: Review ──────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostReviewAsync(int reportId, string? notes)
    {
        // 검토 권한은 페이지 로드 후 체크 필요 — 여기서 재계산
        var report = await _svc.GetReportAsync(reportId);
        if (report == null) return NotFound();

        Project = report.Project;
        Week    = report.WeekStartDate;
        await ComputePermissionsAsync();

        if (!CanReview) return Forbid();

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        try
        {
            await _svc.ReviewReportAsync(reportId, notes, userId, userName);
            TempData["Success"] = "Report marked as Reviewed.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { reportId, from = From, to = To });
    }

    // ── POST: Approve ─────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostApproveAsync(int reportId, string? notes)
    {
        if (!CanApproveAllowed()) return Forbid();

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        try
        {
            await _svc.ApproveReportAsync(reportId, notes, userId, userName);
            TempData["Success"] = "Report approved and locked.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { reportId, from = From, to = To });
    }

    // ── POST: Void ────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostVoidAsync(int reportId, string? reason)
    {
        if (!CanApproveAllowed()) return Forbid();

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        try
        {
            await _svc.VoidApprovalAsync(reportId, reason, userId, userName);
            TempData["Success"] = "Approval voided.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { reportId, from = From, to = To });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task ComputePermissionsAsync()
    {
        bool isAdmin = User.IsInRole(PrivilegeCodes.Admin);

        // 승인 권한: Admin / SafetyAdmin / SafetyManager
        CanApprove = isAdmin
                  || User.IsInRole(PrivilegeCodes.SafetyAdmin)
                  || User.IsInRole(PrivilegeCodes.SafetyManager);

        // 검토 권한: Admin 또는 해당 프로젝트의 PM/SPM
        if (isAdmin)
        {
            CanReview = true;
        }
        else
        {
            var empIdStr = User.FindFirst("EmployeeId")?.Value;
            if (int.TryParse(empIdStr, out var empId) && Project != null)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                CanReview = await _db.EmpRoles
                    .AnyAsync(r => r.EmployeeId == empId
                                && r.OrgUnit.ProjectId == Project.Id
                                && r.JobPositionId != null
                                && (r.JobPosition!.Code == "PM" || r.JobPosition!.Code == "SPM")
                                && (r.EndDate == null || r.EndDate >= today));
            }
        }

        // 업로드 권한: Safety staff, ProjectManager/Superintendent (담당 프로젝트)
        bool isSafetyStaff = isAdmin
            || User.IsInRole(PrivilegeCodes.SafetyAdmin)
            || User.IsInRole(PrivilegeCodes.SafetyManager)
            || User.IsInRole(PrivilegeCodes.SafetyUser);

        bool locked = Report?.IsLocked == true;

        if (isSafetyStaff)
        {
            CanUpload     = !locked;
            CanMarkNoWork = !locked;
        }
        else if (User.IsInRole(PrivilegeCodes.ProjectManager)
              || User.IsInRole(PrivilegeCodes.Superintendent))
        {
            var (userId, _) = GetUser();
            if (userId > 0 && Project != null)
            {
                var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
                bool isAssigned = assigned.Contains(Project.Id);
                // 검토 이상은 현장 사용자가 수정 불가
                bool notReviewed = Report == null
                    || Report.Status is SafetyWkRepStatus.Draft or SafetyWkRepStatus.NoWork;
                CanUpload     = isAssigned && notReviewed;
                CanMarkNoWork = isAssigned && notReviewed;
            }
        }
    }

    /// <summary>
    /// 주의 월요일 + 요일 → 해당 주 내 날짜 계산.
    /// 예) Monday=Apr14, Friday → Apr18
    /// </summary>
    private static DateOnly ComputeDefaultReportDate(DateOnly weekMonday, DayOfWeek day)
    {
        int offset = ((int)day - (int)DayOfWeek.Monday + 7) % 7;
        return weekMonday.AddDays(offset);
    }

    private bool CanMarkNoWorkAllowed() =>
        User.IsInRole(PrivilegeCodes.Admin)
        || User.IsInRole(PrivilegeCodes.SafetyAdmin)
        || User.IsInRole(PrivilegeCodes.SafetyManager)
        || User.IsInRole(PrivilegeCodes.SafetyUser)
        || User.IsInRole(PrivilegeCodes.ProjectManager)
        || User.IsInRole(PrivilegeCodes.Superintendent);

    private bool CanApproveAllowed() =>
        User.IsInRole(PrivilegeCodes.Admin)
        || User.IsInRole(PrivilegeCodes.SafetyAdmin)
        || User.IsInRole(PrivilegeCodes.SafetyManager);

    private (int userId, string userName) GetUser()
    {
        var idStr = User.FindFirst("UserId")?.Value
                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id) || id <= 0) return (0, "");
        return (id, User.Identity?.Name ?? "Unknown");
    }
}
