import { useEffect, useRef } from "react";

/**
 * SVAR Gantt Action 컬럼에 편집(Status) + 삭제 버튼을 주입합니다.
 *
 * SVAR React wrapper(wx-react-gantt)의 Kf() 변환 함수가 다중 액션 버튼을
 * 지원하지 않기 때문에 MutationObserver + DOM 직접 주입 방식으로 구현합니다.
 * SVAR는 props.api ref를 통해 exec("show-editor"), exec("delete-task") API를
 * 노출하므로 이를 활용합니다.
 *
 * ⚠️ deps 배열 없음 (intentional): 컴포넌트는 초기 loading 상태에서 spinner를
 * 렌더링하다가 데이터 로드 후 Gantt div를 렌더링합니다. ref 객체는 변하지 않으므로
 * deps에 ref를 넣으면 effect가 재실행되지 않습니다.
 * setupRef.current.container 가드로 중복 설정을 방지합니다.
 *
 * @param {React.RefObject} ganttApiRef    - <Gantt api={ganttApiRef} />
 * @param {React.RefObject} containerRef  - div.aci-gantt-container ref
 * @param {object}          [opts]
 * @param {Function}        [opts.onDeleteTask]  - (taskId) => void  백엔드 삭제 콜백
 * @param {Function}        [opts.onEditTask]    - (taskId) => void  편집 콜백 (없으면 SVAR 에디터)
 */
export function useGanttActionButtons(ganttApiRef, containerRef, opts = {}) {
  const setupRef = useRef({ observer: null, container: null, handleClick: null });

  // ── deps 없음: 매 렌더 후 실행되지만 같은 container면 즉시 종료 ─────────────
  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => {
    const container = containerRef.current;
    if (!container) return; // 아직 DOM 미준비 (loading 상태)
    if (setupRef.current.container === container) return; // 이미 이 element에 연결됨

    // ── 이전 attachment 해제 ─────────────────────────────────────────────────
    if (setupRef.current.observer) {
      setupRef.current.observer.disconnect();
    }
    if (setupRef.current.container && setupRef.current.handleClick) {
      setupRef.current.container.removeEventListener("click", setupRef.current.handleClick, true);
    }

    // ── 1. 버튼 주입 ─────────────────────────────────────────────────────────
    function injectButtons(root) {
      root.querySelectorAll('[data-col-id="action"]:not([data-aci-enh])').forEach((cell) => {
        cell.setAttribute("data-aci-enh", "1");

        // SVAR가 만든 내부 div (정렬 컨테이너)
        const inner = cell.firstElementChild || cell;

        // 편집 버튼 (bi-pencil)
        const editBtn = document.createElement("i");
        editBtn.className = "bi bi-pencil aci-act-icon";
        editBtn.setAttribute("data-aci-action", "show-editor");
        editBtn.setAttribute("title", "Edit task");

        // 삭제 버튼 (bi-trash)
        const delBtn = document.createElement("i");
        delBtn.className = "bi bi-trash aci-act-icon aci-act-icon--danger";
        delBtn.setAttribute("data-aci-action", "delete-task");
        delBtn.setAttribute("title", "Delete task");

        inner.appendChild(editBtn);
        inner.appendChild(delBtn);
      });
    }

    // ── 2. 클릭 이벤트 위임 (capture phase로 SVAR 내부 핸들러보다 먼저 처리) ──
    function handleClick(e) {
      const btn = e.target.closest("[data-aci-action]");
      if (!btn) return;

      const action = btn.dataset.aciAction;
      if (action !== "show-editor" && action !== "delete-task") return;

      // SVAR 행의 data-id 속성에서 taskId 추출
      const row = e.target.closest("[data-id]");
      if (!row) return;
      const taskId = parseInt(row.dataset.id, 10);
      if (!taskId || !ganttApiRef.current) return;

      e.stopPropagation();
      e.preventDefault();

      if (action === "show-editor") {
        if (opts.onEditTask) {
          opts.onEditTask(taskId);
        } else {
          ganttApiRef.current.exec("show-editor", { id: taskId });
        }
      } else if (action === "delete-task") {
        const api = ganttApiRef.current;
        const task = api.getTask?.(taskId);
        const name = task?.text || `Task #${taskId}`;
        if (!window.confirm(`"${name}" 태스크를 삭제하시겠습니까?`)) return;

        // SVAR 내부 상태에서 제거
        api.exec("delete-task", { id: taskId });

        // 백엔드 삭제 콜백
        if (opts.onDeleteTask) opts.onDeleteTask(taskId);
      }
    }

    // ── 3. MutationObserver: SVAR가 DOM 갱신할 때마다 재주입 ─────────────────
    const observer = new MutationObserver((mutations) => {
      if (mutations.some((m) => m.addedNodes.length > 0)) injectButtons(container);
    });

    observer.observe(container, { childList: true, subtree: true });
    injectButtons(container); // 초기 주입

    container.addEventListener("click", handleClick, true); // capture

    // 현재 attachment 저장
    setupRef.current = { observer, container, handleClick };
  }); // ← 의도적으로 deps 없음 (매 렌더 후 실행, container 가드로 중복 방지)

  // ── unmount 시 정리 ───────────────────────────────────────────────────────
  useEffect(() => {
    return () => {
      const { observer, container, handleClick } = setupRef.current;
      if (observer) observer.disconnect();
      if (container && handleClick) {
        container.removeEventListener("click", handleClick, true);
      }
      setupRef.current = { observer: null, container: null, handleClick: null };
    };
  }, []); // eslint-disable-line react-hooks/exhaustive-deps
}
