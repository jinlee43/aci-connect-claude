using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

public enum UserRole
{
    Admin          = 0,  // 시스템 관리자
    ProjectManager = 1,  // PM - 전체 편집권
    Superintendent = 2,  // 현장소장 - 룩어헤드/주간계획 편집
    SafetyOfficer  = 3,  // 안전관리자
    TradePartner   = 4,  // 하도급 - 본인 작업만
    Viewer         = 5   // 읽기 전용
}

/// <summary>
/// Custom auth user (BCrypt). NOT ASP.NET Identity.
/// Linked to Employee record if the user is also a company employee.
/// </summary>
public class ApplicationUser
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt.Net hashed password.</summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Viewer;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Optional: link to the employee record
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public string RoleDisplayName => Role switch
    {
        UserRole.Admin          => "Admin",
        UserRole.ProjectManager => "Project Manager",
        UserRole.Superintendent => "Superintendent",
        UserRole.SafetyOfficer  => "Safety Officer",
        UserRole.TradePartner   => "Trade Partner",
        UserRole.Viewer         => "Viewer",
        _                       => Role.ToString()
    };
}
