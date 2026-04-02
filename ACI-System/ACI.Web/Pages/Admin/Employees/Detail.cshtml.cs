using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Admin.Employees;

public class DetailModel : PageModel
{
    private readonly AppDbContext _db;
    public DetailModel(AppDbContext db) => _db = db;

    [BindProperty]
    public Employee Emp { get; set; } = new();

    public bool IsNew => Emp.Id == 0;

    public List<SelectListItem> OrgUnitOptions { get; set; } = [];
    public List<SelectListItem> JobPositionOptions { get; set; } = [];
    public List<EmpRole> Roles { get; set; } = [];

    // For adding a new role
    [BindProperty] public int NewRoleOrgUnitId { get; set; }
    [BindProperty] public int? NewRoleJobPositionId { get; set; }
    [BindProperty] public bool NewRoleIsPrimary { get; set; }
    [BindProperty] public DateOnly? NewRoleStartDate { get; set; }
    [BindProperty] public DateOnly? NewRoleEndDate { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        await LoadDropdownsAsync();

        if (id == 0)
        {
            Emp = new Employee { IsActive = true };
            return Page();
        }

        var emp = await _db.Employees
            .Include(e => e.EmpRoles)
                .ThenInclude(r => r.OrgUnit)
            .Include(e => e.EmpRoles)
                .ThenInclude(r => r.JobPosition)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (emp == null) return NotFound();

        Emp = emp;
        Roles = emp.EmpRoles.OrderByDescending(r => r.IsPrimary).ThenBy(r => r.OrgUnit?.Name).ToList();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        // Remove navigation props from validation
        ModelState.Remove("Emp.EmpRoles");
        ModelState.Remove("Emp.UserAccount");

        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            await ReloadRolesAsync();
            return Page();
        }

        if (Emp.Id == 0)
        {
            Emp.CreatedAt = DateTime.UtcNow;
            Emp.UpdatedAt = DateTime.UtcNow;
            _db.Employees.Add(Emp);
        }
        else
        {
            var existing = await _db.Employees.FindAsync(Emp.Id);
            if (existing == null) return NotFound();

            existing.EmpNum        = Emp.EmpNum;
            existing.FirstName     = Emp.FirstName;
            existing.MiddleName    = Emp.MiddleName;
            existing.LastName      = Emp.LastName;
            existing.NameSuffix    = Emp.NameSuffix;
            existing.KnownName     = Emp.KnownName;
            existing.Gender        = Emp.Gender;
            // Work contact
            existing.Phone         = Emp.Phone;
            existing.WorkEmail     = Emp.WorkEmail;
            // Private contact
            existing.PrivHomePhone  = Emp.PrivHomePhone;
            existing.PrivCellPhone  = Emp.PrivCellPhone;
            existing.PersonalEmail  = Emp.PersonalEmail;
            // Home address
            existing.HomeAddress1      = Emp.HomeAddress1;
            existing.HomeAddress2      = Emp.HomeAddress2;
            existing.HomeAddressCity   = Emp.HomeAddressCity;
            existing.HomeAddressState  = Emp.HomeAddressState;
            existing.HomeAddressZip    = Emp.HomeAddressZip;
            existing.HomeAddressCounty = Emp.HomeAddressCounty;
            // Emergency contacts
            existing.EmgContact1Name     = Emp.EmgContact1Name;
            existing.EmgContact1Relation = Emp.EmgContact1Relation;
            existing.EmgContact1Email    = Emp.EmgContact1Email;
            existing.EmgContact1Tel      = Emp.EmgContact1Tel;
            existing.EmgContact1Cell     = Emp.EmgContact1Cell;
            existing.EmgContact2Name     = Emp.EmgContact2Name;
            existing.EmgContact2Relation = Emp.EmgContact2Relation;
            existing.EmgContact2Email    = Emp.EmgContact2Email;
            existing.EmgContact2Tel      = Emp.EmgContact2Tel;
            existing.EmgContact2Cell     = Emp.EmgContact2Cell;
            existing.EmgContact3Name     = Emp.EmgContact3Name;
            existing.EmgContact3Relation = Emp.EmgContact3Relation;
            existing.EmgContact3Email    = Emp.EmgContact3Email;
            existing.EmgContact3Tel      = Emp.EmgContact3Tel;
            existing.EmgContact3Cell     = Emp.EmgContact3Cell;
            // Employment
            existing.HireDate      = Emp.HireDate;
            existing.TerminationDate = Emp.TerminationDate;
            existing.IsActive      = Emp.IsActive;
            existing.Notes         = Emp.Notes;
            existing.UpdatedAt     = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"{Emp.DisplayName} saved successfully.";
        return RedirectToPage("Detail", new { id = Emp.Id });
    }

    public async Task<IActionResult> OnPostAddRoleAsync(int id)
    {
        if (NewRoleOrgUnitId == 0)
        {
            TempData["Error"] = "Please select a department/team.";
            return RedirectToPage("Detail", new { id });
        }

        // If setting as primary, clear existing primary
        if (NewRoleIsPrimary)
        {
            var existing = await _db.EmpRoles
                .Where(r => r.EmployeeId == id && r.IsPrimary)
                .ToListAsync();
            existing.ForEach(r => r.IsPrimary = false);
        }

        _db.EmpRoles.Add(new EmpRole
        {
            EmployeeId    = id,
            OrgUnitId     = NewRoleOrgUnitId,
            JobPositionId = NewRoleJobPositionId,
            IsPrimary     = NewRoleIsPrimary,
            StartDate     = NewRoleStartDate,
            EndDate       = NewRoleEndDate,
            CreatedAt     = DateTime.UtcNow,
            UpdatedAt     = DateTime.UtcNow,
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = "Role added.";
        return RedirectToPage("Detail", new { id });
    }

    public async Task<IActionResult> OnPostDeleteRoleAsync(int id, int roleId)
    {
        var role = await _db.EmpRoles.FindAsync(roleId);
        if (role != null)
        {
            _db.EmpRoles.Remove(role);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Role removed.";
        }
        return RedirectToPage("Detail", new { id });
    }

    public async Task<IActionResult> OnPostSetPrimaryAsync(int id, int roleId)
    {
        var roles = await _db.EmpRoles.Where(r => r.EmployeeId == id).ToListAsync();
        foreach (var r in roles)
            r.IsPrimary = (r.Id == roleId);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Primary role updated.";
        return RedirectToPage("Detail", new { id });
    }

    private async Task LoadDropdownsAsync()
    {
        OrgUnitOptions = await _db.OrgUnits
            .Where(o => o.IsActive)
            .OrderBy(o => o.Name)
            .Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name })
            .ToListAsync();

        JobPositionOptions = await _db.JobPositions
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
            .ToListAsync();
    }

    private async Task ReloadRolesAsync()
    {
        Roles = await _db.EmpRoles
            .Include(r => r.OrgUnit)
            .Include(r => r.JobPosition)
            .Where(r => r.EmployeeId == Emp.Id)
            .OrderByDescending(r => r.IsPrimary)
            .ToListAsync();
    }
}
