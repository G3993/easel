/*{
  "DESCRIPTION": "Connect-Numbers — a floating data-graph in pseudo-3D depth. A Voronoi-relaxed swarm of nodes drifts across three parallax planes, each plane wired by smooth-min capsule edges into a Delaunay-ish web. Every node wears a tiny numeric label drawn from the font atlas (27..36 = 0..9) — the labels twitch between values, ticking faster on whichever player[i] is hot. A caption from cue.latest typewriters along the bottom as a telemetry line. Inspired by connect-the-dots planners and editorial network diagrams, but everything is abstract: nodes are not a face, edges are not a graph, numbers are not data — they are the *feeling* of a network listening.",
  "CREDIT": "ShaderClaw — A-List drop · connectnumbers",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "LABEL": "Telemetry", "TYPE": "text", "DEFAULT": "CONNECT THE NUMBERS", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "energyA",  "LABEL": "Player 1 Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",  "LABEL": "Player 2 Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",  "LABEL": "Player 3 Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "activeA",  "LABEL": "Player 1 Active", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].active" },
    { "NAME": "activeB",  "LABEL": "Player 2 Active", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].active" },
    { "NAME": "bassDrive","LABEL": "Bass Drive",      "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.bass" },

    { "NAME": "nodeCount",  "LABEL": "Nodes",         "TYPE": "long",  "DEFAULT": 24, "VALUES": [12,16,20,24,28,32,40], "LABELS": ["12","16","20","24","28","32","40"] },
    { "NAME": "lineWidth",  "LABEL": "Line Thickness","TYPE": "float", "DEFAULT": 0.0018, "MIN": 0.0006, "MAX": 0.006 },
    { "NAME": "nodeRadius", "LABEL": "Node Radius",   "TYPE": "float", "DEFAULT": 0.014,  "MIN": 0.005,  "MAX": 0.030 },
    { "NAME": "motionSpeed","LABEL": "Motion Speed",  "TYPE": "float", "DEFAULT": 0.55,  "MIN": 0.0,    "MAX": 2.0 },
    { "NAME": "audioDepth", "LABEL": "Audio Depth",   "TYPE": "float", "DEFAULT": 0.7,   "MIN": 0.0,    "MAX": 2.0 },
    { "NAME": "depthAmount","LABEL": "Parallax Depth","TYPE": "float", "DEFAULT": 0.65,  "MIN": 0.0,    "MAX": 1.6 },
    { "NAME": "labelScale", "LABEL": "Number Scale",  "TYPE": "float", "DEFAULT": 1.0,   "MIN": 0.4,    "MAX": 2.0 },

    { "NAME": "palette",    "LABEL": "Palette", "TYPE": "long", "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Cream/Ink","Cyan/Magenta","Forest","Mono"] },
    { "NAME": "paperColor", "LABEL": "Paper", "TYPE": "color", "DEFAULT": [0.93, 0.91, 0.86, 1.0] },
    { "NAME": "inkColor",   "LABEL": "Ink",   "TYPE": "color", "DEFAULT": [0.08, 0.07, 0.09, 1.0] }
  ]
}*/

// =====================================================================
//  Connect-Numbers · floating data-graph · numeric nodes + edges
//  Three parallax planes, each a Voronoi-relaxed swarm. Edges are
//  k-nearest connections drawn as smooth-min capsules with fwidth AA.
//  Numbers are drawn from atlas indices 27..36 (= 0..9). Each player
//  channel owns one plane: its nodes pulse and its numbers tick faster
//  on energy. cue.latest types into the bottom telemetry line.
// =====================================================================

#define MAX_NODES   40
#define PLANES       3
#define MAX_WALK    48
#define SPACE_CH    26
#define DIGIT0      27
#define TAU         6.28318530718

// ─── Font atlas (37 cells: A-Z, space, 0-9 at 27-36) ────────────────
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

// ─── Hash / smin / capsule / palette ────────────────────────────────
float h11(float n)   { return fract(sin(n * 127.1) * 43758.5453); }
vec2  h21(float n)   { return vec2(h11(n), h11(n + 17.31)); }
float h21f(vec2 p)   { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

float smin(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

// Distance from point p to segment a-b.
float segDist(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a, ba = b - a;
    float t = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-6), 0.0, 1.0);
    return length(pa - ba * t);
}

// Palette: 4 hand-tuned schemes returning (paper, ink, accent1, accent2).
void getPalette(int idx, out vec3 paper, out vec3 ink, out vec3 acc1, out vec3 acc2) {
    if (idx == 1) {       // Cyan / Magenta — electric data
        paper = vec3(0.06, 0.07, 0.10);
        ink   = vec3(0.92, 0.95, 1.00);
        acc1  = vec3(0.20, 0.85, 0.95);
        acc2  = vec3(0.95, 0.32, 0.78);
    } else if (idx == 2) {// Forest — deep green editorial
        paper = vec3(0.94, 0.93, 0.88);
        ink   = vec3(0.08, 0.14, 0.10);
        acc1  = vec3(0.18, 0.52, 0.30);
        acc2  = vec3(0.82, 0.42, 0.18);
    } else if (idx == 3) {// Mono — paper / charcoal only
        paper = vec3(0.95, 0.94, 0.91);
        ink   = vec3(0.10, 0.09, 0.10);
        acc1  = vec3(0.30, 0.30, 0.32);
        acc2  = vec3(0.55, 0.54, 0.56);
    } else {              // 0 — Cream / Ink default (paper-planner)
        paper = paperColor.rgb;
        ink   = inkColor.rgb;
        acc1  = vec3(0.82, 0.20, 0.18);   // editorial red
        acc2  = vec3(0.18, 0.32, 0.72);   // editorial blue
    }
}

// Per-plane / per-node position. Voronoi-relaxed initial layout
// (jittered grid) + slow per-node orbit so the swarm breathes. Returns
// pos in aspect-corrected space, and writes out the node's "digit value"
// so labels are deterministic per node (drift slowly with TIME + energy).
vec2 nodePos(int planeIdx, int n, int total, float aspect, float t,
             float planeOffset, float planeScale) {
    // Jittered grid layout so nodes never overlap and the layout reads
    // as a "graph" not a pile. Grid sized from per-plane node share.
    int gridX = int(ceil(sqrt(float(total) * max(aspect, 0.6))));
    if (gridX < 2) gridX = 2;
    int gridY = (total + gridX - 1) / gridX;
    int gx = n - (n / gridX) * gridX;
    int gy = n / gridX;

    float cellW = (1.85 * aspect) / float(gridX);
    float cellH = 1.60 / float(gridY);

    // Per-node jitter seed includes plane index — each plane is a
    // different Voronoi relaxation of the swarm.
    float seed = float(n) * 13.371 + float(planeIdx) * 91.7;
    vec2  j    = h21(seed) - 0.5;          // [-0.5, 0.5]

    vec2 base;
    base.x = (float(gx) + 0.5 + j.x * 0.85) * cellW - 0.5 * cellW * float(gridX);
    base.y = (float(gy) + 0.5 + j.y * 0.85) * cellH - 0.5 * cellH * float(gridY);

    // Slow drift orbit, frequency varies per node.
    float a   = t * (0.35 + 0.55 * h11(seed + 3.1)) + h11(seed + 7.7) * TAU;
    float orb = 0.045 + 0.055 * h11(seed + 1.7);
    base += vec2(cos(a), sin(a * 1.13)) * orb;

    // Parallax: nearer planes are pushed outward and scaled slightly.
    base = (base + planeOffset * 0.16 * vec2(cos(seed), sin(seed))) * planeScale;
    return base;
}

// Per-node 2-digit value 0..99. Increments faster when the plane's
// player is hot. Stable enough to read.
int nodeDigits(int planeIdx, int n, float t, float energy) {
    float seed = float(n) * 5.71 + float(planeIdx) * 19.3;
    float rate = 0.6 + 6.0 * energy * energy;            // ticks/sec at peak
    float tick = floor(t * rate + h11(seed + 0.5) * 17.0);
    float v    = fract(sin(tick * 12.9898 + seed) * 43758.5453) * 100.0;
    return int(clamp(floor(v), 0.0, 99.0));
}

// Render a single character into the canvas given a top-left anchor and
// glyph cell size. Returns coverage 0..1 (fwidth-AA via smoothstep on the
// raw atlas alpha). Writes the localised cell uv check itself.
float drawChar(vec2 p, vec2 anchor, vec2 cell, int ch) {
    vec2 lp = p - anchor;
    if (lp.x < 0.0 || lp.x > cell.x) return 0.0;
    if (lp.y < 0.0 || lp.y > cell.y) return 0.0;
    // lp.y grows y-up (p.y is y-up world, anchor at bottom of cell). The
    // font atlas stores letter-top at v=1 (matches the host's FontAtlas
    // upload — see render_isf.py build_font_atlas + ShaderSource), so
    // mapping lp.y → cuv.y directly puts letter-top at screen-top. The
    // previous `1.0 -` here flipped the result and rendered glyphs upside
    // down.
    vec2 cuv = vec2(lp.x / cell.x, lp.y / cell.y);
    float s = sampleChar(ch, cuv);
    return smoothstep(0.30, 0.62, s);
}

void main() {
    vec2 res    = RENDERSIZE;
    vec2 uv     = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    // Aspect-corrected centred coordinate.
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    // Palette + time.
    vec3 paper, ink, acc1, acc2;
    getPalette(int(palette), paper, ink, acc1, acc2);

    float t   = TIME * max(motionSpeed, 0.0);
    float aud = clamp(audioDepth, 0.0, 2.0);
    float bass = clamp(bassDrive, 0.0, 1.0);

    int totalNodes = int(nodeCount);
    if (totalNodes > MAX_NODES) totalNodes = MAX_NODES;
    if (totalNodes <  6)        totalNodes = 6;
    // Each plane gets a third of the nodes (rounded).
    int planeN = (totalNodes + PLANES - 1) / PLANES;
    if (planeN > MAX_NODES) planeN = MAX_NODES;

    // ── Background — warm paper with a faint vignette and gentle
    //    chromatic warmth toward the upper-right. Never flat.
    vec3 col = paper;
    float vig = smoothstep(1.05, 0.20, length(uv - 0.5) * 1.6);
    col *= mix(0.86, 1.03, vig);
    // Subtle paper grain (deterministic, low amplitude).
    float grain = h21f(floor(gl_FragCoord.xy * 0.5)) - 0.5;
    col += grain * 0.012;

    // Per-plane energy/active arrays (3 planes, 3 channels). The 3rd
    // plane uses the smoothed mean so an unbound 3rd player still moves.
    float E[3];
    float A[3];
    E[0] = clamp(energyA, 0.0, 1.0);
    E[1] = clamp(energyB, 0.0, 1.0);
    E[2] = clamp(energyC, 0.0, 1.0);
    A[0] = clamp(activeA, 0.0, 1.0);
    A[1] = clamp(activeB, 0.0, 1.0);
    A[2] = 0.5 * (A[0] + A[1]);

    // Rest energy so silent shaders still breathe — but plane scale is
    // dominated by the bound channel when it goes hot.
    float rest = 0.12;

    // SDF accumulator for combined edge web with proper fwidth AA.
    float edgeSdf = 1e6;
    vec3  edgeCol = vec3(0.0);

    // Node disc accumulator.
    float nodeSdf = 1e6;
    vec3  nodeTint = ink;

    // Label accumulator — drawn after nodes so digits sit on top.
    float labelMask = 0.0;
    vec3  labelCol  = ink;

    // ── Three parallax planes, back → front ─────────────────────────
    for (int pl = 0; pl < PLANES; pl++) {
        float fpl = float(pl);
        // Plane depth in [0..1]: 0 = far, 1 = near. depthAmount stretches.
        float depth = fpl / float(PLANES - 1);
        float planeScale = mix(0.78, 1.10, depth) * (1.0 + 0.06 * depthAmount);
        float planeOffset = (depth - 0.5) * 2.0 * depthAmount;

        float e = mix(rest, 1.0, E[pl]);
        float aActive = mix(0.5, 1.0, A[pl]);
        // Per-plane tint: alternate accent colours; far plane fades to ink.
        vec3 tint = (pl == 0) ? mix(ink, acc2, 0.40 + 0.50 * E[pl])
                  : (pl == 1) ? mix(ink, acc1, 0.55 + 0.40 * E[pl])
                              : mix(ink, vec3(0.5)*ink + 0.5*acc1, 0.70 * E[pl]);

        // Node positions for this plane — cache up to MAX_NODES.
        vec2 pos[MAX_NODES];
        for (int i = 0; i < MAX_NODES; i++) {
            if (i >= planeN) break;
            pos[i] = nodePos(pl, i, planeN, aspect, t, planeOffset, planeScale);
        }

        // ── EDGES — k-nearest (k=2) per node, deterministically chosen.
        //    We collapse edges with smooth-min so the web reads as one
        //    sheet of capsules with anti-aliased thickness.
        float planeEdge = 1e6;
        // Edge thickness scales with depth (foreground a touch thicker)
        // and lifts on bass for a kinetic feel.
        float lw = lineWidth * mix(0.65, 1.15, depth) * (1.0 + 0.6 * bass * aud);

        for (int i = 0; i < MAX_NODES; i++) {
            if (i >= planeN) break;
            // Find two nearest neighbours j > i (avoids double-segment).
            float d1 = 1e6, d2 = 1e6;
            int   k1 = -1, k2 = -1;
            for (int j = 0; j < MAX_NODES; j++) {
                if (j >= planeN) break;
                if (j == i) continue;
                float dd = length(pos[i] - pos[j]);
                if (dd < d1) { d2 = d1; k2 = k1; d1 = dd; k1 = j; }
                else if (dd < d2) { d2 = dd; k2 = j; }
            }
            // Only draw j>i to avoid duplicates.
            if (k1 > i) {
                float ds = segDist(p, pos[i], pos[k1]) - lw;
                planeEdge = smin(planeEdge, ds, lw * 1.4);
            }
            if (k2 > i) {
                float ds = segDist(p, pos[i], pos[k2]) - lw;
                planeEdge = smin(planeEdge, ds, lw * 1.4);
            }
        }

        // Compose this plane's edges into the global SDF with parallax
        // weighting — closer planes win when overlapping.
        float zw = mix(0.85, 1.0, depth);
        float planeEdgeW = planeEdge - (zw - 1.0) * 0.001;
        if (planeEdgeW < edgeSdf) {
            edgeSdf = planeEdgeW;
            edgeCol = mix(tint, ink, 0.35) * mix(0.9, 1.15, depth) * aActive;
        }

        // ── NODES — discs with fwidth AA, plus a soft outer halo on the
        //    most energetic plane.
        for (int i = 0; i < MAX_NODES; i++) {
            if (i >= planeN) break;
            float seed = float(i) * 3.13 + fpl * 11.0;
            float rad  = nodeRadius * mix(0.7, 1.25, depth)
                       * (1.0 + 0.55 * e * (0.5 + 0.5 * sin(t * 2.1 + seed)))
                       * (1.0 + 0.40 * bass * aud);
            float dN   = length(p - pos[i]) - rad;

            if (dN < nodeSdf) {
                nodeSdf  = dN;
                nodeTint = mix(ink, tint, 0.55 + 0.30 * e);
            }

            // Halo for hot nodes — additive bloom-y glow, capped.
            float halo = exp(-pow(max(length(p - pos[i]) - rad, 0.0) / max(rad * 1.6, 1e-3), 2.0));
            col += halo * tint * 0.18 * E[pl] * aActive;

            // ── LABEL — 2-digit number at node, slightly above-right.
            //   Scale tracks node radius + the labelScale control.
            int  val = nodeDigits(pl, i, t, E[pl]);
            int  tens = val / 10;
            int  ones = val - tens * 10;
            // Cell size: smaller on far plane (parallax-readable).
            float gh = nodeRadius * 1.55 * labelScale * mix(0.75, 1.18, depth);
            float gw = gh * (5.0 / 7.0);
            // Anchor: above-right of node, like the planner reference.
            vec2 anchor = pos[i] + vec2(rad * 1.05, rad * 0.55);
            // Two-digit cell stride
            float ds1 = drawChar(p, anchor,                     vec2(gw, gh), DIGIT0 + tens);
            float ds2 = drawChar(p, anchor + vec2(gw * 1.02,0), vec2(gw, gh), DIGIT0 + ones);
            float ds  = max(ds1, ds2);
            // Hot players: numbers shine in accent; cold: ink.
            vec3  lcol = mix(ink, tint, 0.35 + 0.55 * E[pl]);
            float w    = ds * aActive * mix(0.55, 1.0, depth);
            if (w > labelMask) { labelMask = w; labelCol = lcol; }
        }
    }

    // ── COMPOSITE — paper ← edges ← nodes ← labels ───────────────────
    // Edges: fwidth-AA capsule shell.
    float ew   = fwidth(edgeSdf);
    float eFill = 1.0 - smoothstep(-ew, ew, edgeSdf);
    col = mix(col, edgeCol, eFill * 0.92);

    // Soft glow under edges driven by audio bass — never overpowers.
    float eGlow = exp(-max(edgeSdf, 0.0) * 220.0);
    col += eGlow * edgeCol * 0.30 * bass * aud;

    // Nodes: filled disc with thin ink ring for definition.
    float nw    = fwidth(nodeSdf);
    float nFill = 1.0 - smoothstep(-nw, nw, nodeSdf);
    float nRing = smoothstep(-nw * 1.5, 0.0, nodeSdf) - smoothstep(0.0, nw * 1.5, nodeSdf);
    col = mix(col, nodeTint,    nFill);
    col = mix(col, ink,         nRing * 0.55);

    // Labels on top.
    col = mix(col, labelCol, labelMask);

    // ── TELEMETRY — bottom strip with typewriter cue.latest ──────────
    //   Lives in a horizontal band along the bottom; types in as the
    //   sentence arrives. Renders ABOVE everything else.
    {
        int total = charCount();
        bool live = msgAge >= 0.0;
        // Typewriter limit on live utterances.
        int shown = total;
        if (live) {
            float cps = 22.0;
            shown = int(clamp(floor(msgAge * cps), 0.0, float(total)));
        }
        if (shown > 0) {
            // Glyph metrics — fit shown chars across most of the canvas.
            float pad  = 0.04 * aspect;
            float boxW = aspect * 0.92;
            int   maxRow = min(shown, 36);
            float gw   = boxW / float(maxRow);
            float gh   = gw * (7.0 / 5.0);
            // Band is sized to FULLY contain the glyph cell (was 0.05
            // half-height, which clipped the top of the cell when gh was
            // larger). Anchor the cell with its bottom at bandY - gh/2.
            float bandY = -0.45;
            float bandH = gh * 0.55;       // a little padding around cell
            // Inside band?
            if (p.y > bandY - bandH && p.y < bandY + bandH) {
                // Centre vertically inside the band (anchor = bottom of cell).
                float topY  = bandY - gh * 0.5;
                // Walk and render shown chars in a single row, left-aligned.
                float x0 = -0.5 * aspect + pad;
                for (int i = 0; i < 48; i++) {
                    if (i >= shown) break;
                    int ch = getChar(i);
                    if (ch < 0 || ch > 36) continue;
                    if (ch == SPACE_CH) continue;
                    vec2 anchor = vec2(x0 + float(i) * gw, topY);
                    float ds = drawChar(p, anchor, vec2(gw * 0.92, gh), ch);
                    if (ds > 0.001) {
                        col = mix(col, ink, ds);
                    }
                }
                // Caret on the right edge of the typewriter (live only).
                if (live && shown < total) {
                    float caretX = x0 + float(shown) * gw;
                    float caretW = gw * 0.12;
                    float caretH = gh * 0.85;
                    if (abs(p.x - caretX) < caretW
                        && abs(p.y - (topY - gh * 0.5)) < caretH * 0.5) {
                        float blink = step(0.5, fract(TIME * 1.7));
                        col = mix(col, ink, blink);
                    }
                }
            }
        }
    }

    // Final tone — gentle sigmoid to keep highlights from clipping; the
    // host applies its own tonemap, so stay restrained.
    col = col / (1.0 + 0.25 * max(col - 1.0, 0.0));
    col = clamp(col, 0.0, 1.5);

    gl_FragColor = vec4(col, 1.0);
}
