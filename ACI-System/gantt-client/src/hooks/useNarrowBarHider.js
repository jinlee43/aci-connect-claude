import { useEffect, useRef } from "react";

/**
 * 타임라인 바의 너비에 따라 CSS 클래스를 부착합니다.
 *
 * - wx-bar--narrow  : 바 너비 <= NARROW_THRESHOLD → 레이블 숨김 (너무 작아서 의미 없음)
 * - wx-bar--wide    : 바 너비 >= WIDE_THRESHOLD   → 레이블을 바 안쪽에 표시
 *                     (바가 너무 넓어서 오른쪽 레이블이 화면 밖으로 나갈 경우)
 *
 * CSS (aci-gantt.css) 에서:
 *   .wx-bar--narrow .wx-content  { display: none }
 *   .wx-bar--wide   .wx-content  { position: relative; left: auto; ... inside bar }
 */
const NARROW_THRESHOLD = 20;  // px 이하 → 레이블 숨김
const WIDE_THRESHOLD   = 400; // px 이상 → 레이블 바 안쪽

export function useNarrowBarHider(containerRef) {
  const setupRef = useRef({ observer: null, container: null });

  // eslint-disable-next-line react-hooks/exhaustive-deps
  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;
    if (setupRef.current.container === container) return;

    if (setupRef.current.observer) setupRef.current.observer.disconnect();

    function classifyBars(root) {
      root.querySelectorAll(".wx-bar").forEach((bar) => {
        const w = bar.offsetWidth;
        if (w <= NARROW_THRESHOLD) {
          bar.classList.add("wx-bar--narrow");
          bar.classList.remove("wx-bar--wide");
        } else if (w >= WIDE_THRESHOLD) {
          bar.classList.add("wx-bar--wide");
          bar.classList.remove("wx-bar--narrow");
        } else {
          bar.classList.remove("wx-bar--narrow");
          bar.classList.remove("wx-bar--wide");
        }
      });
    }

    const observer = new MutationObserver((mutations) => {
      if (mutations.some((m) => m.addedNodes.length > 0 || m.type === "attributes")) {
        classifyBars(container);
      }
    });

    observer.observe(container, {
      childList: true,
      subtree: true,
      attributes: true,
      attributeFilter: ["style"],
    });
    classifyBars(container);

    setupRef.current = { observer, container };
  });

  useEffect(() => {
    return () => {
      if (setupRef.current.observer) setupRef.current.observer.disconnect();
      setupRef.current = { observer: null, container: null };
    };
  }, []); // eslint-disable-line react-hooks/exhaustive-deps
}
