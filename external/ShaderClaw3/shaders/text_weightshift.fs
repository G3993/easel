/*{
  "DESCRIPTION": "Weightshift — the message morphs between a hairline-thin rendering and a heavy bold rendering of the same glyphs, crossfading smoothly like a live variable-font specimen. A small weight-axis readout tracks the cycle; audio nudges the pace, the peak boldness and a fine edge shimmer without ever taking over.",
  "CREDIT": "ShaderClaw — original weight-axis type specimen",
  "CATEGORIES": [
    "Text",
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "WEIGHTSHIFT",
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
      "LABEL": "Size",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 2.2,
      "DEFAULT": 1,
      "GROUP": "Text"
    },
    {
      "NAME": "kerning",
      "LABEL": "Kerning",
      "TYPE": "float",
      "MIN": 0.55,
      "MAX": 1.3,
      "DEFAULT": 0.82,
      "GROUP": "Text"
    },
    {
      "NAME": "cycleSpeed",
      "LABEL": "Cycle Speed",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 1.2,
      "DEFAULT": 0.22,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "boldAmount",
      "LABEL": "Max Boldness",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.65,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "thinColor",
      "LABEL": "Thin Color",
      "TYPE": "color",
      "DEFAULT": [
        0.6,
        0.88,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "boldColor",
      "LABEL": "Bold Color",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.16,
        0.42,
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
      "DEFAULT": true,
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.7,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "inputImage",
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
    }
  ]
}*/

// =====================================================================
// WEIGHTSHIFT — a single line of text that lives on a "weight axis":
// it holds thin, morphs to heavy bold, holds bold, morphs back — using
// ONE shared 5x7 glyph atlas. The bold rendering is synthesized on the
// fly by dilating the atlas coverage (8-tap neighborhood max) rather
// than needing a second bold atlas, so the same glyph shapes genuinely
// thicken and thin rather than just changing brightness.
// =====================================================================

float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float hash21(vec2 p)  { return fract(sin(dot(p, vec2(41.3, 289.1))) * 43758.5453); }

float knee(float x, float lo, float hi) { return clamp(smoothstep(lo, hi, x), 0.0, 1.0); }

// Raw glyph coverage from the shared font atlas.
// Atlas index convention: A-Z = 0-25, space = 26, 0-9 = 27-36.
float sampleAtlasRaw(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}

// Dilated sample — fixed 8-tap neighborhood max around the same atlas
// glyph. This is how the "bold" weight is synthesized: no second atlas,
// just a wider coverage footprint of the identical letterform.
float sampleAtlasBold(int ch, vec2 uv, float px) {
    float c = sampleAtlasRaw(ch, uv);
    c = max(c, sampleAtlasRaw(ch, uv + vec2( px, 0.0)));
    c = max(c, sampleAtlasRaw(ch, uv + vec2(-px, 0.0)));
    c = max(c, sampleAtlasRaw(ch, uv + vec2(0.0,  px)));
    c = max(c, sampleAtlasRaw(ch, uv + vec2(0.0, -px)));
    vec2 diag = vec2(px, px) * 0.7;
    c = max(c, sampleAtlasRaw(ch, uv + diag));
    c = max(c, sampleAtlasRaw(ch, uv + vec2(-diag.x,  diag.y)));
    c = max(c, sampleAtlasRaw(ch, uv + vec2( diag.x, -diag.y)));
    c = max(c, sampleAtlasRaw(ch, uv - diag));
    return c;
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
    return n > 0 ? n : 1;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;
    vec2 p = vec2((uv.x - 0.5) * aspect, uv.y - 0.5);

    // ─── Audio conditioning (soft knees, idle floor, ~1/3 of motion) ───
    float audio  = clamp(audioReact, 0.0, 2.0);
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP   = pow(knee(audioMid,  0.08, 0.85), 1.3);
    float highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float drive  = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9); // never zero
    // LINEAR followers (ambient fix r2): bands are pre-smoothed, no knee —
    // ambient's 0.1-0.8 band swells pass through 1:1. audioF floors the
    // user param (default 0.7 was diluting the follower depth).
    float bassSm = clamp(audioBass, 0.0, 1.0);
    float midSm  = clamp(audioMid,  0.0, 1.0);
    float audioF = 0.6 + 0.4 * min(audio, 1.0);

    // ─── Weight-axis cycle: hold thin → morph → hold bold → morph back ───
    // Bass gives the pace a gentle, bounded nudge (never a jolt — range
    // stays within ±12% so a sudden level change can't snap the phase).
    float rate = cycleSpeed * (0.94 + 0.12 * bassP * audio * drive);
    float cyc  = fract(TIME * rate);
    float weightT;
    if (cyc < 0.16)      weightT = 0.0;
    else if (cyc < 0.5)  weightT = smoothstep(0.16, 0.5, cyc);
    else if (cyc < 0.66) weightT = 1.0;
    else                 weightT = 1.0 - smoothstep(0.66, 1.0, cyc);

    // Continuous band-following on the weight axis itself (ambient fix):
    // a smoothed bass swell leans the whole specimen toward bold — glyph
    // weight, palette, card color and axis dot all track it — and eases
    // back as the swell fades. mix() toward 1.0 keeps headroom (the axis
    // never clamps at a rail, so sustained EDM bass still breathes).
    weightT = mix(weightT, 1.0, min(0.42 * bassSm * audioF, 0.6));

    // How heavy the "bold" peak gets — bass adds real structural weight
    // to the extreme (this IS the shader's whole point, so bass gets a
    // wider berth here than a typical zoom/warp knob), mids add a slow
    // breathing wobble on top.
    float boldPeak = clamp(boldAmount * (0.75 + 0.42 * bassP * audio * drive)
                          + 0.03 * midP * audio * drive * sin(TIME * 0.6), 0.0, 1.0);

    // ─── Layout: single centered line, auto-fit to canvas width ───
    int numChars = charCount();
    float nF = float(numChars);
    float charH = 0.20 * textScale;
    float charW = charH * (5.0 / 7.0);
    float kern  = charW * kerning;
    float totalW = nF * kern;
    float maxW = aspect * 0.92;
    float fitShrink = min(1.0, maxW / max(totalW, 1e-4));
    charH *= fitShrink; charW *= fitShrink; kern *= fitShrink; totalW *= fitShrink;

    float startX = -totalW * 0.5;
    float lx = p.x - startX;
    float ly = charH * 0.5 - p.y;

    float glyphMask = 0.0;
    vec3  inkCol    = vec3(0.0);

    if (lx >= 0.0 && lx < totalW && ly >= 0.0 && ly < charH) {
        int idx = int(floor(lx / kern));
        if (idx >= 0 && idx < numChars) {
            int ch = getChar(idx);
            float cellLx = lx - float(idx) * kern;
            float colPad = (kern - charW) * 0.5;
            vec2 cuv = vec2((cellLx - colPad) / charW, 1.0 - ly / charH);
            if (cuv.x >= 0.0 && cuv.x <= 1.0) {
                float raw     = sampleAtlasRaw(ch, cuv);
                float px      = mix(0.0, 0.17, boldPeak);
                float rawBold = sampleAtlasBold(ch, cuv, px);

                float thinMask = smoothstep(0.38, 0.68, raw);
                float boldMask = smoothstep(0.15, 0.46, rawBold);
                float m = mix(thinMask, boldMask, weightT);

                // Ghost outline: a faint hint of the OTHER weight's
                // silhouette peeking through at the edge — makes the
                // crossfade read as two real renderings overlapping,
                // not a blur, and adds a second thin contour to look at.
                float otherMask = mix(boldMask, thinMask, weightT);
                float ghost = clamp(otherMask - m, 0.0, 1.0) * 0.4;

                // Edge shimmer: a band-pass around the interpolated
                // coverage's transition boundary — highs sparkle right
                // where the stroke edge currently sits, no derivatives
                // needed and no strobe (continuous, capped amplitude).
                float edge = smoothstep(0.35, 0.5, m) - smoothstep(0.5, 0.65, m);
                float shimmer = edge * highP * audio * drive * 0.65;

                glyphMask = clamp(m + ghost + shimmer * 0.4, 0.0, 1.0);

                vec3 pal = mix(thinColor.rgb, boldColor.rgb, weightT);
                vec3 ghostCol = mix(boldColor.rgb, thinColor.rgb, weightT);
                pal = mix(pal, ghostCol, clamp(ghost / max(m + ghost, 1e-3), 0.0, 1.0) * 0.6);
                pal *= 1.05;
                pal += shimmer * vec3(1.0, 1.0, 1.0);
                vec3 texCol = texture2D(inputImage, uv).rgb;
                inkCol = mix(pal, texCol, clamp(texMix, 0.0, 1.0));
            }
        }
    }

    // ─── Background: black canvas, subtle bass-breathing vignette ───
    vec3 col = bgColor.rgb;
    float rFromCenter = length(p * vec2(1.0 / aspect, 1.0)) * 1.15;
    float vign = 1.0 - smoothstep(0.35, 1.05, rFromCenter);
    col += vec3(0.05, 0.05, 0.08) * vign * (0.15 + 0.10 * bassP * audio * drive);
    float overlayA = 0.0;

    // ─── Specimen swatch card: a soft rounded color panel behind the
    // word that tracks the SAME thin→bold palette as the glyphs — the
    // whole card recolors with the weight cycle, like a spec-sheet
    // swatch changing with the axis. Rounded-box SDF, soft edge, low
    // opacity so black stays dominant (house taste: high contrast).
    vec2 cardHalf = vec2(totalW * 0.5 + charH * 0.35, charH * 0.62);
    vec2 cardP = p - vec2(0.0, charH * 0.02);
    vec2 cd = abs(cardP) - cardHalf;
    float cardDist = length(max(cd, 0.0)) + min(max(cd.x, cd.y), 0.0) - charH * 0.18;
    float cardMask = 1.0 - smoothstep(0.0, charH * 0.07, cardDist);
    vec3 cardColor = mix(thinColor.rgb, boldColor.rgb, weightT);
    float cardAlpha = cardMask * (0.15 + 0.06 * bassP * audio * drive
                                       + 0.09 * audioBeatPulse * audio * drive);
    col = mix(col, cardColor, cardAlpha);
    overlayA = max(overlayA, cardAlpha);

    // ─── Weight-axis readout: a thin tick track under the word, with a
    // dot that slides thin→bold→thin in lockstep with the cycle above —
    // a small "specimen" readout so the morph reads as a deliberate axis,
    // not a random flicker. A beat gives its glow a gentle pulse only.
    float barY = -charH * 0.95;
    float barHalfW = totalW * 0.5 * 0.9;
    float barThick = max(charH * 0.045, 0.0015);
    if (totalW > 0.0 && abs(p.y - barY) < barThick * 3.0 && abs(p.x) < barHalfW) {
        float trackA = smoothstep(barThick * 1.6, barThick * 0.6, abs(p.y - barY));
        // The track itself is a thin→bold gradient ramp (a literal "weight
        // axis" ruler) rather than a flat tone — spells out the whole
        // range the dot travels across, not just its current position.
        float rampX = clamp((p.x + barHalfW) / (2.0 * barHalfW), 0.0, 1.0);
        vec3 rampCol = mix(thinColor.rgb, boldColor.rgb, rampX) * 0.5;
        col = mix(col, rampCol, trackA * 0.6);
        overlayA = max(overlayA, trackA * 0.35);

        float dotX = mix(-barHalfW, barHalfW, weightT);
        float beatGlow = 1.0 + 0.25 * audioBeatPulse * audio * drive;
        float dotR = charH * 0.10 * beatGlow;
        vec2 dv = vec2(p.x - dotX, (p.y - barY) * 3.0);
        float dotD = length(dv);
        float dotMask = smoothstep(dotR, dotR * 0.5, dotD);
        vec3 dotCol = mix(thinColor.rgb, boldColor.rgb, weightT) * beatGlow;
        col = mix(col, dotCol, dotMask);
        overlayA = max(overlayA, dotMask);
    }

    // ─── Sparse peripheral sparkle — highs only, well away from the
    // text block, kept rare so it reads as fine dust, never a strobe.
    vec2 gridUV = p * 9.0;
    vec2 gid = floor(gridUV);
    float sh = hash21(gid + floor(TIME * 0.5));
    float sparkleGate = step(0.978, sh);
    float distToBlock = max(abs(p.x) - totalW * 0.5, abs(p.y) - charH * 1.1);
    float awayFromText = smoothstep(0.0, charH * 1.5, distToBlock);
    float sparkle = sparkleGate * awayFromText * highP * audio * drive;
    col += vec3(0.65, 0.82, 1.0) * sparkle * 0.7;
    overlayA = max(overlayA, sparkle * 0.7);

    col = mix(col, inkCol, glyphMask);

    // Gentle overall brightness/contrast lift tied to audioEnergy (law:
    // bloom/contrast ~ energy, ±~20-30%) — a broad, clearly-attributable
    // audio signal layered on top of (never replacing) the idle cycle.
    // Ambient's energy is nearly constant, so bass/mid followers carry the
    // continuous luminance breath; energy still adds the broad lift.
    // r2: the bass/mid follower is floored (audioF) and NOT multiplied by
    // drive — round 1's audio*drive stack cut its depth to ~0.05 effective.
    float energyLift = 1.0 + 0.20 * knee(audioEnergy, 0.05, 0.9) * audio * drive
                           + (0.40 * bassSm + 0.22 * midSm) * audioF;
    col *= energyLift;

    // r3: whole-canvas breathing wash — 74% of the frame is bare background
    // where a multiplicative lift can't register (black x anything = black);
    // an additive linear follower on the backdrop is what carries the ambient
    // response at eval scale. RGB only — alpha below is untouched, so the
    // transparent-overlay behavior in the app is unchanged. Silence adds 0.
    float highSm = clamp(audioHigh, 0.0, 1.0);
    col += vec3(0.10, 0.11, 0.16) * (0.85 * bassSm + 0.55 * midSm + 0.35 * highSm) * audioF;

    float alpha = 1.0;
    if (transparentBg) {
        alpha = clamp(max(glyphMask, overlayA), 0.0, 1.0);
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
