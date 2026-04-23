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

// ─── Toast 알림 헬퍼 ────────────────────────────────────────────
window.showToast = function (msg, type = "success") {
    const el = document.createElement('div');
    el.className = `alert alert-${type} position-fixed bottom-0 end-0 m-3`;
    el.style.zIndex = 9999;
    el.textContent = msg;
    document.body.appendChild(el);
    setTimeout(() => el.remove(), 3000);
};

// ─── 전역 확인 모달 ─────────────────────────────────────────────────────────
//
//  confirmAction(formId, title, message)
//    폼 제출 확인. "Confirm" 클릭 시 해당 formId 폼을 submit.
//
//  confirmAsync(title, message) → Promise<boolean>
//    JS 코드에서 await 로 사용. true = Confirm, false = Cancel.
//
(function () {
    let _modal = null;
    let _resolveAsync = null;

    function getModal() {
        if (!_modal) _modal = new bootstrap.Modal(document.getElementById('aci-confirm-modal'));
        return _modal;
    }

    function setup(title, message) {
        document.getElementById('aci-confirm-title').textContent = title;
        document.getElementById('aci-confirm-body').textContent  = message;
    }

    window.confirmAction = function (formId, title, message) {
        setup(title, message);
        const okBtn = document.getElementById('aci-confirm-ok');
        const newOk = okBtn.cloneNode(true);          // 이전 이벤트 제거
        okBtn.parentNode.replaceChild(newOk, okBtn);
        newOk.addEventListener('click', () => {
            getModal().hide();
            document.getElementById(formId)?.submit();
        });
        getModal().show();
    };

    window.confirmAsync = function (title, message) {
        return new Promise(resolve => {
            setup(title, message);
            _resolveAsync = resolve;
            const okBtn = document.getElementById('aci-confirm-ok');
            const newOk = okBtn.cloneNode(true);
            okBtn.parentNode.replaceChild(newOk, okBtn);
            newOk.addEventListener('click', () => {
                getModal().hide();
                if (_resolveAsync) { _resolveAsync(true); _resolveAsync = null; }
            });
            document.getElementById('aci-confirm-modal')
                .addEventListener('hidden.bs.modal', () => {
                    if (_resolveAsync) { _resolveAsync(false); _resolveAsync = null; }
                }, { once: true });
            getModal().show();
        });
    };
})();

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
