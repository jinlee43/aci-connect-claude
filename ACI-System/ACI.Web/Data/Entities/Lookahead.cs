using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

public enum LookaheadStatus
{
    Draft     = 0,
    Published = 1,
    Archived  = 2
}

public enum LookaheadTaskStatus
{
    Planned    = 0,
    InProgress = 1,
    Completed  = 2,
    Delayed    = 3
}

public enum ConstraintCategory
{
    Design      = 0,   // 설계/도면
    Submittal   = 1,   // 제출물 승인
    Material    = 2,   // 자재 조달
    Labor       = 3,   // 인력
    Equipment   = 4,   // 장비
    Inspection  = 5,   // 검사/승인
    Coordination = 6,  // 공종 협의
    Owner       = 7,   // 발주처
    Weather     = 8,   // 날씨
    Other       = 99
}

/// <summary>
/// Lookahead schedule header (3/4/6-week window).
/// Each project can have multiple lookaheads over time.
/// </summary>
public class Lookahead : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;   // e.g. "Week 12 Lookahead"

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate   { get; set; }

    /// <summary>3, 4, or 6 weeks.</summary>
    public int WeeksCount { get; set; } = 3;

    public LookaheadStatus Status { get; set; } = LookaheadStatus.Draft;

    // Navigation
    public ICollection<LookaheadTask> Tasks { get; set; } = [];
}

/// <summary>
/// Individual task within a Lookahead window.
/// Optionally linked to a Master Schedule (ScheduleTask).
/// </summary>
public class LookaheadTask : BaseEntity
{
    public int LookaheadId { get; set; }
    public Lookahead Lookahead { get; set; } = null!;

    /// <summary>Optional link to master schedule task.</summary>
    public int? ScheduleTaskId { get; set; }
    public ScheduleTask? ScheduleTask { get; set; }

    public int? TradeId { get; set; }
    public Trade? Trade { get; set; }

    /// <summary>Assigned foreman / employee.</summary>
    public int? AssignedToId { get; set; }
    public Employee? AssignedTo { get; set; }

    [Required, MaxLength(300)]
    public string Text { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? Location { get; set; }

    public DateOnly StartDate { get; set; }
    public DateOnly EndDate   { get; set; }
    public int Duration       { get; set; } = 1;
    public double Progress    { get; set; } = 0;
    public int CrewSize       { get; set; } = 0;

    public LookaheadTaskStatus Status { get; set; } = LookaheadTaskStatus.Planned;

    // Constraint
    public bool HasConstraint          { get; set; } = false;
    public ConstraintCategory? ConstraintCategory { get; set; }

    [MaxLength(500)]
    public string? ConstraintNote      { get; set; }
    public bool IsConstraintResolved   { get; set; } = false;

    // Navigation
    public ICollection<WeeklyTask> WeeklyTasks { get; set; } = [];
}
