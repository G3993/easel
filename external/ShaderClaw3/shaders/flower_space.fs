/*{
  "DESCRIPTION": "Flower Space — an infinite 3D petal tunnel: nested scalloped discs stacked toward a dark center, glowing ember orange to deep red, with cream light-spokes cutting between the petal columns. The whole flower slowly falls inward forever, petals breathe and the bloom rotates. Bass swells the petals, mids push the fall, beats send a light ring blooming outward, highs glint the petal rims.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "petalCount",  "LABEL": "Petal Count",   "TYPE": "float", "MIN": 6.0, "MAX": 16.0, "DEFAULT": 10.0, "GROUP": "Shape / Geometry" },
    { "NAME": "flowerSize",  "LABEL": "Flower Size",   "TYPE": "float", "MIN": 0.5, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "spokeAmt",    "LABEL": "Light Spokes",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.8,  "GROUP": "Shape / Geometry" },
    { "NAME": "fallSpeed",   "LABEL": "Fall Speed",    "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "spinSpeed",   "LABEL": "Spin",          "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.25, "GROUP": "Motion / Animation" },
    { "NAME": "breatheAmt",  "LABEL": "Petal Breathe", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.1,  "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "fsScene" },
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// FLOWER SPACE — after the nested-petal radial tunnel reference.
//   Rings of scalloped petal discs stacked in depth, each ring a fixed
//   scale ratio smaller, colored on a cream→amber→ember→black-red ramp
//   toward the vanishing center. Rings scroll inward on a log-radius
//   clock so the flower falls into itself seamlessly. Cream spokes sit
//   BETWEEN the petal columns and are shaded as rounded 3D light-tubes
//   that widen toward the viewer. Petals get lobe shading: darker at the
//   scallop edge, hot in the lobe belly, with a crisp specular arc on
//   each rim. 3-pass: scene → persistent trail → bloom composite.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

vec3 hueRotate(vec3 c, float a) {
    float an = a * TAU;
    vec3 k = vec3(0.57735);
    return c * cos(an) + cross(k, c) * sin(an) + k * dot(k, c) * (1.0 - cos(an));
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / (R.y * flowerSize);

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);

    float t  = TIME * motionSpeed;
    float tf = t * fallSpeed * 0.28 + amt * 0.45 * midP;      // fall clock, phase-pushed
    float ts = t * spinSpeed * 0.22;

    float n = floor(petalCount + 0.5);

    float r = length(q);
    float a = atan(q.y, q.x) + ts;

    // ── log-radius ring stack: k = ring depth, scrolls inward ───────────
    float lr    = log(max(r, 1e-4)) / log(0.80);              // ring every ×0.80
    float kf    = lr + tf;                                     // scrolling depth
    float breathe = breatheAmt * (0.5 + amt * 0.9 * bassP);

    vec3 col = vec3(0.0);
    float covered = 0.0;

    // walk candidate rings INNERMOST-FIRST: the smallest disc that still
    // contains the pixel sits on top of the stack, so it wins the pixel
    for (int i = 2; i >= -2; i--) {
        float k  = floor(kf) + float(i);
        float rk = pow(0.80, k - tf);                          // ring base radius
        if (rk < 0.004 || rk > 3.5) continue;

        // per-ring scallop: deep petal lobes around the disc edge
        float ph  = k * 0.30;                                  // ring petal twist
        float lob = cos(a * n + ph);
        float lobe = pow(0.5 + 0.5 * lob, 1.4);                // rounded lobes
        float wob = 0.86 + 0.20 * lobe + 0.025 * breathe * sin(t * 1.3 + k * 1.7);
        float edge = rk * wob;

        float aa   = 1.5 / R.y;
        float mask = smoothstep(edge + aa, edge - aa, r) * (1.0 - covered);
        if (mask <= 0.001) continue;

        // depth from the ring's own radius: cream far out → black core
        // (tuned so the on-screen radius range sweeps the WHOLE ramp)
        float depth = clamp(1.0 - pow(rk / 0.78, 0.8), 0.0, 1.0);

        // ember ramp: cream → amber → orange → ember red → black-red
        vec3 cCream = vec3(1.00, 0.94, 0.86);
        vec3 cAmber = vec3(1.00, 0.68, 0.18);
        vec3 cOrng  = vec3(0.97, 0.34, 0.05);
        vec3 cEmbr  = vec3(0.70, 0.09, 0.03);
        vec3 cCore  = vec3(0.10, 0.012, 0.008);
        vec3 ring = mix(cCream, cAmber, smoothstep(0.00, 0.18, depth));
        ring = mix(ring, cOrng, smoothstep(0.16, 0.40, depth));
        ring = mix(ring, cEmbr, smoothstep(0.38, 0.66, depth));
        ring = mix(ring, cCore, smoothstep(0.72, 0.98, depth));

        // petal shading: each lobe is a soft dome — bright near its rim,
        // falling into shadow where the next inner disc will sit
        float rimD  = clamp((edge - r) / max(edge * 0.28, 1e-4), 0.0, 1.0);
        ring *= 1.12 - 0.42 * rimD;                            // lit lip → shaded base
        ring *= 0.80 + 0.28 * lobe;                            // lobe belly light
        // crisp specular arc hugging each petal rim (tight, HD)
        float spec = exp(-pow((edge - r) * R.y * 0.06, 2.0)) * (0.35 + 0.65 * lobe);
        ring += vec3(1.0, 0.85, 0.6) * spec * (0.16 + amt * 0.4 * highP);

        col = mix(col, ring, mask);
        covered = min(1.0, covered + mask);
    }

    // anything never covered: outside the biggest ring = warm paper,
    // inside everything = the black core
    vec3 uncov = mix(vec3(0.03, 0.006, 0.005), vec3(0.99, 0.95, 0.90), smoothstep(0.6, 1.1, r));
    col = mix(uncov, col, covered);

    // continuous level glow so the flower breathes with any music
    col *= 1.0 + amt * 0.20 * clamp(audioLevel, 0.0, 1.0);

    // ── cream light spokes BETWEEN petal columns, 3D tube shading ───────
    float sa   = fract((a + TAU / n * 0.5) * n / TAU) - 0.5;   // spoke-local angle
    float sw   = 0.13 * (0.40 + 0.60 * smoothstep(0.05, 0.95, r)); // widen outward
    float sd   = abs(sa) / sw;
    float tube = smoothstep(1.0, 0.84, sd) * spokeAmt;
    tube *= smoothstep(0.035, 0.18, r);                        // spokes die at core
    float tShade = sqrt(max(1.0 - sd * sd, 0.0));              // cylinder profile
    vec3 spokeC = vec3(0.99, 0.93, 0.84) * (0.70 + 0.30 * tShade);
    spokeC += vec3(1.0, 0.97, 0.9) * pow(tShade, 18.0) * 0.22; // center gloss line
    // spokes pick up ember glow near the center
    spokeC = mix(vec3(1.0, 0.45, 0.12), spokeC, smoothstep(0.06, 0.55, r));
    col = mix(col, spokeC, tube * 0.92);

    // ── beat bloom ring rippling outward ────────────────────────────────
    float ringR = fract(t * 0.22) * 1.4;
    float ringG = exp(-pow((r - ringR) * 9.0, 2.0)) * amt * beatP;
    col += vec3(1.0, 0.55, 0.2) * ringG * 0.5;

    // fine grain
    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.02;
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        gl_FragColor = vec4(clamp(renderScene(), 0.0, 1.0), 1.0);
    } else if (PASSINDEX == 1) {
        vec3 col = texture2D(fsScene, uv).rgb;
        vec3 prev = texture2D(abTrail, uv).rgb;
        float decay = 0.50 + 0.46 * trailAmt;
        col = max(col, prev * decay - 0.0045);
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else {
        vec2 dir = uv - 0.5;
        float ab = aberration * 0.0045;
        vec3 base;
        base.r = texture2D(abTrail, uv + dir * ab).r;
        base.g = texture2D(abTrail, uv).g;
        base.b = texture2D(abTrail, uv - dir * ab).b;
        vec3 bl = vec3(0.0);
        for (int i = 0; i < 8; i++) {
            float an = float(i) * 0.7853982;
            vec2 o = vec2(cos(an), sin(an)) * (3.5 / R.y);
            bl += texture2D(abTrail, uv + o).rgb;
            bl += texture2D(abTrail, uv + o * 2.6).rgb * 0.6;
        }
        bl /= 12.8;
        bl = max(bl - 0.52, 0.0);
        vec3 col = base + bl * bl * bloomAmt * 1.8;
        col = hueRotate(col, hueShift);
        float lvl = clamp(audioLevel, 0.0, 1.0);
        col *= brightness * mix(1.0, 0.64 + 0.36 * lvl, audioReact)
             * (1.0 + audioReact * 0.06 * clamp(audioBeatPulse, 0.0, 1.0));
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        gl_FragColor = vec4(col, 1.0);
    }
}
