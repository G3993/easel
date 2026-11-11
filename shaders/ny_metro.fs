/*{
  "DESCRIPTION": "Metro — Shades of New York 004. A living map of the New York subway, seen by a camera that orbits and tours over the city. The five boroughs sit in their rivers: the long island of Manhattan with its street grid and the park in its middle, the Bronx above, Queens and Brooklyn across the East River, the Rockaways around Jamaica Bay, Staten Island across the harbour and the New Jersey shore. An original schematic of the trunk lines runs over them in the line colours: the west-side red, the east-side green, the 8th Avenue blue, the 6th Avenue orange, the Broadway yellow, the purple Flushing line, the lime crosstown, the grey 14th Street line, the brown Jamaica line and the Staten Island railway. Every line has two tracks and the trains never stop: lit cars with headlights in front and tail lights behind leave streaks of light along the tracks, and stations flash as a train pulls through. Streets carry tiny moving traffic lights and the water shimmers. Colours change here and there: Colour Drift lets lines trade colours now and then, and the themes go from Night Map and Day Map to Neon, Blueprint and Ink on Paper. Camera: Orbit spins slowly around the city while the tour glides between Manhattan, Brooklyn and Queens; Tilt lowers the camera from straight down to a skyline-height view, and Zoom pulls from the whole city into the streets. Camera friendly: put an image or the live camera under the map. Body tracking (MediaPipe pose + segmentation) turns you into a subway line: your skeleton is drawn over the city as a glowing line in trunk-line colours (red spine, yellow shoulders and hips, green arms, blue legs) with trains running along your limbs and a station at every joint, while the map lights up and shows the camera inside your silhouette. Your mouse, or your hand in ShaderClaw, lights the district under it; press or pinch to send a pulse racing out through the tracks. Audio: bass makes the trains glow and the lines breathe, beats send a ring of light out from Times Square through the network, mids drive train speed, highs make the stations twinkle. Show dynamics: at Rest the city is dim, the trains slow and the camera high and still; the Build speeds every train up, brightens the network, spins the camera faster and lowers it toward the streets; the Drop flashes the whole network in a shockwave out of Midtown with a camera punch; the Peak runs the system at full rush hour.",
  "CREDIT": "Easel / ShaderClaw house shader — Shades of New York series, 2026-11-11 — original schematic, not the official map.",
  "CATEGORIES": ["Generator", "Shades of New York", "Map", "3D", "Interactive", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "theme",       "LABEL": "Theme", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2, 3, 4], "LABELS": ["Night Map", "Day Map", "Neon", "Blueprint", "Ink on Paper"], "GROUP": "Look" },
    { "NAME": "colourDrift", "LABEL": "Colour Drift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.2, "GROUP": "Look" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",        "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "GROUP": "Look" },
    { "NAME": "lineWidth",   "LABEL": "Line Width",       "TYPE": "float", "MIN": 0.4, "MAX": 2.5, "DEFAULT": 1.0, "GROUP": "Look" },
    { "NAME": "glowAmt",     "LABEL": "Glow",             "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.9, "GROUP": "Look" },
    { "NAME": "streets",     "LABEL": "Streets & Traffic","TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.7, "GROUP": "Look" },
    { "NAME": "landTint",    "LABEL": "Land Tint",        "TYPE": "color", "DEFAULT": [1.0, 1.0, 1.0, 1.0], "GROUP": "Look" },
    { "NAME": "waterTint",   "LABEL": "Water Tint",       "TYPE": "color", "DEFAULT": [1.0, 1.0, 1.0, 1.0], "GROUP": "Look" },
    { "NAME": "trainSpeed",  "LABEL": "Train Speed",      "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0, "GROUP": "Trains" },
    { "NAME": "trainDensity","LABEL": "Trains Per Line",  "TYPE": "float", "MIN": 0.3, "MAX": 3.0, "DEFAULT": 1.0, "GROUP": "Trains" },
    { "NAME": "trailAmt",    "LABEL": "Light Trails",     "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.8, "GROUP": "Trains" },
    { "NAME": "stationFlash","LABEL": "Station Flash",    "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.8, "GROUP": "Trains" },
    { "NAME": "spin",        "LABEL": "Orbit Spin",       "TYPE": "float", "MIN": -1.0, "MAX": 1.0, "DEFAULT": 0.3, "GROUP": "Camera" },
    { "NAME": "tilt",        "LABEL": "Tilt",             "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Camera" },
    { "NAME": "zoomAmt",     "LABEL": "Zoom",             "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.45, "GROUP": "Camera" },
    { "NAME": "tour",        "LABEL": "City Tour",        "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6, "GROUP": "Camera" },
    { "NAME": "interact",    "LABEL": "Hand", "TYPE": "long", "DEFAULT": 1, "VALUES": [0, 1], "LABELS": ["Off", "Light + Pulse (press / pinch)"], "GROUP": "Hand" },
    { "NAME": "bodyMode",    "LABEL": "Body Tracking", "TYPE": "long", "DEFAULT": 3, "VALUES": [0, 1, 2, 3], "LABELS": ["Off", "Silhouette (the city lights up inside you)", "Skeleton (your body is a subway line)", "Both"], "GROUP": "Body" },
    { "NAME": "bodyMirror",  "LABEL": "Mirror (selfie view)", "TYPE": "bool", "DEFAULT": true, "GROUP": "Body" },
    { "NAME": "bodyLineW",   "LABEL": "Body Line Width",  "TYPE": "float", "MIN": 0.3, "MAX": 3.0, "DEFAULT": 1.0, "GROUP": "Body" },
    { "NAME": "bodyCamMix",  "LABEL": "Camera Inside You", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Body" },
    { "NAME": "camTex",      "LABEL": "Image / Camera",   "TYPE": "image", "GROUP": "Camera Input" },
    { "NAME": "camMix",      "LABEL": "Image Under Map",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "GROUP": "Camera Input" },
    { "NAME": "paletteMode", "LABEL": "Studio Palette", "TYPE": "long", "DEFAULT": 4, "VALUES": [0, 1, 2, 3, 4], "LABELS": ["Colorful", "Red · Black · White", "Blue · Black · White", "Black & White", "NYC Reference"], "GROUP": "Studio Palette" },
    { "NAME": "palAmount",   "LABEL": "Palette Amount",   "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 1.0, "GROUP": "Studio Palette" },
    { "NAME": "palRed",      "LABEL": "Red Accent",       "TYPE": "color", "DEFAULT": [0.92, 0.04, 0.07, 1.0], "GROUP": "Studio Palette" },
    { "NAME": "palBlue",     "LABEL": "Blue Accent",      "TYPE": "color", "DEFAULT": [0.07, 0.32, 1.0, 1.0],  "GROUP": "Studio Palette" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.65, "GROUP": "Audio Reactivity" },
    { "NAME": "showState",   "LABEL": "Show State", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2, 3], "LABELS": ["Follow Music", "Rest", "Build", "Peak"], "GROUP": "Show Dynamics" },
    { "NAME": "dynamics",    "LABEL": "Build & Drop Depth", "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.85, "GROUP": "Show Dynamics" },
    { "NAME": "bgMode",      "LABEL": "Sky", "TYPE": "long", "DEFAULT": 1, "VALUES": [0, 1, 2], "LABELS": ["Solid", "Vertical Gradient", "Radial Gradient"], "GROUP": "Background" },
    { "NAME": "bgColor",     "LABEL": "BG Color / Top / Center", "TYPE": "color", "DEFAULT": [0.01, 0.012, 0.03, 1.0], "GROUP": "Background" },
    { "NAME": "bgColor2",    "LABEL": "BG Bottom / Edge", "TYPE": "color", "DEFAULT": [0.08, 0.06, 0.1, 1.0], "GROUP": "Background" },
    { "NAME": "transparentBg","LABEL": "Transparent BG (lines only)", "TYPE": "bool", "DEFAULT": false, "GROUP": "Background" }
  ],
  "PASSES": [
    { "TARGET": "netBuf", "FLOAT": true },
    {}
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// Metro — Shades of New York 004
//   pass 0  netBuf: a 16×16 corner of texels holds the network. Texel (i, L)
//           = control point i of line L (x, y, arc length so far, 1);
//           texel (15, L) = that line's bounding box. netPt() (the line
//           table) is evaluated only here, from one call site.
//   pass 1  the map: an orbiting pinhole camera hits the city plane; every
//           pixel finds its nearest track (bounding boxes skip far lines),
//           then draws land, water, streets, lines, trains and stations.
// ─────────────────────────────────────────────────────────────────────────

#define PI  3.14159265359
#define TAU 6.28318530718
#define NL 16

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


vec2 h22(vec2 p) { vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031, 0.1030, 0.0973)); p3 += dot(p3, p3.yzx + 33.33); return fract((p3.xx + p3.yz) * p3.zy); }
float vnoise(vec2 x) {
    vec2 i = floor(x), f = fract(x); f = f * f * (3.0 - 2.0 * f);
    return mix(mix(h12(i), h12(i + vec2(1, 0)), f.x), mix(h12(i + vec2(0, 1)), h12(i + vec2(1, 1)), f.x), f.y);
}
vec3 hueRot(vec3 c, float h) {
    if (h <= 0.0) return c;
    float a = h * TAU; vec3 k = vec3(0.57735); float ca = cos(a);
    return c * ca + cross(k, c) * sin(a) + k * dot(k, c) * (1.0 - ca);
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

// generated by metro_data.py: control point i of line L (x > 90 = end of line)
vec2 netPt(float L, float i) {
    if (L < 0.5) {
        if (i < 0.5) return vec2(-0.8231, 1.4769);
        else if (i < 1.5) return vec2(-0.8154, 1.0769);
        else if (i < 2.5) return vec2(-0.8031, 0.7385);
        else if (i < 3.5) return vec2(-0.7769, 0.4615);
        else if (i < 4.5) return vec2(-0.6846, 0.3231);
        else if (i < 5.5) return vec2(-0.6646, 0.0000);
        else if (i < 6.5) return vec2(-0.6385, -0.3846);
        else if (i < 7.5) return vec2(-0.5692, -0.4923);
        else if (i < 8.5) return vec2(-0.2769, -0.5538);
        else if (i < 9.5) return vec2(-0.0923, -0.6462);
        return vec2(99.0);
    }
    else if (L < 1.5) {
        if (i < 0.5) return vec2(-0.0923, -0.6462);
        else if (i < 1.5) return vec2(0.1231, -0.7231);
        else if (i < 2.5) return vec2(0.3846, -0.7692);
        else if (i < 3.5) return vec2(0.6615, -1.0462);
        return vec2(99.0);
    }
    else if (L < 2.5) {
        if (i < 0.5) return vec2(0.3846, -0.7692);
        else if (i < 1.5) return vec2(0.6000, -0.6000);
        else if (i < 2.5) return vec2(0.8154, -0.3538);
        return vec2(99.0);
    }
    else if (L < 3.5) {
        if (i < 0.5) return vec2(-0.4077, 1.5231);
        else if (i < 1.5) return vec2(-0.4123, 0.8923);
        else if (i < 2.5) return vec2(-0.4000, 0.4615);
        else if (i < 3.5) return vec2(-0.3846, 0.0000);
        else if (i < 4.5) return vec2(-0.3769, -0.3846);
        else if (i < 5.5) return vec2(-0.3692, -0.6462);
        else if (i < 6.5) return vec2(-0.1231, -0.7077);
        else if (i < 7.5) return vec2(0.1231, -0.7385);
        else if (i < 8.5) return vec2(0.3692, -0.8000);
        else if (i < 9.5) return vec2(0.6769, -1.0154);
        return vec2(99.0);
    }
    else if (L < 4.5) {
        if (i < 0.5) return vec2(-0.4000, 0.8769);
        else if (i < 1.5) return vec2(-0.2154, 0.9231);
        else if (i < 2.5) return vec2(-0.0769, 1.0308);
        else if (i < 3.5) return vec2(0.2462, 1.3692);
        return vec2(99.0);
    }
    else if (L < 5.5) {
        if (i < 0.5) return vec2(-0.9000, 1.2769);
        else if (i < 1.5) return vec2(-0.8692, 1.0308);
        else if (i < 2.5) return vec2(-0.7692, 0.8462);
        else if (i < 3.5) return vec2(-0.7615, 0.4615);
        else if (i < 4.5) return vec2(-0.7508, -0.0769);
        else if (i < 5.5) return vec2(-0.5692, -0.1385);
        else if (i < 6.5) return vec2(-0.5462, -0.4615);
        else if (i < 7.5) return vec2(-0.0923, -0.4923);
        else if (i < 8.5) return vec2(0.1538, -0.4462);
        else if (i < 9.5) return vec2(0.4615, -0.2769);
        return vec2(99.0);
    }
    else if (L < 6.5) {
        if (i < 0.5) return vec2(0.4615, -0.2769);
        else if (i < 1.5) return vec2(0.7692, -0.1538);
        else if (i < 2.5) return vec2(0.9231, -0.0923);
        else if (i < 3.5) return vec2(1.0769, 0.3385);
        return vec2(99.0);
    }
    else if (L < 7.5) {
        if (i < 0.5) return vec2(-0.7538, 0.3385);
        else if (i < 1.5) return vec2(-0.4308, 0.3231);
        else if (i < 2.5) return vec2(-0.1231, 0.3077);
        else if (i < 3.5) return vec2(0.2615, 0.5538);
        else if (i < 4.5) return vec2(0.7385, 0.5538);
        else if (i < 5.5) return vec2(0.9385, 0.3692);
        else if (i < 6.5) return vec2(1.0615, 0.4615);
        return vec2(99.0);
    }
    else if (L < 8.5) {
        if (i < 0.5) return vec2(-0.4923, 1.5077);
        else if (i < 1.5) return vec2(-0.4800, 0.9385);
        else if (i < 2.5) return vec2(-0.5462, 0.4615);
        else if (i < 3.5) return vec2(-0.5385, -0.1538);
        else if (i < 4.5) return vec2(-0.3538, -0.2462);
        else if (i < 5.5) return vec2(-0.0923, -0.4308);
        else if (i < 6.5) return vec2(0.1385, -0.8000);
        else if (i < 7.5) return vec2(0.2615, -1.0769);
        else if (i < 8.5) return vec2(0.3846, -1.3538);
        return vec2(99.0);
    }
    else if (L < 9.5) {
        if (i < 0.5) return vec2(-0.5462, 0.4615);
        else if (i < 1.5) return vec2(-0.3538, 0.4769);
        else if (i < 2.5) return vec2(0.0000, 0.5692);
        else if (i < 3.5) return vec2(0.3077, 0.5846);
        else if (i < 4.5) return vec2(0.8308, 0.6154);
        return vec2(99.0);
    }
    else if (L < 10.5) {
        if (i < 0.5) return vec2(-0.0308, 0.7692);
        else if (i < 1.5) return vec2(-0.0846, 0.4769);
        else if (i < 2.5) return vec2(-0.3846, 0.4538);
        else if (i < 3.5) return vec2(-0.6231, 0.4154);
        else if (i < 4.5) return vec2(-0.6308, 0.1538);
        else if (i < 5.5) return vec2(-0.4769, 0.0000);
        else if (i < 6.5) return vec2(-0.4385, -0.3846);
        else if (i < 7.5) return vec2(-0.3385, -0.5385);
        else if (i < 8.5) return vec2(0.0308, -0.7692);
        else if (i < 9.5) return vec2(0.3846, -1.3538);
        return vec2(99.0);
    }
    else if (L < 11.5) {
        if (i < 0.5) return vec2(-0.8462, 0.1538);
        else if (i < 1.5) return vec2(-0.6154, 0.1846);
        else if (i < 2.5) return vec2(-0.3692, 0.2154);
        else if (i < 3.5) return vec2(-0.1231, 0.3077);
        else if (i < 4.5) return vec2(0.1538, 0.5538);
        else if (i < 5.5) return vec2(0.5231, 0.9077);
        return vec2(99.0);
    }
    else if (L < 12.5) {
        if (i < 0.5) return vec2(-0.1231, 0.3077);
        else if (i < 1.5) return vec2(-0.1923, 0.0000);
        else if (i < 2.5) return vec2(-0.0923, -0.2615);
        else if (i < 3.5) return vec2(0.0923, -0.4154);
        else if (i < 4.5) return vec2(0.1154, -0.7692);
        else if (i < 5.5) return vec2(0.2923, -0.9231);
        return vec2(99.0);
    }
    else if (L < 13.5) {
        if (i < 0.5) return vec2(-0.7538, -0.0615);
        else if (i < 1.5) return vec2(-0.3846, -0.0692);
        else if (i < 2.5) return vec2(-0.0923, 0.0000);
        else if (i < 3.5) return vec2(0.2000, -0.0923);
        else if (i < 4.5) return vec2(0.4769, -0.2769);
        else if (i < 5.5) return vec2(0.7538, -0.5846);
        return vec2(99.0);
    }
    else if (L < 14.5) {
        if (i < 0.5) return vec2(-0.3846, -0.4615);
        else if (i < 1.5) return vec2(-0.2000, -0.2308);
        else if (i < 2.5) return vec2(0.4615, -0.2769);
        else if (i < 3.5) return vec2(0.6923, 0.2308);
        else if (i < 4.5) return vec2(1.0615, 0.4769);
        return vec2(99.0);
    }
    else if (L < 15.5) {
        if (i < 0.5) return vec2(-0.4154, -0.8000);
        else if (i < 1.5) return vec2(-0.4769, -0.9385);
        else if (i < 2.5) return vec2(-0.6154, -1.0769);
        else if (i < 3.5) return vec2(-0.7846, -1.3385);
        return vec2(99.0);
    }
    else if (L < 16.5) {
        if (i < 0.5) return vec2(-0.2154, 0.8154);
        else if (i < 1.5) return vec2(0.0000, 0.8615);
        else if (i < 2.5) return vec2(0.1231, 0.7385);
        else if (i < 3.5) return vec2(0.2308, 0.8154);
        else if (i < 4.5) return vec2(0.4308, 0.8769);
        else if (i < 5.5) return vec2(0.6923, 0.9538);
        else if (i < 6.5) return vec2(1.3231, 1.0769);
        else if (i < 7.5) return vec2(1.3231, -0.2769);
        else if (i < 8.5) return vec2(1.0769, -0.2769);
        else if (i < 9.5) return vec2(0.9692, -0.3692);
        else if (i < 10.5) return vec2(0.7692, -0.3385);
        else if (i < 11.5) return vec2(0.5538, -0.5077);
        else if (i < 12.5) return vec2(0.5538, -0.7231);
        else if (i < 13.5) return vec2(0.8154, -0.8615);
        else if (i < 14.5) return vec2(0.8308, -1.0769);
        else if (i < 15.5) return vec2(0.6154, -1.2769);
        else if (i < 16.5) return vec2(0.3846, -1.4000);
        else if (i < 17.5) return vec2(0.1538, -1.3077);
        else if (i < 18.5) return vec2(-0.0923, -1.2154);
        else if (i < 19.5) return vec2(-0.1538, -1.0000);
        else if (i < 20.5) return vec2(-0.2154, -0.7538);
        else if (i < 21.5) return vec2(-0.3154, -0.6615);
        else if (i < 22.5) return vec2(-0.2769, -0.5077);
        else if (i < 23.5) return vec2(-0.2077, -0.3385);
        else if (i < 24.5) return vec2(-0.1923, -0.1231);
        else if (i < 25.5) return vec2(-0.2385, 0.1077);
        else if (i < 26.5) return vec2(-0.2769, 0.3385);
        else if (i < 27.5) return vec2(-0.2462, 0.6154);
        return vec2(99.0);
    }
    else if (L < 17.5) {
        if (i < 0.5) return vec2(-0.6923, 1.6308);
        else if (i < 1.5) return vec2(0.6154, 1.6308);
        else if (i < 2.5) return vec2(0.6154, 1.3231);
        else if (i < 3.5) return vec2(0.3538, 1.2769);
        else if (i < 4.5) return vec2(0.2769, 1.0923);
        else if (i < 5.5) return vec2(0.0923, 0.9462);
        else if (i < 6.5) return vec2(-0.1538, 0.9000);
        else if (i < 7.5) return vec2(-0.3538, 0.8385);
        else if (i < 8.5) return vec2(-0.4769, 0.8615);
        else if (i < 9.5) return vec2(-0.6154, 1.0154);
        else if (i < 10.5) return vec2(-0.6954, 1.3077);
        return vec2(99.0);
    }
    else if (L < 18.5) {
        if (i < 0.5) return vec2(-0.7538, -0.8615);
        else if (i < 1.5) return vec2(-0.4308, -0.7923);
        else if (i < 2.5) return vec2(-0.3462, -0.9077);
        else if (i < 3.5) return vec2(-0.3769, -1.1231);
        else if (i < 4.5) return vec2(-0.5538, -1.3538);
        else if (i < 5.5) return vec2(-0.7692, -1.4000);
        else if (i < 6.5) return vec2(-0.9077, -1.2923);
        else if (i < 7.5) return vec2(-0.8692, -1.0462);
        return vec2(99.0);
    }
    else if (L < 19.5) {
        if (i < 0.5) return vec2(-1.6923, 1.6308);
        else if (i < 1.5) return vec2(-0.9923, 1.6308);
        else if (i < 2.5) return vec2(-0.9769, 0.7692);
        else if (i < 3.5) return vec2(-0.9462, 0.1538);
        else if (i < 4.5) return vec2(-0.9077, -0.4615);
        else if (i < 5.5) return vec2(-0.8308, -0.7692);
        else if (i < 6.5) return vec2(-0.9692, -1.0769);
        else if (i < 7.5) return vec2(-0.9538, -1.6923);
        else if (i < 8.5) return vec2(-1.6923, -1.6923);
        return vec2(99.0);
    }
    return vec2(99.0);
}
float netColIdx(float L) {
    if (L < 0.5) return 2.0;
    else if (L < 1.5) return 2.0;
    else if (L < 2.5) return 2.0;
    else if (L < 3.5) return 3.0;
    else if (L < 4.5) return 3.0;
    else if (L < 5.5) return 4.0;
    else if (L < 6.5) return 4.0;
    else if (L < 7.5) return 4.0;
    else if (L < 8.5) return 1.0;
    else if (L < 9.5) return 1.0;
    else if (L < 10.5) return 0.0;
    else if (L < 11.5) return 6.0;
    else if (L < 12.5) return 5.0;
    else if (L < 13.5) return 8.0;
    else if (L < 14.5) return 7.0;
    else if (L < 15.5) return 9.0;
    return 0.0;
}


// line colours (trunk-line colours, no marks)
vec3 lineColBase(float i) {
    i = mod(floor(i + 0.5), 10.0);
    if (i < 0.5) return vec3(0.99, 0.80, 0.04);    // yellow
    if (i < 1.5) return vec3(1.0, 0.39, 0.10);     // orange
    if (i < 2.5) return vec3(0.93, 0.21, 0.18);    // red
    if (i < 3.5) return vec3(0.0, 0.62, 0.26);     // green
    if (i < 4.5) return vec3(0.05, 0.30, 0.78);    // blue
    if (i < 5.5) return vec3(0.45, 0.80, 0.28);    // lime
    if (i < 6.5) return vec3(0.73, 0.20, 0.68);    // purple
    if (i < 7.5) return vec3(0.62, 0.42, 0.22);    // brown
    if (i < 8.5) return vec3(0.62, 0.63, 0.65);    // grey
    return vec3(0.12, 0.20, 0.55);                 // Staten Island blue
}

// ── geography (reference-photo pixel coords → city units) ───────────────
vec4 netTex(float i, float L) { return texture2D(netBuf, vec2(i + 0.5, L + 0.5) / RENDERSIZE.xy); }
vec2 PX(float x, float y) { return vec2((x - 700.0) / 650.0, (1000.0 - y) / 650.0); }
float sdSeg(vec2 p, vec2 a, vec2 b) { vec2 pa = p - a, ba = b - a; float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0); return length(pa - ba * h); }
float sdBox(vec2 p, vec2 c, vec2 b) { vec2 q = abs(p - c) - b; return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0); }
float smin(float a, float b, float k) { float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0); return mix(b, a, h) - k * h * (1.0 - h); }
float gManh;     // distance inside Manhattan (for its street grid)
// signed distance to a land polygon stored in data row `row` (negative inside)
float polySD(float row, vec2 p) {
    vec2 a = netTex(0.0, row).xy, first = a;
    float d = dot(p - a, p - a), s = 1.0;
    for (int i = 1; i < 31; i++) {
        vec4 vb = netTex(float(i), row);
        bool last = vb.x > 90.0;
        vec2 b = last ? first : vb.xy;
        vec2 e = b - a, w = p - a;
        vec2 q = w - e * clamp(dot(w, e) / dot(e, e), 0.0, 1.0);
        d = min(d, dot(q, q));
        bool c1 = p.y >= a.y, c2 = p.y < b.y, c3 = e.x * w.y > e.y * w.x;
        if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) s = -s;
        if (last) break;
        a = b;
    }
    return s * sqrt(d);
}
float landSD(vec2 c) {
    float wob = 0.012 * (vnoise(c * 7.0) - 0.5) + 0.006 * (vnoise(c * 23.0) - 0.5) + 0.002 * (vnoise(c * 80.0) - 0.5);
    // Manhattan: a tapered chain of capsules
    float m = 9.0;
    m = min(m, sdSeg(c, PX(345.0, 1420.0), PX(272.0, 1250.0)) - 0.105);
    m = min(m, sdSeg(c, PX(272.0, 1250.0), PX(240.0, 1000.0)) - 0.16);
    m = min(m, sdSeg(c, PX(240.0, 1000.0), PX(226.0, 760.0)) - 0.17);
    m = min(m, sdSeg(c, PX(226.0, 760.0), PX(188.0, 560.0)) - 0.165);
    m = min(m, sdSeg(c, PX(188.0, 560.0), PX(142.0, 330.0)) - 0.135);
    m = min(m, sdSeg(c, PX(142.0, 330.0), PX(125.0, 120.0)) - 0.1);
    gManh = m;
    float li = polySD(float(NL), c);
    li = max(li, -(length((c - PX(1240.0, 1330.0)) / vec2(0.3, 0.1)) - 1.0) * 0.1);    // Jamaica Bay
    li = min(li, sdSeg(c, PX(1120.0, 1500.0), PX(1560.0, 1370.0)) - 0.03);            // the Rockaways
    float bx = polySD(float(NL) + 1.0, c);
    float si = polySD(float(NL) + 2.0, c);
    float nj = polySD(float(NL) + 3.0, c);
    return min(min(m, bx), min(li, min(si, nj))) + wob;
}
float parkSD(vec2 c) {
    float cp = sdBox(c, PX(210.0, 600.0), vec2(0.045, 0.2)) - 0.01;                   // the park in Manhattan
    float pp = length(c - PX(800.0, 1600.0)) - 0.07;                                    // a Brooklyn park
    float fm = length((c - PX(1050.0, 680.0)) / vec2(1.0, 1.6)) - 0.06;                 // a Queens park
    return min(cp, min(pp, fm));
}

// ── network data (pass 0) ───────────────────────────────────────────────
vec4 dataPass(vec2 fc) {
    if (fc.x > 32.0 || fc.y > float(NL) + 4.0) return vec4(0.0);
    float i = floor(fc.x), L = floor(fc.y);
    vec2 prev = vec2(0.0);
    float acc = 0.0;
    vec4 bb = vec4(99.0, 99.0, -99.0, -99.0);
    vec2 me = vec2(99.0);
    for (int k = 0; k < 30; k++) {
        float fk = float(k);
        vec2 q = netPt(L, fk);
        if (q.x > 90.0) break;
        if (k > 0 && fk <= i) acc += length(q - prev);
        if (fk == i) me = q;
        bb = vec4(min(bb.xy, q), max(bb.zw, q));
        prev = q;
    }
    if (i > 30.5) return bb;
    if (me.x > 90.0) return vec4(99.0, 99.0, 0.0, 0.0);
    return vec4(me, acc, 1.0);
}

// ── nearest track to a city point ───────────────────────────────────────
float gNd, gNs, gNside, gNL, gN2;
void trace(vec2 c, float maxD) {
    gNd = 9.0; gNL = -1.0; gNs = 0.0; gNside = 1.0; gN2 = 9.0;
    for (int L = 0; L < NL; L++) {
        float fL = float(L);
        vec4 bb = netTex(31.0, fL);
        if (c.x < bb.x - maxD || c.y < bb.y - maxD || c.x > bb.z + maxD || c.y > bb.w + maxD) continue;
        vec4 a = netTex(0.0, fL);
        float best = 9.0;
        for (int i = 1; i < 10; i++) {
            vec4 b = netTex(float(i), fL);
            if (b.x > 90.0) break;
            vec2 ab = b.xy - a.xy, pa = c - a.xy;
            float h = clamp(dot(pa, ab) / dot(ab, ab), 0.0, 1.0);
            float d = length(pa - ab * h);
            if (d < best) best = d;
            if (d < gNd) {
                gNd = d; gNL = fL; gNs = a.z + h * length(ab);
                gNside = ab.x * pa.y - ab.y * pa.x >= 0.0 ? 1.0 : -1.0;
            }
            a = b;
        }
        if (fL != gNL) gN2 = min(gN2, best);
    }
}

// a train pattern along arc length s, one direction (+1 / -1): returns
// (body 0..1, position in train 0 tail → 1 head, distance ahead of head, distance behind tail)
float gTrainT;
vec4 trains(float s, float dir, float L) {
    float D = (0.3 + 0.16 * h11(L * 3.7 + 1.0)) / trainDensity;
    float len = 0.07;
    float v = 0.1 * (0.8 + 0.4 * h11(L * 9.1 + 2.0));
    float ph = (s - dir * v * gTrainT) / D + h11(L * 5.3 + dir) ;
    float f = fract(ph) * D;                                  // arc distance past the train's rear (in s)
    float body = step(f, len);
    float u = f / len;
    float head = dir > 0.0 ? u : 1.0 - u;
    float ahead = dir > 0.0 ? f - len : D - f;               // gap in front of the head
    float behind = dir > 0.0 ? D - f : f - len;               // gap behind the tail
    if (body > 0.5) { ahead = 0.0; behind = 0.0; }
    return vec4(body, head, ahead, behind);
}

vec3 gLand, gWater, gCoast, gPark, gStreet;
float gThemeLines;   // 0 colour lines, 1 white (blueprint), 2 ink

void main() {
    showDynamics();
    vec2 R = RENDERSIZE.xy, fc = gl_FragCoord.xy, uv = fc / R;
    if (PASSINDEX == 0) { gl_FragColor = dataPass(fc); return; }
    vec2 p = (fc - 0.5 * R) / R.y;
    float ar = audioReact;
    float th = float(theme);

    // ── theme ──
    if (th < 0.5)      { gLand = vec3(0.075, 0.078, 0.09); gWater = vec3(0.02, 0.035, 0.07); gCoast = vec3(0.18, 0.24, 0.36); gPark = vec3(0.05, 0.11, 0.07); gStreet = vec3(0.13, 0.13, 0.15); gThemeLines = 0.0; }
    else if (th < 1.5) { gLand = vec3(0.93, 0.92, 0.88); gWater = vec3(0.62, 0.74, 0.88); gCoast = vec3(0.75, 0.8, 0.86); gPark = vec3(0.78, 0.86, 0.72); gStreet = vec3(0.86, 0.85, 0.82); gThemeLines = 0.0; }
    else if (th < 2.5) { gLand = vec3(0.012, 0.01, 0.02); gWater = vec3(0.0); gCoast = vec3(0.35, 0.1, 0.6); gPark = vec3(0.0, 0.04, 0.03); gStreet = vec3(0.04, 0.03, 0.06); gThemeLines = 0.0; }
    else if (th < 3.5) { gLand = vec3(0.07, 0.2, 0.42); gWater = vec3(0.04, 0.13, 0.3); gCoast = vec3(0.75, 0.88, 1.0); gPark = vec3(0.08, 0.24, 0.46); gStreet = vec3(0.13, 0.3, 0.52); gThemeLines = 1.0; }
    else               { gLand = vec3(0.95, 0.93, 0.88); gWater = vec3(0.9, 0.88, 0.83); gCoast = vec3(0.1); gPark = vec3(0.88, 0.86, 0.8); gStreet = vec3(0.86, 0.84, 0.79); gThemeLines = 2.0; }
    gLand *= landTint.rgb; gPark *= landTint.rgb; gWater *= waterTint.rgb;
    bool neon = th > 1.5 && th < 2.5;
    bool dark = th < 0.5 || neon || (th > 2.5 && th < 3.5);

    // ── camera: orbit + tour + tilt + zoom ──
    float T = TIME;
    float yaw = T * spin * 0.06 + audioMidTime * 0.01 * ar * spin + 1.2 * gBuild * gBuild * sign(spin + 1e-3);
    float pitch = mix(1.5707963, 0.42, clamp(tilt + 0.25 * gBuild * gBuild, 0.0, 1.0));
    pitch -= 0.05 * gRest;
    float dist = mix(5.2, 1.3, zoomAmt) * (1.0 - 0.18 * gDrop * gDrop) * (1.0 + 0.12 * gRest);
    vec2 tg = PX(520.0, 1000.0) + tour * vec2(0.45 * sin(T * 0.031) + 0.2 * sin(T * 0.077), 0.55 * sin(T * 0.023 + 1.0)) * (0.4 + 0.6 * zoomAmt);
    vec3 fwd = vec3(cos(pitch) * sin(yaw), cos(pitch) * cos(yaw), -sin(pitch));
    vec3 rgt = vec3(cos(yaw), -sin(yaw), 0.0);
    vec3 upv = cross(rgt, fwd);
    vec3 ro = vec3(tg, 0.0) - fwd * dist;
    float fl = 1.7;
    vec2 pp = p + (h21(floor(TIME * 30.0)) - 0.5) * 0.004 * smoothstep(0.6, 1.0, gBuild);
    vec3 rd = normalize(pp.x * rgt + pp.y * upv + fl * fwd);

    // hand → city point
    vec2 mp = (mousePos - 0.5) * vec2(R.x / R.y, 1.0);
    vec3 rdm = normalize(mp.x * rgt + mp.y * upv + fl * fwd);
    vec2 mC = (ro + rdm * (-ro.z / min(rdm.z, -0.01))).xy;
    float handOn = step(0.5, float(interact)) * step(rdm.z, -0.01);

    vec3 bg = backdrop(uv);
    vec3 col;
    float alpha = 1.0;
    if (rd.z > -0.01) {
        col = bg + vec3(0.5, 0.35, 0.6) * 0.06 * exp(-abs(pp.y) * 6.0) * float(dark);   // city glow on the horizon
        alpha = transparentBg ? 0.0 : 1.0;
    } else {
        float t = -ro.z / rd.z;
        vec2 c = (ro + rd * t).xy;
        float fp = t / (fl * R.y) / max(-rd.z, 0.2);                 // city units per pixel
        float aa = fp * 1.3;

        // ── land, water, coast, parks ──
        float ld = landSD(c);
        float land = smoothstep(aa, -aa, ld);
        float Tw = TIME * 0.15;
        float rip = vnoise(c * 38.0 + vec2(Tw, -Tw * 0.7)) * 0.6 + vnoise(c * 90.0 - vec2(Tw * 1.3, Tw)) * 0.4;
        vec3 water = gWater * (0.85 + 0.3 * rip);
        // reflections of the city lights on the water near the shore
        water += gCoast * 0.25 * exp(-max(ld, 0.0) / 0.05) * (0.6 + 0.4 * rip) * float(dark);
        water += vec3(1.0) * pow(rip, 8.0) * 0.12 * float(dark);
        vec3 landC = gLand * (0.9 + 0.12 * vnoise(c * 6.0)) * (0.88 + 0.2 * smoothstep(0.0, -0.05, ld));
        landC = mix(landC, gPark, smoothstep(aa, -aa, parkSD(c)) * 0.9);
        if (camMix > 0.001) {
            vec3 ci = texture2D(camTex, uv).rgb;
            landC = mix(landC, ci * (dark ? 0.55 : 1.0), camMix);
            water = mix(water, ci * (dark ? 0.25 : 0.7), camMix * 0.6);
        }
        // streets: Manhattan's tight grid, a looser grid elsewhere; tiny traffic lights move along them
        float inM = smoothstep(0.01, -0.02, gManh);
        vec2 sg = inM > 0.5 ? c * vec2(1.0 / 0.024, 1.0 / 0.011) : (c * mat2(0.96, -0.28, 0.28, 0.96)) / 0.03;
        vec2 sf = abs(fract(sg) - 0.5);
        float lw = fp / (inM > 0.5 ? 0.011 : 0.03) * 0.9;
        float st = max(smoothstep(lw + 0.04, 0.0, 0.5 - sf.x), smoothstep(lw + 0.04, 0.0, 0.5 - sf.y));
        st *= streets * land * smoothstep(0.03, 0.12, fp < 0.004 ? 1.0 : 0.004 / fp);
        vec2 cell = floor(sg);
        float dashA = step(0.93, fract(sg.y * 1.0 + TIME * (0.6 + h12(cell.xx)) * 1.5 + h12(cell.xx + 3.0)));
        float dashB = step(0.93, fract(sg.x * 1.0 - TIME * (0.6 + h12(cell.yy)) * 1.5 + h12(cell.yy + 5.0)));
        float traffic = (smoothstep(0.12, 0.02, 0.5 - sf.x) * dashA + smoothstep(0.12, 0.02, 0.5 - sf.y) * dashB) * streets * land * step(fp, 0.004);
        landC = mix(landC, gStreet, st * 0.8);
        // coastline: a crisp edge + a soft drop shadow over the water
        float coast = smoothstep(aa * 1.5 + 0.002, 0.0, abs(ld));
        vec3 base = mix(water * (1.0 - 0.35 * exp(-max(ld, 0.0) / 0.012)), landC, land);
        base = mix(base, gCoast, coast * 0.75);
        base += (dark ? vec3(1.0, 0.82, 0.55) : vec3(0.9, 0.2, 0.1)) * traffic * 0.6;

        // ── the network ──
        float w = max(0.0095 * lineWidth, fp * 1.4) * (1.0 + 0.12 * bassP() * ar);
        float glowR = 0.05 * (0.6 + 0.4 * glowAmt);
        trace(c, glowR * 2.5);
        float L = gNL;
        float d = gNd;
        gTrainT = TIME * trainSpeed * (0.55 + 0.45 * gLift) + audioMidTime * 0.6 * ar * trainSpeed + 3.0 * gBuild * gBuild * trainSpeed;
        // line colour (with drift: lines trade colours now and then)
        float ci = netColIdx(L);
        float dk = TIME * 0.035 + h11(L * 7.3) * 4.0;
        float dOn = h11(L * 3.3 + floor(dk) * 1.7 + 0.71) < colourDrift ? 1.0 : 0.0;
        float dNext = h11(L * 3.3 + floor(dk + 1.0) * 1.7 + 0.71) < colourDrift ? 1.0 : 0.0;
        vec3 lcA = lineColBase(ci + dOn * floor(h11(floor(dk) + L) * 9.0 + 1.0));
        vec3 lcB = lineColBase(ci + dNext * floor(h11(floor(dk) + 1.0 + L) * 9.0 + 1.0));
        vec3 lc = mix(lcA, lcB, smoothstep(0.85, 1.0, fract(dk)));
        lc = hueRot(lc, hueShift);
        if (gThemeLines > 0.5 && gThemeLines < 1.5) lc = mix(vec3(0.85, 0.95, 1.0), lc, 0.15);
        if (neon) lc = clamp(lc * 1.25 + 0.05, 0.0, 1.0);

        float lineM = smoothstep(w + aa, w - aa, d);
        float casing = smoothstep(w * 1.45 + aa, w * 1.45 - aa, d);
        float lat = gNd * gNside;                                     // signed offset across the line
        // beat ring out of Midtown, drop shockwave, hand pulse
        vec2 O = PX(300.0, 880.0);
        float dO = length(c - O);
        float ringR = (1.0 - audioBeatPulse) * 1.6;
        float beatRing = exp(-pow((dO - ringR) / 0.08, 2.0)) * audioBeatPulse * ar;
        float shock = exp(-pow((dO - (1.0 - gDrop) * 3.0) / 0.18, 2.0)) * gDrop;
        float dM = length(c - mC);
        float handPulse = handOn * step(0.5, mouseDown) * exp(-pow((dM - fract(TIME * 0.8) * 0.8) / 0.05, 2.0)) * (1.0 - fract(TIME * 0.8));
        float lightUp = beatRing * 1.2 + shock * 2.0 + handPulse * 1.5 + 0.6 * gPeak * bassP() * ar;
        float handLamp = handOn * exp(-dM * dM / 0.012);

        // glow halo
        float glow = exp(-d / (glowR * 0.35)) * glowAmt * (dark ? 0.55 : 0.18) * (0.55 + 0.45 * gLift + lightUp);
        base += lc * glow * (neon ? 1.6 : 1.0);

        // the line itself: a slightly raised tube with two tracks
        float tube = 1.0 - pow(clamp(abs(lat) / w, 0.0, 1.0), 2.0);
        vec3 lineCol = lc * (0.72 + 0.35 * tube) * (dark ? 0.85 : 1.0);
        float rail = smoothstep(fp * 1.2, 0.0, abs(abs(lat) - w * 0.45)) * step(fp, w * 0.25);
        lineCol = mix(lineCol, lineCol * 0.6, rail * 0.5);
        lineCol += lc * lightUp * 0.8 + vec3(1.0) * shock * 0.5;
        vec3 caseC = dark ? vec3(0.0) : vec3(1.0);
        if (gThemeLines > 1.5) { caseC = gLand; lineCol = vec3(0.07) + lc * lightUp; }
        base = mix(base, caseC, casing * 0.85);
        base = mix(base, lineCol, lineM);

        // trains on both tracks: lit cars, headlights, tail lights, light trails
        float dir = lat >= 0.0 ? 1.0 : -1.0;
        vec4 tr = trains(gNs, dir, L);
        float lane = smoothstep(w * 0.95, w * 0.75, abs(lat)) * smoothstep(0.0, w * 0.12, abs(lat));
        float car = smoothstep(0.0, 0.06, fract(tr.y * 5.0)) * smoothstep(1.0, 0.94, fract(tr.y * 5.0));
        vec3 carC = mix(vec3(1.0), lc, 0.35) * (1.2 + 0.8 * bassP() * ar + lightUp);
        float body = tr.x * lane * mix(0.75, 1.0, car);
        base = mix(base, carC, body);
        base += vec3(1.0, 0.95, 0.8) * tr.x * smoothstep(0.85, 1.0, tr.y) * lane * 1.5;           // headlight cab
        base += vec3(1.0, 0.95, 0.8) * exp(-tr.z / 0.02) * lane * 0.9 * step(tr.x, 0.5);           // beam ahead
        base += vec3(1.0, 0.1, 0.05) * tr.x * smoothstep(0.12, 0.0, tr.y) * lane * 0.9;            // tail lights
        base += lc * exp(-tr.w / (0.09 * trailAmt + 1e-3)) * lane * trailAmt * 0.9 * step(tr.x, 0.5); // light trail
        base += lc * glowAmt * 0.6 * tr.x * exp(-d / (w * 2.0)) * (dark ? 1.0 : 0.3);              // train glow

        // stations: dots along every line, flashing as trains pull through
        float sp = 0.085;
        float so = h11(L * 2.3) * sp;
        float sk = floor((gNs + so) / sp);
        float sC = sk * sp + 0.5 * sp - so;
        float su = gNs - sC;
        float sr = w * 0.62;
        float sd = length(vec2(su, d));
        float sMask = smoothstep(sr + aa, sr - aa, sd) * step(0.02, sC) * step(fp, w * 1.2);
        vec4 tA = trains(sC, 1.0, L), tB = trains(sC, -1.0, L);
        float flash = max(tA.x, tB.x) * stationFlash;
        float twinkle = highP() * ar * step(0.8, h11(sk * 7.1 + L * 3.0 + floor(TIME * 10.0)));
        vec3 stC = dark ? vec3(0.95) : vec3(1.0);
        stC += (lc * 0.8 + 0.6) * (flash + twinkle);
        float ring = smoothstep(sr * 0.55 + aa, sr * 0.55 - aa, sd);
        base = mix(base, gThemeLines > 1.5 ? vec3(0.07) : vec3(0.02), sMask);
        base = mix(base, stC, ring * sMask);
        base += lc * flash * exp(-sd / (w * 1.5)) * 0.5 * (dark ? 1.0 : 0.4);

        // the hand lights its district
        base += (dark ? vec3(0.35, 0.3, 0.25) : vec3(0.1)) * handLamp + lc * handLamp * exp(-d / 0.02) * 0.6;

        // depth: fog toward the horizon, show lift
        float fog = smoothstep(dist * 1.2, dist * 3.2, t);
        col = mix(base * (0.82 + 0.3 * gLift), bg, fog);
        alpha = transparentBg ? clamp(max(lineM, glow) + body, 0.0, 1.0) * (1.0 - fog) : 1.0;
    }

    // ── body layer: your silhouette lights the city, your skeleton is a subway line ──
    float bodyOn = (float(bodyMode) > 0.5 && PASSINDEX == 1) ? smoothstep(0.3, 0.7, bodyPresence()) : 0.0;
    if (bodyOn > 0.01) {
        float asp = R.x / R.y;
        if (bodyUseSil() > 0.5) {
            float m = bodySeg(uv);
            vec3 camC = texture2D(camTex, clamp(bodyCamUV(uv), 0.0, 1.0)).rgb;
            col = mix(col, col * 1.35 + vec3(0.04, 0.035, 0.05), m * bodyOn * 0.6);
            col = mix(col, camC * 0.8 + col * 0.4, m * bodyOn * bodyCamMix);
            float e = 2.0 / R.y;
            float edge = abs(bodySeg(uv + vec2(e, 0.0)) - bodySeg(uv - vec2(e, 0.0))) + abs(bodySeg(uv + vec2(0.0, e)) - bodySeg(uv - vec2(0.0, e)));
            col += vec3(0.6, 0.7, 1.0) * clamp(edge, 0.0, 1.0) * 0.35 * bodyOn;
        }
        if (bodyUseSkel() > 0.5) {
            float bd = boneDist(p, asp);
            float bw = 0.011 * bodyLineW;
            float pxs = 1.0 / R.y;
            vec3 bc = lineColBase(gBoneId < 1.5 ? 2.0 : gBoneId < 3.5 ? 0.0 : gBoneId < 7.5 ? 3.0 : 4.0);
            bc = hueRot(bc, hueShift);
            float line = smoothstep(bw + pxs, bw - pxs, bd);
            float casing = smoothstep(bw * 1.5 + pxs, bw * 1.5 - pxs, bd);
            col += bc * exp(-bd / (bw * 3.0)) * 0.5 * glowAmt * bodyOn;
            col = mix(col, vec3(0.0), casing * 0.8 * bodyOn);
            col = mix(col, bc * (0.85 + 0.3 * bassP() * ar), line * bodyOn);
            // trains run along every limb
            float along = gBoneId * 1.7 + gBoneT;
            float tf = fract(along * 1.5 - TIME * trainSpeed * 0.7);
            float trainB = step(tf, 0.22) * smoothstep(bw * 0.9, bw * 0.4, bd);
            col = mix(col, mix(vec3(1.0), bc, 0.3) * 1.4, trainB * bodyOn);
            col += vec3(1.0, 0.95, 0.8) * smoothstep(0.18, 0.22, tf) * step(tf, 0.22) * line * bodyOn;
            // a station at every joint, flashing on the beat
            float jd = 9.0;
            for (int k = 0; k < 13; k++) {
                float jid = k == 0 ? 0.0 : k < 7 ? 10.0 + float(k) : k < 9 ? 22.0 + float(k - 6) : 24.0 + float(k - 8);
                vec3 j = bodyJoint(jid);
                if (j.z < 0.4) continue;
                jd = min(jd, length(p - (j.xy - 0.5) * vec2(asp, 1.0)));
            }
            float sr = bw * 1.6;
            float st = smoothstep(sr + pxs, sr - pxs, jd);
            float stIn = smoothstep(sr * 0.6 + pxs, sr * 0.6 - pxs, jd);
            col = mix(col, vec3(0.02), st * bodyOn);
            col = mix(col, vec3(1.0) * (1.0 + audioBeatPulse * ar * stationFlash), stIn * bodyOn);
        }
    }
    col += vec3(0.9, 0.95, 1.0) * gDrop * gDrop * 0.08;
    vec2 dc = uv - 0.5;
    col *= 1.0 - 0.3 * smoothstep(0.35, 1.0, length(dc * vec2(R.x / R.y, 1.0)));
    col += (h12(fc + fract(TIME * 13.7) * 91.0) - 0.5) * 0.012;
    col += (h12(fc * 1.03 + fract(TIME * 7.1) * 57.0) + h12(fc * 0.97 + 11.0) - 1.0) * (1.5 / 255.0);
    col = studioPalette(clamp(col, 0.0, 1.0));
    gl_FragColor = vec4(clamp(col, 0.0, 1.0), alpha);
}
