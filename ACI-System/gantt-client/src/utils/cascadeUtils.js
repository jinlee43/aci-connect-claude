import { countWorkingDays, addWorkingDays } from "./dateUtils";

const DAY_MS = 24 * 3600 * 1000;

/**
 * 날짜에 lag(캘린더 일수)를 더한 Date 반환
 */
function addLag(date, lag) {
  return new Date(date.getTime() + (lag || 0) * DAY_MS);
}

/**
 * FS / SS / FF / SF 링크를 따라 후속 태스크를 재귀적으로 이동합니다.
 *
 * Link type 매핑 (SVAR):
 *   e2s = FS (Finish-to-Start)   — 선행 End   → 후속 Start
 *   s2s = SS (Start-to-Start)    — 선행 Start → 후속 Start
 *   e2e = FF (Finish-to-Finish)  — 선행 End   → 후속 End (Start도 같이 이동)
 *   s2e = SF (Start-to-Finish)   — 선행 Start → 후속 End (Start도 같이 이동, 드문 케이스)
 *
 * @param {object}   movedTask   - 이동된 태스크 { id, start:Date, end:Date, duration }
 * @param {object}   oldTask     - 이동 전 태스크 (delta 계산용) { start:Date, end:Date }
 * @param {Array}    allLinks    - 전체 링크 배열
 * @param {Array}    allTasks    - 현재 태스크 배열 (스냅샷, duration 조회용)
 * @param {Function} onUpdate    - (id, newStart, newEnd, newDuration) => void
 * @param {Set}      [visited]   - 사이클 방지용 (내부 사용)
 */
export function cascadeFS(movedTask, oldTask, allLinks, allTasks, onUpdate, visited = new Set()) {
  if (visited.has(movedTask.id)) return;
  visited.add(movedTask.id);

  // 이 태스크에서 나가는 모든 링크
  const outLinks = allLinks.filter(l => l.source === movedTask.id);

  outLinks.forEach(link => {
    const lag = link.lag || 0;
    const successor = allTasks.find(t => t.id === link.target);
    if (!successor) return;

    const dur = successor.duration || 1;
    let newStart, newEnd, newDuration;

    switch (link.type) {
      case "e2s": // FS — 선행 End → 후속 Start
        newStart   = addLag(movedTask.end, lag);
        newEnd     = addWorkingDays(newStart, dur);
        newDuration = countWorkingDays(newStart, newEnd);
        break;

      case "s2s": // SS — 선행 Start → 후속 Start
        newStart   = addLag(movedTask.start, lag);
        newEnd     = addWorkingDays(newStart, dur);
        newDuration = countWorkingDays(newStart, newEnd);
        break;

      case "e2e": // FF — 선행 End → 후속 End, duration 유지하며 Start 역산
        newEnd     = addLag(movedTask.end, lag);
        // End에서 duration만큼 역방향으로 Start 계산 (Working Days)
        newStart   = subtractWorkingDays(newEnd, dur);
        newDuration = countWorkingDays(newStart, newEnd);
        break;

      case "s2e": // SF — 선행 Start → 후속 End, duration 유지하며 Start 역산
        newEnd     = addLag(movedTask.start, lag);
        newStart   = subtractWorkingDays(newEnd, dur);
        newDuration = countWorkingDays(newStart, newEnd);
        break;

      default:
        return; // 알 수 없는 링크 타입은 건너뜀
    }

    onUpdate(link.target, newStart, newEnd, newDuration);

    // 재귀: 이 후속 태스크의 후속도 cascade
    cascadeFS(
      { ...successor, start: newStart, end: newEnd, duration: newDuration },
      successor,   // oldTask = 이동 전 원본 (재귀에서는 delta 계산 안 씀)
      allLinks,
      allTasks,
      onUpdate,
      visited
    );
  });
}

/**
 * endDate에서 N Working Days 이전 날짜를 반환 (endDate = N일째)
 * FF/SF 링크에서 End → Start 역산 시 사용
 */
function subtractWorkingDays(endDate, days) {
  const d = new Date(endDate);
  d.setHours(0, 0, 0, 0);
  // endDate가 주말이면 금요일로 이동
  while (d.getDay() === 0 || d.getDay() === 6) d.setDate(d.getDate() - 1);
  let remaining = Math.max(1, days) - 1;
  while (remaining > 0) {
    d.setDate(d.getDate() - 1);
    if (d.getDay() !== 0 && d.getDay() !== 6) remaining--;
  }
  return d;
}
