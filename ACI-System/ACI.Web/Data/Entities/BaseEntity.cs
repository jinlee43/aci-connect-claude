namespace ACI.Web.Data.Entities;

/// <summary>
/// All entities inherit from this. Provides PK, soft-delete, and audit timestamps.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public int? CreatedById { get; set; }
    public int? UpdatedById { get; set; }
}
