/*{
  "DESCRIPTION": "Data Collector — a parallax telemetry deck. Three depth columns of scrolling values (timecodes, hex IDs, percentages, hashtags) tick past each other in z while a thermal heatmap cloud blooms across a dossier-grid backdrop. The current cue utterance lays down as the live log entry, revealed letter-by-letter via msgAge. Each column is its own data feed; mute one and the deck visibly thins. Abstract feel of 'the system is observing you' — no spectrum bars, no EKG, no literal icons.",
  "CREDIT": "easel a-list — data_collector_text",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",         "LABEL": "Log Entry",     "TYPE": "text",  "DEFAULT": "COLLECTING TELEMETRY",
      "MAX_LENGTH": 48, "BIND": "cue.latest" },
    { "NAME": "energyA",     "LABEL": "Column 1 Feed", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,
      "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",     "LABEL": "Column 2 Feed", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,
      "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",     "LABEL": "Column 3 Feed", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,
      "DEFAULT": 0.0, "BIND": "player[3].energy" },
    { "NAME": "aliveA",     "LABEL": "Col 1 Active",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,
      "DEFAULT": 1.0, "BIND": "player[1].active" },
    { "NAME": "aliveB",     "LABEL": "Col 2 Active",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,
      "DEFAULT": 1.0, "BIND": "player[2].active" },
    { "NAME": "aliveC",     "LABEL": "Col 3 Active",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,
      "DEFAULT": 1.0, "BIND": "player[3].active" },
    { "NAME": "heat",        "LABEL": "Heatmap Bloom", "TYPE": "float", "MIN": 0.0, "MAX": 1.5,
      "DEFAULT": 0.8, "BIND": "audio.level" },
    { "NAME": "datNoise",    "LABEL": "Data Noise",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,
      "DEFAULT": 0.55, "BIND": "data.entropy" },
    { "NAME": "columnCount", "LABEL": "Columns",       "TYPE": "long",
      "DEFAULT": 3, "VALUES": [2,3,4,5,6], "LABELS": ["2","3","4","5","6"] },
    { "NAME": "scrollSpeed", "LABEL": "Scroll Speed",  "TYPE": "float", "MIN": 0.05, "MAX": 3.0,
      "DEFAULT": 0.85 },
    { "NAME": "fontSize",    "LABEL": "Font Size",     "TYPE": "float", "MIN": 0.5, "MAX": 1.8,
      "DEFAULT": 1.0 },
    { "NAME": "audioDepth",  "LABEL": "Audio Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 2.0,
      "DEFAULT": 0.9 },
    { "NAME": "palette",     "LABEL": "Palette",       "TYPE": "long",
      "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Thermal","Mono Dossier","Cyan Console","Acid"] },
    { "NAME": "gridDensity", "LABEL": "Grid Density",  "TYPE": "float", "MIN": 0.0, "MAX": 2.0,
      "DEFAULT": 1.0 },
    { "NAME": "vignette",    "LABEL": "Vignette",      "TYPE": "float", "MIN": 0.0, "MAX": 1.5,
      "DEFAULT": 0.6 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  data_collector_text — telemetry deck. Independent depth columns of
//  scrolling values + a thermal heatmap cloud + a live cue log entry.
//  Each column owns its own channel; you can mute one and see it thin.
//  Smooth, fwidth-AA, motion every frame.
// ════════════════════════════════════════════════════════════════════════

#define MAX_COLS  6
#define SPACE_CH  26
#define MAX_WALK  48

// ─── Font atlas helpers (same atlas contract as Easel text shaders) ──
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

// ─── Hash / noise ────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float hash12(vec2  p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
vec2  hash22(vec2  p) {
    return fract(sin(vec2(dot(p, vec2(127.1, 311.7)),
                          dot(p, vec2(269.5,  183.3)))) * 43758.5453);
}

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash12(i);
    float b = hash12(i + vec2(1.0, 0.0));
    float c = hash12(i + vec2(0.0, 1.0));
    float d = hash12(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}
float fbm2(vec2 p) {
    float v = 0.0, a = 0.55;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.03 + vec2(11.3, 5.7);
        a *= 0.52;
    }
    return v;
}

// Deterministic synthetic-glyph: each (col, row, tickIdx) yields a
// pseudo-random char index in [0,35]. Letter-vs-digit mix biased per
// column so columns look distinct: some are timecodes, some are hex
// IDs, some are hashtag streams. Returns -1 for the rare "blank" slot.
int synthGlyph(int col, int row, int tickIdx, float letterBias) {
    float seed = float(col) * 91.3 + float(row) * 13.7 + float(tickIdx) * 7.21;
    float r = hash11(seed);
    if (r < 0.04) return -1;            // sparse blanks → reads as a stream
    float c = hash11(seed + 5.11);
    if (c < letterBias) {
        // letters: A..Z map to char indices 0..25
        return int(floor(hash11(seed + 17.0) * 26.0));
    } else {
        // digits: 0..9 map to char indices 27..36
        return 27 + int(floor(hash11(seed + 33.0) * 10.0));
    }
}

// Palette lookup — heat or dossier or console. t in [0,1].
vec3 palette_thermal(float t) {
    // dark navy → magenta → orange → bright yellow (heatmap)
    vec3 c0 = vec3(0.04, 0.05, 0.10);
    vec3 c1 = vec3(0.62, 0.08, 0.50);
    vec3 c2 = vec3(0.98, 0.45, 0.15);
    vec3 c3 = vec3(1.00, 0.92, 0.55);
    if (t < 0.33) return mix(c0, c1, t / 0.33);
    if (t < 0.66) return mix(c1, c2, (t - 0.33) / 0.33);
    return mix(c2, c3, clamp((t - 0.66) / 0.34, 0.0, 1.0));
}
vec3 palette_mono(float t) {
    vec3 paper = vec3(0.93, 0.91, 0.86);
    vec3 ink   = vec3(0.07, 0.06, 0.06);
    return mix(paper, ink, smoothstep(0.0, 1.0, t));
}
vec3 palette_console(float t) {
    vec3 deep  = vec3(0.02, 0.05, 0.08);
    vec3 cyan  = vec3(0.25, 0.85, 0.95);
    vec3 white = vec3(0.85, 0.98, 1.00);
    if (t < 0.5) return mix(deep, cyan, t / 0.5);
    return mix(cyan, white, (t - 0.5) / 0.5);
}
vec3 palette_acid(float t) {
    vec3 c0 = vec3(0.02, 0.10, 0.04);
    vec3 c1 = vec3(0.10, 0.85, 0.35);
    vec3 c2 = vec3(0.95, 1.00, 0.20);
    if (t < 0.5) return mix(c0, c1, t / 0.5);
    return mix(c1, c2, (t - 0.5) / 0.5);
}
vec3 pickPalette(float t, int which) {
    if (which == 1) return palette_mono(t);
    if (which == 2) return palette_console(t);
    if (which == 3) return palette_acid(t);
    return palette_thermal(t);
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;
    // Aspect-corrected; origin centred so columns symmetric.
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    int   cols     = int(columnCount);
    if (cols > MAX_COLS) cols = MAX_COLS;
    if (cols < 2)        cols = 2;
    float speed    = clamp(scrollSpeed, 0.05, 3.0);
    float fSize    = clamp(fontSize,    0.5, 1.8);
    float aDepth   = clamp(audioDepth,  0.0, 2.0);
    int   palIdx   = int(palette);
    float gridAmt  = clamp(gridDensity, 0.0, 2.0);
    float vigAmt   = clamp(vignette,    0.0, 1.5);
    float heatAmt  = clamp(heat,        0.0, 1.5);
    float entropy  = clamp(datNoise,    0.0, 1.0);

    float audio = clamp(audioLevel, 0.0, 1.0);
    float bass  = clamp(audioBass,  0.0, 1.0);
    float mid   = clamp(audioMid,   0.0, 1.0);
    float hi    = clamp(audioHigh,  0.0, 1.0);

    float t = TIME;

    // ─── Background — dossier grid + parchment fbm ───────────────────
    // Warm off-white with low-frequency stain; thin ruled grid (faint),
    // brighter ticks every 8 cells. Tooth fbm so it never reads as pixels.
    vec3 baseBg;
    if (palIdx == 0)      baseBg = vec3(0.06, 0.05, 0.09);   // thermal dark
    else if (palIdx == 1) baseBg = vec3(0.94, 0.92, 0.86);   // dossier paper
    else if (palIdx == 2) baseBg = vec3(0.03, 0.06, 0.09);   // console black
    else                  baseBg = vec3(0.03, 0.07, 0.04);   // acid black

    vec3 col = baseBg;

    // Faint orthogonal grid — 32 lines across, 18 down, aspect-stable.
    float gx = abs(fract(p.x * 16.0 * gridAmt) - 0.5);
    float gy = abs(fract(p.y * 18.0 * gridAmt) - 0.5);
    float gLine = min(gx, gy);
    float gAA = fwidth(gLine);
    float grid = 1.0 - smoothstep(0.0, gAA * 1.8, gLine - 0.46);
    // Tick marks every 8 cells: brighter dots.
    float tx = abs(fract(p.x * 2.0 * gridAmt) - 0.5);
    float ty = abs(fract(p.y * 2.25 * gridAmt) - 0.5);
    float tickDot = smoothstep(0.04, 0.0, length(vec2(tx, ty)));

    vec3 gridInk = (palIdx == 1) ? vec3(0.55, 0.50, 0.45)
                                 : vec3(0.55, 0.60, 0.75);
    col = mix(col, gridInk, grid * 0.10 * gridAmt);
    col = mix(col, gridInk, tickDot * 0.18 * gridAmt);

    // Parchment / fog: low-freq fbm, slow time drift. Provides the
    // 'pages of the dossier' marbling under everything.
    float dust = fbm2(p * 1.3 + vec2(t * 0.02, t * -0.015));
    col *= 0.92 + 0.16 * dust;

    // ─── Heatmap cloud — audio.level driven thermal bloom ────────────
    // Three drifting gaussian blobs, modulated by audio + fbm. Wraps the
    // composition in a hot/cold field; the deck reads as 'the system is
    // looking somewhere'. Cloud intensifies on loud passages.
    vec2 hcA = vec2(sin(t * 0.21) * 0.55,        cos(t * 0.17) * 0.30);
    vec2 hcB = vec2(cos(t * 0.13 + 1.2) * 0.65,  sin(t * 0.11) * 0.35 + 0.05);
    vec2 hcC = vec2(sin(t * 0.09 + 2.7) * 0.40, -0.18 + cos(t * 0.19) * 0.25);
    float hA = exp(-pow(length(p - hcA), 2.0) * 6.0);
    float hB = exp(-pow(length(p - hcB), 2.0) * 5.0);
    float hC = exp(-pow(length(p - hcC), 2.0) * 7.0);
    float heatField = (hA * (0.6 + bass * aDepth)
                    +  hB * (0.5 + mid  * aDepth)
                    +  hC * (0.4 + hi   * aDepth));
    // Stretch + perturb with fbm so the cloud has internal texture.
    heatField *= 0.55 + 0.9 * fbm2(p * 2.2 + vec2(t * 0.07, -t * 0.05));
    heatField *= heatAmt * (0.6 + 0.7 * audio);
    // Push it through the palette and blend additively.
    vec3 heatCol = pickPalette(clamp(heatField, 0.0, 1.0), palIdx);
    float heatMask = smoothstep(0.04, 0.55, heatField);
    col = mix(col, heatCol, heatMask * 0.75);

    // ─── Parallax columns of values ──────────────────────────────────
    // Each column lives at its own depth (zScale: smaller = far) and
    // scrolls vertically at a per-column speed. Glyphs deterministic
    // per (col,row,tickIdx); tickIdx advances as the column scrolls
    // so the values *actually change* rather than just translating.
    //
    // Per-column bindings:
    //   col 0 → energyA / aliveA   (the "primary" stream)
    //   col 1 → energyB / aliveB
    //   col 2 → energyC / aliveC
    //   cols 3..5 → use mixes of A/B/C (extras share with the primaries)
    //
    // When aliveX is 0, the column visibly thins (alpha drops, scroll
    // pauses) so you can SEE which feed went silent.

    float texAccum = 0.0;        // accumulated text mask
    vec3  textCol  = vec3(0.0);
    float liveCaret = -1.0;      // x position of caret on live row, when relevant

    // Sweep front-to-back so closer columns occlude farther ones.
    for (int cc = MAX_COLS - 1; cc >= 0; cc--) {
        if (cc >= cols) continue;
        float fc = float(cc);

        // Channel pick — first 3 columns use direct bindings.
        float energy, alive;
        if (cc == 0)      { energy = energyA; alive = aliveA; }
        else if (cc == 1) { energy = energyB; alive = aliveB; }
        else if (cc == 2) { energy = energyC; alive = aliveC; }
        else {
            // Extras: deterministic mix of the three primaries so they
            // still respond to a channel — never silent regardless.
            float w = hash11(fc * 3.7);
            if (w < 0.33)      { energy = energyA; alive = aliveA; }
            else if (w < 0.66) { energy = energyB; alive = aliveB; }
            else               { energy = energyC; alive = aliveC; }
        }
        energy = clamp(energy, 0.0, 1.0);
        alive  = clamp(alive,  0.0, 1.0);

        // Column depth — 0=back, 1=front. Distribute non-uniformly so
        // the deck reads as space, not as evenly stacked panes.
        float depthSeed = hash11(fc * 5.3);
        float zT = depthSeed;                    // 0..1
        float zScale = mix(0.55, 1.15, zT);      // perspective scale
        float zAlpha = mix(0.45, 1.00, zT);      // fog/atmospheric
        float zSpeed = mix(0.45, 1.30, zT);      // parallax velocity

        // Column horizontal position: spread across canvas, with a
        // slight parallax sway based on z and time.
        float colSlot = (fc + 0.5) / float(cols) - 0.5;
        float colX = colSlot * (aspect - 0.10);
        colX += 0.018 * sin(t * 0.4 + fc * 1.7) * (1.0 - zT);
        // Column horizontal half-width (text column body).
        float colW = (aspect / float(cols)) * 0.42 * zScale;

        // Local space inside this column (centered on colX).
        vec2 cp = vec2(p.x - colX, p.y);
        // Quick reject — outside the column bounds.
        if (abs(cp.x) > colW * 1.35) continue;

        // Glyph metrics — scaled by depth and the global fontSize.
        float glyphH = 0.030 * fSize * zScale;
        float glyphW = glyphH * 0.62;        // typical 5:8 grotesk ratio
        float linePitch = glyphH * 1.18;

        // Per-column letter bias — gives each column visual personality.
        // 0.85 → mostly letters (hashtag stream); 0.20 → mostly digits
        // (timecode/percentage stream). Deterministic per column.
        float letterBias = mix(0.18, 0.85, hash11(fc * 9.7));

        // Scroll offset — time × speed × per-column zSpeed × (1 + energy).
        // When alive==0, the column FREEZES (scroll halts) so muting a
        // player is *visible* compositionally, not just an alpha fade.
        float scrollPx = t * speed * zSpeed * (0.5 + 0.9 * energy);
        scrollPx = mix(scrollPx * 0.05, scrollPx, alive);
        // Map vertical position to row index. Top row = newest.
        float vy = cp.y + scrollPx;
        float rowF = vy / linePitch;
        int   rowI = int(floor(rowF));
        float rowFrac = rowF - float(rowI);

        // Horizontal cell index inside the column.
        float charsPerRow = max(1.0, floor((colW * 2.0) / glyphW));
        float vx = cp.x + colW;       // 0..2·colW
        float colF = vx / glyphW;
        int   colI = int(floor(colF));
        float colFrac = colF - float(colI);

        if (colI < 0 || float(colI) >= charsPerRow) continue;

        // Synthetic-glyph value for this cell. tickIdx folds in row so
        // each row holds a different value; modulate by a slow integer
        // tick so values *change* without scroll, like polling.
        int tickIdx = rowI + int(floor(t * (0.7 + energy * 1.6)));
        int ch = synthGlyph(cc, rowI, tickIdx, letterBias);
        if (ch < 0) continue;

        // Glyph cell uv. rowFrac is the fractional part of vy/linePitch
        // (vy = p.y + scrollPx, p.y is y-UP in world coords), so rowFrac
        // already grows screen-bottom→top. The host font atlas stores
        // letter-top at v=1, so a direct mapping puts letter-top at
        // screen-top. The previous `1.0 -` flipped this and rendered
        // glyphs upside down. Map colFrac into atlas u; small padding
        // so glyphs breathe.
        vec2 cellUV;
        cellUV.x = clamp((colFrac - 0.10) / 0.80, 0.0, 1.0);
        cellUV.y = clamp((rowFrac - 0.05) / 0.85, 0.0, 1.0);

        float s = sampleChar(ch, cellUV);
        // fwidth-AA on the alpha threshold for gallery-clean glyph edges.
        float aa = max(fwidth(s), 0.001);
        float glyphAlpha = smoothstep(0.45 - aa, 0.45 + aa, s);
        if (glyphAlpha < 0.001) continue;

        // Per-column ink — palette-driven, hue rotated per column.
        float hueT = fract(0.12 + fc * 0.21 + zT * 0.3);
        vec3 ink = pickPalette(0.20 + 0.65 * hueT, palIdx);
        // Mono palette inverts — paper bg means dark ink instead.
        if (palIdx == 1) ink = vec3(0.06, 0.05, 0.05) + vec3(0.10, 0.07, 0.04) * fc / float(cols);

        // Highlight band — periodically a row gets brightened, like a
        // selected log line. Drifts slowly.
        float highlight = step(0.92, hash11(float(rowI) + fc * 19.3));
        ink = mix(ink, ink * 1.7 + vec3(0.05), highlight * 0.4);

        // Pulse glow on the freshest 3 rows when energy spikes.
        float freshness = clamp(1.0 - float(rowI - int(floor(scrollPx / linePitch))) / 3.0, 0.0, 1.0);
        ink += freshness * energy * vec3(0.20, 0.16, 0.10);

        // Depth fade (atmospheric).
        float colAlpha = glyphAlpha * zAlpha * (0.55 + 0.55 * alive);

        // Composite (front-to-back, alpha-over).
        textCol = mix(textCol, ink, colAlpha * (1.0 - texAccum));
        texAccum = texAccum + colAlpha * (1.0 - texAccum);
    }

    col = mix(col, textCol, texAccum);

    // ─── Live log entry — cue.latest as the bottom-anchor caption ────
    // Typewriter via msgAge: reveal cps≈28. Floats just below center,
    // boxed by a thin underline. When msgAge < 0 (no live transcript)
    // we still show the static msg so the shader previews nicely.
    int total = charCount();
    if (total > 0) {
        bool live = msgAge >= 0.0;
        float revealed = live ? min(float(total), msgAge * 28.0)
                              : float(total);
        int   visN = int(floor(revealed));
        float caretFrac = revealed - float(visN);

        // Layout: centered horizontally, vertical ~ -0.30 (lower third).
        float capScale = 1.05 * fSize;
        float capH = 0.040 * capScale;
        float capW = capH * 0.62;
        // Width of the visible string in screen units.
        float strW = float(total) * capW * 1.05;
        // Cap to 90% of aspect-corrected width.
        float maxW = (aspect) * 0.92;
        if (strW > maxW) {
            float k = maxW / strW;
            capH *= k; capW *= k; strW *= k;
        }
        float capY = -0.30;
        float capX0 = -strW * 0.5;

        // Background plate for the caption — a faint dark strip so the
        // log entry reads on top of the heatmap regardless of palette.
        float plateY0 = capY - capH * 0.55;
        float plateY1 = capY + capH * 1.55;
        float plateX0 = capX0 - capW * 1.2;
        float plateX1 = capX0 + strW + capW * 1.2;
        bool inPlate = (p.x >= plateX0 && p.x <= plateX1
                     && p.y >= plateY0 && p.y <= plateY1);
        if (inPlate) {
            float plateAA = max(fwidth(p.x), 0.002);
            float edgeFade = smoothstep(0.0, 0.04, min(p.x - plateX0, plateX1 - p.x));
            edgeFade *= smoothstep(0.0, 0.04, min(p.y - plateY0, plateY1 - p.y));
            vec3 plateInk = (palIdx == 1) ? vec3(0.98, 0.96, 0.92)
                                          : vec3(0.02, 0.03, 0.05);
            col = mix(col, plateInk, edgeFade * 0.55);
        }

        // Walk the visible characters; render each as a glyph in the strip.
        for (int i = 0; i < 48; i++) {
            if (i >= visN) break;
            if (i >= total) break;
            int ch = getChar(i);
            // Render space as a blank advance.
            float cellX0 = capX0 + float(i) * capW * 1.05;
            float cellX1 = cellX0 + capW;
            if (p.x < cellX0 - capW || p.x > cellX1 + capW) continue;
            if (p.y < capY - capH * 0.10 || p.y > capY + capH * 1.10) continue;
            if (ch < 0 || ch > 35) continue;
            vec2 gUV;
            gUV.x = clamp((p.x - cellX0) / capW, 0.0, 1.0);
            // p.y is y-UP world coord; capY anchors the BOTTOM of the
            // caption cell. (p.y - capY) is positive going up, and the
            // host atlas stores letter-top at v=1 — so the direct
            // mapping puts letter-top at screen-top. The previous
            // `1.0 -` here flipped glyphs upside down.
            gUV.y = clamp((p.y - capY) / capH, 0.0, 1.0);
            float s = sampleChar(ch, gUV);
            float aa = max(fwidth(s), 0.001);
            float a = smoothstep(0.45 - aa, 0.45 + aa, s);
            if (a < 0.001) continue;
            vec3 logInk = (palIdx == 1) ? vec3(0.06, 0.04, 0.04)
                                        : vec3(0.98, 0.93, 0.78);
            col = mix(col, logInk, a);
        }
        // Blinking caret at the live write head.
        float caretX = capX0 + float(visN) * capW * 1.05;
        float caretXd = abs(p.x - caretX);
        float caretYd = max(0.0, abs(p.y - (capY + capH * 0.5)) - capH * 0.5);
        float caretSDF = max(caretXd - capW * 0.10, caretYd);
        float caretAA = max(fwidth(caretSDF), 0.001);
        float caretMask = 1.0 - smoothstep(0.0, caretAA * 1.5, caretSDF);
        // Blink at 2Hz, but ALSO modulates with the reveal-fraction so
        // the caret feels alive during typing (no blink mid-glyph).
        float blink = step(0.5, fract(t * 2.0));
        float caretAlpha = caretMask * (live ? mix(0.9, blink, smoothstep(0.1, 0.9, caretFrac))
                                             : blink) * 0.85;
        vec3 caretInk = (palIdx == 1) ? vec3(0.10, 0.06, 0.06)
                                       : vec3(1.00, 0.95, 0.80);
        col = mix(col, caretInk, caretAlpha);
    }

    // ─── Scanline tick (subtle, every frame) ─────────────────────────
    // A single bright row crawls top→bottom slowly — the "scan head"
    // of the data collector. Gives motion even at full silence.
    float scanY = mod(t * 0.18, 1.0) - 0.5;
    float scanD = abs(p.y - scanY);
    float scanA = exp(-scanD * 38.0) * 0.18;
    vec3 scanInk = (palIdx == 1) ? vec3(0.15, 0.10, 0.10)
                                  : vec3(0.95, 0.80, 0.55);
    col += scanInk * scanA * (0.4 + 0.8 * audio);

    // ─── Data noise grain — entropy channel as visible noise floor ───
    // Adds a soft chromatic grain weighted by data.entropy; reads as
    // "the system is uncertain". Never overwhelms; capped.
    float gn = hash12(gl_FragCoord.xy + vec2(t * 60.0));
    col += (gn - 0.5) * 0.06 * entropy;

    // ─── Vignette + tone curve ───────────────────────────────────────
    float r = length(uv - 0.5);
    col *= 1.0 - vigAmt * smoothstep(0.35, 0.85, r);

    // Reinhard-ish soft toe so brightnesses never clip.
    col = col / (1.0 + 0.55 * col);
    col = pow(max(col, 0.0), vec3(0.92));

    gl_FragColor = vec4(col, 1.0);
}
