# Critique — `morphing_organic_text`

## Concept

A continuously morphing organic field rendered as a domain-warped FBM that
gets shaped by a reaction-diffusion-feel gain function (smoothstep around two
moving phases). Three players inject moving warp sources in different
canvas slices — left/middle/right anchors with independent drift axes — so
muting one player visibly stills its band of chemistry. A pseudo-3D normal
field derived from the gain gradient produces specular glints; a parallaxed
back shell (warped further, dimmer, slower) gives genuine z depth. The cue
utterance arrives as a typewriter line that sinks into the field with
luminance-adaptive ink color.

## Reference fit

The reference shows pixelated/blocky chemistry-like bands of lime-green
and black flowing as morphing currents across a warm paper backdrop, with
hand-laid text. This shader captures the *essence*: the lime/ink palette
(`palette=0`), the dual-phase ridge seam that reads as fused-organic, the
chemistry-band morph, and the typewriter text contrast-adapted against
the morph beneath. The pixelated blockiness of the reference is
intentionally *abstracted away* — the rubric anti-pattern list rules out
"default checkerboard / SDF debug grid", and the morph here is fluid and
continuous instead of literal pixels.

## INPUTS / BIND

- `msg`        → `cue.latest` (text, MAX_LENGTH 48)
- `energyA`    → `player[1].energy`
- `energyB`    → `player[2].energy`
- `energyC`    → `player[3].energy`
- `activeA`    → `player[1].active`
- `activeB`    → `player[2].active`
- `bassDepth`  → `audio.bass`
- Style inputs (unbound): `morphSpeed`, `warpAmp`, `octaves` (3–6),
  `palette` (4 options), `textContrast`, `textScale`, `zParallax`

Binding floor: 7 channel binds (6 numeric + 1 text). Three distinct
`player[i]` energies feed three regions; two `player[i].active`
gates; one `cue.latest`; one `audio.bass`. Easily clears the floor.

## Rubric self-score — /25

- **(a) Multi-player separability — 4/5** — three players anchor
  three column slices with different drift axes; muting one band
  visibly stills its halo and its warp gust. Halos are color-coded
  (lime / rose / sky) so you can ID which voice went silent. Stops
  short of 5 because the warps cross-pollinate at the seams (which
  is intentional — chemistry, not silos).
- **(b) Depth & dimensionality — 4/5** — z-parallax back shell + a
  pseudo-3D normal field with Lambert + Blinn-Phong specular off
  the gain gradient. Mouse offset nudges the parallax. No raymarch.
- **(c) Intentional motion — 4/5** — silence reads as a slow
  drifting field (still alive, never frozen), energy creates focal
  warp gusts that pull the bands toward each player anchor, and
  fused-halo brightening at multi-player overlap acts as a moment.
- **(d) Abstract not literal — 5/5** — pure abstraction; the
  "voices" are felt as warp sources in a morphing chemistry, not
  drawn as figures, EKGs, spectrums, icons, or grids. The text is
  the only literal element and it's contrast-pressed *into* the
  field, not floated over it.
- **(e) Surprise / risk — 4/5** — reaction-diffusion-feel via
  smoothstep around two moving phases of warped FBM is a less
  common gain shape than the usual fbm-to-color ramp; combining it
  with pseudo-3D normals from the gain gradient (instead of from
  height) gives a chemistry that *feels lit* without ever being 3D.

**Total: 21/25**

## Anti-pattern audit

- No EKG: no scanline / wave traversal.
- No spectrum bars: no per-bin FFT decoration.
- No literal icons: no glyphs except the cue text.
- No default checkerboard / SDF grid: domain-warped FBM with
  rotated octave matrices, no axis-aligned periodicity.
- No mirror-symmetric horizon: aspect-corrected centered field,
  asymmetric player anchors.

## Validation

glslang `-S frag` exit 0 against an Easel-mirrored preamble
(`#version 330 core`, `out vec4 FragColor`, `#define gl_FragColor
FragColor`, `#define texture2D texture`, all ISF builtins and
text-uniform arrays declared).

## Caveats / known trade-offs

- `octaves=6` triggers six FBM calls per pixel; in the gradient
  step that's 4 × FBM × 6 octaves = 24 noise stacks per pixel. On
  retina the back shell adds another ~4. Watch perf on 4K.
- `playerWarp` uses an inverse-square gust capped at 1.0; with all
  three energies at 1.0 the field gets quite churny. That's the
  intent — crescendo — but a user might want a `warpAmp` ceiling
  below 2.5 for legibility on a venue projector.
- Typewriter is single-row, lower-third. Long utterances auto-shrink
  the glyph height to fit aspect-corrected canvas width. No word-
  wrap — by design, the line reads as a caption stamp, not a
  paragraph.
- The fontAtlas character set is 0–35 (A–Z + 0–9) with SPACE=26; any
  punctuation collapses to a blank cell, which is the existing
  Easel/ShaderClaw font convention.
