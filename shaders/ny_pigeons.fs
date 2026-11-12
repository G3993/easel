/*{
  "DESCRIPTION": "Pigeons — Shades of New York 006. A flock of city pigeons wheeling over the rooftops: cornices and the wooden water towers on stilts against the sky. Every bird is drawn with a body, head, beak, tail and two wings that beat (the near wing in front, the far wing darker), dove-grey with dark wing bars, an orange eye and the green-to-purple iridescent neck; the odd brown bird mixes in. One leader path with every bird trailing it at its own lag and drift keeps them moving as a flock, banking and turning; the depth of each bird sets its size and haze. Sky moods: overcast, golden hour or night. Audio: beats and drops scatter the flock outward. Interactive: press (or pinch with your hand in ShaderClaw) to scare the birds away from that spot. Body: with Perch up, a few birds take turns landing on your head, shoulders, elbows and wrists, folding their wings and standing there until the drop scares them off; Silhouette shows you on the rooftop; Skeleton draws a dove-grey line along your bones that sheds feathers on the beat.",
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
      "NAME": "flockSize",
      "LABEL": "Pigeons",
      "TYPE": "float",
      "MIN": 4.0,
      "MAX": 48.0,
      "DEFAULT": 34.0,
      "GROUP": "Flock"
    },
    {
      "NAME": "flySpeed",
      "LABEL": "Fly Speed",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 2.5,
      "DEFAULT": 1.0,
      "GROUP": "Flock"
    },
    {
      "NAME": "wingBeat",
      "LABEL": "Wing Beat",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2.0,
      "DEFAULT": 1.0,
      "GROUP": "Flock"
    },
    {
      "NAME": "birdScale",
      "LABEL": "Bird Size",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 2.5,
      "DEFAULT": 1.0,
      "GROUP": "Flock"
    },
    {
      "NAME": "scatter",
      "LABEL": "Scatter on Beat",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.5,
      "GROUP": "Flock"
    },
    {
      "NAME": "perch",
      "LABEL": "Perch On You",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.7,
      "GROUP": "Flock"
    },
    {
      "NAME": "formYou",
      "LABEL": "Flock Forms You",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.45,
      "GROUP": "Flock"
    },
    {
      "NAME": "featherFall",
      "LABEL": "Feathers",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.35,
      "GROUP": "Flock"
    },
    {
      "NAME": "iridescence",
      "LABEL": "Neck Iridescence",
      "TYPE": "float",
      "MIN": 0.0,
      "MAX": 1.0,
      "DEFAULT": 0.8,
      "GROUP": "Look"
    },
    {
      "NAME": "skyMood",
      "LABEL": "Sky",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2
      ],
      "LABELS": [
        "Overcast",
        "Golden Hour",
        "Night"
      ],
      "GROUP": "Look"
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
        "Silhouette (you on the roof, birds land on you)",
        "Skeleton (dove-grey bones shedding feathers)",
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
        0.75,
        0.78,
        0.82,
        1.0
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "bgColor2",
      "LABEL": "BG Bottom / Edge",
      "TYPE": "color",
      "DEFAULT": [
        0.55,
        0.58,
        0.62,
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


// ── rooftops: cornices and the city's wooden water towers against the sky ──
vec4 rooftops(vec2 p, float asp) {
    float x = p.x * 3.0;
    float id = floor(x);
    float top = -0.36 + 0.14 * h11(id * 2.3) + 0.05 * step(0.5, h11(id * 5.1));
    float cor = top + 0.012 + 0.006 * sin(p.x * 400.0) * step(0.0, p.y - top);          // cornice dentils
    float body = step(p.y, top);
    // water tower: barrel on stilts with a conical cap
    float has = step(0.45, h11(id * 9.7));
    vec2 tc = vec2((id + 0.3 + 0.4 * h11(id * 3.3)) / 3.0, top);
    vec2 tq = p - tc;
    float barrel = step(abs(tq.x), 0.035) * step(0.035, tq.y) * step(tq.y, 0.11);
    float cap = step(0.11, tq.y) * step(tq.y, 0.11 + 0.035 * (1.0 - abs(tq.x) / 0.04)) * step(abs(tq.x), 0.04);
    float legs = step(tq.y, 0.035) * step(0.0, tq.y) * (step(abs(abs(tq.x) - 0.028), 0.003) + step(abs(tq.y - 0.018), 0.002) * step(abs(tq.x), 0.03));
    float tower = has * clamp(barrel + cap + legs, 0.0, 1.0);
    float hoops = barrel * has * step(0.5, fract(tq.y * 90.0)) * 0.25;
    float m = clamp(body + tower, 0.0, 1.0);
    return vec4(m, hoops, step(abs(p.y - top), 0.006) * body, 0.0);
}

// ── one pigeon in its own frame (x forward, y up, ~1 unit long) ──
// returns coverage (x) and colour (yzw)
vec4 pigeon(vec2 q, float flap, float aa, float hsh, float perched) {
    float w = sin(flap);
    float body = sdEllipse(q - vec2(0.0, 0.0), vec2(0.5, 0.2));
    float head = length(q - vec2(0.5, 0.14 + 0.06 * perched)) - 0.15;
    float neck = sdSeg(q, vec2(0.25, 0.05), vec2(0.48, 0.12 + 0.06 * perched)) - 0.12;
    float beak = sdSeg(q, vec2(0.62, 0.13 + 0.06 * perched), vec2(0.74, 0.1 + 0.06 * perched)) - 0.025;
    vec2 tq = q - vec2(-0.62, -0.02);
    tq = rot(0.12) * tq;
    float tail = sdBox(tq, vec2(0.2, 0.06 - 0.03 * tq.x));
    float legs = perched > 0.5 ? min(sdSeg(q, vec2(0.05, -0.15), vec2(0.08, -0.36)), sdSeg(q, vec2(-0.08, -0.15), vec2(-0.05, -0.36))) - 0.018 : 9.0;
    // wings: the near wing sweeps up and down with the flap; folded flat when perched
    float span = mix(0.75, 0.0, perched);
    vec2 sh = vec2(0.08, 0.1);
    vec2 tip = sh + vec2(-0.38, 0.05 + span * w);
    vec2 elbow = mix(sh, tip, 0.5) + vec2(0.06, 0.08 * w);
    float wing = min(sdSeg(q, sh, elbow) - 0.13, sdSeg(q, elbow, tip) - mix(0.1, 0.05, clamp(length(q - elbow) / 0.4, 0.0, 1.0)));
    if (perched > 0.5) wing = sdEllipse(rot(0.12) * (q - vec2(-0.08, 0.04)), vec2(0.42, 0.13));
    vec2 tip2 = sh + vec2(-0.3, 0.05 + span * w * 0.75);
    float wing2 = sdSeg(q, sh + vec2(-0.05, 0.0), tip2) - 0.08;      // the far wing, darker
    float d = min(min(min(body, head), min(neck, tail)), min(min(wing, wing2), min(beak, legs)));
    float cov = 1.0 - smoothstep(-aa, aa, d);
    if (cov <= 0.0) return vec4(0.0);
    vec3 grey = mix(vec3(0.52, 0.55, 0.61), vec3(0.42, 0.44, 0.49), hsh);
    if (hsh > 0.86) grey = vec3(0.68, 0.6, 0.55);                    // the odd brown/white bird
    vec3 c = grey;
    if (wing2 < min(body, wing)) c = grey * 0.7;
    if (tail < body) c = grey * 0.62;
    if (head < 0.0 || neck < 0.0) c = vec3(0.36, 0.38, 0.43);
    // the iridescent neck: green ↔ purple as the light catches it
    float neckM = (1.0 - smoothstep(-0.02, 0.06, neck)) * (1.0 - smoothstep(-0.02, 0.02, -head));
    vec3 irid = mix(vec3(0.15, 0.62, 0.35), vec3(0.55, 0.2, 0.62), 0.5 + 0.5 * sin(q.y * 20.0 + flap * 0.3 + hsh * 9.0));
    c = mix(c, irid, neckM * iridescence);
    if (wing < min(body, min(head, neck))) {
        c = grey * 1.12;
        vec2 wb = q - sh;
        float bar = step(abs(fract(length(wb) * 4.0) - 0.5), 0.08) * step(0.2, length(wb)) * step(length(wb), 0.6);
        c = mix(c, vec3(0.18, 0.19, 0.22), bar * 0.8);
        c *= 1.0 - 0.35 * smoothstep(0.35, 0.75, length(wb));
    }
    if (beak < 0.0) c = vec3(0.25, 0.22, 0.2);
    if (legs < 0.0) c = vec3(0.85, 0.35, 0.35);
    float eye = 1.0 - smoothstep(0.02, 0.035, length(q - vec2(0.56, 0.18 + 0.06 * perched)));
    c = mix(c, vec3(0.95, 0.45, 0.1), eye);
    c *= 0.82 + 0.25 * smoothstep(-0.2, 0.25, q.y);                    // top-lit
    return vec4(cov, c);
}

void main() {
    showDynamics();
    vec2 R = RENDERSIZE;
    vec2 uv = gl_FragCoord.xy / R;
    float asp = R.x / R.y;
    vec2 p = (uv - 0.5) * vec2(asp, 1.0);
    float ar = audioReact, t = TIME * flySpeed;
    float bodyOn = float(bodyMode) > 0.5 ? smoothstep(0.3, 0.7, bodyPresence()) : 0.0;
    float sil = bodyOn * bodyUseSil() * bodySeg(uv);

    // sky
    float m = float(skyMood);
    vec3 col = backdrop(uv);
    if (m < 0.5) col = mix(vec3(0.62, 0.65, 0.7), vec3(0.82, 0.84, 0.86), uv.y) * (0.85 + 0.15 * fbm(p * 2.0 + vec2(TIME * 0.02, 0.0)));
    else if (m < 1.5) col = mix(vec3(1.0, 0.62, 0.32), vec3(0.42, 0.55, 0.78), smoothstep(0.1, 0.95, uv.y)) * (0.9 + 0.1 * fbm(p * 2.5 + TIME * 0.02));
    else col = mix(vec3(0.05, 0.06, 0.1), vec3(0.12, 0.14, 0.24), uv.y);
    float cover = 0.0;
    vec4 rt = rooftops(p, asp);
    vec3 roofC = m < 1.5 ? vec3(0.2, 0.19, 0.2) : vec3(0.03, 0.03, 0.05);
    // building fronts below the roofline: brick tone and rows of windows
    vec2 bw = vec2(p.x * 26.0, p.y * 18.0);
    float wins = step(0.3, fract(bw.x)) * step(fract(bw.x), 0.7) * step(0.25, fract(bw.y)) * step(fract(bw.y), 0.8);
    vec3 front = roofC * (0.9 + 0.2 * h11(floor(p.x * 3.0) * 4.1)) + vec3(0.04, 0.025, 0.02) * step(m, 1.5);
    vec3 winC = m < 1.5 ? vec3(0.32, 0.36, 0.42) * (0.7 + 0.5 * h12(floor(bw))) : vec3(1.0, 0.75, 0.4) * step(0.6, h12(floor(bw))) * 0.6;
    front = mix(front, winC, wins * step(p.y, -0.42 + 0.14 * h11(floor(p.x * 3.0) * 2.3)));
    col = mix(col, front + rt.y * 0.06 + rt.z * 0.15, rt.x);
    cover = rt.x;

    // you, behind the flock: your own camera image, a touch cooler
    if (sil > 0.001) { col = mix(col, camAt(uv) * vec3(0.95, 0.98, 1.05), sil); cover = max(cover, sil); }

    // the flock: one leader path, every bird trailing it with its own drift; beats and drops scatter them
    vec2 lead = vec2(sin(t * 0.21) * 0.55 * asp * 0.5 + 0.15 * sin(t * 0.53), 0.08 + 0.14 * sin(t * 0.37) + 0.05 * sin(t * 0.91));
    float burst = (0.25 * audioBeatPulse * ar + 0.9 * gDrop) * scatter;
    vec2 mp = (mousePos - 0.5) * vec2(asp, 1.0);
    float nb = flockSize;
    vec4 best = vec4(0.0); float bestZ = -1.0;
    for (int i = 0; i < 48; i++) {
        float fi = float(i);
        if (fi >= nb) break;
        float h = h11(fi * 7.31 + 0.5), h2 = h11(fi * 3.17 + 2.0);
        float z = 0.45 + 0.55 * h2;                                      // depth: small = far
        float lag = fi * 0.07 + h * 0.8;
        float tl = t - lag;
        vec2 l2 = vec2(sin(tl * 0.21) * 0.55 * asp * 0.5 + 0.15 * sin(tl * 0.53), 0.08 + 0.14 * sin(tl * 0.37) + 0.05 * sin(tl * 0.91));
        vec2 off = vec2(0.32 * asp * 0.5, 0.2) * vec2(sin(t * (0.6 + h) + fi), cos(t * (0.5 + h2) + fi * 1.7)) * (0.6 + 0.4 * h);
        vec2 pos = l2 + off;
        vec2 away = normalize(pos - lead + 1e-4);
        pos += away * burst * (0.3 + 0.5 * h);
        if (mouseDown > 0.5) { vec2 dm = pos - mp; pos += normalize(dm + 1e-4) * 0.25 * exp(-length(dm) * 5.0); }
        // the flock forms you: birds hold stations along your limbs, hovering, until the drop breaks them up
        if (bodyOn > 0.5 && formYou > 0.01 && fi >= 7.0) {
            vec2 tp = limbPt(mod(fi, 3.0), fract(h2 * 1.7 + fi * 0.13), asp);
            tp += gLimbN * (h - 0.5) * 0.06 + vec2(0.012 * sin(TIME * 7.0 + fi), 0.01 * cos(TIME * 5.0 + fi));
            float w = formYou * bodyOn * (1.0 - smoothstep(0.1, 0.5, gDrop + burst * 0.5));
            pos = mix(pos, tp, w);
            z = mix(z, 0.75, w);
        }
        // perch: a few birds take turns landing on your head, shoulders and arms
        float perched = 0.0;
        if (fi < 7.0 && bodyOn > 0.5 && perch > 0.01) {
            float jid = fi < 0.5 ? 0.0 : fi < 1.5 ? 11.0 : fi < 2.5 ? 12.0 : fi < 3.5 ? 13.0 : fi < 4.5 ? 14.0 : fi < 5.5 ? 15.0 : 16.0;
            float cyc = smoothstep(0.15, 0.45, sin(TIME * 0.17 + h * 6.28) + perch - 0.5) * (1.0 - smoothstep(0.2, 0.6, gDrop + burst));
            if (jointV(jid) > 0.5 && cyc > 0.0) {
                vec2 jp = jointP(jid) + vec2(0.0, jid < 0.5 ? 0.11 : 0.05);
                pos = mix(pos, jp, cyc);
                perched = step(0.97, cyc);
                z = mix(z, 1.0, cyc);
            }
        }
        float sc = 0.095 * birdScale * z;
        vec2 d = p - pos;
        if (dot(d, d) > sc * sc * 1.6) continue;
        vec2 l3 = vec2(sin((tl - 0.05) * 0.21) * 0.55 * asp * 0.5 + 0.15 * sin((tl - 0.05) * 0.53), 0.08 + 0.14 * sin((tl - 0.05) * 0.37) + 0.05 * sin((tl - 0.05) * 0.91));
        vec2 vel = pos - (l3 + off);
        float heading = perched > 0.5 ? (h > 0.5 ? 0.0 : PI) : atan(vel.y, vel.x);
        vec2 q = rot(-heading) * d / sc;
        if (cos(heading) < 0.0) { q.y = -q.y; }
        float flap = TIME * wingBeat * (9.0 + 4.0 * h) * (1.0 + 0.5 * burst) + h * 6.28;
        vec4 b = pigeon(q, flap, 1.5 / (R.y * sc), h2, perched);
        if (b.x > 0.0 && z > bestZ) {
            vec3 haze = mix(col, b.yzw, mix(0.55, 1.0, z));
            best = vec4(b.x, haze); bestZ = z;
        }
    }
    col = mix(col, best.yzw, best.x);
    cover = max(cover, best.x);
    // feathers drifting down (more on the drop)
    float fa = featherFall + gDrop * 0.8;
    if (fa > 0.01) {
        for (int k = 0; k < 2; k++) {
            float fk = float(k);
            vec2 q = p * (9.0 + fk * 6.0) + vec2(sin(TIME * 0.7 + fk) * 0.8, TIME * (0.5 + fk * 0.3));
            vec2 id = floor(q);
            float hh = h12(id + fk * 31.0);
            if (hh > fa * 0.35) continue;
            vec2 fq = fract(q) - 0.5;
            fq = rot(TIME * (hh - 0.5) * 3.0 + hh * 6.0) * fq;
            float shaft = sdSeg(fq, vec2(-0.22, 0.0), vec2(0.22, 0.0));
            float vane = sdEllipse(fq - vec2(0.02, 0.0), vec2(0.2, 0.07 + 0.02 * sin(fq.x * 20.0)));
            float fm = 1.0 - smoothstep(0.0, 0.02, min(vane, shaft - 0.005));
            col = mix(col, mix(vec3(0.72, 0.74, 0.78), vec3(0.95), hh * 3.0), fm * (0.6 + 0.4 * fk));
        }
    }

    // skeleton: a dove-grey line along your bones with feathers shed on the beat
    if (bodyOn > 0.01 && bodyUseSkel() > 0.5) {
        float bd = boneDist(p, asp);
        col = mix(col, vec3(0.88, 0.9, 0.95), (1.0 - smoothstep(0.003, 0.009, bd)) * bodyOn * 0.9);
        float fe = vnoise(p * 60.0 + vec2(0.0, TIME * 2.0));
        col += vec3(0.9) * step(0.92, fe) * exp(-bd * 25.0) * audioBeatPulse * ar * bodyOn;
    }
    col = studioPalette(clamp(col, 0.0, 1.0));
    gl_FragColor = vec4(col, transparentBg ? cover : 1.0);
}
