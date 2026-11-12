/*{
  "DESCRIPTION": "Times — Shades of New York 015. The morning paper coming off the press: newsprint pages scroll up the screen, each with a masthead and rules, a dateline, a banner headline, six columns of justified copy with sub-heads and column rules, and a four-column photograph in halftone (your camera, or a skyline), with the fold across the middle, fibres, and paper that yellows at the edges. On the beat and the drop the cyan and magenta plates slip out of register so the ink splits into colour; the drop runs the press flat out. Body: Silhouette wraps the text around you and prints you into the page as halftone; Skeleton inks you in heavy brush strokes. Original layout and type (no newspaper's masthead).",
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
      "NAME": "pressSpeed",
      "LABEL": "Press Speed",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 3.0,
      "DEFAULT": 0.5,
      "GROUP": "Press"
    },
    {
      "NAME": "halftoneSize",
      "LABEL": "Halftone Size",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 2.5,
      "DEFAULT": 1.0,
      "GROUP": "Press"
    },
    {
      "NAME": "inkSpread",
      "LABEL": "Ink Spread",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.4,
      "GROUP": "Press"
    },
    {
      "NAME": "registration",
      "LABEL": "Misregistration",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.5,
      "GROUP": "Press"
    },
    {
      "NAME": "paperAge",
      "LABEL": "Paper Age",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.35,
      "GROUP": "Press"
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
        "Silhouette (text wraps around you, printed in halftone)",
        "Skeleton (inked in brush strokes)",
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
        0.12,
        0.12,
        0.13,
        1.0
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "bgColor2",
      "LABEL": "BG Bottom / Edge",
      "TYPE": "color",
      "DEFAULT": [
        0.04,
        0.04,
        0.05,
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
float mast(float i) {  // "SHADES OF NEW YORK"
    if (i < 0.5) return 28.0;
    if (i < 1.5) return 17.0;
    if (i < 2.5) return 10.0;
    if (i < 3.5) return 13.0;
    if (i < 4.5) return 14.0;
    if (i < 5.5) return 28.0;
    if (i < 6.5) return 43.0;
    if (i < 7.5) return 24.0;
    if (i < 8.5) return 15.0;
    if (i < 9.5) return 43.0;
    if (i < 10.5) return 23.0;
    if (i < 11.5) return 14.0;
    if (i < 12.5) return 32.0;
    if (i < 13.5) return 43.0;
    if (i < 14.5) return 34.0;
    if (i < 15.5) return 24.0;
    if (i < 16.5) return 27.0;
    if (i < 17.5) return 20.0;
    return 43.0;
}
const float mastLen = 18.0;
float date(float i) {  // "LATE EDITION"
    if (i < 0.5) return 21.0;
    if (i < 1.5) return 10.0;
    if (i < 2.5) return 29.0;
    if (i < 3.5) return 14.0;
    if (i < 4.5) return 43.0;
    if (i < 5.5) return 14.0;
    if (i < 6.5) return 13.0;
    if (i < 7.5) return 18.0;
    if (i < 8.5) return 29.0;
    if (i < 9.5) return 18.0;
    if (i < 10.5) return 24.0;
    if (i < 11.5) return 23.0;
    return 43.0;
}
const float dateLen = 12.0;
float head0(float i) {  // "BIG APPLE GLOWS ALL NIGHT"
    if (i < 0.5) return 11.0;
    if (i < 1.5) return 18.0;
    if (i < 2.5) return 16.0;
    if (i < 3.5) return 43.0;
    if (i < 4.5) return 10.0;
    if (i < 5.5) return 25.0;
    if (i < 6.5) return 25.0;
    if (i < 7.5) return 21.0;
    if (i < 8.5) return 14.0;
    if (i < 9.5) return 43.0;
    if (i < 10.5) return 16.0;
    if (i < 11.5) return 21.0;
    if (i < 12.5) return 24.0;
    if (i < 13.5) return 32.0;
    if (i < 14.5) return 28.0;
    if (i < 15.5) return 43.0;
    if (i < 16.5) return 10.0;
    if (i < 17.5) return 21.0;
    if (i < 18.5) return 21.0;
    if (i < 19.5) return 43.0;
    if (i < 20.5) return 23.0;
    if (i < 21.5) return 18.0;
    if (i < 22.5) return 16.0;
    if (i < 23.5) return 17.0;
    if (i < 24.5) return 29.0;
    return 43.0;
}
const float head0Len = 25.0;
float head1(float i) {  // "PIGEONS TAKE MIDTOWN"
    if (i < 0.5) return 25.0;
    if (i < 1.5) return 18.0;
    if (i < 2.5) return 16.0;
    if (i < 3.5) return 14.0;
    if (i < 4.5) return 24.0;
    if (i < 5.5) return 23.0;
    if (i < 6.5) return 28.0;
    if (i < 7.5) return 43.0;
    if (i < 8.5) return 29.0;
    if (i < 9.5) return 10.0;
    if (i < 10.5) return 20.0;
    if (i < 11.5) return 14.0;
    if (i < 12.5) return 43.0;
    if (i < 13.5) return 22.0;
    if (i < 14.5) return 18.0;
    if (i < 15.5) return 13.0;
    if (i < 16.5) return 29.0;
    if (i < 17.5) return 24.0;
    if (i < 18.5) return 32.0;
    if (i < 19.5) return 23.0;
    return 43.0;
}
const float head1Len = 20.0;
float head2(float i) {  // "STEAM RISES ON FIFTH"
    if (i < 0.5) return 28.0;
    if (i < 1.5) return 29.0;
    if (i < 2.5) return 14.0;
    if (i < 3.5) return 10.0;
    if (i < 4.5) return 22.0;
    if (i < 5.5) return 43.0;
    if (i < 6.5) return 27.0;
    if (i < 7.5) return 18.0;
    if (i < 8.5) return 28.0;
    if (i < 9.5) return 14.0;
    if (i < 10.5) return 28.0;
    if (i < 11.5) return 43.0;
    if (i < 12.5) return 24.0;
    if (i < 13.5) return 23.0;
    if (i < 14.5) return 43.0;
    if (i < 15.5) return 15.0;
    if (i < 16.5) return 18.0;
    if (i < 17.5) return 15.0;
    if (i < 18.5) return 29.0;
    if (i < 19.5) return 17.0;
    return 43.0;
}
const float head2Len = 20.0;
float head3(float i) {  // "CITY NEVER SLEEPS"
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
    return 43.0;
}
const float head3Len = 17.0;


float gCamOK, gSil, gPage;
float headChar(float k, float i) {
    float m = mod(k, 4.0);
    if (m < 0.5) return head0(i);
    if (m < 1.5) return head1(i);
    if (m < 2.5) return head2(i);
    return head3(i);
}
float headLen(float k) { float m = mod(k, 4.0); return m < 0.5 ? head0Len : m < 1.5 ? head1Len : m < 2.5 ? head2Len : head3Len; }

// a line of text drawn with the dot font in solid pixels, centred in a band; returns ink
float textLine(vec2 q, float k, float which, float ch, float W) {
    float len = which < 0.5 ? mastLen : which < 1.5 ? headLen(k) : dateLen;
    ch = min(ch, W * 0.94 / (len * 0.78));
    float cw = ch * 0.78;
    float x0 = -len * cw * 0.5;
    vec2 tq = vec2((q.x - x0) / cw, q.y / ch);
    float ci = floor(tq.x);
    if (ci < 0.0 || ci >= len || tq.y < 0.0 || tq.y > 1.0) return 0.0;
    float c = which < 0.5 ? mast(ci) : which < 1.5 ? headChar(k, ci) : date(ci);
    return glyph(c, vec2(fract(tq.x), tq.y), 1.0);
}

// greeked body copy: justified lines of words with letter texture
float copy(vec2 q, float lh) {
    float line = floor(q.y / lh);
    float ly = fract(q.y / lh);
    float band = step(0.3, ly) * step(ly, 0.75) + step(0.75, ly) * step(ly, 0.82) * step(0.6, h11(floor(q.x * 400.0) + line));   // x-height + the odd descender
    float wx = q.x * 34.0 + h11(line) * 9.0;
    float wid = floor(wx);
    float inWord = step(0.12, fract(wx)) * step(0.15, h11(wid * 3.7 + line * 1.3) + 0.2);
    float letters = 0.55 + 0.45 * step(0.35, fract(q.x * 260.0 + h11(wid) * 3.0));
    return band * inWord * letters;
}

// halftone: dot area follows the darkness of the picture (screen at 45°)
float halftone(vec2 p, float dark, float cell) {
    vec2 r = rot(0.785) * p / cell;
    vec2 f = fract(r) - 0.5;
    float rad = sqrt(clamp(dark, 0.0, 1.0)) * 0.62;
    return 1.0 - smoothstep(rad - 0.08 - inkSpread * 0.08, rad + 0.04, length(f));
}

// everything printed on the page at pp (page coords, x centred, y from the page top downward)
float gW;
float inkAt(vec2 pp, vec2 screenUV) {
    float W = gW;
    if (abs(pp.x) > W * 0.5) return 0.0;
    float y = pp.y;
    float ink = 0.0;
    // masthead with rules
    if (y < 0.13) {
        ink = textLine(vec2(pp.x, 0.115 - y - 0.035), gPage, 0.0, 0.075, W);
        ink = max(ink, step(abs(y - 0.012), 0.002) + step(abs(y - 0.128), 0.0015));
        ink = max(ink, textLine(vec2(pp.x + W * 0.32, 0.03 - y + 0.0 + 0.0), gPage, 2.0, 0.014, W) * step(y, 0.03));
        return clamp(ink, 0.0, 1.0);
    }
    if (y < 0.145) return step(abs(y - 0.138), 0.0018);
    // headline
    if (y < 0.24) return textLine(vec2(pp.x, 0.225 - y), gPage, 1.0, 0.065, W);
    // columns
    float ncol = 6.0;
    float cwid = W / ncol;
    float cx = (pp.x + W * 0.5) / cwid;
    float ci = floor(cx);
    float fx = fract(cx);
    float gutter = step(fx, 0.06) * step(0.5, ci);
    float rule = step(abs(fx), 0.004) * step(0.5, ci);
    if (gutter > 0.5) return rule;
    vec2 cq = vec2(pp.x, y - 0.25);
    // the photo across the first four columns
    float photo = step(ci, 3.5) * step(cq.y, 0.42);
    if (photo > 0.5) {
        vec2 pq = vec2((pp.x + W * 0.5) / (cwid * 4.0), 1.0 - cq.y / 0.42);
        vec3 pic = gCamOK > 0.5 ? texture2D(camTex, bodyCamUV(vec2(0.1 + pq.x * 0.8, pq.y))).rgb
                               : vec3(mix(0.85, 0.2, step(pq.y, 0.25 + 0.5 * h11(floor(pq.x * 14.0)))));
        float frame = step(pq.x, 0.01) + step(0.99, pq.x) + step(pq.y, 0.01) + step(0.99, pq.y);
        return max(halftone(pp, 1.0 - lum(pic) * 1.05, 0.0075 * halftoneSize), clamp(frame, 0.0, 1.0));
    }
    if (ci < 3.5 && cq.y < 0.46) return copy(vec2(pp.x, cq.y), 0.016) * 0.6 * step(cq.y, 0.44);   // caption
    // body copy, wrapping round your silhouette (you're printed there as halftone instead)
    if (gSil > 0.5) return halftone(pp, 1.0 - lum(camAt(screenUV)) * 1.05, 0.006 * halftoneSize);
    // sub-heads now and then
    float sub = step(0.86, h11(floor(cq.y / 0.12) + ci * 7.0)) * step(fract(cq.y / 0.12), 0.18);
    if (sub > 0.5) return copy(vec2(pp.x * 0.6, cq.y), 0.028) * 1.0;
    return copy(cq, 0.0125) * 0.85;
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
    float silWide = bodyOn * bodyUseSil() * max(max(bodySeg(uv + vec2(0.02, 0.0)), bodySeg(uv - vec2(0.02, 0.0))), bodySeg(uv));
    gSil = step(0.5, silWide);
    gCamOK = step(0.02, lum(texture2D(camTex, vec2(0.5)).rgb) + lum(texture2D(camTex, vec2(0.3, 0.6)).rgb) + lum(texture2D(camTex, vec2(0.7, 0.4)).rgb));

    // the web press: pages come off the roll and scroll up; the drop runs the press flat out
    float pageH = 1.35;
    float scroll = t * pressSpeed * 0.06 + gDrop * 0.4;
    gW = min(asp * 0.92, 1.25);
    vec2 pp = vec2(p.x, 0.5 - p.y + scroll);
    gPage = floor(pp.y / pageH);
    float py = mod(pp.y, pageH);
    pp.y = py - 0.03;
    // ink registration slips on the beat and the drop: cyan and magenta plates drift apart
    float reg = registration * (0.004 * smoothstep(0.35, 1.0, audioBeatPulse) * ar + 0.016 * gDrop);
    vec2 oC = vec2(reg, -reg * 0.6), oM = vec2(-reg * 0.8, reg);
    float k0 = inkAt(pp, uv);
    float kc = reg > 0.0002 ? inkAt(pp + oC, uv) : k0;
    float km = reg > 0.0002 ? inkAt(pp + oM, uv) : k0;
    // newsprint: warm grey paper, fibres, yellowing at the edges, the fold across the middle
    vec3 paper = vec3(0.93, 0.91, 0.86) * (0.95 + 0.05 * vnoise(p * vec2(300.0, 900.0)));
    float edge = smoothstep(gW * 0.5 - 0.08, gW * 0.5, abs(p.x));
    paper = mix(paper, vec3(0.86, 0.78, 0.6), clamp(paperAge * (0.4 + edge), 0.0, 1.0) * 0.5);
    float fold = abs(py - pageH * 0.5);
    paper *= 1.0 - 0.12 * exp(-fold * 120.0) + 0.04 * exp(-fold * 40.0) * sign(py - pageH * 0.5);
    float gap = step(pageH - 0.03, py);
    vec3 inkC = vec3(0.07, 0.07, 0.08);
    vec3 col = paper * vec3(1.0 - kc * 0.92, 1.0 - km * 0.92, 1.0 - k0 * 0.92);
    col = mix(col, inkC, k0 * 0.08);
    // off the page: the press hall behind
    float onPage = step(abs(p.x), gW * 0.5) * (1.0 - gap);
    vec3 hall = backdrop(uv) * (0.6 + 0.4 * step(0.5, fract(p.y * 3.0 + t * pressSpeed * 0.3)));
    col = mix(hall, col, onPage);
    // shadow at the page's edges
    col *= 1.0 - 0.35 * smoothstep(gW * 0.5 - 0.01, gW * 0.5 + 0.03, abs(p.x)) * (1.0 - onPage) ;
    // skeleton: you, inked in heavy brush strokes
    if (bodyOn > 0.01 && bodyUseSkel() > 0.5) {
        float bd = boneDist(p, asp);
        float brush = 1.0 - smoothstep(0.008, 0.016, bd + (vnoise(p * 80.0) - 0.5) * 0.008);
        col = mix(col, inkC, brush * bodyOn);
    }
    col *= 1.0 + 0.05 * audioBeatPulse * ar;
    col = studioPalette(clamp(col, 0.0, 1.0));
    gl_FragColor = vec4(col, 1.0);
}
