import { useState } from "react";

export default function GanttToolbar({
  onSearch,
  onZoomIn, onZoomOut, onFitToScreen, onToday,
  currentScale, onScaleChange,
  scales = ["day", "week", "month", "quarter", "year"],
  onDateRangeApply, onDateRangeReset,
  onLookahead, activeLookahead,
  showLinks, onToggleLinks,
  showCriticalPath, onToggleCriticalPath,
  onFreezeBaseline, onImportXml, onExportXml, onExportPdf,
  showBaselineOverlay, onToggleBaselineOverlay,
  showDelayedOnly, onToggleDelayedOnly,
  colorMode, onColorModeChange,
  onExportCsv,
  unsavedCount, onSave, onDiscard, isSaving,
  draftTitle,
  mode = "baseline",
}) {
  const [searchVal, setSearchVal] = useState("");
  const [drFrom, setDrFrom]       = useState("");
  const [drTo, setDrTo]           = useState("");
  const [showDateDrop, setShowDateDrop] = useState(false);

  const handleSearch = (v) => { setSearchVal(v); onSearch?.(v); };

  const handleDrApply = () => { onDateRangeApply?.(drFrom, drTo); setShowDateDrop(false); };
  const handleDrReset = () => { setDrFrom(""); setDrTo(""); onDateRangeReset?.(); setShowDateDrop(false); };

  const LOOKAHEAD = [
    { w: 2, l: "2W" }, { w: 3, l: "3W" }, { w: 4, l: "4W" },
    { w: 6, l: "6W" }, { w: 8, l: "8W" }, { w: 12, l: "12W" },
  ];

  return (
    <div className="d-flex gap-2 mb-2 px-1 align-items-center flex-wrap"
      style={{ borderBottom: "1px solid #e5e7eb", paddingBottom: 8 }}>

      {/* 검색 */}
      <div className="input-group input-group-sm" style={{ width: 190 }}>
        <span className="input-group-text bg-white border-end-0 pe-1">
          <i className="bi bi-search text-muted" style={{ fontSize: 12 }} />
        </span>
        <input type="text" className="form-control border-start-0 ps-0"
          placeholder="Search tasks…" style={{ fontSize: 12 }}
          value={searchVal} onChange={(e) => handleSearch(e.target.value)} />
        {searchVal && (
          <button className="btn btn-outline-secondary" onClick={() => handleSearch("")} title="Clear">
            <i className="bi bi-x" style={{ fontSize: 13 }} />
          </button>
        )}
      </div>

      {/* Zoom + Fit + Today */}
      <div className="btn-group btn-group-sm">
        <button className="btn btn-outline-secondary" onClick={onZoomIn} title="Zoom in"><i className="bi bi-zoom-in" /></button>
        <button className="btn btn-outline-secondary" onClick={onZoomOut} title="Zoom out"><i className="bi bi-zoom-out" /></button>
        <button className="btn btn-outline-secondary" onClick={onFitToScreen} title="Fit all"><i className="bi bi-arrows-angle-contract" /></button>
        <button className="btn btn-outline-secondary" onClick={onToday} title="Jump to today"><i className="bi bi-calendar-check" /></button>
      </div>

      {/* Time Scale */}
      <div className="btn-group btn-group-sm">
        {scales.includes("day")     && <button className={`btn btn-outline-secondary${currentScale==="day"?" active":""}`}     onClick={()=>onScaleChange?.("day")}>Day</button>}
        {scales.includes("week")    && <button className={`btn btn-outline-secondary${currentScale==="week"?" active":""}`}    onClick={()=>onScaleChange?.("week")}>Week</button>}
        {scales.includes("month")   && <button className={`btn btn-outline-secondary${currentScale==="month"?" active":""}`}   onClick={()=>onScaleChange?.("month")}>Month</button>}
        {scales.includes("quarter") && <button className={`btn btn-outline-secondary${currentScale==="quarter"?" active":""}`} onClick={()=>onScaleChange?.("quarter")}>Quarter</button>}
        {scales.includes("year")    && <button className={`btn btn-outline-secondary${currentScale==="year"?" active":""}`}    onClick={()=>onScaleChange?.("year")}>Year</button>}
      </div>

      {/* Lookahead (Outbuild 핵심) */}
      {onLookahead && (
        <div className="d-flex align-items-center gap-1">
          <span style={{ fontSize: 11, color: "#6b7280", whiteSpace: "nowrap" }}>Lookahead:</span>
          <div className="btn-group btn-group-sm">
            {LOOKAHEAD.map(({ w, l }) => (
              <button key={w}
                className={`btn btn-sm${activeLookahead===w?" btn-primary":" btn-outline-primary"}`}
                style={{ fontSize: 11, padding: "2px 7px" }}
                onClick={() => onLookahead(activeLookahead===w ? null : w)}
                title={`Show next ${w} weeks`}
              >{l}</button>
            ))}
          </div>
        </div>
      )}

      {/* Date Range picker */}
      <div className="dropdown" style={{ position: "relative" }}>
        <button className="btn btn-sm btn-outline-secondary" onClick={() => setShowDateDrop(!showDateDrop)} title="Custom date range">
          <i className="bi bi-calendar-range" />
        </button>
        {showDateDrop && (
          <div className="dropdown-menu show p-3"
            style={{ minWidth: 260, position: "absolute", top: "100%", left: 0, zIndex: 1050 }}
            onClick={(e) => e.stopPropagation()}>
            <div style={{ fontSize: 11, fontWeight: 700, color: "#6b7280", textTransform: "uppercase", marginBottom: 8 }}>Visible Period</div>
            <div className="d-flex align-items-center gap-2 mb-2">
              <label style={{ fontSize: 12, width: 32, flexShrink: 0 }}>From</label>
              <input type="date" className="form-control form-control-sm" value={drFrom} onChange={(e) => setDrFrom(e.target.value)} />
            </div>
            <div className="d-flex align-items-center gap-2 mb-3">
              <label style={{ fontSize: 12, width: 32, flexShrink: 0 }}>To</label>
              <input type="date" className="form-control form-control-sm" value={drTo} onChange={(e) => setDrTo(e.target.value)} />
            </div>
            <div className="d-flex gap-2">
              <button className="btn btn-sm btn-primary flex-fill" onClick={handleDrApply}>Apply</button>
              <button className="btn btn-sm btn-outline-secondary flex-fill" onClick={handleDrReset}>Reset</button>
            </div>
          </div>
        )}
      </div>

      <div style={{ width: 1, height: 20, background: "#e5e7eb" }} />

      {/* Links 토글 */}
      <button className={`btn btn-sm${showLinks?" btn-secondary":" btn-outline-secondary"}`}
        onClick={onToggleLinks} title="Toggle dependency arrows">
        <i className="bi bi-arrow-right me-1" />Links
      </button>

      {/* Critical Path */}
      {onToggleCriticalPath && (
        <button className={`btn btn-sm${showCriticalPath?" btn-danger":" btn-outline-danger"}`}
          onClick={onToggleCriticalPath} title="Highlight critical path">
          <i className="bi bi-diagram-3 me-1" />Critical
        </button>
      )}

      {/* Bar Color 모드: Status vs Trade */}
      {onColorModeChange && (
        <div className="btn-group btn-group-sm" title="Bar color mode">
          <button className={`btn btn-outline-secondary${colorMode==="status"?" active":""}`}
            style={{ fontSize: 11 }} onClick={() => onColorModeChange("status")}>
            <i className="bi bi-circle-half me-1" />Status
          </button>
          <button className={`btn btn-outline-secondary${colorMode==="trade"?" active":""}`}
            style={{ fontSize: 11 }} onClick={() => onColorModeChange("trade")}>
            <i className="bi bi-palette me-1" />Trade
          </button>
        </div>
      )}

      {/* Baseline Overlay + Delayed Only (Progress) */}
      {mode === "progress" && (
        <>
          <div className="form-check form-switch mb-0">
            <input className="form-check-input" type="checkbox" id="showBaseline"
              checked={showBaselineOverlay} onChange={(e) => onToggleBaselineOverlay?.(e.target.checked)} />
            <label className="form-check-label small" htmlFor="showBaseline">Baseline</label>
          </div>
          <div className="form-check form-switch mb-0">
            <input className="form-check-input" type="checkbox" id="showDelayed"
              checked={showDelayedOnly} onChange={(e) => onToggleDelayedOnly?.(e.target.checked)} />
            <label className="form-check-label small" htmlFor="showDelayed">Delayed Only</label>
          </div>
        </>
      )}

      {/* Freeze Baseline */}
      {mode === "baseline" && onFreezeBaseline && (
        <button className="btn btn-sm btn-primary" onClick={onFreezeBaseline}>
          <i className="bi bi-snow me-1" />Freeze
        </button>
      )}

      {/* Import / Export */}
      <div className="dropdown">
        <button className="btn btn-sm btn-outline-secondary dropdown-toggle" data-bs-toggle="dropdown">
          <i className="bi bi-arrow-down-up me-1" />{mode === "baseline" ? "Import/Export" : "Export"}
        </button>
        <ul className="dropdown-menu dropdown-menu-end">
          {mode === "baseline" && onImportXml && (
            <li><a className="dropdown-item" href="#" onClick={(e) => { e.preventDefault(); onImportXml(); }}>
              <i className="bi bi-file-earmark-arrow-up me-2 text-primary" />Import MS Project XML
            </a></li>
          )}
          {mode === "baseline" && <li><hr className="dropdown-divider" /></li>}
          {mode === "baseline" && onExportXml && (
            <li><a className="dropdown-item" href="#" onClick={(e) => { e.preventDefault(); onExportXml(); }}>
              <i className="bi bi-file-earmark-spreadsheet me-2" />Export MS Project XML
            </a></li>
          )}
          <li><a className="dropdown-item" href="#" onClick={(e) => { e.preventDefault(); onExportCsv?.(); }}>
            <i className="bi bi-file-earmark-excel me-2 text-success" />Export CSV/Excel
          </a></li>
          {onExportPdf && (
            <li><a className="dropdown-item" href="#" onClick={(e) => { e.preventDefault(); onExportPdf(); }}>
              <i className="bi bi-file-earmark-pdf me-2 text-danger" />Print / Export PDF
            </a></li>
          )}
        </ul>
      </div>

      {/* Color Legend 드롭다운 */}
      <div className="dropdown">
        <button className="btn btn-sm btn-outline-secondary" data-bs-toggle="dropdown" title="Color legend">
          <i className="bi bi-info-circle" />
        </button>
        <ul className="dropdown-menu dropdown-menu-end p-2" style={{ minWidth: 220, fontSize: 12 }}>
          <li className="px-1 mb-1" style={{ fontSize: 11, fontWeight: 700, color: "#6b7280", textTransform: "uppercase" }}>Bar Colors</li>
          {mode === "baseline" ? (
            <>
              {[
                { color: "#16a34a", label: "Complete (100%)" },
                { color: "#3b82f6", label: "In Progress" },
                { color: "#94a3b8", label: "Not Started" },
                { color: "#ef4444", label: "Overdue" },
                { color: "#d97706", label: "Milestone" },
                { color: "#1e40af", label: "Summary / Phase" },
              ].map(({ color, label }) => (
                <li key={label} className="d-flex align-items-center gap-2 px-1 py-1">
                  <span style={{ width: 28, height: 12, borderRadius: 3, background: color, display: "inline-block", flexShrink: 0 }} />
                  <span>{label}</span>
                </li>
              ))}
              <li><hr className="dropdown-divider my-1" /></li>
              <li className="px-1 mb-1" style={{ fontSize: 11, fontWeight: 700, color: "#6b7280", textTransform: "uppercase" }}>Link Types</li>
              {[
                { color: "#6b7280", label: "FS — Finish to Start" },
                { color: "#16a34a", label: "SS — Start to Start" },
                { color: "#ea580c", label: "FF — Finish to Finish" },
                { color: "#9333ea", label: "SF — Start to Finish" },
              ].map(({ color, label }) => (
                <li key={label} className="d-flex align-items-center gap-2 px-1 py-1">
                  <span style={{ width: 28, height: 2, background: color, display: "inline-block", flexShrink: 0 }} />
                  <span>{label}</span>
                </li>
              ))}
            </>
          ) : (
            [
              { color: "#198754", label: "Complete" },
              { color: "#0dcaf0", label: "In Progress" },
              { color: "#ffc107", label: "Delayed (>0d)" },
              { color: "#dc3545", label: "Critically Delayed (>5d)" },
              { color: "#0d6efd", label: "New (no baseline)" },
              { color: "#adb5bd", label: "Removed" },
            ].map(({ color, label }) => (
              <li key={label} className="d-flex align-items-center gap-2 px-1 py-1">
                <span style={{ width: 28, height: 12, borderRadius: 3, background: color, display: "inline-block", flexShrink: 0 }} />
                <span>{label}</span>
              </li>
            ))
          )}
        </ul>
      </div>

      {/* Progress: Save / Discard */}
      {mode === "progress" && unsavedCount > 0 && (
        <>
          <span className="badge bg-warning text-dark" style={{ fontSize: ".85rem" }}>
            <i className="bi bi-circle-fill me-1" style={{ fontSize: ".5rem", verticalAlign: "middle" }} />
            {unsavedCount} unsaved
          </span>
          <button className="btn btn-sm btn-success" onClick={onSave} disabled={isSaving}>
            {isSaving ? <span className="spinner-border spinner-border-sm me-1" role="status" /> : <i className="bi bi-floppy me-1" />}
            Save
          </button>
          <button className="btn btn-sm btn-outline-secondary" onClick={onDiscard} disabled={isSaving}>
            <i className="bi bi-arrow-counterclockwise me-1" />Discard
          </button>
        </>
      )}
      {mode === "progress" && draftTitle && (
        <span className="badge bg-secondary ms-1" style={{ fontSize: ".8rem" }}>
          <i className="bi bi-pencil me-1" />{draftTitle}
        </span>
      )}

      {/* Progress: Status legend */}
      {mode === "progress" && (
        <span className="ms-auto d-flex gap-2 align-items-center flex-wrap" style={{ fontSize: 11 }}>
          <span className="badge bg-success-subtle text-success border border-success-subtle">■ Complete</span>
          <span className="badge bg-info-subtle text-info border border-info-subtle">■ In Progress</span>
          <span className="badge bg-warning-subtle text-warning border border-warning-subtle">■ Delayed</span>
          <span className="badge bg-danger-subtle text-danger border border-danger-subtle">■ Critical</span>
          <span className="badge bg-primary-subtle text-primary border border-primary-subtle">■ New</span>
        </span>
      )}
    </div>
  );
}
