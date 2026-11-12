/*{
  "DESCRIPTION": "Tag — Shades of New York 001. A handstyle tag is spray-painted live onto a painted brick wall, stroke by stroke, the way a writer actually hits a wall. Soft spray clouds go up first, puffy scalloped cumulus with flat bottoms, sprayed with a fat cap held off the wall and filled in a spiral. With a camera and body tracking on (MediaPipe pose + segmentation), the tag moves onto you: each writer takes one of your limbs as its baseline (arms across the shoulders, head down the spine into the left leg, the right leg) and writes the handstyle along it, swirl writers scribble loops that fill your silhouette and nothing outside it, and the outline of your body is airbrushed around you; the paint fades so the figure keeps up as you move. With no body in frame, up to three invisible writers lay down the tag in real time — a top row of tall pointed crowns and loops, a second row tucked underneath, and one long swooping bar slashed through the middle — with pen lifts between letters. Auto Variety makes every tag different: letter count, size, position, slant, row layout, number of writers and handstyle mix change each cycle, each writer picks a skinny, standard or fat cap, and every stroke swells and thins with hand pressure along its length. The paint is a real persistent spray layer: a hard white core, a soft overspray halo, stray mist specks, paint pooling wherever the can slows down (the tips of the crowns, the starts of strokes), and drips that run down from those pools under gravity, stuttering brick by brick. Paint sits thinner in the mortar grooves, and the brick is lit as a 3D relief: running-bond courses with recessed mortar, per-brick tint, pores, a glossy painted finish and a slow sun glare drifting across the top of the wall, like the reference. Three lifecycles: Write · Hold · Buff (the tag is finished, held, then a city buff roller paints over it in a slightly mismatched patch and the next tag goes up on top), Layer Up (new tags land at new spots and sizes over the fading old ones until the wall is bombed), and Write Once & Hold. Interactive: with Hand Spray on, your mouse — or your hand in ShaderClaw, pinching to spray — becomes a spray can. Camera friendly: switch the wall to Image / Camera to tag over a live feed, or keep the brick relief lighting on top of the camera. Audio: bass opens the nozzle (fatter lines), mids drive the writing speed, highs throw more overspray, and every beat makes the fresh paint flash and bloom. Show dynamics: at Rest the writers move slowly under soft light; the Build speeds the writing up, the hand starts to shake, more drips run and the sun sweeps; on the Drop the whole tag flashes white-hot, the cans splatter outward and the wall shakes; the Peak keeps the fresh paint glowing. NYC Reference palette (default) keeps the photo's brown brick and blue-white paint; the four Studio palettes are one switch away.",
  "CREDIT": "Easel / ShaderClaw house shader — Shades of New York series, 2026-11-10 — original.",
  "CATEGORIES": ["Generator", "Shades of New York", "Street", "Interactive", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "tagStyle",    "LABEL": "Handstyle", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2, 3, 4], "LABELS": ["Auto Mix", "Crowns", "Loops", "Swoops", "Wild"], "GROUP": "Tag" },
    { "NAME": "lettersN",    "LABEL": "Letters",          "TYPE": "float", "AUDIO": false, "MIN": 3.0, "MAX": 10.0, "DEFAULT": 6.0, "GROUP": "Tag" },
    { "NAME": "writersN",    "LABEL": "Writers (rows + bar)", "TYPE": "float", "AUDIO": false, "MIN": 1.0, "MAX": 3.0, "DEFAULT": 3.0, "GROUP": "Tag" },
    { "NAME": "tagSize",     "LABEL": "Tag Size",         "TYPE": "float", "AUDIO": false, "MIN": 0.4, "MAX": 1.5, "DEFAULT": 1.0, "GROUP": "Tag" },
    { "NAME": "slant",       "LABEL": "Slant",            "TYPE": "float", "MIN": -0.6, "MAX": 0.6, "DEFAULT": 0.12, "GROUP": "Tag" },
    { "NAME": "liftChance",  "LABEL": "Pen Lifts",        "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 0.8, "DEFAULT": 0.3, "GROUP": "Tag" },
    { "NAME": "seedSel",     "LABEL": "Tag Seed",         "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 100.0, "DEFAULT": 7.0, "GROUP": "Tag" },
    { "NAME": "variety",     "LABEL": "Auto Variety",     "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.75, "GROUP": "Variety" },
    { "NAME": "widthRange",  "LABEL": "Thick / Thin",     "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6, "GROUP": "Variety" },
    { "NAME": "capMix",      "LABEL": "Cap Mix (skinny ↔ fat)", "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6, "GROUP": "Variety" },
    { "NAME": "cloudsN",     "LABEL": "Spray Clouds",     "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 4.0, "DEFAULT": 3.0, "GROUP": "Clouds" },
    { "NAME": "cloudSize",   "LABEL": "Cloud Size",       "TYPE": "float", "AUDIO": false, "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Clouds" },
    { "NAME": "cloudSoft",   "LABEL": "Softness (fat cap)","TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.65, "GROUP": "Clouds" },
    { "NAME": "cloudPuffs",  "LABEL": "Puffiness",        "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6, "GROUP": "Clouds" },
    { "NAME": "cloudColor",  "LABEL": "Cloud Paint",      "TYPE": "color", "DEFAULT": [0.80, 0.83, 0.86, 1.0], "GROUP": "Clouds" },
    { "NAME": "cloudOpacity","LABEL": "Cloud Opacity",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7, "GROUP": "Clouds" },
    { "NAME": "cycleMode",   "LABEL": "Lifecycle", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2], "LABELS": ["Write · Hold · Buff", "Layer Up", "Write Once & Hold"], "GROUP": "Motion" },
    { "NAME": "writeSpeed",  "LABEL": "Writing Speed",    "TYPE": "float", "MIN": 0.1, "MAX": 3.0, "DEFAULT": 1.0, "GROUP": "Motion" },
    { "NAME": "tagPeriod",   "LABEL": "Cycle Length (s)", "TYPE": "float", "AUDIO": false, "MIN": 6.0, "MAX": 60.0, "DEFAULT": 16.0, "GROUP": "Motion" },
    { "NAME": "shake",       "LABEL": "Hand Shake",       "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.2, "GROUP": "Motion" },
    { "NAME": "lineWidth",   "LABEL": "Line Width",       "TYPE": "float", "MIN": 0.3, "MAX": 3.0, "DEFAULT": 1.0, "GROUP": "Spray" },
    { "NAME": "overspray",   "LABEL": "Overspray",        "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Spray" },
    { "NAME": "dripAmt",     "LABEL": "Drips",            "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.45, "GROUP": "Spray" },
    { "NAME": "gravity",     "LABEL": "Drip Gravity",     "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5, "GROUP": "Spray" },
    { "NAME": "paintColor",  "LABEL": "Paint",            "TYPE": "color", "DEFAULT": [0.87, 0.93, 0.97, 1.0], "GROUP": "Spray" },
    { "NAME": "glowAmt",     "LABEL": "Fresh Paint Glow", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.7, "GROUP": "Spray" },
    { "NAME": "paintFX",     "LABEL": "Paint Style", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2, 3], "LABELS": ["Classic", "Rainbow Fade", "Chrome Fill", "Blacklight UV"], "GROUP": "Spray" },
    { "NAME": "canMode",     "LABEL": "Hand Spray", "TYPE": "long", "DEFAULT": 1, "VALUES": [0, 1, 2], "LABELS": ["Off", "While Pressed / Pinching", "Always Follow"], "GROUP": "Hand Spray" },
    { "NAME": "canSize",     "LABEL": "Can Nozzle",       "TYPE": "float", "AUDIO": false, "MIN": 0.3, "MAX": 3.0, "DEFAULT": 1.2, "GROUP": "Hand Spray" },
    { "NAME": "resetPaint",  "LABEL": "Clean Wall",       "TYPE": "bool",  "DEFAULT": false, "GROUP": "Hand Spray" },
    { "NAME": "bodyMode",    "LABEL": "Body Tracking", "TYPE": "long", "DEFAULT": 3, "VALUES": [0, 1, 2, 3], "LABELS": ["Off", "Silhouette (swirl fill + outline)", "Skeleton (tags along limbs)", "Both"], "GROUP": "Body" },
    { "NAME": "bodyMirror",  "LABEL": "Mirror (selfie view)", "TYPE": "bool", "DEFAULT": true, "GROUP": "Body" },
    { "NAME": "swirlAmt",    "LABEL": "Swirl Fill",       "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.8, "GROUP": "Body" },
    { "NAME": "swirlSpeed",  "LABEL": "Swirl Speed",      "TYPE": "float", "MIN": 0.2, "MAX": 6.0, "DEFAULT": 3.0, "GROUP": "Body" },
    { "NAME": "swirlSize",   "LABEL": "Swirl Can Size",   "TYPE": "float", "MIN": 0.4, "MAX": 3.0, "DEFAULT": 1.7, "GROUP": "Body" },
    { "NAME": "outlineAmt",  "LABEL": "Outline Spray",    "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.7, "GROUP": "Body" },
    { "NAME": "bodyFade",    "LABEL": "Paint Fade (follows you)", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.25, "GROUP": "Body" },
    { "NAME": "wallSource",  "LABEL": "Wall", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2, 3], "LABELS": ["Painted Brick", "Image / Camera", "Camera + Brick Relief", "Background Colour"], "GROUP": "Wall" },
    { "NAME": "wallTex",     "LABEL": "Image / Camera",   "TYPE": "image", "GROUP": "Wall" },
    { "NAME": "courses",     "LABEL": "Brick Courses",    "TYPE": "float", "AUDIO": false, "MIN": 10.0, "MAX": 70.0, "DEFAULT": 34.0, "GROUP": "Wall" },
    { "NAME": "relief",      "LABEL": "3D Relief",        "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Wall" },
    { "NAME": "wallColor",   "LABEL": "Brick Paint",      "TYPE": "color", "DEFAULT": [0.45, 0.37, 0.35, 1.0], "GROUP": "Wall" },
    { "NAME": "wallShadow",  "LABEL": "Brick Shadow",     "TYPE": "color", "DEFAULT": [0.29, 0.24, 0.235, 1.0], "GROUP": "Wall" },
    { "NAME": "sunGlare",    "LABEL": "Sun Glare",        "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 1.0, "GROUP": "Wall" },
    { "NAME": "lightDrift",  "LABEL": "Light Drift",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Wall" },
    { "NAME": "paletteMode", "LABEL": "Studio Palette", "TYPE": "long", "DEFAULT": 4, "VALUES": [0, 1, 2, 3, 4], "LABELS": ["Colorful", "Red · Black · White", "Blue · Black · White", "Black & White", "NYC Reference"], "GROUP": "Studio Palette" },
    { "NAME": "palAmount",   "LABEL": "Palette Amount",   "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 1.0, "GROUP": "Studio Palette" },
    { "NAME": "palRed",      "LABEL": "Red Accent",       "TYPE": "color", "DEFAULT": [0.92, 0.04, 0.07, 1.0], "GROUP": "Studio Palette" },
    { "NAME": "palBlue",     "LABEL": "Blue Accent",      "TYPE": "color", "DEFAULT": [0.07, 0.32, 1.0, 1.0],  "GROUP": "Studio Palette" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.65, "GROUP": "Audio Reactivity" },
    { "NAME": "showState",   "LABEL": "Show State", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2, 3], "LABELS": ["Follow Music", "Rest", "Build", "Peak"], "GROUP": "Show Dynamics" },
    { "NAME": "dynamics",    "LABEL": "Build & Drop Depth", "TYPE": "float", "AUDIO": false, "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.85, "GROUP": "Show Dynamics" },
    { "NAME": "bgMode",      "LABEL": "Background", "TYPE": "long", "DEFAULT": 0, "VALUES": [0, 1, 2], "LABELS": ["Solid", "Vertical Gradient", "Radial Gradient"], "GROUP": "Background" },
    { "NAME": "bgColor",     "LABEL": "BG Color / Top / Center", "TYPE": "color", "DEFAULT": [0.0, 0.0, 0.0, 1.0], "GROUP": "Background" },
    { "NAME": "bgColor2",    "LABEL": "BG Bottom / Edge", "TYPE": "color", "DEFAULT": [0.05, 0.04, 0.04, 1.0], "GROUP": "Background" },
    { "NAME": "transparentBg","LABEL": "Transparent BG (paint only)", "TYPE": "bool", "DEFAULT": false, "GROUP": "Background" }
  ],
  "PASSES": [
    { "TARGET": "paintBuf", "PERSISTENT": true, "FLOAT": true },
    {}
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// Tag — Shades of New York 001
//   pass 0  paintBuf (persistent): the wall's paint layer
//           r = paint coverage   g = wet paint / drips
//           b = buff patch       a = freshness (glows on beats)
//           Each frame every writer's pen is advanced along its handstyle
//           path in sub-steps and sprayed onto the buffer; drips flow down.
//   pass 1  wall relief + paint lighting + glow + palette
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

float gBody;     // 0..1 a tracked body is driving the writers

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

// ── the writing clock (integrated: music speeds it up without jumps) ────
float gAsp;
// The clock is integrated in the paint buffer's corner texel (r = whole part mod 2048,
// g = fraction — exact even in half-float targets), so tempo changes never jump the pen.
float penRate() { return writeSpeed * (0.5 + 0.55 * midP() * audioReact) * (0.65 + 0.35 * gLift) * (1.0 + 1.6 * gBuild * gBuild); }
float penClockStored() { vec4 s = texture2D(paintBuf, vec2(0.5) / RENDERSIZE.xy); return s.r + s.g; }
// cycle bookkeeping: which tag, and where in its life we are
float cycleId(float c)    { return float(cycleMode) > 1.5 ? 0.0 : floor(c / tagPeriod); }
float cyclePhase(float c) { return float(cycleMode) > 1.5 ? min(c / tagPeriod, 0.8) : fract(c / tagPeriod); }

// tag placement for a cycle: centre (screen-height units) and scale
vec3 tagFrame(float cyc) {
    float v = variety;
    vec2 r = h21(cyc * 3.17 + seedSel * 1.3);
    float s = h11(cyc * 7.7 + seedSel + 2.0);
    if (float(cycleMode) > 0.5 && float(cycleMode) < 1.5)             // Layer Up: every tag lands somewhere new
        return vec3((r - 0.5) * vec2(0.42 * gAsp, 0.36), 0.62 + 0.42 * s);
    // Auto Variety: each new tag shifts a little and changes size
    return vec3((r - 0.5) * vec2(0.12 * gAsp, 0.12) * v, mix(1.0, 0.64 + 0.4 * s, v));
}
// per-cycle letter count, slant and writer count
float cycLetters(float cyc) { return clamp(floor(lettersN + variety * (h11(cyc * 5.3 + seedSel + 1.0) * 4.0 - 2.0) + 0.5), 3.0, 10.0); }
float cycSlant(float cyc)   { return slant + variety * (h11(cyc * 2.9 + seedSel + 4.0) - 0.4) * 0.55; }
float cycWriters(float cyc) {
    float h = h11(cyc * 8.1 + seedSel + 6.0);
    return clamp(floor(writersN - variety * (h < 0.25 ? 2.0 : h < 0.55 ? 1.0 : 0.0) + 0.5), 1.0, 3.0);
}
float cycClouds(float cyc)  { return floor(cloudsN * mix(1.0, 0.35 + 0.9 * h11(cyc * 4.4 + seedSel + 8.0), variety) + 0.5); }
// each writer picks a cap per tag: skinny, standard or fat
float capWidth(float wk, float cyc) {
    float h = h11(wk * 13.7 + cyc * 3.3 + seedSel + 11.0);
    float cap = h < 0.5 * (1.0 - capMix) + 0.1 ? 0.55 : h > 1.0 - 0.45 * capMix ? 2.1 : 1.0;
    return mix(1.0, cap, variety);
}

// per-letter handstyle: (loop ratio k, vertical stretch v, flip s)
//   k < 1 → wave / swoop,  k ≈ 1 → cusp (pointed crown),  k > 1 → loop
vec3 letterParams(float i, float seed, float wk) {
    float a = h11(i * 7.31 + seed * 113.0 + wk * 17.0);
    float b = h11(i * 3.77 + seed * 71.0 + wk * 29.0 + 5.0);
    float c = h11(i * 5.13 + seed * 37.0 + wk * 11.0 + 9.0);
    float st = float(tagStyle);
    // Auto Mix shifts its blend of crowns / loops / swoops from tag to tag
    float t1 = mix(0.45, 0.15 + 0.6 * h11(seed * 41.0 + 1.0), variety), t2 = mix(0.8, t1 + (1.0 - t1) * (0.4 + 0.5 * h11(seed * 29.0 + 3.0)), variety);
    float kind = st < 0.5 ? (a < t1 ? 0.0 : a < t2 ? 1.0 : (a > 0.93 && variety > 0.3 ? 3.0 : 2.0)) : st < 1.5 ? 0.0 : st < 2.5 ? 1.0 : st < 3.5 ? 2.0 : 3.0;
    vec3 P;
    if (kind < 0.5)      P = vec3(1.0 + 0.22 * b, 1.9 + 1.3 * c, 1.0);                       // crown: tall cusp / teardrop
    else if (kind < 1.5) P = vec3(1.45 + 0.55 * b, 0.9 + 0.7 * c, c < 0.5 ? -1.0 : 1.0);      // loop
    else if (kind < 2.5) P = vec3(0.45 + 0.3 * b, 1.3 + 1.4 * c, c < 0.5 ? -1.0 : 1.0);       // swoop
    else                 P = vec3(0.4 + 1.6 * b, 0.8 + 2.4 * c, c < 0.4 ? -1.0 : 1.0);        // wild
    P.y *= mix(1.0, 0.65 + 0.8 * h11(i * 9.1 + seed * 19.0 + wk), variety);                 // taller / squatter letters
    if (wk > 1.5) P = vec3(0.32 + 0.15 * b, 0.55 + 0.3 * c, c < 0.5 ? -1.0 : 1.0);           // the long bar
    return P;
}

// pen of writer wk at clock c → (x, y, down) in screen-height units.  Sets gW, gPool, gPr.
float gW, gPool, gPr, gNl, gWm;
vec3 pen(float wk, float c) {
    float cyc = cycleId(c), ph = cyclePhase(c);
    float seed = h11(cyc * 1.37 + seedSel * 0.731 + 0.17);
    float st = wk < 0.5 ? 0.03 : wk < 1.5 ? 0.11 : 0.36;
    float len = wk < 1.5 ? 0.5 : 0.24;
    float pr = (ph - st) / len;
    gPr = pr;
    vec3 fr = tagFrame(cyc);
    float sc = fr.z * tagSize;
    float nMain = cycLetters(cyc);
    float totalW = min(gAsp * 0.84, 1.5) * sc;
    float W = totalW / nMain;
    float nl = wk < 0.5 ? nMain : wk < 1.5 ? max(nMain - 1.0, 2.0) : 2.4;
    gNl = nl;
    float u = clamp(pr, 0.0, 1.0) * nl;
    float i = floor(u), f = u - i;
    vec3 P = vec3(0.0);
    float tm = smoothstep(0.3, 0.7, f);
    for (int q = 0; q < 2; q++) P += letterParams(i + float(q), seed, wk) * (q == 0 ? 1.0 - tm : tm);
    float th = TAU * u;
    float lw = wk < 1.5 ? W : totalW * 1.05 / nl;                    // the bar's "letters" are long
    float x = u - P.x / TAU * sin(th) + variety * 0.12 * sin(u * 2.3 + seed * 17.0);   // uneven spacing
    float y = P.z * P.x / TAU * cos(th) * P.y;
    y += 0.22 * sin(u * 0.9 + seed * 6.0) + (wk > 1.5 ? 0.25 * sin(u * 1.7 + 1.0) : 0.0);
    vec2 q = vec2(x, y) * lw;
    q.x += cycSlant(cyc) * q.y;
    float x0 = fr.x - 0.5 * totalW + (wk > 0.5 && wk < 1.5 ? 0.5 * W : 0.0) - (wk > 1.5 ? 0.04 * sc : 0.0);
    float y0 = fr.y + ((wk < 0.5 ? 0.115 : wk < 1.5 ? -0.13 : -0.02) + variety * 0.05 * (h11(seed * 7.0 + wk * 3.0) - 0.5)) * sc;
    q += vec2(x0, y0);
    if (gBody > 0.5 && bodyUseSkel() > 0.5) {                          // the tag rides a limb
        vec2 base = limbPt(wk, clamp(x / max(nl, 1.0), 0.0, 1.0), gAsp);
        float lh = clamp(gLimbLen / max(nl, 1.0) * 0.85, 0.018, 0.09);
        q = base + gLimbN * y * lh;
        W = gLimbLen / max(nl, 1.0);
    }
    // pen lifts between letters, and pools at landings / stroke starts
    float lifted = step(h11(i * 2.9 + seed * 53.0 + wk * 7.0), liftChance) * step(i, nl - 1.5);
    float prevLift = step(h11((i - 1.0) * 2.9 + seed * 53.0 + wk * 7.0), liftChance) * step(0.5, i);
    float down = step(0.0, pr) * step(pr, 1.0) * (1.0 - lifted * step(0.86, f));
    float sp = length(vec2(1.0 - P.x * cos(th), P.x * P.y * sin(th))) / (1.0 + 0.6 * P.x * P.y);
    gPool = max(max(prevLift * (1.0 - smoothstep(0.0, 0.05, f)), 1.0 - smoothstep(0.0, 0.03, pr)),
                1.0 - smoothstep(0.04, 0.22, sp));
    gW = W;
    // hand pressure: the line swells and thins along the stroke, thinnest on fast flicks
    float pres = 0.55 * sin(u * 2.1 + seed * 9.0 + wk * 2.0) + 0.35 * sin(u * 5.3 + seed * 3.0 + wk);
    gWm = clamp(1.0 + widthRange * (pres - 0.35 * smoothstep(0.6, 1.4, sp)), 0.35, 2.2) * capWidth(wk, cyc);
    return vec3(q, down);
}

// ── spray clouds: scalloped puffy cumulus, outlined then filled in an inward spiral ──
float gCloudW, gCloudRs;
vec3 swirlPen(float ci, float c) {                                    // loops that fill the silhouette
    vec3 j11 = bodyJoint(11.0), j12 = bodyJoint(12.0), j23 = bodyJoint(23.0), j24 = bodyJoint(24.0);
    vec2 s = vec2(gAsp, 1.0);
    vec2 neck = (0.5 * (j11.xy + j12.xy) - 0.5) * s, pelv = (0.5 * (j23.xy + j24.xy) - 0.5) * s;
    float torso = max(length(neck - pelv), 0.05);
    vec2 cen = ci < 0.5 ? mix(neck, pelv, 0.3) : ci < 1.5 ? mix(neck, pelv, 0.85)
             : ci < 2.5 ? (bodyJoint(25.0).xy - 0.5) * s : (bodyJoint(26.0).xy - 0.5) * s;
    float r = torso * (ci < 1.5 ? 0.42 : 0.3) * (0.8 + 0.4 * h11(ci * 3.1)) * (0.85 + 0.15 * swirlSize);
    float a = c * (3.2 + ci * 0.7) * swirlAmt * swirlSpeed + ci * 2.1;
    cen += torso * 0.22 * vec2(sin(c * 0.9 * swirlSpeed + ci * 1.7), cos(c * 0.7 * swirlSpeed + ci * 2.9));   // roam so the fill covers you fast
    vec2 q = cen + r * vec2(cos(a), sin(a)) + 0.55 * r * vec2(cos(2.3 * a + ci), sin(1.7 * a + 2.0 * ci));
    gPr = 0.5;
    return vec3(q, step(0.05, swirlAmt));
}
vec3 cloudPen(float ci, float c) {
    float cyc = cycleId(c), ph = cyclePhase(c);
    float seed = h11(cyc * 1.37 + seedSel * 0.731 + 0.17);
    float st = 0.005 + 0.07 * ci, len = 0.3;
    float pr = (ph - st) / len;
    vec3 fr = tagFrame(cyc);
    float sc = fr.z * tagSize;
    float totalW = min(gAsp * 0.84, 1.5) * sc;
    vec2 hc = h21(ci * 7.13 + seed * 31.0 + 2.0);
    float hs = h11(ci * 3.7 + seed * 13.0);
    // clouds sit behind and around the tag: spread across it, bias toward the rows
    vec2 cen = fr.xy + vec2((hc.x - 0.5) * totalW * 0.78, (hc.y - 0.5) * 0.34 * sc + 0.02 * sc);
    float Rr = (0.055 + 0.075 * hs) * cloudSize * sc * mix(1.0, 0.7 + 0.6 * h11(ci + cyc * 2.1), variety);
    float ax = 1.1 + 0.4 * h11(ci * 5.9 + seed * 3.0);               // clouds are a little wider than tall
    float u = clamp(pr, 0.0, 1.0);
    float th = u * 4.0 * TAU + hs * 6.0;
    float rs = 1.0 - 0.85 * smoothstep(0.25, 1.0, u);                // one turn round the outline, then spiral in
    float nb = floor(4.0 + 4.0 * h11(ci * 2.3 + seed * 5.0));
    // cumulus: round puffs along the top, cusped between them; the base stays flat
    float bump = cloudPuffs * 0.6 * sin(PI * fract(th * nb / TAU)) * smoothstep(-0.35, 0.45, sin(th));
    float rr = Rr * rs * (1.0 + bump * smoothstep(0.45, 1.0, rs));
    vec2 q = cen + vec2(cos(th) * ax, sin(th)) * rr;
    q.y = max(q.y, cen.y - mix(1.0, 0.5, cloudPuffs) * Rr * rs);      // flat cloud base
    gCloudW = Rr * mix(0.34, 0.6, cloudSoft) * (0.75 + 0.25 * rs);
    gCloudRs = rs;
    gPr = pr;
    float down = step(0.0, pr) * step(pr, 1.0) * step(ci + 0.5, cycClouds(cyc));
    return vec3(q, down);
}

float sdSeg(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-9), 0.0, 1.0);
    return length(pa - ba * h);
}

// spray one nozzle segment onto the buffer state S
// Ink levels share one channel: clouds and mist live in 0..0.5, solid tag paint in 0.5..1,
// so the tag always lands on top of the clouds (max) and both colour separately.
void spray(inout vec4 S, vec2 p, vec2 a, vec2 b, float w, float pool, float dripOn, vec2 fc, float burst, float soft) {
    float d = sdSeg(p, a, b);
    float clip = soft > 1.5 ? bodySeg(fc / RENDERSIZE.xy) : 1.0;       // body swirls only paint inside you
    if (soft > 0.5 && soft < 1.5) {                                  // fat cap held off the wall: airbrushed cloud
        if (d > w * 3.5) return;
        float g = exp(-d * d / (w * w) * 1.4);
        float mottle = 0.78 + 0.22 * h12(floor(fc / 2.0) + floor(TIME * 9.0) * 0.37);
        float spk = step(h12(fc * 1.13 + fract(TIME * 5.7) * 211.0), 0.05 * overspray * exp(-pow(d / (w * 2.2), 2.0)) * (1.0 + burst));
        float lvl = mix(0.34, 0.5, smoothstep(0.75, 1.0, pool));     // the outline pass sprays brightest
        S.r = max(S.r, min(0.5, lvl * g * mottle + 0.4 * spk));
        S.a = max(S.a, g * 0.35);
        return;
    }
    if (d > w * 7.0) return;
    float core = smoothstep(w, w * 0.5, d) * clip;
    float halo = exp(-pow(d / (w * 3.0), 2.0)) * 0.16 * min(overspray, 1.5) * clip;
    float pr = 0.05 * overspray * (1.0 + 1.5 * highP() * audioReact + 2.0 * burst) * exp(-pow(d / (w * 4.5), 2.0));
    float spk = step(h12(fc * 0.913 + fract(TIME * 7.31) * 413.0), pr) * (0.6 + 0.4 * h12(fc + 3.7)) * clip;
    S.r = max(S.r, max(core, halo + spk * 0.7));
    S.a = max(S.a, core * 0.9 + spk * 0.5);
    // pooled paint seeds a drip (tight, so drips stay thin)
    S.g = max(S.g, dripOn * pool * smoothstep(w * 0.75, w * 0.2, d));
}

vec4 stateA(float n) { return texture2D(paintBuf, vec2(n + 0.5, 1.5) / RENDERSIZE.xy); }
vec4 stateB(float n) { return texture2D(paintBuf, vec2(n + 0.5, 2.5) / RENDERSIZE.xy); }

vec4 paintPass() {
    vec2 R = RENDERSIZE.xy, fc = gl_FragCoord.xy, uv = fc / R;
    vec2 p = (fc - 0.5 * R) / R.y;
    vec4 S = texture2D(paintBuf, uv);
    bool fresh = FRAMEINDEX < 2 || resetPaint;
    if (fresh) S = vec4(0.0);
    float dt = clamp(TIMEDELTA, 0.0, 0.1);
    float ar = audioReact;
    float cPrev = fresh ? 0.0 : penClockStored();
    float c = cPrev + dt * penRate();
    if (fc.x < 1.0 && fc.y < 1.0) { float wh = floor(c); return vec4(mod(wh, 2048.0), c - wh, 0.0, 1.0); }
    float burst = audioBeatPulse * ar;
    float wBase = 0.0042 * lineWidth * (1.0 + 0.45 * bassP() * ar) * (0.85 + 0.25 * gLift);

    // ── state texels: the pen samples are computed ONCE per frame here (rows 1-2, x 0..35)
    //    and every wall pixel reads them back next frame, instead of each pixel re-evaluating
    //    36 pens. pen() and cloudPen() have this one call site in the whole shader.
    if (fc.y < 3.0 && fc.y >= 1.0 && fc.x < 36.0) {
        float n = floor(fc.x);
        bool rowB = fc.y >= 2.0;
        if (n > 34.5) {                                               // the hand can
            float cm = float(canMode);
            float on = cm > 1.5 ? 1.0 : (cm > 0.5 ? step(0.5, mouseDown) : 0.0);
            vec2 m1 = (mousePos * R - 0.5 * R) / R.y;
            vec2 dl = mouseDelta * R / R.y;
            if (length(dl) > 0.25) dl = vec2(0.0);
            if (rowB) return vec4(m1 - dl, 1.0 - smoothstep(0.0, 0.004, length(dl)), -1.0);
            return vec4(m1, on, wBase * canSize * 1.3);
        }
        bool isCloud = n > 14.5;
        float gi = isCloud ? floor((n - 15.0) / 5.0) : floor(n / 5.0);
        float j = isCloud ? n - 15.0 - gi * 5.0 : n - gi * 5.0;
        float cj = mix(cPrev, c, j / 4.0);
        float cycB = cycleId(cj);
        vec3 pb;
        float w, pool, dripOn;
        float flag = isCloud ? 1.0 : 0.0;
        bool swirl = isCloud && gBody > 0.5 && bodyUseSil() > 0.5;
        if (swirl) {
            pb = swirlPen(gi, cj);
            w = wBase * (0.8 + 0.5 * h11(gi * 5.1)) * (1.0 + 0.4 * bassP() * ar) * swirlSize;
            pool = 0.0; dripOn = 0.0; flag = 2.0;
        } else if (isCloud) {
            pb = cloudPen(gi, cj);
            pb.z *= step(gi + 0.5, cycClouds(cycB)) * (1.0 - gBody);
            w = gCloudW * (1.0 + 0.25 * bassP() * ar);
            pool = gCloudRs; dripOn = 0.0;                         // clouds: pool slot carries spiral radius
        } else {
            pb = pen(gi, cj);
            pb.z *= step(gi + 0.5, cycWriters(cycB));
            // hand shake grows through the build
            pb.xy += (h21(floor(cj * 40.0) + gi * 13.0) - 0.5) * gW * 0.05 * (shake + 1.2 * gBuild);
            pool = gPool;                                             // slow pen (cusps, tight turns) pools paint
            w = wBase * tagFrame(cycB).z * gWm * (1.0 + 0.55 * pool);
            float letter = floor(clamp(gPr, 0.0, 1.0) * gNl);
            dripOn = step(h11(letter * 3.1 + cycB * 17.0 + gi * 5.0 + seedSel), dripAmt);
        }
        if (rowB) return vec4(pool, dripOn, flag, mod(cycB, 2048.0));
        return vec4(pb.xy, pb.z, w);
    }
    float mode = float(cycleMode);

    // ── lifecycle on the wall ──
    S.a *= exp(-dt * 1.6);                                            // fresh paint cools
    if (gBody > 0.01) {                                               // following a body: paint fades so the figure keeps up
        float k = exp(-dt * mix(0.04, 1.4, bodyFade) * gBody);
        S.r *= k; S.g *= k; S.b *= k;
        // the silhouette is airbrushed around you
        if (bodyUseSil() > 0.5 && outlineAmt > 0.0) {
            vec2 e = 3.0 / R;
            float gx = bodySeg(uv + vec2(e.x, 0.0)) - bodySeg(uv - vec2(e.x, 0.0));
            float gy = bodySeg(uv + vec2(0.0, e.y)) - bodySeg(uv - vec2(0.0, e.y));
            float edge = clamp(length(vec2(gx, gy)) * 1.4, 0.0, 1.0);
            float mottle = 0.7 + 0.3 * h12(floor(fc / 2.0) + floor(TIME * 7.0));
            float spk = step(h12(fc * 1.7 + fract(TIME * 3.1) * 50.0), 0.06 * edge);
            S.r = max(S.r, min(0.48, edge * outlineAmt * 0.45 * mottle + 0.4 * spk) * gBody);
            S.a = max(S.a, edge * 0.3 * audioBeatPulse * ar);
        }
    } else if (mode < 0.5) {
        float ph = cyclePhase(c);
        S.b *= exp(-dt * 0.9 / tagPeriod);                            // old buff patches weather away
        if (ph > 0.82 && ph < 0.95) {                                 // the city buff roller
            float rp = (ph - 0.82) / 0.12;
            vec3 fr = tagFrame(cycleId(c));
            float sc = fr.z * tagSize;
            float half_w = min(gAsp * 0.84, 1.5) * sc * 0.5 + 0.08 + 0.12 * cloudSize * sc * step(0.5, cloudsN);
            vec2 lp = p - fr.xy;
            float rx = mix(-half_w - 0.05, half_w + 0.05, rp) + 0.02 * sin(p.y * 40.0) + 0.012 * h12(vec2(floor(p.y * 90.0), 1.0));
            float hh = (0.36 + 0.08 * step(0.5, cloudsN)) * sc + 0.015 * sin(p.x * 23.0);
            if (lp.x < rx && lp.x > -half_w - 0.02 && abs(lp.y) < hh) { S.r = 0.0; S.g = 0.0; S.a = 0.0; S.b = 1.0; }
        }
    } else if (mode < 1.5) {
        S.r *= exp(-dt * 0.35 / tagPeriod);                           // old tags fade under new ones
        S.g *= exp(-dt * 0.35 / tagPeriod);
    }

    // ── drips: wet paint runs down, a pixel at a time, stuttering per column ──
    float above = texture2D(paintBuf, uv + vec2(0.0, 1.0 / R.y)).g;
    if (fresh) above = 0.0;
    float moveP = gravity * (0.35 + 0.5 * gBuild) * min(R.y / 900.0, 1.6);
    if (above > 0.2 && h12(vec2(fc.x, floor(TIME * 60.0))) < moveP)
        S.g = max(S.g, above - 0.003 / max(gravity, 0.05) * 540.0 / R.y);
    if (fresh) return S;

    // ── spray every segment between consecutive pen samples (writers, clouds, then the can) ──
    vec4 pA = stateA(0.0), pB = stateB(0.0);
    for (int k = 1; k < 36; k++) {
        float fk = float(k);
        vec4 qA = stateA(fk), qB = stateB(fk);
        bool isCan = k == 35;
        bool first = !isCan && (k == 15 || k == 20 || k == 25 || k == 30 || k == 5 || k == 10);
        if (isCan) {
            if (qA.z > 0.5) spray(S, p, qB.xy, qA.xy, qA.w, qB.z, step(0.3, dripAmt), fc, burst, 0.0);
        } else if (!first && qA.z > 0.5 && pA.z > 0.5 && abs(qB.a - pB.a) < 0.5) {
            spray(S, p, pA.xy, qA.xy, qA.w, qB.x, qB.y, fc, burst, qB.z);
        }
        // drop: the can splatters outward around each writer's pen head
        if ((k == 4 || k == 9 || k == 14) && gDrop > 0.05 && qA.z > 0.5) {
            float dd = length(p - qA.xy);
            float ring = exp(-pow((dd - 0.02 - 0.12 * (1.0 - gDrop)) / 0.035, 2.0));
            if (h12(fc * 1.37 + fract(TIME * 3.3) * 97.0) < gDrop * 0.015 * ring) { S.r = max(S.r, 0.9); S.a = 1.0; }
        }
        pA = qA; pB = qB;
    }
    // drop: the whole tag flashes fresh
    S.a = max(S.a, S.r * gDrop);
    return clamp(S, 0.0, 1.0);
}

// ── painted brick wall ──────────────────────────────────────────────────
// height of the brick relief at p (screen-height units): 1 = brick face, 0 = mortar groove
vec3 gBrick;   // (brick id hash, mortar mask, -)
float brickH(vec2 p) {
    float cs = courses;
    vec2 q = p * cs;
    float row = floor(q.y);
    float bx = q.x / 2.9 + 0.5 * mod(row, 2.0);
    vec2 f = vec2(fract(bx) * 2.9, fract(q.y));
    vec2 e = min(f, vec2(2.9, 1.0) - f);
    float edge = min(e.x, e.y);
    float mw = 0.07;
    float h = smoothstep(mw * 0.5, mw + 0.12, edge);
    gBrick = vec3(h12(vec2(floor(bx), row)), 1.0 - smoothstep(mw * 0.5, mw + 0.04, edge), 0.0);
    return h;
}

vec3 wallColour(vec2 p, vec2 uv, vec2 sp, out float mortar) {
    float px = 1.0 / RENDERSIZE.y;
    float e = max(px * 1.2, 0.0008);
    float h0 = 0.0, hx = 0.0, hy = 0.0;
    vec3 br = vec3(0.0);
    for (int q = 0; q < 3; q++) {
        float hq = brickH(p + (q == 1 ? vec2(e, 0.0) : q == 2 ? vec2(0.0, e) : vec2(0.0)));
        if (q == 0) { h0 = hq; br = gBrick; } else if (q == 1) hx = hq; else hy = hq;
    }
    mortar = br.y;
    vec2 g = vec2(hx - h0, hy - h0) / e / courses;                   // slope in brick units
    vec3 n = normalize(vec3(-g * 0.9 * relief, 1.0));
    // the light drifts slowly; build sweeps it
    float la = 0.9 + 0.35 * sin(TIME * 0.05 * lightDrift + 1.0) + 1.2 * gBuild * gBuild;
    vec3 L = normalize(vec3(cos(la) * 0.6, sin(la) * 0.6 + 0.35, 0.7));
    float dif = clamp(dot(n, L) * 0.45 + 0.6, 0.0, 1.2);
    // painted brick: per-brick tint, top-lit gradient, pores
    vec3 base = mix(wallShadow.rgb, wallColor.rgb, smoothstep(-0.55, 0.5, p.y));
    base *= 0.95 + 0.1 * br.x;
    float pore = step(0.988, h12(floor(p * courses * 13.0))) * (1.0 - mortar);
    base *= 1.0 - 0.32 * pore;
    base = mix(base, base * 0.9, mortar * 0.6);
    vec3 col = base * mix(1.0, dif, relief);
    // glossy paint catches the sun: big soft glare on the upper right, sharpened on brick faces
    vec2 gp = vec2(0.3 * gAsp + 0.12 * sin(TIME * 0.04 * lightDrift), 0.34 + 0.06 * cos(TIME * 0.03 * lightDrift));
    gp.x -= 0.5 * gBuild * gBuild * gAsp;
    float glare = exp(-dot((p - gp) * vec2(0.5, 1.0), (p - gp) * vec2(0.5, 1.0)) * 1.6);
    vec3 H = normalize(L + vec3(0.0, 0.0, 1.0));
    float spec = pow(max(dot(n, H), 0.0), 18.0);
    vec3 glareC = vec3(0.74, 0.78, 0.80);
    col = mix(col, glareC * (0.8 + 0.25 * spec), clamp(glare * sunGlare * (0.75 - 0.35 * mortar) * (0.75 + 0.35 * gLift), 0.0, 0.9));
    col += glareC * spec * 0.06 * relief * (0.4 + glare);
    return col;
}

vec4 paintAt(vec2 q) { return texture2D(paintBuf, max(q, 4.5 / RENDERSIZE.xy)); }
vec4 finalPass() {
    vec2 R = RENDERSIZE.xy, fc = gl_FragCoord.xy, uv = fc / R;
    float ar = audioReact;
    // drop shake
    vec2 shk = (h21(floor(TIME * 30.0)) - 0.5) * 0.012 * gDrop * gDrop;
    vec2 p = (fc - 0.5 * R) / R.y + shk;
    vec2 suv = uv + shk * vec2(R.y / R.x, 1.0);
    float ws = float(wallSource);

    // wall
    float mortar = 0.0;
    vec3 wall;
    vec3 brick = wallColour(p, suv, p, mortar);
    if (ws < 0.5) wall = brick;
    else if (ws < 2.5) {
        vec3 cam = texture2D(wallTex, bodyCamUV(suv)).rgb;
        if (ws < 1.5) { wall = cam; mortar = 0.0; }
        else wall = cam * (brick / max(lum(wallColor.rgb), 0.05)) * 0.55;   // camera lit by the brick relief
    } else { wall = backdrop(uv); mortar = 0.0; }

    // paint layer
    vec4 S = paintAt(suv);
    if (gDrop > 0.02) {                                               // drop: chromatic burst in the paint
        vec2 co = (suv - 0.5) * 0.02 * gDrop;
        S.r = max(S.r, paintAt(suv + co).r * 0.6);
    }
    // the buff patch: a slightly mismatched coat with roller streaks
    float patchMix = S.b * (ws < 0.5 ? 1.0 : 0.6);
    vec3 patchC = wall * vec3(1.05, 0.99, 0.96) + vec3(0.012, 0.008, 0.006);
    patchC *= 0.97 + 0.04 * h12(vec2(floor(p.x * 140.0), 3.0));
    wall = mix(wall, patchC, patchMix);

    float drip = smoothstep(0.06, 0.22, S.g);
    float cov = max(smoothstep(0.5, 0.82, S.r), drip);               // solid tag paint
    float cloudA = clamp(S.r / 0.5, 0.0, 1.0);                       // cloud / mist ink
    float thin = mix(1.0, 0.72, mortar * (1.0 - smoothstep(0.6, 0.95, cov)));   // spray skips the grooves a little
    float a = clamp(cov * thin, 0.0, 1.0);
    // enamel sheen on thick paint
    float e = 1.5 / R.y;
    float cx = paintAt(suv + vec2(e * R.y / R.x, 0.0)).r, cy = paintAt(suv + vec2(0.0, e)).r;
    vec3 pn = normalize(vec3(-(cx - S.r) * 2.5, -(cy - S.r) * 2.5, 1.0));
    float pspec = pow(max(dot(pn, normalize(vec3(0.3, 0.5, 1.0))), 0.0), 30.0) * smoothstep(0.5, 0.9, cov);
    vec3 pc = paintColor.rgb * (0.9 + 0.12 * smoothstep(0.6, 1.0, cov)) + vec3(0.12) * pspec * relief;
    // clouds: soft airbrushed white with a cumulus shade (lit top, greyer underside)
    float cys = paintAt(suv + vec2(0.0, 9.0 / R.y)).r, cyd = paintAt(suv - vec2(0.0, 9.0 / R.y)).r;
    float cshade = clamp(0.95 + 1.6 * (cyd - cys) + 0.15 * smoothstep(0.3, 0.5, S.r), 0.7, 1.15);   // lit tops, greyer bellies
    vec3 cc = cloudColor.rgb * cshade;
    // paint styles: a rainbow fade fill, a chrome fill with a black outline, or blacklight paint on a dark wall
    float pfx = float(paintFX);
    if (pfx > 0.5) {
        float edgeP = clamp(length(vec2(cx - S.r, cy - S.r)) * 9.0, 0.0, 1.0);
        if (pfx < 1.5) {
            vec3 fade = 0.5 + 0.5 * cos(6.2832 * (vec3(0.0, 0.33, 0.67) + p.y * 1.6 + p.x * 0.35 + TIME * 0.05));
            pc = mix(pc, fade * 1.15 + 0.05, 0.85) + vec3(0.12) * pspec * relief;
            pc = mix(pc, vec3(0.02), edgeP * 0.8);
        } else if (pfx < 2.5) {
            float band = fract(p.y * 5.0 + 0.1 * sin(p.x * 9.0));
            vec3 chrome = mix(vec3(0.95, 0.97, 1.0), vec3(0.25, 0.28, 0.35), smoothstep(0.42, 0.55, band)) ;
            chrome = mix(chrome, vec3(0.75, 0.6, 0.45), smoothstep(0.75, 0.95, band) * 0.6);
            chrome += vec3(1.0) * pow(pspec, 0.5) * 0.6 + vec3(1.0) * step(0.997, h12(floor(p * 160.0))) * (0.5 + 0.5 * sin(TIME * 6.0));
            pc = mix(chrome, vec3(0.01), edgeP);
        } else {
            wall *= 0.12; wall += vec3(0.04, 0.0, 0.08);
            cc = mix(cc * 0.15, vec3(0.6, 0.2, 1.0), 0.35);
            vec3 uvC = 0.5 + 0.5 * cos(6.2832 * (vec3(0.0, 0.33, 0.67) + p.x * 0.5 + TIME * 0.07));
            pc = uvC * 1.6 + 0.1;
        }
    }
    vec3 col = mix(wall, cc, cloudA * cloudOpacity * mix(1.0, 0.85, mortar));
    col = mix(col, pc, a);

    // fresh-paint glow: beats and the drop make the newest paint bloom
    vec3 gsum = vec3(0.0);
    for (int i = 0; i < 8; i++) {
        float fi = float(i);
        float ang = fi * 2.39996 + 0.7;
        vec2 o = vec2(cos(ang), sin(ang)) * (5.0 + 3.0 * fi) / R;
        vec4 t = paintAt(suv + o);
        gsum += vec3(t.r * t.a);
    }
    float gk = glowAmt * (0.25 + 0.9 * audioBeatPulse * ar + 1.4 * gDrop + 0.35 * gPeak);
    vec3 glow = gsum / 8.0 * gk * mix(paintColor.rgb, vec3(1.0), 0.4);
    if (pfx > 2.5) { glow *= 2.5 * (0.5 + 0.5 * cos(6.2832 * (vec3(0.0, 0.33, 0.67) + p.x * 0.5 + TIME * 0.07))); }
    col += glow * 0.6 + paintColor.rgb * S.a * a * gk * 0.35;

    // live spray mist at every pen that is down (pen heads from the state texels)
    for (int k = 0; k < 3; k++) {
        vec4 hd = stateA(float(k) * 5.0 + 4.0);
        if (hd.z > 0.5) {
            float d = length(p - hd.xy);
            float wq = hd.w * 4.0;
            col += paintColor.rgb * exp(-d * d / (wq * wq)) * (0.12 + 0.25 * audioBeatPulse * ar);
        }
    }
    // the hand can: a soft nozzle ring where it aims
    float cm = float(canMode);
    if (cm > 0.5) {
        vec2 m1 = (mousePos * R - 0.5 * R) / R.y;
        float d = length(p - m1);
        float rr = 0.0042 * lineWidth * canSize * 1.3 * 3.0;
        float on = cm > 1.5 ? 1.0 : step(0.5, mouseDown);
        col += paintColor.rgb * exp(-d * d / (rr * rr)) * 0.15 * on;
    }

    // show lift, drop flash, vignette, grain
    col *= 0.9 + 0.2 * gLift;
    col += vec3(0.9, 0.95, 1.0) * gDrop * gDrop * 0.08;
    vec2 dc = uv - 0.5;
    col *= 1.0 - 0.16 * smoothstep(0.45, 1.0, length(dc * vec2(R.x / R.y, 1.0)));
    col += (h12(fc + fract(TIME * 13.7) * 91.0) - 0.5) * 0.025;
    col += (h12(fc * 1.03 + fract(TIME * 7.1) * 57.0) + h12(fc * 0.97 + 11.0) - 1.0) * (1.5 / 255.0);
    col = studioPalette(clamp(col, 0.0, 1.0));
    float alpha = transparentBg ? clamp(max(a, cloudA * cloudOpacity) + lum(glow), 0.0, 1.0) : 1.0;
    return vec4(clamp(col, 0.0, 1.0), alpha);
}

void main() {
    showDynamics();
    gAsp = RENDERSIZE.x / RENDERSIZE.y;
    gBody = float(bodyMode) > 0.5 ? smoothstep(0.3, 0.7, bodyPresence()) : 0.0;
    if (PASSINDEX == 0) gl_FragColor = paintPass();
    else                gl_FragColor = finalPass();
}
