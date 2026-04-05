using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Web.Data.Entities;

/// <summary>
/// Immutable snapshot of a task at the time a baseline was frozen.
/// One row per task per baseline version.
/// Once created, these rows are NEVER modified — they are the permanent record.
/// </summary>
public class BaselineTaskSnapshot
{
    public int Id { get; set; }

    public int BaselineId { get; set; }
    public ScheduleBaseline Baseline { get; set; } = null!;

    /// <summary>
    /// Reference to the source task at freeze time.
    /// Points to ScheduleTask (for v1 from initial baseline) or WorkingTask (for subsequent versions).
    /// Null if the source task was later deleted.
    /// </summary>
    public int? SourceScheduleTaskId { get; set; }
    public ScheduleTask? SourceScheduleTask { get; set; }

    public int? SourceWorkingTaskId { get; set; }
    public WorkingTask? SourceWorkingTask { get; set; }

    // ── Frozen task data ─────────────────────────────────────────────────────
    [MaxLength(30)]
    public string? WbsCode { get; set; }

    [Required, MaxLength(300)]
    public string Text { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public GanttTaskType TaskType { get; set; } = GanttTaskType.Task;

    // ── Hierarchy (snapshot-internal parent, not FK to live data) ─────────────
    /// <summary>Parent snapshot Id within the same baseline. Null = root.</summary>
    public int? ParentSnapshotId { get; set; }
    public BaselineTaskSnapshot? ParentSnapshot { get; set; }
    public ICollection<BaselineTaskSnapshot> ChildSnapshots { get; set; } = [];

    public int SortOrder { get; set; } = 0;
    public bool IsOpen { get; set; } = true;

    // ── Trade / Assignment (denormalized names for historical accuracy) ───────
    public int? TradeId { get; set; }
    [MaxLength(100)]
    public string? TradeName { get; set; }
    [MaxLength(10)]
    public string? TradeColor { get; set; }

    public int? AssignedToId { get; set; }
    [MaxLength(150)]
    public string? AssignedToName { get; set; }

    // ── Dates ────────────────────────────────────────────────────────────────
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate   { get; set; }
    public int      Duration  { get; set; } = 1;
    public double   Progress  { get; set; } = 0;

    public DateOnly? ActualStartDate { get; set; }
    public DateOnly? ActualEndDate   { get; set; }

    // ── Constraint ───────────────────────────────────────────────────────────
    public TaskConstraintType? ConstraintType { get; set; }
    public DateOnly?           ConstraintDate { get; set; }

    // ── Display ──────────────────────────────────────────────────────────────
    [MaxLength(10)]
    public string? Color { get; set; }
    public int CrewSize { get; set; } = 0;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // ── Computed ──────────────────────────────────────────────────────────────
    [NotMapped]
    public string GanttTypeString => TaskType switch
    {
        GanttTaskType.Project   => "project",
        GanttTaskType.Milestone => "milestone",
        _                       => "task"
    };

    [NotMapped]
    public int ProgressPercent => (int)(Progress * 100);
}
