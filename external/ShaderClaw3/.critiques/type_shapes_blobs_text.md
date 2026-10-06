## 2026-05-20
**Prior rating:** 0.0★ (new shader)
**Approach:** Serif-typewriter + stepped-rectangle ridges + ovoid blob chorus on a warm-paper z-stack; reference `type_shapes-blobs_text.jpg` (consciousness-manual page "in the space between two thoughts lies the garden …")
**Critique:**
1. **Reference fidelity** — Reference shows lines of serif type alternating with stacked rectangle silhouettes ("ridges") and a horizontal chorus of fused ovals ("the [ooooo] garden / of bliss [oooo] easily"). Shader mirrors this: 8 baselines, each carrying a ridge of up to 12 stepped rectangle cells (gate-opened by player A energy + per-cell seed → quiet lines stay near-flat, loud lines build the stepped silhouette from the page) AND a horizontal smooth-union'd oval chorus drifting in front of the type. Word-wrapped `msg` types into the same 8 baselines via `msgAge`, alternating horizontal offset so words and ridges trade places line-by-line.
2. **Compositional craft** — Warm-paper backdrop with fbm grain, vignette, low-frequency tonal wash; four palette modes (Ink/Paper, Bone/Plum, Cream/Slate, Carbon/Bone) cover four moods. Three z-planes: paper (back, hazy gradient) → ridges (mid, ink) → blob chorus + type (front, with under-cluster soft shadow + tiny inner specular tick). Audio-depth wash and a single rear bass-accent bar give a discreet "music is happening" cue without becoming a spectrum.
3. **Technical execution** — Premium fwidth-AA on every silhouette: rectangle SDF for ridges, scaled-circle oval SDF for blobs (smooth-union with k=0.018 so the chorus reads as one fluid form), real shadow occlusion under the cluster, gentle filmic compress at the end. Word-wrap pre-pass per line; typewriter reveal at ~26 cps; line-weight breathes on player C. No anti-pattern silhouettes (no EKG, no spectrum bars, no checkerboard, no horizon, no logo).
4. **Liveness** — Six channel binds: `energyA → player[1].energy` (ridge gate + step heights), `energyB → player[2].energy` (blob lift + accent tint), `energyC → player[3].energy` (line weight + kerning), `activeA → player[1].active` (ridge accent push), `activeB → player[2].active` (blob accent push), `audioDepth → audio.level` (front sheen + rear band). `msg → cue.latest` typewriter. Mute player 1 → ridges fall flat; mute player 2 → blob chorus settles to a calm line; silence → intentional stillness with paper grain only.
5. **Differentiation** — Distinct from prior corpus: no other A-List shader couples a stepped-rectangle "typographic ornament" ridge to a fused-oval chorus on shared baselines. Consciousness-manual register is shared with `forms_text.fs` but the formal vocabulary (rectangles + ovals + serif word-wrap, not auras) is non-overlapping. Avoids the auto-fail list cleanly.

**Inputs with BIND:**
- `energyA` → `player[1].energy` (ridge step gate + heights)
- `energyB` → `player[2].energy` (blob chorus lift + accent)
- `energyC` → `player[3].energy` (type line weight + kerning)
- `activeA` → `player[1].active`
- `activeB` → `player[2].active`
- `audioDepth` → `audio.level` (front sheen + rear bass band)
- `msg` → `cue.latest` (typewriter)

**Style inputs:** `blobCount` (3..8), `blobSize`, `kerning`, `motionSpeed`, `palette` (4 modes), `paperTone`.

**Validation:** glslang clean against the Easel ISF preamble harness (`/tmp/easel_validate/type_shapes_blobs_text.frag`); exit 0, zero warnings.

**Self-score (out of 25):**
- a) Multi-player separability: 5 (3 player.energy + 2 player.active binds; mute any one of the three players and a distinct visual element settles)
- b) Depth & dimensionality: 4 (three z-planes with parallax + under-cluster soft shadow + atmospheric front sheen; pseudo-3D, not raymarched)
- c) Intentional motion: 4 (energy-gated ridge cells produce true silence→build→swell behaviour; type reveal is composed in time via msgAge; bass band only blooms when bass crosses a soft floor)
- d) Abstract not literal: 5 (no logo, no EKG, no bars; type is cue text not decoration; ridges and blobs are abstract typographic ornaments, not depictions)
- e) Surprise / risk: 4 (stepped-rectangle "typographic ridge" as a player.energy gate, fused-oval chorus on the same baseline rhythm — a new authoring move for the corpus)
- **Total: 22 / 25**

**Files written:**
- `/Users/lu/easel/shaders/type_shapes_blobs_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/type_shapes_blobs_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/MacOS/shaders/type_shapes_blobs_text.fs`

**Caveats:**
- No `.easel` project edits, no relaunch, no commits per instructions.
- The shader font atlas is the standard Easel 37-glyph monospace; the reference's true serif weight is suggested by line-weight + paper warmth, not literal glyph swapping (Easel has no serif atlas).
- The "caret" hook on the active typewriter line is intentionally a no-op block — the live reveal already reads alive; adding a vertical caret bar would compete with the ridges visually.
- Ridge cells use deterministic per-line/per-cell seeds; if the user wants a different rhythm, the seed scaling constants (`7.13`, `3.71`) are the knobs.
