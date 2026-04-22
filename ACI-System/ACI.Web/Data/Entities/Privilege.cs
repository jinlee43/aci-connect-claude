using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

/// <summary>
/// 시스템 권한 마스터. 구 <c>UserRole</c> enum 을 DB 엔티티로 전환한 것.
///
/// <para>
/// <b>Code</b> 는 <c>Program.cs</c> Authorization 정책 및 <c>User.IsInRole(...)</c> /
/// <c>[Authorize(Roles = "...")]</c> 체크에서 사용하는 문자열 키와 1:1 매핑됩니다.
/// 따라서 Code 를 수정하는 것은 사실상 API 변경 — 빌트인은 관리 UI 에서 수정 불가.
/// </para>
///
/// <para>
/// <b>상하위 관계</b>(예: Admin ⊇ HrAdmin ⊇ HrUser)는 DB 가 아닌
/// <see cref="ACI.Web.Services.PrivilegeExpander"/> 코드에 정의되어 있습니다.
/// 사용자는 상위 Privilege 하나만 할당받으면 로그인 시 하위 Role 클레임이 자동 생성됩니다.
/// </para>
/// </summary>
public class Privilege
{
    public int Id { get; set; }

    /// <summary>
    /// 정책/Role 체크에 쓰이는 고유 코드.
    /// 예: "Admin", "HrAdmin", "HrUser", "LsProjAdmin".
    /// 대소문자 민감. Program.cs 정책 문자열과 반드시 일치해야 함.
    /// </summary>
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    /// <summary>화면 표시용 이름 (영문). 예: "HR Admin".</summary>
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>해당 권한의 용도 설명.</summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 시스템 내장(Seed) 여부. true 이면 관리 UI 에서 Code 수정 및 삭제 불가.
    /// (Name/Description/IsActive 는 편집 가능)
    /// </summary>
    public bool IsBuiltIn { get; set; }

    /// <summary>비활성화된 priv 는 신규 할당 불가. 기존 할당은 로그인 시 스킵.</summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<UserPrivilege> UserPrivileges { get; set; } = new List<UserPrivilege>();
}
