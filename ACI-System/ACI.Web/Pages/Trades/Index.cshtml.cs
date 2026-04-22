using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ACI.Web.Pages.Trades;

/// <summary>
/// Trades &amp; Subs 마스터 관리 — 향후 구현 예정.
/// <para>조회(OnGet)는 인증된 모든 사용자 허용.</para>
/// <para>편집(OnPost*)은 <c>ProjectAdmin</c> 정책으로 보호: Admin ∪ LsProjAdmin ∪ JocProjAdmin.</para>
/// </summary>
public class IndexModel : PageModel
{
    /// <summary>뷰에서 Add/Edit 버튼 노출 여부 판단.</summary>
    public bool CanEdit { get; private set; }

    public void OnGet()
    {
        CanEdit = User.IsInRole("Admin")
               || User.IsInRole("LsProjAdmin")
               || User.IsInRole("JocProjAdmin");
    }

    // 편집 핸들러는 향후 구현 시 아래 패턴 사용:
    //
    // [Authorize(Policy = "ProjectAdmin")]
    // public async Task<IActionResult> OnPostSaveAsync(...) { ... }
    //
    // [Authorize(Policy = "ProjectAdmin")]
    // public async Task<IActionResult> OnPostToggleAsync(int id) { ... }
}
