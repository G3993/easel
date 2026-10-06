/*{
  "DESCRIPTION": "Milk Light — a bright-field piece: soft milky pearl fog fills the frame while dozens of small out-of-focus colored light points (green, amber, magenta, cyan, violet) drip slow vertical light-trails downward like luminous rain seen through frosted glass. Heavy gaussian softness, pastel color pooling in the corners, fine film grain. Level swells the inner glow, bass slowly breathes the fog density, mids lengthen the drips, highs pop tiny sharp sparkles that immediately blur away. Stays a lit paper lantern in silence.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "lightWarm",
      "LABEL": "Light Warm Anchor",
      "TYPE": "color",
      "DEFAULT": [1.0, 0.68, 0.22, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "lightCool",
      "LABEL": "Light Cool Anchor",
      "TYPE": "color",
      "DEFAULT": [0.20, 0.78, 0.92, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "fogTint",
      "LABEL": "Fog Tint",
      "TYPE": "color",
      "DEFAULT": [0.93, 0.91, 0.94, 1.0],
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
      "NAME": "dripCount",
      "LABEL": "Drip Count",
      "TYPE": "float",
      "MIN": 8,
      "MAX": 30,
      "DEFAULT": 24,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "softness",
      "LABEL": "Softness",
      "TYPE": "float",
      "MIN": 0.55,
      "MAX": 1.8,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dripLength",
      "LABEL": "Drip Length",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 1.6,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "fallSpeed",
      "LABEL": "Fall Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
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

// MILK LIGHT — bright-field bokeh rain. Everything analytic: each light is a
// hashed (column, phase, size, hue) tuple; the falling head + the beaded trail
// above it are rendered from age along the column, no decay buffer anywhere.
// Audio: level -> inner glow (display gain, soft-compressed so the bright
// field never clips), bass -> fog density breath, mids -> drip velocity +
// trail length (audioMidTime content clock), highs -> sharp sparkle pops that
// blur away. Silence = the exact authored lantern (clocks pause, floors keep
// every element lit).

#define NL 30

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
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1, 0)), u.x),
               mix(hash21(i + vec2(0, 1)), hash21(i + vec2(1, 1)), u.x), u.y);
}
float fbm(vec2 p) {
    float v = 0.0, a = 0.55;
    for (int k = 0; k < 3; k++) {
        v += a * vnoise(p);
        p = p * 2.17 + 11.3;
        a *= 0.5;
    }
    return v;
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

// the five lamp hues, derived from the two color anchors
vec3 lampColor(float h, vec3 warm, vec3 cool) {
    vec3 grn  = hueRot(warm, 1.9);   // amber -> green
    vec3 mag  = hueRot(cool, 2.3);   // cyan  -> magenta
    vec3 vio  = hueRot(cool, 2.9);   // cyan  -> violet
    if (h < 0.20) return grn;
    if (h < 0.42) return warm;
    if (h < 0.62) return mag;
    if (h < 0.82) return cool;
    return vio;
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    float asp = RENDERSIZE.x / RENDERSIZE.y;
    vec2 p = vec2(uv.x * asp, uv.y);

    // ---- audio conditioning (soft knees; linear bands for display motion) ----
    float amt   = clamp(audioReact, 0.0, 1.0);
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.5);
    float midP  = pow(knee(audioMid,  0.06, 0.85), 1.2);
    float highP = pow(knee(audioHigh, 0.08, 0.90), 1.2);
    float levL  = clamp(audioLevel, 0.0, 1.0);
    float midL  = clamp(audioMid,   0.0, 1.0);
    // content clock: idle fall + envelope-proportional velocity (mid + level)
    float clk = TIME * fallSpeed + amt * (2.0 * audioMidTime + 2.4 * audioTime);
    // display-only sway: the frosted glass tilts a breath with the mid/high
    // envelope; silence leaves the frame pinned
    p += amt * vec2(0.030 * midL + 0.012 * clamp(audioHigh, 0.0, 1.0),
                    -0.045 * midL);

    float ps = paletteShift * 6.2831853;
    vec3 warm = hueRot(lightWarm.rgb, ps);
    vec3 cool = hueRot(lightCool.rgb, ps);

    // ---- milky fog field ----
    vec3 pearl = mix(fogTint.rgb, vec3(1.0), 0.10);
    float drift = TIME * 0.05 + amt * 1.2 * audioTime;
    float m = fbm(p * 2.1 + vec2(drift * 0.7, -drift * 0.4));
    // bass slowly breathes the fog density (mottling contrast, large-area)
    float fogAmp = 0.088 * (1.0 + amt * 1.5 * bassP);
    vec3 col = pearl * (0.995 - fogAmp + fogAmp * 1.45 * m);

    // pastel color pooling in the corners (soft, airy)
    vec3 mag = hueRot(cool, 2.3);
    vec3 grn = hueRot(warm, 1.9);
    vec2 d0 = p - vec2(0.06 * asp, 0.06);
    col = mix(col, mix(col, mag,  0.42), 0.75 * exp(-dot(d0, d0) / 0.14));
    d0 = p - vec2(0.97 * asp, 0.10);
    col = mix(col, mix(col, warm, 0.34), 0.65 * exp(-dot(d0, d0) / 0.10));
    d0 = p - vec2(1.00 * asp, 0.92);
    col = mix(col, mix(col, cool, 0.30), 0.60 * exp(-dot(d0, d0) / 0.12));
    d0 = p - vec2(0.02 * asp, 0.95);
    col = mix(col, mix(col, grn,  0.26), 0.55 * exp(-dot(d0, d0) / 0.09));
    // soft warm pool low-center (the lantern heart) — level swells it gently
    d0 = p - vec2(0.58 * asp, 0.38);
    col = mix(col, mix(col, vec3(0.995, 0.965, 0.905), 0.50),
              (0.34 + amt * 0.45 * levL) * exp(-dot(d0, d0) / 0.18));

    // gentle vignette (bright field stays bright; just a whisper of shade)
    vec2 vd = (p - vec2(0.5 * asp, 0.5)) / vec2(asp, 1.0);
    col *= 1.0 - 0.10 * smoothstep(0.25, 0.62, dot(vd, vd));

    // ---- the luminous rain: heads + analytic beaded trails ----
    float sMul = softness;
    float trailStretch = 1.0 + amt * 1.5 * midP;   // mids lengthen the drips
    float lampGain = 1.0 + amt * (0.50 * levL + 0.30 * bassP + 0.45 * midL
                                + 0.35 * highP
                                + 0.40 * clamp(audioBeatPulse, 0.0, 1.0));
    for (int i = 0; i < NL; i++) {
        if (float(i) >= dripCount) break;
        float fi = float(i);
        float h1 = hash11(fi * 7.31 + 1.7);
        float h2 = hash11(fi * 13.7 + 4.1);
        float h3 = hash11(fi * 29.3 + 9.3);
        float h4 = hash11(fi * 3.77 + 0.7);

        float lx = (0.05 + 0.90 * h1) * asp
                 + 0.006 * sin(TIME * 0.23 + h2 * 6.28);
        float spd = 0.014 + 0.040 * h2 * h2;
        float yc = fract(h3 + clk * spd);
        float yHead = 1.22 - yc * 1.46;               // falls top -> bottom
        float fade = smoothstep(0.0, 0.05, yc) * smoothstep(1.0, 0.93, yc);

        vec3 cc = lampColor(h4, warm, cool);
        float dx = p.x - lx;
        float dy = p.y - yHead;

        // out-of-focus head: tight-ish core + wide bloom (both gaussian-soft)
        float rH = (0.016 + 0.034 * h2) * sMul;
        float g2 = dx * dx + dy * dy;
        float head = exp(-g2 / (rH * rH));
        float bloom = 0.40 * exp(-g2 / (rH * rH * 10.0));

        // beaded trail above the head (age is distance fallen past this pixel)
        float trail = 0.0;
        if (dy > 0.0) {
            float L = dripLength * (0.15 + 0.35 * h3) * trailStretch;
            float ta = exp(-dy / L);
            float beads = 0.52 + 0.48 * sin(dy * (26.0 + 44.0 * h2) + h1 * 6.28);
            float wS = (0.0020 + 0.0026 * h4) * sMul;
            float wW = wS * 5.0;
            trail = ta * beads * (1.00 * exp(-dx * dx / (wS * wS))
                                + 0.50 * exp(-dx * dx / (wW * wW)));
            trail *= smoothstep(-0.05, 0.10, p.y);   // let it dissolve low
        }

        float w = (0.55 + 0.45 * h2) * fade * lampGain;
        float a = clamp((head * 1.05 + bloom + trail * 0.85) * w, 0.0, 1.0);
        // most lamps are saturated glow; some are deep moody spots darker
        // than the milk (the reference's dark teal / crimson pools)
        float h5 = hash11(fi * 17.9 + 2.3);
        vec3 cSat = clamp(mix(vec3(dot(cc, vec3(0.299, 0.587, 0.114))), cc, 1.30), 0.0, 1.0);
        vec3 lamp = (h5 < 0.28) ? cSat * 0.42
                                : mix(cSat, vec3(1.0), 0.22 * clamp(head * 1.4, 0.0, 1.0));
        col = mix(col, lamp, a * 0.92);
    }

    // ---- high sparkles: sharp pop, then immediately blur away ----
    float cells = 26.0;
    vec2 sp = p * cells;
    vec2 sc = floor(sp);
    float hs = hash21(sc + 17.0);
    if (hs > 0.84) {
        vec2 dotp = vec2(0.2, 0.2) + 0.6 * vec2(hash21(sc + 3.3), hash21(sc + 6.1));
        float tw = fract(TIME * (0.9 + 0.8 * hs) + hs * 9.7);
        float e = pow(1.0 - tw, 4.0);                 // sharp birth, quick decay
        float sig = mix(0.045, 0.30, tw);             // blurs away as it fades
        vec2 dd = fract(sp) - dotp;
        float spark = exp(-dot(dd, dd) / (sig * sig));
        vec3 scol = mix(vec3(1.0), lampColor(hash21(sc + 9.9), warm, cool), 0.35);
        col = mix(col, scol, clamp(spark * e * amt * 2.4 * highP, 0.0, 0.9));
    }

    // ---- level swells the overall inner glow (display gain, soft-knee
    //      compressed so the bright field never clips to flat white) ----
    float gain = 1.0 + amt * (0.30 * levL + 0.22 * midL
                            + 0.38 * clamp(audioBeatPulse, 0.0, 1.0));
    col = col * gain / (1.0 + 0.85 * (gain - 1.0) * col);

    // fine film grain — static seed: the idle frame stays perfectly quiet
    float grain = hash21(uv * RENDERSIZE.xy + 11.7);
    col += (grain - 0.5) * 0.016;

    // brightness with the same clip-safe compression
    col = col * brightness / (1.0 + 0.75 * max(brightness - 1.0, 0.0) * col);
    gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
}
