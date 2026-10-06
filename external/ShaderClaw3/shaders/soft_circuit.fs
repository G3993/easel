/*{
  "DESCRIPTION": "Soft Circuit — an interlocking maze of rounded pipe tubes tiling the whole frame like a puzzle, each outline a soft airbrush-glow stroke in red, blue, green, yellow and pink neon-pastel on warm paper white. The maze continuously reroutes: quarter-turn truchet tiles smoothly flip their connections in eased waves that spread from wandering epicenters, so paths re-wire while the sheet stays a complete circuit. Beats accelerate the flip waves, bass thickens the stroke glow, mids advance a slow traveling hue cycle along the paths, highs brighten the stroke cores. Idle: a beautiful maze calmly re-routing forever.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "strokeA",
      "LABEL": "Stroke Warm Anchor",
      "TYPE": "color",
      "DEFAULT": [0.92, 0.30, 0.26, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "strokeB",
      "LABEL": "Stroke Cool Anchor",
      "TYPE": "color",
      "DEFAULT": [0.22, 0.44, 0.85, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "paperTint",
      "LABEL": "Paper",
      "TYPE": "color",
      "DEFAULT": [0.945, 0.925, 0.885, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "paletteShift",
      "LABEL": "Palette Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "tileScale",
      "LABEL": "Tile Scale",
      "TYPE": "float",
      "MIN": 3,
      "MAX": 8,
      "DEFAULT": 5.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "strokeWidth",
      "LABEL": "Stroke Width",
      "TYPE": "float",
      "MIN": 0.55,
      "MAX": 1.7,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "glowSoft",
      "LABEL": "Airbrush Softness",
      "TYPE": "float",
      "MIN": 0.6,
      "MAX": 1.8,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "rerouteSpeed",
      "LABEL": "Reroute Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2.5,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.35,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// SOFT CIRCUIT — rounded truchet pipe maze on warm paper, all analytic.
// Rerouting = a hashed schedule of flip WAVES: wave w launches at clock C = w
// from epicenter E(w) = hash(w); a tile begins its eased quarter-turn when the
// wavefront (distance-staggered, hash-jittered) reaches it. Waves older than
// the 4-wave window are complete by construction, so total rotation is exact
// and nothing needs persistence. The wave clock advances with TIME (idle
// floor: calm perpetual rerouting) plus audioTime, so beats and loud passages
// genuinely flip more tiles per second. Bass thickens the airbrush glow
// (linear band, display side), mids run the traveling hue cycle via
// audioMidTime, highs whiten the stroke cores.

#define HPI 1.5707963

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

vec3 hueRot(vec3 c, float a) {
    const vec3 W = vec3(0.299, 0.587, 0.114);
    float ca = cos(a), sa = sin(a);
    vec3 g = vec3(dot(c, W));
    vec3 d = c - g;
    vec3 cr = cross(vec3(0.57735), c);
    return max(g + d * ca + cr * sa, 0.0);
}

// keep derived hues clean: renormalize luma and re-saturate after rotation
vec3 cleanHue(vec3 c, float a) {
    vec3 r = hueRot(c, a);
    const vec3 W = vec3(0.299, 0.587, 0.114);
    float l0 = dot(c, W), l1 = max(dot(r, W), 1e-4);
    r *= l0 / l1;
    r = mix(vec3(dot(r, W)), r, 1.35);
    return clamp(r, 0.0, 1.0);
}

// pin each anchor to a clean luminance so no derived hue goes muddy
vec3 toLuma(vec3 c, float L) {
    const vec3 W = vec3(0.299, 0.587, 0.114);
    return clamp(c * (L / max(dot(c, W), 1e-4)), 0.0, 1.0);
}

// continuous 5-anchor palette: red, yellow, green, blue, pink (from anchors)
vec3 pal5(float t, vec3 A, vec3 B) {
    vec3 red = toLuma(A, 0.42);
    vec3 yel = toLuma(mix(vec3(0.98, 0.76, 0.12), A, 0.22), 0.68);
    vec3 grn = toLuma(mix(vec3(0.22, 0.66, 0.30), B, 0.22), 0.44);
    vec3 blu = toLuma(B, 0.40);
    vec3 pnk = toLuma(mix(A, vec3(1.0, 0.72, 0.84), 0.60), 0.70);
    float x = fract(t) * 5.0;
    float f = smoothstep(0.40, 0.60, fract(x));
    if (x < 1.0) return mix(red, yel, f);
    if (x < 2.0) return mix(yel, grn, f);
    if (x < 3.0) return mix(grn, blu, f);
    if (x < 4.0) return mix(blu, pnk, f);
    return mix(pnk, red, f);
}

// total eased rotation (radians) of a tile under the flip-wave schedule.
// C = wave clock (one wave per unit); tc = tile center in normalized space.
float tileAngle(float C, vec2 tc, float h, float aspN) {
    float W = floor(C);
    // waves fully behind the 4-wave window are complete: one quarter-turn each
    float ang = max(W - 3.0, 0.0) * HPI;
    for (int k = 0; k < 4; k++) {
        float w = W - float(k);
        if (w < 0.0) continue;
        vec2 e = vec2(hash11(w * 13.17 + 2.3) * aspN, hash11(w * 7.71 + 5.9));
        float dw = length(tc - e) * 1.35 + h * 0.30;   // <= ~2.5
        float cw = C - w - dw;
        ang += HPI * smoothstep(0.0, 0.14, cw);
    }
    return ang;
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    float asp = RENDERSIZE.x / RENDERSIZE.y;
    vec2 p = vec2(uv.x * asp, uv.y);

    // ---- audio conditioning ----
    // (display-side maze sway is added just below, after the bands)
    float amt   = clamp(audioReact, 0.0, 1.0);
    float bassL = clamp(audioBass, 0.0, 1.0);
    float midL  = clamp(audioMid,  0.0, 1.0);
    float highL = clamp(audioHigh, 0.0, 1.0);
    float highP = pow(knee(audioHigh, 0.08, 0.9), 1.2);
    float pulse = clamp(audioBeatPulse, 0.0, 1.0);

    // display-only sway: the whole sheet drifts with the smoothed mid/high
    // envelope (never touches the schedule) — silence leaves it pinned
    p += amt * vec2(0.055 * midL + 0.020 * highL, -0.035 * midL);

    // wave clock: idle reroute floor + energy/beat-accumulated acceleration
    float C = 2.0 + TIME * 0.032 * rerouteSpeed + amt * 0.80 * audioTime;
    // traveling hue cycle: slow idle drift + mid-envelope clock
    float hueClk = TIME * 0.010 + amt * 0.32 * audioMidTime;

    float ps = paletteShift;
    vec3 A = hueRot(strokeA.rgb, ps * 6.2831853);
    vec3 B = hueRot(strokeB.rgb, ps * 6.2831853);

    // ---- warm paper with a faint tooth ----
    float tooth = hash21(floor(p * 340.0) + 3.7);
    vec3 paper = paperTint.rgb * (0.985 + 0.030 * tooth);
    vec2 vd = (p - vec2(0.5 * asp, 0.5)) / vec2(asp, 1.0);
    paper *= 1.0 - 0.07 * smoothstep(0.22, 0.62, dot(vd, vd));

    // ---- truchet field, 3x3 neighborhood so airbrush glow crosses seams ----
    vec2 q = p * tileScale + vec2(7.31, 3.77);
    vec2 baseCell = floor(q);
    float wGlow = 0.058 * strokeWidth * glowSoft
                * (1.0 + amt * 0.55 * bassL);           // bass thickens glow
    float wCore = 0.022 * strokeWidth;
    // keep strokes resolvable at any resolution
    float pxCell = tileScale / RENDERSIZE.y;
    wGlow = max(wGlow, pxCell * 1.8);
    wCore = max(wCore, pxCell * 1.1);

    vec3 colAcc = vec3(0.0);
    float aAcc = 0.0;
    float coreAcc = 0.0;

    for (int oy = -1; oy <= 1; oy++)
    for (int ox = -1; ox <= 1; ox++) {
        vec2 cell = baseCell + vec2(float(ox), float(oy));
        vec2 f = q - cell - 0.5;                     // -0.5..0.5 in that tile
        float h  = hash21(cell * 1.618 + 0.31);
        float h2 = hash21(cell * 2.113 + 7.7);

        // eased quarter-turn from the wave schedule (+ hashed initial pose)
        vec2 tc = (cell + 0.5) / tileScale;          // tile center, p-space
        float ang = tileAngle(C, tc, h, asp) + floor(h * 4.0) * HPI;
        // beat wobble: tiny eased over-rotation that glides back
        ang += amt * 0.05 * pulse * sin(h * 6.28 + TIME * 0.7);
        float ca = cos(ang), sa = sin(ang);
        vec2 fr = vec2(ca * f.x + sa * f.y, -sa * f.x + ca * f.y);

        // pipe distance: rounded quarter-arcs (or a straight cross, 12%)
        float d;
        if (h2 < 0.12) {
            d = min(abs(fr.x), abs(fr.y));
        } else {
            float d1 = abs(length(fr - vec2(-0.5, -0.5)) - 0.5);
            float d2 = abs(length(fr - vec2( 0.5,  0.5)) - 0.5);
            d = min(d1, d2);
        }

        float glow = exp(-(d * d) / (wGlow * wGlow));
        float core = exp(-(d * d) / (wCore * wCore));
        if (glow < 0.004) continue;

        // hue travels along the path direction (diagonal phase) + mid clock
        float tHue = h * 0.9 + hueClk + (cell.x + cell.y) * 0.045;
        vec3 scol = pal5(tHue, A, B);

        colAcc += scol * glow;
        aAcc += glow;
        coreAcc += core;
    }

    // mids gently swell stroke presence (display-side, clip-safe)
    float strokeGain = 1.0 + amt * (0.60 * midL + 0.25 * pulse);
    float alpha = 1.0 - exp(-aAcc * 2.6 * strokeGain);
    vec3 scol = colAcc / max(aAcc, 1e-4);
    // airbrush body: color densest mid-stroke, core lifts toward paper-light
    float coreT = clamp(coreAcc, 0.0, 1.0);
    float coreLift = 0.42 + amt * 0.50 * highP;         // highs whiten cores
    scol = mix(scol, mix(scol, vec3(1.0, 0.99, 0.96), 0.75), coreT * clamp(coreLift, 0.0, 1.0));

    vec3 col = mix(paper, scol, clamp(alpha, 0.0, 1.0) * 0.95);

    // fine paper grain — static seed keeps the idle frame quiet
    float grain = hash21(uv * RENDERSIZE.xy + 5.3);
    col += (grain - 0.5) * 0.014;
    col = col * brightness / (1.0 + 0.75 * max(brightness - 1.0, 0.0) * col);
    gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
}
