# digital_wave_grid_text — stacked perspective wireframe terrains + title

## Reference

`/Users/lu/Documents/A-List Shaders/digitalabstract wave grid simple stacked full screen_text.jpg`
— filename brief: a digital, abstract wave grid composition, simple,
stacked, full-screen, with text. (The JPG on disk had been overwritten
with an unrelated archival image, so the filename + slug were used as
the design contract.)

## Concept

Three independent perspective-projected wireframe terrains stacked
vertically across the full canvas. Each band is its own world:

- **Band A (upper third)** — driven by `player[1].energy`, palette A.
- **Band B (middle third)** — driven by `player[2].energy`, palette B.
- **Band C (lower third)** — driven by `player[3].energy`, palette C.

Bands STACK across screen-y with thin sky strips between them — they
do **not** mirror across a central horizon (explicit anti-pattern guard).
Each band is a perspective-projected mesh: rows compress toward the
band's far edge, columns hold parallel in screen-x but get warped by
the band's own travelling-sine height field. Wave amplitude rides each
band's player energy (idle floor so silent bands still ripple).

`cue.latest` flows into `msg`; the typewriter title is rendered as an
engraved slab inside the sky gap between bands A and B, with msgAge
driving a left-to-right reveal at ~28 cps. The text rides the grid,
never replaces it.

### Wave mechanic (surprise)

Not a 2D sine. Each band has its own perspective camera that
foreshortens rows toward a band-local vanishing line, and the wave
displaces row positions in mesh-V space — so crests pinch tighter at
the far edge and stretch at the near edge. The wireframe distance
function is analytic (fract on the bent V coordinate) so lines hold
fwidth-AA edges at any density, and ridges at wave crests get a small
brightness halo. Three travelling sines per band + a diagonal crest +
a tiny audio-bass spike give every frame fresh motion variance.

## INPUTS & BIND

| input | bind | role |
|---|---|---|
| `msg` (text) | `cue.latest` | typewriter title; reveal via `msgAge` |
| `energyA` (float) | `player[1].energy` | band A wave amplitude |
| `energyB` (float) | `player[2].energy` | band B wave amplitude |
| `energyC` (float) | `player[3].energy` | band C wave amplitude |
| `audioDepth` (float) | manual | global energy multiplier (audio.* fallback) |
| `gridDensity` (float) | manual | mesh rows/cols per band |
| `waveAmp` (float) | manual | global wave amplitude |
| `perspective` (float) | manual | row-compression strength |
| `motionSpeed` (float) | manual | global time multiplier |
| `lineWidth` (float) | manual | wireframe line weight |
| `textSize` (float) | manual | title scale |
| `variant` (long) | manual | 0 Tide / 1 Magenta / 2 Mono |
| `skyA`, `skyB` (color) | manual | sky gradient endpoints |
| `lineA`, `lineB`, `lineC` (color) | manual | per-band line colours |
| `inkColor` (color) | manual | title ink |

Audio fallbacks read `audioBass` / `audioMid` / `audioHigh` so the shader
still breathes when no players are bound.

Binding floor: **4 channel binds** (1 `cue.*` + 3 `player[i].energy`).
Hard floor passed.

## Validation

glslang: `glslangValidator -S frag /tmp/dwgt_full.glsl` → EXIT=0 with the
Easel ISF preamble (RENDERSIZE, TIME, mousePos, audio*, msg_0..47,
msg_len, msgAge, fontAtlasTex). Clean pass.

## 5-axis self-score (RUBRIC.md v2)

| axis | score | rationale |
|---|---|---|
| **a. Multi-player separability** | 4/5 | Three independent player binds, each owns a screen-y band with distinct palette, wave seed, time offset and density bias. Muting any player visibly flatlines its band to the idle floor. Not a 5 because the three bands share one wireframe vocabulary — different palettes/clocks, same structural language. |
| **b. Depth & dimensionality** | 4/5 | Each band uses real perspective projection (`bandProject` / `bandUnproject`) with row foreshortening, plus per-band depth fog, ridge highlights and a slow camera dolly. Not raymarched, so the rubric caps at 4 — but the depth is *computed* per pixel, not faked with a 2D gradient. |
| **c. Intentional motion** | 4/5 | Idle floor keeps lines rippling even at silence; player energy ramps amplitude and ridges, audio.* injection adds a small bass-tied tug, and per-band time offsets break sync so the three bands never pulse together. Title typewriter adds a discrete compositional event. Not 5 — no surprise stops/holds beyond the typewriter reveal. |
| **d. Abstract not literal** | 5/5 | No EKG, no spectrum bars, no checkerboard, no mirror horizon. The bands are stacked (not reflected); each is a *field*, not a depiction of a thing. The title is the only readable artefact and it lives in the sky gap, not in place of imagery. |
| **e. Surprise / risk** | 4/5 | The corpus has grid shaders (`grid_text`, `images_grid_*`) but none stack three independent perspective-projected wave terrains as full-screen horizontal bands; the analytic bent-V wireframe distance + per-band foreshortening is a new authoring move in this corpus. Not 5 because the wireframe-terrain idiom itself is established outside the corpus. |
| **total** | **21/25** | |

## Anti-pattern checklist

- EKG line across canvas? **no** — no canvas-spanning sine traced as a stroke.
- Spectrum bars? **no** — audio drives amplitude/depth, never bar height.
- Default checkerboard / SDF debug grid? **no** — the grid is a projected mesh, displaced by a height field, with foreshortening and depth fog.
- Mirror-symmetric horizon? **no** — bands stack, do not reflect; each band has its own palette, clock and density bias.
- Literal icon / readable logo as central visual? **no** — only the cue typewriter slab between bands, which is the legitimate `msg` channel.
- Single-colour noise plane? **no** — wireframe lines + ridges + depth fade + per-band palette.

## Files

- Source: `/Users/lu/easel/shaders/digital_wave_grid_text.fs`
- Bundle copy: `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/digital_wave_grid_text.fs`
- This critique: `/Users/lu/ShaderClaw3/.critiques/digital_wave_grid_text.md`

## Caveats

- The reference JPG on disk had been overwritten with an unrelated
  archival image; the slug filename was used as the design contract.
- No raymarch loop — depth is per-band perspective + fog, capped at axis
  (b) = 4 per the rubric.
- The shader uses `texture2D` and `gl_FragColor` for ISF/desktop-GL
  compatibility (matches `text_clusters.fs` and `grid_text.fs`).
