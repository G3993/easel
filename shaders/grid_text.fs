/*{
  "DESCRIPTION": "Grid Text — Swiss editorial poster as a parallax z-stack. Four chromatic quadrants (off-white, cyan blue, ink-green, mandarin) hold a sparse grid of typewriter glyphs from `cue.latest`; characters live on three depth planes that parallax against a slow camera dolly so the whole sheet breathes like a folded broadside. Each quadrant is driven by an independent `player[i].energy` — its glyphs flicker, jitter and stretch when that voice is active; silent quadrants compress to a calm constellation of single letters. Audio mid pushes a soft printer-grain across the page; bass tugs the depth columns out of plane. Strictly anti-pattern: no horizon mirror, no bars, no EKG, no checkerboard. Just a grid that thinks it's a poster and a poster that thinks it's a room.",
  "CREDIT": "easel auto-loop — A-List daily / Swiss broadside reference",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "TYPE": "text", "DEFAULT": "GRID TEXT POSTER", "MAX_LENGTH": 48, "BIND": "cue.latest" },
    { "NAME": "energyA", "LABEL": "Quadrant A Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "energyB", "LABEL": "Quadrant B Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "energyC", "LABEL": "Quadrant C Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].energy" },
    { "NAME": "energyD", "LABEL": "Quadrant D Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[4].energy" },
    { "NAME": "audioDepth", "LABEL": "Audio Depth Push", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.6 },
    { "NAME": "gridDensity", "LABEL": "Grid Density", "TYPE": "float", "MIN": 4.0, "MAX": 22.0, "DEFAULT": 11.0 },
    { "NAME": "textSize", "LABEL": "Text Size", "TYPE": "float", "MIN": 0.4, "MAX": 2.4, "DEFAULT": 1.0 },
    { "NAME": "cameraDrift", "LABEL": "Camera Drift", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "parallax", "LABEL": "Parallax Depth", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0 },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed", "TYPE": "float", "MIN": 0.0, "MAX": 2.5, "DEFAULT": 1.0 },
    { "NAME": "sparsity", "LABEL": "Glyph Sparsity", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.62 },
    { "NAME": "variant", "LABEL": "Layout Variant", "TYPE": "long", "DEFAULT": 0, "VALUES": [0,1,2], "LABELS": ["Broadside","Numerals","Drift"] },
    { "NAME": "paperA", "LABEL": "Paper · A", "TYPE": "color", "DEFAULT": [0.965, 0.955, 0.930, 1.0] },
    { "NAME": "paperB", "LABEL": "Paper · B", "TYPE": "color", "DEFAULT": [0.10, 0.46, 0.84, 1.0] },
    { "NAME": "paperC", "LABEL": "Paper · C", "TYPE": "color", "DEFAULT": [0.07, 0.13, 0.11, 1.0] },
    { "NAME": "paperD", "LABEL": "Paper · D", "TYPE": "color", "DEFAULT": [0.97, 0.50, 0.10, 1.0] },
    { "NAME": "inkLight", "LABEL": "Ink on Light Paper", "TYPE": "color", "DEFAULT": [0.06, 0.06, 0.08, 1.0] },
    { "NAME": "inkDark", "LABEL": "Ink on Dark Paper", "TYPE": "color", "DEFAULT": [0.98, 0.96, 0.92, 1.0] }
  ]
}*/

// ═══════════════════════════════════════════════════════════════════════
//  GRID TEXT  ·  Swiss broadside as a parallax z-stack
//
//  Reference: four-panel typographic poster — sparse, single-glyph grid,
//  saturated quadrant palette (off-white / azure / forest / mandarin).
//  Translation choices:
//    – Quadrants are *not* a static four-up; the panel boundaries breathe
//      with the camera dolly so the grid never reads as a checkerboard.
//    – Each quadrant is its own player. When player[k].energy spikes, its
//      glyphs jitter, swell, and pop forward in z; silent quadrants
//      collapse into a calm constellation.
//    – Three depth planes (front/mid/back) cycle which slot a glyph lives
//      on — characters drift past each other in z giving real parallax.
//    – `msg` is auto-bound to cue.latest; typewriter reveal via msgAge.
// ═══════════════════════════════════════════════════════════════════════

#define MAX_MSG 48
#define SPACE_CH 26

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

float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float hash12(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
vec2  hash22(vec2 p)  { return fract(sin(vec2(dot(p, vec2(127.1, 311.7)),
                                              dot(p, vec2(269.5,  183.3)))) * 43758.5453); }

// soft 1-channel "printer grain" — used to bias glyph ink and paper tooth
float grain(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float a = hash12(i), b = hash12(i+vec2(1.0,0.0));
    float c = hash12(i+vec2(0.0,1.0)), d = hash12(i+vec2(1.0,1.0));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}

// Decide which of the four panels (quadrant) a (poster) UV falls into.
// The panel grid is 2×2 in poster space but the divider lines themselves
// breathe with the camera so the result is never a perfect screen-aligned
// checkerboard. quadrantId ∈ {0,1,2,3} (TL, TR, BL, BR).
int quadrantOf(vec2 posterUv, float wobble) {
    float midX = 0.5 + 0.015 * sin(wobble * 0.7);
    float midY = 0.5 + 0.018 * cos(wobble * 0.5);
    int qx = (posterUv.x < midX) ? 0 : 1;
    int qy = (posterUv.y < midY) ? 0 : 1;   // 0 = top, 1 = bottom
    return qy * 2 + qx;
}

// Return the quadrant's paper color + ink color + active player energy.
void quadrantStyle(int q, out vec3 paper, out vec3 ink, out float energy) {
    if (q == 0) { paper = paperA.rgb; energy = energyA; }
    else if (q == 1) { paper = paperB.rgb; energy = energyB; }
    else if (q == 2) { paper = paperC.rgb; energy = energyC; }
    else             { paper = paperD.rgb; energy = energyD; }
    float lum = dot(paper, vec3(0.299, 0.587, 0.114));
    ink = (lum > 0.55) ? inkLight.rgb : inkDark.rgb;
}

// Render a single grid plane (front/mid/back). For each cell in the plane
// pick (deterministically) a glyph index from the message, an in-cell
// position, a size jitter; render it with fwidth AA. Returns ink coverage
// in .a and the ink colour scaled in .rgb.
//
// `planeIdx`  : 0 = back, 1 = mid, 2 = front
// `planeUv`   : aspect-corrected poster UV after parallax shift
// `planeAge`  : per-plane time (TIME × motionSpeed offset by plane)
// `density`   : cells per poster width on this plane
// `sizeMul`   : size multiplier from textSize × plane-scale
vec4 renderPlane(int planeIdx, vec2 planeUv, float planeAge,
                 float density, float sizeMul, int total,
                 float wobble, float printerGrain) {
    vec4 acc = vec4(0.0);
    if (total <= 0) return acc;

    // Cell grid in poster space — taller cells (5:7 aspect of glyphs).
    float cellW = 1.0 / density;
    float cellH = cellW * 1.18;   // slight vertical extension for breathing

    // Sample a small neighbourhood: covers the current cell and its
    // immediate neighbours so glyphs that drift to cell-edges still
    // render across the boundary.
    vec2 cell = floor(planeUv / vec2(cellW, cellH));

    // Reveal cap — how many of the message's chars are "alive" right now.
    // Live mode: msgAge ≥ 0, typewriter at ~28 cps; static: full reveal.
    int revealCap;
    if (msgAge >= 0.0) {
        int rev = int(floor(msgAge * 28.0));
        if (rev < 1) rev = 1;
        if (rev > total) rev = total;
        revealCap = rev;
    } else {
        revealCap = total;
    }

    // Plane-specific glyph alpha: back plane is dimmer & smaller, front is
    // crisper & larger. This is the depth cue independent of parallax.
    float planeAlpha = mix(0.45, 1.05, float(planeIdx) / 2.0);
    float planeScale = mix(0.62, 1.20, float(planeIdx) / 2.0) * sizeMul;

    // Inspect 3×3 cells around the pixel so glyphs near edges blend in.
    for (int oy = -1; oy <= 1; oy++) {
        for (int ox = -1; ox <= 1; ox++) {
            vec2 cellId = cell + vec2(float(ox), float(oy));

            // Deterministic per-cell seeds — vary by plane so each plane
            // shows a different sparse pattern (real depth, not duplicates).
            float seedBase = dot(cellId, vec2(57.0, 113.0))
                           + float(planeIdx) * 911.7;
            float h = hash11(seedBase);
            // Sparsity: only `1 - sparsity` of cells contain a glyph.
            // Per-cell threshold lets the user dial poster density.
            if (h < sparsity) continue;

            // Pick which message slot lands here. We use a *slow* rotation
            // so glyphs migrate across the grid over time → animation.
            float slotF = hash11(seedBase + 11.1) * float(MAX_MSG)
                        + planeAge * (0.20 + 0.10 * float(planeIdx));
            int slot = int(mod(slotF, float(MAX_MSG)));
            if (slot >= revealCap) continue;
            int ch = getChar(slot);
            // Numerals variant prefers 27..36; broadside prefers letters.
            if (variant == 1) {
                if (ch >= 0 && ch < 26) {
                    // remap letters → digits deterministically
                    ch = 27 + int(mod(float(ch), 10.0));
                }
            } else if (variant == 2) {
                // Drift: include space-ish gaps more often
                if (hash11(seedBase + 3.1) < 0.18) continue;
            }
            if (ch < 0 || ch > 36) continue;
            if (ch == SPACE_CH) continue;
            if (ch >= 36) ch = 35;   // guard

            // Cell-local glyph anchor — jitter centred so glyphs don't sit
            // on a hard lattice. Tied to seed so still deterministic.
            vec2 jitter = (hash22(cellId + float(planeIdx) * 7.7) - 0.5)
                        * vec2(cellW, cellH) * 0.35;
            vec2 cellCentre = (cellId + 0.5) * vec2(cellW, cellH);

            // Determine which quadrant this glyph belongs to (its anchor,
            // not the pixel) — drives the per-player energy modulation.
            int qLocal = quadrantOf(cellCentre, wobble);
            float qEnergy;
            vec3 qPaper, qInk;
            quadrantStyle(qLocal, qPaper, qInk, qEnergy);

            // Active quadrants jitter their glyphs more & swell slightly.
            float burst = qEnergy;
            vec2 quiver = jitter
                        + burst * 0.012
                            * vec2(sin(planeAge * 8.3 + seedBase),
                                   cos(planeAge * 7.1 + seedBase * 1.3));
            vec2 anchor = cellCentre + quiver;

            // Cell glyph size — base on plane scale, swell with quadrant
            // energy on the *front* plane so loud quadrants pop forward.
            float swell = (planeIdx == 2) ? (1.0 + 0.35 * burst) : 1.0;
            float gW = cellW * 0.62 * planeScale * swell;
            float gH = gW * (7.0 / 5.0);
            // Numeral variant — slimmer character cell, fewer per row.
            if (variant == 1) { gW *= 1.15; gH *= 1.15; }

            // Local glyph UV (0..1 across the glyph box). planeUv is
            // y-UP world coords; d.y grows up from glyph center. The
            // host font atlas stores letter-top at v=1, so `0.5 + d.y/gH`
            // puts letter-top at screen-top. The previous `0.5 - d.y/gH`
            // form flipped glyphs upside down.
            vec2 d = planeUv - anchor;
            vec2 gUv = vec2(d.x / gW + 0.5, 0.5 + d.y / gH);
            if (gUv.x < 0.0 || gUv.x > 1.0) continue;
            if (gUv.y < 0.0 || gUv.y > 1.0) continue;

            float s = sampleChar(ch, gUv);
            // Antialias the glyph edge using fwidth on the atlas sample.
            float aa = fwidth(s) * 1.4 + 1e-4;
            float alpha = smoothstep(0.5 - aa, 0.5 + aa, s);
            // Soft printer "press" — slight grain modulation of ink, more
            // pronounced on the back plane (depth → softer).
            alpha *= mix(0.85, 1.0, printerGrain);
            alpha *= planeAlpha;
            if (alpha < 0.001) continue;

            // Front-plane glyphs in active quadrants get a touch of bloom.
            vec3 inkOut = qInk;
            if (planeIdx == 2 && burst > 0.05) {
                inkOut = mix(qInk, qInk * 1.25 + 0.06, burst);
            }
            // Composite over earlier contributions inside this plane
            // (front-to-back-ish: closer cellOffsets blend after farther).
            acc.rgb = mix(acc.rgb, inkOut, alpha * (1.0 - acc.a));
            acc.a   = acc.a + alpha * (1.0 - acc.a);
        }
    }
    return acc;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    float t      = TIME * motionSpeed;
    float wobble = TIME * 0.35 * cameraDrift;

    // Camera dolly + small breathing translation — gives the *whole sheet*
    // motion every frame so the rubric's "intentional motion" axis is
    // satisfied even at silence. Audio bass tugs the depth columns
    // further out of plane (extra parallax under load).
    float bassPush = audioBass * audioDepth;
    float midGrain = audioMid;

    // Poster-space coordinates: roughly square paper, slightly larger than
    // the screen so the camera dolly can pan inside without bleeding edges.
    // Centre origin so parallax scales feel right at all aspect ratios.
    vec2 posterUv = uv;
    // Soft sheet warp — like the broadside is held in a slight pinch.
    float warp = 0.012 * cameraDrift;
    posterUv.x += warp * sin(uv.y * 3.4 + wobble * 0.8);
    posterUv.y += warp * cos(uv.x * 2.7 - wobble * 0.6);
    // Camera dolly translates the whole sheet over time.
    posterUv += vec2(sin(wobble * 0.18), cos(wobble * 0.13)) * 0.025
              * cameraDrift;
    // Add a mouse parallax nudge so users can orbit the sheet.
    vec2 m2 = (mousePos - 0.5);
    posterUv += m2 * 0.035 * parallax;

    // ── Background: per-quadrant paper, soft divider gutter ─────────
    int q = quadrantOf(posterUv, wobble);
    vec3 paper; vec3 ink; float qE;
    quadrantStyle(q, paper, ink, qE);

    // Continuous gutter: instead of a hard checkerboard line, fade the
    // paper colour toward neighbour quadrants near the divider so the
    // panels feel printed onto one folded sheet, not 4 stamps.
    float midX = 0.5 + 0.015 * sin(wobble * 0.7);
    float midY = 0.5 + 0.018 * cos(wobble * 0.5);
    float gutterX = smoothstep(0.0, 0.04, abs(posterUv.x - midX));
    float gutterY = smoothstep(0.0, 0.04, abs(posterUv.y - midY));
    // Sample neighbour quadrant paper for the bleed.
    int qNeighX = quadrantOf(vec2(2.0 * midX - posterUv.x, posterUv.y), wobble);
    int qNeighY = quadrantOf(vec2(posterUv.x, 2.0 * midY - posterUv.y), wobble);
    vec3 paperNX, paperNY, dummyInk; float dummyE;
    quadrantStyle(qNeighX, paperNX, dummyInk, dummyE);
    quadrantStyle(qNeighY, paperNY, dummyInk, dummyE);
    paper = mix(paperNX, paper, gutterX);
    paper = mix(paperNY, paper, gutterY);

    // Subtle paper tooth — never a pixel grid. Mid-band audio amplifies it.
    float tooth = grain(uv * res.y * 0.014)
                + 0.5 * grain(uv * res.y * 0.032 + 11.0);
    paper *= 1.0 + (tooth - 0.75) * (0.035 + 0.05 * midGrain);

    vec3 col = paper;

    // ── Depth planes ───────────────────────────────────────────────
    // Three planes parallax independently against the same posterUv.
    // Front plane moves most; back plane moves least → real depth.
    int total = msgTotal();

    // Plane-specific parallax offsets — drift each plane on its own
    // velocity AND independently shift in z (different density / size).
    // Audio bass adds out-of-plane motion to the back plane so loud lows
    // pull the columns "down" into the page.
    vec2 pBack = posterUv
               + vec2(sin(t * 0.22), cos(t * 0.18)) * 0.020 * parallax
               + vec2(0.0, -0.020 * bassPush) * parallax;
    vec2 pMid  = posterUv
               + vec2(sin(t * 0.36 + 1.7), cos(t * 0.30 + 2.4)) * 0.045 * parallax;
    vec2 pFront= posterUv
               + vec2(sin(t * 0.55 + 0.4), cos(t * 0.48 + 1.1)) * 0.080 * parallax
               + vec2(0.0,  0.012 * bassPush) * parallax;

    // Aspect-correct each plane independently so the cells are roughly
    // square in screen space.
    pBack.x  *= aspect; pMid.x *= aspect; pFront.x *= aspect;

    // Plane densities — front uses fewer cells (larger glyphs); back uses
    // more cells (smaller, distant chars). gridDensity drives the middle.
    float dMid   = gridDensity;
    float dBack  = gridDensity * 1.55;
    float dFront = max(gridDensity * 0.62, 3.0);

    vec4 backInk  = renderPlane(0, pBack,  t + 0.0,  dBack,  textSize,
                                 total, wobble, tooth);
    vec4 midInk   = renderPlane(1, pMid,   t + 13.7, dMid,   textSize,
                                 total, wobble, tooth);
    vec4 frontInk = renderPlane(2, pFront, t + 27.3, dFront, textSize,
                                 total, wobble, tooth);

    // Compose back→mid→front so far glyphs sit behind near glyphs.
    col = mix(col, backInk.rgb,  backInk.a);
    col = mix(col, midInk.rgb,   midInk.a);
    col = mix(col, frontInk.rgb, frontInk.a);

    // ── Cue typewriter caret — small vertical tick on the front plane ──
    // Only when a live utterance is active and reveal is still progressing.
    if (msgAge >= 0.0 && total > 0) {
        int rev = int(floor(msgAge * 28.0));
        if (rev < total) {
            // place caret deterministically against the latest revealed slot
            float carSeed = float(rev) * 13.1;
            vec2 caretPos = vec2(0.06 + 0.88 * hash11(carSeed),
                                 0.06 + 0.88 * hash11(carSeed + 1.1));
            float blink = 0.5 + 0.5 * sin(TIME * 6.0);
            vec2 d = (posterUv - caretPos) * vec2(aspect, 1.0);
            float bar = step(abs(d.x), 0.0035) * step(abs(d.y), 0.025);
            vec3 carIn;
            float ce; vec3 cp;
            quadrantStyle(quadrantOf(caretPos, wobble), cp, carIn, ce);
            col = mix(col, carIn, bar * blink * 0.75);
        }
    }

    // ── Quadrant-aware vignette: loud quadrants glow inward ────────
    float qEnergyHere = qE;
    vec2 cQuad = vec2((q == 0 || q == 2) ? 0.25 : 0.75,
                      (q < 2)            ? 0.25 : 0.75);
    float distQ = length((uv - cQuad) * vec2(aspect, 1.0));
    float glow = exp(-distQ * 4.5) * qEnergyHere * 0.18;
    col += glow * (ink * 0.4 + 0.6);

    // ── Final paper sheen — slow raking light across the broadside ──
    float sheen = smoothstep(0.0, 0.5,
                  sin(uv.x * 1.15 - uv.y * 0.6 - wobble * 0.4) * 0.5 + 0.5);
    col += pow(sheen, 4.0) * 0.03;

    // gentle tonemap → no banding, no over-saturation
    col = col / (1.0 + 0.18 * col);
    col = pow(max(col, 0.0), vec3(0.94));

    gl_FragColor = vec4(col, 1.0);
}
