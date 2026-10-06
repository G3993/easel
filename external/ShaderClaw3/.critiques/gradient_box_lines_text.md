# gradient_box_lines_text — critique

## Concept

Gallery-poster homage to the reference (Nisargadatta Maharaj / "I am nothing
/ I am everything"): a thin black-ruled inner frame holds three layered
watercolor gradient clouds (back blue / mid violet / front mint) drifting
at distinct parallax z-depths. Two sharp structural curves — a long
S-trajectory and a tighter lower-right loop — cross the frame. The
`cue.latest` message is decomposed word-by-word into numbered footnote
chips that pop in along the primary curve via `msgAge`, alternating
sides. Header rule + tiny title at top; chromatic foil strip at bottom.
Paper backdrop with vignette + grain.

## Reference fidelity

Strong on **what the reference signals**, intentionally not a literal
copy:

- Layered watercolor blooms inside a thin frame — yes (cloud()
  long-axis warped gradients, NOT discs).
- Two structural curves — yes (primary variant + secondary loop overlay).
- Numbered list items along the inside arc — yes (one chip per word with
  superscript index, alternating sides of the curve).
- Header / footer plates — yes (rule + title at top, foil strip at bottom).
- No literal logo or pictorial content — passes the rubric anti-pattern
  list (no spectrum bars, no waveform, no checkerboard, no mirror beach).

## INPUTS / BIND map

- `msg` (text)            → cue.latest (auto-bound by Easel)
- `energyA` (float)       → player[1].energy   (front mint cloud)
- `energyB` (float)       → player[2].energy   (mid violet cloud)
- `energyC` (float)       → player[3].energy   (back blue cloud)
- `bassDrive` (float)     → audio.bass         (line density + cloud swell)
- `highDrive` (float)     → audio.high         (line crispness shimmer)
- `gradientAngle`         → style (cloud long-axis rotation)
- `paletteShift`          → style (hue rotation across all tints)
- `structureVariant`      → style (S / Loop / Double / Helix)
- `lineDensity`           → style (line weight base)
- `lineCrispness`         → style (line edge falloff)
- `motionSpeed`           → style (global motion scalar)
- `audioDepth`            → style (audio→visual depth coupling)
- `textSize`              → style (footnote glyph size)
- `colorA/B/C/paper/ink`  → style (palette)
- `showFrame/Header/Footer/grain` → style toggles

Six channel binds total (3 players + 2 audio + cue.latest). Hard floor
passed: shader speaks the channel language.

## glslang validation

`glslangValidator -S frag` on the Easel-preamble-preprocessed source:
**PASS** (no errors, no warnings). Preproc size 28.5 KB. Bundles to
`Easel.app/Contents/Resources/shaders/`.

## Rubric self-score (provisional)

- **a. Multi-player separability — 4/5.** Three clouds, three independent
  `player[i].energy` binds, each with its own anchor / axis / softness /
  drift speed. Mute one player and that cloud body holds still + falls
  to a faded silhouette; the other two keep breathing. Could push to 5
  with more divergent visual languages (e.g. front cloud could shift to
  a different SDF shape on high energy), holding at 4.
- **b. Depth & dimensionality — 4/5.** Real parallax: clouds at three
  different cam-offset factors (0.30 / 0.65 / 1.10), DOF softness varies
  per layer (1.8 / 1.2 / 0.85), lines render OVER clouds with crisp
  edges (faux z-stack), chips render OVER lines. Helix variant adds
  pseudo-3D projection. Not raymarched — capped at 4.
- **c. Intentional motion — 4/5.** Silence → tiny anchor drift only
  (≈ 0.05 amplitude); bass swells line density + cloud breath; high
  sharpens line edges; per-player energy lifts each cloud
  independently. Footnote chips pop in as discrete events (not a smooth
  ramp). Could reach 5 with explicit "hold" beats between word reveals.
- **d. Abstract not literal — 5/5.** No literal scoreboard / EKG /
  spectrum bars / face. Gradient clouds + analytical curves + numbered
  footnotes are the *grammar* of editorial design, not depictions. The
  reference's wisdom-and-love duality is felt as composition (two
  curves crossing through a layered field), not spelled out.
- **e. Surprise / risk — 4/5.** Footnote-on-curve text layout is a
  novel authoring move for this corpus (most text shaders use grid or
  ring layouts). Long-axis warped clouds (instead of radial gradients)
  give the silhouettes their painter-stretch quality. Could push to 5
  by making chip orientations follow the curve tangent (right now
  glyphs are axis-aligned).

**Total: 21/25.**

## Anti-patterns triggered

None detected:
- No scoreboard, no soccer-ball, no pitch outline.
- No sound-wave EKG line (the structural curves are analytical / hand-
  feeling, not driven by FFT amplitude).
- No spectrum-bar default.
- No checkerboard / SDF debug grid.
- No single-color noise plane (composition is layered + framed).
- No mirror-symmetric horizon scene.
- No central logo. Cue text appears as small numbered footnotes, which
  the rubric explicitly allows ("cue text inputs are fine; rendered
  glyphs as decoration are not"). The header title is a small rule-mate
  along the top, not a central focus.

## Caveats / known limitations

- **Footnote glyph rotation.** Chips are axis-aligned for legibility. A
  future pass could rotate them to follow the tangent at their t-value;
  this would lift axis (e) to 5 but cost readability.
- **Curve sampling.** `curveDist` uses N=96 linear samples per pixel ×
  two curves = 192 samples. Fine on modern GPUs; if perf becomes an
  issue, a coarse + refine pass could halve it.
- **Word count cap.** MAX_WORDS=12. Longer utterances will only show
  the first 12 words as footnotes (rest still typed via msgAge but
  silently dropped from chip rendering). Adequate for typical cue
  cadence; raise if needed.
- **Index digit display.** Superscript index shows the LSB digit only
  (1-9, then 0, 1, 2…). Visual rhythm, not a literal counter — fine for
  >9-word messages but worth noting.
- **No raymarch / no DOF blur shader.** Depth is faked via parallax +
  per-layer softness in the falloff exponent. Holds at axis (b) = 4.
- **Helix variant** is the most experimental — under audio peaks it can
  read as slightly less "poster" than the other three. Default ships
  as variant 0 (S-curve) to match the reference.
