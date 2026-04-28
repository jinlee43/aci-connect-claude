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
            City           = project.City,
            ZipCode        = project.ZipCode,
            State          = project.State,
            Latitude       = project.Latitude,
            Longitude      = project.Longitude,
            OwnerName      = project.OwnerName,
            OwnerContact   = project.OwnerContact,
            OwnerEmail     = project.OwnerEmail,
            OwnerAddress   = project.OwnerAddress,
            OwnerCity      = project.OwnerCity,
            OwnerZipCode   = project.OwnerZipCode,
            OwnerState     = project.OwnerState,
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
                City            = Input.City,
                ZipCode         = Input.ZipCode,
                State           = Input.State,
                Latitude        = Input.Latitude,
                Longitude       = Input.Longitude,
                OwnerName       = Input.OwnerName,
                OwnerContact    = Input.OwnerContact,
                OwnerEmail      = Input.OwnerEmail,
                OwnerAddress    = Input.OwnerAddress,
                OwnerCity       = Input.OwnerCity,
                OwnerZipCode    = Input.OwnerZipCode,
                OwnerState      = Input.OwnerState,
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
            project.City            = Input.City;
            project.ZipCode         = Input.ZipCode;
            project.State           = Input.State;
            project.Latitude        = Input.Latitude;
            project.Longitude       = Input.Longitude;
            project.OwnerName       = Input.OwnerName;
            project.OwnerContact    = Input.OwnerContact;
            project.OwnerEmail      = Input.OwnerEmail;
            project.OwnerAddress    = Input.OwnerAddress;
            project.OwnerCity       = Input.OwnerCity;
            project.OwnerZipCode    = Input.OwnerZipCode;
            project.OwnerState      = Input.OwnerState;
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
        public ProjectType  Type           { get; set; } = ProjectType.LumpSum;
        public ProjectStatus Status        { get; set; } = ProjectStatus.Planning;
        public string?      SiteAddress    { get; set; }
        public string?      City           { get; set; }
        public string?      ZipCode        { get; set; }
        public string?      State          { get; set; }
        public double?      Latitude       { get; set; }
        public double?      Longitude      { get; set; }
        public string?      OwnerName      { get; set; }
        public string?      OwnerContact   { get; set; }
        public string?      OwnerEmail     { get; set; }
        public string?      OwnerAddress   { get; set; }
        public string?      OwnerCity      { get; set; }
        public string?      OwnerZipCode   { get; set; }
        public string?      OwnerState     { get; set; }

        [Range(0, double.MaxValue)]
        public decimal      ContractAmount { get; set; }

        public DateOnly?    SchdStartDate   { get; set; }
        public DateOnly?    SchdEndDate     { get; set; }
        public DateOnly?    ActualStartDate { get; set; }
        public DateOnly?    ActualEndDate   { get; set; }
        public DateOnly?    CompletedDate   { get; set; }
    }
}
