// ============================================================
// ShaderClaw — MediaPipe Body Part Registry & Binding Resolution
// ============================================================

const MP_BODY_PARTS = {
  hand: [
    { name: 'Wrist', index: 0 }, { name: 'Thumb Tip', index: 4 },
    { name: 'Index Tip', index: 8 }, { name: 'Middle Tip', index: 12 },
    { name: 'Ring Tip', index: 16 }, { name: 'Pinky Tip', index: 20 },
    { name: 'Palm Center', index: 9 },
  ],
  face: [
    { name: 'Nose Tip', index: 1 }, { name: 'Left Eye', index: 33 },
    { name: 'Right Eye', index: 263 }, { name: 'Mouth Center', index: 13 },
    { name: 'Chin', index: 152 }, { name: 'Forehead', index: 10 },
    { name: 'Left Ear', index: 234 }, { name: 'Right Ear', index: 454 },
    { name: 'Left Eyebrow', index: 70 }, { name: 'Right Eyebrow', index: 300 },
    { name: 'Upper Lip', index: 0 }, { name: 'Lower Lip', index: 17 },
    { name: 'Left Cheek', index: 123 }, { name: 'Right Cheek', index: 352 },
    { name: 'Nose Bridge', index: 6 },
  ],
  pose: [
    { name: 'Nose', index: 0 }, { name: 'Left Shoulder', index: 11 },
    { name: 'Right Shoulder', index: 12 }, { name: 'Left Elbow', index: 13 },
    { name: 'Right Elbow', index: 14 }, { name: 'Left Wrist', index: 15 },
    { name: 'Right Wrist', index: 16 }, { name: 'Left Hip', index: 23 },
    { name: 'Right Hip', index: 24 }, { name: 'Left Knee', index: 25 },
    { name: 'Right Knee', index: 26 }, { name: 'Left Ankle', index: 27 },
    { name: 'Right Ankle', index: 28 },
  ],
};

// Derived signals registry — organized by group, used by live signals UI and picker
const DERIVED_SIGNALS = {
  hand: [
    { name: 'Pinch Dist', key: 'pinchDist' },
    { name: 'Pinch', key: 'pinchHold' },
    { name: 'Grip Strength', key: 'gripStrength' },
    { name: 'Finger Spread', key: 'fingerSpread' },
    { name: 'Hand Angle', key: 'handAngle' },
    { name: 'Thumb Curl', key: 'thumbCurl' },
    { name: 'Index Curl', key: 'indexCurl' },
    { name: 'Middle Curl', key: 'middleCurl' },
    { name: 'Ring Curl', key: 'ringCurl' },
    { name: 'Pinky Curl', key: 'pinkyCurl' },
  ],
  face: [
    { name: 'Head Yaw', key: 'headYaw' },
    { name: 'Head Pitch', key: 'headPitch' },
    { name: 'Head Roll', key: 'headRoll' },
    { name: 'Mouth Open', key: 'mouthOpen' },
    { name: 'Left Blink', key: 'leftBlink' },
    { name: 'Right Blink', key: 'rightBlink' },
    { name: 'Eyebrow Raise', key: 'eyebrowRaise' },
  ],
  pose: [
    { name: 'Body Lean', key: 'bodyLean' },
    { name: 'Left Arm Angle', key: 'leftArmAngle' },
    { name: 'Right Arm Angle', key: 'rightArmAngle' },
    { name: 'Shoulder Width', key: 'shoulderWidth' },
  ],
};

const AUDIO_SIGNALS = [
  // --- Levels (smoothed amplitude per band) ---
  { name: 'Level (RMS)',   key: 'audioLevel' },
  { name: 'Bass',          key: 'audioBass' },
  { name: 'Mid',           key: 'audioMid' },
  { name: 'High',          key: 'audioHigh' },
  // --- Hits (transient/onset spikes, fast decay) ---
  { name: 'Bass Hit',      key: 'audioBassHit' },
  { name: 'Mid Hit',       key: 'audioMidHit' },
  { name: 'High Hit',      key: 'audioHighHit' },
  // --- Envelope followers (asymmetric attack/release) ---
  { name: 'Bass Env',      key: '_envBass' },
  { name: 'Mid Env',       key: '_envMid' },
  { name: 'High Env',      key: '_envHigh' },
  { name: 'Level Env',     key: '_envLevel' },
  // --- Audio-driven time (accumulates faster with louder signal) ---
  { name: 'Bass Time',     key: 'audioBassTime' },
  { name: 'Mid Time',      key: 'audioMidTime' },
  { name: 'High Time',     key: 'audioHighTime' },
];

const MOUSE_SIGNALS = [
  { name: 'Mouse X',    key: 'mouseX' },
  { name: 'Mouse Y',    key: 'mouseY' },
  { name: 'Mouse Down', key: 'mouseDown' },
];

const DATA_SIGNALS = [
  { name: 'Sine Wave',   key: 'dataSine',     fn: t => Math.sin(t * Math.PI * 2) * 0.5 + 0.5 },
  { name: 'Triangle',    key: 'dataTriangle',  fn: t => Math.abs(((t * 0.5) % 1) * 2 - 1) },
  { name: 'Sawtooth',    key: 'dataSawtooth',  fn: t => (t * 0.5) % 1 },
  { name: 'Square',      key: 'dataSquare',    fn: t => ((t * 0.5) % 1) < 0.5 ? 1 : 0 },
  { name: 'Random Walk', key: 'dataRandom' },
  { name: 'Noise',       key: 'dataNoise',     fn: () => Math.random() },
  { name: 'BPM Pulse',   key: 'dataBpm' },
  { name: 'Clock',       key: 'dataClock',     fn: t => (t % 60) / 60 },
];

const _dataManager = {
  values: {},
  _randomWalk: 0.5,
  _bpm: 120,
  _bpmPhase: 0,
  _lastTime: 0,
  expressions: {},    // id → compiled Function
  csvData: {},        // id → { data: Float32Array, index: 0 }

  update() {
    const now = performance.now() / 1000;
    const dt = this._lastTime ? (now - this._lastTime) : 0.016;
    this._lastTime = now;

    // Built-in generators
    for (const sig of DATA_SIGNALS) {
      if (sig.fn) {
        this.values[sig.key] = sig.fn(now);
      }
    }

    // Random walk (Brownian motion clamped 0-1)
    this._randomWalk += (Math.random() - 0.5) * 0.02;
    this._randomWalk = Math.max(0, Math.min(1, this._randomWalk));
    this.values.dataRandom = this._randomWalk;

    // BPM pulse: sharp attack + exponential decay
    this._bpmPhase += dt * (this._bpm / 60);
    const frac = this._bpmPhase % 1;
    this.values.dataBpm = Math.exp(-frac * 6);

    // Custom expressions
    for (const id in this.expressions) {
      try {
        this.values['expr_' + id] = Math.max(0, Math.min(1, this.expressions[id](now, now)));
      } catch (_) {
        this.values['expr_' + id] = 0;
      }
    }

    // CSV/JSON playback at 30fps, looping
    for (const id in this.csvData) {
      const csv = this.csvData[id];
      if (!csv.data || !csv.data.length) continue;
      const idx = Math.floor(now * 30) % csv.data.length;
      this.values['csv_' + id] = csv.data[idx];
    }
  },

  setExpression(id, expr) {
    try {
      this.expressions[id] = new Function('t', 'TIME', 'return ' + expr);
      // Test it
      this.expressions[id](0, 0);
      return null; // no error
    } catch (e) {
      delete this.expressions[id];
      return e.message;
    }
  },

  loadCSV(id, text, colIndex) {
    const lines = text.trim().split('\n');
    const vals = [];
    for (const line of lines) {
      const cols = line.split(/[,\t]/);
      const v = parseFloat(cols[colIndex || 0]);
      if (!isNaN(v)) vals.push(v);
    }
    if (!vals.length) return;
    // Normalize to 0-1
    const min = Math.min(...vals), max = Math.max(...vals);
    const range = max - min || 1;
    this.csvData[id] = { data: vals.map(v => (v - min) / range) };
  },

  loadJSON(id, arr) {
    if (!Array.isArray(arr) || !arr.length) return;
    const vals = arr.map(Number).filter(v => !isNaN(v));
    if (!vals.length) return;
    const min = Math.min(...vals), max = Math.max(...vals);
    const range = max - min || 1;
    this.csvData[id] = { data: vals.map(v => (v - min) / range) };
  }
};

/**
 * Apply easing, range mapping, and smoothing for a single binding.
 * rawSignal is 0-1 from the signal source. Stores _lastRawSignal for UI.
 */
function applyBindingValue(layer, b, rawSignal) {
  // Easing curve: transform 0-1 signal
  let v = Math.max(0, Math.min(1, rawSignal));
  const easing = b.easing || 'linear';
  if (easing === 'easeIn') v = v * v;
  else if (easing === 'easeOut') v = 1 - (1 - v) * (1 - v);
  else if (easing === 'easeInOut') v = v < 0.5 ? 2 * v * v : 1 - 2 * (1 - v) * (1 - v);
  else if (easing === 'spring') { const sm = b.smoothing || 0; v = Math.max(0, Math.min(1.2, 1 - Math.exp(-6 * v) * Math.cos(v * (8 + sm * 12) * Math.PI))); }
  else if (easing === 'custom' && b._bz) {
    // Cubic bezier easing with draggable control points
    const bz = b._bz;
    function _cbz(t, p0, p1, p2, p3) { const u=1-t; return u*u*u*p0+3*u*u*t*p1+3*u*t*t*p2+t*t*t*p3; }
    let t = v;
    for (let i = 0; i < 8; i++) {
      const cx = _cbz(t, 0, bz.x1, bz.x2, 1) - v;
      const dx = 3*(1-t)*(1-t)*bz.x1 + 6*(1-t)*t*(bz.x2-bz.x1) + 3*t*t*(1-bz.x2);
      if (Math.abs(dx) < 1e-6) break;
      t = Math.max(0, Math.min(1, t - cx / dx));
    }
    v = Math.max(0, Math.min(1.2, _cbz(t, 0, bz.y1, bz.y2, 1)));
  }

  // Range mapping
  let target = b.min + v * (b.max - b.min);

  // Per-binding EMA smoothing (attack/release asymmetric for natural feel)
  const smoothing = b.smoothing || 0;
  if (smoothing > 0 && b._smoothedValue != null) {
    const rising = target > b._smoothedValue;
    // Attack is faster than release for punchy but smooth response
    const attackAlpha = Math.pow(0.05, smoothing); // faster rise
    const releaseAlpha = Math.pow(0.01, smoothing); // slower decay
    const alpha = rising ? attackAlpha : releaseAlpha;
    target = b._smoothedValue + (target - b._smoothedValue) * (1 - alpha);
  }
  b._smoothedValue = target;
  b._lastRawSignal = rawSignal;

  layer.inputValues[b.param] = target;
}

/**
 * Resolve MediaPipe bindings for a layer: body part position → parameter value.
 * Supports binding types: landmark, derived, audio, mouse.
 */
function resolveBindings(layer, mediaPipeMgr, renderer) {
  if (!layer.mpBindings || !layer.mpBindings.length) return;
  for (const b of layer.mpBindings) {
    // Audio signal binding
    if (b.source === 'audio') {
      const sigs = {
        audioLevel, audioBass, audioMid, audioHigh,
        audioBassHit, audioMidHit, audioHighHit,
        _envBass, _envMid, _envHigh, _envLevel,
        // Time accumulators: use fractional part so they cycle 0-1
        audioBassTime: audioBassTime % 1,
        audioMidTime: audioMidTime % 1,
        audioHighTime: audioHighTime % 1,
      };
      const v = sigs[b.signalKey];
      if (v != null) applyBindingValue(layer, b, v);
      continue;
    }
    // Mouse signal binding
    if (b.source === 'mouse') {
      let v = 0;
      if (b.signalKey === 'mouseX') v = renderer ? renderer.mousePos[0] : 0.5;
      else if (b.signalKey === 'mouseY') v = renderer ? renderer.mousePos[1] : 0.5;
      else if (b.signalKey === 'mouseDown') v = renderer ? (renderer.mouseDown || 0) : 0;
      applyBindingValue(layer, b, v);
      continue;
    }
    // Data signal binding
    if (b.source === 'data') {
      let v = 0;
      if (b.dataType === 'expression') v = _dataManager.values['expr_' + b._bindId] || 0;
      else if (b.dataType === 'csv') v = _dataManager.values['csv_' + b._bindId] || 0;
      else v = _dataManager.values[b.signalKey] || 0;
      applyBindingValue(layer, b, v);
      continue;
    }
    // Derived signal binding
    if (b.source === 'derived') {
      const v = gestureProcessor.derived[b.signalKey];
      if (v != null) applyBindingValue(layer, b, v);
      continue;
    }
    // Landmark binding
    let pos = null;
    if (b.group === 'hand' && mediaPipeMgr.handCount > 0) {
      if (mediaPipeMgr._lastHandLandmarks && mediaPipeMgr._lastHandLandmarks[b.landmarkIndex]) {
        pos = mediaPipeMgr._lastHandLandmarks[b.landmarkIndex];
      } else if (b.landmarkIndex === 9) {
        pos = { x: mediaPipeMgr.handPos[0], y: mediaPipeMgr.handPos[1], z: mediaPipeMgr.handPos[2] };
      }
    } else if (b.group === 'face' && mediaPipeMgr._lastFaceLandmarks) {
      pos = mediaPipeMgr._lastFaceLandmarks[b.landmarkIndex];
    } else if (b.group === 'pose' && mediaPipeMgr._lastPoseLandmarks) {
      pos = mediaPipeMgr._lastPoseLandmarks[b.landmarkIndex];
    }
    if (!pos) continue;
    let v = (b.axis === 'y') ? pos.y : (b.axis === 'z') ? (pos.z || 0) : pos.x;
    applyBindingValue(layer, b, v);
  }
}
