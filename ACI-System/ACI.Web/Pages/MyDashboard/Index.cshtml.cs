using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace ACI.Web.Pages.MyDashboard;

/// <summary>
/// 현장 참여자(PM / Superintendent) 전용 대시보드.
/// 담당 프로젝트의 안전 보고서 현황 및 향후 RFI/C.O./FWD/Daily 카운트 카드.
/// </summary>
[Authorize]
public class IndexModel : PageModel
{
    private readonly ISafetyWkRepService _svc;
    private readonly AppDbContext        _db;

    public IndexModel(ISafetyWkRepService svc, AppDbContext db)
    {
        _svc = svc;
        _db  = db;
    }

    // ── Summary card counts ───────────────────────────────────────────────────
    public int ActiveProjectCount { get; set; }
    public int SafetyPendingCount { get; set; }
    public int SafetyMissingCount { get; set; }

    // Placeholder counts (features not yet implemented)
    public int RfiCount   { get; set; }
    public int CoCount    { get; set; }
    public int FwdCount   { get; set; }
    public int DailyCount { get; set; }

    // ── Detail rows ───────────────────────────────────────────────────────────
    public List<SafetyDocRow> SafetyRows { get; set; } = [];

    // ── GET ───────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync()
    {
        var (userId, _) = GetUser();
        if (userId <= 0) return Forbid();

        var assignedIds = await _svc.GetAssignedProjectIdsAsync(userId);

        if (assignedIds.Count == 0)
            return Page();   // no assignments — show zeros

        // ── Active assigned projects ──────────────────────────────────────────
        var projects = await _db.Projects
            .Where(p => p.IsActive && assignedIds.Contains(p.Id))
            .OrderBy(p => p.ProjectCode)
            .ToListAsync();

        ActiveProjectCount = projects.Count;
        var projectDict = projects.ToDictionary(p => p.Id);

        // ── Safety: pending reports ───────────────────────────────────────────
        var pendingReports = await _db.SafetyWkReps
            .Include(r => r.Project)
            .Where(r => r.IsActive
                     && assignedIds.Contains(r.ProjectId)
                     && r.Status != SafetyWkRepStatus.Approved
                     && r.Status != SafetyWkRepStatus.NoWorkApproved
                     && r.Status != SafetyWkRepStatus.Voided)
            .OrderByDescending(r => r.WeekStartDate)
            .ThenBy(r => r.Project.ProjectCode)
            .ToListAsync();

        SafetyPendingCount = pendingReports.Count;

        // ── Safety: missing reports (past weeks) ──────────────────────────────
        var today    = DateOnly.FromDateTime(DateTime.Today);
        var thisWeek = GetWeekMonday(today);
        var lastWeek = thisWeek.AddDays(-7);

        var settings = await _db.SafetyWkRepSettings
            .Where(s => s.IsActive && assignedIds.Contains(s.ProjectId))
            .ToListAsync();

        var existingDates = await _db.SafetyWkReps
            .Where(r => r.IsActive && assignedIds.Contains(r.ProjectId))
            .Select(r => new { r.ProjectId, r.WeekStartDate })
            .ToListAsync();

        var reportSet = existingDates
            .Select(r => (r.ProjectId, r.WeekStartDate))
            .ToHashSet();

        // 최근 8주까지만 테이블에 표시 (총 누적 Missing 건수는 전체 집계)
        var recentCutoff = lastWeek.AddDays(-7 * 7);
        var missingRows  = new List<SafetyDocRow>();

        foreach (var s in settings)
        {
            var end  = s.EndDate.HasValue && s.EndDate.Value < lastWeek
                       ? s.EndDate.Value
                       : lastWeek;
            var week = GetWeekMonday(s.StartDate);

            while (week <= end)
            {
                if (!reportSet.Contains((s.ProjectId, week)))
                {
                    SafetyMissingCount++;

                    // 테이블은 최근 8주만
                    if (week >= recentCutoff && projectDict.TryGetValue(s.ProjectId, out var proj))
                    {
                        missingRows.Add(new SafetyDocRow(
                            ProjectId:   s.ProjectId,
                            ProjectCode: proj.ProjectCode,
                            ProjectName: proj.Name,
                            DocType:     "Weekly Safety Report",
                            StatusLabel: "Missing",
                            StatusClass: "danger",
                            WeekStart:   week,
                            WeekNumber:  ISOWeek.GetWeekOfYear(week.ToDateTime(TimeOnly.MinValue)),
                            Year:        week.Year,
                            ReportId:    null
                        ));
                    }
                }
                week = week.AddDays(7);
            }
        }

        // ── Build final row list: pending first, then missing ─────────────────
        var pendingRows = pendingReports.Select(r => new SafetyDocRow(
            ProjectId:   r.ProjectId,
            ProjectCode: r.Project.ProjectCode,
            ProjectName: r.Project.Name,
            DocType:     "Weekly Safety Report",
            StatusLabel: StatusToLabel(r.Status),
            StatusClass: StatusToCss(r.Status),
            WeekStart:   r.WeekStartDate,
            WeekNumber:  r.WeekNumber,
            Year:        r.Year,
            ReportId:    r.Id
        ));

        SafetyRows = pendingRows
            .Concat(missingRows)
            .OrderByDescending(r => r.WeekStart)
            .ThenBy(r => r.ProjectCode)
            .ToList();

        return Page();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private (int userId, string userName) GetUser()
    {
        var idStr = User.FindFirst("UserId")?.Value
                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id) || id <= 0) return (0, "");
        return (id, User.Identity?.Name ?? "Unknown");
    }

    private static DateOnly GetWeekMonday(DateOnly date)
    {
        int diff = (7 + ((int)date.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }

    private static string StatusToLabel(SafetyWkRepStatus s) => s switch
    {
        SafetyWkRepStatus.Staged         => "Staged",
        SafetyWkRepStatus.Draft          => "Submitted",
        SafetyWkRepStatus.Reviewed       => "Reviewed",
        SafetyWkRepStatus.Approved       => "Approved",
        SafetyWkRepStatus.NoWork         => "No Work",
        SafetyWkRepStatus.NoWorkReviewed => "No Work (Reviewed)",
        SafetyWkRepStatus.NoWorkApproved => "No Work (Approved)",
        SafetyWkRepStatus.Voided         => "Voided",
        _                                => s.ToString()
    };

    private static string StatusToCss(SafetyWkRepStatus s) => s switch
    {
        SafetyWkRepStatus.Staged         => "secondary",
        SafetyWkRepStatus.Draft          => "primary",
        SafetyWkRepStatus.Reviewed       => "info",
        SafetyWkRepStatus.Approved       => "success",
        SafetyWkRepStatus.NoWork         => "primary",
        SafetyWkRepStatus.NoWorkReviewed => "info",
        SafetyWkRepStatus.NoWorkApproved => "success",
        SafetyWkRepStatus.Voided         => "warning",
        _                                => "secondary"
    };
}

// ── DTO ───────────────────────────────────────────────────────────────────────
public record SafetyDocRow(
    int      ProjectId,
    string   ProjectCode,
    string   ProjectName,
    string   DocType,
    string   StatusLabel,
    string   StatusClass,
    DateOnly WeekStart,
    int      WeekNumber,
    int      Year,
    int?     ReportId
);
