namespace ACI.Web.Data.Entities;

/// <summary>
/// User ↔ Privilege 조인 엔티티 (many-to-many).
/// Composite PK: (UserId, PrivilegeId). <see cref="AppDbContext.OnModelCreating"/> 참고.
/// </summary>
public class UserPrivilege
{
    public int UserId      { get; set; }
    public int PrivilegeId { get; set; }

    /// <summary>권한 부여 일시 (감사용).</summary>
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    /// <summary>권한을 부여한 사용자 ID (감사용, 선택).</summary>
    public int? GrantedByUserId { get; set; }

    // Navigation
    public ApplicationUser? User       { get; set; }
    public Privilege?       Privilege  { get; set; }
    public ApplicationUser? GrantedBy  { get; set; }
}
