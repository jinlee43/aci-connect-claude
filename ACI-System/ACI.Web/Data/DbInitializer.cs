using ACI.Web.Data.Entities;
using ACI.Web.Services;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Data;

/// <summary>
/// Development seed data. Only runs if tables are empty.
/// Production data is loaded via migration SQL from the existing DB.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();

        // ── Job Positions ─────────────────────────────────────────────────
        if (!db.JobPositions.Any())
        {
            var positions = new List<JobPosition>
            {
                new() { Code = "SVP",   Name = "Senior Vice President",    OrdNum = 100  },
                new() { Code = "VP",    Name = "Vice President",           OrdNum = 200  },
                new() { Code = "SPM",   Name = "Senior PM",                OrdNum = 300  },
                new() { Code = "PM",    Name = "Project Manager",          OrdNum = 400  },
                new() { Code = "PE",    Name = "Project Engineer",         OrdNum = 500  },
                new() { Code = "APM",   Name = "Assistant PM",             OrdNum = 600  },
                new() { Code = "SUPT",  Name = "Superintendent",           OrdNum = 700  },
                new() { Code = "SSUPT", Name = "Senior Superintendent",    OrdNum = 750  },
                new() { Code = "ASUPT", Name = "Assistant Superintendent", OrdNum = 800  },
                new() { Code = "SF",    Name = "Safety Manager",           OrdNum = 900  },
                new() { Code = "EST",   Name = "Estimator",                OrdNum = 1000 },
                new() { Code = "ACC",   Name = "Accounting Manager",       OrdNum = 1100 },
                new() { Code = "IT",    Name = "IT Manager",               OrdNum = 1200 },
            };
            db.JobPositions.AddRange(positions);
            await db.SaveChangesAsync();
        }

        // ── OrgUnits ──────────────────────────────────────────────────────
        if (!db.OrgUnits.Any())
        {
            var company = new OrgUnit { Code = "ACI", Name = "Angeles Contractor Inc.", Type = OrgUnitType.Company };
            db.OrgUnits.Add(company);
            await db.SaveChangesAsync();

            var units = new List<OrgUnit>
            {
                new() { Code = "LS",   Name = "Lumpsum Division",     Type = OrgUnitType.Division,   ParentId = company.Id },
                new() { Code = "JOC",  Name = "JOC Division",         Type = OrgUnitType.Division,   ParentId = company.Id },
                new() { Code = "ADM",  Name = "Administration",       Type = OrgUnitType.Department, ParentId = company.Id },
                new() { Code = "SAF",  Name = "Safety Department",    Type = OrgUnitType.Department, ParentId = company.Id },
                new() { Code = "IT",   Name = "IT Division",          Type = OrgUnitType.Team,       ParentId = company.Id },
            };
            db.OrgUnits.AddRange(units);
            await db.SaveChangesAsync();
        }

        // ── Privileges (빌트인) ────────────────────────────────────────────
        // PrivilegeCodes.All 기준으로 누락된 priv 를 idempotent 하게 시드.
        // 이미 존재하는 Code 는 건드리지 않음(관리자가 Name/Description 을 수정했을 수 있음).
        var builtInSpecs = new (string Code, string Name, string Description)[]
        {
            (PrivilegeCodes.Admin,          "System Admin",       "Full system access. Includes all HR and Project admin privileges."),
            (PrivilegeCodes.HrAdmin,        "HR Admin",           "Access sensitive HR data (PII) and manage employee role assignments."),
            (PrivilegeCodes.HrManager,      "HR Manager",         "HR approvals and management tasks. Includes HR User privileges."),
            (PrivilegeCodes.HrUser,         "HR User",            "Basic HR access: view employees and edit general details."),
            (PrivilegeCodes.LsProjAdmin,    "LS Project Admin",   "Edit Lump Sum project master data (Trades & Subs, etc)."),
            (PrivilegeCodes.JocProjAdmin,   "JOC Project Admin",  "Edit JOC project master data (Trades & Subs, etc)."),
            (PrivilegeCodes.SafetyAdmin,     "Safety Admin",       "Full safety management including approval revocation."),
            (PrivilegeCodes.SafetyManager,  "Safety Manager",     "Safety report approval and management."),
            (PrivilegeCodes.SafetyUser,     "Safety User",        "Safety report upload and basic access."),
            // ProjectManager / Superintendent 은 HR 파생 롤이므로 Privilege row 없음
            (PrivilegeCodes.TradePartner,   "Trade Partner",      "Subcontractor: update status of own tasks only."),
            (PrivilegeCodes.Viewer,         "Viewer",             "Read-only access."),
        };

        var existingCodes = await db.Privileges
            .Select(p => p.Code)
            .ToListAsync();
        var missing = builtInSpecs
            .Where(s => !existingCodes.Contains(s.Code))
            .Select(s => new Privilege
            {
                Code        = s.Code,
                Name        = s.Name,
                Description = s.Description,
                IsBuiltIn   = true,
                IsActive    = true,
            })
            .ToList();
        if (missing.Count > 0)
        {
            db.Privileges.AddRange(missing);
            await db.SaveChangesAsync();
        }

        // ── Admin User ────────────────────────────────────────────────────
        if (!db.Users.Any())
        {
            var adminUser = new ApplicationUser
            {
                Name         = "Admin",
                Email        = "admin@aci-la.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@12345"),
                IsActive     = true,
            };
            db.Users.Add(adminUser);
            await db.SaveChangesAsync();

            // Admin priv 할당 (PrivilegeExpander 가 HrAdmin/LsProjAdmin/JocProjAdmin 등 상하위로 자동 전개)
            var adminPriv = await db.Privileges.FirstAsync(p => p.Code == PrivilegeCodes.Admin);
            db.UserPrivileges.Add(new UserPrivilege
            {
                UserId      = adminUser.Id,
                PrivilegeId = adminPriv.Id,
                GrantedAt   = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        // ── Sample Project ────────────────────────────────────────────────
        if (!db.Projects.Any())
        {
            var project = new Project
            {
                ProjectCode    = "ACI-2025-001",
                Name           = "LA City Hall Renovation",
                SiteAddress    = "200 N Spring St",
                City           = "Los Angeles, CA",
                ZipCode        = "90012",
                Type           = ProjectType.JOC,
                Status         = ProjectStatus.Active,
                ContractAmount = 2500000m,
                OwnerName      = "City of Los Angeles",
                OwnerEmail     = "procurement@lacity.org",
                SchdStartDate  = new DateOnly(2025, 3, 1),
                SchdEndDate    = new DateOnly(2025, 12, 31),
                ActualStartDate = new DateOnly(2025, 3, 15),
            };
            db.Projects.Add(project);
            await db.SaveChangesAsync();

            // Sample Trades
            var trades = new List<Trade>
            {
                new() { ProjectId = project.Id, Name = "General Conditions",  Code = "GC",   Color = "#6c757d" },
                new() { ProjectId = project.Id, Name = "Concrete",            Code = "CONC", Color = "#e67e22", CompanyName = "LA Concrete Inc." },
                new() { ProjectId = project.Id, Name = "Steel / Structural",  Code = "STL",  Color = "#2c3e50", CompanyName = "Pacific Steel" },
                new() { ProjectId = project.Id, Name = "MEP - Mechanical",    Code = "MECH", Color = "#27ae60", CompanyName = "SoCal Mechanical" },
                new() { ProjectId = project.Id, Name = "MEP - Electrical",    Code = "ELEC", Color = "#f1c40f", CompanyName = "Watts Electric" },
                new() { ProjectId = project.Id, Name = "MEP - Plumbing",      Code = "PLMB", Color = "#3498db", CompanyName = "Valley Plumbing" },
                new() { ProjectId = project.Id, Name = "Drywall / Framing",   Code = "DW",   Color = "#9b59b6" },
                new() { ProjectId = project.Id, Name = "Finishes",            Code = "FIN",  Color = "#e74c3c" },
            };
            db.Trades.AddRange(trades);
            await db.SaveChangesAsync();

            // Sample WBS (Baseline Schedule)
            var wbs1 = new ScheduleTask
            {
                ProjectId = project.Id, WbsCode = "1", Text = "Pre-Construction",
                TaskType = GanttTaskType.Project, SortOrder = 1,
                StartDate = new DateOnly(2025, 3, 1), EndDate = new DateOnly(2025, 3, 28),
                Duration = 20, Progress = 1.0
            };
            var wbs2 = new ScheduleTask
            {
                ProjectId = project.Id, WbsCode = "2", Text = "Site Work & Demo",
                TaskType = GanttTaskType.Project, SortOrder = 2,
                StartDate = new DateOnly(2025, 3, 17), EndDate = new DateOnly(2025, 5, 9),
                Duration = 40, Progress = 0.7
            };
            var wbs3 = new ScheduleTask
            {
                ProjectId = project.Id, WbsCode = "3", Text = "Structure",
                TaskType = GanttTaskType.Project, SortOrder = 3,
                StartDate = new DateOnly(2025, 5, 1), EndDate = new DateOnly(2025, 8, 29),
                Duration = 85, Progress = 0.3
            };
            var wbs4 = new ScheduleTask
            {
                ProjectId = project.Id, WbsCode = "4", Text = "MEP Rough-In",
                TaskType = GanttTaskType.Project, SortOrder = 4,
                StartDate = new DateOnly(2025, 7, 1), EndDate = new DateOnly(2025, 10, 31),
                Duration = 90, Progress = 0.1
            };
            var wbs5 = new ScheduleTask
            {
                ProjectId = project.Id, WbsCode = "5", Text = "Finishes",
                TaskType = GanttTaskType.Project, SortOrder = 5,
                StartDate = new DateOnly(2025, 10, 1), EndDate = new DateOnly(2025, 12, 19),
                Duration = 60, Progress = 0
            };
            db.ScheduleTasks.AddRange(wbs1, wbs2, wbs3, wbs4, wbs5);
            await db.SaveChangesAsync();

            // Level-2 child tasks under Structure
            var concrete = trades.First(t => t.Code == "CONC");
            var steel    = trades.First(t => t.Code == "STL");
            var children = new List<ScheduleTask>
            {
                new() {
                    ProjectId = project.Id, ParentId = wbs3.Id, TradeId = concrete.Id,
                    WbsCode = "3.1", Text = "Foundation & Footings",
                    StartDate = new DateOnly(2025, 5, 1), EndDate = new DateOnly(2025, 6, 13),
                    Duration = 30, Progress = 0.8, SortOrder = 1, CrewSize = 8
                },
                new() {
                    ProjectId = project.Id, ParentId = wbs3.Id, TradeId = steel.Id,
                    WbsCode = "3.2", Text = "Steel Erection",
                    StartDate = new DateOnly(2025, 6, 16), EndDate = new DateOnly(2025, 8, 29),
                    Duration = 55, Progress = 0.1, SortOrder = 2, CrewSize = 12
                },
            };
            db.ScheduleTasks.AddRange(children);
            await db.SaveChangesAsync();
        }
    }
}
