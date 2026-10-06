/*{
  "DESCRIPTION": "Cruz-Diez — a Physichromie of dense vertical painted slats over chevron color fields. Diamond bands of curated greens, reds, teals and warm yellows breathe while odd/even slats interleave two color series, making the surface vibrate optically. Slats now blend into their neighbors with soft gradient edges, and a small per-channel dispersion feedback halos the lines. Motion rides the music clock; bass swells the chevrons and pushes the feedback, mids breathe sparse slats, highs micro-shift the hues.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Geometry",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "stripeCount",
      "LABEL": "Slats",
      "TYPE": "float",
      "MIN": 60,
      "MAX": 320,
      "DEFAULT": 170,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "zigzagFreq",
      "LABEL": "Diamonds Across",
      "TYPE": "float",
      "MIN": 2,
      "MAX": 14,
      "DEFAULT": 6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "chevronRows",
      "LABEL": "Bands Down",
      "TYPE": "float",
      "MIN": 2,
      "MAX": 14,
      "DEFAULT": 6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "zigzagAmp",
      "LABEL": "Zigzag Amplitude",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1.4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "textureAmt",
      "LABEL": "Surface Texture",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "blendAmt",
      "LABEL": "Line Blend / Softness",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.45,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "feedback",
      "LABEL": "Dispersion Feedback",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.9,
      "DEFAULT": 0.35,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "dispersion",
      "LABEL": "Chromatic Dispersion",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "moveSpeed",
      "LABEL": "Drift Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
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
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "fbBuf",
      "PERSISTENT": true
    },
    {
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

// Curated Cruz-Diez series — flat silkscreen inks, no pastels.
vec3 palette(float i) {
    float k = mod(i, 10.0);
    if (k < 0.5) return vec3(0.980, 0.845, 0.075);  // cadmium yellow
    if (k < 1.5) return vec3(0.950, 0.520, 0.060);  // orange
    if (k < 2.5) return vec3(0.585, 0.800, 0.085);  // chartreuse
    if (k < 3.5) return vec3(0.145, 0.680, 0.235);  // signal green
    if (k < 4.5) return vec3(0.040, 0.360, 0.215);  // deep green
    if (k < 5.5) return vec3(0.045, 0.435, 0.435);  // teal
    if (k < 6.5) return vec3(0.760, 0.115, 0.145);  // vermillion red
    if (k < 7.5) return vec3(0.555, 0.080, 0.245);  // crimson-magenta
    if (k < 8.5) return vec3(0.055, 0.115, 0.300);  // ultramarine navy
    return vec3(0.115, 0.325, 0.585);               // cerulean
}

// Zone-weighted pick: warm inks pool toward the bottom of the sheet,
// cool greens/teals/reds toward the top — like the reference print.
float pickIndex(float h, float zone) {
    float warm = floor(h * 3.999);            // 0..3  yellow..green
    float cool = 3.0 + floor(h * 6.999);      // 3..9  green..cerulean
    return (hash11(h * 91.7) < zone) ? cool : warm;
}

// Audio conditioning shared across passes (set once in main)
float gA;       // reactivity master
float gBassP;   // structural weight
float gMidP;    // detail breath
float gHighP;   // sparkle / hue
float gMclk;    // motion clock: drift time + music-accumulated time

// Flat ink of one chevron region, both interleaved series resolved by parity
vec3 regionInk(float cell, float colId, float parity, float rows, float amp) {
    float cy   = clamp((cell + 0.5) / (rows + amp), 0.0, 1.0);
    float zone = smoothstep(0.12, 0.72, cy);
    vec2  rid  = vec2(cell, colId);

    float hA = hash21(rid * 1.618 + paletteShift * 7.3);
    float hB = hash21(rid * 3.113 + 41.7 + paletteShift * 7.3);
    vec3 colA = palette(pickIndex(hA, zone));
    // partner series: a second ink, pulled dark — reads as the shadow color
    // between rods that makes the field shimmer at a distance
    vec3 colB = mix(palette(pickIndex(hB, 1.0 - zone * 0.5)), vec3(0.03, 0.05, 0.10), 0.45);

    return (parity < 0.5) ? colA : mix(colA * 0.42, colB, 0.62);
}

// Region color of one slat column at height uvy — chevron field, soft band
// gradient between vertically adjacent regions, band-tip depth shading, and
// the per-slat mid-band breathing.
vec3 stripColor(float strip, float uvy) {
    float n  = max(floor(stripeCount), 8.0);
    float zf = max(zigzagFreq, 1.0);
    float amp = zigzagAmp * (1.0
                + 0.06 * sin(gMclk * 0.31)         // slow breathing
                + 0.22 * gA * gBassP);             // bass swells the diamonds
    float ph  = gMclk * 0.045;                     // sideways drift on the music clock
    float xw  = (strip + 0.5) / n;
    float xp  = xw * zf + ph;
    float w1  = abs(fract(xp) * 2.0 - 1.0);        // triangle wave 0..1
    float colId = floor(xp);
    float parity = mod(strip, 2.0);

    float rows  = max(chevronRows, 1.0);
    float field = uvy * rows + w1 * amp + 0.18 * sin(gMclk * 0.11)
                + 0.36 * gA * gBassP;              // bass rides the whole lattice
    float cell  = floor(field);
    float fy    = fract(field);

    vec3 inkC = regionInk(cell, colId, parity, rows, amp);
    vec3 inkN = regionInk(cell + 1.0, colId, parity, rows, amp);

    // soft gradient across the band boundary instead of a hard cut
    float yw = 0.05 + 0.32 * blendAmt;
    vec3 ink = mix(inkC, inkN, smoothstep(1.0 - yw, 1.0, fy));

    // band depth: darken toward chevron tips like layered card stock,
    // widened and lightened as softness rises
    float ew = 0.14 + 0.18 * blendAmt;
    float bandEdge = smoothstep(0.0, ew, fy) * smoothstep(1.0, 1.0 - ew, fy);
    float depth = 0.28 * (1.0 - 0.45 * blendAmt);
    ink *= (1.0 - depth) + depth * bandEdge;

    // sparse slats breathe with the mids
    float spark = step(0.90, hash11(strip * 3.7 + 11.0));
    ink *= 1.0 + spark * 0.28 * gA * gMidP;

    return ink;
}

// ── Pass 0: physichromie + dispersion feedback into persistent buffer ──
vec4 passField() {
    vec2 uv = isf_FragNormCoord.xy;
    float n = max(floor(stripeCount), 8.0);
    float sx = uv.x * n;
    float strip = floor(sx);
    float fx = fract(sx);

    // region color, blended into both neighbor slats near the edges so the
    // lines melt together instead of cutting hard
    vec3 ink = stripColor(strip, uv.y);
    float xw = 0.10 + 0.38 * blendAmt;
    float mR = smoothstep(1.0 - xw, 1.0, fx) * (0.25 + 0.35 * blendAmt);
    float mL = smoothstep(xw, 0.0, fx)       * (0.25 + 0.35 * blendAmt);
    ink = mix(ink, stripColor(strip + 1.0, uv.y), mR);
    ink = mix(ink, stripColor(strip - 1.0, uv.y), mL);

    // slat relief: rounded painted rod — groove narrows and relief relaxes
    // as softness rises, edges stay as soft gradients rather than dark cuts
    float slat   = sin(fx * 3.14159265);
    float shade  = mix(1.0, 0.30 + 0.70 * pow(max(slat, 0.0), 0.55), 0.5 + 0.5 * textureAmt);
    float gw     = 0.10 * (1.0 - 0.72 * blendAmt);
    float groove = smoothstep(0.0, gw, fx) * smoothstep(1.0, 1.0 - gw, fx);
    float hilite = pow(max(sin((fx - 0.14) * 3.14159265), 0.0), 9.0) * 0.22 * textureAmt;
    float relief = 0.85 * (1.0 - 0.45 * blendAmt);
    ink = ink * mix(1.0, shade * groove, relief) + hilite * (ink + 0.25);

    // silkscreen streak texture (kept pre-feedback so trails inherit it)
    float streak = hash21(vec2(strip * 1.7, floor(uv.y * 96.0)));
    ink *= 1.0 + (streak - 0.5) * 0.10 * textureAmt;

    // ── small dispersion feedback ──
    // previous frame sampled through a gentle outward zoom, each channel at a
    // slightly different magnification → soft rainbow-fringed halo around the
    // lines. Bass pushes the field outward and deepens the echo.
    vec2 c = uv - 0.5;
    float zm = 1.0 + 0.006 + 0.012 * gA * gBassP;
    float dr = dispersion * 0.006;
    vec3 prev;
    prev.r = texture2D(fbBuf, clamp(0.5 + c / (zm + dr), 0.0, 1.0)).r;
    prev.g = texture2D(fbBuf, clamp(0.5 + c /  zm,       0.0, 1.0)).g;
    prev.b = texture2D(fbBuf, clamp(0.5 + c / (zm - dr), 0.0, 1.0)).b;
    float fbA = feedback * (0.85 + 0.30 * gA * gBassP);
    vec3 col = max(ink, prev * (0.50 + 0.48 * fbA));

    return vec4(col, 1.0);
}

// ── Pass 1: soft blur of the feedback field + full-frame audio breath ──
vec4 passPresent() {
    vec2 uv = isf_FragNormCoord.xy;
    vec2 px = 1.0 / RENDERSIZE.xy;

    // 5-tap diagonal gaussian — blurred edges, radius grows with softness
    float r = 0.6 + 1.8 * blendAmt;
    vec3 ink = texture2D(fbBuf, uv).rgb * 0.4;
    ink += texture2D(fbBuf, uv + vec2( r,  r) * px).rgb * 0.15;
    ink += texture2D(fbBuf, uv + vec2(-r,  r) * px).rgb * 0.15;
    ink += texture2D(fbBuf, uv + vec2( r, -r) * px).rgb * 0.15;
    ink += texture2D(fbBuf, uv + vec2(-r, -r) * px).rgb * 0.15;

    // fine static paper tooth
    float grain = hash21(uv * RENDERSIZE.xy);
    ink += (grain - 0.5) * 0.035 * textureAmt;

    // hue micro-shift on highs; slow chromatic breath so silence never freezes
    ink = mix(ink, ink.gbr, 0.08 * gA * gHighP);
    ink = mix(ink, ink.brg, 0.025 + 0.025 * sin(gMclk * 0.07));

    float lift = 1.0 + 0.20 * gA * knee(audioLevel, 0.02, 0.8);
    ink *= brightness * lift;
    return vec4(max(ink, 0.0), 1.0);
}

void main() {
    gA     = clamp(audioReact, 0.0, 1.0);
    gBassP = pow(knee(audioBass, 0.03, 0.65), 1.4);
    gMidP  = pow(knee(audioMid,  0.04, 0.70), 1.3);
    gHighP = pow(knee(audioHigh, 0.05, 0.75), 1.2);
    // motion clock: baseline drift plus music-accumulated time, so movement
    // itself speeds and slows with the track (no discontinuities — audioTime
    // integrates host-side)
    gMclk  = TIME * moveSpeed + audioTime * 0.45 * gA * moveSpeed;

    if (PASSINDEX == 0) gl_FragColor = passField();
    else                gl_FragColor = passPresent();
}
