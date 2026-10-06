/*{
  "DESCRIPTION": "Maximalist 3D Shapes Text — a dense corecore poster collage on warm paper. A vertical neon lightning spine splits the canvas; raymarched 3D shapes (sphere/wing/bust/hand/torus) tumble in real depth behind a confetti of procedural image cutouts (flower, statue, sticker, robot, pixel-art card, star emojis) that parallax in z. Three player channels each twitch one shape-cluster and two cutout swarms; bass owns the lightning, cue.latest types out as the giant headline ribbon that scribbles across the middle. Ticker bars top + bottom carry secondary glyphs. Dense, loud, lo-fi internet-poster — not symmetrical, not literal, no spectrum bars, no horizon.",
  "CREDIT": "ShaderClaw — A-List drop · maximalist_3dshapes_text",
  "CATEGORIES": [
    "Generator",
    "Text",
    "A-List"
  ],
  "INPUTS": [
    {
      "NAME": "swarmA",
      "LABEL": "Shapes A · Energy",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[1].energy"
    },
    {
      "NAME": "swarmB",
      "LABEL": "Cutouts B · Active",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[2].active"
    },
    {
      "NAME": "swarmC",
      "LABEL": "Stickers C · Pitch",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "BIND": "player[3].pitch"
    },
    {
      "NAME": "grain",
      "LABEL": "Print Grain",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.2,
      "DEFAULT": 0.45
    },
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
      "NAME": "shapeCount",
      "LABEL": "3D Shape Count",
      "TYPE": "long",
      "DEFAULT": 6,
      "VALUES": [
        3,
        4,
        5,
        6,
        7,
        8
      ],
      "LABELS": [
        "3",
        "4",
        "5",
        "6",
        "7",
        "8"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "cutoutCount",
      "LABEL": "Image Cutout Count",
      "TYPE": "long",
      "DEFAULT": 7,
      "VALUES": [
        3,
        4,
        5,
        6,
        7,
        8,
        9
      ],
      "LABELS": [
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "9"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "density",
      "LABEL": "Density",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 1.6,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "motion",
      "LABEL": "Motion Tempo",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "boltJitter",
      "LABEL": "Bolt Jitter",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.7,
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
      "NAME": "palette",
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
        "Poster",
        "Acid",
        "Mono",
        "Risograph"
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
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "MAJESTIC CASUAL",
      "MAX_LENGTH": 48,
      "BIND": "cue.latest",
      "LABEL": "Message",
      "GROUP": "Text"
    },
    {
      "NAME": "headlineSize",
      "LABEL": "Headline Size",
      "TYPE": "float",
      "MIN": 0.04,
      "MAX": 0.16,
      "DEFAULT": 0.085,
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
      "NAME": "boltPulse",
      "LABEL": "Lightning · Bass",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "BIND": "audio.bass",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "audioDepth",
      "LABEL": "Audio Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
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


// ════════════════════════════════════════════════════════════════════════
//  MAXIMALIST 3D SHAPES TEXT
//
//  Composition (not literal): warm paper ground, hard chromatic vertical
//  bolt slashing the page, raymarched 3D bodies tumbling at distinct
//  z-depths BEHIND a swarm of procedural image-cutouts and a stuttering
//  typewritten headline that sprawls across the middle band. Three
//  player channels each own one swarm; bass owns the bolt; cue.latest
//  is the headline. No symmetry, no horizon, no bars.
// ════════════════════════════════════════════════════════════════════════

#define MAX_SHAPES   8
#define MAX_CUTOUTS  9
#define MAX_WALK     48
#define SPACE_CH     26

const float TAU = 6.28318530718;
const float PI  = 3.14159265359;

// ─── text atlas ───
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

// ─── hash + noise ───
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

// ─── SDF primitives ───
float smin(float a,float b,float k){
    float h=clamp(0.5+0.5*(b-a)/k,0.0,1.0);
    return mix(b,a,h)-k*h*(1.0-h);
}
float sdRoundBox(vec2 p, vec2 b, float r){
    vec2 d = abs(p)-b+r;
    return min(max(d.x,d.y),0.0) + length(max(d,0.0)) - r;
}
float sdCircle(vec2 p, float r){ return length(p)-r; }

// ─── globals ───
float gT, gMotion, gBass, gSwarmA, gSwarmB, gSwarmC, gJitter;
int   gPaletteSw;

// ─── 3D shape distance field — variants 0:sphere 1:torus 2:capsule 3:prism ───
float sdSphere(vec3 p, float r){ return length(p)-r; }
float sdTorus(vec3 p, vec2 t){
    vec2 q = vec2(length(p.xz)-t.x, p.y);
    return length(q)-t.y;
}
float sdCapsule(vec3 p, float h, float r){
    p.y -= clamp(p.y, -h, h);
    return length(p)-r;
}
float sdBox3(vec3 p, vec3 b){
    vec3 d = abs(p)-b;
    return length(max(d,0.0)) + min(max(d.x,max(d.y,d.z)),0.0);
}

mat2 rot2(float a){ float c=cos(a), s=sin(a); return mat2(c,-s,s,c); }

// Map a single 3D shape instance (i) — returns distance, writes material id m.
// Each shape orbits its own anchor; some twitch with player A.
float shapeMapOne(int i, vec3 p, out float mOut){
    float fi = float(i);
    vec2  s1 = h21(fi*9.13 + 1.7);
    vec2  s2 = h21(fi*17.3 + 4.9);
    int kind = int(mod(fi, 4.0));
    // anchor (in world space) — densely packed around origin
    vec3 anc;
    anc.x = (s1.x - 0.5) * 4.2;
    anc.y = (s1.y - 0.5) * 2.8;
    anc.z = (s2.x - 0.5) * 3.4;
    // slow tumble + per-shape twitch from swarmA
    float t = gT * (0.5 + 0.6*s2.y) + fi*1.3;
    anc.x += 0.22 * sin(t*0.7 + fi);
    anc.y += 0.18 * cos(t*0.9 + fi*1.7);
    anc.z += 0.40 * sin(t*0.5 + fi*2.3);
    // twitch on swarmA — distinct kick when active
    anc += vec3(sin(fi*3.1), cos(fi*1.7), sin(fi*4.3)) * 0.18 * gSwarmA;
    vec3 q = p - anc;
    // independent rotation per shape
    q.xz = rot2(t*0.6 + fi)*q.xz;
    q.yz = rot2(t*0.4 + fi*0.7)*q.yz;
    float scale = mix(0.28, 0.62, s1.y) * (1.0 + 0.70*gBass);
    float d;
    if (kind == 0)      d = sdSphere(q, scale);
    else if (kind == 1) d = sdTorus(q, vec2(scale*0.85, scale*0.32));
    else if (kind == 2) d = sdCapsule(q, scale*0.9, scale*0.45);
    else                d = sdBox3(q, vec3(scale*0.62));
    mOut = fract(fi*0.314 + 0.1);
    return d;
}

// Composite distance field: union of all shapes (no smooth-union — we
// want them to read as SEPARATE bodies so swarmA's twitch is visible).
float shapeMap(vec3 p, out float mOut, out int hitIdx, int count){
    float best = 1e5; mOut = 0.0; hitIdx = 0;
    for (int i = 0; i < MAX_SHAPES; i++){
        if (i >= count) break;
        float m;
        float d = shapeMapOne(i, p, m);
        if (d < best){ best = d; mOut = m; hitIdx = i; }
    }
    return best;
}

vec3 shapeNormal(vec3 p, int idx){
    vec2 e = vec2(0.003, 0.0);
    float dummy;
    return normalize(vec3(
        shapeMapOne(idx, p+e.xyy, dummy) - shapeMapOne(idx, p-e.xyy, dummy),
        shapeMapOne(idx, p+e.yxy, dummy) - shapeMapOne(idx, p-e.yxy, dummy),
        shapeMapOne(idx, p+e.yyx, dummy) - shapeMapOne(idx, p-e.yyx, dummy)));
}

// ─── palette ───
vec3 palettePick(int sw, float t){
    if (sw == 1) {
        // Acid — chartreuse / hot pink / cyan / black
        if (t < 0.25) return vec3(0.78,0.98,0.20);
        if (t < 0.50) return vec3(0.98,0.18,0.62);
        if (t < 0.75) return vec3(0.18,0.85,0.95);
        return vec3(0.04,0.04,0.06);
    } else if (sw == 2) {
        // Mono — paper / ink / silver / charcoal
        if (t < 0.25) return vec3(0.92,0.91,0.88);
        if (t < 0.50) return vec3(0.10,0.10,0.12);
        if (t < 0.75) return vec3(0.62,0.62,0.66);
        return vec3(0.30,0.30,0.34);
    } else if (sw == 3) {
        // Risograph — orange / teal / cream / plum
        if (t < 0.25) return vec3(0.95,0.42,0.20);
        if (t < 0.50) return vec3(0.18,0.55,0.62);
        if (t < 0.75) return vec3(0.96,0.92,0.78);
        return vec3(0.42,0.20,0.38);
    }
    // Poster (default) — internet-corecore palette
    if (t < 0.20) return vec3(0.92,0.18,0.22); // cherry
    if (t < 0.40) return vec3(0.20,0.92,0.42); // bolt-green
    if (t < 0.60) return vec3(0.20,0.42,0.92); // cobalt
    if (t < 0.80) return vec3(0.98,0.86,0.20); // lemon
    return vec3(0.92,0.42,0.92);               // magenta
}

// ─── image cutout artwork (procedural, non-literal — evokes ref) ───
vec3 cutoutArt(int kind, vec2 uv, int paletteSw){
    // uv in [-1,1] inside cutout bounding box. Returns RGB.
    if (kind == 0) {
        // tulip-bloom
        vec3 col = vec3(0.92,0.90,0.84);
        float stem = smoothstep(0.03, 0.0, abs(uv.x) - 0.025);
        stem *= smoothstep(-0.9, -0.1, uv.y);
        col = mix(col, vec3(0.20,0.55,0.22), stem);
        float bloom = smoothstep(0.30, 0.0, length(uv - vec2(0.0,0.50)) - 0.28);
        col = mix(col, vec3(0.92,0.20,0.32), bloom);
        float petal = smoothstep(0.18, 0.0, length(uv - vec2(-0.18,0.55)) - 0.16);
        col = mix(col, vec3(0.96,0.42,0.30), petal*0.8);
        return col;
    } else if (kind == 1) {
        // sculpture-bust silhouette on cream
        vec3 col = vec3(0.94,0.92,0.84);
        float head = smoothstep(0.34, 0.0, length(uv - vec2(0.0,0.10)) - 0.32);
        float neck = smoothstep(0.18, 0.0, sdRoundBox(uv - vec2(0.0,-0.42), vec2(0.18,0.20), 0.08));
        float body = max(head, neck);
        col = mix(col, vec3(0.74,0.70,0.65), body);
        float shade = smoothstep(0.10, 0.0, length(uv - vec2(0.10,0.18)) - 0.10);
        col = mix(col, vec3(0.55,0.50,0.45), shade*body*0.5);
        return col;
    } else if (kind == 2) {
        // pixel-card with confetti grid (evokes "phone grid" cutout)
        vec3 col = vec3(0.10,0.10,0.14);
        vec2 cell = floor(uv * 5.5 + 5.5);
        float pick = h12(cell);
        vec3 chip = palettePick(paletteSw, pick);
        col = mix(col, chip, step(0.35, pick));
        return col;
    } else if (kind == 3) {
        // robot/mecha — boxes stacked, knee bend
        vec3 col = vec3(0.86,0.84,0.78);
        float head = smoothstep(0.04, 0.0, sdRoundBox(uv - vec2(0.0,0.55), vec2(0.20,0.12), 0.04));
        float torso = smoothstep(0.04, 0.0, sdRoundBox(uv - vec2(0.0,0.20), vec2(0.28,0.22), 0.06));
        float legL  = smoothstep(0.04, 0.0, sdRoundBox(uv - vec2(-0.14,-0.30), vec2(0.10,0.30), 0.04));
        float legR  = smoothstep(0.04, 0.0, sdRoundBox(uv - vec2( 0.14,-0.30), vec2(0.10,0.30), 0.04));
        float mech  = max(max(head,torso), max(legL,legR));
        col = mix(col, vec3(0.30,0.30,0.34), mech);
        // accent eye
        float eye = smoothstep(0.025, 0.0, length(uv - vec2(0.0,0.55)) - 0.025);
        col = mix(col, vec3(0.98,0.20,0.18), eye);
        return col;
    } else if (kind == 4) {
        // sticker-star — 5-pt star approximation via polar
        float a = atan(uv.y, uv.x);
        float r = length(uv);
        float pts = 5.0;
        float spikes = abs(cos(a*pts*0.5));
        float starR  = mix(0.18, 0.45, spikes);
        float in_ = smoothstep(0.01, -0.01, r - starR);
        vec3 col = palettePick(paletteSw, 0.65);
        vec3 bg  = vec3(0.94,0.92,0.86);
        return mix(bg, col, in_);
    } else if (kind == 5) {
        // wing/feather curl — three soft scallops
        vec3 col = vec3(0.78,0.62,0.42);
        float a = smoothstep(0.10, 0.0, length(uv - vec2(-0.25,-0.10)) - 0.18);
        float b = smoothstep(0.10, 0.0, length(uv - vec2( 0.05, 0.05)) - 0.20);
        float c = smoothstep(0.10, 0.0, length(uv - vec2( 0.30, 0.20)) - 0.22);
        float w = max(max(a,b),c);
        col = mix(vec3(0.94,0.90,0.82), col, w);
        return col;
    } else if (kind == 6) {
        // fingertips — three fingers reaching up
        vec3 col = vec3(0.92,0.78,0.62);
        float f1 = smoothstep(0.04, 0.0, sdRoundBox(uv - vec2(-0.25,0.00), vec2(0.06,0.36), 0.06));
        float f2 = smoothstep(0.04, 0.0, sdRoundBox(uv - vec2( 0.00,0.10), vec2(0.06,0.42), 0.06));
        float f3 = smoothstep(0.04, 0.0, sdRoundBox(uv - vec2( 0.25,0.05), vec2(0.06,0.38), 0.06));
        float hand = max(max(f1,f2),f3);
        vec3 skin = vec3(0.88,0.62,0.48);
        vec3 bg   = vec3(0.94,0.92,0.86);
        return mix(bg, skin, hand);
    } else if (kind == 7) {
        // QR-ish glyph pad
        vec3 col = vec3(0.94,0.92,0.86);
        vec2 cell = floor(uv * 7.0 + 7.0);
        float on = step(0.55, h12(cell));
        col = mix(col, vec3(0.04,0.04,0.06), on);
        // corner anchors
        float corner = smoothstep(0.04, 0.0,
            sdRoundBox(abs(uv) - vec2(0.55,0.55), vec2(0.10,0.10), 0.02));
        col = mix(col, vec3(0.04,0.04,0.06), corner);
        return col;
    }
    // kind 8 — emoji-ish smiley dot
    vec3 col = vec3(0.98,0.86,0.20);
    float ball = smoothstep(0.02, 0.0, length(uv) - 0.50);
    col = mix(vec3(0.94,0.92,0.86), col, ball);
    float eL = smoothstep(0.02,0.0, length(uv - vec2(-0.16,0.12)) - 0.04);
    float eR = smoothstep(0.02,0.0, length(uv - vec2( 0.16,0.12)) - 0.04);
    col = mix(col, vec3(0.04,0.04,0.06), max(eL,eR));
    float mouth = smoothstep(0.02,0.0,
        abs(length(uv - vec2(0.0,0.04)) - 0.18) - 0.025);
    mouth *= smoothstep(0.0, 0.1, -uv.y);
    col = mix(col, vec3(0.04,0.04,0.06), mouth);
    return col;
}

// per-cutout layout — pos (NDC, aspect-corrected), half extents, rotation, z-plane
void cutoutLayout(int i, int total, out vec2 pos, out vec2 half2,
                  out float rot, out float zPlane, out int kind){
    float fi = float(i);
    vec2 s1 = h21(fi*5.71 + 0.3);
    vec2 s2 = h21(fi*11.9 + 7.7);

    // Drift trajectories — large-scale random tour of the canvas (no symmetry).
    // Each cutout has its own slow Lissajous orbit anchored by its seed.
    float ang = fi * 2.39996323;
    float r   = mix(0.30, 0.95, fract(fi*0.41 + 0.21));
    vec2 base = vec2(cos(ang)*r*1.05, sin(ang*1.3)*r*0.55);
    base += vec2(0.07 * sin(gT*0.13*gMotion + fi*0.9),
                 0.06 * cos(gT*0.17*gMotion + fi*1.7));

    // swarmB-driven offset: cutouts of even index twitch (player[2].active)
    if (mod(fi, 2.0) < 0.5) {
        base += vec2(sin(fi*4.7), cos(fi*3.1)) * 0.05 * gSwarmB;
    }

    pos    = base;
    half2  = vec2(mix(0.06, 0.14, s1.x), mix(0.08, 0.18, s1.y));
    rot    = (s2.x - 0.5) * 0.6 + 0.10 * sin(gT*0.4*gMotion + fi);
    zPlane = mix(0.15, 0.95, fract(fi*0.273 + 0.07));
    kind   = int(mod(fi, 9.0));
}

// per-shape z-plane (for ordering shapes in front of/behind cutouts).
float shapeZPlane(int i){
    return mix(0.40, 0.85, fract(float(i)*0.179 + 0.13));
}

// ─── lightning bolt — vertical jagged spine ───
// returns intensity in [0,1] at canvas point p (aspect-corrected, centered).
float lightning(vec2 p){
    // base axis at x = 0.04 (slightly off-center per ref).
    float x0 = 0.04;
    // jitter the axis as a function of y so it zig-zags
    float zig = 0.0;
    zig += 0.030 * sin(p.y*8.0  + gT*1.7);
    zig += 0.020 * sin(p.y*22.0 + gT*4.3);
    zig += 0.060 * (vnoise(vec2(p.y*5.0, gT*0.6)) - 0.5);
    float jx = x0 + zig * (0.5 + 0.7*gJitter);
    float dx = abs(p.x - jx);
    // hot core + soft halo
    float core = smoothstep(0.012, 0.0, dx);
    float halo = smoothstep(0.055, 0.0, dx) * 0.6;
    // top-cap fade
    core *= smoothstep(0.55, 0.30, p.y);
    halo *= smoothstep(0.60, 0.32, p.y);
    float intensity = core + halo;
    // bass-driven double pulse
    intensity *= (0.60 + 1.5*gBass);
    return clamp(intensity, 0.0, 1.0);
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    uv += mkMotion(uv, TIME);
    float aspect = res.x / res.y;
    // aspect-corrected centered coords; +y up, +x right
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    // Continuous band-follow (ambient fix): smooth bass breathes the whole
    // poster's zoom (~±9%) — every edge moves with beatless swells, no beat
    // gating. Silence: exactly the authored framing.
    float bassF = smoothstep(0.03, 0.9, audioBass) * clamp(audioDepth, 0.0, 2.0);
    float midF  = smoothstep(0.05, 0.85, audioMid) * clamp(audioDepth, 0.0, 2.0);
    p /= 1.0 + 0.09 * bassF;

    // ─── globals from inputs ───
    gT      = TIME;
    gMotion = motion;
    gBass   = clamp(audioBass * audioDepth, 0.0, 2.0);
    gSwarmA = clamp(swarmA, 0.0, 1.0);
    gSwarmB = clamp(swarmB, 0.0, 1.0);
    gSwarmC = clamp(swarmC, 0.0, 1.0);
    gJitter = clamp(boltJitter, 0.0, 1.5);
    gPaletteSw = int(palette);

    int nShapes  = int(shapeCount);
    if (nShapes > MAX_SHAPES) nShapes = MAX_SHAPES;
    int nCutouts = int(cutoutCount);
    if (nCutouts > MAX_CUTOUTS) nCutouts = MAX_CUTOUTS;
    float dens = density;

    // ─── warm paper backdrop ───
    vec3 paper = mix(vec3(0.96,0.95,0.92), vec3(0.92,0.91,0.86),
                     fbm2(p * 1.3));
    paper *= 1.0 - 0.05 * dot(p, p);
    paper = mix(paper, bgColor.rgb, bgColor.a);  // universal background
    vec3 col = paper;

    // ─── ticker bars top + bottom (lo-fi info strips) ───
    // top strip
    float topStrip = smoothstep(0.005, 0.0, abs(p.y - 0.46));
    float topBand  = step(0.43, p.y) * step(p.y, 0.49);
    // alternating red/yellow chips along the top
    float tcell = floor((p.x + aspect*0.5) * 6.0);
    vec3 tchip  = (mod(tcell, 2.0) < 0.5)
        ? palettePick(gPaletteSw, 0.10)
        : palettePick(gPaletteSw, 0.70);
    col = mix(col, tchip*0.95, topBand * 0.9);
    col = mix(col, vec3(0.04,0.04,0.06), topStrip*0.6);
    // bottom strip
    float botBand = step(-0.49, p.y) * step(p.y, -0.43);
    float bcell = floor((p.x + aspect*0.5) * 6.0 + 0.5);
    vec3 bchip  = (mod(bcell, 2.0) < 0.5)
        ? palettePick(gPaletteSw, 0.30)
        : palettePick(gPaletteSw, 0.50);
    col = mix(col, bchip*0.92, botBand * 0.9);

    // ─── raymarch the 3D shape swarm (BEHIND the cutouts) ───
    // Camera at z=4 looking at origin; orthographic-ish narrow FOV
    // to keep shapes filling the frame without extreme perspective.
    vec3 ro = vec3(0.0, 0.0, 4.2 - 1.6*gBass);
    vec3 rd = normalize(vec3(p.x*1.3, p.y*1.3, -1.5));
    float tt = 0.0;
    bool hit = false;
    float mShape = 0.0;
    int hitIdx = 0;
    for (int i = 0; i < 56; i++){
        vec3 wp = ro + rd*tt;
        if (abs(wp.x) > 5.0 || abs(wp.y) > 5.0 || wp.z < -5.0) break;
        float d = shapeMap(wp, mShape, hitIdx, nShapes);
        if (d < 0.005){ hit = true; break; }
        tt += d * 0.85;
        if (tt > 14.0) break;
    }
    float shapeZ = 0.55;          // global z-plane for the shape layer
    vec3 shapeCol = vec3(0.0);
    float shapeAlpha = 0.0;
    if (hit) {
        vec3 wp = ro + rd*tt;
        vec3 n  = shapeNormal(wp, hitIdx);
        vec3 v  = normalize(ro - wp);
        vec3 L  = normalize(vec3(0.4, 0.7, 0.6));
        float lam = clamp(dot(n,L), 0.0, 1.0);
        float fres = pow(1.0 - clamp(dot(n,v),0.0,1.0), 3.0);
        vec3 base = palettePick(gPaletteSw, mShape);
        // soft shading: posterized bands so shape reads as flat poster art with depth
        float bands = floor(lam * 4.0 + 0.5) / 4.0;
        shapeCol = base * (0.35 + 0.65 * bands);
        // rim highlight in complementary palette pick
        shapeCol += fres * palettePick(gPaletteSw, fract(mShape + 0.5)) * 0.45;
        // crisp ink outline
        float dCheck;
        int idxCheck;
        float dEdge = shapeMap(ro + rd*(tt + 0.012), dCheck, idxCheck, nShapes);
        float edge = smoothstep(0.020, 0.0, dEdge);
        shapeCol = mix(shapeCol, vec3(0.04,0.04,0.06), edge*0.7);
        shapeAlpha = 1.0;
        // store actual z-plane for layering
        shapeZ = shapeZPlane(hitIdx);
    }

    // ─── compose paper ← shapes (will composite cutouts on top by z) ───
    // We need z-sorting: cutouts can be in front of OR behind shapes.
    // We collect all cutouts that lie behind shape z first, then composite
    // shape, then cutouts in front.

    // Two-pass cutout compositing. Pass 1: cutouts BEHIND the shape layer.
    for (int i = 0; i < MAX_CUTOUTS; i++){
        if (i >= nCutouts) break;
        vec2 cpos; vec2 chalf; float crot; float czp; int ckind;
        cutoutLayout(i, nCutouts, cpos, chalf, crot, czp, ckind);
        if (czp >= shapeZ) continue;   // in-front — handled later
        // apply rotation around cpos
        vec2 d = p - cpos;
        d = rot2(crot) * d;
        vec2 uvCard = d / chalf;
        if (abs(uvCard.x) > 1.0 || abs(uvCard.y) > 1.0) continue;
        // soft mask edge
        float edge = smoothstep(1.0, 0.92, max(abs(uvCard.x), abs(uvCard.y)));
        // shadow ground for depth read
        float shadow = smoothstep(1.05, 0.85, max(abs(uvCard.x), abs(uvCard.y)));
        col = mix(col, col*0.78, shadow * 0.20 * (1.0 - czp));
        vec3 art = cutoutArt(ckind, uvCard, gPaletteSw);
        col = mix(col, art, edge);
    }

    // composite shape on top of behind-cutouts
    col = mix(col, shapeCol, shapeAlpha);

    // Pass 2: cutouts IN FRONT of the shape layer
    for (int i = 0; i < MAX_CUTOUTS; i++){
        if (i >= nCutouts) break;
        vec2 cpos; vec2 chalf; float crot; float czp; int ckind;
        cutoutLayout(i, nCutouts, cpos, chalf, crot, czp, ckind);
        if (czp < shapeZ) continue;
        vec2 d = p - cpos;
        d = rot2(crot) * d;
        vec2 uvCard = d / chalf;
        if (abs(uvCard.x) > 1.0 || abs(uvCard.y) > 1.0) continue;
        float edge = smoothstep(1.0, 0.92, max(abs(uvCard.x), abs(uvCard.y)));
        float shadow = smoothstep(1.10, 0.92, max(abs(uvCard.x), abs(uvCard.y)));
        col = mix(col, col*0.72, shadow * 0.25);
        vec3 art = cutoutArt(ckind, uvCard, gPaletteSw);
        col = mix(col, art, edge);
    }

    // ─── lightning bolt (in front of everything, chromatic) ───
    float bolt = lightning(p);
    if (bolt > 0.001) {
        // chromatic split — RGB offset on the same axis
        float boltR = lightning(p - vec2(0.005, 0.0));
        float boltB = lightning(p + vec2(0.005, 0.0));
        vec3 boltCol = vec3(boltR*0.95, bolt*1.05, boltB*0.95);
        // green/cyan signature, multiplied by palette pick
        vec3 boltTint = palettePick(gPaletteSw, 0.30);
        boltCol = mix(boltCol, boltTint, 0.55);
        col = mix(col, boltCol, clamp(bolt*1.2, 0.0, 1.0));
    }

    // ─── stickers swarm C — small scattered emoji dots driven by player[3].pitch ───
    // Cells across the canvas; each cell has a deterministic dot whose
    // visibility opens with swarmC. Pitch shifts the "live" band of cells.
    {
        vec2 cellP = p * 4.5;
        vec2 cell  = floor(cellP);
        vec2 frc   = fract(cellP) - 0.5;
        float seed = h12(cell);
        // pitch (gSwarmC) selects which cells light up
        float band = abs(fract(seed*3.7 + gT*0.05) - gSwarmC);
        float on   = step(band, 0.20) * step(0.40, seed);
        float dotR = 0.18 + 0.10 * h11(seed*7.3);
        float dotD = smoothstep(dotR, dotR-0.03, length(frc));
        if (on * dotD > 0.01) {
            vec3 dotCol = palettePick(gPaletteSw, fract(seed*5.1));
            col = mix(col, dotCol, dotD * on * 0.85);
            // tiny ink ring
            float ring = smoothstep(0.02,0.0, abs(length(frc) - dotR)) * on;
            col = mix(col, vec3(0.04,0.04,0.06), ring*0.5);
        }
    }

    // ─── headline typewriter — cue.latest sprawls across the middle band ───
    // Multi-line, left-aligned in a band roughly y ∈ [-0.10, +0.30]. The
    // typewriter reveals chars progressively via msgAge (matches Easel's
    // live typewriter speed ~28 cps). Glyphs are large, hand-painted
    // colors per word.
    int total = charCount();
    if (total > 0) {
        bool live = msgAge >= 0.0;
        // characters revealed
        const float CPS = 28.0;
        int revealed = total;
        if (live) {
            float rf = max(0.0, msgAge) * CPS;
            revealed = int(min(float(total), floor(rf)));
        }

        float headH = clamp(headlineSize, 0.04, 0.16);
        float headW = headH * (5.0 / 7.0);
        float kern  = headW * 1.05;
        float lineH = headH * 1.30;

        // band origin (top-left of headline) — slightly left of center,
        // upper-middle.
        float bandLeft   = -0.40 * aspect;
        float bandTop    =  0.22;
        float bandRight  =  0.42 * aspect;
        float bandWidth  = bandRight - bandLeft;

        // word-wrap: compute the column count that fits bandWidth at our kern
        int colsMax = int(floor(bandWidth / kern));
        if (colsMax < 6) colsMax = 6;
        if (colsMax > 24) colsMax = 24;

        // Pixel position relative to band top-left
        float lx = p.x - bandLeft;
        float ly = bandTop - p.y;  // top→down
        if (lx >= 0.0 && lx <= bandWidth && ly >= 0.0 && ly <= lineH * 4.0) {
            int targetCol = int(floor(lx / kern));
            int targetRow = int(floor(ly / lineH));
            if (targetCol >= 0 && targetCol < colsMax && targetRow >= 0 && targetRow < 4) {
                // walk the message with word-wrap to find char at (targetRow,targetCol)
                int cursorR = 0, cursorC = 0;
                int outCh = -1;
                int wordIdx = 0;
                int outWord = 0;
                for (int i = 0; i < MAX_WALK; i++) {
                    if (i >= total) break;
                    if (cursorR > targetRow) break;
                    int ch = getChar(i);
                    if (ch == SPACE_CH) {
                        // peek next word length
                        int wlen = 0;
                        for (int j = 1; j < MAX_WALK; j++) {
                            int jj = i + j;
                            if (jj >= total) break;
                            int chj = getChar(jj);
                            if (chj == SPACE_CH || chj < 0 || chj > 36) break;
                            wlen++;
                        }
                        if (cursorC > 0 && cursorC + 1 + wlen > colsMax) {
                            cursorR++; cursorC = 0;
                            wordIdx++;
                        } else if (cursorC > 0) {
                            if (cursorR == targetRow && cursorC == targetCol) {
                                outCh = SPACE_CH;
                                outWord = wordIdx;
                            }
                            cursorC++;
                        }
                    } else if (ch >= 0 && ch <= 36) {
                        if (cursorR == targetRow && cursorC == targetCol) {
                            outCh = ch;
                            outWord = wordIdx;
                            // typewriter gate
                            if (i >= revealed) outCh = -1;
                        }
                        cursorC++;
                        if (cursorC >= colsMax) {
                            cursorR++; cursorC = 0;
                            wordIdx++;
                        }
                    }
                }
                if (outCh >= 0 && outCh < 36 && outCh != SPACE_CH) {
                    // glyph sample
                    float fx = (lx - float(targetCol)*kern) / headW;
                    float fy = 1.0 - (ly - float(targetRow)*lineH) / headH;
                    if (fx >= 0.0 && fx <= 1.0 && fy >= 0.0 && fy <= 1.0) {
                        float s = sampleChar(outCh, vec2(fx, fy));
                        s = smoothstep(0.20, 0.55, s);
                        if (s > 0.001) {
                            // each word gets its own poster color
                            vec3 inkCol = palettePick(gPaletteSw, fract(float(outWord)*0.27 + 0.05));
                            // word #0 = bold black title
                            if (outWord == 0) inkCol = vec3(0.04,0.04,0.06);
                            // headline color override: alternate fill / outline per word
                            bool outlined = (mod(float(outWord), 3.0) > 1.9);
                            if (outlined) {
                                // glyph is outline only — sample edge band
                                float inner = smoothstep(0.55, 0.75, s);
                                float ring = s - inner;
                                col = mix(col, inkCol, clamp(ring*1.4, 0.0, 1.0));
                            } else {
                                col = mix(col, inkCol, s);
                            }
                        }
                    }
                }
            }
        }
    }

    // ─── bass breath — whole-poster DARKEN-dip (round-2) ───
    // The round-1 lift `1.0 + 1.6*(gBass-0.10)` brightened, which clips flat
    // on the near-white paper (most of the frame), and wasn't unity at
    // silence (x0.84). Darken-dips always read on white; linear bands with
    // a floored depth so low Audio Depth can't dilute it. Silence = 1.0.
    float aDm = 0.6 + 0.4 * min(clamp(audioDepth, 0.0, 2.0), 1.0);
    col *= 1.0 - aDm * (0.22 * clamp(audioBass, 0.0, 1.0)
                      + 0.12 * clamp(audioMid,  0.0, 1.0));

    // mids tilt the paper temperature (continuous, beatless-friendly; the
    // exposure lift above clamps on white paper, this keeps varying there)
    col *= vec3(1.0 + 0.07 * midF, 1.0, 1.0 - 0.07 * midF);

    // ─── grain + vignette ───
    float gn = (h12(gl_FragCoord.xy + gT) - 0.5) * 0.10 * grain;
    col += gn;
    col *= 1.0 - 0.20 * smoothstep(0.45, 0.95, length(p));

    // density master — when low, slightly desaturate non-paper areas to thin the composition
    if (dens < 1.0) {
        float lum = dot(col, vec3(0.299,0.587,0.114));
        col = mix(vec3(lum), col, dens);
    } else if (dens > 1.0) {
        col = clamp(col * (1.0 + 0.15*(dens-1.0)), 0.0, 1.0);
    }

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        col = clamp(hM * col, 0.0, 1.0);
    }
    gl_FragColor = vec4(fidApply(clamp(col, 0.0, 1.0), gl_FragCoord.xy), 1.0);
}
