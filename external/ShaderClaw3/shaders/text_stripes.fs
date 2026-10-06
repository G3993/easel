/*{
  "DESCRIPTION": "Stripes — the message hides behind sweeping diagonal stripe bands that shift phase and width over time, revealing letters only in the gaps between bars.",
  "CREDIT": "ShaderClaw — original take on a diagonal-stripe reveal/mask effect",
  "CATEGORIES": [
    "Generator",
    "Text",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "ETHEREA",
      "MAX_LENGTH": 48,
      "LABEL": "Message",
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
      "DEFAULT": 1,
      "MIN": 0.4,
      "MAX": 2.4,
      "GROUP": "Text"
    },
    {
      "NAME": "kerning",
      "LABEL": "Kerning",
      "TYPE": "float",
      "DEFAULT": 0.9,
      "MIN": 0.55,
      "MAX": 1.4,
      "GROUP": "Text"
    },
    {
      "NAME": "stripeCount",
      "LABEL": "Stripe Count",
      "TYPE": "float",
      "DEFAULT": 10,
      "MIN": 3,
      "MAX": 28,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "stripeAngle",
      "LABEL": "Stripe Angle",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "sweepSpeed",
      "LABEL": "Sweep Speed",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "widthPulse",
      "LABEL": "Width Breathing",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "sweepDir",
      "LABEL": "Sweep Direction",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "gapBreathRate",
      "LABEL": "Gap Breath Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 5,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "bandStagger",
      "LABEL": "Band Stagger",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "angleSway",
      "LABEL": "Angle Sway",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "colorA",
      "LABEL": "Stripe Color A",
      "TYPE": "color",
      "DEFAULT": [
        0.02,
        0.85,
        0.95,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "colorB",
      "LABEL": "Stripe Color B",
      "TYPE": "color",
      "DEFAULT": [
        0.95,
        0.15,
        0.55,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "textColor",
      "LABEL": "Text Color",
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
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent BG",
      "TYPE": "bool",
      "DEFAULT": false,
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "inputTex",
      "LABEL": "Stripe Texture",
      "TYPE": "image"
    },
    {
      "NAME": "texMix",
      "LABEL": "Texture Mix",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1
    }
  ]
}*/

// ═══════════════════════════════════════════════════════════════════════
// STRIPES — message masked/revealed by animated diagonal stripe bands.
// Bands sweep across the canvas, shifting phase + width over time; the
// message only shows through the gaps between bars, so the composition
// reads as text being scanned/unveiled by a moving venetian-blind field.
// House font idiom: fontAtlasTex sampling, A-Z=0-25/space=26/0-9=27-36,
// msg_0..msg_47 slots + msg_len (same convention as text_ascii/clusters).
// ═══════════════════════════════════════════════════════════════════════

#define SPACE_CH 26

float charPixel(int ch, float col, float row) {
    if (ch < 0 || ch > 36) return 0.0;
    vec2 uv = vec2((float(ch) + col / 5.0) / 37.0, 1.0 - row / 7.0);
    if (uv.x < 0.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return smoothstep(0.12, 0.55, texture2D(fontAtlasTex, uv).r);
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
    return int(msg_47);
}

int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 1;
    if (n > 48) return 48;
    return n;
}

float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }

// Soft-knee helper (playbook standard conditioning snippet).
float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    // Centered, aspect-corrected plane.
    vec2 p = vec2((uv.x - 0.5) * aspect, uv.y - 0.5);

    // ── Audio conditioning (playbook laws 1,3,6,7) ──────────────────
    // Idle floor keeps the sweep alive in total silence; audio adds
    // roughly a third of the total motion on top of the base drive.
    float audio  = clamp(audioReact, 0.0, 2.0);
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);   // structural: band width
    float midP   = pow(knee(audioMid, 0.08, 0.85), 1.3);    // detail: phase jitter
    float highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);   // sparkle: edge shimmer
    float drive  = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9); // never zero

    // Time-warp clock — whole sweep breathes with the track, but the
    // idle base speed alone already reads as a complete, good-looking
    // animation (sound-off test). Kept modest so audio's contribution
    // (added below on duty/width/glow) reads clearly against it.
    // Fixed base rate + a bounded, energy-following phase push. Scaling
    // TIME by a live factor made the phase jump grow with elapsed time
    // (choppy); a bounded offset glides with the smoothed envelope.
    // Silence: drive floors at 0.25 -> exactly TIME * 0.37 as before.
    float musicTime = TIME * 0.37 + 0.9 * (drive - 0.25) * audio;

    // ── Diagonal stripe field ───────────────────────────────────────
    // Angle 0..1 maps to a shallow..steep diagonal; project position
    // onto the stripe normal to get a 1D coordinate that the bands
    // sweep along.
    // Angle Sway: the stripe field slowly rocks its angle
    float ang = mix(0.15, 1.35, clamp(stripeAngle, 0.0, 1.0)) + angleSway * 0.3 * sin(musicTime * 0.4);
    vec2 dir = vec2(cos(ang), sin(ang));
    float along = dot(p, dir);

    // Band count + width breathe slowly over time; bass adds a
    // structural swell on top — audio's share of the total width
    // motion (~±20-25% around the idle wobble), never all of it.
    float nBase = max(stripeCount, 1.0);
    // Bass no longer rescales the band count — a global rescale slides
    // every stripe edge at once on each kick; its swell moved to the duty
    // cycle below, where motion stays bounded within a band period.
    float nSwell = 1.0 + widthPulse * 0.30 * sin(musicTime * 0.31);
    float n = nBase * nSwell;

    // Phase shift sweeps the stripe field across the canvas over time;
    // mids add fine jitter to the phase so bands don't feel mechanical
    // (detail/turbulence routing — texture, not structure).
    float phase = musicTime * sweepDir * mix(0.10, 0.8, clamp(sweepSpeed, 0.0, 2.0) * 0.5)
                + midP * audio * 0.18 * sin(musicTime * 2.1);

    float band = along * n + phase;
    float bandFrac = fract(band);
    float bandId = floor(band);

    // Duty cycle (how much of each band period is "open" vs "bar")
    // breathes slowly on its own, plus bass widens/narrows the gaps —
    // a continuous structural swell (~±24% of the duty range), not a
    // binary gate or strobe.
    float duty = 0.5 + 0.20 * sin(musicTime * 0.53 * gapBreathRate + bandId * 0.7 * bandStagger)
               + 0.16 * widthPulse * bassP * audio;
    duty = clamp(duty, 0.15, 0.85);

    // Soft edge width — high-frequency energy sharpens the bar edges
    // (sparkle band, edge-only, sparse subset of the signal).
    float edge = mix(0.06, 0.014, highP * audio) + 0.015;

    // isOpen: 1.0 where the message can show through (the gap between
    // bars), 0.0 where an opaque stripe bar occludes the text.
    float isOpen = smoothstep(duty - edge, duty + edge, bandFrac);

    // Per-band hash drives a subtle per-band color/brightness variance
    // so the bars don't feel like one flat animated gradient.
    float bh = hash11(bandId * 12.9898);

    // ── Text layout (house bitmap-font idiom) ───────────────────────
    int total = charCount();
    float charH = 0.11 * textScale;
    float charW = charH * (5.0 / 7.0);
    float kern  = charW * kerning;
    float textW = kern * float(total);

    vec2 tp = p - vec2(-textW * 0.5, 0.0);
    float col = tp.x / kern;
    int slot = int(floor(col));
    float glyph = 0.0;
    if (slot >= 0 && slot < total) {
        float lx = fract(col) * kern / charW;
        float ly = (tp.y / charH) + 0.5;
        if (lx >= 0.0 && lx < 1.0 && ly >= 0.0 && ly < 1.0) {
            int ch = getChar(slot);
            if (ch != SPACE_CH) {
                glyph = charPixel(ch, lx * 5.0, (1.0 - ly) * 7.0);
            }
        }
    }

    // ── Compose ──────────────────────────────────────────────────────
    // Curated 2-hue palette for the stripe bars; text is a clean third
    // hue (textColor) so it always reads distinctly from the bars.
    vec3 stripeCol = mix(colorA.rgb, colorB.rgb, bh);
    stripeCol *= 0.75 + 0.25 * bh;
    // Bass adds a gentle global lift to the bar brightness — continuous
    // swell (±20%, bloom/contrast routing), not a strobe/flash per beat.
    stripeCol *= 1.0 + 0.20 * bassP * audio;

    // Rim glow along the open/bar boundary — brightens with bass so the
    // edges themselves seem to pulse with the beat without ever gating
    // hard on/off (soft knee already baked into bassP).
    float rim = 1.0 - abs(bandFrac - duty) / max(edge * 3.0, 0.001);
    rim = clamp(rim, 0.0, 1.0);
    stripeCol += rim * rim * 0.5 * bassP * audio * mix(colorA.rgb, colorB.rgb, 1.0 - bh);

    vec3 bg = transparentBg ? vec3(0.0) : bgColor.rgb;

    // Optional image blended into the stripe bars only (texture on the
    // occluding bands), gated behind explicit texMix — never inferred
    // from IMG_SIZE, per house convention.
    vec3 barCol = stripeCol;
    if (texMix > 0.001) {
        vec4 tex = IMG_NORM_PIXEL(inputTex, uv);
        barCol = mix(stripeCol, tex.rgb * (0.6 + 0.4 * bh), texMix);
    }

    // Base: background where open (before text), bar color where closed.
    vec3 col3 = mix(barCol, bg, isOpen);
    float alpha = transparentBg ? (1.0 - isOpen) : 1.0;

    // Text only shows through the open gaps between stripes — the
    // stripes literally mask/reveal the message.
    float textVis = glyph * isOpen;
    col3 = mix(col3, textColor.rgb, textVis);
    if (transparentBg) alpha = max(alpha, textVis);

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = col3;
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
    col3 = uc;

    gl_FragColor = vec4(col3, alpha);
}
