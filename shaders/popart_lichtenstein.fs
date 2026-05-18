/*{
  "CATEGORIES": ["Generator", "Art Movement", "Audio Reactive"],
  "DESCRIPTION": "Pop Art after Roy Lichtenstein — Whaam! (1963), Drowning Girl (1963), Crying Girl (1963), M-Maybe (1965), plus a clean Bauhaus/Warhol primary-field mood. The signature element is the BEN-DAY DOT — large evenly-spaced halftone dots in cyan or red printed across flat fields of primary colour. Hard black outlines trace every shape boundary. A rectangular speech bubble with rounded corners and a pointed tail cycles through comic onomatopoeia — WHAAM!, POW!, BLAM!, OH..., IT'S OVER! — drawn as block-letter rectangle SDFs. Five mood panels rotate on TIME: Whaam explosion, Drowning Girl wave, Crying Girl tear, radiating Sunrise rays, Bauhaus primary-color fields. Panel transitions are smooth 1.5s cross-fades — no wipes, no scanlines. Bass triggers explosion flashes, treble shimmers the dot field. Five-colour palette only: yellow, red, cyan, white, black. Returns LINEAR HDR — host applies ACES.",
  "INPUTS": [
    { "NAME": "moodOverride", "LABEL": "Mood",          "TYPE": "long",  "DEFAULT": -1, "VALUES": [-1, 0, 1, 2, 3, 4], "LABELS": ["Auto Cycle", "Whaam!", "Drowning Girl", "Crying Girl", "Sunrise", "Bauhaus"] },
    { "NAME": "dotDensity",   "LABEL": "Ben-Day Density","TYPE": "float", "MIN": 30.0, "MAX": 160.0, "DEFAULT": 78.0 },
    { "NAME": "dotRadius",    "LABEL": "Dot Radius",     "TYPE": "float", "MIN": 0.20, "MAX": 0.48, "DEFAULT": 0.36 },
    { "NAME": "outlineWeight","LABEL": "Outline Weight", "TYPE": "float", "MIN": 0.001,"MAX": 0.010,"DEFAULT": 0.0042 },
    { "NAME": "speechBubble", "LABEL": "Speech Bubble",  "TYPE": "bool",  "DEFAULT": false },
    { "NAME": "panelDuration","LABEL": "Panel Seconds",  "TYPE": "float", "MIN": 4.0,  "MAX": 20.0, "DEFAULT": 9.0 },
    { "NAME": "audioReact",   "LABEL": "Audio React",    "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0 }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  POP ART — Roy Lichtenstein, 1963–1965
//
//  Curator's note: previous iteration tried to be a Pop-Art swiss-army
//  knife (Lichtenstein + Warhol + Rosenquist all in one) and ended up
//  reading like none of them. Strip it. Lichtenstein is precisely four
//  things: BEN-DAY DOTS, hard black outlines, primary-colour fields,
//  and a comic-book speech bubble. Build only those, in five colours,
//  and rotate four iconic subjects on a timer. The shader earns its
//  star by being unmistakably, only, Lichtenstein.
// ════════════════════════════════════════════════════════════════════════

// ─── PALETTE — five colours, hard-coded, no mixing into mud ───────────
const vec3 LL_YELLOW = vec3(0.98, 0.85, 0.10);
const vec3 LL_RED    = vec3(0.92, 0.18, 0.16);
const vec3 LL_CYAN   = vec3(0.10, 0.55, 0.82);
const vec3 LL_WHITE  = vec3(0.96, 0.94, 0.88);
const vec3 LL_BLACK  = vec3(0.04, 0.04, 0.06);

// ─── SDF helpers ──────────────────────────────────────────────────────
float sdCircle(vec2 p, float r) { return length(p) - r; }
float sdRect  (vec2 p, vec2 b)  { vec2 d = abs(p) - b; return max(d.x, d.y); }
float sdRoundRect(vec2 p, vec2 b, float r) {
    vec2 d = abs(p) - b + vec2(r);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - r;
}
float sdSegment(vec2 p, vec2 a, vec2 b, float w) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h) - w;
}

// ─── Ben-Day halftone field ───────────────────────────────────────────
// Hex-ish offset row pattern. Density is the number of dot cells across
// the canvas width. radius is dot fill in [0, 0.5] of cell size.
float benDay(vec2 uv, float density, float radius, float aspect, float jitter) {
    vec2 g = vec2(uv.x * aspect, uv.y) * density;
    // Offset every other row by half a cell — proper printed halftone.
    float row = floor(g.y);
    g.x += 0.5 * mod(row, 2.0);
    vec2 cell = fract(g) - 0.5;
    // Treble shimmer — radius wobbles per cell on the high band.
    float r = radius + jitter * 0.05 * sin(row * 1.91 + g.x * 3.0);
    return smoothstep(r, r - 0.04, length(cell));
}

// ─── BLOCK-LETTER GLYPHS — solid rectangles ───────────────────────────
// Drawn in unit square 0..1; each function returns 0/1 ink mask.
float bar(vec2 p, vec4 r) {
    return step(r.x, p.x) * step(p.x, r.z)
         * step(r.y, p.y) * step(p.y, r.w);
}
float gW(vec2 p) {
    float L = bar(p, vec4(0.00, 0.05, 0.20, 0.95));
    float R = bar(p, vec4(0.80, 0.05, 1.00, 0.95));
    float ML= bar(p, vec4(0.32, 0.05, 0.48, 0.55));
    float MR= bar(p, vec4(0.52, 0.05, 0.68, 0.55));
    return max(max(L, R), max(ML, MR));
}
float gH(vec2 p) {
    float L = bar(p, vec4(0.00, 0.05, 0.20, 0.95));
    float R = bar(p, vec4(0.80, 0.05, 1.00, 0.95));
    float M = bar(p, vec4(0.20, 0.42, 0.80, 0.58));
    return max(max(L, R), M);
}
float gA(vec2 p) {
    float L = bar(p, vec4(0.00, 0.05, 0.20, 0.85));
    float R = bar(p, vec4(0.80, 0.05, 1.00, 0.85));
    float T = bar(p, vec4(0.05, 0.85, 0.95, 1.00));
    float M = bar(p, vec4(0.20, 0.42, 0.80, 0.55));
    return max(max(L, R), max(T, M));
}
float gM(vec2 p) {
    float L = bar(p, vec4(0.00, 0.05, 0.20, 1.00));
    float R = bar(p, vec4(0.80, 0.05, 1.00, 1.00));
    float ML= bar(p, vec4(0.20, 0.55, 0.40, 0.95));
    float MR= bar(p, vec4(0.60, 0.55, 0.80, 0.95));
    float MM= bar(p, vec4(0.40, 0.45, 0.60, 0.75));
    return max(max(L, R), max(max(ML, MR), MM));
}
float gP(vec2 p) {
    float L = bar(p, vec4(0.00, 0.05, 0.20, 1.00));
    float T = bar(p, vec4(0.20, 0.85, 0.85, 1.00));
    float B = bar(p, vec4(0.20, 0.50, 0.85, 0.65));
    float R = bar(p, vec4(0.80, 0.50, 1.00, 1.00));
    return max(max(L, T), max(B, R));
}
float gO(vec2 p) {
    float OUT = bar(p, vec4(0.00, 0.05, 1.00, 0.95));
    float IN  = bar(p, vec4(0.22, 0.25, 0.78, 0.75));
    return OUT * (1.0 - IN);
}
float gB(vec2 p) {
    float L = bar(p, vec4(0.00, 0.05, 0.20, 1.00));
    float T = bar(p, vec4(0.20, 0.85, 0.82, 1.00));
    float M = bar(p, vec4(0.20, 0.45, 0.82, 0.58));
    float Bt= bar(p, vec4(0.20, 0.05, 0.82, 0.20));
    float RT= bar(p, vec4(0.82, 0.55, 1.00, 0.92));
    float RB= bar(p, vec4(0.82, 0.13, 1.00, 0.50));
    return max(max(max(L, T), max(M, Bt)), max(RT, RB));
}
float gL(vec2 p) {
    float V = bar(p, vec4(0.00, 0.05, 0.20, 1.00));
    float H = bar(p, vec4(0.20, 0.05, 0.95, 0.20));
    return max(V, H);
}
float gI(vec2 p) {
    return bar(p, vec4(0.40, 0.05, 0.60, 1.00));
}
float gT(vec2 p) {
    float Tb= bar(p, vec4(0.00, 0.85, 1.00, 1.00));
    float V = bar(p, vec4(0.40, 0.05, 0.60, 0.85));
    return max(Tb, V);
}
float gS(vec2 p) {
    float T = bar(p, vec4(0.00, 0.85, 1.00, 1.00));
    float TL= bar(p, vec4(0.00, 0.55, 0.20, 0.85));
    float M = bar(p, vec4(0.00, 0.42, 1.00, 0.58));
    float BR= bar(p, vec4(0.80, 0.20, 1.00, 0.45));
    float B = bar(p, vec4(0.00, 0.05, 1.00, 0.20));
    return max(max(max(T, TL), M), max(BR, B));
}
float gE(vec2 p) {
    float V = bar(p, vec4(0.00, 0.05, 0.20, 1.00));
    float T = bar(p, vec4(0.20, 0.85, 1.00, 1.00));
    float M = bar(p, vec4(0.20, 0.42, 0.85, 0.58));
    float B = bar(p, vec4(0.20, 0.05, 1.00, 0.20));
    return max(max(V, T), max(M, B));
}
float gR(vec2 p) {
    float L = bar(p, vec4(0.00, 0.05, 0.20, 1.00));
    float T = bar(p, vec4(0.20, 0.85, 0.82, 1.00));
    float M = bar(p, vec4(0.20, 0.45, 0.82, 0.58));
    float RT= bar(p, vec4(0.82, 0.50, 1.00, 0.95));
    float LegL= bar(p, vec4(0.30, 0.05, 0.55, 0.45));
    float LegR= bar(p, vec4(0.70, 0.05, 1.00, 0.45));
    return max(max(max(L, T), max(M, RT)), max(LegL, LegR));
}
float gV(vec2 p) {
    float L = bar(p, vec4(0.00, 0.40, 0.20, 1.00));
    float R = bar(p, vec4(0.80, 0.40, 1.00, 1.00));
    float ML= bar(p, vec4(0.20, 0.05, 0.45, 0.45));
    float MR= bar(p, vec4(0.55, 0.05, 0.80, 0.45));
    return max(max(L, R), max(ML, MR));
}
float gExcl(vec2 p) {
    float V = bar(p, vec4(0.40, 0.30, 0.60, 1.00));
    float D = bar(p, vec4(0.40, 0.00, 0.60, 0.18));
    return max(V, D);
}
float gDot(vec2 p) {
    return bar(p, vec4(0.40, 0.00, 0.60, 0.18));
}

// Render one glyph by index
float renderGlyph(int g, vec2 p) {
    if (p.x < 0.0 || p.x > 1.0 || p.y < 0.0 || p.y > 1.0) return 0.0;
    if (g == 0)  return gW(p);
    if (g == 1)  return gH(p);
    if (g == 2)  return gA(p);
    if (g == 3)  return gM(p);
    if (g == 4)  return gP(p);
    if (g == 5)  return gO(p);
    if (g == 6)  return gB(p);
    if (g == 7)  return gL(p);
    if (g == 8)  return gI(p);
    if (g == 9)  return gT(p);
    if (g == 10) return gS(p);
    if (g == 11) return gE(p);
    if (g == 12) return gR(p);
    if (g == 13) return gV(p);
    if (g == 14) return gExcl(p);
    if (g == 15) return gDot(p);
    return 0.0;
}

// Word strings — fetch glyph index for slot in word
//   0: WHAAM!     (6 glyphs)
//   1: POW!       (4)
//   2: BLAM!      (5)
//   3: OH...      (5)
//   4: IT'S OVER! (9)
int wordGlyph(int word, int i) {
    if (word == 0) {
        if (i == 0) return 0;  // W
        if (i == 1) return 1;  // H
        if (i == 2) return 2;  // A
        if (i == 3) return 2;  // A
        if (i == 4) return 3;  // M
        return 14;             // !
    }
    if (word == 1) {
        if (i == 0) return 4;  // P
        if (i == 1) return 5;  // O
        if (i == 2) return 0;  // W
        return 14;             // !
    }
    if (word == 2) {
        if (i == 0) return 6;  // B
        if (i == 1) return 7;  // L
        if (i == 2) return 2;  // A
        if (i == 3) return 3;  // M
        return 14;             // !
    }
    if (word == 3) {
        if (i == 0) return 5;  // O
        if (i == 1) return 1;  // H
        return 15;             // .
    }
    // word 4 — IT'S OVER!
    if (i == 0) return 8;      // I
    if (i == 1) return 9;      // T
    if (i == 2) return 10;     // S
    if (i == 3) return 5;      // O
    if (i == 4) return 13;     // V
    if (i == 5) return 11;     // E
    if (i == 6) return 12;     // R
    return 14;                 // !
}
int wordLength(int word) {
    if (word == 0) return 6;   // WHAAM!
    if (word == 1) return 4;   // POW!
    if (word == 2) return 5;   // BLAM!
    if (word == 3) return 5;   // OH... (O H . . .) — render only 3 cells
    return 8;                  // IT'S OVER! ignoring apostrophe
}

// Draw a word centered on `origin` in normalized canvas, fitting `width`,
// with glyph height `h`. Returns ink mask 0/1.
float drawWord(vec2 uv, vec2 origin, float width, float h, float aspect, int word) {
    int n = wordLength(word);
    if (n <= 0) return 0.0;
    float cellW = width / float(n);
    vec2 d = uv - origin;
    d.x *= aspect;
    // Letter cell index
    float fx = d.x / cellW + float(n) * 0.5;
    int idx = int(floor(fx));
    if (idx < 0 || idx >= n) return 0.0;
    vec2 lp = vec2(fract(fx), (d.y + h * 0.5) / h);
    int g = wordGlyph(word, idx);
    return renderGlyph(g, lp);
}

// ─── SUBJECT FIELDS — four mood panels ────────────────────────────────
// Each returns: base color, an outline mask (1 where black ink should go),
// and a dot-region mask that selects WHERE Ben-Day dots should appear.
struct Subject { vec3 base; float outline; float dotRegion; vec3 dotColor; };

// Whaam! — yellow/red explosion silhouette over cyan sky
Subject subjWhaam(vec2 uv, float aspect, float t, float bass) {
    Subject S;
    vec2 p = (uv - vec2(0.55, 0.50));
    p.x *= aspect;
    float ang = atan(p.y, p.x);
    float r = length(p);
    // Spiky burst silhouette
    float burst = 0.30 + 0.06 * cos(ang * 9.0)
                + 0.02 * cos(ang * 17.0 + t * 0.8)
                + 0.04 * bass;
    float inBurst = step(r, burst);
    float inCore  = step(r, burst * 0.55);
    // Background cyan sky with tilted plane horizon
    float horizon = step(uv.y, 0.32 + 0.04 * sin(uv.x * 6.0));
    S.base = mix(LL_CYAN, LL_RED, horizon * 0.4);
    S.base = mix(S.base, LL_YELLOW, inBurst);
    S.base = mix(S.base, LL_RED,    inCore);
    // Black outline of burst rim
    float rim = abs(r - burst);
    S.outline = step(rim, 0.012) * (1.0 - inCore);
    // Add inner core rim
    S.outline = max(S.outline, step(abs(r - burst * 0.55), 0.010));
    // Dots only on the cyan sky (not the explosion)
    S.dotRegion = (1.0 - inBurst) * (1.0 - horizon);
    S.dotColor = LL_RED;
    return S;
}

// Drowning Girl — cyan wavy water with white foam, large face silhouette
Subject subjDrowning(vec2 uv, float aspect, float t, float bass) {
    Subject S;
    vec2 p = uv;
    // Water waves — sweeping curves
    float w = 0.50
            + 0.10 * sin(uv.x * 7.0 + t * 0.5)
            + 0.05 * sin(uv.x * 13.0 - t * 0.3);
    float water = step(uv.y, w);
    // Foam — high-curl bands above wave
    float foam = smoothstep(0.012, 0.0, abs(uv.y - w))
               + smoothstep(0.008, 0.0, abs(uv.y - (w - 0.06 - 0.02 * sin(uv.x * 11.0))));
    // Face silhouette — rounded shape upper-right
    vec2 fp = (uv - vec2(0.62, 0.66));
    fp.x *= aspect;
    float face = sdCircle(fp * vec2(0.85, 1.0), 0.18);
    float inFace = step(face, 0.0);
    // Tear streak
    float tear = sdCircle((uv - vec2(0.58, 0.55 + 0.02 * sin(t))) * vec2(aspect * 1.0, 2.5), 0.012);
    S.base = mix(LL_WHITE, LL_CYAN, water);
    S.base = mix(S.base, LL_WHITE, foam);
    S.base = mix(S.base, LL_YELLOW, inFace);
    S.base = mix(S.base, LL_CYAN, step(tear, 0.0));
    // Outline: face boundary + wave crest line
    S.outline = step(abs(face), 0.008);
    S.outline = max(S.outline, smoothstep(0.004, 0.0, abs(uv.y - w)) * (1.0 - inFace));
    // Dots on cyan water region only
    S.dotRegion = water * (1.0 - inFace) * (1.0 - foam);
    S.dotColor = LL_WHITE;
    // (Bass slightly bumps wave)
    S.base = mix(S.base, LL_WHITE, bass * 0.05);
    return S;
}

// Crying Girl — yellow hair field, cyan teardrop, red lips
Subject subjCrying(vec2 uv, float aspect, float t, float bass) {
    Subject S;
    vec2 p = uv;
    // Yellow hair zone — top half with curling boundary
    float hair = step(uv.y, 0.62 + 0.08 * sin(uv.x * 4.0 + 1.2));
    // Face — skin yellow too, but block with red lips
    vec2 lp = (uv - vec2(0.50, 0.30));
    lp.x *= aspect;
    float lips = sdRect(lp * vec2(1.0, 3.5), vec2(0.10, 0.04));
    float inLips = step(lips, 0.0);
    // Tear — falling cyan drop
    vec2 tp = (uv - vec2(0.60, 0.42 - 0.10 * fract(t * 0.4)));
    tp.x *= aspect;
    float tear = sdCircle(tp * vec2(1.0, 1.4), 0.018);
    float inTear = step(tear, 0.0);
    // Eye outline — small ellipse
    vec2 ep = (uv - vec2(0.58, 0.46));
    ep.x *= aspect;
    float eye = abs(length(ep * vec2(1.0, 2.2)) - 0.05);
    S.base = LL_WHITE;
    S.base = mix(S.base, LL_YELLOW, hair);
    S.base = mix(S.base, LL_WHITE, step(uv.y, 0.45) * (1.0 - hair));
    S.base = mix(S.base, LL_RED, inLips);
    S.base = mix(S.base, LL_CYAN, inTear);
    // Outline: hair-boundary, lips, tear, eye
    S.outline = smoothstep(0.006, 0.0, abs(uv.y - (0.62 + 0.08 * sin(uv.x * 4.0 + 1.2))));
    S.outline = max(S.outline, step(abs(lips), 0.006));
    S.outline = max(S.outline, step(abs(tear), 0.006));
    S.outline = max(S.outline, step(eye, 0.005));
    // Dots: red dots over hair (signature Lichtenstein hair pattern)
    S.dotRegion = hair;
    S.dotColor = LL_RED;
    S.base = mix(S.base, S.base, bass);
    return S;
}

// Sunrise — radiating yellow/red rays from lower horizon
Subject subjSunrise(vec2 uv, float aspect, float t, float bass) {
    Subject S;
    vec2 p = (uv - vec2(0.50, 0.10));
    p.x *= aspect;
    float ang = atan(p.y, max(p.x, 0.0001));
    float r = length(p);
    // Sun disc
    float sun = step(r, 0.18 + 0.02 * bass);
    // Alternating ray wedges
    float ray = step(0.0, sin(ang * 11.0 + t * 0.2));
    float aboveHorizon = step(0.10, uv.y);
    S.base = LL_CYAN;
    S.base = mix(S.base, LL_YELLOW, aboveHorizon * (1.0 - ray));
    S.base = mix(S.base, LL_RED, aboveHorizon * ray);
    S.base = mix(S.base, LL_YELLOW, sun);
    // Outline: sun rim + horizon line
    S.outline = step(abs(r - 0.18), 0.008);
    S.outline = max(S.outline, smoothstep(0.005, 0.0, abs(uv.y - 0.10)));
    // Dot region — over the cyan sky portion outside the sun
    S.dotRegion = (1.0 - aboveHorizon);
    S.dotColor = LL_RED;
    return S;
}

// Bauhaus / Warhol — clean primary-color fields, Ben-Day dots only.
// Three vertical color bars (yellow / red / cyan) plus a single bold
// black circle motif. No subject, no outlines beyond the circle. The
// palette discipline carries the composition.
Subject subjBauhaus(vec2 uv, float aspect, float t, float bass) {
    Subject S;
    // Three equal vertical fields — yellow | red | cyan.
    float band = floor(uv.x * 3.0);
    vec3 fieldA = LL_YELLOW;
    vec3 fieldB = LL_RED;
    vec3 fieldC = LL_CYAN;
    vec3 field  = (band < 0.5) ? fieldA : ((band < 1.5) ? fieldB : fieldC);
    S.base = field;
    // Single bold circle motif — Bauhaus geometric primitive.
    vec2 cp = (uv - vec2(0.50, 0.50));
    cp.x *= aspect;
    float disc = sdCircle(cp, 0.16);
    float inDisc = step(disc, 0.0);
    S.base = mix(S.base, LL_BLACK, inDisc);
    // Crisp outline ring around the disc.
    S.outline = step(abs(disc), 0.006);
    // Ben-Day dots overlay the WHOLE composition except the black disc —
    // pure halftone discipline over the primary fields.
    S.dotRegion = (1.0 - inDisc);
    // Choose dot color that contrasts with each band.
    vec3 dotA = LL_RED;    // red on yellow
    vec3 dotB = LL_WHITE;  // white on red
    vec3 dotC = LL_WHITE;  // white on cyan
    S.dotColor = (band < 0.5) ? dotA : ((band < 1.5) ? dotB : dotC);
    // Bass adds a quiet pulse to the disc edge — kept subtle.
    S.outline = max(S.outline, step(abs(disc - bass * 0.02), 0.004));
    return S;
}

// Dispatch by mood index — keep this in one place so the cross-fade
// blend can call it for both A and B panels.
Subject subjectByMood(int mood, vec2 uv, float aspect, float t, float bass) {
    if (mood == 0) return subjWhaam   (uv, aspect, t, bass);
    if (mood == 1) return subjDrowning(uv, aspect, t, bass);
    if (mood == 2) return subjCrying  (uv, aspect, t, bass);
    if (mood == 3) return subjSunrise (uv, aspect, t, bass);
    return            subjBauhaus  (uv, aspect, t, bass);
}

// Render a complete panel into a single color — base + ben-day dots +
// outline composited in subject space. We must do this *per panel* so
// the final cross-fade can mix between two fully-formed images instead
// of blending each element (outline / dots / base) independently, which
// produced the muddy mid-transition look.
vec3 renderPanel(int mood, vec2 uv, float aspect, float t,
                 float bass, float treble,
                 float dotDensityV, float dotRadiusV) {
    Subject S = subjectByMood(mood, uv, aspect, t, bass);
    vec3 c = S.base;
    float dots = benDay(uv, dotDensityV, dotRadiusV, aspect, treble);
    c = mix(c, S.dotColor, dots * S.dotRegion);
    c = mix(c, LL_BLACK, clamp(S.outline, 0.0, 1.0));
    return c;
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    float t = TIME;

    float bass   = clamp(audioBass, 0.0, 1.0) * audioReact;
    float mid    = clamp(audioMid,  0.0, 1.0) * audioReact;
    float treble = clamp(audioHigh, 0.0, 1.0) * audioReact;

    // ─── Panel selection — auto-cycle on TIME, override via input.
    // Five moods (0..4). Each panel dwells static for most of its
    // duration, then hard-fades over the LAST 1.5 seconds into the
    // next. We render both panels into FULL vec3 colors (base + dots +
    // outline already composited) and mix ONCE at the end, so all of
    // a panel's elements move together — no element-by-element blending
    // muddying the mid-transition.
    const int N_MOODS = 5;
    const float TRANSITION_SECONDS = 1.5;

    int moodA, moodB;
    float blend; // 0 = fully A, 1 = fully B
    if (moodOverride < 0) {
        float dur = max(panelDuration, 0.5);
        float panelT = t / dur;
        float idxA = floor(panelT);
        moodA = int(mod(idxA, float(N_MOODS)));
        moodB = int(mod(idxA + 1.0, float(N_MOODS)));
        float local = fract(panelT) * dur;          // seconds into panel A
        float startBlend = max(dur - TRANSITION_SECONDS, 0.0);
        // Hard time-driven fade — dwell static, then ramp.
        blend = smoothstep(0.0, 1.0,
                  clamp((local - startBlend) / max(TRANSITION_SECONDS, 0.0001), 0.0, 1.0));
    } else {
        moodA = int(moodOverride);
        moodB = moodA;
        blend = 0.0;
    }

    // ─── Render each panel as a complete image, then mix ONCE.
    vec3 colA = renderPanel(moodA, uv, aspect, t, bass, treble, dotDensity, dotRadius);
    vec3 col;
    if (blend <= 0.0) {
        col = colA;
    } else {
        vec3 colB = renderPanel(moodB, uv, aspect, t, bass, treble, dotDensity, dotRadius);
        col = mix(colA, colB, blend);
    }

    // Bass-triggered explosion flash — yellow starburst flares in
    // upper-left corner on bass impact, regardless of panel.
    {
        vec2 fc = (uv - vec2(0.18, 0.82));
        fc.x *= aspect;
        float fa = atan(fc.y, fc.x);
        float fr = length(fc);
        float flashR = 0.12 + 0.04 * cos(fa * 10.0);
        float flashAmt = step(fr, flashR) * bass;
        col = mix(col, LL_YELLOW, flashAmt * 0.85);
        col = mix(col, LL_BLACK,  step(abs(fr - flashR), 0.008) * bass);
    }

    // (No panel-transition wipe — panel changes are a smooth cross-fade.)

    // ─── SPEECH BUBBLE — cycles phrases on TIME, lives in silence
    if (speechBubble) {
        // Bubble center varies per panel for composition. We anchor to
        // moodA (the leaving panel) so the bubble doesn't snap during
        // the cross-fade.
        int mood = moodA;
        vec2 bC;
        if      (mood == 0) bC = vec2(0.22, 0.78);
        else if (mood == 1) bC = vec2(0.25, 0.30);
        else if (mood == 2) bC = vec2(0.22, 0.82);
        else if (mood == 3) bC = vec2(0.78, 0.78);
        else                bC = vec2(0.50, 0.78);

        vec2 bD = uv - bC;
        bD.x *= aspect;
        float bSz = 0.13 * (1.0 + 0.08 * sin(t * 1.6))
                          * (1.0 + bass * 0.18);
        // Rounded-rect bubble
        float bd = sdRoundRect(bD, vec2(bSz, bSz * 0.50), bSz * 0.18);
        float inB = step(bd, 0.0);
        // Pointed tail — segment from bubble edge toward subject
        vec2 tailFrom = vec2(-bSz * 0.3, -bSz * 0.45);
        vec2 tailTo   = vec2(-bSz * 0.85, -bSz * 1.1);
        float tail = sdSegment(bD, tailFrom, tailTo, bSz * 0.06);
        float inT = step(tail, 0.0);
        // Fill bubble white
        col = mix(col, LL_WHITE, max(inB, inT));
        // Outline
        float ringB = step(abs(bd), outlineWeight * 1.4);
        float ringT = step(abs(tail), outlineWeight * 1.0) * (1.0 - inB);
        col = mix(col, LL_BLACK, max(ringB, ringT));

        // Word inside bubble — cycle every panelDuration*0.5 seconds
        int word = int(mod(floor(t / max(panelDuration * 0.5, 0.5)), 5.0));
        // Render glyphs only inside bubble fill area
        float ink = 0.0;
        if (inB > 0.5) {
            ink = drawWord(uv, bC, bSz * 1.5, bSz * 0.55, aspect, word);
        }
        col = mix(col, LL_BLACK, ink);
    }

    // ─── Final ben-day shimmer — treble adds a faint extra dot pass
    if (treble > 0.05) {
        float d2 = benDay(uv + vec2(0.5 / max(dotDensity, 1.0)), dotDensity * 1.4,
                          dotRadius * 0.5, aspect, treble);
        col = mix(col, LL_CYAN, d2 * treble * 0.10);
    }

    // Output linear HDR — host applies ACES.
    gl_FragColor = vec4(col, 1.0);
}
