# moving_circular_grid_text — Critique

## Concept
A drifting matrix of circular research cells (10 distinct treatments: concentric
rings, halftone, gradient lens, eclipse crescent, ring-arc, dot mandala, scan-line
disc, gear rotor, ripple corona, vesica chevron), scrambled by `randomSeed`. Cells
randomly *pop* (burst-enlarge + flash) on per-cell phases that quicken with player
energy. Three back→front parallax bands (back/mid/front), one owned by each player
(A/B/C). A single elected front-band cell hosts the live cue caption as a typewriter
slab, never the center. Editorial paper palette inspired by the reference image.

## INPUTS (with BIND)
- `msg` (text, MAX_LENGTH 48) — **BIND `cue.latest`** (typewriter via `msgAge`)
- `energyA/B/C` (float) — **BIND `player[1..3].energy`** (drive band drift speed + pop cadence + brightness lift)
- `activeA/B` (float) — **BIND `player[1..2].active`** (gate band activity)
- `bassDrive` (float) — **BIND `audio.bass`** (stroke weight `sw` across every cell variant)
- `highDrive` (float) — **BIND `audio.high`** (AA crispness + pop gating in silence)
- `cellCount`, `randomSeed`, `palette`, `motionSpeed`, `audioDepth`, `parallax`,
  `popAmount`, `captionScale`, `kerning`, `paperColor`, `inkColor` — style INPUTS.

≥3 controllable elements: 10 cell variants, 3 bands, pop cadence, palette, parallax.
≥2 INPUTS bound to distinct `player[i].energy/active`: yes (`player[1..3].energy` +
`player[1..2].active` = 5 distinct player binds). One bound to `cue.latest`: `msg`.
One bound to `audio.*`: `audio.bass`, `audio.high`.

## glslang
Validated against synthesized Easel preamble (`#version 330 core`, msg_0..47,
fontAtlasTex, audioFFT, RENDERSIZE, msgAge, all uniforms). `glslangValidator -S frag`
exits 0, no warnings.

Also compiled live: `./build/test_shaders shaders` → `[PASS] moving_circular_grid_text.fs (19 inputs)`.

## Rubric self-score (/25)

| Axis | Score | Rationale |
|------|-------|-----------|
| a. Multi-player separability | 5/5 | Three depth bands, each owned by `player[i].energy`+`active`; muting one band kills its parallax + pop cadence + brightness lift. Five distinct player binds. |
| b. Depth & dimensionality | 4/5 | Three parallax bands, each at its own drift speed + cell-grid frequency + haze. Pop scale acts as forward push. Not raymarched, but cleanly layered z. |
| c. Intentional motion | 4/5 | Idle drift is very slow (≈0.04); per-cell pops are short bursts gated by energy, so silence reads as held composition and crescendos arrive as rhythmic bursts. Pop period contracts 6s→2s with energy. |
| d. Abstract not literal | 5/5 | No literal depiction — abstract typographic/research-plate composition. Caption uses cue text legitimately; no logos or readable-text-as-decoration. |
| e. Surprise / risk | 4/5 | The "elected pop-cell hosts the caption" mechanic + 10-variant scrambled-by-seed cell library is a fresh authoring move; reference's organic noodle aesthetic is reinterpreted as discrete cell pops. |
| **Total** | **22/25** | Hard floor passed (multiple player/cue/audio binds). No anti-patterns. |

## Files
- `/Users/lu/easel/shaders/moving_circular_grid_text.fs` (canonical source)
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/moving_circular_grid_text.fs` (bundle copy)

## Caveats
- The dot-mandala variant computes `md` over a 12-iter loop; for very dense grids
  (cellCount=8 → 64 front-band cells × 12 dots) shader cost rises but stays well
  within real-time on Apple Silicon at 1080p.
- Variant id is scrambled by `floor(randomSeed)`; integer seed steps reshuffle the
  whole grid cleanly.
- Reference image emphasised rounded-rectangle "noodle" tracks; this shader takes
  the *grid + random pop + text-in-cell* essence rather than literal track shapes,
  consistent with rubric axis (d) abstract-not-literal.
- No `.easel` edits, no relaunch, no commits — all per instructions.
