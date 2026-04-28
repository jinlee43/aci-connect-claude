using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ACI.Web.Data.Entities;

public enum ProjectType
{
    LumpSum  = 0,   // 일식계약
    JOC      = 1,   // Job Order Contract
    GMP      = 2,   // Guaranteed Maximum Price
    CostPlus = 3    // Cost Plus
}

public enum ProjectStatus
{
    Planning  = 0,  // 계획중
    Active    = 1,  // 진행중
    OnHold    = 2,  // 보류
    Completed = 3,  // 완료
    Cancelled = 4   // 취소
}

/// <summary>
/// Construction project (Lump Sum or JOC).
/// </summary>
public class Project : BaseEntity
{
    /// <summary>Human-readable code, e.g. "ACI-2025-001".</summary>
    [Required, MaxLength(30)]
    public string ProjectCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public ProjectType Type   { get; set; } = ProjectType.LumpSum;
    public ProjectStatus Status { get; set; } = ProjectStatus.Planning;

    // Site location
    [MaxLength(300)]
    public string? SiteAddress { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(10)]
    public string? ZipCode { get; set; }

    [MaxLength(50)]
    public string? State { get; set; }

    /// <summary>GPS 위도 — Nominatim 지오코딩으로 한 번만 저장, 날씨 조회에 사용.</summary>
    public double? Latitude  { get; set; }
    /// <summary>GPS 경도.</summary>
    public double? Longitude { get; set; }

    // Owner / Customer
    [MaxLength(200)]
    public string? OwnerName { get; set; }

    [MaxLength(200)]
    public string? OwnerContact { get; set; }

    [MaxLength(200)]
    public string? OwnerEmail { get; set; }

    [MaxLength(300)]
    public string? OwnerAddress { get; set; }

    [MaxLength(100)]
    public string? OwnerCity { get; set; }

    [MaxLength(10)]
    public string? OwnerZipCode { get; set; }

    [MaxLength(50)]
    public string? OwnerState { get; set; }

    // Contract
    public decimal ContractAmount { get; set; } = 0;

    // Schedule dates
    public DateOnly? SchdStartDate { get; set; }
    public DateOnly? SchdEndDate   { get; set; }
    public DateOnly? ActualStartDate { get; set; }
    public DateOnly? ActualEndDate   { get; set; }
    public DateOnly? CompletedDate   { get; set; }

    // Weekly Report tracking (safety / daily log start dates)
    public DateOnly? WeeklyReportStartDate  { get; set; }
    public DateOnly? InspectReportStartDate { get; set; }

    // Navigation
    public ICollection<OrgUnit>              OrgUnits          { get; set; } = [];
    public ICollection<ProjectExternalParty> ExternalParties   { get; set; } = [];
    public ICollection<Trade>                Trades            { get; set; } = [];
    public ICollection<ScheduleTask>   Tasks         { get; set; } = [];
    public ICollection<ScheduleBaseline> Baselines   { get; set; } = [];
    public ICollection<ScheduleSimulation> Simulations { get; set; } = [];
    public ICollection<Lookahead>      Lookaheads    { get; set; } = [];
    public ICollection<WeeklyWorkPlan> WeeklyPlans   { get; set; } = [];

    // Computed helpers (not mapped)
    [NotMapped]
    public string TypeLabel => Type switch
    {
        ProjectType.LumpSum  => "Lump Sum",
        ProjectType.JOC      => "JOC",
        ProjectType.GMP      => "GMP",
        ProjectType.CostPlus => "Cost Plus",
        _                    => Type.ToString()
    };

    [NotMapped]
    public string StatusLabel => Status switch
    {
        ProjectStatus.Planning  => "Planning",
        ProjectStatus.Active    => "Active",
        ProjectStatus.OnHold    => "On Hold",
        ProjectStatus.Completed => "Completed",
        ProjectStatus.Cancelled => "Cancelled",
        _                       => Status.ToString()
    };

    [NotMapped]
    public string StatusCssClass => Status switch
    {
        ProjectStatus.Planning  => "badge-planning",
        ProjectStatus.Active    => "badge-active",
        ProjectStatus.OnHold    => "badge-on-hold",
        ProjectStatus.Completed => "badge-completed",
        ProjectStatus.Cancelled => "badge-cancelled",
        _                       => ""
    };
}
