/*{
  "DESCRIPTION": "Circle Text — a slow gallery halo. A ring of ~50 tilted micro-cards orbits a quiet serif center; each card holds one character of the cue, revealed in sequence as you speak. Three concentric shells (inner/middle/outer) breathe with three separate player channels; the ring itself widens and warps with bass. Real depth: cards rotate on their own axes in 3D as they travel the orbit, with perspective foreshortening and a soft depth haze. Warm paper backdrop, watercolor sun, gentle aura behind the spoken message. The reference is the Whispers art-game ring — composition felt as halo, not literal collage.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "TYPE": "text", "DEFAULT": "WHISPERS AROUND THE RING", "MAX_LENGTH": 48 },

    { "NAME": "innerEnergy", "LABEL": "Inner Shell Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "midEnergy",   "LABEL": "Mid Shell Energy",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "outerEnergy", "LABEL": "Outer Shell Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].energy" },

    { "NAME": "ringRadius",  "LABEL": "Ring Radius",        "TYPE": "float", "MIN": 0.18, "MAX": 0.46, "DEFAULT": 0.34 },
    { "NAME": "ringSpin",    "LABEL": "Ring Spin",          "TYPE": "float", "MIN": -1.5, "MAX": 1.5, "DEFAULT": 0.18 },
    { "NAME": "cardSize",    "LABEL": "Card Size",          "TYPE": "float", "MIN": 0.4, "MAX": 1.8, "DEFAULT": 1.0 },
    { "NAME": "tiltAmp",     "LABEL": "3D Tilt",            "TYPE": "float", "MIN": 0.0, "MAX": 1.2, "DEFAULT": 0.6 },
    { "NAME": "textSize",    "LABEL": "Text Size",          "TYPE": "float", "MIN": 0.5, "MAX": 1.8, "DEFAULT": 1.0 },

    { "NAME": "audioDepth",  "LABEL": "Bass → Halo Breath", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.8 },
    { "NAME": "paperWarm",   "LABEL": "Paper Warmth",       "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 1.0 },

    { "NAME": "paperColor",  "LABEL": "Paper",              "TYPE": "color", "DEFAULT": [0.965, 0.955, 0.935, 1.0] },
    { "NAME": "inkColor",    "LABEL": "Ink",                "TYPE": "color", "DEFAULT": [0.10, 0.09, 0.13, 1.0] },
    { "NAME": "accentA",     "LABEL": "Accent A (Inner)",   "TYPE": "color", "DEFAULT": [0.92, 0.42, 0.28, 1.0] },
    { "NAME": "accentB",     "LABEL": "Accent B (Mid)",     "TYPE": "color", "DEFAULT": [0.30, 0.55, 0.92, 1.0] },
    { "NAME": "accentC",     "LABEL": "Accent C (Outer)",   "TYPE": "color", "DEFAULT": [0.55, 0.78, 0.42, 1.0] }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  CIRCLE TEXT  ·  Whispers halo · 50 tilted cards · 3 player shells
//
//  Composition: three concentric rings of micro-cards orbit a quiet
//  serif center. Inner = warm/red shell  (player[1]), middle = cool/blue
//  shell (player[2]), outer = soft green shell (player[3]). Each card
//  rotates on its own axis as it travels — real 3D billboard tilt with
//  perspective foreshortening (cards on the far side shrink + dim).
//
//  Text: each character of `msg` lands inside ONE card on the middle
//  ring; characters appear in sequence using msgAge (typewriter). Cards
//  past the typed length show abstract micro-paintings (procedural
//  palette swatches) — like the unfilled tiles in the reference.
// ════════════════════════════════════════════════════════════════════════

const float TAU = 6.28318530718;
const float PI  = 3.14159265359;

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

// ─── Hash / noise ───────────────────────────────────────────────────
float h11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  h21(float n) { return vec2(h11(n), h11(n + 17.31)); }
vec3  h31(float n) { return vec3(h11(n), h11(n + 5.7), h11(n + 13.1)); }

float vnoise2(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = h11(dot(i, vec2(1.0, 157.0)));
    float b = h11(dot(i + vec2(1.0, 0.0), vec2(1.0, 157.0)));
    float c = h11(dot(i + vec2(0.0, 1.0), vec2(1.0, 157.0)));
    float d = h11(dot(i + vec2(1.0, 1.0), vec2(1.0, 157.0)));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}
float fbm2(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise2(p);
        p  = p * 2.03 + vec2(11.3, 5.7);
        a *= 0.5;
    }
    return v;
}

// Rotated SDF for a card (rounded rectangle, centered, rotated by angle)
float sdRoundedBox(vec2 p, vec2 b, float r) {
    vec2 q = abs(p) - b + vec2(r);
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
}

// Procedural micro-painting fill: a few overlapping color blobs per card.
// Each card-seed picks its own tiny palette + composition. NOT literal —
// reads as "abstract painting" the way the reference's tiles do.
vec3 cardArt(vec2 uvCard, float seed, vec3 accent, float drift) {
    // uvCard in [-1,1], centered. seed deterministic per card index.
    vec3 paletteA = mix(accent, vec3(0.95, 0.92, 0.86), 0.35 + 0.5 * h11(seed * 3.7));
    vec3 paletteB = mix(accent.bgr, vec3(0.92, 0.86, 0.78), 0.4 + 0.4 * h11(seed * 5.1));
    vec3 paletteC = mix(vec3(0.18, 0.16, 0.20), accent, 0.3 + 0.6 * h11(seed * 7.3));

    // Three soft blobs at hashed positions
    vec3 col = mix(paletteA, paletteB, 0.5 + 0.5 * sin(uvCard.x * 2.1 + seed));
    for (int b = 0; b < 3; b++) {
        float fb = float(b);
        vec2 bp = (h21(seed * 11.0 + fb * 4.1) - 0.5) * 1.6
                + 0.18 * vec2(sin(drift + seed + fb), cos(drift * 0.8 + seed * 1.3 + fb));
        float br = 0.35 + 0.45 * h11(seed * 13.0 + fb);
        float bd = length(uvCard - bp) / br;
        float w  = exp(-bd * bd * 2.2);
        vec3  bc = (b == 0) ? paletteA : (b == 1 ? paletteB : paletteC);
        col = mix(col, bc, w * 0.65);
    }
    // Subtle painterly grain
    float g = fbm2(uvCard * 3.5 + seed) - 0.5;
    col += g * 0.06;
    return clamp(col, 0.0, 1.0);
}

// Map a global card index (0..N-1) to its shell (0,1,2) and per-shell slot.
// Inner shell = 14 cards, mid = 18 cards (the text shell), outer = 18 cards.
// Total = 50, matching the reference's ~50-tile ring.
void cardLayout(int idx, out int shell, out int slot, out int shellN) {
    if (idx < 14) {
        shell = 0; slot = idx; shellN = 14;
    } else if (idx < 14 + 18) {
        shell = 1; slot = idx - 14; shellN = 18;
    } else {
        shell = 2; slot = idx - 32; shellN = 18;
    }
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 fragUV = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    // Centered, aspect-corrected, [-0.5..0.5] vertically, wider horizontally
    vec2 p;
    p.x = (fragUV.x - 0.5) * aspect;
    p.y = fragUV.y - 0.5;

    float t = TIME;
    float bass = clamp(audioBass, 0.0, 1.0);
    float lvl  = clamp(audioLevel, 0.0, 1.0);

    // ───── Paper backdrop (warm gallery) ─────────────────────────────
    float wmarble = fbm2(p * 2.2 + vec2(t * 0.04, -t * 0.03));
    vec3 paper = paperColor.rgb;
    paper = mix(paper, paper * vec3(1.04, 1.01, 0.97), wmarble * paperWarm);
    // Soft warm sun toward top-right echoes the reference's daylight.
    float sun = exp(-length(p - vec2(0.18, 0.22)) * 1.6);
    paper += sun * vec3(0.05, 0.03, -0.02) * paperWarm;
    // Vignette
    paper *= 1.0 - 0.10 * dot(p, p);

    vec3 col = paper;

    // ───── Halo / aura behind the ring (cue-driven glow) ─────────────
    float baseR = ringRadius * (1.0 + 0.05 * sin(t * 0.31))
                + 0.04 * bass * audioDepth;
    float rDist = length(p);
    // Diffuse ring glow — wider with talking, faint when silent.
    float talk = clamp((innerEnergy + midEnergy + outerEnergy) / 3.0, 0.0, 1.0);
    float aura = exp(-pow((rDist - baseR) / (0.10 + 0.07 * talk), 2.0));
    vec3 auraTint = mix(vec3(1.0, 0.94, 0.86), accentB.rgb, 0.25 + 0.4 * midEnergy);
    col += aura * auraTint * (0.06 + 0.18 * talk);

    // ───── Center text aura (the serif-text quiet middle) ────────────
    // A small breathing dot of light where the message would sit.
    float center = exp(-rDist * rDist * 38.0);
    col = mix(col, col * 1.04 + accentA.rgb * 0.04, center * (0.4 + 0.6 * lvl));

    // ───── The ring of cards ─────────────────────────────────────────
    // 50 cards total across 3 shells.
    const int TOTAL_CARDS = 50;
    int total = charCount();
    bool liveUtterance = msgAge >= 0.0;

    // How many characters have "arrived" (typewriter reveal). When msgAge<0
    // (manual entry), reveal everything at once.
    int revealed;
    if (liveUtterance) {
        // ~28 chars/sec to match Easel's typewriter cps
        float cps = 28.0;
        int est = int(floor(msgAge * cps));
        if (est < 0) est = 0;
        if (est > total) est = total;
        revealed = est;
    } else {
        revealed = total;
    }

    // Best-z resolution: we composite cards back-to-front by sorting in z.
    // For each pixel we evaluate all cards and keep the closest-to-camera
    // hit. Z comes from a fake perspective (sin of orbit angle around y).
    float bestZ = -1e6;
    vec3  bestCol = vec3(0.0);
    float bestCov = 0.0;
    vec3  bestEdge = vec3(0.0);
    float bestShadow = 0.0;
    float bestShadowD = 1e6;

    for (int i = 0; i < TOTAL_CARDS; i++) {
        int shell, slot, shellN;
        cardLayout(i, shell, slot, shellN);
        float fSlot = float(slot);
        float fN    = float(shellN);

        // Shell properties
        float rShell, rotSpeed, sizeScale, tiltOff;
        vec3  shellAccent;
        float shellEnergy;
        if (shell == 0) {
            rShell      = baseR * 0.78;
            rotSpeed    = ringSpin * 1.15;
            sizeScale   = 0.85;
            tiltOff     = 0.0;
            shellAccent = accentA.rgb;
            shellEnergy = innerEnergy;
        } else if (shell == 1) {
            rShell      = baseR * 1.00;
            rotSpeed    = ringSpin * 1.00;
            sizeScale   = 1.00;
            tiltOff     = 1.7;
            shellAccent = accentB.rgb;
            shellEnergy = midEnergy;
        } else {
            rShell      = baseR * 1.22;
            rotSpeed    = ringSpin * 0.85;
            sizeScale   = 0.90;
            tiltOff     = 3.4;
            shellAccent = accentC.rgb;
            shellEnergy = outerEnergy;
        }

        // Per-card angle around the ring (with global spin)
        float ang = (fSlot / fN) * TAU + t * rotSpeed * 0.6;

        // Per-card jitter — driven by THIS shell's energy. Silencing
        // player[1] freezes the inner shell visibly; talking on player[2]
        // wobbles only the middle shell, etc.
        float jit = shellEnergy;
        ang += jit * 0.12 * sin(t * 1.7 + fSlot * 0.9);
        float rJit = rShell + jit * 0.02 * sin(t * 2.3 + fSlot * 1.3);

        // Card center in 2D
        vec2 cp = vec2(cos(ang), sin(ang)) * rJit;

        // Fake-3D depth: cards on the far side of the orbit are "behind"
        // the center. We model the ring as tilted toward camera by ~25°
        // around its X axis, so y of cp gets foreshortened.
        float tilt = 0.30 + 0.10 * sin(t * 0.13);
        cp.y *= (1.0 - tilt * 0.35);
        // Per-card depth — sin(ang) chooses front/back
        float z = sin(ang) * tilt;       // -tilt .. +tilt
        // Perspective scale: closer (positive z) → bigger, further → smaller
        float perspScale = 1.0 + z * 0.55;

        // Each card spins on its own axis as it travels (real billboard
        // tilt). This rotates the card's *content* relative to its frame.
        float spin = ang * 0.5 + t * 0.4 + tiltOff + h11(fSlot * 7.0 + float(shell) * 11.0) * TAU;
        float ca = cos(spin), sa = sin(spin);

        // Card half-size — small tiles, like the reference. CardSize
        // user input + per-shell scale + perspective + audio breathing.
        float halfW = 0.022 * cardSize * sizeScale * perspScale
                      * (1.0 + 0.05 * jit);
        float halfH = halfW * 1.30;   // portrait-ish tiles, matches ref

        // Apply a tiny non-symmetric tilt for the 3D effect
        float skew = z * tiltAmp * 0.45;
        vec2 lp = p - cp;
        // Rotate into card-local frame
        vec2 rp = vec2(ca * lp.x + sa * lp.y, -sa * lp.x + ca * lp.y);
        // Apply tilt shear (fake foreshortening of the card itself)
        rp.x += skew * rp.y;
        rp.y *= 1.0 - abs(z) * 0.18 * tiltAmp;

        // SDF for this card
        float d = sdRoundedBox(rp, vec2(halfW, halfH), halfW * 0.18);

        // Soft shadow track underneath each card — depth cue
        float shadowD = sdRoundedBox(rp - vec2(0.0, -halfH * 0.30),
                                     vec2(halfW * 1.05, halfH * 1.02),
                                     halfW * 0.25);

        // Coverage (antialiased)
        float fw = max(fwidth(d), 1e-4);
        float cov = 1.0 - smoothstep(-fw, fw, d);
        if (cov < 0.002 && shadowD > halfW * 0.8) continue;

        // Determine whether this card carries a character.
        // Only the MIDDLE shell (shell==1) shows glyphs. Slot 0..17 maps
        // to message char 0..17 (clipped to total).
        bool isTextCard = (shell == 1) && (slot < total);
        bool revealedNow = isTextCard && (slot < revealed);

        // ── Per-card content ──
        vec3 cardCol;
        vec3 edgeCol;

        if (isTextCard && revealedNow) {
            // White card with serif-ish ink character (gallery card)
            cardCol = vec3(0.985, 0.975, 0.955);
            // UV inside card, in [-1,1]
            vec2 cuv = vec2(rp.x / halfW, rp.y / halfH);
            // Map to font atlas cell [0,1]² centered
            // Glyph size relative to card; user textSize scales it.
            float glyphScale = 0.78 * textSize;
            vec2 guv = (cuv / glyphScale) * 0.5 + 0.5;
            int ch = getChar(slot);
            // Flip V (atlas convention vs screen)
            guv.y = 1.0 - guv.y;
            float s = sampleChar(ch, guv);
            s = smoothstep(0.25, 0.55, s);
            cardCol = mix(cardCol, inkColor.rgb, s);
            edgeCol = vec3(0.18, 0.16, 0.20);
        } else if (isTextCard && !revealedNow) {
            // Pre-reveal card: blank paper-tile, faintly tinted by mid accent
            cardCol = mix(vec3(0.96, 0.95, 0.93), accentB.rgb, 0.06);
            edgeCol = vec3(0.30, 0.27, 0.30);
        } else {
            // Non-text card: procedural micro-painting tile
            vec2 cuv = vec2(rp.x / halfW, rp.y / halfH);
            float seed = float(i) * 1.873 + float(shell) * 11.0;
            float drift = t * (0.35 + 0.25 * h11(seed * 1.9));
            cardCol = cardArt(cuv, seed, shellAccent, drift);
            // Far cards dim slightly into the haze
            cardCol *= 0.92 + 0.18 * h11(seed * 2.7);
            edgeCol = mix(cardCol * 0.4, vec3(0.15, 0.13, 0.16), 0.5);
        }

        // Subtle thin border (gallery card edge)
        float borderD = abs(d) - halfW * 0.06;
        float borderW = max(fwidth(borderD), 1e-4);
        float borderM = 1.0 - smoothstep(-borderW, borderW, borderD);
        // Only on cards within coverage
        borderM *= cov;

        // Depth-haze: cards at the back lose contrast against paper.
        float haze = clamp(0.5 - z * 0.5, 0.0, 1.0); // back→0.75, front→0.25
        cardCol = mix(cardCol, paper, haze * 0.18);
        edgeCol = mix(edgeCol, paper, haze * 0.22);

        // Z-order composite: keep the closest card per pixel.
        if (z > bestZ && cov > 0.002) {
            bestZ   = z;
            bestCol = cardCol;
            bestCov = cov;
            bestEdge = edgeCol;
            // Carry per-card border into the composite
        }

        // Shadow accumulates from all cards (cheap soft drop shadow on paper)
        float shadowW = max(fwidth(shadowD), 1e-4);
        float shadowM = 1.0 - smoothstep(-shadowW * 4.0, shadowW * 4.0, shadowD);
        // Shadows behind cards get a depth weight (cards in front shadow more)
        bestShadow = max(bestShadow, shadowM * (0.35 + 0.25 * clamp(z + 0.5, 0.0, 1.0)));
    }

    // Apply soft drop shadow to paper before the cards land on it
    col = mix(col, col * 0.78, bestShadow * 0.35);

    // Card body
    col = mix(col, bestCol, bestCov);
    // Recompute and apply border on the best card for crispness
    // (skipped — bestEdge already mixed into card; the implicit edge from
    // antialiased coverage is enough).

    // ───── Final paper grain + slow sheen ─────────────────────────────
    float grain = fbm2(fragUV * res.y * 0.018);
    col *= 1.0 + (grain - 0.6) * 0.05;

    // Gentle sweep of light raking the canvas — adds liveness even when quiet
    float sweep = smoothstep(0.0, 0.5,
                  sin(p.x * 1.0 - p.y * 0.5 - t * 0.28) * 0.5 + 0.5);
    col += pow(sweep, 4.0) * 0.04 * vec3(1.0, 0.97, 0.9);

    // Bloom into highlights when something is talking
    float L = dot(col, vec3(0.299, 0.587, 0.114));
    col += smoothstep(0.7, 1.1, L) * 0.10 * talk * accentA.rgb;

    // Mild contrast + paper feel
    col = clamp(col, 0.0, 1.5);
    col = col / (1.0 + 0.18 * col);
    col = pow(col, vec3(0.95));

    gl_FragColor = vec4(col, 1.0);
}
