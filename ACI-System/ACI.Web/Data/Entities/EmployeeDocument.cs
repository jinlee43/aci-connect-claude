using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Web.Data.Entities;

/// <summary>
/// File attachment associated with an employee.
/// New uploads are stored on disk; migrated records from old system retain the original NAS path via LegacyPath.
/// </summary>
public class EmployeeDocument : BaseEntity
{
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    /// <summary>Original file name shown to users (e.g. "W2_2023.pdf")</summary>
    [Required, MaxLength(300)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>Actual file name on disk (GUID-based, unique). Empty for legacy-only records.</summary>
    [MaxLength(400)]
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>Lowercase extension without dot: "pdf", "jpg", "docx"</summary>
    [MaxLength(20)]
    public string Extension { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    [MaxLength(150)]
    public string? UploadedByName { get; set; }

    /// <summary>
    /// For records migrated from old system: the original NAS/server file path.
    /// Used as a fallback when StoredFileName is empty.
    /// </summary>
    [MaxLength(500)]
    public string? LegacyPath { get; set; }

    // ── Computed helpers ──────────────────────────────────────────────────────

    [NotMapped]
    public bool IsLegacyOnly => string.IsNullOrEmpty(StoredFileName) && !string.IsNullOrEmpty(LegacyPath);

    [NotMapped]
    public string FileIconClass => Extension.ToLowerInvariant() switch
    {
        "pdf"                        => "bi-file-pdf text-danger",
        "jpg" or "jpeg" or "png"
            or "gif" or "webp"
            or "bmp" or "tiff"       => "bi-file-image text-success",
        "doc" or "docx"              => "bi-file-word text-primary",
        "xls" or "xlsx"              => "bi-file-excel text-success",
        "ppt" or "pptx"              => "bi-file-ppt text-warning",
        "txt" or "csv"               => "bi-file-text text-secondary",
        "zip" or "rar" or "7z"       => "bi-file-zip text-secondary",
        _                            => "bi-file-earmark text-secondary"
    };

    [NotMapped]
    public string FileSizeDisplay
    {
        get
        {
            if (FileSizeBytes < 1024) return $"{FileSizeBytes} B";
            if (FileSizeBytes < 1024 * 1024) return $"{FileSizeBytes / 1024.0:F1} KB";
            return $"{FileSizeBytes / (1024.0 * 1024):F1} MB";
        }
    }

    [NotMapped]
    public bool IsPreviewable => Extension.ToLowerInvariant() switch
    {
        "pdf" or "jpg" or "jpeg" or "png" or "gif" or "webp" => true,
        _ => false
    };
}
