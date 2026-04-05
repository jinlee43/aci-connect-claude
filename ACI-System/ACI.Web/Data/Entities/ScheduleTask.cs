using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Web.Data.Entities;

public enum GanttTaskType
{
    Task      = 0,   // 일반 작업 바
    Project   = 1,   // WBS 요약 바 (자식 있음)
    Milestone = 2    // 마일스톤 (다이아몬드)
}

public enum TaskConstraintType
{
    StartNoEarlierThan = 0,   // SNET
    FinishNoLaterThan  = 1,   // FNLT
    MustStartOn        = 2,   // MSO
    MustFinishOn       = 3    // MFO
}

/// <summary>
/// Baseline Schedule WBS item. Maps directly to dhtmlxGantt's task object.
/// Equivalent to old system's AciProjItem.
/// </summary>
public class ScheduleTask : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>WBS code, e.g. "1.2.3" — user-defined or auto-generated.</summary>
    [MaxLength(30)]
    public string? WbsCode { get; set; }

    [Required, MaxLength(300)]
    public string Text { get; set; } = string.Empty;   // dhtmlxGantt "text" field

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }   // Work zone / area on site

    public GanttTaskType TaskType { get; set; } = GanttTaskType.Task;

    // WBS Hierarchy
    public int? ParentId { get; set; }
    public ScheduleTask? Parent { get; set; }
    public ICollection<ScheduleTask> Children { get; set; } = [];

    /// <summary>Sort order within the same parent level.</summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>Whether WBS node is expanded in Gantt.</summary>
    public bool IsOpen { get; set; } = true;

    // Trade / Subcontractor
    public int? TradeId { get; set; }
    public Trade? Trade { get; set; }

    // Assigned employee (Foreman / responsible person)
    public int? AssignedToId { get; set; }
    public Employee? AssignedTo { get; set; }

    // ── Scheduled dates ──────────────────────────────────────────────────────
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate   { get; set; }

    /// <summary>Calendar days (dhtmlxGantt calculates from start+end).</summary>
    public int Duration { get; set; } = 1;

    /// <summary>Progress 0.0 – 1.0 (e.g. 0.75 = 75%)</summary>
    public double Progress { get; set; } = 0;

    // ── Baseline (original plan, set at project kickoff) ─────────────────────
    public DateOnly? BaselineStart { get; set; }
    public DateOnly? BaselineEnd   { get; set; }

    // ── Actual dates ──────────────────────────────────────────────────────────
    public DateOnly? ActualStartDate { get; set; }
    public DateOnly? ActualEndDate   { get; set; }
    public DateOnly? CompletedDate   { get; set; }

    public bool IsDone { get; set; } = false;

    // ── Constraint ───────────────────────────────────────────────────────────
    public TaskConstraintType? ConstraintType { get; set; }
    public DateOnly? ConstraintDate { get; set; }

    // ── Display ───────────────────────────────────────────────────────────────
    /// <summary>Color override (hex). Falls back to trade color.</summary>
    [MaxLength(10)]
    public string? Color { get; set; }

    /// <summary>Planned crew size.</summary>
    public int CrewSize { get; set; } = 0;

    [MaxLength(1000)]
    public string? Notes { get; set; }

    // ── Dependencies ─────────────────────────────────────────────────────────
    public ICollection<TaskDependency> Predecessors { get; set; } = [];   // target = this
    public ICollection<TaskDependency> Successors   { get; set; } = [];   // source = this

    // ── Computed helpers ─────────────────────────────────────────────────────
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
