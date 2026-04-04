using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Web.Data.Entities;

public enum WorkingTaskStatus
{
    Active    = 0,   // 정상 진행
    Removed   = 1,   // 공정 삭제 (Change Order 등으로 제거)
    Suspended = 2,   // 공정 보류 (임시 중단)
}

/// <summary>
/// Progress Schedule task — the "live" version of ScheduleTask (Baseline).
/// Created by forking the Baseline; updated as the project progresses.
/// All changes are tracked via ScheduleChange records within a ScheduleRevision.
/// UI label: "Progress Schedule"
/// </summary>
public class WorkingTask : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>
    /// Link to the original Baseline task. Null = task added after baseline (new scope).
    /// </summary>
    public int? BaselineTaskId { get; set; }
    public ScheduleTask? BaselineTask { get; set; }

    // ── WBS / Display ────────────────────────────────────────────────────────
    [MaxLength(30)]
    public string? WbsCode { get; set; }

    [Required, MaxLength(300)]
    public string Text { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public GanttTaskType TaskType { get; set; } = GanttTaskType.Task;

    // ── Hierarchy ────────────────────────────────────────────────────────────
    public int? ParentId { get; set; }
    public WorkingTask? Parent { get; set; }
    public ICollection<WorkingTask> Children { get; set; } = [];

    public int SortOrder { get; set; } = 0;
    public bool IsOpen    { get; set; } = true;

    // ── Trade / Assignment ───────────────────────────────────────────────────
    public int? TradeId { get; set; }
    public Trade? Trade { get; set; }

    public int? AssignedToId { get; set; }
    public Employee? AssignedTo { get; set; }

    // ── Current Scheduled Dates ──────────────────────────────────────────────
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate   { get; set; }
    public int      Duration  { get; set; } = 1;

    /// <summary>Progress 0.0–1.0</summary>
    public double Progress { get; set; } = 0;

    // ── Actual Dates ─────────────────────────────────────────────────────────
    public DateOnly? ActualStartDate { get; set; }
    public DateOnly? ActualEndDate   { get; set; }
    public DateOnly? CompletedDate   { get; set; }
    public bool      IsDone          { get; set; } = false;

    // ── Constraint ───────────────────────────────────────────────────────────
    public TaskConstraintType? ConstraintType { get; set; }
    public DateOnly?           ConstraintDate { get; set; }

    // ── Display ───────────────────────────────────────────────────────────────
    [MaxLength(10)]
    public string? Color    { get; set; }
    public int     CrewSize { get; set; } = 0;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // ── Status ───────────────────────────────────────────────────────────────
    public WorkingTaskStatus WorkingStatus { get; set; } = WorkingTaskStatus.Active;

    // ── Navigation ───────────────────────────────────────────────────────────
    public ICollection<ScheduleChange> Changes { get; set; } = [];

    // ── Computed ─────────────────────────────────────────────────────────────
    [NotMapped]
    public string GanttTypeString => TaskType switch
    {
        GanttTaskType.Project   => "project",
        GanttTaskType.Milestone => "milestone",
        _                       => "task"
    };

    [NotMapped]
    public int ProgressPercent => (int)(Progress * 100);

    /// <summary>Days delayed vs Baseline (positive = late, negative = ahead).</summary>
    [NotMapped]
    public int? DaysDelayed => BaselineTask is null
        ? null
        : EndDate.DayNumber - BaselineTask.EndDate.DayNumber;
}
