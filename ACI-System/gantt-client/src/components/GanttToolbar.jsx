import { useState } from "react";

const SCALE_LABELS = {
  day: "Daily",
  week: "Weekly",
  month: "Monthly",
  quarter: "Quarterly",
  year: "Yearly",
};

export default function GanttToolbar({
  onSearch,
  onZoomIn, onZoomOut, onFitToScreen, onToday,
  currentScale, onScaleChange,
  scales = ["day", "week", "month", "quarter", "year"],
  onDateRangeApply, onDateRangeReset,
  onLookahead, activeLookahead,
  showLinks, onToggleLinks,
  showTrade, onToggleTrade,
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

  const iconBtn = (active, onClick, title, icon, danger = false) => (
    <button
      className={`aci-tb-btn${active ? (danger ? " aci-tb-btn--danger-active" : " aci-tb-btn--active") : ""}`}
      onClick={onClick}
      title={title}
    >
      <i className={`bi ${icon}`} />
    </button>
  );

  return (
    <div className="aci-toolbar">
      {/* ── 왼쪽 그룹 ── */}
      <div className="aci-toolbar__left">

        {/* 검색 */}
        <div className="aci-tb-search">
          <i className="bi bi-search aci-tb-search__icon" />
          <input
            type="text"
            className="aci-tb-search__input"
            placeholder="Search tasks…"
            value={searchVal}
            onChange={(e) => handleSearch(e.target.value)}
          />
          {searchVal && (
            <button className="aci-tb-search__clear" onClick={() => handleSearch("")}>
              <i className="bi bi-x" />
            </button>
          )}
        </div>

        <div className="aci-tb-sep" />

        {/* Scale 드롭다운 (outbuild: "Weekly" dropdown) */}
        <div className="dropdown">
          <button
            className="aci-tb-dropdown dropdown-toggle"
            data-bs-toggle="dropdown"
            title="Time scale"
          >
            <i className="bi bi-calendar3 me-1" />
            {SCALE_LABELS[currentScale] ?? "Weekly"}
          </button>
          <ul className="dropdown-menu dropdown-menu-start" style={{ minWidth: 130, fontSize: 13 }}>
            {scales.map(s => (
              <li key={s}>
                <a className={`dropdown-item${currentScale === s ? " active" : ""}`}
                  href="#" onClick={(e) => { e.preventDefault(); onScaleChange?.(s); }}>
                  {SCALE_LABELS[s]}
                </a>
              </li>
            ))}
          </ul>
        </div>

        <div className="aci-tb-sep" />

        {/* Zoom / Fit / Today */}
        <div className="aci-tb-group">
          {iconBtn(false, onZoomIn,      "Zoom in",       "bi-zoom-in")}
          {iconBtn(false, onZoomOut,     "Zoom out",      "bi-zoom-out")}
          {iconBtn(false, onFitToScreen, "Fit to screen", "bi-arrows-angle-contract")}
          {iconBtn(false, onToday,       "Jump to today", "bi-crosshair")}
        </div>

        <div className="aci-tb-sep" />

        {/* Date Range */}
        <div className="dropdown" style={{ position: "relative" }}>
          <button className="aci-tb-btn" onClick={() => setShowDateDrop(!showDateDrop)} title="Custom date range">
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
      </div>

      {/* ── 오른쪽 그룹 ── */}
      <div className="aci-toolbar__right">

        {/* Links */}
        {iconBtn(showLinks, onToggleLinks, "Toggle dependency arrows", "bi-diagram-2")}

        {/* Trade 컬럼 */}
        {onToggleTrade && iconBtn(showTrade, onToggleTrade, "Show/hide Trade column", "bi-tags")}

        {/* Critical Path */}
        {onToggleCriticalPath && iconBtn(showCriticalPath, onToggleCriticalPath, "Critical path", "bi-lightning-charge", true)}

        {/* Bar Color 모드: Status / Trade */}
        {onColorModeChange && (
          <div className="aci-tb-group" title="Bar color mode">
            <button
              className={`aci-tb-btn${colorMode === "status" ? " aci-tb-btn--active" : ""}`}
              onClick={() => onColorModeChange("status")} title="Color by status">
              <i className="bi bi-circle-half" />
            </button>
            <button
              className={`aci-tb-btn${colorMode === "trade" ? " aci-tb-btn--active" : ""}`}
              onClick={() => onColorModeChange("trade")} title="Color by trade">
              <i className="bi bi-palette" />
            </button>
          </div>
        )}

        {/* Progress: Baseline Overlay + Delayed Only */}
        {mode === "progress" && (
          <>
            <button
              className={`aci-tb-btn${showBaselineOverlay ? " aci-tb-btn--active" : ""}`}
              onClick={() => onToggleBaselineOverlay?.(!showBaselineOverlay)}
              title="Show baseline overlay">
              <i className="bi bi-layers" />
            </button>
            <button
              className={`aci-tb-btn${showDelayedOnly ? " aci-tb-btn--active" : ""}`}
              onClick={() => onToggleDelayedOnly?.(!showDelayedOnly)}
              title="Delayed tasks only">
              <i className="bi bi-exclamation-circle" />
            </button>
          </>
        )}

        <div className="aci-tb-sep" />

        {/* Import / Export */}
        <div className="dropdown">
          <button className="aci-tb-btn dropdown-toggle" data-bs-toggle="dropdown" title="Import / Export">
            <i className="bi bi-arrow-down-up" />
          </button>
          <ul className="dropdown-menu dropdown-menu-end" style={{ fontSize: 13 }}>
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

        {/* Color Legend */}
        <div className="dropdown">
          <button className="aci-tb-btn dropdown-toggle" data-bs-toggle="dropdown" title="Color legend">
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
                  { color: "#6b7280", label: "FS: Finish to Start" },
                  { color: "#16a34a", label: "SS: Start to Start" },
                  { color: "#ea580c", label: "FF: Finish to Finish" },
                  { color: "#9333ea", label: "SF: Start to Finish" },
                ].map(({ color, label }) => (
                  <li key={label} className="d-flex align-items-center gap-2 px-1 py-1">
                    <span style={{ width: 28, height: 2, background: color, display: "inline-block", flexShrink: 0 }} />
                    <span>{label}</span>
                  </li>
                ))}
              </>
            ) : (
              <>
                {[
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
                ))}
                <li><hr className="dropdown-divider my-1" /></li>
                <li className="px-1 mb-1" style={{ fontSize: 11, fontWeight: 700, color: "#6b7280", textTransform: "uppercase" }}>Link Types</li>
                {[
                  { color: "#6b7280", label: "FS: Finish to Start" },
                  { color: "#16a34a", label: "SS: Start to Start" },
                  { color: "#ea580c", label: "FF: Finish to Finish" },
                  { color: "#9333ea", label: "SF: Start to Finish" },
                ].map(({ color, label }) => (
                  <li key={label} className="d-flex align-items-center gap-2 px-1 py-1">
                    <span style={{ width: 28, height: 2, background: color, display: "inline-block", flexShrink: 0 }} />
                    <span>{label}</span>
                  </li>
                ))}
              </>
            )}
          </ul>
        </div>

        <div className="aci-tb-sep" />

        {/* Progress: unsaved badge */}
        {mode === "progress" && unsavedCount > 0 && (
          <>
            <span className="aci-tb-badge">
              <i className="bi bi-circle-fill me-1" style={{ fontSize: ".5rem", verticalAlign: "middle" }} />
              {unsavedCount} unsaved
            </span>
            <button className="aci-tb-btn" onClick={onDiscard} disabled={isSaving} title="Discard changes">
              <i className="bi bi-arrow-counterclockwise" />
            </button>
          </>
        )}

        {/* Freeze (Baseline) / Save (Progress) — 우측 끝 강조 버튼 */}
        {mode === "baseline" && onFreezeBaseline && (
          <button className="aci-tb-btn--primary" onClick={onFreezeBaseline} title="Freeze baseline">
            <i className="bi bi-snow me-1" />Freeze
          </button>
        )}
        {mode === "progress" && (
          <button
            className="aci-tb-btn--primary"
            onClick={onSave}
            disabled={isSaving || unsavedCount === 0}
            title="Save changes"
          >
            {isSaving
              ? <span className="spinner-border spinner-border-sm me-1" role="status" />
              : <i className="bi bi-floppy me-1" />}
            Save
          </button>
        )}
      </div>
    </div>
  );
}
