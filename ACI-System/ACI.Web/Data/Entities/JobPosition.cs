using System.ComponentModel.DataAnnotations;

namespace ACI.Web.Data.Entities;

/// <summary>
/// Job title / position definition.
/// e.g., CEO, Senior VP, VP, Senior PM, PM, Assistant PM, PE, Assistant PE,
///       Senior Superintendent, Superintendent, Assistant Superintendent, etc.
/// Used for both company org titles (Employee's home position) and
/// project roles (EmpRole.JobPositionId).
/// </summary>
public class JobPosition : BaseEntity
{
    [Required, MaxLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Display ordering — lower = higher rank (CEO = 1, Asst. Super = 999).</summary>
    public int OrdNum { get; set; } = 999;

    /// <summary>
    /// 포지션 분류 태그. 예: "Project" — Project Staffing 화면 드롭다운 필터 기준.
    /// null 이면 분류 없음 (회사 조직용 직책 등).
    /// </summary>
    [MaxLength(50)]
    public string? Type { get; set; }

    public ICollection<EmpRole> EmpRoles { get; set; } = [];
}
