/*{
  "DESCRIPTION": "Moving Circular Grid · Text — a drifting matrix of circular research cells where each cell holds a different circle treatment (halftone, concentric rings, gradient lens, eclipse crescent, ring-arc, dot mandala, scan-line disc, rotor, ripple corona, vesica chevron) and the whole grid breathes with parallax bands. Cells randomly POP — bursting into a brief enlarged variant — driven by per-player energy. Three depth bands stack back-to-front; players A/B/C each own one band so their pops read as 'that voice'. Audio.bass thickens stroke weight, audio.high crisps highlights. The live cue line types into one randomly-elected pop-cell as a slab caption — never the center, never a logo. Premium fwidth-AA SDFs throughout. Returns LINEAR HDR.",
  "CREDIT": "ShaderClaw — A-List drop · moving_circular_grid_text",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "LABEL": "Caption", "TYPE": "text", "DEFAULT": "MOVING GRID · CIRCULAR RESEARCH", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "energyA", "LABEL": "Player 1 (Back band)",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB", "LABEL": "Player 2 (Mid band)",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC", "LABEL": "Player 3 (Front band)", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "activeA", "LABEL": "Player 1 Active",       "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].active" },
    { "NAME": "activeB", "LABEL": "Player 2 Active",       "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].active" },

    { "NAME": "bassDrive", "LABEL": "Bass (Stroke Weight)", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.bass" },
    { "NAME": "highDrive", "LABEL": "High (Crisp / Pop)",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.high" },

    { "NAME": "cellCount",   "LABEL": "Cells per Row",    "TYPE": "long",  "DEFAULT": 5, "VALUES": [3,4,5,6,7,8], "LABELS": ["3","4","5","6","7","8"] },
    { "NAME": "randomSeed",  "LABEL": "Random Seed",      "TYPE": "float", "DEFAULT": 7.0, "MIN": 0.0, "MAX": 64.0 },
    { "NAME": "palette",     "LABEL": "Palette",          "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Paper","Editorial","Acid","Mono"] },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",     "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 1.6 },
    { "NAME": "audioDepth",  "LABEL": "Audio Depth",      "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "parallax",    "LABEL": "Parallax",         "TYPE": "float", "DEFAULT": 0.9, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "popAmount",   "LABEL": "Pop Amount",       "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "captionScale","LABEL": "Caption Scale",    "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.5, "MAX": 1.8 },
    { "NAME": "kerning",     "LABEL": "Kerning",          "TYPE": "float", "DEFAULT": 0.95, "MIN": 0.55, "MAX": 1.4 },
    { "NAME": "paperColor",  "LABEL": "Paper",            "TYPE": "color", "DEFAULT": [0.945, 0.935, 0.910, 1.0] },
    { "NAME": "inkColor",    "LABEL": "Ink",              "TYPE": "color", "DEFAULT": [0.050, 0.050, 0.070, 1.0] }
  ]
}*/

// ═══════════════════════════════════════════════════════════════════════════
//  MOVING CIRCULAR GRID · TEXT
//  Drifting matrix of circle-research cells. 10 distinct cell variants
//  scrambled by seed; random POP events flash enlarged variants in tempo
//  with per-player energy. Three parallax depth bands (Back/Mid/Front),
//  one per player. Cue caption lands in one elected pop-cell as a slab.
//  Premium fwidth-AA SDFs, gallery-grade composition.
// ═══════════════════════════════════════════════════════════════════════════

#define MAX_WALK   48
#define SPACE_CH   26
#define VARIANT_N  10
const float TAU = 6.28318530718;

// ─── Font atlas + msg routing ─────────────────────────────────────────────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}
int getChar(int slot) {
    if (slot ==  0) return int(msg_0);  if (slot ==  1) return int(msg_1);
    if (slot ==  2) return int(msg_2);  if (slot ==  3) return int(msg_3);
    if (slot ==  4) return int(msg_4);  if (slot ==  5) return int(msg_5);
    if (slot ==  6) return int(msg_6);  if (slot ==  7) return int(msg_7);
    if (slot ==  8) return int(msg_8);  if (slot ==  9) return int(msg_9);
    if (slot == 10) return int(msg_10); if (slot == 11) return int(msg_11);
    if (slot == 12) return int(msg_12); if (slot == 13) return int(msg_13);
    if (slot == 14) return int(msg_14); if (slot == 15) return int(msg_15);
    if (slot == 16) return int(msg_16); if (slot == 17) return int(msg_17);
    if (slot == 18) return int(msg_18); if (slot == 19) return int(msg_19);
    if (slot == 20) return int(msg_20); if (slot == 21) return int(msg_21);
    if (slot == 22) return int(msg_22); if (slot == 23) return int(msg_23);
    if (slot == 24) return int(msg_24); if (slot == 25) return int(msg_25);
    if (slot == 26) return int(msg_26); if (slot == 27) return int(msg_27);
    if (slot == 28) return int(msg_28); if (slot == 29) return int(msg_29);
    if (slot == 30) return int(msg_30); if (slot == 31) return int(msg_31);
    if (slot == 32) return int(msg_32); if (slot == 33) return int(msg_33);
    if (slot == 34) return int(msg_34); if (slot == 35) return int(msg_35);
    if (slot == 36) return int(msg_36); if (slot == 37) return int(msg_37);
    if (slot == 38) return int(msg_38); if (slot == 39) return int(msg_39);
    if (slot == 40) return int(msg_40); if (slot == 41) return int(msg_41);
    if (slot == 42) return int(msg_42); if (slot == 43) return int(msg_43);
    if (slot == 44) return int(msg_44); if (slot == 45) return int(msg_45);
    if (slot == 46) return int(msg_46); if (slot == 47) return int(msg_47);
    return -1;
}
int charCount() { int n = int(msg_len); if (n <= 0) return 0; if (n > 48) return 48; return n; }

// ─── Hashes / noise ──────────────────────────────────────────────────────
float h11(float n) { return fract(sin(n * 91.3458) * 47453.5453); }
float h21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
vec2  h22(vec2 p)  { return fract(sin(vec2(dot(p, vec2(127.1,311.7)),
                                           dot(p, vec2(269.5,183.3)))) * 43758.5453); }
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float a = h21(i), b = h21(i+vec2(1.0,0.0));
    float c = h21(i+vec2(0.0,1.0)), d = h21(i+vec2(1.0,1.0));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}

// ─── Palettes ────────────────────────────────────────────────────────────
vec3 palettePaper(float t) {
    // warm paper + editorial accents (red/yellow/green/blue) — like ref.
    vec3 r = vec3(0.93, 0.30, 0.28);
    vec3 y = vec3(0.96, 0.82, 0.30);
    vec3 g = vec3(0.28, 0.62, 0.42);
    vec3 b = vec3(0.30, 0.52, 0.86);
    float k = fract(t);
    if (k < 0.25)      return mix(r, y, k/0.25);
    else if (k < 0.50) return mix(y, g, (k-0.25)/0.25);
    else if (k < 0.75) return mix(g, b, (k-0.50)/0.25);
    else               return mix(b, r, (k-0.75)/0.25);
}
vec3 paletteEditorial(float t) {
    return 0.6 + 0.4*cos(TAU*(t + vec3(0.0, 0.18, 0.42)));
}
vec3 paletteAcid(float t) {
    return 0.5 + 0.5*cos(TAU*(t*1.4 + vec3(0.1, 0.55, 0.85)));
}
vec3 paletteMono(float t) {
    float k = 0.18 + 0.55*fract(t*1.7);
    return vec3(k);
}
vec3 paletteOf(int mode, float t) {
    if (mode == 1) return paletteEditorial(t);
    if (mode == 2) return paletteAcid(t);
    if (mode == 3) return paletteMono(t);
    return palettePaper(t);
}

// ─── Cell SDF variants ───────────────────────────────────────────────────
// All cells live in local (cx,cy) ∈ [-1,1]; return (mask, accent) where
// mask ∈ [0,1] is the body fill and accent ∈ [0,1] is the highlight/edge.
// Stroke weight `sw` scales with bass; `crisp` (high) sharpens AA gradient.
// Time `t` drives intra-cell animation. Variant id v selects treatment.

float ring(float d, float r, float w, float sw) {
    return 1.0 - smoothstep(0.0, sw, abs(d-r) - w);
}
float disc(float d, float r, float sw) {
    return 1.0 - smoothstep(0.0, sw, d-r);
}

// helper: 2D rotation
vec2 rot2(vec2 p, float a) {
    float c=cos(a), s=sin(a);
    return vec2(c*p.x - s*p.y, s*p.x + c*p.y);
}

// Each variant returns vec2(bodyMask, edgeMask).
vec2 variantCell(int v, vec2 lp, float t, float sw, float crisp) {
    float d = length(lp);
    float ang = atan(lp.y, lp.x);
    float fw = max(fwidth(d), 1e-4) * (1.0 + 1.2*(1.0-crisp));
    vec2 outc = vec2(0.0);

    if (v == 0) {
        // concentric rings, slowly rotating
        float a = 0.0;
        for (int i=0;i<4;i++){
            float r = 0.20 + 0.18*float(i);
            a = max(a, ring(d, r, 0.025 + 0.020*sw, fw));
        }
        float dot0 = disc(d, 0.06, fw);
        outc = vec2(max(dot0*0.6, a*0.85), a);
    } else if (v == 1) {
        // halftone dot grid clipped to disc
        vec2 g = lp*4.0;
        vec2 gi = floor(g), gf = fract(g)-0.5;
        float r = 0.18 + 0.16*sin(t*0.9 + h21(gi)*6.2832);
        float dot = 1.0 - smoothstep(0.0, fw*6.0, length(gf) - r*0.30);
        float mask = disc(d, 0.78, fw);
        outc = vec2(dot*mask, mask*0.30);
    } else if (v == 2) {
        // gradient lens — half-filled bowl
        float bowl = smoothstep(0.0, 1.0, 0.55 - lp.y*0.9);
        float m = disc(d, 0.78, fw);
        outc = vec2(bowl*m, m*0.6);
    } else if (v == 3) {
        // eclipse crescent — disc minus offset disc
        vec2 off = vec2(0.18*sin(t*0.7), 0.10*cos(t*0.6));
        float a = disc(d, 0.78, fw);
        float b = disc(length(lp - off), 0.62, fw);
        float crescent = clamp(a - b, 0.0, 1.0);
        outc = vec2(crescent, a*0.4);
    } else if (v == 4) {
        // ring-arc: stroke ring with mask sweep
        float r = 0.62 + 0.06*sin(t*0.5);
        float rg = ring(d, r, 0.045 + 0.045*sw, fw);
        float sweep = step(0.0, sin(ang*1.0 + t*0.8));
        outc = vec2(rg*sweep, rg);
    } else if (v == 5) {
        // dot mandala: ring of dots
        float n = 12.0;
        float a2 = mod(ang + t*0.4, TAU/n) - 0.5*TAU/n;
        vec2 dp = vec2(cos(a2), sin(a2)) * 0.58;
        float dotR = 0.075 + 0.025*sin(t*1.3);
        float dotM = 1.0 - smoothstep(0.0, fw*4.0, length(lp - dp*length(lp)/0.58) - dotR*length(lp)/0.78);
        // Simpler: place 12 dots on a ring
        float md = 1e3;
        for (int i=0;i<12;i++){
            float aa = (TAU/12.0)*float(i) + t*0.4;
            vec2 dq = vec2(cos(aa), sin(aa))*0.58;
            md = min(md, length(lp-dq));
        }
        float dots = 1.0 - smoothstep(0.0, fw*2.0, md - (0.075 + 0.020*sw));
        float center = disc(d, 0.10, fw);
        outc = vec2(max(dots, center*0.6), dots);
    } else if (v == 6) {
        // scan-line disc: horizontal bands inside circle
        float bands = 0.5 + 0.5*sin(lp.y*22.0 - t*1.8);
        bands = smoothstep(0.30, 0.70, bands);
        float m = disc(d, 0.78, fw);
        outc = vec2(bands*m, m*0.35);
    } else if (v == 7) {
        // rotor / gear-tooth ring
        float teeth = 0.5 + 0.5*cos(ang*8.0 + t*0.6);
        teeth = smoothstep(0.45, 0.80, teeth);
        float rng = ring(d, 0.58, 0.10 + 0.04*sw, fw);
        float core = disc(d, 0.22, fw);
        outc = vec2(rng*teeth + core*0.7, rng);
    } else if (v == 8) {
        // ripple corona: concentric soft waves
        float w = 0.5 + 0.5*sin(d*22.0 - t*2.2);
        w *= smoothstep(0.85, 0.20, d);
        outc = vec2(w*0.9, w*0.6);
    } else {
        // vesica chevron: rotated lens shape
        vec2 q = rot2(lp, t*0.25);
        float a = length(q - vec2(0.22, 0.0)) - 0.65;
        float b = length(q + vec2(0.22, 0.0)) - 0.65;
        float vesica = max(a, b);
        float m = 1.0 - smoothstep(0.0, fw, vesica);
        outc = vec2(m, m*0.5);
    }
    return outc;
}

// ─── Caption typesetting (single elected cell) ──────────────────────────
// Draws a slab caption inside a unit-square local frame (-1..1). Returns
// mask (1 inside glyph) so caller composites with chosen ink color.
float drawCaption(vec2 lp, int total, float scale, float kern) {
    if (total <= 0) return 0.0;
    // Box inset within the cell (square, 90% wide).
    float boxHalf = 0.78;
    float boxW = boxHalf * 2.0;
    if (abs(lp.x) > boxHalf || abs(lp.y) > boxHalf) return 0.0;

    // Word-wrap pre-pass: longest word + simple square block sizing.
    int longestWord = 0;
    {
        int run = 0;
        for (int i=0;i<MAX_WALK;i++){
            if (i>=total) break;
            int ch = getChar(i);
            bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
            if (isSpace) { run = 0; }
            else { run++; if (run > longestWord) longestWord = run; }
        }
    }
    float baseCols = ceil(sqrt(float(total) * 1.6));
    int charsPerRow = int(ceil(baseCols / max(scale, 0.5)));
    if (charsPerRow < longestWord) charsPerRow = longestWord;
    if (charsPerRow > 32) charsPerRow = 32;
    if (charsPerRow < 1)  charsPerRow = 1;

    // Pre-pass: count rows used at this width.
    int usedRows = 1;
    {
        int preR = 0, preC = 0;
        for (int i=0;i<MAX_WALK;i++){
            if (i>=total) break;
            int ch = getChar(i);
            if (ch == SPACE_CH) {
                int wlen = 0;
                for (int j=1;j<MAX_WALK;j++){
                    int jj = i+j; if (jj>=total) break;
                    int chj = getChar(jj);
                    if (chj==SPACE_CH || chj<0 || chj>36) break;
                    wlen++;
                }
                if (preC > 0 && preC + 1 + wlen > charsPerRow) { preR++; preC = 0; }
                else if (preC > 0) { preC++; }
            } else if (ch >= 0 && ch <= 36) {
                preC++;
                if (preC >= charsPerRow) { preR++; preC = 0; }
            }
        }
        usedRows = preR + 1;
    }

    // Glyph sizing.
    float effKern  = boxW / float(charsPerRow);
    float effCharW = effKern / max(kern, 0.55);
    float effCharH = effCharW * (7.0/5.0);
    const float LEADING = 1.18;
    float linePitch = effCharH * LEADING;
    float blockH = float(usedRows) * linePitch;
    if (blockH > boxW) {
        float shrink = boxW / blockH;
        effCharH *= shrink; effCharW *= shrink; linePitch *= shrink; blockH = boxW;
    }
    effCharW = min(effCharW, effCharH*(5.0/7.0));
    float yOff = (boxW - blockH)*0.5;

    // top-left origin
    float lx = lp.x + boxHalf;
    float ly = (boxHalf - lp.y) - yOff;
    if (lx < 0.0 || lx > boxW) return 0.0;
    if (ly < 0.0 || ly > blockH) return 0.0;

    int targetCol = int(floor(lx / effKern));
    int targetRow = int(floor(ly / linePitch));
    if (targetCol >= charsPerRow) return 0.0;
    if (targetRow >= usedRows)    return 0.0;

    float rowPad = (linePitch - effCharH)*0.5;
    float yInRow = (ly - float(targetRow)*linePitch) - rowPad;
    if (yInRow < 0.0 || yInRow > effCharH) return 0.0;

    // Walk to (targetRow, targetCol) with the same wrap rules.
    int cursorR = 0, cursorC = 0;
    int outCh = -1;
    for (int i=0;i<MAX_WALK;i++){
        if (i>=total) break;
        if (cursorR > targetRow) break;
        int ch = getChar(i);
        if (ch == SPACE_CH) {
            int wlen = 0;
            for (int j=1;j<MAX_WALK;j++){
                int jj = i+j; if (jj>=total) break;
                int chj = getChar(jj);
                if (chj==SPACE_CH || chj<0 || chj>36) break;
                wlen++;
            }
            if (cursorC > 0 && cursorC + 1 + wlen > charsPerRow) {
                cursorR++; cursorC = 0;
            } else if (cursorC > 0) {
                if (cursorR == targetRow && cursorC == targetCol) outCh = SPACE_CH;
                cursorC++;
            }
        } else if (ch >= 0 && ch <= 36) {
            if (cursorR == targetRow && cursorC == targetCol) outCh = ch;
            cursorC++;
            if (cursorC >= charsPerRow) { cursorR++; cursorC = 0; }
        }
    }
    if (outCh < 0 || outCh > 35 || outCh == SPACE_CH) return 0.0;

    float colPad = (effKern - effCharW)*0.5;
    vec2 cellLocal = vec2((lx - float(targetCol)*effKern - colPad)/effCharW,
                          1.0 - yInRow/effCharH);
    float s = sampleChar(outCh, cellLocal);
    return smoothstep(0.18, 0.55, s);
}

// ─── MAIN ─────────────────────────────────────────────────────────────────
void main() {
    vec2 res = RENDERSIZE;
    vec2 fragUV = gl_FragCoord.xy / res;
    float aspect = res.x / max(res.y, 1.0);

    // Centered, aspect-corrected coord in [-aspect/2, aspect/2] × [-0.5, 0.5]
    vec2 p;
    p.x = (fragUV.x - 0.5) * aspect;
    p.y = (fragUV.y - 0.5);

    float t = TIME * max(motionSpeed, 0.0);
    float bass  = clamp(bassDrive,  0.0, 1.0);
    float high  = clamp(highDrive,  0.0, 1.0);
    float aDep  = clamp(audioDepth, 0.0, 2.0);
    float seed  = randomSeed;

    float eA = clamp(energyA, 0.0, 1.0);
    float eB = clamp(energyB, 0.0, 1.0);
    float eC = clamp(energyC, 0.0, 1.0);
    float aAact = max(activeA, eA);
    float aBact = max(activeB, eB);
    int paletteMode = clamp(int(palette), 0, 3);

    // ── Paper backdrop (subtle warm) ──────────────────────────────────
    vec3 paper = paperColor.rgb;
    paper *= 1.0 - 0.06 * dot(p, p);  // gentle vignette
    paper += 0.012 * (vnoise(p*40.0 + seed) - 0.5); // paper tooth
    vec3 col = paper;

    // ── Three parallax bands ──────────────────────────────────────────
    // Each band has its own cell count, offset, and player owner. Back
    // largest cells (sparse), Front smallest cells (dense). Bands are
    // drawn back→front and composited with alpha so depth reads.
    int baseCells = clamp(int(cellCount), 3, 8);

    // Per-band parameters: bandScale (cell size), bandShift (parallax px),
    // bandTint (color bias toward palette index), bandOwner energy/active.
    for (int band = 0; band < 3; band++) {
        float fb = float(band);
        // band 0 = back (large cells, slow), band 2 = front (small cells, fast)
        int cellsRow = baseCells + band;             // 3→5, 4→6 etc.
        float bandSpeed = 0.20 + 0.30*fb;
        float bandPx    = 0.05 + 0.07*fb;             // parallax magnitude
        float bandScale = mix(1.4, 0.85, fb*0.5);     // larger back

        // owner energy / active
        float bandEnergy = (band == 0) ? eA : ((band == 1) ? eB : eC);
        float bandActive = (band == 0) ? aAact : ((band == 1) ? aBact : eC);

        // Audio-energy aware drift: each band drifts in its own direction,
        // and the *speed* of drift modulates with energy + bass so motion
        // is intentional (silence = nearly still, crescendo = fast).
        float idle = 0.04 + 0.06*fb;
        float speed = idle + 0.55 * bandEnergy * aDep + 0.20 * bass * aDep;
        vec2 drift = vec2(cos(fb*1.7 + seed*0.3), sin(fb*1.9 + seed*0.4)) * speed;
        vec2 bp = p * bandScale * float(cellsRow) - drift * t * parallax;

        // parallax band offset relative to cursor for pseudo-3D
        bp += (vec2(sin(t*0.20+fb), cos(t*0.17+fb)) * bandPx * parallax);

        // grid coords
        vec2 gi = floor(bp);
        vec2 gf = fract(bp) - 0.5;
        gf *= 2.0;   // → [-1,1]

        // per-cell seed (stable id mixed with global seed + band)
        float cid = h21(gi + vec2(seed*1.31, fb*23.7));
        // variant pick — 10 variants, scrambled by seed
        int v = int(floor(cid * float(VARIANT_N)));
        v = int(mod(float(v) + floor(seed), float(VARIANT_N)));

        // per-cell phase for the pop event
        float popSeed = h21(gi + vec2(7.31, fb*11.0 + seed*0.97));
        // pop event period (each cell has its own loop): pops are short
        // bursts that scale up + brighten the cell. Energy speeds events.
        float popPeriod = mix(6.0, 2.0, bandEnergy);
        float popPhase  = fract((t + popSeed*popPeriod) / popPeriod);
        float popEnv    = smoothstep(0.0, 0.06, popPhase) *
                          (1.0 - smoothstep(0.06, 0.32, popPhase));
        popEnv *= popAmount;
        // pop also gated by audio energy/high so silence holds still
        popEnv *= mix(0.25, 1.0, max(bandEnergy, 0.5*high));

        // local cell coord, scaled to grow during pop
        float popScale = 1.0 + 0.55 * popEnv;
        vec2 lp = gf / popScale;

        // skip cells far outside (anti-aliasing margin)
        if (max(abs(lp.x), abs(lp.y)) > 1.10) continue;

        // stroke weight scaled by bass
        float sw = 0.4 + 0.7*bass;
        float crisp = 0.55 + 0.45*high;

        // cell body + edge
        vec2 ce = variantCell(v, lp, t + popSeed*6.2832, sw, crisp);
        float body = ce.x;
        float edge = ce.y;
        if (body + edge < 0.001) continue;

        // pick palette index per cell
        float cellHue = fract(cid * 4.0 + seed*0.07 + fb*0.21);
        vec3 hueCol = paletteOf(paletteMode, cellHue);

        // edge gets darker ink, body gets hue
        vec3 cellRGB = mix(hueCol*0.90, hueCol*1.10 + vec3(0.10*popEnv), body);
        // dark editorial outline pulse during pop
        cellRGB = mix(cellRGB, inkColor.rgb, edge * 0.45 * (0.5 + 0.5*popEnv));

        // band-specific dimming: back band sits behind a slight haze
        float haze = mix(0.18, 0.0, fb*0.5);
        cellRGB = mix(cellRGB, paper, haze);

        // composite alpha — body + edge, both fwidth-AA inside variantCell
        float alpha = clamp(max(body, edge), 0.0, 1.0);
        // pop lifts brightness + alpha briefly
        alpha *= (1.0 + 0.45*popEnv);
        alpha = clamp(alpha, 0.0, 1.0);

        col = mix(col, cellRGB, alpha);

        // ── Elected caption cell ─────────────────────────────────────
        // Front band only. A single cell per band-frame is "elected" by
        // hashing the integer time bucket + grid id; that elected cell
        // hosts the typewriter caption inside its disc.
        if (band == 2) {
            float bucket = floor(t * 0.45);
            float pick = h21(vec2(bucket, seed*9.13));
            // map pick → integer grid pair
            vec2 elect = floor(vec2(pick*float(cellsRow*2)-float(cellsRow),
                                    h11(bucket+seed)*float(cellsRow*2)-float(cellsRow)));
            float isElect = step(0.5, 1.0 - length(gi - elect));
            int total = charCount();
            if (isElect > 0.5 && total > 0) {
                float capScale = clamp(captionScale, 0.5, 1.8);
                // shrink local cell so caption sits inside the cell disc
                float cap = drawCaption(lp*1.05, total, capScale, kerning);
                // type-in growth: msgAge ≥ 0 → live; otherwise full
                float typed = 1.0;
                if (msgAge >= 0.0) typed = smoothstep(0.0, 0.6, msgAge);
                cap *= typed;
                if (cap > 0.001) {
                    // ink contrasts cell body
                    vec3 ink = inkColor.rgb;
                    col = mix(col, ink, cap);
                }
            }
        }
    }

    // ── Global crescendo response ─────────────────────────────────────
    // Subtle full-canvas warmth bump on combined high energy.
    float totalE = max(max(eA, eB), eC);
    col += 0.04 * totalE * vec3(1.0, 0.96, 0.88);

    // ── Output ────────────────────────────────────────────────────────
    col = clamp(col, 0.0, 1.0);
    gl_FragColor = vec4(col, 1.0);
}
