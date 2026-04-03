using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Services;

public interface ILookaheadService
{
    Task<List<Lookahead>> GetProjectLookaheadsAsync(int projectId);
    Task<Lookahead?> GetLookaheadAsync(int id);
    Task<Lookahead> CreateLookaheadAsync(int projectId, DateOnly startDate, int weeks, string name);
    Task<LookaheadTask> CreateTaskAsync(LookaheadTask task);
    Task<LookaheadTask> UpdateTaskAsync(LookaheadTask task);
    Task DeleteTaskAsync(int taskId);
    Task<List<ScheduleTask>> GetScheduleTasksForPeriodAsync(int projectId, DateOnly start, DateOnly end);
    /// <summary>Master Schedule 에서 해당 기간의 task 를 Lookahead 로 가져온다. 이미 연결된 task 는 중복 추가하지 않음.</summary>
    Task<int> PullFromScheduleAsync(int lookaheadId, int projectId, DateOnly start, DateOnly end);
}

public interface IWeeklyPlanService
{
    Task<List<WeeklyWorkPlan>> GetProjectPlansAsync(int projectId);
    Task<WeeklyWorkPlan?> GetPlanAsync(int id);
    Task<WeeklyWorkPlan> GetOrCreateCurrentWeekPlanAsync(int projectId);
    Task<WeeklyTask> CommitTaskAsync(int taskId);
    Task<WeeklyTask> CompleteTaskAsync(int taskId, bool completed, VarianceCategory? variance, string? note);
    Task<WeeklyPpcStats> CalculatePpcAsync(int planId);
}

public class LookaheadService : ILookaheadService
{
    private readonly AppDbContext _db;
    public LookaheadService(AppDbContext db) => _db = db;

    public async Task<List<Lookahead>> GetProjectLookaheadsAsync(int projectId) =>
        await _db.Lookaheads
            .Where(l => l.ProjectId == projectId && l.IsActive)
            .OrderByDescending(l => l.StartDate)
            .ToListAsync();

    public async Task<Lookahead?> GetLookaheadAsync(int id) =>
        await _db.Lookaheads
            .Include(l => l.Tasks).ThenInclude(t => t.Trade)
            .Include(l => l.Tasks).ThenInclude(t => t.AssignedTo)
            .FirstOrDefaultAsync(l => l.Id == id && l.IsActive);

    public async Task<Lookahead> CreateLookaheadAsync(int projectId, DateOnly startDate, int weeks, string name)
    {
        var lookahead = new Lookahead
        {
            ProjectId  = projectId,
            Name       = name,
            StartDate  = startDate,
            EndDate    = startDate.AddDays(weeks * 7),
            WeeksCount = weeks,
            CreatedAt  = DateTime.UtcNow
        };
        _db.Lookaheads.Add(lookahead);
        await _db.SaveChangesAsync();
        return lookahead;
    }

    public async Task<LookaheadTask> CreateTaskAsync(LookaheadTask task)
    {
        _db.LookaheadTasks.Add(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task<LookaheadTask> UpdateTaskAsync(LookaheadTask task)
    {
        task.UpdatedAt = DateTime.UtcNow;
        _db.LookaheadTasks.Update(task);
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task DeleteTaskAsync(int taskId)
    {
        var task = await _db.LookaheadTasks.FindAsync(taskId);
        if (task != null)
        {
            _db.LookaheadTasks.Remove(task);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<ScheduleTask>> GetScheduleTasksForPeriodAsync(
        int projectId, DateOnly start, DateOnly end) =>
        await _db.ScheduleTasks
            .Where(t => t.ProjectId == projectId
                && t.IsActive
                && t.StartDate <= end
                && t.EndDate   >= start
                && t.TaskType  != GanttTaskType.Project)
            .Include(t => t.Trade)
            .OrderBy(t => t.StartDate)
            .ToListAsync();

    public async Task<int> PullFromScheduleAsync(int lookaheadId, int projectId, DateOnly start, DateOnly end)
    {
        // 이미 이 Lookahead 에 연결된 ScheduleTaskId 목록
        var existingIds = await _db.LookaheadTasks
            .Where(t => t.LookaheadId == lookaheadId && t.ScheduleTaskId != null)
            .Select(t => t.ScheduleTaskId!.Value)
            .ToListAsync();

        var scheduleTasks = await GetScheduleTasksForPeriodAsync(projectId, start, end);

        var toAdd = scheduleTasks
            .Where(s => !existingIds.Contains(s.Id))
            .Select(s => new LookaheadTask
            {
                LookaheadId    = lookaheadId,
                ScheduleTaskId = s.Id,
                Text           = s.Text,
                StartDate      = s.StartDate < start ? start : s.StartDate,
                EndDate        = s.EndDate   > end   ? end   : s.EndDate,
                Duration       = (s.EndDate < start ? start : s.StartDate < start ? start : s.StartDate)
                                     .DayNumber - (s.StartDate < start ? start : s.StartDate).DayNumber + 1,
                TradeId        = s.TradeId,
                Status         = LookaheadTaskStatus.Planned,
                CreatedAt      = DateTime.UtcNow,
                UpdatedAt      = DateTime.UtcNow
            })
            .ToList();

        // Duration 재계산 (단순하게)
        foreach (var t in toAdd)
            t.Duration = Math.Max(1, t.EndDate.DayNumber - t.StartDate.DayNumber + 1);

        if (toAdd.Count > 0)
        {
            _db.LookaheadTasks.AddRange(toAdd);
            await _db.SaveChangesAsync();
        }
        return toAdd.Count;
    }
}

public class WeeklyPlanService : IWeeklyPlanService
{
    private readonly AppDbContext _db;
    public WeeklyPlanService(AppDbContext db) => _db = db;

    public async Task<List<WeeklyWorkPlan>> GetProjectPlansAsync(int projectId) =>
        await _db.WeeklyWorkPlans
            .Where(w => w.ProjectId == projectId && w.IsActive)
            .OrderByDescending(w => w.WeekStartDate)
            .ToListAsync();

    public async Task<WeeklyWorkPlan?> GetPlanAsync(int id) =>
        await _db.WeeklyWorkPlans
            .Include(w => w.Tasks).ThenInclude(t => t.Trade)
            .Include(w => w.Tasks).ThenInclude(t => t.AssignedTo)
            .FirstOrDefaultAsync(w => w.Id == id && w.IsActive);

    public async Task<WeeklyWorkPlan> GetOrCreateCurrentWeekPlanAsync(int projectId)
    {
        var monday = GetMonday(DateOnly.FromDateTime(DateTime.Today));
        var existing = await _db.WeeklyWorkPlans
            .Include(w => w.Tasks)
            .FirstOrDefaultAsync(w => w.ProjectId == projectId && w.WeekStartDate == monday);

        if (existing != null) return existing;

        var plan = new WeeklyWorkPlan
        {
            ProjectId     = projectId,
            WeekStartDate = monday,
            WeekEndDate   = monday.AddDays(4),
            WeekNumber    = System.Globalization.ISOWeek.GetWeekOfYear(monday.ToDateTime(TimeOnly.MinValue)),
            Year          = monday.Year,
            CreatedAt     = DateTime.UtcNow
        };
        _db.WeeklyWorkPlans.Add(plan);
        await _db.SaveChangesAsync();
        return plan;
    }

    public async Task<WeeklyTask> CommitTaskAsync(int taskId)
    {
        var task = await _db.WeeklyTasks.FindAsync(taskId)
            ?? throw new KeyNotFoundException($"WeeklyTask {taskId} not found");
        task.IsCommitted = true;
        task.UpdatedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return task;
    }

    public async Task<WeeklyTask> CompleteTaskAsync(int taskId, bool completed,
        VarianceCategory? variance, string? note)
    {
        var task = await _db.WeeklyTasks.FindAsync(taskId)
            ?? throw new KeyNotFoundException($"WeeklyTask {taskId} not found");

        task.IsCompleted      = completed;
        task.CompletedDate    = completed ? DateOnly.FromDateTime(DateTime.Today) : null;
        task.VarianceCategory = completed ? null : variance;
        task.VarianceNote     = completed ? null : note;
        task.UpdatedAt        = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Recalculate PPC counts on the plan
        var plan = await _db.WeeklyWorkPlans
            .Include(w => w.Tasks)
            .FirstAsync(w => w.Id == task.WeeklyWorkPlanId);
        plan.TotalTaskCount     = plan.Tasks.Count(t => t.IsCommitted);
        plan.CompletedTaskCount = plan.Tasks.Count(t => t.IsCommitted && t.IsCompleted);
        plan.UpdatedAt          = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return task;
    }

    public async Task<WeeklyPpcStats> CalculatePpcAsync(int planId)
    {
        var plan = await _db.WeeklyWorkPlans
            .Include(w => w.Tasks)
            .FirstOrDefaultAsync(w => w.Id == planId)
            ?? throw new KeyNotFoundException($"WeeklyWorkPlan {planId} not found");

        var committed = plan.Tasks.Where(t => t.IsCommitted).ToList();
        var variances = committed
            .Where(t => !t.IsCompleted && t.VarianceCategory.HasValue)
            .GroupBy(t => t.VarianceCategory!.Value)
            .Select(g => new VarianceStat { Category = g.Key, Count = g.Count() })
            .OrderByDescending(v => v.Count)
            .ToList();

        return new WeeklyPpcStats
        {
            PlanId         = planId,
            TotalCommitted = committed.Count,
            Completed      = committed.Count(t => t.IsCompleted),
            PPC            = plan.PPC,
            Variances      = variances
        };
    }

    private static DateOnly GetMonday(DateOnly date)
    {
        int diff = (7 + ((int)date.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }
}

public class WeeklyPpcStats
{
    public int PlanId         { get; set; }
    public int TotalCommitted { get; set; }
    public int Completed      { get; set; }
    public double PPC         { get; set; }
    public List<VarianceStat> Variances { get; set; } = [];
}

public class VarianceStat
{
    public VarianceCategory Category { get; set; }
    public int Count { get; set; }
}
