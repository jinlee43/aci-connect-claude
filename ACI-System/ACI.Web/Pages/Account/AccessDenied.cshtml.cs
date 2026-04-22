using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ACI.Web.Pages.Account;

// 인증은 되었으나 정책/역할 미달 시 이 페이지로 리다이렉트됨.
// 자체는 누구나 볼 수 있어야 하므로 [AllowAnonymous].
[AllowAnonymous]
public class AccessDeniedModel : PageModel
{
    public string? ReturnUrl { get; private set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }
}
