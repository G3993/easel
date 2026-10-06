## 2026-05-06
**Prior rating:** 0.0★
**Approach:** 3D raymarch — NEW ANGLE: Spectral Prism, chromatic dispersion beams (first critique; original was inputImage tinting effect)
**Critique:**
1. Reference fidelity: Glass prism chromatic dispersion is a strong standalone concept replacing the useless tinting utility.
2. Compositional craft: Volumetric beam glow creates depth; prism silhouette anchors the scene center.
3. Technical execution: 64-step march + volumetric accumulation pass; fwidth() AA on all beam edges.
4. Liveness: Camera orbits TIME-driven; beam spread audio-modulated.
5. Differentiation: Void black + 3 saturated HDR beams gives maximum contrast and full saturation.
**Changes:**
- Full rewrite from inputImage tinting utility to 3D raymarched spectral prism
- Glass prism SDF (sdBox approximation) with 3 dispersion beam capsules
- Volumetric glow accumulated along eye ray (exp falloff)
- Palette: crimson 2.0+, electric blue 3.0, acid yellow 2.5+, warm white 2.0
- Audio modulates beam brightness
**HDR peaks reached:** beam cores: crimson 2.0, blue 3.0, yellow 2.5; volumetric glow ~1.5 surround
**Estimated rating:** 4.0★

## 2026-05-12
**Prior rating:** 0.0★
**Approach:** 3D raymarch — NEW ANGLE: Soap Bubble Iridescence (prior 2026-05-06 was 3D spectral prism with discrete RGB beams; this is 3D sphere with continuous thin-film rainbow sweep)
**Critique:**
1. Composition: single centered sphere on void background — portrait focal vs prior beam geometry.
2. Palette: continuous full-spectrum HSV thin-film hue cycle (all 6 colors) vs prior 3-color discrete beams.
3. Motion: calm orbital camera (default 0.07) vs prior none documented.
4. Silhouette: sphere silhouette + fresnel darkening = clean orb against void.
5. HDR fidelity: specPeak default 3.5, specular peaks 3.5+ linear; film color 1.6×; background near-zero for max contrast.
**Changes:**
- Full rewrite: SDF sphere + thin-film fresnel hue sweep (filmTurns, filmSpeed params)
- Dual-band iridescence: Fresnel angle + latitude position angle driving hue
- Two specular lights: warm key (256 exponent) + cool fill (80 exponent), both HDR
- Calm orbit default 0.07 (min 0.0, max 1.0)
- Audio: bass→bubble scale breath (K=0.07, within cap), high→hue phase shift
- Void background + faint radial nebula for context
**Motion audit:** orbitSpeed default 0.07 ✓ (calm floor); audio K=0.07 ≤ 1.5 ✓; no step transitions.
**HDR peaks reached:** specPeak * spK = 3.5 (warm white); fill spec 1.6; film color 1.6
**Estimated rating:** 4.0★
