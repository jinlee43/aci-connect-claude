using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public int TotalProjects      { get; set; }
    public int ActiveProjects     { get; set; }
    public int OverdueTasksTotal  { get; set; }
    public decimal TotalContractValue { get; set; }
    public List<ProjectSummaryVm> Projects { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        // Admin / ProjAdmin / PM 이 아닌 사용자는 개인 대시보드로 이동
        var isPortfolioUser =
            User.IsInRole(PrivilegeCodes.Admin)       ||
            User.IsInRole(PrivilegeCodes.LsProjAdmin) ||
            User.IsInRole(PrivilegeCodes.JocProjAdmin)||
            User.IsInRole(PrivilegeCodes.ProjectManager);

        if (!isPortfolioUser)
            return RedirectToPage("/MyDashboard/Index");

        var projects  = await _db.Projects.Where(p => p.IsActive).ToListAsync();
        var tasksAll  = await _db.ScheduleTasks.Where(t => t.IsActive).ToListAsync();
        var today     = DateOnly.FromDateTime(DateTime.Today);

        TotalProjects      = projects.Count;
        ActiveProjects     = projects.Count(p => p.Status == ProjectStatus.Active);
        TotalContractValue = projects.Sum(p => p.ContractAmount);
        OverdueTasksTotal  = tasksAll.Count(t => t.EndDate < today && t.Progress < 1.0);

        Projects = projects.Select(p =>
        {
            var tasks    = tasksAll.Where(t => t.ProjectId == p.Id).ToList();
            var progress = tasks.Any() ? tasks.Average(t => t.Progress) * 100 : 0;
            return new ProjectSummaryVm
            {
                Id              = p.Id,
                ProjectCode     = p.ProjectCode,
                Name            = p.Name,
                Type            = p.Type,
                Status          = p.Status,
                ContractAmount  = p.ContractAmount,
                SchdEndDate     = p.SchdEndDate,
                OverallProgress = Math.Round(progress, 1)
            };
        })
        .OrderByDescending(p => p.Status == ProjectStatus.Active)
        .ThenBy(p => p.SchdEndDate)
        .ToList();

        return Page();
    }
}

public class ProjectSummaryVm
{
    public int          Id              { get; set; }
    public string       ProjectCode     { get; set; } = string.Empty;
    public string       Name            { get; set; } = string.Empty;
    public ProjectType  Type            { get; set; }
    public ProjectStatus Status         { get; set; }
    public decimal      ContractAmount  { get; set; }
    public DateOnly?    SchdEndDate     { get; set; }
    public double       OverallProgress { get; set; }
}
