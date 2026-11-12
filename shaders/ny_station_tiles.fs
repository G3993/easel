/*{
  "DESCRIPTION": "Station Tiles — Shades of New York 003. The tiled walls of the subway, built tile by tile in glossy, handmade HD ceramic. Four walls flow into each other: White Subway Squares (pillowed glossy squares with crisp tube-light highlights on every top edge and thin dark grout), Black Glass Mosaic (small hand-glazed black tiles, wavy and liquid, with cream grout and silvery reflections), Greek Key Band (the classic station frieze of brick-coursed tesserae: cream, yellow and tan rows, a maroon stripe, a green meander with white-framed maroon boxes), and Ornament Panels (a maroon field of upright tesserae around a white frame and a central motif whose white contour band follows the shape tile by tile, filled with blue, yellow and tan). The motif keeps morphing: diamond, circle, star, octagon, flower and cross, with tesserae changing colour one by one as the outline sweeps through them. The walls don't fade into each other, they are rebuilt in time with the music. The shader keeps its own beat and phrase count (locked to the track's tempo when there is one), changes wall on the downbeat of a phrase (every 2, 4, 8 or 16 bars), and every tile lands on the beat grid. Each wall arrives with its own choreography: the white squares split into four and turn to black glass ring by ring from the centre; the frieze is laid row by row out from its centre line; the ornament panel is traced (frame and motif outline drawn around like a pen stroke), then filled from the middle, then the maroon field turns over; the white wall comes back as a split-flap board sweeping top to bottom. Tiles show their glazed edge as they turn, cast shadows as they lift, flash as they face the lights, and the grout lights up along the moving front. A drop slams the next wall in as a shockwave ring. The grid between the tiles is alive: light runs along the grout lines and the whole grid flashes on the beat, tiles lift off the wall in waves and cast shadows into the grout, and tiles tilt so glints race across the glaze. Every tile has its own colour variance, glaze waviness, pillowed edge, chips and craquelure, lit by a reflection of the station's fluorescent tubes plus a passing train light. Camera Mosaic turns any wall into a mosaic portrait from the camera image. Body tracking (MediaPipe pose + segmentation) makes you set the tile size: the station wall stays outside you, your silhouette turns to small black glass tiles, and your skeleton is laid in fine tesserae along every bone, in camera colours or the station mosaic palette (green arms, maroon spine, blue legs, gold shoulders and hips), each tile growing in as you step into it. Your mouse, or your hand in ShaderClaw, lifts and tilts the tiles under it like a magnet; press or pinch to spin them over. Audio: bass lifts the tiles, beats flash the grout grid, mids speed up the tile flips and the motif morph, highs make the glaze glint. Show dynamics: at Rest the wall is still with soft glints; the Build makes the grout light run faster and the tiles rattle on their beds; the Drop spins every tile in a shockwave ring from the centre with a flash; the Peak lifts and glints tiles with the music.",
  "CREDIT": "Easel / ShaderClaw house shader — Shades of New York series, 2026-11-11 — original.",
  "CATEGORIES": ["Generator", "Shades of New York", "Texture", "Mosaic", "Interactive", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "sequence",    "LABEL": "Wall", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2, 3, 4], "LABELS": ["Auto Story (all four)", "White Subway Squares", "Black Glass Mosaic", "Greek Key Band", "Ornament Panels"], "GROUP": "Wall" },
    { "NAME": "changeBars",  "LABEL": "Change Wall Every", "TYPE": "long", "DEFAULT": 2, "VALUES": [0, 1, 2, 3], "LABELS": ["2 bars", "4 bars", "8 bars", "16 bars"], "GROUP": "Transitions" },
    { "NAME": "transBeats",  "LABEL": "Transition Length (beats)", "TYPE": "float", "AUDIO": false, "MIN": 2.0, "MAX": 32.0, "DEFAULT": 8.0, "GROUP": "Transitions" },
    { "NAME": "beatGrid",    "LABEL": "Tiles Land On", "TYPE": "long", "DEFAULT": 1, "VALUES": [0, 1, 2], "LABELS": ["Every beat", "Every 1/8", "Every 1/16"], "GROUP": "Transitions" },
    { "NAME": "choreo",      "LABEL": "Choreography", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2, 3, 4, 5], "LABELS": ["Auto (each wall its own move)", "Subdivide (rings)", "Lay Rows (from centre)", "Trace & Fill", "Split-Flap Sweep", "Shock Ring"], "GROUP": "Transitions" },
    { "NAME": "dropSlam",    "LABEL": "Drop Forces Change", "TYPE": "bool", "DEFAULT": true, "GROUP": "Transitions" },
    { "NAME": "tempo",       "LABEL": "Tempo Without Music (BPM)", "TYPE": "float", "AUDIO": false, "MIN": 60.0, "MAX": 180.0, "DEFAULT": 120.0, "GROUP": "Transitions" },
    { "NAME": "zoom",        "LABEL": "Tiles On Screen",  "TYPE": "float", "AUDIO": false, "MIN": 2.0, "MAX": 14.0, "DEFAULT": 5.5, "GROUP": "Wall" },
    { "NAME": "drift",       "LABEL": "Drift",            "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.2, "GROUP": "Wall" },
    { "NAME": "shapeMode",   "LABEL": "Ornament", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2, 3, 4, 5, 6], "LABELS": ["Morph All", "Diamond", "Circle", "Star", "Octagon", "Flower", "Cross"], "GROUP": "Ornament" },
    { "NAME": "morphSpeed",  "LABEL": "Morph Speed",      "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.6, "GROUP": "Ornament" },
    { "NAME": "fillColor",   "LABEL": "Motif Fill",       "TYPE": "color", "DEFAULT": [0.14, 0.36, 0.52, 1.0], "GROUP": "Ornament" },
    { "NAME": "gridGlow",    "LABEL": "Grout Light",      "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.7, "GROUP": "Grid" },
    { "NAME": "rainbowGlaze", "LABEL": "Iridescent Glaze", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.3, "GROUP": "Glaze" },
    { "NAME": "glowColor",   "LABEL": "Grout Light Colour", "TYPE": "color", "DEFAULT": [1.0, 0.82, 0.45, 1.0], "GROUP": "Grid" },
    { "NAME": "liftAmt",     "LABEL": "Tile Lift",        "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.6, "GROUP": "Grid" },
    { "NAME": "tiltAmt",     "LABEL": "Tile Tilt / Glints", "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.5, "GROUP": "Grid" },
    { "NAME": "groutW",      "LABEL": "Grout Width",      "TYPE": "float", "MIN": 0.3, "MAX": 3.0, "DEFAULT": 1.0, "GROUP": "Grid" },
    { "NAME": "gloss",       "LABEL": "Glaze Gloss",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.85, "GROUP": "Glaze" },
    { "NAME": "waviness",    "LABEL": "Handmade Waviness","TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.7, "GROUP": "Glaze" },
    { "NAME": "pillow",      "LABEL": "Pillowed Edges",   "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.8, "GROUP": "Glaze" },
    { "NAME": "wear",        "LABEL": "Chips & Grime",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.45, "GROUP": "Glaze" },
    { "NAME": "tubes",       "LABEL": "Fluorescent Tubes","TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.9, "GROUP": "Light" },
    { "NAME": "trainEvery",  "LABEL": "Train Light Every (s, 0 = off)", "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 90.0, "DEFAULT": 18.0, "GROUP": "Light" },
    { "NAME": "trainColor",  "LABEL": "Train Light",      "TYPE": "color", "DEFAULT": [0.75, 0.85, 1.0, 1.0], "GROUP": "Light" },
    { "NAME": "interact",    "LABEL": "Hand", "TYPE": "long", "DEFAULT": 1, "VALUES": [0, 1], "LABELS": ["Off", "Magnet (press / pinch spins)"], "GROUP": "Hand" },
    { "NAME": "handSize",    "LABEL": "Hand Size",        "TYPE": "float", "AUDIO": false, "MIN": 0.3, "MAX": 3.0, "DEFAULT": 1.0, "GROUP": "Hand" },
    { "NAME": "bodyMode",    "LABEL": "Body Tracking", "TYPE": "long", "DEFAULT": 3, "VALUES": [0, 1, 2, 3], "LABELS": ["Off", "Silhouette (glass inside you)", "Skeleton (mosaic bones)", "Both"], "GROUP": "Body" },
    { "NAME": "bodyMirror",  "LABEL": "Mirror (selfie view)", "TYPE": "bool", "DEFAULT": true, "GROUP": "Body" },
    { "NAME": "boneWidth",   "LABEL": "Bone Width",       "TYPE": "float", "MIN": 0.2, "MAX": 3.0, "DEFAULT": 1.0, "GROUP": "Body" },
    { "NAME": "boneColours", "LABEL": "Bone Tiles", "TYPE": "long", "DEFAULT": 1, "VALUES": [0, 1], "LABELS": ["Camera colours", "Station mosaic colours"], "GROUP": "Body" },
    { "NAME": "camTex",      "LABEL": "Image / Camera",   "TYPE": "image", "GROUP": "Camera" },
    { "NAME": "camMix",      "LABEL": "Camera Mosaic",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "GROUP": "Camera" },
    { "NAME": "paletteMode", "LABEL": "Studio Palette", "TYPE": "long", "DEFAULT": 4, "VALUES": [0, 1, 2, 3, 4], "LABELS": ["Colorful", "Red · Black · White", "Blue · Black · White", "Black & White", "NYC Reference"], "GROUP": "Studio Palette" },
    { "NAME": "palAmount",   "LABEL": "Palette Amount",   "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 1.0, "GROUP": "Studio Palette" },
    { "NAME": "palRed",      "LABEL": "Red Accent",       "TYPE": "color", "DEFAULT": [0.92, 0.04, 0.07, 1.0], "GROUP": "Studio Palette" },
    { "NAME": "palBlue",     "LABEL": "Blue Accent",      "TYPE": "color", "DEFAULT": [0.07, 0.32, 1.0, 1.0],  "GROUP": "Studio Palette" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.65, "GROUP": "Audio Reactivity" },
    { "NAME": "showState",   "LABEL": "Show State", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2, 3], "LABELS": ["Follow Music", "Rest", "Build", "Peak"], "GROUP": "Show Dynamics" },
    { "NAME": "dynamics",    "LABEL": "Build & Drop Depth", "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.85, "GROUP": "Show Dynamics" },
    { "NAME": "bgMode",      "LABEL": "Behind Flipping Tiles", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2], "LABELS": ["Solid", "Vertical Gradient", "Radial Gradient"], "GROUP": "Background" },
    { "NAME": "bgColor",     "LABEL": "BG Color / Top / Center", "TYPE": "color", "DEFAULT": [0.05, 0.045, 0.04, 1.0], "GROUP": "Background" },
    { "NAME": "bgColor2",    "LABEL": "BG Bottom / Edge", "TYPE": "color", "DEFAULT": [0.0, 0.0, 0.0, 1.0], "GROUP": "Background" },
    { "NAME": "transparentBg","LABEL": "Transparent BG (tiles only)", "TYPE": "bool", "DEFAULT": false, "GROUP": "Background" }
  ],
  "PASSES": [
    { "TARGET": "stateBuf", "PERSISTENT": true, "FLOAT": true },
    {}
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// Station Tiles — Shades of New York 003
//   One pass. A "layout" maps a wall point to the tile it lies in: the tile's
//   centre, half size, corner radius, grout width, local coordinates and
//   base colour (from the design: frieze, panel, motif). Transitions flip the
//   NEXT layout's tiles one by one; a flipping tile shows the old wall until
//   it turns edge-on, then the new one. evalLayout() has ONE call site, run
//   at most twice per pixel (find the flipping tile, then look through it).
// ─────────────────────────────────────────────────────────────────────────

#define PI  3.14159265359
#define TAU 6.28318530718

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


vec2 h22(vec2 p) { vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031, 0.1030, 0.0973)); p3 += dot(p3, p3.yzx + 33.33); return fract((p3.xx + p3.yz) * p3.zy); }
float vnoise(vec2 x) {
    vec2 i = floor(x), f = fract(x); f = f * f * (3.0 - 2.0 * f);
    return mix(mix(h12(i), h12(i + vec2(1, 0)), f.x), mix(h12(i + vec2(0, 1)), h12(i + vec2(1, 1)), f.x), f.y);
}
mat2 rot(float a) { float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }

// station palette, from the four reference photos (a touch richer)
const vec3 C_WHITE  = vec3(0.93, 0.92, 0.88);
const vec3 C_CREAM  = vec3(0.95, 0.89, 0.74);
const vec3 C_YELLOW = vec3(0.96, 0.84, 0.36);
const vec3 C_TAN    = vec3(0.85, 0.63, 0.36);
const vec3 C_MAROON = vec3(0.44, 0.12, 0.09);
const vec3 C_GREEN  = vec3(0.07, 0.26, 0.21);
const vec3 C_BLACK  = vec3(0.022, 0.022, 0.027);

// ── layout output (one tile) ────────────────────────────────────────────
vec3  tCol;      // base colour
float tMat;      // 0 glossy subway glaze, 1 black hand glaze, 2 matte mosaic
vec2  tC;        // tile centre (wall units)
vec2  tB;        // tile half size including half the grout
vec2  tLc;       // pixel relative to the tile centre, in the tile's frame
float tR;        // corner radius
float tGw;       // grout half width
float tId;       // per-tile hash
float tEdge;     // extra grout: distance to a design boundary that cuts tiles (large = none)
float tReg;      // choreography info: region (0 main, 1 white surround / frame, 2 contour, 3 fill, 4 corners, 5 field)
float tAng;      // 0..1 position around a traced outline (clockwise from the top)
float tRad;      // 0..1 distance from a motif / panel centre
float tRow;      // row index for laid rows
vec3  tGrout;    // grout colour

void squareCell(vec2 p, float s, float jit, float seed) {
    vec2 id = floor(p / s);
    vec2 c = (id + 0.5) * s;
    vec2 j = (h22(id + seed) - 0.5) * jit * s;
    float a = (h12(id + seed + 9.0) - 0.5) * jit * 0.6;
    tC = c + j;
    tLc = rot(a) * (p - tC);
    tB = vec2(0.5 * s) * (1.0 - jit * 0.35 * h12(id + seed + 4.0));
    tId = h12(id * 1.37 + seed);
}
// brick-coursed tesserae; vertical = 1 lays them upright (rows become columns)
void brickCell(vec2 p, float th, float tw, float vertical, float seed, float aligned) {
    vec2 q = vertical > 0.5 ? p.yx : p;
    float row = floor(q.y / th);
    float off = aligned > 0.5 ? 0.0 : h11(row * 7.13 + seed) * tw;
    float col = floor((q.x - off) / tw);
    vec2 id = vec2(col, row);
    float hh = h12(id + seed * 3.1);
    vec2 c = vec2((col + 0.5) * tw + off, (row + 0.5) * th);
    float wsh = aligned > 0.5 ? 1.0 : 0.82 + 0.18 * h12(id + seed + 2.0);    // tesserae differ in width
    c.x += (hh - 0.5) * tw * (1.0 - wsh) * 0.9;
    vec2 lc = rot((h12(id + seed + 7.0) - 0.5) * 0.06) * (q - c);
    vec2 b = vec2(0.5 * tw * wsh, 0.5 * th * (0.94 + 0.06 * h12(id + seed + 5.0)));
    if (vertical > 0.5) { c = c.yx; lc = lc.yx; b = b.yx; }
    tC = c; tLc = lc; tB = b; tId = hh;
}

// ── ornament motif: signed distance, morphing through six shapes ────────
float gShapeA, gShapeB, gShapeF;
float shapeSD(vec2 P, float k) {
    float r = length(P);
    float a = atan(P.y, P.x);
    vec2 A = abs(P);
    if (k < 0.5) return (A.x / 0.62 + A.y / 0.95 - 1.0) * 0.5;                            // diamond (tall)
    if (k < 1.5) return r - 0.62;                                                         // circle
    if (k < 2.5) return r - 0.66 * (0.72 + 0.28 * cos(5.0 * a + PI * 0.5)) - 0.04;        // star
    if (k < 3.5) return max(max(A.x, A.y), (A.x + A.y) * 0.7071) - 0.56;                  // octagon
    if (k < 4.5) return r - 0.58 * (0.78 + 0.22 * abs(cos(3.0 * a))) - 0.05;              // flower
    vec2 bx = A - vec2(0.62, 0.2), by = A - vec2(0.2, 0.62);                              // cross
    return min(length(max(bx, 0.0)) + min(max(bx.x, bx.y), 0.0), length(max(by, 0.0)) + min(max(by.x, by.y), 0.0));
}
float motifSD(vec2 P) { return mix(shapeSD(P, gShapeA), shapeSD(P, gShapeB), gShapeF); }

// ── Greek key: design grid, 14 columns × 8 rows per period ──────────────
// 0 = background (yellow/tan), 1 = green stroke, 2 = white frame, 3 = maroon
float keyCell(float ix, float iy) {
    ix = mod(ix, 14.0);
    if (iy < 0.5 || iy > 6.5) return 1.0;                             // top + bottom borders
    if (ix < 0.5 || (ix > 7.5 && ix < 8.5)) return 1.0;               // the uprights
    if (ix < 7.5) {                                                   // the hook
        if (iy > 4.5 && iy < 5.5 && ix < 6.5) return 1.0;
        if (ix > 5.5 && ix < 6.5 && iy > 1.5 && iy < 5.5) return 1.0;
        if (iy > 1.5 && iy < 2.5 && ix > 2.5 && ix < 6.5) return 1.0;
        if (ix > 2.5 && ix < 3.5 && iy > 1.5 && iy < 3.5) return 1.0;
        return 0.0;
    }
    if (ix > 9.5 && ix < 12.5 && iy > 1.5 && iy < 5.5) return 3.0;    // maroon heart
    if (ix > 8.5) return 2.0;                                         // white frame
    return 0.0;
}

vec3 mixYT(float h) { return h < 0.5 ? mix(C_YELLOW, C_CREAM, 0.25 * h) : mix(C_TAN, C_YELLOW, 0.4 * (h - 0.5)); }

// ── body: the tile size follows the person ──────────────────────────────
vec2 gCam; float gSc, gBody, gAspT;
vec2 wallToUV(vec2 w) { return ((w - gCam) / gSc) * vec2(1.0 / gAspT, 1.0) + 0.5; }
vec2 wallToP(vec2 w) { return (w - gCam) / gSc; }
// returns true if the body claimed this point (tile outputs set)
bool bodyTiles(vec2 p) {
    if (gBody < 0.5) return false;
    // level 1: inside the silhouette → 0.5 glass squares
    vec2 c1 = (floor(p / 0.5) + 0.5) * 0.5;
    float m1 = bodyUseSil() > 0.5 ? bodySeg(wallToUV(c1)) : 0.0;
    float bw = 0.035 * boneWidth;
    float b1 = bodyUseSkel() > 0.5 ? boneDist(wallToP(c1), gAspT) : 9.0;
    if (m1 < 0.5 && b1 > bw + 0.09 / gSc * 1.0) {
        // even outside the coarse cell centre, bones still claim their fine tiles
        if (bodyUseSkel() < 0.5) return false;
    }
    // level 2: along the bones → 0.25 tesserae
    vec2 c2 = (floor(p / 0.25) + 0.5) * 0.25;
    float b2 = bodyUseSkel() > 0.5 ? boneDist(wallToP(c2), gAspT) : 9.0;
    float boneId = gBoneId;
    if (b2 < bw) {
        squareCell(p, 0.25, 0.08, 71.0);
        float grow = smoothstep(bw, bw * 0.4, b2);
        tB *= 0.55 + 0.45 * grow;
        vec3 cc;
        if (float(boneColours) < 0.5) cc = texture2D(camTex, clamp(bodyCamUV(wallToUV(tC)), 0.0, 1.0)).rgb * 1.15;
        else {
            cc = boneId < 1.5 ? C_MAROON : boneId < 3.5 ? C_YELLOW : boneId < 7.5 ? C_GREEN * 1.6 : fillColor.rgb * 1.2;
            cc *= 0.85 + 0.3 * tId;
        }
        tCol = cc;
        tMat = 2.0; tR = 0.012; tGw = 0.009 * groutW;
        tGrout = vec3(0.4, 0.37, 0.33);
        tReg = 0.0; tRow = 0.0;
        return true;
    }
    if (m1 > 0.5) {
        squareCell(p, 0.5, 0.06, 3.0);
        float m2 = bodySeg(wallToUV(tC));
        tB *= 0.6 + 0.4 * smoothstep(0.3, 0.8, m2);                     // tiles grow in as you step into them
        tCol = C_BLACK * (0.8 + 0.5 * tId) + vec3(0.0, 0.004, 0.012) * step(0.8, tId);
        if (camMix > 0.001) tCol = mix(tCol, texture2D(camTex, clamp(bodyCamUV(wallToUV(tC)), 0.0, 1.0)).rgb * 0.5, camMix);
        tMat = 1.0; tR = 0.03; tGw = 0.018 * groutW;
        tGrout = vec3(0.86, 0.8, 0.68);
        return true;
    }
    return false;
}

// ── layouts ─────────────────────────────────────────────────────────────
void evalLayout(float L, vec2 p) {
    tEdge = 9.0; tReg = 0.0; tAng = 0.0; tRad = 0.0; tRow = 0.0;
    if (bodyTiles(p)) return;
    tMat = 2.0; tR = 0.02; tGw = 0.011 * groutW;
    tGrout = vec3(0.47, 0.43, 0.39);
    if (L < 0.5) {                                                    // White Subway Squares
        squareCell(p, 1.0, 0.012, 1.0);
        float v = tId;
        tCol = mix(vec3(0.97, 0.965, 0.95), vec3(0.94, 0.95, 0.96), v);
        tMat = 0.0; tR = 0.045; tGw = 0.012 * groutW;
        tGrout = vec3(0.4, 0.39, 0.37);
    } else if (L < 1.5) {                                             // Black Glass Mosaic
        squareCell(p, 0.5, 0.06, 3.0);
        tCol = C_BLACK * (0.8 + 0.5 * tId) + vec3(0.0, 0.004, 0.012) * step(0.8, tId);
        tMat = 1.0; tR = 0.03; tGw = 0.018 * groutW;
        tGrout = vec3(0.86, 0.8, 0.68);
    } else if (L < 2.5) {                                             // Greek Key Band
        float th = 0.15;
        float rowF = floor(p.y / th);
        float band = 10.0;                                            // band rows: -10 .. 9
        if (rowF < -band || rowF > band - 1.0) {                      // the white wall around it
            squareCell(p + vec2(0.0, 0.5), 1.0, 0.012, 1.0);
            tC -= vec2(0.0, 0.5);
            tCol = mix(C_WHITE, vec3(0.9, 0.92, 0.93), tId);
            tMat = 0.0; tR = 0.07; tGw = 0.012 * groutW;
            tGrout = vec3(0.32, 0.31, 0.29);
            tEdge = abs(p.y - (rowF < 0.0 ? -band : band) * th);
            tReg = 1.0; tRow = 10.0 + floor((abs(tC.y) - band * th) / 1.0) * 2.0;
        } else {
            float r = rowF + band;                                    // 0 .. 19 (bottom → top)
            tRow = abs(r - 9.5) - 0.5;                                // 0 at the centre line → 9 at the edges
            bool key = r > 5.5 && r < 13.5;
            brickCell(p, th, key ? 0.2 : 0.21, 0.0, 11.0 + (key ? 0.0 : r), key ? 1.0 : 0.0);
            float h = tId;
            vec3 c;
            if (r < 0.5 || r > 18.5) c = C_WHITE;
            else if (r > 2.5 && r < 3.5 || r > 15.5 && r < 16.5) c = C_MAROON * (0.9 + 0.2 * h);
            else if (!key) c = mixYT(h);
            else {
                float k = keyCell(floor(tC.x / 0.2 + 3.0), r - 6.0);
                c = k < 0.5 ? mixYT(h) : k < 1.5 ? C_GREEN * (0.85 + 0.3 * h) : k < 2.5 ? C_WHITE : C_MAROON * (0.9 + 0.2 * h);
            }
            tCol = c;
            tEdge = min(abs(p.y + band * th), abs(p.y - band * th));
        }
    } else {                                                          // Ornament Panels
        float Pw = 2.5, Ph = 3.3;
        vec2 pid = vec2(floor(p.x / Pw + 0.5), 0.0);
        vec2 P = p - pid * Pw;
        P.y = mod(P.y + Ph * 0.5, Ph + 0.6) - Ph * 0.5;               // panels stack too
        vec2 A = abs(P);
        float fx = 0.92, fy = 1.38, fw = 0.12;                        // inner frame (half size) and band width
        float outer = max(A.x - fx, A.y - fy);
        if (outer > 0.0) {                                            // maroon field of upright tesserae
            brickCell(p, 0.115, 0.27, 1.0, 21.0, 0.0);
            tCol = C_MAROON * (0.82 + 0.3 * tId) + vec3(0.04, 0.0, 0.0) * step(0.7, tId);
            tEdge = abs(outer);
            tReg = 5.0; tRad = clamp(outer / 0.7, 0.0, 1.0);
        } else if (outer > -fw) {                                     // white frame: square tesserae along it
            vec2 Q = P;
            bool vert = A.x - fx > A.y - fy;
            float sgn = vert ? sign(P.x) : sign(P.y);
            vec2 along = vert ? vec2(P.y, (A.x - fx + fw * 0.5)) : vec2(P.x, (A.y - fy + fw * 0.5));
            squareCell(vec2(along.x, along.y) + 0.5 * fw, fw, 0.05, 31.0 + sgn);
            tC = vert ? vec2(sgn * (fx - fw * 0.5), tC.x - 0.5 * fw) + pid * Pw : vec2(tC.x - 0.5 * fw, sgn * (fy - fw * 0.5)) + pid * Pw;
            tCol = C_WHITE * (0.95 + 0.05 * tId);
            tEdge = min(abs(outer), abs(outer + fw));
            tReg = 1.0; tAng = fract(atan(P.x, P.y) / TAU + 1.0);
        } else {
            vec2 M = P / 1.3;                                         // motif space
            float s = motifSD(M) * 1.3;
            float bw = 0.12;
            if (abs(s) < bw * 0.5) {                                  // white contour band, tiles follow the outline
                float ang = atan(P.y, P.x);
                float n = 34.0;
                float u = (ang / TAU + 0.5) * n;
                vec2 cid = vec2(floor(u), 0.0);
                float uc = (cid.x + 0.5) / n * TAU - PI;
                float rr = length(P);
                vec2 dirc = vec2(cos(uc), sin(uc));
                tC = pid * Pw + dirc * rr;                            // approx centre on the band
                vec2 tang = vec2(-dirc.y, dirc.x);
                float arcW = TAU * max(rr, 0.2) / n;
                tLc = vec2((fract(u) - 0.5) * arcW, s);
                tB = vec2(0.5 * arcW, 0.5 * bw);
                tId = h12(cid + pid * 7.0 + 41.0);
                tCol = C_WHITE * (0.94 + 0.06 * tId);
                tEdge = 9.0;
                tReg = 2.0; tAng = fract(atan(dirc.x, dirc.y) / TAU + 1.0);
            } else if (s < 0.0) {                                     // the motif fill, brick coursed
                brickCell(p, 0.13, 0.19, 0.0, 51.0, 0.0);
                vec3 fc = fillColor.rgb;
                tCol = fc * (0.8 + 0.35 * tId) + vec3(0.0, 0.03, 0.03) * step(0.6, h12(vec2(tId, 3.0)));
                tEdge = abs(s + bw * 0.5);
                tReg = 3.0; tRad = clamp(length(tC - pid * Pw) / 1.0, 0.0, 1.0);
            } else {                                                  // yellow / tan corners
                brickCell(p, 0.13, 0.2, 0.0, 61.0, 0.0);
                tCol = mixYT(tId);
                tEdge = min(abs(s - bw * 0.5), abs(outer + fw));
                tReg = 4.0; tRad = clamp(length(P) / 1.6, 0.0, 1.0);
            }
        }
    }
}

// ── grime / chips on a tile ─────────────────────────────────────────────
float chipMask(vec2 lc, vec2 b, float id) {
    float hit = step(h11(id * 91.3), wear * 0.55);
    vec2 cp = (h22(vec2(id * 17.0, 3.0)) * 2.0 - 1.0) * b * 0.9;
    float rr = (0.12 + 0.25 * h11(id * 7.7)) * min(b.x, b.y);
    float n = vnoise(lc * 60.0 + id * 9.0);
    return hit * smoothstep(rr, rr * 0.4, length(lc - cp) * (0.7 + 0.6 * n));
}

// ── the show clock + wall state (pass 0, three texels) ──────────────────
//   texel 0 = (beat count whole mod 1024, beat fraction, current wall, next wall)
//   texel 1 = (transition start whole (-1 idle), start fraction, length in beats, choreography)
//   texel 2 = (previous drop level, -, -, valid)
vec4 stateAt(float i) { return texture2D(stateBuf, vec2(i + 0.5, 0.5) / RENDERSIZE.xy); }
float beatsBetween(float aW, float aF, float bW, float bF) { return mod(bW - aW + 512.0, 1024.0) - 512.0 + (bF - aF); }
float barsVal() { float c = float(changeBars); return c < 0.5 ? 2.0 : c < 1.5 ? 4.0 : c < 2.5 ? 8.0 : 16.0; }
// each destination wall has its own move (unless one is forced)
float choreoFor(float target) {
    float c = float(choreo);
    if (c > 0.5) return c;
    return target < 0.5 ? 4.0 : target < 1.5 ? 1.0 : target < 2.5 ? 2.0 : 3.0;
}
vec4 statePass(vec2 fc) {
    if (fc.y > 1.0 || fc.x > 3.0) return vec4(0.0);
    vec4 S0 = stateAt(0.0), S1 = stateAt(1.0), S2 = stateAt(2.0);
    float seqW = float(sequence);
    float pick = seqW > 0.5 ? seqW - 1.0 : 0.0;
    if (FRAMEINDEX < 2 || S2.w < 0.5) { S0 = vec4(0.0, 0.0, pick, pick); S1 = vec4(-1.0, 0.0, 8.0, 1.0); S2 = vec4(0.0, 0.0, 0.0, 1.0); }
    float dt = clamp(TIMEDELTA, 0.0, 0.1);
    bool conf = audioBPMConfidence > 0.35 && audioBPM > 40.0;
    float bpm = conf ? audioBPM : tempo;
    float Bw = S0.x, Bf = S0.y + dt * bpm / 60.0;
    if (conf) { float err = audioBeatPhase - fract(Bf); err -= floor(err + 0.5); Bf += err * 0.2; }   // phase-lock to the track
    float carry = floor(Bf);
    Bf -= carry;
    Bw = mod(Bw + carry, 1024.0);
    bool crossed = carry > 0.5;
    float cur = S0.z, nxt = S0.w;
    float sW = S1.x, sF = S1.y, len = S1.z, sty = S1.w;
    if (sW >= 0.0 && beatsBetween(sW, sF, Bw, Bf) >= len) { cur = nxt; sW = -1.0; }
    bool dropEdge = dropSlam && gDrop > 0.55 && S2.x <= 0.55;
    if (sW < 0.0) {
        float target = -1.0;
        bool slam = false;
        if (seqW < 0.5) {
            if (crossed && mod(Bw, 4.0 * barsVal()) < 0.5) target = mod(cur + 1.0, 4.0);   // phrase downbeat
            if (dropEdge) { target = mod(cur + 1.0, 4.0); slam = true; }
        } else if (abs(pick - cur) > 0.5 && crossed) target = pick;                         // a manual pick lands on the next beat
        if (target >= 0.0) {
            nxt = target;
            sW = Bw; sF = slam ? Bf : 0.0;
            len = slam ? max(2.0, transBeats * 0.35) : transBeats;
            sty = slam ? 5.0 : choreoFor(target);
        }
    }
    float i = floor(fc.x);
    if (i < 0.5) return vec4(Bw, Bf, cur, nxt);
    if (i < 1.5) return vec4(sW, sF, len, sty);
    return vec4(gDrop, 0.0, 0.0, 1.0);
}

float easeOutBack(float x) { float c1 = 1.9, c3 = c1 + 1.0; float y = x - 1.0; return 1.0 + c3 * y * y * y + c1 * y * y; }

void main() {
    showDynamics();
    vec2 R = RENDERSIZE.xy, fc = gl_FragCoord.xy, uv = fc / R;
    if (PASSINDEX == 0) { gl_FragColor = statePass(fc); return; }
    vec2 p0 = (fc - 0.5 * R) / R.y;
    float ar = audioReact;
    float sc = zoom;
    vec2 cam = vec2(TIME * 0.035 * drift + 0.02 * sin(TIME * 0.17), 0.025 * sin(TIME * 0.11) * drift);
    p0 += (h21(floor(TIME * 30.0)) - 0.5) * 0.004 * smoothstep(0.55, 1.0, gBuild);   // build: the wall rattles
    vec2 p = p0 * sc + cam;
    float pxW = sc / R.y;
    gCam = cam; gSc = sc; gAspT = R.x / R.y;
    gBody = float(bodyMode) > 0.5 ? smoothstep(0.3, 0.7, bodyPresence()) : 0.0;

    // ── read the show state ──
    vec4 S0 = stateAt(0.0), S1 = stateAt(1.0);
    float LA = S0.z, LB = S0.w;
    bool inTrans = S1.x >= 0.0 && FRAMEINDEX > 2;
    float since = inTrans ? beatsBetween(S1.x, S1.y, S0.x, S0.y) : 0.0;
    float len = max(S1.z, 1.0), sty = S1.w;
    float beatF = S0.y;                                               // where we are inside the beat
    float grid = float(beatGrid) < 0.5 ? 1.0 : float(beatGrid) < 1.5 ? 0.5 : 0.25;

    // ornament morph steps on the bar
    float sm = float(shapeMode);
    float mclk = (S0.x + S0.y) / 16.0 * morphSpeed;                   // one shape every 4 bars at speed 1
    if (sm < 0.5) { gShapeA = mod(floor(mclk), 6.0); gShapeB = mod(floor(mclk) + 1.0, 6.0); float fm = fract(mclk); gShapeF = smoothstep(0.75, 1.0, fm); gShapeF = easeOutBack(gShapeF) ; }
    else { gShapeA = sm - 1.0; gShapeB = gShapeA; gShapeF = 0.0; }

    vec2 mW = ((mousePos - 0.5) * vec2(R.x / R.y, 1.0)) * sc + cam;
    float handOn = step(0.5, float(interact));

    // ── find the tile, choreograph its flip, look through it (ONE evalLayout call site) ──
    float Lsel = inTrans ? LB : LA;
    vec2 pe = p;
    float back = 0.0, edgeHit = 0.0, flipShade = 1.0, faceGlint = 0.0, front = 0.0;
    float sbT = 0.0, flipB = 1.0;
    for (int k = 0; k < 2; k++) {
        evalLayout(Lsel, pe);
        if (k == 1) break;
        vec2 cB = tC, bB = tB;
        vec2 cr = (cB - cam) / sc;                                   // tile centre in screen-height units
        float ang = 0.0, axis = 0.0, f = 0.0;
        if (inTrans) {
            // when does this tile turn? (normalised 0..1 across the transition, then put on the beat grid)
            float sn;
            float halfW = 0.5 * R.x / R.y + 0.1;
            if (sty < 1.5) {                                          // Subdivide: rings of the big squares, children clockwise
                vec2 par = floor(cB + 0.5) - floor(cam + 0.5);
                float ring = max(abs(par.x), abs(par.y));
                vec2 ch = step(fract(cB + 0.5), vec2(0.5));
                float order = ch.x < 0.5 ? (ch.y < 0.5 ? 0.0 : 3.0) : (ch.y < 0.5 ? 1.0 : 2.0);
                sn = ring / (0.5 * sc * R.x / R.y + 1.0) + order * 0.012;
                flipB = clamp(len * 0.12, 0.5, 2.0);
            } else if (sty < 2.5) {                                   // Lay Rows: frieze from its centre line, then the white wall
                sn = tReg > 0.5 ? 0.62 + 0.38 * clamp((tRow - 10.0) / 6.0, 0.0, 1.0) : 0.6 * tRow / 9.0;
                axis = 1.0;
                flipB = clamp(len * 0.08, 0.35, 1.0);
            } else if (sty < 3.5) {                                   // Trace & Fill: outline drawn, then filled, then the field
                if (tReg > 1.5 && tReg < 2.5)      sn = 0.02 + 0.3 * tAng;
                else if (tReg > 0.5 && tReg < 1.5) sn = 0.0 + 0.3 * tAng;
                else if (tReg > 2.5 && tReg < 3.5) sn = 0.36 + 0.24 * tRad;
                else if (tReg > 3.5 && tReg < 4.5) sn = 0.58 + 0.12 * tRad;
                else                               sn = 0.72 + 0.28 * tRad;
                flipB = tReg > 0.5 && tReg < 2.5 ? clamp(len * 0.05, 0.25, 0.6) : clamp(len * 0.09, 0.35, 1.2);
            } else if (sty < 4.5) {                                   // Split-Flap: rows top → bottom, flipping over the top edge
                sn = clamp(0.5 - cr.y, 0.0, 1.0);
                axis = 1.0;
                flipB = clamp(len * 0.1, 0.4, 1.0);
            } else {                                                  // Shock Ring (drop)
                sn = clamp(length(cr) / (halfW * 1.1), 0.0, 1.0);
                flipB = clamp(len * 0.25, 0.4, 1.0);
            }
            sbT = sn * max(len - flipB, 0.0);
            sbT = floor(sbT / grid + 0.5) * grid;                     // tiles start on the beat grid
            f = clamp((since - sbT) / flipB, 0.0, 1.0);
            ang = easeOutBack(f) * PI;                                // snaps over, overshoots, settles
            front = exp(-pow((since - sbT - flipB * 0.5) / (flipB * 0.9), 2.0));
        }
        // hand: press / pinch spins the tiles under it
        float dh = length(cB - mW) / (sc * 0.12 * handSize);
        float spinH = handOn * step(0.5, mouseDown) * exp(-dh * dh);
        ang += spinH * TAU * fract(TIME * 0.9);
        float s = cos(ang);
        bool showB = inTrans && f >= 0.5;
        flipShade = 0.5 + 0.5 * abs(s);
        faceGlint = exp(-pow((abs(s) - 0.55) / 0.12, 2.0)) * step(abs(s), 0.98);
        if (abs(s) > 0.999 && (!inTrans || showB || f <= 0.0)) {
            if (inTrans && f <= 0.0) { Lsel = LA; continue; }            // not its turn yet: the old wall
            break;
        }
        // project through the turning tile; its glazed edge shows on the leading side
        vec2 lcl = p - cB;
        float lx = axis < 0.5 ? lcl.x : lcl.y;
        float bx = axis < 0.5 ? bB.x : bB.y;
        float as = max(abs(s), 1e-3);
        float proj = lx / as;
        float thick = bx * 0.16 * abs(sin(ang));
        float side = sin(ang) >= 0.0 ? 1.0 : -1.0;
        if (abs(proj) > bx) {
            float outer = (abs(lx) - bx * as) * side * sign(lx);
            if (outer > 0.0 && outer < thick) { edgeHit = 1.0; }
            back = 1.0;
            break;
        }
        pe = axis < 0.5 ? cB + vec2(proj, lcl.y) : cB + vec2(lcl.x, proj);
        Lsel = showB ? LB : LA;
    }

    // ── shade the tile ──
    vec2 lc = tLc;
    vec2 b = tB - tGw;
    float aa = pxW * 1.2;
    vec2 qd = abs(lc) - (b - tR);
    float d = length(max(qd, 0.0)) + min(max(qd.x, qd.y), 0.0) - tR;   // rounded box SDF
    float edgeCut = tEdge - tGw;                                     // design boundaries cut tiles too
    float inside = smoothstep(aa, -aa, d) * smoothstep(-aa, aa, edgeCut);
    vec2 g = max(qd.x, qd.y) > 0.0 ? normalize(max(qd, 0.0)) * sign(lc) : (qd.x > qd.y ? vec2(sign(lc.x), 0.0) : vec2(0.0, sign(lc.y)));

    // per-tile motion: lift waves, tilt glints, magnet hand
    float id = tId;
    vec2 cw = (tC - cam) / sc;
    float T = TIME * (0.6 + 0.4 * gLift) + audioBassTime * 0.3 * ar;
    float wave = pow(0.5 + 0.5 * sin(dot(cw, vec2(2.3, 1.1)) * 3.0 - T * 2.2), 4.0);
    float lift = liftAmt * (0.25 * wave + 0.6 * bassP() * ar * (0.3 + 0.7 * wave) + 0.5 * audioBeatPulse * ar * step(0.82, h11(id * 31.0 + floor(TIME * 2.0))));
    vec2 tilt = tiltAmt * 0.18 * vec2(sin(T * 1.7 + id * 40.0 + cw.x * 4.0), cos(T * 1.3 + id * 23.0 + cw.y * 4.0));
    tilt *= 0.35 + 0.65 * gLift + 1.5 * gBuild * gBuild;
    float dHand = length(tC - mW) / (sc * 0.16 * handSize);
    float mag = handOn * exp(-dHand * dHand);
    lift += mag * 0.9;
    tilt += mag * normalize(mW - tC + 1e-4) * 0.35;

    // surface normal: pillowed edge + handmade glaze waviness + tilt
    float tsz = min(b.x, b.y);
    float ew = tsz * (tMat < 0.5 ? 0.055 : tMat < 1.5 ? 0.22 : 0.12);
    float e = max(-d, 0.0);
    float slope = pillow * exp(-e / ew) * 0.9 * (tMat > 1.5 ? 0.6 : 1.0);
    vec2 nxy = g * slope + tilt;
    float wv = waviness * (tMat < 0.5 ? 0.04 : tMat < 1.5 ? 0.16 : 0.06);
    vec2 wl = lc / max(tsz, 0.05);
    nxy += wv * vec2(sin(wl.x * 5.1 + id * 30.0) * cos(wl.y * 3.7 + id * 11.0) + 0.5 * sin(wl.y * 9.0 + id * 7.0),
                     cos(wl.y * 4.3 + id * 19.0) * sin(wl.x * 3.1 + id * 5.0) + 0.5 * cos(wl.x * 8.0 + id * 13.0));
    nxy += (tMat > 1.5 ? 0.05 : 0.0) * (vec2(vnoise(lc * 120.0), vnoise(lc * 120.0 + 7.0)) - 0.5);   // matte tesserae: bisque texture
    vec3 N = normalize(vec3(nxy, 1.0));

    // base colour, camera mosaic, chips, grime
    vec3 base = tCol;
    if (camMix > 0.001) {
        vec2 cu = ((tC - cam) / sc) * vec2(R.y / R.x, 1.0) + 0.5;
        vec3 cc = texture2D(camTex, clamp(bodyCamUV(cu), 0.0, 1.0)).rgb;
        base = mix(base, cc * (0.92 + 0.12 * id), camMix);
    }
    float chip = chipMask(lc, b, id) * (tMat > 1.5 ? 1.0 : tMat > 0.5 ? 0.4 : 0.0);
    base = mix(base, vec3(0.78, 0.68, 0.52), chip);                 // exposed bisque
    float grime = wear * 0.12 * smoothstep(0.4, 1.0, vnoise(p * 1.7)) + wear * 0.25 * exp(-e / (tsz * 0.1)) * step(0.5, tMat) * step(tMat, 1.5) * 0.0;
    base *= 1.0 - grime;

    // ── light: soft top light, fluorescent tubes in reflection, train light, hand glints ──
    vec3 V = normalize(vec3(-p0 * 0.35, 1.0));
    vec3 Lk = normalize(vec3(-0.25, 0.65, 0.72));
    float dif = clamp(dot(N, Lk) * 0.55 + 0.55, 0.0, 1.2);
    float topLight = 0.9 + 0.18 * uv.y;
    vec3 Rf = reflect(-V, N);
    float ry = Rf.y / max(Rf.z, 0.25);
    float rx = Rf.x / max(Rf.z, 0.25);
    float tubeA = smoothstep(0.07, 0.0, abs(ry - 0.42 - 0.04 * sin(cam.x * 0.5))) * smoothstep(1.4, 0.3, abs(rx));
    float tubeB = smoothstep(0.05, 0.0, abs(ry - 0.95)) * smoothstep(1.6, 0.2, abs(rx + 0.3)) * 0.7;
    float ceilG = smoothstep(-0.2, 1.2, ry) * 0.25;
    float fres = 0.05 + 0.95 * pow(1.0 - max(dot(N, V), 0.0), 5.0);
    float glossK = gloss * (tMat < 0.5 ? 1.0 : tMat < 1.5 ? 1.15 : 0.22) * (1.0 - 0.8 * chip);
    float sharp = tMat > 1.5 ? 0.3 : 1.0;
    vec3 env = vec3(1.0, 0.98, 0.94) * ((tubeA + tubeB) * tubes * mix(0.35, 1.0, sharp) + ceilG);
    // train light: a bright streak sweeping along the wall
    float passT = trainEvery > 0.5 ? mod(TIME, trainEvery) / 4.0 : 9.0;
    float trainOn = step(passT, 1.0) * smoothstep(0.0, 0.15, passT) * smoothstep(1.0, 0.8, passT);
    float tx = mix(2.2, -2.2, passT) * (R.x / R.y);
    float trainSp = smoothstep(0.35, 0.0, abs(rx - tx)) * smoothstep(-0.6, 0.6, ry) * trainOn;
    env += trainColor.rgb * trainSp * 1.6;
    // highs: glints jump between tiles
    float glint = highP() * ar * step(0.9, h11(id * 51.0 + floor(TIME * 8.0))) * pow(max(dot(N, normalize(Lk + V)), 0.0), 60.0);
    vec3 lit = base * dif * topLight * (0.9 + 0.35 * lift) * (tMat < 0.5 ? 1.16 : 1.0);
    lit = mix(lit, lit * 0.25 + env, clamp(glossK * (0.06 + 0.9 * fres) * (tMat > 0.5 && tMat < 1.5 ? 2.2 : 1.0), 0.0, 1.0) * 0.85);
    lit += env * glossK * 0.18 * sharp + vec3(1.0) * glint * 1.5 * gloss;
    // hand: a soft lamp under the palm
    float hl = mag * 0.25;
    lit += vec3(1.0, 0.96, 0.9) * hl * (base * 0.6 + pow(max(dot(N, normalize(vec3(mW - tC, 0.4))), 0.0), 40.0) * glossK);
    lit *= flipShade;
    lit += vec3(1.0, 0.97, 0.92) * faceGlint * gloss * 0.9;             // the face catches the tubes as it turns

    // ── grout: the grid between the tiles, with light running through it ──
    float gd = max(d, -edgeCut);                                     // >0 in the grout
    vec3 groutC = tGrout * (0.85 + 0.15 * vnoise(p * 9.0)) * (1.0 - 0.3 * wear * vnoise(p * 2.3));
    // shadow of a lifted tile falls into the grout (down-right)
    float sh = smoothstep(0.0, tsz * 0.35 * (0.3 + lift), gd) ;
    groutC *= mix(0.55, 1.0, sh) * (1.0 - 0.35 * lift);
    float runPos = abs(lc.x) > abs(lc.y) * (b.x / max(b.y, 1e-3)) ? p.y : p.x;
    float runner = pow(0.5 + 0.5 * sin(runPos * 2.2 - TIME * (1.5 + 5.0 * gBuild) * (0.6 + 0.4 * gLift) + id * 2.0), 12.0);
    float gGlow = gridGlow * (0.35 * runner + 0.9 * audioBeatPulse * ar + 1.2 * gDrop + 0.5 * gBuild * runner + 2.2 * front);
    vec3 groutLit = groutC * (0.75 + 0.3 * topLight) + glowColor.rgb * gGlow;

    // iridescent glaze: each tile shifts through oil-slick colours with the light and the beat
    vec3 irid = 0.5 + 0.5 * cos(6.2832 * (vec3(0.0, 0.33, 0.67) + id * 0.137 + topLight * 0.6 + TIME * 0.06 + 0.2 * audioBeatPulse * ar));
    lit = mix(lit, lit * (0.55 + 0.9 * irid), rainbowGlaze);
    groutLit += irid * gGlow * rainbowGlaze * 0.6;
    vec3 col = mix(groutLit, lit, inside);
    vec3 bg = backdrop(uv);
    if (back > 0.5) col = bg * 0.7 + glowColor.rgb * gridGlow * 0.35 * front;   // the empty bed behind a turning tile
    if (edgeHit > 0.5) col = vec3(0.86, 0.8, 0.7) * (0.6 + 0.4 * flipShade) + vec3(0.2) * faceGlint;   // glazed edge
    float alpha = transparentBg ? (back > 0.5 ? 0.0 : inside) : 1.0;

    // show lift, drop flash, vignette, grain
    col *= 0.86 + 0.24 * gLift;
    col += vec3(1.0, 0.95, 0.85) * gDrop * gDrop * 0.12;
    vec2 dc = uv - 0.5;
    col *= 1.0 - 0.25 * smoothstep(0.4, 1.0, length(dc * vec2(R.x / R.y, 1.0)));
    col += (h12(fc + fract(TIME * 13.7) * 91.0) - 0.5) * 0.015;
    col += (h12(fc * 1.03 + fract(TIME * 7.1) * 57.0) + h12(fc * 0.97 + 11.0) - 1.0) * (1.5 / 255.0);
    col = studioPalette(clamp(col, 0.0, 1.0));
    gl_FragColor = vec4(clamp(col, 0.0, 1.0), alpha);
}
