# connectnumbers_text — self-critique

## Reference image
`/Users/lu/Documents/A-List Shaders/connectnumbers_text.jpg`

A "connect-the-dots" planner page: numbered dots (01..55) scattered across the
canvas, joined by hand-drawn pen lines into a loose contour, with brush
gestures (mouth, eyes, antennae) and a tiny month-of-year colophon underneath.
The numbers are the structural conceit — they sequence the eye through space.

## Concept

**A floating data-graph in pseudo-3D depth.** Three parallax planes, each a
Voronoi-relaxed jittered-grid swarm of nodes drifting on slow orbits. Edges are
the *deterministic two-nearest neighbours* of each node (Delaunay-ish without
the storage cost), composed into one combined web via smooth-min on the segment
SDFs so the graph reads as one fwidth-AA sheet of capsules. Every node carries
a two-digit numeric label drawn from atlas indices 27..36, and the labels
twitch between values — slowly when the corresponding `player[i].energy` is
silent, frantically when the player is hot. A typewriter telemetry line at the
bottom reveals `cue.latest` letter-by-letter via `msgAge`.

The reference's *numbers + connecting lines* idea is preserved abstractly: the
graph is not data, the numbers are not values, the edges are not relationships.
It is the *feeling* of a network listening.

## Bindings

| INPUT       | TYPE  | BIND                |
|-------------|-------|---------------------|
| `msg`       | text  | `cue.latest`        |
| `energyA`   | float | `player[1].energy`  |
| `energyB`   | float | `player[2].energy`  |
| `energyC`   | float | `player[3].energy`  |
| `activeA`   | float | `player[1].active`  |
| `activeB`   | float | `player[2].active`  |
| `bassDrive` | float | `audio.bass`        |

Six channel-bound INPUTS plus the text input. Three distinct player slots
drive three independent planes — silencing one plane is observable (its nodes
stop pulsing, its numbers stop ticking, its tint fades to ink). Binding floor
met (≥2 player bindings, plus cue.latest, plus audio.*).

## Style INPUTS (non-bound, user-tweakable)

`nodeCount, lineWidth, nodeRadius, motionSpeed, audioDepth, depthAmount,
labelScale, palette, paperColor, inkColor` — 10 controllables (≥3 required).

## Anti-patterns checklist

- EKG sound-wave line: **NO** — there is no horizontal waveform.
- Spectrum bars: **NO** — bass drives node radius / edge glow, not bars.
- Literal icons / readable logo: **NO** — telemetry text is a side band, not
  the central visual; the graph is the subject.
- Default checkerboard / SDF grid: **NO** — paper backdrop is warm with
  vignette + grain; no debug tile.
- Single-color noise plane: **NO** — composition is structured around graph
  edges + nodes.
- Mirror-symmetric horizon: **NO** — Voronoi swarm is asymmetric by design.
- Literal scoreboard / face / soccer ball: **NO**.

None triggered.

## 5-axis self-score /25

| Axis | Score | Rationale |
|------|------:|-----------|
| (a) Multi-player separability | **4** | 3 planes, 3 player binds, each plane has its own tint, pulse-rate, and label-twitch rate. Mute player 2 and plane 2's numbers freeze and tint dims to ink. Distinct languages per plane; not a 5 because the *spatial* separation is co-mingled (planes overlap in screen space rather than living in 3 corners). |
| (b) Depth & dimensionality   | **4** | Three parallax planes with depth-driven scale (`mix(0.78, 1.10, depth)`), edge thickness (`mix(0.65, 1.15, depth)`), label size, and offset push. Closer planes win in z. Not raymarched, but the parallax + size grading reads as space, not as flat layers. |
| (c) Intentional motion       | **4** | Silent state: nodes orbit slowly at `rest=0.12`, labels barely tick (0.6 ticks/s). Loud: nodes pulse, halos bloom, numbers race at 6.6 ticks/s, edge thickness lifts on bass. Three energy curves give three motion modes. Holds at silence (rest energy is gentle, not zero), crescendos arrive on energy spikes. Not a 5 because there is no explicit "drop" composition event. |
| (d) Abstract not literal     | **5** | Numbers are not data, edges are not graph relationships, planes are not actual depth. The reference's *idea* of "connect the dots" is preserved as a felt mechanic, not depicted. No literal icons. |
| (e) Surprise / risk          | **4** | Move that's new for this corpus: 2-digit numeric labels stuck to every node, driven by a hash-of-time so they tick semi-randomly — gives the graph a "telemetry / readout" feel that nothing else in the corpus has. K-nearest edge selection via smooth-min capsule SDF is also unusual in 2D ISF. Not a 5 because the underlying technique (parallax planes + Voronoi swarm) is in-corpus. |

**Total: 21 / 25.**

Hard floor passed (7 channel bindings present, well above the 0 cutoff).

## glslang validation

Validated against the Easel preamble harness (`#version 330 core`, `FragColor`,
`texture2D→texture`, all ISF builtins, `msg_0..47 + msg_len`, `fontAtlasTex`,
all declared INPUTS as uniforms typed per ShaderSource.cpp). **glslang exit
code 0, no warnings.**

## Next-iteration ideas

- Pull the telemetry strip up into the graph: have one of the digits at a
  random node *spell out* the next character of `cue.latest`, so a number flips
  to a letter briefly. Crosses the symbolic boundary between "numeric node" and
  "talking node" without being literal text overlay.
- Sequenced connections: when `audio.bass` peaks, briefly highlight the
  k-nearest path **in order** like the planner reference, then drop back to the
  ambient web.
- Replace the digit value with a quantised `player[i].pitch` so the label
  becomes a poor man's pitch readout — readable telemetry that ties tightly to
  the bound channel.
- A fourth plane behind everything — wide, very low contrast — to fake fog
  and push axis (b) to a 5.

## Files

- Shader (source-of-truth):
  `/Users/lu/easel/shaders/connectnumbers_text.fs`
- Shader (app bundle copy):
  `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/connectnumbers_text.fs`
- Critique (this file):
  `/Users/lu/ShaderClaw3/.critiques/connectnumbers_text.md`
- Reference:
  `/Users/lu/Documents/A-List Shaders/connectnumbers_text.jpg`
