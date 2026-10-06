/*{
  "CATEGORIES": [
    "Generator",
    "Light",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Aura Drift — soft dipole gradient orbs adrift on a grainy pastel field (Turrell-meets-aura). Six two-tone orbs breathe per stem (bass = biggest), drift on EaselAudio Time clocks, and get slowly re-choreographed by the 8-beat phase ramp. Color temperature follows audioBrightness; kicks bloom the halos. Stunning in silence, alive with music. Layers: orbs / halo bloom / grain / vignette + universal hueShift/colorBoost/bgColor/audioReactivity.",
  "INPUTS": [
    {
      "NAME": "orbGlow",
      "LABEL": "Orbs",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "haloBloom",
      "LABEL": "Halo Bloom",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "grainAmount",
      "LABEL": "Grain",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5
    },
    {
      "NAME": "driftSpeed",
      "LABEL": "Drift Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "wanderRange",
      "LABEL": "Wander Range",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2.5,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "breathRate",
      "LABEL": "Breath Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 5,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "dipoleSpin",
      "LABEL": "Dipole Spin",
      "TYPE": "float",
      "MIN": -10,
      "MAX": 10,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "orbSwirl",
      "LABEL": "Orb Swirl",
      "TYPE": "float",
      "MIN": -2,
      "MAX": 2,
      "DEFAULT": 0,
      "GROUP": "Motion"
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
      "NAME": "vignetteAmount",
      "LABEL": "Vignette",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.45,
      "GROUP": "Camera / Layout"
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
      "NAME": "audioReactivity",
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
//  AURA DRIFT — EaselAudio flagship (id 1203)
//  Playbook: motion on Time clocks (never raw audio→position), frequency→
//  space (bass = biggest orb), structure on the bar ramp, texture on levels,
//  color on spectral character (audioBrightness → temperature). Sound-off:
//  the field keeps breathing on TIME alone; every audio gain multiplies to
//  exactly 1.0 in silence.
// ════════════════════════════════════════════════════════════════════════

float hash11(float p) { return fract(sin(p * 127.1) * 43758.5453); }
float hash21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

float vnoise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

float fbm(vec2 p) {
    float v = 0.0;
    float amp = 0.5;
    for (int i = 0; i < 3; i++) {
        v += amp * vnoise(p);
        p *= 2.13;
        amp *= 0.5;
    }
    return v;
}

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}
vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

// Paper grain — spatially STATIC. Measured lesson: per-frame grain flicker
// floods the silence noise floor and drowns the ambient followers; static
// grain keeps all the texture and edge energy at zero motion cost.
// Single-tap grain: the old two-tap average blurred the tooth toward a
// triangular distribution — one uniform tap per pixel keeps it razor-fine.
float filmGrain(vec2 uv, float t) {
    return hash21(uv * RENDERSIZE.xy) - 0.5;
}

// Per-orb two-tone aura palettes (core / fringe) — curated pastels
vec3 orbColA(int i) {
    if (i == 0) return vec3(1.00, 0.60, 0.40); // peach
    if (i == 1) return vec3(0.96, 0.42, 0.56); // rose
    if (i == 2) return vec3(0.72, 0.62, 0.98); // lilac
    if (i == 3) return vec3(0.40, 0.82, 0.86); // aqua
    if (i == 4) return vec3(0.99, 0.88, 0.58); // pale gold
    return vec3(0.98, 0.62, 0.52);             // coral
}
vec3 orbColB(int i) {
    if (i == 0) return vec3(0.92, 0.36, 0.58); // rose shadow
    if (i == 1) return vec3(0.58, 0.40, 0.88); // violet
    if (i == 2) return vec3(0.42, 0.66, 0.98); // sky
    if (i == 3) return vec3(0.52, 0.92, 0.70); // mint
    if (i == 4) return vec3(0.98, 0.96, 0.88); // cream
    return vec3(0.70, 0.62, 0.94);             // lavender
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    vec2 p  = uv - 0.5;
    p.x *= RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    float t = TIME;

    float aR = clamp(audioReactivity, 0.0, 2.0);

    // ── EaselAudio conditioning — LINEAR followers on continuous paths ──
    float bass = clamp(audioBass, 0.0, 1.0);
    float mid  = clamp(audioMid,  0.0, 1.0);
    float high = clamp(audioHigh, 0.0, 1.0);
    float sBass = clamp(max(stemBass, bass), 0.0, 1.0);
    float sMel  = clamp(max(stemMelody, mid), 0.0, 1.0);
    float sAir  = clamp(max(stemAir, high), 0.0, 1.0);
    float present = smoothstep(0.02, 0.12, max(audioLevel, audioEnergy));
    // Hit paths ramped in over the first ~28% of the beat (chop-safe)
    float beatRamp = smoothstep(0.0, 0.28, audioBeatPhase);
    float kick = max(clamp(audioBassHit, 0.0, 1.0), clamp(stemDrumsHit, 0.0, 1.0) * 0.8) * beatRamp * aR;

    // ── Grainy pastel-dusk field (kept deep so the orbs carry the light) ─
    float mottle = fbm(uv * 2.2 + vec2(t * 0.014, -t * 0.009));
    float grad = clamp(uv.y * 0.8 + mottle * 0.35, 0.0, 1.0);
    vec3 field = mix(vec3(0.20, 0.17, 0.26), vec3(0.46, 0.37, 0.38), grad);
    field = mix(field, bgColor.rgb, clamp(bgColor.a, 0.0, 1.0));

    // Color temperature follows spectral brightness (music-gated so the
    // sound-off field is exactly the authored dusk pastel)
    float temp = (clamp(audioBrightness, 0.0, 1.0) - 0.32) * present * aR;
    vec3 tempShift = vec3(0.10, 0.02, -0.09) * temp;
    field += tempShift * 0.6;

    vec3 col = field;

    // ── Six dipole orbs — frequency → space ─────────────────────────────
    for (int i = 0; i < 6; i++) {
        float fi = float(i);
        float h  = hash11(fi * 7.31 + 1.7);

        // Band + Time clock per orb: bass=big, melody=mid, air=small
        float bnd; float bt;
        if (i < 2)      { bnd = sBass; bt = audioBassTime; }
        else if (i < 4) { bnd = sMel;  bt = audioMidTime;  }
        else            { bnd = sAir;  bt = audioHighTime; }

        // Integrated motion: TIME drift + band Time clock (playbook law 2)
        float tt = t * (0.10 + 0.06 * h) * max(driftSpeed, 0.0)
                 + bt * 0.36 * aR + fi * 2.399;

        // 8-beat phase ramp slowly re-choreographs anchors (smooth at wrap)
        float w8 = 0.5 - 0.5 * cos(6.28318 * audioPhase8);
        vec2 aA = (vec2(hash11(fi * 3.13 + 0.7), hash11(fi * 5.71 + 2.3)) * 2.0 - 1.0) * vec2(0.42, 0.32);
        vec2 aB = (vec2(hash11(fi * 7.91 + 4.1), hash11(fi * 9.37 + 6.9)) * 2.0 - 1.0) * vec2(0.42, 0.32);
        vec2 anchor = mix(aA, aB, w8 * 0.65);

        // Wander Range: how far each orb roams from its anchor
        vec2 c = anchor + vec2(sin(tt + h * 6.28), cos(tt * 0.77 + h * 3.0)) * vec2(0.26, 0.20) * wanderRange;
        // Orb Swirl: the whole constellation revolves about the centre
        if (abs(orbSwirl) > 0.0001) {
            float sa = t * orbSwirl * 0.12;
            c = vec2(c.x * cos(sa) - c.y * sin(sa), c.x * sin(sa) + c.y * cos(sa));
        }

        // Radius: idle breath + per-band breathing (silence = idle only)
        float r0 = 0.50 / (1.0 + fi * 0.34);
        float r = r0 * (1.0 + 0.07 * sin(t * breathRate * (0.35 + 0.2 * h) + fi * 1.7)
                            + 0.34 * bnd * aR);

        float d = length(p - c) / max(r, 1e-3);
        float core = exp(-d * d * 2.6);
        float halo = exp(-d * 1.9);

        // Dipole gradient across the orb (two-tone aura)
        vec2 axis = vec2(cos(fi * 2.1 + t * 0.045 * dipoleSpin), sin(fi * 2.1 + t * 0.045 * dipoleSpin));
        float side = clamp(0.5 + 0.5 * dot((p - c) / max(r, 1e-3), axis), 0.0, 1.0);
        vec3 orbCol = mix(orbColA(i), orbColB(i), side) + tempShift;

        col = mix(col, orbCol, clamp(core * 0.85 * clamp(orbGlow, 0.0, 2.0), 0.0, 1.0));
        col += orbCol * halo * 0.085 * clamp(haloBloom, 0.0, 2.0) * (1.0 + 1.1 * kick);
    }

    // ── Whole-frame darken-dip follower (bright pastel scene → dips) ────
    float beatSoft = clamp(audioBeatPulse, 0.0, 1.0) * beatRamp;
    float dip = (0.34 * bass + 0.26 * mid + 0.20 * high
               + 0.30 * beatSoft + 0.20 * clamp(audioPunch, 0.0, 1.0) * beatRamp)
              * 0.40 * aR;
    col *= 1.0 - clamp(dip, 0.0, 0.6);

    // ── Universal color block ───────────────────────────────────────────
    if (hueShift > 0.001) {
        vec3 hsv = rgb2hsv(clamp(col, 0.0, 2.0));
        hsv.x = fract(hsv.x + hueShift);
        col = hsv2rgb(hsv);
    }
    float luma = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(luma), col, clamp(colorBoost, 0.0, 2.0));

    // ── Vignette ────────────────────────────────────────────────────────
    float r2 = dot(p, p);
    col *= 1.0 - clamp(vignetteAmount, 0.0, 1.0) * smoothstep(0.18, 0.85, r2) * 0.55;

    // ── Grain — shadows carry more (photographic), pixel-sharp tooth ────
    float l2 = dot(col, vec3(0.299, 0.587, 0.114));
    float shadowW = 1.0 - smoothstep(0.0, 0.7, l2);
    float g = filmGrain(uv, t);
    col += g * clamp(grainAmount, 0.0, 1.0) * 0.155 * mix(0.72, 1.0, shadowW);

    gl_FragColor = vec4(col, 1.0);
}
