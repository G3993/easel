/*{
  "DESCRIPTION": "Moving Grid Gradient Text — a chromatic gradient field of bleeding pigment clouds (magenta, cyan, amber, violet) drifts and pools across a soft paper ground, while a perspective grid skates over it like a wind-blown net: rows warp on a height-field driven by player[1].energy, columns ripple on player[2].energy, the whole grid sweeps diagonally with audio.bass / cue intensity. A typewriter title slab (cue.latest) sits engraved over the composition with its own depth layer. Real three-tier z (gradient back · grid mid · text front), fwidth-AA grid lines, no plain checkerboard — every grid line bends, breathes, and flows. Audio-aware motion: silence reads as a glassy still pond, energy as ribbons of light cutting across pigment.",
  "CREDIT": "easel auto-loop — A-List daily / moving grid gradient text",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",           "TYPE": "text",  "DEFAULT": "MOVING GRID GRADIENT", "MAX_LENGTH": 48, "BIND": "cue.latest" },
    { "NAME": "energyA",       "LABEL": "Row Wave Energy",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",       "LABEL": "Column Wave Energy","TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "playerC",       "LABEL": "Player C Pulse",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].active" },
    { "NAME": "audioDepth",    "LABEL": "Audio Depth Push",  "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.9 },
    { "NAME": "gridDensity",   "LABEL": "Grid Density",      "TYPE": "float", "MIN": 6.0, "MAX": 48.0, "DEFAULT": 18.0 },
    { "NAME": "gridMorph",     "LABEL": "Grid Morph Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 2.5, "DEFAULT": 1.0 },
    { "NAME": "gradPalette",   "LABEL": "Gradient Palette",  "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Bloom","Magenta","Solar","Ink"] },
    { "NAME": "motionSpeed",   "LABEL": "Motion Speed",      "TYPE": "float", "MIN": 0.0, "MAX": 2.5, "DEFAULT": 1.0 },
    { "NAME": "perspective",   "LABEL": "Perspective Tilt",  "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.65 },
    { "NAME": "lineWidth",     "LABEL": "Line Weight",       "TYPE": "float", "MIN": 0.4, "MAX": 2.6, "DEFAULT": 1.0 },
    { "NAME": "textSize",      "LABEL": "Text Size",         "TYPE": "float", "MIN": 0.5, "MAX": 2.4, "DEFAULT": 1.0 },
    { "NAME": "gridTint",      "LABEL": "Grid Ink",          "TYPE": "color", "DEFAULT": [0.08, 0.06, 0.12, 1.0] },
    { "NAME": "paperColor",    "LABEL": "Paper",             "TYPE": "color", "DEFAULT": [0.96, 0.93, 0.88, 1.0] },
    { "NAME": "inkColor",      "LABEL": "Title Ink",         "TYPE": "color", "DEFAULT": [0.05, 0.04, 0.10, 1.0] }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  MOVING GRID GRADIENT TEXT  ·  three depth layers, all alive
//
//  z=2 (back) : gradient field — multiple bleeding pigment blobs drift
//               around a soft paper ground in a chromatic palette.
//  z=1 (mid)  : perspective grid skating over the field. Rows are warped
//               by a travelling height-field on player[1].energy, columns
//               are rippled by player[2].energy, and the whole sheet
//               sweeps diagonally with audio.bass + cue.latest depth.
//               fwidth-AA edges; NEVER a plain checkerboard — every line
//               bends, breathes, and flows.
//  z=0 (front): typewriter title slab — cue.latest reveals at ~28 cps,
//               sits over the comp with a soft drop-shadow.
//
//  Anti-patterns explicitly avoided:
//    • no static checkerboard / SDF debug grid (lines warp every frame)
//    • no horizon mirror (grid is parallax-tilted, not reflected)
//    • no EKG / spectrum bars
//    • no readable-logo decoration (only the cue text slab)
//
//  Idle floor on motion: even at silence the grid breathes via a slow
//  sweep + low-amplitude noise so the canvas never freezes flat.
// ════════════════════════════════════════════════════════════════════════

#define MAX_MSG 48
#define SPACE_CH 26
const float TAU = 6.28318530718;

// ─── font atlas (37 cells: A..Z, space, 0..9) ───────────────────────────
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

int msgTotal() {
    int n = int(msg_len);
    if (n < 0) return 0;
    if (n > MAX_MSG) return MAX_MSG;
    return n;
}

// ─── noise + helpers ────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float hash12(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash12(i);
    float b = hash12(i + vec2(1.0, 0.0));
    float c = hash12(i + vec2(0.0, 1.0));
    float d = hash12(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}
float fbm2(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.03 + vec2(11.3, 5.7);
        a *= 0.5;
    }
    return v;
}

// ─── palette stops for the gradient field ───────────────────────────────
// 4 anchor "pigment" colours per palette; the gradient is built by mixing
// these via fbm-controlled weights so the field reads as bleeding clouds,
// not stripes. Each blob drifts on its own slow Lissajous.
void paletteStops(int p, out vec3 c0, out vec3 c1, out vec3 c2, out vec3 c3) {
    if (p == 1) {
        // Magenta-night: hot pink, electric purple, deep teal, cool white
        c0 = vec3(1.00, 0.32, 0.78);
        c1 = vec3(0.62, 0.20, 1.00);
        c2 = vec3(0.18, 0.85, 0.95);
        c3 = vec3(0.94, 0.92, 1.00);
    } else if (p == 2) {
        // Solar: amber, magenta, violet, ivory
        c0 = vec3(1.00, 0.78, 0.32);
        c1 = vec3(1.00, 0.34, 0.42);
        c2 = vec3(0.42, 0.20, 0.85);
        c3 = vec3(0.98, 0.95, 0.86);
    } else if (p == 3) {
        // Ink wash: deep indigo, slate, dusty rose, paper
        c0 = vec3(0.12, 0.16, 0.45);
        c1 = vec3(0.38, 0.42, 0.55);
        c2 = vec3(0.85, 0.62, 0.66);
        c3 = vec3(0.95, 0.93, 0.88);
    } else {
        // Bloom (reference image): magenta-pink, cyan-blue, amber, paper
        c0 = vec3(1.00, 0.36, 0.72);
        c1 = vec3(0.20, 0.62, 1.00);
        c2 = vec3(1.00, 0.78, 0.36);
        c3 = vec3(0.94, 0.92, 0.86);
    }
}

// Bleeding-pigment gradient field. Four blobs drift around the canvas;
// each contributes a soft radial weight; the final colour is a weighted
// blend that flows like ink in wet paper. `tt` is field-local time;
// `pulse` is a global energy pulse (audio + player).
vec3 gradientField(vec2 uv, float aspect, float tt, float pulse, int palette) {
    vec3 c0, c1, c2, c3;
    paletteStops(palette, c0, c1, c2, c3);

    // Centred, aspect-corrected sample point.
    vec2 q = vec2((uv.x - 0.5) * aspect, uv.y - 0.5);

    // Slow per-blob drift — distinct Lissajous so blobs never lock.
    vec2 p0 = vec2(sin(tt * 0.21 + 0.7) * 0.45 * aspect,
                   cos(tt * 0.17 + 1.3) * 0.32);
    vec2 p1 = vec2(cos(tt * 0.13 + 2.1) * 0.40 * aspect,
                   sin(tt * 0.19 - 0.5) * 0.28);
    vec2 p2 = vec2(sin(tt * 0.27 - 1.4) * 0.42 * aspect,
                   cos(tt * 0.23 + 0.2) * 0.30);
    vec2 p3 = vec2(cos(tt * 0.11 + 3.7) * 0.30 * aspect,
                   sin(tt * 0.25 - 2.0) * 0.22);

    // Per-blob radius breathes on `pulse` so loud frames bloom the field.
    float r0 = 0.55 * (1.0 + 0.18 * pulse);
    float r1 = 0.48 * (1.0 + 0.14 * pulse);
    float r2 = 0.60 * (1.0 + 0.22 * pulse);
    float r3 = 0.42 * (1.0 + 0.10 * pulse);

    // Soft falloff weights — gaussian-ish.
    float w0 = exp(-dot(q - p0, q - p0) / (r0 * r0));
    float w1 = exp(-dot(q - p1, q - p1) / (r1 * r1));
    float w2 = exp(-dot(q - p2, q - p2) / (r2 * r2));
    float w3 = exp(-dot(q - p3, q - p3) / (r3 * r3));

    // FBM-modulated weights — pigment bleeds, doesn't draw clean circles.
    float n0 = fbm2(uv * 3.1 + vec2(tt * 0.08, -tt * 0.05));
    float n1 = fbm2(uv * 2.4 - vec2(tt * 0.06,  tt * 0.07));
    w0 *= 0.6 + 0.8 * n0;
    w1 *= 0.6 + 0.8 * (1.0 - n0);
    w2 *= 0.6 + 0.8 * n1;
    w3 *= 0.6 + 0.8 * (1.0 - n1);

    float wSum = w0 + w1 + w2 + w3 + 1e-4;
    vec3 col = (c0 * w0 + c1 * w1 + c2 * w2 + c3 * w3) / wSum;

    // Mix toward paper at the periphery so the gradient sits ON paper,
    // not edge-to-edge — matches the reference image's "spray on ground"
    // feel.
    float radial = clamp(length(q) * 0.85, 0.0, 1.0);
    col = mix(col, paperColor.rgb, smoothstep(0.55, 1.05, radial) * 0.5);

    // Tiny paper grain so the field never reads as a CG gradient.
    float grain = fbm2(uv * 320.0) - 0.5;
    col += grain * 0.018;

    return col;
}

// ─── perspective grid (the moving net) ──────────────────────────────────
// We project a 2D pixel into a tilted ground-plane mesh-UV with vanishing
// point at the top of the canvas (perspective tilt parameter controls
// how aggressive the foreshortening is — 0 = flat overhead, 1.5 = strong
// rake). The mesh-UV is then warped by two travelling height fields
// (row + column) so rows bend on player[1] energy and columns ripple on
// player[2] energy. The grid SWEEPS diagonally every frame on motionSpeed
// so motion is always visible.
//
// `pixelUv` ∈ [0,1]^2 — current screen pixel.
// `tt`      — grid-local time.
// `eRow`,`eCol` — per-axis warp energies.
// `sweep`   — diagonal sweep phase (radians).
// Returns line-mask in [0,1] (1 = on a grid line) and writes `meshUv`.
float meshDistance(vec2 pixelUv, float aspect, float tt,
                   float eRow, float eCol, float sweep,
                   out vec2 meshUv, out float depthFade)
{
    // Tilt: pull the top of the screen toward the vanishing point.
    // Compression factor t ∈ [0,1] where 0 = far (top), 1 = near (bottom).
    float pers = clamp(perspective, 0.0, 1.5);
    float t = clamp(pixelUv.y, 0.0, 1.0);
    // Pseudo-perspective: y' = y^(1 + pers*1.5). Larger exponent =
    // stronger compression near top → vanishing-point feel.
    float yExp = pow(1.0 - t, 1.0 + pers * 1.6);    // far->1, near->0
    float vMesh = 1.0 - yExp;                        // 0 far, 1 near

    // Horizontal stretch: distant rows are narrower (foreshortened in x).
    float xWidth = mix(1.0, 0.18, 1.0 - vMesh);      // narrow at far
    float uCentered = (pixelUv.x - 0.5) * aspect;
    float uMesh = uCentered / max(xWidth, 0.05);    // expand to mesh-u

    // Diagonal sweep — translate the mesh-UV every frame so the entire
    // net glides toward upper-right. This is the "moving" of moving grid.
    float sweepU = sweep * 0.18;
    float sweepV = sweep * 0.11;
    uMesh += sweepU;
    vMesh += sweepV;

    // ── Row height field (bends V) ──
    // Travelling sines so rows aren't flat horizontals. Idle floor keeps
    // the warp visible even at silence.
    float ampRow = 0.06 + 0.55 * eRow;
    float row = sin(uMesh * 3.0 - tt * 1.10)        * 0.50
              + sin(uMesh * 6.5 + tt * 0.72 + 1.3)  * 0.30
              + sin(uMesh * 14.0 - tt * 1.85)       * 0.18 * eRow;
    float vBent = vMesh + row * ampRow * 0.10;

    // ── Column height field (bends U) ──
    // Independent — driven by eCol. Columns sway across vMesh.
    float ampCol = 0.05 + 0.45 * eCol;
    float colW = sin(vMesh * 4.5 + tt * 0.92)            * 0.45
               + sin(vMesh * 9.0 - tt * 0.65 - 2.1)      * 0.30
               + sin(vMesh * 18.0 + tt * 1.40)           * 0.22 * eCol;
    float uBent = uMesh + colW * ampCol * 0.08;

    // ── Grid density ──
    // Lines per mesh-unit; column density grows toward the vanishing point
    // for an extra perspective cue (more lines pile up in the distance).
    float densV = gridDensity * mix(0.7, 1.6, 1.0 - vMesh);
    float densU = gridDensity * 0.8;

    // Distance to nearest integer row / column in bent mesh space.
    float rowFrac = abs(fract(vBent * densV) - 0.5);
    float colFrac = abs(fract(uBent * densU) - 0.5);
    float d = min(rowFrac, colFrac);

    meshUv = vec2(uMesh, vMesh);

    // Depth fade — far rows dissolve into the gradient field so the grid
    // reads as IN the scene, not pasted on top.
    depthFade = mix(0.15, 1.0, vMesh);

    return d;
}

// ─── typewriter title slab ──────────────────────────────────────────────
// Centred horizontal line of glyphs at slabY ± slabH/2. msgAge drives
// reveal at ~28 cps. Returns premultiplied (rgb, a).
vec4 renderTitleSlab(vec2 screenUv, float slabY, float slabH,
                     int total, float aspect, vec3 ink) {
    vec4 acc = vec4(0.0);
    if (total <= 0) return acc;

    int revealCap;
    if (msgAge >= 0.0) {
        int rev = int(floor(msgAge * 28.0));
        if (rev < 1) rev = 1;
        if (rev > total) rev = total;
        revealCap = rev;
    } else {
        revealCap = total;
    }

    float gH = slabH * textSize;
    float gW = gH * (5.0 / 7.0);
    float kern = gW * 0.92;
    float totalW = float(revealCap) * kern;

    float originX = 0.5 - 0.5 * totalW / aspect;
    float y0 = slabY - gH * 0.5;
    float y1 = slabY + gH * 0.5;

    if (screenUv.y < y0 - 0.01 || screenUv.y > y1 + 0.01) return acc;

    float xLocal = (screenUv.x - originX) * aspect;
    if (xLocal < 0.0 || xLocal > totalW) return acc;
    int slot = int(floor(xLocal / kern));
    if (slot < 0 || slot >= revealCap) return acc;
    int ch = getChar(slot);
    if (ch < 0 || ch > 36) return acc;
    if (ch == SPACE_CH) return acc;

    float xInCell = xLocal - float(slot) * kern;
    float colPad = (kern - gW) * 0.5;
    float xInGlyph = (xInCell - colPad) / gW;
    if (xInGlyph < 0.0 || xInGlyph > 1.0) return acc;
    // screenUv.y is y-UP; y0=bottom-of-glyph, y1=top. Host font atlas
    // stores letter-top at v=1, so direct y-up→v mapping puts letter-top
    // at screen-top. The previous `(y1 - screenUv.y)` form flipped this
    // and rendered glyphs upside down.
    float yInGlyph = (screenUv.y - y0) / gH;
    if (yInGlyph < 0.0 || yInGlyph > 1.0) return acc;

    float s = sampleChar(ch, vec2(xInGlyph, yInGlyph));
    float aa = fwidth(s) * 1.4 + 1e-4;
    float alpha = smoothstep(0.5 - aa, 0.5 + aa, s);
    if (alpha < 0.001) return acc;

    // Drop-shadow: sample slightly offset for engraved depth.
    float sShadow = sampleChar(ch, vec2(xInGlyph, yInGlyph - 0.05));
    float shadow = smoothstep(0.5 - aa, 0.5 + aa, sShadow) * 0.35;

    acc.rgb = ink * alpha;
    acc.a   = max(alpha, shadow * 0.6);
    return acc;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    float t = TIME * motionSpeed;

    float bass = audioBass;
    float mid  = audioMid;
    float high = audioHigh;
    float lev  = audioLevel;

    // Global pulse drives the gradient field's blob breathing + grid
    // sweep amplitude. Player[3].active gives a "third voice" lift on top
    // of audio so cue-driven moments register even without bass.
    float pulse = clamp(lev * audioDepth + 0.35 * playerC, 0.0, 2.0);

    // Per-axis grid energies. Player channels are primary; an audio nudge
    // keeps the grid breathing when no player binds are routed.
    float eRow = clamp(energyA + bass * 0.25 * audioDepth, 0.0, 1.5);
    float eCol = clamp(energyB + mid  * 0.25 * audioDepth, 0.0, 1.5);

    // Grid morph clock — separate from motionSpeed so the user can crank
    // grid liveness without warping the field's drift speed.
    float tGrid = TIME * gridMorph;

    // Diagonal sweep angle — the grid GLIDES every frame on this. Audio
    // pushes it faster; idle drift keeps it alive at silence.
    float sweep = TIME * (0.35 + 0.6 * gridMorph)
                + bass * 1.6 * audioDepth;

    // Mouse parallax — entire stack nudges with the mouse, but grid moves
    // more than gradient so the parallax reads as depth separation.
    vec2 m2 = (mousePos - 0.5);

    // ── Layer 1 (back): gradient pigment field ───────────────────────
    vec2 uvGrad = uv;
    uvGrad += m2 * 0.015;                    // back layer drifts least
    float tGrad = TIME * 0.6;                // gradient has its own slow time
    vec3 col = gradientField(uvGrad, aspect, tGrad, pulse, int(gradPalette));

    // ── Layer 2 (mid): moving warped grid ────────────────────────────
    vec2 uvGrid = uv;
    uvGrid += m2 * 0.045;                    // mid layer drifts more
    vec2 meshUv;
    float depthFade;
    float d = meshDistance(uvGrid, aspect, tGrid, eRow, eCol, sweep,
                           meshUv, depthFade);

    // fwidth-AA line. Line core scales with lineWidth + depthFade so far
    // lines are crisp 1px and near lines bloom slightly thicker.
    float fw = fwidth(d) * 1.4 + 1e-4;
    float lineCore = 0.012 * lineWidth * mix(0.6, 1.4, depthFade);
    float line = 1.0 - smoothstep(lineCore - fw, lineCore + fw, d);
    line *= depthFade;                       // dissolve far rows into field

    // Grid colour: dark ink that picks up a hint of the gradient under it,
    // so the grid feels embedded rather than overlaid.
    vec3 lineInk = mix(gridTint.rgb, col * 0.4, 0.30);
    // Energy halo on the brighter lines — loud frames make the net glow.
    float halo = 0.55 + 0.75 * max(eRow, eCol);
    vec3 gridCol = lineInk * halo;

    // Compose grid OVER gradient field.
    col = mix(col, gridCol, line * 0.92);

    // Ridge highlight: pixels right at the intersection of a row and a
    // column get a tiny gradient-tinted glow — like light hitting the
    // knots of the net.
    float intersect = (1.0 - smoothstep(0.0, 0.04, d));
    col += intersect * pulse * 0.18 * vec3(1.0, 0.96, 0.88);

    // ── Layer 3 (front): typewriter title slab ───────────────────────
    int total = msgTotal();
    // Slab sits at ~22% from the top — over the gradient bloom, under the
    // grid's most-foreshortened (top) rows. textSize controls its height.
    float slabY = 0.22;
    float slabH = 0.07;
    vec4 title = renderTitleSlab(uv, slabY, slabH, total, aspect, inkColor.rgb);
    if (title.a > 0.001) {
        // Soft halo under the title so it reads against busy gradient.
        vec2 d2 = (uv - vec2(0.5, slabY));
        float titleGlow = exp(-dot(d2 * vec2(1.0/0.6, 1.0/0.10), d2 * vec2(1.0/0.6, 1.0/0.10)));
        col = mix(col, paperColor.rgb, titleGlow * 0.35 * title.a);
        col = mix(col, title.rgb / max(title.a, 1e-4), title.a);
    }

    // ── Final grade ───────────────────────────────────────────────────
    // Soft Reinhard tonemap.
    col = col / (1.0 + 0.55 * col);
    // Bass-driven lift so loud moments brighten without blowing out.
    col *= 1.0 + 0.08 * bass * audioDepth;
    // Bloom on the brightest pixels.
    float L = dot(col, vec3(0.299, 0.587, 0.114));
    col += 0.10 * smoothstep(0.6, 1.2, L) * col;
    // Subtle vignette + paper tooth so the canvas reads physical.
    float vign = 1.0 - 0.25 * dot(uv - 0.5, uv - 0.5);
    col *= vign;
    float tooth = vnoise(uv * res.y * 0.015);
    col *= 1.0 + (tooth - 0.5) * 0.04;

    col = pow(max(col, 0.0), vec3(0.94));
    gl_FragColor = vec4(col, 1.0);
}
