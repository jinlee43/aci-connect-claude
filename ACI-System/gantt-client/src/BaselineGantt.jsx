import { useState, useEffect, useRef, useCallback, useMemo } from "react";
import { Gantt, defaultEditorShape } from "wx-react-gantt";
import "wx-react-gantt/dist/gantt.css";
import GanttToolbar from "./components/GanttToolbar";
import TaskDialog from "./components/TaskDialog";
import { toSvarTask, toSvarLink, formatDate, displayDate, sortByTreeId } from "./utils/dateUtils";
import { cascadeFS } from "./utils/cascadeUtils";
import "./styles/aci-gantt.css";
import { useGanttActionButtons } from "./hooks/useGanttActionButtons";
import { useNarrowBarHider } from "./hooks/useNarrowBarHider";

// ── 스케일 설정 ──────────────────────────────────────────────────────────────
const SCALE_CONFIGS = {
  day: [
    { unit: "week",  step: 1, format: "'W'w MMM yy" },
    { unit: "day",   step: 1, format: "d EEE" },
  ],
  week: [
    { unit: "month", step: 1, format: "MMM yyyy" },
    { unit: "week",  step: 1, format: "'W'w" },
  ],
  month: [
    { unit: "year",  step: 1, format: "yyyy" },
    { unit: "month", step: 1, format: "MMM" },
  ],
  quarter: [
    { unit: "year",    step: 1, format: "yyyy" },
    { unit: "quarter", step: 1, format: "QQQ" },
  ],
  year: [
    { unit: "year",  step: 1, format: "yyyy" },
    { unit: "month", step: 3, format: "MMM" },
  ],
};
const SCALE_ORDER = ["day", "week", "month", "quarter", "year"];

// ── 태스크 색상 ──────────────────────────────────────────────────────────────
function getTaskColor(task, colorMode, criticalIds) {
  // Critical Path 강조 (빨간색)
  if (criticalIds?.has(task.id)) return "#ef4444";

  if (colorMode === "trade" && task.trade_color) return task.trade_color;

  if (task.type === "summary") return "#1e40af";
  if (task.type === "milestone") return "#d97706";

  const pct = (task.progress || 0) * 100;
  const now = new Date();
  if (pct >= 100) return "#16a34a";
  if (task.end && task.end < now && pct < 100) return "#ef4444";
  if (pct > 0) return "#3b82f6";
  return "#94a3b8";
}

// ── Critical Path 계산 (Forward/Backward pass) ────────────────────────────────
function calcCriticalPath(tasks, links) {
  if (!tasks.length) return new Set();

  // id → task 맵
  const taskMap = new Map(tasks.map((t) => [t.id, { ...t }]));
  const dayMs = 24 * 3600 * 1000;

  // Forward pass: earliest start/finish
  tasks.forEach((t) => {
    const task = taskMap.get(t.id);
    task.ES = task.start ? task.start.getTime() : 0;
    task.EF = task.end   ? task.end.getTime()   : task.ES + (task.duration || 1) * dayMs;
  });

  // FS 링크 기반 forward propagation (간략화)
  const fsLinks = links.filter((l) => l.type === "e2s");
  let changed = true;
  for (let iter = 0; iter < 20 && changed; iter++) {
    changed = false;
    fsLinks.forEach((l) => {
      const src = taskMap.get(l.source);
      const tgt = taskMap.get(l.target);
      if (!src || !tgt) return;
      if (tgt.ES < src.EF) {
        const dur = tgt.EF - tgt.ES;
        tgt.ES = src.EF;
        tgt.EF = tgt.ES + dur;
        changed = true;
      }
    });
  }

  // 프로젝트 종료 시간
  const projectEnd = Math.max(...Array.from(taskMap.values()).map((t) => t.EF));

  // Backward pass
  taskMap.forEach((t) => {
    t.LF = projectEnd;
    t.LS = t.LF - (t.EF - t.ES);
  });

  let changed2 = true;
  for (let iter = 0; iter < 20 && changed2; iter++) {
    changed2 = false;
    [...fsLinks].reverse().forEach((l) => {
      const src = taskMap.get(l.source);
      const tgt = taskMap.get(l.target);
      if (!src || !tgt) return;
      if (src.LF > tgt.LS) {
        const dur = src.LF - src.LS;
        src.LF = tgt.LS;
        src.LS = src.LF - dur;
        changed2 = true;
      }
    });
  }

  // Total float = LS - ES ≈ 0 → critical
  const TOLERANCE_MS = dayMs; // 1일 이내 = critical
  const criticalIds = new Set();
  taskMap.forEach((t, id) => {
    if (t.type === "summary") return;
    const float = t.LS - t.ES;
    if (Math.abs(float) <= TOLERANCE_MS) criticalIds.add(id);
  });
  return criticalIds;
}

// ── 컬럼 설정: Outbuild 스타일 순서 (ID → Task → Actions → Start → Dur → End → % → Responsible → Trade)
// seqMap: Map<taskId, sequentialNumber> — 태스크 표시 순서 기반 정수 일련번호
function buildColumns(showTrade = true, seqMap = new Map()) {
  const all = [
    {
      id: "wbs",
      header: "ID",
      width: 45,
      align: "center",
      template: (_, task) => String(seqMap.get(task.id) ?? task.id),
    },
    {
      // flexgrow 제거: SVAR는 flexgrow 컬럼 존재 시 grid를 440px로 고정 → Task 컬럼 사라짐
      id: "text",
      header: "Task",
      width: 200,
      tree: true,
      template: (t, task) => {
        const prefix = task.type === "summary" ? "▸ " : task.type === "milestone" ? "◆ " : "  ";
        const text = prefix + (t || "");
        return task.type === "summary" ? `<b>${text}</b>` : text;
      },
    },
    // ── Action 컬럼: Outbuild처럼 Task 바로 옆에 ──────────────────────────────
    {
      id: "action",
      header: "Actions",
      width: 88,   /* + 연필 쓰레기통 가로 배치에 충분한 너비 */
      align: "center",
    },
    {
      id: "start",
      header: "Start",
      width: 68,
      align: "center",
      template: (t) => t ? displayDate(t) : "",
    },
    {
      id: "duration",
      header: "Dur",
      width: 46,
      align: "center",
      template: (t) => (t != null ? t : "") + " d",
    },
    {
      id: "end",
      header: "End",
      width: 68,
      align: "center",
      template: (t) => t ? displayDate(t) : "",
    },
    {
      id: "progress",
      header: "%",
      width: 44,
      align: "center",
      template: (t) => Math.round((t || 0) * 100) + "%",
    },
    {
      id: "responsible",
      header: "Responsible",
      width: 96,   /* "RESPONSIBLE" 헤더가 잘리지 않는 너비 */
      align: "center",
      template: (_, task) => {
        if (!task.assigned_to_name) return "";
        const parts = task.assigned_to_name.split(" ").filter(Boolean);
        return parts.length >= 2
          ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
          : (parts[0] || "").substring(0, 2).toUpperCase();
      },
    },
    {
      id: "trade_name",
      header: "Trade",
      width: 80,
      template: (t) => t || "",
    },
  ];
  // text(Task) · action 은 항상 유지, trade_name 만 토글
  const alwaysVisible = new Set(["wbs", "text", "action"]);
  return showTrade
    ? all
    : all.filter((c) => alwaysVisible.has(c.id) || c.id !== "trade_name");
}

// ── BaselineGantt ─────────────────────────────────────────────────────────────
export default function BaselineGantt({ projectId, apiBase = "/api", importXmlUrl }) {
  const [tasks, setTasks]       = useState([]);
  const [links, setLinks]       = useState([]);
  const [allTasks, setAllTasks] = useState([]);
  const [allLinks, setAllLinks] = useState([]);
  const [loading, setLoading]   = useState(true);
  const [error, setError]       = useState(null);
  const [currentScale, setCurrentScale] = useState("week");
  const [showLinks, setShowLinks]       = useState(true);
  const [showCriticalPath, setShowCriticalPath] = useState(false);
  const [criticalIds, setCriticalIds]   = useState(new Set());
  const [showFreezeModal, setShowFreezeModal] = useState(false);
  const [ganttStart, setGanttStart]     = useState(null);
  const [ganttEnd, setGanttEnd]         = useState(null);
  const [colorMode, setColorMode]       = useState("status"); // "status" | "trade"
  const [activeLookahead, setActiveLookahead] = useState(null); // weeks | null
  const [showTrade, setShowTrade]       = useState(false);
  // ── 다이얼로그 상태 ──────────────────────────────────────────────────────────
  const [dialogState, setDialogState]   = useState(null); // null | { mode:'add'|'edit', task, parentId }
  const [metaTrades, setMetaTrades]     = useState([]);
  const [metaEmployees, setMetaEmployees] = useState([]);
  // ── 스케줄 잠금 상태 ─────────────────────────────────────────────────────────
  const [scheduleEditable, setScheduleEditable] = useState(true);
  const [showRevisionModal, setShowRevisionModal] = useState(false);

  const ganttApi       = useRef(null);
  const ganttContainer = useRef(null);
  // 트리 선순(depth-first pre-order) 탐색 기반 정수 일련번호 맵
  // → SVAR 화면 표시 순서와 일치 (배열 인덱스 기반은 parent-child 순서 불일치)
  const columns = useMemo(() => {
    const childMap = new Map();
    allTasks.forEach(t => {
      const pid = t.parent ?? 0;
      if (!childMap.has(pid)) childMap.set(pid, []);
      childMap.get(pid).push(t);
    });
    const seqMap = new Map();
    let counter = 0;
    const walk = (pid) => {
      (childMap.get(pid) || []).sort((a, b) => a.id - b.id).forEach(task => {
        seqMap.set(task.id, ++counter);
        walk(task.id);
      });
    };
    walk(0);
    return buildColumns(showTrade, seqMap);
  }, [showTrade, allTasks]);

  // ── 편집/삭제 버튼 주입 (MutationObserver + SVAR API) ──────────────────────
  useGanttActionButtons(ganttApi, ganttContainer, {
    onDeleteTask: (taskId) => handleDeleteTask({ id: taskId }),
    onEditTask:   (taskId) => openEditDialog(taskId),
  });

  // ── SVAR add-task 인터셉트 → 다이얼로그로 대체 ──────────────────────────────
  useEffect(() => {
    const api = ganttApi.current;
    if (!api?.intercept) return;
    return api.intercept("add-task", (ev) => {
      // SVAR 네이티브 + 버튼이 아닌 직접 exec("add-task")는 통과
      if (ev._fromDialog) return true;
      openAddDialog(ev.parent ?? 0);
      return false;
    });
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [ganttApi.current]);
  // ── 좁은 바 레이블 숨김 ──────────────────────────────────────────────────
  useNarrowBarHider(ganttContainer);

  // ── 데이터 로드 ──────────────────────────────────────────────────────────
  useEffect(() => {
    setLoading(true);
    Promise.all([
      fetch(`${apiBase}/gantt/projects/${projectId}/data`, { credentials: "include" }).then(r => { if (!r.ok) throw new Error(`HTTP ${r.status}`); return r.json(); }),
      fetch(`${apiBase}/gantt/projects/${projectId}/meta`, { credentials: "include" }).then(r => r.json()).catch(() => ({ trades: [], employees: [] })),
      fetch(`${apiBase}/gantt/projects/${projectId}/schedule-status`, { credentials: "include" }).then(r => r.json()).catch(() => ({ editable: true })),
    ])
      .then(([d, meta, status]) => {
        const svarTasks = sortByTreeId((d.data  || []).map(toSvarTask));
        const svarLinks = (d.links || []).map(toSvarLink);
        setAllTasks(svarTasks);
        setAllLinks(svarLinks);
        setTasks(svarTasks);
        setLinks(svarLinks);
        setMetaTrades(meta.trades || []);
        setMetaEmployees(meta.employees || []);
        setScheduleEditable(status.editable !== false);
        setLoading(false);
      })
      .catch((e) => { setError(e.message); setLoading(false); });
  }, [projectId, apiBase]);

  // ── 링크 디버그 ──────────────────────────────────────────────────────────
  useEffect(() => {
    console.log(`[Gantt] tasks=${allTasks.length}, links=${allLinks.length}`, allLinks.slice(0, 5));
  }, [allTasks, allLinks]);

  // ── Critical Path 계산 ───────────────────────────────────────────────────
  useEffect(() => {
    if (showCriticalPath && allTasks.length) {
      setCriticalIds(calcCriticalPath(allTasks, allLinks));
    } else {
      setCriticalIds(new Set());
    }
  }, [showCriticalPath, allTasks, allLinks]);

  // ── bar color: task.color 프로퍼티 직접 설정 (SVAR 네이티브 방식) ─────────
  const coloredTasks = useMemo(
    () => tasks.map((t) => ({ ...t, color: getTaskColor(t, colorMode, criticalIds) })),
    [tasks, colorMode, criticalIds]
  );

  // ── 검색 ─────────────────────────────────────────────────────────────────
  const handleSearch = useCallback((q) => {
    if (!q.trim()) { setTasks(allTasks); return; }
    const lower = q.toLowerCase();
    const matchIds = new Set();
    allTasks.forEach((t) => {
      if (t.text?.toLowerCase().includes(lower) || t.wbs_code?.toLowerCase().includes(lower) || t.trade_name?.toLowerCase().includes(lower)) {
        matchIds.add(t.id);
        let p = t.parent;
        while (p && p !== 0) {
          matchIds.add(p);
          p = allTasks.find((x) => x.id === p)?.parent;
        }
      }
    });
    setTasks(allTasks.filter((t) => matchIds.has(t.id)));
  }, [allTasks]);

  // ── Lookahead 필터 (Outbuild 핵심) ───────────────────────────────────────
  const handleLookahead = useCallback((weeks) => {
    setActiveLookahead(weeks);
    if (weeks === null) {
      setGanttStart(null);
      setGanttEnd(null);
      return;
    }
    const now = new Date();
    // 오늘부터 N주
    const end = new Date(now.getTime() + weeks * 7 * 24 * 3600 * 1000);
    setGanttStart(new Date(now.getTime() - 2 * 24 * 3600 * 1000)); // 2일 여유
    setGanttEnd(end);
    // 해당 기간에 겹치는 태스크만 표시
    const lookIds = new Set();
    allTasks.forEach((t) => {
      const tStart = t.start;
      const tEnd   = t.end || tStart;
      if (!tStart) return;
      if (tStart <= end && tEnd >= now) {
        lookIds.add(t.id);
        let p = t.parent;
        while (p && p !== 0) {
          lookIds.add(p);
          p = allTasks.find((x) => x.id === p)?.parent;
        }
      }
    });
    setTasks(lookIds.size ? allTasks.filter((t) => lookIds.has(t.id)) : allTasks);
  }, [allTasks]);

  // ── Zoom ─────────────────────────────────────────────────────────────────
  const handleZoomIn  = () => { const i = SCALE_ORDER.indexOf(currentScale); if (i > 0) setCurrentScale(SCALE_ORDER[i-1]); };
  const handleZoomOut = () => { const i = SCALE_ORDER.indexOf(currentScale); if (i < SCALE_ORDER.length-1) setCurrentScale(SCALE_ORDER[i+1]); };

  // ── Fit to Screen ────────────────────────────────────────────────────────
  const handleFit = () => {
    if (!tasks.length) return;
    const all = [...tasks.map((t) => t.start), ...tasks.map((t) => t.end || t.start)].filter(Boolean);
    if (!all.length) return;
    const pad = 7 * 24 * 3600 * 1000;
    setGanttStart(new Date(Math.min(...all.map((d) => d.getTime())) - pad));
    setGanttEnd(new Date(Math.max(...all.map((d) => d.getTime())) + pad));
    setActiveLookahead(null);
  };

  const handleToday = () => {
    if (ganttApi.current?.exec) ganttApi.current.exec("scroll-to", { date: new Date() });
  };

  const handleDateRangeApply = (from, to) => {
    if (from) setGanttStart(new Date(from));
    if (to)   setGanttEnd(new Date(to));
    setActiveLookahead(null);
  };
  const handleDateRangeReset = () => { setGanttStart(null); setGanttEnd(null); setActiveLookahead(null); };

  // ── CRUD 이벤트 ──────────────────────────────────────────────────────────
  const handleAddTask = useCallback(async ({ id, task }) => {
    try {
      const res = await fetch(`${apiBase}/gantt/projects/${projectId}/task`, {
        method: "POST", headers: { "Content-Type": "application/json" }, credentials: "include",
        body: JSON.stringify(toApiTask(task, projectId)),
      });
      const data = await res.json();
      if (data.tid && ganttApi.current) {
        const updatedTask = { ...task, id: data.tid };
        ganttApi.current.exec("update-task", { id, task: updatedTask });
        // allTasks에 추가 후 id 순 재정렬 → seqMap 재계산으로 일련번호 업데이트
        setAllTasks(prev => sortByTreeId([...prev, updatedTask]));
      }
    } catch (e) { console.error("[Gantt] add-task:", e); }
  }, [projectId, apiBase]);

  // ── 다이얼로그 open/save 핸들러 ─────────────────────────────────────────────
  const openAddDialog = useCallback((parentId = 0) => {
    setDialogState({ mode: "add", task: null, parentId });
  }, []);

  const openEditDialog = useCallback((taskId) => {
    const task = ganttApi.current?.getTask?.(taskId) ?? allTasks.find(t => t.id === taskId);
    if (!task) return;
    setDialogState({ mode: "edit", task, parentId: task.parent ?? 0 });
  }, [allTasks]);

  const handleDialogSave = useCallback(async (formData) => {
    const state = dialogState;
    setDialogState(null);
    if (state?.mode === "add") {
      // _fromDialog 플래그로 intercept 우회 → handleAddTask가 서버 저장 처리
      const tempId = Date.now();
      const newTask = { ...formData, id: tempId, parent: state.parentId ?? 0, _fromDialog: true };
      ganttApi.current?.exec("add-task", { task: newTask, id: tempId });
    } else if (state?.mode === "edit") {
      const id = state.task.id;
      const updatedTask = { ...state.task, ...formData };
      ganttApi.current?.exec("update-task", { id, task: updatedTask });
      await handleUpdateTask({ id, task: updatedTask });
      setAllTasks(prev => sortByTreeId(prev.map(t => t.id === id ? { ...t, ...updatedTask } : t)));
      setTasks(prev => prev.map(t => t.id === id ? { ...t, ...updatedTask } : t));

      // ── FS Cascade: 날짜가 바뀐 경우 후속 태스크 이동 ────────────────────
      // Start 또는 End 변경 시 cascade (SS/SF는 Start 기준, FS/FF는 End 기준)
      const startChanged = state.task.start?.getTime() !== updatedTask.start?.getTime();
      const endChanged   = state.task.end?.getTime()   !== updatedTask.end?.getTime();
      if (startChanged || endChanged) {
        const snapshot = allTasks.map(t => t.id === id ? updatedTask : t);
        cascadeFS(updatedTask, state.task, allLinks, snapshot, async (succId, newStart, newEnd, newDuration) => {
          const succ = snapshot.find(t => t.id === succId);
          if (!succ) return;
          const shifted = { ...succ, start: newStart, end: newEnd, duration: newDuration };
          ganttApi.current?.exec("update-task", { id: succId, task: shifted });
          await handleUpdateTask({ id: succId, task: shifted });
          setAllTasks(prev => prev.map(t => t.id === succId ? { ...t, ...shifted } : t));
          setTasks(prev => prev.map(t => t.id === succId ? { ...t, ...shifted } : t));
        });
      }
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [dialogState, allLinks, allTasks]);

  const handleUpdateTask = useCallback(async ({ id, task }) => {
    try {
      await fetch(`${apiBase}/gantt/projects/${projectId}/task/${id}`, {
        method: "PUT", headers: { "Content-Type": "application/json" }, credentials: "include",
        body: JSON.stringify(toApiTask(task, projectId)),
      });
    } catch (e) { console.error("[Gantt] update-task:", e); }
  }, [projectId, apiBase]);

  const handleDeleteTask = useCallback(async ({ id }) => {
    try {
      await fetch(`${apiBase}/gantt/projects/${projectId}/task/${id}/subtree`, {
        method: "DELETE", credentials: "include",
      });
      // 삭제된 태스크 및 서브트리를 allTasks에서 제거 → seqMap 재계산
      const removeSubtree = (list) => {
        const toRemove = new Set([id]);
        let changed = true;
        while (changed) {
          changed = false;
          list.forEach(t => {
            if (!toRemove.has(t.id) && toRemove.has(t.parent)) {
              toRemove.add(t.id); changed = true;
            }
          });
        }
        return list.filter(t => !toRemove.has(t.id));
      };
      setAllTasks(prev => removeSubtree(prev));
      setTasks(prev => removeSubtree(prev));
    } catch (e) { console.error("[Gantt] delete-task:", e); }
  }, [projectId, apiBase]);

  const handleAddLink = useCallback(async ({ link }) => {
    try {
      const res = await fetch(`${apiBase}/gantt/projects/${projectId}/link`, {
        method: "POST", headers: { "Content-Type": "application/json" }, credentials: "include",
        body: JSON.stringify({ source: link.source, target: link.target, type: String(link.type || "e2s") }),
      });
      const data = await res.json();
      if (data.tid && ganttApi.current) {
        ganttApi.current.exec("update-link", { id: link.id, link: { ...link, id: data.tid } });
      }
    } catch (e) { console.error("[Gantt] add-link:", e); }
  }, [projectId, apiBase]);

  const handleDeleteLink = useCallback(async ({ id }) => {
    try {
      await fetch(`${apiBase}/gantt/projects/${projectId}/link/${id}`, {
        method: "DELETE", credentials: "include",
      });
    } catch (e) { console.error("[Gantt] delete-link:", e); }
  }, [projectId, apiBase]);

  // ── Export ────────────────────────────────────────────────────────────────
  const handleExportCsv = () => {
    const rows = [["ID","WBS","Task","Trade","Start","End","Duration","Progress","Owner"]];
    tasks.forEach((t) => rows.push([
      t.id, t.wbs_code||"", t.text||"", t.trade_name||"",
      t.start ? formatDate(t.start) : "", t.end ? formatDate(t.end) : "",
      t.duration||0, Math.round((t.progress||0)*100)+"%", t.assigned_to_name||"",
    ]));
    const csv = rows.map((r) => r.map((v) => `"${String(v).replace(/"/g,'""')}"`).join(",")).join("\n");
    const blob = new Blob(["\uFEFF"+csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const a = Object.assign(document.createElement("a"), { href: url, download: "baseline_schedule.csv" });
    document.body.appendChild(a); a.click();
    setTimeout(() => { document.body.removeChild(a); URL.revokeObjectURL(url); }, 500);
  };

  const handleExportPdf = () => window.print();
  const handleExportXml = () => { window.location.href = `${apiBase}/gantt/projects/${projectId}/export-xml`; };
  const handleImportXml = () => { if (importXmlUrl) window.location.href = importXmlUrl; };

  // ── Today 마커 ───────────────────────────────────────────────────────────
  const markers = useMemo(() => [{ id: "today", start: new Date(), text: "Today", css: "aci-today-marker" }], []);
  const scales  = SCALE_CONFIGS[currentScale];

  if (loading) return (
    <div className="d-flex justify-content-center align-items-center" style={{ height: "60vh" }}>
      <div className="text-center">
        <div className="spinner-border text-primary mb-3" role="status" />
        <div className="text-muted">Loading schedule…</div>
      </div>
    </div>
  );

  if (error) return (
    <div className="alert alert-danger mx-3">
      <i className="bi bi-exclamation-triangle me-2" />Failed to load schedule: {error}
    </div>
  );

  return (
    <>
      <GanttToolbar
        mode="baseline"
        onSearch={handleSearch}
        onZoomIn={handleZoomIn}
        onZoomOut={handleZoomOut}
        onFitToScreen={handleFit}
        onToday={handleToday}
        currentScale={currentScale}
        onScaleChange={setCurrentScale}
        scales={["day","week","month","quarter","year"]}
        onLookahead={handleLookahead}
        activeLookahead={activeLookahead}
        showLinks={showLinks}
        onToggleLinks={() => setShowLinks((v) => !v)}
        showTrade={showTrade}
        onToggleTrade={() => setShowTrade((v) => !v)}
        showCriticalPath={showCriticalPath}
        onToggleCriticalPath={() => setShowCriticalPath((v) => !v)}
        colorMode={colorMode}
        onColorModeChange={setColorMode}
        onDateRangeApply={handleDateRangeApply}
        onDateRangeReset={handleDateRangeReset}
        onFreezeBaseline={() => setShowFreezeModal(true)}
        onImportXml={handleImportXml}
        onExportXml={handleExportXml}
        onExportCsv={handleExportCsv}
        onExportPdf={handleExportPdf}
      />

      {/* ── 잠금 배너 ── */}
      {!scheduleEditable && (
        <div style={{
          background: "#fff3cd", borderBottom: "1px solid #ffc107",
          padding: "8px 16px", display: "flex", alignItems: "center", gap: 12,
        }}>
          <i className="bi bi-lock-fill text-warning" />
          <span style={{ fontSize: 13 }}>
            <strong>Schedule Locked</strong> — Baseline이 제출/승인된 상태입니다.
            수정하려면 오너의 승인을 받아 Revision을 시작하세요.
          </span>
          <button
            className="btn btn-sm btn-warning ms-auto"
            onClick={() => setShowRevisionModal(true)}
          >
            <i className="bi bi-pencil-square me-1" />Start Revision
          </button>
        </div>
      )}

      <div ref={ganttContainer} className="aci-gantt-container" style={{ width: "100%", height: `calc(100vh - ${scheduleEditable ? 172 : 210}px)` }}>
        <Gantt
          key={`baseline-${showTrade}`}
          api={ganttApi}
          tasks={coloredTasks}
          links={showLinks ? links : []}
          scales={scales}
          columns={columns}
          markers={markers}
          start={ganttStart}
          end={ganttEnd}
          cellWidth={currentScale === "day" ? 38 : currentScale === "week" ? 50 : 80}
          cellHeight={28}
          cellBorders=""
          readonly={!scheduleEditable}
          editorShape={[
            { key: "text",     type: "text",    label: "Task Name",      config: { placeholder: "Enter task name", focus: true } },
            { key: "type",     type: "select",  label: "Type",           options: [{ id: "task", label: "Task" }, { id: "summary", label: "Summary" }, { id: "milestone", label: "Milestone" }] },
            { key: "start",    type: "date",    label: "Start Date" },
            { key: "end",      type: "date",    label: "End Date" },
            { key: "duration", type: "counter", label: "Duration (days)", config: { min: 1, max: 9999 } },
            { key: "progress", type: "slider",  label: "Progress" },
          ]}
          onAddTask={handleAddTask}
          onUpdateTask={handleUpdateTask}
          onDeleteTask={handleDeleteTask}
          onAddLink={handleAddLink}
          onDeleteLink={handleDeleteLink}
        />
      </div>

      {showRevisionModal && (
        <RevisionModal
          projectId={projectId}
          apiBase={apiBase}
          onClose={() => setShowRevisionModal(false)}
          onStarted={() => { setShowRevisionModal(false); setScheduleEditable(true); }}
        />
      )}

      {dialogState && (
        <TaskDialog
          task={dialogState.task}
          parentId={dialogState.parentId}
          trades={metaTrades}
          employees={metaEmployees}
          onSave={handleDialogSave}
          onClose={() => setDialogState(null)}
        />
      )}

      {showFreezeModal && (
        <FreezeModal projectId={projectId} onClose={() => setShowFreezeModal(false)} />
      )}
    </>
  );
}

// ── Freeze Baseline 모달 ─────────────────────────────────────────────────────
function FreezeModal({ projectId, onClose }) {
  const [title, setTitle]       = useState("");
  const [description, setDescription] = useState("");

  const handleSubmit = (e) => {
    e.preventDefault();
    const form = document.createElement("form");
    form.method = "post";
    form.action = `/Schedule/${projectId}?handler=Freeze`;
    const add = (n, v) => {
      const i = document.createElement("input");
      i.type = "hidden"; i.name = n; i.value = v; form.appendChild(i);
    };
    add("projectId", projectId);
    add("title", title);
    add("description", description);
    const tok = document.querySelector('input[name="__RequestVerificationToken"]');
    if (tok) add("__RequestVerificationToken", tok.value);
    document.body.appendChild(form);
    form.submit();
  };

  return (
    <div className="modal show d-block" tabIndex="-1" style={{ background: "rgba(0,0,0,0.5)" }}>
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title"><i className="bi bi-snow me-2 text-primary" />Freeze Baseline</h5>
            <button type="button" className="btn-close" onClick={onClose} />
          </div>
          <form onSubmit={handleSubmit}>
            <div className="modal-body">
              <div className="alert alert-info small mb-3">
                <i className="bi bi-info-circle me-1" />
                Creates an immutable snapshot for owner approval.
              </div>
              <div className="mb-3">
                <label className="form-label">Baseline Title *</label>
                <input type="text" className="form-control" required
                  placeholder="e.g. Initial Baseline v1.0"
                  value={title} onChange={(e) => setTitle(e.target.value)} />
              </div>
              <div className="mb-3">
                <label className="form-label">Description</label>
                <textarea className="form-control" rows="2"
                  value={description} onChange={(e) => setDescription(e.target.value)} />
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={onClose}>Cancel</button>
              <button type="submit" className="btn btn-primary">
                <i className="bi bi-snow me-1" />Freeze
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

// ── Revision 시작 모달 ────────────────────────────────────────────────────────
function RevisionModal({ projectId, apiBase = "/api", onClose, onStarted }) {
  const [title, setTitle]       = useState("");
  const [description, setDescription] = useState("");
  const [loading, setLoading]   = useState(false);
  const [error, setError]       = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!title.trim()) return;
    setLoading(true);
    try {
      const res = await fetch(`${apiBase}/gantt/projects/${projectId}/start-revision`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        credentials: "include",
        body: JSON.stringify({ title: title.trim(), description: description || null }),
      });
      if (!res.ok) {
        const err = await res.json();
        throw new Error(err.message || "Failed to start revision");
      }
      onStarted();
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="modal show d-block" tabIndex="-1" style={{ background: "rgba(0,0,0,0.5)" }}>
      <div className="modal-dialog">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">
              <i className="bi bi-pencil-square me-2 text-warning" />Start Schedule Revision
            </h5>
            <button type="button" className="btn-close" onClick={onClose} />
          </div>
          <form onSubmit={handleSubmit}>
            <div className="modal-body">
              <div className="alert alert-warning small mb-3">
                <i className="bi bi-exclamation-triangle me-1" />
                오너 승인 하에 새 Revision을 시작합니다. 이전 Baseline은 보존됩니다.
              </div>
              {error && <div className="alert alert-danger small">{error}</div>}
              <div className="mb-3">
                <label className="form-label">Revision Title *</label>
                <input type="text" className="form-control" required
                  placeholder="예: Rev.2 — Owner Requested Change"
                  value={title} onChange={e => setTitle(e.target.value)} />
              </div>
              <div className="mb-3">
                <label className="form-label">Description</label>
                <textarea className="form-control" rows={3}
                  placeholder="변경 사유 또는 오너 지시사항..."
                  value={description} onChange={e => setDescription(e.target.value)} />
              </div>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={onClose}>Cancel</button>
              <button type="submit" className="btn btn-warning" disabled={loading}>
                {loading
                  ? <><span className="spinner-border spinner-border-sm me-1" />Starting…</>
                  : <><i className="bi bi-pencil-square me-1" />Start Revision</>}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
}

// ── DTO 변환 ──────────────────────────────────────────────────────────────────
function toApiTask(task, projectId) {
  return {
    id: task.id,
    text: task.text,
    start_date: formatDate(task.start),
    end_date: formatDate(task.end),
    duration: task.duration || 1,
    progress: task.progress || 0,
    parent: task.parent || 0,
    type: task.type === "summary" ? "project" : task.type || "task",
    open: task.open !== false,
    trade_id: task.trade_id,
    color: task.trade_color,
    wbs_code: task.wbs_code,
    notes: task.notes,
  };
}
