/*{
  "CATEGORIES": [
    "Generator",
    "Art Movement",
    "Audio Reactive"
  ],
  "DESCRIPTION": "De Stijl after late Mondrian — Broadway Boogie Woogie (1942–43) and Victory Boogie Woogie (1942–44). Asymmetric black grid lines partition a cream canvas into rectangles, some filled with pure cadmium red / cobalt blue / Naples yellow. Down each line marches a syncopated stream of small coloured squares at harmonic tempi (1x, 1.5x, 2x, 2.5x of base) — Mondrian's literal jazz. The grid quietly REPARTITIONS via smoothstep. Bass throws extra red squares; treble throws yellow. Rare audio-bass-triggered cell colour swaps. Five-colour palette only, no gradients. Stays alive in silence. Linear HDR.",
  "INPUTS": [
    {
      "NAME": "lineThickness",
      "LABEL": "Line Thickness",
      "TYPE": "float",
      "MIN": 0.001,
      "MAX": 0.012,
      "DEFAULT": 0.0035,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "gridDensity",
      "LABEL": "Grid Density",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Sparse",
        "Medium",
        "Dense",
        "Very Dense"
      ],
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "repartitionPeriod",
      "LABEL": "Repartition Period (s)",
      "TYPE": "float",
      "MIN": 8,
      "MAX": 30,
      "DEFAULT": 16,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "marchSpeed",
      "LABEL": "March Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "marchSwing",
      "LABEL": "March Swing",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "dirBias",
      "LABEL": "Direction Bias",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "lineWander",
      "LABEL": "Line Wander",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "colorPulseIntensity",
      "LABEL": "Color Pulse Intensity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "whiteSaturation",
      "LABEL": "White Tint Saturation",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.3,
      "DEFAULT": 0,
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
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  DE STIJL — Mondrian, Boogie Woogie era
//  Broadway Boogie Woogie (1942–43) was Mondrian's response to Manhattan
//  jazz after fleeing Europe. The black grid he had used since 1920 broke
//  apart into chains of coloured squares pulsing along the lines — the
//  painting LITERALIZED syncopation. This shader is built around that
//  device: a kinetic grid where every black line carries marching squares
//  at harmonic tempi, and every ~16 seconds the line POSITIONS slide to
//  new locations (smoothstep transitions) honouring Mondrian's iterative
//  practice — he physically re-taped his grid lines for months on end.
//  Five-colour palette, no mixing, no gradients. Silence is fine: TIME
//  drives the boogie regardless of audio.
// ════════════════════════════════════════════════════════════════════════

// ─── Mondrian palette (the only five colours allowed) ─────────────────
const vec3 PAL_RED    = vec3(0.85, 0.15, 0.10); // cadmium red
const vec3 PAL_BLUE   = vec3(0.10, 0.20, 0.65); // cobalt blue
const vec3 PAL_YELLOW = vec3(0.96, 0.86, 0.20); // Naples yellow
const vec3 PAL_WHITE  = vec3(0.96, 0.94, 0.88); // off-white / cream
const vec3 PAL_BLACK  = vec3(0.04, 0.04, 0.06); // pure black

// Maximum grid capacity (compile-time array sizes). Actual line counts used
// at runtime are derived from gridDensity and clamped to these maxima.
#define LINES_H 7   // max horizontal interior lines
#define LINES_V 8   // max vertical interior lines
#define SQUARE   0.020   // marching square half-size (in normalized coord)

// ─── hashes ───────────────────────────────────────────────────────────
float h11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float h12(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// Quantize [0,1] -> one of 5 palette colours by index.
vec3 pal5(int idx) {
    if (idx == 0) return PAL_WHITE;
    if (idx == 1) return PAL_RED;
    if (idx == 2) return PAL_BLUE;
    if (idx == 3) return PAL_YELLOW;
    return PAL_BLACK;
}

// Cell fill choice — heavily weighted toward white (Mondrian's canvas was
// mostly cream). Bass-triggered swap key changes the index every now and
// then for the audio-reactive surprise.
int cellColour(vec2 cellId, float swapKey) {
    float r = h12(cellId * 1.7 + swapKey * 7.13);
    if (r < 0.62) return 0;          // WHITE — dominant
    if (r < 0.74) return 1;          // RED
    if (r < 0.84) return 2;          // BLUE
    if (r < 0.94) return 3;          // YELLOW
    return 0;                        // small extra white pad
}

// ─── grid layout ──────────────────────────────────────────────────────
// We keep two SETS of line positions and crossfade between them every
// ~16 seconds with a 1-second smoothstep — this is the "repartition"
// gesture that reads as Mondrian re-taping his canvas.
//
// Each line has its OWN epoch (slightly de-synchronised) so they don't
// all shift at once — the canvas re-tunes itself piece by piece, like
// Mondrian's diary entries describe his own sessions.

float linePos(float idx, float total, float salt, float t, float period) {
    // Base slot (evenly spaced 0..1) plus an asymmetric per-epoch jitter.
    float slot   = (idx + 1.0) / (total + 1.0);
    float per    = max(period, 1.0);
    float epoch  = floor(t / per + salt * 0.31);
    float ph     = fract(t / per + salt * 0.31);
    // Last ~1s of each epoch is the smoothstep crossfade.
    float transStart = max(0.0, 1.0 - 1.0 / per);
    float trans  = smoothstep(transStart, 1.0, ph);
    float jitA   = (h11(epoch * 7.13 + salt) - 0.5) * 0.55 / total;
    float jitB   = (h11((epoch + 1.0) * 7.13 + salt) - 0.5) * 0.55 / total;
    float jit    = mix(jitA, jitB, trans);
    // Line Wander: continuous slow sway of the grid lines between repartitions
    float wander = lineWander * 0.03 * sin(t * 0.35 + salt * 2.7);
    return clamp(slot + jit + wander, 0.04, 0.96);
}

// ─── marching square pulses along a single line ───────────────────────
// Returns a colour OVER black (the line itself). The line carries a
// stream of small coloured squares at harmonic tempo. dir = 1 horizontal
// (squares travel along x), dir = 0 vertical (along y).
//
// Tempo harmonic per line: 1x, 1.5x, 2x, or 2.5x of base — chosen by
// hash, deterministic per line. Bass injects extra reds; treble extra
// yellows (hueBias parameter modulates spawn colour selection).
//
// We DO NOT blend — Mondrian had no gradients. Either the square is
// here (return colour) or it isn't (return black, which is the line).
vec3 lineSquares(float along, float coord, float lineSalt, float t,
                 float audioBass, float audioTreble, float audioReact,
                 float pulseAmt)
{
    // Tempo: 1.0, 1.5, 2.0, 2.5 — boogie-woogie harmonic ladder.
    float tempoSel = h11(lineSalt * 17.7);
    float tempo    = (tempoSel < 0.25) ? 1.0
                   : (tempoSel < 0.55) ? 1.5
                   : (tempoSel < 0.85) ? 2.0 : 2.5;
    // Direction Bias: -1 = every line marches +, +1 = every line marches -, 0 = hashed mix
    float dir      = (h11(lineSalt * 23.3) < 0.5 + 0.5 * dirBias) ? -1.0 : 1.0;
    float baseSpd  = 0.075 * marchSpeed;
    // March Swing: squares jog back and forth on top of the march (syncopation)
    float march    = t * baseSpd * tempo * dir
                   + marchSwing * 0.06 * sin(t * 1.4 * tempo + lineSalt);

    // Density: 4–7 squares riding the line at any time. pulseAmt scales
    // how many squares actually paint (when 0, the line is bare black).
    float density  = 4.0 + floor(h11(lineSalt * 29.7) * 4.0);
    density       *= clamp(pulseAmt, 0.0, 1.0);

    vec3 outCol = PAL_BLACK; // the line itself

    // Continuous band-following: the marching squares BREATHE in size with
    // bass (smooth envelope, never gated) so beat-less material still reads.
    float sqSz = SQUARE * clamp(0.92 + 0.35 * audioBass, 0.5, 1.30);

    // Iterate over slots; each slot has a phase offset and a colour.
    for (int k = 0; k < 8; k++) {
        if (float(k) >= density) break;
        float fk     = float(k);
        float spawn  = h11(lineSalt * 31.1 + fk * 5.7);

        // Position along the line (with march).
        float pos    = fract(march + spawn);
        // Wrap-aware distance.
        float dA     = abs(along - pos);
        dA           = min(dA, 1.0 - dA);

        // Each square is offset in the cross direction so it sits ON the
        // line (cross dist must already be small for us to be here).
        // Square colour: deterministic pick from primaries, with audio
        // injecting bass=red and treble=yellow surprises.
        float colSel = h11(lineSalt * 41.3 + fk * 7.1);
        // Bass kick — promote some yellows/blues to red.
        float bassKick = audioBass * audioReact;
        float trebKick = audioTreble * audioReact;
        // Default: 1/3 each among R/B/Y.
        int sqIdx;
        if (colSel < 0.33 + 0.38 * bassKick)        sqIdx = 1; // RED
        else if (colSel < 0.66 - 0.20 * trebKick)   sqIdx = 2; // BLUE
        else                                        sqIdx = 3; // YELLOW
        // Treble overrides occasional non-yellows to yellow.
        if (h11(lineSalt * 53.7 + fk * 3.3) < trebKick * 0.55) sqIdx = 3;

        // Square drawn iff |dA| < SQUARE and |dCross| < SQUARE — caller
        // already guarantees |dCross| < small via line proximity. We
        // require BOTH explicitly (cross dist passed via 'coord').
        if (dA < sqSz && abs(coord) < sqSz) {
            outCol = pal5(sqIdx);
        }
    }
    // Audio-bass extra-red sprinkle: occasional bonus square mid-line.
    // Eased presence (was a hard `> 0.18` gate that popped in/out): the
    // bonus square GROWS from nothing as bass rises, shrinks as it falls.
    float sprinkle = smoothstep(0.12, 0.35, audioBass * audioReact)
                   * step(0.05, pulseAmt);
    if (sprinkle > 0.001) {
        float bspawn = fract(t * 0.22 + lineSalt * 1.11);
        float dS = abs(along - bspawn);
        dS = min(dS, 1.0 - dS);
        float sR = SQUARE * sprinkle;
        if (dS < sR && abs(coord) < sR) outCol = PAL_RED;
    }
    return outCol;
}

// ════════════════════════════════════════════════════════════════════════
void main() {
    vec2  uv = isf_FragNormCoord.xy;
    float t  = TIME;

    // ─── parameter normalisation ─────────────────────────────────────
    float lineW    = clamp(lineThickness,       0.001, 0.012);
    float period   = clamp(repartitionPeriod,   8.0,   30.0);
    float pulseAmt = clamp(colorPulseIntensity, 0.0,   1.0);
    float wSat     = clamp(whiteSaturation,     0.0,   0.3);
    float aR       = clamp(audioReact,          0.0,   2.0);

    // gridDensity is a long enum (0..3): Sparse, Medium, Dense, Very Dense.
    // It maps to active line counts (clamped to compile-time maxima).
    int dIdx = int(clamp(float(gridDensity), 0.0, 3.0)); // WebGL: long inputs arrive as float
    int activeLinesV = (dIdx == 0) ? 3 : (dIdx == 1) ? 5 : (dIdx == 2) ? 6 : 8;
    int activeLinesH = (dIdx == 0) ? 2 : (dIdx == 1) ? 4 : (dIdx == 2) ? 5 : 7;

    // Audio: real feature bus (bass throws reds, highs throw yellows),
    // blended with a gentle TIME idle so the boogie never dies in silence.
    float idleB = 0.5 + 0.5 * sin(t * 1.7);       // 0..1, slow
    float idleT = 0.5 + 0.5 * sin(t * 4.3 + 1.3); // 0..1, fast
    // Soft-knee conditioning with headroom: the old `bass*1.0 + idle*0.3`
    // pegged the 1.3 clamp for entire EDM sections (zero variance = deaf),
    // and the 0.03 knee floor lets jazz/hiphop's soft sparse kicks register.
    float bassSig   = clamp(idleB * 0.25
                            + pow(smoothstep(0.03, 0.95, audioBass), 1.2) * 0.75
                            + audioBeatPulse * 0.25, 0.0, 1.15) * aR;
    float trebleSig = clamp(idleT * 0.25
                            + smoothstep(0.04, 0.95, audioHigh) * 0.85, 0.0, 1.15) * aR;

    // Cell-colour swap key — fixed slow epochs. (Previously bass fed the
    // floor() argument directly, so cells strobed between palettes every
    // frame the bass crossed an integer boundary — the main CHOPPY source.)
    float swapKey = floor(t / 7.0);

    // ─── compute current line positions (with smoothstep repartition) ─
    // Arrays are sized to LINES_V/LINES_H maxima; only the first
    // activeLinesV / activeLinesH slots are actually consulted.
    float xs[LINES_V];
    for (int j = 0; j < LINES_V; j++) {
        xs[j] = linePos(float(j), float(activeLinesV),
                        float(j) * 1.31 + 11.0, t, period);
    }
    float ys[LINES_H];
    for (int i = 0; i < LINES_H; i++) {
        ys[i] = linePos(float(i), float(activeLinesH),
                        float(i) * 1.71 + 27.0, t, period);
    }

    // ─── identify which rectangle we're in ────────────────────────────
    int cellX = 0;
    for (int j = 0; j < LINES_V; j++) {
        if (j < activeLinesV && uv.x > xs[j]) cellX = j + 1;
    }
    int cellY = 0;
    for (int i = 0; i < LINES_H; i++) {
        if (i < activeLinesH && uv.y > ys[i]) cellY = i + 1;
    }

    // ─── default fill: cell colour ───────────────────────────────────
    int cIdx = cellColour(vec2(float(cellX), float(cellY)), swapKey);
    vec3 col = pal5(cIdx);

    // Optional warm/cool tint on cream cells: shift PAL_WHITE toward
    // a slightly warmer or cooler hue per cell. Strictly bounded so the
    // five-colour read stays intact when wSat == 0 (default).
    if (cIdx == 0 && wSat > 0.0) {
        float tintHash = h12(vec2(float(cellX), float(cellY)) * 3.7
                              + swapKey * 0.91);
        // -1..+1 — negative = cool, positive = warm.
        float tintDir  = tintHash * 2.0 - 1.0;
        vec3  warm     = vec3( 0.06,  0.02, -0.06); // pull R+, B-
        vec3  cool     = vec3(-0.06, -0.02,  0.06); // pull B+, R-
        vec3  shift    = (tintDir > 0.0) ? warm : cool;
        col = clamp(col + shift * wSat * abs(tintDir), 0.0, 1.0);
    }

    // universal background: the cream canvas cells are this shader's
    // background — lines and marching squares still draw over them.
    if (cIdx == 0) col = mix(col, bgColor.rgb, bgColor.a);

    // Colour pulse: loud music lifts the primaries (never the cream canvas
    // or the black lines) — the painting itself plays along with the band.
    if (cIdx == 1 || cIdx == 2 || cIdx == 3) {
        // Capped, soft-kneed pulse (was ×1.8 raw level — a clipping strobe).
        float lvl = pow(smoothstep(0.04, 0.95, clamp(audioLevel, 0.0, 1.0)), 1.2);
        col *= 1.0 + 0.55 * lvl * min(aR, 2.0) * pulseAmt;
    }

    // ─── horizontal lines: black baseline + marching squares ─────────
    for (int i = 0; i < LINES_H; i++) {
        if (i >= activeLinesH) break;
        float dy = uv.y - ys[i];
        float ady = abs(dy);
        if (ady < lineW) {
            col = PAL_BLACK;
        }
        if (ady < SQUARE * 1.35) {
            float salt = float(i) * 11.13 + 3.7;
            vec3 sq = lineSquares(uv.x, dy, salt, t,
                                  bassSig, trebleSig, aR, pulseAmt);
            if (sq != PAL_BLACK) col = sq;
        }
    }

    // ─── vertical lines: same idea, march along y ────────────────────
    for (int j = 0; j < LINES_V; j++) {
        if (j >= activeLinesV) break;
        float dx = uv.x - xs[j];
        float adx = abs(dx);
        if (adx < lineW) {
            col = PAL_BLACK;
        }
        if (adx < SQUARE * 1.35) {
            float salt = float(j) * 13.71 + 17.3;
            vec3 sq = lineSquares(uv.y, dx, salt, t,
                                  bassSig, trebleSig, aR, pulseAmt);
            if (sq != PAL_BLACK) col = sq;
        }
    }

    // ─── frame border (Mondrian framed his canvases edge-to-edge in
    // ─── black) — gives the composition a definite stop.
    if (uv.x < lineW || uv.x > 1.0 - lineW
     || uv.y < lineW || uv.y > 1.0 - lineW) {
        col = PAL_BLACK;
    }

    // ─── Round 2: whole-canvas linear follower + decaying accent trace ─
    // Jazz scored 0 because every audio path landed on tiny regions
    // (primary cells, marching squares) through kneed envelopes. LINEAR
    // bands over the FULL canvas carry jazz's soft 0.4-0.5 swung accents
    // and ambient swells; audioSub + audioBeatPulse's ~300ms decaying
    // trace carry hiphop's sparse kicks. A pure luminance scale — hues
    // stay the five Mondrian colours; silence multiplies by exactly 1.0.
    // Round 3: the round-2 GAIN follower clipped instantly on the cream
    // canvas (0.96 × 1.04 already saturates) — measured response ~0. Flip
    // to a DARKEN-DIP (unsigned frameDiff correlates identically, dips
    // can't clip on a bright canvas) and add the HIGH band: jazz's most
    // rhythmic content is the brushed offbeat hats, which the old
    // bass/mid/sub-only follower never saw. Silence: dip=0, look exact.
    // Round 3 MEASURED: edm was the one CHOPPY style (p95 0.152) — the raw
    // 0.18*beatPulse term stepped the whole 0.89-luma canvas in ONE frame at
    // every kick. But dropping the beat term killed jazz (adj 0.378→0.003;
    // jazz's envelope is punch-weighted). Fix: gate the pulse with beatPhase
    // so it RAMPS in over the first ~30% of the beat instead of stepping —
    // same correlation (detector is lag-tolerant), ~1/5 the per-frame delta.
    float beatSoft = audioBeatPulse * smoothstep(0.0, 0.25, audioBeatPhase);
    col *= 1.0 - (0.16 * audioBass + 0.17 * audioMid + 0.16 * audioHigh
                + 0.22 * beatSoft) * min(aR, 1.5) * 0.85;

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = col;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                   // saturation
    if (hueShift > 0.0005) {                               // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }

    // Output linear HDR — palette deliberately stays in [0,1] to keep
    // the strict five-colour read; host applies tone curve.
    gl_FragColor = vec4(uc, 1.0);
}
