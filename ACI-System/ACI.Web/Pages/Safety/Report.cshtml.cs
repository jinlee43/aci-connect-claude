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
///   없음(Missing)     → 파일 업로드 or No Work
///   Staged(파일 있음) → 파일 목록 + 추가 업로드 + Submit 버튼
///   Draft             → 파일 목록 + 추가 업로드 + [Mark as Reviewed] (PM/SPM)
///   NoWork            → No Work 표시 + [Mark as Reviewed] (PM/SPM)
///   Reviewed / NoWorkReviewed → 읽기전용(업로드 잠금) + [Approve] (SafetyManager/SafetyAdmin)
///   Approved / NoWorkApproved → 읽기 전용
///   Voided            → 파일 목록 + 추가 업로드 → Staged 로 전환 후 재제출
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

    [BindProperty(SupportsGet = true)] public string? From  { get; set; }
    [BindProperty(SupportsGet = true)] public string? To    { get; set; }
    [BindProperty(SupportsGet = true)] public bool    Modal { get; set; }

    // ── View data ─────────────────────────────────────────────────────────────
    public SafetyWkRep?         Report   { get; set; }
    public Project?             Project  { get; set; }
    public DateOnly             Week     { get; set; }
    public SafetyWkRepSettings? Settings { get; set; }

    public DateOnly DefaultReportDate { get; set; }

    public bool IsNew           => ReportId is null or 0;
    public bool CanUpload       { get; set; }
    public bool CanMarkNoWork   { get; set; }
    public bool CanReview       { get; set; }
    public bool CanApprove      { get; set; }
    public bool CanVoid         { get; set; }

    /// <summary>현재 사용자가 이 보고서의 제출자인지 여부.</summary>
    public bool IsSubmitter     { get; set; }

    /// <summary>Admin 또는 SafetyAdmin — 제출자 제한 예외 적용.</summary>
    public bool IsAdminOverride { get; set; }

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

            var existing = await _svc.GetReportByWeekAsync(ProjectId.Value, Week);
            if (existing != null)
                return RedirectToPage(new { reportId = existing.Id, from = From, to = To, modal = Modal });
        }
        else
        {
            var report = await _svc.GetReportAsync(ReportId!.Value);
            if (report == null) return NotFound("Report not found.");
            Report  = report;
            Project = report.Project;
            Week    = report.WeekStartDate;
        }

        if (Project != null)
        {
            Settings = await _svc.GetSettingsAsync(Project.Id);
            var submitDay = Settings?.DefaultSubmitDay ?? DayOfWeek.Friday;
            DefaultReportDate = ComputeDefaultReportDate(Week, submitDay);
        }

        var (currentUserId, _) = GetUser();
        IsSubmitter     = currentUserId > 0 && Report?.UploadedById == currentUserId;
        IsAdminOverride = User.IsInRole(PrivilegeCodes.Admin)
                       || User.IsInRole(PrivilegeCodes.SafetyAdmin);

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
            return RedirectToPage(new { reportId = report.Id, from = From, to = To, modal = Modal });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage(new { projectId, weekStart, from = From, to = To, modal = Modal });
        }
    }

    // ── POST: Submit (Staged → Draft) ─────────────────────────────────────────
    public async Task<IActionResult> OnPostSubmitAsync(int reportId, string? reportDate)
    {
        var report = await _svc.GetReportAsync(reportId);
        if (report == null) return NotFound();

        Project = report.Project;
        Week    = report.WeekStartDate;
        await ComputePermissionsAsync();

        if (!CanUpload) return Forbid();

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        DateOnly? parsedDate = DateOnly.TryParse(reportDate, out var rd) ? rd : null;

        try
        {
            await _svc.SubmitReportAsync(reportId, parsedDate, userId, userName);
            TempData["Success"] = "Report submitted for review.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { reportId, from = From, to = To, modal = Modal });
    }

    // ── POST: Delete ──────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostDeleteAsync(int reportId)
    {
        var report = await _svc.GetReportAsync(reportId);
        if (report == null) return NotFound();

        if (report.IsLocked || report.Status == SafetyWkRepStatus.Draft)
            return Forbid();

        Project = report.Project;
        Week    = report.WeekStartDate;
        await ComputePermissionsAsync();
        if (!CanUpload && !CanMarkNoWork) return Forbid();

        var projectId = report.ProjectId;
        var weekStart = report.WeekStartDate.ToString("yyyy-MM-dd");

        try
        {
            await _svc.DeleteReportAsync(reportId);
            TempData["Success"] = "Report cancelled successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage(new { reportId, from = From, to = To, modal = Modal });
        }

        return RedirectToPage(new { projectId, weekStart, from = From, to = To, modal = Modal });
    }

    // ── POST: Unsubmit (Draft → Staged, 제출 취소) ───────────────────────────
    public async Task<IActionResult> OnPostUnsubmitAsync(int reportId)
    {
        var report = await _svc.GetReportAsync(reportId);
        if (report == null) return NotFound();

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        bool isAdmin = User.IsInRole(PrivilegeCodes.Admin)
                    || User.IsInRole(PrivilegeCodes.SafetyAdmin);
        if (!isAdmin && report.UploadedById != userId) return Forbid();

        try
        {
            await _svc.UnsubmitReportAsync(reportId, userId, userName);
            TempData["Success"] = "Submission cancelled. Report returned to Staged status.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { reportId, from = From, to = To, modal = Modal });
    }

    // ── POST: Review ──────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostReviewAsync(int reportId, string? notes)
    {
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

        return RedirectToPage(new { reportId, from = From, to = To, modal = Modal });
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

        return RedirectToPage(new { reportId, from = From, to = To, modal = Modal });
    }

    // ── POST: Void ────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostVoidAsync(int reportId, string? reason)
    {
        if (!CanVoidAllowed()) return Forbid();

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        try
        {
            await _svc.VoidApprovalAsync(reportId, reason, userId, userName);
            TempData["Success"] = "Approval voided.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { reportId, from = From, to = To, modal = Modal });
    }

    // ── POST: Unvoid ──────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostUnvoidAsync(int reportId)
    {
        if (!CanVoidAllowed()) return Forbid();

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        try
        {
            await _svc.UnvoidAsync(reportId, userId, userName);
            TempData["Success"] = "Void cancelled. Report restored to Approved status.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { reportId, from = From, to = To, modal = Modal });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task ComputePermissionsAsync()
    {
        bool isAdmin = User.IsInRole(PrivilegeCodes.Admin);

        CanApprove = isAdmin
                  || User.IsInRole(PrivilegeCodes.SafetyAdmin)
                  || User.IsInRole(PrivilegeCodes.SafetyManager);

        CanVoid = isAdmin || User.IsInRole(PrivilegeCodes.SafetyAdmin);

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

        bool isSafetyStaff = isAdmin
            || User.IsInRole(PrivilegeCodes.SafetyAdmin)
            || User.IsInRole(PrivilegeCodes.SafetyManager)
            || User.IsInRole(PrivilegeCodes.SafetyUser);

        bool locked       = Report?.IsLocked == true;
        // 업로드는 Reviewed 이상(리뷰됨/승인됨) 모두 잠금
        bool uploadLocked = Report?.Status is SafetyWkRepStatus.Reviewed
                                           or SafetyWkRepStatus.NoWorkReviewed
                                           or SafetyWkRepStatus.Approved
                                           or SafetyWkRepStatus.NoWorkApproved;

        if (isSafetyStaff)
        {
            CanUpload     = !uploadLocked;
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
                bool editable = Report == null
                    || Report.Status is SafetyWkRepStatus.Staged
                                     or SafetyWkRepStatus.Draft
                                     or SafetyWkRepStatus.NoWork
                                     or SafetyWkRepStatus.Voided;
                CanUpload     = isAssigned && editable;
                CanMarkNoWork = isAssigned && editable;
            }
        }
    }

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

    private bool CanVoidAllowed() =>
        User.IsInRole(PrivilegeCodes.Admin)
        || User.IsInRole(PrivilegeCodes.SafetyAdmin);

    private (int userId, string userName) GetUser()
    {
        var idStr = User.FindFirst("UserId")?.Value
                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id) || id <= 0) return (0, "");
        return (id, User.Identity?.Name ?? "Unknown");
    }
}
