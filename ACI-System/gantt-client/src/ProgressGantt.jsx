import { useState, useEffect, useRef, useCallback, useMemo } from "react";
import { Gantt } from "wx-react-gantt";
import "wx-react-gantt/dist/gantt.css";
import GanttToolbar from "./components/GanttToolbar";
import { toSvarTask, toSvarLink, formatDate, displayDate } from "./utils/dateUtils";
import "./styles/aci-gantt.css";

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

// ── 태스크 색상 (Current Schedule) ──────────────────────────────────────────
function getProgressTaskColor(task, colorMode, criticalIds) {
  // Critical Path 강조
  if (criticalIds?.has(task.id)) return "#ef4444";

  // Trade color 모드
  if (colorMode === "trade" && task.trade_color) return task.trade_color;

  // Status 기반 색상
  if (task.working_status === "Removed") return "#adb5bd";
  if (!task.baseline_id) return "#0d6efd";             // New task (파란색)
  if ((task.days_shifted || 0) > 5) return "#dc3545";  // Severely delayed (빨간)
  if ((task.days_shifted || 0) > 0) return "#ffc107";  // Delayed (노란)
  if ((task.progress || 0) >= 1) return "#198754";     // Complete (초록)
  if ((task.progress || 0) > 0) return "#0dcaf0";      // In Progress (시안)
  return null; // 기본 SVAR 색
}

// ── Critical Path 계산 (Forward/Backward pass) ────────────────────────────────
function calcCriticalPath(tasks, links) {
  if (!tasks.length) return new Set();

  const taskMap = new Map(tasks.map((t) => [t.id, { ...t }]));
  const dayMs = 24 * 3600 * 1000;

  // Forward pass: earliest start/finish
  tasks.forEach((t) => {
    const task = taskMap.get(t.id);
    task.ES = task.start ? task.start.getTime() : 0;
    task.EF = task.end   ? task.end.getTime()   : task.ES + (task.duration || 1) * dayMs;
  });

  // FS 링크 기반 forward propagation
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

  // Total float ≈ 0 → critical
  const TOLERANCE_MS = dayMs;
  const criticalIds = new Set();
  taskMap.forEach((t, id) => {
    if (t.type === "summary") return;
    if (Math.abs(t.LS - t.ES) <= TOLERANCE_MS) criticalIds.add(id);
  });
  return criticalIds;
}

// ── Current Schedule 컬럼 (SVAR template = textNode, plain text only) ────────
const PROGRESS_COLUMNS = [
  {
    id: "wbs",
    header: "ID",
    width: 55,
    align: "center",
    template: (t) => t.wbs_code || String(t.id),
  },
  {
    id: "text",
    header: "Task",
    flexgrow: 1,
    minWidth: 200,
    tree: true,
    template: (t) => {
      const prefix = t.type === "summary" ? "▸ " : t.type === "milestone" ? "◆ " : "  ";
      const removed = t.working_status === "Removed" ? " [removed]" : "";
      return prefix + (t.text || "") + removed;
    },
  },
  {
    id: "trade_name",
    header: "Trade",
    width: 85,
    template: (t) => t.trade_name || "",
  },
  {
    id: "start",
    header: "Start",
    width: 75,
    align: "center",
    template: (t) => t.start ? displayDate(t.start) : "",
  },
  {
    id: "duration",
    header: "Dur",
    width: 48,
    align: "center",
    template: (t) => (t.duration || "") + "d",
  },
  {
    id: "end",
    header: "End",
    width: 75,
    align: "center",
    template: (t) => t.end ? displayDate(t.end) : "",
  },
  {
    id: "days_shifted",
    header: "Delay",
    width: 55,
    align: "center",
    template: (t) => {
      if (t.days_shifted == null) return "–";
      if (t.days_shifted === 0) return "✓";
      if (t.days_shifted > 0) return "+" + t.days_shifted + "d";
      return t.days_shifted + "d";
    },
  },
  {
    id: "progress",
    header: "%",
    width: 48,
    align: "center",
    template: (t) => Math.round((t.progress || 0) * 100) + "%",
  },
  {
    id: "responsible",
    header: "Owner",
    width: 55,
    align: "center",
    template: (t) => {
      if (!t.assigned_to_name) return "";
      const parts = t.assigned_to_name.split(" ").filter(Boolean);
      return parts.length >= 2
        ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
        : (parts[0] || "").substring(0, 2).toUpperCase();
    },
  },
];

// ── ProgressGantt 컴포넌트 ───────────────────────────────────────────────────
export default function ProgressGantt({ projectId, isInitialized }) {
  const [tasks, setTasks]           = useState([]);
  const [allTasks, setAllTasks]     = useState([]);
  const [links, setLinks]           = useState([]);
  const [loading, setLoading]       = useState(true);
  const [error, setError]           = useState(null);
  const [currentScale, setCurrentScale] = useState("week");
  const [showLinks, setShowLinks]   = useState(false);
  const [showBaseline, setShowBaseline] = useState(true);
  const [showDelayedOnly, setShowDelayedOnly] = useState(false);
  const [pendingChanges, setPendingChanges] = useState(new Map());
  const [isSaving, setIsSaving]     = useState(false);
  const [draftTitle, setDraftTitle] = useState("");
  const [ganttStart, setGanttStart] = useState(null);
  const [ganttEnd, setGanttEnd]     = useState(null);

  // ── Outbuild features ─────────────────────────────────────────────────────
  const [colorMode, setColorMode]   = useState("status"); // "status" | "trade"
  const [showCriticalPath, setShowCriticalPath] = useState(false);
  const [criticalIds, setCriticalIds] = useState(new Set());
  const [activeLookahead, setActiveLookahead] = useState(null); // weeks | null

  const ganttApi = useRef(null);
  const antiForgeryToken = useRef(
    document.querySelector('input[name="__RequestVerificationToken"]')?.value || ""
  );

  // ── 데이터 로드 ──────────────────────────────────────────────────────────
  const loadTasks = useCallback(() => {
    setLoading(true);
    fetch(`/Progress/${projectId}?handler=TasksJson&projectId=${projectId}`, { credentials: "include" })
      .then((r) => {
        if (!r.ok) throw new Error(`HTTP ${r.status}`);
        return r.json();
      })
      .then((d) => {
        const svarTasks = (d.data || []).map(toSvarTask);
        const svarLinks = (d.links || []).map(toSvarLink);
        setAllTasks(svarTasks);
        setLinks(svarLinks);
        setTasks(applyFilters(svarTasks, "", showDelayedOnly));
        setLoading(false);
      })
      .catch((e) => {
        setError(e.message);
        setLoading(false);
      });
  }, [projectId]);

  useEffect(() => {
    if (isInitialized) loadTasks();
    else setLoading(false);
  }, [isInitialized, loadTasks]);

  // ── Critical Path 계산 ───────────────────────────────────────────────────
  useEffect(() => {
    if (showCriticalPath && allTasks.length) {
      setCriticalIds(calcCriticalPath(allTasks, links));
    } else {
      setCriticalIds(new Set());
    }
  }, [showCriticalPath, allTasks, links]);

  // ── CSS 색상 인젝션 ──────────────────────────────────────────────────────
  useEffect(() => {
    const styleId = "aci-progress-task-colors";
    let el = document.getElementById(styleId);
    if (!el) {
      el = document.createElement("style");
      el.id = styleId;
      document.head.appendChild(el);
    }
    const rules = tasks
      .map((t) => {
        const c = getProgressTaskColor(t, colorMode, criticalIds);
        if (!c) return "";
        return `.wx-bar[data-id="${t.id}"] { background: ${c} !important; }`;
      })
      .filter(Boolean)
      .join("\n");
    el.textContent = rules;
  }, [tasks, colorMode, criticalIds]);

  // ── 필터 ─────────────────────────────────────────────────────────────────
  function applyFilters(taskList, searchQ, delayedOnly) {
    let filtered = taskList;
    if (delayedOnly) {
      const delayedIds = new Set();
      taskList.forEach((t) => {
        if ((t.days_shifted || 0) > 0 || !t.baseline_id) {
          delayedIds.add(t.id);
          let p = t.parent;
          while (p && p !== 0) {
            delayedIds.add(p);
            p = taskList.find((x) => x.id === p)?.parent;
          }
        }
      });
      filtered = filtered.filter((t) => delayedIds.has(t.id));
    }
    if (searchQ) {
      const lower = searchQ.toLowerCase();
      const matchIds = new Set();
      filtered.forEach((t) => {
        if (t.text?.toLowerCase().includes(lower) || t.wbs_code?.toLowerCase().includes(lower)) {
          matchIds.add(t.id);
          let p = t.parent;
          while (p && p !== 0) { matchIds.add(p); p = filtered.find((x) => x.id === p)?.parent; }
        }
      });
      filtered = filtered.filter((t) => matchIds.has(t.id));
    }
    return filtered;
  }

  const handleSearch = useCallback((q) => {
    setTasks(applyFilters(allTasks, q, showDelayedOnly));
  }, [allTasks, showDelayedOnly]);

  const handleDelayedOnly = (v) => {
    setShowDelayedOnly(v);
    setTasks(applyFilters(allTasks, "", v));
  };

  // ── Lookahead 필터 (Outbuild 핵심) ───────────────────────────────────────
  const handleLookahead = useCallback((weeks) => {
    setActiveLookahead(weeks);
    if (weeks === null) {
      setTasks(applyFilters(allTasks, "", showDelayedOnly));
      setGanttStart(null);
      setGanttEnd(null);
      return;
    }
    const now = new Date();
    const end = new Date(now.getTime() + weeks * 7 * 24 * 3600 * 1000);
    setGanttStart(new Date(now.getTime() - 2 * 24 * 3600 * 1000));
    setGanttEnd(end);

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
  }, [allTasks, showDelayedOnly]);

  // ── 줌 ──────────────────────────────────────────────────────────────────
  const handleZoomIn = () => {
    const idx = SCALE_ORDER.indexOf(currentScale);
    if (idx > 0) setCurrentScale(SCALE_ORDER[idx - 1]);
  };
  const handleZoomOut = () => {
    const idx = SCALE_ORDER.indexOf(currentScale);
    if (idx < SCALE_ORDER.length - 1) setCurrentScale(SCALE_ORDER[idx + 1]);
  };

  // ── Fit ─────────────────────────────────────────────────────────────────
  const handleFit = () => {
    if (!tasks.length) return;
    const all = [
      ...tasks.map((t) => t.start),
      ...tasks.map((t) => t.end || t.start),
      ...tasks.map((t) => t.base_start),
      ...tasks.map((t) => t.base_end),
    ].filter(Boolean);
    if (!all.length) return;
    const pad = 7 * 24 * 3600 * 1000;
    setGanttStart(new Date(Math.min(...all.map((d) => d.getTime())) - pad));
    setGanttEnd(new Date(Math.max(...all.map((d) => d.getTime())) + pad));
    setActiveLookahead(null);
  };

  const handleToday = () => {
    if (ganttApi.current?.exec) {
      ganttApi.current.exec("scroll-to", { date: new Date() });
    }
  };

  const handleDateRangeApply = (from, to) => {
    if (from) setGanttStart(new Date(from));
    if (to)   setGanttEnd(new Date(to));
    setActiveLookahead(null);
  };
  const handleDateRangeReset = () => {
    setGanttStart(null);
    setGanttEnd(null);
    setActiveLookahead(null);
  };

  // ── 태스크 업데이트 (변경 추적) ──────────────────────────────────────────
  const handleUpdateTask = useCallback((ev) => {
    const { id, task } = ev;
    if (task.working_status === "Removed") return;
    if (task.type === "summary") return;

    setPendingChanges((prev) => {
      const next = new Map(prev);
      next.set(id, {
        taskId: id,
        startDate: formatDate(task.start),
        endDate: formatDate(task.end),
        duration: task.duration || 1,
        progress: task.progress || 0,
      });
      return next;
    });

    // Cascade FS successors
    const fwdLinks = links.filter((l) => l.source === id && l.type === "e2s");
    fwdLinks.forEach((link) =>
      cascadeSuccessor(link, task, links, setTasks, setPendingChanges)
    );
  }, [links]);

  // ── 저장 ─────────────────────────────────────────────────────────────────
  const handleSave = async () => {
    if (pendingChanges.size === 0) return;
    setIsSaving(true);
    try {
      const payload = Array.from(pendingChanges.values());
      const res = await fetch(`/Progress/${projectId}?handler=SaveChanges&projectId=${projectId}`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "RequestVerificationToken": antiForgeryToken.current,
        },
        credentials: "include",
        body: JSON.stringify(payload),
      });
      const result = await res.json();
      if (result.success) {
        const updatedTasks = allTasks.map((t) => {
          const saved = result.results?.find((r) => r.taskId === t.id);
          return saved ? { ...t, days_shifted: saved.daysShifted } : t;
        });
        setAllTasks(updatedTasks);
        setTasks(applyFilters(updatedTasks, "", showDelayedOnly));
        setPendingChanges(new Map());
        if (result.revisionTitle) setDraftTitle(result.revisionTitle);
      } else {
        alert("저장 실패: " + (result.error || "알 수 없는 오류"));
      }
    } catch (e) {
      alert("저장 중 오류가 발생했습니다.");
    } finally {
      setIsSaving(false);
    }
  };

  // ── 취소 ─────────────────────────────────────────────────────────────────
  const handleDiscard = () => {
    if (!confirm(`${pendingChanges.size}개의 변경사항을 취소하시겠습니까?`)) return;
    setPendingChanges(new Map());
    loadTasks();
  };

  // ── CSV 내보내기 ─────────────────────────────────────────────────────────
  const handleExportCsv = () => {
    const rows = [["ID", "WBS", "Task", "Trade", "Start", "End", "Duration", "Progress", "Delay"]];
    tasks.forEach((t) => {
      rows.push([
        t.id, t.wbs_code || "", t.text || "", t.trade_name || "",
        t.start ? formatDate(t.start) : "",
        t.end ? formatDate(t.end) : "",
        t.duration || 0,
        Math.round((t.progress || 0) * 100) + "%",
        t.days_shifted != null ? (t.days_shifted >= 0 ? "+" + t.days_shifted : t.days_shifted) + "d" : "",
      ]);
    });
    const csv = rows.map((r) => r.map((v) => `"${String(v).replace(/"/g, '""')}"`).join(",")).join("\n");
    const blob = new Blob(["\uFEFF" + csv], { type: "text/csv;charset=utf-8;" });
    const url = URL.createObjectURL(blob);
    const a = Object.assign(document.createElement("a"), { href: url, download: "current_schedule.csv" });
    document.body.appendChild(a); a.click();
    setTimeout(() => { document.body.removeChild(a); URL.revokeObjectURL(url); }, 500);
  };

  const handleExportPdf = () => window.print();

  // ── beforeunload 경고 ────────────────────────────────────────────────────
  useEffect(() => {
    const handler = (e) => {
      if (pendingChanges.size > 0) { e.preventDefault(); e.returnValue = ""; }
    };
    window.addEventListener("beforeunload", handler);
    return () => window.removeEventListener("beforeunload", handler);
  }, [pendingChanges]);

  // ── Baseline overlay 토글 (base_start/base_end null 처리) ──────────────
  const displayTasks = showBaseline
    ? tasks
    : tasks.map((t) => ({ ...t, base_start: null, base_end: null }));

  const markers = useMemo(
    () => [{ id: "today", start: new Date(), text: "Today", css: "aci-today-marker" }],
    []
  );

  if (!isInitialized) {
    return (
      <div className="card border-0 shadow-sm mx-auto mt-5" style={{ maxWidth: 520 }}>
        <div className="card-body text-center py-5">
          <i className="bi bi-bar-chart-line fs-1 text-success mb-3 d-block" />
          <h4>Current Schedule Not Initialized</h4>
          <p className="text-muted">
            The Current Schedule tracks real-world changes, delays, and actual progress
            against the frozen Baseline.
          </p>
          <form method="post" action={`/Progress/${projectId}?handler=Initialize`}>
            <input type="hidden" name="projectId" value={projectId} />
            <input type="hidden" name="__RequestVerificationToken" value={antiForgeryToken.current} />
            <button type="submit" className="btn btn-success btn-lg mt-2">
              <i className="bi bi-arrow-right-circle me-2" />Initialize from Baseline
            </button>
          </form>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: "60vh" }}>
        <div className="text-center">
          <div className="spinner-border text-success mb-3" role="status" />
          <div className="text-muted">Loading current schedule…</div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="alert alert-danger mx-3">
        <i className="bi bi-exclamation-triangle me-2" />
        Failed to load current schedule: {error}
      </div>
    );
  }

  return (
    <>
      <GanttToolbar
        mode="progress"
        onSearch={handleSearch}
        onZoomIn={handleZoomIn}
        onZoomOut={handleZoomOut}
        onFitToScreen={handleFit}
        onToday={handleToday}
        currentScale={currentScale}
        onScaleChange={setCurrentScale}
        scales={["day", "week", "month", "quarter", "year"]}
        onLookahead={handleLookahead}
        activeLookahead={activeLookahead}
        showLinks={showLinks}
        onToggleLinks={() => setShowLinks(!showLinks)}
        showCriticalPath={showCriticalPath}
        onToggleCriticalPath={() => setShowCriticalPath((v) => !v)}
        colorMode={colorMode}
        onColorModeChange={setColorMode}
        onDateRangeApply={handleDateRangeApply}
        onDateRangeReset={handleDateRangeReset}
        showBaselineOverlay={showBaseline}
        onToggleBaselineOverlay={setShowBaseline}
        showDelayedOnly={showDelayedOnly}
        onToggleDelayedOnly={handleDelayedOnly}
        onExportCsv={handleExportCsv}
        onExportPdf={handleExportPdf}
        unsavedCount={pendingChanges.size}
        onSave={handleSave}
        onDiscard={handleDiscard}
        isSaving={isSaving}
        draftTitle={draftTitle}
      />

      <div style={{ width: "100%", height: "calc(100vh - 190px)" }}>
        <Gantt
          api={ganttApi}
          tasks={displayTasks}
          links={showLinks ? links : []}
          scales={SCALE_CONFIGS[currentScale]}
          columns={PROGRESS_COLUMNS}
          markers={markers}
          start={ganttStart}
          end={ganttEnd}
          cellWidth={currentScale === "day" ? 38 : currentScale === "week" ? 50 : 80}
          cellHeight={32}
          readonly={false}
          baselines={showBaseline}
          onUpdateTask={handleUpdateTask}
        />
      </div>
    </>
  );
}

// ── Cascade FS successors ────────────────────────────────────────────────────
function cascadeSuccessor(link, movedTask, allLinks, setTasks, setPendingChanges) {
  if (link.type !== "e2s") return; // FS 링크만 처리
  const lag = link.lag || 0;
  const newStart = new Date(movedTask.end.getTime() + lag * 24 * 3600 * 1000);
  setTasks((prev) =>
    prev.map((t) => {
      if (t.id !== link.target) return t;
      const dur = t.duration || 1;
      const newEnd = new Date(newStart.getTime() + dur * 24 * 3600 * 1000);
      setPendingChanges((pc) => {
        const next = new Map(pc);
        next.set(t.id, {
          taskId: t.id,
          startDate: formatDate(newStart),
          endDate: formatDate(newEnd),
          duration: dur,
          progress: t.progress || 0,
        });
        return next;
      });
      // 재귀: 이 태스크의 FS 후속도 cascade
      allLinks.filter((l) => l.source === t.id && l.type === "e2s").forEach((nl) =>
        cascadeSuccessor(nl, { ...t, start: newStart, end: newEnd }, allLinks, setTasks, setPendingChanges)
      );
      return { ...t, start: newStart, end: newEnd };
    })
  );
}
