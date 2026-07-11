# Mobile Audio-Reactivity Control Plan

*Written 2026-07-11. Companion to `docs/AUDIO_CONTROL_MAP.md` (the control inventory this plan is built from). Desktop tree: `/Users/lu/easel` (branch `easel-installation`). Mobile tree: `/Users/lu/easel-mobile-james-merge-scratch/EaselMobile` (branch `scratch/james-merge`). SDK: `/Users/lu/easel-agent-sdk`.*

Design language: everything below matches the existing mobile controls sheet — monochrome, tall pill sliders (`ToneSlider`, `ShaderParamsSheet.swift:386`), semantic words instead of numbers (Subtle/Medium/Intense for Reactivity, Smooth/Classic/Chopped for Punch), and the On/Shuffle/Off row with a lit On state that desktop just adopted *from* mobile. Mobile is the reference aesthetic; nothing here introduces a new visual idiom.

---

## 1. Control set by tier

### Tier 1 — ship first (macro parity + house-wide push)

| Control | What it does | Endpoint | Status |
|---|---|---|---|
| **On / Shuffle / Off row** (per layer) | On restores the stashed recipe or shuffles fresh (`AudioPresetEngine::on()`, `src/sources/AudioPresetEngine.cpp:92-110`); Shuffle rerolls ~5 bindings; Off clears + stashes. On renders lit when the layer has bindings, matching desktop's new row. | OSC `/easel/layer/audiopreset [key, "on"|"shuffle"|"off"]` (`Application.cpp:3362` → `setManagedLayerAudioPreset` `:12894`). SDK `shader.audiopreset` (`actions.py:1436`). | OSC ✅. SDK ❌ for `"on"` — **missing**: add `"on"` to `_AUDIOPRESET_COMMANDS` (`actions.py:1434`) and pass it through `adapters.py:690-716` (no new args; identical shape to `shuffle`/`off`). |
| **Reactivity pill** (per layer) | 0–1 intensity, labeled Subtle → Intense. Already shipped. | SDK `shader.audiopreset ["intensity", f]` → OSC `/easel/layer/audiopreset`. | ✅ exists (`ShowController.swift:883`, 120 ms throttle). |
| **Punch pill** (per layer) | −1…+1 character, labeled Smooth → Chopped. Already shipped. | SDK `shader.audiopreset ["character", f]`. | ✅ exists. |
| **Everywhere button** (per layer) | Pushes the current layer's look house-wide: every zone renders only this layer. | OSC `/easel/layer/allzones <layerIndex> [solo=1]` (`Application.cpp:3408-3423` → `soloLayerAcrossZones`). | OSC ✅ (just landed). SDK ❌ — **missing**: new action `layer.allzones` (see §2). |

Tier 1 ships with **no new C++**; both missing pieces are SDK-tuple/action additions.

### Tier 2 — per-param bind editing

| Control | What it does | Endpoint | Status |
|---|---|---|---|
| **Bind list** (per layer) | Rows for each param with an active `AudioBinding`: signal name + range, mirroring the desktop sparkle popup (`PropertyPanel.cpp:1032-1146`). | Readback: bindings persist per layer as `"audioBindings"` in project JSON (`Application.cpp:13780-13798`); the SDK already holds `config_path` (see `set_layer_audiopreset`, `actions.py:1466`), so a read-side `shader.audiobind.list` can parse project JSON with zero desktop changes. | SDK ❌ — **missing**: `shader.audiobind.list`. |
| **Signal picker** (per param) | Choose Level / Bass / Mid / High / Beat / Energy / Build / Drop / Silence / Momentum / None (the `AudioSignal` enum, `AudioBinding.h:11-29`). | OSC ❌ — **missing**: `/easel/layer/audiobind [layerKey, paramName, signalName] [rangeMin, rangeMax, smoothing, character]` (strings ×3, floats ×4; `signalName` `"none"` deletes the binding). Handler in `Application.cpp` beside the audiopreset case at `:3362`, delegating to a new `setManagedLayerAudioBind()` next to `setManagedLayerAudioPreset` (`:12894`); it mutates the layer source's `audioBindings` the same way the PropertyPanel popup does. SDK ❌ — **missing**: `shader.audiobind` action wrapping it. |
| **Range editing** (per param) | Min/Max within the param's [lo, hi]; Min > Max inverts, matching desktop (`PropertyPanel.cpp:3345-3355`). | Same `/easel/layer/audiobind` message (floats 0–1 of the param range, resolved desktop-side against the ISF declaration). | Same missing endpoints as above. |
| **Smoothing + Character pills** (per binding, behind a disclosure) | 0–1 smoothing (default 0.85) and −1…+1 character (default 0), same semantics as `AudioBinding.h:40-44`. | Same `/easel/layer/audiobind` message. | Same. |

### Tier 3 — closing the loop

| Control | What it does | Endpoint | Status |
|---|---|---|---|
| **Live driven-value ticks** on bound sliders | The tick marker desktop just gained (`PropertyPanel.cpp:670-700`) rendered on mobile's `ToneSlider`: the pill shows the audio-driven value dancing on the track while the thumb stays at the user's base value. | Needs the feedback channel designed in §3. Desktop ❌ (bus scaffolded, only `audioBPM` live — `Application.cpp:824-884`), SDK ⚠️ (relay built, no stream endpoint), mobile ❌. | **Missing**: full `/easel/audio/*` emission, new `/easel/binding` OSC message, SDK `GET /api/audio/stream` (SSE). |
| **Audio input picker** | Choose the master analyzer's capture device (System Audio vs a mic), the mobile face of the `##AudioInput` combo (`Application.cpp:6764-6778`). Respects the TCC opt-in gate (`AudioAnalyzer_mac.mm:255`): System Audio stays a deliberate first-tap with an explanatory footnote. | OSC ❌ — **missing**: `/easel/audio/device [deviceName]` (string; `"system"` selects loopback and sets `m_wantsSystemAudio`) + `/easel/audio/devices` query answered on the outbound socket as `/easel/audio/devicelist [json]`. Handler in `Application.cpp` near the toshaders case (`:3446`), reusing the combo's device-switch code path. SDK ❌ — **missing**: `audio.device.list` / `audio.device.set`. | Missing at every hop. |
| **Zone mic PTT** | Hold-to-talk button per zone: finger down → `ptt 1`, up → `ptt 0`. Only enabled when the zone has `micEnabled`. | OSC ✅ `/easel/zone/mic/ptt [zoneIndex, 0/1]` (`Application.cpp:3437-3445`); `zones.mic.set` already arms the mic. SDK ⚠️ — `set_zone_mic_ptt` exists in `adapters.py` but is only fired internally by `remote.audio.*` (`actions.py:2851-2892`). **Missing**: standalone `zones.mic.ptt` action. | One SDK action away. |

---

## 2. Endpoint reference (exact, per control)

Existing — used as-is:

- `POST /api/actions/shader.audiopreset` `{target, command, value?}` → OSC `/easel/layer/audiopreset [key, command] [value]` — Reactivity, Punch, Shuffle, Off (and On once the tuple is patched).
- `POST /api/actions/zones.mic.set` → OSC `/easel/zone/mic/enable [zi, 0/1, deviceId]` — arming a zone mic before PTT.
- `GET /api/audio/features` (`audio_features.py`, UDP :9001 relay) — polling readback; today only `audioBPM` flows.

Missing — must be built (each mapped to the file that owns it):

| # | Endpoint | Shape | Where it's built |
|---|---|---|---|
| 1 | SDK `shader.audiopreset` accepts `"on"` | add to `_AUDIOPRESET_COMMANDS` | `easel-agent-sdk/src/easel_agent/actions.py:1434`; desktop handler already accepts it |
| 2 | SDK `layer.allzones` | `{layerIndex:int, solo:bool=true}` → OSC `/easel/layer/allzones <idx> <solo>` | new `_layer_allzones` in `actions.py` + a two-int OSC send in `adapters.py`; desktop handler exists (`Application.cpp:3408`) |
| 3 | OSC `/easel/layer/audiobind` | strings `[layerKey, paramName, signalName]`, floats `[rangeMin, rangeMax, smoothing, character]`; `"none"` deletes | new `setManagedLayerAudioBind()` in `Application.cpp` (registered beside `:3362`, implemented beside `:12894`) |
| 4 | SDK `shader.audiobind` | `{target, param, signal, rangeMin?, rangeMax?, smoothing?, character?}` → #3 | `actions.py` + `adapters.py` |
| 5 | SDK `shader.audiobind.list` | `{target}` → parsed `"audioBindings"` from project JSON | `actions.py`, read-only, no OSC needed |
| 6 | Desktop full `/easel/audio/*` bus emission | finish the scaffolded per-feature send | `Application.cpp:824-884` (throttle: 20 Hz, skip unchanged values) |
| 7 | OSC outbound `/easel/binding` | strings `[layerKey, paramName]`, float `[drivenValue]`, per bound param at 20 Hz | new emit in the same block as #6, reading `AudioBinding::smoothedValue` |
| 8 | SDK `GET /api/audio/stream` | SSE stream of #6 + #7 (see §3) | `audio_features.py` + the HTTP server it plugs into |
| 9 | OSC `/easel/audio/device` + `/easel/audio/devices` | string device select / JSON list reply | `Application.cpp` near `:3446`, reusing `##AudioInput` combo logic |
| 10 | SDK `audio.device.list` / `audio.device.set` | wraps #9 | `actions.py` + `adapters.py` |
| 11 | SDK `zones.mic.ptt` | `{zoneIndex:int, active:bool}` → OSC `/easel/zone/mic/ptt` | `actions.py`, exposing the existing `adapters.py` `set_zone_mic_ptt` |

## 3. Feedback channel design (Tier 3 prerequisite)

Today the whole mobile surface is open-loop. The loop closes in three hops, each independently useful:

1. **Desktop emits** (`Application.cpp:824-884`): light up the already-written per-feature sends — every `AudioFeatures` float as `/easel/audio/<uniformName>` — plus the new `/easel/binding [layerKey, param, value]` for each active binding's `smoothedValue`. Throttle to 20 Hz and suppress sends when a value moved < 0.005 since last emit; at ~30 bound params this is well under 1 KB/frame of UDP.
2. **SDK relays** (`audio_features.py`): the UDP :9001 listener already parses this bus into a latest-values dict. Add `GET /api/audio/stream` — Server-Sent Events, one `data:` frame per 50 ms containing the merged `{features: {...}, bindings: {"layerKey/param": value}}` delta since the last frame. SSE (not WebSocket) because the phone only ever reads; reconnection is free via `Last-Event-ID`.
3. **Mobile consumes**: a small `AudioFeatureStream` actor in `Core/SDK/` (beside `ShowController.swift`) holding an `URLSession` SSE task, publishing `@Observable` dictionaries. `ToneSlider` gains an optional `liveValue: Double?`; when non-nil it draws the same minimal tick the desktop sliders use. Stream connects only while a params sheet is visible — no background battery cost.

Fallback ordering: hop 1 alone makes `GET /api/audio/features` (already built) fully live for polling; hops 2–3 add push latency good enough for dancing ticks (~100–150 ms end-to-end over LAN).

## 4. SwiftUI surface

All paths under `EaselMobile/Features/SDKHome/` unless noted.

- **`ShaderParamsSheet.swift` — "Audio Reactivity" section (`:37`)**: the Tier 1 home. Extend `AudioPresetControls` (`:128`): the current Shuffle/Off button pair (`:144`) becomes an **On / Shuffle / Off** three-segment row, On lit (filled monochrome pill) when the layer has bindings — exactly the desktop row's grammar, which itself copied this sheet. Reactivity and Punch pills stay as-is above it.
- **`ShaderParamsSheet.swift` — section footer**: the **Everywhere** button, full-width quiet pill directly under the audio group ("Everywhere · this look on every zone"). It acts on the sheet's target layer, so it belongs with the layer's controls, not in ZonesView. Mirror it in the Home Controls copy of `AudioPresetControls` (`SDKHomeView.swift:225`).
- **`ShaderParamRow.swift` (`ShaderParamsSheet.swift:251`) — Tier 2**: a small signal glyph on any bound row; tapping it (or long-pressing the row) opens a **bind editor sheet** — signal picker (horizontal pill row: None Level Bass Mid High Beat Energy Build Drop Silence Momentum), a range-bracket editor drawn on a copy of the param's ToneSlider, and Smoothing/Character pills behind a "Feel" disclosure. One `shader.audiobind` call per commit, 120 ms-throttled like the existing knobs.
- **`ToneSlider` (`ShaderParamsSheet.swift:386`) — Tier 3**: optional live tick marker fed by `AudioFeatureStream`, visually identical to the desktop's new tick.
- **`ZonesView.swift` — Tier 3**: per-zone **hold-to-talk** circle button on each zone row, visible only when the zone is mic-armed (`zones.mic.set`); pressed state fills the circle, release sends `ptt 0`. A safety `onDisappear`/scenePhase hook always sends `ptt 0`.
- **Audio input picker — Tier 3**: a row in `SDKSettingsView.swift` (not the params sheet — it's global, not per-layer): "Audio Input" → device list from `audio.device.list`, System Audio flagged with the one-line TCC note.

## 5. Milestones

**M1 — On everywhere the macro already is** (SDK + Swift only)
Add `"on"` to `_AUDIOPRESET_COMMANDS`; reshape `AudioPresetControls` into the On/Shuffle/Off row (both sheet and Home). Lit-state heuristic: On lights after any on/shuffle and after Reactivity > 0 on a layer that auto-shuffled; clears on Off.
*Verify*: with music playing, Off then On from the phone → desktop stderr shows `[OSC] layer/audiopreset: <key> on` and the same bindings return (compare the desktop popup before/after); On from a never-bound layer shuffles fresh; SDK unit test rejects nothing in the new tuple.

**M2 — Everywhere** (SDK + Swift)
New `layer.allzones` action; Everywhere button in both surfaces.
*Verify*: 3-zone project, tap Everywhere on layer 2 → every zone renders only layer 2 (desktop zone panel confirms solo sets); repeat with `solo:false` via curl to confirm the non-solo arg passes through.

**M3 — Bind editing** (first C++ milestone)
`/easel/layer/audiobind` handler + `setManagedLayerAudioBind()`; SDK `shader.audiobind` + `shader.audiobind.list`; mobile bind editor sheet.
*Verify*: bind Bass→`speed` from the phone → desktop sparkle popup shows Bass with the same range; save + reload the project → binding persists (`"audioBindings"` in project JSON matches); signal `"none"` from the phone removes it; range Min > Max inverts direction visibly.

**M4 — Live feedback** (desktop emit → SDK SSE → ticks)
Light up the `/easel/audio/*` bus + `/easel/binding` emission; `GET /api/audio/stream`; `AudioFeatureStream` + ToneSlider ticks.
*Verify*: `nc -ul 9001` shows the full feature bus at ~20 Hz (not just `audioBPM`); `curl -N /api/audio/stream` streams deltas; on the phone, a Bass-bound slider's tick dances with the kick and freezes ≤ 1 s after pausing music; Instruments shows no stream traffic once the sheet closes.

**M5 — Input picker + zone PTT**
`/easel/audio/device(s)` OSC pair + `audio.device.*` SDK actions + settings row; `zones.mic.ptt` action + hold-button in ZonesView.
*Verify*: switch to a USB mic from the phone → desktop `##AudioInput` combo reflects it and levels respond to the mic; System Audio selection triggers TCC only on first-ever use; hold the PTT button while speaking → only that zone's layer reacts (zone-mic substitution, `Application.cpp:1409-1431`), release stops within one frame; backgrounding the app mid-hold releases PTT.

Dependency notes: M1/M2 are independent and can land same-day. M3 is independent of M4. M4 hop 1 (desktop emit) can land any time and immediately improves the existing `/api/audio/features` poll. M5 is independent of everything except its own SDK plumbing.
