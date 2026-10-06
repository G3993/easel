/*{
  "DESCRIPTION": "Ether — volumetric light tendrils with rotating space distortion, dual detail layers and chromatic shimmer",
  "CREDIT": "nimitz (Shadertoy), adapted for ShaderClaw",
  "CATEGORIES": [
    "Generator"
  ],
  "INPUTS": [
    {
      "NAME": "tendrilSize",
      "LABEL": "Tendril Size",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.3,
      "MAX": 3,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "detailAmt",
      "LABEL": "Detail Layer",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "twist",
      "LABEL": "Twist",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "tendrilPulse",
      "LABEL": "Tendril Pulse",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "tendrilFlow",
      "LABEL": "Tendril Flow",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "axisTilt",
      "LABEL": "Axis Tilt",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.0,
      "MIN": -2.0,
      "MAX": 2.0
    },
    {
      "NAME": "detailSpin",
      "LABEL": "Detail Spin",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 1.0,
      "MIN": -2.0,
      "MAX": 2.0
    },
    {
      "NAME": "colorTint",
      "LABEL": "Color",
      "TYPE": "color",
      "DEFAULT": [
        0.5647,
        0.2941,
        0.5098,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "highlightR",
      "LABEL": "Highlight R",
      "TYPE": "float",
      "DEFAULT": 5,
      "MIN": 0,
      "MAX": 12,
      "GROUP": "Color"
    },
    {
      "NAME": "highlightG",
      "LABEL": "Highlight G",
      "TYPE": "float",
      "DEFAULT": 2.5,
      "MIN": 0,
      "MAX": 12,
      "GROUP": "Color"
    },
    {
      "NAME": "highlightB",
      "LABEL": "Highlight B",
      "TYPE": "float",
      "DEFAULT": 3,
      "MIN": 0,
      "MAX": 12,
      "GROUP": "Color"
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Color"
    },
    {
      "NAME": "shimmerAmt",
      "LABEL": "Shimmer",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Color"
    },
    {
      "NAME": "shimmerFreq",
      "LABEL": "Shimmer Freq",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.1,
      "MAX": 4,
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "depth",
      "LABEL": "Depth",
      "TYPE": "float",
      "DEFAULT": 2.5,
      "MIN": 0.5,
      "MAX": 6,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "fov",
      "LABEL": "FOV",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.4,
      "MAX": 2.5,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "centerX",
      "LABEL": "Center X",
      "TYPE": "float",
      "DEFAULT": 0.9,
      "MIN": -1,
      "MAX": 2,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "centerY",
      "LABEL": "Center Y",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": -1,
      "MAX": 2,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": 1,
      "GROUP": "Background"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ── hash helpers (borrowed pattern from data_sculpture) ──────────────────
float h11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float h21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

mat2 rot(float a) {
    float c = cos(a), s = sin(a);
    return mat2(c, -s, s, c);
}

// ── Primary tendril SDF ──────────────────────────────────────────────────
float map(vec3 p, float t) {
    p.xz *= rot(t * 0.4 * twist);
    p.xy *= rot(t * 0.3 * twist);
    p.yz *= rot(t * 0.25 * axisTilt);              // MOTION: third-axis tilt
    vec3 q = p * 2.0 + t * tendrilFlow;             // MOTION: wave flow along tendrils
    return length(p + vec3(sin(t * 0.7 * tendrilPulse))) * log(length(p) + 1.0)  // MOTION: pulse
         + sin(q.x + sin(q.z + sin(q.y))) * 0.5 * tendrilSize - 1.0;
}

// ── Secondary (detail) tendril SDF — finer scale, counter-twisted ────────
float mapDetail(vec3 p, float t) {
    // Slightly different rotation rhythm creates cross-hatch interference
    p.xz *= rot(t * 0.27 * twist * detailSpin + 0.9);   // MOTION: detail layer spin
    p.yz *= rot(t * 0.35 * twist * detailSpin + 1.4);
    p.xy *= rot(t * 0.20 * axisTilt);
    vec3 q = p * 3.7 + t * 1.3 * tendrilFlow;
    return length(p + vec3(cos(t * 0.53 * tendrilPulse + 1.1))) * log(length(p) + 1.2)
         + sin(q.x + sin(q.z * 1.2 + sin(q.y * 0.9))) * 0.38 * tendrilSize - 1.0;
}

// ── Chromatic shimmer: disperses the colour slightly per-channel ──────────
// Returns a small RGB offset vector based on uv and time.
vec3 chromaticShimmer(vec2 uv, float t) {
    float sc = shimmerFreq;
    // Three slowly-rotating sine waves, one per channel.
    float r = sin(uv.x * 7.3 * sc + t * 1.1)  * sin(uv.y * 5.1 * sc + t * 0.7);
    float g = sin(uv.x * 6.1 * sc + t * 0.9 + 1.047)
            * sin(uv.y * 8.3 * sc + t * 1.3 + 1.047);
    float b = sin(uv.x * 9.7 * sc + t * 1.4 + 2.094)
            * sin(uv.y * 4.7 * sc + t * 0.6 + 2.094);
    // Keep it gentle — thin iridescent veil
    return vec3(r, g, b) * 0.5 + 0.5;   // [0,1]
}

void main() {
    // ── Audio ──────────────────────────────────────────────────────────
    // Constant clock + small bounded phase offset. Multiplying the TIME
    // rate by bass teleported the whole field on transients (the chop).
    float t = TIME * speed + 0.3 * audioBass;

    vec2 uv = gl_FragCoord.xy / RENDERSIZE;
    vec2 p  = gl_FragCoord.xy / RENDERSIZE.y - vec2(centerX, centerY);
    p *= fov;
    // Audio zoom breathing — geometry response that reads even where the
    // HDR tendril cores clip (a color gain alone doesn't register there).
    p /= 1.0 + 0.15 * audioBass + 0.13 * audioMid;

    // ── PRIMARY ray-march ─────────────────────────────────────────────
    vec3 cl = vec3(0.0);
    float d = depth;

    for (int i = 0; i <= 5; i++) {
        vec3 pos = vec3(0.0, 0.0, 5.0) + normalize(vec3(p, -1.0)) * d;
        float rz = map(pos, t);
        float f  = clamp((rz - map(pos + 0.1, t)) * 0.5, -0.1, 1.0);
        vec3  l  = colorTint.rgb + vec3(highlightR, highlightG, highlightB) * f;

        float aa   = max(fwidth(rz) * 1.5, 1e-4);
        float core = smoothstep(2.5, -aa, rz);
        cl = cl * l + core * 0.7 * l;
        d += min(rz, 1.0);
    }

    // ── SECONDARY detail ray-march ────────────────────────────────────
    // Uses a shifted starting depth and finer field; blended additively
    // with a separate tint (complementary hue shift of ~120°).
    vec3  cl2 = vec3(0.0);
    float d2  = depth * 0.72 + 0.4;

    // Detail colour: rotate hue by ~120° relative to colorTint
    vec3 detailTint = vec3(colorTint.b, colorTint.r, colorTint.g)
                    + vec3(highlightB, highlightR, highlightG) * 0.18;

    for (int i = 0; i <= 4; i++) {
        vec3  pos2 = vec3(0.0, 0.0, 5.0) + normalize(vec3(p, -1.0)) * d2;
        float rz2  = mapDetail(pos2, t);
        float f2   = clamp((rz2 - mapDetail(pos2 + 0.1, t)) * 0.5, -0.1, 1.0);
        vec3  l2   = detailTint + vec3(highlightG, highlightB, highlightR) * f2;

        float aa2   = max(fwidth(rz2) * 1.5, 1e-4);
        float core2 = smoothstep(2.0, -aa2, rz2);
        cl2 = cl2 * l2 + core2 * 0.55 * l2;
        d2 += min(rz2, 1.0);
    }

    // Blend detail into primary — additive, user-controlled
    cl += cl2 * detailAmt;

    // ── CHROMATIC SHIMMER ─────────────────────────────────────────────
    // A gentle iridescent veil drawn from the fingerprint-warp idea in
    // data_sculpture: three phase-offset sine planes create RGB dispersion.
    {
        float shimT = TIME * shimmerFreq * 0.6;
        vec3  shimRGB = chromaticShimmer(uv, shimT);

        // Warp factor: how much the shimmer displaces each channel.
        // Tied to the luminance of cl so it glows brighter at tendril cores.
        float lumCl = dot(cl, vec3(0.299, 0.587, 0.114));
        float shimStrength = shimmerAmt * (0.3 + 0.7 * smoothstep(0.05, 0.6, lumCl));

        // Smooth, slowly-breathing envelope
        float breathe = 0.65 + 0.35 * sin(TIME * 0.31 + 1.2);

        // Add per-channel dispersion: small iridescent halo
        cl.r += shimRGB.r * shimStrength * breathe * 0.28;
        cl.g += shimRGB.g * shimStrength * breathe * 0.18;
        cl.b += shimRGB.b * shimStrength * breathe * 0.32;

        // Thin specular highlight along shimmer ridges (only at bright cores)
        float ridgeR = smoothstep(0.88, 1.0, shimRGB.r);
        float ridgeG = smoothstep(0.88, 1.0, shimRGB.g);
        float ridgeB = smoothstep(0.88, 1.0, shimRGB.b);
        float peakG  = smoothstep(0.3, 0.8, lumCl);
        cl += vec3(ridgeR, ridgeG, ridgeB) * peakG * shimStrength * 0.18;

        // Treble-style sparkle ticks from hash (data_sculpture pattern)
        float sparkSeed = h21(floor(uv * RENDERSIZE.xy * 0.5)
                            + vec2(floor(TIME * 6.0 * shimmerFreq), 0.0));
        float sparkThresh = 0.993 - audioHigh * audioReact * 0.025;
        cl += shimRGB * step(sparkThresh, sparkSeed) * shimStrength * 0.55;
    }

    // ── Audio brightness & HDR peaks ─────────────────────────────────
    cl *= brightness * (0.85 + audioLevel * audioReact * 0.55);

    float lum = dot(cl, vec3(0.299, 0.587, 0.114));
    float peakMask    = smoothstep(0.55, 1.1, lum);
    float audioPeakLift = 1.0 + audioBass * audioReact * 0.85 * peakMask;
    cl *= 1.0 + peakMask * 0.85;
    cl *= audioPeakLift;

    // Calibrated whole-frame follower (linear bands — no knee, no user-param
    // dilution) + decaying beat accent. Replaces the correlation that the
    // removed TIME-rate multiplication used to provide.
    cl *= 1.0 + 0.25 * audioBass + 0.15 * audioMid;
    float kick = pow(max(audioBeatPulse, 0.8 * audioPunch), 1.3);
    cl *= 1.0 + 0.30 * kick;

    // ── Surprise aurora curtain (every ~50 s) ─────────────────────────
    {
        vec2  _suv = uv;
        // Same 50 s period, phase-shifted so the curtain's first pass lands
        // at ~35 s instead of 0-15 s (it was polluting motion baselines).
        float _ph  = fract(TIME / 50.0 + 0.3);
        float _fw  = fwidth(_ph);
        float _f   = smoothstep(0.0, max(0.05, _fw * 2.0), _ph)
                   * smoothstep(0.30, 0.18 + _fw, _ph);
        float _wave = sin(_suv.y * 8.0 + TIME * 4.0);
        vec3  _shift = vec3(sin(_suv.y * 6.0 + TIME * 2.0),
                            sin(_suv.y * 6.0 + TIME * 2.0 + 2.094),
                            sin(_suv.y * 6.0 + TIME * 2.0 + 4.188)) * 0.5 + 0.5;
        float _w2 = _wave * _wave;
        cl += _shift * 0.22 * _f * _w2;
        cl += _shift * 0.55 * _f * _w2 * smoothstep(0.6, 1.0, _w2);
    }

    // ── Alpha ─────────────────────────────────────────────────────────
    float alpha = 1.0;
    if (transparentBg) {
        float preLum = dot(cl, vec3(0.299, 0.587, 0.114));
        alpha = clamp(preLum * 1.5, 0.0, 1.0);
    }

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = cl;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                   // saturation
    if (hueShift > 0.0005) {                               // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    float ucA = alpha;
    if (bgColor.a > 0.0) {                                 // fill low-alpha bg region
        uc = mix(uc, bgColor.rgb, (1.0 - clamp(ucA, 0.0, 1.0)) * bgColor.a);
        ucA = ucA + (1.0 - clamp(ucA, 0.0, 1.0)) * bgColor.a;
    }

    // NO TONEMAP — linear HDR out
    gl_FragColor = vec4(uc, ucA);
}