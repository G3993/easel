/*{
  "DESCRIPTION": "Color Prints — a living riso poster: a big grid of gradient circles, moon-phase half-discs and rainbow gradient bars on printed paper, quadrant color-blocked like the reference. Every cell steps through its own shape sequence — discs eclipse into slivers, bars pour their gradients, staircases of cells cascade diagonally — with paper grain and a slight print emboss lifting each cell in 3D. Bass pumps the discs, mids pour the gradients, beats advance the sequence cascade, highs sparkle the grain.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "gridN",       "LABEL": "Grid Cells",    "TYPE": "float", "MIN": 6.0, "MAX": 16.0, "DEFAULT": 10.0, "GROUP": "Shape / Geometry" },
    { "NAME": "shapeMix",    "LABEL": "Bars vs Discs", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Shape / Geometry" },
    { "NAME": "embossAmt",   "LABEL": "Print Emboss",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Shape / Geometry" },
    { "NAME": "morphSpeed",  "LABEL": "Morph Speed",   "TYPE": "float", "MIN": 0.0, "MAX": 2.0,  "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "pourSpeed",   "LABEL": "Gradient Pour", "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.6,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "hueDrift",    "LABEL": "Hue Drift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.25, "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.06, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.28, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// COLOR PRINTS — after the riso gradient-shapes poster reference.
//   Paper sheet with a wide margin; inside, a cell grid split into four
//   quadrant plates (pink/green/blue/orange tint fields like the
//   original). Each cell draws one of: a flat disc, an eclipsing
//   moon-phase disc (circle cut by a sliding circle), or a vertical
//   rainbow gradient bar. A per-cell clock (staggered along the
//   diagonal) steps each disc through its phase sequence so whole
//   staircases animate together. Gradient bars pour continuously.
//   Print emboss = darkened SE edge + light NW edge per cell.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

vec3 pal(float h) {
    // punchy riso ink ramp: red→orange→yellow→green→blue→magenta
    return clamp(vec3(
        0.55 + 0.55 * cos(TAU * (h + 0.00)),
        0.50 + 0.55 * cos(TAU * (h + 0.67)),
        0.50 + 0.55 * cos(TAU * (h + 0.33))), 0.0, 1.0);
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 uv = fc / R;
    float asp = R.x / R.y;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);

    float t = TIME * motionSpeed;

    // paper + margin frame
    vec3 paper = vec3(0.93, 0.92, 0.89);
    float grain = hash21(fc * 0.7) - 0.5;
    paper += grain * 0.03;

    vec2 m = vec2(0.05 / asp, 0.05);
    vec2 inUV = (uv - m) / (1.0 - 2.0 * m);
    float inside = step(0.0, inUV.x) * step(inUV.x, 1.0) * step(0.0, inUV.y) * step(inUV.y, 1.0);

    float N = floor(gridN + 0.5);
    vec2 g = inUV * vec2(N * min(asp, 1.6), N);
    vec2 id = floor(g);
    vec2 lp = fract(g) - 0.5;
    float aa = 2.0 * N / R.y;

    float h = hash21(id + 7.3);
    float h2 = hash21(id * 1.7 + 3.1);

    // quadrant plates: 4 tint fields behind the shapes
    vec2 quad = step(vec2(0.5), inUV);
    float qi = quad.x + quad.y * 2.0;
    vec3 plate = pal(qi * 0.23 + 0.05 + hueDrift * 0.2 * sin(t * 0.05));
    plate = mix(plate, paper, 0.45);                       // printed tint field

    // slow hue drift across the sheet
    float baseHue = h * 0.9 + hueDrift * (0.08 * t + 0.2 * sin(t * 0.11));

    // ── per-cell clock: diagonal stagger + beat advance ─────────────────
    float clk = t * morphSpeed * 0.35 + (id.x + id.y) * 0.11 + amt * 0.25 * midP;
    float phase = fract(clk + h2 * 0.3);

    vec3 col = plate;
    float isBar = step(1.0 - shapeMix, hash21(id * 3.7 + 11.0));

    if (isBar > 0.5) {
        // ── vertical rainbow gradient bar, pouring ──────────────────────
        float bw = 0.36;
        float bar = smoothstep(bw + aa, bw - aa, abs(lp.x))
                  * smoothstep(0.48 + aa, 0.48 - aa, abs(lp.y));
        float pour = lp.y * 1.3 + t * pourSpeed * 0.5 + h * 4.0 + amt * 0.4 * midP;
        vec3 barC = pal(baseHue + pour * 0.35);
        // vertical sheen for a rounded-print feel + level warmth
        barC *= (0.85 + 0.25 * cos(lp.x / bw * 1.57))
              * (1.0 + amt * 0.14 * clamp(audioLevel, 0.0, 1.0));
        col = mix(col, barC, bar);
    } else {
        // ── disc with moon-phase eclipse morph ──────────────────────────
        float rad = 0.40 * (1.0 + amt * 0.10 * bassP);
        float d = length(lp);
        float disc = smoothstep(rad + aa, rad - aa, d);
        // eclipsing cutter circle slides across with the cell clock
        float ph = phase * TAU;
        float cutX = cos(ph) * rad * 2.2;                  // sweeps through
        float dcut = length(lp - vec2(cutX, 0.0));
        float cut = smoothstep(rad + aa, rad - aa, dcut);
        float moon = clamp(disc - cut * 0.94, 0.0, 1.0);
        vec3 discC = pal(baseHue);
        // radial ink density: darker rim = printed disc depth
        discC *= 0.80 + 0.28 * smoothstep(rad, rad * 0.2, d);
        // gradient across the disc face (the reference's dual-tone discs)
        discC = mix(discC, pal(baseHue + 0.14), clamp(lp.y + 0.5, 0.0, 1.0) * 0.55);
        discC *= 1.0 + amt * 0.14 * clamp(audioLevel, 0.0, 1.0);
        col = mix(col, discC, moon);
        // beat: freshly-advanced discs flash their rim
        float rim = smoothstep(rad, rad - 0.05, d) - smoothstep(rad - 0.10, rad - 0.15, d);
        col += pal(baseHue + 0.5) * max(rim, 0.0) * amt * beatP * 0.35;
    }

    // ── print emboss: cell plate lifts off the paper ────────────────────
    float ex = smoothstep(0.5, 0.42, abs(lp.x)) * smoothstep(0.5, 0.42, abs(lp.y));
    vec2 eg = vec2(
        smoothstep(0.5, 0.42, abs(lp.x + 0.04)) - smoothstep(0.5, 0.42, abs(lp.x - 0.04)),
        smoothstep(0.5, 0.42, abs(lp.y + 0.04)) - smoothstep(0.5, 0.42, abs(lp.y - 0.04)));
    col *= 1.0 + embossAmt * 0.10 * (eg.x + eg.y);
    col = mix(col, col * (0.94 + 0.06 * ex), embossAmt);

    // compose sheet: outside margin = plain paper
    col = mix(paper, col, inside);

    // paper grain + high sparkle
    col += grain * (0.035 + amt * 0.03 * highP);
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
