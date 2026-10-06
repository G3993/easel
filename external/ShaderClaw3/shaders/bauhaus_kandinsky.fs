/*{
  "CATEGORIES": [
    "Generator",
    "Art Movement",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Bauhaus after Kandinsky's Composition VIII (1923) and Several Circles (1926) — geometric SDF primitives floating on white, each shape strictly paired to its primary (yellow triangle, red square, blue circle). Lissajous orbits, weighted-circle gradient halos, thin black supporting lines.",
  "INPUTS": [
    {
      "NAME": "haloStrength",
      "LABEL": "Halo Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55
    },
    {
      "NAME": "strictPairing",
      "LABEL": "Strict Pairing",
      "TYPE": "bool",
      "DEFAULT": true
    },
    {
      "NAME": "compositionSeed",
      "LABEL": "Seed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 50,
      "DEFAULT": 0
    },
    {
      "NAME": "inputTex",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "shapeCount",
      "LABEL": "Shape Count",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 22,
      "DEFAULT": 13,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "shapeSize",
      "LABEL": "Shape Size",
      "TYPE": "float",
      "MIN": 0.04,
      "MAX": 0.2,
      "DEFAULT": 0.09,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "haloRadius",
      "LABEL": "Halo Radius",
      "TYPE": "float",
      "MIN": 1.2,
      "MAX": 4,
      "DEFAULT": 2.2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "lineCount",
      "LABEL": "Support Lines",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 16,
      "DEFAULT": 8,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "lineWidth",
      "LABEL": "Line Width",
      "TYPE": "float",
      "MIN": 0.0008,
      "MAX": 0.008,
      "DEFAULT": 0.0028,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "patternBands",
      "LABEL": "Pattern Bands",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6,
      "DEFAULT": 3,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "orbitSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.25,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "orbitRange",
      "LABEL": "Orbit Range",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0.22,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "lineMotion",
      "LABEL": "Line Motion",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.45,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "patternFlow",
      "LABEL": "Pattern Flow",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "kandinskyWork",
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
        "Composition VIII (1923)",
        "Several Circles (1926)",
        "Yellow Red Blue (1925)",
        "Composition X (1939)",
        "On White II (1923)"
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "useTexPalette",
      "LABEL": "Sample Tex for Palette",
      "TYPE": "bool",
      "DEFAULT": false,
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
      "NAME": "springReact",
      "LABEL": "Bass Spring",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.4,
      "DEFAULT": 0.12,
      "GROUP": "Audio Reactivity"
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

// Kandinsky's Bauhaus pedagogy: yellow=triangle, red=square, blue=circle.
// We instantiate N shape SDFs with Lissajous-driven centres and union
// them via min(distance) / closer-wins compositing. The "weighted
// circle" gradient is the Several Circles halo.

float hash11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }

float sdCircle(vec2 p, float r) { return length(p) - r; }

// Centred axis-aligned square SDF
float sdBox(vec2 p, float r) {
    vec2 d = abs(p) - r;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

// Equilateral triangle pointing up
float sdTriangle(vec2 p, float r) {
    const float k = 1.7320508;  // sqrt(3)
    p.x = abs(p.x) - r;
    p.y = p.y + r / k;
    if (p.x + k * p.y > 0.0) p = vec2(p.x - k * p.y, -k * p.x - p.y) / 2.0;
    p.x -= clamp(p.x, -2.0 * r, 0.0);
    return -length(p) * sign(p.y);
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    vec2 P = vec2(uv.x * aspect, uv.y);

    // Per-painting background — Composition VIII / Several Circles
    // (black) / Yellow-Red-Blue (warm white) / Composition X (black) /
    // On White II (pure white).
    int kw = int(kandinskyWork);
    vec3 col;
    if      (kw == 1) col = vec3(0.06, 0.06, 0.08);  // Several Circles black
    else if (kw == 3) col = vec3(0.04, 0.04, 0.05);  // Composition X black
    else if (kw == 4) col = vec3(0.97, 0.97, 0.97);  // On White II
    else if (kw == 2) col = vec3(0.95, 0.92, 0.84);  // Yellow Red Blue warm
    else              col = vec3(0.97, 0.96, 0.92);  // Composition VIII default

    // User background: blend the canvas ground toward the chosen color.
    col = mix(col, bgColor.rgb, bgColor.a);

    // Palette — strict pairing or input-derived.
    vec3 yellowC = vec3(0.98, 0.85, 0.10);
    vec3 redC    = vec3(0.89, 0.12, 0.14);
    vec3 blueC   = vec3(0.10, 0.18, 0.65);
    if (useTexPalette && IMG_SIZE_inputTex.x > 0.0) {
        yellowC = texture(inputTex, vec2(0.2, 0.5)).rgb;
        redC    = texture(inputTex, vec2(0.5, 0.5)).rgb;
        blueC   = texture(inputTex, vec2(0.8, 0.5)).rgb;
    }

    int N = int(clamp(shapeCount, 1.0, 22.0));

    // Pass 1 — halo / weighted-circle gradient field. Each shape casts a
    // soft glow whose radius scales with the shape's size, and where
    // halos overlap they additively brighten — Several Circles vibe.
    float haloField = 0.0;
    vec3  haloCol   = vec3(0.0);
    float haloWt    = 0.0;
    for (int i = 0; i < 22; i++) {
        if (i >= N) break;
        float fi = float(i) + compositionSeed * 1.71;
        // Lissajous orbit
        // Orbital home itself walks slowly so the canvas is never the
        // same N positions rotating in fixed circles.
        vec2 home = vec2(0.5 + (hash11(fi * 1.3) - 0.5) * 1.4
                             + 0.10 * sin(TIME * 0.07 + fi),
                         0.5 + (hash11(fi * 2.7) - 0.5) * 1.4
                             + 0.10 * cos(TIME * 0.05 + fi * 1.3));
        float a = TIME * orbitSpeed
                * (0.4 + hash11(fi * 3.1) * 1.4);
        vec2  orbit = vec2(sin(a + fi),
                           cos(a * 0.7 + fi * 1.7))
                    * orbitRange;
        // Bass spring — push outward briefly
        vec2 fromCtr = home + orbit - vec2(0.5);
        if (length(fromCtr) > 1e-4) fromCtr = normalize(fromCtr);
        vec2 ctr = home + orbit + fromCtr * springReact * audioBass * audioReact * 4.0;
        ctr.x *= aspect;

        float sz = shapeSize * (0.7 + hash11(fi * 5.3) * 0.6)
                 * (1.0 + audioLevel * audioReact * 0.75);
        float r  = length(P - ctr);
        float halo = exp(-pow(r / (sz * haloRadius), 2.0));
        haloField += halo;

        int t = int(mod(fi, 3.0));
        vec3 c = (t == 0) ? yellowC : (t == 1) ? redC : blueC;
        haloCol += c * halo;
        haloWt += halo;
    }
    if (haloWt > 1e-4) {
        haloCol /= haloWt;
        float haloAudio = 1.0 + audioLevel * audioReact * 1.6;
        col = mix(col, haloCol, clamp(haloField * haloStrength * 0.35 * haloAudio, 0.0, 1.0));
    }

    // Pass 2 — solid shape compositing. Closest-shape-wins via min over
    // signed distances; each shape uses its strict-paired colour.
    float bestSD = 1e9;
    vec3  bestCol = col;
    for (int i = 0; i < 22; i++) {
        if (i >= N) break;
        float fi = float(i) + compositionSeed * 1.71;
        // Orbital home itself walks slowly so the canvas is never the
        // same N positions rotating in fixed circles.
        vec2 home = vec2(0.5 + (hash11(fi * 1.3) - 0.5) * 1.4
                             + 0.10 * sin(TIME * 0.07 + fi),
                         0.5 + (hash11(fi * 2.7) - 0.5) * 1.4
                             + 0.10 * cos(TIME * 0.05 + fi * 1.3));
        float a = TIME * orbitSpeed
                * (0.4 + hash11(fi * 3.1) * 1.4);
        vec2  orbit = vec2(sin(a + fi),
                           cos(a * 0.7 + fi * 1.7))
                    * orbitRange;
        vec2 fromCtr = home + orbit - vec2(0.5);
        if (length(fromCtr) > 1e-4) fromCtr = normalize(fromCtr);
        vec2 ctr = home + orbit + fromCtr * springReact * audioBass * audioReact * 4.0;
        ctr.x *= aspect;

        float sz = shapeSize * (0.7 + hash11(fi * 5.3) * 0.6)
                 * (1.0 + audioLevel * audioReact * 0.75);

        // Per-shape rotation
        float rot = TIME * audioMid * audioReact * 0.9
                  + hash11(fi * 7.7) * 6.28;
        float ca = cos(-rot), sa = sin(-rot);
        vec2 lp = vec2(ca * (P.x - ctr.x) - sa * (P.y - ctr.y),
                       sa * (P.x - ctr.x) + ca * (P.y - ctr.y));

        int t = int(mod(fi, 3.0));
        if (!strictPairing) t = int(mod(fi * 3.7, 3.0));

        float sd = (t == 0) ? sdTriangle(lp, sz)
                : (t == 1) ? sdBox(lp, sz)
                : sdCircle(lp, sz);

        if (sd < bestSD) {
            bestSD = sd;
            // Every 4th shape gets a checkerboard interior — Composition
            // VIII signature pattern (chess-grid fills inside primitives).
            vec2  chk    = floor(lp / max(sz * 0.32, 1e-4));
            bool  chkOn  = (mod(chk.x + chk.y, 2.0) < 1.0);
            bool  useChk = (mod(fi, 4.0) >= 3.0);
            vec3  baseC  = (t == 0) ? yellowC : (t == 1) ? redC : blueC;
            vec3  altC   = vec3(0.05, 0.05, 0.06);
            bestCol = useChk ? (chkOn ? baseC : altC) : baseC;
        }
    }
    float fillMask = 1.0 - smoothstep(0.0, 0.0025, bestSD);
    col = mix(col, bestCol, fillMask);

    // Pass 3 — thin black support lines — diagonal scaffolds across
    // the canvas, characteristic of Composition VIII. Each line drifts.
    int NL = int(clamp(lineCount, 0.0, 16.0));
    for (int k = 0; k < 16; k++) {
        if (k >= NL) break;
        float fk = float(k) + compositionSeed * 0.71;
        float angle = hash11(fk * 1.7) * 6.2832
                    + sin(TIME * lineMotion * 0.5 + fk * 1.3) * 0.4;
        vec2 dir = vec2(cos(angle), sin(angle));
        vec2 pt  = vec2(hash11(fk * 3.3), hash11(fk * 5.1));
        pt += vec2(sin(TIME * lineMotion + fk),
                   cos(TIME * lineMotion * 0.8 + fk * 1.7)) * 0.04;
        vec2 d = uv - pt;
        float perp = abs(d.x * (-dir.y) + d.y * dir.x);
        float lw = lineWidth * (0.7 + hash11(fk * 7.13) * 0.7);
        float lm = smoothstep(lw, 0.0, perp);
        // Some lines are dashed (variety)
        if (hash11(fk * 11.7) > 0.6) {
            float along = d.x * dir.x + d.y * dir.y;
            float dash = step(0.5, fract(along * 28.0 + TIME * lineMotion * 0.5));
            lm *= dash;
        }
        col = mix(col, vec3(0.05, 0.05, 0.06),
                  lm * (0.6 + audioHigh * audioReact * 0.8));
    }

    // Pattern bands — thin parallel stripes filling negative space; flow
    // adds movement within the patterns themselves.
    if (patternBands > 0.001) {
        int PB = int(clamp(patternBands, 0.0, 6.0));
        for (int pb = 0; pb < 6; pb++) {
            if (pb >= PB) break;
            float fpb = float(pb);
            float pbAng = hash11(fpb * 13.7) * 6.2832 + TIME * patternFlow * 0.3;
            vec2 pbDir = vec2(cos(pbAng), sin(pbAng));
            vec2 pbCtr = vec2(hash11(fpb * 17.3), hash11(fpb * 19.1));
            float pbW = 0.06 + hash11(fpb * 23.7) * 0.10;
            vec2 dRel = uv - pbCtr;
            float along = dRel.x * pbDir.x + dRel.y * pbDir.y;
            float across = abs(dRel.x * (-pbDir.y) + dRel.y * pbDir.x);
            if (across < pbW) {
                float stripe = sin(along * 60.0 + TIME * patternFlow * 2.0) * 0.5 + 0.5;
                float band = step(0.6, stripe);
                float fade = smoothstep(pbW, pbW * 0.5, across);
                col = mix(col, vec3(0.06, 0.06, 0.08), band * fade * 0.45);
            }
        }
    }

    // Surprise: occasionally a Malevich Black Square appears at one of
    // the four corners — a quiet historical wink, ~0.8s every ~35s.
    {
        float _ph = fract(TIME / 35.0);
        float _f  = smoothstep(0.0, 0.04, _ph) * smoothstep(0.18, 0.10, _ph);
        float _which = floor(fract(TIME / 35.0 + 0.5) * 4.0);
        vec2 _suv = gl_FragCoord.xy / RENDERSIZE;
        vec2 _origin = vec2(_which < 2.0 ? 0.06 : 0.74,
                            (mod(_which, 2.0) < 1.0) ? 0.06 : 0.74);
        vec2 _d = _suv - _origin;
        float _sq = step(0.0, _d.x) * step(_d.x, 0.20) * step(0.0, _d.y) * step(_d.y, 0.20);
        col = mix(col, vec3(0.02), _f * _sq);
    }

    // Whole-canvas breathing pulse — the entire composition dips gently
    // with the beat (a soft "duck" rather than a garish brighten), so the
    // painting reads as alive to audio even where no shape/halo sits.
    col *= 1.0 - 0.16 * clamp(audioLevel, 0.0, 1.0) * min(audioReact, 2.0);

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

    gl_FragColor = vec4(col, 1.0);
}
