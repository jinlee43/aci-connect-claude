using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Web.Data.Entities;

public enum DepartmentType
{
    Company    = 0,  // 회사 (최상위)
    Division   = 1,  // 사업부
    Department = 2,  // 부서
    Team       = 3,  // 팀
    Branch     = 4   // 지사/지점
}

/// <summary>
/// Company organizational hierarchy. Self-referencing tree.
/// </summary>
public class Department : BaseEntity
{
    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public DepartmentType Type { get; set; } = DepartmentType.Department;

    public int? ParentDeptId { get; set; }
    public Department? ParentDept { get; set; }

    public ICollection<Department> ChildDepts { get; set; } = [];
    public ICollection<Employee> Employees { get; set; } = [];

    [NotMapped]
    public int Level { get; set; } = 0;
}
