# grid_text — Swiss broadside as a parallax z-stack

## Reference
`/Users/lu/Documents/A-List Shaders/grid_text.jpg` — four typographic
posters in a 2×2 layout. Top-left: black `i`s on off-white. Top-right: a
loose constellation of lowercase letters (`s o c a u a n e`) on azure.
Bottom-left: lowercase letters on ink-green. Bottom-right: numerals on
mandarin. Every poster is *sparse* — single glyphs at low density on a
slightly irregular grid, with the kind of restrained Swiss-modernist
composition that earns its negative space.

## Concept

A four-panel poster sheet rendered as a depth-parallaxed page rather than
a static four-up. Three independent depth planes (back, mid, front)
populate a sparse glyph grid; each plane drifts at its own velocity so
characters cross past each other in z while the camera dollies. Quadrant
colours and ink come from the reference palette (off-white / azure /
ink-green / mandarin), with continuous gutter bleed so the panels read as
one folded sheet — not a checkerboard.

Each of the four quadrants is its own player:
`player[1..4].energy` modulate glyph jitter, swell, and a soft inward
glow in that quadrant. Silent quadrants collapse to a calm constellation
of single letters; loud quadrants stretch + pop forward on the front
plane. Loud lows pull the back plane down out of plane.

Text comes from `cue.latest` (auto-bound to `msg`). The typewriter
reveal animates via `msgAge` × 28 cps — newly-revealed slots ripple
across all three depth planes (each plane samples slots on a slow
rotation, so the same incoming character can briefly appear front, mid,
and back at different scales). A small blinking caret marks the live
write head.

Variants:
- **Broadside** (default) — letters, full alphabet.
- **Numerals** — remaps letters to digits (mandarin-poster reference).
- **Drift** — extra sparsity holes; the page feels more empty.

## INPUTS & BIND

| input | bind | role |
|---|---|---|
| `msg` (text) | `cue.latest` | typewriter content; revealed via `msgAge` |
| `energyA` | `player[1].energy` | top-left quadrant jitter / glow |
| `energyB` | `player[2].energy` | top-right quadrant jitter / glow |
| `energyC` | `player[3].energy` | bottom-left quadrant jitter / glow |
| `energyD` | `player[4].energy` | bottom-right quadrant jitter / glow |
| `audioDepth` (float) | manual | scales bass-driven plane separation |
| `gridDensity` (float) | manual | mid-plane cells/width |
| `textSize` (float) | manual | global glyph scale |
| `cameraDrift` (float) | manual | camera dolly + sheet warp |
| `parallax` (float) | manual | per-plane drift amplitude |
| `motionSpeed` (float) | manual | global time multiplier |
| `sparsity` (float) | manual | per-cell glyph drop probability |
| `variant` (long) | manual | 0 Broadside / 1 Numerals / 2 Drift |
| `paperA..D` (color) | manual | quadrant paper colours |
| `inkLight`, `inkDark` (color) | manual | dual ink palette (luma-picked) |

Audio also reads `audioBass` (depth push) and `audioMid` (paper grain).

Binding floor: 5 channel binds (1 `cue.*` + 4 `player[i].energy`).
Hard floor passed.

## 5-axis self-score (RUBRIC.md v2)

| axis | score | rationale |
|---|---|---|
| **a. Multi-player separability** | 4/5 | Four independent player binds, each owns a quadrant with distinct paper + glow + glyph-jitter response. A/B muting any player would visibly silence one corner. Not a 5 because the *visual languages* between quadrants are colour-different but motion-similar (same jitter formula, just per-quadrant energy gate). |
| **b. Depth & dimensionality** | 4/5 | Three z-stacked planes with independent parallax velocities and scale, plus camera dolly, mouse parallax, sheet warp, and audio-bass out-of-plane displacement. Not raymarched, so capped at 4 — the rubric reserves 5 for fully spatial DOF/fog reads. |
| **c. Intentional motion** | 4/5 | Camera dolly + per-plane drift gives continuous motion every frame; player energy turns silent-quadrant calm into loud-quadrant jitter (distinct quiet vs. loud). Typewriter caret + msgAge-driven reveal adds compositional moments. Not a 5 because there are no surprise stops/holds. |
| **d. Abstract not literal** | 4/5 | Glyphs are present (the reference *is* typography), but they are deployed as a compositional field — sparse, parallaxed, never spelling the message in a single readable run. The Swiss-poster *feeling* of negative space + grid is the subject, not legibility. Not 5 because actual glyphs from `msg` do render and a careful viewer can read fragments. |
| **e. Surprise / risk** | 4/5 | The corpus has text shaders (`text_clusters`, `text_bricks`, `pixel_type_text`) but none treat the page as a *physical broadside with 4 quadrants and z-stacked depth planes that share one message*. The continuous gutter (not hard panels) + per-quadrant player binding is a new authoring move. Not 5 because the glyph-on-grid template is established, even if this composition is new. |
| **total** | **20/25** | |

## Anti-pattern checklist

- EKG line? **no** — no canvas-spanning sine.
- Spectrum bars? **no** — audio drives depth/grain, never bar height.
- Literal icons? **no** — glyphs come from `cue.latest`, not pre-baked symbols.
- Default checkerboard? **no** — gutter is continuous bleed; midlines wobble; 4 quadrants are coloured, not a 2-tone grid.
- Mirror-symmetric horizon? **no** — there is no horizon; midlines are non-static; quadrants are colour-asymmetric.
- Logo / readable text as central visual? **no** — text is fragmented across 3 depth planes and sparse cells; the message is a *field*, not a headline.
- Single-color noise plane? **no** — noise only modulates paper tooth ≤ 5%.

## Next iteration ideas

1. **Per-quadrant motion language.** Right now jitter is the same formula scaled by per-quadrant energy. Give each quadrant a *different* response: TL drifts vertically, TR rotates, BL pulses scale, BR cascades reveal direction. Would push axis (a) to 5.
2. **DOF blur between planes.** A cheap separable blur on the back plane based on `parallax` would push axis (b) to 5 and read as a real photographed broadside.
3. **Compositional moments.** Detect a high-energy onset (any player > 0.7) and freeze the camera for ~250ms before a small zoom — gives motion (c) its "moments not gradients" upgrade.
4. **Numeral-only variant could swap to a 7-segment-ish atlas** (still abstract) — would feel even more poster-like for the mandarin reference.
5. **Cue-coach headline mode.** When `cue.coach.headline` is bound, render that single string at giant scale across the whole sheet for ~1s as a "drop" moment.

## Files written

- `/Users/lu/easel/shaders/grid_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/grid_text.fs`
- `/Users/lu/ShaderClaw3/.critiques/grid_text.md`

## Validation

`glslangValidator -S frag` on the Easel-preamble harness:
**RC = 0**, no stdout, no stderr.
