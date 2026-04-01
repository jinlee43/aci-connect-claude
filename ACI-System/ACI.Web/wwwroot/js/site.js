/* ================================================================
   ACI Project System – Site JavaScript
   ================================================================ */

// ─── Sidebar Toggle ──────────────────────────────────────────
(function () {
    const sidebar = document.getElementById('sidebar');
    const main = document.getElementById('mainContent');
    const btn = document.getElementById('sidebarToggle');
    if (!sidebar || !btn) return;

    const STORAGE_KEY = 'aci_sidebar_collapsed';
    const isMobile = () => window.innerWidth <= 768;

    function applyState(collapsed) {
        if (isMobile()) {
            sidebar.classList.toggle('mobile-open', !collapsed);
        } else {
            sidebar.classList.toggle('collapsed', collapsed);
            main?.classList.toggle('expanded', collapsed);
        }
    }

    // 저장된 상태 복원
    const saved = localStorage.getItem(STORAGE_KEY) === 'true';
    applyState(saved);

    btn.addEventListener('click', () => {
        const next = !sidebar.classList.contains(isMobile() ? 'mobile-open' : 'collapsed')
            ? (isMobile() ? false : true)
            : (isMobile() ? true : false);
        if (!isMobile()) localStorage.setItem(STORAGE_KEY, next);
        applyState(next);
    });

    // 모바일: 콘텐츠 클릭 시 사이드바 닫기
    main?.addEventListener('click', () => {
        if (isMobile() && sidebar.classList.contains('mobile-open')) {
            sidebar.classList.remove('mobile-open');
        }
    });
})();

// ─── HTMX 전역 설정 ─────────────────────────────────────────
if (typeof htmx !== 'undefined') {
    htmx.config.defaultSwapStyle = 'outerHTML';
    document.body.addEventListener('htmx:configRequest', e => {
        // CSRF 토큰 자동 첨부
        const token = document.querySelector('input[name="__RequestVerificationToken"]');
        if (token) e.detail.headers['RequestVerificationToken'] = token.value;
    });
}

// ─── ACI Gantt Helper ────────────────────────────────────────
window.ACIGantt = {
    /**
     * dhtmlxGantt 초기화
     * @param {string} containerId  - gantt 컨테이너 div id
     * @param {number} projectId    - 프로젝트 ID
     * @param {boolean} readonly    - 읽기전용 여부
     */
    init(containerId, projectId, readonly = false) {
        const container = document.getElementById(containerId);
        if (!container || typeof gantt === 'undefined') return;

        // ── 기본 설정 ──────────────────────────────────────
        gantt.config.date_format = "%m-%d-%Y %H:%i";
        gantt.config.xml_date = "%m-%d-%Y %H:%i";
        gantt.config.work_time = true;          // 주말 제외
        gantt.config.correct_work_time = true;
        gantt.config.fit_tasks = true;
        gantt.config.show_progress = true;
        gantt.config.drag_progress = !readonly;
        gantt.config.drag_resize = !readonly;
        gantt.config.drag_move = !readonly;
        gantt.config.readonly = readonly;

        // ── 그리드 컬럼 설정 ──────────────────────────────
        gantt.config.columns = [
            {
                name: "text", label: "Activity Name", tree: true,
                width: 220, resize: true,
                template: task => `<span title="${task.text}">${task.text}</span>`
            },
            {
                name: "start_date", label: "Start", align: "center",
                width: 90, resize: true
            },
            {
                name: "duration", label: "Dur.", align: "center",
                width: 55, resize: true
            },
            {
                name: "progress", label: "%", align: "center",
                width: 55, resize: true,
                template: task => Math.round(task.progress * 100) + "%"
            },
        ];

        // ── 타임스케일 (월 + 주) ──────────────────────────
        gantt.config.scales = [
            { unit: "month", step: 1, format: "%M %Y" },
            { unit: "week", step: 1, format: "W%W" },
        ];

        // ── 태스크 색상 (trade color 적용) ────────────────
        gantt.templates.task_class = (start, end, task) => {
            if (task.color) return '';
            return task.type === 'project' ? 'gantt-task-project' : '';
        };
        gantt.templates.task_text = (start, end, task) => {
            const pct = Math.round(task.progress * 100);
            return `${task.text} <b style="opacity:.7">${pct}%</b>`;
        };

        // ── 크리티컬 패스 ──────────────────────────────────
        gantt.plugins({ critical_path: true, tooltip: true, marker: true });
        gantt.config.highlight_critical_path = true;

        // ── Today marker ──────────────────────────────────
        gantt.addMarker({
            start_date: new Date(),
            css: "today-marker",
            text: "Today",
            title: new Date().toLocaleDateString()
        });

        // ── 베이스라인 ─────────────────────────────────────
        gantt.plugins({ baselines: false });  // GPL에서는 별도 구현 필요

        // ── 데이터 로드 & DataProcessor 설정 ──────────────
        gantt.init(containerId);
        gantt.load(`/api/gantt/projects/${projectId}/data`);

        if (!readonly) {
            const dp = gantt.createDataProcessor({
                url: `/api/gantt/projects/${projectId}`,
                mode: "REST",
                deleteAfterConfirmation: true
            });
            dp.attachEvent("onAfterUpdate", (id, action, tid, response) => {
                if (action === "error") {
                    console.error("Gantt save error:", response);
                    ACIGantt.showToast("Save failed. Please try again.", "danger");
                }
            });
        }
    },

    showToast(msg, type = "success") {
        const el = document.createElement('div');
        el.className = `alert alert-${type} position-fixed bottom-0 end-0 m-3`;
        el.style.zIndex = 9999;
        el.textContent = msg;
        document.body.appendChild(el);
        setTimeout(() => el.remove(), 3000);
    }
};

// ─── PPC 도넛 차트 ───────────────────────────────────────────
window.ACICharts = {
    renderPPC(containerId, ppc) {
        if (typeof ApexCharts === 'undefined') return;
        const opts = {
            chart: { type: 'radialBar', height: 160 },
            series: [ppc],
            labels: ['PPC'],
            colors: [ppc >= 80 ? '#22c55e' : ppc >= 60 ? '#f59e0b' : '#ef4444'],
            plotOptions: {
                radialBar: {
                    hollow: { size: '55%' },
                    dataLabels: {
                        name: { show: false },
                        value: { fontSize: '1.4rem', fontWeight: 700 }
                    }
                }
            }
        };
        new ApexCharts(document.getElementById(containerId), opts).render();
    },

    renderVarianceBar(containerId, variances) {
        if (typeof ApexCharts === 'undefined' || !variances.length) return;
        new ApexCharts(document.getElementById(containerId), {
            chart: { type: 'bar', height: 200, toolbar: { show: false } },
            series: [{ name: 'Count', data: variances.map(v => v.count) }],
            xaxis: { categories: variances.map(v => v.category) },
            colors: ['#f59e0b'],
            plotOptions: { bar: { borderRadius: 4, horizontal: true } },
            dataLabels: { enabled: false }
        }).render();
    }
};
