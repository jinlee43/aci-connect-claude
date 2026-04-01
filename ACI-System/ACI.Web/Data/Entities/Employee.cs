using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Web.Data.Entities;

public enum EmployeeGender { Male, Female, Other }

/// <summary>
/// Company employee record. Separate from ApplicationUser (login account).
/// An employee may or may not have a login account.
/// </summary>
public class Employee : BaseEntity
{
    /// <summary>Auto-assigned sequential employee number (e.g., 1001).</summary>
    public int EmpNum { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? MiddleName { get; set; }

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    /// <summary>Preferred/known name used in UI.</summary>
    [MaxLength(50)]
    public string? KnownName { get; set; }

    public EmployeeGender? Gender { get; set; }
    public DateOnly? BirthDate { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? PersonalEmail { get; set; }

    [MaxLength(200)]
    public string? WorkEmail { get; set; }

    // Current primary job position
    public int? JobPositionId { get; set; }
    public JobPosition? JobPosition { get; set; }

    // Current primary department
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }

    public DateOnly? HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // Navigation
    public ApplicationUser? UserAccount { get; set; }
    public ICollection<ProjectMember> ProjectMemberships { get; set; } = [];

    [NotMapped]
    public string DisplayName => KnownName ?? $"{FirstName} {LastName}".Trim();

    [NotMapped]
    public string FullName => string.IsNullOrEmpty(MiddleName)
        ? $"{FirstName} {LastName}".Trim()
        : $"{FirstName} {MiddleName} {LastName}".Trim();
}
