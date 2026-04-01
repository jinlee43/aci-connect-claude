using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ACI.Web.Controllers;

/// <summary>
/// dhtmlxGantt와 통신하는 REST API 컨트롤러
/// dhtmlxGantt의 dataProcessor가 이 엔드포인트로 CRUD 요청을 보냄
/// </summary>
[ApiController]
[Route("api/gantt")]
[Authorize]
public class GanttController : ControllerBase
{
    private readonly IGanttDataService _gantt;
    private readonly ILogger<GanttController> _logger;

    public GanttController(IGanttDataService gantt, ILogger<GanttController> logger)
    {
        _gantt = gantt;
        _logger = logger;
    }

    // ─── 프로젝트 전체 데이터 로드 ────────────────────────────
    // GET /api/gantt/projects/{projectId}/data
    [HttpGet("projects/{projectId:int}/data")]
    public async Task<IActionResult> GetData(int projectId)
    {
        var data = await _gantt.GetProjectDataAsync(projectId);
        return Ok(data);
    }

    // ─── Task CRUD (dhtmlxGantt dataProcessor 형식) ────────────

    // POST /api/gantt/projects/{projectId}/task
    [HttpPost("projects/{projectId:int}/task")]
    public async Task<IActionResult> CreateTask(int projectId, [FromBody] GanttTaskDto task)
    {
        var created = await _gantt.CreateTaskAsync(projectId, task);
        return Ok(new { action = "inserted", tid = created.Id });
    }

    // PUT /api/gantt/projects/{projectId}/task/{id}
    [HttpPut("projects/{projectId:int}/task/{id:int}")]
    public async Task<IActionResult> UpdateTask(int projectId, int id, [FromBody] GanttTaskDto task)
    {
        await _gantt.UpdateTaskAsync(projectId, id, task);
        return Ok(new { action = "updated" });
    }

    // DELETE /api/gantt/projects/{projectId}/task/{id}
    [HttpDelete("projects/{projectId:int}/task/{id:int}")]
    public async Task<IActionResult> DeleteTask(int projectId, int id)
    {
        await _gantt.DeleteTaskAsync(id);
        return Ok(new { action = "deleted" });
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
}
