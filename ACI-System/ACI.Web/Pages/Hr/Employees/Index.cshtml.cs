using ACI.Web.Data;
using ACI.Web.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Pages.Hr.Employees;

public class IndexModel : PageModel
{
    private readonly AppDbContext _db;
    public IndexModel(AppDbContext db) => _db = db;

    public List<Employee> Employees { get; set; } = [];

    [BindProperty(SupportsGet = true)] public string? Search          { get; set; }
    [BindProperty(SupportsGet = true)] public bool    IncludeInactive { get; set; } = false;
    [BindProperty(SupportsGet = true)] public int     PageSize        { get; set; } = 25;
    [BindProperty(SupportsGet = true)] public int     PageNum         { get; set; } = 1;
    /// <summary>멀티 정렬: "name:asc,dept:desc" 형식</summary>
    [BindProperty(SupportsGet = true)] public string? Sort            { get; set; }

    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public int[] PageSizeOptions { get; } = [10, 15, 20, 25, 30, 50, 100, 200];

    public async Task OnGetAsync()
    {
        // PageSize 유효성 보정
        if (!PageSizeOptions.Contains(PageSize)) PageSize = 20;
        if (PageNum < 1) PageNum = 1;

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

        // "name:asc,dept:desc" 형식 파싱 → OrderBy + ThenBy 체인
        var sortEntries = (Sort ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Split(':'))
            .Where(p => p.Length == 2)
            .Select(p => (col: p[0].Trim().ToLower(), desc: p[1].Trim().ToLower() == "desc"))
            .DistinctBy(e => e.col)
            .ToList();

        IOrderedQueryable<Employee>? ordered = null;
        foreach (var (col, desc) in sortEntries)
        {
            if (ordered == null)
                ordered = col switch
                {
                    "status" => desc ? query.OrderByDescending(e => e.IsActive)        : query.OrderBy(e => e.IsActive),
                    "since"  => desc ? query.OrderByDescending(e => e.HireDate)        : query.OrderBy(e => e.HireDate),
                    "dept"   => desc ? query.OrderByDescending(e => e.EmpRoles.Where(r => r.IsPrimary).Select(r => r.OrgUnit.Name).FirstOrDefault())
                                     : query.OrderBy(e => e.EmpRoles.Where(r => r.IsPrimary).Select(r => r.OrgUnit.Name).FirstOrDefault()),
                    "pos"    => desc ? query.OrderByDescending(e => e.EmpRoles.Where(r => r.IsPrimary).Select(r => r.JobPosition!.Name).FirstOrDefault())
                                     : query.OrderBy(e => e.EmpRoles.Where(r => r.IsPrimary).Select(r => r.JobPosition!.Name).FirstOrDefault()),
                    "phone"  => desc ? query.OrderByDescending(e => e.Phone)           : query.OrderBy(e => e.Phone),
                    "email"  => desc ? query.OrderByDescending(e => e.WorkEmail)       : query.OrderBy(e => e.WorkEmail),
                    "city"   => desc ? query.OrderByDescending(e => e.HomeAddressCity) : query.OrderBy(e => e.HomeAddressCity),
                    _        => desc ? query.OrderByDescending(e => e.KnownName ?? e.FirstName)
                                     : query.OrderBy(e => e.KnownName ?? e.FirstName),
                };
            else
                ordered = col switch
                {
                    "status" => desc ? ordered.ThenByDescending(e => e.IsActive)        : ordered.ThenBy(e => e.IsActive),
                    "since"  => desc ? ordered.ThenByDescending(e => e.HireDate)        : ordered.ThenBy(e => e.HireDate),
                    "dept"   => desc ? ordered.ThenByDescending(e => e.EmpRoles.Where(r => r.IsPrimary).Select(r => r.OrgUnit.Name).FirstOrDefault())
                                     : ordered.ThenBy(e => e.EmpRoles.Where(r => r.IsPrimary).Select(r => r.OrgUnit.Name).FirstOrDefault()),
                    "pos"    => desc ? ordered.ThenByDescending(e => e.EmpRoles.Where(r => r.IsPrimary).Select(r => r.JobPosition!.Name).FirstOrDefault())
                                     : ordered.ThenBy(e => e.EmpRoles.Where(r => r.IsPrimary).Select(r => r.JobPosition!.Name).FirstOrDefault()),
                    "phone"  => desc ? ordered.ThenByDescending(e => e.Phone)           : ordered.ThenBy(e => e.Phone),
                    "email"  => desc ? ordered.ThenByDescending(e => e.WorkEmail)       : ordered.ThenBy(e => e.WorkEmail),
                    "city"   => desc ? ordered.ThenByDescending(e => e.HomeAddressCity) : ordered.ThenBy(e => e.HomeAddressCity),
                    _        => desc ? ordered.ThenByDescending(e => e.KnownName ?? e.FirstName)
                                     : ordered.ThenBy(e => e.KnownName ?? e.FirstName),
                };
        }
        query = ordered ?? query.OrderBy(e => e.KnownName ?? e.FirstName);

        TotalCount = await query.CountAsync();

        // PageNum 범위 보정
        if (PageNum > TotalPages && TotalPages > 0) PageNum = TotalPages;

        Employees = await query
            .Skip((PageNum - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();
    }
}
