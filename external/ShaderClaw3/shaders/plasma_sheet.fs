/*{
  "DESCRIPTION": "Plasma Sheet — a volumetric sheet of plasma folded through domain-warped space: thin luminous membranes where two sine fields cancel, wrapped around a glowing spherical shell, colour cycling through a full spectral ramp with depth. Extras: electric filaments along the membrane edges, a beat-driven shock shell that races outward from the core, an optional kaleidoscope fold, and slow hue weather. Bass feeds the shell glow, beats fire the shock, mids push the warp, highs flicker the filaments.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-11 — after the sin/cos membrane raymarch",
  "CATEGORIES": ["Generator", "3D", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "flowSpeed",   "LABEL": "Flow Speed",      "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Plasma" },
    { "NAME": "warpAmt",     "LABEL": "Warp",            "TYPE": "float", "MIN": 0.0,  "MAX": 0.6,  "DEFAULT": 0.24, "GROUP": "Plasma" },
    { "NAME": "warpOct",     "LABEL": "Warp Octaves",    "TYPE": "float", "MIN": 1.0,  "MAX": 6.0,  "DEFAULT": 4.0,  "GROUP": "Plasma" },
    { "NAME": "sheetFreq",   "LABEL": "Sheet Frequency", "TYPE": "float", "MIN": 0.5,  "MAX": 3.0,  "DEFAULT": 1.2,  "GROUP": "Plasma" },
    { "NAME": "sheetThin",   "LABEL": "Sheet Thinness",  "TYPE": "float", "MIN": 2.0,  "MAX": 16.0, "DEFAULT": 9.0,  "GROUP": "Plasma" },
    { "NAME": "shellRadius", "LABEL": "Shell Radius",    "TYPE": "float", "MIN": 0.5,  "MAX": 3.5,  "DEFAULT": 1.65, "GROUP": "Plasma" },
    { "NAME": "shellGlow",   "LABEL": "Shell Glow",      "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.2,  "GROUP": "Plasma" },
    { "NAME": "coreGlow",    "LABEL": "Core Glow",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.15, "GROUP": "Plasma" },
    { "NAME": "stepSize",    "LABEL": "March Step",      "TYPE": "float", "MIN": 0.02, "MAX": 0.12, "DEFAULT": 0.045, "GROUP": "Plasma" },
    { "NAME": "camDist",     "LABEL": "Camera Distance", "TYPE": "float", "MIN": 2.0,  "MAX": 8.0,  "DEFAULT": 4.2,  "GROUP": "Camera" },
    { "NAME": "camFov",      "LABEL": "Lens",            "TYPE": "float", "MIN": 0.8,  "MAX": 3.0,  "DEFAULT": 1.65, "GROUP": "Camera" },
    { "NAME": "camOrbit",    "LABEL": "Orbit",           "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": 0.1,  "GROUP": "Camera" },
    { "NAME": "kaleido",     "LABEL": "Kaleido Fold",    "TYPE": "float", "MIN": 0.0,  "MAX": 8.0,  "DEFAULT": 0.0,  "GROUP": "Camera" },
    { "NAME": "colorFreq",   "LABEL": "Colour Bands",    "TYPE": "float", "MIN": 1.0,  "MAX": 14.0, "DEFAULT": 7.0,  "GROUP": "Color" },
    { "NAME": "hueBase",     "LABEL": "Hue",             "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "hueDrift",    "LABEL": "Hue Drift",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.2,  "GROUP": "Color" },
    { "NAME": "depthTint",   "LABEL": "Depth Tint",      "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "saturation",  "LABEL": "Saturation",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.6,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "exposure",    "LABEL": "Exposure",        "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,  "DEFAULT": 1.25, "GROUP": "Color" },
    { "NAME": "vignette",    "LABEL": "Vignette",        "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",      "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "filaments",   "LABEL": "Filaments",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6, "GROUP": "Extras" },
    { "NAME": "shockAmt",    "LABEL": "Shock Shell",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.4,  "GROUP": "Extras" },
    { "NAME": "shockRate",   "LABEL": "Shock Rate",      "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.5,  "GROUP": "Extras" },
    { "NAME": "audioReact",  "LABEL": "Audio React",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.1,  "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.4,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "psScene" },
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// PLASMA SHEET
//   pass 0  psScene — 110-step volumetric march. Density lives where
//                     g = dot(sin(q*a), cos(q.yzx*b)) crosses zero (thin
//                     membranes), boosted near a spherical shell |r-R|.
//                     Extras: filaments (second, much thinner membrane
//                     term, flickering), a shock shell (bright sphere that
//                     races out from the core on beats), kaleido fold.
//   pass 1  abTrail — house motion trail.
//   pass 2  final   — house bloom + aberration + HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define PI  3.14159265
#define TAU 6.28318531

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

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

vec3 renderScene() {
    vec2 I = gl_FragCoord.xy;
    vec2 u = (I * 2.0 - R) / R.y;
    float amt = audioReact;

    // kaleido fold (0 = off)
    if (kaleido >= 1.0) {
        float n = floor(kaleido + 0.5);
        float an = atan(u.y, u.x);
        float seg = TAU / n;
        an = abs(mod(an + seg * 0.5, seg) - seg * 0.5);
        u = length(u) * vec2(cos(an), sin(an));
    }

    // phase-push clocks (never time-scale)
    float t  = TIME * flowSpeed + amt * 0.30 * audioMidTime;
    float tw = TIME * flowSpeed * 0.8 + amt * 0.25 * audioMidTime;

    vec3 d = normalize(vec3(u, -camFov));
    float orb = camOrbit * TIME * 0.15;
    mat2 orbM = mat2(cos(orb), -sin(orb), sin(orb), cos(orb));
    d.xz = orbM * d.xz;

    float z = 0.05, g = 0.0;
    vec3 O = vec3(0.0);
    vec3 fil = vec3(0.0);
    float shock = 0.0;

    // shock shell: radius grows with time+beats, wraps, re-fires
    float shockClock = TIME * shockRate * 0.6 + amt * 0.8 * audioBassTime * shockRate;
    float shockR = fract(shockClock) * 3.2;
    float shockW = 0.08 + 0.10 * fract(shockClock);
    float shockA = shockAmt * (1.0 - fract(shockClock)) * (0.4 + 0.6 * amt * (0.5 + 0.5 * beatP()) + 0.6 * (1.0 - amt));

    vec3 camPos = vec3(0.0, 0.0, camDist);
    camPos.xz = orbM * camPos.xz;

    int oct = int(warpOct);
    float thin = sheetThin;
    float sr = shellRadius;
    float sg = shellGlow * (1.0 + amt * 0.5 * bassP());

    for (int i = 0; i < 110; i++) {
        if (z >= 10.0) break;
        vec3 p = camPos + d * z;
        vec3 q = p;
        float f = 1.0;
        for (int k = 0; k < 6; k++) {
            if (k >= oct) break;
            q += sin(q.yzx * f + tw + vec3(0.0, 2.0, 4.0)) * warpAmt / f;
            f *= 2.0;
        }
        float r = length(p);
        g = dot(sin(q * sheetFreq * 1.04), cos(q.yzx * sheetFreq * 0.96));
        float kk = 4.0 * (0.7 * p.z + 0.35 * p.x) / max(r, 0.05);
        vec3 pal = 0.5 + 0.5 * cos(g * colorFreq + sin(q.x * 2.0 + q.y * 3.0 - 2.0 * t + r * 4.0) + t
                                   + vec3(kk * depthTint, 1.5, 4.0 - kk * depthTint));
        float shell = 0.35 + sg * exp(-2.5 * abs(r - sr));
        float dens = exp(-thin * abs(g)) * shell + coreGlow * exp(-2.2 * r);
        float fog = exp(-0.18 * z);
        O += pal * dens * fog * 0.035;

        // filaments: a much thinner membrane, flickering, white-hot
        if (filaments > 0.001) {
            float fl = exp(-thin * 9.0 * abs(g)) * shell;
            float flick = 0.6 + 0.4 * sin(TIME * 9.0 + r * 12.0 + g * 30.0) * (0.5 + amt * highP());
            fil += (pal * 0.4 + 0.6) * fl * flick * fog * 0.035;
        }
        // shock shell: bright thin sphere around the core
        if (shockAmt > 0.001) {
            shock += exp(-pow((r - shockR) / shockW, 2.0)) * exp(-thin * 0.5 * abs(g)) * fog * 0.07;
        }

        z += stepSize * (1.0 + abs(g));
    }

    vec3 col = O + fil * filaments + vec3(0.85, 0.9, 1.0) * shock * shockA;
    col *= exposure;
    col = 1.0 - exp(-2.0 * col);
    float vig = mix(1.0, 0.35 + 1.55 * smoothstep(1.4, 0.2, length(u)), vignette);
    col *= vig;

    float h = hueBase + hueDrift * 0.15 * sin(TIME * 0.13) + hueDrift * 0.06 * sin(TIME * 0.041 + 1.0)
            + amt * 0.04 * midP();
    col = hueRotate(col, h);
    float l = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(l), col, saturation);
    col *= 1.0 + amt * 0.10 * levP();
    col += (hash21(I + fract(TIME) * vec2(17.0, 29.0)) - 0.5) * 0.01;
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        gl_FragColor = vec4(clamp(renderScene(), 0.0, 1.0), 1.0);
    } else if (PASSINDEX == 1) {
        vec3 col  = texture2D(psScene, uv).rgb;
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
        vec3 bl = vec3(0.0);
        for (int i = 0; i < 8; i++) {
            float an = float(i) * 0.7853982;
            vec2 o = vec2(cos(an), sin(an)) * (3.5 / R.y);
            bl += texture2D(abTrail, uv + o).rgb;
            bl += texture2D(abTrail, uv + o * 2.6).rgb * 0.6;
        }
        bl /= 12.8;
        bl = max(bl - 0.5, 0.0);
        vec3 col = base + bl * bl * bloomAmt * 1.8;
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
