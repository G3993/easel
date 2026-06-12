/*{
  "DESCRIPTION": "Gradient Text — abstract concert-poster homage. Three parallax layers of soft chromatic dots (orange, purple, teal) drift in faux-3D depth over a paper backdrop, each layer bound to its own player.energy channel so silent voices fall still and active voices shimmer. The cue.latest message cascades downward in a stair-step of repeated rows that grow from a whisper to a shout, typewriter-revealed by msgAge. Audio bass swells the dot field; mid/high modulate parallax. Returns LINEAR HDR — host applies ACES.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",          "TYPE": "text",  "DEFAULT": "AURAL SPACES", "MAX_LENGTH": 48 },
    { "NAME": "textSize",     "LABEL": "Text Size",        "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.4, "MAX": 2.2 },
    { "NAME": "cascadeRows",  "LABEL": "Cascade Rows",     "TYPE": "long",  "DEFAULT": 7, "VALUES": [3,4,5,6,7,8,9,10,12], "LABELS": ["3","4","5","6","7","8","9","10","12"] },
    { "NAME": "rowStagger",   "LABEL": "Row Stagger",      "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 1.2 },
    { "NAME": "growth",       "LABEL": "Type Growth",      "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.0, "MAX": 1.5 },

    { "NAME": "energyA",      "LABEL": "Player 1 Energy",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",      "LABEL": "Player 2 Energy",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",      "LABEL": "Player 3 Energy",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },

    { "NAME": "bassDrive",    "LABEL": "Bass Drive",       "TYPE": "float", "DEFAULT": 0.7, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.bass" },
    { "NAME": "midDrive",     "LABEL": "Mid Drive",        "TYPE": "float", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.mid" },
    { "NAME": "highDrive",    "LABEL": "High Drive",       "TYPE": "float", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.high" },

    { "NAME": "dotDensity",   "LABEL": "Dot Density",      "TYPE": "float", "DEFAULT": 14.0, "MIN": 5.0, "MAX": 30.0 },
    { "NAME": "dotSoftness",  "LABEL": "Dot Softness",     "TYPE": "float", "DEFAULT": 0.65, "MIN": 0.1, "MAX": 1.2 },
    { "NAME": "depthAmount",  "LABEL": "Depth / Parallax", "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 2.5 },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",     "TYPE": "float", "DEFAULT": 0.6, "MIN": 0.0, "MAX": 2.0 },

    { "NAME": "paletteShift", "LABEL": "Palette Shift",    "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "colorA",       "LABEL": "Color · P1 (Orange)", "TYPE": "color", "DEFAULT": [0.96, 0.50, 0.18, 1.0] },
    { "NAME": "colorB",       "LABEL": "Color · P2 (Purple)", "TYPE": "color", "DEFAULT": [0.65, 0.46, 0.96, 1.0] },
    { "NAME": "colorC",       "LABEL": "Color · P3 (Teal)",   "TYPE": "color", "DEFAULT": [0.20, 0.78, 0.62, 1.0] },
    { "NAME": "paperColor",   "LABEL": "Paper Color",      "TYPE": "color", "DEFAULT": [0.86, 0.86, 0.84, 1.0] },
    { "NAME": "inkColor",     "LABEL": "Ink Color",        "TYPE": "color", "DEFAULT": [0.05, 0.05, 0.07, 1.0] },

    { "NAME": "headerStrip",  "LABEL": "Header Strip",     "TYPE": "bool",  "DEFAULT": 1.0 },
    { "NAME": "footerStrip",  "LABEL": "Footer Strip",     "TYPE": "bool",  "DEFAULT": 1.0 },
    { "NAME": "grain",        "LABEL": "Paper Grain",      "TYPE": "float", "DEFAULT": 0.35, "MIN": 0.0, "MAX": 1.0 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//   GRADIENT TEXT  ·  concert-poster homage  ·  3 parallax player layers
//
//   Composition:
//     • Paper backdrop, off-white, subtle grain.
//     • Three z-stacked dot layers — back/teal, mid/purple, front/orange.
//       Each layer parallax-shifts at its own speed and is sized to its
//       own player.energy channel. Silent player → still, faded layer.
//     • A cascade of repeated message rows from small-top-left to
//       big-bottom-right (the AURAL → AURAL SPACES → S P A C E S step).
//     • Header dot strip (abstracted concert masthead).
//     • Footer dot strip (abstracted credit row).
//
//   Depth:
//     • Faux-3D — dot layers have different parallax speeds, depth-of-
//       field blur (softness varies per layer), and the cascade rows
//       perspective-foreshorten via Z scaling. Mouse parallaxes camera.
//
//   Motion:
//     • Layers drift at distinct speeds (motionSpeed × layer factor).
//     • Bass swells the dot radii in synchrony.
//     • Mid/high modulate layer offset → chromatic separation pulses.
//     • Cascade has its own slow downward drift; rows oscillate left/
//       right gently. Typewriter reveal driven by msgAge.
//     • Quiet → still drift; loud → chromatic shimmer and dot bloom.
//
// ════════════════════════════════════════════════════════════════════════

#define MAX_CHARS    48
#define SPACE_CH     26
#define MAX_ROWS     12
#define TAU          6.28318530718

// ─── Font atlas sampling (shared idiom with text_clusters.fs) ──────────
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
    if (n > MAX_CHARS) return MAX_CHARS;
    return n;
}

// ─── Utility hashes / noise ────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  hash21(float n) { return fract(sin(vec2(n, n + 17.31)) * vec2(127.1, 311.7)); }
vec2  hash22(vec2 p) {
    p = vec2(dot(p, vec2(127.1, 311.7)), dot(p, vec2(269.5, 183.3)));
    return fract(sin(p) * 43758.5453);
}

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

// ─── Single dot layer: returns soft glow + coverage at this fragment ──
// `cellSize` = grid spacing in normalized space, `pos` = aspect-corrected
// fragment coords already in the layer's parallax frame. Each cell hosts
// one fuzzy disc; radius depends on a per-cell hash + global energy.
// `softness` controls the falloff bell (faux DOF: back layers softer).
vec3 dotLayer(vec2 pos, float cellSize, float energy,
              vec3 tint, float softness, float t, float jitter)
{
    // Per-layer slow drift / breathing.
    pos += vec2(sin(t * 0.13) * 0.08, cos(t * 0.11) * 0.05) * jitter;

    vec2 gp = pos / cellSize;
    vec2 ip = floor(gp);
    vec2 fp = fract(gp) - 0.5;

    vec3 acc = vec3(0.0);
    // 3x3 neighbour search so soft halos blend across cells (no grid lines)
    for (int dy = -1; dy <= 1; dy++) {
        for (int dx = -1; dx <= 1; dx++) {
            vec2 off = vec2(float(dx), float(dy));
            vec2 cell = ip + off;
            vec2 h = hash22(cell + 71.3);
            // Some cells empty (sparser at the edges of the canvas)
            float occupancy = step(0.18, h.x);
            // Per-cell jitter inside its slot — placement, not a literal grid
            vec2 center = off + vec2(h.x - 0.5, h.y - 0.5) * 0.72;
            // Per-cell radius — energy modulates a portion of it so silent
            // players have ghost dots and active players bloom.
            float baseR = mix(0.10, 0.38, h.x);
            float r = baseR * (0.55 + 0.85 * energy);
            // Slow per-cell pulse so even quiet layers aren't dead — but
            // amplitude scales with energy, so silence ≈ near-still.
            float ph = h.y * TAU;
            r *= 1.0 + 0.10 * sin(t * 1.7 + ph) * (0.35 + energy);

            float d = length(fp - center);
            // Smooth glow falloff — wider tail = softer / further away
            float core = exp(-pow(d / max(r, 1e-4), 1.8) * (3.5 / softness));
            // Add a hard-ish edge to keep dots reading as discs not smudges
            float edge = smoothstep(r * 1.05, r * 0.75, d);
            float glow = max(core * 0.95, edge);
            acc += tint * glow * occupancy;
        }
    }
    return acc;
}

// ─── Header / footer strip — small abstracted dot pattern ──────────────
// Decorative band of small dots, irregularly spaced (not a literal logo),
// gives the poster's masthead/credit feel without rendering icons.
float bandDots(vec2 uv, float yCenter, float yHeight, float density) {
    float band = smoothstep(yHeight, yHeight * 0.5, abs(uv.y - yCenter));
    if (band <= 0.001) return 0.0;
    float x = uv.x * density;
    float xi = floor(x);
    float xf = fract(x) - 0.5;
    vec2 h = hash22(vec2(xi, floor(yCenter * 31.3)));
    float r = mix(0.15, 0.40, h.x);
    float occ = step(0.35, h.y);
    float d = length(vec2(xf, (uv.y - yCenter) / yHeight));
    return band * occ * smoothstep(r, r * 0.5, d);
}

void main() {
    vec2 res    = RENDERSIZE;
    vec2 uv     = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    // Aspect-corrected centered coords. y up.
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = (uv.y - 0.5);

    float t       = TIME * motionSpeed;
    float bass    = clamp(audioBass * bassDrive, 0.0, 2.0);
    float mid     = clamp(audioMid  * midDrive,  0.0, 2.0);
    float high    = clamp(audioHigh * highDrive, 0.0, 2.0);

    // Mouse-driven camera parallax: shifts each layer by a different amount.
    vec2 cam = (mousePos - 0.5) * depthAmount * 0.18;

    // Per-layer parallax depths. Back layer barely moves; front layer
    // shifts the most → real-feeling parallax window.
    vec2 pBack  = p - cam * 0.25 + vec2(sin(t * 0.07), cos(t * 0.05)) * 0.04 * depthAmount;
    vec2 pMid   = p - cam * 0.55 + vec2(cos(t * 0.09), sin(t * 0.06)) * 0.07 * depthAmount;
    vec2 pFront = p - cam * 1.00 + vec2(sin(t * 0.11), cos(t * 0.08)) * 0.11 * depthAmount;

    // Mid/high audio adds small chromatic separation per layer (RGB-split feel
    // but applied as positional offset to each tint layer, not a grade).
    pBack  += vec2( 0.005, 0.0) * mid;
    pMid   += vec2(-0.007, 0.003) * high;
    pFront += vec2( 0.010,-0.004) * (mid + high) * 0.5;

    // Cell sizes per layer — smaller cells in front for the "denser closer"
    // depth cue. Bass swells all cells slightly (whole field breathes).
    float densityBase = max(dotDensity, 5.0);
    float cellBack  = (1.4 / densityBase) * (1.0 + 0.12 * bass);
    float cellMid   = (1.0 / densityBase) * (1.0 + 0.10 * bass);
    float cellFront = (0.75 / densityBase) * (1.0 + 0.08 * bass);

    // Player-energy bindings — each layer responds to its OWN channel.
    // Add an audio.bass baseline so audio-only setups still animate; the
    // shader passes the binding floor without requiring all three players.
    float eA = clamp(energyA + bass * 0.25, 0.0, 1.5);   // front / orange
    float eB = clamp(energyB + bass * 0.20, 0.0, 1.5);   // mid   / purple
    float eC = clamp(energyC + bass * 0.15, 0.0, 1.5);   // back  / teal

    // Optional palette shift — slow hue rotation across all three tints.
    float ps = paletteShift;
    vec3 cA = mix(colorA.rgb, colorA.rgb.gbr, ps);
    vec3 cB = mix(colorB.rgb, colorB.rgb.brg, ps);
    vec3 cC = mix(colorC.rgb, colorC.rgb.gbr, ps);

    // Soften back layer most (faux DOF), front layer crispest.
    float soft = max(dotSoftness, 0.15);
    vec3 back  = dotLayer(pBack,  cellBack,  eC, cC, soft * 1.6, t * 0.7, 1.0);
    vec3 midL  = dotLayer(pMid,   cellMid,   eB, cB, soft * 1.1, t * 1.0, 0.7);
    vec3 front = dotLayer(pFront, cellFront, eA, cA, soft * 0.8, t * 1.4, 0.4);

    // ── Paper backdrop ────────────────────────────────────────────────
    vec3 paper = paperColor.rgb;
    // Subtle vignette so center reads as light and edges sit back
    paper *= 1.0 - 0.10 * dot(p, p);
    // Gentle paper marble
    float marb = fbm2(p * 1.4 + 7.0);
    paper *= 1.0 + (marb - 0.5) * 0.05;

    vec3 col = paper;

    // Composite dot layers back→mid→front using soft "screen-ish" add so
    // overlapping discs sum to brighter colored areas (the reference's
    // teal-on-purple-on-orange overlap regions).
    col += back  * 0.85;
    col += midL  * 0.95;
    col += front * 1.05;

    // ── Cascade text — the "AURAL SPACES" stair ───────────────────────
    int total = charCount();
    bool liveUtterance = msgAge >= 0.0;
    int rows = int(cascadeRows);
    if (rows > MAX_ROWS) rows = MAX_ROWS;
    if (rows < 1) rows = 1;

    // Reveal pacing — typewriter walks across each row, then the next row
    // is born after a stagger gap. In static mode (no live cue) all rows
    // show full so the user previews everything.
    const float CPS = 24.0;            // chars/sec reveal pace
    float ageRow = liveUtterance ? msgAge : 1e6;

    // Background row drift — slow downward shimmer driven by mid energy
    float bgDrift = 0.0 + 0.02 * sin(t * 0.5) * (0.5 + mid);

    // Cascade vertical range (top → bottom of the poster, with margins
    // reserved for header/footer strips).
    float topY    =  0.40;
    float botY    = -0.40;
    float rowsF   = float(max(rows - 1, 1));

    // Highest charMask seen across all rows wins the ink at that fragment.
    float charMask = 0.0;

    for (int r = 0; r < MAX_ROWS; r++) {
        if (r >= rows) break;
        float fr = float(r);
        float ru = fr / rowsF;            // 0 at top → 1 at bottom

        // Per-row birth time: each row blooms after the previous finishes
        // a fraction of its reveal. Stagger=0 → all rows reveal together;
        // bigger stagger → progressive cascade.
        float rowBirth = fr * rowStagger * (float(total) / CPS) * 0.4;
        if (liveUtterance && ageRow < rowBirth) continue;

        // Row scale ramp — small at top, big at bottom (the AURAL → SPACES
        // growth). Growth slider controls how steep this ramp is.
        float rowScale = mix(0.45, 1.55, pow(ru, 0.85)) * mix(1.0, 1.0 + growth * 0.5, ru);
        // Multiplied by user textSize slider.
        float scaleAbs = textSize * rowScale;

        // Glyph metrics in poster space.
        float charH = 0.040 * scaleAbs;
        float charW = charH * (5.0 / 7.0);
        float kern  = charW * 1.05;

        // Row y position, with stair-step diagonal x offset so each row
        // shifts right as it grows (the reference's down-and-right stagger).
        float yCenter = mix(topY, botY, ru) + bgDrift;
        // Small horizontal oscillation per row for liveness (different
        // phase per row so it never looks like a rigid wave).
        float xJitter = sin(t * 0.6 + fr * 1.3) * 0.005 * (0.4 + 0.6 * high);
        // Per-row diagonal offset: top rows start further left, bottom rows
        // slightly right of center. Aspect-aware so it scales on wide canvases.
        float xCenter = mix(-0.18, 0.05, ru) * aspect + xJitter;

        // Per-row typewriter cursor — how many chars revealed so far.
        float rowAge = liveUtterance ? max(ageRow - rowBirth, 0.0) : 1e6;
        int revealed = liveUtterance ? int(floor(rowAge * CPS)) : total;
        if (revealed > total) revealed = total;
        if (revealed < 0) revealed = 0;

        // Row total width (only revealed chars take space, but layout is
        // left-aligned from xCenter so growth feels like typing).
        float rowW = float(total) * kern;
        // Anchor: row left edge at xCenter - rowW/2 (centered) or shifted
        // to give the staggered diagonal. We'll center each row at xCenter
        // so the entire row block centers on the diagonal track.
        float xLeft = xCenter - rowW * 0.5;

        // Pixel-row test — is the fragment inside this row's vertical band?
        float yTop = yCenter + charH * 0.5;
        float yBot = yCenter - charH * 0.5;
        if (p.y > yTop || p.y < yBot) continue;

        // Local x along the row, normalized to column index.
        float lx = p.x - xLeft;
        if (lx < 0.0 || lx > rowW) continue;
        int col_i = int(floor(lx / kern));
        if (col_i < 0 || col_i >= total) continue;
        if (col_i >= revealed) continue;

        // Local glyph coords (atlas expects u left→right, v bottom→top
        // after we flip).
        float xInCol = lx - float(col_i) * kern;
        float colPad = (kern - charW) * 0.5;
        float gx = (xInCol - colPad) / charW;
        if (gx < 0.0 || gx > 1.0) continue;
        float gy = (p.y - yBot) / charH;        // 0 at bottom → 1 at top
        // Atlas glyphs are stored with V=1 at TOP, so the v we sample is
        // gy directly (gy=1 → top of glyph). text_clusters flips because
        // its yInRow grows top→down; ours grows bottom→up already.
        int ch = getChar(col_i);
        if (ch < 0 || ch > 35 || ch == SPACE_CH) continue;

        float s = sampleChar(ch, vec2(gx, gy));
        // Antialias edge with fwidth(s) for crisp type at any scale
        float w = fwidth(s) + 1e-4;
        float glyph = smoothstep(0.5 - w, 0.5 + w, s);
        // Per-row fade — newly-born rows pop in softly so cascade reveals
        // don't feel binary. Hold full alpha after a brief ease.
        float rowFade = liveUtterance ? smoothstep(0.0, 0.18, rowAge) : 1.0;
        // Smaller rows feel lighter / slightly transparent (depth cue)
        float depthFade = mix(0.78, 1.0, ru);
        glyph *= rowFade * depthFade;

        if (glyph > charMask) charMask = glyph;
    }

    // Ink the glyphs over the composite.
    col = mix(col, inkColor.rgb, clamp(charMask, 0.0, 1.0));

    // ── Header strip — small concert-masthead dots ────────────────────
    if (headerStrip) {
        float band = bandDots(vec2(p.x, p.y), 0.46, 0.018, 60.0);
        col = mix(col, inkColor.rgb, band * 0.85);
    }
    // ── Footer strip — credit-row dots ────────────────────────────────
    if (footerStrip) {
        float band = bandDots(vec2(p.x, p.y), -0.46, 0.012, 75.0);
        col = mix(col, inkColor.rgb, band * 0.70);
    }

    // ── Subtle HDR bloom on dots — energy → glow swell ────────────────
    float layerLum = dot(front + midL * 0.7, vec3(0.299, 0.587, 0.114));
    float bloomAmt = smoothstep(0.6, 1.4, layerLum);
    col += bloomAmt * (front * 0.6 + midL * 0.3) * (0.4 + 0.8 * bass);

    // ── Paper grain (continuous noise, never a pixel grid) ────────────
    if (grain > 0.001) {
        float g = fbm2(p * res.y * 0.018) + 0.5 * fbm2(p * res.y * 0.05 + 17.0);
        col *= 1.0 + (g - 0.75) * grain * 0.10;
    }

    // Soft contrast tweak — keeps blacks rich without hard clipping.
    col = col / (1.0 + 0.25 * col);
    col = pow(max(col, 0.0), vec3(0.95));

    gl_FragColor = vec4(col, 1.0);
}
