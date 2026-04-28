using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ACI.Web.Controllers;

/// <summary>
/// Daily Report 파일 업로드 / 다운로드 / 삭제 API.
///
/// Storage layout:
///   {FileItemRoot}/DailyReports/{year}/{projectId}/{reportDate:yyyy-MM}/{guid.ext}
///
/// 권한:
///   Admin → 모든 프로젝트
///   Superintendent → 담당(assigned) 프로젝트만, Draft/Submitted 상태만 업로드/삭제
///   ProjectEngineer/ProjectManager → 담당(assigned) 프로젝트만 열람
/// </summary>
[ApiController]
[Route("api/daily-reports")]
[Authorize]
public class DailyReportController : ControllerBase
{
    private readonly IDailyReportService _svc;
    private readonly AppDbContext        _db;
    private readonly FileStorageOptions  _storage;
    private readonly ILogger<DailyReportController> _logger;

    private const long MaxFileSizeBytes = 500L * 1024 * 1024;   // 500 MB

    public DailyReportController(
        IDailyReportService svc,
        AppDbContext db,
        IOptions<FileStorageOptions> storage,
        ILogger<DailyReportController> logger)
    {
        _svc     = svc;
        _db      = db;
        _storage = storage.Value;
        _logger  = logger;
    }

    // ── Weather Cache ─────────────────────────────────────────────────────────
    // POST /api/daily-reports/weather-cache
    [HttpPost("weather-cache")]
    public async Task<IActionResult> SaveWeatherCache(
        [FromBody] WeatherCacheDto dto)
    {
        if (!DateOnly.TryParse(dto.Date, out var date))
            return BadRequest(new { error = "Invalid date." });

        var existing = await _db.WeatherCache
            .FirstOrDefaultAsync(w => w.ProjectId == dto.ProjectId && w.Date == date);

        if (existing != null)
        {
            // 이미 있으면 덮어씀 (재조회 결과 반영)
            existing.Latitude  = dto.Latitude;
            existing.Longitude = dto.Longitude;
            existing.Address   = dto.Address;
            existing.Condition = dto.Condition;
            existing.TempHigh  = dto.TempHigh;
            existing.TempLow   = dto.TempLow;
            existing.IsWindy   = dto.IsWindy;
            existing.IsRainy   = dto.IsRainy;
            existing.FetchedAt = DateTime.UtcNow;
            existing.Source    = dto.Source ?? "open-meteo";
        }
        else
        {
            _db.WeatherCache.Add(new WeatherCache
            {
                ProjectId  = dto.ProjectId,
                Date       = date,
                Latitude   = dto.Latitude,
                Longitude  = dto.Longitude,
                Address    = dto.Address,
                Condition  = dto.Condition,
                TempHigh   = dto.TempHigh,
                TempLow    = dto.TempLow,
                IsWindy    = dto.IsWindy,
                IsRainy    = dto.IsRainy,
                FetchedAt  = DateTime.UtcNow,
                Source     = dto.Source ?? "open-meteo",
            });
        }

        await _db.SaveChangesAsync();
        return Ok();
    }

    // ── Upload ────────────────────────────────────────────────────────────────
    // POST /api/daily-reports/upload
    [HttpPost("upload")]
    [RequestSizeLimit(502L * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] int                projectId,
        [FromForm] string             reportDate,
        [FromForm] List<IFormFile>?   files,
        [FromForm] List<int>?         fileTypes)   // 0=Document, 1=Photo per file
    {
        if (files == null || files.Count == 0)
            return BadRequest(new { error = "No file provided." });

        var oversized = files.FirstOrDefault(f => f.Length > MaxFileSizeBytes);
        if (oversized != null)
            return BadRequest(new { error = $"'{oversized.FileName}' exceeds the 500 MB limit." });

        if (!DateOnly.TryParse(reportDate, out var date))
            return BadRequest(new { error = "Invalid report date." });

        var (userId, userName) = GetUser();
        if (userId <= 0) return Forbid();

        // Superintendent + 담당 프로젝트만 업로드 가능 (Admin/HrAdmin 포함 그 외 모두 불가)
        if (!IsSuperintendent()) return Forbid();
        var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
        if (!assigned.Contains(projectId)) return Forbid();

        var existing = await _svc.GetReportByDateAsync(projectId, date);
        if (existing != null && existing.IsLocked)
            return BadRequest(new { error = "Report is locked." });

        // 신규(미저장) report에는 업로드 불가 — 먼저 저장해야 함
        if (existing == null)
            return BadRequest(new { error = "Please save the report before uploading files." });

        var uploadDir = GetUploadDir(projectId, date);
        Directory.CreateDirectory(uploadDir);

        var savedPaths = new List<string>();
        try
        {
            int? lastReportId = null;
            for (int i = 0; i < files.Count; i++)
            {
                var file       = files[i];
                var ext        = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
                var storedName = $"{Guid.NewGuid():N}.{ext}";
                var storedPath = Path.Combine(uploadDir, storedName);

                await using (var fs = System.IO.File.Create(storedPath))
                    await file.CopyToAsync(fs);
                savedPaths.Add(storedPath);

                var fType = (fileTypes != null && i < fileTypes.Count && fileTypes[i] == 1)
                            ? DailyReportFileType.Photo : DailyReportFileType.Document;

                var (report, fileId) = await _svc.AddFileAsync(
                    projectId, date,
                    file.FileName, storedName, ext, file.Length,
                    fType, null,
                    userId, userName);
                lastReportId = report.Id;
            }
            return Ok(new { reportId = lastReportId, count = files.Count });
        }
        catch (Exception ex)
        {
            foreach (var p in savedPaths)
                try { System.IO.File.Delete(p); } catch { /* best-effort */ }
            return BadRequest(new { error = ex.Message });
        }
    }

    // ── Serve file ────────────────────────────────────────────────────────────
    // GET /api/daily-reports/files/{fileId}/{fileName?}?inline=true
    [HttpGet("files/{fileId:int}/{fileName?}")]
    public async Task<IActionResult> GetFile(int fileId, string? fileName,
                                              [FromQuery] bool inline = false)
    {
        var file = await _svc.GetFileAsync(fileId);
        if (file == null) return NotFound();

        // Admin / HrAdmin은 모든 프로젝트 파일 열람 가능; 그 외는 담당 프로젝트만
        if (!IsAdmin() && !IsHrAdmin())
        {
            var (userId, _) = GetUser();
            if (userId <= 0) return Forbid();
            var assigned = await _svc.GetAssignedProjectIdsAsync(userId);
            if (!assigned.Contains(file.DailyReport.ProjectId)) return Forbid();
        }

        var uploadDir   = GetUploadDir(file.DailyReport.ProjectId, file.DailyReport.ReportDate);
        var allowedBase = Path.GetFullPath(uploadDir);
        var fullPath    = Path.GetFullPath(Path.Combine(allowedBase, file.StoredFileName));

        if (!fullPath.StartsWith(allowedBase + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return BadRequest("Invalid file path.");
        if (!System.IO.File.Exists(fullPath))
            return NotFound("File not found on disk.");

        var ext         = file.Extension?.ToLowerInvariant() ?? "";
        var contentType = GetContentType(ext);

        if (inline && IsPreviewable(ext))
        {
            var encoded = Uri.EscapeDataString(file.FileName);
            Response.Headers["Content-Disposition"] =
                $"inline; filename=\"{encoded}\"; filename*=UTF-8''{encoded}";
            return PhysicalFile(fullPath, contentType);
        }

        return PhysicalFile(fullPath, contentType, file.FileName);
    }

    // ── Delete file ───────────────────────────────────────────────────────────
    // DELETE /api/daily-reports/files/{fileId}
    [HttpDelete("files/{fileId:int}")]
    public async Task<IActionResult> DeleteFile(int fileId)
    {
        var file = await _svc.GetFileAsync(fileId);
        if (file == null) return NotFound();

        var (userId, _) = GetUser();
        if (userId <= 0) return Forbid();

        // Superintendent + 담당 프로젝트만 삭제 가능 (Admin/HrAdmin 포함 그 외 모두 불가)
        if (!IsSuperintendent()) return Forbid();
        var assignedDel = await _svc.GetAssignedProjectIdsAsync(userId);
        if (!assignedDel.Contains(file.DailyReport.ProjectId)) return Forbid();
        if (file.DailyReport.IsLocked)
            return BadRequest("Report is locked.");

        try
        {
            var (deletedFile, storedName) = await _svc.RemoveFileAsync(fileId);
            var filePath = Path.Combine(
                GetUploadDir(deletedFile.DailyReport.ProjectId, deletedFile.DailyReport.ReportDate),
                storedName);
            if (System.IO.File.Exists(filePath))
                try { System.IO.File.Delete(filePath); }
                catch (Exception ex) { _logger.LogWarning(ex, "Could not delete daily report file {Path}", filePath); }

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

    private bool IsSuperintendent() =>
        User.IsInRole(PrivilegeCodes.Superintendent);

    private bool IsAdmin() =>
        User.IsInRole(PrivilegeCodes.Admin);

    private bool IsHrAdmin() =>
        User.IsInRole(PrivilegeCodes.HrAdmin);

    private string GetUploadDir(int projectId, DateOnly date) =>
        Path.Combine(_storage.FileItemRoot, "DailyReports",
                     date.Year.ToString(), projectId.ToString(),
                     date.ToString("yyyy-MM"));

    private static string GetContentType(string ext) => ext switch
    {
        "pdf"           => "application/pdf",
        "jpg" or "jpeg" => "image/jpeg",
        "png"           => "image/png",
        "gif"           => "image/gif",
        "webp"          => "image/webp",
        "doc"           => "application/msword",
        "docx"          => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "xls"           => "application/vnd.ms-excel",
        "xlsx"          => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        _               => "application/octet-stream",
    };

    private static bool IsPreviewable(string ext) =>
        ext is "pdf" or "jpg" or "jpeg" or "png" or "gif" or "webp";
}

// ── DTOs ──────────────────────────────────────────────────────────────────────
public record WeatherCacheDto(
    int     ProjectId,
    string  Date,         // "yyyy-MM-dd"
    double? Latitude,
    double? Longitude,
    string? Address,
    string? Condition,
    int?    TempHigh,
    int?    TempLow,
    bool    IsWindy,
    bool    IsRainy,
    string? Source
);
