/*{
  "CATEGORIES": [
    "Generator",
    "Geometric",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Dense black-and-white Op Art that breathes with the music. Switch between Riley waves, zigzags, checkers, diamonds, and wobbling stripes.",
  "INPUTS": [
    {
      "NAME": "contrast",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "LABEL": "Contrast"
    },
    {
      "NAME": "texDisplace",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.3,
      "DEFAULT": 0,
      "LABEL": "Texture Displace"
    },
    {
      "NAME": "inputTex",
      "TYPE": "image",
      "LABEL": "Texture"
    },
    {
      "NAME": "pattern",
      "LABEL": "Pattern",
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
        "Wave",
        "Zigzag",
        "Checker",
        "Diamond",
        "Stripe Wobble"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "freq",
      "TYPE": "float",
      "MIN": 10,
      "MAX": 160,
      "DEFAULT": 60,
      "LABEL": "Frequency",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "warpAmp",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0.12,
      "LABEL": "Warp Amplitude",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "xFreq",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 12,
      "DEFAULT": 3,
      "LABEL": "X Frequency",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "accentEvery",
      "TYPE": "float",
      "MIN": 2,
      "MAX": 20,
      "DEFAULT": 7,
      "LABEL": "Accent Every Nth",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "flow",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.4,
      "LABEL": "Flow Speed",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "accentColor",
      "TYPE": "color",
      "DEFAULT": [
        0.95,
        0.15,
        0.25,
        1
      ],
      "LABEL": "Accent Color",
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
    }
  ]
}*/

float hash(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// Curl-noise-ish secondary turbulence so warp isn't pure sin; gives the
// "swimming" feel that Riley's plates evoke.
float audioCurl(vec2 uv, float t) {
    return sin(uv.x * 5.0 + t) * cos(uv.y * 7.0 - t) * 0.5;
}

// Triangle wave in [-1,1] with period 2*PI — drop-in for sin() so the
// AA / accent / crest math downstream still works.
float triWave(float x) {
    float p = x * (1.0 / 6.2831853);
    return abs(fract(p) * 4.0 - 2.0) - 1.0;
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;

    // Optional luminance guide from inputTex — bends pattern around silhouettes.
    float guide = 0.0;
    if (texDisplace > 0.001 && IMG_SIZE_inputTex.x > 0.0) {
        vec3 t = texture(inputTex, uv).rgb;
        guide = (t.r + t.g + t.b) / 3.0 - 0.5;
    }

    // Primary warp: low-frequency sin in x. Bass pushes it harder.
    float warp = sin(uv.x * xFreq + TIME * flow) * warpAmp * (1.0 + audioBass * 4.0);
    warp += audioCurl(uv, TIME * flow) * audioMid * 0.45;
    warp += guide * texDisplace;

    float effectiveFreq = freq * (1.0 + audioHigh * 0.6 + audioBass * 0.2);

    // ---- pattern dispatch ---------------------------------------------------
    // Each branch produces:
    //   phase  — used for accent-stripe indexing (continuous coordinate)
    //   field  — value in [-1,1] feeding the AA smoothstep
    //   aaScale— derivative-based AA width, kept tight to preserve Op-Art bite
    int mode = int(pattern + 0.5);
    float phase;
    float field;
    float aaScale;

    if (mode == 1) {
        // ZIGZAG: triangle wave instead of sin — sharp peaks, harder optical
        // tension because the eye chases the corners.
        float y = uv.y + warp;
        phase = y * effectiveFreq;
        field = triWave(phase);
        aaScale = fwidth(phase) * 1.25 + 1e-4;
    } else if (mode == 2) {
        // CHECKER: large checkerboard. Op illusion comes from low frequency
        // and the warp twisting the grid lines.
        vec2 cuv = uv;
        cuv.x += warp * 0.5;
        cuv.y += sin(uv.x * xFreq * 0.7 + TIME * flow) * warpAmp * 0.5;
        // tile count derived from freq so the existing knob still matters
        float tiles = max(2.0, effectiveFreq * 0.18);
        vec2 g = cuv * vec2(tiles * (RENDERSIZE.x / RENDERSIZE.y), tiles);
        // Use the SDF of grid cell center as a smooth field
        vec2 f = fract(g) - 0.5;
        // checker sign: +1 white cell, -1 black cell
        float parity = mod(floor(g.x) + floor(g.y), 2.0) * 2.0 - 1.0;
        // distance to cell edge — keeps fwidth-AA semantics
        float edge = 0.5 - max(abs(f.x), abs(f.y));
        field = parity * sign(edge) * (1.0 - smoothstep(0.0, 0.02, abs(edge)) * 0.0);
        // Actually we want a hard parity field with AA at cell boundaries:
        field = parity;
        aaScale = fwidth(parity) * 1.25 + 1e-4;
        // phase used for accent indexing — diagonal stripe through the grid
        phase = (floor(g.x) + floor(g.y)) * 3.14159;
    } else if (mode == 3) {
        // DIAMOND: rotated square grid. abs(x)+abs(y) iso-lines give diamond
        // rings; warp bends them into a moiré.
        vec2 d = uv - 0.5;
        d.x *= RENDERSIZE.x / RENDERSIZE.y;
        d += vec2(warp, warp * 0.6);
        float r = abs(d.x) + abs(d.y);
        phase = r * effectiveFreq;
        field = sin(phase);
        aaScale = fwidth(phase) * 1.25 + 1e-4;
    } else if (mode == 4) {
        // STRIPE WOBBLE: vertical stripes whose x-position wobbles per row,
        // giving the page-bowing Riley illusion ("Cataract").
        float wob = sin(uv.y * xFreq * 2.0 + TIME * flow * 1.3) * warpAmp;
        wob += audioCurl(uv.yx, TIME * flow) * audioMid * 0.22;
        float x = uv.x + wob;
        // aspect-correct so stripes look uniform
        x *= RENDERSIZE.x / RENDERSIZE.y;
        phase = x * effectiveFreq;
        field = sin(phase);
        aaScale = fwidth(phase) * 1.25 + 1e-4;
    } else {
        // WAVE (default, original Riley behavior).
        float y = uv.y + warp;
        phase = y * effectiveFreq;
        field = sin(phase);
        aaScale = fwidth(phase) * 1.25 + 1e-4;
    }

    // Soft AA: derivative-based smoothstep so edges stay crisp at any scale
    // without shimmering. Op Art lives or dies on edge fidelity.
    float bw = smoothstep(-aaScale, aaScale, field);

    // Accent stripe substitution — every Nth dark band becomes accent colour.
    float idx = floor(phase / 3.14159);
    bool isAccent = mod(idx, max(2.0, accentEvery)) < 0.5;
    vec3 darkCol = isAccent ? accentColor.rgb : vec3(0.0);

    vec3 col = mix(darkCol, vec3(1.0), bw);

    // CONTRAST: 1.0 = full binary B/W (original Riley bite); lower values
    // pull the stripe field toward mid-gray for variety / softer Op variants.
    // Applied as a lerp from 0.5-gray so accent colour still reads.
    {
        float c = clamp(contrast, 0.0, 1.0);
        vec3 mid = mix(vec3(0.5), darkCol * 0.5 + vec3(0.5) * 0.5, 0.0);
        // mid stays neutral 0.5 so the eye gets a clean "half tone" at c=0
        col = mix(vec3(0.5), col, c);
    }

    // HDR PEAKS: only the very crest of each white stripe lifts above 1.0,
    // so Phase Q bloom kisses the ridge — never the whole white field.
    // Restraint: 1.2–1.4 max, gated tight on stripe maxima. Scales with
    // contrast so soft mode doesn't bloom unnaturally.
    float crest = smoothstep(0.85, 0.995, field) * bw;
    col += vec3(crest) * 0.3 * clamp(contrast, 0.0, 1.0);

    // Audio peak invert flash — tasteful, only at very high level.
    float flash = smoothstep(0.15, 0.6, audioLevel);
    col = mix(col, vec3(1.0) - col, flash * 0.85);

    // Surprise: every ~16s the wave frequency briefly doubles — for
    // ~0.5s the eye sees twice as many bands, optical-illusion judder.
    // Bridget Riley's whole game.
    {
        vec2 _suv = gl_FragCoord.xy / RENDERSIZE;
        float _ph = fract(TIME / 16.0);
        float _f  = smoothstep(0.0, 0.04, _ph) * smoothstep(0.18, 0.08, _ph);
        float _doubled = step(0.5, fract(_suv.y * 80.0)) * 2.0 - 1.0;
        col = mix(col, vec3(0.5 + 0.5 * _doubled), _f * 0.45 * clamp(contrast, 0.0, 1.0));
    }

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    vec3 uc = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hueA = hueShift * 6.2831853;
        float hueC = cos(hueA), hueS = sin(hueA);
        mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                  + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                  + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hueM * uc, 0.0, 1.0);
    }
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

    // LINEAR HDR output — no gamma encode here; downstream tone-mapper handles it.
    gl_FragColor = vec4(uc, 1.0);
}
