/*{
  "DESCRIPTION": "Snap — every glyph of the message is flung out to a scattered, tumbling position around the frame, then elastically snaps home into the sentence with a springy overshoot, holds, and shatters back out to repeat. Characters stagger in and out so the assembly ripples rather than arriving in lockstep. Two-hue electric palette, warm flare on characters mid-flight, cooling to ink once settled.",
  "CREDIT": "ShaderClaw — original",
  "CATEGORIES": [
    "Generator",
    "Text",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "scatterSpread",
      "LABEL": "Scatter Spread",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.4,
      "MAX": 2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "cycleSpeed",
      "LABEL": "Cycle Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.3,
      "MAX": 2.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "holdTime",
      "LABEL": "Hold Time",
      "TYPE": "float",
      "DEFAULT": 1.7,
      "MIN": 0.3,
      "MAX": 5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorA",
      "LABEL": "Ink A",
      "TYPE": "color",
      "DEFAULT": [
        0.2,
        0.9,
        0.98,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "colorB",
      "LABEL": "Ink B",
      "TYPE": "color",
      "DEFAULT": [
        0.68,
        0.34,
        0.98,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "glowColor",
      "LABEL": "Flight Glow",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.55,
        0.2,
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
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "ETHEREA",
      "MAX_LENGTH": 32,
      "GROUP": "Text"
    },
    {
      "NAME": "textScale",
      "LABEL": "Size",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.4,
      "MAX": 2,
      "GROUP": "Text"
    },
    {
      "NAME": "kerning",
      "LABEL": "Kerning",
      "TYPE": "float",
      "DEFAULT": 1.05,
      "MIN": 0.7,
      "MAX": 1.6,
      "GROUP": "Text"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0.02,
        0.02,
        0.035,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent BG",
      "TYPE": "bool",
      "DEFAULT": 1,
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "DEFAULT": 0.8,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
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
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1
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
// SNAP — glyphs scatter to random tumbling positions around the frame,
// elastically spring home into the sentence, hold, then shatter back
// out and repeat. Per-character stagger (own delay band, own scatter
// direction/radius) keeps the assembly reading as a ripple, never a
// lockstep pop. Bitmap font sampled from fontAtlasTex (house convention:
// atlas index A-Z=0-25, space=26, 0-9=27-36; msg_0..msg_N + msg_len).
// =====================================================================

#define MAX_CHARS 32
#define SPACE_CH  26

// ─── Bitmap font (house convention) ────────────────────────────────
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
    return -1;
}

int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 0;
    if (n > MAX_CHARS) return MAX_CHARS;
    return n;
}

// ─── Utility ────────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  hash21(float n) { return vec2(hash11(n), hash11(n + 17.31)); }

// Soft-knee audio conditioning (playbook standard snippet).
float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
// Log-frequency FFT lookup — musical energy lives in the low bins.
float fftLog(float t) { return texture2D(audioFFT, vec2(pow(clamp(t, 0.0, 1.0), 2.2) * 0.5, 0.5)).r; }

// Damped-oscillation "spring" ease: 0 at t=0, overshoots past 1 then
// settles to 1 — the elastic-snap curve every character rides home on.
// `wobble` scales the oscillation amplitude (audio-reactive punch).
float springOut(float t, float wobble) {
    t = clamp(t, 0.0, 1.35);
    float decay = exp(-6.0 * t);
    float osc = cos(t * 15.5) * decay * wobble;
    return 1.0 - osc;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    // ── Audio conditioning (playbook laws 1,3,6,7) ──────────────────
    float reactAmt = clamp(audioReact, 0.0, 2.0);
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float drive  = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9);   // idle floor, never zero
    float punch  = clamp(audioBeatPulse, 0.0, 1.0);

    int numChars = charCount();
    if (numChars <= 0) {
        gl_FragColor = transparentBg ? vec4(0.0) : vec4(ucApply(bgColor.rgb), 1.0);
        return;
    }

    // ── Timeline: snap-in -> hold -> shatter-out -> repeat ──────────
    const float SNAP_DUR    = 0.85;
    const float SCATTER_DUR = 0.55;
    float holdDur  = clamp(holdTime, 0.2, 6.0);
    float cycleLen = SNAP_DUR + holdDur + SCATTER_DUR;

    // Constant base pace + bounded, smooth audio phase push. (Multiplying
    // TIME by a live audio value made the cycle phase wander erratically at
    // large TIME — uncorrelated jumps that drowned the real response.)
    float speedMul  = clamp(cycleSpeed, 0.2, 4.0);
    float musicClock = TIME * speedMul + 0.35 * drive * reactAmt;
    float phase = mod(musicClock, cycleLen);

    bool inSnap = phase < SNAP_DUR;
    bool inHold = (!inSnap) && (phase < SNAP_DUR + holdDur);
    float snapT     = clamp(phase / SNAP_DUR, 0.0, 1.0);
    float scatterT  = clamp((phase - SNAP_DUR - holdDur) / max(SCATTER_DUR, 0.001), 0.0, 1.0);

    // Bass adds punch to the elastic overshoot — structural, on-beat.
    float wobbleAmt = 0.16 * (1.0 + 0.6 * bassP * reactAmt);

    // ── Layout geometry (single centered row, house glyph proportions,
    // auto-fit so long messages never overflow the frame) ───────────
    float sizeMul = clamp(textScale, 0.4, 2.0);
    float baseCharH = 0.16 * sizeMul;
    float baseCharW = baseCharH * (5.0 / 7.0);
    float kernMul   = clamp(kerning, 0.7, 1.6);
    float baseKern  = baseCharW * kernMul;
    float maxTotalW = 0.86 * max(aspect, 0.5);
    float neededW   = float(numChars) * baseKern;
    float fitScale  = (neededW > maxTotalW) ? (maxTotalW / max(neededW, 0.0001)) : 1.0;
    float charH = baseCharH * fitScale;
    float charW = charH * (5.0 / 7.0);
    float kern  = charW * kernMul;
    float totalW = float(numChars) * kern;
    float startX = -totalW * 0.5 + kern * 0.5;

    vec3 col = bgColor.rgb;
    if (transparentBg) col = vec3(0.0);
    float alpha = transparentBg ? 0.0 : 1.0;

    // Faint always-on ambient so the frame is never a dead void in
    // silence (law 7) — a slow, low breathing wash tied to drive.
    float ambient = 0.02 + 0.03 * drive * reactAmt;
    col += glowColor.rgb * ambient * smoothstep(1.1, 0.0, length(p) / max(aspect, 1.0)) * (1.0 - float(transparentBg));

    // Ambient dust field — faint drifting motes, the residue of the
    // scatter energy hanging in the frame between snaps. Always alive
    // at a low floor (law 7); highs brighten it, sparse and subtle
    // (frequency -> space: fine detail <- audioHigh).
    vec2 dustUV = p * 9.0 + vec2(TIME * 0.035, -TIME * 0.025);
    vec2 dustCell = floor(dustUV);
    vec2 dustF = fract(dustUV) - 0.5;
    vec2 dustSeed = hash21(dot(dustCell, vec2(127.1, 311.7)));
    float dustDot = smoothstep(0.10 + dustSeed.x * 0.10, 0.0, length(dustF) - dustSeed.x * 0.05);
    float dustTwinkle = 0.5 + 0.5 * sin(TIME * 1.3 + dustSeed.y * 40.0);
    float dustAmt = dustDot * dustTwinkle * (0.045 + 0.12 * highP * reactAmt);
    vec3 dustCol = mix(colorA.rgb, colorB.rgb, dustSeed.y);
    col += dustCol * dustAmt;
    if (transparentBg) alpha = max(alpha, dustAmt * 0.5);

    // R2 whole-frame follower. The round-1 terms lived only on the glyphs
    // (~5% of the frame's pixels) and rode pow-crushed knees, so beatless
    // styles measured ~0. Add a linear bass+mid breathing wash across the
    // whole frame (ink-colored, center-weighted) + a decaying beat accent;
    // alpha rises with it so it reads in-app too. Silence = exact current
    // look (wash is 0 at zero audio).
    float fbL = audioBass;
    float fmL = audioMid;
    float kickL = pow(punch, 1.3);
    float washHalo = smoothstep(1.6, 0.0, length(p));
    // R3 chop fix: the 0.25 kick term attacks in a single frame across the
    // whole wash halo — it alone pushed EDM p95 steps to 0.12. The smooth
    // bass/mid terms carry the correlation (ambient sits at 1.1 — do not
    // shrink them); the kick is just an accent now.
    float washAmt = (0.30 * fbL + 0.18 * fmL + 0.09 * kickL) * washHalo * 0.65;
    vec3 washCol = mix(colorA.rgb, colorB.rgb, 0.5 + 0.5 * sin(TIME * 0.23));
    col += washCol * washAmt;
    if (transparentBg) alpha = max(alpha, washAmt * 0.8);

    for (int i = 0; i < MAX_CHARS; i++) {
        if (i >= numChars) break;
        int ch = getChar(i);
        if (ch == SPACE_CH || ch < 0 || ch > 36) continue;

        float fi = float(i);
        vec2 seed = hash21(fi * 12.97 + 4.31);

        // Per-character stagger bands: some glyphs lead, some lag, so
        // the assembly/shatter reads as a ripple rather than one pop.
        float snapDelay    = seed.x * 0.42;
        float scatterDelay = (1.0 - seed.x) * 0.42;

        float settle;
        if (inSnap) {
            float localT = clamp((snapT - snapDelay) / max(1.0 - snapDelay, 0.2), 0.0, 1.0);
            settle = springOut(localT, wobbleAmt);
        } else if (inHold) {
            settle = 1.0;
        } else {
            float localT = clamp((scatterT - scatterDelay) / max(1.0 - scatterDelay, 0.2), 0.0, 1.0);
            settle = 1.0 - localT * localT; // ease-in shatter
        }

        // Target (final sentence) slot, with a whisper of idle bob.
        vec2 targetPos = vec2(startX + fi * kern, 0.0);
        targetPos.y += 0.006 * sizeMul * sin(TIME * 0.7 + fi * 1.7);

        // Scattered origin: evenly spread via golden-angle turns, radius
        // varies per-glyph so arrivals feel organic, not radial-uniform.
        // Personal FFT bin (frequency->space) nudges each glyph's own
        // scatter radius so the burst has spectral texture, not lockstep.
        float ang = seed.y * 6.2832 + fi * 2.39996;
        float band = fftLog(seed.x);
        float rad = mix(0.85, 1.55, seed.x) * max(aspect, 1.0) * clamp(scatterSpread, 0.3, 3.0);
        rad *= 1.0 + 0.15 * band * reactAmt;
        vec2 scatterPos = vec2(cos(ang), sin(ang)) * rad;

        vec2 center = mix(scatterPos, targetPos, settle);

        float scaleT = clamp(settle, -0.05, 1.35);
        float scale  = mix(0.16, 1.0, scaleT);
        // Settled glyphs breathe with the bass — the sentence itself (the
        // dominant element, on screen for the whole hold) carries the audio.
        float settled = clamp(settle, 0.0, 1.0);
        scale *= 1.0 + 0.14 * audioBass * reactAmt * settled;
        float rotT   = clamp(settle, -0.1, 1.2);
        float rotStart = (seed.y - 0.5) * 2.6;
        float rot = mix(rotStart, 0.0, rotT);

        vec2 rel = p - center;
        float ca = cos(-rot), sa = sin(-rot);
        vec2 relR = vec2(rel.x * ca - rel.y * sa, rel.x * sa + rel.y * ca);
        vec2 relS = relR / max(scale, 0.001);

        vec2 cuv = vec2(0.5 + relS.x / charW, 0.5 + relS.y / charH);
        if (cuv.x < 0.0 || cuv.x > 1.0 || cuv.y < 0.0 || cuv.y > 1.0) continue;

        float s = sampleChar(ch, cuv);
        s = smoothstep(0.18, 0.55, s);
        if (s <= 0.001) continue;

        float flight = 1.0 - clamp(settle, 0.0, 1.0);
        vec3 inkBase = mix(colorA.rgb, colorB.rgb, seed.y);
        vec3 charCol = mix(inkBase, glowColor.rgb, flight * 0.85);

        // Continuous band-following luminance on the settled ink — LINEAR
        // bands (the old pow-crushed knees flattened ambient/jazz swells;
        // the ink colors have headroom so this doesn't clip).
        charCol *= 1.0 + settled * reactAmt * (0.45 * audioBass + 0.25 * audioMid);

        // Sparkle glint right at the settle moment — highs gate a sparse,
        // sub-strobe highlight (never a jolt: gaussian window, capped low).
        float settleWin = exp(-pow((settle - 1.0) * 5.5, 2.0));
        float sparkle = settleWin * (0.10 + 0.55 * highP * reactAmt);
        charCol += vec3(1.0, 0.95, 0.85) * sparkle;
        // Beat flash riding the same narrow window — event-only, eased.
        charCol += glowColor.rgb * (punch * punch) * 0.22 * reactAmt * settleWin;

        if (texMix > 0.001) {
            vec3 texCol = texture2D(inputTex, uv).rgb;
            charCol = mix(charCol, charCol * 0.4 + texCol * 1.1, texMix);
        }

        float charAlpha = s * mix(0.4, 1.0, clamp(settle, 0.0, 1.0));
        col = mix(col, charCol, charAlpha);
        if (transparentBg) alpha = max(alpha, charAlpha);
    }

    gl_FragColor = vec4(ucApply(col), clamp(alpha, 0.0, 1.0));
}
