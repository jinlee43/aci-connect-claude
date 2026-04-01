using System.ComponentModel.DataAnnotations;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ACI.Web.Pages.Projects;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IProjectService _svc;
    public CreateModel(IProjectService svc) => _svc = svc;

    [BindProperty] public ProjectInputModel Input { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var project = new Project
        {
            ProjectCode    = Input.ProjectCode,
            Name           = Input.Name,
            Description    = Input.Description,
            SiteAddress    = Input.SiteAddress,
            City           = Input.City,
            Type           = Input.Type,
            Status         = Input.Status,
            ContractAmount = Input.ContractAmount,
            SchdStartDate  = Input.SchdStartDate,
            SchdEndDate    = Input.SchdEndDate,
            OwnerName      = Input.OwnerName,
            OwnerEmail     = Input.OwnerEmail,
        };

        var created = await _svc.CreateProjectAsync(project);
        TempData["Success"] = $"Project '{created.Name}' created.";
        return RedirectToPage("/Schedule/Index", new { projectId = created.Id });
    }

    public class ProjectInputModel
    {
        [Required, MaxLength(30)]  public string ProjectCode  { get; set; } = string.Empty;
        [Required, MaxLength(200)] public string Name         { get; set; } = string.Empty;
        public string? Description  { get; set; }
        public string? SiteAddress  { get; set; }
        public string? City         { get; set; }
        public ProjectType   Type   { get; set; } = ProjectType.LumpSum;
        public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

        [Range(0, double.MaxValue)]
        public decimal ContractAmount { get; set; }

        [Required] public DateOnly SchdStartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        [Required] public DateOnly SchdEndDate   { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddMonths(6));
        public string? OwnerName  { get; set; }
        public string? OwnerEmail { get; set; }
    }
}
