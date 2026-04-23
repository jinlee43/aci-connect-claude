using ACI.Web.Controllers;
using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ACI.Web.Pages.Safety;

/// <summary>
/// PM / Superintendent용 주간 보고서 페이지 — 담당 프로젝트 한정.
///
/// 권한:
///   업로드/NoWork/삭제 : 인증된 모든 사용자 (담당 프로젝트, Draft/NoWork/Voided 상태만)
///   검토(Review)       : PM(ProjectManager) + SafetyManager+  (담당 프로젝트)
///   검토취소(Unreview) : SafetyManager+
/// </summary>
[Authorize]
public class MyReportsModel : PageModel
{
    private readonly ISafetyWkRepService _svc;
    private readonly AppDbContext        _db;
    private readonly FileStorageOptions  _storage;
    private readonly ILogger<MyReportsModel> _logger;

    public MyReportsModel(
        ISafetyWkRepService svc, AppDbContext db,
        IOptions<FileStorageOptions> storage, ILogger<MyReportsModel> logger)
    {
        _svc     = svc;
        _db      = db;
        _storage = storage.Value;
        _logger  = logger;
    }

    // ── Filter ────────────────────────────────────────────────────────────────
    [BindProperty(SupportsGet = true)] public string? From { get; set; }
    [BindProperty(SupportsGet = true)] public string? To   { get; set; }

    // ── View data ─────────────────────────────────────────────────────────────
    public List<DateOnly>   Weeks      { get; set; } = [];
    public List<ProjectRow> Rows       { get; set; } = [];

    /// <summary>SafetyManager+ 또는 ProjectManager(담당)가 검토 가능.</summary>
    public bool CanReview     { get; set; }
    public bool CanUnreview   { get; set; }   // SafetyManager+ 전용

    public record ProjectRow(
        int     ProjectId,
        string  ProjectCode,
        string  ProjectName,
        Dictionary<DateOnly, SafetyWkRep?> Slots);

    // ── GET ───────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync()
    {
        var (userId, _) = GetUser();
        if (userId <= 0) return Forbid();

        CanReview   = IsSafetyManager() || IsProjectManager();
        CanUnreview = IsSafetyManager();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var from  = ParseDate(From) ?? GetWeekMonday(today).AddDays(-7 * 7);
        var to    = ParseDate(To)   ?? today;
        From = from.ToString("yyyy-MM-dd");
        To   = to.ToString("yyyy-MM-dd");

        var cur = GetWeekMonday(from);
        while (cur <= to) { Weeks.Add(cur); cur = cur.AddDays(7); }

        var projectIds = await _svc.GetAssignedProjectIdsAsync(userId);
        if (projectIds.Count == 0) return Page();

        var reports = await _db.SafetyWkReps
            .Where(r => r.IsActive
                     && projectIds.Contains(r.ProjectId)
                     && r.WeekStartDate >= GetWeekMonday(from)
                     && r.WeekStartDate <= to)
            .ToListAsync();

        var projects = await _db.Projects
            .Where(p => p.IsActive && projectIds.Contains(p.Id))
            .OrderBy(p => p.ProjectCode)
            .ToListAsync();

        var byProject = reports
            .GroupBy(r => r.ProjectId)
            .ToDictionary(g => g.Key, g => g.ToDictionary(r => r.WeekStartDate));

        Rows = projects.Select(p =>
        {
            byProject.TryGetValue(p.Id, out var map);
            map ??= [];
            var slots = Weeks.ToDictionary(w => w,
                w => map.TryGetValue(w, out var r) ? r : null);
            return new ProjectRow(p.Id, p.ProjectCode, p.Name, slots);
        }).ToList();

        return Page();
    }

    // ── POST: Mark No Work ────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostMarkNoWorkAsync(int projectId, string weekStart)
    {
        if (!DateOnly.TryParse(weekStart, out var weekDate))
        {
            TempData["Error"] = "Invalid week date.";
            return RedirectToPage(new { from = From, to = To });
        }

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        // 비 Safety 스태프는 담당 프로젝트 + 상태 체크
        if (!IsSafetyStaff())
        {
            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(projectId))
                return Forbid();

            var existing = await _svc.GetReportByWeekAsync(projectId, weekDate);
            if (existing != null &&
                (existing.Status == SafetyWkRepStatus.Reviewed ||
                 existing.Status == SafetyWkRepStatus.Approved))
            {
                TempData["Error"] = "Cannot change a reviewed or approved report.";
                return RedirectToPage(new { from = From, to = To });
            }
        }

        try
        {
            await _svc.MarkNoWorkAsync(projectId, weekDate, userId, userName);
            TempData["Success"] = "Week marked as No Work.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { from = From, to = To });
    }

    // ── POST: Delete report ───────────────────────────────────────────────────
    public async Task<IActionResult> OnPostDeleteAsync(int reportId)
    {
        var (userId, _) = GetUser();
        if (userId <= 0) return Forbid();

        // 삭제할 보고서 조회
        var report = await _db.SafetyWkReps.FindAsync(reportId);
        if (report == null) return NotFound();

        // 비 Safety 스태프는 담당 프로젝트 + 상태 체크
        if (!IsSafetyStaff())
        {
            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(report.ProjectId))
                return Forbid();

            if (report.Status == SafetyWkRepStatus.Reviewed ||
                report.Status == SafetyWkRepStatus.Approved)
            {
                TempData["Error"] = "Cannot delete a reviewed or approved report.";
                return RedirectToPage(new { from = From, to = To });
            }
        }

        try
        {
            var (deleted, storedNames) = await _svc.DeleteReportAsync(reportId);
            var halfYear = deleted.WeekStartDate.Month <= 6 ? "A" : "B";
            var uploadDir = Path.Combine(
                _storage.FileItemRoot, "SafetyMgmt", "SafetyWkRepFiles",
                $"{deleted.WeekStartDate.Year}{halfYear}");
            foreach (var storedName in storedNames)
            {
                var filePath = Path.Combine(uploadDir, storedName);
                if (System.IO.File.Exists(filePath))
                    try { System.IO.File.Delete(filePath); }
                    catch (Exception ex) { _logger.LogWarning(ex, "Could not delete safety file {Path}", filePath); }
            }
            TempData["Success"] = "Report deleted.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { from = From, to = To });
    }

    // ── POST: Review (PM + SafetyManager+) ───────────────────────────────────
    public async Task<IActionResult> OnPostReviewAsync(int reportId, string? notes)
    {
        // PM은 담당 프로젝트만, SafetyManager+는 전체
        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        if (!IsSafetyManager())
        {
            // PM만 허용
            if (!IsProjectManager())
                return Forbid();
            var report   = await _db.SafetyWkReps.FindAsync(reportId);
            if (report == null) return NotFound();
            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(report.ProjectId))
                return Forbid();
        }

        try
        {
            await _svc.ReviewReportAsync(reportId, notes, userId, userName);
            TempData["Success"] = "Report marked as Reviewed.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { from = From, to = To });
    }

    // ── POST: Unreview (SafetyManager+ 전용) ─────────────────────────────────
    public async Task<IActionResult> OnPostUnreviewAsync(int reportId)
    {
        if (!IsSafetyManager()) return Forbid();

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        try
        {
            await _svc.UnreviewReportAsync(reportId, userId, userName);
            TempData["Success"] = "Review status cleared.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { from = From, to = To });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private (int userId, string userName) GetUser()
    {
        var idStr = User.FindFirst("UserId")?.Value
                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id) || id <= 0) return (0, "");
        return (id, User.Identity?.Name ?? "Unknown");
    }

    private bool IsSafetyStaff() =>
        User.IsInRole(PrivilegeCodes.Admin)
        || User.IsInRole(PrivilegeCodes.SafetyAdmin)
        || User.IsInRole(PrivilegeCodes.SafetyManager)
        || User.IsInRole(PrivilegeCodes.SafetyUser);

    private bool IsSafetyManager() =>
        User.IsInRole(PrivilegeCodes.Admin)
        || User.IsInRole(PrivilegeCodes.SafetyAdmin)
        || User.IsInRole(PrivilegeCodes.SafetyManager);

    private bool IsProjectManager() =>
        User.IsInRole(PrivilegeCodes.ProjectManager);

    private static DateOnly? ParseDate(string? s) =>
        DateOnly.TryParse(s, out var d) ? d : null;

    private static DateOnly GetWeekMonday(DateOnly date)
    {
        int diff = (7 + ((int)date.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }
}
