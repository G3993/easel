/*{
  "CATEGORIES": [
    "Generator",
    "Abstract",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Synesthete — layered domain-warped silk gradients crossed by ribbon interference. Mood follows the music: calm = slow silk, intense = electric moiré (audioEnergy morphs ribbon frequency and contrast). Stems map to layers: stemBass drives the warp depth, stemMelody the ribbon amplitude, stemAir the sheen. audioToggleOnBeat rotates the palette a subtle notch every beat. Layers: silk / ribbons / sheen / palette drift + universal hueShift/colorBoost/bgColor/audioReactivity.",
  "INPUTS": [
    {
      "NAME": "sheenAmount",
      "LABEL": "Sheen",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "silkAmount",
      "LABEL": "Silk",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "ribbonAmount",
      "LABEL": "Ribbons",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "paletteDrift",
      "LABEL": "Palette Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
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
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  SYNESTHETE — EaselAudio flagship (id 1205)
//  Free-form engine showcase. Stems are the primary drivers: stemBass →
//  warp depth (through the bass Time clock, never raw→position), stemMelody
//  → ribbon amplitude, stemAir → sheen gain. audioEnergy morphs the whole
//  mood from slow silk to electric moiré; audioToggleOnBeat flips a subtle
//  palette rotation each beat (music-gated). Whole-frame GAIN follower on
//  the mid-luma field keeps every style — including beatless ambient —
//  measurably alive. Silence: slow silk on TIME alone, gains = exactly 1.
// ════════════════════════════════════════════════════════════════════════

float hash21(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

float vnoise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

float fbm(vec2 p) {
    float v = 0.0;
    float amp = 0.5;
    for (int i = 0; i < 4; i++) {
        v += amp * vnoise(p);
        p = p * 2.07 + vec2(1.7, 9.2);
        amp *= 0.5;
    }
    return v;
}

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

// iq cosine palette — silk duotone through violet / teal / rose
vec3 silkPal(float u) {
    return vec3(0.46, 0.40, 0.52)
         + vec3(0.34, 0.28, 0.34) * cos(6.28318 * (u + vec3(0.02, 0.30, 0.58)));
}

vec2 rot2(vec2 p, float a) {
    float s = sin(a), c = cos(a);
    return vec2(c * p.x - s * p.y, s * p.x + c * p.y);
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    vec2 p  = uv - 0.5;
    p.x *= RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    float t = TIME;

    float aR = clamp(audioReactivity, 0.0, 2.0);

    // ── EaselAudio conditioning ─────────────────────────────────────────
    float bass = clamp(audioBass, 0.0, 1.0);
    float mid  = clamp(audioMid,  0.0, 1.0);
    float high = clamp(audioHigh, 0.0, 1.0);
    float sBass = clamp(max(stemBass, bass), 0.0, 1.0);
    float sMel  = clamp(max(stemMelody, mid), 0.0, 1.0);
    float sAir  = clamp(max(stemAir, high), 0.0, 1.0);
    float present = smoothstep(0.02, 0.12, max(audioLevel, audioEnergy));
    float beatRamp = smoothstep(0.0, 0.40, audioBeatPhase);
    float beatSoft = clamp(audioBeatPulse, 0.0, 1.0) * beatRamp;

    // Mood: calm silk ↔ electric moiré
    float inten = smoothstep(0.28, 0.85, clamp(audioEnergy, 0.0, 1.0) * min(aR, 1.5));

    // ── Palette rotation: EASED beat toggle (music-gated) + slow drift ──
    // The toggle flips at the exact frame beatPhase resets, so easing on
    // beatPhase reconstructs the previous state and glides to the new one
    // over the first ~35% of the beat — no single-frame palette step.
    float tog = clamp(audioToggleOnBeat, 0.0, 1.0);
    float togEased = mix(1.0 - tog, tog, smoothstep(0.0, 0.35, audioBeatPhase));
    float palPhase = 0.035 * sin(t * 0.05) * clamp(paletteDrift, 0.0, 2.0)
                   + 0.040 * togEased * present * aR;

    // ── Domain-warped silk — advected by the bass Time clock ────────────
    float t1 = t * 0.055 + audioBassTime * 0.16 * aR;
    vec2 q = rot2(p, t * 0.02 + audioTime * 0.03 * aR) * 1.45;
    float n1 = fbm(q + vec2(t1, -t1 * 0.7));
    float n2 = fbm(q * 1.6 + vec2(-t1 * 0.6, t1 * 0.5) + n1 * 2.7);
    // Stem → warp depth, split between the fast stem and its slow Presence
    // EMA so a kick can't step the whole warp field in a single frame
    // (fast bass feel rides the audioBassTime advection clock instead)
    float sBassP = clamp(stemBassPresence, 0.0, 1.0);
    float warpAmp = 0.55 + (0.10 * sBass + 0.42 * sBassP) * aR;
    float w = fbm(q + warpAmp * 2.1 * vec2(n1, n2));

    // Widened field spread + luminance sculpting for real contrast
    vec3 silk = silkPal((w - 0.5) * 1.9 + 0.5 + palPhase + t * 0.008);
    silk *= 0.55 + 0.9 * w;

    // silk thread contours: crisp iso-lines of the warp field — the weave.
    // Tight value-threshold cores (thin bright thread + hairline shade) give
    // pixel-scale detail across the whole cloth without touching its motion.
    float thr = 1.0 - abs(fract(w * 11.0 + palPhase * 2.0) * 2.0 - 1.0);
    float thread = smoothstep(0.86, 0.96, thr);
    float shade  = smoothstep(0.86, 0.96, 1.0 - abs(fract(w * 11.0 + palPhase * 2.0 + 0.5) * 2.0 - 1.0));
    silk += (silkPal((w - 0.5) * 1.9 + 0.72 + palPhase) * 0.85 + 0.15) * thread * 0.58
          * clamp(silkAmount, 0.0, 2.0);
    silk *= 1.0 - shade * 0.22;

    silk = mix(silk, bgColor.rgb, clamp(bgColor.a, 0.0, 1.0) * 0.85);
    silk *= clamp(silkAmount, 0.0, 2.0) * 0.5 + 0.5;            // 0→dim, 1→full

    // ── Ribbon interference — stemMelody amplitude, mid Time clock ──────
    float rFreq = mix(7.0, 16.0, inten);                        // moiré densifies
    float a0 = 0.6 + 0.15 * sin(t * 0.04);
    float axis = p.x * sin(a0) + p.y * cos(a0);
    float ph1 = axis * rFreq + w * mix(3.0, 7.5, inten) + t * 0.35 + audioMidTime * 1.9 * aR;
    float ph2 = ph1 * 1.13 + w * 4.0 - t * 0.27 - audioMidTime * 0.9 * aR + 1.7;
    float rib = (0.5 + 0.5 * sin(ph1)) * (0.5 + 0.5 * sin(ph2));
    float sMelP = clamp(stemMelodyPresence, 0.0, 1.0);
    float ribAmp = clamp(ribbonAmount, 0.0, 2.0) * (0.40 + (0.28 * sMel + 0.34 * sMelP) * aR);
    float ribMask = smoothstep(0.42, mix(0.78, 0.56, inten), rib) * clamp(ribAmp, 0.0, 1.6);
    vec3 ribCol = silkPal((w - 0.5) * 1.9 + 0.95 + palPhase) * 1.55;
    vec3 col = mix(silk, ribCol, clamp(ribMask * 0.72, 0.0, 1.0));
    // crisp ribbon centerline: tight threshold at the interference peak —
    // a sharp bright filament riding the soft moiré band
    float ribLine = smoothstep(0.80, 0.92, rib) * clamp(ribAmp, 0.0, 1.6);
    col += ribCol * ribLine * 0.62;
    // fine striations inside the ribbon body (detail octave on the mid clock)
    float striae = smoothstep(0.70, 0.88, 0.5 + 0.5 * sin(ph1 * 3.0 + w * 5.0));
    col += ribCol * striae * ribMask * 0.30;

    // ── Sheen — satin highlight scrolling on the high Time clock ────────
    float shv = 0.5 + 0.5 * sin(w * 9.4 + t * 0.55 + audioHighTime * 1.5 * aR);
    float sh = pow(clamp(shv, 0.0, 1.0), 6.0);
    float shLine = smoothstep(0.955, 0.990, shv);   // crisp specular hairline
    col += vec3(0.90, 0.92, 1.00) * (sh * 0.20 + shLine * 0.48)
         * clamp(sheenAmount, 0.0, 2.0) * (0.38 + 0.62 * sAir * aR);

    // ── Whole-frame GAIN follower (mid-luma scene, linear — playbook) ───
    float gain = (0.28 * bass + 0.24 * mid + 0.18 * high
                + 0.30 * beatSoft + 0.18 * clamp(audioPunch, 0.0, 1.0) * beatRamp)
               * 0.50 * aR;
    col *= 1.0 + gain;

    // ── Universal color block ───────────────────────────────────────────
    if (hueShift > 0.001) {
        vec3 hsv = rgb2hsv(clamp(col, 0.0, 2.0));
        hsv.x = fract(hsv.x + hueShift);
        col = hsv2rgb(hsv);
    }
    float luma = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(luma), col, clamp(colorBoost, 0.0, 2.0));

    // Gentle vignette + whisper of grain (keeps blacks alive)
    col *= 1.0 - 0.30 * smoothstep(0.28, 0.95, dot(p, p));
    // STATIC grain, silk tooth: fine edge energy at zero motion cost
    // (static — must not drown the ambient followers)
    float g = hash21(uv * RENDERSIZE.xy) - 0.5;
    col += g * 0.048;

    gl_FragColor = vec4(col, 1.0);
}
