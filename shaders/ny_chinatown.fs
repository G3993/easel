/*{
  "DESCRIPTION": "Chinatown — Shades of New York 011. A narrow street at night between brick tenements with fire escapes zigzagging down the fronts and warm windows: strings of red paper lanterns hang across it at three depths, ribbed and glowing from inside, with gold caps, tassels and gold 福 on some, swaying in the wind and on the bass; vertical neon signs of characters (中 大 人 口 日 田 王 山 木 天 上 下 十 三 福 四, drawn as glass tubes with white-hot cores and coloured halos) and lucky $888 price signs flicker and spill their colour on the walls. Firecracker bursts go off on the drop and peak. Body: Silhouette fills you with red and gold characters (deeper red in your shadows); Skeleton turns you into a neon sign. Audio: beats pulse the neon, bass swings the lanterns, the drop sets off the firecrackers.",
  "CREDIT": "Easel / ShaderClaw house shader — Shades of New York series, 2026-11-12 — original artwork.",
  "CATEGORIES": [
    "Generator",
    "Shades of New York",
    "Interactive",
    "Audio Reactive",
    "Body Tracking",
    "Type"
  ],
  "INPUTS": [
    {
      "NAME": "lanterns",
      "LABEL": "Lanterns",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.8,
      "GROUP": "Street"
    },
    {
      "NAME": "sway",
      "LABEL": "Sway",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.5,
      "GROUP": "Street"
    },
    {
      "NAME": "signCount",
      "LABEL": "Neon Signs",
      "TYPE": "float",
      "MIN": 2.0,
      "MAX": 8.0,
      "DEFAULT": 5.0,
      "GROUP": "Neon"
    },
    {
      "NAME": "neonFlicker",
      "LABEL": "Flicker",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.3,
      "GROUP": "Neon"
    },
    {
      "NAME": "glow",
      "LABEL": "Neon Glow",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 2.0,
      "DEFAULT": 1.0,
      "GROUP": "Neon"
    },
    {
      "NAME": "firecrackers",
      "LABEL": "Firecrackers",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.6,
      "GROUP": "Street"
    },
    {
      "NAME": "dragon",
      "LABEL": "Dragon Dance",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.8,
      "GROUP": "Street"
    },
    {
      "NAME": "goldRain",
      "LABEL": "Gold Coins",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.3,
      "GROUP": "Street"
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
        "Silhouette (red & gold characters fill you)",
        "Skeleton (a neon sign of you)",
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
        0.08,
        0.02,
        0.03,
        1.0
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "bgColor2",
      "LABEL": "BG Bottom / Edge",
      "TYPE": "color",
      "DEFAULT": [
        0.02,
        0.01,
        0.02,
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


// a neon tube following glyph strokes: core + halo
vec3 neonGlyph(float id, vec2 q, vec3 tint, float on, float pxs) {
    float d = hanziDist(id, q);
    float core = 1.0 - smoothstep(0.035, 0.035 + pxs, d);
    float tube = 1.0 - smoothstep(0.0, 0.07, d - 0.03);
    float halo = exp(-d * 9.0);
    return (vec3(1.0, 0.95, 0.9) * core * 0.9 + tint * tube * 0.8 + tint * halo * 0.45 * glow) * on;
}

// a paper lantern hanging at c, radius r (y up); returns coverage + colour
vec4 lantern(vec2 p, vec2 lc0, float r, float sway, float hsh) {
    vec2 q = p - lc0;
    q = rot(sway) * (q + vec2(0.0, r * 1.6)) - vec2(0.0, r * 1.6);       // swings from the wire above
    vec2 e = q / vec2(r, r * 0.82);
    float d = length(e) - 1.0;
    float cap = step(abs(q.x), r * 0.42) * step(abs(abs(q.y) - r * 0.86), r * 0.12);
    float tassel = step(abs(q.x), r * 0.05 + 0.02 * r * sin(q.y * 80.0)) * step(-r * 1.6, q.y) * step(q.y, -r * 0.9);
    float string = step(abs(q.x), r * 0.025) * step(r * 0.9, q.y) * step(q.y, r * 1.6);
    float body = 1.0 - smoothstep(-0.02, 0.02, d);
    float m = max(max(body, cap), max(tassel, string));
    if (m <= 0.0) return vec4(0.0);
    vec3 red = vec3(0.95, 0.08, 0.05);
    float ribs = 0.75 + 0.25 * cos(asin(clamp(e.x, -1.0, 1.0)) * 8.0);
    float inner = pow(max(0.0, 1.0 - length(e)), 0.6);
    vec3 c = red * ribs * (0.35 + 1.3 * inner) + vec3(1.0, 0.7, 0.3) * pow(inner, 3.0) * 0.6;
    c = mix(c, vec3(1.0, 0.75, 0.2) * (0.6 + 0.4 * sin(q.x / r * 9.0)), cap);
    c = mix(c, vec3(1.0, 0.6, 0.1), max(tassel, string) * (1.0 - body));
    // a gold 福 on some lanterns
    if (hsh > 0.55) {
        vec2 gq = (q / r) * 0.5 / 0.75 + 0.5;
        float gd = hanziDist(14.0, gq);
        c = mix(c, vec3(1.0, 0.82, 0.25), (1.0 - smoothstep(0.03, 0.06, gd)) * body * step(abs(q.x), r * 0.75) * step(abs(q.y), r * 0.75));
    }
    return vec4(m, c);
}

void main() {
    showDynamics();
    vec2 R = RENDERSIZE;
    vec2 uv = gl_FragCoord.xy / R;
    float asp = R.x / R.y;
    vec2 p = (uv - 0.5) * vec2(asp, 1.0);
    float ar = audioReact, t = TIME;
    float bodyOn = float(bodyMode) > 0.5 ? smoothstep(0.3, 0.7, bodyPresence()) : 0.0;
    float sil = bodyOn * bodyUseSil() * bodySeg(uv);
    float pxs = 2.0 / R.y;

    // the street at night: tenement walls, fire escapes zigzagging down the fronts
    vec3 col = backdrop(uv);
    float wallM = step(abs(p.x), asp * 0.5);
    vec2 bq = vec2(p.x * 4.0, p.y * 6.0);
    float bid = floor(bq.x);
    vec3 brick = mix(vec3(0.16, 0.05, 0.04), vec3(0.08, 0.04, 0.05), h11(bid * 3.0)) * (0.7 + 0.3 * vnoise(p * vec2(80.0, 160.0)));
    col = mix(col, brick, 0.85);
    // windows with warm light
    vec2 wg = vec2(p.x * 9.0, p.y * 7.0);
    vec2 wf = fract(wg), wi = floor(wg);
    float win = step(0.3, wf.x) * step(wf.x, 0.7) * step(0.25, wf.y) * step(wf.y, 0.85) * step(-0.15, p.y);
    col = mix(col, vec3(1.0, 0.6, 0.3) * step(0.5, h12(wi + floor(t * 0.05))) * 0.5 + vec3(0.02), win);
    // fire escapes
    float fe = fract(bq.x);
    float landing = step(abs(fract(p.y * 3.0) - 0.15), 0.01) * step(0.1, fe) * step(fe, 0.9) * step(0.5, h11(bid * 7.0));
    float stair = step(abs(fract(p.y * 3.0) - (0.15 + (fe - 0.1) * 0.85 * 0.4)), 0.006) * step(0.1, fe) * step(fe, 0.9) * step(0.5, h11(bid * 7.0)) * step(-0.2, p.y);
    float railing = step(abs(fract(fe * 30.0) - 0.5), 0.08) * step(fract(p.y * 3.0), 0.15) * step(0.07, fract(p.y * 3.0)) * step(0.1, fe) * step(fe, 0.9) * step(0.5, h11(bid * 7.0));
    col = mix(col, vec3(0.02, 0.015, 0.015), clamp(landing + stair + railing * 0.8, 0.0, 1.0));
    // the street glow
    col += vec3(0.6, 0.12, 0.05) * smoothstep(-0.1, -0.5, p.y) * 0.4;

    // vertical neon signs of characters (and lucky-number price signs)
    float ns = floor(signCount);
    float sw = asp / ns;
    float si = floor((p.x + asp * 0.5) / sw);
    float hs = h11(si * 5.3 + 2.0);
    vec2 sc = vec2(-asp * 0.5 + (si + 0.5) * sw + (hs - 0.5) * sw * 0.3, 0.02 + 0.12 * h11(si * 1.9));
    float gH = min(sw * 0.62, 0.17);                                   // glyph size
    float nG = 3.0 + floor(h11(si * 8.7) * 2.0);
    vec2 sq = p - sc;
    float signBox = sdBox(sq, vec2(gH * 0.6, gH * nG * 0.55));
    vec3 tint = h11(si * 4.1) < 0.6 ? vec3(1.0, 0.12, 0.2) : h11(si * 4.1) < 0.85 ? vec3(1.0, 0.75, 0.15) : vec3(0.3, 1.0, 0.75);
    float flick = 1.0 - neonFlicker * step(0.93, vnoise(vec2(t * 6.0, si * 7.0))) * 0.9;
    float on = flick * (0.85 + 0.3 * audioBeatPulse * ar + 0.6 * gDrop);
    if (signBox < 0.03) {
        col = mix(col, vec3(0.03, 0.01, 0.015), 1.0 - smoothstep(0.0, 0.004, signBox));
        float k = floor((gH * nG * 0.5 - sq.y) / (gH * 1.05));
        if (k >= 0.0 && k < nG) {
            vec2 gq = vec2(sq.x / gH + 0.5, (gH * nG * 0.5 - sq.y) / (gH * 1.05) - k);
            gq.y = 1.0 - gq.y;
            gq = (gq - 0.5) * 1.1 + 0.5;
            if (h11(si * 6.6) < 0.78) {
                float id = floor(h11(si * 13.0 + k * 3.7) * HANZI_N);
                col += neonGlyph(id, gq, tint, on, pxs / gH);
            } else {
                float c = k < 0.5 ? 40.0 : 8.0;                         // $888 — lucky eights
                float g = glyph(c, gq * vec2(1.0, 1.0), 1.0);
                col += tint * g * on * 1.4 + tint * on * 0.05;
            }
        }
    }
    // neon spill on the walls
    col += tint * exp(-max(signBox, 0.0) * 18.0) * 0.18 * on * glow;

    // strings of lanterns across the street, three depths
    for (int row = 0; row < 3; row++) {
        float fr = float(row);
        float depth = 1.0 - fr * 0.28;
        float wy = 0.36 - fr * 0.14;
        float span = asp * 1.1;
        float sag = 0.07 * depth;
        float wireY = wy - sag * (1.0 - pow(p.x / (span * 0.5), 2.0));
        col = mix(col, vec3(0.02), (1.0 - smoothstep(0.0, 0.0025, abs(p.y - wireY))) * 0.8);
        float r = 0.045 * depth * (lanterns + 0.4) * 1.2;
        float spacing = r * 3.4;
        float li = floor((p.x + span) / spacing);
        float lx = -span + (li + 0.5) * spacing;
        float hl = h11(li * 3.1 + fr * 9.0);
        if (hl > lanterns) continue;
        float ly = wy - sag * (1.0 - pow(lx / (span * 0.5), 2.0)) - r * 1.6;
        float swy = (sway * 0.25 + 0.2 * bassP() * ar) * sin(t * (1.2 + hl) + li);
        vec4 L4 = lantern(p, vec2(lx, ly), r, swy, hl);
        col = mix(col, L4.yzw * mix(0.7, 1.0, depth), L4.x);
        col += vec3(1.0, 0.25, 0.08) * exp(-length(p - vec2(lx, ly)) / r * 1.2) * 0.12 * depth;
    }

    // the dragon dance: a long red-and-gold body undulating across the street, head first
    if (dragon > 0.01) {
        float L = 1.5 * (0.6 + 0.4 * dragon);
        float hx = mod(t * 0.28, asp + L + 0.6) - asp * 0.5 - 0.3;
        hx = asp * 0.5 + 0.3 - (hx + asp * 0.5 + 0.3);                     // travels right → left
        float baseY = -0.18 + (mouseDown > 0.5 ? (mousePos.y - 0.5) * 0.6 : 0.0);
        float best = 9.0, bs = 0.0;
        vec2 prev = vec2(hx, baseY + 0.07 * sin(hx * 5.0 - t * 3.0));
        for (int i = 1; i < 24; i++) {
            float sd = float(i) / 23.0 * L;
            float x = hx + sd;
            vec2 cur = vec2(x, baseY + (0.07 + 0.04 * bassP() * ar) * sin(x * 5.0 - t * 3.0));
            vec2 pa = p - prev, ba = cur - prev;
            float hh = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
            float d = length(pa - ba * hh);
            float sAt = (float(i) - 1.0 + hh) / 23.0;
            float r = 0.055 * dragon * (1.0 - 0.55 * sAt) + 0.01;
            if (d - r < best) { best = d - r; bs = sAt; }
            prev = cur;
        }
        float body = 1.0 - smoothstep(0.0, 0.004, best);
        if (body > 0.0) {
            float scale = step(0.5, fract(bs * 60.0 + floor(-best * 80.0) * 0.5));
            vec3 dc = mix(vec3(0.85, 0.06, 0.04), vec3(1.0, 0.75, 0.15), scale * 0.6);
            dc *= 0.6 + 0.6 * smoothstep(-0.05, 0.0, best);               // rounded
            float fringe = step(0.8, fract(bs * 40.0)) * smoothstep(-0.012, 0.0, best);
            dc = mix(dc, vec3(1.0, 0.85, 0.3), fringe);
            col = mix(col, dc, body);
        }
        // the head: a big painted head with gold horns, white eyes and whiskers
        vec2 hc = vec2(hx, baseY + 0.07 * sin(hx * 5.0 - t * 3.0));
        vec2 hq = (p - hc) / (0.09 * dragon + 0.02);
        float head = sdEllipse(hq, vec2(1.1, 0.8));
        float horn = min(sdSeg(hq, vec2(0.2, 0.5), vec2(0.7, 1.3)), sdSeg(hq, vec2(-0.1, 0.55), vec2(0.2, 1.4))) - 0.12;
        float jaw = sdEllipse(hq - vec2(-0.7, -0.35 - 0.15 * sin(t * 6.0)), vec2(0.6, 0.25));
        float whisk = min(sdSeg(hq, vec2(-0.9, 0.0), vec2(-1.7, 0.4 + 0.2 * sin(t * 4.0))), sdSeg(hq, vec2(-0.9, -0.1), vec2(-1.8, -0.5 + 0.2 * cos(t * 3.0)))) - 0.04;
        float eye = length(hq - vec2(-0.3, 0.3)) - 0.22;
        vec3 hcl = vec3(0.9, 0.08, 0.05);
        col = mix(col, vec3(1.0, 0.8, 0.2), 1.0 - smoothstep(0.0, 0.08, min(horn, whisk)));
        col = mix(col, hcl * (0.8 + 0.4 * smoothstep(0.0, -0.6, head)), 1.0 - smoothstep(0.0, 0.08, min(head, jaw)));
        col = mix(col, vec3(1.0), 1.0 - smoothstep(0.0, 0.06, eye));
        col = mix(col, vec3(0.02), 1.0 - smoothstep(0.0, 0.06, length(hq - vec2(-0.35, 0.28)) - 0.1));
    }
    // gold coins tumbling down (a shower on the drop)
    float gr = goldRain + gDrop;
    if (gr > 0.01) {
        for (int k = 0; k < 2; k++) {
            float fk = float(k);
            vec2 q = p * (10.0 + fk * 7.0) + vec2(sin(t * 0.6 + fk) * 0.5, t * (1.6 + fk));
            vec2 id = floor(q);
            float hh = h12(id + fk * 23.0);
            if (hh > gr * 0.35) continue;
            vec2 fq = fract(q) - 0.5;
            float spin = t * (2.0 + hh * 5.0) + hh * 9.0;
            vec2 cq = fq / vec2(max(abs(cos(spin)), 0.08), 1.0);
            float coin = length(cq) - 0.28;
            float hole = sdBox(cq, vec2(0.07));
            float m = (1.0 - smoothstep(0.0, 0.05, coin)) * smoothstep(0.0, 0.03, hole);
            vec3 gc = vec3(1.0, 0.78, 0.25) * (0.6 + 0.5 * abs(cos(spin))) + vec3(1.0) * pow(abs(sin(spin)), 20.0) * 0.6;
            col = mix(col, gc, m);
        }
    }
    // you: red and gold, filled with characters
    if (sil > 0.001) {
        vec2 g = p * 9.0 + vec2(0.0, t * 0.3);
        vec2 gi = floor(g);
        float id = floor(h12(gi + floor(t * 0.5)) * HANZI_N);
        float d = hanziDist(id, fract(g));
        float gl = 1.0 - smoothstep(0.04, 0.08, d);
        float l = lum(camAt(uv));
        vec3 you = mix(vec3(0.55, 0.02, 0.03), vec3(0.95, 0.12, 0.08), smoothstep(0.1, 0.7, l));
        you = mix(you, vec3(1.0, 0.8, 0.25), gl);
        col = mix(col, you, sil);
    }
    // skeleton: you as a neon sign
    if (bodyOn > 0.01 && bodyUseSkel() > 0.5) {
        float bd = boneDist(p, asp);
        col += (vec3(1.0, 0.95, 0.9) * (1.0 - smoothstep(0.003, 0.006, bd)) + vec3(1.0, 0.12, 0.25) * exp(-bd * 45.0) * glow) * bodyOn;
    }
    // firecrackers on the drop and the peak: bursts of sparks
    float fc = firecrackers * (gDrop + 0.4 * gPeak * audioBeatPulse);
    if (fc > 0.01) {
        for (int b = 0; b < 3; b++) {
            float fb = float(b);
            vec2 c = vec2((h11(fb * 3.0 + floor(t * 2.0)) - 0.5) * asp, h11(fb * 7.0 + floor(t * 2.0)) * 0.6 - 0.2);
            vec2 d = p - c;
            float a = atan(d.y, d.x);
            float ray = step(0.82, h11(floor(a * 12.0) + fb * 5.0 + floor(t * 2.0)));
            float rr = length(d);
            float ph = fract(t * 2.0);
            col += vec3(1.0, 0.8, 0.3) * ray * exp(-abs(rr - ph * 0.25) * 90.0) * fc * 1.5;
        }
    }
    col = studioPalette(clamp(col, 0.0, 1.0));
    gl_FragColor = vec4(col, 1.0);
}
