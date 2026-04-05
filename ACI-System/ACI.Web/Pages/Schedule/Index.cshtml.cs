using ACI.Web.Data;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Schedule;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IBaselineService _baselineSvc;

    public IndexModel(AppDbContext db, IBaselineService baselineSvc)
    {
        _db = db;
        _baselineSvc = baselineSvc;
    }

    [BindProperty(SupportsGet = true)]
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var project = await _db.Projects.FindAsync(ProjectId);
        if (project == null) return NotFound();

        ProjectName = project.Name;
        return Page();
    }

    // ── POST: Freeze Baseline from Schedule page ────────────────────────────
    public async Task<IActionResult> OnPostFreezeAsync(string title, string? description)
    {
        var userId   = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var userName = User.Identity?.Name ?? "System";

        var baseline = await _baselineSvc.FreezeBaselineAsync(
            ProjectId, title, description, userId, userName);

        TempData["Success"] = $"Baseline v{baseline.VersionNumber} frozen — {baseline.TaskCount} tasks captured.";
        return RedirectToPage(new { projectId = ProjectId });
    }
}
