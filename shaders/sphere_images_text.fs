/*{
  "DESCRIPTION": "Sphere Images Text — a raymarched constellation. Dozens of small textured spheres swarm a central nucleus, each carrying a cutout-image rectangle on its surface; thin filaments connect the cluster like a social-graph star-burst. Three player channels each command one shell (inner / middle / outer): a player's voice swells their shell's spheres, brightens their image-cutouts and tightens their orbit. Bass deepens the dolly. The cue text typewrites into the empty hub at the center — speech as the gravity well the spheres orbit. Real depth (raymarched + depth haze), per-frame energy-aware motion, abstract not literal.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "TYPE": "text", "DEFAULT": "GRAVITY OF THE SPOKEN WORD", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "innerEnergy", "LABEL": "Inner Shell Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "midEnergy",   "LABEL": "Mid Shell Energy",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "outerEnergy", "LABEL": "Outer Shell Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].energy" },

    { "NAME": "audioDepth",  "LABEL": "Bass → Dolly Breath","TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.8, "BIND": "audio.bass" },

    { "NAME": "sphereCount", "LABEL": "Sphere Count",       "TYPE": "long",  "DEFAULT": 24, "VALUES": [12,18,24,32,40], "LABELS": ["12","18","24","32","40"] },
    { "NAME": "sphereSize",  "LABEL": "Sphere Size",        "TYPE": "float", "MIN": 0.25, "MAX": 1.6,  "DEFAULT": 0.85 },
    { "NAME": "surfaceMode", "LABEL": "Surface",            "TYPE": "long",  "DEFAULT": 1, "VALUES": [0,1,2], "LABELS": ["Cutout Grid","Photo Tile","Marbled"] },
    { "NAME": "palette",     "LABEL": "Palette",            "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Paper","Neon","Ember","Glacier"] },
    { "NAME": "motion",      "LABEL": "Motion",             "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "filaments",   "LABEL": "Filaments",          "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.7 },
    { "NAME": "textSize",    "LABEL": "Text Size",          "TYPE": "float", "MIN": 0.5, "MAX": 1.8, "DEFAULT": 1.0 },
    { "NAME": "fog",         "LABEL": "Atmosphere",         "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.9 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  SPHERE IMAGES TEXT  ·  raymarched constellation · 3 player shells
//
//  Composition (essence, not literal social-graph slop):
//    A central nucleus where the typewritten cue lives, surrounded by
//    three concentric shells of small spheres. Each shell is bound to one
//    `player[i].energy` channel — louder = bigger spheres, faster
//    rotation, brighter surface image-cutouts in that shell. The bass
//    channel dollies the camera. Thin filaments connect each sphere back
//    toward the hub, drawn in screen-space after the raymarch so they
//    behave like an x-ray of the social graph rather than literal lines.
//
//  Raymarched (sphere SDFs smooth-min'd into the field) → real depth,
//  perspective foreshortening, atmospheric haze. Surface "images" are
//  procedural cutout rectangles painted in sphere tangent space — they
//  read as small portraits / icons without being literal photographs.
// ════════════════════════════════════════════════════════════════════════

const float TAU = 6.28318530718;
const float PI  = 3.14159265359;

#define MAX_SPHERES 40

// ─── Font atlas (msg_0..msg_47, msg_len, msgAge injected by host) ──────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}
int getChar(int slot) {
    if (slot ==  0) return int(msg_0);   if (slot ==  1) return int(msg_1);
    if (slot ==  2) return int(msg_2);   if (slot ==  3) return int(msg_3);
    if (slot ==  4) return int(msg_4);   if (slot ==  5) return int(msg_5);
    if (slot ==  6) return int(msg_6);   if (slot ==  7) return int(msg_7);
    if (slot ==  8) return int(msg_8);   if (slot ==  9) return int(msg_9);
    if (slot == 10) return int(msg_10);  if (slot == 11) return int(msg_11);
    if (slot == 12) return int(msg_12);  if (slot == 13) return int(msg_13);
    if (slot == 14) return int(msg_14);  if (slot == 15) return int(msg_15);
    if (slot == 16) return int(msg_16);  if (slot == 17) return int(msg_17);
    if (slot == 18) return int(msg_18);  if (slot == 19) return int(msg_19);
    if (slot == 20) return int(msg_20);  if (slot == 21) return int(msg_21);
    if (slot == 22) return int(msg_22);  if (slot == 23) return int(msg_23);
    if (slot == 24) return int(msg_24);  if (slot == 25) return int(msg_25);
    if (slot == 26) return int(msg_26);  if (slot == 27) return int(msg_27);
    if (slot == 28) return int(msg_28);  if (slot == 29) return int(msg_29);
    if (slot == 30) return int(msg_30);  if (slot == 31) return int(msg_31);
    if (slot == 32) return int(msg_32);  if (slot == 33) return int(msg_33);
    if (slot == 34) return int(msg_34);  if (slot == 35) return int(msg_35);
    if (slot == 36) return int(msg_36);  if (slot == 37) return int(msg_37);
    if (slot == 38) return int(msg_38);  if (slot == 39) return int(msg_39);
    if (slot == 40) return int(msg_40);  if (slot == 41) return int(msg_41);
    if (slot == 42) return int(msg_42);  if (slot == 43) return int(msg_43);
    if (slot == 44) return int(msg_44);  if (slot == 45) return int(msg_45);
    if (slot == 46) return int(msg_46);  if (slot == 47) return int(msg_47);
    return -1;
}
int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 0;
    if (n > 48) return 48;
    return n;
}

// ─── Hash / noise ──────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  hash21(float n) { return vec2(hash11(n), hash11(n + 17.31)); }
vec3  hash31(float n) { return vec3(hash11(n), hash11(n + 31.7), hash11(n + 71.3)); }

float vnoise(vec3 p){
    vec3 i=floor(p), f=fract(p);
    f=f*f*(3.0-2.0*f);
    float n000=hash11(dot(i,vec3(1.0,57.0,113.0)));
    float n100=hash11(dot(i+vec3(1,0,0),vec3(1.0,57.0,113.0)));
    float n010=hash11(dot(i+vec3(0,1,0),vec3(1.0,57.0,113.0)));
    float n110=hash11(dot(i+vec3(1,1,0),vec3(1.0,57.0,113.0)));
    float n001=hash11(dot(i+vec3(0,0,1),vec3(1.0,57.0,113.0)));
    float n101=hash11(dot(i+vec3(1,0,1),vec3(1.0,57.0,113.0)));
    float n011=hash11(dot(i+vec3(0,1,1),vec3(1.0,57.0,113.0)));
    float n111=hash11(dot(i+vec3(1,1,1),vec3(1.0,57.0,113.0)));
    return mix(mix(mix(n000,n100,f.x), mix(n010,n110,f.x), f.y),
               mix(mix(n001,n101,f.x), mix(n011,n111,f.x), f.y), f.z);
}
float fbm3(vec3 p){
    float v=0.0, a=0.55;
    for(int i=0;i<4;i++){ v+=a*vnoise(p); p=p*1.97+vec3(11.3,5.7,3.1); a*=0.5; }
    return v;
}
float smin(float a, float b, float k){
    float h=clamp(0.5+0.5*(b-a)/k,0.0,1.0);
    return mix(b,a,h)-k*h*(1.0-h);
}

// ─── Palette ───────────────────────────────────────────────────────────
vec3 paletteColor(int mode, float t) {
    if (mode == 1) {
        // Neon — magenta / cyan / acid lime against indigo
        vec3 a=vec3(0.06,0.04,0.18);
        vec3 b=vec3(0.92,0.18,0.62);
        vec3 c=vec3(0.16,0.86,0.92);
        vec3 d=vec3(0.78,0.96,0.20);
        return mix(mix(a,b,smoothstep(0.0,0.4,t)),
                   mix(c,d,smoothstep(0.5,0.95,t)),
                   smoothstep(0.3,0.7,t));
    } else if (mode == 2) {
        // Ember — black → ember → bone
        return mix(mix(vec3(0.05,0.02,0.03), vec3(0.92,0.34,0.10), smoothstep(0.0,0.55,t)),
                   vec3(0.99,0.92,0.78), smoothstep(0.55,1.0,t));
    } else if (mode == 3) {
        // Glacier — deep teal / steel / pale ice
        return mix(mix(vec3(0.04,0.10,0.16), vec3(0.18,0.45,0.55), smoothstep(0.0,0.5,t)),
                   vec3(0.86,0.95,0.98), smoothstep(0.55,1.0,t));
    }
    // Paper (default) — warm cream / muted graph
    return mix(mix(vec3(0.93,0.91,0.86), vec3(0.62,0.55,0.50), smoothstep(0.0,0.5,t)),
               vec3(0.18,0.16,0.18), smoothstep(0.6,1.0,t));
}

// Globals routed into map() — avoids passing many args.
float gT, gMotion;
float gE1, gE2, gE3;
int   gN;
float gRad;

// Per-sphere descriptor we recompute as needed.
// shell ∈ {0,1,2} → which player channel owns the sphere.
int sphereShell(int i) {
    int third = gN / 3;
    if (i < third) return 0;
    if (i < third * 2) return 1;
    return 2;
}
float shellEnergy(int s) {
    if (s == 0) return gE1;
    if (s == 1) return gE2;
    return gE3;
}
// Sphere center in world space (3D, depth varies → real perspective).
vec3 sphereCenter(int i) {
    float fi = float(i);
    int s = sphereShell(i);
    float baseR = 1.05 + 0.55 * float(s);   // shell radius from hub
    float jitter = hash11(fi * 5.3) * 0.18;
    float energy = shellEnergy(s);
    // Tighter orbit when its owner-player is energetic — louder = closer.
    float r = (baseR + jitter) * (1.0 - 0.18 * energy);

    vec3 axis = normalize(hash31(fi * 1.7) - 0.5);
    float baseSpeed = 0.18 + 0.32 * hash11(fi * 9.1);
    float speed = baseSpeed * gMotion * (1.0 + 1.4 * energy);
    float phase = fi * 1.7 + gT * speed;
    // Build an orbit basis perpendicular to axis.
    vec3 u = normalize(cross(axis, vec3(0.0, 1.0, 0.001)));
    vec3 v = cross(axis, u);
    vec3 orbit = (u * cos(phase) + v * sin(phase)) * r;
    // A little perpendicular wobble that breathes with energy.
    orbit += axis * sin(gT * 0.4 + fi) * 0.20 * (0.4 + energy);
    return orbit;
}
float sphereRadius(int i) {
    float fi = float(i);
    int s = sphereShell(i);
    float energy = shellEnergy(s);
    float base = gRad * mix(0.55, 1.15, hash11(fi * 2.9));
    // Outer shell smaller by default → reads as depth.
    base *= mix(1.0, 0.72, float(s) / 2.0);
    return base * (1.0 + 0.35 * energy);
}

// Scene SDF: smooth-union of every sphere + tiny hub.
float map(vec3 p, out float idF) {
    float d = 1e5;
    idF = 0.0;
    // Tiny soft hub — a felt presence at the center where text lives.
    float hub = length(p) - 0.12;
    d = hub;
    idF = -1.0; // sentinel for hub
    for (int i = 0; i < MAX_SPHERES; i++) {
        if (i >= gN) break;
        vec3 c = sphereCenter(i);
        float r = sphereRadius(i);
        float di = length(p - c) - r;
        float pd = d;
        d = smin(d, di, 0.10);
        // Track which sphere "won" this pixel (by smallest dist before smin).
        if (di < pd) idF = float(i);
    }
    return d;
}
vec3 calcNormal(vec3 p) {
    vec2 e = vec2(0.0015, 0.0);
    float id;
    return normalize(vec3(
        map(p + e.xyy, id) - map(p - e.xyy, id),
        map(p + e.yxy, id) - map(p - e.yxy, id),
        map(p + e.yyx, id) - map(p - e.yyx, id)));
}

// ─── Procedural sphere "image" surface ─────────────────────────────────
// Maps a 3D point on a sphere to (u,v) and paints procedural image-cutout
// rectangles — small framed micro-paintings on the surface. Abstract:
// reads as "the sphere is wearing a photograph" without being literal.
vec3 sphereSurface(vec3 p, vec3 c, int idx, int mode, int paletteId, float energy) {
    vec3 d = normalize(p - c);
    float u = atan(d.z, d.x) / TAU + 0.5;
    float v = asin(clamp(d.y, -1.0, 1.0)) / PI + 0.5;

    float fi = float(idx);
    vec3 base = paletteColor(paletteId, hash11(fi * 3.7));

    if (mode == 0) {
        // Cutout Grid — a tight 3×3 of micro-frames on the sphere face.
        // Each tile is a flat color tinted from a per-tile seed; thin
        // gutters between tiles. Reads as a contact-sheet wrapped on a ball.
        vec2 g = vec2(u, v) * 3.0;
        vec2 gi = floor(g), gf = fract(g);
        vec2 inset = step(0.08, gf) * step(gf, vec2(0.92));
        float frame = inset.x * inset.y;
        float seed = dot(gi, vec2(7.3, 11.7)) + fi;
        vec3 tint = paletteColor(paletteId, hash11(seed));
        // Tile inside-glow energizes with the owning shell.
        float spark = smoothstep(0.4, 0.0, length(gf - 0.5));
        vec3 surf = mix(base * 0.4, tint, frame);
        surf += spark * frame * 0.45 * energy * paletteColor(paletteId, 0.85);
        return surf;
    } else if (mode == 1) {
        // Photo Tile — a single offset rectangle (like a polaroid taped on)
        // with procedural "photo" content inside (fbm landscape) and a
        // light frame margin. The rect rotates slowly so the photo
        // crawls across the sphere — keeps it abstract, not literal.
        float a = fi * 0.7 + gT * 0.10;
        vec2 puv = vec2(u, v) - 0.5;
        puv = mat2(cos(a), -sin(a), sin(a), cos(a)) * puv;
        vec2 frame = step(vec2(-0.28), puv) * step(puv, vec2(0.28));
        float inFrame = frame.x * frame.y;
        // Inner photo region (slight margin so frame reads white).
        vec2 frame2 = step(vec2(-0.22), puv) * step(puv, vec2(0.22));
        float inPhoto = frame2.x * frame2.y;
        // Procedural "photo": fbm horizon + a warm sun.
        vec3 sky = mix(paletteColor(paletteId, 0.85),
                       paletteColor(paletteId, 0.55),
                       smoothstep(-0.2, 0.2, puv.y));
        float n = fbm3(vec3(puv * 5.0, fi));
        sky += vec3(0.10, 0.04, -0.05) * smoothstep(0.4, 0.7, n);
        vec3 surf = base * 0.45;
        surf = mix(surf, vec3(0.97, 0.95, 0.91), inFrame * (1.0 - inPhoto));
        surf = mix(surf, sky, inPhoto);
        // Brighten the photo when its shell-owner is talking.
        surf *= 1.0 + 0.55 * energy * inFrame;
        return surf;
    }
    // Marbled (mode 2) — no frames; a flowing tinted marble that swirls.
    float n = fbm3(vec3(u * 6.0, v * 6.0, fi + gT * 0.18));
    vec3 m1 = paletteColor(paletteId, hash11(fi * 1.7));
    vec3 m2 = paletteColor(paletteId, hash11(fi * 1.7 + 0.4));
    vec3 surf = mix(m1, m2, smoothstep(0.3, 0.7, n));
    surf *= 1.0 + 0.4 * energy;
    return surf;
}

// ─── Screen-space filament from sphere center → hub ────────────────────
// Drawn additively after the raymarch — gives the "social-graph" tracery
// without being literal lines on the ball.
float filamentMask(vec2 px, vec2 a, vec2 b, float thickness) {
    vec2 pa = px - a;
    vec2 ba = b - a;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-6), 0.0, 1.0);
    float d = length(pa - ba * h);
    return smoothstep(thickness, 0.0, d);
}

// World → screen NDC projection helpers (cheap pinhole identical to the
// raymarch camera). We rebuild these in main().
vec3 gRO, gFw, gRt, gUp;
float gFL;
vec2  gAspect;
vec2 worldToScreen(vec3 w) {
    vec3 rel = w - gRO;
    float dz = dot(rel, gFw);
    if (dz < 0.001) dz = 0.001;
    float sx = dot(rel, gRt) / dz * gFL;
    float sy = dot(rel, gUp) / dz * gFL;
    return vec2(sx, sy);
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = (gl_FragCoord.xy - 0.5 * res) / res.y;

    // Globals.
    gT      = TIME;
    gMotion = max(motion, 0.0);
    gE1     = clamp(innerEnergy, 0.0, 1.0);
    gE2     = clamp(midEnergy,   0.0, 1.0);
    gE3     = clamp(outerEnergy, 0.0, 1.0);
    gN      = int(sphereCount);
    if (gN > MAX_SPHERES) gN = MAX_SPHERES;
    gRad    = 0.20 * clamp(sphereSize, 0.25, 1.6);
    int paletteId = int(palette);
    int surfMode  = int(surfaceMode);

    float bass = clamp(audioDepth, 0.0, 2.0);

    // ── Camera (with bass-driven dolly) ────────────────────────────────
    float yaw = sin(gT * 0.12) * 0.35;
    float pit = sin(gT * 0.09) * 0.18;
    float dolly = 4.6 - 0.55 * bass;
    vec3 ro = vec3(sin(yaw) * dolly, pit * 1.4, cos(yaw) * dolly);
    vec3 ta = vec3(0.0);
    vec3 ww = normalize(ta - ro);
    vec3 uu = normalize(cross(ww, vec3(0.0, 1.0, 0.0)));
    vec3 vv = cross(uu, ww);
    float focal = 1.6;
    vec3 rd = normalize(uv.x * uu + uv.y * vv + focal * ww);

    // Cache for filament projection.
    gRO = ro; gFw = ww; gRt = uu; gUp = vv; gFL = focal;

    // ── Background — palette gradient + soft vignette ──────────────────
    vec3 bgA = paletteColor(paletteId, 0.92);
    vec3 bgB = paletteColor(paletteId, 0.08);
    vec3 paper = mix(bgA, bgB, smoothstep(-0.4, 0.6, uv.y));
    // Soft halo behind the hub — pulses with combined shell energy.
    float halo = exp(-length(uv) * 2.6);
    float shellSum = (gE1 + gE2 + gE3) * 0.34;
    paper += halo * paletteColor(paletteId, 0.4) * (0.18 + 0.5 * shellSum);
    paper *= 1.0 - 0.30 * dot(uv, uv);

    // ── Raymarch ──────────────────────────────────────────────────────
    float tt = 0.0;
    float idF = 0.0;
    bool hit = false;
    for (int i = 0; i < 96; i++) {
        vec3 p = ro + rd * tt;
        float d = map(p, idF);
        if (d < 0.003) { hit = true; break; }
        tt += d * 0.85;
        if (tt > 16.0) break;
    }

    vec3 col = paper;
    if (hit) {
        vec3 p = ro + rd * tt;
        vec3 n = calcNormal(p);
        // Resolve sphere index (idF is float; clamp + round; -1 = hub).
        int idx = int(idF + 0.5);
        vec3 surf;
        if (idx < 0) {
            // Hub: very dark, soft.
            surf = paletteColor(paletteId, 0.02) * 0.4;
        } else {
            if (idx >= gN) idx = gN - 1;
            int s = sphereShell(idx);
            float energy = shellEnergy(s);
            vec3 c = sphereCenter(idx);
            surf = sphereSurface(p, c, idx, surfMode, paletteId, energy);
            // Lighting: a key + fill in palette tones, mild fresnel rim.
            vec3 vdir = normalize(ro - p);
            vec3 lkey = normalize(vec3(0.6, 0.9, 0.3));
            float diff = max(dot(n, lkey), 0.0);
            float fres = pow(1.0 - clamp(dot(n, vdir), 0.0, 1.0), 3.0);
            vec3 rimC = paletteColor(paletteId, 0.85);
            surf *= 0.45 + 0.75 * diff;
            surf += fres * rimC * 0.35 * (0.5 + 0.8 * energy);
        }
        // Atmospheric haze (real depth cue).
        float haze = 1.0 - exp(-tt * 0.08 * fog);
        col = mix(surf, paper, haze);
    }

    // ── Screen-space filaments from each sphere to hub ─────────────────
    if (filaments > 0.001) {
        vec2 hub2 = worldToScreen(vec3(0.0));
        vec3 acc = vec3(0.0);
        for (int i = 0; i < MAX_SPHERES; i++) {
            if (i >= gN) break;
            int s = sphereShell(i);
            float energy = shellEnergy(s);
            vec2 sp = worldToScreen(sphereCenter(i));
            float thick = 0.0014 + 0.0030 * energy;
            float m = filamentMask(uv, sp, hub2, thick);
            vec3 fc = paletteColor(paletteId, 0.7 + 0.25 * float(s));
            acc += fc * m * (0.35 + 0.9 * energy);
        }
        col += acc * filaments * 0.45;
    }

    // ── Typewriter text at the hub (revealed by msgAge) ────────────────
    // Live utterance: only show chars whose index <= msgAge*cps.
    int total = charCount();
    if (total > 0) {
        // Text block sits in screen-space hub; its size scales with textSize.
        vec2 hub2 = worldToScreen(vec3(0.0));
        vec2 tp = uv - hub2;
        // Soft circular text well (gives glyphs a quiet floor).
        float well = smoothstep(0.34, 0.20, length(tp));
        col = mix(col, paletteColor(paletteId, 0.02), well * 0.45);

        float boxHalf = 0.26;
        if (abs(tp.x) <= boxHalf && abs(tp.y) <= boxHalf * 0.55) {
            // Auto-fit columns so the message reads in 1-3 lines.
            float ts = clamp(textSize, 0.5, 1.8);
            int charsPerRow = int(ceil(sqrt(float(total) * 2.1) / ts));
            if (charsPerRow < 4)  charsPerRow = 4;
            if (charsPerRow > 24) charsPerRow = 24;
            int rows = (total + charsPerRow - 1) / charsPerRow;
            if (rows < 1) rows = 1;
            if (rows > 4) rows = 4;
            float boxW = boxHalf * 2.0;
            float effKern  = boxW / float(charsPerRow);
            float effCharW = effKern * 0.78;
            float effCharH = effCharW * (7.0 / 5.0);
            float linePitch = effCharH * 1.25;
            float blockH = float(rows) * linePitch;

            float lx = tp.x + boxHalf;
            float ly = (boxHalf * 0.55 - tp.y) - (boxHalf * 1.1 - blockH) * 0.5;
            if (lx >= 0.0 && lx <= boxW && ly >= 0.0 && ly <= blockH) {
                int targetCol = int(floor(lx / effKern));
                int targetRow = int(floor(ly / linePitch));
                if (targetCol < charsPerRow && targetRow < rows) {
                    int slot = targetRow * charsPerRow + targetCol;
                    if (slot < total) {
                        // Typewriter gate — show only revealed chars.
                        bool live = (msgAge >= 0.0);
                        float reveal = live ? msgAge * 28.0 : 1e6;
                        if (float(slot) <= reveal) {
                            int ch = getChar(slot);
                            if (ch >= 0 && ch <= 35 && ch != 26) {
                                float colPad = (effKern - effCharW) * 0.5;
                                float rowPad = (linePitch - effCharH) * 0.5;
                                float yInRow = (ly - float(targetRow) * linePitch) - rowPad;
                                if (yInRow >= 0.0 && yInRow <= effCharH) {
                                    vec2 cellLocal = vec2(
                                        (lx - float(targetCol) * effKern - colPad) / effCharW,
                                        1.0 - yInRow / effCharH);
                                    float s = sampleChar(ch, cellLocal);
                                    s = smoothstep(0.18, 0.55, s);
                                    if (s > 0.001) {
                                        vec3 ink = paletteColor(paletteId, 0.96);
                                        col = mix(col, ink, s);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // Final tonemap-ish soft contrast.
    col = col / (1.0 + 0.35 * col);
    col = pow(max(col, 0.0), vec3(0.92));
    gl_FragColor = vec4(col, 1.0);
}
