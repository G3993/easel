/*{
  "DESCRIPTION": "Vinyl Pour — glossy superflat paint in slow motion: fat blobs of red, cobalt, yellow, pink and cream pour and merge across a cool gray studio wall, each puddle dead-flat with a dark meniscus outline and a wet vinyl highlight, splitting and swallowing each other like lava-lamp acrylic. Bass fattens the pour, beats wobble the puddles apart, mids speed the drift, level polishes the gloss.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "blobFat",      "LABEL": "Blob Fatness",  "TYPE": "float", "MIN": 0.4, "MAX": 1.6, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "outlineAmt",   "LABEL": "Outline",       "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "gloss",        "LABEL": "Vinyl Gloss",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.65, "GROUP": "Color" },
    { "NAME": "paletteShift", "LABEL": "Palette Shift", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "wallTone",     "LABEL": "Wall Tone",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.65, "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "flowSpeed",    "LABEL": "Pour Speed",    "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.4,  "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.2, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// VINYL POUR — from the Murakami-style glossy vinyl reference: a metaball
//   winner-takes-flat system. 12 drifting gaussian blobs, each tagged one
//   of 5 paint colors; per pixel the per-COLOR fields compete and the
//   strongest color paints a dead-flat fill (superflat!), with the dark
//   meniscus drawn where the total field crosses its threshold and a wet
//   specular highlight computed from the field gradient — so merged
//   puddles share one continuous outline and one continuous gloss, just
//   like real poured vinyl. Fuses: orby's metaball lifecycle field, the
//   flat-ink discipline of tangle, and liquid_toy's gloop.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
vec3 hueRotate(vec3 c, float a) {
    float cs = cos(a), sn = sin(a);
    mat3 m = mat3(
        0.299 + 0.701*cs + 0.168*sn, 0.587 - 0.587*cs + 0.330*sn, 0.114 - 0.114*cs - 0.497*sn,
        0.299 - 0.299*cs - 0.328*sn, 0.587 + 0.413*cs + 0.035*sn, 0.114 - 0.114*cs + 0.292*sn,
        0.299 - 0.300*cs + 1.250*sn, 0.587 - 0.588*cs - 1.050*sn, 0.114 + 0.886*cs - 0.203*sn);
    return clamp(c * m, 0.0, 2.0);
}

// one blob's field contribution at offset d with radius r
float blobF(vec2 d, float r) {
    float x = dot(d, d) / (r * r);
    return exp(-x * 2.2);
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;
    float A = 0.5 * R.x / R.y;

    float t   = TIME * flowSpeed * 0.35;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ── accumulate per-color fields from 12 pouring blobs ───────────────
    float f0 = 0.0, f1 = 0.0, f2 = 0.0, f3 = 0.0, f4 = 0.0;
    float drift = 1.0 + amt * 0.7 * midP;
    float fat = blobFat * (1.0 + amt * 0.22 * bassP);
    float wob = amt * beatP;
    for (int i = 0; i < 12; i++) {
        float fi = float(i);
        float h1 = hash11(fi * 12.99 + 1.0);
        float h2 = hash11(fi * 4.41 + 7.0);
        float h3 = hash11(fi * 7.77 + 3.0);
        // slow pouring drift: mostly downward, wrapping vertically
        vec2 pos = vec2(
            (h1 * 2.0 - 1.0) * (A + 0.1) + 0.14 * sin(t * (0.3 + 0.4 * h2) * drift + h3 * TAU),
            mix(0.62, -0.62, fract(h2 + t * (0.02 + 0.025 * h3) * drift)));
        pos += wob * 0.03 * vec2(sin(t * 9.0 + fi * 2.0), cos(t * 8.0 + fi));
        float r = fat * (0.13 + 0.14 * h3);
        float f = blobF(q - pos, r);
        float c5 = mod(fi, 5.0);
        if (c5 < 1.0) f0 += f; else if (c5 < 2.0) f1 += f;
        else if (c5 < 3.0) f2 += f; else if (c5 < 4.0) f3 += f; else f4 += f;
    }
    float total = f0 + f1 + f2 + f3 + f4;

    // ── winner-takes-flat paint choice ──────────────────────────────────
    vec3 paint = vec3(0.85, 0.12, 0.10);  float best = f0;       // red
    if (f1 > best) { best = f1; paint = vec3(0.10, 0.30, 0.75); } // cobalt
    if (f2 > best) { best = f2; paint = vec3(1.00, 0.82, 0.05); } // yellow
    if (f3 > best) { best = f3; paint = vec3(0.97, 0.68, 0.78); } // pink
    if (f4 > best) { best = f4; paint = vec3(0.96, 0.94, 0.88); } // cream

    // ── studio wall, meniscus outline, flat fill, wet gloss ─────────────
    vec3 wall = mix(vec3(0.58, 0.58, 0.60), vec3(0.80, 0.80, 0.81), wallTone);
    wall *= 1.0 - 0.10 * smoothstep(0.4, 1.0, length(q));
    float th = 0.30;
    float inside = smoothstep(th, th + 0.02, total);
    float outline = smoothstep(th - 0.035 * outlineAmt, th, total)
                  * smoothstep(th + 0.10, th + 0.035, total);

    // field gradient → wet highlight that flows across merged puddles
    vec2 e = vec2(0.012, 0.0);
    // cheap gradient of the total field via 2 extra probes of the largest term
    float gx = 0.0, gy = 0.0;
    {
        // finite difference on a smooth proxy: sum of all = total sampled at offsets
        float tx = 0.0, ty = 0.0;
        for (int i = 0; i < 12; i++) {
            float fi = float(i);
            float h1 = hash11(fi * 12.99 + 1.0);
            float h2 = hash11(fi * 4.41 + 7.0);
            float h3 = hash11(fi * 7.77 + 3.0);
            vec2 pos = vec2(
                (h1 * 2.0 - 1.0) * (A + 0.1) + 0.14 * sin(t * (0.3 + 0.4 * h2) * drift + h3 * TAU),
                mix(0.62, -0.62, fract(h2 + t * (0.02 + 0.025 * h3) * drift)));
            pos += wob * 0.03 * vec2(sin(t * 9.0 + fi * 2.0), cos(t * 8.0 + fi));
            float r = fat * (0.13 + 0.14 * h3);
            tx += blobF(q + e.xy - pos, r);
            ty += blobF(q + e.yx - pos, r);
        }
        gx = (tx - total) / e.x;
        gy = (ty - total) / e.x;
    }
    vec3 n = normalize(vec3(-gx, -gy, 1.4));
    float spec = pow(max(dot(n, normalize(vec3(-0.4, 0.55, 0.75))), 0.0), 34.0);
    spec *= gloss * (1.0 + amt * 0.5 * levelP);

    vec3 col = wall;
    col = mix(col, paint, inside);
    col = mix(col, vec3(0.08, 0.06, 0.05), outline * outlineAmt * inside);
    col += vec3(1.0) * spec * inside * 0.9;
    // soft drop shadow under every puddle edge
    col *= 1.0 - 0.15 * smoothstep(th, th - 0.10, total) * smoothstep(th - 0.20, th - 0.06, total) * (1.0 - inside);

    if (paletteShift > 0.001) col = hueRotate(col, paletteShift * TAU);
    col *= brightness * mix(1.0, 0.82 + 0.30 * levelP + 0.26 * clamp(audioBass, 0.0, 1.0)
                                 + 0.14 * clamp(audioMid, 0.0, 1.0) + 0.10 * beatP, amt);
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
