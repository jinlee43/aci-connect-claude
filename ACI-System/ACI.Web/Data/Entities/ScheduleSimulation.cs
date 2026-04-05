using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Web.Data.Entities;

public enum SimulationStatus
{
    Active    = 0,   // 편집 가능
    Saved     = 1,   // 저장 완료 (편집 가능)
    Archived  = 2    // 아카이브 (읽기 전용)
}

public enum SimulationSourceType
{
    CurrentPlan = 0,   // Current Plan(WorkingTask) 기반
    Baseline    = 1    // 특정 Baseline 버전 기반
}

/// <summary>
/// What-If simulation scenario.
/// Created from Current Plan or a specific Baseline version.
/// Contains overridden task data — only stores modified fields (delta approach).
/// Users can create multiple simulations, compare them, and optionally promote to Current Plan.
/// </summary>
public class ScheduleSimulation : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;   // e.g. "Scenario A – Accelerated Foundation"

    [MaxLength(2000)]
    public string? Description { get; set; }

    public SimulationStatus Status { get; set; } = SimulationStatus.Active;

    // ── Source ────────────────────────────────────────────────────────────────
    public SimulationSourceType SourceType { get; set; } = SimulationSourceType.CurrentPlan;

    /// <summary>If based on a specific baseline version. Null = based on Current Plan.</summary>
    public int? SourceBaselineId { get; set; }
    public ScheduleBaseline? SourceBaseline { get; set; }

    // ── Creator ──────────────────────────────────────────────────────────────
    public int? CreatedByUserId { get; set; }
    public ApplicationUser? CreatedByUser { get; set; }

    [MaxLength(150)]
    public string? CreatedByName { get; set; }

    // ── Impact summary (denormalized for quick display) ──────────────────────
    public int ModifiedTaskCount { get; set; } = 0;
    public int? TotalDaysImpact  { get; set; }          // net shift in project end date
    public DateOnly? SimulatedEndDate { get; set; }      // projected completion date

    // ── Navigation ───────────────────────────────────────────────────────────
    public ICollection<SimulationTask> Tasks { get; set; } = [];

    // ── Computed ─────────────────────────────────────────────────────────────
    public string StatusBadgeClass => Status switch
    {
        SimulationStatus.Active   => "bg-primary",
        SimulationStatus.Saved    => "bg-success",
        SimulationStatus.Archived => "bg-secondary",
        _                         => "bg-secondary"
    };
}

/// <summary>
/// A task override within a simulation.
/// Stores only the fields that differ from the source (delta approach).
/// Null fields = use the original value from the source task.
/// </summary>
public class SimulationTask
{
    public int Id { get; set; }

    public int SimulationId { get; set; }
    public ScheduleSimulation Simulation { get; set; } = null!;

    // ── Source task reference ─────────────────────────────────────────────────
    /// <summary>
    /// If source is CurrentPlan → WorkingTask Id.
    /// If source is Baseline → BaselineTaskSnapshot Id.
    /// </summary>
    public int? SourceWorkingTaskId  { get; set; }
    public WorkingTask? SourceWorkingTask { get; set; }

    public int? SourceSnapshotId { get; set; }
    public BaselineTaskSnapshot? SourceSnapshot { get; set; }

    // ── Overridden fields (null = use source value) ──────────────────────────
    [MaxLength(300)]
    public string? Text { get; set; }

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate   { get; set; }
    public int?      Duration  { get; set; }
    public double?   Progress  { get; set; }

    public int?  TradeId       { get; set; }
    public int?  AssignedToId  { get; set; }
    public int?  CrewSize      { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // ── What changed ─────────────────────────────────────────────────────────
    /// <summary>Days shifted vs source. Positive = delayed, Negative = accelerated.</summary>
    public int? DaysShifted { get; set; }

    [MaxLength(500)]
    public string? ChangeReason { get; set; }   // Why this modification was made

    // ── New task flag ────────────────────────────────────────────────────────
    /// <summary>True if this task was added in the simulation (no source counterpart).</summary>
    public bool IsNewTask { get; set; } = false;

    /// <summary>True if this task is removed in the simulation scenario.</summary>
    public bool IsRemoved { get; set; } = false;

    // ── Computed ──────────────────────────────────────────────────────────────
    [NotMapped]
    public bool HasDateChange => StartDate.HasValue || EndDate.HasValue || Duration.HasValue;

    [NotMapped]
    public bool HasAnyChange => HasDateChange || Text != null || Progress.HasValue
                                || TradeId.HasValue || AssignedToId.HasValue
                                || CrewSize.HasValue || Notes != null || IsRemoved;
}
