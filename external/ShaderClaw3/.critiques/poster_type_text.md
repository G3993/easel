# poster_type_text — critique

**Slug**: `poster_type_text`
**File**: `/Users/lu/easel/shaders/poster_type_text.fs`
**Reference**: `/Users/lu/Documents/A-List Shaders/poster_type_text.jpg`
**Category**: `["Generator","Text","A-List"]`

## Concept

A live editorial poster. The `msg` (cue.latest) IS the artwork: a giant
two-line word-broken headline locks the top band, a foot rail breaks the
message in half across the bottom corners, and a vertical micro-caption
threads the right rail. Between them sits the LENS — an oval window cut
into the paper showing a parallax horizon (5 z-planes: sky stratum →
sun disc → far ridge → midground field → foreground grass-veil) that
dollies under player[1].energy and the mouse.

The reference is a Meller Specs poster — heavy black sans headline,
horizon viewed through a lens, micro-caption blocks at corners. This
shader abstracts that *language* (type-as-hero + lens-into-world +
editorial gutters), not the literal landscape — the lens is a horizon
as essence, not a postcard.

## INPUTS & BIND

| Input          | Type  | BIND                | Drives                          |
|----------------|-------|---------------------|---------------------------------|
| `msg`          | text  | `cue.latest`        | headline + foot rail + vertical micro-caption (typewriter via `msgAge`) |
| `headlineSize` | float | —                   | headline band scale             |
| `palette`      | long  | —                   | 5 editorial palettes            |
| `layoutVariant`| long  | —                   | 4 layout variants (lens center/low/wide/off-axis) |
| `motionSpeed`  | float | —                   | global time multiplier          |
| `audioDepth`   | float | `audio.high`        | lens chromatic separation + rail parallax |
| `energyA`      | float | `player[1].energy`  | lens camera dolly               |
| `energyB`      | float | `player[2].energy`  | sun pulse + headline chroma split |
| `energyC`      | float | `player[3].energy`  | registration-mark twitch + headline shear |
| `bassPunch`    | float | `audio.bass`        | sun radiance pulse              |
| `transparentBg`| bool  | —                   |                                  |

That's **4 numeric channel binds + 1 text bind** — `cue.latest`, three
`player[i].energy` slots, plus `audio.high` and `audio.bass`. Each
player drives a *distinct* visual element (dolly vs sun vs reg-mark+
shear) so muting one is immediately visible.

## Validation

- **glslang** (#version 330 core via Easel preamble harness): **PASS**.
  Combined preamble + body compiles clean, no warnings.
- **Easel `test_shaders` runtime**: **PASS** — 11 inputs parsed, full
  GL compile succeeds.

## Rubric self-score (target /25)

| Axis | Score | Why |
|---|---|---|
| (a) Multi-player separability | **4/5** | 3 player binds + 2 audio binds, each on a distinct element (dolly, sun, reg-mark+shear). Mute B → sun stops pulsing & chroma collapses; mute C → reg-mark stills & headline straightens. Visually distinguishable. -1 because energyA's effect (camera dolly) is shared with mouse parallax so attribution is less crisp than B/C. |
| (b) Depth & dimensionality   | **4/5** | 5 explicit parallax planes inside the lens (sky stratum, sun, ridge, field, grass-veil) at distinct camera-coupling rates (0.15× / 0.35× / 0.55× / 0.85× / 1.4×). Pseudo-perspective rows for the field. Outside the lens the poster is intentionally flat — that flat/depth contrast IS the composition. Not raymarched, so not a 5. |
| (c) Intentional motion       | **4/5** | At rest: a still poster (paper drift, sun hovers, no twitch). With energy: headline shears, sun blooms with bass, registration mark jitters, chromatic separation appears on glyphs. Silence reads as a finished print; loud reads as a press-room snapshot mid-run. |
| (d) Abstract not literal     | **4/5** | The lens references the ref's oval-landscape motif but never depicts a "real" landscape — it's a stratified gradient with parallax registers, more print-color-bars than postcard. No literal sun rays, no figurative trees. Type is type, not iconography. -1 because the horizon-with-sun is still readable as "horizon." |
| (e) Surprise / risk          | **4/5** | The editorial-poster-as-shader move (headline word-wrap, two-line break, foot rail, vertical micro-caption, registration mark, ✚ flourish) is rare in the corpus. The lens-cut-into-paper compositing reads as a print artifact, not a generic vignette. New authoring move: a *typographic* shader where the type drives the whole hierarchy of the canvas. |
| **Total**                    | **20/25** | |

Hard floor: passes (multiple `player[*]`, `cue.*`, `audio.*` binds).
Anti-patterns: none triggered (no spectrum bars, no waveform, no SDF
checkerboard, no mirror-symmetric beach, no logo). The headline is the
*subject*, not decoration — `cue.latest` text inputs are explicitly
allowed by the rubric.

## Caveats / known soft edges

- Headline word-wrap uses a single split point at the midpoint space.
  For messages with one very long word or zero spaces, the split falls
  at `total/2` and that token will hard-break — acceptable for the
  poster aesthetic but not glyph-perfect.
- The vertical right rail stacks up to 16 chars top-down; longer
  messages truncate on the rail (the foot rail and headline still show
  the full msg).
- `transparentBg` uses a luminance-distance-to-paper alpha
  approximation, not a perfect cutout. Fine for layering over video.
- Lens parallax is camera-yaw only (X-dolly); a real cone-of-view
  pitch would require a 3rd-axis transform. Could be a v2 move.
- On extreme landscape aspect, the lens inner world stretches slightly
  on the X axis even with the `aspect * 0.75` correction. Acceptable
  for the 16:9 / 4:3 range, would need a `min(aspect,…)` clamp for
  ultrawide.

## Files

- `/Users/lu/easel/shaders/poster_type_text.fs` (canonical source)
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/poster_type_text.fs` (bundled copy)
- `/Users/lu/ShaderClaw3/.critiques/poster_type_text.md` (this file)

No `.easel` project edits, no app relaunch, no commits.
