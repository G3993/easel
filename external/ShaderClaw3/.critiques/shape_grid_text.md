# shape_grid_text — critique

## Concept

Editorial contact-sheet plate. R×C grid (default 4×4), each cell hosts a
distinct SDF specimen drawn from a 12-variant library: square, triangle,
diamond, cross, star, halfmoon (vesica), arc bracket, hexagon, chevron,
capsule, skewed blade, lens. Variant assignment is hash-deterministic per
cell index, so the field reads as designed-on-paper — variety, not
checkerboard.

Hairline rules frame every cell; corner numerals (1..N×N) inset by
deterministic corner choice mirror the reference's editorial numbering. A
black caption slab at the bottom holds the typewriter caption (cue.latest
bound). The slab horizontally anchors under the **loudest player's
brightest cell** when audio is live; centred otherwise.

Three players each own a diagonal-band of the grid (Back / Mid / Front).
Their band's cells pop forward on energy: scale up, shear toward the
centre as pseudo-3D tilt, swap to a slightly darker/redder ink wash, and
spin faster. Bass thickens hairlines; mid drives uniform shape rotation;
high crisps the SDF antialias (sharper fwidth band).

Reference evoked: yellow hi-vis page, black bordered rectangular panels,
diverse shape cutouts, small numerals at corners, bold black inverted
caption block. Palette mode 0 is the explicit yellow match.

## INPUTS with BIND

- `msg` (text) → `cue.latest` — typewriter caption in bottom slab
- `energyA` → `player[1].energy` — Back band cells
- `energyB` → `player[2].energy` — Mid band cells
- `energyC` → `player[3].energy` — Front band cells
- `activeA` → `player[1].active`
- `activeB` → `player[2].active`
- `bassDrive` → `audio.bass`
- `midDrive`  → `audio.mid`
- `highDrive` → `audio.high`

Style-only (no BIND): rows, cols, paletteMode, variantMix, motionSpeed,
audioDepth, parallax, tilt3D, popAmount, labelScale, kerning. **11
controllable elements** (≫3); **9 channel binds** (≫2 player-energy/active,
≥1 audio.*, 1 cue.latest).

## glslang

`glslangValidator /tmp/_shape_grid_text.frag` → exit 0 against the Easel
preamble harness (uniforms + macros from ShaderSource.cpp lines 234-360).
No warnings. text→texture2D rewrite simulated to match Easel's regex pass.

## Rubric self-score /25

- (a) Multi-player separability: **5** — three bands, three distinct
  channel binds, each band visually responds (scale + tilt + ink wash +
  caption anchor). Muting any player A/B/C immediately freezes its band.
- (b) Depth & dimensionality: **4** — three parallax planes with per-cell
  z-offset, energy-driven pseudo-3D shear toward a vanishing point at
  the canvas centre. Not raymarched but a coherent fake-perspective trick.
- (c) Intentional motion: **4** — silence reads as a static research
  plate; bass thickens rules, mid spins shapes, high crisps edges, energy
  pops cells forward as compositional events. Per-cell breathing pulse
  keeps the field alive without idle drift.
- (d) Abstract not literal: **4** — the SDF specimens are geometric
  glyphs; the grid evokes a typographic plate, not a depiction of a
  thing. Auto-fail anti-patterns avoided: variety prevents the
  checkerboard fail; no spectrum bars, no EKG, no logo.
- (e) Surprise / risk: **4** — 12-variant SDF library inside a grid
  framework with energy-anchored caption is a move I haven't seen in the
  corpus; the deterministic corner-numeral inset reads as designed
  editorial.

**Total: 21/25.** Hard floor passed (9 BIND channels). No anti-patterns
triggered.

## Files

- `/Users/lu/easel/shaders/shape_grid_text.fs` (source of truth)
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/shape_grid_text.fs`
  (bundle copy, byte-identical)
- `/Users/lu/ShaderClaw3/.critiques/shape_grid_text.md` (this file)

## Caveats / known limits

- Corner numerals top out at 2 digits — fine for rows×cols ≤ 6×6 = 36
  (`MAX_CELLS`).
- Caption is capped at 36 visible chars in the slab; `msg` MAX_LENGTH=48
  so cue.latest still flows but the slab clips the tail rather than
  shrinking glyphs. Could add an auto-shrink branch if longer captions
  become common.
- Pseudo-3D tilt is a 2D shear — looks right at moderate `tilt3D`
  values; cranking the slider past ~1.2 with high energy can shear
  shapes into adjacent cells. The pop scale clamp keeps it within
  ~35% of the cell size, but tilt is uncapped on purpose for expression.
- No `.easel` project edits, no relaunch, no git commits per the brief.
