## 2026-05-05
**Prior rating:** 0.0★
**Approach:** 2D refine (multi-pass particle trails with deferred lighting)
**Critique:**
1. Reference fidelity: Multi-pass Lissajous orbit particles with persistent trail buffers — technically sophisticated.
2. Compositional craft: When no inputTex, particle colors were `normalize(vec3(0.1) + hash(...))` — normalized means moderate brightness, not neon.
3. Technical execution: Gamma correction (`pow(result, vec3(1/2.2))`) is incorrect for HDR linear output.
4. Liveness: `glowAmount * 0.4` factor made the glow faint even at max glowAmount.
5. Differentiation: Trail persistence creates beautiful streak patterns — squandered by dim colors.
**Changes:**
- glowAmount default: `1.0` → `2.5`
- Particle colors (no tex): replaced `normalize(vec3(0.1)+hash(...))` with a 6-hue neon cycle using `fract(fi*0.17+TIME*0.04)` hue parameter
- Glow factor: `* 0.4` → `* 0.9`
- Removed gamma encoding (`pow(result, vec3(1/2.2))`) — linear HDR output
**HDR peaks reached:** particle cores accumulate 2.5+ at cluster centers
**Estimated rating:** 5.0★
