using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Web.Data.Entities;

public enum WeeklyPlanStatus
{
    Planning  = 0,   // 계획 수립 중
    Committed = 1,   // 커밋 완료 (주 시작)
    Reviewing = 2,   // 주말 결산 중
    Closed    = 3    // 마감 완료
}

public enum VarianceCategory
{
    Design        = 0,   // 설계 변경/오류
    Subcontractor = 1,   // 하도급 미이행
    Material      = 2,   // 자재 미도착
    Equipment     = 3,   // 장비 문제
    Labor         = 4,   // 인력 부족
    Weather       = 5,   // 기상
    Owner         = 6,   // 발주처 지시
    Inspection    = 7,   // 검사 지연
    Prerequisite  = 8,   // 선행 작업 미완료
    Other         = 99
}

/// <summary>
/// Weekly Work Plan header (Last Planner System).
/// One plan per project per week. WeekStartDate is always Monday.
/// </summary>
public class WeeklyWorkPlan : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>Monday of the plan week.</summary>
    public DateOnly WeekStartDate { get; set; }

    /// <summary>Friday of the plan week.</summary>
    public DateOnly WeekEndDate { get; set; }

    /// <summary>ISO week number (1-53).</summary>
    public int WeekNumber { get; set; }
    public int Year { get; set; }

    public WeeklyPlanStatus Status { get; set; } = WeeklyPlanStatus.Planning;

    // PPC (Percent Plan Complete) — updated when tasks are closed
    public int TotalTaskCount     { get; set; } = 0;
    public int CompletedTaskCount { get; set; } = 0;

    [NotMapped]
    public double PPC => TotalTaskCount > 0
        ? Math.Round((double)CompletedTaskCount / TotalTaskCount * 100, 1)
        : 0;

    // Navigation
    public ICollection<WeeklyTask> Tasks { get; set; } = [];
}

/// <summary>
/// Individual task within a Weekly Work Plan.
/// Optionally pulled from a LookaheadTask.
/// </summary>
public class WeeklyTask : BaseEntity
{
    public int WeeklyWorkPlanId { get; set; }
    public WeeklyWorkPlan WeeklyWorkPlan { get; set; } = null!;

    /// <summary>Optional: pulled from lookahead.</summary>
    public int? LookaheadTaskId { get; set; }
    public LookaheadTask? LookaheadTask { get; set; }

    public int? TradeId { get; set; }
    public Trade? Trade { get; set; }

    /// <summary>Assigned foreman / employee.</summary>
    public int? AssignedToId { get; set; }
    public Employee? AssignedTo { get; set; }

    [Required, MaxLength(300)]
    public string Text { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Location { get; set; }

    /// <summary>Target completion date within the week.</summary>
    public DateOnly PlannedDate { get; set; }
    public int CrewSize { get; set; } = 0;

    // Commit & Complete
    public bool IsCommitted { get; set; } = false;
    public bool IsCompleted { get; set; } = false;
    public DateOnly? CompletedDate { get; set; }

    // Reason for Variance (LPS metric)
    public VarianceCategory? VarianceCategory { get; set; }

    [MaxLength(500)]
    public string? VarianceNote { get; set; }
}
