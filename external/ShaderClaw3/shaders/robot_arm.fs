/*{
  "DESCRIPTION": "Robot arm choreography — 1 or 2 articulated industrial arms (forward kinematics) tracing audio-reactive paths in a workshop environment. Bass triggers pose snaps, mids drive speed, treble adds servo-jitter. Inspired by Madeline Gannon's Manus, Quayola's Asymmetric Archeology, and KUKA pick-and-place choreographies.",
  "CREDIT": "ShaderClaw — curated revision",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "keyAngle",
      "LABEL": "Key Light Angle",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6.2832,
      "DEFAULT": 0.785
    },
    {
      "NAME": "keyElevation",
      "LABEL": "Key Elevation",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5708,
      "DEFAULT": 0.7
    },
    {
      "NAME": "ambient",
      "LABEL": "Ambient",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0.08
    },
    {
      "NAME": "rimStrength",
      "LABEL": "Rim Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.5
    },
    {
      "NAME": "mood",
      "LABEL": "Mood",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Solo Reach",
        "Dual Choreography",
        "Pick & Place",
        "Conductor"
      ]
    },
    {
      "NAME": "floorReflection",
      "LABEL": "Floor Reflection",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.35
    },
    {
      "NAME": "mouseX",
      "LABEL": "Mouse X",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "mouseY",
      "LABEL": "Mouse Y",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "armSegments",
      "LABEL": "Arm Segments",
      "TYPE": "long",
      "DEFAULT": 5,
      "VALUES": [
        4,
        5,
        6,
        7
      ],
      "LABELS": [
        "4-link",
        "5-link",
        "6-link",
        "7-link"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "armScale",
      "LABEL": "Size",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 1.6,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "armCount",
      "LABEL": "Arm Count",
      "TYPE": "long",
      "DEFAULT": 1,
      "VALUES": [
        1,
        2,
        3,
        4
      ],
      "LABELS": [
        "1 Arm",
        "2 Arms",
        "3 Arms",
        "4 Arms"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "keyColor",
      "LABEL": "Key Light",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.94,
        0.82,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "fillColor",
      "LABEL": "Fill Light",
      "TYPE": "color",
      "DEFAULT": [
        0.55,
        0.7,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "exposure",
      "LABEL": "Exposure",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "paintColor",
      "LABEL": "Arm Paint",
      "TYPE": "color",
      "DEFAULT": [
        0.08,
        0.08,
        0.1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "jointColor",
      "LABEL": "Joint",
      "TYPE": "color",
      "DEFAULT": [
        0.18,
        0.2,
        0.24,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "warningColor",
      "LABEL": "Warning",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.78,
        0.1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "ledColor",
      "LABEL": "LED",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.18,
        0.12,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "floorColor",
      "LABEL": "Floor",
      "TYPE": "color",
      "DEFAULT": [
        0.13,
        0.14,
        0.16,
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
      "NAME": "camDist",
      "LABEL": "Camera Distance",
      "TYPE": "float",
      "MIN": 1.5,
      "MAX": 12,
      "DEFAULT": 4.5,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "camHeight",
      "LABEL": "Camera Height",
      "TYPE": "float",
      "MIN": -3,
      "MAX": 4,
      "DEFAULT": 1.2,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "camOrbitSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.18,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "camAzimuth",
      "LABEL": "Camera Azimuth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6.2832,
      "DEFAULT": 0,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "showGrid",
      "LABEL": "Engineering Grid",
      "TYPE": "bool",
      "DEFAULT": true,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "LABEL": "Background",
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ROBOT ARM CHOREOGRAPHY — audio-driven, MediaPipe-free.
// Forward kinematics, bass-snap pose targets, mid-tempo, treble servo-jitter.

float sdCircle(vec2 p, vec2 c, float r) { return length(p - c) - r; }
float hash11(float n) { return fract(sin(n * 91.345) * 43758.5453); }

// Universal lighting helper: key + fill + ambient + rim + spec, returns lit color
vec3 universalLight(vec3 N, vec3 baseCol, vec3 Lkey, vec3 Lfil,
                    vec3 keyTint, vec3 fillTint, float amb, float rimAmt, float specAmt)
{
    float diffK = max(0.0, dot(N, Lkey));
    float diffF = max(0.0, dot(N, Lfil));
    float spec  = pow(max(0.0, dot(reflect(-Lkey, N), vec3(0.0, 0.0, 1.0))), 64.0);
    float rim   = pow(1.0 - max(0.0, N.z), 3.0);
    vec3 lit = baseCol * (amb + diffK * keyTint + diffF * 0.45 * fillTint)
             + spec * keyTint * specAmt
             + baseCol * rim * rimAmt;
    return lit;
}

// ── Capsule shading: metallic + warning stripes + LED dot ──────────────
vec3 shadeSegment(vec2 p, vec2 a, vec2 b, float r,
                  vec3 baseCol, vec3 warnCol, vec3 ledCol, float ledPulse,
                  vec3 Lkey, vec3 Lfil, vec3 keyTint, vec3 fillTint,
                  float amb, float rimAmt, float px, out float mask)
{
    vec2 pa = p - a, ba = b - a;
    float bl = max(length(ba), 1e-4);
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    vec2 closest = a + ba * h;
    float dist = length(p - closest);
    mask = smoothstep(r + px, r - px, dist);
    if (mask < 0.001) return vec3(0.0);

    float t = clamp(dist / r, 0.0, 1.0);
    float nz = sqrt(max(0.0, 1.0 - t * t));
    vec2 perp = (dist > 1e-4) ? (p - closest) / dist : vec2(0.0, 1.0);
    vec3 N = normalize(vec3(perp, nz));

    // Brushed paint stripes (longitudinal) — very subtle
    float brush = 0.5 + 0.5 * sin(perp.x * 80.0 + h * 4.0);
    vec3  metal = baseCol * (0.95 + 0.05 * brush);

    // Joint-end warning stripes (yellow/black diagonals near caps)
    float capProx = min(h, 1.0 - h);
    float capBand = smoothstep(0.18, 0.06, capProx);
    float diag    = step(0.5, fract(h * 18.0 + perp.y * 4.0));
    vec3  warn    = mix(vec3(0.04), warnCol, diag);
    metal = mix(metal, warn, capBand * 0.55);

    // Tiny LED dot near distal end of segment
    vec2 ledPos = a + ba * 0.82;
    float ledD = length(p - ledPos);
    float led = smoothstep(0.012, 0.004, ledD);
    vec3  ledEmit = ledCol * (0.4 + 1.6 * ledPulse) * led;

    vec3 lit = universalLight(N, metal, Lkey, Lfil, keyTint, fillTint, amb, rimAmt, 0.6);
    return lit + ledEmit;
}

// Joint disc (the dark hub between segments)
vec3 shadeJoint(vec2 p, vec2 c, float r, vec3 jc, vec3 warnCol,
                vec3 Lkey, vec3 Lfil, vec3 keyTint, vec3 fillTint,
                float amb, float rimAmt, float px, out float mask)
{
    float d = sdCircle(p, c, r);
    mask = smoothstep(px, -px, d);
    if (mask < 0.001) return vec3(0.0);
    float t = clamp(length(p - c) / r, 0.0, 1.0);
    float nz = sqrt(max(0.0, 1.0 - t * t));
    vec2 dir = (p - c) / max(length(p - c), 1e-4);
    vec3 N = normalize(vec3(dir, nz));

    float ring = smoothstep(0.04, 0.0, abs(t - 0.66)) * 0.45;
    float ang = atan(dir.y, dir.x);
    float bolt = smoothstep(0.08, 0.0, abs(fract(ang / 1.5708 + 0.5) - 0.5) - 0.1)
               * smoothstep(0.05, 0.0, abs(t - 0.45));
    vec3 base = jc;
    vec3 c0 = universalLight(N, base, Lkey, Lfil, keyTint, fillTint, amb + 0.15, rimAmt * 0.5, 0.3);
    c0 = mix(c0, warnCol * 0.9, ring * 0.18);
    c0 = mix(c0, vec3(0.06), bolt);
    return c0;
}

// Forward-kinematic chain. Bass-triggered pose targets ease into angles.
struct Pose {
    float a0; float a1; float a2; float a3; float a4; float a5; float a6;
};

Pose mixPose(Pose p, Pose q, float t) {
    Pose r;
    r.a0 = mix(p.a0, q.a0, t); r.a1 = mix(p.a1, q.a1, t);
    r.a2 = mix(p.a2, q.a2, t); r.a3 = mix(p.a3, q.a3, t);
    r.a4 = mix(p.a4, q.a4, t); r.a5 = mix(p.a5, q.a5, t);
    r.a6 = mix(p.a6, q.a6, t);
    return r;
}

Pose poseForSeed(float seed, float spread) {
    Pose p;
    p.a0 = mix(-0.4, 0.4, hash11(seed + 1.0)) * spread;
    p.a1 = mix(-1.4, 0.2, hash11(seed + 2.0));
    p.a2 = mix(-0.4, 1.8, hash11(seed + 3.0));
    p.a3 = mix(-1.0, 1.0, hash11(seed + 4.0));
    p.a4 = mix(-0.6, 0.6, hash11(seed + 5.0));
    p.a5 = mix(-0.5, 0.5, hash11(seed + 6.0));
    p.a6 = mix(-0.4, 0.4, hash11(seed + 7.0));
    return p;
}

vec2 moodTarget(int moodId, float t, float bass, float aspect, float side) {
    if (moodId == 2) {
        float ph = fract(t * 0.35);
        vec2 binA = vec2(-aspect * 0.32, -0.30);
        vec2 binB = vec2( aspect * 0.32, -0.30);
        vec2 mid  = mix(binA, binB, smoothstep(0.0, 1.0, ph));
        float arc = sin(ph * 3.14159) * 0.42;
        return vec2(mid.x, mid.y + arc);
    } else if (moodId == 3) {
        float beat = mod(floor(t * 2.2), 4.0);
        vec2 g0 = vec2( 0.00,  0.30);
        vec2 g1 = vec2( 0.34, -0.05);
        vec2 g2 = vec2( 0.00, -0.20);
        vec2 g3 = vec2(-0.34, -0.05);
        vec2 dst = (beat < 0.5) ? g0 : (beat < 1.5) ? g1 : (beat < 2.5) ? g2 : g3;
        return dst * (0.95 + 0.10 * bass) + vec2(0.0, -0.05);
    }
    return vec2(0.0);
}

// Solve chain for up to 7 segments
void solveChain(vec2 base, float scl, int nseg, float[7] angs, out vec2[8] pts) {
    float seg[7];
    seg[0] = 0.21 * scl; seg[1] = 0.18 * scl; seg[2] = 0.15 * scl;
    seg[3] = 0.10 * scl; seg[4] = 0.08 * scl; seg[5] = 0.07 * scl;
    seg[6] = 0.06 * scl;

    pts[0] = base;
    float cum = 1.5708;
    for (int i = 0; i < 7; i++) {
        if (i >= nseg) { pts[i + 1] = pts[i]; continue; }
        cum += angs[i];
        pts[i + 1] = pts[i] + seg[i] * vec2(cos(cum), sin(cum));
    }
}

void poseToAngs(Pose p, out float[7] a) {
    a[0] = p.a0; a[1] = p.a1; a[2] = p.a2; a[3] = p.a3;
    a[4] = p.a4; a[5] = p.a5; a[6] = p.a6;
}

// Render one arm
void drawArm(vec2 p, vec2 base, Pose pose, float scl, int nseg, float jitter, float ledPulse,
             vec3 armC, vec3 jntC, vec3 warnC, vec3 ledC,
             vec3 Lkey, vec3 Lfil, vec3 keyTint, vec3 fillTint, float amb, float rimAmt,
             float px, inout vec3 col, inout float armMask)
{
    float a[7]; poseToAngs(pose, a);
    a[0] += (hash11(floor(TIME * 60.0) + 0.1) - 0.5) * jitter;
    a[1] += (hash11(floor(TIME * 60.0) + 0.3) - 0.5) * jitter * 1.2;
    a[2] += (hash11(floor(TIME * 60.0) + 0.5) - 0.5) * jitter * 1.4;
    a[3] += (hash11(floor(TIME * 60.0) + 0.7) - 0.5) * jitter * 1.6;
    a[4] += (hash11(floor(TIME * 60.0) + 0.9) - 0.5) * jitter * 1.8;
    a[5] += (hash11(floor(TIME * 60.0) + 1.1) - 0.5) * jitter * 1.9;
    a[6] += (hash11(floor(TIME * 60.0) + 1.3) - 0.5) * jitter * 2.0;

    vec2 pts[8];
    solveChain(base, scl, nseg, a, pts);

    float radii[7];
    radii[0] = 0.034 * scl; radii[1] = 0.028 * scl; radii[2] = 0.022 * scl;
    radii[3] = 0.017 * scl; radii[4] = 0.014 * scl; radii[5] = 0.012 * scl;
    radii[6] = 0.010 * scl;

    for (int i = 0; i < 7; i++) {
        if (i >= nseg) break;
        float m;
        vec3 c = shadeSegment(p, pts[i], pts[i + 1], radii[i],
                              armC, warnC, ledC, ledPulse,
                              Lkey, Lfil, keyTint, fillTint, amb, rimAmt, px, m);
        col = mix(col, c, m);
        armMask = max(armMask, m);
    }

    float jr[8];
    jr[0] = 0.040 * scl; jr[1] = 0.034 * scl; jr[2] = 0.027 * scl; jr[3] = 0.022 * scl;
    jr[4] = 0.018 * scl; jr[5] = 0.015 * scl; jr[6] = 0.012 * scl; jr[7] = 0.010 * scl;
    for (int j = 0; j < 8; j++) {
        if (j > nseg) break;
        float m;
        vec3 c = shadeJoint(p, pts[j], jr[j], jntC, warnC,
                            Lkey, Lfil, keyTint, fillTint, amb, rimAmt, px, m);
        col = mix(col, c, m);
        armMask = max(armMask, m);
    }

    // Mounting plinth
    float plinth = max(abs(p.x - base.x) - 0.075 * scl, abs(p.y - base.y + 0.045 * scl) - 0.045 * scl);
    float pm = smoothstep(px, -px, plinth);
    col = mix(col, jntC * 0.55, pm);
    armMask = max(armMask, pm);

    // Emergency-stop indicator (uses LED color)
    vec2 stopC = base + vec2(0.045 * scl, -0.04 * scl);
    float stopD = length(p - stopC) - 0.012 * scl;
    float sm = smoothstep(px, -px, stopD);
    vec3 stopGlow = ledC * (0.5 + 0.5 * sin(TIME * 1.7));
    col = mix(col, stopGlow * 0.9, sm);
    armMask = max(armMask, sm);
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    vec2 p = (uv - 0.5) * vec2(aspect, 1.0);
    float px = 1.5 / RENDERSIZE.y;

    // ── Universal camera: orbiting azimuth shifts horizontal framing,
    //    camHeight shifts vertical, camDist scales the scene ──────────
    float orbit = camAzimuth + TIME * camOrbitSpeed;
    float camOffsetX = sin(orbit) * 0.15;
    float zoom = 4.5 / max(camDist, 0.1);
    // Continuous band-follow (ambient fix r2): LINEAR bands — the bands are
    // already smoothed upstream, and any knee crushes ambient's 0.1-0.8
    // swells. Bass breathes the camera zoom so beatless swells move every
    // edge — no beat gating, silence = exactly the authored framing.
    float bassF = clamp(audioBass, 0.0, 1.0) * audioReact;
    float midF  = clamp(audioMid,  0.0, 1.0) * audioReact;
    p = p / (max(zoom, 0.01) * (1.0 + 0.11 * bassF));
    p.x += camOffsetX;
    p.y -= camHeight * 0.08;

    // ── Universal lighting: build key/fill direction vectors ────────
    float kx = cos(keyAngle) * cos(keyElevation);
    float ky = sin(keyElevation);
    float kz = sin(keyAngle) * cos(keyElevation);
    vec3 Lkey = normalize(vec3(kx, ky, kz));
    vec3 Lfil = normalize(vec3(-kx * 0.6, -ky * 0.4, kz * 0.7 + 0.3));
    vec3 keyTint = keyColor.rgb;
    vec3 fillTint = fillColor.rgb;

    float bass = clamp(audioBass, 0.0, 1.0) * audioReact;
    float mids = clamp(audioMid,  0.0, 1.0) * audioReact;
    float treb = clamp(audioHigh, 0.0, 1.0) * audioReact;
    int moodId = int(mood);
    int nseg = int(armSegments);

    // Workshop floor: concrete + horizon gradient + grain
    vec3 col = floorColor.rgb;
    float horizon = 1.0 - smoothstep(0.0, 0.7, abs(p.y));
    col += vec3(0.04, 0.045, 0.05) * horizon;
    float n = fract(sin(dot(floor(p * 480.0), vec2(12.9898, 78.233))) * 43758.5453);
    col += (n - 0.5) * 0.012;

    if (showGrid) {
        vec2 gp = p * 14.0;
        vec2 gf = abs(fract(gp) - 0.5);
        float fine = smoothstep(0.49, 0.5, max(gf.x, gf.y));
        vec2 gp2 = p * 2.8;
        vec2 gf2 = abs(fract(gp2) - 0.5);
        float major = smoothstep(0.495, 0.5, max(gf2.x, gf2.y));
        col += vec3(0.08, 0.09, 0.11) * fine * 0.35;
        col += vec3(0.18, 0.20, 0.24) * major * 0.55;
    }

    float speed = 0.45 + mids * 1.3;
    float beatT = TIME * speed + bass * 0.6;
    float beatI = floor(beatT);
    float beatF = fract(beatT);
    float ease = 1.0 - pow(1.0 - beatF, 3.0);

    float jitter = treb * 0.06;
    float ledPulse = 0.3 + 0.7 * smoothstep(0.0, 0.25, beatF);

    int nArms = int(clamp(float(armCount), 1.0, 4.0)); // WebGL: long inputs arrive as float; int clamp() has no (float,int,int) overload
    bool mouseActive = (abs(mouseX) > 0.001 || abs(mouseY) > 0.001);

    // Mouse target in scene-space (matches normalized p coordinates: x in ~[-aspect/2, aspect/2], y in [-0.5, 0.5])
    vec2 mouseTgt = vec2(mouseX * aspect * 0.5, mouseY * 0.5);

    float shadowY = -0.34;

    // Compute base positions: spread evenly across screen
    // For 1 arm: center. For N arms: spread across [-aspect*0.35, +aspect*0.35]
    // Floor reflection pass first
    if (floorReflection > 0.001 && p.y < shadowY) {
        vec2 mp = vec2(p.x, 2.0 * shadowY - p.y);
        vec3 refCol = col;
        float refMask = 0.0;
        for (int i = 0; i < 4; i++) {
            if (i >= nArms) break;
            float fi = float(i);
            float fn = float(nArms);
            float t = (fn > 1.0) ? (fi / (fn - 1.0)) : 0.5;
            float bx = mix(-aspect * 0.35, aspect * 0.35, t);
            if (nArms == 1) bx = 0.0;
            vec2 base_i = vec2(bx, shadowY);

            float seedOff = fi * 13.0;
            Pose poseA = poseForSeed(beatI * 1.7 + seedOff,         1.0);
            Pose poseB = poseForSeed((beatI + 1.0) * 1.7 + seedOff, 1.0);
            Pose pose_i = mixPose(poseA, poseB, ease);

            if (mouseActive) {
                vec2 dir = mouseTgt - base_i;
                float ang = atan(dir.y, dir.x) - 1.5708;
                float reach = clamp(length(dir) / (armScale * 0.6), 0.2, 1.0);
                pose_i.a0 = ang * 0.6;
                pose_i.a1 = mix(-1.2, -0.2, reach);
                pose_i.a2 = mix( 1.6,  0.3, reach);
                pose_i.a3 = sin(TIME * 2.0 + bass * 4.0) * 0.2;
                pose_i.a4 = cos(TIME * 1.7) * 0.15;
                pose_i.a5 = sin(TIME * 1.3) * 0.12;
                pose_i.a6 = cos(TIME * 2.4) * 0.10;
            } else if (moodId == 2 || moodId == 3) {
                vec2 tgt = moodTarget(moodId, TIME, bass, aspect, 1.0);
                vec2 dir = tgt - base_i;
                float ang = atan(dir.y, dir.x) - 1.5708;
                float reach = clamp(length(dir) / (armScale * 0.6), 0.2, 1.0);
                pose_i.a0 = ang * 0.6;
                pose_i.a1 = mix(-1.2, -0.2, reach);
                pose_i.a2 = mix( 1.6,  0.3, reach);
                pose_i.a3 = sin(TIME * 2.0 + bass * 4.0) * 0.4;
                pose_i.a4 = cos(TIME * 1.7) * 0.3;
                pose_i.a5 = sin(TIME * 1.3) * 0.25;
                pose_i.a6 = cos(TIME * 2.4) * 0.2;
            }

            drawArm(mp, base_i, pose_i, armScale, nseg, jitter * (1.0 + fi * 0.05), ledPulse * (1.0 - fi * 0.05),
                    paintColor.rgb, jointColor.rgb, warningColor.rgb, ledColor.rgb,
                    Lkey, Lfil, keyTint, fillTint, ambient, rimStrength, px, refCol, refMask);
        }
        float falloff = smoothstep(0.45, 0.0, shadowY - p.y);
        col = mix(col, refCol, refMask * floorReflection * falloff * 0.9);
    }

    // Main arm pass
    float armMask = 0.0;
    for (int i = 0; i < 4; i++) {
        if (i >= nArms) break;
        float fi = float(i);
        float fn = float(nArms);
        float t = (fn > 1.0) ? (fi / (fn - 1.0)) : 0.5;
        float bx = mix(-aspect * 0.35, aspect * 0.35, t);
        if (nArms == 1) bx = 0.0;
        vec2 base_i = vec2(bx, shadowY);

        float seedOff = fi * 13.0;
        Pose poseA = poseForSeed(beatI * 1.7 + seedOff,         1.0);
        Pose poseB = poseForSeed((beatI + 1.0) * 1.7 + seedOff, 1.0);
        Pose pose_i = mixPose(poseA, poseB, ease);

        if (mouseActive) {
            vec2 dir = mouseTgt - base_i;
            float ang = atan(dir.y, dir.x) - 1.5708;
            float reach = clamp(length(dir) / (armScale * 0.6), 0.2, 1.0);
            pose_i.a0 = ang * 0.6;
            pose_i.a1 = mix(-1.2, -0.2, reach);
            pose_i.a2 = mix( 1.6,  0.3, reach);
            pose_i.a3 = sin(TIME * 2.0 + bass * 4.0) * 0.2;
            pose_i.a4 = cos(TIME * 1.7) * 0.15;
            pose_i.a5 = sin(TIME * 1.3) * 0.12;
            pose_i.a6 = cos(TIME * 2.4) * 0.10;
        } else if (moodId == 2 || moodId == 3) {
            vec2 tgt = moodTarget(moodId, TIME, bass, aspect, 1.0);
            vec2 dir = tgt - base_i;
            float ang = atan(dir.y, dir.x) - 1.5708;
            float reach = clamp(length(dir) / (armScale * 0.6), 0.2, 1.0);
            pose_i.a0 = ang * 0.6;
            pose_i.a1 = mix(-1.2, -0.2, reach);
            pose_i.a2 = mix( 1.6,  0.3, reach);
            pose_i.a3 = sin(TIME * 2.0 + bass * 4.0) * 0.4;
            pose_i.a4 = cos(TIME * 1.7) * 0.3;
            pose_i.a5 = sin(TIME * 1.3) * 0.25;
            pose_i.a6 = cos(TIME * 2.4) * 0.2;
        }

        drawArm(p, base_i, pose_i, armScale, nseg, jitter * (1.0 + fi * 0.05), ledPulse * (1.0 - fi * 0.05),
                paintColor.rgb, jointColor.rgb, warningColor.rgb, ledColor.rgb,
                Lkey, Lfil, keyTint, fillTint, ambient, rimStrength, px, col, armMask);

        // Floor contact shadow per arm
        float sh = exp(-pow((p.x - base_i.x) * 4.5, 2.0))
                 * smoothstep(0.0, 0.06, shadowY - p.y + 0.01)
                 * smoothstep(0.10, 0.0, abs(p.y - shadowY + 0.04));
        col *= 1.0 - sh * 0.45;
    }

    // Vignette
    float vig = 1.0 - smoothstep(0.4, 1.05, length(p));
    col *= mix(0.78, 1.0, vig);

    // Universal exposure + bass HDR boost + continuous smooth band-follow
    // (bass/mid lift the workshop light so beatless material reads).
    // r3 (measured): 0.40 total bass depth x meanLuma 0.18 gave rMag 0.0006 —
    // no clipping involved, just underpowered. Deepened to clear the floor.
    col *= exposure * (1.0 + bass * 0.18 + 0.50 * bassF + 0.48 * midF + 0.16 * treb);

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    vec3 uc = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hueA = hueShift * 6.2831853;
        float hueC = cos(hueA), hueS = sin(hueA);
        mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                  + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                  + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hueM * uc, 0.0, 1.0);
    }
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

    gl_FragColor = vec4(uc, 1.0);
}
