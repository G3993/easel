/*{
  "DESCRIPTION": "Shape Grid Circular — a contact-sheet of circle treatments. Each cell of an R×C grid holds a distinct circular SDF specimen — concentric rings, halftone dots, dot-cluster mandalas, gradient lenses, eclipse crescents, ring-arcs, glyph-mark vesicas, gear-tooth rotors, ripple corona, scan-line discs — so the grid reads as a typography research plate, not a chequerboard. Three player channels each own a sub-region of the grid (Back / Mid / Front depth bands): their cell variants brighten, swell, and parallax forward as their player speaks. Bass thickens stroke weight on the whole sheet; mid pumps the orbital animations; high crisps the highlights. The live cue line types out across the bottom in slab caption form, glued under the active player's strongest cell — never the central visual. Real depth: three parallax planes, per-cell pop on energy spikes, gentle camera breath. Returns LINEAR HDR.",
  "CREDIT": "ShaderClaw — A-List drop · shape_grid_circular_text",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "LABEL": "Caption", "TYPE": "text", "DEFAULT": "CIRCULAR FORMS · RESEARCH PLATE 03", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "energyA", "LABEL": "Player 1 (Back)",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB", "LABEL": "Player 2 (Mid)",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC", "LABEL": "Player 3 (Front)", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "activeA", "LABEL": "Player 1 Active",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].active" },
    { "NAME": "activeB", "LABEL": "Player 2 Active",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].active" },

    { "NAME": "bassDrive", "LABEL": "Bass (Stroke)",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.bass" },
    { "NAME": "midDrive",  "LABEL": "Mid (Orbits)",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.mid" },
    { "NAME": "highDrive", "LABEL": "High (Crisp)",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.high" },

    { "NAME": "rows",        "LABEL": "Rows",            "TYPE": "long",  "DEFAULT": 5, "VALUES": [3,4,5,6,7], "LABELS": ["3","4","5","6","7"] },
    { "NAME": "cols",        "LABEL": "Cols",            "TYPE": "long",  "DEFAULT": 4, "VALUES": [3,4,5,6,7], "LABELS": ["3","4","5","6","7"] },
    { "NAME": "paletteMode", "LABEL": "Palette",         "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Paper","Editorial","Acid","Mono"] },
    { "NAME": "variantMix",  "LABEL": "Variant Mix",     "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",    "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 1.6 },
    { "NAME": "audioDepth",  "LABEL": "Audio Depth",     "TYPE": "float", "DEFAULT": 0.75, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "parallax",    "LABEL": "Parallax",        "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "popAmount",   "LABEL": "Pop (per cell)",  "TYPE": "float", "DEFAULT": 0.65, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "labelScale",  "LABEL": "Caption Scale",   "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.5, "MAX": 1.8 },
    { "NAME": "kerning",     "LABEL": "Kerning",         "TYPE": "float", "DEFAULT": 0.95, "MIN": 0.55, "MAX": 1.4 },
    { "NAME": "paperColor",  "LABEL": "Paper",           "TYPE": "color", "DEFAULT": [0.945, 0.935, 0.910, 1.0] },
    { "NAME": "inkColor",    "LABEL": "Ink",             "TYPE": "color", "DEFAULT": [0.050, 0.050, 0.070, 1.0] }
  ]
}*/

// ═══════════════════════════════════════════════════════════════════════════
//  SHAPE GRID CIRCULAR · TEXT
//  R×C contact-sheet of distinct circular SDF treatments. Three players each
//  own a depth band (back/mid/front); their cells parallax forward and pop
//  on energy. Caption lives in a slab caption row glued under the active
//  player's hottest cell — never the central visual. Premium fwidth-AA.
// ═══════════════════════════════════════════════════════════════════════════

#define MAX_CELLS    49      // 7×7 ceiling
#define MAX_WALK     48
#define SPACE_CH     26
#define VARIANT_N    10
const float TAU = 6.28318530718;

// ─── Font atlas (text caption row) ────────────────────────────────────────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}
int getChar(int slot) {
    if (slot ==  0) return int(msg_0);  if (slot ==  1) return int(msg_1);
    if (slot ==  2) return int(msg_2);  if (slot ==  3) return int(msg_3);
    if (slot ==  4) return int(msg_4);  if (slot ==  5) return int(msg_5);
    if (slot ==  6) return int(msg_6);  if (slot ==  7) return int(msg_7);
    if (slot ==  8) return int(msg_8);  if (slot ==  9) return int(msg_9);
    if (slot == 10) return int(msg_10); if (slot == 11) return int(msg_11);
    if (slot == 12) return int(msg_12); if (slot == 13) return int(msg_13);
    if (slot == 14) return int(msg_14); if (slot == 15) return int(msg_15);
    if (slot == 16) return int(msg_16); if (slot == 17) return int(msg_17);
    if (slot == 18) return int(msg_18); if (slot == 19) return int(msg_19);
    if (slot == 20) return int(msg_20); if (slot == 21) return int(msg_21);
    if (slot == 22) return int(msg_22); if (slot == 23) return int(msg_23);
    if (slot == 24) return int(msg_24); if (slot == 25) return int(msg_25);
    if (slot == 26) return int(msg_26); if (slot == 27) return int(msg_27);
    if (slot == 28) return int(msg_28); if (slot == 29) return int(msg_29);
    if (slot == 30) return int(msg_30); if (slot == 31) return int(msg_31);
    if (slot == 32) return int(msg_32); if (slot == 33) return int(msg_33);
    if (slot == 34) return int(msg_34); if (slot == 35) return int(msg_35);
    if (slot == 36) return int(msg_36); if (slot == 37) return int(msg_37);
    if (slot == 38) return int(msg_38); if (slot == 39) return int(msg_39);
    if (slot == 40) return int(msg_40); if (slot == 41) return int(msg_41);
    if (slot == 42) return int(msg_42); if (slot == 43) return int(msg_43);
    if (slot == 44) return int(msg_44); if (slot == 45) return int(msg_45);
    if (slot == 46) return int(msg_46); if (slot == 47) return int(msg_47);
    return -1;
}
int charCount() { int n = int(msg_len); if (n <= 0) return 0; if (n > 48) return 48; return n; }

// ─── Hash / noise ─────────────────────────────────────────────────────────
float h11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float h12(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
vec2  h21(float n) { return vec2(h11(n), h11(n + 17.31)); }
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0 - 2.0*f);
    float a = h12(i), b = h12(i + vec2(1.0, 0.0));
    float c = h12(i + vec2(0.0, 1.0)), d = h12(i + vec2(1.0, 1.0));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}
float fbm2(vec2 p) {
    float s = 0.0, a = 0.55;
    for (int i = 0; i < 3; i++) { s += a*vnoise(p); p = p*2.03 + 7.31; a *= 0.5; }
    return s;
}

// ─── Palette ──────────────────────────────────────────────────────────────
vec3 palettePick(int mode, int idx, float t) {
    // Six anchor swatches per mode; idx is wrapped.
    int i = int(mod(float(idx), 6.0));
    vec3 c;
    if (mode == 1) {           // Editorial — punchy, magazine
        if (i == 0) c = vec3(0.97, 0.36, 0.28);
        else if (i == 1) c = vec3(0.10, 0.42, 0.92);
        else if (i == 2) c = vec3(0.97, 0.80, 0.10);
        else if (i == 3) c = vec3(0.18, 0.74, 0.46);
        else if (i == 4) c = vec3(0.62, 0.30, 0.86);
        else c = vec3(0.96, 0.94, 0.90);
    } else if (mode == 2) {    // Acid — high-key
        if (i == 0) c = vec3(0.99, 0.95, 0.10);
        else if (i == 1) c = vec3(0.20, 1.00, 0.60);
        else if (i == 2) c = vec3(1.00, 0.30, 0.70);
        else if (i == 3) c = vec3(0.10, 0.85, 1.00);
        else if (i == 4) c = vec3(0.85, 0.10, 0.95);
        else c = vec3(1.00, 0.55, 0.10);
    } else if (mode == 3) {    // Mono — paper + ink only
        float k = float(i) / 5.0;
        c = mix(vec3(0.05, 0.05, 0.07), vec3(0.93, 0.92, 0.88), k);
    } else {                   // Paper — warm editorial-ish (matches reference)
        if (i == 0) c = vec3(0.92, 0.34, 0.30);
        else if (i == 1) c = vec3(0.13, 0.36, 0.80);
        else if (i == 2) c = vec3(0.96, 0.74, 0.20);
        else if (i == 3) c = vec3(0.32, 0.56, 0.42);
        else if (i == 4) c = vec3(0.92, 0.46, 0.16);
        else c = vec3(0.30, 0.28, 0.32);
    }
    // tiny slow hue breath so colour isn't static
    float b = 0.04 * sin(t * 0.7 + float(idx) * 1.3);
    return clamp(c + vec3(b * 0.6, b * 0.2, -b * 0.5), 0.0, 1.0);
}

// ─── Cell variants (10 distinct treatments) ───────────────────────────────
// uv: local square coords [-1,1]; r = length(uv); a = atan; t = local time;
// stroke: scaled stroke weight; seed: per-cell hash; pump: orbital phase (0..1);
// fw: per-cell fwidth scaler for AA.
// Returns inkAmount (0..1) — gets composited against the cell's accent color.

float aa(float d, float fw) { return 1.0 - smoothstep(-fw, fw, d); }
float ring(float r, float r0, float w, float fw) {
    float d = abs(r - r0) - w;
    return aa(d, fw);
}
float disk(float r, float r0, float fw) { return aa(r - r0, fw); }

// 0 concentric rings
float v_concentric(vec2 uv, float t, float stroke, float seed, float pump, float fw) {
    float r = length(uv);
    float rings = 4.0 + floor(seed * 4.0);
    float s = 0.0;
    for (int i = 1; i <= 7; i++) {
        if (float(i) > rings) break;
        float r0 = float(i) / (rings + 0.5) * 0.82;
        float w  = stroke * (0.012 + 0.004 * sin(t*1.1 + seed*9.0 + float(i)));
        s = max(s, ring(r, r0, w, fw));
    }
    return s * aa(r - 0.92, fw);
}
// 1 halftone dots
float v_halftone(vec2 uv, float t, float stroke, float seed, float pump, float fw) {
    if (length(uv) > 0.92) return 0.0;
    float n = 6.0 + floor(seed * 4.0);
    vec2 g = uv * n;
    vec2 cell = floor(g) + 0.5;
    vec2 lp = g - cell;
    float r = length(uv);
    float rad = (0.42 - 0.34 * r) * (0.5 + 0.7 * stroke);
    float d = length(lp) - rad;
    return aa(d, fw * n) * aa(r - 0.92, fw);
}
// 2 dot mandala (orbits)
float v_mandala(vec2 uv, float t, float stroke, float seed, float pump, float fw) {
    float r = length(uv); float a = atan(uv.y, uv.x);
    float s = ring(r, 0.78, 0.012 * stroke, fw);
    int N = 12;
    float A = TAU / float(N);
    float ang = a + t * 0.35 + pump * 0.6 + seed * 6.28;
    float idx = floor(ang / A + 0.5);
    float ca = idx * A;
    vec2 c1 = vec2(cos(ca), sin(ca)) * 0.55;
    vec2 c2 = vec2(cos(ca + A*0.5), sin(ca + A*0.5)) * 0.30;
    s = max(s, aa(length(uv - c1) - (0.055 + 0.02 * stroke), fw));
    s = max(s, aa(length(uv - c2) - (0.030 + 0.012 * stroke), fw));
    s = max(s, aa(length(uv) - (0.06 + 0.02 * stroke), fw));
    return s * aa(r - 0.95, fw);
}
// 3 gradient lens (radial gradient inside a disc)
float v_lens(vec2 uv, float t, float stroke, float seed, float pump, float fw) {
    float r = length(uv);
    float mask = aa(r - 0.88, fw);
    float g = smoothstep(0.85, 0.0, r);
    // bevel rim
    float rim = ring(r, 0.85, 0.018 * stroke, fw);
    return clamp(g * mask + rim, 0.0, 1.0);
}
// 4 eclipse crescent
float v_eclipse(vec2 uv, float t, float stroke, float seed, float pump, float fw) {
    float a = aa(length(uv) - 0.85, fw);
    vec2 off = vec2(cos(seed*6.28 + t*0.2), sin(seed*6.28 + t*0.2)) * (0.18 + 0.10 * pump);
    float b = aa(length(uv - off) - 0.78, fw);
    return clamp(a - b, 0.0, 1.0);
}
// 5 ring arc segments (clock face)
float v_arcs(vec2 uv, float t, float stroke, float seed, float pump, float fw) {
    float r = length(uv); float a = atan(uv.y, uv.x);
    float s = 0.0;
    float N = 8.0;
    float A = TAU / N;
    float wedge = step(0.5, fract(a / A + t*0.10 + seed*3.0 + pump));
    float w = 0.035 * stroke + 0.018;
    float band = ring(r, 0.70, w, fw);
    s = max(s, band * wedge);
    s = max(s, ring(r, 0.32, 0.012 * stroke, fw));
    return s * aa(r - 0.92, fw);
}
// 6 vesica / two-circle overlap
float v_vesica(vec2 uv, float t, float stroke, float seed, float pump, float fw) {
    float off = 0.22 + 0.04 * sin(t*0.7 + seed*6.0);
    float a = aa(length(uv - vec2(-off, 0.0)) - 0.55, fw);
    float b = aa(length(uv - vec2( off, 0.0)) - 0.55, fw);
    float overlap = a * b;                // inner vesica
    float outline = max(ring(length(uv - vec2(-off, 0.0)), 0.55, 0.012*stroke, fw),
                        ring(length(uv - vec2( off, 0.0)), 0.55, 0.012*stroke, fw));
    return clamp(overlap * 0.65 + outline, 0.0, 1.0);
}
// 7 gear rotor
float v_gear(vec2 uv, float t, float stroke, float seed, float pump, float fw) {
    float r = length(uv); float a = atan(uv.y, uv.x);
    float teeth = floor(8.0 + seed * 8.0);
    float bump  = 0.05 + 0.025 * stroke;
    float wave  = bump * cos(a * teeth + t * 0.6 + pump * 6.28);
    float body  = aa(r - (0.72 + wave), fw);
    float hole  = aa(r - 0.22, fw);
    return clamp(body - hole, 0.0, 1.0);
}
// 8 ripple corona
float v_corona(vec2 uv, float t, float stroke, float seed, float pump, float fw) {
    float r = length(uv);
    float ph = t * 1.4 + seed * 6.0 + pump * 3.14;
    float s = 0.0;
    for (int i = 0; i < 4; i++) {
        float k = float(i) / 4.0;
        float r0 = fract(k + ph * 0.18);     // travelling rings
        r0 = mix(0.18, 0.86, r0);
        float w = (0.010 + 0.004 * stroke) * (1.0 - smoothstep(0.55, 0.95, r0));
        s = max(s, ring(r, r0, w, fw));
    }
    s = max(s, aa(r - 0.08, fw));            // bright core
    return s * aa(r - 0.92, fw);
}
// 9 scan-line disc (parallel chords clipped to circle)
float v_scan(vec2 uv, float t, float stroke, float seed, float pump, float fw) {
    float r = length(uv);
    float mask = aa(r - 0.86, fw);
    float n = 10.0 + floor(seed * 6.0);
    float ang = seed * 3.1416 + t * 0.20;
    vec2 R = vec2(cos(ang), sin(ang));
    float u = dot(uv, R);                    // perpendicular projection
    float phase = u * n + t * 0.6 + pump * 2.0;
    float line = abs(fract(phase) - 0.5) * 2.0;
    float lw = 0.45 - 0.20 * stroke;
    float l = smoothstep(lw, lw - 0.08, line);
    return l * mask;
}

float cellVariant(int variant, vec2 uv, float t, float stroke, float seed, float pump, float fw) {
    if      (variant == 0) return v_concentric(uv, t, stroke, seed, pump, fw);
    else if (variant == 1) return v_halftone  (uv, t, stroke, seed, pump, fw);
    else if (variant == 2) return v_mandala   (uv, t, stroke, seed, pump, fw);
    else if (variant == 3) return v_lens      (uv, t, stroke, seed, pump, fw);
    else if (variant == 4) return v_eclipse   (uv, t, stroke, seed, pump, fw);
    else if (variant == 5) return v_arcs      (uv, t, stroke, seed, pump, fw);
    else if (variant == 6) return v_vesica    (uv, t, stroke, seed, pump, fw);
    else if (variant == 7) return v_gear      (uv, t, stroke, seed, pump, fw);
    else if (variant == 8) return v_corona    (uv, t, stroke, seed, pump, fw);
    else                   return v_scan      (uv, t, stroke, seed, pump, fw);
}

// Returns the depth-band index for a (col,row) cell. 0=back, 1=mid, 2=front.
// Stripes the grid into 3 horizontal bands so each player owns a row-region.
int cellBand(int cy, int rowsN) {
    float t = float(cy) / max(float(rowsN - 1), 1.0);
    if (t < 0.34) return 0;
    if (t < 0.67) return 1;
    return 2;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 frag = gl_FragCoord.xy;
    vec2 uv01 = frag / res;
    float aspect = res.x / res.y;

    int rowsN = clamp(int(rows), 3, 7);
    int colsN = clamp(int(cols), 3, 7);
    int mode  = clamp(int(paletteMode), 0, 3);

    float t   = TIME * motionSpeed;
    float eA  = clamp(energyA, 0.0, 1.0);
    float eB  = clamp(energyB, 0.0, 1.0);
    float eC  = clamp(energyC, 0.0, 1.0);
    float aAv = clamp(activeA, 0.0, 1.0);
    float aBv = clamp(activeB, 0.0, 1.0);
    float bD  = clamp(bassDrive, 0.0, 1.0);
    float mD  = clamp(midDrive,  0.0, 1.0);
    float hD  = clamp(highDrive, 0.0, 1.0);

    // ── Background paper (so the grid lives on a sheet, not a clearcolor) ──
    vec3 paper = paperColor.rgb;
    vec2 c01 = uv01 - 0.5;
    float vign = 1.0 - 0.18 * dot(c01, c01);
    float fibre = fbm2(uv01 * vec2(aspect, 1.0) * 5.0);
    paper *= mix(0.96, 1.04, fibre) * vign;
    vec3 col = paper;

    // ── Subtle perspective tilt — camera breath + parallax per band ──
    // We render each band with its own shifted/scaled "camera" to a separate
    // accumulator, then composite back→front. This gives a real 3-plane
    // parallax stack.
    float yaw   = sin(t * 0.13) * 0.012 + (mousePos.x - 0.5) * 0.018;
    float pitch = sin(t * 0.09) * 0.010 + (mousePos.y - 0.5) * 0.014;

    // Caption row sits at the bottom — reserve 14% of canvas for it.
    const float CAPTION_FRAC = 0.14;
    float gridTop    = 0.95;
    float gridBot    = CAPTION_FRAC + 0.04;
    float gridH      = gridTop - gridBot;
    float gridLeftN  = 0.045 * aspect;      // normalised inset (aspect units)
    float gridRightN = 0.045 * aspect;

    // Map fragCoord into grid space (aspect-corrected).
    // Center origin to play nicely with parallax shifts.
    vec2 sp;
    sp.x = (uv01.x - 0.5) * aspect;
    sp.y = (uv01.y - 0.5);

    // Per-band visit. We loop back→front so later bands occlude earlier ones
    // via direct over-compositing.
    for (int band = 0; band < 3; band++) {
        // Player ownership per band.
        float energy = (band == 0) ? eA : (band == 1) ? eB : eC;
        float activ  = (band == 0) ? aAv : (band == 1) ? aBv : 1.0;  // front always on
        float depth  = -0.30 + float(band) * 0.30;                   // -0.3 / 0.0 / +0.3

        // Parallax: each band offsets independently with camera + audio shove.
        float pxAmt = parallax * (0.10 + 0.10 * float(band));
        vec2  par   = vec2(yaw, pitch) * pxAmt * 4.0;
        // Audio shove: bands drift on their own player's energy * audioDepth.
        par += vec2(sin(t * (0.4 + 0.2*float(band)) + float(band)),
                    cos(t * (0.35 + 0.15*float(band)) + float(band)*1.7))
               * 0.018 * audioDepth * energy;

        // Slight per-band scale so depth reads as size variance.
        float depthScale = 1.0 + depth * 0.06 * parallax;

        // Compute band's grid frame (aspect-corrected, centered).
        vec2 gp = (sp - par) / depthScale;

        // Grid extents (in same aspect-corrected space).
        float gridLeft  = -0.5 * aspect + gridLeftN;
        float gridRight =  0.5 * aspect - gridRightN;
        // Vertical centred around grid mid.
        float gridMidY  = (gridTop + gridBot) * 0.5 - 0.5;     // in [-0.5,0.5]
        float gw = gridRight - gridLeft;
        float gh = gridH;
        float gridBotY  = gridMidY - gh * 0.5;
        float gridTopY  = gridMidY + gh * 0.5;

        // Inside grid bounds? (For this band only.)
        if (gp.x < gridLeft || gp.x > gridRight) continue;
        if (gp.y < gridBotY || gp.y > gridTopY) continue;

        float cellW = gw / float(colsN);
        float cellH = gh / float(rowsN);

        int cx = int(floor((gp.x - gridLeft) / cellW));
        int cy = int(floor((gp.y - gridBotY) / cellH));
        if (cx < 0 || cx >= colsN || cy < 0 || cy >= rowsN) continue;

        // Only the band that OWNS this cell row paints here.
        int ownedBand = cellBand(cy, rowsN);
        if (ownedBand != band) continue;

        // Cell local coords [-1,1] inside the cell (with small inset gutter).
        float gutter = 0.08;
        vec2 cellCenter = vec2(gridLeft + (float(cx) + 0.5) * cellW,
                               gridBotY + (float(cy) + 0.5) * cellH);
        vec2 cellLocal = (gp - cellCenter) / vec2(cellW, cellH) * 2.0;
        // Use circular fit — divide by min, but inset by gutter.
        float fitR = (1.0 - gutter);
        vec2 uv = cellLocal / fitR;

        // Deterministic per-cell seed + variant pick.
        float seed = h12(vec2(float(cx) + 0.31, float(cy) + 0.73));
        // Variant choice: weighted by variantMix; at mix=0 every cell shows
        // 'concentric' (base reference); at mix=1.5, full diversity + bias
        // toward seldom-used variants.
        float variantF = mix(0.0, float(VARIANT_N), clamp(variantMix, 0.0, 1.5) / 1.5);
        int variant = int(mod(floor(seed * variantF + float(cx)*0.7 + float(cy)*1.3), float(VARIANT_N)));

        // Per-cell pump (orbital phase) + audio kick.
        float pump = fract(t * 0.18 + seed * 1.3 + float(band) * 0.27);
        // Pop on energy: cell scales up and brightens when its player is hot.
        // Per-cell threshold so not every cell pops at once.
        float popThresh = 0.25 + 0.55 * seed;
        float popAmt = smoothstep(popThresh, 1.0, energy * audioDepth) * popAmount;
        // Uniform scale toward center on pop.
        float popScale = 1.0 + 0.16 * popAmt;
        uv /= popScale;

        // Skip if outside the inscribed unit circle (cells are circular).
        float r = length(uv);
        if (r > 1.05) continue;

        // Stroke weight responds to bass + per-band activity.
        float stroke = mix(0.8, 1.6, bD * audioDepth) * (0.9 + 0.4 * activ);
        // Orbits respond to mid.
        float pumpAud = pump + 0.6 * mD * audioDepth * sin(t * 1.7 + seed * 5.0);

        // AA — fwidth in cell-local space; we approximate by sampling
        // the screen-space derivative of r against the cell radius.
        float fw = max(fwidth(r) * 1.2, 0.004);
        // High-band crispens by tightening fwidth.
        fw *= mix(1.0, 0.55, hD * audioDepth);

        // Render variant.
        float ink = cellVariant(variant, uv, t, stroke, seed, pumpAud, fw);

        // Cell accent — palette indexed by (cx,cy,band).
        int paletteIdx = int(mod(seed * 6.0 + float(cx) + float(cy) * 2.0 + float(band)*3.0, 6.0));
        vec3 accent    = palettePick(mode, paletteIdx, t + float(band) * 1.7);

        // Per-cell card: a soft tinted disc behind the variant — gives the
        // contact-sheet a coloured ground (matches reference's gradient cells).
        float card = aa(length(uv) - 0.94, fw);
        // Subtle internal gradient on the card so it reads dimensional.
        float cardGrad = smoothstep(0.96, -0.20, uv.y - 0.05 * sin(t*0.6 + seed*6.0));
        vec3 cardCol = mix(accent * 0.55, accent, cardGrad);
        // Cards on the front band are slightly desaturated when their player
        // is silent (so active player visually dominates).
        cardCol = mix(cardCol * 0.85, cardCol, 0.4 + 0.6 * (energy * activ));

        // Ink colour: contrasted accent (use inkColor when accent is light).
        float lum = dot(accent, vec3(0.299, 0.587, 0.114));
        vec3 inkC = (lum > 0.55) ? inkColor.rgb : mix(vec3(1.0), inkColor.rgb, 0.20);
        // Stronger contrast on owned/active cells.
        inkC = mix(inkC, accent * 1.4 + vec3(0.05), 0.10 * energy);

        // Pop highlight: when popping, add a soft rim.
        float rim = ring(r, 0.95, 0.012 + 0.020 * popAmt, fw);
        vec3 rimCol = mix(vec3(1.0), accent, 0.35);

        // Compose this cell.
        vec3 cellOut = mix(col, cardCol, card * 0.92);
        cellOut = mix(cellOut, inkC, ink);
        cellOut = mix(cellOut, rimCol, rim * (0.4 + 0.6 * popAmt));

        // Depth haze: back band fades into paper; front band crispens.
        float haze = (band == 0) ? 0.22 : (band == 1) ? 0.10 : 0.0;
        cellOut = mix(cellOut, paper, haze * (1.0 - 0.5 * energy));

        // Card alpha (used so cells don't bleed across each other when bands
        // overlap during parallax). Use card silhouette.
        float cellAlpha = clamp(card + ink + rim, 0.0, 1.0);
        col = mix(col, cellOut, cellAlpha);
    }

    // ── Caption row — slab-typewriter under the active player's hot cell ──
    int total = charCount();
    bool live = (msgAge >= 0.0);
    if (total > 0) {
        // Reveal cap from msgAge (≈28 cps), full when static.
        const float CPS = 28.0;
        float reveal = live ? floor(msgAge * CPS) : float(total);
        int shown = clamp(int(reveal), 0, total);

        // Caption band: from gridBot down to ~0.025.
        float capTopN = CAPTION_FRAC;
        float capBotN = 0.025;
        if (uv01.y >= capBotN && uv01.y <= capTopN) {
            // Choose horizontal anchor: under whichever band has highest energy.
            float bestE = max(max(eA, eB), eC);
            // Re-bias x anchor by 30% of bestE so caption follows the loudest player.
            float anchorX = mix(0.5, 0.5 + 0.18 * (eC - eA), clamp(bestE * audioDepth, 0.0, 1.0));

            // Text layout: shown characters laid out in a single row, centered
            // on anchorX. labelScale × baseline.
            float baseH = 0.040 * labelScale;
            float baseW = baseH * (5.0 / 7.0);
            float kern  = baseW * kerning;
            float rowH  = capTopN - capBotN;
            float gh2   = baseH;
            // Centered row.
            float rowY0 = (capTopN + capBotN) * 0.5 - gh2 * 0.5;
            float rowY1 = rowY0 + gh2;
            // Caption width.
            float capW = float(total) * kern;
            // Aspect-corrected anchor → uv01 space.
            float startX = anchorX - capW * 0.5;

            if (uv01.y >= rowY0 && uv01.y <= rowY1 &&
                uv01.x >= startX && uv01.x <= startX + capW) {

                float lx = uv01.x - startX;
                float ly = uv01.y - rowY0;
                int   tcol = int(floor(lx / kern));
                if (tcol >= 0 && tcol < total && tcol < shown) {
                    float colPad = (kern - baseW) * 0.5;
                    float cellLx = (lx - float(tcol) * kern - colPad) / baseW;
                    // uv01.y is y-UP normalized; ly = uv01.y - rowY0 is
                    // y-up from row bottom. Host font atlas stores
                    // letter-top at v=1, so direct y-up→v mapping puts
                    // letter-top at screen-top. The previous `1.0 -`
                    // here flipped glyphs upside down.
                    float cellLy = ly / baseH;
                    int ch = getChar(tcol);
                    if (ch >= 0 && ch <= 35 && ch != SPACE_CH) {
                        float s = sampleChar(ch, vec2(cellLx, cellLy));
                        s = smoothstep(0.22, 0.55, s);
                        // Caption ink: contrast against paper.
                        vec3 capInk = inkColor.rgb;
                        // Caption underline — slab under the row.
                        col = mix(col, capInk, s);
                    }
                }
            }

            // Caption baseline rule — a thin slab under the row.
            float ruleY = rowY0 - 0.008;
            float ruleW = 0.002 + 0.002 * labelScale;
            if (abs(uv01.y - ruleY) < ruleW &&
                uv01.x > 0.05 * aspect && uv01.x < 1.0 - 0.05 * aspect) {
                col = mix(col, inkColor.rgb, 0.18);
            }
        }
    }

    // ── Global film grain (continuous, never a pixel grid) ──
    float grain = fbm2(uv01 * res.y * 0.012 + vec2(0.0, t * 0.05)) - 0.5;
    col *= 1.0 + grain * 0.045;

    // ── Bass-lift global luminance bump ──
    col *= 1.0 + 0.05 * bD * audioDepth;

    // Soft tone curve so highlights don't clip.
    col = col / (1.0 + 0.20 * col);
    col = pow(max(col, 0.0), vec3(0.92));

    gl_FragColor = vec4(col, 1.0);
}
