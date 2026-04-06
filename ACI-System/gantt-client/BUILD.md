# SVAR Gantt React 빌드 안내

## 최초 설치 (한 번만)

```bash
cd ACI-System/gantt-client
npm install
```

## 빌드 (코드 수정 후 실행)

```bash
cd ACI-System/gantt-client
npm run build
```

빌드 결과물이 자동으로 `ACI.Web/wwwroot/gantt-dist/` 에 생성됩니다.
- `gantt-bundle.js`
- `gantt-bundle.css`

## 개발 모드 (Hot Reload)

```bash
npm run dev
```

→ http://localhost:5174 에서 React 컴포넌트만 독립 실행 (API는 ASP.NET 서버 필요)

## 파일 구조

```
gantt-client/
├── src/
│   ├── main.jsx              ← 마운트 포인트 (페이지별 컴포넌트 연결)
│   ├── BaselineGantt.jsx     ← Baseline Schedule 전체
│   ├── ProgressGantt.jsx     ← Current Schedule 전체
│   ├── components/
│   │   └── GanttToolbar.jsx  ← 공용 툴바
│   ├── utils/
│   │   └── dateUtils.js      ← 날짜 변환 유틸
│   └── styles/
│       └── aci-gantt.css     ← 커스텀 스타일
└── vite.config.js
```

## SVAR Gantt 문서

https://docs.svar.dev/react/gantt/
