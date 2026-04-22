# Privilege System Migration Guide

Created: 2026-04-20

This document describes how to apply the **UserRole → Privilege** redesign to an existing
ACI database. The code already reflects the new model (see §1); only the database needs to
catch up (see §2).

---

## 1. 코드 변경 요약

| 구분 | Before | After |
|---|---|---|
| ApplicationUser.Role (enum) | `UserRole Role { get; set; }` | **제거** |
| 신규 엔티티 | — | `Privilege` (마스터), `UserPrivilege` (join) |
| 관계 | 1:1 (user → role) | **many-to-many** (user ↔ privilege) |
| 계층 (Admin ⊇ HrAdmin …) | enum 비교 로직에 내장 | `Services/PrivilegeExpander.cs` 에 코드 하드코딩 |
| 로그인 클레임 | Role 1개 | 직접 부여 priv 들을 Expander 로 확장 → 다중 `ClaimTypes.Role` |
| 관리 UI | 없음 (하드코딩 enum) | `/SystemAdmin/Privileges` (priv 마스터), `/Hr/Users` (사용자↔priv 할당) |
| Program.cs 정책 | `RequireRole("X")` / `FindFirst(ClaimTypes.Role)` | `ctx.User.IsInRole(PrivilegeCodes.X)` (다중 클레임 대응) |

주의: **Code 문자열은 Program.cs 정책 키와 1:1**. 빌트인 priv 의 Code 는 관리 UI 에서 수정/삭제 불가.

---

## 2. DB 마이그레이션

### 2-A. EF CLI 방식 (권장)

로컬에서:

```bash
cd ACI-System/ACI.Web

# 1) 마이그레이션 생성 — 기존 모델 스냅샷과 현재 모델의 diff 를 기반으로 자동 작성
dotnet ef migrations add AddPrivilegeEntities

# 2) 결과 확인 — 아래 변경이 포함되어야 정상
#    - CreateTable "Privileges"
#    - CreateTable "UserPrivileges"  (복합 PK UserId+PrivilegeId)
#    - DropColumn "Role" from "Users"
#    - CreateIndex "IX_Privileges_Code" (Unique)
#    - CreateIndex "IX_UserPrivileges_PrivilegeId"
#    - CreateIndex "IX_UserPrivileges_GrantedByUserId"
cat Migrations/*_AddPrivilegeEntities.cs

# 3) 개발 DB 에 적용
dotnet ef database update
```

개발환경(`Program.cs` 의 `IsDevelopment()` 블록)에서는 앱 기동 시 `Database.MigrateAsync()` 가
자동으로 적용합니다. `DbInitializer.SeedAsync` 가 이어서 11개 빌트인 priv 를 seed 하고,
admin@aci-la.com 에 `Admin` priv 를 1개 부여합니다.

### 2-B. 수동 SQL 방식 (CLI 안 쓸 때)

PostgreSQL 콘솔에서:

```sql
-- ─── 1) Privilege 마스터 ───────────────────────────────────────────────
CREATE TABLE "Privileges" (
    "Id"          serial PRIMARY KEY,
    "Code"        varchar(50)  NOT NULL,
    "Name"        varchar(100) NOT NULL,
    "Description" varchar(500) NULL,
    "IsBuiltIn"   boolean      NOT NULL DEFAULT FALSE,
    "IsActive"    boolean      NOT NULL DEFAULT TRUE,
    "CreatedAt"   timestamp with time zone NOT NULL DEFAULT now(),
    "UpdatedAt"   timestamp with time zone NOT NULL DEFAULT now()
);
CREATE UNIQUE INDEX "IX_Privileges_Code" ON "Privileges" ("Code");

-- ─── 2) User ↔ Privilege 조인 (복합 PK) ────────────────────────────────
CREATE TABLE "UserPrivileges" (
    "UserId"          int NOT NULL REFERENCES "Users" ("Id") ON DELETE CASCADE,
    "PrivilegeId"     int NOT NULL REFERENCES "Privileges" ("Id") ON DELETE CASCADE,
    "GrantedAt"       timestamp with time zone NOT NULL DEFAULT now(),
    "GrantedByUserId" int NULL REFERENCES "Users" ("Id") ON DELETE SET NULL,
    PRIMARY KEY ("UserId", "PrivilegeId")
);
CREATE INDEX "IX_UserPrivileges_PrivilegeId"     ON "UserPrivileges" ("PrivilegeId");
CREATE INDEX "IX_UserPrivileges_GrantedByUserId" ON "UserPrivileges" ("GrantedByUserId");

-- ─── 3) 구 Role 칼럼 제거 ─────────────────────────────────────────────
-- ⚠ 기존 데이터가 있으면 아래 마이그레이션 스크립트 먼저 실행 (2-C 참고)
ALTER TABLE "Users" DROP COLUMN "Role";
```

### 2-C. 기존 Role 데이터 → UserPrivilege 이관 (프로덕션만)

운영 DB에 이미 사용자가 있다면 `DROP COLUMN "Role"` 전에 아래 스크립트로 이관:

```sql
-- 1) 먼저 빌트인 priv 를 seed (앱 기동이 할 수도 있음)
INSERT INTO "Privileges" ("Code","Name","Description","IsBuiltIn","IsActive")
VALUES
    ('Admin',          'System Admin',      'Full system access. Includes all HR and Project admin privileges.', TRUE, TRUE),
    ('HrAdmin',        'HR Admin',          'Access sensitive HR data (PII) and manage employee role assignments.', TRUE, TRUE),
    ('HrManager',      'HR Manager',        'HR approvals and management tasks. Includes HR User privileges.', TRUE, TRUE),
    ('HrUser',         'HR User',           'Basic HR access: view employees and edit general details.', TRUE, TRUE),
    ('LsProjAdmin',    'LS Project Admin',  'Edit Lump Sum project master data (Trades & Subs, etc).', TRUE, TRUE),
    ('JocProjAdmin',   'JOC Project Admin', 'Edit JOC project master data (Trades & Subs, etc).', TRUE, TRUE),
    ('ProjectManager', 'Project Manager',   'Full project schedule edit access.', TRUE, TRUE),
    ('Superintendent', 'Superintendent',    'Field lead: edit lookahead and weekly plans.', TRUE, TRUE),
    ('SafetyOfficer',  'Safety Officer',    'Safety review and incident logging.', TRUE, TRUE),
    ('TradePartner',   'Trade Partner',     'Subcontractor: update status of own tasks only.', TRUE, TRUE),
    ('Viewer',         'Viewer',            'Read-only access.', TRUE, TRUE)
ON CONFLICT ("Code") DO NOTHING;

-- 2) 기존 Role enum 값을 Code 로 매핑하여 UserPrivilege 를 생성
--    (구 enum: 0=Admin, 1=HrAdmin, 2=HrManager, 3=HrUser, 4=LsProjAdmin, 5=JocProjAdmin,
--     6=ProjectManager, 7=Superintendent, 8=SafetyOfficer, 9=TradePartner, 10=Viewer)
INSERT INTO "UserPrivileges" ("UserId","PrivilegeId","GrantedAt")
SELECT u."Id",
       p."Id",
       now()
FROM "Users" u
JOIN "Privileges" p
  ON p."Code" = CASE u."Role"
                    WHEN 0  THEN 'Admin'
                    WHEN 1  THEN 'HrAdmin'
                    WHEN 2  THEN 'HrManager'
                    WHEN 3  THEN 'HrUser'
                    WHEN 4  THEN 'LsProjAdmin'
                    WHEN 5  THEN 'JocProjAdmin'
                    WHEN 6  THEN 'ProjectManager'
                    WHEN 7  THEN 'Superintendent'
                    WHEN 8  THEN 'SafetyOfficer'
                    WHEN 9  THEN 'TradePartner'
                    WHEN 10 THEN 'Viewer'
                END
ON CONFLICT DO NOTHING;

-- 3) 이관 결과 확인
SELECT u."Email", string_agg(p."Code", ',' ORDER BY p."Code") AS priv_codes
FROM   "Users" u
LEFT   JOIN "UserPrivileges" up ON up."UserId" = u."Id"
LEFT   JOIN "Privileges" p       ON p."Id"     = up."PrivilegeId"
GROUP  BY u."Email"
ORDER  BY u."Email";

-- 4) 전부 정상이면 구 칼럼 드롭
ALTER TABLE "Users" DROP COLUMN "Role";
```

---

## 3. 롤백

개발 환경 롤백(로컬):

```bash
# 직전 마이그레이션 undo
dotnet ef database update <이전_마이그레이션_이름>

# 마이그레이션 파일 삭제
dotnet ef migrations remove
```

단, 이 경우 `ApplicationUser.Role` 프로퍼티가 코드에 없으므로 컴파일 이전 상태로
되돌리려면 git 에서 해당 커밋 이전으로 리버트 필요.

---

## 4. 관리 UI 엔트리 포인트

- **빌트인/커스텀 priv 마스터 관리** — `/SystemAdmin/Privileges`
  (Admin 전용, 빌트인 Code 수정·삭제 불가)
- **사용자 ↔ priv 할당** — `/Hr/Users`
  (접근은 HR 권한자 전체, 변경 핸들러는 HrAdmin 전용)
- 계층 전개는 로그인 시 자동. 관리자는 **가장 상위 priv 만** 부여하면 됨
  (예: HrAdmin 하나만 → HrManager, HrUser 도 자동 획득).

---

## 5. 선택적 설정

`appsettings.json` 에 회사 이메일 도메인을 재정의하려면:

```json
{
  "Aci": {
    "EmailDomain": "aci-la.com"
  }
}
```

미지정 시 기본값 `aci-la.com` 사용 (`Pages/Hr/Users/Index.cshtml.cs` 의
`OnPostCreateAsync` 참조).
