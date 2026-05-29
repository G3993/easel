/*{
  "DESCRIPTION": "Grid Colors Text — a bold editorial poster as a parallax z-stack of colored publication tiles. Three depth layers of cells (back/mid/front) drift at different speeds against a slow camera dolly so the sheet has *real* room. Each layer's palette is owned by a player channel: when player[i].energy spikes the tiles on that layer swell forward in z, tilt off-axis, and burn a chromatic-stripe insert across their face; silent players collapse to a quiet constellation of flat slabs. `cue.latest` types across the tiles as a typewriter — the same headline scattered, fragmented, re-rendered. Cells are alive (corner-jitter, micro-skew, fresnel rim, drop-shadow under each), so the dominant texture is never a checkerboard. Returns LINEAR HDR.",
  "CREDIT": "easel auto-loop — A-List daily / Pulkki ‘Musical Analogues’ broadside reference",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",          "TYPE": "text",  "DEFAULT": "MUSICAL ANALOGUES OF MATHEMATICAL CONCEPTS", "MAX_LENGTH": 48, "BIND": "cue.latest" },
    { "NAME": "energyA",      "LABEL": "Layer A Energy (back)",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",      "LABEL": "Layer B Energy (mid)",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",      "LABEL": "Layer C Energy (front)", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].energy" },
    { "NAME": "activeA",      "LABEL": "Layer A Active",         "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].active" },
    { "NAME": "activeB",      "LABEL": "Layer B Active",         "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].active" },
    { "NAME": "audioDepth",   "LABEL": "Audio Depth Push",       "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.7, "BIND": "audio.bass" },
    { "NAME": "rows",         "LABEL": "Rows",                   "TYPE": "float", "MIN": 3.0, "MAX": 10.0, "DEFAULT": 6.0 },
    { "NAME": "cols",         "LABEL": "Columns",                "TYPE": "float", "MIN": 2.0, "MAX": 7.0,  "DEFAULT": 4.0 },
    { "NAME": "palette",      "LABEL": "Palette",                "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Pulkki","Broadside","Riso","Mono"] },
    { "NAME": "variant",      "LABEL": "Cell Variant",           "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2], "LABELS": ["Tile","Tilted","Stack"] },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",           "TYPE": "float", "MIN": 0.0, "MAX": 2.5, "DEFAULT": 1.0 },
    { "NAME": "parallax",     "LABEL": "Parallax Depth",         "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "textSize",     "LABEL": "Text Size",              "TYPE": "float", "MIN": 0.4, "MAX": 2.4, "DEFAULT": 1.0 },
    { "NAME": "stripeAmount", "LABEL": "Chromatic Stripes",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.65 },
    { "NAME": "shadow",       "LABEL": "Drop Shadow",            "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.9 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  GRID COLORS TEXT  ·  bold colored publication tiles, parallax z-stack
//
//  Reference: Ville Pulkki "Musical Analogues of Mathematical Concepts"
//  broadside — saturated rectangular blocks (red/green/lime/purple/gray)
//  with editorial typography slamming across them. Translation choices:
//    – Three depth layers (back/mid/front) instead of a flat poster, each
//      drifting at its own parallax speed so the sheet has REAL room.
//    – Each layer owns a player channel; player.energy swells its tiles
//      forward in z, tilts them, burns chromatic stripes across their face.
//    – player[i].active hard-cuts tile birth/death (compositional events).
//    – Cells are *alive*: corner-jitter, micro-skew, fresnel rim, drop
//      shadow. Never a static checkerboard.
//    – `msg` typewriters across tiles (cue.latest auto-bound).
//    – Returns LINEAR HDR — host applies ACES.
// ════════════════════════════════════════════════════════════════════════

#define MAX_MSG 48
#define SPACE_CH 26
#define TAU 6.28318530718

// ─── font atlas (37 cells: A..Z, space, 0..9) ───────────────────────────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
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
int msgTotal() {
    int n = int(msg_len);
    if (n < 0) return 0;
    if (n > MAX_MSG) return MAX_MSG;
    return n;
}

// ─── hash / noise ───────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float hash12(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
vec2  hash22(vec2 p)  { return fract(sin(vec2(dot(p, vec2(127.1, 311.7)),
                                              dot(p, vec2(269.5,  183.3)))) * 43758.5453); }
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float a = hash12(i);
    float b = hash12(i + vec2(1.0, 0.0));
    float c = hash12(i + vec2(0.0, 1.0));
    float d = hash12(i + vec2(1.0, 1.0));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}

// ─── palette ─────────────────────────────────────────────────────────────
// 6-slot palette per scheme. Reference is the Pulkki broadside: punchy
// red / lime / azure / purple / grey on near-black.
vec3 paletteColor(int scheme, int slot) {
    int s = slot - 6 * (slot / 6);
    if (scheme == 0) {
        // Pulkki — broadside primaries on near-black
        if (s == 0) return vec3(0.94, 0.18, 0.14);   // vermillion
        if (s == 1) return vec3(0.62, 0.94, 0.18);   // lime
        if (s == 2) return vec3(0.24, 0.26, 0.92);   // ultramarine
        if (s == 3) return vec3(0.74, 0.48, 0.96);   // lilac
        if (s == 4) return vec3(0.78, 0.78, 0.78);   // platinum
        return            vec3(0.05, 0.05, 0.06);    // near-black
    } else if (scheme == 1) {
        // Broadside — older inks
        if (s == 0) return vec3(0.85, 0.15, 0.20);
        if (s == 1) return vec3(0.95, 0.78, 0.20);
        if (s == 2) return vec3(0.12, 0.32, 0.62);
        if (s == 3) return vec3(0.08, 0.55, 0.46);
        if (s == 4) return vec3(0.96, 0.94, 0.88);
        return            vec3(0.07, 0.06, 0.05);
    } else if (scheme == 2) {
        // Riso duotone-leaning
        if (s == 0) return vec3(1.00, 0.42, 0.55);
        if (s == 1) return vec3(0.18, 0.86, 0.74);
        if (s == 2) return vec3(0.96, 0.84, 0.30);
        if (s == 3) return vec3(0.36, 0.20, 0.62);
        if (s == 4) return vec3(0.92, 0.90, 0.84);
        return            vec3(0.04, 0.04, 0.05);
    }
    // Mono
    float v = 0.10 + 0.18 * float(s);
    return vec3(v);
}

// Chromatic-stripe band — the "rainbow insert" across loud tiles.
// Six saturated horizontal stripes scrolling slowly.
vec3 stripes(vec2 cellUv, float t, float amount) {
    float n = 6.0;
    float row = floor(cellUv.y * n);
    float r = mod(row, 6.0);
    vec3 c;
    if (r < 1.0) c = vec3(0.95, 0.20, 0.20);
    else if (r < 2.0) c = vec3(0.98, 0.62, 0.12);
    else if (r < 3.0) c = vec3(0.98, 0.92, 0.18);
    else if (r < 4.0) c = vec3(0.22, 0.84, 0.34);
    else if (r < 5.0) c = vec3(0.20, 0.52, 0.96);
    else              c = vec3(0.62, 0.30, 0.92);
    // slight scroll so stripes feel alive
    float scroll = 0.5 + 0.5 * sin(cellUv.x * TAU * 1.5 + t * 1.4);
    c *= 0.85 + 0.15 * scroll;
    return mix(vec3(0.0), c, amount);
}

// Anti-aliased rectangle (inside == 1.0).
float aaRect(vec2 p, vec2 halfSize) {
    vec2 d = abs(p) - halfSize;
    float outside = length(max(d, vec2(0.0)));
    float inside  = min(max(d.x, d.y), 0.0);
    float sd = outside + inside;
    float fw = fwidth(sd);
    return 1.0 - smoothstep(-fw, fw, sd);
}

// 2D rotation
mat2 rot2(float a) { float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }

// ─── layer sample: render ONE depth layer of cells into a (col, premul) ──
// Each layer is a row/col grid offset by `layerOffset` and parallaxed.
// Tiles within the layer get individual seeds → color slot, jitter, tilt.
// Returns vec4(rgb premultiplied, alpha).
vec4 sampleLayer(
    vec2  p,            // screen-space (aspect-corrected, centered)
    float layerIdx,     // 0=back, 1=mid, 2=front
    vec2  layerOffset,  // parallax offset for this layer
    float layerZ,       // z position (0=back, 1=front) — affects scale
    int   scheme,
    float energy,       // player[i].energy 0..1
    float actv,         // player[i].active 0..1
    float stripeAmt,    // global stripe amount
    float t,            // global time
    float mvar,         // cell variant (0/1/2)
    int   total,
    vec2  cellGridSize  // (cols, rows) for this layer
) {
    // Parallax: cell-space coords for this layer.
    vec2 lp = (p - layerOffset) * mix(0.85, 1.15, layerZ);

    // Build a slight per-layer rotation so layers cross at angles.
    float lr = (layerIdx - 1.0) * 0.04;
    lp = rot2(lr) * lp;

    // Cell index — uses a non-integer grid; offsets per row for editorial
    // mis-alignment that breaks any checkerboard read.
    float gx = cellGridSize.x;
    float gy = cellGridSize.y;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    // Aspect-aware cell pitch
    vec2 cellSize = vec2(aspect, 1.0) / vec2(gx, gy);
    // Row-stagger: shift every other row by ~30% so columns don't line up.
    vec2 ip = lp;
    float rowF = floor((ip.y + 0.5) / cellSize.y);
    ip.x += mod(rowF, 2.0) * cellSize.x * 0.30;

    vec2 cellI = floor(vec2(ip.x / cellSize.x + 0.5*gx, ip.y / cellSize.y + 0.5*gy));
    vec2 cellCenterAbs;
    cellCenterAbs.x = (cellI.x - 0.5*gx + 0.5) * cellSize.x;
    cellCenterAbs.y = (cellI.y - 0.5*gy + 0.5) * cellSize.y;
    // un-stagger to get cell center in lp space
    cellCenterAbs.x -= mod(cellI.y, 2.0) * cellSize.x * 0.30;

    vec2 cellLocal = lp - cellCenterAbs;

    // Per-cell deterministic seed
    vec2 seed = hash22(cellI + layerIdx * 17.13);

    // ── Tile life-cycle ───────────────────────────────────────────────
    // Each cell has its own lifetime; actv=0 collapses tile to a slab
    // (smaller, flatter, no stripe). actv=1 lets the cell bloom.
    float life = seed.x;
    // Active-driven existence — at low actv, only the high-seed cells render.
    float existGate = smoothstep(0.55 - 0.55 * actv, 0.55, life);
    if (existGate < 0.001) return vec4(0.0);

    // ── Per-cell color slot ───────────────────────────────────────────
    int slot = int(floor(seed.y * 6.0));
    vec3 baseColor = paletteColor(scheme, slot);

    // ── Cell dimensions, with per-cell aspect (some tall, some wide) ──
    float aspectVar = mix(0.55, 1.55, seed.x);
    if (mvar == 2.0) aspectVar *= mix(0.5, 1.3, hash11(life * 91.3)); // Stack: bigger variance
    vec2 halfSize = cellSize * 0.5 * vec2(aspectVar, 1.0 / aspectVar);
    halfSize = min(halfSize, cellSize * 0.65); // never overflow the cell hugely

    // Pop-in scale animated by energy + time
    float bloom = 0.80 + 0.20 * energy + 0.10 * sin(t * 1.3 + life * 9.0);
    halfSize *= bloom;

    // ── Cell tilt — tilted variant rotates each cell by small angle ───
    float tiltAng = 0.0;
    if (mvar == 1.0) tiltAng = (seed.x - 0.5) * 0.35;
    if (mvar == 2.0) tiltAng = (seed.y - 0.5) * 0.18;
    // energy adds an off-axis kick so loud tiles "shake"
    tiltAng += (seed.x - 0.5) * 0.20 * energy;
    vec2 tiledCell = rot2(-tiltAng) * cellLocal;

    // ── Corner jitter so cells aren't perfect rectangles ──────────────
    vec2 jitter = (hash22(cellI * 3.7 + layerIdx) - 0.5) * cellSize * 0.06;
    tiledCell -= jitter;

    // ── Fill check ────────────────────────────────────────────────────
    float fill = aaRect(tiledCell, halfSize);
    if (fill < 0.001) return vec4(0.0);

    // ── Cell uv (0..1 across tile, used for stripes / text) ───────────
    vec2 cellUv = tiledCell / halfSize * 0.5 + 0.5;

    // ── Tile color: base, with optional chromatic stripe band ─────────
    vec3 col = baseColor;

    // Sub-band: ~30% of tiles get the colored-stripe insert across their
    // upper third (matches reference's small horizontal-stripe blocks).
    float stripeGate = step(0.65, seed.x) * stripeAmt;
    stripeGate *= 0.4 + 0.6 * energy;     // louder voice → more stripes
    float bandMask = smoothstep(0.50, 0.55, cellUv.y) * (1.0 - smoothstep(0.92, 0.96, cellUv.y));
    if (stripeGate > 0.01) {
        vec3 sc = stripes(cellUv, t + life * 5.0, 1.0);
        col = mix(col, sc, bandMask * stripeGate);
    }

    // ── Fresnel rim around tile edge — soft chromatic shimmer ─────────
    vec2 fromCenter = abs(tiledCell) / halfSize;
    float edge = max(fromCenter.x, fromCenter.y);
    float rim = smoothstep(0.85, 1.00, edge);
    vec3 rimCol = paletteColor(scheme, slot + 1);
    col = mix(col, rimCol, rim * 0.18 * (0.4 + energy));

    // ── Text from msg: type one line of text across the tile ─────────
    if (total > 0) {
        // Each cell shows a slice of the message starting at seed-derived offset
        int startOff = int(floor(seed.y * float(total)));
        // How many chars fit across this tile?
        float baseCharW = halfSize.x * 0.18 / max(textSize, 0.4);
        float baseCharH = baseCharW * (7.0 / 5.0);
        int   slotsX = int(floor(2.0 * halfSize.x / max(baseCharW, 1e-4)));
        if (slotsX > 16) slotsX = 16;
        if (slotsX < 1) slotsX = 1;
        float blockW = float(slotsX) * baseCharW;
        // Center text horizontally in the cell; pin to upper portion if tile is tall
        float blockH = baseCharH;
        // Vertical anchor: 60% down for "headline" feel (varies per tile via seed)
        float vAnchor = mix(0.35, 0.75, hash11(life * 13.7));
        vec2 textOrigin = vec2(-blockW * 0.5, halfSize.y * (vAnchor - 0.5) - blockH * 0.5);
        vec2 tlocal = tiledCell - textOrigin;
        if (tlocal.x >= 0.0 && tlocal.x <= blockW &&
            tlocal.y >= 0.0 && tlocal.y <= blockH) {
            int col_i = int(floor(tlocal.x / baseCharW));
            if (col_i >= 0 && col_i < slotsX) {
                // Typewriter reveal: only chars up through msg_len visible
                int idx = startOff + col_i;
                idx = idx - total * (idx / total);  // wrap
                if (idx < total) {
                    int ch = getChar(idx);
                    if (ch >= 0 && ch <= 36 && ch != SPACE_CH) {
                        // Glyph uv inside its cell. tlocal.y is y-UP in
                        // the tile-local frame (textOrigin at bottom).
                        // Host font atlas stores letter-top at v=1, so
                        // direct y-up→v mapping puts letter-top at
                        // screen-top. The previous `1.0 -` flipped
                        // glyphs upside down.
                        float yIn = clamp(tlocal.y / blockH, 0.0, 1.0);
                        float xIn = clamp((tlocal.x - float(col_i) * baseCharW) / baseCharW, 0.0, 1.0);
                        float s = sampleChar(ch, vec2(xIn, yIn));
                        s = smoothstep(0.30, 0.55, s);
                        // ink color: contrast against tile color (auto)
                        float lum = dot(baseColor, vec3(0.299, 0.587, 0.114));
                        vec3 ink = (lum > 0.55) ? vec3(0.04, 0.04, 0.07) : vec3(0.98, 0.96, 0.92);
                        col = mix(col, ink, s);
                    }
                }
            }
        }
    }

    // ── Subtle interior "tooth" so tile isn't a flat plane ────────────
    float tooth = vnoise(cellLocal * 38.0 + life * 19.0);
    col *= 1.0 + (tooth - 0.5) * 0.06;

    // ── Energy lift on the tile ───────────────────────────────────────
    col *= 1.0 + 0.18 * energy;

    return vec4(col * fill, fill);
}

// Drop-shadow lookup for one layer (samples aaRect just slightly above-left).
float layerShadow(
    vec2  p, vec2 layerOffset, float layerZ,
    vec2  shadowDir,           // pixel offset
    vec2  cellGridSize,
    float actv
) {
    vec2 lp = (p - layerOffset - shadowDir) * mix(0.85, 1.15, layerZ);
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    float gx = cellGridSize.x, gy = cellGridSize.y;
    vec2 cellSize = vec2(aspect, 1.0) / vec2(gx, gy);
    vec2 ip = lp;
    float rowF = floor((ip.y + 0.5) / cellSize.y);
    ip.x += mod(rowF, 2.0) * cellSize.x * 0.30;
    vec2 cellI = floor(vec2(ip.x / cellSize.x + 0.5*gx, ip.y / cellSize.y + 0.5*gy));
    vec2 cellCenterAbs;
    cellCenterAbs.x = (cellI.x - 0.5*gx + 0.5) * cellSize.x;
    cellCenterAbs.y = (cellI.y - 0.5*gy + 0.5) * cellSize.y;
    cellCenterAbs.x -= mod(cellI.y, 2.0) * cellSize.x * 0.30;
    vec2 cellLocal = lp - cellCenterAbs;
    vec2 seed = hash22(cellI + 17.13);
    float life = seed.x;
    float existGate = smoothstep(0.55 - 0.55 * actv, 0.55, life);
    if (existGate < 0.001) return 0.0;
    float aspectVar = mix(0.55, 1.55, seed.x);
    vec2 halfSize = min(cellSize * 0.5 * vec2(aspectVar, 1.0 / aspectVar), cellSize * 0.65);
    // Soft shadow: large smoothstep on the same rect
    vec2 d = abs(cellLocal) - halfSize;
    float outside = length(max(d, vec2(0.0)));
    float inside  = min(max(d.x, d.y), 0.0);
    float sd = outside + inside;
    return (1.0 - smoothstep(-0.025, 0.025, sd)) * existGate;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    int scheme = int(palette);
    int total  = msgTotal();
    float t    = TIME * clamp(motionSpeed, 0.0, 2.5);

    // ── camera drift — slow translational dolly, audio.bass kicks z ──
    float bass = clamp(audioDepth, 0.0, 2.0);
    vec2 dolly = vec2(sin(t * 0.10) * 0.05, cos(t * 0.07) * 0.04);
    float zPush = 0.06 * bass;

    // Layer parallax: back drifts least, front most. Each layer has its
    // own slow lateral drift offset by 1/3 phases.
    vec2 layerA_off = dolly * (0.4 * parallax) + vec2(sin(t * 0.05 + 0.0)  * 0.04, cos(t * 0.06 + 1.7)  * 0.03) * parallax;
    vec2 layerB_off = dolly * (0.8 * parallax) + vec2(sin(t * 0.07 + 2.1)  * 0.05, cos(t * 0.08 + 3.4)  * 0.04) * parallax;
    vec2 layerC_off = dolly * (1.3 * parallax) + vec2(sin(t * 0.09 + 4.2)  * 0.06, cos(t * 0.10 + 5.1)  * 0.05) * parallax;

    // ── grid dimensions per layer (slight variation per depth) ──────
    float gx = clamp(cols, 2.0, 7.0);
    float gy = clamp(rows, 3.0, 10.0);
    vec2 gridA = vec2(max(gx - 1.0, 2.0), max(gy - 1.0, 3.0));
    vec2 gridB = vec2(gx, gy);
    vec2 gridC = vec2(max(gx - 1.0, 2.0), max(gy + 1.0, 4.0));

    // ── background — near-black with subtle warm vignette ───────────
    vec3 bg = vec3(0.045, 0.040, 0.055);
    bg += 0.020 * vec3(0.6, 0.4, 0.9) * vnoise(uv * 7.0);
    bg *= 1.0 - 0.25 * dot((uv - 0.5), (uv - 0.5));

    // ── shadow pass (sample each layer offset to bottom-right) ──────
    vec2 shadowOff = vec2(0.012, -0.014) * shadow;
    float shA = layerShadow(p, layerA_off, 0.0,  shadowOff, gridA, max(activeA, 0.35));
    float shB = layerShadow(p, layerB_off, 0.5,  shadowOff, gridB, max(activeB, 0.35));
    float shC = layerShadow(p, layerC_off, 1.0,  shadowOff, gridC, max(activeA + activeB, 0.35));
    float totalShadow = clamp((shA + shB + shC) * 0.55, 0.0, 0.85) * shadow;
    bg *= 1.0 - totalShadow * 0.85;

    // ── render three depth layers (back→mid→front, painter's algo) ──
    vec3 col = bg;
    float mvar = float(int(variant));

    // back layer
    {
        vec4 layer = sampleLayer(
            p, 0.0, layerA_off, 0.0 + zPush * 0.4,
            scheme, energyA, max(activeA, 0.45), stripeAmount,
            t, mvar, total, gridA);
        // back layer is slightly desaturated (depth fog)
        layer.rgb = mix(layer.rgb, vec3(dot(layer.rgb, vec3(0.299, 0.587, 0.114))), 0.18);
        layer.rgb *= 0.88;
        col = mix(col, layer.rgb, layer.a * 0.95);
    }
    // mid layer
    {
        vec4 layer = sampleLayer(
            p, 1.0, layerB_off, 0.5 + zPush * 0.7,
            scheme, energyB, max(activeB, 0.55), stripeAmount,
            t + 1.3, mvar, total, gridB);
        col = mix(col, layer.rgb, layer.a * 0.98);
    }
    // front layer
    {
        vec4 layer = sampleLayer(
            p, 2.0, layerC_off, 1.0 + zPush,
            scheme, energyC, 0.85, stripeAmount,
            t + 2.7, mvar, total, gridC);
        // front layer slightly punchier
        layer.rgb *= 1.05;
        col = mix(col, layer.rgb, layer.a);
    }

    // ── overall depth fog so far tiles dissolve to bg ───────────────
    float depthFog = smoothstep(0.6, 1.05, length(p));
    col = mix(col, bg, depthFog * 0.18);

    // ── subtle film grain so nothing reads as a pixel grid ──────────
    float g = vnoise(uv * res.y * 0.012) + 0.5 * vnoise(uv * res.y * 0.03 + 7.0);
    col *= 1.0 + (g - 0.75) * 0.045;

    // ── reinhard-ish tonemap so HDR rim+stripe overdrive doesn't blow ─
    col = col / (1.0 + 0.45 * col);
    col = pow(max(col, 0.0), vec3(0.94));

    gl_FragColor = vec4(col, 1.0);
}
