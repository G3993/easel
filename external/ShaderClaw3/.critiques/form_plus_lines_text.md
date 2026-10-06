# `form_plus_lines_text` — initial critique

**Slug**: `form_plus_lines_text`
**Reference**: `~/Documents/A-List Shaders/formpluslines_text.jpg` (Beaming
Design "love languages" chart — pastel iridescent auras with dashed contour
drawings inside, italic+roman headline above)
**Status**: first drop, not iterated yet.

## Concept

The reference is a *chart* of 9 motifs — small auras with dashed line
drawings, captioned. Rather than literally re-create the grid (which would
auto-fail axis d as a "diagram"), we collapse it into ONE centerpiece
composition: a raymarched abstract form floating inside a single
iridescent halo, crossed by an editorial line cage that lives on its own
screen-space z-plane, with `cue.latest` typing across the top as an
italic+roman masthead.

The poster idiom (italic lead word + roman remainder) is preserved via the
`italicLead` long input — the first N chars of `msg` render with a 0.22
shear. Lines read as "drawn on glass" because they parallax independently
of the form.

## Channel decomposition

| Element | Channel | Effect |
|---|---|---|
| Sculpture left half | `player[1].energy` (`energyA`) | warps + warms hue + opens halo |
| Sculpture right half | `player[2].energy` (`energyB`) | warps + cools hue + tilts line cage |
| Form A retract | `player[1].active` (`playerActiveA`) | mute → that half retracts |
| Form B retract | `player[2].active` (`playerActiveB`) | mute → that half retracts |
| Halo radius | `audio.bass` (`bassDrive`) | crescendo pulses the aura outward |
| Sub-blob jitter | `audio.mid` (`midDrive`) | inner micro-life |
| Rim sharpness + dash flicker | `audio.high` (`highDrive`) | high-end snap |
| Global audio trim | `audio.level` (`audioDepth`) | rubric anti-mash safety knob |
| Masthead text | `cue.latest` (`msg`, auto-bound) | typewriter at 28 cps |

Five distinct channel binds, two of them on `player[i].*` slots (rubric
hard-floor pass), one on `cue.latest`, three on `audio.*`. Each bind drives
a visually distinguishable axis — muting either player visibly retracts a
side of the sculpture.

## Anti-patterns avoided

- No grid of 9 motifs (would have read as a chart → axis d auto-fail).
- No EKG line, no spectrum bars.
- Masthead text is editorial framing, not the central visual — the
  sculpture and lines own the canvas. The "logo as central visual" anti-
  pattern is avoided because the form, not the text, is the subject.
- Lines are a cage, not a default debug grid; they have density / style /
  dash / parallax controls and bend with `tilt` driven by audio + energyB.

## Rubric self-score (advisory)

| Axis | Score | Rationale |
|---|---|---|
| (a) Multi-player separability | **4** | Two distinct halves on `player[1/2].energy`, plus their `.active` toggles. Could be /5 once a third channel decomposes (say `data.*`); for now 2 halves cleanly muteable. |
| (b) Depth & dimensionality | **5** | Raymarched SDF body + screen-space line cage on its own z-plane + atmospheric haze on raymarch t + lineFog. Genuine layered z. |
| (c) Intentional motion | **4** | Silence → autonomic breath only; energy → spread + halo open; bass → halo pulse; high → dash flicker. Distinct silence/mid/loud states. Holds back from /5 because there's no explicit "drop" choreography (yet). |
| (d) Abstract not literal | **5** | No diagram, no chart, no literal hearts/symbols. Reference is felt as a single sculpted presence rather than a 9-tile grid. |
| (e) Surprise / risk | **4** | Combination is fresh: raymarched 3D form + 2D editorial line cage on a distinct parallax z + italic-lead masthead, all sharing one halo. Holds at 4 because each individual technique exists in the corpus (raymarch in `color_world`, masthead in `forms_text`, dashed lines in `gradient_box_lines_text`); the integration is the move. |

**Estimated total: 22 / 25.**

Hard floor: PASS (5 bound channels incl. 2× `player[*]` + 1× `cue.latest`).
Anti-patterns: none triggered.

## Caveats / known issues to verify on first render

1. The `formSize` slider scales the *pixel* coords fed into the raymarch
   rather than camera distance; at extreme sizes the form can clip the
   right/left frame. May want to wrap as camera distance instead.
2. `lineSDF_hatch` uses approximate length only along v — at extreme tilt
   the line caps may look slightly stretched. Concentric and orbit modes
   are exact ellipse/circle distance.
3. The italic shear is sample-coord (atlas-space), not geometric — glyph
   widths in the masthead aren't widened to account for slant. For short
   leads (<8 chars) this is invisible; for longer leads the trailing
   italic char may overlap the first roman char marginally.
4. Halo color depends on `atan(p.y, p.x)` which has a discontinuity along
   the negative x axis. There's a small angular seam at p.y≈0, p.x<0 that
   is masked by halo softness but visible at low `haloSoftness`.

## Next iteration ideas

- Add a `data.cuePulse` BIND on a third element (e.g. line dash density)
  so axis a hits /5.
- Optional explicit "drop" choreography: at audioBass > 0.8 for >0.2s,
  briefly invert halo polarity → axis c /5.
- Make italic shear geometric (widen cell) for crispness at long
  italicLead settings.
