using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Safety;

/// <summary>
/// Safety staff all-projects weekly grid view.
/// SafetyUser+ can view. SafetyManager/SafetyAdmin can approve.
/// Per-project review rights live in each row's CanReviewThisRow (PM/SPM).
/// </summary>
[Authorize(Policy = "SafetyUser")]
public class ReportsModel : PageModel
{
    private readonly ISafetyWkRepService _svc;
    private readonly AppDbContext        _db;
    private readonly ILogger<ReportsModel> _logger;

    public ReportsModel(ISafetyWkRepService svc, AppDbContext db, ILogger<ReportsModel> logger)
    {
        _svc    = svc;
        _db     = db;
        _logger = logger;
    }

    // ── Filter ────────────────────────────────────────────────────────────────
    [BindProperty(SupportsGet = true)] public string? From { get; set; }
    [BindProperty(SupportsGet = true)] public string? To   { get; set; }

    // ── View data ─────────────────────────────────────────────────────────────
    public List<DateOnly>               Weeks        { get; set; } = [];
    public List<SafetyWkRepGridRowDto>  Rows         { get; set; } = [];
    public bool                         CanApprove   { get; set; }


    // ── GET ───────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync()
    {
        CanApprove = User.IsInRole(PrivilegeCodes.Admin)
                  || User.IsInRole(PrivilegeCodes.SafetyAdmin)
                  || User.IsInRole(PrivilegeCodes.SafetyManager);

        var today    = DateOnly.FromDateTime(DateTime.Today);
        var thisWeek = GetWeekMonday(today);
        var lastWeek = thisWeek.AddDays(-7);

        var from = ParseDate(From) ?? thisWeek.AddDays(-7 * 11);
        var to   = ParseDate(To)   ?? today;

        From = from.ToString("yyyy-MM-dd");
        To   = to.ToString("yyyy-MM-dd");

        var cur = GetWeekMonday(from);
        while (cur <= to) { Weeks.Add(cur); cur = cur.AddDays(7); }

        var (userId, _) = GetUser();
        bool isAdmin = User.IsInRole(PrivilegeCodes.Admin);
        Rows = await _svc.GetWeeklyGridAsync(from, to, userId, isAdmin);

        // ── 전체 누적 집계 ──────────────────────────────────────────────────
        // Pending: Approved/NoWorkApproved/Voided 제외한 모든 활성 보고서
        ViewData["TotalPending"] = await _db.SafetyWkReps
            .CountAsync(r => r.IsActive
                          && r.Status != SafetyWkRepStatus.Approved
                          && r.Status != SafetyWkRepStatus.NoWorkApproved
                          && r.Status != SafetyWkRepStatus.Voided);

        // Missing: 각 프로젝트 시작일~지난주까지 기대되는 주 중 보고서 없는 주
        var settings = await _db.SafetyWkRepSettings
            .Where(s => s.IsActive)
            .ToListAsync();

        var existingDates = await _db.SafetyWkReps
            .Where(r => r.IsActive)
            .Select(r => new { r.ProjectId, r.WeekStartDate })
            .ToListAsync();

        var reportSet = existingDates
            .Select(r => (r.ProjectId, r.WeekStartDate))
            .ToHashSet();

        int totalMissing = 0;
        foreach (var s in settings)
        {
            var end = s.EndDate.HasValue && s.EndDate.Value < lastWeek
                ? s.EndDate.Value
                : lastWeek;
            // StartDate는 첫 제출 요일 날짜 → 해당 주 월요일부터 카운트
            var week = GetWeekMonday(s.StartDate);
            while (week <= end)
            {
                if (!reportSet.Contains((s.ProjectId, week)))
                    totalMissing++;
                week = week.AddDays(7);
            }
        }
        ViewData["TotalMissing"] = totalMissing;

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

    private static DateOnly? ParseDate(string? s) =>
        DateOnly.TryParse(s, out var d) ? d : null;

    private static DateOnly GetWeekMonday(DateOnly date)
    {
        int diff = (7 + ((int)date.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }
}
