/*{
  "DESCRIPTION": "Image Grid · Color BG · Text — a gallery-grade editorial poster. A saturated bold-color ground (vermillion, electric ultramarine, acid green, paper-white-on-black) is cut open by a diagonal staircase of image-cutout tiles. Each tile is a window into a different procedural world (raymarched studio room, abstract sculpture lights, marbled paper, mint-ticket gradient), parallaxed on its own z-plane with shadow-bake and a thin paper edge. Structural typography — the live cue.latest message — anchors the composition at three editorial scales: a giant centred date/time, a vertical column of indices on the left margin, a footer tag at the bottom. Three player channels each own a stride of the staircase: when a player gets loud, their tiles pop forward in z, tilt, and a flash of paint flares through their column; bass owns the ground hue (a Studio drift through the bold palette); cue.latest types onto the ground as the date numerals and into the tile windows as room captions. Anti-pattern free — no spectrum bars, no EKG, no checkerboard. Cutouts are content not pattern; the grid is the composition.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",         "TYPE": "text",  "DEFAULT": "OPEN STUDIOS 9000",                       "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "strideA",     "LABEL": "Stride A · Player 1", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "strideB",     "LABEL": "Stride B · Player 2", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "strideC",     "LABEL": "Stride C · Player 3", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].active" },
    { "NAME": "groundPulse", "LABEL": "Ground · Bass Hue",   "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.8, "BIND": "audio.bass" },

    { "NAME": "tileRows",    "LABEL": "Tile Rows",       "TYPE": "long",  "DEFAULT": 7, "VALUES": [4,5,6,7,8,9,10,12], "LABELS": ["4","5","6","7","8","9","10","12"] },
    { "NAME": "tileCols",    "LABEL": "Tile Cols",       "TYPE": "long",  "DEFAULT": 6, "VALUES": [4,5,6,7,8,9,10,12], "LABELS": ["4","5","6","7","8","9","10","12"] },
    { "NAME": "bgMode",      "LABEL": "Background Mode", "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Vermillion","Ultramarine","Acid","Paper/Black"] },
    { "NAME": "palette",     "LABEL": "Window Palette",  "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Studio","Marbled","Riso","Mono"] },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",    "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0 },
    { "NAME": "audioDepth",  "LABEL": "Audio Depth",     "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },

    { "NAME": "staircaseBias","LABEL":"Staircase Bias",  "TYPE": "float", "MIN": -1.0, "MAX": 1.0, "DEFAULT": 0.35 },
    { "NAME": "tileGutter",  "LABEL": "Gutter",          "TYPE": "float", "MIN": 0.0, "MAX": 0.10, "DEFAULT": 0.022 },
    { "NAME": "edgeShadow",  "LABEL": "Edge Shadow",     "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.85 },
    { "NAME": "textBlock",   "LABEL": "Type Block",      "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 1.0 },
    { "NAME": "grain",       "LABEL": "Print Grain",     "TYPE": "float", "MIN": 0.0, "MAX": 1.2, "DEFAULT": 0.40 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  IMAGE GRID · COLOR BG · TEXT  —  editorial poster, gallery grade
//
//  Reference: a saturated bold-color ground (vermillion in the ref) is
//  cut by a diagonal STAIRCASE of small image tiles. Each tile is a
//  window into a different "studio" — we draw procedural worlds, not
//  uploaded photos. Typography anchors three editorial slots: huge
//  centre date, vertical index column, footer tag. Cutouts have a paper
//  edge + drop shadow so they read as collaged paper, not screen tiles.
//
//  Three player channels each own a stripe of the staircase. When that
//  player is loud their tiles pop forward in z (real parallax), tilt
//  slightly, and a flash of complementary paint flares through their
//  column. Bass shifts the ground hue. cue.latest renders as the
//  giant centre numeral block + column captions inside the windows.
// ════════════════════════════════════════════════════════════════════════

#define MAX_WALK 48
#define MAX_ROWS 12
#define MAX_COLS 12
#define SPACE_CH 26

const float TAU = 6.28318530718;
const float PI  = 3.14159265359;

// ─── font atlas ──────────────────────────────────────────────────────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}
int getChar(int slot) {
    if (slot ==  0) return int(msg_0);   if (slot ==  1) return int(msg_1);
    if (slot ==  2) return int(msg_2);   if (slot ==  3) return int(msg_3);
    if (slot ==  4) return int(msg_4);   if (slot ==  5) return int(msg_5);
    if (slot ==  6) return int(msg_6);   if (slot ==  7) return int(msg_7);
    if (slot ==  8) return int(msg_8);   if (slot ==  9) return int(msg_9);
    if (slot == 10) return int(msg_10);  if (slot == 11) return int(msg_11);
    if (slot == 12) return int(msg_12);  if (slot == 13) return int(msg_13);
    if (slot == 14) return int(msg_14);  if (slot == 15) return int(msg_15);
    if (slot == 16) return int(msg_16);  if (slot == 17) return int(msg_17);
    if (slot == 18) return int(msg_18);  if (slot == 19) return int(msg_19);
    if (slot == 20) return int(msg_20);  if (slot == 21) return int(msg_21);
    if (slot == 22) return int(msg_22);  if (slot == 23) return int(msg_23);
    if (slot == 24) return int(msg_24);  if (slot == 25) return int(msg_25);
    if (slot == 26) return int(msg_26);  if (slot == 27) return int(msg_27);
    if (slot == 28) return int(msg_28);  if (slot == 29) return int(msg_29);
    if (slot == 30) return int(msg_30);  if (slot == 31) return int(msg_31);
    if (slot == 32) return int(msg_32);  if (slot == 33) return int(msg_33);
    if (slot == 34) return int(msg_34);  if (slot == 35) return int(msg_35);
    if (slot == 36) return int(msg_36);  if (slot == 37) return int(msg_37);
    if (slot == 38) return int(msg_38);  if (slot == 39) return int(msg_39);
    if (slot == 40) return int(msg_40);  if (slot == 41) return int(msg_41);
    if (slot == 42) return int(msg_42);  if (slot == 43) return int(msg_43);
    if (slot == 44) return int(msg_44);  if (slot == 45) return int(msg_45);
    if (slot == 46) return int(msg_46);  if (slot == 47) return int(msg_47);
    return -1;
}
int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 0;
    if (n > 48) return 48;
    return n;
}

// ─── hash & noise ────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  hash21(float n) { return vec2(hash11(n), hash11(n + 17.31)); }
vec3  hash31(float n) { return vec3(hash11(n), hash11(n+5.7), hash11(n+12.1)); }

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

// ─── ground palettes ─────────────────────────────────────────────────
// Saturated bold poster grounds. Audio shifts hue within each set.
vec3 groundColor(int mode, float t, float pulse) {
    if (mode == 0) {
        // Vermillion — the reference ground.
        vec3 base = vec3(0.93, 0.18, 0.16);
        vec3 hot  = vec3(1.00, 0.30, 0.10);
        return mix(base, hot, 0.5 + 0.5 * sin(t * 0.4)) + pulse * vec3(0.06, -0.02, -0.03);
    } else if (mode == 1) {
        // Electric ultramarine.
        vec3 base = vec3(0.10, 0.16, 0.85);
        vec3 hot  = vec3(0.18, 0.30, 1.00);
        return mix(base, hot, 0.5 + 0.5 * sin(t * 0.35)) + pulse * vec3(0.0, 0.04, 0.08);
    } else if (mode == 2) {
        // Acid green.
        vec3 base = vec3(0.55, 0.92, 0.10);
        vec3 hot  = vec3(0.70, 1.00, 0.20);
        return mix(base, hot, 0.5 + 0.5 * sin(t * 0.5)) + pulse * vec3(0.04, 0.06, -0.04);
    }
    // Paper-on-black — inverse mode: black ground, paper-white tiles.
    vec3 base = vec3(0.02, 0.02, 0.025);
    return base + pulse * vec3(0.04, 0.03, 0.06);
}

// ─── procedural window palettes ──────────────────────────────────────
// Each tile is a procedural "room" — we pick its base palette and a few
// shapes inside. The palette family is selected by `palette`.
vec3 windowPalette(int family, float seed) {
    if (family == 0) {
        // Studio — warm whites, parquet browns, blacks.
        vec3 a = vec3(0.92, 0.90, 0.86);
        vec3 b = vec3(0.58, 0.40, 0.26);
        vec3 c = vec3(0.10, 0.10, 0.12);
        float t = hash11(seed * 3.7);
        return mix(mix(a, b, t), c, hash11(seed * 5.3) * 0.25);
    } else if (family == 1) {
        // Marbled — soft blues, lilacs, cream.
        vec3 a = vec3(0.86, 0.88, 0.95);
        vec3 b = vec3(0.62, 0.55, 0.78);
        vec3 c = vec3(0.98, 0.93, 0.84);
        float t = hash11(seed * 4.1);
        return mix(mix(a, b, t), c, hash11(seed * 6.7) * 0.5);
    } else if (family == 2) {
        // Riso — mint, salmon, butter.
        vec3 a = vec3(0.62, 0.92, 0.78);
        vec3 b = vec3(1.00, 0.65, 0.55);
        vec3 c = vec3(0.98, 0.92, 0.55);
        float t = hash11(seed * 2.3);
        return mix(mix(a, b, t), c, hash11(seed * 8.1) * 0.6);
    }
    // Mono — silver greys.
    float g = 0.45 + 0.45 * hash11(seed * 9.7);
    return vec3(g);
}

// ─── procedural window contents ──────────────────────────────────────
// Inside each tile we draw 1–3 SDFs over a graded ground so the tile
// reads as a tiny photographed scene rather than a flat color swatch.
vec3 drawWindow(vec2 uv, int family, float seed, float depth, float t) {
    // Soft graded floor — vertical luminance ramp (horizon hint).
    vec3 base = windowPalette(family, seed);
    float horizon = smoothstep(-0.1, 0.6, uv.y);
    vec3 floorCol = base * mix(0.55, 1.0, 1.0 - horizon);
    vec3 wallCol  = base * mix(0.85, 1.15, horizon);
    vec3 col = mix(floorCol, wallCol, horizon);

    // A few procedural objects sitting on the floor.
    int objKind = int(floor(hash11(seed * 11.3) * 4.0));
    vec2 objPos = vec2((hash11(seed * 13.1) - 0.5) * 0.5, -0.2 + hash11(seed * 17.7) * 0.15);
    float objR  = 0.12 + 0.10 * hash11(seed * 23.7);

    if (objKind == 0) {
        // A round sculpture / stool.
        float d = length(uv - objPos) - objR;
        float shape = 1.0 - smoothstep(-0.005, 0.005, d);
        vec3 shapeCol = mix(base * 0.35, base * 1.4, 0.5 + 0.4 * uv.y);
        col = mix(col, shapeCol, shape);
        // Soft contact shadow under the object.
        float sh = smoothstep(0.05, 0.0, length((uv - objPos - vec2(0.0,-objR)) * vec2(1.0, 3.0)));
        col *= mix(1.0, 0.65, sh * 0.7);
    } else if (objKind == 1) {
        // Vertical rod / standing figure.
        float dx = abs(uv.x - objPos.x) - 0.04;
        float dy = uv.y - (objPos.y + objR * 1.8);
        float d  = max(dx, abs(dy) - objR * 1.8);
        float shape = 1.0 - smoothstep(-0.005, 0.005, d);
        col = mix(col, base * 0.25, shape);
    } else if (objKind == 2) {
        // Tilted panel / canvas on wall.
        vec2 q = uv - vec2(0.0, 0.15);
        float ang = (hash11(seed * 19.1) - 0.5) * 0.4;
        float ca = cos(ang), sa = sin(ang);
        q = mat2(ca, -sa, sa, ca) * q;
        float d = max(abs(q.x) - 0.18, abs(q.y) - 0.12);
        float shape = 1.0 - smoothstep(-0.005, 0.005, d);
        vec3 panelCol = mix(base * 1.3, vec3(1.0), 0.4);
        col = mix(col, panelCol, shape);
    } else {
        // Two stacked rectangles — ladder / shelf.
        for (int s = 0; s < 3; s++) {
            float fs = float(s);
            vec2 q = uv - vec2(objPos.x, -0.25 + fs * 0.18);
            float d = max(abs(q.x) - 0.20, abs(q.y) - 0.03);
            float shape = 1.0 - smoothstep(-0.004, 0.004, d);
            col = mix(col, base * (0.35 + 0.18 * fs), shape);
        }
    }

    // Light source — soft window highlight in the upper-left.
    float key = exp(-pow(length(uv - vec2(-0.25, 0.30)), 2.0) * 6.0);
    col += key * vec3(1.0, 0.97, 0.88) * 0.18;

    // Subtle fbm grain inside the tile so it reads as a photograph.
    float n = fbm2(uv * 9.0 + seed * 41.0 + t * 0.07);
    col *= 1.0 + (n - 0.5) * 0.18;

    // Depth tint — far tiles drop saturation slightly so the parallax
    // reads as space, not just z-translation.
    float satMul = mix(0.55, 1.0, depth);
    float lum = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(lum), col, satMul);
    return col;
}

// ─── rotated tile space ──────────────────────────────────────────────
vec2 rot2(vec2 p, float a) {
    float ca = cos(a), sa = sin(a);
    return mat2(ca, -sa, sa, ca) * p;
}

// ─── render typography block ─────────────────────────────────────────
// Draws a left-aligned word-wrapped string from msg[start..end) inside a
// rectangle (centered at `centre`, hsz-size `hsz`, with rotation `ang`).
// Returns alpha (0..1) for the glyph cell at this pixel.
float textBlockAlpha(vec2 p, vec2 centre, vec2 hsz, float ang,
                     int colsPerRow, int rowsMax, int startCh, int endCh) {
    vec2 q = rot2(p - centre, -ang);
    if (abs(q.x) > hsz.x || abs(q.y) > hsz.y) return 0.0;
    // Top-left origin local coords inside the box.
    float lx = q.x + hsz.x;
    float ly = hsz.y - q.y;
    float boxW = hsz.x * 2.0;
    float boxH = hsz.y * 2.0;
    float cellW = boxW / float(colsPerRow);
    float cellH = boxH / float(rowsMax);
    int tc = int(floor(lx / cellW));
    int tr = int(floor(ly / cellH));
    if (tc < 0 || tc >= colsPerRow) return 0.0;
    if (tr < 0 || tr >= rowsMax)    return 0.0;
    // Walk msg, laying chars onto the grid with simple word-wrap.
    int cursorR = 0, cursorC = 0, outCh = -1;
    for (int i = 0; i < MAX_WALK; i++) {
        int gi = startCh + i;
        if (gi >= endCh) break;
        if (cursorR > tr) break;
        int ch = getChar(gi);
        if (ch == SPACE_CH) {
            int wlen = 0;
            for (int j = 1; j < MAX_WALK; j++) {
                int gj = gi + j;
                if (gj >= endCh) break;
                int chj = getChar(gj);
                if (chj == SPACE_CH || chj < 0 || chj > 36) break;
                wlen++;
            }
            if (cursorC > 0 && cursorC + 1 + wlen > colsPerRow) {
                cursorR++; cursorC = 0;
            } else if (cursorC > 0) {
                if (cursorR == tr && cursorC == tc) outCh = SPACE_CH;
                cursorC++;
            }
        } else if (ch >= 0 && ch <= 36) {
            if (cursorR == tr && cursorC == tc) outCh = ch;
            cursorC++;
            if (cursorC >= colsPerRow) { cursorR++; cursorC = 0; }
        }
    }
    if (outCh < 0 || outCh > 35 || outCh == SPACE_CH) return 0.0;
    // Glyph aspect 5:7. Make tight cells with small padding.
    float glyphW = cellW * 0.88;
    float glyphH = cellH * 0.88;
    float padX = (cellW - glyphW) * 0.5;
    float padY = (cellH - glyphH) * 0.5;
    float gx = (lx - float(tc) * cellW - padX) / glyphW;
    float gy = 1.0 - (ly - float(tr) * cellH - padY) / glyphH;
    if (gx < 0.0 || gx > 1.0 || gy < 0.0 || gy > 1.0) return 0.0;
    float s = sampleChar(outCh, vec2(gx, gy));
    return smoothstep(0.18, 0.55, s);
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    float T   = TIME * motionSpeed;
    float aud = clamp(audioDepth, 0.0, 2.0);
    float bs  = clamp(groundPulse * aud, 0.0, 2.0);
    float lvl = clamp(audioLevel * aud, 0.0, 1.0);

    int rows = int(tileRows); if (rows > MAX_ROWS) rows = MAX_ROWS; if (rows < 2) rows = 2;
    int cols = int(tileCols); if (cols > MAX_COLS) cols = MAX_COLS; if (cols < 2) cols = 2;

    int total = charCount();
    bool liveUtterance = msgAge >= 0.0;
    int paletteFamily = int(palette);
    int bgIdx         = int(bgMode);

    // ── Ground (saturated bold color) ──────────────────────────────
    vec3 col = groundColor(bgIdx, T, bs * 0.35);
    // Subtle paper noise on the ground so it isn't a flat block.
    float pn = fbm2(p * 1.4 + T * 0.05);
    col *= 1.0 + (pn - 0.5) * 0.05;
    // A second, slower hue drift driven by audio level (full mix energy).
    col += lvl * vec3(0.05, 0.0, -0.04) * (bgIdx == 0 ? 1.0 : -1.0);

    // ── Staircase: pick a "step function" mapping column → row range ──
    // Tiles only fill cells inside the staircase corridor. The corridor
    // walks diagonally across the canvas with width staircaseBias.
    float bias = staircaseBias;

    // Per-stripe player ownership — divide columns into 3 stripes,
    // each owned by one player channel. Within a stripe we still render
    // multiple tiles; the player's energy z-pushes them forward and
    // tilts them slightly. Visual decomposition is by stripe.
    // strideA → cols [0, cols/3), strideB → [cols/3, 2cols/3), strideC → rest.
    float playerEnergies[3];
    playerEnergies[0] = clamp(strideA, 0.0, 1.0);
    playerEnergies[1] = clamp(strideB, 0.0, 1.0);
    playerEnergies[2] = clamp(strideC, 0.0, 1.0);

    // We'll composite tiles back-to-front. Iterate all cells, but for
    // each cell pick the dominant "card" based on staircase membership.
    // To keep cost bounded we sort cells by z implicitly: a tile's z is
    // a function of (column stripe, player energy, micro-noise). We do
    // two passes: first the shadow bake, second the tiles themselves.

    // Composite accumulator for tiles (over the ground).
    vec4 tileCol = vec4(0.0);

    // Gutter (paper-edge space) between tiles.
    float gut = max(tileGutter, 0.0);

    // Convert p (centered) into a normalized canvas [0..1] domain so the
    // grid sits regardless of aspect; staircase walks along this domain.
    vec2 nc = vec2((p.x / aspect) + 0.5, p.y + 0.5);

    // Pass 1 — shadows: each tile drops a soft shadow on the ground that
    // depends on its z-offset (front tiles → bigger, softer shadows).
    for (int r = 0; r < MAX_ROWS; r++) {
        if (r >= rows) break;
        for (int c = 0; c < MAX_COLS; c++) {
            if (c >= cols) break;
            float fr = float(r), fc = float(c);
            float fcols = float(cols), frows = float(rows);

            // Staircase membership — is this cell part of the corridor?
            float diag = (fr / frows) - (fc / fcols) - bias * 0.5;
            // corridor hsz-width (in normalized units of the diagonal)
            float corridor = 0.22 + 0.05 * sin(T * 0.13 + fr * 0.7);
            if (abs(diag) > corridor) continue;

            // Pick player stripe for this column.
            int stripe = int(floor((fc + 0.5) * 3.0 / fcols));
            stripe = clamp(stripe, 0, 2);
            float pe = playerEnergies[stripe];

            float seed = fr * 11.0 + fc * 7.0 + 0.13;
            float zPop = mix(0.0, 0.18, pe) + 0.04 * hash11(seed);
            float tilt = (hash11(seed * 3.1) - 0.5) * 0.20 * (0.35 + 0.65 * pe);

            // Tile centre in our centered p space.
            vec2 ctr;
            ctr.x = ((fc + 0.5) / fcols - 0.5) * aspect;
            ctr.y = ((fr + 0.5) / frows - 0.5);

            // Tile hsz-size minus gutter.
            vec2 hsz;
            hsz.x = (1.0 / fcols) * aspect * 0.5 - gut;
            hsz.y = (1.0 / frows) * 0.5 - gut;
            // Audio-aware breath — tiles "pop" with their player's energy.
            hsz *= (1.0 + 0.12 * pe + 0.04 * sin(T * 1.6 + seed));

            // Shadow centre offset by z-pop (front tiles → bigger drop).
            vec2 sCtr = ctr + vec2(zPop * 0.12, -zPop * 0.20);
            vec2 sHalf = hsz * (1.0 + zPop * 1.5);

            vec2 sq = rot2(p - sCtr, -tilt);
            float dx = abs(sq.x) - sHalf.x;
            float dy = abs(sq.y) - sHalf.y;
            float sd = max(dx, dy);
            // Soft shadow falloff with z-aware width.
            float sw = 0.018 + zPop * 0.06;
            float shA = smoothstep(sw, 0.0, sd);
            col *= mix(1.0, 0.55, shA * edgeShadow * 0.6);
        }
    }

    // Pass 2 — tiles themselves. Back-to-front naturally because we
    // iterate rows top to bottom; for cells with z-pop we draw them
    // over their neighbours because tileCol accumulates as "over".
    for (int r = 0; r < MAX_ROWS; r++) {
        if (r >= rows) break;
        for (int c = 0; c < MAX_COLS; c++) {
            if (c >= cols) break;
            float fr = float(r), fc = float(c);
            float fcols = float(cols), frows = float(rows);

            float diag = (fr / frows) - (fc / fcols) - bias * 0.5;
            float corridor = 0.22 + 0.05 * sin(T * 0.13 + fr * 0.7);
            if (abs(diag) > corridor) continue;

            int stripe = int(floor((fc + 0.5) * 3.0 / fcols));
            stripe = clamp(stripe, 0, 2);
            float pe = playerEnergies[stripe];

            float seed = fr * 11.0 + fc * 7.0 + 0.13;
            float zPop = mix(0.0, 0.18, pe) + 0.04 * hash11(seed);
            float tilt = (hash11(seed * 3.1) - 0.5) * 0.20 * (0.35 + 0.65 * pe);

            vec2 ctr;
            ctr.x = ((fc + 0.5) / fcols - 0.5) * aspect;
            ctr.y = ((fr + 0.5) / frows - 0.5);
            vec2 hsz;
            hsz.x = (1.0 / fcols) * aspect * 0.5 - gut;
            hsz.y = (1.0 / frows) * 0.5 - gut;
            hsz *= (1.0 + 0.12 * pe + 0.04 * sin(T * 1.6 + seed));

            // z-pop translates the tile slightly toward the viewer (parallax).
            vec2 tc2 = ctr + vec2(zPop * 0.04, zPop * 0.05);

            vec2 q = rot2(p - tc2, -tilt);
            float dx = abs(q.x) - hsz.x;
            float dy = abs(q.y) - hsz.y;
            float sd = max(dx, dy);
            // Anti-aliased fill.
            float fw = fwidth(sd);
            float fill = 1.0 - smoothstep(-fw, fw, sd);
            if (fill < 0.001) continue;

            // Local UV inside the tile, normalized to [-0.5..0.5].
            vec2 tuv = vec2(q.x / max(hsz.x, 1e-4), q.y / max(hsz.y, 1e-4)) * 0.5;

            // Depth value for this tile (0 back → 1 front). Used for
            // saturation falloff and edge highlight.
            float depth = clamp(zPop / 0.22, 0.0, 1.0);

            // Procedural window contents.
            vec3 wc = drawWindow(tuv, paletteFamily, seed, depth, T);

            // Energy flash — a sweep of complementary paint across a tile
            // owned by an active player. The flash uses bgMode's
            // complement so it reads against the ground.
            float sweep = smoothstep(0.0, 0.6,
                sin((tuv.x + tuv.y) * 6.0 - T * 2.0 + seed) * 0.5 + 0.5);
            vec3 flashColr = vec3(0.0);
            if (bgIdx == 0) flashColr = vec3(0.10, 0.85, 0.95); // teal vs vermillion
            else if (bgIdx == 1) flashColr = vec3(1.0, 0.78, 0.18); // gold vs ultramarine
            else if (bgIdx == 2) flashColr = vec3(0.85, 0.10, 0.55); // magenta vs acid
            else                 flashColr = vec3(0.98, 0.92, 0.55); // paper vs black
            wc = mix(wc, flashColr, sweep * pe * 0.35);

            // Paper edge — thin lighter rim suggesting a cut-paper tile.
            float edge = smoothstep(0.0, fw * 1.2, -sd) - smoothstep(0.0, fw * 2.4, -sd - 0.004);
            wc = mix(wc, vec3(1.0), clamp(edge, 0.0, 1.0) * 0.35);

            // Top-edge specular ridge for front-popped tiles only.
            float topRidge = smoothstep(hsz.y * 0.94, hsz.y * 0.99, q.y)
                           * smoothstep(hsz.x, hsz.x * 0.5, -abs(q.x));
            wc += topRidge * depth * vec3(1.0, 0.96, 0.86) * 0.18;

            // Tile-internal caption text (per-stripe small text label).
            // Only on a sparse subset of tiles so it reads as captioning,
            // not as a logo center. Uses cue.latest, word-wrapped to a
            // small grid at the tile's bottom edge.
            if (total > 0 && fract(hash11(seed * 51.7)) < 0.55) {
                vec2 capCtr = vec2(0.0, -hsz.y * 0.62);
                vec2 capHalf = vec2(hsz.x * 0.78, hsz.y * 0.20);
                float cap = textBlockAlpha(p - tc2, capCtr, capHalf, tilt,
                                           10, 2, 0, total);
                if (cap > 0.0) {
                    // Caption ink: dark on light tiles, light on dark.
                    float lumW = dot(wc, vec3(0.299, 0.587, 0.114));
                    vec3 ink = (lumW > 0.55) ? vec3(0.05, 0.05, 0.07) : vec3(0.98);
                    wc = mix(wc, ink, cap * 0.95);
                }
            }

            // Composite tile over running tileCol (back-to-front).
            float a = fill;
            tileCol.rgb = mix(tileCol.rgb, wc, a * (1.0 - tileCol.a));
            tileCol.a   = tileCol.a + a * (1.0 - tileCol.a);
        }
    }

    // ── Composite tiles over the ground ─────────────────────────────
    col = mix(col, tileCol.rgb, tileCol.a);

    // ── Structural Typography ───────────────────────────────────────
    // Three editorial blocks:
    //   1) Vertical index column on the LEFT margin (rotated 0°, narrow).
    //   2) Giant centered numerals / date band (mid-canvas, large).
    //   3) Footer tag bottom-left (small, horizontal).
    //
    // All three pull from msg (cue.latest typewriter). They render in a
    // contrasting ink — black on light grounds, white on dark grounds.

    if (total > 0 && textBlock > 0.001) {
        float lumG = dot(col, vec3(0.299, 0.587, 0.114));
        vec3 ink = (lumG > 0.55) ? vec3(0.04, 0.04, 0.06) : vec3(0.97, 0.97, 0.95);

        // Block 1 — left margin index column. Stays inside the ground
        // (won't overlap most tiles because the staircase doesn't touch
        // the far edges). Narrow column, many rows.
        {
            vec2 ctr  = vec2(-aspect * 0.5 + 0.10, 0.0);
            vec2 hsz = vec2(0.06, 0.34);
            float a = textBlockAlpha(p, ctr, hsz, 0.0, 6, 14, 0, total);
            col = mix(col, ink, a * textBlock);
        }

        // Block 2 — giant centred numeral / headline band. Sits in the
        // middle of the canvas at large scale; overlaps tiles intentionally
        // so the type reads as primary, image tiles as secondary. Slight
        // audio-driven kerning breath via tilt.
        {
            vec2 ctr  = vec2(0.0, 0.05);
            vec2 hsz = vec2(0.42, 0.13);
            float ang = sin(T * 0.07) * 0.012 * (0.5 + lvl);
            float a = textBlockAlpha(p, ctr, hsz, ang, 6, 2, 0, total);
            col = mix(col, ink, a * textBlock);
        }

        // Block 3 — footer tag bottom-left (small horizontal block).
        {
            vec2 ctr  = vec2(-aspect * 0.5 + 0.22, -0.45);
            vec2 hsz = vec2(0.20, 0.04);
            float a = textBlockAlpha(p, ctr, hsz, 0.0, 16, 2, 0, total);
            col = mix(col, ink, a * textBlock);
        }
    }

    // ── Print grain (analogue surprise) ─────────────────────────────
    if (grain > 0.001) {
        float g = fbm2(p * res.y * 0.012 + 7.3) - 0.5;
        g += 0.5 * (fbm2(p * res.y * 0.05 - 3.1) - 0.5);
        col *= 1.0 + g * grain * 0.10;
    }

    // ── Gentle vignette + chromatic edge to settle the poster ───────
    float vig = 1.0 - 0.22 * dot(p, p);
    col *= vig;

    // Soft tonemap (the host applies final ACES on linear HDR).
    col = col / (1.0 + 0.55 * col);

    gl_FragColor = vec4(col, 1.0);
}
