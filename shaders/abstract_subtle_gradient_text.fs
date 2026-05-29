/*{
  "DESCRIPTION": "Abstract Subtle Gradient Text — quiet editorial composition. A muted three-stop gradient field (warm rose / mint / amber-magenta) breathes across a paper canvas; three player.energy channels each own one bloom so silent voices fade and active voices warm the field. Over it, a sparse vocabulary of abstract editorial marks — rounded bars, a thin tilted outline rectangle, a small star, a wavy line, a tiny grid, and a calendar-style numeric arc — float at three z-depths with mouse parallax. The cue.latest message typewriter-reveals as a small dated caption row beside the arc. Restraint is the move: low energy is nearly still, crescendos warm the field rather than thrash it. Returns LINEAR HDR — host applies ACES.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",            "TYPE": "text",  "DEFAULT": "AUGUST FIELD NOTES", "MAX_LENGTH": 48 },

    { "NAME": "energyA",        "LABEL": "Player 1 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",        "LABEL": "Player 2 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",        "LABEL": "Player 3 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "audioDepth",     "LABEL": "Audio Depth",       "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.level" },
    { "NAME": "bassDrive",      "LABEL": "Bass Drive",        "TYPE": "float", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.bass" },

    { "NAME": "shapeVariant",   "LABEL": "Shape Variant",     "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2], "LABELS": ["Editorial","Sparse","Dense"] },
    { "NAME": "palette",        "LABEL": "Palette",           "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Bloom","Cool","Warm","Pastel"] },
    { "NAME": "motionSpeed",    "LABEL": "Motion Speed",      "TYPE": "float", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "gradientSoft",   "LABEL": "Gradient Softness", "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.4, "MAX": 2.2 },
    { "NAME": "parallax",       "LABEL": "Parallax Depth",    "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 2.0 },

    { "NAME": "textSize",       "LABEL": "Caption Size",      "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.5, "MAX": 2.0 },
    { "NAME": "paperColor",     "LABEL": "Paper Color",       "TYPE": "color", "DEFAULT": [0.97, 0.96, 0.95, 1.0] },
    { "NAME": "inkColor",       "LABEL": "Ink Color",         "TYPE": "color", "DEFAULT": [0.06, 0.05, 0.07, 1.0] },
    { "NAME": "grain",          "LABEL": "Paper Grain",       "TYPE": "float", "DEFAULT": 0.28, "MIN": 0.0, "MAX": 1.0 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//   ABSTRACT · SUBTLE GRADIENT · TEXT
//
//   A quiet editorial composition. Three layered z-planes:
//
//     z=0 (back)   subtle three-stop gradient field. Each stop is
//                  owned by a player.energy channel; silent players
//                  vanish from the field.
//     z=1 (mid)    abstract shape vocabulary — rounded bars, thin
//                  outline rectangle on a slant, tiny star, wavy
//                  line, small grid, dot row.
//     z=2 (front)  calendar-style numeric arc (1..30) and the
//                  typewriter caption (cue.latest via msgAge).
//
//   Restraint is the design move. Low energy = nearly still field
//   (gradient breathes only); crescendos warm the field and gently
//   lift shapes — no thrashing.
// ════════════════════════════════════════════════════════════════════════

#define MAX_CHARS    48
#define SPACE_CH     26
#define TAU          6.28318530718

// ─── Font atlas (Shader-Claw idiom) ─────────────────────────────────────
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

// Atlas digit indices: '0'..'9' live at 27..36 (matches the rest of the corpus).
int digitGlyph(int d) {
    if (d < 0 || d > 9) return -1;
    return 27 + d;
}

// ─── Hash & noise ───────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  hash21(float n) { return vec2(hash11(n), hash11(n + 17.31)); }
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0 - 2.0*f);
    float a = hash11(dot(i,                vec2(1.0, 157.0)));
    float b = hash11(dot(i + vec2(1.0,0.0),vec2(1.0, 157.0)));
    float c = hash11(dot(i + vec2(0.0,1.0),vec2(1.0, 157.0)));
    float d = hash11(dot(i + vec2(1.0,1.0),vec2(1.0, 157.0)));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}
float fbm2(vec2 p) {
    float v = 0.0, a = 0.55;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p  = p * 1.97 + vec2(11.3, 5.7);
        a *= 0.5;
    }
    return v;
}

// ─── SDF helpers ────────────────────────────────────────────────────────
float sdRoundedBar(vec2 p, vec2 hb, float r) {
    vec2 q = abs(p) - hb + vec2(r);
    return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
}
float sdCircle(vec2 p, float r) { return length(p) - r; }
float sdRing(vec2 p, float r, float w) {
    return abs(length(p) - r) - w * 0.5;
}
// Thin-outline rectangle (rotated). Returns SDF of the rectangle edge band.
float sdRectOutline(vec2 p, vec2 hb, float thick) {
    float box = max(abs(p.x) - hb.x, abs(p.y) - hb.y);
    return abs(box) - thick * 0.5;
}
// Four-pointed star (think compass spark). p in shape-local coords.
float sdStar4(vec2 p, float r) {
    vec2 q = abs(p);
    // Bias: pinch on the diagonal so the four arms read sharp.
    float a = q.x + q.y;
    float b = max(q.x, q.y);
    return mix(a, b, 0.62) - r;
}
// Wavy horizontal line: SDF distance to y = amp*sin(freq*x).
float sdWave(vec2 p, float amp, float freq, float thick) {
    float y = amp * sin(p.x * freq);
    float dy = abs(p.y - y);
    // Approximate distance (good enough for thin lines).
    float slope = amp * freq * cos(p.x * freq);
    float norm  = sqrt(1.0 + slope * slope);
    return dy / norm - thick * 0.5;
}

// Anti-aliased fill mask from an SDF using fwidth.
float fillMask(float d) {
    float fw = max(fwidth(d), 1e-4);
    return 1.0 - smoothstep(-fw, fw, d);
}

// ─── Palettes ───────────────────────────────────────────────────────────
// Each palette returns three muted bloom colors keyed to player[1..3].
// Restraint: saturation kept low; tints, not poster colors.
void palettePick(int idx, out vec3 cA, out vec3 cB, out vec3 cC) {
    if      (idx == 1) { // Cool — sage / mist / dusty blue
        cA = vec3(0.62, 0.72, 0.68);
        cB = vec3(0.78, 0.82, 0.86);
        cC = vec3(0.58, 0.66, 0.78);
    } else if (idx == 2) { // Warm — peach / apricot / faded rose
        cA = vec3(0.95, 0.78, 0.66);
        cB = vec3(0.94, 0.84, 0.62);
        cC = vec3(0.90, 0.66, 0.62);
    } else if (idx == 3) { // Pastel — lavender / mint / butter
        cA = vec3(0.82, 0.78, 0.92);
        cB = vec3(0.76, 0.90, 0.82);
        cC = vec3(0.94, 0.90, 0.74);
    } else {              // Bloom (default) — the reference image's voice
        cA = vec3(0.96, 0.62, 0.58);   // warm rose
        cB = vec3(0.66, 0.88, 0.78);   // mint
        cC = vec3(0.92, 0.74, 0.58);   // amber
    }
}

// ─── Main ───────────────────────────────────────────────────────────────
void main() {
    vec2 res    = RENDERSIZE;
    vec2 uv     = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    // Audio-shaped time. Idle motion is faint; energy lifts it gently.
    float eA = clamp(energyA, 0.0, 1.0);
    float eB = clamp(energyB, 0.0, 1.0);
    float eC = clamp(energyC, 0.0, 1.0);
    float lvl    = clamp(audioDepth, 0.0, 2.0);
    float bass   = clamp(bassDrive, 0.0, 2.0);
    float totalE = clamp((eA + eB + eC) / 3.0, 0.0, 1.0);
    // "Restraint" curve: at totalE ≈ 0 motion is ~15% of normal; at totalE=1
    // it reaches ~120%. This is what gives the silence its weight.
    float motionGain = mix(0.15, 1.2, smoothstep(0.0, 0.85, totalE + 0.25 * lvl));
    float tBase = TIME * motionSpeed * motionGain;

    // Mouse parallax (gentle — this is editorial, not a window into a room).
    vec2 mShift = (mousePos - 0.5) * 0.06 * parallax;

    // ── PALETTE ────────────────────────────────────────────────────────
    vec3 cA, cB, cC;
    palettePick(int(palette), cA, cB, cC);

    // ── z=0 · SUBTLE GRADIENT FIELD ────────────────────────────────────
    // Three soft Gaussian blooms, each owned by a player. Centres breathe
    // slowly; energy puffs their reach. fbm warps the centres so the field
    // is never axis-aligned — it feels painted.
    float soft = max(gradientSoft, 0.4);
    float warpA = fbm2(p * 0.9 + vec2(tBase * 0.08,  tBase * 0.05));
    float warpB = fbm2(p * 1.1 + vec2(-tBase * 0.06, tBase * 0.07));
    vec2 wp = p + 0.18 * vec2(warpA - 0.5, warpB - 0.5);

    // Anchor positions tuned to evoke the reference (heaviest bloom lower-left).
    vec2 pA = vec2(-0.42, -0.18) + 0.05 * vec2(sin(tBase*0.21), cos(tBase*0.17));
    vec2 pB = vec2( 0.12,  0.06) + 0.04 * vec2(cos(tBase*0.19), sin(tBase*0.23));
    vec2 pC = vec2(-0.05, -0.34) + 0.05 * vec2(sin(tBase*0.15 + 1.7), cos(tBase*0.18 + 0.9));

    // Per-stop reach scales with that player's energy. Silent player → bloom
    // shrinks to a faint smudge (never zero — the field still breathes).
    float rA = (0.55 + 0.35 * eA) * soft;
    float rB = (0.50 + 0.30 * eB) * soft;
    float rC = (0.62 + 0.25 * eC) * soft;

    // Gaussian weights.
    float wA = exp(-dot(wp - pA, wp - pA) / (rA * rA));
    float wB = exp(-dot(wp - pB, wp - pB) / (rB * rB));
    float wC = exp(-dot(wp - pC, wp - pC) / (rC * rC));

    // Field intensity. Energy lifts the contrast; quiet is a near-flat paper.
    float reach = mix(0.35, 0.85, smoothstep(0.0, 0.8, totalE + 0.25 * lvl));
    vec3 field = paperColor.rgb;
    field = mix(field, cA, clamp(wA * reach, 0.0, 1.0));
    field = mix(field, cB, clamp(wB * reach * 0.85, 0.0, 1.0));
    field = mix(field, cC, clamp(wC * reach * 0.9,  0.0, 1.0));

    // A slow, low-frequency hue warp so the gradient is never a perfect lerp.
    float hueWarp = fbm2(p * 1.6 + tBase * 0.04);
    field += (hueWarp - 0.5) * 0.04 * vec3(1.0, 0.95, 1.05);

    // Subtle vignette toward white edges (paper, not stage).
    field *= 1.0 - 0.10 * dot(p, p);

    vec3 col = field;

    // ── z=1 · ABSTRACT SHAPES ──────────────────────────────────────────
    // Shapes are dark editorial marks. Density follows shapeVariant.
    // We accumulate the darkest mark per pixel (max of fill masks).
    vec3 ink = inkColor.rgb;
    float shapeMask = 0.0;

    // Slight z=1 parallax (front-of-field).
    vec2 ps = p + mShift * 0.7;

    int variant = int(shapeVariant);

    // (a) Heavy bar — upper area. Echo the reference's two black bars.
    {
        vec2 c   = vec2(-0.18,  0.36) + 0.01 * vec2(sin(tBase*0.30), cos(tBase*0.27));
        vec2 q   = ps - c;
        float d  = sdRoundedBar(q, vec2(0.16, 0.022), 0.022);
        shapeMask = max(shapeMask, fillMask(d));
    }
    // (b) Second bar — lower-right.
    {
        vec2 c  = vec2(0.20, -0.18) + 0.01 * vec2(cos(tBase*0.22), sin(tBase*0.25));
        vec2 q  = ps - c;
        float d = sdRoundedBar(q, vec2(0.13, 0.02), 0.02);
        shapeMask = max(shapeMask, fillMask(d));
    }
    // (c) Tilted thin outline rectangle — the reference's hero element.
    {
        vec2 c = vec2(0.08, 0.30) + 0.012 * vec2(sin(tBase*0.18), cos(tBase*0.20));
        vec2 q = ps - c;
        // Rotate by ~-22°
        float ang = -0.38 + 0.04 * sin(tBase * 0.12);
        float ca = cos(ang), sa = sin(ang);
        q = mat2(ca, -sa, sa, ca) * q;
        float d = sdRectOutline(q, vec2(0.20, 0.028), 0.004);
        shapeMask = max(shapeMask, fillMask(d) * 0.92);
    }
    // (d) Small star — east-of-centre.
    {
        vec2 c  = vec2(0.16, 0.22);
        float d = sdStar4(ps - c, 0.022);
        shapeMask = max(shapeMask, fillMask(d));
    }
    // (e) Wavy line — upper-right.
    if (variant != 1) {
        vec2 c  = vec2(0.30, 0.30);
        vec2 q  = ps - c;
        float d = sdWave(q, 0.013, 36.0, 0.006);
        // Localize to a short segment.
        float seg = 1.0 - smoothstep(0.10, 0.16, abs(q.x));
        shapeMask = max(shapeMask, fillMask(d) * seg);
    }
    // (f) Tiny grid — right side. Skip in sparse mode.
    if (variant != 1) {
        vec2 c  = vec2(0.36, 0.04);
        vec2 q  = ps - c;
        float gridD = 1e6;
        for (int i = 0; i < 3; i++) {
            for (int j = 0; j < 2; j++) {
                vec2 cell = q - vec2(float(i) * 0.022 - 0.022, float(j) * 0.022 - 0.011);
                float dc = sdCircle(cell, 0.008);
                gridD = min(gridD, dc);
            }
        }
        shapeMask = max(shapeMask, fillMask(gridD));
    }
    // (g) Small dot row — lower-centre. Sparse mode keeps this.
    {
        vec2 c  = vec2(-0.10, -0.30);
        vec2 q  = ps - c;
        float dotD = 1e6;
        int n = (variant == 2) ? 9 : 6;
        for (int i = 0; i < 9; i++) {
            if (i >= n) break;
            vec2 d2 = q - vec2(float(i) * 0.022, 0.0);
            dotD = min(dotD, sdCircle(d2, 0.006));
        }
        shapeMask = max(shapeMask, fillMask(dotD) * 0.75);
    }
    // (h) Dense mode: scatter a few extra micro-marks driven by player C.
    if (variant == 2) {
        for (int i = 0; i < 6; i++) {
            float fi = float(i);
            vec2 sp = (hash21(fi * 9.7) - 0.5) * vec2(aspect * 0.9, 0.85);
            vec2 q  = ps - sp;
            float d = sdCircle(q, 0.004 + 0.003 * hash11(fi * 5.1));
            shapeMask = max(shapeMask, fillMask(d) * (0.4 + 0.5 * eC));
        }
    }

    // Compose shapes.
    col = mix(col, ink, shapeMask);

    // ── z=2 · CALENDAR ARC + TYPEWRITER CAPTION ────────────────────────
    // The reference shows numerals on a circular arc. We render glyphs by
    // walking each number around an arc. Cap = today (typewriter-style).
    vec2 pf = p + mShift;

    // Caption first — small editorial row near bottom-centre. The typewriter
    // is naturally driven by `msg_len` growing as cue.latest streams in:
    // older slots return valid chars, future slots return -1 → blank.
    int total = charCount();
    if (total > 0) {
        // Slow caret pulse hint when msgAge is recent.
        float caret = (msgAge >= 0.0)
            ? smoothstep(0.0, 0.4, sin(msgAge * 6.0) * 0.5 + 0.5)
            : 0.0;

        float ts   = clamp(textSize, 0.5, 2.0);
        float h    = 0.022 * ts;
        float w    = h * (5.0 / 7.0);
        float kern = w * 1.05;

        // Caption baseline — under the bar/arc area, left-aligned to roughly
        // the third of the canvas (matches reference "August 1 2 3 ...").
        vec2 capOrigin = vec2(-0.30, -0.10);
        // Walk visible chars.
        for (int i = 0; i < MAX_CHARS; i++) {
            if (i >= total) break;
            int ch = getChar(i);
            if (ch < 0) continue;
            vec2 cellP = pf - capOrigin - vec2(float(i) * kern, 0.0);
            // Clip to glyph rect.
            if (cellP.x < 0.0 || cellP.x > w) continue;
            if (cellP.y < -h * 0.5 || cellP.y > h * 0.5) continue;
            vec2 g = vec2(cellP.x / w, 0.5 + cellP.y / h);
            float s = sampleChar(ch, g);
            s = smoothstep(0.30, 0.55, s);
            col = mix(col, ink, s);
        }
        // Caret tick after the last glyph while live.
        if (msgAge >= 0.0) {
            vec2 cellP = pf - capOrigin - vec2(float(total) * kern, 0.0);
            float caretD = sdRoundedBar(cellP - vec2(w*0.3, 0.0), vec2(w*0.05, h*0.4), w*0.04);
            col = mix(col, ink, fillMask(caretD) * caret * 0.7);
        }
    }

    // Calendar-style numeric arc (1..30) — small typographic ring on the
    // right side. Reads as an editorial calendar without being literal.
    {
        vec2 c    = vec2(0.32, 0.12);                  // arc centre
        float R   = 0.30;                              // arc radius
        // Numbers placed every ~12° between -10° and +200° (roughly the
        // reference's arc through August). Soft per-energy rotation drift.
        float arcRot = 0.04 * sin(tBase * 0.18) + 0.05 * (eB - 0.5);
        // Pre-compute polar of pf relative to arc centre.
        vec2 q = pf - c;
        float pr = length(q);
        // Cull: only evaluate near the arc to keep glyph walks cheap.
        if (abs(pr - R) < 0.045) {
            float pa = atan(q.y, q.x);  // [-π, π]
            // Walk 30 day glyphs.
            for (int dnum = 1; dnum <= 30; dnum++) {
                float fd = float(dnum);
                float a  = arcRot + radians(-92.0) + fd * radians(11.0);
                // Wrap a within [-π,π].
                float da = pa - a;
                da = atan(sin(da), cos(da));
                // Angular width: bigger for highlighted "today" markers
                // every 7 days (subtle compositional rhythm).
                float aw = 0.045;
                if (abs(da) > aw) continue;
                // Position-local coords (rotate so glyph stands upright).
                float ca2 = cos(a + 1.5708), sa2 = sin(a + 1.5708);
                vec2 lp = mat2(ca2, sa2, -sa2, ca2) * q;
                // Centre on arc.
                lp.y -= R;
                // Glyph height
                float gh = 0.024;
                float gw = gh * (5.0 / 7.0);
                // Pair of digits for >=10.
                int hi = dnum / 10;
                int lo = dnum - hi * 10;
                bool twoDigit = (dnum >= 10);
                float totalW = twoDigit ? gw * 2.05 : gw;
                vec2 gp = lp + vec2(totalW * 0.5, gh * 0.5);
                // First digit.
                if (twoDigit) {
                    vec2 cellP = gp - vec2(0.0, 0.0);
                    if (cellP.x >= 0.0 && cellP.x <= gw &&
                        cellP.y >= 0.0 && cellP.y <= gh) {
                        int ch = digitGlyph(hi);
                        float s = sampleChar(ch, vec2(cellP.x / gw, 1.0 - cellP.y / gh));
                        s = smoothstep(0.30, 0.55, s);
                        col = mix(col, ink, s);
                    }
                }
                // Second (or only) digit.
                {
                    vec2 cellP = gp - vec2(twoDigit ? gw * 1.05 : 0.0, 0.0);
                    if (cellP.x >= 0.0 && cellP.x <= gw &&
                        cellP.y >= 0.0 && cellP.y <= gh) {
                        int ch = digitGlyph(lo);
                        float s = sampleChar(ch, vec2(cellP.x / gw, 1.0 - cellP.y / gh));
                        s = smoothstep(0.30, 0.55, s);
                        col = mix(col, ink, s);
                    }
                }
                // Tiny ring around weekly markers (every 7) — subtle accent
                // bound to bass so musical hits feel like calendar pulses.
                if ((dnum / 7) * 7 == dnum) {
                    float ringD = sdRing(lp - vec2(twoDigit ? gw * 0.5 : gw * 0.25, gh * 0.5),
                                         gh * 0.55, 0.004 + 0.003 * bass);
                    col = mix(col, ink, fillMask(ringD) * (0.65 + 0.35 * bass));
                }
            }
        }
    }

    // ── Paper grain & final breath ─────────────────────────────────────
    if (grain > 0.001) {
        float gr = fbm2(uv * res.y * 0.015) - 0.5;
        col += gr * 0.05 * grain;
    }
    // Bass lift — barely visible, but lifts the field warmth on hits.
    col *= 1.0 + 0.04 * bass;

    // Soft tonemap — very gentle since we already returned LINEAR HDR-ish.
    col = col / (1.0 + 0.25 * max(col - 1.0, 0.0));

    gl_FragColor = vec4(col, 1.0);
}
