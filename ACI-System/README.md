# ACI Project System
**Angeles Contractor Inc. | Los Angeles, CA**

건설 프로젝트 일정 관리 시스템 — Outbuild 유사 기능 구현

---

## 기술 스택

**기본 방침**: Razor Pages 기반 서버 렌더링이 표준이며, **Gantt 화면에 한해서만** 전용 컨트롤(SVAR Gantt, React)을 임베드합니다. 그 외 화면은 Razor + Bootstrap 5로 구성합니다.

| 레이어 | 기술 |
|---|---|
| Framework | **ASP.NET Core (.NET 10)** — Razor Pages + Web API 컨트롤러 |
| 언어 | C# 12 |
| ORM | Entity Framework Core 10 + Npgsql 10 |
| DB | PostgreSQL 15+ |
| Gantt 차트 | **SVAR Gantt** (`wx-react-gantt` 1.3.x) — Gantt 페이지에만 React 컴포넌트로 임베드 |
| Gantt 빌드 | Vite (React 18) → `wwwroot/gantt-dist/gantt-bundle.js` (IIFE 단일 번들) |
| CSS | Bootstrap 5.3 + Bootstrap Icons + Custom(site.css) |
| 차트/분석 | ApexCharts |
| **인증** | **쿠키 기반 인증 (Custom Cookie Authentication)** — `AciCookies` 스킴, 8시간 Sliding Expiration, HttpOnly + SameSite=Lax. **ASP.NET Core Identity는 사용하지 않음.** |
| 비밀번호 해시 | BCrypt.Net-Next 4.x |
| PII 암호화 | AES-256-GCM (AppSettings `Encryption:Key`, 32 bytes Base64) |

> `.NET 10` 기준이며, 프로젝트 전반(`ACI.Web.csproj`의 `TargetFramework=net10.0`, EF Core 10.0.0, Npgsql.EntityFrameworkCore.PostgreSQL 10.0.0)에 일관됩니다. `dotnet-tools.json` 역시 `dotnet-ef 10.0.x` 로 고정되어 있습니다.

---

## 시작하기

### 1. 사전 요구사항
- **.NET 10 SDK**
- PostgreSQL 15+
- Visual Studio 2022 17.12+ (또는 .NET 10을 지원하는 최신 VS / VS Code + C# Dev Kit)
- Node.js 18+ (SVAR Gantt 번들 빌드용, `gantt-client/`)

### 2. DB 연결 설정
`ACI.Web/appsettings.Development.json` 수정:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=aci_system_dev;Username=postgres;Password=YOUR_PASSWORD"
  },
  "Encryption": {
    "Key": "YOUR_BASE64_32BYTE_KEY"
  }
}
```

> `Encryption:Key`는 32바이트(Base64 인코딩된 문자열) AES-256 키입니다. 운영 환경에서는 appsettings가 아닌 환경변수 또는 시크릿 매니저로 주입할 것을 권장합니다.

### 3. Gantt 번들 빌드 (최초 1회 + 수정 시)
```bash
cd gantt-client
npm install
npm run build
```
→ `ACI.Web/wwwroot/gantt-dist/gantt-bundle.js` + `gantt-bundle.css` 생성. Razor 레이아웃은 Gantt 페이지에서만 이 번들을 로드합니다(`ViewData["LoadGanttBundle"] = true`).

### 4. 첫 실행 (DB 마이그레이션 + 시드)
```bash
cd ACI.Web

# 패키지 복원
dotnet restore

# EF Core 마이그레이션 (최초 1회, 이미 Migrations 폴더에 들어있으면 생략)
dotnet ef migrations add InitialCreate

# 실행 (개발 환경에서는 기동 시 MigrateAsync + 시드 자동 수행)
dotnet run
```

### 5. 기본 로그인 계정 (개발 시드)
| 계정 | 비밀번호 | 권한 |
|---|---|---|
| admin@aci-la.com | Admin@12345 | 시스템 관리자 (Admin) |
| pm@aci-la.com | Pm@12345 | Project Manager (PM) |

---

## 인증 & 권한

### 인증 방식
- **쿠키 기반 인증** (`AddAuthentication("AciCookies").AddCookie(...)`)
- 로그인 성공 시 `ClaimsIdentity`에 `UserId`, `Email`, `Name`, `Role` 클레임을 담아 암호화된 세션 쿠키로 발급
- `ExpireTimeSpan = 8h`, `SlidingExpiration = true`, `HttpOnly`, `SameSite=Lax`
- 비밀번호는 BCrypt로 해시되어 DB 저장 (ApplicationUser.PasswordHash)

### 역할(Role) 정의
| Role | 설명 |
|---|---|
| **Admin** | 시스템 관리자. **모든 기능 접근 가능(HrAdmin 권한을 포함).** |
| **HrAdmin** | HR 관리자. 인사/조직 관리 전담 |
| PM | Project Manager |
| Superintendent | 현장 Superintendent |
| SafetyOfficer | 안전 담당 |
| TradePartner | 공종 파트너 |
| Viewer | 읽기 전용 |

### 권한 정책 — HrAdmin
다음 영역은 `[Authorize(Policy = "HrAdmin")]` 정책으로 보호되며, **HrAdmin 또는 Admin** 역할만 접근 가능합니다.

- **직원 목록 / 직원 상세 / 직원 문서** (`/Admin/Employees/*`, `EmployeeDocumentController`)
- **조직 단위 관리** (`/Admin/OrgUnits/*`)
- **직책(Job Positions) 관리** (`/Admin/JobPositions/*`)
- **사용자 관리** (`/Admin/Users/*`)
- SSN, TIN, 운전면허, 여권, Alien Number 등 암호화된 PII 필드 **복호화/열람**

정책 정의 (`Program.cs`):
```csharp
options.AddPolicy("HrAdmin", policy =>
    policy.RequireAssertion(ctx =>
    {
        var role = ctx.User.FindFirst(ClaimTypes.Role)?.Value;
        return role == "Admin" || role == "HrAdmin";   // Admin은 HrAdmin을 포함
    }));
```

> **원칙**: Admin은 시스템 관리자로서 **모든 권한을 가지며, HrAdmin이 보유한 모든 권한도 자동 포함**합니다. 별도 역할을 부여할 필요 없이 Admin 한 역할로 HR 영역까지 접근 가능합니다.

---

## PII 암호화

직원의 민감 개인정보는 **AES-256-GCM**으로 암호화되어 DB에 저장되며, `HrAdmin` 정책 통과 시에만 복호화됩니다.

**암호화 대상 필드** (`Employee.cs`):
- SSN, TIN (세무 식별)
- Driver's License Number
- Passport Number
- Alien Registration Number

**구현**: `Services/EncryptionService.cs` — `AesGcm` 클래스 사용, Nonce(12B) + Ciphertext + Tag(16B)를 Base64로 합쳐 저장.

> 암호화 유지는 필수 사항입니다. 필드 추가/변경 시에도 개인정보 성격의 필드는 반드시 Encrypted 버전으로 저장합니다.

---

## 주요 기능

### Baseline Schedule (베이스라인 스케줄)
- **SVAR Gantt** 기반 인터랙티브 간트 차트 (React 컴포넌트 `BaselineGantt.jsx`)
- WBS 계층 구조 (드래그&드롭, 서브태스크 무제한 중첩)
- 작업 간 의존성 (FS, SS, FF, SF + lag/lead)
- 크리티컬 패스 하이라이트
- 스케일 전환 (일/주/월/분기/년)
- Today 마커, Trade별 색상, 진행률 바

### Current Schedule & Revision (Working Schedule)
- **SVAR Gantt** — `ProgressGantt.jsx`
- 베이스라인 대비 delta 표시 (지연/신규/삭제/완료)
- Change Log(리비전) 기반 승인 워크플로 (Draft → Submitted → Approved/Rejected)
- Revision별 첨부 문서(변경명령서, 공문, RFI 등)

### Schedule Comparison
- **SVAR Gantt** — `ComparisonGantt.jsx`
- 두 베이스라인 간 비교 / 베이스라인 ↔ Current Plan 비교
- 리비전 애니메이션 프레임 재생

### What-If Simulation (시나리오)
- Current Plan 또는 특정 Baseline 기준으로 Sparse Delta 시뮬레이션
- Impact 요약 (수정 태스크 수, 총 일수 영향, 예상 종료일)
- Promote → Current Plan 반영 + ScheduleChange 감사 로그

### Lookahead Schedule (룩어헤드)
- 3주/4주/6주 선행 계획
- 공종별 그룹 표시
- 제약 조건(Constraint) 추적
- 베이스라인 스케줄 연동 (Pull From Schedule)

### Weekly Work Plan (주간 업무 계획)
- Last Planner System(LPS) 기반
- 일별 작업 커밋 & 완료 체크
- PPC(Percent Plan Complete) 자동 계산
- Reason for Variance 기록

### Analytics
- PPC 트렌드 (주별)
- Variance 분류 분석
- 프로젝트 진행률

---

## 프로젝트 구조

```
ACI-System/
├── ACI.sln
├── dotnet-tools.json              # dotnet-ef 10.x 로컬 툴
├── ACI.Web/                       # 메인 프로젝트 (.NET 10, Razor Pages)
│   ├── Program.cs                 # 쿠키 인증 + DI + HrAdmin 정책
│   ├── Controllers/               # REST API (Gantt용 + EmployeeDocument)
│   ├── Data/
│   │   ├── Entities/              # EF Core 도메인 모델
│   │   ├── AppDbContext.cs
│   │   └── DbInitializer.cs       # 시드 데이터
│   ├── Services/                  # 비즈니스 로직
│   │   ├── EncryptionService.cs   # AES-256-GCM
│   │   ├── BaselineService.cs
│   │   ├── ProgressScheduleService.cs
│   │   ├── SimulationService.cs
│   │   ├── LookaheadService.cs
│   │   ├── WeeklyPlanService.cs
│   │   ├── GanttDataService.cs    # SVAR Gantt ↔ ScheduleTask 변환
│   │   └── ProjectService.cs
│   ├── Migrations/                # EF Core 10 마이그레이션
│   ├── Pages/                     # Razor Pages (기본 UI)
│   │   ├── Account/               # 로그인/로그아웃
│   │   ├── Admin/                 # HrAdmin 정책 적용 영역
│   │   │   ├── Employees/
│   │   │   ├── OrgUnits/
│   │   │   ├── JobPositions/
│   │   │   └── Users/             # (예정)
│   │   ├── Projects/              # 프로젝트 관리
│   │   ├── Schedule/              # 베이스라인 Gantt (SVAR)
│   │   ├── Baselines/             # 베이스라인 버전 관리
│   │   ├── Progress/              # Current Schedule + Revisions + Comparison
│   │   ├── Simulations/           # What-If
│   │   ├── Lookahead/
│   │   ├── WeeklyPlan/
│   │   └── Shared/_Layout.cshtml
│   └── wwwroot/                   # 정적 파일 (CSS/JS)
│       └── gantt-dist/            # SVAR Gantt 빌드 산출물(자동 생성)
└── gantt-client/                  # SVAR Gantt React 소스 (Vite 프로젝트)
    ├── package.json               # wx-react-gantt 1.3.x
    ├── vite.config.js             # IIFE 출력 → ACI.Web/wwwroot/gantt-dist/
    └── src/
        ├── main.jsx               # Razor DOM 노드에 React 마운트
        ├── BaselineGantt.jsx
        ├── ProgressGantt.jsx
        ├── ComparisonGantt.jsx
        ├── components/            # GanttToolbar, TaskDialog
        ├── hooks/                 # useGanttActionButtons, useNarrowBarHider
        ├── utils/                 # dateUtils, cascadeUtils
        └── styles/aci-gantt.css   # SVAR 전용 커스텀 스타일
```

---

## Gantt 통합 방식

일반 화면은 Razor Pages로 그리지만, Gantt는 SVAR의 React 컴포넌트를 Vite로 IIFE 번들링 후 Razor 페이지에 **DOM 마운트 포인트**를 심어 주입합니다.

**Razor 측** (`Pages/Schedule/Index.cshtml` 등):
```html
<div id="baseline-gantt-root"
     data-projectId="@Model.ProjectId"
     data-apiBase="/api/gantt"></div>
```
레이아웃은 `ViewData["LoadGanttBundle"] = true` 일 때만 `/gantt-dist/gantt-bundle.js`를 로드합니다.

**React 측** (`gantt-client/src/main.jsx`):
```js
const el = document.getElementById("baseline-gantt-root");
if (el) createRoot(el).render(<BaselineGantt {...el.dataset} />);
```

이 방식의 장점: SPA 라우팅/인증/상태관리를 구축하지 않고도 Razor 보안/세션을 그대로 활용. 단점: React 컴포넌트 측 상태와 서버 DB가 REST(`/api/gantt/...`)로만 통신.

---

## 다음 개발 단계 (TODO)

- [ ] Admin/Trades.cshtml — 공종/하도급 관리 페이지
- [ ] Admin/Users.cshtml — 사용자 관리 페이지 (HrAdmin 정책)
- [ ] Projects/Detail.cshtml — 프로젝트 상세/수정
- [ ] Analytics/Index.cshtml — 분석 대시보드
- [ ] Lookahead → WeeklyPlan 작업 Pull 기능
- [ ] P6 XML Import/Export
- [ ] 엑셀 내보내기 (ClosedXML)
- [ ] 베이스라인 비교 표시 (애니메이션)
- [ ] 동시성 토큰(RowVersion) 추가로 충돌 감지

---

## SVAR Gantt 라이선스 안내

`wx-react-gantt`는 SVAR의 React Gantt 컴포넌트이며, GPL 커뮤니티 라이선스 + 상용 라이선스의 듀얼 라이선싱 정책입니다. 사내 내부 시스템에서 GPL 조건으로 사용 중이며, SaaS 형태의 외부 배포 또는 재배포 시 SVAR의 최신 라이선스 조건을 재확인해야 합니다.

참고: https://docs.svar.dev/react/gantt/
