/*{
  "DESCRIPTION": "Psy Burst — the psychedelic riso explosion: candy-colored wedge rays firing from an off-center point, some rays carrying black-and-white zebra stripes, everything dusted in halftone speckle, with a melting rainbow wriggle at the core. The whole burst slowly rotates while stripes race down their rays. Bass fattens the rays, mids swirl the melt, beats fire a stripe cascade, highs boil the speckle.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "rayCount",    "LABEL": "Ray Count",     "TYPE": "float", "MIN": 8.0, "MAX": 28.0, "DEFAULT": 18.0, "GROUP": "Shape / Geometry" },
    { "NAME": "zebraAmt",    "LABEL": "Zebra Rays",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.4,  "GROUP": "Shape / Geometry" },
    { "NAME": "meltSize",    "LABEL": "Core Melt",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.55, "GROUP": "Shape / Geometry" },
    { "NAME": "spinSpeed",   "LABEL": "Burst Spin",    "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.25, "GROUP": "Motion / Animation" },
    { "NAME": "raceSpeed",   "LABEL": "Stripe Race",   "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "hueShift",    "LABEL": "Palette Spin",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "speckleAmt",  "LABEL": "Halftone Dust", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.06, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// PSY BURST — after the psychedelic dotted-burst reference.
//   Angular wedge rays around a vanishing point set high in the frame.
//   Each wedge hashes into either a flat candy ink (coral, sky blue,
//   olive gold, pink, green) or a black/cream zebra ray whose stripes
//   race along the radius with wobbling edges. Wedge borders get a
//   halftone stipple dust (the riso speckle), denser near ray edges.
//   Inside meltSize radius the domain goes molten: strong fbm swirl
//   warps the wedges into the reference's central wriggle, mid-driven.
//   Slow global spin; wedge hues walk very slowly so the burst never
//   freezes.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float hash11(float n) { return fract(sin(n) * 43758.5453123); }
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1.0, 0.0)), f.x),
               mix(hash21(i + vec2(0.0, 1.0)), hash21(i + vec2(1.0, 1.0)), f.x), f.y);
}
float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.13 + vec2(7.3, 3.1);
        a *= 0.5;
    }
    return v;
}

vec3 candy(float k) {
    // the reference inks: coral, sky, olive-gold, pink, green, cream
    float s = fract(k);
    vec3 coral = vec3(0.98, 0.45, 0.33);
    vec3 sky   = vec3(0.25, 0.70, 0.92);
    vec3 olive = vec3(0.76, 0.66, 0.12);
    vec3 pink  = vec3(0.93, 0.62, 0.80);
    vec3 green = vec3(0.22, 0.65, 0.42);
    if (s < 0.2) return coral;
    if (s < 0.4) return sky;
    if (s < 0.6) return olive;
    if (s < 0.8) return pink;
    return green;
}

vec3 hueRotate(vec3 c, float a) {
    float an = a * TAU;
    vec3 k = vec3(0.57735);
    return c * cos(an) + cross(k, c) * sin(an) + k * dot(k, c) * (1.0 - cos(an));
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - vec2(0.5, 0.62) * R) / R.y;   // burst point above center

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float levP  = clamp(audioLevel, 0.0, 1.0);
    float t = TIME * motionSpeed;

    float r = length(q);

    // ── molten core swirl ───────────────────────────────────────────────
    float melt = smoothstep(meltSize * 0.45, 0.0, r) * meltSize;
    float sw = t * 0.15 + amt * 0.6 * midP;
    vec2 qm = q;
    float swirlA = melt * (3.5 + 1.5 * sin(sw));
    float ca = cos(swirlA), sa2 = sin(swirlA);
    qm = mat2(ca, -sa2, sa2, ca) * qm;
    qm += melt * 0.12 * vec2(fbm(q * 9.0 + sw * 0.5) - 0.5,
                             fbm(q * 9.0 + 40.0 - sw * 0.4) - 0.5) * 2.0;

    float ang = atan(qm.x, -qm.y) + t * spinSpeed * 0.12;
    float N = floor(rayCount + 0.5);
    float wpos = ang / TAU * N;
    float wi = floor(wpos);
    float wf = fract(wpos);

    float hw = hash11(mod(wi + N * 10.0, N) * 7.7 + 3.0);
    float hw2 = hash11(mod(wi + N * 10.0, N) * 3.1 + 9.0);

    // wobbling wedge edges (hand-cut feel)
    float edgeWob = 0.06 * sin(r * 30.0 + t * 0.6 + wi * 3.0);
    float inWedge = wf + edgeWob;

    // ── ray fill: candy ink or zebra ────────────────────────────────────
    float slowWalk = t * 0.01;
    vec3 ink = candy(hw + floor(slowWalk + hw2) * 0.2);
    ink = hueRotate(ink, hueShift);

    float isZebra = step(1.0 - zebraAmt, hw2);
    // zebra stripes race along the radius, edges wriggle
    float race = t * raceSpeed * 0.6 + amt * 0.3 * midP;
    float zfreq = 26.0 + 14.0 * hw;
    float zed = sin(r * zfreq - race * 4.0 + 1.5 * fbm(qm * 6.0 + wi));
    float zebra = smoothstep(-0.15, 0.15, zed);
    vec3 zc = mix(vec3(0.06, 0.05, 0.06), vec3(0.94, 0.90, 0.82), zebra);
    vec3 col = mix(ink, zc, isZebra);

    // beat cascade: stripes flash white racing outward on beats
    float casc = exp(-abs(fract(r * 2.0 - t * 0.5) - 0.5) * 9.0);
    col += vec3(1.0, 0.95, 0.85) * casc * amt * beatP * 0.20;

    // wedge border darkening + stipple dust
    float border = smoothstep(0.0, 0.10, wf) * smoothstep(1.0, 0.90, wf);
    col *= 0.82 + 0.18 * border;
    float dotg = step(0.72, vnoise(fc * 0.55));         // stipple field
    float dust = dotg * speckleAmt * (0.35 + 0.65 * (1.0 - border));
    dust *= 1.0 + amt * 0.5 * highP;
    col = mix(col, col * 0.35, dust * 0.5);
    col = mix(col, vec3(0.95, 0.9, 0.8), dust * 0.12);

    // radial ink deepening toward the burst point + level breathing
    col *= (0.85 + 0.25 * smoothstep(0.0, 0.5, r)) * (1.0 + amt * 0.15 * levP);
    // bass fattens rays: brighten wedge centers on bass
    col *= 1.0 + amt * 0.15 * bassP * border;

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.018;
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
