# Easel Audio Control Map

*Synthesized 2026-07-11 from three read-only audits: desktop control inventory (`/Users/lu/easel`, branch `easel-installation`), the capture→shader signal-path trace (Easel + ShaderClaw3), and the EaselMobile surface audit (branch `scratch/james-merge`; SDK at `/Users/lu/easel-agent-sdk`). Line numbers are from those working trees and will drift.*

---

## 1. Signal Path

```
                                CAPTURE
  ┌──────────────────────────────────────────────────────────────────┐
  │ ScreenCaptureKit system-audio loopback (default; gated behind    │
  │  m_wantsSystemAudio to avoid the Screen Recording TCC prompt —   │
  │  AudioAnalyzer_mac.mm:250-258)                                   │
  │ CoreAudio HAL mic input (explicit capture device;                │
  │  AudioAnalyzer_mac.mm:135-241)                                   │
  │ AudioMixer external feed (Enable Mixer → setExternalFeed(true),  │
  │  Application.cpp:761-770)                                        │
  │ Per-zone mic (micEnabled && pushToTalkActive → zone->micAnalyzer,│
  │  Application.cpp:909-911)                                        │
  └───────────────┬──────────────────────────────────────────────────┘
                  ▼  48kHz mono → 512-sample ring (kFFTSize, AudioAnalyzer.h:96)
                                ANALYZER  (AudioAnalyzer::update, AudioAnalyzer.cpp:53)
  ┌──────────────────────────────────────────────────────────────────┐
  │ 1. Hann + kiss_fft; linear RMS pre-AGC        (.cpp:294-336)     │
  │ 2. Bands: bass ~94-188Hz / lowMid ~282-940Hz /                   │
  │    highMid ~1-3.9kHz / treble ~4-12kHz        (.cpp:341-357)     │
  │ 3. Conditioning: per-band gain × master gain → dB-domain AGC →   │
  │    noise gate → per-band + Master AudioCurve (floor/ceil/gamma/  │
  │    S-curve) → asymmetric envelope (Ambient defaults: attack 3.0, │
  │    release 0.8)                               (.cpp:380, .h:328) │
  │ 4. Beat: dual fast/slow follower on pre-AGC bass energy (.h:348) │
  │ 5. Temperaments: Hit / Presence / Time per band  (.cpp:416)      │
  │ 6. Pseudo-stems via causal-median HPSS: Bass/Drums/Melody/Air/   │
  │    Vocal, each with Hit + Presence            (.cpp:447-468)     │
  │ 7. Tiers 2-5: spectral, affect, structure, palette/harmony       │
  │ 8. Tempo: autocorrelation 50-220 BPM + confidence → BPMSync      │
  │ 9. audioFFT texture: 128×1 GL_R8 spectrum     (.h:216-217)       │
  └───────────────┬──────────────────────────────────────────────────┘
                  ▼
              FEATURES → UNIFORMS + BINDINGS  (Application::updateSources, :1354)
  ┌──────────────────────────────────────────────────────────────────┐
  │ • Song-arc signals computed once per frame, NOT gated by the     │
  │   Audio→Shaders toggle: energy, build, drop, momentum, silence   │
  │   (:1366-1382)                                                   │
  │ • Zone-mic substitution: a layer owned by exactly one live-mic   │
  │   zone reads that zone's analyzer instead of the global one      │
  │   (:1409-1431)                                                   │
  │ • If m_audioToShaders: full AudioFeatures bus (incl. BPMSync     │
  │   beat/bar/2-16 phases, confidence-blended onBeat) pushed via    │
  │   ShaderSource::setAudioFeatures (:1473-1546); if off, audio     │
  │   state zeroed globally (:1548)                                  │
  │ • Per-param bindings (NOT gated by the toggle):                  │
  │   AudioBinding::follow — smoothing 0.85 default → attack 14→1.5, │
  │   release 7→0.7 /s taus; character −1…+1; conditioned 0-1 mapped │
  │   onto [rangeMin, rangeMax] (AudioBinding.h:61-81) — applied by  │
  │   ShaderSource/FluidSource/FluidSource3D::applyAudioBindings,    │
  │   writing parameter VALUES, not hidden uniforms                  │
  │ • Also fed: Particles, Hologram glitch, per-zone Compositor      │
  │   AudioState for blend effects                                   │
  └───────────────┬──────────────────────────────────────────────────┘
                  ▼
                               SHADER  (ShaderSource::uploadUniforms, :760-866)
  ┌──────────────────────────────────────────────────────────────────┐
  │ ~100 uniforms per pass: legacy quartet (audioLevel/Bass/Mid/High │
  │ + audioFFT sampler) · Tier 1 rhythm · Tier 2 spectral · Tier 3   │
  │ affect · Tier 4 structure · Tier 5 palette/harmony · temperament │
  │ matrix · rhythm bus · 15 stem* uniforms.                         │
  │ ShaderClaw3 house pattern (AUDIO_REACTIVITY_PLAYBOOK.md): soft-  │
  │ knee conditioning, structure-on-beats / texture-on-levels /      │
  │ color-on-character, drive floor ≥0.25 so silence stays alive.    │
  └──────────────────────────────────────────────────────────────────┘
```

Spec of record for the analyzer contract: `/Users/lu/ShaderClaw3/docs/EASELAUDIO_SPEC.md`. Authoring contract for shaders: `/Users/lu/ShaderClaw3/docs/AUDIO_REACTIVITY_PLAYBOOK.md`.

---

## 2. Control Inventory

Surfaces: **AUDIO panel** = desktop Audio panel · **PropertyPanel** = per-layer desktop UI · **OSC** = remote UDP surface · **SDK** = easel-agent-sdk HTTP action · **Mobile** = EaselMobile UI. "—" in the OSC column means no remote address exists.

### 2.1 Capture & input

| Control | Surface | Range / default | OSC address | file:line |
|---|---|---|---|---|
| Input device combo `##AudioInput` | AUDIO panel | System Audio (loopback, **default**) + FFmpeg-enumerated mics/loopbacks; not persisted | — | `Application.cpp:6764-6778` |
| Refresh devices | AUDIO panel | re-enumerates via `VideoRecorder::enumerateAudioDevices()` | — | `Application.cpp:6780` |
| System-audio opt-in gate | internal | `m_wantsSystemAudio`, off until user picks System Audio (TCC-prompt avoidance) | — | `AudioAnalyzer_mac.mm:255`, `AudioAnalyzer.h:122-123` |
| Zone mic enable + device | OSC only (no desktop widget) | `micEnabled` default false, `micDeviceId` default "" (system default); **persisted in project JSON** | `/easel/zone/mic/enable [zoneIndex, 0/1, deviceId]` | `OutputZone.h:49-58`, `Application.cpp:3421-3436` |
| Zone mic push-to-talk | OSC only | momentary, honored only when `micEnabled`; transient | `/easel/zone/mic/ptt [zoneIndex, 0/1]` | `Application.cpp:3437-3445` |

### 2.2 Signal conditioning (all runtime-only — nothing here persists)

| Control | Surface | Range / default | OSC address | file:line |
|---|---|---|---|---|
| Per-band gain ×4 `##bandGain` | AUDIO panel | 0.0–5.0×, default 1.0 | — | `Application.cpp:6838-6850`, `AudioAnalyzer.h:317-320` |
| Input (master gain) `##masterGain` | AUDIO panel | 0–10×, default 1.0 (reset on device change) | — | `Application.cpp:6893`, `AudioAnalyzer.h:316` |
| Gate `##nGate` | AUDIO panel | 0–0.5, default 0.0 | — | `Application.cpp:6894`, `AudioAnalyzer.h:321` |
| Attack `##audAttack` | AUDIO panel | 0.5–30 /s, boot default 3.0 | — | `Application.cpp:6898`, `AudioAnalyzer.h:341` |
| Release `##audRelease` | AUDIO panel | 0.5–30 /s, boot default 0.8 | — | `Application.cpp:6899`, `AudioAnalyzer.h:342` |
| Curve preset combo `##audCurvePreset` | AUDIO panel | 9 presets; boot default **Ambient** | — | `Application.cpp:6940-6965`, `AudioAnalyzer.h:70-90` |
| Apply-to-all-bands checkbox | AUDIO panel | bool, default true | — | `Application.cpp:6968` |
| Band selector (Master/Bass/Low/High/Treble) | AUDIO panel | `CurveBand` enum, default Bass | — | `Application.cpp:6976-6983`, `AudioAnalyzer.h:95` |
| Curve `##cvExp` (gamma) | AUDIO panel | 0.20–4.0, default 1.0 | — | `Application.cpp:7019` |
| Floor `##cvFloor` | AUDIO panel | 0–0.9, default 0.0 (clamped to Ceil−0.02) | — | `Application.cpp:7020` |
| Ceil `##cvCeil` | AUDIO panel | 0.10–1.0, default 1.0 | — | `Application.cpp:7021` |
| S-Curve `##cvCon` (contrast) | AUDIO panel | 0–1, default 0.0 | — | `Application.cpp:7022` |
| Reset Curve / Reset Gains | AUDIO panel | identity curve; gains→1, gate→0, attack/release→8/3 | — | `Application.cpp:7026, 7030-7040` |

### 2.3 Mixer (HAS_FFMPEG; all runtime-only)

| Control | Surface | Range / default | OSC address | file:line |
|---|---|---|---|---|
| Enable Mixer | AUDIO panel | bool, default off → `setExternalFeed(true)` | — | `Application.cpp:7046-7056` |
| OUTPUT combo `##MixerOut` | AUDIO panel | Default (−1) / None-NDI-only (−2) / enumerated outputs | — | `Application.cpp:7063-7081` |
| Master volume `##Master` | AUDIO panel | 0–100 % | — | `Application.cpp:7085` |
| Send NDI Audio (HAS_NDI) | AUDIO panel | bool → publishes "Easel Audio" | — | `Application.cpp:7091` |
| Per-input mute / volume / remove | AUDIO panel | per input, 0–100 % | — | `Application.cpp:7102-7121` |
| + Add Input combo | AUDIO panel | any enumerated device | — | `Application.cpp:7124-7135` |

### 2.4 Tempo

| Control | Surface | Range / default | OSC address | file:line |
|---|---|---|---|---|
| TAP button | AUDIO panel + OSC | tap tempo → Manual clock | `/easel/tap` | `Application.cpp:7183, 3240` |
| BPM drag `##BPMVal` | AUDIO panel + OSC | 0–300, default 0 (Free/auto-detect); published in play-state JSON but not restored | `/easel/bpm <float>` | `Application.cpp:7188, 3238, 13177` |
| Reset (→ Free) | AUDIO panel | `setBPM(0)` + `resetPhase()` re-enables autocorrelation | — | `Application.cpp:7194-7197` |
| MIDI mapping targets | MIDI | `BPMSet` (40–240), `BPMTap` | — | `Application.cpp:3201-3205` |

### 2.5 Per-layer reactivity macro (AudioPresetEngine — shared by panel, OSC, and mobile)

| Control | Surface | Range / default | OSC address | file:line |
|---|---|---|---|---|
| Reactivity pill | PropertyPanel + OSC + **Mobile** | 0–1, default 0.22; depth 8 %→50 % of param range; auto-shuffles if nothing bound | `/easel/layer/audiopreset [key, "intensity", f]` | `PropertyPanel.cpp:1223-1231`, `AudioPresetEngine.cpp:18-42`, `Application.cpp:3362` |
| Punch pill (character) | PropertyPanel + OSC + **Mobile** | −1…+1, default −0.5 (maps to neutral conditioner 0) | `…["character", f]` | `PropertyPanel.cpp:1245-1253`, `AudioPresetEngine.cpp:13-16` |
| On | PropertyPanel + OSC (desktop only) | restores stashed recipe or shuffles fresh | `…["on"]` — **not in SDK/mobile** | `PropertyPanel.cpp:1277-1284`, `AudioPresetEngine.cpp:92-110` |
| Shuffle | PropertyPanel + OSC + **Mobile** | ~5 random params on continuous signals only (Level/Bass/Mid/High/Energy/Build) | `…["shuffle"]` | `PropertyPanel.cpp:1286`, `AudioPresetEngine.cpp:44-66` |
| Off | PropertyPanel + OSC + **Mobile** | clears bindings, stashes recipe | `…["off"]` | `PropertyPanel.cpp:1292` |

Knob/recipe state is runtime-only; the resulting **bindings persist** per layer in project JSON (`Application.cpp:13780-13798`) and `adoptExisting` reconstructs a recipe on load (`PropertyPanel.cpp:1205`).

### 2.6 Per-param sparkle bind (desktop only — no OSC surface at all)

| Control | Surface | Range / default | OSC address | file:line |
|---|---|---|---|---|
| SOURCE combo `##sig` | PropertyPanel popup | `AudioSignal`: None (default), Level, Bass, Mid, High, Beat, MIDI, Energy, Build, Drop, Silence, Momentum | — | `PropertyPanel.cpp:1032-1050`, `AudioBinding.h:11-29` |
| MIDI CC / Ch / Learn | PropertyPanel popup | CC 0–127 (default −1), Ch 1–16 (0 = any) | — | `PropertyPanel.cpp:1052-1120` |
| OUTPUT RANGE `##arng` + Min/Max | PropertyPanel popup + inline row | within param's [lo, hi]; new binding defaults to full range; Min > Max inverts | — | `PropertyPanel.cpp:1125-1136, 3345-3355, 3606-3637` |
| SMOOTHING Amount pill | PropertyPanel popup | 0–1, default 0.85 → attack 14→1.5, release 7→0.7 /s | — | `PropertyPanel.cpp:1138-1139`, `AudioBinding.h:40, 69-74` |
| CHARACTER pill | PropertyPanel popup | −1…+1, default 0.0 (exact legacy feel) | — | `PropertyPanel.cpp:1141-1146`, `AudioBinding.h:44` |
| Live tick markers + `live %.3f` readout | PropertyPanel (display) | driven value, never serialized outward | — | `PropertyPanel.cpp:670-700, 940-965` |
| Fluid `sndSmoothing` param | PropertyPanel | 0–1 | (settable via `shader.param` if exposed) | `PropertyPanel.cpp:3540-3543` |

Persisted per layer as `"audioBindings"` (param, signal int, rangeMin/Max, smoothing, character if ≠0, midiCC/midiChannel).

### 2.7 Global toggles

| Control | Surface | Range / default | OSC address | file:line |
|---|---|---|---|---|
| Audio → Shaders | View menu + OSC + SDK (`easel.audio.set`, unused in mobile UI) | bool `m_audioToShaders`; not persisted; gates uniforms only, not sparkle bindings or song-arc signals | `/easel/audio/toshaders <0/1>` | `Application.cpp:11846, 3446-3448, 1473` |
| Voice tab (legacy) | dead code, `if (false …)` | mic on/off, capture picker, Decay 0.5–10 s | — | `Application.cpp:4781-4896` |

### 2.8 Outbound telemetry

Every audio-feature float is *intended* to emit per frame as `/easel/audio/<uniformName>` (`Application.cpp:824-884`): audioLevel/Bass/Mid/High/Sub/Treble, Energy, Brightness, Punch, BeatPulse, Onset, Beat, per-band Hit/Presence/Time, BPM + confidence, beat/bar/2-16 phases, OnBeat, ToggleOnBeat. The SDK's `audio_features.py` listens for exactly this bus on UDP :9001 — but per the mobile audit, **only `audioBPM` is live today**; the per-feature emission is scaffolded, not shipped.

---

## 3. Mobile's Current Reach

Everything mobile can do today, all **open-loop / write-only** (no readback of binding state or driven values):

1. **Audiopreset macro** — `AudioPresetControls` (`ShaderParamsSheet.swift:128`, also on the Home Controls section, `SDKHomeView.swift:225`): Reactivity knob (0–1, 120 ms throttle), Punch knob (−1…1), Shuffle, Off. Path: `ShowController.setAudioPreset` (`ShowController.swift:883`) → `POST /api/actions/shader.audiopreset` → validated against `_AUDIOPRESET_COMMANDS = ("intensity","character","shuffle","off")` (`actions.py:1434`) → OSC `/easel/layer/audiopreset` (`adapters.py:690-716`). **No On button** — the command isn't in the SDK tuple.
2. **Raw ISF param sets** — `shader.param` → `/easel/layer/param`; "Audio Reactivity" ISF group is listed first (`ShowController.swift:1537`); gallery badges shaders with ≥3 such params (`GalleryView.swift:195`).
3. **Master motion gate** — `easel.audio.set` → `/easel/audio/toshaders` exists in the SDK (`actions.py:371`) but has **no mobile UI**.
4. **Remote/zone mic** — `remote.audio.on/off/status` SSH-pipes a remote box's mic into the show and *internally* drives zone mic enable + PTT (`actions.py:2851-2892`). `zones.mic.devices` / `zones.mic.set` can enumerate and arm a zone mic. No standalone hold-to-talk; a per-zone mic picker UI was tried and reverted 2026-07-09 (`ShowController.swift:890`).
5. **Fluid** — selectable via `flux.shader.set` id `fluid` (→ `/easel/layer/ensure/fluid`), and the audiopreset macro works on it — but its list entry carries `params: []`, so zero Fluid-specific params are editable from the phone. **Fluid3D is entirely unreachable** (no ensure verb, no list entry).
6. **Local on-phone analysis** — `Core/Audio/EaselAudio.swift` runs a full DSP bus feeding the Play tab's `ShaderWebView`; phone-mic-local only, never reaches the desktop.
7. **Telemetry** — `GET /api/audio/features` relays the `/easel/audio/*` bus, but only `audioBPM` flows today.

---

## 4. Gaps

Ranked roughly by value ÷ effort:

| # | Gap | Desktop | SDK | Mobile | Fix |
|---|---|---|---|---|---|
| 1 | **`on` audiopreset command** | ✅ OSC handles it | ❌ not in `_AUDIOPRESET_COMMANDS` (`actions.py:1434`) | ❌ only Shuffle/Off | Cheapest win: add `"on"` to the SDK tuple + a mobile button. (Desktop OSC already accepts it per the desktop audit; verify `setManagedLayerAudioPreset` ~`Application.cpp:12904` handles it end-to-end.) |
| 2 | **Zone-mic push-to-talk from the phone** | ✅ OSC `/easel/zone/mic/ptt` | ⚠️ `set_zone_mic_ptt` exists but only fired internally by `remote.audio.*` | ⚠️ side-effect only | Add a standalone `zones.mic.ptt` SDK action → real hold-button on mobile. |
| 3 | **Live feedback channel** (`/easel/audio/*` bus + per-binding driven values) | ⚠️ outbound send written (`Application.cpp:824-884`) but only `audioBPM` observed live; `AudioBinding.smoothedValue` is panel-only | ⚠️ `audio_features.py` relay is built and waiting | ❌ knobs are write-only by design | Light up full bus emission on desktop; `GET /api/audio/features` then works with zero SDK changes. Per-binding driven values could ride the same bus. |
| 4 | **Per-param bind editing** (signal / range / smoothing / character) | ✅ full popup UI | ❌ no OSC verb exists | ❌ | Blocked on a new desktop OSC address (e.g. `/easel/layer/audiobind`). Bindings are already serialized in project JSON, so the data model is ready. |
| 5 | **Band conditioning remotely** (per-band gains, master gain, gate, attack/release, curves) | ✅ AUDIO panel | ❌ nothing — only the all-or-nothing toshaders gate | ❌ | No OSC surface at all for the conditioning chain. Also note none of it persists even on desktop. |
| 6 | **Master analyzer input-device selection** | ✅ device combo | ⚠️ only `audio.mode` (radio/mic) + per-zone `zones.mic.set` | ❌ (picker reverted) | Needs a master-analyzer device verb; complicated by the TCC opt-in gate. |
| 7 | **Fluid params / Fluid3D from mobile** | ✅ full UI for both | ⚠️ Fluid ensure-able but `params: []`; Fluid3D absent | ⚠️ / ❌ | Publish Fluid's bindable param table in `/api/shader/list`; add a Fluid3D ensure verb. |
| 8 | **AUDIO-panel persistence** (desktop-local gap) | ❌ device, gains, gate, attack/release, curves, mixer config all reset every launch | n/a | n/a | Serialize analyzer conditioning state into the project JSON alongside `audioBindings`. BPM is published in play-state but never restored. |

### Persistence summary (desktop)

| Persisted in project JSON | Runtime-only (lost on quit) |
|---|---|
| Per-layer `audioBindings` (signal, range, smoothing, character, MIDI CC/ch) | Entire AUDIO panel: input device, band gains, master gain, gate, attack/release, curves + preset, mixer |
| Zone `micEnabled` + `micDeviceId` | AudioPresetEngine knob state + recipes (rebuilt via `adoptExisting`) |
| — | BPM (published, not restored), `m_audioToShaders`, `pushToTalkActive` |
