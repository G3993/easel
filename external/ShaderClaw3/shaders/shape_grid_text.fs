/*{
  "DESCRIPTION": "Shape Grid · Text — an editorial contact-sheet plate. Each cell of an R×C grid hosts a DISTINCT SDF specimen (square / triangle / diamond / cross / star / halfmoon / arc / hexagon / chevron / capsule / blade / lens), framed by hairline rules and corner index numerals — a typography research sheet, not a checkerboard. Three player channels each own a parallax depth band (Back / Mid / Front): cells in that band swell, brighten, and tilt forward when their player speaks. Bass thickens hairlines; mid pumps shape rotation; high crisps highlights. The live cue line types out across the bottom slab as caption, glued under the loudest cell. Real depth: three parallax planes + per-cell pseudo-3D tilt on energy spikes. Premium fwidth-AA. Returns LINEAR HDR.",
  "CREDIT": "ShaderClaw — A-List drop · shape_grid_text",
  "CATEGORIES": [
    "Generator",
    "Text",
    "A-List"
  ],
  "INPUTS": [
    {
      "NAME": "fidBloom",
      "LABEL": "Glow",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1.5
    },
    {
      "NAME": "fidDither",
      "LABEL": "Dither",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "fidGamma",
      "LABEL": "Gamma",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "fidEdgeGlow",
      "LABEL": "Edge Glow",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "fidVignette",
      "LABEL": "Vignette",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1.5
    },
    {
      "NAME": "fidGrain",
      "LABEL": "Grain",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "rows",
      "LABEL": "Rows",
      "TYPE": "long",
      "DEFAULT": 4,
      "VALUES": [
        3,
        4,
        5,
        6
      ],
      "LABELS": [
        "3",
        "4",
        "5",
        "6"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "cols",
      "LABEL": "Cols",
      "TYPE": "long",
      "DEFAULT": 4,
      "VALUES": [
        3,
        4,
        5,
        6
      ],
      "LABELS": [
        "3",
        "4",
        "5",
        "6"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "variantMix",
      "LABEL": "Variant Mix",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "motionSpeed",
      "LABEL": "Motion Speed",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1.6,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "popAmount",
      "LABEL": "Pop (per cell)",
      "TYPE": "float",
      "DEFAULT": 0.65,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionDrift",
      "LABEL": "Drift Speed",
      "TYPE": "float",
      "DEFAULT": 1.3,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionJitter",
      "LABEL": "Jitter",
      "TYPE": "float",
      "DEFAULT": 0.25,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionFlicker",
      "LABEL": "Flicker",
      "TYPE": "float",
      "DEFAULT": 0.15,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionSway",
      "LABEL": "Sway",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionChaos",
      "LABEL": "Chaos",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "paletteMode",
      "LABEL": "Palette",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Hi-Vis",
        "Paper",
        "Editorial",
        "Mono"
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "LABEL": "Hue Shift",
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "LABEL": "Color Boost",
      "GROUP": "Color"
    },
    {
      "NAME": "parallax",
      "LABEL": "Parallax",
      "TYPE": "float",
      "DEFAULT": 0.95,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "tilt3D",
      "LABEL": "Pseudo-3D Tilt",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "msg",
      "LABEL": "Caption",
      "TYPE": "text",
      "DEFAULT": "SHAPE GRID PLATE 01 IS A GALLERY",
      "MAX_LENGTH": 48,
      "BIND": "cue.latest",
      "GROUP": "Text"
    },
    {
      "NAME": "labelScale",
      "LABEL": "Caption Scale",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.5,
      "MAX": 1.8,
      "GROUP": "Text"
    },
    {
      "NAME": "kerning",
      "LABEL": "Kerning",
      "TYPE": "float",
      "DEFAULT": 0.95,
      "MIN": 0.55,
      "MAX": 1.4,
      "GROUP": "Text"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "LABEL": "Background",
      "GROUP": "Background"
    },
    {
      "NAME": "energyA",
      "LABEL": "Player 1 (Back)",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "BIND": "player[1].energy",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "energyB",
      "LABEL": "Player 2 (Mid)",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "BIND": "player[2].energy",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "energyC",
      "LABEL": "Player 3 (Front)",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "BIND": "player[3].energy",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "activeA",
      "LABEL": "Player 1 Active",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "BIND": "player[1].active",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "activeB",
      "LABEL": "Player 2 Active",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "BIND": "player[2].active",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "bassDrive",
      "LABEL": "Bass (Stroke)",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1,
      "BIND": "audio.bass",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "midDrive",
      "LABEL": "Mid (Rotation)",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 1,
      "BIND": "audio.mid",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "highDrive",
      "LABEL": "High (Crisp)",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 1,
      "BIND": "audio.high",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "audioDepth",
      "LABEL": "Audio Depth",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ── FIDELITY KIT v2 (text-safe cinematic polish) ────────────────────
// Pure RGB math on the final color — no displacement, no chroma shift,
// glyph shapes never touched. Stages: edge glow (dFdx) → headroom bloom
// → vignette → animated grain → soft Reinhard tonemap → dither + sRGB.
vec3 fidApply(vec3 col, vec2 frag) {
    float l = dot(col, vec3(0.299, 0.587, 0.114));
    vec2  lg   = vec2(dFdx(l), dFdy(l));
    float edge = clamp(length(lg) * 7.0, 0.0, 1.0);
    col += col * edge * fidEdgeGlow * 1.50;
    float headroom = smoothstep(0.28, 0.95, l);
    col += col * headroom * fidBloom * 1.80;
    vec2  uvN = frag / RENDERSIZE - 0.5;
    float vig = 1.0 - dot(uvN, uvN) * 1.80 * fidVignette;
    col *= clamp(vig, 0.0, 1.0);
    float g = fract(sin(dot(frag + vec2(TIME * 73.0, TIME * 41.0),
                            vec2(12.9898, 78.233))) * 43758.5453);
    col += (g - 0.5) * fidGrain * 0.045;
    col = col / (1.0 + col * 0.18);
    float n = fract(sin(dot(frag, vec2(12.9898, 78.233))) * 43758.5453);
    col += (n - 0.5) * (1.0 / 255.0) * fidDither;
    col = mix(col, pow(max(col, 0.0), vec3(1.0 / 2.2)), fidGamma);
    return col;
}


// ─── MOTION KIT (shared across recent text shaders) ─────────────────
// Adds life on top of each shader's native animation: a breathing sway +
// continuous drift + coarse reseeding jitter on the working coord, plus a
// brightness flicker. Driven by the motion* uniforms. mkMotion() returns an
// offset to add to a coord; mkFlicker() a brightness multiplier.
float mkHash(vec2 p){ p = fract(p * vec2(127.1, 311.7)); p += dot(p, p + 34.5); return fract(p.x * p.y); }
vec2 mkMotion(vec2 q, float t){
    float ch = 0.4 + motionChaos;
    vec2 sway  = vec2(sin(t * 0.32 + q.y * 1.8), cos(t * 0.27 + q.x * 1.6)) * motionSway  * 0.09;
    vec2 drift = vec2(sin(t * 0.12 * ch), cos(t * 0.10 * ch))            * motionDrift * 0.05;
    // Smooth organic wander — layered incommensurate sines, slow + no stepping.
    float f = 1.0 + 1.2 * motionChaos;
    vec2 jit = vec2(
        sin(t * 0.70 * f + q.y * 3.1) * 0.6 + sin(t * 0.45 * f + q.x * 2.3 + 1.7) * 0.4,
        cos(t * 0.60 * f + q.x * 2.7) * 0.6 + cos(t * 0.50 * f + q.y * 2.9 + 4.2) * 0.4
    ) * motionJitter * 0.05;
    return sway + drift + jit;
}
float mkFlicker(vec2 q, float t){
    // Smooth, slow brightness undulation + soft scanline (no hard strobe).
    float n    = 0.5 + 0.5 * sin(t * 2.0 + q.x * 7.0 + q.y * 5.0);
    float scan = 0.5 + 0.5 * sin(q.y * 180.0 + t * 3.0);
    return 1.0 - motionFlicker * (0.5 * n + 0.30 * scan);
}


// ═══════════════════════════════════════════════════════════════════════════
//  SHAPE GRID · TEXT  ·  editorial contact-sheet
//  Each cell hosts a DISTINCT premium SDF specimen — 12 variants on rotation.
//  Three player channels own parallax bands; cells pop forward on energy.
//  Caption types across the bottom slab. No checkerboard — variety is the
//  whole point. Premium fwidth-AA, gallery grade.
// ═══════════════════════════════════════════════════════════════════════════

#define MAX_CELLS    36
#define MAX_WALK     48
#define SPACE_CH     26
#define VARIANT_N    12
const float TAU = 6.28318530718;

// ─── Font atlas ──────────────────────────────────────────────────────────
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

// Atlas index for an ASCII digit (0..9 → 27..36 in the Shader-Claw atlas).
// Used to render the per-cell index numerals at the corners.
int digitGlyph(int d) {
    if (d < 0 || d > 9) return -1;
    return 27 + d;
}

// ─── Hash / noise utils ──────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float hash12(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// ─── SDF primitives ──────────────────────────────────────────────────────
float sdBox(vec2 p, vec2 b) {
    vec2 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}
float sdCircle(vec2 p, float r) { return length(p) - r; }
float sdTriangleEq(vec2 p, float r) {
    const float k = 1.7320508;
    p.x = abs(p.x) - r;
    p.y = p.y + r / k;
    if (p.x + k * p.y > 0.0) p = vec2(p.x - k * p.y, -k * p.x - p.y) / 2.0;
    p.x -= clamp(p.x, -2.0 * r, 0.0);
    return -length(p) * sign(p.y);
}
float sdRhombus(vec2 p, vec2 b) {
    p = abs(p);
    float h = clamp((-2.0 * (p.x * b.x - p.y * b.y) + b.x * b.x - b.y * b.y) /
                    dot(b, b), -1.0, 1.0);
    float d = length(p - 0.5 * b * vec2(1.0 - h, 1.0 + h));
    return d * sign(p.x * b.y + p.y * b.x - b.x * b.y);
}
float sdCross(vec2 p, vec2 b, float r) {
    p = abs(p);
    if (p.y > p.x) p = p.yx;
    vec2 q = p - b;
    float k = max(q.y, q.x);
    vec2 w = (k > 0.0) ? q : vec2(b.y - p.x, -k);
    return sign(k) * length(max(w, 0.0)) + r;
}
float sdHexagon(vec2 p, float r) {
    const vec3 k = vec3(-0.866025404, 0.5, 0.577350269);
    p = abs(p);
    p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
    p -= vec2(clamp(p.x, -k.z * r, k.z * r), r);
    return length(p) * sign(p.y);
}
float sdStar5(vec2 p, float r, float rf) {
    const vec2 k1 = vec2(0.809016994, -0.587785252);
    const vec2 k2 = vec2(-k1.x, k1.y);
    p.x = abs(p.x);
    p -= 2.0 * max(dot(k1, p), 0.0) * k1;
    p -= 2.0 * max(dot(k2, p), 0.0) * k2;
    p.x = abs(p.x);
    p.y -= r;
    vec2 ba = rf * vec2(-k1.y, k1.x) - vec2(0.0, 1.0);
    float h = clamp(dot(p, ba) / dot(ba, ba), 0.0, r);
    return length(p - ba * h) * sign(p.y * ba.x - p.x * ba.y);
}
float sdArc(vec2 p, vec2 sc, float ra, float rb) {
    p.x = abs(p.x);
    return ((sc.y * p.x > sc.x * p.y) ? length(p - sc * ra) : abs(length(p) - ra)) - rb;
}
float sdSegment(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h);
}
float sdRoundBox(vec2 p, vec2 b, float r) {
    vec2 d = abs(p) - b + r;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0) - r;
}
float sdVesica(vec2 p, float r, float d) {
    p = abs(p);
    float b = sqrt(r * r - d * d);
    return ((p.y - b) * d > p.x * b)
        ? length(p - vec2(0.0, b)) * sign(d)
        : length(p - vec2(-d, 0.0)) - r;
}
mat2 rot2(float a) { float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }

// ─── Cell variant — distinct SDF specimen per cell index ─────────────────
// Returns signed distance for the shape. cellHalf is the cell's half-extent
// (0.5 of cell side). pulse pumps shape size; spin rotates around centre.
float cellShape(int variant, vec2 q, float cellHalf, float pulse, float spin) {
    float baseR = cellHalf * (0.55 + 0.10 * pulse);
    q = rot2(spin) * q;

    if (variant == 0) {                       // SQUARE
        return sdBox(q, vec2(baseR));
    } else if (variant == 1) {                // TRIANGLE
        return sdTriangleEq(q, baseR * 1.05);
    } else if (variant == 2) {                // DIAMOND
        return sdRhombus(q, vec2(baseR, baseR * 1.05));
    } else if (variant == 3) {                // CROSS
        return sdCross(q, vec2(baseR, baseR * 0.34), 0.0);
    } else if (variant == 4) {                // STAR
        return sdStar5(q, baseR, 0.42);
    } else if (variant == 5) {                // HALFMOON (vesica)
        return sdVesica(rot2(0.0) * q, baseR * 1.15, baseR * 0.55);
    } else if (variant == 6) {                // ARC (open ring slice)
        // half-circle bracket — sc encodes half angle
        vec2 sc = vec2(sin(2.0), cos(2.0));
        return sdArc(q, sc, baseR * 0.92, baseR * 0.12);
    } else if (variant == 7) {                // HEXAGON
        return sdHexagon(q, baseR * 0.96);
    } else if (variant == 8) {                // CHEVRON (rotated thick V)
        vec2 q2 = rot2(0.78539816) * q;       // 45°
        float a = sdSegment(q2, vec2(-baseR * 0.9, -baseR * 0.4), vec2(0.0,  baseR * 0.4));
        float b = sdSegment(q2, vec2( baseR * 0.9, -baseR * 0.4), vec2(0.0,  baseR * 0.4));
        return min(a, b) - baseR * 0.16;
    } else if (variant == 9) {                // CAPSULE
        float seg = sdSegment(q, vec2(-baseR * 0.7, 0.0), vec2(baseR * 0.7, 0.0));
        return seg - baseR * 0.34;
    } else if (variant == 10) {               // BLADE — rounded skewed
        vec2 q2 = q;
        q2.x += 0.20 * q2.y;
        return sdRoundBox(q2, vec2(baseR * 0.95, baseR * 0.55), baseR * 0.35);
    } else {                                  // LENS — two circle intersection
        float c1 = sdCircle(q - vec2( baseR * 0.45, 0.0), baseR * 0.95);
        float c2 = sdCircle(q - vec2(-baseR * 0.45, 0.0), baseR * 0.95);
        return max(c1, c2);
    }
}

// ─── Palette ─────────────────────────────────────────────────────────────
// paletteMode 0 = Hi-Vis yellow (matches the reference), 1 = Paper, 2 =
// Editorial (warm white + red accent), 3 = Mono.
vec3 paletteBg(int m) {
    if (m == 0) return vec3(0.994, 0.910, 0.094);   // saturated yellow
    if (m == 1) return vec3(0.945, 0.935, 0.910);   // warm paper
    if (m == 2) return vec3(0.965, 0.945, 0.920);   // editorial off-white
    return vec3(0.910, 0.910, 0.905);               // mono
}
vec3 paletteInk(int m) {
    if (m == 0) return vec3(0.040, 0.040, 0.045);   // near-black on yellow
    if (m == 1) return vec3(0.050, 0.050, 0.070);
    if (m == 2) return vec3(0.700, 0.110, 0.090);   // editorial red
    return vec3(0.080, 0.080, 0.090);
}
vec3 paletteAccent(int m) {
    if (m == 0) return vec3(0.040, 0.040, 0.045);   // mono rules on yellow
    if (m == 1) return vec3(0.120, 0.130, 0.180);
    if (m == 2) return vec3(0.105, 0.110, 0.135);
    return vec3(0.300, 0.300, 0.305);
}

// ─── Player-band assignment for a cell ───────────────────────────────────
// Splits the grid into three diagonal/horizontal bands so each player owns
// a visually contiguous region. 0 = Back, 1 = Mid, 2 = Front.
int cellBand(int cx, int cy, int Rc, int Cc) {
    // Diagonal split — distance from top-left as fraction of total span.
    float t = (float(cx) + float(cy)) / max(float(Rc + Cc - 2), 1.0);
    if (t < 0.40) return 0;
    if (t < 0.72) return 1;
    return 2;
}

// ─── Glyph blitter — single character at (cx, cy) of a left-aligned row
// at given height. Used for the corner index numerals AND the caption row.
float blitChar(int ch, vec2 p, vec2 origin, float h) {
    if (ch < 0 || ch > 36) return 0.0;
    float w = h * (5.0 / 7.0);
    vec2 q = p - origin;
    if (q.x < 0.0 || q.x > w) return 0.0;
    if (q.y < 0.0 || q.y > h) return 0.0;
    // q.y = p.y - origin.y; p.y is y-UP world, so q.y grows
    // screen-bottom→top with origin at the bottom of the glyph box. The
    // host font atlas stores letter-top at v=1, so direct y-up→v mapping
    // puts letter-top at screen-top. The previous comment claimed q grew
    // top→bottom — that was incorrect, and the `1.0 -` flipped glyphs
    // upside down.
    float s = sampleChar(ch, vec2(q.x / w, q.y / h));
    return smoothstep(0.18, 0.55, s);
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    uv += mkMotion(uv, TIME);
    float aspect = res.x / res.y;
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = (uv.y - 0.5);

    // ─── Audio / channel intake ──────────────────────────────────────────
    float eA = clamp(energyA, 0.0, 1.0);
    float eB = clamp(energyB, 0.0, 1.0);
    float eC = clamp(energyC, 0.0, 1.0);
    float aA = clamp(activeA, 0.0, 1.0);
    float aB = clamp(activeB, 0.0, 1.0);
    float bandE0 = eA * mix(0.5, 1.0, aA);
    float bandE1 = eB * mix(0.5, 1.0, aB);
    float bandE2 = eC;
    float audAct  = clamp(audioDepth, 0.0, 2.0);
    // Live audio bus tops up each Drive knob so silence still holds the
    // knob baseline but the track visibly pushes stroke/rotation/crisp.
    float bass    = clamp(bassDrive + audioBass * 0.6, 0.0, 1.4);
    float midF    = clamp(midDrive  + audioMid  * 0.6, 0.0, 1.4);
    float highF   = clamp(highDrive + audioHigh * 0.6, 0.0, 1.4);
    float spdMul  = clamp(motionSpeed, 0.0, 1.6);
    float t       = TIME * (0.45 + 0.65 * spdMul);

    // ─── Palette ─────────────────────────────────────────────────────────
    int   pmode  = int(paletteMode);
    vec3  bg     = paletteBg(pmode);
    vec3  ink    = paletteInk(pmode);
    vec3  rule   = paletteAccent(pmode);

    // Subtle background vignette + faint paper grain so the field never
    // reads as a flat fill — keeps the high-vis yellow from looking digital.
    float vign = 1.0 - 0.10 * dot(p * 1.05, p * 1.05);
    float grain = hash12(floor(gl_FragCoord.xy * 0.5)) - 0.5;
    vec3 col = bg * vign + grain * 0.012;
    // universal background override — tint the plate base (a=0 -> untouched)
    col = mix(col, bgColor.rgb, bgColor.a);

    // ─── Grid layout ─────────────────────────────────────────────────────
    int Rc = int(rows); if (Rc < 3) Rc = 3; if (Rc > 6) Rc = 6;
    int Cc = int(cols); if (Cc < 3) Cc = 3; if (Cc > 6) Cc = 6;

    // Margin on the canvas; reserve a slab at the bottom for the caption.
    float margin     = 0.06;
    float capSlabH   = 0.13;
    float gridLeft   = -0.5 * aspect + margin;
    float gridRight  =  0.5 * aspect - margin;
    float gridTop    =  0.5 - margin;
    float gridBot    = -0.5 + margin + capSlabH;
    float gridW      = gridRight - gridLeft;
    float gridH      = gridTop   - gridBot;
    float cellW      = gridW / float(Cc);
    float cellH      = gridH / float(Rc);

    // ─── Cell pass ───────────────────────────────────────────────────────
    // Find the cell this fragment lives in (if any) and accumulate the
    // best (smallest) shape SDF + a per-band parallax offset.
    float hairlineMix = 0.0;     // rule strokes between cells
    float fillMix     = 0.0;     // shape fill alpha
    float numeralMix  = 0.0;     // corner numerals alpha
    vec3  cellInk     = ink;
    int   loudestCell = 0;
    float loudestE    = -1.0;
    int   loudestCx   = 0;
    int   loudestCy   = 0;
    float loudestCx0  = gridLeft;
    float loudestCy0  = gridBot;

    if (p.x > gridLeft && p.x < gridRight && p.y > gridBot && p.y < gridTop) {
        // Cell index for this fragment
        int cxF = int(floor((p.x - gridLeft) / cellW));
        int cyF = int(floor((p.y - gridBot) / cellH));
        if (cxF < 0) cxF = 0; if (cxF >= Cc) cxF = Cc - 1;
        if (cyF < 0) cyF = 0; if (cyF >= Rc) cyF = Rc - 1;

        // Cell origin (lower-left) and centre
        float x0 = gridLeft + float(cxF) * cellW;
        float y0 = gridBot  + float(cyF) * cellH;
        vec2  ctr = vec2(x0 + 0.5 * cellW, y0 + 0.5 * cellH);

        // ── Hairline rules — vertical + horizontal column/row dividers ──
        float strokeBase = 0.0024 + 0.0035 * bass;
        float dV = min(abs(p.x - x0), abs(p.x - (x0 + cellW)));
        float dH = min(abs(p.y - y0), abs(p.y - (y0 + cellH)));
        float dRule = min(dV, dH);
        float fw    = fwidth(dRule);
        float rulA  = 1.0 - smoothstep(strokeBase - fw, strokeBase + fw, dRule);
        // Outer page edges thicker than inner rules (editorial frame)
        bool edge =
            (abs(p.x - gridLeft) < 0.0015) ||
            (abs(p.x - gridRight) < 0.0015) ||
            (abs(p.y - gridBot) < 0.0015) ||
            (abs(p.y - gridTop) < 0.0015);
        if (edge) rulA = max(rulA, 1.0);
        hairlineMix = rulA;

        // ── Per-cell variant + band + parallax offset ────────────────────
        int cellIdx = cyF * Cc + cxF;
        float cellHash = hash12(vec2(float(cxF) * 13.7, float(cyF) * 7.21));
        // Deterministic but varied variant assignment — never repeats two
        // adjacent cells (xor with neighbour hash bumps the index).
        int variant = int(mod(floor(cellHash * float(VARIANT_N) * clamp(variantMix, 0.0, 1.5)), float(VARIANT_N)));
        int band    = cellBand(cxF, cyF, Rc, Cc);
        float e     = ((band == 0) ? bandE0 : ((band == 1) ? bandE1 : bandE2)) * audAct;

        // Parallax: band depth pushes cells forward toward the camera.
        // bandZ in [-1,1], -1 = Back (further), +1 = Front (closer). Local
        // displacement scales by parallax slider; per-cell jitter keeps it
        // from reading as three uniform planes.
        float bandZ = (band == 0) ? -1.0 : (band == 1 ? 0.0 : 1.0);
        float depthShift = bandZ * 0.025 * parallax + 0.012 * (cellHash - 0.5);
        // Energy pop: cell grows toward camera; clamp so neighbours never
        // bleed into adjacent cells.
        float pop   = popAmount * smoothstep(0.04, 0.85, e);
        float scale = 1.0 + 0.35 * pop;

        // Pseudo-3D tilt — energy-driven shear toward a vanishing point
        // at the centre of the canvas. Sells "leaning forward" without
        // raymarching. Tilt amount eases with energy and bandZ.
        vec2 toCenter = -ctr;
        float tiltA = tilt3D * 0.45 * pop * sign(bandZ + 0.001);
        vec2 q = p - ctr;
        q = q - depthShift * normalize(toCenter + vec2(1e-3));
        q = q * (1.0 / scale);
        // Shear toward centre creates fake perspective
        q.x += tiltA * q.y * sign(-ctr.x);
        q.y += tiltA * 0.5 * q.x * sign(-ctr.y);

        // Per-cell spin — mid drives uniform rotation; energy adds wobble.
        float spin = 0.25 * sin(t * 0.6 + cellHash * 9.3)
                   + 0.9 * midF * (cellHash - 0.5)
                   + 1.6 * pop  * sin(t * 1.8 + cellHash * 11.1);

        // Per-cell breathing pulse (frame-by-frame motion, never idle)
        float pulse = 0.18 * sin(t * 0.9 + cellHash * 6.0)
                    + 0.55 * pop;

        float cellHalf = 0.5 * min(cellW, cellH);
        float sd = cellShape(variant, q, cellHalf, pulse, spin);

        // Fill the shape — solid ink, anti-aliased on its silhouette.
        float fw2 = fwidth(sd) * (1.0 - 0.55 * highF);   // crisper highs
        float a = 1.0 - smoothstep(-fw2, fw2, sd);
        fillMix = a;

        // Per-band ink wash — slight hue/brightness diff so back/mid/front
        // read as depth bands without breaking the bichrome rule.
        vec3 bandInk = ink;
        if (pmode == 0) {
            if (band == 0) bandInk = mix(ink, vec3(0.07, 0.04, 0.05), 0.35);
            if (band == 2) bandInk = mix(ink, vec3(0.02, 0.02, 0.04), 0.50);
        } else if (pmode == 2) {
            if (band == 0) bandInk = mix(ink, vec3(0.40, 0.08, 0.07), 0.40);
            if (band == 1) bandInk = ink;
            if (band == 2) bandInk = mix(ink, vec3(0.02, 0.02, 0.04), 0.55);
        }
        // Active-player kicker: bandInk darkens slightly when its player
        // is loud — pulls the band visually forward beyond the geometry.
        bandInk = mix(bandInk, ink * 0.85, 0.35 * smoothstep(0.0, 0.8, e));
        cellInk = bandInk;

        // ── Corner index numerals (one per cell corner, like the ref) ────
        // Each cell gets a 1- or 2-digit numeral inset in one corner. Index
        // is the cell's running number (1..R*C). Corner picked deterministic-
        // ally from the cell hash so the layout reads as designed-on-paper.
        int idxNum = cellIdx + 1;
        int cornerSel = int(mod(floor(cellHash * 4.0), 4.0));
        float numH = cellH * 0.12;
        float numW = numH * (5.0 / 7.0);
        float pad = cellH * 0.07;
        vec2 numOrigin;
        if (cornerSel == 0) numOrigin = vec2(x0 + pad,                     y0 + cellH - pad - numH); // TL
        else if (cornerSel == 1) numOrigin = vec2(x0 + cellW - pad - numW * 2.0, y0 + cellH - pad - numH); // TR
        else if (cornerSel == 2) numOrigin = vec2(x0 + pad,                     y0 + pad);                // BL
        else                     numOrigin = vec2(x0 + cellW - pad - numW * 2.0, y0 + pad);                // BR
        if (idxNum < 10) {
            int d0 = digitGlyph(idxNum);
            float g = blitChar(d0, p, numOrigin, numH);
            numeralMix = max(numeralMix, g);
        } else {
            int d0 = digitGlyph(idxNum / 10);
            int d1 = digitGlyph(idxNum - (idxNum / 10) * 10);
            float g0 = blitChar(d0, p, numOrigin, numH);
            float g1 = blitChar(d1, p, numOrigin + vec2(numW, 0.0), numH);
            numeralMix = max(numeralMix, max(g0, g1));
        }

        // Track loudest cell for caption anchoring (single fragment is fine
        // — we recompute the lookup later at the slab too, but stashing
        // here keeps the data local).
        if (e > loudestE) {
            loudestE   = e;
            loudestCx  = cxF; loudestCy = cyF; loudestCx0 = x0; loudestCy0 = y0;
            loudestCell = cellIdx;
        }
    }

    // ─── Compose grid layer ──────────────────────────────────────────────
    // Hairlines underneath, shape fill on top, numerals on top of that.
    col = mix(col, rule, hairlineMix * 0.95);
    col = mix(col, cellInk, fillMix);
    col = mix(col, ink, numeralMix);

    // ─── Caption slab — typewriter row at bottom ─────────────────────────
    // The slab is a rounded box anchored under the loudest cell horizontal-
    // ly; vertically pinned to the bottom margin so the row reads as a
    // research-plate caption.
    int total = charCount();
    if (total > 0) {
        // Anchor x: centred under loudest cell when audio is present;
        // otherwise centred on the canvas. Smooth between modes.
        float anyE = max(max(eA, eB), eC);
        float anchorX;
        if (anyE > 0.02) {
            // Hash-based loudest-cell lookup over the grid (deterministic
            // pick: scan all cells per fragment is fine — small loop, only
            // when there's actually text to draw).
            float bestE  = -1.0;
            float bestX0 = gridLeft;
            for (int cy = 0; cy < 6; cy++) {
                if (cy >= Rc) break;
                for (int cx = 0; cx < 6; cx++) {
                    if (cx >= Cc) break;
                    int band = cellBand(cx, cy, Rc, Cc);
                    float e = ((band == 0) ? bandE0 : ((band == 1) ? bandE1 : bandE2)) * audAct;
                    if (e > bestE) {
                        bestE = e;
                        bestX0 = gridLeft + float(cx) * cellW;
                    }
                }
            }
            anchorX = bestX0 + 0.5 * cellW;
        } else {
            anchorX = 0.0;
        }
        // Caption baseline + height
        float capH    = capSlabH * 0.42 * clamp(labelScale, 0.5, 1.8);
        float capW    = capH * (5.0 / 7.0);
        float capKern = capW * clamp(kerning, 0.55, 1.4);
        float baseY   = gridBot - capSlabH * 0.55;        // centred in slab

        // Slab background — soft rounded box anchored to anchorX
        float slabW = capKern * min(float(total), 36.0) + capH * 1.4;
        slabW = min(slabW, aspect - 2.0 * margin);
        float slabH = capH * 1.8;
        float slabHalfW = 0.5 * slabW;
        // Keep slab inside the canvas margins
        float ax = clamp(anchorX, -0.5 * aspect + margin + slabHalfW,
                                   0.5 * aspect - margin - slabHalfW);
        vec2 slabCenter = vec2(ax, baseY);
        float slabSdf = sdRoundBox(p - slabCenter, vec2(slabHalfW, 0.5 * slabH), capH * 0.45);
        float slabFw  = fwidth(slabSdf);
        float slabFill = 1.0 - smoothstep(-slabFw, slabFw, slabSdf);
        // Slab is ink-filled with paper text — INVERTED block, editorial.
        col = mix(col, ink, slabFill);

        // Typewriter reveal — clamp visible chars to msg_len. With cue.latest,
        // msg_len grows char-by-char so the caption types out live. Without a
        // live transcript, total = full length → the whole caption renders.
        int shown = total;
        if (shown > 36) shown = 36;
        // Caption layout: left-aligned inside the slab, vertically centred.
        float textOriginX = ax - 0.5 * slabHalfW * 1.6;   // slight inset
        textOriginX = ax - (capKern * float(shown)) * 0.5;
        float textOriginY = baseY - 0.5 * capH;

        // Cursor character (blinking underscore-ish block) when live
        bool live = msgAge >= 0.0;
        float cursorBlink = step(0.5, fract(TIME * 1.8));

        for (int i = 0; i < MAX_WALK; i++) {
            if (i >= shown) break;
            int ch = getChar(i);
            vec2 o = vec2(textOriginX + capKern * float(i), textOriginY);
            float g = blitChar(ch, p, o, capH);
            // Caption colour: paper-on-ink (inverted slab look). Multiply by
            // slabFill so the glyphs only show inside the slab silhouette.
            float gw = g * slabFill;
            col = mix(col, bg, gw);
        }
        // Cursor block at the end of the typed text (live only)
        if (live && cursorBlink > 0.5) {
            vec2 cO = vec2(textOriginX + capKern * float(shown), textOriginY);
            float curBox = sdBox(p - (cO + vec2(capW * 0.5, capH * 0.5)),
                                 vec2(capW * 0.45, capH * 0.45));
            float cFw = fwidth(curBox);
            float cFill = (1.0 - smoothstep(-cFw, cFw, curBox)) * slabFill;
            col = mix(col, bg, cFill);
        }
    }

    // ─── Final tonal polish ──────────────────────────────────────────────
    // Slight crisp boost on the highs (audio-reactive sharpening of the
    // shapes — fillMix term gives the boost a body to land on).
    col += fillMix * highF * 0.04;
    // Gentle gallery sheen — wide diagonal sweep, very subtle
    float sweep = smoothstep(0.0, 1.0, sin(p.x * 1.4 - p.y * 0.9 - t * 0.25) * 0.5 + 0.5);
    col += pow(sweep, 6.0) * 0.022 * vec3(1.0, 0.98, 0.92);
    // Soft tonemap so the yellow never burns
    col = col / (1.0 + 0.18 * col);

    // Bass breath — gentle whole-grid pulse so the piece visibly breathes
    // with the track, not just the stroke width.
    col *= 1.0 - 0.30 * clamp(bass, 0.0, 1.4);

    col *= mkFlicker(gl_FragCoord.xy / RENDERSIZE - 0.5, TIME);

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = fidApply(col, gl_FragCoord.xy);
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);
    if (hueShift > 0.0005) {
        float hueA = hueShift * 6.2831853;
        float hueC = cos(hueA), hueS = sin(hueA);
        mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                  + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                  + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hueM * uc, 0.0, 1.0);
    }
    gl_FragColor = vec4(uc, 1.0);
}
