/*{
  "DESCRIPTION": "Star Foil — the sticker-foil sparkle sheet: four-point diamond stars in candy colors scattered on an offset grid over a huge drifting rainbow gradient, like holographic wrapping paper. Every star twinkles on its own clock — swelling, glinting and settling — while the rainbow field slides beneath. Bass swells the stars, mids drift the rainbow, beats detonate a twinkle burst, highs fire tiny glints.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "starScale",   "LABEL": "Star Density",  "TYPE": "float", "MIN": 3.0, "MAX": 10.0, "DEFAULT": 5.5,  "GROUP": "Shape / Geometry" },
    { "NAME": "starSize",    "LABEL": "Star Size",     "TYPE": "float", "MIN": 0.4, "MAX": 1.2,  "DEFAULT": 0.8,  "GROUP": "Shape / Geometry" },
    { "NAME": "spikeSharp",  "LABEL": "Spike Sharp",   "TYPE": "float", "MIN": 0.2, "MAX": 1.0,  "DEFAULT": 0.65, "GROUP": "Shape / Geometry" },
    { "NAME": "twinkleSpeed","LABEL": "Twinkle Speed", "TYPE": "float", "MIN": 0.0, "MAX": 2.0,  "DEFAULT": 0.6,  "GROUP": "Motion / Animation" },
    { "NAME": "driftSpeed",  "LABEL": "Rainbow Drift", "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.4,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.08, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.4,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// STAR FOIL — after the star-sticker rainbow sheet reference.
//   Background: a large-scale two-axis rainbow field (hot pink → violet
//   → blue → green → gold) drifting diagonally with a soft foil-fold
//   sheen, plus vintage print grain. Stars: an offset (brick) grid;
//   each cell holds one four-point star — the classic concave diamond
//   star SDF |x|^k + |y|^k — at a hashed size, candy ink and personal
//   twinkle clock: it swells, fires a white glint cross at peak, and
//   settles. Beat detonates a coordinated twinkle wave. Stars get a
//   1px darker ink rim so they sit ON the foil like stickers.
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

vec3 candy(float k) {
    float s = fract(k);
    vec3 red   = vec3(0.93, 0.13, 0.16);
    vec3 orng  = vec3(0.98, 0.58, 0.10);
    vec3 pink  = vec3(0.98, 0.22, 0.55);
    vec3 grn   = vec3(0.16, 0.68, 0.28);
    vec3 blu   = vec3(0.16, 0.52, 0.90);
    vec3 vio   = vec3(0.55, 0.32, 0.85);
    float v = s * 6.0;
    return v < 1.0 ? red : (v < 2.0 ? orng : (v < 3.0 ? pink : (v < 4.0 ? grn : (v < 5.0 ? blu : vio))));
}

// concave 4-point star: returns signed distance-ish field
float star4(vec2 p, float rad, float k) {
    p = abs(p) / rad;
    float v = pow(p.x, k) + pow(p.y, k);
    return (pow(v, 1.0 / k) - 1.0) * rad;
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 uv = fc / R;
    vec2 q = fc / R.y;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float levP  = clamp(audioLevel, 0.0, 1.0);
    float t = TIME * motionSpeed;

    // ── drifting rainbow foil ───────────────────────────────────────────
    float drift = t * driftSpeed * 0.05 + amt * 0.15 * midP;
    float fu = uv.x * 0.55 + uv.y * 0.75 + drift;
    float fv = uv.x * 0.9 - uv.y * 0.3 - drift * 0.6;
    vec3 foil = vec3(0.55 + 0.45 * cos(TAU * (fu + hueShift + 0.00)),
                     0.52 + 0.46 * cos(TAU * (fu + hueShift + 0.33)),
                     0.55 + 0.45 * cos(TAU * (fu + hueShift + 0.66)));
    // second axis folds the rainbow like crumpled foil
    foil = mix(foil, foil.gbr, 0.30 + 0.25 * sin(fv * TAU * 0.8));
    // foil sheen band sliding through
    foil += vec3(0.20) * exp(-pow((fract(fu * 0.5 + 0.15) - 0.5) * 4.5, 2.0)) * 0.6;
    foil *= 0.9 + 0.1 * vnoise(fc * 0.5);          // vintage grain
    vec3 col = foil;

    // ── offset star grid ────────────────────────────────────────────────
    float N = starScale;
    vec2 g = q * N;
    float row = floor(g.y);
    g.x += 0.5 * mod(row, 2.0);
    vec2 id = vec2(floor(g.x), row);
    vec2 lp = fract(g) - 0.5;

    float h1 = hash21(id * 3.3 + 1.0);
    float h2 = hash21(id * 7.9 + 5.0);

    // personal twinkle clock: swell → glint → settle
    float clk = fract(t * twinkleSpeed * (0.12 + 0.18 * h1) + h2);
    float swell = 0.75 + 0.45 * (smoothstep(0.0, 0.35, clk) * smoothstep(0.9, 0.45, clk));
    // beat detonation wave through the sheet
    float det = exp(-abs(mod(id.x + id.y, 8.0) - mod(t * 6.0, 8.0)) * 1.2) * amt * beatP;
    swell += det * 0.35 + amt * 0.18 * bassP;

    float rad = min(0.26 * starSize * (0.6 + 0.45 * h2) * swell, 0.42);
    float k = mix(0.9, 0.45, spikeSharp);          // lower k = spikier
    float d = star4(lp, rad, k);
    float aa = 1.8 * N / R.y;

    vec3 ink = candy(h1 * 0.999 + hueShift);
    // sticker rim: darker edge line
    float rim = smoothstep(aa, -aa, d) - smoothstep(-aa, -3.0 * aa, d);
    float m = smoothstep(aa, -aa, d);
    // subtle inner gradient (foil catches the star)
    ink *= 0.88 + 0.24 * (lp.y / max(rad, 1e-3) * 0.5 + 0.5);
    ink *= 1.0 + amt * 0.15 * levP;
    col = mix(col, ink, m);
    col = mix(col, ink * 0.55, clamp(rim, 0.0, 1.0) * 0.8);

    // white glint cross at twinkle peak
    float peak = smoothstep(0.30, 0.42, clk) * smoothstep(0.60, 0.48, clk);
    peak = max(peak, det);
    float gl = exp(-abs(lp.x) * 60.0) * exp(-abs(lp.y) * 14.0)
             + exp(-abs(lp.y) * 60.0) * exp(-abs(lp.x) * 14.0);
    col += vec3(1.0, 0.98, 0.92) * gl * peak * (0.45 + amt * 0.5 * highP) * step(d, rad * 0.4);

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.016;
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
