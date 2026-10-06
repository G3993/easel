// ============================================================
// ShaderClaw 3 — Slider fill sync
// Every input[type=range] gets a --fill custom property (0-100)
// so CSS can draw an iOS-style filled track. Covers sliders that
// exist at load, are added later, or are moved programmatically.
// ============================================================
(function sliderFill() {
  const tracked = new Set();

  function pct(el) {
    const min = parseFloat(el.min) || 0;
    const max = parseFloat(el.max);
    const span = (isNaN(max) ? 100 : max) - min;
    if (span <= 0) return 0;
    const v = (parseFloat(el.value) - min) / span * 100;
    return Math.max(0, Math.min(100, v));
  }

  function sync(el) {
    const p = pct(el);
    if (el._fillPct !== p) {
      el._fillPct = p;
      el.style.setProperty('--fill', p.toFixed(2));
    }
  }

  function track(el) {
    if (tracked.has(el)) return;
    tracked.add(el);
    sync(el);
  }

  function scan(root) {
    if (!root || !root.querySelectorAll) return;
    root.querySelectorAll('input[type="range"]').forEach(track);
    if (root.matches && root.matches('input[type="range"]')) track(root);
  }

  // Immediate feedback while dragging
  document.addEventListener('input', (e) => {
    if (e.target && e.target.type === 'range') sync(e.target);
  }, true);

  // New sliders added by the app (params, presets, signal widgets)
  const mo = new MutationObserver((muts) => {
    for (const m of muts) {
      for (const n of m.addedNodes) {
        if (n.nodeType === 1) scan(n);
      }
    }
  });

  function start() {
    scan(document);
    mo.observe(document.body, { childList: true, subtree: true });
    // Catch programmatic value changes (shader load, bindings, presets)
    setInterval(() => {
      tracked.forEach(el => { if (el.isConnected) sync(el); });
    }, 120);
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', start);
  } else {
    start();
  }
})();
