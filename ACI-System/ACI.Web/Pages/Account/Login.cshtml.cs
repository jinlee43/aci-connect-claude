using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly AppDbContext _db;

    public LoginModel(AppDbContext db) => _db = db;

    [BindProperty] public InputModel Input { get; set; } = new();
    public string? ErrorMessage { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        if (!ModelState.IsValid) return Page();

        var loginId = Input.Username.Trim().ToLower();

        var user = await _db.Users
            .Include(u => u.Employee)
            .Include(u => u.UserPrivileges)
                .ThenInclude(up => up.Privilege)
            .FirstOrDefaultAsync(u => u.Name == loginId && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(Input.Password, user.PasswordHash))
        {
            ErrorMessage = "Invalid username or password.";
            return Page();
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // ── Privilege → ClaimTypes.Role 클레임 (상속 관계 expand) ─────────────
        // 사용자가 직접 부여받은 priv 중 IsActive=true 인 것만 수집 → PrivilegeExpander 로
        // 상하위 계층 전개 → 각 코드를 ClaimTypes.Role 다중 클레임으로 심음.
        // 이렇게 하면 User.IsInRole("HrUser") 체크는 HrAdmin 권한자도 만족.
        IReadOnlySet<string> expandedCodes;

        if (user.Name == "admin")
        {
            // admin 계정은 UserPrivileges 레코드와 무관하게
            // DB상 활성화된 모든 privilege를 소스코드 레벨에서 자동 보유
            var allCodes = await _db.Privileges
                .Where(p => p.IsActive)
                .Select(p => p.Code)
                .ToListAsync();
            expandedCodes = PrivilegeExpander.Expand(allCodes);
        }
        else
        {
            var directCodes = user.UserPrivileges
                .Where(up => up.Privilege != null && up.Privilege.IsActive)
                .Select(up => up.Privilege!.Code)
                .ToList();
            expandedCodes = PrivilegeExpander.Expand(directCodes);
        }

        // ── 기본 클레임 ───────────────────────────────────────────────────────
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier,  user.Id.ToString()),
            new(ClaimNames.UserId,          user.Id.ToString()),
            new(ClaimTypes.Name,            user.Name),
            new(ClaimTypes.Email,           user.Email),
        };

        foreach (var code in expandedCodes)
            claims.Add(new Claim(ClaimTypes.Role, code));

        // ── Employee 연결 정보: OrgUnit + JobPosition 클레임 ─────────────────
        // 로그인 시점에 활성 EmpRole 전체를 한 번 읽어 클레임으로 저장.
        // 이후 페이지에서 DB 재조회 없이 User.Claims 에서 직접 꺼낼 수 있음.
        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim(ClaimNames.EmployeeId, user.EmployeeId.Value.ToString()));

            var today = DateOnly.FromDateTime(DateTime.Today);
            var activeRoles = await _db.EmpRoles
                .Include(r => r.OrgUnit)
                .Include(r => r.JobPosition)
                .Where(r => r.EmployeeId == user.EmployeeId.Value
                         && (r.EndDate == null || r.EndDate >= today))
                .ToListAsync();

            // OrgUnitId 클레임 (중복 제거)
            foreach (var orgUnitId in activeRoles.Select(r => r.OrgUnitId).Distinct())
                claims.Add(new Claim(ClaimNames.OrgUnitId, orgUnitId.ToString()));

            // JobPositionCode 클레임 (중복 제거, null 제외)
            foreach (var posCode in activeRoles
                         .Where(r => r.JobPosition != null)
                         .Select(r => r.JobPosition!.Code)
                         .Distinct())
                claims.Add(new Claim(ClaimNames.JobPositionCode, posCode));

            // ── HR 파생 롤: JobPosition 코드 → Role 클레임 자동 결정 ──────────
            var jobCodes = activeRoles
                .Where(r => r.JobPosition != null)
                .Select(r => r.JobPosition!.Code)
                .ToHashSet();

            if (jobCodes.Any(c => c == "PM" || c == "SPM" || c == "APM"))
                claims.Add(new Claim(ClaimTypes.Role, PrivilegeCodes.ProjectManager));

            if (jobCodes.Any(c => c == "SP" || c == "SUPT" || c == "SSUPT" || c == "ASUPT"))
                claims.Add(new Claim(ClaimTypes.Role, PrivilegeCodes.Superintendent));
        }

        var identity  = new ClaimsIdentity(claims, "AciCookies");
        var principal = new ClaimsPrincipal(identity);

        var authProps = new AuthenticationProperties
        {
            IsPersistent = Input.RememberMe,
            ExpiresUtc   = Input.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync("AciCookies", principal, authProps);

        // Portfolio 사용자(Admin/HrAdmin/HrManager/SafetyAdmin/SafetyManager) → Dashboard(/)
        // 나머지 사용자 → My Dashboard
        var isPortfolioUser = principal.IsInRole("Admin")
                              || principal.IsInRole("HrAdmin")
                              || principal.IsInRole("HrManager")
                              || principal.IsInRole("SafetyAdmin")
                              || principal.IsInRole("SafetyManager");
        var defaultHome = isPortfolioUser ? "/" : "/MyDashboard/Index";

        return LocalRedirect(returnUrl ?? defaultHome);
    }

    public class InputModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
