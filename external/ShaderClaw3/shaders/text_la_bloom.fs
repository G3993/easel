/*{
  "DESCRIPTION": "La Bloom — message characters arranged on a rose-curve flower silhouette that continuously blooms outward from the center. Multiple concurrent bloom waves at offset phases keep a new flower always opening while older ones fade at the rim. Toggle 3D for a tilted bowl-shaped bloom: petals lift toward the viewer with perspective scaling, depth-based haze, and rim-light HDR peaks. Saturated jewel palette, ink-black silhouettes against bloom haze.",
  "CREDIT": "ShaderClaw",
  "CATEGORIES": [
    "Generator",
    "Text",
    "Audio Reactive",
    "3D"
  ],
  "INPUTS": [
    {
      "NAME": "petals",
      "LABEL": "Petals",
      "TYPE": "long",
      "DEFAULT": 6,
      "VALUES": [
        3,
        4,
        5,
        6,
        7,
        8,
        10,
        12
      ],
      "LABELS": [
        "3",
        "4",
        "5",
        "6",
        "7",
        "8",
        "10",
        "12"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "waveCount",
      "LABEL": "Bloom Waves",
      "TYPE": "long",
      "DEFAULT": 3,
      "VALUES": [
        1,
        2,
        3,
        4,
        5,
        6
      ],
      "LABELS": [
        "1",
        "2",
        "3",
        "4",
        "5",
        "6"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "maxRadius",
      "LABEL": "Max Radius",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0.15,
      "MAX": 0.7,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "petalDepth",
      "LABEL": "Petal Depth",
      "TYPE": "float",
      "DEFAULT": 0.34,
      "MIN": 0,
      "MAX": 0.7,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "bowlDepth",
      "LABEL": "Bowl Depth (3D)",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1.4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "bloomSpeed",
      "LABEL": "Bloom Speed",
      "TYPE": "float",
      "DEFAULT": 0.18,
      "MIN": 0.02,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "spin",
      "LABEL": "Spin",
      "TYPE": "float",
      "DEFAULT": 0.12,
      "MIN": -1.5,
      "MAX": 1.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "inkColor",
      "LABEL": "Ink",
      "TYPE": "color",
      "DEFAULT": [
        0.02,
        0.01,
        0.03,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "petalA",
      "LABEL": "Petal Hot",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.32,
        0.55,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "petalB",
      "LABEL": "Petal Cool",
      "TYPE": "color",
      "DEFAULT": [
        0.95,
        0.78,
        0.2,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "centerColor",
      "LABEL": "Pistil",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.95,
        0.55,
        1
      ],
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
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "mode3D",
      "LABEL": "3D Mode",
      "TYPE": "bool",
      "DEFAULT": 0,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "camTilt",
      "LABEL": "Camera Tilt (3D)",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1.4,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "BLOOM ",
      "MAX_LENGTH": 48,
      "GROUP": "Text"
    },
    {
      "NAME": "fontFamily",
      "LABEL": "Font",
      "TYPE": "long",
      "DEFAULT": 3,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Inter",
        "Times New Roman",
        "Libre Caslon",
        "Outfit"
      ],
      "GROUP": "Text"
    },
    {
      "NAME": "textScale",
      "LABEL": "Text Size",
      "TYPE": "float",
      "DEFAULT": 0.07,
      "MIN": 0.02,
      "MAX": 0.18,
      "GROUP": "Text"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0.04,
        0.02,
        0.08,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": 1,
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "haloStrength",
      "LABEL": "Halo",
      "TYPE": "float",
      "DEFAULT": 1.6,
      "MIN": 0,
      "MAX": 4
    },
    {
      "NAME": "hdrBoost",
      "LABEL": "HDR Boost",
      "TYPE": "float",
      "DEFAULT": 2.4,
      "MIN": 1,
      "MAX": 4
    }
  ]
}*/

// ---- universal color block (defaults = no-op) ----
vec3 ucApply(vec3 uc) {
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                      // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    return uc;
}


// =====================================================================
// La Bloom — flower of text. Message characters orbit on a rose curve
// whose radius blooms outward from the center, looping. Multiple waves
// at offset phases keep at least one bloom always opening.
// Output: LINEAR HDR.
// =====================================================================

#define TAU 6.2831853

// ─── Font atlas sampling ────────────────────────────────────────────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
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
    return 26;
}

int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 6;
    if (n > 48) return 48;
    return n;
}

// ─── Hash + noise ───────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash11(dot(i, vec2(1.0, 157.0)));
    float b = hash11(dot(i + vec2(1.0, 0.0), vec2(1.0, 157.0)));
    float c = hash11(dot(i + vec2(0.0, 1.0), vec2(1.0, 157.0)));
    float d = hash11(dot(i + vec2(1.0, 1.0), vec2(1.0, 157.0)));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

// Polar petal radius modulator: rose curve r(theta) = 1 + d*cos(k*theta)
float petalShape(float theta, float k, float depth) {
    return 1.0 + depth * cos(k * theta);
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    // Centered, aspect-corrected coords. Center of canvas = (0,0).
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    float audio = clamp(audioReact, 0.0, 2.0);
    float bass  = audioBass;
    float treb  = audioHigh;
    float pulse = 1.0 + 0.55 * bass * audio;

    // ─── 1) Background — deep gradient + soft pistil glow at center ──
    vec3 col = bgColor.rgb;
    // Subtle radial vignette to push focus to center.
    float r = length(p);
    float vig = 1.0 - smoothstep(0.55, 0.95, r);
    col *= mix(0.55, 1.0, vig);

    // Pistil — a soft warm core that pulses with bass, sits behind petals.
    float pistilR  = 0.085 + 0.07 * bass * audio;
    float pistil   = exp(-pow(r / pistilR, 2.0));
    col += centerColor.rgb * pistil * 2.2 * pulse;
    // Halo around pistil that the bloom waves rise out of.
    float halo = exp(-pow(r / (pistilR * 4.5), 2.0));
    col += centerColor.rgb * halo * 0.45;

    // ─── 2) Bloom waves ──────────────────────────────────────────────
    int total      = charCount();
    // Long-typed inputs can arrive unset (0) on the mobile/eval path —
    // fall back to their documented defaults (app never sends 0).
    int petalsI    = int(petals);
    if (petalsI < 3) petalsI = 6;
    int waves      = int(waveCount);
    if (waves < 1) waves = 3;
    float fpetals  = float(petalsI);
    float fwaves   = float(waves);
    float ftotal   = float(total);
    float charH    = textScale;
    float charW    = charH * (5.0 / 7.0);

    float textMask = 0.0;
    float bloomGlow = 0.0;
    float petalTint = 0.0;  // 0..1 mix between petalA and petalB

    // Each wave has a phase in [0,1). At phase 0 the wave is at the
    // center; at phase 1 it's at the rim. We staggers waves so a new
    // bloom is always opening from the pistil while older ones fade.
    for (int w = 0; w < 6; w++) {
        if (w >= waves) break;
        float fw      = float(w);
        float phase   = mod(TIME * bloomSpeed + fw / fwaves, 1.0);
        // Bell envelope: bloom is brightest when half-grown.
        float env     = sin(phase * 3.14159);
        // Mid-burst HDR push when treble spikes.
        env = pow(env, 0.85) * (1.0 + 1.4 * treb * audio * (1.0 - phase));
        if (env < 0.01) continue;

        // Wave-specific spin so successive blooms aren't aligned.
        float waveSpin = TIME * spin + fw * 0.45;
        // Wave radius grows linearly outward.
        float waveR = phase * maxRadius * pulse;

        // Place each character on the petal silhouette at this wave's radius.
        for (int j = 0; j < 64; j++) {
            if (j >= total) break;
            int ch = getChar(j);
            if (ch < 0 || ch > 36) continue;

            float fj  = float(j);
            // Spread characters evenly around the bloom; offset every
            // wave so the same letter doesn't sit on the same petal.
            float t   = fj / ftotal;
            float ang = t * TAU + waveSpin;
            // Rose-curve modulation gives petaled silhouette.
            float rk  = petalShape(ang, fpetals, petalDepth);
            float rad = waveR * rk;

            vec2 charCenter = vec2(cos(ang), sin(ang)) * rad;

            // Local frame: tangent rotates the character so it faces
            // outward from center (bloom orientation).
            vec2 outward = vec2(cos(ang), sin(ang));
            vec2 tangent = vec2(-sin(ang), cos(ang));

            vec2 d   = p - charCenter;
            // Rotate into char-local space.
            float lx = dot(d, tangent);
            float ly = dot(d, outward);

            vec2 cellUV;
            cellUV.x = lx / charW + 0.5;
            cellUV.y = ly / charH + 0.5;

            float s = sampleChar(ch, cellUV);
            s = smoothstep(0.15, 0.55, s);
            textMask += s * env;

            // Soft halo around each char — pre-mult by env for HDR pop.
            float dlen   = length(d);
            float bloomR = charW * 1.6;
            float bg     = exp(-pow(dlen / bloomR, 2.0));
            bloomGlow   += bg * env * (0.6 + 0.4 * (1.0 - phase));

            // Petal tint blends along the radial position.
            petalTint += bg * env * t;
        }
    }

    textMask  = clamp(textMask,  0.0, 4.0);
    bloomGlow = clamp(bloomGlow, 0.0, 4.0);

    // Petal color — radial gradient from hot to cool.
    vec3 petalCol = mix(petalA.rgb, petalB.rgb, clamp(petalTint * 0.4, 0.0, 1.0));

    // ─── 3) Compose: bloom haze first, then ink chars on top ─────────
    // The bloom glow is the colored halo of light around the characters.
    col += petalCol * bloomGlow * haloStrength * hdrBoost * 0.5;
    // HDR peak ring: where bloom is brightest, push beyond 1.0 for the
    // bloom post-pass to catch.
    float ringPhase = clamp(bloomGlow - 0.5, 0.0, 1.5);
    col += petalCol * pow(ringPhase, 2.0) * hdrBoost;

    // Ink characters — solid silhouette so the petals read as "made of
    // text". Apply after the haze so the letters punch crisp negative
    // space against the glow.
    float charAlpha = clamp(textMask, 0.0, 1.0);
    col = mix(col, inkColor.rgb, charAlpha);

    // ─── 4) Outer-edge dust: small sparkles where waves dissolve ────
    if (treb > 0.02 && audio > 0.0) {
        float sparkN = vnoise(p * 22.0 + TIME * 0.3);
        float sparkMask = step(0.94 - 0.04 * treb * audio, sparkN);
        float edge = smoothstep(maxRadius * 0.7, maxRadius * 1.05, r);
        col += centerColor.rgb * sparkMask * edge * 1.2;
    }

    // ─── 5) Output ──────────────────────────────────────────────────
    float alpha = 1.0;
    if (transparentBg) {
        alpha = clamp(textMask + bloomGlow * 0.3, 0.0, 1.0);
        // For transparent mode skip the bg gradient.
        col = mix(petalCol * bloomGlow * haloStrength * hdrBoost * 0.5,
                  inkColor.rgb, charAlpha);
    }
    gl_FragColor = vec4(ucApply(col), alpha);
}
