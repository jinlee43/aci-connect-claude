using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Web.Data.Entities;

public enum EmployeeGender { Male, Female, Other }

/// <summary>
/// Company employee record. Separate from ApplicationUser (login account).
/// An employee may or may not have a login account.
/// Department/position membership is managed via EmpRole.
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

    /// <summary>Name suffix (Jr., Sr., III, etc.).</summary>
    [MaxLength(20)]
    public string? NameSuffix { get; set; }

    /// <summary>Preferred/known name used in UI.</summary>
    [MaxLength(50)]
    public string? KnownName { get; set; }

    public EmployeeGender? Gender { get; set; }
    public DateOnly? BirthDate { get; set; }

    // ── Work Contact ──────────────────────────────────────────────────────────
    /// <summary>Company-issued cell phone.</summary>
    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(200)]
    public string? WorkEmail { get; set; }

    // ── Private Contact ───────────────────────────────────────────────────────
    [MaxLength(20)]
    public string? PrivHomePhone { get; set; }

    /// <summary>Personal cell phone (separate from company phone).</summary>
    [MaxLength(20)]
    public string? PrivCellPhone { get; set; }

    [MaxLength(200)]
    public string? PersonalEmail { get; set; }

    // ── Home Address ──────────────────────────────────────────────────────────
    [MaxLength(200)]
    public string? HomeAddress1 { get; set; }

    [MaxLength(200)]
    public string? HomeAddress2 { get; set; }

    [MaxLength(100)]
    public string? HomeAddressCity { get; set; }

    [MaxLength(50)]
    public string? HomeAddressState { get; set; }

    [MaxLength(20)]
    public string? HomeAddressZip { get; set; }

    [MaxLength(100)]
    public string? HomeAddressCounty { get; set; }

    // ── Emergency Contact #1 ──────────────────────────────────────────────────
    [MaxLength(100)]
    public string? EmgContact1Name { get; set; }

    [MaxLength(50)]
    public string? EmgContact1Relation { get; set; }

    [MaxLength(200)]
    public string? EmgContact1Email { get; set; }

    [MaxLength(20)]
    public string? EmgContact1Tel { get; set; }

    [MaxLength(20)]
    public string? EmgContact1Cell { get; set; }

    // ── Emergency Contact #2 ──────────────────────────────────────────────────
    [MaxLength(100)]
    public string? EmgContact2Name { get; set; }

    [MaxLength(50)]
    public string? EmgContact2Relation { get; set; }

    [MaxLength(200)]
    public string? EmgContact2Email { get; set; }

    [MaxLength(20)]
    public string? EmgContact2Tel { get; set; }

    [MaxLength(20)]
    public string? EmgContact2Cell { get; set; }

    // ── Emergency Contact #3 ──────────────────────────────────────────────────
    [MaxLength(100)]
    public string? EmgContact3Name { get; set; }

    [MaxLength(50)]
    public string? EmgContact3Relation { get; set; }

    [MaxLength(200)]
    public string? EmgContact3Email { get; set; }

    [MaxLength(20)]
    public string? EmgContact3Tel { get; set; }

    [MaxLength(20)]
    public string? EmgContact3Cell { get; set; }

    // ── Employment ────────────────────────────────────────────────────────────
    public DateOnly? HireDate { get; set; }
    public DateOnly? TerminationDate { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    // ── Employment Admin (HrAdmin only) ───────────────────────────────────────
    public DateOnly? ApplyDate { get; set; }
    public bool IsReEmp { get; set; } = false;
    public DateOnly? OldStartDate { get; set; }

    /// <summary>Resignation | LaidOff</summary>
    [MaxLength(50)]
    public string? TerminationType { get; set; }

    public DateOnly? BkgrndCheckDate { get; set; }
    public bool BackgroudCheckOk { get; set; } = false;
    public DateOnly? DrugScreeningDate { get; set; }
    public bool DrugScreeningOk { get; set; } = false;

    // ── Benefits (HrAdmin only) ───────────────────────────────────────────────
    public DateOnly? HealthStartDate { get; set; }
    public DateOnly? HealthEndDate { get; set; }
    public DateOnly? DentalStartDate { get; set; }
    public DateOnly? DentalEndDate { get; set; }
    public DateOnly? VisionStartDate { get; set; }
    public DateOnly? VisionEndDate { get; set; }
    public DateOnly? Eligible401kDate { get; set; }
    public DateOnly? Enrolled401kDate { get; set; }

    // ── Legal / ID (HrAdmin only, sensitive fields AES-256 encrypted) ─────────
    /// <summary>Citizenship status (e.g. "A citizen of the U.S.")</summary>
    [MaxLength(100)]
    public string? StatusName { get; set; }

    /// <summary>SSN — AES-256-GCM encrypted ciphertext.</summary>
    public string? SsnEncrypted { get; set; }

    /// <summary>Tax ID Number — AES-256-GCM encrypted.</summary>
    public string? TinEncrypted { get; set; }

    /// <summary>Driver's License # — AES-256-GCM encrypted.</summary>
    public string? DriversLicNumEncrypted { get; set; }
    public DateOnly? DriversLicIssuedDate { get; set; }
    public DateOnly? DriversLicExpiration { get; set; }

    /// <summary>Alien (USCIS A-Number) — AES-256-GCM encrypted.</summary>
    public string? AlienNumberEncrypted { get; set; }
    public DateOnly? AlienCardIssuedDate { get; set; }
    public DateOnly? AlienCardExpirationDate { get; set; }

    /// <summary>Passport number — AES-256-GCM encrypted.</summary>
    public string? PassportNumberEncrypted { get; set; }
    public DateOnly? PassportIssedDate { get; set; }
    public DateOnly? PassportExpiredDate { get; set; }

    // ── Navigation ────────────────────────────────────────────────────────────
    public ApplicationUser? UserAccount { get; set; }
    public ICollection<EmpRole> EmpRoles { get; set; } = [];
    public ICollection<EmployeeDocument> Documents { get; set; } = [];

    // ── Computed helpers ──────────────────────────────────────────────────────
    [NotMapped]
    public string DisplayName => KnownName ?? $"{FirstName} {LastName}".Trim();

    [NotMapped]
    public string FullName => string.IsNullOrEmpty(MiddleName)
        ? $"{FirstName} {LastName}".Trim()
        : $"{FirstName} {MiddleName} {LastName}".Trim();

    /// <summary>Primary department role (IsPrimary = true).</summary>
    [NotMapped]
    public EmpRole? PrimaryRole => EmpRoles.FirstOrDefault(r => r.IsPrimary);
}
