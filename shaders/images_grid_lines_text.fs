/*{
  "DESCRIPTION": "Images Grid Lines Text — an editorial photo-album page. A loose 6×8 grid of procedural 'photo cutouts' (each a unique tiny scene: skyline, water, foliage, cat-fur, hair, building, ticket, satellite — never literal, all abstract patches with a printed caption strip) sits over warm paper. Crossing the cutouts at a steeper z is a cage of sharp coloured lines, breathing on bass, raked in space. The editorial title and a Chinese-style headline block typewrite cue.latest across the right margin in big sans-serif. Cutouts are owned by player[1..3].energy — when player N talks, that subset of cards lifts forward, saturates, and the lines that pass through them glow; the rest of the grid sits muted in the back z-plane. Real depth: each cutout has its own z (parallax + tiny DOF blur), lines occupy a third z above, text floats topmost. Anti-pattern free: no spectrum bars, no EKG, no checkerboard — the grid is irregular, organic, and the line cage is structural composition, not decoration.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",         "TYPE": "text", "DEFAULT": "DURING THIS PERIOD PHOTO ALBUMS", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "energyA",     "LABEL": "Player A · Top Row",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",     "LABEL": "Player B · Mid Row",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",     "LABEL": "Player C · Bot Row",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].active" },
    { "NAME": "bassPulse",   "LABEL": "Lines · Bass Breath",   "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.9, "BIND": "audio.bass" },

    { "NAME": "gridCols",    "LABEL": "Grid · Cols",           "TYPE": "long",  "DEFAULT": 6, "VALUES": [4,5,6,7,8], "LABELS": ["4","5","6","7","8"] },
    { "NAME": "gridRows",    "LABEL": "Grid · Rows",           "TYPE": "long",  "DEFAULT": 8, "VALUES": [5,6,7,8,9,10], "LABELS": ["5","6","7","8","9","10"] },
    { "NAME": "lineDensity", "LABEL": "Line Cage Density",     "TYPE": "long",  "DEFAULT": 9, "VALUES": [4,6,8,9,10,12,14], "LABELS": ["4","6","8","9","10","12","14"] },
    { "NAME": "palette",     "LABEL": "Palette",               "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Editorial","Risograph","Mono","Acid"] },

    { "NAME": "motion",      "LABEL": "Motion Tempo",          "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "audioDepth",  "LABEL": "Audio Depth",           "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },

    { "NAME": "cutoutFill",  "LABEL": "Cutout Fill",           "TYPE": "float", "MIN": 0.15, "MAX": 0.95, "DEFAULT": 0.45 },
    { "NAME": "lineWeight",  "LABEL": "Line Weight",           "TYPE": "float", "MIN": 0.5,  "MAX": 4.0,  "DEFAULT": 1.6 },
    { "NAME": "textOpacity", "LABEL": "Title Opacity",         "TYPE": "float", "MIN": 0.0,  "MAX": 1.5,  "DEFAULT": 1.0 },
    { "NAME": "grain",       "LABEL": "Paper Grain",           "TYPE": "float", "MIN": 0.0,  "MAX": 1.2,  "DEFAULT": 0.5 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  IMAGES GRID LINES TEXT  ·  editorial photo-album · line cage · live text
//
//  Reference: a 60×80-grid photo album page with ~14 small image cutouts
//  loosely scattered, captioned, overlaid by a cage of sharp coloured
//  lines and a big editorial title in the right margin.
//
//  We rebuild that *feeling* — never literal:
//    • Cutouts are SDF-positioned 'tiles' on an irregular 6×8 lattice;
//      each one carries a procedural micro-scene (a horizon strip, a
//      water glint, a soft fur patch, a hair wave, a window square,
//      a stripe of grass, a single brush-stroke) chosen by a per-tile
//      seed. Three player channels own three row-bands; talking pulls
//      that band's tiles forward in z and saturates them.
//    • Above the tiles, a cage of 6–14 coloured lines crosses the canvas
//      at random angles. Each line has its own depth, anti-aliased with
//      fwidth and width-modulated by bassPulse. Tiles under a hot line
//      pick up a thin coloured glow.
//    • To the right floats a typographic title block — cue.latest types
//      out as the big editorial headline, with two small caption strips
//      pinned to nearby tiles for editorial rhythm.
//    • Three z-planes (paper → tiles → lines → text) give real layered
//      parallax; tiles have a tiny per-z DOF blur (radius scales with
//      |z|) so the back row reads softer than the front.
// ════════════════════════════════════════════════════════════════════════

#define MAX_COLS  8
#define MAX_ROWS  10
#define MAX_TILES 80
#define MAX_LINES 14
#define MAX_WALK  48
#define SPACE_CH  26

const float TAU = 6.28318530718;
const float PI  = 3.14159265359;

// ── text atlas access (matches text_clusters.fs idiom) ─────────────────
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

// ── hash + noise ───────────────────────────────────────────────────────
float h11(float n){ return fract(sin(n*127.1)*43758.5453); }
vec2  h21(float n){ return vec2(h11(n), h11(n+17.31)); }
vec3  h31(float n){ return vec3(h11(n), h11(n+17.31), h11(n+33.71)); }
float h12(vec2 p){
    vec3 q = fract(vec3(p.xyx)*vec3(0.1031,0.1030,0.0973));
    q += dot(q, q.yzx+33.33);
    return fract((q.x+q.y)*q.z);
}
float vnoise(vec2 p){
    vec2 i=floor(p), f=fract(p);
    f=f*f*(3.0-2.0*f);
    float a=h12(i), b=h12(i+vec2(1,0));
    float c=h12(i+vec2(0,1)), d=h12(i+vec2(1,1));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}
float fbm2(vec2 p){
    float v=0.0, a=0.55;
    for (int i=0;i<4;i++){ v+=a*vnoise(p); p=p*2.07+vec2(11.3,5.7); a*=0.5; }
    return v;
}

// ── SDF primitives ─────────────────────────────────────────────────────
float sdRoundBox(vec2 p, vec2 b, float r){
    vec2 d = abs(p)-b+r;
    return min(max(d.x,d.y),0.0) + length(max(d,0.0)) - r;
}
float sdSegment(vec2 p, vec2 a, vec2 b){
    vec2 pa = p-a, ba = b-a;
    float h = clamp(dot(pa,ba)/max(dot(ba,ba),1e-5), 0.0, 1.0);
    return length(pa - ba*h);
}

// ── palette ────────────────────────────────────────────────────────────
vec3 palettePick(int sw, float t){
    if (sw == 1) {  // Risograph: orange/teal/cream/pink
        if (t < 0.25) return vec3(0.95,0.42,0.20);
        if (t < 0.50) return vec3(0.18,0.55,0.62);
        if (t < 0.75) return vec3(0.96,0.92,0.78);
        return vec3(0.92,0.58,0.66);
    } else if (sw == 2) {  // Mono — paper/charcoal/silver
        if (t < 0.33) return vec3(0.92,0.91,0.88);
        if (t < 0.66) return vec3(0.18,0.17,0.18);
        return vec3(0.62,0.62,0.66);
    } else if (sw == 3) {  // Acid — mint, hot red, lemon, lilac
        if (t < 0.25) return vec3(0.62,0.98,0.78);
        if (t < 0.50) return vec3(0.98,0.18,0.22);
        if (t < 0.75) return vec3(0.98,0.92,0.20);
        return vec3(0.72,0.58,0.98);
    }
    // Editorial (default) — cherry, mint, paper, yellow, pink, sky, slate
    if (t < 0.14) return vec3(0.92,0.18,0.22);
    if (t < 0.28) return vec3(0.62,0.92,0.74);
    if (t < 0.42) return vec3(0.95,0.94,0.91);
    if (t < 0.56) return vec3(0.98,0.86,0.20);
    if (t < 0.70) return vec3(0.92,0.62,0.72);
    if (t < 0.84) return vec3(0.58,0.78,0.92);
    return vec3(0.22,0.24,0.28);
}

// ── per-tile micro-scene generator ─────────────────────────────────────
// Given local UV in [0,1]² inside the tile, a kind 0..7, and a seed,
// paint a tiny abstract scene that reads as 'a photograph of something'
// without being literal. Returns linear RGB.
vec3 microScene(int kind, vec2 uv, vec3 seed){
    vec2 q = uv;
    if (kind == 0) {
        // 'skyline' — horizon stripe with stacked silhouettes
        vec3 sky = mix(vec3(0.42,0.50,0.62), vec3(0.86,0.84,0.78), q.y);
        float horizon = 0.42 + 0.08*vnoise(vec2(q.x*5.0, seed.x*9.0));
        float build = step(q.y, horizon - 0.05*vnoise(vec2(q.x*14.0+seed.y*7.0, seed.z*3.0)));
        vec3 bod = mix(vec3(0.10,0.12,0.18), vec3(0.22,0.18,0.16), seed.z);
        return mix(sky, bod, build);
    }
    if (kind == 1) {
        // 'water' — dark blue with rippled highlight band
        vec3 deep = mix(vec3(0.04,0.06,0.12), vec3(0.16,0.22,0.32), q.y);
        float wave = 0.5 + 0.5*sin(q.y*22.0 + 6.0*vnoise(vec2(q.x*6.0,q.y*4.0+seed.x*3.0)));
        wave = pow(wave, 6.0) * smoothstep(0.2,0.6,q.y);
        return deep + wave*vec3(0.95,0.78,0.42)*0.55;
    }
    if (kind == 2) {
        // 'foliage / grass' — green w/ vertical strokes
        float n = fbm2(q*vec2(38.0, 7.0) + seed.xy*11.0);
        vec3 base = mix(vec3(0.12,0.28,0.16), vec3(0.40,0.58,0.28), n);
        return base * (0.85 + 0.25*n);
    }
    if (kind == 3) {
        // 'fur' — soft warm patch with directional grain (cat)
        float ang = seed.x*TAU;
        vec2 d = vec2(cos(ang), sin(ang));
        float s = vnoise(q*48.0 + d*8.0*vnoise(q*4.0));
        vec3 base = mix(vec3(0.86,0.78,0.66), vec3(0.45,0.38,0.30), s);
        return base;
    }
    if (kind == 4) {
        // 'hair / fabric' — long sweeping strands
        float r = q.x + 0.15*sin(q.y*8.0 + seed.y*9.0);
        float strands = 0.5 + 0.5*sin(r*42.0 + 6.0*fbm2(q*3.0));
        vec3 base = mix(vec3(0.62,0.38,0.32), vec3(0.92,0.62,0.42), strands*strands);
        return base * (0.7 + 0.3*fbm2(q*8.0));
    }
    if (kind == 5) {
        // 'window' — dark rectangle with rim light
        vec2 c = q - 0.5;
        float box = sdRoundBox(c, vec2(0.36,0.22), 0.04);
        float rim = smoothstep(0.0, 0.02, -box) - smoothstep(0.02, 0.05, -box);
        vec3 inside = vec3(0.02,0.03,0.06) + rim*vec3(0.95,0.92,0.80)*0.5;
        return mix(vec3(0.18,0.18,0.20), inside, smoothstep(0.0,0.01,-box));
    }
    if (kind == 6) {
        // 'sticker / ticket' — pastel patch with a faint stripe
        vec3 base = mix(vec3(0.96,0.84,0.42), vec3(0.92,0.92,0.84),
                        smoothstep(0.2,0.8, vnoise(q*6.0+seed.yz*4.0)));
        float stripe = smoothstep(0.02,0.0, abs(q.y - 0.30));
        return base + stripe*vec3(0.86,0.22,0.18)*0.45;
    }
    // kind 7 — 'satellite / map' — top-down green/grey blocks
    float v = fbm2(q*5.5 + seed.zx*7.0);
    vec3 land = mix(vec3(0.36,0.42,0.28), vec3(0.72,0.74,0.66), step(0.5, v));
    float road = smoothstep(0.02, 0.0, abs(q.x*1.2 - q.y - 0.3 + 0.15*vnoise(q*9.0)));
    return mix(land, vec3(0.96,0.94,0.88), road*0.6);
}

// ── main ───────────────────────────────────────────────────────────────
void main() {
    vec2 res = RENDERSIZE;
    vec2 fragP = (gl_FragCoord.xy - 0.5*res) / res.y;   // aspect-corrected centered
    float aspect = res.x / res.y;
    vec2 uv = gl_FragCoord.xy / res;

    float gT     = TIME * 0.6 * motion;
    float gAud   = clamp(audioDepth, 0.0, 2.0);
    float bass   = clamp(bassPulse, 0.0, 2.0);
    int   cols   = clamp(int(gridCols), 2, MAX_COLS);
    int   rows   = clamp(int(gridRows), 2, MAX_ROWS);
    int   nLines = clamp(int(lineDensity), 2, MAX_LINES);
    int   pal    = int(palette);

    // ── 1. Warm paper backdrop with subtle vignette ─────────────────
    vec3 paper = mix(vec3(0.93,0.92,0.88), vec3(0.86,0.84,0.79), uv.y*0.4 + 0.15*vnoise(uv*3.0));
    float vign = 1.0 - 0.22*dot(fragP*vec2(1.0,0.9), fragP*vec2(1.0,0.9));
    paper *= vign;

    vec3 col = paper;

    // ── 2. Tile field — per-tile cutouts in row-bands owned by players
    // We render back-to-front: back row first (small +y offset, lower
    // contrast, larger DOF blur), front row last. Tile cell is the grid
    // cell; the actual cutout is a smaller round-box inside the cell,
    // jittered so the lattice never reads as a literal grid.
    float cellW = (aspect - 0.55) / float(cols);   // left margin ~ -aspect/2, text takes right margin
    float cellH = 1.30 / float(rows);
    vec2  fieldOrigin = vec2(-0.5*aspect + 0.05, -0.65);

    for (int r = 0; r < MAX_ROWS; r++) {
        if (r >= rows) break;
        // Which player owns this row-band? top third → A, mid → B, bot → C
        float rowF = float(r) / max(float(rows-1), 1.0);
        float ownerEnergy;
        if (rowF < 0.34)      ownerEnergy = energyA;
        else if (rowF < 0.67) ownerEnergy = energyB;
        else                  ownerEnergy = energyC;
        float warmth = clamp(ownerEnergy * gAud, 0.0, 1.0);

        for (int c = 0; c < MAX_COLS; c++) {
            if (c >= cols) break;
            float seed = float(r)*8.0 + float(c)*1.0 + 7.31;
            vec3 ts = h31(seed*1.7);

            // Skip ~30% of cells so the grid is loose, not literal
            if (ts.x < 0.30) continue;

            // Cell center
            vec2 cellC = fieldOrigin + vec2((float(c)+0.5)*cellW, (float(r)+0.5)*cellH);
            // Jitter inside cell + slow drift
            vec2 jitter = (ts.yz - 0.5) * vec2(cellW, cellH) * 0.32;
            jitter += vec2(sin(gT*0.5 + seed*1.7), cos(gT*0.43 + seed*2.1)) * 0.012;
            // Owner lifts the tile forward when talking (slight upward and zoom)
            float lift = warmth * 0.04;
            vec2 anchor = cellC + jitter + vec2(0.0, lift);

            // Tile size — small variance, scale up slightly with owner energy
            float baseW = cellW * (0.32 + 0.10*ts.x);
            float baseH = cellH * (0.34 + 0.12*ts.y);
            vec2 sz = vec2(baseW, baseH) * (1.0 + 0.10*warmth);

            // Tilt
            float tilt = (ts.x - 0.5) * 0.45 + warmth*0.05;
            float ca = cos(tilt), sa = sin(tilt);
            vec2 lp = fragP - anchor;
            lp = mat2(ca,-sa,sa,ca) * lp;

            // SDF for the tile (rounded-rect)
            float d = sdRoundBox(lp, sz, min(sz.x,sz.y)*0.08);
            float fw = fwidth(d);
            float fill = 1.0 - smoothstep(-fw, fw, d);
            if (fill < 0.001) continue;

            // Per-row DOF blur — back rows softer. Reduces contrast more
            // for rows farther from the active band.
            float zDepth = mix(1.0, 0.45, rowF) * mix(1.0, 1.18, warmth);
            float softness = mix(0.55, 1.0, warmth);

            // Local UV inside the tile in [0,1]² (for microScene)
            vec2 luv = (lp / sz) * 0.5 + 0.5;
            int kind = int(mod(seed*3.0 + ts.z*8.0, 8.0));
            vec3 scene = microScene(kind, luv, ts);
            // Back-row desaturate + slight cool tint; front row pops
            float L = dot(scene, vec3(0.299,0.587,0.114));
            scene = mix(vec3(L), scene, mix(0.55, 1.0, zDepth));
            scene *= softness;

            // Thin paper border around the cutout (printed photo look)
            float border = smoothstep(-fw*1.5, 0.0, d) - smoothstep(0.0, fw*1.5, d);
            scene = mix(scene, vec3(0.97,0.96,0.93), border*0.6);

            // Tiny caption strip at the bottom of each tile (paper margin
            // with one dot — abstract caption, not readable glyphs)
            float capStripe = smoothstep(sz.y*0.96, sz.y*0.78, abs(lp.y + sz.y*0.86))
                             * smoothstep(sz.x, sz.x*0.85, abs(lp.x));
            scene = mix(scene, vec3(0.99,0.98,0.95), capStripe*0.7);
            float capDot = smoothstep(0.012, 0.006, length(lp - vec2(-sz.x*0.55, -sz.y*0.86)));
            scene = mix(scene, vec3(0.18,0.18,0.20), capDot*0.85);

            // Owner glow rim when warm
            float rim = smoothstep(-fw*4.0, -fw*1.0, d) - smoothstep(-fw*1.0, fw*0.5, d);
            scene += warmth * rim * palettePick(pal, ts.x)*0.45;

            // Composite tile over current col with depth-aware mix
            col = mix(col, scene, fill * mix(0.85, 1.0, zDepth));
        }
    }

    // ── 3. Line cage — sharp coloured lines crossing the grid ───────
    // Each line is a segment between two edge points; its colour comes
    // from the palette; its width is fwidth-AA and breathes on bass.
    // Lines occupy a z above tiles → they always paint over tiles, but
    // we track which segments are near hot rows so we can glow them.
    float lineMask = 0.0;
    vec3  lineCol  = vec3(0.0);
    for (int i = 0; i < MAX_LINES; i++) {
        if (i >= nLines) break;
        float fi = float(i);
        vec3 s = h31(fi*5.71 + 1.23);
        // Pick two random points on the canvas (extended slightly so
        // lines run off-edge). Add slow rotation so the cage breathes.
        float phase = gT*0.10 + fi*1.7;
        vec2 a = vec2((s.x-0.5)*2.0*aspect, (s.y-0.5)*1.4);
        vec2 b;
        float baseAng = s.z * TAU + 0.25*sin(phase);
        float len = mix(0.9, 1.8, h11(fi*9.13));
        b = a + vec2(cos(baseAng), sin(baseAng))*len;

        float d = sdSegment(fragP, a, b);
        float fw = fwidth(d) * lineWeight;
        // Width modulated by bass + own slow oscillation
        float w = fw * (1.0 + 0.6*bass*gAud + 0.18*sin(gT*0.7+fi*2.3));
        float aa = 1.0 - smoothstep(0.0, w*1.6, d);

        if (aa > 0.001) {
            vec3 c = palettePick(pal, fract(s.x*0.7 + fi*0.13));
            // Soft outer glow for hot lines
            float glow = exp(-d/max(w*4.0, 1e-4)) * 0.18 * bass * gAud;
            float w2 = aa + glow;
            // Front-to-back: later lines overwrite earlier ones (we approximate
            // depth as line index parity for a layered cage look)
            lineMask = max(lineMask, w2);
            lineCol  = mix(lineCol, c, w2);
        }
    }
    col = mix(col, lineCol, clamp(lineMask, 0.0, 1.0));

    // ── 4. Editorial title — cue.latest typewriter on the right margin
    int total = charCount();
    if (total > 0 && textOpacity > 0.001) {
        // Title block sits in the right margin (about 30% of canvas width)
        // Anchor: just inside the right edge, vertically centered
        vec2 titleAnchor = vec2(aspect*0.18, 0.10);
        // Slow typewriter reveal driven by msgAge if live; else show all
        bool live = msgAge >= 0.0;
        int reveal = live ? clamp(int(msgAge * 22.0), 0, total) : total;

        // Multi-line wrap — break on spaces, ~10 chars/row
        const int ROW_CAP = 11;
        float charH = 0.085;
        float charW = charH * (5.0/7.0);
        float kern  = charW * 0.95;
        float lineH = charH * 1.20;

        // First pass: count rows used by reveal (so we can vertically center)
        int usedRows = 1;
        {
            int rC = 0, rR = 0;
            for (int i = 0; i < MAX_WALK; i++) {
                if (i >= reveal) break;
                int ch = getChar(i);
                if (ch == SPACE_CH) {
                    int wlen = 0;
                    for (int j = 1; j < MAX_WALK; j++) {
                        int jj = i+j;
                        if (jj >= reveal) break;
                        int chj = getChar(jj);
                        if (chj == SPACE_CH || chj < 0 || chj > 36) break;
                        wlen++;
                    }
                    if (rC > 0 && rC + 1 + wlen > ROW_CAP) { rR++; rC = 0; }
                    else if (rC > 0) { rC++; }
                } else if (ch >= 0 && ch <= 36) {
                    rC++;
                    if (rC >= ROW_CAP) { rR++; rC = 0; }
                }
            }
            usedRows = rR + 1;
        }
        float blockH = float(usedRows) * lineH;

        // Local coordinates relative to title block (top-left origin for layout)
        vec2 tp = fragP - titleAnchor;
        // We want text to grow top→down: flip y so increasing tp.y inside
        // the block is downward
        float lx = tp.x;
        float ly = (blockH*0.5 - tp.y);
        if (lx >= 0.0 && lx <= float(ROW_CAP)*kern && ly >= 0.0 && ly <= blockH) {
            int targetCol = int(floor(lx / kern));
            int targetRow = int(floor(ly / lineH));

            float rowPad = (lineH - charH) * 0.5;
            float yInRow = (ly - float(targetRow)*lineH) - rowPad;

            if (yInRow >= 0.0 && yInRow <= charH) {
                int cursorR = 0, cursorC = 0, outCh = -1;
                for (int i = 0; i < MAX_WALK; i++) {
                    if (i >= reveal) break;
                    if (cursorR > targetRow) break;
                    int ch = getChar(i);
                    if (ch == SPACE_CH) {
                        int wlen = 0;
                        for (int j = 1; j < MAX_WALK; j++) {
                            int jj = i+j;
                            if (jj >= reveal) break;
                            int chj = getChar(jj);
                            if (chj == SPACE_CH || chj < 0 || chj > 36) break;
                            wlen++;
                        }
                        if (cursorC > 0 && cursorC + 1 + wlen > ROW_CAP) {
                            cursorR++; cursorC = 0;
                        } else if (cursorC > 0) {
                            if (cursorR == targetRow && cursorC == targetCol)
                                outCh = SPACE_CH;
                            cursorC++;
                        }
                    } else if (ch >= 0 && ch <= 36) {
                        if (cursorR == targetRow && cursorC == targetCol)
                            outCh = ch;
                        cursorC++;
                        if (cursorC >= ROW_CAP) { cursorR++; cursorC = 0; }
                    }
                }
                if (outCh >= 0 && outCh <= 35 && outCh != SPACE_CH) {
                    float colPad = (kern - charW) * 0.5;
                    vec2 cellLocal = vec2(
                        (lx - float(targetCol)*kern - colPad) / charW,
                        1.0 - yInRow / charH);
                    float s = sampleChar(outCh, cellLocal);
                    s = smoothstep(0.20, 0.55, s);
                    // Title ink: dark slate, modulated by textOpacity and a
                    // subtle audio-driven shimmer so it feels alive
                    vec3 ink = vec3(0.06,0.06,0.08);
                    ink = mix(ink, palettePick(pal, 0.50), 0.18*bass*gAud);
                    col = mix(col, ink, s * clamp(textOpacity, 0.0, 1.5));
                }
            }
        }

        // Blinking caret on the live cursor (only when actively typing)
        if (live && reveal < total) {
            // Find where the cursor lives in the layout (re-walk reveal chars)
            int curR = 0, curC = 0;
            for (int i = 0; i < MAX_WALK; i++) {
                if (i >= reveal) break;
                int ch = getChar(i);
                if (ch == SPACE_CH) {
                    int wlen = 0;
                    for (int j = 1; j < MAX_WALK; j++) {
                        int jj = i+j;
                        if (jj >= total) break;
                        int chj = getChar(jj);
                        if (chj == SPACE_CH || chj < 0 || chj > 36) break;
                        wlen++;
                    }
                    if (curC > 0 && curC + 1 + wlen > ROW_CAP) { curR++; curC = 0; }
                    else if (curC > 0) { curC++; }
                } else if (ch >= 0 && ch <= 36) {
                    curC++;
                    if (curC >= ROW_CAP) { curR++; curC = 0; }
                }
            }
            float caretX = float(curC)*kern + (kern-charW)*0.5;
            float caretYTop = float(curR)*lineH + (lineH-charH)*0.5;
            float blink = step(0.0, sin(TIME*8.0));
            if (lx >= caretX && lx <= caretX+charW*0.18 && ly >= caretYTop && ly <= caretYTop+charH) {
                col = mix(col, vec3(0.06,0.06,0.08), blink * clamp(textOpacity,0.0,1.5));
            }
        }
    }

    // ── 5. Paper grain (continuous, never pixel grid) ──────────────
    float tooth = fbm2(uv*res.y*0.013) + 0.4*fbm2(uv*res.y*0.04 + 7.0);
    col *= 1.0 + (tooth - 0.75) * 0.06 * grain;

    // ── 6. Subtle bottom-edge text labels ('PHOTO ... ALBUMS') ─────
    // Just two static abstract glyph-strips at the bottom corners — gives
    // the editorial photo-album frame without being literally readable.
    {
        float yEdge = smoothstep(0.04, 0.02, uv.y);
        float xL = smoothstep(0.10, 0.06, uv.x);
        float xR = smoothstep(0.90, 0.94, uv.x);
        col = mix(col, vec3(0.10,0.10,0.12), yEdge * (xL+xR) * 0.35);
    }

    gl_FragColor = vec4(col, 1.0);
}
