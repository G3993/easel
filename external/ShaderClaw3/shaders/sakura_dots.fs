/*{
  "DESCRIPTION": "Sakura Dots — a wall of glossy 3D candy dots breathing color from the inside out: a hex grid of shaded spheres carries a radial bloom that runs from a deep wine core through hot pink to porcelain at the edges, while a slow color wave rolls outward through the dots. Bass swells the dark heart, beats send a bright ripple ring through the dot sizes, highs sparkle the gloss.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "dotScale",     "LABEL": "Dot Scale",      "TYPE": "float", "MIN": 8.0, "MAX": 40.0, "DEFAULT": 20.0, "GROUP": "Shape / Geometry" },
    { "NAME": "coreSize",     "LABEL": "Core Size",      "TYPE": "float", "MIN": 0.1, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Shape / Geometry" },
    { "NAME": "gloss",        "LABEL": "Gloss",          "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Color" },
    { "NAME": "hueBase",      "LABEL": "Hue Base",       "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.93, "GROUP": "Color" },
    { "NAME": "hueRange",     "LABEL": "Hue Range",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.12, "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",     "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "waveSpeed",    "LABEL": "Wave Speed",     "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.25, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// SAKURA DOTS — from the SAKURA halftone poster: essence fusion of a
//   halftone dot grid, orby's airbrushed sphere shading, and the wave-
//   interference ripple. Hex-offset grid; every cell is a lit sphere
//   (diffuse from an upper-left key light + moving specular + contact
//   shadow) whose COLOR comes from a radial gradient centered mid-frame:
//   near-black wine at the heart → saturated hot hue → pale porcelain at
//   the rim, with a slow luminous wave rolling outward so the poster
//   breathes. Dot radius carries the ripple too, so beats physically
//   swell a ring of dots as it travels.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

// hue → pastel-capable rgb via cos palette biased warm
vec3 hueCol(float h) { return clamp(0.5 + 0.62 * cos(TAU * h + vec3(0.0, 2.094, 4.188)), 0.0, 1.0); }

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * waveSpeed * 0.5;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ── hex-offset dot grid ─────────────────────────────────────────────
    float s = dotScale;
    vec2 g = q * s;
    float row = floor(g.y);
    g.x += 0.5 * mod(row, 2.0);                    // hex offset
    vec2 id = vec2(floor(g.x), row);
    vec2 lp = fract(vec2(g.x, g.y)) - 0.5;         // local cell coords

    // cell center back in screen units → radial field
    vec2 cc = (id + 0.5 - vec2(0.5 * mod(row, 2.0), 0.0)) / s;
    float rad = length(cc - vec2(0.0, 0.02));

    // ── the breathing radial bloom ──────────────────────────────────────
    float core = coreSize * (1.0 + amt * 0.22 * bassP + 0.04 * sin(t * 0.6));
    float ring = sin(rad * 9.0 - t * 1.6);                       // outward color wave
    float f = smoothstep(0.0, 1.05, rad / max(core + 0.55, 1e-3));
    // beat ripple traveling outward through dot size
    float pz = fract(t * 0.35);
    float ripple = exp(-pow((rad - pz * 1.2) * 7.0, 2.0)) * amt * beatP;

    // color: wine-black core → hot hue → porcelain rim
    vec3 heart = hueCol(hueBase) * 0.16;
    vec3 hot   = hueCol(hueBase + hueRange * 0.4);
    vec3 pale  = mix(vec3(0.97, 0.95, 0.96), hueCol(hueBase + hueRange), 0.22);
    vec3 dotCol = mix(heart, hot, smoothstep(0.12, 0.55, f + 0.05 * ring));
    dotCol = mix(dotCol, pale, smoothstep(0.55, 1.0, f + 0.05 * ring));
    dotCol *= 1.0 + 0.10 * ring * (1.0 - f);

    // ── glossy sphere shading inside the cell ───────────────────────────
    // eased breathing pump: a smoothstep-shaped wave (dwell-swell-dwell, not a
    // raw sin) travels outward through the dot radii; neighbours are phase-
    // offset by radius plus a whisper of per-cell lag, so the pulse flows
    // across the grid like liquid. Audio deepens the pump via the kneed,
    // pre-smoothed bass follower — amplitude only, chop-free.
    float pumpPh = fract(t * 0.30 - rad * 0.85 - 0.05 * hash21(id * 0.37));
    float tri = abs(pumpPh * 2.0 - 1.0);                 // 1→0→1 triangle
    float eased = 1.0 - smoothstep(0.0, 1.0, tri);       // smooth in/out
    eased = eased * eased * (3.0 - 2.0 * eased);         // soft dwell at both ends
    float pumpDepth = 0.16 * (0.9 + 0.9 * amt * bassP + 0.3 * amt * levelP);
    float dr = 0.43 * (1.0 + pumpDepth * (eased - 0.35)
                       + 0.22 * ripple - 0.05 * sin(rad * 9.0 - t * 1.6));
    float r2 = dot(lp, lp);
    float e = length(lp) / max(dr, 1e-4);
    float mask = smoothstep(1.0, 0.965, e);
    float zs = sqrt(max(1.0 - min(e * e, 1.0), 0.0));            // sphere height
    vec3 n = normalize(vec3(lp / max(dr, 1e-4), zs + 0.15));
    vec3 L = normalize(vec3(-0.45, 0.6, 0.66));
    float diff = 0.55 + 0.45 * max(dot(n, L), 0.0);
    float spec = pow(max(dot(n, normalize(L + vec3(0.0, 0.0, 1.0))), 0.0), 42.0);
    spec *= gloss * (1.0 + amt * 0.9 * highP);

    // background = darkened bloom color (grout between dots)
    vec3 col = dotCol * 0.72;
    vec3 sph = dotCol * diff + vec3(1.0) * spec * 0.85;
    sph *= 1.0 + 0.9 * ripple + 0.12 * (eased - 0.35);   // pump carries light too
    // contact shadow at the dot's lower rim
    sph *= 1.0 - 0.25 * smoothstep(0.55, 1.0, e) * smoothstep(0.0, -0.4, lp.y);
    col = mix(col, sph, mask);

    // ── finish ──────────────────────────────────────────────────────────
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.018 * gr;
    col *= brightness * mix(1.0, 0.80 + 0.32 * levelP + 0.26 * clamp(audioBass, 0.0, 1.0)
                                 + 0.14 * clamp(audioMid, 0.0, 1.0) + 0.14 * beatP, amt);
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
