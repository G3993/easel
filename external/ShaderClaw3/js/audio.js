// Audio Analysis Pipeline (ES-module path — layers.js consumers)
//
// ALL analysis + smoothing lives in the shared EaselAudio core
// (js/easel-audio.js) so there is exactly ONE window._audioBus writer at
// runtime regardless of entry point. This module only owns its analyser /
// activeAudioEntry state and the FFT texture upload, mirroring js/media.js
// (the classic-script path index.html actually uses). The old local feature
// bus and its per-frame smoothing alphas are gone.

import './easel-audio.js'; // side-effect: attaches window.EaselAudio (idempotent)

let audioCtx = null;
let audioAnalyser = null;
let audioDataArray = null;
let audioFFTGLTexture = null;
let audioFFTThreeTexture = null;
let audioLevel = 0, audioBass = 0, audioMid = 0, audioHigh = 0;
let activeAudioEntry = null;
let _cachedBarEl = null;
let _cachedBarId = null;
let _lastIngestMs = 0;

function _engine() {
  return (typeof window !== 'undefined' ? window.EaselAudio : globalThis.EaselAudio) || null;
}

/**
 * Initialize audio context (call on user gesture)
 */
export function initAudioContext() {
  if (audioCtx) return audioCtx;
  audioCtx = new (window.AudioContext || window.webkitAudioContext)();
  audioAnalyser = audioCtx.createAnalyser();
  audioAnalyser.fftSize = 2048; // 1024 bins — real sub/air resolution
  audioDataArray = new Uint8Array(audioAnalyser.frequencyBinCount);
  return audioCtx;
}

/**
 * Connect a media element to the audio analyser
 */
export function connectAudioSource(mediaElement) {
  if (!audioCtx) initAudioContext();
  // Disconnect previous
  if (activeAudioEntry && activeAudioEntry._sourceNode) {
    try { activeAudioEntry._sourceNode.disconnect(); } catch(e) {}
  }
  const source = audioCtx.createMediaElementSource(mediaElement);
  source.connect(audioAnalyser);
  audioAnalyser.connect(audioCtx.destination);
  activeAudioEntry = { element: mediaElement, _sourceNode: source };
  return source;
}

/**
 * Set active audio entry directly (for mic, etc.)
 */
export function setActiveAudioEntry(entry) {
  activeAudioEntry = entry;
}

/**
 * Per-frame audio analysis — call from render loop.
 * Feeds the shared EaselAudio core and publishes window._audioBus.
 * @param {WebGLRenderingContext} gl
 */
export function updateAudioUniforms(gl) {
  const engine = _engine();
  const nowMs = performance.now();

  if (!audioAnalyser || !activeAudioEntry) {
    audioLevel = audioBass = audioMid = audioHigh = 0;
    if (engine && nowMs - _lastIngestMs >= 4) {
      _lastIngestMs = nowMs;
      engine.ingestSilence();
      engine.publishBus();
    }
    return;
  }

  if (nowMs - _lastIngestMs >= 4) {
    _lastIngestMs = nowMs;
    audioAnalyser.getByteFrequencyData(audioDataArray);
    const len = audioDataArray.length;

    // Raw RMS level (linear) for the engine's level AGC
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
      const fl = engine.publishBus().floats;
      audioLevel = fl.audioLevel;
      audioBass = fl.audioBass;
      audioMid = fl.audioMid;
      audioHigh = fl.audioHigh;
    } else {
      audioLevel = rawLevel;
    }

    // Upload FFT to GL texture (len x 1 LUMINANCE)
    if (!audioFFTGLTexture) {
      audioFFTGLTexture = gl.createTexture();
      gl.bindTexture(gl.TEXTURE_2D, audioFFTGLTexture);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_S, gl.CLAMP_TO_EDGE);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_WRAP_T, gl.CLAMP_TO_EDGE);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MIN_FILTER, gl.LINEAR);
      gl.texParameteri(gl.TEXTURE_2D, gl.TEXTURE_MAG_FILTER, gl.LINEAR);
    }
    gl.bindTexture(gl.TEXTURE_2D, audioFFTGLTexture);
    gl.texImage2D(gl.TEXTURE_2D, 0, gl.LUMINANCE, len, 1, 0, gl.LUMINANCE, gl.UNSIGNED_BYTE, audioDataArray);

    // Update THREE DataTexture (if THREE is available)
    if (window.THREE) {
      if (!audioFFTThreeTexture) {
        audioFFTThreeTexture = new THREE.DataTexture(audioDataArray, len, 1, THREE.LuminanceFormat);
        audioFFTThreeTexture.needsUpdate = true;
      } else {
        audioFFTThreeTexture.needsUpdate = true;
      }
    }
  }

  // Update audio bar UI (cached DOM ref to avoid querySelector per frame)
  if (activeAudioEntry && activeAudioEntry.id) {
    if (_cachedBarId !== activeAudioEntry.id) {
      _cachedBarEl = document.querySelector('.audio-bar-fill[data-audio-id="' + activeAudioEntry.id + '"]');
      _cachedBarId = activeAudioEntry.id;
    }
    if (_cachedBarEl) _cachedBarEl.style.width = (audioLevel * 100) + '%';
  }
}

/**
 * Get current audio levels (for external queries)
 */
export function getAudioLevels() {
  return {
    level: audioLevel,
    bass: audioBass,
    mid: audioMid,
    high: audioHigh,
    hasAudio: !!activeAudioEntry
  };
}

/**
 * Get audio state for passing to renderer
 */
export function getAudioState() {
  return {
    level: audioLevel,
    bass: audioBass,
    mid: audioMid,
    high: audioHigh,
    fftGLTexture: audioFFTGLTexture,
    fftThreeTexture: audioFFTThreeTexture,
  };
}

/**
 * Cleanup
 */
export function disposeAudio() {
  if (activeAudioEntry && activeAudioEntry._sourceNode) {
    try { activeAudioEntry._sourceNode.disconnect(); } catch(e) {}
  }
  activeAudioEntry = null;
  if (audioCtx) {
    try { audioCtx.close(); } catch(e) {}
    audioCtx = null;
  }
  audioAnalyser = null;
  audioDataArray = null;
  audioFFTGLTexture = null;
  audioFFTThreeTexture = null;
}
