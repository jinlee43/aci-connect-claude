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

        // Eager-load Trade and AssignedTo for display columns
        var tradeIds = tasks.Where(t => t.TradeId.HasValue).Select(t => t.TradeId!.Value).Distinct();
        var trades = await _db.Trades.Where(t => tradeIds.Contains(t.Id)).ToDictionaryAsync(t => t.Id);
        var assigneeIds = tasks.Where(t => t.AssignedToId.HasValue).Select(t => t.AssignedToId!.Value).Distinct();
        var assignees = await _db.Employees.Where(e => assigneeIds.Contains(e.Id)).ToDictionaryAsync(e => e.Id);

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
            color      = t.Color ?? (t.TradeId.HasValue && trades.ContainsKey(t.TradeId.Value) ? trades[t.TradeId.Value].Color : null),
            wbs_code   = t.WbsCode,
            trade_name      = t.TradeId.HasValue && trades.TryGetValue(t.TradeId.Value, out var trade) ? trade.Name : null,
            assigned_to_id   = t.AssignedToId,
            assigned_to_name = t.AssignedToId.HasValue && assignees.TryGetValue(t.AssignedToId.Value, out var emp)
                ? $"{emp.FirstName} {emp.LastName}".Trim() : null,
            baseline_id    = t.BaselineTaskId,
            days_shifted   = t.DaysDelayed,
            working_status = t.WorkingStatus.ToString(),
            baseline_start = t.BaselineTask?.StartDate.ToString("yyyy-MM-dd"),
            baseline_end   = t.BaselineTask?.EndDate.ToString("yyyy-MM-dd"),
        });

        // BaselineTaskId → WorkingTaskId 매핑 (링크 변환용)
        var baselineToWorking = tasks
            .Where(t => t.BaselineTaskId.HasValue)
            .ToDictionary(t => t.BaselineTaskId!.Value, t => t.Id);

        var baselineIds = baselineToWorking.Keys.ToList();

        // Baseline TaskDependency → Working Task 링크로 변환
        var deps = await _db.TaskDependencies
            .Where(d => baselineIds.Contains(d.SourceId) && baselineIds.Contains(d.TargetId))
            .ToListAsync();

        static string ToSvarLinkType(DependencyType t) => t switch
        {
            DependencyType.StartToStart   => "s2s",
            DependencyType.FinishToFinish => "e2e",
            DependencyType.StartToFinish  => "s2e",
            _                             => "e2s",  // FinishToStart (기본)
        };

        var links = deps
            .Where(d => baselineToWorking.ContainsKey(d.SourceId) && baselineToWorking.ContainsKey(d.TargetId))
            .Select(d => new
            {
                id     = d.Id,
                source = baselineToWorking[d.SourceId],
                target = baselineToWorking[d.TargetId],
                type   = ToSvarLinkType(d.Type),
            });

        return new JsonResult(new { data, links });
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
        // 1. "UserId" 커스텀 클레임 (신규 로그인 후 존재)
        var idStr = User.FindFirst("UserId")?.Value
                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(idStr, out var userId) && userId > 0)
        {
            var byId = await _db.Users.FindAsync(userId);   // PK 직접 조회 → 가장 신뢰성 높음
            if (byId != null) return byId;
        }

        // 2. Email fallback
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email))
        {
            var byEmail = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (byEmail != null) return byEmail;
        }

        // 3. Name fallback (ClaimTypes.Name = user.Name, 로그인 시 항상 존재)
        var name = User.Identity?.Name;
        if (!string.IsNullOrEmpty(name))
        {
            var byName = await _db.Users.FirstOrDefaultAsync(u => u.Name == name);
            if (byName != null) return byName;
        }

        throw new InvalidOperationException($"Current user not found. Claims: UserId={idStr}, Name={User.Identity?.Name}");
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
