using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

public enum SafetyWkRepStatus
{
    Draft           = 0,   // Uploaded, pending review
    Reviewed        = 1,   // Reviewed by PM
    Approved        = 2,   // Approved by SafetyManager — locked
    NoWork          = 3,   // No work this week (pending review)
    Voided          = 4,   // Approval revoked by SafetyManager/SafetyAdmin
    NoWorkReviewed  = 5,   // No work — reviewed by PM
    NoWorkApproved  = 6,   // No work — approved by SafetyManager — locked
}

/// <summary>
/// Weekly Safety Report submitted per project per week.
/// One record per (ProjectId, WeekStartDate) — enforced by unique index.
///
/// <para>Workflow: Draft → Reviewed (PM) → Approved (SafetyManager) → [Voided]</para>
/// <para>NoWork is a terminal state indicating no activity that week.</para>
/// <para>Once Approved, the record is immutable until Voided.</para>
/// </summary>
public class SafetyWkRep : BaseEntity
{
    public int     ProjectId { get; set; }
    public Project Project   { get; set; } = null!;

    // ── Week identification ──────────────────────────────────────────────────
    /// <summary>Monday of the report week (natural key alongside ProjectId).</summary>
    public DateOnly WeekStartDate { get; set; }

    /// <summary>Sunday of the report week (WeekStartDate + 6).</summary>
    public DateOnly WeekEndDate   { get; set; }

    public int WeekNumber { get; set; }
    public int Year       { get; set; }

    public SafetyWkRepStatus Status { get; set; } = SafetyWkRepStatus.Draft;

    // ── File info (null when Status == NoWork) ───────────────────────────────
    [MaxLength(260)]
    public string? FileName       { get; set; }   // Original file name shown to users

    [MaxLength(100)]
    public string? StoredFileName { get; set; }   // GUID-based name on disk

    [MaxLength(20)]
    public string? Extension      { get; set; }

    public long FileSize { get; set; } = 0;

    // ── Report date ──────────────────────────────────────────────────────────
    /// <summary>
    /// 보고서 작성일 (해당 주의 보고 요일).
    /// 기본값 = Settings.DefaultSubmitDay 기준으로 계산된 날짜.
    /// </summary>
    public DateOnly? ReportDate { get; set; }

    // ── Upload ───────────────────────────────────────────────────────────────
    public int?  UploadedById   { get; set; }
    public ApplicationUser? UploadedBy { get; set; }

    [MaxLength(150)]
    public string?   UploadedByName { get; set; }
    public DateTime? UploadedAt    { get; set; }

    // ── Review (PM) ──────────────────────────────────────────────────────────
    public int?  ReviewedById   { get; set; }
    public ApplicationUser? ReviewedBy { get; set; }

    [MaxLength(150)]
    public string?   ReviewedByName { get; set; }
    public DateTime? ReviewedAt    { get; set; }

    [MaxLength(500)]
    public string? ReviewNotes { get; set; }

    // ── Approval (SafetyManager) ─────────────────────────────────────────────
    public int?  ApprovedById   { get; set; }
    public ApplicationUser? ApprovedBy { get; set; }

    [MaxLength(150)]
    public string?   ApprovedByName { get; set; }
    public DateTime? ApprovedAt    { get; set; }

    [MaxLength(500)]
    public string? ApprovalNotes { get; set; }

    // ── Void (SafetyManager / SafetyAdmin revokes approval) ─────────────────
    public int?  VoidedById   { get; set; }
    public ApplicationUser? VoidedBy { get; set; }

    [MaxLength(150)]
    public string?   VoidedByName { get; set; }
    public DateTime? VoidedAt    { get; set; }

    [MaxLength(500)]
    public string? VoidReason { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // ── Computed helpers ─────────────────────────────────────────────────────
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool IsLocked => Status is SafetyWkRepStatus.Approved or SafetyWkRepStatus.NoWorkApproved;
    public bool IsNoWork => Status is SafetyWkRepStatus.NoWork
                                   or SafetyWkRepStatus.NoWorkReviewed
                                   or SafetyWkRepStatus.NoWorkApproved;

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool HasFile => !string.IsNullOrEmpty(StoredFileName);
}
