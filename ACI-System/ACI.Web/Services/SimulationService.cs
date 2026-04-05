using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Services;

public interface ISimulationService
{
    // ── CRUD ─────────────────────────────────────────────────────────────────
    Task<List<ScheduleSimulation>> GetSimulationsAsync(int projectId);
    Task<ScheduleSimulation?> GetSimulationAsync(int simulationId);

    /// <summary>Create a new simulation from Current Plan or a specific Baseline.</summary>
    Task<ScheduleSimulation> CreateSimulationAsync(
        int projectId, string name, string? description,
        SimulationSourceType sourceType, int? sourceBaselineId,
        int userId, string userName);

    Task<ScheduleSimulation> UpdateSimulationAsync(int simulationId, string name, string? description);
    Task ArchiveSimulationAsync(int simulationId);
    Task DeleteSimulationAsync(int simulationId);

    // ── Task modifications ───────────────────────────────────────────────────
    /// <summary>Modify a task within the simulation (only changed fields).</summary>
    Task<SimulationTask> ModifyTaskAsync(int simulationId, SimulationTask modification);

    /// <summary>Add a brand-new task to the simulation (not in source).</summary>
    Task<SimulationTask> AddNewTaskAsync(int simulationId, SimulationTask newTask);

    /// <summary>Mark a task as removed in the simulation.</summary>
    Task<SimulationTask> RemoveTaskAsync(int simulationId, int sourceTaskId, string? reason);

    /// <summary>Revert a task modification (remove from simulation overrides).</summary>
    Task RevertTaskAsync(int simulationTaskId);

    // ── Comparison ───────────────────────────────────────────────────────────
    /// <summary>Get full merged view: source tasks + simulation overrides applied.</summary>
    Task<SimulationResultDto> GetSimulationResultAsync(int simulationId);

    /// <summary>Compare two simulations side by side.</summary>
    Task<SimulationComparisonDto> CompareSimulationsAsync(int simIdA, int simIdB);

    // ── Promote ──────────────────────────────────────────────────────────────
    /// <summary>Apply simulation changes to Current Plan (WorkingTasks).</summary>
    Task PromoteToCurrentPlanAsync(int simulationId, int revisionId, int userId, string userName);

    // ── Impact analysis ──────────────────────────────────────────────────────
    /// <summary>Recalculate simulation impact summary (task count, net days shift).</summary>
    Task<ScheduleSimulation> RecalculateImpactAsync(int simulationId);
}

// ─── DTOs ─────────────────────────────────────────────────────────────────────

public class SimulationResultDto
{
    public int SimulationId { get; set; }
    public string SimulationName { get; set; } = string.Empty;
    public string SourceLabel { get; set; } = string.Empty;   // "Current Plan" or "Baseline v2"

    public List<SimulationResultTaskDto> Tasks { get; set; } = [];
    public SimulationImpactDto Impact { get; set; } = new();
}

public class SimulationResultTaskDto
{
    public int      Id        { get; set; }       // Source task ID
    public int?     SimTaskId { get; set; }       // SimulationTask ID if modified
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

    // Original values (before simulation override)
    public DateOnly? OriginalStartDate { get; set; }
    public DateOnly? OriginalEndDate   { get; set; }
    public int?      OriginalDuration  { get; set; }

    // Flags
    public bool IsModified { get; set; }
    public bool IsNewTask  { get; set; }
    public bool IsRemoved  { get; set; }
    public int? DaysShifted { get; set; }
    public string? ChangeReason { get; set; }
}

public class SimulationImpactDto
{
    public int ModifiedTasks   { get; set; }
    public int NewTasks        { get; set; }
    public int RemovedTasks    { get; set; }
    public int DelayedTasks    { get; set; }
    public int AcceleratedTasks { get; set; }
    public int? NetDaysShift   { get; set; }
    public DateOnly? OriginalEndDate   { get; set; }
    public DateOnly? SimulatedEndDate  { get; set; }
}

public class SimulationComparisonDto
{
    public string LabelA { get; set; } = string.Empty;
    public string LabelB { get; set; } = string.Empty;
    public List<SimulationResultTaskDto> TasksA { get; set; } = [];
    public List<SimulationResultTaskDto> TasksB { get; set; } = [];
    public SimulationImpactDto ImpactA { get; set; } = new();
    public SimulationImpactDto ImpactB { get; set; } = new();
}

// ─── Implementation ───────────────────────────────────────────────────────────

public class SimulationService : ISimulationService
{
    private readonly AppDbContext _db;
    private readonly IBaselineService _baselineSvc;

    public SimulationService(AppDbContext db, IBaselineService baselineSvc)
    {
        _db = db;
        _baselineSvc = baselineSvc;
    }

    // ── CRUD ─────────────────────────────────────────────────────────────────

    public async Task<List<ScheduleSimulation>> GetSimulationsAsync(int projectId) =>
        await _db.ScheduleSimulations
            .Where(s => s.ProjectId == projectId && s.IsActive)
            .Include(s => s.SourceBaseline)
            .OrderByDescending(s => s.UpdatedAt)
            .ToListAsync();

    public async Task<ScheduleSimulation?> GetSimulationAsync(int simulationId) =>
        await _db.ScheduleSimulations
            .Include(s => s.Tasks)
            .Include(s => s.SourceBaseline)
            .FirstOrDefaultAsync(s => s.Id == simulationId);

    public async Task<ScheduleSimulation> CreateSimulationAsync(
        int projectId, string name, string? description,
        SimulationSourceType sourceType, int? sourceBaselineId,
        int userId, string userName)
    {
        var simulation = new ScheduleSimulation
        {
            ProjectId        = projectId,
            Name             = name,
            Description      = description,
            Status           = SimulationStatus.Active,
            SourceType       = sourceType,
            SourceBaselineId = sourceBaselineId,
            CreatedByUserId  = userId,
            CreatedByName    = userName,
            CreatedAt        = DateTime.UtcNow,
            UpdatedAt        = DateTime.UtcNow,
            CreatedById      = userId
        };

        _db.ScheduleSimulations.Add(simulation);
        await _db.SaveChangesAsync();

        // ── Auto Snapshot for Current Plan-based simulations ──────────────
        // All simulations must reference an immutable snapshot so that
        // concurrent simulations don't interfere with each other when
        // the live Current Plan changes.
        if (sourceType == SimulationSourceType.CurrentPlan)
        {
            var autoSnapshot = await _baselineSvc.CreateAutoSnapshotAsync(
                projectId, simulation.Id, userId, userName);

            simulation.SourceBaselineId = autoSnapshot.Id;
            simulation.SourceType       = SimulationSourceType.Baseline;
            await _db.SaveChangesAsync();
        }

        return simulation;
    }

    public async Task<ScheduleSimulation> UpdateSimulationAsync(
        int simulationId, string name, string? description)
    {
        var sim = await _db.ScheduleSimulations.FindAsync(simulationId)
            ?? throw new KeyNotFoundException();
        sim.Name        = name;
        sim.Description = description;
        sim.UpdatedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return sim;
    }

    public async Task ArchiveSimulationAsync(int simulationId)
    {
        var sim = await _db.ScheduleSimulations.FindAsync(simulationId)
            ?? throw new KeyNotFoundException();
        sim.Status    = SimulationStatus.Archived;
        sim.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteSimulationAsync(int simulationId)
    {
        var sim = await _db.ScheduleSimulations.FindAsync(simulationId)
            ?? throw new KeyNotFoundException();
        sim.IsActive  = false;
        sim.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Cleanup orphaned auto snapshots that are no longer referenced
        await _baselineSvc.CleanupOrphanedSnapshotsAsync(sim.ProjectId);
    }

    // ── Task Modifications ───────────────────────────────────────────────────

    public async Task<SimulationTask> ModifyTaskAsync(int simulationId, SimulationTask modification)
    {
        // Check if override already exists for this source task
        var existing = await _db.SimulationTasks
            .FirstOrDefaultAsync(t => t.SimulationId == simulationId
                && ((modification.SourceWorkingTaskId != null && t.SourceWorkingTaskId == modification.SourceWorkingTaskId)
                 || (modification.SourceSnapshotId != null && t.SourceSnapshotId == modification.SourceSnapshotId)));

        if (existing != null)
        {
            // Merge overrides
            if (modification.Text != null) existing.Text = modification.Text;
            if (modification.StartDate.HasValue) existing.StartDate = modification.StartDate;
            if (modification.EndDate.HasValue) existing.EndDate = modification.EndDate;
            if (modification.Duration.HasValue) existing.Duration = modification.Duration;
            if (modification.Progress.HasValue) existing.Progress = modification.Progress;
            if (modification.TradeId.HasValue) existing.TradeId = modification.TradeId;
            if (modification.CrewSize.HasValue) existing.CrewSize = modification.CrewSize;
            if (modification.Notes != null) existing.Notes = modification.Notes;
            if (modification.ChangeReason != null) existing.ChangeReason = modification.ChangeReason;
            existing.DaysShifted = modification.DaysShifted;

            await _db.SaveChangesAsync();
            await RecalculateImpactAsync(simulationId);
            return existing;
        }

        modification.SimulationId = simulationId;
        _db.SimulationTasks.Add(modification);
        await _db.SaveChangesAsync();
        await RecalculateImpactAsync(simulationId);
        return modification;
    }

    public async Task<SimulationTask> AddNewTaskAsync(int simulationId, SimulationTask newTask)
    {
        newTask.SimulationId = simulationId;
        newTask.IsNewTask    = true;
        _db.SimulationTasks.Add(newTask);
        await _db.SaveChangesAsync();
        await RecalculateImpactAsync(simulationId);
        return newTask;
    }

    public async Task<SimulationTask> RemoveTaskAsync(
        int simulationId, int sourceTaskId, string? reason)
    {
        var sim = await _db.ScheduleSimulations
            .Include(s => s.SourceBaseline)
            .FirstOrDefaultAsync(s => s.Id == simulationId)
            ?? throw new KeyNotFoundException();

        var removal = new SimulationTask
        {
            SimulationId = simulationId,
            IsRemoved    = true,
            ChangeReason = reason
        };

        // Legacy CurrentPlan simulations reference WorkingTask IDs directly.
        // All new simulations (including auto-snapshot-based) reference snapshot IDs.
        if (sim.SourceType == SimulationSourceType.CurrentPlan)
            removal.SourceWorkingTaskId = sourceTaskId;
        else
            removal.SourceSnapshotId = sourceTaskId;

        _db.SimulationTasks.Add(removal);
        await _db.SaveChangesAsync();
        await RecalculateImpactAsync(simulationId);
        return removal;
    }

    public async Task RevertTaskAsync(int simulationTaskId)
    {
        var task = await _db.SimulationTasks.FindAsync(simulationTaskId)
            ?? throw new KeyNotFoundException();
        var simId = task.SimulationId;
        _db.SimulationTasks.Remove(task);
        await _db.SaveChangesAsync();
        await RecalculateImpactAsync(simId);
    }

    // ── Result / Merged View ─────────────────────────────────────────────────

    public async Task<SimulationResultDto> GetSimulationResultAsync(int simulationId)
    {
        var sim = await _db.ScheduleSimulations
            .Include(s => s.Tasks)
            .Include(s => s.SourceBaseline)
            .FirstOrDefaultAsync(s => s.Id == simulationId)
            ?? throw new KeyNotFoundException();

        var overrides = sim.Tasks.ToList();

        List<SimulationResultTaskDto> resultTasks;
        DateOnly? originalEnd;

        // All simulations now reference baseline snapshots (including auto-snapshots
        // of Current Plan). The legacy CurrentPlan branch is kept for backward
        // compatibility with any simulations created before the auto-snapshot feature.
        if (sim.SourceType == SimulationSourceType.CurrentPlan)
        {
            // Legacy path: direct reference to live WorkingTasks
            var sourceTasks = await _db.WorkingTasks
                .Where(t => t.ProjectId == sim.ProjectId && t.IsActive
                         && t.WorkingStatus == WorkingTaskStatus.Active)
                .Include(t => t.Trade)
                .OrderBy(t => t.SortOrder)
                .ToListAsync();

            originalEnd = sourceTasks.Any() ? sourceTasks.Max(t => t.EndDate) : null;
            resultTasks = MergeCurrentPlanWithOverrides(sourceTasks, overrides);
        }
        else
        {
            var snapshots = await _db.BaselineTaskSnapshots
                .Where(s => s.BaselineId == sim.SourceBaselineId)
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            originalEnd = snapshots.Any() ? snapshots.Max(s => s.EndDate) : null;
            resultTasks = MergeBaselineWithOverrides(snapshots, overrides);
        }

        // Add new tasks from simulation
        var newTasks = overrides.Where(o => o.IsNewTask).Select(o => new SimulationResultTaskDto
        {
            SimTaskId    = o.Id,
            Text         = o.Text ?? "(New Task)",
            StartDate    = o.StartDate ?? DateOnly.FromDateTime(DateTime.Today),
            EndDate      = o.EndDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Duration     = o.Duration ?? 1,
            Progress     = o.Progress ?? 0,
            TaskType     = "task",
            IsNewTask    = true,
            IsModified   = true,
            ChangeReason = o.ChangeReason
        });
        resultTasks.AddRange(newTasks);

        var simEnd = resultTasks.Where(t => !t.IsRemoved).Any()
            ? resultTasks.Where(t => !t.IsRemoved).Max(t => t.EndDate)
            : originalEnd;

        var impact = new SimulationImpactDto
        {
            ModifiedTasks   = resultTasks.Count(t => t.IsModified && !t.IsNewTask && !t.IsRemoved),
            NewTasks        = resultTasks.Count(t => t.IsNewTask),
            RemovedTasks    = resultTasks.Count(t => t.IsRemoved),
            DelayedTasks    = resultTasks.Count(t => t.DaysShifted > 0),
            AcceleratedTasks = resultTasks.Count(t => t.DaysShifted < 0),
            NetDaysShift    = originalEnd.HasValue && simEnd.HasValue
                ? simEnd.Value.DayNumber - originalEnd.Value.DayNumber
                : null,
            OriginalEndDate  = originalEnd,
            SimulatedEndDate = simEnd
        };

        return new SimulationResultDto
        {
            SimulationId   = simulationId,
            SimulationName = sim.Name,
            SourceLabel    = sim.SourceBaseline?.IsAutoSnapshot == true
                ? "Current Plan (snapshot)"
                : sim.SourceType == SimulationSourceType.CurrentPlan
                    ? "Current Plan"
                    : sim.SourceBaseline?.VersionLabel ?? "Baseline",
            Tasks  = resultTasks,
            Impact = impact
        };
    }

    private static List<SimulationResultTaskDto> MergeCurrentPlanWithOverrides(
        List<WorkingTask> source, List<SimulationTask> overrides)
    {
        var overrideMap = overrides
            .Where(o => o.SourceWorkingTaskId.HasValue)
            .ToDictionary(o => o.SourceWorkingTaskId!.Value);

        return source.Select(t =>
        {
            var dto = new SimulationResultTaskDto
            {
                Id        = t.Id,
                Text      = t.Text,
                WbsCode   = t.WbsCode,
                StartDate = t.StartDate,
                EndDate   = t.EndDate,
                Duration  = t.Duration,
                Progress  = t.Progress,
                ParentId  = t.ParentId,
                TaskType  = t.GanttTypeString,
                Color     = t.Color,
                TradeName = t.Trade?.Name,
                OriginalStartDate = t.StartDate,
                OriginalEndDate   = t.EndDate,
                OriginalDuration  = t.Duration,
            };

            if (overrideMap.TryGetValue(t.Id, out var ov))
            {
                dto.SimTaskId    = ov.Id;
                dto.IsModified   = true;
                dto.IsRemoved    = ov.IsRemoved;
                dto.ChangeReason = ov.ChangeReason;
                if (ov.Text != null)          dto.Text      = ov.Text;
                if (ov.StartDate.HasValue)    dto.StartDate = ov.StartDate.Value;
                if (ov.EndDate.HasValue)      dto.EndDate   = ov.EndDate.Value;
                if (ov.Duration.HasValue)     dto.Duration  = ov.Duration.Value;
                if (ov.Progress.HasValue)     dto.Progress  = ov.Progress.Value;
                dto.DaysShifted = dto.EndDate.DayNumber - t.EndDate.DayNumber;
                if (dto.DaysShifted == 0) dto.DaysShifted = null;
            }
            return dto;
        }).ToList();
    }

    private static List<SimulationResultTaskDto> MergeBaselineWithOverrides(
        List<BaselineTaskSnapshot> source, List<SimulationTask> overrides)
    {
        var overrideMap = overrides
            .Where(o => o.SourceSnapshotId.HasValue)
            .ToDictionary(o => o.SourceSnapshotId!.Value);

        return source.Select(s =>
        {
            var dto = new SimulationResultTaskDto
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
                TradeName = s.TradeName,
                OriginalStartDate = s.StartDate,
                OriginalEndDate   = s.EndDate,
                OriginalDuration  = s.Duration,
            };

            if (overrideMap.TryGetValue(s.Id, out var ov))
            {
                dto.SimTaskId    = ov.Id;
                dto.IsModified   = true;
                dto.IsRemoved    = ov.IsRemoved;
                dto.ChangeReason = ov.ChangeReason;
                if (ov.Text != null)          dto.Text      = ov.Text;
                if (ov.StartDate.HasValue)    dto.StartDate = ov.StartDate.Value;
                if (ov.EndDate.HasValue)      dto.EndDate   = ov.EndDate.Value;
                if (ov.Duration.HasValue)     dto.Duration  = ov.Duration.Value;
                if (ov.Progress.HasValue)     dto.Progress  = ov.Progress.Value;
                dto.DaysShifted = dto.EndDate.DayNumber - s.EndDate.DayNumber;
                if (dto.DaysShifted == 0) dto.DaysShifted = null;
            }
            return dto;
        }).ToList();
    }

    // ── Compare Simulations ──────────────────────────────────────────────────

    public async Task<SimulationComparisonDto> CompareSimulationsAsync(int simIdA, int simIdB)
    {
        var resultA = await GetSimulationResultAsync(simIdA);
        var resultB = await GetSimulationResultAsync(simIdB);

        return new SimulationComparisonDto
        {
            LabelA  = resultA.SimulationName,
            LabelB  = resultB.SimulationName,
            TasksA  = resultA.Tasks,
            TasksB  = resultB.Tasks,
            ImpactA = resultA.Impact,
            ImpactB = resultB.Impact
        };
    }

    // ── Promote to Current Plan ──────────────────────────────────────────────

    public async Task PromoteToCurrentPlanAsync(
        int simulationId, int revisionId, int userId, string userName)
    {
        var sim = await _db.ScheduleSimulations
            .Include(s => s.Tasks)
            .Include(s => s.SourceBaseline)
            .FirstOrDefaultAsync(s => s.Id == simulationId)
            ?? throw new KeyNotFoundException();

        // Allow promotion for:
        //  1. Legacy simulations with SourceType == CurrentPlan (direct WotkingTask refs)
        //  2. Auto-snapshot-based simulations (originally from Current Plan)
        bool isAutoSnapshotBased = sim.SourceBaseline?.IsAutoSnapshot == true;
        bool isLegacyCurrentPlan = sim.SourceType == SimulationSourceType.CurrentPlan;

        if (!isAutoSnapshotBased && !isLegacyCurrentPlan)
            throw new InvalidOperationException(
                "Only simulations based on Current Plan (or auto-snapshot) can be promoted directly.");

        // Build snapshot-to-working-task mapping for auto-snapshot-based simulations
        Dictionary<int, int>? snapshotToWorkingMap = null;
        if (isAutoSnapshotBased && sim.SourceBaselineId.HasValue)
        {
            snapshotToWorkingMap = await _db.BaselineTaskSnapshots
                .Where(s => s.BaselineId == sim.SourceBaselineId.Value
                         && s.SourceWorkingTaskId.HasValue)
                .ToDictionaryAsync(s => s.Id, s => s.SourceWorkingTaskId!.Value);
        }

        foreach (var mod in sim.Tasks.Where(t => !t.IsNewTask && !t.IsRemoved && t.HasAnyChange))
        {
            // Resolve to WorkingTask ID
            int? workingTaskId = mod.SourceWorkingTaskId;   // Legacy path
            if (workingTaskId == null && mod.SourceSnapshotId.HasValue && snapshotToWorkingMap != null)
            {
                if (snapshotToWorkingMap.TryGetValue(mod.SourceSnapshotId.Value, out var mapped))
                    workingTaskId = mapped;
            }

            if (workingTaskId == null) continue;

            var working = await _db.WorkingTasks.FindAsync(workingTaskId.Value);
            if (working == null) continue;

            // Record change
            var change = new ScheduleChange
            {
                RevisionId    = revisionId,
                WorkingTaskId = working.Id,
                ChangeType    = ChangeType.DatesShifted,
                OldStartDate  = working.StartDate,
                OldEndDate    = working.EndDate,
                OldDuration   = working.Duration,
                OldProgress   = working.Progress,
                OldText       = working.Text,
                ChangeNote    = $"Promoted from simulation: {sim.Name}. {mod.ChangeReason}",
                ChangedById   = userId,
                ChangedByName = userName,
                ChangedAt     = DateTime.UtcNow
            };

            // Apply overrides
            if (mod.Text != null) working.Text = mod.Text;
            if (mod.StartDate.HasValue) working.StartDate = mod.StartDate.Value;
            if (mod.EndDate.HasValue) working.EndDate = mod.EndDate.Value;
            if (mod.Duration.HasValue) working.Duration = mod.Duration.Value;
            if (mod.Progress.HasValue) working.Progress = mod.Progress.Value;
            working.UpdatedAt = DateTime.UtcNow;

            change.NewStartDate = working.StartDate;
            change.NewEndDate   = working.EndDate;
            change.NewDuration  = working.Duration;
            change.NewProgress  = working.Progress;
            change.NewText      = working.Text;
            change.DaysShifted  = mod.DaysShifted;

            _db.ScheduleChanges.Add(change);
        }

        // Handle removals
        foreach (var rem in sim.Tasks.Where(t => t.IsRemoved))
        {
            int? workingTaskId = rem.SourceWorkingTaskId;   // Legacy path
            if (workingTaskId == null && rem.SourceSnapshotId.HasValue && snapshotToWorkingMap != null)
            {
                if (snapshotToWorkingMap.TryGetValue(rem.SourceSnapshotId.Value, out var mapped))
                    workingTaskId = mapped;
            }

            if (workingTaskId == null) continue;

            var working = await _db.WorkingTasks.FindAsync(workingTaskId.Value);
            if (working == null) continue;
            working.WorkingStatus = WorkingTaskStatus.Removed;
            working.UpdatedAt     = DateTime.UtcNow;

            _db.ScheduleChanges.Add(new ScheduleChange
            {
                RevisionId    = revisionId,
                WorkingTaskId = working.Id,
                ChangeType    = ChangeType.Removed,
                OldText       = working.Text,
                ChangeNote    = $"Removed via simulation: {sim.Name}. {rem.ChangeReason}",
                ChangedById   = userId,
                ChangedByName = userName,
                ChangedAt     = DateTime.UtcNow
            });
        }

        // Handle new tasks
        foreach (var newT in sim.Tasks.Where(t => t.IsNewTask))
        {
            var wt = new WorkingTask
            {
                ProjectId   = sim.ProjectId,
                Text        = newT.Text ?? "(New Task)",
                StartDate   = newT.StartDate ?? DateOnly.FromDateTime(DateTime.Today),
                EndDate     = newT.EndDate ?? DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                Duration    = newT.Duration ?? 1,
                Progress    = newT.Progress ?? 0,
                TradeId     = newT.TradeId,
                CrewSize    = newT.CrewSize ?? 0,
                Notes       = newT.Notes,
                CreatedAt   = DateTime.UtcNow,
                UpdatedAt   = DateTime.UtcNow
            };
            _db.WorkingTasks.Add(wt);
            await _db.SaveChangesAsync();

            _db.ScheduleChanges.Add(new ScheduleChange
            {
                RevisionId    = revisionId,
                WorkingTaskId = wt.Id,
                ChangeType    = ChangeType.Added,
                NewStartDate  = wt.StartDate,
                NewEndDate    = wt.EndDate,
                NewText       = wt.Text,
                ChangeNote    = $"Added via simulation: {sim.Name}. {newT.ChangeReason}",
                ChangedById   = userId,
                ChangedByName = userName,
                ChangedAt     = DateTime.UtcNow
            });
        }

        sim.Status    = SimulationStatus.Archived;
        sim.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // Cleanup orphaned auto snapshots
        await _baselineSvc.CleanupOrphanedSnapshotsAsync(sim.ProjectId);
    }

    // ── Impact Recalculation ─────────────────────────────────────────────────

    public async Task<ScheduleSimulation> RecalculateImpactAsync(int simulationId)
    {
        var result = await GetSimulationResultAsync(simulationId);
        var sim = await _db.ScheduleSimulations.FindAsync(simulationId)!;

        sim!.ModifiedTaskCount = result.Impact.ModifiedTasks
                               + result.Impact.NewTasks
                               + result.Impact.RemovedTasks;
        sim.TotalDaysImpact    = result.Impact.NetDaysShift;
        sim.SimulatedEndDate   = result.Impact.SimulatedEndDate;
        sim.UpdatedAt          = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return sim;
    }
}
