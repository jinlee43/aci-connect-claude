using ACI.Web.Data.Entities;

namespace ACI.Web.Services;

public interface IDailyReportService
{
    // ── 프로젝트 접근 ─────────────────────────────────────────────────────────
    Task<List<int>> GetAssignedProjectIdsAsync(int userId);

    // ── 보고서 조회 ───────────────────────────────────────────────────────────
    Task<DailyReport?> GetReportAsync(int reportId);
    Task<DailyReport?> GetReportByDateAsync(int projectId, DateOnly date);
    Task<List<DailyReport>> GetReportsAsync(int projectId, DateOnly from, DateOnly to);

    /// <summary>복수 프로젝트 × 선택적 날짜 범위 × 페이징. null 날짜 = 제한 없음.</summary>
    Task<(List<DailyReport> items, int total)> GetReportsPagedAsync(
        List<int> projectIds, DateOnly? from, DateOnly? to, int skip, int take);

    // ── 작성 (Superintendent) ─────────────────────────────────────────────────
    /// <summary>날짜에 해당 Draft 보고서가 없으면 새로 생성, 있으면 기존 반환.</summary>
    Task<DailyReport> GetOrCreateDraftAsync(int projectId, DateOnly date,
                                             int userId, string userName);

    Task<DailyReport> SaveDraftAsync(DailyReport report,
                                      List<DailyReportCrewEntry>    crew,
                                      List<DailyReportWorkItem>     workItems,
                                      List<DailyReportTaskProgress> taskProgress,
                                      List<DailyReportEquipment>    equipment);

    Task<DailyReport> SubmitAsync(int reportId, int userId, string userName);
    Task<DailyReport> MarkNoWorkAsync(int projectId, DateOnly date,
                                       string? reason, int userId, string userName);

    // ── 검토 (PE / PM) ────────────────────────────────────────────────────────
    Task<DailyReport> ReviewAsync(int reportId, string? notes, int userId, string userName);

    // ── 승인 (PM / SPM) ───────────────────────────────────────────────────────
    Task<DailyReport> ApproveAsync(int reportId, string? notes, int userId, string userName);

    // ── 무효화 (PM 이상) ─────────────────────────────────────────────────────
    Task<DailyReport> VoidAsync(int reportId, string? reason, int userId, string userName);

    // ── 삭제 (Draft 전용, Superintendent 본인 또는 Admin) ────────────────────
    /// <summary>Draft 상태 보고서만 삭제 가능. 제출 이후는 예외 발생.</summary>
    Task DeleteDraftAsync(int reportId);

    // ── 파일 ─────────────────────────────────────────────────────────────────
    Task<DailyReportFile?> GetFileAsync(int fileId);
    Task<(DailyReport report, int fileId)> AddFileAsync(
        int projectId, DateOnly date,
        string fileName, string storedFileName, string extension, long fileSize,
        DailyReportFileType fileType, string? caption,
        int userId, string userName);
    Task<(DailyReportFile file, string storedName)> RemoveFileAsync(int fileId);

    // ── 다음 보고서 번호 ──────────────────────────────────────────────────────
    Task<int> NextReportNumberAsync(int projectId);
}
