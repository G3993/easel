/*{
  "DESCRIPTION": "Image-Drag — three procedural picture-cutouts smear into staircased trails as if a hand dragged them across the canvas. Each cutout is its own little world (horizon, figure, foreground) sampled along a trail vector and stamped N times with parallax z and decaying alpha, so the smear reads as motion not blur. Three stacks = three players: each stack has its own drag vector, drag length and depth driven by player[i].energy/active. Live caption typewriters in from cue.latest above the drag field. Bass deepens the smear, treble jitters the stamps. Anti-pattern free — no checker, no bars, no spectrum, no horizon symmetry.",
  "CREDIT": "ShaderClaw — A-List drop, after Roope Rainisto / madebysix",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",         "LABEL": "Caption",       "TYPE": "text",  "DEFAULT": "POST PHOTOGRAPHIC PERSPECTIVES", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "energyA",     "LABEL": "P1 Energy",     "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",     "LABEL": "P2 Energy",     "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",     "LABEL": "P3 Energy",     "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "activeA",     "LABEL": "P1 Active",     "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].active" },
    { "NAME": "activeB",     "LABEL": "P2 Active",     "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].active" },

    { "NAME": "bassDrive",   "LABEL": "Bass Drive",    "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.bass" },
    { "NAME": "trebleDrive", "LABEL": "Treble Jitter", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.high" },

    { "NAME": "dragLength",  "LABEL": "Drag Length",   "TYPE": "float", "DEFAULT": 0.65, "MIN": 0.10, "MAX": 1.20 },
    { "NAME": "imageCount",  "LABEL": "Stamp Count",   "TYPE": "long",  "DEFAULT": 9,    "VALUES": [4,6,8,9,10,12,14], "LABELS": ["4","6","8","9","10","12","14"] },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "DEFAULT": 0.35, "MIN": 0.0,  "MAX": 1.5 },
    { "NAME": "audioDepth",  "LABEL": "Audio Depth",   "TYPE": "float", "DEFAULT": 0.75, "MIN": 0.0,  "MAX": 2.0 },
    { "NAME": "trailDecay",  "LABEL": "Trail Decay",   "TYPE": "float", "DEFAULT": 0.62, "MIN": 0.20, "MAX": 0.95 },
    { "NAME": "depthAmount", "LABEL": "Z Parallax",    "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0,  "MAX": 1.5 },

    { "NAME": "paperColor",  "LABEL": "Paper",         "TYPE": "color", "DEFAULT": [0.06, 0.06, 0.07, 1.0] },
    { "NAME": "skyColor",    "LABEL": "Sky Hue",       "TYPE": "color", "DEFAULT": [0.62, 0.74, 0.82, 1.0] },
    { "NAME": "groundColor", "LABEL": "Ground Hue",    "TYPE": "color", "DEFAULT": [0.18, 0.36, 0.16, 1.0] },
    { "NAME": "accentColor", "LABEL": "Figure Accent", "TYPE": "color", "DEFAULT": [0.86, 0.18, 0.20, 1.0] },
    { "NAME": "inkColor",    "LABEL": "Caption Ink",   "TYPE": "color", "DEFAULT": [0.95, 0.94, 0.91, 1.0] },

    { "NAME": "kerning",     "LABEL": "Kerning",       "TYPE": "float", "DEFAULT": 0.92, "MIN": 0.55, "MAX": 1.4 },
    { "NAME": "labelScale",  "LABEL": "Caption Size",  "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.5,  "MAX": 2.0 }
  ]
}*/

// =====================================================================
// Image-Drag — three cutouts smear into trails. Each cutout is a tiny
// procedural picture (sky / horizon / figure / foreground vignette).
// We stamp it N times along a drag vector with decaying alpha and a
// per-stamp z so the trail reads as space, not blur. Three stacks, one
// per player. Trail vector + length + stamp count + jitter all
// energy-aware so silence = stillness, talk = real drag.
// =====================================================================

#define MAX_STAMPS 14
#define MAX_WALK   48
#define SPACE_CH   26
#define TAU 6.28318530718

// ─── Font atlas ─────────────────────────────────────────────────────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}

int getChar(int slot) {
    if (slot ==  0) return int(msg_0);
    if (slot ==  1) return int(msg_1);
    if (slot ==  2) return int(msg_2);
    if (slot ==  3) return int(msg_3);
    if (slot ==  4) return int(msg_4);
    if (slot ==  5) return int(msg_5);
    if (slot ==  6) return int(msg_6);
    if (slot ==  7) return int(msg_7);
    if (slot ==  8) return int(msg_8);
    if (slot ==  9) return int(msg_9);
    if (slot == 10) return int(msg_10);
    if (slot == 11) return int(msg_11);
    if (slot == 12) return int(msg_12);
    if (slot == 13) return int(msg_13);
    if (slot == 14) return int(msg_14);
    if (slot == 15) return int(msg_15);
    if (slot == 16) return int(msg_16);
    if (slot == 17) return int(msg_17);
    if (slot == 18) return int(msg_18);
    if (slot == 19) return int(msg_19);
    if (slot == 20) return int(msg_20);
    if (slot == 21) return int(msg_21);
    if (slot == 22) return int(msg_22);
    if (slot == 23) return int(msg_23);
    if (slot == 24) return int(msg_24);
    if (slot == 25) return int(msg_25);
    if (slot == 26) return int(msg_26);
    if (slot == 27) return int(msg_27);
    if (slot == 28) return int(msg_28);
    if (slot == 29) return int(msg_29);
    if (slot == 30) return int(msg_30);
    if (slot == 31) return int(msg_31);
    if (slot == 32) return int(msg_32);
    if (slot == 33) return int(msg_33);
    if (slot == 34) return int(msg_34);
    if (slot == 35) return int(msg_35);
    if (slot == 36) return int(msg_36);
    if (slot == 37) return int(msg_37);
    if (slot == 38) return int(msg_38);
    if (slot == 39) return int(msg_39);
    if (slot == 40) return int(msg_40);
    if (slot == 41) return int(msg_41);
    if (slot == 42) return int(msg_42);
    if (slot == 43) return int(msg_43);
    if (slot == 44) return int(msg_44);
    if (slot == 45) return int(msg_45);
    if (slot == 46) return int(msg_46);
    if (slot == 47) return int(msg_47);
    return -1;
}
int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 0;
    if (n > 48) return 48;
    return n;
}

// ─── Noise utility ──────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  hash21(float n) { return vec2(hash11(n), hash11(n + 17.31)); }
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash11(dot(i, vec2(1.0, 157.0)));
    float b = hash11(dot(i + vec2(1.0, 0.0), vec2(1.0, 157.0)));
    float c = hash11(dot(i + vec2(0.0, 1.0), vec2(1.0, 157.0)));
    float d = hash11(dot(i + vec2(1.0, 1.0), vec2(1.0, 157.0)));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}
float fbm2(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.07 + vec2(11.3, 5.7);
        a *= 0.5;
    }
    return v;
}

// ─── Procedural cutout content ──────────────────────────────────────
// Each cutout has its own "world" — a horizon scene drawn into a unit
// square in local coords lp ∈ [-0.5, 0.5]^2. variant ∈ {0,1,2} picks
// which little world. Returns (rgb, mask) where mask=1 inside the
// picture rectangle, ~0 outside (with a soft photographic edge).
vec4 cutout(vec2 lp, float variant, float t, float energy) {
    // Rectangular crop with a soft photographic vignette edge.
    vec2 ap = abs(lp);
    if (ap.x > 0.5 || ap.y > 0.5) return vec4(0.0);
    float edge = (0.5 - max(ap.x, ap.y));
    float mask = smoothstep(0.0, 0.018, edge);

    // Sky / ground split with a wandering horizon (variant adjusts height).
    float horizon = mix(-0.05, 0.18, variant * 0.5);
    horizon += 0.04 * sin(lp.x * 4.2 + variant * 2.1 + t * 0.3);
    float skyMask = smoothstep(horizon + 0.01, horizon - 0.01, lp.y);

    // Sky — gradient + drifting cloud noise + subtle warmth at horizon.
    float skyGrad = smoothstep(-0.6, 0.5, lp.y);
    vec3 skyTop   = skyColor.rgb * (0.85 + 0.25 * skyGrad);
    float cloud   = fbm2(lp * vec2(3.4, 2.1) + vec2(t * 0.04 + variant, variant * 7.3));
    cloud = smoothstep(0.45, 0.85, cloud);
    vec3 cloudCol = mix(skyTop, vec3(0.97, 0.95, 0.90), 0.55);
    vec3 sky = mix(skyTop, cloudCol, cloud * 0.85);
    // Warm horizon ribbon.
    sky = mix(sky, vec3(0.97, 0.78, 0.58),
              smoothstep(0.04, 0.0, abs(lp.y - horizon)) * 0.35);

    // Ground — banded fbm "hedge / field" texture, darker at base.
    float gn  = fbm2(lp * vec2(5.0, 9.0) + vec2(variant * 11.0, t * 0.05));
    float gn2 = fbm2(lp * vec2(13.0, 26.0) + vec2(3.1, variant));
    vec3 ground = groundColor.rgb;
    // Vertical stripe pattern for the hedge feel (variant 0).
    if (variant < 0.5) {
        float stripes = 0.5 + 0.5 * sin(lp.x * 90.0 + gn * 6.0);
        ground = mix(ground * 0.55, ground * 1.15, stripes);
        ground *= 0.7 + 0.3 * gn2;
    } else if (variant < 1.5) {
        // Lawn / textured field variant.
        ground = mix(ground * 0.6, vec3(0.55, 0.62, 0.32),
                     smoothstep(0.25, 0.85, gn));
        ground *= 0.85 + 0.2 * gn2;
    } else {
        // Path / gravel variant.
        ground = mix(vec3(0.36, 0.32, 0.26), vec3(0.78, 0.74, 0.66),
                     smoothstep(0.2, 0.9, gn));
        ground = mix(ground, groundColor.rgb,
                     smoothstep(-0.3, 0.0, lp.y - horizon));
    }
    // Depth attenuation — far ground sits lighter / hazier.
    ground = mix(ground, sky, smoothstep(horizon, horizon + 0.15, lp.y) * 0.55);

    vec3 scene = mix(ground, sky, skyMask);

    // A small architectural mass on variant 2 (the "house" beat).
    if (variant > 1.5) {
        vec2 h = lp - vec2(0.18, horizon + 0.08);
        float house = max(abs(h.x) - 0.07, abs(h.y) - 0.07);
        float roof  = (h.y - 0.07) + 0.7 * abs(h.x);
        house = max(house, -roof);
        float hmask = smoothstep(0.004, -0.004, house);
        vec3 wall = mix(vec3(0.92, 0.90, 0.86), vec3(0.74, 0.70, 0.64),
                        smoothstep(0.0, 0.07, h.y));
        scene = mix(scene, wall, hmask);
        // Tiny window.
        float win = max(abs(h.x + 0.01) - 0.012, abs(h.y - 0.02) - 0.018);
        scene = mix(scene, vec3(0.10, 0.12, 0.14),
                    smoothstep(0.002, -0.002, win));
    }

    // Figure — small standing accent shape near foreground, pulses with energy.
    {
        vec2 f = lp - vec2(-0.04 + 0.08 * variant, horizon + 0.06);
        float body = length(vec2(f.x * 4.5, (f.y + 0.06) * 1.8)) - 0.05;
        float head = length(f - vec2(0.0, 0.02)) - 0.018;
        float fig  = min(body, head);
        float fmask = smoothstep(0.003, -0.003, fig);
        // Bias figure visibility by player energy so quieter players have
        // ghostlier figures — the human is on the edge of the smear.
        vec3 figCol = mix(accentColor.rgb * 0.6,
                          accentColor.rgb * (1.0 + 0.4 * energy), energy);
        scene = mix(scene, figCol, fmask * (0.55 + 0.45 * energy));
    }

    // Photographic grain so the cutouts read as printed images, not gradients.
    float grain = vnoise(lp * 320.0 + variant * 31.0 + t * 2.3) - 0.5;
    scene *= 1.0 + grain * 0.06;

    // Vignette inside the photo.
    scene *= 1.0 - 0.18 * dot(lp, lp);

    return vec4(scene, mask);
}

// Sample one of three drag stacks. Returns premultiplied (rgb, alpha)
// after laying down `stamps` copies of `cutout(variant)` along a trail
// vector, with decaying alpha and a per-stamp z used for parallax.
vec4 dragStack(vec2 p, vec2 origin, vec2 dir, float trailLen,
               int stamps, float decay, float zAmount, float variant,
               float energy, float jitter, float t) {
    // Per-cutout size — slightly anisotropic so the staircase reads.
    vec2 size = vec2(0.34, 0.28) * (0.85 + 0.25 * variant);

    vec3 accumCol = vec3(0.0);
    float accumA  = 0.0;
    // Iterate front (newest) to back (oldest) so newer stamps occlude
    // older ones — gives the staircase its overlapping read.
    for (int i = 0; i < MAX_STAMPS; i++) {
        if (i >= stamps) break;
        float fi  = float(i);
        float u   = fi / float(max(stamps - 1, 1));   // 0 = front, 1 = back
        // Stamp offset along the drag direction.
        vec2 off  = dir * trailLen * u;
        // Tiny per-stamp jitter (treble-driven) — never enough to break the staircase.
        vec2 jit  = (hash21(fi + variant * 13.0) - 0.5) * 0.015 * jitter;
        off += jit;
        // Per-stamp z — back stamps sit further away → smaller, lifted up.
        float z = u * zAmount;
        vec2 center = origin + off + vec2(0.0, z * 0.06);
        float scl   = 1.0 - z * 0.18;

        // Local coords inside the cutout.
        vec2 lp = (p - center) / (size * scl);

        vec4 c = cutout(lp, variant, t + fi * 0.7, energy);
        if (c.a < 0.001) continue;

        // Trail alpha decay — front stamps opaque, back stamps faded.
        float aFade = pow(decay, fi);
        // Depth desaturation so back stamps melt into atmosphere.
        c.rgb = mix(c.rgb, paperColor.rgb, z * 0.45);
        // Soft shadow shelf below each stamp (sub-pixel detail).
        c.rgb *= 1.0 - 0.18 * smoothstep(0.49, 0.5, abs(lp.y));

        float a = c.a * aFade * (0.55 + 0.45 * energy);
        // Front-to-back composite (premultiplied).
        float oneMinus = 1.0 - accumA;
        accumCol += c.rgb * a * oneMinus;
        accumA   += a * oneMinus;
        if (accumA > 0.995) break;
    }
    return vec4(accumCol, accumA);
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;
    vec2 p = vec2((uv.x - 0.5) * aspect, uv.y - 0.5);

    float t = TIME * (0.4 + motionSpeed * 1.2);

    // Rest energies — never quite zero so the piece breathes.
    float eA = clamp(max(energyA, 0.08) * (0.55 + 0.6 * activeA) * audioDepth, 0.0, 1.5);
    float eB = clamp(max(energyB, 0.08) * (0.55 + 0.6 * activeB) * audioDepth, 0.0, 1.5);
    float eC = clamp(max(energyC, 0.08) * audioDepth, 0.0, 1.5);
    float bass = bassDrive;
    float jit  = trebleDrive;

    // ── Backdrop: warm/dark paper with a subtle paper-fibre + a wandering raking light.
    vec3 col = paperColor.rgb;
    float fibre = fbm2(uv * vec2(res.x, res.y) * 0.012);
    col *= 0.94 + 0.12 * fibre;
    float sweep = smoothstep(0.0, 0.5, sin(p.x * 0.9 - p.y * 0.4 + t * 0.22) * 0.5 + 0.5);
    col += pow(sweep, 4.0) * 0.04 * vec3(1.0, 0.94, 0.82);
    col *= 1.0 - 0.20 * dot(p, p);

    // ── Three drag stacks. Origins fan across the canvas; directions
    //    diverge per stack so the smears don't read as one parallel comb.
    //    Each stack's drag length grows with its own player's energy.
    int stamps = int(imageCount);
    if (stamps > MAX_STAMPS) stamps = MAX_STAMPS;
    if (stamps < 2) stamps = 2;

    float lenA = dragLength * (0.55 + 0.55 * eA + 0.30 * bass);
    float lenB = dragLength * (0.45 + 0.65 * eB + 0.30 * bass);
    float lenC = dragLength * (0.50 + 0.55 * eC + 0.30 * bass);

    // Wandering origins — slow drift so the composition lives.
    vec2 oA = vec2(-0.55 * aspect, -0.18) + vec2(sin(t * 0.21) * 0.04, cos(t * 0.18) * 0.03);
    vec2 oB = vec2(-0.10 * aspect, -0.30) + vec2(cos(t * 0.17) * 0.05, sin(t * 0.24) * 0.04);
    vec2 oC = vec2( 0.30 * aspect,  0.05) + vec2(sin(t * 0.15 + 1.7) * 0.05, cos(t * 0.19 + 1.1) * 0.03);

    // Drag directions — slightly diagonal "down-right" like the reference,
    // but each stack picks its own angle and energy nudges the angle so
    // active players smear at a steeper rake.
    float angA = -0.35 + 0.18 * sin(t * 0.13) + 0.22 * eA;
    float angB = -0.42 + 0.15 * sin(t * 0.11 + 1.3) - 0.12 * eB;
    float angC = -0.50 + 0.12 * cos(t * 0.16) + 0.28 * eC;
    vec2 dA = vec2(cos(angA), sin(angA));
    vec2 dB = vec2(cos(angB), sin(angB));
    vec2 dC = vec2(cos(angC), sin(angC));

    // Per-stack z amount — front stack flat, back stack deeper.
    float zA = depthAmount * 0.55;
    float zB = depthAmount * 0.85;
    float zC = depthAmount * 1.15;

    // Composite back-to-front so closer stacks occlude farther ones.
    // Stack C lives deepest, A frontmost.
    vec4 sC = dragStack(p, oC, dC, lenC, stamps, trailDecay, zC, 2.0, eC, jit, t);
    vec4 sB = dragStack(p, oB, dB, lenB, stamps, trailDecay, zB, 1.0, eB, jit, t);
    vec4 sA = dragStack(p, oA, dA, lenA, stamps, trailDecay, zA, 0.0, eA, jit, t);

    col = mix(col, sC.rgb / max(sC.a, 1e-3), sC.a);
    col = mix(col, sB.rgb / max(sB.a, 1e-3), sB.a);
    col = mix(col, sA.rgb / max(sA.a, 1e-3), sA.a);

    // Soft motion shadow underneath every stack (extra depth read).
    float shelf = smoothstep(0.0, 0.04, -p.y - 0.30);
    col *= 1.0 - 0.10 * shelf;

    // ── Caption — typewriter from cue.latest, sits in the upper third
    //    like the reference's editorial title. Plain horizontal layout.
    int total = charCount();
    if (total > 0) {
        // Top-band text box: x ∈ [-0.55a, +0.55a], y ∈ [0.25, 0.43].
        float boxYTop    = 0.43;
        float boxYBot    = 0.25;
        float boxXLeft   = -0.55 * aspect;
        float boxXRight  =  0.55 * aspect;
        float boxW       = boxXRight - boxXLeft;
        float boxH       = boxYTop - boxYBot;
        // Glyph size driven by scale slider.
        float lScale  = clamp(labelScale, 0.5, 2.0);
        float charH   = 0.052 * lScale;
        float charW   = charH * (5.0 / 7.0);
        float kern    = charW * clamp(kerning, 0.55, 1.4);
        // How many glyphs fit per row.
        int cols = int(floor(boxW / max(kern, 1e-4)));
        if (cols < 1) cols = 1;
        if (cols > 48) cols = 48;
        int rows = (total + cols - 1) / cols;
        if (rows < 1) rows = 1;
        float blockH = float(rows) * (charH * 1.18);
        if (blockH > boxH) {
            float s = boxH / blockH;
            charH *= s; charW *= s; kern *= s; blockH = boxH;
        }
        float lineP  = charH * 1.18;
        float yStart = (boxYTop + boxYBot) * 0.5 + blockH * 0.5;

        // Pixel space inside box.
        float lx = p.x - boxXLeft;
        float ly = yStart - p.y;
        if (lx >= 0.0 && lx <= boxW && ly >= 0.0 && ly <= blockH) {
            int targetRow = int(floor(ly / lineP));
            float yInRow  = ly - float(targetRow) * lineP;
            int targetCol = int(floor(lx / kern));
            float colPad  = (kern - charW) * 0.5;
            float rowPad  = (lineP - charH) * 0.5;
            if (targetCol < cols && targetRow < rows
                && yInRow >= rowPad && yInRow <= rowPad + charH) {
                int idx = targetRow * cols + targetCol;
                if (idx < total) {
                    int ch = getChar(idx);
                    if (ch >= 0 && ch <= 35 && ch != SPACE_CH) {
                        vec2 cellLocal = vec2(
                            (lx - float(targetCol) * kern - colPad) / charW,
                            1.0 - (yInRow - rowPad) / charH);
                        float s = sampleChar(ch, cellLocal);
                        s = smoothstep(0.18, 0.55, s);
                        if (s > 0.001) {
                            // Caption ink — solid + tiny drag echo (subtler than the
                            // images, so the type reads as a layer on top, not part
                            // of the smear).
                            vec3 ink = inkColor.rgb;
                            col = mix(col, ink, s);
                            // Faint echo behind the glyph in the drag direction.
                            float echo = sampleChar(ch, cellLocal + vec2(0.18, 0.0));
                            echo = smoothstep(0.30, 0.65, echo);
                            col = mix(col, ink * 0.55, echo * 0.25);
                        }
                    }
                }
            }
        }
    }

    // Final tonemap / gentle filmic shoulder so the dark paper holds.
    col = col / (1.0 + 0.45 * col);
    col = pow(max(col, 0.0), vec3(0.94));

    gl_FragColor = vec4(col, 1.0);
}
