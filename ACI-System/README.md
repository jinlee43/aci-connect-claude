# ACI Project System
**Angeles Contractor Inc. | Los Angeles, CA**

건설 프로젝트 일정 관리 시스템 — Outbuild 유사 기능 구현

---

## 기술 스택

| 레이어 | 기술 |
|---|---|
| Framework | ASP.NET Core 8 (Razor Pages + Web API) |
| 언어 | C# 12 |
| ORM | Entity Framework Core 8 + Npgsql |
| DB | PostgreSQL |
| Gantt 차트 | dhtmlxGantt (GPL — 사내용 무료) |
| 동적 UI | HTMX 1.9 |
| CSS | Bootstrap 5.3 + Custom |
| 차트/분석 | ApexCharts |
| 인증 | ASP.NET Core Identity |

---

## 시작하기

### 1. 사전 요구사항
- .NET 8 SDK
- PostgreSQL 15+
- Visual Studio 2022 (또는 VS Code)

### 2. DB 연결 설정
`ACI.Web/appsettings.Development.json` 수정:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=aci_system_dev;Username=postgres;Password=YOUR_PASSWORD"
  }
}
```

### 3. 첫 실행 (DB 마이그레이션 + 시드)
```bash
cd ACI.Web

# 패키지 복원
dotnet restore

# EF Core 마이그레이션 생성
dotnet ef migrations add InitialCreate

# 실행 (마이그레이션 자동 적용 + 시드)
dotnet run
```

### 4. 기본 로그인 계정
| 계정 | 비밀번호 | 권한 |
|---|---|---|
| admin@aci-la.com | Admin@12345 | 시스템 관리자 |
| pm@aci-la.com | Pm@12345 | Project Manager |

---

## 주요 기능

### Baseline Schedule (베이스라인 스케줄)
- dhtmlxGantt 기반 인터랙티브 간트 차트
- WBS 계층 구조 (드래그&드롭)
- 작업 간 의존성 (FS, SS, FF, SF)
- 크리티컬 패스 하이라이트
- 스케일 전환 (주/월/분기)
- Today 마커

### Lookahead Schedule (룩어헤드)
- 3주/4주/6주 선행 계획
- 공종별 그룹 표시
- 제약 조건(Constraint) 추적
- 베이스라인 스케줄 연동

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
ACI.Web/
├── Controllers/         # REST API (Gantt용)
├── Data/
│   ├── Entities/        # EF Core 모델
│   ├── AppDbContext.cs
│   └── DbInitializer.cs # 시드 데이터
├── Services/            # 비즈니스 로직
├── Pages/               # Razor Pages
│   ├── Account/         # 로그인/로그아웃
│   ├── Projects/        # 프로젝트 관리
│   ├── Schedule/        # 베이스라인 스케줄 (Gantt)
│   ├── Lookahead/       # 룩어헤드
│   └── WeeklyPlan/      # 주간 업무 계획
└── wwwroot/             # 정적 파일 (CSS/JS)
```

---

## 다음 개발 단계 (TODO)

- [ ] Admin/Trades.cshtml — 공종/하도급 관리 페이지
- [ ] Admin/Users.cshtml — 사용자 관리 페이지
- [ ] Projects/Detail.cshtml — 프로젝트 상세/수정
- [ ] Analytics/Index.cshtml — 분석 대시보드
- [ ] Lookahead → WeeklyPlan 작업 Pull 기능
- [ ] P6 XML Import/Export
- [ ] 엑셀 내보내기 (ClosedXML)
- [ ] 베이스라인 비교 표시

---

## Gantt 라이선스 안내

dhtmlxGantt GPL 버전은 **사내 내부 시스템에 한해 무료** 사용 가능합니다.
외부 배포 또는 SaaS 제공 시 Commercial License 필요.
