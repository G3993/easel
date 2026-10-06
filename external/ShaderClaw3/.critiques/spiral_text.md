# spiral_text — Critique

**Reference**: `/Users/lu/Documents/A-List Shaders/spiral_text.jpg`
A pencil/pen study where the word "SPIRALE" / "SP" / "R" repeats and coils into
itself — letters dissolve into their own scribbled spiral motifs, the page reads
as a single typographic vortex without a true subject in the center.

## Concept

A logarithmic-spiral typewriter. The live `cue.latest` message is replicated
along **N counter-rotating log-spiral arms** (`r = a·exp(b·θ)`), each owned by a
distinct player. Arms write outward from a hyperbolic-smoothed "eye"; glyphs
rotate **tangent to the curve** (per-pixel local frame from spiral pitch
`atan(1/b)`), shrink with `θ` toward the eye (real perspective depth), and
fade into the paper as they recede. `audio.bass` zooms the camera *into* the
vortex; per-player `energy` thickens its arm, lifts its glyphs off the page,
and adds a soft halo along the curve. Typewriter reveals new glyphs at the
rim as the speaker speaks — older glyphs spiral inward.

**Surprise mechanic**: spiral arms aren't drawn as a curve with glyphs pasted
on — they are solved analytically per-pixel. For each pixel the shader
inverts the spiral (`θ = log(r/a)/b`), computes the nearest glyph slot `k`
along the arm, places it tangent, and tests glyph coverage with
`fwidth`-AA. No buffer, no march, no curve sampling — pure closed-form
typography on a curve.

## INPUTS / Channel Binds

| INPUT | TYPE | BIND |
|---|---|---|
| `msg` | text(48) | `cue.latest` (auto-bound by Application::loadShader) |
| `energyA` | float | `player[1].energy` |
| `energyB` | float | `player[2].energy` |
| `energyC` | float | `player[3].energy` |
| `bassPull` | float | `audio.bass` |
| `armCount` | long(1..6) | manual |
| `coilRate` | float | manual (log-spiral b) |
| `turns` | float | manual |
| `spinSpeed` | float | manual |
| `counterRotate` | float | manual |
| `textAlong` | float | manual (along-curve vs cross-axis) |
| `glyphSize`, `glyphSpacing` | float | manual |
| `depthAmp` | float | manual (eye glow / depth boost) |
| `audioDepth` | float | manual (bass→z amount) |
| `paperColor`, `inkA/B/C` | color | manual palette |

Binding floor: **3× `player[i].energy` + 1× `audio.bass` + 1× `cue.latest`** —
meets and exceeds the intelligence-layer contract.

## 5-Axis Rubric (self-score / 25)

| Axis | Score | Rationale |
|---|---|---|
| **(a) Multi-player separability** | **5** | Three arms, three distinct `player[i].energy` binds, each with its own ink color, rotation direction, phase offset, and message-index shift. Mute one → its arm goes pale and stops swelling; visibly identifiable. |
| **(b) Depth & dimensionality** | **4** | True pseudo-3D: glyphs scale by `1 - θ/θmax` (linear z), atmospheric perspective blends ink → paper toward the eye, hyperbolic eye-warp opens the center, bass zooms the camera Z. Not raymarched but unambiguously *space*. |
| **(c) Intentional motion** | **4** | Energy-aware swell + halo per arm, bass-driven camera zoom, counter-rotation, slow rake pass for sparse text. Silence reads as a calm static spiral (intentional stillness); a bass hit feels like falling in. |
| **(d) Abstract not literal** | **5** | Not a logo, not readable text as decoration — it's a typographic vortex where the message becomes a curve and the curve becomes the image. Mirrors the reference's intent (typography as field, not subject). |
| **(e) Surprise / risk** | **5** | Analytic per-pixel spiral inversion + tangent-frame glyph rendering is a corpus-new authoring move (existing text shaders use grid layouts or bubble metaballs). Counter-rotating multi-arm typewriter on a log-spiral is not in `color_world.fs`, `clusters`, or any A-List shader I read. |

**Total: 23 / 25**

## Anti-pattern checklist — NO

- [x] **NO** EKG / sound-wave line
- [x] **NO** spectrum-analyzer bars
- [x] **NO** literal icons (no soccer ball, scoreboard, etc.)
- [x] **NO** default checkerboard
- [x] **NO** single-color noise plane (composition is the spiral)
- [x] **NO** mirror-symmetric horizon scene
- [x] **NO** central readable-logo glyph (text *is* the composition, distributed; matches RUBRIC's allowance for cue-text inputs)

## Validation

`glslangValidator` against Easel preamble harness: **exit 0**, no warnings.

## Next iteration

- Add a `data.spiralPitch` BIND so a future MIDI feed can morph the coil rate live.
- Try Fibonacci-spaced glyph slots (golden-angle increments) as an `armLayout` mode for an even more organic look.
- Per-arm color noise so vermillion/indigo arms get tiny chromatic shifts at high energy (live feel).
- Optional radial-only mode (`textAlong=0`) currently works; add a smooth-blend animation between along and across for performance moments.

## Files

- `/Users/lu/easel/shaders/spiral_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/spiral_text.fs`
- `/Users/lu/ShaderClaw3/.critiques/spiral_text.md` (this file)
