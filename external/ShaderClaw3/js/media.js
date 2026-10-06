// ShaderClaw — Media Inputs + Audio + Texture Helpers

// ============================================================
// Media Inputs Store
// ============================================================

const mediaInputs = [];
let mediaIdCounter = 0;

// Global mask state (persists across shader switches)
let _maskMediaId = null;
let _maskMode = 0; // 0=off, 1=multiply, 2=invert

// Audio-reactive system.
// media.js OWNS the analyser + activeAudioEntry state; ALL analysis and
// smoothing lives in the shared EaselAudio core (js/easel-audio.js), which
// is the single window._audioBus writer. AGC replaces the old noise-floor
// gate + _AUDIO_GAIN; every envelope is dt-derived (no per-frame alphas).
let audioCtx = null;
let audioAnalyser = null;
let audioDataArray = null;
let audioFFTGLTexture = null;
let audioFFTThreeTexture = null;
let audioLevel = 0, audioBass = 0, audioMid = 0, audioHigh = 0;

// --- Binding-source signals (state.js AUDIO_SIGNALS + app.js live panels) ---
// Mirrored from the EaselAudio bus every analysis frame.
var audioBassHit = 0, audioMidHit = 0, audioHighHit = 0;
var audioBassTime = 0, audioMidTime = 0, audioHighTime = 0;
// Asymmetric envelope followers (fast attack ~10ms, release ~200ms, dt-derived)
var _envBass = 0, _envMid = 0, _envHigh = 0, _envLevel = 0;
let activeAudioEntry = null;
let _micAudioStream = null;
let _micAudioSourceNode = null;
let _micAudioEntry = null;

// Variable font texture system (for Text shader "Variable Font" effect)
let _vfCanvas = null;
let _vfCtx = null;
let _vfGLTexture = null;
let _vfLastMsg = '';
let _vfWeight = 400;
const _fontFamilies = [
  '"Inter", "Segoe UI Variable", "SF Pro", sans-serif',
  '"Times New Roman", "Times", Georgia, serif',
  '"Libre Caslon Text", "Palatino Linotype", "Book Antiqua", serif',
  '"Outfit", "Inter", "Segoe UI Variable", sans-serif',
];

function _getFontStack(inputValues) {
  const idx = Math.round(inputValues['fontFamily'] || 0);
  return _fontFamilies[idx] || _fontFamilies[0];
}

// Invalidate font texture cache when Google Fonts finish loading
if (typeof document !== 'undefined' && document.fonts) {
  document.fonts.ready.then(() => { _vfLastMsg = ''; _fontAtlasLastKey = ''; });
}

function updateVarFontTexture(gl, inputValues) {
  // Build msg from character uniforms
  const maxLen = 24;
  let msg = '';
  const msgLen = inputValues['msg_len'];
  const len = (msgLen != null && msgLen > 0) ? Math.min(msgLen, maxLen) : 0;
  for (let i = 0; i < len; i++) {
    const code = inputValues['msg_' + i];
    if (code == null || code === 26) msg += ' ';
    else if (code >= 0 && code <= 25) msg += String.fromCharCode(65 + code);
    else msg += ' ';
  }
  msg = msg.trim() || 'ETHEREA';

  // Sync weight from ISF param if available
  const iw = inputValues['fontWeight'];
  if (iw != null) _vfWeight = Math.max(100, Math.min(900, iw));

  // Only re-render canvas if text, weight, or font changed
  const fontStack = _getFontStack(inputValues);
  const key = msg + '|' + _vfWeight + '|' + fontStack;
  if (key === _vfLastMsg && _vfGLTexture) return;
  _vfLastMsg = key;

  if (!_vfCanvas) {
    _vfCanvas = document.createElement('canvas');
    _vfCanvas.width = 2048;
    _vfCanvas.height = 512;
    _vfCtx = _vfCanvas.getContext('2d');
  }

  const c = _vfCanvas;
  const ctx = _vfCtx;
  ctx.clearRect(0, 0, c.width, c.height);
  ctx.save();
  // Flip vertically for GL coord system
  ctx.translate(0, c.height);
  ctx.scale(1, -1);
  const w = Math.round(_vfWeight);
  ctx.font = w + ' ' + Math.round(c.height * 0.35) + 'px ' + fontStack;
  ctx.fillStyle = '#ffffff';
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';
  ctx.fillText(msg, c.width / 2, c.height / 2);
  ctx.restore();

  if (!_vfGLTexture) {
    _vfGLTexture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, _vfGLTexture);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
  } else {
    gl.bindTexture(gl.TEXTURE_2D, _vfGLTexture);
  }
  gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, c);
}

// Breathing texture: per-character variable font weight wave (effect 22)
// Renders each character at a staggered weight based on time, creating a
// "breathing" wave across the text (inspired by Splitting.js + variable fonts)
let _breatheStartTime = performance.now();

function updateBreathingTexture(gl, inputValues) {
  const maxLen = 24;
  let msg = '';
  const msgLen = inputValues['msg_len'];
  const len = (msgLen != null && msgLen > 0) ? Math.min(msgLen, maxLen) : 0;
  for (let i = 0; i < len; i++) {
    const code = inputValues['msg_' + i];
    if (code == null || code === 26) msg += ' ';
    else if (code >= 0 && code <= 25) msg += String.fromCharCode(65 + code);
    else msg += ' ';
  }
  msg = msg.trim() || 'ETHEREA';

  const fontStack = _getFontStack(inputValues);
  const spd = inputValues['speed'] != null ? inputValues['speed'] : 0.5;
  const intens = inputValues['intensity'] != null ? inputValues['intensity'] : 0.5;
  const elapsed = (performance.now() - _breatheStartTime) / 1000;

  // Weight range driven by intensity: center ± spread
  const baseWeight = inputValues['fontWeight'] != null ? inputValues['fontWeight'] : 400;
  const spread = intens * 400; // 0..400 range from center
  const minW = Math.max(100, baseWeight - spread);
  const maxW = Math.min(900, baseWeight + spread);

  // Stagger delay per char driven by density
  const dens = inputValues['density'] != null ? inputValues['density'] : 0.5;
  const charDelay = 0.15 + (1.0 - dens) * 0.85; // 0.15s (tight wave) to 1.0s (wide wave)

  if (!_vfCanvas) {
    _vfCanvas = document.createElement('canvas');
    _vfCanvas.width = 2048;
    _vfCanvas.height = 512;
    _vfCtx = _vfCanvas.getContext('2d');
  }

  const c = _vfCanvas;
  const ctx = _vfCtx;
  ctx.clearRect(0, 0, c.width, c.height);
  ctx.save();
  ctx.translate(0, c.height);
  ctx.scale(1, -1);

  const fontSize = Math.round(c.height * 0.35);
  ctx.fillStyle = '#ffffff';
  ctx.textAlign = 'left';
  ctx.textBaseline = 'middle';

  // Measure total width at mid-weight to center the text
  const midWeight = Math.round((minW + maxW) / 2);
  ctx.font = midWeight + ' ' + fontSize + 'px ' + fontStack;
  const totalWidth = ctx.measureText(msg).width;
  let x = (c.width - totalWidth) / 2;

  // Draw each character with its own staggered weight
  for (let i = 0; i < msg.length; i++) {
    const phase = elapsed * spd * Math.PI * 2 - i * charDelay;
    const t = (Math.sin(phase) + 1) / 2; // 0..1
    const w = Math.round(minW + t * (maxW - minW));

    ctx.font = w + ' ' + fontSize + 'px ' + fontStack;
    ctx.fillText(msg[i], x, c.height / 2);
    x += ctx.measureText(msg[i]).width;
  }

  ctx.restore();

  if (!_vfGLTexture) {
    _vfGLTexture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, _vfGLTexture);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
  } else {
    gl.bindTexture(gl.TEXTURE_2D, _vfGLTexture);
  }
  gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, c);
  // Invalidate varfont cache so switching back to effect 20 re-renders
  _vfLastMsg = '';
}

// Font atlas for bitmap effects (0-19) using web fonts
let _fontAtlasCanvas = null;
let _fontAtlasCtx = null;
let _fontAtlasGLTexture = null;
let _fontAtlasLastKey = '';

function updateFontAtlas(gl, inputValues) {
  const fontFamilyIdx = Math.round(inputValues['fontFamily'] || 0);
  // Always generate atlas (even for fontFamily=0) to avoid 26-branch charData() in shader
  const fontStack = _fontFamilies[fontFamilyIdx] || _fontFamilies[0];
  const weight = Math.round(inputValues['fontWeight'] || 400);
  const key = fontStack + '|' + weight + '|512';
  if (key === _fontAtlasLastKey && _fontAtlasGLTexture) return;
  _fontAtlasLastKey = key;

  const cellW = 384, cellH = 360;
  const ATLAS_CHARS = 37; // A-Z (0-25), space (26), 0-9 (27-36)
  const totalW = ATLAS_CHARS * cellW; // 37 * 384 = 14,208 (under 16,384 max)

  if (!_fontAtlasCanvas || _fontAtlasCanvas.width !== totalW) {
    _fontAtlasCanvas = document.createElement('canvas');
    _fontAtlasCanvas.width = totalW;
    _fontAtlasCanvas.height = cellH;
    _fontAtlasCtx = _fontAtlasCanvas.getContext('2d');
  }

  const c = _fontAtlasCanvas;
  const ctx = _fontAtlasCtx;
  ctx.clearRect(0, 0, c.width, c.height);
  ctx.save();
  // Flip vertically for GL coord system
  ctx.translate(0, c.height);
  ctx.scale(1, -1);

  const fontSize = Math.round(cellH * 0.85);
  ctx.font = weight + ' ' + fontSize + 'px ' + fontStack;
  ctx.fillStyle = '#ffffff';
  ctx.textAlign = 'center';
  ctx.textBaseline = 'middle';

  for (let i = 0; i < 26; i++) {
    ctx.fillText(String.fromCharCode(65 + i), (i + 0.5) * cellW, cellH / 2);
  }
  // Index 26 = space (leave blank)
  // Index 27-36 = digits 0-9
  for (let i = 0; i < 10; i++) {
    ctx.fillText(String(i), (27 + i + 0.5) * cellW, cellH / 2);
  }

  ctx.restore();

  if (!_fontAtlasGLTexture) {
    _fontAtlasGLTexture = gl.createTexture();
    gl.bindTexture(gl.TEXTURE_2D, _fontAtlasGLTexture);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
    gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
  } else {
    gl.bindTexture(gl.TEXTURE_2D, _fontAtlasGLTexture);
  }
  gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, c);
}

// Audio-reactive per-frame update (global scope — called from Renderer.render)
// Cached DOM elements for audio UI updates (avoid per-frame getElementById)
let _cachedAudioSignalFill = null;
let _cachedAudioLevelFill = null;
let _cachedAudioBarFill = null;
let _cachedAudioBarId = null;
let _audioFrameCount = 0;

// Analysis throttle: render loops call updateAudioUniforms several times per
// composited frame (main render + per layer). The bus is dt-derived so
// multiple steps stay correct, but re-analyzing the same analyser snapshot is
// wasted work — re-ingest at most every ~4ms.
let _lastAudioIngestMs = 0;

function updateAudioUniforms(gl) {
  const engine = (typeof window !== 'undefined') ? window.EaselAudio : null;
  const nowMs = performance.now();

  if (!audioAnalyser || !activeAudioEntry) {
    audioLevel = audioBass = audioMid = audioHigh = 0;
    audioBassHit = audioMidHit = audioHighHit = 0;
    _envBass = _envMid = _envHigh = _envLevel = 0;
    // Bus must exist and keep being written even with no source (all zeros,
    // Time clocks frozen) so consumers never see a null/stale bus.
    if (engine && nowMs - _lastAudioIngestMs >= 4) {
      _lastAudioIngestMs = nowMs;
      engine.ingestSilence();
      engine.publishBus();
    }
    // Only update DOM every 8th frame when idle
    if ((_audioFrameCount++ & 7) === 0) {
      if (!_cachedAudioSignalFill) _cachedAudioSignalFill = document.getElementById('audio-signal-fill');
      if (_cachedAudioSignalFill) _cachedAudioSignalFill.style.width = '0%';
      if (!_cachedAudioLevelFill) _cachedAudioLevelFill = document.getElementById('audio-level-fill');
      if (_cachedAudioLevelFill) _cachedAudioLevelFill.style.width = '0%';
    }
    return;
  }
  _audioFrameCount++;

  if (nowMs - _lastAudioIngestMs >= 4) {
    const dtSec = Math.min(0.1, Math.max(0.001, (_lastAudioIngestMs > 0 ? nowMs - _lastAudioIngestMs : 16.7) / 1000));
    _lastAudioIngestMs = nowMs;
    audioAnalyser.getByteFrequencyData(audioDataArray);
    const len = audioDataArray.length; // frequencyBinCount (1024 @ fftSize 2048)

    // Raw RMS level (linear) — the engine converts to dB for the level AGC
    let sum = 0;
    for (let i = 0; i < len; i++) sum += audioDataArray[i] * audioDataArray[i];
    const rawLevel = Math.sqrt(sum / len) / 255.0;

    if (engine) {
      engine.ingestSpectrum(audioDataArray, {
        sampleRate: audioCtx ? audioCtx.sampleRate : 44100,
        level: rawLevel,
        byteMinDb: audioAnalyser.minDecibels,
        byteMaxDb: audioAnalyser.maxDecibels,
      });
      const bus = engine.publishBus();
      const fl = bus.floats;
      // Legacy quartet: same names, same 0-1 ranges, AGC'd + visual-binding
      // conditioned (10/500ms == the house 0.85 feel) under the hood.
      audioLevel = fl.audioLevel;
      audioBass = fl.audioBass;
      audioMid = fl.audioMid;
      audioHigh = fl.audioHigh;
      // Binding-source mirrors (state.js AUDIO_SIGNALS)
      audioBassHit = fl.audioBassHit;
      audioMidHit = fl.audioMidHit;
      audioHighHit = fl.audioHighHit;
      audioBassTime = fl.audioBassTime;
      audioMidTime = fl.audioMidTime;
      audioHighTime = fl.audioHighTime;
      // Envelope followers (fast attack, ~200ms release) over the raw AGC'd
      // bands — punchier than the conditioned quartet, dt-derived.
      const raw = engine.raw, slewAR = engine.dsp.slewAR;
      _envBass = slewAR(_envBass, raw.bass, dtSec, 0.01, 0.2);
      _envMid = slewAR(_envMid, raw.mid, dtSec, 0.01, 0.2);
      _envHigh = slewAR(_envHigh, raw.high, dtSec, 0.01, 0.2);
      _envLevel = slewAR(_envLevel, raw.level, dtSec, 0.01, 0.2);
    } else {
      // Core script missing (should not happen in index.html) — minimal
      // unsmoothed fallback so the app still shows signal.
      const nyquist = (audioCtx ? audioCtx.sampleRate : 44100) / 2;
      const binAvg = (fLo, fHi) => {
        let a = Math.max(0, Math.floor((fLo / nyquist) * len));
        let b = Math.min(len, Math.ceil((fHi / nyquist) * len));
        if (b <= a) b = a + 1;
        let s = 0;
        for (let i = a; i < b; i++) s += audioDataArray[i];
        return s / ((b - a) * 255);
      };
      audioLevel = rawLevel;
      audioBass = binAvg(20, 250);
      audioMid = binAvg(250, 2000);
      audioHigh = binAvg(2000, 16000);
    }

    // Upload FFT data to GL texture (len x 1 LUMINANCE — layout unchanged)
    // Use a dedicated texture unit to avoid overwriting the currently active unit
    if (!audioFFTGLTexture) {
      audioFFTGLTexture = gl.createTexture();
      gl.activeTexture(gl.TEXTURE15);
      gl.bindTexture(gl.TEXTURE_2D, audioFFTGLTexture);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    }
    gl.activeTexture(gl.TEXTURE15);
    gl.bindTexture(gl.TEXTURE_2D, audioFFTGLTexture);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.LUMINANCE, len, 1, 0, gl.LUMINANCE, gl.UNSIGNED_BYTE, audioDataArray);

    // Update THREE DataTexture
    if (!audioFFTThreeTexture) {
      audioFFTThreeTexture = new THREE.DataTexture(audioDataArray, len, 1, THREE.LuminanceFormat);
      audioFFTThreeTexture.needsUpdate = true;
    } else {
      audioFFTThreeTexture.needsUpdate = true;
    }
  }

  // Update audio UI bars only every 4th frame (DOM writes are expensive)
  if ((_audioFrameCount & 3) === 0) {
    if (activeAudioEntry) {
      if (_cachedAudioBarId !== activeAudioEntry.id) {
        _cachedAudioBarFill = document.querySelector('.audio-bar-fill[data-audio-id="' + activeAudioEntry.id + '"]');
        _cachedAudioBarId = activeAudioEntry.id;
      }
      if (_cachedAudioBarFill) _cachedAudioBarFill.style.width = (audioLevel * 100) + '%';
    }
    if (!_cachedAudioSignalFill) _cachedAudioSignalFill = document.getElementById('audio-signal-fill');
    if (_cachedAudioSignalFill) _cachedAudioSignalFill.style.width = (audioLevel * 100) + '%';
    if (!_cachedAudioLevelFill) _cachedAudioLevelFill = document.getElementById('audio-level-fill');
    if (_cachedAudioLevelFill) _cachedAudioLevelFill.style.width = (audioLevel * 100) + '%';
  }
}

function detectMediaType(file) {
  const ext = file.name.split('.').pop().toLowerCase();
  if (['glb', 'gltf', 'stl', 'fbx', 'obj'].includes(ext)) return 'model';
  if (['mp3', 'wav', 'ogg'].includes(ext)) return 'audio';
  if (ext === 'svg') return 'svg';
  if (file.type.startsWith('video/')) return 'video';
  return 'image';
}

function mediaTypeIcon(type, name) {
  if (type === 'video' && name === 'Webcam') return '\u{1F4F9}';
  if (type === 'video') return '\u{1F3AC}';
  if (type === 'model') return '\u{1F9CA}';
  if (type === 'audio' && name === 'Microphone') return '\u{1F3A4}';
  if (type === 'audio') return '\u{1F50A}';
  if (type === 'svg') return '\u{2712}';
  return '\u{1F5BC}';
}

function createGLTexture(gl, source) {
  const tex = gl.createTexture();
  gl.bindTexture(gl.TEXTURE_2D, tex);
  gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, true);
  gl.texImage2D(gl.TEXTURE_2D, 0, gl.RGBA, gl.RGBA, gl.UNSIGNED_BYTE, source);
  gl.pixelStorei(gl.UNPACK_FLIP_Y_WEBGL, false);
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
  gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
  gl.bindTexture(gl.TEXTURE_2D, null);
  return tex;
}
