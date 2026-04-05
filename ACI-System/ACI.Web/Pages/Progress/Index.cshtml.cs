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

    // Initialize: fork Baseline → Current Schedule
    public async Task<IActionResult> OnPostInitializeAsync()
    {
        var user = await GetCurrentUserAsync();
        await _svc.ForkBaselineAsync(ProjectId, user.Id, user.Name);
        TempData["Success"] = "Current Schedule initialized from Baseline.";
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
            baseline_start = t.BaselineTask?.StartDate.ToString("yyyy-MM-dd"),
            baseline_end   = t.BaselineTask?.EndDate.ToString("yyyy-MM-dd"),
        });
        return new JsonResult(new { data });
    }

    // API: Batch save all pending Gantt changes
    public async Task<IActionResult> OnPostSaveChangesAsync([FromBody] List<TaskUpdateDto> changes)
    {
        if (changes == null || changes.Count == 0)
            return new JsonResult(new { success = true, saved = 0 });

        try
        {
            var user  = await GetCurrentUserAsync();
            var draft = await _svc.GetOrCreateDraftRevisionAsync(ProjectId, user.Id, user.Name);

            int saved = 0;
            var results = new List<object>();

            foreach (var dto in changes)
            {
                var task = await _db.WorkingTasks
                    .Include(t => t.BaselineTask)
                    .FirstOrDefaultAsync(t => t.Id == dto.TaskId && t.ProjectId == ProjectId);

                if (task == null || task.WorkingStatus == WorkingTaskStatus.Removed)
                    continue;

                task.StartDate = DateOnly.Parse(dto.StartDate);
                task.EndDate   = DateOnly.Parse(dto.EndDate);
                task.Duration  = dto.Duration;
                task.Progress  = Math.Clamp(dto.Progress, 0.0, 1.0);

                var updated = await _svc.UpdateWorkingTaskAsync(task, draft.Id, user.Id, user.Name);
                saved++;

                results.Add(new
                {
                    taskId      = dto.TaskId,
                    daysShifted = updated.DaysDelayed
                });
            }

            return new JsonResult(new
            {
                success       = true,
                saved         = saved,
                revisionId    = draft.Id,
                revisionTitle = draft.Title,
                results
            });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, error = ex.Message }) { StatusCode = 500 };
        }
    }

    private async Task<ApplicationUser> GetCurrentUserAsync()
    {
        var email = User.Identity?.Name ?? string.Empty;
        return await _db.Users.FirstAsync(u => u.Email == email);
    }
}

/// <summary>DTO for Gantt drag-and-drop task updates.</summary>
public class TaskUpdateDto
{
    public int    TaskId    { get; set; }
    public string StartDate { get; set; } = "";
    public string EndDate   { get; set; } = "";
    public int    Duration  { get; set; }
    public double Progress  { get; set; }
}
