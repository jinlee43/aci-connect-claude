import { useState, useEffect, useRef, useMemo, useCallback } from "react";
import { Gantt } from "wx-react-gantt";
import "wx-react-gantt/dist/gantt.css";
import { displayDate } from "./utils/dateUtils";
import "./styles/aci-gantt.css";

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
};

// ── "yyyy-MM-dd" → Date ───────────────────────────────────────────────────────
function parseDate(s) {
  if (!s) return null;
  const d = new Date(s + "T00:00:00");
  return isNaN(d.getTime()) ? null : d;
}

// ── 상태별 바 색상 ────────────────────────────────────────────────────────────
function getCompColor(task) {
  if (task.working_status === "Removed" || task.is_removed) return "#adb5bd";
  if (task.is_new && !task.baseline_task_id) return "#0dcaf0";
  if ((task.days_shifted || 0) > 0) return "#ffc107";
  if ((task.days_shifted || 0) < 0) return "#0d6efd"; // 조기 완료
  return "#198754"; // 정상
}

// ── 작업 태스크 → SVAR 태스크 변환 ────────────────────────────────────────────
function toSvarCompTask(w, baselineMap) {
  const base = baselineMap[w.baseline_task_id] || {};
  return {
    id:           w.id,
    text:         w.text || "",
    start:        parseDate(w.start_date),
    end:          parseDate(w.end_date),
    duration:     w.duration || 1,
    progress:     w.progress || 0,
    parent:       w.parent || 0,
    type:         w.type || "task",
    open:         true,
    color:        getCompColor(w),
    // SVAR 베이스라인 오버레이
    base_start:   parseDate(base.start),
    base_end:     parseDate(base.end),
    // 커스텀 컬럼용 데이터
    wbs_code:         w.wbs_code,
    days_shifted:     w.days_shifted,
    is_new:           w.is_new,
    is_removed:       w.is_removed,
    working_status:   w.working_status,
    baseline_task_id: w.baseline_task_id,
  };
}

// ── 컬럼 정의 ─────────────────────────────────────────────────────────────────
const COLUMNS = [
  {
    id: "wbs",
    header: "ID",
    width: 50,
    align: "center",
    template: (_, t) => t.wbs_code || String(t.id),
  },
  {
    id: "text",
    header: "Task",
    width: 220,
    tree: true,
    template: (v, t) => {
      const prefix  = t.type === "summary" ? "▸ " : t.type === "milestone" ? "◆ " : "  ";
      const removed = (t.is_removed || t.working_status === "Removed") ? " [removed]" : "";
      const label   = prefix + (v || "") + removed;
      return t.type === "summary" ? `<b>${label}</b>` : label;
    },
  },
  {
    id: "start",
    header: "Start",
    width: 75,
    align: "center",
    template: v => v ? displayDate(v) : "",
  },
  {
    id: "end",
    header: "Finish",
    width: 75,
    align: "center",
    template: v => v ? displayDate(v) : "",
  },
  {
    id: "days_shifted",
    header: "Delta",
    width: 72,
    align: "center",
    template: (v, t) => {
      if (t.is_new && !t.baseline_task_id) return '<span style="color:#0dcaf0">New</span>';
      if (t.is_removed || t.working_status === "Removed")
        return '<span style="color:#adb5bd">Removed</span>';
      if (v == null) return "—";
      if (v === 0)   return '<span style="color:#198754">✓</span>';
      if (v > 0)     return `<span style="color:#e0a800;font-weight:600">+${v}d</span>`;
      return `<span style="color:#0d6efd">-${Math.abs(v)}d</span>`;
    },
  },
  {
    id: "progress",
    header: "%",
    width: 45,
    align: "center",
    template: v => Math.round((v || 0) * 100) + "%",
  },
];

// ── ComparisonGantt ───────────────────────────────────────────────────────────
export default function ComparisonGantt({ projectId, isInitialized }) {
  const [loading, setLoading]     = useState(true);
  const [error, setError]         = useState(null);
  const [compData, setCompData]   = useState(null);
  const [baselineMap, setBaselineMap] = useState({});
  const [tasks, setTasks]         = useState([]);
  const [currentFrame, setCurrentFrame] = useState(0);
  const [isPlaying, setIsPlaying] = useState(false);
  const [speed, setSpeed]         = useState(1000);
  const [scale, setScale]         = useState("week");
  const [showBaseline, setShowBaseline] = useState(true);

  const ganttApi    = useRef(null);
  const playTimer   = useRef(null);
  const frameRef    = useRef(0); // interval 클로저용 최신 frame 추적

  // ── 데이터 로드 ──────────────────────────────────────────────────────────
  useEffect(() => {
    if (!isInitialized) { setLoading(false); return; }
    fetch(`?handler=ComparisonJson&projectId=${projectId}`, { credentials: "include" })
      .then(r => r.json())
      .then(data => {
        const bmap = {};
        (data.baselineTasks || []).forEach(b => {
          bmap[b.id] = { start: b.start_date, end: b.end_date };
        });
        setBaselineMap(bmap);
        setCompData(data);
        setTasks((data.workingTasks || []).map(w => toSvarCompTask(w, bmap)));

        // 슬라이더 초기 위치: 마지막 스냅샷
        const snaps = data.revisionSnapshots || [];
        if (snaps.length > 0) {
          const last = snaps.length - 1;
          setCurrentFrame(last);
          frameRef.current = last;
        }
        setLoading(false);
      })
      .catch(e => { setError(e.message); setLoading(false); });
  }, [projectId, isInitialized]);

  // ── 스냅샷 적용 ──────────────────────────────────────────────────────────
  const applyFrame = useCallback((frameIndex, data, bmap) => {
    const d = data || compData;
    const b = bmap || baselineMap;
    if (!d) return;
    const snaps = d.revisionSnapshots || [];
    if (frameIndex < 0 || frameIndex >= snaps.length) return;

    const snap      = snaps[frameIndex];
    const stateById = {};
    (snap.taskStates || []).forEach(ts => { stateById[ts.id] = ts; });

    const merged = (d.workingTasks || []).map(w => {
      const ts = stateById[w.id];
      if (!ts) return w;
      return {
        ...w,
        start_date: ts.start_date,
        end_date:   ts.end_date,
        duration:   ts.duration,
        progress:   ts.progress,
        text:       ts.text,
        is_removed: ts.is_removed,
      };
    });
    setTasks(merged.map(w => toSvarCompTask(w, b)));
  }, [compData, baselineMap]);

  // ── 재생 제어 ─────────────────────────────────────────────────────────────
  const stopPlay = useCallback(() => {
    clearInterval(playTimer.current);
    playTimer.current = null;
    setIsPlaying(false);
  }, []);

  const startPlay = useCallback(() => {
    if (!compData) return;
    const snaps = compData.revisionSnapshots || [];
    if (snaps.length === 0) return;

    // 끝에 있으면 처음부터 재시작
    let frame = frameRef.current >= snaps.length - 1 ? 0 : frameRef.current;
    frameRef.current = frame;
    setCurrentFrame(frame);
    applyFrame(frame);
    setIsPlaying(true);

    playTimer.current = setInterval(() => {
      frameRef.current++;
      if (frameRef.current >= snaps.length) {
        clearInterval(playTimer.current);
        playTimer.current = null;
        setIsPlaying(false);
        return;
      }
      setCurrentFrame(frameRef.current);
      applyFrame(frameRef.current);
    }, speed);
  }, [compData, speed, applyFrame]);

  // 언마운트 시 정리
  useEffect(() => () => clearInterval(playTimer.current), []);

  const stepFrame = useCallback((delta) => {
    if (!compData) return;
    const snaps = compData.revisionSnapshots || [];
    stopPlay();
    const f = Math.max(0, Math.min(snaps.length - 1, frameRef.current + delta));
    frameRef.current = f;
    setCurrentFrame(f);
    applyFrame(f);
  }, [compData, stopPlay, applyFrame]);

  const onSliderChange = useCallback((val) => {
    stopPlay();
    const f = parseInt(val, 10);
    frameRef.current = f;
    setCurrentFrame(f);
    applyFrame(f);
  }, [stopPlay, applyFrame]);

  // ── 베이스라인 오버레이 토글 ──────────────────────────────────────────────
  const displayTasks = useMemo(() =>
    showBaseline ? tasks : tasks.map(t => ({ ...t, base_start: null, base_end: null })),
    [tasks, showBaseline]
  );

  const markers = useMemo(
    () => [{ id: "today", start: new Date(), text: "Today", css: "aci-today-marker" }],
    []
  );

  const snapshots = compData?.revisionSnapshots || [];
  const curSnap   = snapshots[currentFrame];

  // ── 비초기화 화면 ─────────────────────────────────────────────────────────
  if (!isInitialized) {
    return (
      <div className="card border-0 shadow-sm mx-auto mt-5" style={{ maxWidth: 480 }}>
        <div className="card-body text-center py-5">
          <i className="bi bi-layout-split fs-1 text-primary mb-3 d-block" />
          <h4>Current Schedule Not Initialized</h4>
          <p className="text-muted">
            Initialize the Current Schedule first to enable Baseline vs Current comparison.
          </p>
          <a href={`/Progress/${projectId}`} className="btn btn-primary mt-2">
            <i className="bi bi-arrow-right-circle me-2" />Go to Current Schedule
          </a>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="d-flex justify-content-center align-items-center" style={{ height: "60vh" }}>
        <div className="text-center">
          <div className="spinner-border text-primary mb-3" role="status" />
          <div className="text-muted">Loading comparison data…</div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="alert alert-danger mx-3">
        <i className="bi bi-exclamation-triangle me-2" />
        Failed to load comparison: {error}
      </div>
    );
  }

  return (
    <>
      {/* ── 툴바 ──────────────────────────────────────────────────────────── */}
      <div className="d-flex gap-2 mb-2 px-1 align-items-center flex-wrap">

        {/* 스케일 */}
        <div className="btn-group btn-group-sm">
          {["day", "week", "month"].map(s => (
            <button
              key={s}
              className={`btn btn-outline-secondary${scale === s ? " active" : ""}`}
              onClick={() => setScale(s)}
            >
              {s.charAt(0).toUpperCase() + s.slice(1)}
            </button>
          ))}
        </div>

        {/* 베이스라인 토글 */}
        <div className="form-check form-switch ms-2 mb-0">
          <input
            className="form-check-input"
            type="checkbox"
            id="showBaselineChk"
            checked={showBaseline}
            onChange={e => setShowBaseline(e.target.checked)}
          />
          <label className="form-check-label small" htmlFor="showBaselineChk">
            Show Baseline
          </label>
        </div>

        {/* 범례 */}
        <span className="ms-auto d-flex gap-2 align-items-center flex-wrap">
          <span className="badge" style={{ background: "#d1e7dd", color: "#0f5132", border: "1px solid #a3cfbb" }}>■ On Track</span>
          <span className="badge" style={{ background: "#fff3cd", color: "#664d03", border: "1px solid #ffda6a" }}>■ Delayed</span>
          <span className="badge" style={{ background: "#cff4fc", color: "#055160", border: "1px solid #9eeaf9" }}>■ New</span>
          <span className="badge" style={{ background: "#e2e3e5", color: "#41464b", border: "1px solid #c4c8cb" }}>■ Removed</span>
          <span className="badge" style={{ background: "#f0f0f0", color: "#6c757d", border: "1px solid #adb5bd" }}>▬ Baseline</span>
        </span>
      </div>

      {/* ── SVAR Gantt ───────────────────────────────────────────────────── */}
      <div style={{ width: "100%", height: "calc(100vh - 310px)" }}>
        <Gantt
          api={ganttApi}
          tasks={displayTasks}
          links={[]}
          scales={SCALE_CONFIGS[scale]}
          columns={COLUMNS}
          markers={markers}
          readonly={true}
          baselines={showBaseline}
          cellHeight={28}
          cellBorders=""
        />
      </div>

      {/* ── 애니메이션 패널 ───────────────────────────────────────────────── */}
      <div className="card border-0 shadow-sm mt-2" style={{ background: "#f8f9fa" }}>
        <div className="card-body py-2 px-3">
          <div className="d-flex align-items-center gap-3">

            {/* Play / Pause */}
            <button
              className="btn btn-sm btn-primary"
              disabled={snapshots.length === 0}
              onClick={isPlaying ? stopPlay : startPlay}
            >
              <i className={`bi bi-${isPlaying ? "pause" : "play"}-fill`} />
            </button>

            {/* 이전 / 다음 */}
            <button
              className="btn btn-sm btn-outline-secondary"
              disabled={snapshots.length === 0}
              onClick={() => stepFrame(-1)}
            >
              <i className="bi bi-skip-start-fill" />
            </button>
            <button
              className="btn btn-sm btn-outline-secondary"
              disabled={snapshots.length === 0}
              onClick={() => stepFrame(1)}
            >
              <i className="bi bi-skip-end-fill" />
            </button>

            {/* 슬라이더 */}
            <div className="flex-grow-1">
              <input
                type="range"
                className="form-range"
                min={0}
                max={Math.max(0, snapshots.length - 1)}
                value={currentFrame}
                disabled={snapshots.length === 0}
                onChange={e => onSliderChange(e.target.value)}
              />
            </div>

            {/* 속도 */}
            <div className="d-flex align-items-center gap-1">
              <label className="form-label small mb-0 text-muted">Speed</label>
              <select
                className="form-select form-select-sm"
                style={{ width: 80 }}
                value={speed}
                onChange={e => {
                  const newSpeed = parseInt(e.target.value, 10);
                  setSpeed(newSpeed);
                  if (isPlaying) { stopPlay(); /* 다음 클릭 시 새 속도 적용 */ }
                }}
              >
                <option value={2000}>0.5×</option>
                <option value={1000}>1×</option>
                <option value={500}>2×</option>
                <option value={250}>4×</option>
              </select>
            </div>

            {/* 리비전 정보 */}
            <div className="text-end" style={{ minWidth: 220 }}>
              <div className="small fw-semibold">
                {curSnap
                  ? `Rev ${curSnap.revisionNumber}: ${curSnap.title}`
                  : snapshots.length === 0 ? "No revision snapshots" : "—"}
              </div>
              <div className="text-muted" style={{ fontSize: ".75rem" }}>
                {curSnap
                  ? `Approved ${curSnap.approvedAt}${curSnap.approvedBy ? " · " + curSnap.approvedBy : ""}`
                  : "Load comparison data to animate"}
              </div>
            </div>

          </div>
        </div>
      </div>
    </>
  );
}
