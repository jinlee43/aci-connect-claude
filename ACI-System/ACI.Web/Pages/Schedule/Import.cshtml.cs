using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace ACI.Web.Pages.Schedule;

public class ImportModel : PageModel
{
    private readonly AppDbContext _db;
    public ImportModel(AppDbContext db) => _db = db;

    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;

    [BindProperty] public IFormFile? XmlFile { get; set; }
    [BindProperty] public bool ReplaceExisting { get; set; } = true;

    public string? ErrorMessage { get; set; }
    public ImportResult? Result { get; set; }

    /// <summary>
    /// MS Project XML 날짜 파싱.
    /// "NA", 빈 문자열, null → null 반환.
    /// ISO 8601 "2024-01-22T08:00:00" 형식 처리.
    /// </summary>
    private static DateTime? ParseMspDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw) || raw.Trim() == "NA") return null;

        // DateTime.TryParse는 ISO 8601 (2023-09-21T08:00:00) 형식을 기본 지원
        if (DateTime.TryParse(raw, out var dt))
            return dt;

        // 타임존 오프셋 포함 형식 (2024-01-22T08:00:00-08:00) 처리
        if (DateTimeOffset.TryParse(raw, out var dto))
            return dto.DateTime;

        return null;
    }

    public class ImportResult
    {
        public int TasksImported { get; set; }
        public int LinksImported { get; set; }
        public int TasksDeleted { get; set; }
        public List<string> Warnings { get; set; } = [];
    }

    public async Task<IActionResult> OnGetAsync(int projectId)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null) return NotFound();

        ProjectId   = projectId;
        ProjectName = project.Name;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int projectId)
    {
        var project = await _db.Projects.FindAsync(projectId);
        if (project == null) return NotFound();

        ProjectId   = projectId;
        ProjectName = project.Name;

        if (XmlFile == null || XmlFile.Length == 0)
        {
            ErrorMessage = "XML 파일을 선택해 주세요.";
            return Page();
        }

        if (!XmlFile.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "MS Project XML 파일(.xml)만 가능합니다.";
            return Page();
        }

        try
        {
            using var stream = XmlFile.OpenReadStream();
            var doc = XDocument.Load(stream);

            // MS Project XML namespace
            XNamespace ns = "http://schemas.microsoft.com/project";

            var taskElements = doc.Root?.Element(ns + "Tasks")?.Elements(ns + "Task").ToList()
                               ?? [];

            var warnings = new List<string>();

            // ── 1. 기존 태스크 삭제 (옵션) ──────────────────────────────────
            int deletedCount = 0;
            if (ReplaceExisting)
            {
                // 1-a. TaskDependency 먼저 삭제 (FK: SourceId/TargetId → ScheduleTask)
                var existingTaskIds = await _db.ScheduleTasks
                    .Where(t => t.ProjectId == projectId)
                    .Select(t => t.Id)
                    .ToListAsync();

                deletedCount = existingTaskIds.Count;

                await _db.TaskDependencies
                    .Where(d => existingTaskIds.Contains(d.SourceId) || existingTaskIds.Contains(d.TargetId))
                    .ExecuteDeleteAsync();

                // 1-b. ParentId 자기참조 FK를 먼저 NULL로 → 그래야 DELETE 순서 무관
                await _db.ScheduleTasks
                    .Where(t => t.ProjectId == projectId && t.ParentId != null)
                    .ExecuteUpdateAsync(s => s.SetProperty(t => t.ParentId, (int?)null));

                // 1-c. 태스크 삭제 (단일 DELETE WHERE → FK 순서 문제 없음)
                await _db.ScheduleTasks
                    .Where(t => t.ProjectId == projectId)
                    .ExecuteDeleteAsync();
            }

            // ── 2. 태스크 파싱 ────────────────────────────────────────────────
            // uid → ScheduleTask 매핑 (dependency 처리용)
            var uidToTask = new Dictionary<int, ScheduleTask>();
            // uid → OutlineNumber 매핑 (parent 찾기용)
            var uidToOutline = new Dictionary<int, string>();

            int sortOrder = 0;
            foreach (var el in taskElements)
            {
                var uid = (int?)el.Element(ns + "UID") ?? 0;
                if (uid == 0) continue; // 프로젝트 요약 태스크 스킵

                var name = (string?)el.Element(ns + "Name") ?? "(Unnamed)";
                if (string.IsNullOrWhiteSpace(name)) continue;

                var startStr  = (string?)el.Element(ns + "Start");
                var finishStr = (string?)el.Element(ns + "Finish");
                var isSummary   = (string?)el.Element(ns + "Summary")   == "1";
                var isMilestone = (string?)el.Element(ns + "Milestone") == "1";
                var pctComplete = (double?)el.Element(ns + "PercentComplete") ?? 0;
                var outlineNum  = (string?)el.Element(ns + "OutlineNumber") ?? "";
                var notes       = (string?)el.Element(ns + "Notes");

                // MS Project XML 날짜 파싱:
                // - 정상: "2024-01-22T08:00:00" 또는 "2024-01-22T08:00:00-08:00"
                // - Summary 태스크: "NA" 또는 빈 문자열 → 자식에서 계산됨
                var startDt  = ParseMspDate(startStr);
                var finishDt = ParseMspDate(finishStr);

                // 날짜 없는 태스크(NA) 처리: start 없으면 skip하지 말고 일단 오늘로
                if (startDt == null && finishDt == null)
                {
                    warnings.Add($"Task \"{name}\" (UID={uid}): Start/Finish 날짜 없음 — 임시로 오늘 날짜 사용");
                }
                DateTime startDtVal  = startDt  ?? DateTime.Today;
                DateTime finishDtVal = finishDt ?? startDtVal.AddDays(1);

                var startDate = DateOnly.FromDateTime(startDtVal);
                var endDate   = DateOnly.FromDateTime(finishDtVal);
                int duration  = Math.Max(1, (endDate.ToDateTime(TimeOnly.MinValue) - startDtVal.Date).Days);

                var taskType = isMilestone ? GanttTaskType.Milestone
                             : isSummary   ? GanttTaskType.Project
                             :               GanttTaskType.Task;

                var task = new ScheduleTask
                {
                    ProjectId   = projectId,
                    Text        = name,
                    WbsCode     = outlineNum,
                    StartDate   = startDate,
                    EndDate     = endDate,
                    Duration    = duration,
                    Progress    = Math.Clamp(pctComplete / 100.0, 0, 1),
                    TaskType    = taskType,
                    IsOpen      = true,
                    SortOrder   = sortOrder++,
                    Notes       = notes,
                    CreatedAt   = DateTime.UtcNow,
                    UpdatedAt   = DateTime.UtcNow,
                    IsActive    = true,
                };

                _db.ScheduleTasks.Add(task);
                uidToTask[uid]    = task;
                uidToOutline[uid] = outlineNum;
            }

            await _db.SaveChangesAsync();

            // ── 3. 부모-자식 관계 설정 ────────────────────────────────────────
            // OutlineNumber로 부모 찾기: "1.2.3" → 부모는 "1.2"
            var outlineToTask = uidToTask
                .Where(kv => !string.IsNullOrEmpty(uidToOutline[kv.Key]))
                .ToDictionary(kv => uidToOutline[kv.Key], kv => kv.Value);

            foreach (var kv in uidToTask)
            {
                var outline = uidToOutline[kv.Key];
                if (string.IsNullOrEmpty(outline)) continue;

                var lastDot = outline.LastIndexOf('.');
                if (lastDot <= 0) continue; // 최상위 레벨

                var parentOutline = outline[..lastDot];
                if (outlineToTask.TryGetValue(parentOutline, out var parentTask))
                {
                    kv.Value.ParentId = parentTask.Id;
                }
            }

            await _db.SaveChangesAsync();

            // ── 4. Dependency(링크) 파싱 ──────────────────────────────────────
            int linkCount = 0;
            foreach (var el in taskElements)
            {
                var uid = (int?)el.Element(ns + "UID") ?? 0;
                if (uid == 0) continue;
                if (!uidToTask.TryGetValue(uid, out var targetTask)) continue;

                foreach (var predEl in el.Elements(ns + "PredecessorLink"))
                {
                    var predUid = (int?)predEl.Element(ns + "PredecessorUID") ?? 0;
                    if (predUid == 0) continue;
                    if (!uidToTask.TryGetValue(predUid, out var sourceTask))
                    {
                        warnings.Add($"Predecessor UID {predUid} not found — skipped");
                        continue;
                    }

                    // MS Project type: 0=FF, 1=FS, 2=SF, 3=SS
                    var msType = (int?)predEl.Element(ns + "Type") ?? 1;
                    var depType = msType switch
                    {
                        0 => DependencyType.FinishToFinish,
                        1 => DependencyType.FinishToStart,
                        2 => DependencyType.StartToFinish,
                        3 => DependencyType.StartToStart,
                        _ => DependencyType.FinishToStart
                    };

                    // Lag: MS Project stores in tenths of minutes
                    var lagRaw = (int?)predEl.Element(ns + "LinkLag") ?? 0;
                    int lagDays = lagRaw / (60 * 8 * 10); // 10ths of min → work days (8h)

                    _db.TaskDependencies.Add(new TaskDependency
                    {
                        SourceId = sourceTask.Id,
                        TargetId = targetTask.Id,
                        Type     = depType,
                        Lag      = lagDays
                    });
                    linkCount++;
                }
            }

            await _db.SaveChangesAsync();

            Result = new ImportResult
            {
                TasksImported = uidToTask.Count,
                LinksImported = linkCount,
                TasksDeleted  = deletedCount,
                Warnings      = warnings
            };

            TempData["ImportSuccess"] =
                $"{uidToTask.Count}개 태스크, {linkCount}개 dependency 임포트 완료.";

            return Page();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"XML 파싱 오류: {ex.Message}";
            return Page();
        }
    }
}
