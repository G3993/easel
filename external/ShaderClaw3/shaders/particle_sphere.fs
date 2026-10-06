/*{
  "DESCRIPTION": "Particle Sphere — a hollow sphere of glowing dust seen through a shallow lens: hundreds of particles drift over the shell on a 3D noise flow, each rendered as a bokeh disc whose size follows its distance from a breathing focus plane, so the shell sharpens into pin-points where it is in focus and blooms into soft coloured circles elsewhere. Slow rotation, deep violet vignette. Bass swells the bokeh, beats kick the drift, mids shift the hue, highs sharpen the focus. Every parameter is exposed.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-23 — after a Shadertoy nearest-particle DOF sketch pasted by Lu (rebuilt analytically; the per-pixel particle tracker does not survive half-float / 8-bit buffers)",
  "CATEGORIES": ["Generator", "Particles", "3D", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "count",       "LABEL": "Particles",        "TYPE": "float", "MIN": 64.0, "MAX": 512.0,"DEFAULT": 420.0,"GROUP": "Swarm" },
    { "NAME": "radius",      "LABEL": "Sphere Radius",    "TYPE": "float", "MIN": 0.3,  "MAX": 1.5,  "DEFAULT": 0.85, "GROUP": "Swarm" },
    { "NAME": "shellFuzz",   "LABEL": "Shell Thickness",  "TYPE": "float", "MIN": 0.0,  "MAX": 0.5,  "DEFAULT": 0.08, "GROUP": "Swarm" },
    { "NAME": "spin",        "LABEL": "Spin",             "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": 0.2,  "GROUP": "Motion" },
    { "NAME": "drift",       "LABEL": "Drift",            "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "driftScale",  "LABEL": "Drift Scale",      "TYPE": "float", "MIN": 0.5,  "MAX": 4.0,  "DEFAULT": 1.5,  "GROUP": "Motion" },
    { "NAME": "focusBreath", "LABEL": "Focus Breath",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Lens" },
    { "NAME": "focusDist",   "LABEL": "Focus Distance",   "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Lens" },
    { "NAME": "aperture",    "LABEL": "Aperture",         "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Lens" },
    { "NAME": "pointSize",   "LABEL": "Point Size",       "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Lens" },
    { "NAME": "camDist",     "LABEL": "Camera Distance",  "TYPE": "float", "MIN": 1.2,  "MAX": 5.0,  "DEFAULT": 2.4,  "GROUP": "Lens" },
    { "NAME": "colA",        "LABEL": "Colour A",         "TYPE": "color", "DEFAULT": [1.0, 0.35, 0.2, 1.0],  "GROUP": "Color" },
    { "NAME": "colB",        "LABEL": "Colour B",         "TYPE": "color", "DEFAULT": [0.3, 0.6, 1.0, 1.0],   "GROUP": "Color" },
    { "NAME": "colC",        "LABEL": "Colour C",         "TYPE": "color", "DEFAULT": [0.9, 0.9, 1.0, 1.0],   "GROUP": "Color" },
    { "NAME": "bgTint",      "LABEL": "Background",       "TYPE": "color", "DEFAULT": [0.06, 0.01, 0.16, 1.0], "GROUP": "Color" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",        "TYPE": "float", "MIN": -0.5, "MAX": 0.5,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "chroma",      "LABEL": "Chromatic Split",  "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Color" },
    { "NAME": "exposure",    "LABEL": "Exposure",         "TYPE": "float", "MIN": 0.3,  "MAX": 4.0,  "DEFAULT": 2.4,  "GROUP": "Color" },
    { "NAME": "saturation",  "LABEL": "Saturation",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.6,  "DEFAULT": 1.1,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",       "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Color" }
  ],
  "PASSES": [
    { "TARGET": "psScene" },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// PARTICLE SPHERE
//   pass 0  psScene — analytic particles on a sphere shell (index → seed
//                     point + integrated sine-noise drift), projected,
//                     splatted as depth-dependent bokeh discs.
//   pass 1  final   — vignette tint + HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define TAU 6.28318531
#define MAXN 512

float hash11(float n) { return fract(sin(n * 91.3458) * 47453.5453); }
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
    float a = h * TAU; vec3 k = vec3(0.57735);
    float cs = cos(a), sn = sin(a);
    return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
}
mat2 rot(float a) { return mat2(cos(a), -sin(a), sin(a), cos(a)); }

// particle i at time t: shell point + smooth sine-noise drift over the shell
vec3 particle(int i, float t, out float seed) {
    float fi = float(i);
    float h1 = hash11(fi * 1.37 + 0.5), h2 = hash11(fi * 3.11 + 1.7), h3 = hash11(fi * 5.03 + 2.9);
    seed = h3;
    // even-ish shell distribution
    float u = TAU * h1;
    float v = acos(2.0 * h2 - 1.0);
    // drift: integrated sine field over (u, v)
    float ds = driftScale;
    u += drift * (0.35 * sin(t * 0.31 + fi * 0.7 + v * ds) + 0.2 * sin(t * 0.53 + h3 * TAU));
    v += drift * (0.25 * sin(t * 0.27 + fi * 1.3 + u * ds * 0.6) + 0.12 * sin(t * 0.41 + h1 * TAU));
    float r = radius * (1.0 + shellFuzz * (h3 - 0.5) * 2.0 + 0.03 * sin(t * 0.7 + fi));
    return vec3(sin(v) * cos(u), cos(v), sin(v) * sin(u)) * r;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    float ar = audioReact;
    if (PASSINDEX == 0) {
        vec2 p = (2.0 * gl_FragCoord.xy - R) / R.y;
        float t = TIME + ar * 0.5 * audioBassTime;
        int n = int(count + 0.5);
        float yaw = TIME * spin * 0.4;
        float focus = focusDist + focusBreath * 0.5 * sin(TIME * 0.5);
        focus += ar * 0.2 * highP();
        float ap = aperture * (1.0 + ar * 0.45 * bassP());
        vec3 acc = vec3(0.0);
        for (int i = 0; i < MAXN; i++) {
            if (i >= n) break;
            float seed;
            vec3 w = particle(i, t, seed);
            w.xz *= rot(yaw);
            w.xy *= rot(0.35);
            // perspective: camera on +z looking at origin
            float z = camDist - w.z;
            if (z < 0.2) continue;
            vec2 s = w.xy / z * 1.8;
            float dz = (w.z - focus);
            float coc = (0.004 + ap * 0.06 * abs(dz)) * pointSize;   // circle of confusion (screen units)
            vec2 dv = p - s;
            float d = length(dv);
            if (d > coc + 0.01) continue;
            // bokeh disc: bright rim, normalised so energy is conserved
            float disc = smoothstep(coc, coc * 0.78, d);
            float rim  = smoothstep(coc * 0.6, coc * 0.95, d) * 0.6;
            float e = (disc * (0.55 + rim)) / (coc * coc * 2200.0 + 0.4);
            // chromatic split: R/G/B discs slightly different size
            vec3 cw = vec3(smoothstep(coc * (1.0 + chroma * 0.18), coc * 0.78, d),
                           disc,
                           smoothstep(coc * (1.0 - chroma * 0.15), coc * 0.7, d));
            cw = mix(vec3(disc), cw, chroma);
            vec3 c = seed < 0.4 ? mix(colA.rgb, colB.rgb, seed / 0.4)
                   : seed < 0.8 ? mix(colB.rgb, colC.rgb, (seed - 0.4) / 0.4)
                                : mix(colC.rgb, colA.rgb, (seed - 0.8) / 0.2);
            float depthFade = clamp(1.4 - z * 0.35, 0.3, 1.2);
            acc += c * cw * e * depthFade * 1.6;
        }
        gl_FragColor = vec4(acc * exposure, 1.0);
    } else {
        vec3 col = texture2D(psScene, uv).rgb;
        // violet vignette + grain (the reference's length(uv-.5) tint)
        float vg = length(uv - 0.5);
        col += bgTint.rgb * (0.25 + vg * 1.0);
        col = hueRotate(col, hueShift + ar * 0.05 * midP());
        float lvl = levP();
        col *= brightness * mix(1.0, 0.72 + 0.34 * lvl + 0.14 * bassP(), ar)
             * (1.0 + ar * 0.05 * beatP());
        col = col / (1.0 + col * 0.3);
        float l0 = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(vec3(l0), col, saturation);
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.01;
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
