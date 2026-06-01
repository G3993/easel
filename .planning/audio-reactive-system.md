# Easel Next-Generation Audio-Reactive Shader System

**Status:** Design spec, ready for implementation
**Audience:** The engineer who will build this end-to-end (C++ analyzer + GLSL bus + rubric harness)
**Scope:** Replace Easel's lossy 5-value audio boundary with a shared, pre-conditioned *Audio Feature Bus*, define the full synesthesia mapping law, ship a shared GLSL include, and gate the library with an automated reactivity rubric.

---

## 1. Executive Summary + Design Principles

Easel today pushes exactly five numbers into every ISF layer shader — `setAudioState(rms, bass, mid, high, fftTex)`. That is the entire vocabulary ~117 reactive shaders have to express *synesthesia, movement, flow, noise, grain, texture, energy, melancholy, charm, softness, mood, vibe, palette, layering, arrangement, and the build-up*. They can't, so they fake it: hardcoded BPM, per-pixel rainbow hue, twitchy meters that gate to black.

This document specifies the fix as one architectural move and one authoring law:

- **The Audio Feature Bus** — a single `AudioFeatures` struct, computed once per frame in `AudioAnalyzer`, carrying ~40 named, pre-smoothed, normalized perceptual features into every shader through one setter and one uniform-declaration block. Adding a feature touches exactly four sites and never changes a function signature again.
- **The Master Synesthesia Mapping** — a per-attribute correspondence law (audio feature → shader parameter + example GLSL) so every shader expresses the user's vocabulary consistently and tastefully.

Backed by a shared GLSL include (`audio_react.glsl`), an automated conformance rubric (`test_audio_reactivity`, the analog of `test_bridge.sh`), and a phased rollout.

### Design principles

1. **Natural.** Map the perceptual dimension that actually matches the visual dimension (pitch→elevation/brightness, loudness→magnitude, noisiness→grain, tempo→motion-speed). Match the *rate* too: slow musical qualities drive slow visual properties on slow envelopes; transients drive fast properties on peak-hold envelopes. Crossing rates is the #1 cause of twitchy-or-dead reactivity.
2. **Clean.** Every uniform arrives pre-smoothed and normalized with a documented time constant; shaders *read*, never re-filter. Modulate around a living baseline — never gate to black; the shader looks good at silence. One snappy 1:1 channel max per shader; everything else is eased. Color is chosen from a curated, capped-chroma ramp — never per-pixel `hue = f(pitch)`.
3. **Synesthetic.** The whole frame should agree with the whole sound: color keyed to the song's harmony, brightness to its timbre, motion to its pulse, surface to its noisiness, and macro-gesture (build-up, drop, section change) to its arrangement. The image is one coherent instrument, not twelve independent meters.

---

## 2. Current State (Baseline) and the Gap

### What exists (load-bearing, confirmed in code)

- `src/app/AudioAnalyzer.{h,cpp}`: `kFFTSize = 512`, `kBins = 256`; `m_spectrum[256]` (linear magnitude); `expSmooth(current, target, rate, dt)` (dt-correct EMA primitive) at ~line 256; band accessors including `lowMid()` and `highMid()` already computed but merged before they reach shaders; `m_energyHistory`; a bass-energy beat detector; `AudioCurve` UI preset shaping.
- `src/app/BPMSync.h`: `bpm()`, `beatPhase()`, `beatPulse()`, `barPhase()` — **already computed**, but not plumbed to the layer-shader path.
- `src/sources/ShaderSource.{h,cpp}`: 5-arg `setAudioState(rms,bass,mid,high,fftTex)` (~line 907); always-injected uniform decl block (~lines 300–311) declaring `audioLevel/Bass/Mid/High` + `audioFFT` (unit 2); `uploadUniforms` (~lines 706–714) uploads each band as its own smoothed value; **`audioMid` is uploaded directly as its own smoothed band** (this matters for migration, see §8).
- FFT texture: `128 x 1`, `GL_R8` — visible stair-stepping that shaders patch around with `clamp(…,0,.85)` + wobble.
- `src/compositing/CompositeEngine.{h,cpp}` + `shaders/passthrough.frag`: a richer compositor-path `AudioState` (bands + beat + BPM) that the layer path never sees ("Path B").

### The gap

| Need | Today | Consequence |
|---|---|---|
| Beat / BPM in layer shaders | absent on Path A (exists on Path B + BPMSync) | shaders fake BPM (`data_sculpture`, `shatter_grid`) |
| Band resolution | 3 bands (mid = lowMid+highMid merged) | kick/hat/vocal indistinguishable → "layering" impossible |
| Timbre (bright/dark, noisy/tonal) | none | treble behaves identically everywhere |
| Affect (mood, valence, arousal) | none | no melancholy / charm / vibe |
| Structure (build-up, drop, sections) | none | the most expressive macro-gesture is unreachable |
| Harmony / color anchor | none | shaders resort to garish per-pixel hue |
| FFT fidelity | 128×R8 | stair-stepping, sub-bass invisible |
| Extensibility | 5-arg signature | adding a feature is a breaking API change |

The bus closes all of these without per-shader boilerplate.

---

## 3. The Audio Feature Bus — Full Uniform Spec

### 3.1 Conventions

- **τ** = EMA time constant in seconds; use the existing `expSmooth(cur, target, rate, dt)` with `rate = 1/τ`.
- **A/R** = asymmetric attack/release as two τ (e.g. `A 50ms / R 300ms`); falling uses the release rate.
- **Normalization policy (resolved — see critique fix P0-3):** two distinct strategies, never confused:
  - **AGC** (running-min/max normalizer) is applied **only** to features where *relative shape* is the point and absolute level is not: `audioBrightness`, `audioFlatness`, `audioFlux`, `audioTilt`. AGC params: rise τ≈0.25s, fall τ≈20s, `hi` floored at 0.05.
  - **Stable-normalize** (slow track-stable gain, or fixed headroom gain — NOT fast per-track AGC) is used for everything that carries *absolute dynamics*: `audioLevel`, all bands, `audioEnergy`, `audioBuildup`, anything feeding mood/structure. This preserves the quiet-intro-vs-full-drop contrast the whole design depends on.
  - Each Tier 1/2/4 table below carries a **Norm** column making this explicit.
- **AGC ordering (resolved — critique fix P0-2):** the order is **smooth → AGC → final A/R envelope**, never "AGC before smoothing." Feeding raw per-frame values into the min/max tracker makes `hi` chase single-frame peaks and pumps the whole bus. For transient features the pre-AGC stage is a peak-hold.
- **Naming convention:** `audio` prefix + camelCase noun (`audioBrightness`); vectors get the bare noun (`audioFlow`, `audioMood`); textures get `Tex` suffix (`audioFFTTex`, `audioChromaTex`). Centered signals are `[-1,1]`; everything else `[0,1]`. **This spec's names are authoritative — see the glossary in §3.8; all GLSL in §5 uses these names verbatim.**
- **Working color space (resolved — critique fix P2-13):** the ISF layer pipeline works in **linear** light. Palette uniforms are linear-light RGB; shaders blend in linear and the final present stage applies the existing sRGB encode. Any shader still using sRGB color literals must be converted when it adopts the palette (it's part of the retrofit checklist).
- All decays are continuous-time, expressed as τ and routed through the dt-aware helper — **no per-frame multipliers** (critique fix P1-9).

### 3.2 Compute DAG (resolved — critique fix P0-5)

To eliminate the Tier 3↔4 cycle and init-order NaNs, `update(dt)` computes in this **strict single-direction order** each frame:

```
1. computeBands()                      // existing
2. computeSpectralFeatures(dt)         // centroid, spread, rolloff, flatness, flux, tilt, zcr, punch
3. computeChroma()                     // chroma[12], modeMajorness, modeConf, hcdf, dominantPitch
4. computeRoughness(dt)                // smoothed peak set → roughness  (feeds affect)
5. computeAffect(dt)                   // valence/arousal/tension/warmth/softness/charm
                                       //   — reads PREVIOUS-frame structure (buildup/energy)
6. computeStructure(dt)               // energy/vel/acc, buildup, drop, novelty, sections, layers
                                       //   — reads CURRENT-frame affect
7. computeFlow(dt)                    // audioFlow vec2
8. computePalette(dt)                 // Oklch ramp + accent
9. buildChromaTexture(); buildOccupancyTexture();
```

Rule: **`audioTension` reads previous-frame `audioBuildup`; `audioBuildup` reads current-frame `audioTension`.** All `m_*` initialized to neutral (centered→0.5, energy→0); every divide by `ΣS` guarded with `+ε`.

---

### TIER 1 — Core energy & bands

Replaces the lossy 3-band collapse. Legacy `audioLevel/Bass/Mid/High` are kept but **frozen on the existing pre-AGC pipeline** (critique fix P0-1) — see §8 migration rule.

| Uniform | Type | Range | Norm | Represents | DSP recipe | Smoothing |
|---|---|---|---|---|---|---|
| `audioLevel` | float | 0–1 | stable | overall loudness — "the breath" | windowed RMS, perceptual curve | A 60ms / R 300ms |
| `audioSub` | float | 0–1 | stable | sub-bass (kick/808 fundamentals) | see §3.7 FFT note — coarse 1–2 bin proxy at 30–80Hz | A 30ms / R 200ms |
| `audioBass` | float | 0–1 | stable | low-end body | bass band (94–188 Hz) | A 40ms / R 250ms |
| `audioLowMid` | float | 0–1 | stable | warmth, pad/vox body | existing `lowMid` (282–940 Hz), **stop merging** | A 50ms / R 300ms |
| `audioHighMid` | float | 0–1 | stable | vocal presence, leads | existing `highMid` (1–4 kHz), **stop merging** | A 50ms / R 300ms |
| `audioTreble` | float | 0–1 | stable | air, hats, sparkle | treble (4–12 kHz) | A 20ms / R 150ms |
| `audioPunch` | float | 0–1 | stable | transient snap | crest `peak/(rms+ε)` mapped 1→6 | **peak-hold: instant attack, hold 50ms, decay τ120ms** |
| `audioBeat` | float | 0–1 | — | onset envelope (decaying flash) | `beatDecay()` plumbed to Path A | decay τ≈120ms (`exp(-dt/τ)`) |
| `audioBeatPhase` | float | 0–1 | — | sawtooth per beat (anticipation) | `BPMSync::beatPhase()` | none (phase) |
| `audioBeatPulse` | float | 0–1 | — | decaying pulse per beat boundary | `BPMSync::beatPulse()` | decay τ≈80ms |
| `audioBarPhase` | float | 0–1 | — | sawtooth over 4 beats | `BPMSync::barPhase()` | none (phase) |
| `audioBPM` | float | raw | — | tempo | `BPMSync::bpm()` | none |
| `audioTempo01` | float | 0–1 | — | normalized tempo (map 60–180→0–1) | `clamp((bpm-60)/120,0,1)` | none |

Legacy aliases (frozen pipeline): `audioLevel` (= existing rms), `audioBass`, `audioMid` (= existing single mid band, **unchanged**), `audioHigh` (= existing treble band, **unchanged**). The new split bands ship under the new names; **no legacy name is retargeted through AGC.**

`audioPunch` is a transient channel: drive at most one fast param with it; **do not gate global exposure with it** (critique fix P0-4).

---

### TIER 2 — Spectral character

One or two linear passes over `m_spectrum[256]` + a one-frame `prevSpec[256]`.

| Uniform | Type | Range | Norm | Represents | DSP recipe | Smoothing |
|---|---|---|---|---|---|---|
| `audioBrightness` | float | 0–1 | AGC | spectral centroid → bright/dark | `Σ(i·S[i])/ΣS[i]`, log2-map 50Hz→8kHz | double-EMA, τ≈0.7s |
| `audioSpread` | float | 0–1 | — | focused vs washy | spectral std-dev / (kBins/3) | one-pole τ≈0.4s |
| `audioRolloff` | float | 0–1 | — | body vs air (robust brightness) | bin of 0.85·cumulative-energy, log-mapped | one-pole τ≈0.4s |
| `audioFlatness` | float | 0–1 | AGC | **noisiness** tonal(0)↔noise(1) | `exp(mean(log(S+ε)))/mean(S)`, bins 4–200 | one-pole τ≈0.25s |
| `audioFlux` | float | 0–1 | AGC | **movement/change** | `Σmax(0,S−prevS)` / **slow running energy floor** (critique fix P1-8) | one-pole τ≈0.15s |
| `audioOnset` | float | 0–1 | — | discrete attack pulse | flux peak-picked vs 1s median×1.5, 60ms refractory, **gated below absolute energy floor** | peak-hold, decay τ≈80ms |
| `audioOnsetRate` | float | 0–1 | — | rhythmic busyness | onset counter `*=exp(-dt/1.5)`, /8 | inherent decay |
| `audioTilt` | float | -1–1 | AGC | warm(−)/harsh(+) balance | `(highMid+treble − bass−lowMid)/sum` | one-pole τ≈0.5s |
| `audioZCR` | float | 0–1 | — | sibilance / cheap noisiness | sign-changes/N on time buffer | one-pole τ≈0.2s |
| `audioTexture` | float | 0–1 | — | sustained surface (crispy↔smooth) | smoothed `0.5+0.5*audioTilt` blended w/ flatness | one-pole τ≈0.25s |

`audioTexture` is the dedicated **texture** home distinct from grain (transient) and noise (flatness) — see critique fix P3-15 and §5.4–5.6.

---

### TIER 3 — Affect / mood scalars

Derived from Tier 1–2, slow-smoothed, session-stable. Computed in `computeAffect`, reading **previous-frame** structure.

| Uniform | Type | Range | Represents | DSP recipe | Smoothing |
|---|---|---|---|---|---|
| `audioValence` | float | 0–1 | bright/pleasant ↔ sad/dark | `0.45·lerp(.5,modeMajorness,modeConf) + 0.20·audioBrightness + 0.20·(1−audioRoughness) + 0.15·majorChordFit` | A 300ms / R 1.5s |
| `audioArousal` | float | 0–1 | calm ↔ energetic | `0.30·audioLevel + 0.25·audioFlux + 0.20·audioTempo01 + 0.15·audioTreble + 0.10·audioPunch` | A 120ms / R 600ms |
| `audioTension` | float | 0–1 | resolved ↔ unstable | `0.35·audioRoughness + 0.25·prevBuildup + 0.20·(1−modeMajorness)·modeConf + 0.20·fluxVariance` | A 200ms / R 800ms; ×0.3 on `audioDrop` |
| `audioWarmth` | float | 0–1 | warm/intimate ↔ cold/airy | `0.50·audioBass + 0.25·(1−audioTreble) + 0.15·(1−audioFlatness) + 0.10·lerp(.5,modeMajorness,modeConf)` | A 500ms / R 2s |
| `audioSoftness` | float | 0–1 | gentle/blurred ↔ sharp | `0.40·(1−audioPunch) + 0.30·(1−audioFlux) + 0.30·(1−transientDensity)` — **roughness dropped to decorrelate from tension** (critique fix P3-16) | A 400ms / R 1.5s |
| `audioRoughness` | float | 0–1 | sensory dissonance | Plomp–Levelt beating over top ~12 spectral peaks, **parabolic-interpolated, magnitude-floored, peak-set smoothed** (critique fix P1-7) | one-pole τ≈0.5s |
| `audioMood` | vec2 | each 0–1 | valence×arousal point (Russell) | `vec2(audioValence, audioArousal)` | inherits |
| `audioCharm` | float | 0–1 | "lovely groove" sweet spot | `audioValence·(1−audioTension)·gauss(audioArousal,μ=0.5,**σ=0.35**) + floor 0.05` — **widened + floored** (critique fix P3-18) | inherits |

`modeMajorness`/`modeConf`: correlate 12-bin chroma vs 24 Krumhansl–Kessler templates; `modeMajorness = sigmoid(bestMajorCorr − bestMinorCorr)`, `modeConf = max(bestMajor,bestMinor)`.

---

### TIER 4 — Structure / build-up

Dual-timescale EMAs: `slow` = where the song's been, `fast` = where it is, `fast − slow` = where it's going. Computed in `computeStructure`, reading current-frame affect. **Every derivative carries an explicit post-smoothing τ** (critique fix P4-21) — no "implicit."

| Uniform | Type | Range | Norm | Represents | DSP recipe | Smoothing |
|---|---|---|---|---|---|---|
| `audioEnergy` | float | 0–1 | stable | arrangement altitude | slow-EMA of `Σlog(1+S)`, 30s running max | double-EMA τ≈4s |
| `audioEnergyVel` | float | -1–1 | — | rising/falling | d(energy)/dt, clamped at ±(1/0.5s) | post-EMA τ≈0.5s |
| `audioEnergyAcc` | float | -1–1 | — | lift/anticipation | d²(energy)/dt², clamped | post-EMA τ≈0.5s |
| `audioBuildup` | float | 0–1 | stable | riser progress (integrating) | `Σwₖ·clamp((fastₖ−slowₖ)/slowₖ,0,1)` over RMS/treble/onsetRate/brightness | A 250ms / R τ≈1.6s, integrates & holds |
| `audioBuildupRate` | float | -1–1 | — | signed build slope | d(buildup)/dt | post-EMA τ≈0.4s |
| `audioDrop` | float | 0–1 | — | structural impact impulse | `(buildup>0.5 OR gapBefore) AND broadband slam` → 1.0, **1s refractory, clamped, fed to bloom-pre buffer** (critique fix P4-22) | peak-hold, decay τ≈0.8s |
| `audioNovelty` | float | 0–1 | — | section change | cosine distance of 12-band shape vs slow-EMA | A fast / R 2s |
| `audioSectionPhase` | float | 0–1 | — | section index `mod(count,N)/N` | increments on novelty cross, 4s refractory | step |
| `audioSectionAge` | float | 0–1 | — | seconds-into-section (~/60s) | reset on change, ramps | linear |
| `audioLayers` | float | 0–1 | stable | active element count (assembly) | count of 12 log-bands above 1.4×slow-EMA / 12 | **A 400ms / R 2s** (snap in, fade out — critique fix P1-11) |
| `audioDensity` | float | 0–1 | — | mix fullness | `audioFlatness` reused, lightly smoothed | one-pole τ≈0.5s |
| `audioPresence` | vec4 | each 0–1 | stable | per-band presence (bass/lowMid/highMid/treble active) | per-band over-slow-EMA mask, smoothed | A 400ms / R 2s |

`audioPresence` (vec4) is the dedicated per-band layering driver the mapping needs (critique fix P3, P2-12). `audioOccupancyTex` (12×1) below carries the finer 12-band mask.

---

### TIER 5 — Palette anchors (synesthesia, harmonious by construction)

Never `hue = f(pitch)` per pixel. CPU computes a 3-stop Oklch ramp + accent; shaders sample it. Chroma capped `C ≤ 0.16` in Oklch.

| Uniform | Type | Space | Represents | DSP / harmony recipe | Smoothing |
|---|---|---|---|---|---|
| `audioPalShadow` | vec3 | linear RGB | ramp stop 0 (darks) | Oklch(L=mix(.18,.32,V), C=Cbase·.6, H−offset·.5) | slow drivers |
| `audioPalMid` | vec3 | linear RGB | ramp stop 1 | Oklch(mix(.45,.62,V), Cbase, H) | — |
| `audioPalHigh` | vec3 | linear RGB | ramp stop 2 (lights) | Oklch(mix(.72,.92,V), Cbase·.8, H+offset·.5) | — |
| `audioPalAccent` | vec3 | linear RGB | onset pop color | Oklch(mix(Lmid,Lhi,beat), Cbase·(1+.8·beat), **H + 60·spreadSmoothed + 20·warm**) — **coeff reduced + accent hue circularly smoothed** (critique fix P4-19/20) | fast on beat, hue circular τ≈0.6s |
| `audioPalTemp` | float | -1–1 | global warm/cool grade | = `audioWarmth` mapped to ±1 | A 500ms / R 2s |
| `audioPalSat` | float | 0–1 | global vividness | `audioArousal`-shaped Cbase | A 150ms / R 400ms |

**Color drivers** (`PaletteEngine`, once/frame): `H` = energy-weighted circular mean of 12-bin chroma; `offset = mix(18°,130°, chromaDispersion)` (narrow=analogous, wide=triad — never random); `V = audioBrightness`; `Cbase = mix(.02,.16, audioArousal)`. **Hue smoothed circularly** (slerp the sin/cos pair, τ≈1.5s) so it never flickers the 360→0 seam.

---

### 3.6 FFT + chroma textures

| Uniform | Type | Format | Represents | Source |
|---|---|---|---|---|
| `audioFFTTex` | sampler2D | **256×1 R16F** (from 128×R8) | spectrum for frequency-as-space | `m_spectrum`, log-spaced |
| `audioFFTSmoothTex` | sampler2D | 256×1 R16F | peak-hold spectrum for trails | per-bin peak-hold decay |
| `audioChromaTex` | sampler2D | 12×1 R8 | pitch-class energies (note→color) | bins folded by precomputed `binToPC` |
| `audioChroma[12]` | float[12] | 0–1 | chroma as array | L∞-normalized profile |
| `audioDominantPitch` | float | 0–1 | strongest pitch class /12 | `argmax(chroma)/12` — **STEP signal; use only for discrete selection, do not animate** (critique fix P4-20) |
| `audioMajorMinor` | float | 0–1 | minor(0)↔major(1) | `modeMajorness` | 
| `audioHCDF` | float | 0–1 | harmonic-change (chord shifts) | frame-distance of 6-D Tonnetz centroid |

### 3.7 FFT upgrade — honest resolution note (critique fix P1-10)

`audioSub` (30–80 Hz) needs more than the current 512-pt FFT. **At 1024-pt, bin width is 47 Hz, so 30–80 Hz spans only ~1–2 bins** — heavy window leakage, low confidence. Two honest options:

- **1024-pt:** `audioSub` is a *coarse 1–2 bin proxy*. Ship with lowered expectations; adequate for "kick presence," not pitched sub.
- **2048-pt:** real sub resolution (~23 Hz bins), at ~43ms added latency. Recommended if sub fidelity matters.

`GL_R16F`: confirm the desktop GL 3.3 target supports **linear filtering on R16F** (not guaranteed on all GL ES profiles; fine on desktop GL 3.3, but verify on the actual context before relying on it to kill stair-stepping). The analyzer must write float spectrum data into the texture.

### 3.8 Authoritative uniform glossary (critique fix P2-12)

These are the **only** names shaders use. Earlier drafts used `audioCentroid / audioBuild / audioSparkle / audioDynamics / uPal* / bare valence`; those are dead. Canonical → killed:

| Canonical | Killed aliases |
|---|---|
| `audioBrightness` | `audioCentroid` |
| `audioBuildup` | `audioBuild` |
| `audioOnset` + `audioTreble` | `audioSparkle` |
| `audioTexture` | (was undefined) |
| `audioArousal`/`audioLevel`/`audioEnergy` (see §5.7) | `audioDynamics` |
| `audioPresence` (vec4) | (was undefined) |
| `audioFlow` (vec2) | (was undefined — now added, §3 Tier-2 adjunct & §5.3) |
| `audioValence`/`audioArousal`/`audioWarmth`/`audioSoftness` | bare `valence`/`arousal`/… |
| `audioBeatPhase`/`audioBeatPulse`/`audioBarPhase` | bare `beatPhase`/… |
| `audioPalShadow`/`audioPalMid`/`audioPalHigh`/`audioPalAccent`/`audioPalSat`/`audioPalTemp` | `uPalShadow`/`uPalMid`/`uPalHighlight`/`uPalAccent`/`uPalSat`/`uPalBeat`/`uPalTemp` |

`audioFlow` (vec2): magnitude from one-poled low-band (τ≈350ms), direction from `audioBarPhase` (or slow random walk), **vector slew-limited** so direction turns only on phrase boundaries. In struct as `float flow[2]`, uploaded `vec2 audioFlow`.

---

## 4. C++ Implementation Plan — AudioAnalyzer + plumbing

### 4.1 New state (`AudioAnalyzer.h`)

```cpp
// substrate
float m_prevSpec[kBins] = {0};
float m_centroid=0,m_spread=0,m_rolloff=0,m_flatness=0,m_flux=0,m_tilt=0,m_zcr=0,m_texture=.5f;
float m_punch=0, m_roughness=0;
float m_energyFloor=1e-3f;                       // slow floor for flux/onset gating
// dual-timescale energy (structure)
float m_energyFast=0,m_energySlow=0,m_energyNorm=0,m_energyVel=0,m_energyAcc=0,m_prevEnergy=0;
float m_buildup=0,m_buildupRate=0,m_drop=0,m_novelty=0;
float m_sectionPhase=0,m_sectionAge=0; int m_sectionCount=0;
float m_layers=0,m_density=0; uint16_t m_occupancyMask=0;
float m_presence[4]={0,0,0,0};
float m_flow[2]={0,0}, m_flowMag=0;
// affect (neutral init — see DAG)
float m_valence=.5f,m_arousal=0,m_tension=0,m_warmth=.5f,m_softness=.5f,m_charm=.05f,m_roughnessSm=0;
float m_prevBuildup=0;                            // tension reads previous-frame buildup
// chroma / harmony
float m_chroma[12]={0}; float m_modeMajorness=.5f,m_modeConf=0,m_hcdf=0,m_dominantPitch=0;
int   m_binToPC[kBins];
// palette
float m_palShadow[3],m_palMid[3],m_palHigh[3],m_palAccent[3],m_palTemp=0,m_palSat=0;
float m_hueSin=0,m_hueCos=1, m_accentHueSin=0,m_accentHueCos=1;   // circular smoothing
// histories
RingBuffer<float,128> m_fluxHist;
float m_band12Slow[12]={0};
// AGC only on shape features
struct AGC { float lo=0,hi=0.05f; float norm(float x,float dt); };
AGC m_agcBright,m_agcFlat,m_agcFlux,m_agcTilt;
// peak-hold helper
struct PeakHold { float v=0,hold=0; float update(float x,float dt,float holdT,float decayTau); };
PeakHold m_punchPH,m_onsetPH,m_dropPH,m_beatPulsePH;
```

### 4.2 New compute methods (call from `update(dt)` in DAG order, §3.2)

```cpp
void computeSpectralFeatures(float dt);  // centroid,spread,rolloff,flatness,flux,tilt,zcr,punch,texture
void computeChroma();                     // chroma[12] via binToPC; KK correlate; Tonnetz HCDF
void computeRoughness(float dt);          // parabolic peaks, mag floor, smoothed peak set
void computeAffect(float dt);             // valence/arousal/tension(prevBuildup)/warmth/softness/charm
void computeStructure(float dt);          // energy/vel/acc, buildup, drop, novelty, section, layers, presence
void computeFlow(float dt);               // flow vec2 (slew-limited)
void computePalette(float dt);            // Oklch ramp + accent (circular hue smoothing)
void buildChromaTexture();                // 12x1 R8
void buildOccupancyTexture();             // 12x1 R8
void smoothFeature(float& cur,float target,float aTau,float rTau,float dt); // generalize expSmooth
```

End of `update`: `m_prevBuildup = m_buildup;` (closes the DAG cleanly).

Add accessors mirroring `bass()`: `brightness() spread() rolloff() flatness() flux() onset() onsetRate() punch() tilt() zcr() texture() roughness() valence() arousal() tension() warmth() softness() charm() energy() energyVel() energyAcc() buildup() buildupRate() drop() novelty() sectionPhase() sectionAge() layers() density() presence() flow() chroma() chromaTex() occupancyTex() dominantPitch() majorMinor() hcdf() palShadow() palMid() palHigh() palAccent() palTemp() palSat() tempo01()`. Keep `AudioCurve` UI shaping **downstream** so users still shape features.

**Cost:** one extra linear pass over 256 bins + a handful of EMAs + 24 dot-products + ~12-peak roughness ≈ a few µs/frame on M-series. Negligible vs the existing FFT.

### 4.3 End-to-end plumbing (the exact path)

**Step 1 — Bus struct (`src/sources/AudioFeatures.h`, new):**

```cpp
struct AudioFeatures {
  // Tier 1
  float level,sub,bass,lowMid,highMid,treble,punch;
  float beat,beatPhase,beatPulse,barPhase,bpm,tempo01;
  // Tier 2
  float brightness,spread,rolloff,flatness,flux,onset,onsetRate,tilt,zcr,texture;
  // Tier 3
  float valence,arousal,tension,warmth,softness,roughness,charm;
  // Tier 4
  float energy,energyVel,energyAcc,buildup,buildupRate,drop;
  float novelty,sectionPhase,sectionAge,layers,density;
  float presence[4]; float flow[2];
  // Tier 5
  float palShadow[3],palMid[3],palHigh[3],palAccent[3],palTemp,palSat;
  // textures + harmony
  GLuint fftTex=0,chromaTex=0,occupancyTex=0;
  float chroma[12]; float dominantPitch,majorMinor,hcdf;
};
```

**Step 2 — Assemble once per frame (`Application.cpp`, where `AudioState` is built ~1615):**

```cpp
AudioFeatures af;
af.level=a.rms(); af.sub=a.sub(); af.bass=a.bass(); af.lowMid=a.lowMid();
af.highMid=a.highMid(); af.treble=a.treble(); af.punch=a.punch();
af.beat=a.beatDecay(); af.beatPhase=bpm.beatPhase(); af.beatPulse=bpm.beatPulse();
af.barPhase=bpm.barPhase(); af.bpm=bpm.bpm(); af.tempo01=a.tempo01();
af.brightness=a.brightness(); /* …all features… */
af.flow[0]=a.flow()[0]; af.flow[1]=a.flow()[1];
for(int i=0;i<4;i++) af.presence[i]=a.presence()[i];
af.fftTex=a.fftTexture(); af.chromaTex=a.chromaTex(); af.occupancyTex=a.occupancyTex();
```

Pass `af` to every layer (replace the `setAudioState` call site ~1324) via `layer.shader->setAudioFeatures(af)`. Also store `af` on `AudioState` so the compositor shares one source of truth.

**Step 3 — New setter (`ShaderSource.h/.cpp`, replacing 5-arg at ~907):**

```cpp
void ShaderSource::setAudioFeatures(const AudioFeatures& f){ m_af = f; }   // store by value
// keep a thin legacy shim so nothing breaks mid-migration:
void ShaderSource::setAudioState(float rms,float bass,float mid,float high,GLuint fft){
  m_legacyLevel=rms; m_legacyBass=bass; m_legacyMid=mid; m_legacyHigh=high; m_legacyFFT=fft;
}
```

**Step 4 — Declare uniforms (`translateFragment`, always-injected block ~300–311):** append after the existing `audioLevel/Bass/Mid/High`:

```cpp
out << "uniform float audioSub, audioLowMid, audioHighMid, audioTreble, audioPunch;\n";
out << "uniform float audioBeat, audioBeatPhase, audioBeatPulse, audioBarPhase, audioBPM, audioTempo01;\n";
out << "uniform float audioBrightness, audioSpread, audioRolloff, audioFlatness, audioTexture;\n";
out << "uniform float audioFlux, audioOnset, audioOnsetRate, audioTilt, audioZCR;\n";
out << "uniform float audioValence, audioArousal, audioTension, audioWarmth, audioSoftness, audioRoughness, audioCharm;\n";
out << "uniform vec2  audioMood, audioFlow;\n";
out << "uniform float audioEnergy, audioEnergyVel, audioEnergyAcc, audioBuildup, audioBuildupRate, audioDrop;\n";
out << "uniform float audioNovelty, audioSectionPhase, audioSectionAge, audioLayers, audioDensity;\n";
out << "uniform vec4  audioPresence;\n";
out << "uniform vec3  audioPalShadow, audioPalMid, audioPalHigh, audioPalAccent;\n";
out << "uniform float audioPalTemp, audioPalSat, audioDominantPitch, audioMajorMinor, audioHCDF;\n";
out << "uniform float audioChroma[12];\n";
out << "uniform sampler2D audioChromaTex;\n";     // unit 5
out << "uniform sampler2D audioOccupancyTex;\n";  // unit 6
// audioFFTTex already declared (unit 2)
```

Texture units: **2=audioFFTTex, 3=fontAtlas, 4=pass buffers, 5=audioChromaTex, 6=audioOccupancyTex, 8+=image inputs.**

**Step 5 — Upload (`uploadUniforms` ~706–714):**

```cpp
auto& f = m_af;
// legacy (frozen pipeline — uploaded from legacy fields, NOT from new AGC values)
m_shader.setFloat("audioLevel", m_legacyLevel);
m_shader.setFloat("audioBass",  m_legacyBass);
m_shader.setFloat("audioMid",   m_legacyMid);     // unchanged semantics
m_shader.setFloat("audioHigh",  m_legacyHigh);
// new bus
m_shader.setFloat("audioSub", f.sub); m_shader.setFloat("audioLowMid", f.lowMid);
m_shader.setFloat("audioHighMid", f.highMid); m_shader.setFloat("audioTreble", f.treble);
m_shader.setFloat("audioPunch", f.punch);
m_shader.setFloat("audioBeat", f.beat); m_shader.setFloat("audioBeatPhase", f.beatPhase);
m_shader.setFloat("audioBeatPulse", f.beatPulse); m_shader.setFloat("audioBarPhase", f.barPhase);
m_shader.setFloat("audioBPM", f.bpm); m_shader.setFloat("audioTempo01", f.tempo01);
m_shader.setFloat("audioBrightness", f.brightness); /* …all scalars… */
m_shader.setVec2("audioMood", f.valence, f.arousal);
m_shader.setVec2("audioFlow", f.flow[0], f.flow[1]);
m_shader.setVec4("audioPresence", f.presence[0],f.presence[1],f.presence[2],f.presence[3]);
m_shader.setVec3("audioPalShadow", f.palShadow[0],f.palShadow[1],f.palShadow[2]); /* …ramp… */
m_shader.setFloatArray("audioChroma", f.chroma, 12);
m_shader.setInt("audioFFTTex",5? :2); glActiveTexture(GL_TEXTURE2); glBindTexture(GL_TEXTURE_2D,f.fftTex);
m_shader.setInt("audioChromaTex",5);   glActiveTexture(GL_TEXTURE5); glBindTexture(GL_TEXTURE_2D,f.chromaTex);
m_shader.setInt("audioOccupancyTex",6);glActiveTexture(GL_TEXTURE6); glBindTexture(GL_TEXTURE_2D,f.occupancyTex);
```

**Step 6 — Path B (compositor), optional:** fold `AudioFeatures` into `CompositeEngine`'s `AudioState` (one struct everywhere) or add matching `uXxx` uniforms in `passthrough.frag` + `CompositeEngine::setAudioUniforms()`. Unifying on one struct is the cleaner end state.

**The complete recipe for any future feature:** add field to `AudioFeatures` → compute in `AudioAnalyzer` (respecting the DAG) → assemble in `Application.cpp` → declare in `translateFragment` → upload in `uploadUniforms`. **The signature never changes again.**

---

## 5. The Master Synesthesia Mapping

Three authoring rules: **(1) match dimension then rate; (2) modulate around a living baseline, never gate to black; (3) one snappy channel max.** All GLSL uses canonical bus names and `audioPalette(t)` from §6.

### 5.1 Synesthesia — frequency content → color and elevation
- **Feature(s):** `audioChroma[]`/`audioChromaTex` (tonal hue, already baked into the palette H), `audioBrightness` (timbral bright/dark + elevation), `audioHCDF` (chord shifts).
- **Param(s):** position along the palette ramp + vertical elevation of elements; value/lightness.
- **Why clean:** pitch→elevation and pitch→brightness are the most robust cross-modal correspondences (Spence 2011). Hue from chroma (not per-pixel pitch) keeps it harmonious; the capped ramp is the anti-garish guarantee.
```glsl
float elev  = audioBrightness;                 // slow; hue/elevation tolerate lag
float field = uv.y*0.5 + elev*0.5;
vec3  col   = audioPalette(field);             // hue already keyed to chroma engine-side
col *= mix(0.7, 1.15, audioBrightness);        // bright timbre lifts value
```

### 5.2 Movement — eye tracks tempo and events
- **Feature(s):** `audioBeatPhase`/`audioBPM`/`audioTempo01` (continuous pulse), `audioOnsetRate` + `audioFlux` (events).
- **Param(s):** global motion speed; oscillation phase locked to `audioBeatPhase`; emission/scatter rate from `audioOnsetRate`.
- **Why clean:** lock to the *smoothed phase accumulator*, not per-beat jumps — motion glides and anticipates the downbeat instead of lurching after it.
```glsl
float speed = mix(0.4, 1.8, audioArousal);
float pulse = sin(audioBeatPhase*6.2831);      // beat-locked, continuous
vec2  p     = rotate(uv, TIME*speed + pulse*0.15);
float spawn = audioOnsetRate;                  // busier rhythm = more visible events
```

### 5.3 Flow — a continuous current the whole field rides
- **Feature(s):** `audioFlow` (vec2; magnitude from one-poled low band, direction slew-limited to phrase boundaries).
- **Param(s):** UV advection / domain-warp offset, particle wind, gradient scroll.
- **Why clean:** flow is slow and directional; it must never snap. `audioFlow` is pre-smoothed and slew-limited engine-side — the difference between "things jiggle" and "the scene moves like water."
```glsl
vec2  flow = audioFlow;                         // pre-smoothed, slew-limited
vec2  warp = uv + flow*0.12;
float n    = fbm(warp + TIME*0.05*length(flow));
col = mix(col, audioPalette(n), 0.4);
```

### 5.4 Noise — tonal vs broadband
- **Feature(s):** `audioFlatness` (Wiener entropy), backed by `audioZCR`.
- **Param(s):** dither / noise-mix amount, surface dryness, micro-static density.
- **Why clean:** flatness *is* the noisiness axis — pure tone reads 0, cymbal wash/distortion reads 1. Keep noise sub-pixel-soft (animated value noise), never white-pixel static.
```glsl
float grit = audioFlatness;
float nz   = valueNoise(uv*mix(40.,220.,grit) + TIME);   // finer when noisier
col = mix(col, col + (nz-0.5)*0.5, grit);                // tone stays clean
```

### 5.5 Grain — transient ticks → scattered, decaying specks
- **Feature(s):** `audioOnset` (peak-hold) + `audioTreble` (high-band weighting).
- **Param(s):** film-grain intensity, stipple/scatter density, chromatic-aberration micro-jitter.
- **Why clean:** grain is a transient texture — pop and vanish (instant attack, ~80ms decay), like a hi-hat tick. Decoupling from bass (`audioBeatPulse`) means hats and kicks drive *different* elements — the first taste of layering.
```glsl
float spark = audioOnset * (0.5 + 0.5*audioTreble);
float g     = hash(uv*900. + TIME*60.);
float grain = step(1.0 - spark*0.6, g);
col += grain * audioPalette(0.95) * spark;
```

### 5.6 Texture — surface quality crispy vs smooth
- **Feature(s):** `audioTexture` (smoothed tilt blended with flatness), τ≈250ms.
- **Param(s):** material roughness, edge sharpness vs blur, bloom threshold, contrast, noise *scale*.
- **Why clean:** texture is the bouba/kiki of timbre — bright/sibilant reads crispy, dark/round reads smooth. Slower than grain: it describes sustained character, not hits.
```glsl
float rough = audioTexture;
float blurR = mix(3.0, 0.0, rough);
col = mix(gaussian(col, blurR), sharpen(col, rough), rough);
col = mix(col, smoothstep(0.2, 0.8, col), rough*0.4);
```

### 5.7 Energy — the breathing intensity of the whole image
- **Feature(s):** `audioLevel` (instantaneous breath), `audioEnergy` (arrangement altitude), `audioArousal` (perceived intensity for grading). **Pick one per use** (critique fix P3-17): instantaneous loudness→`audioLevel`; altitude→`audioEnergy`; grading→`audioArousal`.
- **Param(s):** global exposure, saturation, contrast, bloom — moved *together*, plus scale/glow.
- **Why clean:** loudness→magnitude is innate; fast-attack/slow-release is what separates pro reactivity from twitchy meters. Perceptual curve (`^0.6`) so it never blows out.
```glsl
float e = pow(audioLevel, 0.6);
col *= mix(0.85, 1.25, e);                                       // exposure breathes
col  = mix(vec3(dot(col,vec3(0.33))), col, mix(0.7,1.15,e));     // saturation co-varies
```

### 5.8 Melancholy — low, slow, minor, dark
- **Feature(s):** low `audioValence` + low `audioArousal` + `audioMajorMinor`→0 + cool `audioWarmth`.
- **Param(s):** cool/desaturated/dim palette, slow drift, soft vignette, reduced contrast.
- **Why clean:** minor + dark + low-dynamics is a near-universal somber valence cue. Driven on a very slow τ (the affect scalars are already double-smoothed) it reads as a settled state, not a momentary dip.
```glsl
float melan = (1.0-audioValence)*(1.0-audioArousal);
col = mix(col, col*vec3(0.85,0.92,1.08), melan);   // cool
col = mix(col, vec3(dot(col,vec3(0.33))), melan*0.5);
col *= 1.0 - melan*0.25;
```

### 5.9 Charm — pleasant, relaxed, mid-energy groove
- **Feature(s):** `audioCharm` (widened Gaussian, floored — reaches on real material).
- **Param(s):** gentle sparkle/glow accents, warm highlight bloom, soft buoyant motion.
- **Why clean:** charm is a sweet spot, not an extreme — the Gaussian-on-arousal (σ=0.35) captures comfortable energy, `(1−tension)` keeps it relaxed.
```glsl
float charm = audioCharm;
col += charm * audioPalette(0.9) * 0.18;
col  = mix(col, col*vec3(1.05,1.0,0.96), charm*0.4);
```

### 5.10 Softness — gentle, blurred, sustained
- **Feature(s):** `audioSoftness` (now decorrelated from tension — built on `(1−punch)`, `(1−flux)`, low transient density).
- **Param(s):** Gaussian blur / bloom radius, low grain, slow easing, lifted black point (haze).
- **Why clean:** softness is the absence of transients. It is *not* `1−tension` — a soft minor pad is high-softness *and* moderate-tension; they must decorrelate (verified on the melancholic-pad probe).
```glsl
float soft = audioSoftness;
col = mix(col, gaussian(col, mix(0.0,6.0,soft)), soft);
col = mix(col, col+0.05, soft*0.5);                 // lifted blacks = haze
```

### 5.11 Mood — macro valence×arousal state
- **Feature(s):** `audioMood` (vec2 = valence×arousal), supported by `audioWarmth`/`audioTilt`.
- **Param(s):** palette quadrant + exposure + saturation + haze as one coherent grade.
- **Why clean:** collapsing dozens of features to a 2D affect point is the cleanest control surface — every quadrant maps to an unmistakable look. Slow τ makes mood a *state*. `audioMood.x` (valence) is `[0,1]`, used as a warm/cool *lerp factor* (not centered).
```glsl
float warm = audioMood.x;                           // valence 0..1
float live = audioMood.y;                           // arousal 0..1
col = mix(col*vec3(0.9,0.95,1.1), col*vec3(1.1,1.0,0.9), warm);
col *= mix(0.8, 1.2, live);
col = mix(col, vec3(dot(col,vec3(0.33))), (1.0-live)*0.4);
```

### 5.12 Vibe — overall feel from timbre + palette + brightness
- **Feature(s):** chroma palette + `audioWarmth` (temperature) + `audioBrightness`; `audioEnergy` undertone.
- **Param(s):** color-temperature grade over the chroma palette, highlight rolloff / tone curve.
- **Why clean:** vibe is the *combination* on long τ. Map warmth to a temperature *tint* (not hue) so it layers over the palette without fighting it.
```glsl
col = gradeTemp(col, audioWarmth);                  // warm/cool atmosphere
float knee = mix(0.6, 0.95, audioBrightness);
col = col / (col + (1.0 - knee));                   // tone curve sets the "air"
```

### 5.13 Palette — constrained, harmonious, key-aware color
- **Feature(s):** engine-side: chroma centroid→H, dispersion→scheme, `audioArousal`→sat, `audioBrightness`→value, `audioBeat`→accent pop.
- **Param(s):** the 3-stop Oklch ramp + accent, sampled via `audioPalette(t)`.
- **Why clean:** **never map audio to per-pixel hue.** Audio chooses position *within* a curated, capped-chroma ramp; only the accent spikes, only on onsets. Base hue smoothed circularly + slowly.
```glsl
vec3 base = audioPalette(luminanceField);           // shadow→mid→high, key-aware
col = base * mix(0.6, 1.0, audioPalSat);            // global vividness from arousal
```

### 5.14 Layering / different elements coming together — one element per instrument
- **Feature(s):** `audioPresence` (vec4 bass/lowMid/highMid/treble), `audioLayers`, `audioOccupancyTex`; percussive ratio `audioFlux*audioFlatness`.
- **Param(s):** crossfade distinct, pre-authored visual subsystems in/out; reveal layers as bands light up.
- **Why clean:** routing *different audio elements to different visual elements* makes a mix read as an assembly. `audioPresence` snaps in (A 400ms) and fades out gently (R 2s) so a visual layer appears *on time* when a stem enters, and sheds gracefully.
```glsl
vec3 c = vec3(0.0);
c += audioPresence.x * bassLayer(uv);               // bass → ground/mass
c += audioPresence.y * lowMidLayer(uv);             // body → mid structures
c += audioPresence.z * highMidLayer(uv);            // leads/vox → focal element
c += audioPresence.w * trebleLayer(uv);             // hats/air → sparkle field
float perc = audioFlux * audioFlatness;
c = mix(c, shake(c, perc), perc);                   // shake only percussive energy
```

### 5.15 Assembly / arrangement of a song — verse→chorus→bridge awareness
- **Feature(s):** `audioNovelty` (section change), `audioSectionPhase`, `audioSectionAge`; `audioBarPhase` for sub-structure.
- **Param(s):** on a boundary, *ease* between two pre-authored looks over 1–2s; `audioSectionAge` lets the look settle as a section matures.
- **Why clean:** arrangement is causal and structural (no future needed). Hysteresis (4s refractory) prevents chatter; easing + settling mirror how sections feel.
```glsl
float t = clamp(audioNovelty, 0.0, 1.0);
vec3  A = lookForSection(floor(audioSectionPhase*4.0));
vec3  B = lookForSection(floor(audioSectionPhase*4.0)+1.0);
col = mix(A, B, smoothstep(0.0,1.0,t));
col = mix(col*0.9, col, clamp(audioSectionAge,0.0,1.0));   // settles as it matures
```

### 5.16 The build-up — winding tension, then release of the drop
- **Feature(s):** `audioBuildup` (integrating, holds at top), `audioEnergyAcc` (earliest anticipation), `audioDrop` (release), `audioTension`.
- **Param(s):** one TENSION scalar several params lean on — zoom/convergence, rising fog/density, desaturation toward a held breath. On the drop: *release everything at once* (clamped burst into the bloom-pre buffer + snap reset).
- **Why clean:** the build is inherently a trend over bars, so it must be an *integrating* detector, not a level. One tension scalar tightening the whole image, then releasing on the resolving downbeat, *is* the felt experience; `audioEnergyAcc` lets the visual charge before the build is confirmed.
```glsl
float T = audioBuildup;                             // 0→1 over the riser, holds at top
uv  = (uv-0.5)*mix(1.0,0.82,T)+0.5;                 // converge / zoom
col = mix(col, vec3(dot(col,vec3(0.33))), T*0.4);   // desaturate toward held breath
col = mix(col, fog(col), T*0.5);
float d = clamp(audioDrop, 0.0, 1.0);               // refractory-gated impulse
col += d * audioPalette(1.0) * 1.0;                 // clamped HDR burst → bloom-pre
uv  = (uv-0.5)*(1.0 + d*0.3) + 0.5;                 // snap expansion = the exhale
```

### 5.17 Attribute → uniform coverage map

| User word | Primary uniforms |
|---|---|
| synesthesia | `audioChroma[]`/`audioChromaTex`, `audioBrightness`, `audioHCDF`, palette ramp |
| movement | `audioBeatPhase`, `audioTempo01`, `audioOnsetRate`, `audioFlux` |
| flow | `audioFlow` (vec2) |
| noise | `audioFlatness`, `audioZCR` |
| grain | `audioOnset` + `audioTreble` |
| texture | `audioTexture` |
| energy | `audioLevel` (breath), `audioEnergy` (altitude), `audioArousal` (grade) |
| melancholy | low `audioValence`+`audioArousal`, `audioMajorMinor`→0, cool `audioWarmth` |
| charm | `audioCharm` |
| softness | `audioSoftness` |
| mood / vibe | `audioMood` (vec2), `audioWarmth`, `audioTilt` |
| palette | `audioPalShadow/Mid/High/Accent`, `audioPalTemp/Sat` |
| layering / elements together | `audioPresence` (vec4), `audioLayers`, `audioOccupancyTex` |
| assembly / arrangement | `audioSectionPhase`, `audioSectionAge`, `audioNovelty` |
| build-up | `audioBuildup`, `audioEnergyAcc`, `audioBuildupRate`, then `audioDrop` |

---

## 6. Reusable GLSL Helper Include — `audio_react.glsl` (proposed)

Injected by `translateFragment` (after the uniform block) so every shader gets the same idioms. Find-replace color literals with `audioPalette(t)`.

```glsl
// ---- audio_react.glsl : shared idioms over the Audio Feature Bus ----
const float TAU = 6.28318530718;

// Sample the engine-built, key-aware, capped-chroma ramp + onset accent.
vec3 audioPalette(float t){
  t = clamp(t, 0.0, 1.0);
  vec3 c = (t < 0.5) ? mix(audioPalShadow, audioPalMid,  t*2.0)
                     : mix(audioPalMid,    audioPalHigh, t*2.0-1.0);
  return mix(c, audioPalAccent, audioBeat * smoothstep(0.6,1.0,t) * 0.6);
}

// Read the spectrum texture (256x1 R16F, log-spaced). f in [0,1].
float audioFFT(float f){ return texture(audioFFTTex, vec2(clamp(f,0.,1.), 0.5)).r; }

// Pitch-class energy (12x1). pc in [0,11].
float audioChromaAt(int pc){ return texture(audioChromaTex, vec2((float(pc)+0.5)/12.0, 0.5)).r; }

// Per-band presence (12x1 occupancy mask). band in [0,11].
float audioOccupancy(int band){ return texture(audioOccupancyTex, vec2((float(band)+0.5)/12.0,0.5)).r; }

// Living baseline: rest value + audio excursion (rule #2 — never gate to black).
float aliveBaseline(float rest, float drive, float amount){ return rest + drive*amount; }

// The one allowed snappy channel: transient → edge sharpness.
float kick(){  return audioBeatPulse; }   // bass transient
float onset(){ return audioOnset;     }   // broadband transient (hats/snare)

// Perceptual loudness curve for exposure-style use.
float audioBreath(){ return pow(audioLevel, 0.6); }
```

Authoring contract: read uniforms, never re-filter; one snappy channel via `kick()`/`onset()`; all color via `audioPalette()`; keep a `rest` value so silence looks intentional.

---

## 7. The Shader Audio-Reactivity Rubric (the test)

### 7.1 How it works

Each shader ships a one-line manifest; a headless harness drives a fixed probe battery, measures the right image statistic per probe, scores each *declared* dimension 0–4, and enforces two universal gates. A shader **passes** when every declared dimension scores ≥2 **and** both gates pass.

```glsl
// reacts: energy, build-up, palette, grain
// emphasis: build-up
```

### 7.2 Scored dimensions (aligned 1:1 with implemented uniforms — critique fix P5-23)

Flow is now scored because `audioFlow` exists. Dimensions map to the §3 bus.

| # | Dimension | Drivers | Image statistic | Probe | Expected shape |
|---|---|---|---|---|---|
| D1 | Synesthesia/Palette | chroma, brightness, palette | dominant hue, warm-cool tilt, in-gamut | pitch-class cycle; bright/dark A/B | monotonic |
| D2 | Movement | beatPhase, tempo01, onsetRate | optical-flow magnitude | 90→140 BPM ramp | monotonic |
| D3 | Flow | audioFlow | directional coherence of frame-diff | steady pad; bass surge | smooth/monotonic |
| D4 | Noise | flatness, zcr | high-spatial-freq energy | tone→noise crossfade | monotonic |
| D5 | Grain | onset, treble | transient stipple density, fast decay | hi-hat loop | unimodal-per-hit |
| D6 | Texture | texture | edge sharpness vs blur | narrow→wide-BW noise | monotonic |
| D7 | Energy | level, energy, arousal | mean luma + saturation tide | crescendo→decrescendo | monotonic, settling |
| D8 | Melancholy | valence/arousal/majorMinor | brightness, hue cool, saturation down | slow minor dark loop | monotonic |
| D9 | Charm/Softness | charm, softness | edge softness, warm tint, gentle motion | warm consonant mid-tempo | unimodal (charm) |
| D10 | Mood/Vibe | audioMood, warmth, tilt | two distinct coherent looks | bright-energetic vs dark-calm A/B | distinguishable pair |
| D11 | Layering/Assembly | presence, occupancy, novelty | layer/complexity (entropy) rising | add stems; verse→chorus | step-accumulate |
| D12 | Build-up | buildup, energyAcc, drop | frame-energy ramp; release step | 8-bar riser→drop | ramp-then-reset |
| D13 | Punch | punch, beatPulse | transient brightness/scale pop | kick loop | unimodal-per-hit |

**Shape-aware scoring (critique fix P5-24):** each dimension declares its expected response shape (monotonic / unimodal / step-with-reset / distinguishable-pair). Non-monotonic-by-design dimensions (charm, build-with-drop) are scored against their shape, not a blanket linear correlation.

**Universal gates:**
- **G1 Liveness floor:** inter-clip variance of frame statistics across the 4 canonical clips > εG1. Catches dead shaders.
- **G2 Naturalness:** (a) **no strobe** — distinguish high-frequency variance (strobe, fail) from low-frequency variance (idle breathing, allowed) by the *frequency* of the variance, not its magnitude (critique fix P5-25); (b) all hues in-gamut; (c) at silence/DC the image is stable, on-palette, no dead-black/blown-white (explicitly compatible with rule #2 "living baseline").

### 7.3 Scoring model (per declared dimension)

| Score | Label | Criteria |
|---|---|---|
| 0 | ABSENT | statistic doesn't move with probe (below noise floor) |
| 1 | WEAK | moves wrong direction, below threshold, or jittery/laggy |
| 2 | PASS | correct shape, settles within ~5τ, no strobe — **minimum to pass a declared dimension** |
| 3 | STRONG | clean, monotone/shape-correct, co-varies coherently with related params |
| 4 | EXEMPLARY | reads instantly to a human, anticipates (ramps *into* beat/drop), declared emphasis done beautifully |

```
PASS ⇔ (∀ declared d: score(d) ≥ 2) ∧ G1=pass ∧ G2=pass
```

### 7.4 Running it in Easel (semi-automated)

1. **`AudioFeatures` bus** (prerequisite) — the rubric scores against the pre-smoothed named uniforms, not raw FFT.
2. **Static gate (no render, CI):** manifest present; for each declared dimension the shader references ≥1 uniform from that dimension's feature set; flag Tier-0-only shaders (`audioLevel/Bass` only) declaring perceptual dimensions → WARN→must-render; anti-pattern scan for hardcoded synthetic BPM and per-pixel `hue=f(pitch)`.
3. **Visual gate (render-to-FBO):** replay each probe's precomputed `AudioFeatures` timeline through the offscreen `ShaderSource` path; read back downsampled frames; compute statistics (mean luma, saturation, dominant hue, R−B tilt, optical-flow magnitude + directional coherence, Sobel edge energy, high-freq spatial energy, spatial entropy, vertical center-of-mass); score against the declared shape with allowed lag = the dimension's release τ; run G1/G2.
4. **Deterministic replay (critique fix P5-26):** AGC and the 20s normalizers are stateful, so each probe gets a **5s pre-roll discarded** (or AGC frozen during replay) and the baked timeline records AGC at a fixed warm-up — same shader scores identically regardless of probe order.
5. **Defined ε per statistic** for G1/G2, measured as windowed variance.
6. **CMake target `test_audio_reactivity`** walks `shaders/` + `ShaderClaw3/shaders/`, runs static then render gate, emits a scorecard table, **returns 0 iff every shader passes its declared dimensions + both gates** — the analog of `test_bridge.sh`.
7. **Scorecard UI overlay + auto-suggest:** a "Reactivity" panel runs the harness on the loaded shader on demand, shows 0–4 bars + gate pass/fail + failing-probe thumbnails, and suggests the exact `audio_react.glsl` idiom to adopt for a flagged Tier-0-only shader.

### 7.5 Human pass (taste the metrics can't capture)

Drive 4 known tracks live: **soft melancholic** ("sad/intimate, not just dark"), **build-up+drop** ("the drop hits, on the beat"), **noisy/grainy** ("texture matches the dirt, grain soft-animated"), **charm groove** ("vibes, not loud"). Human PASS = intended attribute reads within ~2s and nothing looks garish/seizure-inducing.

### 7.6 Conformance checklist (per shader)

```
SHADER: ____________   reacts:[ .......... ]   emphasis: ______
— STATIC GATE —
[ ] manifest present, ≥1 known dimension
[ ] each declared dimension references ≥1 uniform from its feature set
[ ] not Tier-0-only while declaring perceptual D7–D12 (else WARN→render)
[ ] no hardcoded synthetic BPM when real beat uniforms exist
[ ] no per-pixel hue=f(pitch); palette-constrained instead
— DECLARED-DIMENSION SCORES (each ≥2) —
[ ] D1 Synesthesia/Palette ___   [ ] D2 Movement ___   [ ] D3 Flow ___
[ ] D4 Noise ___   [ ] D5 Grain ___   [ ] D6 Texture ___   [ ] D7 Energy ___
[ ] D8 Melancholy ___   [ ] D9 Charm/Softness ___   [ ] D10 Mood/Vibe ___
[ ] D11 Layering/Assembly ___   [ ] D12 Build-up ___   [ ] D13 Punch ___
— UNIVERSAL GATES —
[ ] G1 Liveness: differs across 4 canonical clips (var > εG1)
[ ] G2a no strobe (high-freq variance under steady input < εG2; idle breathing exempt)
[ ] G2b all hues in-gamut
[ ] G2c silence/DC: stable, on-palette, no dead-black/blown-white
— HUMAN PASS (4 tracks) —
[ ] melancholic reads sad/intimate <2s   [ ] build-up winds up + releases ON downbeat
[ ] noisy: texture matches dirt, grain soft   [ ] charm: warm, gentle, vibes
VERDICT: [ ] PASS  [ ] FAIL → __________
```

---

## 8. Adoption / Rollout Plan

### Migration rules (hard)
- **Legacy `audioLevel/Bass/Mid/High` are frozen on the exact existing pre-AGC pipeline** (critique fix P0-1). `audioMid` keeps its current single-band semantics; no legacy name is retargeted through new DSP/AGC. New split bands ship under new names only. This guarantees all ~117 existing shaders behave byte-for-byte identically on the day the bus ships.

### Phases
1. **Phase 0 — Plumbing skeleton.** Add `AudioFeatures.h`, `setAudioFeatures` + legacy shim, the uniform decl block, and `uploadUniforms` wiring. Assemble `af` in `Application.cpp`. No new DSP yet — fields default neutral. Ships invisibly; nothing changes.
2. **Phase 1 — Tier 1 (pure wiring, highest leverage).** Plumb the already-computed BPMSync `beat/beatPhase/beatPulse/barPhase/bpm` and the un-merged `lowMid`/`highMid` into the bus. **Instantly kills the fake-BPM hack** in `data_sculpture`/`shatter_grid` and gives kick/hat/vocal separation. No FFT change required.
3. **Phase 2 — Tier 2 spectral.** Add `computeSpectralFeatures` (centroid/spread/rolloff/flatness/flux/onset/tilt/zcr/punch/texture) + `prevSpec`. Optional FFT 512→1024/2048 + R16F texture upgrade (gated separately; verify R16F linear filtering on the GL context first).
4. **Phase 3 — Tier 5 palette + chroma.** `computeChroma`, `PaletteEngine`, `audioPalette()` in the include. Begin retrofitting color literals.
5. **Phase 4 — Tier 3 affect + Tier 4 structure.** `computeRoughness`/`computeAffect`/`computeStructure`/`computeFlow` per the DAG. Enables melancholy/charm/mood/vibe/build-up/layering/flow.
6. **Phase 5 — Rubric.** Build `test_audio_reactivity` (static + render gate), add manifests across the library, wire the UI scorecard overlay.
7. **Phase 6 — Path B unification.** Fold `AudioFeatures` into the compositor `AudioState`.

### Example retrofit — `crystalline_flow.fs`
1. Add manifest: `// reacts: flow, synesthesia, energy, palette` / `// emphasis: flow`.
2. Replace hand-rolled scroll vector with `audioFlow` (§5.3) — instant clean directional current.
3. Replace color literals / HSV ramp with `audioPalette(field)` (§5.13); convert any sRGB literals to linear.
4. Drive crystal facet brightness from `audioBreath()` instead of raw `audioLevel` (§5.7).
5. Add a subtle `audioOnset` sparkle on facet edges (§5.5) as the one snappy channel.
6. Run `test_audio_reactivity crystalline_flow.fs` → fix until D3/D1/D7 ≥2 and gates pass; eyeball on the 4 human tracks.
- **Effort:** ~30–60 min per shader once the bus + include exist; most are find-replace + 2–3 idiom adoptions.

### Effort & risk

| Item | Effort | Risk | Mitigation |
|---|---|---|---|
| Phase 0–1 plumbing | ~1–2 days | low | legacy shim + frozen legacy uniforms |
| Tier 2–4 DSP | ~1 week | medium (jitter/cycles) | DAG order, explicit derivative τ, AGC-vs-stable policy, smoothed roughness/flux floor |
| FFT 512→2048 + R16F | ~1 day | medium | gate separately; verify R16F linear filtering; honest sub expectations |
| Palette engine | ~2–3 days | medium (garish/flicker) | capped chroma, circular hue smoothing, reduced accent coeff |
| Rubric harness | ~3–5 days | medium | shape-aware scoring, defined ε, AGC pre-roll, idle-motion exemption |
| Library retrofit (~117) | incremental | low | mechanical; rubric gates each one |

**Top risks (all addressed in the spec):** silent semantic break of legacy names (frozen pipeline); AGC pumping (smooth→AGC→envelope ordering + AGC only on shape features); Tier 3↔4 cycle (strict DAG, tension reads prev-frame buildup); jittery derivatives/roughness/punch (explicit τ, peak-hold, smoothed peak set); garish/flickering palette (capped chroma, circular smoothing, reduced accent coeff, step-signal `audioDominantPitch` not animated); oversold FFT sub resolution (honest 1–2 bin proxy or 2048-pt); charm unreachable / softness collinear with tension (widened+floored charm, roughness dropped from softness); rubric scoring a feature that doesn't exist (Flow now has `audioFlow`); non-deterministic replay (AGC pre-roll); strobe gate vs living baseline conflict (variance-by-frequency, idle exemption).
