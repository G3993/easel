/*{
  "DESCRIPTION": "Radiant Geometry — crisp 2.5D geometric shapes (circles, boxes, triangles) lit by six roaming point lights with real-time radial soft shadows. Bass breathes the zoom, mids animate outlines and the traveling ring sequencer, highs flicker accent lights. Layers: back plane / mid ring / front accents / shadow-lit floor. Universal hueShift / colorBoost / bgColor / audioReactivity.",
  "CATEGORIES": ["Generator", "Geometric", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "outlineGlow", "LABEL": "Outlines", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "ringSpin",    "LABEL": "Ring Spin",    "TYPE": "float", "MIN": -6.0, "MAX": 6.0, "DEFAULT": 1.0, "GROUP": "Motion" },
    { "NAME": "shapeTumble", "LABEL": "Shape Tumble", "TYPE": "float", "MIN": -6.0, "MAX": 6.0, "DEFAULT": 1.0, "GROUP": "Motion" },
    { "NAME": "ringWobble",  "LABEL": "Ring Wobble",  "TYPE": "float", "MIN": 0.0,  "MAX": 6.0, "DEFAULT": 1.0, "GROUP": "Motion" },
    { "NAME": "accentSpeed", "LABEL": "Accent Speed", "TYPE": "float", "MIN": 0.0,  "MAX": 5.0, "DEFAULT": 1.0, "GROUP": "Motion" },
    { "NAME": "backPlane",   "LABEL": "Back Plane",  "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Camera / Layout" },
    { "NAME": "midPlane",    "LABEL": "Mid Plane",   "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Camera / Layout" },
    { "NAME": "frontPlane",  "LABEL": "Front Plane", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Camera / Layout" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "GROUP": "Color" },
    { "NAME": "colorBoost",  "LABEL": "Color Boost", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Color" },
    { "NAME": "bgColor",     "LABEL": "Background",  "TYPE": "color", "DEFAULT": [0.0,0.0,0.0,0.0], "GROUP": "Background" },
    { "NAME": "audioReactivity", "LABEL": "Audio React", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Audio Reactivity" },
    { "NAME": "speed",       "LABEL": "Speed",       "TYPE": "float", "MIN": 0.1, "MAX": 3.0, "DEFAULT": 1.0 },
    { "NAME": "brightness",  "LABEL": "Brightness",  "TYPE": "float", "MIN": 0.2, "MAX": 3.0, "DEFAULT": 1.0 },
    { "NAME": "lightRadius", "LABEL": "Light Radius","TYPE": "float", "MIN": 0.01,"MAX": 1.0, "DEFAULT": 0.22,"GROUP": "Lights" },
    { "NAME": "shadowSoft",  "LABEL": "Shadow Soft", "TYPE": "float", "MIN": 0.0, "MAX": 0.15,"DEFAULT": 0.025,"GROUP": "Lights" },
    { "NAME": "tintColor",   "LABEL": "Tint",        "TYPE": "color", "DEFAULT": [1.0,1.0,1.0,1.0], "GROUP": "Color" }
  ],
  "PASSES": [
    { "TARGET": "shadowMap" },
    {}
  ]
}*/

// ═══════════════════════════════════════════════════════════════════════
//  RADIANT GEOMETRY — shadow-lit abstract shapes
//  Pass 0: 1D radial shadow map (6 lights × angle bands)
//  Pass 1: composite — bg + back/mid/front planes lit by shadow map
// ═══════════════════════════════════════════════════════════════════════

#define NUM_LIGHTS 6
#define MAX_STEPS  56
#define TAU        6.28318530718
#define DIST_MAX   2.4

// ── Utilities ──────────────────────────────────────────────────────────
float hash11(float p)  { return fract(sin(p * 127.1) * 43758.5453); }
float hash21(vec2 p)   { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    return vec3(abs(q.z + (q.w - q.y) / (6.0*d + 1e-10)), d / (q.x + 1e-10), q.x);
}
vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

vec2 rot2(vec2 p, float a) {
    float s = sin(a), c = cos(a);
    return vec2(c*p.x - s*p.y, s*p.x + c*p.y);
}
mat2 Rotate(float a) { return mat2(cos(a), sin(a), -sin(a), cos(a)); }

// ── SDFs ───────────────────────────────────────────────────────────────
float sdCircle(vec2 p, float r) { return length(p) - r; }
float sdBox(vec2 p, vec2 b) {
    vec2 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}
float sdTri(vec2 p, float r) {
    float k = 1.7320508;
    p.x = abs(p.x) - r;
    p.y = p.y + r / k;
    if (p.x + k*p.y > 0.0) p = vec2(p.x - k*p.y, -k*p.x - p.y) / 2.0;
    p.x -= clamp(p.x, -2.0*r, 0.0);
    return -length(p) * sign(p.y);
}
float sdRing(vec2 p, float ir, float or_) {
    return abs(length(p) - (ir+or_)*0.5) - (or_-ir)*0.5;
}

// ── Palette ────────────────────────────────────────────────────────────
vec3 palShape(int k) {
    int m = int(mod(float(k), 4.0));
    if (m == 0) return vec3(0.95, 0.42, 0.30);
    if (m == 1) return vec3(0.24, 0.72, 0.66);
    if (m == 2) return vec3(0.98, 0.76, 0.28);
    return vec3(0.88, 0.86, 0.82);
}
vec3 palLight(int id) {
    if (id == 0) return vec3(1.0, 0.25, 0.20);
    if (id == 1) return vec3(1.0, 0.60, 0.55);
    if (id == 2) return vec3(0.40, 1.0, 0.60);
    if (id == 3) return vec3(0.45, 0.50, 1.0);
    if (id == 4) return vec3(1.0, 1.0, 0.40);
    return vec3(1.0, 0.40, 1.0);
}

// ── Audio helpers (evaluated identically in both passes) ───────────────
float gBass, gMid, gHigh, gLevel, gMT, aR;

void initAudio() {
    aR     = clamp(audioReactivity, 0.0, 2.0);
    gMT    = TIME * speed;
    gBass  = pow(clamp((audioBass  - 0.05) / 0.80, 0.0, 1.0), 1.6);
    gMid   = pow(clamp((audioMid   - 0.08) / 0.82, 0.0, 1.0), 1.3);
    gHigh  = pow(clamp((audioHigh  - 0.10) / 0.80, 0.0, 1.0), 1.2);
    gLevel = clamp((audioLevel - 0.05) / 0.85, 0.0, 1.0);
}

// ── Light positions (shared) ───────────────────────────────────────────
vec2 lightPos(int id) {
    float R = lightRadius;
    if (id == 0) return vec2(0.0, 0.0);
    if (id == 1) return vec2(cos(gMT*0.37), sin(gMT*0.23)) * R;
    if (id == 2) return -vec2(cos(gMT*0.29), sin(gMT*0.41)) * R;
    if (id == 3) {
        float a = -gMT * 0.31;
        return vec2(cos(a), sin(a)) * (R * 0.7 + 0.04 * aR * gBass);
    }
    if (id == 4) return vec2( 0.46, sin(3.0*gMT - TAU/4.0) * R * 0.65);
    return           vec2(-0.46, sin(3.0*gMT)              * R * 0.65);
}

vec3 lightColor(int id) {
    vec3 c = palLight(id);
    if (id == 0) c *= (sin(gMT*4.0)*1.5 + 2.5) * mix(1.0, 0.4 + 2.2*gBass, aR*0.6);
    else if (id == 1 || id == 2) c *= 2.2 * mix(1.0, 0.5 + 1.6*gMid,  aR*0.6);
    else if (id == 3) c *= 3.5 * mix(1.0, 0.5 + 1.5*gBass, aR*0.6);
    else              c *= 2.0 * mix(1.0, 0.4 + 2.0*gHigh,  aR*0.6);
    return c;
}

// ── Shadow-map scene (obstacles the lights must navigate) ──────────────
float shadowScene(vec2 uv) {
    // ring obstacle + spinning bar + pillars
    float d = sdRing(uv, 0.25, 0.27);
    d = min(d, sdBox(uv * Rotate(gMT + 0.2*aR*gBass), vec2(0.13, 0.025)));
    // repeating dot pillars
    vec2 rp = mod(uv, vec2(0.20)) - 0.10;
    d = min(d, sdCircle(rp, 0.018));
    return d;
}

// ═══════════════════════════════════════════════════════════════════════
//  PASS 0 — Radial shadow map
// ═══════════════════════════════════════════════════════════════════════
vec4 passShadow() {
    float fx = gl_FragCoord.x * float(NUM_LIGHTS) / RENDERSIZE.x;
    int   id = int(min(floor(fx), float(NUM_LIGHTS - 1)));
    float a  = fract(fx) * TAU;
    vec2  dir  = vec2(cos(a), sin(a));
    vec2  orig = lightPos(id);
    float d = 0.0;
    for (int i = 0; i < MAX_STEPS; i++) {
        float ds = shadowScene(orig + dir * d);
        d += max(ds, 0.0005);
        if (ds < 8e-5 || d > DIST_MAX) break;
    }
    d = clamp(d / DIST_MAX, 0.0, 1.0) * 255.0;
    return vec4(floor(d)/255.0, fract(d), 0.0, 1.0);
}

// ═══════════════════════════════════════════════════════════════════════
//  PASS 1 — Full composite
// ═══════════════════════════════════════════════════════════════════════

float sampleShadow(int id, vec2 rel) {
    float ang  = fract(atan(rel.y, rel.x) / TAU + 0.5);
    float bandW = RENDERSIZE.x / float(NUM_LIGHTS);
    float x     = (float(id) + clamp(ang, 0.001, 0.999)) * bandW;
    x = (floor(x) + 0.5) / RENDERSIZE.x;
    vec4 sm = IMG_PIXEL(shadowMap, vec2(x * RENDERSIZE.x, RENDERSIZE.y * 0.5));
    float dist = (sm.r + sm.g / 255.0) * DIST_MAX;
    return 1.0 - smoothstep(dist, dist + shadowSoft, length(rel));
}

vec3 litAt(vec2 uv) {
    vec3 b = vec3(0.06, 0.06, 0.09) * mix(1.0, 0.5 + 1.5*gLevel, aR*0.5);
    for (int i = 0; i < NUM_LIGHTS; i++) {
        vec2 rel = uv - lightPos(i);
        float att = 0.012 / (dot(rel,rel) + 0.001);
        att *= sampleShadow(i, rel);
        b += lightColor(i) * att;
    }
    return b;
}

vec4 passImage() {
    vec2 uv   = isf_FragNormCoord.xy;
    vec2 p0   = uv - 0.5;
    p0.x     *= RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    float px  = 1.0 / max(RENDERSIZE.y, 1.0);

    float bass = clamp(audioBass, 0.0, 1.0);
    float mid  = clamp(audioMid,  0.0, 1.0);
    float high = clamp(audioHigh, 0.0, 1.0);

    float zoom = 1.0 + 0.030*sin(TIME*0.23) + 0.055*bass*aR;
    vec2 p = p0 / zoom;
    p = rot2(p, 0.04*sin(TIME*0.11));

    // ── Background ──────────────────────────────────────────────────────
    vec3 bg = mix(vec3(0.07, 0.08, 0.13), vec3(0.18, 0.17, 0.24),
                  clamp(uv.y + 0.18*sin(TIME*0.07 + uv.x*2.0), 0.0, 1.0));
    bg += vec3(0.05,0.02,-0.02)*sin(uv.x*3.1 + TIME*0.05)
        + vec3(-0.02,0.03,0.05)*sin(uv.y*2.7 - TIME*0.04 + 1.3);

    // dot lattice
    float pxl  = px * 11.0;
    vec2 lat   = fract(p0 * 11.0) - 0.5;
    bg += vec3(0.18,0.19,0.24) * smoothstep(0.042+pxl, 0.042-pxl, length(lat));
    vec2 lat2  = fract(p0 * 22.0 + 0.5) - 0.5;
    float pxl2 = px * 22.0;
    bg += vec3(0.08,0.09,0.12) * smoothstep(0.030+pxl2, 0.030-pxl2, length(lat2));
    vec2 gd = abs(lat);
    bg += vec3(0.04,0.045,0.06) * smoothstep(1.2*pxl, 0.4*pxl, min(gd.x, gd.y));
    bg  = mix(bg, bgColor.rgb, clamp(bgColor.a, 0.0, 1.0));

    // Apply shadow-map lighting to bg (room illumination)
    vec3 litBG = litAt(p) * brightness * tintColor.rgb;
    bg *= (vec3(0.25) + litBG * 0.75);

    vec3 col = bg;

    float ow  = 0.0075 * (1.0 + 0.15*sin(TIME*0.5)) * (1.0 + 1.1*mid*aR);
    float owF = ow * clamp(outlineGlow, 0.0, 2.0);

    vec2 pan = vec2(sin(TIME*0.09), cos(TIME*0.07)) * 0.10;

    // ══ BACK PLANE ═══════════════════════════════════════════════════════
    for (int i = 0; i < 3; i++) {
        float fi = float(i);
        vec2 q = p - pan*0.35
               - vec2(cos(fi*2.09 + TIME*0.05), sin(fi*2.09 + TIME*0.04)) * 0.44;
        q = rot2(q, TIME*0.03*shapeTumble + fi*1.3);
        float sd;
        if (i == 0)      sd = sdCircle(q, 0.38);
        else if (i == 1) sd = sdBox(q, vec2(0.33, 0.27));
        else             sd = sdTri(q, 0.36);

        // light the shape's centroid position
        vec2 cPos = p0 - pan*0.35
                  - vec2(cos(fi*2.09 + TIME*0.05), sin(fi*2.09 + TIME*0.04)) * 0.44;
        vec3 lit   = litAt(cPos) * brightness * tintColor.rgb;
        float fill = smoothstep(1.5*px, -1.5*px, sd);
        vec3 sc    = palShape(i) * 0.40 * (vec3(0.15) + lit);
        col = mix(col, sc, fill * 0.85 * clamp(backPlane, 0.0, 2.0) * 0.5);

        // drop shadow blob behind shape
        float shadow = smoothstep(0.0, 0.22, sd + 0.06) * 0.35;
        col = mix(col, vec3(0.0), shadow * clamp(backPlane, 0.0, 2.0) * 0.5);

        float olc = smoothstep(max(owF*0.5, 1.2*px)+px, max(owF*0.5, 1.2*px)-px, abs(sd));
        float olg = smoothstep(owF*2.2, 0.0, abs(sd));
        col += palShape(i) * (olc*0.18 + olg*0.06) * clamp(backPlane, 0.0, 2.0) * 0.5;
    }

    // ══ MID PLANE — 8-shape ring sequencer ═══════════════════════════════
    float w8     = 0.5 - 0.5*cos(TAU * audioPhase8);
    float ringRot = TIME*0.06*ringSpin + 0.42*w8;
    float seq    = audioPhase4 * 8.0;

    for (int k = 0; k < 8; k++) {
        float fk  = float(k);
        float ang  = fk * 0.7853982 + ringRot;
        vec2 cPos = vec2(cos(ang), sin(ang)) * (0.335 + 0.025*ringWobble*sin(TIME*0.19 + fk));
        vec2 q    = p - pan*0.7 - cPos;
        q = rot2(q, -ang*0.5 + TIME*0.08*shapeTumble);

        float sz = 0.105 * (1.0 + 0.06*sin(TIME*0.31 + fk*1.9));
        float sd;
        int m = int(mod(fk, 3.0));
        if (m == 0)      sd = sdCircle(q, sz);
        else if (m == 1) sd = sdBox(q, vec2(sz*0.9));
        else             sd = sdTri(q, sz*1.1);

        // traveling highlight
        float ds2 = abs(seq - fk);
        ds2 = min(ds2, 8.0 - ds2);
        float lit2 = smoothstep(1.4, 0.0, ds2);

        // shadow-map lighting at shape center
        vec3 shapeLight = litAt(cPos) * brightness * tintColor.rgb;

        float fillLum = (0.55 + 0.55*lit2) * (1.0 - 0.28*clamp(audioMidHit, 0.0, 1.0)*aR);
        float fill    = smoothstep(1.5*px, -1.5*px, sd);
        vec3 sc       = palShape(k) * fillLum * (vec3(0.20) + shapeLight * 0.80);
        sc *= 1.0 + 0.40 * clamp(q.y / max(sz, 0.001), -1.0, 1.0);

        // soft drop shadow
        float shBlob = smoothstep(0.0, sz*1.8, sd + sz*0.5) * 0.45;
        col = mix(col, vec3(0.0), shBlob * clamp(midPlane, 0.0, 2.0) * 0.5);

        col = mix(col, sc, fill * clamp(midPlane, 0.0, 2.0) * 0.5 * 1.6);

        float lw  = max(owF*0.6, 1.2*px);
        float olc = smoothstep(lw+px, lw-px, abs(sd));
        float olg = smoothstep(owF*2.0, 0.0, abs(sd));
        vec3 oc   = palShape(k) * (vec3(0.3) + shapeLight*0.5);
        col += oc * (olc*0.90 + olg*0.15) * (0.35 + 0.65*lit2)
             * clamp(outlineGlow, 0.0, 2.0) * 0.6;

        // echo ring
        float ed   = abs(sd - sz*0.45);
        float echo = smoothstep(1.2*px+px, 1.2*px-px, ed)*0.70
                   + smoothstep(owF*1.2, 0.0, ed)*0.22;
        col += palShape(k) * echo * 0.30 * clamp(outlineGlow, 0.0, 2.0) * 0.6;

        // inset hairline
        float insd  = abs(sd + sz*0.34);
        float inset = smoothstep(1.1*px+px, 1.1*px-px, insd);
        col += palShape(k) * inset * 0.28 * fill * clamp(midPlane, 0.0, 2.0) * 0.5;
    }

    // ══ FRONT PLANE — small accent shapes ════════════════════════════════
    for (int i = 0; i < 3; i++) {
        float fi = float(i);
        float h  = hash11(fi*5.7 + 3.1);
        vec2 cPos = vec2(sin(TIME*accentSpeed*(0.12 + 0.05*h) + fi*2.4),
                         cos(TIME*accentSpeed*(0.10 + 0.04*h) + fi*1.9)) * vec2(0.46, 0.36);
        vec2 q = p - pan*1.6 - cPos;
        q = rot2(q, TIME*0.2*shapeTumble + fi*2.0);

        float sd;
        if (i == 0)      sd = sdCircle(q, 0.035);
        else if (i == 1) sd = sdBox(q, vec2(0.030));
        else             sd = sdTri(q, 0.038);

        vec3 shapeLight = litAt(cPos) * brightness * tintColor.rgb;
        float lwF = max(owF*0.6, 1.2*px);
        float olc = smoothstep(lwF+px, lwF-px, abs(sd));
        float olg = smoothstep(owF*1.8, 0.0, abs(sd));
        col += palShape(i+1) * (vec3(0.3)+shapeLight*0.5)
             * (olc*0.70 + olg*0.16) * clamp(frontPlane, 0.0, 2.0) * 0.6;
        float fill = smoothstep(1.5*px, -1.5*px, sd);
        vec3 fc = palShape(i+2) * (vec3(0.2)+shapeLight*0.7) * 0.9;
        col = mix(col, fc, fill * 0.55 * clamp(frontPlane, 0.0, 2.0) * 0.5);

        // light-halo glow around accent shapes
        float halo = smoothstep(0.12, 0.0, abs(sd)) * 0.18;
        col += palShape(i+1) * (vec3(0.2)+shapeLight*0.6) * halo
             * clamp(frontPlane, 0.0, 2.0);
    }

    // ── Light flare dots at each light position ──────────────────────────
    for (int i = 0; i < NUM_LIGHTS; i++) {
        vec2 lp  = lightPos(i) * vec2(1.0, RENDERSIZE.y / max(RENDERSIZE.x, 1.0));
        vec2 q   = p - lp;
        float r  = length(q);
        float flare = 0.0008 / (r*r + 0.0002);
        flare = min(flare, 1.8);
        col += lightColor(i) * flare * 0.28 * brightness;
    }

    // ── Whole-frame gain ─────────────────────────────────────────────────
    float beatRamp = smoothstep(0.0, 0.25, audioBeatPhase);
    float gain = (0.28*bass + 0.20*mid + 0.14*high
                + 0.22*clamp(audioBeatPulse,0.0,1.0)*beatRamp
                + 0.14*clamp(audioPunch,0.0,1.0)*beatRamp) * 0.44 * aR;
    col *= 1.0 + gain;

    // ── Hue shift / color boost ──────────────────────────────────────────
    if (hueShift > 0.001) {
        vec3 hsv = rgb2hsv(clamp(col, 0.0, 2.5));
        hsv.x = fract(hsv.x + hueShift);
        col = hsv2rgb(hsv);
    }
    float luma = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(luma), col, clamp(colorBoost, 0.0, 2.0));

    // ── Vignette ─────────────────────────────────────────────────────────
    col *= 1.0 - 0.38 * smoothstep(0.28, 0.92, dot(p0, p0));

    // ── Film grain (static per-pixel, no temporal flicker) ───────────────
    float gr = hash21(uv * RENDERSIZE.xy) - 0.5;
    col += gr * 0.048;

    return vec4(clamp(col, 0.0, 1.0), 1.0);
}

void main() {
    initAudio();
    if (PASSINDEX == 0) {
        gl_FragColor = passShadow();
    } else {
        gl_FragColor = passImage();
    }
}