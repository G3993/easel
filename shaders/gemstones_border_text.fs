/*{
  "DESCRIPTION": "Gemstones Border Text — a ring of polished tumbled gems frames a glowing aperture; inside the aperture, a stratified iridescent gradient breathes and the live cue line types out as a small caption. Pseudo-3D: each gem is a thick rounded shape with a per-gem facet normal driving Fresnel + specular + warm refraction tint, lit by a procedural studio. Three player channels each animate their own gem cluster (left-arc, top-arc, right-arc); cue.latest hands the typewriter; audio.bass swells the aperture; audio.mid sparkles the gems.",
  "CREDIT": "ShaderClaw — gemstone border generator",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "LABEL": "Caption", "TYPE": "text", "DEFAULT": "ode to song", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "energyA", "LABEL": "Cluster A Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "energyB", "LABEL": "Cluster B Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "energyC", "LABEL": "Cluster C Energy", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].energy" },
    { "NAME": "activeA", "LABEL": "Cluster A Active",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].active" },
    { "NAME": "activeB", "LABEL": "Cluster B Active",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].active" },

    { "NAME": "bassDrive", "LABEL": "Bass → Aperture", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0, "BIND": "audio.bass" },
    { "NAME": "midDrive",  "LABEL": "Mid → Sparkle",   "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.8, "BIND": "audio.mid" },

    { "NAME": "gemCount",     "LABEL": "Gem Count",     "TYPE": "long",  "DEFAULT": 14, "VALUES": [8,10,12,14,16,18,20], "LABELS": ["8","10","12","14","16","18","20"] },
    { "NAME": "gemSize",      "LABEL": "Gem Size",      "TYPE": "float", "MIN": 0.05, "MAX": 0.20, "DEFAULT": 0.105 },
    { "NAME": "ringRadius",   "LABEL": "Ring Radius",   "TYPE": "float", "MIN": 0.20, "MAX": 0.55, "DEFAULT": 0.36 },
    { "NAME": "paletteMode",  "LABEL": "Palette",       "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Opal","Citrine","Amethyst","Aurora"] },
    { "NAME": "motionSpeed",  "LABEL": "Motion",        "TYPE": "float", "MIN": 0.0,  "MAX": 2.0, "DEFAULT": 0.6 },
    { "NAME": "audioDepth",   "LABEL": "Audio Depth",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.5, "DEFAULT": 0.8 },
    { "NAME": "facetSharp",   "LABEL": "Facet Sharp",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.5, "DEFAULT": 0.8 },
    { "NAME": "captionScale", "LABEL": "Caption Size",  "TYPE": "float", "MIN": 0.4,  "MAX": 1.8, "DEFAULT": 1.0 },
    { "NAME": "paperColor",   "LABEL": "Paper",         "TYPE": "color", "DEFAULT": [0.965, 0.945, 0.905, 1.0] }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  GEMSTONES BORDER TEXT
//
//  A ring of tumbled gems framing a glowing aperture. Each gem is built
//  from a rounded-blob SDF with per-gem facet noise driving the normal,
//  Fresnel, specular highlight, and a warm internal refraction tint.
//  Three "cluster arcs" (left/top/right) are each owned by a synthetic
//  player channel; their gems pop and re-tint in response. The aperture's
//  iridescent gradient breathes with audio.bass; mid-band drives sparkles
//  riding the gem rims. The live cue line types out as a small caption
//  inside the aperture.
// ════════════════════════════════════════════════════════════════════════

#define MAX_GEMS    20
#define MAX_WALK    48
#define SPACE_CH    26
#define TAU         6.28318530718

// ── Font atlas (matches text_clusters.fs convention) ───────────────────
float sampleChar(int ch, vec2 uv){
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}
int getChar(int slot){
    if (slot ==  0) return int(msg_0);  if (slot ==  1) return int(msg_1);
    if (slot ==  2) return int(msg_2);  if (slot ==  3) return int(msg_3);
    if (slot ==  4) return int(msg_4);  if (slot ==  5) return int(msg_5);
    if (slot ==  6) return int(msg_6);  if (slot ==  7) return int(msg_7);
    if (slot ==  8) return int(msg_8);  if (slot ==  9) return int(msg_9);
    if (slot == 10) return int(msg_10); if (slot == 11) return int(msg_11);
    if (slot == 12) return int(msg_12); if (slot == 13) return int(msg_13);
    if (slot == 14) return int(msg_14); if (slot == 15) return int(msg_15);
    if (slot == 16) return int(msg_16); if (slot == 17) return int(msg_17);
    if (slot == 18) return int(msg_18); if (slot == 19) return int(msg_19);
    if (slot == 20) return int(msg_20); if (slot == 21) return int(msg_21);
    if (slot == 22) return int(msg_22); if (slot == 23) return int(msg_23);
    if (slot == 24) return int(msg_24); if (slot == 25) return int(msg_25);
    if (slot == 26) return int(msg_26); if (slot == 27) return int(msg_27);
    if (slot == 28) return int(msg_28); if (slot == 29) return int(msg_29);
    if (slot == 30) return int(msg_30); if (slot == 31) return int(msg_31);
    if (slot == 32) return int(msg_32); if (slot == 33) return int(msg_33);
    if (slot == 34) return int(msg_34); if (slot == 35) return int(msg_35);
    if (slot == 36) return int(msg_36); if (slot == 37) return int(msg_37);
    if (slot == 38) return int(msg_38); if (slot == 39) return int(msg_39);
    if (slot == 40) return int(msg_40); if (slot == 41) return int(msg_41);
    if (slot == 42) return int(msg_42); if (slot == 43) return int(msg_43);
    if (slot == 44) return int(msg_44); if (slot == 45) return int(msg_45);
    if (slot == 46) return int(msg_46); if (slot == 47) return int(msg_47);
    return -1;
}
int charCount(){
    int n = int(msg_len);
    if (n <= 0) return 0;
    if (n > 48) return 48;
    return n;
}

// ── Hash / noise ───────────────────────────────────────────────────────
float h11(float n){ return fract(sin(n * 127.1) * 43758.5453); }
vec2  h21(float n){ return vec2(h11(n), h11(n + 17.31)); }
float vnoise(vec2 p){
    vec2 i = floor(p), f = fract(p);
    f = f*f*(3.0-2.0*f);
    float a = h11(dot(i,             vec2(1.0,157.0)));
    float b = h11(dot(i+vec2(1,0),   vec2(1.0,157.0)));
    float c = h11(dot(i+vec2(0,1),   vec2(1.0,157.0)));
    float d = h11(dot(i+vec2(1,1),   vec2(1.0,157.0)));
    return mix(mix(a,b,f.x), mix(c,d,f.x), f.y);
}
float fbm2(vec2 p){
    float v=0.0, a=0.5;
    for (int i=0;i<4;i++){ v += a*vnoise(p); p = p*2.07 + vec2(11.3,5.7); a *= 0.5; }
    return v;
}

// Smooth-min — used to fuse neighbouring gems where energy spikes.
float smin(float a, float b, float k){
    float h = clamp(0.5 + 0.5*(b-a)/k, 0.0, 1.0);
    return mix(b,a,h) - k*h*(1.0-h);
}

// Per-gem rounded blob SDF in 2D. We later derive a 2D normal from
// gradient + facet noise to fake a 3D surface.
float gemSDF(vec2 q, float rad, float wobble, float seed){
    // wobble warps the silhouette so each gem has a unique tumbled shape
    float ang  = atan(q.y, q.x);
    float warp = 0.18*sin(ang*3.0 + seed*1.3) + 0.10*sin(ang*5.0 + seed*2.7);
    float r    = rad * (1.0 + warp * wobble);
    return length(q) - r;
}

// Spectrum + palette mode lookup → base hue per gem.
vec3 spectrum(float t){ return 0.5 + 0.5*cos(TAU*(t + vec3(0.00,0.33,0.67))); }
vec3 palette(int mode, float t){
    if (mode == 1){ // Citrine — warm honey + amber + rose
        return mix(vec3(0.99,0.78,0.30), vec3(0.95,0.45,0.32), 0.5+0.5*sin(TAU*t));
    } else if (mode == 2){ // Amethyst — violet + plum + sky
        return mix(vec3(0.55,0.32,0.85), vec3(0.42,0.68,0.95), 0.5+0.5*sin(TAU*t+1.7));
    } else if (mode == 3){ // Aurora — green + cyan + magenta
        return 0.5 + 0.5*cos(TAU*(t + vec3(0.15,0.55,0.95)));
    }
    // 0 Opal — warm rainbow over a milky base (matches reference)
    vec3 rainbow = spectrum(t);
    return mix(rainbow, vec3(0.95,0.92,0.88), 0.25);
}

void main(){
    vec2 res    = RENDERSIZE;
    vec2 uv     = gl_FragCoord.xy / res;
    float aspect = res.x / res.y;
    vec2 p;
    p.x = (uv.x - 0.5) * aspect;
    p.y = uv.y - 0.5;

    float T = TIME * motionSpeed;
    float bass = clamp(audioBass * bassDrive, 0.0, 2.0);
    float mid  = clamp(audioMid  * midDrive,  0.0, 2.0);

    int gems = int(gemCount);
    if (gems > MAX_GEMS) gems = MAX_GEMS;
    if (gems < 6)        gems = 6;

    // ── Paper / backdrop with marbled wash (warm vintage poster) ──
    vec2 wp = vec2(fbm2(p*1.3 + T*0.05), fbm2(p*1.3 + 9.0 - T*0.04));
    vec3 paper = paperColor.rgb;
    paper = mix(paper, paper * vec3(0.96,0.92,0.88), wp.x*0.25);
    paper *= 1.0 - 0.18*dot(p,p);

    // ── Aperture: a glowing arch-shaped opening behind the gem ring ──
    // Distance to the arch interior. Shaped like a tall rounded mihrab,
    // matching the reference's vertical-egg silhouette.
    vec2  ap = p / vec2(0.92, 1.05);           // squash → tall egg
    float apR = ringRadius - gemSize*0.45;     // inside the gem ring
    float apD = length(ap) - apR;              // signed: <0 inside
    float inside = smoothstep(0.005, -0.010, apD);

    // Inside the aperture: a stratified iridescent gradient.
    // y → vertical stripe; T → drift; bass → vertical swell.
    float strat = clamp(0.5 + p.y*1.55 + 0.08*sin(p.x*3.5 + T*0.7), 0.0, 1.0);
    strat += 0.10 * bass * audioDepth * sin(p.y*4.0 - T*1.4);
    vec3 inner = spectrum(strat + T*0.04);
    // Bottom band: deeper teal-into-grass like the reference horizon.
    inner = mix(inner, vec3(0.18,0.42,0.30), smoothstep(0.62, 0.92, 1.0-strat) * 0.55);
    // Soft milky vignette on the rim of the aperture so it reads as glass.
    float rim = smoothstep(0.0, 0.18, apR - length(ap));
    inner *= 0.85 + 0.30*rim;
    // Tiny central silhouette — a distant peak, abstract not literal.
    float peakD = abs(p.x*0.9) + max(0.0, p.y + 0.04)*1.6;
    float peak  = smoothstep(0.18, 0.10, peakD) * smoothstep(-0.02, -0.10, p.y);
    inner = mix(inner, vec3(0.16,0.20,0.22), peak * 0.65);

    vec3 col = mix(paper, inner, inside);

    // ── Gem ring ───────────────────────────────────────────────────────
    // Each gem sits on the ring with a per-gem angular wobble. We loop
    // once over all gems, accumulating the closest gem's distance + its
    // surface color + facet-driven shading, plus a smin-merge with the
    // nearest neighbour so high-energy clusters fuse into "stretched"
    // organic shapes (still reads as separate stones in calm states).
    float bestD   = 1e6;
    vec3  bestCol = vec3(0.0);
    float bestSpec = 0.0;
    float bestFres = 0.0;
    float bestSparkle = 0.0;
    float bestEdge = 1e6;          // closest gem center distance (for halo)

    for (int i = 0; i < MAX_GEMS; i++){
        if (i >= gems) break;
        float fi = float(i);
        // Each gem gets a slot on the ring; ~75% of the ring is filled,
        // the bottom 25% (the "label area") is left bare like the reference.
        float t01 = fi / float(gems);
        float ang = 3.14159 * 0.65 + t01 * TAU * 0.95;        // start at upper-left
        // Per-gem drift on its slot so the ring breathes.
        float drift = sin(T*0.6 + fi*1.9) * 0.020;
        ang += drift;

        // Cluster ownership: left arc (i in first third) → cluster A,
        // top arc → cluster B, right arc → cluster C. Each cluster's
        // energy pops "its" gems outward and bumps their sparkle.
        float clusterT = t01;
        float eA = energyA * smoothstep(0.55, 0.05, clusterT);
        float eB = energyB * (1.0 - abs(clusterT - 0.5)*2.4);
        float eC = energyC * smoothstep(0.45, 0.95, clusterT);
        float eOwn = clamp(max(eA, max(eB, eC)), 0.0, 1.0);

        // Active flags pin a "highlight gem" per cluster (no audio needed).
        float pinA = activeA * smoothstep(0.55, 0.05, clusterT);
        float pinB = activeB * (1.0 - abs(clusterT - 0.5)*2.4);
        float pinOwn = clamp(max(pinA, pinB), 0.0, 1.0);

        // Gem center on the ring; energy nudges it slightly outward.
        float r   = ringRadius + 0.018*sin(T*0.4 + fi*2.7) + 0.040*eOwn;
        vec2  c   = vec2(cos(ang), sin(ang)) * r;
        // Gem-local coordinates with a per-gem tilt.
        float rot = (h11(fi+1.0) - 0.5) * 0.8 + 0.10*sin(T*0.3 + fi);
        float cs  = cos(rot), sn = sin(rot);
        vec2  q   = mat2(cs,-sn,sn,cs) * (p - c);
        // Per-gem radius variance (some pebbles big, some small).
        float rad = gemSize * mix(0.65, 1.30, h11(fi+7.0));
        rad *= 1.0 + 0.12 * eOwn + 0.06 * pinOwn;
        rad *= 1.0 + 0.04 * bass * audioDepth;
        float wob = 0.55 + 0.40 * h11(fi+13.0);
        float d   = gemSDF(q, rad, wob, fi*3.7);

        // Merge with running best — smin where energy is high so adjacent
        // gems on a hot cluster visually fuse; ordinary min otherwise.
        float k = 0.012 + 0.060 * eOwn;
        float pd = bestD;
        bestD = smin(bestD, d, k);
        float winLocal = clamp((pd - bestD) / k + 0.5, 0.0, 1.0);

        // Per-gem facet normal:  gradient of an fbm field inside the gem,
        // scaled by 1-r/rad so highlights cluster near the gem's apex.
        float fbmHere = fbm2(q * (8.0 + 12.0*facetSharp) + fi*5.1);
        float fbmDx   = fbm2(q * (8.0 + 12.0*facetSharp) + vec2(0.04,0.0) + fi*5.1);
        float fbmDy   = fbm2(q * (8.0 + 12.0*facetSharp) + vec2(0.0,0.04) + fi*5.1);
        vec2  gradN   = vec2(fbmDx - fbmHere, fbmDy - fbmHere) / 0.04;
        // Add the radial gradient (away-from-center fakes the dome).
        vec2  radN    = normalize(q + 1e-4) * smoothstep(rad, 0.0, length(q));
        vec3  N       = normalize(vec3(radN + gradN * 0.55, 1.0));

        // Light direction (top-left key).
        vec3 L = normalize(vec3(-0.45, 0.65, 0.6));
        vec3 V = vec3(0.0, 0.0, 1.0);
        vec3 H = normalize(L + V);
        float NdL = clamp(dot(N, L), 0.0, 1.0);
        float NdH = clamp(dot(N, H), 0.0, 1.0);
        float NdV = clamp(dot(N, V), 0.0, 1.0);
        float fres = pow(1.0 - NdV, 4.0);
        float spec = pow(NdH, mix(48.0, 160.0, facetSharp));

        // Base pigment per gem — palette mode + per-gem seed.
        float hue = fract(h11(fi+21.0) + T*0.04 + 0.15*eOwn);
        vec3  pig = palette(int(paletteMode), hue);
        // Warm internal refraction tint — light passes through the stone
        // and warms toward orange like the reference's amber gems.
        vec3  refr = mix(pig, vec3(1.0,0.62,0.30), 0.25 + 0.30*fres);
        // Soft inside body shading (Lambert + AO via SDF depth).
        float ao = clamp(1.0 + d/rad, 0.0, 1.0);
        vec3  body = refr * (0.35 + 0.55*NdL) * (0.55 + 0.45*ao);
        // Specular highlight + Fresnel rim.
        vec3  hi   = vec3(1.0, 0.96, 0.88) * spec * (0.6 + 0.7*facetSharp);
        vec3  rimC = mix(pig, vec3(1.0), 0.55) * fres * 0.7;
        vec3  surf = body + hi + rimC;

        // Sparkle: tiny audio.mid-driven pinpricks on the gem surface.
        float spk = step(0.985, fbm2(q*65.0 + T*4.0 + fi*9.0));
        surf += spk * mid * (0.6 + 0.8*pinOwn) * vec3(1.0,0.94,0.78);

        if (winLocal > 0.001){
            bestCol    = mix(bestCol, surf, winLocal);
            bestSpec   = mix(bestSpec, spec, winLocal);
            bestFres   = mix(bestFres, fres, winLocal);
            bestSparkle= mix(bestSparkle, spk, winLocal);
        }
        float ed = length(p - c);
        if (ed < bestEdge) bestEdge = ed;
    }

    // ── Compose gem ring over backdrop ─────────────────────────────────
    float fw   = fwidth(bestD);
    float fill = 1.0 - smoothstep(-fw, fw, bestD);
    // Drop shadow on the paper (offset down-right, blurred by fwidth).
    float shD  = bestD + 0.014;
    float sh   = (1.0 - smoothstep(-0.005, 0.025, shD)) * 0.25;
    col *= 1.0 - sh * (1.0 - inside*0.6);
    col  = mix(col, bestCol, fill);

    // Halo / inner glow around the ring — bass-reactive sun bleed.
    float halo = exp(-bestEdge * 6.0) * (0.20 + 0.45 * bass * audioDepth);
    col += halo * mix(vec3(1.0,0.78,0.45), vec3(0.55,0.85,1.0), inside) * (1.0 - fill);

    // ── Caption inside the aperture ───────────────────────────────────
    int total = charCount();
    if (total > 0){
        // Caption box: centered, narrow, anchored toward the bottom of
        // the aperture (label-like, not headline).
        float capH = 0.018 * captionScale;
        float capW = capH * (5.0/7.0);
        float kern = capW * 0.95;
        // How many chars per row? Fit row inside ~70% of aperture width.
        float rowW = ringRadius * 1.10;
        int charsPerRow = int(rowW / max(kern, 1e-4));
        if (charsPerRow < 6)  charsPerRow = 6;
        if (charsPerRow > 24) charsPerRow = 24;

        // Pre-pass: row count for word-wrap.
        int usedRows = 1;
        {
            int preR = 0, preC = 0;
            for (int i = 0; i < MAX_WALK; i++){
                if (i >= total) break;
                int ch = getChar(i);
                if (ch == SPACE_CH){
                    int wlen = 0;
                    for (int j = 1; j < MAX_WALK; j++){
                        int gj = i + j;
                        if (gj >= total) break;
                        int chj = getChar(gj);
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
            usedRows = preR + 1;
        }
        float blockH = float(usedRows) * capH * 1.25;
        float blockW = float(charsPerRow) * kern;
        // Caption anchor: just below the aperture's vertical center.
        vec2  capOrigin = vec2(-blockW*0.5, -0.06 - blockH*0.5);
        vec2  lp = p - capOrigin;

        // Typewriter reveal: only show chars up to msgAge*cps.
        float typed = (msgAge >= 0.0) ? clamp(msgAge * 22.0, 0.0, float(total)) : float(total);

        // Walk the message and lay it out.
        int cursorR = 0, cursorC = 0;
        float caretX = 0.0;
        float caretY = 0.0;
        float ink = 0.0;
        for (int i = 0; i < MAX_WALK; i++){
            if (i >= total) break;
            int ch = getChar(i);
            bool reveal = float(i) < typed;
            if (ch == SPACE_CH){
                int wlen = 0;
                for (int j = 1; j < MAX_WALK; j++){
                    int gj = i + j;
                    if (gj >= total) break;
                    int chj = getChar(gj);
                    if (chj == SPACE_CH || chj < 0 || chj > 36) break;
                    wlen++;
                }
                if (cursorC > 0 && cursorC + 1 + wlen > charsPerRow){
                    cursorR++; cursorC = 0;
                } else if (cursorC > 0){
                    cursorC++;
                }
            } else if (ch >= 0 && ch <= 36){
                if (reveal){
                    float cx = float(cursorC) * kern;
                    // Row 0 sits at the TOP of the block (reading order
                    // top→bottom). cy grows DOWN from the block top via
                    // (usedRows-1-cursorR)*lineH because lp.y is y-UP.
                    float cy = float(usedRows - 1 - cursorR) * capH * 1.25;
                    // lp.y is y-UP world (capOrigin at bottom-left). The
                    // host font atlas stores letter-top at v=1, so a
                    // direct y-up→v-up mapping puts letter-top at
                    // screen-top. The previous `1.0 -` here flipped
                    // glyphs upside down.
                    vec2 glyphLocal = vec2((lp.x - cx) / capW,
                                           (lp.y - cy) / capH);
                    float s = sampleChar(ch, glyphLocal);
                    s = smoothstep(0.20, 0.55, s);
                    ink = max(ink, s);
                    caretX = cx + kern;
                    caretY = cy;
                }
                cursorC++;
                if (cursorC >= charsPerRow){ cursorR++; cursorC = 0; }
            }
        }
        // Blinking caret while typing.
        if (msgAge >= 0.0 && typed < float(total)){
            vec2 cl = lp - vec2(caretX, caretY);
            float cb = step(0.0, cl.x) * step(cl.x, capW*0.12)
                     * step(0.0, cl.y) * step(cl.y, capH);
            float blink = 0.5 + 0.5*sin(TIME * 5.5);
            ink = max(ink, cb * blink);
        }

        // Caption color reads light against the aperture's gradient.
        vec3 inkColor = vec3(0.99,0.96,0.88);
        // Only render the caption where the aperture is filled.
        col = mix(col, inkColor, ink * inside * (1.0 - fill));
    }

    // Subtle gallery sheen sweep across the whole canvas.
    float sweep = smoothstep(0.0, 0.5, sin(p.x*1.4 - p.y*0.4 - T*0.5)*0.5 + 0.5);
    col += pow(sweep, 4.0) * 0.04 * vec3(1.0,0.95,0.85);

    // Soft bloom on bright pixels (energy-aware).
    float L = dot(col, vec3(0.299,0.587,0.114));
    col += 0.18 * smoothstep(0.65, 1.20, L) * col * (1.0 + 0.4*bass);

    // Continuous paper tooth so nothing reads as raw pixels.
    float tooth = fbm2(p * res.y * 0.018);
    col *= 1.0 + (tooth - 0.5) * 0.04;

    // Tonemap + gentle gamma curl.
    col = col / (1.0 + 0.55*col);
    col = pow(max(col, 0.0), vec3(0.94));

    gl_FragColor = vec4(col, 1.0);
}
