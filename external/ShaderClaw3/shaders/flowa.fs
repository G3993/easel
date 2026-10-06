/*{
  "DESCRIPTION": "Flowa — a spinning garden of iridescent blooms on deep royal blue, no stems: the hero flower slowly rotates while its petal layers counter-rotate and unfurl in a breathing bloom cycle, rims bursting into thin-film rainbows combed with fine fiber striations. Four satellite flowers of different abstract anatomy — rounded, spiked, a carved ring bloom, a small six-lobe — drift around it, each with the same chromatic aura and fringe. Bass breathes the blooms open, mids ripple the petal edges, highs shimmer the iridescence, beats flash the hearts.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "bloomSize",    "LABEL": "Bloom Size",     "TYPE": "float", "MIN": 0.5, "MAX": 1.5, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "petalWobble",  "LABEL": "Petal Wobble",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Shape / Geometry" },
    { "NAME": "iridescence",  "LABEL": "Iridescence",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.85, "GROUP": "Color" },
    { "NAME": "auraAmt",      "LABEL": "Chromatic Aura", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Color" },
    { "NAME": "paletteShift", "LABEL": "Palette Shift",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",     "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",   "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.4, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// FLOWA v2 — a garden of iridescent blooms, no stems: pure flower. The
//   hero bloom SPINS (whole polar rotation, its three layers counter-
//   rotating slightly against each other) and BLOOMS (layers unfurl
//   outward on offset breathing phases — a slow open/close cycle). Four
//   satellite flowers of different abstract anatomy — 7-lobe rounded,
//   4-lobe spiked, a 9-lobe RING bloom with a carved luminous center,
//   and a small 6-lobe — drift very slowly around it. Every flower keeps
//   the loved iridescent fiber-fringe: cos-palette rims combed by
//   integer-frequency filament striations, plus a chromatic aura leaking
//   past each silhouette. Sound-off is a slow spinning breathing garden.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define PI 3.14159265
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1.0, 0.0)), f.x),
               mix(hash21(i + vec2(0.0, 1.0)), hash21(i + vec2(1.0, 1.0)), f.x), f.y);
}
float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 3; i++) {
        v += a * vnoise(p);
        p = p * 2.13 + vec2(7.3, 3.1);
        a *= 0.5;
    }
    return v;
}
vec3 irid(float h) { return 0.5 + 0.5 * cos(TAU * h + vec3(0.0, 2.094, 4.188)); }
vec3 hueRotate(vec3 c, float a) {
    float cs = cos(a), sn = sin(a);
    mat3 m = mat3(
        0.299 + 0.701*cs + 0.168*sn, 0.587 - 0.587*cs + 0.330*sn, 0.114 - 0.114*cs - 0.497*sn,
        0.299 - 0.299*cs - 0.328*sn, 0.587 + 0.413*cs + 0.035*sn, 0.114 - 0.114*cs + 0.292*sn,
        0.299 - 0.300*cs + 1.250*sn, 0.587 - 0.588*cs - 1.050*sn, 0.114 + 0.886*cs - 0.203*sn);
    return clamp(c * m, 0.0, 2.0);
}

// one iridescent flower: 3 polar layers, spin + unfurl, fiber fringe, aura.
// ringy=1 carves the center into a luminous ring bloom.
void drawFlower(inout vec3 col, vec2 q, vec2 c, float s, float n0, float pcurv,
                float ringy, float spin, float seed, float t,
                float iridGain, float wobAmt, float auraA,
                float amt, float midP, float beatP) {
    vec2 d = q - c;
    float r = length(d);
    if (r > s * 2.1) return;                                    // outside flower + aura
    float th = atan(d.y, d.x);
    vec2 circ = vec2(cos(th), sin(th));
    for (int i = 0; i < 3; i++) {
        float fi = float(i);
        float scale = s * (1.0 - 0.27 * fi);
        float n = n0 + fi;
        // SPIN: whole polar layer rotation; layers counter-rotate slightly
        float dir = (mod(fi, 2.0) < 1.0) ? 1.0 : -0.72;
        float rot = spin * t * dir * (1.0 - 0.30 * fi) + fi * 1.1 + seed * TAU
                  + 0.06 * sin(t * (0.17 + 0.05 * fi) + fi * 2.0 + seed * 5.0);
        // BLOOM: layers unfurl outward on offset breathing phases
        float unf = 0.80 + 0.20 * sin(t * 0.26 + seed * TAU + fi * 0.85);
        float lobe = pow(abs(cos(0.5 * n * (th + rot))), pcurv);
        float wob = 1.0 + wobAmt * (0.05 * fbm(circ * 2.5 + fi * 7.0 + seed * 3.0 + t * 0.06) - 0.025)
                  + amt * 0.06 * midP * sin(n * th * 2.0 + t * 3.0);
        float rad = scale * (0.70 + 0.30 * lobe) * wob * unf;
        float g = r / max(rad, 1e-4);

        // iridescent rim band with combed fiber striations
        float fringe = smoothstep(0.62, 0.97, g) * smoothstep(1.03, 0.99, g);
        float hue = g * 2.2 - lobe * 0.35 + fi * 0.21 + seed * 0.47 + t * 0.05;
        float fib = 0.62 + 0.38 * sin(th * (150.0 + 24.0 * fi)
                                      + fbm(circ * 6.0 + fi * 3.0 + seed * 9.0) * 9.0);
        vec3 rimCol = irid(hue) * fib * (0.9 + 0.5 * iridGain);

        // cream petal body with soft radial shading
        vec3 body = mix(vec3(0.86, 0.83, 0.80), vec3(0.99, 0.97, 0.94),
                        smoothstep(0.0, 0.85, g) * (0.55 + 0.15 * fi))
                  * (0.82 + 0.06 * fi);
        vec3 layerCol = mix(body, rimCol, fringe * iridGain);

        // ring blooms: carve the center, fringe the inner rim too
        float alpha = smoothstep(1.006, 0.994, g)
                    * mix(1.0, smoothstep(0.40, 0.60, g), ringy);
        float fringeIn = ringy * smoothstep(0.66, 0.52, g) * smoothstep(0.38, 0.54, g);
        layerCol = mix(layerCol, irid(hue + 0.42) * fib * (0.9 + 0.5 * iridGain), fringeIn);

        col = mix(col, layerCol, alpha);

        // chromatic aura leaking past the silhouette
        float aura = exp(-max(g - 1.0, 0.0) * (9.0 - 2.0 * fi));
        col += irid(hue + 0.15) * aura * (1.0 - alpha) * auraA
             * (0.14 + 0.05 * fi) * (1.0 + amt * (1.3 * beatP + 0.5 * midP));
    }
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;       // centered, y in [-0.5, 0.5]

    float t   = TIME * motionSpeed * 0.5;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ── royal-blue night garden with vignette ───────────────────────────
    vec3 col = mix(vec3(0.03, 0.09, 0.22), vec3(0.10, 0.26, 0.58),
                   smoothstep(-0.55, 0.55, q.y) * 0.8 + 0.1);
    col *= 1.0 - 0.50 * smoothstep(0.38, 0.98, length(q * vec2(0.85, 1.0)));
    // drifting pollen sparkle in the night air
    vec2 pcell = floor(q * 46.0 + vec2(t * 0.35, -t * 0.22));
    float ph = hash21(pcell + 2.3);
    col += vec3(0.55, 0.65, 0.95) * pow(0.5 + 0.5 * sin(t * 1.7 + ph * TAU), 16.0)
         * step(0.90, ph) * 0.30;

    float bloom = bloomSize * (1.0 + 0.025 * sin(t * 0.5) + amt * 0.15 * bassP);
    float iridGain = iridescence * (1.0 + amt * (1.0 * highP + 0.7 * beatP));

    // ── satellite blooms (behind), each its own abstract anatomy ────────
    // 7-lobe rounded
    vec2 cA = vec2(-0.615, 0.265) + 0.030 * vec2(sin(t * 0.090 + 1.0), cos(t * 0.070 + 2.0));
    drawFlower(col, q, cA, 0.170 * bloom, 7.0, 0.42, 0.0, -0.085, 0.23, t,
               iridGain, petalWobble, auraAmt, amt, midP, beatP);
    // 4-lobe spiked
    vec2 cB = vec2(0.640, -0.270) + 0.026 * vec2(sin(t * 0.075 + 4.0), cos(t * 0.095 + 0.5));
    drawFlower(col, q, cB, 0.135 * bloom, 4.0, 1.15, 0.0, 0.11, 0.61, t,
               iridGain, petalWobble, auraAmt, amt, midP, beatP);
    // 9-lobe RING bloom, carved luminous center
    vec2 cC = vec2(0.575, 0.300) + 0.024 * vec2(sin(t * 0.065 + 2.6), cos(t * 0.085 + 5.1));
    drawFlower(col, q, cC, 0.200 * bloom, 9.0, 0.62, 1.0, -0.070, 0.82, t,
               iridGain, petalWobble, auraAmt, amt, midP, beatP);
    // small 6-lobe
    vec2 cD = vec2(-0.585, -0.305) + 0.028 * vec2(sin(t * 0.080 + 5.6), cos(t * 0.060 + 3.3));
    drawFlower(col, q, cD, 0.105 * bloom, 6.0, 0.80, 0.0, 0.13, 0.44, t,
               iridGain, petalWobble, auraAmt, amt, midP, beatP);

    // ── the hero bloom: centered, spinning, unfurling ───────────────────
    vec2 c = vec2(0.0, 0.0);
    drawFlower(col, q, c, 0.46 * bloom, 5.0, 0.65, 0.0, 0.055, 0.0, t,
               iridGain, petalWobble, auraAmt, amt, midP, beatP);

    // ── stamen heart (spins with the bloom) ─────────────────────────────
    float heartGain = 1.0 + amt * (0.8 * beatP + 0.3 * bassP) + 0.08 * sin(t * 1.1);
    for (int i = 0; i < 6; i++) {
        float fi = float(i);
        float a = fi * TAU / 6.0 + t * 0.155;
        vec2 sc = c + 0.045 * bloom * vec2(cos(a), sin(a) * 0.8);
        float rr = length(q - sc);
        col += irid(fi * 0.166 + t * 0.04) * heartGain * 0.35
             * exp(-(rr * rr) / (0.00022 * bloom));
    }
    float rc = length(q - c);
    col = mix(col, vec3(1.0, 0.98, 0.92),
              clamp(heartGain * exp(-(rc * rc) / (0.0012 * bloom)), 0.0, 0.9));
    // tiny satellite hearts
    float rcC = length(q - cC);
    col += irid(t * 0.05 + 0.5) * 0.5 * heartGain * exp(-(rcC * rcC) / (0.00030 * bloom));

    // ── finish ──────────────────────────────────────────────────────────
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.030 * gr * (1.0 + amt * 0.7 * highP);

    if (paletteShift > 0.001) col = hueRotate(col, paletteShift * TAU);
    col *= brightness * mix(1.0, 0.60 + 0.26 * levelP + 0.09 * beatP + 0.05 * clamp(audioBass, 0.0, 1.0), amt);
    return col;
}

// ─── multipass: trail accumulation → bloom + lens-depth composite ────────
void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        vec3 col = renderScene();
        // luminous motion trails persist in the buffer (8-bit-safe decay floor)
        vec3 prev = texture2D(abTrail, uv).rgb;
        float decay = 0.50 + 0.46 * trailAmt;
        col = max(col, prev * decay - 0.0045);
        // write-dither: keeps soft gradients band-free in 8-bit buffer hosts
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else {
        // chromatic lens depth: R/B pulled apart along the radial axis
        vec2 dir = uv - 0.5;
        float ab = aberration * 0.0045;
        vec3 base;
        base.r = texture2D(abTrail, uv + dir * ab).r;
        base.g = texture2D(abTrail, uv).g;
        base.b = texture2D(abTrail, uv - dir * ab).b;
        // wide two-ring bloom of the bright field — real glow depth
        vec3 bl = vec3(0.0);
        for (int i = 0; i < 8; i++) {
            float a = float(i) * 0.7853982;
            vec2 o = vec2(cos(a), sin(a)) * (3.5 / R.y);
            bl += texture2D(abTrail, uv + o).rgb;
            bl += texture2D(abTrail, uv + o * 2.6).rgb * 0.6;
        }
        bl /= 12.8;
        bl = max(bl - 0.52, 0.0);
        vec3 col = base + bl * bl * bloomAmt * 1.8;
        // HD finish: S-curve contrast + saturation richness
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        gl_FragColor = vec4(col, 1.0);
    }
}
