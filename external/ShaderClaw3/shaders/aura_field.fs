/*{
  "DESCRIPTION": "Aura Field — a radiant aura poster. A layered radial glow breathes from white through gold and hot pink into magenta, framed by a muted grey-mauve border under heavy fine film grain. A delicate orbital cage of hairline white ellipse field-lines — like a magnetic-dipole diagram, some dashed — slowly precesses around the glow core. The core breathes with the music's energy, bass gently swells the aura's reach, and highs make individual field-lines glint bright one at a time.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Geometry",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "innerGlow",
      "LABEL": "Inner Glow",
      "TYPE": "color",
      "DEFAULT": [1.0, 0.98, 0.92, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "outerAura",
      "LABEL": "Outer Aura",
      "TYPE": "color",
      "DEFAULT": [0.87, 0.24, 0.78, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "paletteShift",
      "LABEL": "Palette Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 10,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "lineCount",
      "LABEL": "Field Lines",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 16,
      "DEFAULT": 11,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "coreSize",
      "LABEL": "Core Size",
      "TYPE": "float",
      "MIN": 0.12,
      "MAX": 0.55,
      "DEFAULT": 0.27,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "grainAmount",
      "LABEL": "Film Grain",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.65,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "precessSpeed",
      "LABEL": "Precession Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

float hash11(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

// hue rotation about the luminance axis
vec3 hueShift(vec3 col, float a) {
    const vec3 k = vec3(0.57735);
    float c = cos(a), s = sin(a);
    return col * c + cross(k, col) * s + k * dot(k, col) * (1.0 - c);
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    float asp = RENDERSIZE.x / RENDERSIZE.y;
    vec2 p = (uv - 0.5) * vec2(asp, 1.0);

    // ── audio conditioning ──
    float aR     = clamp(audioReact, 0.0, 1.0);
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float energyP = knee(audioEnergy, 0.05, 0.9);
    float hueA = paletteShift * 0.6283;

    // ── palette (all hue-shiftable) ──
    vec3 cWhite = hueShift(innerGlow.rgb, hueA);
    vec3 cGold  = hueShift(vec3(0.97, 0.76, 0.16), hueA);
    vec3 cPink  = hueShift(vec3(1.00, 0.42, 0.58), hueA);
    vec3 cMag   = hueShift(outerAura.rgb, hueA);
    vec3 cMauve = hueShift(vec3(0.66, 0.44, 0.70), hueA);   // faded lavender-mauve
    vec3 cFrame = vec3(0.575, 0.550, 0.540);                 // muted warm grey border

    // ── aura core, breathing ──
    vec2 c = vec2(0.0, -0.015);
    float silentBreath = 1.0 + 0.045 * sin(TIME * 0.42) + 0.02 * sin(TIME * 0.173 + 1.7);
    float coreR = coreSize * (silentBreath + 0.10 * aR * energyP);
    float auraSwell = 1.0 + 0.10 * aR * bassP;
    float r = length(p - c) / auraSwell;

    // layered radial gradient: magenta -> pink -> gold -> white going inward
    float wMag  = exp(-pow(r / (coreR * 2.45), 2.0));
    float wPink = exp(-pow(r / (coreR * 1.60), 2.0));
    float wGold = exp(-pow(r / (coreR * 1.02), 2.0));
    float wCore = exp(-pow(r / (coreR * 0.55), 2.0));

    // background inside the frame: mauve glow melting to grey toward the edge
    vec3 col = mix(cFrame * 1.02, cMauve, 0.55 * exp(-pow(r / (coreR * 3.6), 2.0)) + 0.22);
    col = mix(col, cMag,  clamp(wMag,  0.0, 1.0));
    col = mix(col, cPink, clamp(wPink, 0.0, 1.0));
    col = mix(col, cGold, clamp(wGold, 0.0, 1.0));
    col = mix(col, cWhite, clamp(wCore, 0.0, 1.0));

    // ── grey-mauve frame border ──
    float m = 0.058;
    vec2 b = min(uv, 1.0 - uv);                 // distance to nearest edge in uv
    float inFrame = smoothstep(m - 0.004, m + 0.004, min(b.x, b.y));
    // aura bleeds faintly onto the border
    vec3 borderCol = cFrame + (cMag - cFrame) * 0.16 * exp(-pow(r / (coreR * 5.0), 2.0));
    col = mix(borderCol, col, inFrame);

    // hairline white frame line
    float dEdge = abs(min(b.x, b.y) - m);
    float wpx = 1.25 / RENDERSIZE.y;
    float frameLine = smoothstep(wpx * 1.4, wpx * 0.3, dEdge);
    col = mix(col, vec3(0.97, 0.965, 0.95), frameLine * 0.55);

    // ── orbital cage of field-line ellipses ──
    float L = floor(lineCount + 0.5);
    float lineA = 0.0;
    float glow = 0.0;
    float glintClock = TIME * 0.9;
    for (int i = 0; i < 16; i++) {
        float fi = float(i);
        if (fi >= L) break;
        float h1 = hash11(fi * 13.71 + 3.0);
        float h2 = hash11(fi * 7.13 + 11.0);
        // longitude circle around the vertical axis, precessing slowly
        float az = fi / L * 3.14159265 + TIME * (0.05 + 0.03 * h2) * precessSpeed + h1 * 6.28;
        float Rl = coreR * (1.30 + 0.85 * h2) * (1.0 + 0.05 * aR * bassP);
        // most lines are longitude circles of the cage; a few are wide flat
        // latitude ellipses, like the dashed equators of a dipole diagram
        bool latitude = (h2 < 0.10);
        vec2 ax = latitude
            ? vec2(Rl * 1.22, max(abs(cos(az * 0.7)), 0.12) * Rl * 0.42)
            : vec2(max(abs(cos(az)), 0.055) * Rl, Rl);
        vec2 q = (p - c - vec2(0.0, 0.02 * sin(TIME * 0.2 + fi))) / ax;
        float lq = length(q);
        float w = max(fwidth(lq), 1e-4);
        float e = abs(lq - 1.0);
        float al = smoothstep(w * 1.5, w * 0.35, e);

        // some lines dashed
        if (h1 > 0.62 || latitude) {
            float ang = atan(q.y, q.x);
            float dsh = fract(ang * 5.7296 + TIME * 0.03);
            al *= smoothstep(0.22, 0.34, dsh) * (1.0 - smoothstep(0.78, 0.92, dsh));
        }
        // vertical-depth fade: line dims where it passes "behind" the core
        al *= 0.55 + 0.45 * smoothstep(-0.2, 0.6, sin(az));

        // highs glint one line at a time (triangular sweep, smooth)
        float ph = mod(glintClock - fi * 1.618, L);
        float tri = max(0.0, 1.0 - ph);
        float glint = 2.6 * aR * highP * tri * tri;
        lineA = max(lineA, al * (0.72 + glint * 0.28));
        glow += al * glint * 0.10;
        // soft halo for glinting line
        glow += exp(-e * e / (w * w * 90.0)) * glint * 0.05;
    }
    vec3 lineInk = vec3(1.0, 0.995, 0.975);
    col = mix(col, lineInk, clamp(lineA, 0.0, 1.0) * 0.9 * inFrame);
    col += lineInk * clamp(glow, 0.0, 1.0) * inFrame;

    // ── heavy fine film grain ──
    float g1 = hash21(uv * RENDERSIZE.xy);
    float g2 = hash21(uv * RENDERSIZE.xy * 0.5 + 17.0);
    float g3 = hash21(uv * RENDERSIZE.xy * 0.25 + 41.0);
    col += (g1 - 0.5) * 0.115 * grainAmount;
    col += (g2 - 0.5) * 0.055 * grainAmount;
    col += (g3 - 0.5) * 0.030 * grainAmount;

    // brightness law (can dip below 1 so lifts stay visible)
    float lift = mix(1.0, 0.80 + 0.34 * knee(audioLevel, 0.02, 0.8), aR * 0.55);
    col *= brightness * lift;

    gl_FragColor = vec4(max(col, 0.0), 1.0);
}
