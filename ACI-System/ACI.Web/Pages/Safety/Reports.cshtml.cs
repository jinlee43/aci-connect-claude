using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
    private readonly ILogger<ReportsModel> _logger;

    public ReportsModel(ISafetyWkRepService svc, ILogger<ReportsModel> logger)
    {
        _svc   = svc;
        _logger = logger;
    }

    // ── Filter ────────────────────────────────────────────────────────────────
    [BindProperty(SupportsGet = true)] public string? From { get; set; }
    [BindProperty(SupportsGet = true)] public string? To   { get; set; }

    // ── View data ─────────────────────────────────────────────────────────────
    public List<DateOnly>               Weeks      { get; set; } = [];
    public List<SafetyWkRepGridRowDto>  Rows       { get; set; } = [];
    public bool CanApprove                         { get; set; }

    // ── GET ───────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync()
    {
        CanApprove = User.IsInRole(PrivilegeCodes.Admin)
                  || User.IsInRole(PrivilegeCodes.SafetyAdmin)
                  || User.IsInRole(PrivilegeCodes.SafetyManager);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var from  = ParseDate(From) ?? GetWeekMonday(today).AddDays(-7 * 11);
        var to    = ParseDate(To)   ?? today;

        From = from.ToString("yyyy-MM-dd");
        To   = to.ToString("yyyy-MM-dd");

        var cur = GetWeekMonday(from);
        while (cur <= to) { Weeks.Add(cur); cur = cur.AddDays(7); }

        var (userId, _) = GetUser();
        bool isAdmin = User.IsInRole(PrivilegeCodes.Admin);
        Rows = await _svc.GetWeeklyGridAsync(from, to, userId, isAdmin);
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
