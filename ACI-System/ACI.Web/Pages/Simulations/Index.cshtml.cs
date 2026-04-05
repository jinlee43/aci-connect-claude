using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ACI.Web.Pages.Simulations;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ISimulationService _simSvc;
    private readonly IBaselineService _baselineSvc;
    private readonly IProgressScheduleService _progressSvc;
    private readonly AppDbContext _db;

    public IndexModel(
        ISimulationService simSvc, IBaselineService baselineSvc,
        IProgressScheduleService progressSvc, AppDbContext db)
    {
        _simSvc      = simSvc;
        _baselineSvc = baselineSvc;
        _progressSvc = progressSvc;
        _db          = db;
    }

    [BindProperty(SupportsGet = true)]
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;
    public List<ScheduleSimulation> Simulations { get; set; } = [];
    public List<ScheduleBaseline> Baselines { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var project = await _db.Projects.FindAsync(ProjectId);
        if (project == null) return NotFound();

        ProjectName = project.Name;
        Simulations = await _simSvc.GetSimulationsAsync(ProjectId);
        Baselines   = await _baselineSvc.GetBaselinesAsync(ProjectId);

        return Page();
    }

    // ── POST: Create simulation ──────────────────────────────────────────────
    public async Task<IActionResult> OnPostCreateAsync(
        string name, string? description, int sourceType, int? sourceBaselineId)
    {
        var userId   = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var userName = User.Identity?.Name ?? "System";

        var sim = await _simSvc.CreateSimulationAsync(
            ProjectId, name, description,
            (SimulationSourceType)sourceType, sourceBaselineId,
            userId, userName);

        TempData["Success"] = $"Simulation '{sim.Name}' created.";
        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── POST: Archive simulation ─────────────────────────────────────────────
    public async Task<IActionResult> OnPostArchiveAsync(int simulationId)
    {
        await _simSvc.ArchiveSimulationAsync(simulationId);
        TempData["Success"] = "Simulation archived.";
        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── POST: Delete simulation ──────────────────────────────────────────────
    public async Task<IActionResult> OnPostDeleteAsync(int simulationId)
    {
        await _simSvc.DeleteSimulationAsync(simulationId);
        TempData["Success"] = "Simulation deleted.";
        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── POST: Promote to Current Plan ────────────────────────────────────────
    public async Task<IActionResult> OnPostPromoteAsync(int simulationId)
    {
        var userId   = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
        var userName = User.Identity?.Name ?? "System";

        var revision = await _progressSvc.GetOrCreateDraftRevisionAsync(
            ProjectId, userId, userName);

        await _simSvc.PromoteToCurrentPlanAsync(simulationId, revision.Id, userId, userName);
        TempData["Success"] = "Simulation changes applied to Current Plan.";
        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── GET: Simulation result JSON ──────────────────────────────────────────
    public async Task<IActionResult> OnGetResultAsync(int simulationId)
    {
        var result = await _simSvc.GetSimulationResultAsync(simulationId);
        return new JsonResult(result);
    }

    // ── GET: Compare two simulations JSON ────────────────────────────────────
    public async Task<IActionResult> OnGetCompareAsync(int simIdA, int simIdB)
    {
        var result = await _simSvc.CompareSimulationsAsync(simIdA, simIdB);
        return new JsonResult(result);
    }
}
