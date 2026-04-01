using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Services;

public class GanttDataService : IGanttDataService
{
    private readonly AppDbContext _db;
    private const string DateFormat = "MM-dd-yyyy HH:mm";

    public GanttDataService(AppDbContext db) => _db = db;

    // ── Load full project Gantt data ──────────────────────────────────────
    public async Task<GanttDataDto> GetProjectDataAsync(int projectId)
    {
        var tasks = await _db.ScheduleTasks
            .Where(t => t.ProjectId == projectId && t.IsActive)
            .Include(t => t.Trade)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.StartDate)
            .ToListAsync();

        var links = await _db.TaskDependencies
            .Where(d => d.Source.ProjectId == projectId)
            .ToListAsync();

        return new GanttDataDto
        {
            Data  = tasks.Select(ToDto).ToList(),
            Links = links.Select(ToLinkDto).ToList()
        };
    }

    // ── Create task ───────────────────────────────────────────────────────
    public async Task<GanttTaskDto> CreateTaskAsync(int projectId, GanttTaskDto dto)
    {
        var start = ParseDate(dto.StartDate);
        var task = new ScheduleTask
        {
            ProjectId   = projectId,
            Text        = dto.Text,
            StartDate   = start,
            Duration    = dto.Duration,
            EndDate     = start.AddDays(dto.Duration),
            Progress    = dto.Progress,
            ParentId    = dto.Parent == 0 ? null : dto.Parent,
            TaskType    = ParseTaskType(dto.Type),
            IsOpen      = dto.Open,
            TradeId     = dto.TradeId,
            Color       = dto.Color,
            Location    = dto.Location,
            Description = dto.Description,
            SortOrder   = dto.SortOrder,
            UpdatedAt   = DateTime.UtcNow
        };

        _db.ScheduleTasks.Add(task);
        await _db.SaveChangesAsync();
        return ToDto(task);
    }

    // ── Update task ───────────────────────────────────────────────────────
    public async Task<GanttTaskDto> UpdateTaskAsync(int projectId, int taskId, GanttTaskDto dto)
    {
        var task = await _db.ScheduleTasks.FindAsync(taskId)
            ?? throw new KeyNotFoundException($"Task {taskId} not found");

        var start = ParseDate(dto.StartDate);
        task.Text        = dto.Text;
        task.StartDate   = start;
        task.Duration    = dto.Duration;
        task.EndDate     = start.AddDays(dto.Duration);
        task.Progress    = dto.Progress;
        task.ParentId    = dto.Parent == 0 ? null : dto.Parent;
        task.TaskType    = ParseTaskType(dto.Type);
        task.IsOpen      = dto.Open;
        task.TradeId     = dto.TradeId;
        task.Color       = dto.Color;
        task.Location    = dto.Location;
        task.Description = dto.Description;
        task.SortOrder   = dto.SortOrder;
        task.UpdatedAt   = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDto(task);
    }

    // ── Delete task ───────────────────────────────────────────────────────
    public async Task DeleteTaskAsync(int taskId)
    {
        var task = await _db.ScheduleTasks.FindAsync(taskId);
        if (task != null)
        {
            _db.ScheduleTasks.Remove(task);
            await _db.SaveChangesAsync();
        }
    }

    // ── Create link ───────────────────────────────────────────────────────
    public async Task<GanttLinkDto> CreateLinkAsync(GanttLinkDto dto)
    {
        var link = new TaskDependency
        {
            SourceId = dto.Source,
            TargetId = dto.Target,
            Type     = (DependencyType)int.Parse(dto.Type),
            Lag      = dto.Lag
        };
        _db.TaskDependencies.Add(link);
        await _db.SaveChangesAsync();
        return ToLinkDto(link);
    }

    // ── Update link ───────────────────────────────────────────────────────
    public async Task<GanttLinkDto> UpdateLinkAsync(int linkId, GanttLinkDto dto)
    {
        var link = await _db.TaskDependencies.FindAsync(linkId)
            ?? throw new KeyNotFoundException($"Link {linkId} not found");

        link.SourceId = dto.Source;
        link.TargetId = dto.Target;
        link.Type     = (DependencyType)int.Parse(dto.Type);
        link.Lag      = dto.Lag;

        await _db.SaveChangesAsync();
        return ToLinkDto(link);
    }

    // ── Delete link ───────────────────────────────────────────────────────
    public async Task DeleteLinkAsync(int linkId)
    {
        var link = await _db.TaskDependencies.FindAsync(linkId);
        if (link != null)
        {
            _db.TaskDependencies.Remove(link);
            await _db.SaveChangesAsync();
        }
    }

    // ── Mapping helpers ───────────────────────────────────────────────────
    private static GanttTaskDto ToDto(ScheduleTask t) => new()
    {
        Id          = t.Id,
        Text        = t.Text,
        StartDate   = t.StartDate.ToDateTime(TimeOnly.MinValue).ToString(DateFormat),
        Duration    = t.Duration,
        Progress    = t.Progress,
        Parent      = t.ParentId ?? 0,
        Type        = t.GanttTypeString,
        Open        = t.IsOpen,
        TradeId     = t.TradeId,
        Color       = t.Color ?? t.Trade?.Color,
        Location    = t.Location,
        Description = t.Description,
        SortOrder   = t.SortOrder,
        PlannedStart = t.BaselineStart.HasValue
            ? t.BaselineStart.Value.ToDateTime(TimeOnly.MinValue).ToString(DateFormat) : null,
        PlannedEnd   = t.BaselineEnd.HasValue
            ? t.BaselineEnd.Value.ToDateTime(TimeOnly.MinValue).ToString(DateFormat) : null,
    };

    private static GanttLinkDto ToLinkDto(TaskDependency d) => new()
    {
        Id     = d.Id,
        Source = d.SourceId,
        Target = d.TargetId,
        Type   = ((int)d.Type).ToString(),
        Lag    = d.Lag
    };

    // dhtmlxGantt sends dates as "MM-dd-yyyy HH:mm" → DateOnly
    private static DateOnly ParseDate(string date)
    {
        var dt = DateTime.ParseExact(date, DateFormat,
            System.Globalization.CultureInfo.InvariantCulture);
        return DateOnly.FromDateTime(dt);
    }

    private static GanttTaskType ParseTaskType(string type) => type switch
    {
        "project"   => GanttTaskType.Project,
        "milestone" => GanttTaskType.Milestone,
        _           => GanttTaskType.Task
    };
}
