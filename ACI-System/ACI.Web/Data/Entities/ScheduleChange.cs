using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

public enum ChangeType
{
    Added           = 0,   // 신규 공정 추가
    Removed         = 1,   // 공정 삭제
    Suspended       = 2,   // 공정 보류
    Resumed         = 3,   // 보류 공정 재개
    StartShifted    = 4,   // 착공일 변경
    FinishShifted   = 5,   // 완공일 변경
    DatesShifted    = 6,   // 착공·완공일 모두 변경
    DurationChanged = 7,   // 공기 변경
    ProgressUpdated = 8,   // 진행률 업데이트
    ScopeChanged    = 9,   // 공종명/내용 변경
    TradeChanged    = 10,  // 담당 Trade 변경
}

/// <summary>
/// Individual task-level change record within a ScheduleRevision.
/// Captures the "before → after" snapshot for each modified WorkingTask.
/// ChangedById is auto-populated from the current authenticated user.
/// </summary>
public class ScheduleChange
{
    public int Id { get; set; }

    public int RevisionId { get; set; }
    public ScheduleRevision Revision { get; set; } = null!;

    public int WorkingTaskId { get; set; }
    public WorkingTask WorkingTask { get; set; } = null!;

    public ChangeType ChangeType { get; set; }

    // ── Before (Baseline or previous state) ──────────────────────────────────
    public DateOnly? OldStartDate { get; set; }
    public DateOnly? OldEndDate   { get; set; }
    public int?      OldDuration  { get; set; }
    public double?   OldProgress  { get; set; }

    [MaxLength(300)]
    public string? OldText { get; set; }

    // ── After (new state) ─────────────────────────────────────────────────────
    public DateOnly? NewStartDate { get; set; }
    public DateOnly? NewEndDate   { get; set; }
    public int?      NewDuration  { get; set; }
    public double?   NewProgress  { get; set; }

    [MaxLength(300)]
    public string? NewText { get; set; }

    // ── Impact ────────────────────────────────────────────────────────────────
    /// <summary>
    /// Calendar days of finish date shift. Positive = delayed, Negative = accelerated.
    /// </summary>
    public int? DaysShifted { get; set; }

    [MaxLength(1000)]
    public string? ChangeNote { get; set; }

    // ── Audit ────────────────────────────────────────────────────────────────
    /// <summary>User who made this change (auto-captured from session).</summary>
    public int? ChangedById { get; set; }
    public ApplicationUser? ChangedBy { get; set; }

    [MaxLength(150)]
    public string ChangedByName { get; set; } = string.Empty;   // denormalized for display

    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
}
