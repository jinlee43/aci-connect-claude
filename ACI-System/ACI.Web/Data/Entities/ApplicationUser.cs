using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

/// <summary>
/// Custom auth user (BCrypt). NOT ASP.NET Identity.
/// Linked to Employee record if the user is also a company employee.
///
/// <para>
/// <b>권한</b>: 구 <c>UserRole</c> enum 은 제거되고 <see cref="Privilege"/> 엔티티 +
/// <see cref="UserPrivilege"/> 조인 테이블로 이행했습니다 (many-to-many).
/// 로그인 시 <see cref="UserPrivileges"/> 를
/// <see cref="ACI.Web.Services.PrivilegeExpander"/> 로 확장한 뒤 각 코드를
/// <c>ClaimTypes.Role</c> 클레임으로 쿠키에 심어 <c>User.IsInRole(...)</c> 체크가
/// 상하위 관계를 자동 만족하게 합니다.
/// </para>
/// </summary>
public class ApplicationUser
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 회사 이메일 주소. 자동 생성: <c>Name@angelescontractor.com</c>.
    /// 직접 편집 금지 — Name 변경 시 서비스 레이어에서 함께 갱신.
    /// </summary>
    [Required, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    /// <summary>BCrypt.Net hashed password.</summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Optional: link to the employee record
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    /// <summary>
    /// 직접 부여된 권한(Privilege) 목록. 상속(implies) 관계는 로그인 시 코드로 expand.
    /// 관리 UI: <c>/Hr/Users</c>(HrAdmin).
    /// </summary>
    public ICollection<UserPrivilege> UserPrivileges { get; set; } = new List<UserPrivilege>();
}
