// ============================================================
// ShaderClaw — Parameter Controls (ISF INPUTS → UI)
// ============================================================

// Insert spaces at camelCase/snake_case/kebab-case word boundaries so a raw
// ISF NAME (no "LABEL" in the shader JSON) reads as words — e.g. "danceSpeed"
// -> "dance Speed" -> displayed as "Dance Speed" — instead of the raw
// unspaced camelCase the label used to fall back to.
function humanizeParamName(name) {
  const spaced = String(name)
    .replace(/[_-]+/g, ' ')
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .trim();
  return spaced.replace(/\w\S*/g, (w) => w[0].toUpperCase() + w.slice(1));
}

// Single source of truth for a param's display label: prefer the ISF JSON
// LABEL (already human-written) and fall back to a humanized NAME rather
// than the raw camelCase NAME.
function paramDisplayLabel(inp) {
  return inp.LABEL || humanizeParamName(inp.NAME);
}

// Detect which inputs are driven by audio/mouse/hand in shader source
function detectReactivity(shaderSource, inputs) {
  if (!shaderSource || !inputs) return {};
  // Strip JSON header
  const bodyStart = shaderSource.indexOf('*/');
  const body = bodyStart >= 0 ? shaderSource.slice(bodyStart + 2) : shaderSource;
  const lines = body.split('\n');

  const audioRe = /\b(audioLevel|audioBass|audioMid|audioHigh|audioFFT)\b/;
  const mouseRe = /\b(mousePos|mouseDelta)\b/;
  const handRe  = /\b(mpHandPos|mpHandPos2|mpHandCount|pinchHold|mpPinch|mpPinch2|mpFacePos|mpBodyPos)\b/;

  const result = {};
  inputs.forEach(inp => {
    const name = inp.NAME;
    const types = new Set();
    lines.forEach(line => {
      if (line.indexOf(name) !== -1) {
        if (audioRe.test(line)) types.add('audio');
        if (mouseRe.test(line)) types.add('mouse');
        if (handRe.test(line))  types.add('hand');
      }
    });
    if (types.size > 0) result[name] = [...types];
  });
  return result;
}

// Add a live signal bar under a control row
// === Range indicator helpers for bound parameters ===
function updateRangeIndicator(row, binding) {
  const wrap = row.querySelector('.slider-wrap');
  if (!wrap) return;
  const ind = wrap.querySelector('.bind-range-indicator');
  if (!ind) return;
  if (!binding) { ind.style.display = 'none'; return; }
  const pMin = binding._pMin != null ? binding._pMin : 0;
  const pMax = binding._pMax != null ? binding._pMax : 1;
  const range = pMax - pMin;
  if (range <= 0) { ind.style.display = 'none'; return; }
  const leftPct = ((binding.min - pMin) / range) * 100;
  const widthPct = ((binding.max - binding.min) / range) * 100;
  ind.style.display = 'block';
  ind.style.left = leftPct.toFixed(1) + '%';
  ind.style.width = widthPct.toFixed(1) + '%';
}

function updateSignalDot(row, rawSignal) {
  const wrap = row.querySelector('.slider-wrap');
  if (!wrap) return;
  const dot = wrap.querySelector('.bind-signal-dot');
  if (!dot) return;
  if (rawSignal == null) { dot.style.display = 'none'; return; }
  dot.style.display = 'block';
  dot.style.left = (Math.max(0, Math.min(1, rawSignal)) * 100).toFixed(1) + '%';
}

// Auto-bind reactive parameters with real bindings + signal rows
function markReactiveParams(container, shaderSource, inputs, layerId) {
  const reactivity = detectReactivity(shaderSource, inputs);
  const layer = window.getLayer ? window.getLayer(layerId) : null;
  if (!layer) return;
  if (!layer.mpBindings) layer.mpBindings = [];

  // Skip param names that shouldn't be auto-bound
  const skipNames = new Set(['opacity', 'blend', 'bgColor', 'transparentBg', 'fontFamily', 'fontWeight', 'font', 'msg']);

  // Helper: create a binding + signal row for a param
  function autoBind(name, source, signalKey) {
    if (layer.mpBindings.find(b => b.param === name)) return; // already bound
    const row = container.querySelector('.control-row[data-name="' + name + '"]');
    if (!row) return;
    const btn = row.querySelector('.bind-add-btn');
    if (!btn) return;
    const inp = inputs.find(i => i.NAME === name);
    const pMin = inp && inp.MIN != null ? inp.MIN : 0;
    const pMax = inp && inp.MAX != null ? inp.MAX : 1;
    // Safe default: bind range 0 → 0.2 of the full span (subtle)
    const fullSpan = pMax - pMin;
    const bMin = pMin;
    const bMax = pMin + fullSpan * 0.01;
    const binding = {
      param: name, source, signalKey,
      min: bMin, max: bMax,
      smoothing: 0.35, easing: 'easeOut',
      _autoReactive: true, _pMin: pMin, _pMax: pMax
    };
    layer.mpBindings.push(binding);
    btn.classList.add('linked', 'auto-reactive');
    btn.title = (source === 'mouse' ? 'Mouse' : source === 'audio' ? 'Audio' : 'Signal') + ' reactive — click to edit';
    if (window.ensureSignalRow) window.ensureSignalRow(row, binding, layerId);
    if (window._bus) window._bus.emit('binding:created', { layerId, binding });
  }

  // Collect reactive param names for reference
  const reactiveNames = new Set(Object.keys(reactivity));

  // Default: bind first reactive float param (or first float param) to mouseX
  // Mouse is always-on, no permission needed — best default signal
  // BUT skip if shader already reads mousePos/mouseDelta natively (avoids double-effect)
  const bodyStart = shaderSource.indexOf('*/');
  const body = bodyStart >= 0 ? shaderSource.slice(bodyStart + 2) : shaderSource;
  const hasNativeMouse = /\b(mousePos|mouseDelta)\b/.test(body);
  const isTextShader = /\bfontAtlasTex\b/.test(body) || layerId === 'text';

  if (!hasNativeMouse && !isTextShader) {
    const candidates = inputs.filter(i => i.TYPE === 'float' && !skipNames.has(i.NAME));
    // Prefer params the shader already uses reactively
    const reactiveFirst = candidates.sort((a, b) => {
      const aR = reactiveNames.has(a.NAME) ? 0 : 1;
      const bR = reactiveNames.has(b.NAME) ? 0 : 1;
      return aR - bR;
    });
    if (reactiveFirst.length > 0) {
      autoBind(reactiveFirst[0].NAME, 'mouse', 'mouseX');
    }
  }
}

function generateControls(inputs, container, onChange) {
  container.innerHTML = '';
  if (!inputs || inputs.length === 0) {
    container.innerHTML = '<div class="no-params">No parameters</div>';
    return {};
  }

  // If this container is inside the text layer card, skip msg text inputs
  // (the prominent bar handles msg)
  const isTextLayer = container.closest && container.closest('[data-layer="text"]');
  if (isTextLayer) {
    inputs = inputs.filter(inp => !(inp.TYPE === 'text' && inp.NAME === 'msg'));
  }
  // Collect default values for params whose UI controls we hide
  // (must happen BEFORE filtering them out of inputs)
  const hiddenDefaults = {};
  inputs.forEach(inp => {
    if (inp.NAME === 'bgColor') hiddenDefaults.bgColor = inp.DEFAULT || [0, 0, 0, 1];
    if (inp.NAME === 'transparentBg') hiddenDefaults.transparentBg = inp.DEFAULT != null ? !!inp.DEFAULT : true;
    if (isTextLayer && inp.NAME === 'textColor') hiddenDefaults.textColor = inp.DEFAULT || [1, 1, 1, 1];
  });
  // Now filter out the hidden params from UI rendering
  inputs = inputs.filter(inp => inp.NAME !== 'bgColor' && inp.NAME !== 'transparentBg');
  if (isTextLayer) {
    inputs = inputs.filter(inp => inp.NAME !== 'textColor');
  }

  // "Audio Reactivity" group renders first, ahead of every other group —
  // mirrors Easel, where these knobs sit directly under the master audio
  // controls. Stable partition: relative order within each half is kept.
  const audioFirst = inputs.filter(inp => inp.GROUP === 'Audio Reactivity');
  const restInputs = inputs.filter(inp => inp.GROUP !== 'Audio Reactivity');
  inputs = [...audioFirst, ...restInputs];

  const values = { ...hiddenDefaults };
  let imageInputIdx = 0;
  let lastGroup = null;

  inputs.forEach(inp => {
    // Subtle cluster label for a run of params sharing the same ISF "GROUP"
    // — not a collapsible section, just a small dim header. Params without
    // a GROUP never trigger this, so ungrouped shaders render exactly as
    // before this feature existed.
    if (inp.GROUP && inp.GROUP !== lastGroup) {
      const groupHeader = document.createElement('div');
      groupHeader.className = 'param-group-header';
      groupHeader.textContent = inp.GROUP;
      container.appendChild(groupHeader);
      lastGroup = inp.GROUP;
    }

    const row = document.createElement('div');
    row.className = 'control-row';
    row.dataset.name = inp.NAME;

    const label = document.createElement('label');
    const dispLabel = paramDisplayLabel(inp);
    label.textContent = dispLabel;
    label.title = dispLabel; // tooltip shows full name on hover
    row.appendChild(label);

    if (inp.TYPE === 'float') {
      const def = inp.DEFAULT != null ? inp.DEFAULT : 0.5;
      const min = inp.MIN != null ? inp.MIN : 0;
      const max = inp.MAX != null ? inp.MAX : 1;
      values[inp.NAME] = def;

      const range = document.createElement('input');
      range.type = 'range';
      range.min = min;
      range.max = max;
      range.step = (max - min) / 200;
      range.value = def;

      const valSpan = document.createElement('span');
      valSpan.className = 'val';
      valSpan.textContent = Number(def).toFixed(2);

      range.addEventListener('input', () => {
        const v = parseFloat(range.value);
        values[inp.NAME] = v;
        valSpan.textContent = v.toFixed(2);
        onChange(values);
      });

      // Bind-add button (dashed circle +) for linking to signal
      const addBtn = document.createElement('button');
      addBtn.className = 'bind-add-btn';
      addBtn.dataset.paramName = inp.NAME;
      addBtn.title = 'Link to signal';
      addBtn.textContent = '\u26A1\uFE0E'; // text presentation \u2014 monochrome, not emoji
      addBtn.addEventListener('click', (e) => {
        e.stopPropagation();
        window.openMpPicker(addBtn, inp.NAME, container);
      });
      row.appendChild(addBtn);

      // Wrap slider + range indicator
      const sliderWrap = document.createElement('div');
      sliderWrap.className = 'slider-wrap';
      const rangeInd = document.createElement('div');
      rangeInd.className = 'bind-range-indicator';
      rangeInd.style.display = 'none';
      const signalDot = document.createElement('div');
      signalDot.className = 'bind-signal-dot';
      signalDot.style.display = 'none';
      rangeInd.appendChild(signalDot);
      sliderWrap.appendChild(rangeInd);
      sliderWrap.appendChild(range);
      row.appendChild(sliderWrap);
      row.appendChild(valSpan);

    } else if (inp.TYPE === 'color' && inp.NAME === 'bgColor') {
      // Compact background color row: swatch | hex | opacity | eye
      const def = inp.DEFAULT || [0, 0, 0, 1];
      values[inp.NAME] = [...def];
      row.classList.add('bg-source-row');

      const panel = _buildBgColorRow(inp, def, values, onChange, container._bgSourceCallback);
      row.appendChild(panel);

    } else if (inp.TYPE === 'color') {
      const def = inp.DEFAULT || [1, 1, 1, 1];
      values[inp.NAME] = [...def];

      const hex = rgbToHex(def[0], def[1], def[2]);
      const picker = document.createElement('input');
      picker.type = 'color';
      picker.value = hex;

      picker.addEventListener('input', () => {
        const rgb = hexToRgb(picker.value);
        values[inp.NAME] = [rgb[0], rgb[1], rgb[2], def[3] || 1];
        onChange(values);
      });

      row.appendChild(picker);

    } else if (inp.TYPE === 'bool') {
      const def = inp.DEFAULT ? true : false;
      values[inp.NAME] = def;

      if (inp.NAME === 'transparentBg') {
        // Render as cam-switch toggle (same as shader layer toggle)
        row.className = 'cam-toggle-row';
        row.dataset.name = inp.NAME;
        const toggle = document.createElement('button');
        toggle.className = 'cam-switch' + (def ? ' active' : '');
        toggle.title = inp.LABEL || 'Transparent BG';
        toggle.addEventListener('click', () => {
          const on = !toggle.classList.contains('active');
          toggle.classList.toggle('active', on);
          values[inp.NAME] = on;
          onChange(values);
        });
        row.appendChild(toggle);
      } else {
        row.className = 'cam-toggle-row';
        row.dataset.name = inp.NAME;
        const toggle = document.createElement('button');
        toggle.className = 'cam-switch' + (def ? ' active' : '');
        toggle.addEventListener('click', () => {
          const on = !toggle.classList.contains('active');
          toggle.classList.toggle('active', on);
          values[inp.NAME] = on;
          onChange(values);
        });
        row.appendChild(toggle);
      }

    } else if (inp.TYPE === 'long' && inp.NAME === 'direction' && inp.LABELS &&
               inp.LABELS.some(l => /right|left|up|down/i.test(l))) {
      // Direction arrows instead of dropdown
      const vals = inp.VALUES || [];
      const labels = inp.LABELS || vals.map(String);
      const def = inp.DEFAULT != null ? inp.DEFAULT : (vals[0] || 0);
      values[inp.NAME] = def;
      const arrows = { right: '\u2192', left: '\u2190', up: '\u2191', down: '\u2193' };
      const group = document.createElement('div');
      group.className = 'direction-arrows';
      for (let i = 0; i < vals.length; i++) {
        const btn = document.createElement('button');
        const dir = labels[i].toLowerCase();
        btn.className = 'direction-arrow-btn' + (vals[i] === def ? ' active' : '');
        btn.textContent = arrows[dir] || labels[i];
        btn.title = labels[i];
        btn.dataset.value = vals[i];
        btn.dataset.dir = dir;
        btn.addEventListener('click', () => {
          group.querySelectorAll('.direction-arrow-btn').forEach(b => b.classList.remove('active'));
          btn.classList.add('active');
          values[inp.NAME] = parseFloat(btn.dataset.value);
          onChange(values);
        });
        group.appendChild(btn);
      }
      row.appendChild(group);

    } else if (inp.TYPE === 'long') {
      const vals = inp.VALUES || [];
      const labels = inp.LABELS || vals.map(String);
      const def = inp.DEFAULT != null ? inp.DEFAULT : (vals[0] || 0);
      values[inp.NAME] = def;

      const select = document.createElement('select');
      for (let i = 0; i < vals.length; i++) {
        const opt = document.createElement('option');
        opt.value = vals[i];
        opt.textContent = labels[i] || vals[i];
        if (vals[i] === def) opt.selected = true;
        select.appendChild(opt);
      }

      select.addEventListener('change', () => {
        values[inp.NAME] = parseFloat(select.value);
        onChange(values);
      });

      row.appendChild(select);

    } else if (inp.TYPE === 'text') {
      const maxLen = inp.MAX_LENGTH || 12;
      const def = (inp.DEFAULT || '').toUpperCase();

      function charToCode(ch) {
        if (!ch || ch === ' ') return 26;
        const code = ch.toUpperCase().charCodeAt(0);
        if (code >= 65 && code <= 90) return code - 65;
        if (code >= 48 && code <= 57) return code - 48 + 27;
        return 26;
      }

      for (let i = 0; i < maxLen; i++) {
        values[inp.NAME + '_' + i] = charToCode(def[i]);
      }
      values[inp.NAME + '_len'] = def.replace(/\s+$/, '').length;

      const textInput = document.createElement('input');
      textInput.type = 'text';
      textInput.maxLength = maxLen;
      textInput.value = def;
      textInput.spellcheck = false;

      textInput.addEventListener('input', () => {
        const str = textInput.value.toUpperCase();
        for (let i = 0; i < maxLen; i++) {
          values[inp.NAME + '_' + i] = charToCode(str[i]);
        }
        values[inp.NAME + '_len'] = str.replace(/\s+$/, '').length;
        onChange(values);
      });

      row.appendChild(textInput);

      // Mic button — toggles speech recognition into this text field
      const SpeechRec = window.SpeechRecognition || window.webkitSpeechRecognition;
      if (SpeechRec) {
        const micBtn = document.createElement('button');
        micBtn.className = 'text-mic-btn';
        micBtn.textContent = '\u{1F3A4}';
        micBtn.title = 'Mic input';
        let micRec = null;
        let micActive = false;

        function startMicRec() {
          micRec = new SpeechRec();
          micRec.continuous = true;
          micRec.interimResults = true;
          micRec.onresult = (event) => {
            let interim = '';
            let final = '';
            for (let i = event.resultIndex; i < event.results.length; i++) {
              if (event.results[i].isFinal) final += event.results[i][0].transcript;
              else interim += event.results[i][0].transcript;
            }
            const raw = (final || interim).trim();
            if (!raw) return;
            let str = raw.toUpperCase();
            if (str.length > maxLen) {
              // Cut to last maxLen chars, then skip to next full word
              str = str.slice(-maxLen);
              const sp = str.indexOf(' ');
              if (sp > 0) str = str.slice(sp + 1);
            }
            textInput.value = str;
            for (let i = 0; i < maxLen; i++) {
              values[inp.NAME + '_' + i] = charToCode(str[i]);
            }
            values[inp.NAME + '_len'] = str.replace(/\s+$/, '').length;
            onChange(values);
          };
          micRec.onerror = (e) => {
            console.warn('Mic error:', e.error);
            if (e.error === 'not-allowed' || e.error === 'aborted') {
              micActive = false;
              micBtn.classList.remove('active');
            }
          };
          micRec.onend = () => {
            if (micActive) {
              // Create fresh instance — Chrome won't restart a used one reliably
              setTimeout(startMicRec, 300);
            }
          };
          micRec.start();
        }

        micBtn.addEventListener('click', () => {
          if (micActive && micRec) {
            micActive = false;
            micRec.stop();
            micRec = null;
            micBtn.classList.remove('active');
            return;
          }
          micActive = true;
          micBtn.classList.add('active');
          startMicRec();
        });
        row.appendChild(micBtn);
      }

    } else if (inp.TYPE === 'image') {
      values[inp.NAME] = null; // texture binding, not a scalar value

      // Create select (always, even if no media yet — it refreshes dynamically)
      const select = document.createElement('select');
      select.dataset.imageInput = inp.NAME;
      select.classList.add('image-input-select');

      function _populateImageSelect() {
        const prev = select.value;
        select.innerHTML = '';
        const noneOpt = document.createElement('option');
        noneOpt.value = '';
        noneOpt.textContent = '(none)';
        select.appendChild(noneOpt);
        const compatibleMedia = mediaInputs.filter(m => m.type === 'image' || m.type === 'video' || m.type === 'svg');
        compatibleMedia.forEach(m => {
          const opt = document.createElement('option');
          opt.value = m.id;
          opt.textContent = mediaTypeIcon(m.type) + ' ' + m.name;
          select.appendChild(opt);
        });
        // --- Add content sources ---
        const addSep = document.createElement('option');
        addSep.disabled = true;
        addSep.textContent = '--- Add Source ---';
        select.appendChild(addSep);
        const addNdiOpt = document.createElement('option');
        addNdiOpt.value = '__add_ndi__';
        addNdiOpt.textContent = '\u{1f4e1} + NDI Input';
        select.appendChild(addNdiOpt);
        const addImageOpt = document.createElement('option');
        addImageOpt.value = '__add_image__';
        addImageOpt.textContent = '\u{1f5bc} + Image File';
        select.appendChild(addImageOpt);
        const addVideoOpt = document.createElement('option');
        addVideoOpt.value = '__add_video__';
        addVideoOpt.textContent = '\u{1f3ac} + Video File';
        select.appendChild(addVideoOpt);
        const addWebcamOpt = document.createElement('option');
        addWebcamOpt.value = '__add_webcam__';
        addWebcamOpt.textContent = '\u{1f4f7} + Webcam';
        select.appendChild(addWebcamOpt);

        // --- Layer outputs as texture sources ---
        const layerNames = { text: '\u{1f532} Text Layer', shader: '\u{1f532} Shader Layer', scene: '\u{1f532} 3D Layer' };
        const ownerEl = select.closest('.layer-params[data-layer]');
        const ownerId = ownerEl ? ownerEl.dataset.layer : null;
        const layerIds = ['text', 'shader', 'scene'].filter(id => id !== ownerId);
        if (layerIds.length) {
          const sep = document.createElement('option');
          sep.disabled = true;
          sep.textContent = '--- Layers ---';
          select.appendChild(sep);
          layerIds.forEach(id => {
            const opt = document.createElement('option');
            opt.value = 'layer:' + id;
            opt.textContent = layerNames[id];
            select.appendChild(opt);
          });
        }

        // Restore previous selection if still valid, otherwise auto-select
        const isLayerRef = prev && prev.startsWith('layer:');
        if (isLayerRef) {
          select.value = prev;
        } else if (prev && compatibleMedia.find(m => String(m.id) === String(prev))) {
          select.value = prev;
        } else if (values[inp.NAME] && String(values[inp.NAME]).startsWith('layer:')) {
          select.value = values[inp.NAME];
        } else if (values[inp.NAME] && compatibleMedia.find(m => String(m.id) === String(values[inp.NAME]))) {
          select.value = values[inp.NAME];
        }
      }
      _populateImageSelect();

      // No auto-select — user picks media explicitly via the dropdown
      imageInputIdx++;

      select.addEventListener('change', () => {
        const val = select.value;
        // Handle "add source" actions
        if (val === '__add_ndi__') {
          select.value = ''; // reset dropdown
          // Open NDI picker and auto-assign to this param when source is added
          const tileNdi = document.getElementById('tile-ndi');
          if (tileNdi) {
            // Stash callback so the NDI flow can assign back to this dropdown
            const _origAutoBindTextures = window._autoBindTextures;
            const _paramName = inp.NAME;
            const _onMediaAdded = () => {
              // After NDI source is added, find it and assign to this param
              const ndiMedia = mediaInputs.filter(m => m._isNdi);
              if (ndiMedia.length > 0) {
                const latest = ndiMedia[ndiMedia.length - 1];
                values[_paramName] = latest.id;
                if (select._refreshOptions) select._refreshOptions();
                select.value = latest.id;
                onChange(values);
              }
              // Unhook
              window.removeEventListener('ndi-source-added', _onMediaAdded);
            };
            window.addEventListener('ndi-source-added', _onMediaAdded);
            tileNdi.click();
          }
          return;
        }
        if (val === '__add_image__') {
          select.value = ''; // reset
          document.getElementById('image-file-input').click();
          return;
        }
        if (val === '__add_video__') {
          select.value = ''; // reset
          document.getElementById('video-file-input').click();
          return;
        }
        if (val === '__add_webcam__') {
          select.value = ''; // reset
          const camBtn = document.getElementById('cam-vis-btn');
          if (camBtn) camBtn.click();
          return;
        }
        values[inp.NAME] = val || null;
        onChange(values);
      });

      // Register for live refresh when media list changes
      select._refreshOptions = _populateImageSelect;

      row.appendChild(select);

    } else if (inp.TYPE === 'point2D') {
      const def = inp.DEFAULT || [0, 0];
      const min = inp.MIN || [-1, -1];
      const max = inp.MAX || [1, 1];
      values[inp.NAME] = [...def];

      for (let axis = 0; axis < 2; axis++) {
        const range = document.createElement('input');
        range.type = 'range';
        range.min = min[axis];
        range.max = max[axis];
        range.step = (max[axis] - min[axis]) / 200;
        range.value = def[axis];
        range.style.flex = '1';

        range.addEventListener('input', () => {
          values[inp.NAME][axis] = parseFloat(range.value);
          onChange(values);
        });

        row.appendChild(range);
      }
    }

    container.appendChild(row);
  });

  return values;
}

function rgbToHex(r, g, b) {
  const c = (v) => Math.round(Math.max(0, Math.min(1, v)) * 255).toString(16).padStart(2, '0');
  return '#' + c(r) + c(g) + c(b);
}

function hexToRgb(hex) {
  const r = parseInt(hex.slice(1, 3), 16) / 255;
  const g = parseInt(hex.slice(3, 5), 16) / 255;
  const b = parseInt(hex.slice(5, 7), 16) / 255;
  return [r, g, b];
}

// ============================================================
// Background Source Panel (for bgColor parameters)
// ============================================================

function _buildBgColorRow(inp, def, values, onChange, onBgSource) {
  const row = document.createElement('div');
  row.className = 'bg-color-row';

  let alpha = def[3] != null ? def[3] : 1;
  let visible = true;

  // Color swatch with hidden native picker
  const swatch = document.createElement('div');
  swatch.className = 'bg-color-swatch';
  swatch.style.background = rgbToHex(def[0], def[1], def[2]);
  const picker = document.createElement('input');
  picker.type = 'color';
  picker.value = rgbToHex(def[0], def[1], def[2]);
  swatch.appendChild(picker);

  // Hex input (no # prefix)
  const hexInput = document.createElement('input');
  hexInput.className = 'bg-color-hex';
  hexInput.type = 'text';
  hexInput.value = rgbToHex(def[0], def[1], def[2]).slice(1).toUpperCase();
  hexInput.maxLength = 6;
  hexInput.spellcheck = false;

  // Opacity display
  const opacitySpan = document.createElement('span');
  opacitySpan.className = 'bg-color-opacity';
  opacitySpan.textContent = Math.round(alpha * 100) + ' %';

  // Eye icon (visibility toggle)
  const eyeBtn = document.createElement('button');
  eyeBtn.className = 'bg-color-eye';
  eyeBtn.innerHTML = '<svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>';
  eyeBtn.title = 'Toggle background visibility';

  function applyColor(hex6) {
    const rgb = hexToRgb('#' + hex6);
    const a = visible ? alpha : 0;
    values[inp.NAME] = [rgb[0], rgb[1], rgb[2], a];
    swatch.style.background = '#' + hex6;
    picker.value = '#' + hex6;
    hexInput.value = hex6.toUpperCase();
    onChange(values);
    if (onBgSource) onBgSource(inp.NAME, { type: 'color' });
  }

  picker.addEventListener('input', () => {
    applyColor(picker.value.slice(1));
  });

  hexInput.addEventListener('change', () => {
    let v = hexInput.value.replace(/[^0-9a-fA-F]/g, '').padEnd(6, '0').slice(0, 6);
    applyColor(v);
  });

  eyeBtn.addEventListener('click', () => {
    visible = !visible;
    eyeBtn.classList.toggle('off', !visible);
    const cur = values[inp.NAME];
    values[inp.NAME] = [cur[0], cur[1], cur[2], visible ? alpha : 0];
    onChange(values);
    if (onBgSource) onBgSource(inp.NAME, { type: 'color' });
  });

  row.appendChild(swatch);
  row.appendChild(hexInput);
  row.appendChild(opacitySpan);
  row.appendChild(eyeBtn);
  return row;
}
