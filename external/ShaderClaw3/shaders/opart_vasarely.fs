/*{
  "CATEGORIES": [
    "Generator",
    "Op Art",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Op Art after Victor Vasarely & Bridget Riley — eight canonical modes producing GENUINE optical vibration. (0) Vega: square grid radially bulged; (1) Tridim: three-axis cube illusion; (2) Riley Wave: sinusoidal frequency-warp bands; (3) Zebra: serpentine ribbons; (4) Zigzag: triangle-wave tension bands; (5) Checker: warped checkerboard moiré; (6) Diamond: concentric diamond rings; (7) Stripe Wobble: page-bowing vertical stripes. Audio drives bulge depth, wave frequency, angular shift, and accent flash. Crisp fwidth-based AA. Color modes: Mono, Mono+Accent, Holographic, Custom. HDR-ready.",
  "INPUTS": [
    {
      "NAME": "contrast",
      "LABEL": "Contrast",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1
    },
    {
      "NAME": "mode",
      "LABEL": "Mode",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3,
        4,
        5,
        6,
        7
      ],
      "LABELS": [
        "Vega",
        "Tridim",
        "Riley Wave",
        "Zebra",
        "Zigzag",
        "Checker",
        "Diamond",
        "Stripe Wobble"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "gridDensity",
      "LABEL": "Grid Density",
      "TYPE": "float",
      "MIN": 8,
      "MAX": 48,
      "DEFAULT": 22,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "bulgeAmount",
      "LABEL": "Bulge Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.65,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "bulgeRadius",
      "LABEL": "Bulge Radius",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 1.4,
      "DEFAULT": 0.85,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "waveFreq",
      "LABEL": "Wave Frequency",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 12,
      "DEFAULT": 4.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "waveAmp",
      "LABEL": "Wave Amplitude",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "rileyFreq",
      "LABEL": "Riley Freq",
      "TYPE": "float",
      "MIN": 10,
      "MAX": 160,
      "DEFAULT": 60,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "warpAmp",
      "LABEL": "Warp Amplitude",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0.12,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "xFreq",
      "LABEL": "X Frequency",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 12,
      "DEFAULT": 3,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "accentEvery",
      "LABEL": "Accent Every Nth",
      "TYPE": "float",
      "MIN": 2,
      "MAX": 20,
      "DEFAULT": 7,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "rotate",
      "LABEL": "Rotation",
      "TYPE": "float",
      "MIN": -3.14159,
      "MAX": 3.14159,
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "flowSpeed",
      "LABEL": "Flow Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.6,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "accentColor",
      "LABEL": "Accent Color",
      "TYPE": "color",
      "DEFAULT": [
        0.95,
        0.15,
        0.25,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "vpColor",
      "LABEL": "VP Color (Tridim only)",
      "TYPE": "bool",
      "DEFAULT": false,
      "GROUP": "Color"
    },
    {
      "NAME": "colorMode",
      "LABEL": "Color Mode",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Mono",
        "Mono+Accent",
        "Holographic",
        "Custom"
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "colorA",
      "LABEL": "Color A (Custom)",
      "TYPE": "color",
      "DEFAULT": [
        0.05,
        0.18,
        0.85,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "colorB",
      "LABEL": "Color B (Custom)",
      "TYPE": "color",
      "DEFAULT": [
        0.98,
        0.78,
        0.05,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "LABEL": "Hue Shift",
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "LABEL": "Color Boost",
      "GROUP": "Color"
    },
    {
      "NAME": "depthAmount",
      "LABEL": "3D Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "LABEL": "Background",
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ═══════════════════════════════════════════════════════════════════════════
//  VASARELY / RILEY  —  Op Art Extended
//
//  8 modes — all crisp, all audio-reactive, all boldly kinetic.
//  Modes 0-3: Vasarely originals (Vega, Tridim, Riley Wave, Zebra)
//  Modes 4-7: Riley extensions (Zigzag, Checker, Diamond, Stripe Wobble)
//  Color modes: Mono / Mono+Accent / Holographic / Custom
// ═══════════════════════════════════════════════════════════════════════════

// ── Helpers ─────────────────────────────────────────────────────────────
float aaStep(float v) {
    float w = fwidth(v) * 0.75 + 1e-5;
    return smoothstep(0.5 - w, 0.5 + w, v);
}

float aaBox(vec2 p, float hw) {
    vec2 d = abs(p) - vec2(hw);
    float e = max(d.x, d.y);
    float w = fwidth(e) * 0.75 + 1e-5;
    return 1.0 - smoothstep(-w, w, e);
}

mat2 rot2(float a) {
    float c = cos(a), s = sin(a);
    return mat2(c, -s, s, c);
}

vec3 hsv2rgb(vec3 c) {
    vec3 p = abs(fract(c.xxx + vec3(0.0, 2.0/3.0, 1.0/3.0)) * 6.0 - 3.0);
    return c.z * mix(vec3(1.0), clamp(p - 1.0, 0.0, 1.0), c.y);
}

// Triangle wave in [-1,1], period 2*PI
float triWave(float x) {
    float p = x * (1.0 / 6.2831853);
    return abs(fract(p) * 4.0 - 2.0) - 1.0;
}

// Curl-noise-ish turbulence — gives the "swimming" feel
float audioCurl(vec2 uv, float t) {
    return sin(uv.x * 5.0 + t) * cos(uv.y * 7.0 - t) * 0.5;
}

// ── Mode 0: Vega ────────────────────────────────────────────────────────
vec3 modeVega(vec2 p, float t, float bulge, float radius, float dens, float depth) {
    float r = length(p);
    float k = clamp(1.0 - r / max(radius, 1e-3), 0.0, 1.0);
    float warp = 1.0 - bulge * k * k;
    p *= warp;
    if (depth > 1e-4) {
        float tilt = 0.55 * depth;
        float sw = sin(t * 0.2) * 0.35 * depth;
        p.y *= 1.0 + tilt * (p.y * 0.5 + 0.5);
        p.x *= 1.0 + sw * p.y;
    }
    vec2 g = p * dens;
    vec2 cell = floor(g);
    vec2 f = fract(g) - 0.5;
    float parity = mod(cell.x + cell.y, 2.0);
    float sizeMod = mix(0.18, 0.46, k);
    float box = aaBox(f, sizeMod);
    float val = mix(parity, 1.0 - parity, box);
    float cellHash = fract(sin(cell.x * 12.9898 + cell.y * 78.233) * 43758.5453);
    return vec3(val, k, cellHash);
}

// ── Mode 1: Tridim ──────────────────────────────────────────────────────
vec3 modeTridim(vec2 uv, float t, float dens, bool vp, float depth) {
    vec2 puv = uv;
    if (depth > 1e-4) {
        float k2 = 0.45 * depth;
        puv.y *= 1.0 + k2 * (puv.y * 0.5 + 0.5);
        puv *= 1.0 - 0.15 * depth;
    }
    vec2 p = puv * dens;
    vec2 bv = vec2(0.5, 0.8660254);
    float v = p.y / bv.y;
    float u = p.x - v * bv.x;
    float ru = floor(u + 0.5);
    float rv = floor(v + 0.5);
    vec2 cellPos = ru * vec2(1.0, 0.0) + rv * bv;
    vec2 q = p - cellPos;
    float ang = atan(q.y, q.x);
    int face;
    if (ang > 1.0472 && ang <= 2.0944) face = 0;
    else if (ang > -1.0472 && ang <= 1.0472) face = 1;
    else face = 2;
    float flip = step(0.0, sin(t * 0.18));
    float topV   = mix(1.0, 0.0, flip);
    float rightV = 0.5;
    float leftV  = mix(0.0, 1.0, flip);
    float val = (face == 0) ? topV : (face == 1) ? rightV : leftV;
    float edge = min(abs(q.x), min(abs(q.y), abs(q.x * 0.5 + q.y * 0.866)));
    edge = min(edge, abs(q.x * 0.5 - q.y * 0.866));
    float lw = fwidth(edge) * 1.2 + 0.02;
    float line = clamp(1.0 - smoothstep(0.0, lw, edge), 0.0, 1.0);
    vec3 col = vec3(val);
    if (vp) {
        vec3 cobalt  = vec3(0.05, 0.18, 0.85);
        vec3 cadmium = vec3(0.98, 0.78, 0.05);
        vec3 black   = vec3(0.0);
        vec3 white   = vec3(1.0);
        col = (face == 0) ? (flip > 0.5 ? black  : white)
            : (face == 1) ? cobalt
            :               (flip > 0.5 ? cadmium : black);
    }
    col *= 1.0 - line * 0.85;
    return col;
}

// ── Mode 2: Riley Wave ──────────────────────────────────────────────────
vec3 modeRiley(vec2 uv, float t, float freq, float amp, float depth) {
    float x = uv.x;
    float y = uv.y;
    if (depth > 1e-4) {
        float dy = (y - 0.5);
        y = 0.5 + dy * (1.0 + 0.55 * depth * dy);
        x = 0.5 + (x - 0.5) * (1.0 - 0.25 * depth * (y - 0.5));
    }
    float phi = 2.0 * 3.14159265 * freq * x
              + amp * sin(2.0 * 3.14159265 * (1.5 * x + 0.25 * y) + t * 0.7)
              + 0.4 * sin(t * 0.35 + y * 6.0);
    float v = 0.5 + 0.5 * sin(phi);
    return vec3(aaStep(v), fract(phi / 6.2831853), 0.0);
}

// ── Mode 3: Zebra ───────────────────────────────────────────────────────
vec3 modeZebra(vec2 uv, float t, float freq, float amp, float depth) {
    vec2 puv = uv;
    if (depth > 1e-4) {
        float dx = puv.x - 0.5;
        puv.x = 0.5 + dx * (1.0 - 0.4 * depth * dx * dx * 4.0);
        puv.y += 0.08 * depth * sin(puv.x * 3.14159);
    }
    float yWarp = puv.y + amp * 0.35 * sin(puv.x * 2.0 + t * 0.4)
                        + amp * 0.18 * sin(puv.x * 5.5 - t * 0.25);
    float xWarp = puv.x + amp * 0.25 * sin(puv.y * 2.7 - t * 0.3);
    float phi = (xWarp * freq * 1.3 + yWarp * freq * 0.4) * 3.14159265;
    float v = 0.5 + 0.5 * sin(phi);
    return vec3(aaStep(v), fract(phi / 6.2831853), 0.0);
}

// ── Mode 4: Zigzag ──────────────────────────────────────────────────────
// Returns vec4: rgb-packed base + phase in .a equivalent via xyz packing.
// We return vec3(bwVal, phase/6.28, accentFlag) like other modes.
vec3 modeZigzag(vec2 uv, float t, float rFreq, float wAmp, float xFr,
                float flow, float audio, float accentEv) {
    float warp = sin(uv.x * xFr + t) * wAmp * (1.0 + audioBass * audio);
    warp += audioCurl(uv, t) * audioMid * audio * 0.08;
    float y = uv.y + warp;
    float phase = y * rFreq;
    float field = triWave(phase);
    float aaS = fwidth(phase) * 1.25 + 1e-4;
    float bw = smoothstep(-aaS, aaS, field);
    float idx = floor(phase / 3.14159);
    float accent = step(0.0, 0.5 - mod(idx, max(2.0, accentEv)));
    return vec3(bw, phase / 6.2831853, accent);
}

// ── Mode 5: Checker ─────────────────────────────────────────────────────
vec3 modeChecker(vec2 uv, float t, float rFreq, float wAmp, float xFr,
                 float flow, float audio, float accentEv) {
    float warp = sin(uv.x * xFr + t) * wAmp * (1.0 + audioBass * audio);
    vec2 cuv = uv;
    cuv.x += warp * 0.5;
    cuv.y += sin(uv.x * xFr * 0.7 + t) * wAmp * 0.5;
    float tiles = max(2.0, rFreq * 0.18);
    vec2 g = cuv * vec2(tiles * (RENDERSIZE.x / max(RENDERSIZE.y, 1.0)), tiles);
    float parity = mod(floor(g.x) + floor(g.y), 2.0) * 2.0 - 1.0;
    float aaS = fwidth(parity) * 1.25 + 1e-4;
    float bw = smoothstep(-aaS, aaS, parity);
    float phase = (floor(g.x) + floor(g.y)) * 3.14159;
    float idx = floor(phase / 3.14159);
    float accent = step(0.0, 0.5 - mod(idx, max(2.0, accentEv)));
    return vec3(bw, phase / 6.2831853, accent);
}

// ── Mode 6: Diamond ─────────────────────────────────────────────────────
vec3 modeDiamond(vec2 uv, float t, float rFreq, float wAmp, float xFr,
                 float flow, float audio, float accentEv) {
    float warp = sin(uv.x * xFr + t) * wAmp * (1.0 + audioBass * audio);
    warp += audioCurl(uv, t) * audioMid * audio * 0.08;
    vec2 d = uv - 0.5;
    d.x *= RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    d += vec2(warp, warp * 0.6);
    float r = abs(d.x) + abs(d.y);
    float phase = r * rFreq;
    float field = sin(phase);
    float aaS = fwidth(phase) * 1.25 + 1e-4;
    float bw = smoothstep(-aaS, aaS, field);
    float idx = floor(phase / 3.14159);
    float accent = step(0.0, 0.5 - mod(idx, max(2.0, accentEv)));
    return vec3(bw, phase / 6.2831853, accent);
}

// ── Mode 7: Stripe Wobble ───────────────────────────────────────────────
vec3 modeStripeWobble(vec2 uv, float t, float rFreq, float wAmp, float xFr,
                      float flow, float audio, float accentEv) {
    float wob = sin(uv.y * xFr * 2.0 + t * 1.3) * wAmp * (1.0 + audioBass * audio);
    wob += audioCurl(uv.yx, t) * audioMid * audio * 0.08;
    float x = uv.x + wob;
    x *= RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    float phase = x * rFreq;
    float field = sin(phase);
    float aaS = fwidth(phase) * 1.25 + 1e-4;
    float bw = smoothstep(-aaS, aaS, field);
    float idx = floor(phase / 3.14159);
    float accent = step(0.0, 0.5 - mod(idx, max(2.0, accentEv)));
    return vec3(bw, phase / 6.2831853, accent);
}

// ── Colorize ─────────────────────────────────────────────────────────────
// base.r = mono value (0..1)
// base.g = secondary cue (phase fraction)
// base.b = tertiary cue (accent flag or cell hash)
vec3 colorize(vec3 base, vec2 pos, float t, int cmode,
              vec3 cA, vec3 cB, vec3 accentRGB, bool hasAccent) {
    float mono = base.r;
    if (cmode == 0) {
        if (hasAccent && base.b > 0.5 && mono < 0.5) return accentRGB;
        return vec3(mono);
    } else if (cmode == 1) {
        if (hasAccent && base.b > 0.5 && mono < 0.5) return accentRGB;
        vec3 hi  = cA;
        vec3 lo  = vec3(0.0);
        vec3 mid = mix(lo, hi, 0.35);
        float midW = 1.0 - smoothstep(0.0, 0.18, abs(mono - 0.5));
        vec3 c = mix(lo, hi, mono);
        return mix(c, mid, midW * 0.65);
    } else if (cmode == 2) {
        float hue = fract(pos.x * 0.55 + pos.y * 0.30
                        + base.b * 0.40 + base.g * 0.25
                        + t * 0.04);
        float shimmer = 0.06 * sin(t * 1.3 + pos.y * 8.0);
        vec3 rainbow = hsv2rgb(vec3(hue, 0.85, 1.0 + shimmer));
        return rainbow * (0.18 + 0.82 * mono);
    } else {
        return mix(cA, cB, mono);
    }
}

// ── Main ─────────────────────────────────────────────────────────────────
void main() {
    vec2 uv  = isf_FragNormCoord.xy;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);

    // Centred coords with aspect compensation.
    vec2 p = uv - 0.5;
    p.x *= aspect;

    float flow  = clamp(flowSpeed, 0.0, 2.0);
    float t     = TIME * (0.6 + 0.7 * flow);
    float audio = clamp(audioReact, 0.0, 2.0);
    float depth = clamp(depthAmount, 0.0, 1.0);

    // Rotation: manual + treble nudge + slow drift.
    float rot = rotate
              + 0.05 * sin(t * 0.13)
              + 0.08 * flow * sin(t * 0.07)
              + 0.007 * audioHigh * audio;   // was 0.04, then 0.015 — rotating a dense grid flips many edge pixels per frame
    p = rot2(rot) * p;

    // Slow spatial drift.
    vec2 driftedUV = uv;
    driftedUV.x += 0.015 * flow * sin(t * 0.31);
    driftedUV.y += 0.012 * flow * cos(t * 0.27);
    p += vec2(0.012 * flow * sin(t * 0.23),
              0.010 * flow * cos(t * 0.19));

    // Audio-modulated parameters.
    float bulge = bulgeAmount * (0.85 + 0.25 * sin(t * 0.5))
                * (1.0 + 0.18 * flow * sin(t * 0.41))
                * (1.0 + 0.06 * audioBass * audio);   // was 0.45, then 0.12 — bulge rescales the whole grid, keep gentle
    float wFreq = waveFreq * (1.0 + 0.18 * sin(t * 0.4))
                * (1.0 + 0.12 * flow * sin(t * 0.29))
                * (1.0 + 0.35 * audioMid * audio);
    float wAmp2 = waveAmp * (1.0 + 0.12 * flow * sin(t * 0.33))
                           * (1.0 + 0.25 * audioMid * audio);
    float rFreqMod = rileyFreq * (1.0 + audioHigh * audio * 0.15);
    float warpMod  = warpAmp   * (1.0 + audioBass * audio * 0.5);

    int m     = int(mode     + 0.5);
    int cmode = int(colorMode + 0.5);

    // Whether this mode supports accent stripes.
    bool hasAccent = (m >= 4);

    vec3 base;
    if      (m == 0) base = modeVega       (p, t, bulge, bulgeRadius, gridDensity, depth);
    else if (m == 1) base = modeTridim     (p, t, gridDensity * 0.45, vpColor, depth);
    else if (m == 2) base = modeRiley      (driftedUV, t, wFreq, wAmp2, depth);
    else if (m == 3) base = modeZebra      (driftedUV, t, wFreq * 0.6, wAmp2, depth);
    else if (m == 4) base = modeZigzag     (driftedUV, t, rFreqMod, warpMod, xFreq, flow, audio, accentEvery);
    else if (m == 5) base = modeChecker    (driftedUV, t, rFreqMod, warpMod, xFreq, flow, audio, accentEvery);
    else if (m == 6) base = modeDiamond    (driftedUV, t, rFreqMod, warpMod, xFreq, flow, audio, accentEvery);
    else             base = modeStripeWobble(driftedUV, t, rFreqMod, warpMod, xFreq, flow, audio, accentEvery);

    vec3 col;
    if (m == 1 && vpColor) {
        col = base;
    } else {
        col = colorize(base, uv, t, cmode, colorA.rgb, colorB.rgb, accentColor.rgb, hasAccent);
    }

    // Contrast pull (mainly for modes 4-7, benign elsewhere).
    float c = clamp(contrast, 0.0, 1.0);
    col = mix(vec3(0.5), col, c);

    // HDR crest boost on white peaks only.
    float crest = smoothstep(0.85, 0.995, base.r) * (1.0 - step(base.r, 0.01));
    col += vec3(crest) * 0.28 * c;

    // Audio follower — linear whole-frame gain (replaces the full-field
    // invert flash, which strobed the frame-step metric). R3: trimmed —
    // at meanLuma 0.54 the 0.22 bass gain plus the geometric couplings
    // stacked to 0.14 p95 single-frame steps on EDM.
    col *= 1.0 + (0.14 * audioBass + 0.09 * audioMid) * min(audio, 1.0);

    // Periodic frequency-doubling judder (~every 16 s, 0.5 s duration).
    {
        float ph2  = fract(TIME / 16.0);
        float judder = smoothstep(0.0, 0.04, ph2) * smoothstep(0.18, 0.08, ph2);
        float doubled = step(0.5, fract(gl_FragCoord.y / max(RENDERSIZE.y, 1.0) * 80.0)) * 2.0 - 1.0;
        col = mix(col, vec3(0.5 + 0.5 * doubled), judder * 0.4 * c);
    }

    // Final clamp + micro HDR overshoot on whites.
    col = clamp(col, 0.0, 1.2);
    col += step(0.95, max(max(col.r, col.g), col.b)) * 0.06;

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    vec3 uc = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hueA = hueShift * 6.2831853;
        float hueC = cos(hueA), hueS = sin(hueA);
        mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                  + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                  + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hueM * uc, 0.0, 1.0);
    }
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

    gl_FragColor = vec4(uc, 1.0);
}