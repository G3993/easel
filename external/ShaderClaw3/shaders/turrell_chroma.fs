/*{
  "CATEGORIES": [
    "Generator",
    "Light",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Turrell Chroma — pure colored light as material. Slowly evolving Ganzfeld fields cycle through curated triads. cycleDuration sets pace (20–600s). colorHold gives each hue a steady dwell before it changes; transitionEase softens the crossfade in/out. Organic breathing, cinematic vignette, and film grain add atmosphere. Output LINEAR HDR.",
  "INPUTS": [
    {
      "NAME": "mood",
      "LABEL": "Mood",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Aten Reign",
        "Wedgework",
        "Ganzfeld",
        "Skyspace"
      ]
    },
    {
      "NAME": "wedgeAngle",
      "LABEL": "Wedge Angle",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6.2832,
      "DEFAULT": 1.05,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "wedgeStrength",
      "LABEL": "Wedge Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.35,
      "DEFAULT": 0.16,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "cycleDuration",
      "LABEL": "Cycle (s)",
      "TYPE": "float",
      "MIN": 20,
      "MAX": 600,
      "DEFAULT": 180,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorHold",
      "LABEL": "Color Hold",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.95,
      "DEFAULT": 0.55,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "transitionEase",
      "LABEL": "Transition Ease",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "organicStrength",
      "LABEL": "Organic Movement",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.38,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "luminance",
      "LABEL": "Luminance",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 1.6,
      "DEFAULT": 1,
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
      "LABEL": "Edge Falloff",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.42,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "vignetteDepth",
      "LABEL": "Vignette Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "grainStrength",
      "LABEL": "Film Grain",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.32
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

// ════════════════════════════════════════════════════════════════════════
//  Turrell Chroma — Light & Space (Cinematic Edition)
//  Added:
//   • Organic breathing: layered domain-warped slow noise displaces the
//     field center, making the light feel alive without obvious motion.
//   • Cinematic vignette: multi-layer oval falloff (not just r²) that
//     mimics a lens barrel, deeper in corners than sides.
//   • Film grain: temporally varying per-pixel noise, luma-weighted so
//     highlights stay clean and shadows carry the grain (photographic).
// ════════════════════════════════════════════════════════════════════════

// ── Palette ───────────────────────────────────────────────────────────
vec3 paletteA(int idx) {
    if (idx == 0)  return vec3(0.86, 0.18, 0.16);
    if (idx == 1)  return vec3(0.96, 0.46, 0.18);
    if (idx == 2)  return vec3(0.98, 0.80, 0.30);
    if (idx == 3)  return vec3(0.10, 0.28, 0.78);
    if (idx == 4)  return vec3(0.18, 0.62, 0.86);
    if (idx == 5)  return vec3(0.46, 0.86, 0.74);
    if (idx == 6)  return vec3(0.16, 0.14, 0.46);
    if (idx == 7)  return vec3(0.74, 0.30, 0.62);
    if (idx == 8)  return vec3(0.96, 0.92, 0.86);
    if (idx == 9)  return vec3(0.16, 0.30, 0.62);
    if (idx == 10) return vec3(0.94, 0.50, 0.42);
    if (idx == 11) return vec3(0.96, 0.92, 0.82);
    if (idx == 12) return vec3(0.58, 0.78, 0.62);
    if (idx == 13) return vec3(0.22, 0.66, 0.62);
    if (idx == 14) return vec3(0.96, 0.94, 0.82);
    if (idx == 15) return vec3(0.34, 0.16, 0.52);
    if (idx == 16) return vec3(0.58, 0.28, 0.78);
    if (idx == 17) return vec3(0.92, 0.52, 0.66);
    if (idx == 18) return vec3(0.20, 0.74, 0.70);
    if (idx == 19) return vec3(0.30, 0.82, 0.92);
    if (idx == 20) return vec3(0.94, 0.96, 0.94);
    if (idx == 21) return vec3(0.94, 0.72, 0.30);
    if (idx == 22) return vec3(0.96, 0.56, 0.22);
    return vec3(0.98, 0.94, 0.84);
}

int moodRing(int mood, int slot) {
    if (mood == 0) {
        if (slot == 0) return 0;  if (slot == 1) return 1;  if (slot == 2) return 2;
        if (slot == 3) return 21; if (slot == 4) return 22; return 23;
    }
    if (mood == 1) {
        if (slot == 0) return 3;  if (slot == 1) return 4;  if (slot == 2) return 5;
        if (slot == 3) return 15; if (slot == 4) return 16; return 17;
    }
    if (mood == 2) {
        if (slot == 0) return 1;  if (slot == 1) return 4;  if (slot == 2) return 7;
        if (slot == 3) return 10; if (slot == 4) return 13; return 19;
    }
    if (slot == 0) return 6;  if (slot == 1) return 7;  if (slot == 2) return 8;
    if (slot == 3) return 9;  if (slot == 4) return 10; return 11;
}

float smoother(float x) {
    x = clamp(x, 0.0, 1.0);
    return x * x * x * (x * (x * 6.0 - 15.0) + 10.0);
}

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)),
                d / (q.x + e), q.x);
}
vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

// ── Hash / noise utilities ─────────────────────────────────────────────
float hash21(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

float hash11(float p) {
    return fract(sin(p * 127.1) * 43758.5453);
}

// 2D value noise — smooth interpolation between random lattice points
float vnoise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

// Layered (fractal) smooth noise — 3 octaves, kept cheap
float fbm(vec2 p) {
    float v = 0.0;
    float amp = 0.5;
    float freq = 1.0;
    for (int i = 0; i < 3; i++) {
        v   += amp * vnoise(p * freq);
        amp  *= 0.5;
        freq *= 2.1;
    }
    return v; // roughly [0,1]
}

// Domain-warped fbm — the warp makes the noise curl organically
float warpFbm(vec2 p) {
    vec2 q = vec2(fbm(p + vec2(0.0, 0.0)),
                  fbm(p + vec2(5.2, 1.3)));
    vec2 r = vec2(fbm(p + 4.0 * q + vec2(1.7, 9.2)),
                  fbm(p + 4.0 * q + vec2(8.3, 2.8)));
    return fbm(p + 4.0 * r);
}

// ── Film grain ────────────────────────────────────────────────────────
// Temporally varying grain. Uses a hash over (uv-cell, frame-bucket) so
// it changes every frame but is spatially correlated at sub-pixel scale.
float filmGrain(vec2 uv, float t) {
    // Randomise per-frame with a coarse time bucket so it flickers at
    // ~24 fps feel regardless of actual render rate.
    float timeSeed = floor(t * 24.0);
    // Two-sample hash for a roughly Gaussian distribution via Box-Muller.
    float u1 = hash21(uv * RENDERSIZE.xy + timeSeed * 17.3);
    float u2 = hash21(uv * RENDERSIZE.xy * 1.3 + timeSeed * 31.7 + 7.0);
    // Approximate Gaussian: average of uniforms (central limit theorem, 2 samples)
    float g = (u1 + u2) * 0.5 - 0.5; // [-0.5, 0.5], roughly bell-shaped
    return g;
}

void main() {
    vec2 uv  = isf_FragNormCoord.xy;
    vec2 ndc = uv * 2.0 - 1.0;
    float t  = TIME;

    int moodI = int(clamp(float(mood), 0.0, 3.0) + 0.5);

    // ── Audio split ────────────────────────────────────────────────────
    // Idle sine keeps the field breathing in silence (sound-off test);
    // real audioBass/Mid/High (previously never read — bass/mid/treb were
    // pure time-sine, so the actual track had zero effect) are knee'd in
    // on top so a driving track visibly lifts saturation/shimmer/pace.
    float a    = clamp(audioReact, 0.0, 2.0);
    float bassIdle = 0.55 + 0.45 * sin(t * 0.7);
    float midIdle  = 0.50 + 0.50 * sin(t * 0.36 + 1.3);
    float trebIdle = 0.50 + 0.50 * sin(t * 2.1 + 2.1);
    // LINEAR live followers (ambient fix r2): the bands are pre-smoothed —
    // round 1's pow(smoothstep, 1.2-1.3) knees crushed ambient's 0.1-0.8
    // swells into tiny variance. Linear keeps EDM headroom just as well.
    float bassLive = clamp(audioBass, 0.0, 1.0);
    float midLive  = clamp(audioMid,  0.0, 1.0);
    float trebLive = clamp(audioHigh, 0.0, 1.0);
    // SATURATION FIX: mix(idle, 1.0, live) pinned the driver at a constant
    // 1.0 whenever the track was loud — on EDM the response stopped varying
    // entirely (zero correlation). Instead, when music is present the
    // drivers FOLLOW the live band envelopes (which keep breathing at high
    // input thanks to the knee headroom); in silence they fall back to the
    // idle sines so the sound-off look is unchanged.
    float present = smoothstep(0.02, 0.12, max(audioLevel, audioEnergy));
    float bass = a * mix(bassIdle, 0.12 + 1.08 * bassLive, present);
    float mid  = a * mix(midIdle,  0.12 + 1.08 * midLive,  present);
    float treb = a * mix(trebIdle, 0.12 + 1.08 * trebLive, present);
    bass = max(bass, 0.0);
    mid  = max(mid,  0.0);
    treb = max(treb, 0.0);

    // ── Cycle position ─────────────────────────────────────────────────
    float transitionScale = (moodI == 3) ? 1.6 : 1.0;
    float period = max(cycleDuration, 20.0) * transitionScale;
    // Audio modulation of pace is now very gentle (±2% and scaled by the
    // audioReact amount) so the cycle timing stays predictable and under
    // the user's control rather than wobbling every frame.
    float rate   = 1.0 / period * (1.0 + 0.02 * (mid - 0.5) * a);
    float phase  = t * rate;
    float ringF  = phase * 6.0;
    float slotF  = floor(ringF);
    float raw    = ringF - slotF;              // 0..1 position within this slot
    int   sA     = int(mod(slotF,       6.0));
    int   sB     = int(mod(slotF + 1.0, 6.0));

    // ── Hold-then-transition ────────────────────────────────────────────
    // Dwell on the current colour for `colorHold` of each slot, then ease
    // into the next over the remaining time. This gives the field a calm
    // resting state on a pure hue instead of perpetually crossfading.
    //   colorHold = 0    → old always-fading behaviour
    //   colorHold = 0.55 → just over half the slot is a steady hold
    float hold  = clamp(colorHold, 0.0, 0.95);
    float tNorm = clamp((raw - hold) / max(1.0 - hold, 1e-3), 0.0, 1.0);
    // Transition Ease softens the in/out further: 0 = single smootherstep,
    // 1 = double-eased (very gradual departure and arrival on each colour).
    float frac  = smoother(tNorm);
    frac        = mix(frac, smoother(frac), clamp(transitionEase, 0.0, 1.0));

    int   idxA   = moodRing(moodI, sA);
    int   idxB   = moodRing(moodI, sB);
    vec3  cA     = paletteA(idxA);
    vec3  cB     = paletteA(idxB);

    // ── Organic breathing — domain warp the sample coordinate ──────────
    // Two nested slow waves displace the "color sample point" so the
    // field breathes and ripples as if made of light passing through warm
    // air. The displacement is kept tiny (≤ organicStrength * 0.18 NDC)
    // so it reads as atmosphere rather than animation.
    float orgS = clamp(organicStrength, 0.0, 1.0);

    // Primary slow breathing: a gentle oscillating warp center
    float breathA = sin(t * 0.11) * 0.5 + 0.5;   // 0..1, ~57s period
    float breathB = sin(t * 0.073 + 1.9) * 0.5 + 0.5; // orthogonal phase

    // Domain warp sample — we sample the warp function at a slow-moving
    // version of the fragment coordinate. The result offsets the color
    // mixing weight spatially.
    vec2 warpSeed = ndc * 0.55 + vec2(t * 0.007, t * 0.0043);
    float warpVal  = warpFbm(warpSeed);                // [0, 1]
    float warpVal2 = warpFbm(warpSeed.yx + vec2(3.7, 1.1)); // orthogonal

    // Slow pulsing displacement (sub-pixel feel, ±organicStrength * 0.18)
    vec2 disp = vec2(warpVal - 0.5, warpVal2 - 0.5) * orgS * 0.18;

    // Breathing scale: the field expands and contracts very slowly
    // (±1.5% max), like lungs, so bright areas gently pulsate.
    float breathScale = 1.0 + orgS * 0.015 * sin(t * 0.094 + warpVal * 2.0);

    // Apply organic displacement to NDC for wedge + color mixing
    vec2 ndcOrg = ndc * breathScale + disp;

    // Secondary slow "shimmer plane" — a second lower-frequency warp
    // modulates the interpolation fraction slightly per pixel.
    float shimmerWarp = vnoise(ndc * 1.8 + vec2(t * 0.019, t * 0.013)) - 0.5;
    float fracOrg = clamp(frac + shimmerWarp * orgS * 0.06, 0.0, 1.0);

    // Primary Ganzfeld field with organic frac
    vec3 field = mix(cA, cB, fracOrg);

    // ── Wedgework gradient ─────────────────────────────────────────────
    float drift    = 0.5236 * sin(phase * 6.2832);
    float angleNow = wedgeAngle + drift;
    vec2  wDir = vec2(cos(angleNow), sin(angleNow));
    float wT   = dot(ndcOrg, wDir);
    wT         = clamp(0.5 + 0.5 * wT, 0.0, 1.0);
    float wedge = mix(1.0 - wedgeStrength, 1.0 + wedgeStrength, wT);
    field *= wedge;

    // Chromatic edge bleed
    int   sPrev = int(mod(slotF + 5.0, 6.0));
    vec3  cPrev = paletteA(moodRing(moodI, sPrev));
    field = mix(field, mix(cPrev, cB, wT), 0.10);

    // ── Bass saturation lift ───────────────────────────────────────────
    vec3 hsv = rgb2hsv(field);
    hsv.y    = clamp(hsv.y + 0.05 + 0.12 * bass, 0.0, 1.0);
    hsv.x    = fract(hsv.x + 0.008 * bass);
    field    = hsv2rgb(hsv);

    // ── Treble corner shimmer ──────────────────────────────────────────
    float r2     = dot(ndcOrg, ndcOrg);
    float corner = smoothstep(0.6, 1.6, r2);
    float shimmerN = hash21(floor(uv * RENDERSIZE.xy * 0.5) + floor(t * 2.4));
    vec3  shimmer  = vec3(shimmerN, hash21(uv + 13.7), hash21(uv + 91.1)) - 0.5;
    field += shimmer * corner * treb * 0.036;

    // Overall audio-driven luminance breath — present across the whole
    // field so a driving track reads even away from the corners. r2: the
    // extra calibrated depth (bass to ~0.24 effective + a mid band) is
    // gated by `present` so the sound-off idle look is exactly unchanged.
    // r3 (measured): UP-gains half-vanish into clipping on this near-white
    // field (predicted rMag 0.0014, measured 0.0008) — under music the
    // luminance breath becomes a darken-DIP, which cannot clip. The idle-sine
    // breath is preserved verbatim in silence via the (1 - present) fade.
    field *= 1.0 + 0.14 * bass * (1.0 - present);
    field *= 1.0 - present * a * (0.36 * bassLive + 0.26 * midLive + 0.12 * trebLive);

    // ── Skyspace sun-arc ───────────────────────────────────────────────
    if (moodI == 3) {
        float arc = 0.5 + 0.5 * sin(t * 0.0084);
        float arcMask = exp(-pow((wT - arc) * 2.6, 2.0));
        field += vec3(0.06, 0.04, 0.01) * arcMask;
    }

    // ── Organic slow-pulse luminance modulation ────────────────────────
    // A very gentle (≤1.4%) luma pulse drifts across the field following
    // the warp — feels like the light source itself is breathing.
    float lumPulse = 1.0 + orgS * 0.014 * (warpVal - 0.5);
    field *= lumPulse;

    // ── Cinematic vignette ─────────────────────────────────────────────
    // Multi-layer: an elliptical outer crush + a soft inner glow roll-off.
    // This mimics a film lens barrel more faithfully than a radial r² term.
    // vignetteDepth controls the cinema crush independently of the original
    // vignette (edge falloff) parameter which remains as architectural trim.
    vec2 ndcAsp  = vec2(ndc.x, ndc.y * (RENDERSIZE.y / max(RENDERSIZE.x, 1.0)));
    float vigR   = length(ndcAsp);                      // aspect-corrected radius
    // Outer barrel crush — strong at extreme corners
    float barrelV = smoothstep(0.55, 1.35, vigR);
    // Inner roll-off — very gentle, starts at center edge
    float innerV  = smoothstep(0.0, 0.8, vigR) * 0.18;
    // r2: bass opens the barrel — the dark vignette border is the one part
    // of this near-white field with real headroom, so breathing it reads
    // even where the center clips. Music-gated; silence = authored barrel.
    float vigOpen = 1.0 - 0.50 * clamp(bass * 0.833, 0.0, 1.0) * present;
    float cinemaVig = 1.0 - clamp(vignetteDepth, 0.0, 1.0) * (barrelV * 0.85 + innerV) * vigOpen;
    field *= cinemaVig;

    // Original architectural edge falloff (separate from cinema vignette)
    float r2raw  = dot(ndc, ndc);
    float falloff = 1.0 - vignette * smoothstep(0.2, 1.8, r2raw);
    field *= falloff;

    // ── Film grain ─────────────────────────────────────────────────────
    // Luma-weighted: shadows carry more grain (photographic silver-halide
    // behaviour). Highlights stay clean. Grain is added post-vignette so
    // dark corners have appropriately noisy blacks like film.
    float luma    = dot(field, vec3(0.299, 0.587, 0.114));
    // Shadow weight: more grain in darker areas
    float shadowW = 1.0 - smoothstep(0.0, 0.65, luma);
    float grainW  = mix(0.4, 1.0, shadowW);             // 0.4 in highlights, 1.0 in shadows
    float grain   = filmGrain(uv, t);
    float grainAmt = clamp(grainStrength, 0.0, 1.0) * 0.028 * grainW;
    field += grain * grainAmt;

    // Overall luminance
    field *= luminance;

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = field;
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
    // background: tint the darkest end (vignetted edges) toward bgColor
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    field = uc;

    // The output is LINEAR HDR; host applies tone mapping.
    gl_FragColor = vec4(field, 1.0);
}