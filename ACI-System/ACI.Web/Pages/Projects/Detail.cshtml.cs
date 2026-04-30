using System.ComponentModel.DataAnnotations;
using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ACI.Web.Pages.Projects;

[Authorize]
public class DetailModel : PageModel
{
    private readonly IProjectService _svc;
    private readonly AppDbContext    _db;

    public DetailModel(IProjectService svc, AppDbContext db)
    {
        _svc = svc;
        _db  = db;
    }

    // ── Route ─────────────────────────────────────────────────────────────────
    [BindProperty(SupportsGet = true)]
    public int? Id { get; set; }

    // ── View state ────────────────────────────────────────────────────────────
    public bool IsNew    => Id is null or 0;
    public bool CanEdit  { get; set; }

    // ── Form input ────────────────────────────────────────────────────────────
    [BindProperty] public ProjectInputModel Input { get; set; } = new();

    // ── GET ───────────────────────────────────────────────────────────────────
    public async Task<IActionResult> OnGetAsync()
    {
        CanEdit = User.IsInRole(PrivilegeCodes.HrAdmin);

        if (IsNew)
        {
            // 신규 생성: HrAdmin만 접근 가능
            if (!CanEdit) return Forbid();
            return Page();
        }

        // 기존 프로젝트 조회
        var project = await _svc.GetProjectAsync(Id!.Value);
        if (project == null) return NotFound();

        Input = new ProjectInputModel
        {
            ProjectCode    = project.ProjectCode,
            Name           = project.Name,
            Description    = project.Description,
            Type           = project.Type,
            Status         = project.Status,
            SiteAddress    = project.SiteAddress,
            SiteAddress2   = project.SiteAddress2,
            City           = project.City,
            ZipCode        = project.ZipCode,
            State          = project.State,
            Latitude       = project.Latitude,
            Longitude      = project.Longitude,
            OwnerName          = project.OwnerName,
            OwnerPhone         = project.OwnerPhone,
            OwnerAddress       = project.OwnerAddress,
            OwnerAddress2      = project.OwnerAddress2,
            OwnerCity          = project.OwnerCity,
            OwnerZipCode       = project.OwnerZipCode,
            OwnerState         = project.OwnerState,
            OwnerContact1Name  = project.OwnerContact1Name,
            OwnerContact1Title = project.OwnerContact1Title,
            OwnerContact1Phone = project.OwnerContact1Phone,
            OwnerContact1Cell  = project.OwnerContact1Cell,
            OwnerContact1Email = project.OwnerContact1Email,
            OwnerContact2Name  = project.OwnerContact2Name,
            OwnerContact2Title = project.OwnerContact2Title,
            OwnerContact2Phone = project.OwnerContact2Phone,
            OwnerContact2Cell  = project.OwnerContact2Cell,
            OwnerContact2Email = project.OwnerContact2Email,
            ContractAmount = project.ContractAmount,
            SchdStartDate  = project.SchdStartDate,
            SchdEndDate    = project.SchdEndDate,
            ActualStartDate = project.ActualStartDate,
            ActualEndDate  = project.ActualEndDate,
            CompletedDate  = project.CompletedDate,
        };

        return Page();
    }

    // ── POST: Save (신규 생성 or 편집) ────────────────────────────────────────
    public async Task<IActionResult> OnPostAsync()
    {
        if (!User.IsInRole(PrivilegeCodes.HrAdmin)) return Forbid();
        CanEdit = true;

        if (!ModelState.IsValid) return Page();

        if (IsNew)
        {
            // 신규 생성
            var project = new Project
            {
                ProjectCode     = Input.ProjectCode,
                Name            = Input.Name,
                Description     = Input.Description,
                Type            = Input.Type,
                Status          = Input.Status,
                SiteAddress     = Input.SiteAddress,
                SiteAddress2    = Input.SiteAddress2,
                City            = Input.City,
                ZipCode         = Input.ZipCode,
                State           = Input.State,
                Latitude        = Input.Latitude,
                Longitude       = Input.Longitude,
                OwnerName          = Input.OwnerName,
                OwnerPhone         = Input.OwnerPhone,
                OwnerAddress       = Input.OwnerAddress,
                OwnerAddress2      = Input.OwnerAddress2,
                OwnerCity          = Input.OwnerCity,
                OwnerZipCode       = Input.OwnerZipCode,
                OwnerState         = Input.OwnerState,
                OwnerContact1Name  = Input.OwnerContact1Name,
                OwnerContact1Title = Input.OwnerContact1Title,
                OwnerContact1Phone = Input.OwnerContact1Phone,
                OwnerContact1Cell  = Input.OwnerContact1Cell,
                OwnerContact1Email = Input.OwnerContact1Email,
                OwnerContact2Name  = Input.OwnerContact2Name,
                OwnerContact2Title = Input.OwnerContact2Title,
                OwnerContact2Phone = Input.OwnerContact2Phone,
                OwnerContact2Cell  = Input.OwnerContact2Cell,
                OwnerContact2Email = Input.OwnerContact2Email,
                ContractAmount  = Input.ContractAmount,
                SchdStartDate   = Input.SchdStartDate,
                SchdEndDate     = Input.SchdEndDate,
                ActualStartDate = Input.ActualStartDate,
                ActualEndDate   = Input.ActualEndDate,
                CompletedDate   = Input.CompletedDate,
            };

            var created = await _svc.CreateProjectAsync(project);
            TempData["Success"] = $"Project '{created.Name}' created.";
            return RedirectToPage(new { id = created.Id });
        }
        else
        {
            // 기존 프로젝트 편집
            var project = await _svc.GetProjectAsync(Id!.Value);
            if (project == null) return NotFound();

            project.ProjectCode     = Input.ProjectCode;
            project.Name            = Input.Name;
            project.Description     = Input.Description;
            project.Type            = Input.Type;
            project.Status          = Input.Status;
            project.SiteAddress     = Input.SiteAddress;
            project.SiteAddress2    = Input.SiteAddress2;
            project.City            = Input.City;
            project.ZipCode         = Input.ZipCode;
            project.State           = Input.State;
            project.Latitude        = Input.Latitude;
            project.Longitude       = Input.Longitude;
            project.OwnerName          = Input.OwnerName;
            project.OwnerPhone         = Input.OwnerPhone;
            project.OwnerAddress       = Input.OwnerAddress;
            project.OwnerAddress2      = Input.OwnerAddress2;
            project.OwnerCity          = Input.OwnerCity;
            project.OwnerZipCode       = Input.OwnerZipCode;
            project.OwnerState         = Input.OwnerState;
            project.OwnerContact1Name  = Input.OwnerContact1Name;
            project.OwnerContact1Title = Input.OwnerContact1Title;
            project.OwnerContact1Phone = Input.OwnerContact1Phone;
            project.OwnerContact1Cell  = Input.OwnerContact1Cell;
            project.OwnerContact1Email = Input.OwnerContact1Email;
            project.OwnerContact2Name  = Input.OwnerContact2Name;
            project.OwnerContact2Title = Input.OwnerContact2Title;
            project.OwnerContact2Phone = Input.OwnerContact2Phone;
            project.OwnerContact2Cell  = Input.OwnerContact2Cell;
            project.OwnerContact2Email = Input.OwnerContact2Email;
            project.ContractAmount  = Input.ContractAmount;
            project.SchdStartDate   = Input.SchdStartDate;
            project.SchdEndDate     = Input.SchdEndDate;
            project.ActualStartDate = Input.ActualStartDate;
            project.ActualEndDate   = Input.ActualEndDate;
            project.CompletedDate   = Input.CompletedDate;

            await _svc.UpdateProjectAsync(project);
            TempData["Success"] = "Project updated.";
            return RedirectToPage(new { id = Id });
        }
    }

    // ── Input model ───────────────────────────────────────────────────────────
    public class ProjectInputModel
    {
        [Required, MaxLength(30)]  public string  ProjectCode     { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string  Name            { get; set; } = string.Empty;
        public string?      Description    { get; set; }
        public ProjectType  Type           { get; set; } = ProjectType.JOC;
        public ProjectStatus Status        { get; set; } = ProjectStatus.Active;
        public string?      SiteAddress    { get; set; }
        public string?      SiteAddress2   { get; set; }
        public string?      City           { get; set; }
        public string?      ZipCode        { get; set; }
        public string?      State          { get; set; }
        public double?      Latitude       { get; set; }
        public double?      Longitude      { get; set; }
        public string?      OwnerName          { get; set; }
        public string?      OwnerPhone         { get; set; }
        public string?      OwnerAddress       { get; set; }
        public string?      OwnerAddress2      { get; set; }
        public string?      OwnerCity          { get; set; }
        public string?      OwnerZipCode       { get; set; }
        public string?      OwnerState         { get; set; }
        public string?      OwnerContact1Name  { get; set; }
        public string?      OwnerContact1Title { get; set; }
        public string?      OwnerContact1Phone { get; set; }
        public string?      OwnerContact1Cell  { get; set; }
        public string?      OwnerContact1Email { get; set; }
        public string?      OwnerContact2Name  { get; set; }
        public string?      OwnerContact2Title { get; set; }
        public string?      OwnerContact2Phone { get; set; }
        public string?      OwnerContact2Cell  { get; set; }
        public string?      OwnerContact2Email { get; set; }

        [Range(0, double.MaxValue)]
        public decimal      ContractAmount { get; set; }

        public DateOnly?    SchdStartDate   { get; set; }
        public DateOnly?    SchdEndDate     { get; set; }
        public DateOnly?    ActualStartDate { get; set; }
        public DateOnly?    ActualEndDate   { get; set; }
        public DateOnly?    CompletedDate   { get; set; }
    }
}
