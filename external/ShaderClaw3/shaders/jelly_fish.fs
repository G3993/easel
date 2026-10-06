/*{
  "DESCRIPTION": "Jelly Fish — a school of medusae wandering through deep water in 3D. Each one drifts on its own path, tilts into its motion, pulses on its own beat and trails long wiggling tentacles; two hues spread across the school, with bioluminescent halos, plankton and light shafts, then bloom. Bass drives the pulses, beats flash the glow, mids add wiggle, highs sparkle the water.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-30 — original",
  "CATEGORIES": ["Generator", "3D", "Organic", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "count",       "LABEL": "Jellies",          "TYPE": "float", "MIN": 1.0,  "MAX": 8.0,  "DEFAULT": 5.0,  "GROUP": "School" },
    { "NAME": "size",        "LABEL": "Size",             "TYPE": "float", "MIN": 0.3,  "MAX": 1.5,  "DEFAULT": 0.8,  "GROUP": "School" },
    { "NAME": "spread",      "LABEL": "Spread",           "TYPE": "float", "MIN": 0.5,  "MAX": 4.0,  "DEFAULT": 2.2,  "GROUP": "School" },
    { "NAME": "wander",      "LABEL": "Wander",           "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.5,  "GROUP": "Motion" },
    { "NAME": "pattern",     "LABEL": "Bell Pattern",     "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Pattern" },
    { "NAME": "patternSpeed","LABEL": "Pattern Speed",    "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Pattern" },
    { "NAME": "speed",       "LABEL": "Speed",            "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 0.7,  "GROUP": "Motion" },
    { "NAME": "pulse",       "LABEL": "Pulse",            "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "wiggle",      "LABEL": "Wiggle",           "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "orbit",       "LABEL": "Camera Orbit",     "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.5,  "GROUP": "Motion" },
    { "NAME": "tentLen",     "LABEL": "Tentacle Length",  "TYPE": "float", "MIN": 0.3,  "MAX": 5.0,  "DEFAULT": 2.4,  "GROUP": "Body" },
    { "NAME": "hueA",        "LABEL": "Hue A",            "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.49, "GROUP": "Color" },
    { "NAME": "hueB",        "LABEL": "Hue B",            "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.60, "GROUP": "Color" },
    { "NAME": "iridescence", "LABEL": "Iridescence",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.65, "GROUP": "Color" },
    { "NAME": "glow",        "LABEL": "Glow",             "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "fog",         "LABEL": "Depth Fog",        "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Color" }
  ],
  "PASSES": [
    { "TARGET": "jsScene" },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// JELLY FISH — the school
//   pass 0  jsScene — up to 8 raymarched medusae, each on its own wander
//                     path with its own pulse phase; bell shell + oral arm +
//                     a ring of 16 wiggling tentacles per jelly; a distance-
//                     field halo gives the bioluminescent glow. Alpha = emissive.
//   pass 1  final   — bloom + HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R    RENDERSIZE.xy
#define TAU  6.28318530718
#define MAXJ 8

float bassP()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.4); }
float midP()   { return pow(smoothstep(0.06, 0.85, audioMid),  1.2); }
float highP()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }
float beatP()  { return clamp(audioBeatPulse, 0.0, 1.0); }
float levP()   { return clamp(audioLevel, 0.0, 1.0); }

float hash11(float p) { p = fract(p * 0.1031); p *= p + 33.33; p *= p + p; return fract(p); }
float hash12(vec2 p)  { vec3 p3 = fract(vec3(p.xyx) * 0.1031); p3 += dot(p3, p3.yzx + 33.33); return fract((p3.x + p3.y) * p3.z); }
float hash13(vec3 p3) { p3 = fract(p3 * 0.1031); p3 += dot(p3, p3.zyx + 31.32); return fract((p3.x + p3.y) * p3.z); }
float vnoise2(vec2 x) {
    vec2 p = floor(x), f = fract(x); f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash12(p), hash12(p + vec2(1, 0)), f.x), mix(hash12(p + vec2(0, 1)), hash12(p + vec2(1, 1)), f.x), f.y);
}
float vnoise3(vec3 x) {
    vec3 p = floor(x), f = fract(x); f = f * f * (3.0 - 2.0 * f);
    return mix(mix(mix(hash13(p), hash13(p + vec3(1, 0, 0)), f.x), mix(hash13(p + vec3(0, 1, 0)), hash13(p + vec3(1, 1, 0)), f.x), f.y),
               mix(mix(hash13(p + vec3(0, 0, 1)), hash13(p + vec3(1, 0, 1)), f.x), mix(hash13(p + vec3(0, 1, 1)), hash13(p + vec3(1, 1, 1)), f.x), f.y), f.z);
}
vec3 hsv2rgb(vec3 c) { vec3 p = abs(fract(c.xxx + vec3(0.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0); return c.z * mix(vec3(1.0), clamp(p - 1.0, 0.0, 1.0), c.y); }
mat2 rot(float a) { float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }

// ── per-frame state ──────────────────────────────────────────────────────
float g_ar, g_t, g_wig, g_glowK;
int   g_n;
vec3  g_pos[MAXJ];     // world position of each jelly
vec3  g_vel[MAXJ];     // its velocity (for the tilt)
float g_pulse[MAXJ];   // 0..1 contraction
float g_hue[MAXJ];     // its hue
float g_seed[MAXJ];

float pulseCurve(float t) {
    float ph = fract(t);
    // softer squeeze, long relax — floaty, never a hop
    float squeeze = exp(-pow((ph - 0.22) * 3.6, 2.0));
    float relax   = smoothstep(0.3, 1.0, ph) * (1.0 - smoothstep(0.3, 1.0, ph)) * 0.6;
    return squeeze - relax * 0.3;
}
// a jelly's wander: a sum of slow sines inside the school's box, nudged
// upward by each pulse (they swim by squeezing)
vec3 wanderPath(float seed, float t) {
    float w1 = 0.06 + 0.04 * seed, w2 = 0.05 + 0.03 * hash11(seed + 2.0), w3 = 0.07 + 0.04 * hash11(seed + 4.0);
    vec3 ph = vec3(seed, hash11(seed + 1.0), hash11(seed + 3.0)) * 40.0;
    // three octaves per axis: a slow roam, a mid wobble and a quick wiggle,
    // so no two jellies ever trace the same loop and they drift through the
    // whole box, up and down included
    // only slow octaves: everything moves on long, lazy arcs
    vec3 p = vec3(sin(t * w1 + ph.x) + 0.35 * sin(t * w1 * 1.6 + ph.y) + 0.6 * sin(t * w2 * 0.37 + ph.z),
                  0.7 * sin(t * w2 + ph.y) + 0.25 * sin(t * w2 * 1.4 + ph.z) + 0.45 * sin(t * w3 * 0.29 + ph.x),
                  sin(t * w3 + ph.z) + 0.35 * cos(t * w3 * 1.5 + ph.x) + 0.6 * sin(t * w1 * 0.41 + ph.y));
    return p * vec3(1.2, 0.85, 1.2) * spread * wander;
}
void setupSchool() {
    g_n = int(clamp(count + 0.5, 1.0, float(MAXJ)));
    float t = g_t * speed;
    for (int i = 0; i < MAXJ; i++) {
        if (i >= g_n) break;
        float seed = hash11(float(i) * 7.31 + 1.0);
        g_seed[i] = seed;
        // spread the school across the box with a fixed home per jelly
        // the home matters less the more they roam
        vec3 home = (vec3(hash11(seed + 10.0), hash11(seed + 20.0), hash11(seed + 30.0)) - 0.5) * vec3(2.4, 1.2, 2.4) * spread * 0.6 / (1.0 + 0.5 * wander);
        float pt = t * (0.45 + 0.3 * seed) * pulse * 0.6 + seed * 3.0 + g_ar * 0.25 * audioBassTime;
        float pu = clamp(pulseCurve(pt) * (1.0 + g_ar * 0.5 * bassP()), 0.0, 1.0);
        g_pulse[i] = pu;
        vec3 p0 = home + wanderPath(seed, t);
        vec3 p1 = home + wanderPath(seed, t + 2.0);
        // the swim stroke lifts the body on a soft cosine, not on the squeeze itself
        float lift = 0.5 - 0.5 * cos(fract(pt) * TAU);
        g_pos[i] = p0 + vec3(0.0, 0.10 * lift, 0.0);
        // velocity over a 2 s window = a smooth heading for the tilt
        g_vel[i] = clamp((p1 - p0) * 0.5, -1.0, 1.0);
        g_hue[i] = mix(hueA, hueB, fract(seed * 1.7 + float(i) * 0.37));
    }
}

// ── one jelly in its LOCAL frame (bell apex up, origin at the bell centre) ──
float g_mat;        // 1 bell, 2 arm, 3 tentacle
float g_matHue;
float jellySDF(vec3 p, int i, float sz) {
    float pu = g_pulse[i];
    float seed = g_seed[i];
    vec3 s = vec3(1.0 - 0.18 * pu, 0.72 + 0.26 * pu, 1.0 - 0.18 * pu) * sz;
    vec3 q = p / s;
    float ang = atan(q.z, q.x);
    float rimW = smoothstep(0.3, -0.3, q.y);
    float fr = sin(ang * 10.0 + sin(ang * 3.0 + g_t * 0.8 + seed * 9.0) * 0.8) * 0.05 * rimW;
    float skin = (vnoise3(q * 8.0 + vec3(seed * 10.0, g_t * 0.3, 0.0)) - 0.5) * 0.025;
    float outer = length(q) - 1.0 - fr - skin;
    float inner = length(q - vec3(0.0, -0.2, 0.0)) - 0.88 - fr * 0.5;
    float bell = max(max(outer, -inner), -(q.y + 0.4 - fr)) * min(s.x, s.y);
    g_mat = 1.0;
    float d = bell;

    // oral arm: one thick wavy strand from the centre
    {
        vec3 a = p - vec3(0.0, -0.25 * sz, 0.0);
        float len = tentLen * 0.55 * sz;
        float yy = clamp(-a.y, 0.0, len);
        float u = yy / len;
        float ph = g_t * 1.5 + seed * 6.3;
        vec2 off = vec2(sin(yy * 2.2 - ph), cos(yy * 1.8 - ph * 0.9)) * 0.18 * sz * (0.3 + u) * g_wig;
        float da = length(a - vec3(off.x, -yy, off.y)) - 0.09 * sz * mix(1.0, 0.3, u);
        da -= (vnoise3(p * 9.0 / sz + seed) - 0.5) * 0.03 * sz;
        if (da < d) { d = da; g_mat = 2.0; }
    }
    // tentacle ring: 16 strands by angular repetition, each with its own wiggle
    {
        float n = 16.0, sec = TAU / n;
        float id = floor((ang + sec * 0.5) / sec);
        float a2 = ang - id * sec;
        float rr = length(p.xz);
        vec3 q2 = vec3(rr * cos(a2), p.y, rr * sin(a2));
        float ts = hash11(id * 3.1 + seed * 50.0);
        vec3 o = vec3((0.95 - 0.16 * pu) * sz, -0.38 * sz, 0.0);
        vec3 t2 = q2 - o;
        float len = tentLen * sz * (0.6 + 0.6 * ts);
        float yy = clamp(-t2.y, 0.0, len);
        float u = yy / len;
        float ph = g_t * 1.7 + ts * 6.3 + seed * 4.0;
        vec2 off = vec2(sin(yy * 2.6 - ph), cos(yy * 2.1 - ph * 0.8)) * 0.3 * sz * (0.2 + u) * g_wig
                 + vec2(sin(yy * 7.0 + ph * 1.6), cos(yy * 6.1 - ph * 1.2)) * 0.05 * sz * u * g_wig;
        float r = 0.02 * sz * mix(1.0, 0.3, u);
        float dt = length(t2 - vec3(off.x, -yy, off.y)) - r;
        float beads = sin(yy * 16.0 + ts * 9.0) * 0.5 + 0.5;
        dt -= beads * beads * 0.004 * sz * step(0.01, yy) * step(yy, len - 0.01);
        if (dt < d) { d = dt; g_mat = 3.0; }
    }
    return d;
}

// world map: every jelly in its own tilted frame. Also tracks the nearest
// bell distance for the halo.
float g_halo;
int   g_hitJ;
float map(vec3 p) {
    float best = 1e9;
    float bestMat = 1.0, bestHue = 0.0;
    int   bestJ = 0;
    g_halo = 1e9;
    for (int i = 0; i < MAXJ; i++) {
        if (i >= g_n) break;
        vec3 l = p - g_pos[i];
        // cheap bound: skip jellies far from this point
        float sz = size * (0.75 + 0.5 * g_seed[i]);
        float bound = length(vec3(l.x, l.y + tentLen * sz * 0.6, l.z)) - (tentLen * 1.7 + 1.6) * sz;
        if (bound > 0.3) { best = min(best, bound); continue; }
        // tilt into the motion
        vec3 v = g_vel[i];
        l.xz = rot(-0.35 * v.x) * l.xz;
        l.yz = rot( 0.35 * v.z) * l.yz;
        float d = jellySDF(l, i, sz);
        g_halo = min(g_halo, length(l * vec3(1.0, 1.4, 1.0)) - sz);   // bell-ish halo distance
        if (d < best) { best = d; bestMat = g_mat; bestHue = g_hue[i]; bestJ = i; }
    }
    g_mat = bestMat; g_matHue = bestHue; g_hitJ = bestJ;
    return best;
}
vec3 calcNormal(vec3 p, float e) {
    vec2 k = vec2(1.0, -1.0) * e;
    return normalize(k.xyy * map(p + k.xyy) + k.yyx * map(p + k.yyx) + k.yxy * map(p + k.yxy) + k.xxx * map(p + k.xxx));
}

// ── water ────────────────────────────────────────────────────────────────
vec3 waterBg(vec3 rd, vec2 uv) {
    float up = clamp(rd.y * 0.5 + 0.5, 0.0, 1.0);
    vec3 col = mix(vec3(0.0, 0.006, 0.014), vec3(0.01, 0.12, 0.2), pow(up, 2.2));
    float sx = uv.x / (0.6 + up * 1.2);
    float st = vnoise2(vec2(sx * 7.0 + g_t * 0.05, up * 1.5)) * vnoise2(vec2(sx * 19.0 - g_t * 0.08, up * 0.7 + 3.0));
    col += vec3(0.25, 0.55, 0.6) * pow(st, 2.5) * pow(up, 3.0) * 1.3;
    return col;
}
float planktonLayer(vec2 uv, float scale, float sp, float seed) {
    vec2 g = uv * scale + vec2(seed, g_t * sp);
    vec2 cell = floor(g), f = fract(g);
    float h = hash12(cell + seed);
    vec2 c = vec2(hash12(cell + 1.3 + seed), hash12(cell + 2.7 + seed));
    float d = length(f - c);
    float tw = 0.6 + 0.4 * sin(g_t * (2.0 + 3.0 * h) + h * 20.0);
    return step(0.74, h) * exp(-d * d * 900.0 / (0.5 + h)) * tw;
}

vec4 renderScene(vec3 ro, vec3 rd, vec2 uv) {
    vec3 lightDir = normalize(vec3(0.3, 1.0, -0.2));
    vec3 bg = waterBg(rd, uv);
    float pk = planktonLayer(uv, 14.0, 0.03, 1.0) * 0.6 + planktonLayer(uv, 26.0, 0.05, 2.0) * 0.4 + planktonLayer(uv, 48.0, 0.08, 3.0) * 0.25;
    pk *= 1.0 + g_ar * 0.8 * highP();
    bg += vec3(0.5, 0.9, 1.0) * pk;

    float t = 0.6, tMax = 22.0;
    float mat = 0.0, hueHit = 0.0;
    vec3 halo = vec3(0.0);
    for (int i = 0; i < 96; i++) {
        vec3 p = ro + rd * t;
        float d = map(p);
        // bioluminescent halo accumulates where the ray passes near a bell
        halo += hsv2rgb(vec3(g_matHue, 0.7, 1.0)) * exp(-max(g_halo, 0.0) * 6.0) * 0.012 * g_glowK;
        if (d < 0.0015 * t) { mat = g_mat; hueHit = g_matHue; break; }
        t += d * 0.7;
        if (t > tMax) break;
    }
    if (mat < 0.5) return vec4(bg + halo, pk * 0.6 + length(halo) * 0.8);

    vec3 p = ro + rd * t;
    vec3 n = calcNormal(p, 0.002);
    float fres = pow(1.0 - max(dot(-rd, n), 0.0), 3.0);
    float ndl  = max(dot(n, lightDir), 0.0);
    float spec = pow(max(dot(reflect(rd, n), lightDir), 0.0), 32.0);
    // iridescence: the hue slides with the viewing angle like a thin film —
    // teal face-on, blue toward the rim, a violet flash at grazing angles —
    // and shimmers slowly across the surface
    float shimmer = vnoise3(p * 3.0 + vec3(0.0, g_t * 0.4, 0.0)) - 0.5;
    float hShift = iridescence * (0.16 * fres - 0.05 * ndl + 0.06 * shimmer);
    vec3 body = hsv2rgb(vec3(fract(hueHit + hShift), 0.72 - 0.2 * fres, 1.0));
    vec3 glowC = hsv2rgb(vec3(fract(hueHit + 0.10 + hShift * 1.5), 0.8, 1.0));
    vec3 col; float emis;
    if (mat < 1.5) {
        // bell: flesh that reads translucent — dark core, bright rim, inner glow at the apex
        int j = g_hitJ;
        float sz = size * (0.75 + 0.5 * g_seed[j]);
        vec3 l = p - g_pos[j];
        vec3 v = g_vel[j];
        l.xz = rot(-0.35 * v.x) * l.xz;
        l.yz = rot( 0.35 * v.z) * l.yz;
        vec3 q = l / sz;
        float apex = smoothstep(-0.3, 1.0, q.y);
        col = body * (0.12 + 0.35 * ndl) * (0.6 + 0.4 * apex) + glowC * apex * 0.35 * g_glowK;
        col += fres * mix(body, vec3(1.0), 0.5) * 1.6 + spec * 1.1;
        emis = 0.25 * apex * g_glowK + fres * 0.6;
        // ── moving patterns on the bell: radial stripes that flow toward the
        //    rim, drifting bioluminescent spots, and rings sweeping down ──
        if (pattern > 0.001) {
            float ang = atan(q.z, q.x);
            float ps = g_t * patternSpeed + g_seed[j] * 9.0;
            float st = sin(ang * 14.0 + sin(q.y * 5.0 - ps * 1.5) * 1.2 - ps * 0.7);
            st = smoothstep(0.6, 0.97, st);
            vec2 g = vec2(ang / TAU * 12.0, q.y * 6.0 + ps * 0.6);
            vec2 cell = floor(g), f = fract(g);
            vec2 c = vec2(hash12(cell + g_seed[j]), hash12(cell + 3.1 + g_seed[j]));
            float spot = exp(-dot(f - c, f - c) * 45.0) * step(0.45, hash12(cell + 7.0 + g_seed[j]));
            float ring = smoothstep(0.75, 1.0, sin(q.y * 9.0 - ps * 2.0));
            float pat = (st * 0.6 + spot * 1.3 + ring * 0.45) * pattern * smoothstep(-0.45, 0.15, q.y);
            pat *= 1.0 + g_ar * 0.6 * beatP();
            col += glowC * pat * 0.9 * g_glowK + body * pat * 0.2;
            emis += pat * 0.6 * g_glowK;
        }
    } else if (mat < 2.5) {
        col = mix(body, glowC, 0.4) * (0.3 + 0.7 * ndl) + fres * glowC * 1.3 + spec * 0.7;
        emis = 0.4 + fres * 0.5;
    } else {
        col = mix(body, glowC, 0.5) * (0.45 + 0.55 * ndl) + fres * glowC * 1.6 + spec * 0.5;
        emis = 0.55 + fres * 0.6;
    }
    emis *= glow;
    float fg = 1.0 - exp(-t * 0.05 * fog);
    col = mix(col, bg, fg * 0.6) + halo;
    return vec4(col, emis * (1.0 - fg * 0.5) + length(halo) * 0.5);
}

void main() {
    vec2 fragCoord = gl_FragCoord.xy;
    vec2 uv = (fragCoord - 0.5 * R) / R.y;
    g_ar = audioReact;
    g_t = TIME;
    g_wig = wiggle * (1.0 + g_ar * 0.8 * midP());
    g_glowK = glow * (1.0 + g_ar * 1.4 * beatP());
    setupSchool();

    if (PASSINDEX == 0) {
        float oa = TIME * 0.06 * orbit;
        float dist = 5.0 + spread * 1.4;
        vec3 ro = vec3(sin(oa) * dist, 0.6 + 0.3 * sin(TIME * 0.17), cos(oa) * dist);
        vec3 ta = vec3(0.0, -0.3, 0.0);
        vec3 ww = normalize(ta - ro);
        vec3 uu = normalize(cross(vec3(0.0, 1.0, 0.0), ww));
        vec3 vv = cross(ww, uu);
        vec3 rd = normalize(uu * uv.x + vv * uv.y + ww * 1.5);
        vec4 sc = renderScene(ro, rd, uv);
        gl_FragColor = vec4(max(sc.rgb, 0.0), clamp(sc.a, 0.0, 4.0));
    } else {
        vec2 st = fragCoord / R;
        vec4 c0 = texture2D(jsScene, st);
        vec3 bl = vec3(0.0);
        float rad = 0.02;
        for (int i = 0; i < 16; i++) {
            float fi = float(i);
            float a = fi * 2.399963 + hash12(fragCoord) * 6.28;
            float rr = sqrt((fi + 0.5) / 16.0) * rad;
            vec2 o = vec2(cos(a), sin(a)) * rr * vec2(R.y / R.x, 1.0);
            vec4 s = texture2D(jsScene, st + o);
            bl += s.rgb * (1.0 - rr / rad) * s.a;
        }
        bl /= 16.0;
        vec3 col = c0.rgb + bl * 0.9 * glow;
        col = col / (1.0 + col * 0.35);
        float lvl = levP();
        col *= mix(1.0, 0.82 + 0.25 * lvl, g_ar);
        col += (hash12(fragCoord + fract(TIME) * 61.0) - 0.5) * 0.006;
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.4);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.15), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.18, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
