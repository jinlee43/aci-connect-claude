using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ACI.Web.Pages.Projects;

/// <summary>
/// /Projects/Create 는 /Projects/Detail 로 통합됨.
/// 기존 북마크/링크 호환을 위해 리다이렉트만 처리.
/// </summary>
[Authorize]
public class CreateModel : PageModel
{
    public IActionResult OnGet()  => RedirectToPage("Detail");
    public IActionResult OnPost() => RedirectToPage("Detail");
}
