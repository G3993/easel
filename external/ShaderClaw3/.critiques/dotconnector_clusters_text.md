## dotconnector_clusters_text — A-List drop

**Reference image:** `/Users/lu/Documents/A-List Shaders/dotconnector_clusters_text.jpg`
(Four editorial posters, each a constellation of black dots connected by thin red lines with a small "a:"/"b:"/"c:"/"d:" caption block in the upper-left. Warm grey paper, gritty concrete vignette.)

### Concept
A constellation of N (3–6) dot-and-connector clusters drifts across a warm paper canvas. Each cluster is a distinct "player" — its own subset of the visual, bound to its own `player[i].energy` and `player[i].active` channels. When player N talks: that cluster's nodes pulse, its bridging lines saturate from muted near-black into the editorial-red of the reference, and the focal-cluster caption typewrites in next to it. Other clusters sit at low rest-energy, still drifting but visually muted. Mute one player and you watch its constellation go still; transitions between speakers are compositional events. Layered parallax: paper grain → drifting dust → connector lines → dots with ring highlights → ink caption. Bass-driven line glow gives global audio liveness without breaking per-player decomposition.

### Channel bindings declared
- `cue.latest` → `msg` (caption text input; auto-bound by Application::loadShader)
- `player[1].energy` → `energyA`
- `player[2].energy` → `energyB`
- `player[3].energy` → `energyC`
- `player[4].energy` → `energyD`
- `player[5].energy` → `energyE`
- `player[6].energy` → `energyF`
- `player[1..6].active` → `activeA..F`
- `audio.bass` → `bassDrive`

Hard-floor satisfied: ≥2 distinct `player[*]` binds (actually 12 across energy+active), `cue.latest` bound, `audio.bass` bound. Six independent per-cluster channel pairs.

### Self-score (5-axis rubric)
- **a. Multi-player separability — 5/5** — Six distinct visual entities, each spatially packed in its own grid cell, each driven by its own `player[i].energy` + `player[i].active` pair. Distinct visual languages (pulse rate per-cluster, line saturation per-cluster, focal caption migrates to the loudest player). Muting one channel literally stops that constellation.
- **b. Depth & dimensionality — 3/5** — Four z-stacked parallax layers (paper grain → dust → lines → dots → text), active clusters scale up to fake forward parallax, inactive recede. Not raymarched; pseudo-3D depth cues only.
- **c. Intentional motion — 4/5** — Quiet state: gentle anchor drift, dots at rest size. Active state: dot pulse, line glow swell, caption typewrites with `msgAge`. Crescendos arrive as moments (focal cluster handoff). Could go to 5 with a punctuated "hold" mid-utterance.
- **d. Abstract not literal — 4/5** — Constellation/dot-graph reads as the *idea* of grouped entities-with-connections rather than literal nodes. Stays clear of soccer/EKG/spectrum-bar anti-patterns. Caption text is structural (cue-driven), not decorative glyphery.
- **e. Surprise / risk — 4/5** — Editorial-poster aesthetic (warm paper + red linework + thin sans caption) is rare in the corpus; the cluster-per-player commitment is the canonical rubric bar. Could push to 5 by introducing a faux-3D ellipse tilt per cluster.

**Total: 20/25**

### Anti-pattern checklist
- EKG sound-wave line: NO
- Spectrum-analyzer bars: NO
- Literal soccer ball / scoreboard / pitch: NO
- Default checkerboard / SDF debug grid: NO
- Single-color noise plane: NO
- Mirror-symmetric beach / horizon: NO
- Logo / decorative readable text as central visual: NO (caption is `cue.latest`, intentional)

### Next-iteration ideas
1. Per-cluster ellipse tilt (faux-3D rotation of the local cluster frame around its anchor) so each constellation looks like a tilted plate floating in z — bumps axis (b) to 4.
2. Punctuated "stillness moments" — when `audio.level` falls below a threshold for > 0.5s, freeze all drift for a beat then resume; bumps (c) to 5.
3. Per-cluster palette variant: alternate red ↔ deep ink connector color by `player[i].pitch` so each speaker carries their own line hue.

### Files written
- `/Users/lu/easel/shaders/dotconnector_clusters_text.fs` (source)
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/dotconnector_clusters_text.fs` (bundle copy)
- `/Users/lu/ShaderClaw3/.critiques/dotconnector_clusters_text.md` (this file)

### Caveats
- Validated against a hand-rolled glslang harness that mirrors Easel's `ShaderSource::translate` preamble (`#version 330 core`, `FragColor`, `gl_FragColor`→`FragColor` macro, `texture2D`→`texture` macro, all ISF builtins, msg_0..47 + msg_len, font atlas, audio uniforms, all 32 declared INPUTS). Compile is clean. Not run in-app this session (no relaunch per constraints), so the only risk is a typing mismatch on a less-common ISF input — but every declared INPUT type appears in existing Easel shaders (`long`, `text`, `color`, `float`).
- Hit one reserved-word collision during authoring (`active` is reserved in GLSL 330 core) — locally renamed to `live`; uniform names `activeA..F` are unaffected.
