using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.SystemAdmin.Privileges;

/// <summary>
/// Privilege 마스터 관리 페이지.
/// <para>
/// 폴더 컨벤션(<c>AuthorizeFolder("/SystemAdmin", "SystemAdmin")</c>)에 의해 Admin 전용.
/// </para>
///
/// <para>
/// <b>편집 제한</b>:
/// <list type="bullet">
///   <item><c>IsBuiltIn=true</c> row 는 <c>Code</c> 수정·삭제 금지 (Program.cs 정책 문자열과 1:1).</item>
///   <item>커스텀 priv 추가는 가능하지만, Program.cs 정책/페이지 로직이 인식하려면 코드 수정이 필요함. UI 에서 경고 표시.</item>
/// </list>
/// </para>
/// </summary>
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Privilege> Privileges { get; set; } = [];

    // 편집 중인 row (inline edit)
    [BindProperty] public int?    EditId          { get; set; }
    [BindProperty] public string  EditCode        { get; set; } = string.Empty;
    [BindProperty] public string  EditName        { get; set; } = string.Empty;
    [BindProperty] public string? EditDescription { get; set; }
    [BindProperty] public bool    EditIsActive    { get; set; }

    public bool IsEditing => EditId.HasValue;

    // 신규 추가 form
    [BindProperty] public string  NewCode        { get; set; } = string.Empty;
    [BindProperty] public string  NewName        { get; set; } = string.Empty;
    [BindProperty] public string? NewDescription { get; set; }

    public async Task<IActionResult> OnGetAsync(int? editId = null)
    {
        await LoadAsync();

        if (editId.HasValue)
        {
            var target = Privileges.FirstOrDefault(p => p.Id == editId.Value);
            if (target != null)
            {
                EditId          = target.Id;
                EditCode        = target.Code;
                EditName        = target.Name;
                EditDescription = target.Description;
                EditIsActive    = target.IsActive;
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (string.IsNullOrWhiteSpace(NewCode) || string.IsNullOrWhiteSpace(NewName))
        {
            TempData["Error"] = "Code and Name are required.";
            return RedirectToPage();
        }

        var code = NewCode.Trim();
        if (await _db.Privileges.AnyAsync(p => p.Code == code))
        {
            TempData["Error"] = $"Privilege code '{code}' already exists.";
            return RedirectToPage();
        }

        _db.Privileges.Add(new Privilege
        {
            Code        = code,
            Name        = NewName.Trim(),
            Description = string.IsNullOrWhiteSpace(NewDescription) ? null : NewDescription.Trim(),
            IsBuiltIn   = false,       // 사용자 정의 priv 는 항상 false
            IsActive    = true,
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Privilege '{code}' created. Remember to wire it up in Program.cs policies.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateAsync()
    {
        if (!EditId.HasValue) return RedirectToPage();

        var priv = await _db.Privileges.FindAsync(EditId.Value);
        if (priv == null) return NotFound();

        // Code 는 빌트인이면 수정 금지. 커스텀이면 중복 체크 후 수정.
        if (!priv.IsBuiltIn)
        {
            var newCode = (EditCode ?? "").Trim();
            if (string.IsNullOrEmpty(newCode))
            {
                TempData["Error"] = "Code cannot be empty.";
                return RedirectToPage(new { editId = EditId });
            }
            if (newCode != priv.Code &&
                await _db.Privileges.AnyAsync(p => p.Code == newCode && p.Id != priv.Id))
            {
                TempData["Error"] = $"Code '{newCode}' is already used by another privilege.";
                return RedirectToPage(new { editId = EditId });
            }
            priv.Code = newCode;
        }

        var newName = (EditName ?? "").Trim();
        if (string.IsNullOrEmpty(newName))
        {
            TempData["Error"] = "Name cannot be empty.";
            return RedirectToPage(new { editId = EditId });
        }

        priv.Name        = newName;
        priv.Description = string.IsNullOrWhiteSpace(EditDescription) ? null : EditDescription.Trim();
        priv.IsActive    = EditIsActive;
        priv.UpdatedAt   = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Privilege '{priv.Code}' updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var priv = await _db.Privileges
            .Include(p => p.UserPrivileges)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (priv == null) return NotFound();

        if (priv.IsBuiltIn)
        {
            TempData["Error"] = $"Built-in privilege '{priv.Code}' cannot be deleted.";
            return RedirectToPage();
        }

        if (priv.UserPrivileges.Any())
        {
            TempData["Error"] = $"Privilege '{priv.Code}' is still assigned to {priv.UserPrivileges.Count} user(s). Revoke it first.";
            return RedirectToPage();
        }

        _db.Privileges.Remove(priv);
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Privilege '{priv.Code}' deleted.";
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        // Admin은 system-only — UI에 노출하지 않음
        var nonAssignable = PrivilegeCodes.UiNonAssignable.ToList();
        Privileges = await _db.Privileges
            .Include(p => p.UserPrivileges)
            .Where(p => !nonAssignable.Contains(p.Code))
            .OrderByDescending(p => p.IsBuiltIn)
            .ThenBy(p => p.Code)
            .ToListAsync();
    }
}
