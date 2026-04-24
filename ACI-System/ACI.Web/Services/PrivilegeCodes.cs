namespace ACI.Web.Services;

/// <summary>
/// 시스템 빌트인 Privilege 코드 상수.
/// <para>
/// 이 값들은 <c>Program.cs</c> Authorization 정책 및 <c>[Authorize(Roles=...)]</c> /
/// <c>User.IsInRole(...)</c> 체크에서 쓰이는 문자열 키와 <b>정확히 일치</b>해야 합니다.
/// 타이포 방지 및 IDE refactor 용이를 위해 모두 상수로 노출.
/// </para>
///
/// <para>
/// DbInitializer 에서 이 코드들로 Privilege row 를 seed 합니다(IsBuiltIn=true).
/// 관리 UI 에서 Code 는 수정 불가(Name/Description/IsActive 만 편집 가능).
/// </para>
/// </summary>
public static class PrivilegeCodes
{
    // ── 최상위 시스템 관리자 ───────────────────────────────────────────────
    public const string Admin          = "Admin";

    // ── HR 계층 (Admin ⊇ HrAdmin ⊇ HrManager ⊇ HrUser) ────────────────────
    public const string HrAdmin        = "HrAdmin";
    public const string HrManager      = "HrManager";
    public const string HrUser         = "HrUser";

    // ── 프로젝트 관리자 (Admin ⊇ LsProjAdmin / JocProjAdmin) ──────────────
    public const string LsProjAdmin    = "LsProjAdmin";
    public const string JocProjAdmin   = "JocProjAdmin";

    // ── Safety 계층 (SafetyAdmin ⊇ SafetyManager ⊇ SafetyUser) ────────────
    public const string SafetyAdmin   = "SafetyAdmin";
    public const string SafetyManager = "SafetyManager";
    public const string SafetyUser    = "SafetyUser";

    // ── 일반 사용자 롤 ────────────────────────────────────────────────────
    public const string TradePartner   = "TradePartner";
    public const string Viewer         = "Viewer";

    // ── HR 파생 롤 (Privilege 테이블에 없음, 로그인 시 JobPosition으로 자동 결정) ──
    // Employee → EmpRole → JobPosition.Code 매핑:
    //   ProjectManager : PM / SPM / APM
    //   Superintendent : SUPT / SSUPT / ASUPT
    public const string ProjectManager = "ProjectManager";
    public const string Superintendent = "Superintendent";

    /// <summary>빌트인 priv 전체 목록(Seed 및 관리 UI 참조용).</summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        Admin,
        HrAdmin, HrManager, HrUser,
        LsProjAdmin, JocProjAdmin,
        SafetyAdmin, SafetyManager, SafetyUser,
        TradePartner, Viewer,
    };

    /// <summary>
    /// 관리 UI(<c>/Hr/Users</c>)를 통해 직접 부여·해제할 수 없는 권한 집합.
    ///
    /// <para>
    /// <b>Admin</b> 은 시스템 내 유일한 최상위 권한입니다.
    /// <c>DbInitializer</c> 의 DB seed 단계에서 <c>admin@aci-la.com</c> 계정에만
    /// 한 번 할당되며, 이후 어떤 UI 경로로도 부여하거나 해제할 수 없습니다.
    /// 새 최상위 관리자가 필요한 경우에는 DB를 직접 수정하거나 별도의 마이그레이션
    /// 스크립트를 사용해야 합니다.
    /// </para>
    ///
    /// <para>
    /// <b>ProjectManager</b> 와 <b>Superintendent</b> 는 HR 파생 롤입니다.
    /// 직원의 <c>EmpRole → JobPosition.Code</c> 매핑(PM/SPM/APM, SUPT/SSUPT/ASUPT)으로
    /// 로그인 시 자동 부여되므로, 이 UI에서 수동으로 부여·해제할 수 없습니다.
    /// </para>
    ///
    /// <para>
    /// 이 집합에 포함된 코드는 <c>/Hr/Users</c> 의 권한 체크박스 목록에서 제외되고,
    /// <c>OnPostCreateAsync</c> / <c>OnPostUpdateAsync</c> 의 서버사이드 가드에서도
    /// 거부됩니다.
    /// </para>
    /// </summary>
    // List 사용 — HashSet은 IReadOnlySet도 구현하므로 EF Core가 번역 실패함
    public static readonly IReadOnlyList<string> UiNonAssignable =
        new List<string> { Admin, ProjectManager, Superintendent };
}
