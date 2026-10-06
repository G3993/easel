/*{
  "DESCRIPTION": "Flowers — a field of organic blooms grown from a kaleidoscopic iterated function: spheres fold, rotate and shrink into petalled clusters that breathe on a wave travelling out of each cell. Stochastic radius + a max-accumulating temporal buffer give the soft-organic 'taste of noise' look while converging to crisp bright petals. Slow camera orbit through a repeating garden. Bass swells the blooms, beats push the petal wave, mids drift the palette, highs sharpen the backlight. Every parameter is exposed.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-23 — after 'taste of noise 7' by Leon Denise (hash13 Dave Hoskins, smin Inigo Quilez, normal NuSan)",
  "CATEGORIES": ["Generator", "3D", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "orbitSpeed",  "LABEL": "Orbit Speed",      "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": 0.12, "GROUP": "Motion" },
    { "NAME": "flySpeed",    "LABEL": "Fly Through",      "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Motion" },
    { "NAME": "waveSpeed",   "LABEL": "Petal Wave Speed", "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "spinAmt",     "LABEL": "Petal Spin",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Motion" },
    { "NAME": "camTilt",     "LABEL": "Camera Tilt",      "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Camera" },
    { "NAME": "camDist",     "LABEL": "Camera Distance",  "TYPE": "float", "MIN": 0.6,  "MAX": 4.0,  "DEFAULT": 1.73, "GROUP": "Camera" },
    { "NAME": "fov",         "LABEL": "Field Of View",    "TYPE": "float", "MIN": 0.5,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Camera" },
    { "NAME": "iterations",  "LABEL": "Petal Levels",     "TYPE": "float", "MIN": 2.0,  "MAX": 6.0,  "DEFAULT": 4.0,  "GROUP": "Bloom" },
    { "NAME": "bloomSize",   "LABEL": "Bloom Size",       "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Bloom" },
    { "NAME": "spread",      "LABEL": "Petal Spread",     "TYPE": "float", "MIN": 0.2,  "MAX": 1.2,  "DEFAULT": 0.5,  "GROUP": "Bloom" },
    { "NAME": "waveAmt",     "LABEL": "Petal Wave",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Bloom" },
    { "NAME": "blend",       "LABEL": "Petal Fusion",     "TYPE": "float", "MIN": 0.2,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Bloom" },
    { "NAME": "gridSize",    "LABEL": "Garden Spacing",   "TYPE": "float", "MIN": 3.0,  "MAX": 9.0,  "DEFAULT": 5.0,  "GROUP": "Bloom" },
    { "NAME": "fuzz",        "LABEL": "Organic Fuzz",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.65,  "GROUP": "Render" },
    { "NAME": "dither",      "LABEL": "Step Dither",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 1.0,  "GROUP": "Render" },
    { "NAME": "steps",       "LABEL": "March Steps",      "TYPE": "float", "MIN": 16.0, "MAX": 64.0, "DEFAULT": 36.0, "GROUP": "Render" },
    { "NAME": "decay",       "LABEL": "Memory",           "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Render" },
    { "NAME": "paletteShift","LABEL": "Palette Shift",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "paletteDrift","LABEL": "Palette Drift",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.25, "GROUP": "Color" },
    { "NAME": "colorSpread", "LABEL": "Color Spread",     "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.6,  "GROUP": "Color" },
    { "NAME": "colA",        "LABEL": "Orange",           "TYPE": "color", "DEFAULT": [1.0, 0.58, 0.05, 1.0], "GROUP": "Color" },
    { "NAME": "colB",        "LABEL": "Pink",             "TYPE": "color", "DEFAULT": [1.0, 0.18, 0.55, 1.0], "GROUP": "Color" },
    { "NAME": "colC",        "LABEL": "Purple",           "TYPE": "color", "DEFAULT": [0.55, 0.12, 0.95, 1.0], "GROUP": "Color" },
    { "NAME": "colD",        "LABEL": "Violet",           "TYPE": "color", "DEFAULT": [0.8, 0.2, 0.75, 1.0], "GROUP": "Color" },
    { "NAME": "lightA",      "LABEL": "Top Light",        "TYPE": "color", "DEFAULT": [1.0, 0.5, 0.15, 1.0], "GROUP": "Color" },
    { "NAME": "lightB",      "LABEL": "Back Light",       "TYPE": "color", "DEFAULT": [0.5, 0.15, 1.0, 1.0], "GROUP": "Color" },
    { "NAME": "lightAmt",    "LABEL": "Light Amount",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.5,  "DEFAULT": 0.55,  "GROUP": "Color" },
    { "NAME": "specular",    "LABEL": "Specular",         "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.4,  "GROUP": "Color" },
    { "NAME": "saturation",  "LABEL": "Saturation",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.6,  "DEFAULT": 1.15, "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",       "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Render" }
  ],
  "PASSES": [
    { "TARGET": "flAcc", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// FLOWERS
//   pass 0  flAcc — stochastic KIFS raymarch (per-pixel white-noise radius
//                   + step dither) max-accumulated into a persistent buffer
//                   (the "taste of noise" temporal trick: brightest sample
//                   wins, fading by `decay`).
//   pass 1  final — brightness / audio lift + HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define PI  3.14159265
#define TAU 6.28318531

// Dave Hoskins — https://www.shadertoy.com/view/4djSRW
float hash13(vec3 p3) {
    p3  = fract(p3 * 0.1031);
    p3 += dot(p3, p3.zyx + 31.32);
    return fract((p3.x + p3.y) * p3.z);
}
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

// Inigo Quilez — https://iquilezles.org/articles/distfunctions
float smin(float d1, float d2, float k) {
    float h = clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0);
    return mix(d2, d1, h) - k * h * (1.0 - h);
}
float smoothing(float d1, float d2, float k) { return clamp(0.5 + 0.5 * (d2 - d1) / k, 0.0, 1.0); }
mat2 rot(float a) { return mat2(cos(a), -sin(a), sin(a), cos(a)); }
vec3 repeat3(vec3 p, float r) { return mod(p, r) - r * 0.5; }

// ── audio shaping ────────────────────────────────────────────────────────
float bassP()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.4); }
float midP()   { return pow(smoothstep(0.06, 0.85, audioMid),  1.2); }
float highP()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }
float beatP()  { return clamp(audioBeatPulse, 0.0, 1.0); }
float levP()   { return clamp(audioLevel, 0.0, 1.0); }

// cyclic four-colour palette
vec3 palette(float t) {
    t = fract(t) * 4.0;
    float i = floor(t), f = smoothstep(0.0, 1.0, fract(t));
    vec3 a = i < 0.5 ? colA.rgb : i < 1.5 ? colB.rgb : i < 2.5 ? colC.rgb : colD.rgb;
    vec3 b = i < 0.5 ? colB.rgb : i < 1.5 ? colC.rgb : i < 2.5 ? colD.rgb : colA.rgb;
    return mix(a, b, f);
}

// per-pixel white noise for this frame
float g_rng;
float g_material;

// ── signed distance: kaleidoscopic iterated blooms ───────────────────────
float map(vec3 p) {
    float ar = audioReact;
    // time stretched with noise; beats/bass push the petal wave phase
    float t = TIME * waveSpeed + g_rng * 0.9 + ar * 1.1 * audioBassTime;

    float grid = gridSize;
    vec3 cell = floor(p / grid);
    p = repeat3(p, grid);

    float dp = length(p);
    vec3 angle = vec3(0.1, -0.5, 0.1) + dp * 0.5 + p * 0.1 + cell
               + spinAmt * TIME * 0.2;

    // stochastic radius — the organic fuzz — swollen by bass
    float size = bloomSize * mix(1.0, sin(g_rng * PI), fuzz)
               * (1.0 + ar * 0.16 * bassP() + ar * 0.07 * beatP());

    float wave = sin(-dp + t + hash13(cell) * TAU) * waveAmt;

    int count = int(iterations + 0.5);
    float a = 1.0;
    float scene = 1000.0;
    float shape = 1000.0;
    for (int i = 0; i < 6; i++) {
        if (i >= count) break;
        p.xz = abs(p.xz) - (spread + wave) * a;
        p.xz *= rot(angle.y / a);
        p.yz *= rot(angle.x / a);
        p.yx *= rot(angle.z / a);
        shape = length(p) - 0.2 * a * size;
        g_material = mix(g_material, float(i), smoothing(shape, scene, 0.3 * a));
        scene = smin(scene, shape, blend * a);
        a /= 1.9;
    }
    return scene;
}

vec3 render(vec2 fragCoord) {
    float ar = audioReact;
    g_material = 0.0;

    vec2 uv = (fragCoord - R * 0.5) / R.y;
    uv *= fov;
    vec3 eye = normalize(vec3(1.0, 1.0, 1.0)) * camDist;
    vec3 at  = vec3(0.0);
    vec3 z = normalize(at - eye);
    vec3 x = normalize(cross(z, vec3(0.0, 1.0, 0.0)));
    vec3 y = cross(x, z);
    vec3 ray = normalize(z + uv.x * x + uv.y * y);
    vec3 pos = eye;

    // camera control: slow orbit replaces the mouse
    float yaw   = TIME * orbitSpeed * 0.35;
    float pitch = camTilt * 1.2;
    ray.xz *= rot(yaw);   pos.xz *= rot(yaw);
    ray.xy *= rot(pitch); pos.xy *= rot(pitch);
    // fly through the repeating garden
    pos += vec3(0.4, 0.2, 1.0) * TIME * flySpeed * 0.6;

    // white noise
    g_rng = hash13(vec3(fragCoord, fract(TIME * 0.37) * 100.0 + floor(TIME * 60.0)));

    int nSteps = int(steps + 0.5);
    float fSteps = float(nSteps);
    float index = fSteps;
    for (int i = 0; i < 64; i++) {
        if (i >= nSteps) break;
        float dist = map(pos);
        if (dist < 0.01) break;
        dist *= 1.0 - dither * 0.1 + dither * 0.1 * g_rng;
        pos += ray * dist;
        index -= 1.0;
    }

    float shade = pow(index / fSteps, 1.3);

    // normal (NuSan)
    vec2 off = vec2(0.0012, 0.0);
    float m0 = map(pos);
    vec3 normal = normalize(m0 - vec3(map(pos - off.xyy), map(pos - off.yxy), map(pos - off.yyx)));

    // palette: orange → pink → purple → magenta, cycling across petal level,
    // distance and garden cell; drifting slowly + mids nudge
    float ph = paletteShift
             + paletteDrift * 0.12 * sin(TIME * 0.11)
             + ar * 0.08 * midP();
    float pt = ph + colorSpread * (g_material * 0.17 + length(pos) * 0.09)
             + 0.55 * hash13(floor((pos + 0.5 * gridSize) / gridSize) + 7.1);
    vec3 tint = palette(pt);

    // lighting
    vec3 rr = reflect(ray, normal);
    float ld = dot(rr, vec3(0.0, 1.0, 0.0)) * 0.5 + 0.5;
    vec3 light = lightA.rgb * sqrt(ld) * lightAmt;
    ld = dot(rr, vec3(0.0, 0.0, -1.0)) * 0.5 + 0.5;
    light += lightB.rgb * sqrt(ld) * lightAmt * (0.6 + ar * 0.4 * highP());
    // crisp specular
    float sp = pow(max(dot(rr, normalize(vec3(0.3, 1.0, -0.4))), 0.0), 28.0);
    light += vec3(1.0, 0.95, 0.9) * sp * specular * 1.2;

    // tint drives the colour; lights tint it further instead of washing white
    vec3 col = (tint * 1.15 + light * (0.55 + 0.45 * tint)) * shade;
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        vec3 col = render(gl_FragCoord.xy);
        vec3 prev = texture2D(flAcc, uv).rgb;
        if (FRAMEINDEX < 2 || any(notEqual(prev, prev))) prev = vec3(0.0);
        // temporal max buffer: brightest sample wins, fades by `decay`
        float fade = mix(0.05, 0.003, decay);
        col = max(col, prev - fade);
        gl_FragColor = vec4(clamp(col, 0.0, 2.0), 1.0);
    } else {
        vec3 col = texture2D(flAcc, uv).rgb;
        if (any(notEqual(col, col))) col = vec3(0.0);
        float lvl = levP();
        // continuous energy lift on the accumulated image (survives the
        // max buffer): dips below 1 in silence, swells on bass and level
        col *= brightness * mix(1.0, 0.66 + 0.40 * lvl + 0.22 * bassP(), audioReact)
             * (1.0 + audioReact * 0.06 * beatP());
        vec3 warm = vec3(1.0, 0.86, 0.72);
        col *= mix(vec3(1.0), warm, audioReact * 0.5 * highP());
        float l0 = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(vec3(l0), col, saturation);
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.008;
        // HD finisher
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
