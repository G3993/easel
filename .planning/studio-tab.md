# Studio Tab — UX Spec

A new top-level workspace where the user **reviews the generative loop's output,
tunes the synthetic-player decomposition, and inspects live channels**. Not in the
inspector. Not in the right panel. Same row as CANVAS / STAGE / SHOW.

## Placement & naming

Top-nav tab, alongside the existing workspace modes (`UIManager::WorkspaceMode` in
`src/ui/UIManager.h:75` — `Canvas | Stage | Show`). New tab: **`STUDIO`**.

Why STUDIO: this is where the *making* happens — review of nightly drops, tuning the
intelligence layer, sketching with channels. CANVAS is for compositing a show; STAGE
is for 3D mapping; SHOW is for live performance. STUDIO is the back-of-house workshop.

Alternative considered: **`LAB`**. Discarded — "lab" implies experiments are not yet
production. The drops here ARE production. STUDIO captures "this is where you work
on what you'll perform."

Entry: top nav, after SHOW. Keyboard: `⌘4` (Canvas=⌘1, Stage=⌘2, Show=⌘3 are the
existing pattern; verify and continue).

## Sections (vertical scroll)

### a. Daily Drops

The last 7 days of generated shaders (from the SYSTEM.md loop).

Per-card layout:
- **Thumbnail** — rendered screenshot (the same one used for scoring)
- **Reference riff** — thumbnail strip of the 1–3 reference images this shader was
  inspired by (click to open full reference)
- **Score breakdown** — five mini-bars for the rubric axes (a–e), total /25 large
- **Concept line** — the one-liner from the critique file
- **Actions row** —
  - `Load into selected layer` — calls `Application::loadShader(.fs path)` against
    the currently selected layer (or appends if none).
  - `Open critique` — opens `.critiques/<slug>.md` in the side drawer.
  - `Rate ★ 1–5` — writes to `ShaderClaw3/.shader_ratings.json`. **User rating feeds
    back into reference-selection weighting on the next loop run** (RUBRIC.md →
    Calibration; SYSTEM.md → step 1).
  - `Reject` — soft-delete the candidate (PR closed, artifact archived).

Grouped by day. Today's drops pinned at top.

### b. Player Routing

Where the user *sees* and *tunes* the synthetic-player decomposition shaders are
binding to (intelligence-layer.md → "Player channels v1").

- **N selector** — number of synthetic players, 1..6. Default 3.
- **Active indicator** — per-player LED that lights when `player[i].active > 0.5`.
- **Live readouts** — per-player `energy`, `pitch`, `confidence` numerics + tiny
  sparkline (last ~3 seconds).
- **Band assignment** — visual: a horizontal log-frequency strip with each player's
  owned band drawn as a colored region. Drag-to-resize bands; double-click to reset
  to even split.
- **Manual override** — per-player toggle: `Force Active` pins `active=1` (useful for
  authoring/debugging; also for the "I want to act as player 2 right now" case).
- **Hysteresis** — two sliders (attack ms, release ms) per the heuristic in
  intelligence-layer.md. Defaults: 60ms attack, 400ms release.
- **VAD threshold** — one slider; visualized with the live mic level for
  reference.

Below the panel: small note: *"Synthetic v1. Phase 3 swaps in real diarization;
shader binds stay identical."*

### c. Channel Inspector

Live read-only view of every channel in `DataBus`. For authoring and debugging.

Layout: collapsible groups matching the intelligence-layer namespace.

- **audio** — `audio.level/bass/mid/high`, FFT mini-spectrum, each with sparkline
- **players** — duplicate of section (b)'s readouts, denser; sparklines for energy
- **cue** — text channels: latest, transcript (truncated), coach.* fields; numeric:
  none today
- **data.*** — anything pushed to `DataBus.setNum/set` under the `data.` prefix;
  empty by default until user wires a feed
- **transport** — `time`, `bpm`, `beat` (phase bar)
- **vision** — existing `vision.*` numerics

Each row: name • current value • sparkline (last ~3s) • copy-key button (copies the
channel name to clipboard for pasting into an ISF `BIND` declaration).

### d. Improvement Backlog

A notes panel where the loop logs *"things to improve next"* after each nightly run
(the user's explicit ask).

- **Top of panel**: input box — user can also add their own backlog items.
- **List**: each entry has a timestamp, source (`loop` or `user`), text body, and
  controls — **star** (promotes to a constraint for next run) and **dismiss**
  (archives).
- **Starred items** are read by SYSTEM.md → step 2 and injected as hard constraints
  into the next concept-drafting step.
- **Filters**: All / Starred / Dismissed / By-shader.

Example loop entries:
- *"Recent drops lean symmetric. Push asymmetric compositions next run."*
- *"`player[3]` rarely activates — band assignment may be too narrow."*
- *"Three shaders this week used the same teal/magenta palette. Force a new palette family."*

### e. Control surface (MIDI-style) — Phase 2 stub

Stub section claiming the space. Not designed yet.

Placeholder copy:
> **Control Surface — coming in Phase 2.** A user-mappable interface for playback,
> layer toggles, scene jumps, and channel overrides. Mappings here will surface
> alongside MIDI mappings (`MappingProfile` in `src/app/MappingProfile.h`).

A single button: *"Tell me when this ships"* (no-op for now, or links to a planning
doc).

## Cross-cutting

- **Keyboard-friendly** — every interactive element is tab-reachable; common
  actions have shortcuts (R to rate selected drop 1–5, L to load into layer, X to
  dismiss).
- **Dark theme** — matches the spacing/color palette already in the inspector
  (PropertyPanel.cpp). Audit spacing as a grid on every pass.
- **Premium feel** — quiet UI, large clear thumbnails for drops, monospace for
  channel values, no chrome that isn't doing work.
- **Settings-level entry** — the user reaches STUDIO from the top nav. It stays out
  of the live performance UI (SHOW) but is one click away. Don't surface drops in
  the live UI; they belong in the workshop.

## Out of scope for this doc

- The actual reference image folder browser (file picker / drag-and-drop UI)
- The PR-merge dance (handled by GitHub UI; STUDIO just shows the loop's outputs)
- Real diarization UI (Phase 3 — band assignment block stays, becomes "speaker
  identity" labels)
- Custom `data.*` feed registration UI (lives in Settings → Data Sources, separate)
