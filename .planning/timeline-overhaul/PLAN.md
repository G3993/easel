# Timeline Overhaul — Live Show Curation & Orchestration

**Goal**: a timeline that's fire on stage. The instrument a VJ uses to curate and orchestrate live shows — every shader parameter alive, every transition shader-backed and audio-reactive, every clip and source orchestrated together.

**Decisions locked in (2026-04-30)**:
- AE-style stopwatch keyframe model — touch-friendly, always-visible diamond affordance per parameter (no right-click required).
- Single-track-only transitions (no cross-track stinger).
- Live previews in transition catalog grid (renders against the actual current adjacent clips, not baked stills).
- One driver per channel: every animatable value is a 0..1 channel with one driver — `Timeline curve` / `LFO` / `Audio band` / `FFT bin` / `MIDI` / `OSC` / `Static`. Curve and binding mix additively.
- One bottom dock for everything time-based (curve editor lives there). No separate Graph Editor window. No node graph. No modulation matrix.
- Transitions live on clip-pair overlaps within a track. Drag-from-grid onto seam, drag edges to retime. Under the hood it's `mix(A, B, t)` where `t` is itself a channel.
- One excellent grid component reused 4× — transition catalog, LFO presets, easing presets, animation presets.

---

## What we already have

Confirmed by codebase audit:

**Data model exists** (`src/timeline/Timeline.h`):
- `TimelineClip`, `TimelineTrack`, `TimelineTransition`, `TimelineLane` (automation), `TimelineMarker`, `TimelineSection`. Lanes serialize to JSON but **don't evaluate at runtime**.

**Three transition paths already wired**:
- Per-clip gl-transitions catalog (built-in, picked by name)
- Per-clip ISF custom shader (path-driven, ShaderClaw integration)
- Cross-layer between-row transition (the popup picker)

**Audio uniforms feed transition shaders today** — `audioRMS`, `audioBass`, `audioMid`, `audioHigh`, `audioBeat` are already piped through `CompositeEngine` to transition shaders. The audio reactivity foundation is built.

**18 of 95** upstream gl-transitions bundled in `assets/transitions/gl/`. `GLTransition.cpp` sanitizes shader source (strips `#version`, bakes inline default uniforms) and compiles lazily on first use.

**Known fragility (do not let these regress)**:
- `applyToLayers()` → `compositeAndWarp()` ordering at `Application.cpp:462–468` is load-bearing. Reorder = 1-frame transition lag.
- Each `VideoSource` spawns its own WASAPI thread. Overlapping audio clips compound at the speaker. Centralized mixer is Phase E.
- `seek()` clears `m_runtime` (`Timeline.cpp:11`), restarting in-flight transitions from `progress=0`. **Fixed in Phase A.**
- Layer deletion must call `Timeline::removeTrackForLayer()` or orphan tracks accumulate.

---

## Architecture

### One model
```
Timeline
├─ tracks[] (bottom→top compositing order)
│   ├─ clips[] (sorted, non-overlapping per track)
│   ├─ transitions[] (each binds to two adjacent clips on this track)
│   └─ trackCurves[] (track-level opacity, blend mix)
├─ markers[] (cue points, optional scene-recall)
├─ sections[] (ruler bands)
└─ workArea, fps, duration

Clip
├─ source (Video / NDI / Shader / Text / Scene / Solid)
├─ start, duration, sourceIn (for video)
├─ staticParams[]   ← un-animated
└─ curves[]         ← animated parameters live here

Transition
├─ clipA, clipB (must be adjacent on same track)
├─ start, duration (the overlap window)
├─ shader (gl-transitions catalog id OR custom ISF path)
├─ staticParams[], curves[]   ← yes, transition params can also animate

Curve
├─ keyframes[] (sorted by time)
├─ binding (Manual / Bass / Mid / High / FFTbin / LFO / MIDI / OSC)
└─ smoothing, scale, offset, lfoRateHz, fftBin

Final value = curveEval(t) + scale·(audioSignal − 0.5) + offset, clamped
```

### Render pipeline (one frame at time T)
1. **Sample** all active curves once. Resolve `value(T)` per `ParamId` into a flat map. Mix in binding (audio/LFO).
2. **Resolve** track state — for each track, find active clip(s) and any transition.
3. **Per-track render** to a track FBO. If 2 clips overlapping with a transition, render A→ping, B→pong, then run transition shader to track FBO.
4. **Composite** track FBOs bottom→top with per-track blend/opacity.
5. **Output** master FBO → display / NDI / Syphon / record.

Perf design: shared FBO pool (4–6 RGBA16F at viewport size), shader cache keyed on (transition id, defines), preview thumbnails round-robined at 6 tiles/frame, video decode pre-rolls 250ms before next clip start.

### Touch-friendly keyframe affordance

A 12px diamond on the left of every animatable parameter row, always visible (16px hit target). Three states:

| State | Visual | Tap action |
|---|---|---|
| Hollow, dim (white α≈0.15) | small ring | Create first keyframe at playhead, open curve dock |
| Filled, accent | solid diamond | Remove the keyframe at playhead |
| Hollow, accent-colored | accent ring | Add keyframe at playhead |

When animated, the row expands ~10px to show an inline keyframe strip beneath the slider — keyframes as 6px diamonds at their time positions on the same horizontal scale as the timeline. Slider stays the slider; the strip is the timeline view of that parameter. Both visible at once.

Long-press the diamond = clear all animation from this parameter (destructive action behind a deliberate gesture).

### Touch interactions in the curve dock
- Pinch = zoom time axis
- Tap-and-hold on a curve = add keyframe at that time/value
- 1-finger drag = move keyframe (shift constrains axis)
- 2-finger drag = pan curve view
- Long-press a keyframe = open Hold/Linear/Ease/Bezier interpolation popover at touch point

---

## Phasing

### Phase A — quick wins (half day)
Fix what's broken now. No data model changes.

1. **Transition picker scrolling.** Replace `BeginCombo` / flat `Selectable` loops at `Application.cpp:5527` (per-clip) and `:5598` (cross-layer popup) with a bounded child window: search field at top, max-height clamp, grouping (Bundled / User / ShaderClaw).
2. **Seek-mid-transition smoothing.** `Timeline::seek()` currently clears `m_runtime`, restarting any in-flight transition from `progress=0`. Compute `progress` directly from `(t − transition.start) / transition.duration` and let the transition continue smoothly when the playhead lands inside.
3. **Import 15 upstream gl-transitions** — `morph`, `displacement`, `polar_function`, `pinwheel`, `hexagonalize`, `pixelize`, `crosshatch`, `luminance_melt`, `perlin`, `ButterflyWaveScrawler`, `StereoViewer`, `Fold`, `GridFlip`, `FilmBurn`, `EdgeTransition`. Catalog: 18 → 33 with no original work.

**Done = catalog browseable, scrubbing through transitions doesn't jitter, 33 transitions available.**

### Phase B — live-preview transition catalog grid (1–2 days)
Replace the picker entirely with a 3-column scrollable grid of *live previews rendering on the user's actual adjacent clips at the seam being edited*.

- Snapshot the from-clip and to-clip frames when picker opens (one offscreen render per clip, stored in two RGBA8 textures).
- Each tile runs the actual transition shader on those snapshots, looping `progress` 0→1 over ~2.5s with a 0.5s hold at each end.
- Round-robin: only ~6 tiles render per frame, distributed across the grid (kept in viewport).
- Top: search field (live filter on type-ahead) + category chips (Wipe / Fade / Distort / Glitch / Geometric / Warp / 3D / Fluid).
- Click tile = apply to selected transition slot. Drag-and-drop tile onto a clip-seam = create transition with default duration.
- Favorites star + recently-used chip.
- "Refresh previews" button to re-snapshot if user wants to update.

Reuse this grid component (`PreviewGrid`) — it'll come back for LFO presets, easing presets, animation presets in Phase D.

**Done = picker is a live grid of 33+ transitions previewing on actual content.**

### Phase C — keyframe runtime + curve editor (3–5 days)
Make `TimelineLane` actually drive shader uniforms. Touch-friendly diamond affordance per parameter. AE-style first-keyframe gesture.

1. **Lane runtime evaluation.** At top of frame, walk all active `Curve`s and resolve `value(T)` into a `unordered_map<ParamId, float>`. `Layer::update()` and `ShaderSource::setParam()` consult this map.
2. **Diamond affordance.** Add to `PropertyPanel.cpp` for every animatable parameter: 12px diamond at row's left edge, three-state (hollow-dim / filled-accent / hollow-accent), 16px tap target. Tap = single-action keyframe toggle. Long-press = clear all animation.
3. **Bottom curve dock** — collapsible, persistent. Each animated parameter is a row with left-gutter source-icon, 1-px stroke curve, 6px keyframe diamonds. Selected curve uses accent color; others use neutral grays.
4. **Direct-manipulation interactions**:
   - Tap-and-hold on curve = add keyframe at touch point
   - Drag keyframe = move (shift constrains)
   - Box-select keys; arrow keys nudge by 1 frame, shift-arrow by 10
   - Long-press a keyframe = interpolation popover (`Hold | Linear | Ease | Bezier`)
5. **Sample-once-per-frame** to avoid per-uniform re-evaluation. Cap warning at ~10k keyframes total.
6. **Inline keyframe strip beneath slider** when parameter is animated. Same horizontal scale as timeline. 6px diamonds.

**Done = right-click eliminated; any slider becomes animatable with one tap; curves edit and play back smoothly.**

### Phase D — bindings + original transitions (3–5 days)
Where Easel stops being a gl-transitions player and becomes a 2026 instrument.

1. **Per-parameter binding popover.** Source icon in curve dock left gutter cycles a popover: `Manual / Bass / Mid / High / FFT bin / LFO / MIDI / OSC`. Below: Scale, Offset, Smoothing, **live meter showing resolved value right now** (the killer feature for debugging "why isn't this pulsing?").
2. **Bound parameter rendering**: curve drawn dimmed to 30%, live audio modulation drawn on top in accent. Visual answer to "what's actually being applied right now."
3. **Build 8 original shader transitions** (mathematically grounded, parametric, audio-bound by default):
   - **Curl-Noise Smear** — ping-pong feedback advected by curl(noise), bass→smear strength, transient→swirl impulse
   - **SDF Morph** — luma SDF interpolation, edges flow between shapes; mid→band width, beat→snap polarity
   - **Halftone Density Dissolve** — rotating dot-screen threshold; high→pitch, beat→angle jump
   - **Spectral Curtain** — column reveal driven by FFT bin amplitude; literal EQ visualization
   - **Voronoi Shatter-Rebuild** — N seed points, cells shatter A then snap to B; RMS→flight height, beat→batch trigger
   - **Latent Bloom** — coarse low-freq noise upscaled, phantom blobs of B grow inside A; sub-bass→latent res
   - **Retime-Loop** — A at 2×, B reversed, crossfade with scrub-blur; tempo-locked speeds
   - **Ray-Marched Volumetric** — emissive volumes in a thin slab, two flat videos meet through fog; bass→extinction
4. **Reuse `PreviewGrid`** for LFO presets, easing presets, parameter animation presets.

**Done = Easel parameters are alive on stage; 8 original transitions ship.**

### Phase E (optional, later) — centralized audio mixer
Replace per-VideoSource WASAPI threads with a centralized mixer. Only worth doing once shows feature overlapping audio clips that need mixing.

---

## What this rejects

- TouchDesigner-style node graphs (separate "logic" surface in v2 if at all).
- AE-style separate Effect Controls panels divorced from the timeline.
- Audio reactivity as a special mode (it's just a binding option).
- Stackable transitions on the same overlap (use intermediate track).
- Cross-track stinger transitions in v1.
- Per-keyframe interpolation tangent handles by default (Bezier mode reveals them).

---

## Source documents
- `.planning/timeline-overhaul/RESEARCH-codebase-audit.md` — file/line map of current implementation
- `.planning/timeline-overhaul/RESEARCH-ux-patterns.md` — Resolume / MadMapper / TD / Notch / AE / Cables.gl pattern crib
- `.planning/timeline-overhaul/RESEARCH-shader-catalog.md` — gl-transitions taxonomy + 8 original concepts
- `.planning/timeline-overhaul/RESEARCH-architecture.md` — data model + render pipeline + UX details

(Research docs follow this PLAN.md; agents already produced them in the synthesis above.)
