using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Admin.Employees;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Employee> Employees { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool IncludeInactive { get; set; } = false;

    public async Task OnGetAsync()
    {
        var query = _db.Employees
            .Include(e => e.EmpRoles.Where(r => r.IsPrimary))
                .ThenInclude(r => r.OrgUnit)
            .Include(e => e.EmpRoles.Where(r => r.IsPrimary))
                .ThenInclude(r => r.JobPosition)
            .AsQueryable();

        if (!IncludeInactive)
            query = query.Where(e => e.IsActive);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var s = Search.Trim().ToLower();
            query = query.Where(e =>
                (e.KnownName != null && e.KnownName.ToLower().Contains(s)) ||
                e.FirstName.ToLower().Contains(s) ||
                e.LastName.ToLower().Contains(s) ||
                (e.WorkEmail != null && e.WorkEmail.ToLower().Contains(s)));
        }

        Employees = await query
            .OrderBy(e => e.KnownName ?? e.FirstName)
            .ToListAsync();
    }
}
