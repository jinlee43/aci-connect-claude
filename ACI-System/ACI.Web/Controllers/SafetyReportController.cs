using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ACI.Web.Controllers;

// ── Config binding ─────────────────────────────────────────────────────────────
public class FileStorageOptions
{
    public const string Section = "FileStorage";
    public string FileItemRoot { get; set; } = string.Empty;
}

/// <summary>
/// Safety weekly report file operations: upload, download, delete.
/// Status transitions (review / approve / void) live in the Razor Page handlers.
///
/// Storage layout:
///   {FileItemRoot}/SafetyMgmt/SafetyWkRepFiles/{year}{A|B}/{guid.ext}
///   A = Jan–Jun, B = Jul–Dec  (WeekStartDate 기준)
///
/// 접근 규칙:
///   SafetyUser+  → 전체 프로젝트, 모든 비잠금 보고서
///   Superintendent/ProjectManager → 담당 프로젝트만, Staged/Draft/Voided 상태만
/// </summary>
[ApiController]
[Route("api/safety-reports")]
[Authorize]
public class SafetyReportController : ControllerBase
{
    private readonly ISafetyWkRepService _svc;
    private readonly FileStorageOptions  _storage;
    private readonly ILogger<SafetyReportController> _logger;

    private const long MaxFileSizeBytes = 2000L * 1024 * 1024;   // 2000 MB

    public SafetyReportController(
        ISafetyWkRepService svc,
        IOptions<FileStorageOptions> storage,
        ILogger<SafetyReportController> logger)
    {
        _svc     = svc;
        _storage = storage.Value;
        _logger  = logger;
    }

    // ── Upload file ───────────────────────────────────────────────────────────
    // POST /api/safety-reports/upload
    // 파일을 Staged 보고서에 추가합니다 (없으면 생성).
    [HttpPost("upload")]
    [RequestSizeLimit(2002L * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] int     projectId,
        [FromForm] string  weekStartDate,
        [FromForm] string? reportDate,
        [FromForm] IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");
        if (file.Length > MaxFileSizeBytes)
            return BadRequest("File exceeds the 2000 MB size limit.");

        if (!DateOnly.TryParse(weekStartDate, out var weekDate))
            return BadRequest("Invalid week start date.");

        DateOnly? parsedReportDate = DateOnly.TryParse(reportDate, out var rd) ? rd : null;

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        // ── 권한 체크 ─────────────────────────────────────────────────────────
        if (!IsSafetyStaff())
        {
            if (!IsFieldUser()) return Forbid();

            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(projectId))
                return Forbid();

            // Field user는 Reviewed 이상이면 파일 추가 불가
            var monday   = SafetyWkRepService.GetWeekMonday(weekDate);
            var existing = await _svc.GetReportByWeekAsync(projectId, monday);
            if (existing != null &&
                existing.Status is SafetyWkRepStatus.Reviewed
                                or SafetyWkRepStatus.NoWorkReviewed
                                or SafetyWkRepStatus.Approved
                                or SafetyWkRepStatus.NoWorkApproved)
                return BadRequest("Report has already been reviewed or approved.");
        }

        var uploadDir  = GetUploadDir(SafetyWkRepService.GetWeekMonday(weekDate));
        Directory.CreateDirectory(uploadDir);

        var ext        = Path.GetExtension(file.FileName);
        var storedName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
        var storedPath = Path.Combine(uploadDir, storedName);

        await using (var fs = System.IO.File.Create(storedPath))
            await file.CopyToAsync(fs);

        try
        {
            var report = await _svc.AddFileAsync(
                projectId,
                SafetyWkRepService.GetWeekMonday(weekDate),
                file.FileName,
                storedName,
                ext.TrimStart('.').ToLowerInvariant(),
                file.Length,
                userId, userName,
                parsedReportDate);

            return Ok(new { reportId = report.Id, status = report.Status.ToString() });
        }
        catch (Exception ex)
        {
            try { System.IO.File.Delete(storedPath); } catch { /* best-effort */ }
            return BadRequest(ex.Message);
        }
    }

    // ── Serve individual file ─────────────────────────────────────────────────
    // GET /api/safety-reports/files/{fileId}?inline=true
    [HttpGet("files/{fileId:int}")]
    public async Task<IActionResult> GetFile(int fileId, [FromQuery] bool inline = false)
    {
        var file = await _svc.GetFileAsync(fileId);
        if (file == null) return NotFound();

        // 비 Safety 스태프는 담당 프로젝트 파일만 접근 가능
        if (!IsSafetyStaff())
        {
            var (userId, _) = GetUser();
            if (userId <= 0) return Forbid();
            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(file.Report.ProjectId))
                return Forbid();
        }

        var uploadDir   = GetUploadDir(file.Report.WeekStartDate);
        var allowedBase = Path.GetFullPath(uploadDir);
        var fullPath    = Path.GetFullPath(Path.Combine(allowedBase, file.StoredFileName));

        if (!fullPath.StartsWith(allowedBase + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Invalid file path.");

        if (!System.IO.File.Exists(fullPath))
            return NotFound("File not found on disk.");

        var ext         = file.Extension?.ToLowerInvariant() ?? "";
        var contentType = GetContentType(ext);
        var disposition = inline && IsPreviewable(ext) ? "inline" : "attachment";

        Response.Headers["Content-Disposition"] =
            $"{disposition}; filename=\"{Uri.EscapeDataString(file.FileName)}\"";

        return PhysicalFile(fullPath, contentType);
    }

    // ── Delete individual file ────────────────────────────────────────────────
    // DELETE /api/safety-reports/files/{fileId}
    [HttpDelete("files/{fileId:int}")]
    public async Task<IActionResult> DeleteFile(int fileId)
    {
        var file = await _svc.GetFileAsync(fileId);
        if (file == null) return NotFound();

        if (!IsSafetyStaff())
        {
            if (!IsFieldUser()) return Forbid();

            var (userId, _) = GetUser();
            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(file.Report.ProjectId))
                return Forbid();

            if (file.Report.Status is SafetyWkRepStatus.Reviewed
                                   or SafetyWkRepStatus.NoWorkReviewed
                                   or SafetyWkRepStatus.Approved
                                   or SafetyWkRepStatus.NoWorkApproved)
                return BadRequest("Cannot remove files from a reviewed or approved report.");
        }

        try
        {
            var (deletedFile, storedName) = await _svc.RemoveFileAsync(fileId);

            var filePath = Path.Combine(GetUploadDir(deletedFile.Report.WeekStartDate), storedName);
            if (System.IO.File.Exists(filePath))
                try { System.IO.File.Delete(filePath); }
                catch (Exception ex) { _logger.LogWarning(ex, "Could not delete safety file {Path}", filePath); }

            return Ok(new { deleted = true });
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (KeyNotFoundException)         { return NotFound(); }
    }

    // ── Delete entire report ──────────────────────────────────────────────────
    // DELETE /api/safety-reports/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var report = await _svc.GetReportAsync(id);
        if (report == null) return NotFound();

        if (!IsSafetyStaff())
        {
            if (!IsFieldUser()) return Forbid();

            var (userId, _) = GetUser();
            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(report.ProjectId))
                return Forbid();

            if (report.Status is SafetyWkRepStatus.Reviewed
                              or SafetyWkRepStatus.Approved)
                return BadRequest("Cannot delete a reviewed or approved report.");
        }

        try
        {
            var (deleted, storedNames) = await _svc.DeleteReportAsync(id);

            foreach (var storedName in storedNames)
            {
                var filePath = Path.Combine(GetUploadDir(deleted.WeekStartDate), storedName);
                if (System.IO.File.Exists(filePath))
                    try { System.IO.File.Delete(filePath); }
                    catch (Exception ex) { _logger.LogWarning(ex, "Could not delete safety file {Path}", filePath); }
            }

            return Ok(new { deleted = true });
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (KeyNotFoundException)         { return NotFound(); }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private (int userId, string userName) GetUser()
    {
        var idStr = User.FindFirst("UserId")?.Value
                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idStr, out var id) || id <= 0) return (0, "");
        return (id, User.Identity?.Name ?? "Unknown");
    }

    /// <summary>SafetyUser 이상 — 전체 프로젝트 접근 가능</summary>
    private bool IsSafetyStaff() =>
        User.IsInRole(PrivilegeCodes.Admin)
        || User.IsInRole(PrivilegeCodes.SafetyAdmin)
        || User.IsInRole(PrivilegeCodes.SafetyManager)
        || User.IsInRole(PrivilegeCodes.SafetyUser);

    /// <summary>Superintendent / ProjectManager — 담당 프로젝트 한정</summary>
    private bool IsFieldUser() =>
        User.IsInRole(PrivilegeCodes.Superintendent)
        || User.IsInRole(PrivilegeCodes.ProjectManager);

    private string GetUploadDir(DateOnly weekDate)
    {
        var halfYear = weekDate.Month <= 6 ? "A" : "B";
        return Path.Combine(_storage.FileItemRoot, "SafetyMgmt", "SafetyWkRepFiles",
                            $"{weekDate.Year}{halfYear}");
    }

    private static string GetContentType(string ext) => ext switch
    {
        "pdf"           => "application/pdf",
        "jpg" or "jpeg" => "image/jpeg",
        "png"           => "image/png",
        "doc"           => "application/msword",
        "docx"          => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "xls"           => "application/vnd.ms-excel",
        "xlsx"          => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        _               => "application/octet-stream"
    };

    private static bool IsPreviewable(string ext) =>
        ext is "pdf" or "jpg" or "jpeg" or "png";
}
