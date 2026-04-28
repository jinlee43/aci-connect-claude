using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ACI.Web.Services;

/// <summary>
/// Daily Report 비즈니스 로직.
/// 권한 검사는 Page/Controller 레이어에서 수행, 서비스는 순수 데이터 조작만 담당.
/// </summary>
public class DailyReportService : IDailyReportService
{
    private readonly AppDbContext _db;

    public DailyReportService(AppDbContext db) => _db = db;

    // ── 프로젝트 접근 권한 ────────────────────────────────────────────────────
    public async Task<List<int>> GetAssignedProjectIdsAsync(int userId)
    {
        // EmpRole → OrgUnit(ProjectTeam) → Project 경로로 담당 프로젝트 수집
        var today = DateOnly.FromDateTime(DateTime.Today);
        return await _db.EmpRoles
            .Where(r => r.Employee!.UserAccount != null
                     && r.Employee.UserAccount.Id == userId
                     && (r.EndDate == null || r.EndDate >= today)
                     && r.OrgUnit.Type == OrgUnitType.ProjectTeam
                     && r.OrgUnit.ProjectId != null)
            .Select(r => r.OrgUnit.ProjectId!.Value)
            .Distinct()
            .ToListAsync();
    }

    // ── 조회 ─────────────────────────────────────────────────────────────────
    public async Task<DailyReport?> GetReportAsync(int reportId) =>
        await _db.DailyReports
            .Include(r => r.Project)
            .Include(r => r.AuthoredBy).ThenInclude(u => u!.Employee)
            .Include(r => r.ReviewedBy).ThenInclude(u => u!.Employee)
            .Include(r => r.ApprovedBy).ThenInclude(u => u!.Employee)
            .Include(r => r.CrewEntries).ThenInclude(c => c.Trade)
            .Include(r => r.WorkItems).ThenInclude(w => w.Trade)
            .Include(r => r.TaskProgress).ThenInclude(t => t.WorkingTask)
            .Include(r => r.Equipment)
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.Id == reportId && r.IsActive);

    public async Task<DailyReport?> GetReportByDateAsync(int projectId, DateOnly date) =>
        await _db.DailyReports
            .Include(r => r.Project)
            .Include(r => r.CrewEntries).ThenInclude(c => c.Trade)
            .Include(r => r.WorkItems).ThenInclude(w => w.Trade)
            .Include(r => r.TaskProgress).ThenInclude(t => t.WorkingTask)
            .Include(r => r.Equipment)
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.ProjectId == projectId
                                   && r.ReportDate == date
                                   && r.IsActive);

    public async Task<List<DailyReport>> GetReportsAsync(int projectId, DateOnly from, DateOnly to) =>
        await _db.DailyReports
            .Where(r => r.ProjectId == projectId
                     && r.IsActive
                     && r.ReportDate >= from
                     && r.ReportDate <= to)
            .Include(r => r.Files)
            .OrderByDescending(r => r.ReportDate)
            .ToListAsync();

    public async Task<(List<DailyReport> items, int total)> GetReportsPagedAsync(
        List<int> projectIds, DateOnly? from, DateOnly? to, int skip, int take)
    {
        var query = _db.DailyReports
            .Where(r => r.IsActive && projectIds.Contains(r.ProjectId));

        if (from.HasValue) query = query.Where(r => r.ReportDate >= from.Value);
        if (to.HasValue)   query = query.Where(r => r.ReportDate <= to.Value);

        var total = await query.CountAsync();

        var items = await query
            .Include(r => r.Project)
            .Include(r => r.CrewEntries)
            .Include(r => r.TaskProgress)
            .Include(r => r.Files)
            .OrderByDescending(r => r.ReportDate)
            .ThenBy(r => r.Project!.ProjectCode)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        return (items, total);
    }

    // ── 초안 생성 또는 기존 Draft 반환 ──────────────────────────────────────
    public async Task<DailyReport> GetOrCreateDraftAsync(int projectId, DateOnly date,
                                                           int userId, string userName)
    {
        var existing = await _db.DailyReports
            .Include(r => r.CrewEntries).ThenInclude(c => c.Trade)
            .Include(r => r.WorkItems).ThenInclude(w => w.Trade)
            .Include(r => r.TaskProgress).ThenInclude(t => t.WorkingTask)
            .Include(r => r.Equipment)
            .Include(r => r.Files)
            .FirstOrDefaultAsync(r => r.ProjectId == projectId
                                   && r.ReportDate == date
                                   && r.IsActive);
        if (existing != null) return existing;

        var reportNum = await NextReportNumberAsync(projectId);
        var report = new DailyReport
        {
            ProjectId      = projectId,
            ReportDate     = date,
            ReportNumber   = reportNum,
            Status         = DailyReportStatus.Draft,
            AuthoredById   = userId,
            AuthoredByName = userName,
            AuthoredAt     = DateTime.UtcNow,
        };
        _db.DailyReports.Add(report);
        await _db.SaveChangesAsync();
        return report;
    }

    // ── 저장 (Draft) ─────────────────────────────────────────────────────────
    public async Task<DailyReport> SaveDraftAsync(DailyReport report,
                                                    List<DailyReportCrewEntry>    crew,
                                                    List<DailyReportWorkItem>     workItems,
                                                    List<DailyReportTaskProgress> taskProgress,
                                                    List<DailyReportEquipment>    equipment)
    {
        if (report.IsLocked)
            throw new InvalidOperationException("Approved or NoWork-approved reports cannot be edited.");

        // 기존 자식 레코드 삭제 후 새로 삽입 (단순 replace 전략)
        var existingCrew     = _db.DailyReportCrewEntries.Where(c => c.DailyReportId == report.Id);
        var existingWork     = _db.DailyReportWorkItems.Where(w => w.DailyReportId == report.Id);
        var existingProgress = _db.DailyReportTaskProgress.Where(t => t.DailyReportId == report.Id);
        var existingEquip    = _db.DailyReportEquipment.Where(q => q.DailyReportId == report.Id);

        _db.DailyReportCrewEntries.RemoveRange(await existingCrew.ToListAsync());
        _db.DailyReportWorkItems.RemoveRange(await existingWork.ToListAsync());
        _db.DailyReportTaskProgress.RemoveRange(await existingProgress.ToListAsync());
        _db.DailyReportEquipment.RemoveRange(await existingEquip.ToListAsync());

        foreach (var c in crew)     { c.DailyReportId = report.Id; _db.DailyReportCrewEntries.Add(c); }
        foreach (var w in workItems){ w.DailyReportId = report.Id; _db.DailyReportWorkItems.Add(w); }
        foreach (var t in taskProgress){ t.DailyReportId = report.Id; _db.DailyReportTaskProgress.Add(t); }
        foreach (var q in equipment){ q.DailyReportId = report.Id; _db.DailyReportEquipment.Add(q); }

        report.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return report;
    }

    // ── 제출 ─────────────────────────────────────────────────────────────────
    public async Task<DailyReport> SubmitAsync(int reportId, int userId, string userName)
    {
        var report = await _db.DailyReports.FindAsync(reportId)
                    ?? throw new KeyNotFoundException();

        if (report.Status != DailyReportStatus.Draft)
            throw new InvalidOperationException("Only Draft reports can be submitted.");

        report.Status      = DailyReportStatus.Submitted;
        report.SubmittedAt = DateTime.UtcNow;
        report.UpdatedAt   = DateTime.UtcNow;
        report.UpdatedById = userId;
        await _db.SaveChangesAsync();
        return report;
    }

    // ── 무작업 마킹 ──────────────────────────────────────────────────────────
    public async Task<DailyReport> MarkNoWorkAsync(int projectId, DateOnly date,
                                                    string? reason, int userId, string userName)
    {
        var report = await _db.DailyReports
            .FirstOrDefaultAsync(r => r.ProjectId == projectId && r.ReportDate == date && r.IsActive);

        if (report == null)
        {
            var num = await NextReportNumberAsync(projectId);
            report = new DailyReport
            {
                ProjectId      = projectId,
                ReportDate     = date,
                ReportNumber   = num,
                AuthoredById   = userId,
                AuthoredByName = userName,
                AuthoredAt     = DateTime.UtcNow,
            };
            _db.DailyReports.Add(report);
        }
        else if (report.IsLocked)
            throw new InvalidOperationException("Report is locked.");

        report.Status        = DailyReportStatus.NoWork;
        report.IsNoWork      = true;
        report.NoWorkReason  = reason;
        report.SubmittedAt   = DateTime.UtcNow;
        report.UpdatedAt     = DateTime.UtcNow;
        report.UpdatedById   = userId;
        await _db.SaveChangesAsync();
        return report;
    }

    // ── 검토 (PE / PM) ────────────────────────────────────────────────────────
    public async Task<DailyReport> ReviewAsync(int reportId, string? notes,
                                                int userId, string userName)
    {
        var report = await _db.DailyReports.FindAsync(reportId)
                    ?? throw new KeyNotFoundException();

        if (report.Status is not (DailyReportStatus.Submitted or DailyReportStatus.NoWork))
            throw new InvalidOperationException("Only Submitted or NoWork reports can be reviewed.");

        report.Status         = report.IsNoWork
                                    ? DailyReportStatus.NoWork     // NoWork → 그대로 NoWork, 승인만 분리
                                    : DailyReportStatus.Reviewed;
        report.ReviewedById   = userId;
        report.ReviewedByName = userName;
        report.ReviewedAt     = DateTime.UtcNow;
        report.ReviewNotes    = notes;
        report.UpdatedAt      = DateTime.UtcNow;
        report.UpdatedById    = userId;
        await _db.SaveChangesAsync();
        return report;
    }

    // ── 승인 (PM / SPM) ───────────────────────────────────────────────────────
    public async Task<DailyReport> ApproveAsync(int reportId, string? notes,
                                                 int userId, string userName)
    {
        var report = await _db.DailyReports.FindAsync(reportId)
                    ?? throw new KeyNotFoundException();

        if (report.Status is not (DailyReportStatus.Reviewed
                                or DailyReportStatus.Submitted
                                or DailyReportStatus.NoWork))
            throw new InvalidOperationException("Report is not in an approvable state.");

        report.Status         = report.IsNoWork
                                    ? DailyReportStatus.NoWorkApproved
                                    : DailyReportStatus.Approved;
        report.ApprovedById   = userId;
        report.ApprovedByName = userName;
        report.ApprovedAt     = DateTime.UtcNow;
        report.ApprovalNotes  = notes;
        report.UpdatedAt      = DateTime.UtcNow;
        report.UpdatedById    = userId;
        await _db.SaveChangesAsync();
        return report;
    }

    // ── 무효화 ────────────────────────────────────────────────────────────────
    public async Task<DailyReport> VoidAsync(int reportId, string? reason,
                                              int userId, string userName)
    {
        var report = await _db.DailyReports.FindAsync(reportId)
                    ?? throw new KeyNotFoundException();

        if (report.Status is DailyReportStatus.Draft or DailyReportStatus.Voided)
            throw new InvalidOperationException("Draft or already voided reports cannot be voided.");

        report.Status       = DailyReportStatus.Voided;
        report.VoidedById   = userId;
        report.VoidedByName = userName;
        report.VoidedAt     = DateTime.UtcNow;
        report.VoidReason   = reason;
        report.UpdatedAt    = DateTime.UtcNow;
        report.UpdatedById  = userId;
        await _db.SaveChangesAsync();
        return report;
    }

    // ── 파일 ─────────────────────────────────────────────────────────────────
    public async Task<DailyReportFile?> GetFileAsync(int fileId) =>
        await _db.DailyReportFiles
            .Include(f => f.DailyReport)
            .FirstOrDefaultAsync(f => f.Id == fileId && f.IsActive);

    public async Task<(DailyReport report, int fileId)> AddFileAsync(
        int projectId, DateOnly date,
        string fileName, string storedFileName, string extension, long fileSize,
        DailyReportFileType fileType, string? caption,
        int userId, string userName)
    {
        var report = await _db.DailyReports
            .FirstOrDefaultAsync(r => r.ProjectId == projectId
                                   && r.ReportDate == date
                                   && r.IsActive);

        if (report == null)
        {
            var num = await NextReportNumberAsync(projectId);
            report = new DailyReport
            {
                ProjectId      = projectId,
                ReportDate     = date,
                ReportNumber   = num,
                Status         = DailyReportStatus.Draft,
                AuthoredById   = userId,
                AuthoredByName = userName,
                AuthoredAt     = DateTime.UtcNow,
            };
            _db.DailyReports.Add(report);
            await _db.SaveChangesAsync();
        }

        if (report.IsLocked)
            throw new InvalidOperationException("Cannot add files to a locked report.");

        var file = new DailyReportFile
        {
            DailyReportId  = report.Id,
            FileType       = fileType,
            FileName       = fileName,
            StoredFileName = storedFileName,
            Extension      = extension,
            FileSize       = fileSize,
            Caption        = caption,
            UploadedById   = userId,
            UploadedByName = userName,
            UploadedAt     = DateTime.UtcNow,
        };
        _db.DailyReportFiles.Add(file);
        await _db.SaveChangesAsync();
        return (report, file.Id);
    }

    public async Task<(DailyReportFile file, string storedName)> RemoveFileAsync(int fileId)
    {
        var file = await _db.DailyReportFiles
            .Include(f => f.DailyReport)
            .FirstOrDefaultAsync(f => f.Id == fileId)
            ?? throw new KeyNotFoundException();

        if (file.DailyReport.IsLocked)
            throw new InvalidOperationException("Cannot remove files from a locked report.");

        var stored = file.StoredFileName;
        _db.DailyReportFiles.Remove(file);
        await _db.SaveChangesAsync();
        return (file, stored);
    }

    // ── 삭제 (Draft 전용) ─────────────────────────────────────────────────────
    public async Task DeleteDraftAsync(int reportId)
    {
        var report = await _db.DailyReports
            .FirstOrDefaultAsync(r => r.Id == reportId)
            ?? throw new KeyNotFoundException("Report not found.");

        if (report.Status != DailyReportStatus.Draft)
            throw new InvalidOperationException("Only Draft reports can be deleted.");

        // soft-delete (BaseEntity.IsActive = false)
        report.IsActive  = false;
        report.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── 다음 보고서 번호 ──────────────────────────────────────────────────────
    public async Task<int> NextReportNumberAsync(int projectId)
    {
        var max = await _db.DailyReports
            .Where(r => r.ProjectId == projectId)
            .MaxAsync(r => (int?)r.ReportNumber) ?? 0;
        return max + 1;
    }
}
