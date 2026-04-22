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

    // ── Assigned project access (PM / Superintendent) ─────────────────────────
    Task<List<int>> GetAssignedProjectIdsAsync(int userId);

    // ── Report actions ────────────────────────────────────────────────────────
    Task<SafetyWkRep> UploadReportAsync(
        int projectId, DateOnly weekStartDate,
        string fileName, string storedFileName, string extension, long fileSize,
        int userId, string userName, DateOnly? reportDate = null);
    Task<SafetyWkRep> ReplaceFileAsync(
        int reportId,
        string fileName, string storedFileName, string extension, long fileSize,
        int userId, string userName);
    Task<SafetyWkRep> MarkNoWorkAsync(int projectId, DateOnly weekStartDate, int userId, string userName, DateOnly? reportDate = null);
    Task<(SafetyWkRep Report, string? OldStoredFileName)> DeleteReportAsync(int reportId);
    Task<SafetyWkRep> ReviewReportAsync(int reportId, string? notes, int userId, string userName);
    Task<SafetyWkRep> UnreviewReportAsync(int reportId, int userId, string userName);
    Task<SafetyWkRep> ApproveReportAsync(int reportId, string? notes, int userId, string userName);
    Task<SafetyWkRep> VoidApprovalAsync(int reportId, string? reason, int userId, string userName);
}

// ── Implementation ────────────────────────────────────────────────────────────

public class SafetyWkRepService : ISafetyWkRepService
{
    private readonly AppDbContext _db;

    public SafetyWkRepService(AppDbContext db) => _db = db;

    // ── Helpers ───────────────────────────────────────────────────────────────

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
            .Include(r => r.UploadedBy)
            .Include(r => r.ReviewedBy)
            .Include(r => r.ApprovedBy)
            .Include(r => r.VoidedBy)
            .FirstOrDefaultAsync(r => r.Id == reportId && r.IsActive);

    public async Task<SafetyWkRep?> GetReportByWeekAsync(int projectId, DateOnly weekStartDate) =>
        await _db.SafetyWkReps
            .Include(r => r.Project)
            .FirstOrDefaultAsync(r => r.ProjectId == projectId
                                   && r.WeekStartDate == GetWeekMonday(weekStartDate)
                                   && r.IsActive);

    public async Task<List<SafetyWkRep>> GetProjectReportsAsync(
        int projectId, DateOnly from, DateOnly to) =>
        await _db.SafetyWkReps
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

        // 활성 프로젝트 + 해당 기간 보고서 + 설정을 한 번에 로드
        var projects = await _db.Projects
            .Where(p => p.IsActive)
            .OrderBy(p => p.ProjectCode)
            .ToListAsync();

        var reports = await _db.SafetyWkReps
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

        // PM/SPM 담당 프로젝트 조회 (Admin이 아닌 경우)
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

    // ── Report actions ─────────────────────────────────────────────────────────

    public async Task<SafetyWkRep> UploadReportAsync(
        int projectId, DateOnly weekStartDate,
        string fileName, string storedFileName, string extension, long fileSize,
        int userId, string userName, DateOnly? reportDate = null)
    {
        var monday = GetWeekMonday(weekStartDate);
        var existing = await _db.SafetyWkReps
            .FirstOrDefaultAsync(r => r.ProjectId == projectId
                                   && r.WeekStartDate == monday
                                   && r.IsActive);

        if (existing != null && existing.IsLocked)
            throw new InvalidOperationException(
                "Report is approved and locked. Void the approval before replacing.");

        if (existing != null)
        {
            // 기존 Draft/Reviewed/NoWork 레코드에 파일 교체
            existing.FileName       = fileName;
            existing.StoredFileName = storedFileName;
            existing.Extension      = extension;
            existing.FileSize       = fileSize;
            existing.Status         = SafetyWkRepStatus.Draft;
            existing.ReportDate     = reportDate;
            existing.UploadedById   = userId;
            existing.UploadedByName = userName;
            existing.UploadedAt     = DateTime.UtcNow;
            // 기존 Review 정보 초기화 (파일이 바뀌었으므로)
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
        report.FileName       = fileName;
        report.StoredFileName = storedFileName;
        report.Extension      = extension;
        report.FileSize       = fileSize;
        report.Status         = SafetyWkRepStatus.Draft;
        report.ReportDate     = reportDate;
        report.UploadedById   = userId;
        report.UploadedByName = userName;
        report.UploadedAt     = DateTime.UtcNow;
        report.CreatedById    = userId;

        _db.SafetyWkReps.Add(report);
        await _db.SaveChangesAsync();
        return report;
    }

    public async Task<SafetyWkRep> ReplaceFileAsync(
        int reportId,
        string fileName, string storedFileName, string extension, long fileSize,
        int userId, string userName)
    {
        var report = await _db.SafetyWkReps.FindAsync(reportId)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        if (report.IsLocked)
            throw new InvalidOperationException(
                "Report is approved and locked.");

        report.FileName       = fileName;
        report.StoredFileName = storedFileName;
        report.Extension      = extension;
        report.FileSize       = fileSize;
        report.Status         = SafetyWkRepStatus.Draft;
        report.UploadedById   = userId;
        report.UploadedByName = userName;
        report.UploadedAt     = DateTime.UtcNow;
        // Review 초기화
        report.ReviewedById   = null;
        report.ReviewedByName = null;
        report.ReviewedAt     = null;
        report.ReviewNotes    = null;
        report.UpdatedAt      = DateTime.UtcNow;
        report.UpdatedById    = userId;

        await _db.SaveChangesAsync();
        return report;
    }

    public async Task<SafetyWkRep> MarkNoWorkAsync(
        int projectId, DateOnly weekStartDate, int userId, string userName,
        DateOnly? reportDate = null)
    {
        var monday = GetWeekMonday(weekStartDate);
        var existing = await _db.SafetyWkReps
            .FirstOrDefaultAsync(r => r.ProjectId == projectId
                                   && r.WeekStartDate == monday
                                   && r.IsActive);

        if (existing != null && existing.IsLocked)
            throw new InvalidOperationException("Report is approved and locked.");

        if (existing != null)
        {
            existing.Status         = SafetyWkRepStatus.NoWork;
            existing.FileName       = null;
            existing.StoredFileName = null;
            existing.Extension      = null;
            existing.FileSize       = 0;
            existing.ReportDate     = reportDate;
            existing.UploadedById   = userId;
            existing.UploadedByName = userName;
            existing.UploadedAt     = DateTime.UtcNow;
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
        report.CreatedById    = userId;

        _db.SafetyWkReps.Add(report);
        await _db.SaveChangesAsync();
        return report;
    }

    /// <summary>
    /// 보고서를 삭제합니다 (Draft / Reviewed / NoWork 상태만 가능).
    /// 반환값의 OldStoredFileName 이 non-null 이면 호출측에서 디스크 파일도 삭제해야 합니다.
    /// </summary>
    public async Task<(SafetyWkRep Report, string? OldStoredFileName)> DeleteReportAsync(
        int reportId)
    {
        var report = await _db.SafetyWkReps.FindAsync(reportId)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        if (report.Status == SafetyWkRepStatus.Approved)
            throw new InvalidOperationException(
                "Approved reports cannot be deleted. Void the approval first.");

        var oldFile = report.StoredFileName;
        _db.SafetyWkReps.Remove(report);
        await _db.SaveChangesAsync();
        return (report, oldFile);
    }

    public async Task<SafetyWkRep> ReviewReportAsync(
        int reportId, string? notes, int userId, string userName)
    {
        var report = await _db.SafetyWkReps.FindAsync(reportId)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        if (report.Status is SafetyWkRepStatus.Approved or SafetyWkRepStatus.NoWorkApproved)
            throw new InvalidOperationException("Cannot review an already approved report.");

        if (report.Status == SafetyWkRepStatus.Voided)
            throw new InvalidOperationException("Cannot review a voided report.");

        // NoWork → NoWorkReviewed, 나머지 → Reviewed
        report.Status = report.Status == SafetyWkRepStatus.NoWork
            ? SafetyWkRepStatus.NoWorkReviewed
            : SafetyWkRepStatus.Reviewed;
        report.ReviewedById   = userId;
        report.ReviewedByName = userName;
        report.ReviewedAt     = DateTime.UtcNow;
        report.ReviewNotes    = notes;
        report.UpdatedAt      = DateTime.UtcNow;
        report.UpdatedById    = userId;

        await _db.SaveChangesAsync();
        return report;
    }

    public async Task<SafetyWkRep> UnreviewReportAsync(int reportId, int userId, string userName)
    {
        var report = await _db.SafetyWkReps.FindAsync(reportId)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        if (report.Status is not (SafetyWkRepStatus.Reviewed or SafetyWkRepStatus.NoWorkReviewed))
            throw new InvalidOperationException("Report is not in Reviewed status.");

        report.Status = report.Status == SafetyWkRepStatus.NoWorkReviewed
            ? SafetyWkRepStatus.NoWork
            : SafetyWkRepStatus.Draft;
        report.ReviewedById   = null;
        report.ReviewedByName = null;
        report.ReviewedAt     = null;
        report.ReviewNotes    = null;
        report.UpdatedAt      = DateTime.UtcNow;
        report.UpdatedById    = userId;

        await _db.SaveChangesAsync();
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
                $"Report cannot be approved. Current status: {report.Status}");

        // NoWork 계열 → NoWorkApproved, 나머지 → Approved
        report.Status = report.Status is SafetyWkRepStatus.NoWork or SafetyWkRepStatus.NoWorkReviewed
            ? SafetyWkRepStatus.NoWorkApproved
            : SafetyWkRepStatus.Approved;

        report.ApprovedById   = userId;
        report.ApprovedByName = userName;
        report.ApprovedAt     = DateTime.UtcNow;
        report.ApprovalNotes  = notes;
        report.UpdatedAt      = DateTime.UtcNow;
        report.UpdatedById    = userId;

        await _db.SaveChangesAsync();
        return report;
    }

    public async Task<SafetyWkRep> VoidApprovalAsync(
        int reportId, string? reason, int userId, string userName)
    {
        var report = await _db.SafetyWkReps.FindAsync(reportId)
            ?? throw new KeyNotFoundException($"Report {reportId} not found.");

        if (report.Status is not (SafetyWkRepStatus.Approved or SafetyWkRepStatus.NoWorkApproved))
            throw new InvalidOperationException("Report is not currently approved.");

        report.Status       = SafetyWkRepStatus.Voided;
        report.VoidedById   = userId;
        report.VoidedByName = userName;
        report.VoidedAt     = DateTime.UtcNow;
        report.VoidReason   = reason;
        report.UpdatedAt    = DateTime.UtcNow;
        report.UpdatedById  = userId;

        await _db.SaveChangesAsync();
        return report;
    }
}
