using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Daily;

[Authorize]
public class ReportModel : PageModel
{
    private readonly IDailyReportService _svc;
    private readonly AppDbContext        _db;

    public ReportModel(IDailyReportService svc, AppDbContext db)
    {
        _svc = svc;
        _db  = db;
    }

    // ── 라우트 파라미터 ──────────────────────────────────────────────────────
    [BindProperty(SupportsGet = true)] public int?    ReportId  { get; set; }
    [BindProperty(SupportsGet = true)] public int?    ProjectId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Date      { get; set; }
    [BindProperty(SupportsGet = true)] public bool    Modal     { get; set; }

    // ── 뷰 데이터 ────────────────────────────────────────────────────────────
    public DailyReport?        Report       { get; set; }
    public List<Trade>         Trades       { get; set; } = [];
    /// <summary>리포트 날짜까지 시작됐고 미완료인 모든 Task.</summary>
    public List<WorkingTask>   ActiveTasks  { get; set; } = [];
    /// <summary>기존 저장된 TaskProgress — TaskId → 엔티티 (편집 화면용).</summary>
    public Dictionary<int, DailyReportTaskProgress> TaskProgressMap { get; set; } = [];

    public bool CanWrite   { get; set; }  // Superintendent
    public bool CanReview  { get; set; }  // PE / PM
    public bool CanApprove { get; set; }  // PM
    public bool CanVoid    { get; set; }  // PM

    public bool IsNew => Report == null || Report.Id == 0;

    /// <summary>Project SiteAddress + City + State — Location/Area 기본값.</summary>
    public string DefaultLocation { get; set; } = "";
    /// <summary>날씨 자동 조회용 GPS 위도 (Project.Latitude).</summary>
    public double? SiteLat { get; set; }
    /// <summary>날씨 자동 조회용 GPS 경도 (Project.Longitude).</summary>
    public double? SiteLon { get; set; }
    /// <summary>좌표 없을 때 Nominatim fallback 용 주소 문자열.</summary>
    public string WeatherAddress  { get; set; } = "";
    /// <summary>페이지 헤더용 프로젝트 코드.</summary>
    public string ProjectCode { get; set; } = "";
    /// <summary>페이지 헤더용 프로젝트 이름.</summary>
    public string ProjectName { get; set; } = "";
    /// <summary>페이지 헤더용 PM 이름.</summary>
    public string? PmName { get; set; }
    /// <summary>DB에서 찾은 날씨 캐시 (신규 report 작성 시 폼 자동 채우기용).</summary>
    public WeatherCache? CachedWeather { get; set; }

    // ── GET ───────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync()
    {
        var (userId, _) = GetUser();
        if (userId <= 0) return Forbid();

        SetPermissions();

        if (ReportId.HasValue && ReportId > 0)
        {
            Report = await _svc.GetReportAsync(ReportId.Value);
            if (Report == null) return NotFound();
            ProjectId = Report.ProjectId;
        }
        else if (ProjectId.HasValue && !string.IsNullOrEmpty(Date))
        {
            if (!DateOnly.TryParse(Date, out var date)) return BadRequest("Invalid date.");
            Report = await _svc.GetReportByDateAsync(ProjectId.Value, date);
            // Report가 없으면 null (새 작성 화면)
        }
        else
        {
            return BadRequest("Either reportId or projectId+date is required.");
        }

        // Admin / HrAdmin은 모든 프로젝트 읽기 가능; 그 외는 담당 프로젝트만
        var pid = ProjectId ?? Report?.ProjectId;
        if (pid.HasValue
            && !User.IsInRole(PrivilegeCodes.Admin)
            && !User.IsInRole(PrivilegeCodes.HrAdmin))
        {
            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(pid.Value)) return Forbid();
        }

        // 리포트 날짜 확정 (기존 리포트 or URL 파라미터 or 오늘)
        var reportDate = Report?.ReportDate
            ?? (DateOnly.TryParse(Date, out var pd) ? pd : DateOnly.FromDateTime(DateTime.Today));

        await LoadFormDataAsync(pid ?? 0, reportDate);

        // 기존 저장된 TaskProgress → 빠른 조회용 Map
        if (Report != null)
            TaskProgressMap = Report.TaskProgress
                .Where(tp => tp.WorkingTaskId.HasValue)
                .ToDictionary(tp => tp.WorkingTaskId!.Value);

        return Page();
    }

    // ── POST: Save Draft ─────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostSaveAsync()
    {
        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();
        if (!User.IsInRole(PrivilegeCodes.Superintendent))
            return Forbid();

        var pid = await ResolveProjectIdAsync();
        if (!pid.HasValue) return BadRequest("Unable to determine project.");
        var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
        if (!assigned.Contains(pid.Value)) return Forbid();

        var report = await GetOrCreateReportAsync(userId, userName);
        if (report == null) return BadRequest("Unable to find or create report.");

        UpdateReportHeaderFromForm(report);

        var crew         = ParseCrewEntries();
        var workItems    = ParseWorkItems();
        var taskProgress = ParseTaskProgress();
        var equipment    = ParseEquipment();

        await _svc.SaveDraftAsync(report, crew, workItems, taskProgress, equipment);

        TempData["Success"] = "Daily report saved.";
        if (Modal)
        {
            NotifyParent();
            return RedirectToPage(new { reportId = report.Id, modal = true });
        }
        return RedirectToPage(new { reportId = report.Id });
    }

    // ── POST: Submit ─────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostSubmitAsync()
    {
        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();
        if (!User.IsInRole(PrivilegeCodes.Superintendent))
            return Forbid();

        var pid = await ResolveProjectIdAsync();
        if (!pid.HasValue) return BadRequest("Unable to determine project.");
        var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
        if (!assigned.Contains(pid.Value)) return Forbid();

        var report = await GetOrCreateReportAsync(userId, userName);
        if (report == null) return BadRequest();

        UpdateReportHeaderFromForm(report);
        var crew         = ParseCrewEntries();
        var workItems    = ParseWorkItems();
        var taskProgress = ParseTaskProgress();
        var equipment    = ParseEquipment();
        await _svc.SaveDraftAsync(report, crew, workItems, taskProgress, equipment);
        await _svc.SubmitAsync(report.Id, userId, userName);

        TempData["Success"] = "Daily report submitted for review.";
        if (Modal) { NotifyParent(); return RedirectToPage(new { reportId = report.Id, modal = true }); }
        return RedirectToPage(new { reportId = report.Id });
    }

    // ── POST: NoWork ─────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostNoWorkAsync(string? noWorkReason)
    {
        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();
        if (!User.IsInRole(PrivilegeCodes.Superintendent))
            return Forbid();

        int pid = ProjectId ?? 0;
        if (!DateOnly.TryParse(Date, out var date)) return BadRequest();
        if (ReportId.HasValue)
        {
            var rep = await _db.DailyReports.FindAsync(ReportId.Value);
            if (rep != null) { pid = rep.ProjectId; date = rep.ReportDate; }
        }

        if (pid <= 0) return BadRequest("Unable to determine project.");
        var assignedNW = await _svc.GetAssignedProjectIdsAsync(userId);
        if (!assignedNW.Contains(pid)) return Forbid();

        await _svc.MarkNoWorkAsync(pid, date, noWorkReason, userId, userName);
        TempData["Success"] = "Marked as No Work.";
        if (Modal) { NotifyParent(); return RedirectToPage(new { projectId = pid, date = date.ToString("yyyy-MM-dd"), modal = true }); }
        return RedirectToPage(new { projectId = pid, date = date.ToString("yyyy-MM-dd") });
    }

    // ── POST: Review ─────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostReviewAsync(string? reviewNotes)
    {
        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();
        if (!User.IsInRole(PrivilegeCodes.ProjectEngineer)
         && !User.IsInRole(PrivilegeCodes.ProjectManager))
            return Forbid();

        var pidRev = await ResolveProjectIdAsync();
        if (!pidRev.HasValue) return BadRequest("Unable to determine project.");
        var assignedRev = await _svc.GetAssignedProjectIdsAsync(userId);
        if (!assignedRev.Contains(pidRev.Value)) return Forbid();

        await _svc.ReviewAsync(ReportId!.Value, reviewNotes, userId, userName);
        TempData["Success"] = "Daily report reviewed.";
        if (Modal) { NotifyParent(); return RedirectToPage(new { reportId = ReportId, modal = true }); }
        return RedirectToPage(new { reportId = ReportId });
    }

    // ── POST: Approve ─────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostApproveAsync(string? approvalNotes)
    {
        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();
        if (!User.IsInRole(PrivilegeCodes.ProjectManager))
            return Forbid();

        var pidApv = await ResolveProjectIdAsync();
        if (!pidApv.HasValue) return BadRequest("Unable to determine project.");
        var assignedApv = await _svc.GetAssignedProjectIdsAsync(userId);
        if (!assignedApv.Contains(pidApv.Value)) return Forbid();

        await _svc.ApproveAsync(ReportId!.Value, approvalNotes, userId, userName);
        TempData["Success"] = "Daily report approved.";
        if (Modal) { NotifyParent(); return RedirectToPage(new { reportId = ReportId, modal = true }); }
        return RedirectToPage(new { reportId = ReportId });
    }

    // ── POST: Delete (Draft only) ─────────────────────────────────────────
    public async Task<IActionResult> OnPostDeleteAsync()
    {
        var (userId, _) = GetUser();
        if (userId <= 0) return Forbid();
        if (!User.IsInRole(PrivilegeCodes.Superintendent))
            return Forbid();

        if (!ReportId.HasValue || ReportId <= 0)
            return BadRequest("Report ID is required.");

        var pid = await ResolveProjectIdAsync();
        if (!pid.HasValue) return BadRequest("Unable to determine project.");
        var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
        if (!assigned.Contains(pid.Value)) return Forbid();

        try
        {
            await _svc.DeleteDraftAsync(ReportId.Value);
            TempData["Success"] = "Draft report deleted.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage(new { reportId = ReportId });
        }

        return RedirectToPage("/Daily/Index", new { filterProjectId = pid });
    }

    // ── POST: Void ─────────────────────────────────────────────────────────
    public async Task<IActionResult> OnPostVoidAsync(string? voidReason)
    {
        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();
        if (!User.IsInRole(PrivilegeCodes.ProjectManager))
            return Forbid();

        var pidVoid = await ResolveProjectIdAsync();
        if (!pidVoid.HasValue) return BadRequest("Unable to determine project.");
        var assignedVoid = await _svc.GetAssignedProjectIdsAsync(userId);
        if (!assignedVoid.Contains(pidVoid.Value)) return Forbid();

        await _svc.VoidAsync(ReportId!.Value, voidReason, userId, userName);
        TempData["Success"] = "Daily report voided.";
        if (Modal) { NotifyParent(); return RedirectToPage(new { reportId = ReportId, modal = true }); }
        return RedirectToPage(new { reportId = ReportId });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private (int userId, string userName) GetUser()
    {
        var idStr = User.FindFirst("UserId")?.Value
                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id) || id <= 0) return (0, "");
        return (id, User.Identity?.Name ?? "Unknown");
    }

    private void SetPermissions()
    {
        CanWrite   = User.IsInRole(PrivilegeCodes.Superintendent);
        CanReview  = User.IsInRole(PrivilegeCodes.ProjectEngineer)
                  || User.IsInRole(PrivilegeCodes.ProjectManager);
        CanApprove = User.IsInRole(PrivilegeCodes.ProjectManager);
        CanVoid    = CanApprove;
    }

    private async Task LoadFormDataAsync(int projectId, DateOnly reportDate)
    {
        if (projectId <= 0) return;
        Trades = await _db.Trades
            .Where(t => t.ProjectId == projectId && t.IsActive)
            .OrderBy(t => t.Name)
            .ToListAsync();

        // 리포트 날짜까지 시작됐고 아직 완료되지 않은 Task 전체
        ActiveTasks = await _db.WorkingTasks
            .Where(t => t.ProjectId == projectId
                     && t.IsActive
                     && t.TaskType == GanttTaskType.Task
                     && t.StartDate <= reportDate
                     && !t.IsDone)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.Text)
            .ToListAsync();

        // 프로젝트 주소 → Location 기본값, 좌표 → 날씨 조회, 코드/이름/PM → 헤더
        var project = await _db.Projects.FindAsync(projectId);
        if (project != null)
        {
            ProjectCode = project.ProjectCode;
            ProjectName = project.Name;

            DefaultLocation = string.Join(", ",
                new[] { project.SiteAddress, project.City, project.State }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

            if (project.Latitude.HasValue && project.Longitude.HasValue)
            {
                SiteLat = project.Latitude;
                SiteLon = project.Longitude;
            }
            else
            {
                WeatherAddress = string.Join(", ",
                    new[] { project.SiteAddress, project.City, project.State, project.ZipCode }
                        .Where(s => !string.IsNullOrWhiteSpace(s)));
            }

            // 날씨 캐시 조회 (신규 report 작성 시에만)
            if (Report == null)
            {
                CachedWeather = await _db.WeatherCache
                    .FirstOrDefaultAsync(w => w.ProjectId == projectId && w.Date == reportDate);
            }

            // PM 이름 조회 (JobPosition Code: PM / SPM / APM)
            var today = DateOnly.FromDateTime(DateTime.Today);
            var pm = await _db.EmpRoles
                .Where(r => r.OrgUnit.ProjectId == projectId
                         && r.JobPositionId != null
                         && (r.JobPosition!.Code == "PM" || r.JobPosition!.Code == "SPM" || r.JobPosition!.Code == "APM")
                         && (r.EndDate == null || r.EndDate >= today))
                .Select(r => new { r.Employee.FirstName, r.Employee.LastName })
                .FirstOrDefaultAsync();
            if (pm != null)
                PmName = $"{pm.FirstName} {pm.LastName}".Trim();
        }
    }

    private async Task<DailyReport?> GetOrCreateReportAsync(int userId, string userName)
    {
        if (ReportId.HasValue && ReportId > 0)
            return await _db.DailyReports
                .Include(r => r.CrewEntries)
                .Include(r => r.WorkItems)
                .Include(r => r.TaskProgress)
                .Include(r => r.Equipment)
                .Include(r => r.Files)
                .FirstOrDefaultAsync(r => r.Id == ReportId.Value && r.IsActive);

        if (!ProjectId.HasValue || string.IsNullOrEmpty(Date)) return null;
        if (!DateOnly.TryParse(Date, out var date)) return null;
        return await _svc.GetOrCreateDraftAsync(ProjectId.Value, date, userId, userName);
    }

    private void UpdateReportHeaderFromForm(DailyReport r)
    {
        r.Location        = Request.Form["Location"].FirstOrDefault();
        r.WeatherCondition = Request.Form["WeatherCondition"].FirstOrDefault();
        r.TempHigh        = int.TryParse(Request.Form["TempHigh"], out var hi)  ? hi  : null;
        r.TempLow         = int.TryParse(Request.Form["TempLow"],  out var lo)  ? lo  : null;
        r.IsWindy         = Request.Form["IsWindy"] == "true";
        r.IsRainy         = Request.Form["IsRainy"] == "true";
        r.WeatherNotes    = Request.Form["WeatherNotes"].FirstOrDefault();
        r.Notes           = Request.Form["Notes"].FirstOrDefault();
    }

    // Crew 섹션 제거 — Work Performed에 Company/Hours 통합됨
    private static List<DailyReportCrewEntry> ParseCrewEntries() => [];

    private List<DailyReportWorkItem> ParseWorkItems()
    {
        var list      = new List<DailyReportWorkItem>();
        var trades    = Request.Form["work_trade"];
        var areas     = Request.Form["work_area"];
        var companies = Request.Form["work_company"];
        var counts    = Request.Form["work_count"];
        var starts    = Request.Form["work_start"];
        var ends      = Request.Form["work_end"];
        var hours     = Request.Form["work_hours"];
        var descs     = Request.Form["work_desc"];

        // Trade 이름 → TradeId 매핑 (프로젝트 Trade 목록 캐시 활용)
        var tradeMap = Trades.ToDictionary(t => t.Name, t => t.Id, StringComparer.OrdinalIgnoreCase);

        int count = Math.Max(trades.Count, descs.Count);
        for (int i = 0; i < count; i++)
        {
            var tradeText = trades.ElementAtOrDefault(i);
            var desc      = descs.ElementAtOrDefault(i);
            // Trade 또는 Description 중 하나라도 있어야 행 저장
            if (string.IsNullOrWhiteSpace(tradeText) && string.IsNullOrWhiteSpace(desc)) continue;

            var startStr = starts.ElementAtOrDefault(i);
            var endStr   = ends.ElementAtOrDefault(i);
            TimeOnly? startTime = TimeOnly.TryParse(startStr, out var st) ? st : null;
            TimeOnly? endTime   = TimeOnly.TryParse(endStr,   out var et) ? et : null;
            int? workerCount = int.TryParse(counts.ElementAtOrDefault(i), out var wc) && wc > 0 ? wc : null;

            // Man-Hours: JS 계산값 우선, 없으면 서버에서 재계산
            decimal? wh = null;
            if (decimal.TryParse(hours.ElementAtOrDefault(i), out var whVal) && whVal > 0)
                wh = whVal;
            else if (startTime.HasValue && endTime.HasValue && workerCount.HasValue)
                wh = (decimal)(endTime.Value - startTime.Value).TotalHours * workerCount.Value;

            list.Add(new DailyReportWorkItem
            {
                TradeText   = string.IsNullOrWhiteSpace(tradeText) ? null : tradeText,
                TradeId     = !string.IsNullOrWhiteSpace(tradeText) && tradeMap.TryGetValue(tradeText, out var tid) ? tid : null,
                Area        = areas.ElementAtOrDefault(i),
                CompanyName = companies.ElementAtOrDefault(i),
                WorkerCount = workerCount,
                StartTime   = startTime,
                EndTime     = endTime,
                WorkerHours = wh,
                Description = desc ?? "",
                SortOrder   = i,
            });
        }
        return list;
    }

    private List<DailyReportTaskProgress> ParseTaskProgress()
    {
        var list    = new List<DailyReportTaskProgress>();
        var taskIds = Request.Form["tp_taskId"];
        var befores = Request.Form["tp_before"];
        var afters  = Request.Form["tp_after"];
        var txts    = Request.Form["tp_text"];
        var notes   = Request.Form["tp_notes"];

        for (int i = 0; i < taskIds.Count; i++)
        {
            if (!double.TryParse(afters.ElementAtOrDefault(i), out var af)) continue;
            double.TryParse(befores.ElementAtOrDefault(i), out var bef);
            list.Add(new DailyReportTaskProgress
            {
                WorkingTaskId  = int.TryParse(taskIds.ElementAtOrDefault(i), out var tid) && tid > 0 ? tid : null,
                TaskText       = txts.ElementAtOrDefault(i),
                ProgressBefore = bef / 100.0,
                ProgressAfter  = af  / 100.0,
                Notes          = notes.ElementAtOrDefault(i),
                SortOrder      = i,
            });
        }
        return list;
    }

    private List<DailyReportEquipment> ParseEquipment()
    {
        var list  = new List<DailyReportEquipment>();
        var names = Request.Form["eq_name"];
        var tags  = Request.Form["eq_tag"];
        var hours = Request.Form["eq_hours"];
        var notes = Request.Form["eq_notes"];

        for (int i = 0; i < names.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(names[i])) continue;
            list.Add(new DailyReportEquipment
            {
                Name         = names[i]!,
                EquipmentTag = tags.ElementAtOrDefault(i),
                HoursUsed    = decimal.TryParse(hours.ElementAtOrDefault(i), out var h) ? h : 8,
                Notes        = notes.ElementAtOrDefault(i),
                SortOrder    = i,
            });
        }
        return list;
    }

    /// <summary>
    /// POST 핸들러에서 assignment 체크를 위해 ProjectId를 확인.
    /// ReportId가 있으면 DB에서 조회, 없으면 바운드 ProjectId 사용.
    /// </summary>
    private async Task<int?> ResolveProjectIdAsync()
    {
        if (ReportId.HasValue && ReportId > 0)
            return await _db.DailyReports
                .Where(r => r.Id == ReportId.Value && r.IsActive)
                .Select(r => (int?)r.ProjectId)
                .FirstOrDefaultAsync();
        return ProjectId;
    }

    private void NotifyParent()
    {
        // JS postMessage는 View에서 처리
    }
}
