# Easel Timeline UI/UX Audit

## 1. Transport Bar (lines 3416–3532)

**What it does:**  
Play/pause/stop icon buttons, loop toggle, MM:SS playhead display, duration field editor, "+ Tracks" button to reconcile timeline tracks with layer stack.

**What's good:**
- Icons are hand-drawn (triangles, bars, lemniscate), pixel-perfect, always visible
- Baseline alignment with DragFloat for cohesive layout
- Keyboard shortcuts (Space, Home, End) work when timeline is focused
- Duration field sensible: enforces 1.0s minimum, units labeled "Dur"
- "+ Tracks" tooltip explains intent: "Add a timeline track for every layer in your stack"

**What's confusing/broken/redundant:**
- **Icon ambiguity:** Stop button is a filled square—many users expect it to also rewind to 0 (it does via code, but the icon doesn't telegraph "reset"). Play/pause toggle works, but lacks visual feedback that playhead is rewound
- **"+ Tracks" placement:** Right-aligned in the transport row, but *not* a creation action—it's a reconciliation action. A user might click it expecting to add *new* layers; instead, it only scaffolds tracks for *existing* layers. Label doesn't clarify "from existing layers"
- **Time display:** MM:SS loses subsecond precision when zoomed in (3685 shows "%.0fs" for very short clips). This is clip-label logic, not transport, but shows the mental model gap: timeline supports sub-second timing, but UI hides it
- **No current-frame source indicator:** Transport shows time but not "what's playing now?" — which layer, which source file. Compare to Premiere: always shows the active clip name/source path

**Keep / Cut / Rethink:**
- **Keep:** Play/pause/stop trio, icon style, keyboard shortcuts, duration field
- **Cut:** Stop button (merge into Play: click-to-stop-and-stay, or require shift+click to rewind)
- **Rethink:** "+ Tracks" label → "+ Tracks from Layers" or "+ Missing Tracks". Move to a separate "Sync" or "Layout" section, or tie it to layer-panel changes automatically

---

## 2. Ruler + Scrubbing (lines 3564–3634)

**What it does:**  
Adaptive tick intervals (10s → 5s → 2.5s → down to 0.1s based on zoom), click/drag to seek, shift/alt+wheel to pan, plain wheel to zoom centered on cursor, double-click to reset zoom.

**What's good:**
- Adaptive tick intervals are elegant: visibleDur / majorInterval bounded between 6–18 ticks (smart auto-scaling)
- Zoom gestures sensible: plain wheel zooms, shift+wheel pans (modifier telegraphs mode)
- Double-click reset is discoverable (click ruler twice to reset)
- Tick labels use MM:SS, fallback to MM:SS.CS for sub-second intervals
- Playhead seeking works by clicking or dragging ruler

**What's confusing/broken/redundant:**
- **Zoom gesture not documented:** No tooltip, no UI hint that wheel zooms. Most NLEs put a zoom slider or label
- **Pan modifier conflict:** Shift+wheel and Alt+wheel both pan—why duplicate? Shift is standard (scrollbars), Alt is less discoverable
- **Tick label precision loss:** At high zoom, "MM:SS.CS" truncates to 2 decimal places (3583: "%.2f"). Fine for frame-based NLEs, but confusing for sub-frame sources
- **Ruler click vs. drag:** Both seek instantly (3594–3598). Clicking feels like scrub-lock (playhead jumps), dragging feels like scrub-drag. No visual diff in intent
- **No playhead snapping UI:** Code snaps playhead within 6px of clip boundaries during drag (3801), but no magnet icon or hint

**Keep / Cut / Rethink:**
- **Keep:** Adaptive ticks, wheel zoom, pan modifier
- **Cut:** Duplicate pan modifier (pick Shift XOR Alt, not both)
- **Rethink:** Add a zoom slider or percentage readout (e.g., "Zoom: 4x"). Add tooltip on ruler hover: "Scroll to zoom, Shift+scroll to pan, double-click to reset"

---

## 3. Track Rows (lines 3649–3782)

**What it does:**  
Pill-shaped track containers with muted/solo buttons, clip rectangles with drag-to-move, left/right-edge drag to trim, right-click empty space to add clip, playhead line.

**What's good:**
- Visual pill shape with rounded corners feels cohesive (AE-style)
- Vertical grid lines (synced to ruler majors) provide rhythm guides
- Trim handles (left/right edges) appear on hover—discoverability solid
- Clip label auto-adjusts: "5s" for tiny clips, "0.5s" for small, "Clip Name · 0.5s" for normal
- Mute/solo M/S buttons in header, clear tooltips
- Clip-click-to-drag initiates mode: center=move, left-edge=left-trim, right-edge=right-trim
- Playhead snap within 6px during drag (feels responsive)

**What's confusing/broken/redundant:**
- **No visual clip-type indicator:** All clips rendered identically (white pill, 22% alpha). If clip.sourcePath points to a video vs. a shader, no visual cue. User must hover to see label to guess what will play
- **Track header cluttered:** Layer name + M/S buttons squeezed into 120px pill. If layer name is long, it truncates (3745–3750). No affordance for renaming from timeline
- **Solo semantics unclear:** "Solo this track — only soloed tracks play" (3768 tooltip) doesn't say "muted+soloed = muted wins" or clarify interaction with global mute. Current code: `gated = muted || (anySolo && !solo)`, so mute always silences (correct), but UI doesn't explain
- **Clip drag snap only during drag:** Playhead snap (3801) happens mid-drag. If user releases off-snap, clip lands sub-frame and *looks* mis-aligned visually but the snap was never shown. Better: highlight snap zones on hover, or snap on-release if within threshold
- **Right-click add-clip:** Creates 5s default clip at click time (3777). No way to set default duration. Name defaults to `track.name` (layer name), not unique per clip
- **No in/out-point UI:** Timeline clips are [startTime, duration] only. No way to say "play 0:05–0:15 of this source file into the timeline at 10:00–10:20" (source trimming). Hard-coded as load-full-source-or-sourcePath-empty
- **Overlapping clips behavior opaque:** Code supports overlapping clips (applies *first* active clip found, 3204–3205). If two clips overlap, which plays? No visual feedback—clips just stack in Z-order

**Keep / Cut / Rethink:**
- **Keep:** Pill shape, M/S buttons, trim handles, grid lines, playhead snap logic
- **Cut:** Redundant track header if layer panel exists; consider moving M/S there
- **Rethink:** Add clip-type color coding (video = blue pill, shader = purple pill, etc.). Add per-clip in/out sliders or timeline scrubber for source trimming. Add visual priority indicator if overlaps occur. Auto-name clips incrementally ("Clip 1", "Clip 2", …) instead of layer name

---

## 4. Clip Model (Timeline.h TimelineClip, lines 13–24)

**What it does:**  
Stores id, startTime, duration, name, sourcePath. Minimal representation of "play this source during this time interval."

**What's missing / insufficient:**
- **No source in/out points:** sourcePath is whole-file only. Can't trim source (e.g., "use 0:10–0:20 of input.mp4"). Users must pre-edit sources externally
- **No per-clip opacity/blend:** Layer has blend mode + opacity; clips don't. Can't keyframe or set per-clip blend (all-or-nothing via layer)
- **No labels/color:** No way to color-code clips (red="action", blue="dialogue"). Naming alone is weak UX
- **No playback rate:** Can't speed up or slow down individual clips—all timing is absolute seconds
- **No transition override:** Layer has transitionType + transitionShaderPath, but clips can't override. If you want two clips with different transitions on the same layer, tough luck
- **No metadata:** No way to tag clips (e.g., "B-roll", "VFX needed") or link to external notes

**Keep / Cut / Rethink:**
- **Keep:** id, startTime, duration (core)
- **Cut:** name could be auto-generated if color/label replaces it
- **Rethink:** Add `sourceInPoint, sourceOutPoint` (doubles) for per-clip source trimming. Add `color` (enum or hex) for visual tagging. Add `speed` (float) for playback rate. Consider per-clip `blendMode` and `opacity` if layer-wide is insufficient

---

## 5. Timeline-Layer Coupling (applyToLayers, Timeline.cpp lines 194–274)

**What it does:**  
Once per frame, for each track, find the active clip at playhead time. If clip changed (edge), hide/show layer, load source from sourcePath, or start shader transition. If muted/solo'd, skip.

**What's good:**
- Edge detection is elegant: `prevActiveId != curActiveId` fires only on clip-change, not every frame (efficient)
- Shader transition logic: if transitionType==Shader and sourcePath differs and previous clip existed, queue shader blend (smart)
- Fallback: if Shader transition is set but no shader path, warn once per layer and instant-swap (safe)
- Mute/solo gating is correct: muted always silences, solo gates unless this track is solo'd (clear precedence)

**What's magical / non-obvious:**
- **Layer visibility magic:** User doesn't explicitly tell layer to hide; timeline hides it when clip ends (lines 267–268). If user manually unhides mid-clip-exit, next frame might hide it again. No clear ownership: timeline or user?
- **Source swap on clip-enter:** sourcePath in clip triggers `loadSourceForPath()` (line 234), but this is a hidden call with no UI affordance. User adds clip, sets sourcePath somewhere else (where?), and it "just works" on seek/play. How does user set sourcePath? Not obvious in timeline UI
- **Transition state machine:** transitionActive and shaderTransitionActive are separate; the code flips them on layer based on clip edges. If user is mid-transition, does the timeline interrupt? Code doesn't check (probably should)

**Keep / Cut / Rethink:**
- **Keep:** Edge detection, shader transition logic, mute/solo precedence
- **Rethink:** Make source assignment explicit in UI: right-click clip → "Set Source" dialog, or drag-drop file into clip. Clarify layer ownership: if timeline is controlling visibility, disable manual visibility toggle during clip playback (or show "Timeline Override" badge). Document state machine in code comment: "Transitions are interrupted by clip changes."

---

## 6. "+ Tracks" Behavior (Application.cpp lines 3509–3531)

**What it does:**  
On click, iterate all layers in LayerStack. For each layer without a timeline track, create an empty track (no clips). If a track already exists, skip it.

**Is this a footgun?**  
**Yes:**
- User adds 10 layers to the layer panel. Clicks "+ Tracks". Gets 10 empty timeline tracks. Expected? Maybe. User then expects a "sync" button to keep them in sync if layers are added/removed later. No such button
- Inverse problem: user deletes a layer. Timeline track orphans (layerId points to a layer that no longer exists). Code in applyToLayers (line 210–213) silently skips orphaned tracks. UI doesn't warn or clean up
- **Better approach:** Track auto-creation on layer-add (hook in LayerStack::addLayer). Track auto-removal on layer-remove (hook in LayerStack::removeLayer). This is opinionated but safer—no manual sync button needed

**Keep / Cut / Rethink:**
- **Cut:** "+ Tracks" button
- **Rethink:** Auto-sync tracks with layers. When layer is added, create track. When layer is removed, remove track (warn if track has clips). Or: make this opt-in per project ("Auto-sync timeline tracks" checkbox in settings)

---

## 7. Overall Mental Model

**If a brand-new user opens Easel and sees the timeline, what do they think?**

Current UI suggests:
- "This is Adobe Premiere–style: horizontal time axis, vertical layer stacks, clips on tracks, drag to edit"
- Play/stop/loop controls at top (media player UX)
- Ruler with timecode (standard NLE)

**Gap between mental model and actual behavior:**
- **Missing mental affordance #1:** No visual hint that clips are *linked to sources*. User might expect "clip = recorded segment" (like Premiere), not "clip = 'when does this layer play' with optional source file override"
- **Missing affordance #2:** Layers and tracks are orthogonal. Layer has properties (opacity, blend, effects). Track has clips. User edits layer in layer panel, timeline controls *when* it plays. This is clearer in Nuke (nodes + timeline), but less clear in Premiere-inspired UI
- **Missing affordance #3:** No indication that overlapping clips are *possible* or what happens. Does the later clip override? Do they blend? Do both play?
- **Mental model mismatch:** Timeline seems to *own* layer visibility (clips turn layers on/off), but layer panel also allows manual visibility toggle. Which wins? Code says: if clip is active, show; if not, hide. Manual toggle is overridden next frame. Confusing

**Better positioning:**
- Clarify: "Timeline schedules *when* layers are active. Clips are [start time, duration, optional source override, optional transitions]."
- Make layer-timeline relationship explicit: maybe rename "Timeline" → "Schedule" or add subtitle "Layer Timeline" to emphasize it's not video editing
- Or: add visual link between layer panel and timeline (highlight layer when hovering clip; highlight clip when selecting layer)

---

## 8. Redundancy / Things to Cut

**Duplicate functionality:**
- **M/S in track header + in layer panel (if it exists):** Check if LayerStack UI has mute/solo. If so, one is redundant
- **Clip name in timeline + in layer name:** Clips default to layer name; user must rename clip separately to differentiate. Consider: no clip name field, use color/label instead
- **Duration display in transport ("Dur" field) + in clip labels:** Transport shows total timeline duration. Clip shows individual clip duration. Useful, but visually redundant if both are always visible

**Unused code:**
- **TrackRuntime struct (Timeline.h lines 90–94):** Used only to track `activeClipId` for edge detection. Could be a simpler map; struct buys nothing
- **clip.contains(t):** Useful, but called only once per track per frame (line 205). Could inline

**Things to consider cutting:**
- **Loop button:** If looping is critical, make it a checkbox in a "Playback Options" dialog, not an icon button (saves UI real estate)
- **Stop button:** Play/pause toggle is more standard. Stop (rewind + pause) is rare in modern UIs
- **Track name in header:** If track is 1:1 with layer, and layer name is in the layer panel, the track header name is redundant. Just show M/S

---

## Summary: Keep / Cut / Rethink

### KEEP
- Icon-based transport (play/pause/stop/loop) — familiar to all users
- Adaptive ruler ticks — elegant and auto-scaling
- Clip drag-to-edit (move, left-trim, right-trim) — core UX
- Mute/solo buttons — standard mix control
- Playhead snap during drag — responsive feel

### CUT
- "+ Tracks" button (replace with auto-sync)
- Stop button (merge into play toggle)
- Duplicate pan modifier (Shift XOR Alt, not both)
- Redundant track header if layer panel has M/S

### RETHINK
- **Clip visual identity:** Add color coding (video=blue, shader=purple, color=green) so users see clip *type* at a glance
- **Source assignment:** Explicit "Set Source" UI, not hidden sourcePath field
- **Clip model:** Add sourceInPoint/sourceOutPoint for per-clip trimming, `speed` for playback rate, `color` for tagging, optional per-clip blend/opacity
- **Layer-timeline ownership:** Make timeline the *sole* controller of layer.visible during playback; disable manual visibility toggle during clip playback (or show "Timeline Override" badge)
- **Overlapping clips:** Add visual priority indicator (z-order, alpha gradient) if clips overlap
- **Zoom affordance:** Add zoom slider or percentage readout (e.g., "Zoom: 2x")
- **Auto-sync tracks:** Remove manual "+ Tracks" button; auto-create/remove tracks when layers are added/removed

---

## Estimated Effort

| Task | Effort | Priority |
|------|--------|----------|
| Auto-sync tracks | Medium (hook LayerStack) | High |
| Clip color coding | Low (enum + UI) | Medium |
| Source assignment UI | High (dialog/drag-drop) | High |
| Per-clip trim (in/out) | High (UI + model) | Medium |
| Zoom slider | Low (ImGui widget) | Low |
| Layer ownership clarification | Low (code comment + badge) | Low |

