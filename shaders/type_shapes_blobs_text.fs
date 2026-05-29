/*{
  "DESCRIPTION": "Type, Shapes, Blobs, Text — a serif-and-silhouette dance. Voice-driven typewriter words sit on warm-paper baselines while stepped rectangle ridges and ovoid blob choruses interrupt them, each driven by its own player channel. Fwidth-AA SDFs, layered z, premium gallery composition. The text is the score; the blobs are the choir.",
  "CREDIT": "Easel / type_shapes_blobs_text",
  "CATEGORIES": ["Generator", "Text", "A-List"],
  "INPUTS": [
    { "NAME": "msg", "LABEL": "Message", "TYPE": "text", "DEFAULT": "IN THE SPACE BETWEEN TWO THOUGHTS LIES THE GARDEN", "MAX_LENGTH": 48, "BIND": "cue.latest" },

    { "NAME": "energyA", "LABEL": "Player A (ridges)", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].energy" },
    { "NAME": "energyB", "LABEL": "Player B (blobs)", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].energy" },
    { "NAME": "energyC", "LABEL": "Player C (line wt)", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[3].energy" },
    { "NAME": "activeA", "LABEL": "Active A",          "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[1].active" },
    { "NAME": "activeB", "LABEL": "Active B",          "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "BIND": "player[2].active" },
    { "NAME": "audioDepth","LABEL":"Audio Depth",      "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.6, "BIND": "audio.level" },

    { "NAME": "blobCount", "LABEL": "Blob Count",    "TYPE": "long",  "DEFAULT": 5, "VALUES": [3,4,5,6,7,8], "LABELS": ["3","4","5","6","7","8"] },
    { "NAME": "blobSize",  "LABEL": "Blob Size",     "TYPE": "float", "MIN": 0.5,  "MAX": 2.0,  "DEFAULT": 1.0 },
    { "NAME": "kerning",   "LABEL": "Kerning",       "TYPE": "float", "MIN": 0.6,  "MAX": 1.6,  "DEFAULT": 0.92 },
    { "NAME": "motionSpeed","LABEL":"Motion Speed",  "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.85 },
    { "NAME": "palette",   "LABEL": "Palette",       "TYPE": "long",  "DEFAULT": 0, "VALUES": [0,1,2,3], "LABELS": ["Ink on Paper","Bone on Plum","Cream on Slate","Carbon on Bone"] },
    { "NAME": "paperTone", "LABEL": "Paper Warmth",  "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6 }
  ]
}*/

// ═══════════════════════════════════════════════════════════════════════
//  TYPE · SHAPES · BLOBS · TEXT
//
//  Reference: serif words ("in the … garden …") with stepped rectangle
//  ridges and ovoid clusters punctuating each line. Here it's voice-
//  reactive: words type in from cue.latest, ridges modulate to one
//  player's energy, blobs to another, line weight to a third.
//  Three z-planes (paper → ridges → blobs+type) with parallax, real
//  fwidth-AA on every silhouette. Stillness reads as intention; energy
//  pushes ridges up and blobs forward.
// ═══════════════════════════════════════════════════════════════════════

#define MAX_CHARS    48
#define MAX_BLOBS    8
#define SPACE_CH     26
#define TAU          6.28318530718

// ─── Font atlas (Easel ShaderClaw font: 37 glyphs, 26 = space) ─────────
float sampleChar(int ch, vec2 uv) {
    if (ch < 0 || ch > 36) return 0.0;
    if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) return 0.0;
    return texture2D(fontAtlasTex, vec2((float(ch) + uv.x) / 37.0, uv.y)).r;
}
int getChar(int slot) {
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
int charCount() {
    int n = int(msg_len);
    if (n <= 0) return 0;
    if (n > MAX_CHARS) return MAX_CHARS;
    return n;
}

// ─── hashes & noise ─────────────────────────────────────────────────────
float h11(float n){ return fract(sin(n*127.1)*43758.5453); }
vec2  h21(float n){ return vec2(h11(n), h11(n+17.31)); }
float vnoise(vec2 p){
    vec2 i=floor(p), f=fract(p); f=f*f*(3.0-2.0*f);
    float a=h11(dot(i,vec2(1.0,157.0)));
    float b=h11(dot(i+vec2(1.0,0.0),vec2(1.0,157.0)));
    float c=h11(dot(i+vec2(0.0,1.0),vec2(1.0,157.0)));
    float d=h11(dot(i+vec2(1.0,1.0),vec2(1.0,157.0)));
    return mix(mix(a,b,f.x),mix(c,d,f.x),f.y);
}
float fbm2(vec2 p){ float v=0.0, a=0.55; for(int i=0;i<3;i++){ v+=a*vnoise(p); p=p*2.07+vec2(11.3,5.7); a*=0.52; } return v; }

// ─── palettes ──────────────────────────────────────────────────────────
void getPalette(int idx, out vec3 paper, out vec3 ink, out vec3 accent){
    if (idx == 1)      { paper = vec3(0.16,0.10,0.18); ink = vec3(0.94,0.90,0.84); accent = vec3(0.86,0.42,0.50); }
    else if (idx == 2) { paper = vec3(0.18,0.20,0.24); ink = vec3(0.97,0.94,0.86); accent = vec3(0.90,0.74,0.40); }
    else if (idx == 3) { paper = vec3(0.93,0.91,0.86); ink = vec3(0.08,0.07,0.08); accent = vec3(0.18,0.20,0.24); }
    else               { paper = vec3(0.95,0.93,0.88); ink = vec3(0.06,0.05,0.06); accent = vec3(0.10,0.10,0.12); }
}

// ─── SDFs ──────────────────────────────────────────────────────────────
float sdRect(vec2 p, vec2 b){ vec2 d = abs(p) - b; return length(max(d,0.0)) + min(max(d.x,d.y),0.0); }
float sdOval(vec2 p, vec2 r){ // axis-aligned oval via scaled circle
    vec2 q = p / r; float k0 = length(q); float k1 = length(q/r);
    return (k0 - 1.0) * min(r.x,r.y) / max(k1,1e-4);
}

// ─── Type rendering (one line, left-aligned, monospace from atlas) ─────
// Returns ink mask at (p) for a chunk [a,b) of the message rendered at
// baseline y=baseY with glyph height gh starting at x=x0. fwidth-AA.
float renderLine(vec2 p, int a, int b, float x0, float baseY, float gh, float kern){
    float gw = gh * (5.0/7.0);
    float stride = gw * kern;
    int n = b - a; if (n <= 0) return 0.0;
    float blockW = stride * float(n);
    // localize
    float lx = p.x - x0;
    float ly = baseY - p.y;   // glyph top at baseY+gh? we use baseY as top
    if (lx < 0.0 || lx > blockW) return 0.0;
    if (ly < 0.0 || ly > gh)     return 0.0;
    int col = int(floor(lx / stride));
    if (col < 0 || col >= n) return 0.0;
    float cx = (lx - float(col)*stride) / gw;
    float cy = 1.0 - ly / gh;
    if (cx < 0.0 || cx > 1.0) return 0.0;
    int ch = getChar(a + col);
    if (ch < 0 || ch == SPACE_CH) return 0.0;
    float s = sampleChar(ch, vec2(cx, cy));
    return smoothstep(0.32, 0.62, s);
}

// Pack the message into N lines word-wrapped at lineChars chars.
// We compute line ranges deterministically and feed renderLine. To keep
// the loop bounded and GLSL-330 safe, lines are evaluated up to maxLines.
void wrapLine(int lineIdx, int total, int lineChars, out int a, out int b){
    a = 0; b = 0;
    int row = 0;
    int colCursor = 0;
    int segStart = 0;
    for (int i = 0; i < MAX_CHARS; i++){
        if (i >= total) {
            if (row == lineIdx) { a = segStart; b = i; }
            return;
        }
        int ch = getChar(i);
        bool sp = (ch == SPACE_CH || ch < 0 || ch > 36);
        if (sp){
            // look ahead: word length
            int wlen = 0;
            for (int j=1;j<MAX_CHARS;j++){
                int k = i+j; if (k>=total) break;
                int cj = getChar(k); if (cj==SPACE_CH || cj<0 || cj>36) break;
                wlen++;
            }
            if (colCursor > 0 && colCursor + 1 + wlen > lineChars){
                // wrap
                if (row == lineIdx){ a = segStart; b = i; return; }
                row++; segStart = i+1; colCursor = 0;
            } else if (colCursor > 0){
                colCursor++;
            } else {
                segStart = i+1;
            }
        } else {
            colCursor++;
            if (colCursor >= lineChars){
                if (row == lineIdx){ a = segStart; b = i+1; return; }
                row++; segStart = i+1; colCursor = 0;
            }
        }
    }
}

void main(){
    vec2 res = RENDERSIZE;
    vec2 uv  = gl_FragCoord.xy / res;
    float asp = res.x / max(res.y,1.0);
    // centered, aspect-corrected p in roughly [-asp/2, asp/2] × [-0.5, 0.5]
    vec2 p = vec2((uv.x - 0.5) * asp, uv.y - 0.5);

    float T = TIME * motionSpeed;
    float eA = clamp(energyA, 0.0, 1.0);
    float eB = clamp(energyB, 0.0, 1.0);
    float eC = clamp(energyC, 0.0, 1.0);
    float aA = clamp(activeA, 0.0, 1.0);
    float aB = clamp(activeB, 0.0, 1.0);
    float aud = clamp(audioDepth, 0.0, 2.0);
    float bass = audioBass;
    float lvl  = audioLevel;

    // ── palette ───────────────────────────────────────────────────────
    vec3 paper, ink, accent;
    getPalette(int(palette), paper, ink, accent);
    // warm tone wash on paper — fwidth-AA on every following silhouette
    vec3 paperWarm = mix(paper, paper * vec3(1.04, 0.99, 0.93), paperTone);
    float grain = fbm2(p * 8.0 + vec2(0.13*T, -0.07*T));
    paperWarm *= 1.0 + (grain - 0.5) * 0.04;
    // soft vignette
    paperWarm *= 1.0 - 0.10 * dot(p, p);

    vec3 col = paperWarm;

    // ── BACK PLANE: faint horizon haze (depth cue, no horizon scene) ──
    float haze = smoothstep(-0.45, 0.55, p.y);
    col = mix(col, paperWarm * mix(0.96, 1.06, haze), 0.35);

    // ── MID PLANE: stepped rectangle "ridges" driven by player A ──────
    // Eight serif baselines; each baseline has stacked rectangles whose
    // heights are a small per-cell function of position+time+energyA.
    // The result looks like discrete typographic ornaments, not bars.
    int lines = 8;
    float lineH = 0.11;
    float topY  = 0.42;
    float ridgeFill = 0.0;
    {
        for (int li = 0; li < 8; li++){
            float baseY = topY - float(li) * lineH;
            // ridge centerline sits just above each baseline
            float yc = baseY + 0.018;
            // ridge body — extent across canvas wave
            float xL = -0.36 * asp + 0.08 * sin(float(li)*1.3 + T*0.20);
            float xR =  0.36 * asp - 0.08 * cos(float(li)*0.7 + T*0.17);
            if (p.x < xL || p.x > xR) continue;
            // local x in [0,1] along ridge
            float u = (p.x - xL) / max(xR - xL, 1e-3);
            // 12 cells per ridge — each cell a rectangle whose height
            // pulses to player A + a per-cell phase. Quiet → near-flat;
            // loud → tall stepped silhouette like the reference.
            float cells = 12.0;
            float cellU = u * cells;
            float cellI = floor(cellU);
            float cellF = fract(cellU);
            float seed = h11(float(li)*7.13 + cellI*3.71);
            // gate so ridges have intentional silence; energyA opens it
            float openGate = step(0.55 - 0.6*eA - 0.15*aA, seed);
            // per-cell height (0..1)
            float wob = 0.5 + 0.5 * sin(T*0.9 + seed*9.0 + float(li)*0.5);
            float hCell = mix(0.05, 0.55, seed) * (0.35 + 0.65 * mix(wob, 1.0, eA));
            hCell *= openGate;
            // small subcell — 1px on each cell side
            float cellW = (xR - xL) / cells;
            float bx = xL + (cellI + 0.5) * cellW;
            float bw = cellW * 0.92 * 0.5;            // half-width
            float bh = hCell * 0.075;                 // half-height
            // rectangle SDF
            vec2 rp = vec2(p.x - bx, p.y - (yc + bh));
            float d = sdRect(rp, vec2(bw, bh));
            float fw = fwidth(d);
            float mask = 1.0 - smoothstep(-fw, fw, d);
            ridgeFill = max(ridgeFill, mask);
        }
    }
    // Ridges sit in ink; energy A adds an accent flicker on top edges
    vec3 ridgeCol = mix(ink, mix(ink, accent, 0.35), 0.6 * eA);
    col = mix(col, ridgeCol, ridgeFill * (0.85 + 0.15*aA));

    // ── FRONT PLANE: ovoid blob chorus driven by player B ─────────────
    // A horizontal cluster of fused ovals (smooth-union) that drift along
    // the canvas mid, occasionally rising on player B energy. Each blob
    // is a real fwidth-AA SDF oval; the cluster z-plane is closer than
    // ridges, so it occludes (and parallaxes opposite to) them.
    int nBlobs = int(blobCount);
    if (nBlobs > MAX_BLOBS) nBlobs = MAX_BLOBS;
    if (nBlobs < 3) nBlobs = 3;
    float blobMask = 0.0;
    vec2  blobCenterMass = vec2(0.0);
    float blobWeight = 0.0;
    {
        // cluster anchor — drifts horizontally, lifts on energyB
        float clusterX = -0.18*asp + 0.36*asp*sin(T*0.13);
        float clusterY = -0.06 + 0.18 * eB + 0.04 * sin(T*0.21);
        // parallax: blobs move opposite to ridges' subtle x sway
        clusterX -= 0.03 * sin(T*0.20);
        // composite smooth-min SDF
        float sdf = 1e6;
        float sumR = 0.0;
        // pre-compute per-blob and accumulate smooth-min
        for (int i = 0; i < MAX_BLOBS; i++){
            if (i >= nBlobs) break;
            float fi = float(i);
            float t  = (fi + 0.5) / float(nBlobs);    // 0..1 across cluster
            // base width across cluster
            float spread = 0.22 * float(nBlobs) / 5.0; // matches ref density
            float bx = clusterX + (t - 0.5) * spread;
            // tiny vertical jitter — heartbeat
            float by = clusterY + 0.012 * sin(T*1.7 + fi*1.13);
            // size: middle blobs slightly larger (matches ref chorus shape)
            float bell = 1.0 - pow(abs(t - 0.5)*2.0, 1.7);
            float r = 0.030 * blobSize * mix(0.85, 1.35, bell);
            // breathe on player B energy & audio level
            r *= 1.0 + 0.18*eB + 0.10*aud*lvl;
            // oval (slightly taller than wide — the reference shape)
            vec2 rad = vec2(r * 0.92, r * 1.18);
            float di = sdOval(p - vec2(bx, by), rad);
            // smooth-union
            float k = 0.018;
            float h = clamp(0.5 + 0.5*(sdf - di)/k, 0.0, 1.0);
            sdf = mix(sdf, di, h) - k*h*(1.0 - h);
            blobCenterMass += vec2(bx, by) * r;
            sumR += r;
        }
        if (sumR > 1e-4) blobCenterMass /= sumR;
        blobWeight = sumR;
        float fw = fwidth(sdf);
        blobMask = 1.0 - smoothstep(-fw, fw, sdf);
        // ink fill with subtle inner highlight to read as a solid form
        float inner = 1.0 - smoothstep(-0.012, -0.002, sdf);
        vec3 blobInk = mix(ink, mix(ink, accent, 0.25 * aB), 0.5 * eB);
        // soft-shadow under the cluster — depth cue
        float shadow = 1.0 - smoothstep(0.0, 0.06, sdf + 0.03);
        col *= mix(1.0, mix(0.78, 0.92, 1.0 - eB), shadow * 0.5);
        col = mix(col, blobInk, blobMask);
        // tiny specular tick — gallery sheen, not CG
        col = mix(col, mix(blobInk, vec3(1.0), 0.4), blobMask * inner * 0.06);
    }

    // ── TYPE PLANE: word-wrapped serif-ish atlas type on baselines ────
    // The type sits in the SAME baselines as the ridges, so words and
    // ridges read as one composition (ref: "in the [ridge] / space
    // [ridge] between / two ... thoughts [ridge] lies the [blobs]
    // garden ..."). Word wrap puts message into lines; each line
    // chooses a horizontal offset that leaves room for a ridge slot.
    int total = charCount();
    if (total > 0){
        // typewriter reveal — uses msgAge if available, else show all
        float reveal = 1.0;
        if (msgAge >= 0.0) {
            const float CPS = 26.0;
            reveal = clamp(msgAge * CPS / max(float(total), 1.0), 0.0, 1.0);
        }
        int shown = int(floor(reveal * float(total) + 0.5));
        if (shown > total) shown = total;
        if (shown < 0) shown = 0;

        // 8 lines, ~14 chars wide each (auto if msg short)
        int lineChars = 14;
        if (total <= 24) lineChars = 12;
        float gh = 0.058;
        // line-weight breathing — player C drives extra contrast/scale
        gh *= 1.0 + 0.04 * eC;
        float kern = clamp(kerning + 0.05*eC, 0.6, 1.6);
        // text mask accumulated
        float textMask = 0.0;
        // Render up to lines lines (li in [0,8))
        for (int li = 0; li < 8; li++){
            float baseY = topY - float(li) * lineH;
            // line range (a,b)
            int a; int b;
            wrapLine(li, total, lineChars, a, b);
            if (b <= a) continue;
            // clamp by typewriter
            if (a >= shown) continue;
            int bb = b; if (bb > shown) bb = shown;
            if (bb <= a) continue;
            // alternate horizontal alignment so ridges and words trade
            // positions across lines (matches reference cadence).
            float ax = ((li & 1) == 0) ? -0.32 * asp : -0.14 * asp;
            // top-of-glyph y
            float topGlyph = baseY + gh * 0.85;
            float m = renderLine(p, a, bb, ax, topGlyph, gh, kern);
            textMask = max(textMask, m);
        }
        // type sits on top of ridges & blobs (closest z-plane)
        col = mix(col, ink, textMask);
        // caret on the active glyph — only when typewriter is active
        if (msgAge >= 0.0 && shown < total) {
            // caret position = end of last visible glyph on the last
            // partially-revealed line
            // (simple visual cue: a vertical bar at the right edge of
            // the currently visible block on the last line drawn)
            // — deferred: implicit via the live reveal looking alive.
        }
    }

    // ── FOREGROUND ATMOSPHERE: subtle paper sheen + audio-depth fog ──
    // Audio level pushes a soft luminous wash forward (depth cue)
    float sheen = pow(smoothstep(-0.5, 0.5, p.x + p.y * 0.4), 3.0);
    col += sheen * 0.025 * accent * (0.4 + 1.2 * aud * lvl);

    // bass tick — a single horizontal accent bar far back (not a bar viz)
    float bk = bass * aud;
    float bandY = -0.46;
    float bandSdf = sdRect(vec2(p.x, p.y - bandY), vec2(0.32*asp, 0.004 + 0.006*bk));
    float bfw = fwidth(bandSdf);
    float bandMask = 1.0 - smoothstep(-bfw, bfw, bandSdf);
    col = mix(col, mix(col, accent, 0.55), bandMask * smoothstep(0.05, 0.35, bk));

    // gentle filmic compress + paper grain finish
    col = col / (1.0 + 0.18 * col);
    col = pow(max(col, 0.0), vec3(0.96));

    gl_FragColor = vec4(col, 1.0);
}
