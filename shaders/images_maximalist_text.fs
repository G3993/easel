/*{
  "DESCRIPTION": "Images Maximalist Text — a dense editorial collage. Dozens of cutout image-rectangles tumble across multiple parallax z-planes — magazine clippings, polaroids, sticker scraps — while bold typographic text reflows underneath as the cue typewriter speaks. Three player channels each animate one cutout DRAWER (foreground hand-stickers, mid-plane photo polaroids, back-plane wallpaper tiles): a louder player floods their drawer with more, brighter, jitterier scraps. audio.bass scales total density (the collage breathes), audio.high adds tape-grain. cue.latest typewrites the headline-sized text running behind/between the cutouts. Real depth via per-plane parallax and depth-of-field haze. Maximalist, exuberant, curated — not a moodboard, an editorial spread.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "TYPE": "text", "DEFAULT": "WE LOOK AT ALL THE DIFFERENT TOPICS IN THE WORLD", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "frontDrawer", "LABEL": "Front Drawer Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "midDrawer",   "LABEL": "Mid Drawer Energy",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "backDrawer",  "LABEL": "Back Drawer Energy",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].energy" },

    { "NAME": "audioDensity","LABEL": "Bass → Density",      "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.8, "BIND": "audio.bass" },
    { "NAME": "grain",       "LABEL": "High → Tape Grain",   "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.6, "BIND": "audio.high" },

    { "NAME": "imageCount",  "LABEL": "Cutouts per Plane",   "TYPE": "long",  "DEFAULT": 14, "VALUES": [8,10,12,14,18,22], "LABELS": ["8","10","12","14","18","22"] },
    { "NAME": "densityBias", "LABEL": "Density Bias",        "TYPE": "float", "MIN": 0.3, "MAX": 1.8, "DEFAULT": 1.0 },
    { "NAME": "paletteMix",  "LABEL": "Palette Mix",         "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Editorial","Newsprint","Pop","Risograph"] },
    { "NAME": "motionSpeed", "LABEL": "Motion",              "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "parallaxAmt", "LABEL": "Parallax Strength",   "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "textSize",    "LABEL": "Headline Size",       "TYPE": "float", "MIN": 0.6, "MAX": 1.8, "DEFAULT": 1.0 },
    { "NAME": "textInk",     "LABEL": "Headline Ink",        "TYPE": "color", "DEFAULT": [0.04, 0.04, 0.05, 1.0] },
    { "NAME": "paperTint",   "LABEL": "Paper Tint",          "TYPE": "color", "DEFAULT": [0.96, 0.94, 0.90, 1.0] },
    { "NAME": "fog",         "LABEL": "Depth Haze",          "TYPE": "float", "MIN": 0.0, "MAX": 1.5, "DEFAULT": 0.7 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  IMAGES MAXIMALIST TEXT  ·  editorial cutout collage, 3 player drawers
//
//  The reference is a Seoul Design Festival-style maximalist composition:
//  a grid of cutout-photo stickers stacked over dense letterpress text that
//  fills the whole page. Here that becomes THREE PARALLAX PLANES of
//  procedural cutout rectangles ("image scraps") layered over a
//  typewritten headline grid:
//
//   plane 0 (back, z=-2)  : wallpaper tiles    ← player[3].energy
//   plane 1 (mid,  z= 0)  : photo polaroids    ← player[2].energy
//   plane 2 (front, z=+1) : hand stickers      ← player[1].energy
//
//  Each plane drifts at its own parallax rate; louder player floods their
//  plane with more scraps (count fade), brighter scrap interiors, and
//  jittery rotation. audio.bass globally scales density (the spread
//  breathes); audio.high adds analog tape grain. The cue typewrites a
//  giant editorial headline that reflows behind/around the cutouts —
//  the text never lives inside one cell, it OWNS THE PAGE.
//
//  Anti-pattern guards:
//   - no horizon line, no symmetry, no scoreboard, no spectrum bars
//   - text is the cue (live transcript), not decorative glyphs
//   - cutouts are abstract colored frames with fbm "photo" content — they
//     evoke clippings without depicting any literal object
//   - smoothing is host-side (per-binding); the shader treats inputs as
//     already-shaped values per the intelligence-layer contract.
// ════════════════════════════════════════════════════════════════════════

const float TAU = 6.28318530718;
const float PI  = 3.14159265359;

#define MAX_CUTOUTS 22

// ─── Font atlas (msg_0..msg_47, msg_len, msgAge injected by host) ──────
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

// ─── Hash / noise ──────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  hash21(float n) { return vec2(hash11(n), hash11(n + 17.31)); }
vec3  hash31(float n) { return vec3(hash11(n), hash11(n + 31.7), hash11(n + 71.3)); }
vec4  hash41(float n) { return vec4(hash11(n), hash11(n+13.1), hash11(n+27.7), hash11(n+41.9)); }

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
        p = p * 2.03 + vec2(7.7, 3.1);
        a *= 0.55;
    }
    return v;
}

// ─── Palette ───────────────────────────────────────────────────────────
vec3 pickPalette(float t, int mode) {
    // mode 0 Editorial : warm paper + bold cmy primaries
    // mode 1 Newsprint : muted greys + ink red
    // mode 2 Pop       : saturated playgroundprimaries
    // mode 3 Risograph : flat duotone (fluor pink / azure / mustard)
    if (mode == 0) {
        vec3 a = vec3(0.94, 0.20, 0.18);  // ink red
        vec3 b = vec3(0.18, 0.34, 0.78);  // sky blue
        vec3 c = vec3(0.98, 0.78, 0.20);  // canary yellow
        vec3 d = vec3(0.18, 0.62, 0.36);  // grass green
        vec3 e = vec3(0.18, 0.16, 0.18);  // near-black
        if (t < 0.2) return a;
        if (t < 0.4) return b;
        if (t < 0.6) return c;
        if (t < 0.8) return d;
        return e;
    } else if (mode == 1) {
        vec3 a = vec3(0.86, 0.84, 0.80);
        vec3 b = vec3(0.32, 0.32, 0.32);
        vec3 c = vec3(0.82, 0.22, 0.18);
        vec3 d = vec3(0.55, 0.55, 0.52);
        if (t < 0.3) return a;
        if (t < 0.55) return b;
        if (t < 0.80) return c;
        return d;
    } else if (mode == 2) {
        vec3 a = vec3(1.00, 0.40, 0.62);  // pink
        vec3 b = vec3(0.25, 0.78, 0.96);  // cyan
        vec3 c = vec3(1.00, 0.86, 0.30);  // sun yellow
        vec3 d = vec3(0.46, 0.30, 0.84);  // purple
        vec3 e = vec3(0.20, 0.78, 0.55);  // mint
        if (t < 0.2) return a;
        if (t < 0.4) return b;
        if (t < 0.6) return c;
        if (t < 0.8) return d;
        return e;
    }
    // Riso
    vec3 a = vec3(1.00, 0.32, 0.68);
    vec3 b = vec3(0.22, 0.42, 0.92);
    vec3 c = vec3(0.96, 0.78, 0.18);
    if (t < 0.34) return a;
    if (t < 0.67) return b;
    return c;
}

// ─── Rotated rectangle SDF (cutout silhouette) ─────────────────────────
// Returns signed distance to the rotated rect AABB (half-extents `he`)
// centered at `c`. Negative inside. Also fills `local` with the local
// (un-rotated) coords so the caller can paint the rect interior.
float sdRotRect(vec2 p, vec2 c, vec2 he, float rot, out vec2 local) {
    vec2 q = p - c;
    float co = cos(rot), si = sin(rot);
    local = vec2(co * q.x + si * q.y, -si * q.x + co * q.y);
    vec2 d = abs(local) - he;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

// Paint the interior of a cutout in its LOCAL (un-rotated) coords:
// frame stripe, photo fill, optional sticker dot. Returns RGB.
vec3 paintCutout(vec2 local, vec2 he, float seed, int palMode, float bright) {
    // Photo fill — abstract fbm tinted with two palette swatches.
    vec2 uv = (local / he) * 0.5 + 0.5;
    float n = fbm2(uv * 4.5 + seed * 11.0);
    float n2 = fbm2(uv * 1.8 - seed * 3.7);
    vec3 cA = pickPalette(fract(seed * 0.31), palMode);
    vec3 cB = pickPalette(fract(seed * 0.71 + 0.27), palMode);
    vec3 cC = pickPalette(fract(seed * 0.13 + 0.55), palMode);
    vec3 inner = mix(cA, cB, smoothstep(0.30, 0.70, n));
    inner = mix(inner, cC, smoothstep(0.55, 0.92, n2) * 0.55);

    // Photo "subject" shape — a soft elliptical blob biased by seed,
    // so each cutout has its own visual focal point.
    vec2 focus = vec2((hash11(seed*9.1)-0.5)*0.5, (hash11(seed*17.7)-0.5)*0.5);
    float subj = exp(-dot((local/he)-focus,(local/he)-focus) * 5.5);
    inner += subj * 0.35 * pickPalette(fract(seed*0.91), palMode);

    // White paper border ("printed margin"): masked by proximity to edge.
    float borderW = max(min(he.x, he.y) * 0.12, 0.005);
    vec2 inset = abs(local) - (he - borderW);
    float edge = max(inset.x, inset.y);
    float onBorder = smoothstep(-0.5*borderW, 0.0, edge);
    vec3 border = vec3(0.97, 0.96, 0.92);

    // Occasional sticker dot — a saturated bullseye on ~1/3 of cutouts.
    float dot1 = step(0.66, hash11(seed * 5.1));
    vec2  dotC = (hash21(seed * 3.3) - 0.5) * he * 1.1;
    float dotR = min(he.x, he.y) * 0.18;
    float dotMask = dot1 * (1.0 - smoothstep(dotR*0.6, dotR, length(local - dotC)));
    vec3 dotCol = pickPalette(fract(seed * 0.42 + 0.18), palMode);
    inner = mix(inner, dotCol, dotMask * 0.85);

    vec3 col = mix(inner, border, onBorder);
    col *= bright;

    // Tiny inner shadow toward the bottom-right — a corner-curled paper hint.
    float shadow = smoothstep(0.0, 1.0, (local.x - local.y) / (he.x + he.y) * 0.5 + 0.5);
    col *= mix(0.94, 1.04, shadow);
    return col;
}

// ─── Single plane composite ────────────────────────────────────────────
// Walks N cutouts on one plane, returns premultiplied RGB+A so the caller
// can layer planes back-to-front. `planeOffset` parallaxes the plane,
// `density` (0..1) crossfades each scrap individually so player energy
// floods more scraps in without changing scene topology.
vec4 renderPlane(vec2 p, int planeIdx, int n, float density,
                 float t, vec2 planeOffset, float jitter,
                 int palMode, float bright, float aspect) {
    vec4 acc = vec4(0.0);
    float fp = float(planeIdx);
    // Per-plane scale of the cutout footprint (front bigger, back smaller).
    float scaleBase = (planeIdx == 2) ? 0.20 : (planeIdx == 1) ? 0.15 : 0.105;
    // Cutout aspect range (some landscape, some portrait, some square).
    for (int i = 0; i < MAX_CUTOUTS; i++) {
        if (i >= n) break;
        float fi = float(i);
        vec4 h4 = hash41(fp * 53.1 + fi * 7.91);
        // base anchor inside [-0.5*aspect, 0.5*aspect] x [-0.5, 0.5]
        vec2 anchor;
        anchor.x = (h4.x - 0.5) * (aspect - 0.05);
        anchor.y = (h4.y - 0.5) * 0.94;
        // Slow drift + parallax offset; jitter scales with player energy.
        float driftSpeed = 0.10 + 0.18 * h4.z + 0.20 * fp;
        float driftAmp   = 0.03 + 0.04 * h4.w;
        anchor.x += sin(t * driftSpeed + fi * 1.7) * driftAmp;
        anchor.y += cos(t * driftSpeed * 0.83 + fi * 2.3) * driftAmp;
        anchor   += planeOffset;
        // Jitter: a high-frequency shimmy when player energy is loud —
        // gives the "I'm peeling the sticker off" hand-felt motion.
        if (jitter > 0.001) {
            anchor.x += sin(t * 7.3 + fi * 3.1) * 0.006 * jitter;
            anchor.y += cos(t * 6.7 + fi * 4.7) * 0.006 * jitter;
        }

        // Half-extents: 0.55..1.45 multiplier in each axis on scaleBase.
        vec2 he = scaleBase * vec2(mix(0.55, 1.45, h4.x), mix(0.55, 1.45, h4.y));
        // 1/4 of cutouts go portrait, 1/4 landscape, 1/2 near-square via
        // re-rolling — gives editorial-grid variety.
        if (h4.z < 0.25)      he.y *= 1.6;
        else if (h4.z < 0.50) he.x *= 1.6;

        // Rotation: small for back plane, bigger for front sticker plane.
        float rotMax = (planeIdx == 2) ? 0.6 : (planeIdx == 1) ? 0.35 : 0.18;
        float rot = (h4.w - 0.5) * 2.0 * rotMax
                  + jitter * 0.10 * sin(t * 3.7 + fi * 2.1);

        vec2 local;
        float d = sdRotRect(p, anchor, he, rot, local);

        // Anti-aliased fill mask + density crossfade (per-scrap birth).
        float fw = fwidth(d);
        float fill = 1.0 - smoothstep(-fw, fw, d);
        // density (0..1) thresholds which scraps are "out of the drawer".
        float scrapBirth = smoothstep(h4.x, h4.x + 0.18, density);
        fill *= scrapBirth;
        if (fill < 0.001) continue;

        vec3 col = paintCutout(local, he, hash11(fp * 19.7 + fi * 31.1), palMode, bright);

        // Drop shadow — soft, offset by a few pixels in local space,
        // gives layering against the layer behind.
        vec2 shadowLocal = local - vec2(0.012, -0.012);
        vec2 shadowInset = abs(shadowLocal) - he;
        float shadowD = length(max(shadowInset, 0.0)) + min(max(shadowInset.x, shadowInset.y), 0.0);
        float shadowMask = (1.0 - smoothstep(-0.025, 0.0, shadowD)) * 0.35;
        // pre-composite shadow behind everything in this plane via "under"
        vec4 shadowCol = vec4(0.0, 0.0, 0.0, shadowMask * (1.0 - fill));
        acc.rgb = acc.rgb + (1.0 - acc.a) * shadowCol.rgb * shadowCol.a;
        acc.a   = acc.a + (1.0 - acc.a) * shadowCol.a;

        // Composite this scrap on top of accumulated plane.
        vec4 srcCol = vec4(col, fill);
        acc.rgb = acc.rgb + (1.0 - acc.a) * srcCol.rgb * srcCol.a;
        acc.a   = acc.a + (1.0 - acc.a) * srcCol.a;
        if (acc.a > 0.995) break;
    }
    return acc;
}

// ─── Headline text grid ────────────────────────────────────────────────
// Lays the full cue message into a left-aligned multi-row block that
// fills the canvas, word-wrapping at row boundaries. Returns ink mask.
float renderHeadline(vec2 p, float aspect, float baseCharH, out float inkAlpha) {
    inkAlpha = 0.0;
    int total = charCount();
    if (total <= 0) return 0.0;

    // Box: full canvas with small inset margin.
    float marginX = 0.04;
    float marginY = 0.06;
    float boxW = aspect - 2.0 * marginX;
    float boxH = 1.0 - 2.0 * marginY;
    // Glyph height & pitch — derived from textSize, capped to keep
    // the headline readable but page-filling.
    float charH = clamp(baseCharH, 0.05, 0.18);
    float charW = charH * (5.0 / 7.0);
    // Kerning: tight, editorial — lock to roughly 1.05× glyph width.
    float kern  = charW * 1.05;
    int charsPerRow = int(floor(boxW / kern));
    if (charsPerRow < 6) charsPerRow = 6;
    if (charsPerRow > 26) charsPerRow = 26;

    // Local p relative to top-left of box.
    vec2 tl = vec2(-aspect * 0.5 + marginX, 0.5 - marginY);
    float lx = p.x - tl.x;
    float ly = tl.y - p.y;     // flip so rows increase downward
    if (lx < 0.0 || lx > boxW) return 0.0;
    if (ly < 0.0 || ly > boxH) return 0.0;

    // Leading: tight, slightly larger than 1.0 so rows kiss but don't touch.
    float LEADING = 1.08;
    float linePitch = charH * LEADING;
    int targetRow = int(floor(ly / linePitch));
    int targetCol = int(floor(lx / kern));
    if (targetCol >= charsPerRow) return 0.0;

    int maxRows = int(floor(boxH / linePitch));
    if (maxRows < 1) maxRows = 1;

    // Word-wrap walk: cycle the message so the page is FULL of text
    // (editorial "wall of type"). When the cue message is short, it
    // repeats with a single space between repetitions; the cue still
    // reads as the dominant phrase because of typewriter timing.
    int cursorR = 0;
    int cursorC = 0;
    int outCh = -1;
    const int MAX_WALK = 256;
    for (int i = 0; i < MAX_WALK; i++) {
        if (cursorR > targetRow) break;
        if (cursorR == maxRows) break;
        // Cycle through the message; insert a single space between cycles
        // so the wrap algorithm sees a separator.
        int cyc = i % (total + 1);
        int ch = (cyc == total) ? 26 /*SPACE*/ : getChar(cyc);

        if (ch == 26) {
            // Look ahead to next word length in the cycled stream.
            int wlen = 0;
            for (int j = 1; j < MAX_WALK; j++) {
                int cycJ = (i + j) % (total + 1);
                int chj = (cycJ == total) ? 26 : getChar(cycJ);
                if (chj == 26 || chj < 0 || chj > 35) break;
                wlen++;
            }
            if (cursorC > 0 && cursorC + 1 + wlen > charsPerRow) {
                cursorR++; cursorC = 0;
            } else if (cursorC > 0) {
                if (cursorR == targetRow && cursorC == targetCol) outCh = 26;
                cursorC++;
            }
        } else if (ch >= 0 && ch <= 35) {
            if (cursorR == targetRow && cursorC == targetCol) outCh = ch;
            cursorC++;
            if (cursorC >= charsPerRow) { cursorR++; cursorC = 0; }
        }
    }

    if (outCh < 0 || outCh > 35) return 0.0;

    // Glyph cell.
    float colPad = (kern - charW) * 0.5;
    float rowPad = (linePitch - charH) * 0.5;
    float gx = lx - float(targetCol) * kern - colPad;
    float gy = ly - float(targetRow) * linePitch - rowPad;
    if (gx < 0.0 || gx > charW) return 0.0;
    if (gy < 0.0 || gy > charH) return 0.0;
    vec2 cellLocal = vec2(gx / charW, 1.0 - gy / charH);
    float s = sampleChar(outCh, cellLocal);
    s = smoothstep(0.20, 0.55, s);
    inkAlpha = s;
    return s;
}

// ─── Tape grain (high-freq, audio.high modulated) ──────────────────────
float tapeGrain(vec2 fragXY, float t, float amount) {
    float g  = hash11(dot(floor(fragXY * 1.7), vec2(12.989, 78.233)) + floor(t * 24.0));
    float g2 = hash11(dot(floor(fragXY * 0.85), vec2(53.71, 17.13)) + floor(t * 12.0));
    return ((g - 0.5) * 0.7 + (g2 - 0.5) * 0.3) * amount;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;
    // Aspect-corrected, centered coords (range roughly [-aspect/2,+aspect/2] x [-0.5,0.5])
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    float t = TIME * motionSpeed;
    float bass = clamp(audioDensity, 0.0, 2.0);
    float high = clamp(grain, 0.0, 1.5);

    // ── Paper backdrop with subtle marbled paper texture ──────────────
    vec3 paperA = paperTint.rgb;
    vec3 paperB = paperA * vec3(0.94, 0.93, 0.90);
    float marble = fbm2(uv * 2.7 + vec2(t * 0.05, -t * 0.04));
    vec3 paper = mix(paperA, paperB, marble * 0.7);
    paper *= 1.0 - 0.10 * dot(uv - 0.5, uv - 0.5);   // soft vignette

    // ── Headline text painted FIRST as a wallpaper of type ────────────
    // The typewriter (msg_len growing with cue.latest) reveals it left→right,
    // top→bottom because charCount() reflects the current msg length.
    float baseCharH = 0.085 * textSize;
    float inkA = 0.0;
    renderHeadline(p, aspect, baseCharH, inkA);
    // Ink is slightly varied per glyph row by a low-freq fbm so the
    // type reads "letterpress" not "vector".
    float typewear = 0.85 + 0.30 * fbm2(p * 4.0 + 13.7);
    vec3 ink = textInk.rgb * typewear;
    vec3 col = mix(paper, ink, inkA);

    // ── Three parallax cutout planes (back → mid → front) ─────────────
    int n = int(imageCount);
    if (n > MAX_CUTOUTS) n = MAX_CUTOUTS;

    int palMode = int(paletteMix);

    // Density per plane: player[i].energy → fraction of scraps shown.
    // densityBias scales globally; audio.bass adds a breathing pump
    // so even silent players push scraps out on a bass hit.
    float globalDensity = clamp(densityBias + 0.30 * bass, 0.20, 1.6);
    float fD = clamp((0.35 + 0.70 * frontDrawer) * globalDensity, 0.0, 1.6);
    float mD = clamp((0.55 + 0.55 * midDrawer)   * globalDensity, 0.0, 1.6);
    float bD = clamp((0.75 + 0.40 * backDrawer)  * globalDensity, 0.0, 1.6);

    // Per-plane parallax offsets. Slow & subtle on back, big & lively on front.
    float pAmt = parallaxAmt;
    vec2 mouseLook = (mousePos - 0.5);
    vec2 offBack  = vec2(sin(t*0.07)*0.04, cos(t*0.05)*0.03) * pAmt + mouseLook * 0.04 * pAmt;
    vec2 offMid   = vec2(sin(t*0.11)*0.07, cos(t*0.09)*0.06) * pAmt + mouseLook * 0.10 * pAmt;
    vec2 offFront = vec2(sin(t*0.17)*0.11, cos(t*0.14)*0.09) * pAmt + mouseLook * 0.18 * pAmt;

    // Per-plane jitter: each player's energy adds jittery rotation.
    float jBack  = backDrawer  * 0.9;
    float jMid   = midDrawer   * 1.0;
    float jFront = frontDrawer * 1.2;

    // Per-plane brightness: louder players brighten their scraps.
    float brBack  = mix(0.78, 1.10, backDrawer);
    float brMid   = mix(0.86, 1.18, midDrawer);
    float brFront = mix(0.92, 1.28, frontDrawer);

    // Per-plane fog (back plane fades into paper haze).
    float fogBack  = 0.55 * fog;
    float fogMid   = 0.25 * fog;
    float fogFront = 0.05 * fog;

    // BACK plane (wallpaper tiles) — smaller scraps tinted toward paper.
    vec4 backPlane = renderPlane(p, 0, n, bD, t, offBack, jBack, palMode, brBack, aspect);
    backPlane.rgb = mix(backPlane.rgb, paper, fogBack);
    col = mix(col, backPlane.rgb, backPlane.a);
    // Re-apply ink so type sits ABOVE the back wallpaper but UNDER mid/front.
    col = mix(col, ink, inkA * 0.88);

    // MID plane (photo polaroids) — mid-size with photo fbm fills.
    vec4 midPlane = renderPlane(p, 1, n, mD, t, offMid, jMid, palMode, brMid, aspect);
    midPlane.rgb = mix(midPlane.rgb, paper, fogMid);
    col = mix(col, midPlane.rgb, midPlane.a);

    // FRONT plane (hand stickers) — biggest, brightest, jitteriest.
    vec4 frontPlane = renderPlane(p, 2, n, fD, t, offFront, jFront, palMode, brFront, aspect);
    frontPlane.rgb = mix(frontPlane.rgb, paper, fogFront);
    col = mix(col, frontPlane.rgb, frontPlane.a);

    // ── Top-line headline: a thin band of ink stays VISIBLE above the
    //    front plane in a narrow band, simulating a chromed editorial
    //    masthead. Keeps text legible even with dense cutouts.
    float mastBand = smoothstep(0.42, 0.49, p.y) - smoothstep(0.49, 0.50, p.y);
    col = mix(col, ink, inkA * mastBand * 0.55);

    // ── Tape grain (audio.high) + paper fiber noise ───────────────────
    vec2 fragXY = gl_FragCoord.xy;
    float grainAmt = (0.06 + 0.18 * high) * (0.6 + 0.6 * frontDrawer);
    col += tapeGrain(fragXY, TIME, grainAmt) * vec3(1.0, 0.97, 0.93);

    float fiber = fbm2(uv * res.y * 0.011) * 0.06;
    col *= (1.0 - 0.5 * fiber + 0.25);

    // ── Subtle bottom-edge ink "press registration" marks ─────────────
    // four tiny rotated swatches near the bottom — a printers'-marks nod
    float regBand = smoothstep(-0.49, -0.46, p.y) - smoothstep(-0.46, -0.45, p.y);
    if (regBand > 0.0) {
        // 4 colored ticks at fixed positions
        for (int k = 0; k < 4; k++) {
            float kf = float(k);
            float xc = -aspect * 0.5 + 0.10 + kf * 0.06;
            float d  = abs(p.x - xc);
            float tick = (1.0 - smoothstep(0.005, 0.012, d)) * regBand;
            vec3 swCol = (k == 0) ? vec3(0.95, 0.15, 0.15)
                       : (k == 1) ? vec3(0.10, 0.45, 0.95)
                       : (k == 2) ? vec3(0.96, 0.82, 0.18)
                       :            vec3(0.10, 0.10, 0.10);
            col = mix(col, swCol, tick * 0.85);
        }
    }

    // ── Final color polish ────────────────────────────────────────────
    // Mild s-curve so cutouts pop against paper without crushing.
    col = col / (1.0 + 0.18 * max(col, vec3(0.0)));
    col = pow(max(col, 0.0), vec3(0.95));

    gl_FragColor = vec4(col, 1.0);
}
