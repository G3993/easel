# colorpantone_boxes_text — Pantone swatches as a faux-3D parallax stack

## Reference
`/Users/lu/Documents/A-List Shaders/colorfulpantonecolorboxes_text.jpg` —
a tilted, overlapping field of saturated rectangles (coral, teal, salmon,
plum, mustard, sky, pink) on a cool grey paper, intercut with chunky
display text fragments. The composition is *layered* — boxes slip
under and over each other and the text — not a flat grid; the rectangles
read as Pantone-style colour chips with editorial typography pasted on top.

## Concept

Three z-stacked planes (back / mid / front), each carrying a sparse
grid of tilted Pantone-style colour swatches. Each box is rendered as a
faux-3D extrude: the rotated top face is the chip; the two receding
edges are darkened to read as side-faces — fake 3D, no raymarch, but
the depth cue is genuine. Each plane scrolls at its own velocity and
direction, so cells pass each other in z while the camera holds still.
Per-cell tilt breathes with TIME + bass; a slow palette-swap lottery
re-rolls each cell's hue every few seconds, blending prev→next so the
canvas never settles.

Three players own three planes:
`player[1..3].energy` push their plane forward (pop), `player[i].active`
anchors a wandering "hot spot" onto a swatch. The live cue line types out
near the hottest swatch with a blinking caret — text reads like a
Pantone reference code glued to its chip, not a headline.

Palette modes: 0 Pantone, 1 Editorial, 2 Acid, 3 Mono+Pop. Bass jitters
the tilt; mid speeds palette swaps. The whole field is gallery-toned
with a tiny bloom and `fwidth` AA on every silhouette + side face.

## INPUTS & BIND

| input | bind | role |
|---|---|---|
| `msg` (text) | `cue.latest` | typewriter caption near the hot swatch |
| `energyA` | `player[1].energy` | back plane pop / hot-spot intensity |
| `energyB` | `player[2].energy` | mid plane pop / hot-spot intensity |
| `energyC` | `player[3].energy` | front plane pop / hot-spot intensity |
| `activeA` | `player[1].active` | anchors back-plane hot spot |
| `activeB` | `player[2].active` | anchors mid-plane hot spot |
| `activeC` | `player[3].active` | anchors front-plane hot spot |
| `bassDrive` | `audio.bass` | synchronized tilt shudder |
| `midDrive` | `audio.mid` | palette-swap acceleration |
| `cols/rows` (long) | manual | grid density per plane |
| `paletteMode` (long) | manual | 0 Pantone / 1 Editorial / 2 Acid / 3 Mono+Pop |
| `motionSpeed` | manual | global drift speed |
| `tiltAmount` | manual | per-cell rotation amplitude |
| `audioDepth` | manual | how strongly audio drives tilt |
| `swapRate` | manual | palette lottery frequency |
| `labelScale` / `kerning` | manual | caption metrics |
| `paperColor` / `inkColor` | manual | background + text ink |

Binding floor: 9 channel binds (1 `cue.*` + 3 `player[i].energy` + 3
`player[i].active` + 2 `audio.*`). **Hard floor passed.**

## 5-axis self-score (RUBRIC.md v2)

| axis | score | rationale |
|---|---|---|
| **a. Multi-player separability** | 4/5 | Three players own three planes with distinct depth/scale/parallax envelopes and independent hot spots; muting one player obviously stops one plane's pop and freezes its hot-spot wander. Not 5 because all three planes share the same swatch primitive — visual *languages* aren't fully different beyond scale and parallax depth. |
| **b. Depth & dimensionality** | 4/5 | Genuine z-layering: 3 planes with independent drift velocities, per-cell tilt with 2D-faking-3D side-faces, plane-dependent atmospheric haze, and pop that scales boxes when a player goes hot. Not raymarched, so capped at 4 by the rubric. |
| **c. Intentional motion** | 4/5 | Constant motion every frame (per-plane drift + per-cell tilt breathing + palette swap interpolation). Energy turns calm drift into pop + jitter; the palette lottery introduces compositional moments every few seconds; the typewriter caret marks micro-moments. Not 5 because there are no surprise stops/holds. |
| **d. Abstract not literal** | 4/5 | The reference is colour swatches and text — this represents the *idea* of editorial colour-system layouts (swatch-as-field, code-as-label) without printing literal Pantone numbers or anything resembling spectrum bars / EKG / icons. Glyphs come from `cue.latest`, used as a Pantone-code-style annotation, not as the central headline. Not 5 because boxes-with-labels remain a recognizable referent. |
| **e. Surprise / risk** | 4/5 | The corpus has no faux-3D-extrude swatch stacks: the side-face shading + per-cell tilt + palette-lottery swap-blend is a new authoring move here, and combining it with a typed caption glued to a *wandering hottest swatch* is novel. Not 5 because the swatch-grid trope itself is established in design culture. |
| **total** | **20/25** | |

## Anti-pattern checklist

- EKG line? **no** — no canvas-spanning sine.
- Spectrum bars? **no** — audio drives tilt + swap rate, never bar height.
- Literal icons? **no** — no pre-baked symbols; only procedural box SDFs and font-atlas glyphs from `cue.latest`.
- Default checkerboard? **no** — cells are tilted, scrolling, palette-swapping, never a 2-tone grid; three planes overlap continuously.
- Mirror-symmetric horizon? **no** — three planes drift asymmetrically; no horizon line.
- Logo / readable text as central visual? **no** — caption is small, glued to a moving hot swatch on the front plane, typewriter-revealed; the swatches are the subject.
- Single-color noise plane? **no** — no fbm fill; noise only modulates ≤ 4% paper tooth.

## Next iteration ideas

1. **Per-plane visual language.** Right now all three planes use the same tilted-extrude swatch. Give the back plane *flat un-extruded chips*, the mid plane the tilted extrude, the front plane *foiled* swatches with a slow sheen sweep — would push axis (a) to 5.
2. **DOF blur between planes.** A cheap separable blur on the back plane gated by `energyA` (focal player) would push (b) to 5 and read as a real photographed studio shot.
3. **Compositional drop moment.** Detect any player > 0.7 onset, briefly freeze the drift on the other two planes for ~250ms while the hot plane re-rolls its palette — gives motion (c) its "moments not gradients" upgrade.
4. **Pantone-code generator.** When `msg` is empty, render a procedural `PMS NNNN-NN` style code per hot swatch — fills the silence with credible Pantone-flavoured text without needing live cue.
5. **Side-face gradient.** Side-faces are flat darken; gradient them from the top-edge hue down to a deeper shade for a more chip-like finish.

## Files written

- `/Users/lu/easel/shaders/colorpantone_boxes_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/colorpantone_boxes_text.fs`
- `/Users/lu/ShaderClaw3/.critiques/colorpantone_boxes_text.md`

## Validation

`glslangValidator -S frag` on the Easel-preamble harness (matches
`ShaderSource::translateFragment`):
**RC = 0**, no stdout, no stderr.
