/*{
  "DESCRIPTION": "Doubleperson Convo Text — a two-mind conversation rendered as two abstract gradient pillars (Player A on the left, Player B on the right) traversed by an arc-trace of inked breath. Each pillar is its OWN player channel: A pulses+grows when player[1] speaks, B when player[2] speaks. Punctuation orbs drift through depth; the active speaker's caption typewriters in next to their pillar. The conversation is the alternation — overlap, hand-off, silence. Anti-pattern free: no portraits, no waveforms, no spectrum bars, asymmetric by construction. Returns LINEAR HDR.",
  "CREDIT": "ShaderClaw — A-List drop",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg",        "LABEL": "Caption",         "TYPE": "text",  "DEFAULT": "where I'm going, I don't know — but I'm on my way", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "energyA",    "LABEL": "Player A Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].energy" },
    { "NAME": "energyB",    "LABEL": "Player B Energy", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].energy" },
    { "NAME": "activeA",    "LABEL": "Player A Active", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].active" },
    { "NAME": "activeB",    "LABEL": "Player B Active", "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].active" },
    { "NAME": "pitchA",     "LABEL": "Player A Pitch",  "TYPE": "float", "DEFAULT": 0.4, "MIN": 0.0, "MAX": 1.0, "BIND": "player[1].pitch" },
    { "NAME": "pitchB",     "LABEL": "Player B Pitch",  "TYPE": "float", "DEFAULT": 0.6, "MIN": 0.0, "MAX": 1.0, "BIND": "player[2].pitch" },

    { "NAME": "bassDrive",  "LABEL": "Bass Drive",      "TYPE": "float", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 1.0, "BIND": "audio.bass" },

    { "NAME": "paperColor", "LABEL": "Paper",           "TYPE": "color", "DEFAULT": [0.93, 0.92, 0.91, 1.0] },
    { "NAME": "colorA",     "LABEL": "Player A Color",  "TYPE": "color", "DEFAULT": [0.22, 0.45, 0.48, 1.0] },
    { "NAME": "colorB",     "LABEL": "Player B Color",  "TYPE": "color", "DEFAULT": [0.95, 0.55, 0.45, 1.0] },
    { "NAME": "inkColor",   "LABEL": "Ink",             "TYPE": "color", "DEFAULT": [0.05, 0.05, 0.07, 1.0] },

    { "NAME": "separation", "LABEL": "Separation",      "TYPE": "float", "DEFAULT": 0.42, "MIN": 0.20, "MAX": 0.70 },
    { "NAME": "flowSpeed",  "LABEL": "Conversation Flow", "TYPE": "float", "DEFAULT": 0.30, "MIN": 0.0, "MAX": 1.5 },
    { "NAME": "audioDepth", "LABEL": "Audio Depth",     "TYPE": "float", "DEFAULT": 0.6,  "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "textScale",  "LABEL": "Text Scale",      "TYPE": "float", "DEFAULT": 1.0,  "MIN": 0.5, "MAX": 2.0 },
    { "NAME": "kerning",    "LABEL": "Kerning",         "TYPE": "float", "DEFAULT": 0.85, "MIN": 0.55, "MAX": 1.4 },
    { "NAME": "glow",       "LABEL": "Arc Glow",        "TYPE": "float", "DEFAULT": 0.6,  "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "restEnergy", "LABEL": "Rest Energy",     "TYPE": "float", "DEFAULT": 0.16, "MIN": 0.0, "MAX": 1.0 }
  ]
}*/

// =====================================================================
// Doubleperson Convo Text — two abstract minds talk in gradient pillars.
//
// Each pillar is asymmetric in size, hue, and vertical placement so the
// composition never reads as mirror-symmetric. An ink arc traces between
// them, modulated by whichever player is hot; punctuation orbs drift
// through z-depth. The active speaker's pillar swells, brightens, and
// pulls the caption to its side. Silence reads as both pillars settling
// to a small "rest breath" — never frozen, never identical.
// =====================================================================

#define MAX_WALK 48
#define SPACE_CH 26

const float TAU = 6.28318530718;

// ─── Font atlas ─────────────────────────────────────────────────────
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

int charCount() {
    int n = int(msg_len);
    if (n < 0) return 0;
    if (n > 48) return 48;
    return n;
}

// ─── Small utilities ────────────────────────────────────────────────
float h11(float n){ return fract(sin(n*127.1)*43758.5453); }
vec2  h21(float n){ return vec2(h11(n), h11(n+17.31)); }

float vnoise(vec2 p){
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float a = h11(dot(i,         vec2(1.0,157.0)));
    float b = h11(dot(i+vec2(1,0),vec2(1.0,157.0)));
    float c = h11(dot(i+vec2(0,1),vec2(1.0,157.0)));
    float d = h11(dot(i+vec2(1,1),vec2(1.0,157.0)));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}
float fbm2(vec2 p){
    float v=0.0, a=0.5;
    for (int i=0;i<4;i++){ v+=a*vnoise(p); p=p*2.07+vec2(11.3,5.7); a*=0.52; }
    return v;
}

// Round-rect SDF — the "pillar" body. r = corner radius.
float sdRoundedRect(vec2 p, vec2 b, float r){
    vec2 q = abs(p)-b+vec2(r);
    return min(max(q.x,q.y),0.0)+length(max(q,0.0))-r;
}

// Capsule (segment) SDF — used for the arc-trace approximation.
float sdCapsule(vec2 p, vec2 a, vec2 b, float r){
    vec2 pa = p-a, ba = b-a;
    float h = clamp(dot(pa,ba)/dot(ba,ba), 0.0, 1.0);
    return length(pa-ba*h)-r;
}

void main(){
    vec2 res    = RENDERSIZE;
    vec2 uv     = gl_FragCoord.xy / res;
    float aspect= res.x/res.y;

    // Aspect-corrected centered coords (y up).
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    float t = TIME;

    // ─── Player energies (semantic, post-binding) ──────────────────
    // Floors prevent total silence from looking dead — "rest breath".
    float restE = clamp(restEnergy, 0.0, 1.0);
    float eA = clamp(max(energyA, restE * 0.55), 0.0, 1.0);
    float eB = clamp(max(energyB, restE * 0.55), 0.0, 1.0);
    float aA = clamp(activeA, 0.0, 1.0);
    float aB = clamp(activeB, 0.0, 1.0);
    // Active-bias: if a player is "active" but energy is low (just
    // started speaking), give them visual presence anyway.
    eA = max(eA, aA * 0.45);
    eB = max(eB, aB * 0.45);
    float bass = clamp(bassDrive * audioDepth, 0.0, 2.0);

    // Whose turn it is, [-1..+1]. A wins → -1, B wins → +1.
    float turn = (eB - eA) / max(eA + eB, 1e-3);
    // Smoothed binary "who's talking" used by the caption side & arc
    // direction. Holds briefly when both quiet so silence reads.
    float sideB = smoothstep(-0.15, 0.15, turn);
    float sideA = 1.0 - sideB;

    // ─── Paper backdrop with subtle film grain noise ───────────────
    vec3 col = paperColor.rgb;
    float grain = fbm2(p * 6.0 + vec2(t*0.04, -t*0.03));
    col *= 1.0 - 0.06 * (grain - 0.5);
    // Faint vertical center spine — the implicit axis the convo crosses.
    float spine = smoothstep(0.004, 0.0, abs(p.x));
    col = mix(col, col*0.92, spine * 0.35);

    // ─── Depth layer 0: drifting punctuation orbs (background) ─────
    // Small dark circles meandering through deep space; bass jitters
    // them, the active player gravitationally pulls them sideways.
    float orbMask = 0.0;
    for (int i=0; i<7; i++){
        float fi = float(i);
        vec2 s = h21(fi*9.31);
        // Each orb assigned to a side by its seed; the OTHER side's
        // energy reels it inward, so when B talks, B-tinted orbs drift
        // toward B's pillar (visible parallax to the active speaker).
        float assign = step(0.5, s.x);                       // 0=A, 1=B
        vec2 home;
        home.x = mix(-0.30, 0.30, s.x) * aspect;
        home.y = (s.y - 0.5) * 0.85;
        // Slow elliptical drift — z parallax cue (small amp = "deep").
        float spd = 0.08 + 0.12 * s.x;
        vec2 drift = vec2(sin(t*spd + fi)*0.06,
                          cos(t*spd*0.83 + fi*1.7)*0.04);
        // Active-side gravity well — orbs of that side glide toward
        // their pillar's mouth (sub-pixel motion, depth cue).
        float pull = (assign < 0.5) ? eA*0.10 : -eB*0.10;
        vec2 op   = home + drift + vec2(pull, 0.0);
        float r   = mix(0.008, 0.018, s.y) * (1.0 + 0.4*bass);
        float d   = length(p - op) - r;
        float fw  = fwidth(d);
        orbMask  += (1.0 - smoothstep(-fw, fw, d)) * 0.85;
    }
    // Ink-dark orbs over paper.
    col = mix(col, inkColor.rgb, clamp(orbMask, 0.0, 1.0) * 0.85);

    // ─── Depth layer 1: the two pillar bodies ──────────────────────
    // Asymmetric by construction — different widths, different heights,
    // different vertical offsets — so the scene is NEVER mirror-symmetric.
    float sep   = clamp(separation, 0.20, 0.70);
    float xA    = -sep * aspect * 0.55;
    float xB    =  sep * aspect * 0.60;          // intentionally != -xA
    // Pillar A: taller, slimmer, sits low.
    vec2  bA    = vec2(0.105, 0.27 + 0.04*eA);
    vec2  cA    = vec2(xA, -0.10 + 0.04*sin(t*0.20));
    // Pillar B: shorter, wider, sits high.
    vec2  bB    = vec2(0.135, 0.22 + 0.04*eB);
    vec2  cB    = vec2(xB,  0.13 + 0.04*sin(t*0.17 + 1.3));
    // Energy-driven swell: active speaker's pillar grows ~12%.
    bA *= 1.0 + 0.12 * (eA - 0.5*eB);
    bB *= 1.0 + 0.12 * (eB - 0.5*eA);

    float sdA = sdRoundedRect(p - cA, bA, 0.012);
    float sdB = sdRoundedRect(p - cB, bB, 0.014);

    // Vertical-gradient fill inside each pillar. The TOP is the pillar's
    // own hue, the BOTTOM bleeds into a darker resonant ink — reads as
    // breath compressing downward. Pitch shifts hue slightly so a higher
    // voice reads as brighter.
    vec2  qA   = (p - cA) / max(bA.y, 1e-3);
    vec2  qB   = (p - cB) / max(bB.y, 1e-3);
    float vA   = clamp(qA.y * 0.5 + 0.5, 0.0, 1.0);
    float vB   = clamp(qB.y * 0.5 + 0.5, 0.0, 1.0);
    vec3  hueA = mix(colorA.rgb*0.18, colorA.rgb, smoothstep(0.0, 0.85, vA));
    vec3  hueB = mix(colorB.rgb*0.20, colorB.rgb, smoothstep(0.0, 0.85, vB));
    // Pitch tint (subtle): higher pitch shifts toward warmer for both.
    hueA = mix(hueA, hueA + vec3(0.08,0.02,-0.04), clamp(pitchA*0.6, 0.0, 0.6));
    hueB = mix(hueB, hueB + vec3(0.08,0.02,-0.04), clamp(pitchB*0.6, 0.0, 0.6));
    // Pulse: active speaker brightens; the inactive one fades to 70%.
    hueA *= 0.70 + 0.55*eA;
    hueB *= 0.70 + 0.55*eB;

    // Soft inner film grain across pillars — keeps them from reading flat.
    float gA = fbm2(p*4.0 + 13.0);
    float gB = fbm2(p*4.0 - 7.0);
    hueA *= 0.92 + 0.16*gA;
    hueB *= 0.92 + 0.16*gB;

    // AA fill the pillars over paper.
    float fwA   = fwidth(sdA);
    float fwB   = fwidth(sdB);
    float fillA = 1.0 - smoothstep(-fwA, fwA, sdA);
    float fillB = 1.0 - smoothstep(-fwB, fwB, sdB);
    col = mix(col, hueA, fillA);
    col = mix(col, hueB, fillB);

    // Soft dark "shadow stem" beneath each pillar (depth cue / weight).
    {
        // A's stem
        vec2 sp = p - (cA + vec2(0.0, -bA.y - 0.07));
        float stem = sdRoundedRect(sp, vec2(0.018, 0.06), 0.005);
        float sfw  = fwidth(stem);
        float sf   = 1.0 - smoothstep(-sfw, sfw, stem);
        col = mix(col, inkColor.rgb*0.85, sf * (0.30 + 0.25*eA));
    }
    {
        // B's stem — different length, different x offset
        vec2 sp = p - (cB + vec2(0.01, -bB.y - 0.085));
        float stem = sdRoundedRect(sp, vec2(0.012, 0.085), 0.004);
        float sfw  = fwidth(stem);
        float sf   = 1.0 - smoothstep(-sfw, sfw, stem);
        col = mix(col, inkColor.rgb*0.85, sf * (0.30 + 0.25*eB));
    }

    // ─── Depth layer 2: the ink arc traversing between minds ───────
    // A polyline approximated as 6 capsule segments along a sinusoid
    // anchored at the talking pillar's "mouth" (its TOP) and curling
    // toward the listener's. The arc's energy mirrors who's speaking.
    // No EKG: the curve crosses the center spine multiple times,
    // composing as a sweep, not a 1-D waveform.
    vec2 mouthA = cA + vec2(0.0, bA.y);
    vec2 mouthB = cB + vec2(0.0, bB.y);
    // Flow direction: A→B when A active, B→A when B active. When both
    // quiet, arc gently breathes in place. flowPhase advances with time
    // and is biased by who's talking.
    float flow  = t * (0.4 + 0.8 * clamp(flowSpeed, 0.0, 1.5));
    float dir   = sideB - sideA;            // -1..+1
    float arcInk = 0.0;
    const int SEGS = 12;
    vec2 prev = mouthA;
    for (int i=0; i<SEGS; i++){
        float u  = float(i+1) / float(SEGS);
        // Linear interp between mouths.
        vec2  base = mix(mouthA, mouthB, u);
        // Add a vertical sine that swirls along u — multi-crossing arc.
        float amp  = 0.16 * (0.5 + 0.5*sin(u*TAU*1.5 + flow*0.7));
        amp       *= 0.7 + 0.7*max(eA, eB);
        float phase = u*TAU*1.6 + flow*1.1 + dir*0.6;
        vec2  cur   = base + vec2(0.0, sin(phase)*amp);
        // Slight horizontal wobble so the path is not a pure sine.
        cur.x      += cos(phase*0.6 + u*1.7)*0.025;
        float r     = mix(0.0035, 0.0014, u);   // tapers toward listener
        r          *= 0.85 + 0.45 * max(eA, eB);
        float d     = sdCapsule(p, prev, cur, r);
        float fw    = fwidth(d);
        float seg   = 1.0 - smoothstep(-fw, fw, d);
        arcInk      = max(arcInk, seg);
        prev        = cur;
    }
    // Arc color: ink core + tinted halo from the speaker.
    vec3 arcTint = mix(colorA.rgb, colorB.rgb, sideB);
    col = mix(col, inkColor.rgb, arcInk * 0.95);
    // Halo (glow) — wider falloff outside the capsule, biased by glow input.
    // We approximate halo by recomputing a few samples around the same path
    // — but cheaper: reuse arcInk's soft fringe via a second pass with a
    // slightly larger radius via fwidth widening.
    float halo = arcInk * (0.6 + 0.6*max(eA,eB));
    col = mix(col, arcTint, halo * 0.18 * clamp(glow, 0.0, 2.0));

    // Arrowhead accent at the listener-side terminus (silent compose cue).
    // A tiny triangle SDF at `prev`, oriented along the last segment.
    {
        vec2 tipDir = normalize(prev - mouthA + vec2(1e-3, 0.0));
        vec2 n      = vec2(-tipDir.y, tipDir.x);
        vec2 lp     = p - prev;
        // axial / lateral
        float ax    = dot(lp, tipDir);
        float lat   = dot(lp, n);
        // Triangle from (0..0.025) wide tapering, but BEHIND tip (negative ax).
        float tri   = max(-ax - 0.025, abs(lat) - max(0.0, 0.012 + ax*0.4));
        float tfw   = fwidth(tri);
        float tf    = 1.0 - smoothstep(-tfw, tfw, tri);
        col = mix(col, inkColor.rgb, tf * 0.85 * max(eA, eB));
    }

    // ─── Depth layer 3: typewriter caption near active speaker ─────
    // The caption appears as small glyphs floating beside whichever
    // pillar is currently active. msgAge drives typewriter reveal.
    int total = charCount();
    int reveal = total;
    bool live = msgAge >= 0.0;
    if (live) {
        // ~26 chars per second, matching cluster shaders.
        float typed = clamp(msgAge * 26.0, 0.0, float(total));
        reveal = int(floor(typed));
        if (reveal < 0) reveal = 0;
        if (reveal > total) reveal = total;
    }

    if (reveal > 0) {
        // Caption anchor: just to the inside of the active pillar's
        // mid-height. When neither is active, sit near A (left).
        vec2 anchorA = cA + vec2(bA.x + 0.025, 0.04);
        vec2 anchorB = cB + vec2(-bB.x - 0.025, -0.02);
        vec2 anchor  = mix(anchorA, anchorB, sideB);
        // Caption block geometry — small editorial size.
        float scale = clamp(textScale, 0.5, 2.0);
        float charH = 0.026 * scale;
        float charW = charH * (5.0/7.0);
        float kern  = charW * clamp(kerning, 0.55, 1.4);
        // Word-wrap target width — slimmer when on the right, so it
        // doesn't fall off canvas.
        float boxW  = 0.34;
        int   charsPerRow = max(1, int(floor(boxW / kern)));
        if (charsPerRow > 24) charsPerRow = 24;

        // Pre-walk: count rows used by reveal chars under word-wrap.
        int preR = 0, preC = 0;
        for (int i=0; i<MAX_WALK; i++){
            if (i >= reveal) break;
            int ch = getChar(i);
            if (ch == SPACE_CH) {
                int wlen = 0;
                for (int j=1; j<MAX_WALK; j++){
                    int jj = i+j;
                    if (jj >= reveal) break;
                    int chj = getChar(jj);
                    if (chj == SPACE_CH || chj < 0 || chj > 36) break;
                    wlen++;
                }
                if (preC > 0 && preC + 1 + wlen > charsPerRow){
                    preR++; preC = 0;
                } else if (preC > 0){
                    preC++;
                }
            } else if (ch >= 0 && ch <= 36){
                preC++;
                if (preC >= charsPerRow){ preR++; preC = 0; }
            }
        }
        int rows = preR + 1;
        float blockH = float(rows) * charH * 1.25;

        // Caption local coords. Caption sits ABOVE its anchor.
        vec2 lp;
        lp.x = p.x - (anchor.x - boxW*0.5*step(0.5, 1.0 - sideB));
        // When on B side (right), anchor is the RIGHT edge of the box.
        if (sideB > 0.5) lp.x = p.x - (anchor.x - boxW);
        else             lp.x = p.x - anchor.x;
        lp.y = (anchor.y + blockH*0.5) - p.y;
        // Clip to caption box.
        if (lp.x >= 0.0 && lp.x <= boxW && lp.y >= 0.0 && lp.y <= blockH){
            int targetCol = int(floor(lp.x / kern));
            int targetRow = int(floor(lp.y / (charH * 1.25)));
            if (targetCol < charsPerRow && targetRow < rows){
                // Glyph-local coords inside this cell.
                float rowPad = (charH * 1.25 - charH) * 0.5;
                float yInRow = lp.y - float(targetRow) * (charH*1.25) - rowPad;
                float colPad = (kern - charW) * 0.5;
                float xInCol = lp.x - float(targetCol) * kern - colPad;
                if (yInRow >= 0.0 && yInRow <= charH &&
                    xInCol >= 0.0 && xInCol <= charW){
                    // Walk reveal to find the char at (targetRow,targetCol).
                    int cursorR = 0, cursorC = 0;
                    int outCh = -1;
                    for (int i=0; i<MAX_WALK; i++){
                        if (i >= reveal) break;
                        if (cursorR > targetRow) break;
                        int ch = getChar(i);
                        if (ch == SPACE_CH){
                            int wlen = 0;
                            for (int j=1; j<MAX_WALK; j++){
                                int jj = i+j;
                                if (jj >= reveal) break;
                                int chj = getChar(jj);
                                if (chj == SPACE_CH || chj < 0 || chj > 36) break;
                                wlen++;
                            }
                            if (cursorC > 0 && cursorC + 1 + wlen > charsPerRow){
                                cursorR++; cursorC = 0;
                            } else if (cursorC > 0){
                                if (cursorR == targetRow && cursorC == targetCol)
                                    outCh = SPACE_CH;
                                cursorC++;
                            }
                        } else if (ch >= 0 && ch <= 36){
                            if (cursorR == targetRow && cursorC == targetCol)
                                outCh = ch;
                            cursorC++;
                            if (cursorC >= charsPerRow){
                                cursorR++; cursorC = 0;
                            }
                        }
                    }
                    if (outCh >= 0 && outCh <= 35 && outCh != SPACE_CH){
                        // V-flip atlas coord (atlas origin bottom-left).
                        vec2 cellLocal = vec2(xInCol/charW, 1.0 - yInRow/charH);
                        float s = sampleChar(outCh, cellLocal);
                        s = smoothstep(0.18, 0.55, s);
                        if (s > 0.001){
                            // Caption tints toward the active speaker.
                            vec3 ink = mix(inkColor.rgb, arcTint*0.35 + inkColor.rgb*0.75, 0.4);
                            col = mix(col, ink, s);
                        }
                    }
                }
            }
        }
    }

    // ─── Foreground accent: a few sparse paper specks (top z-layer) ─
    // Keeps the foreground from feeling thin; intensity tied to bass.
    float specks = pow(fbm2(p*22.0 + vec2(t*0.5, 0.0)), 8.0);
    col = mix(col, inkColor.rgb, specks * 0.10 * (0.5 + bass));

    // ─── Vignette + tooth ──────────────────────────────────────────
    float vig = 1.0 - 0.20*dot(p, p);
    col *= vig;
    float tooth = fbm2(uv * res.y * 0.012) + 0.5*fbm2(uv * res.y * 0.03 + 7.0);
    col *= 1.0 + (tooth - 0.75) * 0.04;

    gl_FragColor = vec4(col, 1.0);
}
