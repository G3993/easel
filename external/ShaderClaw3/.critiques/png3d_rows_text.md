# png3d_rows_text — A-List drop critique

## Reference
`/Users/lu/Documents/A-List Shaders/3D_pngs_rows_text.jpg` — editorial
"Creative Concept" page on warm near-white paper: a thin arrow rule at top,
then several rows interleaving parenthesized words ("(classical)", "(magic",
"(and", "texture") with monochrome 3D PNG cutouts — a classical bust, an
axonometric cube, an Ionic column, a marble triangle fragment, and a
wood-grain rectangle. Calm, gallery-grade, type-as-hero with objects as
punctuation. Bottom hairline rule + "Creative Concept" tag.

## Concept
**Rows of cutout objects + parenthesized words drifting in parallax.** The
canvas is a horizontally-banded diorama: N rows (3–6, default 4), each
hosting `itemsPerRow` cells (3–8, default 5). Every cell deterministically
chooses to render either:

- a **procedural PNG-like cutout** (one of 8 archetypes: bust, cube, column,
  sphere, triangle, slab, ring, prism), each with a per-archetype
  "fake-3D" shading pass (axonometric face shading for the cube, fluting
  for the column, NE highlight for the sphere, wood grain for the slab,
  marbled fbm for the triangle, concentric bands for the ring, vertical
  stripes for the prism, cheek/neck highlights for the bust), OR
- a `(WORD)` chunk drawn from the live `msg` text, with parens rendered as
  procedural SDF arcs (so the font atlas's missing-paren slots don't blank
  out) and the alphanumerics through the existing `fontAtlasTex`.

Rows drift left at depth-dependent speeds (back rows slower → real
parallax). Each row casts a soft offset shadow onto the rows BEHIND it
(back-to-front compositing), with shadow offset + blur scaling by depth
and by `audioBass`. The header arrow rule and footer hairline + ink dot
echo the reference's editorial chrome.

Typewriter: `msg` reveals one char at a time via `msgAge` (auto-bound to
`cue.latest`), so cells appear to write themselves in sequence as speech
arrives — silence keeps prior cells visible, no live transcript = full
message preview.

## INPUT bindings (channel contract)
| INPUT | BIND | Role |
|---|---|---|
| `msg` | `cue.latest` | typewriter text; revealed by `msgAge` |
| `energyA` | `player[1].energy` | jitter on row 0 (cell offsets + size pulse) |
| `energyB` | `player[2].energy` | vertical breath on row 1 |
| `energyC` | `player[3].energy` | slow vertical drift on row 2 |
| `audioDepth` | `audio.bass` | widens parallax horizontal separation, lifts shadow |
| `cueLevel` | `audio.level` | overall pulse, modulates wood-knot glow |

Six bound channels, three distinct `player[i]` axes, one cue stream, two
distinct `audio.*` bands. Muting `player[1]` visibly freezes row 0's
twitchy jitter while rows 1–2 keep breathing; muting `player[2]` stills
the vertical swell of row 1 specifically; muting bass collapses the
parallax separation and softens shadows.

## Style INPUTS (non-bound)
- `rowCount` 3/4/5/6 — diorama depth count
- `itemsPerRow` 3..8 — cell density per row
- `palette` 5 sets: Paper/Graphite (default), Cream/Indigo, Linen/Oxblood,
  Bone/Forest, Onyx/Sulphur (dark palette for high-contrast venues)
- `motionSpeed` 0..3 — global clock multiplier
- `shadowDepth` 0..2 — drop-shadow strength
- `transparentBg` — alpha-mask the paper for compositing

## Real depth
- Parallax: per-row drift speed = `mix(0.05, 0.32, depth01) * motionSpeed`
  plus a bass-driven horizontal separation term — distinct, not a uniform
  multiplier.
- Cutouts cast **offset soft shadows** computed by re-evaluating the same
  SDF at `(cellLocal - vec2(shOff, -shOff)) / scale` with a `shBlur`-
  inflated fwidth — true SDF shadow shape, not a blurred sprite.
- Back-to-front compositing means shadows from front rows land on top of
  the painted back rows + paper, not on neighbours in the same row.
- Per-archetype interior shading (axonometric faces, NE-lit sphere normal,
  marble fbm, wood grain) suggests volume; cube actually computes three
  diagonal-split faces so it reads as a 3D box, not a flat icon.

## Motion contract
- Idle (silence, all energies = 0): rows still drift on their depth-tied
  parallax clock; each cell has its own slow vertical bob (per-seed phase)
  so the canvas breathes even at full quiet — stillness reads as
  intentional, not as "shader stopped".
- Player A energy → row 0 jitter (sub-cell translation) + size pulse on a
  4 Hz carrier; muting A makes row 0 visibly settle.
- Player B → row 1 vertical breath (1.6 Hz cosine).
- Player C → row 2 slow vertical drift (0.7 Hz).
- Bass → widens parallax separation + thickens shadow blur.
- Audio level → modulates wood-knot glow on the slab archetype.
- The typewriter reveal is an *intentional* arrival event — words appear
  one glyph at a time when `cue.latest` fires, holds when silence returns.

## Anti-pattern checks
- No spectrum bars, no waveform, no checkerboard, no horizon scene, no
  mirror-symmetric composition.
- No literal logos rendered as decoration — the only glyphs are the live
  `msg` chunks (allowed text content), parenthesized to match the
  editorial reference.
- Per-row binding is distinct (axis a): rows 0/1/2 are *separable* by
  channel mute, not all bound to `audio.level`.

## Rubric self-score /25

- **a. Multi-player separability — 4/5** — 3 player rows + 2 audio
  bands, each with a visually distinct role (jitter / breath / drift /
  parallax-width / wood-knot glow). One point reserved: rows 4–5 share
  player energies, so muting one doesn't read as cleanly past row 2.
- **b. Depth & dimensionality — 4/5** — multi-layer parallax with
  back-to-front shadows landing on planes behind, per-archetype faked-3D
  shading (cube has real axonometric face split, sphere has 3D-normal
  lighting). Not raymarched, hence not a 5.
- **c. Intentional motion — 4/5** — distinct silence / low / high
  states; bass arrival snaps the parallax wider as a felt moment, the
  typewriter is a *composed* arrival event. Holds at silence rather than
  fading to zero motion. One held back: no true "drop" mode beyond the
  bass widen.
- **d. Abstract not literal — 4/5** — cutouts are stylized archetypes
  (bust silhouette, axonometric cube), not photographs; parens-words are
  the literal content but they ARE the subject, mirroring the editorial
  reference's text-as-hero treatment. Not pure abstraction.
- **e. Surprise / risk — 4/5** — the combination of typewriter parens
  text *interleaved* with PNG-cutout archetypes on parallax rows is a new
  authoring move in this corpus; cube's axonometric face split done as
  diagonal half-plane tests inside an SDF cell is a small technical
  trick.

**Total: 20/25** — hard floor clearly passed (6 bound channels).

## Caveats
- Atlas fallback for parens: standard ShaderClaw font atlas only holds
  slots 0..36 (A-Z + digits + space). Indices 27/28 in atlas would normally
  blank; this shader paints procedural SDF arcs at those slots, so parens
  always render. If the host's atlas DOES carry paren glyphs at those
  slots they'd composite on top — visually similar, no harm.
- Word reuse: when `wordsAvail` is fewer than text cells, the same word
  may appear in multiple cells (intentional — it echoes the reference's
  repeated bracketed words).
- Validated under `#version 330 core` via local glslang with the Easel
  preamble simulated (built-ins, font atlas, audio uniforms, msg_*).
  EXIT 0, clean compile, zero warnings.
- No `.easel` project edits, no app relaunch, no commits — shader is in
  place at `/Users/lu/easel/shaders/png3d_rows_text.fs` and the bundle
  `Easel.app/Contents/Resources/shaders/`.
