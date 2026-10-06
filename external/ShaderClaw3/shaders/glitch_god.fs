/*{
  "CATEGORIES": [
    "Generator",
    "Glitch",
    "Audio Reactive"
  ],
  "DESCRIPTION": "GLITCH GOD — the entire glitch-art + analog-degradation vocabulary fused into one shader (52 modules). A single GOD SEED hashes into a recipe deciding which modules fire and how hard. DIGITAL: chromatic aberration, pixel-sort, datamosh P-frame smear, 8x8 macroblock/DCT corruption, kaleido fold, sine warp, slab slice, sync tear, block-jump, jitter, mosaic, posterize, hue-rotate, channel swap, solarize, static, dither/bit-crush, ghost echo, sobel neon edges, strobe, HDR bloom. ANALOG LIBRARY (research-authentic): VHS color-under chroma smear, head-switching tear band, tape dropout, tracking-error roll, time-base wobble, generation loss, RF/luma snow; NTSC dot-crawl + cross-color rainbow, PAL Hanover bars, ringing/overshoot, interlace combing, shadow-mask + Gaussian scanlines, phosphor persistence, vertical-hold roll + VBI bar; RF multipath ghosting, hum bars, co-channel herringbone, ignition sparkle; analog video synthesis — Rutt-Etra scan terrain, video-feedback tunnel, Sandin colorizer, Paik-Abe hue, wobbulator, chroma bloom. 2^52 configurations. CHAOS stacks modules, MUTATE auto-cycles recipes live, bass/mid/treble drive the corruption. Operates on inputTex, falls back to a vivid procedural test signal. Linear HDR out for bloom. After Menkman, Murata, JODI, Cory Arcangel, Nam June Paik & Shuya Abe, Bill Etra & Steve Rutt, Dan Sandin.",
  "INPUTS": [
    {
      "NAME": "godSeed",
      "LABEL": "God Seed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1024,
      "DEFAULT": 222
    },
    {
      "NAME": "intensity",
      "LABEL": "Intensity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 1
    },
    {
      "NAME": "scanlineAmt",
      "LABEL": "Scanlines",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "noiseAmt",
      "LABEL": "Noise / Static",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "feedback",
      "LABEL": "Feedback",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "fmDepth",
      "LABEL": "FM Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "fmRatio",
      "LABEL": "FM Ratio",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5
    },
    {
      "NAME": "crossMix",
      "LABEL": "FB/FM Cross Mix",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5
    },
    {
      "NAME": "depth",
      "LABEL": "Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "vhsTex",
      "LABEL": "VHS Texture",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "grainAmt",
      "LABEL": "Texture / Grain",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "crtScreen",
      "LABEL": "CRT Screen",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "inputTex",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "pixelateAmt",
      "LABEL": "Pixelate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "blockGlitch",
      "LABEL": "Block Glitch",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "chaos",
      "LABEL": "Chaos",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "mutate",
      "LABEL": "Mutate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "glitchRate",
      "LABEL": "Glitch Rate",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "waveWarp",
      "LABEL": "Wave Warp",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "feedbackRotate",
      "LABEL": "Feedback Rotate",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "fmRate",
      "LABEL": "FM Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "chromaOrbit",
      "LABEL": "Chroma Orbit",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 0.23,
      "GROUP": "Motion"
    },
    {
      "NAME": "rollRate",
      "LABEL": "Roll Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "tearRate",
      "LABEL": "Tear Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "wobbleRate",
      "LABEL": "Wobble Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "rgbSplit",
      "LABEL": "RGB Split",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "splitAngle",
      "LABEL": "Split Angle",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "chromaRadial",
      "LABEL": "Radial Chroma",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "monochrome",
      "LABEL": "Black & White",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "posterizeAmt",
      "LABEL": "Posterize",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
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
      "NAME": "invertAmt",
      "LABEL": "Invert",
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
      "NAME": "vignetteAmt",
      "LABEL": "Vignette",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "feedbackZoom",
      "LABEL": "Feedback Zoom",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Camera / Layout"
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
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// =====================================================================
//  GLITCH GOD — 52 modules, one GOD SEED.
//  The seed hashes into a recipe: which modules are active, how strong,
//  what flavor. CHAOS = how many stack. MUTATE > 0 auto-advances the
//  recipe over time, cycling through configurations. Half the modules
//  are digital glitch; half are research-authentic analog degradation
//  (VHS / NTSC-PAL / RF / video-synthesis). Glitch art is a condition
//  the signal lives in, not a single button.
// =====================================================================

// ---- globals set in main(), read by helpers --------------------------
float gT;        // TIME * speed
float gSeed;     // effective integer recipe seed (godSeed + mutation)
float gChaos;    // module-stacking density
float gBass;     // audio bass (already audioReact-scaled)
float gMosh;     // datamosh pull active?
float gMoshAmt;  // datamosh pull amount
float gVHold;    // CRT vertical-hold roll active? (pairs VBI bar)
float gVHoldP;   // vertical-hold roll param

// ---- hash / noise ----------------------------------------------------
float h11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float h21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
vec2  h22(vec2 p)  {
    return fract(sin(vec2(dot(p, vec2(127.1, 311.7)),
                          dot(p, vec2(269.5, 183.3)))) * 43758.5453);
}

// ---- color helpers ---------------------------------------------------
vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}
vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
float luma(vec3 c) { return dot(c, vec3(0.299, 0.587, 0.114)); }

// ---- recipe RNG: deterministic per (seed, salt) ----------------------
float rr(float salt) {
    return h21(vec2(gSeed * 0.61803 + 11.0, salt * 1.7 + 3.7));
}
// module gate: rarity 0 = almost always on, 1 = rare. chaos lowers the bar.
float onMod(float salt, float rarity) {
    float bar = mix(0.15, 0.85, rarity) - gChaos * 0.42;
    return step(clamp(bar, 0.02, 0.98), rr(salt));
}
// per-module parameter in [0,1], stable for a given seed
float par(float salt) { return rr(salt + 50.0); }

// ---- procedural fallback test signal — vivid, gives glitches food ----
vec3 testSignal(vec2 uv, float t) {
    vec2 q = fract(uv);
    // cosine-palette color field (cheap + vivid) — much lighter than the old
    // bars+grid+shapes pattern; this runs on every fallback sample.
    vec3 col = 0.5 + 0.5 * cos(6.28318 * (vec3(0.0, 0.33, 0.67)
               + (q.x + q.y) * 0.9 + t * 0.07));
    col *= 0.72 + 0.28 * step(0.5, fract(q.x * 7.0));        // coarse bar banding
    vec2 p1 = vec2(0.5 + 0.35 * sin(t * 0.7), 0.5 + 0.30 * cos(t * 0.9));
    col += vec3(1.0, 0.9, 0.7) * smoothstep(0.16, 0.0, length(q - p1)) * 0.8;
    return col;
}

// ---- source fetch: input texture or fallback, with datamosh pull -----
vec3 src(vec2 p) {
    if (IMG_SIZE_inputTex.x > 0.5) return texture(inputTex, fract(p)).rgb;
    return testSignal(p, gT);
}
vec3 fetch(vec2 p) {
    if (gMosh > 0.5) {
        float bs = 20.0;
        vec2  id = floor(p * bs);
        float tb = floor(gT * 0.6);
        vec2  mv = (h22(id + tb * 13.0) - 0.5) * 2.0;
        float stick = step(0.82, h21(id + tb * 7.0));
        p -= mv * gMoshAmt * (1.0 + gBass * 1.5) * (1.0 + stick * 4.0);
    }
    return src(p);
}

// ---- shared UV-domain helpers ----------------------------------------
vec2 barrel(vec2 uv, float k) {
    vec2 c = uv - 0.5;
    float r2 = dot(c, c);
    c *= 1.0 + k * r2 + k * 0.6 * r2 * r2;
    return c + 0.5;
}
vec2 kaleido(vec2 uv, float segs) {
    vec2 c = uv - 0.5;
    float a = atan(c.y, c.x);
    float r = length(c);
    float wedge = 6.28318 / segs;
    a = abs(mod(a, wedge) - wedge * 0.5);
    return vec2(cos(a), sin(a)) * r + 0.5;
}
float bayer2(vec2 px) {
    float x = mod(px.x, 2.0);
    float y = mod(px.y, 2.0);
    float m = (x < 1.0) ? ((y < 1.0) ? 0.0 : 3.0)
                        : ((y < 1.0) ? 2.0 : 1.0);
    return (m + 0.5) / 4.0 - 0.5;
}

// =====================================================================
//  ANALOG LIBRARY  — authentic hardware-degradation modules.
//  Researched & contributed by domain agents (VHS / NTSC-PAL / RF /
//  video-synthesis). Color modules composite over the running `col`;
//  UV modules return a displaced coordinate. ISF coord origin = bottom
//  left, so head-switching sits at the bottom (uv.y small) as on real
//  decks. Pixel step where needed: vec2 px = 1.0/RES;
// =====================================================================

// --- VHS / magnetic tape ---------------------------------------------

// Color-under chroma bleed: VHS downconverts chroma to ~629 kHz so color
// is low-res and lags/smears to the right of luma edges.
vec3 vhsChromaUnder(vec3 col, vec2 uv, vec2 RES, float aB, float p, float amt) {
    if (amt <= 0.0) return col;
    float px    = 1.0 / RES.x;
    float reach = mix(4.0, 22.0, p) * (1.0 + 0.5 * aB);
    float lag   = reach * 0.45 * px;
    float Y     = luma(src(uv));
    vec3  cAcc  = vec3(0.0); float wSum = 0.0;
    const int N = 4;
    for (int i = 0; i < N; i++) {
        float fi = float(i) / float(N - 1);
        vec2  suv = vec2(uv.x - fi * reach * px - lag, uv.y);
        float w  = 1.0 - 0.85 * fi;
        cAcc += rgb2hsv(src(suv)) * w; wSum += w;
    }
    vec3 chsv = cAcc / max(wSum, 1e-4);
    vec3 outc = clamp(hsv2rgb(vec3(chsv.x, chsv.y, 1.0)) * Y, 0.0, 4.0);
    return mix(col, vec3(Y) + (outc - vec3(luma(outc))), amt);
}

// Head-switching band: heads switch during VBI, tearing + snowing the
// bottom ~6-12 scanlines.
vec3 vhsHeadSwitch(vec3 col, vec2 uv, float t, vec2 RES, float aH, float p, float amt) {
    float lines = mix(6.0, 12.0, p);
    float bandY = lines / RES.y;
    if (uv.y > bandY || amt <= 0.0) return col;
    float depth = 1.0 - uv.y / bandY;
    float lineId = floor(uv.y * RES.y);
    float jag   = (h21(vec2(lineId, floor(t * 5.0))) - 0.5);
    float shove = (depth * depth) * (0.12 + 0.18 * jag) * (0.6 + 0.4 * aH);
    vec2  tuv   = vec2(fract(uv.x - shove), uv.y);
    vec3  base  = src(tuv);
    float snow  = pow(h21(vec2(uv.x * RES.x * 0.7 + t * 97.0, lineId + t * 31.0)), 3.0);
    vec3  noisy = mix(base, vec3(0.9), snow * depth);
    noisy       = mix(noisy, noisy * 0.2, smoothstep(0.6, 1.0, depth) * 0.5);
    return mix(col, noisy, amt);
}

// Tape dropout: oxide loss -> brief white/black streaks (p<0.5) or
// dropout-compensator line-repeat (p>=0.5).
vec3 vhsDropout(vec3 col, vec2 uv, float t, vec2 RES, float p, float amt) {
    if (amt <= 0.0) return col;
    float lineId = floor(uv.y * RES.y);
    float frame  = floor(t * 30.0);
    float occur  = step(1.0 - mix(0.012, 0.07, amt), h21(vec2(lineId * 0.137, frame)));
    if (occur < 0.5) return col;
    float start  = h21(vec2(lineId, frame + 11.0));
    float len    = mix(0.04, 0.22, h21(vec2(lineId, frame + 23.0)));
    if (step(start, uv.x) * step(uv.x, start + len) < 0.5) return col;
    float edge   = smoothstep(0.0, 0.03, uv.x - start) * smoothstep(0.0, 0.03, (start + len) - uv.x);
    if (p < 0.5) {
        float pol = step(0.5, h21(vec2(lineId, frame + 5.0)));
        vec3 flash = pol > 0.5 ? vec3(0.95) : vec3(0.02);
        return mix(col, flash, edge * amt);
    }
    vec3 above = src(vec2(uv.x, uv.y + 1.0 / RES.y));
    return mix(col, above, edge * amt);
}

// Tracking error: a vertically-rolling band of horizontal jitter from
// head/track misalignment.
vec2 vhsTrackingRoll(vec2 uv, float t, vec2 RES, float aM, float p, float amt) {
    if (amt <= 0.0) return uv;
    float speed = mix(-0.25, 0.25, p) * (1.0 + 0.4 * aM);
    float bandC = fract(t * speed);
    float bandW = mix(0.06, 0.18, amt);
    float d = abs(uv.y - bandC); d = min(d, 1.0 - d);
    float inBand = 1.0 - smoothstep(0.0, bandW, d);
    float lineId = floor(uv.y * RES.y);
    float jit = (h21(vec2(lineId, floor(t * 60.0))) - 0.5);
    float wob = sin(uv.y * 180.0 + t * 40.0) * 0.5;
    return vec2(fract(uv.x + (jit * 0.10 + wob * 0.02) * inBand * amt), uv.y);
}

// Time-base instability: per-line H-sync jitter + top-of-frame flagging.
vec2 vhsTimeBase(vec2 uv, float t, vec2 RES, float aB, float p, float amt) {
    if (amt <= 0.0) return uv;
    float lineId = floor(uv.y * RES.y);
    float drift = sin(lineId * 0.07 + t * 3.0) * 0.5 + sin(lineId * 0.013 - t * 1.7) * 0.5;
    float jit   = (h21(vec2(lineId, floor(t * 48.0))) - 0.5) * 2.0;
    float wob   = (drift * 0.006 + jit * 0.0035) * (0.7 + 0.5 * aB);
    float topAmt = smoothstep(0.92, 1.0, uv.y);
    float skew  = topAmt * mix(-0.05, 0.05, p) * (drift * 0.5 + 0.5);
    return vec2(fract(uv.x + (wob + skew) * amt * mix(0.5, 1.6, p)), uv.y);
}

// Generation loss: softened luma + grown chroma noise + desat + crush.
vec3 vhsGenLoss(vec3 col, vec2 uv, float t, vec2 RES, float aM, float p, float amt) {
    if (amt <= 0.0) return col;
    float gen  = mix(1.0, 4.0, p);
    float blur = gen * 0.9 / RES.x;
    vec3 soft  = (src(uv) * 2.0 + src(uv + vec2(blur, 0.0)) + src(uv - vec2(blur, 0.0))) * 0.25;
    vec3 hsv   = rgb2hsv(soft);
    float cn   = (h21(vec2(dot(uv, vec2(311.0, 127.0)), floor(t * 24.0))) - 0.5);
    hsv.x = fract(hsv.x + cn * 0.02 * gen);
    hsv.y = clamp(hsv.y * mix(1.0, 0.55, p) + cn * 0.10 * gen, 0.0, 1.0);
    vec3 c = hsv2rgb(hsv);
    c = (c - 0.5) * mix(1.0, 0.85, p) + 0.5;
    c = clamp(c - 0.01 * gen, 0.0, 1.0) + 0.005 * gen;
    c += (h21(vec2(uv * RES + t * 60.0)) - 0.5) * 0.03 * gen * (0.8 + 0.4 * aM);
    return mix(col, clamp(c, 0.0, 1.0), amt);
}

// RF/luma snow: sparse animated white specks, worst in shadows.
vec3 vhsLumaSnow(vec3 col, vec2 uv, float t, vec2 RES, float aH, float p, float amt) {
    if (amt <= 0.0) return col;
    float Y  = luma(col);
    float s1 = h21(vec2(uv.x * RES.x + t * 113.0, uv.y * RES.y - t * 71.0));
    float s2 = h21(vec2(uv.x * RES.x * 0.5 - t * 53.0, uv.y * RES.y * 0.5 + t * 97.0));
    float sp = max(pow(s1, 8.0), pow(s2, 6.0) * 0.6);
    float k  = sp * mix(1.0, 1.0 - Y, 0.7) * mix(0.15, 0.8, p) * (0.7 + 0.6 * aH);
    float pol = step(0.92, h21(vec2(floor(uv.x * RES.x), floor(uv.y * RES.y) + t)));
    vec3 spk = mix(vec3(1.0), vec3(0.0), pol);
    return mix(col, mix(col, spk, clamp(k, 0.0, 1.0)), amt);
}

// --- CRT / NTSC-PAL composite ----------------------------------------

// Dot crawl: 3.58 MHz subcarrier chroma/luma crosstalk -> crawling
// checkerboard on vertical color edges.
vec3 crtDotCrawl(vec3 col, vec2 uv, float t, vec2 RES, float amt) {
    vec2 px = 1.0 / RES;
    float edge = length(rgb2hsv(src(uv + vec2(px.x, 0.0))).gb
                      - rgb2hsv(src(uv - vec2(px.x, 0.0))).gb);
    float phase = (uv.x * RES.x) * 1.5707963 + floor(uv.y * RES.y) * 3.1415926 + t * 9.0;
    return col + sin(phase) * edge * (0.6 * amt);
}

// Cross-color rainbow: fine luma detail misdecoded as chroma.
vec3 crtCrossColor(vec3 col, vec2 uv, float t, vec2 RES, float amt) {
    vec2 px = 1.0 / RES;
    float hf = abs(2.0 * luma(src(uv)) - luma(src(uv - vec2(px.x, 0.0)))
                                       - luma(src(uv + vec2(px.x, 0.0))));
    float phase = uv.x * RES.x * 1.5707963 + t * 7.0;
    vec3 chroma = vec3(cos(phase), cos(phase + 2.094), cos(phase + 4.188));
    return col + chroma * hf * (3.5 * amt);
}

// PAL Hanover bars: per-line V-phase error -> opposing-hue horizontal stripes.
vec3 crtPalHanover(vec3 col, vec2 uv, float t, vec2 RES, float p, float amt) {
    vec3 hsv = rgb2hsv(col);
    float line = floor(uv.y * RES.y);
    float alt  = mod(line, 2.0) * 2.0 - 1.0;
    float band = sin(line * 0.5 + t * 1.5);
    hsv.x = fract(hsv.x + alt * (p * 2.0 - 1.0) * 0.06 * (0.5 + 0.5 * band) * amt);
    hsv.y *= 1.0 + alt * 0.10 * amt;
    return hsv2rgb(hsv);
}

// Ringing / overshoot: bandwidth-limited step response -> bright halo +
// decaying dark echo to the right of edges.
vec3 crtRinging(vec3 col, vec2 uv, float t, vec2 RES, float amt) {
    vec2 px = 1.0 / RES;
    float c = luma(src(uv));
    float ring = 0.9 * (c - luma(src(uv - vec2(px.x * 1.0, 0.0))))
               - 0.5 * (c - luma(src(uv - vec2(px.x * 3.0, 0.0))))
               + 0.22 * (c - luma(src(uv - vec2(px.x * 6.0, 0.0))));
    return col + vec3(ring) * (1.4 * amt);
}

// Interlace combing: odd field lags -> comb teeth on horizontal motion.
vec3 crtInterlaceComb(vec3 col, vec2 uv, float t, vec2 RES, float amt) {
    vec2 px = 1.0 / RES;
    float odd = mod(floor(uv.y * RES.y), 2.0);
    float g = luma(src(uv + vec2(px.x, 0.0))) - luma(src(uv - vec2(px.x, 0.0)));
    float dx = g * (3.0 * px.x) * (sin(t * 60.0) * 0.5 + 0.5) * odd * amt;
    return mix(col, fetch(uv + vec2(dx, 0.0)), amt);
}

// Shadow-mask + Gaussian scanlines: RGB phosphor triads + soft beam ridges.
vec3 crtPhosphorMask(vec3 col, vec2 uv, float t, vec2 RES, float p, float amt) {
    float colIdx = mod(floor(uv.x * RES.x), 3.0);
    colIdx = mod(colIdx + step(0.5, p) * mod(floor(uv.y * RES.y / 2.0), 2.0), 3.0);
    vec3 mask = mix(vec3(1.0), vec3(colIdx == 0.0, colIdx == 1.0, colIdx == 2.0) * 1.6, amt);
    float fy = fract(uv.y * RES.y) - 0.5;
    float scan = mix(1.0, exp(-(fy * fy) / 0.10), amt * 0.85);
    return col * mask * scan;
}

// Phosphor persistence: bright objects leave a soft afterglow.
vec3 crtPersistence(vec3 col, vec2 uv, float t, vec2 RES, float amt) {
    vec3 ghost = src(uv - vec2(0.0, 1.5 / RES.y));
    float gl = max(max(ghost.r, ghost.g), ghost.b);
    return max(col, ghost * (0.35 + 0.25 * gl) * amt);
}

// Vertical-hold roll: picture slides; pair with VBI bar below.
vec2 crtVHoldRoll(vec2 uv, float t, float p, float amt) {
    uv.y = fract(uv.y + t * (p * 2.0 - 1.0) * 0.35);
    return uv;
}
// Black VBI retrace bar riding the wrap seam (same scroll as the roll).
vec3 crtVBIBar(vec3 col, vec2 uv, float t, float p, float amt) {
    float dy = fract(uv.y - fract(t * (p * 2.0 - 1.0) * 0.35));
    float barH = 0.02 + 0.06 * amt;
    return col * clamp(1.0 - step(dy, barH), 0.0, 1.0);
}

// --- RF / transmission / EMI -----------------------------------------

// RF snow: tilted noise-floor gradient that swallows the image where the
// carrier is weakest.
vec3 rfSnow(vec3 col, vec2 uv, float t, vec2 RES, float aH, float p, float amt) {
    vec2 ax = normalize(vec2(cos(p * 6.2831), sin(p * 6.2831)));
    float snr = dot(uv - 0.5, ax) * 0.5 + 0.5;
    float weak = clamp(clamp(amt * 1.3 - snr * 0.6, 0.0, 1.0) + aH * 0.15, 0.0, 1.0);
    float n = (h21(floor(uv * RES) + floor(t * 60.0) * vec2(13.1, 57.7)) - 0.5) * 1.9 + 0.5;
    return mix(col, vec3(n), smoothstep(0.0, 0.9, weak));
}

// Multipath ghosting: delayed, attenuated, horizontally-offset echoes.
vec3 rfGhost(vec3 col, vec2 uv, float t, float p, float amt) {
    float d0 = 0.018 + p * 0.045;
    vec3 acc = col; float w = amt;
    for (int i = 1; i <= 3; i++) {
        float fi = float(i);
        float off = d0 * fi;
        float gain = amt * pow(0.55, fi);
        vec3 echo = src(uv - vec2(off, 0.0));
        if (i == 1) echo.r = src(uv - vec2(off * 1.15, 0.0)).r;
        acc += echo * gain; w += gain;
    }
    return acc / max(w, 1.0);
}

// Sync loss: per-line H-tear bursts + slow vertical roll.
vec2 rfSyncLoss(vec2 uv, float t, vec2 RES, float aB, float p, float amt) {
    float y = fract(uv.y + fract(t * mix(0.02, 0.4, p) + aB * 0.05));
    float jitter = (h11(floor(uv.y * RES.y) * 1.7 + floor(t * 48.0)) - 0.5);
    float burst = step(0.6 - amt * 0.5, h11(floor(uv.y * 18.0) + floor(t * 7.0)));
    return vec2(fract(uv.x + jitter * amt * 0.25 * burst + aB * 0.04 * burst), y);
}

// Hum bars: 50/60 Hz mains beat -> soft brightness bands rolling up.
vec3 rfHumBar(vec3 col, vec2 uv, float t, float p, float amt) {
    float hum = pow(sin((uv.y * mix(1.0, 2.2, p) - t * mix(0.06, 0.13, p)) * 6.28318) * 0.5 + 0.5, 1.4);
    float depth = amt * 0.45;
    return col * (1.0 - depth + depth * hum) + depth * 0.04 * hum;
}

// Co-channel herringbone: two beating carriers -> drifting diagonal weave.
vec3 rfHerringbone(vec3 col, vec2 uv, float t, vec2 RES, float aM, float p, float amt) {
    float freq = mix(40.0, 130.0, p);
    float ang  = mix(0.35, 1.2, p);
    float c1 = sin((dot(uv, vec2(cos(ang), sin(ang))) * freq) * 6.28318 + t * 1.3);
    float c2 = sin((dot(uv, vec2(cos(-ang * 0.9), sin(-ang * 0.9))) * freq * 1.03) * 6.28318 - t * 0.91);
    float beat = 0.5 + 0.5 * sin(t * 0.4 + aM * 3.0);
    return clamp(col + vec3(c1 * c2 * amt * (0.4 + 0.6 * beat) * 0.5), 0.0, 1.0);
}

// Ignition sparkle: EMI impulse noise -> bright specks + horizontal dashes.
vec3 rfSparkle(vec3 col, vec2 uv, float t, vec2 RES, float aH, float p, float amt) {
    float gate = step(0.5, sin(t * mix(6.0, 22.0, p)));
    float density = amt * 0.06 + aH * 0.04;
    vec2 px = floor(uv * RES);
    float spark = step(1.0 - density * gate, h21(px + floor(t * 72.0) * vec2(91.7, 7.3)));
    float dash = step(1.0 - density * gate * 0.5, h21((px - vec2(1.0, 0.0)) + floor(t * 72.0) * vec2(91.7, 7.3)));
    return col + vec3(max(spark, dash * 0.7)) * (1.2 + aH);
}

// --- analog video synthesis ------------------------------------------

// Video-feedback tunnel: single-pass iterated zoom+rotate of the source.
vec3 fbTunnel(vec2 uv, float t, vec2 RES, float aB, float aM, float aH, float p, float amt) {
    vec2 c = vec2(0.5);
    float zoom = mix(1.06, 1.13, aM);
    float rot  = (p * 0.22 + 0.04 * sin(t * 0.7)) * (0.5 + aH);
    float decay = mix(0.82, 0.62, amt);
    vec2 q = uv; float gain = 1.0;
    vec3 acc = vec3(0.0); float wsum = 0.0;
    for (int i = 0; i < 5; i++) {
        if (gain < 0.02) break;                  // skip negligible loops
        float s = sin(rot), cs = cos(rot);
        q = c + mat2(cs, -s, s, cs) * (q - c) / zoom;
        q += 0.004 * vec2(sin(t * 1.1 + q.y * 9.0), cos(t * 0.9 + q.x * 9.0)) * aH;
        acc += src(clamp(q, 0.0, 1.0)) * gain;
        wsum += gain; gain *= decay;
    }
    return mix(src(uv), acc / max(wsum, 1e-3), 0.55 + 0.45 * amt);
}

// Rutt-Etra scan terrain: luminance deflects each scanline -> wireframe.
vec3 ruttEtra(vec2 uv, float t, vec2 RES, float aB, float aM, float aH, float p, float amt) {
    float lines = mix(80.0, 320.0, p);
    float row   = floor(uv.y * lines) / lines;
    float depth = (0.05 + 0.22 * amt) * (0.5 + aM);
    float L  = luma(src(vec2(uv.x, row + 0.5 / lines)));
    vec2  q  = vec2(uv.x, row - (L - 0.5) * depth);
    float L2 = luma(src(q));
    vec3  c  = src(clamp(q, 0.0, 1.0));
    float beam = pow(smoothstep(0.5, 0.0, abs(fract(uv.y * lines) - 0.5) * 2.0), 6.0) * (0.6 + 0.4 * aH);
    return (c + beam * (0.3 + 0.7 * L2)) * (0.7 + 0.6 * aB);
}

// Sandin amplitude colorizer: quantize luma -> voltage-controlled palette.
vec3 sandinColorize(vec3 col, float t, vec2 RES, float aB, float aM, float aH, float p, float amt) {
    float L = luma(col);
    float lv = mix(3.0, 9.0, p);
    float q  = floor(L * lv) / (lv - 1.0);
    float hue = fract(q * 0.85 + 0.5 * step(0.5, q) + aH * 0.5 + 0.04 * sin(t * 0.5));
    vec3 band = hsv2rgb(vec3(hue, mix(0.55, 1.0, aM), clamp(0.25 + q, 0.0, 1.0))) * (0.6 + 0.8 * aB);
    return mix(col, band, amt);
}

// Paik-Abe hue rotation: chroma subcarrier phase shift w/ nonlinear twist.
vec3 paikHueRotate(vec3 col, float t, vec2 RES, float aB, float aM, float aH, float p, float amt) {
    vec3 hsv = rgb2hsv(col);
    float phase = p + 0.15 * sin(t * 0.6) + aH * 0.5;
    hsv.x = fract(hsv.x + phase * amt * (1.0 + (hsv.z - 0.5) * aM));
    hsv.y = clamp(hsv.y * (0.7 + 0.9 * aM), 0.0, 1.0);
    return hsv2rgb(hsv) * (0.7 + 0.6 * aB);
}

// Wobbulator: audio-rate sine modulation of the deflection yoke.
vec2 wobbulator(vec2 uv, float t, vec2 RES, float aB, float aM, float aH, float p, float amt) {
    float fx = mix(2.0, 14.0, p);
    float fy = mix(1.5, 11.0, p) * (0.6 + 0.8 * aH);
    float A  = amt * 0.06 * (0.5 + aM);
    vec2 d;
    d.x = A * sin(uv.y * fy * 6.28318 + t * 2.0);
    d.y = A * sin(uv.x * fx * 6.28318 + t * 1.7);
    d += A * 0.6 * vec2(sin((uv.x + uv.y) * fx * 3.14 + t), -sin((uv.x - uv.y) * fy * 3.14 - t));
    return uv + d;
}

// Solarize / Sabattier fold: nonlinear transfer inverts highlights + Mackie rim.
vec3 solarFold(vec3 col, float t, vec2 RES, float aB, float aM, float aH, float p, float amt) {
    float thr = mix(0.35, 0.7, p) + 0.05 * sin(t * 0.4);
    vec3 folded = clamp(abs(col - thr * 2.0 * step(thr, col)), 0.0, 1.0);
    vec3 c = mix(col, folded, amt * (0.6 + 0.5 * aM));
    float mackie = smoothstep(0.06, 0.0, abs(luma(col) - thr)) * amt * (0.4 + 0.6 * aH);
    return clamp(c + mackie + (aB - 1.0) * 0.1, 0.0, 1.0);
}

// Chroma bloom: overdriven colorizer gain clips into oversaturated rails.
vec3 chromaBloom(vec3 col, float t, vec2 RES, float aB, float aM, float aH, float p, float amt) {
    vec3 hsv = rgb2hsv(col);
    hsv.y = hsv.y * (1.0 + amt * 2.0 * (0.5 + aM)) + p * 0.4 * hsv.z;
    hsv.y = clamp(hsv.y / (1.0 + max(hsv.y - 1.0, 0.0) * 0.7), 0.0, 1.0);
    hsv.x = fract(hsv.x + aH * 0.1);
    hsv.z = clamp(hsv.z * (0.85 + 0.3 * aB) + (hsv.y > 0.95 ? 0.05 : 0.0), 0.0, 1.0);
    return hsv2rgb(hsv);
}

// --- surface texture & depth (direct) --------------------------------
// Multi-octave film/video grain — "way more texture", luma-weighted to mids.
vec3 addGrain(vec3 col, vec2 uv, float t, vec2 RES, float aH, float amt) {
    if (amt <= 0.0) return col;
    float g1 = h21(floor(uv * RES)        + floor(t * 40.0))        - 0.5;
    float g2 = h21(floor(uv * RES * 0.5)  + floor(t * 23.0) + 11.0) - 0.5;
    float g3 = h21(floor(uv * RES * 0.25) + floor(t * 11.0) + 27.0) - 0.5;
    float grain = g1 * 0.6 + g2 * 0.3 + g3 * 0.2;
    float L = luma(col);
    float w = 4.0 * L * (1.0 - L);                                    // film grain peaks in mids
    return col + vec3(grain) * amt * 0.22 * (0.4 + 0.6 * w) * (1.0 + aH * 0.4);
}

// Composite VHS tape texture: grain + tracking lines + chroma noise + streaks.
vec3 vhsTexture(vec3 col, vec2 uv, float t, vec2 RES, float aH, float amt) {
    if (amt <= 0.0) return col;
    vec3 c = col;
    float g  = h21(floor(uv * RES) + floor(t * 30.0)) - 0.5;
    float ln = sin(uv.y * RES.y * 3.14159) * 0.5 + 0.5;
    float cn = h21(floor(uv * RES * 0.5) + floor(t * 18.0) + vec2(3.0, 7.0)) - 0.5;
    float streak = smoothstep(0.72, 1.0, h21(vec2(floor(uv.y * RES.y), floor(t * 8.0))));
    c += vec3(g) * amt * 0.13;                                        // tape grain
    c *= 1.0 - amt * 0.10 * ln;                                       // tracking-line darkening
    c += vec3(cn, -cn * 0.5, cn * 0.8) * amt * 0.07 * (1.0 + aH);     // chroma noise
    c = mix(c, c + vec3(0.12), streak * amt * 0.18);                  // dropout streak
    float L = luma(c);
    c = mix(c, mix(c, vec3(L), 0.18), amt);                           // mild desat
    c *= mix(vec3(1.0), vec3(1.03, 0.98, 1.03), amt);                 // faint magenta cast
    return c;
}

// Depth: luma-relief shading + volumetric center + highlight bloom.
vec3 addDepth(vec2 uv, vec3 col, float t, vec2 RES, float aB, float amt) {
    if (amt <= 0.0) return col;
    vec2 px = 1.0 / RES;
    float lx = luma(fetch(uv + vec2(px.x * 2.0, 0.0))) - luma(fetch(uv - vec2(px.x * 2.0, 0.0)));
    float ly = luma(fetch(uv + vec2(0.0, px.y * 2.0))) - luma(fetch(uv - vec2(0.0, px.y * 2.0)));
    vec3 N  = normalize(vec3(-lx, -ly, 0.35));
    vec3 Ld = normalize(vec3(0.4 * sin(t * 0.3), 0.4, 0.8));
    float shade = clamp(dot(N, Ld), 0.0, 1.0);
    vec3 c = mix(col, col * (0.55 + 0.9 * shade), amt * 0.7);          // relief shading
    float vol = 1.0 - smoothstep(0.0, 0.9, length(uv - 0.5));
    c *= mix(1.0, 0.72 + 0.5 * vol, amt * 0.5);                        // volumetric center
    c += c * smoothstep(0.7, 1.0, luma(col)) * amt * (0.4 + 0.3 * aB); // depth bloom
    return c;
}

// --- analog feedback loop CROSS frequency modulation -----------------
// Single-pass video-feedback recursion (zoom + rotate + colorize per loop)
// cross-modulated by an FM pair (modulator frequency-modulates a carrier).
// crossMix bleeds FM into the feedback transform and vice-versa, so the two
// systems interfere the way a patched analog rack does.
vec3 feedbackFM(vec2 uv, float t, vec2 RES, float aB, float aM, float aH,
                float fb, float fbZoom, float fbRot,
                float fmAmt, float fmRate, float fmRatio, float xmod) {
    // FM oscillator pair — modulator detunes the carrier's instantaneous phase
    float fr   = mix(0.5, 9.0, fmRate);
    float frat = mix(0.25, 6.0, fmRatio);
    float modOsc  = sin(t * fr * frat + uv.y * 6.0);            // modulator
    float carrier = sin(t * fr + fmAmt * 6.2831 * modOsc);      // true FM
    // FM scan warp on the sampling coordinate (audio lifts depth)
    vec2 fmWarp = vec2(carrier, modOsc) * fmAmt * 0.05 * (0.6 + aM);
    vec2 startUV = uv + fmWarp;
    if (fb < 0.004) return src(clamp(startUV, 0.0, 1.0));        // pure FM, no feedback

    vec2  c    = vec2(0.5);
    float zoom = 1.0 + fbZoom * 0.14;                            // signed: in/out
    float decay = mix(0.9, 0.5, fb);
    vec2  q = startUV; float gain = 1.0;
    vec3  acc = vec3(0.0); float wsum = 0.0;
    for (int i = 0; i < 6; i++) {
        if (gain < 0.02) break;                  // skip negligible loops
        float fi = float(i);
        // cross-mod: FM wobbles the per-loop rotation of the feedback path
        float rot = fbRot * 0.4 + xmod * fmAmt * 0.30 * sin(t * fr + fi * 0.6);
        float s = sin(rot), cs = cos(rot);
        q = c + mat2(cs, -s, s, cs) * (q - c) / zoom;
        q += vec2(carrier, modOsc) * fmAmt * 0.02 * xmod;       // FM injected each loop
        vec3 sm = src(clamp(q, 0.0, 1.0));
        vec3 hsv = rgb2hsv(sm);                                  // colorizer in the loop
        hsv.x = fract(hsv.x + fi * 0.015 + carrier * 0.06 * xmod + aH * 0.1);
        acc += hsv2rgb(hsv) * gain; wsum += gain; gain *= decay;
    }
    return (acc / max(wsum, 1e-3)) * (0.85 + 0.3 * aB);
}

// =====================================================================
void main() {
    // ---- recipe seed (+ live mutation) ----
    gChaos = clamp(chaos, 0.0, 1.0);
    gSeed  = floor(godSeed + 0.5) + floor(TIME * mix(0.0, 2.5, clamp(mutate, 0.0, 1.0)));

    float intens = clamp(intensity, 0.0, 1.5);
    float iScale = mix(0.4, 1.15, intens / 1.5);

    float aR   = clamp(audioReact, 0.0, 2.0);
    float aB   = clamp(audioBass * aR, 0.0, 2.0);
    float aM   = clamp(audioMid  * aR, 0.0, 2.0);
    float aH   = clamp(audioHigh * aR, 0.0, 2.0);
    float aAll = (aB + aM + aH) / 3.0;
    gBass = aB;

    gT = TIME * clamp(speed, 0.0, 4.0);
    float t = gT;
    float dt = gT * clamp(glitchRate, 0.1, 4.0);  // direct-control event time
    // bespoke MOTION clocks (all default 1.0 = original timing):
    float tRoll = gT * clamp(rollRate, 0.0, 3.0);   // analog rolls: V-hold, tracking band, sync roll, hum bars
    float tTear = gT * clamp(tearRate, 0.0, 3.0);   // digital event clock: slab slice, block jitter, sync tear, frame jump
    float tWob  = gT * clamp(wobbleRate, 0.0, 4.0); // continuous oscillators: sine warp, wobbulator, time-base

    vec2 uv0 = isf_FragNormCoord.xy;
    vec2 uv  = uv0;
    vec2 RES = RENDERSIZE.xy;
    vec2 px  = 1.0 / max(RES, vec2(1.0));

    // ===== CRT SCREEN (direct) — tube curvature + bezel mask ========
    float crtS = clamp(crtScreen, 0.0, 1.0);
    vec2  crtUV = uv0;
    float crtBezel = 1.0;
    if (crtS > 0.001) {
        crtUV = barrel(uv0, crtS * 0.22);
        vec2 ed = smoothstep(vec2(0.0), vec2(0.012), crtUV)
                * smoothstep(vec2(0.0), vec2(0.012), 1.0 - crtUV);
        crtBezel = ed.x * ed.y;
        uv = barrel(uv, crtS * 0.22);   // curve the sampling coordinate too
    }

    // ===== UV-DOMAIN: DIGITAL =======================================
    float onBarrel = onMod(1.0, 0.5);
    float bezel = 1.0;
    if (onBarrel > 0.5) {
        uv = barrel(uv, (0.04 + 0.14 * par(1.0)) * iScale);
        bezel = step(0.0, uv.x) * step(uv.x, 1.0) * step(0.0, uv.y) * step(uv.y, 1.0);
    }
    if (onMod(2.0, 0.82) > 0.5) uv = kaleido(uv, 2.0 + floor(par(2.0) * 6.0));
    if (onMod(3.0, 0.45) > 0.5) {
        float amp  = (0.006 + 0.05 * par(3.0)) * iScale * (1.0 + aM);
        float freq = 6.0 + 30.0 * par(3.0);
        uv.x += amp * sin(uv.y * freq + tWob * 2.0);
        uv.y += amp * 0.6 * cos(uv.x * freq * 1.3 + tWob * 1.7);
    }
    if (onMod(4.0, 0.4) > 0.5) {
        float slabs = 6.0 + floor(par(4.0) * 26.0);
        float slab  = floor(uv.y * slabs);
        float sh = (h21(vec2(slab, floor(tTear * (1.0 + 8.0 * par(4.0))))) - 0.5) * (0.04 + 0.30 * par(4.0)) * iScale;
        uv.x = fract(uv.x + sh);
    }
    if (onMod(5.0, 0.5) > 0.5) {
        float grid = 8.0 + floor(par(5.0) * 40.0);
        float tb   = floor(tTear * (2.0 + 14.0 * par(5.0)));
        vec2  bid  = floor(uv * grid);
        float jOn  = step(0.62 - aB * 0.25, h21(bid + tb));
        uv += (h22(bid + tb * 1.31) - 0.5) * (0.05 + 0.20 * par(5.0)) * iScale * jOn;
    }
    if (onMod(6.0, 0.3) > 0.5) {
        float row = floor(uv.y * RES.y * 0.5);
        uv.x = fract(uv.x + (h21(vec2(row, floor(tTear * (8.0 + aH * 22.0)))) - 0.5) * (0.03 + 0.14 * par(6.0)) * iScale);
    }
    if (onMod(7.0, 0.6) > 0.5) { float N = mix(220.0, 14.0, par(7.0)); uv = (floor(uv * N) + 0.5) / N; }
    if (onMod(8.0, 0.55) > 0.5) {
        uv += (h22(vec2(floor(tTear * (6.0 + 24.0 * par(8.0))), 7.0)) - 0.5) * (0.01 + 0.05 * par(8.0)) * iScale * (1.0 + aB);
    }
    gMosh = onMod(9.0, 0.45);
    gMoshAmt = (0.012 + 0.05 * par(9.0)) * iScale;

    // ===== UV-DOMAIN: ANALOG ========================================
    gVHold = 0.0; gVHoldP = 0.0;
    if (onMod(24.0, 0.7) > 0.5)  uv = wobbulator(uv, tWob, RES, aB, aM, aH, par(24.0), (0.35 + 0.6 * par(24.0)) * iScale);
    if (onMod(25.0, 0.55) > 0.5) uv = vhsTrackingRoll(uv, tRoll, RES, aM, par(25.0), 0.5 + 0.4 * par(25.0));
    if (onMod(26.0, 0.5) > 0.5)  uv = vhsTimeBase(uv, tWob, RES, aB, par(26.0), 0.3 + 0.4 * par(26.0));
    if (onMod(27.0, 0.78) > 0.5) { gVHold = 1.0; gVHoldP = par(27.0); uv = crtVHoldRoll(uv, tRoll, par(27.0), 0.5); }
    if (onMod(28.0, 0.75) > 0.5) uv = rfSyncLoss(uv, tRoll, RES, aB, par(28.0), 0.4 + 0.4 * par(28.0));

    // ===== DIRECT UV CONTROLS (manual knobs, 0 = off) ===============
    // Wave Warp — sine displacement, both axes
    if (waveWarp > 0.001) {
        float f = mix(8.0, 44.0, waveWarp);
        uv.x += waveWarp * 0.06 * sin(uv.y * f + dt * 3.0);
        uv.y += waveWarp * 0.04 * cos(uv.x * f * 1.2 + dt * 2.3);
    }
    // Block Glitch — digital block displacement + horizontal slice tear
    if (blockGlitch > 0.001) {
        float a = blockGlitch;
        float grid = mix(40.0, 8.0, a);
        vec2  bid  = floor(uv * grid);
        float tb   = floor(dt * mix(4.0, 18.0, a));
        float on   = step(0.70 - a * 0.45, h21(bid + tb));
        uv += (h22(bid + tb * 1.7) - 0.5) * a * 0.16 * on;
        float row  = floor(uv.y * mix(10.0, 44.0, a));
        float tear = step(0.6, h21(vec2(row, tb * 0.7))) * (h21(vec2(row, tb)) - 0.5);
        uv.x = fract(uv.x + tear * a * 0.25);
    }
    // Pixelate — mosaic quantize (last so blocks stay crisp)
    if (pixelateAmt > 0.001) {
        float N = mix(220.0, 10.0, pixelateAmt);
        uv = (floor(uv * N) + 0.5) / N;
    }

    // ===== SAMPLE with chromatic aberration =========================
    float chromaOn = onMod(10.0, 0.12);
    float chr = (3.0 + 22.0 * par(10.0)) * px.x * iScale * (0.7 + aH * 1.5) * (0.35 + chromaOn);
    float ang = t * chromaOrbit + par(10.0) * 6.28;   // chromaOrbit spins the RGB fringe direction
    vec2  dR  = vec2( cos(ang),  sin(ang)) * chr;
    vec2  dB  = vec2(-cos(ang * 1.13), -sin(ang * 1.13)) * chr;
    // DIRECT chromatic controls: linear RGB Split (with angle) + Radial Chroma
    float sAng = splitAngle * 6.28318;
    vec2  sDir = vec2(cos(sAng), sin(sAng));
    vec2  uSplit  = sDir * rgbSplit * 0.06;            // up to 6% screen, directional
    vec2  uRadial = (uv - 0.5) * chromaRadial * 0.12;  // prismatic dispersion from center
    vec2  offR = dR + uSplit + uRadial;
    vec2  offB = dB - uSplit - uRadial;
    vec3 col = vec3(fetch(uv + offR).r, fetch(uv).g, fetch(uv + offB).b);

    // ===== ANALOG BASE SAMPLERS (replace/blend the base image) ======
    if (onMod(29.0, 0.72) > 0.5) col = mix(col, fbTunnel(uv, t, RES, aB, aM, aH, par(29.0), 0.6 + 0.3 * par(29.0)), 0.55 + 0.35 * par(29.0));
    if (onMod(30.0, 0.75) > 0.5) col = mix(col, ruttEtra(uv, t, RES, aB, aM, aH, par(30.0), 0.4 + 0.5 * par(30.0)), 0.6);
    if (onMod(31.0, 0.45) > 0.5) col = vhsChromaUnder(col, uv, RES, aB, par(31.0), 0.5 + 0.4 * par(31.0));

    // ===== ANALOG FEEDBACK x FM ENGINE (direct knobs) ===============
    if (feedback > 0.001 || fmDepth > 0.001) {
        vec3 ffm = feedbackFM(uv, t, RES, aB, aM, aH,
                              feedback, feedbackZoom, feedbackRotate,
                              fmDepth, fmRate, fmRatio, crossMix);
        col = mix(col, ffm, clamp(max(feedback, fmDepth) * 1.1, 0.0, 1.0));
    }

    // ===== COLOR-DOMAIN: DIGITAL ====================================
    if (onMod(11.0, 0.55) > 0.5) {
        float ag = par(11.0) * 6.28;
        vec2  go = vec2(cos(ag), sin(ag)) * (0.01 + 0.06 * par(11.0)) * iScale;
        col += fetch(uv + go) * (0.25 + 0.45 * par(11.0)) + fetch(uv - go) * (0.15 + 0.30 * par(11.0));
    }
    if (onMod(12.0, 0.7) > 0.5) {
        float lx = luma(fetch(uv + vec2(px.x, 0.0))) - luma(fetch(uv - vec2(px.x, 0.0)));
        float ly = luma(fetch(uv + vec2(0.0, px.y))) - luma(fetch(uv - vec2(0.0, px.y)));
        float edge = clamp(length(vec2(lx, ly)) * (4.0 + 30.0 * par(12.0)), 0.0, 1.0);
        vec3 eCol = hsv2rgb(vec3(fract(par(12.0) + t * 0.1), 0.9, 1.0));
        col = mix(col, col * 0.3 + eCol * 1.3, edge * (0.5 + 0.5 * par(12.0)));
    }
    if (onMod(13.0, 0.5) > 0.5) {
        float lv = 2.0 + floor(par(13.0) * 6.0);
        col = floor(col * lv + 0.5) / lv;
        col *= 1.0 + 0.4 * step(0.66, max(max(col.r, col.g), col.b));
    }
    if (onMod(14.0, 0.55) > 0.5) {
        vec3 hsv = rgb2hsv(col);
        hsv.x = fract(hsv.x + par(14.0) + t * 0.05 * step(0.5, par(14.0)));
        hsv.y = clamp(hsv.y * (0.8 + par(14.0) * 0.8), 0.0, 1.0);
        col = hsv2rgb(hsv);
    }
    if (onMod(15.0, 0.7) > 0.5) {
        float v = par(15.0);
        if      (v < 0.30) col = vec3(1.0) - col;
        else if (v < 0.55) col = col.gbr;
        else if (v < 0.80) col = col.brg;
        else               col = vec3(1.0 - col.r, col.g, 1.0 - col.b);
    }
    if (onMod(16.0, 0.7) > 0.5) {
        float thr = 0.35 + 0.4 * par(16.0);
        col = mix(col, vec3(1.0) - col, step(vec3(thr), col));
    }
    if (onMod(17.0, 0.5) > 0.5) {
        float bs    = mix(6.0, 24.0, par(17.0));
        vec2  blkPx = floor(uv0 * RES / bs) * bs;
        vec2  blkUV = (blkPx + bs * 0.5) / RES;
        float seed  = h21(blkPx + floor(t * 12.0));
        float kick  = clamp(aB * 1.6 + 0.16, 0.0, 1.7);
        float thresh = 0.20 + kick * 0.40;
        if (seed < thresh) {
            vec3 hsv = rgb2hsv(fetch(blkUV));
            hsv.x = fract(hsv.x + seed * 1.7 + 0.3);
            hsv.z = clamp(hsv.z * (0.85 + seed * 0.7), 0.0, 1.6);
            vec3 mb = hsv2rgb(hsv);
            if (seed < thresh * 0.3) mb = vec3(1.0) - mb;
            col = mix(col, mb * (1.0 + kick * 0.6), 0.9);
        }
    }
    if (onMod(18.0, 0.25) > 0.5) {
        float scan = 0.88 + 0.12 * sin(uv0.y * RES.y * 3.14159);
        col = col * scan + vec3(0.12, 0.15, 0.12) * smoothstep(0.99, 1.0, scan);
    }
    if (onMod(19.0, 0.35) > 0.5) {
        float amt = (0.05 + 0.25 * par(19.0)) * (0.5 + aH);
        col += vec3(h21(floor(uv0 * RES) + floor(t * 40.0)) - 0.5) * amt;
        float speck = step(0.997 - aH * 0.012, h21(floor(uv0 * RES * 0.7) + floor(t * 26.0)));
        col += vec3(1.15, 1.05, 0.95) * speck * 1.3;
    }
    if (onMod(20.0, 0.6) > 0.5) {
        float lv = 2.0 + floor(par(20.0) * 5.0);
        col = floor(col * lv + 0.5 + (bayer2(uv0 * RES) / lv) * lv) / lv;
    }
    if (onMod(21.0, 0.3) > 0.5) {
        float L = luma(col);
        col *= mix(vec3(1.0), vec3(1.06, 0.90, 1.00), 1.0 - smoothstep(0.0, 0.42, L));
        col *= mix(vec3(1.0), vec3(0.95, 1.05, 0.93), smoothstep(0.55, 1.10, L));
    }
    if (onMod(22.0, 0.8) > 0.5) {
        float on = step(0.78, h11(floor(t * (2.0 + 10.0 * par(22.0))) + gSeed));
        if (on > 0.5) col = (par(22.0) > 0.5) ? vec3(1.0) - col : col + vec3(0.6) * (0.5 + aAll);
    }

    // ===== COLOR-DOMAIN: ANALOG (signal artifacts) ==================
    if (onMod(32.0, 0.55) > 0.5) col = crtDotCrawl(col, uv0, t, RES, (0.4 + 0.5 * par(32.0)) * iScale);
    if (onMod(33.0, 0.6) > 0.5)  col = crtCrossColor(col, uv0, t, RES, (0.3 + 0.4 * par(33.0)) * iScale);
    if (onMod(34.0, 0.55) > 0.5) col = crtRinging(col, uv0, t, RES, (0.3 + 0.4 * par(34.0)) * iScale);
    if (onMod(35.0, 0.7) > 0.5)  col = crtPalHanover(col, uv0, t, RES, par(35.0), 0.5 + 0.5 * par(35.0));
    if (onMod(36.0, 0.72) > 0.5) col = crtInterlaceComb(col, uv0, t, RES, 0.3 + 0.4 * par(36.0));
    if (onMod(37.0, 0.6) > 0.5)  col = sandinColorize(col, t, RES, aB, aM, aH, par(37.0), 0.55 + 0.35 * par(37.0));
    if (onMod(38.0, 0.65) > 0.5) col = paikHueRotate(col, t, RES, aB, aM, aH, par(38.0), 0.45 + 0.4 * par(38.0));
    if (onMod(39.0, 0.7) > 0.5)  col = solarFold(col, t, RES, aB, aM, aH, par(39.0), 0.5 + 0.4 * par(39.0));
    if (onMod(40.0, 0.6) > 0.5)  col = chromaBloom(col, t, RES, aB, aM, aH, par(40.0), 0.35 + 0.4 * par(40.0));
    if (onMod(41.0, 0.6) > 0.5)  col = rfGhost(col, uv, t, par(41.0), 0.35 + 0.35 * par(41.0));
    if (onMod(42.0, 0.5) > 0.5)  col = rfHumBar(col, uv0, tRoll, par(42.0), 0.3 + 0.35 * par(42.0));
    if (onMod(43.0, 0.7) > 0.5)  col = rfHerringbone(col, uv0, t, RES, aM, par(43.0), 0.25 + 0.35 * par(43.0));

    // ===== HDR bloom feeders (digital) ==============================
    if (onMod(23.0, 0.15) > 0.5) {
        col += col * smoothstep(0.78, 1.10, luma(col)) * (0.6 + 0.6 * par(23.0));
        col += max(col - vec3(1.0), vec3(0.0)) * 0.8;
    }
    col += vec3(0.04 * (h21(floor(uv0 * RES * 0.5) + floor(t * 5.0)) - 0.5));

    // ===== ANALOG DISPLAY / OVERLAY LAYER (last, on top) ============
    if (onMod(44.0, 0.65) > 0.5) col = crtPersistence(col, uv0, t, RES, 0.3 + 0.25 * par(44.0));
    if (onMod(45.0, 0.6) > 0.5)  col = crtPhosphorMask(col, uv0, t, RES, par(45.0), 0.4 + 0.4 * par(45.0));
    if (onMod(46.0, 0.55) > 0.5) col = vhsGenLoss(col, uv0, t, RES, aM, par(46.0), 0.5 + 0.4 * par(46.0));
    if (onMod(47.0, 0.5) > 0.5)  col = rfSnow(col, uv0, t, RES, aH, par(47.0), 0.3 + 0.3 * par(47.0));
    if (onMod(48.0, 0.55) > 0.5) col = vhsLumaSnow(col, uv0, t, RES, aH, par(48.0), 0.3 + 0.3 * par(48.0));
    if (onMod(49.0, 0.7) > 0.5)  col = rfSparkle(col, uv0, t, RES, aH, par(49.0), 0.3 + 0.4 * par(49.0));
    if (onMod(50.0, 0.55) > 0.5) col = vhsDropout(col, uv0, t, RES, par(50.0), 0.3 + 0.4 * par(50.0));
    if (onMod(51.0, 0.5) > 0.5)  col = vhsHeadSwitch(col, uv0, t, RES, aH, par(51.0), 0.6 + 0.3 * par(51.0));
    if (gVHold > 0.5)            col = crtVBIBar(col, uv0, tRoll, gVHoldP, 0.4);

    // Depth — relief shading + volume + bloom (before color finishing)
    col = addDepth(uv0, col, t, RES, aB, depth);

    // ===== DIRECT COLOR CONTROLS (manual finishing knobs) ===========
    if (hueShift > 0.001) { vec3 hsv = rgb2hsv(col); hsv.x = fract(hsv.x + hueShift); col = hsv2rgb(hsv); }
    if (posterizeAmt > 0.001) { float lv = mix(16.0, 2.0, posterizeAmt); col = mix(col, floor(col * lv + 0.5) / lv, posterizeAmt); }
    col = mix(col, vec3(1.0) - col, clamp(invertAmt, 0.0, 1.0));     // Invert
    col = mix(col, vec3(luma(col)), clamp(monochrome, 0.0, 1.0));    // Black & White
    if (scanlineAmt > 0.001) col *= 1.0 - scanlineAmt * 0.6 * (0.5 + 0.5 * sin(uv0.y * RES.y * 3.14159));
    if (noiseAmt > 0.001) col += vec3(h21(floor(uv0 * RES) + floor(dt * 40.0)) - 0.5) * noiseAmt * 0.6;
    if (vignetteAmt > 0.001) col *= 1.0 - vignetteAmt * smoothstep(0.3, 0.9, length(uv0 - 0.5));

    // ===== AUDIO PUNCH — always-on reactive hit, independent of the
    // probabilistic module stack above (so audio reads even when this
    // frame's random recipe happened to dodge every audio-linked module) ==
    float aPunch = clamp(aAll, 0.0, 1.0);
    vec3 hsvP = rgb2hsv(col);
    hsvP.x = fract(hsvP.x + aPunch * 0.16);                    // hue rotate on loud
    col = hsv2rgb(hsvP);
    col = mix(col, vec3(1.0) - col, aPunch * 0.42);            // invert-flash on loud
    col *= 1.0 + aPunch * 0.55;                                // brightness lift on loud

    // surface texture (final layer): VHS tape texture, then film/video grain
    col = vhsTexture(col, uv0, t, RES, aH, vhsTex);
    col = addGrain(col, uv0, t, RES, aH, grainAmt);

    // ===== CRT SCREEN overlay: scanlines + shadow mask + vignette ===
    // (pure math on the existing color — no extra source samples)
    if (crtS > 0.001) {
        float scan = 0.6 + 0.4 * sin(crtUV.y * RES.y * 3.14159);
        col *= mix(1.0, scan, crtS * 0.5);
        float mx = mod(floor(crtUV.x * RES.x), 3.0);
        vec3 mask = vec3(mx == 0.0, mx == 1.0, mx == 2.0) * 1.4 + 0.25;
        col *= mix(vec3(1.0), mask, crtS * 0.4);
        float vig = 1.0 - smoothstep(0.5, 1.15, length((crtUV - 0.5) * 1.3));
        col *= mix(1.0, vig, crtS * 0.6);
        col += col * smoothstep(0.6, 1.0, luma(col)) * crtS * 0.2;  // tube glow
        col *= crtBezel;                                            // black outside tube
    }

    // bezel: outside curved CRT goes black
    col *= bezel;

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);
    col = mix(col, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    // Fully opaque — solid output, no alpha/transparency.
    gl_FragColor = vec4(max(col, vec3(0.0)), 1.0);
}
