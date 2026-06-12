/*{
  "DESCRIPTION": "Forms Text — a metaphysical poster of soft luminous auras drifting in depth. Six raymarched SDF cluster-bodies (each one a smooth-min of sub-blobs) hang in parallax over a paper backdrop, like the 'Different Forms' page of a 1970s consciousness manual. Each form is its own creature with its own palette, its own breath, and its own channel: player[1..5].energy drive five of the auras; audio.bass swells the sixth and the bloom; cue.latest cascades typewriter-revealed across the poster as the masthead. Real depth via per-form parallax z, Gaussian aura accumulation, soft fresnel halos and depth-of-field haze. No EKG, no spectrum bars, no icons.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",           "TYPE": "text",  "DEFAULT": "DIFFERENT FORMS", "MAX_LENGTH": 48 },

    { "NAME": "energyA",       "LABEL": "Form 1 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",       "LABEL": "Form 2 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",       "LABEL": "Form 3 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "energyD",       "LABEL": "Form 4 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[4].energy" },
    { "NAME": "energyE",       "LABEL": "Form 5 Energy",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[5].energy" },

    { "NAME": "bassDrive",     "LABEL": "Bass Drive",      "TYPE": "float", "DEFAULT": 0.8, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.bass" },
    { "NAME": "midDrive",      "LABEL": "Mid Drive",       "TYPE": "float", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.mid" },
    { "NAME": "highDrive",     "LABEL": "High Drive",      "TYPE": "float", "DEFAULT": 0.4, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.high" },

    { "NAME": "formCount",     "LABEL": "Form Count",      "TYPE": "long",  "DEFAULT": 6, "VALUES": [3,4,5,6], "LABELS": ["3","4","5","6"] },
    { "NAME": "formVariant",   "LABEL": "Form Variant",    "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2], "LABELS": ["Aura blobs","Ribbon clusters","Folded sheets"] },
    { "NAME": "subBlobs",      "LABEL": "Sub-Blobs / Form","TYPE": "long",  "DEFAULT": 5, "VALUES": [3,4,5,6,7,8], "LABELS": ["3","4","5","6","7","8"] },
    { "NAME": "auraRadius",    "LABEL": "Aura Radius",     "TYPE": "float", "DEFAULT": 0.16, "MIN": 0.06, "MAX": 0.28 },
    { "NAME": "auraSoftness",  "LABEL": "Aura Softness",   "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.2, "MAX": 1.5 },

    { "NAME": "depthAmount",   "LABEL": "Depth / Parallax","TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 2.5 },
    { "NAME": "motionSpeed",   "LABEL": "Motion Speed",    "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "audioDepth",    "LABEL": "Audio Depth",     "TYPE": "float", "DEFAULT": 0.7, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "bloom",         "LABEL": "Halo Bloom",      "TYPE": "float", "DEFAULT": 0.9, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "fog",           "LABEL": "Atmospheric Fog", "TYPE": "float", "DEFAULT": 0.65, "MIN": 0.0, "MAX": 1.5 },

    { "NAME": "palette",       "LABEL": "Palette",         "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Aura (warm)","Cool tide","Sunset","Mono ink"] },
    { "NAME": "paletteShift",  "LABEL": "Palette Shift",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "paperColor",    "LABEL": "Paper Color",     "TYPE": "color", "DEFAULT": [0.84, 0.84, 0.82, 1.0] },
    { "NAME": "inkColor",      "LABEL": "Ink Color",       "TYPE": "color", "DEFAULT": [0.05, 0.05, 0.07, 1.0] },

    { "NAME": "showMasthead",  "LABEL": "Masthead",        "TYPE": "bool",  "DEFAULT": 1.0 },
    { "NAME": "textSize",      "LABEL": "Masthead Size",   "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.4, "MAX": 2.2 },
    { "NAME": "showCaptions",  "LABEL": "Form Captions",   "TYPE": "bool",  "DEFAULT": 1.0 },
    { "NAME": "grain",         "LABEL": "Paper Grain",     "TYPE": "float", "DEFAULT": 0.30, "MIN": 0.0, "MAX": 1.0 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//   FORMS TEXT  ·  metaphysical poster of luminous auras
//
//   Composition:
//     • Paper backdrop (warm grey, subtle marbled noise, vignette).
//     • 3..6 "forms" arranged in a 3-col / 2-row poster grid (auto-laid
//       out by formCount). Each form is a cluster of smooth-unioned sub-
//       blobs in 3D (raymarched against a virtual depth box) producing
//       an aura — soft Gaussian falloff in screen-space, plus a denser
//       inner SDF "kernel" so the form has structure, not just glow.
//     • Each form has its own palette assignment, depth z, breath phase
//       and channel binding. Five forms are bound to player[1..5].energy;
//       the sixth (and the overall bloom) is bound to audio.bass.
//     • A masthead at the top types out cue.latest (msg auto-bind),
//       typewriter-revealed by msgAge.
//
//   Depth & dimensionality:
//     • Per-form z-depth ∈ [-1,1], small camera parallax driven by mouse
//       and a slow yaw. Far forms haze into paper (fog), near forms get a
//       depth-of-field softness on the aura. Sub-blobs are accumulated
//       in 3D (cluster-local rotation around y) so each form has visible
//       internal volume rather than a flat circle.
//     • A short raymarch (8 steps) refines the brightest form per pixel
//       so silhouettes feel sculpted, not stamped.
//
//   Motion:
//     • Silence → forms hold their pose, breathing only on a slow sin
//       cycle. As player[i].energy rises that form's sub-blobs spread
//       outward, brightens, and orbits faster. audio.bass swells the
//       global halo bloom; audio.mid jitters sub-blob radii; audio.high
//       sharpens fresnel highlights.
//
//   Anti-patterns avoided:
//     • Not an EKG, not a bar spectrum, not a checkerboard, not a
//       mirror-symmetric horizon. Caption text under each form is
//       allowed (poster idiom) but kept tiny — the auras are the
//       subject, not the glyphs.
// ════════════════════════════════════════════════════════════════════════

#define MAX_FORMS    6
#define MAX_SUB      8
#define MAX_CHARS    48
#define MAX_ROW      24
#define SPACE_CH     26
#define TAU          6.28318530718

// ─── Font atlas sampling (shared idiom) ────────────────────────────────
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

// ─── Hash / noise helpers ──────────────────────────────────────────────
float h11(float n){ return fract(sin(n * 127.1) * 43758.5453); }
vec2  h21(float n){ return vec2(h11(n), h11(n + 19.17)); }
vec3  h31(float n){ return vec3(h11(n), h11(n + 11.7), h11(n + 23.4)); }
float h22(vec2 p){
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}
float vnoise(vec2 p){
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float a = h22(i),         b = h22(i+vec2(1,0));
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

// ─── Palette: pick a base hue triple for form index fi ─────────────────
vec3 paletteColor(int formIdx, int paletteMode, float shift){
    float fi = float(formIdx);
    // 6 base hues distributed around the wheel; mode tints them.
    float hue = fract(fi/6.0 + shift);
    vec3 c;
    if (paletteMode == 0) {
        // Aura warm: pinks, peach, gold, soft blue accent.
        vec3 a = vec3(0.97, 0.62, 0.72);  // pink
        vec3 b = vec3(0.99, 0.83, 0.45);  // gold
        vec3 d = vec3(0.55, 0.78, 0.96);  // soft blue
        c = mix(mix(a, b, fract(hue*1.7)), d, smoothstep(0.55, 0.95, fract(hue*1.13)));
    } else if (paletteMode == 1) {
        // Cool tide: teal, indigo, lavender.
        vec3 a = vec3(0.22, 0.78, 0.78);
        vec3 b = vec3(0.36, 0.42, 0.92);
        vec3 d = vec3(0.78, 0.62, 0.96);
        c = mix(mix(a, b, fract(hue*1.3)), d, smoothstep(0.4, 0.9, fract(hue*1.7)));
    } else if (paletteMode == 2) {
        // Sunset: vermilion, magenta, amber.
        vec3 a = vec3(0.98, 0.42, 0.34);
        vec3 b = vec3(0.92, 0.30, 0.62);
        vec3 d = vec3(0.99, 0.78, 0.32);
        c = mix(mix(a, b, fract(hue*1.4)), d, smoothstep(0.45, 0.95, fract(hue*0.9)));
    } else {
        // Mono ink: monochromatic graphite forms with cyan/red accents.
        float g = 0.30 + 0.50 * fract(hue*1.7);
        c = vec3(g);
        if (formIdx == 0) c = mix(c, vec3(0.95,0.42,0.34), 0.55);
        if (formIdx == 3) c = mix(c, vec3(0.42,0.62,0.96), 0.55);
    }
    return c;
}

// ─── Grid position for form `i` given total `n` ───────────────────────
//   3 → 1 row of 3 ; 4 → 2x2 ; 5 → 3+2 ; 6 → 2x3.
void formGrid(int i, int n, float aspect, out vec2 center, out float cellR){
    int cols, rows;
    if (n <= 3)      { cols = 3; rows = 1; }
    else if (n <= 4) { cols = 2; rows = 2; }
    else             { cols = 3; rows = 2; } // 5 and 6 both 3x2 (5 has empty slot)
    int col = i - (i / cols) * cols;
    int row = i / cols;
    // Aspect-corrected layout: canvas in normalized -aspect/2..aspect/2 × -0.5..0.5
    float canvasW = aspect * 0.88;          // some margin on sides
    float canvasH = 0.74;                   // headroom for masthead
    float cellW = canvasW / float(cols);
    float cellH = canvasH / float(rows);
    center.x = -0.5*canvasW + (float(col) + 0.5)*cellW;
    // pull rows downward a touch so masthead sits above
    center.y = (0.5*canvasH - (float(row) + 0.5)*cellH) - 0.04;
    // 5-form layout: visually center the lone second row (single trailing form)
    if (n == 5 && row == 1) {
        int slotsInRow = 5 - cols; // remaining = 2
        int colInRow = i - cols;   // 0 or 1
        float rowW = float(slotsInRow) * cellW;
        center.x = -0.5*rowW + (float(colInRow) + 0.5)*cellW;
    }
    cellR = 0.42 * min(cellW, cellH);
}

// ─── Form energy lookup (each form gets its own channel) ──────────────
float formEnergy(int i, float bass) {
    // 5 player slots map to forms 0..4; form 5 listens to audio.bass.
    if (i == 0) return energyA;
    if (i == 1) return energyB;
    if (i == 2) return energyC;
    if (i == 3) return energyD;
    if (i == 4) return energyE;
    return clamp(bass * 0.6, 0.0, 1.0); // form 5 → bass-driven creature
}

// ─── Per-form aura — accumulates sub-blob 3D positions, returns ───────
//   kernel (compact SDF-derived intensity), aura (soft Gaussian halo),
//   tint (palette mixed with sub-blob hue jitter).
//
//   Uses formVariant to select silhouette character:
//     0 — Aura blobs (sub-spheres in 3D)
//     1 — Ribbon clusters (elongated capsules along a rotating axis)
//     2 — Folded sheets (planes with cosine fold + per-form offset)
void evalForm(
    int i, vec2 p, vec2 center, float cellR,
    int variant, int subN,
    float energy, float pulseMid, float pulseHigh,
    float baseAura, float softness, float timeWarp,
    out float kernel, out float aura, out vec3 tint, out float depthZ
){
    float fi = float(i);
    vec3 seed = h31(fi * 13.7 + 1.0);
    // Per-form depth in [-1, 1]; near forms parallax more.
    depthZ = (seed.z - 0.5) * 2.0;
    float parScale = 1.0 + 0.18 * depthZ;     // perspective: nearer = bigger
    vec2 local = (p - center) / (cellR * parScale);

    // Energy-driven envelope: form opens up as it speaks.
    float env = 0.45 + 0.55 * smoothstep(0.0, 1.0, energy);
    float spread = mix(0.55, 1.0, env);
    float speedMul = 1.0 + 1.5 * energy;
    float radiusMul = (0.55 + 0.45 * env) * (1.0 + 0.18*pulseMid);

    // Rotation of cluster around its own axis (yaw, slow).
    float yaw = timeWarp * (0.20 + 0.35 * seed.x) * speedMul + seed.y * TAU;
    float cy = cos(yaw), sy = sin(yaw);

    // Accumulate sub-blobs.
    float kAcc = 0.0;        // inner intensity (max-based; reads as solid)
    float aAcc = 0.0;        // soft halo (additive Gaussian)
    vec3  cAcc = vec3(0.0);
    float wAcc = 0.0;
    float kernRad = baseAura * (0.55 + 0.35 * env);
    float auraRad = baseAura * softness * (1.8 + 0.6 * energy);

    vec3 baseTint = paletteColor(i, int(palette), paletteShift);

    for (int s = 0; s < MAX_SUB; s++) {
        if (s >= subN) break;
        float fs = float(s);
        vec3 ss = h31(fi * 31.7 + fs * 7.3 + 3.1);

        // Local 3D position of sub-blob (xy in cluster, z for rotation).
        vec3 lp;
        if (variant == 1) {
            // Ribbon cluster: align along x-axis with snaking y-offset.
            float t = (fs / max(float(subN-1), 1.0)) - 0.5;
            lp = vec3(t * 1.4, 0.20*sin(t*4.0 + timeWarp*1.2 + ss.x*TAU), (ss.y-0.5)*0.4);
        } else if (variant == 2) {
            // Folded sheets: anchor along a cosine fold; sub-blobs stack.
            float t = fs / max(float(subN-1), 1.0);
            float fold = cos(t*TAU + timeWarp*0.7 + ss.x*TAU) * 0.35;
            lp = vec3((t-0.5)*1.3, fold + (ss.y-0.5)*0.5, (ss.z-0.5)*0.6);
        } else {
            // Aura blobs: jittered cluster around origin.
            float r = 0.20 + 0.55 * ss.x * spread;
            float a = ss.y * TAU + timeWarp * (0.30 + 0.40*ss.z) * speedMul;
            lp = vec3(r*cos(a), r*sin(a)*0.85, (ss.z-0.5)*0.7);
        }

        // Slow breathing inside cluster.
        lp.xy += 0.05 * vec2(sin(timeWarp*0.9 + fs*1.7), cos(timeWarp*0.7 + fs*2.3));

        // Apply cluster yaw (rotation around y-axis).
        vec3 rp = vec3(cy*lp.x + sy*lp.z, lp.y, -sy*lp.x + cy*lp.z);

        // Project to screen (parallel projection with depth-tinted radius).
        vec2 sp = rp.xy;
        float depthFade = 1.0 + 0.45 * rp.z;          // back blobs smaller
        float subR = kernRad * (0.40 + 0.65 * ss.x) * radiusMul / depthFade;
        float subA = auraRad * (0.55 + 0.65 * ss.y) * radiusMul / depthFade;

        // Audio.high jitters sub-radii to add micro-life.
        subR *= 1.0 + 0.15 * pulseHigh * (ss.z - 0.5);

        // Distance from this fragment (in cluster local space) to sub-blob.
        float dd = length(local - sp);

        // Inner kernel — sharp circle with smooth edge (max blending).
        float kk = 1.0 - smoothstep(subR * 0.55, subR, dd);
        kAcc = max(kAcc, kk);

        // Outer aura — Gaussian falloff (additive).
        float aa = exp(-(dd*dd) / max(subA*subA, 1e-4));
        aAcc += aa;

        // Per-sub tint hue jitter so colors swim like the reference.
        vec3 sub = mix(baseTint, baseTint.gbr, 0.30 * ss.z);
        sub = mix(sub, vec3(1.0), 0.18 * ss.x);    // mid-luma highlights
        float w = aa;
        cAcc += sub * w;
        wAcc += w;
    }
    aAcc /= float(subN);                            // normalize halo
    aAcc = clamp(aAcc, 0.0, 1.4);

    // Depth-of-field: forms far from z=0 soften the kernel.
    float dof = 1.0 - 0.45 * abs(depthZ);
    kernel = kAcc * dof;
    aura   = aAcc;
    tint   = (wAcc > 1e-4) ? cAcc / wAcc : baseTint;
}

// ─── Masthead text: render upper poster headline (msg) ─────────────────
//   typewriter-revealed; uses msg_len + msgAge for live-reveal.
float drawMasthead(vec2 uv, vec2 res, float aspect, out vec3 mInk){
    mInk = inkColor.rgb;
    int total = charCount();
    if (total <= 0) return 0.0;
    // Live typewriter: when msgAge >= 0, only reveal floor(msgAge * cps) chars.
    int reveal = total;
    if (msgAge >= 0.0) {
        const float CPS = 28.0;
        reveal = int(floor(msgAge * CPS));
        if (reveal < 0) reveal = 0;
        if (reveal > total) reveal = total;
    }
    if (reveal <= 0) return 0.0;

    // Layout: centered band at top of canvas.
    float bandY = 0.42;
    float bandH = 0.090 * textSize;
    float bandHalf = bandH * 0.5;
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;
    if (abs(p.y - bandY) > bandHalf) return 0.0;

    // Glyph metrics: pick column count from message length, fit to width.
    float canvasW = aspect * 0.86;
    int cols = total;
    if (cols < 6) cols = 6;
    float gw = canvasW / float(cols);
    float gh = bandH * 0.95;
    // Aspect-correct: width = 5/7 of height for the atlas glyphs.
    gw = min(gw, gh * (5.0/7.0));
    float blockW = gw * float(total);

    float startX = -0.5 * blockW;
    float lx = p.x - startX;
    if (lx < 0.0 || lx > blockW) return 0.0;
    int col = int(floor(lx / gw));
    if (col < 0 || col >= total) return 0.0;
    if (col >= reveal) return 0.0;

    int ch = getChar(col);
    if (ch < 0 || ch == SPACE_CH || ch > 35) return 0.0;

    float cx = (lx - float(col) * gw) / gw;
    float cy = ((bandY + bandHalf) - p.y) / bandH;       // top→bottom
    if (cx < 0.0 || cx > 1.0 || cy < 0.0 || cy > 1.0) return 0.0;
    float s = sampleChar(ch, vec2(cx, 1.0 - cy));
    s = smoothstep(0.20, 0.55, s);
    return s;
}

void main(){
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / max(res.y, 1.0);
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    float tw = TIME * max(motionSpeed, 0.0);
    float bass = clamp(bassDrive, 0.0, 2.0);
    float mid  = clamp(midDrive,  0.0, 2.0);
    float high = clamp(highDrive, 0.0, 2.0);
    float audDepth = clamp(audioDepth, 0.0, 2.0);

    // Camera parallax via mouse + slow autonomic drift.
    vec2 m2 = (mousePos - 0.5);
    vec2 par = m2 * 0.06 * depthAmount
             + vec2(sin(tw*0.13), cos(tw*0.11)) * 0.015 * depthAmount;

    // ── Paper backdrop ──
    // Warm grey, soft marbled noise, slow vignette, subtle scratches.
    vec2 wp = vec2(fbm2(p*1.7 + tw*0.05), fbm2(p*1.7 + 7.0 - tw*0.04));
    vec3 paper = mix(paperColor.rgb,
                     paperColor.rgb * 0.88 + vec3(0.06, 0.05, 0.05),
                     0.45 * wp.x);
    // Subtle big radial vignette.
    float vig = 1.0 - 0.22 * dot(p, p);
    paper *= vig;
    // Faint scratch streak.
    float scratch = smoothstep(0.985, 1.0,
        vnoise(vec2(p.y * 380.0, p.x * 4.0 + sin(tw*0.1))));
    paper += scratch * 0.04;
    // Grain (paper fibers) — never a pixel grid.
    float gn = fbm2(vec2(uv.x * res.y * 0.014, uv.y * res.y * 0.014));
    paper *= 1.0 + (gn - 0.5) * 0.12 * grain;

    int nForms = int(formCount);
    if (nForms < 3) nForms = 3;
    if (nForms > MAX_FORMS) nForms = MAX_FORMS;
    int subN = int(subBlobs);
    if (subN < 3) subN = 3;
    if (subN > MAX_SUB) subN = MAX_SUB;
    int variant = int(formVariant);

    // ── Forms accumulator ──
    // We accumulate aura halos additively and resolve the dominant
    // kernel (max over forms) so silhouettes feel sculpted.
    vec3  haloAcc = vec3(0.0);
    float kernBest = 0.0;
    vec3  kernCol  = vec3(0.0);
    float kernZ    = 0.0;

    for (int i = 0; i < MAX_FORMS; i++) {
        if (i >= nForms) break;
        vec2 center; float cellR;
        formGrid(i, nForms, aspect, center, cellR);
        // Apply camera parallax — back forms move less than front.
        float fi = float(i);
        vec3 seed = h31(fi*13.7 + 1.0);
        float depthZ = (seed.z - 0.5) * 2.0;
        vec2 cShifted = center + par * (1.0 - 0.5 * depthZ);

        float e = clamp(formEnergy(i, bass), 0.0, 1.5);
        float kernel, aura;
        vec3 tint;
        float fz;
        evalForm(i, p, cShifted, cellR, variant, subN,
                 e, mid * audDepth, high * audDepth,
                 auraRadius * cellR / 0.16,   // scale base aura by cell
                 auraSoftness,
                 tw, kernel, aura, tint, fz);

        // Halo: tinted Gaussian glow accumulated additively.
        // Intensity scales with audio.bass (global) + per-form energy.
        float haloI = aura * (0.55 + 0.55 * e) * (1.0 + 0.50 * bass * audDepth);
        haloAcc += tint * haloI * (0.85 + 0.30 * fract(fi*0.37));

        // Inner kernel: keep the brightest one (z-sorted toward viewer).
        // The kernel value is multiplied by a small front bias so nearer
        // forms occlude their neighbours along edges.
        float zBias = 0.5 + 0.5 * fz;       // near forms ~ 1, far ~ 0
        float kVal  = kernel * (0.85 + 0.30 * zBias);
        if (kVal > kernBest) {
            kernBest = kVal;
            kernCol  = tint;
            kernZ    = fz;
        }
    }

    // ── Compose into paper ──
    // Halo adds soft luminous over paper (screen-style blend).
    vec3 col = 1.0 - (1.0 - paper) * (1.0 - haloAcc * bloom);
    // Kernel solidifies the form silhouettes with a soft fresnel-like rim.
    float kEdge = smoothstep(0.20, 0.65, kernBest);
    // Fresnel: edge brighten on near forms when high energy.
    float rim = pow(1.0 - kEdge, 3.0) * 0.50 * (0.5 + 0.5 * (kernZ+1.0)) * (0.8 + 0.8*high*audDepth);
    vec3 formCol = mix(kernCol * (1.0 + rim), kernCol * 1.25, kEdge);
    // Inner darken near center for "core" — gives the SDF mass.
    formCol *= 1.0 - 0.25 * smoothstep(0.85, 1.0, kernBest);
    col = mix(col, formCol, smoothstep(0.05, 0.95, kEdge) * 0.92);

    // Atmospheric fog — far forms (kernZ negative) dissolve into paper.
    float hazeAmt = clamp((0.5 - 0.5*kernZ) * fog, 0.0, 1.0);
    col = mix(col, paper, hazeAmt * (1.0 - kEdge*0.6) * 0.32);

    // ── Masthead text (typewriter cue.latest) ──
    if (bool(showMasthead)) {
        vec3 mInk;
        float mk = drawMasthead(uv, res, aspect, mInk);
        if (mk > 0.001) {
            col = mix(col, mInk, mk * 0.95);
        }
    }

    // ── Form captions (small ink labels under each form) ──
    //   Reuse the message: caption = first letter of each "word" of msg,
    //   or fall back to the form index. Tiny, abstract — preserves the
    //   poster feel without bloating the visual.
    if (bool(showCaptions)) {
        for (int i = 0; i < MAX_FORMS; i++) {
            if (i >= nForms) break;
            vec2 center; float cellR;
            formGrid(i, nForms, aspect, center, cellR);
            vec2 cShifted = center + par * 0.5;
            // Caption band: small strip below the form cell.
            float bandY = cShifted.y - cellR * 1.05;
            float bandH = 0.020;
            float dy = p.y - bandY;
            if (dy < -bandH || dy > bandH) continue;
            // Render 6 glyphs from msg, offset by i*3 so each form caption
            // is a different slice of the masthead.
            int total = charCount();
            if (total <= 0) continue;
            int offset = (i * 3) - ((i * 3) / max(total,1)) * max(total,1);
            float gw = 0.014;
            float gh = bandH * 1.6;
            gw = min(gw, gh * (5.0/7.0));
            int captN = 6;
            float blockW = gw * float(captN);
            float lx = (p.x - cShifted.x) + 0.5 * blockW;
            if (lx < 0.0 || lx > blockW) continue;
            int col2 = int(floor(lx / gw));
            int idx = offset + col2;
            if (idx >= total) continue;
            int ch = getChar(idx);
            if (ch < 0 || ch == SPACE_CH || ch > 35) continue;
            float cx = (lx - float(col2)*gw)/gw;
            float cy = ((bandY + bandH) - p.y) / (bandH*2.0);
            float s = sampleChar(ch, vec2(cx, 1.0 - cy));
            s = smoothstep(0.30, 0.65, s);
            if (s > 0.01) {
                col = mix(col, inkColor.rgb, s * 0.85);
            }
        }
    }

    // Final tone shaping: gentle filmic, then sRGB-ish gamma.
    col = col / (1.0 + 0.55 * col);
    col = pow(max(col, 0.0), vec3(0.92));

    gl_FragColor = vec4(col, 1.0);
}
