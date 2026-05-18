# Session Handoff — 2026-05-04 (afternoon)

Continuation of the morning session. This handoff supersedes the earlier one. Pick up at "Open threads" near the bottom.

## Top-of-mind state

- **Easel running** (PID 26381 from this session — kill + relaunch when you want any new code/shader changes visible).
- **All shipped shaders compile** (137/142 PASS in test_shaders; 5 pre-existing failures unrelated to session work: `body_fluid`, `fluid_image`, `multi_layer_particles`, `optical_flow_distortfs`, `text_unified`).
- **Auto-improve cron is live**, runs daily 09:00 America/New_York. **PR #4 is open** at https://github.com/G3993/ShaderClaw3/pulls (3D Itten color wheel for `color_picker`). Awaiting your merge.
- **Custom shader import is now wired** in the ShaderClaw browser ("+ Import .fs" button right-aligned next to the VFX/Text/3D pills).

## What landed this session

### Curator-style 1-star shader sweep — 31 shaders rewritten
Persona: art-history professor with curated shows at top contemporary galleries (David Zwirner, Hauser & Wirth, Pace). Each rewrite makes one big load-bearing curatorial decision rooted in real art-historical reference. Compile-tested + mirrored to submodule.

Shipped (every one PASS in `test_shaders`):

| Shader | Curatorial angle |
|---|---|
| art_nouveau_mucha | Halo cartouche + whiplash hair tendrils. 4 mood palettes (Reverie/Tempête/Aube/Crépuscule). |
| cubism_picasso | Charcoal armature + 6–10 ochre/umber planes. Stenciled letters present but **default OFF** after text-strip pass. |
| dada_hoch | 28 torn fragments shuffling on bass kicks. Fraktur scene retired (slot 5 → swatch). |
| destijl_mondrian | Boogie-woogie animated grid, 5-color hard rule, 16s repartition. |
| expressionism_kirchner | Carved-woodcut hatched ground, Brücke chromatics, 3 moods. |
| constructivism_lissitzky | Axonometric Proun, 4-color discipline, wedge-slam beat. Cyrillic glyphs **default OFF**. |
| fauvism_matisse | 4 mood scenes (La Danse / Jazz Cut-Out / Femme au chapeau / Goldfish). |
| popart_lichtenstein | 4 panels with Ben-Day dots. Speech bubble **default OFF**. |
| opart_vasarely | 4 modes (Vega bulge / Tridim / Riley wave / Zebra), fwidth-AA. |
| memphis_primitives | 6-layer kinetic Memphis pattern stack on cream. |
| surrealism_magritte | 4 moods (Golconda / Empire of Light / Son of Man / Treachery). |
| glitch_datamosh | 5 modes (Pixel Sort / Datamosh / JPEG Block / Sync Loss / Compound). |
| vaporwave_floral_shoppe | Twin-sun, Tron grid, marble bust silhouette. Katakana **default OFF**. |
| hologram_glitch | Real raymarched SDF wireframe + Leia-grade signal degradation. |
| liquid_warp | Soak-stain pools + curl-noise. 4 palettes (Frankenthaler / Anadol / Bath Salts / Magma). |
| mirror_fractal | True N-fold mirror with restrained interior compositions. |
| chladni_figures | Real Chladni equation + 12 hand-picked (n,m) pairs. 3 moods. |
| sonoluminescence | Single-bubble collapse with 3-tier HDR halo + shockwave rings. |
| solar_flare_corona | NASA-SDO-grade composition with coronal loops + prominences. |
| black_hole_sun | Analytic EHT/Gargantua portrait. No noise carpet. |
| laser_labyrinth | 4 compositions (Crossfire/Cathedral/Pyramid/TronGrid). |
| minimalism_stella | 5 moods (Black Paintings/Protractor/Agnes Martin/Kelly/LeWitt). |
| turrell_chroma | Pure Ganzfeld color field. 5 hand-picked color triads. |
| soph_orb | Single hero orb. 4 surface treatments (Pearl/Wood/Plasma/Marble). |
| edges | Edge-detect-as-drawing. 5 modes (Charcoal/Pencil/Etching/Schiele/Hockney). |
| geometric_tunnel | 5 tunnel modes (Stargate/McCall/Escher/Bauhaus/Brutalist). |
| liquid_ripples_3d | Real raymarched water surface. 4 submerged-pattern moods. |
| data_sculpture | Synthetic 32-band FFT drives 5 moods. |
| robot_arm | 4-link FK arm. 4 moods. **Compile error fixed** (mp* dependencies removed). |
| basic_shapes_3d | Tillmans-style still life. 5 mood backdrops. |
| volumetric_clouds_3d | 4 moods (Turner/Rothko/Constable/Friedrich). |
| futurism_boccioni | (morning) SDF stutter-cloned humanoid. |

### New 3D references
- **`pbr_glass_dispersion.fs`** (id 502) — refractive crystal sculpture with per-channel chromatic dispersion, Fresnel-mixed reflections, specular highlight + rim halo. Shipped this morning.

### Floating-text strip pass
User feedback: "remove the text from these shaders, some of them have text floating around and its weird." Done by changing input defaults so text is OFF out of the box; the inputs themselves are preserved so users can re-enable per-layer.

- popart_lichtenstein: `speechBubble` DEFAULT bool true → false
- cubism_picasso: `lettering` DEFAULT 0.85 → 0.0
- constructivism_lissitzky: `glyphAmount` DEFAULT 0.85 → 0.0
- vaporwave_floral_shoppe: `katakanaCount` DEFAULT 9 → 0 (Off)
- dada_hoch: Fraktur scene retired — dispatcher slot 5 redirects to swatch.

### Custom shader import
- "+ Import .fs" button in ShaderClaw panel header, right-aligned with the VFX/Text/3D pills.
- Opens native macOS file picker (`openFileDialog_mac`).
- Copies file into connected shaders dir, appends manifest entry with auto-incremented id (≥1000), prettifies title from filename (`my_shader.fs` → "My Shader"), tags categories `["Imported", "Generator"]`.
- Calls `m_shaderClaw.refreshManifest()` so the new shader appears immediately.
- Handles collisions by suffixing `_1`, `_2`, etc.
- Located in Application.cpp around the VFX/Text/3D sub-tabs row (search `"+ Import .fs"`).

### Other host-side ships
- **Mic icon moved** to LEFT of System Audio dropdown in transport row.
- **`text_james.fs` reserved-word fix** — `active` → `isActive` at lines 162/183/198 (GLSL ES reserved word).
- **DataBus numeric extension** — `setNum / getNum / hasNum` API + `availableNumericKeys()` listing 11 vision channels. Test target `test_databus` ships with 5 test cases / 3298 assertions covering thread-safe concurrency.

## Repo state

| Repo | Path | State |
|---|---|---|
| Easel app | `/Users/lu/easel` | Working tree dirty — many staged. **Don't auto-commit.** |
| ShaderClaw3 (standalone, runtime) | `/Users/lu/ShaderClaw3` | Working tree dirty with all 31 rewrites + new pbr_glass_dispersion + manifest. Stash `pre-pull: shader work-in-progress` retained. |
| ShaderClaw3 (Easel submodule) | `/Users/lu/easel/external/ShaderClaw3` | Mirror of standalone. |
| Cloud worklist | github.com/G3993/ShaderClaw3 | `.shader_ratings.json` committed. PR #4 open with auto-improve color_picker rewrite. |

## Open threads (priority order)

### Highest priority — finish the 3D sweep (3 items)
**Agent budget reset at 13:30 America/New_York.** Three rewrites were dispatched but hit the rate limit:

1. **`pbr_reference_3d.fs` rewrite** — last 1-star 3D shader untouched. Curator brief: Kapoor mirror / Brancusi bronze / Klein monochrome / Judd stack mood enum. Treat as the new high-end PBR reference template — luxury museum-piece feel.
2. **`audio_particles_3d.fs` rewrite** — current 97-line implementation feels weak. Brief: Anadol Cloud / Gursky Repetition / Memo Form / Constellation moods.
3. **`shatter_grid.fs` create** — manifest references it (id 414) but file does not exist on disk. Brief: mirror-tile grid that explodes on bass; tile fragments reveal pieces of `inputTex`. 4 moods (Mirror Mosaic / Stained Glass / Pixel Mosaic / Hockney).

Resume by spawning fresh agents with the briefs in this session's earlier message log, OR write them by hand if you want to skip the agent layer.

### High priority — universal 3D camera + lighting bundle (deferred)
**User directive (verbatim):** *"bundle the camera controls together so they are universal for all 3d shaders. lighting too. add a few more parameters and really push the limits of these new shaders."*

Plan:
- Standardize uniform names across every 3D-tagged `.fs`:
  - **Camera**: `camDist` (float 2–12), `camHeight` (float -2–4), `camOrbitSpeed` (float 0–2), `camAzimuth` (float 0–360 manual rotation), `camFov` (float 30–100 optional).
  - **Lighting**: `keyAngle` (float 0–360), `keyElevation` (float 0–90), `keyColor` (color), `fillColor` (color), `ambient` (float 0–0.5), `rimStrength` (float 0–1), `exposure` (float 0.5–3).
  - **Surface trim (per-shader-mood overrides)**: `roughnessTrim` (float -0.3..0.3 nudge), `metalnessTrim`, `clearcoat` (float 0..1).
- For each 3D shader, add these inputs to the ISF JSON header + wire them into the camera/light constants. Drop the per-shader bespoke camera/light inputs in favour of the standard set. Move shader-specific look knobs into a "Look" subgroup.
- Easel host nice-to-have: a "3D Defaults" panel that pushes these uniforms across all 3D layers in one click.
- Affected files (~10): `basic_shapes_3d`, `pbr_glass_dispersion`, `pbr_reference_3d`, `volumetric_clouds_3d`, `audio_particles_3d`, `soph_orb`, `liquid_ripples_3d`, `robot_arm`, `futurism_boccioni`, `color_picker` (3D Itten variant after PR merge), and any future 3D shader.
- Coordinated wave — best dispatched as parallel subagents once budget resets. Give each agent the same standardized-uniform header to insert verbatim.

### Medium priority
4. **Phase Q v4 — bloom pipeline** at end of warp pass. Two-pass separable Gaussian on luminance > threshold, screen-blend back. Adds the "luxurious glow" feel. Multi-FBO chain already exists.
5. **Phase V V1.1 — Apple Vision real feed**. `src/voice/VisionSource.{h,mm}` using Vision.framework + AVCaptureSession. Output: pose/face/hand landmarks packed into 1D RGBA16F texture bound to `mpPoseLandmarks`. **DataBus numeric API is now in place** — VisionSource can push values via `setNum("vision.pose.head.x", v)`.
6. **PropertyPanel binding UI** for vision keys — surface `DataBus::availableNumericKeys()` in the binding dropdown.

### Phase M — Metal port (parking lot, multi-month)
**User directive (verbatim):** *"continue with the phase m and all of the 3D work."*

Phase M is a Metal-API port for Niagara/Nanite/Lumen-class fidelity. GL 4.1 ceiling is real — every 3D shader currently maxes at the OpenGL 4.1 feature set (no compute shaders, no descriptor indexing, no mesh shaders, no real RT, no proper async compute).

Recommended split:
- **M.0 — Metal abstraction layer**: `src/render/RenderBackend.{h,cpp}` wraps GL & Metal behind one interface. Existing GL paths untouched.
- **M.1 — Metal renderer for ShaderSource**: SPIRV-Cross → MSL transpile of existing ISF shaders. Verify all 142 shaders work via Metal.
- **M.2 — Metal compute shaders**: enable compute-pass effects (separable Gaussian for Phase Q v4 bloom done correctly, parallel reductions, particle simulations).
- **M.3 — Metal RT**: use Apple's `MTLAccelerationStructure` for real raytraced reflections / soft shadows on 3D shaders. Replaces the current SDF raymarch fakery for select shaders.
- **M.4 — Metal mesh shaders**: when applicable.

This is the path to the "Niagara/Nanite/Lumen" ambition. Don't start until usage proves the GL ceiling is the actual bottleneck (prominent profiling data point: are you seeing >16ms frame times on real content?). Recommended pre-Phase-M: ship Phase Q v4 bloom + finish 3D shader sweep + run a profiling pass.

### Lower priority / parking lot
- More 3D references: `volumetric_smoke.fs`, `pbr_metal_chrome.fs`, `chrome_orb_3d.fs`.
- EffectKit format for compute-heavy multi-pass effects.
- Voice script recording (timeline-tied voice command replay).
- Multi-user co-creation sync layer.
- Phase V V1.2: vision-driven layer triggering ("when smile detected, fade in Fauvism").

## Files most-touched this session

```
/Users/lu/easel/
├── src/app/
│   ├── Application.cpp        # mic-icon swap, "+ Import .fs" button
│   └── DataBus.h              # numeric API + availableNumericKeys()
├── tests/
│   └── test_databus.cpp       # 5 test cases / 3298 assertions
├── CMakeLists.txt             # add_executable(test_databus)
└── .planning/
    └── SESSION-HANDOFF.md     # this file

/Users/lu/ShaderClaw3/
└── shaders/                   # 31 curator rewrites + pbr_glass_dispersion (new) + manifest update
```

## Bootstrap for next session

1. `cd /Users/lu/easel && git status -s | wc -l` — confirm dirty file count.
2. `pgrep -lf "Easel.app/Contents/MacOS/Easel"` — see if app is running.
3. `gh pr list -R G3993/ShaderClaw3` — check open auto-improve PRs.
4. `./build/test_shaders /Users/lu/ShaderClaw3/shaders 2>&1 | tail -10` — confirm 137/142 PASS.
5. After 13:30 EDT: resume the deferred shader rewrites (pbr_reference_3d, audio_particles_3d, shatter_grid create) + the universal camera/lighting bundle.

## Notes for the next agent

- Project compiles **without ARC**. Any new Obj-C++ needs manual `[release]`.
- `/Users/lu/ShaderClaw3` is the auto-connected runtime path — edits here are what Easel sees.
- Voice mic uses tap-to-toggle, mic icon is now LEFT of System Audio.
- User wants Niagara/Nanite/Lumen-grade fidelity — Phase M is the path; manage expectations on timeline (multi-month).
- 3D pill in ShaderClaw filters by manifest `categories: ["3D", ...]`.
- Custom shader import: "+ Import .fs" button in ShaderClaw header. Imported shaders are tagged `"Imported"` so a future "Imported" sub-pill can be added if the collection grows.
