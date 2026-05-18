# Self-Improvement Loop — Continuation Plan

**Last paused:** 2026-05-05 ~10:13 EDT, 59 open PRs, highest #61.

## Loop state

- **Trigger ID:** `trig_01RbpSu8a47QVkvP935wZAKF`
- **Cron schedule:** `0 13 * * *` UTC (09:00 EDT daily) — continues automatically.
- **Manual fire:** `RemoteTrigger run trig_01RbpSu8a47QVkvP935wZAKF` via the claude.ai MCP.
- **Repo:** https://github.com/G3993/ShaderClaw3 (PRs queue against `master`).

## Current cron prompt (already deployed)

- Up to 12 shaders per run.
- **No dedup-by-branch** — already-PR'd shaders are eligible again.
- **v2/v3/v4 branch suffixes** auto-increment when today's branch already exists.
- **Mandatory new-angle rule:** each PR must differ on at least one axis (2D→3D, lighting style, reference work, composition, color grading).
- **Anti-washout rules:** HDR peaks 2.0+ linear, fully saturated palette, strong silhouette, ink contrast, 4–6 colors max.
- 5-axis critique appended to `.critiques/<filename>.md` per shader.

## Continuation steps (in fresh session)

1. **Check current PR count:**
   ```bash
   gh pr list -R G3993/ShaderClaw3 --json number,createdAt --limit 200 \
     | python3 -c "import json,sys; d=json.load(sys.stdin); print(f'{len(d)} PRs, highest #{max(p[\"number\"] for p in d)}')"
   ```

2. **Fire 4–6 cloud runs in parallel:**
   ```
   RemoteTrigger run trig_01RbpSu8a47QVkvP935wZAKF  (×6)
   ```
   Each run produces up to 12 PRs. The v2/v3 branching prevents collisions across parallel runs.

3. **Schedule wakeup loop** (15–25 min cadence) to re-fire and check stagnation:
   - If highest # increased → fire 2 more, schedule next wakeup.
   - If 3 consecutive wakeups produce 0 new PRs → STOP, worklist exhausted.

4. **When ready to merge:** triage PRs at https://github.com/G3993/ShaderClaw3/pulls. Many overlap with local rewrites already in `/Users/lu/ShaderClaw3/shaders` (uncommitted). Pick the version of each conflict shader you prefer; rebase or cherry-pick.

## Known overlaps (local vs. remote PRs)

These shaders have BOTH local rewrites (this session, in working tree) and cloud PRs:
- popart_lichtenstein, abex_pollock, sonoluminescence, swirl_spin, laser_labyrinth, liquid_ripples_3d (deleted), chladni_figures, color_picker, edges, expressionism_kirchner, dada_hoch, art_nouveau_mucha, constructivism_lissitzky, memphis_primitives, solar_flare_corona

## Rate-up rule for stale shaders

If the loop stagnates (same PRs being re-PR'd), the user can:
- Edit `.shader_ratings.json` locally → push → next cron run picks fresh targets.
- OR merge a batch of PRs → ratings drop those targets from "low-rated" pool.

## Tooling state

- Easel app builds clean, 134/139 shaders pass. 4 pre-existing failures unrelated.
- Phase Q v4 bloom pipeline live + soft-knee tonemap in passthrough.frag (whites preserved).
- Phase M.0–M.4 Metal abstraction + GL/Metal backends shipped (test_render_backend passing).
- Custom shader import button live in ShaderClaw panel.

## Stop condition

User explicitly says "stop the loop" — OR — 3 consecutive wakeups with 0 new PRs → autostop.
