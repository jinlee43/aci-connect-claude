import { useState, useEffect, useRef } from "react";
import { formatDate, countWorkingDays, addWorkingDays } from "../utils/dateUtils";

/**
 * 태스크 추가 / 편집 다이얼로그
 *
 * Props:
 *   task      - 편집 시 기존 태스크 객체 (null이면 신규 추가)
 *   parentId  - 신규 추가 시 부모 태스크 id (0 = root)
 *   trades    - [{ id, name, color }]
 *   employees - [{ id, name }]
 *   onSave    - (taskData) => void
 *   onClose   - () => void
 */
export default function TaskDialog({ task, parentId = 0, trades = [], employees = [], onSave, onClose }) {
  const isEdit = !!task;

  const toInputDate = (d) => {
    if (!d) return "";
    const dt = d instanceof Date ? d : new Date(d);
    return dt.toISOString().slice(0, 10);
  };

  const [form, setForm] = useState({
    text:           task?.text        ?? "",
    type:           task?.type        ?? "task",
    start:          toInputDate(task?.start),
    end:            toInputDate(task?.end),
    duration:       task?.duration    ?? 1,
    progress:       Math.round((task?.progress ?? 0) * 100),
    wbs_code:       task?.wbs_code    ?? "",
    trade_id:       task?.trade_id    ?? "",
    assigned_to_id: task?.assigned_to_id ?? "",
    notes:          task?.notes       ?? "",
  });
  const [errors, setErrors] = useState({});
  const firstRef = useRef(null);

  useEffect(() => { firstRef.current?.focus(); }, []);

  // start 변경 시 end 자동 계산 (Working Days)
  useEffect(() => {
    if (form.start && form.duration > 0 && !form.end) {
      setForm(f => ({ ...f, end: toInputDate(addWorkingDays(f.start, f.duration)) }));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [form.start]);

  // duration 변경 시 end 재계산 (Working Days)
  const handleDurationChange = (val) => {
    const dur = Math.max(1, parseInt(val) || 1);
    let end = form.end;
    if (form.start) end = toInputDate(addWorkingDays(form.start, dur));
    setForm(f => ({ ...f, duration: dur, end }));
  };

  // end 변경 시 duration 재계산 (Working Days)
  const handleEndChange = (val) => {
    let dur = form.duration;
    if (form.start && val) dur = countWorkingDays(form.start, val);
    setForm(f => ({ ...f, end: val, duration: dur }));
  };

  const set = (k) => (e) => setForm(f => ({ ...f, [k]: e.target.value }));

  const validate = () => {
    const e = {};
    if (!form.text.trim()) e.text = "Task name is required";
    if (!form.start)        e.start = "Start date is required";
    if (!form.end)          e.end = "End date is required";
    if (form.start && form.end && form.end < form.start) e.end = "End must be after start";
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSave = () => {
    if (!validate()) return;
    onSave({
      text:           form.text.trim(),
      type:           form.type,
      start:          new Date(form.start),
      end:            new Date(form.end),
      duration:       Number(form.duration),
      progress:       Number(form.progress) / 100,
      wbs_code:       form.wbs_code || null,
      trade_id:       form.trade_id ? Number(form.trade_id) : null,
      assigned_to_id: form.assigned_to_id ? Number(form.assigned_to_id) : null,
      notes:          form.notes || null,
      ...(isEdit ? {} : { parent: parentId }),
    });
  };

  const handleKeyDown = (e) => {
    if (e.key === "Escape") onClose();
    if (e.key === "Enter" && e.target.tagName !== "TEXTAREA") handleSave();
  };

  const selectedTrade = trades.find(t => String(t.id) === String(form.trade_id));

  return (
    <div className="aci-dialog-backdrop" onMouseDown={(e) => { if (e.target === e.currentTarget) onClose(); }}>
      <div className="aci-dialog" onKeyDown={handleKeyDown}>

        {/* 헤더 */}
        <div className="aci-dialog__header">
          <h5 className="aci-dialog__title">
            <i className={`bi ${isEdit ? "bi-pencil-square" : "bi-plus-circle"} me-2`} />
            {isEdit ? "Edit Task" : "Add Task"}
          </h5>
          <button className="aci-dialog__close" onClick={onClose} title="Close">
            <i className="bi bi-x-lg" />
          </button>
        </div>

        {/* 바디 */}
        <div className="aci-dialog__body">

          {/* Task Name */}
          <div className="aci-dialog__field">
            <label className="aci-dialog__label">Task Name <span className="text-danger">*</span></label>
            <input ref={firstRef} type="text" className={`aci-dialog__input${errors.text ? " is-invalid" : ""}`}
              value={form.text} onChange={set("text")} placeholder="Enter task name" />
            {errors.text && <div className="aci-dialog__error">{errors.text}</div>}
          </div>

          {/* Type */}
          <div className="aci-dialog__field">
            <label className="aci-dialog__label">Type</label>
            <div className="aci-dialog__radio-group">
              {[
                { val: "task",      icon: "bi-bar-chart-steps", label: "Task" },
                { val: "summary",   icon: "bi-folder2",         label: "Summary" },
                { val: "milestone", icon: "bi-diamond",         label: "Milestone" },
              ].map(({ val, icon, label }) => (
                <label key={val} className={`aci-dialog__radio${form.type === val ? " aci-dialog__radio--active" : ""}`}>
                  <input type="radio" name="type" value={val}
                    checked={form.type === val} onChange={set("type")} className="visually-hidden" />
                  <i className={`bi ${icon} me-1`} />{label}
                </label>
              ))}
            </div>
          </div>

          {/* 날짜 / Duration — 2열 */}
          <div className="aci-dialog__row3">
            <div className="aci-dialog__field">
              <label className="aci-dialog__label">Start Date <span className="text-danger">*</span></label>
              <input type="date" className={`aci-dialog__input${errors.start ? " is-invalid" : ""}`}
                value={form.start} onChange={set("start")} />
              {errors.start && <div className="aci-dialog__error">{errors.start}</div>}
            </div>
            <div className="aci-dialog__field">
              <label className="aci-dialog__label">End Date <span className="text-danger">*</span></label>
              <input type="date" className={`aci-dialog__input${errors.end ? " is-invalid" : ""}`}
                value={form.end} onChange={(e) => handleEndChange(e.target.value)} />
              {errors.end && <div className="aci-dialog__error">{errors.end}</div>}
            </div>
            <div className="aci-dialog__field">
              <label className="aci-dialog__label">Duration (days)</label>
              <input type="number" min={1} max={9999} className="aci-dialog__input"
                value={form.duration} onChange={(e) => handleDurationChange(e.target.value)} />
            </div>
          </div>

          {/* Progress */}
          <div className="aci-dialog__field">
            <label className="aci-dialog__label">Progress — {form.progress}%</label>
            <div className="d-flex align-items-center gap-3">
              <input type="range" className="form-range flex-grow-1" min={0} max={100} step={5}
                value={form.progress} onChange={set("progress")} />
              <input type="number" min={0} max={100} className="aci-dialog__input" style={{ width: 70 }}
                value={form.progress} onChange={set("progress")} />
            </div>
          </div>

          {/* Trade / Responsible — 2열 */}
          <div className="aci-dialog__row2">
            <div className="aci-dialog__field">
              <label className="aci-dialog__label">Trade</label>
              <div className="d-flex align-items-center gap-2">
                {selectedTrade && (
                  <span style={{ width: 12, height: 12, borderRadius: 3, background: selectedTrade.color, display: "inline-block", flexShrink: 0 }} />
                )}
                <select className="aci-dialog__input" value={form.trade_id} onChange={set("trade_id")}>
                  <option value="">— None —</option>
                  {trades.map(t => (
                    <option key={t.id} value={t.id}>{t.name}</option>
                  ))}
                </select>
              </div>
            </div>
            <div className="aci-dialog__field">
              <label className="aci-dialog__label">Responsible</label>
              <select className="aci-dialog__input" value={form.assigned_to_id} onChange={set("assigned_to_id")}>
                <option value="">— None —</option>
                {employees.map(e => (
                  <option key={e.id} value={e.id}>{e.name}</option>
                ))}
              </select>
            </div>
          </div>

          {/* WBS Code */}
          <div className="aci-dialog__field">
            <label className="aci-dialog__label">WBS Code</label>
            <input type="text" className="aci-dialog__input" style={{ maxWidth: 160 }}
              value={form.wbs_code} onChange={set("wbs_code")} placeholder="e.g. 1.2.3" />
          </div>

          {/* Notes */}
          <div className="aci-dialog__field">
            <label className="aci-dialog__label">Notes</label>
            <textarea className="aci-dialog__input" rows={3}
              value={form.notes} onChange={set("notes")} placeholder="Optional notes…" />
          </div>
        </div>

        {/* 푸터 */}
        <div className="aci-dialog__footer">
          <button className="aci-dialog__btn aci-dialog__btn--secondary" onClick={onClose}>Cancel</button>
          <button className="aci-dialog__btn aci-dialog__btn--primary" onClick={handleSave}>
            <i className={`bi ${isEdit ? "bi-check-lg" : "bi-plus-lg"} me-1`} />
            {isEdit ? "Save Changes" : "Add Task"}
          </button>
        </div>
      </div>
    </div>
  );
}
