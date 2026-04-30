# Timeline UI Research for Easel

Research date: 2026-04-21
Purpose: Inform the redesign of Easel's timeline (C++/ImGui, projection-mapping / live-visuals tool) for stacking shaders and videos over a fixed duration, scrubbing, and mp4 export.

Coverage: Resolume Arena, Synesthesia Pro, MadMapper, TouchDesigner, After Effects, Premiere Pro, Unreal Engine Sequencer.

---

## 1. Resolume Arena

**Context:** VJ / live-performance clip launcher. The primary UI is a *grid* (clips in columns, layers in rows), triggered live — not a linear timeline. It has two different "timelines" depending on context.

1. **Layout.** No dedicated composition timeline panel at the bottom. Clip launcher grid dominates (layers = horizontal rows, columns = vertical). When a clip is selected, the *Clip Time panel* at the bottom of the inspector shows a per-clip mini-timeline (playhead + trim handles for that single clip). A global, composition-wide timeline is a long-requested missing feature per their forums.
2. **Layer-to-track mapping.** Layers are composition-wide; each layer holds many clips side-by-side in columns. Only one clip per layer can play at a time (triggered). So layer = row, but clips on a row are mutually exclusive triggerable slots, not a temporal sequence.
3. **Clip model.** A clip wraps one source (video, image, shader, audio). It has In/Out points (sub-range trim of the source), Duration, Speed (-∞ to +∞), Direction, Transport mode (hold/loop/random/bounce/once). Clip contains its OWN micro-timeline; there is no composition timeline placing clips at global times.
4. **Interaction.** Drag media from the file browser into a grid cell; or drag an existing clip to reorder. To "split" in the performance sense you just place the same clip in two cells with different in/outs. Trim = drag blue handles on the clip's in/out bar in the Clip Time panel. Move in time has no real meaning because clips aren't on a timeline.
5. **Playhead & scrubbing.** Per-clip: grab the blue playhead pointer and drag ("DJ-scratch"). No global timeline playhead in the editor. BPM sync dominates instead.
6. **Audio track.** Audio is either embedded in a video clip (plays alongside) or lives as an audio-only clip in the grid. No separate audio lane.
7. **Loop/duration.** Infinite / performance-driven. A composition has no fixed duration. You can mark beat grids via BPM and use the Dashboard for timing automations, but the show ends when you stop.
8. **Export/render.** Two paths: **Record** captures the live output to disk in real time (the performance path). **Clip Renderer** (right-click clip → Render to File) renders a single clip offline for frame-perfect output. No "export this composition range" because there is no range. This is a known gap for cue-based show users.
9. **The ONE THING.** The *grid metaphor*: layers stack vertically exactly like paint ("Layer 1 is behind Layer 2..."), and every column is a triggerable snapshot. It makes live performance instantly readable — but fails for linear shows.

---

## 2. Synesthesia Pro

**Context:** Music-reactive visualizer for VJs. Preset / scene-switcher first; timeline is secondary.

1. **Layout.** Dual-panel interface: scene library on the left, "Demo Scenes" playlist on the right. The timeline is a thin ruler with a movable playhead that shows media playback progress — primarily for the currently-playing scene/video, not for orchestrating a linear show.
2. **Layer-to-track mapping.** No tracks/layers in the NLE sense. The "stack" concept is replaced by a **playlist**: an ordered list of scene+preset pairs, each with its own countdown/duration. Scenes are whole visual states, not stackable rows.
3. **Clip model.** A playlist entry = one scene + one preset + a countdown time. Individual video media has its own small timeline with playhead for scrubbing. No global start/end placement on a master timeline.
4. **Interaction.** Reorder playlist entries with arrows or drag. Set per-entry time. Next/Previous/Play buttons (MIDI-mappable). Rename/duplicate playlists.
5. **Playhead & scrubbing.** Scrubbable playhead on the currently-playing video. Mouse-drag to jump. Manual through-clip navigation.
6. **Audio track.** Audio is the *input driver*, not a timeline object — FFT analysis feeds visuals; audio output from clips can play but isn't stack-edited.
7. **Loop/duration.** Per-entry countdowns add up to a show duration, but it's still a list, not a ruler. Playlists can loop.
8. **Export/render.** Presets/scenes can be bulk-exported as assets. Live output is recorded. No offline "render this playlist to MP4" flow surfaced prominently.
9. **The ONE THING.** The *playlist* is just an ordered list with times — radically simpler than a multi-track timeline. For a VJ who just wants "play these 8 visuals, 30 seconds each, reactive to audio", it is immediately legible.

---

## 3. MadMapper (v6)

**Context:** Projection mapping tool that in v6 gained a full NLE-style timeline. Most relevant reference for Easel — closest analog.

1. **Layout.** Dedicated Timeline panel (new in v6). Full-width, bottom. Sits alongside the existing Cues/Scenes UI. Export button top-right of the timeline itself.
2. **Layer-to-track mapping.** Multiple track *types*: Montage Tracks (media — movies, materials, generators, essentially the visual "clip" lanes), Parameter/Value tracks (animate any parameter over time), Audio tracks (multichannel, FFT), Laser tracks, DMX. You can make as many montage tracks as you want; value clips can be dragged between tracks. Crucially: Tracks are NOT bound 1:1 to the surfaces/mappings panel — they orchestrate media onto surfaces.
3. **Clip model.** A Clip is a "short, repeatable, loopable segment" — ghostable (referenceable) and consolidatable into unique instances. On montage tracks, a clip wraps a media source and has in/out, trim, time-reverse. On value tracks, a clip is an animation segment with keyframes + Bezier/Flat interpolation. Markers inside clips can trigger follow actions (wait for button press, goto, etc.).
4. **Interaction.** Drag media from the library onto a montage track. Trim by edge-dragging. Cut/chop and time-reverse via track tools. Value clips drag between tracks. Markers are point-in-time events with logic.
5. **Playhead & scrubbing.** Standard NLE playhead. Click ruler / drag playhead. Markers provide jump points.
6. **Audio track.** First-class, unlimited multichannel, on the timeline below/among other tracks. FFT drives visuals. Baked into export.
7. **Loop/duration.** Timeline has a defined length; you can have multiple timelines and switch between them (replacing the old Cues). Loop sections are selectable; export can target either the whole timeline or just the loop section.
8. **Export/render.** One Export button on the timeline bakes everything: per-video-output movies, audio file, ILDA for lasers, DMX MadLight sequence. Choose full project or loop section; pick codec/audio.
9. **The ONE THING.** *Timelines replace cues* — one metaphor instead of two. Plus the Export-button-on-the-timeline: wherever the timeline is authored is where render happens. No separate render dialog to hunt for.

---

## 4. TouchDesigner

**Context:** Node-based real-time system. Timeline is more of a clock than an NLE.

1. **Layout.** Global timeline is a thin strip at the bottom of the main window — a ruler + transport. It is a *clock*, not an editing surface. Keyframe editing lives inside Animation COMPs (open a dedicated Animation Editor window). Most TD users are famously told to "get off the timeline" and drive time from networks.
2. **Layer-to-track mapping.** There are no timeline tracks in the NLE sense. Compositing happens in the node network (TOPs layered via Composite TOP, Over TOP, etc.). Each Animation COMP is a keyframe container; channels inside it are the closest to "tracks."
3. **Clip model.** No "clip" on the global timeline. Media plays via Movie File In TOPs (with speed/trim parameters). Animation is keyframes inside Animation COMPs evaluated against a Time Reference.
4. **Interaction.** Add media = drop a Movie File In TOP and wire it. Animate = add Animation COMP, set channels, keyframe via Animation Editor. Components can have their own Time COMP = their own clock.
5. **Playhead & scrubbing.** Global playhead on the bottom strip; drag it. Each component can have its own playhead via Time COMP. Working Range lets you focus on a subset of frames for playback.
6. **Audio track.** Audio is just CHOPs — not a timeline lane. Played/analyzed via Audio File In / Audio Play CHOPs.
7. **Loop/duration.** Project has Start/End and a Working Range (subset for iteration). Looping is a timeline setting. Per-component clocks make "duration" fuzzy — time is a graph, not a single bar.
8. **Export/render.** Export Movie Dialog: set Start Frame, End Frame, Duration (any one auto-updates the others), pick codec (H.264, ProRes, HAP, NotchLC), render offline — NOT real time. Alternatively Movie File Out TOP for live recording.
9. **The ONE THING.** Time is *data, not UI*. That's powerful for experts but actively hostile to a "place this clip at 0:30" mental model. TD is the *anti-example* for Easel's use case, but its Export Movie Dialog (Start/End/Duration auto-reconciled) is a clean pattern.

---

## 5. After Effects

**Context:** Motion-graphics compositor. Canonical reference for layer-stacks + keyframes.

1. **Layout.** Timeline panel is full-width at the bottom (default workspace). Composition viewer top, Project/Effects panels left/right, Timeline below. Each Composition opens its own Timeline panel as a tab.
2. **Layer-to-track mapping.** One layer = one row in the Timeline. Rows are strictly one-layer-per-row (explicitly different from NLEs). Stacking order in the Timeline = back-to-front render order in the Composition viewer.
3. **Clip model.** "Layer" IS the clip. A layer has a source (footage, shader-like effect, solid, text, light, camera), In Point, Out Point, duration bar in the timeline, and properties with keyframes. Trim by dragging bar edges; slip by opt-drag; timestretch via stretch property.
4. **Interaction.** Add = drag from Project panel into Timeline (drop indicator shows both stacking position and time-in-point). Or drag to the New Comp icon with "Sequence Layers" on to auto-chain. Move = drag the bar horizontally. Trim = drag edges. Split at playhead = Ctrl/Cmd+Shift+D. Duplicate = Ctrl/Cmd+D.
5. **Playhead & scrubbing.** Click ruler to jump. Drag CTI (Current Time Indicator). Frame-step with Page Up/Down. Keyboard: Spacebar preview, J/K/L not standard (those are NLE).
6. **Audio track.** Audio layers are also just rows in the same timeline. Waveform reveals with LL. No separate "audio lane below video" — audio is a layer like any other (reinforcing layer=row-of-anything).
7. **Loop/duration.** Composition has a fixed Duration set at creation (editable in Comp Settings). Work Area (B = set in, N = set out) defines the preview/render sub-range. The comp ends at its duration; doesn't auto-extend.
8. **Export/render.** Composition → Add to Render Queue (or Add to Adobe Media Encoder). Render Queue uses the Work Area by default or the full comp. You pick output module (codec, format) and file path. Two-step: queue → render.
9. **The ONE THING.** **Layer = row. Row stacking order = z-order in the viewer.** One mental model collapses compositing AND time into one panel. When a designer drags layer 3 above layer 2, it visibly jumps forward — that feedback is the whole magic.

---

## 6. Premiere Pro

**Context:** NLE for film editing. Canonical reference for clip-on-track mechanics.

1. **Layout.** Timeline panel full-width, bottom. Source Monitor + Program Monitor top, Project bin left, Effects/Audio right. Timeline is the gravity well.
2. **Layer-to-track mapping.** Tracks are independent of any "layer" concept. V1, V2, V3 (video, stacking upward — V2 is above V1) and A1, A2, A3 (audio, stacking downward visually but mixed equally). Multiple clips can live on a single track back-to-back. Higher V tracks render on top.
3. **Clip model.** Clip references a bin asset with In/Out subrange (set in Source Monitor), placed on a track at a timeline start time. Trim head/tail (drag edges), Ripple (closes gap), Roll (moves boundary between two clips), Slip (changes source in/out without moving clip), Slide (moves clip between neighbors). Linked video+audio move together by default; unlink to edit separately.
4. **Interaction.** Source Monitor workflow: open clip, set In/Out, then Insert (,) or Overwrite (.) at the playhead into track-targeted V/A tracks. Or drag directly from bin/Source Monitor to a track. Cut at playhead = Razor tool (C) or Ctrl/Cmd+K. Move = drag. Copy = Alt-drag.
5. **Playhead & scrubbing.** Click ruler to jump. Drag CTI. J/K/L shuttle (reverse/pause/forward, tap to speed up). Arrow keys frame-step. Timecode input in the top-left of timeline.
6. **Audio track.** Dedicated audio tracks below video tracks. When you drop a clip with embedded audio, video lands on Vn and audio auto-lands on An, linked. Visible waveforms; mixer for levels.
7. **Loop/duration.** Sequence auto-extends as you place clips — no fixed duration. Mark In/Mark Out on the sequence defines export range and loop region.
8. **Export/render.** File → Export → Media. Defaults to Sequence In/Out if set, else full sequence. Uses Media Encoder queue; H.264 mp4 is one preset click. Real-time preview during editing (rendered bars indicate cache state).
9. **The ONE THING.** **Track targeting + Insert/Overwrite at playhead.** Once users learn V1/A1 source-patches and comma/period, they can assemble a cut without ever touching the mouse. For Easel this is overkill, but the lesson — *playhead is always the drop point, tracks are independent of source* — is essential.

---

## 7. Unreal Engine Sequencer

**Context:** Cinematic/animation timeline for a real-time 3D engine. Relevant because Easel is also real-time + must export.

1. **Layout.** Sequencer is a dockable editor window (not permanently at the bottom like NLEs), with tracks listed on the left and a timeline ruler+content on the right. Opens per Level Sequence asset.
2. **Layer-to-track mapping.** Tracks are added per actor/property. One track per actor, with sub-tracks per animated property. Not layers in the compositing sense — tracks are the animation channels of 3D objects.
3. **Clip model.** **Sections** are the clip primitive: time-ranged containers of keyframes/state on a track. Sections can be finite or infinite, moved, trimmed, blended. Cinematic Shot tracks contain Shot sections that reference subsequences (nested timelines). Subsequence tracks similarly.
4. **Interaction.** Drag actors in to create tracks. Drag subsequence/animation/audio assets onto tracks to create sections; hold **Shift to snap to playhead** on drop. Right-click for cut/split. Sections are movable, trimmable, blendable.
5. **Playhead & scrubbing.** Green Start / Red End markers define playback range. Drag playhead to scrub (viewport previews live). Transport bar with frame-step. Keyboard shortcuts for zoom/frame-fit.
6. **Audio track.** Audio is a first-class track type — drag sound assets onto audio tracks as sections.
7. **Loop/duration.** Explicit Green/Red Start/End markers. Sequence has a playback range; sections can extend beyond (won't play). Subsequences compose long shows out of small sequences.
8. **Export/render.** Movie Render Queue (MRQ): add sequence(s), configure output resolution/codec/anti-aliasing, warm-up frames, render offline frame-by-frame. Note: MRQ end frame is exclusive. Separate from real-time preview.
9. **The ONE THING.** **Sections with Shift-snap-to-playhead drop.** Sections unify "keyframe group" and "clip" — a block you can grab, move, trim. Shift-snap makes "add this at the playhead" effortless. And the Green/Red start/end markers make the export range visually obvious on the timeline itself — no separate dialog to find it.

---

## SYNTHESIS

### Universal patterns (present in 4+ tools — safe defaults)

1. **Full-width timeline panel, bottom of screen.** AE, Premiere, MadMapper, Sequencer (when open), TouchDesigner. Even Synesthesia embeds a ruler. Default: put Easel's timeline full-width at the bottom.
2. **Playhead is draggable; clicking the ruler jumps to that time.** Universal. Frame-step via arrows is also universal among the serious ones (AE, Premiere, TD, Sequencer).
3. **Clips/sections as horizontal bars with draggable edges for trim.** AE, Premiere, MadMapper, Sequencer. Each bar = a placed instance of a source with start time + in/out.
4. **Drag-from-library / drag-from-bin to place.** AE, Premiere, MadMapper, Sequencer (and Resolume into grid cells). User picks asset, drags into timeline, drop position = start time.
5. **Explicit playback-range markers on the timeline.** AE Work Area (B/N), Premiere In/Out (I/O), Sequencer Green/Red, MadMapper loop section, TD Working Range. Universal pattern: the export range is *visible on the timeline itself*, not only in a dialog.
6. **Export respects the range markers by default, with an option for full.** AE Render Queue, Premiere Export Media, Sequencer MRQ, MadMapper Export button, TD Export Dialog. Universal.
7. **Audio is a first-class track on the same timeline** (except TouchDesigner and Resolume, which treat audio as signal). 4 of 7 tools have audio lanes. Video-with-audio imports drop both, linked, in all NLE-style tools.

### Divergent patterns (user must choose)

- **Layer == Track row, or Track independent of Layer?**
  - AE: layer IS the row (strict 1:1, one clip per row).
  - Premiere/MadMapper/Sequencer: tracks are independent — multiple clips per track, stacking order determined by track vertical order.
  - For Easel: since users think in "layers of shaders/videos stacked," AE's model is more intuitive than Premiere's. But it forbids back-to-back clips on one layer. Compromise: **layer = track, multiple clips allowed per layer, playing one at a time** (which happens to be how Resolume layers behave).

- **Fixed vs auto-extending duration.**
  - Fixed: AE comp, Sequencer, MadMapper timeline, TD range.
  - Auto-extending: Premiere sequence.
  - User asked for fixed (e.g. 1 minute). Go fixed. It matches AE/Sequencer/MadMapper — the tools that export finished pieces, not edits-in-progress.

- **"Clip" granularity.**
  - AE layer = long-lived track of one source + properties.
  - Premiere clip = short slice of source on a track.
  - Sequencer section = time-ranged keyframe group.
  - MadMapper has both (montage clip + value clip on different track types).
  - For Easel: **a clip is a placed instance of a shader/video source with {track, start_time, duration, source_in, source_out, speed}**. Parameters can be keyframed on the same clip (AE-style) OR on separate parameter tracks (MadMapper/Sequencer-style). Start with the simpler AE-style.

- **Grid vs linear timeline.** Resolume and Synesthesia avoid linear timelines entirely. Easel cares about linear export, so reject the grid as the primary model — but keep the lesson that live performance often wants *triggerable columns*. Optional v2 feature.

### Simplest intuitive default for Easel

Recommend this baseline, which cherry-picks the universal patterns and leans AE/Sequencer because those are the "render a finished piece" tools:

1. **Layout:** Timeline panel docked full-width at the bottom. Left gutter: track headers (name, mute/solo, collapse). Right: ruler + track content. Transport bar at the top of the timeline panel.
2. **Layer = Track (row).** Each row is one layer. Stacking order top-to-bottom in the timeline = render z-order (users can pick either AE's "top = front" or Premiere's "higher V track = front"; AE is more common among visual artists). Multiple clips allowed per row, one plays at a time where they are placed.
3. **Clip = a placed instance** of a source (shader or video) with `{track, start_time, duration, source_in, source_out, speed}`. Properties of the clip (uniforms, opacity, blend mode) are keyframable inline (AE-style: twirl-down reveals property rows with keyframes) rather than requiring separate parameter tracks.
4. **Fixed composition duration** (e.g. 60s at creation; editable in settings). Timeline ends at that duration; cannot drag clips past it.
5. **Work Area markers** on the ruler (draggable In/Out). Default to full duration. Export uses these unless overridden. Keyboard: I/O to set from playhead (Premiere convention, more discoverable than AE's B/N).
6. **Add a clip** via drag-from-library onto a track at the drop-time (AE/Premiere/MadMapper/Sequencer universal). Hold Shift on drop to snap to playhead (Sequencer convention — cheap to add, feels great). Right-click on a track at playhead for "Add [selected source] here." Cut at playhead = Cmd+K (Premiere convention).
7. **Playhead** always drawn as a vertical line across all tracks. Click ruler to jump. Drag playhead to scrub. Arrow keys frame-step, Shift+arrow for larger step. Spacebar play/pause. The viewport must update live as playhead moves — the feedback loop is the whole point.
8. **Audio track** as a dedicated track type at the bottom (NLE convention). When a video-with-audio clip is dropped on a video track, an audio clip auto-appears on the default audio track, linked, following Premiere's convention — the strongest "I understand this tool" moment in NLEs.
9. **Export** = single button on the timeline (MadMapper's move — don't bury it in a File menu). Dialog: uses Work Area by default, offers Full, shows Start/End/Duration auto-reconciled (TouchDesigner's auto-updating trio is the clearest), picks codec (H.264 mp4 default), renders offline frame-by-frame for frame-perfect output. Distinguish this from a separate **Record** button that captures the live real-time output (Resolume's split — crucial for a live-visuals tool that also exports).

### The single highest-leverage design choice

**Make the playback-range visible on the timeline ruler** (Sequencer-style Green/Red or AE-style Work Area bar) **and put the Export button right next to it.** This collapses three mental steps — "define the range," "confirm it's right," "export it" — into one spatial glance. Every tool that does linear export well does some version of this. Every tool that doesn't (Resolume, TouchDesigner) punishes users with a disconnect between what they see and what they render.

A close second: **the playhead must scrub the viewport in real time.** This is the single thing that converts "I have a bunch of clips on a grid" into "I am authoring a timed piece." If the viewport doesn't follow, the timeline is just a database view.

---

## Sources

- Resolume: https://resolume.com/support/en/layers, https://resolume.com/support/en/clip-time-panel, https://resolume.com/support/en/clip-renderer, https://resolume.com/forum/viewtopic.php?t=21124
- Synesthesia: https://synesthesia.live/resources/user-guide.pdf, https://www.synesthesia.live/public-release/changelog/1.23.0.114.html, https://app.synesthesia.live/docs/faq/video.html
- MadMapper: https://docs.madmapper.com/madmapper/6/what's-new/madmapper-v6, https://cdm.link/madmapper-timelines-clips-v6/, https://cdm.link/madmapper-6-brings-timelines-markers-clips-more/
- TouchDesigner: https://derivative.ca/UserGuide/Timeline, https://docs.derivative.ca/Animation_COMP, https://docs.derivative.ca/Time_COMP, https://derivative.ca/UserGuide/Export_Movie_Dialog, https://interactiveimmersive.io/blog/controlling-touchdesigner/get-off-the-timeline-1-systems-thinking/
- After Effects: https://helpx.adobe.com/after-effects/using/selecting-arranging-layers.html, https://helpx.adobe.com/after-effects/using/blending-modes-layer-styles.html, https://helpx.adobe.com/after-effects/using/workspaces-panels-viewers.html, https://www.schoolofmotion.com/blog/after-effects-timeline-shortcuts
- Premiere Pro: https://helpx.adobe.com/premiere-pro/using/adding-clips-sequences.html, https://helpx.adobe.com/premiere/desktop/add-audio-effects/basic-audio-editing/synchronize-clips-in-the-timeline-panel.html, https://www.peachpit.com/articles/article.aspx?p=2102372&seqNum=8
- Unreal Sequencer: https://docs.unrealengine.com/4.27/en-US/AnimatingObjects/Sequencer/Overview/ReferenceEditor/, https://docs.unrealengine.com/4.27/en-US/AnimatingObjects/Sequencer/Overview/Keyframing, https://dev.epicgames.com/documentation/en-us/unreal-engine/cinematic-workflow-tips-for-sequencer-in-unreal-engine, https://docs.unrealengine.com/4.27/en-US/AnimatingObjects/Sequencer/Overview/Tracks/TracksSub
