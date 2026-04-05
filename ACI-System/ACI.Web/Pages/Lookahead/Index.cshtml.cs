using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using LookaheadEntity = ACI.Web.Data.Entities.Lookahead;

namespace ACI.Web.Pages.Lookahead;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext      _db;
    private readonly ILookaheadService _lookaheadSvc;

    public IndexModel(AppDbContext db, ILookaheadService svc)
    {
        _db           = db;
        _lookaheadSvc = svc;
    }

    [BindProperty(SupportsGet = true)] public int      ProjectId    { get; set; }
    [BindProperty(SupportsGet = true)] public int      Weeks        { get; set; } = 3;
    [BindProperty(SupportsGet = true)] public int?     LookaheadId  { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly BaseDate     { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public string           ProjectName        { get; set; } = string.Empty;
    public int              CurrentLookaheadId { get; set; }
    public DateOnly         PeriodStart        { get; set; }
    public List<Trade>      Trades             { get; set; } = [];
    public List<TradeTaskGroup> TasksByTrade   { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var project = await _db.Projects.FindAsync(ProjectId);
        if (project == null) return NotFound();
        ProjectName = project.Name;

        Trades = await _db.Trades
            .Where(t => t.ProjectId == ProjectId && t.IsActive)
            .OrderBy(t => t.Code)
            .ToListAsync();

        PeriodStart = GetMonday(BaseDate);
        var periodEnd = PeriodStart.AddDays(Weeks * 7);

        // 선택한 기간을 포함하는 Lookahead 찾기 (특정 ID 지정 or 날짜 기준)
        LookaheadEntity? lookahead;
        if (LookaheadId.HasValue)
        {
            lookahead = await _db.Lookaheads
                .Include(l => l.Tasks).ThenInclude(t => t.Trade)
                .Include(l => l.Tasks).ThenInclude(t => t.AssignedTo)
                .FirstOrDefaultAsync(l => l.Id == LookaheadId && l.IsActive);
        }
        else
        {
            // 선택한 주(PeriodStart)를 포함하는 Lookahead 검색
            lookahead = await _db.Lookaheads
                .Include(l => l.Tasks).ThenInclude(t => t.Trade)
                .Include(l => l.Tasks).ThenInclude(t => t.AssignedTo)
                .Where(l => l.ProjectId == ProjectId && l.IsActive
                         && l.StartDate <= PeriodStart && l.EndDate >= PeriodStart)
                .OrderByDescending(l => l.StartDate)
                .FirstOrDefaultAsync();
        }

        if (lookahead == null)
        {
            // 해당 기간 Lookahead 없음 → 새로 생성 후 Baseline Schedule 자동 pull
            var weekNum = System.Globalization.ISOWeek.GetWeekOfYear(PeriodStart.ToDateTime(TimeOnly.MinValue));
            lookahead = await _lookaheadSvc.CreateLookaheadAsync(
                ProjectId, PeriodStart, Weeks,
                $"Lookahead W{weekNum}-{PeriodStart.Year}");

            await _lookaheadSvc.PullFromScheduleAsync(lookahead.Id, ProjectId, PeriodStart, periodEnd);

            // task 가 추가된 상태로 다시 로드
            lookahead = await _db.Lookaheads
                .Include(l => l.Tasks).ThenInclude(t => t.Trade)
                .Include(l => l.Tasks).ThenInclude(t => t.AssignedTo)
                .FirstAsync(l => l.Id == lookahead.Id);
        }

        CurrentLookaheadId = lookahead.Id;
        // PeriodStart 는 사용자가 선택한 날짜 기준 유지 (lookahead.StartDate 로 덮어쓰지 않음)

        TasksByTrade = lookahead.Tasks
            .Where(t => t.StartDate <= periodEnd && t.EndDate >= PeriodStart)
            .GroupBy(t => t.Trade)
            .Select(g =>
            {
                var trade = g.Key;
                var color = trade?.Color ?? "#6c757d";
                return new TradeTaskGroup
                {
                    TradeId      = trade?.Id ?? 0,
                    TradeName    = trade?.Name ?? "Unassigned",
                    TradeColor   = color,
                    TradeColor20 = color + "22",
                    Tasks        = g.Select(t => new LookaheadTaskVm
                    {
                        Id            = t.Id,
                        Text          = t.Text,
                        StartDate     = t.StartDate,
                        EndDate       = t.EndDate,
                        Location      = t.Location,
                        AssigneeName  = t.AssignedTo?.DisplayName,
                        CrewSize      = t.CrewSize,
                        HasConstraint = t.HasConstraint,
                        Status        = t.Status
                    }).ToList()
                };
            })
            .OrderBy(g => g.TradeName)
            .ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostSyncScheduleAsync(int lookaheadId, DateOnly periodStart, int weeks)
    {
        var periodEnd = periodStart.AddDays(weeks * 7);
        var pulled = await _lookaheadSvc.PullFromScheduleAsync(lookaheadId, ProjectId, periodStart, periodEnd);
        TempData[pulled > 0 ? "Success" : "Info"] =
            pulled > 0 ? $"{pulled} task(s) pulled from Baseline Schedule." : "No new tasks to pull.";
        return RedirectToPage(new { projectId = ProjectId, baseDate = periodStart.ToString("yyyy-MM-dd"), weeks });
    }

    public async Task<IActionResult> OnPostAddTaskAsync(
        int lookaheadId, string text, DateOnly startDate, int duration,
        int? tradeId, int crewSize, string? location, bool hasConstraint)
    {
        var task = new LookaheadTask
        {
            LookaheadId   = lookaheadId,
            Text          = text,
            StartDate     = startDate,
            EndDate       = startDate.AddDays(duration),
            Duration      = duration,
            TradeId       = tradeId,
            CrewSize      = crewSize,
            Location      = location,
            HasConstraint = hasConstraint,
            CreatedAt     = DateTime.UtcNow,
            UpdatedAt     = DateTime.UtcNow
        };
        await _lookaheadSvc.CreateTaskAsync(task);
        TempData["Success"] = $"Task '{text}' added.";
        return RedirectToPage(new { projectId = ProjectId, baseDate = BaseDate.ToString("yyyy-MM-dd") });
    }

    private static DateOnly GetMonday(DateOnly date)
    {
        int diff = (7 + ((int)date.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }
}

public class TradeTaskGroup
{
    public int    TradeId      { get; set; }
    public string TradeName    { get; set; } = string.Empty;
    public string TradeColor   { get; set; } = "#6c757d";
    public string TradeColor20 { get; set; } = "#6c757d22";
    public List<LookaheadTaskVm> Tasks { get; set; } = [];
}

public class LookaheadTaskVm
{
    public int    Id            { get; set; }
    public string Text          { get; set; } = string.Empty;
    public DateOnly StartDate   { get; set; }
    public DateOnly EndDate     { get; set; }
    public string? Location     { get; set; }
    public string? AssigneeName { get; set; }
    public int    CrewSize      { get; set; }
    public bool   HasConstraint { get; set; }
    public LookaheadTaskStatus Status { get; set; }
}
