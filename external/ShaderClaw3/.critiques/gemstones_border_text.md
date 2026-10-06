# gemstones_border_text — a ring of polished gems framing a glowing aperture

## Reference
`/Users/lu/Documents/A-List Shaders/3D_magical_gemstones_border_text.jpg` —
a flyer where a U-shaped border of tumbled, glossy gemstones (amber, milky-blue,
violet, rose, sand-yellow, plum) frames a tall arched aperture; inside the
aperture, a warm rainbow gradient bleeds into a teal/grass horizon, with a
small dark peak in the center. Decorative type runs vertically down both
sides of the page. The gems read as photographed pebbles — each one has its
own facet, specular highlight, internal warm refraction, and shadow on the
paper.

## Concept

A procedural border of `N` gems (default 14) arranged on a partial ring
(top ~75% of the circle — the bottom is intentionally left bare like the
flyer's blank label margin). Each gem is built from a wobbled circle SDF
with a per-gem facet-noise normal driving Fresnel, specular highlight, warm
internal refraction tint, and a soft drop shadow on the paper. The aperture
inside the ring is a stratified iridescent gradient (warm rainbow up top,
teal-green at the bottom, abstract peak silhouette in the middle) that
breathes with `audio.bass` and rolls with `TIME`.

Three "cluster arcs" own the ring — left arc → `player[1]`, top arc →
`player[2]`, right arc → `player[3]`. Each player's `energy` pushes its arc
of gems outward and bumps their sparkle; high energy also `smin`-fuses
adjacent gems on that arc into stretched organic shapes so the ring
visually deforms. `player[1].active` and `player[2].active` pin a sparkle
hot-spot per cluster (no audio required). `audio.mid` drives tiny sparkle
pinpricks travelling across all gem surfaces. The live `cue.latest` types
out as a small caption inside the aperture with a blinking caret, then
holds while bubbles linger.

Palette modes: 0 Opal (warm rainbow over milky base — matches reference),
1 Citrine (honey + amber + rose), 2 Amethyst (violet + plum + sky),
3 Aurora (green + cyan + magenta).

## INPUTS & BIND

| input | bind | role |
|---|---|---|
| `msg` (text) | `cue.latest` | typewriter caption inside the aperture |
| `energyA` | `player[1].energy` | left-arc pop + sparkle + warp |
| `energyB` | `player[2].energy` | top-arc pop + sparkle + warp |
| `energyC` | `player[3].energy` | right-arc pop + sparkle + warp |
| `activeA` | `player[1].active` | pins sparkle hot-spot on left arc |
| `activeB` | `player[2].active` | pins sparkle hot-spot on top arc |
| `bassDrive` | `audio.bass` | aperture swell + halo bleed + bloom |
| `midDrive` | `audio.mid` | per-gem sparkle pinpricks |
| `gemCount` (long) | manual | 8–20 stones |
| `gemSize` / `ringRadius` | manual | gem footprint + frame geometry |
| `paletteMode` (long) | manual | 4 palettes |
| `motionSpeed` | manual | global drift |
| `audioDepth` | manual | how strongly audio shapes aperture + halo |
| `facetSharp` | manual | sharpness of specular + facet noise frequency |
| `captionScale` | manual | caption legibility |
| `paperColor` | manual | warm vintage paper |

Channel bindings: 1 `cue.*` + 3 `player[i].energy` + 2 `player[i].active`
+ 2 `audio.*` = **8 channel binds**. **Hard floor passed.**

## 5-axis self-score (RUBRIC.md v2)

| axis | score | rationale |
|---|---|---|
| **a. Multi-player separability** | 4/5 | Three player arcs each own a contiguous slice of the gem ring; muting one player visibly stops its arc's pop / sparkle / warp. Active flags add an orthogonal axis (hot-spot pinning independent of energy). Not 5 because the three arcs share the same gem primitive — separability is by region, not by visual *language*. |
| **b. Depth & dimensionality** | 4/5 | Genuine pseudo-3D per gem: facet-noise driven 2D normal blended with radial dome → Fresnel + specular + warm refraction tint + Lambert body + AO + drop shadow. Aperture has its own depth via stratified gradient + rim vignette + central peak silhouette. Not raymarched, so capped at 4 per the rubric. |
| **c. Intentional motion** | 4/5 | Constant motion every frame (per-gem slot drift + per-gem tilt breathing + aperture gradient roll + sweep + sparkle). Audio: bass swells aperture + halo, mid drives sparkle, player energies pop arcs and `smin`-fuse adjacent gems — calm vs hot states read distinctly. Not 5 because there are no programmed silence-holds / surprise stops. |
| **d. Abstract not literal** | 4/5 | The reference is literal photographed gems framing a flyer; this is the *idea* of gem-border framing — procedural facet shading, abstract aperture gradient, no specific stone species, no literal landscape (the inner "peak" is a tiny dark wedge, not a depicted mountain). Caption is small and label-like, not a headline. Not 5 because "gems forming a frame" remains a recognizable referent. |
| **e. Surprise / risk** | 4/5 | The corpus has `crystal_cubes` but nothing that does a per-gem facet-noise normal + Fresnel + warm refraction in 2D, no border-aperture composition with a stratified iridescent interior, and the `smin`-fusion on energy spikes (so adjacent stones literally merge under crescendo) is a new authoring move. Not 5 because gem-ring frames exist in design culture. |
| **total** | **20/25** | |

## Anti-pattern checklist

- EKG line? **no** — no canvas-spanning sine.
- Spectrum bars? **no** — audio drives aperture swell, halo, sparkle frequency, gem pop, never bar height.
- Literal icons? **no** — gems are procedural SDFs, no pre-baked symbols.
- Default checkerboard? **no** — composition is a partial gem ring over an irregular aperture; no grid.
- Mirror-symmetric horizon? **no** — the ring is asymmetric (open at the bottom), aperture gradient drifts, peak silhouette is small and off-axis.
- Logo / readable text as central visual? **no** — caption is a small label-sized typewriter caption inside the aperture, not a headline; only revealed when `msgAge ≥ 0`.
- Single-color noise plane? **no** — fbm only modulates paper tooth (≤4%) and per-gem facet normals.

## Next iteration ideas

1. **Per-arc visual language.** Currently all gems use the same primitive. Give left arc *opal cabochons* (low specular, broad rim), top arc *faceted briolettes* (sharp spec, hard edges), right arc *tumbled river-stones* (soft Fresnel, no spec). Pushes (a) to 5.
2. **Real raymarched dome.** Replace the 2D facet-normal hack with a tiny per-gem sphere-mapped raymarch (≤8 steps per pixel inside the gem mask). Pushes (b) to 5.
3. **Compositional hold on player onset.** When any `player[i].energy` crosses 0.7, freeze gem drift on the OTHER two arcs for ~200ms while the hot arc reorganizes — gives motion (c) its "moments not gradients" upgrade.
4. **Inscribed text along the ring.** When `msg` is long, optionally route the latter half along the inner edge of the ring (polar text) — exploits the bare bottom margin and adds a typographic surprise (e).
5. **Caustic floor.** A faint refractive-caustic noise field on the paper outside the ring, modulated by gem energies — would tie the gems to their surface physically without raymarching.

## Files written

- `/Users/lu/easel/shaders/gemstones_border_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/gemstones_border_text.fs`
- `/Users/lu/ShaderClaw3/.critiques/gemstones_border_text.md`

## Validation

`glslangValidator -S frag` on the Easel-preamble harness (mirrors
`ShaderSource::translateFragment` — `#version 330 core`, full ISF built-ins,
`fontAtlasTex` + `msg_*` text uniforms, audio uniforms, user INPUTS):
**RC = 0**, no stdout, no stderr.
