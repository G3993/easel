/*{
  "CATEGORIES": [
    "Generator",
    "Audio Reactive",
    "Geometric"
  ],
  "DESCRIPTION": "Perfect rectangular constellation; every cell bound to one FFT bin, bass at left, treble at right, breathing in unison. Kraftwerk + Ryoji Ikeda data.matrix.",
  "INPUTS": [
    {
      "NAME": "decay",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 0.99,
      "DEFAULT": 0.9,
      "LABEL": "Decay"
    },
    {
      "NAME": "useTex",
      "TYPE": "bool",
      "DEFAULT": false,
      "LABEL": "Use Texture"
    },
    {
      "NAME": "inputTex",
      "TYPE": "image",
      "LABEL": "Texture"
    },
    {
      "NAME": "cols",
      "TYPE": "float",
      "MIN": 8,
      "MAX": 96,
      "DEFAULT": 48,
      "LABEL": "Columns",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "rows",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 48,
      "DEFAULT": 24,
      "LABEL": "Rows",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "cellRadius",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 0.5,
      "DEFAULT": 0.32,
      "LABEL": "Cell Radius",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "jitter",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.4,
      "DEFAULT": 0.05,
      "LABEL": "Jitter",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "lowColor",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.2,
        0.3,
        1
      ],
      "LABEL": "Low Color",
      "GROUP": "Color"
    },
    {
      "NAME": "highColor",
      "TYPE": "color",
      "DEFAULT": [
        0.2,
        0.8,
        1,
        1
      ],
      "LABEL": "High Color",
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

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 grid = vec2(max(8.0, cols), max(4.0, rows));
    vec2 cId = floor(uv * grid);
    vec2 cUV = fract(uv * grid) - 0.5;

    // Each cell column = one FFT bin slice. Skip top 5% (mostly noise).
    float bin = (cId.x + 0.5) / grid.x * 0.95;
    float amp = texture(audioFFT, vec2(bin, 0.5)).r;

    // Approximate the temporal-decay smoothing without persistent buffers:
    // we mix the raw FFT value with a subtle per-cell idle wobble so quiet
    // bins still breathe rather than going dead. The `decay` knob here
    // governs how much of the wobble is mixed in — high decay → smooth.
    float wobble = 0.5 + 0.5 * sin(TIME * 1.5 + hash(cId) * 6.2832);
    amp = mix(amp, max(amp, wobble * 0.15), 1.0 - decay);

    // Per-cell phase jitter so dots don't all pulse on the same frame.
    vec2 jit = (vec2(hash(cId), hash(cId + 1.7)) - 0.5) * jitter * (amp + 0.2);

    // Cell radius modulated by bin amplitude — the dot grows with sound.
    float r = cellRadius * (0.3 + amp * 1.5 + audioLevel * 0.2);
    float dotMask = smoothstep(r, r * 0.85, length(cUV - jit));

    // Colour: spectrum gradient unless a texture mosaic is requested.
    vec3 base;
    if (useTex && IMG_SIZE_inputTex.x > 0.0) {
        // Each cell samples live video at its center → frequency-aware mosaic.
        base = texture(inputTex, (cId + 0.5) / grid).rgb;
    } else {
        base = mix(lowColor.rgb, highColor.rgb, cId.x / grid.x);
    }

    // Per-cell rotation hash for slight visual life on tall columns.
    float rotPhase = hash(cId + 7.7) * 6.2832;
    float twinkle = 0.85 + 0.15 * sin(TIME * 2.0 + rotPhase);

    vec3 col = base * dotMask * (amp + 0.1 + audioLevel * 0.05) * twinkle;

    // Base bloom — bass column 0 gets an audioBass boost, treble end gets audioHigh.
    if (cId.x < 1.0) col += lowColor.rgb * audioBass * 0.4 * dotMask;
    if (cId.x > grid.x - 2.0) col += highColor.rgb * audioHigh * 0.4 * dotMask;

    // Surprise: every ~10s a diagonal cascade lights up like dominoes —
    // each cell flares with a slight delay along the canvas diagonal.
    {
        float _ph = fract(TIME / 10.0);
        float _wave = (cId.x + cId.y) / max(grid.x + grid.y, 1.0);
        float _front = smoothstep(0.0, 0.04, _ph - _wave * 0.30)
                     * smoothstep(_wave * 0.30 + 0.18, _wave * 0.30 + 0.10, _ph);
        col += vec3(1.0, 0.85, 0.55) * dotMask * _front * 0.7;
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

    gl_FragColor = vec4(uc, 1.0);
}
