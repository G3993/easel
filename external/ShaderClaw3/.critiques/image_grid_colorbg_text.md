# image_grid_colorbg_text — Critique

**Slug**: `image_grid_colorbg_text`
**Files**:
- `/Users/lu/easel/shaders/image_grid_colorbg_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/image_grid_colorbg_text.fs`
- Reference: `/Users/lu/Documents/A-List Shaders/image_grid_colorbg_text.jpg`

## Concept

The reference is a 2022 KASK Open Studios poster: a vermillion ground is sliced open by a diagonal **staircase** of small image-cutout tiles (each tile is a photograph from a different studio), with structural typography running down the margins (vertical name columns) and a giant centred date band. The visual move is *bold-color ground · cutout windows · editorial type at three scales*.

We rebuild the **feeling**, not the literal poster:

- The ground is a saturated palette (vermillion / ultramarine / acid green / paper-on-black), audio-modulated.
- The staircase corridor sweeps across the canvas; cells inside it become tiles, cells outside stay solid ground.
- Each tile is a **procedural studio window** (graded floor, one of four object SDFs — sculpture, rod, panel, shelf — plus key light + film grain). Tile palettes split into four families (Studio / Marbled / Riso / Mono).
- Three player channels each **own a stripe of columns** — when that player is loud, their tiles z-pop forward (real parallax: position offset, scaled shadow), tilt slightly, and a complementary paint flash sweeps through them.
- `cue.latest` renders in three editorial blocks: a **vertical index column** on the left margin, a **giant centre headline band**, a **footer tag** bottom-left — plus optional small captions inside ~55% of the tiles.
- Bass owns the ground hue drift; `audio.level` adds a slow secondary tint.

## INPUTS with BIND

| Input | Type | BIND | Role |
|---|---|---|---|
| `msg` | text | `cue.latest` | typewriter feed for all three type blocks + tile captions |
| `strideA` | float | `player[1].energy` | left third of staircase — z-pop + flash |
| `strideB` | float | `player[2].energy` | middle third |
| `strideC` | float | `player[3].active` | right third |
| `groundPulse` | float | `audio.bass` | ground hue drift |
| `tileRows`, `tileCols` | long | — | grid density |
| `bgMode` | long | — | Vermillion / Ultramarine / Acid / Paper-on-Black |
| `palette` | long | — | tile interior family (Studio / Marbled / Riso / Mono) |
| `motionSpeed` | float | — | global tempo |
| `audioDepth` | float | — | audio amplification |
| `staircaseBias`, `tileGutter`, `edgeShadow` | float | — | staircase composition controls |
| `textBlock`, `grain` | float | — | type opacity + print noise |

Five `BIND` declarations total: three `player[i]` (separability), one `audio.bass`, one `cue.latest`. Passes the intelligence-layer hard floor.

## Validation (glslang Easel preamble harness)

```
glslangValidator -S frag /tmp/image_grid_colorbg_text.glsl
exit=0
```

Preamble matches `src/sources/ShaderSource.cpp:240-340` (RENDERSIZE, TIME, mousePos, msgAge, fontAtlasTex, audioFFT, audio* uniforms, msg_0..msg_47 + msg_len, all user INPUTS declared, `texture2D → texture` rewritten, `gl_FragColor` aliased to `FragColor`). One fix during dev: GLSL 330 reserves `half` — renamed local `vec2 half` → `vec2 hsz`.

## Self-score against RUBRIC.md — /25

| Axis | Score | Rationale |
|---|---|---|
| **a. Multi-player separability** | **4/5** | Three column stripes, three player binds, visually distinct (z-pop + complementary flash per stripe). Falls short of 5 only because all three stripes render the same family of tile interiors — muting one player kills its column unambiguously, but the *visual language* between the three is the same alphabet at different positions. |
| **b. Depth & dimensionality** | **3/5** | Pseudo-3D: real z-pop translation, scaled drop shadows that grow with z, tilt per tile, top-edge specular ridge that only appears on front-popped tiles, depth-aware saturation falloff in tile interiors. No raymarch, so capped below 4. |
| **c. Intentional motion** | **4/5** | Stillness mode = static staircase of muted tiles with slow corridor wobble. Mid energy = tiles breathing, flash sweeps emerging. High energy = z-pop punches, type kerning breath, paint flashes. Distinct quiet / mid / loud states with smooth transitions; the silence reads as *poster at rest*, not absence of motion. |
| **d. Abstract not literal** | **4/5** | Not depicting a poster — depicting the *feeling* of editorial collage. The staircase + cutouts read as a compositional move, not as "look, a poster of a poster". Cuts windows into a color field, doesn't draw a soccer ball or EKG or bar chart. The typography is rendered glyphs from a live cue stream, not a decorative logo. (Anti-pattern safe: no literal logo center — the centre type is *live cue.latest*, treated as content the way the reference poster treats the date "15 > 16 MEI".) |
| **e. Surprise / risk** | **4/5** | The staircase corridor as a procedural mask, the per-stripe complementary paint flash, and a fully procedural "studio interior" baked into every tile (with one of four object SDFs picked per cell) — three moves I haven't seen combined in the corpus. Falls short of 5 because the underlying technique is still 2D SDFs + grid layout, not a wholly new authoring move. |

**Total: 19/25** — clears the 18/25 PR threshold; passes the binding-less hard floor.

## Anti-patterns checked

- [x] No spectrum-analyzer bars
- [x] No EKG sound-wave line
- [x] No checkerboard / SDF debug grid (the staircase is a corridor *cut into* a color field, not the dominant texture)
- [x] No single-color noise plane (grain is print noise, ≤10% modulation)
- [x] No mirror-symmetric beach / horizon
- [x] No logo/readable text as central decoration — centre block is cue.latest, treated as live editorial content

## Caveats / what to try next

- Tile interiors share an alphabet of 4 object SDFs; for a "5" on axis (a) we'd want each player stripe to render its own SDF family (e.g. sculptures for player 1, panels for player 2, ladders for player 3) so muting one stripe visibly removes a whole *kind* of object.
- The staircase corridor is a single diagonal; an "S-curve" or branching corridor (driven by `audio.high`) would unlock more compositional variation across runs.
- The footer tag and index column currently both word-wrap the same `cue.latest`; a future revision could split cue feeds (`cue.coach.headline`, `cue.transcript`) into separate blocks so the typography hierarchy reads as multiple voices.
- `bgMode == 3` (Paper-on-Black) inverts the entire visual relationship — tested in concept, worth a screenshot pass.
- Tile count caps at 12×12 (MAX_ROWS/MAX_COLS); beyond that the per-pixel cost would spike. Likely fine for live use.
