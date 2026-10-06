/*{
  "CATEGORIES": [
    "Generator",
    "Text"
  ],
  "DESCRIPTION": "Typewriter — characters appear one by one with blinking cursor",
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
      "DEFAULT": 0,
      "GROUP": "Text"
    },
    {
      "NAME": "textScale",
      "LABEL": "Size",
      "TYPE": "float",
      "MIN": 0.01,
      "MAX": 1,
      "DEFAULT": 0.3,
      "GROUP": "Text"
    },
    {
      "NAME": "kerning",
      "LABEL": "Spacing",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Text"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 40,
      "DEFAULT": 12,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "cursorBlink",
      "LABEL": "Cursor Blink",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 5,
      "DEFAULT": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "oscSpeed",
      "LABEL": "Osc Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 10,
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "oscAmount",
      "LABEL": "Osc Amount",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.2,
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "oscSpread",
      "LABEL": "Osc Spread",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "textColor",
      "LABEL": "Color",
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
        0.02,
        0.02,
        0.04,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": true,
      "GROUP": "Background"
    },
    {
      "NAME": "voiceSync",
      "LABEL": "Voice Sync",
      "TYPE": "bool",
      "DEFAULT": false
    },
    {
      "NAME": "loop",
      "LABEL": "Loop",
      "TYPE": "bool",
      "DEFAULT": false
    }
  ]
}*/

float h11(float x) { return fract(sin(x * 127.1) * 43758.5453); }

float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}

int getChar(int slot) {
    if (slot == 0) return int(msg_0);
    if (slot == 1) return int(msg_1);
    if (slot == 2) return int(msg_2);
    if (slot == 3) return int(msg_3);
    if (slot == 4) return int(msg_4);
    if (slot == 5) return int(msg_5);
    if (slot == 6) return int(msg_6);
    if (slot == 7) return int(msg_7);
    if (slot == 8) return int(msg_8);
    if (slot == 9) return int(msg_9);
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
    if (n <= 0) return 7;
    if (n > 64) return 64;
    return n;
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    int numChars = charCount();
    float sc = textScale > 0.01 ? textScale : 1.0;
    float kr = kerning > 0.01 ? kerning : 1.0;

    vec3 col = bgColor.rgb;
    float alpha = transparentBg ? 0.0 : 1.0;

    vec2 p = vec2((uv.x - 0.5) * aspect + 0.5, uv.y);
    float maxW = aspect * 0.9;

    // LINEAR band envelopes (r2): the round-1 pow/smoothstep knees crushed
    // ambient's 0.1-0.8 swells into ~2% size wiggle — deaf. The bands are
    // already smoothed upstream; these all feed geometry (scale/bob/drift),
    // so they take no knee. Knee-shaping stays on the beat accent only.
    // NOTE: audio modulates AMPLITUDES of a stable TIME clock — the old
    // `TIME * (0.5 + 1.2*energy)` clock phase-jumped instead of following.
    float bassP = audioBass;
    float midP  = audioMid;
    float highP = audioHigh;
    float beatKick = audioBeatPulse * audioBeatPulse;

    // Typewriter reveal
    int revealed;
    if (voiceSync) {
        // Voice sync mode: show exactly as many chars as speech has produced
        // msg_len is updated in real-time by the speech recognition system
        revealed = numChars;
    } else {
        float typeTime = float(numChars) / speed;
        float t = TIME;
        if (loop) {
            float cycle = typeTime + 2.0;
            t = mod(t, cycle);
        }
        revealed = int(floor(t * speed));
        if (revealed > numChars) revealed = numChars;
    }
    int showCount = revealed;

    // Auto-scale: shrink text to fit all revealed characters on screen
    float baseH = 0.18 * sc;
    if (aspect < 1.0) baseH *= aspect;
    float baseW = baseH * (5.0 / 7.0);
    float baseGap = baseW * 0.25 * kr;
    float baseStep = baseW + baseGap;
    float neededW = max(float(showCount), 1.0) * baseStep;
    float fitScale = neededW > maxW ? maxW / neededW : 1.0;

    // Size breathing — the whole word swells with bass and mids. This is
    // the main audible→visible path: glyphs are the only big thing on
    // screen, so scale is where the response has to live. Silence → 1.0.
    float sizePulse = 1.0 + 0.14 * bassP + 0.07 * midP;
    float charH = baseH * fitScale * sizePulse;
    float charW = charH * (5.0 / 7.0);
    float gap = charW * 0.25 * kr;
    float cellStep = charW + gap;

    // Slow global bob — always-on autonomous drift; bass deepens its swing.
    float globalBob = charH * (0.05 + 0.10 * bassP) * sin(TIME * 0.5);
    float originY = 0.5 - charH * 0.5 + globalBob;

    // Center visible text — all characters always visible
    float visibleW = float(showCount) * cellStep - gap;
    if (showCount <= 0) visibleW = 0.0;
    float originX = 0.5 - visibleW * 0.5;

    // Render characters
    float textMask = 0.0;
    vec3 textCol = vec3(0.0);
    float lastX = originX;

    for (int i = 0; i < 64; i++) {
        if (i >= showCount) break;
        if (i >= numChars) break;

        int ch = getChar(i);
        // Autonomous per-character drift — small, incommensurate frequencies
        // (golden-angle phase spacing) so characters never lock into a shared
        // period; lives even with the user oscillator off / in silence.
        float idxPhase = float(i) * 2.399963;
        float driftX = charW * (0.04 + 0.09 * highP) * sin(TIME * (0.23 + 0.07 * h11(float(i) + 3.7)) + idxPhase * 1.3);
        float driftY = charH * (0.06 + 0.15 * midP) * sin(TIME * (0.35 + 0.11 * h11(float(i))) + idxPhase);
        float cx = originX + float(i) * cellStep + driftX;
        // Oscillator: per-character Y offset (user-controlled + autonomous drift)
        float oscY = driftY + oscAmount * sin(TIME * oscSpeed * 6.2832 + float(i) * oscSpread * 3.14159);

        if (ch >= 0 && ch <= 36 && ch != 26) {
            vec2 cellUV = vec2((p.x - cx) / charW, (p.y - (originY + oscY)) / charH);
            if (cellUV.x >= 0.0 && cellUV.x <= 1.0 && cellUV.y >= 0.0 && cellUV.y <= 1.0) {
                float s = sampleChar(ch, cellUV);
                if (s > 0.05) {
                    textCol = textColor.rgb;
                    textMask = max(textMask, smoothstep(0.1, 0.5, s));
                }
            }
        }

        lastX = cx + cellStep;
    }

    // Blinking cursor after last char — phase-wobbled so it never freezes
    // into a perfectly periodic (visually static) blink, and gets a soft
    // width pulse on beat.
    float blinkPhase = fract(TIME * cursorBlink + 0.15 * sin(TIME * 0.37));
    float cursorOn = step(0.5, blinkPhase);
    float cursorW = charW * 0.15 * (1.0 + 0.4 * beatKick);
    if (p.x >= lastX && p.x <= lastX + cursorW &&
        p.y >= originY && p.y <= originY + charH) {
        textCol = textColor.rgb;
        textMask = max(textMask, cursorOn);
    }

    // R3: backdrop RGB breath — the eval (and any RGB-reading consumer)
    // ignores alpha; with a transparent bg 99.8% of the canvas carried zero
    // RGB response and the tiny glyphs (~0.2% of pixels) could never move
    // frameDiff. Linear composite follower with bus band-mix weights; alpha
    // stays textMask so the app's transparent overlay is untouched.
    // Silence = exactly the current look (adds 0).
    float bgB = 0.14 * bassP + 0.09 * midP + 0.055 * highP;
    col += vec3(0.85, 0.90, 1.15) * bgB;

    col = mix(col, textCol, clamp(textMask, 0.0, 1.0));
    if (transparentBg) alpha = clamp(textMask, 0.0, 1.0);

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
