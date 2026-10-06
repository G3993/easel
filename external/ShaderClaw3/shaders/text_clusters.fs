/*{
  "DESCRIPTION": "Clusters — text lives inside soft cell circles that fuse into metaball clusters (Flüx-style). Each chunk of the message gets its own circle; circles within the same cluster bridge with smooth-min SDFs into organic blobs. New clusters keep spawning across the canvas, popping in and fading out so the composition is always evolving. Two-color palette with text-on-color contrast.",
  "CREDIT": "ShaderClaw — inspired by Clear Supply Flüx Modular soft cell kit",
  "CATEGORIES": [
    "Generator",
    "Text"
  ],
  "INPUTS": [
    {
      "NAME": "clusterCount",
      "LABEL": "Clusters",
      "TYPE": "long",
      "DEFAULT": 9,
      "VALUES": [
        4,
        6,
        8,
        9,
        10,
        12,
        14,
        16
      ],
      "LABELS": [
        "4",
        "6",
        "8",
        "9",
        "10",
        "12",
        "14",
        "16"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "nodesPerCluster",
      "LABEL": "Nodes / Cluster",
      "TYPE": "long",
      "DEFAULT": 1,
      "VALUES": [
        1,
        2,
        3,
        4,
        5
      ],
      "LABELS": [
        "1",
        "2",
        "3",
        "4",
        "5"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "nodeRadius",
      "LABEL": "Node Radius",
      "TYPE": "float",
      "DEFAULT": 0.095,
      "MIN": 0.025,
      "MAX": 0.18,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "radiusVariance",
      "LABEL": "Radius Variance",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "bridgeK",
      "LABEL": "Bridge Smoothness",
      "TYPE": "float",
      "DEFAULT": 0.045,
      "MIN": 0,
      "MAX": 0.12,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "interBridgeK",
      "LABEL": "Inter-Cluster Bridge",
      "TYPE": "float",
      "DEFAULT": 0.02,
      "MIN": 0,
      "MAX": 0.35,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "orbitSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "DEFAULT": 0.15,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "spawnRate",
      "LABEL": "Spawn Rate",
      "TYPE": "float",
      "DEFAULT": 0.18,
      "MIN": 0.04,
      "MAX": 0.8,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "morphAmp",
      "LABEL": "Bridge Morph",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "autoTextColor",
      "LABEL": "Auto Text Color",
      "TYPE": "bool",
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "cellA",
      "LABEL": "Cell Color A",
      "TYPE": "color",
      "DEFAULT": [
        0.84,
        0.66,
        0.86,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "cellB",
      "LABEL": "Cell Color B",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.49,
        0.39,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "manualTextColor",
      "LABEL": "Manual Text",
      "TYPE": "color",
      "DEFAULT": [
        0.05,
        0.05,
        0.07,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "BUBBLES APPEAR AS YOU SPEAK",
      "MAX_LENGTH": 48,
      "GROUP": "Text"
    },
    {
      "NAME": "fontFamily",
      "LABEL": "Font",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Inter",
        "Times",
        "Caslon",
        "Outfit"
      ],
      "GROUP": "Text"
    },
    {
      "NAME": "textScale",
      "LABEL": "Cluster Text Size",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.5,
      "MAX": 2,
      "GROUP": "Text"
    },
    {
      "NAME": "kerning",
      "LABEL": "Kerning",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0.55,
      "MAX": 1.4,
      "GROUP": "Text"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0.42,
        0.42,
        0.45,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent BG",
      "TYPE": "bool",
      "DEFAULT": 1,
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ---- universal color block (defaults = no-op) ----
vec3 ucApply(vec3 uc) {
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                      // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    return uc;
}


// =====================================================================
// Clusters — text lives in metaball-fused circles. Each "node" is a
// circle with a chunk of the message; nodes within the same cluster
// smooth-min into one organic blob. Clusters spawn at deterministic
// positions, pop in, hold, fade out — composition evolves continuously.
// =====================================================================

#define MAX_CLUSTERS 16
#define MAX_NODES    5
#define MAX_WALK     64
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

// Count whitespace-delimited words in [0,total). A run of one or more
// SPACE_CH separates words; leading/trailing spaces don't make empty
// words. Returns at least 1 when there's any non-space content.
int wordCount(int total) {
    int words = 0;
    bool inWord = false;
    for (int i = 0; i < 48; i++) {
        if (i >= total) break;
        int ch = getChar(i);
        bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
        if (!isSpace && !inWord) { words++; inWord = true; }
        else if (isSpace)        { inWord = false; }
    }
    return words;
}

// Resolve cluster `c`'s character range [outStart,outEnd) by handing it
// `wordsPer` whole words (skipping separating/leading spaces). Words are
// never split across bubbles. If the cluster is past the end of the
// message, outEnd <= outStart (caller skips it).
void clusterRange(int c, int total, int wordsPer,
                  out int outStart, out int outEnd) {
    int wordsSkip = c * wordsPer;   // words owned by earlier clusters
    int wordsTake = wordsPer;       // words this cluster shows
    int idx = 0;
    // Skip the leading whitespace + `wordsSkip` complete words.
    int skipped = 0;
    bool inWord = false;
    for (int i = 0; i < 48; i++) {
        if (idx >= total) break;
        if (skipped >= wordsSkip && !inWord) break;
        int ch = getChar(idx);
        bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
        if (!isSpace && !inWord) { inWord = true; }
        else if (isSpace && inWord) { inWord = false; skipped++; }
        idx++;
    }
    // Skip whitespace before this cluster's first word.
    for (int i = 0; i < 48; i++) {
        if (idx >= total) break;
        int ch = getChar(idx);
        bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
        if (!isSpace) break;
        idx++;
    }
    outStart = idx;
    // Take `wordsTake` words (including the single spaces between them).
    int taken = 0;
    inWord = false;
    int end = idx;
    for (int i = 0; i < 48; i++) {
        if (idx >= total) break;
        if (taken >= wordsTake && !inWord) break;
        int ch = getChar(idx);
        bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
        if (!isSpace) { inWord = true; idx++; end = idx; }
        else {
            if (inWord) { inWord = false; taken++; }
            if (taken >= wordsTake) break;
            idx++;   // keep single separating space inside the chunk
        }
    }
    outEnd = end;
}

float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  hash21(float n) { return vec2(hash11(n), hash11(n + 17.31)); }

// Smooth minimum — metaball glue between two SDFs.
float smin(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

// Smooth-min variant that ALSO returns the blend factor `h`. h=1 means
// "a" wins, h=0 means "b" wins, anything between is the bridge zone
// — used to mix the two clusters' colors along the connecting tissue.
float smin_h(float a, float b, float k, out float h) {
    h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

// Cheap value noise + 3-octave fbm — used as the slow morphing field
// that perturbs the bridge SDF so connections feel organic, not linear.
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
    for (int i = 0; i < 3; i++) {
        v += a * vnoise(p);
        p = p * 2.07 + vec2(11.3, 5.7);
        a *= 0.5;
    }
    return v;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;

    // Aspect-corrected, centered.
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    float audio = clamp(audioReact, 0.0, 2.0);
    float bass  = audioBass;
    // Continuous smooth band-followers (ambient fix): low knee so ambient's
    // 0.1-0.8 swells span the travel, hi 0.95 keeps EDM headroom.
    float bassSm = smoothstep(0.04, 0.95, audioBass);
    float midSm  = smoothstep(0.05, 0.95, audioMid);

    // Long-typed inputs can arrive unset (0) on the mobile/eval path —
    // fall back to their documented defaults (app never sends 0).
    int   clusters = int(clusterCount);
    if (clusters < 1) clusters = 9;
    if (clusters > MAX_CLUSTERS) clusters = MAX_CLUSTERS;
    int   nodesEach = int(nodesPerCluster);
    if (nodesEach < 1) nodesEach = 1;
    if (nodesEach > MAX_NODES) nodesEach = MAX_NODES;
    int   total     = charCount();
    // User-facing "Cluster Text Size" is now a relative multiplier
    // (0.5×–2.0×, 1.0 = the auto-fit baseline look). The auto-fit
    // block below computes a baseline glyph size that fits the whole
    // word-wrapped sentence inside each bubble; `sizeFactor` scales
    // that baseline. Larger → fewer chars/row → more rows (still
    // inside the bubble); smaller → smaller, centered text.
    float sizeFactor = clamp(textScale, 0.5, 2.0);
    // Absolute glyph-height reference for the legibility cap, derived
    // from the old default (0.020) so the cap tracks the slider too.
    float charH     = 0.020 * sizeFactor;
    float charW     = charH * (5.0 / 7.0);
    float kern      = charW * kerning;

    // No text → blank canvas. Empty msg = nothing to say. When a live
    // utterance is driving `msg` via cue.latest, the typewriter grows
    // total from 0 → full so bubbles appear progressively. When the
    // user typed `msg` manually (no live transcript), total jumps to
    // full immediately and bubbles use the static fallback below.
    if (total <= 0) {
        if (transparentBg) { gl_FragColor = vec4(0.0); return; }
        gl_FragColor = vec4(ucApply(bgColor.rgb), 1.0);
        return;
    }
    bool liveUtterance = msgAge >= 0.0;

    // Every cluster shows the ENTIRE message — each bubble word-wraps
    // the full sentence. `clusters` controls how many bubbles appear;
    // the word-slice split is neutralized (no per-cluster word range).
    // All clusters are "used" — none are skipped for being past the
    // words. The whole-sentence chunk is [0, total) for every bubble.
    int usedClusters = clusters;
    // Per-cluster spawn stagger (live mode): bubbles still pop in
    // progressively rather than all at once, even though each holds
    // the full message. Small fixed offset per cluster index.
    const float CLUSTER_STAGGER = 0.22;   // seconds between bubble births

    // Per-cluster lifecycle durations (seconds). Tuned so the curtain
    // of speech bubbles lasts ~6-7s after the utterance finishes
    // before drifting off.
    const float CPS_EST   = 28.0;   // matches C++ typewriter cps
    const float SPAWN_DUR = 0.40;
    const float HOLD_DUR  = 5.5;
    const float EXIT_DUR  = 1.6;

    // Text-mass + voice driven node size. As the message grows, bubbles
    // grow with it so more glyphs fit; live audio adds a continuous pulse
    // so the bubble breathes with whoever's talking. The fitMax cell-clamp
    // below still wins, so neighbours can never collide.
    float textMass   = smoothstep(6.0, 42.0, float(total));
    float voicePulse = 1.0 + 0.28 * clamp(bass * audio, 0.0, 1.0);
    float sizeMul    = mix(0.55, 1.25, textMass) * voicePulse;

    // Each cluster's life cycles every `lifetime` seconds. Phase staggered
    // by clusterIdx/clusters so spawns never bunch up.
    float lifetime = float(clusters) / max(spawnRate, 0.05);

    // ── Background ─────────────────────────────────────────────────
    vec3 col = bgColor.rgb;

    // Accumulators across all clusters: best (smallest) SDF, the chosen
    // cluster's color, the chosen node's character cell.
    float blobSdf  = 1e6;
    vec3  blobCol  = vec3(0.0);
    float charMask = 0.0;
    vec3  textCol  = vec3(0.0);

    for (int c = 0; c < MAX_CLUSTERS; c++) {
        if (c >= clusters) break;
        float fc = float(c);

        // Every cluster covers the ENTIRE message [0, total) — the whole
        // sentence word-wraps inside each bubble. No per-cluster word
        // slice; no cluster is skipped for being "past the words".
        if (c >= usedClusters) continue;
        int chunkStart = 0;
        int chunkEnd   = total;
        if (chunkEnd <= chunkStart) continue;

        // Two lifecycle modes:
        //   LIVE (msgAge ≥ 0): each cluster is born when the typewriter
        //     reveals its first char; it pops in, holds, drifts up and
        //     exits. msgAge resets on every new utterance.
        //   STATIC (msgAge < 0, manual msg only): all clusters always
        //     alive at full size — no spawn-exit animation. Lets users
        //     preview the shader without a live transcript.
        float tAge, popIn, exitT;
        if (liveUtterance) {
            // chunkStart is now always 0, so stagger births by cluster
            // index instead — bubbles pop in progressively, not all at
            // once. Each holds the full message once born.
            float tBirth = fc * CLUSTER_STAGGER;
            tAge = msgAge - tBirth;
            if (tAge < 0.0) continue;
            if (tAge > SPAWN_DUR + HOLD_DUR + EXIT_DUR) continue;
            popIn = smoothstep(0.0, SPAWN_DUR, tAge);
            exitT = clamp(
                (tAge - SPAWN_DUR - HOLD_DUR) / max(EXIT_DUR, 1e-3),
                0.0, 1.0);
        } else {
            tAge  = SPAWN_DUR + HOLD_DUR * 0.5;   // mid-life
            popIn = 1.0;
            exitT = 0.0;
        }
        float fadeOut = 1.0 - exitT;
        float env     = popIn * fadeOut;
        if (env < 0.01) continue;
        // For the legacy bass-pulse on freshly-born clusters; matches
        // the prior 0-25% spawn window heuristic.
        float phase = clamp(tAge / max(SPAWN_DUR + HOLD_DUR, 1e-3), 0.0, 1.0);

        // Grid-packed deterministic anchor — each cluster gets its own
        // cell so bubbles don't pile up. Grid sized from cluster count
        // and aspect; small jitter inside the cell keeps the layout
        // breathing rather than rigid. Like discrete speech bubbles
        // across the canvas, each in its own spatial neighborhood.
        vec2 baseSeed = hash21(fc * 13.7);
        int gridX = int(ceil(sqrt(float(clusters) * max(aspect, 0.5))));
        if (gridX < 1) gridX = 1;
        int gridY = (clusters + gridX - 1) / gridX;
        int cx = c - (c / gridX) * gridX;
        int cy = c / gridX;
        float canvasW = aspect - 0.18;
        float canvasH = 0.90;
        float cellW   = canvasW / float(gridX);
        float cellH   = canvasH / float(gridY);
        vec2 anchor;
        anchor.x = -0.5 * canvasW + (float(cx) + 0.5) * cellW;
        anchor.y = -0.5 * canvasH + (float(cy) + 0.5) * cellH;
        // Per-cluster jitter inside the cell — keeps the grid from
        // looking like a literal grid. Capped so bubbles can't reach
        // the cell boundary (≤ 18% of cell extent each way).
        anchor.x += (baseSeed.x - 0.5) * cellW * 0.18;
        anchor.y += (baseSeed.y - 0.5) * cellH * 0.18;
        // Slow per-cluster drift — gentle organic motion within the cell.
        float driftSeed = fc * 7.21;
        anchor.x += cellW * 0.06 * sin(TIME * 0.18 + driftSeed);
        anchor.y += cellH * 0.06 * cos(TIME * 0.22 + driftSeed * 1.7);

        // Exit drift: float upward off the canvas with a slight sway. Each
        // cluster picks its own sway phase so the exit isn't a uniform
        // marching column.
        if (exitT > 0.0) {
            float ease = exitT * exitT * (3.0 - 2.0 * exitT); // Hermite
            anchor.y += ease * 0.70;
            anchor.x += ease * 0.05 * sin(driftSeed * 3.1 + TIME * 0.8);
        }

        // Two-color tint: alternate by cluster index for visual rhythm.
        vec3 cTint = (mod(fc, 2.0) < 0.5) ? cellA.rgb : cellB.rgb;
        // Pop-in scale (cluster grows from a point at spawn).
        float clusterScale = mix(0.4, 1.0, popIn);

        // Auto-fit: clamp the cluster's max footprint to ~42% of the
        // smaller cell dimension so neighbours never touch. Single-node
        // clusters use the first-node spread factor (0.4×orbR + rad)
        // ≈ nodeRadius·2.12; multi-node clusters orbit the anchor with
        // spread=1.0 giving max-extent ≈ nodeRadius·3.32.
        float extentFactor = (nodesEach > 1) ? 3.32 : 2.12;
        float fitMax  = 0.42 * min(cellW, cellH);
        float fitRad  = min(nodeRadius * sizeMul, fitMax / extentFactor);

        // Build the cluster's metaball SDF: smooth-min of every node circle.
        // Track the closest node so we know which character cell to draw.
        float clusterSdf  = 1e6;
        int   nearestNode = 0;
        float nearestDist = 1e6;
        vec2  nearestPos  = anchor;
        float nearestRad  = fitRad;

        for (int n = 0; n < MAX_NODES; n++) {
            if (n >= nodesEach) break;
            float fn = float(n);
            // Node-specific seed.
            vec2 ns = hash21(fc * 41.3 + fn * 7.7);

            // Node sits offset from anchor on a slow orbit; offset radius
            // scales with cluster's mean node size.
            float orbR  = fitRad * (1.4 + 0.6 * ns.x);
            float orbA  = ns.y * 6.2832
                        + TIME * orbitSpeed * (1.0 + 0.4 * (ns.x - 0.5));
            // First node sits closer to anchor; later nodes spread outward.
            float spread = (n == 0) ? 0.4 : 1.0;
            vec2 nodeP = anchor + spread * orbR
                       * vec2(cos(orbA), sin(orbA)) * clusterScale;

            // Node radius variance — some nodes much bigger than others
            // (matches the "1 big + 2 small bridges" look of the reference).
            float rad = fitRad
                      * mix(1.0, 0.4 + 1.4 * hash11(fc * 31.1 + fn * 5.3),
                            radiusVariance);
            // Bass pulse — applied AFTER the fitRad auto-fit cap so it
            // isn't swallowed by the neighbour-collision clamp above.
            // Always-on (not just the spawn window) so live clusters keep
            // breathing with the music for their whole hold phase.
            rad *= 1.0 + (0.18 * bassSm + 0.10 * midSm) * audio;
            rad *= clusterScale;

            // Circle SDF.
            float d = length(p - nodeP) - rad;

            // Metaball-glue with running min.
            if (n == 0) clusterSdf = d;
            else        clusterSdf = smin(clusterSdf, d, bridgeK);

            // Track nearest node for character cell mapping.
            float pd = length(p - nodeP);
            if (pd < nearestDist) {
                nearestDist = pd;
                nearestNode = n;
                nearestPos  = nodeP;
                nearestRad  = rad;
            }
        }

        // Fade-aware SDF (puff out slightly as it spawns / expires for
        // softer appearance).
        clusterSdf -= (env - 1.0) * 0.005;

        // ─── Inter-cluster bridge ───────────────────────────────
        // Chain smooth-min ALL clusters into the running compositeSdf
        // with a wider k than intra-cluster. Distant clusters retain
        // their separate silhouettes; close ones grow connecting
        // ribbons. The h factor blends the two clusters' colors
        // along the bridge so the underlying tissue reads smoothly.
        float bk = max(interBridgeK, 0.001);
        if (c == 0) {
            blobSdf = clusterSdf;
            blobCol = cTint;
        } else {
            float h;
            blobSdf = smin_h(blobSdf, clusterSdf, bk, h);
            // h=1 means existing wins; h=0 means current wins.
            blobCol = mix(cTint, blobCol, h);
        }

        // Anti-aliased fill check for the pixel — uses the per-cluster
        // SDF so text only renders inside its OWN cluster (the bridges
        // are background tissue, not text-bearing nodes).
        float fw   = fwidth(clusterSdf);
        float fill = 1.0 - smoothstep(-fw, fw, clusterSdf);
        if (fill < 0.001) continue;

        // ─── Multi-line word-wrapped text inside the primary node ───
        // Text only renders on the cluster's first node (n==0). With the
        // default 1-node layout this is "the bubble"; with multi-node
        // clusters, extra nodes are visual extensions of the same bubble
        // and stay text-free so glyphs don't double-render.
        if (nearestNode != 0) continue;
        vec2 localP = p - nearestPos;

        // Inscribed square that fits inside the circular bubble: side
        // 2·boxHalf with boxHalf = r·0.70 (0.70 < 1/√2 ≈ 0.707, so the
        // whole text box stays inside the silhouette — no glyphs land in
        // the corners outside the blob where fill→0 and they'd vanish).
        float boxHalf = nearestRad * 0.70;

        // This bubble shows the WHOLE message [chunkStart, chunkEnd) =
        // [0, total). The full sentence word-wraps inside the box.
        int chunkN = chunkEnd - chunkStart;
        if (chunkN < 1) chunkN = 1;

        float boxW = boxHalf * 2.0;

        // Longest whitespace-delimited word in [chunkStart,chunkEnd):
        // tracks the max run of non-space glyphs (same SPACE_CH==26 /
        // out-of-range test wordCount uses). The row width must be at
        // least this wide so a whole word never splits mid-word.
        int longestWord = 0;
        {
            int run = 0;
            for (int i = 0; i < MAX_WALK; i++) {
                int gIdx = chunkStart + i;
                if (gIdx >= chunkEnd) break;
                int ch = getChar(gIdx);
                bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
                if (isSpace) { run = 0; }
                else { run++; if (run > longestWord) longestWord = run; }
            }
        }
        // Absolute legibility cap on the row width. If a single token is
        // longer than this, that token (only) is allowed to hard-wrap so
        // glyphs stay readable instead of shrinking to nothing.
        int rowCap = (total + 47) / 48 + 24;   // ~24 for ≤48-char msgs
        if (rowCap > 48) rowCap = 48;
        if (rowCap < 1)  rowCap = 1;

        // Pick a column count for a roughly square block, then widen it
        // so the LONGEST WORD fits on one row → words wrap only at word
        // boundaries, never split (unless a lone word exceeds rowCap, in
        // which case that token hard-wraps). A word-wrap PRE-PASS then
        // counts the rows the full message needs at that width; glyphs
        // are sized so ALL rows fit vertically — never clipped.
        // Baseline column count for a roughly square block at 1.0×.
        // The size slider scales glyphs by shrinking the column count:
        // bigger text (sizeFactor>1) → fewer chars/row → more rows,
        // each glyph wider/taller but still inside the same box.
        // Smaller text (sizeFactor<1) → more chars/row → fewer rows →
        // smaller, centered glyphs. Division can't be zero (sizeFactor
        // clamped ≥ 0.5). longestWord/rowCap clamps below still keep
        // whole words intact and enforce the legibility floor.
        float baseCols  = ceil(sqrt(float(chunkN) * 1.6));
        int charsPerRow = int(ceil(baseCols / sizeFactor));
        if (charsPerRow < longestWord) charsPerRow = longestWord;
        if (charsPerRow > rowCap) charsPerRow = rowCap;
        if (charsPerRow < 1) charsPerRow = 1;
        if (charsPerRow > 48) charsPerRow = 48;

        // Pre-pass: simulate the same word-wrap the render walk does and
        // count total rows used (usedRows = last cursorR + 1).
        int usedRows = 1;
        {
            int preR = 0;
            int preC = 0;
            for (int i = 0; i < MAX_WALK; i++) {
                int gIdx = chunkStart + i;
                if (gIdx >= chunkEnd) break;
                int ch = getChar(gIdx);
                if (ch == SPACE_CH) {
                    int wlen = 0;
                    for (int j = 1; j < MAX_WALK; j++) {
                        int gj = chunkStart + i + j;
                        if (gj >= chunkEnd) break;
                        int chj = getChar(gj);
                        if (chj == SPACE_CH || chj < 0 || chj > 36) break;
                        wlen++;
                    }
                    if (preC > 0 && preC + 1 + wlen > charsPerRow) {
                        preR++; preC = 0;
                    } else if (preC > 0) {
                        preC++;
                    }
                } else if (ch >= 0 && ch <= 36) {
                    preC++;
                    if (preC >= charsPerRow) { preR++; preC = 0; }
                }
            }
            usedRows = preR + 1;
            if (usedRows < 1) usedRows = 1;
        }
        int maxRows = usedRows;

        // Size glyphs to fit the box WIDTH per the charsPerRow/word-wrap
        // logic. Height no longer spreads rows across the whole box —
        // instead each line gets a tight pitch derived from the glyph
        // height (glyph + a small gap), so wrapped lines sit close
        // together (snug leading) rather than far apart.
        float effKern  = boxW / float(charsPerRow);
        float effCharW = effKern / max(kerning, 0.55);
        float effCharH = effCharW * (7.0 / 5.0);
        effCharH = min(effCharH, charH * 6.0);

        // Tight leading: line pitch = glyph height × LEADING (glyph +
        // a small inter-line gap), DECOUPLED from boxW/maxRows. This is
        // what packs the wrapped rows closer together.
        const float LEADING = 1.18;
        float linePitch = effCharH * LEADING;

        // Overflow guard: if many rows make the tight block taller than
        // the box, scale the glyph (and pitch with it) down so it still
        // fits — never clip, never overflow the bubble. No div-by-zero:
        // maxRows ≥ 1, LEADING > 0, effCharH > 0.
        float blockH = float(maxRows) * linePitch;
        if (blockH > boxW) {
            float shrink = boxW / blockH;
            effCharH  *= shrink;
            effCharW  *= shrink;
            linePitch *= shrink;
            blockH     = boxW;
        }
        effCharW = min(effCharW, effCharH * (5.0 / 7.0));

        // Vertically center the (now shorter) tight block in the box.
        float lineH = linePitch;          // row stride for the walk below
        float yOff  = (boxW - blockH) * 0.5;

        // Pixel position inside the text box. Top-left origin so rows
        // read top→bottom and columns read left→right (left-aligned).
        float lx = localP.x + boxHalf;
        float ly = (boxHalf - localP.y) - yOff;   // shift into centered block
        if (lx < 0.0 || lx > boxW) continue;
        if (ly < 0.0 || ly > blockH) continue;     // clip to actual block only

        int targetCol = int(floor(lx / effKern));
        int targetRow = int(floor(ly / lineH));
        if (targetCol >= charsPerRow) continue;
        if (targetRow >= maxRows)     continue;

        // Center the glyph vertically within its lineH strip (the
        // remainder is inter-line whitespace). yInRow is the offset
        // from the glyph's top edge.
        float rowPad = (lineH - effCharH) * 0.5;
        float yInRow = (ly - float(targetRow) * lineH) - rowPad;
        if (yInRow < 0.0 || yInRow > effCharH) continue;

        // Full message [chunkStart, chunkEnd) = [0, total). The pre-pass
        // above sized the grid to hold every row the word-wrap needs, so
        // this walk just lays the whole sentence onto that grid with the
        // SAME wrap rules (whole words wrap as a unit; over-long word
        // hard-wraps). outCh is filled when the walk reaches targetCell.
        int cursorR = 0;
        int cursorC = 0;
        int outCh = -1;

        for (int i = 0; i < MAX_WALK; i++) {
            int globalIdx = chunkStart + i;
            if (globalIdx >= chunkEnd) break;        // past cluster's chunk
            if (cursorR > targetRow) break;

            int ch = getChar(globalIdx);

            if (ch == SPACE_CH) {
                // Look-ahead: length of the upcoming word (bounded by
                // the cluster's chunk — words don't span across bubbles).
                int wlen = 0;
                for (int j = 1; j < MAX_WALK; j++) {
                    int jj = i + j;
                    int gj = chunkStart + jj;
                    if (gj >= chunkEnd) break;
                    int chj = getChar(gj);
                    if (chj == SPACE_CH || chj < 0 || chj > 36) break;
                    wlen++;
                }
                if (cursorC > 0 && cursorC + 1 + wlen > charsPerRow) {
                    // Wrap before the word: drop the space, advance row.
                    cursorR++;
                    cursorC = 0;
                } else if (cursorC > 0) {
                    if (cursorR == targetRow && cursorC == targetCol) {
                        outCh = SPACE_CH;
                    }
                    cursorC++;
                }
                // Leading-space-on-new-row is silently consumed.
            } else if (ch >= 0 && ch <= 36) {
                if (cursorR == targetRow && cursorC == targetCol) {
                    outCh = ch;
                }
                cursorC++;
                if (cursorC >= charsPerRow) {
                    // Hard wrap — single word exceeded row width.
                    cursorR++;
                    cursorC = 0;
                }
            }
        }

        // Space cells render nothing (atlas idx 26 is blank); skip.
        if (outCh < 0 || outCh > 35 || outCh == SPACE_CH) continue;

        // Glyph centered in its effKern-wide column. V flipped: ly grows
        // top→bottom on screen, atlas glyphs are stored with V=1 at top
        // (matches OpenGL texture origin at bottom-left).
        float colPad = (effKern - effCharW) * 0.5;
        vec2 cellLocal = vec2((lx - float(targetCol) * effKern - colPad) / effCharW,
                              1.0 - yInRow / effCharH);
        float s = sampleChar(outCh, cellLocal);
        s = smoothstep(0.18, 0.55, s);
        if (s > 0.001) {
            vec3 inkColor;
            if (autoTextColor) {
                float lum = dot(cTint, vec3(0.299, 0.587, 0.114));
                inkColor = (lum > 0.55) ? vec3(0.04, 0.04, 0.07) : vec3(1.0);
            } else {
                inkColor = manualTextColor.rgb;
            }
            float w = s * env * fill;
            charMask = max(charMask, w);
            textCol  = mix(textCol, inkColor, w);
        }
    }

    // Slow morphing perturbation on the bridges — nudges the SDF
    // by a low-frequency fbm so the connecting tissue between
    // clusters wobbles and blooms organically rather than reading
    // as straight metaball capsules.
    if (morphAmp > 0.001) {
        float n = fbm2(p * 1.4 + vec2(TIME * 0.07, TIME * -0.05));
        blobSdf -= (n - 0.5) * morphAmp * 0.07;
    }

    // Compose: bg ← cluster blob ← text ink. Bass lights the whole
    // blob tissue up a touch so the cell color itself breathes with
    // the music (audible even when node-radius pulse alone would be
    // too thin a band to read at a glance).
    vec3 blobColLit = blobCol * (1.0 + (0.16 * bassSm + 0.08 * midSm) * audio);
    float blobFw   = fwidth(blobSdf);
    float blobFill = 1.0 - smoothstep(-blobFw, blobFw, blobSdf);
    col = mix(col, blobColLit, blobFill);
    col = mix(col, textCol, charMask);

    float alpha = 1.0;
    if (transparentBg) {
        alpha = clamp(blobFill, 0.0, 1.0);
        col   = mix(blobColLit, textCol, charMask);
    }

    gl_FragColor = vec4(ucApply(col), alpha);
}
