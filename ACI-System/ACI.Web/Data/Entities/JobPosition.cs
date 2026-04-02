using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

/// <summary>
/// Job title / position definition.
/// e.g., CEO, Senior VP, VP, Senior PM, PM, Assistant PM, PE, Assistant PE,
///       Senior Superintendent, Superintendent, Assistant Superintendent, etc.
/// Used for both company org titles (Employee's home position) and
/// project roles (EmpRole.JobPositionId).
/// </summary>
public class JobPosition : BaseEntity
{
    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Display ordering — lower = higher rank (CEO = 1, Asst. Super = 999).</summary>
    public int OrdNum { get; set; } = 999;

    public ICollection<EmpRole> EmpRoles { get; set; } = [];
}
