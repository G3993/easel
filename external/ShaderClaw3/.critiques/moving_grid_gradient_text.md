# moving_grid_gradient_text — critique

**Reference**: `/Users/lu/Documents/A-List Shaders/moving grid and gradient with _text.jpg` — bleeding magenta/cyan/amber pigment clouds on warm paper, with annotation labels and an axis grid drawn over the field. The compositional read is "spray on paper with a structural lattice over it". The shader translates that into a moving perspective grid over a chromatic gradient field of bleeding pigment blobs, with cue text as the structural overlay (not literal labels).

## Concept (one line)
Three depth layers — gradient pigment field (back) · warped perspective grid sweeping diagonally (mid) · cue.latest typewriter slab (front) — each driven by a distinct channel so the visual decomposes by player.

## Channel decomposition (axis a — multi-player separability)
- `msg` → `cue.latest` — typewriter title slab (depth z=0, front)
- `energyA` → `player[1].energy` — bends grid ROW height-field (vertical warp amplitude)
- `energyB` → `player[2].energy` — bends grid COLUMN ripple (horizontal warp amplitude)
- `playerC` → `player[3].active` — boosts gradient blob "breathing" pulse (back layer)
- `audioBass / audioMid / audioHigh / audioLevel` — secondary nudges so the comp still breathes if player binds aren't routed
- Mute any one channel → its layer goes quiet: zero `energyA` flattens row warp into idle drift only; zero `energyB` straightens columns; zero `playerC` calms the gradient's bloom. A/B is visible.

**Self-score: 4/5** — 3 distinct player binds with 3 distinct visual languages (row warp vs column ripple vs gradient bloom). Not 5 because the binds are all on the same grid object (different axes of the same lattice) rather than 3 fully separate entities.

## Depth & dimensionality (axis b)
- z=2 gradient field with parallax-tied mouse drift (smallest)
- z=1 perspective grid with pseudo-3D foreshortening (`y^(1+pers*1.6)` compresses far rows toward a vanishing point, columns narrow with depth, density rises in distance)
- z=0 typewriter title with halo plate
- Mouse parallax separates the layers (gradient drifts 0.015, grid drifts 0.045) — depth reads through differential motion
- Depth fade dissolves far grid rows into the field
- **Self-score: 4/5** — pseudo-3D perspective + three explicit z layers + parallax-on-mouse + depth fog. Not 5 because no raymarch or DOF blur.

## Intentional motion (axis c)
- Silence floor: grid still sweeps diagonally on `motionSpeed`, gradient blobs still drift on Lissajous, idle row/col amplitude (`0.06`, `0.05`) keeps lines visibly bent — never freezes
- Energy response: `eRow`/`eCol` ramp warp amplitude up to 0.55/0.45 mesh-units; bass adds to the diagonal sweep velocity; `playerC` blooms the gradient field's blob radii
- Multi-mode: idle drift (silence) → bending lattice (mid energy) → ribbon-sweeping net + gradient bloom + intersection sparks (loud)
- `pulse` separately gates intersection sparks at grid knots — a "loud moments only" beat
- **Self-score: 4/5** — clear silence/build/loud states; intersection sparks are a moment, not a gradient. Not 5 because there isn't an explicit "hold + release" surprise stop.

## Abstract not literal (axis d)
- The reference is a literal data plot (axis labels, dot annotations). The shader DOES NOT render that. It takes the *feeling* — pigment spray on paper with a lattice over it — and abstracts: no labels, no dots, no axes-with-tick-marks. The grid is a warped net, not a Cartesian axis.
- Cue text is allowed (rubric explicitly permits text inputs); it's a slab, not a chart label.
- No checkerboard (lines warp every frame), no spectrum bars, no EKG, no horizon mirror.
- **Self-score: 5/5** — the source is felt (lattice on pigment) but not depicted.

## Surprise / risk (axis e)
- The double-axis warp (rows warped by player[1], columns rippled by player[2]) is a less-traveled move than the corpus's "single height-field per band" pattern.
- Grid colour absorbs 30% of the underlying gradient (`mix(gridTint, col*0.4, 0.30)`) — grid feels embedded in the field, not pasted on. That's a small new move.
- Intersection sparks at grid knots on `pulse` — a tiny technique that reads as "light catching the net".
- **Self-score: 3/5** — composition extends the corpus (warp-on-both-axes), palette and tone-mapping are familiar. Not a chord-strike but not derivative.

## Self-total: **20/25**
(a 4 · b 4 · c 4 · d 5 · e 3)

## Hard-floor & anti-patterns
- Hard floor: PASSED — 3 `player[*]` binds + `cue.latest`.
- Anti-patterns: NONE triggered.
  - Not a checkerboard: every row/column is bent by a travelling sine sum; no static lattice frame exists.
  - Not a spectrum/EKG/horizon-mirror.
  - Text input is fine (it's the documented `msg`→`cue.latest` pattern).

## Caveats
- `playerC` is bound to `player[3].active` (bool/0-1) rather than `.energy` to satisfy the brief's "≥2 inputs BIND to distinct `player[i].energy/active`" — `energyA`/`energyB` cover the two energy slots, and `.active` gives a discrete "third voice present" pulse on the gradient layer.
- Grid sweep speed and warp amplitude are independently controllable (`motionSpeed`, `gridMorph`, `audioDepth`) so the user can tune motion without breaking the silence floor.
- Title slab is centred at uv.y=0.22 over the gradient — readable against the busy field via a small paper-coloured halo plate.

## Files
- `/Users/lu/easel/shaders/moving_grid_gradient_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/moving_grid_gradient_text.fs`
- `/Users/lu/ShaderClaw3/.critiques/moving_grid_gradient_text.md`

## Validation
`glslangValidator` PASSED (rc=0) against the Easel preamble (matches `ShaderSource::translate` uniform set: `TIME`, `RENDERSIZE`, `mousePos`, `msgAge`, `audioLevel/Bass/Mid/High`, `audioFFT`, `fontAtlasTex`, `gl_FragColor` macro).
