# Critique — `maximalist_3dshapes_text`

Date: 2026-05-20
Reference: `/Users/lu/Documents/A-List Shaders/maximalist_images_3Dshapes_text.jpg`
Files:
- `/Users/lu/easel/shaders/maximalist_3dshapes_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/maximalist_3dshapes_text.fs`

## Concept

The reference is a corecore / internet-poster collage: warm paper ground, hard
chromatic vertical "lightning" axis bisecting the page, overlapping headline
typography (Majestic Casual, Snail Away, Unluca…) bursting through layered
image cutouts (rose, sculpture-bust wing, fingertips, ROBOT, pixel-card),
stickers + emoji-stars scattered, ticker bars on top/bottom edges. We rebuild
the *feeling*: maximalist, asymmetric, lo-fi, busy but legible.

The piece is genuinely 3D. Raymarched shape swarm (sphere / torus / capsule /
prism) tumbles in real world-space with per-instance rotation, and is sorted
in z against a procedural cutout swarm (tulip, bust, robot, fingers, pixel
card, sticker-star, wing, QR-pad, smiley dot). The lightning is a chromatic
SDF with R/G/B offset and bass-driven pulse. Headline wraps word-aware in a
center band with per-word color and outline-only variants.

## INPUTS w/ BIND

| Name | Type | Purpose | BIND |
|---|---|---|---|
| `msg` | text | Headline ribbon | `cue.latest` |
| `swarmA` | float | 3D shapes twitch | `player[1].energy` |
| `swarmB` | float | Cutout swarm jiggle (even idx) | `player[2].active` |
| `swarmC` | float | Sticker-dot band selection | `player[3].pitch` |
| `boltPulse` | float | Lightning intensity envelope | `audio.bass` |
| `shapeCount` | long | 3..8 raymarched shapes | — |
| `cutoutCount` | long | 3..9 procedural cutouts | — |
| `palette` | long | Poster / Acid / Mono / Risograph | — |
| `motion` | float | Tempo master | — |
| `audioDepth` | float | Audio→visual gain | — |
| `density` | float | Saturation master | — |
| `headlineSize` | float | Headline glyph height | — |
| `boltJitter` | float | Lightning axis chaos | — |
| `grain` | float | Print noise | — |

Five distinct live binds: 3 player channels, 1 audio band, 1 cue text. Hard
floor passed.

## Validation

`glslangValidator -S frag` against the simulated Easel preamble: PASS, no
warnings, no errors. (723-line translated output.)

## Rubric self-score /25

| Axis | Score | Rationale |
|---|---|---|
| (a) Multi-player separability | **5** | Three player channels each drive a *visually distinct* layer: swarmA → 3D shape twitch (you can see the shapes kick), swarmB → cutout offset on even indices (the bust/robot/pixel-card jiggle), swarmC → sticker-dot band selection (different cells light up as pitch changes). Muting any one is immediately identifiable. |
| (b) Depth & dimensionality | **5** | Real raymarched 3D shape swarm with per-instance rotation; cutouts sorted in z against the shapes (two-pass behind/in-front compositing); parallax drift; shadow under each cutout reads as ground; lightning sits on top plane. |
| (c) Intentional motion | **4** | Multi-mode: shapes tumble continuously, swarms twitch on player events, lightning pulses with bass (silent → still axis; bass crescendo → strobe). Cutouts have slow Lissajous + per-event kick. Could earn a 5 with explicit silence-hold composition, currently it idles. |
| (d) Abstract not literal | **4** | Cutouts evoke (tulip-bloom, bust silhouette, fingers, robot, pixel card, sticker-star) but none are photoreal; the lightning is a chromatic SDF, not a depicted bolt; the 3D shapes are abstract primitives. We don't show a literal scoreboard / EKG / spectrum. Not a 5 because some cutouts (smiley, star) are recognizable icons. |
| (e) Surprise / risk | **4** | Z-sorted hybrid raymarch + 2D cutout collage with chromatic SDF lightning is not in the corpus — `images_3dshape_text.fs` uses one cone with surface-wrapped text; this uses N shapes and z-sorts them against a card swarm. Word-aware multi-line headline with per-word color/outline mode is also new. |

**Total: 22/25** — clears hard floor, no anti-patterns triggered.

## Anti-pattern check

- No spectrum bars ✓
- No EKG waveform ✓
- No SDF debug grid ✓
- No single-color noise plane ✓
- No mirror symmetry (lightning at x≈0.04 is intentionally off-center, cutout layout uses golden-angle distribution) ✓
- No central logo / readable text-as-decoration (cue.latest headline lives in a deliberately off-center band, but the headline IS the text input — not decoration) ✓

## Caveats

- Headline word-wrap uses a 4-row cap; ultra-long messages will clip. Trade-off
  for staying inside the band.
- Sticker dot swarm uses pitch as a *band selector*, not a continuous knob —
  this is intentional (rubric (a) bonus for distinguishable response), but
  feels stepped if player[3].pitch jumps fast.
- Raymarch budget is 56 iters with no early-out tightening — fine on M-series
  GPUs at 1080p, may need a `MAX_STEPS` knob later for projector-grade
  resolutions.
- Cutout art is procedural and intentionally lo-fi; not a real image atlas.
  Phase 2 could swap selected `kind` slots for `IMG_NORM_PIXEL` calls bound to
  user image inputs.
