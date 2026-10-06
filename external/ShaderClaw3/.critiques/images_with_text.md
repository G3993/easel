## images_with_text — A-List drop

**Reference image:** `/Users/lu/Documents/A-List Shaders/images_with_text.jpg`
(Editorial photo-album spread: small tilted image cutouts scattered on a
diagonal line down a warm paper canvas, thin colored ribbon-lines threading
between them, a big editorial sans headline at upper-right — "DURING THIS
PERIOD PHOTO ALBUMS (P-I)" — plus small Chinese date tags under each photo.
Layered diagonal of color-coded lines, an em-dash before the headline, a
secondary "COLOUR," mid-right.)

### Concept
A contact-sheet collage that lives. 4–12 procedural image cutouts march
along an editorial diagonal (or scatter / column variants) at three
parallaxed z-slices on a warm paper backdrop. Each card is its own little
procedural photo — sky-with-window porthole, garden-with-figure, night
stage, cat-portrait, hair-strands, dusk skyline, ocean strip, paper-with-
red-circles, satellite map, silhouette, archway, color swatch. Thin
palette-colored ribbon lines thread between adjacent card centers; on
transient highs a comet-bead sweeps along each ribbon. The headline is
`cue.latest` typed out in the upper-right as a multi-line ribbon — the
"DURING THIS PERIOD PHOTO ALBUMS" role — breathing on bass.

Three cards each own a `player[i]` channel: that speaker → card scales
forward in z, saturation blooms from desaturated-rest to crisp-active,
and a warm rim glow appears. Other cards drift at idle. Each card has a
small dark caption tick + bullet underneath (the (5.26)-style date tags).

### Channel bindings declared
- `cue.latest` → `msg` (headline ribbon; auto-bound by Application::loadShader)
- `player[1].energy` → `cardA`
- `player[2].energy` → `cardB`
- `player[3].active` → `cardC`
- `audio.high` → `ribGlint`   (ribbon comet brightness)
- `audio.bass` → `bassBreath` (headline breathing)

Hard-floor satisfied: ≥2 distinct `player[*]` binds (3), `cue.latest`
bound to `msg`, two distinct `audio.*` binds. None of the binds collapse
to the same channel.

### Self-score (5-axis rubric)
- **a. Multi-player separability — 4/5** — Three named cards each own a
  distinct `player[i]` channel; muting one cleanly de-activates that
  specific card (rim glow vanishes, saturation drops, no forward push).
  Visual languages share the same "card pops + rim" idiom across the
  three named slots — same response, different positions — so it's clean
  3-way decomposition but not 3 *different* visual languages. Would
  reach 5 by giving each named player its own response motif (one pops,
  one rotates, one ignites its caption tick).
- **b. Depth & dimensionality — 3/5** — Three explicit z-slices with
  back-to-front compositing, parallax drift proportional to z, soft
  drop-shadow under each card, atmospheric haze fades far slices into
  paper, and a global vignette+haze pass. Pseudo-3D not raymarched, but
  the layer stack reads as space (paper → dust → back cards → mid cards
  → front cards → ribbons → headline). Bumping to 4 would need a real
  depth-of-field on the far slice or perspective skew per card.
- **c. Intentional motion — 4/5** — Idle drift is small + per-card
  individuated (sin/cos at unique phases), not loopy. Player activation
  is a moment (forward push + saturation jump + rim glow). Ribbon comets
  are transient highs-events, not constant. Headline breathes with bass.
  Multi-mode: quiet/forward/glint/breath. Could go to 5 with an explicit
  "hold" beat on silence and a coordinated handoff animation between
  cards when the active player switches.
- **d. Abstract not literal — 4/5** — The piece *evokes* an editorial
  photo album but the "images" are abstract procedural patches (sky
  porthole, light cone, color swatch) — not photos. No spectrum bars,
  no EKG, no logo center. Caption text is the cue stream, not decorative
  glyphery. Headline is structural typography (the data IS the visual).
- **e. Surprise / risk — 4/5** — Editorial photo-album diagonal with
  procedurally-painted card interiors and live ribbon comet glints on
  highs is a composition I haven't seen in this corpus (`images_*_text`
  variants in the corpus go heavier on 3D hero shapes; this one stays
  flat-collage and earns its complexity from layered z + connector
  ribbons + caption tags). A new authoring move: connector ribbons as
  audio-transient comet tracks.

**Total: 19/25**

### Anti-pattern checklist
- Spectrum-analyzer bars: NO (no FFT bins visualized as bars)
- EKG sound-wave line: NO (ribbons connect points, not waveform plots)
- Literal soccer ball / scoreboard: NO
- Default checkerboard / SDF debug grid: NO
- Single-color noise plane: NO (paper has marbled fbm + dust motes, but
  composition dominates)
- Mirror-symmetric beach / horizon: NO (diagonal layout is intentionally
  asymmetric)
- Logo / decorative readable text as central visual: NO — `msg` is
  `cue.latest` (live transcript) sitting upper-right; structural, not
  decorative.

### Style INPUTS exposed
- `imageCount` (4–12) — count of procedural photo cards
- `palette` (Editorial/Risograph/Mono/Acid) — color rotation
- `motionTempo` — global time scale
- `audioDepth` — audio reactivity strength
- `layoutVariant` (Diagonal/Scatter/Column) — three composition styles
- `ribbonDensity` — connector line thickness/visibility
- `fogDensity` — atmospheric haze on far slices
- `grain` — paper fiber tooth
- `headlineScale` — headline ribbon glyph size

### Next-iteration ideas
1. Give each named player its OWN response motif (cardA pops forward,
   cardB rotates 8°, cardC ignites its caption ink) — bumps (a) → 5.
2. Stillness hold: when audio.level < threshold for ≥0.5s, freeze drift
   for a beat then resume — bumps (c) → 5.
3. Real DOF on z-slice 0 (downsample fbm-blur the far cards) — bumps
   (b) → 4.
4. Connector ribbon as a Bezier arc instead of straight segment — more
   organic, matches reference's curved-line feel exactly.

### Files written
- `/Users/lu/easel/shaders/images_with_text.fs` (source)
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/images_with_text.fs` (bundle copy)
- `/Users/lu/ShaderClaw3/.critiques/images_with_text.md` (this file)

### Validation
Preamble harness mirrors `ShaderSource::translate`: `#version 330 core`,
`gl_FragColor` aliased to `FragColor`, `texture2D` → `texture`, all ISF
builtins (TIME, RENDERSIZE, mousePos, msgAge, audioLevel/Bass/Mid/High,
audioFFT, fontAtlasTex, vision uniforms, msg_0..47 + msg_len) plus the
14 declared INPUTS as uniforms. Harness: `/tmp/easel_glslang_harness_iwt.sh`.

```
glslangValidator -S frag /tmp/easel_isf_iwt_*.frag
--- exit=0 ---
```

Clean compile, no warnings.

### Caveats
- Not relaunched in-app this session (per constraints). Risk surface is
  bundle path / INPUT type matching, both of which match patterns used
  by `images_3dshape_text.fs` and other Easel A-List shaders already
  shipping. All INPUT types (`text`, `float`, `long`) appear elsewhere
  in the corpus.
- The shader uses `fwidth` for card AA — Easel runs on GL 3.3 core which
  has it in the fragment stage by default; no `#extension` directive
  needed.
- Headline word-wrap simulates the same algorithm `text_clusters.fs`
  uses: whole words never split unless a single token exceeds the row
  width. Multi-line headlines wrap into ≤5 rows; the row count cap
  prevents overflow into the cards below.
- No reserved-word collisions encountered. Variable name `palette` is
  used as both an INPUT and a category — they're in different scopes
  (uniform vs. local var passed via paletteSw).
