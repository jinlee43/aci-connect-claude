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

        var user = await _db.Users
            .Include(u => u.Employee)
            .Include(u => u.UserPrivileges)
                .ThenInclude(up => up.Privilege)
            .FirstOrDefaultAsync(u => u.Name == Input.Username && u.IsActive);

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

        // Create claims principal
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("UserId",                  user.Id.ToString()),   // 커스텀 클레임: 여러 페이지 호환
            new(ClaimTypes.Name,           user.Name),
            new(ClaimTypes.Email,          user.Email),
        };

        foreach (var code in expandedCodes)
            claims.Add(new Claim(ClaimTypes.Role, code));

        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));

            // ── HR 파생 롤: JobPosition 코드로 PM / Superintendent 자동 결정 ──
            // Privilege 테이블이 아닌 EmpRole → JobPosition 에서 읽음
            var today = DateOnly.FromDateTime(DateTime.Today);
            var jobCodes = await _db.EmpRoles
                .Where(r => r.EmployeeId == user.EmployeeId.Value
                         && r.JobPositionId != null
                         && (r.EndDate == null || r.EndDate >= today))
                .Select(r => r.JobPosition!.Code)
                .Distinct()
                .ToListAsync();

            // PM / SPM / APM → ProjectManager 클레임
            if (jobCodes.Any(c => c == "PM" || c == "SPM" || c == "APM"))
                claims.Add(new Claim(ClaimTypes.Role, PrivilegeCodes.ProjectManager));

            // SUPT / SSUPT / ASUPT → Superintendent 클레임
            if (jobCodes.Any(c => c == "SUPT" || c == "SSUPT" || c == "ASUPT"))
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

        return LocalRedirect(returnUrl ?? "/");
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
