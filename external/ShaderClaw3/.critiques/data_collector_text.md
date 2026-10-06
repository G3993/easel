# data_collector_text — telemetry deck (parallax columns + heatmap + live log)

## Reference
`/Users/lu/Documents/A-List Shaders/data_collector_text.jpg` — a
data-collector / dossier poster. A green frame around an analytic canvas
filled with: a hot/cold heatmap cloud blooming across the page, dense
columnar values, `_01`/`_02` ticker labels, dotted scatter, hashtag-led
sub-modules (`# REDES SOCIALES`, `# DISOCIACIÓN`, `# ATRAPADOS`), running
"Hostname/User" tables, "DOSIS DE DOPAMINA" callouts, percentage stacks
(`3.50`, `82%`), and a vertical timeline of timestamps. The feeling is
*the system is collecting*: lots of streams, lots of small numbers, lots
of overlapping reads — abstract surveillance lyric, not a literal HUD.

## Concept

A parallax telemetry deck. Three (configurable 2–6) vertical columns of
synthetic glyph-streams scroll at independent speeds and depths;
**each column is its own data feed**. The deck floats over a thin
dossier grid + parchment fbm marbling, with a thermal heatmap cloud
(three drifting gaussians, audio-modulated) blooming through the
composition. The current cue utterance lays down as the **live log
entry** at the lower third, typewriter-revealed via `msgAge`, with a
blinking caret on the write-head.

Each column has its own personality (per-column letter-vs-digit bias):
some read as timecode/percentage streams, others as hashtag/word
streams. Per-column `player[i].energy` modulates scroll velocity and a
fresh-row glow; `player[i].active` *freezes* the column when the player
goes silent, so muting a player is a visible compositional event, not
just an alpha fade. `audio.level` drives the heatmap intensity;
`data.entropy` shows up as visible chromatic grain — uncertainty made
literal.

Front-to-back depth: columns at different z get different scale, alpha,
and parallax velocity; closer columns occlude farther ones via alpha-
over composition. Glyph edges are fwidth-AA'd; scanline ticks every
frame so motion is non-zero even at silence.

## INPUTS & BIND

| input | bind | role |
|---|---|---|
| `msg` (text) | `cue.latest` | live log entry; typewriter via `msgAge` |
| `energyA` | `player[1].energy` | col 1 scroll velocity + freshness glow |
| `energyB` | `player[2].energy` | col 2 scroll velocity + freshness glow |
| `energyC` | `player[3].energy` | col 3 scroll velocity + freshness glow |
| `aliveA` | `player[1].active` | col 1 freeze gate (mute → frozen column) |
| `aliveB` | `player[2].active` | col 2 freeze gate |
| `aliveC` | `player[3].active` | col 3 freeze gate |
| `heat` | `audio.level` | heatmap cloud intensity |
| `datNoise` | `data.entropy` | chromatic grain floor (uncertainty) |
| `columnCount` (long) | manual | 2–6 columns |
| `scrollSpeed` (float) | manual | global tick velocity |
| `fontSize` (float) | manual | glyph scale |
| `audioDepth` (float) | manual | per-band heatmap weighting |
| `palette` (long) | manual | Thermal / Mono Dossier / Cyan Console / Acid |
| `gridDensity` (float) | manual | dossier grid amount |
| `vignette` (float) | manual | corner falloff |

Local var `alive` is used in code because GLSL reserves `active`; the
*channel name* on the wire stays `player[i].active` per the
intelligence-layer contract.

## Anti-pattern audit (RUBRIC.md)

- ❌ EKG line — none; no horizontal waveform across canvas.
- ❌ Spectrum bars — none; no regular vertical bar grid mapped to FFT.
- ❌ Literal icons — none; only synthetic glyph streams + heatmap blobs.
- ❌ Default checkerboard — grid is faint, asymmetric (16×18), and
  modulated by parchment fbm, not a stark checker.
- ❌ Single-color noise plane — multi-layer composition: bg + heatmap +
  3+ parallax text columns + live caption + scan + grain.
- ❌ Mirror-symmetric horizon — columns are deterministically spread by
  hash, not mirrored.
- ❌ Logo / readable centre — `msg` is short caption, not the central
  decoration; main visual is the abstract column field.

## Rubric self-score /25

- (a) Multi-player separability — **5/5**. Three independent column
  channels with distinct freeze gates; muting `player[2].active` halts
  column 2's scroll and dims its freshness glow while 1 and 3 keep
  running. Plus `data.entropy` and `audio.level` as additional
  visually-distinguishable feeds.
- (b) Depth & dimensionality — **4/5**. Pseudo-3D parallax columns with
  distinct z (scale, alpha, velocity per column), atmospheric fade,
  alpha-over compositing front-to-back. No raymarch — pure 2D z-stack —
  so caps below 5.
- (c) Intentional motion — **4/5**. Frozen-column behaviour gives
  silence a *shape* (specific column halts); energy spikes glow the
  freshest rows; heatmap blooms with audio bands; scanline ticks every
  frame. Composes in time across silence ↔ crescendo.
- (d) Abstract not literal — **5/5**. No EKG, no bars, no icons. The
  feeling of "the system is collecting" via abstract glyph streams +
  heatmap haze, not a literal dashboard.
- (e) Surprise / risk — **4/5**. Frozen-on-mute as a compositional
  device (rather than alpha fade) is novel for the corpus; thermal
  heatmap layered through scrolling text is unusual; per-column
  letter/digit bias gives streams character without literal labelling.

**Estimated total: 22/25.** Hard floor passed (7 channel binds, 3
`player[*]`, 1 `audio.*`, 1 `data.*`, 1 `cue.*`).

## Files

- `/Users/lu/easel/shaders/data_collector_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/data_collector_text.fs`

## Caveats / known limits

- Synthetic glyphs are deterministic noise, not real telemetry; the
  shader is the *feeling* of telemetry, not telemetry itself. If/when
  `data.*` numeric feeds become bindable to text-input glyph slots
  (not yet supported on float→text in INPUTS), columns could render
  real values.
- `data.entropy` is one of many possible `data.*` keys — author intent
  is "user pushes any 0..1 uncertainty signal here". Empty `data.*`
  feeds default to 0.55 so the shader still grains nicely cold.
- Mono Dossier palette is intentionally the only paper-mode; the
  caption plate inverts there. Switch palette → entire deck retones.
