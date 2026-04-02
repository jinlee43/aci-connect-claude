using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

public enum ExternalPartyType
{
    Customer      = 0,  // 발주처 (Owner)
    Partner       = 1,  // 파트너 / 감리 (Inspector, Consultant)
    Subcontractor = 2   // 하도급 (Trade Partner)
}

/// <summary>
/// External company or individual participating in projects.
/// Covers Customers (owners), Partners (inspectors/consultants), and Subcontractors.
/// </summary>
public class ExternalParty : BaseEntity
{
    public ExternalPartyType Type { get; set; } = ExternalPartyType.Customer;

    [Required, MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContactName { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public ICollection<ProjectExternalParty> ProjectParticipations { get; set; } = [];

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string TypeLabel => Type switch
    {
        ExternalPartyType.Customer      => "Customer",
        ExternalPartyType.Partner       => "Partner",
        ExternalPartyType.Subcontractor => "Subcontractor",
        _                               => Type.ToString()
    };

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string DisplayName => string.IsNullOrEmpty(ContactName)
        ? CompanyName
        : $"{ContactName} ({CompanyName})";
}
