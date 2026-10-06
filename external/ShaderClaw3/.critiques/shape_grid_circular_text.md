# shape_grid_circular_text — critique

## Reference
`/Users/lu/Documents/A-List Shaders/shape_grid_circularshapes_text.jpg` — a
contact-sheet poster: irregular grid of cells, each holding a distinct
circular/curved soft-cell shape against a warm paper backdrop, with gradient
fills (cyan→pink, yellow→magenta, blue→orange) and editorial slab type
floating across the page ("F.12", "F.61", "3²", "F.8", "F.9", "6E"). It reads
as a research plate, not a chequerboard — the "variety of treatments" IS the
visual.

## Concept
A live R×C contact-sheet of circular SDF specimens, where each cell shows a
**different** circle treatment (10 variants: concentric rings, halftone dots,
dot mandala, gradient lens, eclipse crescent, segmented arcs, vesica, gear
rotor, ripple corona, scan-line disc). The grid is sliced into three
horizontal **depth bands** (back/mid/front); each band is owned by one
synthetic player (`player[1..3].energy`), parallax-shifted to its own plane,
and pops/brightens when that player speaks. The cue caption types out on a
slab baseline at the bottom — never the central visual.

## INPUTS / BIND map
| INPUT          | BIND                  | Role |
|----------------|-----------------------|------|
| msg            | cue.latest            | live typewriter caption |
| energyA / B / C| player[1..3].energy   | own back/mid/front band — pop + brightness |
| activeA / B    | player[1..2].active   | gates stroke weight per band |
| bassDrive      | audio.bass            | global stroke weight + lum lift |
| midDrive       | audio.mid             | orbital pump phase across variants |
| highDrive      | audio.high            | crispens AA fwidth |
| rows / cols    | —                     | grid geometry 3..7 |
| paletteMode    | —                     | Paper/Editorial/Acid/Mono |
| variantMix     | —                     | 0 = all concentric, 1.5 = full diversity |
| motionSpeed    | —                     | time multiplier |
| audioDepth     | —                     | audio-reactive gain |
| parallax       | —                     | per-band z-offset amount |
| popAmount      | —                     | per-cell energy pop amplitude |
| labelScale, kerning | —                | caption typography |
| paperColor, inkColor | —               | base palette |

≥3 controllable elements (R×C grid, variant mix, three depth bands), ≥2
inputs BIND to distinct `player[i]` channels (1/2/3 energy + 1/2 active), one
input is `msg` → `cue.latest`. Plus `audio.bass/mid/high` for richness.

## Rubric self-score (RUBRIC.md v2)
- **a. Multi-player separability — 4/5.** Three bands owned by distinct
  `player[i].energy`. You can A/B mute one player and the matching band
  visually quiets (cards desaturate, no pop). Each band lives on its own
  parallax plane — clear visual ownership. Not 5 because the band-stripe
  layout means owned cells stay co-located, so visually adjacent bands can
  bleed when both are loud.
- **b. Depth & dimensionality — 3/5.** Genuine 3-plane parallax with
  per-band camera offset, depth-scale, and haze gradient. Per-cell pop scales
  cells toward the viewer. Not raymarched — pseudo-3D.
- **c. Intentional motion — 4/5.** Multi-mode: stillness reads as a frozen
  contact-sheet; mid pumps orbital phase in mandala/arcs/scan-line/corona;
  bass adds stroke breath; energy spikes pop individual cells with per-cell
  thresholds so the pops *stagger* rather than mass-firing. Crescendos
  arrive as moments. Not 5 because the per-cell pump is sin-driven (always
  some idle motion).
- **d. Abstract not literal — 5/5.** No literal depiction. The grid is a
  research-plate metaphor; each variant is an abstract circle treatment,
  not a representation of the audio. Caption is a slab caption, not a
  rendered scoreboard or readable logo.
- **e. Surprise / risk — 4/5.** 10 distinct cell variants is a new corpus
  move — most ISF grids do one treatment everywhere. Band-owned per-player
  parallax bands + per-cell stagger pop is uncommon. Slab caption following
  the loudest player's anchor X is the small surprise. Not 5 because the
  technique (2D SDF grid + AA) isn't novel on its own — the composition is.

**Total: 20/25.**

## Anti-pattern check
- Plain checkerboard — **no.** Cells render distinct variants; even at
  `variantMix=0` the cell colours still vary per palette index.
- Single-color noise plane — no.
- Spectrum-analyzer bars — no.
- Mirror-symmetric horizon — no.
- Readable logo / decorative glyphs — caption is `cue.latest` only.

## Premium AA
Every SDF rendered with `fwidth`-driven smoothstep. `highDrive` narrows the
AA band (crispens highlights) without aliasing — derived from the per-pixel
`fwidth(r)` so it stays resolution-independent.

## Validation
`glslangValidator -S frag` (with the Easel ISF preamble) — **exit 0**, clean.

## Files
- shader: `/Users/lu/easel/shaders/shape_grid_circular_text.fs`
- bundle: `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/shape_grid_circular_text.fs`

## Caveats / future tunes
- Caption row hard-positioned at the bottom — could orbit the active cell
  for stronger "follows the speaker" feel.
- `cellBand` stripes are row-aligned; a checker-style ownership (each player
  owns scattered cells) would push axis (a) toward 5 but loses parallax
  clarity. Tradeoff held in favour of depth.
- Only 10 variants — corpus could grow to 16 if more diversity wanted.
