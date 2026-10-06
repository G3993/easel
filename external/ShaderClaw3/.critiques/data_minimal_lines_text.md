# data_minimal_lines_text — critique

## Concept
A typographic data score on warm paper, in the spirit of Cage's "Lecture on
Nothing": generous whitespace, scattered hairlines, asemic glyph clusters
(brackets, deltas, dots, ticks), and sparse solid black bars. The live cue
lays in as the central spoken line, typewriter-revealed with a thin growing
underline and blinking caret. Three quiet agents share the page — each
mark-type owns a player channel and visibly thins when its agent is muted.
Restraint is the move: at silence the page is nearly still; with energy
and audio, lines drift, glyph clusters wake up, and bars elongate.

## INPUTS / BIND
- `msg` (text) → **cue.latest**  (typewriter via msgAge·CPS=28)
- `energyA` (float) → **player[1].energy**  (hairlines count + drift)
- `energyB` (float) → **player[2].energy**  (asemic clusters wake/fade)
- `energyC` (float) → **player[3].energy**  (bar visibility + length)
- `aliveA/B/C` (float) → **player[1..3].active**  (hard mute per agent)
- `drift` (float) → **audio.level**  (global drift / pulse contrast)
- Style controls: `lineDensity`, `shapeCount` (3..12), `palette` (4 options:
  Ivory Score / Slate / Vellum / Inverse), `motionSpeed`, `audioDepth`, `grain`

## glslang
PASS — `build/test_shaders shaders` reports
`[PASS] data_minimal_lines_text.fs (14 inputs)`.

## Rubric (self-score, /25)
- **a. Multi-player separability — 5/5.** Three independent player channels
  drive three visually distinct mark languages (hairlines vs glyphs vs bars).
  Each `aliveX` flag is a hard mute; muting one strips that mark-type from
  the page, the other two remain. The composition reads as composed of three
  voices, not one signal smeared three ways.
- **b. Depth & dimensionality — 3/5.** Layered z (far/near bar layer with
  parallax-rate drift + opacity falloff), depth-staggered hairline ranks,
  paper marbling + edge vignette. Not raymarched — fits the flat-page brief.
- **c. Intentional motion — 4/5.** Silence reads as stillness (motion floor
  is a barely-perceptible gallery sheen + slow agent drift). Energy gates
  bring agents online progressively; bass elongates near-layer bars; treble
  nudges glyph size. The page composes in time rather than idling.
- **d. Abstract not literal — 5/5.** No spectrum bars, no EKG, no chart, no
  literal pictogram. The image is the *feeling* of "data being notated."
  Bars are data marks, not bars-as-EQ — they don't grow vertically from a
  baseline and they sit at random angles/positions.
- **e. Surprise / risk — 4/5.** Asemic micro-glyph SDF set (delta, brackets,
  ticks, double-dot, dash) is a novel authoring move in this corpus; the
  Cage-score reference and the growing underline + caret keep the page
  reading as living typography rather than decoration.

**Self-total: 21/25.** Anti-patterns: none triggered. Hard floor passed
(seven `player[*]` binds + one `cue.latest`).

## Files
- `/Users/lu/easel/shaders/data_minimal_lines_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/data_minimal_lines_text.fs`
- `/Users/lu/ShaderClaw3/.critiques/data_minimal_lines_text.md`

## Caveats / follow-ups
- Asemic glyphs are SDF-rendered (not the font atlas), so palette/contrast
  reads the same across all atlases — intentional.
- `aliveX` channels are 0..1 floats here (per intelligence-layer "bool / 0..1");
  binding popup will treat them as `player[i].active`.
- No `.easel` edits, no app relaunch, no commits — pure shader addition.
- If a future review wants more "Cage page" feel, consider adding a sparse
  ellipsis row near the lower third — currently the glyph clusters cover it.
