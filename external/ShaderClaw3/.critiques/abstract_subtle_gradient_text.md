# abstract_subtle_gradient_text — critique

## Concept

Quiet editorial composition over a three-stop muted gradient field. The
reference image is an art-direction brief: a soft, off-center bloom of
warm rose / mint / amber wash, scattered editorial marks (rounded bars, a
tilted outline rectangle, a small four-pointed star, a wavy line, a tiny
dot grid), and a calendar-style numeric arc with a small dated caption.
The shader translates that vocabulary into a three-channel,
restraint-first piece.

## Channel binds (intelligence-layer contract)

- `energyA → player[1].energy` — owns the warm-rose bloom (lower-left, the
  reference's heaviest wash).
- `energyB → player[2].energy` — owns the mint bloom (upper-mid) and the
  arc's slow rotation drift.
- `energyC → player[3].energy` — owns the amber bloom (centre-low) and the
  optional scatter of micro-marks in the Dense variant.
- `audioDepth → audio.level` — global field contrast lift.
- `bassDrive → audio.bass` — weekly ring accents around the arc + a barely
  visible warmth pulse on hits.
- `msg → cue.latest` (text input; auto-bound by Application::loadShader) —
  typewriter caption that follows `msg_len` while live, with `msgAge`
  driving the caret pulse.

## Rubric self-score · /25

| axis | score | rationale |
|---|---:|---|
| a. Multi-player separability | 4 | Three blooms each owned by a distinct
`player[i].energy`, each in a distinct hue, each at a distinct anchor
position. Muting any one player visibly shrinks its bloom to a faint
smudge — A/B-testable by zeroing a channel. Loses one for the blooms
sharing the same Gaussian language. |
| b. Depth & dimensionality | 3 | Three explicit z-planes — gradient
field, abstract marks, arc+caption — each parallax-shifted by the mouse
at a different rate. Fake perspective via z-layered scales and arc
projection. Not raymarched; pseudo-3D. |
| c. Intentional motion | 4 | The `motionGain` curve maps total energy →
[0.15..1.2] so silence is ~15% of normal — visibly *still*, not idle.
Crescendos arrive as bloom warmth and ring pulses rather than thrash.
Holds (silence) read as composition, not absence. |
| d. Abstract not literal | 4 | The reference's calendar is read as an arc
of numerals — recognisable as an editorial calendar but not a literal
day-planner. Shapes are felt as marks, not depicted as objects. Loses
one only because the numeric arc retains some readability. |
| e. Surprise / risk | 4 | The calendar-arc-as-z2 plus the "restraint
curve" (silence = stillness) is a new authoring move in the corpus —
most A-List pieces lean into motion under audio; this one *withdraws*
under silence. The shape vocabulary breaks from the pack's dots/lines
default. |
| **total** | **19/25** | clears hard floor; ≥3 channel binds; no
anti-patterns triggered. |

Anti-pattern audit: not a soccer ball, not an EKG, not a spectrum bar,
not a checkerboard, not noise-plane, not horizon-mirror, and the rendered
text is a small caption (cue-driven, not a logo).

## INPUTS (with BIND)

- `msg` (text) — auto-bound to `cue.latest`
- `energyA/B/C` — `player[1..3].energy`
- `audioDepth` — `audio.level`
- `bassDrive` — `audio.bass`
- Style: `shapeVariant` (Editorial / Sparse / Dense), `palette` (Bloom /
  Cool / Warm / Pastel), `motionSpeed`, `gradientSoft`, `parallax`,
  `textSize`, `paperColor`, `inkColor`, `grain`.

## glslang validation

```
glslangValidator combined.frag  →  exit 0
```

Preamble harness mirrors `ShaderSource.cpp` declarations (TIME, RENDERSIZE,
mousePos, msgAge, fontAtlasTex, audioFFT, audioLevel/Bass/Mid/High,
mpPose/Face/Hand/Segmentation, msg_0..47 + msg_len, plus declared INPUTS).
`gl_FragColor` macro-aliased to `FragColor`. `texture2D` regex-rewritten
to `texture`. Reserved-word collision (`half`) renamed to `hb` in SDF
helpers.

## Caveats / future work

- The arc currently shows 1..30 with weekly bass-pulsed ring accents.
  Could anchor "today" to `transport.time` for a real running calendar.
- Caption uses a single row; multi-line wrap (à la text_clusters.fs)
  could host longer cue utterances without truncation.
- Palette `Bloom` is the reference's voice. `Cool`/`Warm`/`Pastel` are
  extension swatches; each holds the same restraint contract.
- No raymarching — depth is real but pseudo-3D. A future variant could
  parallax the field by sampling fbm at three different z-slices.
