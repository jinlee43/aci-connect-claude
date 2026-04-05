using System.Text.Json.Serialization;

namespace ACI.Web.Services;

/// <summary>dhtmlxGantt 전용 데이터 서비스 인터페이스</summary>
public interface IGanttDataService
{
    Task<GanttDataDto> GetProjectDataAsync(int projectId);
    Task<GanttTaskDto> CreateTaskAsync(int projectId, GanttTaskDto task);
    Task<GanttTaskDto> UpdateTaskAsync(int projectId, int taskId, GanttTaskDto task);
    Task DeleteTaskAsync(int taskId);
    Task<int> DeleteTaskSubtreeAsync(int taskId);
    Task<GanttLinkDto> CreateLinkAsync(GanttLinkDto link);
    Task<GanttLinkDto> UpdateLinkAsync(int linkId, GanttLinkDto link);
    Task DeleteLinkAsync(int linkId);
}

// ─── DTOs (dhtmlxGantt JSON 형식과 정확히 매칭) ────────────────────
// dhtmlxGantt는 snake_case를 사용 (start_date, planned_start 등)
// ASP.NET Core 기본값은 camelCase이므로 [JsonPropertyName]으로 명시

public class GanttDataDto
{
    public List<GanttTaskDto> Data { get; set; } = [];
    public List<GanttLinkDto> Links { get; set; } = [];
}

public class GanttTaskDto
{
    public long Id { get; set; }   // long: dhtmlxGantt assigns timestamp-based temp IDs on create
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("start_date")]
    public string StartDate { get; set; } = string.Empty;   // "MM-dd-yyyy HH:mm"

    [JsonPropertyName("end_date")]
    public string? EndDate { get; set; }                     // read-only, computed from start+duration

    public int Duration { get; set; }
    public double Progress { get; set; }
    public int? Parent { get; set; }
    public string Type { get; set; } = "task";              // "task" | "project" | "milestone"
    public bool Open { get; set; } = true;
    public int? TradeId { get; set; }
    public string? Color { get; set; }
    public string? TextColor { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }

    // ── Outbuild-style extra columns ──────────────────────────
    [JsonPropertyName("wbs_code")]
    public string? WbsCode { get; set; }

    [JsonPropertyName("trade_name")]
    public string? TradeName { get; set; }                   // read-only display

    [JsonPropertyName("assigned_to_id")]
    public int? AssignedToId { get; set; }

    [JsonPropertyName("assigned_to_name")]
    public string? AssignedToName { get; set; }              // read-only display

    [JsonPropertyName("crew_size")]
    public int CrewSize { get; set; }

    // Constraint (Procore-style SNET etc.)
    [JsonPropertyName("constraint_type")]
    public string? ConstraintType { get; set; }

    [JsonPropertyName("constraint_date")]
    public string? ConstraintDate { get; set; }

    // 베이스라인 (dhtmlxGantt extension)
    [JsonPropertyName("planned_start")]
    public string? PlannedStart { get; set; }

    [JsonPropertyName("planned_end")]
    public string? PlannedEnd { get; set; }
}

public class GanttLinkDto
{
    public int Id { get; set; }
    public int Source { get; set; }
    public int Target { get; set; }
    public string Type { get; set; } = "0";     // "0"=FS, "1"=SS, "2"=FF, "3"=SF
    public int Lag { get; set; } = 0;
}
