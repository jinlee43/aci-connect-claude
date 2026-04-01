using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

/// <summary>
/// Job title / position definition (e.g., Project Manager, Superintendent, Foreman).
/// </summary>
public class JobPosition : BaseEntity
{
    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Display ordering — lower = higher rank.</summary>
    public int OrdNum { get; set; } = 999;

    public ICollection<Employee> Employees { get; set; } = [];
}
