## 2026-05-05
**Prior rating:** 0.0★
**Approach:** 2D refine (particle system, 3D category added)
**Critique:**
1. Reference fidelity: Particle bounce concept is solid but LED grid default masks all output — black on dark bg.
2. Compositional craft: Capsule streak particles are a strong visual idea, lost in default darkness.
3. Technical execution: uses undeclared audio uniforms (audioBass, audioHigh) safely; LED mode quantizes to near-black at default ledSize.
4. Liveness: TIME-driven particle motion works but LED mode destroys visibility.
5. Differentiation: Unique capsule-stretch bounce system; killed by LED default and desaturated colorJitter mixing with white.
**Changes:**
- Removed LED wall mode entirely (was default ON, producing near-black output)
- Replaced colorJitter white-mixing with fully saturated 6-hue neon palette (magenta→cyan→gold→orange→violet→lime)
- Glow boosted: default 1.3 → 2.5 (HDR range)
- Particle count stays at 128 (was N=256 const regardless of particleCount input)
- Added 3D category
- Black background (0.0, 0.0, 0.01)
- Stretch, particle size defaults tuned up for visibility
**HDR peaks reached:** particle cores + halo accumulation → 2.5+ per cluster
**Estimated rating:** 4.0★

## 2026-05-12
**Prior rating:** 0.0★
**Approach:** 2D refine — NEW ANGLE: Wide coastal panorama (prior 2026-05-05 was close-up Picasso face portrait; this is landscape orientation with lighthouse, cliff, seagulls, horizon — different composition axis and reference)
**Critique:**
1. Composition: wide panoramic landscape (horizon line + cliff + lighthouse + sky) vs prior portrait face; completely different framing.
2. Reference: coastal scene → all 5 drawing moods (Charcoal/Pencil/Etching/Schiele/Hockney) now applied to landscape.
3. Motion: subtle sway animation (0.004 * sin(t*0.42)) for wind effect.
4. Silhouette: lighthouse tower strong vertical; cliff diagonal; horizon horizontal — 3 clear compositional elements.
5. HDR fidelity: beacon lamp accent (HDR amber 2.5) replaces prior ear stud; same intensity level.
**Changes:**
- Replaced facePortrait() with coastalScene(): horizon line, cliff bezier, lighthouse SDF (vertical segments + arc), wave arcs, seagull M-arches, distant headland
- RED accent → beacon lamp dot (amber, same smoothstep radii)
- Maintained all 5 drawing moods, Sobel pipeline, HDR rim, hatch system
- Sway 0.004 (vs face sway 0.010) — calmer
**Motion audit:** sway 0.004 ✓ (calm); audio params unchanged from prior (K via audioReact param ≤ 2.0, user-controlled).
**HDR peaks reached:** ink rim vec3(2.3+) per mood; beacon lamp 2.5; paper 1.0 baseline
**Estimated rating:** 4.0★
