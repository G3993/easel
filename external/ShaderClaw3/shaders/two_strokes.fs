/*{
  "DESCRIPTION": "2 Strokes — a small swarm of glowing particles, each carrying two heads, wandering a sinusoidal flow field. Every head throws a wide 1/d halo in its own slowly cycling colour; halos overlap and square up into hot cores and soft blooms, and each head drags a short stroke behind it. Bass thickens the halos, beats kick the flow, mids cycle the colour, highs sharpen the cores. Every parameter is exposed.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-23 — after a Shadertoy two-head particle sketch pasted by Lu",
  "CATEGORIES": ["Generator", "Particles", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "parts",       "LABEL": "Particles",        "TYPE": "float", "MIN": 4.0,  "MAX": 64.0, "DEFAULT": 24.0, "GROUP": "Swarm" },
    { "NAME": "speed",       "LABEL": "Speed",            "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "flowScale",   "LABEL": "Flow Scale",       "TYPE": "float", "MIN": 0.2,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "flowDrift",   "LABEL": "Flow Drift",       "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "headSplit",   "LABEL": "Head Split",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Motion" },
    { "NAME": "haloSize",    "LABEL": "Halo Size",        "TYPE": "float", "MIN": 0.2,  "MAX": 4.0,  "DEFAULT": 1.8,  "GROUP": "Glow" },
    { "NAME": "coreSize",    "LABEL": "Core Size",        "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.4,  "GROUP": "Glow" },
    { "NAME": "gamma",       "LABEL": "Halo Curve",       "TYPE": "float", "MIN": 1.0,  "MAX": 4.0,  "DEFAULT": 2.0,  "GROUP": "Glow" },
    { "NAME": "trailAmt",    "LABEL": "Strokes",          "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.4,  "GROUP": "Glow" },
    { "NAME": "colorSpeed",  "LABEL": "Colour Cycle",     "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "colorSpread", "LABEL": "Colour Spread",    "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",        "TYPE": "float", "MIN": -0.5, "MAX": 0.5,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "saturation",  "LABEL": "Saturation",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.6,  "DEFAULT": 1.1,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",       "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Color" }
  ],
  "PASSES": [
    { "TARGET": "tsState", "PERSISTENT": true },
    { "TARGET": "tsScene" },
    { "TARGET": "tsTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// 2 STROKES
//   pass 0  tsState — particle state in the two bottom pixel rows:
//                     row 0 = head A, row 1 = head B, column = particle.
//                     Positions normalised 0..1, packed hi/lo per axis
//                     (rg = x, ba = y) so they survive 8-bit / half-float.
//   pass 1  tsScene — per pixel: sum of 1/d halos from every head, squared.
//   pass 2  tsTrail — short additive strokes.
//   pass 3  final   — HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define MAXP 64

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
    float a = h * 6.28318531; vec3 k = vec3(0.57735);
    float cs = cos(a), sn = sin(a);
    return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
}

// hi/lo packing of a 0..1 value into two channels
vec2 pack(float v) { v = clamp(v, 0.0, 0.99999); float hi = floor(v * 255.0) / 255.0; return vec2(hi, fract(v * 255.0)); }
float unpack(vec2 p) { return p.x + p.y / 255.0; }

vec2 readHead(int i, int row) {
    vec4 s = texture2D(tsState, vec2((float(i) + 0.5) / R.x, (float(row) + 0.5) / R.y));
    if (any(notEqual(s, s))) s = vec4(0.5, 0.0, 0.5, 0.0);
    return vec2(unpack(s.rg), unpack(s.ba));
}

vec2 seedPos(int i, int row) {
    float fi = float(i) + float(row) * 37.0;
    return vec2(hash11(fi * 1.37 + 0.5), hash11(fi * 3.11 + 1.7));
}

// flow field in normalised space — the reference's sin(pos.yxwz/60 + t)
vec2 flow(vec2 p, vec2 other, float t) {
    vec2 k = R / 60.0 * flowScale;
    return vec2(sin(p.y * k.y + t), sin(p.x * k.x + t))
         + headSplit * 0.6 * vec2(sin(other.y * k.y * 0.7 + t * 1.3), sin(other.x * k.x * 0.7 + t * 1.1));
}

vec3 headColor(int i, int row, vec2 p, float t) {
    float fi = float(i);
    vec3 freq = row == 0 ? vec3(0.01, 0.014, 0.018) : vec3(0.02, 0.014, 0.018);
    float ph = fi * 0.7 * colorSpread + float(row) * 1.3;
    vec2 px = p * R;
    return 0.5 + 0.5 * sin(freq * (px.x + px.y * 0.5) * colorSpread + ph + t * colorSpeed);
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    float ar = audioReact;
    int n = int(parts + 0.5);

    if (PASSINDEX == 0) {
        // only the two bottom rows hold state
        if (gl_FragCoord.y > 2.0) { gl_FragColor = vec4(0.0); return; }
        int i = int(gl_FragCoord.x);
        int row = int(gl_FragCoord.y);
        if (i >= n) { gl_FragColor = vec4(0.0); return; }
        vec2 p;
        if (FRAMEINDEX < 2) {
            p = seedPos(i, row);
        } else {
            p = readHead(i, row);
            vec2 other = readHead(i, 1 - row);
            float t = TIME * 0.6 * speed + ar * 0.5 * audioBassTime;
            float kick = 1.0 + ar * 0.6 * beatP();
            vec2 vel = flow(p, other, t) * 1.5 / R.y * 1.6 * speed * flowDrift * kick;
            vel *= vec2(R.y / R.x, 1.0);
            p = fract(p + vel + 1.0);
        }
        gl_FragColor = vec4(pack(p.x), pack(p.y));
    } else if (PASSINDEX == 1) {
        vec2 frag = gl_FragCoord.xy;
        float t = TIME * speed;
        vec3 col = vec3(0.0);
        float halo = 0.3 * haloSize * R.y / 1080.0 * 2.2 * (1.0 + ar * 0.35 * bassP());
        float core = coreSize * (1.0 + ar * 0.5 * highP());
        for (int i = 0; i < MAXP; i++) {
            if (i >= n) break;
            for (int row = 0; row < 2; row++) {
                vec2 h = readHead(i, row) * R;
                // wrap-aware distance
                vec2 dv = frag - h;
                dv = dv - R * floor(dv / R + 0.5);
                float d = length(dv);
                vec3 c = headColor(i, row, h / R, t);
                col += halo / (d + 1.0) * c;
                col += core * 2.5 * exp(-d * d * 0.06) * (0.6 + 0.4 * c);
            }
        }
        col = pow(max(col, 0.0), vec3(gamma));
        gl_FragColor = vec4(col, 1.0);
    } else if (PASSINDEX == 2) {
        vec3 col  = texture2D(tsScene, uv).rgb;
        vec3 prev = texture2D(tsTrail, uv).rgb;
        if (FRAMEINDEX < 2 || any(notEqual(prev, prev))) prev = vec3(0.0);
        float decay = 0.55 + 0.42 * trailAmt;
        col = max(col, prev * decay - 0.004);
        gl_FragColor = vec4(clamp(col, 0.0, 8.0), 1.0);
    } else {
        vec3 col = texture2D(tsTrail, uv).rgb;
        col = hueRotate(col, hueShift + ar * 0.05 * midP());
        float lvl = levP();
        col *= brightness * mix(1.0, 0.70 + 0.34 * lvl + 0.16 * bassP(), ar)
             * (1.0 + ar * 0.05 * beatP());
        col = col / (1.0 + col * 0.25);
        float l0 = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(vec3(l0), col, saturation);
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
