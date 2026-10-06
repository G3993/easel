# Critique — meaningful_forms_text

## Concept
A poster grid of nine small luminous "sculptures of meaning" — each cell a
self-contained raymarched aura with its own intent. The reference (Beaming
Design's *meaningful things*) is a 3×3 of mint/aqua/lilac auric orbs with
editorial captions; we evoke it as live data: each form is bound to its own
channel so when a voice arrives, *that* form swells while siblings sit in
intentional stillness. `cue.latest` types a poetic caption under the loudest
form. Iridescent thin-film + soft pink cores + dotted mint rings track the
reference palette without ever quoting a literal glyph from the poster.

## INPUTS with BIND
- `msg` (text) → `cue.latest` — masthead + caption typewriter
- `energyA..E` (float ×5) → `player[1..5].energy` — five forms breathe per player
- `activeA/activeB` (float ×2) → `player[1].active` / `player[2].active`
- `bassDrive` (float) → `audio.bass` — drives form 6
- `beatPhase` (float) → `transport.beat` — gentle idle pulse for trailing forms

Style controls (no BIND, all user-shapeable):
`formCount {4,6,8,9,12}`, `formVariant {Aura sculptures, Folded sheets, Ribbon knots}`,
`subBlobs {3..7}`, `palette {Aurora, Cool tide, Sunset, Mono ink}`, `paletteShift`,
`motionSpeed`, `breathe`, `audioDepth`, `depthAmount`, `dof`, `bloom`, `fog`,
`paperColor`, `inkColor`, `ringTint`, `showMasthead`, `textSize`, `captionUnder`,
`grain`.

## glslang validation
`#version 330 core` preamble (mirrors `ShaderSource::translateFragment`) +
INPUTS-derived uniforms (including `msg_len`, `msg_0..msg_47`, `msgAge`,
`fontAtlasTex`, `audio*`, `RENDERSIZE`, `mousePos`, …). `glslangValidator`
exits 0 with no warnings on both the source file and the bundle copy.

## Rubric self-score — /25
- **a. Multi-player separability — 5/5** — Seven distinct channel binds with
  visually distinguishable outputs (5× `player[i].energy`, `audio.bass`,
  `transport.beat`, plus `player[1/2].active` lift). Each form lives in its
  own grid cell; muting a player visibly silences exactly one sculpture.
- **b. Depth & dimensionality — 5/5** — Genuine per-cell raymarch with orbiting
  camera, normal-based lighting, thin-film fresnel, atmospheric fog, and a
  depth-of-field wash on far forms. Three sculptural variants (aura blobs,
  folded sheets, ribbon knot torus) all in 3D.
- **c. Intentional motion — 4/5** — Idle uses `breathe` + tiny beat-phase
  pulse so silence reads as stillness, not freeze; energy adds orbit-lean
  and core swell. Crescendos shift one form forward against still siblings
  — a *compositional event* rather than a uniform pulse. Could push further
  with surprise-stop holds; left at 4 honestly.
- **d. Abstract not literal — 5/5** — Pure abstract sculptures of meaning.
  Text is editorial caption, not a logo. No bars/EKG/horizon/checkerboard.
- **e. Surprise / risk — 4/5** — Cell-local raymarch grid (rather than a
  single scene SDF) is a novel composition move for the corpus; the per-form
  dotted-ring tick band quotes the reference poster's "outlined orb" aesthetic
  without being literal. Palette/material choices extend rather than copy
  `forms_text.fs`. Not a 5 — the underlying technique still descends from
  the smooth-min raymarch family already in `color_world.fs` / `forms_text.fs`.
- **Hard-floor**: passed (7 `player[*]` / `audio.*` / `cue.*` / `transport.*`
  binds).
- **Anti-pattern audit**: no bars, no spectrum, no symmetric horizon, no
  rendered logo. Captions are reveal-paced `cue.latest`, not decoration.

**Total: 23/25**

## Files written
- `/Users/lu/easel/shaders/meaningful_forms_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/meaningful_forms_text.fs`
- `/Users/lu/ShaderClaw3/.critiques/meaningful_forms_text.md`

## Caveats / known limits
- Per-pixel raymarch over a 3×3 (up to 4×3) grid with a 48-step bound + cell
  AABB cull is well-bounded on modern GPUs but heavier than the flat 2D
  text-cluster shaders; if perf is tight on integrated GPUs, drop
  `formCount` to 4 or 6.
- The grid grows roughly square — `formCount=12` at 16:9 becomes 4×3 with
  smaller cells; tune `textSize` if the masthead crowds at high counts.
- Caption defaults to the loudest player; user can pin via `captionUnder`.
  In total silence the "loudest" is form 0 (deterministic, no flicker).
- Per-form raymarch shares the same scene SDF for normal estimation (5
  extra `formMap` calls) — acceptable cost given the small cell footprint.
- No `.easel` edits, no relaunch, no commits — pickup happens on next
  shader scan.
