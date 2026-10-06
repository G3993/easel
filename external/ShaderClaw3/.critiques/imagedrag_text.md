# imagedrag_text — A-List drop critique

## Reference
`/Users/lu/Documents/A-List Shaders/imagedrag_text.jpg` — a Six / madebysix
Instagram tile for Roope Rainisto's *Post Photographic Perspectives*. A single
photographic cutout (sky / hedge / lawn / house / red-dressed figure) is
repeated as a staircased smear across the canvas, as if the artwork was dragged
diagonally from upper-left to lower-right. Editorial caption sits in the upper
third on a dark paper backdrop. Calm but kinetic — the still photo *moves*.

## Concept
Three independent **drag stacks** (front / mid / back) lay across the canvas.
Each stack is a procedurally drawn photo cutout (sky + horizon + ground + a
small figure, with a tiny house on the back stack) repeated N times along a
trail vector with decaying alpha and a real per-stamp z. The smear isn't a
post-process blur — it's *N distinct stamps*, so the staircase reads as motion
and as space at the same time. Newer (frontmost) stamps occlude older ones via
front-to-back premultiplied composition.

- **Stack A** (foreground) — variant 0: tall hedge stripes + figure. Bound to
  `player[1].energy/active`. When P1 is silent, the trail collapses to a
  near-still cutout; when active, the drag stretches and the rake angle
  steepens by ~0.22 rad.
- **Stack B** (mid-depth) — variant 1: lawn + figure. Bound to
  `player[2].energy/active`. Independent origin, independent drag direction,
  independent length.
- **Stack C** (deepest) — variant 2: gravel + small house + figure. Driven by
  `player[3].energy` (rest energy floor so it's never dead).

`audio.bass` deepens *all* drag lengths additively (a shared push that doesn't
overwhelm per-player decomposition). `audio.high` jitters every stamp's offset
by ≤1.5% — feels like vibration in the hand dragging the cutout.

Caption is the live `msg`/`cue.latest` typewriter, plain horizontal layout in
the editorial top third, with a faint forward echo behind each glyph that
recalls the image-drag without literally smearing the type.

## INPUTS & binds
- `msg` ← `cue.latest` (text)
- `energyA/B/C` ← `player[1..3].energy`
- `activeA/B` ← `player[1..2].active`
- `bassDrive` ← `audio.bass`
- `trebleDrive` ← `audio.high`
- Style sliders: `dragLength`, `imageCount`, `motionSpeed`, `audioDepth`,
  `trailDecay`, `depthAmount`, palette colors, `kerning`, `labelScale`.

8 distinct channel binds across player/audio/cue. Floor passed.

## Rubric self-score — /25

- **a. Multi-player separability — 4/5.** Three visually independent stacks,
  each on its own player. Distinct origins, drag directions, depths, and
  cutout variants → muting P1 immediately stills the foreground stack while
  P2/P3 keep moving. Capped at 4 (not 5) because all three share the same
  cutout *language* (horizon photo); the visual language doesn't fully
  diverge per player.
- **b. Depth & dimensionality — 4/5.** Real layered z: per-stamp scale +
  vertical lift + paper-fog desaturation + per-stack depth amount + global
  back-to-front compositing across three depth tiers. Not raymarched, so
  not a 5.
- **c. Intentional motion — 4/5.** Silence = near-still (rest energy keeps a
  small breath, motionSpeed scales the whole field), talking = trail extends
  and rake steepens, bass = whole stack pushes, treble = micro-jitter. Four
  distinct response surfaces → not one curve.
- **d. Abstract not literal — 4/5.** The cutouts are *recognizable as
  photo-ish vignettes* (intentional, matching the reference) but no anti-pattern
  triggers: no checker, no bars, no waveform, no scoreboard, no logo. The
  horizon is asymmetric and wandering — not the mirrored beach failure mode.
- **e. Surprise / risk — 5/5.** Image-drag-as-shader-primitive: drawing a
  procedural photo and stamping it along a trail vector with decaying alpha
  is a new authoring move in this corpus. The staircase composition isn't
  in the existing pack.

**Total: 21/25.**

## Hazards / watch-outs
- The procedural "photo" lives or dies on the cutout being readable as an
  image even at the smallest back-stack scale. If the figure / horizon
  read collapses, the smear loses meaning. Worth a screenshot pass.
- Stamp count > 12 with depthAmount > 1.0 starts to ghost the figure into the
  paper — feature, not bug, but tune cap if it gets muddy.
- Caption box uses naive equal-width row splits (no word-wrap pre-pass). Long
  multi-word captions can split a word at the row boundary. Acceptable for
  uppercase editorial titles; if users start using sentences, port the
  word-wrap pre-pass from `dotconnector_clusters_text.fs`.
- No anti-pattern flagged.

## Files
- `/Users/lu/easel/shaders/imagedrag_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/imagedrag_text.fs`
