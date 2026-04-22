namespace ACI.Web.Services;

/// <summary>
/// Privilege 계층 확장 서비스.
/// <para>
/// 사용자가 직접 부여받은 Privilege 코드 집합을 입력받아,
/// 상위 priv 가 내포하는 하위 priv 를 전부 추가한 완전 집합을 반환합니다.
/// 예: {Admin} → {Admin, HrAdmin, HrManager, HrUser, LsProjAdmin, JocProjAdmin, ...}
/// </para>
///
/// <para>
/// 계층은 <b>DB 가 아닌 이 코드에 하드코딩</b> 됩니다(설계 선택).
/// 새 빌트인 priv 를 추가하거나 계층을 바꾸려면 <see cref="DirectImplies"/> 맵을 수정.
/// </para>
///
/// <para>
/// 사용 지점: 로그인 시 <c>Pages/Account/Login.cshtml.cs</c> 에서 확장 후 각 code 를
/// <c>ClaimTypes.Role</c> 클레임으로 쿠키에 심어, <c>User.IsInRole / RequireRole</c>
/// 체크가 상속 관계까지 자동으로 만족하도록 합니다.
/// </para>
/// </summary>
public static class PrivilegeExpander
{
    /// <summary>
    /// <b>직접(1단계) implies</b> 관계 맵. 재귀적 전개는 <see cref="Expand"/> 에서 처리.
    /// 키가 값 priv 들을 '포함한다' — 즉 Admin 사용자는 HrAdmin, LsProjAdmin, JocProjAdmin 도 가진 것으로 간주.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, string[]> DirectImplies =
        new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            // Admin 은 Login.cshtml.cs 에서 DB 전체 privilege 를 동적으로 주입하므로
            // 여기서는 별도 처리 불필요.

            // HR 3단 계층: HrAdmin ⊇ HrManager ⊇ HrUser
            [PrivilegeCodes.HrAdmin]   = new[] { PrivilegeCodes.HrManager },
            [PrivilegeCodes.HrManager] = new[] { PrivilegeCodes.HrUser },

            // Safety 3단 계층: SafetyAdmin ⊇ SafetyManager ⊇ SafetyUser
            [PrivilegeCodes.SafetyAdmin]   = new[] { PrivilegeCodes.SafetyManager },
            [PrivilegeCodes.SafetyManager] = new[] { PrivilegeCodes.SafetyUser },

            // 프로젝트 관리자는 하위 imply 없음 (나란히 존재)
            // 일반 롤(ProjectManager, Superintendent, TradePartner, Viewer)도 imply 없음
        };

    /// <summary>
    /// 사용자가 직접 부여받은 priv 코드들을 입력받아,
    /// 상속 관계를 전개한 최종 권한 집합을 반환합니다.
    /// 결과는 중복 제거되고 대소문자 구분.
    /// </summary>
    /// <param name="directlyGranted">DB 의 UserPrivilege 에서 로드한 Code 목록.</param>
    public static IReadOnlySet<string> Expand(IEnumerable<string> directlyGranted)
    {
        var result = new HashSet<string>(StringComparer.Ordinal);
        var stack  = new Stack<string>();

        foreach (var code in directlyGranted)
        {
            if (!string.IsNullOrWhiteSpace(code))
                stack.Push(code);
        }

        while (stack.Count > 0)
        {
            var code = stack.Pop();
            if (!result.Add(code)) continue;     // 이미 처리한 priv 는 스킵 (순환 방지)

            if (DirectImplies.TryGetValue(code, out var implied))
            {
                foreach (var sub in implied)
                    stack.Push(sub);
            }
        }

        return result;
    }
}
