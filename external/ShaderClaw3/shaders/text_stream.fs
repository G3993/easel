/*{
  "DESCRIPTION": "Stream — flowing ribbons of text drift like jellyfish tentacles. Many vertical character columns sway side-to-side at different frequencies and speeds, characters fall along each ribbon, tops & bottoms taper softly. Lots of text on screen at once. Bass deepens the sway, treble lights up individual character peaks. Bind a Background Layer for any backdrop.",
  "CREDIT": "ShaderClaw",
  "CATEGORIES": [
    "Generator",
    "Text",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "ribbonCount",
      "LABEL": "Ribbons",
      "TYPE": "long",
      "DEFAULT": 11,
      "VALUES": [
        4,
        6,
        8,
        10,
        11,
        12,
        14,
        16,
        20,
        24
      ],
      "LABELS": [
        "4",
        "6",
        "8",
        "10",
        "11",
        "12",
        "14",
        "16",
        "20",
        "24"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "ribbonThickness",
      "LABEL": "Ribbon Width",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0.2,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "ribbonJitter",
      "LABEL": "Ribbon Variance",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "tipFade",
      "LABEL": "Tip Fade",
      "TYPE": "float",
      "DEFAULT": 0.18,
      "MIN": 0,
      "MAX": 0.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "fallSpeed",
      "LABEL": "Fall Speed",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "swayAmp",
      "LABEL": "Sway Amplitude",
      "TYPE": "float",
      "DEFAULT": 0.06,
      "MIN": 0,
      "MAX": 0.18,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "swayFreq",
      "LABEL": "Sway Frequency",
      "TYPE": "float",
      "DEFAULT": 1.7,
      "MIN": 0,
      "MAX": 8,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "textA",
      "LABEL": "Tip Color",
      "TYPE": "color",
      "DEFAULT": [
        1,
        1,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "textB",
      "LABEL": "Mid Color",
      "TYPE": "color",
      "DEFAULT": [
        0.2,
        0.95,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "textC",
      "LABEL": "Tail Color",
      "TYPE": "color",
      "DEFAULT": [
        0.6,
        0.2,
        1,
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
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "STREAM RIBBONS DRIFT THROUGH THE DEEP",
      "MAX_LENGTH": 48,
      "GROUP": "Text"
    },
    {
      "NAME": "fontFamily",
      "LABEL": "Font",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Inter",
        "Times",
        "Caslon",
        "Outfit"
      ],
      "GROUP": "Text"
    },
    {
      "NAME": "textScale",
      "LABEL": "Text Size",
      "TYPE": "float",
      "DEFAULT": 0.05,
      "MIN": 0.02,
      "MAX": 0.12,
      "GROUP": "Text"
    },
    {
      "NAME": "kerning",
      "LABEL": "Vertical Spacing",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.6,
      "MAX": 1.8,
      "GROUP": "Text"
    },
    {
      "NAME": "inputTex",
      "LABEL": "Background Layer",
      "TYPE": "image",
      "GROUP": "Background"
    },
    {
      "NAME": "bgOpacity",
      "LABEL": "BG Layer Opacity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Background"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0.01,
        0.02,
        0.05,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent BG",
      "TYPE": "bool",
      "DEFAULT": 1,
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "haloStrength",
      "LABEL": "Halo",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "hdrBoost",
      "LABEL": "HDR Boost",
      "TYPE": "float",
      "DEFAULT": 1.8,
      "MIN": 1,
      "MAX": 3.5
    }
  ]
}*/

// =====================================================================
// Stream — vertical text ribbons that sway like jellyfish tentacles.
// Each ribbon is a column of characters falling along a sine-wave path.
// The whole field sways with audio bass; individual characters flicker
// with treble. Top + bottom taper softly. LINEAR HDR.
// =====================================================================

#define MAX_RIBBONS 24

// ─── Font atlas ─────────────────────────────────────────────────────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}

// Accept both atlas indices (0-36, fed by the app) and raw ASCII codes
// (fed by some hosts): map ASCII letters/digits/space onto atlas indices.
int normChar(int c) {
    if (c >= 65 && c <= 90) return c - 65;      // ASCII 'A'-'Z'
    if (c >= 97 && c <= 122) return c - 97;     // ASCII 'a'-'z'
    if (c == 32) return 26;                     // ASCII space
    if (c >= 48 && c <= 57) return c - 48 + 27; // ASCII '0'-'9'
    return c;                                    // already an atlas index
}

int getChar(int slot) {
    if (slot ==  0) return int(msg_0);
    if (slot ==  1) return int(msg_1);
    if (slot ==  2) return int(msg_2);
    if (slot ==  3) return int(msg_3);
    if (slot ==  4) return int(msg_4);
    if (slot ==  5) return int(msg_5);
    if (slot ==  6) return int(msg_6);
    if (slot ==  7) return int(msg_7);
    if (slot ==  8) return int(msg_8);
    if (slot ==  9) return int(msg_9);
    if (slot == 10) return int(msg_10);
    if (slot == 11) return int(msg_11);
    if (slot == 12) return int(msg_12);
    if (slot == 13) return int(msg_13);
    if (slot == 14) return int(msg_14);
    if (slot == 15) return int(msg_15);
    if (slot == 16) return int(msg_16);
    if (slot == 17) return int(msg_17);
    if (slot == 18) return int(msg_18);
    if (slot == 19) return int(msg_19);
    if (slot == 20) return int(msg_20);
    if (slot == 21) return int(msg_21);
    if (slot == 22) return int(msg_22);
    if (slot == 23) return int(msg_23);
    if (slot == 24) return int(msg_24);
    if (slot == 25) return int(msg_25);
    if (slot == 26) return int(msg_26);
    if (slot == 27) return int(msg_27);
    if (slot == 28) return int(msg_28);
    if (slot == 29) return int(msg_29);
    if (slot == 30) return int(msg_30);
    if (slot == 31) return int(msg_31);
    if (slot == 32) return int(msg_32);
    if (slot == 33) return int(msg_33);
    if (slot == 34) return int(msg_34);
    if (slot == 35) return int(msg_35);
    if (slot == 36) return int(msg_36);
    if (slot == 37) return int(msg_37);
    if (slot == 38) return int(msg_38);
    if (slot == 39) return int(msg_39);
    if (slot == 40) return int(msg_40);
    if (slot == 41) return int(msg_41);
    if (slot == 42) return int(msg_42);
    if (slot == 43) return int(msg_43);
    if (slot == 44) return int(msg_44);
    if (slot == 45) return int(msg_45);
    if (slot == 46) return int(msg_46);
    if (slot == 47) return int(msg_47);
    return -1;
}

int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 8;
    if (n > 48) return 48;
    return n;
}

float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }

// Vertical sine path of a ribbon: returns the x offset at this y.
// Time-shifted + per-ribbon phase + a low-frequency wandering term so
// ribbons don't all wave in lockstep.
float ribbonX(float baseX, float y, float amp, float freq, float phase, float t) {
    float w1 = sin(y * freq          + t * 0.9  + phase) * amp;
    float w2 = sin(y * freq * 0.43   + t * 0.45 + phase * 1.7) * amp * 0.55;
    return baseX + w1 + w2;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    // Centered, aspect-corrected. Y goes 0(bottom) → 1(top).
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y;

    float audio = clamp(audioReact, 0.0, 2.0);
    float bass  = audioBass;
    float treb  = audioHigh;

    // ─── Background ────────────────────────────────────────────────
    vec3 layerBG = IMG_NORM_PIXEL(inputTex, uv).rgb;
    vec3 col = mix(bgColor.rgb, layerBG, bgOpacity);

    int total      = charCount();
    int ribbons    = int(ribbonCount);
    if (ribbons <= 0) ribbons = 11;   // host left it unset — use the default
    if (ribbons > MAX_RIBBONS) ribbons = MAX_RIBBONS;
    float charH    = textScale;
    float charW    = charH * (5.0 / 7.0);
    float vStep    = charH * kerning;     // vertical char spacing
    float ftotal   = float(total);

    // The ribbon field spans roughly [-aspect/2, aspect/2] horizontally.
    // Ribbons are evenly distributed but jittered per-ribbon so they
    // don't read as a perfect grid.
    float ribbonSpread = aspect * 0.95;

    float textMask = 0.0;
    vec3  textCol  = vec3(0.0);
    float halo     = 0.0;

    // Bass deepens the global sway by stretching swayAmp.
    float effSway = swayAmp * (1.0 + 0.6 * bass * audio);

    for (int ri = 0; ri < MAX_RIBBONS; ri++) {
        if (ri >= ribbons) break;
        float fri = float(ri);

        // Per-ribbon randomness (deterministic seed).
        float seed1 = hash11(fri * 13.7 + 0.1);
        float seed2 = hash11(fri * 27.3 + 0.7);
        float seed3 = hash11(fri * 41.9 + 1.3);

        // Distributed base x with small jitter so ribbons don't form
        // straight columns.
        float t01    = (ribbons > 1) ? fri / float(ribbons - 1) : 0.5;
        float baseX  = mix(-ribbonSpread * 0.5, ribbonSpread * 0.5, t01)
                     + (seed1 - 0.5) * ribbonSpread / float(ribbons) * ribbonJitter * 1.5;

        // Per-ribbon sway amp and frequency variation.
        float ampR   = effSway * (0.55 + 1.0 * seed2);
        float freqR  = swayFreq * (0.7 + 0.7 * seed3);
        float phaseR = seed1 * 6.2832;
        // Per-ribbon vertical fall speed.
        float spdR   = fallSpeed * (0.7 + 0.7 * seed2);

        // Pixel x offset to ribbon center at this pixel's y.
        float rx     = ribbonX(baseX, p.y, ampR, freqR, phaseR, TIME);
        float dx     = p.x - rx;

        // Ribbon thickness check — anything outside this band can't be
        // text on this ribbon. Half-width = charW * 0.55 so ribbon
        // thickness is roughly one char wide.
        float bandHalf = charW * (0.55 + ribbonThickness * 0.15);
        if (abs(dx) > bandHalf) continue;

        // Vertical "cell coordinate" — characters stack at vStep along y,
        // scrolling downward over time so the column reads like falling
        // rain.
        float scrolled = (1.0 - p.y) + TIME * spdR;
        // Per-ribbon offset so messages don't all scroll in sync.
        scrolled += seed3 * 7.31;
        float cellF    = scrolled / vStep;
        int   slot     = int(floor(mod(cellF, ftotal)));
        if (slot < 0) slot += total;
        int   ch       = normChar(getChar(slot));

        // Cell-local UV: x within ribbon band → centered on char width.
        // y within current cell.
        vec2 cellUV;
        cellUV.x = dx / charW + 0.5;
        cellUV.y = fract(cellF);
        if (cellUV.x < 0.0 || cellUV.x > 1.0) continue;

        float s = sampleChar(ch, cellUV);
        s = smoothstep(0.18, 0.55, s);

        // Tip + tail soft fade so ribbons don't hard-cut at edges.
        float tipFadeT = smoothstep(0.0, tipFade, p.y) *
                        (1.0 - smoothstep(1.0 - tipFade, 1.0, p.y));
        // Bottom-up gradient: brighter at the leading edge ("head") of
        // each character batch — adds a sense of head/tail like a comet.
        float headBoost = smoothstep(0.55, 1.0, fract(cellF));

        // Per-ribbon depth/parallax: ribbons that sway more (closer to
        // viewer) read brighter.
        float depthBoost = 0.65 + 0.55 * seed2;

        // Treble shimmer per char — random sparkle when bright treble.
        float sparkle = 1.0 + 0.45 * treb * audio
                      * step(0.93 - 0.05 * treb * audio, hash11(float(slot) * 11.7 + fri));

        // 3-stop color along the ribbon (head → mid → tail). Gives the
        // ribbon a depth-by-color feel like a tentacle gradient.
        float headT = clamp(headBoost, 0.0, 1.0);
        float tailT = clamp(1.0 - p.y - 0.1, 0.0, 1.0);
        vec3  baseColor = mix(textC.rgb, textB.rgb, smoothstep(0.0, 0.6, p.y));
        baseColor = mix(baseColor, textA.rgb, headT * 0.85);

        float w = s * tipFadeT * depthBoost * sparkle;
        if (w < 0.001) continue;

        textMask = max(textMask, w);
        textCol  = mix(textCol, baseColor, w);

        // Soft halo around each lit char.
        float dlen = length(vec2(dx, (cellUV.y - 0.5) * charH));
        halo += exp(-pow(dlen / (charW * 1.1), 2.0)) * w * 0.3;
    }

    halo = clamp(halo, 0.0, 4.0);

    // Compose: backdrop → halo glow → ink chars on top.
    col += textCol * halo * haloStrength;
    // HDR ring for the bloom post-pass.
    float ring = clamp(halo - 0.4, 0.0, 1.5);
    col += textCol * pow(ring, 2.0) * hdrBoost * 0.4;

    // Solid characters layered on top.
    col = mix(col, textCol, textMask);

    float alpha = 1.0;
    if (transparentBg) {
        alpha = clamp(textMask + halo * 0.25, 0.0, 1.0);
        col   = textCol;
    }

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = col;
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
    col = uc;

    gl_FragColor = vec4(col, alpha);
}
