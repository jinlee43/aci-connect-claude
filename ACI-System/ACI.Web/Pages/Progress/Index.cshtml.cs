using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Progress;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext              _db;
    private readonly IProgressScheduleService  _svc;
    private readonly IWebHostEnvironment       _env;

    public IndexModel(AppDbContext db, IProgressScheduleService svc, IWebHostEnvironment env)
    {
        _db  = db;
        _svc = svc;
        _env = env;
    }

    [BindProperty(SupportsGet = true)] public int ProjectId { get; set; }

    public string            ProjectName   { get; set; } = string.Empty;
    public bool              IsInitialized { get; set; }
    public ScheduleRevision? DraftRevision { get; set; }
    public List<WorkingTask> Tasks         { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var project = await _db.Projects.FindAsync(ProjectId);
        if (project == null) return NotFound();
        ProjectName = project.Name;

        IsInitialized = await _db.WorkingTasks.AnyAsync(t => t.ProjectId == ProjectId);

        if (IsInitialized)
        {
            Tasks = await _svc.GetWorkingTasksAsync(ProjectId);
            DraftRevision = await _db.ScheduleRevisions
                .FirstOrDefaultAsync(r => r.ProjectId == ProjectId
                                       && r.Status == RevisionStatus.Draft
                                       && r.IsActive);
        }

        return Page();
    }

    // Initialize: fork Baseline → Progress Schedule
    public async Task<IActionResult> OnPostInitializeAsync()
    {
        var user = await GetCurrentUserAsync();
        await _svc.ForkBaselineAsync(ProjectId, user.Id, user.Name);
        TempData["Success"] = "Progress Schedule initialized from Baseline.";
        return RedirectToPage(new { projectId = ProjectId });
    }

    // API: Get working tasks as JSON for Gantt
    public async Task<IActionResult> OnGetTasksJsonAsync()
    {
        var tasks = await _svc.GetWorkingTasksAsync(ProjectId);
        var data = tasks.Select(t => new
        {
            id         = t.Id,
            text       = t.Text,
            start_date = t.StartDate.ToString("yyyy-MM-dd"),
            end_date   = t.EndDate.ToString("yyyy-MM-dd"),
            duration   = t.Duration,
            progress   = t.Progress,
            parent     = t.ParentId ?? 0,
            type       = t.GanttTypeString,
            open       = t.IsOpen,
            color      = t.Color,
            wbs_code   = t.WbsCode,
            baseline_id    = t.BaselineTaskId,
            days_shifted   = t.DaysDelayed,
            working_status = t.WorkingStatus.ToString(),
            // Baseline reference dates for comparison bars
            baseline_start = t.BaselineTask?.StartDate.ToString("yyyy-MM-dd"),
            baseline_end   = t.BaselineTask?.EndDate.ToString("yyyy-MM-dd"),
        });
        return new JsonResult(new { data });
    }

    private async Task<ApplicationUser> GetCurrentUserAsync()
    {
        var email = User.Identity?.Name ?? string.Empty;
        return await _db.Users.FirstAsync(u => u.Email == email);
    }
}
