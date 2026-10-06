/*{
  "DESCRIPTION": "Chromastack — stacked sliced color terraces. Dozens of horizontal paper-cut slices form terraced wave mountains over a flat gray studio backdrop. Bass (stemBass + audioBassTime clock) swells and rolls the terrain, mids ripple the slice silhouettes with per-slice phase lag, highs lay a satin sheen along every slice edge. 3D reads from crevice shadows, slice body falloff and scalloped ribs. No text.",
  "CREDIT": "ShaderClaw",
  "CATEGORIES": [
    "Generator",
    "Audio"
  ],
  "INPUTS": [
    {
      "NAME": "sheenAmount",
      "LABEL": "Edge Sheen",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "terrainWarp",
      "LABEL": "Terrain Warp",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "sliceRipple",
      "LABEL": "Slice Ripple",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "rollRate",
      "LABEL": "Wave Roll",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "rippleRate",
      "LABEL": "Ripple Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "depthSweep",
      "LABEL": "Depth Sweep",
      "TYPE": "float",
      "MIN": -3,
      "MAX": 3,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "lobeDrift",
      "LABEL": "Lobe Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.3,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "paletteSpread",
      "LABEL": "Palette Spread",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
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
      "NAME": "bgVignette",
      "LABEL": "Bg Vignette",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1,
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
      "NAME": "audioReactivity",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ============================================================================
// CHROMASTACK — A-List VFX #1 (ref: shapes_warped_colorfullayers.jpg)
//   A back-to-front stack of 44 paper-cut slices. Each slice's silhouette is a
//   heightfield H(x, depth) with three mountain lobes along depth, so crests
//   terrace across many slices — the sliced-3D-terrain look of the reference.
//   Audio law compliance:
//     * position/phase driven by integrated clocks (TIME + audioBassTime),
//       never raw band -> position;
//     * LINEAR followers on continuous paths (amp swell, ripple, sheen);
//     * frequency -> space: bass = whole-terrain swell, mids = per-slice
//       ripple with per-slice phase lag, highs = fine edge sparkle;
//     * beautiful in silence: waves keep rolling on TIME alone.
// ============================================================================

#define NROWS 44
#define TAU 6.2831853

float hash11(float n){ return fract(sin(n * 127.1) * 43758.5453123); }
float hash21(vec2 p){
  p = fract(p * vec2(123.34, 456.21));
  p += dot(p, p + 45.32);
  return fract(p.x * p.y);
}
float noise1(float x){
  float i = floor(x), f = fract(x);
  float u = f * f * (3.0 - 2.0 * f);
  return mix(hash11(i), hash11(i + 1.0), u);
}

vec3 hueRotate(vec3 c, float a){
  float an = a * TAU;
  vec3 k = vec3(0.57735);
  float cs = cos(an), sn = sin(an);
  return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
}

// Candy anchor palette (reference: crimson/orange/gold/green/royal/pink/cream/navy)
vec3 anchorColor(float h){
  float k = fract(h) * 8.0;
  if      (k < 1.0) return vec3(0.86, 0.09, 0.17);
  else if (k < 2.0) return vec3(0.99, 0.44, 0.07);
  else if (k < 3.0) return vec3(1.00, 0.73, 0.11);
  else if (k < 4.0) return vec3(0.05, 0.58, 0.26);
  else if (k < 5.0) return vec3(0.13, 0.34, 0.90);
  else if (k < 6.0) return vec3(0.99, 0.61, 0.77);
  else if (k < 7.0) return vec3(0.97, 0.92, 0.78);
  return vec3(0.12, 0.09, 0.30);
}

vec3 rowColor(float fi, float spread){
  float h = hash11(fi * 17.31 + 4.7);
  float t = 0.5 + (h - 0.5) * spread;
  vec3 c = anchorColor(t);
  return c * (0.90 + 0.18 * hash11(fi * 3.3 + 9.1));
}

// Top silhouette of slice fi at horizontal x. ph = integrated wave clock,
// amp = terrain amplitude (bass-swelled), rip = mid ripple amount.
float rowCurve(float fi, float x, float ph, float amp, float rip){
  float z = fi / float(NROWS - 1);                 // 0 back .. 1 front
  // mid ripple: per-slice phase lag (law 3 — nothing snaps in lockstep)
  float xr = x + rip * sin(x * 11.0 + fi * 0.93 + TIME * 1.6 * rippleRate);
  // three terraced mountain lobes along depth
  // Lobe Drift: the three mountain lobes migrate through depth on the wave clock
  float ld1 = lobeDrift * sin(ph * 0.50);
  float ld2 = lobeDrift * sin(ph * 0.37 + 2.0);
  float ld3 = lobeDrift * sin(ph * 0.43 + 4.0);
  float env = exp(-pow((z - 0.16 - ld1) * 4.4, 2.0))
            + 1.15 * exp(-pow((z - 0.52 - ld2) * 4.0, 2.0))
            + 0.95 * exp(-pow((z - 0.86 - ld3) * 4.6, 2.0));
  float n  = noise1(xr * 1.5 + z * 3.1 - ph * 0.35) * 2.4;
  float w1 = 0.5 + 0.5 * sin(xr * 4.3 + n + z * 5.2 + ph * (1.0 + depthSweep * z));
  float w2 = 0.5 + 0.5 * sin(xr * 7.9 - z * 8.5 - ph * 1.31 + 2.7);
  float h  = pow(w1, 1.7) * (0.68 + 0.32 * w2);
  // pinch wave height at the frame edges (finger-end taper of the reference)
  float taper = smoothstep(0.0, 0.18, x) * smoothstep(1.0, 0.82, x);
  return 0.88 - fi * 0.019 + h * env * amp * taper;
}

void main(){
  vec2 uv = isf_FragNormCoord;
  float react = audioReactivity;

  // --- conditioned audio drivers (LINEAR followers on continuous paths) ---
  float bassL = clamp(0.65 * stemBass + 0.45 * audioBass, 0.0, 1.4) * react;
  float midL  = clamp(0.70 * audioMid + 0.40 * stemMelody, 0.0, 1.4) * react;
  float highL = clamp(0.60 * audioHigh + 0.50 * stemAir, 0.0, 1.4) * react;
  float presL = clamp(stemBassPresence * 0.8 + audioBassPresence * 0.5, 0.0, 1.2) * react;

  // wave clock: TIME keeps it rolling in silence; audioBassTime is the
  // integrated bass swell; audioPhase8 adds a smooth bar-scale sway
  float ph  = TIME * 0.30 * rollRate + audioBassTime * 0.55 + 0.5 * sin(TAU * audioPhase8) * react;
  float amp = (0.15 + 0.06 * presL) * terrainWarp * (1.0 + 0.35 * bassL);
  float rip = 0.016 * sliceRipple * (0.25 + 0.75 * midL);

  // --- find the front-most slice covering this pixel -----------------------
  float x = uv.x;
  float winFi = -1.0, winY = 0.0;
  for (int i = 0; i < NROWS; i++){
    float fi = float(i);
    float yc = rowCurve(fi, x, ph, amp, rip);
    if (uv.y <= yc){ winFi = fi; winY = yc; }
  }

  // --- flat studio backdrop ------------------------------------------------
  vec3 bgBase = (bgColor.a > 0.004) ? bgColor.rgb : vec3(0.735, 0.740, 0.745);
  vec2 vc = uv - 0.5;
  vec3 col = bgBase * (1.0 - bgVignette * 1.1 * dot(vc, vc));

  if (winFi >= 0.0){
    float px = 1.0 / RENDERSIZE.y;               // 1 pixel, in uv units — crisp at any res
    vec3 rc = rowColor(winFi, paletteSpread);
    float d = winY - uv.y;                       // depth below this slice edge
    // slice body: paper-thickness falloff, pixel-scaled so the lip stays tight
    float body = 0.80 + 0.20 * exp(-d / (7.0 * px));
    // scalloped ribs: crisp ~1px groove cuts (was a soft sine wash)
    float rp   = x * 200.0 + hash11(winFi * 5.7) * TAU + sin(uv.y * 24.0 + winFi * 0.7);
    float rdst = abs(fract(rp / TAU) - 0.5) * (TAU / 200.0); // uv distance to groove line
    float rib  = 0.995 - 0.34 * smoothstep(1.5 * px, 0.45 * px, rdst)
                       + 0.030 * sin(rp);
    // crevice shadow cast by the slice in front — hard 1-2px paper-cut contact
    // line with a tight ambient shadow beneath it (the 3D stack read)
    float crev = 0.0;
    if (winFi < float(NROWS - 1)){
      float yN = rowCurve(winFi + 1.0, x, ph, amp, rip);
      float dc = max(uv.y - yN, 0.0);
      crev = 0.30 * exp(-dc / (3.0 * px))
           + 0.32 * (1.0 - smoothstep(0.3 * px, 1.8 * px, dc));
    }
    col = rc * body * rib * (1.0 - crev);
    // satin sheen along the slice edge — 1px razor highlight + tight falloff
    float sheenK = sheenAmount * (0.30 + 0.65 * highL);
    vec3 sheenCol = mix(vec3(1.0), rc, 0.25 + 0.15 * sin(TAU * audioPhase4));
    col += sheenCol * (1.0 - smoothstep(0.2 * px, 1.6 * px, d)) * sheenK * 0.55
         + sheenCol * exp(-d / (3.0 * px)) * sheenK * 0.35;
  }

  // film grain (subtle; keeps baseline motion honest)
  col += (hash21(uv * vec2(521.7, 343.1) + fract(TIME) * 7.3) - 0.5) * 0.012;

  // whole-frame LINEAR follower (multiplies to exactly 1.0 in silence)
  col *= 1.0 + react * (0.13 * audioBass + 0.09 * audioMid);

  // universal grading
  col = hueRotate(col, hueShift);
  float l = dot(col, vec3(0.299, 0.587, 0.114));
  col = mix(vec3(l), col, colorBoost);

  gl_FragColor = vec4(col, 1.0);
}
