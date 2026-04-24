namespace ACI.Web.Services;

/// <summary>
/// 로그인 시 쿠키에 심는 커스텀 클레임 이름 상수.
/// <para>
/// 표준 ClaimTypes (NameIdentifier, Name, Email, Role) 외에
/// ACI 시스템 전용으로 추가하는 클레임들.
/// </para>
/// </summary>
public static class ClaimNames
{
    /// <summary>ApplicationUser.Id (int → string)</summary>
    public const string UserId = "UserId";

    /// <summary>Employee.Id (int → string). EmployeeId가 없는 사용자(외부 계정 등)에는 없을 수 있음.</summary>
    public const string EmployeeId = "EmployeeId";

    /// <summary>
    /// 활성 EmpRole의 OrgUnit.Id (int → string, 다중 클레임).
    /// 프로젝트 팀 OrgUnit(ProjectId != null)뿐 아니라 부서/사업부도 포함.
    /// </summary>
    public const string OrgUnitId = "OrgUnitId";

    /// <summary>
    /// 활성 EmpRole의 JobPosition.Code (다중 클레임).
    /// 예: "PM", "SPM", "SUPT", "SSUPT" 등.
    /// HR 파생 롤(ProjectManager / Superintendent) 결정에도 사용됨.
    /// </summary>
    public const string JobPositionCode = "JobPositionCode";
}
