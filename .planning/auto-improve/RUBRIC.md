# Shader Rubric — v2

Five dimensions, 0–5 each, total **/25**. The loop self-scores; the user can
override any axis in the Studio tab. Overrides calibrate the loop's self-scorer over
time.

## The five axes

### a. Multi-player separability — /5

Does the visual decompose into N independent entities, regions, or layers, each
driven by its own channel (typically `player[i]` or `data.*`)?

- **0** — monolithic; one signal modulates the whole image.
- **1** — two visual elements but both driven by `audio.level` / same channel.
- **2** — N entities visually distinct, only one is actually bound to a player channel.
- **3** — N entities, N independent channel binds, but responses look similar.
- **4** — N entities with distinct visual languages, each responding to its own channel.
- **5** — N entities so cleanly separated you could mute one player and immediately
  see which entity went silent; transitions between "this player" and "that player"
  read as a compositional event.

Measurable: count of distinct `player[*]` / `data.*` channel binds with **visually
distinguishable output** in the screenshot (loop can A/B test by zeroing a channel).

### b. Depth & dimensionality — /5

Is there genuine spatial depth — parallax, raymarching, perspective, layered z?

- **0** — flat 2D, no parallax, no occlusion.
- **1** — two layers with simple alpha.
- **2** — multi-layer parallax driven by motion.
- **3** — pseudo-3D (fake perspective, gradient depth cues).
- **4** — raymarched or genuinely 3D with coherent lighting.
- **5** — raymarched/parallax with depth-of-field, fog, or layered z that reads as
  *space* — you could orbit it mentally.

Measurable: presence of raymarch loops, depth-blur, parallax (multiple `gl_FragCoord`
transforms with different speeds), or perspective transforms in the source.

### c. Intentional motion — /5

Is movement crisp and varied across energy levels — stillness ↔ crescendo — not
loop-y idle?

- **0** — pure idle drift; same motion regardless of input.
- **1** — single response curve (louder = faster).
- **2** — distinct quiet vs. loud states, no in-between.
- **3** — multi-mode motion (calm / build / drop) with audible transitions.
- **4** — motion has its own moments: holds, swells, surprise stops.
- **5** — silence reads as **intentional stillness**, not absence; crescendos arrive
  as moments, not gradients; the visual *composes* in time.

Measurable: variance of frame-difference across silence / low-energy / high-energy
samples; the loop renders the shader against a 3-segment synthetic audio feed
(silent / mid / loud) and measures motion energy at each.

### d. Abstract not literal — /5

Does the shader represent the *essence* of the data rather than depicting the thing?

- **0** — literal depiction (a soccer ball, a waveform EKG, an equalizer, "audio
  spectrum" bars, an actual face).
- **1** — stylized version of the literal thing.
- **2** — symbolic (an icon-like representation).
- **3** — abstract but the source is still readable.
- **4** — abstract; the source is felt, not seen.
- **5** — pure abstraction; the *feeling* of the data is the entire image.

Auto-fail to 0 if the LLM judge (or a CLIP classifier) identifies the screenshot as
one of the anti-pattern templates below.

### e. Surprise / risk — /5

Is the result something the rubric author wouldn't have predicted from the brief?

- **0** — generic ISF decoration; could be from any pack.
- **1** — competent but expected.
- **2** — one mild surprise (a color choice, a motion idea).
- **3** — a composition or palette I haven't seen in this corpus.
- **4** — a technique used unexpectedly (raymarch in a 2D piece, or a 2D trick used
  to fake depth in a novel way).
- **5** — a chord struck — at least one of: composition, palette, motion, or
  technique that meaningfully extends the corpus. A new authoring move.

Measured by: nearest-neighbor distance to the existing corpus (embed all .fs files,
embed the candidate, distance below threshold → derivative; above → surprise).

## Hard floor — binding-less shaders score ≤ 10/25

If a shader has **zero** `player[*].*` / `cue.*` / `data.*` bindings, **total score
is capped at 10/25 regardless of axis scores.** This enforces the intelligence-layer
contract — shaders that don't speak the channel language can't graduate.

`audio.*`-only shaders are *not* binding-less (they pass the floor), but axis (a) is
also capped at 1 for them (because audio.* doesn't decompose by player).

## Hard floor — illegible text shaders score ≤ 10/25

Only applies to shaders that declare a `msg` text input (the user-visible utterance
hook). The eval orchestrator runs `tools/check_text_legible.py` against the rendered
PNG before the rubric pass; the vision agent answers one structured question — is the
rendered text readable in the orientation a human expects? Five verdicts:

| Verdict | text_legible | Caps total? |
|---|---|---|
| `PASS` | true | no |
| `BACKWARD_X` (mirrored on X, reads right-to-left or each glyph reversed) | false | yes, ≤ 10/25 |
| `FLIPPED_Y` (upside-down) | false | yes, ≤ 10/25 |
| `GARBLED` (atlas cells wrong, glyph fragments unrecognisable) | false | yes, ≤ 10/25 |
| `NO_TEXT_VISIBLE` (expected text didn't render at all) | false | yes, ≤ 10/25 |
| `N/A` (shader has no `msg` text input) | true | no |

The cap is applied **after** the rubric agent scores the visual axes — `score_drop.py`
records the original rubric total, overwrites `total` to ≤ 10/25 when the gate fails,
and appends a `[text-legibility cap]` line to `rationale` so calibration logs
remember why the score was held back. Visual axes (a-e) are NOT zeroed — illegible
text shaders can still report their decorative quality, but they can't graduate.

This mirrors the binding-floor cap: both express the same idea — shaders that fail
a baseline contract (channel binds, or readable speech) can't ship, regardless of
how pretty the decoration is.

Tolerance: designs that rotate glyphs along a curve (spiral, circular cards) PASS
when glyph orientation matches the curve's local frame. Pixel-glitch effects PASS
when the underlying letters are upright (even if fragmented).

## Anti-pattern auto-fail list

Any of the following auto-fails axis (d) to 0 and caps total at 8/25 (so the loop
can still learn from the artifact, but it won't open a PR):

- **Literal soccer ball / scoreboard / pitch outline**
- **Sound-wave EKG line** across the canvas
- **Spectrum-analyzer bars** (the default ISF "I have audio" output)
- **Default checkerboard / SDF debug grid** as the dominant texture
- **Single-color noise plane** (Perlin/simplex with no compositional structure)
- **Mirror-symmetric beach / horizon scene** unless symmetry is the explicit subject
- **Logo / readable text** as the central visual (cue text inputs are fine; rendered
  glyphs as decoration are not)

Detection: LLM judge prompt + simple structural heuristics
(brightness symmetry test, horizon-line test, regular-bar-frequency test in the
rendered screenshot).

## Scoring outputs

For each candidate the loop writes:

```jsonc
{
  "slug": "…",
  "scores": { "a": 4, "b": 5, "c": 3, "d": 4, "e": 5, "total": 21 },
  "rationale": { "a": "…", "b": "…", "c": "…", "d": "…", "e": "…" },
  "anti_patterns_triggered": [],
  "hard_floor_passed": true,
  "user_override": null  // filled in later from Studio tab
}
```

## Calibration

When the user overrides a score in the Studio tab, the delta is logged. Once there
are ≥ 20 overrides, the loop's self-scorer prompt is updated with example
(self-score → user-score) pairs so future scoring drifts toward the user's taste.
Until then, self-scores are advisory only — the user is the ground truth.
