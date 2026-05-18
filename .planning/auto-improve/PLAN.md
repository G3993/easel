# Shader Auto-Improvement Loop

**Goal**: every shader rated below 5★ gets continuously critiqued and improved until it earns 5★. Loop runs daily (or on-demand), agent-driven, human in the loop for final acceptance.

## Current state (snapshot from `~/.easel/shader_ratings.json`)

| Stars | Count | Notes |
|---|---|---|
| 1★ | 30 | Priority pile — biggest worklist |
| 2★ | 10 | Adjacent improvement candidates |
| 3★ | 11 | Need direction polish |
| 4★ | 7 | Closest to ship-quality (`balls`, `bars`, `dispersion`, `flow`, `liquid_metal`, `Water`, `swirl_spin`) |
| 5★ | 0 | None yet — north star |

## The loop

```
worklist (sorted asc by stars)
   │
   ▼
pick lowest-rated unprocessed shader
   │
   ▼
RENDER snapshot   (Easel takes a screenshot at default params)
   │
   ▼
CRITIQUE          (Claude reads source + screenshot → diagnosis + fixes)
   │
   ▼
APPLY             (rewrite the .fs file with concrete code changes)
   │
   ▼
RE-RENDER         (snapshot again)
   │
   ▼
SELF-EVAL         (Claude compares before/after → estimated rating bump)
   │
   ├─ if ≥5★ confidence → mark "auto 5★" pending user review
   ├─ if <5★            → log critique, queue for next iteration (max 5)
   └─ if regressed      → revert, log "blocked" with reason
   │
   ▼
LOG to ~/.easel/shader_critiques/<filename>.md
   │
   ▼
COMMIT to ShaderClaw3 with structured message
   │
   ▼
next shader
```

## Critique prompt template

Lives at `.planning/auto-improve/critique_prompt.md`. Claude (or any LLM) receives:
- Shader source code (full .fs file)
- Current screenshot (PNG, base64)
- Reference work for the shader's category (e.g. for `futurism_boccioni`: Boccioni "Unique Forms of Continuity 1913", Balla "Dynamism of a Dog on a Leash 1912")
- Current user rating
- Prior critique history (if any iterations exist)

Returns structured output:
```yaml
diagnosis: "2-3 sentences on what's failing"
fixes:
  - line: 42
    change: "swap Voronoi for SDF circle pattern"
    reason: "current technique is anachronistic for early-1910s reference"
estimated_rating_after: 4
confidence: "medium"
study_next: "look at Boccioni 'States of Mind: Those Who Stay'"
```

## Critique dimensions (the rubric)

Every shader is evaluated against five axes. Each axis 1–5; total /25 → mapped to 1–5★.

1. **Reference fidelity** — does it actually look like the artist/movement it claims? (1 = generic, 5 = unmistakable)
2. **Compositional craft** — color, light, balance, focal point. Painter's eye, not just GPU tricks.
3. **Technical execution** — no banding, AA on edges, smooth gradients (Phase Q helps), clean compile, audio actually drives something visible.
4. **Liveness** — does it move? Does it have moments? Does it react to silence vs. drop?
5. **Differentiation** — does this shader feel specific, or could it be any other gl-transitions shader with a different palette?

## Concrete deliverables of THIS first cycle

Already shipped this turn:
- **`futurism_boccioni.fs` rewrite** as the demo iteration. Old approach (frame-feedback motion blur) replaced with the iconic Futurist gesture: SDF subject (body / wheel / leg) repeated at N phase-offset positions along a Lissajous velocity vector, sharp diagonal speed lines, warm sienna palette, fwidth-AA on every edge so it stays crisp on 4K displays. New params: `phantomCount`, `subjectShape` enum, `pathCurve`, `speedLineLength`, etc.

To verify: open it in Easel — should now read as "moving figure with sequential leg-positions" not "blurry smear".

## Next concrete deliverables

1. `scripts/improve_next_shader.sh` — picks lowest-rated shader, opens it in editor, prints critique scaffolding for Claude to fill.
2. `~/.easel/shader_critiques/<filename>.md` — running history per shader (auto-created).
3. `~/.easel/shader_critique_history.json` — structured log for charting (dates, ratings, iteration count).
4. Auto-bump `improvement_candidates` counter in Easel stderr each launch so the user has a daily metric.
5. Optional: `/loop` integration — schedule a daily auto-improve agent that picks the lowest-rated shader, generates a rewrite proposal, commits to a side branch, and the user reviews.

## Open architectural questions

- **Where does the snapshot come from?** Easel's existing thumbnail cache (`m_scThumbnails`) renders each shader at 256×256. Can repurpose that. For higher-fidelity critique, render at 1024×1024 with actual default params, save under `~/.easel/shader_snapshots/`.
- **How does Claude evaluate "looks like Boccioni"?** Same way it does in conversation: visual reasoning over the snapshot. Keep reference images on disk so the critique prompt can include them as comparison.
- **Who breaks ties on rating?** User rating > Claude estimate. When user re-rates, that's the truth.
- **Iteration cap?** 5 attempts per shader. If still <5★, mark "human-required" and surface it for the user.

## Cadence

- **Daily**: 1–2 shaders processed in background by a `/schedule`'d agent.
- **Weekly**: review accumulated improvements, accept/reject, update master ratings.
- **Quarterly**: prune the catalog. Anything stuck below 3★ for 90 days gets culled.
