using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Admin.OrgUnits;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<OrgUnit> OrgUnits { get; set; } = [];

    [BindProperty] public OrgUnit Input { get; set; } = new();
    public List<OrgUnit> ParentOptions { get; set; } = [];

    public async Task OnGetAsync()
    {
        await LoadAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
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
        var org = await _db.OrgUnits.FindAsync(id);
        if (org != null)
        {
            org.IsActive = !org.IsActive;
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        OrgUnits = await _db.OrgUnits
            .Include(o => o.Parent)
            .OrderBy(o => o.Type).ThenBy(o => o.Name)
            .ToListAsync();

        ParentOptions = await _db.OrgUnits
            .Where(o => o.IsActive)
            .OrderBy(o => o.Name)
            .ToListAsync();
    }
}
