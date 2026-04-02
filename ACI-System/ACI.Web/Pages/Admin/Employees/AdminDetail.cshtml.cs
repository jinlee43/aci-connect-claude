using ACI.Web.Data;
using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Admin.Employees;

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

    // ── 화면 표시용 ────────────────────────────────────────────────────────
    public string EmpDisplayName { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var emp = await _db.Employees.FindAsync(id);
        if (emp == null) return NotFound();

        Emp             = emp;
        EmpDisplayName  = emp.DisplayName;

        // 복호화
        SsnPlain        = _enc.Decrypt(emp.SsnEncrypted);
        TinPlain        = _enc.Decrypt(emp.TinEncrypted);
        DriversLicPlain = _enc.Decrypt(emp.DriversLicNumEncrypted);
        AlienNumPlain   = _enc.Decrypt(emp.AlienNumberEncrypted);
        PassportNumPlain= _enc.Decrypt(emp.PassportNumberEncrypted);

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
            case "ssn":      emp.SsnEncrypted           = null; break;
            case "tin":      emp.TinEncrypted            = null; break;
            case "drvlic":   emp.DriversLicNumEncrypted  = null; break;
            case "alien":    emp.AlienNumberEncrypted    = null; break;
            case "passport": emp.PassportNumberEncrypted = null; break;
        }
        emp.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Field cleared.";
        return RedirectToPage("AdminDetail", new { id });
    }
}
