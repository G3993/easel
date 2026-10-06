/*{
  "DESCRIPTION": "Recursive black hole — fractal tiled glow rings with cosine palette, depth iterations, and audio reactivity",
  "CATEGORIES": [
    "Generator"
  ],
  "INPUTS": [
    {
      "NAME": "glowIntensity",
      "LABEL": "Glow",
      "TYPE": "float",
      "DEFAULT": 0.02,
      "MIN": 0.005,
      "MAX": 0.1
    },
    {
      "NAME": "iterations",
      "LABEL": "Depth",
      "TYPE": "float",
      "DEFAULT": 4,
      "MIN": 1,
      "MAX": 8,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "ringScale",
      "LABEL": "Ring Scale",
      "TYPE": "float",
      "DEFAULT": 1.5,
      "MIN": 0.5,
      "MAX": 4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "ringSpeed",
      "LABEL": "Ring Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorSpeed",
      "LABEL": "Color Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "warp",
      "LABEL": "Warp",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
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
      "NAME": "palette",
      "LABEL": "Palette",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Neon",
        "Fire",
        "Ice",
        "Acid"
      ],
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
      "NAME": "zoom",
      "LABEL": "Zoom",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.3,
      "MAX": 5,
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
    }
  ]
}*/

// Cosine palette — art-grade color from distance + time
// color = a + b * cos(2π * (c * t + d))
vec3 cosPalette(float t, vec3 a, vec3 b, vec3 c, vec3 d) {
    return a + b * cos(6.28318 * (c * t + d));
}

vec3 getPalette(float t, int pal) {
    // Neon (default) — cyan/magenta/yellow
    if (pal == 1) {
        // Fire — red/orange/gold
        return cosPalette(t,
            vec3(0.5, 0.3, 0.1),
            vec3(0.5, 0.4, 0.2),
            vec3(1.0, 0.7, 0.4),
            vec3(0.0, 0.15, 0.20));
    }
    if (pal == 2) {
        // Ice — blue/white/cyan
        return cosPalette(t,
            vec3(0.4, 0.5, 0.7),
            vec3(0.3, 0.3, 0.3),
            vec3(1.0, 1.0, 1.0),
            vec3(0.0, 0.1, 0.2));
    }
    if (pal == 3) {
        // Acid — green/yellow/purple
        return cosPalette(t,
            vec3(0.5, 0.5, 0.2),
            vec3(0.5, 0.5, 0.5),
            vec3(1.0, 1.0, 0.5),
            vec3(0.8, 0.9, 0.3));
    }
    // Neon
    return cosPalette(t,
        vec3(0.5, 0.5, 0.5),
        vec3(0.5, 0.5, 0.5),
        vec3(1.0, 1.0, 1.0),
        vec3(0.263, 0.416, 0.557));
}

void main() {
    // Coordinate normalization — centered, aspect-corrected
    vec2 uv = (gl_FragCoord.xy * 2.0 - RENDERSIZE.xy) / RENDERSIZE.y;
    vec2 uv0 = uv; // save original for color indexing
    uv *= zoom;

    // Mouse influence — warp UV toward/away from mouse
    vec2 mp = (mousePos - 0.5) * 2.0;
    mp.x *= RENDERSIZE.x / RENDERSIZE.y;

    // Soft-knee audio conditioning (playbook law 6): audioLevel pegs near
    // constant under sustained music, so follow per-band envelopes instead.
    float bassP = pow(smoothstep(0.04, 0.90, audioBass), 1.3);
    float midP  = pow(smoothstep(0.05, 0.90, audioMid),  1.2);
    float highP = pow(smoothstep(0.08, 0.90, audioHigh), 1.2);

    // Mids push the ring phase (bounded smooth offset — visible inward motion).
    float t = TIME * ringSpeed + midP * 1.2;
    int iters = int(iterations);
    int pal = int(palette);

    vec3 finalColor = vec3(0.0);

    // ── Recursive iteration — each pass adds layered depth ──
    for (int i = 0; i < 8; i++) {
        if (i >= iters) break;
        float fi = float(i);

        // Fractal tiling — fract creates infinite grid, -0.5 centers each cell
        uv = fract(uv * ringScale) - 0.5;

        // Mouse warp — subtle push per iteration
        uv += mp * warp * 0.05 / (fi + 1.0);

        // Distance from cell center — the circle shape
        float d = length(uv);

        // "Black hole" effect: add time to distance inside sin()
        // Makes rings appear to be sucked into the center
        // Bass breathes the ring frequency — rings visibly widen on kicks.
        d = sin(d * (8.0 - 2.0 * bassP) + t + fi * 0.5) / 8.0;
        d = abs(d);

        float audioPhase = audioMid * 0.3;

        // Cosine palette color — driven by original UV distance + time + iteration
        float colorIndex = length(uv0) + fi * 0.4 + TIME * colorSpeed * 0.2 + hueShift + audioPhase;
        vec3 col = getPalette(colorIndex, pal);

        // Glow effect — small value / distance = bright neon glow
        // Closer to ring = brighter, falls off with distance
        float glow = glowIntensity / d;

        // Audio boost to glow — knee'd bands keep headroom so the response
        // still breathes at EDM levels (raw audioLevel*2 saturated flat).
        glow *= 1.0 + 0.9 * bassP + 0.5 * highP;

        // Pow for sharper, more defined rings (less muddy)
        glow = pow(glow, 1.2);

        // Accumulate — each iteration adds its own colored glow layer
        finalColor += col * glow;
    }

    // Tone mapping — prevent blowout while keeping the neon punch
    finalColor = finalColor / (1.0 + finalColor);

    // The strike: in silence, the old ~7s timer flash survives untouched;
    // under music the timer yields to beat-driven strikes (audioBeatPulse
    // already decays, capped depth) so flashes correlate with the track
    // instead of polluting it.
    {
        float _ph = fract(TIME / 7.0);
        float _flash = smoothstep(0.0, 0.005, _ph) * smoothstep(0.08, 0.04, _ph);
        float _music = smoothstep(0.05, 0.6, audioEnergy);
        _flash *= 1.0 - 0.9 * _music;
        _flash = max(_flash, audioBeatPulse * audioBeatPulse * 0.35);
        finalColor = mix(finalColor, vec3(1.0), clamp(_flash, 0.0, 1.0) * 0.95);
    }

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(finalColor, vec3(0.299, 0.587, 0.114));
    finalColor = mix(vec3(ucL), finalColor, colorBoost);
    finalColor = mix(finalColor, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    gl_FragColor = vec4(finalColor, 1.0);
}
