using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ACI.Web.Controllers;

/// <summary>
/// REST API for employee file attachments.
///
/// Storage layout:
///   {FileItemRoot}/Hr/EmpDataItems/{year}{A|B}/{guid.ext}
///   where A = Jan–Jun, B = Jul–Dec  (based on upload date = doc.CreatedAt)
///
/// LegacyPath: 구버전 시스템에서 마이그레이션된 파일의 전체 경로 (StoredFileName이 없는 경우).
/// </summary>
[ApiController]
[Route("api/employees/{empId:int}/documents")]
[Authorize(Policy = "HrAdmin")]
public class EmployeeDocumentController : ControllerBase
{
    private readonly AppDbContext        _db;
    private readonly FileStorageOptions  _storage;
    private readonly ILogger<EmployeeDocumentController> _logger;

    private const long MaxFileSizeBytes = 50 * 1024 * 1024;   // 50 MB

    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp",
            ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
            ".txt", ".csv", ".zip", ".rar", ".7z", ".xml", ".dwg", ".dxf"
        };

    public EmployeeDocumentController(
        AppDbContext db,
        IOptions<FileStorageOptions> storage,
        ILogger<EmployeeDocumentController> logger)
    {
        _db      = db;
        _storage = storage.Value;
        _logger  = logger;
    }

    // ── Upload (one or more files) ────────────────────────────────────────────
    // POST /api/employees/{empId}/documents
    [HttpPost]
    [RequestSizeLimit(100 * 1024 * 1024)]
    public async Task<IActionResult> Upload(int empId, [FromForm] IFormFileCollection files)
    {
        if (!await _db.Employees.AnyAsync(e => e.Id == empId))
            return NotFound("Employee not found");

        if (files == null || files.Count == 0)
            return BadRequest("No files provided");

        var uploadDate = DateTime.UtcNow;
        var uploadDir  = GetUploadDir(uploadDate);
        Directory.CreateDirectory(uploadDir);

        var saved = new List<object>();

        foreach (var file in files)
        {
            if (file.Length == 0) continue;
            if (file.Length > MaxFileSizeBytes)
                return BadRequest($"File '{file.FileName}' exceeds the 50 MB size limit");

            var ext = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(ext))
                return BadRequest($"File type '{ext}' is not allowed");

            var storedName = $"{Guid.NewGuid():N}{ext.ToLowerInvariant()}";
            var storedPath = Path.Combine(uploadDir, storedName);

            await using (var fs = System.IO.File.Create(storedPath))
                await file.CopyToAsync(fs);

            var doc = new EmployeeDocument
            {
                EmployeeId     = empId,
                FileName       = file.FileName,
                StoredFileName = storedName,
                Extension      = ext.TrimStart('.').ToLowerInvariant(),
                FileSizeBytes  = file.Length,
                UploadedByName = User.Identity?.Name,
                CreatedAt      = uploadDate,
                UpdatedAt      = uploadDate,
            };

            _db.EmployeeDocuments.Add(doc);
            await _db.SaveChangesAsync();

            saved.Add(new
            {
                id              = doc.Id,
                fileName        = doc.FileName,
                extension       = doc.Extension,
                fileSizeBytes   = doc.FileSizeBytes,
                fileSizeDisplay = doc.FileSizeDisplay,
                fileIconClass   = doc.FileIconClass,
                isPreviewable   = doc.IsPreviewable,
                uploadedAt      = doc.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            });
        }

        return Ok(new { uploaded = saved });
    }

    // ── Serve file ────────────────────────────────────────────────────────────
    // GET /api/employees/{empId}/documents/{docId}/file
    [HttpGet("{docId:int}/file")]
    public async Task<IActionResult> GetFile(int empId, int docId, [FromQuery] bool inline = false)
    {
        var doc = await _db.EmployeeDocuments.FindAsync(docId);
        if (doc == null || doc.EmployeeId != empId) return NotFound();

        // 신규 업로드 — StoredFileName(GUID) 기준 경로 재구성
        if (!string.IsNullOrEmpty(doc.StoredFileName))
        {
            // 업로드 시점(CreatedAt)으로 {year}{A|B} 폴더 결정
            var allowedBase = Path.GetFullPath(GetUploadDir(doc.CreatedAt));
            var fullPath    = Path.GetFullPath(Path.Combine(allowedBase, doc.StoredFileName));

            // Path traversal 방어
            if (!fullPath.StartsWith(allowedBase + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                return BadRequest("Invalid file path.");

            if (!System.IO.File.Exists(fullPath))
                return NotFound("File not found on disk");

            var contentType = GetContentType(doc.Extension);
            var disposition = inline && doc.IsPreviewable ? "inline" : "attachment";
            Response.Headers["Content-Disposition"] =
                $"{disposition}; filename=\"{Uri.EscapeDataString(doc.FileName)}\"";
            return PhysicalFile(fullPath, contentType);
        }

        // 레거시 — 구버전 시스템에서 마이그레이션된 파일 (전체 경로 직접 사용)
        if (!string.IsNullOrEmpty(doc.LegacyPath))
        {
            if (!System.IO.File.Exists(doc.LegacyPath))
                return NotFound("Legacy file path is not accessible from this server");

            var contentType = GetContentType(doc.Extension);
            var disposition = inline && doc.IsPreviewable ? "inline" : "attachment";
            Response.Headers["Content-Disposition"] =
                $"{disposition}; filename=\"{Uri.EscapeDataString(doc.FileName)}\"";
            return PhysicalFile(doc.LegacyPath, contentType);
        }

        return NotFound("No file path available for this document");
    }

    // ── Delete ────────────────────────────────────────────────────────────────
    // DELETE /api/employees/{empId}/documents/{docId}
    [HttpDelete("{docId:int}")]
    public async Task<IActionResult> Delete(int empId, int docId)
    {
        var doc = await _db.EmployeeDocuments.FindAsync(docId);
        if (doc == null || doc.EmployeeId != empId) return NotFound();

        if (!string.IsNullOrEmpty(doc.StoredFileName))
        {
            var filePath = Path.Combine(GetUploadDir(doc.CreatedAt), doc.StoredFileName);
            if (System.IO.File.Exists(filePath))
                try { System.IO.File.Delete(filePath); }
                catch (Exception ex) { _logger.LogWarning(ex, "Could not delete file {Path}", filePath); }
        }

        _db.EmployeeDocuments.Remove(doc);
        await _db.SaveChangesAsync();

        return Ok(new { deleted = true });
    }

    // ── List ──────────────────────────────────────────────────────────────────
    // GET /api/employees/{empId}/documents
    [HttpGet]
    public async Task<IActionResult> List(int empId)
    {
        var docs = await _db.EmployeeDocuments
            .Where(d => d.EmployeeId == empId && d.IsActive)
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new
            {
                d.Id,
                d.FileName,
                d.Extension,
                d.FileSizeBytes,
                FileSizeDisplay = d.FileSizeBytes < 1024      ? $"{d.FileSizeBytes} B"
                                : d.FileSizeBytes < 1048576   ? $"{d.FileSizeBytes / 1024.0:F1} KB"
                                :                               $"{d.FileSizeBytes / 1048576.0:F1} MB",
                FileIconClass   = d.Extension == "pdf"        ? "bi-file-pdf text-danger"
                                : d.Extension == "jpg" || d.Extension == "jpeg"
                                  || d.Extension == "png" || d.Extension == "gif"
                                  || d.Extension == "webp"    ? "bi-file-image text-success"
                                : d.Extension == "doc"  || d.Extension == "docx" ? "bi-file-word text-primary"
                                : d.Extension == "xls"  || d.Extension == "xlsx" ? "bi-file-excel text-success"
                                : d.Extension == "ppt"  || d.Extension == "pptx" ? "bi-file-ppt text-warning"
                                :                               "bi-file-earmark text-secondary",
                IsPreviewable   = d.Extension == "pdf"
                               || d.Extension == "jpg" || d.Extension == "jpeg"
                               || d.Extension == "png" || d.Extension == "gif"
                               || d.Extension == "webp",
                IsLegacyOnly    = d.StoredFileName == "" && d.LegacyPath != null,
                UploadedAt      = d.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                d.UploadedByName
            })
            .ToListAsync();

        return Ok(docs);
    }

    // ── Path helper ───────────────────────────────────────────────────────────

    /// <summary>
    /// {FileItemRoot}/Hr/EmpDataItems/{year}{A|B}
    /// A = Jan–Jun, B = Jul–Dec (uploadDate 기준)
    /// </summary>
    private string GetUploadDir(DateTime uploadDate)
    {
        var halfYear = uploadDate.Month <= 6 ? "A" : "B";
        var folder   = $"{uploadDate.Year}{halfYear}";
        return Path.Combine(_storage.FileItemRoot, "Hr", "EmpDataItems", folder);
    }

    // ── Content type helper ───────────────────────────────────────────────────
    private static string GetContentType(string ext) => ext.ToLowerInvariant() switch
    {
        "pdf"             => "application/pdf",
        "jpg" or "jpeg"   => "image/jpeg",
        "png"             => "image/png",
        "gif"             => "image/gif",
        "webp"            => "image/webp",
        "doc"             => "application/msword",
        "docx"            => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "xls"             => "application/vnd.ms-excel",
        "xlsx"            => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "ppt"             => "application/vnd.ms-powerpoint",
        "pptx"            => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "txt"             => "text/plain",
        "csv"             => "text/csv",
        "xml"             => "application/xml",
        _                 => "application/octet-stream"
    };
}
