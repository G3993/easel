/*{
  "DESCRIPTION": "Meaning — a poster about a single word. The cue text is the hero: an oversized italic-serif headline floats up the page, glyph by glyph, with a wavy hand-drawn underline that breathes. Behind it, three abstract aura-orbs hover at different depths — each bound to its own player[i].energy, each its own colour family — so when a voice arrives one orb swells forward, the others recede into atmosphere. cue.latest types the word, audio.bass kicks the page tone, transport.beat keeps the support shapes alive in silence. Real depth: text z-plane in front, three orb z-planes behind, paper backdrop with fog and a soft vignette. No bars, no spectrum, no logo — abstract supporting language around a single word.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",          "TYPE": "text",  "DEFAULT": "meaning", "MAX_LENGTH": 24, "BIND": "cue.latest" },

    { "NAME": "energyA",      "LABEL": "Orb A Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",      "LABEL": "Orb B Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",      "LABEL": "Orb C Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "activeA",      "LABEL": "Orb A Active",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].active" },
    { "NAME": "activeB",      "LABEL": "Orb B Active",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].active" },

    { "NAME": "bassDrive",    "LABEL": "Bass Drive",     "TYPE": "float", "DEFAULT": 0.8, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.bass" },
    { "NAME": "beatPhase",    "LABEL": "Beat Phase",     "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "transport.beat" },

    { "NAME": "palette",      "LABEL": "Palette",        "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Aurora","Cool tide","Sunset","Mono ink"] },
    { "NAME": "paletteShift", "LABEL": "Palette Shift",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "layoutVariant","LABEL": "Layout",         "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2], "LABELS": ["Triad below","Halo around","Diagonal"] },

    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",   "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "audioDepth",   "LABEL": "Audio Depth",    "TYPE": "float", "DEFAULT": 0.80, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "breathe",      "LABEL": "Idle Breathe",   "TYPE": "float", "DEFAULT": 0.40, "MIN": 0.0, "MAX": 1.5 },

    { "NAME": "textScale",    "LABEL": "Hero Size",      "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.4, "MAX": 2.2 },
    { "NAME": "italicSlant",  "LABEL": "Italic Slant",   "TYPE": "float", "DEFAULT": 0.18, "MIN": 0.0, "MAX": 0.40 },
    { "NAME": "underlineWave","LABEL": "Underline Wave", "TYPE": "float", "DEFAULT": 0.65, "MIN": 0.0, "MAX": 1.5 },

    { "NAME": "fog",          "LABEL": "Atmosphere",     "TYPE": "float", "DEFAULT": 0.65, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "bloom",        "LABEL": "Aura Bloom",     "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "grain",        "LABEL": "Paper Grain",    "TYPE": "float", "DEFAULT": 0.30, "MIN": 0.0, "MAX": 1.0 },

    { "NAME": "paperColor",   "LABEL": "Paper",          "TYPE": "color", "DEFAULT": [0.92, 0.91, 0.89, 1.0] },
    { "NAME": "inkColor",     "LABEL": "Ink",            "TYPE": "color", "DEFAULT": [0.05, 0.05, 0.07, 1.0] }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//   MEANING · single-word hero + three abstract aura-orbs at depth
//
//   Composition (after the reference poster):
//     • Soft paper backdrop, vignetted, with low-freq marbled wash.
//     • Three small aura-orbs hover at THREE DIFFERENT Z-DEPTHS behind
//       the headline — each bound to its own player[i].energy + its own
//       hue family + its own parallax response. Silence: they drift on
//       transport.beat. Voice: the speaking player's orb swells toward
//       the camera while the others recede into atmospheric fog.
//     • The cue.latest text is the HERO — large italic-serif-style
//       glyphs (slanted + scaled per-character) typed in across the
//       page, with a hand-drawn wavy underline that breathes with
//       audio.bass. Empty cue → default "meaning".
//     • Audio.bass lifts the page's warm tint; the underline modulates;
//       transport.beat phases the orb drift so the page is never dead.
//
//   Three z-planes of depth:
//     1. Hero text (front plane, z≈0 — sharp, italic, kerned).
//     2. Aura-orbs (middle plane, raymarched per cell with parallax).
//     3. Paper backdrop (back plane, fbm wash + vignette + fog).
//
//   Anti-pattern guard:
//     • No bars, no spectrum line, no EKG waveform, no logo glyph, no
//       horizon symmetry, no SDF debug grid. The orbs are abstract
//       sculptures; the text is the cue, not decoration.
// ════════════════════════════════════════════════════════════════════════

#define MAX_GLYPHS  24
#define MAX_BLOBS   5
#define SPACE_CH    26
#define TAU_F       6.28318530718

// ─── font / text helpers (mirror text_clusters.fs) ───────────────────
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
    return -1;
}
int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 0;
    if (n > MAX_GLYPHS) return MAX_GLYPHS;
    return n;
}
// Typewriter reveal — msgAge < 0 means manual msg (no live transcript),
// so reveal all glyphs at once. Otherwise count glyphs at ~28 cps.
int revealedCount(int total) {
    if (msgAge < 0.0) return total;
    int rev = int(floor(msgAge * 28.0));
    if (rev < 0) rev = 0;
    if (rev > total) rev = total;
    return rev;
}

// ─── hashes / noise ──────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  hash21(float n) { return vec2(hash11(n), hash11(n + 17.31)); }
vec3  hash31(float n) { return vec3(hash11(n), hash11(n + 11.7), hash11(n + 23.3)); }
float vnoise2(vec2 p) {
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
        v += a * vnoise2(p);
        p = p * 2.07 + vec2(11.3, 5.7);
        a *= 0.5;
    }
    return v;
}
float vnoise3(vec3 p) {
    vec3 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    vec2 uv = i.xy + i.z * vec2(37.0, 17.0) + f.xy;
    float a = hash11(dot(uv, vec2(1.0, 157.0)));
    float b = hash11(dot(uv + vec2(1.0, 0.0), vec2(1.0, 157.0)));
    float c = hash11(dot(uv + vec2(0.0, 1.0), vec2(1.0, 157.0)));
    float d = hash11(dot(uv + vec2(1.0, 1.0), vec2(1.0, 157.0)));
    float lo = mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
    uv += vec2(37.0, 17.0);
    a = hash11(dot(uv, vec2(1.0, 157.0)));
    b = hash11(dot(uv + vec2(1.0, 0.0), vec2(1.0, 157.0)));
    c = hash11(dot(uv + vec2(0.0, 1.0), vec2(1.0, 157.0)));
    d = hash11(dot(uv + vec2(1.0, 1.0), vec2(1.0, 157.0)));
    float hi = mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
    return mix(lo, hi, f.z);
}
float smin_k(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

// ─── palette ─────────────────────────────────────────────────────────
vec3 spectrum(float t) {
    return 0.5 + 0.5 * cos(TAU_F * (t + vec3(0.00, 0.33, 0.67)));
}
vec3 orbPalette(int paletteId, float h) {
    h = fract(h + paletteShift);
    if (paletteId == 1) {
        return 0.5 + 0.5 * cos(TAU_F * (h + vec3(0.62, 0.50, 0.85)));
    } else if (paletteId == 2) {
        return 0.5 + 0.5 * cos(TAU_F * (h + vec3(0.00, 0.20, 0.55)));
    } else if (paletteId == 3) {
        vec3 c = spectrum(h * 0.25 + 0.1);
        float lum = dot(c, vec3(0.299, 0.587, 0.114));
        return mix(vec3(lum), c, 0.18);
    }
    // 0 — aurora (mint / lilac / peach — like the reference)
    return 0.5 + 0.5 * cos(TAU_F * (h * 0.40 + vec3(0.42, 0.32, 0.78)));
}

// ─── per-orb raymarched aura SDF (cell-local) ───────────────────────
float orbMap(vec3 p, float seed, float energy, out float matId) {
    float t = TIME * motionSpeed;
    float d = 1e5;
    matId = 0.0;
    float breath = 0.85
                 + 0.20 * breathe * sin(t * 0.7 + seed * 9.13)
                 + 0.45 * energy;
    for (int i = 0; i < MAX_BLOBS; i++) {
        float fi = float(i);
        vec3 off = (hash31(seed * 31.7 + fi * 5.13) - 0.5) * 0.9;
        off += 0.12 * vec3(
            sin(t * 0.7 + seed + fi * 2.1),
            cos(t * 0.6 + seed * 1.7 + fi * 1.3),
            sin(t * 0.5 + seed * 0.7 + fi * 3.4));
        float r = (0.24 + 0.18 * hash11(seed * 91.3 + fi)) * breath
                + 0.10 * energy;
        float n = vnoise3((p - off) * 2.6 + t * 0.4 + seed);
        float di = length(p - off) - r - 0.04 * (n - 0.5);
        if (i == 0) {
            d = di;
            matId = hash11(seed * 7.1 + fi);
        } else {
            float prev = d;
            d = smin_k(d, di, 0.30);
            matId = mix(hash11(seed * 7.1 + fi), matId,
                        clamp((prev - d) / 0.30 + 0.5, 0.0, 1.0));
        }
    }
    return d;
}
vec3 orbNormal(vec3 p, float seed, float energy) {
    vec2 e = vec2(0.0022, 0.0);
    float dummy;
    return normalize(vec3(
        orbMap(p + e.xyy, seed, energy, dummy) - orbMap(p - e.xyy, seed, energy, dummy),
        orbMap(p + e.yxy, seed, energy, dummy) - orbMap(p - e.yxy, seed, energy, dummy),
        orbMap(p + e.yyx, seed, energy, dummy) - orbMap(p - e.yyx, seed, energy, dummy)));
}

// Per-orb channel selector — three orbs map to player[1..3].energy; in
// silence they drift on transport.beat so the page is never dead.
float orbEnergyOf(int idx) {
    float fi = float(idx);
    float energy;
    if      (idx == 0) energy = energyA;
    else if (idx == 1) energy = energyB;
    else                energy = energyC;
    // active gates — promote whichever player is speaking right now
    if (idx == 0) energy = max(energy, 0.55 * clamp(activeA, 0.0, 1.0));
    if (idx == 1) energy = max(energy, 0.55 * clamp(activeB, 0.0, 1.0));
    // idle beat-pulse so silence still breathes (small amplitude)
    float idle = 0.16 * pow(fract(beatPhase + fi * 0.137), 3.0);
    energy = max(energy, idle);
    return clamp(energy * (0.5 + 0.7 * audioDepth), 0.0, 1.4);
}

// Orb anchor (xy in centred aspect coords) + per-orb z-depth.
// Three layout variants — triad-below, halo-around, diagonal.
// z ∈ [0,1] — 0 = front plane (near hero), 1 = back plane (atmospheric).
void orbAnchor(int idx, int variant, float aspect, out vec2 c, out float zPlane, out float radius) {
    float fi = float(idx);
    if (variant == 1) {
        // halo — three orbs at ~120° around the headline
        float ang = (fi / 3.0) * TAU_F + 0.5;
        float R = 0.46;
        c = vec2(cos(ang), sin(ang) * 0.55) * R;
        zPlane = 0.20 + 0.30 * hash11(fi * 3.17);
        radius = 0.16 + 0.04 * hash11(fi * 7.3);
    } else if (variant == 2) {
        // diagonal — three orbs sweeping bottom-left → top-right
        float t = fi / 2.0;       // 0, 0.5, 1
        c = mix(vec2(-0.55 * aspect, -0.38), vec2(0.55 * aspect, 0.32), t);
        zPlane = 0.10 + 0.40 * t;
        radius = 0.15 + 0.05 * hash11(fi * 5.7);
    } else {
        // 0 — triad below (matches the reference's grid of forms under the headline)
        float xs[3]; xs[0] = -0.46; xs[1] = 0.00; xs[2] = 0.46;
        c = vec2(xs[idx] * 1.10, -0.30 + 0.05 * sin(TIME * 0.4 + fi * 1.7));
        zPlane = 0.10 + 0.32 * hash11(fi * 11.7);
        radius = 0.16 + 0.04 * hash11(fi * 4.1);
    }
}

// Render a single orb inside its local cell (cellLocal ≈ [-1,1]).
// Returns (rgb, silhouette_alpha) plus the marched depth.
vec4 renderOrb(vec2 cellLocal, int idx, int paletteId, float energy, float seed,
               out float depthOut) {
    float t = TIME * motionSpeed;
    float yaw = sin(t * 0.22 + seed * 3.1) * 0.55 + energy * 0.30;
    float pit = cos(t * 0.17 + seed * 1.7) * 0.28;
    vec3 ro = vec3(sin(yaw) * 2.4, pit * 0.9, cos(yaw) * 2.4);
    vec3 ta = vec3(0.0);
    vec3 ww = normalize(ta - ro);
    vec3 uu = normalize(cross(ww, vec3(0.0, 1.0, 0.0)));
    vec3 vv = cross(uu, ww);
    vec3 rd = normalize(cellLocal.x * uu + cellLocal.y * vv + 1.6 * ww);

    float tt = 0.0;
    float mat = 0.0;
    bool hit = false;
    for (int i = 0; i < 48; i++) {
        vec3 p = ro + rd * tt;
        float d = orbMap(p, seed, energy, mat);
        if (d < 0.004) { hit = true; break; }
        tt += d * 0.9;
        if (tt > 4.5) break;
    }
    depthOut = tt;
    if (!hit) return vec4(0.0);

    vec3 p = ro + rd * tt;
    vec3 n = orbNormal(p, seed, energy);
    vec3 v = normalize(ro - p);
    float fres = pow(1.0 - clamp(dot(n, v), 0.0, 1.0), 4.0);

    float hueBase = seed * 0.61 + 0.13 * mat;
    float thinFilm = 0.18 * sin(fres * 6.0 + seed * 4.0 + t * 0.4);
    vec3 pig  = orbPalette(paletteId, hueBase + thinFilm);
    vec3 core = mix(pig, vec3(1.0, 0.86, 0.93), 0.55);

    vec3 lDir = normalize(vec3(0.4, 0.7, 0.3));
    float diff = clamp(dot(n, lDir) * 0.5 + 0.5, 0.0, 1.0);
    float spec = pow(clamp(dot(reflect(-lDir, n), v), 0.0, 1.0), 22.0);

    vec3 surf = mix(core, pig, smoothstep(0.2, 0.9, fres));
    surf = mix(surf * 0.55, surf, diff);
    surf += spec * 0.5 * mix(vec3(1.0), pig, 0.4);
    surf += fres * pig * 0.55;
    surf += energy * 0.35 * core;
    surf *= 1.0 + 0.20 * energy;

    return vec4(surf, 1.0);
}

// ─── hero text layout ───────────────────────────────────────────────
// We render every glyph with its own slanted box, scattered along a
// subtly waved baseline. Reveal-aware: only first `revealed` glyphs paint.
// Returns ink-mask coverage in [0,1] at point p (aspect-corrected, centered).
// boxesYMin / boxesYMax: feed the wavy underline so it follows the text run.
float renderHero(vec2 p, int total, int revealed, float aspect,
                 out float xMin, out float xMax, out float baseY) {
    // figure out the per-glyph metrics → run width → centred origin
    float scale  = textScale;
    float charH  = 0.22 * scale;            // hero height
    float charW  = charH * (5.0 / 7.0);
    float kern   = charW * 1.08;            // generous tracking for serif feel
    float runW   = float(total) * kern;

    // origin centred — text occupies the top third of the page
    vec2 origin;
    origin.x = -runW * 0.5;
    origin.y = 0.18 - charH * 0.5;          // sits above the orb triad

    xMin  = origin.x;
    xMax  = origin.x + runW;
    baseY = origin.y;

    if (total <= 0) return 0.0;

    // per-glyph wave (gentle baseline ripple — italic feel) +
    // per-glyph weight breathe (the most-recent glyph breathes hardest)
    float ink = 0.0;
    for (int g = 0; g < MAX_GLYPHS; g++) {
        if (g >= total) break;
        if (g >= revealed) break;            // typewriter — not yet revealed
        int ch = getChar(g);
        if (ch < 0 || ch > 35 || ch == SPACE_CH) continue;

        float fg = float(g);
        // baseline wave — small vertical perturbation per glyph
        float baselineWave = 0.012 * scale * sin(fg * 0.9 + TIME * 0.6);
        // per-glyph breathe — newest glyphs lift slightly off the baseline
        float age = float(revealed - 1 - g);
        float lift = 0.020 * scale * exp(-age * 0.8);

        vec2 glyphOrigin = origin + vec2(fg * kern, baselineWave + lift);
        // local coords inside this glyph box
        vec2 local = p - glyphOrigin;
        // italic slant — shear x by y (top of glyph leans right)
        float slant = italicSlant * (local.y / charH);
        local.x -= slant * charH;
        // clip to box
        if (local.x < 0.0 || local.x > charW) continue;
        if (local.y < 0.0 || local.y > charH) continue;
        // local.y is y-UP (glyphOrigin at bottom-left). Host font atlas
        // stores letter-top at v=1, so direct y-up→v mapping puts
        // letter-top at screen-top. The previous `1.0 -` flipped glyphs
        // upside down.
        vec2 cellUV = vec2(local.x / charW, local.y / charH);
        float s = sampleChar(ch, cellUV);
        s = smoothstep(0.20, 0.55, s);
        // newest glyph weight-bias — slightly bolder while it's freshest
        s *= 1.0 + 0.18 * exp(-age * 1.2);
        ink = max(ink, s);
    }
    return ink;
}

// Wavy hand-drawn underline that tracks the headline run.
// xMin/xMax bound it; baseY is the glyph baseline. The wave amplitude
// modulates with audio.bass × underlineWave so the line breathes audibly.
float renderUnderline(vec2 p, float xMin, float xMax, float baseY) {
    if (xMax <= xMin) return 0.0;
    // sits just below the glyphs
    float yLine = baseY - 0.035 * textScale;
    // horizontal extent — clip to a small lateral pad so the line tucks
    // under the word like the reference's pen-stroke underscore
    float pad = 0.02;
    if (p.x < xMin - pad || p.x > xMax + pad) return 0.0;
    // wavy displacement — low-freq sine + small noise wobble, driven
    // by audio.bass × underlineWave so the line breathes audibly.
    float t = TIME * 0.8;
    float amp = 0.010 + 0.014 * underlineWave * clamp(bassDrive, 0.0, 1.5);
    float wave = amp * sin((p.x - xMin) * 9.0 + t)
               + 0.6 * amp * sin((p.x - xMin) * 23.0 - t * 1.7);
    // also dotted in places — emulate the reference's broken underline
    float dotPhase = fract((p.x - xMin) * 14.0 + TIME * 0.2);
    float dotMask  = smoothstep(0.0, 0.15, dotPhase) - smoothstep(0.45, 0.55, dotPhase);
    float dy = abs(p.y - (yLine + wave));
    float lw = 0.0035;
    float line = smoothstep(lw * 1.2, lw * 0.5, dy);
    return line * (0.55 + 0.45 * dotMask);
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 fragUV = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;
    vec2 p;
    p.x = (fragUV.x - 0.5) * aspect;
    p.y = fragUV.y - 0.5;

    int paletteId = int(palette);
    int variant   = int(layoutVariant);

    // ── paper backdrop (no logo, no horizon) ────────────────────
    vec2 marbleP = p * 1.3 + vec2(TIME * 0.04, TIME * -0.03);
    float marble = fbm2(marbleP);
    vec3 paper = mix(paperColor.rgb,
                     paperColor.rgb * vec3(0.97, 0.96, 1.02),
                     marble * 0.40);
    // soft vignette
    float vig = 1.0 - 0.20 * dot(p, p);
    paper *= vig;
    // bass-driven warm tint — when the room is loud, the page warms
    float warm = clamp(bassDrive, 0.0, 1.5);
    paper = mix(paper, paper * vec3(1.04, 1.00, 0.96), warm * 0.18);

    vec3 col = paper;
    float bloomMask = 0.0;

    // ── orbs (three z-planes) ───────────────────────────────────
    // Iterate fixed N=3, depth-sort by zPlane (back-to-front composite).
    // For only three orbs we can sort with three comparisons.
    int idxOrder[3];
    float zPlanes[3];
    vec2  centers[3];
    float radii[3];
    for (int i = 0; i < 3; i++) {
        vec2 cc; float zp; float rr;
        orbAnchor(i, variant, aspect, cc, zp, rr);
        idxOrder[i] = i;
        zPlanes[i]  = zp;
        centers[i]  = cc;
        radii[i]    = rr;
    }
    // bubble sort by descending zPlane (back first → drawn first)
    for (int a = 0; a < 3; a++) {
        for (int b = a + 1; b < 3; b++) {
            if (zPlanes[idxOrder[b]] > zPlanes[idxOrder[a]]) {
                int tmp = idxOrder[a]; idxOrder[a] = idxOrder[b]; idxOrder[b] = tmp;
            }
        }
    }
    for (int k = 0; k < 3; k++) {
        int i = idxOrder[k];
        float energy = orbEnergyOf(i);
        // energy pushes orb forward → reduce its zPlane (parallax kick)
        float zEff   = clamp(zPlanes[i] - 0.25 * energy, 0.0, 1.0);
        // parallax offset — back orbs drift less, front orbs more
        vec2 par = (vec2(sin(TIME * 0.16 + float(i) * 2.1),
                         cos(TIME * 0.13 + float(i) * 1.7))
                   * 0.030 * (1.0 - zEff));
        vec2 anchor = centers[i] + par;
        float radius = radii[i] * (1.0 + 0.18 * energy);
        // skip pixels far from this orb
        vec2 dxy = p - anchor;
        if (abs(dxy.x) > radius * 2.2) continue;
        if (abs(dxy.y) > radius * 2.2) continue;
        // cell-local coords inside the orb's inscribed circle
        vec2 cellLocal = dxy / radius;

        float rad = length(cellLocal);
        float disc = 1.0 - smoothstep(0.92, 1.06, rad);
        float halo = 1.0 - smoothstep(0.92, 1.55, rad);

        float seed = float(i) * 11.71 + 3.5;
        if (disc > 0.001) {
            float depthOut;
            vec4 fc = renderOrb(cellLocal, i, paletteId, energy, seed, depthOut);
            // z-plane fog: back orbs fade into paper; front orbs stay crisp
            float fogA = mix(0.08, 0.65, zEff) * fog;
            fc.rgb = mix(fc.rgb, paper, fogA);
            // soft DoF wash for back-plane orbs (mute saturation)
            vec3 wash = orbPalette(paletteId, seed * 0.21 + 0.13);
            fc.rgb = mix(fc.rgb, wash, zEff * 0.30);
            col = mix(col, fc.rgb, fc.a * disc);
        }
        // soft mint outline like the reference's dashed ring
        float ringDist = abs(rad - 1.00);
        float ring = smoothstep(0.018, 0.000, ringDist);
        vec3  ringCol = orbPalette(paletteId, seed * 0.21);
        col = mix(col, ringCol, ring * (0.45 + 0.45 * energy) * halo * (1.0 - 0.5 * zEff));
        // dotted-circle ticks
        float ang = atan(cellLocal.y, cellLocal.x);
        float ticks = step(0.5,
            fract(ang * (8.0 / TAU_F) + TIME * 0.05 * (1.0 + energy)));
        float tickBand = smoothstep(0.038, 0.018, abs(rad - 1.035));
        col = mix(col, ringCol * 0.80, ticks * tickBand * 0.30 * (1.0 - 0.4 * zEff));

        bloomMask = max(bloomMask, halo * (0.30 + 0.70 * energy) * (1.0 - 0.4 * zEff));
    }

    // ── soft aura bloom over the orb layer ─────────────────────
    if (bloom > 0.001) {
        float L = dot(col, vec3(0.299, 0.587, 0.114));
        col += bloom * 0.16 * smoothstep(0.45, 1.05, L) * col;
        col += bloom * 0.10 * bloomMask * spectrum(paletteShift + 0.2);
    }

    // ── hero headline (front plane, italic, kerned, breathing) ─
    int total    = charCount();
    int revealed = revealedCount(total);
    if (total <= 0) {
        // fallback default — still typeset the word "meaning" so the page
        // isn't empty when no cue has fired yet. SPACE_CH==26 reserved.
        // (Atlas indices: a..z = 0..25). We render "meaning" = m,e,a,n,i,n,g.
        int fallback[7]; fallback[0]=12; fallback[1]=4; fallback[2]=0;
        fallback[3]=13; fallback[4]=8; fallback[5]=13; fallback[6]=6;
        // mimic a typed run of 7
        // We'll synthesise ink directly via a tiny inline loop that mirrors
        // renderHero but reads from `fallback` instead of msg slots.
        float scale  = textScale;
        float charH  = 0.22 * scale;
        float charW  = charH * (5.0 / 7.0);
        float kern   = charW * 1.08;
        float runW   = 7.0 * kern;
        vec2 origin  = vec2(-runW * 0.5, 0.18 - charH * 0.5);
        float ink = 0.0;
        for (int g = 0; g < 7; g++) {
            float fg = float(g);
            float baselineWave = 0.012 * scale * sin(fg * 0.9 + TIME * 0.6);
            vec2 glyphOrigin = origin + vec2(fg * kern, baselineWave);
            vec2 local = p - glyphOrigin;
            float slant = italicSlant * (local.y / charH);
            local.x -= slant * charH;
            if (local.x < 0.0 || local.x > charW) continue;
            if (local.y < 0.0 || local.y > charH) continue;
            vec2 cellUV = vec2(local.x / charW, 1.0 - local.y / charH);
            float s = sampleChar(fallback[g], cellUV);
            s = smoothstep(0.20, 0.55, s);
            ink = max(ink, s);
        }
        col = mix(col, inkColor.rgb, ink);
        // dotted wavy underline for the fallback word too
        float ul = renderUnderline(p, origin.x, origin.x + runW, origin.y);
        col = mix(col, inkColor.rgb, ul * 0.85);
    } else {
        float xMin, xMax, baseY;
        float ink = renderHero(p, total, revealed, aspect, xMin, xMax, baseY);
        // ink rides above the orb layer — pure front plane
        col = mix(col, inkColor.rgb, ink);
        // wavy underline beneath the run, scaled to revealed chars
        if (revealed > 0) {
            // shrink xMax to where the revealed glyphs ended
            float scale = textScale;
            float charH = 0.22 * scale;
            float charW = charH * (5.0 / 7.0);
            float kern  = charW * 1.08;
            float endX  = xMin + float(revealed) * kern;
            float ul = renderUnderline(p, xMin, endX, baseY);
            col = mix(col, inkColor.rgb, ul * 0.85);
        }
    }

    // ── paper grain (low-freq tooth + high-freq fibre) ─────────
    if (grain > 0.001) {
        float tooth = fbm2(p * res.y * 0.012)
                    + 0.5 * fbm2(p * res.y * 0.030 + 7.0);
        col *= 1.0 + (tooth - 0.75) * 0.06 * grain;
        float sp = hash11(dot(gl_FragCoord.xy, vec2(12.9898, 78.233)) + TIME);
        col += (sp - 0.5) * 0.012 * grain;
    }

    // soft global vignette
    col *= 1.0 - 0.10 * dot(p * vec2(1.0 / max(aspect, 0.5), 1.0),
                            p * vec2(1.0 / max(aspect, 0.5), 1.0));

    // tonemap + slight gamma
    col = col / (1.0 + 0.55 * col);
    col = pow(max(col, 0.0), vec3(0.92));

    gl_FragColor = vec4(col, 1.0);
}
