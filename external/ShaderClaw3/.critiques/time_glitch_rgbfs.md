## 2026-05-05
**Prior rating:** 0.0★
**Approach:** 3D raymarch
**Critique:**
1. Reference fidelity: VIDVOX 8-frame buffer delay — requires inputImage; produces nothing standalone.
2. Compositional craft: Frame buffering is purely an effect; no content without source.
3. Technical execution: 9-pass persistent buffer architecture is complex and correct, but all passes output noise without input.
4. Liveness: TIME-driven via random delay shift, but input-dependent.
5. Differentiation: Interesting channel-split temporal effect; not a generator.
**Changes:**
- Full rewrite as "Signal Interference" — raymarched 3D RGB data planes with glitch geometry
- Three independently marched color planes (R/G/B) at Y-offsets (planeOffset parameter)
- Each plane: scanlines + column bars + glitch blocks as SDF geometry
- Per-channel glitch: horizontal displacement driven by hash(floor(y * 8 + t * rate))
- HDR: signal red, data green, electric blue — fully saturated
- White-hot specular peak on hit surfaces
- Camera slowly sweeps through the planes (sin(t * 0.13))
- hdrBoost parameter (default 2.0)
- audioMod modulates displacement and brightness
**HDR peaks reached:** per-channel hdrBoost * diffuse = 2.0; white spec adds ~2.5
**Estimated rating:** 4.0★

## 2026-05-06
**Prior rating:** 0.0★
**Approach:** 2D standalone — NEW ANGLE: RGB Prism Rings 2D with per-channel glitch (prior 2026-05-05 was 3D RGB data planes, never committed)
**Critique:**
1. Reference fidelity: Per-channel ring glitch references chromatic aberration/RGB splitting — more visually coherent than 3D plane geometry.
2. Compositional craft: Concentric rings create strong radial focal point; per-channel offsets create depth without 3D.
3. Technical execution: fwidth() AA on ring edges, per-band hash noise for block-glitch, clean additive RGB combination.
4. Liveness: Ring phase animates with TIME; glitch shifts with TIME; audio modulates width.
5. Differentiation: 3D→2D axis change; different visual grammar (rings vs planes); different palette arrangement (additive R+G+B vs separated plane colors).
**Changes:**
- Full rewrite from VIDVOX frame buffer to standalone 2D ring glitch generator
- Per-channel (R/G/B) concentric rings with independent glitch offsets
- Palette: fully saturated signal red 2.0, data green 2.0, electric blue 2.5 — no white mix
- fwidth() ring edge AA, additive channel combination
- Audio modulates ring width and glitch amount
**HDR peaks reached:** single channel 2.0-2.5; ring overlaps additive ~3.0-4.0
**Estimated rating:** 4.0★

## 2026-05-12
**Prior rating:** 0.0★
**Approach:** 3D raymarch — NEW ANGLE: Neon Circuit Board (prior v1 was 3D RGB data planes, prior v2 was 2D ring glitch; this is 3D architectural tech surface with procedural PCB geometry)
**Critique:**
1. Composition: low-angle orbit across PCB surface — wide architectural vs ring's radial pattern vs planes' frontal view.
2. Palette: dark PCB green, copper gold traces, RGB data pulse dots — more restrained than prior full-RGB bleed.
3. Motion: orbit default 0.08; pulse speed default 0.55.
4. Silhouette: circuit trace extrusions cast micro-shadows; via pads add focal nodes.
5. HDR fidelity: hdrPeak default 3.0 on pulse dots; trace specular 2.2; depth fog keeps far traces dark.
**Changes:**
- Full rewrite: flat board SDF + H-trace + V-trace + via-pad SDFs; hash-based trace presence
- dataPulse(): 6 H-pulses + 5 V-pulses, RGB channel split ±0.012 XZ offset
- Low-angle orbit camera (camH 0.32) looking across PCB surface
- fwidth(p.y) edge AA on trace top surface
- Audio: bass * (1 + 0.5 * onTrace) → K ≤ 0.5 ✓
- Depth fog exp(-t*0.12)
**Motion audit:** orbitSpeed 0.08 ✓; pulseSpeed 0.55 ✓ (calm); audio K ≤ 0.8 ✓.
**HDR peaks reached:** hdrPeak 3.0 on pulse dot cores; trace spec 2.2; mid-trace blend ~1.5
**Estimated rating:** 4.0★
