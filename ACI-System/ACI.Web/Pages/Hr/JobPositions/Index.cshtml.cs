using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Hr.JobPositions;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<JobPosition> JobPositions { get; set; } = [];

    [BindProperty] public JobPosition Input { get; set; } = new();

    /// <summary>편집 모드 여부 — 폼을 기존 레코드로 채워서 열어둔다.</summary>
    public bool IsEditing => Input.Id != 0;

    public async Task<IActionResult> OnGetAsync(int? editId = null)
    {
        JobPositions = await _db.JobPositions
            .OrderBy(p => p.OrdNum).ThenBy(p => p.Name)
            .ToListAsync();

        if (editId is int id && id > 0)
        {
            var existing = await _db.JobPositions.FindAsync(id);
            if (existing == null) return NotFound();
            Input = existing;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!User.IsInRole(PrivilegeCodes.HrAdmin)) return Forbid();

        ModelState.Remove("Input.EmpRoles");

        if (!ModelState.IsValid)
        {
            JobPositions = await _db.JobPositions.OrderBy(p => p.OrdNum).ToListAsync();
            return Page();
        }

        if (Input.Id == 0)
        {
            Input.CreatedAt = DateTime.UtcNow;
            Input.UpdatedAt = DateTime.UtcNow;
            _db.JobPositions.Add(Input);
        }
        else
        {
            var existing = await _db.JobPositions.FindAsync(Input.Id);
            if (existing == null) return NotFound();
            existing.Code      = Input.Code;
            existing.Name      = Input.Name;
            existing.OrdNum    = Input.OrdNum;
            existing.Type      = string.IsNullOrWhiteSpace(Input.Type) ? null : Input.Type.Trim();
            existing.IsActive  = Input.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"'{Input.Name}' saved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        if (!User.IsInRole(PrivilegeCodes.HrAdmin)) return Forbid();

        var pos = await _db.JobPositions.FindAsync(id);
        if (pos != null) { pos.IsActive = !pos.IsActive; await _db.SaveChangesAsync(); }
        return RedirectToPage();
    }
}
