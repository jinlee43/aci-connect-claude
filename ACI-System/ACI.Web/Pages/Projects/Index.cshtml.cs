using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Projects;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<ProjectCardVm> Projects { get; set; } = [];

    /// <summary>New Project 버튼 표시 여부 — HrAdmin+ 전용.</summary>
    public bool CanCreate { get; set; }

    public async Task OnGetAsync()
    {
        CanCreate = User.IsInRole(PrivilegeCodes.HrAdmin);

        var projects = await GetVisibleProjectsAsync();
        var projectIds = projects.Select(p => p.Id).ToList();
        var tasks = await _db.ScheduleTasks
            .Where(t => t.IsActive && projectIds.Contains(t.ProjectId))
            .ToListAsync();

        // 프로젝트별 PM 이름 조회 (JobPosition: PM / SPM / APM)
        var today = DateOnly.FromDateTime(DateTime.Today);
        var pmMap = await _db.EmpRoles
            .Where(r => r.OrgUnit.ProjectId != null
                     && projectIds.Contains(r.OrgUnit.ProjectId!.Value)
                     && r.JobPositionId != null
                     && (r.JobPosition!.Code == "PM" || r.JobPosition!.Code == "SPM" || r.JobPosition!.Code == "APM")
                     && (r.EndDate == null || r.EndDate >= today))
            .Select(r => new { ProjectId = r.OrgUnit.ProjectId!.Value, r.Employee.FirstName, r.Employee.LastName })
            .ToListAsync();

        Projects = projects.Select(p =>
        {
            var pt  = tasks.Where(t => t.ProjectId == p.Id).ToList();
            var pm  = pmMap.FirstOrDefault(x => x.ProjectId == p.Id);
            return new ProjectCardVm
            {
                Id              = p.Id,
                ProjectCode     = p.ProjectCode,
                Name            = p.Name,
                SiteAddress     = string.Join(", ", new[] { p.SiteAddress, p.City }
                                    .Where(s => !string.IsNullOrWhiteSpace(s))),
                Type            = p.Type,
                Status          = p.Status,
                SchdStartDate   = p.SchdStartDate,
                SchdEndDate     = p.SchdEndDate,
                ActualEndDate   = p.ActualEndDate,
                OverallProgress = pt.Any() ? Math.Round(pt.Average(t => t.Progress) * 100, 1) : 0,
                PmName          = pm != null ? $"{pm.FirstName} {pm.LastName}".Trim() : null,
            };
        })
        .OrderByDescending(p => p.Status == ProjectStatus.Active)
        .ToList();
    }

    // ── 역할별 프로젝트 가시성 ──────────────────────────────────────────────
    private async Task<List<Project>> GetVisibleProjectsAsync()
    {
        // HrAdmin, HrManager, SafetyAdmin, SafetyManager → 모든 프로젝트
        if (User.IsInRole(PrivilegeCodes.HrAdmin)
            || User.IsInRole(PrivilegeCodes.HrManager)
            || User.IsInRole(PrivilegeCodes.SafetyAdmin)
            || User.IsInRole(PrivilegeCodes.SafetyManager))
        {
            return await AllActiveAsync();
        }

        // Employee 연결이 없으면 빈 목록
        var empIdStr = User.FindFirst("EmployeeId")?.Value;
        if (!int.TryParse(empIdStr, out var empId))
            return [];

        var today = DateOnly.FromDateTime(DateTime.Today);

        // VP 여부: SVP / VP 직책으로 LS 또는 JOC Division에 소속된 경우
        var vpDivCodes = await _db.EmpRoles
            .Where(r => r.EmployeeId == empId
                     && r.JobPositionId != null
                     && (r.EndDate == null || r.EndDate >= today)
                     && (r.JobPosition!.Code == "VP" || r.JobPosition!.Code == "SVP"))
            .Select(r => r.OrgUnit.Code)
            .Distinct()
            .ToListAsync();

        // LsProjAdmin 또는 LS Division VP → Lump Sum 계열 전체
        bool canSeeLS  = User.IsInRole(PrivilegeCodes.LsProjAdmin) || vpDivCodes.Contains("LS");
        // JocProjAdmin 또는 JOC Division VP → JOC 전체
        bool canSeeJOC = User.IsInRole(PrivilegeCodes.JocProjAdmin) || vpDivCodes.Contains("JOC");

        if (canSeeLS && canSeeJOC)
            return await AllActiveAsync();

        if (canSeeLS)
            return await _db.Projects
                .Where(p => p.IsActive && p.Type != ProjectType.JOC)
                .OrderByDescending(p => p.Status == ProjectStatus.Active)
                .ToListAsync();

        if (canSeeJOC)
            return await _db.Projects
                .Where(p => p.IsActive && p.Type == ProjectType.JOC)
                .OrderByDescending(p => p.Status == ProjectStatus.Active)
                .ToListAsync();

        // PM / Superintendent → 담당 프로젝트만 (EmpRole → OrgUnit.ProjectId)
        var assignedIds = await _db.EmpRoles
            .Where(r => r.EmployeeId == empId
                     && r.OrgUnit.ProjectId != null
                     && (r.EndDate == null || r.EndDate >= today))
            .Select(r => r.OrgUnit.ProjectId!.Value)
            .Distinct()
            .ToListAsync();

        return await _db.Projects
            .Where(p => p.IsActive && assignedIds.Contains(p.Id))
            .OrderByDescending(p => p.Status == ProjectStatus.Active)
            .ToListAsync();
    }

    private Task<List<Project>> AllActiveAsync() =>
        _db.Projects
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.Status == ProjectStatus.Active)
            .ToListAsync();
}

public class ProjectCardVm
{
    public int           Id              { get; set; }
    public string        ProjectCode     { get; set; } = string.Empty;
    public string        Name            { get; set; } = string.Empty;
    public string?       SiteAddress     { get; set; }
    public ProjectType   Type            { get; set; }
    public ProjectStatus Status          { get; set; }
    public DateOnly?     SchdStartDate   { get; set; }
    public DateOnly?     SchdEndDate     { get; set; }
    public DateOnly?     ActualEndDate   { get; set; }
    public double        OverallProgress { get; set; }
    public string?       PmName          { get; set; }
}
