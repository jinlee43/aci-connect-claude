using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Hr.Employees;

/// <summary>
/// 직원 기본 정보 편집(Detail). HrUser 이상 접근 가능.
/// EmpRole(부서/직책) 부여/변경/해제는 HrAdmin 권한이 필요하므로 AdminDetail 페이지로 분리.
/// Detail 에서는 Roles 목록을 읽기 전용으로만 표시.
/// </summary>
public class DetailModel : PageModel
{
    private readonly AppDbContext _db;
    public DetailModel(AppDbContext db) => _db = db;

    [BindProperty]
    public Employee Emp { get; set; } = new();

    public bool IsNew => Emp.Id == 0;

    /// <summary>읽기 전용 역할(부서·직책) 목록 — 편집은 AdminDetail 에서.</summary>
    public List<EmpRole> Roles { get; set; } = [];
    public List<EmployeeDocument> Documents { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
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
        Roles = emp.EmpRoles
            .OrderByDescending(r => r.IsPrimary)
            .ThenBy(r => r.OrgUnit?.Name)
            .ToList();
        Documents = await _db.EmployeeDocuments
            .Where(d => d.EmployeeId == id && d.IsActive)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        // Remove navigation props from validation
        ModelState.Remove("Emp.EmpRoles");
        ModelState.Remove("Emp.UserAccount");

        if (!ModelState.IsValid)
        {
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
            existing.HireDate        = Emp.HireDate;
            existing.TerminationDate = Emp.TerminationDate;
            existing.IsActive        = Emp.IsActive;
            existing.Notes           = Emp.Notes;
            existing.UpdatedAt       = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = $"{Emp.DisplayName} saved successfully.";
        return RedirectToPage("Detail", new { id = Emp.Id });
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
