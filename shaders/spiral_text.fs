/*{
  "DESCRIPTION": "Spiral Text — the cue message is whispered around a logarithmic vortex by three counter-rotating arms that wind into perspective depth. Each arm belongs to a player; their energy thickens the coil, lifts glyphs off the page and brightens its ink. Letters bend tangent to the curve, scale with z-depth, and recede into a paper-cream haze. Bass pulses the camera zoom into the eye of the spiral; cue.latest types in via the typewriter so the spiral writes itself as the speaker speaks. fwidth-antialiased curves, ink-on-paper palette, no defaults harmed.",
  "CREDIT": "Easel A-List · spiral_text",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "TYPE": "text", "DEFAULT": "SPIRAL SP R SP RALE SP R SP", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "energyA", "LABEL": "Arm 1 Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB", "LABEL": "Arm 2 Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC", "LABEL": "Arm 3 Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "bassPull", "LABEL": "Bass Vortex Pull", "TYPE": "float", "DEFAULT": 0.6, "MIN": 0.0, "MAX": 1.5, "BIND": "audio.bass" },

    { "NAME": "armCount", "LABEL": "Arms", "TYPE": "long", "DEFAULT": 3, "VALUES": [1,2,3,4,5,6], "LABELS": ["1","2","3","4","5","6"] },
    { "NAME": "coilRate", "LABEL": "Coil Rate (log b)", "TYPE": "float", "DEFAULT": 0.22, "MIN": 0.05, "MAX": 0.55 },
    { "NAME": "turns", "LABEL": "Turns", "TYPE": "float", "DEFAULT": 4.5, "MIN": 1.5, "MAX": 7.5 },
    { "NAME": "spinSpeed", "LABEL": "Spin Speed", "TYPE": "float", "DEFAULT": 0.18, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "counterRotate", "LABEL": "Counter Rotate", "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 1.0 },

    { "NAME": "textAlong", "LABEL": "Glyphs Along Curve", "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "glyphSize", "LABEL": "Glyph Size", "TYPE": "float", "DEFAULT": 0.034, "MIN": 0.014, "MAX": 0.065 },
    { "NAME": "glyphSpacing", "LABEL": "Glyph Spacing", "TYPE": "float", "DEFAULT": 1.05, "MIN": 0.55, "MAX": 1.6 },

    { "NAME": "depthAmp", "LABEL": "Depth Parallax", "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "audioDepth", "LABEL": "Audio Depth", "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 1.5 },

    { "NAME": "paperColor", "LABEL": "Paper", "TYPE": "color", "DEFAULT": [0.953, 0.937, 0.886, 1.0] },
    { "NAME": "inkA", "LABEL": "Ink — Arm 1 (Sumi)", "TYPE": "color", "DEFAULT": [0.060, 0.058, 0.071, 1.0] },
    { "NAME": "inkB", "LABEL": "Ink — Arm 2 (Vermillion)", "TYPE": "color", "DEFAULT": [0.760, 0.180, 0.140, 1.0] },
    { "NAME": "inkC", "LABEL": "Ink — Arm 3 (Indigo)", "TYPE": "color", "DEFAULT": [0.130, 0.180, 0.380, 1.0] }
  ]
}*/

// =====================================================================
//  SPIRAL TEXT
//  A logarithmic spiral typewriter. The cue.latest message is repeated
//  along N arms r = a·exp(b·θ); each arm carries a player's voice and
//  recedes into perspective depth via z = (theta_end - theta) so glyphs
//  shrink and dim toward the vortex eye. Glyphs rotate tangent to the
//  curve. Bass zooms the camera into the eye; energy thickens each arm
//  and lifts its glyphs off the page (parallax-z highlight + drop).
//  No spectrum bars. No EKG. No checkerboards.
// =====================================================================

#define MAX_MSG     48
#define SPACE_CH    26
#define MAX_ARMS    6
#define TAU         6.28318530718

// ─── Font atlas (37 cells: A-Z, space, 0-9) ─────────────────────────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}

int getChar(int slot) {
    if (slot ==  0) return int(msg_0);
    if (slot ==  1) return int(msg_1);
    if (slot ==  2) return int(msg_2);
    if (slot ==  3) return int(msg_3);
    if (slot ==  4) return int(msg_4);
    if (slot ==  5) return int(msg_5);
    if (slot ==  6) return int(msg_6);
    if (slot ==  7) return int(msg_7);
    if (slot ==  8) return int(msg_8);
    if (slot ==  9) return int(msg_9);
    if (slot == 10) return int(msg_10);
    if (slot == 11) return int(msg_11);
    if (slot == 12) return int(msg_12);
    if (slot == 13) return int(msg_13);
    if (slot == 14) return int(msg_14);
    if (slot == 15) return int(msg_15);
    if (slot == 16) return int(msg_16);
    if (slot == 17) return int(msg_17);
    if (slot == 18) return int(msg_18);
    if (slot == 19) return int(msg_19);
    if (slot == 20) return int(msg_20);
    if (slot == 21) return int(msg_21);
    if (slot == 22) return int(msg_22);
    if (slot == 23) return int(msg_23);
    if (slot == 24) return int(msg_24);
    if (slot == 25) return int(msg_25);
    if (slot == 26) return int(msg_26);
    if (slot == 27) return int(msg_27);
    if (slot == 28) return int(msg_28);
    if (slot == 29) return int(msg_29);
    if (slot == 30) return int(msg_30);
    if (slot == 31) return int(msg_31);
    if (slot == 32) return int(msg_32);
    if (slot == 33) return int(msg_33);
    if (slot == 34) return int(msg_34);
    if (slot == 35) return int(msg_35);
    if (slot == 36) return int(msg_36);
    if (slot == 37) return int(msg_37);
    if (slot == 38) return int(msg_38);
    if (slot == 39) return int(msg_39);
    if (slot == 40) return int(msg_40);
    if (slot == 41) return int(msg_41);
    if (slot == 42) return int(msg_42);
    if (slot == 43) return int(msg_43);
    if (slot == 44) return int(msg_44);
    if (slot == 45) return int(msg_45);
    if (slot == 46) return int(msg_46);
    if (slot == 47) return int(msg_47);
    return -1;
}

int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 0;
    if (n > MAX_MSG) return MAX_MSG;
    return n;
}

// Tiny hash for per-arm jitter (no determinism issues).
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }

// Cheap value-noise for the paper grain (fwidth-AA never touches this).
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float a = hash11(dot(i, vec2(1.0, 113.0)));
    float b = hash11(dot(i + vec2(1.0,0.0), vec2(1.0,113.0)));
    float c = hash11(dot(i + vec2(0.0,1.0), vec2(1.0,113.0)));
    float d = hash11(dot(i + vec2(1.0,1.0), vec2(1.0,113.0)));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}

// Glyph SDF-ish fill: sample the atlas, smoothstep with fwidth for AA.
// Returns 0..1 coverage at the given local UV inside one glyph cell.
float glyphCoverage(int ch, vec2 uv) {
    if (ch < 0 || ch == SPACE_CH || ch > 35) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    float s = sampleChar(ch, uv);
    // Atlas is grayscale; treat 0.5 as edge, use fwidth on the sample
    // for clean premium AA.
    float fw = max(fwidth(s), 1e-4);
    return smoothstep(0.5 - fw, 0.5 + fw, s);
}

void main() {
    vec2 res    = RENDERSIZE;
    vec2 frag   = gl_FragCoord.xy;
    float aspect = res.x / max(res.y, 1.0);

    // Aspect-corrected centered coords (-asp/2..+asp/2, -.5..+.5)
    vec2 uv;
    uv.x = (frag.x / res.x - 0.5) * aspect;
    uv.y = (frag.y / res.y - 0.5);

    int total = charCount();
    bool hasText = (total > 0);

    // ── Camera zoom: bass pulls you into the vortex eye ─────────────
    // bassPull is BIND'd to audio.bass; the zoom factor maps that to a
    // gentle z-dolly so silence reads as a calm spiral, bass-hits as
    // genuine forward motion. Counter-rotating arms make the depth read.
    float bp     = clamp(bassPull, 0.0, 1.5);
    float zoom   = 1.0 - 0.18 * bp - 0.06 * sin(TIME * 0.7);
    zoom         = clamp(zoom, 0.55, 1.4);
    vec2  pView  = uv / zoom;

    // Polar coords of the view-space point.
    float r0   = length(pView);
    float th0  = atan(pView.y, pView.x);

    // ── Vortex eye: warp r by a hyperbolic compression so the center
    // ── opens like a real eye (not just a point). Adds dimensionality
    // ── without raymarching.
    float eye  = 0.04;                    // eye radius
    float rW   = sqrt(r0 * r0 + eye * eye); // hyperbolic-smoothed radius

    // ── Arms (per-player) ───────────────────────────────────────────
    int nArms = int(armCount);
    if (nArms < 1) nArms = 1;
    if (nArms > MAX_ARMS) nArms = MAX_ARMS;

    float b      = clamp(coilRate, 0.05, 0.55);     // log-spiral tightness
    float nTurns = clamp(turns, 1.5, 7.5);
    float thetaMax = nTurns * TAU;                  // glyph 0 sits at theta=thetaMax (outside)
    float spin   = TIME * spinSpeed;

    // Paper backdrop with marbled paper-grain — never a hard pixel grid.
    vec3 paper = paperColor.rgb;
    float grain = vnoise(uv * 320.0) * 0.5 + vnoise(uv * 80.0 + 7.0) * 0.5;
    paper *= 0.94 + 0.10 * grain;
    // Soft warm vignette so the spiral has a stage.
    paper *= 1.0 - 0.22 * dot(uv, uv);

    vec3  col      = paper;
    float coverAll = 0.0;     // accumulated glyph coverage
    vec3  inkAll   = vec3(0.0);

    // Energies → array
    float energies[MAX_ARMS];
    energies[0] = energyA;
    energies[1] = energyB;
    energies[2] = energyC;
    energies[3] = (energyA + energyB) * 0.5;   // synth fill for 4+ arms
    energies[4] = (energyB + energyC) * 0.5;
    energies[5] = (energyA + energyC) * 0.5;

    for (int a = 0; a < MAX_ARMS; a++) {
        if (a >= nArms) break;
        float fa = float(a);
        // Arm direction: alternate sign when counterRotate active.
        float dir = ( (mod(fa, 2.0) < 0.5) || counterRotate < 0.5 ) ? 1.0 : -1.0;
        // Arm rotational phase offset around the disk.
        float phase = fa * (TAU / float(nArms));

        // Per-arm energy → 0..1; shapes brightness, ink lift, glyph swell.
        float e = clamp(energies[a], 0.0, 1.0);

        // Ink color: cycle through inkA/B/C, jittered per arm.
        vec3 inkBase;
        float t = mod(fa, 3.0);
        if (t < 0.5)      inkBase = inkA.rgb;
        else if (t < 1.5) inkBase = inkB.rgb;
        else              inkBase = inkC.rgb;
        // 4-6 get mixed with the next color so they're not duplicates.
        if (a >= 3) {
            vec3 alt = (t < 0.5) ? inkB.rgb : (t < 1.5 ? inkC.rgb : inkA.rgb);
            inkBase = mix(inkBase, alt, 0.35 + 0.25 * hash11(fa));
        }

        // ── Find the glyph slot k on this arm closest to pView ──
        // Spiral parametrization: r(theta) = a0 * exp(b * theta),
        // with theta running from 0 at the eye outward to thetaMax at
        // the rim. We place glyph k at theta_k = thetaMax - k * dtheta,
        // dtheta chosen so arc-length spacing matches glyphSize.
        //
        // To find which glyph the current pixel is closest to without
        // looping over every slot, invert: given the pixel's (rW, th0)
        // pick the spiral arc whose theta matches (th0 - spin - phase)
        // mod TAU on this arm's branch. Compute the *spiral* theta that
        // satisfies r = a0 * exp(b * theta), and compare arc-position
        // to the nearest glyph slot.
        //
        // a0 is fixed so the outer turn lands near the canvas rim.
        float aRim  = 0.50 / exp(b * thetaMax); // r at theta=thetaMax ≈ 0.5
        // Avoid log(0) in the eye.
        float rClamp = max(rW, aRim * 0.5);
        // Theta on the spiral curve for this pixel (continuous, not mod).
        float thetaCurve = log(rClamp / aRim) / b;
        if (thetaCurve < 0.0) thetaCurve = 0.0;
        if (thetaCurve > thetaMax + 0.5) thetaCurve = thetaMax + 0.5;

        // Angle the spiral curve would be at this theta on this arm
        // (with spin and per-arm phase).
        float thetaArm = dir * thetaCurve + spin * dir + phase;
        // Angular delta between this pixel's angle and the spiral arm.
        float dAng = th0 - thetaArm;
        // Wrap to (-PI, PI].
        dAng = mod(dAng + 3.14159265, TAU) - 3.14159265;

        // Radial distance to the spiral curve r=a0*exp(b*theta) for the
        // pixel's *actual* theta — used for inter-arm strand thickness.
        // (The arm sits exactly along the curve where dAng=0.)
        float armR     = aRim * exp(b * thetaCurve);
        // Convert dAng into an arc-length offset at this radius.
        float crossArc = dAng * armR;

        // ── Depth: z runs 0 at the rim → 1 at the eye ───────────────
        // Glyphs shrink and dim toward the eye; bass and audioDepth
        // exaggerate the recession. This is genuine pseudo-3D parallax.
        float zNorm = clamp(1.0 - thetaCurve / thetaMax, 0.0, 1.0);
        float depthScale = mix(1.0, 0.25, zNorm);          // far → small
        float depthScaleBoost = 1.0 - 0.20 * bp * audioDepth * zNorm;
        depthScale *= depthScaleBoost;

        // Glyph cell size (world units). Spacing along arc.
        float gSize  = clamp(glyphSize, 0.014, 0.065) * depthScale;
        float gSpace = gSize * clamp(glyphSpacing, 0.55, 1.6);
        // Add energy swell: louder arm → glyphs swell a hair.
        gSize *= 1.0 + 0.18 * e;

        // Convert glyph spacing in world arc-length → dtheta at this r.
        // arc = r * dtheta  →  dtheta = arc / r
        float dtheta = gSpace / max(armR, gSize);

        // Which slot k along this arm holds the glyph nearest this px?
        // Outer turn = slot 0 (newest), inner = older.
        float kf = (thetaMax - thetaCurve) / max(dtheta, 1e-3);
        int   k  = int(floor(kf + 0.5));
        if (k < 0) continue;
        // Cap walk: spiral can hold many glyphs over many turns; we
        // don't render beyond what the message has.
        if (!hasText) continue;
        // Each arm carries the full message, shifted by arm index so
        // arms feel like different speakers saying the same line.
        int srcLen = total;
        int srcIdx = int(mod(float(k + a * 3), float(srcLen)));
        int ch     = getChar(srcIdx);
        if (ch < 0 || ch == SPACE_CH) continue;

        // ── Typewriter mask: when msgAge≥0 (live cue), only reveal
        //    slots up to the current type position. Slot 0 is freshest
        //    glyph (outer rim) — that's where new text appears as the
        //    speaker speaks. Older glyphs spiral inward.
        bool live = msgAge >= 0.0;
        const float CPS = 28.0;     // matches typewriter cps
        if (live) {
            float typed = msgAge * CPS;
            if (float(k) > typed + 0.5) continue;
        }

        // The glyph's center on the curve:
        float thetaK = thetaMax - float(k) * dtheta;
        float thKArm = dir * thetaK + spin * dir + phase;
        float rK     = aRim * exp(b * thetaK);
        vec2  center = rK * vec2(cos(thKArm), sin(thKArm));

        // ── Tangent frame: glyph rotates to follow the curve ────────
        // Tangent angle of log-spiral: alpha = thetaArm + atan(1/b)
        // (the constant spiral pitch angle). Re-derive at this theta.
        float pitch = atan(1.0, b);  // pitch angle from radial
        float alpha = thKArm + dir * pitch;
        // Letters readable along curve: align glyph y-up with the
        // outward normal (perpendicular to tangent).
        float ca = cos(alpha);
        float sa = sin(alpha);

        // Pixel position in glyph-local frame. tangent = (ca, sa).
        vec2 d = pView - center;
        // Project onto tangent (along curve) and normal (radial).
        float along = d.x * ca + d.y * sa;
        float across = -d.x * sa + d.y * ca;

        // Optional: text across the radii instead of along the curve.
        // textAlong=1 → glyphs read tangent (default, like the reference);
        // textAlong=0 → glyphs sit radial (cross-axis).
        float ta01 = clamp(textAlong, 0.0, 1.0);
        if (ta01 < 0.999) {
            float along2  =  d.x * (-sa) + d.y * ca;
            float across2 =  d.x * ca   + d.y * sa;
            along  = mix(along2, along, ta01);
            across = mix(across2, across, ta01);
        }

        // Glyph local UV (0..1). Aspect 5:7 like the atlas.
        float halfW = gSize * 0.5 * (5.0 / 7.0);
        float halfH = gSize * 0.5;
        float u = (along  + halfW) / (2.0 * halfW);
        float v = (across + halfH) / (2.0 * halfH);

        // Skip if pixel is outside this glyph cell.
        if (u < 0.0 || u > 1.0 || v < 0.0 || v > 1.0) continue;

        float cov = glyphCoverage(ch, vec2(u, v));
        if (cov < 0.001) continue;

        // ── Ink shading: depth fades to paper, energy lifts highlight
        // ── + drop-shadow for real layered z. ───────────────────────
        // Depth fade: distant (eye-ward) glyphs blur into the paper.
        float depthFade = mix(1.0, 0.35, zNorm);
        // Energy lift: louder arm → fuller, darker ink + faint glow.
        float lift = 0.65 + 0.55 * e;
        vec3 ink   = inkBase * lift;
        // Atmospheric perspective: blend ink → paper as it recedes.
        ink = mix(paper, ink, depthFade);

        // Drop shadow offset (parallax cue: arm sits above paper by e).
        // Sample the same glyph slightly offset; cheaper to just darken
        // the silhouette by a hair on the back side.
        float shadowBias = 0.10 * e * (1.0 - zNorm);

        // Composite: ink over paper with coverage.
        float w = cov;
        // Soft glow halo around active arms (only when energy is up).
        float halo = 0.0;
        if (e > 0.02) {
            float dCurve = abs(crossArc);  // perpendicular distance to spiral
            halo = exp(-dCurve * 90.0) * e * 0.35;
        }
        vec3 added = ink * w + inkBase * halo * 0.6;

        // Over-compositing approximation across arms.
        coverAll = 1.0 - (1.0 - coverAll) * (1.0 - w);
        inkAll   = mix(inkAll, added, w * (1.0 - shadowBias));
    }

    // Compose ink over paper.
    col = mix(paper, inkAll / max(coverAll, 1e-3), coverAll);

    // Eye glow (subtle warm light bleeding from the vortex center) —
    // depth & dimensionality cue.
    float eyeGlow = exp(-length(pView) * 12.0) * (0.12 + 0.25 * bp);
    col += eyeGlow * vec3(0.92, 0.85, 0.70) * depthAmp;

    // Slow rake of light across paper for life when text is sparse.
    float rake = smoothstep(0.0, 0.5,
                            sin(uv.x * 1.7 - uv.y * 0.9 - TIME * 0.18) * 0.5 + 0.5);
    col += pow(rake, 4.0) * 0.025 * vec3(1.0, 0.97, 0.88);

    // Paper-tooth micro-modulation, never a pixel grid.
    float tooth = vnoise(uv * res.y * 0.018);
    col *= 1.0 + (tooth - 0.5) * 0.04;

    // Final tone curve — gentle.
    col = col / (1.0 + 0.10 * col);
    col = pow(max(col, 0.0), vec3(0.95));

    gl_FragColor = vec4(col, 1.0);
}
