using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Safety;

/// <summary>
/// 프로젝트별 Safety Weekly Report 설정 페이지.
///
/// 접근 규칙:
///   편집(Save)      : SafetyManager+ 또는 PM/Superintendent (담당 프로젝트, 미승인 상태만)
///   승인(Approve)   : SafetyManager+ 전용
///   승인취소(Void)  : SafetyManager+ 전용
///
/// 프로젝트 목록:
///   SafetyManager+ → 모든 활성 프로젝트
///   PM/Superintendent → 담당 프로젝트만
/// </summary>
[Authorize]
public class SettingsModel : PageModel
{
    private readonly ISafetyWkRepService _svc;
    private readonly AppDbContext        _db;

    public SettingsModel(ISafetyWkRepService svc, AppDbContext db)
    {
        _svc = svc;
        _db  = db;
    }

    // ── View data ─────────────────────────────────────────────────────────────
    [BindProperty(SupportsGet = true)]
    public int ProjectId { get; set; }

    public List<Project>            Projects        { get; set; } = [];
    public Project?                 SelectedProject { get; set; }
    public SafetyWkRepSettings?     Settings        { get; set; }

    /// <summary>현재 사용자가 선택된 프로젝트 설정을 편집할 수 있는지.</summary>
    public bool CanEdit    { get; set; }
    /// <summary>SafetyManager+ 전용 — 승인/승인취소 버튼 표시 여부.</summary>
    public bool CanApprove { get; set; }

    // ── Form input ────────────────────────────────────────────────────────────
    [BindProperty] public string  InputStartDate { get; set; } = string.Empty;
    [BindProperty] public string? InputEndDate   { get; set; }
    [BindProperty] public int     InputSubmitDay { get; set; } = (int)DayOfWeek.Friday;
    [BindProperty] public string? InputNotes     { get; set; }

    // ── GET ───────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync()
    {
        var (userId, _) = GetUser();
        if (userId <= 0) return Forbid();

        CanApprove = IsSafetyManager();

        // 프로젝트 목록: SafetyManager+ → 전체, 그 외 → 담당 프로젝트만
        if (IsSafetyManager())
        {
            Projects = await _db.Projects
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProjectCode)
                .ToListAsync();
        }
        else
        {
            var assignedIds = await _svc.GetAssignedProjectIdsAsync(userId);
            if (assignedIds.Count == 0) return Page();   // 담당 프로젝트 없음 — 빈 페이지

            Projects = await _db.Projects
                .Where(p => p.IsActive && assignedIds.Contains(p.Id))
                .OrderBy(p => p.ProjectCode)
                .ToListAsync();
        }

        if (ProjectId > 0)
        {
            SelectedProject = Projects.FirstOrDefault(p => p.Id == ProjectId);
            if (SelectedProject == null) return NotFound();

            Settings = await _svc.GetSettingsAsync(ProjectId);

            // 편집 가능 여부: SafetyManager+, 또는 담당 프로젝트이면서 미승인 상태
            CanEdit = IsSafetyManager()
                   || (Projects.Any(p => p.Id == ProjectId) && Settings?.IsApproved != true);

            if (Settings != null)
            {
                InputStartDate = Settings.StartDate.ToString("yyyy-MM-dd");
                InputEndDate   = Settings.EndDate?.ToString("yyyy-MM-dd");
                InputSubmitDay = (int)Settings.DefaultSubmitDay;
                InputNotes     = Settings.Notes;
            }
        }

        return Page();
    }

    // ── POST: Save settings ───────────────────────────────────────────────────
    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (ProjectId <= 0) return BadRequest("No project selected.");

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        // 편집 권한 확인
        if (!IsSafetyManager())
        {
            // PM/Superintendent: 담당 프로젝트인지 확인
            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(ProjectId))
                return Forbid();

            // 승인된 설정은 SafetyManager+만 편집 가능
            var existing = await _svc.GetSettingsAsync(ProjectId);
            if (existing?.IsApproved == true)
            {
                TempData["Error"] = "Settings are approved. Only Safety Manager can edit approved settings.";
                return RedirectToPage(new { projectId = ProjectId });
            }
        }

        if (!DateOnly.TryParse(InputStartDate, out var startDate))
        {
            TempData["Error"] = "Invalid start date.";
            return RedirectToPage(new { projectId = ProjectId });
        }

        DateOnly? endDate = null;
        if (!string.IsNullOrWhiteSpace(InputEndDate))
        {
            if (!DateOnly.TryParse(InputEndDate, out var ed))
            {
                TempData["Error"] = "Invalid end date.";
                return RedirectToPage(new { projectId = ProjectId });
            }
            if (ed <= startDate)
            {
                TempData["Error"] = "End date must be after start date.";
                return RedirectToPage(new { projectId = ProjectId });
            }
            endDate = ed;
        }

        try
        {
            await _svc.SaveSettingsAsync(
                ProjectId, startDate, endDate,
                (DayOfWeek)InputSubmitDay, InputNotes,
                userId, userName);
            TempData["Success"] = "Settings saved successfully.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── POST: Approve (SafetyManager+ 전용) ───────────────────────────────────
    public async Task<IActionResult> OnPostApproveAsync(int settingsId)
    {
        if (!IsSafetyManager()) return Forbid();

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        try
        {
            await _svc.ApproveSettingsAsync(settingsId, userId, userName);
            TempData["Success"] = "Settings approved and locked.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── POST: Void (SafetyManager+ 전용) ──────────────────────────────────────
    public async Task<IActionResult> OnPostVoidAsync(int settingsId)
    {
        if (!IsSafetyManager()) return Forbid();

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        try
        {
            await _svc.VoidSettingsApprovalAsync(settingsId, userId, userName);
            TempData["Success"] = "Settings approval revoked. Settings can now be edited.";
        }
        catch (Exception ex) { TempData["Error"] = ex.Message; }

        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private (int userId, string userName) GetUser()
    {
        var idStr = User.FindFirst("UserId")?.Value
                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id) || id <= 0) return (0, "");
        return (id, User.Identity?.Name ?? "Unknown");
    }

    private bool IsSafetyManager() =>
        User.IsInRole(PrivilegeCodes.Admin)
        || User.IsInRole(PrivilegeCodes.SafetyAdmin)
        || User.IsInRole(PrivilegeCodes.SafetyManager);
}
