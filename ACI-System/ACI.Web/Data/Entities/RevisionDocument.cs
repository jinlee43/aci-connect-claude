using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

public enum RevisionDocumentType
{
    ChangeOrder    = 0,   // Change Order 문서
    Email          = 1,   // 이메일 캡처/첨부
    RFI            = 2,   // Request for Information
    OwnerLetter    = 3,   // 발주처 공문
    SubLetter      = 4,   // 하수급인 공문
    MeetingMinutes = 5,   // 회의록
    Photo          = 6,   // 현장 사진
    Drawing        = 7,   // 도면
    Specification  = 8,   // 시방서
    Other          = 99
}

/// <summary>
/// File attachment linked to a ScheduleRevision.
/// Supports Change Orders, emails, RFIs, letters, meeting minutes, etc.
/// New uploads stored at {ContentRoot}/uploads/revisions/{revisionId}/{guid}{ext}.
/// </summary>
public class RevisionDocument : BaseEntity
{
    public int RevisionId { get; set; }
    public ScheduleRevision Revision { get; set; } = null!;

    public RevisionDocumentType DocumentType { get; set; } = RevisionDocumentType.Other;

    /// <summary>Display name shown to users (original filename or user-provided title).</summary>
    [Required, MaxLength(300)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>Actual filename on disk (GUID-based). Empty for legacy/linked records.</summary>
    [MaxLength(400)]
    public string StoredFileName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Extension { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    /// <summary>Reference number: CO number, RFI number, email subject, etc.</summary>
    [MaxLength(200)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    /// <summary>Who uploaded this document (auto-populated).</summary>
    public int? UploadedById { get; set; }
    public ApplicationUser? UploadedBy { get; set; }

    [MaxLength(150)]
    public string UploadedByName { get; set; } = string.Empty;

    // ── Computed ─────────────────────────────────────────────────────────────
    public string FileIconClass => Extension.ToLowerInvariant() switch
    {
        "pdf"                  => "bi-file-pdf text-danger",
        "jpg" or "jpeg"
            or "png" or "gif"  => "bi-file-image text-success",
        "doc" or "docx"        => "bi-file-word text-primary",
        "xls" or "xlsx"        => "bi-file-excel text-success",
        "ppt" or "pptx"        => "bi-file-ppt text-warning",
        "eml" or "msg"         => "bi-envelope-fill text-info",
        "txt"                  => "bi-file-text text-secondary",
        "zip" or "rar"         => "bi-file-zip text-secondary",
        _                      => "bi-file-earmark text-secondary"
    };

    public string FileSizeDisplay
    {
        get
        {
            if (FileSizeBytes < 1024) return $"{FileSizeBytes} B";
            if (FileSizeBytes < 1024 * 1024) return $"{FileSizeBytes / 1024.0:F1} KB";
            return $"{FileSizeBytes / (1024.0 * 1024):F1} MB";
        }
    }

    public bool IsPreviewable => Extension.ToLowerInvariant() switch
    {
        "pdf" or "jpg" or "jpeg" or "png" or "gif" => true,
        _ => false
    };

    public string DocumentTypeLabel => DocumentType switch
    {
        RevisionDocumentType.ChangeOrder    => "Change Order",
        RevisionDocumentType.Email          => "Email",
        RevisionDocumentType.RFI            => "RFI",
        RevisionDocumentType.OwnerLetter    => "Owner Letter",
        RevisionDocumentType.SubLetter      => "Sub Letter",
        RevisionDocumentType.MeetingMinutes => "Meeting Minutes",
        RevisionDocumentType.Photo          => "Photo",
        RevisionDocumentType.Drawing        => "Drawing",
        RevisionDocumentType.Specification  => "Specification",
        _                                   => "Other"
    };
}
