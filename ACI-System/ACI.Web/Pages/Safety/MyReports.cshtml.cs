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
/// Flat list view: (Project × Week) 행 목록, 컬럼 정렬 지원.
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
    public List<FlatRow> Rows     { get; set; } = [];
    public bool CanReview         { get; set; }
    public bool CanUnreview       { get; set; }

    /// <summary>UserId → Employee.DisplayName (없으면 User.Name 폴백)</summary>
    public Dictionary<int, string> UserDisplayNames { get; set; } = [];

    /// <summary>
    /// (Project × Week) 단일 행.
    /// Report == null 이면 아직 제출하지 않은 주.
    /// </summary>
    public record FlatRow(
        int          ProjectId,
        string       ProjectCode,
        string       ProjectName,
        int          WeekNumber,
        int          Year,
        DateOnly     WeekStartDate,
        DateOnly     DueDate,       // Settings.DefaultSubmitDay 기준 계산
        SafetyWkRep? Report);       // null = 미제출

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

        var projectIds = await _svc.GetAssignedProjectIdsAsync(userId);
        if (projectIds.Count == 0) return Page();

        var projects = await _db.Projects
            .Where(p => p.IsActive && projectIds.Contains(p.Id))
            .OrderBy(p => p.ProjectCode)
            .ToListAsync();

        // 기간 내 기존 보고서 — Files 포함 로드
        var fromMonday = GetWeekMonday(from);
        var reports = await _db.SafetyWkReps
            .Include(r => r.Files)
            .Where(r => r.IsActive
                     && projectIds.Contains(r.ProjectId)
                     && r.WeekStartDate >= fromMonday
                     && r.WeekStartDate <= to)
            .ToListAsync();

        // 프로젝트별 Settings (DefaultSubmitDay)
        var settings = await _db.SafetyWkRepSettings
            .Where(s => projectIds.Contains(s.ProjectId))
            .ToListAsync();
        var submitDayMap = settings.ToDictionary(
            s => s.ProjectId,
            s => s.DefaultSubmitDay);

        // 보고서 인덱스: (ProjectId, WeekStartDate) → SafetyWkRep
        var repIndex = reports.ToDictionary(r => (r.ProjectId, r.WeekStartDate));

        // 주 목록 생성
        var weeks = new List<DateOnly>();
        var cur = fromMonday;
        while (cur <= to) { weeks.Add(cur); cur = cur.AddDays(7); }

        // Flat rows 생성
        var rows = new List<FlatRow>();
        foreach (var p in projects)
        {
            var submitDay = submitDayMap.GetValueOrDefault(p.Id, DayOfWeek.Friday);
            foreach (var week in weeks)
            {
                var dueDate = CalcDueDate(week, submitDay);
                repIndex.TryGetValue((p.Id, week), out var rep);
                rows.Add(new FlatRow(
                    p.Id, p.ProjectCode, p.Name,
                    System.Globalization.ISOWeek.GetWeekOfYear(week.ToDateTime(TimeOnly.MinValue)),
                    week.Year,
                    week, dueDate, rep));
            }
        }

        // 기본 정렬: Due Date 내림차순 → Project Code
        Rows = rows
            .OrderByDescending(r => r.DueDate)
            .ThenBy(r => r.ProjectCode)
            .ToList();

        // Submitter 표시명: UploadedById → Employee.DisplayName (없으면 User.Name)
        var uploaderIds = reports
            .Where(r => r.UploadedById.HasValue)
            .Select(r => r.UploadedById!.Value)
            .Distinct()
            .ToList();
        if (uploaderIds.Count > 0)
        {
            UserDisplayNames = await _db.Users
                .Where(u => uploaderIds.Contains(u.Id))
                .Include(u => u.Employee)
                .ToDictionaryAsync(
                    u => u.Id,
                    u => u.Employee != null ? u.Employee.DisplayName : u.Name);
        }

        return Page();
    }

    // ── POST: Mark No Work ────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostMarkNoWorkAsync(int projectId, string weekStart, string? notes)
    {
        if (!DateOnly.TryParse(weekStart, out var weekDate))
        {
            TempData["Error"] = "Invalid week date.";
            return RedirectToPage(new { from = From, to = To });
        }

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        if (!IsSafetyStaff())
        {
            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(projectId)) return Forbid();

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
            await _svc.MarkNoWorkAsync(projectId, weekDate, userId, userName, notes: notes);
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

        var report = await _db.SafetyWkReps.FindAsync(reportId);
        if (report == null) return NotFound();

        if (!IsSafetyStaff())
        {
            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(report.ProjectId)) return Forbid();

            if (report.Status is SafetyWkRepStatus.Draft
                              or SafetyWkRepStatus.Reviewed
                              or SafetyWkRepStatus.Approved)
            {
                TempData["Error"] = "Cannot delete a submitted, reviewed, or approved report.";
                return RedirectToPage(new { from = From, to = To });
            }
        }

        try
        {
            var (deleted, storedNames) = await _svc.DeleteReportAsync(reportId);
            var halfYear  = deleted.WeekStartDate.Month <= 6 ? "A" : "B";
            var uploadDir = Path.Combine(
                _storage.FileItemRoot, "SafetyMgmt", "SafetyWkRepFiles",
                $"{deleted.WeekStartDate.Year}{halfYear}");
            foreach (var name in storedNames)
            {
                var path = Path.Combine(uploadDir, name);
                if (System.IO.File.Exists(path))
                    try { System.IO.File.Delete(path); }
                    catch (Exception ex) { _logger.LogWarning(ex, "Could not delete safety file {Path}", path); }
            }
            TempData["Success"] = "Report deleted.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { from = From, to = To });
    }

    // ── POST: Review ──────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostReviewAsync(int reportId, string? notes)
    {
        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        if (!IsSafetyManager())
        {
            if (!IsProjectManager()) return Forbid();
            var report   = await _db.SafetyWkReps.FindAsync(reportId);
            if (report == null) return NotFound();
            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(report.ProjectId)) return Forbid();
        }

        try
        {
            await _svc.ReviewReportAsync(reportId, notes, userId, userName);
            TempData["Success"] = "Report marked as Reviewed.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { from = From, to = To });
    }

    // ── POST: Unreview ────────────────────────────────────────────────────────
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

    /// <summary>WeekStartDate(월요일)에서 submitDay 요일까지의 날짜 계산.</summary>
    private static DateOnly CalcDueDate(DateOnly monday, DayOfWeek submitDay)
    {
        int offset = ((int)submitDay - (int)DayOfWeek.Monday + 7) % 7;
        return monday.AddDays(offset);
    }
}
