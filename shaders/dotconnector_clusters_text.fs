/*{
  "DESCRIPTION": "Dot-Connector Clusters — N constellation clusters drift across a warm paper canvas. Each cluster is a different 'player': its nodes pulse, its connector lines bloom, and its caption typewriters in when that player's channel goes hot. Inspired by Catalan editorial poster constellations, but each cluster is bound to its own player[i] channel — mute one and you instantly see which constellation went still. Parallax-layered (dust → connectors → nodes → label) with bass-driven line glow. Returns LINEAR HDR.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "LABEL": "Caption", "TYPE": "text", "DEFAULT": "LA TABACALERA. VIDEO I PROJECCIONS.", "MAX_LENGTH": 48, "BIND": "cue.latest" },
    { "NAME": "clusterCount", "LABEL": "Clusters", "TYPE": "long", "DEFAULT": 4, "VALUES": [3,4,5,6], "LABELS": ["3","4","5","6"] },

    { "NAME": "energyA", "LABEL": "Player 1 Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB", "LABEL": "Player 2 Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC", "LABEL": "Player 3 Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "energyD", "LABEL": "Player 4 Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[4].energy" },
    { "NAME": "energyE", "LABEL": "Player 5 Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[5].energy" },
    { "NAME": "energyF", "LABEL": "Player 6 Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[6].energy" },

    { "NAME": "activeA", "LABEL": "Player 1 Active", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].active" },
    { "NAME": "activeB", "LABEL": "Player 2 Active", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].active" },
    { "NAME": "activeC", "LABEL": "Player 3 Active", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[3].active" },
    { "NAME": "activeD", "LABEL": "Player 4 Active", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[4].active" },
    { "NAME": "activeE", "LABEL": "Player 5 Active", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[5].active" },
    { "NAME": "activeF", "LABEL": "Player 6 Active", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[6].active" },

    { "NAME": "bassDrive", "LABEL": "Bass Drive", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.bass" },

    { "NAME": "paperColor", "LABEL": "Paper", "TYPE": "color", "DEFAULT": [0.91, 0.89, 0.85, 1.0] },
    { "NAME": "dotColor",   "LABEL": "Dot",   "TYPE": "color", "DEFAULT": [0.08, 0.07, 0.07, 1.0] },
    { "NAME": "lineColor",  "LABEL": "Line",  "TYPE": "color", "DEFAULT": [0.82, 0.10, 0.14, 1.0] },
    { "NAME": "inkColor",   "LABEL": "Ink",   "TYPE": "color", "DEFAULT": [0.05, 0.05, 0.07, 1.0] },

    { "NAME": "dotRadius",     "LABEL": "Dot Radius",     "TYPE": "float", "DEFAULT": 0.013, "MIN": 0.004, "MAX": 0.035 },
    { "NAME": "lineWidth",     "LABEL": "Line Width",     "TYPE": "float", "DEFAULT": 0.0015, "MIN": 0.0005, "MAX": 0.006 },
    { "NAME": "clusterSpread", "LABEL": "Cluster Spread", "TYPE": "float", "DEFAULT": 0.18, "MIN": 0.05, "MAX": 0.40 },
    { "NAME": "driftSpeed",    "LABEL": "Drift Speed",    "TYPE": "float", "DEFAULT": 0.15, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "kerning",       "LABEL": "Kerning",        "TYPE": "float", "DEFAULT": 0.88, "MIN": 0.55, "MAX": 1.4 },
    { "NAME": "labelScale",    "LABEL": "Label Scale",    "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.5, "MAX": 2.0 },
    { "NAME": "glowAmount",    "LABEL": "Line Glow",      "TYPE": "float", "DEFAULT": 0.6,  "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "depthAmount",   "LABEL": "Depth Parallax", "TYPE": "float", "DEFAULT": 0.55, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "restEnergy",    "LABEL": "Rest Energy",    "TYPE": "float", "DEFAULT": 0.18, "MIN": 0.0, "MAX": 1.0 }
  ]
}*/

// =====================================================================
// Dot-Connector Clusters — N constellation clusters, one per player.
// Each cluster has 6 nodes + a deterministic spanning-path of connector
// lines. Active player → nodes pulse, lines saturate, label fades in
// with typewriter reveal. Inactive players → muted, low contrast,
// hovering at rest.
// =====================================================================

#define MAX_CLUSTERS 6
#define NODES_PER    7
#define MAX_WALK     48
#define SPACE_CH     26

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

// ─── Utility ────────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  hash21(float n) { return vec2(hash11(n), hash11(n + 17.31)); }

// Per-cluster energy/active accessor. Index is 0..5 mapping to A..F.
float clusterEnergy(int i) {
    if (i == 0) return energyA;
    if (i == 1) return energyB;
    if (i == 2) return energyC;
    if (i == 3) return energyD;
    if (i == 4) return energyE;
    return energyF;
}
float clusterActive(int i) {
    if (i == 0) return activeA;
    if (i == 1) return activeB;
    if (i == 2) return activeC;
    if (i == 3) return activeD;
    if (i == 4) return activeE;
    return activeF;
}

// Deterministic node position inside a unit-radius local cluster frame.
// 7 nodes, irregular but reproducible per (cluster, node).
vec2 nodeLocal(int c, int n) {
    float seed = float(c) * 13.137 + float(n) * 5.713;
    vec2 r = hash21(seed) - 0.5;
    // Push outward a touch so nodes don't pile on the center.
    float ang = hash11(seed * 1.91) * 6.2832;
    vec2 ring = 0.42 * vec2(cos(ang), sin(ang));
    return r * 1.05 + ring * (0.4 + 0.6 * hash11(seed * 2.3));
}

// Distance from point p to segment ab.
float sdSegment(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-6), 0.0, 1.0);
    return length(pa - ba * h);
}

// Connector graph — deterministic spanning walk through the cluster's
// nodes. NOT a straight zig-zag: each cluster picks its own permutation
// from the seed so silhouettes differ. Returns the i-th edge endpoints.
ivec2 edge(int c, int i) {
    // Simple permutation: rotate the index ring by a per-cluster offset
    // and skip-step so edges crisscross. Stays inside [0, NODES_PER).
    int off = int(hash11(float(c) * 7.7) * float(NODES_PER));
    int step = 1 + int(hash11(float(c) * 11.3 + float(i) * 0.71) * 3.0);
    int a = (i * step + off) - ((i * step + off) / NODES_PER) * NODES_PER;
    int b = ((i + 1) * step + off) - (((i + 1) * step + off) / NODES_PER) * NODES_PER;
    return ivec2(a, b);
}

// Cheap value noise + 2-octave fbm for paper grain + dust layer.
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash11(dot(i, vec2(1.0, 157.0)));
    float b = hash11(dot(i + vec2(1.0, 0.0), vec2(1.0, 157.0)));
    float c = hash11(dot(i + vec2(0.0, 1.0), vec2(1.0, 157.0)));
    float d = hash11(dot(i + vec2(1.0, 1.0), vec2(1.0, 157.0)));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

// Anchor for cluster c — drifts slowly, packed into the canvas via a
// grid so clusters don't overlap. Cluster index is 0-based.
vec2 clusterAnchor(int c, int clusters, float aspect) {
    int gridX = int(ceil(sqrt(float(clusters) * max(aspect, 0.5))));
    if (gridX < 1) gridX = 1;
    int gridY = (clusters + gridX - 1) / gridX;
    int cx = c - (c / gridX) * gridX;
    int cy = c / gridX;
    float canvasW = aspect - 0.15;
    float canvasH = 0.85;
    float cellW = canvasW / float(gridX);
    float cellH = canvasH / float(gridY);
    vec2 a;
    a.x = -0.5 * canvasW + (float(cx) + 0.5) * cellW;
    a.y = -0.5 * canvasH + (float(cy) + 0.5) * cellH;
    // Slow drift inside the cell.
    float s = float(c) * 6.18;
    a.x += cellW * 0.08 * sin(TIME * driftSpeed * 0.7 + s);
    a.y += cellH * 0.08 * cos(TIME * driftSpeed * 0.9 + s * 1.3);
    return a;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    // Aspect-corrected, centered.
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    int clusters = int(clusterCount);
    if (clusters < 1) clusters = 1;
    if (clusters > MAX_CLUSTERS) clusters = MAX_CLUSTERS;

    int total = charCount();
    bool liveUtterance = msgAge >= 0.0;

    // ── Background: warm paper + subtle grain + concrete-flecked vignette ──
    vec3 col = paperColor.rgb;
    // Paper grain (cheap fbm) — extremely subtle.
    float grain = vnoise(p * 240.0) * 0.04 - 0.02;
    col += vec3(grain);
    // Soft vignette so the poster has a corner pull like the reference.
    float vig = smoothstep(0.95, 0.20, length(p * vec2(0.85, 1.05)));
    col *= mix(0.86, 1.02, vig);

    // ── Parallax dust layer — far behind the constellations ──
    {
        vec2 dustP = p * 6.0 + vec2(TIME * driftSpeed * 0.08, TIME * driftSpeed * 0.05);
        float d = vnoise(dustP);
        d *= vnoise(dustP * 2.31 + 7.0);
        float dustMask = smoothstep(0.62, 0.85, d) * depthAmount * 0.18;
        col = mix(col, vec3(0.55, 0.50, 0.46), dustMask);
    }

    // Bass — only used to add a global glow boost to the lines so the
    // audio reactivity is felt without overpowering per-player decomposition.
    float bass = clamp(bassDrive, 0.0, 1.0);

    // Strongest active cluster determines which gets the caption.
    // The caption text always renders next to whichever cluster has
    // the highest energy (with active as a tiebreaker).
    int focalC = 0;
    float bestScore = -1.0;
    for (int c = 0; c < MAX_CLUSTERS; c++) {
        if (c >= clusters) break;
        float e = clusterEnergy(c);
        float a = clusterActive(c);
        float s = e + a * 0.05;
        if (s > bestScore) { bestScore = s; focalC = c; }
    }

    // Accumulators for line/dot/text layers (premultiplied alpha mixing).
    vec3 lineAccum = vec3(0.0);
    float lineAlpha = 0.0;
    vec3 dotAccum  = vec3(0.0);
    float dotAlpha = 0.0;
    vec3 textAccum = vec3(0.0);
    float textAlpha = 0.0;

    // ── Per-cluster pass ──────────────────────────────────────────
    for (int c = 0; c < MAX_CLUSTERS; c++) {
        if (c >= clusters) break;

        float energy = clamp(clusterEnergy(c), 0.0, 1.0);
        float live   = clamp(clusterActive(c), 0.0, 1.0);
        // Resting state — never fully dead so the composition reads even
        // with no audio (the loop's test feeds tend to zero everything).
        float life   = max(energy, restEnergy);

        // Cluster anchor & local spread.
        vec2 anchor = clusterAnchor(c, clusters, aspect);
        float spread = clusterSpread * (0.9 + 0.2 * hash11(float(c) * 3.71));
        // Active cluster grows; inactive cluster contracts a touch.
        spread *= mix(0.92, 1.08, life);
        // Parallax: active cluster nudges forward (slightly larger),
        // inactive recedes — fakes depth without raymarching.
        float zScale = mix(0.88, 1.0 + 0.08 * depthAmount, live);
        spread *= zScale;

        // ── Connector lines (deterministic spanning walk) ──
        // Draw NODES_PER edges through the cluster — creates the
        // crisscrossing constellation look from the reference.
        float lineHit = 1e6;
        for (int e = 0; e < NODES_PER; e++) {
            ivec2 ed = edge(c, e);
            vec2 na = anchor + spread * nodeLocal(c, ed.x);
            vec2 nb = anchor + spread * nodeLocal(c, ed.y);
            float ds = sdSegment(p, na, nb);
            if (ds < lineHit) lineHit = ds;
        }
        float lw = lineWidth * (0.6 + 0.6 * life) * (1.0 + 0.5 * bass);
        float lfw = fwidth(lineHit);
        float lineCore = 1.0 - smoothstep(lw - lfw, lw + lfw, lineHit);
        // Glow halo around the line — bass-modulated.
        float glowR = lw * (4.0 + 18.0 * life * (0.7 + 0.8 * bass) * glowAmount);
        float lineGlow = exp(-pow(lineHit / max(glowR, 1e-4), 1.6));
        lineGlow *= 0.35 * glowAmount * (0.4 + 0.9 * life);
        float lineMask = clamp(lineCore + lineGlow * (1.0 - lineCore), 0.0, 1.5);

        // Line color: saturates with energy. Inactive clusters fade
        // toward a desaturated near-black (still readable on paper).
        vec3 muted = mix(vec3(0.32, 0.28, 0.28), lineColor.rgb, 0.35);
        vec3 lc    = mix(muted, lineColor.rgb, smoothstep(0.05, 0.7, life));
        // HDR pop for the actively-driven cluster.
        lc *= 1.0 + 0.8 * life * live;
        lineAccum += lc * lineMask;
        lineAlpha += lineMask;

        // ── Nodes (dots) ──
        // Each node draws as a soft-edged disk with a faint inner
        // highlight; active cluster nodes pulse with energy.
        float pulse = 1.0 + 0.55 * energy * sin(TIME * (3.0 + float(c) * 0.7));
        float baseR = dotRadius * (0.8 + 0.6 * life) * pulse;
        for (int n = 0; n < NODES_PER; n++) {
            vec2 np = anchor + spread * nodeLocal(c, n);
            // Per-node radius jitter — some nodes bigger like the reference.
            float jr = baseR * (0.7 + 0.6 * hash11(float(c) * 21.1 + float(n) * 3.3));
            float dd = length(p - np);
            float dfw = fwidth(dd);
            float dotM = 1.0 - smoothstep(jr - dfw, jr + dfw, dd);
            // Inner ring highlight (only on active clusters for emphasis).
            float ring = (1.0 - smoothstep(jr * 0.55 - dfw, jr * 0.55 + dfw, dd))
                       - (1.0 - smoothstep(jr * 0.35 - dfw, jr * 0.35 + dfw, dd));
            vec3 dc = mix(vec3(0.35, 0.32, 0.30), dotColor.rgb,
                          smoothstep(0.0, 0.6, life));
            // Glow around active nodes.
            float dglow = exp(-pow(dd / (jr * 5.0 * (1.0 + live * 1.5)), 2.0))
                         * 0.18 * live * glowAmount;
            dotAccum += dc * dotM + lineColor.rgb * ring * 0.55 * live;
            dotAccum += lineColor.rgb * dglow;
            dotAlpha += dotM + dglow * 0.3;
        }

        // ── Per-cluster caption ──
        // Only the focal cluster (highest energy / active) gets the
        // typewriter caption — like the "a:", "b:" labels in the
        // reference, the text belongs to a chosen constellation.
        if (c == focalC && total > 0) {
            // Caption anchor: top-left of the cluster, like the
            // reference's "a:" labels above each constellation.
            vec2 captAnchor = anchor + vec2(-spread * 0.95, spread * 0.55);

            // Glyph metrics.
            float charH = 0.022 * labelScale;
            float charW = charH * (5.0 / 7.0);
            float kern  = charW * kerning;

            // Word-wrap to a reasonable column count.
            int charsPerRow = 10;
            int rowsNeeded  = (total + charsPerRow - 1) / charsPerRow;
            if (rowsNeeded < 1) rowsNeeded = 1;
            float lineH = charH * 1.25;

            // Typewriter reveal: characters appear at ~28 cps.
            int visibleN = total;
            float captionFade = 1.0;
            if (liveUtterance) {
                visibleN = int(floor(msgAge * 28.0));
                if (visibleN > total) visibleN = total;
                if (visibleN < 0) visibleN = 0;
                captionFade = smoothstep(0.0, 0.3, msgAge);
            }

            // Caption fade also coupled to focal cluster's activity.
            float labelFade = mix(0.4, 1.0, max(energy, live));
            captionFade *= labelFade;

            // Pixel position relative to caption origin (top-left).
            vec2 lp = p - captAnchor;
            // Flip Y so rows read top→bottom.
            float lx = lp.x;
            float ly = -lp.y;
            if (lx >= 0.0 && ly >= 0.0
                && lx < float(charsPerRow) * kern
                && ly < float(rowsNeeded) * lineH) {
                int targetCol = int(floor(lx / kern));
                int targetRow = int(floor(ly / lineH));
                if (targetCol >= 0 && targetCol < charsPerRow
                    && targetRow >= 0 && targetRow < rowsNeeded) {
                    // Walk with word-wrap.
                    int cursorR = 0;
                    int cursorC = 0;
                    int outCh = -1;
                    for (int i = 0; i < MAX_WALK; i++) {
                        if (i >= visibleN) break;
                        if (cursorR > targetRow) break;
                        int ch = getChar(i);
                        if (ch == SPACE_CH) {
                            // Look-ahead for word length.
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
                        // Local glyph UV.
                        float gx = (lx - float(targetCol) * kern) / charW;
                        float gy = 1.0 - (ly - float(targetRow) * lineH) / charH;
                        float s = sampleChar(outCh, vec2(gx, gy));
                        s = smoothstep(0.18, 0.55, s);
                        float w = s * captionFade;
                        textAccum += inkColor.rgb * w;
                        textAlpha += w;
                    }
                }
            }

            // Small "label dot" (the colon/dot lead-in like the
            // reference's "a:") just left of the caption.
            {
                vec2 dotPos = captAnchor + vec2(-charW * 1.8, -charH * 0.45);
                float dd = length(p - dotPos);
                float r = charH * 0.18;
                float dfw = fwidth(dd);
                float m = 1.0 - smoothstep(r - dfw, r + dfw, dd);
                textAccum += inkColor.rgb * m * captionFade;
                textAlpha += m * captionFade;
            }
        }
    }

    // ── Compose layers, back→front: lines → dots → text ──
    lineAlpha = clamp(lineAlpha, 0.0, 1.0);
    if (lineAlpha > 0.001) {
        col = mix(col, lineAccum / max(lineAlpha, 1e-3), lineAlpha);
    }
    dotAlpha = clamp(dotAlpha, 0.0, 1.0);
    if (dotAlpha > 0.001) {
        col = mix(col, dotAccum / max(dotAlpha, 1e-3), dotAlpha);
    }
    textAlpha = clamp(textAlpha, 0.0, 1.0);
    if (textAlpha > 0.001) {
        col = mix(col, textAccum / max(textAlpha, 1e-3), textAlpha);
    }

    gl_FragColor = vec4(col, 1.0);
}
