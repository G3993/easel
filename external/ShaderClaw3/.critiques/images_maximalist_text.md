# images_maximalist_text — critique

## Reference

`/Users/lu/Documents/A-List Shaders/images_maximalist_text.jpg` — a Seoul
Design Festival-style editorial collage: dense cutout-photo stickers
(bagel, teddy on a cloud, foot, banana, watermelon, traffic cone, mushroom,
warning triangle, watering can, etc.) stacked over a full-page wall of
letterpress headline text. Reads as exuberant maximalism — every square
inch is curated, the type owns the spread, the cutouts pop forward in
parallax layers.

## Concept

Three procedural cutout DRAWERS in parallax planes layered over a wall
of headline type:

- back plane (wallpaper tiles, z=−2) ← `player[3].energy`
- mid plane  (photo polaroids, z=0)  ← `player[2].energy`
- front plane (hand stickers, z=+1)  ← `player[1].energy`

Each plane drifts on its own clock + parallaxes to the mouse. Louder
player floods their drawer with more scraps (per-scrap birth crossfade),
brighter scrap interiors, jittery rotation. `audio.bass` globally scales
density (the spread breathes on a bass hit); `audio.high` adds analog
tape grain. `cue.latest` typewrites a giant editorial headline that
word-wraps to fill the page beneath/between the cutouts. The cycled-text
wrap fills the spread even when the cue is short, so the page reads
"wall of type" instead of "centered headline."

## INPUTS with BIND

| INPUT | TYPE | BIND |
|---|---|---|
| `msg` | text | `cue.latest` (typewriter headline) |
| `frontDrawer` | float | `player[1].energy` (front stickers) |
| `midDrawer`   | float | `player[2].energy` (mid polaroids) |
| `backDrawer`  | float | `player[3].energy` (back wallpaper) |
| `audioDensity`| float | `audio.bass` (global density breath) |
| `grain`       | float | `audio.high` (tape grain) |
| `imageCount` | long | manual (8/10/12/14/18/22) |
| `densityBias` | float | manual |
| `paletteMix` | long | manual (Editorial / Newsprint / Pop / Risograph) |
| `motionSpeed` | float | manual |
| `parallaxAmt` | float | manual |
| `textSize` | float | manual |
| `textInk` | color | manual |
| `paperTint` | color | manual |
| `fog` | float | manual |

5 channel binds total — 3× `player[*].energy`, 2× `audio.*` (bass + high),
plus `msg → cue.latest`. Passes the binding-less hard-floor cleanly.
Two distinct audio sub-channels (bass density vs. high grain) satisfy
the ≥2 distinct BIND requirement on `player[i]` and the ≥1 `audio.*`/`cue.*`
rule with margin.

## Validation

glslangValidator pass: clean (exit 0, no warnings). Mirrors
`ShaderSource.cpp:235-360` preamble (`#version 330 core`, `FragColor`,
`gl_FragColor`/`texture2D` redirect, text input expanded as
`msg_0..msg_47` + `msg_len`, audio uniforms, `fontAtlasTex`,
`mousePos`/`msgAge`/`TIME`). Compile harness at
`/tmp/easel_validate/preamble.glsl` + `combined.frag`.

## Rubric self-score — /25

- **a. Multi-player separability — 4/5**: three distinct planes (back
  wallpaper / mid polaroid / front sticker) each bound to one
  `player[i].energy`. Visual languages differ on scale, rotation amount,
  fog depth, jitter — muting one player visibly empties one parallax
  band. Not 5 because the per-plane visual language difference is
  scale-and-jitter rather than three totally separate compositional
  modes (e.g. one plane could be diagonal stripes, etc.).
- **b. Depth & dimensionality — 4/5**: three parallax planes with
  per-plane drift rate, fog falloff, scale base, mouse-parallax depth
  factor + per-scrap drop shadows. Genuine layered z. Not 5 because
  it's parallax 2.5D, not raymarched 3D — there's no occluded volume.
- **c. Intentional motion — 4/5**: planes drift at three distinct rates;
  player energy adds jitter that reads as hand-felt sticker-peeling
  vs. quiet drift; audio.bass pumps density (visible birth-fade of
  scraps); audio.high adds grain. Distinct silence / mid / loud states.
  Not 5 because there are no explicit "holds" or surprise stops — the
  motion is energy-continuous rather than composed in time.
- **d. Abstract not literal — 4/5**: cutouts are abstract colored
  rectangles with fbm "photo" interiors, soft elliptical "subject"
  blobs, white print margins and occasional sticker dots — they
  EVOKE clippings without depicting any literal object. The headline
  is the live cue, not decorative glyphs (passes the rendered-text
  anti-pattern check). Not 5 because the structural homage (text wall
  + cutout collage) maps closely to the reference; a stronger
  abstraction would dissolve the cutouts into pure color-field stains.
- **e. Surprise / risk — 4/5**: combining a wall-of-headline
  letterpress text bed with three parallax planes of procedurally
  painted "image scraps" + per-plane player binding + bass density
  breath + high tape grain is a composite move not present in the
  corpus. Four palette modes (Editorial / Newsprint / Pop / Risograph)
  multiply the design space. Not 5 because each individual technique
  (parallax cutouts, typewriter text bed) exists elsewhere — the
  novelty is the synthesis.

**Total: 20/25** — clears the 18/25 PR threshold.

Anti-patterns triggered: none.
- no horizon line, no mirror symmetry (planes drift independently)
- text is the cue (live transcript), not decorative readable glyphs
- cutouts are abstract colored frames, not literal photos / logos
- no spectrum bars, no EKG waveform, no scoreboard, no SDF debug grid

## Files

- `/Users/lu/easel/shaders/images_maximalist_text.fs` (source of truth)
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/images_maximalist_text.fs` (bundle copy)
- `/Users/lu/ShaderClaw3/.critiques/images_maximalist_text.md` (this file)

## What to try next

- Per-scrap typography: a 1-2 word caption rendered INSIDE the front
  plane stickers (a portion of `cue.latest` split per scrap) — would
  push axis (a) toward 5 since each player's drawer would carry
  literal slices of the cue.
- Vision-driven drawer mapping: `vision.hand.position` could pull
  the front drawer toward the hand, scattering the sticker pile
  across the canvas.
- "Hold" beats: detect bass onsets and FREEZE all plane drift for
  ~250ms after a hit — would push axis (c) to 5 with composed
  silence/burst moments.

## Caveats

- 22-cutout worst case × 3 planes = 66 SDF tests per pixel. On retina
  at full size expect ≈3-4ms per frame on M-series; drop `imageCount`
  to 10 if frame budget is tight.
- Headline word-wrap walk runs once per pixel inside `renderHeadline`
  (max 256 iterations). Bounded but the longest single hot loop.
- The cycled-text fill ensures the page never reads "empty" with a
  short cue. Side effect: a 6-char cue repeats many times — feature,
  not bug (it reads as a typographic refrain), but if the user types
  a short manual msg they may want to lower textSize to thin it.
