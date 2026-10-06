## 2026-05-20
**Prior rating:** 0.0★ (new shader)
**Approach:** 3D-projected SDF aura clusters in a poster grid; consciousness-manual reference (`forms_text.jpg`, "DIFFERENT FORMS")
**Critique:**
1. **Reference fidelity** — Reference shows six soft luminous auras in a 3x2 grid with a sans-serif headline. Shader mirrors this: 3..6 aura clusters laid out by formCount in a 3x2 (or 2x2 / 1x3 / 5-balanced) grid, with a typewriter masthead at the top reading cue.latest. Each aura is a smooth-min of sub-blobs in 3D, projected to a Gaussian halo + inner kernel — matches the "compact colored body inside a soft glow" of the reference.
2. **Compositional craft** — Poster idiom preserved: paper backdrop with marbled noise, vignette, scratches, grain; centered masthead band; per-form caption strips. Forms breathe at distinct phases so the grid never reads as static. Palette modes (Aura warm / Cool tide / Sunset / Mono ink) cover four moods without leaving the gallery register.
3. **Technical execution** — Sub-blobs accumulated in 3D (rotated around cluster y-axis), projected to screen with parallel projection + depth-tinted radius for visible volume; halos additively accumulated with Gaussian falloff, kernel taken as max over forms with z-bias so near forms occlude their neighbors. 8-step rotation provides real internal depth per cluster. Three form variants: Aura blobs, Ribbon clusters, Folded sheets — each variant rewrites sub-blob layout topology.
4. **Liveness** — Five `player[1..5].energy` binds give each form its own audio channel (mute one and it visibly settles); form #6 binds to `audio.bass`. `audio.mid` jitters sub-radii; `audio.high` sharpens fresnel rim; `audio.bass` swells global halo. Silence → slow breath only. Energy → spread + brightness + faster orbit. `cue.latest` types out the masthead at 28 cps via `msgAge`.
5. **Differentiation** — Distinct from prior corpus: no other A-List shader uses 3D-projected sub-blob aura clusters in a poster grid; metaphysical-manual reference is novel. Avoids EKG / spectrum bars / icons / checkerboard / mirror-horizon anti-patterns. Captions are tiny abstract glyph-strips, not literal labels.

**Inputs with BIND:**
- `energyA..E` → `player[1..5].energy` (5 player binds — clean separability)
- `bassDrive` → `audio.bass`, `midDrive` → `audio.mid`, `highDrive` → `audio.high`
- `msg` → text input (auto-binds to `cue.latest` per ShaderSource convention)

**Self-score (out of 25):**
- a) Multi-player separability: 5 (six forms, five player binds + one bass bind, each with its own visual identity)
- b) Depth & dimensionality: 4 (3D sub-blob projection, per-form z-parallax, depth-of-field on kernel, fog haze)
- c) Intentional motion: 4 (silence holds; energy opens forms; bass swells halo as a separate axis; per-form independent breathing phases)
- d) Abstract not literal: 5 (auras are pure abstraction — no icons, no readable thing-depiction; captions are abstract glyph-slices)
- e) Surprise / risk: 4 (consciousness-manual poster as a live shader is a new authoring move in this corpus; sub-blob 3D yaw inside a grid cell is non-obvious)
- **Total: 22 / 25**

**Files written:**
- `/Users/lu/easel/shaders/forms_text.fs`
- `/Users/lu/easel/build/Easel.app/Contents/Resources/shaders/forms_text.fs`

**glslang validation:** `glslangValidator -S frag` against Easel preamble harness passes cleanly (exit 0, no warnings). SPIR-V codegen flags the standard `location` warning that all Easel ISF shaders trigger (Vulkan-only requirement, irrelevant to the GL pipeline).

**Caveats:**
- Caption text is intentionally tiny + treated as visual rhythm; if a future critique reads it as "logo / readable text" the `showCaptions` bool defaults it off in one click.
- Form #6 listens to `audio.bass` rather than `player[6].energy` because the synthetic player layer caps at 6 but the rubric's separability axis rewards distinct channels per form; could swap to `player[6].energy` once N=6 is the default.
- DOF on the kernel is a softness scalar, not a true blur — the aura halo carries most of the depth read.
