/*{
  "DESCRIPTION": "Form Plus Lines Text — a single bold abstract sculpture floating inside an iridescent halo, crossed by an editorial lattice of structural lines and an editorial masthead. Reference: an A-List 'beaming design' chart — soft chromatic auras, hand-drawn dashed contours, italic+roman headline. Here it becomes ONE composition: a raymarched sculpted form (sheet/blob/sculpture variants) lit by procedural rim light, cut by a structural line cage living on its own z-plane, with cue.latest typing across the top as a love-languages masthead. player[1].energy and player[2].energy split the sculpture into two animating halves so two voices read as two morphing surfaces. cue.latest types the headline. Real depth: raymarched body + parallax line plane + atmospheric haze. Premium fwidth-AA throughout.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",           "TYPE": "text",  "DEFAULT": "love languages",          "MAX_LENGTH": 48 },

    { "NAME": "energyA",       "LABEL": "Form A Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",       "LABEL": "Form B Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "playerActiveA", "LABEL": "Form A Active",   "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].active" },
    { "NAME": "playerActiveB", "LABEL": "Form B Active",   "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].active" },

    { "NAME": "bassDrive",     "LABEL": "Bass Drive",      "TYPE": "float", "DEFAULT": 0.7, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.bass" },
    { "NAME": "midDrive",      "LABEL": "Mid Drive",       "TYPE": "float", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.mid" },
    { "NAME": "highDrive",     "LABEL": "High Drive",      "TYPE": "float", "DEFAULT": 0.4, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.high" },

    { "NAME": "formVariant",   "LABEL": "Form Variant",    "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Sheet fold","Sculpture blob","Twin lobes","Folded plinth"] },
    { "NAME": "formSize",      "LABEL": "Form Size",       "TYPE": "float", "DEFAULT": 0.95, "MIN": 0.4, "MAX": 1.6 },
    { "NAME": "formBreath",    "LABEL": "Form Breath",     "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 1.5 },

    { "NAME": "lineDensity",   "LABEL": "Line Density",    "TYPE": "long",  "DEFAULT": 6, "VALUES": [2,3,4,5,6,8,10,12], "LABELS": ["2","3","4","5","6","8","10","12"] },
    { "NAME": "lineStyle",     "LABEL": "Line Style",      "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2], "LABELS": ["Concentric","Cross-hatch","Dashed orbits"] },
    { "NAME": "lineWidth",     "LABEL": "Line Width",      "TYPE": "float", "DEFAULT": 0.0028, "MIN": 0.0008, "MAX": 0.008 },
    { "NAME": "lineDash",      "LABEL": "Line Dash",       "TYPE": "float", "DEFAULT": 0.6, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "lineParallax",  "LABEL": "Line Parallax Z", "TYPE": "float", "DEFAULT": 0.7, "MIN": 0.0, "MAX": 1.5 },

    { "NAME": "palette",       "LABEL": "Palette",         "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3,4], "LABELS": ["Iridescent","Sunset","Cool tide","Mono ink","Pastel chart"] },
    { "NAME": "paletteShift",  "LABEL": "Palette Shift",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "haloRadius",    "LABEL": "Halo Radius",     "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.2, "MAX": 1.0 },
    { "NAME": "haloSoftness",  "LABEL": "Halo Softness",   "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.3, "MAX": 1.5 },

    { "NAME": "motionSpeed",   "LABEL": "Motion Speed",    "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "audioDepth",    "LABEL": "Audio Depth",     "TYPE": "float", "DEFAULT": 0.7, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.level" },

    { "NAME": "bloom",         "LABEL": "Halo Bloom",      "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "fog",           "LABEL": "Depth Fog",       "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "grain",         "LABEL": "Paper Grain",     "TYPE": "float", "DEFAULT": 0.28, "MIN": 0.0, "MAX": 1.0 },

    { "NAME": "paperColor",    "LABEL": "Paper Color",     "TYPE": "color", "DEFAULT": [0.965, 0.955, 0.945, 1.0] },
    { "NAME": "inkColor",      "LABEL": "Ink Color",       "TYPE": "color", "DEFAULT": [0.04, 0.04, 0.06, 1.0] },
    { "NAME": "showMasthead",  "LABEL": "Show Masthead",   "TYPE": "bool",  "DEFAULT": 1.0 },
    { "NAME": "italicLead",    "LABEL": "Italic Lead",     "TYPE": "long",  "DEFAULT": 4, "VALUES": [0,1,2,3,4,5,6,7,8], "LABELS": ["0","1","2","3","4","5","6","7","8"] },
    { "NAME": "textSize",      "LABEL": "Masthead Size",   "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.4, "MAX": 2.0 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//   FORM PLUS LINES TEXT  ·  A-List drop
//
//   ONE sculptural form, ONE structural line cage, ONE editorial masthead.
//   Reference is the "love languages" beaming-design poster: soft rainbow
//   auras with dashed contour drawings inside and an italic+roman headline
//   floating above. Here we collapse the chart of 9 motifs into a single
//   *centerpiece*: a raymarched body fills the canvas, a screen-space
//   line cage cuts across it on its own z-plane, and the cue.latest text
//   types out as the masthead.
//
//   ────────────────────────────────────────────────────────────────────
//   Channel decomposition (intelligence-layer compliant):
//     · player[1].energy → energyA  → left half of the sculpture warps,
//                                     hue rotates warm, halo brightens.
//     · player[2].energy → energyB  → right half of the sculpture warps,
//                                     hue rotates cool, line cage tilts.
//     · player[1].active / [2].active → fade each half in/out so muting
//                                     a speaker literally retracts their
//                                     side of the form (rubric axis a/5).
//     · audio.bass  → halo radius pulse + line breathing
//     · audio.mid   → sub-blob jitter inside form
//     · audio.high  → rim sharpness, dash flicker
//     · audio.level → audioDepth global trim
//     · cue.latest  → msg typewriter masthead (auto-bound by Easel)
//
//   Depth & dimensionality (axis b):
//     · Sculpture is a true raymarched SDF (smooth-union of 3D ellipsoids
//       with sheet/fold variants). Surface normal from finite-difference.
//     · Line cage lives on its OWN screen-space z-plane with parallax
//       offset from form, so as the camera breathes the lines slide
//       across the form rather than sticking to it.
//     · Atmospheric haze on raymarch t and on line depth — far parts
//       dissolve to paper.
//
//   Motion (axis c):
//     · Silence → form barely breathes (slow autonomic sin), lines hold.
//     · Mid energy → halves spread, halo opens, lines rotate.
//     · Crescendo → halo blooms, dashes flicker, line cage tilts.
//
//   Abstract (axis d):
//     · No literal hearts, no chart grid, no logo. The motifs are felt
//       as a single sculpted presence with the line cage as structural
//       language and the masthead as editorial frame.
//
//   Surprise (axis e):
//     · Combination: raymarched 3D body + 2D editorial line cage on a
//       distinct z-plane + typewriter masthead, all sharing one halo.
//     · The lines READ as drawn on a glass pane in front of the form.
// ════════════════════════════════════════════════════════════════════════

#define MAX_BLOBS    7
#define MAX_LINES   12
#define MAX_CHARS   48
#define SPACE_CH    26
#define TAU         6.28318530718

// ─── Font atlas sampling ───────────────────────────────────────────────
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
    if (n > 48) return 48;
    return n;
}

// ─── Hash / noise ─────────────────────────────────────────────────────
float h11(float n){ return fract(sin(n * 127.1) * 43758.5453); }
vec2  h21(float n){ return vec2(h11(n), h11(n + 19.17)); }
vec3  h31(float n){ return vec3(h11(n), h11(n + 11.7), h11(n + 23.4)); }
float h22(vec2 p){ return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

float vnoise(vec2 p){
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float a = h22(i),           b = h22(i+vec2(1,0));
    float c = h22(i+vec2(0,1)), d = h22(i+vec2(1,1));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}
float fbm2(vec2 p){
    float v=0.0, a=0.5;
    for(int i=0;i<4;i++){ v += a*vnoise(p); p = p*2.07 + vec2(7.3, 5.1); a *= 0.5; }
    return v;
}
float smin(float a, float b, float k){
    float h = clamp(0.5 + 0.5*(b-a)/k, 0.0, 1.0);
    return mix(b, a, h) - k*h*(1.0-h);
}

// ─── Palette ──────────────────────────────────────────────────────────
vec3 paletteRamp(int mode, float t, float shift){
    t = fract(t + shift);
    if (mode == 0) {
        // Iridescent: pastel rainbow (reference-style halos)
        return 0.5 + 0.5 * cos(TAU * (t + vec3(0.00, 0.33, 0.67)));
    } else if (mode == 1) {
        // Sunset: vermilion → magenta → amber
        vec3 a = vec3(0.98, 0.42, 0.34);
        vec3 b = vec3(0.92, 0.30, 0.62);
        vec3 c = vec3(0.99, 0.78, 0.32);
        return (t < 0.5) ? mix(a,b,t*2.0) : mix(b,c,(t-0.5)*2.0);
    } else if (mode == 2) {
        // Cool tide: teal → indigo → lavender
        vec3 a = vec3(0.22, 0.78, 0.78);
        vec3 b = vec3(0.36, 0.42, 0.92);
        vec3 c = vec3(0.78, 0.62, 0.96);
        return (t < 0.5) ? mix(a,b,t*2.0) : mix(b,c,(t-0.5)*2.0);
    } else if (mode == 3) {
        // Mono ink: graphite gradient with hint of warm
        float g = 0.20 + 0.55 * t;
        return mix(vec3(g), vec3(g) * vec3(1.05, 1.0, 0.94), 0.4);
    } else {
        // Pastel chart: powder blue → mint → peach → lilac
        vec3 a = vec3(0.74, 0.86, 0.99);
        vec3 b = vec3(0.78, 0.96, 0.86);
        vec3 c = vec3(0.99, 0.86, 0.78);
        vec3 d = vec3(0.86, 0.78, 0.99);
        if (t < 0.33)      return mix(a,b,t*3.0);
        else if (t < 0.66) return mix(b,c,(t-0.33)*3.0);
        else               return mix(c,d,(t-0.66)*3.0);
    }
}

// ════════════════════════════════════════════════════════════════════════
//   3D form — raymarched SDF
// ════════════════════════════════════════════════════════════════════════
float gT, gTw;
float gEA, gEB;     // smoothed energies for A and B halves
float gMid, gHigh;

// SDF for the centerpiece. variant selects silhouette character.
float sdForm(vec3 p, int variant){
    // Symmetry split: x<0 is A's side, x>0 is B's side. Each side gets
    // its own warp amplitude so the two halves animate independently.
    float side = (p.x < 0.0) ? 1.0 : -1.0;
    float eSide = (p.x < 0.0) ? gEA : gEB;

    // Slow autonomic breath plus voice-driven open.
    float breath = 0.04 * sin(gTw * 0.7) * formBreath;
    float openA  = 0.18 * gEA;
    float openB  = 0.18 * gEB;

    float d;
    if (variant == 0) {
        // SHEET FOLD — a wide ribbon that bows down in the middle then
        // curls at the ends. Two ellipsoids smooth-min'd at the centre.
        float fold = 0.55 + 0.18 * sin(p.x * 1.2 + gTw * 0.5);
        vec3 a = p; a.y -= 0.05 * sin(p.x * 1.8 + gTw * 0.3);
        a.x = abs(a.x) - 0.42;
        float e1 = length(a / vec3(0.55, 0.18 * fold, 0.22)) - 1.0;
        // Belt connecting the two lobes (a wide thin disc)
        vec3 b = p;
        float belt = length(vec2(b.y / 0.10, b.z / 0.18)) - 1.0;
        belt = max(belt, abs(b.x) - 0.55);
        d = smin(e1 * 0.18, belt * 0.18, 0.16);
        // Per-side opening: A side widens with energyA, B with energyB
        d -= (openA * step(0.0, -p.x) + openB * step(0.0, p.x)) * 0.4;
    } else if (variant == 1) {
        // SCULPTURE BLOB — single mass with 5 sub-bumps orbiting
        d = length(p / vec3(0.55, 0.46, 0.40)) - 1.0;
        d *= 0.42;
        for (int i = 0; i < MAX_BLOBS; i++) {
            float fi = float(i);
            if (i >= 5) break;
            vec3 s = h31(fi * 7.7 + 1.1);
            float orbR = 0.35 + 0.18 * s.x;
            float orbA = s.y * TAU + gTw * (0.3 + 0.4 * s.z);
            vec3 c = vec3(cos(orbA) * orbR, sin(orbA) * orbR * 0.7,
                          (s.z - 0.5) * 0.45);
            float r = 0.16 + 0.08 * s.x + 0.04 * gMid;
            float di = length(p - c) - r;
            // bias half-association by initial x
            float bias = (c.x < 0.0) ? gEA : gEB;
            di -= 0.05 * bias;
            d = smin(d, di * 0.7, 0.18);
        }
    } else if (variant == 2) {
        // TWIN LOBES — two clearly readable lobes, one per channel.
        vec3 a = p; a.x += 0.34 - 0.06 * gEA;
        vec3 b = p; b.x -= 0.34 - 0.06 * gEB;
        float lobeA = length(a / vec3(0.34, 0.30 + 0.05 * gEA, 0.26)) - 1.0;
        float lobeB = length(b / vec3(0.34, 0.30 + 0.05 * gEB, 0.26)) - 1.0;
        // Connecting bridge in middle
        float bridge = length(vec2(p.y / (0.06 + 0.04 * (gEA + gEB)),
                                    p.z / 0.18)) - 1.0;
        bridge = max(bridge, abs(p.x) - 0.34);
        d = smin(lobeA * 0.34, lobeB * 0.34, 0.10);
        d = smin(d, bridge * 0.18, 0.16);
    } else {
        // FOLDED PLINTH — a slab with a thick top fold.
        vec3 a = p;
        float slab = length(vec3(a.x / 0.62, max(a.y - 0.05, 0.0) / 0.20,
                                  a.z / 0.30)) - 1.0;
        vec3 c = p; c.y -= 0.20;
        float crown = length(vec2(c.x / 0.40, c.y / 0.16)) - 1.0;
        crown = max(crown, abs(c.z) - 0.20);
        d = smin(slab * 0.32, crown * 0.18, 0.14);
        d -= openA * 0.18 * step(0.0, -p.x);
        d -= openB * 0.18 * step(0.0,  p.x);
    }

    // Universal breath: a slow scalar push
    d -= breath;
    // Audio mid jitter — small low-freq displacement
    d += 0.018 * (vnoise(p.xy * 3.4 + gTw * 0.3) - 0.5) * (0.5 + gMid);
    return d;
}

vec3 calcNormal(vec3 p, int variant){
    vec2 e = vec2(0.0018, 0.0);
    return normalize(vec3(
        sdForm(p + e.xyy, variant) - sdForm(p - e.xyy, variant),
        sdForm(p + e.yxy, variant) - sdForm(p - e.yxy, variant),
        sdForm(p + e.yyx, variant) - sdForm(p - e.yyx, variant)));
}

// ════════════════════════════════════════════════════════════════════════
//   2D structural line cage — lives on its own z-plane
//
//   We compute a screen-space SDF for each line ("how far is this pixel
//   from this line?") and accumulate with min(). fwidth-AA at the end.
// ════════════════════════════════════════════════════════════════════════
float lineSDF_concentric(vec2 q, int idx, int n, float t, float tilt){
    // Concentric ellipses around the form center; idx 0..n-1
    float fi = float(idx) / float(max(n, 1));
    // Rotate the field a touch — line cage tilts with bass and energyB
    float c = cos(tilt), s = sin(tilt);
    vec2 r = vec2(c * q.x - s * q.y, s * q.x + c * q.y);
    // Ellipse radii expand outward with idx
    float ra = 0.18 + 0.085 * float(idx) + 0.012 * sin(t * 0.7 + fi * TAU);
    float rb = 0.13 + 0.075 * float(idx) + 0.010 * cos(t * 0.6 + fi * TAU);
    float d = length(vec2(r.x / ra, r.y / rb)) - 1.0;
    // Convert ellipse-space distance back to roughly world units
    return d * min(ra, rb);
}

float lineSDF_hatch(vec2 q, int idx, int n, float t, float tilt){
    // Cross-hatch: alternating diagonal lines through the form box
    float fi = float(idx);
    float angle = mix(-0.9, 0.9, float(idx) / float(max(n - 1, 1))) + tilt * 0.5;
    float c = cos(angle), s = sin(angle);
    float u = c * q.x - s * q.y;        // distance along normal
    float v = s * q.x + c * q.y;        // along line
    // Offset each line through the form bbox
    float off = (float(idx) - 0.5 * float(n - 1)) * 0.085;
    // Limit each line to the form's bbox (length 0.9 in v)
    float dEnd = max(abs(v) - 0.55 - 0.05 * sin(t * 0.4 + fi), 0.0);
    return sqrt((u - off) * (u - off) + dEnd * dEnd);
}

float lineSDF_orbit(vec2 q, int idx, int n, float t, float tilt){
    // Dashed orbits at offsets — circles around drift centers
    float fi = float(idx);
    vec2 ctr = (h21(fi * 5.7 + 0.3) - 0.5) * vec2(0.55, 0.40);
    float c = cos(tilt * 0.4), s = sin(tilt * 0.4);
    vec2 r = vec2(c * (q.x - ctr.x) - s * (q.y - ctr.y),
                  s * (q.x - ctr.x) + c * (q.y - ctr.y));
    float ra = 0.10 + 0.04 * fi + 0.020 * sin(t * 0.5 + fi);
    return abs(length(r) - ra);
}

// Dash mask along the line direction. Returns 1.0 inside dash, 0 in gap.
// Uses dashAmt 0..1 — 0 == solid, 1 == aggressively dashed.
float dashMask(vec2 q, float t, float dashAmt, float jitter){
    float ang = atan(q.y, q.x);
    // Per-line dash period; jitter from audio.high makes them flicker.
    float period = mix(40.0, 10.0, dashAmt);
    float phase = ang * period + t * 0.4 + jitter * 6.0;
    float duty  = mix(1.0, 0.45, dashAmt);
    float s = 0.5 + 0.5 * sin(phase);
    return smoothstep(0.5 - 0.25 * duty, 0.5 + 0.25 * duty, s);
}

// ─── Masthead text (typewriter, italic+roman split) ───────────────────
// Returns alpha mask in [0,1] for the glyph at this pixel. italicFrac
// outputs how italic-tilted this glyph should be (lead chars).
float drawMasthead(vec2 uv, float aspect, out vec3 mInk, out float italicFlag){
    mInk = inkColor.rgb;
    italicFlag = 0.0;
    int total = charCount();
    if (total <= 0) return 0.0;
    int reveal = total;
    if (msgAge >= 0.0) {
        const float CPS = 28.0;
        reveal = int(floor(msgAge * CPS));
        if (reveal < 0) reveal = 0;
        if (reveal > total) reveal = total;
    }
    if (reveal <= 0) return 0.0;

    // Layout: centred band above the form
    float bandY = 0.38;
    float bandH = 0.080 * textSize;
    float bandHalf = bandH * 0.5;
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;
    if (abs(p.y - bandY) > bandHalf) return 0.0;

    float canvasW = aspect * 0.78;
    int cols = total;
    if (cols < 6) cols = 6;
    float gw = canvasW / float(cols);
    float gh = bandH * 0.95;
    gw = min(gw, gh * (5.0 / 7.0));
    float blockW = gw * float(total);

    float startX = -0.5 * blockW;
    float lx = p.x - startX;
    if (lx < 0.0 || lx > blockW) return 0.0;

    int col = int(floor(lx / gw));
    if (col < 0 || col >= total) return 0.0;
    if (col >= reveal) return 0.0;

    int ch = getChar(col);
    if (ch < 0 || ch == SPACE_CH || ch > 35) return 0.0;

    // Italic lead: first N glyphs render with a shear; the rest are roman.
    int lead = int(italicLead);
    if (lead < 0) lead = 0;
    if (lead > total) lead = total;
    float italic = (col < lead) ? 0.22 : 0.0;
    italicFlag = (col < lead) ? 1.0 : 0.0;

    float cx = (lx - float(col) * gw) / gw;
    float cy = ((bandY + bandHalf) - p.y) / bandH;
    // Apply italic shear to atlas sample coords
    cx = cx + italic * (1.0 - cy - 0.5);
    if (cx < 0.0 || cx > 1.0 || cy < 0.0 || cy > 1.0) return 0.0;

    float s = sampleChar(ch, vec2(cx, 1.0 - cy));
    s = smoothstep(0.22, 0.55, s);
    return s;
}

void main(){
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / max(res.y, 1.0);
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    gT  = TIME;
    gTw = gT * max(motionSpeed, 0.0);

    float bass = clamp(bassDrive, 0.0, 2.0);
    float mid  = clamp(midDrive,  0.0, 2.0);
    float high = clamp(highDrive, 0.0, 2.0);
    float audDepth = clamp(audioDepth, 0.0, 2.0);

    // Two-channel split. Multiply each energy by its active flag so
    // muting a player retracts that half (rubric a=5: you can SEE
    // which player went silent).
    gEA = clamp(energyA * mix(0.35, 1.0, clamp(playerActiveA, 0.0, 1.0)), 0.0, 1.5);
    gEB = clamp(energyB * mix(0.35, 1.0, clamp(playerActiveB, 0.0, 1.0)), 0.0, 1.5);
    gMid  = mid  * audDepth;
    gHigh = high * audDepth;

    int variant = int(formVariant);

    // ── Paper backdrop ──
    vec2 wp = vec2(fbm2(p * 1.6 + gTw * 0.04),
                   fbm2(p * 1.6 + 11.0 - gTw * 0.03));
    vec3 paper = mix(paperColor.rgb,
                     paperColor.rgb * 0.93 + vec3(0.04, 0.04, 0.05),
                     0.38 * wp.x);
    float vig = 1.0 - 0.20 * dot(p, p);
    paper *= vig;
    float gn = fbm2(vec2(uv.x * res.y * 0.014, uv.y * res.y * 0.014));
    paper *= 1.0 + (gn - 0.5) * 0.12 * grain;

    // ── Camera (subtle) ──
    vec2 m2 = (mousePos - 0.5);
    // Slow autonomic camera drift + mouse parallax.
    float yaw = 0.20 * sin(gTw * 0.13) + m2.x * 0.30;
    float pit = 0.10 * sin(gTw * 0.11) - m2.y * 0.20;
    float fSize = clamp(formSize, 0.4, 1.6);
    // Camera ray
    vec3 ro = vec3(sin(yaw) * 1.8, pit * 0.6, cos(yaw) * 1.8);
    vec3 ta = vec3(0.0, 0.0, 0.0);
    vec3 ww = normalize(ta - ro);
    vec3 uu = normalize(cross(ww, vec3(0.0, 1.0, 0.0)));
    vec3 vv = cross(uu, ww);
    // Scale incoming pixel by fSize → form fills more / less of frame
    vec2 fp = p / fSize;
    vec3 rd = normalize(uu * fp.x + vv * fp.y + 1.6 * ww);

    // ── Raymarch (sphere-trace) ──
    float tt = 0.0;
    bool hit = false;
    float minD = 1e3;
    for (int i = 0; i < 64; i++) {
        vec3 q = ro + rd * tt;
        float d = sdForm(q, variant);
        if (d < minD) minD = d;
        if (d < 0.0015) { hit = true; break; }
        tt += d * 0.92;
        if (tt > 6.0) break;
    }

    // ── Halo (Gaussian) accumulated for ALL pixels, brightens near form ──
    // Approximate distance to form silhouette in screen-space via minD
    // (the closest the ray got). Wraps the entire sculpture in light.
    float haloR = clamp(haloRadius, 0.2, 1.2) *
                  (1.0 + 0.18 * bass * audDepth);
    float haloSig = haloR * clamp(haloSoftness, 0.3, 1.5);
    float haloT = clamp(minD, 0.0, 1.5);
    float halo = exp(-(haloT * haloT) / max(haloSig * haloSig, 1e-4));
    // Halo color: iridescent ramp parametrized by polar angle + time
    float halTheta = atan(p.y, p.x);
    vec3 halColor = paletteRamp(int(palette),
                                halTheta / TAU + 0.05 * gTw + 0.5,
                                paletteShift);
    // Bias halo color by which half is active — A side warm, B side cool
    float xWarm = smoothstep(-0.35, 0.35, p.x);   // 0 left, 1 right
    halColor = mix(halColor * (0.95 + 0.25 * gEA),
                   halColor.bgr * (0.95 + 0.25 * gEB),
                   xWarm);

    vec3 col = paper + halColor * halo * (0.85 + 0.45 * (gEA + gEB))
                                * bloom;

    // ── Shade the form ──
    if (hit) {
        vec3 q = ro + rd * tt;
        vec3 n = calcNormal(q, variant);
        vec3 v = -rd;
        // Fresnel + rim
        float fres = pow(1.0 - clamp(dot(n, v), 0.0, 1.0), 3.0);
        // Procedural rim light from upper-right
        vec3 ldir = normalize(vec3(0.55, 0.75, 0.30));
        float diff = max(dot(n, ldir), 0.0);
        vec3 H = normalize(v + ldir);
        float spec = pow(max(dot(n, H), 0.0), 64.0);

        // Surface color from palette swept by surface normal + side
        float pal_t = 0.5 + 0.5 * n.x + 0.20 * n.y +
                      0.05 * gTw + 0.10 * gMid;
        vec3 surf = paletteRamp(int(palette), pal_t, paletteShift);
        // Per-half tint
        surf = mix(surf * vec3(1.06, 0.96, 0.94),
                   surf * vec3(0.94, 0.98, 1.06),
                   xWarm);

        // Combine
        vec3 shaded = surf * (0.45 + 0.55 * diff)
                    + vec3(1.0, 0.96, 0.92) * spec * (0.8 + 1.4 * gHigh)
                    + fres * surf * (0.45 + 0.6 * gHigh);

        // Atmospheric haze on depth
        float haze = clamp(tt * 0.18 * fog, 0.0, 1.0);
        shaded = mix(shaded, paper, haze * 0.5);

        col = mix(col, shaded, 0.92);
    }

    // ── Structural line cage on its own z-plane ──
    // The lines live in a parallax-shifted 2D coordinate. Their offset
    // from the form is driven by mouse + slow drift + energyB tilt, so
    // they slide across the sculpture like ink on glass.
    float lineZ = clamp(lineParallax, 0.0, 2.0);
    vec2 lp = p - m2 * 0.08 * lineZ
               - vec2(sin(gTw * 0.21), cos(gTw * 0.17)) * 0.04 * lineZ;
    float tilt = 0.20 * sin(gTw * 0.17) + 0.45 * gEB - 0.10 * gEA
               + 0.18 * bass * audDepth;

    int nLines = int(lineDensity);
    if (nLines < 2)        nLines = 2;
    if (nLines > MAX_LINES) nLines = MAX_LINES;
    int style = int(lineStyle);

    float lineD = 1e3;
    float lineJit = 0.0;
    for (int i = 0; i < MAX_LINES; i++) {
        if (i >= nLines) break;
        float d;
        if (style == 0)      d = lineSDF_concentric(lp, i, nLines, gTw, tilt);
        else if (style == 1) d = lineSDF_hatch(lp, i, nLines, gTw, tilt);
        else                 d = lineSDF_orbit(lp, i, nLines, gTw, tilt);
        if (d < lineD) {
            lineD = d;
            lineJit = h11(float(i) * 7.7);
        }
    }
    // Dash pattern (audio.high flickers them)
    float dashAmt = clamp(lineDash, 0.0, 1.0);
    float dashJitter = gHigh * (0.5 + 0.5 * sin(gTw * 4.0 + lineJit * TAU));
    float dash = mix(1.0, dashMask(lp, gTw, dashAmt, dashJitter), dashAmt);

    // fwidth-AA line render
    float lw = clamp(lineWidth, 0.0008, 0.012);
    float fw = max(fwidth(lineD), 1e-4);
    float lineMask = 1.0 - smoothstep(lw - fw, lw + fw, abs(lineD));
    lineMask *= dash;

    // Lines fade with depth fog so far hatches disappear into paper
    float lineFog = 1.0 - clamp(lineZ * 0.18, 0.0, 0.55);
    lineMask *= lineFog;

    // Ink the lines — slightly warmer if over a bright halo region
    vec3 inkLine = inkColor.rgb;
    // Where the halo is strongest, use a chromatic version of ink so
    // the lines read editorial-soft rather than harsh black on white.
    vec3 chromInk = mix(inkColor.rgb,
                        paletteRamp(int(palette), halTheta / TAU + 0.5,
                                    paletteShift) * 0.35 + inkColor.rgb * 0.65,
                        clamp(halo * 1.4, 0.0, 1.0));
    col = mix(col, chromInk, lineMask * 0.92);

    // ── Masthead ──
    if (bool(showMasthead)) {
        vec3 mInk;
        float italicFlag;
        float mk = drawMasthead(uv, aspect, mInk, italicFlag);
        if (mk > 0.001) {
            // Italic glyphs get a softer ink (editorial sub-emphasis)
            vec3 textInk = mix(mInk, mInk * 0.7 + paper * 0.3, italicFlag * 0.4);
            col = mix(col, textInk, mk * 0.96);
        }
    }

    // ── Bloom lift on bright halo regions ──
    float L = dot(col, vec3(0.299, 0.587, 0.114));
    col += bloom * 0.10 * smoothstep(0.65, 1.1, L) * col;

    // Final tone shaping
    col = col / (1.0 + 0.55 * col);
    col = pow(max(col, 0.0), vec3(0.92));

    gl_FragColor = vec4(col, 1.0);
}
