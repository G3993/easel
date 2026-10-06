// EaselAudio — unified audio-reactive analysis engine (ShaderClaw3 core)
//
// One analyzer core, one bus contract, ONE bus writer at runtime.
// Implements the EaselAudio spec v1.0 (docs/EASELAUDIO_SPEC.md +
// spec/easel_audio_bus.json):
//
//   - dB-domain AGC per band (noise floor gate, fast-grow / slow-shrink range)
//   - dt-derived conditioning (seconds, never per-frame alphas;
//     visual-binding preset 10ms/500ms == the house "smoothing 0.85" feel)
//   - temperament matrix per band: Level / Hit / Presence / Time
//   - dual-follower self-normalizing onsets (no absolute thresholds)
//   - autocorrelation tempo with confidence feeding ONE shared beat clock
//   - tier-1 pseudo-stems: log-band split + causal median HPSS on FFT frames
//
// Loaded as a CLASSIC script (index.html, before js/media.js) it attaches to
// window.EaselAudio. The ES-module path (js/audio.js) imports this same file
// for its side effect — the attach is idempotent, so both entry points share
// the single engine instance. media.js keeps ownership of the analyser +
// activeAudioEntry and feeds frames in via ingestSpectrum()/ingestSilence();
// publishBus() then writes the renderer's window._audioBus every frame
// (floats + vec2/vec3/vec4 groups, matching renderer.js _uploadAudioBus).
//
// Adapted from the proven etherea-ai port
// (etherea-ai/src/web-client/static/js/easel-audio.js).

(function (globalScope) {
  'use strict';

  // Idempotent: classic-script load + ES-module import must not double-init.
  if (globalScope && globalScope.EaselAudio) return;

  // ---------------------------------------------------------------------------
  // Constants (from spec/easel_audio_bus.json)
  // ---------------------------------------------------------------------------

  const LN10 = Math.LN10; // time-to-90% -> tau divisor (ln 10 ≈ 2.3026)

  const CONDITIONING = {
    // visual-binding role preset — the mandated smooth default
    attackSec: 0.01,
    releaseSec: 0.5,
    hardChangeThreshold: 0.35,
    microSlewSec: 0.003,
  };

  const AGC = {
    // Mix-level (true RMS) gate per spec
    levelFloorDb: -60,
    // Per-band FFT-bin energies sit far below mix RMS; -90 dB matches the
    // legacy per-band byte gates (byte ~30 on the WebAudio -100..-30 mapping).
    bandFloorDb: -90,
    initRangeDb: 50,
    minRangeDb: 20,
    maxRangeDb: 80,
    rangeGrowSec: 0.1,
    rangeShrinkSec: 60,
  };

  const HIT = { attackSec: 0.01, releaseSec: 0.35, releasePow: 1.6 };
  const PRESENCE = { attackSec: 1.5, releaseSec: 4.0 };
  const ONSET = {
    fastAttack: 0.01,
    fastRelease: 0.15,
    slowSec: 0.6,
    ratio: 1.4,
    bias: 0.02,
    refractorySec: 0.18,
  };

  const TEMPO = {
    sampleHz: 50, // onset envelope resample rate
    bufferLen: 512, // ~10s of onset history
    minBPM: 50,
    maxBPM: 220,
    recomputeSec: 2.0,
    confidenceFloor: 0.4, // below this, beat-locked signals fall back
  };

  const STEMS = {
    bins: 96, // log-spaced reduced spectrum for HPSS
    hpssFrames: 31, // causal (past-only) median kernel across time
    hpssFreqKernel: 31, // median kernel across frequency
    power: 2.0,
    margin: 2.5,
    releaseSec: 0.75, // +50% release vs mix bands (mask-flutter guard)
  };

  const FREQ_BANDS = {
    sub: [20, 60],
    bass: [20, 250],
    lowMid: [250, 1000],
    mid: [250, 2000],
    highMid: [1000, 4000],
    high: [2000, 16000],
    treble: [6000, 16000],
  };

  const DEFAULT_SAMPLE_RATE = 44100;
  const BYTE_MIN_DB = -100; // WebAudio getByteFrequencyData default mapping
  const BYTE_MAX_DB = -30;

  // ---------------------------------------------------------------------------
  // Small DSP helpers — everything dt-derived, in seconds
  // ---------------------------------------------------------------------------

  function clamp01(v) {
    return v < 0 ? 0 : v > 1 ? 1 : v;
  }

  function clamp(v, lo, hi) {
    return v < lo ? lo : v > hi ? hi : v;
  }

  /** One-pole slew where `sec` is time-to-90%. */
  function slew(current, target, dt, sec) {
    if (sec <= 0) return target;
    const k = 1 - Math.exp(-(dt * LN10) / sec);
    return current + (target - current) * k;
  }

  /** Asymmetric slew: attackSec rising, releaseSec falling. */
  function slewAR(current, target, dt, attackSec, releaseSec) {
    return slew(current, target, dt, target > current ? attackSec : releaseSec);
  }

  /**
   * Shared conditioning block (spec §2, lean subset):
   * attack/release in seconds + hard-change bypass + mandatory micro-slew.
   */
  function Conditioner(attackSec, releaseSec) {
    this.attackSec = attackSec != null ? attackSec : CONDITIONING.attackSec;
    this.releaseSec = releaseSec != null ? releaseSec : CONDITIONING.releaseSec;
    this.value = 0;
  }
  Conditioner.prototype.step = function (target, dt) {
    const delta = target - this.value;
    if (Math.abs(delta) > CONDITIONING.hardChangeThreshold) {
      // Notch rule: big deltas bypass smoothing (micro-slew still applies)
      this.value = slew(this.value, target, dt, CONDITIONING.microSlewSec);
    } else {
      this.value = slewAR(this.value, target, dt, this.attackSec, this.releaseSec);
    }
    return this.value;
  };

  /**
   * dB-domain AGC (spec §3): norm = clamp((dB - noiseFloor) / range, 0, 1)
   * with the noise floor doubling as the silence gate and the range
   * auto-tracked fast-grow / slow-shrink (no pumping). Replaces _AUDIO_GAIN
   * and every other magic gain.
   */
  function BandAGC(floorDb) {
    this.floorDb = floorDb != null ? floorDb : AGC.bandFloorDb;
    this.rangeDb = AGC.initRangeDb;
    this.clipping = false;
  }
  BandAGC.prototype.step = function (db, dt) {
    const above = db - this.floorDb;
    if (!(above > 0)) {
      // Gated: silence for this band. Range drifts back very slowly.
      this.rangeDb = slew(this.rangeDb, AGC.initRangeDb, dt, AGC.rangeShrinkSec);
      this.clipping = false;
      return 0;
    }
    const targetRange = clamp(above, AGC.minRangeDb, AGC.maxRangeDb);
    this.rangeDb = slewAR(this.rangeDb, targetRange, dt, AGC.rangeGrowSec, AGC.rangeShrinkSec);
    this.clipping = above > this.rangeDb * 1.1;
    return clamp01(above / this.rangeDb);
  };

  /**
   * Dual-follower self-normalizing onset detector (Ableton AnalysisGrabber
   * pattern): fast follower crossing slow follower * ratio => onset.
   * Drives an AD "Hit" envelope (attack 10ms pow 1.0, release 350ms pow 1.6).
   */
  function OnsetDetector(opts) {
    const o = opts || {};
    this.fast = 0;
    this.slow = 0;
    this.sinceOnset = 999;
    this.ratio = o.ratio != null ? o.ratio : ONSET.ratio;
    this.bias = o.bias != null ? o.bias : ONSET.bias;
    this.refractorySec = o.refractorySec != null ? o.refractorySec : ONSET.refractorySec;
    this.hitLin = 0; // linear AD state
    this.hitOut = 0; // shaped + slewed output
    this.strength = 0; // instantaneous onset strength (0 except on trigger frame)
  }
  OnsetDetector.prototype.step = function (env, dt) {
    this.fast = slewAR(this.fast, env, dt, ONSET.fastAttack, ONSET.fastRelease);
    this.slow = slew(this.slow, env, dt, ONSET.slowSec);
    this.sinceOnset += dt;
    this.strength = 0;
    if (this.fast > this.slow * this.ratio + this.bias && this.sinceOnset >= this.refractorySec) {
      this.strength = clamp01((this.fast - this.slow) * 2 + 0.25);
      this.hitLin = Math.max(this.hitLin, this.strength);
      this.sinceOnset = 0;
    }
    // AD envelope: linear release over releaseSec, shaped by releasePow
    this.hitLin = Math.max(0, this.hitLin - dt / HIT.releaseSec);
    const shaped = Math.pow(this.hitLin, HIT.releasePow);
    this.hitOut = slewAR(this.hitOut, shaped, dt, HIT.attackSec, 0.02);
    return this.hitOut;
  };

  function median(arr, n, scratch) {
    for (let i = 0; i < n; i++) scratch[i] = arr[i];
    const s = scratch.subarray(0, n);
    s.sort();
    return s[n >> 1];
  }

  // ---------------------------------------------------------------------------
  // Engine
  // ---------------------------------------------------------------------------

  function EaselAudioEngine() {
    this.bus = {}; // uniform name -> number (the schema bus)
    this.raw = {
      // instantaneous gated+AGC'd values (no temporal smoothing) — legacy
      level: 0,
      bass: 0,
      mid: 0,
      high: 0,
      sub: 0,
      treble: 0,
      centroid: 0,
    };
    this.indicator = 'silent'; // 'ok' | 'clip' | 'silent' (3-state, spec §3)
    this.globals = { intensity: 0.22, character: -0.5, manualGainDb: 0 };

    // Band machinery (lowMid/highMid feed the legacy bus names)
    this._bandNames = ['sub', 'bass', 'lowMid', 'mid', 'highMid', 'high', 'treble'];
    this._agc = {};
    this._cond = {}; // visual-binding conditioned band values
    this._onset = {};
    this._presence = {};
    for (const b of this._bandNames) {
      this._agc[b] = new BandAGC(AGC.bandFloorDb);
      this._cond[b] = new Conditioner();
      this._onset[b] = new OnsetDetector();
      this._presence[b] = new Conditioner(PRESENCE.attackSec, PRESENCE.releaseSec);
    }
    this._levelAgc = new BandAGC(AGC.levelFloorDb);
    this._levelCond = new Conditioner();
    this._levelOnset = new OnsetDetector();
    this._levelPresence = new Conditioner(PRESENCE.attackSec, PRESENCE.releaseSec);
    this._energyCond = new Conditioner(0.3, 0.3); // meters preset — macro feel
    this._brightCond = new Conditioner();
    this._punchCond = new Conditioner(0.01, 0.25);

    // Time clocks (monotonic, pause in silence)
    this._time = { level: 0, bass: 0, mid: 0, high: 0 };

    // Shared beat clock / rhythm bus
    this._beatsFloat = 0;
    this._bpm = 0;
    this._bpmConfidence = 0;
    this._externalBPM = null; // { bpm, confidence, ageSec }
    this._beatPulse = 0;
    this._sinceBeatEvent = 999;
    this._onBeatT = 999;
    this._toggle = 0;
    this._toggleOut = 0;

    // Tempo tracking (onset envelope autocorrelation)
    this._onsetRing = new Float32Array(TEMPO.bufferLen);
    this._onsetRingPos = 0;
    this._onsetRingCount = 0;
    this._resampleAcc = 0;
    this._sinceTempo = 0;

    // Reduced log spectrum + HPSS state
    this._reduced = new Float32Array(STEMS.bins); // linear magnitudes
    this._reducedPrev = new Float32Array(STEMS.bins);
    this._hpssRing = new Float32Array(STEMS.hpssFrames * STEMS.bins);
    this._hpssPos = 0;
    this._hpssCount = 0;
    this._medScratch = new Float32Array(Math.max(STEMS.hpssFrames, STEMS.hpssFreqKernel));
    this._harm = new Float32Array(STEMS.bins);
    this._perc = new Float32Array(STEMS.bins);
    this._binEdges = null; // recomputed on sampleRate / binCount change
    this._binEdgeKey = '';
    this._vocalFluxEnv = 0;
    this._fluxOut = 0; // broadband positive-flux envelope (legacy audioFlux)

    this._stemNames = ['stemBass', 'stemDrums', 'stemMelody', 'stemAir', 'stemVocal'];
    this._stemAgc = {};
    this._stemCond = {};
    this._stemOnset = {};
    this._stemPresence = {};
    for (const s of this._stemNames) {
      this._stemAgc[s] = new BandAGC(AGC.bandFloorDb);
      this._stemCond[s] = new Conditioner(CONDITIONING.attackSec, STEMS.releaseSec);
      this._stemOnset[s] = new OnsetDetector(
        s === 'stemDrums' ? { ratio: 1.25, bias: 0.01 } : null
      );
      this._stemPresence[s] = new Conditioner(PRESENCE.attackSec, PRESENCE.releaseSec);
    }

    // Spectrum state
    this._spectrumDb = null; // last full-resolution dB spectrum
    this._spectrumLen = 0;
    this._sampleRate = DEFAULT_SAMPLE_RATE;

    this._lastNow = 0;
    this._dt = 1 / 60; // last resolved dt (used by publishBus derivations)
    this._zeroBus();
  }

  // --- public API -------------------------------------------------------------

  /**
   * Feed one spectrum frame. Exactly one of `floatDb` (getFloatFrequencyData
   * output, dB) or `bytes` (getByteFrequencyData output, 0-255) is required.
   *
   * opts: { sampleRate, dt, level (linear rms 0..1), byteMinDb, byteMaxDb }
   */
  EaselAudioEngine.prototype.ingestSpectrum = function (input, opts) {
    const o = opts || {};
    const dt = this._resolveDt(o.dt);
    const sampleRate = o.sampleRate || this._sampleRate || DEFAULT_SAMPLE_RATE;
    this._sampleRate = sampleRate;

    let db = null;
    let len = 0;
    if (input && input.length) {
      len = input.length;
      if (!this._dbScratch || this._dbScratch.length !== len) {
        this._dbScratch = new Float32Array(len);
      }
      db = this._dbScratch;
      const gain = this.globals.manualGainDb || 0;
      if (input instanceof Float32Array) {
        for (let i = 0; i < len; i++) {
          const v = input[i];
          db[i] = (Number.isFinite(v) ? v : -160) + gain;
        }
      } else {
        const minDb = o.byteMinDb != null ? o.byteMinDb : BYTE_MIN_DB;
        const maxDb = o.byteMaxDb != null ? o.byteMaxDb : BYTE_MAX_DB;
        const scale = (maxDb - minDb) / 255;
        for (let i = 0; i < len; i++) {
          const byte = input[i];
          db[i] = (byte <= 0 ? -160 : minDb + byte * scale) + gain;
        }
      }
    }
    this._spectrumDb = db;
    this._spectrumLen = len;

    this._step(db, len, sampleRate, dt, o.level);
    return this.bus;
  };

  /** Feed silence (source stopped / no data) — everything decays, clocks freeze. */
  EaselAudioEngine.prototype.ingestSilence = function (dt) {
    this._spectrumDb = null;
    this._spectrumLen = 0;
    this._step(null, 0, this._sampleRate, this._resolveDt(dt), 0);
    return this.bus;
  };

  /** Feed a mix level only (no spectrum) — level AGC + the shared beat clock. */
  EaselAudioEngine.prototype.ingestLevel = function (level, dt) {
    this._step(null, 0, this._sampleRate, this._resolveDt(dt), clamp01(level || 0));
    return this.bus;
  };

  /**
   * External tempo hint. The engine remains the single beat clock; hints are
   * reconciled with confidence.
   */
  EaselAudioEngine.prototype.setExternalBPM = function (bpm, confidence) {
    if (!(bpm > 0)) return;
    this._externalBPM = {
      bpm,
      confidence: confidence != null ? clamp01(confidence) : 0.9,
      ageSec: 0,
    };
  };

  EaselAudioEngine.prototype.getState = function () {
    return {
      bpm: this._bpm,
      bpmConfidence: this._bpmConfidence,
      indicator: this.indicator,
      raw: this.raw,
      bus: this.bus,
    };
  };

  /**
   * ShaderClaw bus publisher — writes window._audioBus in the exact shape
   * renderer.js _uploadAudioBus consumes ({ floats, vec2, vec3, vec4 }).
   * Copies the schema bus verbatim, then derives the legacy extended names
   * (isf.js AUDIO_BUS_FLOATS/VECS: affect, structure, palette anchors) from
   * the same core signals so older shaders keep their inputs. Exactly one
   * exception: `audioPresence` is a FLOAT on the core bus (spec) but a VEC4
   * in the long-standing GLSL contract — the float stays core-only and the
   * vec4 [bass, mid, high, energy] is published for shaders.
   */
  EaselAudioEngine.prototype.publishBus = function (globalObj) {
    const g = globalObj || globalScope;
    const b = this.bus;
    const dt = this._dt || 1 / 60;
    if (!this._shaderBus) {
      this._shaderBus = {
        floats: {},
        vec2: { audioMood: [0.5, 0], audioFlow: [0, 0] },
        vec3: {
          audioPalShadow: [0, 0, 0],
          audioPalMid: [0, 0, 0],
          audioPalHigh: [0, 0, 0],
          audioPalAccent: [0, 0, 0],
        },
        vec4: { audioPresence: [0, 0, 0, 0] },
        _writes: 0,
      };
    }
    const out = this._shaderBus;
    const f = out.floats;
    for (const k in b) {
      if (k === 'audioPresence') continue; // GLSL contract: vec4 (see above)
      f[k] = b[k];
    }

    if (!this._lg) {
      this._lg = {
        prevEnergy: 0, vel: 0, prevVel: 0, buildup: 0, energyPrevHigh: 0,
        spread: 0, flat: 0, tilt: 0, onsetRate: 0,
        valence: 0.5, arousal: 0, tension: 0, warmth: 0, softness: 1, rough: 0,
        layers: 0,
      };
    }
    const L = this._lg;
    const bass = b.audioBass, mid = b.audioMid, high = b.audioHigh;
    const sub = b.audioSub, treble = b.audioTreble;
    const energy = b.audioEnergy, bright = b.audioBrightness;

    // Energy dynamics / structure
    const velRaw = clamp((energy - L.prevEnergy) / Math.max(dt, 1e-3), -1, 1);
    L.vel = slewAR(L.vel, velRaw, dt, 0.05, 0.3);
    const acc = clamp((L.vel - L.prevVel) / Math.max(dt, 1e-3), -1, 1);
    L.prevVel = L.vel;
    L.prevEnergy = energy;
    L.buildup = slewAR(L.buildup, Math.max(0, L.vel), dt, 0.4, 3.0);
    L.energyPrevHigh = Math.max(energy, slew(L.energyPrevHigh, 0, dt, 8));
    f.audioEnergyVel = L.vel;
    f.audioEnergyAcc = acc;
    f.audioBuildup = L.buildup;
    f.audioBuildupRate = Math.max(0, L.vel);
    f.audioDrop = energy < 0.08 && L.energyPrevHigh > 0.4 ? 1 : 0;
    f.audioTempo01 = clamp01((b.audioBPM - 60) / 120);

    // Spectral character (cheap approximations, dt-conditioned)
    L.spread = slewAR(L.spread, clamp01(Math.abs(treble - sub) + Math.abs(b.audioHighMid - b.audioLowMid)), dt, 0.05, 0.4);
    f.audioSpread = L.spread;
    f.audioRolloff = bright;
    L.flat = slewAR(L.flat, 1 - Math.abs(bass - high), dt, 0.1, 0.5);
    f.audioFlatness = L.flat;
    f.audioTexture = L.flat;
    f.audioFlux = this._fluxOut;
    L.onsetRate = slewAR(L.onsetRate, b.audioOnset, dt, 0.1, 2.0);
    f.audioOnsetRate = L.onsetRate;
    L.tilt = slewAR(L.tilt, bass - high, dt, 0.1, 0.5);
    f.audioTilt = L.tilt;
    f.audioZCR = bright;

    // Affect / mood (heuristic but musical)
    L.valence = slewAR(L.valence, clamp01(0.5 + 0.5 * (bright - 0.35) + 0.2 * (b.audioBeatPulse - 0.3)), dt, 0.2, 0.6);
    L.arousal = slewAR(L.arousal, clamp01(energy * 1.4 + L.onsetRate * 0.4), dt, 0.2, 0.6);
    L.tension = slewAR(L.tension, clamp01(L.buildup * 1.5), dt, 0.2, 0.6);
    L.warmth = slewAR(L.warmth, clamp01(bass * 1.2 * (1 - bright * 0.5)), dt, 0.2, 0.6);
    L.softness = slewAR(L.softness, Math.max(0, 1 - L.onsetRate * 2), dt, 0.2, 0.6);
    L.rough = slewAR(L.rough, clamp01(this._fluxOut * 1.5), dt, 0.2, 0.6);
    f.audioValence = L.valence;
    f.audioArousal = L.arousal;
    f.audioTension = L.tension;
    f.audioWarmth = L.warmth;
    f.audioSoftness = L.softness;
    f.audioRoughness = L.rough;
    f.audioCharm = L.valence * L.softness;
    f.audioNovelty = b.audioOnset;
    f.audioSectionPhase = b.audioBarPhase;
    f.audioSectionAge = clamp01(this._sinceBeatEvent / 16);
    L.layers = slewAR(L.layers, clamp01((bass > 0.1 ? 0.34 : 0) + (mid > 0.1 ? 0.33 : 0) + (high > 0.1 ? 0.33 : 0)), dt, 0.3, 1.0);
    f.audioLayers = L.layers;
    f.audioDensity = L.layers;
    f.audioDominantPitch = bright;
    f.audioMajorMinor = L.valence;
    f.audioHCDF = this._fluxOut;

    // Palette anchors (warmth/brightness driven — same recipe as before)
    const warm = L.warmth;
    f.audioPalTemp = warm;
    f.audioPalSat = clamp01(0.4 + energy);
    const ps = out.vec3.audioPalShadow, pm = out.vec3.audioPalMid;
    const ph = out.vec3.audioPalHigh, pa = out.vec3.audioPalAccent;
    ps[0] = 0.02 + warm * 0.05; ps[1] = 0.02; ps[2] = 0.05 + (1 - warm) * 0.06;
    pm[0] = 0.2 + warm * 0.5; pm[1] = 0.15 + bright * 0.3; pm[2] = 0.5 - warm * 0.25 + bright * 0.2;
    ph[0] = 0.6 + warm * 0.4; ph[1] = 0.55 + bright * 0.4; ph[2] = 0.75 + (1 - warm) * 0.25;
    pa[0] = 0.9; pa[1] = 0.25 + bright * 0.55; pa[2] = 0.35 + (1 - warm) * 0.5;

    const mood = out.vec2.audioMood, flow = out.vec2.audioFlow, pres = out.vec4.audioPresence;
    mood[0] = L.valence; mood[1] = L.arousal;
    flow[0] = L.tilt; flow[1] = L.vel;
    pres[0] = bass; pres[1] = mid; pres[2] = high; pres[3] = energy;

    out._writes++;
    if (g) g._audioBus = out;
    return out;
  };

  // --- internals ---------------------------------------------------------------

  EaselAudioEngine.prototype._resolveDt = function (dt) {
    if (dt != null && Number.isFinite(dt)) return clamp(dt, 0.001, 0.1);
    const now = (typeof performance !== 'undefined' ? performance.now() : Date.now()) / 1000;
    const out = this._lastNow > 0 ? now - this._lastNow : 1 / 60;
    this._lastNow = now;
    return clamp(out, 0.001, 0.1);
  };

  EaselAudioEngine.prototype._bandDb = function (db, len, sampleRate, fLo, fHi) {
    const nyquist = sampleRate / 2;
    let a = Math.floor((fLo / nyquist) * len);
    let b = Math.ceil((fHi / nyquist) * len);
    a = clamp(a, 0, len - 1);
    b = clamp(b, a + 1, len);
    let power = 0;
    for (let i = a; i < b; i++) {
      power += Math.pow(10, db[i] / 10);
    }
    power /= b - a;
    return power > 0 ? 10 * Math.log10(power) : -160;
  };

  EaselAudioEngine.prototype._step = function (db, len, sampleRate, dt, levelLinear) {
    const bus = this.bus;
    this._dt = dt;

    // ---- band levels (dB domain) --------------------------------------------
    const bandDb = {};
    let centroid = 0;
    if (db && len > 0) {
      for (const b of this._bandNames) {
        const [lo, hi] = FREQ_BANDS[b];
        bandDb[b] = this._bandDb(db, len, sampleRate, lo, hi);
      }
      // Spectral centroid (linear magnitude weighted), normalized vs 8kHz
      let wsum = 0;
      let msum = 0;
      const nyquist = sampleRate / 2;
      for (let i = 0; i < len; i++) {
        const m = Math.pow(10, db[i] / 20);
        wsum += m * ((i / len) * nyquist);
        msum += m;
      }
      centroid = msum > 1e-9 ? clamp01(wsum / msum / 8000) : 0;
    } else {
      for (const b of this._bandNames) bandDb[b] = -160;
    }

    // Mix level: prefer true RMS when the caller has it, else derive from bands
    let levelDb;
    if (levelLinear != null) {
      levelDb = levelLinear > 0 ? 20 * Math.log10(levelLinear) : -160;
    } else if (db && len > 0) {
      // Bands sit in the per-bin domain; approximate mix level from them
      levelDb = Math.max(bandDb.bass, bandDb.mid, bandDb.high) - (AGC.bandFloorDb - AGC.levelFloorDb);
    } else {
      levelDb = -160;
    }

    // ---- AGC + temperaments ---------------------------------------------------
    const rawBands = {};
    let anySignal = false;
    let anyClip = false;
    for (const b of this._bandNames) {
      rawBands[b] = this._agc[b].step(bandDb[b], dt);
      if (rawBands[b] > 0.001) anySignal = true;
      if (this._agc[b].clipping) anyClip = true;
    }
    const rawLevel = this._levelAgc.step(levelDb, dt);
    if (rawLevel > 0.001) anySignal = true;

    this.raw.level = rawLevel;
    this.raw.sub = rawBands.sub;
    this.raw.bass = rawBands.bass;
    this.raw.mid = rawBands.mid;
    this.raw.high = rawBands.high;
    this.raw.treble = rawBands.treble;
    this.raw.centroid = centroid;
    this.indicator = !anySignal ? 'silent' : anyClip ? 'clip' : 'ok';

    // Conditioned (visual-binding 10/500ms — the smooth default)
    bus.audioSub = this._cond.sub.step(rawBands.sub, dt);
    bus.audioBass = this._cond.bass.step(rawBands.bass, dt);
    bus.audioLowMid = this._cond.lowMid.step(rawBands.lowMid, dt);
    bus.audioMid = this._cond.mid.step(rawBands.mid, dt);
    bus.audioHighMid = this._cond.highMid.step(rawBands.highMid, dt);
    bus.audioHigh = this._cond.high.step(rawBands.high, dt);
    bus.audioTreble = this._cond.treble.step(rawBands.treble, dt);
    bus.audioLevel = this._levelCond.step(rawLevel, dt);
    bus.audioEnergy = this._energyCond.step(rawLevel, dt);
    bus.audioBrightness = this._brightCond.step(centroid, dt);

    // Onset envelopes run on fixed-span gated dB (self-normalizing ratio test)
    const span = 60;
    const env = {};
    for (const b of this._bandNames) {
      env[b] = clamp01((bandDb[b] - AGC.bandFloorDb) / span);
    }
    const envLevel = clamp01((levelDb - AGC.levelFloorDb) / span);

    bus.audioBassHit = this._onset.bass.step(env.bass, dt);
    bus.audioMidHit = this._onset.mid.step(env.mid, dt);
    bus.audioHighHit = this._onset.high.step(env.high, dt);
    this._levelOnset.step(envLevel, dt);
    // Hidden extra bands still tick their followers (used by punch/onset)
    this._onset.sub.step(env.sub, dt);
    this._onset.treble.step(env.treble, dt);

    bus.audioPunch = this._punchCond.step(
      Math.max(this._onset.sub.hitOut, this._onset.bass.hitOut),
      dt
    );
    bus.audioOnset = Math.max(
      this._onset.bass.strength,
      this._onset.mid.strength,
      this._onset.high.strength,
      this._levelOnset.strength
    );

    bus.audioBassPresence = this._presence.bass.step(rawBands.bass, dt);
    bus.audioMidPresence = this._presence.mid.step(rawBands.mid, dt);
    bus.audioHighPresence = this._presence.high.step(rawBands.high, dt);
    bus.audioPresence = this._levelPresence.step(rawLevel, dt);

    // Time clocks: integrate the conditioned bands — pause in silence
    this._time.bass += dt * bus.audioBass;
    this._time.mid += dt * bus.audioMid;
    this._time.high += dt * bus.audioHigh;
    this._time.level += dt * bus.audioLevel;
    bus.audioBassTime = this._time.bass;
    bus.audioMidTime = this._time.mid;
    bus.audioHighTime = this._time.high;
    bus.audioTime = this._time.level;

    // ---- stems (tier-1: log-band split + causal median HPSS) ------------------
    this._stepStems(db, len, sampleRate, dt, bus);

    // ---- rhythm: onset envelope -> tempo -> ONE shared beat clock --------------
    this._stepRhythm(dt, bus, db != null && len > 0);
  };

  EaselAudioEngine.prototype._ensureBinEdges = function (len, sampleRate) {
    const key = len + ':' + sampleRate;
    if (this._binEdgeKey === key && this._binEdges) return;
    this._binEdgeKey = key;
    const nyquist = sampleRate / 2;
    const fLo = 20;
    const fHi = Math.min(16000, nyquist);
    const edges = new Int32Array(STEMS.bins + 1);
    for (let i = 0; i <= STEMS.bins; i++) {
      const f = fLo * Math.pow(fHi / fLo, i / STEMS.bins);
      edges[i] = clamp(Math.round((f / nyquist) * len), 0, len);
    }
    // Guarantee monotonically increasing, at least 1 bin each
    for (let i = 1; i <= STEMS.bins; i++) {
      if (edges[i] <= edges[i - 1]) edges[i] = Math.min(len, edges[i - 1] + 1);
    }
    this._binEdges = edges;
    // Precompute reduced-bin center frequencies
    const centers = new Float32Array(STEMS.bins);
    for (let i = 0; i < STEMS.bins; i++) {
      centers[i] = fLo * Math.pow(fHi / fLo, (i + 0.5) / STEMS.bins);
    }
    this._binCenters = centers;
  };

  EaselAudioEngine.prototype._stepStems = function (db, len, sampleRate, dt, bus) {
    const R = STEMS.bins;
    const reduced = this._reduced;

    if (db && len > 0) {
      this._ensureBinEdges(len, sampleRate);
      const edges = this._binEdges;
      for (let i = 0; i < R; i++) {
        let sum = 0;
        const a = edges[i];
        const b = Math.max(edges[i + 1], a + 1);
        for (let j = a; j < b && j < len; j++) sum += Math.pow(10, db[j] / 20);
        reduced[i] = sum / (b - a);
      }
    } else {
      reduced.fill(0);
    }

    // Push into causal history ring
    const F = STEMS.hpssFrames;
    this._hpssRing.set(reduced, this._hpssPos * R);
    this._hpssPos = (this._hpssPos + 1) % F;
    if (this._hpssCount < F) this._hpssCount++;

    // Harmonic estimate: median across time (past-only). Percussive: median
    // across frequency at the current frame.
    const nT = this._hpssCount;
    const scratch = this._medScratch;
    const harm = this._harm;
    const perc = this._perc;
    if (!this._medScratch2) this._medScratch2 = new Float32Array(F);
    if (!this._medScratch3) this._medScratch3 = new Float32Array(STEMS.hpssFreqKernel + 1);
    for (let i = 0; i < R; i++) {
      for (let t = 0; t < nT; t++) scratch[t] = this._hpssRing[t * R + i];
      harm[i] = median(scratch, nT, this._medScratch2);
    }
    const K = STEMS.hpssFreqKernel;
    const half = K >> 1;
    for (let i = 0; i < R; i++) {
      let n = 0;
      for (let j = Math.max(0, i - half); j < Math.min(R, i + half + 1); j++) {
        scratch[n] = reduced[j];
        n++;
      }
      perc[i] = median(scratch, n, this._medScratch3);
    }

    // Soft masks (power 2.0, margin 2.5) -> component magnitudes
    const p = STEMS.power;
    const margin = STEMS.margin;
    const centers = this._binCenters;
    let percSum = 0;
    let melodySum = 0;
    let airSum = 0;
    let vocalSum = 0;
    let vocalFlux = 0;
    let totalFlux = 0;
    let melodyN = 0;
    let airN = 0;
    let vocalN = 0;
    for (let i = 0; i < R; i++) {
      const m = reduced[i];
      const hp = Math.pow(harm[i], p);
      const pp = Math.pow(perc[i], p);
      const denomH = hp + margin * pp;
      const denomP = pp + margin * hp;
      const hMask = denomH > 1e-20 ? hp / denomH : 0;
      const pMask = denomP > 1e-20 ? pp / denomP : 0;
      const f = centers ? centers[i] : 0;
      percSum += m * pMask;
      const dAll = m - this._reducedPrev[i];
      if (dAll > 0) totalFlux += dAll;
      if (f >= 120 && f < 2000) {
        melodySum += m * hMask;
        melodyN++;
      }
      if (f >= 2000) {
        airSum += m * hMask;
        airN++;
      }
      if (f >= 2000 && f < 6000) {
        vocalSum += m * hMask;
        vocalN++;
        if (dAll > 0) vocalFlux += dAll;
      }
    }
    this._reducedPrev.set(reduced);

    // Vocal flux gate: modulation in the 2-6k harmonic region reads as voice
    this._vocalFluxEnv = slewAR(this._vocalFluxEnv, clamp01(vocalFlux * 400), dt, 0.05, 0.8);
    // Broadband positive-flux envelope (legacy audioFlux / roughness driver)
    this._fluxOut = slewAR(this._fluxOut, clamp01(totalFlux * 12), dt, 0.05, 0.4);

    const toDb = (mag) => (mag > 0 ? 20 * Math.log10(mag) : -160);
    const stemMag = {
      stemBass: this._bandDbFromReduced(reduced, centers, 20, 120),
      stemDrums: toDb(percSum / Math.max(1, R)),
      stemMelody: toDb(melodyN > 0 ? melodySum / melodyN : 0),
      stemAir: toDb(airN > 0 ? airSum / airN : 0),
      stemVocal: toDb(vocalN > 0 ? (vocalSum / vocalN) * this._vocalFluxEnv : 0),
    };

    const span = 60;
    for (const s of this._stemNames) {
      const norm = this._stemAgc[s].step(stemMag[s], dt);
      bus[s] = this._stemCond[s].step(norm, dt);
      const env = clamp01((stemMag[s] - AGC.bandFloorDb) / span);
      bus[s + 'Hit'] = this._stemOnset[s].step(env, dt);
      bus[s + 'Presence'] = this._stemPresence[s].step(norm, dt);
    }
  };

  EaselAudioEngine.prototype._bandDbFromReduced = function (reduced, centers, fLo, fHi) {
    if (!centers) return -160;
    let sum = 0;
    let n = 0;
    for (let i = 0; i < STEMS.bins; i++) {
      if (centers[i] >= fLo && centers[i] < fHi) {
        sum += reduced[i];
        n++;
      }
    }
    return n > 0 && sum > 0 ? 20 * Math.log10(sum / n) : -160;
  };

  EaselAudioEngine.prototype._stepRhythm = function (dt, bus, hasSpectrum) {
    // Onset-strength envelope, bass-weighted, resampled at a fixed rate
    const strength =
      this._onset.bass.strength * 1.5 +
      this._onset.mid.strength * 0.5 +
      this._levelOnset.strength * 0.5;
    this._resampleAcc += dt;
    const period = 1 / TEMPO.sampleHz;
    this._pendingOnset = Math.max(this._pendingOnset || 0, strength);
    while (this._resampleAcc >= period) {
      this._resampleAcc -= period;
      this._onsetRing[this._onsetRingPos] = this._pendingOnset;
      this._onsetRingPos = (this._onsetRingPos + 1) % TEMPO.bufferLen;
      if (this._onsetRingCount < TEMPO.bufferLen) this._onsetRingCount++;
      this._pendingOnset = 0;
    }

    // Recompute tempo periodically
    this._sinceTempo += dt;
    if (this._sinceTempo >= TEMPO.recomputeSec && this._onsetRingCount >= 128) {
      this._sinceTempo = 0;
      const est = this._autocorrelateTempo();
      if (est) {
        // Smooth toward the estimate; snap when far off
        if (this._bpm <= 0 || Math.abs(est.bpm - this._bpm) > 15) {
          this._bpm = est.bpm;
        } else {
          this._bpm = slew(this._bpm, est.bpm, TEMPO.recomputeSec, 4);
        }
        this._bpmConfidence = slew(this._bpmConfidence, est.confidence, TEMPO.recomputeSec, 4);
      } else {
        this._bpmConfidence = slew(this._bpmConfidence, 0, TEMPO.recomputeSec, 8);
      }
    }

    // External hint reconciles into the same clock
    if (this._externalBPM) {
      this._externalBPM.ageSec += dt;
      if (this._externalBPM.ageSec > 12) {
        this._externalBPM = null;
      } else if (this._externalBPM.confidence >= this._bpmConfidence) {
        this._bpm = this._bpm > 0 ? slew(this._bpm, this._externalBPM.bpm, dt, 1.5) : this._externalBPM.bpm;
        this._bpmConfidence = Math.max(this._bpmConfidence, this._externalBPM.confidence);
      }
    }

    // Decay confidence in silence
    if (!hasSpectrum && this.raw.level < 0.001) {
      this._bpmConfidence = slew(this._bpmConfidence, 0, dt, 10);
    }

    const bpm = this._bpm;
    const confident = this._bpmConfidence >= TEMPO.confidenceFloor;

    // Advance the ONE shared clock
    if (bpm > 0) {
      const prevBeats = this._beatsFloat;
      this._beatsFloat += (dt * bpm) / 60;
      if (Math.floor(this._beatsFloat) > Math.floor(prevBeats)) {
        // Whole-beat crossing
        this._onBeatT = 0;
        this._toggle = this._toggle > 0.5 ? 0 : 1;
      }
    }

    // Bass onsets: fire the beat event + phase-lock the clock
    this._sinceBeatEvent += dt;
    this._beatPulse *= Math.exp(-dt * 4.0);
    let beatGate = 0;
    const onsetFired = this._onset.bass.strength > 0 || this._levelOnset.strength > 0;
    if (onsetFired && this._sinceBeatEvent >= 0.2) {
      this._sinceBeatEvent = 0;
      this._beatPulse = 1;
      beatGate = 1;
      if (bpm > 0) {
        // Nudge phase toward the nearest whole beat (phase-lock, not replace)
        const frac = this._beatsFloat % 1;
        const err = frac < 0.5 ? -frac : 1 - frac;
        this._beatsFloat += err * (confident ? 0.15 : 0.5);
      }
    }

    this._onBeatT += dt;
    const onBeatClock = this._onBeatT < 0.12 ? 1 - this._onBeatT / 0.12 : 0;
    // Logistic-ish easing on the one-shot
    const eased = onBeatClock > 0 ? onBeatClock * onBeatClock * (3 - 2 * onBeatClock) : 0;
    this._toggleOut = slew(this._toggleOut, this._toggle, dt, 0.03);

    const beats = this._beatsFloat;
    bus.audioBPM = bpm;
    bus.audioBPMConfidence = this._bpmConfidence;
    bus.audioBeat = beatGate;
    bus.audioBeatPulse = this._beatPulse;
    bus.audioBeatPhase = bpm > 0 ? beats % 1 : 0;
    bus.audioBarPhase = bpm > 0 ? (beats / 4) % 1 : 0;
    bus.audioPhase2 = bpm > 0 ? (beats / 2) % 1 : 0;
    bus.audioPhase4 = bus.audioBarPhase;
    bus.audioPhase8 = bpm > 0 ? (beats / 8) % 1 : 0;
    bus.audioPhase16 = bpm > 0 ? (beats / 16) % 1 : 0;
    // Confidence rule: below the floor, beat-locked one-shots fall back to hits
    bus.audioOnBeat = confident ? eased : bus.audioBassHit;
    bus.audioToggleOnBeat = confident ? this._toggleOut : Math.round(this._toggleOut);
  };

  EaselAudioEngine.prototype._autocorrelateTempo = function () {
    const n = this._onsetRingCount;
    const buf = this._onsetRing;
    const start = (this._onsetRingPos - n + TEMPO.bufferLen) % TEMPO.bufferLen;
    if (!this._acScratch || this._acScratch.length < n) this._acScratch = new Float32Array(n);
    const x = this._acScratch;
    let mean = 0;
    for (let i = 0; i < n; i++) {
      x[i] = buf[(start + i) % TEMPO.bufferLen];
      mean += x[i];
    }
    mean /= n;
    let energy = 0;
    for (let i = 0; i < n; i++) {
      x[i] -= mean;
      energy += x[i] * x[i];
    }
    if (energy < 1e-6) return null;

    const fps = TEMPO.sampleHz;
    const minLag = Math.max(2, Math.floor((fps * 60) / TEMPO.maxBPM));
    const maxLag = Math.min(n - 8, Math.ceil((fps * 60) / TEMPO.minBPM));
    if (maxLag <= minLag) return null;

    const corrAt = (lag) => {
      let c = 0;
      for (let i = 0; i < n - lag; i++) c += x[i] * x[i + lag];
      return c / (n - lag);
    };

    let bestLag = 0;
    let bestCorr = -Infinity;
    for (let lag = minLag; lag <= maxLag; lag++) {
      const c = corrAt(lag);
      if (c > bestCorr) {
        bestCorr = c;
        bestLag = lag;
      }
    }
    if (bestLag === 0 || bestCorr <= 0) return null;

    // Octave correction: prefer the faster lag when nearly as strong
    const halfLag = Math.round(bestLag / 2);
    if (halfLag >= minLag) {
      const cHalf = corrAt(halfLag);
      if (cHalf > bestCorr * 0.85) {
        bestLag = halfLag;
        bestCorr = cHalf;
      }
    }

    const zeroLag = energy / n;
    const confidence = clamp01(bestCorr / (zeroLag + 1e-12));
    const bpm = (fps * 60) / bestLag;
    if (bpm < TEMPO.minBPM || bpm > TEMPO.maxBPM) return null;
    return { bpm, confidence };
  };

  EaselAudioEngine.prototype._zeroBus = function () {
    const names = [
      'audioLevel', 'audioBass', 'audioMid', 'audioHigh', 'audioSub', 'audioTreble',
      'audioLowMid', 'audioHighMid',
      'audioEnergy', 'audioBrightness', 'audioPunch', 'audioBeatPulse', 'audioOnset',
      'audioBeat',
      'audioBassHit', 'audioMidHit', 'audioHighHit',
      'audioBassPresence', 'audioMidPresence', 'audioHighPresence', 'audioPresence',
      'audioBassTime', 'audioMidTime', 'audioHighTime', 'audioTime',
      'audioBPM', 'audioBPMConfidence', 'audioBeatPhase', 'audioBarPhase',
      'audioPhase2', 'audioPhase4', 'audioPhase8', 'audioPhase16',
      'audioOnBeat', 'audioToggleOnBeat',
      'stemBass', 'stemDrums', 'stemMelody', 'stemAir', 'stemVocal',
      'stemBassHit', 'stemDrumsHit', 'stemMelodyHit', 'stemAirHit', 'stemVocalHit',
      'stemBassPresence', 'stemDrumsPresence', 'stemMelodyPresence', 'stemAirPresence',
      'stemVocalPresence',
    ];
    for (const nm of names) this.bus[nm] = 0;
  };

  // ---------------------------------------------------------------------------
  // Export: one shared engine instance (the single bus) + the class + helpers
  // ---------------------------------------------------------------------------

  const engine = new EaselAudioEngine();
  engine.Engine = EaselAudioEngine;
  engine.createEngine = function () {
    return new EaselAudioEngine();
  };
  // dt-derived slew helpers for consumers (media.js binding-source envelopes)
  engine.dsp = { clamp01, clamp, slew, slewAR };

  if (globalScope) globalScope.EaselAudio = engine;
  if (typeof module !== 'undefined' && module.exports) {
    module.exports = engine;
  }
})(typeof window !== 'undefined' ? window : typeof globalThis !== 'undefined' ? globalThis : null);
