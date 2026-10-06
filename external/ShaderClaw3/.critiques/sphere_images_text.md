# sphere_images_text — critique

## Reference

`/Users/lu/Documents/A-List Shaders/sphere_images_text.jpg` — a hub-and-spoke
social-graph: a dense central cloud of small image-tiles with thin filaments
fanning to outer image-tiles around the rim. Reads as a constellation of
miniature portraits / icons gravitating around a center.

## Concept

A raymarched constellation of small textured spheres orbiting a central hub
of typewritten cue text. Three concentric shells, each bound to one
`player[i].energy` channel. The bass channel dollies the camera (depth
breath). Spheres wear procedural "image cutouts" on their surface (three
modes: cutout grid, photo tile, marbled) — a portrait *feeling* without
literal photographs. Thin screen-space filaments connect each sphere back
to the hub, evoking the social-graph tracery in the reference without
becoming the literal graph.

Speech is the gravity well: louder shells tighten their orbits, brighten
their image-cutouts, and pulse their filaments. Silence reads as still
spheres drifting on their own quiet axes.

## INPUTS with BIND

| INPUT | TYPE | BIND |
|---|---|---|
| `msg` | text | `cue.latest` (typewriter into hub) |
| `innerEnergy` | float | `player[1].energy` |
| `midEnergy` | float | `player[2].energy` |
| `outerEnergy` | float | `player[3].energy` |
| `audioDepth` | float | `audio.bass` |
| `sphereCount` | long | manual (12/18/24/32/40) |
| `sphereSize` | float | manual |
| `surfaceMode` | long | manual (Cutout Grid / Photo Tile / Marbled) |
| `palette` | long | manual (Paper / Neon / Ember / Glacier) |
| `motion` | float | manual |
| `filaments` | float | manual |
| `textSize` | float | manual |
| `fog` | float | manual |

4 channel binds total: 3× `player[*].energy` + 1× `audio.bass` + `msg →
cue.latest`. Passes the binding-less hard-floor cleanly.

## Validation

glslang pass: clean (exit 0). Mirrors `ShaderSource.cpp:235-360` preamble
(`#version 330 core`, `FragColor`, `gl_FragColor`/`texture2D` redirect,
text inputs expanded as `msg_0..msg_47` + `msg_len`, audio uniforms,
`fontAtlasTex`). No warnings.

## Rubric self-score — /25

- **a. Multi-player separability — 4/5**: three shells, three distinct
  player-energy binds; each shell's spheres respond independently
  (size, rotation speed, image-cutout brightness, filament intensity).
  Not 5 because the three shells are visually similar (radial rings of
  spheres) — the per-shell color tint helps but they don't have totally
  separate visual languages.
- **b. Depth & dimensionality — 5/5**: genuine raymarched 3D with
  perspective foreshortening, atmospheric haze, depth-driven sphere
  size, and bass-driven camera dolly. You could mentally orbit it.
- **c. Intentional motion — 4/5**: per-sphere axes + per-shell energy +
  bass dolly + camera breath produce distinct stillness vs. crescendo
  states. Not 5 because there's no explicit "hold / surprise stop"
  beat — motion is energy-mapped continuously.
- **d. Abstract not literal — 4/5**: surface "images" are procedural
  (frames + abstract tile colors + fbm photo-substitute), not literal
  photographs. The filaments evoke a social graph without drawing one.
  Not 5 because the structural homage to the reference (hub + outer
  ring) is still readable.
- **e. Surprise / risk — 4/5**: combining raymarched spheres with
  procedural per-sphere surface "image cutouts" and a screen-space
  filament overlay is a composite move I don't see in the corpus.
  Three surface modes + four palettes multiply the design space.

**Total: 21/25** — clears the 18/25 PR threshold.

Anti-patterns triggered: none. The center text is the cue, not decorative
glyphs; the spheres are abstract balls, not a literal photo-graph.

## Files

- `/Users/lu/easel/shaders/sphere_images_text.fs` (source of truth)
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/sphere_images_text.fs` (bundle copy)
- `/Users/lu/ShaderClaw3/.critiques/sphere_images_text.md` (this file)

## What to try next

- A "scatter" mode where one shell breaks orbit and drifts off-screen
  when its player goes silent for >2s — would push axis (c) to 5.
- Replace the procedural photo-fbm with sampling `mpFaceLandmarks` /
  pose for a live "the spheres wear your audience" effect (still
  abstract because they sample chunks, not whole frames).
- Vision-driven shell binding: `vision.pose.handsHeight` could swap
  which shell owns which player so the visual rearranges live.

## Caveats

- 40-sphere worst case + 96 raymarch steps is the heaviest path; on
  retina at full size expect ≈3-5ms per frame on M-series. Drop
  sphereCount to 18 if frame budget is tight.
- The screen-space filament pass walks all spheres per pixel — O(N)
  inside the fragment shader. Caps at 40 keep this safe.
- Surface "Photo Tile" mode uses fbm3 inside the surface — second-most
  expensive mode. "Cutout Grid" is cheapest.
