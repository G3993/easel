/*{
  "DESCRIPTION": "Pixel Type Text — chunky pixel-grid typography as the hero. The msg input is rendered as a giant blocky headline that fills the canvas, every glyph quantized onto a coarse pixel lattice and tinted by a color-shuffle palette so each block lights its own hue. msgAge stamps the message in left-to-right with a typewriter wave; bass jiggles the pixel grid on the X axis like a CRT bleed. Behind the headline a layered field of tetromino sprite-fragments drifts on a slow flow, each fragment bound to one of three synthetic players so distinct voices push distinct shapes. A back wall of soft pinstripes parallaxes at half speed for depth. Quiet = still glyphs on a black wall; loud = blocks jitter, sprites swarm, palette boils.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "TYPE": "text", "DEFAULT": "PIXEL TYPE", "MAX_LENGTH": 48, "BIND": "cue.latest" },
    { "NAME": "pixelSize",    "LABEL": "Pixel Size",       "TYPE": "float", "DEFAULT": 110.0, "MIN": 40.0,  "MAX": 260.0 },
    { "NAME": "glyphSpacing", "LABEL": "Glyph Spacing",    "TYPE": "float", "DEFAULT": 1.05,  "MIN": 0.75,  "MAX": 1.6 },
    { "NAME": "headlineSize", "LABEL": "Headline Scale",   "TYPE": "float", "DEFAULT": 1.0,   "MIN": 0.5,   "MAX": 1.8 },
    { "NAME": "paletteMode",  "LABEL": "Palette",          "TYPE": "long",  "DEFAULT": 0,
      "VALUES": [0,1,2,3], "LABELS": ["Tetris Pop","Magenta Acid","Mono Lime","Sunset"] },
    { "NAME": "audioJitter",  "LABEL": "Pixel Jitter",     "TYPE": "float", "DEFAULT": 1.0,   "MIN": 0.0,   "MAX": 3.0, "BIND": "audio.bass" },
    { "NAME": "spriteCount",  "LABEL": "Sprite Density",   "TYPE": "long",  "DEFAULT": 26,    "VALUES": [8,14,20,26,34,44], "LABELS": ["8","14","20","26","34","44"] },
    { "NAME": "spriteScale",  "LABEL": "Sprite Scale",     "TYPE": "float", "DEFAULT": 1.0,   "MIN": 0.4,   "MAX": 2.4 },
    { "NAME": "energyA",      "LABEL": "Player A Push",    "TYPE": "float", "DEFAULT": 0.0,   "MIN": 0.0,   "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",      "LABEL": "Player B Push",    "TYPE": "float", "DEFAULT": 0.0,   "MIN": 0.0,   "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "energyC",      "LABEL": "Player C Push",    "TYPE": "float", "DEFAULT": 0.0,   "MIN": 0.0,   "MAX": 1.0, "BIND": "player[3].energy" },
    { "NAME": "parallaxDepth","LABEL": "Parallax Depth",   "TYPE": "float", "DEFAULT": 0.55,  "MIN": 0.0,   "MAX": 1.5 },
    { "NAME": "stripeFreq",   "LABEL": "Backdrop Stripes", "TYPE": "float", "DEFAULT": 18.0,  "MIN": 0.0,   "MAX": 60.0 },
    { "NAME": "typeWave",     "LABEL": "Typewriter Wave",  "TYPE": "float", "DEFAULT": 0.9,   "MIN": 0.0,   "MAX": 2.0 },
    { "NAME": "bgColor",      "LABEL": "Background",       "TYPE": "color", "DEFAULT": [0.02, 0.02, 0.03, 1.0] },
    { "NAME": "transparentBg","LABEL": "Transparent BG",   "TYPE": "bool",  "DEFAULT": 0.0 }
  ]
}*/

// =====================================================================
//  PIXEL TYPE TEXT — text-as-hero, pixel-quantized blocky type
//  with tetromino-sprite swarm and parallax pinstripe backdrop.
// =====================================================================

#define MAX_CHARS  48
#define SPACE_CH   26
#define MAX_SPRITES 44

// ─── font atlas helpers ──────────────────────────────────────────
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
    if (n < 0) return 0;
    if (n > MAX_CHARS) return MAX_CHARS;
    return n;
}

// ─── hash / noise ────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
vec2  hash21(float n) { return vec2(hash11(n), hash11(n + 17.31)); }
float hash12(vec2 p) {
    vec3 q = fract(vec3(p.xyx) * 0.1031);
    q += dot(q, q.yzx + 33.33);
    return fract((q.x + q.y) * q.z);
}

// ─── palette: one tint per pixel-block index ─────────────────────
vec3 paletteTetris(float t) {
    // Cycle: hot pink, lime green, cyan, yellow, white, magenta
    t = fract(t);
    if (t < 0.1667) return vec3(1.00, 0.18, 0.62);   // hot pink
    if (t < 0.3333) return vec3(0.22, 0.93, 0.30);   // lime
    if (t < 0.5000) return vec3(0.18, 0.78, 1.00);   // cyan
    if (t < 0.6667) return vec3(1.00, 0.92, 0.20);   // yellow
    if (t < 0.8333) return vec3(0.97, 0.97, 0.99);   // white
    return vec3(1.00, 0.42, 0.88);                   // magenta
}
vec3 paletteMagentaAcid(float t) {
    t = fract(t);
    if (t < 0.25) return vec3(1.00, 0.10, 0.55);
    if (t < 0.50) return vec3(0.85, 0.20, 1.00);
    if (t < 0.75) return vec3(0.20, 1.00, 0.85);
    return vec3(1.00, 0.95, 0.30);
}
vec3 paletteMonoLime(float t) {
    t = fract(t);
    float v = 0.35 + 0.65 * smoothstep(0.0, 1.0, t);
    return vec3(0.10, 0.95, 0.30) * v;
}
vec3 paletteSunset(float t) {
    t = fract(t);
    if (t < 0.20) return vec3(0.95, 0.25, 0.30);
    if (t < 0.40) return vec3(1.00, 0.55, 0.18);
    if (t < 0.60) return vec3(1.00, 0.85, 0.20);
    if (t < 0.80) return vec3(0.95, 0.40, 0.78);
    return vec3(0.45, 0.30, 0.92);
}
vec3 pickPalette(int mode, float t) {
    if (mode == 1) return paletteMagentaAcid(t);
    if (mode == 2) return paletteMonoLime(t);
    if (mode == 3) return paletteSunset(t);
    return paletteTetris(t);
}

// ─── tetromino sprite (5×5 pixel mask, 8 variants) ───────────────
// Each variant is a 5x5 stamp of 1/0 cells. We test (cx,cy) in [0..4].
float spriteMask(int variant, ivec2 c) {
    if (c.x < 0 || c.x > 4 || c.y < 0 || c.y > 4) return 0.0;
    // I, L, J, T, S, Z, O, plus a diamond
    if (variant == 0) {
        // I-piece — horizontal bar centered
        return (c.y == 2 && c.x >= 0 && c.x <= 3) ? 1.0 : 0.0;
    }
    if (variant == 1) {
        // L
        if (c.x == 1 && c.y >= 1 && c.y <= 3) return 1.0;
        if (c.y == 3 && c.x >= 1 && c.x <= 2) return 1.0;
        return 0.0;
    }
    if (variant == 2) {
        // J
        if (c.x == 2 && c.y >= 1 && c.y <= 3) return 1.0;
        if (c.y == 3 && c.x >= 1 && c.x <= 2) return 1.0;
        return 0.0;
    }
    if (variant == 3) {
        // T
        if (c.y == 2 && c.x >= 0 && c.x <= 2) return 1.0;
        if (c.x == 1 && c.y == 3) return 1.0;
        return 0.0;
    }
    if (variant == 4) {
        // S
        if (c.y == 2 && (c.x == 1 || c.x == 2)) return 1.0;
        if (c.y == 1 && (c.x == 2 || c.x == 3)) return 1.0;
        return 0.0;
    }
    if (variant == 5) {
        // Z
        if (c.y == 1 && (c.x == 1 || c.x == 2)) return 1.0;
        if (c.y == 2 && (c.x == 2 || c.x == 3)) return 1.0;
        return 0.0;
    }
    if (variant == 6) {
        // O — 2x2 square
        if (c.x >= 1 && c.x <= 2 && c.y >= 1 && c.y <= 2) return 1.0;
        return 0.0;
    }
    // diamond (plus)
    if (c.x == 2 && c.y == 1) return 1.0;
    if (c.y == 2 && c.x >= 1 && c.x <= 3) return 1.0;
    if (c.x == 2 && c.y == 3) return 1.0;
    return 0.0;
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;
    vec2 p = vec2((uv.x - 0.5) * aspect, uv.y - 0.5);

    float t  = TIME;
    float aB = clamp(audioBass, 0.0, 1.0);
    float aL = clamp(audioLevel, 0.0, 1.0);
    float aH = clamp(audioHigh, 0.0, 1.0);

    // Combined player push — each one nudges a different visual lane.
    float eA = clamp(energyA, 0.0, 1.0);
    float eB = clamp(energyB, 0.0, 1.0);
    float eC = clamp(energyC, 0.0, 1.0);
    float anyPlayer = max(eA, max(eB, eC));

    int total = charCount();
    bool liveUtt = msgAge >= 0.0;

    // ── BACKGROUND LAYER 0: pinstripe parallax wall ──
    // Two layers of vertical stripes at different parallax speeds give
    // a sense of a wall receding behind everything.
    vec3 col = bgColor.rgb;
    if (stripeFreq > 0.001) {
        float pDepth = clamp(parallaxDepth, 0.0, 1.5);
        float slow = (p.x + t * 0.012 * (1.0 + eA * 0.7)) * stripeFreq;
        float fast = (p.x + t * 0.032 * (1.0 + eB * 0.7)) * stripeFreq * 1.7;
        float s1 = 0.5 + 0.5 * sin(slow * 6.2832);
        float s2 = 0.5 + 0.5 * sin(fast * 6.2832);
        // narrow bright bands, mostly dark
        float band1 = smoothstep(0.88, 0.99, s1) * 0.18;
        float band2 = smoothstep(0.92, 1.00, s2) * 0.10;
        vec3 wallTint = pickPalette(int(paletteMode), 0.55 + 0.2 * sin(t * 0.13));
        col += wallTint * band1 * pDepth;
        col += vec3(0.85, 0.85, 0.95) * band2 * pDepth * 0.5;
    }

    // ── MID LAYER: tetromino sprite swarm ──
    // Each sprite is owned by one of three "lanes" → lane k is driven by
    // player[k+1].energy. Sprites drift on a slow flow and live in their
    // own pixel sub-grid so they read as discrete blocks, not blobs.
    int sCount = int(spriteCount);
    if (sCount > MAX_SPRITES) sCount = MAX_SPRITES;
    if (sCount < 1) sCount = 1;
    float sScale = clamp(spriteScale, 0.4, 2.4);

    vec3 spriteCol = vec3(0.0);
    float spriteAlpha = 0.0;

    for (int i = 0; i < MAX_SPRITES; i++) {
        if (i >= sCount) break;
        float fi = float(i);
        // Lane assignment 0,1,2 → A,B,C
        int lane = int(mod(fi, 3.0));
        float lanePush = (lane == 0) ? eA : (lane == 1) ? eB : eC;

        // Per-sprite seed
        vec2 s0 = hash21(fi * 11.7 + 3.13);
        vec2 s1 = hash21(fi * 19.3 + 7.71);

        // Home position on a coarse 6x4 grid plus jitter.
        float gx = 6.0;
        float gy = 4.0;
        int cx = int(mod(fi, gx));
        int cy = int(mod(floor(fi / gx), gy));
        vec2 home;
        home.x = -0.5 * (aspect - 0.10) + (float(cx) + 0.5) * (aspect - 0.10) / gx;
        home.y = -0.45 + (float(cy) + 0.5) * 0.90 / gy;
        home += (s0 - 0.5) * vec2(0.08, 0.05);

        // Drift on a slow flow field; the magnitude is lane-pushed.
        float speed = 0.10 + 0.45 * s1.x + 0.6 * lanePush;
        vec2 drift;
        drift.x = sin(t * speed + s0.x * 6.2832) * 0.06 * (1.0 + lanePush * 1.4);
        drift.y = cos(t * speed * 0.83 + s0.y * 6.2832) * 0.04 * (1.0 + lanePush);
        vec2 pos = home + drift;

        // Pseudo-3D depth: lane 0 = far, lane 1 = mid, lane 2 = near.
        // Far sprites are smaller, dimmer; near sprites bigger, brighter.
        float zT = float(lane) / 2.0;                 // 0..1, 0 far → 1 near
        float depthScale = mix(0.55, 1.20, zT);
        float depthDim   = mix(0.50, 1.00, zT);

        // Sprite size in world units, scaled by audio level.
        float baseSize = 0.045 * sScale * depthScale * (1.0 + 0.25 * aL);
        // Local UV (-0.5..0.5) inside the sprite stamp
        vec2 local = (p - pos) / baseSize;
        if (abs(local.x) > 0.5 || abs(local.y) > 0.5) continue;

        // Map into 5x5 cell coords
        vec2 cellF = (local + 0.5) * 5.0;
        ivec2 cc = ivec2(floor(cellF));
        int variant = int(floor(s1.y * 8.0));
        float mask = spriteMask(variant, cc);
        if (mask < 0.5) continue;

        // Color drawn from palette indexed by sprite seed.
        vec3 tint = pickPalette(int(paletteMode), s0.x + 0.07 * sin(t * 0.4 + fi));
        tint *= depthDim;
        // Pop on lane push — louder lanes light brighter.
        tint *= 0.7 + 0.6 * lanePush;

        // Painter's algorithm: nearer (higher zT) overwrites farther.
        // We approximate by always letting the most recent (later i)
        // win when it's nearer; track a simple z buffer via alpha boost.
        float w = mix(0.6, 1.0, zT);
        spriteCol = mix(spriteCol, tint, w);
        spriteAlpha = max(spriteAlpha, w);
    }

    col = mix(col, spriteCol, spriteAlpha);

    // ── FOREGROUND LAYER: HERO PIXEL TYPE ──
    // Compute how many chars to draw based on typewriter age.
    int shownTotal = total;
    float waveAmt = clamp(typeWave, 0.0, 2.0);
    if (liveUtt && total > 0) {
        // host typewriter speed ~28 cps; emulate so reveal looks granular
        float CPS = 28.0;
        float reveal = clamp(msgAge * CPS, 0.0, float(total));
        shownTotal = int(reveal);
        if (shownTotal < 1) shownTotal = 1;
        if (shownTotal > total) shownTotal = total;
    }

    if (total > 0 && shownTotal > 0) {
        // Layout: single line, scaled to fit canvas width.
        float hScale = clamp(headlineSize, 0.5, 1.8);
        float spacing = clamp(glyphSpacing, 0.75, 1.6);

        // Each glyph cell is character-width wide × char-height tall.
        // Solve for cell width so all `total` glyphs fit in ~85% of width.
        float maxWidth = (aspect * 0.92);
        // 5/7 aspect glyph; cell stride includes kerning
        float cellW = maxWidth / (float(total) * spacing);
        // also cap so a short message doesn't become huge
        float capH = 0.55 * hScale;
        float cellH = cellW * (7.0 / 5.0);
        if (cellH > capH) {
            cellH = capH;
            cellW = cellH * (5.0 / 7.0);
        }
        cellH *= hScale;
        cellW *= hScale;
        float stride = cellW * spacing;

        // Center the headline horizontally and vertically.
        float blockW = stride * float(total);
        float xStart = -blockW * 0.5;
        float yCenter = 0.0;

        // Audio jitter — shake the pixel grid horizontally.
        float jit = clamp(audioJitter, 0.0, 3.0);
        float jitterX = 0.012 * jit * aB * sin(t * 14.0 + p.y * 6.0);
        float jitterY = 0.004 * jit * aB * cos(t * 9.5);

        // Typewriter wave: each glyph bobs vertically based on its slot,
        // peaking just after it's revealed.
        // We loop the canvas instead of looping glyphs: figure out which
        // glyph slot this pixel is over.
        vec2 pq = p + vec2(jitterX, jitterY);
        float relX = pq.x - xStart;
        int slot = int(floor(relX / stride));
        if (slot >= 0 && slot < total && slot < shownTotal) {
            int ch = getChar(slot);
            if (ch >= 0 && ch <= 36 && ch != SPACE_CH) {
                // local coord inside the glyph cell
                float xInCell = relX - float(slot) * stride;
                // wave: cosine of distance from current reveal head
                float headPos = float(shownTotal) - 1.0;
                float dFromHead = headPos - float(slot);
                float wavePhase = clamp(dFromHead * 0.6 - t * 1.2, -3.14, 3.14);
                float yWave = waveAmt * 0.05 * exp(-0.6 * abs(dFromHead))
                            * sin(wavePhase * 2.0);
                // anyPlayer also nudges every glyph slightly
                yWave += 0.012 * anyPlayer * sin(t * 3.0 + float(slot) * 1.7);

                float yLocal = (pq.y - (yCenter + yWave)) + cellH * 0.5;

                if (xInCell >= 0.0 && xInCell <= cellW
                    && yLocal >= 0.0 && yLocal <= cellH) {
                    // Quantize sample point onto a coarse pixel grid so
                    // glyphs read blocky regardless of viewport size.
                    float px = max(40.0, pixelSize);
                    // pixel cell size in world units — derived from cell width
                    float pix = cellW / px * 28.0; // ~28 pixels across a glyph at default
                    if (pix < 1e-4) pix = 1e-4;
                    vec2 sampleP = vec2(
                        floor(xInCell / pix) * pix + pix * 0.5,
                        floor(yLocal / pix) * pix + pix * 0.5
                    );
                    // Sample the font atlas at the pixel center.
                    // sampleP.y is y-UP within the cell (yLocal=0 at
                    // bottom, =cellH at top). Host font atlas stores
                    // letter-top at v=1, so direct mapping puts
                    // letter-top at screen-top. The previous `1.0 -`
                    // form flipped glyphs upside down.
                    vec2 atlasUV = vec2(sampleP.x / cellW, sampleP.y / cellH);
                    float s = sampleChar(ch, atlasUV);
                    // hard threshold for blocky look (no smoothstep edge)
                    float on = step(0.35, s);

                    if (on > 0.5) {
                        // Per-pixel-block hue: stable index from quantized cell
                        ivec2 pcell = ivec2(floor(sampleP / pix));
                        float hueIdx = hash12(vec2(pcell) + float(slot) * 0.37);
                        // slight color boil with time + audio
                        hueIdx = fract(hueIdx + 0.10 * sin(t * 0.7 + float(slot)) + 0.15 * aH);
                        vec3 glyphCol = pickPalette(int(paletteMode), hueIdx);

                        // Pseudo-3D extrusion: a shadow block to lower-right
                        // gives chunky depth without parallax cost.
                        // We're already inside the glyph mask, so just lift
                        // brightness on "near" sub-pixels and darken edges.
                        float edgeT = min(min(sampleP.x, cellW - sampleP.x),
                                          min(sampleP.y, cellH - sampleP.y));
                        float edgeShade = smoothstep(0.0, cellW * 0.10, edgeT);
                        glyphCol *= mix(0.75, 1.10, edgeShade);

                        // Recently-revealed head glyph: brief bright flash
                        if (liveUtt) {
                            float headBoost = exp(-1.5 * max(dFromHead, 0.0));
                            glyphCol += vec3(1.0) * headBoost * 0.35;
                        }

                        col = glyphCol;
                    } else {
                        // inside the glyph cell but glyph is "off" → also
                        // draw a thin pixel-grid shadow block underneath
                        // (offset by 1 pixel right+down) to fake extrusion.
                        vec2 shadowP = sampleP - vec2(pix, pix);
                        if (shadowP.x >= 0.0 && shadowP.y >= 0.0
                            && shadowP.x <= cellW && shadowP.y <= cellH) {
                            // shadowP.y is y-UP (same frame as sampleP);
                            // map y-up→v directly so atlas letter-top
                            // lands at screen-top. The previous `1.0 -`
                            // form Y-flipped the shadow.
                            vec2 sUV = vec2(shadowP.x / cellW, shadowP.y / cellH);
                            float ss = sampleChar(ch, sUV);
                            float sOn = step(0.35, ss);
                            if (sOn > 0.5) {
                                // shadow color: dimmed palette
                                ivec2 spcell = ivec2(floor(shadowP / pix));
                                float shi = hash12(vec2(spcell) + float(slot) * 0.31);
                                vec3 sc = pickPalette(int(paletteMode), shi) * 0.22;
                                col = mix(col, sc, 0.85);
                            }
                        }
                    }
                }
            }
        }
    }

    // gentle global vignette so the wall feels lit, not flat
    float v = 1.0 - 0.35 * dot(p, p);
    col *= clamp(v, 0.55, 1.0);

    float alpha = 1.0;
    if (transparentBg) {
        // expose the foreground only — sprites + headline pixels.
        // crude: if col is close to bgColor, drop alpha.
        float d = length(col - bgColor.rgb);
        alpha = clamp(d * 4.0, 0.0, 1.0);
    }

    gl_FragColor = vec4(col, alpha);
}
