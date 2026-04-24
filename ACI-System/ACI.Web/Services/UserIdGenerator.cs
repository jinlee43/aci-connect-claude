using ACI.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Services;

/// <summary>
/// 신규입사자용 기본 로그인 아이디 생성기.
/// 규칙: <c>lowercase(LastName + FirstName[0])</c>
///   - 예) Jin Lee  → "leej"
///   - 예) Sarah Kim → "kims"
/// 같은 아이디가 이미 쓰이고 있으면 뒤에 소문자 a,b,c,...를 덧붙임
///   - "leej" 사용 중 → "leeja" → "leejb" → ... → "leejz"
///   - 그 다음 → "leejaa", "leejab", ...  (2글자 확장)
/// 중복 체크는 <see cref="Data.Entities.ApplicationUser.Name"/> 필드를 기준으로 수행.
/// 반환값은 곧 <c>ApplicationUser.Name</c> 에 저장되고, Email 은 <c>Name@angelescontractor.com</c> 으로 자동 조립.
/// </summary>
public interface IUserIdGenerator
{
    Task<string> GenerateDefaultUserIdAsync(string firstName, string lastName, CancellationToken ct = default);
}

public class UserIdGenerator : IUserIdGenerator
{
    private readonly AppDbContext _db;

    public UserIdGenerator(AppDbContext db) => _db = db;

    public async Task<string> GenerateDefaultUserIdAsync(
        string firstName,
        string lastName,
        CancellationToken ct = default)
    {
        var baseId = BuildBase(firstName, lastName);
        if (string.IsNullOrEmpty(baseId))
            throw new ArgumentException("FirstName and LastName must contain at least one usable ASCII letter or digit.");

        // 동일 prefix로 시작하는 기존 user ID들을 한 번에 조회하여
        // 메모리에서 빠르게 충돌 판정.
        //   ex) baseId="leej" → "leej", "leeja", "leejb", ..., "leejzz" 등 매치
        var used = await _db.Users
            .AsNoTracking()
            .Where(u => u.Name.StartsWith(baseId))
            .Select(u => u.Name)
            .ToListAsync(ct);

        var usedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var id in used)
            usedIds.Add(id.ToLowerInvariant());

        // 1) 베이스 자체가 비어 있으면 그대로 반환
        if (!usedIds.Contains(baseId))
            return baseId;

        // 2) 1글자 suffix (a~z)
        for (char c = 'a'; c <= 'z'; c++)
        {
            var candidate = baseId + c;
            if (!usedIds.Contains(candidate))
                return candidate;
        }

        // 3) 2글자 suffix (aa~zz) — 극단적 동명이인 방어
        for (char c1 = 'a'; c1 <= 'z'; c1++)
        {
            for (char c2 = 'a'; c2 <= 'z'; c2++)
            {
                var candidate = baseId + c1 + c2;
                if (!usedIds.Contains(candidate))
                    return candidate;
            }
        }

        throw new InvalidOperationException(
            $"Unable to generate a unique user ID based on '{baseId}' (2-letter suffix range exhausted).");
    }

    /// <summary>
    /// LastName 전체 + FirstName 첫 글자를 소문자로, 영문자/숫자만 남긴 base ID를 만듭니다.
    /// 공백·하이픈·악센트 등은 제거.  예) "O'Brien","Anne" → "obriena"
    /// </summary>
    private static string BuildBase(string firstName, string lastName)
    {
        var last = Sanitize(lastName);
        var first = Sanitize(firstName);
        if (last.Length == 0 || first.Length == 0) return string.Empty;
        return (last + first[0]).ToLowerInvariant();
    }

    private static string Sanitize(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        var buf = new System.Text.StringBuilder(s.Length);
        foreach (var ch in s)
        {
            if (char.IsLetterOrDigit(ch) && ch < 128)  // ASCII 영문/숫자만
                buf.Append(ch);
        }
        return buf.ToString();
    }
}
