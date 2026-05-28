# Shader Auto-Generation Loop — v3

**Mission**: nightly, generate **new** shaders that score high on `RUBRIC.md`,
using a user-provided reference image folder + the existing corpus as inspiration.
**Bias toward surprise. Penalize safety and derivative output.**

v3 replaces the "render in Easel headless" hand-wave from v2 with a real,
unattended-ish render-and-score pipeline. Easel is not relaunched.

## Pipeline

```
shader (.fs)  ──► render_isf.py  ──► rendered.png  (2×2 contact sheet)
                                        │
                       ┌────────────────┘
                       ▼
              score_drop.py
                 │      │
   numeric ─────┘      └──── score.prompt.json  (vision-pass scaffolding)
   (LAB-EMD,                       │
    SSIM,                          ▼
    L delta)                  agent reads:
                                rendered.png + reference + shader + RUBRIC.md
                                writes: score.rubric.json
                       │
                       ▼
              score_drop.py (re-run merges) → score.json (final)
```

Three new tools — all in `/Users/lu/easel/tools/`:

| Tool | What |
|---|---|
| `render_isf.py` | Offline GL3.3 ISF renderer (Python + ModernGL + Pillow). Reads .fs, parses ISF header, builds an Easel-equivalent preamble (mirrors `ShaderSource::translateFragment`), renders fullscreen-triangle pass to an offscreen FBO at 4 TIME values (default 0, 1.5, 3.5, 7) and assembles a 2×2 contact sheet. Writes a sibling `.log` with the full GLSL on compile/link failure. |
| `score_drop.py` | Cheap numeric similarity (LAB-histogram EMD + grayscale SSIM + luminance delta), writes `score.prompt.json` for the agent's vision pass, and assembles `score.json` from numeric + rubric portions. Idempotent — re-running after the agent drops `score.rubric.json` merges them. |
| `eval_drop.sh` | One-shot orchestrator: resolves shader + reference (fuzzy match against `~/Documents/A-List Shaders/`), runs renderer, runs numeric scorer, prints next-step instructions for the agent to do the vision pass. |

## Per-drop artifacts

For every shader scored on a given date, the loop writes:

```
.planning/drops/<YYYYMMDD>/<slug>/
   ├── rendered.png         2×2 contact sheet, 4 TIME samples
   ├── rendered.log         preamble + compile log (debugging)
   ├── score.prompt.json    self-describing brief for the vision pass
   ├── score.rubric.json    agent's rubric output (5 axes + anti-patterns + rationale)
   └── score.json           final merged record — Studio tab consumes this
```

The Studio tab's "Daily Drops" section reads `score.json` and shows `rendered.png`
as the thumbnail; clicking opens the rationale + per-axis breakdown.

## Renderer fidelity

`render_isf.py`'s preamble mirrors `src/sources/ShaderSource.cpp::translateFragment`:
all builtins (`TIME`, `TIMEDELTA`, `RENDERSIZE`, `PASSINDEX`, `FRAMEINDEX`,
`mousePos`, `mouseDelta`, `pinchHold`, `mouseDown`), MediaPipe sampler stubs,
`audioFFT` + `audioLevel/Bass/Mid/High`, `_voiceLevel`/`_voiceGlitch`,
`msgAge`, ISF image stubs (`IMG_SIZE_<name>` + `_flip_<name>`), the IMG_*
macro set, per-input uniforms (`float`/`color`/`bool`/`long`/`point2D`/`text`),
and text inputs compiled to `name_0..name_N + name_len` (same A=0..Z=25 /
space=26 / 0-9=27-36 encoding).

`player[i].*`, `cue.*`, `audio.*`, `transport.beat` binds get synthetic envelopes
per-frame so the visual doesn't look dead.

Font atlas: a procedural 37-cell ramp (A-Z, space, 0-9) generated with PIL using
Arial Bold from `/System/Library/Fonts/Supplemental/`. Visually approximate to
Easel's stb_truetype atlas but readable. Pass `--font-atlas <path>` to override.

## Per-shader iteration loop (closes inside the agent)

```
attempt = 0
while attempt < 3:
    render_isf.py shaders/<slug>.fs → rendered.png
    score_drop.py (numeric)
    agent vision pass → score.rubric.json
    score_drop.py (merge) → score.json
    if score.total >= 18: break
    agent reads (rendered.png, score.json, reference, .fs) and rewrites the .fs
    attempt += 1
```

Cap: 3 implementation attempts per shader. If still < 18, the drop stays in
`drops/<DATE>/<slug>/` and the agent opens a PR with `[low-score]` in the title
for human review.

## What this rejects

- **Auto-merging PRs** — same as v2. User remains final arbiter.
- **Generating shaders without `player[*]` / `cue.*` / `data.*` binds.** Rubric
  hard floor (≤ 10/25) auto-rejects from PR.
- **Per-frame critique during Easel runtime.** Critique runs once per generation
  in the cron job.

## What this honestly cannot do (yet)

- **Single-shader frames at fixed TIME values.** No arbitrary live audio/cue
  inputs — synthetic envelopes only. Real audio reactivity is verified by
  shipping to Easel and listening.
- **No IMG_* texture content** — image inputs are bound to a 1×1 black stub and
  `IMG_SIZE_<name>` is `vec2(0)`. Shaders that gate on IMG_SIZE fall back; shaders
  that unconditionally sample external textures will render incomplete.
- **Font atlas differs from Easel's exact glyph shapes.** Composition reads
  correctly (letters land in right cells); pixel-exact diff against an
  Easel-captured screenshot won't match.
- **Vision-pass is a separate step** that the orchestrating agent performs —
  not fully unattended. Closing this loop is **Phase-2.1** (below).

## Phase-2.1 (next, not yet shipped)

- Drive the agent's vision pass directly from `eval_drop.sh` (call the Claude
  API from inside the script with `score.prompt.json` as the body), so the
  whole pipeline runs as one cron job with zero hand-offs.
- Replace numeric similarity with a real CLIP embedding so the "novel composition"
  axis (e) gets a numerical floor.
- Render at 3 audio energy levels (silent / mid / loud) so axis (c) gets a
  motion-variance number too.

## Schedule

`0 13 * * *` UTC — unchanged from v2.

## Files this system touches

| Path | What |
|---|---|
| `/Users/lu/easel/shaders/<slug>.fs` | shader output (still in repo) |
| `/Users/lu/easel/.planning/drops/<DATE>/<slug>/` | per-drop artifacts (this is new in v3) |
| `/Users/lu/easel/tools/render_isf.py` | offline renderer |
| `/Users/lu/easel/tools/score_drop.py` | scorer |
| `/Users/lu/easel/tools/eval_drop.sh` | orchestrator |
| `/Users/lu/easel/.venv-eval/` | Python venv for the loop (moderngl, Pillow, numpy, scikit-image) |
| `/Users/lu/easel/.planning/auto-improve/RUBRIC.md` | scoring spec (unchanged) |
| `/Users/lu/easel/.planning/intelligence-layer.md` | channel contract (unchanged) |
