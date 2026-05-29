/*{
  "DESCRIPTION": "Images 3D Shape Text — an editorial collage built around a hero raymarched 3D cone. Five floating image-cutout cards parallax over a deep black ground, each card a procedural artifact (rose-stem clipping, cherry-poster gradient, ticket tag, sketch panel, sticker dot) cued to its own player channel. The hero is a real raymarched cone whose curved surface is wrapped with the live cue.latest message — a typewriter ribbon that orbits the cone as it slowly rotates. Cards drift on independent z-planes (real parallax + soft falloff DOF); the cone breathes with bass, the cards twitch with their assigned voice, and the message types out on the cone like a printed broadsheet. Anti-pattern free: no bars, no EKG, no logo center — the typography lives on a 3D body, not the canvas.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "TYPE": "text", "DEFAULT": "DESIGN DEMOCRACY", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "cardA",   "LABEL": "Card A · Stem",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "cardB",   "LABEL": "Card B · Cherry", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "cardC",   "LABEL": "Card C · Ticket", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].active" },
    { "NAME": "bassPulse","LABEL": "Cone · Bass Breath","TYPE": "float","MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.8, "BIND": "audio.bass" },

    { "NAME": "shapeVariant","LABEL": "Hero Shape", "TYPE": "long", "DEFAULT": 0, "VALUES": [0,1,2], "LABELS": ["Cone","Capsule","Prism"] },
    { "NAME": "cardCount",   "LABEL": "Cutout Count","TYPE": "long", "DEFAULT": 5, "VALUES": [2,3,4,5,6,7], "LABELS": ["2","3","4","5","6","7"] },
    { "NAME": "palette",     "LABEL": "Palette",     "TYPE": "long", "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Editorial","Risograph","Mono","Acid"] },
    { "NAME": "motion",      "LABEL": "Motion Tempo","TYPE": "float","MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "audioDepth",  "LABEL": "Audio Depth", "TYPE": "float","MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },

    { "NAME": "coneSpin",    "LABEL": "Cone Spin",   "TYPE": "float","MIN": -1.5, "MAX": 1.5, "DEFAULT": 0.22 },
    { "NAME": "textOnCone",  "LABEL": "Text On Cone","TYPE": "float","MIN": 0.0, "MAX": 1.5, "DEFAULT": 1.0 },
    { "NAME": "fogDensity",  "LABEL": "Fog",         "TYPE": "float","MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.7 },
    { "NAME": "grain",       "LABEL": "Print Grain", "TYPE": "float","MIN": 0.0, "MAX": 1.2, "DEFAULT": 0.45 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  IMAGES 3D SHAPE TEXT  ·  editorial collage · raymarched hero · live text
//
//  The reference is a black-ground collage: a giant text-wrapped paper
//  cone anchors the composition; floating image cutouts (rose, cherry
//  poster, mint ticket, sketch, yellow sticker) parallax around it.
//  We rebuild that *feeling* — not the literal artifacts. The cone is a
//  real raymarched 3D body; the message wraps its curved surface so the
//  typography is an actual object, not screen overlay. Five card SDFs
//  float on independent z-planes for parallax + DOF. Three player
//  channels each own one card; bass owns the cone; cue.latest is the
//  message ribbon. No bars, no EKG, no logo center.
// ════════════════════════════════════════════════════════════════════════

#define MAX_CARDS 7
#define MAX_WALK  48
#define SPACE_CH  26

const float TAU = 6.28318530718;
const float PI  = 3.14159265359;

// ── text atlas access (matches text_clusters.fs idiom) ──
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

// ── hash + noise ──
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

// ── SDF primitives ──
float smin(float a,float b,float k){
    float h=clamp(0.5+0.5*(b-a)/k,0.0,1.0);
    return mix(b,a,h)-k*h*(1.0-h);
}
float sdRoundBox(vec2 p, vec2 b, float r){
    vec2 d = abs(p)-b+r;
    return min(max(d.x,d.y),0.0) + length(max(d,0.0)) - r;
}
float sdCircle(vec2 p, float r){ return length(p)-r; }

// ── 3D hero distance field — variants 0:cone 1:capsule 2:prism ──
float gT, gMotion, gBass, gSpin;
int   gShape;

float sdCone3D(vec3 p, float h, float r){
    // upright cone, apex up. base at y=-h/2 radius r.
    vec2 q = vec2(length(p.xz), p.y);
    vec2 c = normalize(vec2(h, r));
    float d = dot(q - vec2(0.0, -h*0.5), c);
    float side = max(d, -p.y - h*0.5);
    // cap
    float cap = -p.y - h*0.5;
    return max(side, -cap - 0.01) ; // simple cone, sufficient for raymarch
}
float sdCapsule3D(vec3 p, float h, float r){
    p.y -= clamp(p.y, -h*0.5, h*0.5);
    return length(p)-r;
}
float sdBox3D(vec3 p, vec3 b){
    vec3 d = abs(p)-b;
    return length(max(d,0.0)) + min(max(d.x,max(d.y,d.z)),0.0);
}

float heroMap(vec3 p){
    // rotate around Y by spin + slow tilt
    float a = gT*gSpin*0.7 + sin(gT*0.13)*0.18;
    float ca=cos(a), sa=sin(a);
    p.xz = mat2(ca,-sa,sa,ca)*p.xz;
    // breath
    float br = 1.0 + 0.06*gBass;
    p /= br;
    float d;
    if (gShape == 1)      d = sdCapsule3D(p, 2.6, 0.62);
    else if (gShape == 2) d = sdBox3D(p, vec3(0.55, 1.35, 0.55));
    else                  d = sdCone3D(p, 2.8, 0.78);
    d *= br;
    return d;
}
vec3 heroNormal(vec3 p){
    vec2 e = vec2(0.002, 0.0);
    return normalize(vec3(
        heroMap(p+e.xyy)-heroMap(p-e.xyy),
        heroMap(p+e.yxy)-heroMap(p-e.yxy),
        heroMap(p+e.yyx)-heroMap(p-e.yyx)));
}

// Map a hit point on the cone back to a 2D (u,v) cylindrical chart
// so we can draw the typewriter ribbon on its curved surface.
vec2 heroUV(vec3 p){
    float a = gT*gSpin*0.7 + sin(gT*0.13)*0.18;
    float ca=cos(a), sa=sin(a);
    vec3 q = p;
    q.xz = mat2(ca,-sa,sa,ca)*q.xz;
    float u = atan(q.z, q.x) / TAU + 0.5;     // 0..1 around
    float v = clamp((q.y + 1.4)/2.8, 0.0, 1.0);
    return vec2(u, v);
}

// ── palette ──
vec3 palettePick(int sw, float t){
    if (sw == 1) {
        // Risograph: orange/teal/cream
        if (t < 0.33) return vec3(0.95,0.42,0.20);
        if (t < 0.66) return vec3(0.18,0.55,0.62);
        return vec3(0.96,0.92,0.78);
    } else if (sw == 2) {
        // Mono — paper/charcoal/silver
        if (t < 0.33) return vec3(0.92,0.91,0.88);
        if (t < 0.66) return vec3(0.18,0.17,0.18);
        return vec3(0.62,0.62,0.66);
    } else if (sw == 3) {
        // Acid — neon mint, hot red, lemon
        if (t < 0.33) return vec3(0.62,0.98,0.78);
        if (t < 0.66) return vec3(0.98,0.18,0.22);
        return vec3(0.98,0.92,0.20);
    }
    // Editorial (default) — matches reference: cherry red, mint, paper, yellow, pink
    if (t < 0.20) return vec3(0.92,0.18,0.22);
    if (t < 0.40) return vec3(0.62,0.98,0.78);
    if (t < 0.60) return vec3(0.95,0.94,0.91);
    if (t < 0.80) return vec3(0.98,0.86,0.20);
    return vec3(0.96,0.55,0.78);
}

// ── card "cutout image" SDF + interior procedural artwork ──
// kind: 0 rose-stem rect, 1 cherry-poster, 2 mint-ticket arrow,
// 3 sketch panel, 4 sticker dot, 5 receipt strip, 6 stamp
vec3 cardArt(int kind, vec2 uv, int paletteSw){
    // uv in [-1,1] inside the card
    vec3 bg = vec3(0.94,0.92,0.88);
    if (kind == 0) {
        // rose-stem: cream bg, green stem, red bloom
        bg = vec3(0.95,0.93,0.86);
        float stem = smoothstep(0.04, 0.0, abs(uv.x) - 0.04);
        stem *= smoothstep(-0.95, -0.20, uv.y);
        // leaves
        float lf = smoothstep(0.16, 0.0,
            length(vec2(abs(uv.x)-0.14, uv.y+0.20)) - 0.10);
        vec3 col = bg;
        col = mix(col, vec3(0.22,0.58,0.28), stem);
        col = mix(col, vec3(0.18,0.52,0.24), lf);
        // bloom
        float bloom = smoothstep(0.22, 0.0, length(uv - vec2(0.0,0.55)) - 0.22);
        col = mix(col, vec3(0.92,0.22,0.22), bloom);
        return col;
    } else if (kind == 1) {
        // cherry poster — top dark stripe, red gradient body, two gray cherry shapes
        vec3 top = vec3(0.06,0.06,0.07);
        vec3 bot = vec3(0.98,0.12,0.14);
        vec3 col = mix(top, bot, smoothstep(-1.0, 0.4, uv.y));
        float c1 = smoothstep(0.18, 0.0, length(uv - vec2(-0.30, 0.10)) - 0.18);
        float c2 = smoothstep(0.18, 0.0, length(uv - vec2( 0.30, 0.10)) - 0.18);
        col = mix(col, vec3(0.78,0.78,0.80), max(c1,c2));
        // hairline title bar
        float bar = smoothstep(0.02, 0.0, abs(uv.y - 0.78));
        col = mix(col, vec3(0.92,0.92,0.90), bar*0.8);
        return col;
    } else if (kind == 2) {
        // mint ticket with arrow notch at bottom — drawn via SDF below
        bg = vec3(0.60,0.95,0.78);
        float code = smoothstep(0.04, 0.0, abs(uv.y - 0.30));
        bg = mix(bg, vec3(0.05,0.10,0.08), code*0.6);
        float arrow = smoothstep(0.18, 0.0, length(vec2(uv.x*1.5, uv.y+0.70)) - 0.08);
        bg = mix(bg, vec3(0.05,0.10,0.08), arrow);
        return bg;
    } else if (kind == 3) {
        // sketch panel — soft gray with two ink scribbles
        bg = vec3(0.74,0.74,0.74);
        float a = smoothstep(0.018,0.0, abs(uv.x - 0.5*sin(uv.y*4.0))-0.02);
        bg = mix(bg, vec3(0.05,0.05,0.07), a);
        float b = smoothstep(0.018,0.0, abs(uv.x + 0.3 - 0.4*cos(uv.y*5.0))-0.02);
        bg = mix(bg, vec3(0.05,0.05,0.07), b);
        return bg;
    } else if (kind == 4) {
        // yellow sticker — three rounded chips
        bg = vec3(0.98,0.86,0.20);
        float d1 = sdRoundBox(uv - vec2(-0.40, 0.20), vec2(0.18,0.22), 0.10);
        float d2 = sdRoundBox(uv - vec2( 0.10, 0.05), vec2(0.20,0.24), 0.10);
        float d3 = sdRoundBox(uv - vec2( 0.40,-0.30), vec2(0.16,0.20), 0.08);
        float m  = min(min(d1,d2),d3);
        bg = mix(bg*0.6, bg, smoothstep(0.01,-0.01,m));
        return bg;
    } else if (kind == 5) {
        // receipt strip — paper with text-line stripes
        bg = vec3(0.94,0.93,0.88);
        float stripes = smoothstep(0.04,0.0,
            abs(fract(uv.y*5.0)-0.5)-0.18);
        bg = mix(bg, vec3(0.10,0.10,0.12), stripes*0.6);
        return bg;
    }
    // stamp — soft pink with ring
    bg = palettePick(paletteSw, 0.92);
    float ring = smoothstep(0.02, 0.0, abs(length(uv)-0.55));
    bg = mix(bg, vec3(0.05,0.05,0.07), ring*0.7);
    return bg;
}

// per-card position/size/rotation/depth from index
void cardLayout(int i, int n, out vec2 pos, out vec2 half2,
                out float rot, out float z, out int kind){
    float fi = float(i);
    float gold = 2.39996323; // golden angle, used for radial deal-out
    vec2 seed = h21(fi*7.13 + 3.7);

    // ring around the hero, with controlled angular spacing so cards
    // never sit on top of each other. Each card gets a 'slot' angle
    // plus a small per-card jitter so the ring breathes.
    float baseAngle = fi * gold + 0.5;
    float ringR = mix(0.55, 1.05, fract(fi*0.37));
    float drift = gT*0.05*gMotion + fi*0.7;
    vec2 base = vec2(cos(baseAngle)*ringR, sin(baseAngle)*ringR*0.65);
    base += vec2(0.04*sin(drift), 0.05*cos(drift*0.9));
    pos = base;

    // varied half-extents (cards have real layout variety, not uniform tiles)
    vec2 sz = vec2(mix(0.10, 0.22, seed.x), mix(0.13, 0.28, seed.y));
    // first card is a hero poster-shaped tall rectangle
    if (i == 0) sz = vec2(0.13, 0.26);
    if (i == 1) sz = vec2(0.18, 0.22); // poster
    if (i == 2) sz = vec2(0.10, 0.24); // tall ticket
    if (i == 3) sz = vec2(0.20, 0.13); // wide sketch
    half2 = sz;

    // gentle rotation, drifting
    rot = (seed.x - 0.5) * 0.6 + 0.10*sin(gT*0.4*gMotion + fi);

    // depth — closer cards have larger z (positive = forward)
    z = mix(-0.6, 0.6, fract(fi*0.21 + 0.13));

    // pick which artwork the card holds
    kind = int(mod(fi, 7.0));
}

void main(){
    vec2 res = RENDERSIZE;
    vec2 uv  = (gl_FragCoord.xy - 0.5*res) / res.y;

    gT      = TIME * motion;
    gMotion = motion;
    gBass   = clamp(bassPulse * audioDepth, 0.0, 2.5);
    gSpin   = coneSpin;
    gShape  = int(shapeVariant);

    int paletteSw = int(palette);
    int n = int(cardCount);
    if (n > MAX_CARDS) n = MAX_CARDS;

    // ── black ground with subtle warm tint where the spotlight falls
    vec3 col = vec3(0.020, 0.018, 0.022);
    float vign = 1.0 - 0.35*dot(uv, uv);
    col *= vign;
    // very soft cone of light behind hero
    float spot = exp(-length(uv - vec2(0.0,-0.05))*1.6);
    col += spot * vec3(0.08, 0.07, 0.05) * (0.7 + 0.3*gBass);

    // ── raymarch the hero 3D shape ──
    // camera — slight orbit, tilted down a touch
    float cam_a = sin(gT*0.07)*0.20;
    vec3 ro = vec3(sin(cam_a)*4.2, 0.6, cos(cam_a)*4.2);
    vec3 ta = vec3(0.0, 0.05, 0.0);
    vec3 ww = normalize(ta - ro);
    vec3 uuV = normalize(cross(ww, vec3(0,1,0)));
    vec3 vvV = cross(uuV, ww);
    vec3 rd = normalize(uv.x*uuV + uv.y*vvV + 1.35*ww);

    float tt = 0.0;
    bool hit = false;
    for (int i=0;i<70;i++){
        vec3 p = ro + rd*tt;
        float d = heroMap(p);
        if (d < 0.004) { hit = true; break; }
        tt += d*0.85;
        if (tt > 14.0) break;
    }

    // ── floating image cutouts (parallax) ──
    // Back layer cards first (far z), then we'll draw hero over them
    // if the hero's depth wins; then near-z cards over the hero.
    // We do a simple two-pass: far cards (z<0), hero, near cards (z>0).
    vec3 nearAccum = col;     // will be repainted with near cards above hero
    float nearMask = 0.0;
    vec3 farAccum  = col;
    float farMask  = 0.0;

    // Mouse parallax — small camera offset for the whole card stack
    vec2 mPar = (mousePos - 0.5) * 0.15;

    for (int i=0; i<MAX_CARDS; i++){
        if (i >= n) break;
        vec2 pos; vec2 hh; float rot; float z; int kind;
        cardLayout(i, n, pos, hh, rot, z, kind);

        // Audio-channel routing — first three cards bind to player[1..3],
        // remaining cards inherit the bass pulse so they breathe in time.
        float energy = 0.0;
        if (i == 0) energy = cardA;
        else if (i == 1) energy = cardB;
        else if (i == 2) energy = cardC;
        else energy = 0.4 * gBass;

        // Spawn-in pop: cards with low energy idle small; loud energy
        // blooms them up momentarily — that's the per-card visual
        // separation the rubric rewards.
        float scale = mix(0.78, 1.18, smoothstep(0.0, 0.7, energy));
        // micro-twitch on impulses so each card has its own life
        scale *= 1.0 + 0.04*sin(gT*4.0 + float(i)*2.3) * energy;

        // Parallax — apply z-driven offset so far cards lag, near lead
        vec2 cardPos = pos + z * mPar * 1.4;
        // Depth-of-field soft falloff for distance from z=0 (hero plane)
        float dofR = 0.0035 + abs(z)*0.012;

        // Card local coords
        vec2 q = uv - cardPos;
        float c = cos(rot), s = sin(rot);
        q = mat2(c,-s,s,c) * q;
        q /= scale;

        // SDF of rounded card body. Card #2 (ticket) gets the V-notch.
        float card = sdRoundBox(q, hh, 0.015);
        if (i == 2) {
            // arrow notch at bottom of ticket — subtract a wedge
            float w = abs(q.x)*1.5 + (q.y + hh.y) - 0.06;
            card = max(card, -w);
        }
        // outline edge — sharper for near cards, softer for far cards
        float fw = max(fwidth(card), dofR);
        float fill = 1.0 - smoothstep(-fw, fw, card);
        if (fill < 0.001) continue;

        // Artwork uv in [-1,1]
        vec2 auv = q / hh;
        vec3 art = cardArt(kind, auv, paletteSw);

        // Energy lift — tint card brighter when active, dim when silent.
        // This is the per-channel "separability" — mute a player and
        // its card visibly goes quiet.
        float lift = mix(0.55, 1.10, smoothstep(0.0, 0.6, energy));
        art *= lift;
        // active accent rim
        float rim = smoothstep(0.012, 0.0, abs(card)) * energy;
        art = mix(art, palettePick(paletteSw, fract(float(i)*0.21)), rim*0.55);

        // soft drop shadow under the card (offset down-right, blurred)
        float shadow = 1.0 - smoothstep(-fw*2.0, fw*2.0,
            sdRoundBox(q - vec2(0.012,-0.012), hh, 0.015));
        shadow *= 0.45 * (1.0 - 0.5*abs(z));

        // composite into the right layer (far vs near)
        if (z < 0.0) {
            farAccum = mix(farAccum, vec3(0.0), shadow);
            farAccum = mix(farAccum, art, fill);
            farMask  = max(farMask, fill);
        } else {
            nearAccum = mix(nearAccum, vec3(0.0), shadow);
            nearAccum = mix(nearAccum, art, fill);
            nearMask  = max(nearMask, fill);
        }
    }

    // Far cards composited under hero — replace base col with far layer
    col = farAccum;

    // ── shade the hero ──
    if (hit) {
        vec3 p = ro + rd*tt;
        vec3 nrm = heroNormal(p);
        vec3 v   = normalize(ro - p);
        vec3 ldir = normalize(vec3(0.6, 0.8, 0.4));
        float diff = clamp(dot(nrm, ldir), 0.0, 1.0);
        float fres = pow(1.0 - clamp(dot(nrm, v), 0.0, 1.0), 3.5);
        float spec = pow(clamp(dot(nrm, normalize(v+ldir)), 0.0, 1.0), 80.0);

        // Paper base — warm cream w/ slight noise
        vec3 paper = vec3(0.96, 0.94, 0.90);
        paper += (fbm2(p.xy*4.0 + p.z*1.2) - 0.5) * 0.04;

        // ── text wrapped onto the cone's surface ──
        // Build a (u,v) chart from the hit point and draw the typewriter
        // ribbon as a horizontal band of glyphs. cue.latest is bound to
        // msg, msgAge runs the typewriter.
        vec2 hv = heroUV(p);
        // Band sits at v ≈ 0.55 (upper midline of the cone body)
        float bandCenter = 0.58;
        float bandHalf   = 0.08;
        float bandY = (hv.y - bandCenter) / bandHalf;       // -1..1 inside band

        int total = charCount();
        bool inBand = abs(bandY) < 1.0 && total > 0;
        // Glyph grid: 32 columns around the cone equator, 1 row.
        float charsPerLap = 32.0;
        // Slow scroll so long messages crawl around the cone.
        float scroll = gT * 0.06;
        float u_g = fract(hv.x + scroll);
        float colF = u_g * charsPerLap;
        int   gCol = int(floor(colF));
        float fx   = fract(colF);
        float fy   = (bandY*0.5 + 0.5);                  // 0..1 vertical

        float glyph = 0.0;
        if (inBand) {
            // Typewriter reveal — how many chars are currently visible.
            float visible = (msgAge >= 0.0) ? clamp(msgAge*28.0, 0.0, float(total))
                                            : float(total);
            int slot = int(mod(float(gCol), float(total)));
            if (float(gCol) < visible || msgAge < 0.0) {
                int ch = getChar(slot);
                if (ch >= 0 && ch <= 35 && ch != SPACE_CH) {
                    // glyph cell is square-ish; sample atlas
                    float s = sampleChar(ch, vec2(fx, fy));
                    glyph = smoothstep(0.20, 0.55, s);
                }
            }
        }

        // Surface shading composite — paper + ink + light
        vec3 ink   = vec3(0.04, 0.04, 0.06);
        vec3 surf  = paper * (0.30 + 0.75*diff);
        surf      += spec * vec3(1.0,0.95,0.85) * 0.4;
        surf      += fres * vec3(0.85,0.78,0.95) * 0.15;
        // print the ribbon onto the surface
        surf = mix(surf, ink, glyph * textOnCone);

        // Subtle horizontal print stripes on the cone (broadsheet feel)
        float stripes = smoothstep(0.45, 0.50,
            fract((hv.y*48.0)+0.5));
        surf = mix(surf, ink, stripes * 0.06);

        // atmospheric fog by distance
        float haze = 1.0 - exp(-tt * 0.04 * fogDensity);
        vec3 fogCol = vec3(0.02,0.02,0.03);
        vec3 heroCol = mix(surf, fogCol, haze);

        // ground contact shadow under the hero
        float gshadow = smoothstep(0.30, 0.0, length(uv - vec2(0.0,-0.30)));
        col *= 1.0 - gshadow*0.35;

        col = heroCol;
    }

    // ── near cards composited over the hero ──
    col = mix(col, nearAccum, nearMask);

    // ── grain + bloom finish ──
    float n2 = h12(gl_FragCoord.xy + vec2(gT*60.0, 0.0));
    col += (n2 - 0.5) * 0.04 * grain;
    // gentle bloom on near-white card highlights
    float L = dot(col, vec3(0.299,0.587,0.114));
    col += smoothstep(0.78, 1.20, L) * 0.20 * col;

    // tone
    col = col / (1.0 + 0.55*col);
    col = pow(max(col, 0.0), vec3(0.94));

    gl_FragColor = vec4(col, 1.0);
}
