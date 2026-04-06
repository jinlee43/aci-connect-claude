import { useState, useEffect, useRef, useCallback, useMemo } from "react";
import { Gantt } from "wx-react-gantt";
import "wx-react-gantt/dist/gantt.css";
import GanttToolbar from "./components/GanttToolbar";
import { toSvarTask, toSvarLink, formatDate, displayDate } from "./utils/dateUtils";
import "./styles/aci-gantt.css";

// ── 스케일 설정 ──────────────────────────────────────────────────────────────
const SCALE_CONFIGS = {
  day: [
    { unit: "week",  step: 1, format: "'W'w MMM yyyy" },
    { unit: "day",   step: 1, format: "d EEE" },
  ],
  week: [
    { unit: "month", step: 1, format: "MMMM yyyy" },
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

// ── 컬럼 설정 ────────────────────────────────────────────────────────────────
function buildColumns() {
  return [
    {
      id: "wbs",
      header: "ID",
      width: 45,
      align: "center",
      template: (t) =>
        t.wbs_code
          ? `<span style="font-size:11px;color:#6b7280">${t.wbs_code}</span>`
          : `<span style="font-size:11px;color:#9ca3af">${t.id}</span>`,
    },
    {
      id: "text",
      header: "Task",
      flexgrow: 1,
      minWidth: 180,
      tree: true,
      template: (t) => {
        const prefix = t.type === "summary" ? "▸ " : t.type === "milestone" ? "⬥ " : "";
        if (t.type === "milestone") {
          return `<span title="${t.text}" style="font-weight:600;color:#d97706">${prefix}${t.text}</span>`;
        }
        const lvl = t.$level ?? 0;
        let style =
          lvl === 0 ? "font-weight:700;font-size:13px;color:#111827"
          : lvl === 1 ? "color:#374151"
          : "color:#6b7280;font-size:11.5px";
        return `<span title="${t.text}" style="${style}">${prefix}${t.text}</span>`;
      },
    },
    {
      id: "actions",
      header: "",
      width: 65,
      align: "center",
      template: (t) => {
        if (t.type === "summary") {
          return `<span class="aci-act-btn" title="Add subtask" data-action="add" data-id="${t.id}">
            <i class="bi bi-plus" style="font-size:14px;color:#3b82f6"></i></span>`;
        }
        const isDone = (t.progress || 0) >= 1;
        const chk = isDone ? "bi-check-circle-fill" : "bi-circle";
        const chkColor = isDone ? "#16a34a" : "#d1d5db";
        return `
          <span class="aci-act-btn" title="Add after" data-action="add" data-id="${t.id}" data-after="1">
            <i class="bi bi-plus" style="font-size:14px;color:#3b82f6"></i></span>
          <span class="aci-act-btn" title="${isDone ? "Mark incomplete" : "Mark complete"}" data-action="done" data-id="${t.id}">
            <i class="bi ${chk}" style="font-size:12px;color:${chkColor}"></i></span>
          <span class="aci-act-btn" title="Delete" data-action="delete" data-id="${t.id}">
            <i class="bi bi-trash3" style="font-size:11px;color:#d1d5db"></i></span>`;
      },
    },
    {
      id: "trade_name",
      header: "Trade",
      width: 90,
      template: (t) => {
        if (!t.trade_name) return "";
        const c = t.trade_color || "#6b7280";
        return `<span style="font-size:11px;color:${c};overflow:hidden;text-overflow:ellipsis;white-space:nowrap;display:block" title="${t.trade_name}">${t.trade_name}</span>`;
      },
    },
    { id: "start", header: "Start", width: 85, align: "center" },
    {
      id: "duration",
      header: "Dur",
      width: 50,
      align: "center",
      template: (t) => `<span style="font-size:12px">${t.duration || ""}d</span>`,
    },
    { id: "end", header: "End", width: 85, align: "center",
      template: (t) => t.end ? `<span style="font-size:12px">${displayDate(t.end)}</span>` : "",
    },
    {
      id: "progress",
      header: "%",
      width: 55,
      align: "center",
      template: (t) => {
        const pct = Math.round((t.progress || 0) * 100);
        const c = pct >= 100 ? "#16a34a" : pct >= 50 ? "#059669" : pct > 0 ? "#d97706" : "#9ca3af";
        return `<span style="font-weight:700;color:${c};font-size:12px">${pct}%</span>`;
      },
    },
    {
      id: "responsible",
      header: "Owner",
      width: 55,
      align: "center",
      template: (t) => {
        if (!t.assigned_to_name) return "";
        const parts = t.assigned_to_name.split(" ").filter(Boolean);
        const initials = parts.length >= 2
          ? (parts[0][0] + parts[parts.length - 1][0]).toUpperCase()
          : (parts[0] || "").substring(0, 2).toUpperCase();
        const palette = ["#3b82f6","#16a34a","#d97706","#9333ea","#dc2626","#0891b2","#db2777"];
        const ci = (t.assigned_to_id || 0) % palette.length;
        return `<span title="${t.assigned_to_name}"
          style="display:inline-flex;align-items:center;justify-content:center;
          width:26px;height:26px;border-radius:50%;background:${palette[ci]};
          color:#fff;font-size:10px;font-weight:700">${initials}</span>`;
      },
    },
  ];
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

  const ganttApi = useRef(null);
  const columns  = useMemo(() => buildColumns(), []);

  // ── 데이터 로드 ──────────────────────────────────────────────────────────
  useEffect(() => {
    setLoading(true);
    fetch(`${apiBase}/gantt/projects/${projectId}/data`, { credentials: "include" })
      .then((r) => { if (!r.ok) throw new Error(`HTTP ${r.status}`); return r.json(); })
      .then((d) => {
        const svarTasks = (d.data  || []).map(toSvarTask);
        const svarLinks = (d.links || []).map(toSvarLink);
        setAllTasks(svarTasks);
        setAllLinks(svarLinks);
        setTasks(svarTasks);
        setLinks(svarLinks);
        setLoading(false);
      })
      .catch((e) => { setError(e.message); setLoading(false); });
  }, [projectId, apiBase]);

  // ── Critical Path 계산 ───────────────────────────────────────────────────
  useEffect(() => {
    if (showCriticalPath && allTasks.length) {
      setCriticalIds(calcCriticalPath(allTasks, allLinks));
    } else {
      setCriticalIds(new Set());
    }
  }, [showCriticalPath, allTasks, allLinks]);

  // ── CSS 색상 인젝션 ──────────────────────────────────────────────────────
  useEffect(() => {
    const styleId = "aci-baseline-task-colors";
    let el = document.getElementById(styleId);
    if (!el) {
      el = document.createElement("style");
      el.id = styleId;
      document.head.appendChild(el);
    }
    const rules = tasks
      .map((t) => {
        const c = getTaskColor(t, colorMode, criticalIds);
        return `.wx-bar[data-id="${t.id}"] { background: ${c} !important; }`;
      })
      .join("\n");
    el.textContent = rules;
  }, [tasks, colorMode, criticalIds]);

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
        ganttApi.current.exec("update-task", { id, task: { ...task, id: data.tid } });
      }
    } catch (e) { console.error("[Gantt] add-task:", e); }
  }, [projectId, apiBase]);

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

      <div className="aci-gantt-container" style={{ width: "100%", height: "calc(100vh - 172px)" }}>
        <Gantt
          api={ganttApi}
          tasks={tasks}
          links={showLinks ? links : []}
          scales={scales}
          columns={columns}
          markers={markers}
          start={ganttStart}
          end={ganttEnd}
          cellWidth={currentScale === "day" ? 38 : currentScale === "week" ? 50 : 80}
          cellHeight={32}
          readonly={false}
          onAddTask={handleAddTask}
          onUpdateTask={handleUpdateTask}
          onDeleteTask={handleDeleteTask}
          onAddLink={handleAddLink}
          onDeleteLink={handleDeleteLink}
        />
      </div>

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
