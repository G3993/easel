/*{
  "DESCRIPTION": "Ring Tunnel — three tunnels flown through at once and fused on the same rays: a ribbed throat of stacked iris plates (rounded ribs with gaps, cold core glow, warm rim), a streak field of thin neon line-boxes bending along a snaking path (Jan Mróz), and a radial grid of comet capsules whose skins carry a live 2D fluid ink that is re-inked each time a capsule fires, with neon comet heads lighting the walls. Speed drives all three; each system has its own colors, fade distance and light amount, and a distant color tints the far end. Bass breathes the iris and pumps the core, mids wobble the ribs and churn the ink, highs shimmer the rim and lines, beats fire comets and flash their lights.",
  "CREDIT": "Iris plates: Shadertoy paste (author unknown). Neon lines: Jan Mróz (jaszunio15, CC BY 3.0). Comet capsules + fluid: Shadertoy paste (fluid core from XtGcDK). ShaderClaw port + fusion.",
  "CATEGORIES": ["Generator", "3D", "Tunnel", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "speed",        "LABEL": "Speed",            "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Speed" },
    { "NAME": "plateSpeed",   "LABEL": "Plates Speed",     "TYPE": "float", "DEFAULT": 1.0,  "MIN": -3.0, "MAX": 3.0,  "GROUP": "Speed" },
    { "NAME": "lineSpeed",    "LABEL": "Lines Speed",      "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Speed" },
    { "NAME": "cometSpeed",   "LABEL": "Comets Speed",     "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Speed" },
    { "NAME": "plateAmount",  "LABEL": "Plates",           "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Mix (solo each tunnel)" },
    { "NAME": "lineAmount",   "LABEL": "Neon Lines",       "TYPE": "float", "DEFAULT": 1.2,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Mix (solo each tunnel)" },
    { "NAME": "cometAmount",  "LABEL": "Comets",           "TYPE": "float", "DEFAULT": 0.8,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Mix (solo each tunnel)" },
    { "NAME": "coreColor",    "LABEL": "Core Glow",        "TYPE": "color", "DEFAULT": [0.3, 0.7, 0.9, 1.0],   "GROUP": "Colors" },
    { "NAME": "rimColor",     "LABEL": "Iris Rim",         "TYPE": "color", "DEFAULT": [0.74, 0.65, 0.65, 1.0], "GROUP": "Colors" },
    { "NAME": "lineColorA",   "LABEL": "Lines A",          "TYPE": "color", "DEFAULT": [1.0, 0.42, 0.17, 1.0],  "GROUP": "Colors" },
    { "NAME": "lineColorB",   "LABEL": "Lines B",          "TYPE": "color", "DEFAULT": [0.18, 0.73, 1.0, 1.0],  "GROUP": "Colors" },
    { "NAME": "cometTint",    "LABEL": "Comet Tint",       "TYPE": "color", "DEFAULT": [1.0, 1.0, 1.0, 1.0],    "GROUP": "Colors" },
    { "NAME": "cometHue",     "LABEL": "Comet Hue Spread", "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Colors" },
    { "NAME": "brightness",   "LABEL": "Brightness",       "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.2,  "MAX": 3.0,  "GROUP": "Colors" },
    { "NAME": "contrast",     "LABEL": "Contrast",         "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Colors" },
    { "NAME": "plateFade",    "LABEL": "Plates Fade",      "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.2,  "MAX": 3.0,  "GROUP": "Fade" },
    { "NAME": "lineFade",     "LABEL": "Lines Fade",       "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.2,  "MAX": 3.0,  "GROUP": "Fade" },
    { "NAME": "cometFade",    "LABEL": "Comets Fade",      "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.2,  "MAX": 3.0,  "GROUP": "Fade" },
    { "NAME": "inkFade",      "LABEL": "Ink Fade",         "TYPE": "float", "DEFAULT": 0.999,"MIN": 0.98, "MAX": 0.9999,"GROUP": "Fade" },
    { "NAME": "vignetteStr",  "LABEL": "Vignette",         "TYPE": "float", "DEFAULT": 0.12, "MIN": 0.0,  "MAX": 0.5,  "GROUP": "Fade" },
    { "NAME": "coreAmount",   "LABEL": "Core Light",       "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Lights" },
    { "NAME": "rimAmount",    "LABEL": "Rim Light",        "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Lights" },
    { "NAME": "lineGlow",     "LABEL": "Line Glow",        "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Lights" },
    { "NAME": "lineBloom",    "LABEL": "Line Bloom",       "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Lights" },
    { "NAME": "cometLight",   "LABEL": "Comet Lights",     "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Lights" },
    { "NAME": "capsuleLight", "LABEL": "Capsule Light",    "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Lights" },
    { "NAME": "distantColor", "LABEL": "Distant Color",    "TYPE": "color", "DEFAULT": [0.05, 0.08, 0.16, 1.0], "GROUP": "Distant Colors" },
    { "NAME": "distantAmount","LABEL": "Distant Amount",   "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Distant Colors" },
    { "NAME": "distantPulse", "LABEL": "Distant Pulse",    "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Distant Colors" },
    { "NAME": "drift",        "LABEL": "Drift",            "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Motion" },
    { "NAME": "spin",         "LABEL": "Spin",             "TYPE": "float", "DEFAULT": 0.0,  "MIN": -2.0, "MAX": 2.0,  "GROUP": "Motion" },
    { "NAME": "breath",       "LABEL": "Breath",           "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 2.0,  "GROUP": "Motion" },
    { "NAME": "sway",         "LABEL": "Sway",             "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Motion" },
    { "NAME": "wobble",       "LABEL": "Rib Wobble",       "TYPE": "float", "DEFAULT": 0.6,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Motion" },
    { "NAME": "cometRate",    "LABEL": "Comet Fire Rate",  "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 4.0,  "GROUP": "Motion" },
    { "NAME": "glowRes",      "LABEL": "Glow Resolution",  "TYPE": "float", "DEFAULT": 0.6,  "MIN": 0.3,  "MAX": 1.0,  "GROUP": "Tunnel" },
    { "NAME": "lens",         "LABEL": "Lens",             "TYPE": "float", "DEFAULT": 0.45, "MIN": 0.25, "MAX": 1.2,  "GROUP": "Tunnel" },
    { "NAME": "bend",         "LABEL": "Path Bend",        "TYPE": "float", "DEFAULT": 0.0,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Tunnel" },
    { "NAME": "layers",       "LABEL": "Plates",           "TYPE": "float", "DEFAULT": 140.0,"MIN": 40.0, "MAX": 260.0,"GROUP": "Tunnel" },
    { "NAME": "plateGap",     "LABEL": "Plate Gap",        "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.4,  "MAX": 3.0,  "GROUP": "Tunnel" },
    { "NAME": "holeSize",     "LABEL": "Iris Size",        "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.3,  "MAX": 2.5,  "GROUP": "Tunnel" },
    { "NAME": "ribDensity",   "LABEL": "Rib Density",      "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.2,  "MAX": 3.0,  "GROUP": "Tunnel" },
    { "NAME": "ribJitter",    "LABEL": "Rib Jitter",       "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Tunnel" },
    { "NAME": "lineWidth",    "LABEL": "Line Width",       "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.3,  "MAX": 4.0,  "GROUP": "Tunnel" },
    { "NAME": "lineSteps",    "LABEL": "Line Quality",     "TYPE": "float", "DEFAULT": 44.0, "MIN": 20.0, "MAX": 90.0, "GROUP": "Tunnel" },
    { "NAME": "cometSteps",   "LABEL": "Comet Quality",    "TYPE": "float", "DEFAULT": 34.0, "MIN": 16.0, "MAX": 70.0, "GROUP": "Tunnel" },
    { "NAME": "audioReact",   "LABEL": "Audio React",      "TYPE": "float", "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 1.0,  "GROUP": "Audio Reactivity" },
    { "NAME": "bassResponse", "LABEL": "Bass → Iris",      "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Audio Reactivity" },
    { "NAME": "midResponse",  "LABEL": "Mid → Wobble/Ink", "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Audio Reactivity" },
    { "NAME": "highResponse", "LABEL": "High → Rim/Lines", "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Audio Reactivity" },
    { "NAME": "beatFire",     "LABEL": "Beat → Comets",    "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 3.0,  "GROUP": "Audio Reactivity" }
  ],
  "PASSES": [
    { "TARGET": "envBuf",   "PERSISTENT": true },
    { "TARGET": "fluidBuf", "PERSISTENT": true },
    { "TARGET": "glowBuf",  "WIDTH": "floor($WIDTH*$glowRes)", "HEIGHT": "floor($HEIGHT*$glowRes)" },
    {}
  ]
}*/

#define PI  3.14159265
#define TAU 6.28318531
#define MAX_LAYERS 260

// ── comet grid ──────────────────────────────────────────────────────────────
#define GRID vec2(10.0, 5.0)
#define NUM_OBJ 50
#define CAP_LENGTH 2.0
#define TUNNEL_RADIUS 1.5
#define CAP_RADIUS 0.05
#define COMET_RADIUS 0.015
#define INK_LINE_LENGTH 0.04
#define TRAPEZOID vec2(0.4, 0.6)
#define FLUID_STRETCH vec2(8.0, 0.8)

// ── neon lines ──────────────────────────────────────────────────────────────
#define LINE_LENGTH 1.0
#define LINE_SPACE 1.0
#define BOUNDING_CYLINDER 1.8
#define INSIDE_CYLINDER 0.32
#define LINE_FOG 30.0

// ---- audio (smooth, knee'd) -------------------------------------------------
float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
float aBass()  { return pow(knee(audioBass, 0.05, 0.85), 1.5) * bassResponse; }
float aMid()   { return pow(knee(audioMid,  0.08, 0.90), 1.3) * midResponse; }
float aHigh()  { return pow(knee(audioHigh, 0.10, 0.90), 1.2) * highResponse; }

float g_t, g_hole, g_wobble, g_ar, g_bass, g_mid, g_high, g_beat;
vec2  g_sway;    // shared lateral camera offset, as a fraction of each tunnel's radius
// One lens for all three tunnels: forward is -z, screen right +x, up +y.
vec3 camRay(vec2 fg) { return normalize(vec3(fg, -(0.5 * RENDERSIZE.y) / lens)); }

// ---- hashes -----------------------------------------------------------------
float hash13(vec3 p3) {
    p3 = fract(p3 * vec3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yxz + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}
float hash1(float n) { return fract(sin(n) * 753.5453123); }
vec3 hash31(float n) {
    return fract(sin(vec3(n, n + 1.0, n + 2.0)) * vec3(43758.5453123, 22578.1459123, 19642.3490423));
}
float hash12(vec2 x) {
    vec3 p3 = fract(vec3(x.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}
vec3 hash33(vec3 x) {
    vec3 p3 = fract(x * vec3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yxz + 33.33);
    return fract((p3.xxy + p3.yxx) * p3.zyx);
}
mat2 rot2(float a) { return mat2(cos(a), -sin(a), sin(a), cos(a)); }

// ═════════════════════════════════════════════════════════════════════════════
// 1. IRIS PLATES
// ═════════════════════════════════════════════════════════════════════════════
float plateRadius(vec2 uv, float i, out float shade) {
    float l = length(uv);
    float n = fract((atan(uv.y, uv.x) + PI) / TAU + 0.801 * i);
    n += 0.0125 * sin(0.5 * g_t);
    n += smoothstep(0.17, 0.10, l) * 0.004 * sin(0.5 * i + 3.0 * g_t + 100.0 * l) * g_wobble;
    float cs = (250.0 + mod(i, 3.0) * 75.0) * ribDensity;
    float c  = mod(floor(cs * n), cs);
    if (mod(c, 2.0) >= 1.0) { shade = 0.0; return 1.0; }
    float v = fract(cs * n) - 0.5;
    shade = sqrt(1.0 - v * v);
    float base = mix(0.18, 0.19, 0.5 + 0.5 * sin(0.5 * i)) * g_hole;
    return base - 0.05 * ribJitter * hash13(vec3(11.0 * c, 5.0142 * i, 0.125 + 17.0 * c)) - 0.012 * shade;
}
float tracePlates(vec3 o, vec3 r, out float shade) {
    float g  = 0.0066 * plateGap;
    float L  = floor(clamp(layers, 40.0, float(MAX_LAYERS)));
    float dd = 1e5;
    shade = 0.0;
    for (int k = 0; k < MAX_LAYERS; k++) {
        if (float(k) >= L) break;
        float i = float(k);
        float q = mod(g * i - 0.1 * g_t * plateSpeed, L * g);
        float d = -(o.z + q) / r.z;
        vec2 uv = (o + r * d).xy;
        float s;
        if (length(uv) > plateRadius(uv, i, s)) {
            if (d < dd && d > 0.0) { dd = d; shade = s; }
        }
    }
    return dd;
}
float gyroid(vec3 p, float f) {
    return clamp(abs(dot(sin(p * 0.5), cos(p.zxy * 1.23) * f)) - 0.1, 0.0, 3.0) / 3.0;
}
float coreFbm(vec3 p) {
    p.z -= 1.6 * g_t;
    const int o = 4;
    float s = 1.95, n = PI / float(o), w = 0.0, a = 1.0, f = 1.0, r = 0.0;
    mat3 m3 = s * mat3(cos(n), sin(n), 0.0, -sin(n), cos(n), 0.0, 0.0, 0.0, 1.0);
    for (int i = 0; i < o; i++) {
        r += a * gyroid(p, f);
        p *= m3; w += a; a *= 0.7; f *= 0.78;
    }
    return clamp(r / w, 0.0, 1.0);
}
float falloff(float d, float r, float e) { return max(0.0, pow(r / max(d, 1e-5), e)); }

// returns colour; coverage = how solidly a plate was hit (for the distant tint)
vec3 renderPlates(vec2 fg, out float coverage) {
    vec3 ro = vec3(g_sway * 0.18, 0.05);      // iris radius 0.18
    vec3 rd = camRay(fg);
    float shade;
    float d = tracePlates(ro, rd, shade);
    vec3 p = ro + rd * d;
    float lp = length(p.xy);
    float depthFade = smoothstep(-1.2 * plateFade, 0.0, p.z);
    depthFade *= smoothstep(0.0, 0.05, d);           // dissolve at the lens, no pop
    coverage = (d < 1e4) ? depthFade * shade : 0.0;
    vec3 col = 0.0625 * smoothstep(0.2, 0.0, lp) * coreColor.rgb
             * falloff(coreFbm(16.0 * p), 0.5, 1.5) * coreAmount * (1.0 + 1.2 * g_beat + 0.6 * g_bass);
    col += 0.5 * rimColor.rgb * falloff(1.0 - smoothstep(0.15, 0.1155, lp), 0.1, 3.0)
         * rimAmount * (1.0 + 0.8 * g_high);
    col *= shade * shade * shade * depthFade;
    col = 1.0 - exp(-col);
    return pow(col, vec3(0.4545));
}

// ═════════════════════════════════════════════════════════════════════════════
// 2. NEON LINES (Jan Mróz, CC BY 3.0)
// ═════════════════════════════════════════════════════════════════════════════
float boxSDF(vec3 p, vec3 b) {
    vec3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0);
}
vec4 repeatBoxSDF(vec3 point) {
    vec3 rootPoint = floor(vec3(point.x / LINE_SPACE, point.y / LINE_SPACE, point.z / LINE_LENGTH));
    rootPoint.z *= LINE_LENGTH; rootPoint.xy *= LINE_SPACE;
    float minSDF = 10000.0;
    vec3 mainColor = vec3(0.0);
    float lw = 0.007 * lineWidth;
    for (float x = -1.0; x <= 1.1; x++)
    for (float y = -1.0; y <= 1.1; y++)
    for (float z = -1.0; z <= 1.1; z++) {
        vec3 trp = rootPoint + vec3(x * LINE_SPACE, y * LINE_SPACE, z * LINE_LENGTH);
        vec3 lineHash = hash33(trp);
        lineHash.z = pow(lineHash.z, 10.0);
        float h = hash12(trp.xy) - 0.5;
        trp.z += h * LINE_LENGTH;
        vec3 boxCenter = trp + vec3(0.5 * LINE_SPACE, 0.5 * LINE_SPACE, 0.5 * LINE_LENGTH);
        boxCenter.xy += (lineHash.xy - 0.5) * LINE_SPACE;
        vec3 boxSize = vec3(lw, lw, LINE_LENGTH * (1.0 - lineHash.z));
        vec3 color = (lineHash.x < 0.5) ? lineColorB.rgb * 1.1 : lineColorA.rgb * 1.44;
        float sdf = boxSDF(point - boxCenter, boxSize);
        if (sdf < minSDF) { mainColor = color; minSDF = sdf; }
    }
    return vec4(mainColor, minSDF);
}
vec3 spaceBounding(vec3 p) { return vec3(sin(p.z * 0.15) * 5.0, cos(p.z * 0.131) * 5.0, 0.0) * bend; }
vec4 lineSDF(vec3 point) {
    point += spaceBounding(point);
    vec4 lines = repeatBoxSDF(point);
    float cyl = length(point.xy) - BOUNDING_CYLINDER;
    float inner = -(length(point.xy) - INSIDE_CYLINDER);
    float object = max(max(lines.a, cyl), inner);
    return vec4(lines.rgb, object);
}
vec3 renderLines(vec2 fg) {
    float T = g_t * 0.4 * lineSpeed;
    float fogD = LINE_FOG * lineFade;
    vec3 cam = vec3(g_sway * INSIDE_CYLINDER, -T * 10.0); cam -= spaceBounding(cam);
    vec3 rd = camRay(fg);
    vec3 ro = cam;
    vec3 glow = vec3(0.0);
    float dist = 0.0;
    int steps = int(clamp(lineSteps, 20.0, 90.0));
    for (int i = 0; i < 90; i++) {
        if (i >= steps) break;
        vec4 s = lineSDF(ro);
        glow += s.rgb * sqrt(smoothstep(0.45, 0.0, s.a)) * pow(smoothstep(fogD * 0.6, 0.0, dist), 3.0) * 0.2;
        ro += rd * s.a * 0.7;
        dist += s.a;
        if (length(ro.xy) > BOUNDING_CYLINDER + 10.0) break;
    }
    vec4 sdf = lineSDF(ro);
    float vision = smoothstep(0.035, 0.0, sdf.a);
    float fog = sqrt(smoothstep(fogD, 0.0, distance(cam, ro)));
    vec3 bloom = smoothstep(0.0, 15.0, glow) * lineBloom;
    vec3 col = glow * vision * 0.12 * fog * lineGlow * (1.0 + 0.6 * g_high) + bloom * 0.8;
    return smoothstep(-0.01, 1.5, col * 1.1);
}

// ═════════════════════════════════════════════════════════════════════════════
// 3. COMET CAPSULES + FLUID INK
// ═════════════════════════════════════════════════════════════════════════════
// Envelopes live in 64 normalised slots across an 8-texel band at the bottom
// of envBuf, so a pass of ANY size (the reduced-res glow pass) reads them
// the same way — no texel addressing against the full canvas size.
#define ENV_SLOTS 64.0
float envAt(int id) {
    float e = texture2D(envBuf, vec2((float(id) + 0.5) / ENV_SLOTS, 2.0 / RENDERSIZE.y)).x;
    if (e != e) e = 0.0;                 // NaN guard
    return clamp(e, 0.0, 1.0);
}
vec2 tunnelPath(float z) { float s = sin(z / 24.0) * cos(z / 16.0); return vec2(s * 9.0, 0.0) * bend; }
vec2 hashDisplace(float id) { return vec2(sin(id * 210.656) * 0.2, cos(id * 3020.121454) * 0.3); }
vec3 lightColor(int id) {
    vec3 h = hash31(float(id) * 0.003);
    return mix(vec3(0.8), h, cometHue) * cometTint.rgb;
}
vec3 capsuleFront(int id) {
    float e = envAt(id);
    float m = 2.0 - pow(e, 1.5) * 1.8;
    vec3 pos = vec3(0.0, m, 0.0) + vec3(0.0, TUNNEL_RADIUS, 0.0);
    pos.xz += hashDisplace(float(id));
    return pos;
}
vec3 cometWorld(int id, float time) {
    float angle = TAU / GRID.x;
    vec3 pos = capsuleFront(id);
    int idDepth = id / int(GRID.x);
    float idRad = mod(float(id), GRID.x);
    float z = time - mod(time + float(idDepth), GRID.y) - 0.5;
    pos.xy *= rot2((idRad + 3.0) * angle);
    pos.z += z;
    pos.xy += tunnelPath(z);
    return pos;
}
vec2 pathGrid(float id) {
    vec2 sector = 1.0 / GRID;
    vec2 gp = vec2(mod(id, GRID.x), floor(id / GRID.x));
    return gp * sector + sector * 0.5;
}
float trapezoid(float x, vec2 be) {
    x = 1.0 - x;
    return min(smoothstep(0.0, be.x, x), smoothstep(1.0, be.y, x));
}
float segDist(vec2 p, vec2 a, vec2 b) {
    return length(p - a - (b - a) * clamp(dot(p - a, b - a) / dot(b - a, b - a), 0.0, 1.0));
}
float sdCapsule(vec3 p, vec3 a, vec3 b, float r) {
    vec3 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h) - r;
}
vec2 capsuleUV(vec3 p, float capLen, float rota) {
    p.xz *= rot2(rota);
    float u = fract((atan(p.z, p.x) / 2.0) / PI + 0.5);
    float v = p.y / capLen;
    return vec2(u, v);
}
mat2 radialRepeat(vec2 p, inout float id) {
    float angle = TAU / GRID.x;
    float radialPos = atan(p.x, p.y) + angle * 0.5;
    float sector = floor(radialPos / angle);
    p.xy *= rot2(angle * 0.25);
    id = floor(((atan(p.y, p.x) + TAU * 0.5) / TAU) * GRID.x);
    return rot2(angle * sector);
}
float depthRepeat(float z, inout float id) {
    id = mod(floor(abs(z)), GRID.y);
    return mod(z, 1.0) - 0.5;
}
vec2 sphDistances(vec3 ro, vec3 rd, vec4 sph) {
    vec3 oc = ro - sph.xyz;
    float b = dot(oc, rd);
    float c = dot(oc, oc) - sph.w * sph.w;
    float h = b * b - c;
    float d = sqrt(max(0.0, sph.w * sph.w - h)) - sph.w;
    return vec2(d, -b - sqrt(max(h, 0.0)));
}
vec4 fluidTexture(float id, vec2 capUv) {
    vec2 sector = 1.0 / GRID;
    vec2 gp = vec2(mod(id, GRID.x), floor(id / GRID.x) + sector.y * 0.5) * sector;
    float stretch = 1.0 / FLUID_STRETCH.x;
    capUv.x = capUv.x * stretch + (1.0 - stretch) * 0.5;
    capUv *= sector;
    capUv.y /= FLUID_STRETCH.y;
    capUv += gp;
    return texture2D(fluidBuf, capUv);
}
vec4 cometMap(vec3 p, float time) {
    vec3 q = p;
    q.xy -= tunnelPath(q.z);
    float idRad = -1.0;
    mat2 rm = radialRepeat(q.xy, idRad);
    q.xy *= rm;
    float idDepth = 0.0;
    q.z = depthRepeat(q.z, idDepth);
    int idLine = int(clamp(idDepth * GRID.x + idRad, 0.0, float(NUM_OBJ - 1)));
    vec3 front = capsuleFront(idLine);
    float env = envAt(idLine);
    vec3 back = front + vec3(0.0, CAP_LENGTH, 0.0);
    vec2 capUv = capsuleUV(q - front, CAP_LENGTH * 2.0, sin(g_t * 1.7) * (env + 0.1));
    vec4 fluid = fluidTexture(float(idLine), capUv);
    float radius = CAP_RADIUS + CAP_RADIUS * fluid.y * 1.1;
    float cap = sdCapsule(q, front, back, radius);
    return vec4(cap * 0.7, float(idLine), capUv);
}
vec3 cometNormal(vec3 p, float time) {
    vec2 e = vec2(1.0, -1.0) * 0.5773 * 0.0005;
    return normalize(e.xyy * cometMap(p + e.xyy, time).x + e.yyx * cometMap(p + e.yyx, time).x +
                     e.yxy * cometMap(p + e.yxy, time).x + e.xxx * cometMap(p + e.xxx, time).x);
}
vec2 cometDistance(vec3 ro, vec3 rd, int id, float time) {
    vec3 pos = cometWorld(id, time);
    vec2 cd = vec2(sphDistances(ro, rd, vec4(pos, 0.8)).x, sphDistances(ro, rd, vec4(pos, CAP_LENGTH * 0.5)).x);
    cd.y = 1.0 - cd.y;
    return cd;
}
vec3 cometLights(vec3 ro, vec3 rd, float time) {
    vec3 res = vec3(0.0);
    for (int id = 0; id < NUM_OBJ; id++) {
        vec3 pos = cometWorld(id, time);
        float env = envAt(id);
        float ld = sphDistances(ro, rd, vec4(pos, COMET_RADIUS)).x;
        float fo = 0.52 * sqrt(env);
        float li = fo / pow(abs(ld), 0.42);
        li = pow(li, 5.4545) * (1.0 - env);
        res += lightColor(id) * li;
    }
    return res;
}
vec3 encodeSRGB(vec3 c) {
    vec3 a = 12.92 * c;
    vec3 b = 1.055 * pow(max(c, 0.0), vec3(1.0 / 2.4)) - 0.055;
    return mix(a, b, step(vec3(0.0031308), c));
}
vec3 renderComets(vec2 fg) {
    float time = -g_t * 3.0 * cometSpeed;
    vec3 ro = vec3(g_sway * TUNNEL_RADIUS, 1.0 + time);
    ro.xy += tunnelPath(ro.z);
    vec3 rd = camRay(fg);

    vec3 ligPos = normalize(vec3(2.0, 1.0, 0.0));
    float rayDist = length(rd);
    float thresh = 0.05125;
    float maxDist = 26.0 * cometFade;
    vec3 col = vec3(0.0);
    int steps = int(clamp(cometSteps, 16.0, 70.0));
    for (int i = 0; i < 70; i++) {
        if (i >= steps) break;
        if (dot(col, vec3(0.299, 0.587, 0.114)) > 1.0 || rayDist > maxDist) break;
        vec3 p = ro + rd * rayDist;
        vec4 obj = cometMap(p, time);
        float hit = obj.x; int id = int(obj.y);
        vec2 capUv = obj.zw;
        float accum = (thresh - abs(hit) * 31.0 / 32.0) / thresh;
        if (accum > 0.0) {
            vec3 ph = ro + rd * hit;
            vec3 norm = cometNormal(ph, time) * sign(hit);
            vec3 single = max(0.0, dot(norm, ligPos) * 0.5 + 0.5) * lightColor(id) * 2.0;
            vec4 fluid = fluidTexture(float(id), capUv + vec2(0.0, 0.1));
            vec2 cd = cometDistance(ro, rd, id, time);
            if (cd.y > 0.5 && fluid.w > 0.0) {
                vec3 ink = 0.5 + 0.5 * sin(fluid.w * 1.5 * fluid.x * 10.0 * fluid.y * 5.2 * fluid.z * vec3(1.0, 2.0, 3.0));
                col += single * mix(vec3(0.6), ink, 0.6) * capsuleLight;
            }
        }
        rayDist += max(abs(hit) * 0.95, thresh * 0.15);
    }
    float farFade = 1.0 - smoothstep(maxDist * 0.5, maxDist, rayDist);
    col *= mix(0.6, 1.0, farFade);
    col += cometLights(ro, rd, time) * 1.5 * cometLight * (1.0 + 1.5 * g_beat);
    col = encodeSRGB(col);
    return pow(clamp(col, 0.0, 1.0), vec3(0.45));
}

// ═════════════════════════════════════════════════════════════════════════════
// PASS 0 — comet trigger envelopes (row 0, one texel per capsule)
// ═════════════════════════════════════════════════════════════════════════════
vec4 passEnv() {
    vec2 fc = floor(gl_FragCoord.xy);
    float id = floor(gl_FragCoord.x / RENDERSIZE.x * ENV_SLOTS);
    if (fc.y >= 8.0 || id >= float(NUM_OBJ)) return vec4(0.0);   // never leave garbage
    vec4 prev = texture2D(envBuf, gl_FragCoord.xy / RENDERSIZE);
    if (any(notEqual(prev, prev))) prev = vec4(0.0, 0.5, 0.6, 1.0);
    vec3 vfd = hash31(id);
    float freq = vfd.y * 0.2 * cometRate, dur = 1.0 / (vfd.z * 55.0);
    float ramp = 1.0 - fract(freq * TIME * max(speed, 0.05));
    if (FRAMEINDEX < 2) prev = vec4(0.0, 0.5, 0.6, 1.0);
    float env = prev.x, prevRamp = prev.y;
    bool trigger = ramp > prevRamp;
    // beats fire a random subset of capsules
    float beatGate = hash1(id * 7.31 + floor(TIME * 8.0));
    if (clamp(audioBeatPulse, 0.0, 1.0) * audioReact * beatFire > 0.55 && beatGate > 0.88 && env < 0.15) trigger = true;
    float armed = prev.z;                              // .z = attack in progress
    if (trigger) armed = 1.0;
    if (armed > 0.5) { env += (1.0 - env) * 0.3; if (env > 0.97) { env = 1.0; armed = 0.0; } }
    else env += (0.0 - env) * dur;
    return vec4(env, ramp, armed, 0.0);
}

// ═════════════════════════════════════════════════════════════════════════════
// PASS 1 — fluid ink (XtGcDK core), re-inked where a capsule fires
// ═════════════════════════════════════════════════════════════════════════════
vec4 fluidAt(vec2 U) {
    U.x = mod(U.x, RENDERSIZE.x - 1.0);
    return texture2D(fluidBuf, U / RENDERSIZE);
}
vec4 passFluid() {
    vec2 R = RENDERSIZE;
    vec2 U = gl_FragCoord.xy;
    vec2 A = U + vec2(1.0, 0.0), B = U + vec2(0.0, 1.0), C = U + vec2(-1.0, 0.0), D = U + vec2(0.0, -1.0);
    vec4 u = fluidAt(U), a = fluidAt(A), b = fluidAt(B), c = fluidAt(C), d = fluidAt(D);
    vec4 p = vec4(0.0);
    vec2 g = vec2(0.0);
    for (int i = 0; i < 2; i++) {
        U -= u.xy; A -= a.xy; B -= b.xy; C -= c.xy; D -= d.xy;
        p += vec4(length(U - A), length(U - B), length(U - C), length(U - D)) - 1.0;
        g += vec2(a.z - c.z, b.z - d.z);
        u = fluidAt(U); a = fluidAt(A); b = fluidAt(B); c = fluidAt(C); d = fluidAt(D);
    }
    vec4 Q = u;
    vec4 N = 0.25 * (a + b + c + d);
    Q = mix(Q, N, vec4(0.0, 0.0, 1.0, 0.0));
    Q.xy -= g / 10.0 / 2.0;
    Q.z += (p.x + p.y + p.z + p.w) / 10.0;
    Q.z *= inkFade;
    // which capsule sector is this texel in?
    vec2 Uo = gl_FragCoord.xy;
    vec2 sectorPx = R / GRID;
    vec2 cell = floor(Uo / sectorPx);
    if (cell.x >= 0.0 && cell.x < GRID.x && cell.y >= 0.0 && cell.y < GRID.y) {
        int id = int(cell.x + cell.y * GRID.x);
        float env = envAt(id);
        if (env < 0.0001) {
            Q = mix(Q, vec4(0.0), 0.5);
        } else {
            vec2 ahead = pathGrid(float(id));
            vec2 behind = ahead - vec2(0.0, INK_LINE_LENGTH);
            vec4 line = vec4(behind, ahead) * R.xyxy;
            float trapez = 1.0 - trapezoid(env, TRAPEZOID);
            float q = segDist(Uo, line.xy, line.zw);
            vec2 m = line.xy - line.zw;
            float l = length(m);
            if (env > 0.5 && l > 0.0) {
                float churn = 1.0 + 1.5 * aMid() * audioReact;
                Q.xyw = mix(Q.xyw, vec3(-normalize(m) * min(l, 10.0) / 5.0 * churn, 1.0), max(0.0, 4.0 * trapez - q) / 15.0);
            }
        }
    }
    if (FRAMEINDEX < 2) Q = vec4(0.0);
    // NaN guard (persistent targets can start as garbage)
    if (any(notEqual(Q, Q))) Q = vec4(0.0);
    return Q;
}

// ═════════════════════════════════════════════════════════════════════════════
// PASS 2 — composite
// ═════════════════════════════════════════════════════════════════════════════
// PASS 2 — neon lines + comets at reduced resolution (both are glow fields,
// so they upsample cleanly; the crisp plates stay full-res in the last pass)
vec4 passGlow() {
    vec2 fg = gl_FragCoord.xy - RENDERSIZE * 0.5;
    fg = rot2(spin * g_t * 0.3) * fg;
    vec3 lines  = (lineAmount  > 0.0) ? renderLines(fg)  * lineAmount  : vec3(0.0);
    vec3 comets = (cometAmount > 0.0) ? renderComets(fg) * cometAmount : vec3(0.0);
    return vec4(lines + comets, 1.0);
}

vec4 passImage() {
    vec2 fg = gl_FragCoord.xy - RENDERSIZE * 0.5;
    float sa = spin * g_t * 0.3;
    fg = rot2(sa) * fg;

    float coverage;
    vec3 plates = (plateAmount > 0.0) ? renderPlates(fg, coverage) * plateAmount : vec3(0.0);
    if (plateAmount <= 0.0) coverage = 0.0;
    vec3 glowRGB = texture2D(glowBuf, gl_FragCoord.xy / RENDERSIZE).rgb;
    vec3 lines  = glowRGB;
    vec3 comets = vec3(0.0);

    // the plate walls occlude what sits beyond them; the iris opening and
    // the far, faded plates let the lines + comets through
    float open = 1.0 - clamp(coverage, 0.0, 1.0);
    vec3 col = plates + (lines + comets) * mix(0.35, 1.0, open);

    // distant colour: tints the far end (iris opening + faded plates), pulsing
    float pulse = 1.0 + distantPulse * (pow(sin(g_t * 0.5) * 0.5 + 0.5, 2.0) * 0.6 - 0.3);
    col += distantColor.rgb * distantAmount * 0.6 * open * pulse;

    col *= brightness;
    vec2 u = gl_FragCoord.xy / RENDERSIZE;
    col *= 1.0 - vignetteStr * (1.0 - pow(16.0 * u.x * u.y * (1.0 - u.x) * (1.0 - u.y), 0.5));
    col = clamp(col, 0.0, 1.0);
    col = mix(col, col * col * (3.0 - 2.0 * col), contrast);
    float l = dot(col, vec3(0.299, 0.587, 0.114));
    col = clamp(mix(vec3(l), col, 1.22), 0.0, 1.0);
    return vec4(col, 1.0);
}

void main() {
    g_ar   = clamp(audioReact, 0.0, 1.0);
    g_bass = aBass() * g_ar; g_mid = aMid() * g_ar; g_high = aHigh() * g_ar;
    g_beat = clamp(audioBeatPulse, 0.0, 1.0) * g_ar;
    g_t      = TIME * speed;
    g_hole   = holeSize * (1.0 + 0.06 * breath * sin(g_t * 0.9)) * (1.0 + 0.25 * g_bass);
    g_wobble = wobble * (1.0 + 1.5 * g_mid);
    float st = 0.05 * g_t * drift + 20.0;
    g_sway   = vec2(cos(st), -sin(st)) * 0.42 * sway;
    if      (PASSINDEX == 0) gl_FragColor = passEnv();
    else if (PASSINDEX == 1) gl_FragColor = passFluid();
    else if (PASSINDEX == 2) gl_FragColor = passGlow();
    else                     gl_FragColor = passImage();
}
