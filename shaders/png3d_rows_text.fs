/*{
  "DESCRIPTION": "PNG3D Rows Text — editorial collage of monochrome cutout 'PNG' objects (bust, cube, column, sphere, prism, slab) laid across parallax rows on warm paper. Each row drifts at its own speed; cutouts cast soft offset shadows onto the row behind them so the canvas reads as a layered diorama, not a flat poster. The `msg` (auto-bound to cue.latest) types itself out as parenthesized words interleaved with the cutouts — text and objects share each row's rhythm. Three players animate three distinct row bands (jitter / breathe / drift) so muting one player visibly stills that row. Bass widens the parallax separation and lifts the shadow blur. No literal logos, no spectrum bars, no checkerboard — just type, depth, and intent.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",          "TYPE": "text",  "DEFAULT": "CLASSICAL MAGIC AND TEXTURE", "MAX_LENGTH": 48, "BIND": "cue.latest" },
    { "NAME": "rowCount",     "LABEL": "Rows",                 "TYPE": "long",  "DEFAULT": 4, "VALUES": [3,4,5,6], "LABELS": ["3","4","5","6"] },
    { "NAME": "itemsPerRow",  "LABEL": "Items / Row",          "TYPE": "long",  "DEFAULT": 5, "VALUES": [3,4,5,6,7,8], "LABELS": ["3","4","5","6","7","8"] },
    { "NAME": "palette",      "LABEL": "Palette",              "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3,4], "LABELS": ["Paper / Graphite","Cream / Indigo","Linen / Oxblood","Bone / Forest","Onyx / Sulphur"] },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",         "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 3.0 },
    { "NAME": "audioDepth",   "LABEL": "Parallax (bass)",      "TYPE": "float", "DEFAULT": 0.8, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.bass" },
    { "NAME": "shadowDepth",  "LABEL": "Shadow Depth",         "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "energyA",      "LABEL": "Player A — Row 1 jitter",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",      "LABEL": "Player B — Row 2 breath",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",      "LABEL": "Player C — Row 3 drift",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "cueLevel",     "LABEL": "Cue Pulse (level)",    "TYPE": "float", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 2.0, "BIND": "audio.level" },
    { "NAME": "transparentBg","LABEL": "Transparent BG",       "TYPE": "bool",  "DEFAULT": 0.0 }
  ]
}*/

// ═══════════════════════════════════════════════════════════════════════
//  PNG3D ROWS TEXT  ·  editorial collage · cutout objects · parallax rows
//
//  Reference (Documents/A-List Shaders/3D_pngs_rows_text.jpg):
//   rows of words in parentheses interleaved with monochrome 3D cutouts
//   (bust, cube, column, marble triangle, wood slab) on warm paper.
//
//  Implementation:
//    - N horizontal rows; each row drifts horizontally at its own speed
//      (back rows slower, front rows faster → real parallax depth).
//    - Each row carries `itemsPerRow` cells. Each cell renders EITHER a
//      procedural cutout object (8 archetypes — bust, sphere, cube, column,
//      triangle, slab, ring, prism) OR a chunk of the message in
//      parentheses, deterministically chosen per cell.
//    - Cutouts are signed-distance shapes with fwidth anti-aliasing and a
//      faked-3D normal (gradient lighting + edge contour) so they read as
//      flat-shaded PNG cutouts rather than vector silhouettes.
//    - Each cutout casts a soft offset shadow onto the row PLANE behind
//      it (back-to-front compositing); shadow offset + blur scale with the
//      row's depth and with bass.
//    - Text is the typewriter `msg`: it reveals one char at a time via
//      `msgAge` so words appear in sequence across the cells, like the
//      reference's "(classical) (magic) & (and) texture" rhythm.
// ═══════════════════════════════════════════════════════════════════════

#define MAX_CHARS  48
#define SPACE_CH   26
#define MAX_ROWS    6
#define MAX_ITEMS   8

const float TAU = 6.28318530718;

// ─── font atlas helpers ─────────────────────────────────────────────────
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

// ─── hash / noise ───────────────────────────────────────────────────────
float h11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  h21(float n) { return vec2(h11(n), h11(n + 17.31)); }
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0 - 2.0*f);
    float a = h11(dot(i, vec2(1.0, 157.0)));
    float b = h11(dot(i + vec2(1.0, 0.0), vec2(1.0, 157.0)));
    float c = h11(dot(i + vec2(0.0, 1.0), vec2(1.0, 157.0)));
    float d = h11(dot(i + vec2(1.0, 1.0), vec2(1.0, 157.0)));
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
float smin(float a, float b, float k) {
    float h = clamp(0.5 + 0.5*(b - a)/k, 0.0, 1.0);
    return mix(b, a, h) - k*h*(1.0 - h);
}

// ─── palette ────────────────────────────────────────────────────────────
struct Pal { vec3 paper; vec3 ink; vec3 accent; vec3 shadow; };
Pal getPalette(int idx) {
    Pal P;
    if (idx == 1)      { P.paper = vec3(0.95, 0.93, 0.88); P.ink = vec3(0.10, 0.12, 0.30); P.accent = vec3(0.78, 0.32, 0.30); P.shadow = vec3(0.18, 0.18, 0.28); }
    else if (idx == 2) { P.paper = vec3(0.94, 0.91, 0.86); P.ink = vec3(0.46, 0.10, 0.13); P.accent = vec3(0.32, 0.30, 0.28); P.shadow = vec3(0.30, 0.16, 0.16); }
    else if (idx == 3) { P.paper = vec3(0.93, 0.91, 0.85); P.ink = vec3(0.10, 0.22, 0.16); P.accent = vec3(0.38, 0.36, 0.30); P.shadow = vec3(0.12, 0.20, 0.14); }
    else if (idx == 4) { P.paper = vec3(0.10, 0.10, 0.10); P.ink = vec3(0.96, 0.94, 0.60); P.accent = vec3(0.55, 0.55, 0.56); P.shadow = vec3(0.02, 0.02, 0.02); }
    else               { P.paper = vec3(0.91, 0.90, 0.88); P.ink = vec3(0.07, 0.07, 0.09); P.accent = vec3(0.42, 0.42, 0.44); P.shadow = vec3(0.12, 0.12, 0.14); }
    return P;
}

// ─── cutout SDFs ────────────────────────────────────────────────────────
//  Each takes local coords in roughly [-1,1]² and returns a signed distance
//  (negative inside). They're stylized to read as monochrome PNG-like
//  cutouts rather than literal photographs.

// Classical bust silhouette (head + neck + shoulders)
float sdBust(vec2 p) {
    // head ellipse
    vec2 hp = (p - vec2(0.0, 0.35)) / vec2(0.32, 0.40);
    float head = length(hp) - 1.0;
    head *= min(0.32, 0.40);
    // neck capsule
    vec2 np = p - vec2(0.0, -0.05);
    float neck = length(vec2(np.x, max(abs(np.y) - 0.18, 0.0))) - 0.13;
    // shoulders — wide rounded trapezoid
    vec2 sp = p - vec2(0.0, -0.55);
    float shoulder = length(vec2(sp.x*0.55, max(abs(sp.y + 0.05) - 0.05, 0.0))) - 0.46;
    shoulder = max(shoulder, -p.y - 0.85);
    // hair tuft (asymmetric lump)
    vec2 tp = (p - vec2(-0.12, 0.55)) / vec2(0.22, 0.18);
    float tuft = length(tp) - 1.0;
    tuft *= 0.18;
    float d = smin(head, neck, 0.06);
    d = smin(d, shoulder, 0.10);
    d = smin(d, tuft, 0.05);
    return d;
}

// Cube (axonometric — three visible faces hinted by interior lines later)
float sdBoxRound(vec2 p, vec2 b, float r) {
    vec2 q = abs(p) - b + r;
    return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - r;
}
float sdCube(vec2 p) { return sdBoxRound(p, vec2(0.55, 0.55), 0.04); }

// Column (Ionic-ish — narrow rectangle with capital + base flares)
float sdColumn(vec2 p) {
    float shaft  = sdBoxRound(p, vec2(0.18, 0.72), 0.02);
    float cap    = sdBoxRound(p - vec2(0.0,  0.70), vec2(0.32, 0.08), 0.02);
    float capTop = sdBoxRound(p - vec2(0.0,  0.78), vec2(0.36, 0.04), 0.01);
    float base   = sdBoxRound(p - vec2(0.0, -0.70), vec2(0.32, 0.08), 0.02);
    float baseB  = sdBoxRound(p - vec2(0.0, -0.78), vec2(0.36, 0.04), 0.01);
    float d = min(shaft, cap);
    d = min(d, capTop);
    d = min(d, base);
    d = min(d, baseB);
    return d;
}

// Sphere (filled disc — adds a shaded highlight in shading pass)
float sdSphere(vec2 p) { return length(p) - 0.62; }

// Triangle (marble fragment — equilateral, slight irregularity later)
float sdTri(vec2 p) {
    const float k = 1.7320508;
    p.x = abs(p.x) - 0.62;
    p.y = p.y + 0.62/k;
    if (p.x + k*p.y > 0.0) p = vec2(p.x - k*p.y, -k*p.x - p.y) * 0.5;
    p.x -= clamp(p.x, -2.0*0.62, 0.0);
    return -length(p) * sign(p.y);
}

// Slab (wood-grain wide rectangle)
float sdSlab(vec2 p) { return sdBoxRound(p, vec2(0.70, 0.28), 0.04); }

// Ring (torus profile in 2D — annulus)
float sdRing(vec2 p) {
    float r = length(p);
    return max(r - 0.62, 0.38 - r);
}

// Prism (vertical narrow rectangle stood on end, with chamfer)
float sdPrism(vec2 p) { return sdBoxRound(p, vec2(0.26, 0.66), 0.03); }

// Dispatch
float cutoutSdf(int kind, vec2 p) {
    if (kind == 0) return sdBust(p);
    if (kind == 1) return sdCube(p);
    if (kind == 2) return sdColumn(p);
    if (kind == 3) return sdSphere(p);
    if (kind == 4) return sdTri(p);
    if (kind == 5) return sdSlab(p);
    if (kind == 6) return sdRing(p);
    return sdPrism(p);  // 7
}

// Per-archetype interior detail: returns 0..1 brightness modulation so the
// cutout reads as flat-shaded with a 3D suggestion — bust gets a soft
// vertical light, cube gets axonometric face shading, column gets fluting,
// slab gets wood grain, sphere gets a highlight.
float cutoutShade(int kind, vec2 p, float aud) {
    // base flat-ish gradient (top→bottom light from upper left)
    float baseL = 0.62 + 0.30 * clamp(0.5 + 0.5*(p.x*0.5 + p.y*0.9), 0.0, 1.0);

    if (kind == 0) {
        // bust: cheek highlight + neck shadow
        float cheek = exp(-pow(length(p - vec2(0.12, 0.32))*3.2, 2.0));
        float neckShade = smoothstep(0.0, -0.30, p.y) * 0.20;
        return clamp(baseL + cheek*0.18 - neckShade, 0.0, 1.0);
    }
    if (kind == 1) {
        // cube: split into top / left / right faces by diagonals
        float dx = p.x, dy = p.y;
        float top  = step(dy,  abs(dx) + 0.05) * step(dy, -dx + 0.05) * 0.0; // unused
        // 3-face axonometric: top face is brighter, left face mid, right dim
        float face = 0.55;
        if (dy > 0.15 - 0.6*abs(dx)) face = 0.92;          // top
        else if (dx < 0.0)            face = 0.62;          // left
        else                          face = 0.40;          // right
        // edge lines between faces
        float e1 = abs(dy - (0.15 - 0.6*dx));   // top/right diagonal
        float e2 = abs(dy - (0.15 + 0.6*dx));   // top/left diagonal
        float e3 = abs(dx);                     // vertical seam in lower half
        float edge = min(min(e1, e2), e3);
        face *= 1.0 - 0.45*exp(-pow(edge*22.0, 2.0));
        // tiny embossed glyph stub on the front face (purely abstract texture)
        float gx = step(0.0, dx) * exp(-pow((dx-0.18)*9.0, 2.0)) *
                                   exp(-pow((dy+0.20)*9.0, 2.0));
        face -= 0.18 * gx;
        return clamp(face, 0.0, 1.0);
    }
    if (kind == 2) {
        // column: vertical flutes
        float flute = 0.5 + 0.5 * cos(p.x * 18.0);
        float capBand = smoothstep(0.62, 0.70, abs(p.y)) * 0.18;
        return clamp(baseL * mix(0.75, 1.0, flute) + capBand, 0.0, 1.0);
    }
    if (kind == 3) {
        // sphere: bright NE highlight + soft terminator
        vec3 n = vec3(p, sqrt(max(1.0 - dot(p,p)*1.6, 0.0)));
        float diff = clamp(dot(normalize(n), normalize(vec3(0.55, 0.65, 0.55))), 0.0, 1.0);
        float spec = pow(diff, 24.0);
        return clamp(0.28 + 0.72*diff + 0.35*spec, 0.0, 1.0);
    }
    if (kind == 4) {
        // triangle: marbled fbm
        float m = fbm2(p*3.6 + vec2(TIME*0.05, 0.0));
        return clamp(baseL * (0.7 + 0.6*m), 0.0, 1.0);
    }
    if (kind == 5) {
        // slab: horizontal wood grain
        float grain = 0.5 + 0.5*sin(p.x*4.5 + 1.6*fbm2(p*vec2(0.8, 6.0)));
        float knots = exp(-pow(length((p - vec2(0.18, 0.05))*vec2(2.4, 5.0)), 2.0));
        return clamp(baseL * (0.72 + 0.28*grain) + knots*0.12*aud, 0.0, 1.0);
    }
    if (kind == 6) {
        // ring: radial banding (concentric)
        float r = length(p);
        float band = 0.5 + 0.5*cos((r - 0.50)*36.0);
        return clamp(0.45 + 0.42*band + 0.18*(0.62 - abs(r - 0.50)), 0.0, 1.0);
    }
    // prism: vertical stripes
    float stripe = 0.5 + 0.5*sin(p.x*9.0);
    return clamp(baseL * (0.78 + 0.22*stripe), 0.0, 1.0);
}

// ─── parens text helper ─────────────────────────────────────────────────
// Render a parenthesized word at local cell coords.
// Returns ink mask 0..1.
// We assume the parent has selected this cell to be text and supplied the
// word bounds [wStart, wEnd) in the message; here we draw "(WORD)" left-
// aligned across the cell.
float renderParensWord(vec2 cellLocal, vec2 cellSize,
                       int wStart, int wEnd, int totalReveal,
                       out float trailHit) {
    trailHit = 0.0;
    int wlen = wEnd - wStart;
    if (wlen < 1) return 0.0;
    // Includes both parens — visible chars = wlen + 2
    int visChars = wlen + 2;
    // glyph height = 60% of cell height; width derived
    float chH = cellSize.y * 0.42;
    float chW = chH * (5.0 / 7.0);
    // total block width
    float blockW = float(visChars) * chW * 1.05;
    if (blockW > cellSize.x * 0.96) {
        float s = (cellSize.x * 0.96) / blockW;
        chH *= s; chW *= s; blockW *= s;
    }
    // origin: center horizontally, sit on baseline ~45% from top
    vec2 origin = vec2(-blockW * 0.5, -chH * 0.5);
    vec2 lp = cellLocal - origin;
    if (lp.x < 0.0 || lp.x > blockW) return 0.0;
    if (lp.y < 0.0 || lp.y > chH)   return 0.0;
    float col = lp.x / (chW * 1.05);
    int   ci  = int(floor(col));
    if (ci < 0 || ci >= visChars) return 0.0;
    int ch;
    if (ci == 0)             ch = 27;             // '(' is atlas slot 27 (paren approx; falls back to blank)
    else if (ci == visChars-1) ch = 28;           // ')' approx slot 28
    else {
        int srcIdx = wStart + (ci - 1);
        ch = getChar(srcIdx);
        // Hide chars past the typewriter reveal — gives the per-row sequencing
        if (srcIdx >= totalReveal) return 0.0;
    }
    // glyph cell uv
    float xInCell = (lp.x - float(ci) * chW * 1.05) / chW;
    if (xInCell < 0.0 || xInCell > 1.0) return 0.0;
    // lp.y is y-UP world; lp.y∈[0,chH] with chH at screen-top of cell.
    // Host font atlas stores letter-top at v=1, so direct y-up→v mapping
    // puts letter-top at screen-top. The previous `1.0 -` flipped
    // glyphs upside down.
    float yInCell = (lp.y / chH);
    // Many fonts only have 0..36 (alphanum + space); for paren glyphs we fall
    // back to a hand-drawn SDF so the parentheses never go missing.
    if (ch == 27 || ch == 28) {
        // Procedural paren: narrow vertical arc curving toward the inside
        float side = (ch == 27) ? -1.0 : 1.0;
        vec2 g = vec2(xInCell - 0.5, yInCell - 0.5);
        // arc radius ~0.55, opening on the inside (right side for '(', left for ')')
        float r = length(vec2(g.x + side*0.10, g.y*1.2));
        float ring = abs(r - 0.46);
        float arcMask = step(side*g.x, 0.10);   // only the outward half
        float a = (1.0 - smoothstep(0.04, 0.10, ring)) * arcMask;
        return a;
    }
    float s = sampleChar(ch, vec2(xInCell, yInCell));
    s = smoothstep(0.18, 0.55, s);
    // mark the rightmost visible cell as the typewriter caret position so
    // the row can paint a subtle blinking cursor
    if (ci == visChars - 2) trailHit = 1.0;
    return s;
}

// ─── word-range lookup ──────────────────────────────────────────────────
// Find the k-th whitespace-delimited word in `msg` and return its char
// range [outStart, outEnd) within msg. If there aren't enough words,
// outEnd <= outStart.
void getWord(int k, int total, out int outStart, out int outEnd) {
    int i = 0;
    int wordIdx = -1;
    outStart = 0;
    outEnd = 0;
    bool inWord = false;
    int wStart = 0;
    for (int it = 0; it < 48; it++) {
        if (i >= total) {
            if (inWord) {
                wordIdx++;
                if (wordIdx == k) { outStart = wStart; outEnd = i; return; }
            }
            return;
        }
        int ch = getChar(i);
        bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
        if (!isSpace && !inWord) { inWord = true; wStart = i; }
        else if (isSpace && inWord) {
            inWord = false;
            wordIdx++;
            if (wordIdx == k) { outStart = wStart; outEnd = i; return; }
        }
        i++;
    }
}
int countWords(int total) {
    int w = 0;
    bool inW = false;
    for (int i = 0; i < 48; i++) {
        if (i >= total) break;
        int ch = getChar(i);
        bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
        if (!isSpace && !inW) { w++; inW = true; }
        else if (isSpace)     { inW = false; }
    }
    return w;
}

// ─── main ───────────────────────────────────────────────────────────────
void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / max(res.y, 1.0);

    // Aspect-corrected canvas centered at origin: x in [-aspect/2, aspect/2],
    // y in [-0.5, 0.5].
    vec2 p = vec2((uv.x - 0.5) * aspect, uv.y - 0.5);

    int   rows  = int(rowCount);
    if (rows < 1) rows = 1;
    if (rows > MAX_ROWS) rows = MAX_ROWS;
    int   items = int(itemsPerRow);
    if (items < 1) items = 1;
    if (items > MAX_ITEMS) items = MAX_ITEMS;
    int   total = charCount();
    int   wordsAvail = (total > 0) ? countWords(total) : 0;

    Pal P = getPalette(int(palette));

    // motion clocks
    float t = TIME * max(motionSpeed, 0.0);
    float bass = clamp(audioBass * audioDepth, 0.0, 2.0);
    float lvl  = clamp(audioLevel * cueLevel, 0.0, 2.0);

    // Typewriter reveal — chars revealed so far for the live transcript.
    // Static fallback when no live utterance: show everything.
    bool liveUtterance = msgAge >= 0.0;
    float cps = 28.0;
    int   revealed = liveUtterance
        ? clamp(int(floor(msgAge * cps)), 0, total)
        : total;

    // ─── paper backdrop ─────────────────────────────────────────────────
    vec3 col = P.paper;
    // marbled paper tooth
    float paperN = fbm2(uv * vec2(aspect * 1.4, 1.4) * 1.6);
    col *= 1.0 - 0.06 * (paperN - 0.5);
    // warm sun pool top-left
    float sun = exp(-length(p - vec2(-aspect*0.30, 0.32)) * 1.8);
    col += sun * P.accent * 0.06;
    // edge vignette
    col *= 1.0 - 0.12 * dot(p, p);

    // ─── header rule (echoes the ref's top arrow line) ─────────────────
    // a thin horizontal hairline near the top of the canvas
    {
        float y0 = 0.40;
        float rule = smoothstep(0.003, 0.0, abs(p.y - y0));
        // arrow tail: short notch at left
        float arrow = smoothstep(0.005, 0.0, abs(p.y - y0));
        arrow *= smoothstep(-aspect*0.45, -aspect*0.30, p.x) *
                 (1.0 - smoothstep(aspect*0.0, aspect*0.05, p.x));
        col = mix(col, P.ink, max(rule * smoothstep(-aspect*0.45, -aspect*0.0, p.x), arrow) * 0.85);
    }

    // ─── rows (back to front) ──────────────────────────────────────────
    // Canvas vertical band used for rows (leaves space for header + footer).
    float bandTop = 0.32;
    float bandBot = -0.42;
    float bandH   = bandTop - bandBot;
    float rowH    = bandH / float(rows);
    // Cell horizontal layout: full aspect width with margins.
    float cellW = (aspect - 0.10) / float(items);

    // we composite back-to-front so each row's shadow falls on the rows
    // behind it that are already painted.
    for (int ri = 0; ri < MAX_ROWS; ri++) {
        if (ri >= rows) break;
        // back rows first → smaller index = further away.
        float fri = float(ri);
        float depth01 = (rows > 1) ? fri / float(rows - 1) : 0.5;  // 0 = back
        // parallax: back rows drift slower; row baseline moves left over time
        float speed  = mix(0.05, 0.32, depth01) * max(motionSpeed, 0.0);
        // Per-player row binding: rows 0,1,2 mapped to players A,B,C (others share)
        float eRow = 0.0;
        if      (ri == 0) eRow = energyA;
        else if (ri == 1) eRow = energyB;
        else if (ri == 2) eRow = energyC;
        else              eRow = max(max(energyA, energyB), energyC) * 0.5;

        // bass widens parallax separation horizontally
        float parallaxOff = -t * speed - bass * (0.18 + 0.20*depth01);

        // row vertical band
        float rowCY = bandTop - (fri + 0.5) * rowH;
        // Player-driven vertical breathing: B gently swells row 2; C drifts row 3 vertically
        if (ri == 1) rowCY += 0.012 * sin(t*1.6) * eRow;
        if (ri == 2) rowCY += 0.025 * sin(t*0.7) * eRow;

        // shadow falls down-right onto the row behind
        float shOff   = 0.022 + 0.018*depth01 + 0.020*bass;
        float shBlur  = 0.012 + 0.020*depth01 + 0.014*bass;
        shOff  *= shadowDepth;
        shBlur *= max(shadowDepth, 0.0001);

        // We pass over `items` cells in this row, drawing each cutout OR text.
        // To keep parallax seamless when the row scrolls, we tile cells with
        // wrap: virtual cell index = real index + floor(scroll / cellW).
        float wrapScroll = parallaxOff;
        int firstWrap = int(floor(wrapScroll / cellW));
        // accumulate row contribution into row-only buffers, then composite
        vec3 rowCol = vec3(0.0);
        float rowAlpha = 0.0;
        vec3 shadowCol = vec3(0.0);
        float shadowAlpha = 0.0;

        for (int ii = -1; ii <= MAX_ITEMS; ii++) {
            if (ii > items) break;
            // global "virtual" cell id keeps content stable as we scroll
            int cellId = firstWrap + ii;
            float cellCX = -aspect*0.5 + 0.05 +
                           (float(ii) + 0.5) * cellW
                           + (firstWrap * cellW)
                           - wrapScroll;
            vec2 cellLocal = vec2(p.x - cellCX, p.y - rowCY);
            // bail if pixel is well outside this cell horizontally and
            // outside the row band — saves shader cost on most pixels.
            if (abs(cellLocal.x) > cellW*0.5 + 0.06) continue;
            if (abs(cellLocal.y) > rowH*0.5 + 0.04) continue;

            // deterministic content choice per (row, cellId)
            float seed = float(cellId) * 13.7 + fri * 31.1;
            float chooser = h11(seed);
            // chance of text increases when there are more words to lay down
            float textProb = clamp(float(wordsAvail) / float(rows * items), 0.10, 0.55);
            bool isText = (wordsAvail > 0) && (chooser < textProb);

            // small per-cell parallax-y jitter (Player A jitters row 0 hard)
            vec2 jitter = vec2(0.0);
            if (ri == 0) {
                jitter.x = (h11(seed + 1.7) - 0.5) * 0.020 * (0.5 + 1.6*energyA);
                jitter.y = (h11(seed + 2.3) - 0.5) * 0.015 * (0.5 + 1.6*energyA);
            } else {
                jitter.x = (h11(seed + 1.7) - 0.5) * 0.012 * (0.3 + eRow);
                jitter.y = (h11(seed + 2.3) - 0.5) * 0.010 * (0.3 + eRow);
            }
            // gentle bob on top of jitter — alive even at silence
            jitter.y += 0.006 * sin(t * (0.5 + 0.4*depth01) + seed);
            cellLocal -= jitter;

            // cutout scale: random per-cell with row-depth bias (back smaller)
            float scale = mix(0.42, 0.62, h11(seed + 3.1));
            scale *= mix(0.78, 1.10, depth01);   // back rows slightly smaller
            // Player A pulses row 0 sizes; bass adds an overall lift
            if (ri == 0) scale *= 1.0 + 0.18*energyA*sin(t*4.0 + seed);
            scale *= 1.0 + 0.08*bass;

            // scaled local
            vec2 lp = cellLocal / scale;

            if (isText) {
                // pick which word this cell shows: cellId-derived index
                // mod wordCount so cells reuse words as we scroll
                int wIdx = int(mod(float(cellId) * 1.0, float(max(wordsAvail, 1))));
                if (wIdx < 0) wIdx = 0;
                int wS, wE;
                getWord(wIdx, total, wS, wE);
                if (wE <= wS) continue;

                // text "cell size" for layout helper
                vec2 tcSz = vec2(cellW * 0.94, rowH * 0.78);
                float trailHit = 0.0;
                float m = renderParensWord(cellLocal, tcSz, wS, wE, revealed, trailHit);
                if (m > 0.001) {
                    // text is inked at full strength + a soft halo behind it
                    rowCol = mix(rowCol, P.ink, m);
                    rowAlpha = max(rowAlpha, m);
                    // shadow: soft offset replica
                    vec2 shLocal = cellLocal - vec2(shOff, -shOff);
                    float ms = renderParensWord(shLocal, tcSz, wS, wE, revealed, trailHit);
                    ms = smoothstep(0.0, 0.7, ms);
                    shadowCol = mix(shadowCol, P.shadow, ms * 0.45);
                    shadowAlpha = max(shadowAlpha, ms * 0.45);
                }
                // typewriter caret hint on the freshest cell (last word
                // currently appearing)
                if (trailHit > 0.5 && liveUtterance) {
                    float caret = step(0.5, fract(TIME * 1.7));
                    rowCol = mix(rowCol, P.accent, caret * 0.0);
                }
            } else {
                // cutout: archetype id stable per (row, cellId)
                int kind = int(mod(floor(h11(seed + 4.1) * 8.0), 8.0));
                float d  = cutoutSdf(kind, lp);
                // fwidth AA in local-space; scale up by 1/scale for screen space
                float fw = fwidth(d) + 1e-4;
                float fill = 1.0 - smoothstep(-fw, fw, d);
                if (fill > 0.002) {
                    float shade = cutoutShade(kind, lp, lvl);
                    // Final cutout color: ink * shade with a subtle accent rim
                    vec3 cutCol = mix(P.ink, P.accent, 0.18);
                    cutCol *= mix(0.45, 1.0, shade);
                    // edge contour: thin dark stroke at silhouette
                    float edge = exp(-pow((d / max(fw, 1e-4)) * 0.6, 2.0));
                    cutCol *= 1.0 - 0.25 * edge;
                    // composite over row
                    rowCol = mix(rowCol, cutCol, fill);
                    rowAlpha = max(rowAlpha, fill);
                    // shadow plane: SDF offset down-right, larger blur
                    vec2 shLp = (cellLocal - vec2(shOff, -shOff)) / scale;
                    float ds = cutoutSdf(kind, shLp);
                    float shFw = fwidth(ds) + shBlur*8.0;
                    float shFill = 1.0 - smoothstep(-shFw, shFw, ds);
                    shadowCol = mix(shadowCol, P.shadow, shFill * 0.55);
                    shadowAlpha = max(shadowAlpha, shFill * 0.55);
                }
            }
        }

        // Composite shadow first (so it lands on the rows BEHIND), then
        // the row contents. The accumulated `col` already contains all
        // previously-painted (further-back) rows + paper.
        col = mix(col, shadowCol, shadowAlpha * 0.85);
        col = mix(col, rowCol,   rowAlpha);
    }

    // ─── footer rule + tiny tag (echo the ref's "Creative Concept") ────
    {
        float y0 = -0.44;
        float rule = smoothstep(0.003, 0.0, abs(p.y - y0));
        col = mix(col, P.ink, rule * 0.6);
        // a small ink dot bottom-left
        float dot1 = smoothstep(0.012, 0.000, length(p - vec2(-aspect*0.42, -0.46)));
        col = mix(col, P.ink, dot1);
    }

    // ─── grain + sheen ─────────────────────────────────────────────────
    float grain = fbm2(uv * res.y * 0.018) * 0.5 + fbm2(uv * res.y * 0.06 + 9.0) * 0.5;
    col *= 1.0 + (grain - 0.5) * 0.04;
    // raking sheen drifting slowly across canvas — quiet polish
    float sheen = smoothstep(0.0, 0.45, sin(p.x*1.2 - p.y*0.5 - t*0.18)*0.5 + 0.5);
    col += pow(sheen, 4.0) * 0.04 * P.accent;
    // bass lift
    col *= 1.0 + 0.06 * bass;

    float alpha = 1.0;
    if (transparentBg) {
        // alpha = how much we painted on top of paper. Approximated from
        // the difference between final and paper luminance.
        float dL = length(col - P.paper);
        alpha = clamp(dL * 2.5, 0.0, 1.0);
    }
    gl_FragColor = vec4(col, alpha);
}
