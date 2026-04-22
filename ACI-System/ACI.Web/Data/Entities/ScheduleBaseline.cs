using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

public enum BaselineStatus
{
    Draft     = 0,   // 작성 중 (아직 Freeze 전)
    Frozen    = 1,   // Freeze 완료 — Owner 제출 대기
    Submitted = 2,   // Owner에게 제출됨
    Approved  = 3,   // Owner 승인 완료 (공식 Baseline)
    Rejected  = 4,   // Owner 반려
    Superseded = 5   // 후속 Baseline 승인으로 비활성화
}

/// <summary>
/// A frozen snapshot of the schedule at a point in time.
/// Each project can have multiple baselines (v1, v2, v3…).
/// Procore-style: Owner approval converts a frozen snapshot into an official baseline.
/// The "Current Plan" (WorkingTask table) is the live editable schedule.
/// </summary>
public class ScheduleBaseline : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>Sequential version number per project: 1, 2, 3…</summary>
    public int VersionNumber { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;   // e.g. "Baseline v1 – Original Contract Schedule"

    [MaxLength(2000)]
    public string? Description { get; set; }

    public BaselineStatus Status { get; set; } = BaselineStatus.Draft;

    // ── Freeze info ──────────────────────────────────────────────────────────
    /// <summary>When the snapshot was frozen (tasks copied).</summary>
    public DateTime? FrozenAt { get; set; }

    /// <summary>User who froze this baseline.</summary>
    public int? FrozenById { get; set; }
    public ApplicationUser? FrozenBy { get; set; }

    [MaxLength(150)]
    public string? FrozenByName { get; set; }

    // ── Owner approval ───────────────────────────────────────────────────────
    public DateTime? SubmittedAt { get; set; }
    public DateTime? ApprovedAt  { get; set; }

    [MaxLength(150)]
    public string? ApprovedByName { get; set; }   // External: Owner / CM / Architect

    [MaxLength(1000)]
    public string? ApprovalNotes { get; set; }

    // ── Data date ────────────────────────────────────────────────────────────
    /// <summary>Schedule data-date at the time of freeze.</summary>
    public DateOnly? DataDate { get; set; }

    // ── Auto Snapshot ────────────────────────────────────────────────────────
    /// <summary>
    /// True = auto-created snapshot when a What-If simulation is based on Current Plan.
    /// Auto snapshots are NOT official baselines — they don't appear in the baseline
    /// version list, don't go through Owner approval, and can be cleaned up when
    /// all referencing simulations are deleted.
    /// </summary>
    public bool IsAutoSnapshot { get; set; } = false;

    /// <summary>The simulation that triggered this auto snapshot (if any).</summary>
    public int? SourceSimulationId { get; set; }
    public ScheduleSimulation? SourceSimulation { get; set; }

    // ── Stats (denormalized for quick display) ───────────────────────────────
    public int TaskCount { get; set; }
    public DateOnly? EarliestStart { get; set; }
    public DateOnly? LatestFinish  { get; set; }
    public int? TotalCalendarDays  { get; set; }

    // ── Navigation ───────────────────────────────────────────────────────────
    public ICollection<BaselineTaskSnapshot> TaskSnapshots { get; set; } = [];

    // ── Computed ─────────────────────────────────────────────────────────────
    public string VersionLabel => $"Baseline v{VersionNumber}";
    public string StatusBadgeClass => Status switch
    {
        BaselineStatus.Draft      => "bg-secondary",
        BaselineStatus.Frozen     => "bg-info text-dark",
        BaselineStatus.Submitted  => "bg-warning text-dark",
        BaselineStatus.Approved   => "bg-success",
        BaselineStatus.Rejected   => "bg-danger",
        BaselineStatus.Superseded => "bg-dark",
        _                         => "bg-secondary"
    };

    public bool IsLocked => Status >= BaselineStatus.Frozen;
}
