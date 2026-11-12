/*{
  "DESCRIPTION": "Stock Exchange — Shades of New York 008. The trading floor as light: an LED ticker tape scrolls quotes across the top (four-letter symbols, prices, green ▲ and red ▼), a chart of candlesticks with a glowing moving-average line scrolls through the middle, and a board of quotes fills the bottom with digits that flip as rows trade, in front of a hall of columns. The market has a mood: Auto drifts between bull and bear, or hold it Bull or Bear, or Crash & Rally where the drop crashes the newest candles. LED dots or solid pixels. Audio: the peak speeds trading up, beats flash the last price, the drop crashes the chart and flashes red. Body: Silhouette fills you with live quotes (green where the camera sees light, red in shadow); with Skeleton your bones glow amber and your raised hand moves the newest candles: lift it and the market rallies, lower it and it sells off.",
  "CREDIT": "Easel / ShaderClaw house shader — Shades of New York series, 2026-11-12 — original artwork.",
  "CATEGORIES": [
    "Generator",
    "Shades of New York",
    "Interactive",
    "Audio Reactive",
    "Body Tracking"
  ],
  "INPUTS": [
    {
      "NAME": "tickerSpeed",
      "LABEL": "Ticker Speed",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 3.0,
      "DEFAULT": 1.0,
      "GROUP": "Market"
    },
    {
      "NAME": "volatility",
      "LABEL": "Volatility",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 2.0,
      "DEFAULT": 1.0,
      "GROUP": "Market"
    },
    {
      "NAME": "candleCount",
      "LABEL": "Candles",
      "TYPE": "float",
      "MIN": 16.0,
      "MAX": 96.0,
      "DEFAULT": 48.0,
      "GROUP": "Market"
    },
    {
      "NAME": "boardRows",
      "LABEL": "Board Rows",
      "TYPE": "float",
      "MIN": 3.0,
      "MAX": 14.0,
      "DEFAULT": 7.0,
      "GROUP": "Market"
    },
    {
      "NAME": "mood",
      "LABEL": "Market Mood",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Auto",
        "Bull",
        "Bear",
        "Crash & Rally"
      ],
      "GROUP": "Market"
    },
    {
      "NAME": "ledStyle",
      "LABEL": "LED Style",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1
      ],
      "LABELS": [
        "LED Dots",
        "Solid Pixels"
      ],
      "GROUP": "Market"
    },
    {
      "NAME": "chartStyle",
      "LABEL": "Chart",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2
      ],
      "LABELS": [
        "Candles",
        "Line + Fill",
        "Neon Mountain"
      ],
      "GROUP": "Market"
    },
    {
      "NAME": "bigPrice",
      "LABEL": "Giant Index Digits",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.6,
      "GROUP": "Market"
    },
    {
      "NAME": "bodyMode",
      "LABEL": "Body Tracking",
      "TYPE": "long",
      "DEFAULT": 3,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Off",
        "Silhouette (you're made of live quotes)",
        "Skeleton (your raised hand moves the market)",
        "Both"
      ],
      "GROUP": "Body"
    },
    {
      "NAME": "bodyMirror",
      "LABEL": "Mirror (selfie view)",
      "TYPE": "bool",
      "DEFAULT": true,
      "GROUP": "Body"
    },
    {
      "NAME": "camTex",
      "LABEL": "Image / Camera",
      "TYPE": "image",
      "GROUP": "Camera"
    },
    {
      "NAME": "paletteMode",
      "LABEL": "Studio Palette",
      "TYPE": "long",
      "DEFAULT": 4,
      "VALUES": [
        0,
        1,
        2,
        3,
        4
      ],
      "LABELS": [
        "Colorful",
        "Red · Black · White",
        "Blue · Black · White",
        "Black & White",
        "NYC Reference"
      ],
      "GROUP": "Studio Palette"
    },
    {
      "NAME": "palAmount",
      "LABEL": "Palette Amount",
      "TYPE": "float",
      "AUDIO": false,
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 1.0,
      "GROUP": "Studio Palette"
    },
    {
      "NAME": "palRed",
      "LABEL": "Red Accent",
      "TYPE": "color",
      "DEFAULT": [
        0.92,
        0.04,
        0.07,
        1.0
      ],
      "GROUP": "Studio Palette"
    },
    {
      "NAME": "palBlue",
      "LABEL": "Blue Accent",
      "TYPE": "color",
      "DEFAULT": [
        0.07,
        0.32,
        1.0,
        1.0
      ],
      "GROUP": "Studio Palette"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.65,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "showState",
      "LABEL": "Show State",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Follow Music",
        "Rest",
        "Build",
        "Peak"
      ],
      "GROUP": "Show Dynamics"
    },
    {
      "NAME": "dynamics",
      "LABEL": "Build & Drop Depth",
      "TYPE": "float",
      "AUDIO": false,
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.85,
      "GROUP": "Show Dynamics"
    },
    {
      "NAME": "bgMode",
      "LABEL": "Background",
      "TYPE": "long",
      "DEFAULT": 1,
      "VALUES": [
        0,
        1,
        2
      ],
      "LABELS": [
        "Solid",
        "Vertical Gradient",
        "Radial Gradient"
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "bgColor",
      "LABEL": "BG Color / Top / Center",
      "TYPE": "color",
      "DEFAULT": [
        0.01,
        0.015,
        0.025,
        1.0
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "bgColor2",
      "LABEL": "BG Bottom / Edge",
      "TYPE": "color",
      "DEFAULT": [
        0.03,
        0.04,
        0.06,
        1.0
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent BG",
      "TYPE": "bool",
      "DEFAULT": false,
      "GROUP": "Background"
    }
  ]
}*/

float h11(float p) { p = fract(p * 0.1031); p *= p + 33.33; p *= p + p; return fract(p); }
float h12(vec2 p)  { vec3 p3 = fract(vec3(p.xyx) * 0.1031); p3 += dot(p3, p3.yzx + 33.33); return fract((p3.x + p3.y) * p3.z); }
vec2  h21(float p) { vec3 p3 = fract(vec3(p) * vec3(0.1031, 0.1030, 0.0973)); p3 += dot(p3, p3.yzx + 33.33); return fract((p3.xx + p3.yz) * p3.zy); }
float bassP() { return smoothstep(0.05, 0.85, audioBass); }
float midP()  { return smoothstep(0.08, 0.85, audioMid); }
float highP() { return smoothstep(0.08, 0.9, audioHigh); }
float lum(vec3 c) { return dot(c, vec3(0.2126, 0.7152, 0.0722)); }

vec3 studioPalette(vec3 c) {
    float m = float(paletteMode);
    if (palAmount <= 0.0 || m > 3.5) return c;                     // NYC Reference = the photo's own colours
    float L = clamp(lum(c), 0.0, 1.0);
    float mx = max(c.r, max(c.g, c.b)), mn = min(c.r, min(c.g, c.b));
    float sat = mx > 1e-4 ? (mx - mn) / mx : 0.0;
    vec3 o;
    if (m < 0.5) {
        vec3 hueC = pow((c - mn) / max(mx - mn, 1e-4), vec3(1.4));
        float s2 = 1.0 - pow(1.0 - sat, 4.0);
        float v = clamp(pow(mx, 0.85) * 1.12, 0.0, 1.0);
        o = mix(vec3(1.0), hueC, s2) * v;
        float hx = L * 1.7 + 0.04 * TIME;
        vec3 neon = pow(clamp(abs(fract(hx + vec3(0.0, 0.6667, 0.3333)) * 6.0 - 3.0) - 1.0, 0.0, 1.0), vec3(1.4));
        float wn = (1.0 - smoothstep(0.02, 0.1, sat)) * smoothstep(0.06, 0.25, L) * (1.0 - smoothstep(0.55, 0.85, L));
        o = mix(o, neon * v, wn);
        o = mix(o, vec3(1.0), smoothstep(0.85, 1.0, L) * 0.5);
    } else if (m > 2.5) {
        o = vec3(smoothstep(0.02, 0.95, pow(L, 0.9)));
    } else {
        vec3 acc = m < 1.5 ? palRed.rgb : palBlue.rgb;
        float t = pow(L, 0.8);
        vec3 ramp = t < 0.5 ? mix(vec3(0.0), acc, t * 2.0) : mix(acc, vec3(1.0), (t - 0.5) * 2.0);
        vec3 mono = vec3(smoothstep(0.0, 1.0, L));
        o = mix(mono, ramp, smoothstep(0.06, 0.4, sat));
    }
    return mix(c, o, palAmount);
}
vec3 backdrop(vec2 uv) {
    float m = float(bgMode);
    if (m < 0.5) return bgColor.rgb;
    if (m < 1.5) return mix(bgColor2.rgb, bgColor.rgb, smoothstep(0.0, 1.0, uv.y));
    vec2 dd = (uv - 0.5) * vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0);
    return mix(bgColor.rgb, bgColor2.rgb, smoothstep(0.0, 0.85, length(dd)));
}

// ── Show dynamics: rest → build → peak / drop ───────────────────────────
float gRest, gBuild, gPeak, gDrop, gLift;
void showDynamics() {
    float m = float(showState);
    float e = clamp(audioEnergy, 0.0, 1.0);
    float b = clamp(audioBuildup, 0.0, 1.0);
    float d = clamp(audioDrop, 0.0, 1.0);
    if (m > 0.5) {
        if (m < 1.5)      { e = 0.04; b = 0.0; d = 0.0; }
        else if (m < 2.5) { float r = fract(TIME / 8.0); e = 0.3 + 0.35 * r; b = r * r; d = 0.0; }
        else              { e = 1.0; b = 0.0; d = exp(-fract(TIME / 4.0) * 5.0); }
    }
    float k = dynamics;
    gBuild = b * k;
    gDrop  = d * k;
    gPeak  = smoothstep(0.5, 0.9, e) * k;
    gRest  = (1.0 - smoothstep(0.08, 0.4, e)) * (1.0 - b) * k;
    gLift  = mix(0.5, clamp(0.15 + e * 0.7 + gBuild * 0.25 + d * 0.3, 0.0, 1.0), k);
}

// ── Body layer (MediaPipe, ShaderClaw contract) ─────────────────────────
//   mpPoseLandmarks: 33×1 RGBA8 per joint (x, 1 − y, z + 0.5, visibility), x as the camera sees it
//   mpSegMask:       selfie-segmentation confidence (r), row 0 = top of the camera image
//   Both come from the host (ShaderClaw web MediaPipe / Easel vision); unfed = zeros = no body.
//   Joints and the mask are mirrored for a selfie view when Mirror is on, and every shader
//   samples its camera image through bodyCamUV() so picture, mask and skeleton line up.
vec2 bodyCamUV(vec2 uv) { return vec2(bodyMirror ? 1.0 - uv.x : uv.x, uv.y); }
vec3 bodyJoint(float i) {                       // (uv.x, uv.y, visibility)
    vec4 lm = texture2D(mpPoseLandmarks, vec2((i + 0.5) / 33.0, 0.5));
    return vec3(bodyMirror ? 1.0 - lm.r : lm.r, lm.g, lm.a);
}
float bodySeg(vec2 uv) {
    vec2 s = vec2(bodyMirror ? 1.0 - uv.x : uv.x, 1.0 - uv.y);
    return smoothstep(0.35, 0.65, texture2D(mpSegMask, clamp(s, 0.0, 1.0)).r);
}
// how sure we are there is a body (shoulders + hips visible)
float bodyPresence() {
    float v = bodyJoint(11.0).z + bodyJoint(12.0).z + bodyJoint(23.0).z + bodyJoint(24.0).z;
    return smoothstep(1.2, 2.6, v);
}
float bodyUseSkel() { float m = float(bodyMode); return (m > 1.5) ? 1.0 : 0.0; }          // Skeleton or Both
float bodyUseSil()  { float m = float(bodyMode); return (m > 0.5 && m < 1.5) || m > 2.5 ? 1.0 : 0.0; }   // Silhouette or Both
// distance (screen-height units, p-space) from p to the skeleton's bones; gBoneT = 0..1 along the nearest bone
float gBoneT, gBoneId;
float boneSeg(vec2 p, vec3 a, vec3 b, float asp, float id, float best) {
    if (a.z < 0.4 || b.z < 0.4) return best;
    vec2 A = (a.xy - 0.5) * vec2(asp, 1.0), B = (b.xy - 0.5) * vec2(asp, 1.0);
    vec2 pa = p - A, ba = B - A;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-6), 0.0, 1.0);
    float d = length(pa - ba * h);
    if (d < best) { gBoneT = h; gBoneId = id; return d; }
    return best;
}
float boneDist(vec2 p, float asp) {
    vec3 j0 = bodyJoint(0.0), j11 = bodyJoint(11.0), j12 = bodyJoint(12.0), j13 = bodyJoint(13.0), j14 = bodyJoint(14.0);
    vec3 j15 = bodyJoint(15.0), j16 = bodyJoint(16.0), j23 = bodyJoint(23.0), j24 = bodyJoint(24.0);
    vec3 j25 = bodyJoint(25.0), j26 = bodyJoint(26.0), j27 = bodyJoint(27.0), j28 = bodyJoint(28.0);
    vec3 neck = vec3(0.5 * (j11.xy + j12.xy), min(j11.z, j12.z));
    vec3 pelv = vec3(0.5 * (j23.xy + j24.xy), min(j23.z, j24.z));
    float d = 9.0;
    gBoneT = 0.0; gBoneId = -1.0;
    d = boneSeg(p, j0, neck, asp, 0.0, d);
    d = boneSeg(p, neck, pelv, asp, 1.0, d);
    d = boneSeg(p, j11, j12, asp, 2.0, d);
    d = boneSeg(p, j23, j24, asp, 3.0, d);
    d = boneSeg(p, j11, j13, asp, 4.0, d);
    d = boneSeg(p, j13, j15, asp, 5.0, d);
    d = boneSeg(p, j12, j14, asp, 6.0, d);
    d = boneSeg(p, j14, j16, asp, 7.0, d);
    d = boneSeg(p, j23, j25, asp, 8.0, d);
    d = boneSeg(p, j25, j27, asp, 9.0, d);
    d = boneSeg(p, j24, j26, asp, 10.0, d);
    d = boneSeg(p, j26, j28, asp, 11.0, d);
    return d;
}
// a point along one of three limb chains (0 wingspan, 1 head → spine → left leg, 2 right leg), t 0..1;
// returns p-space position, sets gLimbN (unit normal) and gLimbLen (chain length)
vec2 gLimbN;
float gLimbLen;
vec2 limbPt(float wk, float t, float asp) {
    vec3 P0, P1, P2, P3, P4, P5;
    vec3 j11 = bodyJoint(11.0), j12 = bodyJoint(12.0), j23 = bodyJoint(23.0), j24 = bodyJoint(24.0);
    vec3 neck = vec3(0.5 * (j11.xy + j12.xy), 1.0), pelv = vec3(0.5 * (j23.xy + j24.xy), 1.0);
    float n;
    if (wk < 0.5)      { P0 = bodyJoint(15.0); P1 = bodyJoint(13.0); P2 = j11; P3 = j12; P4 = bodyJoint(14.0); P5 = bodyJoint(16.0); n = 5.0; }
    else if (wk < 1.5) { P0 = bodyJoint(0.0); P1 = neck; P2 = pelv; P3 = j23; P4 = bodyJoint(25.0); P5 = bodyJoint(27.0); n = 5.0; }
    else               { P0 = pelv; P1 = j24; P2 = bodyJoint(26.0); P3 = bodyJoint(28.0); P4 = P3; P5 = P3; n = 3.0; }
    vec2 s = vec2(asp, 1.0);
    vec2 a0 = (P0.xy - 0.5) * s, a1 = (P1.xy - 0.5) * s, a2 = (P2.xy - 0.5) * s, a3 = (P3.xy - 0.5) * s, a4 = (P4.xy - 0.5) * s, a5 = (P5.xy - 0.5) * s;
    float l0 = length(a1 - a0), l1 = length(a2 - a1), l2 = length(a3 - a2), l3 = n > 3.5 ? length(a4 - a3) : 0.0, l4 = n > 4.5 ? length(a5 - a4) : 0.0;
    float L = max(l0 + l1 + l2 + l3 + l4, 1e-4);
    gLimbLen = L;
    float d = clamp(t, 0.0, 1.0) * L;
    vec2 A = a0, B = a1; float u = d / max(l0, 1e-5);
    if (d > l0)                { A = a1; B = a2; u = (d - l0) / max(l1, 1e-5); }
    if (d > l0 + l1)           { A = a2; B = a3; u = (d - l0 - l1) / max(l2, 1e-5); }
    if (d > l0 + l1 + l2 && n > 3.5)      { A = a3; B = a4; u = (d - l0 - l1 - l2) / max(l3, 1e-5); }
    if (d > l0 + l1 + l2 + l3 && n > 4.5) { A = a4; B = a5; u = (d - l0 - l1 - l2 - l3) / max(l4, 1e-5); }
    vec2 tg = normalize(B - A + 1e-5);
    gLimbN = vec2(-tg.y, tg.x);
    return mix(A, B, clamp(u, 0.0, 1.0));
}


// ── series library ───────────────────────────────────────────────────────
const float PI = 3.14159265;
mat2 rot(float a) { float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }
float vnoise(vec2 p) { vec2 i = floor(p), f = fract(p); f = f * f * (3.0 - 2.0 * f);
    return mix(mix(h12(i), h12(i + vec2(1.0, 0.0)), f.x), mix(h12(i + vec2(0.0, 1.0)), h12(i + 1.0), f.x), f.y); }
float fbm(vec2 p) { float s = 0.0, a = 0.5; for (int i = 0; i < 5; i++) { s += a * vnoise(p); p = rot(0.5) * p * 2.02 + vec2(1.7, 9.2); a *= 0.5; } return s; }
float fbm3(vec2 p) { float s = 0.0, a = 0.5; for (int i = 0; i < 3; i++) { s += a * vnoise(p); p = rot(0.5) * p * 2.02 + vec2(1.7, 9.2); a *= 0.5; } return s; }
vec3 hue(float h) { return clamp(abs(fract(h + vec3(0.0, 0.6667, 0.3333)) * 6.0 - 3.0) - 1.0, 0.0, 1.0); }
vec3 aces(vec3 x) { return clamp((x * (2.51 * x + 0.03)) / (x * (2.43 * x + 0.59) + 0.14), 0.0, 1.0); }
float sdBox(vec2 p, vec2 b) { vec2 d = abs(p) - b; return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0); }
float sdSeg(vec2 p, vec2 a, vec2 b) { vec2 pa = p - a, ba = b - a; float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0); return length(pa - ba * h); }
float sdEllipse(vec2 p, vec2 r) { float k = length(p / r); return (k - 1.0) * min(r.x, r.y); }
float aaStep(float d, float w) { return 1.0 - smoothstep(-w, w, d); }
// screen point (p-space: x in aspect units, centred) → camera UV with the selfie mirror, and the body helpers' p-space
vec2 pToUV(vec2 p) { return vec2(p.x * RENDERSIZE.y / RENDERSIZE.x, p.y) + 0.5; }
vec2 jointP(float i) { vec3 j = bodyJoint(i); return (j.xy - 0.5) * vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0); }
float jointV(float i) { return bodyJoint(i).z; }
vec3 camAt(vec2 uv) { return texture2D(camTex, bodyCamUV(uv)).rgb; }

// 5x7 dot-matrix font. code: 0-9 digits, then A-Z, then + - . % $ ^(up) v(down) space ! : * <(heart)
vec2 fontBits(float c) {
    if (c < 0.5) return vec2(476789.0, 26158.0);
    if (c < 1.5) return vec2(143492.0, 4238.0);
    if (c < 2.5) return vec2(476194.0, 4383.0);
    if (c < 3.5) return vec2(1017986.0, 1582.0);
    if (c < 4.5) return vec2(72018.0, 31810.0);
    if (c < 5.5) return vec2(1033153.0, 1582.0);
    if (c < 6.5) return vec2(205342.0, 17966.0);
    if (c < 7.5) return vec2(1016900.0, 8456.0);
    if (c < 8.5) return vec2(476718.0, 17966.0);
    if (c < 9.5) return vec2(476719.0, 1100.0);
    if (c < 10.5) return vec2(476735.0, 17969.0);
    if (c < 11.5) return vec2(1001022.0, 17982.0);
    if (c < 12.5) return vec2(476688.0, 16942.0);
    if (c < 13.5) return vec2(936497.0, 18012.0);
    if (c < 14.5) return vec2(1032734.0, 16927.0);
    if (c < 15.5) return vec2(1032734.0, 16912.0);
    if (c < 16.5) return vec2(476695.0, 17967.0);
    if (c < 17.5) return vec2(575039.0, 17969.0);
    if (c < 18.5) return vec2(462980.0, 4238.0);
    if (c < 19.5) return vec2(231490.0, 2636.0);
    if (c < 20.5) return vec2(576152.0, 21073.0);
    if (c < 21.5) return vec2(541200.0, 16927.0);
    if (c < 22.5) return vec2(585397.0, 17969.0);
    if (c < 23.5) return vec2(575285.0, 20017.0);
    if (c < 24.5) return vec2(476721.0, 17966.0);
    if (c < 25.5) return vec2(1001022.0, 16912.0);
    if (c < 26.5) return vec2(476721.0, 22093.0);
    if (c < 27.5) return vec2(1001022.0, 21073.0);
    if (c < 28.5) return vec2(508430.0, 1086.0);
    if (c < 29.5) return vec2(1020036.0, 4228.0);
    if (c < 30.5) return vec2(575025.0, 17966.0);
    if (c < 31.5) return vec2(575025.0, 17732.0);
    if (c < 32.5) return vec2(575029.0, 22186.0);
    if (c < 33.5) return vec2(574788.0, 10801.0);
    if (c < 34.5) return vec2(575018.0, 4228.0);
    if (c < 35.5) return vec2(1016900.0, 8735.0);
    if (c < 36.5) return vec2(4255.0, 4224.0);
    if (c < 37.5) return vec2(31.0, 0.0);
    if (c < 38.5) return vec2(0.0, 396.0);
    if (c < 39.5) return vec2(812100.0, 8803.0);
    if (c < 40.5) return vec2(147086.0, 6084.0);
    if (c < 41.5) return vec2(4575.0, 0.0);
    if (c < 42.5) return vec2(31.0, 14464.0);
    if (c < 43.5) return vec2(0.0, 0.0);
    if (c < 44.5) return vec2(135300.0, 4100.0);
    if (c < 45.5) return vec2(12672.0, 12672.0);
    if (c < 46.5) return vec2(11246.0, 32064.0);
    if (c < 47.5) return vec2(360447.0, 14464.0);
    return vec2(0.0);
}
// is the dot at column x (0..4, left→right) and row y (0..6, top→bottom) of glyph c on?
float fontDot(float c, float x, float y) {
    vec2 b = fontBits(c);
    float idx = y * 5.0 + x;                       // bit index from the top-left
    float v = idx < 20.0 ? b.x : b.y;
    float n = idx < 20.0 ? 19.0 - idx : 34.0 - idx;
    return mod(floor(v / exp2(n)), 2.0);
}
// glyph c in a cell, p in 0..1 (y up). Round LED dots (style 0) or solid pixels (style 1)
float glyph(float c, vec2 p, float style) {
    if (p.x < 0.0 || p.y < 0.0 || p.x >= 1.0 || p.y >= 1.0) return 0.0;
    vec2 g = vec2(p.x * 6.0, (1.0 - p.y) * 8.0) - vec2(0.5);   // 5x7 dots with a 1-dot gap
    vec2 id = floor(g + 0.5);
    if (id.x < 0.0 || id.x > 4.0 || id.y < 0.0 || id.y > 6.0) return 0.0;
    float on = fontDot(c, id.x, id.y);
    vec2 f = g - id;
    float sh = style < 0.5 ? 1.0 - smoothstep(0.32, 0.48, length(f)) : 1.0 - smoothstep(0.46, 0.52, max(abs(f.x), abs(f.y)));
    return on * sh;
}
float randChar(float h) { return 10.0 + floor(h * 26.0); }        // a random capital letter
float randDigit(float h) { return floor(h * 10.0); }
// simple hanzi as brush strokes: distance (box units) from p (0..1, y up) to glyph id's strokes
float hanziDist(float id, vec2 p) {
    float d = 9.0;
    if (id < 0.5) { d = min(d, sdSeg(p, vec2(0.5, 0.95), vec2(0.5, 0.05))); d = min(d, sdSeg(p, vec2(0.18, 0.72), vec2(0.18, 0.32))); d = min(d, sdSeg(p, vec2(0.18, 0.72), vec2(0.82, 0.72))); d = min(d, sdSeg(p, vec2(0.82, 0.72), vec2(0.82, 0.32))); d = min(d, sdSeg(p, vec2(0.18, 0.32), vec2(0.82, 0.32))); return d; }   // 中
    if (id < 1.5) { d = min(d, sdSeg(p, vec2(0.12, 0.62), vec2(0.88, 0.62))); d = min(d, sdSeg(p, vec2(0.5, 0.95), vec2(0.5, 0.62))); d = min(d, sdSeg(p, vec2(0.5, 0.62), vec2(0.14, 0.06))); d = min(d, sdSeg(p, vec2(0.5, 0.62), vec2(0.88, 0.06))); return d; }   // 大
    if (id < 2.5) { d = min(d, sdSeg(p, vec2(0.52, 0.95), vec2(0.5, 0.6))); d = min(d, sdSeg(p, vec2(0.5, 0.6), vec2(0.12, 0.06))); d = min(d, sdSeg(p, vec2(0.5, 0.6), vec2(0.9, 0.06))); return d; }   // 人
    if (id < 3.5) { d = min(d, sdSeg(p, vec2(0.18, 0.8), vec2(0.18, 0.18))); d = min(d, sdSeg(p, vec2(0.18, 0.8), vec2(0.82, 0.8))); d = min(d, sdSeg(p, vec2(0.82, 0.8), vec2(0.82, 0.18))); d = min(d, sdSeg(p, vec2(0.18, 0.18), vec2(0.82, 0.18))); return d; }   // 口
    if (id < 4.5) { d = min(d, sdSeg(p, vec2(0.24, 0.92), vec2(0.24, 0.08))); d = min(d, sdSeg(p, vec2(0.24, 0.92), vec2(0.76, 0.92))); d = min(d, sdSeg(p, vec2(0.76, 0.92), vec2(0.76, 0.08))); d = min(d, sdSeg(p, vec2(0.24, 0.08), vec2(0.76, 0.08))); d = min(d, sdSeg(p, vec2(0.24, 0.5), vec2(0.76, 0.5))); return d; }   // 日
    if (id < 5.5) { d = min(d, sdSeg(p, vec2(0.15, 0.88), vec2(0.15, 0.12))); d = min(d, sdSeg(p, vec2(0.15, 0.88), vec2(0.85, 0.88))); d = min(d, sdSeg(p, vec2(0.85, 0.88), vec2(0.85, 0.12))); d = min(d, sdSeg(p, vec2(0.15, 0.12), vec2(0.85, 0.12))); d = min(d, sdSeg(p, vec2(0.15, 0.5), vec2(0.85, 0.5))); d = min(d, sdSeg(p, vec2(0.5, 0.88), vec2(0.5, 0.12))); return d; }   // 田
    if (id < 6.5) { d = min(d, sdSeg(p, vec2(0.15, 0.88), vec2(0.85, 0.88))); d = min(d, sdSeg(p, vec2(0.22, 0.5), vec2(0.78, 0.5))); d = min(d, sdSeg(p, vec2(0.1, 0.1), vec2(0.9, 0.1))); d = min(d, sdSeg(p, vec2(0.5, 0.88), vec2(0.5, 0.1))); return d; }   // 王
    if (id < 7.5) { d = min(d, sdSeg(p, vec2(0.5, 0.92), vec2(0.5, 0.12))); d = min(d, sdSeg(p, vec2(0.15, 0.62), vec2(0.15, 0.12))); d = min(d, sdSeg(p, vec2(0.85, 0.62), vec2(0.85, 0.12))); d = min(d, sdSeg(p, vec2(0.15, 0.12), vec2(0.85, 0.12))); return d; }   // 山
    if (id < 8.5) { d = min(d, sdSeg(p, vec2(0.1, 0.68), vec2(0.9, 0.68))); d = min(d, sdSeg(p, vec2(0.5, 0.95), vec2(0.5, 0.05))); d = min(d, sdSeg(p, vec2(0.5, 0.68), vec2(0.12, 0.2))); d = min(d, sdSeg(p, vec2(0.5, 0.68), vec2(0.88, 0.2))); return d; }   // 木
    if (id < 9.5) { d = min(d, sdSeg(p, vec2(0.2, 0.88), vec2(0.8, 0.88))); d = min(d, sdSeg(p, vec2(0.1, 0.6), vec2(0.9, 0.6))); d = min(d, sdSeg(p, vec2(0.5, 0.88), vec2(0.5, 0.6))); d = min(d, sdSeg(p, vec2(0.5, 0.6), vec2(0.14, 0.06))); d = min(d, sdSeg(p, vec2(0.5, 0.6), vec2(0.88, 0.06))); return d; }   // 天
    if (id < 10.5) { d = min(d, sdSeg(p, vec2(0.45, 0.95), vec2(0.45, 0.1))); d = min(d, sdSeg(p, vec2(0.45, 0.55), vec2(0.82, 0.55))); d = min(d, sdSeg(p, vec2(0.1, 0.1), vec2(0.9, 0.1))); return d; }   // 上
    if (id < 11.5) { d = min(d, sdSeg(p, vec2(0.1, 0.9), vec2(0.9, 0.9))); d = min(d, sdSeg(p, vec2(0.45, 0.9), vec2(0.45, 0.05))); d = min(d, sdSeg(p, vec2(0.47, 0.62), vec2(0.78, 0.42))); return d; }   // 下
    if (id < 12.5) { d = min(d, sdSeg(p, vec2(0.1, 0.55), vec2(0.9, 0.55))); d = min(d, sdSeg(p, vec2(0.5, 0.95), vec2(0.5, 0.05))); return d; }   // 十
    if (id < 13.5) { d = min(d, sdSeg(p, vec2(0.2, 0.85), vec2(0.8, 0.85))); d = min(d, sdSeg(p, vec2(0.25, 0.5), vec2(0.75, 0.5))); d = min(d, sdSeg(p, vec2(0.1, 0.12), vec2(0.9, 0.12))); return d; }   // 三
    if (id < 14.5) { d = min(d, sdSeg(p, vec2(0.22, 0.95), vec2(0.25, 0.82))); d = min(d, sdSeg(p, vec2(0.08, 0.78), vec2(0.36, 0.78))); d = min(d, sdSeg(p, vec2(0.36, 0.78), vec2(0.12, 0.45))); d = min(d, sdSeg(p, vec2(0.24, 0.58), vec2(0.24, 0.05))); d = min(d, sdSeg(p, vec2(0.24, 0.5), vec2(0.38, 0.38))); d = min(d, sdSeg(p, vec2(0.48, 0.9), vec2(0.92, 0.9))); d = min(d, sdSeg(p, vec2(0.55, 0.78), vec2(0.55, 0.62))); d = min(d, sdSeg(p, vec2(0.55, 0.78), vec2(0.85, 0.78))); d = min(d, sdSeg(p, vec2(0.85, 0.78), vec2(0.85, 0.62))); d = min(d, sdSeg(p, vec2(0.55, 0.62), vec2(0.85, 0.62))); d = min(d, sdSeg(p, vec2(0.5, 0.5), vec2(0.5, 0.05))); d = min(d, sdSeg(p, vec2(0.5, 0.5), vec2(0.9, 0.5))); d = min(d, sdSeg(p, vec2(0.9, 0.5), vec2(0.9, 0.05))); d = min(d, sdSeg(p, vec2(0.5, 0.05), vec2(0.9, 0.05))); d = min(d, sdSeg(p, vec2(0.5, 0.28), vec2(0.9, 0.28))); d = min(d, sdSeg(p, vec2(0.7, 0.5), vec2(0.7, 0.05))); return d; }   // 福
    if (id < 15.5) { d = min(d, sdSeg(p, vec2(0.12, 0.85), vec2(0.12, 0.12))); d = min(d, sdSeg(p, vec2(0.12, 0.85), vec2(0.88, 0.85))); d = min(d, sdSeg(p, vec2(0.88, 0.85), vec2(0.88, 0.12))); d = min(d, sdSeg(p, vec2(0.12, 0.12), vec2(0.88, 0.12))); d = min(d, sdSeg(p, vec2(0.4, 0.85), vec2(0.32, 0.42))); d = min(d, sdSeg(p, vec2(0.6, 0.85), vec2(0.6, 0.5))); d = min(d, sdSeg(p, vec2(0.6, 0.5), vec2(0.82, 0.45))); return d; }   // 四
    return d;
}
const float HANZI_N = 16.0;


// the market: a price walk with a mood (bull, bear, or a crash on the drop and a rally after)
float gTrend;
float price(float x) {
    float v = 0.0;
    v += 0.22 * sin(x * 0.11 + 1.3) + 0.12 * sin(x * 0.29 + 4.0) + 0.06 * sin(x * 0.77 + 2.0);
    v += 0.05 * volatility * (vnoise(vec2(x * 0.9, 3.0)) - 0.5) * 2.0;
    v += 0.025 * volatility * (vnoise(vec2(x * 3.1, 7.0)) - 0.5) * 2.0;
    return v + gTrend * x * 0.004;
}

// LED text cell: glyph c drawn green, red or amber
float ledText(vec2 q, float c) { return glyph(c, q, float(ledStyle)); }

void main() {
    showDynamics();
    vec2 R = RENDERSIZE;
    vec2 uv = gl_FragCoord.xy / R;
    float asp = R.x / R.y;
    vec2 p = (uv - 0.5) * vec2(asp, 1.0);
    float ar = audioReact, t = TIME;
    float bodyOn = float(bodyMode) > 0.5 ? smoothstep(0.3, 0.7, bodyPresence()) : 0.0;
    float sil = bodyOn * bodyUseSil() * bodySeg(uv);
    float md = float(mood);
    gTrend = md < 0.5 ? sin(t * 0.05) * 1.5 : md < 1.5 ? 2.5 : md < 2.5 ? -2.5 : 0.0;
    vec3 up = vec3(0.1, 1.0, 0.45), dn = vec3(1.0, 0.18, 0.16), amb = vec3(1.0, 0.72, 0.15);
    p += (h21(floor(t * 30.0)) - 0.5) * 0.025 * gDrop;                     // the floor shakes on the crash

    vec3 col = backdrop(uv);
    float cover = 0.0;
    // the trading floor glow and a hall of columns behind the boards
    float colx = fract(p.x * 2.2 + 0.5);
    float column = smoothstep(0.08, 0.0, abs(colx - 0.5) - 0.12) * step(p.y, 0.48);
    col = mix(col, vec3(0.06, 0.065, 0.08) * (0.7 + 0.6 * pow(1.0 - abs(colx - 0.5) * 3.0, 2.0)), column * 0.8);
    col += vec3(0.05, 0.07, 0.1) * smoothstep(0.5, -0.5, p.y) * 0.5;

    // giant LED index digits counting behind everything
    if (bigPrice > 0.01) {
        float idx = floor(18000.0 + price(t * 0.8) * 4000.0 + t * 3.0);
        float ch = 0.34, cw = ch * 0.78;
        float n = 6.0;
        vec2 tq = vec2((p.x + n * cw * 0.5) / cw, (p.y + ch * 0.5 - 0.02) / ch);
        float ci = floor(tq.x);
        if (ci >= 0.0 && ci < n && tq.y > 0.0 && tq.y < 1.0) {
            float dig = ci < 4.5 ? mod(floor(idx / exp2(0.0) / pow(10.0, 4.0 - ci)), 10.0) : 38.0;
            if (ci > 4.5) dig = mod(floor(t * 7.0), 10.0);
            float on = glyph(dig, vec2(fract(tq.x), tq.y), 0.0);
            col += (gTrend >= 0.0 ? up : dn) * on * 0.22 * bigPrice * (0.7 + 0.3 * audioBeatPulse * ar);
        }
    }
    // ── the ticker tape across the top ──
    float th = 0.09;
    float ty = 0.5 - th * 0.5 - 0.02;
    if (abs(p.y - ty) < th * 0.5) {
        float ch = th * 0.62;                                          // character height
        float cw = ch * 0.75;
        float sx = p.x + t * tickerSpeed * 0.35 + 3.0;
        float slot = floor(sx / (cw * 14.0));                           // each quote: 4 letters, price, arrow, change
        float within = sx / cw - slot * 14.0;
        float ci = floor(within);
        vec2 q = vec2(fract(within), (p.y - (ty - ch * 0.5)) / ch);
        float hs = h11(slot * 3.3);
        float gain = step(0.5 + 0.2 * (-gTrend * 0.2), h11(slot * 7.7 + floor(t * 0.3)));
        float c = 43.0;                                                 // space
        if (ci < 4.0) c = randChar(h11(slot * 11.0 + ci * 1.7));
        else if (ci > 4.5 && ci < 7.5) c = randDigit(h11(slot * 5.0 + ci + floor(t * 2.0) * 0.1));
        else if (ci > 7.5 && ci < 8.5) c = 38.0;                       // .
        else if (ci > 8.5 && ci < 10.5) c = randDigit(h11(slot * 9.0 + ci + floor(t * 4.0)));
        else if (ci > 10.5 && ci < 11.5) c = gain > 0.5 ? 41.0 : 42.0; // ▲ ▼
        float on = ledText(q, c);
        vec3 tc = ci < 4.0 ? vec3(1.0, 0.95, 0.85) : (gain > 0.5 ? up : dn);
        col = mix(vec3(0.01, 0.012, 0.014), col, 0.15);
        col += tc * on * 1.2;
        // the LED matrix grain
        col *= 0.8 + 0.2 * step(0.25, fract(p.x * R.y / 3.0));
        cover = 1.0;
    }

    // ── the chart: candles and a glowing price line ──
    vec2 cb = vec2(asp * 0.46, 0.2);                                    // half-size of the chart window
    vec2 cc = vec2(0.0, 0.12);
    vec2 cq = (p - cc) / cb;                                            // -1..1
    if (abs(cq.x) < 1.0 && abs(cq.y) < 1.0) {
        vec3 cbg = vec3(0.015, 0.02, 0.03);
        float grid = max(step(0.97, fract(cq.x * 6.0 * 0.5 + 0.5)), step(0.96, fract(cq.y * 2.0 + 0.5)));
        cbg += vec3(0.05, 0.08, 0.1) * grid;
        float n = floor(candleCount);
        float xs = (cq.x * 0.5 + 0.5) * n;
        float k = floor(xs);
        float scroll = floor(t * (0.6 + 1.5 * gPeak));
        float x0 = k + scroll, x1 = x0 + 1.0;
        float o = price(x0), cl = price(x1);
        // your hand drives the newest candles
        if (bodyOn > 0.5 && bodyUseSkel() > 0.5 && mouseDown > 0.5) {
            float target = (mousePos.y - 0.5 - cc.y) / cb.y * 0.45;
            float w0 = smoothstep(n - 8.0, n, k), w1 = smoothstep(n - 8.0, n, k + 1.0);
            o = mix(o, target, w0); cl = mix(cl, target, w1);
        }
        if (gDrop > 0.05) { float crash = gDrop * 0.6 * smoothstep(n * 0.7, n, k); o -= crash; cl -= crash * 1.2; }
        float hi = max(o, cl) + 0.04 * h11(x0 * 3.1) * volatility;
        float lo = min(o, cl) - 0.04 * h11(x0 * 5.7) * volatility;
        float yv = cq.y * 0.45;                                         // the chart's value scale
        float fxk = fract(xs);
        float body = step(abs(fxk - 0.5), 0.32) * step(min(o, cl), yv) * step(yv, max(max(o, cl), min(o, cl) + 0.004));
        float wick = step(abs(fxk - 0.5), 0.05) * step(lo, yv) * step(yv, hi);
        vec3 cc2 = cl >= o ? up : dn;
        vec3 c = cbg + cc2 * max(body, wick * 0.8) * 0.9;
        // the moving-average line, glowing
        float ma = mix(price(x0 - 0.5) * 0.5 + price(x0 + 0.5) * 0.5, price(x0 + 1.5) * 0.5 + price(x0 + 0.5) * 0.5, fxk);
        float dl = abs(yv - ma) / 0.45 * cb.y;
        c += amb * (1.0 - smoothstep(0.0, 0.004, dl - 0.0015)) + amb * exp(-dl * 70.0) * 0.3;
        // last price tag
        c += (cl >= o ? up : dn) * exp(-abs(xs - n + 0.5) * 2.0) * 0.15 * (0.5 + audioBeatPulse * ar);
        float cs = float(chartStyle);
        if (cs > 0.5) {
            float val = mix(price(x0), price(x1), smoothstep(0.0, 1.0, fxk));
            if (gDrop > 0.05) val -= gDrop * 0.6 * smoothstep(n * 0.7, n, xs);
            float above = yv - val;
            float edgeL = 1.0 - smoothstep(0.0, 0.006, abs(above));
            if (cs < 1.5) {                                                // line + gradient fill
                vec3 lc = val >= price(x0 - 4.0) ? up : dn;
                c = cbg + lc * step(above, 0.0) * (0.06 + 0.3 * smoothstep(-0.45, 0.0, above)) + lc * edgeL * 1.4 + lc * exp(-abs(above) * 60.0) * 0.35;
            } else {                                                       // neon mountain with scanlines
                float fill = step(above, 0.0) * (0.5 + 0.5 * step(0.5, fract(cq.y * 40.0 - t * 2.0)));
                vec3 m1 = vec3(0.1, 0.9, 1.0), m2 = vec3(1.0, 0.2, 0.85);
                c = cbg + mix(m2, m1, smoothstep(-0.45, 0.0, above)) * fill * 0.35 + vec3(1.0) * edgeL * 1.2 + m1 * exp(-abs(above) * 45.0) * 0.6;
            }
        }
        col = mix(col, c, 0.95);
        // frame
        cover = 1.0;
    }
    float frame = abs(sdBox(p - cc, cb)) ;
    col += vec3(0.25, 0.35, 0.45) * (1.0 - smoothstep(0.0, 0.003, frame)) * 0.6;

    // ── the board: rows of quotes, digits flipping ──
    float by0 = -0.5 + 0.03, by1 = cc.y - cb.y - 0.04;
    if (p.y > by0 && p.y < by1) {
        float nrows = floor(boardRows);
        float rh = (by1 - by0) / nrows;
        float row = floor((p.y - by0) / rh);
        float ry = (p.y - by0) / rh - row;
        float ch = rh * 0.6;
        float cw = ch * 0.75;
        float ncol = floor(asp / (cw * 26.0)) + 1.0;
        float colW = asp / ncol;
        float cxw = (p.x + asp * 0.5) / colW;
        float bc = floor(cxw);
        float within = (cxw - bc) * colW / cw;
        float ci = floor(within);
        vec2 q = vec2(fract(within) * 1.0, (ry - 0.2) / 0.6);
        float id = row * 13.0 + bc * 71.0;
        float tick = floor(t * (1.5 + 3.0 * gPeak) + h11(id) * 10.0);
        float gain = step(0.5 - 0.1 * gTrend, h11(id + tick * 0.37));
        float c = 43.0;
        if (ci > 0.5 && ci < 4.5) c = randChar(h11(id + ci * 3.1));
        else if (ci > 5.5 && ci < 8.5) c = randDigit(h11(id + ci * 1.7 + tick * 0.13));
        else if (ci > 8.5 && ci < 9.5) c = 38.0;
        else if (ci > 9.5 && ci < 11.5) c = randDigit(h11(id + ci + tick));
        else if (ci > 12.5 && ci < 13.5) c = gain > 0.5 ? 36.0 : 37.0;    // + / -
        else if (ci > 13.5 && ci < 14.5) c = randDigit(h11(id + tick * 0.7));
        else if (ci > 14.5 && ci < 15.5) c = 38.0;
        else if (ci > 15.5 && ci < 17.5) c = randDigit(h11(id + ci * 2.0 + tick));
        else if (ci > 17.5 && ci < 18.5) c = 39.0;                     // %
        float on = ledText(q, c);
        vec3 tc = ci < 4.5 ? amb : (gain > 0.5 ? up : dn);
        // a row lights up when it trades
        float flash = exp(-fract(t * (1.5 + 3.0 * gPeak) + h11(id) * 10.0) * 6.0) * 0.12;
        vec3 c3 = vec3(0.012) + tc * on + tc * flash * step(0.2, ry) * step(ry, 0.85);
        col = mix(col, c3, 0.92);
        cover = 1.0;
    }

    // body: you're made of live quotes
    if (sil > 0.001) {
        vec2 g = p * vec2(26.0, 20.0) + vec2(0.0, -t * 2.0);
        vec2 gi = floor(g);
        float c = h12(gi) < 0.3 ? (h12(gi + 2.0) < 0.5 ? 41.0 : 42.0) : randDigit(h12(gi + floor(t * 3.0)));
        float on = glyph(c, fract(g), 1.0);
        float l = lum(camAt(uv));
        vec3 yc = mix(dn, up, step(0.45, l)) * (0.25 + on * 1.1);
        col = mix(col, yc, sil);
        cover = max(cover, sil);
    }
    if (bodyOn > 0.01 && bodyUseSkel() > 0.5) {
        float bd = boneDist(p, asp);
        col += amb * (1.0 - smoothstep(0.003, 0.008, bd)) * bodyOn;
    }
    col += (gDrop * dn * 0.25) * step(0.5, fract(t * 6.0));
    col = studioPalette(clamp(col, 0.0, 1.0));
    gl_FragColor = vec4(col, transparentBg ? cover : 1.0);
}
