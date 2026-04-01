using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Web.Data.Entities;

public enum ProjectRole
{
    ProjectManager  = 0,   // PM
    Superintendent  = 1,   // 현장소장
    LeadForeman     = 2,   // 현장 책임자
    SafetyOfficer   = 3,   // 안전관리자
    TradePartner    = 4,   // 하도급
    Owner           = 5,   // 발주처 담당자
    Inspector       = 6,   // 감리/검사
    Viewer          = 7    // 읽기 전용
}

/// <summary>
/// Project team member. Links Employee to Project with a role.
/// Also links ApplicationUser for access control.
/// </summary>
public class ProjectMember : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>The employee assigned to this project role.</summary>
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    /// <summary>Login user account (may differ from Employee in edge cases).</summary>
    public int? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ProjectRole Role { get; set; } = ProjectRole.Viewer;

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate   { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }

    [NotMapped]
    public string RoleLabel => Role switch
    {
        ProjectRole.ProjectManager => "Project Manager",
        ProjectRole.Superintendent => "Superintendent",
        ProjectRole.LeadForeman    => "Lead Foreman",
        ProjectRole.SafetyOfficer  => "Safety Officer",
        ProjectRole.TradePartner   => "Trade Partner",
        ProjectRole.Owner          => "Owner",
        ProjectRole.Inspector      => "Inspector",
        ProjectRole.Viewer         => "Viewer",
        _                          => Role.ToString()
    };
}
