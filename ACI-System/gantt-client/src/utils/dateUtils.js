/**
 * ASP.NET 날짜 문자열 → Date 객체 변환
 * GanttDataService가 "MM-dd-yyyy HH:mm" 또는 ISO "yyyy-MM-dd" 형식으로 반환
 */
export function parseGanttDate(str) {
  if (!str) return null;
  // ISO 형식 (yyyy-MM-dd) 처리
  if (/^\d{4}-\d{2}-\d{2}/.test(str)) {
    return new Date(str);
  }
  // MM-dd-yyyy HH:mm 형식
  const m = str.match(/^(\d{2})-(\d{2})-(\d{4})/);
  if (m) {
    return new Date(parseInt(m[3]), parseInt(m[1]) - 1, parseInt(m[2]));
  }
  return new Date(str);
}

/**
 * Date → "YYYY-MM-DD" 문자열 (API 전송용)
 */
export function formatDate(date) {
  if (!date) return "";
  const d = date instanceof Date ? date : new Date(date);
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${y}-${m}-${day}`;
}

/**
 * Date → "MM/DD/YY" 표시용 (간트차트 셀)
 */
export function displayDate(date) {
  if (!date) return "";
  const d = date instanceof Date ? date : new Date(date);
  const m  = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  const y  = String(d.getFullYear()).slice(2);
  return `${m}/${day}/${y}`;
}

/**
 * DB 링크 타입 숫자 → SVAR link type string
 * FS=0→"e2s", SS=1→"s2s", FF=2→"e2e", SF=3→"s2e"
 */
export function parseLinkType(type) {
  const map = { 0: "e2s", 1: "s2s", 2: "e2e", 3: "s2e" };
  return map[parseInt(type)] || "e2s";
}

/**
 * ASP.NET GanttTaskDto → SVAR Gantt task 형식 변환
 */
export function toSvarTask(dto) {
  const start = parseGanttDate(dto.start_date);
  const end = dto.end_date ? parseGanttDate(dto.end_date) : null;

  const type = dto.type === "project" ? "summary" : dto.type || "task";

  // duration은 Working Days 기준으로 재계산 (start+end 둘 다 있을 때)
  const duration = (start && end) ? countWorkingDays(start, end) : (dto.duration || 1);

  return {
    id: Number(dto.id),
    text: dto.text || dto.label || "",
    start: start,
    end: end,
    duration,
    parent: Number(dto.parent) || 0,
    progress: dto.progress || 0,
    type,
    // open은 summary(부모) 태스크에만 설정 — leaf에 open:true 설정 시
    // SVAR 내부 Ho()에서 data=null에 forEach 호출해 크래시 발생
    ...(type === "summary" ? { open: dto.open !== false } : {}),

    // Custom fields (passed through)
    wbs_code: dto.wbs_code,
    trade_name: dto.trade_name,
    trade_color: dto.color,
    assigned_to_name: dto.assigned_to_name,
    assigned_to_id: dto.assigned_to_id,
    notes: dto.notes,

    // Current Schedule fields
    days_shifted: dto.days_shifted,
    working_status: dto.working_status,
    baseline_id: dto.baseline_id,

    // Baseline overlay fields (SVAR native baseline support)
    base_start: dto.baseline_start ? parseGanttDate(dto.baseline_start) : null,
    base_end: dto.baseline_end ? parseGanttDate(dto.baseline_end) : null,
  };
}

/**
 * ASP.NET GanttLinkDto → SVAR Gantt link 형식 변환
 * - id/source/target 은 Number로 통일 (문자열 vs 숫자 불일치 방지)
 * - lag 제거 (SVAR 미지원 필드 → 렌더링 방해 가능)
 */
export function toSvarLink(dto) {
  return {
    id: Number(dto.id),
    source: Number(dto.source),
    target: Number(dto.target),
    type: parseLinkType(dto.type),
  };
}

/** toSvarTask에서도 id/parent를 Number로 통일 */
export function normalizeId(v) {
  const n = Number(v);
  return isNaN(n) ? v : n;
}

/**
 * 두 날짜 사이의 Working Days 수 (시작·종료 포함, 토·일 제외)
 * 예) 월~금 = 5, 금~다음월 = 2
 */
export function countWorkingDays(start, end) {
  const s = new Date(start); s.setHours(0, 0, 0, 0);
  const e = new Date(end);   e.setHours(0, 0, 0, 0);
  if (s > e) return 1;
  let count = 0;
  const cur = new Date(s);
  while (cur <= e) {
    const day = cur.getDay();
    if (day !== 0 && day !== 6) count++;
    cur.setDate(cur.getDate() + 1);
  }
  return Math.max(1, count);
}

/**
 * 시작일로부터 N Working Days 후의 날짜 반환 (시작일 = 1일째)
 * 시작일이 주말이면 다음 월요일을 시작으로 간주
 */
export function addWorkingDays(start, days) {
  const d = new Date(start); d.setHours(0, 0, 0, 0);
  // 시작일이 주말이면 월요일로 이동
  while (d.getDay() === 0 || d.getDay() === 6) d.setDate(d.getDate() + 1);
  let remaining = Math.max(1, days) - 1; // 시작일 자체가 1일째
  while (remaining > 0) {
    d.setDate(d.getDate() + 1);
    if (d.getDay() !== 0 && d.getDay() !== 6) remaining--;
  }
  return d;
}

/**
 * 태스크 배열을 DB id 기준 depth-first pre-order로 정렬
 * siblings 내에서 id 오름차순 → SVAR 표시 순서 = DB id 순서
 */
export function sortByTreeId(tasks) {
  const childMap = new Map();
  tasks.forEach(t => {
    const pid = t.parent ?? 0;
    if (!childMap.has(pid)) childMap.set(pid, []);
    childMap.get(pid).push(t);
  });
  childMap.forEach(children => children.sort((a, b) => a.id - b.id));
  const result = [];
  const walk = (pid) => {
    (childMap.get(pid) || []).forEach(task => {
      result.push(task);
      walk(task.id);
    });
  };
  walk(0);
  return result;
}
