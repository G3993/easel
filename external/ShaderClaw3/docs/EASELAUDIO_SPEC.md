# EaselAudio — Unified Audio-Reactive Engine Spec v1.0 (2026-07-08)

One analyzer core, one bus contract, five products: Easel desktop (C++), ShaderClaw3 (JS/WebGL),
etherea-ai (JS), easel-agent-sdk (Python relay), easel-mobile (Swift).
Research basis: docs-adjacent synthesis of Resolume, MadMapper, Ableton/M4L, disguise/Notch,
Synesthesia, TouchDesigner, Unreal (Synesthesia/Quartz), and the DJ-app stem landscape
(scratchpad/audio_engine_research.md). Nothing existing is renamed; all additions are additive.

## 0. Non-negotiables
- **Smooth by default** (standing user mandate): felt behavior of smoothing 0.85 / preset
  intensity 0.22 is preserved as the default preset. Everything ships pre-damped; "chopped"
  is an opt-in character, never the default.
- All smoothing times in **seconds**, coefficients derived from dt every frame
  (`k = 1 - exp(-dt/tau)`). No per-frame alphas, ever (fixes ShaderClaw3 ease=0.25@60fps
  and mobile's per-callback alphas).
- Legacy quartet `audioLevel/audioBass/audioMid/audioHigh` keeps its names, 0-1 ranges, and
  default feel (now AGC'd under the hood). Existing shaders must not change behavior in silence.
- Analysis rate decoupled from render rate where the platform allows (UE pattern).

## 1. Bus contract (uniform names, all float 0-1 unless noted)
### 1.1 Existing (keep verbatim — already auto-injected in ShaderClaw3/Easel)
audioLevel, audioBass, audioMid, audioHigh, audioSub, audioTreble, audioEnergy,
audioPunch, audioBeatPulse, audioOnset, audioBeat, audioBrightness, audioFFT (texture)
— plus every other uniform currently in js/audio.js `_bus` / Easel AudioFeatures.h.
Implementations enumerate the existing set from those files; none are removed.

### 1.2 New — temperament matrix (Synesthesia taxonomy)
For band in {Bass, Mid, High} plus the mix (no suffix = level-band already exists):
- `audio<Band>Hit`   — dual-follower onset per band, AD-enveloped (attack 10ms pow 1.0,
                        release 350ms pow 1.6), self-normalizing (no absolute threshold).
- `audio<Band>Presence` — slow macro envelope (attack 1.5s / release 4s) of the AGC'd band:
                        "is this band present in the mix right now".
- `audio<Band>Time`  — monotonic integrated clock: `clock += dt * band` (UNCLAMPED float,
                        wraps nothing). THE smooth-but-reactive drive: use as a time source
                        (`sin(audioBassTime*2.0)`) — can never jitter, pauses in silence.
Plus mix-level: `audioPresence`, `audioTime`.

### 1.3 New — rhythm bus (separate channel from FFT)
- `audioBPM` (absolute, ~50-220), `audioBPMConfidence` (0-1),
- `audioBeatPhase` (0-1 ramp per beat), `audioBarPhase` (0-1 per 4 beats),
- `audioPhase2, audioPhase4, audioPhase8, audioPhase16` (0-1 ramps over 2/4/8/16 beats),
- `audioOnBeat` (logistic-eased one-shot, ~120ms), `audioToggleOnBeat` (0/1 flip, eased).
Tempo: autocorrelation/comb filter over the onset-strength envelope (mel/band flux),
octave-corrected, with confidence. Detected tempo PHASE-LOCKS the product's existing BPM
clock (Easel BPMSync; tap/OSC stay as overrides). When confidence < 0.4, beat-locked
signals fall back to level-driven equivalents (Synesthesia rule).

### 1.4 New — pseudo-stems (tier 1, zero-ML, ships now)
4-band Linkwitz-Riley split + causal median HPSS (past-only kernel 31, power 2.0, margin 2.5;
one STFT hop latency):
- `stemBass`   — sub+low band level (<120 Hz)
- `stemDrums`  — HPSS percussive level (all-band transients)
- `stemMelody` — harmonic minus sub (tonal body 120Hz-2k)
- `stemAir`    — >2k harmonic (pads/cymbals wash)
- `stemVocal`  — 2-6 kHz harmonic x spectral-flux gate (vocal presence approximation)
Each also gets `<stem>Hit` and `<stem>Presence` (same temperaments). Per-stem conditioning:
stems get +50% release vs mix bands (mask flutter guard); stemDrumsHit threshold runs lower.
Tier 2 (background demucs-mlx per file, cached by hash, crossfaded in) and tier 3 (live
HS-TasNet/CoreML) are Phase-2+ — the uniform names above are already stem-splitter-agnostic.

## 2. Conditioning chain (one shared block — analyzer bands AND per-binding)
```
gate(thresholdDb=-60, hysteresis 3dB)
→ attack(attackSec, attackPow)        // time-to-90%, curve exponent per edge
→ hold(holdSec)                       // peak-hold (Resolume Fall / MadMapper decay)
→ release(releaseSec, releasePow)
→ hardChangeThreshold(delta=0.35)     // Notch: deltas > delta bypass smoothing entirely
→ character(-1..+1)                   // -1 = extra One-Euro smooth … +1 = spiky/derivative-boosted
→ remap(inLo,inHi,outLo,outHi,gamma)
→ micro-slew(3ms, mandatory)          // zipper guard, non-removable
```
Role presets: general 10/100ms · visual-binding 10/500ms (the 0.85-feel default) ·
meters 300/300ms · stems +50% release. UI simple path = 2 knobs:
**Intensity** (0-1, default 0.22) + **Character** (smooth↔chopped, default -0.5),
macro-mapped onto the chain. Full chain behind an advanced disclosure.
"Chopped" variants for free: gate + beat-quantized sample-and-hold (1/4, 1/8, 1/16) as
binding options — chopped is ALSO conditioned (eased edges ~30ms), never raw steps.

## 3. AGC (default ON)
Per band: `norm = clamp((dB - noiseFloor) / range, 0, 1)`, noiseFloor -60dB (doubles as the
silence gate); `range` auto-tracked fast-grow/slow-shrink (attack 0.1s, release 60s — no
pumping). Kills all magic gains (Easel 60/100/200/400x, mobile x8/x12/x10, ShaderClaw
_AUDIO_GAIN). One defeatable global manual gain for pros; 3-state indicator (ok/clip/silent).

## 4. Mapping modes (per binding, where the product has bindings)
Modulate (default — around the authored base value) · Own (min/max remap) ·
Speed (audio as d/dt — Resolume drive) · Accumulate (integrator / Time-clock).
Algebra: Add/Mul/Replace + Blend amount. TimeOffsetSec on every binding.

## 5. Universal shader params (task-12 pass, ShaderClaw3 INPUTS additions)
Standard block appended to every shader's INPUTS (defaults = zero visual change):
- `hueShift`  float 0-1 default 0 — final-output hue rotation.
- `colorBoost` float 0-2 default 1 — final-output saturation multiplier.
- `bgColor`   color default (0,0,0,0) — alpha 0 = native background; alpha>0 blends the
  shader's background region toward rgb (background = shader-specific: cleared/void/far
  region; for text overlays this stays transparent-aware).
- `audioReactivity` float 0-2 default 1 — scales the shader's own audio response depth
  (multiplies the DEVIATION of audio terms from 1.0, not the bands themselves).
Engine-side (no shader edits): master Intensity + Character knobs scale/shape the whole bus
before upload, so every shader — old or new — obeys the global feel controls.

## 6. Per-product implementation notes
### Easel desktop (/Users/lu/easel) — Phase 1 core
- New analyzer core behind the existing `AudioFeatures` struct; legacy quartet = AGC'd aliases.
- Replace the 4 divergent smoothing stages with the shared conditioning block; add per-binding
  attack/release/character behind the existing smoothing slider (slider maps onto the block).
- Layer-transform bindings + per-effect mods get smoothing (CompositeEngine.cpp:503-521 gap).
- Beat: dual-follower onsets + autocorr tempo phase-locking BPMSync (AudioAnalyzer.cpp:708 naive
  trigger replaced). kiss_fft cfg cached (alloc'd per call today, AudioAnalyzer.cpp:287).
- Extend ShaderSource.cpp GLSL preamble with §1.2-1.4 uniforms; wire chromaTex (declared, never fed).
- PropertyPanel preset row gains the 2-knob simple path (Intensity default 0.22 + Character).
### ShaderClaw3 (/Users/lu/ShaderClaw3)
- **Wiring fix first**: renderer path must feed a REAL bus — unify media.js/js/audio.js
  (classic-script core `js/easel-audio.js` owning analysis; media.js updateAudioUniforms
  delegates to it; window._audioBus written every frame from the LIVE analyser).
- isf.js preamble += §1.2-1.4 uniforms. eval_page.html StyleGen extended to drive the new
  uniforms (styles already model kick/swing/energy — derive Hit/Presence/Time/stems from
  the same envelopes). fftSize 256→2048 (bins 1024) for real sub/air resolution.
### etherea-ai (/Users/lu/etherea-ai)
- Fix bass=mid=high=rms*4 (audio-source-manager.js:456) — port the same easel-audio core;
  wire or replace the orphaned p5/p95 normalizer with §3 AGC; single beat clock; defaults to
  0.85-feel; vendor Meyda/worklets locally (no unpkg at runtime).
### easel-agent-sdk (/Users/lu/easel-agent-sdk)
- EaselAudio IS the "audio.features producer" (issue 12): relay `/easel/bpm`, `/easel/tap`,
  and an `audio.features` telemetry snapshot (bus floats at 10-30Hz, delivery decoupled);
  OSC map auto-generated from the bus schema (`/easel/audio/<feature>`).
### easel-mobile (/Users/lu/easel-mobile, branch scratch/james-merge ONLY)
- Swift port of the core (EaselAudio.swift): real FFT (Accelerate vDSP), dt-based conditioning,
  AGC replacing x8/x12/x10 gains, feed the full uniform set (audioMid is declared-but-never-fed
  today); keep delivery mechanism as-is this phase (evaluateJavaScript), decoupled 30Hz push.
  Do NOT touch local main (diverged + dirty).

## 7. Verification
- ShaderClaw3: tools/eval_harness.cjs multi-style run stays the gate (145+ shaders, 5 styles,
  audioMin >= 1.0, zero CHOPPY). New-uniform smoke test: calibration shaders driving
  Hit/Presence/Time/stem uniforms.
- Easel: cmake build + existing test_shaders; NDI/manual smoke optional.
- etherea/SDK/mobile: build/lint + a bus-snapshot unit test (silence → all zeros except Time
  clocks frozen; sine sweep → correct band ordering; kick pattern → Hit fires, Presence rises).
- One bus schema JSON (spec/easel_audio_bus.json) enumerates every uniform: name, type, range,
  temperament, description — codegen target for GLSL preamble/JS uploader/Swift struct/OSC map,
  and the source of the auto-generated "personality" doc.
