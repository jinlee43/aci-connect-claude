using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Hr.ProjectTeams;

/// <summary>
/// 프로젝트별 팀 구성 관리 (HrAdmin 전용).
/// 각 Project 에 자동 생성된 ProjectTeam OrgUnit 에
/// EmpRole 레코드를 추가·삭제하는 방식으로 팀원을 관리합니다.
/// </summary>
[Authorize(Policy = "HrAdmin")]
public class IndexModel : PageModel
{
    // 프로젝트당 1명 제한 직책 코드
    private static readonly HashSet<string> UniquePositionCodes =
        new(StringComparer.OrdinalIgnoreCase) { "PM", "SUPT", "SP" };

    private static readonly Dictionary<string, string> UniquePositionLabels =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["PM"]   = "Project Manager",
            ["SUPT"] = "Superintendent",
            ["SP"]   = "Superintendent",
        };
    private readonly AppDbContext _db;

    public IndexModel(AppDbContext db) => _db = db;

    // ── 프로젝트 선택 ──────────────────────────────────────────────────────
    [BindProperty(SupportsGet = true)]
    public int? ProjectId { get; set; }

    // ── 페이지 데이터 ──────────────────────────────────────────────────────
    public List<Project>     AllProjects       { get; set; } = [];
    public Project?          SelectedProject   { get; set; }
    public OrgUnit?          TeamUnit          { get; set; }   // Type = ProjectTeam
    public List<EmpRole>     Members           { get; set; } = [];
    public List<Employee>    AvailableEmployees { get; set; } = [];
    public List<JobPosition> JobPositions      { get; set; } = [];

    // ── 팀원 추가 폼 ──────────────────────────────────────────────────────
    [BindProperty] public int       AddEmployeeId    { get; set; }
    [BindProperty] public int?      AddJobPositionId { get; set; }
    [BindProperty] public DateOnly? AddStartDate     { get; set; }
    [BindProperty] public DateOnly? AddEndDate       { get; set; }
    [BindProperty] public string?   AddNotes         { get; set; }

    // ─────────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync()
    {
        await LoadBaseDataAsync();

        if (ProjectId.HasValue)
        {
            SelectedProject = AllProjects.FirstOrDefault(p => p.Id == ProjectId.Value);
            if (SelectedProject != null)
                await LoadTeamAsync(ProjectId.Value);
        }

        return Page();
    }

    // ── 팀원 추가 ─────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostAddMemberAsync()
    {
        if (!ProjectId.HasValue) return RedirectToPage();

        // ProjectTeam OrgUnit 확인
        var unit = await _db.OrgUnits
            .FirstOrDefaultAsync(o => o.ProjectId == ProjectId.Value
                                   && o.Type == OrgUnitType.ProjectTeam);

        if (unit == null)
        {
            TempData["Error"] = "Project team unit not found. Please contact an administrator.";
            return RedirectToPage(new { projectId = ProjectId });
        }

        var today = DateOnly.FromDateTime(DateTime.Today);

        // 중복 체크 (동일 직원이 이미 활성 멤버인지)
        var alreadyActive = await _db.EmpRoles.AnyAsync(r =>
            r.EmployeeId == AddEmployeeId &&
            r.OrgUnitId  == unit.Id &&
            (r.EndDate == null || r.EndDate >= today));

        if (alreadyActive)
        {
            TempData["Error"] = "This employee is already an active member of this project team.";
            return RedirectToPage(new { projectId = ProjectId });
        }

        // PM / Superintendent 중복 체크 (프로젝트당 1명 제한)
        if (AddJobPositionId.HasValue)
        {
            var pos = await _db.JobPositions.FindAsync(AddJobPositionId.Value);
            if (pos != null && UniquePositionCodes.Contains(pos.Code))
            {
                // navigation property 비교 대신 FK ID 목록으로 체크 (EF expression tree 안전)
                var sameCodeIds = await _db.JobPositions
                    .Where(jp => jp.Code == pos.Code)
                    .Select(jp => jp.Id)
                    .ToListAsync();

                var conflict = await _db.EmpRoles
                    .Include(r => r.Employee)
                    .FirstOrDefaultAsync(r =>
                        r.OrgUnitId == unit.Id &&
                        r.JobPositionId.HasValue &&
                        sameCodeIds.Contains(r.JobPositionId!.Value) &&
                        (r.EndDate == null || r.EndDate >= today));

                if (conflict != null)
                {
                    var label = UniquePositionLabels.GetValueOrDefault(pos.Code, pos.Code);
                    var empName = conflict.Employee?.DisplayName ?? $"Employee #{conflict.EmployeeId}";
                    TempData["Error"] =
                        $"A {label} ({empName}) is already assigned to this project. "
                      + $"Please change their position first before assigning a new {label}.";
                    return RedirectToPage(new { projectId = ProjectId });
                }
            }
        }

        _db.EmpRoles.Add(new EmpRole
        {
            EmployeeId    = AddEmployeeId,
            OrgUnitId     = unit.Id,
            JobPositionId = AddJobPositionId,
            IsPrimary     = false,   // 프로젝트 배정은 primary 부서가 아님
            StartDate     = AddStartDate,
            EndDate       = AddEndDate,
            Notes         = AddNotes?.Trim(),
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = "Team member added.";
        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── 팀원 제거 ─────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostRemoveMemberAsync(int roleId)
    {
        var role = await _db.EmpRoles
            .Include(r => r.OrgUnit)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null) return NotFound();

        // 해당 프로젝트의 팀 role 인지 검증 (타 프로젝트 role 삭제 방지)
        if (role.OrgUnit.ProjectId != ProjectId)
        {
            TempData["Error"] = "Invalid operation.";
            return RedirectToPage(new { projectId = ProjectId });
        }

        _db.EmpRoles.Remove(role);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Team member removed.";
        return RedirectToPage(new { projectId = ProjectId });
    }

    // ── 포지션·날짜 변경 ──────────────────────────────────────────────────
    public async Task<IActionResult> OnPostUpdateRoleAsync(
        int roleId, int? newPositionId, DateOnly? newStartDate, DateOnly? newEndDate, string? newNotes)
    {
        var role = await _db.EmpRoles
            .Include(r => r.OrgUnit)
            .Include(r => r.JobPosition)
            .FirstOrDefaultAsync(r => r.Id == roleId);

        if (role == null) return NotFound();

        if (role.OrgUnit.ProjectId != ProjectId)
        {
            TempData["Error"] = "Invalid operation.";
            return RedirectToPage(new { projectId = ProjectId });
        }

        // PM / Superintendent 중복 체크 — 새 포지션이 제한 직책인 경우
        if (newPositionId.HasValue)
        {
            var newPos = await _db.JobPositions.FindAsync(newPositionId.Value);
            if (newPos != null && UniquePositionCodes.Contains(newPos.Code))
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                var sameCodeIds = await _db.JobPositions
                    .Where(jp => jp.Code == newPos.Code)
                    .Select(jp => jp.Id)
                    .ToListAsync();

                var conflict = await _db.EmpRoles
                    .Include(r => r.Employee)
                    .FirstOrDefaultAsync(r =>
                        r.Id != roleId &&
                        r.OrgUnitId == role.OrgUnitId &&
                        r.JobPositionId.HasValue &&
                        sameCodeIds.Contains(r.JobPositionId!.Value) &&
                        (r.EndDate == null || r.EndDate >= today));

                if (conflict != null)
                {
                    var label = UniquePositionLabels.GetValueOrDefault(newPos.Code, newPos.Code);
                    var empName = conflict.Employee?.DisplayName ?? $"Employee #{conflict.EmployeeId}";
                    TempData["Error"] =
                        $"A {label} ({empName}) is already assigned. "
                      + $"Please change their position first.";
                    return RedirectToPage(new { projectId = ProjectId });
                }
            }
        }

        role.JobPositionId = newPositionId;
        role.StartDate     = newStartDate;
        role.EndDate       = newEndDate;
        role.Notes         = string.IsNullOrWhiteSpace(newNotes) ? null : newNotes.Trim();
        await _db.SaveChangesAsync();

        TempData["Success"] = "Updated.";
        return RedirectToPage(new { projectId = ProjectId });
    }

    // ─────────────────────────────────────────────────────────────────────
    private async Task LoadBaseDataAsync()
    {
        AllProjects = await _db.Projects
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        // Type = "Project" 인 직책만 표시 (OrdNum 순)
        JobPositions = await _db.JobPositions
            .Where(p => p.IsActive && p.Type == "Project")
            .OrderBy(p => p.OrdNum)
            .ToListAsync();
    }

    private async Task LoadTeamAsync(int projectId)
    {
        // 활성 직원 전체 — TeamUnit 존재 여부와 무관하게 항상 로드
        AvailableEmployees = (await _db.Employees
            .Where(e => e.IsActive)
            .ToListAsync())
            .OrderBy(e => e.DisplayName)
            .ToList();

        // 해당 프로젝트의 ProjectTeam OrgUnit
        TeamUnit = await _db.OrgUnits
            .FirstOrDefaultAsync(o => o.ProjectId == projectId
                                   && o.Type == OrgUnitType.ProjectTeam);

        if (TeamUnit == null) return;

        // 현재 팀원 — IsActive 는 [NotMapped] 이므로 메모리 정렬
        Members = (await _db.EmpRoles
            .Include(r => r.Employee)
            .Include(r => r.JobPosition)
            .Where(r => r.OrgUnitId == TeamUnit.Id)
            .ToListAsync())
            .OrderByDescending(r => r.IsActive)                    // Active 먼저
            .ThenBy(r => r.JobPosition?.OrdNum ?? int.MaxValue)    // Position OrdNum 순
            .ThenBy(r => r.StartDate)                              // Start 순
            .ThenBy(r => r.EndDate)                                // End 순
            .ToList();
    }
}
