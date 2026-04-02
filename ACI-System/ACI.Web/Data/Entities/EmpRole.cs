using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

/// <summary>
/// Employee role within an OrgUnit (department or project team).
/// Replaces both Employee.DepartmentId / JobPositionId and ProjectMember.
/// One employee can have multiple roles (e.g., Senior PM in Estimating Dept
/// AND Superintendent on Project A). IsPrimary marks the home department.
/// </summary>
public class EmpRole : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int OrgUnitId { get; set; }
    public OrgUnit OrgUnit { get; set; } = null!;

    /// <summary>Role/title within this OrgUnit (may differ from company title).</summary>
    public int? JobPositionId { get; set; }
    public JobPosition? JobPosition { get; set; }

    /// <summary>True = primary department (home dept). Only one per employee.</summary>
    public bool IsPrimary { get; set; } = false;

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate   { get; set; }

    [MaxLength(200)]
    public string? Notes { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public new bool IsActive => EndDate == null || EndDate >= DateOnly.FromDateTime(DateTime.Today);
}
