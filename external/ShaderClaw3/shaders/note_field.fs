/*{
  "DESCRIPTION": "Note Field — a generative musical score painting itself: warm-white paper with faint staff-line groups, Miró-primary note dots (red/blue/green/yellow/black) stamped on a rhythmic grid, joined into little phrases by short beams, stems and slur arcs. A living subset of dots hops between adjacent staff lines with eased leaps and every dot drifts gently along its line — mids and beats wind the hop clock so the music visibly flows through the score. A writing head fills the page, then gently re-writes it column by column. Beats stamp new notes, bass swells the freshest marks, mids sweep a playhead shimmer, highs add tiny black tick accents. No glyphs — only dots, bars and arcs.",
  "CREDIT": "Easel original — A-List batch 2 (datadots lineage).",
  "CATEGORIES": ["Generator", "Geometry", "Audio"],
  "INPUTS": [
    { "NAME": "paperColor",  "LABEL": "Paper",         "TYPE": "color", "DEFAULT": [0.964, 0.952, 0.928, 1.0], "GROUP": "Color" },
    { "NAME": "inkColor",    "LABEL": "Ink",           "TYPE": "color", "DEFAULT": [0.070, 0.065, 0.070, 1.0], "GROUP": "Color" },
    { "NAME": "noteColorA",  "LABEL": "Note Red",      "TYPE": "color", "DEFAULT": [0.800, 0.110, 0.100, 1.0], "GROUP": "Color" },
    { "NAME": "noteColorB",  "LABEL": "Note Blue",     "TYPE": "color", "DEFAULT": [0.130, 0.270, 0.660, 1.0], "GROUP": "Color" },
    { "NAME": "paletteShift","LABEL": "Palette Shift", "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3,  "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "density",     "LABEL": "Note Density",  "TYPE": "float", "MIN": 0.25, "MAX": 1.0, "DEFAULT": 0.72, "GROUP": "Shape / Geometry" },
    { "NAME": "noteScale",   "LABEL": "Note Size",     "TYPE": "float", "MIN": 0.6,  "MAX": 1.6, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "staffGroups", "LABEL": "Staff Groups",  "TYPE": "float", "MIN": 6.0,  "MAX": 16.0,"DEFAULT": 11.0, "GROUP": "Shape / Geometry" },
    { "NAME": "writeSpeed",  "LABEL": "Write Speed",   "TYPE": "float", "MIN": 0.2,  "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Audio Reactivity" }
  ],
  "PASSES": [
    { "TARGET": "sbuf", "PERSISTENT": true },
    {}
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// NOTE FIELD — the page is a rhythmic grid: NC columns × (staffGroups
// groups × 13 vertical slots). Content is ANALYTIC: every mark is hashed
// from (column, slot, group, lap), so nothing relies on decay buffers.
// Compact state (bottom row of sbuf) — ALL values packed into [0,1] with
// per-frame increments > 1/255, because web-host persistent buffers can
// fall back to 8-bit (the packing-quanta lesson):
//   texel (0,0): head column / 64, head fraction 0..1, lap / 16
//   texel (1,0): mid-accumulated playhead boost phase 0..1
// Beats speed the head (audioBeatPulse-gated), so new notes are STAMPED
// on hits; column age = head distance, rendered analytically:
//   pop-in  exp(-age*2.2), bass swell exp(-age*0.55) on the fresh columns.
// Idle floor: seeded head position = full page from frame 0 — a composed
// static score, never blank paper. All audio gain is display-side.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define ASP (RENDERSIZE.x / RENDERSIZE.y)
#define PI  3.1415926535
#define NC  44.0
#define MRG 0.045

float hash11(float p) { p = fract(p * 0.1031); p *= p + 33.33; p *= p + p; return fract(p); }
float hash21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// capsule distance in aspect-corrected space
float sdCap(vec2 q, vec2 a, vec2 b) {
    vec2 sc = vec2(ASP, 1.0);
    vec2 pa = (q - a) * sc, ba = (b - a) * sc;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-8), 0.0, 1.0);
    return length(pa - ba * h);
}
float dIso(vec2 a, vec2 b) { return length((a - b) * vec2(ASP, 1.0)); }

void main() {
    float amt   = clamp(audioReact, 0.0, 1.0);
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float levP  = clamp(smoothstep(0.04, 0.85, audioLevel), 0.0, 1.0);

    // ───────── PASS 0 — compact state (bottom row) ─────────
    if (PASSINDEX == 0) {
        ivec2 ip = ivec2(gl_FragCoord.xy);
        if (ip.y == 0 && ip.x < 2) {
            vec4 s0 = texture2D(sbuf, (vec2(0.0, 0.0) + 0.5) / R);
            vec4 s1 = texture2D(sbuf, (vec2(1.0, 0.0) + 0.5) / R);
            // decode [0,1]-packed state (8-bit-buffer safe)
            float pInt = floor(s0.x * 64.0 + 0.5);
            float pFrac = s0.y;
            float lap = floor(s0.z * 16.0 + 0.5);
            float phM = s1.x;
            if (FRAMEINDEX < 4) { pInt = 0.0; pFrac = 0.0; lap = 1.0; phM = 0.30; }
            // Writing head: slow authored crawl + beat stamps + bass push.
            // pFrac lives in [0,1) with increments > 1/255 so accumulation
            // never stalls even in an 8-bit persistent buffer.
            float colRate = writeSpeed *
                (0.35 + amt * (3.2 * beatP * beatP + 0.8 * bassP));
            pFrac += colRate * 0.0166;
            if (pFrac >= 1.0) {
                pFrac -= 1.0; pInt += 1.0;
                if (pInt >= NC) { pInt = 0.0; lap = mod(lap + 1.0, 16.0); }
            }
            // Playhead boost: mids + level accumulate extra sweep phase, so
            // the beam's VELOCITY tracks the envelope (the idle drift itself
            // is TIME-analytic in the display pass).
            phM = fract(phM + 0.0166 * amt * (2.2 * midP + 1.2 * levP));
            // Hop accumulator: mids + beats wind the migrating dots' hop
            // clock forward — more music, more dots hopping between lines.
            // Wrap-safe: consumers scale it by 4 and read it mod 4.
            float phH = s1.y;
            if (FRAMEINDEX < 4) phH = 0.10;
            phH = fract(phH + 0.0166 * amt * (1.8 * midP + 2.4 * beatP * beatP) * 0.9);
            if (ip.x == 0) gl_FragColor = vec4(pInt / 64.0, pFrac, lap / 16.0, 1.0);
            else           gl_FragColor = vec4(phM, phH, 0.0, 1.0);
            return;
        }
        gl_FragColor = vec4(0.0);
        return;
    }

    // ───────── PASS 1 — the page, rendered analytically ─────────
    vec4 s0 = texture2D(sbuf, (vec2(0.0, 0.0) + 0.5) / R);
    vec4 s1 = texture2D(sbuf, (vec2(1.0, 0.0) + 0.5) / R);
    float pInt = floor(s0.x * 64.0 + 0.5);
    float lap = floor(s0.z * 16.0 + 0.5);
    float hp = pInt + s0.y;               // continuous head position (cols)
    float ph = fract(TIME * 0.06 + s1.x); // analytic idle sweep + mid boost

    vec2 uv = gl_FragCoord.xy / R;
    float aa = 1.5 / R.y;

    // paper: warm white, soft vignette, static tooth grain (no idle churn)
    vec3 col = paperColor.rgb;
    vec2 cq = uv - 0.5;
    col *= 1.04 - 0.11 * dot(cq, cq) * 4.0 * 0.25;
    col += (hash21(floor(gl_FragCoord.xy * 0.71)) - 0.5) * 0.028;

    float G = floor(staffGroups + 0.5);
    float span = 1.0 - 2.0 * MRG;
    // level-proportional page micro-drift (display-only): the whole score
    // breathes a few pixels with the envelope — silence -> perfectly still
    vec2 puv = uv + vec2(-0.0022 * amt * midP,
                          0.0035 * amt * (0.45 * levP + 0.55 * bassP));
    float xN = (puv.x - MRG) / span;                // 0..1 across page
    float gy = (puv.y - MRG) / span;                // 0..1 down page
    bool onPage = (xN >= 0.0 && xN < 1.0 && gy >= 0.0 && gy < 1.0);

    float lg = 0.09;                                // line gap (group-local)
    float hg = 0.045;                               // slot gap = lg/2
    float groupH = span / G;                        // group height in uv

    vec3 ink = inkColor.rgb;

    if (onPage) {
        float gI = floor(gy * G);
        float v  = fract(gy * G);

        // ── faint staff lines: 5 per group, v = 0.32 .. 0.68 ──
        float rel = (v - 0.32) / lg;
        float k = clamp(floor(rel + 0.5), 0.0, 4.0);
        float dLpx = abs(rel - k) * lg * groupH * R.y;
        float lineA = smoothstep(1.4, 0.35, dLpx);
        // staff ink darkens a touch with the envelope (full-width response)
        col = mix(col, ink, lineA * (0.20 + 0.12 * amt * levP));

        // ── the note grid ──
        float cF = floor(xN * NC);
        float jF = floor((v - 0.23) / hg + 0.5);    // nearest slot 0..12
        float cw = span / NC;                       // column width (uv x)
        float rb = groupH * lg * 0.62 * noteScale;  // base dot radius (uv y)

        // level deepens the pigments (display-side, envelope-tracking)
        float deep = 1.0 - 0.32 * amt * levP;

        vec3 colG = vec3(0.075, 0.500, 0.270);
        vec3 colY = vec3(0.950, 0.760, 0.080);

        // playhead x (needed by the hoppers below — dots surge near the beam)
        float xph = MRG + ph * span;
        // audio-wound hop clock (×4, consumed mod 4 → wrap-safe)
        float phH4 = 4.0 * s1.y;
        float slotH = span * hg / G;              // one slot height in uv

        for (int dc = -3; dc <= 0; dc++) {
            for (int dj = -2; dj <= 2; dj++) {
                float ccl = cF + float(dc);
                float jj  = jF + float(dj);
                if (ccl < -0.5 || ccl > NC - 0.5) continue;
                if (jj < -0.5 || jj > 12.5) continue;

                // seed generation: columns behind the head were written
                // this lap, columns ahead of it last lap.
                float lapC = (ccl <= pInt + 0.5) ? lap : lap + 15.0;
                lapC = mod(lapC, 16.0);
                vec2 sd = vec2(ccl + lapC * 57.0, jj * 7.0 + gI * 131.0);

                // presence: staff-weighted rhythmic grid + phrase clumping
                float onLine = (jj >= 1.5 && jj <= 10.5)
                             ? (mod(jj, 2.0) < 0.5 ? 1.0 : 0.55) : 0.22;
                float colMul = 0.65 + 0.7 * hash21(vec2(ccl * 3.1 + lapC, gI * 17.0));
                if (hash21(sd) > density * 0.62 * onLine * colMul) continue;

                // column age in columns since the head stamped it
                float age = mod(hp - ccl, NC);
                float reveal = smoothstep(0.0, 0.22, age);
                if (reveal <= 0.001) continue;
                // gentle wipe: oldest columns fade just before rewrite
                float fadeO = 1.0 - 0.55 * smoothstep(NC - 4.0, NC - 0.6, age);

                // Miró palette pick (paletteShift rotates the wheel)
                float hcol = fract(hash21(sd + 3.3) + paletteShift);
                vec3 ncol = ink;
                if (hcol > 0.34)
                    ncol = (hcol < 0.52) ? noteColorA.rgb
                         : (hcol < 0.70) ? noteColorB.rgb
                         : (hcol < 0.86) ? colG : colY;
                ncol *= deep;

                // size: variation + arrival pop + bass swell on fresh notes
                float h4 = hash21(sd + 9.1);
                float scl = (0.78 + 0.50 * h4)
                          * (1.0 + 0.45 * exp(-age * 2.2)
                                 + amt * 0.85 * bassP * exp(-age * 0.55))
                          * (1.0 + 0.20 * amt * bassP
                                 + 0.05 * sin(TIME * 0.9 + h4 * 6.2832));
                float r = min(rb * scl, rb * 2.1);

                // anchor position (x jitters inside the cell, y snaps)
                vec2 p0 = vec2(MRG + (ccl + 0.5 + (hash21(sd + 5.5) - 0.5) * 0.45) * cw,
                               MRG + span * (gI + 0.5 + (jj - 6.0) * hg) / G);

                // ── living dots: a hashed subset migrates between adjacent
                // lines with eased hops (line above → home → line below → home),
                // and every dot drifts gently along its line. Music flows
                // through: dots near the sweeping playhead hop bigger, and the
                // audio-wound clock (phH4) makes hops come faster with mids
                // and beats.
                float hmg = hash21(sd + 21.7);
                float mig = step(hmg, 0.40);          // ~40% of dots migrate
                float hopSp = 0.13 + 0.24 * hash21(sd + 27.1);
                float phHop = TIME * hopSp + hash21(sd + 31.7) * 8.0 + phH4;
                float stq = floor(phHop) + smoothstep(0.18, 0.82, fract(phHop));
                float lvl = abs(mod(stq, 4.0) - 2.0) - 1.0;   // eased -1..1 staircase
                float nearPh = exp(-pow((p0.x - xph) * ASP / 0.10, 2.0));
                float hopAmp = mig * (0.90 + 0.10 * levP
                                      + 0.30 * amt * (0.5 * midP + 0.8 * beatP) * nearPh);
                p0.y += lvl * hopAmp * slotH;
                p0.x += cw * (0.30 * mig + 0.10)
                      * sin(TIME * (0.35 + 0.45 * h4) + h4 * 6.2832);

                float alpha = reveal * fadeO;
                float h2 = hash21(sd + 7.7);

                // dot at the anchor, always
                float dDot = dIso(puv, p0) - r;
                col = mix(col, ncol, smoothstep(aa, -aa, dDot) * alpha);

                if (h2 < 0.58) {
                    // plain dot — done
                } else if (h2 < 0.78) {
                    // beam: thick bar to a partner dot 2-3 columns right
                    float L = 2.0 + floor(hash21(sd + 11.3) * 1.999);
                    if (ccl + L < NC - 0.5) {
                        vec2 p1 = p0 + vec2(L * cw, 0.0);
                        float dB = sdCap(puv, p0, p1) - r * 0.55;
                        col = mix(col, ncol, smoothstep(aa, -aa, dB) * alpha);
                        float dD2 = dIso(puv, p1) - r * 0.9;
                        col = mix(col, ncol, smoothstep(aa, -aa, dD2) * alpha);
                    }
                } else if (h2 < 0.88) {
                    // stem: short vertical stroke up two slots
                    if (jj < 10.5) {
                        vec2 p1 = p0 + vec2(0.0, 2.0 * hg * groupH);
                        float dV = sdCap(puv, p0, p1) - r * 0.38;
                        col = mix(col, ncol, smoothstep(aa, -aa, dV) * alpha);
                        float dD2 = dIso(puv, p1) - r * 0.75;
                        col = mix(col, ncol, smoothstep(aa, -aa, dD2) * alpha);
                    }
                } else {
                    // slur arc: thin half-ring joining two dots 2 cols apart
                    if (ccl + 2.0 < NC - 0.5) {
                        vec2 p1 = p0 + vec2(2.0 * cw, 0.0);
                        vec2 m  = (p0 + p1) * 0.5;
                        float r0 = dIso(p0, m);
                        float dA = abs(dIso(puv, m) - r0) - r * 0.30;
                        float up = step(m.y, uv.y);
                        col = mix(col, ncol, smoothstep(aa, -aa, dA) * up * alpha);
                        float dD2 = dIso(puv, p1) - r * 0.8;
                        col = mix(col, ncol, smoothstep(aa, -aa, dD2) * alpha);
                    }
                }

                // tiny black tick accents — highs bring them out
                if (hash21(sd + 15.9) < 0.24) {
                    vec2 pt = p0 + vec2(0.0, hg * groupH * 1.1);
                    float dT = dIso(puv, pt) - r * 0.30;
                    float tickA = (0.18 + amt * 1.25 * highP) * alpha;
                    col = mix(col, ink, smoothstep(aa, -aa, dT) * clamp(tickA, 0.0, 1.0));
                }
            }
        }

        // ── playhead shimmer: a soft bright band sweeping left→right ──
        float bx = (puv.x - xph) * ASP;
        float bw = 0.021 * (1.0 + 0.7 * amt * midP);
        float band = exp(-bx * bx / (bw * bw));
        float shimmer = band * (0.20 + amt * 1.10 * midP);
        col = mix(col, vec3(1.0), clamp(shimmer, 0.0, 0.9) * 0.5);
    }

    // hairline page frame
    {
        vec2 b = abs(uv - 0.5) - vec2(0.5 - MRG * 0.55);
        float dF = max(b.x * ASP, b.y);
        float fA = smoothstep(aa * 1.2, 0.0, abs(dF)) * 0.0;
        col = mix(col, ink, fA);
    }

    // brightness with soft compression — no slider white-out on paper
    vec3 outc = col * brightness / (vec3(1.0) + max(brightness - 1.0, 0.0) * col);
    gl_FragColor = vec4(clamp(outc, 0.0, 1.0), 1.0);
}
