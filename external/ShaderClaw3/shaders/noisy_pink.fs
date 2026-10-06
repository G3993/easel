/*{
  "DESCRIPTION": "Noisy Pink — a self-feeding reaction automaton: a red field diffuses through its own gradients, wraps on a slowly breathing ceiling and is fed a sine gradient from the frame edges, growing cellular ridges, veins and grainy pink plates that never settle. State is mapped to a deep-magenta → hot-pink → blush palette with crisp ridge lines and film grain. Bass feeds the field, beats push the ceiling phase, mids shift the hue, highs add grain. Every parameter is exposed.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-23 — after a Shadertoy feedback-buffer automaton pasted by Lu",
  "CATEGORIES": ["Generator", "Feedback", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "speed",       "LABEL": "Speed",            "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "breath",      "LABEL": "Ceiling Breath",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Motion" },
    { "NAME": "feed",        "LABEL": "Feed",             "TYPE": "float", "MIN": 0.0,  "MAX": 0.1,  "DEFAULT": 0.03, "GROUP": "Field" },
    { "NAME": "diffusion",   "LABEL": "Diffusion",        "TYPE": "float", "MIN": 0.8,  "MAX": 1.05, "DEFAULT": 0.93, "GROUP": "Field" },
    { "NAME": "gradientAmt", "LABEL": "Gradient Warp",    "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Field" },
    { "NAME": "pixelScale",  "LABEL": "Cell Scale",       "TYPE": "float", "MIN": 1.0,  "MAX": 6.0,  "DEFAULT": 2.0,  "GROUP": "Field" },
    { "NAME": "edgeWidth",   "LABEL": "Edge Feed Width",  "TYPE": "float", "MIN": 0.0,  "MAX": 0.25, "DEFAULT": 0.05, "GROUP": "Field" },
    { "NAME": "seedNoise",   "LABEL": "Seed Noise",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Field" },
    { "NAME": "colDark",     "LABEL": "Shadow",           "TYPE": "color", "DEFAULT": [0.12, 0.0, 0.08, 1.0],  "GROUP": "Color" },
    { "NAME": "colMid",      "LABEL": "Magenta",          "TYPE": "color", "DEFAULT": [0.85, 0.05, 0.45, 1.0], "GROUP": "Color" },
    { "NAME": "colHi",       "LABEL": "Hot Pink",         "TYPE": "color", "DEFAULT": [1.0, 0.35, 0.7, 1.0],   "GROUP": "Color" },
    { "NAME": "colTop",      "LABEL": "Blush",            "TYPE": "color", "DEFAULT": [1.0, 0.85, 0.92, 1.0],  "GROUP": "Color" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",        "TYPE": "float", "MIN": -0.5, "MAX": 0.5,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "ridgeAmt",    "LABEL": "Ridge Lines",      "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.8,  "GROUP": "Color" },
    { "NAME": "grainAmt",    "LABEL": "Grain",            "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.2, "GROUP": "Color" },
    { "NAME": "contrast",    "LABEL": "Contrast",         "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "detail",      "LABEL": "Local Detail",     "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "saturation",  "LABEL": "Saturation",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.6,  "DEFAULT": 1.1,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",       "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Color" }
  ],
  "PASSES": [
    { "TARGET": "npState", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// NOISY PINK
//   pass 0  npState — the automaton (verbatim dynamics of the reference):
//                     .r field, .g/.b hold the horizontal/vertical gradients,
//                     4-tap average fed back with a breathing mod ceiling
//                     and a sine gradient injected at the frame border.
//   pass 1  final   — state → pink palette, ridge lines from the gradient,
//                     grain, HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float bassP()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.4); }
float midP()   { return pow(smoothstep(0.06, 0.85, audioMid),  1.2); }
float highP()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }
float beatP()  { return clamp(audioBeatPulse, 0.0, 1.0); }
float levP()   { return clamp(audioLevel, 0.0, 1.0); }

vec3 hueRotate(vec3 c, float h) {
    float a = h * 6.28318531;
    vec3 k = vec3(0.57735);
    float cs = cos(a), sn = sin(a);
    return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
}

vec3 S(vec2 tv, vec2 o) {
    vec3 v = texture2D(npState, tv + o).rgb;
    if (any(notEqual(v, v))) v = vec3(0.0);
    return v;
}

void main() {
    vec2 tv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        float ar = audioReact;
        // clock: speed-scaled, beats/bass push the ceiling phase
        float t = TIME * speed + ar * 0.6 * audioBassTime;

        if (FRAMEINDEX < 2) {
            // spawn visible: noisy gradient seed
            float n = hash21(gl_FragCoord.xy * 0.37 + 3.1);
            float g = 0.5 + 0.5 * sin((tv.x + tv.y) * 6.0);
            gl_FragColor = vec4(mix(g * 0.6, n, seedNoise), 0.0, 0.0, 1.0);
            return;
        }

        vec2 uv = -1.0 + 2.0 * tv;
        vec2 px = (1.0 / R) * pixelScale;
        vec3 c = S(tv, vec2(0.0));
        px *= (1.0 - c.r) * c.r * 4.0 * gradientAmt + (1.0 - gradientAmt) * 0.5;
        vec3 col = vec3(0.0);
        vec3 u = S(tv, vec2(0.0,  px.y));
        vec3 d = S(tv, vec2(0.0, -px.y));
        vec3 l = S(tv, vec2( px.x, 0.0));
        vec3 r = S(tv, vec2(-px.x, 0.0));
        vec3 udlr = (u + d + l + r) * 0.25;

        float ew = edgeWidth;
        if (tv.x < ew || tv.y < ew || tv.x > 1.0 - ew || tv.y > 1.0 - ew) {
            col.r = sin(uv.x + uv.y + t * 0.1) * 0.5 + 0.5;
        }
        if (u.r != d.r) col.b = (u.r - d.r) * 500.0;
        if (l.r != r.r) col.g = (l.r - r.r) * 500.0;

        vec3 c2 = S(tv, vec2((col.b - 0.5) * px.x, (col.g - 0.5) * px.y));
        float fd = feed * (1.0 + ar * 0.8 * bassP());
        col.r += (u.r + d.r + l.r + r.r) / 4.3;
        float ceil = sin(t * 0.1) * 0.5 * breath + (1.0 - 0.5 * breath) * 0.5 + 0.5;
        col.r = mod(col.r + fd, max(ceil, 0.05)) * (0.96 + (diffusion - 0.93) * 0.5);
        col.r += c2.r * 0.2;
        col.b *= 0.001;
        col.g *= 0.01;
        col = mod(col * diffusion, 1.0);
        col = mix(col, (udlr + c) * 0.5, col.b - col.r * 0.95);
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else {
        float ar = audioReact;
        vec3 s = S(tv, vec2(0.0));
        float v = s.r;

        // ridge lines: gradient magnitude of the field, tight
        vec2 e = 1.0 / R;
        float gx = S(tv, vec2(e.x, 0.0)).r - S(tv, vec2(-e.x, 0.0)).r;
        float gy = S(tv, vec2(0.0, e.y)).r - S(tv, vec2(0.0, -e.y)).r;
        float ridge = clamp(length(vec2(gx, gy)) * 18.0, 0.0, 1.0);
        ridge = smoothstep(0.12, 0.55, ridge);

        // palette: shadow → magenta → hot pink → blush
        // local contrast: the field saturates high, so read structure as
        // deviation from a wide neighbourhood + a global tilt
        vec2 e6 = 6.0 / R;
        float avg = (S(tv, vec2(e6.x, 0.0)).r + S(tv, vec2(-e6.x, 0.0)).r
                   + S(tv, vec2(0.0, e6.y)).r + S(tv, vec2(0.0, -e6.y)).r
                   + S(tv, e6).r + S(tv, -e6).r + S(tv, vec2(e6.x, -e6.y)).r + S(tv, vec2(-e6.x, e6.y)).r) * 0.125;
        float vv = 0.42 + (v - avg) * 2.2 * detail + (v - 0.78) * 1.1 * contrast;
        vv = clamp(vv, 0.0, 1.0);
        vec3 col = vv < 0.5 ? mix(colDark.rgb, colMid.rgb, vv * 2.0)
                            : mix(colMid.rgb, colHi.rgb, vv * 2.0 - 1.0);
        col = mix(col, colTop.rgb, smoothstep(0.9, 1.0, vv) * 0.8);
        col += mix(colHi.rgb, colTop.rgb, 0.5) * ridge * ridgeAmt * 0.3;
        col = mix(col, colDark.rgb * 0.5, ridge * ridgeAmt * 0.35 * (1.0 - vv));

        // hue: knob + mids drift
        col = hueRotate(col, hueShift + ar * 0.04 * midP());

        // grain: film noise, highs add more
        float g = hash21(gl_FragCoord.xy + fract(TIME) * vec2(53.0, 91.0)) - 0.5;
        col *= 1.0 + g * (grainAmt * 0.35 + ar * 0.25 * highP());

        float lvl = levP();
        col *= brightness * mix(1.0, 0.72 + 0.36 * lvl + 0.16 * bassP(), ar)
             * (1.0 + ar * 0.05 * beatP());
        float l0 = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(vec3(l0), col, saturation);
        // HD finisher
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
