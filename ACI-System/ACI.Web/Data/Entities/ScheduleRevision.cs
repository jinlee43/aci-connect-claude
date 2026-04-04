using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

public enum RevisionType
{
    Initial          = 0,   // 최초 Baseline → Progress Schedule 전환
    MonthlyUpdate    = 1,   // 월별 정기 업데이트
    ChangeOrder      = 2,   // Change Order 반영
    OwnerDirected    = 3,   // 발주처 지시
    RecoverySchedule = 4,   // 공기 회복 계획
    Acceleration     = 5,   // 공사 단축
    WeatherDelay     = 6,   // 기후 지연
    Other            = 99
}

public enum RevisionStatus
{
    Draft     = 0,   // 작성 중
    Submitted = 1,   // 발주처/감리 제출
    Approved  = 2,   // 승인 완료
    Rejected  = 3    // 반려
}

/// <summary>
/// A named batch of schedule changes submitted for approval.
/// Analogous to a Schedule Update submittal in US construction contracts.
/// UI label: "Progress Schedule Revision"
/// </summary>
public class ScheduleRevision : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>Sequential revision number per project: 0 (Initial), 1, 2, …</summary>
    public int RevisionNumber { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;   // e.g. "Rev 3 – March Update"

    [MaxLength(2000)]
    public string? Description { get; set; }

    public RevisionType   RevisionType { get; set; } = RevisionType.MonthlyUpdate;
    public RevisionStatus Status       { get; set; } = RevisionStatus.Draft;

    /// <summary>Change Order number if applicable (e.g. "CO-007").</summary>
    [MaxLength(50)]
    public string? ChangeOrderRef { get; set; }

    // ── Dates ────────────────────────────────────────────────────────────────
    public DateOnly? DataDate    { get; set; }   // Schedule data date (as-of date)
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt  { get; set; }

    /// <summary>Name of external approver (Owner / CM / Architect).</summary>
    [MaxLength(150)]
    public string? ApprovedByName { get; set; }

    [MaxLength(500)]
    public string? ApprovalNotes { get; set; }

    // ── Who created this revision ─────────────────────────────────────────────
    /// <summary>Internal user who created / submitted this revision.</summary>
    public int? SubmittedById { get; set; }
    public ApplicationUser? SubmittedBy { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public ICollection<ScheduleChange>   Changes   { get; set; } = [];
    public ICollection<RevisionDocument> Documents { get; set; } = [];

    // ── Computed ─────────────────────────────────────────────────────────────
    public string RevisionLabel => $"Rev {RevisionNumber}";
    public string StatusBadgeClass => Status switch
    {
        RevisionStatus.Draft     => "bg-secondary",
        RevisionStatus.Submitted => "bg-warning text-dark",
        RevisionStatus.Approved  => "bg-success",
        RevisionStatus.Rejected  => "bg-danger",
        _                        => "bg-secondary"
    };
}
