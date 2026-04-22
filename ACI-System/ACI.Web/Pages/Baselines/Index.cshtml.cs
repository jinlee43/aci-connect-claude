using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Baselines;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IBaselineService _baselineSvc;
    private readonly AppDbContext _db;

    public IndexModel(IBaselineService baselineSvc, AppDbContext db)
    {
        _baselineSvc = baselineSvc;
        _db          = db;
    }

    // ── Bound properties ─────────────────────────────────────────────────────
    [BindProperty(SupportsGet = true)]
    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;
    public List<ScheduleBaseline> Baselines { get; set; } = [];
    public List<BaselineAnimationFrameDto> AnimationFrames { get; set; } = [];

    // ── GET ──────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync()
    {
        var project = await _db.Projects.FindAsync(ProjectId);
        if (project == null) return NotFound();

        ProjectName = project.Name;
        Baselines = await _baselineSvc.GetBaselinesAsync(ProjectId);
        AnimationFrames = await _baselineSvc.GetBaselineEvolutionAsync(ProjectId);

        return Page();
    }

    // ── POST: Freeze Baseline ────────────────────────────────────────────────
    public async Task<IActionResult> OnPostFreezeAsync(string title, string? description)
    {
        var idStr = User.FindFirst("UserId")?.Value
                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var userId) || userId <= 0) return Forbid();
        var userName = User.Identity?.Name ?? "System";

        try
        {
            var baseline = await _baselineSvc.FreezeBaselineAsync(
                ProjectId, title, description, userId, userName);
            TempData["Success"] = $"{baseline.VersionLabel} frozen successfully. {baseline.TaskCount} tasks captured.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── POST: Submit for Approval ────────────────────────────────────────────
    public async Task<IActionResult> OnPostSubmitAsync(int baselineId)
    {
        try
        {
            await _baselineSvc.SubmitForApprovalAsync(baselineId);
            TempData["Success"] = "Baseline submitted for owner approval.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── POST: Approve ────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostApproveAsync(
        int baselineId, string approvedByName, string? approvalNotes)
    {
        try
        {
            await _baselineSvc.ApproveBaselineAsync(
                baselineId, approvedByName, approvalNotes, DateOnly.FromDateTime(DateTime.Today));
            TempData["Success"] = "Baseline approved and locked as official reference.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── POST: Reject ─────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostRejectAsync(int baselineId, string? rejectionNotes)
    {
        try
        {
            await _baselineSvc.RejectBaselineAsync(baselineId, rejectionNotes);
            TempData["Success"] = "Baseline rejected.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── GET: Animation data (JSON) ───────────────────────────────────────────
    public async Task<IActionResult> OnGetAnimationDataAsync()
    {
        var frames = await _baselineSvc.GetBaselineEvolutionAsync(ProjectId);
        return new JsonResult(frames);
    }

    // ── GET: Compare two baselines (JSON) ────────────────────────────────────
    public async Task<IActionResult> OnGetCompareAsync(int baselineIdA, int baselineIdB)
    {
        var result = await _baselineSvc.CompareBaselinesAsync(baselineIdA, baselineIdB);
        return new JsonResult(result);
    }

    // ── GET: Compare baseline vs current (JSON) ──────────────────────────────
    public async Task<IActionResult> OnGetCompareCurrentAsync(int baselineId)
    {
        var result = await _baselineSvc.CompareBaselineVsCurrentAsync(baselineId, ProjectId);
        return new JsonResult(result);
    }
}
