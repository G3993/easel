/*{
  "DESCRIPTION": "Times Square — Shades of New York 010. Hyper-real and maximalist: the camera dollies up a canyon of screens, every facade stacked with billboards of different sizes (some merging into giant boards), each running its own invented ad: brand wordmarks over sweeping gradients, a spinning can in fizz, amber news zippers crawling headlines, a fashion figure in colour blocks with NEW, liquid colour, flashing prices, starbursts and pixel waves. Ads change on the beat grid with a wipe, the LED subpixels show up close, the empty boards go dark, and the wet street reflects it all with yellow cabs streaking by. A jumbotron fills the end of the square. Audio: beats pump every screen, the drop blows them out white. Body: You're on the big screen: the jumbotron (and every board that rolls the live slot) shows your camera with your silhouette lit; Skeleton draws an LED outline of you across the whole square.",
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
      "NAME": "dolly",
      "LABEL": "Walk Forward",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 2.0,
      "DEFAULT": 0.5,
      "GROUP": "Square"
    },
    {
      "NAME": "adChange",
      "LABEL": "Beats per Ad",
      "TYPE": "float",
      "MIN": 1.0,
      "MAX": 16.0,
      "DEFAULT": 4.0,
      "GROUP": "Square"
    },
    {
      "NAME": "maximal",
      "LABEL": "Maximalism",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.85,
      "GROUP": "Square"
    },
    {
      "NAME": "exposure",
      "LABEL": "Exposure",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 2.5,
      "DEFAULT": 1.1,
      "GROUP": "Square"
    },
    {
      "NAME": "screenPixels",
      "LABEL": "LED Pixels Up Close",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.5,
      "GROUP": "Square"
    },
    {
      "NAME": "youOnScreen",
      "LABEL": "You on the Jumbotron",
      "TYPE": "bool",
      "DEFAULT": true,
      "GROUP": "Body"
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
        "Silhouette (you're on the jumbotron)",
        "Skeleton (an LED outline of you)",
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
        0.06,
        0.03,
        0.08,
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
        0.02,
        0.03,
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
float words(float i) {  // "NOVAZESTLUXEVOLTGLOWMEGASODAAURABOLDHYPE"
    if (i < 0.5) return 23.0;
    if (i < 1.5) return 24.0;
    if (i < 2.5) return 31.0;
    if (i < 3.5) return 10.0;
    if (i < 4.5) return 35.0;
    if (i < 5.5) return 14.0;
    if (i < 6.5) return 28.0;
    if (i < 7.5) return 29.0;
    if (i < 8.5) return 21.0;
    if (i < 9.5) return 30.0;
    if (i < 10.5) return 33.0;
    if (i < 11.5) return 14.0;
    if (i < 12.5) return 31.0;
    if (i < 13.5) return 24.0;
    if (i < 14.5) return 21.0;
    if (i < 15.5) return 29.0;
    if (i < 16.5) return 16.0;
    if (i < 17.5) return 21.0;
    if (i < 18.5) return 24.0;
    if (i < 19.5) return 32.0;
    if (i < 20.5) return 22.0;
    if (i < 21.5) return 14.0;
    if (i < 22.5) return 16.0;
    if (i < 23.5) return 10.0;
    if (i < 24.5) return 28.0;
    if (i < 25.5) return 24.0;
    if (i < 26.5) return 13.0;
    if (i < 27.5) return 10.0;
    if (i < 28.5) return 10.0;
    if (i < 29.5) return 30.0;
    if (i < 30.5) return 27.0;
    if (i < 31.5) return 10.0;
    if (i < 32.5) return 11.0;
    if (i < 33.5) return 24.0;
    if (i < 34.5) return 21.0;
    if (i < 35.5) return 13.0;
    if (i < 36.5) return 17.0;
    if (i < 37.5) return 34.0;
    if (i < 38.5) return 25.0;
    if (i < 39.5) return 14.0;
    return 43.0;
}
const float wordsLen = 40.0;
float news(float i) {  // "CITY NEVER SLEEPS * BIG APPLE GLOWS * TRAINS RUN ALL NIGHT * PIGEONS TAKE MIDTOWN * "
    if (i < 0.5) return 12.0;
    if (i < 1.5) return 18.0;
    if (i < 2.5) return 29.0;
    if (i < 3.5) return 34.0;
    if (i < 4.5) return 43.0;
    if (i < 5.5) return 23.0;
    if (i < 6.5) return 14.0;
    if (i < 7.5) return 31.0;
    if (i < 8.5) return 14.0;
    if (i < 9.5) return 27.0;
    if (i < 10.5) return 43.0;
    if (i < 11.5) return 28.0;
    if (i < 12.5) return 21.0;
    if (i < 13.5) return 14.0;
    if (i < 14.5) return 14.0;
    if (i < 15.5) return 25.0;
    if (i < 16.5) return 28.0;
    if (i < 17.5) return 43.0;
    if (i < 18.5) return 46.0;
    if (i < 19.5) return 43.0;
    if (i < 20.5) return 11.0;
    if (i < 21.5) return 18.0;
    if (i < 22.5) return 16.0;
    if (i < 23.5) return 43.0;
    if (i < 24.5) return 10.0;
    if (i < 25.5) return 25.0;
    if (i < 26.5) return 25.0;
    if (i < 27.5) return 21.0;
    if (i < 28.5) return 14.0;
    if (i < 29.5) return 43.0;
    if (i < 30.5) return 16.0;
    if (i < 31.5) return 21.0;
    if (i < 32.5) return 24.0;
    if (i < 33.5) return 32.0;
    if (i < 34.5) return 28.0;
    if (i < 35.5) return 43.0;
    if (i < 36.5) return 46.0;
    if (i < 37.5) return 43.0;
    if (i < 38.5) return 29.0;
    if (i < 39.5) return 27.0;
    if (i < 40.5) return 10.0;
    if (i < 41.5) return 18.0;
    if (i < 42.5) return 23.0;
    if (i < 43.5) return 28.0;
    if (i < 44.5) return 43.0;
    if (i < 45.5) return 27.0;
    if (i < 46.5) return 30.0;
    if (i < 47.5) return 23.0;
    if (i < 48.5) return 43.0;
    if (i < 49.5) return 10.0;
    if (i < 50.5) return 21.0;
    if (i < 51.5) return 21.0;
    if (i < 52.5) return 43.0;
    if (i < 53.5) return 23.0;
    if (i < 54.5) return 18.0;
    if (i < 55.5) return 16.0;
    if (i < 56.5) return 17.0;
    if (i < 57.5) return 29.0;
    if (i < 58.5) return 43.0;
    if (i < 59.5) return 46.0;
    if (i < 60.5) return 43.0;
    if (i < 61.5) return 25.0;
    if (i < 62.5) return 18.0;
    if (i < 63.5) return 16.0;
    if (i < 64.5) return 14.0;
    if (i < 65.5) return 24.0;
    if (i < 66.5) return 23.0;
    if (i < 67.5) return 28.0;
    if (i < 68.5) return 43.0;
    if (i < 69.5) return 29.0;
    if (i < 70.5) return 10.0;
    if (i < 71.5) return 20.0;
    if (i < 72.5) return 14.0;
    if (i < 73.5) return 43.0;
    if (i < 74.5) return 22.0;
    if (i < 75.5) return 18.0;
    if (i < 76.5) return 13.0;
    if (i < 77.5) return 29.0;
    if (i < 78.5) return 24.0;
    if (i < 79.5) return 32.0;
    if (i < 80.5) return 23.0;
    if (i < 81.5) return 43.0;
    if (i < 82.5) return 46.0;
    if (i < 83.5) return 43.0;
    return 43.0;
}
const float newsLen = 84.0;


float gBeats;
float gBody, gUV;
// a word of four letters from the invented-brand list
float brandChar(float w, float i) { return words(mod(w, 10.0) * 4.0 + i); }

// one billboard's content. q 0..1 in the panel, id = panel id, aspect = panel w/h
vec3 ad(float type, vec2 q, vec2 id, float aspect, float t) {
    float h = h12(id * 1.7 + 3.0);
    vec3 c1 = hue(h), c2 = hue(fract(h + 0.35 + 0.2 * h12(id)));
    vec2 qa = vec2(q.x * aspect, q.y);
    if (type < 0.5) {                                  // brand: gradient, a sweep, a big wordmark
        vec3 c = mix(c1, c2, q.x + 0.2 * sin(q.y * 3.0 + t));
        c += pow(max(0.0, 1.0 - abs(q.y - 0.5 - 0.3 * sin(q.x * 3.0 + t * 1.5)) * 5.0), 3.0) * 0.6;
        float w = floor(h * 10.0);
        float cw = min(aspect / 4.6, 0.75 * 0.62) ;
        vec2 tq = vec2((qa.x - (aspect - cw * 4.0) * 0.5) / cw, (q.y - 0.5) / (cw * 1.33) + 0.5);
        float ci = floor(tq.x);
        float on = ci >= 0.0 && ci < 4.0 ? glyph(brandChar(w, ci), vec2(fract(tq.x), tq.y), 1.0) : 0.0;
        return mix(c, vec3(1.0), on);
    }
    if (type < 1.5) {                                  // product: a spinning can, fizz behind it
        vec3 c = mix(c2 * 0.3, c2, q.y);
        vec2 bq = qa * vec2(1.0, 1.0) * 12.0 + vec2(0.0, -t * 2.0);
        c += vec3(1.0) * step(0.9, h12(floor(bq))) * (1.0 - smoothstep(0.1, 0.3, length(fract(bq) - 0.5))) * 0.5;
        vec2 cq = vec2(qa.x - aspect * 0.5, q.y - 0.5);
        float cw = 0.17, ch = 0.36;
        if (abs(cq.x) < cw && abs(cq.y) < ch) {
            float u = cq.x / cw;
            float th = asin(clamp(u, -1.0, 1.0)) + t * 1.5;
            vec3 lab = mix(c1, vec3(1.0), step(0.5, fract(th / 3.14159 * 2.0)));
            lab = mix(lab, c2, step(abs(cq.y), 0.06));
            float sh = sqrt(max(0.0, 1.0 - u * u));
            c = lab * (0.35 + 0.75 * sh) + pow(sh, 30.0) * 0.0 + pow(max(0.0, 1.0 - abs(u + 0.45) * 7.0), 2.0) * 0.7;
            if (abs(cq.y) > ch - 0.04) c = vec3(0.75) * (0.5 + 0.5 * sh);
        }
        return c;
    }
    if (type < 2.5) {                                  // news zipper: amber LED text crawling
        vec3 c = vec3(0.01);
        float ch = 0.62;
        float cw = ch * 0.75;
        float sx = qa.x / cw + t * 6.0 + h * 50.0;
        float ci = floor(sx);
        float on = glyph(news(mod(ci, newsLen)), vec2(fract(sx), (q.y - 0.19) / ch), 0.0);
        return c + vec3(1.0, 0.7, 0.15) * on * 1.3;
    }
    if (type < 3.5) {                                  // fashion: a model in colour blocks, NEW
        vec3 c = mix(c1, c1 * 0.4, q.y);
        vec2 fq = vec2(qa.x - aspect * 0.62, q.y - 0.45);
        float sway = 0.03 * sin(t * 2.0 + h * 9.0);
        float fig = min(min(length(fq - vec2(sway, 0.36)) - 0.07, sdSeg(fq, vec2(sway, 0.28), vec2(0.0, 0.0)) - 0.08),
                        min(sdSeg(fq, vec2(0.0, 0.0), vec2(-0.06, -0.42)) - 0.035, sdSeg(fq, vec2(0.0, 0.0), vec2(0.07, -0.42)) - 0.035));
        fig = min(fig, sdSeg(fq, vec2(0.0, 0.2), vec2(0.13 + sway, 0.02)) - 0.025);
        c = mix(c, c2 * 1.1, 1.0 - smoothstep(0.0, 0.006, fig));
        float cw = 0.14;
        vec2 tq = vec2((qa.x - 0.05) / cw, (q.y - 0.62) / (cw * 1.33));
        float ci = floor(tq.x);
        float on = ci >= 0.0 && ci < 3.0 ? glyph(ci < 0.5 ? 23.0 : ci < 1.5 ? 14.0 : 32.0, vec2(fract(tq.x), tq.y), 1.0) : 0.0;   // N E W
        return mix(c, vec3(1.0), on);
    }
    if (type < 4.5) {                                  // liquid: colour pouring through itself
        vec2 w = qa * 2.0;
        float n = fbm(w + vec2(fbm(w + t * 0.3), fbm(w - t * 0.25)) * 1.5);
        return mix(mix(c1, c2, n), vec3(1.0), pow(n, 6.0) * 2.0);
    }
    if (type < 5.5) {                                  // price: big flashing digits
        vec3 c = mix(vec3(0.9, 0.05, 0.08), vec3(1.0, 0.85, 0.1), step(0.5, fract(t * 2.0)) * 0.3);
        float cw = min(aspect / 3.6, 0.5);
        vec2 tq = vec2((qa.x - (aspect - cw * 3.0) * 0.5) / cw, (q.y - 0.5) / (cw * 1.33) + 0.5);
        float ci = floor(tq.x);
        float d1 = floor(mod(t * 0.5 + h * 9.0, 9.0)) + 1.0;
        float g = ci < 0.5 ? 40.0 : ci < 1.5 ? d1 : 9.0;
        float on = ci >= 0.0 && ci < 3.0 ? glyph(g, vec2(fract(tq.x), tq.y), 1.0) : 0.0;
        return mix(c, vec3(1.0), on);
    }
    if (type < 6.5) {                                  // burst: rays and stars
        vec2 bq = vec2(qa.x - aspect * 0.5, q.y - 0.5);
        float a = atan(bq.y, bq.x);
        vec3 c = mix(c1, c2, step(0.5, fract(a / 6.2832 * 16.0 + t * 0.3)));
        float st = glyph(46.0, (bq / 0.5 + 0.5), 1.0);
        return mix(c, vec3(1.0, 0.95, 0.6), st);
    }
    // pixel wave
    vec2 g = floor(qa * 14.0);
    float v = sin(g.x * 0.6 + t * 3.0) * sin(g.y * 0.8 - t * 2.0);
    return hue(fract(v * 0.2 + t * 0.1 + h)) * step(0.0, v);
}

// the street canyon: what a ray sees on the walls and far end (no floor)
vec3 canyon(vec3 ro, vec3 rd, float t, out float dist) {
    float Wd = 1.1;
    float tw = 1e4, side = 0.0;
    if (rd.x < 0.0) { float tt = (-Wd - ro.x) / rd.x; if (tt < tw) { tw = tt; side = -1.0; } }
    if (rd.x > 0.0) { float tt = (Wd - ro.x) / rd.x; if (tt < tw) { tw = tt; side = 1.0; } }
    float endZ = ro.z + 16.0;
    float te = (endZ - ro.z) / max(rd.z, 1e-4);
    dist = min(tw, te);
    vec3 hp = ro + rd * dist;
    vec2 wq; vec2 base;
    if (te < tw) { wq = vec2(hp.x + 7.0, hp.y); side = 0.0; }
    else wq = vec2(hp.z * (side > 0.0 ? 1.0 : -1.0), hp.y);
    if (hp.y > 2.8) {                                  // above the screens: night sky, a glow of light pollution
        return mix(vec3(0.1, 0.05, 0.12), vec3(0.01, 0.01, 0.03), clamp((rd.y) * 2.0, 0.0, 1.0));
    }
    // panels: a jittered grid where some cells merge into big boards
    float pw = 1.25, ph = 0.6;
    vec2 cell = floor(vec2(wq.x / pw, (wq.y + 0.5) / ph));
    vec2 big = floor(cell / 2.0);
    float merge = step(0.62, h12(big + side * 31.0));
    vec2 pid = merge > 0.5 ? big * 2.0 : cell;
    vec2 psz = merge > 0.5 ? vec2(pw, ph) * 2.0 : vec2(pw, ph);
    vec2 porg = vec2(pid.x * pw, pid.y * ph - 0.5);
    vec2 q = (wq - porg) / psz;
    float frame = step(0.03, q.x) * step(q.x, 0.97) * step(0.04, q.y) * step(q.y, 0.96);
    float empty = step(maximal, h12(pid * 3.1 + side) * 1.2);
    vec2 id = pid + side * 57.0;
    // the jumbotron at the end of the square
    float jumbo = side == 0.0 ? step(abs(hp.x), 0.9) * step(0.2, hp.y) * step(hp.y, 1.4) : 0.0;
    vec3 c;
    float per = floor(adChange);
    float slot = floor(gBeats / per + h12(id) * 3.0);
    float type = floor(h12(id + slot * 0.71) * 8.0);
    if (jumbo > 0.5) {
        q = vec2((hp.x + 0.9) / 1.8, (hp.y - 0.2) / 1.2);
        type = 7.0;
        id = vec2(99.0);
    }
    if (type > 6.5 && youOnScreen && gBody > 0.01) {
        vec2 cu = q;                                  // you, on the big screen
        float s = bodySeg(cu);
        vec3 cam = texture2D(camTex, bodyCamUV(cu)).rgb;
        c = mix(vec3(0.02, 0.03, 0.08) + 0.04 * step(0.5, fract(q.y * 90.0)), cam * 1.15, mix(0.25, 1.0, s) * gBody);
        c += vec3(0.3, 0.6, 1.0) * (1.0 - smoothstep(0.0, 0.02, abs(s - 0.5) - 0.2)) * 0.3 * gBody;
    } else {
        float aspect = psz.x / psz.y;
        c = ad(type, q, id, aspect, t);
        float inT = fract(gBeats / per + h12(id) * 3.0);
        c = mix(c, vec3(1.0), (1.0 - smoothstep(0.0, 0.03, abs(q.x - inT * 6.0))) * step(inT, 0.17));   // wipe to the next ad
    }
    if (jumbo < 0.5) c *= frame * (1.0 - empty);
    // the LED structure up close
    vec2 px = fract(q * psz * vec2(90.0, 90.0));
    vec3 sub = vec3(step(px.x, 0.33), step(0.33, px.x) * step(px.x, 0.66), step(0.66, px.x)) * 2.2;
    c *= mix(vec3(1.0), sub * step(0.15, px.y), screenPixels * smoothstep(6.0, 1.5, dist));
    // building fabric between boards
    vec3 fab = vec3(0.03, 0.03, 0.04) * (0.6 + 0.4 * vnoise(wq * 8.0));
    float lit = jumbo > 0.5 ? 1.0 : frame * (1.0 - empty);
    c = mix(fab, c, lit);
    return c * (1.0 + 0.5 * audioBeatPulse * audioReact + 1.6 * gDrop);
}

void main() {
    showDynamics();
    vec2 R = RENDERSIZE;
    vec2 uv = gl_FragCoord.xy / R;
    float asp = R.x / R.y;
    vec2 p = (uv - 0.5) * vec2(asp, 1.0);
    float ar = audioReact, t = TIME;
    gBody = float(bodyMode) > 0.5 ? smoothstep(0.3, 0.7, bodyPresence()) : 0.0;
    gBeats = TIME * (audioBPM > 1.0 ? audioBPM : 120.0) / 60.0;
    float camZ = t * dolly * 0.6;
    vec3 ro = vec3(0.12 * sin(t * 0.13), 0.15 + 0.03 * sin(t * 0.21), camZ);
    vec3 rd = normalize(vec3(p.x, p.y + 0.12, 1.25));
    rd.xz = rot(0.06 * sin(t * 0.09)) * rd.xz;

    float dist;
    vec3 col;
    float floorY = -0.5;
    float tf = rd.y < 0.0 ? (floorY - ro.y) / rd.y : 1e4;
    vec3 wall = canyon(ro, rd, t, dist);
    if (tf < dist) {
        // wet street: the boards reflected, broken by the asphalt; yellow cabs streaking by
        vec3 hp = ro + rd * tf;
        vec3 rr = vec3(rd.x, -rd.y, rd.z);
        float rough = (vnoise(hp.xz * vec2(8.0, 2.0)) - 0.5) * 0.06;
        rr.x += rough; rr.y += rough * 0.5;
        float d2;
        vec3 refl = canyon(hp, normalize(rr), t, d2);
        vec3 asph = vec3(0.025, 0.025, 0.03) * (0.7 + 0.5 * vnoise(hp.xz * 30.0));
        col = asph + refl * 0.45 * (0.6 + 0.4 * vnoise(hp.xz * 3.0));
        float lane = abs(fract(hp.x * 1.6 + 0.5) - 0.5);
        float cab = step(lane, 0.09) * step(0.7, h11(floor(hp.x * 1.6))) * smoothstep(0.85, 1.0, fract(hp.z * 0.12 + t * 0.9 + h11(floor(hp.x * 1.6)) * 5.0));
        col += vec3(1.0, 0.8, 0.15) * cab * 0.8 + vec3(1.0, 0.15, 0.1) * cab * step(0.6, fract(hp.z * 0.12 + t * 0.9)) * 0.4;
        col *= exp(-tf * 0.05);
    } else col = wall;
    // haze, glow and the LED overdrive of the place
    col = col * exposure;
    col += vec3(0.25, 0.12, 0.3) * 0.06 * (1.0 - abs(p.x) / asp);
    // skeleton: an LED outline of you across the whole square
    if (gBody > 0.01 && bodyUseSkel() > 0.5) {
        float bd = boneDist(p, asp);
        col += vec3(0.4, 0.9, 1.0) * (1.0 - smoothstep(0.003, 0.008, bd)) * gBody * 1.5 + vec3(0.3, 0.6, 1.0) * exp(-bd * 50.0) * 0.4 * gBody;
    }
    if (gBody > 0.01 && bodyUseSil() > 0.5 && !youOnScreen) {
        float s = bodySeg(uv);
        col = mix(col, texture2D(camTex, bodyCamUV(uv)).rgb, s * gBody);
    }
    col = aces(col * 1.2);
    col = studioPalette(clamp(col, 0.0, 1.0));
    gl_FragColor = vec4(col, 1.0);
}
