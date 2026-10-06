/*{
  "CATEGORIES": [
    "Generator",
    "Art Movement",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Fauvism after Matisse — four discrete moods, each its own visual world. La Danse (1909–10): five silhouetted figures ringing on cobalt/viridian ground, holding hands, audio bass throws the tempo. Jazz Cut-Out (1947): flat Icarus-style paper shapes — cobalt body, red heart-circle, yellow stars on white. Femme au chapeau (1905): the Salon scandal portrait — central oval head with green nose-stripe, vermilion cheek, viridian shadow. Goldfish (1912): orange-vermilion fish in glass bowl, flattened multi-perspective. Loud, unmixed, wild-beasts color. Single-pass, LINEAR HDR.",
  "INPUTS": [
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
        "La Danse",
        "Jazz Cut-Out",
        "Femme au chapeau",
        "Goldfish (1912)"
      ]
    },
    {
      "NAME": "paintTooth",
      "LABEL": "Paint Tooth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.4,
      "DEFAULT": 0.12,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "figureScale",
      "LABEL": "Figure Scale",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "tempo",
      "LABEL": "Tempo",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.7,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "wildness",
      "LABEL": "Wildness",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "gestureSpeed",
      "LABEL": "Gesture Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
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

// ════════════════════════════════════════════════════════════════════════
//  Fauvism — Matisse, four moods.
//  Each mood is a self-contained scene. No cross-blending: when you flip
//  the enum the world changes. Palette is loud, unmixed, saturated.
// ════════════════════════════════════════════════════════════════════════

// ─── shared palette (loud, unmixed) ──────────────────────────────────
const vec3 VERMILION = vec3(0.96, 0.22, 0.12);
const vec3 COBALT    = vec3(0.05, 0.22, 0.78);
const vec3 VIRIDIAN  = vec3(0.05, 0.55, 0.42);
const vec3 LEMON     = vec3(0.98, 0.86, 0.16);
const vec3 ROSE      = vec3(0.95, 0.32, 0.55);
const vec3 ORANGE    = vec3(0.98, 0.52, 0.10);
const vec3 EMERALD   = vec3(0.10, 0.70, 0.30);
const vec3 PAPER     = vec3(0.97, 0.95, 0.88);

// ─── hash / noise / tooth ────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float hash21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// painter's tooth — coarse canvas grain, no fine noise
float paintGrain(vec2 p) {
    vec2 g = floor(p * 220.0);
    return hash21(g) - 0.5;
}

mat2 rot(float a) { float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }

// ─── SDFs ────────────────────────────────────────────────────────────
float sdCircle(vec2 p, float r) { return length(p) - r; }
float sdEllipse(vec2 p, vec2 r) { return (length(p / r) - 1.0) * min(r.x, r.y); }
float sdSegment(vec2 p, vec2 a, vec2 b, float r) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h) - r;
}
// 5-pointed star
float sdStar(vec2 p, float r) {
    p = abs(p);
    float a = atan(p.x, p.y);
    float n = 5.0;
    float ang = mod(a, 6.2831853 / n) - 3.1415926 / n;
    return length(p) * cos(ang) - r * cos(3.1415926 / n);
}

// ─── MOOD 0 — La Danse ───────────────────────────────────────────────
//  Five silhouetted figures in a ring on saturated cobalt/viridian ground.
//  Heads + torso + reaching arms. Slowly rotating. Bass throws tempo.
vec3 moodLaDanse(vec2 uv, float t, float audio, float wild, float fScale) {
    // Two-band ground — cobalt sky upper, viridian ground lower (horizon ~0.55)
    float horizon = 0.52 + 0.04 * sin(uv.x * 3.0);
    vec3 col = uv.y > horizon ? COBALT : VIRIDIAN;

    // Slowly rotating ring center
    vec2 center = vec2(0.5, 0.46);
    vec2 q = (uv - center) / fScale;
    q.x *= 1.0;  // already aspect-corrected by caller

    float ring = 0.30;
    // Constant tempo (silence-identical: audio rests at 0.8*audioReact) plus a
    // small BOUNDED bass throw — never multiply t by an audio-driven rate.
    float dance = t * (0.4 + 0.48 * clamp(audioReact, 0.0, 2.0))
                + 0.30 * audioBass;

    float bestD = 1e9;
    int bestI = 0;
    // Five figures
    for (int i = 0; i < 5; i++) {
        float fi = float(i);
        float a = dance + fi * 1.25663706;  // 2pi/5
        vec2 fp = vec2(cos(a), sin(a)) * ring;

        // Figure-local coords — rotate with the dance so they face outward
        vec2 lp = q - fp;
        // Tilt slightly outward (Matisse's bodies lean away from center)
        float lean = a + 1.5707963;
        lp = rot(-lean) * lp;

        // Head — slightly above the body
        float head = sdCircle(lp - vec2(0.0, 0.085), 0.040);

        // Torso — vertical capsule
        float torso = sdSegment(lp, vec2(0.0, 0.05), vec2(0.0, -0.06), 0.038);

        // Reaching arms — toward neighbors (left/right)
        float armSwing = sin(dance * 2.0 + fi) * 0.05 * wild;
        float armL = sdSegment(lp,
            vec2(-0.02, 0.03),
            vec2(-0.115, 0.012 + armSwing),
            0.018);
        float armR = sdSegment(lp,
            vec2( 0.02, 0.03),
            vec2( 0.115, 0.012 - armSwing),
            0.018);

        // Legs — slight stride
        float stride = sin(dance * 2.0 + fi * 1.7) * 0.02;
        float legL = sdSegment(lp, vec2(-0.012, -0.06), vec2(-0.025 + stride, -0.135), 0.020);
        float legR = sdSegment(lp, vec2( 0.012, -0.06), vec2( 0.025 - stride, -0.135), 0.020);

        float fig = min(head, min(torso, min(min(armL, armR), min(legL, legR))));
        if (fig < bestD) { bestD = fig; bestI = i; }
    }

    // Figures are silhouetted in vermilion-pink (Matisse's terracotta-red bodies)
    vec3 figureCol = vec3(0.86, 0.30, 0.18);  // Matisse's body-red
    float silh = 1.0 - smoothstep(0.0, 0.004, bestD * fScale);
    col = mix(col, figureCol, silh);

    // Tiny rim of darker red where bodies meet ground (Matisse outlined)
    float rim = smoothstep(0.006, 0.0, abs(bestD * fScale)) * 0.6;
    col = mix(col, figureCol * 0.55, rim * 0.4);

    return col;
}

// ─── MOOD 1 — Jazz Cut-Out (Icarus, plate VIII, 1947) ───────────────
//  Flat colored paper shapes: cobalt body w/ red heart-circle, yellow stars,
//  stark white ground. Audio drives star pulse + heartbeat.
vec3 moodJazz(vec2 uv, float t, float audio, float wild, float fScale) {
    vec3 col = PAPER;  // stark off-white paper

    // Icarus body — falling silhouette, cobalt cut-paper
    // Body roughly centered; arms flung up; legs trailing
    vec2 c = vec2(0.50, 0.48);
    vec2 p = (uv - c) / fScale;
    // Slight sway as if falling
    p.x += 0.012 * sin(t * 0.8);

    // Torso — elongated ellipse
    float torso = sdEllipse(p - vec2(0.0, 0.0), vec2(0.055, 0.13));
    // Head
    float head = sdCircle(p - vec2(0.0, 0.16), 0.045);
    // Arms — flung up and out
    float armL = sdSegment(p, vec2(-0.02, 0.08), vec2(-0.16, 0.20 + 0.01 * sin(t)), 0.025);
    float armR = sdSegment(p, vec2( 0.02, 0.08), vec2( 0.17, 0.18 - 0.01 * sin(t)), 0.025);
    // Legs — kicking
    float legL = sdSegment(p, vec2(-0.025, -0.10), vec2(-0.05, -0.24 + 0.012 * sin(t * 1.3)), 0.030);
    float legR = sdSegment(p, vec2( 0.025, -0.10), vec2( 0.06, -0.23 - 0.012 * sin(t * 1.3)), 0.030);

    float body = min(torso, min(head, min(min(armL, armR), min(legL, legR))));
    // Hard cut-paper edge — almost binary
    float bodyMask = 1.0 - smoothstep(0.0, 0.0015, body * fScale);
    col = mix(col, COBALT, bodyMask);

    // Heart-circle on the body — red, audio-pulses
    float heartBeat = 0.85 + 0.20 * sin(t * (3.0 + audio * 4.0));
    float heart = sdCircle(p - vec2(0.0, 0.02), 0.030 * heartBeat);
    float heartMask = 1.0 - smoothstep(0.0, 0.002, heart * fScale);
    col = mix(col, VERMILION, heartMask);

    // Yellow stars scattered on the page — pulse with audio
    for (int i = 0; i < 9; i++) {
        float fi = float(i);
        vec2 sp = vec2(hash11(fi * 3.7), hash11(fi * 5.13));
        // Push stars away from center mass
        sp = mix(vec2(0.1, 0.1), vec2(0.9, 0.9), sp);
        // Skip ones too close to the body
        vec2 sd = uv - sp;
        if (length(sd - vec2(0.0, 0.0) + (sp - c)) < 0.01) continue;

        float starR = mix(0.020, 0.038, hash11(fi * 7.7));
        starR *= 0.85 + 0.30 * sin(t * 1.5 + fi) * (0.5 + audio * wild * 0.8);

        // Rotate each star a bit
        vec2 lq = uv - sp;
        lq = rot(fi * 0.6 + t * 0.1) * lq;
        float st = sdStar(lq, starR);
        float sm = 1.0 - smoothstep(0.0, 0.0015, st);
        col = mix(col, LEMON, sm);
    }

    // Two small green leaf shapes (Matisse loved tucking these in)
    for (int j = 0; j < 2; j++) {
        float fj = float(j);
        vec2 lp = uv - vec2(0.12 + 0.76 * fj, 0.78 - 0.55 * fj);
        lp = rot(0.8 + fj * 1.2) * lp;
        float leaf = sdEllipse(lp, vec2(0.030, 0.012));
        float lm = 1.0 - smoothstep(0.0, 0.0015, leaf);
        col = mix(col, EMERALD, lm);
    }

    return col;
}

// ─── MOOD 2 — Femme au chapeau (1905) ────────────────────────────────
//  Single oval head, wild non-naturalistic colors: green nose stripe,
//  vermilion cheek, viridian shadow. The Salon scandal piece.
vec3 moodFemme(vec2 uv, float t, float audio, float wild, float fScale) {
    // Background — riotous unblended brushstrokes (rose / orange / yellow patches)
    vec2 bp = uv * vec2(8.0, 6.0);
    vec2 bc = floor(bp);
    float h = hash21(bc);
    vec3 bg;
    if (h < 0.33) bg = ROSE;
    else if (h < 0.66) bg = ORANGE;
    else bg = LEMON;
    // Soften patch borders just a hair
    vec2 bf = fract(bp);
    float patchEdge = min(min(bf.x, 1.0 - bf.x), min(bf.y, 1.0 - bf.y));
    bg *= 0.88 + 0.12 * smoothstep(0.0, 0.04, patchEdge);
    vec3 col = bg;

    vec2 c = vec2(0.50, 0.52);
    vec2 p = (uv - c) / fScale;

    // The hat — large flamboyant ellipse on top, multi-color
    vec2 hp = p - vec2(0.0, 0.20);
    float hat = sdEllipse(hp, vec2(0.26, 0.13));
    float hatMask = 1.0 - smoothstep(0.0, 0.002, hat * fScale);
    // Hat is bands of color — vermilion crown, viridian band, rose plume
    float hatBand = (hp.y + 0.13) / 0.26;
    vec3 hatCol = VERMILION;
    if (hatBand > 0.55) hatCol = VIRIDIAN;
    if (hatBand > 0.78) hatCol = ROSE;
    // Plume — small offset blob
    float plume = sdEllipse(hp - vec2(0.13, 0.06), vec2(0.07, 0.05));
    float plumeMask = 1.0 - smoothstep(0.0, 0.002, plume);
    col = mix(col, hatCol, hatMask);
    col = mix(col, LEMON, plumeMask);

    // Head — oval, off-white-ish base
    vec2 fp = p - vec2(0.0, 0.0);
    float face = sdEllipse(fp, vec2(0.13, 0.18));
    float faceMask = 1.0 - smoothstep(0.0, 0.002, face * fScale);
    // Base skin — pale rose
    vec3 skin = vec3(0.96, 0.78, 0.62);
    vec3 faceCol = skin;

    // The infamous green nose stripe — vertical band down center
    float nose = abs(fp.x) - 0.018;
    float noseStripe = (1.0 - smoothstep(0.0, 0.005, nose)) * step(fp.y, 0.07) * step(-0.10, fp.y);
    faceCol = mix(faceCol, EMERALD, noseStripe * 0.95 * wild);

    // Vermilion cheek — left side, audio-flush
    vec2 cheekP = fp - vec2(-0.065, -0.035);
    float cheek = sdEllipse(cheekP, vec2(0.030, 0.025));
    float cheekMask = (1.0 - smoothstep(0.0, 0.004, cheek)) * (0.85 + 0.15 * audio);
    faceCol = mix(faceCol, VERMILION, cheekMask * wild);

    // Viridian shadow — right side of face (the scandalous one)
    vec2 shadP = fp - vec2(0.06, -0.02);
    float shad = sdEllipse(shadP, vec2(0.045, 0.10));
    float shadMask = (1.0 - smoothstep(0.0, 0.012, shad)) * 0.85;
    faceCol = mix(faceCol, VIRIDIAN, shadMask * wild);

    // Eyes — two small cobalt almond shapes
    float eyeL = sdEllipse(fp - vec2(-0.045, 0.03), vec2(0.012, 0.006));
    float eyeR = sdEllipse(fp - vec2( 0.045, 0.03), vec2(0.012, 0.006));
    float eyeMask = (1.0 - smoothstep(0.0, 0.0015, min(eyeL, eyeR)));
    faceCol = mix(faceCol, COBALT, eyeMask);

    // Mouth — small vermilion mark, gently pulses
    float mouth = sdEllipse(fp - vec2(0.0, -0.085), vec2(0.022, 0.008 * (0.9 + 0.2 * sin(t * 0.7))));
    float mouthMask = 1.0 - smoothstep(0.0, 0.002, mouth);
    faceCol = mix(faceCol, vec3(0.88, 0.18, 0.28), mouthMask);

    col = mix(col, faceCol, faceMask);

    return col;
}

// ─── MOOD 3 — Goldfish (1912) ────────────────────────────────────────
//  Orange-vermilion fish drifting in glass bowl, viewed top + side
//  simultaneously (Matisse's flattened multi-perspective).
vec3 moodGoldfish(vec2 uv, float t, float audio, float wild, float fScale) {
    // Background — Matisse's signature pink-rose interior wall
    vec3 col = vec3(0.94, 0.55, 0.50);

    // Green leafy frame at edges (the ferns)
    vec2 ep = uv;
    float leaves = 0.0;
    for (int i = 0; i < 5; i++) {
        float fi = float(i);
        vec2 lc = vec2(0.05 + 0.22 * fi, 0.05 + 0.18 * sin(fi * 2.0));
        vec2 lp = uv - lc;
        lp = rot(fi * 1.4) * lp;
        float lf = sdEllipse(lp, vec2(0.06, 0.018));
        leaves = max(leaves, 1.0 - smoothstep(0.0, 0.003, lf));
    }
    // mirror on other corner
    for (int i = 0; i < 5; i++) {
        float fi = float(i);
        vec2 lc = vec2(0.95 - 0.22 * fi, 0.95 - 0.18 * sin(fi * 1.7));
        vec2 lp = uv - lc;
        lp = rot(-fi * 1.1) * lp;
        float lf = sdEllipse(lp, vec2(0.05, 0.015));
        leaves = max(leaves, 1.0 - smoothstep(0.0, 0.003, lf));
    }
    col = mix(col, EMERALD, leaves);

    // The bowl — circular (top view) inset on the left,
    // and an ellipse (side view) on the right. Both contain fish.
    vec2 cTop = vec2(0.32, 0.50);
    vec2 cSide = vec2(0.72, 0.50);

    // Top-view bowl — a circle of pale water
    float topBowl = sdCircle(uv - cTop, 0.20 * fScale);
    float topMask = 1.0 - smoothstep(0.0, 0.003, topBowl);
    vec3 water = vec3(0.78, 0.92, 0.95);
    col = mix(col, water, topMask);
    // Bowl rim
    float topRim = abs(topBowl) - 0.005;
    col = mix(col, vec3(0.35, 0.55, 0.58), 1.0 - smoothstep(0.0, 0.002, topRim));

    // Side-view bowl — an upright ellipse (cylinder seen from side)
    vec2 sp = uv - cSide;
    float sideBowl = sdEllipse(sp, vec2(0.16, 0.22) * fScale);
    float sideMask = 1.0 - smoothstep(0.0, 0.003, sideBowl);
    col = mix(col, water, sideMask);
    float sideRim = abs(sideBowl) - 0.005;
    col = mix(col, vec3(0.35, 0.55, 0.58), 1.0 - smoothstep(0.0, 0.002, sideRim));

    // Fish in top-view bowl — circular drift
    for (int i = 0; i < 4; i++) {
        float fi = float(i);
        float a = t * (0.4 + audio * 0.3) + fi * 1.5707963;
        vec2 fp = cTop + vec2(cos(a), sin(a)) * 0.10;
        // fish body — small ellipse pointing along motion
        vec2 lp = uv - fp;
        lp = rot(-a + 1.5707963) * lp;
        float fish = sdEllipse(lp, vec2(0.030, 0.012));
        // Tail — triangle behind
        float tail = sdSegment(lp, vec2(-0.025, 0.0), vec2(-0.045, 0.015 * sin(t * 4.0 + fi)), 0.008);
        float fishMask = (1.0 - smoothstep(0.0, 0.002, min(fish, tail))) * topMask;
        col = mix(col, VERMILION, fishMask);
        // Tiny dark eye
        float eye = sdCircle(lp - vec2(0.018, 0.003), 0.003);
        col = mix(col, vec3(0.10, 0.05, 0.08), (1.0 - smoothstep(0.0, 0.0008, eye)) * topMask);
    }

    // Fish in side-view bowl — drifting horizontally
    for (int i = 0; i < 3; i++) {
        float fi = float(i);
        float dx = sin(t * 0.6 + fi * 2.1) * 0.08;
        float dy = (fi - 1.0) * 0.07 + 0.012 * sin(t + fi);
        vec2 fp = cSide + vec2(dx, dy);
        vec2 lp = uv - fp;
        // Flip by direction of motion
        float dir = sign(cos(t * 0.6 + fi * 2.1));
        lp.x *= dir;
        float fish = sdEllipse(lp, vec2(0.028, 0.011));
        float tail = sdSegment(lp, vec2(-0.024, 0.0), vec2(-0.040, 0.012 * sin(t * 3.0 + fi * 2.0)), 0.007);
        float fishMask = (1.0 - smoothstep(0.0, 0.002, min(fish, tail))) * sideMask;
        col = mix(col, ORANGE, fishMask * 0.7 + VERMILION.r * 0.0);
        // re-mix with vermilion for boldness
        col = mix(col, VERMILION, fishMask * 0.6);
        float eye = sdCircle(lp - vec2(0.017, 0.003), 0.0028);
        col = mix(col, vec3(0.10, 0.05, 0.08), (1.0 - smoothstep(0.0, 0.0008, eye)) * sideMask);
    }

    return col;
}

// ─── main ────────────────────────────────────────────────────────────
void main() {
    vec2 uv = isf_FragNormCoord.xy;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);

    // Aspect-correct so circles stay circles
    vec2 cuv = uv;
    cuv.x = (uv.x - 0.5) * aspect + 0.5;

    float gSpeed = clamp(gestureSpeed, 0.0, 2.0);
    float t = TIME * (0.6 + tempo) * gSpeed;

    // Soft-knee audio conditioning (playbook) — bass throws the tempo,
    // mids flush the color, energy breathes the whole canvas.
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
    float midP  = smoothstep(0.08, 0.85, audioMid);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float drive = 0.25 + 0.75 * smoothstep(0.05, 0.9, audioEnergy);

    // Live audio replaces the old constant: rests near 1.0 in silence,
    // swells with bass (structure) and mids (flush/detail).
    float audio = clamp(audioReact, 0.0, 2.0) * (0.80 + 0.35 * bassP + 0.15 * midP);
    float wild = clamp(wildness, 0.0, 1.5);
    float fScale = clamp(figureScale, 0.5, 2.0);

    int m = int(mood + 0.5);
    vec3 col;
    if (m == 0)      col = moodLaDanse(cuv, t, audio, wild, fScale);
    else if (m == 1) col = moodJazz(cuv, t, audio, wild, fScale);
    else if (m == 2) col = moodFemme(cuv, t, audio, wild, fScale);
    else             col = moodGoldfish(cuv, t, audio, wild, fScale);

    // Painter's tooth — coarse canvas grain, highs raise the sparkle
    float tooth = paintGrain(uv) * paintTooth * (1.0 + 0.35 * highP);
    col += tooth * 0.15;

    // Subtle audio breath — never flatten the silence; energy lifts the canvas
    col *= 0.95 + 0.10 * audio * (0.5 + 0.5 * sin(t * 1.4)) + 0.05 * (drive - 0.25);

    // Linear whole-frame follower + decaying beat accent — the bands are
    // pre-smoothed upstream, use them straight (no knee) so quieter broadband
    // styles (rock/jazz) still register.
    float ar = clamp(audioReact, 0.0, 1.5);
    // R3 chop fix: 0.22 bass + 0.20 kick whole-frame gains stacked to 0.14
    // single-frame steps on EDM (ratio 6.1, just over the 6.0 gate). Trim the
    // instant-attack kick hard and the bass follower a little; mids unchanged.
    col *= 1.0 + (0.17 * audioBass + 0.14 * audioMid) * ar;
    float kick = pow(max(audioBeatPulse, 0.8 * audioPunch), 1.3);
    col *= 1.0 + 0.09 * kick * ar;

    // LINEAR HDR — keep saturation high; host applies tonemap
    col = max(col, vec3(0.0));

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = col;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                   // saturation
    if (hueShift > 0.0005) {                               // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    // background = darkest end of the canvas (full-field painting, no void)
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

    gl_FragColor = vec4(uc, 1.0);
}
