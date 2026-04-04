using ACI.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ACI.Web.Data;

/// <summary>
/// Main application DbContext. NOT IdentityDbContext — we use custom BCrypt auth.
/// PostgreSQL with snake_case column naming convention.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ─── Auth ──────────────────────────────────────────────────────────────
    public DbSet<ApplicationUser>      Users                => Set<ApplicationUser>();

    // ─── HR ───────────────────────────────────────────────────────────────
    public DbSet<Employee>             Employees            => Set<Employee>();
    public DbSet<OrgUnit>              OrgUnits             => Set<OrgUnit>();
    public DbSet<JobPosition>          JobPositions         => Set<JobPosition>();
    public DbSet<EmpRole>              EmpRoles             => Set<EmpRole>();
    public DbSet<EmployeeDocument>     EmployeeDocuments    => Set<EmployeeDocument>();

    // ─── External ─────────────────────────────────────────────────────────
    public DbSet<ExternalParty>        ExternalParties      => Set<ExternalParty>();
    public DbSet<ProjectExternalParty> ProjectExternalParties => Set<ProjectExternalParty>();

    // ─── Projects ─────────────────────────────────────────────────────────
    public DbSet<Project>              Projects             => Set<Project>();
    public DbSet<Trade>                Trades               => Set<Trade>();

    // ─── Scheduling ───────────────────────────────────────────────────────
    public DbSet<ScheduleTask>         ScheduleTasks        => Set<ScheduleTask>();
    public DbSet<TaskDependency>       TaskDependencies     => Set<TaskDependency>();

    // ─── Progress Schedule ────────────────────────────────────────────────────
    public DbSet<WorkingTask>          WorkingTasks         => Set<WorkingTask>();
    public DbSet<ScheduleRevision>     ScheduleRevisions    => Set<ScheduleRevision>();
    public DbSet<ScheduleChange>       ScheduleChanges      => Set<ScheduleChange>();
    public DbSet<RevisionDocument>     RevisionDocuments    => Set<RevisionDocument>();

    // ─── Lookahead ────────────────────────────────────────────────────────
    public DbSet<Lookahead>            Lookaheads           => Set<Lookahead>();
    public DbSet<LookaheadTask>        LookaheadTasks       => Set<LookaheadTask>();

    // ─── Weekly Work Plan ─────────────────────────────────────────────────
    public DbSet<WeeklyWorkPlan>       WeeklyWorkPlans      => Set<WeeklyWorkPlan>();
    public DbSet<WeeklyTask>           WeeklyTasks          => Set<WeeklyTask>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── ApplicationUser ───────────────────────────────────────────────
        builder.Entity<ApplicationUser>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Name).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();

            e.HasOne(u => u.Employee)
             .WithOne(emp => emp.UserAccount)
             .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Employee ──────────────────────────────────────────────────────
        builder.Entity<Employee>(e =>
        {
            e.HasKey(emp => emp.Id);
            e.HasIndex(emp => emp.EmpNum).IsUnique();

            e.HasMany(emp => emp.EmpRoles)
             .WithOne(r => r.Employee)
             .HasForeignKey(r => r.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── OrgUnit (self-referencing) ────────────────────────────────────
        builder.Entity<OrgUnit>(e =>
        {
            e.HasKey(o => o.Id);

            e.HasOne(o => o.Parent)
             .WithMany(o => o.Children)
             .HasForeignKey(o => o.ParentId)
             .OnDelete(DeleteBehavior.Restrict);

            // ProjectTeam → Project link
            e.HasOne(o => o.Project)
             .WithMany(p => p.OrgUnits)
             .HasForeignKey(o => o.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(o => o.EmpRoles)
             .WithOne(r => r.OrgUnit)
             .HasForeignKey(r => r.OrgUnitId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── JobPosition ───────────────────────────────────────────────────
        builder.Entity<JobPosition>(e =>
        {
            e.HasKey(p => p.Id);

            e.HasMany(p => p.EmpRoles)
             .WithOne(r => r.JobPosition)
             .HasForeignKey(r => r.JobPositionId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── EmployeeDocument ──────────────────────────────────────────────────
        builder.Entity<EmployeeDocument>(e =>
        {
            e.HasKey(d => d.Id);

            e.HasOne(d => d.Employee)
             .WithMany(emp => emp.Documents)
             .HasForeignKey(d => d.EmployeeId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── EmpRole ───────────────────────────────────────────────────────
        builder.Entity<EmpRole>(e =>
        {
            e.HasKey(r => r.Id);

            // Only one primary role per employee
            e.HasIndex(r => new { r.EmployeeId, r.IsPrimary })
             .HasFilter("\"IsPrimary\" = true")
             .IsUnique();
        });

        // ── ExternalParty ─────────────────────────────────────────────────
        builder.Entity<ExternalParty>(e =>
        {
            e.HasKey(ep => ep.Id);

            e.HasMany(ep => ep.ProjectParticipations)
             .WithOne(pep => pep.ExternalParty)
             .HasForeignKey(pep => pep.ExternalPartyId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── ProjectExternalParty ──────────────────────────────────────────
        builder.Entity<ProjectExternalParty>(e =>
        {
            e.HasKey(pep => pep.Id);

            e.HasOne(pep => pep.Project)
             .WithMany(p => p.ExternalParties)
             .HasForeignKey(pep => pep.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Project ───────────────────────────────────────────────────────
        builder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.ProjectCode).IsRequired().HasMaxLength(30);
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.ContractAmount).HasPrecision(18, 2);
            e.HasIndex(p => p.ProjectCode).IsUnique();

            e.HasMany(p => p.Trades)
             .WithOne(t => t.Project)
             .HasForeignKey(t => t.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(p => p.Tasks)
             .WithOne(t => t.Project)
             .HasForeignKey(t => t.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(p => p.Lookaheads)
             .WithOne(l => l.Project)
             .HasForeignKey(l => l.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(p => p.WeeklyPlans)
             .WithOne(w => w.Project)
             .HasForeignKey(w => w.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Trade ─────────────────────────────────────────────────────────
        builder.Entity<Trade>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.ContractAmount).HasPrecision(18, 2);
        });

        // ── ScheduleTask (self-referencing WBS tree) ──────────────────────
        builder.Entity<ScheduleTask>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Progress).HasPrecision(5, 4);

            e.HasOne(t => t.Parent)
             .WithMany(t => t.Children)
             .HasForeignKey(t => t.ParentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Trade)
             .WithMany(t => t.Tasks)
             .HasForeignKey(t => t.TradeId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.AssignedTo)
             .WithMany()
             .HasForeignKey(t => t.AssignedToId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── TaskDependency ────────────────────────────────────────────────
        builder.Entity<TaskDependency>(e =>
        {
            e.HasKey(d => d.Id);

            e.HasOne(d => d.Source)
             .WithMany(t => t.Successors)
             .HasForeignKey(d => d.SourceId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(d => d.Target)
             .WithMany(t => t.Predecessors)
             .HasForeignKey(d => d.TargetId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Lookahead ─────────────────────────────────────────────────────
        builder.Entity<Lookahead>(e =>
        {
            e.HasKey(l => l.Id);

            e.HasMany(l => l.Tasks)
             .WithOne(t => t.Lookahead)
             .HasForeignKey(t => t.LookaheadId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<LookaheadTask>(e =>
        {
            e.HasKey(t => t.Id);

            e.HasOne(t => t.Trade)
             .WithMany(t => t.LookaheadTasks)
             .HasForeignKey(t => t.TradeId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.ScheduleTask)
             .WithMany()
             .HasForeignKey(t => t.ScheduleTaskId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.AssignedTo)
             .WithMany()
             .HasForeignKey(t => t.AssignedToId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── WorkingTask (Progress Schedule, self-referencing WBS) ────────────
        builder.Entity<WorkingTask>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Progress).HasPrecision(5, 4);

            e.HasOne(t => t.Project)
             .WithMany()
             .HasForeignKey(t => t.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(t => t.BaselineTask)
             .WithMany()
             .HasForeignKey(t => t.BaselineTaskId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.Parent)
             .WithMany(t => t.Children)
             .HasForeignKey(t => t.ParentId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(t => t.Trade)
             .WithMany()
             .HasForeignKey(t => t.TradeId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.AssignedTo)
             .WithMany()
             .HasForeignKey(t => t.AssignedToId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(t => t.ProjectId);
            e.HasIndex(t => t.BaselineTaskId);
        });

        // ── ScheduleRevision ──────────────────────────────────────────────────
        builder.Entity<ScheduleRevision>(e =>
        {
            e.HasKey(r => r.Id);

            e.HasOne(r => r.Project)
             .WithMany()
             .HasForeignKey(r => r.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.SubmittedBy)
             .WithMany()
             .HasForeignKey(r => r.SubmittedById)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasMany(r => r.Changes)
             .WithOne(c => c.Revision)
             .HasForeignKey(c => c.RevisionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(r => r.Documents)
             .WithOne(d => d.Revision)
             .HasForeignKey(d => d.RevisionId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(r => new { r.ProjectId, r.RevisionNumber }).IsUnique();
        });

        // ── ScheduleChange ────────────────────────────────────────────────────
        builder.Entity<ScheduleChange>(e =>
        {
            e.HasKey(c => c.Id);

            e.HasOne(c => c.WorkingTask)
             .WithMany(t => t.Changes)
             .HasForeignKey(c => c.WorkingTaskId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(c => c.ChangedBy)
             .WithMany()
             .HasForeignKey(c => c.ChangedById)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(c => c.RevisionId);
            e.HasIndex(c => c.WorkingTaskId);
        });

        // ── RevisionDocument ──────────────────────────────────────────────────
        builder.Entity<RevisionDocument>(e =>
        {
            e.HasKey(d => d.Id);

            e.HasOne(d => d.UploadedBy)
             .WithMany()
             .HasForeignKey(d => d.UploadedById)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(d => d.RevisionId);
        });

        // ── WeeklyWorkPlan ────────────────────────────────────────────────
        builder.Entity<WeeklyWorkPlan>(e =>
        {
            e.HasKey(w => w.Id);

            // One plan per project per week
            e.HasIndex(w => new { w.ProjectId, w.WeekStartDate }).IsUnique();

            e.HasMany(w => w.Tasks)
             .WithOne(t => t.WeeklyWorkPlan)
             .HasForeignKey(t => t.WeeklyWorkPlanId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<WeeklyTask>(e =>
        {
            e.HasKey(t => t.Id);

            e.HasOne(t => t.LookaheadTask)
             .WithMany(l => l.WeeklyTasks)
             .HasForeignKey(t => t.LookaheadTaskId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.Trade)
             .WithMany(t => t.WeeklyTasks)
             .HasForeignKey(t => t.TradeId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.AssignedTo)
             .WithMany()
             .HasForeignKey(t => t.AssignedToId)
             .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
