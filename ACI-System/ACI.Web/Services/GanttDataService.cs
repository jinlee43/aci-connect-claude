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
            .Include(t => t.AssignedTo)
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
            Description    = dto.Description,
            SortOrder      = dto.SortOrder,
            ConstraintType = ParseConstraintType(dto.ConstraintType),
            ConstraintDate = string.IsNullOrWhiteSpace(dto.ConstraintDate)
                ? null : ParseDate(dto.ConstraintDate),
            UpdatedAt      = DateTime.UtcNow
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
        task.SortOrder      = dto.SortOrder;
        task.ConstraintType = ParseConstraintType(dto.ConstraintType);
        task.ConstraintDate = string.IsNullOrWhiteSpace(dto.ConstraintDate)
            ? null : ParseDate(dto.ConstraintDate);
        task.UpdatedAt      = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // 부모 태스크의 시작일/종료일/% 재계산 (재귀적으로 상위까지)
        if (task.ParentId.HasValue)
            await RecalcParentsAsync(task.ParentId.Value);

        return ToDto(task);
    }

    // ── 부모 태스크 자동 재계산 ───────────────────────────────────────────
    private async Task RecalcParentsAsync(int parentId)
    {
        var children = await _db.ScheduleTasks
            .Where(t => t.ParentId == parentId && t.IsActive)
            .ToListAsync();

        if (children.Count == 0) return;

        var parent = await _db.ScheduleTasks.FindAsync(parentId);
        if (parent == null) return;

        var minStart     = children.Min(c => c.StartDate);
        var maxEnd       = children.Max(c => c.EndDate);
        var totalDur     = children.Sum(c => c.Duration);
        var weightedProg = totalDur > 0
            ? children.Sum(c => c.Progress * c.Duration) / totalDur
            : 0.0;

        parent.StartDate  = minStart;
        parent.EndDate    = maxEnd;
        parent.Duration   = (maxEnd.DayNumber - minStart.DayNumber);
        parent.Progress   = weightedProg;
        parent.UpdatedAt  = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // 조부모까지 재귀
        if (parent.ParentId.HasValue)
            await RecalcParentsAsync(parent.ParentId.Value);
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

    // ── Cascade delete (task + all descendants) ───────────────────────────
    public async Task<int> DeleteTaskSubtreeAsync(int taskId)
    {
        var allIds = new List<int>();
        await CollectDescendantIdsAsync(taskId, allIds);
        allIds.Add(taskId);

        // 의존성 먼저 삭제
        await _db.TaskDependencies
            .Where(d => allIds.Contains(d.SourceId) || allIds.Contains(d.TargetId))
            .ExecuteDeleteAsync();

        // 자기참조 FK 해제 후 삭제
        await _db.ScheduleTasks
            .Where(t => allIds.Contains(t.Id) && t.ParentId != null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ParentId, (int?)null));

        await _db.ScheduleTasks
            .Where(t => allIds.Contains(t.Id))
            .ExecuteDeleteAsync();

        return allIds.Count;
    }

    private async Task CollectDescendantIdsAsync(int parentId, List<int> ids)
    {
        var children = await _db.ScheduleTasks
            .Where(t => t.ParentId == parentId && t.IsActive)
            .Select(t => t.Id)
            .ToListAsync();

        foreach (var childId in children)
        {
            ids.Add(childId);
            await CollectDescendantIdsAsync(childId, ids);
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
        EndDate     = t.EndDate.ToDateTime(TimeOnly.MinValue).ToString(DateFormat),
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
        WbsCode        = t.WbsCode,
        TradeName      = t.Trade?.Name,
        AssignedToId   = t.AssignedToId,
        AssignedToName = t.AssignedTo != null
            ? $"{t.AssignedTo.FirstName} {t.AssignedTo.LastName}".Trim() : null,
        CrewSize       = t.CrewSize,
        ConstraintType = t.ConstraintType.HasValue ? t.ConstraintType.Value switch
        {
            TaskConstraintType.StartNoEarlierThan => "snet",
            TaskConstraintType.FinishNoLaterThan  => "fnlt",
            TaskConstraintType.MustStartOn        => "mso",
            TaskConstraintType.MustFinishOn       => "mfo",
            _ => null
        } : null,
        ConstraintDate = t.ConstraintDate.HasValue
            ? t.ConstraintDate.Value.ToDateTime(TimeOnly.MinValue).ToString(DateFormat) : null,
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
    private static DateOnly ParseDate(string? date)
    {
        if (string.IsNullOrWhiteSpace(date))
            return DateOnly.FromDateTime(DateTime.Today);

        // 1) 정확한 포맷 먼저 시도
        if (DateTime.TryParseExact(date, DateFormat,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var dt1))
            return DateOnly.FromDateTime(dt1);

        // 2) 다양한 포맷 허용 (dhtmlxGantt 버전에 따라 다를 수 있음)
        if (DateTime.TryParse(date,
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var dt2))
            return DateOnly.FromDateTime(dt2);

        return DateOnly.FromDateTime(DateTime.Today);
    }

    private static GanttTaskType ParseTaskType(string type) => type switch
    {
        "project"   => GanttTaskType.Project,
        "milestone" => GanttTaskType.Milestone,
        _           => GanttTaskType.Task
    };

    private static TaskConstraintType? ParseConstraintType(string? type) => type?.ToLowerInvariant() switch
    {
        "snet" => TaskConstraintType.StartNoEarlierThan,
        "fnlt" => TaskConstraintType.FinishNoLaterThan,
        "mso"  => TaskConstraintType.MustStartOn,
        "mfo"  => TaskConstraintType.MustFinishOn,
        _      => null
    };
}
