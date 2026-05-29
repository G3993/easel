/*{
  "DESCRIPTION": "Gradient Box Lines Text — gallery-poster homage. Inside a thin paper-framed inner box, three watercolor gradient bodies (player[1..3].energy) bloom and parallax-drift at distinct z depths — back / mid / front — so each voice gets its own atmospheric cloud. Two sharp structural curves (a long S and a tighter loop) cross the frame, their crispness and density driven by audio. The cue.latest message is revealed as numbered list items along the inner curve via msgAge: one footnote chip per word, drawn in tiny serif-style glyphs with a superscript index. Header and footer plates hold poster typography; a thin chromatic foil strip sits along the bottom. Premium gallery-grade composition with real layered depth, audio-aware motion, and player-separable cloud bodies.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",          "TYPE": "text",  "DEFAULT": "BETWEEN THE TWO MY LIFE MOVES",  "MAX_LENGTH": 48 },

    { "NAME": "energyA",      "LABEL": "Player 1 Energy",     "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",      "LABEL": "Player 2 Energy",     "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",      "LABEL": "Player 3 Energy",     "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },

    { "NAME": "bassDrive",    "LABEL": "Bass → Line Density", "TYPE": "float", "DEFAULT": 0.7, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.bass" },
    { "NAME": "highDrive",    "LABEL": "High → Line Crisp",   "TYPE": "float", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.high" },

    { "NAME": "gradientAngle","LABEL": "Gradient Angle",      "TYPE": "float", "DEFAULT": 0.42, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "paletteShift", "LABEL": "Palette Shift",       "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "structureVariant","LABEL": "Structure Variant","TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["S-curve","Loop","Double","Helix"] },
    { "NAME": "lineDensity",  "LABEL": "Line Density",        "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.2, "MAX": 3.0 },
    { "NAME": "lineCrispness","LABEL": "Line Crispness",      "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.2, "MAX": 1.5 },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",        "TYPE": "float", "DEFAULT": 0.6, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "audioDepth",   "LABEL": "Audio Depth",         "TYPE": "float", "DEFAULT": 0.7, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "textSize",     "LABEL": "Footnote Size",       "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.5, "MAX": 2.0 },

    { "NAME": "colorA",       "LABEL": "Color · Back (Blue)",  "TYPE": "color", "DEFAULT": [0.32, 0.40, 0.72, 1.0] },
    { "NAME": "colorB",       "LABEL": "Color · Mid (Violet)", "TYPE": "color", "DEFAULT": [0.55, 0.40, 0.78, 1.0] },
    { "NAME": "colorC",       "LABEL": "Color · Front (Mint)", "TYPE": "color", "DEFAULT": [0.55, 0.86, 0.78, 1.0] },
    { "NAME": "paperColor",   "LABEL": "Paper Color",          "TYPE": "color", "DEFAULT": [0.965, 0.955, 0.935, 1.0] },
    { "NAME": "inkColor",     "LABEL": "Ink Color",            "TYPE": "color", "DEFAULT": [0.06, 0.06, 0.09, 1.0] },

    { "NAME": "showFrame",    "LABEL": "Show Frame",           "TYPE": "bool",  "DEFAULT": 1.0 },
    { "NAME": "showHeader",   "LABEL": "Header Plate",         "TYPE": "bool",  "DEFAULT": 1.0 },
    { "NAME": "showFooter",   "LABEL": "Foil Strip",           "TYPE": "bool",  "DEFAULT": 1.0 },
    { "NAME": "grain",        "LABEL": "Paper Grain",          "TYPE": "float", "DEFAULT": 0.45, "MIN": 0.0, "MAX": 1.0 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  GRADIENT BOX LINES TEXT  ·  three watercolor clouds inside a frame,
//  pierced by two structural curves; cue text falls onto the curve as
//  numbered footnotes.
//
//  Composition (matches the reference):
//   • Paper backdrop, off-white, soft vignette + grain.
//   • Inner thin-rule FRAME with poster proportions.
//   • Three layered gradient CLOUDS — each bound to its own player[i].
//     - Back  (blue, low z, soft DOF, slow drift)        → player[3]
//     - Mid   (violet, middle z, medium soft, mid drift) → player[2]
//     - Front (mint/iridescent fringe, sharp, fast)      → player[1]
//     Clouds use long-axis fbm-warped radial gradients so silhouettes
//     are NOT discs — they bloom asymmetrically like watercolor.
//   • Two STRUCTURAL CURVES — analytical, hand-traced thin lines —
//     cross the inner frame. Density modulated by audio.bass;
//     edge crispness modulated by audio.high.
//   • cue.latest → footnotes: msgAge typewriter places one numbered
//     chip per WORD at evenly-spaced t-values along the primary curve.
//     Tiny glyphs + superscript index. Each chip pops in on reveal.
//   • Header plate (small rule + serif-style block via the atlas) at top.
//   • Bottom: thin chromatic FOIL STRIP (small abstracted color band).
//
//  Depth: parallaxed z layers (clouds shift at different speeds vs.
//  mousePos), lines render OVER clouds at z=0 (frame z), footnote chips
//  render OVER lines at z=+1, paper backdrop at z=-2.
//
//  Motion: every frame energy-aware. Silence → near-still drift; bass
//  swells line density and cloud breath; high adds crisp edge shimmer;
//  per-player energy lifts each cloud independently.
// ════════════════════════════════════════════════════════════════════════

#define MAX_CHARS    48
#define SPACE_CH     26
#define MAX_WORDS    12
#define TAU          6.28318530718

// ─── Font atlas (shared idiom with text_clusters / gradient_text) ──────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
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
    if (n > MAX_CHARS) return MAX_CHARS;
    return n;
}

// ─── Hashes / noise ────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  hash22(vec2 p) {
    p = vec2(dot(p, vec2(127.1, 311.7)), dot(p, vec2(269.5, 183.3)));
    return fract(sin(p) * 43758.5453);
}
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash11(dot(i, vec2(1.0, 157.0)));
    float b = hash11(dot(i + vec2(1.0, 0.0), vec2(1.0, 157.0)));
    float c = hash11(dot(i + vec2(0.0, 1.0), vec2(1.0, 157.0)));
    float d = hash11(dot(i + vec2(1.0, 1.0), vec2(1.0, 157.0)));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}
float fbm2(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.07 + vec2(11.3, 5.7);
        a *= 0.5;
    }
    return v;
}

// ─── Word indexing (each cluster = one numbered footnote chip) ─────────
// Find the start index of the k-th whitespace-delimited word inside the
// message, and write the inclusive end (one past last glyph) to outEnd.
// If the word doesn't exist, outEnd <= outStart.
void wordRange(int k, int total, out int outStart, out int outEnd) {
    int seen = 0;
    int idx = 0;
    bool inWord = false;
    int wStart = -1;
    int wEnd = -1;
    for (int i = 0; i < MAX_CHARS; i++) {
        if (idx >= total) break;
        int ch = getChar(idx);
        bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
        if (!isSpace && !inWord) {
            inWord = true;
            wStart = idx;
        }
        if ((isSpace && inWord) || (idx == total - 1 && !isSpace)) {
            if (!isSpace) { wEnd = idx + 1; }
            else          { wEnd = idx; }
            if (seen == k) { outStart = wStart; outEnd = wEnd; return; }
            seen++;
            inWord = false;
        }
        idx++;
    }
    outStart = 0; outEnd = 0;
}

int countWords(int total) {
    int seen = 0;
    bool inWord = false;
    for (int i = 0; i < MAX_CHARS; i++) {
        if (i >= total) break;
        int ch = getChar(i);
        bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
        if (!isSpace && !inWord) { seen++; inWord = true; }
        else if (isSpace)        { inWord = false; }
    }
    return seen;
}

// ─── Structural curve evaluator ────────────────────────────────────────
// Returns position on the curve for parameter t∈[0,1], plus the unit
// tangent (so footnote chips align along the line).
// `variant` selects one of four hand-feeling curves: S, Loop, Double, Helix.
void curveEval(int variant, float t, float aspect, float wobble,
               out vec2 pos, out vec2 tang) {
    // Curve domain: roughly the inner frame extents.
    float W = aspect * 0.78;
    float H = 0.78;
    float x, y;
    float dx, dy;
    if (variant == 0) {
        // Long S — like the reference. Travels diagonally with one big
        // inflection so the long inside-arc reads like the poster.
        x = (t - 0.5) * W;
        y = 0.5 * H * sin(t * 3.4 - 0.6) * cos(t * 1.1);
        dx = W;
        dy = 0.5 * H * (3.4 * cos(t * 3.4 - 0.6) * cos(t * 1.1)
                       -1.1 * sin(t * 3.4 - 0.6) * sin(t * 1.1));
    } else if (variant == 1) {
        // Tighter loop (lower-right closure echoing the reference).
        float a = t * TAU * 0.9 + 0.4;
        float r = mix(0.18, 0.42, t);
        x = cos(a) * r * W * 0.9 + 0.05 * W;
        y = sin(a) * r * H * 0.9 - 0.08 * H;
        dx = -sin(a) * r * W * 0.9 * TAU * 0.9 + cos(a) * (0.42 - 0.18) * W * 0.9;
        dy =  cos(a) * r * H * 0.9 * TAU * 0.9 + sin(a) * (0.42 - 0.18) * H * 0.9;
    } else if (variant == 2) {
        // Double swoop — two opposing waves that meet near center.
        x = (t - 0.5) * W;
        y = 0.32 * H * sin(t * TAU * 0.5) * (1.0 - 2.0 * abs(t - 0.5));
        dx = W;
        dy = 0.32 * H * (TAU * 0.5 * cos(t * TAU * 0.5) * (1.0 - 2.0 * abs(t - 0.5))
                       + sin(t * TAU * 0.5) * (t < 0.5 ? 2.0 : -2.0));
    } else {
        // Faux-3D helix (projects a vertical helix to 2D — perspective fake).
        float a = t * TAU * 1.2;
        x = sin(a) * 0.30 * W + (t - 0.5) * W * 0.3;
        y = (t - 0.5) * H * 1.05;
        dx = cos(a) * 0.30 * W * TAU * 1.2 + W * 0.3;
        dy = H * 1.05;
    }
    // Hand-drawn wobble — tiny low-amp fbm offset along the normal.
    vec2 t0 = normalize(vec2(dx, dy));
    vec2 n0 = vec2(-t0.y, t0.x);
    float wob = (fbm2(vec2(t * 6.0, 7.7)) - 0.5) * wobble;
    pos  = vec2(x, y) + n0 * wob;
    tang = t0;
}

// Compute distance from point p to the curve by sampling N points.
// Returns dist + at the best sample writes out the t-value.
float curveDist(int variant, vec2 p, float aspect, float wobble,
                out float bestT) {
    float best = 1e6;
    bestT = 0.0;
    const int N = 96;
    for (int i = 0; i < N; i++) {
        float t = float(i) / float(N - 1);
        vec2 cp, ct;
        curveEval(variant, t, aspect, wobble, cp, ct);
        float d = length(p - cp);
        if (d < best) { best = d; bestT = t; }
    }
    return best;
}

// ─── Cloud: long-axis warped gradient, NOT a disc ──────────────────────
// `c`     — cloud center in aspect-corrected space
// `axis`  — long-axis unit vector (the gradient stretches along this)
// `rx,ry` — semi-axes (rx = long, ry = short)
// `tint`  — base color
// `e`     — energy 0..2 (silence ≈ 0, bloom ≈ 1.5+)
// `t`     — time (already speed-scaled)
// `seed`  — per-cloud randomness phase
// `soft`  — DOF softness (1.0 = neutral, >1 softer / further back)
// Returns linear-HDR additive color contribution.
vec3 cloud(vec2 p, vec2 c, vec2 axis, float rx, float ry,
           vec3 tint, float e, float t, float seed, float soft) {
    vec2 d = p - c;
    // Project to long/short axis local coords.
    vec2 nx = axis;
    vec2 ny = vec2(-axis.y, axis.x);
    float u = dot(d, nx) / rx;
    float v = dot(d, ny) / ry;
    // Long-axis warp via fbm — gives the painter-stretch silhouette.
    float warp = fbm2(vec2(u * 1.6 + t * 0.2, v * 1.6 + seed)) * 0.6;
    float u2 = u + warp * 0.4;
    float v2 = v + (warp - 0.3) * 0.6;
    float r  = length(vec2(u2, v2));
    // Falloff — long, asymmetric. Bigger e → wider bell + softer tail.
    float fall = exp(-pow(r / max(0.45 + 0.55 * e, 0.05), 1.7) * (2.0 / soft));
    // Inner over-bright lobe — gives that watercolor saturation core.
    float core = exp(-pow(r * 1.8, 2.0)) * (0.5 + 0.6 * e);
    float val = fall * 0.85 + core * 0.55;
    // Iridescent fringe on energy peaks (subtle hue shift along radius).
    vec3 fringe = 0.5 + 0.5 * cos(TAU * (r * 1.3 + seed + vec3(0.0, 0.33, 0.66)));
    vec3 col = mix(tint, fringe, 0.18 * smoothstep(0.6, 1.4, e));
    return col * val;
}

// ─── Header / footer plate helpers ─────────────────────────────────────
float rectMask(vec2 p, vec2 c, vec2 hs, float feather) {
    vec2 q = abs(p - c) - hs;
    float outside = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);
    return 1.0 - smoothstep(0.0, feather, outside);
}

// Thin rule line (filled-rect band).
float ruleBand(vec2 p, float yCenter, float yHeight, float xMin, float xMax) {
    if (p.x < xMin || p.x > xMax) return 0.0;
    return 1.0 - smoothstep(yHeight * 0.5, yHeight, abs(p.y - yCenter));
}

// Foil strip — chromatic abstract bar (no logos, just color band).
vec3 foilStrip(vec2 p, float y0, float y1, float xL, float xR, float t) {
    if (p.x < xL || p.x > xR) return vec3(0.0);
    if (p.y < y0 || p.y > y1) return vec3(0.0);
    float u = (p.x - xL) / max(xR - xL, 1e-3);
    // Soft palette across the bar — warm → cool → green sliver
    vec3 a = vec3(0.92, 0.62, 0.32);  // amber
    vec3 b = vec3(0.95, 0.55, 0.55);  // coral
    vec3 c = vec3(0.55, 0.55, 0.86);  // periwinkle
    vec3 d = vec3(0.32, 0.32, 0.66);  // indigo
    vec3 e = vec3(0.86, 0.86, 0.55);  // pale yellow
    vec3 col;
    if      (u < 0.20) col = mix(a, b, smoothstep(0.0,  0.20, u));
    else if (u < 0.45) col = mix(b, c, smoothstep(0.20, 0.45, u));
    else if (u < 0.70) col = mix(c, d, smoothstep(0.45, 0.70, u));
    else               col = mix(d, e, smoothstep(0.70, 1.0,  u));
    // Tiny vertical wobble inside the bar so the edge is not a knife
    float wob = (fbm2(vec2(p.x * 30.0, t * 0.5)) - 0.5) * 0.02;
    float band = 1.0 - smoothstep(0.0, (y1 - y0) * 0.5 + wob,
                                  abs(p.y - 0.5 * (y0 + y1)));
    return col * band;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / max(res.y, 1.0);

    // Aspect-corrected centered coords. y up.
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = (uv.y - 0.5);

    float t    = TIME * motionSpeed;
    float bass = clamp(audioBass * bassDrive, 0.0, 2.0);
    float high = clamp(audioHigh * highDrive, 0.0, 2.0);

    // Energies — clamp + small audio.bass floor so audio-only setups still
    // animate. Each cloud responds to its OWN channel; A=front, C=back.
    float eA = clamp(energyA + bass * 0.15 * audioDepth, 0.0, 1.8);
    float eB = clamp(energyB + bass * 0.10 * audioDepth, 0.0, 1.8);
    float eC = clamp(energyC + bass * 0.08 * audioDepth, 0.0, 1.8);

    // Mouse-driven parallax: shifts each cloud layer differently → depth.
    vec2 cam = (mousePos - 0.5) * audioDepth * 0.18;

    // ── Paper backdrop ────────────────────────────────────────────────
    vec3 paper = paperColor.rgb;
    paper *= 1.0 - 0.10 * dot(p, p);                       // vignette
    float marb = fbm2(p * 1.5 + 7.0);
    paper *= 1.0 + (marb - 0.5) * 0.04;
    vec3 col = paper;

    // ── Inner frame box (poster rule) ─────────────────────────────────
    // Aspect-aware inner frame, slightly portrait so it always fits both
    // landscape & square host canvases. Top reserved for header plate.
    float frameW = min(aspect, 1.6) * 0.74;
    float frameH = 0.82;
    vec2 frameC  = vec2(0.0, -0.04);
    // Thin black rule around the frame
    if (showFrame) {
        float fwidthRule = 0.0035;
        float outerMask = 1.0 - rectMask(p, frameC,
                                          vec2(frameW * 0.5, frameH * 0.5),
                                          fwidthRule * 1.5);
        float innerMask = 1.0 - rectMask(p, frameC,
                                          vec2(frameW * 0.5 - fwidthRule,
                                               frameH * 0.5 - fwidthRule),
                                          fwidthRule * 1.5);
        float ruleMask  = clamp(innerMask - outerMask, 0.0, 1.0);
        col = mix(col, inkColor.rgb, ruleMask * 0.85);
    }

    // ── Three layered gradient clouds (back → mid → front) ────────────
    // Apply parallax offsets per layer (back moves least). Each cloud
    // has its own long-axis (angle slider rotates them as a group), its
    // own center wander, and its own DOF softness.
    float ga = gradientAngle * TAU;
    vec2 axisRoot = vec2(cos(ga), sin(ga));

    // Per-cloud anchor centers — drift slowly inside the frame.
    vec2 cBack = vec2( 0.05 * sin(t * 0.18 + 1.3),
                       0.06 * cos(t * 0.15 + 0.9)) - cam * 0.30;
    cBack += frameC + vec2(-0.10, 0.12) * frameW;
    vec2 cMid  = vec2( 0.07 * cos(t * 0.21 + 0.4),
                       0.05 * sin(t * 0.17 + 2.1)) - cam * 0.65;
    cMid  += frameC + vec2( 0.04,-0.02) * frameW;
    vec2 cFro  = vec2( 0.09 * sin(t * 0.27 + 2.6),
                       0.07 * cos(t * 0.23 + 1.5)) - cam * 1.10;
    cFro  += frameC + vec2( 0.12, 0.05) * frameW;

    // Long-axis vectors per cloud — slightly rotated relative to root so
    // they fan out (real composition has overlapping non-parallel blooms).
    vec2 axA = vec2(cos(ga + 0.30), sin(ga + 0.30));
    vec2 axB = axisRoot;
    vec2 axC = vec2(cos(ga - 0.40), sin(ga - 0.40));

    // Palette shift across all three tints (slow hue rotation).
    float ps = paletteShift;
    vec3 cA = mix(colorC.rgb, colorC.rgb.brg, ps);  // front (mint)
    vec3 cB = mix(colorB.rgb, colorB.rgb.gbr, ps);  // mid (violet)
    vec3 cBk = mix(colorA.rgb, colorA.rgb.gbr, ps); // back (blue)

    // Cloud sizes — bass swell breathes all three but each by a different
    // amount so they don't move as a single field.
    float swell = 1.0 + 0.18 * bass;
    vec3 back  = cloud(p, cBack, axB, 0.55 * frameW * swell,
                       0.34 * frameH * swell, cBk, eC, t * 0.7, 1.3, 1.8);
    vec3 midL  = cloud(p, cMid, axC, 0.48 * frameW * (1.0 + 0.14 * bass),
                       0.30 * frameH * (1.0 + 0.14 * bass), cB, eB,
                       t * 1.0, 3.7, 1.2);
    vec3 front = cloud(p, cFro, axA, 0.36 * frameW * (1.0 + 0.10 * bass),
                       0.22 * frameH * (1.0 + 0.10 * bass), cA, eA,
                       t * 1.4, 7.1, 0.85);

    // Clip clouds to the inner frame (soft).
    float clipMask = rectMask(p, frameC,
                              vec2(frameW * 0.5 - 0.004,
                                   frameH * 0.5 - 0.004),
                              0.018);
    back  *= clipMask;
    midL  *= clipMask;
    front *= clipMask;

    // Composite: back → mid → front with screen-ish add.
    col += back  * 0.95;
    col += midL  * 1.05;
    col += front * 1.10;

    // ── Structural lines (sharp, OVER clouds) ─────────────────────────
    // Primary curve (user-chosen variant). Wobble shrinks with audio.high
    // (high → crisp lines). Width modulated by density × lineDensity.
    int variant = int(structureVariant);
    if (variant < 0) variant = 0;
    if (variant > 3) variant = 3;
    float wobble = 0.012 * (1.0 - clamp(high * 0.6, 0.0, 0.9));
    float bestT;
    float dPrim = curveDist(variant, p - frameC, aspect,
                            wobble, bestT);
    float lineW = 0.0030 * lineDensity * (1.0 + 0.4 * bass);
    float crisp = lineCrispness * (0.7 + 0.6 * (1.0 - high * 0.3));
    float primLine = 1.0 - smoothstep(lineW, lineW * (1.0 + crisp), dPrim);
    // Secondary curve — always the loop, lighter weight, gives the
    // "two-curve" look from the reference even when primary is the S.
    int secondary = (variant == 1) ? 0 : 1;
    float bestT2;
    float dSec = curveDist(secondary, p - frameC, aspect,
                           wobble * 1.1, bestT2);
    float lineW2 = 0.0021 * lineDensity * (1.0 + 0.3 * bass);
    float secLine = 1.0 - smoothstep(lineW2, lineW2 * (1.0 + crisp), dSec);
    // Inside-frame clip for the lines too.
    primLine *= clipMask;
    secLine  *= clipMask * 0.65;
    col = mix(col, inkColor.rgb, clamp(primLine + secLine, 0.0, 1.0));

    // ── Numbered footnote chips along the primary curve ───────────────
    int total = charCount();
    bool live = msgAge >= 0.0;
    if (total > 0) {
        int words = countWords(total);
        if (words > MAX_WORDS) words = MAX_WORDS;
        if (words < 1) words = 1;

        // Reveal pacing — each word births at a fraction of msgAge.
        const float SECS_PER_WORD = 0.45;

        for (int w = 0; w < MAX_WORDS; w++) {
            if (w >= words) break;
            float fw = float(w);
            float tBirth = fw * SECS_PER_WORD;
            float age    = live ? (msgAge - tBirth) : 1e6;
            if (age < 0.0) continue;
            float popIn  = clamp(age / 0.30, 0.0, 1.0);
            float fade   = smoothstep(0.0, 0.30, age);

            // Word's t along the curve — evenly spaced; small per-word
            // jitter so chips don't sit on a perfect grid.
            float tCurve = (fw + 0.5) / float(words);
            tCurve = clamp(tCurve, 0.04, 0.96);
            tCurve += (hash11(fw * 3.7) - 0.5) * 0.025;

            vec2 cPos, cTan;
            curveEval(variant, tCurve, aspect, wobble, cPos, cTan);
            cPos += frameC;
            // Push the chip slightly OFF the curve along its normal so
            // the number sits in the white margin, not on top of the line.
            vec2 cNor = vec2(-cTan.y, cTan.x);
            // Alternate sides per word so chips don't pile on one side.
            float side = (mod(fw, 2.0) < 0.5) ? 1.0 : -1.0;
            cPos += cNor * 0.028 * side;

            // Local coords inside the chip — chip is left-aligned at cPos.
            vec2 lp = p - cPos;

            // Get the word's character range.
            int wStart, wEnd;
            wordRange(w, total, wStart, wEnd);
            if (wEnd <= wStart) continue;

            // Glyph metrics — tiny serif-style footnote.
            float charH = 0.018 * textSize * mix(0.6, 1.0, popIn);
            float charW = charH * (5.0 / 7.0);
            float kern  = charW * 1.05;

            // Superscript index — half-height digits ABOVE the baseline.
            // (We render the LSB of the index as a digit; for >9 we just
            //  show the last digit — keeps it visual, not literal counter.)
            float idxH = charH * 0.55;
            float idxW = idxH * (5.0 / 7.0);
            float idxKern = idxW * 1.05;
            int wIdx = (w + 1);
            int digit = wIdx - (wIdx / 10) * 10;
            // Atlas digits live at index 26+... wait — atlas index 0..25
            // are letters A-Z, 26 is space, 27..36 are digits 0..9.
            int digitCh = 27 + digit;

            // Draw the index glyph first (above baseline, left of word).
            {
                float dxLocal = lp.x;
                float dyLocal = lp.y - charH * 0.55;   // superscript lifted
                if (dxLocal >= 0.0 && dxLocal <= idxW
                    && dyLocal >= 0.0 && dyLocal <= idxH) {
                    vec2 gUv = vec2(dxLocal / idxW, dyLocal / idxH);
                    float s = sampleChar(digitCh, gUv);
                    float wgt = fwidth(s) + 1e-4;
                    float glyph = smoothstep(0.5 - wgt, 0.5 + wgt, s) * fade;
                    if (glyph > 0.001) {
                        col = mix(col, inkColor.rgb, clamp(glyph, 0.0, 1.0));
                    }
                }
            }

            // Draw the word's glyphs to the right of the index.
            float wordX0 = idxW + idxKern * 0.4;   // small gap after index
            int wn = wEnd - wStart;
            if (wn > 16) wn = 16;
            for (int gi = 0; gi < 16; gi++) {
                if (gi >= wn) break;
                int ch = getChar(wStart + gi);
                if (ch < 0 || ch > 35 || ch == SPACE_CH) continue;
                float gx0 = wordX0 + float(gi) * kern;
                float gx1 = gx0 + charW;
                float dxLocal = lp.x - gx0;
                float dyLocal = lp.y;            // baseline at y=0
                if (lp.x < gx0 || lp.x > gx1) continue;
                if (dyLocal < 0.0 || dyLocal > charH) continue;
                vec2 gUv = vec2(dxLocal / charW, dyLocal / charH);
                float s = sampleChar(ch, gUv);
                float wgt = fwidth(s) + 1e-4;
                float glyph = smoothstep(0.5 - wgt, 0.5 + wgt, s) * fade;
                if (glyph > 0.001) {
                    col = mix(col, inkColor.rgb, clamp(glyph, 0.0, 1.0));
                }
            }
        }
    }

    // ── Header plate ──────────────────────────────────────────────────
    // Top-left small black rule + small uppercase title via the atlas.
    if (showHeader) {
        // Thin black rule
        float ruleY = 0.42;
        float rule = ruleBand(p, ruleY, 0.004,
                              -aspect * 0.40, -aspect * 0.05);
        col = mix(col, inkColor.rgb, rule * 0.95);

        // Tiny header text along the rule — atlas glyphs of the first
        // word of the message, scaled small. If no msg, skip.
        if (total > 0) {
            int wStart, wEnd;
            wordRange(0, total, wStart, wEnd);
            int hn = wEnd - wStart;
            if (hn > 12) hn = 12;
            float chH = 0.022;
            float chW = chH * (5.0 / 7.0);
            float kern = chW * 1.05;
            float xStart = aspect * 0.06;
            float yBase  = 0.40;
            for (int gi = 0; gi < 12; gi++) {
                if (gi >= hn) break;
                int ch = getChar(wStart + gi);
                if (ch < 0 || ch > 35 || ch == SPACE_CH) continue;
                float gx0 = xStart + float(gi) * kern;
                float dxLocal = p.x - gx0;
                float dyLocal = p.y - yBase;
                if (p.x < gx0 || p.x > gx0 + chW) continue;
                if (dyLocal < 0.0 || dyLocal > chH) continue;
                vec2 gUv = vec2(dxLocal / chW, dyLocal / chH);
                float s = sampleChar(ch, gUv);
                float wgt = fwidth(s) + 1e-4;
                float glyph = smoothstep(0.5 - wgt, 0.5 + wgt, s);
                col = mix(col, inkColor.rgb, glyph);
            }
        }
    }

    // ── Foil strip + footer (bottom) ──────────────────────────────────
    if (showFooter) {
        // Foil bar — short, sits inside lower-middle of frame
        vec3 foil = foilStrip(p,
                              -0.49, -0.465,
                              -aspect * 0.10, aspect * 0.30,
                              t);
        col += foil * 0.95;
    }

    // ── Paper grain (continuous noise, never a pixel grid) ────────────
    if (grain > 0.001) {
        float g = fbm2(p * res.y * 0.018)
                + 0.5 * fbm2(p * res.y * 0.05 + 17.0);
        col *= 1.0 + (g - 0.75) * grain * 0.10;
    }

    // Soft contrast — keeps blacks rich without hard clip; very subtle
    // bloom lift on energy crescendos so silence really reads as still.
    float energySum = eA + eB + eC;
    col += smoothstep(1.0, 3.0, energySum) * 0.04 * (front + midL * 0.5);
    col = col / (1.0 + 0.25 * col);
    col = pow(max(col, 0.0), vec3(0.95));

    gl_FragColor = vec4(col, 1.0);
}
