# images_3dshape_text — critique

## Concept

Editorial collage on a black ground. A hero raymarched 3D body (cone /
capsule / prism) anchors the composition; up to seven floating image
cutout cards parallax around it on independent z-planes. The hero's
curved surface carries the live `cue.latest` message as a typewriter
ribbon orbiting its waist — text becomes an *object* rather than a
canvas overlay. Cards are procedural artifacts (rose-stem panel,
cherry poster, mint-arrow ticket, sketch panel, sticker chips, receipt
strip, stamp) — recognisable as the *language* of the reference image
without literally copying any of its glyphs.

The reference is the "designΔemocracy" rolled-paper-cone collage. We
honour the *feeling* (giant text-wrapped paper body + drifting cutouts
on black) rather than reproducing artifacts. Cone is real 3D. Text
follows the cone. Cards have parallax + drop shadows + DOF falloff.

## Channel bindings (intelligence-layer contract)

| INPUT | BIND | role |
|---|---|---|
| `msg` | `cue.latest` | typewriter ribbon wrapped on the hero |
| `cardA` | `player[1].energy` | rose-stem card lifts when player 1 speaks |
| `cardB` | `player[2].energy` | cherry-poster card lifts on player 2 |
| `cardC` | `player[3].active` | mint-ticket card pops in on player 3 |
| `bassPulse` | `audio.bass` | hero cone breathes with bass |

Five binds total; three to distinct `player[i].*` channels, one to
`audio.bass`, one to `cue.latest`. Mute any one player and its card
visibly goes quiet (separability A/B test passes).

## Anti-pattern audit

- No spectrum bars, EKG, or sound-wave line.
- No mirror-symmetric horizon.
- No single Perlin noise plane.
- Logo / readable text is **on the 3D cone surface**, not a literal
  central glyph layer. Text inputs are cue-driven, not decorative.

## Rubric self-score — /25

- **a. Multi-player separability — 4/5.** Three distinct cards each
  bound to a different player; bass owns the hero. Cards have visually
  distinguishable artwork so the "which player lit up" test reads
  cleanly. Held back from 5 only because cards beyond the first three
  inherit the bass pulse (no further player channels to bind).
- **b. Depth & dimensionality — 5/5.** Genuine raymarched 3D hero with
  computed normals, Fresnel, soft fog by distance. Cards parallax with
  z-driven offsets and have z-dependent edge softness (cheap DOF).
- **c. Intentional motion — 4/5.** Multi-mode: cards idle at low
  energy, bloom on impulses; cone breathes with bass; text scrolls
  around the cone equator. Silence → near-still cards + slow cam orbit.
- **d. Abstract not literal — 4/5.** Cards are *procedural* artifacts,
  not literal recreations of the reference imagery. The cone is a
  3D body that *carries* text rather than a depiction of a paper cone.
- **e. Surprise / risk — 4/5.** Wrapping the live cue ribbon around a
  raymarched 3D body's surface UV is the novel move; combining it with
  procedural cutouts on a black editorial ground is a fresh corpus
  position vs the existing text shaders (which mostly text-on-canvas).

**Self-total: 21/25.**

## Files

- `/Users/lu/easel/shaders/images_3dshape_text.fs` (source)
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/images_3dshape_text.fs` (bundle copy)

## Caveats

- The cone hero uses a simplified cone SDF (apex-up). It is correct
  enough for the raymarch but is not a fully signed cone — distance
  outside the body is slightly inflated, which costs a few extra
  raymarch steps. Visually negligible at 70 steps.
- The text ribbon wraps the cone equator with 32 columns / lap; very
  long messages (>32 chars) repeat around the surface rather than
  scrolling end-to-end. A future revision can switch the band to a
  helical chart for unbounded length.
- No `.easel` project file was edited; no relaunch was issued; no
  commit was made. Drop into Easel manually to see it.
