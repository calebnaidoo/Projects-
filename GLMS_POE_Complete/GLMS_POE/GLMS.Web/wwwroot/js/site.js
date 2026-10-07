/**
 * GLMS — site.js
 * Handles:
 *  1. Live multi-currency converter (USD, EUR, GBP → ZAR)
 *  2. Auto-dismiss toast notifications
 *  3. File upload label updates
 */

// ── MULTI-CURRENCY LIVE CONVERTER ───────────────────────────────────────────
(function () {
    const amountInput   = document.getElementById('OriginalCost');
    const currencySelect= document.getElementById('Currency');
    const zarDisplay    = document.getElementById('zarResult');
    const statusEl      = document.getElementById('rateStatus');
    const zarHidden     = document.getElementById('CostZAR');
    const rateHidden    = document.getElementById('ExchangeRateToZAR');

    if (!amountInput || !zarDisplay) return;

    // Rates loaded from hidden fields (set by the server on page load)
    let rates = {
        USD: parseFloat(document.getElementById('UsdToZar')?.value || '18.5'),
        EUR: parseFloat(document.getElementById('EurToZar')?.value || '20.1'),
        GBP: parseFloat(document.getElementById('GbpToZar')?.value || '23.4')
    };

    function getCurrentRate() {
        const currency = (currencySelect?.value || 'USD').toUpperCase();
        return rates[currency] || rates['USD'];
    }

    function updateZar() {
        const amount = parseFloat(amountInput.value) || 0;
        const rate   = getCurrentRate();
        const currency = (currencySelect?.value || 'USD').toUpperCase();

        if (amount > 0 && rate > 0) {
            const zar = (amount * rate).toFixed(2);
            zarDisplay.textContent = 'R\u00a0' + Number(zar).toLocaleString('en-ZA', { minimumFractionDigits: 2 });
            if (zarHidden)  zarHidden.value  = zar;
            if (rateHidden) rateHidden.value = rate;
        } else {
            zarDisplay.textContent = 'R\u00a0—';
            if (zarHidden)  zarHidden.value  = '0';
        }

        // Update individual rate displays
        const rateDisplays = document.querySelectorAll('[data-rate-display]');
        rateDisplays.forEach(el => {
            const cur = el.dataset.rateDisplay;
            if (rates[cur]) el.textContent = rates[cur].toFixed(4);
        });
    }

    // Fetch fresh rates from the server (which calls ExchangeRate-API)
    function fetchRates() {
        if (statusEl) statusEl.textContent = 'Fetching live rates…';
        fetch('/ServiceRequests/GetRates')
            .then(r => r.json())
            .then(data => {
                rates.USD = data.usdToZar;
                rates.EUR = data.eurToZar;
                rates.GBP = data.gbpToZar;
                if (statusEl) statusEl.textContent = data.isLive ? '● Live' : '● Fallback';
                updateZar();
            })
            .catch(() => {
                if (statusEl) statusEl.textContent = '● Offline (fallback)';
            });
    }

    amountInput.addEventListener('input', updateZar);
    if (currencySelect) currencySelect.addEventListener('change', updateZar);

    fetchRates();
    // Refresh rates every 30 minutes
    setInterval(fetchRates, 30 * 60 * 1000);
})();

// ── AUTO-DISMISS TOASTS ──────────────────────────────────────────────────────
document.querySelectorAll('.toast').forEach(el => {
    setTimeout(() => {
        el.style.transition = 'opacity .4s ease';
        el.style.opacity = '0';
        setTimeout(() => el.remove(), 400);
    }, 5000);
});

// ── FILE UPLOAD LABEL ────────────────────────────────────────────────────────
document.querySelectorAll('input[type=file]').forEach(input => {
    input.addEventListener('change', () => {
        const area = input.closest('.file-drop');
        if (area) {
            const nameEl = area.querySelector('.file-drop-name');
            if (nameEl) nameEl.textContent = input.files[0]?.name ?? 'No file chosen';
        }
    });
});
