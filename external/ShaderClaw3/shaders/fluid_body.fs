/*{
  "DESCRIPTION": "Fluid Body + Image — organic fluid simulation with UV-advection texture warping, chromatic aberration, color rotation, surface lighting, and full audio-reactive momentum. Single-pass merge of fluid_body and fluid_image.",
  "CREDIT": "ShaderClaw",
  "CATEGORIES": [
    "Generator",
    "Simulation",
    "VFX"
  ],
  "INPUTS": [
    {
      "NAME": "inputTex",
      "LABEL": "Image/Video",
      "TYPE": "image"
    },
    {
      "NAME": "coruscate",
      "LABEL": "Coruscate",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "zap",
      "LABEL": "Zap",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "bars",
      "LABEL": "Bars",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1
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
      "NAME": "texBlend",
      "LABEL": "Texture Blend",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "showUV",
      "LABEL": "Show UV Field",
      "TYPE": "bool",
      "DEFAULT": false
    },
    {
      "NAME": "scaleFactor",
      "LABEL": "Scale Factor",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0.1,
      "MAX": 2,
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
      "NAME": "depthLayers",
      "LABEL": "Depth Layers",
      "TYPE": "float",
      "DEFAULT": 3,
      "MIN": 1,
      "MAX": 6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "moveSpread",
      "LABEL": "Move Spread",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 1,
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
      "NAME": "splatForce",
      "LABEL": "Splat Force",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.1,
      "MAX": 10,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "pulse",
      "LABEL": "Pulse",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "momentum",
      "LABEL": "Momentum",
      "TYPE": "float",
      "DEFAULT": 0.25,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "flow",
      "LABEL": "Flow",
      "TYPE": "float",
      "DEFAULT": 0.25,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "driftX",
      "LABEL": "Drift X",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": -1,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "driftY",
      "LABEL": "Drift Y",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": -1,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "advectSpeed",
      "LABEL": "Advect Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "returnRate",
      "LABEL": "Return Rate",
      "TYPE": "float",
      "DEFAULT": 0.004,
      "MIN": 0,
      "MAX": 0.05,
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
      "NAME": "vorticity",
      "LABEL": "Vorticity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "moveMode",
      "LABEL": "Movement Mode",
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
      "DEFAULT": 1,
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
      "NAME": "moveIntensity",
      "LABEL": "Move Intensity",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorRotate",
      "LABEL": "Color Rotate",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "colorSpeed",
      "LABEL": "Color Speed",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "lowColor",
      "LABEL": "Low Color",
      "TYPE": "color",
      "DEFAULT": [
        0.05,
        0,
        0.1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "highColor",
      "LABEL": "High Color",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.6,
        0.1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "limitColors",
      "LABEL": "Limit Colors",
      "TYPE": "bool",
      "DEFAULT": false,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Color"
    },
    {
      "NAME": "chromatic",
      "LABEL": "Chromatic",
      "TYPE": "float",
      "DEFAULT": 0.18,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "lens",
      "LABEL": "Lens Distort",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": true,
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
      "NAME": "reactivity",
      "LABEL": "Reactivity",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "audioSpeed",
      "LABEL": "Audio Speed",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ============================================================
// SINGLE-PASS fluid body + UV-advection texture warping
// Strategy: run full fluid field computation analytically each
// frame (no persistent buffer needed). We approximate the
// velocity field with multi-octave curl noise driven by TIME,
// then warp both the generated fluid colors AND an optional
// texture input through that field.
// ============================================================

#define PI  3.14159265
#define PI2 6.28318530
#define ROT_NUM 5

// ---- Hash / Noise ----
float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

vec2 hash22(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.xx + p3.yz) * p3.zy);
}

float simplex2D(vec2 p) {
    const float K1 = 0.366025404;
    const float K2 = 0.211324865;
    vec2 i  = floor(p + (p.x + p.y) * K1);
    vec2 a  = p - i + (i.x + i.y) * K2;
    float m = step(a.y, a.x);
    vec2 o  = vec2(m, 1.0 - m);
    vec2 b  = a - o + K2;
    vec2 c  = a - 1.0 + 2.0 * K2;
    vec3 h  = max(0.5 - vec3(dot(a,a), dot(b,b), dot(c,c)), 0.0);
    h = h * h * h * h;
    vec3 n  = h * vec3(
        dot(a, hash22(i)       - 0.5),
        dot(b, hash22(i + o)   - 0.5),
        dot(c, hash22(i + 1.0) - 0.5)
    );
    return dot(n, vec3(70.0));
}

// ---- Rotation ----
vec2 rot2(vec2 v, float a) {
    float c = cos(a), s = sin(a);
    return vec2(c*v.x - s*v.y, s*v.x + c*v.y);
}

// ---- HSV ----
vec3 rgb2hsv(vec3 c) {
    vec4 K  = vec4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
    vec4 p  = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q  = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

// ---- Lens distortion ----
vec2 lensWarp(vec2 uv, float amount) {
    vec2 c      = uv - 0.5;
    float r2    = dot(c, c);
    float dist  = 1.0 + amount * r2 * 4.0;
    return c * dist + 0.5;
}

// ---- Analytic curl-noise velocity field ----
// Returns a 2D velocity vector at position p, time t, scale sc.
vec2 curlNoise(vec2 p, float t, float sc) {
    float eps = 0.5;
    vec2 sp = p * sc + t * 0.12;
    float nx0 = simplex2D(sp + vec2( eps, 0.0));
    float nx1 = simplex2D(sp + vec2(-eps, 0.0));
    float ny0 = simplex2D(sp + vec2(0.0,  eps));
    float ny1 = simplex2D(sp + vec2(0.0, -eps));
    // curl of (ny, -nx) => (dny/dx - dnx/dy) gives rotation
    return vec2(ny0 - ny1, -(nx0 - nx1)) / (2.0 * eps);
}

// Multi-octave curl for deep turbulence
vec2 multiCurl(vec2 uv, float t, float sc, int octaves) {
    vec2 vel = vec2(0.0);
    float amp = 1.0;
    float freq = sc;
    float totalAmp = 0.0;
    for (int i = 0; i < 6; i++) {
        if (i >= octaves) break;
        vel += curlNoise(uv, t, freq) * amp;
        totalAmp += amp;
        amp  *= 0.55;
        freq *= 2.1;
    }
    return vel / totalAmp;
}

// ---- Movement pattern splat velocity at a UV position ----
vec2 movementVelocity(vec2 uv, float t, float aspect, int mMode) {
    float spread    = mix(0.05, 0.42, moveSpread);
    float intensity = moveIntensity * 0.18;
    float splatR    = splatRadius * 2.5;
    float splatR2   = splatR * splatR;
    float cutoff2   = splatR2 * 12.0;
    vec2 vel = vec2(0.0);

    if (mMode == 1) {
        for (int s = 0; s < 3; s++) {
            float fs    = float(s);
            float phase = t * (0.5 + fs * 0.3) + fs * 1.257;
            vec2 sPos   = vec2(
                0.5 + spread * sin(phase) * cos(phase * 0.7 + fs),
                0.5 + spread * cos(phase * 0.8) * sin(phase * 0.5 + fs * 1.5)
            );
            vec2 d = uv - sPos;
            d.x *= aspect;
            float d2 = dot(d, d);
            if (d2 < cutoff2) {
                vel += vec2(cos(phase * 1.3 + fs), sin(phase * 0.9 + fs * 2.0))
                     * intensity * exp(-d2 / splatR2);
            }
        }
    } else if (mMode == 2) {
        for (int s = 0; s < 3; s++) {
            float fs    = float(s);
            float phase = t * (0.8 + fs * 0.25) + fs * 1.257;
            float r     = spread * 0.5 * (0.3 + 0.7 * abs(sin(phase * 0.5)));
            float a     = phase * 0.7 + fs * 1.257;
            vec2 sPos   = vec2(0.5 + cos(a) * r, 0.5 + sin(a) * r);
            vec2 d      = uv - sPos;
            d.x *= aspect;
            float d2 = dot(d, d);
            if (d2 < cutoff2) {
                vec2 dir = normalize(sPos - 0.5 + 0.001);
                vel += dir * sin(phase * 1.5) * intensity * 1.5 * exp(-d2 / splatR2);
            }
        }
        vec2 cd = uv - 0.5;
        cd.x *= aspect;
        float cf = exp(-dot(cd, cd) / (spread * spread * 0.3));
        vel += vec2(-cd.y, cd.x) * sin(t * 0.6) * intensity * 0.5 * cf;
    } else if (mMode == 3) {
        for (int s = 0; s < 3; s++) {
            float fs    = float(s);
            float wp    = t * 0.8 + fs * 0.7;
            vec2 sPos   = vec2(
                0.5 + spread * sin(wp * 0.6 + fs * 0.8),
                0.5 + spread * 0.7 * sin(wp * 1.1 + fs * 1.7)
            );
            vec2 d  = uv - sPos;
            d.x    *= aspect;
            float d2 = dot(d, d);
            if (d2 < cutoff2) {
                vel += vec2(
                    cos(wp * 0.6 + fs * 0.8) * intensity * 1.2,
                    sin(wp * 2.2 + fs) * intensity * 0.6
                ) * exp(-d2 / splatR2);
            }
        }
        vel += vec2(
            sin(uv.x * 8.0 + t * 1.5) * sin(uv.y * 6.0 - t * 0.8),
            cos(uv.x * 5.0 - t)
        ) * intensity * 0.1;
    } else if (mMode == 4) {
        for (int s = 0; s < 3; s++) {
            float fs  = float(s);
            float vp  = t * (0.6 + fs * 0.2);
            float r   = spread * (0.15 + fs * 0.15);
            vec2 ctr  = vec2(
                0.5 + spread * 0.3 * sin(t * 0.3 + fs),
                0.5 + spread * 0.3 * cos(t * 0.25 + fs * 1.5)
            );
            vec2 sPos = ctr + vec2(cos(vp), sin(vp)) * r;
            vec2 d    = uv - sPos;
            d.x      *= aspect;
            float d2  = dot(d, d);
            if (d2 < cutoff2) {
                vec2 rad = sPos - ctr;
                vel += vec2(-rad.y, rad.x) * intensity * 2.0 / (r + 0.05) * exp(-d2 / splatR2);
            }
        }
        vec2 gd = uv - 0.5;
        gd.x   *= aspect;
        float rf = exp(-dot(gd, gd) / (spread * spread));
        float sgn = (sin(t * 0.4) > 0.0) ? 1.0 : -1.0;
        vel += vec2(-gd.y, gd.x) * sgn * intensity * 0.3 * rf;
    }
    return vel;
}

void main() {
    vec2 Res    = RENDERSIZE;
    vec2 uv     = isf_FragNormCoord;
    float aspect = Res.x / Res.y;

    float audioMod     = audioBass * reactivity;
    // Constant clock + small bounded phase offset. Multiplying the TIME
    // rate by bass made the entire curl field teleport on transients.
    float t            = TIME + 0.2 * audioBass * (0.5 + 0.5 * audioSpeed);
    int   mMode        = int(moveMode);
    int   octaves      = int(clamp(depthLayers, 1.0, 6.0));

    // ----------------------------------------------------------------
    // 1. LENS WARP on the base UV
    // ----------------------------------------------------------------
    vec2 renderUV = uv;
    if (lens > 0.01) {
        renderUV = lensWarp(uv, lens * 0.5);
    }

    // ----------------------------------------------------------------
    // 2. COMPUTE ANALYTIC MULTI-SCALE FLUID VELOCITY at renderUV
    // ----------------------------------------------------------------
    float baseScale  = scaleFactor * 1.8;
    vec2  fluidVel   = multiCurl(renderUV, t * 0.4, baseScale, octaves);
    fluidVel        *= mix(0.5, 2.0, vorticity / 5.0);

    // Movement-pattern overlay
    vec2 moveVel = movementVelocity(renderUV, t * moveSpeed, aspect, mMode);
    fluidVel += moveVel * 2.0;

    // Pulse injection
    if (pulse > 0.01) {
        float pulsePhase = sin(t * (0.5 + pulse * 2.0));
        vec2  center     = renderUV - 0.5;
        float dist       = length(center);
        float pMask      = smoothstep(0.3, 0.0, dist);
        vec2  radial     = normalize(center + 0.001);
        fluidVel += radial * pulsePhase * pulse * 0.6 * pMask;
    }

    // Organic swirl points
    for (int s = 0; s < 3; s++) {
        float fs    = float(s);
        float phase = t * (0.3 + fs * 0.15) + fs * 2.094;
        vec2 sPos   = vec2(
            0.5 + 0.3 * sin(phase * 0.7 + fs),
            0.5 + 0.3 * cos(phase * 0.5 + fs * 1.5)
        );
        vec2 d       = renderUV - sPos;
        d.x         *= aspect;
        float falloff = 1.0 / (dot(d, d) / 0.04 + 0.08);
        vec2 sVel    = vec2(cos(phase + fs * 0.5), sin(phase * 1.3 + fs));
        fluidVel    += sVel * flow * 0.08 * falloff;
    }

    // Zap burst
    if (zap > 0.01) {
        float zapPhase = hash21(vec2(floor(t * 4.0), 0.0)) * PI2;
        vec2 zapPos    = vec2(0.5 + 0.3 * cos(zapPhase), 0.5 + 0.3 * sin(zapPhase));
        vec2 d         = renderUV - zapPos;
        d.x           *= aspect;
        float falloff  = 1.0 / (dot(d, d) / 0.02 + 0.03);
        vec2 zapDir    = vec2(cos(zapPhase * 3.0), sin(zapPhase * 2.7));
        fluidVel      += zapDir * zap * 0.3 * falloff;
    }

    // Audio push
    if (audioMod > 0.05) {
        vec2  center    = renderUV - 0.5;
        // Slow continuous rotation — was a fresh random hash per frame,
        // which jittered velocity/hue every frame under audio.
        float pushAngle = TIME * 0.7;
        float dist      = length(center);
        float radialPush = smoothstep(0.4, 0.0, dist);
        fluidVel += vec2(cos(pushAngle), sin(pushAngle)) * audioMod * 0.2 * radialPush;
        float turb = audioMid * reactivity * 0.15;
        fluidVel  += (hash22(renderUV * 0.1 + TIME) - 0.5) * turb;
    }

    // ----------------------------------------------------------------
    // 3. UV ADVECTION — warp the texture lookup UV
    // ----------------------------------------------------------------
    // We integrate the velocity field over a virtual time window
    // using multiple Euler steps to get a smooth, deep warp.
    vec2 warpUV = renderUV;
    float stepSize = advectSpeed * 0.004 * fluidSpeed;
    int steps = 4;
    for (int i = 0; i < 4; i++) {
        vec2 stepVel = multiCurl(warpUV, t * 0.4 - float(i) * 0.05, baseScale, octaves);
        stepVel     += movementVelocity(warpUV, t * moveSpeed - float(i) * 0.05, aspect, mMode) * 2.0;
        warpUV      -= stepVel * stepSize;
    }
    // Return-to-identity force (lerp back toward renderUV)
    warpUV = mix(warpUV, renderUV, returnRate * 60.0 * TIMEDELTA + returnRate * 0.05);
    warpUV = fract(warpUV);

    // ----------------------------------------------------------------
    // 4. SURFACE NORMAL from UV displacement gradient
    // ----------------------------------------------------------------
    float delta = 1.5 / max(Res.x, Res.y);

    vec2 uvL = renderUV + vec2(-delta, 0.0);
    vec2 uvR = renderUV + vec2( delta, 0.0);
    vec2 uvUp= renderUV + vec2(0.0,  delta);
    vec2 uvDn= renderUV + vec2(0.0, -delta);

    // Warp neighbor UVs independently to get gradient
    vec2 wL = uvL; vec2 wR = uvR; vec2 wU = uvUp; vec2 wD = uvDn;
    for (int i = 0; i < 4; i++) {
        float fi = float(i);
        vec2 svL = multiCurl(wL, t*0.4 - fi*0.05, baseScale, octaves);
        vec2 svR = multiCurl(wR, t*0.4 - fi*0.05, baseScale, octaves);
        vec2 svU = multiCurl(wU, t*0.4 - fi*0.05, baseScale, octaves);
        vec2 svD = multiCurl(wD, t*0.4 - fi*0.05, baseScale, octaves);
        wL -= svL * stepSize; wR -= svR * stepSize;
        wU -= svU * stepSize; wD -= svD * stepSize;
    }
    wL = fract(wL); wR = fract(wR); wU = fract(wU); wD = fract(wD);

    float hL = length(wL - uvL);
    float hR = length(wR - uvR);
    float hUt= length(wU - uvUp);
    float hDt= length(wD - uvDn);

    vec3 surfNormal = normalize(vec3(
        (hR - hL)  * bumpHeight,
        (hUt - hDt)* bumpHeight,
        1.0
    ));

    // ----------------------------------------------------------------
    // 5. FLUID COLOR FIELD (analytic, no buffer)
    // ----------------------------------------------------------------
    float colorPhase = t * colorSpeed * 0.2 + colorRotate;
    float noiseVal   = simplex2D(renderUV * 3.0 + t * 0.1);
    float noiseVal2  = simplex2D(renderUV * 6.0 - t * 0.07 + 100.0);
    float speed      = length(fluidVel);

    float hue1 = fract(colorPhase + noiseVal * 0.4 + speed * 1.5);
    float hue2 = fract(hue1 + 0.33);
    float hue3 = fract(hue1 + 0.67);

    vec3 pal1 = hsv2rgb(vec3(hue1, 0.7 + noiseVal2 * 0.25, 0.55 + speed * 1.8));
    vec3 pal2 = hsv2rgb(vec3(hue2, 0.82, 0.5  + abs(noiseVal) * 0.4));
    vec3 pal3 = hsv2rgb(vec3(hue3, 0.6,  0.75));

    float blendA = abs(noiseVal);
    float blendB = abs(noiseVal2) * 0.5;
    vec3 fluidCol = mix(pal1, pal2, blendA);
    fluidCol      = mix(fluidCol, pal3, blendB);

    // ----------------------------------------------------------------
    // 6. CHROMATIC ABERRATION on velocity direction
    // ----------------------------------------------------------------
    float chromAmt  = chromatic * 0.025 * (1.0 + audioMod);
    vec2  normVel   = normalize(fluidVel + 0.0001) * chromAmt;

    // ----------------------------------------------------------------
    // 7. TEXTURE SAMPLING with warped UV + chromatic split
    // ----------------------------------------------------------------
    bool hasInput   = (IMG_SIZE(inputTex).x > 0.0);
    vec3 texCol     = vec3(0.0);

    if (hasInput && !showUV) {
        // Multi-layer parallax depth: sample at slightly offset depths
        int numLayers   = int(clamp(depthLayers, 1.0, 6.0));
        vec3 layerAccum = vec3(0.0);
        float totalW    = 0.0;

        for (int li = 0; li < 6; li++) {
            if (li >= numLayers) break;
            float fi      = float(li);
            float layerZ  = 1.0 - fi / max(float(numLayers) - 1.0, 1.0);
            float parallax= fi * 0.018 * advectSpeed;
            vec2 lUV      = fract(warpUV + fluidVel * parallax * 0.05);
            // Chromatic split per layer
            float rS      = IMG_NORM_PIXEL(inputTex, fract(lUV + normVel * layerZ)).r;
            float gS      = IMG_NORM_PIXEL(inputTex, fract(lUV)).g;
            float bS      = IMG_NORM_PIXEL(inputTex, fract(lUV - normVel * layerZ)).b;
            float w       = exp(-fi * 0.5);
            layerAccum   += vec3(rS, gS, bS) * w;
            totalW       += w;
        }
        texCol = layerAccum / totalW;
    } else if (showUV) {
        texCol = vec3(warpUV.x, warpUV.y, 0.3 + 0.3 * sin(TIME * 0.5));
    } else {
        // No input: chromatic-split from fluid color
        texCol = vec3(
            hsv2rgb(vec3(hue1, 0.7, 0.6 + speed)).r,
            hsv2rgb(vec3(hue2, 0.8, 0.55)).g,
            hsv2rgb(vec3(hue3, 0.6, 0.7)).b
        );
    }

    // ----------------------------------------------------------------
    // 8. BLEND fluid color + texture
    // ----------------------------------------------------------------
    vec3 col = mix(fluidCol, texCol, texBlend);

    // ----------------------------------------------------------------
    // 9. SURFACE LIGHTING (Blinn-Phong with HDR caustic peaks)
    // ----------------------------------------------------------------
    if (bumpHeight > 0.01) {
        vec3 lightDir = normalize(vec3(0.5, 0.8, 1.0));
        float diff    = clamp(dot(surfNormal, lightDir), 0.3, 1.0);

        vec2  sc      = (gl_FragCoord.xy - Res * 0.5) / Res.x;
        vec3  viewDir = normalize(vec3(sc, -1.0));
        vec3  halfVec = normalize(lightDir - viewDir);
        float specRaw = pow(max(dot(surfNormal, halfVec), 0.0), specPow);

        float ridgeMag = length(vec2(hR - hL, hUt - hDt)) * bumpHeight;
        float aaW      = max(fwidth(ridgeMag), 0.0001);
        float ridgeMask= smoothstep(0.0, aaW * 4.0, ridgeMag);
        float spec     = specRaw * specAmount * ridgeMask;

        float srcBright= max(max(col.r, col.g), col.b);
        float hdrGate  = smoothstep(0.45, 0.85, srcBright)
                       * smoothstep(0.04, 0.20, ridgeMag)
                       * specRaw;
        vec3  hdrPeak  = vec3(1.0, 0.97, 0.90) * hdrGate * 1.6;

        col = col * diff + vec3(spec) + hdrPeak;
    }

    // ----------------------------------------------------------------
    // 10. CORUSCATE sparkle
    // ----------------------------------------------------------------
    if (coruscate > 0.01) {
        float sparkle = pow(max(simplex2D(renderUV * Res * 0.02 + TIME * 2.0), 0.0), 8.0);
        col += sparkle * coruscate * vec3(1.0, 0.9, 0.8) * 0.6;
    }

    // ----------------------------------------------------------------
    // 11. BARS
    // ----------------------------------------------------------------
    if (bars > 0.01) {
        float barFreq    = 10.0 + bars * 40.0;
        float barPattern = abs(sin(renderUV.x * barFreq * PI));
        barPattern       = smoothstep(0.3, 0.7, barPattern);
        col = mix(col, col * (0.3 + barPattern * 0.7), bars);
    }

    // ----------------------------------------------------------------
    // 12. COLOR ROTATION
    // ----------------------------------------------------------------
    if (colorRotate > 0.01 || colorSpeed > 0.01) {
        vec3 hsv    = rgb2hsv(col);
        hsv.x       = fract(hsv.x + colorRotate + TIME * colorSpeed * 0.1);
        col         = hsv2rgb(hsv);
    }

    // ----------------------------------------------------------------
    // 13. LIMIT COLORS
    // ----------------------------------------------------------------
    if (limitColors) {
        float lum = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(lowColor.rgb, highColor.rgb, smoothstep(0.0, 1.0, lum));
    }

    // ----------------------------------------------------------------
    // 14. AUDIO BRIGHTNESS + CLAMP
    // ----------------------------------------------------------------
    col += col * audioMod * 0.3;
    // Calibrated whole-frame follower (raw linear bands — NOT scaled by
    // reactivity, whose 0.5 default would halve the depth) + beat accent.
    col *= 1.0 + 0.25 * audioBass + 0.15 * audioMid;
    // Kick kept shallow: this frame is 100% lit (blackFrac 0), so a deep
    // whole-frame beat gain spikes the frame-step metric into chop range.
    float kick = pow(max(audioBeatPulse, 0.8 * audioPunch), 1.3);
    col *= 1.0 + 0.05 * kick;
    col  = clamp(col, 0.0, 1.0);

    // ----------------------------------------------------------------
    // 15. ALPHA
    // ----------------------------------------------------------------
    float alpha = 1.0;
    if (transparentBg) {
        float lum = dot(col, vec3(0.299, 0.587, 0.114));
        alpha     = smoothstep(0.02, 0.15, lum);
    }

    // ---- universal color block (defaults = no-op) ----
    // (hue rotation handled by the existing colorRotate/colorSpeed inputs)
    vec3 uc = col;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                   // saturation
    float ucA = alpha;
    if (bgColor.a > 0.0) {                                 // fill low-alpha bg region
        uc = mix(uc, bgColor.rgb, (1.0 - ucA) * bgColor.a);
        ucA = ucA + (1.0 - ucA) * bgColor.a;
    }

    gl_FragColor = vec4(uc, ucA);
}