# SafetyWkRep — Weekly Safety Report Feature Design

## 1. 개요

매주 Superintendent 또는 Safety 담당자가 지정 요일에 안전 보고서를 업로드하고,  
PM → SafetyManager 순으로 검토/승인하는 워크플로.  
승인 후에는 잠금 상태가 되며, SafetyManager/SafetyAdmin만 승인 취소(Revision) 가능.

---

## 2. 신규 Privilege

기존 `SafetyOfficer` 를 3단 계층으로 분리:

```
SafetyAdmin ⊇ SafetyManager ⊇ SafetyUser
```

| 코드 | 역할 |
|------|------|
| `SafetyAdmin` | 전체 관리, 승인 취소 가능 |
| `SafetyManager` | 승인/편집, 승인 취소 가능 |
| `SafetyUser` | 모든 프로젝트 업로드/편집 가능 |

> **기존 SafetyOfficer 처리**: `SafetyUser` 로 이름 변경하거나 alias 처리.  
> Admin은 PrivilegeExpander에서 SafetyAdmin을 implies 하도록 추가.

**PrivilegeCodes.cs 추가 상수:**
```csharp
public const string SafetyAdmin   = "SafetyAdmin";
public const string SafetyManager = "SafetyManager";
public const string SafetyUser    = "SafetyUser";
```

**PrivilegeExpander.cs DirectImplies 추가:**
```csharp
[PrivilegeCodes.Admin]        = new[] { ..., PrivilegeCodes.SafetyAdmin },
[PrivilegeCodes.SafetyAdmin]  = new[] { PrivilegeCodes.SafetyManager },
[PrivilegeCodes.SafetyManager]= new[] { PrivilegeCodes.SafetyUser },
```

---

## 3. 프로젝트-사용자 접근 범위

PM / Superintendent 는 **담당 프로젝트**의 보고서만 접근 가능.

**담당 프로젝트 판별 체인 (확인됨):**
```
ApplicationUser.EmployeeId
  → Employee.EmpRoles
      → EmpRole.OrgUnit (OrgUnit.ProjectId != null)
          → Project
```

서비스 레이어 쿼리 패턴:
```csharp
var assignedProjectIds = await _db.ApplicationUsers
    .Where(u => u.Id == currentUserId)
    .SelectMany(u => u.Employee!.EmpRoles)
    .Where(r => r.OrgUnit.ProjectId != null && r.IsActive)
    .Select(r => r.OrgUnit.ProjectId!.Value)
    .Distinct()
    .ToListAsync();
```

SafetyAdmin/SafetyManager/SafetyUser 는 **전체 프로젝트** 접근 가능.

---

## 4. 신규 엔티티

### 4-1. SafetyWkRepSettings (프로젝트별 안전보고서 설정)

```csharp
public class SafetyWkRepSettings : BaseEntity
{
    public int       ProjectId            { get; set; }
    public Project   Project              { get; set; } = null!;

    /// <summary>안전보고서 제출 시작일 (첫 주 기준일).</summary>
    public DateOnly  StartDate            { get; set; }

    /// <summary>안전보고서 제출 종료일 (프로젝트 완료 등). null = 무기한.</summary>
    public DateOnly? EndDate              { get; set; }

    /// <summary>주로 제출하는 요일 (참고용, 강제 아님). 0=Sunday…6=Saturday.</summary>
    public DayOfWeek DefaultSubmitDay     { get; set; } = DayOfWeek.Friday;

    // ── 승인 상태 (SafetyManager 승인 후 편집 잠금) ──────────────────────────
    public bool      IsApproved           { get; set; } = false;
    public DateTime? ApprovedAt           { get; set; }
    public int?      ApprovedById         { get; set; }
    public ApplicationUser? ApprovedBy   { get; set; }

    [MaxLength(150)]
    public string?   ApprovedByName       { get; set; }

    // ── Revision 추적 (승인취소 → 수정 → 재승인 이력) ──────────────────────
    public int       RevisionNumber       { get; set; } = 0;

    [MaxLength(500)]
    public string?   Notes                { get; set; }
}
```

> **승인 잠금 규칙**: `IsApproved == true` 이면 PM/Superintendent 편집 불가.  
> SafetyManager/SafetyAdmin 이 승인 취소(RevisionNumber++) 시 다시 편집 가능.

---

### 4-2. SafetyWkRep (주간 안전보고서 본체)

```csharp
public enum SafetyWkRepStatus
{
    Draft     = 0,  // 업로드됨, 미검토
    Reviewed  = 1,  // PM 검토 완료
    Approved  = 2,  // SafetyManager 승인 (잠금)
    NoWork    = 3,  // 해당 주 무작업 (파일 없음)
    Voided    = 4,  // SafetyManager/SafetyAdmin 이 승인 취소
}

public class SafetyWkRep : BaseEntity
{
    public int     ProjectId    { get; set; }
    public Project Project      { get; set; } = null!;

    // ── 주(Week) 식별 ─────────────────────────────────────────────────────
    /// <summary>해당 주의 월요일 날짜 (주 식별 키).</summary>
    public DateOnly WeekStartDate { get; set; }
    public DateOnly WeekEndDate   { get; set; }   // WeekStartDate + 6
    public int      WeekNumber    { get; set; }
    public int      Year          { get; set; }

    public SafetyWkRepStatus Status { get; set; } = SafetyWkRepStatus.Draft;

    // ── 파일 정보 ─────────────────────────────────────────────────────────
    [MaxLength(260)]
    public string?  FileName       { get; set; }   // 원본 파일명

    [MaxLength(100)]
    public string?  StoredFileName { get; set; }   // 서버 저장 파일명 (GUID 기반)

    [MaxLength(20)]
    public string?  Extension      { get; set; }

    public long     FileSize       { get; set; }

    // ── 업로드 정보 ───────────────────────────────────────────────────────
    public int?     UploadedById   { get; set; }
    public ApplicationUser? UploadedBy { get; set; }

    [MaxLength(150)]
    public string?  UploadedByName { get; set; }
    public DateTime? UploadedAt   { get; set; }

    // ── 검토 정보 (PM) ────────────────────────────────────────────────────
    public int?     ReviewedById   { get; set; }
    public ApplicationUser? ReviewedBy { get; set; }

    [MaxLength(150)]
    public string?  ReviewedByName { get; set; }
    public DateTime? ReviewedAt   { get; set; }

    [MaxLength(500)]
    public string?  ReviewNotes    { get; set; }

    // ── 승인 정보 (SafetyManager) ─────────────────────────────────────────
    public int?     ApprovedById   { get; set; }
    public ApplicationUser? ApprovedBy { get; set; }

    [MaxLength(150)]
    public string?  ApprovedByName { get; set; }
    public DateTime? ApprovedAt   { get; set; }

    [MaxLength(500)]
    public string?  ApprovalNotes  { get; set; }

    // ── 승인 취소 정보 (Voided) ───────────────────────────────────────────
    public int?     VoidedById     { get; set; }

    [MaxLength(150)]
    public string?  VoidedByName   { get; set; }
    public DateTime? VoidedAt     { get; set; }

    [MaxLength(500)]
    public string?  VoidReason     { get; set; }

    [MaxLength(500)]
    public string?  Notes          { get; set; }
}
```

**DB Unique Constraint**: `(ProjectId, WeekStartDate)` — 프로젝트당 주에 한 건만 존재.

---

## 5. 권한 매트릭스

| 작업 | SafetyAdmin | SafetyManager | SafetyUser | PM | Superintendent |
|------|:-----------:|:-------------:|:----------:|:--:|:--------------:|
| Upload (any project) | ✅ | ✅ | ✅ | ❌ | ❌ |
| Upload (담당 프로젝트) | ✅ | ✅ | ✅ | ❌ | ✅ |
| Edit Draft | ✅ | ✅ | ✅ | ❌ | ✅ (담당) |
| Delete Draft | ✅ | ✅ | ✅ | ❌ | ✅ (담당) |
| Mark NoWork | ✅ | ✅ | ✅ | ❌ | ✅ (담당) |
| Review (검토) | ✅ | ✅ | ❌ | ✅ (담당) | ❌ |
| Approve (승인) | ✅ | ✅ | ❌ | ❌ | ❌ |
| Void Approval (취소) | ✅ | ✅ | ❌ | ❌ | ❌ |
| Edit Approved | ❌ | ❌ | ❌ | ❌ | ❌ |
| Edit Settings | ✅ | ✅ | ❌ | ✅ (담당) | ✅ (담당) |
| Approve Settings | ✅ | ✅ | ❌ | ❌ | ❌ |
| Void Settings Approval | ✅ | ✅ | ❌ | ❌ | ❌ |
| View (all projects) | ✅ | ✅ | ✅ | ❌ | ❌ |
| View (담당 프로젝트만) | ✅ | ✅ | ✅ | ✅ | ✅ |

---

## 6. 상태 워크플로

### SafetyWkRep 상태 전이
```
[없음]
  └─ Upload / NoWork
       └─ Draft ──────────────┐
            │                 │ Delete (Draft만 가능)
            │ Review (PM)     │
            ▼                 │
         Reviewed ◄───────────┘ (Review 취소: 다시 Draft)
            │
            │ Approve (SafetyManager)
            ▼
         Approved ─── Void (SafetyManager/SafetyAdmin) ──► Voided
                                                              │
                                                              │ Re-upload
                                                              ▼
                                                            Draft
```

### SafetyWkRepSettings 상태 전이
```
미승인(IsApproved=false) ──► Approve(SafetyManager) ──► 승인됨(IsApproved=true)
                                                              │
                                                              │ Void(SafetyManager/SafetyAdmin)
                                                              ▼
                                                         미승인(RevisionNumber++)
```

---

## 7. UI 구성

### 7-1. Safety Report Settings 페이지
- **경로**: `/Safety/Settings/{projectId}` 또는 `/Projects/{projectId}/SafetySettings`
- **접근**: PM, Superintendent (담당 프로젝트), SafetyManager, SafetyAdmin
- **기능**: StartDate, EndDate, DefaultSubmitDay 편집 / 승인 / 승인취소
- **잠금**: `Settings.IsApproved == true` 이면 편집 UI 비활성화, 승인취소 버튼만 표시

### 7-2. PM/Superintendent용 목록 페이지
- **경로**: `/Safety/MyReports?projectId={id}`
- **레이아웃**: 주(Week) 단위 행 목록. 컬럼: Week, Status, FileName, UploadedBy, ReviewedBy
- **기능 (행별)**: Upload, Edit, Delete, NoWork, View 파일

### 7-3. Safety Staff용 전체 그리드 페이지
- **경로**: `/Safety/Reports`
- **레이아웃**: 행=프로젝트, 열=Week. 각 셀에 상태 뱃지.
- **기능**: 셀 클릭 → 상세 패널 (Upload/Edit/Delete/Review/Approve/Void)
- **필터**: 연도/주차 범위, 프로젝트, 상태

---

## 8. 파일 저장 구조

`EmployeeDocumentController` 패턴과 동일하게 `ContentRootPath/uploads/safety/{projectId}/{weekStartDate}/{guid.ext}` 형태 사용.

---

## 9. 구현 순서 (권장)

1. **PrivilegeCodes + PrivilegeExpander** — SafetyAdmin/SafetyManager/SafetyUser 추가
2. **Entity**: `SafetyWkRepSettings`, `SafetyWkRep`
3. **AppDbContext** — EntityTypeConfiguration 추가
4. **Migration** — `dotnet ef migrations add AddSafetyWkRep`
5. **DbInitializer** — 신규 Privilege 3개 Seed
6. **Program.cs** — 정책 등록 (`SafetyAdmin`, `SafetyManager`, `SafetyUser`, `SafetyStaff`)
7. **SafetyWkRepService** — Upload, Review, Approve, Void, GetWeeklyGrid
8. **Pages/Safety/Settings** — 설정 CRUD + 승인 워크플로
9. **Pages/Safety/MyReports** — PM/Superintendent 뷰
10. **Pages/Safety/Reports** — Safety Staff 전체 그리드 뷰
11. **SafetyReportController** — 파일 서빙 (다운로드/미리보기)

---

## 10. 확정 사항 및 참고

- **SafetyOfficer → SafetyUser rename** ✅ (확정)
- **User↔Employee 체인** ✅ `ApplicationUser.EmployeeId` 존재 확인됨
- **Revision 개념**: `RevisionNumber++` + `IsApproved=false` 로 단순화 (이력 테이블 없음)
- **Project.WeeklyReportStartDate**: SafetyWkRepSettings.StartDate 와 중복 — Settings 도입 후 Project 컬럼 deprecate 예정
