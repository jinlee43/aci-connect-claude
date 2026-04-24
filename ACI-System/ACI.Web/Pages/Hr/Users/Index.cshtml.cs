using System.ComponentModel.DataAnnotations;
using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Hr.Users;

/// <summary>
/// Users 관리 페이지.
///
/// <para>
/// <b>폴더 컨벤션</b>: <c>AuthorizeFolder("/Hr", "Hr")</c> — 접근 자체는 HR 권한자 전체에 허용
/// (조회용). 단, 신규 생성 / 삭제(비활성화) / 권한 부여 등 <b>변경 핸들러</b>는
/// <c>[Authorize(Policy = "HrAdmin")]</c> 로 별도 보호합니다.
/// </para>
///
/// <para>
/// <b>비밀번호 관리</b>: 생성/리셋 시 평문 1회 노출 후 BCrypt 해시만 DB 보관.
/// 평문은 <see cref="TempData"/> 로만 잠깐 전달되고 DB/로그에는 기록하지 않습니다.
/// </para>
///
/// <para>
/// <b>권한(Privilege) 부여</b>: 인라인 편집 모드에서 체크박스로 다중 선택.
/// 저장 시 UserPrivileges join row 를 diff 기반으로 insert/delete. 상속 관계(Admin ⊇ HrAdmin…)는
/// <see cref="PrivilegeExpander"/> 가 로그인 시 전개하므로 여기서는 <b>직접 부여(direct)</b> 만 관리.
/// </para>
/// </summary>
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IUserIdGenerator _idGen;

    private const string EmailDomain = "angelescontractor.com";

    public IndexModel(AppDbContext db, IUserIdGenerator idGen)
    {
        _db    = db;
        _idGen = idGen;
    }

    // ── List / Filter ─────────────────────────────────────────────────
    public List<ApplicationUser> Users { get; set; } = [];
    public List<Privilege>       AllPrivileges { get; set; } = [];

    /// <summary>드롭다운용: 아직 Login 계정이 없는 직원 목록 (새 user 생성 시).</summary>
    public List<Employee> EmployeesWithoutAccount { get; set; } = [];

    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public bool   IncludeInactive { get; set; }

    // ── Inline edit state ────────────────────────────────────────────
    [BindProperty(SupportsGet = true)] public int? EditId { get; set; }
    public bool IsEditing => EditId.HasValue;

    /// <summary>Login ID = Name 필드 (편집 폼).</summary>
    [BindProperty] public string  EditName     { get; set; } = string.Empty;
    [BindProperty] public bool    EditIsActive { get; set; }
    /// <summary>체크된 privilege Code 들 (폼 post 시).</summary>
    [BindProperty] public List<string> EditPrivilegeCodes { get; set; } = [];

    // ── New user form ─────────────────────────────────────────────────
    [BindProperty] public CreateInput Create { get; set; } = new();

    public class CreateInput
    {
        /// <summary>Employee 와 연결해서 만들 때만 세팅. null 이면 수동 입력 이름으로 생성.</summary>
        public int? EmployeeId { get; set; }

        /// <summary>Employee 미연결 수동 생성 시 사용자 표시명.</summary>
        [MaxLength(100)]
        public string? Name { get; set; }

        /// <summary>
        /// Login ID (= Name). 비워두면 Employee 의 First/Last Name 으로 <see cref="IUserIdGenerator"/> 가 자동 생성.
        /// 수동 입력 시에는 그대로 사용 (소문자·영숫자).
        /// </summary>
        [MaxLength(100)]
        public string? LoginId { get; set; }  // Name 필드에 저장됨

        [MaxLength(500)]
        public string? InitialPassword { get; set; }

        /// <summary>초기 부여할 privilege Code 목록 (선택).</summary>
        public List<string> PrivilegeCodes { get; set; } = [];
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadListAsync();

        if (EditId.HasValue)
        {
            var target = Users.FirstOrDefault(u => u.Id == EditId.Value);
            if (target != null)
            {
                EditName     = target.Name;
                EditIsActive = target.IsActive;
                EditPrivilegeCodes = target.UserPrivileges
                    .Where(up => up.Privilege != null)
                    .Select(up => up.Privilege!.Code)
                    .ToList();
            }
        }

        return Page();
    }

    // ────────────────────────────────────────────────────────────────
    //  CREATE  (HrAdmin only)
    // ────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!User.IsInRole(PrivilegeCodes.HrAdmin)) return Forbid();

        Employee? emp = null;
        string displayName;
        string localId;

        // ── 1) 이름 / 로그인 ID 결정 ───────────────────────────────────
        if (Create.EmployeeId.HasValue)
        {
            emp = await _db.Employees.FirstOrDefaultAsync(e => e.Id == Create.EmployeeId.Value);
            if (emp == null)
            {
                TempData["Error"] = "Selected employee not found.";
                return RedirectToPage();
            }
            if (await _db.Users.AnyAsync(u => u.EmployeeId == emp.Id))
            {
                TempData["Error"] = $"Employee '{emp.DisplayName}' already has a user account.";
                return RedirectToPage();
            }

            localId = string.IsNullOrWhiteSpace(Create.LoginId)
                ? await _idGen.GenerateDefaultUserIdAsync(emp.FirstName, emp.LastName)
                : Sanitize(Create.LoginId);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(Create.LoginId))
            {
                TempData["Error"] = "Login ID is required when no employee is linked.";
                return RedirectToPage();
            }
            localId = Sanitize(Create.LoginId);
        }

        if (string.IsNullOrWhiteSpace(localId))
        {
            TempData["Error"] = "Login ID could not be generated — please provide one manually.";
            return RedirectToPage();
        }

        if (await _db.Users.AnyAsync(u => u.Name == localId))
        {
            TempData["Error"] = $"Login ID '{localId}' is already in use.";
            return RedirectToPage();
        }

        // ── 2) 초기 비밀번호 ─────────────────────────────────────────
        var plainPw = string.IsNullOrWhiteSpace(Create.InitialPassword)
            ? GenerateTempPassword()
            : Create.InitialPassword.Trim();

        var user = new ApplicationUser
        {
            Name         = localId,
            Email        = $"{localId}@{EmailDomain}",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPw),
            IsActive     = true,
            EmployeeId   = emp?.Id,
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // ── 3) 초기 privilege 부여 ───────────────────────────────────
        if (Create.PrivilegeCodes.Count > 0)
        {
            // Admin(UiNonAssignable)은 이 UI로 부여 불가 — 폼 조작 방어
            if (Create.PrivilegeCodes.Any(c => PrivilegeCodes.UiNonAssignable.Contains(c)))
            {
                TempData["Error"] = "System-reserved privileges (e.g. Admin) cannot be assigned here.";
                return RedirectToPage();
            }

            var privIds = await _db.Privileges
                .Where(p => Create.PrivilegeCodes.Contains(p.Code) && p.IsActive)
                .Select(p => p.Id)
                .ToListAsync();

            var granterId = GetCurrentUserId();
            foreach (var pid in privIds)
            {
                _db.UserPrivileges.Add(new UserPrivilege
                {
                    UserId          = user.Id,
                    PrivilegeId     = pid,
                    GrantedAt       = DateTime.UtcNow,
                    GrantedByUserId = granterId,
                });
            }
            await _db.SaveChangesAsync();
        }

        // ── 4) 평문 비번은 TempData 로 1회 노출 ──────────────────────
        TempData["Success"]       = $"User '{localId}' created.";
        TempData["NewUserLogin"]  = localId;
        TempData["NewUserPwd"]    = plainPw;

        return RedirectToPage();
    }

    // ────────────────────────────────────────────────────────────────
    //  UPDATE (이름 / 활성 / 권한)  (HrAdmin only)
    // ────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!User.IsInRole(PrivilegeCodes.HrAdmin)) return Forbid();
        if (!EditId.HasValue) return RedirectToPage();

        var user = await _db.Users
            .Include(u => u.UserPrivileges)
                .ThenInclude(up => up.Privilege)
            .FirstOrDefaultAsync(u => u.Id == EditId.Value);
        if (user == null) return NotFound();

        if (string.IsNullOrWhiteSpace(EditName))
        {
            TempData["Error"] = "Login ID cannot be empty.";
            return RedirectToPage(new { editId = EditId });
        }

        // Name = Login ID: sanitize, 중복 체크
        var newName = Sanitize(EditName);
        if (string.IsNullOrWhiteSpace(newName))
        {
            TempData["Error"] = "Login ID is invalid.";
            return RedirectToPage(new { editId = EditId });
        }
        if (!string.Equals(newName, user.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (await _db.Users.AnyAsync(u => u.Name == newName && u.Id != user.Id))
            {
                TempData["Error"] = $"Login ID '{newName}' is already in use.";
                return RedirectToPage(new { editId = EditId });
            }
        }
        user.Name     = newName;
        user.Email    = $"{newName}@{EmailDomain}";
        user.IsActive = EditIsActive;

        // ── Privilege diff ───────────────────────────────────────────
        // 원칙:
        //   • UiNonAssignable(Admin 등)은 이 UI로 부여·해제 불가. 폼에 체크박스가 없으므로
        //     targetCodes에 포함되지 않지만, 현재 보유 중인 경우에도 삭제하지 않습니다.
        //   • IsActive=false 인 priv는 체크박스가 disabled 상태라 submit 안 됨 →
        //     targetCodes에 없어도 삭제하지 않습니다(silent revocation 방지).
        //   • DB에 없는 Code나 IsActive=false priv를 추가 요청해도 무시합니다(안전).
        var targetCodes = EditPrivilegeCodes
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .ToHashSet(StringComparer.Ordinal);

        var currentCodes = user.UserPrivileges
            .Where(up => up.Privilege != null)
            .ToDictionary(up => up.Privilege!.Code, up => up, StringComparer.Ordinal);

        // 삭제: 현재는 있는데 target에 없는 priv
        //   단, UiNonAssignable(Admin) 또는 IsActive=false인 priv는 보호.
        foreach (var (code, up) in currentCodes)
        {
            if (PrivilegeCodes.UiNonAssignable.Contains(code)) continue; // Admin 보호
            if (up.Privilege?.IsActive == false) continue;               // 비활성 priv 보호
            if (!targetCodes.Contains(code))
                _db.UserPrivileges.Remove(up);
        }

        // 추가: target에는 있는데 현재 없는 priv (존재 + IsActive 체크)
        var toAddCodes = targetCodes
            .Where(c => !currentCodes.ContainsKey(c))
            .ToList();

        // Admin(UiNonAssignable)은 이 UI로 부여 불가 — 폼 조작 방어
        if (toAddCodes.Any(c => PrivilegeCodes.UiNonAssignable.Contains(c)))
        {
            TempData["Error"] = "System-reserved privileges (e.g. Admin) cannot be assigned here.";
            return RedirectToPage(new { editId = EditId });
        }

        if (toAddCodes.Count > 0)
        {
            var addables = await _db.Privileges
                .Where(p => toAddCodes.Contains(p.Code) && p.IsActive)
                .ToListAsync();
            var granterId = GetCurrentUserId();
            foreach (var p in addables)
            {
                _db.UserPrivileges.Add(new UserPrivilege
                {
                    UserId          = user.Id,
                    PrivilegeId     = p.Id,
                    GrantedAt       = DateTime.UtcNow,
                    GrantedByUserId = granterId,
                });
            }
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"User '{newName}' updated.";
        return RedirectToPage();
    }

    // ────────────────────────────────────────────────────────────────
    //  비밀번호 리셋 — 임시 랜덤 발급  (HrAdmin only)
    // ────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostResetPasswordAsync(int id)
    {
        if (!User.IsInRole(PrivilegeCodes.HrAdmin)) return Forbid();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        var plainPw = GenerateTempPassword();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(plainPw);
        await _db.SaveChangesAsync();

        TempData["Success"]      = $"Password reset for '{user.Name}'. Share the temporary password below.";
        TempData["NewUserLogin"] = user.Name;
        TempData["NewUserPwd"]   = plainPw;
        return RedirectToPage();
    }

    // ────────────────────────────────────────────────────────────────
    //  소프트 삭제: IsActive = false  (HrAdmin only)
    // ────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostDeactivateAsync(int id)
    {
        if (!User.IsInRole(PrivilegeCodes.HrAdmin)) return Forbid();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        // 자기 자신은 비활성화 못 함(잠금 방지)
        if (user.Id == GetCurrentUserId())
        {
            TempData["Error"] = "You cannot deactivate your own account.";
            return RedirectToPage();
        }

        user.IsActive = false;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"User '{user.Name}' deactivated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostReactivateAsync(int id)
    {
        if (!User.IsInRole(PrivilegeCodes.HrAdmin)) return Forbid();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound();

        user.IsActive = true;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"User '{user.Name}' reactivated.";
        return RedirectToPage();
    }

    // ────────────────────────────────────────────────────────────────
    //  Helpers
    // ────────────────────────────────────────────────────────────────
    private async Task LoadListAsync()
    {
        // UiNonAssignable(Admin 등)은 체크박스 목록에서 제외.
        // Admin은 DB seed 단계에서만 할당되며, 이 UI를 통해 부여·해제할 수 없습니다.
        // EF Core가 static 필드 참조를 번역하지 못하므로 로컬 변수로 캡처.
        var nonAssignable = PrivilegeCodes.UiNonAssignable.ToList();
        AllPrivileges = await _db.Privileges
            .Where(p => !nonAssignable.Contains(p.Code))
            .OrderByDescending(p => p.IsBuiltIn)
            .ThenBy(p => p.Code)
            .ToListAsync();

        // admin 계정은 system-only — HR Users UI에 노출하지 않음
        var q = _db.Users
            .Include(u => u.Employee)
            .Include(u => u.UserPrivileges)
                .ThenInclude(up => up.Privilege)
            .Where(u => u.Name != "admin")
            .AsQueryable();

        if (!IncludeInactive)
            q = q.Where(u => u.IsActive);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim().ToLower();
            q = q.Where(u => u.Name.ToLower().Contains(s));
        }

        Users = await q.OrderBy(u => u.Name).ToListAsync();

        EmployeesWithoutAccount = (await _db.Employees
            .Where(e => e.IsActive && e.UserAccount == null)
            .ToListAsync())
            .OrderBy(e => e.DisplayName)
            .ToList();
    }

    private int? GetCurrentUserId()
    {
        var idStr = User.FindFirst("UserId")?.Value;
        return int.TryParse(idStr, out var id) ? id : null;
    }

    private static string Sanitize(string s)
    {
        // @domain 부분이 포함된 경우 로컬 파트만 사용
        var atIdx = s.IndexOf('@');
        if (atIdx >= 0) s = s[..atIdx];

        var buf = new System.Text.StringBuilder(s.Length);
        foreach (var ch in s.Trim())
        {
            if (char.IsLetterOrDigit(ch) && ch < 128)
                buf.Append(char.ToLowerInvariant(ch));
            else if (ch == '.' || ch == '-' || ch == '_')
                buf.Append(ch);
        }
        return buf.ToString();
    }

    /// <summary>
    /// 12자리 임시 비밀번호 — 대/소/숫자/특수 각 최소 1자 포함.
    /// 복붙 혼동 방지 위해 0/O, 1/l/I, 2/Z 등은 제외.
    /// </summary>
    private static string GenerateTempPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXY";
        const string lower = "abcdefghjkmnpqrstuvwxy";
        const string digit = "3456789";
        const string symbol = "!@#$%^&*?";

        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        char Pick(string pool)
        {
            var b = new byte[4];
            rng.GetBytes(b);
            var idx = (int)(BitConverter.ToUInt32(b, 0) % (uint)pool.Length);
            return pool[idx];
        }

        // 12자: 각 풀에서 3자씩 + 남은 1자는 랜덤(전체 풀)
        var all = upper + lower + digit + symbol;
        var chars = new List<char>();
        for (int i = 0; i < 3; i++) chars.Add(Pick(upper));
        for (int i = 0; i < 3; i++) chars.Add(Pick(lower));
        for (int i = 0; i < 3; i++) chars.Add(Pick(digit));
        for (int i = 0; i < 2; i++) chars.Add(Pick(symbol));
        chars.Add(Pick(all));

        // Fisher-Yates shuffle (cryptographic)
        for (int i = chars.Count - 1; i > 0; i--)
        {
            var b = new byte[4];
            rng.GetBytes(b);
            var j = (int)(BitConverter.ToUInt32(b, 0) % (uint)(i + 1));
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars.ToArray());
    }
}
