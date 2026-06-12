/*{
  "DESCRIPTION": "Highlighted Text — marker-style word emphasis on warm paper. As the caption types in, colored highlight bands sweep behind each word from left to right with a felt-tip marker stroke (rough top/bottom edges, ink pooling at ends). Each word is assigned to a player slot (1→2→3→1…) so a three-way conversation paints the page in three different highlighter colors; the most-recently-spoken player's bands pulse and swell. Real layered depth: paper backdrop → highlight bands on their own z-plane → ink text on top → blue endpoint accent dots in front. Smooth fwidth-AA bands, audio-driven sweep, marker grain. Anti-pattern free: no bars, no EKG, no icons. Returns LINEAR HDR.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",        "LABEL": "Caption",          "TYPE": "text",  "DEFAULT": "STEDELIJK MUSEUM AMSTERDAM SAYS HELLO TODAY", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "energyA",    "LABEL": "Player 1 Energy",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",    "LABEL": "Player 2 Energy",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",    "LABEL": "Player 3 Energy",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "activeA",    "LABEL": "Player 1 Active",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].active" },
    { "NAME": "activeB",    "LABEL": "Player 2 Active",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].active" },
    { "NAME": "activeC",    "LABEL": "Player 3 Active",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].active" },

    { "NAME": "bassDrive",  "LABEL": "Bass Drive",       "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.bass" },

    { "NAME": "paperColor", "LABEL": "Paper",            "TYPE": "color", "DEFAULT": [0.965, 0.955, 0.935, 1.0] },
    { "NAME": "highlightA", "LABEL": "Highlight 1",      "TYPE": "color", "DEFAULT": [0.55, 0.78, 0.97, 1.0] },
    { "NAME": "highlightB", "LABEL": "Highlight 2",      "TYPE": "color", "DEFAULT": [0.99, 0.86, 0.32, 1.0] },
    { "NAME": "highlightC", "LABEL": "Highlight 3",      "TYPE": "color", "DEFAULT": [0.99, 0.55, 0.78, 1.0] },
    { "NAME": "inkColor",   "LABEL": "Ink",              "TYPE": "color", "DEFAULT": [0.04, 0.05, 0.10, 1.0] },
    { "NAME": "accentColor","LABEL": "Accent Dots",      "TYPE": "color", "DEFAULT": [0.10, 0.42, 0.95, 1.0] },

    { "NAME": "textScale",  "LABEL": "Text Size",        "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.5,  "MAX": 2.2 },
    { "NAME": "sweepSpeed", "LABEL": "Sweep Speed",      "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.1,  "MAX": 3.0 },
    { "NAME": "audioDepth", "LABEL": "Audio Depth",      "TYPE": "float", "DEFAULT": 0.6,  "MIN": 0.0,  "MAX": 2.0 },
    { "NAME": "opacity",    "LABEL": "Highlight Opacity","TYPE": "float", "DEFAULT": 0.78, "MIN": 0.20, "MAX": 1.00 },
    { "NAME": "grain",      "LABEL": "Marker Grain",     "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0,  "MAX": 1.2 },
    { "NAME": "tiltAmount", "LABEL": "Band Tilt",        "TYPE": "float", "DEFAULT": 0.06, "MIN": 0.0,  "MAX": 0.20 },
    { "NAME": "lineGap",    "LABEL": "Line Gap",         "TYPE": "float", "DEFAULT": 0.020,"MIN": 0.0,  "MAX": 0.080 },
    { "NAME": "kerning",    "LABEL": "Kerning",          "TYPE": "float", "DEFAULT": 0.88, "MIN": 0.55, "MAX": 1.30 }
  ]
}*/

// =====================================================================
// Highlighted Text — the highlight is the hero. Words are detected from
// the live caption, assigned round-robin to player slots, and painted
// with a marker swipe of that slot's color. The swipe animates left→
// right per word as the typewriter reveals it (msgAge); pulses on bass.
// Layered z: paper(0) → highlight bands(1) → ink text(2) → dots(3).
// =====================================================================

#define MAX_WALK    48
#define SPACE_CH    26
#define MAX_WORDS   12

const float TAU = 6.28318530718;

// ─── Font atlas ─────────────────────────────────────────────────────
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

int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 0;
    if (n > 48) return 48;
    return n;
}

// ─── Noise helpers (paper fibre + marker grain) ──────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float hash21(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
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

// Soft per-word color: round-robin assign words 0..N-1 to player slots.
vec3 wordHighlight(int wordIdx, vec3 cA, vec3 cB, vec3 cC,
                   float eA, float eB, float eC,
                   float aA, float aB, float aC) {
    int slot = wordIdx - (wordIdx / 3) * 3;
    if (slot == 0) return cA * (0.78 + 0.55 * eA + 0.30 * aA);
    if (slot == 1) return cB * (0.78 + 0.55 * eB + 0.30 * aB);
    return                cC * (0.78 + 0.55 * eC + 0.30 * aC);
}

// Returns which player slot owns the given word.
int wordSlot(int wordIdx) {
    return wordIdx - (wordIdx / 3) * 3;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    // Aspect-corrected, centered.
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    int   total   = charCount();
    bool  live    = (msgAge >= 0.0);
    float typeAge = max(msgAge, 0.0);

    // Per-player energies (declarative — never smoothed in shader).
    float eA = clamp(energyA, 0.0, 1.0);
    float eB = clamp(energyB, 0.0, 1.0);
    float eC = clamp(energyC, 0.0, 1.0);
    float aA = clamp(activeA, 0.0, 1.0);
    float aB = clamp(activeB, 0.0, 1.0);
    float aC = clamp(activeC, 0.0, 1.0);
    float bass = clamp(bassDrive, 0.0, 1.0) * audioDepth;

    // ── LAYER 0: paper backdrop ──────────────────────────────────────
    // Warm cream with subtle marbled fibre + a soft vignette so it
    // reads as a real sheet sitting under the bands. Always moving:
    // the fibre slowly drifts even at silence (intentional stillness).
    vec3 paper = paperColor.rgb;
    float fibre = fbm2(p * 3.2 + vec2(TIME * 0.04, -TIME * 0.03));
    paper *= 0.93 + 0.10 * fibre;
    // hand-laid paper edge shading
    float vign = 1.0 - 0.22 * dot(p, p);
    paper *= vign;
    // very faint warm wash from upper-left (studio light)
    float wash = exp(-length(p - vec2(-0.35, 0.30)) * 1.6);
    paper += wash * vec3(0.05, 0.035, 0.015);

    vec3 col = paper;

    if (total <= 0) {
        // No caption yet: paper still breathes + faint highlighter ghost
        // sweeps slowly across the page so the canvas is never frozen.
        float ghost = smoothstep(0.30, 0.05, abs(p.y - 0.0))
                    * smoothstep(-0.65, -0.55, p.x - 0.35 * sin(TIME * 0.25));
        col = mix(col, mix(highlightA.rgb, highlightB.rgb, 0.5),
                  ghost * 0.06);
        gl_FragColor = vec4(col, 1.0);
        return;
    }

    // ── Pre-pass: walk the caption, find each word's [start,end] char
    //    indices and the typewriter reveal index. Layout the words into
    //    rows so highlight bands match the wrapped text exactly.
    int wordStart[MAX_WORDS];
    int wordEnd  [MAX_WORDS];
    int wordRow  [MAX_WORDS];
    int wordCol  [MAX_WORDS];   // starting column on its row
    int wordLen  [MAX_WORDS];   // chars in the word
    for (int i = 0; i < MAX_WORDS; i++) {
        wordStart[i] = 0; wordEnd[i] = 0;
        wordRow[i] = 0; wordCol[i] = 0; wordLen[i] = 0;
    }
    int wordCount = 0;

    // Glyph metrics. textScale controls visual size; lineGap controls
    // vertical spacing between rows. Bass adds a tiny continuous swell.
    float sf       = clamp(textScale, 0.5, 2.2);
    float charH    = 0.075 * sf * (1.0 + 0.04 * bass);
    float charW    = charH * (5.0 / 7.0);
    float kern     = charW * clamp(kerning, 0.55, 1.30);
    float lineH    = charH + clamp(lineGap, 0.0, 0.080);

    // Page interior. Bands extend slightly past glyph extent for the
    // "wider than the word" highlighter look.
    float pageHalfW = (aspect * 0.5) - 0.10;
    if (pageHalfW < 0.10) pageHalfW = 0.10;
    float pageHalfH = 0.42;
    float charsPerRow = max(2.0, floor((pageHalfW * 2.0) / kern));

    // First pass: discover word boundaries; assign row+column via simple
    // word-wrap (whole words never split).
    {
        int curRow = 0;
        int curCol = 0;
        int idx = 0;
        bool inWord = false;
        int wStartIdx = 0;
        int wStartCol = 0;
        for (int i = 0; i < MAX_WALK; i++) {
            if (idx >= total) break;
            int ch = getChar(idx);
            bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
            if (!isSpace) {
                if (!inWord) {
                    // measure word length ahead so we can decide wrap
                    int wlen = 0;
                    for (int j = 0; j < MAX_WALK; j++) {
                        int kk = idx + j;
                        if (kk >= total) break;
                        int cj = getChar(kk);
                        if (cj == SPACE_CH || cj < 0 || cj > 36) break;
                        wlen++;
                    }
                    // wrap if word can't fit
                    if (curCol > 0 && float(curCol + wlen) > charsPerRow) {
                        curRow++;
                        curCol = 0;
                    }
                    inWord = true;
                    wStartIdx = idx;
                    wStartCol = curCol;
                }
                curCol++;
                idx++;
            } else {
                if (inWord) {
                    if (wordCount < MAX_WORDS) {
                        wordStart[wordCount] = wStartIdx;
                        wordEnd  [wordCount] = idx;
                        wordRow  [wordCount] = curRow;
                        wordCol  [wordCount] = wStartCol;
                        wordLen  [wordCount] = idx - wStartIdx;
                        wordCount++;
                    }
                    inWord = false;
                }
                // single space advances column unless wrap pending
                if (curCol > 0 && curCol < int(charsPerRow)) curCol++;
                idx++;
            }
        }
        if (inWord && wordCount < MAX_WORDS) {
            wordStart[wordCount] = wStartIdx;
            wordEnd  [wordCount] = idx;
            wordRow  [wordCount] = curRow;
            wordCol  [wordCount] = wStartCol;
            wordLen  [wordCount] = idx - wStartIdx;
            wordCount++;
        }
    }

    int totalRows = 1;
    for (int i = 0; i < MAX_WORDS; i++) {
        if (i >= wordCount) break;
        if (wordRow[i] + 1 > totalRows) totalRows = wordRow[i] + 1;
    }

    // Typewriter reveal: how many characters of the message have been
    // shown so far. ~28 chars/sec matches Easel's typewriter cps.
    float CPS = 28.0;
    float revealedChars = live ? min(typeAge * CPS, float(total)) : float(total);

    // Vertical block offset so text sits centered on the page.
    float blockH    = float(totalRows) * lineH;
    float blockTopY = blockH * 0.5;   // y of the TOP of the first row
    // (we draw rows top→down: row 0 sits highest)

    // ── LAYER 1: highlight bands ─────────────────────────────────────
    // For each word: compute its rect in p-space; build an SDF that
    // accepts the band; sweep its right-edge in with msgAge; soften
    // edges with fwidth + marker grain so it reads as a felt-tip mark.
    vec3  bandCol = vec3(0.0);
    float bandAlpha = 0.0;
    float bandZ = 0.0;       // hottest band's "z presence" (for shadow)

    for (int w = 0; w < MAX_WORDS; w++) {
        if (w >= wordCount) break;

        // Word rect in p-space.
        float rowY = blockTopY - (float(wordRow[w]) + 0.5) * lineH;
        // Centerline of the word.
        float x0   = -pageHalfW + (float(wordCol[w])              ) * kern;
        float x1   = -pageHalfW + (float(wordCol[w] + wordLen[w]) ) * kern;
        float cx   = 0.5 * (x0 + x1);
        float halfW= 0.5 * (x1 - x0) + 0.6 * kern;   // wider than word
        float halfH= charH * 0.62;                   // a bit shorter

        // Band tilts slightly per word (deterministic, gentle).
        float tilt = (hash11(float(w) * 7.13) - 0.5) * 2.0 * tiltAmount;
        // Local coords inside the band frame.
        vec2 q = p - vec2(cx, rowY);
        float ct = cos(tilt), st = sin(tilt);
        vec2 qr = vec2(ct * q.x - st * q.y, st * q.x + ct * q.y);

        // Sweep progress: how far the highlighter has crossed this word.
        // Driven by the typewriter — when revealedChars reaches the word's
        // first char, sweep starts; reaches its last char → fully swept.
        float wStartChar = float(wordStart[w]);
        float wEndChar   = float(wordEnd  [w]);
        float charsAcrossWord = max(wEndChar - wStartChar, 1.0);
        float sweep = clamp((revealedChars - wStartChar)
                          / (charsAcrossWord / max(sweepSpeed, 0.1)),
                          0.0, 1.0);
        // Ease the sweep so it lingers at the end (marker pooling).
        float ease = sweep * sweep * (3.0 - 2.0 * sweep);
        // Map sweep into local-x: -halfW → +halfW
        float swipeX = mix(-halfW - 0.02, halfW + 0.02, ease);

        // Inside the rect AND to the left of swipeX
        float dx = max(qr.x - halfW, -halfW - qr.x);   // signed in/out X
        float dy = max(qr.y - halfH, -halfH - qr.y);   // signed in/out Y
        // Rounded rect SDF (slightly rounded corners feel hand-drawn)
        float rad = 0.012;
        vec2 d2 = vec2(max(dx, 0.0), max(dy, 0.0));
        float outDist = length(d2) - rad;
        float inDist  = min(max(dx, dy) + rad, 0.0);
        float rectSdf = outDist + inDist;

        // Rough top/bottom edges (marker isn't a printer): perturb the
        // SDF with low-freq noise along x so the band edge wobbles.
        float edgeWob = (fbm2(qr * vec2(18.0, 4.0) + float(w) * 31.7) - 0.5)
                      * 0.010 * (0.6 + 1.4 * grain);
        rectSdf += edgeWob;

        // Cut by the sweep line.
        float swipeMask = smoothstep(swipeX + 0.008,
                                     swipeX - 0.008,
                                     qr.x);

        // Anti-aliased fill.
        float fw = max(fwidth(rectSdf), 1e-4);
        float fill = 1.0 - smoothstep(-fw, fw, rectSdf);
        fill *= swipeMask;
        if (fill < 0.0015) continue;

        // Marker grain: streak the inside of the band along x so it has
        // the "two passes of highlighter" striation.
        float streak = 0.7 + 0.3 *
            fbm2(vec2(qr.x * 90.0, qr.y * 18.0) + float(w) * 12.4);
        // End-pooling: darker/more-saturated ink at sweep tip.
        float tipPool = smoothstep(swipeX - 0.020, swipeX, qr.x);
        tipPool *= (1.0 - ease) + ease * 0.35;  // fades as word settles

        // Per-word color from its assigned player slot.
        vec3 hcol = wordHighlight(w,
                                  highlightA.rgb, highlightB.rgb, highlightC.rgb,
                                  eA, eB, eC, aA, aB, aC);
        // Active speaker subtly brighter; rest energy lifts a touch.
        int slot = wordSlot(w);
        float slotE = (slot == 0) ? eA : (slot == 1) ? eB : eC;
        float slotA = (slot == 0) ? aA : (slot == 1) ? aB : aC;
        hcol *= 1.0 + 0.12 * slotA + 0.10 * bass;

        // Apply streak + tip pool.
        vec3 banded = hcol * mix(0.85, 1.05, streak);
        banded *= mix(1.0, 0.78, tipPool * (1.0 - grain * 0.4));

        float a = fill * clamp(opacity, 0.2, 1.0);
        // Slight extra opacity for active speaker.
        a = clamp(a * (1.0 + 0.10 * slotA), 0.0, 1.0);

        // Z-stacking: front-to-back across words. Hottest active wins
        // any equal-fill ties (purely visual ordering, no real z buffer).
        // Composite over running band.
        bandCol   = mix(bandCol, banded, a * (1.0 - bandAlpha));
        bandAlpha = bandAlpha + a * (1.0 - bandAlpha);
        bandZ     = max(bandZ, a * (0.5 + 0.5 * slotE));
    }

    // Drop a soft cast shadow from the band onto the paper (band depth).
    if (bandAlpha > 0.001) {
        // shadow is a copy of the bandAlpha but smeared down + softened.
        // Cheap: re-evaluate a quick blur via fbm-tinted lift on paper.
        float lift = bandAlpha * 0.18 * bandZ;
        col *= 1.0 - lift * 0.12;
    }
    col = mix(col, bandCol, bandAlpha);

    // ── LAYER 2: ink text on top ─────────────────────────────────────
    // Render the caption glyph-by-glyph using the same row+column grid
    // the highlight pass used, so text sits exactly on top of its band.
    float ink = 0.0;
    {
        // Where is this pixel inside the block?
        // p.y up-positive; rows count down from top.
        float ly = blockTopY - p.y;   // distance from top of block
        int  row = int(floor(ly / lineH));
        if (row >= 0 && row < totalRows) {
            float rowTopY = blockTopY - float(row) * lineH;
            float rowCenterY = rowTopY - lineH * 0.5;
            float yInGlyph = (rowCenterY - p.y) + charH * 0.5;
            // yInGlyph 0..charH = the glyph cell vertically.
            if (yInGlyph >= 0.0 && yInGlyph <= charH) {
                float lx = p.x + pageHalfW;
                int colIdx = int(floor(lx / kern));
                if (colIdx >= 0 && float(colIdx) < charsPerRow) {
                    // Walk the message and find what char sits at
                    // (row, colIdx). Same wrap rules as pre-pass.
                    int curRow2 = 0;
                    int curCol2 = 0;
                    int idx2 = 0;
                    int outCh = -1;
                    bool inWord2 = false;
                    for (int i = 0; i < MAX_WALK; i++) {
                        if (idx2 >= total) break;
                        if (curRow2 > row) break;
                        int ch = getChar(idx2);
                        bool isSp = (ch == SPACE_CH || ch < 0 || ch > 36);
                        if (!isSp) {
                            if (!inWord2) {
                                int wlen = 0;
                                for (int j = 0; j < MAX_WALK; j++) {
                                    int kk = idx2 + j;
                                    if (kk >= total) break;
                                    int cj = getChar(kk);
                                    if (cj == SPACE_CH || cj < 0 || cj > 36) break;
                                    wlen++;
                                }
                                if (curCol2 > 0 && float(curCol2 + wlen) > charsPerRow) {
                                    curRow2++;
                                    curCol2 = 0;
                                    if (curRow2 > row) break;
                                }
                                inWord2 = true;
                            }
                            if (curRow2 == row && curCol2 == colIdx) {
                                outCh = ch;
                            }
                            // typewriter: only show revealed chars
                            if (float(idx2) >= revealedChars) {
                                outCh = (curRow2 == row && curCol2 == colIdx) ? -1 : outCh;
                            }
                            curCol2++;
                            idx2++;
                        } else {
                            if (inWord2) { inWord2 = false; }
                            if (curCol2 > 0 && curCol2 < int(charsPerRow)) {
                                if (curRow2 == row && curCol2 == colIdx) {
                                    outCh = SPACE_CH;
                                }
                                curCol2++;
                            }
                            idx2++;
                        }
                    }
                    if (outCh >= 0 && outCh <= 35 && outCh != SPACE_CH) {
                        // Glyph cell local coords. Column position within
                        // its kern cell, centered, glyph width slightly
                        // narrower than kern.
                        float colLeft = float(colIdx) * kern;
                        float pad = (kern - charW) * 0.5;
                        float cellX = (lx - colLeft - pad) / charW;
                        float cellY = 1.0 - yInGlyph / charH;
                        float s = sampleChar(outCh, vec2(cellX, cellY));
                        s = smoothstep(0.22, 0.55, s);
                        ink = max(ink, s);
                    }
                }
            }
        }
    }
    if (ink > 0.001) {
        col = mix(col, inkColor.rgb, ink);
    }

    // ── LAYER 3: blue accent dots (in FRONT of everything) ───────────
    // Two dots — at the start of the first highlighted word, and the end
    // of the last. They mark cue endpoints (where the marker entered/
    // left the page). They wobble with audio so the layer reads alive.
    if (wordCount > 0) {
        // Start dot
        {
            int w = 0;
            float rowY = blockTopY - (float(wordRow[w]) + 0.5) * lineH;
            float x0 = -pageHalfW + (float(wordCol[w])) * kern;
            float halfH = charH * 0.62;
            vec2 dpos = vec2(x0 - kern * 0.25, rowY + halfH + 0.018);
            dpos += vec2(0.003 * sin(TIME * 0.7),
                         0.003 * cos(TIME * 0.5)) * (1.0 + bass);
            float r = 0.022 * (1.0 + 0.12 * bass + 0.10 * aA);
            float d = length(p - dpos);
            float aa = max(fwidth(d), 1e-4);
            float dotFill = 1.0 - smoothstep(r - aa, r + aa, d);
            // halo
            float halo = exp(-d * 30.0) * 0.25 * (0.4 + 0.6 * eA);
            col += accentColor.rgb * halo;
            col = mix(col, accentColor.rgb, dotFill);
        }
        // End dot
        {
            int w = wordCount - 1;
            float rowY = blockTopY - (float(wordRow[w]) + 0.5) * lineH;
            float x1 = -pageHalfW + (float(wordCol[w] + wordLen[w])) * kern;
            float halfH = charH * 0.62;
            vec2 dpos = vec2(x1 + kern * 0.25, rowY - halfH - 0.018);
            dpos += vec2(0.003 * cos(TIME * 0.6),
                         0.003 * sin(TIME * 0.8)) * (1.0 + bass);
            float r = 0.022 * (1.0 + 0.12 * bass + 0.10 * aC);
            float d = length(p - dpos);
            float aa = max(fwidth(d), 1e-4);
            float dotFill = 1.0 - smoothstep(r - aa, r + aa, d);
            float halo = exp(-d * 30.0) * 0.25 * (0.4 + 0.6 * eC);
            col += accentColor.rgb * halo;
            col = mix(col, accentColor.rgb, dotFill);
        }
    }

    // Final: gentle canvas tooth so nothing reads as flat pixels.
    float tooth = fbm2(p * 220.0);
    col *= 1.0 + (tooth - 0.5) * 0.025;

    // Soft tone, never crush.
    col = col / (1.0 + 0.10 * col);
    col = pow(max(col, 0.0), vec3(0.96));

    gl_FragColor = vec4(col, 1.0);
}
