/*{
  "DESCRIPTION": "Danger — hazard warning-tape stripes animate diagonally behind/through the message, black-yellow high-contrast with a slow amber pulse and rare klaxon flash.",
  "CREDIT": "ShaderClaw — original hazard-tape text treatment",
  "CATEGORIES": [
    "Generator",
    "Text",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "stripeCount",
      "LABEL": "Stripe Density",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0.1,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "stripeAngle",
      "LABEL": "Stripe Angle",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "scrollSpeed",
      "LABEL": "Scroll Speed",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "stripeColorA",
      "LABEL": "Stripe Color A",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.78,
        0,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "stripeColorB",
      "LABEL": "Stripe Color B",
      "TYPE": "color",
      "DEFAULT": [
        0.05,
        0.04,
        0.02,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "textColor",
      "LABEL": "Text Color",
      "TYPE": "color",
      "DEFAULT": [
        0.05,
        0.04,
        0.02,
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
      "DEFAULT": "DANGER",
      "MAX_LENGTH": 48,
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
      "MAX": 2.2,
      "GROUP": "Text"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Punch Out Dark Stripes",
      "TYPE": "bool",
      "DEFAULT": false,
      "GROUP": "Background"
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
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "inputTex",
      "TYPE": "image",
      "LABEL": "Texture"
    },
    {
      "NAME": "texMix",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Texture Mix"
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


// ═══════════════════════════════════════════════════════════════════════
// DANGER — diagonal hazard-tape stripes scroll behind a stenciled warning
// message. Stripes cut straight through the letterforms (the tape reads
// as one continuous surface, text is just where it gets darker/lighter),
// with a slow amber breathing pulse and a rare, brief klaxon flash gated
// hard by audio so it never strobes on every beat.
// ═══════════════════════════════════════════════════════════════════════

// Atlas-only font sampling (A-Z=0-25, space=26, 0-9=27-36).
// row 0=top .. 7=bottom, atlas V=1 at top → invert.
float charPixel(int ch, float col, float row) {
    if (ch < 0 || ch > 36) return 0.0;
    if (col < 0.0 || col > 5.0 || row < 0.0 || row > 7.0) return 0.0;
    vec2 uv = vec2((float(ch) + col / 5.0) / 37.0, 1.0 - row / 7.0);
    return smoothstep(0.12, 0.55, texture2D(fontAtlasTex, uv).r);
}

int getChar(int slot) {
    if (slot == 0)  return int(msg_0);  if (slot == 1)  return int(msg_1);
    if (slot == 2)  return int(msg_2);  if (slot == 3)  return int(msg_3);
    if (slot == 4)  return int(msg_4);  if (slot == 5)  return int(msg_5);
    if (slot == 6)  return int(msg_6);  if (slot == 7)  return int(msg_7);
    if (slot == 8)  return int(msg_8);  if (slot == 9)  return int(msg_9);
    if (slot == 10) return int(msg_10); if (slot == 11) return int(msg_11);
    if (slot == 12) return int(msg_12); if (slot == 13) return int(msg_13);
    if (slot == 14) return int(msg_14); if (slot == 15) return int(msg_15);
    if (slot == 16) return int(msg_16); if (slot == 17) return int(msg_17);
    if (slot == 18) return int(msg_18); if (slot == 19) return int(msg_19);
    if (slot == 20) return int(msg_20); if (slot == 21) return int(msg_21);
    if (slot == 22) return int(msg_22); if (slot == 23) return int(msg_23);
    if (slot == 24) return int(msg_24); if (slot == 25) return int(msg_25);
    if (slot == 26) return int(msg_26); if (slot == 27) return int(msg_27);
    if (slot == 28) return int(msg_28); if (slot == 29) return int(msg_29);
    if (slot == 30) return int(msg_30); if (slot == 31) return int(msg_31);
    if (slot == 32) return int(msg_32); if (slot == 33) return int(msg_33);
    if (slot == 34) return int(msg_34); if (slot == 35) return int(msg_35);
    if (slot == 36) return int(msg_36); if (slot == 37) return int(msg_37);
    if (slot == 38) return int(msg_38); if (slot == 39) return int(msg_39);
    if (slot == 40) return int(msg_40); if (slot == 41) return int(msg_41);
    if (slot == 42) return int(msg_42); if (slot == 43) return int(msg_43);
    if (slot == 44) return int(msg_44); if (slot == 45) return int(msg_45);
    if (slot == 46) return int(msg_46); return int(msg_47);
}

int charCount() {
    int n = int(msg_len);
    return n > 0 ? n : 1;
}

float hash(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    // ─── Standard conditioning: soft knees + idle floor, never a strobe ───
    float audio  = clamp(audioReact, 0.0, 1.5);
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);   // structural weight -> stripe scroll
    float midP   = pow(knee(audioMid, 0.05, 0.92), 1.3);    // rock's sustained mids / jazz walking mids
    float highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);   // sparkle -> tape glint
    float drive  = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9); // idle floor, alive in silence
    float breatheAmt = 0.10 + 0.06 * drive; // idle breathing always present, audio widens it slightly
    // Decaying hit trace: every backbeat/accent leaves a brief eased glow.
    // NOT squared (round 2): squaring crushed jazz's soft 0.4-0.5 accents.
    float hitT   = audioBeatPulse;
    // Follower depth floor (round 2): scaling response by raw audioReact
    // (default 0.6) diluted the follower into invisibility for rock/jazz.
    float folD   = 0.6 + 0.4 * audio;

    // Klaxon flash: floor lowered to 0.25 + gentler curve so jazz's soft
    // swung accents can fire it too — still punch-gated, brief & eased.
    float flashRaw = pow(knee(audioPunch, 0.25, 0.90), 1.3);
    float flash = flashRaw * audio * 0.5; // audio is ~a third of total motion, flash stays subtle

    // ─── Diagonal hazard-tape stripes ───
    // Angle sweeps from steep to classic 45deg via stripeAngle.
    float ang = mix(0.15, 0.85, stripeAngle) * 3.14159265 * 0.5;
    vec2 dir = vec2(cos(ang), sin(ang));
    // Aspect-correct sampling axis so stripes stay straight on wide canvases.
    vec2 p = vec2(uv.x * aspect, uv.y);
    float along = dot(p, dir);

    float stripeFreq = mix(6.0, 26.0, stripeCount);
    // Round 3: the constant full-speed hard-edge stripe scroll flipped ~18%
    // of the canvas between colorA/colorB EVERY frame — a huge uncorrelated
    // delta source that drowned every brightness follower (rock corr 0).
    // Calm the baseline to ~1/3 speed and drive the rest with INTEGRATED
    // smoothed bass (audioBassTime only advances while bass plays): the
    // dominant motion now literally speeds up and slows down with the music,
    // phase-continuous, no jumps. Silence-safe: stripes still march at the
    // calm base rate.
    float scroll = (TIME * 0.35 + audioBassTime * 1.05) * scrollSpeed;
    float phase = along * stripeFreq - scroll * stripeFreq;
    float stripeMask = step(0.5, fract(phase));

    vec3 tapeCol = mix(stripeColorB.rgb, stripeColorA.rgb, stripeMask);

    // Slow amber breathing pulse across the whole tape (idle-alive, silence-safe).
    float breathe = 1.0 - breatheAmt + breatheAmt * (0.5 + 0.5 * sin(TIME * 0.6 + along * 1.3));
    tapeCol *= breathe;
    // Continuous LINEAR mid follower (rock = sustained mids, no knee) +
    // linear bass + decaying beat traces on the whole tape brightness —
    // silence multiplies by exactly 1.0, so the idle look is untouched.
    tapeCol *= 1.0 + folD * (0.26 * audioMid + 0.12 * audioBass + 0.18 * hitT);
    // Highs add a fine glint that rides the stripe edges only.
    float edgeGlint = highP * audio * 0.18 * smoothstep(0.42, 0.5, fract(phase)) * smoothstep(0.58, 0.5, fract(phase));
    tapeCol += edgeGlint;

    // Rare flash brightens the whole tape briefly (event-gated, not per-frame).
    tapeCol = mix(tapeCol, vec3(1.0, 0.98, 0.85), flash);

    // ─── Optional image blend — dissolved into the tape surface ───
    if (texMix > 0.001) {
        vec3 texCol = texture2D(inputTex, uv).rgb;
        vec3 blended = tapeCol * (0.55 + 0.45 * texCol);
        tapeCol = mix(tapeCol, blended, texMix);
    }

    // ─── Message text, stenciled through the tape ───
    int numChars = charCount();
    float charH = 0.16 * textScale;
    float charW = charH * (5.0 / 7.0);
    float kern  = charW * 1.05;
    float totalW = kern * float(numChars);

    vec2 centered = vec2((uv.x - 0.5) * aspect, uv.y - 0.5);
    float startX = -totalW * 0.5;
    float localX = centered.x - startX;
    float localY = centered.y + charH * 0.5;

    float pixel = 0.0;
    if (localY >= 0.0 && localY <= charH && localX >= 0.0 && localX < totalW) {
        int slot = int(floor(localX / kern));
        float cellX = localX - float(slot) * kern;
        float gc = (cellX / charW) * 5.0;
        float gr = (1.0 - localY / charH) * 7.0;
        if (slot >= 0 && slot < numChars && gc >= 0.0 && gc < 5.0) {
            int ch = getChar(slot);
            pixel = charPixel(ch, gc, gr);
        }
    }

    // Text ink stays high-contrast against the tape (dark ink on light
    // stripe zones, bright ink on dark stripe zones) so it reads at any
    // stripe phase — a hazard label stenciled INTO the tape, not floating
    // above it.
    float tapeLum = dot(tapeCol, vec3(0.299, 0.587, 0.114));
    vec3 inkAuto = mix(vec3(1.0), textColor.rgb, step(0.5, tapeLum));
    vec3 col = mix(tapeCol, inkAuto, pixel);

    // There's no empty backdrop to key out on a full-bleed tape, so
    // "transparent" instead punches out the dark stripe bands (colorB),
    // leaving only the bright hazard stripes + stenciled text opaque —
    // useful for compositing the tape as an overlay. Text ink always
    // stays opaque so the message reads regardless.
    float alpha = 1.0;
    if (transparentBg) alpha = clamp(max(stripeMask, pixel), 0.0, 1.0);

    col = ucApply(col);
    if (bgColor.a > 0.0005) {                      // universal background fill (a=0 -> no-op)
        col = mix(col, bgColor.rgb, bgColor.a * (1.0 - alpha));
        alpha = max(alpha, bgColor.a);
    }
    gl_FragColor = vec4(col, alpha);
}
