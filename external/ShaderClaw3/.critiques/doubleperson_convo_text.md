# doubleperson_convo_text

**Reference image:** `/Users/lu/Documents/A-List Shaders/doublepersonconvo_text.jpg`
(Two offset vertical gradient pillars — slate-teal lower-left, peach upper-right — traversed by ink arc(s) and ink-dot punctuation. Two captions in different cells. Asymmetric. Editorial.)

## Concept
Two minds in conversation rendered as **two asymmetric gradient pillars** (round-rect SDF bodies), each bound to its own `player[i]` channel. The active speaker swells, brightens, and pulls the typewriter caption to its side; an **ink arc** traces between the pillars, modulated by who's talking and flowing A→B or B→A depending on `turn = sign(energyB - energyA)`. **Punctuation orbs** drift through a back z-layer with parallax gravity toward the active pillar. **Asymmetric by construction** — A is taller/slimmer/lower; B is shorter/wider/higher; horizontal offsets are deliberately non-mirrored (`xA = -sep·aspect·0.55`, `xB = +sep·aspect·0.60`). No portraits, no waveforms, no spectrum bars, no mirror symmetry.

## Bindings (intelligence-layer channels)
| Input | Channel | Role |
|---|---|---|
| `msg` | `cue.latest` | typewriter caption (msgAge reveal) |
| `energyA` | `player[1].energy` | pillar A swell + brightness + arc bias |
| `energyB` | `player[2].energy` | pillar B swell + brightness + arc bias |
| `activeA` | `player[1].active` | pre-energy presence boost for A |
| `activeB` | `player[2].active` | pre-energy presence boost for B |
| `pitchA` | `player[1].pitch` | hue warm-shift on pillar A |
| `pitchB` | `player[2].pitch` | hue warm-shift on pillar B |
| `bassDrive` | `audio.bass` | orb jitter + foreground specks |

Style INPUTS (no BIND): `paperColor`, `colorA`, `colorB`, `inkColor`, `separation`, `flowSpeed`, `audioDepth`, `textScale`, `kerning`, `glow`, `restEnergy`.

## glslang validation
`glslangValidator -S frag` against the Easel preamble harness (TIME, RENDERSIZE, msgAge, fontAtlasTex, audioFFT, audioBass/Mid/High, mp* landmark samplers, msg_0..msg_47 + msg_len, all declared INPUTS). **Exit 0, zero warnings, zero errors.**

## 5-axis self-score /25

| Axis | Score | Rationale |
|---|---|---|
| **a. Multi-player separability** | **5** | Two pillars with distinct visual languages (height, width, x-offset, hue, gradient direction, stem length); muting `player[1].energy` instantly collapses A's swell, dims its hue to 70%, and reverses arc flow + caption side. 6 distinct `player[*]` binds across 2 channels (energy/active/pitch ×2) + arc direction. |
| **b. Depth & dimensionality** | **4** | Four discrete z-layers: paper grain → 7 drifting punctuation orbs with parallax → pillar bodies with stems (cast-shadow depth cue) → ink arc (12-segment capsule chain with taper) → typewriter caption → foreground specks. Not raymarched, but composed-z is legible. |
| **c. Intentional motion** | **4** | Three motion modes: **rest** (both quiet → pillars hold at 70% brightness, arc gently breathes, orbs drift), **single speaker** (one pillar swells/brightens, arc flows toward listener, caption pulls), **handoff** (mid-`turn` arc is multi-crossing, both pulse). Silence is composed, not absent. |
| **d. Abstract not literal** | **5** | Two gradient pillars + an arc + punctuation orbs. No faces, no speech bubbles, no waveform, no spectrum bars. The "conversation" is felt via alternation of brightness and arc direction. |
| **e. Surprise / risk** | **4** | Composition risk: deliberate asymmetric placement (different x, y, w, h per pillar), 12-capsule polyline arc as a *trace* not a waveform, with tapering width + arrowhead terminus and tinted halo. Color extraction (slate-teal + peach over warm paper) matches the reference's editorial tonality. |

**Total: 22/25.**

## Anti-pattern check
- Literal portrait/avatar — **NO** (two abstract round-rect pillars).
- Speech-bubble emoji — **NO**.
- EKG sound-wave line — **NO** (arc crosses center spine multiple times, tapers, has direction).
- Spectrum-analyzer bars — **NO**.
- Mirror-symmetric — **NO** (asymmetric by construction: different pillar widths, heights, x offsets, vertical center offsets, stem lengths).
- Default checkerboard / single-color noise plane — **NO**.
- Logo/readable text as central visual — **NO** (caption is editorial-scale, beside the active pillar, never centered).

Hard floor: passes (8 channel binds, well above the `audio.*`-only line).

## Next iteration ideas
- Replace the 12-capsule arc with a true cubic Bezier traced via signed distance to the curve (better tangent continuity at high `flowSpeed`).
- When *both* players are simultaneously hot (`min(eA,eB) > 0.5`), bloom a third "shared" arc bridging mid-air — explicit "overlap" composition event.
- Bind `cue.coach.severity` to push paper toward charcoal during high-stakes exchanges (would need a new DataBus channel surfaced).
- Pitch could drive sub-divisions of the arc (higher pitch → more crossings) for more semantic decomposition.

## Files
- `/Users/lu/easel/shaders/doubleperson_convo_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/doubleperson_convo_text.fs`
- `/Users/lu/ShaderClaw3/.critiques/doubleperson_convo_text.md` (this file)
