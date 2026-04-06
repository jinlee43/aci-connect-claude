import { createRoot } from "react-dom/client";
import BaselineGantt from "./BaselineGantt";
import ProgressGantt from "./ProgressGantt";

// ── Baseline Schedule 마운트 ──────────────────────────────────────────────
const baselineEl = document.getElementById("baseline-gantt-root");
if (baselineEl) {
  const projectId = parseInt(baselineEl.dataset.projectId, 10);
  const apiBase   = baselineEl.dataset.apiBase || "/api";
  const importUrl = baselineEl.dataset.importUrl || "";

  createRoot(baselineEl).render(
    <BaselineGantt
      projectId={projectId}
      apiBase={apiBase}
      importXmlUrl={importUrl}
    />
  );
}

// ── Current Schedule 마운트 ───────────────────────────────────────────────
const progressEl = document.getElementById("progress-gantt-root");
if (progressEl) {
  const projectId     = parseInt(progressEl.dataset.projectId, 10);
  const isInitialized = progressEl.dataset.initialized === "true";

  createRoot(progressEl).render(
    <ProgressGantt
      projectId={projectId}
      isInitialized={isInitialized}
    />
  );
}
