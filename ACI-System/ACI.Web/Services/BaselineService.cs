using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Services;

public interface IBaselineService
{
    // ── Baseline CRUD ────────────────────────────────────────────────────────
    Task<List<ScheduleBaseline>> GetBaselinesAsync(int projectId);
    Task<ScheduleBaseline?> GetBaselineAsync(int baselineId);
    Task<ScheduleBaseline?> GetLatestApprovedBaselineAsync(int projectId);

    // ── Freeze ───────────────────────────────────────────────────────────────
    /// <summary>
    /// Freeze current schedule as a new baseline version.
    /// For v1: snapshots ScheduleTask (initial baseline schedule).
    /// For v2+: snapshots WorkingTask (current plan).
    /// </summary>
    Task<ScheduleBaseline> FreezeBaselineAsync(
        int projectId, string title, string? description,
        int userId, string userName);

    // ── Auto Snapshot (for What-If based on Current Plan) ──────────────────
    /// <summary>
    /// Create an auto-snapshot of Current Plan for use as a simulation base.
    /// Auto snapshots are invisible in the baseline version list and do not
    /// go through the Owner approval workflow.
    /// </summary>
    Task<ScheduleBaseline> CreateAutoSnapshotAsync(
        int projectId, int simulationId, int userId, string userName);

    /// <summary>Delete auto-snapshot if no simulations reference it anymore.</summary>
    Task CleanupOrphanedSnapshotsAsync(int projectId);

    // ── Owner approval workflow ──────────────────────────────────────────────
    Task<ScheduleBaseline> SubmitForApprovalAsync(int baselineId);
    Task<ScheduleBaseline> ApproveBaselineAsync(
        int baselineId, string approvedByName, string? notes, DateOnly? approvedDate);
    Task<ScheduleBaseline> RejectBaselineAsync(int baselineId, string? notes);

    // ── Comparison ───────────────────────────────────────────────────────────
    /// <summary>Compare two baseline versions side by side.</summary>
    Task<BaselineComparisonDto> CompareBaselinesAsync(int baselineIdA, int baselineIdB);

    /// <summary>Compare a baseline version vs current plan.</summary>
    Task<BaselineComparisonDto> CompareBaselineVsCurrentAsync(int baselineId, int projectId);

    /// <summary>Get all baseline snapshots for animation (evolution over time).</summary>
    Task<List<BaselineAnimationFrameDto>> GetBaselineEvolutionAsync(int projectId);
}

// ─── DTOs ─────────────────────────────────────────────────────────────────────

public class BaselineComparisonDto
{
    public string LabelA { get; set; } = string.Empty;   // e.g. "Baseline v1"
    public string LabelB { get; set; } = string.Empty;   // e.g. "Baseline v2" or "Current Plan"
    public List<ComparisonTaskDto> Tasks { get; set; } = [];
    public ComparisonSummaryDto Summary { get; set; } = new();
}

public class ComparisonTaskDto
{
    public string Text      { get; set; } = string.Empty;
    public string? WbsCode  { get; set; }
    public string TaskType  { get; set; } = "task";
    public int? ParentIdA   { get; set; }

    // Side A
    public DateOnly? StartDateA { get; set; }
    public DateOnly? EndDateA   { get; set; }
    public int?      DurationA  { get; set; }
    public double?   ProgressA  { get; set; }

    // Side B
    public DateOnly? StartDateB { get; set; }
    public DateOnly? EndDateB   { get; set; }
    public int?      DurationB  { get; set; }
    public double?   ProgressB  { get; set; }

    // Delta
    public int?  DaysShifted   { get; set; }
    public bool  IsNew         { get; set; }   // exists only in B
    public bool  IsRemoved     { get; set; }   // exists only in A
    public bool  IsChanged     { get; set; }   // dates differ
    public string? Color       { get; set; }
}

public class ComparisonSummaryDto
{
    public int TotalTasks       { get; set; }
    public int ChangedTasks     { get; set; }
    public int NewTasks         { get; set; }
    public int RemovedTasks     { get; set; }
    public int DelayedTasks     { get; set; }
    public int AcceleratedTasks { get; set; }
    public int? NetDaysShift    { get; set; }   // project-level shift
}

public class BaselineAnimationFrameDto
{
    public int    BaselineId     { get; set; }
    public int    VersionNumber  { get; set; }
    public string Title          { get; set; } = string.Empty;
    public string Status         { get; set; } = string.Empty;
    public DateTime? ApprovedAt  { get; set; }
    public int    TaskCount      { get; set; }
    public DateOnly? EarliestStart { get; set; }
    public DateOnly? LatestFinish  { get; set; }
    public List<SnapshotTaskDto> Tasks { get; set; } = [];
}

public class SnapshotTaskDto
{
    public int      Id        { get; set; }
    public string   Text      { get; set; } = string.Empty;
    public string?  WbsCode   { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate   { get; set; }
    public int      Duration  { get; set; }
    public double   Progress  { get; set; }
    public int?     ParentId  { get; set; }
    public string   TaskType  { get; set; } = "task";
    public string?  Color     { get; set; }
    public string?  TradeName { get; set; }
}

// ─── Implementation ───────────────────────────────────────────────────────────

public class BaselineService : IBaselineService
{
    private readonly AppDbContext _db;
    public BaselineService(AppDbContext db) => _db = db;

    // ── CRUD ─────────────────────────────────────────────────────────────────

    public async Task<List<ScheduleBaseline>> GetBaselinesAsync(int projectId) =>
        await _db.ScheduleBaselines
            .Where(b => b.ProjectId == projectId && b.IsActive && !b.IsAutoSnapshot)
            .OrderByDescending(b => b.VersionNumber)
            .ToListAsync();

    public async Task<ScheduleBaseline?> GetBaselineAsync(int baselineId) =>
        await _db.ScheduleBaselines
            .Include(b => b.TaskSnapshots)
            .FirstOrDefaultAsync(b => b.Id == baselineId);

    public async Task<ScheduleBaseline?> GetLatestApprovedBaselineAsync(int projectId) =>
        await _db.ScheduleBaselines
            .Where(b => b.ProjectId == projectId
                     && b.Status == BaselineStatus.Approved
                     && b.IsActive)
            .OrderByDescending(b => b.VersionNumber)
            .FirstOrDefaultAsync();

    // ── Freeze ───────────────────────────────────────────────────────────────

    public async Task<ScheduleBaseline> FreezeBaselineAsync(
        int projectId, string title, string? description,
        int userId, string userName)
    {
        var nextVersion = (await _db.ScheduleBaselines
            .Where(b => b.ProjectId == projectId)
            .MaxAsync(b => (int?)b.VersionNumber) ?? 0) + 1;

        var baseline = new ScheduleBaseline
        {
            ProjectId     = projectId,
            VersionNumber = nextVersion,
            Title         = title,
            Description   = description,
            Status        = BaselineStatus.Frozen,
            FrozenAt      = DateTime.UtcNow,
            FrozenById    = userId,
            FrozenByName  = userName,
            DataDate      = DateOnly.FromDateTime(DateTime.Today),
            CreatedAt     = DateTime.UtcNow,
            UpdatedAt     = DateTime.UtcNow,
            CreatedById   = userId
        };

        _db.ScheduleBaselines.Add(baseline);
        await _db.SaveChangesAsync();

        // Determine source: v1 = ScheduleTask, v2+ = WorkingTask (Current Plan)
        bool hasCurrentPlan = await _db.WorkingTasks
            .AnyAsync(t => t.ProjectId == projectId && t.IsActive);

        if (hasCurrentPlan)
            await SnapshotFromWorkingTasks(baseline, projectId);
        else
            await SnapshotFromScheduleTasks(baseline, projectId);

        // Update stats
        var snapshots = await _db.BaselineTaskSnapshots
            .Where(s => s.BaselineId == baseline.Id)
            .ToListAsync();

        baseline.TaskCount        = snapshots.Count;
        baseline.EarliestStart    = snapshots.Min(s => s.StartDate);
        baseline.LatestFinish     = snapshots.Max(s => s.EndDate);
        baseline.TotalCalendarDays = baseline.LatestFinish.HasValue && baseline.EarliestStart.HasValue
            ? baseline.LatestFinish.Value.DayNumber - baseline.EarliestStart.Value.DayNumber
            : null;

        await _db.SaveChangesAsync();
        return baseline;
    }

    private async Task SnapshotFromScheduleTasks(ScheduleBaseline baseline, int projectId)
    {
        var tasks = await _db.ScheduleTasks
            .Where(t => t.ProjectId == projectId && t.IsActive)
            .Include(t => t.Trade)
            .Include(t => t.AssignedTo)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();

        // First pass: create snapshots without parent links
        var idMap = new Dictionary<int, int>();   // ScheduleTask.Id → Snapshot.Id
        var snapshots = new List<BaselineTaskSnapshot>();

        foreach (var t in tasks)
        {
            var snap = new BaselineTaskSnapshot
            {
                BaselineId           = baseline.Id,
                SourceScheduleTaskId = t.Id,
                WbsCode              = t.WbsCode,
                Text                 = t.Text,
                Description          = t.Description,
                Location             = t.Location,
                TaskType             = t.TaskType,
                SortOrder            = t.SortOrder,
                IsOpen               = t.IsOpen,
                TradeId              = t.TradeId,
                TradeName            = t.Trade?.Name,
                TradeColor           = t.Trade?.Color,
                AssignedToId         = t.AssignedToId,
                AssignedToName       = t.AssignedTo?.FullName,
                StartDate            = t.StartDate,
                EndDate              = t.EndDate,
                Duration             = t.Duration,
                Progress             = t.Progress,
                ActualStartDate      = t.ActualStartDate,
                ActualEndDate        = t.ActualEndDate,
                ConstraintType       = t.ConstraintType,
                ConstraintDate       = t.ConstraintDate,
                Color                = t.Color,
                CrewSize             = t.CrewSize,
                Notes                = t.Notes
            };
            snapshots.Add(snap);
            _db.BaselineTaskSnapshots.Add(snap);
        }

        await _db.SaveChangesAsync();

        // Build ID map
        for (int i = 0; i < tasks.Count; i++)
            idMap[tasks[i].Id] = snapshots[i].Id;

        // Second pass: resolve parent links
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].ParentId.HasValue && idMap.TryGetValue(tasks[i].ParentId.Value, out var parentSnapId))
                snapshots[i].ParentSnapshotId = parentSnapId;
        }

        await _db.SaveChangesAsync();
    }

    private async Task SnapshotFromWorkingTasks(ScheduleBaseline baseline, int projectId)
    {
        var tasks = await _db.WorkingTasks
            .Where(t => t.ProjectId == projectId && t.IsActive
                     && t.WorkingStatus == WorkingTaskStatus.Active)
            .Include(t => t.Trade)
            .Include(t => t.AssignedTo)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();

        var idMap = new Dictionary<int, int>();   // WorkingTask.Id → Snapshot.Id
        var snapshots = new List<BaselineTaskSnapshot>();

        foreach (var t in tasks)
        {
            var snap = new BaselineTaskSnapshot
            {
                BaselineId          = baseline.Id,
                SourceWorkingTaskId = t.Id,
                WbsCode             = t.WbsCode,
                Text                = t.Text,
                Description         = t.Description,
                Location            = t.Location,
                TaskType            = t.TaskType,
                SortOrder           = t.SortOrder,
                IsOpen              = t.IsOpen,
                TradeId             = t.TradeId,
                TradeName           = t.Trade?.Name,
                TradeColor          = t.Trade?.Color,
                AssignedToId        = t.AssignedToId,
                AssignedToName      = t.AssignedTo?.FullName,
                StartDate           = t.StartDate,
                EndDate             = t.EndDate,
                Duration            = t.Duration,
                Progress            = t.Progress,
                ActualStartDate     = t.ActualStartDate,
                ActualEndDate       = t.ActualEndDate,
                ConstraintType      = t.ConstraintType,
                ConstraintDate      = t.ConstraintDate,
                Color               = t.Color,
                CrewSize            = t.CrewSize,
                Notes               = t.Notes
            };
            snapshots.Add(snap);
            _db.BaselineTaskSnapshots.Add(snap);
        }

        await _db.SaveChangesAsync();

        for (int i = 0; i < tasks.Count; i++)
            idMap[tasks[i].Id] = snapshots[i].Id;

        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].ParentId.HasValue && idMap.TryGetValue(tasks[i].ParentId.Value, out var parentSnapId))
                snapshots[i].ParentSnapshotId = parentSnapId;
        }

        await _db.SaveChangesAsync();
    }

    // ── Owner Approval Workflow ──────────────────────────────────────────────

    public async Task<ScheduleBaseline> SubmitForApprovalAsync(int baselineId)
    {
        var baseline = await _db.ScheduleBaselines.FindAsync(baselineId)
            ?? throw new KeyNotFoundException($"Baseline {baselineId} not found");

        if (baseline.Status != BaselineStatus.Frozen)
            throw new InvalidOperationException($"Baseline must be Frozen to submit. Current: {baseline.Status}");

        baseline.Status      = BaselineStatus.Submitted;
        baseline.SubmittedAt = DateTime.UtcNow;
        baseline.UpdatedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return baseline;
    }

    public async Task<ScheduleBaseline> ApproveBaselineAsync(
        int baselineId, string approvedByName, string? notes, DateOnly? approvedDate)
    {
        var baseline = await _db.ScheduleBaselines.FindAsync(baselineId)
            ?? throw new KeyNotFoundException($"Baseline {baselineId} not found");

        if (baseline.Status != BaselineStatus.Submitted && baseline.Status != BaselineStatus.Frozen)
            throw new InvalidOperationException($"Baseline must be Submitted or Frozen to approve. Current: {baseline.Status}");

        // Supersede the previous approved baseline
        var previousApproved = await _db.ScheduleBaselines
            .Where(b => b.ProjectId == baseline.ProjectId
                     && b.Status == BaselineStatus.Approved
                     && b.Id != baselineId
                     && b.IsActive)
            .ToListAsync();

        foreach (var prev in previousApproved)
        {
            prev.Status    = BaselineStatus.Superseded;
            prev.UpdatedAt = DateTime.UtcNow;
        }

        baseline.Status         = BaselineStatus.Approved;
        baseline.ApprovedAt     = approvedDate.HasValue
            ? approvedDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            : DateTime.UtcNow;
        baseline.ApprovedByName = approvedByName;
        baseline.ApprovalNotes  = notes;
        baseline.UpdatedAt      = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return baseline;
    }

    public async Task<ScheduleBaseline> RejectBaselineAsync(int baselineId, string? notes)
    {
        var baseline = await _db.ScheduleBaselines.FindAsync(baselineId)
            ?? throw new KeyNotFoundException($"Baseline {baselineId} not found");

        baseline.Status        = BaselineStatus.Rejected;
        baseline.ApprovalNotes = notes;
        baseline.UpdatedAt     = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return baseline;
    }

    // ── Comparison ───────────────────────────────────────────────────────────

    public async Task<BaselineComparisonDto> CompareBaselinesAsync(int baselineIdA, int baselineIdB)
    {
        var bA = await _db.ScheduleBaselines
            .Include(b => b.TaskSnapshots)
            .FirstOrDefaultAsync(b => b.Id == baselineIdA)
            ?? throw new KeyNotFoundException($"Baseline {baselineIdA} not found");

        var bB = await _db.ScheduleBaselines
            .Include(b => b.TaskSnapshots)
            .FirstOrDefaultAsync(b => b.Id == baselineIdB)
            ?? throw new KeyNotFoundException($"Baseline {baselineIdB} not found");

        var tasksA = bA.TaskSnapshots.ToDictionary(
            s => s.SourceScheduleTaskId ?? s.SourceWorkingTaskId ?? s.Id,
            s => s);
        var tasksB = bB.TaskSnapshots.ToDictionary(
            s => s.SourceScheduleTaskId ?? s.SourceWorkingTaskId ?? s.Id,
            s => s);

        return BuildComparison(bA.VersionLabel, bB.VersionLabel, tasksA, tasksB);
    }

    public async Task<BaselineComparisonDto> CompareBaselineVsCurrentAsync(
        int baselineId, int projectId)
    {
        var baseline = await _db.ScheduleBaselines
            .Include(b => b.TaskSnapshots)
            .FirstOrDefaultAsync(b => b.Id == baselineId)
            ?? throw new KeyNotFoundException($"Baseline {baselineId} not found");

        var currentTasks = await _db.WorkingTasks
            .Where(t => t.ProjectId == projectId && t.IsActive
                     && t.WorkingStatus == WorkingTaskStatus.Active)
            .Include(t => t.Trade)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();

        var tasksA = baseline.TaskSnapshots.ToDictionary(
            s => s.SourceScheduleTaskId ?? s.SourceWorkingTaskId ?? s.Id,
            s => s);

        // Convert WorkingTasks to snapshot-like dict
        var tasksB = currentTasks.ToDictionary(
            t => (int)(t.BaselineTaskId ?? t.Id),
            t => new BaselineTaskSnapshot
            {
                Id        = t.Id,
                WbsCode   = t.WbsCode,
                Text      = t.Text,
                TaskType  = t.TaskType,
                StartDate = t.StartDate,
                EndDate   = t.EndDate,
                Duration  = t.Duration,
                Progress  = t.Progress,
                Color     = t.Color,
                TradeName = t.Trade?.Name,
                ParentSnapshotId = t.ParentId
            });

        return BuildComparison(baseline.VersionLabel, "Current Plan", tasksA, tasksB);
    }

    private static BaselineComparisonDto BuildComparison(
        string labelA, string labelB,
        Dictionary<int, BaselineTaskSnapshot> tasksA,
        Dictionary<int, BaselineTaskSnapshot> tasksB)
    {
        var allKeys = tasksA.Keys.Union(tasksB.Keys).ToHashSet();
        var comparisons = new List<ComparisonTaskDto>();
        int changed = 0, newCount = 0, removed = 0, delayed = 0, accelerated = 0;
        int totalShift = 0;

        foreach (var key in allKeys)
        {
            var hasA = tasksA.TryGetValue(key, out var a);
            var hasB = tasksB.TryGetValue(key, out var b);

            var dto = new ComparisonTaskDto
            {
                Text       = b?.Text ?? a?.Text ?? "",
                WbsCode    = b?.WbsCode ?? a?.WbsCode,
                TaskType   = (b?.GanttTypeString ?? a?.GanttTypeString) ?? "task",
                Color      = b?.Color ?? a?.Color,
                StartDateA = a?.StartDate,
                EndDateA   = a?.EndDate,
                DurationA  = a?.Duration,
                ProgressA  = a?.Progress,
                StartDateB = b?.StartDate,
                EndDateB   = b?.EndDate,
                DurationB  = b?.Duration,
                ProgressB  = b?.Progress,
                IsNew      = !hasA && hasB,
                IsRemoved  = hasA && !hasB,
            };

            if (hasA && hasB)
            {
                var shift = b!.EndDate.DayNumber - a!.EndDate.DayNumber;
                dto.DaysShifted = shift != 0 ? shift : null;
                dto.IsChanged   = a.StartDate != b.StartDate || a.EndDate != b.EndDate;
                if (dto.IsChanged) changed++;
                if (shift > 0) { delayed++; totalShift += shift; }
                if (shift < 0) { accelerated++; totalShift += shift; }
            }
            else if (dto.IsNew)  newCount++;
            else if (dto.IsRemoved) removed++;

            comparisons.Add(dto);
        }

        return new BaselineComparisonDto
        {
            LabelA = labelA,
            LabelB = labelB,
            Tasks  = comparisons.OrderBy(t => t.WbsCode).ToList(),
            Summary = new ComparisonSummaryDto
            {
                TotalTasks       = allKeys.Count,
                ChangedTasks     = changed,
                NewTasks         = newCount,
                RemovedTasks     = removed,
                DelayedTasks     = delayed,
                AcceleratedTasks = accelerated,
                NetDaysShift     = totalShift != 0 ? totalShift : null
            }
        };
    }

    // ── Auto Snapshot (for What-If based on Current Plan) ──────────────────

    public async Task<ScheduleBaseline> CreateAutoSnapshotAsync(
        int projectId, int simulationId, int userId, string userName)
    {
        // Use a separate version numbering for auto snapshots to avoid
        // colliding with user-created baselines.
        // Convention: negative version numbers (or 0) for auto snapshots.
        var nextAutoNum = (await _db.ScheduleBaselines
            .Where(b => b.ProjectId == projectId && b.IsAutoSnapshot)
            .MinAsync(b => (int?)b.VersionNumber) ?? 0) - 1;

        var baseline = new ScheduleBaseline
        {
            ProjectId          = projectId,
            VersionNumber      = nextAutoNum,
            Title              = $"Auto Snapshot (Simulation #{simulationId})",
            Description        = "Automatically created snapshot of Current Plan for What-If simulation.",
            Status             = BaselineStatus.Frozen,
            IsAutoSnapshot     = true,
            SourceSimulationId = simulationId,
            FrozenAt           = DateTime.UtcNow,
            FrozenById         = userId,
            FrozenByName       = userName,
            DataDate           = DateOnly.FromDateTime(DateTime.Today),
            CreatedAt          = DateTime.UtcNow,
            UpdatedAt          = DateTime.UtcNow,
            CreatedById        = userId
        };

        _db.ScheduleBaselines.Add(baseline);
        await _db.SaveChangesAsync();

        // Snapshot from Current Plan (WorkingTasks)
        await SnapshotFromWorkingTasks(baseline, projectId);

        // Update stats
        var snapshots = await _db.BaselineTaskSnapshots
            .Where(s => s.BaselineId == baseline.Id)
            .ToListAsync();

        baseline.TaskCount     = snapshots.Count;
        baseline.EarliestStart = snapshots.Any() ? snapshots.Min(s => s.StartDate) : null;
        baseline.LatestFinish  = snapshots.Any() ? snapshots.Max(s => s.EndDate) : null;
        baseline.TotalCalendarDays = baseline.LatestFinish.HasValue && baseline.EarliestStart.HasValue
            ? baseline.LatestFinish.Value.DayNumber - baseline.EarliestStart.Value.DayNumber
            : null;

        await _db.SaveChangesAsync();
        return baseline;
    }

    public async Task CleanupOrphanedSnapshotsAsync(int projectId)
    {
        // Find auto snapshots whose source simulation no longer exists or is inactive
        var orphans = await _db.ScheduleBaselines
            .Where(b => b.ProjectId == projectId
                     && b.IsAutoSnapshot
                     && b.IsActive)
            .ToListAsync();

        foreach (var snap in orphans)
        {
            if (snap.SourceSimulationId == null)
            {
                snap.IsActive = false;
                continue;
            }

            // Check if the simulation still references this snapshot
            var simExists = await _db.ScheduleSimulations
                .AnyAsync(s => s.SourceBaselineId == snap.Id && s.IsActive);

            if (!simExists)
            {
                snap.IsActive  = false;
                snap.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();
    }

    // ── Animation ────────────────────────────────────────────────────────────

    public async Task<List<BaselineAnimationFrameDto>> GetBaselineEvolutionAsync(int projectId)
    {
        var baselines = await _db.ScheduleBaselines
            .Where(b => b.ProjectId == projectId && b.IsActive && !b.IsAutoSnapshot
                     && (b.Status == BaselineStatus.Approved || b.Status == BaselineStatus.Superseded))
            .Include(b => b.TaskSnapshots)
            .OrderBy(b => b.VersionNumber)
            .ToListAsync();

        return baselines.Select(b => new BaselineAnimationFrameDto
        {
            BaselineId    = b.Id,
            VersionNumber = b.VersionNumber,
            Title         = b.Title,
            Status        = b.Status.ToString(),
            ApprovedAt    = b.ApprovedAt,
            TaskCount     = b.TaskCount,
            EarliestStart = b.EarliestStart,
            LatestFinish  = b.LatestFinish,
            Tasks = b.TaskSnapshots
                .OrderBy(s => s.SortOrder)
                .Select(s => new SnapshotTaskDto
                {
                    Id        = s.Id,
                    Text      = s.Text,
                    WbsCode   = s.WbsCode,
                    StartDate = s.StartDate,
                    EndDate   = s.EndDate,
                    Duration  = s.Duration,
                    Progress  = s.Progress,
                    ParentId  = s.ParentSnapshotId,
                    TaskType  = s.GanttTypeString,
                    Color     = s.Color,
                    TradeName = s.TradeName
                }).ToList()
        }).ToList();
    }
}
