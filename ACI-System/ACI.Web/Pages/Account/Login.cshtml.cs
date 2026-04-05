using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using ACI.Web.Data;
using ACI.Web.Data.Entities;
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
            .FirstOrDefaultAsync(u => u.Name == Input.Username && u.IsActive);

        if (user == null || !BCrypt.Net.BCrypt.Verify(Input.Password, user.PasswordHash))
        {
            ErrorMessage = "Invalid email or password.";
            return Page();
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        // Create claims principal
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("UserId",                  user.Id.ToString()),   // 커스텀 클레임: 여러 페이지 호환
            new(ClaimTypes.Name,           user.Name),
            new(ClaimTypes.Email,          user.Email),
            new(ClaimTypes.Role,           user.Role.ToString()),
        };

        if (user.EmployeeId.HasValue)
            claims.Add(new Claim("EmployeeId", user.EmployeeId.Value.ToString()));

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
