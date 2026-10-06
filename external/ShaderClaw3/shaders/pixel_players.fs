/*{
  "DESCRIPTION": "Pixel Players — a flock of small pixelated face icons drifting around the screen on a flow field. Each player gets its own seed, palette, gaze, mouth, and accessories. The msg text input auto-binds to the live transcript: each character of what the user says spawns and seeds a player, mouths flap when text is present, eyebrows raise on high-frequency hits, bodies squash on bass.",
  "CREDIT": "ShaderClaw — Lu / Pixel Players",
  "CATEGORIES": [
    "Generator",
    "Character",
    "Audio",
    "Text"
  ],
  "INPUTS": [
    {
      "NAME": "seed",
      "LABEL": "Seed",
      "TYPE": "float",
      "DEFAULT": 17,
      "MIN": 0,
      "MAX": 1000
    },
    {
      "NAME": "showHair",
      "LABEL": "Hair Tuft",
      "TYPE": "bool",
      "DEFAULT": 1
    },
    {
      "NAME": "accessories",
      "LABEL": "Accessories",
      "TYPE": "bool",
      "DEFAULT": 1
    },
    {
      "NAME": "fleckChance",
      "LABEL": "Fleck Chance",
      "TYPE": "float",
      "DEFAULT": 0.86,
      "MIN": 0.5,
      "MAX": 1
    },
    {
      "NAME": "mouthMode",
      "LABEL": "Mouth",
      "TYPE": "long",
      "DEFAULT": 2,
      "VALUES": [
        0,
        1,
        2
      ],
      "LABELS": [
        "Off",
        "Always Talking",
        "Audio + Text"
      ]
    },
    {
      "NAME": "playerCount",
      "LABEL": "Players",
      "TYPE": "float",
      "DEFAULT": 9,
      "MIN": 1,
      "MAX": 16,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "playerSize",
      "LABEL": "Size",
      "TYPE": "float",
      "DEFAULT": 0.18,
      "MIN": 0.06,
      "MAX": 0.4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "gridX",
      "LABEL": "Pixel Grid",
      "TYPE": "float",
      "DEFAULT": 14,
      "MIN": 8,
      "MAX": 28,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "faceAspect",
      "LABEL": "Head Aspect",
      "TYPE": "float",
      "DEFAULT": 1.18,
      "MIN": 0.8,
      "MAX": 1.6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "headRadius",
      "LABEL": "Head Size",
      "TYPE": "float",
      "DEFAULT": 0.36,
      "MIN": 0.18,
      "MAX": 0.55,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "eyeGap",
      "LABEL": "Eye Gap",
      "TYPE": "float",
      "DEFAULT": 0.18,
      "MIN": 0.08,
      "MAX": 0.35,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "eyeRow",
      "LABEL": "Eye Row",
      "TYPE": "float",
      "DEFAULT": 0.46,
      "MIN": 0.25,
      "MAX": 0.75,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "bigEyes",
      "LABEL": "Big Eyes",
      "TYPE": "bool",
      "DEFAULT": 0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "useFlowField",
      "LABEL": "Flow-field Drift",
      "TYPE": "bool",
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "driftAmt",
      "LABEL": "Drift Range",
      "TYPE": "float",
      "DEFAULT": 0.22,
      "MIN": 0,
      "MAX": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "driftSpeed",
      "LABEL": "Drift Speed",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "morphSpeed",
      "LABEL": "Morph Speed",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 2.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "shapeWobble",
      "LABEL": "Shape Wobble",
      "TYPE": "float",
      "DEFAULT": 0.32,
      "MIN": 0,
      "MAX": 0.8,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "gazeSpeed",
      "LABEL": "Gaze Speed",
      "TYPE": "float",
      "DEFAULT": 0.9,
      "MIN": 0,
      "MAX": 4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "blinkChance",
      "LABEL": "Blink Chance",
      "TYPE": "float",
      "DEFAULT": 0.94,
      "MIN": 0.85,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "squashAmt",
      "LABEL": "Squash & Stretch",
      "TYPE": "float",
      "DEFAULT": 0.18,
      "MIN": 0,
      "MAX": 0.6,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "playerArche",
      "LABEL": "Archetype",
      "TYPE": "long",
      "DEFAULT": 8,
      "VALUES": [
        0,
        1,
        2,
        3,
        4,
        5,
        6,
        7,
        8
      ],
      "LABELS": [
        "Warm",
        "Peach",
        "Reef",
        "Rainbow A",
        "Ocean",
        "Rainbow B",
        "Magenta",
        "Pastel",
        "Per-player Random"
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "colorSteps",
      "LABEL": "Color Bands",
      "TYPE": "float",
      "DEFAULT": 4,
      "MIN": 2,
      "MAX": 9,
      "GROUP": "Color"
    },
    {
      "NAME": "eyeColor",
      "LABEL": "Eye White",
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
      "NAME": "pupilColor",
      "LABEL": "Pupil",
      "TYPE": "color",
      "DEFAULT": [
        0.04,
        0.04,
        0.04,
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
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "PIXEL PLAYERS",
      "MAX_LENGTH": 16,
      "GROUP": "Text",
      "LABEL": "Message"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0.961,
        0.937,
        0.882,
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
      "NAME": "eyebrowReact",
      "LABEL": "Eyebrow Raise",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "audioBassPump",
      "LABEL": "Bass Pump",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 2.5,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "audioMidColor",
      "LABEL": "Mid Color Drift",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "audioHighSparkle",
      "LABEL": "High Sparkle",
      "TYPE": "float",
      "DEFAULT": 0.8,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

#ifdef GL_ES
precision highp float;
#endif

#define PI  3.14159265358979
#define TAU 6.28318530718
#define MAX_PLAYERS 16

// ───────── hashing / noise ─────────────────────────────────────
float hash11(float x){ return fract(sin(x * 127.1) * 43758.5453); }
float hash12(vec2 p){
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}
float vnoise(vec2 p){
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f*f*(3.0 - 2.0*f);
    return mix(mix(hash12(i+vec2(0.0,0.0)), hash12(i+vec2(1.0,0.0)), u.x),
               mix(hash12(i+vec2(0.0,1.0)), hash12(i+vec2(1.0,1.0)), u.x), u.y);
}
float fbm(vec2 p){
    float a = 0.5, v = 0.0;
    for (int i = 0; i < 5; i++){ v += a * vnoise(p); p = p * 2.03 + 17.0; a *= 0.5; }
    return v;
}

// ───────── IQ cosine palette ──────────────────────────────────
vec3 cosPal(float t, vec3 a, vec3 b, vec3 c, vec3 d){
    return a + b * cos(TAU * (c * t + d));
}
vec3 paletteD(int arche, float s){
    if (arche == 0) return vec3(0.00, 0.10, 0.20);
    if (arche == 1) return vec3(0.30, 0.20, 0.20);
    if (arche == 2) return vec3(0.80, 0.90, 0.30);
    if (arche == 3) return vec3(0.66, 0.55, 0.85);
    if (arche == 4) return vec3(0.10, 0.40, 0.65);
    if (arche == 5) return vec3(0.00, 0.33, 0.67);
    if (arche == 6) return vec3(0.55, 0.20, 0.40);
    if (arche == 7) return vec3(0.25, 0.25, 0.75);
    return vec3(hash11(s * 1.13), hash11(s * 2.71), hash11(s * 3.91));
}
vec3 paletteFor(int arche, float s, float t){
    return cosPal(t, vec3(0.5), vec3(0.5), vec3(1.0), paletteD(arche, s));
}

// ───────── text msg helpers (msg_0..msg_15 are auto-injected) ──
float msgChar(int slot){
    if (slot ==  0) return msg_0;
    if (slot ==  1) return msg_1;
    if (slot ==  2) return msg_2;
    if (slot ==  3) return msg_3;
    if (slot ==  4) return msg_4;
    if (slot ==  5) return msg_5;
    if (slot ==  6) return msg_6;
    if (slot ==  7) return msg_7;
    if (slot ==  8) return msg_8;
    if (slot ==  9) return msg_9;
    if (slot == 10) return msg_10;
    if (slot == 11) return msg_11;
    if (slot == 12) return msg_12;
    if (slot == 13) return msg_13;
    if (slot == 14) return msg_14;
    if (slot == 15) return msg_15;
    return 0.0;
}
int msgLen(){
    // Host provides the real text length in msg_len and pads unused slots
    // with code 26 ("space"), so a "> 0.5" scan would always count all
    // MAX_PLAYERS slots. Trust the host-provided length. Compute in float so
    // this is valid whether the host injects msg_len as int or float.
    return int(clamp(float(msg_len), 0.0, float(MAX_PLAYERS)));
}

// ───────── one face, sampled in local UV ([0..1]^2) ────────────
vec4 renderFace(vec2 luv, float pseed, int arche, float pTime,
                float audMid, float audHigh, float audBass,
                float charCode){
    float gx = max(8.0, gridX);
    float gy = max(8.0, floor(gx * faceAspect + 0.5));
    vec2 grid = vec2(gx, gy);
    if (luv.x < 0.0 || luv.x > 1.0 || luv.y < 0.0 || luv.y > 1.0) return vec4(0.0);

    vec2 cell = floor(luv * grid);
    vec2 cuv  = (cell + 0.5) / grid;

    // Silhouette: noise-warped ellipse, head-shaped
    vec2  center = vec2(0.5, 0.52);
    vec2  d = (cuv - center) * vec2(1.35, 1.0);
    float baseR = length(d);
    float archeRand = hash11(pseed * 3.17);
    float edgeN = fbm(cuv * 2.4 + vec2(pseed * 5.3, pseed * 1.7) + pTime * 0.35);
    float radius = headRadius;
    float wob = shapeWobble * mix(0.6, 1.4, archeRand);
    float silh = step(baseR - edgeN * wob, radius);

    // optional hair tuft
    if (showHair) {
        float topMask = smoothstep(0.55, 0.10, cuv.y);
        float h = fbm(vec2(cuv.x * 4.0 + pseed * 11.0, pTime * 0.6 + pseed)) * topMask;
        silh = max(silh, step(0.55, h));
    }

    // Body color, palette-quantized + audio drift
    float colN = fbm(cuv * 3.2 + vec2(pseed * 4.1, pseed * 2.3) + pTime * 0.45);
    colN += audMid * audioMidColor * 0.18;
    float steps = max(2.0, floor(colorSteps));
    float qN = floor(colN * steps) / steps;
    vec3 col = paletteFor(arche, pseed, qN + pseed * 0.13);

    // Flecks
    float fleckThresh = clamp(fleckChance, 0.5, 1.0);
    float fleck = step(fleckThresh, hash12(cell + floor(pTime * 1.3) + pseed * 31.0));
    if (fleck > 0.5) col = paletteFor(arche, pseed, qN + 0.5);

    // Sparkle on highs
    float sparkN = hash12(cell + floor(TIME * 22.0) + pseed * 13.0);
    float sparkThresh = 1.0 - audHigh * audioHighSparkle * 0.18;
    float sparkle = step(sparkThresh, sparkN);
    col = mix(col, vec3(1.0), sparkle * 0.85);

    // ── Eye geometry ─────────────────────────────────────────
    float eyeRowF = floor(grid.y * eyeRow);
    float gap     = max(2.0, floor(grid.x * eyeGap));
    float midX    = floor(grid.x * 0.5);
    float lex     = midX - gap - 1.0;
    float rex     = midX + gap;
    float eyeW    = bigEyes ? 2.0 : 1.0;
    float eyeH    = bigEyes ? 1.0 : 0.0;

    // Eyebrow raise: bass spike or high-freq hit shifts eye row up by 1 cell
    float brow = clamp((audBass * 1.2 + audHigh * 1.5) * eyebrowReact, 0.0, 1.0);
    float browLift = step(0.55, brow);
    float eyeRowEff = eyeRowF - browLift;

    bool inLeftEye  = (cell.y >= eyeRowEff && cell.y <= eyeRowEff + eyeH) &&
                      (cell.x >= lex     && cell.x <= lex + eyeW);
    bool inRightEye = (cell.y >= eyeRowEff && cell.y <= eyeRowEff + eyeH) &&
                      (cell.x >= rex     && cell.x <= rex + eyeW);

    // Eyebrows: 1-cell line above each eye when reacting
    bool inLeftBrow  = browLift > 0.5 && cell.y == eyeRowEff - 1.0 &&
                       cell.x >= lex - 1.0 && cell.x <= lex + eyeW;
    bool inRightBrow = browLift > 0.5 && cell.y == eyeRowEff - 1.0 &&
                       cell.x >= rex && cell.x <= rex + eyeW + 1.0;

    // Gaze
    float gazeT = TIME * gazeSpeed + pseed * 0.7;
    float gazeX = sin(gazeT + pseed * 4.7);
    float gazeY = sin(gazeT * 0.61 + pseed * 1.3);
    float pxOff = (gazeX > 0.0) ? eyeW : 0.0;
    float pyOff = (gazeY > 0.4 && bigEyes) ? 1.0 : 0.0;
    bool leftPupil  = inLeftEye  && cell.x == lex + pxOff && cell.y == eyeRowEff + pyOff;
    bool rightPupil = inRightEye && cell.x == rex + pxOff && cell.y == eyeRowEff + pyOff;

    // Body fill behind face features so they always sit on color
    bool faceBand = (cell.y >= eyeRowEff - 1.0 && cell.y <= eyeRowF + eyeH + 4.0) &&
                    (cell.x >= lex - 1.0       && cell.x <= rex + eyeW + 1.0);
    if (faceBand) silh = 1.0;

    // Per-player blink
    float blinkSlot = floor(TIME * 0.6 + pseed * 0.31);
    float blinkRand = hash11(blinkSlot * 17.0 + pseed * 9.7);
    float blinking  = step(blinkChance, blinkRand);

    // ── Mouth ─────────────────────────────────────────────────
    // Mouth row sits 3 cells below eyes
    float mouthRow  = eyeRowF + 3.0;
    float mouthCol0 = midX - 1.0;
    float mouthCol1 = midX;
    bool inMouth = (cell.y == mouthRow) && (cell.x == mouthCol0 || cell.x == mouthCol1);

    // talkPhase oscillates open/closed; charCode personalises rhythm
    float talkPhase = sin(TIME * 6.0 + pseed * 1.7 + charCode * 0.4);
    bool talking = false;
    int mouthModeI = int(mouthMode + 0.5);
    if (mouthModeI == 1) {
        talking = talkPhase > 0.0;                       // always talking
    } else if (mouthModeI == 2) {
        bool textPresent = charCode > 0.5;
        bool bassHit = audBass > 0.45;
        talking = (textPresent && talkPhase > -0.2) || bassHit;
    }
    bool mouthOpen = inMouth && talking;
    bool mouthClosed = inMouth && !talking && mouthModeI != 0;

    // ── Accessories (per-player, stable by seed) ──────────────
    bool inAntenna = false;
    bool inEarringL = false;
    bool inEarringR = false;
    bool inHat = false;
    if (accessories) {
        float a = hash11(pseed * 13.7);
        float topY = floor(grid.y * 0.16);
        // antenna: single cell sticking up from the top center
        if (a > 0.78) {
            inAntenna = (cell.x == midX) && (cell.y == topY - 1.0 || cell.y == topY - 2.0);
        }
        // hat: 2-row band across top
        else if (a > 0.55) {
            inHat = (cell.y == topY || cell.y == topY + 1.0) &&
                    (cell.x >= lex - 1.0 && cell.x <= rex + eyeW + 1.0);
        }
        // earrings: small accent cells at the cheek edges
        else if (a > 0.40) {
            inEarringL = (cell.y == eyeRowF + 2.0) && (cell.x == lex - 2.0);
            inEarringR = (cell.y == eyeRowF + 2.0) && (cell.x == rex + eyeW + 2.0);
        }
    }
    if (inAntenna || inHat || inEarringL || inEarringR) silh = 1.0;

    // ── Compose ───────────────────────────────────────────────
    vec3 final = col;

    // accessories painted in accent palette
    if (inHat)   final = paletteFor(arche, pseed * 1.7, 0.25);
    if (inAntenna) final = paletteFor(arche, pseed * 2.3, 0.7);
    if (inEarringL || inEarringR) final = paletteFor(arche, pseed * 3.1, 0.9);

    // brows
    if (inLeftBrow || inRightBrow) final = pupilColor.rgb;

    // eyes (override brows/accessories at eye cells)
    if (inLeftEye || inRightEye) final = blinking > 0.5 ? col : eyeColor.rgb;
    if ((leftPupil || rightPupil) && blinking < 0.5) final = pupilColor.rgb;

    // mouth (override at mouth cells)
    if (mouthOpen)   final = pupilColor.rgb;
    if (mouthClosed) final = mix(col, pupilColor.rgb, 0.35);

    return vec4(final, silh);
}

void main(){
    vec2 res = RENDERSIZE;
    float aspect = res.x / res.y;
    vec2 uv = isf_FragNormCoord.xy;
    uv.y = 1.0 - uv.y;

    // World: x in [0, aspect], y in [0,1]
    vec2 wpos = vec2(uv.x * aspect, uv.y);

    float bass = audioBass;
    float mid  = audioMid;
    float high = audioHigh;
    float lvl  = audioLevel;

    int   archeIn = int(playerArche);
    // playerCount is the AUTHORITATIVE number of players to render: the slider
    // directly sets it across its whole range, independent of message length.
    int   pcount  = int(clamp(floor(playerCount + 0.5), 1.0, float(MAX_PLAYERS)));
    // textLen only feeds per-player character selection (not the count).
    int   textLen = msgLen();

    float baseSize = playerSize;

    vec3  finalCol  = bgColor.rgb;
    float anyPlayer = 0.0;

    for (int i = 0; i < MAX_PLAYERS; ++i){
        if (i >= pcount) break;

        // Each player carries one character of msg, cycling the available
        // message chars across however many players the slider asked for.
        // Empty message -> charCode 0.0 (graceful blank, players still render).
        int   charSlot = (textLen > 0) ? int(mod(float(i), float(textLen))) : -1;
        float charCode = (charSlot >= 0) ? msgChar(charSlot) : 0.0;
        float pseed = seed + float(i) * 73.137 + charCode * 1.91;

        // Audio bass squash/stretch oscillation, per-player phase
        float bp = bass * audioBassPump;
        float oscPhase = TIME * 6.0 + pseed * 0.3;
        float squashY = 1.0 + squashAmt * bp * sin(oscPhase);
        float squashX = 1.0 - squashAmt * bp * sin(oscPhase) * 0.6;

        float sizeBob = baseSize * (0.85 + 0.18 * hash11(pseed * 0.91))
                                * (1.0 + 0.06 * sin(TIME * 1.3 + pseed))
                                * (1.0 + bp * 0.2);

        // Anchor inside safe area
        float marginX = sizeBob * 0.55;
        float marginY = sizeBob * 0.55 * faceAspect;
        vec2 anchor;
        anchor.x = marginX + hash11(pseed * 1.13) * (aspect - 2.0 * marginX);
        anchor.y = marginY + hash11(pseed * 2.71) * (1.0    - 2.0 * marginY);

        // Drift: flow-field via fbm sample, or fall back to Lissajous
        vec2 drift;
        if (useFlowField) {
            float t = TIME * driftSpeed * 0.18;
            float nx = fbm(vec2(pseed * 1.31 + t, pseed * 0.7));
            float ny = fbm(vec2(pseed * 2.17,     pseed * 1.7 + t));
            drift = (vec2(nx, ny) - 0.5) * driftAmt * 2.0;
        } else {
            float spd = (0.4 + 0.6 * hash11(pseed * 5.3)) * driftSpeed;
            drift = vec2(sin(TIME * spd * 0.71 + pseed * 3.1),
                         cos(TIME * spd * 0.59 + pseed * 4.7)) * driftAmt;
        }

        vec2 pcenter = anchor + drift;

        // Per-player face box, with audio squash/stretch
        vec2 boxHalf = vec2(sizeBob * 0.5 * squashX, sizeBob * 0.5 * faceAspect * squashY);

        // AABB cull
        vec2 dlt = wpos - pcenter;
        if (abs(dlt.x) > boxHalf.x || abs(dlt.y) > boxHalf.y) continue;

        // Local UV in [0..1]^2 inside the face box
        vec2 luv = (dlt / boxHalf) * 0.5 + 0.5;

        // Per-player archetype + time
        int   pArche = (archeIn == 8) ? int(mod(hash11(pseed * 7.91) * 8.0, 8.0)) : archeIn;
        float pTime  = TIME * morphSpeed + pseed * 0.13 + bp * 0.6;

        vec4 face = renderFace(luv, pseed, pArche, pTime, mid, high, bass, charCode);
        if (face.a > 0.5) {
            finalCol  = face.rgb * (1.0 + lvl * 0.22);
            anyPlayer = 1.0;
        }
    }

    float aOut = (anyPlayer > 0.5) ? 1.0
               : (transparentBg ? 0.0 : 1.0);

    // ---- universal color block (defaults = no-op; bgColor already native) ----
    float ucL = dot(finalCol, vec3(0.299, 0.587, 0.114));
    vec3 uc = mix(vec3(ucL), finalCol, colorBoost);
    if (hueShift > 0.0005) {
        float hueA = hueShift * 6.2831853;
        float hueC = cos(hueA), hueS = sin(hueA);
        mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                  + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                  + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hueM * uc, 0.0, 1.0);
    }
    gl_FragColor = vec4(uc, aOut);
}
