/*{
  "DESCRIPTION": "Standing Wave — Faraday-style cymatics: rotated sets of standing plane waves interfere into nodal lattices (3- to 7-fold quasicrystal symmetries) that morph as the spectrum shifts — bass weights the coarse pattern, highs pull in a fine lattice — with sand-grain shimmer collecting along the nodal lines.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Cymatics",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "patternScale",
      "LABEL": "Pattern Scale",
      "TYPE": "float",
      "MIN": 6,
      "MAX": 30,
      "DEFAULT": 14,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "fineDetail",
      "LABEL": "Fine Lattice",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "nodeWidth",
      "LABEL": "Node Width",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 3,
      "DEFAULT": 1.2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "sandAmount",
      "LABEL": "Sand Grain",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "morphPeriod",
      "LABEL": "Morph Period (s)",
      "TYPE": "float",
      "MIN": 3,
      "MAX": 15,
      "DEFAULT": 7,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "GROUP": "Audio Reactivity",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6
    },
    {
      "NAME": "tintColor",
      "LABEL": "Tint",
      "TYPE": "color",
      "GROUP": "Color",
      "DEFAULT": [1.0, 1.0, 1.0, 1.0]
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "GROUP": "Color",
      "MIN": 0.2,
      "MAX": 3.0,
      "DEFAULT": 1.0
    }
  ]
}*/

// ════════════════════════════════════════════════════════════════════
//  Standing Wave — Faraday-wave quasicrystal cymatics.
//  NOT a Chladni plate: the field is a superposition of S standing
//  plane waves at evenly rotated angles,
//      F(p) = (1/S) · Σ_j cos( k · dot(p, dir_j) + φ_j )
//  which for S = 5, 7 produces the quasicrystalline lattices seen in
//  real Faraday-wave experiments. Symmetry count crossfades on a slow
//  clock (3→7-fold), phases drift, and the whole lattice slowly rotates
//  so the pattern is alive with zero audio.
//  Spectrum → geometry: the displayed field blends a COARSE lattice
//  (weight grows with bass) and a FINE lattice at ~3× the wavenumber
//  (weight grows with highs) — so bass music shows big bold cells and
//  bright music dissolves into a fine mesh. Sand grains twinkle along
//  the nodal lines (|F| ≈ 0), where real sand would collect.
//  Audio touches only blend weights and tone — never noise/domain args —
//  per the house anti-choppiness rules.
// ════════════════════════════════════════════════════════════════════

#define PI  3.14159265359
#define TAU 6.28318530718

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
float hash11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float hash21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// Superposition of S standing plane waves, evenly rotated.
// Phases are keyed on S so each symmetry variant is stable, and drift
// slowly on TIME only.
float fieldSum(vec2 p, float S, float k, float rot, float t) {
    float f = 0.0;
    for (int j = 0; j < 8; j++) {
        float fj = float(j);
        if (fj > S - 0.5) break;
        float a = rot + PI * fj / S;
        vec2 d = vec2(cos(a), sin(a));
        float ph = TAU * hash11(fj * 9.131 + S * 0.717)
                 + 0.30 * sin(t * 0.09 + fj * 1.93);
        f += cos(k * dot(p, d) + ph);
    }
    return f / S;
}

void main() {
    vec2 uv = isf_FragNormCoord;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    vec2 p = uv - 0.5;
    p.x *= aspect;

    float ar      = clamp(audioReact, 0.0, 1.0);
    float bassP   = pow(knee(audioBass,  0.05, 0.85), 1.6);
    float midP    = pow(knee(audioMid,   0.08, 0.90), 1.3);
    float highP   = pow(knee(audioHigh,  0.10, 0.90), 1.2);
    float levelP  = knee(audioLevel, 0.05, 0.90);
    float brightP = knee(audioBrightness, 0.15, 0.85);

    // ── symmetry morph: crossfade between adjacent fold counts ───────
    float rot = TIME * 0.020 + 0.15 * sin(TIME * 0.043);
    float per = max(morphPeriod, 3.0);
    float idxF = TIME / per + ar * 0.4 * brightP;   // brightness nudges the morph
    float sA = 3.0 + mod(floor(idxF), 5.0);          // 3..7-fold
    float sB = 3.0 + mod(floor(idxF) + 1.0, 5.0);
    float u = fract(idxF);
    u = u * u * (3.0 - 2.0 * u);

    // ── coarse + fine lattices ───────────────────────────────────────
    float kC = patternScale * (1.0 + 0.05 * sin(TIME * 0.047));
    float kF = kC * (2.6 + 1.2 * fineDetail);
    float Fc = mix(fieldSum(p, sA, kC, rot, TIME),
                   fieldSum(p, sB, kC, rot, TIME), u);
    float Ff = mix(fieldSum(p, sA, kF, -rot * 0.7, TIME + 13.0),
                   fieldSum(p, sB, kF, -rot * 0.7, TIME + 13.0), u);

    // spectrum → geometry: bass weights the coarse cells, highs the mesh
    float wC = 0.62 + ar * 0.55 * bassP;
    float wF = (0.16 + 0.30 * fineDetail) + ar * (0.55 + 0.45 * fineDetail) * highP;
    float F = (wC * Fc + wF * Ff) / (wC + wF);

    // ── nodal lines (AA'd via screen-space derivative) ───────────────
    float aaw = max(fwidth(F), 1e-4);
    float nd  = abs(F) / aaw;
    float w0  = nodeWidth;
    float line = 1.0 - smoothstep(w0, w0 + 1.6, nd);
    float halo = (1.0 - smoothstep(w0 * 3.4, w0 * 3.4 + 2.0, nd)) - line;

    // ── sand grains twinkling on the nodes ───────────────────────────
    // fixed per-grain twinkle rates (hashed) — audio only scales amplitude
    vec2 gp = floor(gl_FragCoord.xy / 2.0);
    float h1 = hash21(gp);
    float h2 = hash21(gp + 7.7);
    float h3 = hash21(gp + 13.3);
    float gate = 1.0 - smoothstep(w0 * 3.0, w0 * 3.0 + 2.4, nd);
    float dens = step(1.0 - sandAmount * 0.55, h1);
    float tw = 0.5 + 0.5 * sin(TIME * (0.9 + 2.4 * h3) + h2 * TAU);
    float grain = gate * dens * (0.35 + 0.65 * tw);
    float grainAmp = 0.55 + ar * 1.10 * highP;

    // ── antinode glow — the parts of the surface that are oscillating ─
    float anti = smoothstep(0.45, 1.0, abs(F) * 1.35);
    float breath = 0.70 + 0.30 * sin(TIME * 0.6 + F * 2.0);

    // ── assemble ─────────────────────────────────────────────────────
    vec3 plate = vec3(0.030, 0.036, 0.052);
    vec3 sand  = vec3(0.93, 0.89, 0.80);
    vec3 antiC = vec3(0.10, 0.16, 0.30);

    vec3 col = plate;
    col += antiC * anti * (0.55 + 0.45 * breath) * (0.50 + ar * 0.90 * bassP);
    col = mix(col, sand * (0.85 + ar * 0.30 * midP), line);
    col += sand * halo * 0.18;
    col += sand * grain * grainAmp * 0.55;

    // sustained loudness lift — dips below 1 in silence
    col *= mix(1.0, 0.72 + 0.55 * levelP + 0.28 * bassP, ar);

    // vignette + floor (never fully black)
    vec2 q = uv - 0.5;
    col *= 1.0 - 0.28 * dot(q, q);
    col = max(col, vec3(0.010, 0.012, 0.016));

    gl_FragColor = vec4(col * tintColor.rgb * brightness, 1.0);
}
