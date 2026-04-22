using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Hr.OrgUnits;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<OrgUnit> OrgUnits { get; set; } = [];

    [BindProperty] public OrgUnit Input { get; set; } = new();
    public List<OrgUnit> ParentOptions { get; set; } = [];

    /// <summary>편집 모드 여부 — 폼을 기존 레코드로 채워서 열어둔다.</summary>
    public bool IsEditing => Input.Id != 0;

    public async Task<IActionResult> OnGetAsync(int? editId = null)
    {
        await LoadAsync(editId);

        if (editId is int id && id > 0)
        {
            var existing = await _db.OrgUnits.FindAsync(id);
            if (existing == null) return NotFound();
            Input = existing;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!User.IsInRole(PrivilegeCodes.HrAdmin)) return Forbid();

        ModelState.Remove("Input.Children");
        ModelState.Remove("Input.Parent");
        ModelState.Remove("Input.EmpRoles");
        ModelState.Remove("Input.Project");

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        if (Input.Id == 0)
        {
            Input.CreatedAt = DateTime.UtcNow;
            Input.UpdatedAt = DateTime.UtcNow;
            _db.OrgUnits.Add(Input);
        }
        else
        {
            var existing = await _db.OrgUnits.FindAsync(Input.Id);
            if (existing == null) return NotFound();
            existing.Code     = Input.Code;
            existing.Name     = Input.Name;
            existing.Type     = Input.Type;
            existing.ParentId = Input.ParentId;
            existing.IsActive = Input.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"'{Input.Name}' saved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        if (!User.IsInRole(PrivilegeCodes.HrAdmin)) return Forbid();

        var org = await _db.OrgUnits.FindAsync(id);
        if (org != null)
        {
            org.IsActive = !org.IsActive;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    private async Task LoadAsync(int? editId = null)
    {
        OrgUnits = await _db.OrgUnits
            .Include(o => o.Parent)
            .OrderBy(o => o.Type).ThenBy(o => o.Name)
            .ToListAsync();

        // Parent 드롭다운: 활성 항목만, 편집 중인 자기 자신은 제외 (순환 방지)
        ParentOptions = await _db.OrgUnits
            .Where(o => o.IsActive && (!editId.HasValue || o.Id != editId.Value))
            .OrderBy(o => o.Name)
            .ToListAsync();
    }
}
