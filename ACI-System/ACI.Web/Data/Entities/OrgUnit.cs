using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Web.Data.Entities;

public enum OrgUnitType
{
    Company     = 0,  // 회사 (최상위)
    Division    = 1,  // 사업부
    Department  = 2,  // 부서
    Team        = 3,  // 팀
    Branch      = 4,  // 지사/지점
    ProjectTeam = 5   // 프로젝트 팀 (Project 연결)
}

/// <summary>
/// Unified organizational unit — covers both company org chart and project teams.
/// Self-referencing tree. When Type = ProjectTeam, ProjectId is set.
/// </summary>
public class OrgUnit : BaseEntity
{
    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public OrgUnitType Type { get; set; } = OrgUnitType.Department;

    // Hierarchy
    public int? ParentId { get; set; }
    public OrgUnit? Parent { get; set; }
    public ICollection<OrgUnit> Children { get; set; } = [];

    // Only set when Type = ProjectTeam
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    // Members in this unit
    public ICollection<EmpRole> EmpRoles { get; set; } = [];

    [NotMapped]
    public int Level { get; set; } = 0;

    [NotMapped]
    public string TypeLabel => Type switch
    {
        OrgUnitType.Company     => "Company",
        OrgUnitType.Division    => "Division",
        OrgUnitType.Department  => "Department",
        OrgUnitType.Team        => "Team",
        OrgUnitType.Branch      => "Branch",
        OrgUnitType.ProjectTeam => "Project Team",
        _                       => Type.ToString()
    };
}
