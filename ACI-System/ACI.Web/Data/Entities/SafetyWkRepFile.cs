using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

/// <summary>
/// A single file attached to a weekly safety report.
/// One SafetyWkRep can have many SafetyWkRepFiles.
/// </summary>
public class SafetyWkRepFile : BaseEntity
{
    public int        ReportId { get; set; }
    public SafetyWkRep Report  { get; set; } = null!;

    // ── File info ────────────────────────────────────────────────────────────
    [MaxLength(260)]
    public string FileName { get; set; } = "";       // Original file name shown to users

    [MaxLength(100)]
    public string StoredFileName { get; set; } = ""; // GUID-based name on disk

    [MaxLength(20)]
    public string? Extension { get; set; }

    public long FileSize { get; set; } = 0;

    // ── Upload audit ─────────────────────────────────────────────────────────
    public int? UploadedById { get; set; }
    public ApplicationUser? UploadedBy { get; set; }

    [MaxLength(150)]
    public string?   UploadedByName { get; set; }
    public DateTime? UploadedAt    { get; set; }
}
