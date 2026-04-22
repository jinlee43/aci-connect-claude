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
    public DbSet<Privilege>            Privileges           => Set<Privilege>();
    public DbSet<UserPrivilege>        UserPrivileges       => Set<UserPrivilege>();

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

    // ─── Baselines ───────────────────────────────────────────────────────────
    public DbSet<ScheduleBaseline>     ScheduleBaselines    => Set<ScheduleBaseline>();
    public DbSet<BaselineTaskSnapshot> BaselineTaskSnapshots => Set<BaselineTaskSnapshot>();

    // ─── Current Plan (Current Schedule) ────────────────────────────────────
    public DbSet<WorkingTask>          WorkingTasks         => Set<WorkingTask>();
    public DbSet<ScheduleRevision>     ScheduleRevisions    => Set<ScheduleRevision>();
    public DbSet<ScheduleChange>       ScheduleChanges      => Set<ScheduleChange>();
    public DbSet<RevisionDocument>     RevisionDocuments    => Set<RevisionDocument>();

    // ─── Simulations (What-If) ───────────────────────────────────────────────
    public DbSet<ScheduleSimulation>   ScheduleSimulations  => Set<ScheduleSimulation>();
    public DbSet<SimulationTask>       SimulationTasks      => Set<SimulationTask>();

    // ─── Lookahead ────────────────────────────────────────────────────────
    public DbSet<Lookahead>            Lookaheads           => Set<Lookahead>();
    public DbSet<LookaheadTask>        LookaheadTasks       => Set<LookaheadTask>();

    // ─── Weekly Work Plan ─────────────────────────────────────────────────
    public DbSet<WeeklyWorkPlan>       WeeklyWorkPlans      => Set<WeeklyWorkPlan>();
    public DbSet<WeeklyTask>           WeeklyTasks          => Set<WeeklyTask>();

    // ─── Safety Weekly Report ─────────────────────────────────────────────
    public DbSet<SafetyWkRepSettings>  SafetyWkRepSettings  => Set<SafetyWkRepSettings>();
    public DbSet<SafetyWkRep>          SafetyWkReps         => Set<SafetyWkRep>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // ── ApplicationUser ───────────────────────────────────────────────
        builder.Entity<ApplicationUser>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Name).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Name).IsUnique();   // 로그인 키 — 중복 Name 방지
            e.HasIndex(u => u.Email).IsUnique();

            e.HasOne(u => u.Employee)
             .WithOne(emp => emp.UserAccount)
             .HasForeignKey<ApplicationUser>(u => u.EmployeeId)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── Privilege ─────────────────────────────────────────────────────
        builder.Entity<Privilege>(e =>
        {
            e.HasKey(p => p.Id);
            e.HasIndex(p => p.Code).IsUnique();
        });

        // ── UserPrivilege (many-to-many join) ─────────────────────────────
        builder.Entity<UserPrivilege>(e =>
        {
            e.HasKey(up => new { up.UserId, up.PrivilegeId });

            e.HasOne(up => up.User)
             .WithMany(u => u.UserPrivileges)
             .HasForeignKey(up => up.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(up => up.Privilege)
             .WithMany(p => p.UserPrivileges)
             .HasForeignKey(up => up.PrivilegeId)
             .OnDelete(DeleteBehavior.Cascade);

            // GrantedBy 는 감사용이며 사용자가 삭제되어도 기록은 유지 (SetNull)
            e.HasOne(up => up.GrantedBy)
             .WithMany()
             .HasForeignKey(up => up.GrantedByUserId)
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
            // Progress: DB는 double precision — numeric(5,4) 적용 안 함

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

        // ── WorkingTask (Current Schedule, self-referencing WBS) ────────────
        builder.Entity<WorkingTask>(e =>
        {
            e.HasKey(t => t.Id);
            // Progress: DB는 double precision — numeric(5,4) 적용 안 함

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

        // ── ScheduleBaseline ──────────────────────────────────────────────────
        builder.Entity<ScheduleBaseline>(e =>
        {
            e.HasKey(b => b.Id);

            e.HasOne(b => b.Project)
             .WithMany(p => p.Baselines)
             .HasForeignKey(b => b.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(b => b.FrozenBy)
             .WithMany()
             .HasForeignKey(b => b.FrozenById)
             .OnDelete(DeleteBehavior.SetNull);

            // Auto snapshot 을 생성시킨 Simulation (nullable, SET NULL on delete)
            e.HasOne(b => b.SourceSimulation)
             .WithMany()
             .HasForeignKey(b => b.SourceSimulationId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasMany(b => b.TaskSnapshots)
             .WithOne(s => s.Baseline)
             .HasForeignKey(s => s.BaselineId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(b => new { b.ProjectId, b.VersionNumber }).IsUnique();
        });

        // ── BaselineTaskSnapshot ─────────────────────────────────────────────
        builder.Entity<BaselineTaskSnapshot>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Progress).HasPrecision(5, 4);

            e.HasOne(s => s.SourceScheduleTask)
             .WithMany()
             .HasForeignKey(s => s.SourceScheduleTaskId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(s => s.SourceWorkingTask)
             .WithMany()
             .HasForeignKey(s => s.SourceWorkingTaskId)
             .OnDelete(DeleteBehavior.SetNull);

            // Self-referencing hierarchy within a baseline snapshot
            e.HasOne(s => s.ParentSnapshot)
             .WithMany(s => s.ChildSnapshots)
             .HasForeignKey(s => s.ParentSnapshotId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(s => s.BaselineId);
        });

        // ── ScheduleRevision ──────────────────────────────────────────────────
        builder.Entity<ScheduleRevision>(e =>
        {
            e.HasKey(r => r.Id);

            e.HasOne(r => r.Project)
             .WithMany()
             .HasForeignKey(r => r.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.Baseline)
             .WithMany()
             .HasForeignKey(r => r.BaselineId)
             .OnDelete(DeleteBehavior.SetNull);

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

        // ── ScheduleSimulation ────────────────────────────────────────────────
        builder.Entity<ScheduleSimulation>(e =>
        {
            e.HasKey(s => s.Id);

            e.HasOne(s => s.Project)
             .WithMany(p => p.Simulations)
             .HasForeignKey(s => s.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.SourceBaseline)
             .WithMany()
             .HasForeignKey(s => s.SourceBaselineId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(s => s.CreatedByUser)
             .WithMany()
             .HasForeignKey(s => s.CreatedByUserId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasMany(s => s.Tasks)
             .WithOne(t => t.Simulation)
             .HasForeignKey(t => t.SimulationId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(s => s.ProjectId);
        });

        // ── SimulationTask ───────────────────────────────────────────────────
        builder.Entity<SimulationTask>(e =>
        {
            e.HasKey(t => t.Id);
            // Progress is double? (nullable) — no precision needed for double

            e.HasOne(t => t.SourceWorkingTask)
             .WithMany()
             .HasForeignKey(t => t.SourceWorkingTaskId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(t => t.SourceSnapshot)
             .WithMany()
             .HasForeignKey(t => t.SourceSnapshotId)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(t => t.SimulationId);
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

        // ── SafetyWkRepSettings ───────────────────────────────────────────────
        builder.Entity<SafetyWkRepSettings>(e =>
        {
            e.HasKey(s => s.Id);

            // One settings record per project
            e.HasIndex(s => s.ProjectId).IsUnique();

            e.HasOne(s => s.Project)
             .WithMany()
             .HasForeignKey(s => s.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(s => s.ApprovedBy)
             .WithMany()
             .HasForeignKey(s => s.ApprovedById)
             .OnDelete(DeleteBehavior.SetNull);
        });

        // ── SafetyWkRep ───────────────────────────────────────────────────────
        builder.Entity<SafetyWkRep>(e =>
        {
            e.HasKey(r => r.Id);

            // One report per project per week
            e.HasIndex(r => new { r.ProjectId, r.WeekStartDate }).IsUnique();

            e.HasOne(r => r.Project)
             .WithMany()
             .HasForeignKey(r => r.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(r => r.UploadedBy)
             .WithMany()
             .HasForeignKey(r => r.UploadedById)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(r => r.ReviewedBy)
             .WithMany()
             .HasForeignKey(r => r.ReviewedById)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(r => r.ApprovedBy)
             .WithMany()
             .HasForeignKey(r => r.ApprovedById)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(r => r.VoidedBy)
             .WithMany()
             .HasForeignKey(r => r.VoidedById)
             .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(r => r.ProjectId);
            e.HasIndex(r => new { r.Year, r.WeekNumber });
        });
    }
}
