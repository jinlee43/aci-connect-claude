using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Services;

// ── DTOs ─────────────────────────────────────────────────────────────────────

/// <summary>
/// Safety staff 전체 그리드용 행 데이터.
/// 행 = 프로젝트, 열 = 주(WeekStartDate).
/// WeekSlots: WeekStartDate → 해당 주 보고서 (null = 레코드 없음).
/// </summary>
public record SafetyWkRepGridRowDto(
    int    ProjectId,
    string ProjectCode,
    string ProjectName,
    SafetyWkRepSettings? Settings,
    Dictionary<DateOnly, SafetyWkRep?> WeekSlots,
    bool   CanReviewThisRow);

// ── Interface ─────────────────────────────────────────────────────────────────

public interface ISafetyWkRepService
{
    // ── Settings ─────────────────────────────────────────────────────────────
    Task<SafetyWkRepSettings?> GetSettingsAsync(int projectId);
    Task<SafetyWkRepSettings>  SaveSettingsAsync(
        int projectId, DateOnly startDate, DateOnly? endDate,
        DayOfWeek defaultSubmitDay, string? notes,
        int userId, string userName);
    Task<SafetyWkRepSettings> ApproveSettingsAsync(int settingsId, int userId, string userName);
    Task<SafetyWkRepSettings> VoidSettingsApprovalAsync(int settingsId, int userId, string userName);

    // ── Report queries ────────────────────────────────────────────────────────
    Task<SafetyWkRep?>          GetReportAsync(int reportId);
    Task<SafetyWkRep?>          GetReportByWeekAsync(int projectId, DateOnly weekStartDate);
    Task<List<SafetyWkRep>>     GetProjectReportsAsync(int projectId, DateOnly from, DateOnly to);
    Task<List<SafetyWkRep>>     GetMyReportsAsync(int userId, DateOnly from, DateOnly to);
    Task<List<SafetyWkRepGridRowDto>> GetWeeklyGridAsync(DateOnly from, DateOnly to, int userId, bool isAdmin);

    // ── File queries ──────────────────────────────────────────────────────────
    Task<SafetyWkRepFile?> GetFileAsync(int fileId);

    // ── Assigned project access (PM / Superintendent) ─────────────────────────
    Task<List<int>> GetAssignedProjectIdsAsync(int userId);

    // ── Report actions ────────────────────────────────────────────────────────

    /// <summary>
    /// 파일을 업로드하고 Staged 보고서에 추가합니다.
    /// 레코드가 없으면 Staged 상태로 신규 생성합니다.
    /// Voided 보고서에 추가하면 Staged 로 초기화됩니다.
    /// </summary>
    Task<SafetyWkRep> AddFileAsync(
        int projectId, DateOnly weekStartDate,
        string fileName, string storedFileName, string extension, long fileSize,
        int userId, string userName, DateOnly? reportDate = null, string? notes = null);

    /// <summary>개별 파일을 삭제합니다. 반환된 StoredFileName 으로 디스크 파일을 삭제하세요.</summary>
    Task<(SafetyWkRepFile File, string StoredFileName)> RemoveFileAsync(int fileId);

    /// <summary>Staged → Draft (제출). 파일이 1개 이상 있어야 합니다.</summary>
    Task<SafetyWkRep> SubmitReportAsync(int reportId, DateOnly? reportDate, int userId, string userName);

    Task<SafetyWkRep> MarkNoWorkAsync(int projectId, DateOnly weekStartDate, int userId, string userName, DateOnly? reportDate = null, string? notes = null);
    Task<(SafetyWkRep Report, IReadOnlyList<string> StoredFileNames)> DeleteReportAsync(int reportId);
    Task<SafetyWkRep> ReviewReportAsync(int reportId, string? notes, int userId, string userName);
    Task<SafetyWkRep> UnreviewReportAsync(int reportId, int userId, string userName);
    Task<SafetyWkRep> ApproveReportAsync(int reportId, string? notes, int userId, string userName);
    Task<SafetyWkRep> VoidApprovalAsync(int reportId, string? reason, int userId, string userName);
    Task<SafetyWkRep> UnvoidAsync(int reportId, int userId, string userName);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class SafetyWkRepService : ISafetyWkRepService
{
    private readonly AppDbContext _db;

    public SafetyWkRepService(AppDbContext db) => _db = db;

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string StatusLabel(SafetyWkRepStatus s) => s switch
    {
        SafetyWkRepStatus.Staged         => "Staged",
        SafetyWkRepStatus.Draft          => "Draft",
        SafetyWkRepStatus.Reviewed       => "Reviewed",
        SafetyWkRepStatus.Approved       => "Approved",
        SafetyWkRepStatus.NoWork         => "No Work",
        SafetyWkRepStatus.NoWorkReviewed => "No Work — Reviewed",
        SafetyWkRepStatus.NoWorkApproved => "No Work — Approved",
        SafetyWkRepStatus.Voided         => "Voided",
        _                                => s.ToString()
    };

    /// <summary>현재 상태를 동사구("because ___")로 설명.</summary>
    private static string StatusReason(SafetyWkRepStatus s) => s switch
    {
        SafetyWkRepStatus.Staged         => "it has not been submitted yet",
        SafetyWkRepStatus.Draft          => "it has not been reviewed yet",
        SafetyWkRepStatus.Reviewed       => "it has already been reviewed",
        SafetyWkRepStatus.NoWork         => "it is marked as No Work and has not been reviewed yet",
        SafetyWkRepStatus.NoWorkReviewed => "it has already been reviewed (No Work)",
        SafetyWkRepStatus.Approved       => "it has already been approved",
        SafetyWkRepStatus.NoWorkApproved => "it has already been approved (No Work)",
        SafetyWkRepStatus.Voided         => "its approval has been voided",
        _                                => $"its current status is '{StatusLabel(s)}'"
    };

    private async Task SaveWithConcurrencyCheckAsync(SafetyWkRep report)
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Single();
            await entry.ReloadAsync();
            var current = (SafetyWkRep)entry.Entity;
            throw new InvalidOperationException(
                $"The report status was already changed to '{StatusLabel(current.Status)}' " +
                $"by another user. Please refresh the page and try again.");
        }
    }

    /// <summary>주어진 날짜가 속한 주의 월요일을 반환.</summary>
    public static DateOnly GetWeekMonday(DateOnly date)
    {
        int diff = (7 + ((int)date.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }

    /// <summary>월요일 기준으로 ISO 주차(1~53)와 연도를 반환.</summary>
    private static (int Year, int WeekNumber) GetIsoWeek(DateOnly monday)
    {
        var dt = monday.ToDateTime(TimeOnly.MinValue);
        var week = System.Globalization.ISOWeek.GetWeekOfYear(dt);
        var year = System.Globalization.ISOWeek.GetYear(dt);
        return (year, week);
    }

    private static SafetyWkRep BuildNewReport(int projectId, DateOnly weekStartDate)
    {
        var monday = GetWeekMonday(weekStartDate);
        var (year, week) = GetIsoWeek(monday);
        return new SafetyWkRep
        {
            ProjectId     = projectId,
            WeekStartDate = monday,
            WeekEndDate   = monday.AddDays(6),
            WeekNumber    = week,
            Year          = year,
            Status        = SafetyWkRepStatus.Staged,
            CreatedAt     = DateTime.UtcNow,
            UpdatedAt     = DateTime.UtcNow,
        };
    }

    // ── Settings ──────────────────────────────────────────────────────────────

    public async Task<SafetyWkRepSettings?> GetSettingsAsync(int projectId) =>
        await _db.SafetyWkRepSettings
            .Include(s => s.ApprovedBy)
            .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.IsActive);

    public async Task<SafetyWkRepSettings> SaveSettingsAsync(
        int projectId, DateOnly startDate, DateOnly? endDate,
        DayOfWeek defaultSubmitDay, string? notes,
        int userId, string userName)
    {
        var settings = await _db.SafetyWkRepSettings
            .FirstOrDefaultAsync(s => s.ProjectId == projectId && s.IsActive);

        if (settings != null && settings.IsApproved)
            throw new InvalidOperationException(
                "Settings are approved and locked. Void the approval before editing.");

        if (settings == null)
        {
            settings = new SafetyWkRepSettings
            {
                ProjectId  = projectId,
                CreatedAt  = DateTime.UtcNow,
                CreatedById = userId,
            };
            _db.SafetyWkRepSettings.Add(settings);
        }

        settings.StartDate        = startDate;
        settings.EndDate          = endDate;
        settings.DefaultSubmitDay = defaultSubmitDay;
        settings.Notes            = notes;
        settings.UpdatedAt        = DateTime.UtcNow;
        settings.UpdatedById      = userId;

        await _db.SaveChangesAsync();
        return settings;
    }

    public async Task<SafetyWkRepSettings> ApproveSettingsAsync(
        int settingsId, int userId, string userName)
    {
        var settings = await _db.SafetyWkRepSettings.FindAsync(settingsId)
            ?? throw new KeyNotFoundException($"Settings {settingsId} not found.");

        if (settings.IsApproved)
            throw new InvalidOperationException("Settings are already approved.");

        settings.IsApproved     = true;
        settings.ApprovedAt     = DateTime.UtcNow;
        settings.ApprovedById   = userId;
        settings.ApprovedByName = userName;
        settings.UpdatedAt      = DateTime.UtcNow;
        settings.UpdatedById    = userId;

        await _db.SaveChangesAsync();
        return settings;
    }

    public async Task<SafetyWkRepSettings> VoidSettingsApprovalAsync(
        int settingsId, int userId, string userName)
    {
        var settings = await _db.SafetyWkRepSettings.FindAsync(settingsId)
            ?? throw new KeyNotFoundException($"Settings {settingsId} not found.");

        if (!settings.IsApproved)
            throw new InvalidOperationException("Settings are not currently approved.");

        settings.IsApproved      = false;
        settings.ApprovedAt      = null;
        settings.ApprovedById    = null;
        settings.ApprovedByName  = null;
        settings.RevisionNumber += 1;
        settings.UpdatedAt       = DateTime.UtcNow;
        settings.UpdatedById     = userId;

        await _db.SaveChangesAsync();
        return settings;
    }

    // ── Report queries ─────────────────────────────────────────────────────────

    public async Task<SafetyWkRep?> GetReportAsync(int reportId) =>
        await _db.SafetyWkReps
            .Include(r => r.Project)
            .Include(r => r.Files).ThenInclude(f => f.UploadedBy)
            .Include(r => r.UploadedBy)
            .Include(r => r.ReviewedBy)
            .Include(r => r.ApprovedBy)
            .Include(r => r.VoidedBy)
            .FirstOrDefaultAsync(r => r.Id == reportId && r.IsActive);

    public async Task<SafetyWkRep?> GetReportByWeekAsync(int projectId, DateOnly weekStartDate) =>
        await _db.SafetyWkReps
            .Include(r => r.Project)
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.ProjectId == projectId
                                   && r.WeekStartDate == GetWeekMonday(weekStartDate)
                                   && r.IsActive);

    public async Task<List<SafetyWkRep>> GetProjectReportsAsync(
        int projectId, DateOnly from, DateOnly to) =>
        await _db.SafetyWkReps
            .Include(r => r.Files)
            .Where(r => r.ProjectId == projectId
                     && r.IsActive
                     && r.WeekStartDate >= from
                     && r.WeekStartDate <= to)
            .OrderByDescending(r => r.WeekStartDate)
            .ToListAsync();

    public async Task<List<int>> GetAssignedProjectIdsAsync(int userId)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await _db.Users
            .Where(u => u.Id == userId && u.EmployeeId != null)
            .SelectMany(u => u.Employee!.EmpRoles)
            .Where(r => r.OrgUnit.ProjectId != null
                     && (r.EndDate == null || r.EndDate >= today))
            .Select(r => r.OrgUnit.ProjectId!.Value)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<SafetyWkRep>> GetMyReportsAsync(
        int userId, DateOnly from, DateOnly to)
    {
        var projectIds = await GetAssignedProjectIdsAsync(userId);
        return await _db.SafetyWkReps
            .Include(r => r.Project)
            .Include(r => r.Files)
            .Where(r => projectIds.Contains(r.ProjectId)
                     && r.IsActive
                     && r.WeekStartDate >= from
                     && r.WeekStartDate <= to)
            .OrderBy(r => r.ProjectId)
            .ThenByDescending(r => r.WeekStartDate)
            .ToListAsync();
    }

    public async Task<List<SafetyWkRepGridRowDto>> GetWeeklyGridAsync(
        DateOnly from, DateOnly to, int userId, bool isAdmin)
    {
        // 기간 내 월요일 목록 생성
        var weeks = new List<DateOnly>();
        var cursor = GetWeekMonday(from);
        while (cursor <= to)
        {
            weeks.Add(cursor);
            cursor = cursor.AddDays(7);
        }

        var projects = await _db.Projects
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProjectCode)
            .ToListAsync();

        var reports = await _db.SafetyWkReps
            .Include(r => r.Files)
            .Where(r => r.IsActive
                     && r.WeekStartDate >= GetWeekMonday(from)
                     && r.WeekStartDate <= to)
            .ToListAsync();

        var settingsList = await _db.SafetyWkRepSettings
            .Where(s => s.IsActive)
            .ToListAsync();

        var reportLookup = reports
            .GroupBy(r => r.ProjectId)
            .ToDictionary(g => g.Key,
                g => g.ToDictionary(r => r.WeekStartDate));

        var settingsLookup = settingsList
            .ToDictionary(s => s.ProjectId);

        var pmProjectIds = new HashSet<int>();
        if (!isAdmin && userId > 0)
        {
            var empId = await _db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.EmployeeId)
                .FirstOrDefaultAsync();

            if (empId.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var ids = await _db.EmpRoles
                    .Where(r => r.EmployeeId == empId.Value
                             && r.OrgUnit.ProjectId != null
                             && r.JobPositionId != null
                             && (r.JobPosition!.Code == "PM" || r.JobPosition!.Code == "SPM")
                             && (r.EndDate == null || r.EndDate >= today))
                    .Select(r => r.OrgUnit.ProjectId!.Value)
                    .Distinct()
                    .ToListAsync();
                pmProjectIds = ids.ToHashSet();
            }
        }

        return projects.Select(p =>
        {
            var byWeek = reportLookup.TryGetValue(p.Id, out var map) ? map : [];
            var slots  = weeks.ToDictionary(w => w, w => byWeek.TryGetValue(w, out var r) ? r : null);
            settingsLookup.TryGetValue(p.Id, out var settings);
            bool canReview = isAdmin || pmProjectIds.Contains(p.Id);
            return new SafetyWkRepGridRowDto(p.Id, p.ProjectCode, p.Name, settings, slots, canReview);
        }).ToList();
    }

    // ── File queries ──────────────────────────────────────────────────────────

    public async Task<SafetyWkRepFile?> GetFileAsync(int fileId) =>
        await _db.SafetyWkRepFiles
            .Include(f => f.Report).ThenInclude(r => r.Project)
            .FirstOrDefaultAsync(f => f.Id == fileId && f.IsActive);

    // ── Report actions ─────────────────────────────────────────────────────────

    public async Task<SafetyWkRep> AddFileAsync(
        int projectId, DateOnly weekStartDate,
        string fileName, string storedFileName, string extension, long fileSize,
        int userId, string userName, DateOnly? reportDate = null, string? notes = null)
    {
        var monday = GetWeekMonday(weekStartDate);
        var existing = await _db.SafetyWkReps
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.ProjectId == projectId
                                   && r.WeekStartDate == monday
                                   && r.IsActive);

        if (existing != null && existing.IsLocked)
            throw new InvalidOperationException(
                $"The report cannot be modified because {StatusReason(existing.Status)}.");

        if (existing != null && existing.IsNoWork)
            throw new InvalidOperationException(
                "Cannot add files to a No Work report. Cancel the No Work entry first.");

        SafetyWkRep report;

        if (existing == null)
        {
            // 신규 Staged 보고서 생성
            report = BuildNewReport(projectId, monday);
            report.ReportDate     = reportDate;
            report.UploadedById   = userId;
            report.UploadedByName = userName;
            report.UploadedAt     = DateTime.UtcNow;
            report.CreatedById    = userId;
            if (!string.IsNullOrWhiteSpace(notes))
                report.Notes = notes.Trim();
            _db.SafetyWkReps.Add(report);
            await _db.SaveChangesAsync(); // ID 확보 후 파일 추가
        }
        else if (existing.Status == SafetyWkRepStatus.Voided)
        {
            // Voided → 재제출: Staged 로 초기화
            existing.Status         = SafetyWkRepStatus.Staged;
            existing.UploadedById   = userId;
            existing.UploadedByName = userName;
            existing.UploadedAt     = DateTime.UtcNow;
            existing.UpdatedAt      = DateTime.UtcNow;
            existing.UpdatedById    = userId;
            if (reportDate.HasValue)
                existing.ReportDate = reportDate;
            if (!string.IsNullOrWhiteSpace(notes))
                existing.Notes = notes.Trim();
            report = existing;
        }
        else
        {
            // 기존 Staged / Draft 에 파일 추가
            report = existing;
            if (reportDate.HasValue)
            {
                report.ReportDate  = reportDate;
                report.UpdatedAt   = DateTime.UtcNow;
                report.UpdatedById = userId;
            }
            if (!string.IsNullOrWhiteSpace(notes))
            {
                report.Notes      = notes.Trim();
                report.UpdatedAt  = DateTime.UtcNow;
                report.UpdatedById = userId;
            }
        }

        var fileRecord = new SafetyWkRepFile
        {
            ReportId       = report.Id,
            FileName       = fileName,
            StoredFileName = storedFileName,
            Extension      = extension,
            FileSize       = fileSize,
            UploadedById   = userId,
            UploadedByName = userName,
            UploadedAt     = DateTime.UtcNow,
            CreatedAt      = DateTime.UtcNow,
            UpdatedAt      = DateTime.UtcNow,
            CreatedById    = userId,
        };
        _db.SafetyWkRepFiles.Add(fileRecord);
        await _db.SaveChangesAsync();

        report.Files.Add(fileRecord);
        return report;
    }

    public async Task<(SafetyWkRepFile File, string StoredFileName)> RemoveFileAsync(int fileId)
    {
        var file = await _db.SafetyWkRepFiles
            .Include(f => f.Report)
            .FirstOrDefaultAsync(f => f.Id == fileId && f.IsActive)
            ?? throw new KeyNotFoundException($"File {fileId} not found.");

        if (file.Report.IsLocked)
            throw new InvalidOperationException(
                $"Cannot remove files because {StatusReason(file.Report.Status)}.");

        var storedName = file.StoredFileName;
        _db.SafetyWkRepFiles.Remove(file);
        await _db.SaveChangesAsync();
        return (file, storedName);
    }

    public async Task<SafetyWkRep> SubmitReportAsync(
        int reportId, DateOnly? reportDate, int userId, string userName)
    {
        var report = await _db.SafetyWkReps
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.Id == reportId && r.IsActive)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        if (report.Status != SafetyWkRepStatus.Staged)
            throw new InvalidOperationException(
                $"The report cannot be submitted because {StatusReason(report.Status)}.");

        if (!report.Files.Any())
            throw new InvalidOperationException(
                "Cannot submit a report with no files. Please upload at least one file first.");

        report.Status      = SafetyWkRepStatus.Draft;
        report.UpdatedAt   = DateTime.UtcNow;
        report.UpdatedById = userId;

        if (reportDate.HasValue)
            report.ReportDate = reportDate;

        await SaveWithConcurrencyCheckAsync(report);
        return report;
    }

    public async Task<SafetyWkRep> MarkNoWorkAsync(
        int projectId, DateOnly weekStartDate, int userId, string userName,
        DateOnly? reportDate = null, string? notes = null)
    {
        var monday = GetWeekMonday(weekStartDate);
        var existing = await _db.SafetyWkReps
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.ProjectId == projectId
                                   && r.WeekStartDate == monday
                                   && r.IsActive);

        if (existing != null && existing.IsLocked)
            throw new InvalidOperationException("Report is approved and locked.");

        if (existing != null)
        {
            // Staged 파일이 있으면 DB에서 제거 (디스크 파일은 호출자가 처리)
            if (existing.Files.Any())
                _db.SafetyWkRepFiles.RemoveRange(existing.Files);

            existing.Status         = SafetyWkRepStatus.NoWork;
            existing.ReportDate     = reportDate;
            existing.UploadedById   = userId;
            existing.UploadedByName = userName;
            existing.UploadedAt     = DateTime.UtcNow;
            existing.Notes          = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            existing.ReviewedById   = null;
            existing.ReviewedByName = null;
            existing.ReviewedAt     = null;
            existing.ReviewNotes    = null;
            existing.UpdatedAt      = DateTime.UtcNow;
            existing.UpdatedById    = userId;
            await _db.SaveChangesAsync();
            return existing;
        }

        var report = BuildNewReport(projectId, monday);
        report.Status         = SafetyWkRepStatus.NoWork;
        report.ReportDate     = reportDate;
        report.UploadedById   = userId;
        report.UploadedByName = userName;
        report.UploadedAt     = DateTime.UtcNow;
        report.Notes          = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        report.CreatedById    = userId;

        _db.SafetyWkReps.Add(report);
        await _db.SaveChangesAsync();
        return report;
    }

    /// <summary>
    /// 보고서를 삭제합니다 (Staged / Draft / NoWork / Voided 상태만 가능).
    /// 반환된 StoredFileNames 으로 디스크 파일을 삭제하세요.
    /// </summary>
    public async Task<(SafetyWkRep Report, IReadOnlyList<string> StoredFileNames)> DeleteReportAsync(
        int reportId)
    {
        var report = await _db.SafetyWkReps
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.Id == reportId && r.IsActive)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        if (report.IsLocked)
            throw new InvalidOperationException(
                $"The report cannot be deleted because {StatusReason(report.Status)}. Void the approval first.");

        var storedNames = report.Files.Select(f => f.StoredFileName).ToList();
        _db.SafetyWkReps.Remove(report);
        await _db.SaveChangesAsync();
        return (report, storedNames);
    }

    public async Task<SafetyWkRep> ReviewReportAsync(
        int reportId, string? notes, int userId, string userName)
    {
        var report = await _db.SafetyWkReps.FindAsync(reportId)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        // Review 는 Draft / NoWork 상태에서만 허용
        if (report.Status is not (SafetyWkRepStatus.Draft or SafetyWkRepStatus.NoWork))
            throw new InvalidOperationException(
                $"The report cannot be reviewed because {StatusReason(report.Status)}.");

        report.Status = report.Status == SafetyWkRepStatus.NoWork
            ? SafetyWkRepStatus.NoWorkReviewed
            : SafetyWkRepStatus.Reviewed;
        report.ReviewedById   = userId;
        report.ReviewedByName = userName;
        report.ReviewedAt     = DateTime.UtcNow;
        report.ReviewNotes    = notes;
        report.UpdatedAt      = DateTime.UtcNow;
        report.UpdatedById    = userId;

        await SaveWithConcurrencyCheckAsync(report);
        return report;
    }

    public async Task<SafetyWkRep> UnreviewReportAsync(int reportId, int userId, string userName)
    {
        var report = await _db.SafetyWkReps.FindAsync(reportId)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        if (report.Status is not (SafetyWkRepStatus.Reviewed or SafetyWkRepStatus.NoWorkReviewed))
            throw new InvalidOperationException(
                $"The review cannot be cleared because {StatusReason(report.Status)}.");

        report.Status = report.Status == SafetyWkRepStatus.NoWorkReviewed
            ? SafetyWkRepStatus.NoWork
            : SafetyWkRepStatus.Draft;
        report.ReviewedById   = null;
        report.ReviewedByName = null;
        report.ReviewedAt     = null;
        report.ReviewNotes    = null;
        report.UpdatedAt      = DateTime.UtcNow;
        report.UpdatedById    = userId;

        await SaveWithConcurrencyCheckAsync(report);
        return report;
    }

    public async Task<SafetyWkRep> ApproveReportAsync(
        int reportId, string? notes, int userId, string userName)
    {
        var report = await _db.SafetyWkReps.FindAsync(reportId)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        if (report.Status is not (SafetyWkRepStatus.Draft
                               or SafetyWkRepStatus.Reviewed
                               or SafetyWkRepStatus.NoWork
                               or SafetyWkRepStatus.NoWorkReviewed))
            throw new InvalidOperationException(
                $"The report cannot be approved because {StatusReason(report.Status)}.");

        report.Status = report.Status is SafetyWkRepStatus.NoWork or SafetyWkRepStatus.NoWorkReviewed
            ? SafetyWkRepStatus.NoWorkApproved
            : SafetyWkRepStatus.Approved;

        report.ApprovedById   = userId;
        report.ApprovedByName = userName;
        report.ApprovedAt     = DateTime.UtcNow;
        report.ApprovalNotes  = notes;
        report.UpdatedAt      = DateTime.UtcNow;
        report.UpdatedById    = userId;

        await SaveWithConcurrencyCheckAsync(report);
        return report;
    }

    public async Task<SafetyWkRep> VoidApprovalAsync(
        int reportId, string? reason, int userId, string userName)
    {
        var report = await _db.SafetyWkReps.FindAsync(reportId)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        if (report.Status is not (SafetyWkRepStatus.Approved or SafetyWkRepStatus.NoWorkApproved))
            throw new InvalidOperationException(
                $"The report cannot be voided because {StatusReason(report.Status)}.");

        report.Status       = SafetyWkRepStatus.Voided;
        report.VoidedById   = userId;
        report.VoidedByName = userName;
        report.VoidedAt     = DateTime.UtcNow;
        report.VoidReason   = reason;
        report.UpdatedAt    = DateTime.UtcNow;
        report.UpdatedById  = userId;

        await SaveWithConcurrencyCheckAsync(report);
        return report;
    }

    public async Task<SafetyWkRep> UnvoidAsync(int reportId, int userId, string userName)
    {
        var report = await _db.SafetyWkReps
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.Id == reportId && r.IsActive)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        if (report.Status != SafetyWkRepStatus.Voided)
            throw new InvalidOperationException(
                $"The approval cannot be restored because {StatusReason(report.Status)}.");

        // 파일 있으면 Draft 로 복원 (리뷰 플로우 재진입), 없으면 Staged
        report.Status = report.Files.Any()
            ? SafetyWkRepStatus.Draft
            : SafetyWkRepStatus.Staged;

        // Staged 복원 시 리뷰/승인 감사 필드 초기화 (이전 사이클 데이터 노출 방지)
        if (report.Status == SafetyWkRepStatus.Staged)
        {
            report.ReviewedById   = null;
            report.ReviewedByName = null;
            report.ReviewedAt     = null;
            report.ReviewNotes    = null;
            report.ApprovedById   = null;
            report.ApprovedByName = null;
            report.ApprovedAt     = null;
            report.ApprovalNotes  = null;
        }

        // Void 이력은 감사 목적으로 보존
        report.UpdatedAt   = DateTime.UtcNow;
        report.UpdatedById = userId;

        await SaveWithConcurrencyCheckAsync(report);
        return report;
    }
}
