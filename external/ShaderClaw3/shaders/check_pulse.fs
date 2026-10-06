/*{
  "DESCRIPTION": "Checker Pulse — the hot-pink and red diamond gate pattern woven into living cloth: alternating diamonds carry striped gates and inward-pointing arrows, and the whole textile ripples in 3D waves with real lighting, stripes sliding through the gates as it moves. Both colors are yours to pick. Bass rolls the cloth wave, mids slide the stripes, beats flash a gate cascade, highs shimmer the weave.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "gridScale",   "LABEL": "Pattern Scale", "TYPE": "float", "MIN": 3.0, "MAX": 16.0, "DEFAULT": 9.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "waveAmt",     "LABEL": "Cloth Wave",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.55, "GROUP": "Shape / Geometry" },
    { "NAME": "rotation",    "LABEL": "Rotation",      "TYPE": "float", "MIN": -1.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "scrollSpeed", "LABEL": "Scroll Speed",  "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.4,  "GROUP": "Motion / Animation" },
    { "NAME": "stripeSpeed", "LABEL": "Stripe Slide",  "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.6,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "colorA",      "LABEL": "Field Color",   "TYPE": "color", "DEFAULT": [1.0, 0.18, 0.72, 1.0], "GROUP": "Color" },
    { "NAME": "colorB",      "LABEL": "Gate Color",    "TYPE": "color", "DEFAULT": [0.92, 0.04, 0.13, 1.0], "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.08, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// CHECKER PULSE — after the pink/red diamond-gate textile reference.
//   The motif: a 45° checker where every other diamond holds a "gate" —
//   vertical bars flanked by two inward triangles. Rebuilt as cloth:
//   the pattern lives on a waving 3D surface (two crossed sine sheets),
//   lit by a moving key light with weave-level micro shading. Stripes
//   slide through their gates continuously; a beat sends a brightness
//   cascade diagonally through the diamonds. Pixel-true AA on every
//   bar and triangle edge.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

// the gate motif inside one diamond cell. lp in [-0.5,0.5]^2 (diamond space)
// returns 1 where the motif ink (colorB) is.
float gateMotif(vec2 lp, float aa, float slide) {
    // bars region: |x| < 0.16 → 3 sliding vertical bars
    float ink = 0.0;
    float barsW = 0.19;
    float inBars = smoothstep(barsW + aa, barsW - aa, abs(lp.x));
    float s = fract(lp.x * 8.0 + slide);
    float bar = smoothstep(0.34 + aa * 8.0, 0.34 - aa * 8.0, abs(s - 0.5));
    // bars clipped to diamond height at this x
    float hh = 0.46 - abs(lp.x) * 0.55;
    float clipY = smoothstep(hh + aa, hh - aa, abs(lp.y));
    ink = max(ink, inBars * bar * clipY);
    // two inward triangles: tips pointing at the bars
    for (int side = 0; side < 2; side++) {
        float sx = side == 0 ? -1.0 : 1.0;
        vec2 p = vec2(lp.x * sx, lp.y);          // mirror
        // triangle: base at x=0.47, tip at x=0.22
        float u = (p.x - 0.22) / 0.25;           // 0 at tip → 1 at base
        float halfH = 0.21 * clamp(u, 0.0, 1.0);
        float tri = smoothstep(aa, -aa, abs(p.y) - halfH)
                  * smoothstep(-aa, aa, u) * smoothstep(1.0 + aa, 1.0 - aa, u);
        ink = max(ink, tri);
    }
    return ink;
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);

    float t = TIME * motionSpeed;

    // rotation control + slow drift
    float ra = rotation * 1.57 + t * 0.015;
    float cr = cos(ra), sr = sin(ra);
    q = mat2(cr, -sr, sr, cr) * q;

    // ── 3D cloth wave: height field + analytic normal ───────────────────
    float wAmp = waveAmt * (0.6 + amt * 0.8 * bassP);
    float w1 = sin(q.x * 4.2 + t * 0.7) * sin(q.y * 3.1 - t * 0.5);
    float w2 = sin((q.x + q.y) * 5.0 - t * 0.9);
    float height = wAmp * (0.06 * w1 + 0.035 * w2);
    // parallax displace the pattern lookup by the slope (cloth billows)
    vec2 grad = vec2(
        wAmp * (0.06 * 4.2 * cos(q.x * 4.2 + t * 0.7) * sin(q.y * 3.1 - t * 0.5) + 0.035 * 5.0 * cos((q.x + q.y) * 5.0 - t * 0.9)),
        wAmp * (0.06 * 3.1 * sin(q.x * 4.2 + t * 0.7) * cos(q.y * 3.1 - t * 0.5) + 0.035 * 5.0 * cos((q.x + q.y) * 5.0 - t * 0.9)));
    vec2 pq = q + grad * 0.30;

    // ── diamond lattice (45° checker) ───────────────────────────────────
    float sc = gridScale;
    vec2 g = mat2(0.5, 0.5, -0.5, 0.5) * pq * sc;         // rotate 45° into cell space
    g.y += t * scrollSpeed * 0.35;                         // scroll the cloth
    vec2 id = floor(g);
    vec2 lp45 = fract(g) - 0.5;
    // back to diamond-local axis-aligned coords
    vec2 lp = mat2(1.0, -1.0, 1.0, 1.0) * lp45 * 0.5 * 1.414;
    float checker = mod(id.x + id.y, 2.0);

    float aa = 1.4 * sc / R.y;

    vec3 cA = colorA.rgb;
    vec3 cB = colorB.rgb;

    float slide = t * stripeSpeed * 0.8 + amt * 0.6 * midP;
    float ink = gateMotif(lp, aa, slide);

    // every diamond carries the gate motif — a full tessellation; the
    // checker phase shades alternate plates a hair deeper for the weave
    vec3 plateC = cA * mix(1.0, 0.94, checker);
    vec3 col = mix(plateC, cB, ink);

    // beat cascade: a diagonal brightness wave marching through diamonds
    float casc = fract((id.x - id.y) * 0.07 - t * 0.25);
    float flash = exp(-casc * 7.0) * amt * beatP * 0.5;
    col += cB * flash * ink + cA * flash * (1.0 - ink) * 0.35;

    // ── lighting from the cloth normal ──────────────────────────────────
    vec3 nrm = normalize(vec3(-grad, 1.0));
    vec3 L = normalize(vec3(0.5, 0.65, 0.85));
    float dif = clamp(dot(nrm, L), 0.0, 1.0);
    float spe = pow(clamp(dot(reflect(-L, nrm), vec3(0.0, 0.0, 1.0)), 0.0, 1.0), 24.0);
    col *= 0.74 + 0.34 * dif * (1.0 + amt * 0.25 * clamp(audioLevel, 0.0, 1.0)) + height * 1.6;
    col += vec3(1.0, 0.9, 0.95) * spe * 0.12;

    // weave micro-texture shimmer (highs)
    float weave = sin(pq.x * sc * 40.0) * sin(pq.y * sc * 40.0);
    col *= 1.0 + weave * (0.02 + amt * 0.03 * highP);

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.015;
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        vec3 col = renderScene();
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
