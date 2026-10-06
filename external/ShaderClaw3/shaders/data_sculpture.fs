/*{
  "DESCRIPTION": "Data Sculpture — abstract data-art (Anadol / Koblin / Paley lineage). A synthetic 32-band FFT, hash-driven and audio-modulated, drives five moods: Bar Forest, Particle Stream, Skyline Grid, Fingerprint Field, Ribbon Cloud. Single-pass LINEAR HDR.",
  "CATEGORIES": [
    "Generator",
    "Data",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "intensity",
      "LABEL": "Intensity",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 1.8,
      "DEFAULT": 1
    },
    {
      "NAME": "mood",
      "LABEL": "Mood",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3,
        4
      ],
      "LABELS": [
        "Bar Forest",
        "Particle Stream",
        "Skyline Grid",
        "Fingerprint Field",
        "Ribbon Cloud"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "density",
      "LABEL": "Data Density",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 1.6,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "flow",
      "LABEL": "Flow Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "bandSweep",
      "LABEL": "Band Sweep",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "oscSpread",
      "LABEL": "Osc Spread",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "shapeDrift",
      "LABEL": "Shape Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "breathRate",
      "LABEL": "Breath Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 1,
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
//  Five abstract data-art moods, one synthetic 32-band FFT.
//  Bass = energy; mid = spectral tilt; treble = sparkle. Lives in silence.
// ════════════════════════════════════════════════════════════════════════
#define PI    3.14159265
#define TAU   6.28318531
#define BANDS 32

float h11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float h21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
vec2  h22(vec2 p)  { return fract(sin(vec2(dot(p, vec2(127.1,311.7)),
                                           dot(p, vec2(269.5,183.3)))) * 43758.5453); }

// Synthetic FFT: 32 hash-seeded oscillators shaped by audio components.
float fftBand(int i, float t, float bass, float mid, float treble) {
    float fi = float(i);
    float s1 = h11(fi * 1.731), s2 = h11(fi * 4.219), s3 = h11(fi * 7.913);
    float v  = sin(t * (0.6 + s1 * 1.4 * oscSpread) + s1 * TAU) * 0.55
             + sin(t * (0.3 + s2 * 0.9 * oscSpread) + s2 * TAU + fi * 0.21) * 0.30
             + sin(t * (1.4 + s3 * 2.1 * oscSpread) + s3 * TAU) * 0.15;
    v = v * 0.5 + 0.5;
    float u    = fi / float(BANDS - 1);
    float tilt = mix(1.0 - u * 0.6, 0.4 + u * 0.8, clamp(mid, 0.0, 1.0));
    float spark = step(0.985 - treble * 0.05, h11(fi * 11.0 + floor(t * 6.0)))
                * treble * 0.6 * step(0.55, u);
    return clamp(v * tilt * (0.35 + bass * 0.85) + spark, 0.0, 1.4);
}
float fftSample(float fIdx, float t, float bass, float mid, float treble) {
    // Band Sweep: every element's band assignment rotates through the spectrum over time
    fIdx     = mod(fIdx + bandSweep * TIME * 2.0, float(BANDS));
    fIdx     = clamp(fIdx, 0.0, float(BANDS - 1) - 0.001);
    float i0 = floor(fIdx), f = fIdx - i0;
    return mix(fftBand(int(i0),     t, bass, mid, treble),
               fftBand(int(i0)+1,   t, bass, mid, treble), f);
}
void synthAudio(float t, float ar, out float bass, out float mid, out float treble) {
    float k = clamp(ar, 0.0, 2.0);
    // Synthetic base keeps the piece alive in silence; real audioBass/Mid/High
    // (live signal) are wired in on top, scaled by the Audio React knob, so
    // the sculpture actually breathes with the track instead of only a
    // fixed internal clock.
    bass   = (0.20 + 0.18 * (sin(t * 0.71 * breathRate) * 0.5 + 0.5))           * (0.5 + 0.9 * k) + audioBass * k * 0.6;
    mid    = (0.25 + 0.20 * (sin(t * 1.13 * breathRate + 1.7) * 0.5 + 0.5))     * (0.5 + 0.9 * k) + audioMid  * k * 0.6;
    treble = (0.15 + 0.15 * (sin(t * 2.31 * breathRate + 3.1) * 0.5 + 0.5))     * (0.4 + 1.1 * k) + audioHigh * k * 0.6;
}

// ─── MOOD 0 — BAR FOREST ──────────────────────────────────────────────
//  Skewed-iso forest of vertical bars. Graphite + neon-cyan accents.
vec3 moodBarForest(vec2 uv, float t, float bass, float mid, float treble) {
    vec2  p = uv * 2.0 - 1.0;
    vec2  g = vec2(p.x - p.y * 0.32, p.y * 1.45) * (4.0 * density);
    vec2  cell = floor(g), fr = fract(g);
    vec3  col = vec3(0.018, 0.020, 0.024)
              + vec3(0.025, 0.030, 0.036) * smoothstep(-1.0, 0.6, p.y);
    for (int dy = 4; dy >= -1; dy--)
    for (int dx = -1; dx <= 1; dx++) {
        vec2  id = cell + vec2(float(dx), float(dy));
        float bandF = mod(h21(id) * 31.999, float(BANDS));
        float h  = fftSample(bandF, t * (0.6 + flow * 0.6), bass, mid, treble);
        float bH = 0.05 + h * 1.05 * intensity;
        vec2  lp = fr - vec2(float(dx), float(dy)) - 0.5;
        float top = -0.5 + bH;
        float bar = step(abs(lp.x), 0.32) * step(lp.y, top) * step(-0.5, lp.y);
        if (bar > 0.5) {
            float vert = (lp.y + 0.5) / max(bH, 0.001);
            vec3  face = mix(vec3(0.045,0.052,0.065), vec3(0.085,0.095,0.115), vert)
                       * (1.0 - 0.18 * float(dx));
            float edge = smoothstep(0.04, 0.0, abs(lp.y - top));
            vec3  cyan = vec3(0.10, 0.95, 1.10) * (0.6 + 0.6 * h);
            col = face + cyan * edge * 0.85 + cyan * 0.06 * h * h;
        }
    }
    float gx = abs(fract(g.x) - 0.5), gy = abs(fract(g.y) - 0.5);
    col += vec3(0.04, 0.18, 0.22) * smoothstep(0.49, 0.495, max(gx, gy)) * 0.18;
    col += vec3(0.4, 1.0, 1.1)
         * step(0.997 - treble * 0.06, h21(floor(g * 8.0) + floor(t * 4.0))) * 0.6;
    return col;
}

// ─── MOOD 1 — PARTICLE STREAM ─────────────────────────────────────────
//  Particles trace the waveform; magenta trails fade behind each.
vec3 moodParticleStream(vec2 uv, float t, float bass, float mid, float treble) {
    vec3 col = vec3(0.002, 0.001, 0.004);
    int  N = int(140.0 * density);
    for (int i = 0; i < 200; i++) {
        if (i >= N) break;
        float fi = float(i), seed = h11(fi * 0.731);
        float x  = fract(seed + t * (0.06 + flow * 0.08) + fi * 0.005);
        float bandF = fract(seed * 7.3 + fi * 0.37) * float(BANDS - 1);
        float w = fftSample(bandF, t, bass, mid, treble);
        float y = 0.5 + (w - 0.5) * 0.85 * intensity;
        vec2  d = uv - vec2(x, y);
        float trail = exp(-pow(d.y * 90.0, 2.0))
                    * smoothstep(-(0.06 + 0.10 * w), 0.0, d.x) * step(d.x, 0.0);
        float head  = exp(-dot(d * 220.0, d * 220.0));
        col += vec3(1.0) * head * 1.6 + vec3(1.10, 0.18, 0.85) * trail * 0.55;
    }
    for (int i = 0; i < 16; i++) {
        float fi = float(i);
        vec2 sp = h22(vec2(floor(t * (3.0 + treble * 5.0)) + fi, fi * 1.7));
        col += vec3(1.0, 0.85, 1.0) * exp(-length(uv - sp) * 280.0) * treble * 1.2;
    }
    col += vec3(0.18, 0.02, 0.14) * bass * 0.18;
    return col;
}

// ─── MOOD 2 — SKYLINE GRID ────────────────────────────────────────────
//  Iso city: 32 bars across; cobalt-base → magenta-top per bar.
vec3 moodSkylineGrid(vec2 uv, float t, float bass, float mid, float treble) {
    vec3  col = vec3(0.012, 0.018, 0.045) * smoothstep(-1.0, 1.0, uv.y * 2.0 - 1.0)
              + vec3(0.008, 0.010, 0.022);
    vec2  p   = (uv * 2.0 - 1.0) * vec2(1.4, 1.0);
    float bw  = 1.8 / float(BANDS), baseY = -0.78, depth = 0.18;
    for (int i = BANDS - 1; i >= 0; i--) {
        float fi = float(i);
        float bx = -0.9 + (fi + 0.5) * bw;
        float h  = fftBand(i, t * (0.7 + flow * 0.5), bass, mid, treble);
        float topY = baseY + 0.05 + h * 1.5 * intensity;
        // Front face
        float front = step(abs(p.x - bx), bw * 0.44)
                    * step(p.y, topY) * step(baseY, p.y);
        // Top slab (parallelogram)
        float topThk = depth * 0.35;
        float u = clamp((p.y - topY) / max(topThk, 0.001), 0.0, 1.0);
        float top = step(abs(p.x - mix(bx, bx + depth * 0.6, u)), bw * 0.44)
                  * step(p.y, topY + topThk) * step(topY, p.y);
        // Right side (small parallelogram)
        float sLx = bx + bw * 0.44, sRx = sLx + depth * 0.5;
        float sUp = clamp((p.x - sLx) / max(depth * 0.5, 0.001), 0.0, 1.0);
        float side = step(sLx, p.x) * step(p.x, sRx)
                   * step(p.y, mix(topY, topY + topThk, sUp))
                   * step(mix(baseY, baseY + topThk, sUp), p.y);
        if (front + top + side > 0.001) {
            float vG = clamp((p.y - baseY) / max(topY - baseY, 0.001), 0.0, 1.0);
            vec3  g  = mix(vec3(0.10, 0.30, 1.05), vec3(1.05, 0.18, 0.85), pow(vG, 1.1));
            col = mix(col, g * front + g * 1.45 * top + g * 0.55 * side,
                      clamp(front + top + side, 0.0, 1.0));
            col += vec3(1.05, 0.18, 0.85)
                 * smoothstep(0.012, 0.0, abs(p.y - topY)) * step(abs(p.x - bx), bw * 0.44)
                 * (0.4 + 0.6 * h) * intensity;
        }
    }
    col += vec3(1.0, 0.5, 0.95)
         * step(0.992 - treble * 0.06, h21(floor(uv * 220.0) + floor(t * 8.0))) * 0.6;
    col += vec3(0.30, 0.10, 0.50) * bass * 0.10;
    return col;
}

// ─── MOOD 3 — FINGERPRINT FIELD ───────────────────────────────────────
//  Concentric whorls warped by data — Anadol palette.
vec3 moodFingerprint(vec2 uv, float t, float bass, float mid, float treble) {
    vec2 p = (uv * 2.0 - 1.0) * vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0);
    float warp = 0.0;
    for (int k = 0; k < 3; k++) {
        float fk = float(k);
        vec2  c  = vec2(0.55 * sin(t * shapeDrift * (0.13 + fk * 0.07) + fk * 2.1),
                        0.45 * cos(t * shapeDrift * (0.11 + fk * 0.09) + fk * 1.3));
        vec2  d  = p - c;
        float r  = length(d), a = atan(d.y, d.x);
        float bandF = fract((a + PI) / TAU + fk * 0.3) * float(BANDS - 1);
        float w = fftSample(bandF, t * (0.5 + flow * 0.6), bass, mid, treble);
        warp   += (r * (1.0 + 0.45 * sin(a * (3.0 + fk) + t * 0.4))
                  - w * (0.18 + bass * 0.25)) * (1.0 - fk * 0.25);
    }
    float ridge = sin(warp * 32.0 * density);
    float lines = smoothstep(0.85, 0.95, abs(ridge));
    float hue = clamp(0.5 + 0.5 * sin(warp * 1.2 + t * 0.2), 0.0, 1.0);
    vec3  fc  = mix(vec3(0.05, 0.08, 0.55), vec3(0.55, 0.18, 1.10),
                    smoothstep(0.0, 0.55, hue));
    fc        = mix(fc, vec3(1.10, 0.18, 0.65), smoothstep(0.5, 1.0, hue));
    vec3 col  = mix(vec3(0.02, 0.02, 0.10), fc * 0.65,
                    smoothstep(-0.8, 0.8, sin(warp * 2.0)) * (0.4 + bass * 0.4));
    col += fc * lines * (0.6 + mid * 0.9) * intensity;
    col += vec3(1.10, 0.18, 0.65)
         * exp(-(length(p) - 0.05 + 0.03 * sin(t)) * 4.0) * 0.10 * (0.5 + bass);
    col += vec3(1.05, 0.55, 1.0)
         * step(0.991 - treble * 0.06, h21(floor(uv * 240.0) + floor(t * 9.0))) * 0.55;
    return col;
}

// ─── MOOD 4 — RIBBON CLOUD ────────────────────────────────────────────
//  6 ribbons threading the canvas, latent-space feel; Anadol palette.
vec3 moodRibbonCloud(vec2 uv, float t, float bass, float mid, float treble) {
    vec3 col = mix(vec3(0.020, 0.020, 0.085), vec3(0.085, 0.025, 0.150),
                   smoothstep(0.0, 1.0, uv.y));
    for (int i = 0; i < 6; i++) {
        float fi = float(i), seed = h11(fi * 1.51);
        float bandF = (fi + 0.5) / 6.0 * float(BANDS - 1);
        float w  = fftSample(bandF, t * (0.5 + flow * 0.6), bass, mid, treble);
        float yC = 0.18 + 0.13 * fi + seed * 0.05
                 + 0.13 * sin(uv.x * 6.0  + t * shapeDrift * (0.4 + seed * 0.3) + fi)
                 + 0.07 * sin(uv.x * 14.0 + t * 0.7 * shapeDrift + seed * 5.0)
                 + (w - 0.5) * 0.18 * intensity;
        float thick = max(0.018 + 0.025 * 0.5 * sin(uv.x * 5.0 + t * 0.3 + fi * 1.7)
                        + 0.025 + 0.040 * w * intensity, 0.012);
        float d = abs(uv.y - yC);
        float core = smoothstep(thick, thick * 0.4, d);
        float halo = smoothstep(thick * 4.5, thick, d) * 0.45;
        float a = fi / 5.0;
        vec3  rc = mix(vec3(0.10, 0.20, 1.10), vec3(0.65, 0.18, 1.10),
                       smoothstep(0.0, 0.6, a));
        rc       = mix(rc, vec3(1.15, 0.20, 0.70), smoothstep(0.5, 1.0, a))
                 * (0.7 + 0.6 * w);
        col += rc * core * 1.10 + rc * halo * 0.55;
    }
    for (int i = 0; i < 24; i++) {
        float fi = float(i), seed = h11(fi * 9.71);
        vec2 c = vec2(fract(seed + t * (0.04 + flow * 0.05) + fi * 0.013),
                      0.5 + 0.45 * sin(t * shapeDrift * (0.3 + seed) + fi));
        col += vec3(1.0, 0.85, 1.0) * exp(-length(uv - c) * 220.0) * (0.4 + treble * 1.2);
    }
    col += vec3(0.15, 0.08, 0.35) * bass * 0.18;
    return col;
}

void main() {
    vec2  uv = isf_FragNormCoord.xy;
    float t  = TIME;
    float bass, mid, treble;
    synthAudio(t, audioReact, bass, mid, treble);

    int  m = int(mood + 0.5);
    vec3 col;
    if      (m == 0) col = moodBarForest      (uv, t, bass, mid, treble);
    else if (m == 1) col = moodParticleStream (uv, t, bass, mid, treble);
    else if (m == 2) col = moodSkylineGrid    (uv, t, bass, mid, treble);
    else if (m == 3) col = moodFingerprint    (uv, t, bass, mid, treble);
    else             col = moodRibbonCloud    (uv, t, bass, mid, treble);

    col += (h21(uv * RENDERSIZE.xy + floor(t * 60.0)) - 0.5) * 0.010;

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = max(col, 0.0);
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
    // background = darkest end of the field (the void behind the data)
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

    gl_FragColor = vec4(uc, 1.0);
}
