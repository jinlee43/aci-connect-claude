using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Services;

public interface IProgressScheduleService
{
    // ── Working Tasks ─────────────────────────────────────────────────────────
    /// <summary>Baseline → Working Schedule 복사 (프로젝트 최초 1회). Rev 0 생성.</summary>
    Task<ScheduleRevision> ForkBaselineAsync(int projectId, int userId, string userName);

    /// <summary>프로젝트의 Working Tasks 전체 조회 (WBS 트리 포함).</summary>
    Task<List<WorkingTask>> GetWorkingTasksAsync(int projectId);

    /// <summary>Working Task 날짜/진행률 업데이트 + ScheduleChange 자동 기록.</summary>
    Task<WorkingTask> UpdateWorkingTaskAsync(
        WorkingTask task, int revisionId, int userId, string userName);

    /// <summary>신규 Working Task 추가 (Baseline에 없는 공정) + Change 기록.</summary>
    Task<WorkingTask> AddWorkingTaskAsync(
        WorkingTask task, int revisionId, int userId, string userName);

    /// <summary>Working Task 삭제(Removed 처리) + Change 기록.</summary>
    Task RemoveWorkingTaskAsync(
        int taskId, int revisionId, string reason, int userId, string userName);

    // ── Revisions ─────────────────────────────────────────────────────────────
    /// <summary>프로젝트의 모든 Revision 목록 (최신순).</summary>
    Task<List<ScheduleRevision>> GetRevisionsAsync(int projectId);

    /// <summary>Draft Revision 조회 or 새로 생성.</summary>
    Task<ScheduleRevision> GetOrCreateDraftRevisionAsync(
        int projectId, int userId, string userName);

    /// <summary>Revision 제출 (Draft → Submitted).</summary>
    Task<ScheduleRevision> SubmitRevisionAsync(int revisionId, int userId, string userName);

    /// <summary>Revision 승인 (Submitted → Approved).</summary>
    Task<ScheduleRevision> ApproveRevisionAsync(
        int revisionId, string approvedByName, string? approvalNotes, DateOnly? approvedDate);

    /// <summary>Revision 반려 (→ Rejected).</summary>
    Task<ScheduleRevision> RejectRevisionAsync(int revisionId, string? notes);

    Task<ScheduleRevision?> GetRevisionWithDetailsAsync(int revisionId);

    // ── Comparison ────────────────────────────────────────────────────────────
    /// <summary>
    /// Baseline vs Working Schedule 비교 데이터.
    /// 애니메이션: revisionId 별 스냅샷을 순서대로 제공.
    /// </summary>
    Task<ScheduleComparisonDto> GetComparisonAsync(int projectId, int? upToRevisionId = null);

    // ── Documents ─────────────────────────────────────────────────────────────
    Task<RevisionDocument> AddDocumentAsync(
        int revisionId, IFormFile file, RevisionDocumentType docType,
        string? referenceNumber, string? notes, int userId, string userName,
        IWebHostEnvironment env);

    Task DeleteDocumentAsync(int documentId, IWebHostEnvironment env);
}

// ─── DTOs ─────────────────────────────────────────────────────────────────────

public class ScheduleComparisonDto
{
    public int ProjectId { get; set; }

    /// <summary>Baseline tasks (immutable reference).</summary>
    public List<BaselineTaskDto> BaselineTasks { get; set; } = [];

    /// <summary>Current working tasks with delta vs baseline.</summary>
    public List<WorkingTaskDto> WorkingTasks { get; set; } = [];

    /// <summary>Chronological revision snapshots for animation.</summary>
    public List<RevisionSnapshotDto> RevisionSnapshots { get; set; } = [];
}

public class BaselineTaskDto
{
    public int      Id        { get; set; }
    public string   Text      { get; set; } = string.Empty;
    public string?  WbsCode   { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate   { get; set; }
    public int      Duration  { get; set; }
    public int?     ParentId  { get; set; }
    public string   TaskType  { get; set; } = "task";
    public string?  Color     { get; set; }
}

public class WorkingTaskDto
{
    public int      Id              { get; set; }
    public int?     BaselineTaskId  { get; set; }
    public string   Text            { get; set; } = string.Empty;
    public string?  WbsCode         { get; set; }
    public DateOnly StartDate       { get; set; }
    public DateOnly EndDate         { get; set; }
    public int      Duration        { get; set; }
    public double   Progress        { get; set; }
    public int?     ParentId        { get; set; }
    public string   TaskType        { get; set; } = "task";
    public string?  Color           { get; set; }
    public string   WorkingStatus   { get; set; } = "Active";

    // Delta vs Baseline
    public int?  DaysShifted       { get; set; }   // finish date delta
    public bool  IsNew             { get; set; }   // no baseline counterpart
    public bool  IsRemoved         { get; set; }
    public bool  IsDelayed         => DaysShifted > 0;
    public bool  IsAhead           => DaysShifted < 0;
}

public class RevisionSnapshotDto
{
    public int      RevisionId     { get; set; }
    public int      RevisionNumber { get; set; }
    public string   Title          { get; set; } = string.Empty;
    public DateTime ApprovedAt     { get; set; }
    public string?  ApprovedBy     { get; set; }

    /// <summary>Task states at this revision point (for animation frame).</summary>
    public List<WorkingTaskDto> TaskStates { get; set; } = [];
}

// ─── Implementation ───────────────────────────────────────────────────────────

public class ProgressScheduleService : IProgressScheduleService
{
    private readonly AppDbContext _db;
    public ProgressScheduleService(AppDbContext db) => _db = db;

    // ── Fork ──────────────────────────────────────────────────────────────────

    public async Task<ScheduleRevision> ForkBaselineAsync(
        int projectId, int userId, string userName)
    {
        if (await _db.WorkingTasks.AnyAsync(t => t.ProjectId == projectId))
            throw new InvalidOperationException(
                "Current Schedule already initialized for this project.");

        var baselineTasks = await _db.ScheduleTasks
            .Where(t => t.ProjectId == projectId && t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();

        // Rev 0 — Initial
        var revision = new ScheduleRevision
        {
            ProjectId      = projectId,
            RevisionNumber = 0,
            Title          = "Rev 0 – Initial Current Schedule",
            Description    = "Forked from Baseline Schedule.",
            RevisionType   = RevisionType.Initial,
            Status         = RevisionStatus.Approved,
            ApprovedAt     = DateTime.UtcNow,
            ApprovedByName = userName,
            DataDate       = DateOnly.FromDateTime(DateTime.Today),
            SubmittedById  = userId,
            CreatedAt      = DateTime.UtcNow,
            UpdatedAt      = DateTime.UtcNow
        };
        _db.ScheduleRevisions.Add(revision);
        await _db.SaveChangesAsync();

        // Map baseline ID → working ID for parent resolution
        var idMap = new Dictionary<int, int>();

        // First pass: create WorkingTasks (without parent)
        var workingTasks = baselineTasks.Select(b => new WorkingTask
        {
            ProjectId      = projectId,
            BaselineTaskId = b.Id,
            WbsCode        = b.WbsCode,
            Text           = b.Text,
            Description    = b.Description,
            Location       = b.Location,
            TaskType       = b.TaskType,
            SortOrder      = b.SortOrder,
            IsOpen         = b.IsOpen,
            TradeId        = b.TradeId,
            AssignedToId   = b.AssignedToId,
            StartDate      = b.StartDate,
            EndDate        = b.EndDate,
            Duration       = b.Duration,
            Progress       = b.Progress,
            ActualStartDate = b.ActualStartDate,
            ActualEndDate   = b.ActualEndDate,
            CompletedDate   = b.CompletedDate,
            IsDone          = b.IsDone,
            ConstraintType  = b.ConstraintType,
            ConstraintDate  = b.ConstraintDate,
            Color           = b.Color,
            CrewSize        = b.CrewSize,
            Notes           = b.Notes,
            WorkingStatus   = WorkingTaskStatus.Active,
            CreatedAt       = DateTime.UtcNow,
            UpdatedAt       = DateTime.UtcNow
        }).ToList();

        _db.WorkingTasks.AddRange(workingTasks);
        await _db.SaveChangesAsync();

        // Build ID map and resolve parent links
        for (int i = 0; i < baselineTasks.Count; i++)
            idMap[baselineTasks[i].Id] = workingTasks[i].Id;

        foreach (var (baseline, working) in baselineTasks.Zip(workingTasks))
        {
            if (baseline.ParentId.HasValue && idMap.TryGetValue(baseline.ParentId.Value, out var wParentId))
                working.ParentId = wParentId;
        }

        // Add Rev 0 ScheduleChange entries (type = Added, baseline reference)
        var changes = workingTasks.Select(wt => new ScheduleChange
        {
            RevisionId     = revision.Id,
            WorkingTaskId  = wt.Id,
            ChangeType     = ChangeType.Added,
            NewStartDate   = wt.StartDate,
            NewEndDate     = wt.EndDate,
            NewDuration    = wt.Duration,
            NewProgress    = wt.Progress,
            NewText        = wt.Text,
            DaysShifted    = 0,
            ChangeNote     = "Initial fork from Baseline.",
            ChangedById    = userId,
            ChangedByName  = userName,
            ChangedAt      = DateTime.UtcNow
        }).ToList();

        _db.ScheduleChanges.AddRange(changes);
        await _db.SaveChangesAsync();

        return revision;
    }

    // ── Working Tasks ─────────────────────────────────────────────────────────

    public async Task<List<WorkingTask>> GetWorkingTasksAsync(int projectId) =>
        await _db.WorkingTasks
            .Where(t => t.ProjectId == projectId && t.IsActive)
            .Include(t => t.Trade)
            .Include(t => t.AssignedTo)
            .Include(t => t.BaselineTask)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();

    public async Task<WorkingTask> UpdateWorkingTaskAsync(
        WorkingTask task, int revisionId, int userId, string userName)
    {
        var existing = await _db.WorkingTasks
            .Include(t => t.BaselineTask)
            .FirstOrDefaultAsync(t => t.Id == task.Id)
            ?? throw new KeyNotFoundException($"WorkingTask {task.Id} not found");

        // Detect what changed
        var changeType = DetermineChangeType(existing, task);
        var daysShifted = task.EndDate.DayNumber - existing.EndDate.DayNumber;

        var change = new ScheduleChange
        {
            RevisionId    = revisionId,
            WorkingTaskId = existing.Id,
            ChangeType    = changeType,
            OldStartDate  = existing.StartDate,
            OldEndDate    = existing.EndDate,
            OldDuration   = existing.Duration,
            OldProgress   = existing.Progress,
            OldText       = existing.Text,
            NewStartDate  = task.StartDate,
            NewEndDate    = task.EndDate,
            NewDuration   = task.Duration,
            NewProgress   = task.Progress,
            NewText       = task.Text,
            DaysShifted   = daysShifted != 0 ? daysShifted : null,
            ChangedById   = userId,
            ChangedByName = userName,
            ChangedAt     = DateTime.UtcNow
        };

        // Apply changes
        existing.StartDate      = task.StartDate;
        existing.EndDate        = task.EndDate;
        existing.Duration       = task.Duration;
        existing.Progress       = task.Progress;
        existing.Text           = task.Text;
        existing.Location       = task.Location;
        existing.TradeId        = task.TradeId;
        existing.AssignedToId   = task.AssignedToId;
        existing.ActualStartDate = task.ActualStartDate;
        existing.ActualEndDate   = task.ActualEndDate;
        existing.IsDone         = task.IsDone;
        existing.Notes          = task.Notes;
        existing.UpdatedAt      = DateTime.UtcNow;

        _db.ScheduleChanges.Add(change);
        await _db.SaveChangesAsync();

        return existing;
    }

    public async Task<WorkingTask> AddWorkingTaskAsync(
        WorkingTask task, int revisionId, int userId, string userName)
    {
        task.CreatedAt    = DateTime.UtcNow;
        task.UpdatedAt    = DateTime.UtcNow;
        task.WorkingStatus = WorkingTaskStatus.Active;
        _db.WorkingTasks.Add(task);
        await _db.SaveChangesAsync();

        _db.ScheduleChanges.Add(new ScheduleChange
        {
            RevisionId    = revisionId,
            WorkingTaskId = task.Id,
            ChangeType    = ChangeType.Added,
            NewStartDate  = task.StartDate,
            NewEndDate    = task.EndDate,
            NewDuration   = task.Duration,
            NewText       = task.Text,
            ChangedById   = userId,
            ChangedByName = userName,
            ChangedAt     = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return task;
    }

    public async Task RemoveWorkingTaskAsync(
        int taskId, int revisionId, string reason, int userId, string userName)
    {
        var task = await _db.WorkingTasks.FindAsync(taskId)
            ?? throw new KeyNotFoundException($"WorkingTask {taskId} not found");

        task.WorkingStatus = WorkingTaskStatus.Removed;
        task.UpdatedAt     = DateTime.UtcNow;

        _db.ScheduleChanges.Add(new ScheduleChange
        {
            RevisionId    = revisionId,
            WorkingTaskId = taskId,
            ChangeType    = ChangeType.Removed,
            OldStartDate  = task.StartDate,
            OldEndDate    = task.EndDate,
            OldText       = task.Text,
            ChangeNote    = reason,
            ChangedById   = userId,
            ChangedByName = userName,
            ChangedAt     = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    // ── Revisions ─────────────────────────────────────────────────────────────

    public async Task<List<ScheduleRevision>> GetRevisionsAsync(int projectId) =>
        await _db.ScheduleRevisions
            .Where(r => r.ProjectId == projectId && r.IsActive)
            .Include(r => r.SubmittedBy)
            .Include(r => r.Changes)
            .Include(r => r.Documents)
            .OrderByDescending(r => r.RevisionNumber)
            .ToListAsync();

    public async Task<ScheduleRevision> GetOrCreateDraftRevisionAsync(
        int projectId, int userId, string userName)
    {
        var draft = await _db.ScheduleRevisions
            .Include(r => r.Changes)
            .FirstOrDefaultAsync(r => r.ProjectId == projectId
                                   && r.Status == RevisionStatus.Draft
                                   && r.IsActive);
        if (draft != null) return draft;

        var nextNum = await _db.ScheduleRevisions
            .Where(r => r.ProjectId == projectId)
            .MaxAsync(r => (int?)r.RevisionNumber) ?? -1;
        nextNum++;

        var revision = new ScheduleRevision
        {
            ProjectId      = projectId,
            RevisionNumber = nextNum,
            Title          = $"Rev {nextNum} – Draft",
            RevisionType   = RevisionType.MonthlyUpdate,
            Status         = RevisionStatus.Draft,
            DataDate       = DateOnly.FromDateTime(DateTime.Today),
            SubmittedById  = userId,
            CreatedAt      = DateTime.UtcNow,
            UpdatedAt      = DateTime.UtcNow
        };
        _db.ScheduleRevisions.Add(revision);
        await _db.SaveChangesAsync();
        return revision;
    }

    public async Task<ScheduleRevision> SubmitRevisionAsync(
        int revisionId, int userId, string userName)
    {
        var rev = await _db.ScheduleRevisions.FindAsync(revisionId)
            ?? throw new KeyNotFoundException();
        rev.Status      = RevisionStatus.Submitted;
        rev.SubmittedAt = DateTime.UtcNow;
        rev.UpdatedAt   = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return rev;
    }

    public async Task<ScheduleRevision> ApproveRevisionAsync(
        int revisionId, string approvedByName, string? approvalNotes, DateOnly? approvedDate)
    {
        var rev = await _db.ScheduleRevisions.FindAsync(revisionId)
            ?? throw new KeyNotFoundException();
        rev.Status         = RevisionStatus.Approved;
        rev.ApprovedAt     = approvedDate.HasValue
            ? approvedDate.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
            : DateTime.UtcNow;
        rev.ApprovedByName = approvedByName;
        rev.ApprovalNotes  = approvalNotes;
        rev.UpdatedAt      = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return rev;
    }

    public async Task<ScheduleRevision> RejectRevisionAsync(int revisionId, string? notes)
    {
        var rev = await _db.ScheduleRevisions.FindAsync(revisionId)
            ?? throw new KeyNotFoundException();
        rev.Status        = RevisionStatus.Rejected;
        rev.ApprovalNotes = notes;
        rev.UpdatedAt     = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return rev;
    }

    public async Task<ScheduleRevision?> GetRevisionWithDetailsAsync(int revisionId) =>
        await _db.ScheduleRevisions
            .Include(r => r.Changes).ThenInclude(c => c.WorkingTask)
            .Include(r => r.Changes).ThenInclude(c => c.ChangedBy)
            .Include(r => r.Documents).ThenInclude(d => d.UploadedBy)
            .Include(r => r.SubmittedBy)
            .FirstOrDefaultAsync(r => r.Id == revisionId);

    // ── Comparison / Animation ────────────────────────────────────────────────

    public async Task<ScheduleComparisonDto> GetComparisonAsync(
        int projectId, int? upToRevisionId = null)
    {
        var baselineTasks = await _db.ScheduleTasks
            .Where(t => t.ProjectId == projectId && t.IsActive)
            .Include(t => t.Trade)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();

        var workingTasks = await _db.WorkingTasks
            .Where(t => t.ProjectId == projectId && t.IsActive)
            .Include(t => t.BaselineTask)
            .Include(t => t.Trade)
            .OrderBy(t => t.SortOrder)
            .ToListAsync();

        // Approved revisions in chronological order (for animation snapshots)
        var revisionsQuery = _db.ScheduleRevisions
            .Where(r => r.ProjectId == projectId
                     && r.Status == RevisionStatus.Approved
                     && r.IsActive);

        if (upToRevisionId.HasValue)
            revisionsQuery = revisionsQuery.Where(r => r.Id <= upToRevisionId.Value);

        var revisions = await revisionsQuery
            .Include(r => r.Changes).ThenInclude(c => c.WorkingTask)
            .OrderBy(r => r.RevisionNumber)
            .ToListAsync();

        // Build animation snapshots by replaying changes revision by revision
        var snapshots = BuildAnimationSnapshots(workingTasks, revisions);

        return new ScheduleComparisonDto
        {
            ProjectId = projectId,
            BaselineTasks = baselineTasks.Select(b => new BaselineTaskDto
            {
                Id        = b.Id,
                Text      = b.Text,
                WbsCode   = b.WbsCode,
                StartDate = b.StartDate,
                EndDate   = b.EndDate,
                Duration  = b.Duration,
                ParentId  = b.ParentId,
                TaskType  = b.GanttTypeString,
                Color     = b.Color
            }).ToList(),
            WorkingTasks = workingTasks.Select(w => ToWorkingDto(w)).ToList(),
            RevisionSnapshots = snapshots
        };
    }

    // ── Documents ─────────────────────────────────────────────────────────────

    public async Task<RevisionDocument> AddDocumentAsync(
        int revisionId, IFormFile file, RevisionDocumentType docType,
        string? referenceNumber, string? notes, int userId, string userName,
        IWebHostEnvironment env)
    {
        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        var guid = Guid.NewGuid().ToString("N");
        var storedName = $"{guid}.{ext}";
        var dir = Path.Combine(env.ContentRootPath, "uploads", "revisions", revisionId.ToString());
        Directory.CreateDirectory(dir);

        await using var fs = File.Create(Path.Combine(dir, storedName));
        await file.CopyToAsync(fs);

        var doc = new RevisionDocument
        {
            RevisionId      = revisionId,
            DocumentType    = docType,
            FileName        = file.FileName,
            StoredFileName  = storedName,
            Extension       = ext,
            FileSizeBytes   = file.Length,
            ReferenceNumber = referenceNumber,
            Notes           = notes,
            UploadedById    = userId,
            UploadedByName  = userName,
            CreatedAt       = DateTime.UtcNow,
            UpdatedAt       = DateTime.UtcNow
        };
        _db.RevisionDocuments.Add(doc);
        await _db.SaveChangesAsync();
        return doc;
    }

    public async Task DeleteDocumentAsync(int documentId, IWebHostEnvironment env)
    {
        var doc = await _db.RevisionDocuments.FindAsync(documentId)
            ?? throw new KeyNotFoundException();
        var path = Path.Combine(env.ContentRootPath, "uploads", "revisions",
                                doc.RevisionId.ToString(), doc.StoredFileName);
        if (File.Exists(path)) File.Delete(path);
        _db.RevisionDocuments.Remove(doc);
        await _db.SaveChangesAsync();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static ChangeType DetermineChangeType(WorkingTask old, WorkingTask updated)
    {
        bool startChanged = old.StartDate != updated.StartDate;
        bool endChanged   = old.EndDate   != updated.EndDate;
        if (startChanged && endChanged)     return ChangeType.DatesShifted;
        if (startChanged)                   return ChangeType.StartShifted;
        if (endChanged)                     return ChangeType.FinishShifted;
        if (old.Duration != updated.Duration) return ChangeType.DurationChanged;
        if (old.Progress != updated.Progress) return ChangeType.ProgressUpdated;
        if (old.Text     != updated.Text)     return ChangeType.ScopeChanged;
        if (old.TradeId  != updated.TradeId)  return ChangeType.TradeChanged;
        return ChangeType.ProgressUpdated;
    }

    private static WorkingTaskDto ToWorkingDto(WorkingTask w) => new()
    {
        Id             = w.Id,
        BaselineTaskId = w.BaselineTaskId,
        Text           = w.Text,
        WbsCode        = w.WbsCode,
        StartDate      = w.StartDate,
        EndDate        = w.EndDate,
        Duration       = w.Duration,
        Progress       = w.Progress,
        ParentId       = w.ParentId,
        TaskType       = w.GanttTypeString,
        Color          = w.Color,
        WorkingStatus  = w.WorkingStatus.ToString(),
        IsNew          = w.BaselineTaskId == null,
        IsRemoved      = w.WorkingStatus == WorkingTaskStatus.Removed,
        DaysShifted    = w.BaselineTask != null
            ? w.EndDate.DayNumber - w.BaselineTask.EndDate.DayNumber
            : null
    };

    private static List<RevisionSnapshotDto> BuildAnimationSnapshots(
        List<WorkingTask> currentTasks, List<ScheduleRevision> revisions)
    {
        // Reconstruct historical state at each revision by walking changes backwards
        // For simplicity: each revision snapshot shows the cumulative state up to that revision
        var snapshots = new List<RevisionSnapshotDto>();

        // Build a mutable state map: workingTaskId → (StartDate, EndDate, Duration, Progress)
        var stateMap = currentTasks.ToDictionary(
            t => t.Id,
            t => (t.StartDate, t.EndDate, t.Duration, t.Progress, t.Text));

        // Replay in reverse to reconstruct past states
        // (walk backwards, undo each revision's changes)
        var revList = revisions.OrderByDescending(r => r.RevisionNumber).ToList();
        var snapList = new List<RevisionSnapshotDto>();

        foreach (var rev in revList)
        {
            // Snapshot at this revision = current stateMap
            snapList.Add(new RevisionSnapshotDto
            {
                RevisionId     = rev.Id,
                RevisionNumber = rev.RevisionNumber,
                Title          = rev.Title,
                ApprovedAt     = rev.ApprovedAt ?? rev.CreatedAt,
                ApprovedBy     = rev.ApprovedByName,
                TaskStates     = stateMap
                    .Where(kv => currentTasks.Any(t => t.Id == kv.Key))
                    .Select(kv =>
                    {
                        var t = currentTasks.First(t => t.Id == kv.Key);
                        return new WorkingTaskDto
                        {
                            Id            = t.Id,
                            BaselineTaskId = t.BaselineTaskId,
                            Text          = kv.Value.Text,
                            WbsCode       = t.WbsCode,
                            StartDate     = kv.Value.StartDate,
                            EndDate       = kv.Value.EndDate,
                            Duration      = kv.Value.Duration,
                            Progress      = kv.Value.Progress,
                            ParentId      = t.ParentId,
                            TaskType      = t.GanttTypeString,
                            Color         = t.Color,
                            WorkingStatus = t.WorkingStatus.ToString(),
                            IsNew         = t.BaselineTaskId == null,
                            IsRemoved     = t.WorkingStatus == WorkingTaskStatus.Removed,
                        };
                    }).ToList()
            });

            // Undo this revision's changes to get the state before this revision
            foreach (var change in rev.Changes.OrderByDescending(c => c.ChangedAt))
            {
                if (!stateMap.ContainsKey(change.WorkingTaskId)) continue;
                if (change.OldStartDate.HasValue || change.OldEndDate.HasValue)
                {
                    var cur = stateMap[change.WorkingTaskId];
                    stateMap[change.WorkingTaskId] = (
                        change.OldStartDate ?? cur.StartDate,
                        change.OldEndDate   ?? cur.EndDate,
                        change.OldDuration  ?? cur.Duration,
                        change.OldProgress  ?? cur.Progress,
                        change.OldText      ?? cur.Text
                    );
                }
            }
        }

        // Return chronological order (oldest first = animation plays forward)
        snapList.Reverse();
        return snapList;
    }
}
