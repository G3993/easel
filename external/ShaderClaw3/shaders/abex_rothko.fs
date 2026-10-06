/*{
  "CATEGORIES": [
    "Generator",
    "Art Movement",
    "Audio Reactive",
    "VFX",
    "Simulation"
  ],
  "DESCRIPTION": "Rothko color-field painting dissolved by a UV-advection fluid simulation. Luminous stacked bands breathe and melt while fluid dynamics warp the canvas like pigment dissolving in warm, living light.",
  "INPUTS": [
    {
      "NAME": "feather",
      "LABEL": "Edge Feather",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 0.55,
      "DEFAULT": 0.28
    },
    {
      "NAME": "paintTexture",
      "LABEL": "Paint Texture",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.38
    },
    {
      "NAME": "textureScale",
      "LABEL": "Texture Scale",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 16,
      "DEFAULT": 4.5
    },
    {
      "NAME": "grain",
      "LABEL": "Film Grain",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.05,
      "DEFAULT": 0.014
    },
    {
      "NAME": "specAmount",
      "LABEL": "Specular",
      "TYPE": "float",
      "DEFAULT": 1.5,
      "MIN": 0,
      "MAX": 5
    },
    {
      "NAME": "specPow",
      "LABEL": "Spec Power",
      "TYPE": "float",
      "DEFAULT": 36,
      "MIN": 4,
      "MAX": 128
    },
    {
      "NAME": "inputTex",
      "TYPE": "image",
      "LABEL": "Texture"
    },
    {
      "NAME": "texMix",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "LABEL": "Texture Mix"
    },
    {
      "NAME": "innerInset",
      "LABEL": "Rectangle Inset",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.18,
      "DEFAULT": 0.05,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "bandCount",
      "LABEL": "Bands",
      "TYPE": "float",
      "MIN": 2,
      "MAX": 4,
      "DEFAULT": 3,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "splatRadius",
      "LABEL": "Splat Radius",
      "TYPE": "float",
      "DEFAULT": 0.05,
      "MIN": 0.01,
      "MAX": 0.2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "bumpHeight",
      "LABEL": "Surface Depth",
      "TYPE": "float",
      "DEFAULT": 80,
      "MIN": 0,
      "MAX": 300,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "breathSpeed",
      "LABEL": "Breath Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.6,
      "DEFAULT": 0.09,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "meltDepth",
      "LABEL": "Melt / Bleed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.72,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "waveAmount",
      "LABEL": "Edge Waviness",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.04,
      "DEFAULT": 0.01,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "shimmer",
      "LABEL": "Surface Shimmer",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.18,
      "DEFAULT": 0.05,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "shimmerScale",
      "LABEL": "Shimmer Scale",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 8,
      "DEFAULT": 2.2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "fluidSpeed",
      "LABEL": "Fluid Speed",
      "TYPE": "float",
      "DEFAULT": 5,
      "MIN": 0.5,
      "MAX": 20,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "advectSpeed",
      "LABEL": "Advect Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.1,
      "MAX": 5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "returnRate",
      "LABEL": "Return Rate",
      "TYPE": "float",
      "DEFAULT": 0.005,
      "MIN": 0,
      "MAX": 0.05,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "vorticity",
      "LABEL": "Vorticity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "viscosity",
      "LABEL": "Viscosity",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "splatForce",
      "LABEL": "Splat Force",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.1,
      "MAX": 10,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "moveMode",
      "LABEL": "Movement",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3,
        4
      ],
      "LABELS": [
        "None",
        "Freeform",
        "Center",
        "Wave",
        "Vortex"
      ],
      "DEFAULT": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "moveSpeed",
      "LABEL": "Move Speed",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0.05,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "moveSpread",
      "LABEL": "Move Spread",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "moveIntensity",
      "LABEL": "Move Intensity",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "bandDrift",
      "LABEL": "Band Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.12,
      "DEFAULT": 0.025,
      "GROUP": "Motion"
    },
    {
      "NAME": "waveSpeed",
      "LABEL": "Wave Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "textureDrift",
      "LABEL": "Texture Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 8,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "lightSway",
      "LABEL": "Light Sway",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "rothkoWork",
      "LABEL": "Painting",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3,
        4
      ],
      "LABELS": [
        "Orange Red Yellow",
        "No.61 Rust+Blue",
        "White Center",
        "Seagram Maroon",
        "Black on Maroon"
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "topColor",
      "LABEL": "Top Color",
      "TYPE": "color",
      "DEFAULT": [
        0.92,
        0.5,
        0.22,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "midColor",
      "LABEL": "Mid Color",
      "TYPE": "color",
      "DEFAULT": [
        0.85,
        0.2,
        0.14,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "botColor",
      "LABEL": "Bot Color",
      "TYPE": "color",
      "DEFAULT": [
        0.95,
        0.78,
        0.3,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "groundColor",
      "LABEL": "Ground",
      "TYPE": "color",
      "DEFAULT": [
        0.28,
        0.08,
        0.08,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "chrShimmer",
      "LABEL": "Chromatic Edge",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.025,
      "DEFAULT": 0.008,
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
      "NAME": "vignette",
      "LABEL": "Vignette",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.8,
      "DEFAULT": 0.3,
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
      "NAME": "audioInfluence",
      "LABEL": "Audio Influence",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.12,
      "DEFAULT": 0.04,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "fluidAudio",
      "LABEL": "Fluid Audio React",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "velBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "uvBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "rothkoBuf",
      "PERSISTENT": false
    },
    {}
  ]
}*/

// ────────────────────────────────────────────────────────────────────────────
// PASS 0  — velocity field (rotational curl + movement patterns + audio)
// PASS 1  — UV advection buffer
// PASS 2  — Rothko painting render  (into rothkoBuf)
// PASS 3  — composite: sample Rothko at warped UVs + surface lighting
// ────────────────────────────────────────────────────────────────────────────

#define PI2 6.283185307
#define RotNum 5

// ── shared helpers ────────────────────────────────────────────────────────────

float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float vnoise(vec2 p) {
    vec2 ip = floor(p);
    vec2 fp = fract(p);
    fp = fp * fp * (3.0 - 2.0 * fp);
    float a = hash21(ip);
    float b = hash21(ip + vec2(1.0, 0.0));
    float c = hash21(ip + vec2(0.0, 1.0));
    float d = hash21(ip + vec2(1.0, 1.0));
    return mix(mix(a, b, fp.x), mix(c, d, fp.x), fp.y);
}

float fbm(vec2 p) {
    float v = 0.0, a = 0.52;
    for (int i = 0; i < 5; i++) {
        v += a * vnoise(p);
        p  = p * 2.03 + vec2(3.7, 1.9);
        a *= 0.5;
    }
    return v;
}

vec3 triGrad(float t, vec3 cA, vec3 cB, vec3 cC) {
    t = clamp(t, 0.0, 1.0);
    return (t < 0.5) ? mix(cA, cB, t * 2.0) : mix(cB, cC, (t - 0.5) * 2.0);
}

// ── fluid helpers ─────────────────────────────────────────────────────────────

float _ang = PI2 / float(RotNum);

vec2 rot2(vec2 v, float a) {
    float c = cos(a), s = sin(a);
    return vec2(c * v.x - s * v.y, s * v.x + c * v.y);
}

float getRot(vec2 pos, vec2 b, vec2 Res) {
    vec2 p = b;
    float rotSum = 0.0;
    for (int i = 0; i < RotNum; i++) {
        vec2 samp = IMG_PIXEL(velBuf, fract((pos + p) / Res) * Res).xy - vec2(0.5);
        rotSum += dot(samp, p.yx * vec2(1.0, -1.0));
        p = rot2(p, _ang);
    }
    return rotSum / float(RotNum) / dot(b, b);
}

// ── Rothko band mask ──────────────────────────────────────────────────────────

float bandMask(vec2 uv, float yLo, float yHi, float xIn, float fth, float wave) {
    float wx = wave * sin(uv.x * 11.0 + TIME * 0.07 * waveSpeed);
    float wy = wave * cos(uv.x *  7.3 - TIME * 0.05 * waveSpeed);
    float ym = smoothstep(yLo - fth + wx, yLo + fth + wx, uv.y)
             * (1.0 - smoothstep(yHi - fth + wy, yHi + fth + wy, uv.y));
    float xm = smoothstep(xIn, xIn + fth * 0.5, uv.x)
             * (1.0 - smoothstep(1.0 - xIn - fth * 0.5, 1.0 - xIn, uv.x));
    return ym * xm;
}

// ── Rothko painting ───────────────────────────────────────────────────────────

vec4 rothkoPaint(vec2 uv) {
    int rw = int(rothkoWork);

    vec3 cTop = topColor.rgb;
    vec3 cMid = midColor.rgb;
    vec3 cBot = botColor.rgb;
    vec3 cGnd = groundColor.rgb;

    if      (rw == 1) { cTop = vec3(0.52,0.16,0.16); cMid = vec3(0.18,0.20,0.44); cBot = vec3(0.38,0.14,0.20); cGnd = vec3(0.06,0.04,0.12); }
    else if (rw == 2) { cTop = vec3(0.93,0.87,0.79); cMid = vec3(0.84,0.30,0.18); cBot = vec3(0.52,0.16,0.38); cGnd = vec3(0.18,0.09,0.14); }
    else if (rw == 3) { cTop = vec3(0.28,0.04,0.05); cMid = vec3(0.16,0.03,0.04); cBot = vec3(0.08,0.02,0.03); cGnd = vec3(0.05,0.01,0.02); }
    else if (rw == 4) { cTop = vec3(0.04,0.01,0.02); cMid = vec3(0.26,0.04,0.05); cBot = vec3(0.08,0.02,0.03); cGnd = vec3(0.02,0.01,0.01); }

    float bt = TIME * breathSpeed;

    float ph0 = sin(bt * 0.53)              * 0.5 + 0.5;
    float ph1 = sin(bt * 0.41 + 1.8)        * 0.5 + 0.5;
    float ph2 = sin(bt * 0.37 + 3.5)        * 0.5 + 0.5;
    float ph3 = sin(bt * 0.29 + 5.1)        * 0.5 + 0.5;
    float ph4 = sin(bt * 0.61 + 2.4)        * 0.5 + 0.5;

    vec3 bTop = mix(cTop, mix(cMid, cGnd, 0.25), ph0);
    vec3 bMid = mix(cMid, mix(cBot, cTop, ph1),  ph2);
    vec3 bBot = mix(cBot, mix(cTop, cGnd, 0.15), ph3);

    cTop = mix(cTop, bTop, meltDepth);
    cMid = mix(cMid, bMid, meltDepth);
    cBot = mix(cBot, bBot, meltDepth);

    float tide = sin(bt * 0.19) * 0.5 + 0.5;
    cTop = mix(cTop, mix(cTop, cBot, 0.30), tide * meltDepth * 0.5);
    cMid = mix(cMid, mix(cMid, cTop, 0.30), (1.0 - tide) * meltDepth * 0.5);
    cBot = mix(cBot, mix(cBot, cMid, 0.30), ph4 * meltDepth * 0.5);

    vec3 col = mix(cGnd * 0.82, cGnd * 1.08, uv.y);

    float gBleed = fbm(uv * 1.4 + vec2(bt * 0.07, bt * 0.04));
    col = mix(col, mix(cGnd, cMid, 0.35), gBleed * meltDepth * 0.28);

    int N    = int(clamp(bandCount, 2.0, 4.0));
    float xI = clamp(innerInset, 0.0, 0.38);
    float fth = feather * (1.0 + meltDepth * 1.8);
    float wav = waveAmount;
    float drift = sin(bt * 0.17) * bandDrift * meltDepth;
    float cOff  = chrShimmer * (0.6 + 0.4 * sin(TIME * 0.41));

    if (N >= 3) {
        float y1L = 0.60 + drift;       float y1H = 0.92 + drift * 0.5;
        float mR1 = bandMask(uv + vec2(cOff, 0.0), y1L, y1H, xI, fth, wav);
        float mG1 = bandMask(uv,                   y1L, y1H, xI, fth, wav);
        float mB1 = bandMask(uv - vec2(cOff, 0.0), y1L, y1H, xI, fth, wav);

        float y2L = 0.32 - drift;       float y2H = 0.58 - drift * 0.5;
        float mR2 = bandMask(uv + vec2(cOff, 0.0), y2L, y2H, xI, fth, wav);
        float mG2 = bandMask(uv,                   y2L, y2H, xI, fth, wav);
        float mB2 = bandMask(uv - vec2(cOff, 0.0), y2L, y2H, xI, fth, wav);

        float y3L = 0.06 + drift * 0.3; float y3H = 0.28 + drift * 0.3;
        float mR3 = bandMask(uv + vec2(cOff, 0.0), y3L, y3H, xI, fth, wav);
        float mG3 = bandMask(uv,                   y3L, y3H, xI, fth, wav);
        float mB3 = bandMask(uv - vec2(cOff, 0.0), y3L, y3H, xI, fth, wav);

        col.r = mix(col.r, cTop.r, mR1); col.g = mix(col.g, cTop.g, mG1); col.b = mix(col.b, cTop.b, mB1);
        col.r = mix(col.r, cMid.r, mR2); col.g = mix(col.g, cMid.g, mG2); col.b = mix(col.b, cMid.b, mB2);
        col.r = mix(col.r, cBot.r, mR3); col.g = mix(col.g, cBot.g, mG3); col.b = mix(col.b, cBot.b, mB3);
    } else {
        float y1L = 0.53 + drift;       float y1H = 0.92 + drift * 0.3;
        float mR1 = bandMask(uv + vec2(cOff, 0.0), y1L, y1H, xI, fth, wav);
        float mG1 = bandMask(uv,                   y1L, y1H, xI, fth, wav);
        float mB1 = bandMask(uv - vec2(cOff, 0.0), y1L, y1H, xI, fth, wav);

        float y2L = 0.08 - drift;       float y2H = 0.46 - drift * 0.3;
        float mR2 = bandMask(uv + vec2(cOff, 0.0), y2L, y2H, xI, fth, wav);
        float mG2 = bandMask(uv,                   y2L, y2H, xI, fth, wav);
        float mB2 = bandMask(uv - vec2(cOff, 0.0), y2L, y2H, xI, fth, wav);

        col.r = mix(col.r, cTop.r, mR1); col.g = mix(col.g, cTop.g, mG1); col.b = mix(col.b, cTop.b, mB1);
        col.r = mix(col.r, cBot.r, mR2); col.g = mix(col.g, cBot.g, mG2); col.b = mix(col.b, cBot.b, mB2);
    }

    if (paintTexture > 0.001) {
        float tScale = textureScale;
        float tT     = TIME * breathSpeed * 0.12 * textureDrift;
        float tex1 = fbm(uv * tScale           + vec2(tT * 0.23,  tT * 0.17));
        float tex2 = fbm(uv * tScale * 1.8     + vec2(-tT * 0.19, tT * 0.31) + vec2(4.1, 2.3));
        float tex3 = fbm(uv * tScale * 0.45    + vec2(tT * 0.11, -tT * 0.14) + vec2(1.7, 6.1));
        float tex  = tex1 * 0.55 + tex2 * 0.28 + tex3 * 0.17;
        float modulate = 1.0 + (tex - 0.48) * paintTexture * 1.6;
        col = clamp(col * modulate, 0.0, 1.0);
        float tintPhase = sin(bt * 0.31) * 0.5 + 0.5;
        vec3  tint      = mix(cTop * 1.1, cBot * 0.9, tintPhase);
        float lift      = max(0.0, tex - 0.60) * paintTexture * 0.22;
        col = clamp(col + tint * lift, 0.0, 1.0);
    }

    if (shimmer > 0.001) {
        float sc = shimmerScale;
        float n1 = vnoise(uv * sc       + vec2(TIME * breathSpeed * 0.55, TIME * breathSpeed * 0.40));
        float n2 = vnoise(uv * sc * 2.1 - vec2(TIME * breathSpeed * 0.42, TIME * breathSpeed * 0.31));
        float shm = n1 * 0.65 + n2 * 0.35;
        col *= 1.0 + (shm - 0.5) * shimmer * 2.0;
    }

    float centreGlow = exp(-pow((uv.x - 0.5) * 2.8, 2.0))
                     * exp(-pow((uv.y - 0.5) * 1.4, 2.0));
    centreGlow *= 0.18 * (0.7 + 0.3 * sin(bt * 0.23));
    col += col * centreGlow;

    float d   = length((uv - 0.5) * vec2(1.0, 1.15));
    float vig = pow(d * 1.35, 3.0) * vignette;
    col *= 1.0 - vig;

    float g = hash21(uv * RENDERSIZE + vec2(float(FRAMEINDEX) * 0.317, 0.0));
    col += (g - 0.5) * grain;

    // Soft-knee band following (playbook law 6). The old linear col*mod
    // clipped flat against the bright default palette (~0.9 values), so the
    // response never showed; a gamma lift brightens mid-tones clip-free.
    // Punch term makes sparse rock/hiphop/jazz hits read below the old floor.
    float bassP  = pow(smoothstep(0.04, 0.85, audioBass), 1.4);
    float midP   = pow(smoothstep(0.05, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.08, 0.90, audioHigh), 1.2);
    float punchE = audioBeatPulse * audioBeatPulse;
    float audioMod = 1.0 + audioInfluence * (6.0 * bassP + 3.0 * midP
                                           + 1.5 * highP + 3.5 * punchE);
    col = pow(clamp(col, 0.0, 1.0), vec3(1.0 / audioMod));

    return vec4(clamp(col, 0.0, 1.0), 1.0);
}

// ── main ──────────────────────────────────────────────────────────────────────

void main() {
    vec2 Res = RENDERSIZE;
    vec2 pos = gl_FragCoord.xy;
    vec2 uv  = isf_FragNormCoord;
    float aspect = Res.x / Res.y;

    // ═════════════════════════════════════════════════════════════════════════
    // PASS 0 — Velocity field
    // ═════════════════════════════════════════════════════════════════════════
    if (PASSINDEX == 0) {
        vec2 b = cos(float(FRAMEINDEX) * 0.3 - vec2(0.0, 1.57));
        vec2 v = vec2(0.0);
        float bbMax = 0.5 * Res.y;
        bbMax *= bbMax;

        for (int l = 0; l < 20; l++) {
            if (dot(b, b) > bbMax) break;
            vec2 p = b;
            for (int i = 0; i < RotNum; i++) {
                v += p.yx * getRot(pos + p, -rot2(b, _ang * 0.5), Res);
                p = rot2(p, _ang);
            }
            b *= 2.0;
        }

        v *= mix(0.5, 2.0, vorticity / 5.0);

        float speedScale = fluidSpeed * sqrt(Res.x / 600.0);
        vec2 advUV = fract((pos - v * vec2(-1.0, 1.0) * speedScale) / Res);
        vec4 col = IMG_PIXEL(velBuf, advUV * Res);

        col.xy = mix(col.xy, vec2(0.5), viscosity * 0.02);

        float edgeDist = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
        float edgeForce = smoothstep(0.05, 0.0, edgeDist);
        if (edgeForce > 0.0) {
            vec2 edgeNormal = vec2(0.0);
            if (uv.x < 0.05) edgeNormal.x =  1.0;
            if (uv.x > 0.95) edgeNormal.x = -1.0;
            if (uv.y < 0.05) edgeNormal.y =  1.0;
            if (uv.y > 0.95) edgeNormal.y = -1.0;
            edgeNormal = normalize(edgeNormal + 0.001);
            vec2 vel2 = (col.xy - 0.5) * 2.0;
            float into = dot(vel2, -edgeNormal);
            if (into > 0.0) {
                vel2 += edgeNormal * into * 2.0 * edgeForce;
                col.xy = vel2 * 0.5 + 0.5;
            }
        }

        // Mouse splat
        float interacting = max(mouseDown, pinchHold);
        if (interacting > 0.3) {
            vec2 mDiff = uv - mousePos;
            mDiff.x *= aspect;
            float dist2 = dot(mDiff, mDiff);
            float r2 = splatRadius * splatRadius;
            if (dist2 < r2 * 12.0) {
                float falloff = exp(-dist2 / r2);
                vec2 force = mouseDelta * Res * splatForce * 0.0003 * interacting;
                if (dot(force, force) < 0.000001)
                    force = normalize(mDiff + 0.001) * 0.02 * interacting * splatForce;
                force = clamp(force, vec2(-0.3), vec2(0.3));
                col.xy += force * falloff;
                col.xy = clamp(col.xy, 0.0, 1.0);
            }
        }

        // Movement patterns
        float t = TIME * moveSpeed;
        float spread   = mix(0.05, 0.42, moveSpread);
        float intensity = moveIntensity * 0.15;
        float splatR  = splatRadius * 2.5;
        float splatR2 = splatR * splatR;
        float cutoff2 = splatR2 * 12.0;
        int mMode = int(moveMode);

        if (mMode == 1) {
            for (int s = 0; s < 3; s++) {
                float fs = float(s);
                float phase = t * (0.5 + fs * 0.3) + fs * 1.257;
                vec2 splatPos = vec2(
                    0.5 + spread * sin(phase) * cos(phase * 0.7 + fs),
                    0.5 + spread * cos(phase * 0.8) * sin(phase * 0.5 + fs * 1.5)
                );
                vec2 mDiff = uv - splatPos; mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                if (dist2 < cutoff2)
                    col.xy += vec2(cos(phase * 1.3 + fs), sin(phase * 0.9 + fs * 2.0))
                              * intensity * exp(-dist2 / splatR2);
            }
        } else if (mMode == 2) {
            for (int s = 0; s < 3; s++) {
                float fs = float(s);
                float phase = t * (0.8 + fs * 0.25) + fs * 1.257;
                float r = spread * 0.5 * (0.3 + 0.7 * abs(sin(phase * 0.5)));
                float a = phase * 0.7 + fs * 1.257;
                vec2 splatPos = vec2(0.5 + cos(a) * r, 0.5 + sin(a) * r);
                vec2 mDiff = uv - splatPos; mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                if (dist2 < cutoff2) {
                    vec2 dir = normalize(splatPos - 0.5 + 0.001);
                    col.xy += dir * sin(phase * 1.5) * intensity * 1.5 * exp(-dist2 / splatR2);
                }
            }
            vec2 cDiff = uv - 0.5; cDiff.x *= aspect;
            float cf = exp(-dot(cDiff, cDiff) / (spread * spread * 0.3));
            col.xy += vec2(-cDiff.y, cDiff.x) * sin(t * 0.6) * intensity * 0.5 * cf;
        } else if (mMode == 3) {
            for (int s = 0; s < 3; s++) {
                float fs = float(s);
                float wavePhase = t * 0.8 + fs * 0.7;
                vec2 splatPos = vec2(
                    0.5 + spread * sin(wavePhase * 0.6 + fs * 0.8),
                    0.5 + spread * 0.7 * sin(wavePhase * 1.1 + fs * 1.7)
                );
                vec2 mDiff = uv - splatPos; mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                if (dist2 < cutoff2)
                    col.xy += vec2(
                        cos(wavePhase * 0.6 + fs * 0.8) * intensity * 1.2,
                        sin(wavePhase * 2.2 + fs) * intensity * 0.6
                    ) * exp(-dist2 / splatR2);
            }
            col.xy += vec2(
                sin(uv.x * 8.0 + t * 1.5) * sin(uv.y * 6.0 - t * 0.8),
                cos(uv.x * 5.0 - t)
            ) * intensity * 0.1;
        } else if (mMode == 4) {
            for (int s = 0; s < 3; s++) {
                float fs = float(s);
                float vortexPhase = t * (0.6 + fs * 0.2);
                float r = spread * (0.15 + fs * 0.15);
                vec2 center = vec2(
                    0.5 + spread * 0.3 * sin(t * 0.3 + fs),
                    0.5 + spread * 0.3 * cos(t * 0.25 + fs * 1.5)
                );
                vec2 splatPos = center + vec2(cos(vortexPhase), sin(vortexPhase)) * r;
                vec2 mDiff = uv - splatPos; mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                if (dist2 < cutoff2) {
                    vec2 radial = splatPos - center;
                    col.xy += vec2(-radial.y, radial.x) * intensity * 2.0
                              / (r + 0.05) * exp(-dist2 / splatR2);
                }
            }
            vec2 gDiff = uv - 0.5; gDiff.x *= aspect;
            float rotFalloff = exp(-dot(gDiff, gDiff) / (spread * spread));
            float rotSign = (sin(t * 0.4) > 0.0) ? 1.0 : -1.0;
            col.xy += vec2(-gDiff.y, gDiff.x) * rotSign * intensity * 0.3 * rotFalloff;
        }

        // Audio splats — bass creates fluid bursts. Splat position holds for
        // ~10 frames (the old un-floored seed teleported it EVERY frame —
        // pure uncorrelated noise), and the hard >0.2 gate is now a smooth
        // knee so soft hiphop/jazz kicks still inject.
        if (fluidAudio > 0.01) {
            float bassK = smoothstep(0.10, 0.45, audioBass);
            float at = floor(float(FRAMEINDEX) * 0.1);
            vec2 splatPos = vec2(hash21(vec2(at, 1.0)), hash21(vec2(at, 2.0)));
            vec2 mDiff = uv - splatPos; mDiff.x *= aspect;
            float dist2 = dot(mDiff, mDiff);
            float ar = splatRadius * (1.0 + audioBass * 3.0 * fluidAudio);
            if (dist2 < ar * ar * 12.0) {
                float falloff = exp(-dist2 / (ar * ar));
                float splatAngle = hash21(vec2(at, 3.0)) * PI2;
                col.xy += vec2(cos(splatAngle), sin(splatAngle))
                          * bassK * audioBass * 0.9 * splatForce * fluidAudio * falloff;
                col.xy = clamp(col.xy, 0.0, 1.0);
            }
        }
        // audioMid: global swirl boost (smooth knee, no binary gate)
        if (fluidAudio > 0.01) {
            vec2 cd = uv - 0.5; cd.x *= aspect;
            float swAmp = smoothstep(0.08, 0.6, audioMid) * audioMid * fluidAudio * 0.22;
            col.xy += vec2(-cd.y, cd.x) * swAmp * exp(-dot(cd,cd) * 8.0);
            col.xy = clamp(col.xy, 0.0, 1.0);
        }

        // Initialise
        if (FRAMEINDEX < 4) {
            col = vec4(0.5, 0.5, 0.0, 1.0);
            vec2 d = uv - 0.5;
            col.xy = vec2(0.5 - d.y * 0.2, 0.5 + d.x * 0.2);
        }

        gl_FragColor = col;
        return;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // PASS 1 — UV advection
    // ═════════════════════════════════════════════════════════════════════════
    if (PASSINDEX == 1) {
        if (FRAMEINDEX < 4) {
            gl_FragColor = vec4(uv, 0.0, 1.0);
            return;
        }

        vec2 vel = (IMG_PIXEL(velBuf, pos).xy - 0.5) * 2.0;
        vec2 advUV  = fract(uv - vel * advectSpeed * 0.003);
        vec2 stored = IMG_PIXEL(uvBuf, advUV * Res).rg;
        stored = mix(stored, uv, returnRate);
        gl_FragColor = vec4(stored, 0.0, 1.0);
        return;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // PASS 2 — Render Rothko painting into rothkoBuf
    // ═════════════════════════════════════════════════════════════════════════
    if (PASSINDEX == 2) {
        gl_FragColor = rothkoPaint(uv);
        return;
    }

    // ═════════════════════════════════════════════════════════════════════════
    // PASS 3 — Composite: sample Rothko at warped UVs + surface lighting
    // ═════════════════════════════════════════════════════════════════════════
    vec2 warpedUV = IMG_PIXEL(uvBuf, pos).rg;

    // Bass leans into the fluid warp itself — amplify the stored displacement
    // around its rest state (dominant visual element; silence = unchanged).
    float warpBassP = pow(smoothstep(0.04, 0.85, audioBass), 1.4);
    warpedUV = clamp(uv + (warpedUV - uv) * (1.0 + 0.45 * warpBassP * fluidAudio), 0.0, 1.0);

    // Sample the Rothko painting at the fluid-warped UV coordinates
    vec3 col = IMG_NORM_PIXEL(rothkoBuf, warpedUV).rgb;

    // ── Optional texture input — dissolved into the paint by the same fluid warp ──
    if (texMix > 0.001) {
        vec3 texCol = texture2D(inputTex, warpedUV).rgb;
        vec3 softLit = 1.0 - (1.0 - col) * (1.0 - texCol);
        vec3 blended = mix(col * (0.6 + 0.4 * texCol), softLit, 0.5);
        col = mix(col, blended, texMix);
    }

    // ── Surface lighting from UV displacement gradient ─────────────────────
    if (bumpHeight > 0.0) {
        float delta = max(1.0 / Res.x, 1.0 / Res.y);

        vec2 uvL = IMG_PIXEL(uvBuf, pos + vec2(-delta * Res.x, 0.0)).rg;
        vec2 uvR = IMG_PIXEL(uvBuf, pos + vec2( delta * Res.x, 0.0)).rg;
        vec2 uvU = IMG_PIXEL(uvBuf, pos + vec2(0.0,  delta * Res.y)).rg;
        vec2 uvD = IMG_PIXEL(uvBuf, pos + vec2(0.0, -delta * Res.y)).rg;

        float hL = length(uvL - (uv + vec2(-delta, 0.0)));
        float hR = length(uvR - (uv + vec2( delta, 0.0)));
        float hU = length(uvU - (uv + vec2(0.0,  delta)));
        float hD = length(uvD - (uv + vec2(0.0, -delta)));

        vec3 n = normalize(vec3(
            (hR - hL) * bumpHeight,
            (hU - hD) * bumpHeight,
            1.0
        ));

        // Warm raking light — angle evokes museum spotlight
        // Light Sway: raking spotlight swings slowly around the canvas (0 = fixed museum angle)
        float lsw = lightSway * 0.9 * sin(TIME * 0.21);
        float lsc = cos(lsw), lss = sin(lsw);
        vec3 lightDir = normalize(vec3(0.4 * lsc - 0.7 * lss, 0.4 * lss + 0.7 * lsc, 1.0));
        float diff = clamp(dot(n, lightDir), 0.35, 1.0);

        vec2 sc = (gl_FragCoord.xy - Res * 0.5) / Res.x;
        vec3 viewDir = normalize(vec3(sc, -1.0));
        vec3 halfVec = normalize(lightDir - viewDir);
        float specRaw = pow(max(dot(n, halfVec), 0.0), specPow);

        float ridgeMag = length(vec2(hR - hL, hU - hD)) * bumpHeight;
        float aaW = max(fwidth(ridgeMag), 0.0001);
        float ridgeMask = smoothstep(0.0, aaW * 4.0, ridgeMag);
        float spec = specRaw * specAmount * ridgeMask;

        // Warm specular that echoes the painting's palette
        float srcBright = max(max(col.r, col.g), col.b);
        float hdrGate   = smoothstep(0.45, 0.85, srcBright)
                        * smoothstep(0.04, 0.20, ridgeMag)
                        * specRaw;
        vec3 hdrPeak = vec3(1.0, 0.95, 0.82) * hdrGate * 1.5;

        col = col * diff + vec3(spec) + hdrPeak;
    }

    // R2 ambient fix: linear whole-frame follower at the final composite,
    // outside the fluid feedback and NOT scaled by audioInfluence — the
    // round-1 gamma lift was diluted twice (knee'd bands x audioInfluence
    // default 0.04) to a ~3% response. The bands are already smoothed;
    // dark ground/vignette regions give the multiply headroom even under
    // the bright default palette. Silence -> exactly 1.0.
    col *= 1.0 + 0.25 * audioBass + 0.15 * audioMid;

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = clamp(col, 0.0, 1.0);
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                     // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    // Full-field painting: tint the darkest end toward the chosen background.
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    col = uc;

    gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
}