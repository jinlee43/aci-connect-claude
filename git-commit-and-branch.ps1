# ACI Connect - dhtmlxGantt 버전 커밋 & 브랜치 생성 스크립트
# PowerShell 에서 실행: .\git-commit-and-branch.ps1

Set-Location $PSScriptRoot

Write-Host "=== ACI Connect Git 커밋 스크립트 ===" -ForegroundColor Cyan

# 1. Lock 파일 제거
$lockFiles = @(".git\index.lock", ".git\HEAD.lock", ".git\MERGE_HEAD.lock")
foreach ($lock in $lockFiles) {
    if (Test-Path $lock) {
        Remove-Item $lock -Force
        Write-Host "삭제됨: $lock" -ForegroundColor Yellow
    }
}

# 2. 파일 스테이징
Write-Host "`n파일 스테이징 중..." -ForegroundColor Cyan
git add ACI-System/ACI.Web/Pages/Account/Login.cshtml.cs
git add ACI-System/ACI.Web/Pages/Account/Logout.cshtml
git add ACI-System/ACI.Web/Pages/Progress/Index.cshtml
git add ACI-System/ACI.Web/Pages/Progress/Index.cshtml.cs
git add ACI-System/ACI.Web/Pages/Progress/Revisions.cshtml.cs
git add ACI-System/ACI.Web/Pages/Schedule/Index.cshtml
git add ACI-System/ACI.Web/Pages/Shared/_Layout.cshtml
git add ACI-System/ACI.Web/Services/GanttDataService.cs
git add ACI-System/ACI.Web/Services/IGanttDataService.cs
git add ACI-System/ACI.Web/Migrations/patch_missing_tables.sql
git add ACI-System/ACI.Web/db/

git status --short

# 3. main 에 커밋
Write-Host "`ncommit 중..." -ForegroundColor Cyan
git commit -m "feat: Baseline/Current Schedule Gantt 툴바 완성 및 버그 수정 (2026-04-05)

React 전환 전 마지막 dhtmlxGantt 버전

- %b->%M, Q%q->arrow function 포맷 버그 수정 (dhtmlxGantt GPL 호환)
- zoom.ext.init() 5단계 레벨 (Day/Week/Month/Quarter/Year) 추가
- Current Schedule: pgFilterTasks, pgFitToScreen, pgExportExcel, pgToggleLinks 구현
- gantt.addTaskLayer GPL Pro 기능 가드 추가
- reset_current_schedule_project1.sql 추가 (WorkingTasks 리셋용)
- Login/Logout 페이지 개선
- GanttDataService: baseline/constraint 필드 지원 추가"

if ($LASTEXITCODE -ne 0) {
    Write-Host "커밋 실패. 오류를 확인하세요." -ForegroundColor Red
    exit 1
}

# 4. dhtmlx-gantt 브랜치 생성 (현재 커밋 지점에서)
Write-Host "`ndhtmlx-gantt 브랜치 생성 중..." -ForegroundColor Cyan
git branch dhtmlx-gantt
Write-Host "브랜치 'dhtmlx-gantt' 생성됨 (이 버전으로 언제든 복귀 가능)" -ForegroundColor Green

# 5. main + dhtmlx-gantt 브랜치 모두 push
Write-Host "`nGitHub 에 push 중..." -ForegroundColor Cyan
git push origin main
git push origin dhtmlx-gantt

Write-Host "`n=== 완료 ===" -ForegroundColor Green
Write-Host "main: React 전환용 최신 코드" -ForegroundColor White
Write-Host "dhtmlx-gantt: dhtmlxGantt 완성 버전 (복귀용)" -ForegroundColor White
Write-Host "`n나중에 이 버전으로 돌아오려면:" -ForegroundColor Yellow
Write-Host "  git checkout dhtmlx-gantt" -ForegroundColor White

git log --oneline -5
