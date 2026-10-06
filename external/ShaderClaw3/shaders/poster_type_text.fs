/*{
  "DESCRIPTION": "Poster Type Text — editorial typographic poster. The `msg` is the hero: a huge two-line blocky headline locks the top band, a vertical micro-caption strip threads the right gutter, and a bottom rail breaks into corner notes. Between them sits an oval LENS — a parallax window into a multi-plane horizon (sky stratum, sun disc, far mountains, midground field, drifting foreground veil) that genuinely orbits as motion accumulates. Player channels drive distinct elements: A dollies the camera, B pulses the sun + chromatic separation, C twitches a registration-mark and shears the headline; bass lifts the sun radiance. Quiet reads as a still poster on warm paper; loud breaks it into a torn, jittering compositional event. No spectrum bars, no waveform — just type, depth, and intent.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": [
    "Generator",
    "Text",
    "A-List"
  ],
  "INPUTS": [
    {
      "NAME": "fidBloom",
      "LABEL": "Glow",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1.5
    },
    {
      "NAME": "fidDither",
      "LABEL": "Dither",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "fidGamma",
      "LABEL": "Gamma",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "fidEdgeGlow",
      "LABEL": "Edge Glow",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "fidVignette",
      "LABEL": "Vignette",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1.5
    },
    {
      "NAME": "fidGrain",
      "LABEL": "Grain",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "motionSpeed",
      "LABEL": "Motion Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionDrift",
      "LABEL": "Drift Speed",
      "TYPE": "float",
      "DEFAULT": 1.3,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionJitter",
      "LABEL": "Jitter",
      "TYPE": "float",
      "DEFAULT": 0.25,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionFlicker",
      "LABEL": "Flicker",
      "TYPE": "float",
      "DEFAULT": 0.15,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionSway",
      "LABEL": "Sway",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "motionChaos",
      "LABEL": "Chaos",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "palette",
      "LABEL": "Palette",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3,
        4
      ],
      "LABELS": [
        "Paper / Ink",
        "Cream / Indigo",
        "Mint / Oxblood",
        "Linen / Cobalt",
        "Onyx / Sulphur"
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "LABEL": "Hue Shift",
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "LABEL": "Color Boost",
      "GROUP": "Color"
    },
    {
      "NAME": "layoutVariant",
      "LABEL": "Layout",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Lens Center",
        "Lens Low",
        "Lens Wide",
        "Lens Off-Axis"
      ],
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "msg",
      "TYPE": "text",
      "DEFAULT": "POSTER TYPE COLLECTION VOL. 1",
      "MAX_LENGTH": 48,
      "BIND": "cue.latest",
      "GROUP": "Text",
      "LABEL": "Message"
    },
    {
      "NAME": "headlineSize",
      "LABEL": "Headline Size",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.5,
      "MAX": 1.8,
      "GROUP": "Text"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent BG",
      "TYPE": "bool",
      "DEFAULT": 0,
      "GROUP": "Background"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "LABEL": "Background",
      "GROUP": "Background"
    },
    {
      "NAME": "audioDepth",
      "LABEL": "Lens Depth (treble)",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 2,
      "BIND": "audio.high",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "energyA",
      "LABEL": "Player A — Dolly",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "BIND": "player[1].energy",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "energyB",
      "LABEL": "Player B — Sun",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "BIND": "player[2].energy",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "energyC",
      "LABEL": "Player C — Twitch",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "BIND": "player[3].energy",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "bassPunch",
      "LABEL": "Sun Pulse (bass)",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 2,
      "BIND": "audio.bass",
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ── FIDELITY KIT v2 (text-safe cinematic polish) ────────────────────
// Pure RGB math on the final color — no displacement, no chroma shift,
// glyph shapes never touched. Stages: edge glow (dFdx) → headroom bloom
// → vignette → animated grain → soft Reinhard tonemap → dither + sRGB.
vec3 fidApply(vec3 col, vec2 frag) {
    float l = dot(col, vec3(0.299, 0.587, 0.114));
    vec2  lg   = vec2(dFdx(l), dFdy(l));
    float edge = clamp(length(lg) * 7.0, 0.0, 1.0);
    col += col * edge * fidEdgeGlow * 1.50;
    float headroom = smoothstep(0.28, 0.95, l);
    col += col * headroom * fidBloom * 1.80;
    vec2  uvN = frag / RENDERSIZE - 0.5;
    float vig = 1.0 - dot(uvN, uvN) * 1.80 * fidVignette;
    col *= clamp(vig, 0.0, 1.0);
    float g = fract(sin(dot(frag + vec2(TIME * 73.0, TIME * 41.0),
                            vec2(12.9898, 78.233))) * 43758.5453);
    col += (g - 0.5) * fidGrain * 0.045;
    col = col / (1.0 + col * 0.18);
    float n = fract(sin(dot(frag, vec2(12.9898, 78.233))) * 43758.5453);
    col += (n - 0.5) * (1.0 / 255.0) * fidDither;
    col = mix(col, pow(max(col, 0.0), vec3(1.0 / 2.2)), fidGamma);
    return col;
}


// ─── MOTION KIT (shared across recent text shaders) ─────────────────
// Adds life on top of each shader's native animation: a breathing sway +
// continuous drift + coarse reseeding jitter on the working coord, plus a
// brightness flicker. Driven by the motion* uniforms. mkMotion() returns an
// offset to add to a coord; mkFlicker() a brightness multiplier.
float mkHash(vec2 p){ p = fract(p * vec2(127.1, 311.7)); p += dot(p, p + 34.5); return fract(p.x * p.y); }
vec2 mkMotion(vec2 q, float t){
    float ch = 0.4 + motionChaos;
    vec2 sway  = vec2(sin(t * 0.32 + q.y * 1.8), cos(t * 0.27 + q.x * 1.6)) * motionSway  * 0.09;
    vec2 drift = vec2(sin(t * 0.12 * ch), cos(t * 0.10 * ch))            * motionDrift * 0.05;
    // Smooth organic wander — layered incommensurate sines, slow + no stepping.
    float f = 1.0 + 1.2 * motionChaos;
    vec2 jit = vec2(
        sin(t * 0.70 * f + q.y * 3.1) * 0.6 + sin(t * 0.45 * f + q.x * 2.3 + 1.7) * 0.4,
        cos(t * 0.60 * f + q.x * 2.7) * 0.6 + cos(t * 0.50 * f + q.y * 2.9 + 4.2) * 0.4
    ) * motionJitter * 0.05;
    return sway + drift + jit;
}
float mkFlicker(vec2 q, float t){
    // Smooth, slow brightness undulation + soft scanline (no hard strobe).
    float n    = 0.5 + 0.5 * sin(t * 2.0 + q.x * 7.0 + q.y * 5.0);
    float scan = 0.5 + 0.5 * sin(q.y * 180.0 + t * 3.0);
    return 1.0 - motionFlicker * (0.5 * n + 0.30 * scan);
}


// ═══════════════════════════════════════════════════════════════════════
//  POSTER TYPE TEXT  ·  editorial poster · type-as-hero · lens parallax
//
//  Composition:
//   ┌────────────────────────────────────────────────────────┐
//   │  H E A D L I N E   line 1                            │
//   │  H E A D L I N E   line 2          ┃ side caption     │
//   │   ────────────────────────────     ┃                  │
//   │              ╭──── LENS ────╮      ┃                  │
//   │             (  sky / sun    )      ┃                  │
//   │              ╰─ mountains ──╯      ┃                  │
//   │                 (parallax)         ┃                  │
//   │   ───────────────────────────────────────────────────  │
//   │   bottom-left caption          ✚    bottom-right cap   │
//   └────────────────────────────────────────────────────────┘
//
//  Real depth lives in the lens: 5 parallax planes (sky stratum, sun
//  disc, far ridge, midground field, foreground grass-veil) each move
//  at distinct speeds from a virtual camera dollied by player[1].energy
//  + the mouse. Glyphs are NOT decoration — they are the subject. The
//  lens is not a literal landscape, it's a horizon as essence.
// ═══════════════════════════════════════════════════════════════════════

#define MAX_CHARS  48
#define SPACE_CH   26
#define MAX_LINE_CHARS 24

// ─── font atlas helpers ───────────────────────────────────────────────
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

// ─── hash / noise ─────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float hash12(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float a = hash12(i),                b = hash12(i+vec2(1,0));
    float c = hash12(i+vec2(0,1)),      d = hash12(i+vec2(1,1));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}
float fbm(vec2 p) {
    float v = 0.0, a = 0.55;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.07 + vec2(11.3, 5.7);
        a *= 0.5;
    }
    return v;
}

// ─── palettes (paper, ink, accent1, accent2) ──────────────────────────
void getPalette(int mode, out vec3 paper, out vec3 ink, out vec3 sky, out vec3 land, out vec3 accent) {
    if (mode == 1) {        // Cream / Indigo
        paper  = vec3(0.95, 0.92, 0.84);
        ink    = vec3(0.08, 0.10, 0.22);
        sky    = vec3(0.62, 0.70, 0.88);
        land   = vec3(0.20, 0.28, 0.50);
        accent = vec3(0.90, 0.45, 0.30);
    } else if (mode == 2) { // Mint / Oxblood
        paper  = vec3(0.88, 0.93, 0.86);
        ink    = vec3(0.30, 0.06, 0.08);
        sky    = vec3(0.55, 0.78, 0.70);
        land   = vec3(0.18, 0.40, 0.32);
        accent = vec3(0.85, 0.30, 0.20);
    } else if (mode == 3) { // Linen / Cobalt
        paper  = vec3(0.96, 0.93, 0.86);
        ink    = vec3(0.05, 0.12, 0.30);
        sky    = vec3(0.45, 0.62, 0.92);
        land   = vec3(0.10, 0.25, 0.55);
        accent = vec3(0.96, 0.62, 0.18);
    } else if (mode == 4) { // Onyx / Sulphur
        paper  = vec3(0.10, 0.10, 0.11);
        ink    = vec3(0.95, 0.92, 0.20);
        sky    = vec3(0.20, 0.20, 0.22);
        land   = vec3(0.45, 0.42, 0.10);
        accent = vec3(0.95, 0.30, 0.50);
    } else {                // Paper / Ink (default — closest to ref)
        paper  = vec3(0.94, 0.93, 0.90);
        ink    = vec3(0.04, 0.04, 0.05);
        sky    = vec3(0.42, 0.62, 0.90);
        land   = vec3(0.18, 0.42, 0.20);
        accent = vec3(0.90, 0.20, 0.18);
    }
}

// ─── headline word-wrap helpers ───────────────────────────────────────
// Walk the msg and find the break index that splits it into two roughly
// equal lines on word boundaries. Returns the start char of line 2 (the
// char AFTER the breaking space). If no space found, returns total/2.
int findLineBreak(int total) {
    if (total <= 1) return total;
    int target = total / 2;
    int bestBreak = target;
    int bestDist  = total;
    for (int i = 0; i < MAX_CHARS; i++) {
        if (i >= total) break;
        int ch = getChar(i);
        if (ch == SPACE_CH) {
            int dist = (i > target) ? (i - target) : (target - i);
            if (dist < bestDist) { bestDist = dist; bestBreak = i + 1; }
        }
    }
    return bestBreak;
}

// Count visible (non-leading-space) chars on a line, treating runs of
// SPACE_CH as a single space; used to compute glyph width that fills the
// headline band.
int countVisible(int from, int to) {
    if (to <= from) return 0;
    int n = 0;
    bool prevSpace = true;   // collapses leading spaces
    for (int i = 0; i < MAX_CHARS; i++) {
        int idx = from + i;
        if (idx >= to) break;
        int ch = getChar(idx);
        bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
        if (isSpace) {
            if (!prevSpace) n++;
            prevSpace = true;
        } else {
            n++;
            prevSpace = false;
        }
    }
    return n;
}

// Sample a single headline line. p is in normalized poster space, the
// line occupies y∈[y0,y1] (top→bottom) and x∈[xL,xR]. cells are laid out
// left-to-right; returns ink coverage (0..1) at p.
float headlineLine(vec2 p, float xL, float xR, float y0, float y1,
                   int from, int to, float shearX) {
    if (p.x < xL || p.x > xR) return 0.0;
    if (p.y > y0 || p.y < y1) return 0.0;   // y0 top, y1 bottom (y0>y1)
    int visible = countVisible(from, to);
    if (visible < 1) return 0.0;

    float w   = xR - xL;
    float cellW = w / float(visible);
    float h   = y0 - y1;
    // Glyph aspect is 5×7; size glyphs to fit BOTH dims and center.
    float glyphH = min(h * 0.92, cellW * (7.0/5.0));
    float glyphW = glyphH * (5.0/7.0);
    float xPad   = (cellW - glyphW) * 0.5;
    float yPad   = (h     - glyphH) * 0.5;

    // Apply line shear (headline jitters with player C).
    float shearedX = p.x + shearX * (p.y - y1);

    // Walk through chars and find which cell shearedX lands in.
    int col = int(floor((shearedX - xL) / cellW));
    if (col < 0 || col >= visible) return 0.0;

    // Map col → actual char index, skipping/collapsing spaces.
    int targetCh = -1;
    int seen = -1;
    bool prevSpace = true;
    for (int i = 0; i < MAX_CHARS; i++) {
        int idx = from + i;
        if (idx >= to) break;
        int ch = getChar(idx);
        bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
        if (isSpace) {
            if (!prevSpace) { seen++; if (seen == col) { targetCh = SPACE_CH; break; } }
            prevSpace = true;
        } else {
            seen++;
            if (seen == col) { targetCh = ch; break; }
            prevSpace = false;
        }
    }
    if (targetCh < 0 || targetCh == SPACE_CH) return 0.0;

    float lx = (shearedX - xL) - float(col) * cellW - xPad;
    float ly = (p.y - y1) - yPad;
    if (lx < 0.0 || lx > glyphW) return 0.0;
    if (ly < 0.0 || ly > glyphH) return 0.0;
    vec2 cellUV = vec2(lx / glyphW, ly / glyphH);
    float s = sampleChar(targetCh, cellUV);
    return smoothstep(0.18, 0.55, s);
}

// Micro-caption: tiny single-line strip of the message, with optional
// reverse (right-aligned) and char-offset (which slice of the msg).
float captionLine(vec2 p, float xL, float xR, float y0, float y1,
                  int offset, int charsToShow, bool rightAlign) {
    if (p.x < xL || p.x > xR) return 0.0;
    if (p.y > y0 || p.y < y1) return 0.0;
    int total = charCount();
    int from = offset;
    int to   = offset + charsToShow;
    if (to > total) to = total;
    if (from >= to) return 0.0;

    int visible = countVisible(from, to);
    if (visible < 1) return 0.0;
    float w     = xR - xL;
    float h     = y0 - y1;
    float glyphH = min(h * 0.78, w / float(visible) * (7.0/5.0));
    float glyphW = glyphH * (5.0/7.0);
    float cellW  = glyphW * 1.08;
    float blockW = cellW * float(visible);
    float startX = rightAlign ? (xR - blockW) : xL;
    if (p.x < startX || p.x > startX + blockW) return 0.0;

    int col = int(floor((p.x - startX) / cellW));
    if (col < 0 || col >= visible) return 0.0;

    int targetCh = -1;
    int seen = -1;
    bool prevSpace = true;
    for (int i = 0; i < MAX_CHARS; i++) {
        int idx = from + i;
        if (idx >= to) break;
        int ch = getChar(idx);
        bool isSpace = (ch == SPACE_CH || ch < 0 || ch > 36);
        if (isSpace) {
            if (!prevSpace) { seen++; if (seen == col) { targetCh = SPACE_CH; break; } }
            prevSpace = true;
        } else {
            seen++;
            if (seen == col) { targetCh = ch; break; }
            prevSpace = false;
        }
    }
    if (targetCh < 0 || targetCh == SPACE_CH) return 0.0;

    float lx = (p.x - startX) - float(col) * cellW;
    float ly = (p.y - y1) - (h - glyphH) * 0.5;
    if (lx < 0.0 || lx > glyphW) return 0.0;
    if (ly < 0.0 || ly > glyphH) return 0.0;
    float s = sampleChar(targetCh, vec2(lx / glyphW, ly / glyphH));
    return smoothstep(0.18, 0.55, s);
}

// ─── LENS: parallax horizon ───────────────────────────────────────────
// Returns the composited lens color at lens-local coord lp ∈ approx
// [-1,1]² (square space). 5 planes, distinct motion speeds.
vec3 renderLens(vec2 lp, float t, float dolly, float sunPulse, float chroma,
                vec3 sky, vec3 land, vec3 accent, vec3 paper) {
    // Camera pan: dolly shifts horizon X; mouse adds a window-into-world feel.
    vec2 m2 = (mousePos - 0.5) * 0.4;
    float camX = dolly * 1.2 + m2.x;
    float camY = sin(t * 0.07) * 0.06 + m2.y * 0.5;

    // Horizon Y inside the lens (slightly below center, drifts with camY)
    float horizon = -0.08 + camY * 0.5;

    // 1) Sky stratum: vertical gradient, slow parallax band of cirrus
    float skyT = smoothstep(horizon - 0.02, 1.2, lp.y);
    vec3 skyCol = mix(sky * 0.65, sky * 1.08, skyT);
    // Cirrus band, parallaxes at 0.15× camera
    float cir = fbm(vec2(lp.x * 2.4 - camX * 0.15 + t * 0.04, lp.y * 6.0));
    cir = smoothstep(0.55, 0.85, cir) * smoothstep(horizon, horizon + 0.45, lp.y);
    skyCol = mix(skyCol, paper * 1.05, cir * 0.45);

    // 2) Sun disc: bright lozenge near the horizon, parallaxes at 0.35×
    vec2 sunPos = vec2(0.18 - camX * 0.35, horizon + 0.10);
    float sunR  = 0.22 + 0.32 * sunPulse;
    float sunD  = length((lp - sunPos) / vec2(1.0, 0.85));
    float sun   = smoothstep(sunR, sunR * 0.55, sunD);
    float halo  = smoothstep(sunR * 2.4, sunR * 0.8, sunD) * 0.45;
    // Chromatic separation on the halo when player B is hot
    vec3 sunCol = mix(accent, vec3(1.0, 0.97, 0.85), 0.55);
    skyCol = mix(skyCol, sunCol, sun);
    skyCol += halo * sunCol * (0.6 + 1.3 * sunPulse);
    // Chroma fringe on halo edge
    if (chroma > 0.001) {
        float fringe = smoothstep(sunR * 1.4, sunR * 1.05, sunD)
                     - smoothstep(sunR * 1.05, sunR * 0.85, sunD);
        skyCol.r += fringe * chroma * 0.18;
        skyCol.b -= fringe * chroma * 0.12;
    }

    // 3) Far ridge: silhouetted mountain line, parallaxes at 0.55×
    float ridgeY = horizon - 0.04
                 + 0.06 * sin(lp.x * 3.3 - camX * 0.55 + t * 0.02)
                 + 0.03 * sin(lp.x * 8.1 - camX * 0.55 * 1.4);
    float ridgeMask = smoothstep(ridgeY + 0.01, ridgeY - 0.01, lp.y);
    vec3 ridgeCol = mix(land * 0.55, land * 0.78, smoothstep(horizon - 0.15, ridgeY, lp.y));

    // 4) Midground field: striped rows that converge to horizon — fake
    //    perspective gives real depth. Parallax 0.85×.
    float groundT = smoothstep(horizon, horizon - 0.55, lp.y);
    // Perspective row count rises near horizon
    float persp = 1.0 / max(horizon - lp.y, 0.02);
    float rows = sin(persp * 1.6 + camX * 0.85 * persp * 0.3 + t * 0.05 * persp);
    rows = smoothstep(0.0, 0.6, rows);
    vec3 fieldCol = mix(land * 0.85, land * 1.15, rows);
    fieldCol = mix(land * 0.6, fieldCol, groundT);

    // 5) Foreground grass-veil: short vertical strokes near bottom, parallax 1.4×
    float grassY = smoothstep(-1.05, horizon - 0.35, lp.y);
    grassY = 1.0 - grassY;
    float blade = fbm(vec2(lp.x * 28.0 - camX * 1.4, lp.y * 8.0 + t * 0.15));
    blade = smoothstep(0.5, 0.75, blade);
    vec3 grassCol = mix(land * 0.5, land * 0.9, blade);

    // Composite back→front: sky, sun (in sky), ridge, field, grass
    vec3 col = skyCol;
    col = mix(col, ridgeCol, ridgeMask);
    float groundMask = smoothstep(ridgeY, ridgeY - 0.03, lp.y);
    col = mix(col, fieldCol, groundMask);
    col = mix(col, grassCol, grassY * groundMask * 0.55);

    // Atmospheric haze toward horizon — fakes air perspective
    float haze = smoothstep(horizon - 0.30, horizon + 0.10, lp.y) * 0.0
               + smoothstep(horizon + 0.10, horizon - 0.10, lp.y) * 0.25;
    col = mix(col, sky * 1.05, haze);

    return col;
}

// ─── registration mark (the ✚ that twitches with player C) ─────────────
float regMark(vec2 p, vec2 center, float size, float thick) {
    vec2 d = abs(p - center);
    float h = step(d.y, thick) * step(d.x, size);   // horizontal bar
    float v = step(d.x, thick) * step(d.y, size);   // vertical bar
    return max(h, v);
}

// ──────────────────────────────────────────────────────────────────────
void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    uv += mkMotion(uv, TIME);          // [0,1]²
    float aspect = res.x / res.y;

    // Poster space: y∈[0,1] bottom→top, x∈[0,1] left→right.
    // We design at portrait-ish aspect; if the window is landscape we
    // letterbox the poster horizontally so layout stays correct.
    vec2 p = uv;

    // Time + audio
    float t      = TIME * motionSpeed;
    float eA     = clamp(energyA, 0.0, 1.0);
    float eB     = clamp(energyB, 0.0, 1.0);
    float eC     = clamp(energyC, 0.0, 1.0);
    float bass   = clamp(audioBass * bassPunch, 0.0, 2.0);
    float treble = clamp(audioHigh * audioDepth, 0.0, 2.0);

    // Palette
    vec3 paper, ink, sky, land, accent;
    getPalette(int(palette), paper, ink, sky, land, accent);

    // ── Background paper (warm tooth, micro-noise) ──
    float tooth = fbm(uv * vec2(res.x, res.y) * 0.008) * 0.5
                + fbm(uv * vec2(res.x, res.y) * 0.04 + 17.0) * 0.5;
    vec3 col = paper * (0.96 + (tooth - 0.5) * 0.06);
    // universal background override — tint the paper base (a=0 -> untouched)
    col = mix(col, bgColor.rgb, bgColor.a);
    // Subtle vignette
    vec2 cv = uv - 0.5;
    col *= 1.0 - dot(cv, cv) * 0.22;

    // ── Layout regions (vary by layoutVariant) ──
    // Headline band: top ~28% of canvas
    float hBandTop = 0.96;
    float hBandBot = 0.66;
    float headlineHi = 0.97;
    float headlineLo = 0.66;
    // Caption rail bottom
    float footTop = 0.10;
    float footBot = 0.02;
    // Lens rect (square in normalized space; will be circle-masked)
    vec2 lensCenter = vec2(0.46, 0.40);
    float lensR = 0.30;
    int variant = int(layoutVariant);
    if (variant == 1) {           // Lens Low
        lensCenter = vec2(0.50, 0.30);
        lensR = 0.28;
    } else if (variant == 2) {    // Lens Wide
        lensCenter = vec2(0.50, 0.40);
        lensR = 0.36;
    } else if (variant == 3) {    // Lens Off-Axis
        lensCenter = vec2(0.36, 0.42);
        lensR = 0.28;
    }

    // ── Editorial rules (thin ink lines) ──
    // Top rule under the headline
    float rule1 = smoothstep(0.0023, 0.0, abs(p.y - 0.635));
    // Bottom rule above the foot
    float rule2 = smoothstep(0.0023, 0.0, abs(p.y - 0.115));
    // Right vertical rail
    float railX = 0.92;
    float rule3 = smoothstep(0.0025, 0.0, abs(p.x - railX))
                * step(p.y, 0.62) * step(0.115, p.y);
    float rules = max(max(rule1, rule2), rule3);
    col = mix(col, ink, rules * 0.85);

    // ── LENS ───────────────────────────────────────────────
    // Render the lens scene first into a buffer-like local, then circle-mask.
    vec2 lp = (p - lensCenter) / lensR;       // lens-local, ~[-1,1]²
    // Square out so the world doesn't squish on landscape
    lp.x *= aspect * 0.75;
    float lensD = length(lp);
    float lensMask = 1.0 - smoothstep(0.97, 1.02, lensD);
    if (lensMask > 0.001) {
        float dolly = eA * 1.5 + sin(t * 0.13) * 0.18 * (0.4 + eA);
        float sunP  = bass * 1.8 + eB * 0.9;
        float chromaSep = eB * (0.6 + 0.4 * treble) + treble * 0.4;
        vec3 lensCol = renderLens(lp, t, dolly, sunP, chromaSep,
                                  sky, land, accent, paper);
        // Soft inner shadow + paper-mix at edge so the lens reads as a
        // window cut into the paper (not just a circle pasted on top).
        float edgeShade = smoothstep(0.95, 0.80, lensD);
        lensCol *= mix(0.78, 1.0, edgeShade);
        col = mix(col, lensCol, lensMask);
        // Lens stroke (thin ink ring)
        float ring = smoothstep(0.012, 0.0, abs(lensD - 1.0));
        col = mix(col, ink, ring * 0.9);
        // Tiny ®/index marks just inside the lens — editorial flourish.
        // They appear as small ticks at ~30° and ~330°.
        for (int k = 0; k < 2; k++) {
            float ang = (k == 0) ? 0.52 : -0.52;
            vec2 tickP = vec2(cos(ang), sin(ang)) * 0.92;
            float td = length(lp - tickP);
            float tick = smoothstep(0.04, 0.02, td);
            col = mix(col, ink, tick * 0.6);
        }
    }

    // ── HEADLINE (two-line, breaks on word) ──
    int total = charCount();
    if (total > 0) {
        // Typewriter reveal: if msgAge ≥ 0, only chars 0..reveal are visible.
        // Treat the headline as the primary glyph wall; chars past `reveal`
        // are simply absent (no fade — matches the editorial drop-in feel).
        bool live = (msgAge >= 0.0);
        int reveal = total;
        if (live) {
            float cps = 22.0;                    // chars per second
            reveal = int(floor(msgAge * cps + 0.001));
            if (reveal < 0) reveal = 0;
            if (reveal > total) reveal = total;
        }

        int brk = findLineBreak(reveal);
        // Clamp brk inside [0,reveal]
        if (brk > reveal) brk = reveal;
        if (brk < 0) brk = 0;

        // Headline band: 2 lines, each ~14% of canvas tall
        float lineH = 0.14 * headlineSize;
        float topY  = headlineHi;
        float midY  = topY - lineH;
        float botY  = midY - lineH;
        // Clamp so headline never overlaps the lens rule
        if (botY < 0.66) botY = 0.66;

        // Headline shear: tiny tilt that twitches with player C
        float shear1 = (eC - 0.3) * 0.020 * sin(t * 6.2 + 1.7);
        float shear2 = (eC - 0.3) * 0.020 * sin(t * 5.4 + 4.1);

        float h1 = headlineLine(p, 0.04, 0.96, topY, midY, 0,   brk,    shear1);
        float h2 = headlineLine(p, 0.04, 0.96, midY, botY, brk, reveal, shear2);
        float headlineMask = max(h1, h2);

        // Subtle chromatic offset on headline when player B is hot
        if (eB > 0.02) {
            float off = 0.0025 * eB;
            float h1r = headlineLine(p + vec2(off, 0.0), 0.04, 0.96, topY, midY, 0,   brk,    shear1);
            float h2r = headlineLine(p + vec2(off, 0.0), 0.04, 0.96, midY, botY, brk, reveal, shear2);
            float h1b = headlineLine(p - vec2(off, 0.0), 0.04, 0.96, topY, midY, 0,   brk,    shear1);
            float h2b = headlineLine(p - vec2(off, 0.0), 0.04, 0.96, midY, botY, brk, reveal, shear2);
            col.r = mix(col.r, ink.r, max(h1r, h2r));
            col.b = mix(col.b, ink.b, max(h1b, h2b));
        }
        col = mix(col, ink, headlineMask);

        // ── Foot captions ──
        // Bottom-left: first half of the msg, micro-sized
        int footMid = (total + 1) / 2;
        float fL = captionLine(p, 0.04, 0.46, footTop, footBot, 0, footMid, false);
        float fR = captionLine(p, 0.50, 0.92, footTop, footBot, footMid, total - footMid, true);
        col = mix(col, ink, max(fL, fR));

        // ── Right-rail vertical micro-caption (rotated layout cheat:
        //    we render it as a tall narrow strip with chars stacked top-down).
        // Implementation: same caption code but in a thin column running
        // down the right side; chars stacked one per row by treating each
        // character as a small block whose width = rail width.
        if (variant != 3) {  // off-axis variant suppresses the rail to give breathing room
            float railL = 0.935, railR = 0.985;
            float colTop = 0.62, colBot = 0.16;
            if (p.x > railL && p.x < railR && p.y > colBot && p.y < colTop) {
                int n = total;
                if (n > 16) n = 16;
                float colH  = colTop - colBot;
                float cellH = colH / max(float(n), 1.0);
                // Each char's vertical slot
                int row = int(floor((colTop - p.y) / cellH));
                if (row >= 0 && row < n) {
                    int ch = getChar(row);
                    if (ch >= 0 && ch <= 35 && ch != SPACE_CH) {
                        float yInSlot = (colTop - p.y) - float(row) * cellH;
                        float xInRail = p.x - railL;
                        float railW   = railR - railL;
                        float glyphH  = min(cellH * 0.78, railW * (7.0/5.0));
                        float glyphW  = glyphH * (5.0/7.0);
                        float xPad    = (railW - glyphW) * 0.5;
                        float yPad    = (cellH - glyphH) * 0.5;
                        float lx = xInRail - xPad;
                        float ly = yInSlot - yPad;
                        if (lx >= 0.0 && lx <= glyphW && ly >= 0.0 && ly <= glyphH) {
                            float s = sampleChar(ch, vec2(lx / glyphW, ly / glyphH));
                            s = smoothstep(0.18, 0.55, s);
                            col = mix(col, ink, s * 0.95);
                        }
                    }
                }
            }
        }
    }

    // ── Registration mark (small ✚ that twitches with player C) ──
    {
        // Off-axis position; jitter with eC
        vec2 regC = vec2(0.92, 0.94);
        regC += vec2(sin(t * 17.0), cos(t * 13.0)) * 0.004 * eC;
        float rm = regMark(p, regC, 0.012, 0.0015);
        col = mix(col, accent, rm);
        // Crosshair circle
        float ringD = length(p - regC);
        float regRing = smoothstep(0.0018, 0.0, abs(ringD - 0.014));
        col = mix(col, accent, regRing * 0.85);
    }

    // ── Color rail (thin vertical accent strip on the far edge) ──
    {
        float rx = (variant == 3) ? 0.025 : 0.995;
        // For variant 3 (off-axis), accent rides the LEFT edge
        float dRail = abs(p.x - rx);
        float railMask = smoothstep(0.020, 0.000, dRail) * step(p.y, 0.62) * step(0.115, p.y);
        // Rail itself has its own parallax tone-shift driven by treble
        float railT = fbm(vec2(p.y * 8.0 - t * 0.4 * (0.4 + treble), 3.1));
        vec3 railCol = mix(accent, mix(accent, ink, 0.4), railT);
        col = mix(col, railCol, railMask * 0.85);
    }

    // ── Global grain (paper fiber, not pixel) — energy-aware ──
    float grain = vnoise(gl_FragCoord.xy * 0.7 + t * 13.0) - 0.5;
    col += grain * (0.020 + 0.025 * (eA + eB + eC));

    // ── Output ──
    float alpha = 1.0;
    if (transparentBg) {
        // Transparent BG means: paper drops, everything else (rules, headline,
        // lens, caption, rail, regmark) remains. We rebuild col as ink/accent
        // composited onto zero — but lens fill is also kept opaque.
        // Approximate: alpha from luminance distance to paper.
        float dist = length(col - paper);
        alpha = clamp(dist * 1.8, 0.0, 1.0);
    }

    col *= mkFlicker(gl_FragCoord.xy / RENDERSIZE - 0.5, TIME);

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = fidApply(col, gl_FragCoord.xy);
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);
    if (hueShift > 0.0005) {
        float hueA = hueShift * 6.2831853;
        float hueC = cos(hueA), hueS = sin(hueA);
        mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                  + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                  + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hueM * uc, 0.0, 1.0);
    }
    gl_FragColor = vec4(uc, alpha);
}
