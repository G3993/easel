/*{
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Sound made literally visible — concentric ripples rolling across stacked depth-planes, audio frequencies sculpting cymatic interference patterns in 3D space. Pure black background edition.",
  "INPUTS": [
    {
      "NAME": "refraction",
      "LABEL": "Refraction",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.08,
      "DEFAULT": 0.02
    },
    {
      "NAME": "bloomStr",
      "LABEL": "Bloom Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "inputTex",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "texMix",
      "LABEL": "Texture Mix",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "layers",
      "LABEL": "Layers",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 6,
      "DEFAULT": 6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "freqScale",
      "LABEL": "Frequency Scale",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 40,
      "DEFAULT": 16,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 1.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "idleAmp",
      "LABEL": "Idle Amplitude",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0.15,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "sourceWander",
      "LABEL": "Source Wander",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 5,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "wanderRate",
      "LABEL": "Wander Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 5,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "depthSpeedSpread",
      "LABEL": "Depth Speed Spread",
      "TYPE": "float",
      "MIN": -1.5,
      "MAX": 1.5,
      "DEFAULT": 0,
      "GROUP": "Motion"
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
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "parallax",
      "LABEL": "Parallax",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.3,
      "DEFAULT": 0.08,
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

float hash(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

vec2 hashPos(int L) {
    float f = float(L);
    return vec2(hash(vec2(f, 1.7)), hash(vec2(f, 9.3))) - 0.5;
}

vec3 planeColor(int L, float layerCount) {
    float t = fract(float(L) / max(layerCount - 1.0, 1.0) + TIME * 0.12);
    // Saturated neon palette — stays vivid on pure black
    vec3 c = 0.5 + 0.5 * cos(6.2832 * (vec3(0.0, 0.33, 0.67) + t));
    // Boost saturation by pulling toward max component
    float mx = max(c.r, max(c.g, c.b));
    c = mix(c, vec3(mx), -0.3);
    return clamp(c, 0.0, 1.0);
}

// Soft additive glow kernel (approx bloom via radial samples)
vec3 addGlow(vec3 col, float h, vec3 glowCol, float str) {
    float g = exp(-abs(h) * 6.0) * str;
    return col + glowCol * g * 0.6;
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 p  = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y;

    vec3 col    = vec3(0.0); // TRUE BLACK base
    float totalH = 0.0;
    float totalW = 0.0;

    for (int L = 0; L < 6; L++) {
        if (float(L) >= layers) break;
        float depth = float(L) / max(layers, 1.0);

        // Parallax offset (no mousePos — use slight TIME drift instead)
        vec2 drift = vec2(sin(TIME * 0.07 + float(L) * 0.9), cos(TIME * 0.05 + float(L) * 1.3)) * 0.04;
        vec2 pp = p + drift * parallax * (1.0 - depth);

        // Frequency bin: bass at back, treble at front
        float bin = mix(0.05, 0.6, 1.0 - depth);
        float fftVal = texture(audioFFT, vec2(bin, 0.5)).r;
        float amp = fftVal + idleAmp;

        // Animated source position per layer
        vec2 src = hashPos(L)
            + vec2(
                sin(TIME * 0.3 * wanderRate + float(L) * 1.7),
                cos(TIME * 0.2 * wanderRate + float(L) * 2.3)
              ) * (0.08 * sourceWander + audioBass * 0.12);

        float dist = length(pp - src);
        float fq   = freqScale * (0.6 + depth * 0.8);
        // Depth Speed Spread: front vs back layers ripple at diverging rates
        float spdL = speed * (1.0 + depthSpeedSpread * (depth - 0.5));
        float h    = sin(dist * fq - TIME * spdL) * amp;

        // Second harmonic adds interference complexity
        float dist2 = length(pp - src * vec2(-0.7, 0.9));
        float h2    = sin(dist2 * fq * 1.618 - TIME * spdL * 1.3) * amp * 0.4;
        float hCombined = h + h2;

        float layerWeight = 1.0 - depth;
        totalH += hCombined * layerWeight;
        totalW += layerWeight;

        // --- Per-layer colour: pure additive on black ---
        vec3 lc = planeColor(L, layers);

        // Ripple modulation: dark troughs stay pure black, bright crests glow.
        // hCombined's natural peak tracks amp (~amp*1.4), and amp itself is
        // small (idleAmp default 0.15 + a modest FFT bin) — a raw max(0,
        // hCombined) never gets anywhere near 1.0, so the crests barely lit
        // up at all (the first attempt at "true black" over-corrected into
        // "barely visible"). Normalize against amp's own peak so a crest
        // reaches full brightness regardless of how quiet amp is, while
        // hCombined<=0 still stays exactly 0 (pure black between ripples).
        float crestPeak = max(amp * 1.4, 0.001);
        float brightness = clamp(max(0.0, hCombined) / crestPeak, 0.0, 1.0);
        brightness = pow(brightness, 0.8); // gentle lift so mid-crests read, not just tips
        lc *= brightness * (0.6 + amp * 2.0);

        // Depth attenuation — back layers dimmer, no fog colour added
        lc *= (1.0 - depth * 0.55);

        // Additive bloom halo on strong ridges
        lc = addGlow(lc, hCombined, planeColor(L, layers), bloomStr * amp);

        // Additive composite — black stays black where waves are zero
        col += lc / max(layers, 1.0);
    }

    float normH = (totalW > 0.0) ? totalH / totalW : 0.0;

    // Refract live video through integrated wave height (optional input).
    // IMG_SIZE() reports the canvas size on this engine, not whether an
    // image is actually connected — it's always > 0, so it can't gate this.
    // Blend only when the user explicitly dials texMix up; otherwise the
    // background stays the pure-black additive composite from above.
    if (texMix > 0.001) {
        vec2 refractUV = uv + vec2(normH * refraction, normH * refraction * 0.7);
        vec3 t = IMG_NORM_PIXEL(inputTex, clamp(refractUV, 0.0, 1.0)).rgb;
        col = mix(col, t * (0.5 + 0.5 * abs(normH)), texMix);
    }

    // Contour edge lines — pure white/cyan/orange lines on black
    float cBand  = abs(fract(normH * 2.0 + 0.5) - 0.5) * 2.0;
    float contour = 1.0 - smoothstep(0.0, 0.04, cBand);
    col += contour * vec3(0.4, 0.85, 1.0) * 1.4 * bloomStr;

    float cBand2  = abs(fract(normH * 5.5 + 0.5) - 0.5) * 2.0;
    float contour2 = 1.0 - smoothstep(0.0, 0.03, cBand2);
    col += contour2 * vec3(1.0, 0.55, 0.15) * 0.7 * bloomStr;

    // Subtle specular flash at wave peaks driven by audio level
    float specular = pow(max(0.0, normH), 4.0) * audioLevel * 1.5;
    col += vec3(specular * 0.9, specular * 0.95, specular);

    // LUT snap — discrete palette steps, preserves vividness
    col = mix(col, floor(col * 7.0 + 0.5) / 7.0, 0.35);

    // Tone-map (filmic) so over-driven additive blends stay bold but not blown
    col = col / (col + vec3(0.75));
    col = pow(col, vec3(0.88)); // slight gamma lift for screen display

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        col = clamp(hM * col, 0.0, 1.0);
    }
    col = mix(col, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    gl_FragColor = vec4(col, 1.0);
}