/*{
  "DESCRIPTION": "Color Pantone Boxes — a three-tier parallax stack of tilted Pantone-style colour swatches. Each box is a faux-3D extrude (lid + two side-faces darker on the receding edges) sitting on three z-planes that scroll past each other; palette-juggling swaps swatch hues on a slow lottery while player[i].energy hot-spots push their layer forward, jitter a swatch, and saturate its rim. The live cue line types out in slabs, glued to the swatches like Pantone reference codes. Returns LINEAR HDR.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "LABEL": "Caption", "TYPE": "text", "DEFAULT": "PANTONE 18-1664  COLOR OF THE YEAR", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "energyA", "LABEL": "Player 1 (Back)",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB", "LABEL": "Player 2 (Mid)",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC", "LABEL": "Player 3 (Front)", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "activeA", "LABEL": "Player 1 Active",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].active" },
    { "NAME": "activeB", "LABEL": "Player 2 Active",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].active" },
    { "NAME": "activeC", "LABEL": "Player 3 Active",  "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].active" },

    { "NAME": "bassDrive",  "LABEL": "Bass (Tilt)",   "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.bass" },
    { "NAME": "midDrive",   "LABEL": "Mid (Swap)",    "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.mid" },

    { "NAME": "cols",       "LABEL": "Swatch Columns","TYPE": "long",  "DEFAULT": 5, "VALUES": [3,4,5,6,7,8], "LABELS": ["3","4","5","6","7","8"] },
    { "NAME": "rows",       "LABEL": "Swatch Rows",   "TYPE": "long",  "DEFAULT": 6, "VALUES": [3,4,5,6,7,8], "LABELS": ["3","4","5","6","7","8"] },
    { "NAME": "paletteMode","LABEL": "Palette Mode",  "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Pantone","Editorial","Acid","Mono+Pop"] },
    { "NAME": "motionSpeed","LABEL": "Motion Speed",  "TYPE": "float", "DEFAULT": 0.30, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "tiltAmount", "LABEL": "Tilt Amount",   "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "audioDepth", "LABEL": "Audio Depth",   "TYPE": "float", "DEFAULT": 0.60, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "swapRate",   "LABEL": "Palette Swap",  "TYPE": "float", "DEFAULT": 0.35, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "labelScale", "LABEL": "Label Scale",   "TYPE": "float", "DEFAULT": 1.0, "MIN": 0.5, "MAX": 2.0 },
    { "NAME": "kerning",    "LABEL": "Kerning",       "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.55, "MAX": 1.4 },
    { "NAME": "paperColor", "LABEL": "Paper",         "TYPE": "color", "DEFAULT": [0.93, 0.92, 0.88, 1.0] },
    { "NAME": "inkColor",   "LABEL": "Ink",           "TYPE": "color", "DEFAULT": [0.05, 0.05, 0.07, 1.0] }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  COLOR PANTONE BOXES · faux-3D parallax stack of tilted colour swatches
//
//  Three z-planes (back / mid / front), each carrying a sparse grid of
//  Pantone-style colour boxes. Each box is rendered with a fake-3D pop:
//  the box quad is rotated per-cell on a per-cell tilt vector, then the
//  receding two edges are darkened to read as side-faces (no raymarch,
//  pure 2D-faking-3D). Palette is procedurally generated per palette mode
//  and *swaps* on a slow lottery — boxes randomly trade hues across the
//  canvas so the composition never settles. Each plane scrolls at its own
//  velocity; player[i].energy pushes its plane forward, jitters a chosen
//  swatch, and saturates the rim. Live cue text types onto the front
//  plane glued near hot swatches like Pantone reference codes.
// ════════════════════════════════════════════════════════════════════════

#define MAX_WALK   48
#define SPACE_CH   26
#define TAU        6.28318530718

// ─── Font atlas ─────────────────────────────────────────────────────
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

// ─── Hash + procedural palette ────────────────────────────────────────
float h11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  h21(float n) { return vec2(h11(n), h11(n + 17.31)); }
float h12(vec2 p)  { return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453); }

vec3 spectrum(float t) {
    return 0.5 + 0.5 * cos(TAU * (t + vec3(0.00, 0.33, 0.67)));
}

// Per palette-mode swatch generator. seed in [0,1), returns LINEAR RGB.
vec3 swatchColor(float seed, int mode) {
    if (mode == 0) {
        // Pantone editorial — warm coral, deep teal, salmon, plum, mustard,
        // sky, hot pink. Bands the seed into 7 punchy spots.
        float b = floor(seed * 7.0);
        vec3 c;
        if      (b < 1.0) c = vec3(0.96, 0.42, 0.38);   // coral
        else if (b < 2.0) c = vec3(0.04, 0.55, 0.55);   // teal
        else if (b < 3.0) c = vec3(0.98, 0.66, 0.49);   // salmon
        else if (b < 4.0) c = vec3(0.46, 0.16, 0.42);   // plum
        else if (b < 5.0) c = vec3(0.97, 0.78, 0.30);   // mustard
        else if (b < 6.0) c = vec3(0.40, 0.78, 0.92);   // sky
        else              c = vec3(0.95, 0.30, 0.62);   // hot pink
        return c;
    } else if (mode == 1) {
        // Editorial — desaturated jewel tones with one neutral.
        float h = fract(seed * 1.618 + 0.07);
        vec3 base = spectrum(h);
        base = mix(base, vec3(0.30, 0.28, 0.32), 0.35);
        if (fract(seed * 7.13) < 0.18) base = vec3(0.90, 0.86, 0.78); // bone
        return base;
    } else if (mode == 2) {
        // Acid — neon greens, electric magentas, cyans, lemon.
        float b = floor(seed * 6.0);
        if      (b < 1.0) return vec3(0.62, 1.00, 0.18);
        else if (b < 2.0) return vec3(1.00, 0.13, 0.78);
        else if (b < 3.0) return vec3(0.10, 0.92, 0.95);
        else if (b < 4.0) return vec3(1.00, 0.93, 0.10);
        else if (b < 5.0) return vec3(0.30, 0.20, 0.95);
        else              return vec3(1.00, 0.45, 0.06);
    }
    // Mono + Pop — mostly graphite/cream, one of every 5 boxes is a hot pop.
    if (fract(seed * 11.7) < 0.22) {
        // pop colors
        float b = floor(seed * 4.0);
        if      (b < 1.0) return vec3(0.97, 0.20, 0.30);
        else if (b < 2.0) return vec3(0.20, 0.85, 0.60);
        else if (b < 3.0) return vec3(1.00, 0.72, 0.10);
        else              return vec3(0.20, 0.45, 1.00);
    }
    float g = mix(0.12, 0.92, fract(seed * 3.21));
    return vec3(g, g * 0.99, g * 0.96);
}

// ─── Tilted box SDF + faux-3D side-faces ──────────────────────────────
// Returns negative inside, positive outside; also outputs a face id in
// `face` (0=top, 1=right side, 2=bottom side) and the inside-coordinate
// `local` (for label glyph + edge AA).
float tiltedBoxSDF(
    vec2 p, vec2 c, vec2 hb, float ang,
    out vec2 local, out int face, out vec2 extrude)
{
    // Rotate point into the box's local frame.
    float ca = cos(ang), sa = sin(ang);
    vec2 q = p - c;
    vec2 r = vec2(ca * q.x + sa * q.y, -sa * q.x + ca * q.y);
    local = r;
    // Box top face SDF (axis-aligned in local space).
    vec2 d = abs(r) - hb;
    float dTop = max(d.x, d.y);

    // Side-face extrude — short skewed quads attached to the bottom and
    // right edges that read as the box's "depth". The extrude vector is
    // a tiny push in screen-down + screen-right so all boxes share a
    // consistent vanishing direction.
    extrude = vec2(hb.x * 0.18, -hb.y * 0.18);

    // SDF for the right side-face (a parallelogram).
    // Compose: it's the rectangle whose top edge is the box's right edge
    // (in local space at x=+hb.x, y in [-hb, +hb]) extruded by
    // `extrude` rotated back into world. Cheap approximation: SDF of the
    // axis-aligned rect from (hb.x, -hb.y) to (hb.x + |ex|, +hb.y)
    // in local space, sheared by extrude.y.
    float ex = length(extrude);
    vec2 r2 = r;
    r2.y -= (r2.x - hb.x) * (extrude.y / max(extrude.x, 1e-3));
    vec2 dR = abs(r2 - vec2(hb.x + ex * 0.5, 0.0)) - vec2(ex * 0.5, hb.y);
    float dRight = max(dR.x, dR.y);

    // Bottom side-face (similar, attached to y = -hb.y, extruded down).
    vec2 r3 = r;
    r3.x -= (-r3.y - hb.y) * (extrude.x / max(-extrude.y, 1e-3));
    vec2 dB = abs(r3 - vec2(0.0, -hb.y - ex * 0.5)) - vec2(hb.x, ex * 0.5);
    float dBottom = max(dB.x, dB.y);

    face = 0;
    float dMin = dTop;
    if (dRight < dMin)  { dMin = dRight;  face = 1; }
    if (dBottom < dMin) { dMin = dBottom; face = 2; }
    return dMin;
}

// ─── Render a single z-plane of swatches ─────────────────────────────
// Returns the layer color + alpha for pixel `p`. `zPlane` selects which
// plane (0=back, 1=mid, 2=front). Player energy/active drives that
// plane's behavior. `outHotPos` records the on-canvas position of the
// hottest swatch (used by the caller to glue the label).
void renderLayer(
    int zPlane, vec2 p, float aspect, int gridX, int gridY,
    float energy, float activeF, int paletteM, float speed,
    float tilt, float swap, float bassA,
    out vec3 outCol, out float outAlpha, out vec2 outHotPos,
    out float outHotEnergy)
{
    outCol = vec3(0.0);
    outAlpha = 0.0;
    outHotPos = vec2(0.0);
    outHotEnergy = 0.0;

    float planeF = float(zPlane);
    // Per-plane scale + drift + parallax — back plane smaller and slower,
    // front plane larger and faster, scrolls to the LEFT so they pass each
    // other crisply on screen.
    float scale  = mix(0.78, 1.18, planeF * 0.5);
    float driftX = -TIME * speed * (0.05 + 0.10 * planeF);
    float driftY = -TIME * speed * 0.015 * (1.0 + planeF * 0.5);
    // Energy push: hot plane pops forward (= larger boxes, less margin).
    float pop = 1.0 + 0.18 * energy + 0.12 * activeF;
    scale *= pop;

    // Per-plane palette phase — palette mode + slow swap window per cell.
    // The "swap" mechanic: every `swapPeriod` seconds each cell rolls a
    // new hue from the palette. We blend between the previous and next
    // hue so swaps feel like ink-poured transitions, not pops.
    float swapPeriod = mix(7.5, 2.5, swap);     // larger swap = faster
    float swapNow    = TIME / max(swapPeriod, 0.4);
    float swapFloor  = floor(swapNow);
    float swapMix    = smoothstep(0.55, 1.0, fract(swapNow));

    // Cell metrics in NDC-ish space. Canvas spans roughly x in
    // [-aspect/2, +aspect/2], y in [-0.5, +0.5].
    float canvasW = aspect * 0.94;
    float canvasH = 0.94;
    float cellW   = (canvasW / float(gridX)) * scale;
    float cellH   = (canvasH / float(gridY)) * scale;
    // Inset (margin between boxes) — also breathes with energy.
    float pad     = mix(0.18, 0.10, energy);
    vec2 halfBox  = vec2(cellW * (0.5 - pad * 0.5), cellH * (0.5 - pad * 0.5));

    // Iterate cells. Each plane scrolls by (driftX, driftY) so we sample
    // a ring of cells around the visible area. To keep loop bounds tight,
    // we transform p back into "cell-space" and only test the 9-cell
    // neighbourhood around the pixel.
    vec2 pShift = p - vec2(driftX, driftY);
    // Cell index space.
    float cxF = (pShift.x + canvasW * 0.5) / cellW;
    float cyF = (pShift.y + canvasH * 0.5) / cellH;
    int   cx  = int(floor(cxF));
    int   cy  = int(floor(cyF));

    // Hot swatch: the swatch closest to a per-plane wandering "spot" that
    // is pinned to the active player's energy. When active=1 and energy
    // peaks, the spot anchors on that cell.
    vec2 hotAnchor = vec2(
        sin(TIME * 0.27 + planeF * 1.7) * canvasW * 0.40,
        cos(TIME * 0.21 + planeF * 0.9) * canvasH * 0.40
    );
    hotAnchor *= 1.0 - 0.4 * activeF; // when active, center the hot spot

    // 9-neighbour cell tap — covers any cell that could overlap p.
    for (int oy = -1; oy <= 1; oy++) {
        for (int ox = -1; ox <= 1; ox++) {
            int ix = cx + ox;
            int iy = cy + oy;
            // Wrap so the scrolling layer is "infinite".
            int wx = ix - int(floor(float(ix) / float(gridX))) * gridX;
            int wy = iy - int(floor(float(iy) / float(gridY))) * gridY;
            // Per-cell deterministic seed (uses wrapped coords + plane).
            float sCell = h12(vec2(float(wx) + planeF * 31.7,
                                   float(wy) + planeF * 17.3));
            // Palette seed swaps over time — interpolate prev/next hue.
            float seedPrev = fract(sCell + swapFloor * 0.197);
            float seedNext = fract(sCell + (swapFloor + 1.0) * 0.197);
            vec3 colPrev = swatchColor(seedPrev, paletteM);
            vec3 colNext = swatchColor(seedNext, paletteM);
            vec3 boxCol  = mix(colPrev, colNext, swapMix);

            // Cell center in unshifted world coords.
            vec2 cellC = vec2(
                -canvasW * 0.5 + (float(ix) + 0.5) * cellW + driftX,
                -canvasH * 0.5 + (float(iy) + 0.5) * cellH + driftY
            );

            // Per-cell tilt: every cell gets its own rotation angle that
            // breathes with TIME + cell seed + bass. Tilt range scales
            // with `tilt`. Bass adds a synchronized shudder.
            float tiltAng =
                  (sCell - 0.5) * 0.35 * tilt
                + sin(TIME * 0.6 + sCell * 11.1) * 0.10 * tilt
                + bassA * 0.18 * (sCell - 0.5);

            // Energy jitter: in the hot plane, the swatch nearest the
            // hot spot also shifts a few pixels.
            vec2 toHot = hotAnchor - cellC;
            float dHot = length(toHot);
            float hotW = exp(-dHot * dHot * 6.0) * (0.6 + 0.4 * energy);
            cellC += vec2(sin(TIME * 1.3 + sCell * 9.0),
                          cos(TIME * 1.1 + sCell * 7.3)) * 0.012 * hotW * energy;

            // Track hot swatch for label gluing.
            if (hotW > outHotEnergy) {
                outHotEnergy = hotW;
                outHotPos = cellC;
            }

            // Tilted box SDF.
            vec2 boxLocal;
            int  face;
            vec2 extrude;
            float dBox = tiltedBoxSDF(p, cellC, halfBox, tiltAng,
                                      boxLocal, face, extrude);
            if (dBox > halfBox.x * 0.8) continue; // fast-out

            float fw   = fwidth(dBox);
            float fill = 1.0 - smoothstep(-fw, fw, dBox);
            if (fill < 0.001) continue;

            // Face shading: top = full hue, sides darker for fake-3D.
            vec3 faceCol = boxCol;
            if (face == 1) faceCol *= 0.62;       // right side
            else if (face == 2) faceCol *= 0.48;  // bottom side (deeper shadow)

            // Rim accent on the hot swatch — saturate edges by player energy.
            float rimT = 1.0 - smoothstep(-fw - 0.0015, -fw, dBox);
            faceCol += rimT * energy * 0.35 * (vec3(1.0) - boxCol);

            // Tiny inner highlight on the top face — gallery sheen.
            if (face == 0) {
                float sheen = smoothstep(0.0, 0.6,
                    boxLocal.x / halfBox.x + boxLocal.y / halfBox.y);
                faceCol += sheen * 0.05 * vec3(1.0, 0.98, 0.95);
            }

            // Composite over the layer's accumulator (front-most cell in
            // the 9-neighbourhood wins because of natural z within layer).
            outCol = mix(outCol, faceCol, fill);
            outAlpha = max(outAlpha, fill);
        }
    }
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y =  uv.y - 0.5;

    int   gridX = int(cols);
    int   gridY = int(rows);
    if (gridX < 2) gridX = 2;
    if (gridY < 2) gridY = 2;
    int   paletteM = int(paletteMode);
    if (paletteM < 0) paletteM = 0;
    if (paletteM > 3) paletteM = 3;

    float speed   = max(motionSpeed, 0.0);
    float tilt    = max(tiltAmount, 0.0);
    float swap    = max(swapRate, 0.0)
                  + 0.4 * clamp(midDrive, 0.0, 1.0);
    float bassA   = clamp(bassDrive * audioDepth, 0.0, 1.0);

    // ── Paper backdrop (warm, slightly vignetted, faint tooth) ──
    vec3 paper = paperColor.rgb;
    float vig  = 1.0 - 0.20 * dot(p, p);
    paper *= vig;
    // subtle paper tooth — never a pixel grid, fbm-ish via 2 sin layers
    float tooth = 0.5 + 0.5 * sin(uv.x * res.y * 0.045 + uv.y * res.y * 0.031);
    tooth *= 0.5 + 0.5 * sin(uv.y * res.y * 0.071 - uv.x * res.y * 0.057);
    paper *= 1.0 + (tooth - 0.5) * 0.04;

    vec3 col = paper;

    // ── Three z-planes, back→front ──
    float energies[3];
    energies[0] = clamp(energyA, 0.0, 1.0);
    energies[1] = clamp(energyB, 0.0, 1.0);
    energies[2] = clamp(energyC, 0.0, 1.0);
    float actives[3];
    actives[0] = clamp(activeA, 0.0, 1.0);
    actives[1] = clamp(activeB, 0.0, 1.0);
    actives[2] = clamp(activeC, 0.0, 1.0);

    vec2  bestHotPos = vec2(0.0);
    float bestHotE   = 0.0;
    int   bestHotZ   = 2;

    for (int z = 0; z < 3; z++) {
        vec3 lcol;
        float lalpha;
        vec2 hotPos;
        float hotE;
        renderLayer(
            z, p, aspect, gridX, gridY,
            energies[z], actives[z], paletteM,
            speed, tilt, swap, bassA,
            lcol, lalpha, hotPos, hotE);

        // Atmospheric depth — back plane reads slightly washed; front
        // plane is full saturation.
        float depthT = float(z) / 2.0;
        vec3 hazed = mix(paper * 0.95, lcol, mix(0.55, 1.0, depthT));
        col = mix(col, hazed, lalpha);

        if (hotE * (0.7 + 0.3 * depthT) > bestHotE) {
            bestHotE  = hotE * (0.7 + 0.3 * depthT);
            bestHotPos = hotPos;
            bestHotZ  = z;
        }
    }

    // ── Live caption — types out near the hot swatch on the front plane ──
    int   total = charCount();
    bool  liveUtter = msgAge >= 0.0;
    if (total > 0) {
        int visibleN = total;
        float capFade = 1.0;
        if (liveUtter) {
            visibleN = int(floor(msgAge * 28.0));
            if (visibleN > total) visibleN = total;
            if (visibleN < 0) visibleN = 0;
            capFade = smoothstep(0.0, 0.3, msgAge);
        }
        // Label sits to the lower-right of the hot swatch (Pantone-style
        // code placement).
        float scaleF  = clamp(labelScale, 0.5, 2.0);
        float charH   = 0.018 * scaleF;
        float charW   = charH * (5.0 / 7.0);
        float kern    = charW * kerning;
        int   charsPerRow = 14;
        int   rowsNeeded  = (visibleN + charsPerRow - 1) / charsPerRow;
        if (rowsNeeded < 1) rowsNeeded = 1;
        float lineH = charH * 1.30;

        vec2 anchor = bestHotPos + vec2(0.04, -0.06);
        vec2 lp = p - anchor;
        float lx = lp.x;
        float ly = -lp.y;
        if (lx >= 0.0 && ly >= 0.0
            && lx < float(charsPerRow) * kern
            && ly < float(rowsNeeded) * lineH) {
            int targetCol = int(floor(lx / kern));
            int targetRow = int(floor(ly / lineH));
            int cursorR = 0;
            int cursorC = 0;
            int outCh = -1;
            for (int i = 0; i < MAX_WALK; i++) {
                if (i >= visibleN) break;
                if (cursorR > targetRow) break;
                int ch = getChar(i);
                if (ch == SPACE_CH) {
                    int wlen = 0;
                    for (int j = 1; j < MAX_WALK; j++) {
                        int jj = i + j;
                        if (jj >= visibleN) break;
                        int chj = getChar(jj);
                        if (chj == SPACE_CH || chj < 0 || chj > 36) break;
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
            if (outCh >= 0 && outCh <= 35 && outCh != SPACE_CH) {
                float gx = (lx - float(targetCol) * kern) / charW;
                float gy = 1.0 - (ly - float(targetRow) * lineH) / charH;
                float s = sampleChar(outCh, vec2(gx, gy));
                s = smoothstep(0.18, 0.55, s);
                float w = s * capFade * (0.4 + 0.6 * bestHotE);
                col = mix(col, inkColor.rgb, w);
            }
        }
        // Caret blink at the current write head (only in live mode).
        if (liveUtter && visibleN < total) {
            int caretRow = visibleN / charsPerRow;
            int caretCol = visibleN - caretRow * charsPerRow;
            vec2 caretPos = anchor
                + vec2(float(caretCol) * kern + charW * 0.45,
                       -float(caretRow) * lineH - charH * 0.5);
            float cdx = abs(p.x - caretPos.x) - charW * 0.06;
            float cdy = abs(p.y - caretPos.y) - charH * 0.45;
            float cd  = max(cdx, cdy);
            float cfw = fwidth(cd);
            float cm  = (1.0 - smoothstep(-cfw, cfw, cd))
                      * (0.5 + 0.5 * sin(TIME * 6.0));
            col = mix(col, inkColor.rgb, cm * capFade);
        }
    }

    // ── Tone — subtle bloom on the hot swatch only, gallery sheen ──
    float L = dot(col, vec3(0.299, 0.587, 0.114));
    col += 0.08 * smoothstep(0.65, 1.05, L) * col;
    col = col / (1.0 + 0.55 * col);
    col = pow(max(col, 0.0), vec3(0.92));

    gl_FragColor = vec4(col, 1.0);
}
