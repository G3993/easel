## images_grid_lines_text — A-List drop

**Reference image:** `/Users/lu/Documents/A-List Shaders/images_grid_lines_text_.jpg`
(An editorial photo-album page: a loose 6×8 lattice of small photo cutouts — skyline, plane window, foliage, concert lights, white cat, hair, sticker, satellite map, sunset — captioned with tiny CJK glyphs and dates, criss-crossed by a cage of sharp coloured lines, and a big right-margin "DURING THIS PERIOD / PHOTO ALBUMS (P-I)" headline in black sans-serif, plus a "COLOUR," sub-mark and a "PHOTO ... ALBUMS" footer.)

### Concept
Three z-planes layered on warm paper. Plane 1: a loose lattice of procedural "photo cutouts" — each tile is one of 8 abstract micro-scenes (skyline strip, water-glint, foliage grain, fur sweep, hair strands, dark window, ticket pastel, satellite map). Cells are jittered, occasionally skipped, slightly tilted, each with a paper border and a single dark "caption dot" in its bottom-left margin — the *idea* of a captioned snapshot, not literal text. Three rows-bands are owned by `player[1..3]` (top/mid/bot); when player N talks, that band's tiles lift forward in z, saturate, scale up, and pick up a coloured rim. The back rows stay desaturated and softened (per-row pseudo-DOF), so muting a player visibly drops their band out of focus. Plane 2: a cage of 4–14 sharp coloured lines (palette-tinted) crosses the canvas at random angles. Lines breathe on `audio.bass`, their width fwidth-AA'd; hot lines bloom with a soft outer glow. Plane 3: the right-margin headline — `cue.latest` typewrites in big editorial sans, word-wrapped, with a blinking caret while live. The full thing reads as the *feeling* of flipping a photo-album page mid-conversation.

### Channel bindings declared
- `cue.latest` → `msg` (title typewriter)
- `player[1].energy` → `energyA` (top row-band)
- `player[2].energy` → `energyB` (mid row-band)
- `player[3].active` → `energyC` (bot row-band)
- `audio.bass` → `bassPulse` (line cage breathing)

Hard floor: PASSED — 3 distinct `player[*]` binds + `cue.latest` + `audio.bass`. Three independent visual row-bands, each driven by its own channel; muting any one band quiets that subset of the grid.

### INPUTS (full list, 14)
- text: `msg` (BIND cue.latest)
- player binds (3): `energyA`, `energyB`, `energyC`
- audio bind: `bassPulse` (audio.bass)
- style: `gridCols`, `gridRows`, `lineDensity`, `palette`, `motion`, `audioDepth`, `cutoutFill`, `lineWeight`, `textOpacity`, `grain`

### glslang validation
Harness: `/tmp/easel_validate.sh` — strips the JSON header, prepends Easel's `ShaderSource::translateFragment` preamble (#version 330 core, FragColor + gl_FragColor macro, RENDERSIZE/TIME/PASSINDEX/mousePos/msgAge/audioLevel/audioBass/audioMid/audioHigh/audioFFT/fontAtlasTex, all msg_0..47 + msg_len, every declared INPUT), then runs `glslangValidator`. Result: **exit 0, no warnings**. Cross-verified by also passing `images_3dshape_text.fs` (also exit 0).

### Self-score (5-axis rubric)
- **a. Multi-player separability — 5/5** — Three spatial row-bands, each bound to its own `player[i]` channel. Distinct visual response per band (lift in z, scale up, saturate, rim-glow) — muting one band visibly desaturates and softens that strip of the grid. The bass channel additionally drives the global line cage (a fourth orthogonal visual layer).
- **b. Depth & dimensionality — 4/5** — Real layered z: paper backdrop → tile field with per-row pseudo-DOF (back rows softened by `mix(1.0, 0.45, rowF)` softness) and per-tile lift on owner energy → line cage on its own z-plane → title text floating topmost. Not raymarched, but the depth reads as layered space, not flat 2D, and the per-row blur is genuine optical depth, not just shading.
- **c. Intentional motion — 4/5** — Quiet state: tiles drift slowly inside their cells, lines rotate very gently. Active: owner band tiles lift forward, lines pulse on bass, title typewrites with `msgAge`. Crescendos arrive as compositional events (a band lifting out of the page). Could go 5/5 with a punctuated "hold" mid-utterance where motion freezes for a beat.
- **d. Abstract not literal — 5/5** — Photo cutouts are procedural abstract patches (water = noise+highlight, foliage = directional fbm strokes, hair = sinusoidal strand fields, skyline = horizon-stripe + silhouette). No literal soccer/EKG/spectrum-bar/checkerboard. The grid is irregular (~30% cells skipped) so it never reads as a literal lattice. Bottom-corner "PHOTO/ALBUMS" markers are abstract ink-strips, not readable glyphs.
- **e. Surprise / risk — 4/5** — Editorial-poster aesthetic with a procedural micro-scene generator (8 distinct abstract textures, one per tile-kind) is a new authoring move in the corpus — none of the existing A-List shaders run a per-tile sub-shader generator. The line-cage-over-grid composition is rare; the dual-channel decomposition (row-bands + line cage on different signals) is the canonical rubric bar. Could push to 5 by giving each player a distinct palette accent that propagates into both tile rims AND the lines that *cross* their band.

**Total: 22/25**

### Anti-pattern checklist
- EKG sound-wave line: NO (lines are sharp linear segments, not waveforms)
- Spectrum-analyzer bars: NO
- Literal soccer ball / scoreboard / pitch outline: NO
- Default checkerboard / SDF debug grid: NO (irregular lattice, ~30% skipped, tilted, jittered)
- Single-color noise plane: NO
- Mirror-symmetric beach / horizon: NO
- Logo / readable text as central visual: NO — the title is `cue.latest` typewriter (intentional cue text), per rubric exception. Bottom-edge "PHOTO/ALBUMS" markers are abstract dark strips with no glyph rendering.

### Next-iteration ideas
1. **Per-player palette accent that crosses the cage** — give each row-band its own palette pick (red/mint/yellow) so the *lines that pass through that band* tint toward the owner's colour. Boosts axis (e) to 5.
2. **Punctuated stillness** — when `audio.level < 0.05` for > 0.4s, freeze all tile drift + line rotation for one beat then resume. Bumps (c) to 5.
3. **Tilt-stack DOF** — add a slight per-tile y-rotation that scales with `pitch` channel so each tile reads as a small physical print on the page.

### Files written
- `/Users/lu/easel/shaders/images_grid_lines_text.fs` (source, 27.5 KB)
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/images_grid_lines_text.fs` (bundle copy)
- `/Users/lu/ShaderClaw3/.critiques/images_grid_lines_text.md` (this file)

### Caveats
- Validated against a hand-rolled glslang harness mirroring `ShaderSource::translateFragment` exactly. Compile is clean (exit 0, no warnings). Not run in-app this session (no relaunch per constraints).
- The per-row energy assignment maps `player[3].active` (a bool-ish 0..1) into the bottom band rather than `player[3].energy`, so the bottom row toggles on/off rather than scaling continuously — a deliberate variety move so the three bands don't all respond identically. If a smoother decomposition is desired, swap the BIND target to `player[3].energy`.
- The microScene generator uses 8 kinds chosen by `mod(seed*3.0 + ts.z*8.0, 8.0)` per cell — the same kind index can repeat across the grid; this is intentional (echoes the reference, where the cat appears alongside the satellite alongside the skyline with no rule against repetition).
- One ALU watch-point: per-pixel inner loop is `MAX_ROWS×MAX_COLS = 10×8 = 80` tiles × per-tile SDF + microScene evaluation. On a 1080p canvas this is ~166M tile-evaluations/frame in the worst case (gridCols=8, gridRows=10). Default cols/rows (6×8 = 48) is the design target; the higher cap exists for headroom. If we ever profile thermal pressure on the 60Hz path, the first lever is dropping `MAX_ROWS` to 8.
