/*{
  "DESCRIPTION": "Red Vortex — the marbled zebra whirlpool: ragged red stripes on warm cream spiraling into an off-center eye, edges wriggling like hand-inked marble, the whole vortex turning and drinking itself inward forever. Both inks are pickers. Bass widens the stripes, mids stir the wriggle, beats send a swallow pulse down the spiral, highs fray the edges.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "stripeCount", "LABEL": "Stripe Density","TYPE": "float", "MIN": 4.0, "MAX": 16.0, "DEFAULT": 9.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "twistAmt",    "LABEL": "Spiral Twist",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "raggedAmt",   "LABEL": "Ink Ragged",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.65, "GROUP": "Shape / Geometry" },
    { "NAME": "spinSpeed",   "LABEL": "Vortex Spin",   "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.4,  "GROUP": "Motion / Animation" },
    { "NAME": "drinkSpeed",  "LABEL": "Inward Flow",   "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "colorA",      "LABEL": "Ink Color",     "TYPE": "color", "DEFAULT": [0.85, 0.16, 0.12, 1.0], "GROUP": "Color" },
    { "NAME": "colorB",      "LABEL": "Paper Color",   "TYPE": "color", "DEFAULT": [0.94, 0.90, 0.80, 1.0], "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.05, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.25, "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// RED VORTEX — after the red/cream marbled swirl reference.
//   Log-polar spiral field: stripes are bands of (log r · density +
//   angle · twist + inward clock), so they wind into the eye and the
//   drink clock pulls them in forever without a seam. The marbling:
//   the band coordinate is displaced by two octav es of domain-warped
//   fbm whose strength grows mid-radius (reference stripes are calm
//   at the rim, frantic near the eye), and the threshold edge gets a
//   second high-frequency fray so every stripe boundary is ragged
//   hand-ink, not vector. Two-tone flat with pixel AA + paper grain.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
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
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.13 + vec2(7.3, 3.1);
        a *= 0.5;
    }
    return v;
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - vec2(0.5, 0.52) * R) / R.y;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float levP  = clamp(audioLevel, 0.0, 1.0);
    float t = TIME * motionSpeed;

    float r = length(q);
    float a = atan(q.y, q.x) + t * spinSpeed * 0.15;

    // ── spiral band coordinate ──────────────────────────────────────────
    float drink = t * drinkSpeed * 0.35 + amt * 0.4 * midP;
    float lr = log(max(r, 1e-4));
    float band = lr * stripeCount * 0.55
               + a / TAU * stripeCount * twistAmt * 2.2
               + drink;

    // ── marbling displacement, stronger mid-radius ──────────────────────
    float wobStr = raggedAmt * (0.5 + 0.9 * smoothstep(0.55, 0.12, r));
    vec2 wp = q * 5.0 + vec2(t * 0.03, -t * 0.02);
    float wob = (fbm(wp + fbm(wp * 1.7) * 1.2) - 0.5) * 2.0;
    band += wob * wobStr * 1.6;
    // high-frequency edge fray
    float fray = (vnoise(q * 60.0 + t * 0.1) - 0.5) * raggedAmt;
    fray *= 1.0 + amt * 0.6 * highP;
    band += fray * 0.35;

    // beat swallow pulse: a widening wave rides down the spiral
    float pulse = exp(-abs(fract(lr * 1.2 + t * 0.25) - 0.5) * 8.0) * amt * beatP;
    float duty = 0.5 + amt * 0.10 * bassP + pulse * 0.12;

    // ── two-tone with pixel-true AA on the winding edge ─────────────────
    float px = fwidth(band) * 1.2 + 1e-4;
    float sb = fract(band);
    float ink = smoothstep(duty + px, duty - px, sb);
    // stripes thin out and vanish inside the eye
    ink *= smoothstep(0.012, 0.05, r);

    vec3 col = mix(colorB.rgb, colorA.rgb, ink);
    // ink density modulation (screen-print unevenness) + level breath
    col *= (0.94 + 0.08 * vnoise(q * 9.0 + 3.0)) * (1.0 + amt * 0.10 * levP);
    // the eye: deep ink pool
    col = mix(colorA.rgb * 0.55, col, smoothstep(0.0, 0.035, r));

    // paper grain
    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.02;
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
