using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

/// <summary>
/// Links an ExternalParty to a Project with an optional role description.
/// e.g., "ABC Corp is the Owner on Project A", "XYZ Inspection is Inspector on Project B"
/// </summary>
public class ProjectExternalParty : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int ExternalPartyId { get; set; }
    public ExternalParty ExternalParty { get; set; } = null!;

    /// <summary>Free-text role on this project (e.g., "Owner", "Inspector", "MEP Sub").</summary>
    [MaxLength(100)]
    public string? Role { get; set; }

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate   { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }
}
