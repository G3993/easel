# circle_text — A-List drop critique

## Reference
`/Users/lu/Documents/A-List Shaders/circle_text.jpg` — the *Whispers* sequential
art-game page: a perfect ring of ~50 small tilted artwork tiles on warm near-white
paper, with quiet centered serif text inside. Calm, gallery-like, contemplative,
radial. The text is the still center; the ring is the chorus.

## Concept
Three concentric **shells** (inner / middle / outer) of orbiting micro-cards form
a halo around a quiet centered glow. Total cards = 50, matching the reference.

- **Inner shell** (14 cards, smaller, ~0.78R) — warm/red accent, driven by `player[1].energy`.
- **Middle shell** (18 cards, full size, R) — cool/blue accent, **carries the message
  glyphs**; driven by `player[2].energy`.
- **Outer shell** (18 cards, ~1.22R) — soft green accent, driven by `player[3].energy`.

Each card is its own little world: text cards show one serif-styled character on a
warm-white tile (gallery card); non-text cards render an abstract procedural
micro-painting (three soft palette blobs + painterly grain). Typewriter reveal:
characters of `msg` appear in order as `msgAge` advances, so as a person speaks,
the ring "writes itself" one card at a time. Cards before the reveal are blank
tiles tinted with mid-accent (echoing the unfilled spots in the reference).

Real depth: cards travel an **orbit tilted ~25° toward camera**, so each gets a per-card
z from `sin(ang)`. Z drives perspective scale (front cards bigger, back smaller),
content shear (faux-3D card foreshortening), depth haze (back cards desaturate
into paper), and a soft drop shadow on the paper.

Audio:
- `audioBass` widens the ring radius (halo breath) via `audioDepth`.
- Per-shell `player[i].energy` adds *per-shell* jitter (angle + radius wobble),
  so silencing one player visibly freezes one ring while others keep breathing.
- `audioLevel` brightens the center glow; total talk-energy widens the aura.

Backdrop is a warm-paper watercolor (fbm marble + soft sun + vignette + grain +
slow raking sheen) — quiet, gallery-grade, never flat.

## INPUT bindings (channel contract)
| INPUT | BIND |
|---|---|
| `msg` (text) | `cue.latest` (auto-bound by Application::loadShader for text inputs named `msg`) |
| `innerEnergy` (float) | `player[1].energy` |
| `midEnergy` (float) | `player[2].energy` |
| `outerEnergy` (float) | `player[3].energy` |
| `ringRadius`, `ringSpin`, `cardSize`, `tiltAmp`, `textSize` | manual style controls |
| `audioDepth`, `paperWarm` | manual style controls (audio reactivity strength + paper warmth) |
| `paperColor`, `inkColor`, `accentA/B/C` | palette controls |

Binding-floor: 3 `player[i].energy` binds + 1 `cue.latest` (via `msg`) + uses
`audioBass`/`audioLevel`. **Meets the contract with room to spare.**

## Rubric self-score

| Axis | Score | Rationale |
|---|---|---|
| **(a) Multi-player separability /5** | **5** | Three shells, three independent `player[i].energy` binds, three visually distinct radii + accents. Silencing any one player visibly stops one shell's jitter while the others continue — the rubric's "mute test" passes cleanly. The middle shell additionally owns the text channel, so each shell has its own visual language (inner = warm micro-paintings, middle = serif glyphs on white tiles, outer = green micro-paintings). |
| **(b) Depth & dimensionality /5** | **4** | Genuine pseudo-3D: orbit is tilted, each card has a real z from `sin(ang)` that drives perspective scale, content shear, depth haze, and drop shadow. Back cards shrink and desaturate into paper while front cards pop. Z-ordered compositing keeps the closest card per pixel. Not raymarched, so reserving 5 for true volumetric work. |
| **(c) Intentional motion /5** | **4** | Multi-mode: silence reads as a slow contemplative orbit with a faint center glow; speech triggers per-shell jitter (only the talking player's ring wakes); crescendo widens the halo, brightens the center, blooms accent into highlights. Motion varies between shells (different rotation speeds + tilt phases). Holding back from 5 because there's no surprise-stop or hard cut. |
| **(d) Abstract not literal /5** | **4** | The reference shows tiny artwork tiles arranged in a ring; this shader abstracts each tile into a **procedural micro-painting** (palette blobs + grain) rather than literally rendering art. The serif center text is replaced with character-per-card distribution around the ring — the *idea* of "ring of tiles around a quiet text" is preserved, but no element is depicted literally. Text glyphs ARE used, but they're tiny serif characters as DECORATION OF EACH TILE, not a single readable central word — and they're driven by `cue.latest`, which is the rubric's exemption for cue text. Not a 5 because the ring *is* visually a ring (the composition is direct), but the surface itself is fully abstract. |
| **(e) Surprise / risk /5** | **4** | The corpus has metaball-text shaders (`text_clusters.fs`) and gallery-aesthetic shaders (`color_world.fs`), but none do the **"50 tiny billboards orbiting a tilted ring, each one a per-card mini-painting OR a per-card glyph"** move. The card-by-card typewriter reveal *around a ring* (rather than line-by-line) is a new authoring move for this corpus. Per-card 3D shear + ring perspective + depth haze is a technique combo I haven't seen here. Holding back from 5 because the underlying "orbit of things" is a familiar shader trope. |

**Total: 21 / 25**

## Anti-pattern checklist
- [x] No EKG sound-wave line — **NO**
- [x] No spectrum analyzer bars — **NO**
- [x] No literal object icons (no soccer ball, scoreboard, etc.) — **NO**
- [x] No default checkerboard / SDF debug grid — **NO**
- [x] No single-color noise plane — **NO** (composition is radial + layered)
- [x] No mirror-symmetric beach / horizon scene — **NO** (radial composition, not horizon)
- [x] No logo / readable central text (cue text inputs are exempt; here glyphs are
      distributed one-per-card around the ring, not a single readable word) — **NO**

All clear. **Hard floor passed** (3 player binds + 1 cue bind > 0).

## Files written
- `/Users/lu/easel/shaders/circle_text.fs` (20.5 KB)
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/circle_text.fs` (copy)
- `/Users/lu/ShaderClaw3/.critiques/circle_text.md` (this file)

## Validation
`glslangValidator` exit 0 against Easel's faithful preamble harness
(`#version 330 core`, `out vec4 FragColor`, `#define gl_FragColor FragColor`,
`#define texture2D texture`, all builtins: TIME / TIMEDELTA / RENDERSIZE /
isf_FragNormCoord / audio* / mousePos / pinchHold / msgAge / mpPose* / audioFFT
/ fontAtlasTex; user INPUTS materialized per host typing including
`msg_0..47` + `msg_len`). Clean — no warnings.

## Next-iter ideas
- Inject a faint procedurally-rendered **serif phrase at the center**
  (e.g. the first word of `msg`) so the composition more directly quotes the
  reference's centered-text move. Currently we keep the center quiet (an aura)
  to avoid duplicating glyph rendering with the ring.
- Add a **second tilt axis** (yaw oscillation) so the ring slowly turns like
  a vinyl record on a tilted turntable — would push axis (b) to a 5.
- Per-card **fadeout on `cue.latest` change** (cards "shed" old characters
  before new ones arrive) — would push axis (c) to 5 with hard cuts.
- Bind one card slot to `data.heroIndex` so a user-pushed integer can pin
  ONE card as "the highlighted artwork" (echoes the reference's editorial
  framing).
