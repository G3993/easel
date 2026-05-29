# Intelligence Layer — Channel Contract

The data contract between Easel's live signal sources and shaders. Shaders bind to
named channels; Easel routes values; channel names stay stable as the implementations
behind them evolve (heuristic → diarization, mic → multi-mic, etc.).

## Channel namespace

All channels are flat string keys, lowercase, dot-separated. They live in `DataBus`
(`src/app/DataBus.h`) — strings via `set()/get()`, floats via `setNum()/getNum()`.

| Channel | Type | Source | Notes |
|---|---|---|---|
| `audio.level` | float 0..1 | `AudioAnalyzer_mac.mm` RMS | currently uniform `audioLevel` |
| `audio.bass` / `.mid` / `.high` | float 0..1 | analyzer 4-band | currently `audioBass/Mid/High` |
| `audio.fft[i]` | float | kiss_fft bin | exposed today as `audioFFT` sampler; channels surface bin indices for binding |
| `cue.latest` | string | `EthereaClient` / Cue WS | most recent utterance; auto-bound to `msg` text inputs in `Application::loadShader` and `loadProject` |
| `cue.transcript` | string | rolling concat | full session transcript |
| `cue.prompt`, `cue.coach.headline/quote/feedback/alternative/severity` | string | Cue Coach actions | see `Application.cpp:386-397` |
| `player[i].active` | bool / 0..1 | synthetic v1 (below) | 1-indexed; default N=3, up to N=6 |
| `player[i].energy` | float 0..1 | smoothed RMS while active | 0 when inactive |
| `player[i].pitch` | float 0..1 | normalized dominant FFT bin | 0 when inactive |
| `player[i].confidence` | float 0..1 | heuristic score | quality signal for hard cuts |
| `data.<userKey>` | float or string | user-defined feeds | OSC/MIDI/HTTP push into `DataBus`; namespace reserved for user-supplied data |
| `transport.time` | float seconds | already emitted as `TIME` uniform | surfaces as a channel for non-default bindings |
| `transport.bpm` | float | `BPMSync` | see `Application.cpp:1398` |
| `transport.beat` | float 0..1 | `BPMSync.beatPhase()` | phase within current beat |

Vision keys (`vision.pose.*`, `vision.hand.*`, `vision.face.*`) already live in
`DataBus::availableNumericKeys()` and stay in their own namespace.

## Player channels v1 — synthetic decomposition

A pragmatic stub for the multi-"player" vision while real diarization waits on Phase 3.
**Shader authors bind to `player[i].*` today; the implementation behind those channels
changes later without any shader edit.**

Heuristic (CPU-cheap, computed in the audio analyzer tick):

1. **VAD gate** — `audio.level > vadThreshold` for ≥ 80ms triggers "voice present".
2. **Dominant band** — find argmax over coarse pitch bands (e.g. 8 log-spaced bands
   over 80Hz–4kHz, sampled from the existing FFT ring).
3. **Player assignment** — each player slot owns a band (or a band-cluster). On voice
   present, the player whose band matches becomes `active=1`; others fade `active→0`
   with hysteresis (release ≈ 400ms, attack ≈ 60ms) to prevent flicker.
4. **Energy** — `player[i].energy` = smoothed RMS while `active`, else decays to 0.
5. **Pitch** — `player[i].pitch` = normalized argmax bin within that player's range
   while active, held briefly after release.
6. **Confidence** — `0..1` from band-separation margin + voice-likeness (spectral
   flatness inverse). Below 0.3, treat as silence.

Manual overrides (Studio tab → Player Routing): per-player `force_active`, band
re-assignment, N selector 1..6. Manual override pins `active=1` regardless of audio.

**Stub flag**: this is a heuristic, not real speaker diarization. Phase 3 swaps the
implementation (e.g. on-device embeddings or external diarizer). **The channel names
do not change**, so no shader is ever invalidated.

## ISF `BIND` attribute

Optional new field on an ISF INPUT, in the existing `/*{ … }*/` header JSON. Declares
the *intended* channel so Easel's binding popup pre-suggests it. Backward compatible:
missing `BIND` = current manual behavior.

```jsonc
{ "NAME": "energyA", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0,
  "BIND": "player[1].energy" }
```

Patterns allowed in `BIND`:
- Exact key: `"BIND": "audio.bass"`
- Indexed player: `"BIND": "player[1].energy"` (1-indexed; mirrors how authors think)
- Wildcard suggestion: `"BIND": "player[*].energy"` — popup pre-fills the slot index
  based on the input's positional order among other `player[*]` binds in the file.

Easel parses this in `ShaderSource::loadFromCode` (alongside existing INPUT parsing in
`ShaderSource.h:13-60`) and stores it on `ShaderInput`. The Property Panel's bolt /
right-click binding popup (`PropertyPanel.cpp:2644` `##audiobind`) reads it to seed
the dropdown's default.

## Generalized "Bind to…" popup

Today the popup is audio-only (`AudioBinding` in `ShaderSource.h:86`, smoothing +
range + live caret in `PropertyPanel.cpp:2706-2720`). It generalizes to channel
categories. **All existing per-binding controls stay** — smoothing, output range, live
caret driven by `applyAudioBindings`'s smoothed mapping (`ShaderSource.cpp:909`).

Popup sections (collapsible, keyboard-navigable):

- **Audio** — `audio.level/bass/mid/high`, FFT bins
- **Players** — `player[1..N].{active,energy,pitch,confidence}`
- **Cue** — `cue.latest`, `cue.transcript`, coach feeds (text channels go to text inputs only)
- **Data** — `data.<userKey>` (user feeds; populated from whatever's pushed to DataBus)
- **Transport** — `transport.{time,bpm,beat}`
- **Vision** — existing `vision.*`
- **Manual** — disconnect

If the input has `BIND`, that entry is highlighted at the top.

## Anti-patterns

- **Audio-mash**: binding `audio.level` to every parameter. Defeats decomposition;
  use `player[i].energy` to give each visual element its own channel.
- **Smoothing inside the shader**: the per-binding `smoothing` field plus output
  `[rangeMin, rangeMax]` mapping is the authoring surface. Shaders should treat the
  uniform as already-shaped.
- **Encoding strings as numbers in `data.*`**: text feeds belong on string keys (the
  `m_values` map); numeric feeds on `m_numbers`. Don't smuggle.
- **Hard-coding band indices**: bind to `player[i].pitch` (semantic) instead of
  `audio.fft[37]` (raw).
- **Bypassing channels for "performance"**: a per-frame DataBus lookup is a hashmap
  hit — fine. Don't carve sidechannels.

## Concrete authoring example — "soccer field" shader

A shader visualizing a live match. Three player zones (synthetic players), three
data feeds, one cue stream.

```jsonc
/*{
  "CREDIT": "easel demo",
  "INPUTS": [
    { "NAME": "possession",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,
      "DEFAULT": 0.5, "BIND": "data.possession" },
    { "NAME": "goalsHome",   "TYPE": "long",  "DEFAULT": 0, "BIND": "data.goalsHome" },
    { "NAME": "cornerKicks", "TYPE": "long",  "DEFAULT": 0, "BIND": "data.cornerKicks" },
    { "NAME": "zoneA",       "TYPE": "float", "MIN": 0.0, "MAX": 1.0,
      "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "zoneB",       "TYPE": "float", "MIN": 0.0, "MAX": 1.0,
      "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "zoneC",       "TYPE": "float", "MIN": 0.0, "MAX": 1.0,
      "DEFAULT": 0.0, "BIND": "player[3].energy" },
    { "NAME": "msg",         "TYPE": "text",  "DEFAULT": "",
      "BIND": "cue.latest" }
  ]
}*/
```

`data.possession`, `data.goalsHome`, `data.cornerKicks` are user-fed via OSC or HTTP
into `DataBus.setNum("data.possession", …)`. The shader is *not* literal — see
RUBRIC.md axis (d): the zones are abstract fields of pressure, not a pitch outline;
goals trigger a compositional shift, not a pop-up scoreboard.

## Open questions / stubs

- Phase 3: real diarization implementation (TBD provider, on-device preferred).
- `audio.fft[i]` exposure: today the FFT is a sampler texture (`audioFFT`); for `BIND`
  we likely need per-bin scalar uniforms generated on demand. Decide in Phase 2.
- User-supplied `data.*` feeds need a small "Data Sources" section in Settings to
  register OSC/HTTP endpoints → keys.
