/*{
  "DESCRIPTION": "Meaningful Forms — a poster grid of nine little luminous sculptures, each its own entity with intent. Every cell holds a raymarched aura-orb: a smooth-min cluster of sub-blobs floating inside a tiny depth box, lit with mint/aqua/lilac iridescence and a soft pink core. Each form breathes on its own channel — five forms are driven by player[1..5].energy, one by audio.bass, the rest by transport.beat — so when a voice arrives, a single form swells and lights up while its siblings sit in intentional stillness. cue.latest types a poetic caption under one chosen form (the 'meaningful thing of the moment') and a small editorial masthead floats above the grid. Real depth via per-cell raymarch (orbit camera + parallax + depth-of-field), iridescent thin-film shading, and analog grain on a warm paper backdrop. No bars, no EKG, no logo — abstract sculptures of meaning.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",           "TYPE": "text",  "DEFAULT": "MEANINGFUL THINGS",     "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "energyA",       "LABEL": "Form 1 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",       "LABEL": "Form 2 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",       "LABEL": "Form 3 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "energyD",       "LABEL": "Form 4 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[4].energy" },
    { "NAME": "energyE",       "LABEL": "Form 5 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[5].energy" },
    { "NAME": "activeA",       "LABEL": "Form 1 Active",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].active" },
    { "NAME": "activeB",       "LABEL": "Form 2 Active",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].active" },

    { "NAME": "bassDrive",     "LABEL": "Bass Drive",      "TYPE": "float", "DEFAULT": 0.8, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.bass" },
    { "NAME": "beatPhase",     "LABEL": "Beat Phase",      "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "transport.beat" },

    { "NAME": "formCount",     "LABEL": "Form Count",      "TYPE": "long",  "DEFAULT": 9, "VALUES": [4,6,8,9,12], "LABELS": ["4","6","8","9","12"] },
    { "NAME": "formVariant",   "LABEL": "Form Variant",    "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2], "LABELS": ["Aura sculptures","Folded sheets","Ribbon knots"] },
    { "NAME": "subBlobs",      "LABEL": "Sub-blobs / Form","TYPE": "long",  "DEFAULT": 5, "VALUES": [3,4,5,6,7], "LABELS": ["3","4","5","6","7"] },

    { "NAME": "palette",       "LABEL": "Palette",         "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Aurora (mint/lilac)","Cool tide","Sunset","Mono ink"] },
    { "NAME": "paletteShift",  "LABEL": "Palette Shift",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0 },

    { "NAME": "motionSpeed",   "LABEL": "Motion Speed",    "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "breathe",       "LABEL": "Breathe (idle)",  "TYPE": "float", "DEFAULT": 0.45, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "audioDepth",    "LABEL": "Audio Depth",     "TYPE": "float", "DEFAULT": 0.75, "MIN": 0.0, "MAX": 2.0 },

    { "NAME": "depthAmount",   "LABEL": "Depth / Parallax","TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 2.5 },
    { "NAME": "dof",           "LABEL": "Depth of Field",  "TYPE": "float", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "bloom",         "LABEL": "Aura Bloom",      "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "fog",           "LABEL": "Atmospheric Fog", "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 1.5 },

    { "NAME": "paperColor",    "LABEL": "Paper Color",     "TYPE": "color", "DEFAULT": [0.91, 0.91, 0.89, 1.0] },
    { "NAME": "inkColor",      "LABEL": "Ink Color",       "TYPE": "color", "DEFAULT": [0.05, 0.05, 0.07, 1.0] },
    { "NAME": "ringTint",      "LABEL": "Form Ring",       "TYPE": "color", "DEFAULT": [0.48, 0.95, 0.78, 1.0] },

    { "NAME": "showMasthead",  "LABEL": "Masthead",        "TYPE": "bool",  "DEFAULT": 1.0 },
    { "NAME": "textSize",      "LABEL": "Masthead Size",   "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.4, "MAX": 2.2 },
    { "NAME": "captionUnder",  "LABEL": "Caption Under",   "TYPE": "long",  "DEFAULT": -1, "VALUES": [-1,0,1,2,3,4,5,6,7,8], "LABELS": ["Auto (loudest)","Form 1","Form 2","Form 3","Form 4","Form 5","Form 6","Form 7","Form 8","Form 9"] },
    { "NAME": "grain",         "LABEL": "Paper Grain",     "TYPE": "float", "DEFAULT": 0.32, "MIN": 0.0, "MAX": 1.0 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//   MEANINGFUL FORMS · sculptures of meaning + typewriter caption
//
//   Composition (after the reference poster):
//     • Warm paper backdrop (vignette + low-freq marbled noise + grain).
//     • A small editorial masthead at the top ("MEANINGFUL THINGS" by
//       default, or whatever cue.latest types into msg).
//     • A 3×3 grid (3..12) of small luminous "forms" — each a self-
//       contained raymarched aura-sculpture inside its own tiny depth
//       box. Cell-local raymarch keeps the cost bounded while delivering
//       real 3D per form.
//     • Each form has its own channel: forms 1..5 → player[i].energy,
//       form 6 → audio.bass, the rest fall back to a phase-shifted
//       transport.beat heuristic so the grid stays alive in silence.
//     • One form (auto = loudest player; or pinned via captionUnder)
//       carries a typewriter caption derived from cue.latest. The
//       letters appear in the cue's own pace via msg_len / msgAge so
//       the words feel uttered, not pasted.
//
//   Anti-pattern guard:
//     • No bars, no spectrum line, no logo glyph, no horizon symmetry,
//       no SDF debug grid. The forms are abstract; the texts are typed.
// ════════════════════════════════════════════════════════════════════════

#define MAX_FORMS    12
#define MAX_BLOBS    7
#define MAX_WALK     48
#define SPACE_CH     26
#define TAU_F        6.28318530718

// ─── text helpers (mirror text_clusters.fs) ──────────────────────────
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
    if (n > 48) return 48;
    return n;
}
// Typewriter reveal count — how many glyphs are unveiled right now,
// based on msgAge. msgAge < 0 → live transcript not in use, show all.
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
// Each form has its own hue family; palette = grid theme + per-form shift.
vec3 formPalette(int paletteId, float h) {
    h = fract(h + paletteShift);
    if (paletteId == 1) {
        // cool tide
        return 0.5 + 0.5 * cos(TAU_F * (h + vec3(0.62, 0.50, 0.85)));
    } else if (paletteId == 2) {
        // sunset
        return 0.5 + 0.5 * cos(TAU_F * (h + vec3(0.00, 0.20, 0.55)));
    } else if (paletteId == 3) {
        // mono ink — desaturated, narrow chroma
        vec3 c = spectrum(h * 0.25 + 0.1);
        float lum = dot(c, vec3(0.299, 0.587, 0.114));
        return mix(vec3(lum), c, 0.18);
    }
    // 0 — aurora: mint / aqua / lilac (matches the reference)
    return 0.5 + 0.5 * cos(TAU_F * (h * 0.35 + vec3(0.45, 0.35, 0.78)));
}

// ─── per-form raymarched SDF inside a local depth box ───────────────
// Local p (≈ [-1,1]³), seed = form index → unique blob constellation.
// energy in [0,1] swells radii + adds vital wobble.
float formMap(vec3 p, float seed, float energy, int blobs, int variant, out float matId) {
    float t = TIME * motionSpeed;
    float d = 1e5;
    matId = 0.0;
    // breathing scale — silence reads as stillness, energy as life
    float breath = 0.85
                 + 0.18 * breathe * sin(t * 0.7 + seed * 9.13)
                 + 0.42 * energy;
    if (variant == 1) {
        // folded sheets — two soft slabs smooth-min'd, warped
        float w1 = 0.18 + 0.18 * energy;
        float ang = seed * 1.7 + t * 0.25;
        vec3 q = p;
        q.xy = vec2(q.x * cos(ang) - q.y * sin(ang),
                    q.x * sin(ang) + q.y * cos(ang));
        float n = vnoise3(q * 1.8 + vec3(seed * 3.1, 0.0, t * 0.4));
        float slab = abs(q.z - 0.05 * sin(q.x * 2.3 + t)) - w1 - 0.05 * n;
        float blob = length(q) - (0.62 * breath + 0.10 * n);
        d = smin_k(slab, blob, 0.20);
        matId = 0.5 + 0.5 * n;
    } else if (variant == 2) {
        // ribbon knot — twisted torus that warps with energy
        vec3 q = p;
        float ang = t * 0.6 + seed * 4.0;
        q.xy = vec2(q.x * cos(ang) - q.y * sin(ang),
                    q.x * sin(ang) + q.y * cos(ang));
        float r1 = 0.45 * breath;
        float r2 = 0.10 + 0.08 * energy;
        vec2 c = vec2(length(q.xy) - r1, q.z);
        float n = vnoise3(p * 2.5 + t * 0.5);
        d = length(c) - (r2 + 0.04 * n);
        matId = 0.4 + 0.6 * n;
    } else {
        // 0 — aura sculpture (default): cluster of smooth-min'd blobs
        for (int i = 0; i < MAX_BLOBS; i++) {
            if (i >= blobs) break;
            float fi = float(i);
            vec3 off = (hash31(seed * 31.7 + fi * 5.13) - 0.5) * 0.95;
            // gentle per-blob orbit so the sculpture lives
            off += 0.10 * vec3(
                sin(t * 0.7 + seed + fi * 2.1),
                cos(t * 0.6 + seed * 1.7 + fi * 1.3),
                sin(t * 0.5 + seed * 0.7 + fi * 3.4));
            float r = (0.22 + 0.18 * hash11(seed * 91.3 + fi)) * breath
                    + 0.08 * energy;
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
    }
    return d;
}
vec3 formNormal(vec3 p, float seed, float energy, int blobs, int variant) {
    vec2 e = vec2(0.002, 0.0);
    float dummy;
    return normalize(vec3(
        formMap(p + e.xyy, seed, energy, blobs, variant, dummy)
      - formMap(p - e.xyy, seed, energy, blobs, variant, dummy),
        formMap(p + e.yxy, seed, energy, blobs, variant, dummy)
      - formMap(p - e.yxy, seed, energy, blobs, variant, dummy),
        formMap(p + e.yyx, seed, energy, blobs, variant, dummy)
      - formMap(p - e.yyx, seed, energy, blobs, variant, dummy)));
}

// Channel for a given form index — five forms get player slots, one gets
// audio.bass, the rest fall back to a per-form phase-shifted beat. Stops
// any form from being silent in idle while preserving "this player → that
// form" auditioning.
float formEnergy(int idx) {
    float fi = float(idx);
    float energy;
    if      (idx == 0) energy = energyA;
    else if (idx == 1) energy = energyB;
    else if (idx == 2) energy = energyC;
    else if (idx == 3) energy = energyD;
    else if (idx == 4) energy = energyE;
    else if (idx == 5) energy = clamp(bassDrive, 0.0, 1.0);
    else {
        // beat-pulsed idle for the trailing forms (so silence doesn't kill
        // them; tiny amplitude — they breathe via 'breathe' uniform).
        float phase = fract(beatPhase + fi * 0.137);
        energy = 0.18 * pow(phase, 3.0);
    }
    // gentle boost from activeA/B as "this player is talking right now"
    if (idx == 0) energy = max(energy, 0.65 * clamp(activeA, 0.0, 1.0));
    if (idx == 1) energy = max(energy, 0.65 * clamp(activeB, 0.0, 1.0));
    return clamp(energy * (0.5 + 0.7 * audioDepth), 0.0, 1.4);
}

// Pick the "loudest" form (used as the auto target for the caption).
int loudestForm(int n) {
    int best = 0;
    float bestE = -1.0;
    for (int i = 0; i < MAX_FORMS; i++) {
        if (i >= n) break;
        float e = formEnergy(i);
        if (e > bestE) { bestE = e; best = i; }
    }
    return best;
}

// ─── grid layout ─────────────────────────────────────────────────────
// Compute a roughly-square grid sized to formCount + aspect, then each
// form lives in a cell. Returns cell center + cell half-extent (radius
// of inscribed square / aspect-aware).
void cellOf(int idx, int n, float aspect, out vec2 center, out vec2 halfExt) {
    int cols = int(ceil(sqrt(float(n) * max(aspect, 0.5))));
    if (cols < 1) cols = 1;
    int rows = (n + cols - 1) / cols;
    int cx = idx - (idx / cols) * cols;
    int cy = idx / cols;
    float canvasW = aspect - 0.22;        // small horizontal margin
    float canvasH = 0.78;                  // leave room for masthead + captions
    float cellW = canvasW / float(cols);
    float cellH = canvasH / float(rows);
    center.x = -0.5 * canvasW + (float(cx) + 0.5) * cellW;
    // grid sits a touch below center so masthead has breathing room
    center.y = -0.06 - 0.5 * canvasH + (float(rows - 1 - cy) + 0.5) * cellH;
    halfExt  = 0.42 * vec2(cellW, cellH);  // inscribed orb radius envelope
}

// ─── per-form raymarch ───────────────────────────────────────────────
// Marches the cell-local SDF. Returns rgb + alpha; alpha is the soft
// silhouette so the form composites onto the paper.
vec4 renderForm(vec2 cellLocal, int idx, int blobs, int variant, int paletteId,
                float energy, float seed, out float depthOut) {
    // cellLocal in roughly [-1,1] inside the cell. Camera orbit per-form
    // gives genuine 3D — a tiny window onto the sculpture.
    float t = TIME * motionSpeed;
    // orbit yaw breathes with energy (form leans toward the viewer when alive)
    float yaw = sin(t * 0.22 + seed * 3.1) * 0.6 * depthAmount + energy * 0.35;
    float pit = cos(t * 0.17 + seed * 1.7) * 0.32 * depthAmount;
    vec3 ro = vec3(sin(yaw) * 2.4, pit * 0.9, cos(yaw) * 2.4);
    vec3 ta = vec3(0.0);
    vec3 ww = normalize(ta - ro);
    vec3 uu = normalize(cross(ww, vec3(0.0, 1.0, 0.0)));
    vec3 vv = cross(uu, ww);
    vec3 rd = normalize(cellLocal.x * uu + cellLocal.y * vv + 1.6 * ww);

    float tt = 0.0;
    float mat = 0.0;
    bool hit = false;
    // bounded march — cell-local, cheap
    for (int i = 0; i < 48; i++) {
        vec3 p = ro + rd * tt;
        float d = formMap(p, seed, energy, blobs, variant, mat);
        if (d < 0.004) { hit = true; break; }
        tt += d * 0.9;
        if (tt > 4.5) break;
    }
    depthOut = tt;
    if (!hit) return vec4(0.0);

    vec3 p = ro + rd * tt;
    vec3 n = formNormal(p, seed, energy, blobs, variant);
    vec3 v = normalize(ro - p);
    vec3 r = reflect(-v, n);
    float fres = pow(1.0 - clamp(dot(n, v), 0.0, 1.0), 4.0);

    // form-specific hue family + per-pixel iridescent shift
    float hueBase = seed * 0.61 + 0.13 * mat;
    float thinFilm = 0.18 * sin(fres * 6.0 + seed * 4.0 + t * 0.4);
    vec3 pig  = formPalette(paletteId, hueBase + thinFilm);
    vec3 core = mix(pig, vec3(1.0, 0.86, 0.93), 0.55);   // soft pink centre
    vec3 rim  = mix(formPalette(paletteId, hueBase + 0.18),
                    ringTint.rgb, 0.55);                  // mint/aqua rim

    // procedural studio light: cool sky + warm key
    vec3 lDir = normalize(vec3(0.4, 0.7, 0.3));
    float diff = clamp(dot(n, lDir) * 0.5 + 0.5, 0.0, 1.0);
    float spec = pow(clamp(dot(reflect(-lDir, n), v), 0.0, 1.0), 22.0);

    vec3 surf = mix(core, pig, smoothstep(0.2, 0.9, fres));
    surf = mix(surf * 0.55, surf, diff);
    surf += spec * 0.5 * mix(vec3(1.0), pig, 0.4);
    // thin-film rim flush — what makes it iridescent
    surf += fres * rim * 0.65;
    // energy lifts the core glow + saturates rim
    surf += energy * 0.35 * core;
    surf *= 1.0 + 0.20 * energy;

    // alpha is a soft falloff out from silhouette so each form reads as
    // a glow embedded in paper, not a hard cutout.
    float alpha = 1.0;
    return vec4(surf, alpha);
}

// ─── small typewriter renderer (centered around a point, single line) ─
// Reveal-aware: only the first `revealed` glyphs render.
float typewriterLine(vec2 fp, vec2 center, float scale, int revealed, int startCh, int endCh) {
    // glyph metrics
    float charH = 0.040 * scale;
    float charW = charH * (5.0 / 7.0);
    float kern  = charW * 0.92;
    int total = endCh - startCh;
    if (total <= 0) return 0.0;
    // intrinsic width
    float lineW = float(total) * kern;
    vec2 origin = center - vec2(lineW * 0.5, charH * 0.5);
    vec2 local = fp - origin;
    if (local.y < 0.0 || local.y > charH) return 0.0;
    if (local.x < 0.0 || local.x > lineW) return 0.0;
    int col = int(floor(local.x / kern));
    if (col >= total) return 0.0;
    if (col >= revealed) return 0.0;       // not yet typed
    int ch = getChar(startCh + col);
    if (ch < 0 || ch == SPACE_CH) return 0.0;
    float colPad = (kern - charW) * 0.5;
    // local.y is y-UP world (origin is bottom-left of caption row). The
    // host font atlas stores letter-top at v=1, so direct y-up→v mapping
    // puts letter-top at screen-top. The previous `1.0 -` flipped glyphs
    // upside down.
    vec2 cellUV = vec2((local.x - float(col) * kern - colPad) / charW,
                       local.y / charH);
    float s = sampleChar(ch, cellUV);
    return smoothstep(0.20, 0.55, s);
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 fragUV = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;
    // aspect-corrected centered coords (so circles read as circles)
    vec2 p;
    p.x = (fragUV.x - 0.5) * aspect;
    p.y = fragUV.y - 0.5;

    int n = int(formCount);
    if (n < 1) n = 1;
    if (n > MAX_FORMS) n = MAX_FORMS;
    int blobs = int(subBlobs);
    if (blobs < 1) blobs = 1;
    if (blobs > MAX_BLOBS) blobs = MAX_BLOBS;
    int variant   = int(formVariant);
    int paletteId = int(palette);

    // ── paper backdrop (no logo, no horizon) ─────────────────────
    vec2 marbleP = p * 1.3 + vec2(TIME * 0.04, TIME * -0.03);
    float marble = fbm2(marbleP);
    vec3 paper = mix(paperColor.rgb,
                     paperColor.rgb * vec3(0.97, 0.96, 1.02),
                     marble * 0.45);
    // soft vignette + warm fall-off
    float vig = 1.0 - 0.20 * dot(p, p);
    paper *= vig;
    // gentle warm-cool gradient top→bottom for editorial feel
    paper = mix(paper, paper * vec3(1.02, 1.00, 0.97), smoothstep(-0.5, 0.5, p.y) * 0.5);

    vec3 col = paper;

    // ── grid of forms ────────────────────────────────────────────
    // accumulate aura bloom in screen-space so dark cells get touched too
    float bloomMask = 0.0;
    for (int i = 0; i < MAX_FORMS; i++) {
        if (i >= n) break;
        vec2 center, halfExt;
        cellOf(i, n, aspect, center, halfExt);
        // skip pixels far from this cell — keeps per-pixel cost bounded
        vec2 dxy = p - center;
        // padded test so the bloom halo can still reach outside the orb
        if (abs(dxy.x) > halfExt.x * 1.45) continue;
        if (abs(dxy.y) > halfExt.y * 1.45) continue;
        // cell-local coords in [-1,1] inside the orb's inscribed circle
        float orbR = min(halfExt.x, halfExt.y);
        vec2 cellLocal = dxy / orbR;

        // soft circular falloff (so each form reads as a sphere-like
        // entity inside its cell rather than a square)
        float rad = length(cellLocal);
        float disc = 1.0 - smoothstep(0.92, 1.06, rad);   // hard-ish core
        float halo = 1.0 - smoothstep(0.92, 1.55, rad);   // wider halo

        float seed   = float(i) * 11.71 + 3.5;
        float energy = formEnergy(i);

        // render the sculpture
        if (disc > 0.001) {
            float depthOut;
            vec4 fc = renderForm(cellLocal, i, blobs, variant, paletteId,
                                 energy, seed, depthOut);
            // depth-of-field — forms farther in the scene soften
            float blurAmt = clamp((depthOut - 1.8) * 0.45, 0.0, 1.0) * dof;
            // approximate DoF with a low-freq palette wash
            vec3 wash = formPalette(paletteId, seed * 0.21 + 0.13);
            fc.rgb = mix(fc.rgb, wash, blurAmt * 0.45);
            // fog — far forms fade into paper (atmospheric depth)
            float fogA = 1.0 - exp(-depthOut * 0.18 * fog);
            fc.rgb = mix(fc.rgb, paper, fogA * 0.45);
            // composite with disc falloff so silhouette stays inside the orb
            col = mix(col, fc.rgb, fc.a * disc);
        }

        // soft outer ring (mint outline from the reference) — only on
        // the halo skirt, modulated by energy so live forms glow harder
        float ringDist = abs(rad - 1.00);
        float ring = smoothstep(0.020, 0.000, ringDist);
        vec3 ringCol = mix(ringTint.rgb, formPalette(paletteId, seed * 0.21), 0.45);
        col = mix(col, ringCol, ring * (0.55 + 0.45 * energy) * halo);

        // dotted-circle ticks (like the reference) — short angular dashes
        float ang = atan(cellLocal.y, cellLocal.x);
        float ticks = step(0.5,
            fract(ang * (8.0 / TAU_F) + TIME * 0.05 * (1.0 + energy)));
        float tickBand = smoothstep(0.040, 0.020, abs(rad - 1.035));
        col = mix(col, ringCol * 0.85, ticks * tickBand * 0.35);

        // accumulate screen-space bloom mask weighted by energy
        bloomMask = max(bloomMask, halo * (0.35 + 0.65 * energy));
    }

    // ── aura bloom (additive over paper) ────────────────────────
    if (bloom > 0.001) {
        // soft blur via a few noise-spaced offsets
        float L = dot(col, vec3(0.299, 0.587, 0.114));
        col += bloom * 0.18 * smoothstep(0.45, 1.05, L) * col;
        col += bloom * 0.10 * bloomMask * spectrum(paletteShift + 0.2);
    }

    // ── masthead + caption (text) ────────────────────────────────
    int total = charCount();
    int revealed = revealedCount(total);
    if (showMasthead && total > 0) {
        // top centered masthead — uses the cue text as the headline
        vec2 mastheadCenter = vec2(0.0, 0.40);
        float s = typewriterLine(p, mastheadCenter, textSize, revealed, 0, total);
        if (s > 0.001) {
            col = mix(col, inkColor.rgb, s);
        }
    }
    // caption under one chosen form (auto = loudest, or pinned)
    int capIdx = int(captionUnder);
    if (capIdx < 0) capIdx = loudestForm(n);
    if (capIdx >= 0 && capIdx < n && total > 0) {
        vec2 center, halfExt;
        cellOf(capIdx, n, aspect, center, halfExt);
        // caption sits below the form, in editorial style
        vec2 capCenter = center - vec2(0.0, min(halfExt.x, halfExt.y) + 0.05);
        // smaller text under each form, like the reference labels
        float s = typewriterLine(p, capCenter, 0.55 * textSize,
                                 revealed, 0, total);
        if (s > 0.001) {
            col = mix(col, inkColor.rgb, s * 0.85);
        }
    }

    // ── paper grain (low-freq tooth + high-freq fibre) ───────────
    if (grain > 0.001) {
        float tooth = fbm2(p * res.y * 0.012)
                    + 0.5 * fbm2(p * res.y * 0.030 + 7.0);
        col *= 1.0 + (tooth - 0.75) * 0.06 * grain;
        // analog speckle — very fine, very subtle
        float sp = hash11(dot(gl_FragCoord.xy, vec2(12.9898, 78.233)) + TIME);
        col += (sp - 0.5) * 0.012 * grain;
    }

    // soft global vignette pass
    col *= 1.0 - 0.10 * dot(p * vec2(1.0 / max(aspect, 0.5), 1.0),
                            p * vec2(1.0 / max(aspect, 0.5), 1.0));

    // tonemap (gentle Reinhard) + slight gamma — keeps highlights creamy
    col = col / (1.0 + 0.55 * col);
    col = pow(max(col, 0.0), vec3(0.92));

    gl_FragColor = vec4(col, 1.0);
}
