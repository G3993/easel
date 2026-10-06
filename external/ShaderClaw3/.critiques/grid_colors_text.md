# grid_colors_text — critique

## Reference

Ville Pulkki, "Musical Analogues of Mathematical Concepts" (M8 Art Space, 13 Dec 2018 – 31 Jan 2019). A black-ground broadside collaged from saturated rectangular blocks (vermillion, lime, ultramarine, lilac, platinum), each block slammed with editorial headline type that crosses block boundaries. Six chromatic horizontal-stripe inserts punctuate the rhythm. The composition reads as a stack: blocks are *layered*, the type sits "in front of" the color slabs, and the rainbow stripes are clearly inserts cut into individual cells. Lower half repeats the upper grammar in greyscale — same composition, drained palette.

## Translation choices

| Reference move | Shader move |
|---|---|
| Stacked color slabs, layered z | Three depth layers (back/mid/front), painter's algorithm composite, each layer parallaxed differently |
| Type bleeding across blocks | Per-cell typewriter rendered with `msg`/`cue.latest`; each cell starts at a different msg offset (seeded) so a sentence echoes-fragmented across the grid |
| Six-color chromatic stripe inserts | `stripes()` band drawn on ~30% of cells; scrolling phase keeps them alive every frame |
| Bold vermillion/lime/ultramarine/lilac/platinum on near-black | "Pulkki" palette is exactly that; 3 alternate schemes (Broadside / Riso / Mono) |
| Greyscale repeat in lower half | Not a literal mirror — back layer is desaturated 18% as a depth-fog cue; mvar=2 "Stack" further varies aspect ratios to evoke the lower-half look |
| Mis-aligned column rhythm | Row-stagger: every other row shifts 30%; corner-jitter so no two cells line up perfectly |

## Anti-pattern guard

- **Not a checkerboard**: cells have per-cell aspect (0.55..1.55), corner jitter, tilt (`mvar=1` tilted, `mvar=2` stack), independent existence gates per active channel, and three layers parallaxed at different speeds. The grid breathes; columns drift across each other.
- **Not a literal poster**: no logo, no readable date strings unless the user types them into `msg`. The reference's words become a typographic *texture* — the same phrase scattered, fragmented, rebuilt per cell.
- **Not spectrum bars**: stripes are insert-bands, scrollable, color-cycling — not VU.
- **Not single-color noise**: every cell is colored from a 6-slot palette per scheme; bg has a warm purple noise cast.

## INPUTS — binding contract

| INPUT | Type | BIND | Role |
|---|---|---|---|
| `msg` | text | `cue.latest` | typewriter source; per-cell seeded offset → fragmented mosaic |
| `energyA` | float | `player[1].energy` | bloom + shake on back layer |
| `energyB` | float | `player[2].energy` | bloom + shake on mid layer |
| `energyC` | float | `player[3].energy` | bloom + shake on front layer |
| `activeA` | float | `player[1].active` | gates back-layer cell existence (hard-cut compositional event) |
| `activeB` | float | `player[2].active` | gates mid-layer cell existence |
| `audioDepth` | float | `audio.bass` | pushes layer z forward on bass hits |
| rows / cols / palette / variant / motionSpeed / parallax / textSize / stripeAmount / shadow | style | — | 9 controllable style INPUTS |

7 channel-bound INPUTS · 3 distinct `player[i].*` channels (`player[1].energy`, `player[2].energy`, `player[3].energy` + `player[1].active`, `player[2].active`) · 1 `cue.latest` · 1 `audio.bass`. Well past the ≥2 distinct-bind floor.

## Rubric self-score (/25)

| Axis | Score | Rationale |
|---|---|---|
| a. Multi-player separability | 4/5 | 3 layers each owned by one player; muting `energyA` flattens the back, muting `activeA` removes back-layer cells. Layer C is partially bound (energy + derived active). |
| b. Depth & dimensionality | 4/5 | Three independent parallax layers with per-layer rotation, per-layer drift speeds, per-tile drop shadow sampled separately, bass-driven z-push. Not raymarched, but reads as genuine z-stack. |
| c. Intentional motion | 4/5 | Silence → cells collapse to seeded constellation; energy → bloom + shake; active=0 → cells vanish (compositional cut); bass → layers push forward; stripes scroll independently. |
| d. Abstract not literal | 5/5 | Reference's *type as texture* — the headline becomes a mosaic of fragments, not a readable phrase. Color rhythm rather than depiction. |
| e. Surprise / risk | 4/5 | Three independent parallax layers with painter's-algorithm composite, per-tile drop shadow pass, row-staggered grid that hides any checkerboard read. Stripe inserts as audio-modulated band. |
| **Total** | **21/25** | Above 18 threshold; hard floor passed (7 bound INPUTS). |

Anti-pattern auto-fail list: none triggered. No EKG, no VU bars, no checkerboard (row-stagger + corner jitter + per-tile aspect break it), no horizon mirror, no literal logo.

## Caveats / known gaps

- Per-cell text starts at a `seed`-derived offset; with short `msg` strings (<8 chars) cells will repeat the same fragment. Acceptable — matches reference's repetition aesthetic.
- Three layers × per-fragment grid math: ~50–80 ALU ops per cell × 3 layers + shadow pass. Should be well under 2ms at 1080p on Apple Silicon; not benchmarked.
- `player[3].active` isn't bound (only 5 BIND slots used for players); front layer fakes it from `activeA + activeB`. Fine compositionally; could add a 6th BIND slot if Studio wants strict separability on layer C.
- The chromatic-stripe insert is gated by `seed.x > 0.65`. For very small grids (cols=2, rows=3) it may not appear on any tile. Default 6×4 has ~8 stripe candidates.

## Files

- Source: `/Users/lu/easel/shaders/grid_colors_text.fs`
- Bundle: `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/grid_colors_text.fs`
- Validator harness: `/tmp/validate_grid_colors_text.sh`
- glslang result: clean (`#version 330 core`, Easel preamble, no warnings).
