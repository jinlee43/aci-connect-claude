namespace ACI.Web.Data.Entities;

// ─────────────────────────────────────────────────────────────────────────────
// Daily Report — 1일 1프로젝트 1레코드
// 작성: Superintendent (SP/SUPT/SSUPT/ASUPT)
// 검토: ProjectEngineer (PE/APE) 또는 ProjectManager
// 승인: ProjectManager (PM/SPM)
// ─────────────────────────────────────────────────────────────────────────────
public class DailyReport : BaseEntity
{
    public int       ProjectId    { get; set; }
    public int       ReportNumber { get; set; }           // 프로젝트별 순번 (자동)
    public DateOnly  ReportDate   { get; set; }           // 보고 대상 날짜

    public DailyReportStatus Status { get; set; } = DailyReportStatus.Draft;

    // ── 현장 정보 ─────────────────────────────────────────────────────────────
    public string? Location { get; set; }                 // MaxLength: 200

    // ── 기상 조건 ─────────────────────────────────────────────────────────────
    public string? WeatherCondition { get; set; }         // MaxLength: 50  Clear/Partly Cloudy/Cloudy/Rain/Snow/Fog
    public int?    TempHigh         { get; set; }         // °F
    public int?    TempLow          { get; set; }         // °F
    public bool    IsWindy          { get; set; }
    public bool    IsRainy          { get; set; }
    public string? WeatherNotes     { get; set; }         // MaxLength: 300

    // ── 무작업(No Work) ───────────────────────────────────────────────────────
    public bool    IsNoWork         { get; set; }
    public string? NoWorkReason     { get; set; }         // MaxLength: 500

    // ── 일반 메모 ─────────────────────────────────────────────────────────────
    public string? Notes            { get; set; }         // MaxLength: 2000

    // ── 작성 (Superintendent) ─────────────────────────────────────────────────
    public int?      AuthoredById   { get; set; }
    public string?   AuthoredByName { get; set; }         // MaxLength: 150  (비정규화)
    public DateTime? AuthoredAt     { get; set; }

    // ── 제출 ─────────────────────────────────────────────────────────────────
    public DateTime? SubmittedAt    { get; set; }

    // ── 검토 (PE / PM) ────────────────────────────────────────────────────────
    public int?      ReviewedById   { get; set; }
    public string?   ReviewedByName { get; set; }         // MaxLength: 150
    public DateTime? ReviewedAt     { get; set; }
    public string?   ReviewNotes    { get; set; }         // MaxLength: 500

    // ── 승인 (PM / SPM) ───────────────────────────────────────────────────────
    public int?      ApprovedById   { get; set; }
    public string?   ApprovedByName { get; set; }         // MaxLength: 150
    public DateTime? ApprovedAt     { get; set; }
    public string?   ApprovalNotes  { get; set; }         // MaxLength: 500

    // ── 무효화 ────────────────────────────────────────────────────────────────
    public int?      VoidedById     { get; set; }
    public string?   VoidedByName   { get; set; }         // MaxLength: 150
    public DateTime? VoidedAt       { get; set; }
    public string?   VoidReason     { get; set; }         // MaxLength: 500

    // ── Navigation ───────────────────────────────────────────────────────────
    public Project          Project    { get; set; } = null!;
    public ApplicationUser? AuthoredBy { get; set; }
    public ApplicationUser? ReviewedBy { get; set; }
    public ApplicationUser? ApprovedBy { get; set; }
    public ApplicationUser? VoidedBy   { get; set; }

    public ICollection<DailyReportCrewEntry>   CrewEntries  { get; set; } = [];
    public ICollection<DailyReportWorkItem>    WorkItems    { get; set; } = [];
    public ICollection<DailyReportTaskProgress> TaskProgress { get; set; } = [];
    public ICollection<DailyReportEquipment>   Equipment    { get; set; } = [];
    public ICollection<DailyReportFile>        Files        { get; set; } = [];

    // ── Computed ──────────────────────────────────────────────────────────────
    public bool IsLocked =>
        Status is DailyReportStatus.Approved or DailyReportStatus.NoWorkApproved;

    public decimal TotalManHours =>
        WorkItems.Sum(w => w.WorkerHours ?? 0m);
}

// ─────────────────────────────────────────────────────────────────────────────
// 인력 현황 (Manpower / Crew)
// ─────────────────────────────────────────────────────────────────────────────
public class DailyReportCrewEntry : BaseEntity
{
    public int     DailyReportId { get; set; }
    public string? CompanyName   { get; set; }    // MaxLength: 200  (ACI 또는 하도급)
    public int?    TradeId       { get; set; }
    public string? CraftType     { get; set; }    // MaxLength: 100  e.g. Ironworker, Laborer
    public int     WorkerCount   { get; set; }
    public decimal HoursWorked   { get; set; } = 8;
    public int     SortOrder     { get; set; }
    public string? Notes         { get; set; }    // MaxLength: 300

    public DailyReport DailyReport { get; set; } = null!;
    public Trade?      Trade        { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// 작업 내용 (Work Performed)
// ─────────────────────────────────────────────────────────────────────────────
public class DailyReportWorkItem : BaseEntity
{
    public int      DailyReportId { get; set; }
    public string?  TradeText     { get; set; }  // MaxLength: 100  (직접입력 or Trade.Name 복사)
    public int?     TradeId       { get; set; }  // TradeText와 일치하는 Trade FK (선택)
    public string?  Area          { get; set; }  // MaxLength: 200  (작업 구역/대상)
    public string?  CompanyName   { get; set; }  // MaxLength: 200  (ACI 또는 하도급)
    public int?     WorkerCount   { get; set; }  // 투입 인원수
    public TimeOnly? StartTime    { get; set; }  // 작업 시작시각
    public TimeOnly? EndTime      { get; set; }  // 작업 종료시각
    public decimal? WorkerHours   { get; set; }  // 투입 공수 (man-hours) — JS 자동계산 또는 직접입력
    public string   Description   { get; set; } = string.Empty;  // MaxLength: 500
    public int      SortOrder     { get; set; }

    public DailyReport DailyReport { get; set; } = null!;
    public Trade?      Trade        { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Task 진행률 업데이트
// ─────────────────────────────────────────────────────────────────────────────
public class DailyReportTaskProgress : BaseEntity
{
    public int     DailyReportId  { get; set; }
    public int?    WorkingTaskId  { get; set; }   // FK → WorkingTask (null = 수동입력)
    public string? TaskText       { get; set; }   // MaxLength: 300  (비정규화 or 수동)
    public string? WbsCode        { get; set; }   // MaxLength: 30
    public string? Location       { get; set; }   // MaxLength: 200
    public double  ProgressBefore { get; set; }   // 0.0–1.0
    public double  ProgressAfter  { get; set; }   // 0.0–1.0
    public int     SortOrder      { get; set; }
    public string? Notes          { get; set; }   // MaxLength: 500

    public DailyReport  DailyReport  { get; set; } = null!;
    public WorkingTask? WorkingTask  { get; set; }

    // Computed
    public int ProgressBeforePercent => (int)Math.Round(ProgressBefore * 100);
    public int ProgressAfterPercent  => (int)Math.Round(ProgressAfter  * 100);
    public int Delta                 => ProgressAfterPercent - ProgressBeforePercent;
}

// ─────────────────────────────────────────────────────────────────────────────
// 장비 사용 (Equipment Log)
// ─────────────────────────────────────────────────────────────────────────────
public class DailyReportEquipment : BaseEntity
{
    public int     DailyReportId { get; set; }
    public string  Name          { get; set; } = string.Empty;  // MaxLength: 200
    public string? EquipmentTag  { get; set; }  // MaxLength: 50  (장비번호/태그)
    public decimal HoursUsed     { get; set; } = 8;
    public int     SortOrder     { get; set; }
    public string? Notes         { get; set; }  // MaxLength: 300

    public DailyReport DailyReport { get; set; } = null!;
}

// ─────────────────────────────────────────────────────────────────────────────
// 첨부 파일 (Photos & Documents)
// ─────────────────────────────────────────────────────────────────────────────
public class DailyReportFile : BaseEntity
{
    public int                  DailyReportId  { get; set; }
    public DailyReportFileType  FileType       { get; set; } = DailyReportFileType.Document;
    public string               FileName       { get; set; } = string.Empty;  // MaxLength: 300
    public string               StoredFileName { get; set; } = string.Empty;  // MaxLength: 100
    public string?              Extension      { get; set; }  // MaxLength: 20  lowercase, no dot
    public long                 FileSize       { get; set; }
    public string?              Caption        { get; set; }  // MaxLength: 300
    public int?                 UploadedById   { get; set; }
    public string?              UploadedByName { get; set; }  // MaxLength: 150
    public DateTime?            UploadedAt     { get; set; }

    public DailyReport      DailyReport { get; set; } = null!;
    public ApplicationUser? UploadedBy  { get; set; }

    // Computed
    public bool IsPreviewable =>
        Extension is "pdf" or "jpg" or "jpeg" or "png" or "gif" or "webp";

    public string FileSizeDisplay => FileSize switch
    {
        < 1024           => $"{FileSize} B",
        < 1024 * 1024    => $"{FileSize / 1024.0:F1} KB",
        _                => $"{FileSize / (1024.0 * 1024):F1} MB",
    };
}

// ─────────────────────────────────────────────────────────────────────────────
// Enums
// ─────────────────────────────────────────────────────────────────────────────
public enum DailyReportStatus
{
    Draft          = 0,   // 작성 중 (Superintendent)
    Submitted      = 1,   // 검토 요청
    Reviewed       = 2,   // 검토 완료 (PE / PM)
    Approved       = 3,   // 승인 완료 (PM / SPM) — 잠금
    Voided         = 4,   // 무효화
    NoWork         = 5,   // 무작업 마킹
    NoWorkApproved = 6,   // 무작업 승인 — 잠금
}

public enum DailyReportFileType
{
    Document = 0,
    Photo    = 1,
}

// ─────────────────────────────────────────────────────────────────────────────
// Weather Cache — (ProjectId + Date) 키로 날씨 정보 캐싱
// 신규 Daily Report 작성 시 DB에서 먼저 조회, 없으면 Open-Meteo 호출 후 저장
// ─────────────────────────────────────────────────────────────────────────────
public class WeatherCache : BaseEntity
{
    public int      ProjectId        { get; set; }
    public DateOnly Date             { get; set; }

    // ── 위치 ─────────────────────────────────────────────────────────────────
    public double?  Latitude         { get; set; }
    public double?  Longitude        { get; set; }
    public string?  Address          { get; set; }   // MaxLength: 500  (geocoding에 사용된 주소)

    // ── 날씨 ─────────────────────────────────────────────────────────────────
    public string?  Condition        { get; set; }   // MaxLength: 50   Clear/Partly Cloudy/…
    public int?     TempHigh         { get; set; }   // °F
    public int?     TempLow          { get; set; }   // °F
    public bool     IsWindy          { get; set; }
    public bool     IsRainy          { get; set; }

    // ── 메타 ─────────────────────────────────────────────────────────────────
    public DateTime FetchedAt        { get; set; }   // UTC, Open-Meteo 조회 시각
    public string?  Source           { get; set; }   // MaxLength: 50   "open-meteo" 등

    // ── Navigation ───────────────────────────────────────────────────────────
    public Project  Project          { get; set; } = null!;
}
