using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

/// <summary>
/// Trade / Subcontractor on a project.
/// Each project has its own set of trades (project-scoped).
/// </summary>
public class Trade : BaseEntity
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;   // e.g. "Concrete", "Steel Frame"

    [MaxLength(10)]
    public string Code { get; set; } = string.Empty;   // e.g. "CONC", "STL"

    /// <summary>Hex color used in Gantt / Lookahead bars.</summary>
    [MaxLength(10)]
    public string Color { get; set; } = "#4A90D9";

    // Subcontractor company info
    [MaxLength(200)]
    public string? CompanyName { get; set; }

    [MaxLength(100)]
    public string? ContactName { get; set; }

    [MaxLength(200)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    public decimal? ContractAmount { get; set; }

    // Navigation
    public ICollection<ScheduleTask>   Tasks         { get; set; } = [];
    public ICollection<LookaheadTask>  LookaheadTasks { get; set; } = [];
    public ICollection<WeeklyTask>     WeeklyTasks    { get; set; } = [];
}
