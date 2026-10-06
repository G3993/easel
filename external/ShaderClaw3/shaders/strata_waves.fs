/*{
  "DESCRIPTION": "Strata Waves — stacked horizontal strata of sliced-outline ribbon topography surging in big smooth waves: dozens of laminated paper layers, each a vivid stratum (red/green/blue/pink/yellow/cream) carrying repeated lighter outline copies below its scalloped edge and slope-combed slice ribs, over a light neutral gray paper background. Per pixel a constant stack of wave height functions is evaluated; the topmost covering stratum paints, the next edge below casts a soft paper shadow. Bass swells the wave amplitude, mids roll the wave phase (content velocity rides the envelope via an accumulated clock), highs brighten the outline rims. Reads as thick layered paper relief, never plasma.",
  "CREDIT": "ShaderClaw3 — A-List batch 2.",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    { "NAME": "colorA",       "LABEL": "Paper",           "TYPE": "color", "DEFAULT": [0.80, 0.81, 0.82, 1.0], "GROUP": "Color" },
    { "NAME": "colorB",       "LABEL": "Strata Tint",     "TYPE": "color", "DEFAULT": [1.0, 1.0, 1.0, 1.0],    "GROUP": "Color" },
    { "NAME": "paletteShift", "LABEL": "Palette Shift",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",      "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "strata",       "LABEL": "Strata Count",    "TYPE": "float", "MIN": 6.0, "MAX": 18.0,"DEFAULT": 15.0, "GROUP": "Shape / Geometry" },
    { "NAME": "waveAmp",      "LABEL": "Wave Swell",      "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "ribDetail",    "LABEL": "Slice Ribs",      "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "reliefDepth",  "LABEL": "Relief Outlines", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "driftSpeed",   "LABEL": "Wave Roll",       "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "waveSway",     "LABEL": "Wave Sway",       "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Motion / Animation" },
    { "NAME": "swellBreathe", "LABEL": "Swell Breathe",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Motion / Animation" },
    { "NAME": "ribCrawl",     "LABEL": "Rib Crawl",       "TYPE": "float", "MIN": -2.0,"MAX": 2.0, "DEFAULT": 0.0,  "GROUP": "Motion / Animation" },
    { "NAME": "rimFlow",      "LABEL": "Rim Flow",        "TYPE": "float", "MIN": -2.0,"MAX": 2.0, "DEFAULT": 0.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Audio Reactivity" }
  ],
  "PASSES": [
    { "TARGET": "stateBuf", "PERSISTENT": true },
    {}
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// STRATA WAVES — laminated paper topography, fully analytic.
//   Per pixel, evaluate N wave-edge height functions e_k(x) (constant loop).
//   The stack paints back-to-front: the LAST stratum whose edge sits above
//   the pixel owns it (later strata overpaint). Inside a stratum:
//     - slice ribs: dense rounded columns whose phase is sheared by the
//       local wave slope, so the lamination combs around every surge (3D).
//     - repeated outline copies: periodic lighter echoes of the edge fading
//       with depth — the onion-sliced relief of the reference.
//     - paper shadow: the nearest edge BELOW the pixel (a stratum in front)
//       casts a soft AO band upward — thick paper, not plasma.
//   Waves travel via an ACCUMULATED clock (state texel 0,0) whose rate rides
//   the envelope: mids roll the phase, so content velocity tracks the music.
//   Bass swells amplitude; highs brighten the outline rims. Idle floor:
//   audio 0 -> the full stack gently rolling (authored look, never empty).
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define NMAX 18

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

// per-stratum wave edge: 3 traveling sines + a slow cubed surge that lets
// single strata occasionally swallow their neighbours (the big mountains).
// PH is a 2pi-WRAPPED clock, so every PH multiplier is an INTEGER harmonic
// (wrap-seamless); speed variety comes from harmonic choice and sign.
float stratEdge(float k, float x, float PH, float spacing, float ampMul) {
    float h1 = hash11(k * 17.31 + 3.7);
    float h2 = hash11(k * 9.77 + 11.3);
    float h3 = hash11(k * 23.9 + 5.1);
    float n1 = 1.0 + floor(h3 * 1.999);            // 1..2
    float n2 = (h2 > 0.5 ? 2.0 : 3.0);
    float n3 = (h1 > 0.5 ? 1.0 : -2.0);
    float w = 0.55 * sin(x * 6.2832 * (0.8 + 0.9 * h1) + 6.2832 * h2 + PH * n1)
            + 0.30 * sin(x * 6.2832 * (1.6 + 1.5 * h2) - 6.2832 * h1 - PH * (n2 - 1.0))
            + 0.15 * sin(x * 6.2832 * (2.8 + 2.0 * h3) + 6.2832 * h3 - PH * n3);
    // soft-clip: flat-topped plateaus instead of pure sine mountains
    w = w * 1.45 / (1.0 + 0.65 * abs(w));
    float surge = pow(0.5 + 0.5 * sin(PH + 6.2832 * h1), 3.0);
    float A = spacing * (0.55 + 2.3 * surge) * ampMul;
    return 1.01 - (k + 0.6) * spacing + A * w;
}

// vivid laminated-paper palette, hash-shuffled per stratum
vec3 stratCol(float k) {
    float m = floor(hash11(k * 7.31 + 2.7) * 7.999);
    vec3 c;
    if      (m < 0.5) c = vec3(0.88, 0.10, 0.16);   // red
    else if (m < 1.5) c = vec3(0.95, 0.40, 0.06);   // orange
    else if (m < 2.5) c = vec3(0.98, 0.75, 0.10);   // yellow
    else if (m < 3.5) c = vec3(0.08, 0.56, 0.25);   // green
    else if (m < 4.5) c = vec3(0.13, 0.36, 0.86);   // blue
    else if (m < 5.5) c = vec3(0.98, 0.62, 0.78);   // pink
    else if (m < 6.5) c = vec3(0.96, 0.91, 0.78);   // cream
    else              c = vec3(0.78, 0.85, 0.95);   // pale blue
    return c * (0.92 + 0.16 * hash11(k * 31.7 + 9.9));
}

// reacts: movement, structure, energy, palette
// emphasis: structure (the whole relief surges and rolls with the music)
void main() {
    float amt = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelS = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ───────── PASS 0 — accumulated wave clock (state texel 0,0) ─────────
    if (PASSINDEX == 0) {
        if (gl_FragCoord.x < 1.0 && gl_FragCoord.y < 1.0) {
            // The clock is stored as a COS/SIN pair, decoded with atan and
            // consumed via integer harmonics only. A raw growing float in a
            // half-float persistent buffer inflates (round-up drift scales
            // with magnitude — measured in the web harness); a unit vector
            // keeps the representable quantum tiny and the angle exact.
            vec4 s = texture2D(stateBuf, vec2(0.5, 0.5) / R);
            float ang = atan(s.y, s.x);
            if (FRAMEINDEX < 4) ang = 1.7;
            // idle roll (authored motion in silence) + envelope-rate phase:
            // mids roll the waves, bass and level push the swell forward —
            // per-frame change proportional to the music itself.
            ang += driftSpeed * 0.0022
                 + amt * (0.045 * midP + 0.022 * bassP + 0.014 * levelS);
            gl_FragColor = vec4(cos(ang), sin(ang), 0.0, 1.0);
        } else {
            gl_FragColor = vec4(0.0);
        }
        return;
    }

    // ───────── PASS 1 — image (fully analytic paper stack) ─────────
    vec4 st = texture2D(stateBuf, vec2(0.5, 0.5) / R);
    float PH = atan(st.y, st.x);   // (-pi, pi] — integer harmonics wrap clean

    float x  = gl_FragCoord.x / R.x;
    float py = gl_FragCoord.y / R.y;
    // display-only sway: the whole artwork breathes with the mids
    // (memoryless, silence -> exactly zero offset)
    x  += amt * 0.010 * midP * sin(py * 5.0 + TIME * 0.4);
    py += amt * 0.016 * midP;
    // MOTION: lateral sway — the whole stack shears side to side by depth (memoryless)
    x  += waveSway * 0.035 * sin(py * 4.0 + TIME * 0.35);

    float strataN = floor(strata + 0.5);
    float spacing = 1.22 / strataN;
    // bass swells the wave amplitude (soft knee, headroom kept)
    float ampMul = waveAmp * (1.0 + amt * 1.10 * bassP);

    float kv = -1.0, edist = 0.0, slope = 0.0, shadowD = 9.0;
    vec3 baseCol = vec3(0.0);
    float kh = 0.0;

    for (int k = 0; k < NMAX; k++) {
        float fk = float(k);
        if (fk >= strataN) break;
        // MOTION: per-stratum swell breathing — each layer heaves on its own phase
        float ampK = ampMul * (1.0 + swellBreathe * 0.45 * sin(TIME * 0.45 + fk * 1.7));
        float e = stratEdge(fk, x, PH, spacing, ampK);
        if (e >= py) {
            float e2 = stratEdge(fk, x + 0.004, PH, spacing, ampK);
            kv = fk;
            edist = e - py;
            slope = (e2 - e) / 0.004;
            baseCol = stratCol(fk);
            kh = hash11(fk * 12.9 + 4.4);
            shadowD = 9.0;               // only strata IN FRONT cast shadow
        } else {
            shadowD = min(shadowD, py - e);
        }
    }

    vec3 col;
    if (kv < -0.5) {
        // paper background: light neutral gray with the faintest grain
        col = colorA.rgb * (0.99 + 0.02 * hash21(gl_FragCoord.xy));
    } else {
        col = baseCol;

        // slice ribs: rounded lamination columns, phase sheared by the wave
        // slope so they comb around every surge; stronger where it curves.
        float rib = sin(x * 6.2832 * 68.0 * ribDetail + slope * 16.0 + kh * 6.2832
                        + TIME * ribCrawl * 2.0 * (kh > 0.5 ? 1.0 : -1.0));   // MOTION: lamination lines crawl
        float rr  = pow(0.5 + 0.5 * rib, 1.5);
        float ribA = 0.15 + 0.85 * smoothstep(0.0, 1.2, abs(slope));
        col *= mix(1.0, mix(0.86, 1.10, rr), ribA * (0.85 + amt * 0.35 * levelS));

        // repeated lighter outline copies below the edge (onion-sliced relief)
        float o   = edist / (0.0135 * (0.6 + 0.4 * spacing / 0.075));
        float cop = pow(0.5 + 0.5 * cos((o + TIME * rimFlow * 0.35) * 6.2832), 6.0);   // MOTION: outline echoes flow
        float rim = cop * exp(-o * 0.28) * reliefDepth;
        // highs brighten the rims (sparkle lives on the fine detail)
        rim *= 0.55 * (1.0 + amt * 1.5 * highP);
        col = mix(col, mix(baseCol, vec3(1.0), 0.55), clamp(rim, 0.0, 0.85));

        // crisp bright lip right at the top edge
        float lip = 1.0 - smoothstep(0.0, 0.006, edist);
        col = mix(col, mix(baseCol, vec3(1.0), 0.72), lip * 0.85);

        // soft paper shadow cast up from the next stratum's edge below
        float sh = exp(-shadowD / 0.020);
        col *= 1.0 - 0.32 * sh;

        // strata tint (default white = reference look; recolors cleanly)
        col *= colorB.rgb;

        // gentle vibrance breathing with the music (display-only, bounded)
        float lum = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lum), col, 1.0 + amt * (0.30 * midP + 0.18 * bassP)), 0.0, 1.0);
    }

    // small display gain on a light field — soft-compressed, cannot white out
    float gainF = 1.0 + 0.10 * clamp(audioMid, 0.0, 1.0)
                      + 0.06 * clamp(audioBass, 0.0, 1.0)
                      + amt * 0.08 * beatP;
    col = col * gainF / (1.0 + 0.45 * (gainF - 1.0) * col);

    // brightness with soft shoulder
    col = col * brightness / (1.0 + 0.35 * (brightness - 1.0) * col);

    // ---- universal color block (default = no-op) ----
    if (paletteShift > 0.0005) {
        float hA = paletteShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        col = clamp(hM * col, 0.0, 1.0);
    }

    gl_FragColor = vec4(col, 1.0);
}
