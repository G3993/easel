/*{
  "DESCRIPTION": "Images With Text — an editorial photo-album spread. Small procedural image cutouts drift across a warm paper canvas, each tilted and tagged like a darkroom contact sheet; thin diagonal connector lines thread through the cards like the reference's ribbon graph. A live cue.latest message types out as the editorial headline ribbon at upper-right (the 'DURING THIS PERIOD' role). Three image cards each own a player[i].energy channel (active speaker → that card pops forward in z with a saturation bloom and its connector line ignites); audio.high drives the diagonal rib glints; cue.latest is the headline. Real depth: paper z-plane, drifting dust, parallaxed image cards at three z-slices, headline ribbon floating closest, atmospheric haze through it all. Motion: idle drift in stillness, cards pulse-forward on energy, ribbons light on transient highs, headline breathes with bass. Anti-pattern free — no bars, no EKG, no checker, no logo center; the typography IS the cue.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",       "TYPE": "text",  "DEFAULT": "DURING THIS PERIOD PHOTO ALBUMS", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "cardA",     "LABEL": "Card A · Energy",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "cardB",     "LABEL": "Card B · Energy",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "cardC",     "LABEL": "Card C · Active",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].active" },
    { "NAME": "ribGlint",  "LABEL": "Rib · Highs",      "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.9, "BIND": "audio.high" },
    { "NAME": "bassBreath","LABEL": "Headline Breath",  "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.7, "BIND": "audio.bass" },

    { "NAME": "imageCount","LABEL": "Image Count",   "TYPE": "long",  "DEFAULT": 8, "VALUES": [4,5,6,7,8,9,10,12], "LABELS": ["4","5","6","7","8","9","10","12"] },
    { "NAME": "palette",   "LABEL": "Palette",       "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Editorial","Risograph","Mono","Acid"] },
    { "NAME": "motionTempo","LABEL":"Motion Tempo",  "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "audioDepth","LABEL": "Audio Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "layoutVariant","LABEL":"Layout",      "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2], "LABELS": ["Diagonal","Scatter","Column"] },

    { "NAME": "ribbonDensity","LABEL":"Connector Density","TYPE":"float","MIN":0.0,"MAX":1.5,"DEFAULT":0.85 },
    { "NAME": "fogDensity",   "LABEL": "Atmosphere",     "TYPE":"float","MIN":0.0,"MAX":1.5,"DEFAULT":0.55 },
    { "NAME": "grain",        "LABEL": "Paper Grain",    "TYPE":"float","MIN":0.0,"MAX":1.2,"DEFAULT":0.40 },
    { "NAME": "headlineScale","LABEL": "Headline Size",  "TYPE":"float","MIN":0.4,"MAX":2.0,"DEFAULT":1.0 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  IMAGES WITH TEXT  ·  editorial photo-album spread · live headline
//
//  The reference is a contact-sheet collage — small tilted image cards
//  scattered diagonally, thin colored ribbon lines threading between
//  them, a big editorial headline at upper-right ("DURING THIS PERIOD
//  PHOTO ALBUMS"), tiny date/caption marks under each card. We rebuild
//  that *feeling* — not the literal artifacts.
//
//  · Each image card is a procedurally-painted "photo cutout" (sky-with-
//    window, garden-with-figure, paper-with-marks, ocean-strip, etc.)
//    at one of three z-planes. Three cards each have a player[i] channel:
//    that speaker → card scales forward, saturates up, glows soft.
//  · Connector ribbons are thin lines through card centers at editorial
//    angles, color-rotated through the chosen palette; audio.high makes
//    them glint as transient comets sweep along their length.
//  · The headline is cue.latest typed out as the upper-right ribbon
//    (the "DURING THIS PERIOD PHOTO ALBUMS" role). It breathes with bass.
//  · Real z: paper ← dust ← card_back ← card_mid ← card_front ← ribbons
//    ← headline. Atmospheric haze fades far cards into the paper.
// ════════════════════════════════════════════════════════════════════════

#define MAX_CARDS 12
#define MAX_WALK  48
#define SPACE_CH  26

const float TAU = 6.28318530718;
const float PI  = 3.14159265359;

// ── text atlas access (idiom matches text_clusters.fs) ────────────────
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

// ── hash + noise ──────────────────────────────────────────────────────
float h11(float n){ return fract(sin(n*127.1)*43758.5453); }
vec2  h21(float n){ return vec2(h11(n), h11(n+17.31)); }
float h12(vec2 p){
    vec3 q = fract(vec3(p.xyx)*vec3(0.1031,0.1030,0.0973));
    q += dot(q, q.yzx+33.33);
    return fract((q.x+q.y)*q.z);
}
float vnoise(vec2 p){
    vec2 i=floor(p), f=fract(p);
    f=f*f*(3.0-2.0*f);
    float a=h12(i), b=h12(i+vec2(1,0));
    float c=h12(i+vec2(0,1)), d=h12(i+vec2(1,1));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}
float fbm2(vec2 p){
    float v=0.0, a=0.5;
    for (int i=0;i<4;i++){ v+=a*vnoise(p); p=p*2.07+vec2(11.3,5.7); a*=0.5; }
    return v;
}

// ── SDF primitives ────────────────────────────────────────────────────
float sdRoundBox(vec2 p, vec2 b, float r){
    vec2 d = abs(p)-b+r;
    return min(max(d.x,d.y),0.0) + length(max(d,0.0)) - r;
}
float sdSegment(vec2 p, vec2 a, vec2 b){
    vec2 pa = p-a, ba = b-a;
    float h = clamp(dot(pa,ba)/dot(ba,ba), 0.0, 1.0);
    return length(pa - ba*h);
}
mat2 rot2(float a){ float c=cos(a), s=sin(a); return mat2(c,-s,s,c); }

// ── palette ───────────────────────────────────────────────────────────
vec3 palettePick(int sw, float t){
    if (sw == 1) {
        // Risograph: orange / teal / cream
        if (t < 0.33) return vec3(0.95,0.42,0.20);
        if (t < 0.66) return vec3(0.18,0.55,0.62);
        return vec3(0.96,0.92,0.78);
    } else if (sw == 2) {
        // Mono — paper / charcoal / silver
        if (t < 0.33) return vec3(0.92,0.91,0.88);
        if (t < 0.66) return vec3(0.18,0.17,0.18);
        return vec3(0.62,0.62,0.66);
    } else if (sw == 3) {
        // Acid — neon mint / hot red / lemon
        if (t < 0.33) return vec3(0.62,0.98,0.78);
        if (t < 0.66) return vec3(0.98,0.18,0.22);
        return vec3(0.98,0.92,0.20);
    }
    // Editorial (default) — coral / sky / cream / mustard / lilac
    if (t < 0.20) return vec3(0.92,0.42,0.36);
    if (t < 0.40) return vec3(0.42,0.66,0.82);
    if (t < 0.60) return vec3(0.93,0.91,0.85);
    if (t < 0.80) return vec3(0.96,0.78,0.30);
    return vec3(0.68,0.58,0.82);
}

// ── card "image cutout" interior procedural artwork ───────────────────
// kind: which procedural photo. uv in [-1,1] inside the card.
vec3 cardArt(int kind, vec2 uv, int paletteSw){
    // base bg per kind
    if (kind == 0) {
        // sky-and-window: dark frame with bright porthole
        vec3 frame = vec3(0.05,0.06,0.09);
        vec3 sky   = mix(vec3(0.30,0.55,0.78), vec3(0.85,0.92,1.00),
                         smoothstep(-1.0, 1.0, uv.y));
        float port = smoothstep(0.62, 0.55, length(uv*vec2(1.0,1.15)));
        return mix(frame, sky, port);
    } else if (kind == 1) {
        // garden lawn: green field with small dark figure
        vec3 sky   = vec3(0.78,0.82,0.85);
        vec3 grass = mix(vec3(0.32,0.52,0.30), vec3(0.18,0.36,0.20),
                         smoothstep(-0.2, 0.6, uv.y));
        float horizon = smoothstep(0.05, -0.05, uv.y - 0.10);
        vec3 col = mix(sky, grass, horizon);
        float fig = smoothstep(0.03, 0.0,
            length((uv - vec2(0.10, -0.05))*vec2(2.0,1.0)) - 0.04);
        col = mix(col, vec3(0.08,0.10,0.12), fig);
        return col;
    } else if (kind == 2) {
        // night stage with light cone
        vec3 bg = vec3(0.04,0.05,0.07);
        float lights = smoothstep(0.6, 0.0,
            length(uv - vec2(0.0,-0.10)) - 0.2);
        bg += lights*vec3(0.95,0.78,0.45)*0.65;
        float bar = smoothstep(0.02, 0.0,
            abs(fract(uv.x*4.0)-0.5)-0.18);
        bg += bar*vec3(0.95,0.32,0.18)*0.25;
        return bg;
    } else if (kind == 3) {
        // cat-portrait: warm grey with darker oval face
        vec3 bg = vec3(0.78,0.74,0.70);
        float head = smoothstep(0.50, 0.42,
            length(uv*vec2(1.0,1.15) - vec2(0.0,0.05)));
        bg = mix(bg, vec3(0.92,0.90,0.86), head);
        float eye = smoothstep(0.04, 0.0,
            length(uv - vec2(-0.18, 0.12)) - 0.03);
        eye += smoothstep(0.04, 0.0,
            length(uv - vec2( 0.18, 0.12)) - 0.03);
        bg = mix(bg, vec3(0.10,0.08,0.06), eye);
        return bg;
    } else if (kind == 4) {
        // pink-hair strands: vertical wave field
        vec3 bg = vec3(0.85,0.55,0.55);
        float strand = abs(uv.x - 0.35*sin(uv.y*5.0 + uv.x*1.7));
        bg = mix(bg*0.7, bg*1.1, smoothstep(0.0,0.5,strand));
        return bg;
    } else if (kind == 5) {
        // city skyline at dusk: warm sky, dark silhouette
        vec3 sky = mix(vec3(0.95,0.70,0.45), vec3(0.50,0.42,0.62),
                       smoothstep(-1.0, 1.0, uv.y));
        float skyline = smoothstep(0.02, 0.0,
            uv.y + 0.10 - 0.32*abs(sin(uv.x*5.5)))
            * smoothstep(0.0, -0.6, uv.y);
        return mix(sky, vec3(0.06,0.06,0.10), skyline);
    } else if (kind == 6) {
        // ocean strip: water + warm sun reflection
        vec3 water = mix(vec3(0.08,0.20,0.32), vec3(0.95,0.55,0.30),
                         smoothstep(-1.0, 0.6, uv.y + 0.20*sin(uv.x*4.0+TIME)));
        return water;
    } else if (kind == 7) {
        // paper-with-marks: cream with red-circle scribbles
        vec3 bg = vec3(0.96,0.94,0.86);
        float a = smoothstep(0.015, 0.0,
            abs(length(uv - vec2(-0.30, 0.05)) - 0.22));
        float b = smoothstep(0.015, 0.0,
            abs(length(uv - vec2( 0.20,-0.10)) - 0.18));
        bg = mix(bg, vec3(0.92,0.22,0.20), max(a,b)*0.85);
        float mark = smoothstep(0.018, 0.0,
            abs(uv.x - 0.45) + abs(uv.y + 0.45) - 0.20);
        bg = mix(bg, vec3(0.10,0.10,0.12), mark*0.7);
        return bg;
    } else if (kind == 8) {
        // satellite map: warm beige with green park patch
        vec3 bg = vec3(0.86,0.78,0.62);
        float patch = smoothstep(0.30, 0.20,
            length(uv*vec2(1.4,1.0) - vec2(-0.10,0.10)));
        bg = mix(bg, vec3(0.34,0.52,0.30), patch);
        float road = smoothstep(0.025, 0.0,
            abs(uv.y - 0.40*sin(uv.x*2.5))-0.012);
        bg = mix(bg, vec3(0.20,0.20,0.22), road*0.6);
        return bg;
    } else if (kind == 9) {
        // sketch-portrait silhouette
        vec3 bg = vec3(0.18,0.18,0.20);
        float head = smoothstep(0.32, 0.22,
            length(uv*vec2(1.0,1.2) - vec2(0.0,0.15)));
        bg = mix(bg, palettePick(paletteSw, 0.7), head);
        return bg;
    } else if (kind == 10) {
        // architectural arch: cream with dark archway
        vec3 bg = vec3(0.92,0.88,0.78);
        float arch = smoothstep(0.04, 0.0,
            length(uv*vec2(1.5,1.0) - vec2(0.0,-0.20)) - 0.42);
        // cut a rectangular bottom into the arch
        float cut = step(uv.y, -0.20);
        bg = mix(bg, vec3(0.08,0.08,0.10), arch*(1.0-cut*0.5));
        return bg;
    }
    // 11: gradient color-swatch with two stripes
    vec3 bg = mix(palettePick(paletteSw, 0.1),
                  palettePick(paletteSw, 0.7),
                  smoothstep(-1.0, 1.0, uv.x));
    float stripe = smoothstep(0.02, 0.0, abs(uv.y - 0.40));
    bg = mix(bg, vec3(0.98,0.96,0.92), stripe*0.6);
    return bg;
}

// per-card layout: position, half-extents, rotation, z-slice (0..1), kind
void cardLayout(int i, int n, int variant, out vec2 pos, out vec2 half2,
                out float rot, out float zSlice, out int kind){
    float fi = float(i);
    vec2 seed = h21(fi*9.13 + 1.7);

    // Layout variant
    if (variant == 1) {
        // SCATTER — pseudo-random across the canvas
        pos = vec2(seed.x - 0.5, seed.y - 0.5) * vec2(1.4, 1.0);
    } else if (variant == 2) {
        // COLUMN — left-margin column like the reference's left side
        float t = fi / max(float(n-1), 1.0);
        pos = vec2(-0.55 + (seed.x-0.5)*0.18, mix(0.42, -0.42, t));
    } else {
        // DIAGONAL — the reference look: cards march along an upper-left
        // → lower-right diagonal with offset jitter (the photo album line).
        float t = fi / max(float(n-1), 1.0);
        vec2 a = vec2(-0.62,  0.42);
        vec2 b = vec2( 0.32, -0.40);
        pos = mix(a, b, t);
        // jitter perpendicular to the diagonal axis
        vec2 perp = normalize(vec2(-(b.y-a.y), (b.x-a.x)));
        pos += perp * (seed.x - 0.5) * 0.20;
        pos += vec2(0.0, (seed.y - 0.5) * 0.06);
    }

    // Card size — small contact-sheet thumbnails with some variance
    half2 = vec2(mix(0.055, 0.085, seed.x),
                 mix(0.045, 0.075, seed.y));
    // a couple of "feature" cards a bit bigger
    if (i == 0)               half2 *= 1.20;
    if (i == (n/2))           half2 *= 1.10;

    // slight tilt — between ±10° — gives the casual-album feel
    rot = (h11(fi*4.7) - 0.5) * 0.35;

    // z-slice: three planes (back / mid / front). Round-robin by index
    // so we get a clear back/mid/front mix even at low counts.
    int slot = int(mod(fi, 3.0));
    zSlice = (slot == 0) ? 0.18 : ((slot == 1) ? 0.52 : 0.85);

    // kind: cycle through the 12 procedural artworks
    kind = int(mod(fi, 12.0));
}

// ════════════════════════════════════════════════════════════════════════
//  main
// ════════════════════════════════════════════════════════════════════════
void main(){
    vec2 res = RENDERSIZE;
    vec2 uv  = (gl_FragCoord.xy - 0.5*res) / res.y;
    float aspect = res.x / res.y;

    // global time-scaled tempo
    float gT = TIME * 0.6 * motionTempo;

    int cardN = int(imageCount);
    if (cardN > MAX_CARDS) cardN = MAX_CARDS;
    if (cardN < 1) cardN = 1;
    int paletteSw = int(palette);
    int variant   = int(layoutVariant);

    // bound audio channels
    float aA   = clamp(cardA, 0.0, 1.0);
    float aB   = clamp(cardB, 0.0, 1.0);
    float aC   = clamp(cardC, 0.0, 1.0);
    float aHi  = clamp(ribGlint  * audioDepth, 0.0, 2.5);
    float aLo  = clamp(bassBreath* audioDepth, 0.0, 2.5);

    // ── 1. paper backdrop ──────────────────────────────────────────────
    vec2 wp = vec2(fbm2(uv*1.7), fbm2(uv*1.7 + 7.0));
    vec3 paper = mix(vec3(0.94,0.92,0.86), vec3(0.86,0.84,0.80), wp.x*0.6);
    paper *= 1.0 - 0.10 * dot(uv,uv);
    vec3 col = paper;

    // soft drifting dust / motes (next z-plane up from paper)
    float dust = pow(fbm2(uv*4.5 + vec2(gT*0.10, -gT*0.07)), 6.0);
    col += dust * 0.18 * vec3(1.0,0.95,0.85);

    // ── 2. cards, drawn back-to-front (z-sorted by slice) ──────────────
    // We loop three passes over z-slices [back, mid, front] so far cards
    // get hazed and near cards sit clean on top.
    for (int pass = 0; pass < 3; pass++) {
        float passZ = (pass == 0) ? 0.18 : ((pass == 1) ? 0.52 : 0.85);

        for (int i = 0; i < MAX_CARDS; i++) {
            if (i >= cardN) break;
            vec2 pos, half2; float rot; float zSlice; int kind;
            cardLayout(i, cardN, variant, pos, half2, rot, zSlice, kind);
            if (abs(zSlice - passZ) > 0.05) continue;   // only this pass

            // Player-driven forward push: the three named cards get z-bumped
            // and saturation-bloomed when their channel is energetic. Other
            // cards drift at idle z.
            float playerEnergy = 0.0;
            if (i == 0) playerEnergy = aA;
            else if (i == 1) playerEnergy = aB;
            else if (i == 2) playerEnergy = aC;
            // other cards: gentle implicit energy from a low-amplitude
            // pseudo-channel so they still breathe (not idle-loopy).
            float idleE = 0.04 + 0.03*sin(gT*0.7 + float(i)*2.1);

            // forward push & breath
            float pushZ = playerEnergy;
            float zBoost = 1.0 + 0.18 * pushZ;   // 1.0 → 1.18 scale when active
            vec2 cardPos = pos;
            cardPos.x += 0.014*sin(gT*0.4 + float(i)*1.7);
            cardPos.y += 0.012*cos(gT*0.35+ float(i)*2.3);
            // parallax: front cards drift slightly more than back
            cardPos += vec2(0.018*zSlice*sin(gT*0.15 + float(i)),
                            0.010*zSlice*cos(gT*0.18 + float(i)*1.3));

            // local coords (rotated, scaled)
            vec2 q = (uv - cardPos);
            q = rot2(rot + 0.04*pushZ) * q;
            vec2 hh = half2 * zBoost;
            // SDF for the card body (rounded rect)
            float d = sdRoundBox(q, hh, 0.012);
            // smoother edge by fwidth for clean AA at any res
            float fw = max(fwidth(d), 1e-4);
            float fill = 1.0 - smoothstep(-fw, fw, d);
            if (fill < 0.001) continue;

            // interior uv in [-1,1] for the procedural artwork
            vec2 iuv = q / hh;
            vec3 art = cardArt(kind, iuv, paletteSw);

            // Saturation bloom when player active: boost saturation +
            // a subtle warm glow rim.
            float satBoost = mix(0.65, 1.18, playerEnergy);
            float lum = dot(art, vec3(0.299,0.587,0.114));
            art = mix(vec3(lum), art, satBoost);
            // rim glow when active
            float rim = smoothstep(0.0, -0.018, d) - smoothstep(-0.018, -0.03, d);
            art += rim * playerEnergy * vec3(1.0,0.86,0.62) * 0.55;

            // soft drop shadow under the card (cheap offset SDF)
            float ds = sdRoundBox(q + vec2(0.0,-0.018), hh, 0.015);
            float shad = (1.0 - smoothstep(-0.025, 0.025, ds)) * 0.18 *
                         (0.6 + 0.4*zSlice);
            col = mix(col, col*0.55, shad * (1.0 - fill));

            // tiny caption tick under the card (the (5.26)-style date marks):
            // a small dark dash centered below each card. Length scales with
            // card size; alpha tied to fill so it doesn't bleed outside.
            float tickW = hh.x * 0.55;
            float tickD = sdRoundBox(q + vec2(0.0, hh.y + 0.014),
                                     vec2(tickW, 0.0035), 0.002);
            float tickFill = 1.0 - smoothstep(-0.0015, 0.0015, tickD);
            // small dot to the left of the tick (the bullet) — keeps tag-feel
            float dotD = length(q + vec2(tickW + 0.012, hh.y + 0.014)) - 0.004;
            float dotFill = 1.0 - smoothstep(-0.0015, 0.0015, dotD);
            float tagAlpha = max(tickFill, dotFill);
            // caption color picked from palette so it varies card-to-card
            vec3 tagCol = palettePick(paletteSw, fract(float(i)*0.137));
            col = mix(col, mix(vec3(0.10,0.10,0.12), tagCol, 0.35),
                      tagAlpha * 0.85);

            // atmospheric haze on far cards (slice 0 = farther) — they
            // fade slightly toward paper. Front cards are crisp.
            float haze = clamp((1.0 - zSlice) * fogDensity, 0.0, 0.6);
            art = mix(art, paper, haze);

            // composite the card
            col = mix(col, art, fill);
        }
    }

    // ── 3. connector ribbons (editorial diagonal lines) ────────────────
    // For each adjacent card pair we draw a thin colored line linking
    // their centers. A traveling glint (audio.high driven) sweeps along
    // the line as a transient bead — that's the "highs sparkle" channel.
    if (ribbonDensity > 0.001) {
        // We re-evaluate card centers cheaply.
        for (int i = 0; i < MAX_CARDS-1; i++) {
            if (i >= cardN-1) break;
            vec2 p0, p1, h0, h1; float r0,r1, z0,z1; int k0,k1;
            cardLayout(i,   cardN, variant, p0, h0, r0, z0, k0);
            cardLayout(i+1, cardN, variant, p1, h1, r1, z1, k1);
            // drift cards mirror the same wobble we used above (kept simple)
            p0.x += 0.014*sin(gT*0.4 + float(i)*1.7);
            p0.y += 0.012*cos(gT*0.35+ float(i)*2.3);
            p1.x += 0.014*sin(gT*0.4 + float(i+1)*1.7);
            p1.y += 0.012*cos(gT*0.35+ float(i+1)*2.3);

            float d = sdSegment(uv, p0, p1);
            // line thickness — proportional to ribbon density
            float thick = mix(0.0010, 0.0028, ribbonDensity);
            float line = 1.0 - smoothstep(thick, thick+0.0015, d);
            if (line < 0.001) continue;

            // pick a color per ribbon from the palette
            vec3 ribCol = palettePick(paletteSw,
                                      fract(0.13 + float(i)*0.197));

            // traveling glint: t in [0,1] along the segment, audio.high
            // sets its brightness. The sweep cycles slowly.
            vec2 ba = p1 - p0;
            float t = clamp(dot(uv - p0, ba) / max(dot(ba,ba),1e-6), 0.0, 1.0);
            float sweep = fract(gT*0.18 + float(i)*0.27);
            float bead  = exp(-pow((t - sweep)*8.0, 2.0));
            vec3 glint  = mix(ribCol, vec3(1.0,0.97,0.88), 0.7) *
                          bead * (0.4 + 1.6*aHi);

            // ribbons sit BETWEEN mid and front cards (z ≈ 0.7) — they
            // pass under the closest front cards by virtue of being drawn
            // earlier in the pass loop; here we just composite atop col.
            col = mix(col, ribCol + glint, line * 0.88);
        }
    }

    // ── 4. headline ribbon (cue.latest typewriter) ─────────────────────
    // Upper-right block, multi-line. Big editorial sans, typed out by
    // the host (msg_0..47 are advanced by Application's typewriter when
    // bound to cue.latest). Bass adds a subtle breath to the scale.
    int total = charCount();
    if (total > 0) {
        // headline area in the upper-right of the canvas
        // Define a box: top-right anchor with margin from the right edge.
        float scl = clamp(headlineScale, 0.3, 2.5);
        float baseH = 0.058 * scl;
        float baseW = baseH * (5.0/7.0);
        // breathing — bass nudges the scale up to ~6%
        float breath = 1.0 + 0.06*clamp(aLo, 0.0, 1.0);
        baseH *= breath; baseW *= breath;
        float kern = baseW * 1.05;

        // headline box top-left corner (in centered-uv coords, aspect aware)
        vec2 boxTL = vec2(0.18, 0.40);   // a bit right of center, near top
        float boxW = 0.46;               // horizontal extent

        // figure out wrapping: max chars per row from boxW / kern
        int charsPerRow = int(floor(boxW / kern));
        if (charsPerRow < 6)  charsPerRow = 6;
        if (charsPerRow > 12) charsPerRow = 12;

        // Find the (row, col) for this fragment
        // (uv is already in centered Y-up coords; rows grow downward)
        float lx = uv.x - boxTL.x;
        float ly = boxTL.y - uv.y;
        // pre-compute row-walk to ensure ly maps to a valid row
        // line pitch
        float linePitch = baseH * 1.18;
        if (lx >= 0.0 && lx <= boxW && ly >= 0.0) {
            int targetCol = int(floor(lx / kern));
            int targetRow = int(floor(ly / linePitch));
            if (targetCol < charsPerRow && targetRow >= 0 && targetRow < 5) {
                // sub-cell coords (centered glyph in its kern column)
                float colPad = (kern - baseW) * 0.5;
                float xInCol = lx - float(targetCol)*kern - colPad;
                float yInRow = ly - float(targetRow)*linePitch;
                if (xInCol >= 0.0 && xInCol <= baseW &&
                    yInRow >= 0.0 && yInRow <= baseH)
                {
                    // word-wrap walk to figure out which char lands here
                    int cursorR = 0, cursorC = 0, outCh = -1;
                    for (int i = 0; i < MAX_WALK; i++) {
                        if (i >= total) break;
                        if (cursorR > targetRow) break;
                        int ch = getChar(i);
                        if (ch == SPACE_CH) {
                            // look-ahead word length
                            int wlen = 0;
                            for (int j = 1; j < MAX_WALK; j++) {
                                int jj = i + j;
                                if (jj >= total) break;
                                int chj = getChar(jj);
                                if (chj == SPACE_CH || chj < 0 || chj > 36) break;
                                wlen++;
                            }
                            if (cursorC > 0 && cursorC + 1 + wlen > charsPerRow) {
                                cursorR++; cursorC = 0;
                            } else if (cursorC > 0) {
                                if (cursorR == targetRow && cursorC == targetCol) {
                                    outCh = SPACE_CH;
                                }
                                cursorC++;
                            }
                        } else if (ch >= 0 && ch <= 36) {
                            if (cursorR == targetRow && cursorC == targetCol) {
                                outCh = ch;
                            }
                            cursorC++;
                            if (cursorC >= charsPerRow) {
                                cursorR++; cursorC = 0;
                            }
                        }
                    }

                    if (outCh >= 0 && outCh <= 35 && outCh != SPACE_CH) {
                        vec2 cellUV = vec2(xInCol/baseW,
                                           1.0 - yInRow/baseH);
                        float s = sampleChar(outCh, cellUV);
                        s = smoothstep(0.18, 0.55, s);
                        if (s > 0.001) {
                            // headline ink — near-black with a touch of warm
                            vec3 ink = vec3(0.06, 0.06, 0.08);
                            // soft shadow behind the ink for liftoff
                            col = mix(col, col*0.78, s*0.20);
                            col = mix(col, ink, s);
                        }
                    }
                }
            }
        }

        // Small decorative dash to the left of the first headline row,
        // mirroring the reference's em-dash before "DURING THIS PERIOD".
        if (lx >= -0.045 && lx <= -0.012 &&
            abs(ly - baseH*0.55) < 0.004) {
            col = mix(col, vec3(0.08,0.08,0.10), 0.85);
        }
    }

    // ── 5. paper grain (continuous fiber, never a pixel grid) ──────────
    if (grain > 0.001) {
        float tooth = fbm2(uv*res.y*0.018) + 0.5*fbm2(uv*res.y*0.04 + 7.0);
        col *= 1.0 + (tooth - 0.75) * 0.06 * grain;
    }

    // ── 6. vignette + atmospheric depth ────────────────────────────────
    float vig = 1.0 - 0.18 * dot(uv,uv);
    col *= vig;

    // global haze tied to fog (paper bleeds into edges)
    float gHaze = fogDensity * 0.10;
    col = mix(col, paper, gHaze);

    // gentle output curve
    col = col / (1.0 + 0.15*col);
    col = pow(max(col, 0.0), vec3(0.94));

    gl_FragColor = vec4(col, 1.0);
}
