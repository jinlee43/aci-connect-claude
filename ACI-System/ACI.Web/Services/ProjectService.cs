using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Services;

public interface IProjectService
{
    Task<List<Project>> GetAllProjectsAsync();
    Task<Project?> GetProjectAsync(int id);
    Task<Project> CreateProjectAsync(Project project);
    Task<Project> UpdateProjectAsync(Project project);
    Task DeleteProjectAsync(int id);
    Task<List<Trade>> GetProjectTradesAsync(int projectId);
    Task<ProjectDashboardStats> GetDashboardStatsAsync(int projectId);
    Task<PortfolioStats> GetPortfolioStatsAsync();
}

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;
    public ProjectService(AppDbContext db) => _db = db;

    public async Task<List<Project>> GetAllProjectsAsync() =>
        await _db.Projects
            .Where(p => p.IsActive)
            .Include(p => p.OrgUnits).ThenInclude(o => o.EmpRoles).ThenInclude(r => r.Employee)
            .Include(p => p.Trades)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

    public async Task<Project?> GetProjectAsync(int id) =>
        await _db.Projects
            .Include(p => p.OrgUnits).ThenInclude(o => o.EmpRoles).ThenInclude(r => r.Employee)
            .Include(p => p.Trades)
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

    public async Task<Project> CreateProjectAsync(Project project)
    {
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        // ProjectTeam OrgUnit 자동 생성
        // 타입에 맞는 Division 이 있으면 그 하위에, 없으면 ParentId=null 로 독립 생성
        var divisionCode = project.Type == ProjectType.JOC ? "JOC" : "LS";
        var division = await _db.OrgUnits
            .FirstOrDefaultAsync(o => o.Code == divisionCode && o.Type == OrgUnitType.Division);

        _db.OrgUnits.Add(new OrgUnit
        {
            Code      = project.ProjectCode,
            Name      = project.Name,
            Type      = OrgUnitType.ProjectTeam,
            ParentId  = division?.Id,   // Division 없어도 생성 (ProjectId로 연결됨)
            ProjectId = project.Id,
        });
        await _db.SaveChangesAsync();

        return project;
    }

    public async Task<Project> UpdateProjectAsync(Project project)
    {
        project.UpdatedAt = DateTime.UtcNow;
        _db.Projects.Update(project);
        await _db.SaveChangesAsync();
        return project;
    }

    public async Task DeleteProjectAsync(int id)
    {
        var project = await _db.Projects.FindAsync(id);
        if (project != null)
        {
            project.IsActive = false;  // soft delete
            project.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<Trade>> GetProjectTradesAsync(int projectId) =>
        await _db.Trades
            .Where(t => t.ProjectId == projectId && t.IsActive)
            .OrderBy(t => t.Code)
            .ToListAsync();

    public async Task<ProjectDashboardStats> GetDashboardStatsAsync(int projectId)
    {
        var tasks = await _db.ScheduleTasks
            .Where(t => t.ProjectId == projectId && t.IsActive)
            .ToListAsync();

        var today = DateOnly.FromDateTime(DateTime.Today);
        return new ProjectDashboardStats
        {
            TotalTasks      = tasks.Count,
            CompletedTasks  = tasks.Count(t => t.Progress >= 1.0),
            OverdueTasks    = tasks.Count(t => t.EndDate < today && t.Progress < 1.0),
            OverallProgress = tasks.Any()
                ? Math.Round(tasks.Average(t => t.Progress) * 100, 1) : 0
        };
    }

    public async Task<PortfolioStats> GetPortfolioStatsAsync()
    {
        var projects = await _db.Projects.Where(p => p.IsActive).ToListAsync();
        return new PortfolioStats
        {
            TotalProjects   = projects.Count,
            ActiveProjects  = projects.Count(p => p.Status == ProjectStatus.Active),
            TotalContractValue = projects.Sum(p => p.ContractAmount)
        };
    }
}

public class ProjectDashboardStats
{
    public int    TotalTasks      { get; set; }
    public int    CompletedTasks  { get; set; }
    public int    OverdueTasks    { get; set; }
    public double OverallProgress { get; set; }
}

public class PortfolioStats
{
    public int     TotalProjects      { get; set; }
    public int     ActiveProjects     { get; set; }
    public decimal TotalContractValue { get; set; }
}
