using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

/// <summary>
/// Per-project Weekly Safety Report configuration.
/// Defines the reporting period and default submission day.
/// Once approved by SafetyManager, becomes read-only until approval is revoked.
/// </summary>
public class SafetyWkRepSettings : BaseEntity
{
    public int     ProjectId { get; set; }
    public Project Project   { get; set; } = null!;

    /// <summary>First week's Monday — defines when weekly reporting begins.</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>Last week's Sunday — null means open-ended (project still active).</summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Day of week reports are typically submitted.
    /// Advisory only — not strictly enforced.
    /// </summary>
    public DayOfWeek DefaultSubmitDay { get; set; } = DayOfWeek.Friday;

    // ── Approval (SafetyManager locks settings after approval) ───────────────
    public bool      IsApproved     { get; set; } = false;
    public DateTime? ApprovedAt     { get; set; }
    public int?      ApprovedById   { get; set; }
    public ApplicationUser? ApprovedBy { get; set; }

    [MaxLength(150)]
    public string?   ApprovedByName { get; set; }

    /// <summary>
    /// Incremented each time approval is revoked and settings are re-approved.
    /// Provides a lightweight audit trail without a separate history table.
    /// </summary>
    public int RevisionNumber { get; set; } = 0;

    [MaxLength(500)]
    public string? Notes { get; set; }
}
