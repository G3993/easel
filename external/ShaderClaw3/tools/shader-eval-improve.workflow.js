export const meta = {
  name: 'shader-eval-improve',
  description: 'Score every ShaderClaw3 shader with the headless GPU harness, auto-improve the worst N, verify + keep only real gains',
  whenToUse: 'Run a round of the shader auto-improvement loop. args: {count: N shaders to improve (default 6), focus: "overall"|"audio"|"movement"|"palette"|"density"|"edges" (default overall)}',
  phases: [
    { title: 'Score', detail: 'full harness sweep of all manifest shaders' },
    { title: 'Improve', detail: 'one agent per worst-scoring shader' },
    { title: 'Verify', detail: 'rescore changed shaders; revert regressions' },
  ],
}

const opts = (typeof args === 'string' ? JSON.parse(args || '{}') : args) || {}
const COUNT = opts.count || 6
const FOCUS = opts.focus || 'overall'

phase('Score')
const SWEEP_SCHEMA = {
  type: 'object',
  properties: {
    worst: {
      type: 'array',
      items: {
        type: 'object',
        properties: {
          file: { type: 'string' },
          overall: { type: 'number' },
          weakest: { type: 'string' },
          scores: { type: 'object' },
        },
        required: ['file', 'overall', 'weakest'],
      },
    },
    failing: { type: 'array', items: { type: 'string' } },
  },
  required: ['worst', 'failing'],
}

const sweep = await agent(
`Run the ShaderClaw3 eval harness over the full library and report the worst-scoring shaders.

1. cd /Users/lu/ShaderClaw3 && node tools/eval_harness.cjs --out tools/eval_results_loop.json   (takes several minutes; raise your Bash timeout to 600000)
2. Read tools/eval_results_loop.json. Consider only results with ok:true and scores present.
3. Rank ascending by scores.${FOCUS === 'overall' ? 'overall' : FOCUS}. Exclude files matching /_text\\.fs$|^text_/ from movement-focused ranking only if their movement is intentionally minimal (typography); use judgment.
4. Return the ${COUNT} worst as {file, overall, weakest, scores} where weakest is the lowest-scoring axis name among density/movement/palette/edges${FOCUS === 'audio' ? '/audio' : ''}, plus a list of any files with ok:false as "failing".`,
  { label: 'sweep', phase: 'Score', schema: SWEEP_SCHEMA }
)

log(`Improving ${sweep.worst.length} shaders (focus=${FOCUS}); ${sweep.failing.length} failing outright`)

phase('Improve')
const IMPROVE_SCHEMA = {
  type: 'object',
  properties: {
    file: { type: 'string' },
    before: { type: 'number' },
    after: { type: 'number' },
    kept: { type: 'boolean' },
    change: { type: 'string' },
  },
  required: ['file', 'before', 'after', 'kept', 'change'],
}

const results = await parallel(sweep.worst.map((w) => () => agent(
`Improve the shader /Users/lu/ShaderClaw3/shaders/${w.file} — its weakest quality axis is "${w.weakest}" (current overall ${w.overall.toFixed(1)}/10, scores: ${JSON.stringify(w.scores)}).

FIRST read /Users/lu/ShaderClaw3/docs/AUDIO_REACTIVITY_PLAYBOOK.md and /Users/lu/ShaderClaw3/AUTONOMOUS_LOOP.md (fix templates per axis).

Axis recipes:
- density low → richer structure: secondary noise/detail layer, better value distribution (avoid mostly-empty or blown-out frames)
- movement low → add slow autonomous drift + audio time-warp clock (musicTime += dt*(0.5+1.2*audioEnergy)); every scene must live even in silence
- palette low → replace flat/monochrome output with a designed palette (or audioPalette(t) anchors); avoid rainbow cycling
- edges low → add a contour/line/detail layer or sharpen existing forms
- audio low → apply the playbook routing table (bass→scale/weight, beat→events with decaying life, highs→sparse sparkle) with soft knees and 0.85-smoothing feel; keep depth ≤30%

RULES: preserve the shader's identity — refine, don't redesign. GLSL ES 1.0 only (texture2D, constant loop bounds, no bitwise/uint/%/round/transpose/texelFetch). Keep it mobile-cheap: no loops >128 iterations, no new samplers beyond what exists.

VERIFY: cd /Users/lu/ShaderClaw3 && node tools/eval_harness.cjs --only ${w.file} --out /tmp/improve_${w.file}.json — must be ok:true and scores.overall should beat ${w.overall.toFixed(2)}. Iterate up to 3 times. If you cannot beat the baseline, revert with: git -C /Users/lu/ShaderClaw3 checkout -- shaders/${w.file} and report kept:false.

Return {file, before: ${w.overall.toFixed(2)}, after: <new overall>, kept: <bool>, change: <one-line description>}.`,
  { label: `improve:${w.file}`, phase: 'Improve', schema: IMPROVE_SCHEMA }
)))

phase('Verify')
const kept = results.filter(Boolean).filter((r) => r.kept)
const reverted = results.filter(Boolean).filter((r) => !r.kept)
log(`kept ${kept.length}, reverted ${reverted.length}`)

return {
  focus: FOCUS,
  improved: kept,
  reverted: reverted.map((r) => r.file),
  failingOutright: sweep.failing,
}
