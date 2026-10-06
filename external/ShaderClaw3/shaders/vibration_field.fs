/*{
  "DESCRIPTION": "Vibration Field — a taut surface visibly vibrating: travelling ripple wavefronts radiate from drifting emitters and interfere, moire shimmer blooming where fronts cross. Each emitter listens to its own band (bass/mid/high) so the surface trembles with the music; the summed field is lit as a height map so crests catch the light.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Physical",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "emitterCount",
      "LABEL": "Emitters",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 8,
      "DEFAULT": 5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "waveScale",
      "LABEL": "Wave Scale",
      "TYPE": "float",
      "MIN": 8,
      "MAX": 60,
      "DEFAULT": 26,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "waveSpeed",
      "LABEL": "Wave Speed",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 8,
      "DEFAULT": 2.6,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "driftAmount",
      "LABEL": "Emitter Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "shimmer",
      "LABEL": "Moire Shimmer",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55,
      "GROUP": "Shape / Geometry"
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
//  Vibration Field — travelling-wave interference from moving emitters.
//  Physically: h(p) = Σ w_i · sin(k·r_i − ωt + φ_i) / (1 + a·r_i)
//  Each emitter drifts on a Lissajous orbit and is weighted by one
//  audio band (i%3 → bass/mid/high) with its own idle pulse, so the
//  surface keeps breathing in silence and trembles with music.
//  The field is lit as a height map (screen-space derivatives) and a
//  fine contour carrier over the summed field produces moire shimmer
//  where wavefronts from different emitters genuinely overlap.
// ════════════════════════════════════════════════════════════════════

#define MAX_EM 8
#define TAU 6.28318530718

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
float hash11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
vec2  hash22(float n) { return vec2(hash11(n), hash11(n + 17.31)); }
float soft(float x)   { return x / (1.0 + abs(x)); }

// Lissajous drift, unique per emitter, never repeating.
vec2 emitterPos(float fi, float t, float amt) {
    vec2 seed = hash22(fi * 7.31 + 2.17);
    vec2 base = 0.20 + 0.60 * seed;
    float phx = hash11(fi * 3.71) * TAU;
    float phy = hash11(fi * 5.13) * TAU;
    vec2 lis = vec2(sin(t * 0.131 + phx) + 0.55 * sin(t * 0.293 + phx * 1.41),
                    cos(t * 0.113 + phy) + 0.55 * cos(t * 0.257 + phy * 1.73));
    return base + amt * 0.16 * lis;
}

void main() {
    vec2 uv = isf_FragNormCoord;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);

    float ar     = clamp(audioReact, 0.0, 1.0);
    float bassP  = pow(knee(audioBass,  0.05, 0.85), 1.6);
    float midP   = pow(knee(audioMid,   0.08, 0.90), 1.3);
    float highP  = pow(knee(audioHigh,  0.10, 0.90), 1.2);
    float levelP = knee(audioLevel, 0.05, 0.90);

    // Coherent micro-tremble of the whole surface (sub-pixel, sinusoidal —
    // NOT random jitter, so successive frames stay smooth).
    vec2 tremble = vec2(sin(TIME * 13.7), cos(TIME * 11.3))
                 * 0.0011 * (0.35 + 0.65 * levelP * ar);
    vec2 p = vec2(uv.x * aspect, uv.y) + tremble;

    float cnt = clamp(emitterCount, 1.0, 8.0);
    float k   = waveScale;
    float om  = waveSpeed * 3.0;

    float h = 0.0;      // summed field
    float esum = 0.0;   // total amplitude envelope
    float emax = 0.0;   // dominant emitter envelope (for crossing measure)
    float dots = 0.0;   // emitter markers

    for (int i = 0; i < MAX_EM; i++) {
        float fi = float(i);
        if (fi > cnt - 0.5) break;
        vec2 sp = emitterPos(fi, TIME, driftAmount);
        sp.x *= aspect;
        float r = max(distance(p, sp), 0.004);

        int b = i - 3 * (i / 3);
        float band = (b == 0) ? bassP : ((b == 1) ? midP : highP);

        // idle pulse (always alive) × audio band listener (phase-lagged by id)
        float w = 0.74 + 0.26 * sin(TIME * (0.43 + 0.31 * hash11(fi * 2.91)) + fi * 1.73);
        w *= mix(1.0, 0.55 + 0.90 * band, ar);

        float env = w / (1.0 + 5.5 * r);
        h    += env * sin(r * k - TIME * om + hash11(fi * 11.37) * TAU);
        esum += env;
        emax  = max(emax, env);

        // emitter marker — a soft pulsing dot at the source
        float dc = smoothstep(0.014, 0.0, r);
        float dh = smoothstep(0.050, 0.012, r) * 0.30;
        dots += (dc + dh) * w;
    }

    float hRaw = h;
    h /= max(esum * 0.55, 0.8);

    // Sustained audio amplitude — dips below 1 in silence so loud vs quiet
    // reads even on bright crests.
    float amp = mix(1.0, 0.68 + 0.62 * bassP + 0.30 * levelP, ar);
    h *= amp;
    float hv = soft(h * 1.6);

    // ── height-field lighting ─────────────────────────────────────────
    float g = RENDERSIZE.y * 0.05;
    vec3 N = normalize(vec3(-dFdx(h) * g, -dFdy(h) * g, 1.0));
    float lAng = 0.9 + 0.25 * sin(TIME * 0.05);
    vec3 L = normalize(vec3(cos(lAng) * 0.8, sin(lAng) * 0.8, 0.85));
    float diff = max(dot(N, L), 0.0);
    float spec = pow(max(dot(N, normalize(L + vec3(0.0, 0.0, 1.0))), 0.0), 42.0);

    // ── palette: trough violet → deep → steel blue → crest white ─────
    float t01 = clamp(0.5 + 0.5 * hv, 0.0, 1.0);
    vec3 troughC = vec3(0.30, 0.12, 0.42);
    vec3 deepC   = vec3(0.020, 0.030, 0.060);
    vec3 midC    = vec3(0.10, 0.22, 0.38);
    vec3 crestC  = vec3(0.78, 0.90, 1.00);
    vec3 col;
    if (t01 < 0.30)      col = mix(troughC, deepC, t01 / 0.30);
    else if (t01 < 0.60) col = mix(deepC, midC, (t01 - 0.30) / 0.30);
    else                 col = mix(midC, crestC, (t01 - 0.60) / 0.40);

    col *= 0.40 + 0.85 * diff;
    col += vec3(0.90, 0.95, 1.00) * spec * 0.45;

    // ── moire shimmer where fronts cross ──────────────────────────────
    // "cross" = fraction of local wave energy NOT owned by the dominant
    // emitter — near zero close to a lone source, high where several
    // wavefronts genuinely overlap.
    float cross = clamp((esum - emax) / max(esum, 1e-4), 0.0, 1.0);
    cross = cross * cross;
    float mo = 0.5 + 0.5 * sin(hRaw * (9.0 + 9.0 * shimmer) * 3.14159 + TIME * 1.1);
    float shimGain = shimmer * cross * (0.30 + 0.55 * mix(0.35, highP, ar));
    col += vec3(0.55, 0.75, 0.95) * mo * shimGain * 0.65;

    // emitter dots
    col += vec3(0.85, 0.93, 1.00) * dots * 0.55;

    // sustained loudness lift (dips below 1 in silence)
    col *= mix(1.0, 0.76 + 0.50 * levelP + 0.22 * bassP, ar);

    // vignette + floor (never fully black)
    vec2 q = uv - 0.5;
    col *= 1.0 - 0.30 * dot(q, q);
    col = max(col, vec3(0.012, 0.015, 0.022));

    gl_FragColor = vec4(col * tintColor.rgb * brightness, 1.0);
}
