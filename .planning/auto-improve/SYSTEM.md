# Shader Auto-Improvement — Autonomous System

**Goal**: a system that progressively improves every sub-5★ shader, autonomously, with the user as final arbiter, while we continue raising the rendering-quality floor in parallel.

## Two parallel tracks

### Track A — Content quality (the loop, agent-driven)
A scheduled remote agent runs nightly. Each run:
1. Reads `ShaderClaw3/.shader_ratings.json` (committed in the repo so the cloud agent can see it).
2. Picks the lowest-rated shader (alphabetical tiebreak, skips 5★).
3. Reads `ShaderClaw3/.critiques/<name>.md` if it exists (history of prior attempts).
4. Web-searches reference work for the shader's named artist/movement.
5. Critiques the existing source against the 5-axis rubric (below).
6. Rewrites the .fs file with concrete fixes.
7. Mirrors to `easel/external/ShaderClaw3/shaders/<name>.fs` if needed (submodule).
8. Appends a dated entry to `.critiques/<name>.md`: date, what changed, estimated rating bump, what to study next.
9. Opens a PR titled `auto-improve: <name> — <one-line summary>` against ShaderClaw3 master.
10. Stops. One shader per run.

User reviews the PR in their own time. Merging triggers Easel's hot-reload — they re-rate in Easel; the rating file's next commit feeds the next iteration.

### Track B — Rendering quality (host-side, parallel ship)
Easel's render path keeps improving alongside the content loop. Each landed = every shader gets better at the same time without authors changing anything.

| Phase | What | Status |
|---|---|---|
| Q v1 | 16-bit float composite chain | **shipped** — no banding |
| Q v2 | 2× supersample render → 1× downsample at present | **next** — fixes pixelation feel |
| Q v3 | sRGB-correct compositing (gamma-aware blends) | queued |
| V v1 | `mpPoseLandmarks` etc auto-injected as samplers | **shipped** — shaders compile |
| V v1.1 | Apple Vision face/pose/hand → texture pack | queued |
| V v2 | DataBus numeric extension + binding UI | queued |
| K v1 | EffectKit format for compute / particle pipelines | v2 conversation |

## The 5-axis rubric (used by both human and agent)

Each axis 1–5; total /25 → mapped to 1–5★.

1. **Reference fidelity** — does it actually look like the named artist/movement? (1: generic, 5: unmistakable)
2. **Compositional craft** — color, light, balance, focal point. Painter's eye, not just GPU tricks.
3. **Technical execution** — banding, AA, audio reactivity, clean compile, no obvious sampling errors.
4. **Liveness** — moves with TIME (not just audio gates), has moments, reacts to silence ≠ drop differently.
5. **Differentiation** — feels specific to its reference, not interchangeable with any other generative shader.

## The agent's prompt (committed for review before scheduling)

See `prompts/auto_improve_routine.md`. Self-contained brief the remote agent runs against a fresh clone of ShaderClaw3.

## Files this system touches

| Path | What |
|---|---|
| `ShaderClaw3/.shader_ratings.json` | User's authoritative ratings (committed) |
| `ShaderClaw3/.critiques/<shader>.md` | Per-shader iteration history |
| `ShaderClaw3/shaders/<shader>.fs` | Source the agent rewrites |
| `easel/external/ShaderClaw3/shaders/<shader>.fs` | Submodule mirror (auto on submodule sync) |
| `easel/.planning/auto-improve/PLAN.md` | High-level system design (this directory) |
| `easel/.planning/auto-improve/SYSTEM.md` | This document — autonomous spec |

## What the user does

1. **Rate shaders in Easel** as the primary input signal — the rating file gets committed periodically (manually for now; future enhancement: auto-commit on batched rate-changes).
2. **Review the daily PR** — accept, reject, or comment with redirection ("study Soutine, not Bacon"). The agent's next run reads the comment.
3. **Re-rate the merged result.** If still <5★, the loop runs again on the same shader with the prior critique log informing it.

## Iteration cap

5 attempts per shader. After 5 the agent marks it `human-required` in the critique log and skips on subsequent runs until the user manually clears the marker.

## What this rejects

- **Agent that auto-merges its own PRs.** User judgment is the truth signal; the agent generates candidates, never ships unilaterally.
- **Per-frame critique by Claude during Easel runtime.** Too expensive, too slow. Critique runs once per shader per iteration in the cron run.
- **Replacing the user's rating with the agent's estimate.** Agent estimates are notes, not authority.
- **Trying to automate Phase Q/V (host-side) — those are engineering work that needs review-then-merge.** Track A is for content; Track B is for engineering.
