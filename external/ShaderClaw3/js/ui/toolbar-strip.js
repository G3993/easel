// ============================================================
// ShaderClaw 3 — Tab Bar Controller
// Manages tabbed panel (Canvas / Assets / Gallery)
// ============================================================

(function initToolbarStrip() {
  const propertiesPanel = document.querySelector('.sc3-properties');
  const tabs = document.querySelectorAll('.sc3-tab');
  const tabContents = document.querySelectorAll('.sc3-tab-content');

  // --- Move source panel sections into tab containers ---

  // Gallery → Gallery tab
  const gallerySection = document.getElementById('gallery-tab-content');
  const galleryTarget = document.querySelector('.sc3-tab-content[data-tab-content="gallery"]');
  if (gallerySection && galleryTarget) {
    gallerySection.style.display = '';
    gallerySection.classList.remove('collapsed');
    const body = gallerySection.querySelector('.sc3-section-body');
    if (body) body.style.display = '';
    galleryTarget.appendChild(gallerySection);
  }

  // Assets section → Assets tab
  const assetsSection = document.getElementById('inputs-section');
  const assetsTarget = document.querySelector('.sc3-tab-content[data-tab-content="assets"]');
  if (assetsSection && assetsTarget) {
    assetsSection.style.display = '';
    assetsSection.classList.remove('collapsed');
    assetsTarget.appendChild(assetsSection);
  }

  // Code Editor → inside Assets tab (appended after the assets section)
  const codeSection = document.getElementById('editor-section');
  if (codeSection && assetsTarget) {
    codeSection.style.display = '';
    codeSection.classList.remove('collapsed');
    const body = codeSection.querySelector('.sc3-section-body');
    if (body) body.style.display = '';
    assetsTarget.appendChild(codeSection);
  }

  // --- Wire up new content tile grid clicks ---
  const contentTiles = document.querySelectorAll('.sc3-content-tile[data-type]');
  contentTiles.forEach(tile => {
    tile.addEventListener('click', () => {
      const type = tile.dataset.type;
      // Map to existing hidden tile buttons for compat
      const tileMap = {
        image: 'tile-image',
        video: 'tile-video',
        model: 'tile-model',
        sound: 'tile-sound',
        mic: 'tile-mic',
        webcam: 'tile-webcam',
        ndi: 'tile-ndi',
        code: 'tile-code'
      };
      if (type === 'overlay') {
        const oi = document.getElementById('image-file-input');
        if (oi) oi.click();
      } else if (type === 'file') {
        const fi = document.getElementById('data-file-input') || document.getElementById('code-file-input');
        if (fi) fi.click();
      } else {
        const hiddenTile = document.getElementById(tileMap[type]);
        if (hiddenTile) hiddenTile.click();
      }
    });
  });

  // --- Tab switching ---
  function activateTab(tabName) {
    tabs.forEach(t => t.classList.toggle('active', t.dataset.tab === tabName));
    tabContents.forEach(tc => tc.classList.toggle('active', tc.dataset.tabContent === tabName));

    // CodeMirror needs a refresh when code section becomes visible
    if (tabName === 'assets') {
      const cm = document.querySelector('.CodeMirror');
      if (cm && cm.CodeMirror) {
        setTimeout(() => cm.CodeMirror.refresh(), 50);
      }
    }
  }

  // Tab click handlers
  tabs.forEach(tab => {
    tab.addEventListener('click', () => activateTab(tab.dataset.tab));
  });

  // Show properties panel by default — but collapsed on mobile (canvas-first)
  const _isMobileUI = window.innerWidth <= 900 || /Mobi|Android|iPhone/i.test(navigator.userAgent);
  if (propertiesPanel && !_isMobileUI) {
    propertiesPanel.classList.add('visible');
  }

  // Mobile: CapCut-style — centered stage, bottom dock of effects. Tapping a
  // dock item opens a sheet with just that effect's controls. Drag the pill
  // between half and full; drag down (or tap the active item) to close.
  if (_isMobileUI && propertiesPanel) {
    propertiesPanel.classList.add('visible', 'sheet-hidden');
    const pill = document.getElementById('panel-pill');
    const toggleBtn = document.getElementById('mobile-panel-toggle');
    const canvasTab = document.querySelector('.sc3-tab-content[data-tab-content="canvas"]');
    let sheetState = 'hidden';
    let activeDock = null;
    if (toggleBtn) toggleBtn.classList.add('hidden');

    function applyState(s) {
      sheetState = s;
      propertiesPanel.classList.toggle('sheet-hidden', s === 'hidden');
      propertiesPanel.classList.toggle('sheet-full', s === 'full');
      propertiesPanel.style.height = '';
      document.body.classList.toggle('sheet-half', s === 'half');
      document.body.classList.toggle('sheet-full', s === 'full');
      if (s === 'hidden') {
        activeDock = null;
        document.querySelectorAll('.dock-item').forEach(b => b.classList.remove('active'));
      }
    }

    function soloSection(sectionKey) {
      if (!canvasTab) return;
      canvasTab.classList.toggle('solo', !!sectionKey);
      propertiesPanel.classList.toggle('sheet-solo', !!sectionKey);
      canvasTab.querySelectorAll(':scope > .sc3-section').forEach(sec => {
        const match = sectionKey && sec.querySelector('[data-section="' + sectionKey + '"]');
        sec.classList.toggle('solo-target', !!match);
        if (match) sec.classList.remove('collapsed'); // open the controls
      });
      canvasTab.scrollTop = 0;
    }

    // Dock items
    const ASPECTS = [
      { label: '16:9', value: '1920x1080' },
      { label: '9:16', value: '1080x1920' },
      { label: '1:1',  value: '1080x1080' },
    ];
    let aspectIdx = 0;
    function applyAspect(i) {
      aspectIdx = ((i % ASPECTS.length) + ASPECTS.length) % ASPECTS.length;
      const a = ASPECTS[aspectIdx];
      const sel = document.getElementById('canvas-size-select');
      const lbl = document.getElementById('dock-aspect-label');
      if (lbl) lbl.textContent = a.label;
      if (sel) {
        sel.value = a.value;
        sel.dispatchEvent(new Event('change', { bubbles: true }));
      }
    }

    document.querySelectorAll('.dock-item').forEach(btn => {
      btn.addEventListener('click', () => {
        const key = btn.dataset.dock;
        if (key === 'expand') {
          document.body.classList.add('sc3-fs');
          return;
        }
        if (key === 'aspect') {
          applyAspect(aspectIdx + 1);
          return;
        }
        // Toggle off if tapping the already-active item
        if (activeDock === key && sheetState !== 'hidden') {
          applyState('hidden');
          return;
        }
        activeDock = key;
        document.querySelectorAll('.dock-item').forEach(b =>
          b.classList.toggle('active', b === btn));
        if (key.startsWith('tab:')) {
          soloSection(null);
          activateTab(key.slice(4));
        } else {
          activateTab('canvas');
          soloSection(key);
        }
        if (sheetState === 'hidden') applyState('half');
      });
    });

    // Fullscreen exit chip
    const fsExit = document.getElementById('fs-exit');
    if (fsExit) fsExit.addEventListener('click', () => document.body.classList.remove('sc3-fs'));

    // Default aspect on phones: vertical 9:16, once the renderer is ready
    const aspectPoll = setInterval(() => {
      if (window.getLayer && window.getLayer('shader') &&
          (window.getLayer('shader').program || window.getLayer('shader')._fluidActive)) {
        clearInterval(aspectPoll);
        if (window.innerHeight > window.innerWidth) applyAspect(1);
      }
    }, 400);
    setTimeout(() => clearInterval(aspectPoll), 20000);

    // Drag pill: follow the finger; snap to half or full, drag down to close
    if (pill) {
      let startY = 0, startH = 0, moved = false, activeId = null;
      function detents() {
        const vh = window.innerHeight;
        return { half: Math.round(vh * 0.44), full: vh - 132 };
      }
      pill.addEventListener('pointerdown', (e) => {
        activeId = e.pointerId;
        startY = e.clientY;
        startH = propertiesPanel.getBoundingClientRect().height;
        moved = false;
        propertiesPanel.classList.add('dragging');
        try { pill.setPointerCapture(e.pointerId); } catch (err) {}
      });
      pill.addEventListener('pointermove', (e) => {
        if (activeId !== e.pointerId) return;
        const dy = e.clientY - startY;
        if (Math.abs(dy) > 6) moved = true;
        if (!moved) return;
        const d = detents();
        const h = Math.max(40, Math.min(d.full, startH - dy));
        propertiesPanel.style.height = h + 'px';
      });
      function endDrag(e) {
        if (activeId !== e.pointerId) return;
        activeId = null;
        propertiesPanel.classList.remove('dragging');
        if (!moved) {
          // Tap: toggle half <-> full
          applyState(sheetState === 'full' ? 'half' : 'full');
          return;
        }
        const d = detents();
        const h = propertiesPanel.getBoundingClientRect().height;
        if (h < d.half * 0.6) { applyState('hidden'); return; }
        applyState(Math.abs(d.half - h) < Math.abs(d.full - h) ? 'half' : 'full');
      }
      pill.addEventListener('pointerup', endDrag);
      pill.addEventListener('pointercancel', endDrag);
    }

    // Mobile floating input buttons — proxy clicks to the real panel buttons
    document.querySelectorAll('.sc3-mobile-input-btn').forEach(btn => {
      btn.addEventListener('click', () => {
        const targetId = btn.dataset.target;
        const real = document.getElementById(targetId);
        if (real) real.click();
        // Mirror active state
        btn.classList.toggle('active');
      });
    });
  }

  // --- Keyboard shortcuts ---
  document.addEventListener('keydown', (e) => {
    if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA' || e.target.tagName === 'SELECT') return;
    if (e.target.closest('.CodeMirror')) return;

    if (e.key === 'g' && !e.ctrlKey && !e.metaKey) {
      activateTab('gallery');
    }
    if (e.key === 'e' && !e.ctrlKey && !e.metaKey) {
      activateTab('assets'); // code is inside assets now
    }
    if (e.key === 'p' && !e.ctrlKey && !e.metaKey) {
      activateTab('canvas');
    }
    if (e.key === 'Escape') {
      if (propertiesPanel) propertiesPanel.classList.toggle('visible');
    }
  });

  // --- Expose for other modules (backward compat) ---
  function showSourceSection(sectionId) {
    const map = { gallery: 'gallery', code: 'assets', assets: 'assets' };
    const tabName = map[sectionId] || 'canvas';
    if (propertiesPanel) propertiesPanel.classList.add('visible');
    activateTab(tabName);
  }

  function showAllSourceSections() {
    activateTab('canvas');
  }

  function updateToolbarState() {}

  window._toolbarStrip = {
    showSourceSection,
    showAllSourceSections,
    updateToolbarState,
  };
})();
