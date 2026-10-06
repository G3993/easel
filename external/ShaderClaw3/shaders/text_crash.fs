/*{
  "DESCRIPTION": "Text Crash — the message lives as rigid letterforms that shatter into displaced strips and blocks on beat/punch hits, chromatic edges tearing apart, then settle back to a clean reassembled line between hits. Black stage, cyan/magenta impact palette.",
  "CREDIT": "ShaderClaw — original glitch-impact/shatter-reassemble concept",
  "CATEGORIES": [
    "Generator",
    "Text",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "shatterAmount",
      "LABEL": "Shatter Amount",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "colorA",
      "LABEL": "Ink Color",
      "TYPE": "color",
      "DEFAULT": [
        0.15,
        0.92,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "colorB",
      "LABEL": "Impact Color",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.18,
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
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "ETHEREA",
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
      "DEFAULT": 0.9,
      "MIN": 0.5,
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
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": false,
      "GROUP": "Background"
    },
    {
      "NAME": "reactivity",
      "LABEL": "Beat Reactivity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "img",
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


// ═══════════════════════════════════════════════════════════════════════
// TEXT CRASH — rigid letterforms that shatter into displaced strips/blocks
// on beat hits, then reassemble between hits.
//
// No persistent buffer: every character's "impact" is an analytically
// decaying pulse (Law 2) evaluated straight from TIME, with a per-slot
// hash-based phase offset so letters don't shatter in lockstep (Law 3 —
// "own phase lag hash of element id"). A slow self-timed cadence keeps
// the piece alive in total silence (Law 7); audio (beat/punch) adds an
// extra, capped decaying contribution on top — roughly a third of the
// total motion, never a bare strobe (Law 6/7). Bass scales the big
// whole-letter displacement (Law 3: bass = big/global), mids scale the
// finer internal block-tearing (turbulence), highs drive a sparse
// chromatic-edge split (sparkle, Law 3).
// ═══════════════════════════════════════════════════════════════════════

#define SPACE_CH 26

// ─── Font atlas sampling (house convention: fontAtlasTex, 37-cell strip,
//     A-Z=0-25, space=26, 0-9=27-36) ─────────────────────────────────────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}

// ANGLE-safe character lookup — tent function ensures every msg_N uniform
// is always evaluated, preventing dead-code elimination on some drivers.
float getCharF(float idx) {
    float c = 0.0;
    c += msg_0  * max(0.0, 1.0 - abs(idx - 0.0));
    c += msg_1  * max(0.0, 1.0 - abs(idx - 1.0));
    c += msg_2  * max(0.0, 1.0 - abs(idx - 2.0));
    c += msg_3  * max(0.0, 1.0 - abs(idx - 3.0));
    c += msg_4  * max(0.0, 1.0 - abs(idx - 4.0));
    c += msg_5  * max(0.0, 1.0 - abs(idx - 5.0));
    c += msg_6  * max(0.0, 1.0 - abs(idx - 6.0));
    c += msg_7  * max(0.0, 1.0 - abs(idx - 7.0));
    c += msg_8  * max(0.0, 1.0 - abs(idx - 8.0));
    c += msg_9  * max(0.0, 1.0 - abs(idx - 9.0));
    c += msg_10 * max(0.0, 1.0 - abs(idx - 10.0));
    c += msg_11 * max(0.0, 1.0 - abs(idx - 11.0));
    c += msg_12 * max(0.0, 1.0 - abs(idx - 12.0));
    c += msg_13 * max(0.0, 1.0 - abs(idx - 13.0));
    c += msg_14 * max(0.0, 1.0 - abs(idx - 14.0));
    c += msg_15 * max(0.0, 1.0 - abs(idx - 15.0));
    c += msg_16 * max(0.0, 1.0 - abs(idx - 16.0));
    c += msg_17 * max(0.0, 1.0 - abs(idx - 17.0));
    c += msg_18 * max(0.0, 1.0 - abs(idx - 18.0));
    c += msg_19 * max(0.0, 1.0 - abs(idx - 19.0));
    c += msg_20 * max(0.0, 1.0 - abs(idx - 20.0));
    c += msg_21 * max(0.0, 1.0 - abs(idx - 21.0));
    c += msg_22 * max(0.0, 1.0 - abs(idx - 22.0));
    c += msg_23 * max(0.0, 1.0 - abs(idx - 23.0));
    c += msg_24 * max(0.0, 1.0 - abs(idx - 24.0));
    c += msg_25 * max(0.0, 1.0 - abs(idx - 25.0));
    c += msg_26 * max(0.0, 1.0 - abs(idx - 26.0));
    c += msg_27 * max(0.0, 1.0 - abs(idx - 27.0));
    c += msg_28 * max(0.0, 1.0 - abs(idx - 28.0));
    c += msg_29 * max(0.0, 1.0 - abs(idx - 29.0));
    c += msg_30 * max(0.0, 1.0 - abs(idx - 30.0));
    c += msg_31 * max(0.0, 1.0 - abs(idx - 31.0));
    c += msg_32 * max(0.0, 1.0 - abs(idx - 32.0));
    c += msg_33 * max(0.0, 1.0 - abs(idx - 33.0));
    c += msg_34 * max(0.0, 1.0 - abs(idx - 34.0));
    c += msg_35 * max(0.0, 1.0 - abs(idx - 35.0));
    c += msg_36 * max(0.0, 1.0 - abs(idx - 36.0));
    c += msg_37 * max(0.0, 1.0 - abs(idx - 37.0));
    c += msg_38 * max(0.0, 1.0 - abs(idx - 38.0));
    c += msg_39 * max(0.0, 1.0 - abs(idx - 39.0));
    c += msg_40 * max(0.0, 1.0 - abs(idx - 40.0));
    c += msg_41 * max(0.0, 1.0 - abs(idx - 41.0));
    c += msg_42 * max(0.0, 1.0 - abs(idx - 42.0));
    c += msg_43 * max(0.0, 1.0 - abs(idx - 43.0));
    c += msg_44 * max(0.0, 1.0 - abs(idx - 44.0));
    c += msg_45 * max(0.0, 1.0 - abs(idx - 45.0));
    c += msg_46 * max(0.0, 1.0 - abs(idx - 46.0));
    c += msg_47 * max(0.0, 1.0 - abs(idx - 47.0));
    return c;
}

int charCount() {
    int n = int(msg_len + 0.5);
    if (n < 1) n = 1;
    if (n > 48) n = 48;
    return n;
}

float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }

// Soft-knee audio conditioning (playbook standard snippet).
float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

// ─── Per-frame globals set once in main(), read by glyphAt() ───────────
float g_wordW, g_kern, g_charW, g_charH;
float g_bassP, g_midP, g_highP, g_beatP, g_punchP, g_drive;
float g_shatterAmt, g_reactAmt, g_crashHz;
int   g_numChars;

// Resolve the glyph coverage at local (aspect-corrected, centered) point
// `pIn`. Internally: (1) guess which character cell owns this pixel from
// the UNDISPLACED position, (2) derive that character's own decaying
// impact pulse + random fly-apart direction, (3) pull the sample point
// back by the resulting offset so the letter appears to have moved
// forward by that offset, (4) tear the glyph into horizontal blocks that
// shift independently for the "shattered into blocks" read.
float glyphAt(vec2 pIn) {
    float lx0 = pIn.x + g_wordW * 0.5;
    float ly0 = g_charH * 0.5 - pIn.y;
    if (lx0 < -g_charW || lx0 > g_wordW + g_charW || ly0 < -g_charH || ly0 > g_charH * 2.0) return 0.0;

    int slot0 = int(floor(lx0 / g_kern));
    if (slot0 < 0) slot0 = 0;
    if (slot0 > g_numChars - 1) slot0 = g_numChars - 1;
    float slotF = float(slot0);

    // Self-timed decaying impact pulse (Law 2), staggered per character
    // (Law 3) so the shatter travels across the word instead of snapping
    // in lockstep. Idle cadence keeps it alive in silence (Law 7).
    float seedPhase  = hash11(slotF * 17.13 + 4.0);
    float localClock = TIME * g_crashHz - seedPhase * 0.75;
    float cyclePos    = fract(localClock);
    float selfShatter = exp(-cyclePos * 6.0);

    float audioShatter = max(g_beatP, g_punchP) * 0.9;
    float impact = selfShatter + audioShatter * g_reactAmt * (0.4 + 0.35 * g_drive);
    impact = clamp(impact, 0.0, 1.6);
    impact = max(impact, 0.05); // idle floor: always a little alive, never frozen

    // r2 ambient fix: continuous smooth band-follow — LINEAR raw bands (they
    // arrive pre-smoothed; the kneed g_bassP/g_midP crushed ambient's slow
    // swells) and no extra dilution. Beatless music still leans letters out
    // and settles them back; silence adds 0.
    impact += (0.30 * audioBass + 0.18 * audioMid) * g_reactAmt;

    // Whole-letter fly-apart: bass scales magnitude (big/global — Law 3).
    float ang = hash11(slotF * 3.7 + 91.0) * 6.2831853;
    vec2  dir = vec2(cos(ang), sin(ang));
    float dispMag = g_shatterAmt * g_charW * (0.45 + 1.15 * g_bassP) * impact;
    vec2  offset  = dir * dispMag;

    vec2 q  = pIn - offset;
    float lx = q.x + g_wordW * 0.5;
    float ly = g_charH * 0.5 - q.y;
    if (lx < -g_charW * 0.5 || lx > g_wordW + g_charW * 0.5 ||
        ly < -g_charH * 0.5 || ly > g_charH * 1.5) return 0.0;

    int slot = int(floor(lx / g_kern));
    if (slot < 0) slot = 0;
    if (slot > g_numChars - 1) slot = g_numChars - 1;

    float cellX = fract(lx / g_kern);
    float gc = cellX * (g_kern / g_charW) * 5.0;
    float gr = (ly / g_charH) * 7.0;
    if (gc < -1.5 || gc > 6.5 || gr < -1.5 || gr > 8.5) return 0.0;

    // Block tearing: split the glyph into 4 horizontal strips, each
    // shifted independently — mids scale the amount (turbulence/detail,
    // Law 3). Block membership uses the UNDISPLACED row so it stays
    // stable while the content offset carries the tear.
    float grBlock = clamp(gr, 0.0, 6.999);
    float blockIdx = floor(grBlock / (7.0 / 4.0));
    float blockJitter = hash11(blockIdx * 13.1 + slotF * 7.7 + 2.0) * 2.0 - 1.0;
    float tearAmt = g_shatterAmt * g_midP * impact;
    gc += blockJitter * tearAmt * 2.4;
    gr += blockJitter * tearAmt * 0.5;

    int ch = int(getCharF(slotF) + 0.5);
    if (ch < 0 || ch > 36 || ch == SPACE_CH) return 0.0;

    vec2 cuv = vec2(gc / 5.0, 1.0 - gr / 7.0);
    float raw = sampleChar(ch, cuv);
    return smoothstep(0.12, 0.55, raw);
}

void main() {
    vec2 res = RENDERSIZE.xy;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    g_numChars = charCount();

    // ── Audio conditioning (playbook laws 3/6/7) ─────────────────────
    g_bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);   // structural weight -> big displacement
    g_midP   = pow(knee(audioMid,  0.08, 0.85), 1.3);   // turbulence -> block tear amount
    g_highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);   // sparkle -> chroma edge split
    g_beatP  = audioBeatPulse * audioBeatPulse;         // event-only, squared
    g_punchP = pow(knee(audioPunch, 0.08, 0.85), 1.5);
    g_drive  = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9); // never zero — idle floor

    g_shatterAmt = clamp(shatterAmount, 0.0, 1.0);
    g_reactAmt   = clamp(reactivity, 0.0, 2.0);
    g_crashHz    = 0.45 * (0.85 + 0.5 * g_drive); // idle cadence, ~twice per 4.4s at rest

    // ── Layout: single centered line, auto-fit like the house convention ──
    float ts = max(textScale, 0.05);
    float kn = max(kerning, 0.05);
    float charH = 0.16 * ts;
    if (aspect < 1.0) charH *= aspect;
    float charW = charH * (5.0 / 7.0);
    float gap   = charW * 0.35 * kn;
    float kern  = charW + gap;
    float wordW = float(g_numChars) * kern;
    float maxW  = aspect * 0.92;
    if (wordW > maxW) {
        float sc = maxW / wordW;
        charH *= sc; charW = charH * (5.0 / 7.0);
        gap = charW * 0.35 * kn; kern = charW + gap;
        wordW = float(g_numChars) * kern;
    }
    // Whole-word size breathing (r2 ambient fix): geometry/coverage moves
    // many pixels even on a near-black stage where brightness gain can't.
    // Linear pre-smoothed bands; silence = exactly 1.0.
    float sizeBr = 1.0 + (0.10 * audioBass + 0.06 * audioMid) * g_reactAmt;
    charH *= sizeBr; charW *= sizeBr; gap *= sizeBr; kern *= sizeBr; wordW *= sizeBr;
    g_charH = charH; g_charW = charW; g_kern = kern; g_wordW = wordW;

    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = (uv.y - 0.5);

    // Sparse chromatic-edge split on high-impact hits (highs = fine
    // peripheral sparkle, Law 3) — never a full-screen rainbow, just a
    // thin tear line along the letterform edges.
    float globalPunch = max(g_beatP, g_punchP);
    vec2 chroma = vec2(0.0028 * aspect, 0.0009) * g_highP * (0.25 + 1.0 * globalPunch);

    float mR = glyphAt(p + chroma);
    float mG = glyphAt(p);
    float mB = glyphAt(p - chroma);
    float mask = max(mR, max(mG, mB));

    // Curated 2-hue palette + white-hot core on strong impact — never
    // rainbow, edges fringe toward the impact color where channels
    // diverge (i.e. where the tear is actually happening).
    float fringe = clamp(abs(mR - mB) * 3.0, 0.0, 1.0);
    vec3 textCol = mix(colorA.rgb, colorB.rgb, fringe);
    float hot = clamp(globalPunch * 1.3, 0.0, 1.0);
    textCol = mix(textCol, vec3(1.0), hot * 0.4 * mask);

    // Continuous ink brightness breath (r2: linear bands, deeper) — follows
    // smoothed bass/mid/high directly, multiplicative around 1.0 so silence
    // is untouched.
    textCol *= 1.0 + (0.25 * audioBass + 0.12 * audioMid + 0.08 * g_highP) * g_reactAmt;

    // ── Optional background texture, gated behind explicit texMix
    //    (house convention — never inferred from IMG_SIZE). A restrained
    //    scanline-style glitch rides along with impact so the backdrop
    //    itself feels caught in the same crash, without ever strobing.
    float bandNoise = hash11(floor(uv.y * 36.0) + floor(TIME * 5.0) * 3.1);
    vec2 bgUv = uv;
    bgUv.x += (bandNoise - 0.5) * 0.012 * clamp(globalPunch + 0.1, 0.0, 1.0);
    vec3 imgCol = IMG_NORM_PIXEL(img, fract(bgUv)).rgb;
    vec3 bg = mix(bgColor.rgb, imgCol * 0.55 + bgColor.rgb * 0.15, clamp(texMix, 0.0, 1.0));

    vec3 col = mix(bg, textCol, mask);
    // Faint ambient wash from a big hit, capped low so black stays black
    // between crashes (Law 7 — sound-off test).
    col += colorB.rgb * 0.05 * hot * (1.0 - mask);
    // r2 ambient fix: faint additive stage wash riding the linear bands —
    // dark pixels get an absolute lift (multiplication can't move near-black).
    // Silence adds exactly 0.
    col += (colorA.rgb * 0.5 + colorB.rgb * 0.5) * 0.06
         * (0.7 * audioBass + 0.3 * audioMid) * g_reactAmt * (1.0 - mask);

    if (transparentBg) {
        gl_FragColor = vec4(ucApply(textCol), mask);
    } else {
        gl_FragColor = vec4(ucApply(col), 1.0);
    }
}
