namespace ACI.Web.Data.Entities;

/// <summary>
/// Gantt dependency link between two ScheduleTasks.
/// DB 저장값: 0=FS, 1=SS, 2=FF, 3=SF
/// SVAR Gantt 표기: "e2s"=FS, "s2s"=SS, "e2e"=FF, "s2e"=SF
/// </summary>
public class TaskDependency
{
    public int Id { get; set; }

    /// <summary>Predecessor (source) task.</summary>
    public int SourceId { get; set; }
    public ScheduleTask Source { get; set; } = null!;

    /// <summary>Successor (target) task.</summary>
    public int TargetId { get; set; }
    public ScheduleTask Target { get; set; } = null!;

    public DependencyType Type { get; set; } = DependencyType.FinishToStart;

    /// <summary>Lag in calendar days. Negative = overlap (lead).</summary>
    public int Lag { get; set; } = 0;
}

/// <summary>
/// FS = Finish-to-Start (most common)
/// SS = Start-to-Start
/// FF = Finish-to-Finish
/// SF = Start-to-Finish
/// </summary>
public enum DependencyType
{
    FinishToStart  = 0,
    StartToStart   = 1,
    FinishToFinish = 2,
    StartToFinish  = 3
}
