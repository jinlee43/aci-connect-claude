using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Hr.Employees;

// 상위 폴더 규약(Program.cs 의 AuthorizeFolder("/Hr", "Hr"))은 HrUser 이상을 허용하지만,
// AdminDetail 은 HrAdmin 전용이므로 페이지 레벨에서 강한 정책을 추가로 적용.
// EmpRole 관리 핸들러(AddRole/SetPrimary/DeleteRole)도 이 정책에 의해 동일하게 보호됨.
[Authorize(Policy = "HrAdmin")]
public class AdminDetailModel : PageModel
{
    private readonly AppDbContext _db;
    private readonly IEncryptionService _enc;

    public AdminDetailModel(AppDbContext db, IEncryptionService enc)
    {
        _db  = db;
        _enc = enc;
    }

    [BindProperty]
    public Employee Emp { get; set; } = new();

    // ── 복호화된 민감 필드 (화면 바인딩용) ──────────────────────────────────
    [BindProperty] public string? SsnPlain         { get; set; }
    [BindProperty] public string? TinPlain         { get; set; }
    [BindProperty] public string? DriversLicPlain  { get; set; }
    [BindProperty] public string? AlienNumPlain    { get; set; }
    [BindProperty] public string? PassportNumPlain { get; set; }

    // ── EmpRole 관리용 (HrAdmin 전용) ───────────────────────────────────────
    [BindProperty] public int       NewRoleOrgUnitId     { get; set; }
    [BindProperty] public int?      NewRoleJobPositionId { get; set; }
    [BindProperty] public bool      NewRoleIsPrimary     { get; set; }
    [BindProperty] public DateOnly? NewRoleStartDate     { get; set; }
    [BindProperty] public DateOnly? NewRoleEndDate       { get; set; }

    public List<EmpRole>       Roles              { get; set; } = [];
    public List<SelectListItem> OrgUnitOptions     { get; set; } = [];
    public List<SelectListItem> JobPositionOptions { get; set; } = [];

    // ── 화면 표시용 ────────────────────────────────────────────────────────
    public string EmpDisplayName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var emp = await _db.Employees
            .Include(e => e.EmpRoles)
                .ThenInclude(r => r.OrgUnit)
            .Include(e => e.EmpRoles)
                .ThenInclude(r => r.JobPosition)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (emp == null) return NotFound();

        Emp             = emp;
        EmpDisplayName  = emp.DisplayName;

        // 복호화
        SsnPlain        = _enc.Decrypt(emp.SsnEncrypted);
        TinPlain        = _enc.Decrypt(emp.TinEncrypted);
        DriversLicPlain = _enc.Decrypt(emp.DriversLicNumEncrypted);
        AlienNumPlain   = _enc.Decrypt(emp.AlienNumberEncrypted);
        PassportNumPlain= _enc.Decrypt(emp.PassportNumberEncrypted);

        Roles = emp.EmpRoles
            .OrderByDescending(r => r.IsPrimary)
            .ThenBy(r => r.OrgUnit?.Name)
            .ToList();

        await LoadDropdownsAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostSaveAsync(int id)
    {
        var emp = await _db.Employees.FindAsync(id);
        if (emp == null) return NotFound();

        // ── Employment Admin ───────────────────────────────────────────────
        emp.ApplyDate         = Emp.ApplyDate;
        emp.IsReEmp           = Emp.IsReEmp;
        emp.OldStartDate      = Emp.OldStartDate;
        emp.TerminationType   = Emp.TerminationType;
        emp.BkgrndCheckDate   = Emp.BkgrndCheckDate;
        emp.BackgroudCheckOk  = Emp.BackgroudCheckOk;
        emp.DrugScreeningDate = Emp.DrugScreeningDate;
        emp.DrugScreeningOk   = Emp.DrugScreeningOk;

        // ── Benefits ───────────────────────────────────────────────────────
        emp.HealthStartDate  = Emp.HealthStartDate;
        emp.HealthEndDate    = Emp.HealthEndDate;
        emp.DentalStartDate  = Emp.DentalStartDate;
        emp.DentalEndDate    = Emp.DentalEndDate;
        emp.VisionStartDate  = Emp.VisionStartDate;
        emp.VisionEndDate    = Emp.VisionEndDate;
        emp.Eligible401kDate = Emp.Eligible401kDate;
        emp.Enrolled401kDate = Emp.Enrolled401kDate;

        // ── Legal / ID ─────────────────────────────────────────────────────
        emp.StatusName             = Emp.StatusName;
        emp.DriversLicIssuedDate   = Emp.DriversLicIssuedDate;
        emp.DriversLicExpiration   = Emp.DriversLicExpiration;
        emp.AlienCardIssuedDate    = Emp.AlienCardIssuedDate;
        emp.AlienCardExpirationDate= Emp.AlienCardExpirationDate;
        emp.PassportIssedDate      = Emp.PassportIssedDate;
        emp.PassportExpiredDate    = Emp.PassportExpiredDate;

        // ── 암호화 저장 (입력값이 있을 때만 갱신, 빈칸이면 기존값 유지) ──────
        if (SsnPlain        != null) emp.SsnEncrypted          = _enc.Encrypt(SsnPlain.Trim())        ?? emp.SsnEncrypted;
        if (TinPlain        != null) emp.TinEncrypted          = _enc.Encrypt(TinPlain.Trim())        ?? emp.TinEncrypted;
        if (DriversLicPlain != null) emp.DriversLicNumEncrypted= _enc.Encrypt(DriversLicPlain.Trim()) ?? emp.DriversLicNumEncrypted;
        if (AlienNumPlain   != null) emp.AlienNumberEncrypted  = _enc.Encrypt(AlienNumPlain.Trim())   ?? emp.AlienNumberEncrypted;
        if (PassportNumPlain!= null) emp.PassportNumberEncrypted= _enc.Encrypt(PassportNumPlain.Trim())?? emp.PassportNumberEncrypted;

        emp.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "HR Admin information saved.";
        return RedirectToPage("AdminDetail", new { id });
    }

    /// <summary>민감 필드 값을 지웁니다 (개별 필드 삭제용).</summary>
    public async Task<IActionResult> OnPostClearFieldAsync(int id, string field)
    {
        var emp = await _db.Employees.FindAsync(id);
        if (emp == null) return NotFound();

        switch (field)
        {
            case "ssn":      emp.SsnEncrypted            = null; break;
            case "tin":      emp.TinEncrypted            = null; break;
            case "drvlic":   emp.DriversLicNumEncrypted  = null; break;
            case "alien":    emp.AlienNumberEncrypted    = null; break;
            case "passport": emp.PassportNumberEncrypted = null; break;
        }
        emp.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Field cleared.";
        return RedirectToPage("AdminDetail", new { id });
    }

    // ────────────────────────────────────────────────────────────────────────
    //  EmpRole 관리 (HrAdmin 전용)
    // ────────────────────────────────────────────────────────────────────────

    /// <summary>직원에게 새 Role(부서/직책) 을 부여합니다.</summary>
    public async Task<IActionResult> OnPostAddRoleAsync(int id)
    {
        if (NewRoleOrgUnitId == 0)
        {
            TempData["Error"] = "Please select a department/team.";
            return RedirectToPage("AdminDetail", new { id });
        }

        // Primary 로 지정하면 기존 Primary 해제
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
        return RedirectToPage("AdminDetail", new { id });
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
        return RedirectToPage("AdminDetail", new { id });
    }

    public async Task<IActionResult> OnPostSetPrimaryAsync(int id, int roleId)
    {
        var roles = await _db.EmpRoles.Where(r => r.EmployeeId == id).ToListAsync();
        foreach (var r in roles)
            r.IsPrimary = (r.Id == roleId);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Primary role updated.";
        return RedirectToPage("AdminDetail", new { id });
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
}
