/*{
  "DESCRIPTION": "Cola De Gato Morado — a twisting purple cat's-tail of 3D fire: a volumetric column of turbulent flame that corkscrews on itself, wrapped in a very subtle ring of drifting dust motes and lit by soft sunrays bleeding out of the core. Hue breathes slowly through violet, magenta and indigo. Bass swells the flame, beats push the twist, mids drift the hue, highs glint the dust and rays. Every parameter is exposed.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-11 — after the fragcoord.xyz 3D-fire twist (cx081tgx / 3zoe0vgo)",
  "CATEGORIES": ["Generator", "3D", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "flameSpeed",  "LABEL": "Flame Speed",     "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Flame" },
    { "NAME": "twistAmt",    "LABEL": "Twist",           "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Flame" },
    { "NAME": "turbulence",  "LABEL": "Turbulence",      "TYPE": "float", "MIN": 2.2,  "MAX": 6.0,  "DEFAULT": 4.0,  "GROUP": "Flame" },
    { "NAME": "flameHeight", "LABEL": "Flame Height",    "TYPE": "float", "MIN": 3.0,  "MAX": 12.0, "DEFAULT": 8.0,  "GROUP": "Flame" },
    { "NAME": "flameWidth",  "LABEL": "Flame Width",     "TYPE": "float", "MIN": 0.5,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Flame" },
    { "NAME": "density",     "LABEL": "Density",         "TYPE": "float", "MIN": 0.3,  "MAX": 2.5,  "DEFAULT": 1.0,  "GROUP": "Flame" },
    { "NAME": "quality",     "LABEL": "March Steps",     "TYPE": "float", "MIN": 30.0, "MAX": 90.0, "DEFAULT": 70.0, "GROUP": "Flame" },
    { "NAME": "camDist",     "LABEL": "Camera Distance", "TYPE": "float", "MIN": 5.0,  "MAX": 16.0, "DEFAULT": 9.0,  "GROUP": "Camera" },
    { "NAME": "camHeight",   "LABEL": "Camera Height",   "TYPE": "float", "MIN": -3.0, "MAX": 3.0,  "DEFAULT": 0.0,  "GROUP": "Camera" },
    { "NAME": "camRoll",     "LABEL": "Camera Roll",     "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Camera" },
    { "NAME": "hueBase",     "LABEL": "Hue",             "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "hueDrift",    "LABEL": "Hue Drift",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.25, "GROUP": "Color" },
    { "NAME": "hueSpeed",    "LABEL": "Hue Drift Speed", "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.4,  "GROUP": "Color" },
    { "NAME": "saturation",  "LABEL": "Saturation",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.6,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "exposure",    "LABEL": "Exposure",        "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",      "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "dustAmt",     "LABEL": "Dust Amount",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Dust Ring" },
    { "NAME": "dustRadius",  "LABEL": "Dust Ring Radius","TYPE": "float", "MIN": 1.0,  "MAX": 8.0,  "DEFAULT": 3.6,  "GROUP": "Dust Ring" },
    { "NAME": "dustThick",   "LABEL": "Dust Ring Width", "TYPE": "float", "MIN": 0.1,  "MAX": 3.0,  "DEFAULT": 0.9,  "GROUP": "Dust Ring" },
    { "NAME": "dustHeight",  "LABEL": "Dust Ring Height","TYPE": "float", "MIN": -6.0, "MAX": 6.0,  "DEFAULT": -1.0, "GROUP": "Dust Ring" },
    { "NAME": "dustTilt",    "LABEL": "Dust Ring Tilt",  "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Dust Ring" },
    { "NAME": "dustSpin",    "LABEL": "Dust Spin",       "TYPE": "float", "MIN": -2.0, "MAX": 2.0,  "DEFAULT": 0.3,  "GROUP": "Dust Ring" },
    { "NAME": "dustSize",    "LABEL": "Dust Size",       "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Dust Ring" },
    { "NAME": "rayAmt",      "LABEL": "Sunrays",         "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Sunrays" },
    { "NAME": "rayLength",   "LABEL": "Ray Length",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Sunrays" },
    { "NAME": "rayX",        "LABEL": "Ray Source X",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Sunrays" },
    { "NAME": "rayY",        "LABEL": "Ray Source Y",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.42, "GROUP": "Sunrays" },
    { "NAME": "audioReact",  "LABEL": "Audio React",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.15, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.4,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "cgScene" },
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// COLA DE GATO MORADO
//   pass 0  cgScene — volumetric twisted-fire raymarch (ported from the
//                     fragcoord.xyz "3D fire" twist), hue-rotated to
//                     purple, plus a 3D ring of dust motes projected with
//                     the same camera.
//   pass 1  abTrail — house motion trail.
//   pass 2  final   — sunrays (radial blur out of the core) + bloom +
//                     aberration + HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define PI  3.14159265
#define TAU 6.28318531

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float hash11(float n) { return fract(sin(n * 91.3458) * 47453.5453); }

vec3 tanh3(vec3 x) {
    x = clamp(x, -12.0, 12.0);
    vec3 e = exp(2.0 * x);
    return (e - 1.0) / (e + 1.0);
}

// hue rotation around the grey axis (Rodrigues)
vec3 hueRotate(vec3 c, float h) {
    float a = h * TAU;
    vec3 k = vec3(0.57735);
    float cs = cos(a), sn = sin(a);
    return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
}

float bassP()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.4); }
float midP()   { return pow(smoothstep(0.06, 0.85, audioMid),  1.2); }
float highP()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }
float beatP()  { return clamp(audioBeatPulse, 0.0, 1.0); }
float levP()   { return clamp(audioLevel, 0.0, 1.0); }

// camera: at (0, camHeight, camDist) in flame space, looking down -z
vec3 rayDir(vec2 I) {
    vec3 d = normalize(vec3(I + I, 0.0) - R.xyy);
    float cr = camRoll * 0.6;
    d.xy = mat2(cos(cr), -sin(cr), sin(cr), cos(cr)) * d.xy;
    return d;
}

// project a flame-space point back to pixel coords (inverse of rayDir)
vec2 project(vec3 w, out float depth) {
    vec3 v = w - vec3(0.0, camHeight, camDist);
    float cr = -camRoll * 0.6;
    v.xy = mat2(cos(cr), -sin(cr), sin(cr), cos(cr)) * v.xy;
    depth = -v.z;
    return 0.5 * (R.xy - R.y * v.xy / v.z);
}

vec3 flame(vec2 I) {
    float amt = audioReact;
    // phase-push, not time-scale: bass energy advances the flame clock
    float T = TIME * flameSpeed + amt * 0.35 * audioBassTime;
    float tw = twistAmt * (1.0 + amt * 0.25 * beatP());
    float hgt = flameHeight * (1.0 + amt * 0.30 * bassP());
    float wid = flameWidth;

    vec3 O = vec3(0.0);
    float z = 0.0;
    vec3 dir = rayDir(I);
    int steps = int(quality);
    for (int i = 0; i < 90; i++) {
        if (i >= steps) break;
        vec3 p = z * dir;
        p.z += camDist;
        p.y -= camHeight;
        p.xz /= wid;
        vec3 t = p;
        vec3 Z = vec3(6.0 * T, 0.0, 0.0);
        float d = 2.0;
        float a = (p.y - length(p.xz)) / d * tw - T;
        p.xz *= mat2(cos(a + T + vec4(0.0, 5.0, 8.0, 0.0)));
        for (int k = 0; k < 14; k++) {
            if (d >= turbulence) break;
            p += sin(p.yzx * d - Z) / d;
            d /= 0.9;
        }
        d = min(length(p.xz), hgt - abs(p.y)) / 15.0 / (2.0 + cos(a));
        z += d;                                  // original semantics: negative d
        float den = d / max(length(t.xz - (p.xz / 2.0 + 3.0) * sin(a)), 0.02);   // subtracts (darkens) — keep it
        // near = magenta-purple, far = indigo-blue (was orange→blue by depth)
        vec3 cNear = vec3(6.5, 1.6, 7.0);
        vec3 cFar  = vec3(2.2, 1.2, 8.5);
        O += mix(cNear, cFar, clamp(z / 14.0, 0.0, 1.0)) * den * density;
    }
    O = max(O, 0.0);
    vec3 col = tanh3(O * O / (1000.0 / exposure));
    return col;
}

vec3 dust(vec2 I) {
    if (dustAmt <= 0.001) return vec3(0.0);
    float amt = audioReact;
    float t = TIME;
    vec3 acc = vec3(0.0);
    float tilt = dustTilt * 0.9;
    float ct = cos(tilt), st = sin(tilt);
    for (int i = 0; i < 56; i++) {
        float fi = float(i);
        float h1 = hash11(fi * 1.37 + 0.5), h2 = hash11(fi * 3.11 + 1.7), h3 = hash11(fi * 5.03 + 2.9);
        float ang = fi * (TAU / 56.0) + h1 * 0.3 + t * dustSpin * 0.25 * (0.7 + 0.6 * h2);
        float rr  = dustRadius + (h2 - 0.5) * dustThick + 0.25 * sin(t * 0.7 + fi);
        float yy  = dustHeight + (h3 - 0.5) * dustThick + 0.35 * sin(t * 0.5 + fi * 1.9);
        vec3 w = vec3(cos(ang) * rr, yy, sin(ang) * rr);
        // tilt the ring about x
        w.yz = vec2(w.y * ct - w.z * st, w.y * st + w.z * ct);
        float depth;
        vec2 s = project(w, depth);
        if (depth < 0.5) continue;
        float px = dustSize * R.y * 0.0045 * (9.0 / depth);
        float dd = length(I - s) / max(px, 0.5);
        float glow = exp(-dd * dd * 2.0) + 0.25 * exp(-dd * 0.9);
        float twinkle = 0.55 + 0.45 * sin(t * (1.5 + h3 * 2.0) + fi * 2.3);
        twinkle *= 1.0 + amt * 0.6 * highP();
        vec3 dc = mix(vec3(0.85, 0.7, 1.0), vec3(1.0, 0.92, 0.98), h1);
        acc += dc * glow * twinkle * (0.35 + 0.65 * h2) * clamp(9.0 / depth, 0.3, 2.0);
    }
    return acc * dustAmt * 0.16;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        vec2 I = gl_FragCoord.xy;
        float amt = audioReact;
        vec3 col = flame(I);
        // subtle hue weather: slow drift + mids nudge, around the purple base
        float h = hueBase + hueDrift * 0.12 * sin(TIME * hueSpeed * 0.35)
                + hueDrift * 0.05 * sin(TIME * hueSpeed * 0.11 + 2.0)
                + amt * 0.05 * midP();
        col = hueRotate(col, h);
        float l = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(vec3(l), col, saturation);
        col += dust(I);
        col += (hash21(I + fract(TIME) * vec2(17.0, 29.0)) - 0.5) * 0.01;
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else if (PASSINDEX == 1) {
        vec3 col  = texture2D(cgScene, uv).rgb;
        vec3 prev = texture2D(abTrail, uv).rgb;
        if (any(notEqual(prev, prev))) prev = vec3(0.0);
        float decay = 0.50 + 0.46 * trailAmt;
        col = max(col, prev * decay - 0.0045);
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else {
        vec2 dir = uv - 0.5;
        float ab = aberration * 0.0045;
        vec3 base;
        base.r = texture2D(abTrail, uv + dir * ab).r;
        base.g = texture2D(abTrail, uv).g;
        base.b = texture2D(abTrail, uv - dir * ab).b;

        // sunrays: radial blur of the bright core, stepping toward the source
        vec3 rays = vec3(0.0);
        if (rayAmt > 0.001) {
            vec2 src = vec2(rayX, rayY);
            vec2 toSrc = (src - uv) * (0.15 + 0.85 * rayLength);
            float jitter = hash21(gl_FragCoord.xy + fract(TIME) * 3.0);
            float wsum = 0.0;
            for (int i = 0; i < 24; i++) {
                float f = (float(i) + jitter) / 24.0;
                vec2 su = uv + toSrc * f;
                float w = 1.0 - f;
                vec3 s = texture2D(abTrail, su).rgb;
                rays += max(s - 0.35, 0.0) * w;
                wsum += w;
            }
            rays /= wsum;
            rays *= rayAmt * 0.9 * (1.0 + audioReact * 0.5 * clamp(audioHigh, 0.0, 1.0));
        }

        vec3 bl = vec3(0.0);
        for (int i = 0; i < 8; i++) {
            float an = float(i) * 0.7853982;
            vec2 o = vec2(cos(an), sin(an)) * (3.5 / R.y);
            bl += texture2D(abTrail, uv + o).rgb;
            bl += texture2D(abTrail, uv + o * 2.6).rgb * 0.6;
        }
        bl /= 12.8;
        bl = max(bl - 0.5, 0.0);
        vec3 col = base + rays + bl * bl * bloomAmt * 1.8;
        float lvl = clamp(audioLevel, 0.0, 1.0);
        col *= brightness * mix(1.0, 0.70 + 0.30 * lvl, audioReact)
             * (1.0 + audioReact * 0.05 * clamp(audioBeatPulse, 0.0, 1.0));
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
