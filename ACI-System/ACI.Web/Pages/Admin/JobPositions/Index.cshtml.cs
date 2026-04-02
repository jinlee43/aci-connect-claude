using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Admin.JobPositions;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<JobPosition> JobPositions { get; set; } = [];

    [BindProperty] public JobPosition Input { get; set; } = new();

    public async Task OnGetAsync()
    {
        JobPositions = await _db.JobPositions
            .OrderBy(p => p.OrdNum).ThenBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
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
            existing.IsActive  = Input.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"'{Input.Name}' saved.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostToggleAsync(int id)
    {
        var pos = await _db.JobPositions.FindAsync(id);
        if (pos != null) { pos.IsActive = !pos.IsActive; await _db.SaveChangesAsync(); }
        return RedirectToPage();
    }
}
