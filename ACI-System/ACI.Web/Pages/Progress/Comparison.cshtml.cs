using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Progress;

[Authorize]
public class ComparisonModel : PageModel
{
    private readonly AppDbContext             _db;
    private readonly IProgressScheduleService _svc;

    public ComparisonModel(AppDbContext db, IProgressScheduleService svc)
    {
        _db  = db;
        _svc = svc;
    }

    [BindProperty(SupportsGet = true)] public int ProjectId { get; set; }

    public string ProjectName   { get; set; } = string.Empty;
    public bool   IsInitialized { get; set; }
    public int    RevisionCount { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var project = await _db.Projects.FindAsync(ProjectId);
        if (project == null) return NotFound();
        ProjectName = project.Name;

        IsInitialized = await _db.WorkingTasks.AnyAsync(t => t.ProjectId == ProjectId);
        if (IsInitialized)
        {
            RevisionCount = await _db.ScheduleRevisions
                .CountAsync(r => r.ProjectId == ProjectId
                              && r.Status == RevisionStatus.Approved
                              && r.IsActive);
        }

        return Page();
    }

    // API: Full comparison + animation data
    public async Task<IActionResult> OnGetComparisonJsonAsync()
    {
        if (!await _db.WorkingTasks.AnyAsync(t => t.ProjectId == ProjectId))
            return new JsonResult(new { error = "Not initialized" }) { StatusCode = 404 };

        var dto = await _svc.GetComparisonAsync(ProjectId);

        // Serialize cleanly for JS consumption
        var result = new
        {
            baselineTasks = dto.BaselineTasks.Select(b => new
            {
                id         = b.Id,
                text       = b.Text,
                start_date = b.StartDate.ToString("yyyy-MM-dd"),
                end_date   = b.EndDate.ToString("yyyy-MM-dd"),
                duration   = b.Duration,
                parent     = b.ParentId ?? 0,
                type       = b.TaskType,
                color      = b.Color,
                wbs_code   = b.WbsCode,
                is_baseline = true
            }),
            workingTasks = dto.WorkingTasks.Select(w => new
            {
                id             = w.Id,
                text           = w.Text,
                start_date     = w.StartDate.ToString("yyyy-MM-dd"),
                end_date       = w.EndDate.ToString("yyyy-MM-dd"),
                duration       = w.Duration,
                progress       = w.Progress,
                parent         = w.ParentId ?? 0,
                type           = w.TaskType,
                color          = w.Color,
                wbs_code       = w.WbsCode,
                baseline_task_id = w.BaselineTaskId,
                days_shifted   = w.DaysShifted,
                is_new         = w.IsNew,
                is_removed     = w.IsRemoved,
                working_status = w.WorkingStatus
            }),
            revisionSnapshots = dto.RevisionSnapshots.Select(s => new
            {
                revisionId     = s.RevisionId,
                revisionNumber = s.RevisionNumber,
                title          = s.Title,
                approvedAt     = s.ApprovedAt.ToString("yyyy-MM-dd"),
                approvedBy     = s.ApprovedBy,
                taskStates     = s.TaskStates.Select(t => new
                {
                    id         = t.Id,
                    start_date = t.StartDate.ToString("yyyy-MM-dd"),
                    end_date   = t.EndDate.ToString("yyyy-MM-dd"),
                    duration   = t.Duration,
                    progress   = t.Progress,
                    text       = t.Text,
                    is_removed = t.IsRemoved
                })
            })
        };

        return new JsonResult(result);
    }
}
