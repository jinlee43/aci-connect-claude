using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Progress;

[Authorize]
public class RevisionsModel : PageModel
{
    private readonly AppDbContext             _db;
    private readonly IProgressScheduleService _svc;
    private readonly IWebHostEnvironment      _env;

    public RevisionsModel(AppDbContext db, IProgressScheduleService svc, IWebHostEnvironment env)
    {
        _db  = db;
        _svc = svc;
        _env = env;
    }

    [BindProperty(SupportsGet = true)] public int  ProjectId  { get; set; }
    [BindProperty(SupportsGet = true)] public int? RevisionId { get; set; }

    public string                  ProjectName  { get; set; } = string.Empty;
    public List<ScheduleRevision>  Revisions    { get; set; } = [];
    public ScheduleRevision?       Selected     { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var project = await _db.Projects.FindAsync(ProjectId);
        if (project == null) return NotFound();
        ProjectName = project.Name;

        Revisions = await _svc.GetRevisionsAsync(ProjectId);

        if (RevisionId.HasValue)
            Selected = await _svc.GetRevisionWithDetailsAsync(RevisionId.Value);
        else if (Revisions.Any())
            Selected = await _svc.GetRevisionWithDetailsAsync(Revisions.First().Id);

        return Page();
    }

    // Create new Draft revision
    public async Task<IActionResult> OnPostNewRevisionAsync(string title, string description,
        RevisionType revisionType, string? changeOrderRef, DateOnly? dataDate)
    {
        var user = await GetCurrentUserAsync();
        var rev  = await _svc.GetOrCreateDraftRevisionAsync(ProjectId, user.Id, user.Name);
        rev.Title          = title;
        rev.Description    = description;
        rev.RevisionType   = revisionType;
        rev.ChangeOrderRef = changeOrderRef;
        rev.DataDate       = dataDate;
        rev.UpdatedAt      = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Revision '{title}' created.";
        return RedirectToPage(new { projectId = ProjectId, revisionId = rev.Id });
    }

    // Submit for approval
    public async Task<IActionResult> OnPostSubmitAsync(int revisionId)
    {
        var user = await GetCurrentUserAsync();
        await _svc.SubmitRevisionAsync(revisionId, user.Id, user.Name);
        TempData["Success"] = "Revision submitted for approval.";
        return RedirectToPage(new { projectId = ProjectId, revisionId });
    }

    // Approve
    public async Task<IActionResult> OnPostApproveAsync(int revisionId,
        string approvedByName, string? approvalNotes, DateOnly? approvedDate)
    {
        await _svc.ApproveRevisionAsync(revisionId, approvedByName, approvalNotes, approvedDate);
        TempData["Success"] = "Revision approved.";
        return RedirectToPage(new { projectId = ProjectId, revisionId });
    }

    // Reject
    public async Task<IActionResult> OnPostRejectAsync(int revisionId, string? notes)
    {
        await _svc.RejectRevisionAsync(revisionId, notes);
        TempData["Info"] = "Revision rejected.";
        return RedirectToPage(new { projectId = ProjectId, revisionId });
    }

    // Upload document to revision
    public async Task<IActionResult> OnPostUploadDocAsync(int revisionId,
        IFormFile file, RevisionDocumentType documentType,
        string? referenceNumber, string? notes)
    {
        if (file == null || file.Length == 0)
        { TempData["Error"] = "No file selected."; goto redirect; }

        var allowed = new[] { "pdf","doc","docx","xls","xlsx","ppt","pptx",
                               "jpg","jpeg","png","gif","eml","msg","txt","zip" };
        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLowerInvariant();
        if (!allowed.Contains(ext))
        { TempData["Error"] = "File type not allowed."; goto redirect; }

        var user = await GetCurrentUserAsync();
        await _svc.AddDocumentAsync(revisionId, file, documentType,
                                    referenceNumber, notes, user.Id, user.Name, _env);
        TempData["Success"] = $"Document '{file.FileName}' attached.";

        redirect:
        return RedirectToPage(new { projectId = ProjectId, revisionId });
    }

    // Delete document
    public async Task<IActionResult> OnPostDeleteDocAsync(int documentId, int revisionId)
    {
        await _svc.DeleteDocumentAsync(documentId, _env);
        TempData["Success"] = "Document removed.";
        return RedirectToPage(new { projectId = ProjectId, revisionId });
    }

    // Serve document file
    public async Task<IActionResult> OnGetDocFileAsync(int docId, bool inline = false)
    {
        var doc = await _db.RevisionDocuments.FindAsync(docId);
        if (doc == null) return NotFound();
        var path = Path.Combine(_env.ContentRootPath, "uploads", "revisions",
                                doc.RevisionId.ToString(), doc.StoredFileName);
        if (!System.IO.File.Exists(path)) return NotFound();
        var mime = doc.Extension switch
        {
            "pdf"  => "application/pdf",
            "jpg" or "jpeg" => "image/jpeg",
            "png"  => "image/png",
            "gif"  => "image/gif",
            _      => "application/octet-stream"
        };
        var disp = inline && (doc.Extension is "pdf" or "jpg" or "jpeg" or "png" or "gif")
            ? "inline" : "attachment";
        return PhysicalFile(path, mime,
            disp == "inline" ? null : doc.FileName);
    }

    private async Task<ApplicationUser> GetCurrentUserAsync()
    {
        var idStr = User.FindFirst("UserId")?.Value
                 ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(idStr, out var userId) && userId > 0)
        {
            var byId = await _db.Users.FindAsync(userId);
            if (byId != null) return byId;
        }

        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (!string.IsNullOrEmpty(email))
        {
            var byEmail = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (byEmail != null) return byEmail;
        }

        var name = User.Identity?.Name;
        if (!string.IsNullOrEmpty(name))
        {
            var byName = await _db.Users.FirstOrDefaultAsync(u => u.Name == name);
            if (byName != null) return byName;
        }

        throw new InvalidOperationException($"Current user not found. Claims: UserId={idStr}, Name={User.Identity?.Name}");
    }
}
