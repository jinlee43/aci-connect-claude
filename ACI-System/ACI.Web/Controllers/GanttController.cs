using ACI.Web.Data;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml.Linq;
using ACI.Web.Data.Entities;

namespace ACI.Web.Controllers;

/// <summary>
/// SVAR Gantt (wx-react-gantt) REST API 컨트롤러
/// React 프론트엔드의 CRUD 요청을 처리함
/// </summary>
[ApiController]
[Route("api/gantt")]
[Authorize]
public class GanttController : ControllerBase
{
    private readonly IGanttDataService _gantt;
    private readonly ILogger<GanttController> _logger;
    private readonly AppDbContext _db;
    private readonly IBaselineService _baselineSvc;

    public GanttController(IGanttDataService gantt, ILogger<GanttController> logger,
                           AppDbContext db, IBaselineService baselineSvc)
    {
        _gantt       = gantt;
        _logger      = logger;
        _db          = db;
        _baselineSvc = baselineSvc;
    }

    // ─── 스케줄 편집 가능 여부 ────────────────────────────────────
    // GET /api/gantt/projects/{projectId}/schedule-status
    [HttpGet("projects/{projectId:int}/schedule-status")]
    public async Task<IActionResult> GetScheduleStatus(int projectId)
    {
        var editable = await _baselineSvc.IsScheduleEditableAsync(projectId);
        ScheduleBaseline? draft = null;
        if (editable)
        {
            draft = await _db.ScheduleBaselines
                .Where(b => b.ProjectId == projectId && !b.IsAutoSnapshot
                         && b.IsActive && b.Status == BaselineStatus.Draft)
                .OrderByDescending(b => b.VersionNumber)
                .FirstOrDefaultAsync();
        }
        return Ok(new { editable, draftTitle = draft?.Title, draftVersion = draft?.VersionNumber });
    }

    // ─── Revision 시작 ───────────────────────────────────────────
    // POST /api/gantt/projects/{projectId}/start-revision
    [HttpPost("projects/{projectId:int}/start-revision")]
    public async Task<IActionResult> StartRevision(int projectId, [FromBody] StartRevisionDto dto)
    {
        try
        {
            var draft = await _baselineSvc.StartRevisionAsync(projectId, dto.Title, dto.Description);
            return Ok(new { message = $"Revision v{draft.VersionNumber} started.", version = draft.VersionNumber });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ─── 프로젝트 전체 데이터 로드 ────────────────────────────
    // GET /api/gantt/projects/{projectId}/data
    [HttpGet("projects/{projectId:int}/data")]
    public async Task<IActionResult> GetData(int projectId)
    {
        var data = await _gantt.GetProjectDataAsync(projectId);
        return Ok(data);
    }

    // ─── Task CRUD ──────────────────────────────────────────────

    // POST /api/gantt/projects/{projectId}/task
    [HttpPost("projects/{projectId:int}/task")]
    public async Task<IActionResult> CreateTask(int projectId, [FromBody] GanttTaskDto task)
    {
        if (!await _baselineSvc.IsScheduleEditableAsync(projectId))
            return StatusCode(423, new { message = "Schedule is locked. Start a revision to edit." });
        var created = await _gantt.CreateTaskAsync(projectId, task);
        return Ok(new { action = "inserted", tid = created.Id });
    }

    // PUT /api/gantt/projects/{projectId}/task/{id}
    [HttpPut("projects/{projectId:int}/task/{id:int}")]
    public async Task<IActionResult> UpdateTask(int projectId, int id, [FromBody] GanttTaskDto task)
    {
        if (!await _baselineSvc.IsScheduleEditableAsync(projectId))
            return StatusCode(423, new { message = "Schedule is locked. Start a revision to edit." });
        await _gantt.UpdateTaskAsync(projectId, id, task);
        return Ok(new { action = "updated" });
    }

    // DELETE /api/gantt/projects/{projectId}/task/{id}
    [HttpDelete("projects/{projectId:int}/task/{id:int}")]
    public async Task<IActionResult> DeleteTask(int projectId, int id)
    {
        if (!await _baselineSvc.IsScheduleEditableAsync(id))
            return StatusCode(423, new { message = "Schedule is locked. Start a revision to edit." });
        await _gantt.DeleteTaskAsync(id);
        return Ok(new { action = "deleted" });
    }

    // DELETE /api/gantt/projects/{projectId}/task/{id}/subtree  (cascade)
    [HttpDelete("projects/{projectId:int}/task/{id:int}/subtree")]
    public async Task<IActionResult> DeleteTaskSubtree(int projectId, int id)
    {
        if (!await _baselineSvc.IsScheduleEditableAsync(projectId))
            return StatusCode(423, new { message = "Schedule is locked. Start a revision to edit." });
        var count = await _gantt.DeleteTaskSubtreeAsync(id);
        return Ok(new { action = "deleted", count });
    }

    // ─── Link CRUD ─────────────────────────────────────────────

    // POST /api/gantt/projects/{projectId}/link
    [HttpPost("projects/{projectId:int}/link")]
    public async Task<IActionResult> CreateLink(int projectId, [FromBody] GanttLinkDto link)
    {
        var created = await _gantt.CreateLinkAsync(link);
        return Ok(new { action = "inserted", tid = created.Id });
    }

    // PUT /api/gantt/projects/{projectId}/link/{id}
    [HttpPut("projects/{projectId:int}/link/{id:int}")]
    public async Task<IActionResult> UpdateLink(int projectId, int id, [FromBody] GanttLinkDto link)
    {
        await _gantt.UpdateLinkAsync(id, link);
        return Ok(new { action = "updated" });
    }

    // DELETE /api/gantt/projects/{projectId}/link/{id}
    [HttpDelete("projects/{projectId:int}/link/{id:int}")]
    public async Task<IActionResult> DeleteLink(int projectId, int id)
    {
        await _gantt.DeleteLinkAsync(id);
        return Ok(new { action = "deleted" });
    }

    // ─── Dialog 용 메타데이터 (Trades + Employees) ────────────────
    // GET /api/gantt/projects/{projectId}/meta
    [HttpGet("projects/{projectId:int}/meta")]
    public async Task<IActionResult> GetMeta(int projectId)
    {
        var trades = await _db.Trades
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.Name)
            .Select(t => new { t.Id, t.Name, t.Color, t.Code })
            .ToListAsync();

        var employees = await _db.Employees
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .Select(e => new { e.Id, name = e.FirstName + " " + e.LastName })
            .ToListAsync();

        return Ok(new { trades, employees });
    }

    // ─── MS Project XML Export ─────────────────────────────────
    // GET /api/gantt/projects/{projectId}/export-xml
    [HttpGet("projects/{projectId:int}/export-xml")]
    public async Task<IActionResult> ExportXml(int projectId)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null) return NotFound();

        var tasks = await _db.ScheduleTasks
            .Where(t => t.ProjectId == projectId && t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();

        var links = await _db.TaskDependencies
            .Where(d => d.Source.ProjectId == projectId)
            .ToListAsync();

        // task Id → UID (MS Project uses 1-based sequential UIDs)
        var idToUid = tasks.Select((t, i) => (t.Id, Uid: i + 1))
                           .ToDictionary(x => x.Id, x => x.Uid);

        XNamespace ns = "http://schemas.microsoft.com/project";

        var taskElements = tasks.Select(t =>
        {
            var uid = idToUid[t.Id];
            var el  = new XElement(ns + "Task",
                new XElement(ns + "UID",              uid),
                new XElement(ns + "ID",               uid),
                new XElement(ns + "Name",             t.Text),
                new XElement(ns + "OutlineNumber",    t.WbsCode ?? uid.ToString()),
                new XElement(ns + "Start",            t.StartDate.ToDateTime(TimeOnly.MinValue).ToString("yyyy-MM-ddTHH:mm:ss")),
                new XElement(ns + "Finish",           t.EndDate.ToDateTime(new TimeOnly(17, 0)).ToString("yyyy-MM-ddTHH:mm:ss")),
                new XElement(ns + "Duration",         $"PT{t.Duration * 8}H0M0S"),
                new XElement(ns + "PercentComplete",  (int)(t.Progress * 100)),
                new XElement(ns + "Summary",          t.TaskType == ACI.Web.Data.Entities.GanttTaskType.Project ? "1" : "0"),
                new XElement(ns + "Milestone",        t.TaskType == ACI.Web.Data.Entities.GanttTaskType.Milestone ? "1" : "0"),
                new XElement(ns + "Notes",            t.Notes ?? "")
            );

            // Predecessors
            foreach (var link in links.Where(l => l.TargetId == t.Id))
            {
                if (!idToUid.TryGetValue(link.SourceId, out var predUid)) continue;
                // MS Project type: 0=FF,1=FS,2=SF,3=SS
                int msType = link.Type switch
                {
                    ACI.Web.Data.Entities.DependencyType.FinishToFinish => 0,
                    ACI.Web.Data.Entities.DependencyType.FinishToStart  => 1,
                    ACI.Web.Data.Entities.DependencyType.StartToFinish  => 2,
                    ACI.Web.Data.Entities.DependencyType.StartToStart   => 3,
                    _ => 1
                };
                el.Add(new XElement(ns + "PredecessorLink",
                    new XElement(ns + "PredecessorUID", predUid),
                    new XElement(ns + "Type",           msType),
                    new XElement(ns + "LinkLag",        link.Lag * 60 * 8 * 10)
                ));
            }
            return el;
        });

        var xml = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(ns + "Project",
                new XElement(ns + "Name",    project.Name),
                new XElement(ns + "Title",   project.Name),
                new XElement(ns + "Tasks",   taskElements)
            )
        );

        var bytes = Encoding.UTF8.GetBytes(xml.ToString());
        var fileName = $"{project.Name.Replace(" ", "_")}_schedule.xml";
        return File(bytes, "application/xml", fileName);
    }
}

public record StartRevisionDto(string Title, string? Description);
