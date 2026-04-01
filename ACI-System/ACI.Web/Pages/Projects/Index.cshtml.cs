using ACI.Web.Data;
using ACI.Web.Data.Entities;
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

    public async Task OnGetAsync()
    {
        var projects = await _db.Projects.Where(p => p.IsActive).ToListAsync();
        var tasks    = await _db.ScheduleTasks.Where(t => t.IsActive).ToListAsync();

        Projects = projects.Select(p =>
        {
            var pt = tasks.Where(t => t.ProjectId == p.Id).ToList();
            return new ProjectCardVm
            {
                Id              = p.Id,
                ProjectCode     = p.ProjectCode,
                Name            = p.Name,
                City            = p.City,
                Type            = p.Type,
                Status          = p.Status,
                ContractAmount  = p.ContractAmount,
                SchdStartDate   = p.SchdStartDate,
                SchdEndDate     = p.SchdEndDate,
                OverallProgress = pt.Any() ? Math.Round(pt.Average(t => t.Progress) * 100, 1) : 0
            };
        })
        .OrderByDescending(p => p.Status == ProjectStatus.Active)
        .ToList();
    }
}

public class ProjectCardVm
{
    public int          Id              { get; set; }
    public string       ProjectCode     { get; set; } = string.Empty;
    public string       Name            { get; set; } = string.Empty;
    public string?      City            { get; set; }
    public ProjectType  Type            { get; set; }
    public ProjectStatus Status         { get; set; }
    public decimal      ContractAmount  { get; set; }
    public DateOnly?    SchdStartDate   { get; set; }
    public DateOnly?    SchdEndDate     { get; set; }
    public double       OverallProgress { get; set; }
}
