## 2026-05-05
**Prior rating:** 0.0★
**Approach:** 2D refine (starfield background + HDR depth glow)
**Critique:**
1. Reference fidelity: Perspective tunnel rows with zoom-by-distance is a genuine 3D-feeling effect; invisible transparent.
2. Compositional craft: Depth-scaling rows create parallax; no background means no spatial anchoring.
3. Technical execution: Zoom-by-distance calculation is correct; size-ratio creates strong parallax.
4. Liveness: TIME-driven row scroll with mod() wrap works.
5. Differentiation: Depth-perspective text is unique; needs space context.
**Changes:**
- Added starfieldBg() — 3-layer procedural starfield with nebula color wash
- Star twinkling via sin(TIME * freq + seed)
- Nebula: 4-color (violet, cyan, gold, magenta) sinusoidal wash
- transparentBg default: true→false
- textColor: white (kept), bgColor: deep space navy [0,0,0.02]
- hdrGlow default: 2.0 with depth-based brightness (far rows dimmer)
- starDensity parameter
- Alternating rows: white vs cyan for depth differentiation
- audioMod input added
**HDR peaks reached:** close rows textColor * 2.0 = 2.0, with audio 2.8+
**Estimated rating:** 3.8★

## 2026-05-12
**Prior rating:** 0.0★
**Approach:** 2D refine — NEW ANGLE: Underwater caustics bg (prior was starfield/nebula bg)
**Critique:**
1. Reference fidelity: Spacy perspective tunnel rows composited against animated underwater caustics — deep aquamarine with light refraction patterns, distinct from prior starfield entry.
2. Compositional craft: 3-layer FBM caustic interference produces bright aqua/cyan spots; volumetric godray bands; deep water gradient from floor to surface — strong underwater depth.
3. Technical execution: Caustic intensity = pow(c1*c2*c3*8, 0.6) for specular-like bright spots; bgOverride pattern; hdrGlow=2.0 for HDR cyan text.
4. Liveness: All 3 caustic layers drift at different speeds and directions with TIME; godray bands oscillate.
5. Differentiation: Different bg generator (caustics vs starfield); aquatic vs space reference; animated light vs static stars.
**Changes:**
- Added causticsBg(): 3 FBM layers multiplied for caustic intensity + deep water gradient + aqua/cyan HDR glow + godray bands
- effectSpacy() accepts bgOverride param — uses causticsBg() when transparentBg=false
- transparentBg default: true→false
- textColor default: white→cyan [0.25,1.0,0.88] with hdrGlow=2.0
- bgColor default: black→deep ocean [0.0,0.06,0.12]
- hdrGlow parameter added (default 2.0)
**HDR peaks reached:** caustic hot spots * 1.6 HDR; text * 2.0 = 2.0 cyan
**Estimated rating:** 3.8★
