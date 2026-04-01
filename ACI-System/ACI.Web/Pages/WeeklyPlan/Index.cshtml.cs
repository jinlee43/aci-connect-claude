using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.WeeklyPlan;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext      _db;
    private readonly IWeeklyPlanService _planSvc;

    public IndexModel(AppDbContext db, IWeeklyPlanService planSvc)
    {
        _db      = db;
        _planSvc = planSvc;
    }

    [BindProperty(SupportsGet = true)] public int  ProjectId { get; set; }
    [BindProperty(SupportsGet = true)] public int? PlanId    { get; set; }

    public string           ProjectName  { get; set; } = string.Empty;
    public WeeklyWorkPlan?  Plan         { get; set; }
    public List<WeeklyTaskVm> Tasks      { get; set; } = [];
    public List<Trade>      Trades       { get; set; } = [];
    public List<WeeklyPlanHistoryVm> RecentPlans { get; set; } = [];
    public List<VarianceStat> VarianceStats { get; set; } = [];
    public int    CommittedCount { get; set; }
    public int    CompletedCount { get; set; }
    public int    TotalTaskCount { get; set; }
    public double PPC            { get; set; }
    public int?   PrevPlanId     { get; set; }
    public int?   NextPlanId     { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var project = await _db.Projects.FindAsync(ProjectId);
        if (project == null) return NotFound();
        ProjectName = project.Name;

        Trades = await _db.Trades
            .Where(t => t.ProjectId == ProjectId && t.IsActive)
            .OrderBy(t => t.Code)
            .ToListAsync();

        Plan = PlanId.HasValue
            ? await _db.WeeklyWorkPlans
                .Include(w => w.Tasks).ThenInclude(t => t.Trade)
                .Include(w => w.Tasks).ThenInclude(t => t.AssignedTo)
                .FirstOrDefaultAsync(w => w.Id == PlanId)
            : await _planSvc.GetOrCreateCurrentWeekPlanAsync(ProjectId);

        if (Plan == null) return Page();

        Tasks = Plan.Tasks.Select(t => new WeeklyTaskVm
        {
            Id               = t.Id,
            Text             = t.Text,
            PlannedDate      = t.PlannedDate,
            AssigneeName     = t.AssignedTo?.DisplayName,
            TradeColor       = t.Trade?.Color,
            IsCommitted      = t.IsCommitted,
            IsCompleted      = t.IsCompleted,
            VarianceCategory = t.VarianceCategory,
            VarianceNote     = t.VarianceNote
        }).ToList();

        CommittedCount = Tasks.Count(t => t.IsCommitted);
        CompletedCount = Tasks.Count(t => t.IsCompleted);
        TotalTaskCount = Tasks.Count;
        PPC = CommittedCount > 0 ? Math.Round((double)CompletedCount / CommittedCount * 100, 1) : 0;

        // PPC history (last 8 weeks)
        var allPlans = await _db.WeeklyWorkPlans
            .Where(w => w.ProjectId == ProjectId && w.IsActive)
            .OrderByDescending(w => w.WeekStartDate)
            .Take(8)
            .ToListAsync();

        RecentPlans = allPlans.Select(p => new WeeklyPlanHistoryVm
        {
            Id         = p.Id,
            WeekNumber = p.WeekNumber,
            PPC        = p.PPC
        }).ToList();

        var allIds = allPlans.Select(p => p.Id).OrderBy(x => x).ToList();
        var idx    = allIds.IndexOf(Plan.Id);
        PrevPlanId = idx > 0 ? allIds[idx - 1] : null;
        NextPlanId = idx < allIds.Count - 1 ? allIds[idx + 1] : null;

        if (Plan.Status == WeeklyPlanStatus.Closed)
        {
            var stats = await _planSvc.CalculatePpcAsync(Plan.Id);
            VarianceStats = stats.Variances;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddTaskAsync(
        int planId, string text, DateOnly plannedDate,
        int? tradeId, int? assignedToId, int crewSize)
    {
        var task = new WeeklyTask
        {
            WeeklyWorkPlanId = planId,
            Text             = text,
            PlannedDate      = plannedDate,
            TradeId          = tradeId,
            AssignedToId     = assignedToId,
            CrewSize         = crewSize,
            CreatedAt        = DateTime.UtcNow,
            UpdatedAt        = DateTime.UtcNow
        };
        _db.WeeklyTasks.Add(task);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Task added.";
        return RedirectToPage(new { projectId = ProjectId, planId });
    }

    public async Task<IActionResult> OnPostCommitAsync(int taskId)
    {
        await _planSvc.CommitTaskAsync(taskId);
        return RedirectToPage(new { projectId = ProjectId, planId = PlanId });
    }

    public async Task<IActionResult> OnPostToggleCompleteAsync(int taskId, bool completed)
    {
        await _planSvc.CompleteTaskAsync(taskId, completed, null, null);
        return RedirectToPage(new { projectId = ProjectId, planId = PlanId });
    }
}

public class WeeklyTaskVm
{
    public int      Id               { get; set; }
    public string   Text             { get; set; } = string.Empty;
    public DateOnly PlannedDate      { get; set; }
    public string?  AssigneeName     { get; set; }
    public string?  TradeColor       { get; set; }
    public bool     IsCommitted      { get; set; }
    public bool     IsCompleted      { get; set; }
    public VarianceCategory? VarianceCategory { get; set; }
    public string?  VarianceNote     { get; set; }
}

public class WeeklyPlanHistoryVm
{
    public int    Id         { get; set; }
    public int    WeekNumber { get; set; }
    public double PPC        { get; set; }
}
