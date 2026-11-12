/*{
  "DESCRIPTION": "Museum — Shades of New York 014. A gallery in the round: ten gilded frames float in 3D around you and turn slowly, each frame an ornate profile with beads and coves, gold leaf worn back to red bole on the high points and dark patina in the hollows, lit by spotlights. Each canvas holds a living generative painting in the manner of a movement (none copies a work): drip and splatter, soft colour fields, primaries on a black grid, pointillist river bank, lilies on a pond, a swirling night sky, a pop grid in four colourways, cubist facets, an oil portrait, a red sun over banded waves. The pop grid and the portrait are made of you when the camera is on. Canvases rotate styles over time, with weave and varnish. Body: Silhouette carves you as a marble statue in the room; Skeleton draws you in gilded wire. Audio: bass breathes the frames, beats lift the lights, the drop swings the room.",
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
      "NAME": "rotateSpeed",
      "LABEL": "Turn",
      "TYPE": "float",
      "MIN": -1.0,
      "MAX": 1.0,
      "DEFAULT": 0.15,
      "GROUP": "Gallery"
    },
    {
      "NAME": "frameCount",
      "LABEL": "Frames",
      "TYPE": "float",
      "MIN": 4.0,
      "MAX": 10.0,
      "DEFAULT": 10.0,
      "GROUP": "Gallery"
    },
    {
      "NAME": "goldAge",
      "LABEL": "Gilt Patina",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.5,
      "GROUP": "Gallery"
    },
    {
      "NAME": "spotlights",
      "LABEL": "Spotlights",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.8,
      "GROUP": "Gallery"
    },
    {
      "NAME": "paintMotion",
      "LABEL": "Paint Motion",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 2.0,
      "DEFAULT": 1.0,
      "GROUP": "Gallery"
    },
    {
      "NAME": "wallColor",
      "LABEL": "Wall",
      "TYPE": "color",
      "DEFAULT": [
        0.16,
        0.13,
        0.11,
        1.0
      ],
      "GROUP": "Gallery"
    },
    {
      "NAME": "spill",
      "LABEL": "Paint Spill",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.5,
      "GROUP": "Gallery"
    },
    {
      "NAME": "neonFrames",
      "LABEL": "Neon Frames",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.0,
      "GROUP": "Gallery"
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
        "Silhouette (you become a marble statue)",
        "Skeleton (gilded wire)",
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
      "DEFAULT": 2,
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
        0.14,
        0.12,
        0.11,
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
        0.03,
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


float gCamOK;
vec3 camPic(vec2 q) { return texture2D(camTex, bodyCamUV(vec2(0.2 + q.x * 0.6, 0.12 + q.y * 0.8))).rgb; }

// ten generative paintings, each in a manner of a movement (none copies a work)
vec3 painting(float s, vec2 q, float t, float h) {
    vec3 canvas = vec3(0.93, 0.9, 0.82);
    if (s < 0.5) {                                                   // drip & splatter
        vec3 c = canvas;
        for (int k = 0; k < 4; k++) {
            float fk = float(k);
            float n = fbm(q * vec2(2.5, 2.5) + fk * 5.3 + h * 9.0 + t * 0.01 * paintMotion);
            float line = 1.0 - smoothstep(0.006, 0.014, abs(n - 0.5));
            vec3 pc = fk < 0.5 ? vec3(0.05) : fk < 1.5 ? vec3(0.95, 0.95, 0.9) : fk < 2.5 ? vec3(0.75, 0.55, 0.2) : vec3(0.15, 0.25, 0.4);
            c = mix(c, pc, line);
            float splat = step(0.985, h12(floor(q * 40.0) + fk * 7.0)) * (1.0 - smoothstep(0.2, 0.45, length(fract(q * 40.0) - 0.5)));
            c = mix(c, pc, splat);
        }
        return c;
    }
    if (s < 1.5) {                                                   // colour field: soft stacked rectangles that breathe
        vec3 c = hue(fract(h + 0.02)) * 0.55 + 0.1;
        float edge = (fbm(q * 9.0 + h) - 0.5) * 0.03;
        float b1 = smoothstep(0.02, -0.02, sdBox(q - vec2(0.5, 0.7), vec2(0.38, 0.17 + 0.01 * sin(t * 0.5 * paintMotion))) + edge);
        float b2 = smoothstep(0.02, -0.02, sdBox(q - vec2(0.5, 0.3), vec2(0.38, 0.2)) + edge);
        c = mix(c, hue(fract(h + 0.08)) * 0.85, b1);
        c = mix(c, hue(fract(h + 0.55)) * 0.35 + 0.05, b2);
        return c * (0.92 + 0.08 * fbm(q * 30.0));
    }
    if (s < 2.5) {                                                   // primaries on a grid of black bars
        vec2 g = q;
        float shift = floor(t * 0.25 * paintMotion);
        float x1 = 0.3 + 0.1 * h11(h * 9.0 + shift), x2 = 0.72, y1 = 0.38 + 0.1 * h11(h * 3.0 + shift), y2 = 0.8;
        float bar = max(max(step(abs(g.x - x1), 0.018), step(abs(g.x - x2), 0.012)), max(step(abs(g.y - y1), 0.018), step(abs(g.y - y2), 0.012) * step(x1, g.x)));
        vec3 c = vec3(0.95, 0.93, 0.88);
        if (g.x < x1 && g.y > y1) c = vec3(0.85, 0.08, 0.06);
        if (g.x > x2 && g.y < y1) c = vec3(0.1, 0.2, 0.65);
        if (g.x > x2 && g.y > y2) c = vec3(0.98, 0.8, 0.1);
        return mix(c, vec3(0.03), bar);
    }
    if (s < 3.5) {                                                   // pointillist river bank
        vec2 g = q * 70.0;
        vec2 id = floor(g);
        vec2 cq = (id + 0.5) / 70.0;
        vec3 base = cq.y > 0.6 ? mix(vec3(0.95, 0.85, 0.6), vec3(0.55, 0.75, 0.95), (cq.y - 0.6) / 0.4) : cq.y > 0.35 ? vec3(0.25, 0.5, 0.75) : vec3(0.35, 0.6, 0.25);
        base += (vec3(h12(id), h12(id + 3.0), h12(id + 7.0)) - 0.5) * 0.35;
        float dotM = 1.0 - smoothstep(0.28, 0.42, length(fract(g) - 0.5 + (h21(h12(id)) - 0.5) * 0.2));
        return mix(canvas, base, dotM);
    }
    if (s < 4.5) {                                                   // lilies on a pond
        vec2 w = q * 3.0 + vec2(t * 0.02 * paintMotion, 0.0);
        float n = fbm(w + fbm(w * 2.0));
        vec3 c = mix(vec3(0.15, 0.3, 0.45), vec3(0.45, 0.65, 0.6), n);
        c += vec3(0.6, 0.5, 0.7) * smoothstep(0.6, 0.8, fbm(q * vec2(1.5, 8.0) + 2.0)) * 0.3;
        for (int k = 0; k < 6; k++) {
            float fk = float(k);
            vec2 pc = vec2(h11(fk * 3.0 + h), h11(fk * 7.0 + h));
            float pad = sdEllipse(q - pc, vec2(0.09, 0.035));
            c = mix(c, vec3(0.25, 0.45, 0.2) * (0.8 + 0.4 * fbm(q * 20.0)), 1.0 - smoothstep(0.0, 0.01, pad));
            c = mix(c, vec3(0.95, 0.6, 0.7), 1.0 - smoothstep(0.0, 0.01, length(q - pc - vec2(0.02, 0.01)) - 0.018));
        }
        return c;
    }
    if (s < 5.5) {                                                   // a swirling night sky
        vec2 w = q * 2.0;
        float a = fbm(w * 1.5 + t * 0.02 * paintMotion) * 6.28;
        float strokes = 0.5 + 0.5 * sin((q.x * cos(a) + q.y * sin(a)) * 90.0);
        vec3 c = mix(vec3(0.05, 0.12, 0.35), vec3(0.25, 0.45, 0.75), fbm(w * 2.0 + 1.0)) * (0.75 + 0.35 * strokes);
        vec2 mq = q - vec2(0.78, 0.8);
        c = mix(c, vec3(1.0, 0.85, 0.3), 1.0 - smoothstep(0.06, 0.07, length(mq)));
        c += vec3(1.0, 0.9, 0.5) * exp(-length(mq) * 12.0) * 0.4;
        for (int k = 0; k < 5; k++) { vec2 sp = vec2(h11(float(k) * 2.3 + h), 0.5 + 0.45 * h11(float(k) * 5.1 + h)); c += vec3(1.0, 0.95, 0.6) * exp(-length(q - sp) * 40.0) * (0.8 + 0.2 * sin(t * 2.0 + float(k))); }
        c = mix(c, vec3(0.05, 0.08, 0.05), step(q.y, 0.18 + 0.05 * sin(q.x * 9.0)));
        return c;
    }
    if (s < 6.5) {                                                   // pop: four colourways of you (or of a skyline)
        vec2 cell = floor(q * 2.0);
        vec2 f = fract(q * 2.0);
        float l = gCamOK > 0.5 ? lum(camPic(f)) : step(f.y, 0.3 + 0.4 * h11(floor(f.x * 8.0)));
        float k = cell.x + cell.y * 2.0;
        vec3 a = hue(fract(k * 0.27 + 0.1)), b = hue(fract(k * 0.27 + 0.55));
        return mix(a * 0.9, b, step(0.45, l));
    }
    if (s < 7.5) {                                                   // facets
        vec2 g = q * 5.0;
        vec2 id = floor(g); float md = 9.0; vec2 mid = vec2(0.0);
        for (int y = -1; y <= 1; y++) for (int x = -1; x <= 1; x++) {
            vec2 o = vec2(float(x), float(y));
            vec2 pt = o + h21(h12(id + o) * 77.0 + h);
            float d = length(fract(g) - pt);
            if (d < md) { md = d; mid = id + o; }
        }
        float tone = h12(mid + h);
        vec3 c = mix(vec3(0.3, 0.27, 0.22), vec3(0.85, 0.7, 0.45), tone);
        return c * (0.8 + 0.3 * smoothstep(0.0, 0.3, md));
    }
    if (s < 8.5) {                                                   // a portrait in oil (you, if the camera is on)
        vec3 c = gCamOK > 0.5 ? camPic(q + (vec2(fbm(q * 30.0), fbm(q * 30.0 + 5.0)) - 0.5) * 0.02) : mix(vec3(0.2, 0.12, 0.08), vec3(0.75, 0.55, 0.38), smoothstep(0.35, 0.0, length((q - vec2(0.5, 0.55)) * vec2(1.2, 0.9))));
        float l = lum(c);
        vec3 oil = mix(vec3(0.08, 0.05, 0.04), mix(vec3(0.6, 0.38, 0.22), vec3(0.98, 0.85, 0.65), smoothstep(0.4, 0.9, l)), smoothstep(0.05, 0.5, l));
        return mix(oil, c, 0.25) * (0.88 + 0.18 * sin(q.x * 120.0 + fbm(q * 10.0) * 8.0));
    }
    // a red sun over banded waves
    vec3 c = mix(vec3(0.95, 0.88, 0.72), vec3(0.85, 0.7, 0.55), q.y);
    c = mix(c, vec3(0.85, 0.15, 0.1), 1.0 - smoothstep(0.14, 0.15, length(q - vec2(0.5, 0.68))));
    float wv = sin(q.x * 30.0 + sin(q.y * 40.0 + t * 0.3 * paintMotion) * 1.5) * 0.5 + 0.5;
    c = mix(c, mix(vec3(0.1, 0.2, 0.45), vec3(0.85, 0.9, 0.95), step(0.8, wv)), step(q.y, 0.42 + 0.03 * sin(q.x * 12.0)));
    return c;
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
    gCamOK = step(0.02, lum(texture2D(camTex, vec2(0.5)).rgb) + lum(texture2D(camTex, vec2(0.3, 0.6)).rgb) + lum(texture2D(camTex, vec2(0.7, 0.4)).rgb));

    // the room: dark walls, a floor catching the spots
    vec3 col = mix(wallColor.rgb * 0.4, wallColor.rgb, smoothstep(-0.5, 0.4, p.y)) * (0.85 + 0.15 * vnoise(p * 30.0));
    col = mix(col, backdrop(uv), 0.25);
    col = mix(col, wallColor.rgb * 0.25 + vec3(0.03), smoothstep(-0.28, -0.32, p.y));

    vec3 ro = vec3(0.0, 0.05, 0.0);
    vec3 rd = normalize(vec3(p, 1.5));
    float yaw = t * rotateSpeed * 0.3 + 0.4 * gDrop;
    rd.xz = rot(yaw) * rd.xz;
    float nF = floor(frameCount);
    float bestT = 1e4; vec2 bl = vec2(0.0); float bi = -1.0; vec2 bsz = vec2(1.0); float drip = 0.0;
    for (int i = 0; i < 10; i++) {
        float fi = float(i);
        if (fi >= nF) break;
        float a = fi / nF * 6.2832;
        float rad = 3.0 + 0.3 * h11(fi * 3.0);
        vec3 C = vec3(sin(a) * rad, 0.08 * sin(t * 0.6 + fi) + 0.15 * (h11(fi * 5.0) - 0.5), cos(a) * rad);
        vec3 n = -normalize(vec3(C.x, 0.0, C.z));
        float dn = dot(rd, n);
        if (dn > -0.05) continue;
        float tt = dot(C - ro, n) / dn;
        if (tt <= 0.0 || tt > bestT) continue;
        vec3 hp = ro + rd * tt;
        vec3 right = vec3(n.z, 0.0, -n.x);
        vec2 l = vec2(dot(hp - C, right), hp.y - C.y);
        vec2 sz = vec2(0.62 + 0.25 * h11(fi * 7.0), 0.8 + 0.3 * h11(fi * 11.0)) * (1.0 + 0.04 * bassP() * ar);
        if (abs(l.x) < sz.x && abs(l.y) < sz.y) { bestT = tt; bl = l; bi = fi; bsz = sz; drip = 0.0; }
        else if (spill > 0.01 && abs(l.x) < sz.x - 0.16 && l.y < -sz.y && l.y > -sz.y - 0.9) {
            // paint spilling out of the bottom of the frame and running down the wall
            float colx = floor((l.x + sz.x) * 38.0);
            float hc = h11(colx * 3.7 + fi * 11.0);
            float len = spill * (0.15 + 0.7 * hc * hc) * (0.6 + 0.4 * sin(t * 0.3 + hc * 9.0) + 0.3 * bassP() * ar);
            float w = 1.0 - smoothstep(0.25, 0.45, abs(fract((l.x + sz.x) * 38.0) - 0.5));
            float y = -sz.y - l.y;
            float bead = 1.0 - smoothstep(0.0, 0.02, length(vec2((fract((l.x + sz.x) * 38.0) - 0.5) / 38.0, y - len)) - 0.012);
            if (step(0.45, hc) * (w * step(y, len) + bead) > 0.5) { bestT = tt; bl = l; bi = fi; bsz = sz; drip = 1.0; }
        }
    }
    if (bi >= 0.0 && drip > 0.5) {
        float style = mod(bi + floor(t / 40.0) * 3.0, 10.0);
        vec2 q = vec2((bl.x + bsz.x - 0.16) / (2.0 * (bsz.x - 0.16)), 0.03);
        col = painting(style, q, t, h11(bi * 13.0 + 1.0)) * 1.05 + vec3(0.25) * pow(max(0.0, sin(bl.x * 120.0)), 8.0) * 0.2;
        col *= exp(-(bestT - 3.0) * 0.15);
    } else if (bi >= 0.0) {
        float B = 0.16;
        vec2 e2 = bsz - abs(bl);
        float e = min(e2.x, e2.y);
        vec3 spot = vec3(1.0, 0.92, 0.78) * (0.55 + 0.75 * spotlights * exp(-pow((bl.y - bsz.y) * 0.9, 2.0)) * exp(-bl.x * bl.x * 0.8));
        if (e < B) {
            // the gilded frame: an ornate profile, gold leaf worn back to red bole on the high points, dark patina in the hollows
            float x = e / B;
            float hh = 0.55 * sin(x * PI) + 0.18 * sin(x * PI * 4.0) + 0.08 * sin(x * PI * 9.0);
            float dh = (0.55 * PI * cos(x * PI) + 0.72 * PI * cos(x * PI * 4.0) + 0.72 * PI * cos(x * PI * 9.0)) / B * 0.012;
            vec2 along = e2.x < e2.y ? vec2(-sign(bl.x), 0.0) : vec2(0.0, -sign(bl.y));
            float orn = (vnoise((e2.x < e2.y ? bl.y : bl.x) * vec2(60.0, 1.0) + bi * 5.0) - 0.5) * smoothstep(0.3, 0.6, x) * (1.0 - smoothstep(0.7, 0.9, x));
            vec3 N = normalize(vec3(along * (dh + orn * 3.0), 1.0));
            vec3 Ld = normalize(vec3(0.0, 0.85, 0.55));
            float dif = max(dot(N, Ld), 0.0);
            float spec = pow(max(dot(reflect(-Ld, N), vec3(0.0, 0.0, 1.0)), 0.0), 40.0);
            float wear = smoothstep(0.55, 0.8, fbm(bl * 14.0 + bi) + hh * 0.3) * goldAge;
            vec3 gold = mix(vec3(1.0, 0.76, 0.32), vec3(0.55, 0.16, 0.08), wear);
            gold *= mix(1.0, 0.35, goldAge * (1.0 - smoothstep(0.1, 0.5, hh)));
            col = gold * (0.12 + 0.9 * dif) * spot + vec3(1.0, 0.9, 0.7) * spec * (1.0 - wear) * 1.4;
            col *= 1.0 - 0.6 * smoothstep(0.08, 0.0, x);
            // neon frames: the gilt turns into glowing tubes on the beat
            vec3 neon = hue(fract(bi * 0.13 + t * 0.05));
            float tubes = (1.0 - smoothstep(0.0, 0.12, abs(x - 0.2))) + (1.0 - smoothstep(0.0, 0.1, abs(x - 0.85)));
            col = mix(col, col * 0.3 + neon * tubes * (1.2 + 0.8 * audioBeatPulse * ar), neonFrames);
        } else {
            vec2 q = (bl + bsz - B) / (2.0 * (bsz - B));
            float style = mod(bi + floor(t / 40.0) * 3.0, 10.0);
            vec3 pc = painting(style, q, t, h11(bi * 13.0 + 1.0));
            float weave = 0.94 + 0.06 * sin(q.x * 500.0) * sin(q.y * 500.0);
            float varnish = pow(max(0.0, 1.0 - abs(q.y - 0.75 - 0.1 * q.x)), 30.0) * 0.12;
            col = pc * weave * spot * 0.9 + varnish;
            col *= 1.0 - 0.35 * smoothstep(0.06, 0.0, min(min(q.x, 1.0 - q.x), min(q.y, 1.0 - q.y)));   // the frame's shadow on the canvas
        }
        col *= exp(-(bestT - 3.0) * 0.15);
        col *= 1.0 + 0.2 * audioBeatPulse * ar + 0.5 * gDrop;
    }
    // you: a marble statue in the gallery
    if (sil > 0.001) {
        float l = lum(camAt(uv));
        vec2 mq = p * 4.0;
        float vein = 1.0 - smoothstep(0.0, 0.025, abs(fbm(mq + fbm(mq * 2.0) * 1.5) - 0.5));
        vec3 marble = mix(vec3(0.55, 0.53, 0.5), vec3(0.97, 0.96, 0.93), smoothstep(0.05, 0.8, l));
        marble = mix(marble, vec3(0.45, 0.45, 0.48), vein * 0.5);
        marble *= 0.85 + 0.3 * smoothstep(-0.3, 0.4, p.y);
        col = mix(col, marble, sil);
    }
    if (bodyOn > 0.01 && bodyUseSkel() > 0.5) {
        float bd = boneDist(p, asp);
        col = mix(col, vec3(1.0, 0.78, 0.35) * (0.7 + 0.5 * sin(gBoneT * 40.0)), (1.0 - smoothstep(0.004, 0.01, bd)) * bodyOn);
    }
    col = studioPalette(clamp(col, 0.0, 1.0));
    gl_FragColor = vec4(col, 1.0);
}
