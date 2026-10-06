/*{
  "DESCRIPTION": "La B Petals — silk petals in deep blush, wine and ivory tumbling slowly through dark space on a charcoal-plum gradient. Six parallax depth layers with true depth-of-field: near petals bloom large and soft, the focus band renders crisp satin shading with bright rim highlights and a center crease, far petals recede small and dim. Bass gives the fall a gentle swell, mids push the flutter, beats gleam the satin rims, highs sparkle dust motes at the focus plane.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "petalScale",   "LABEL": "Petal Density", "TYPE": "float", "MIN": 0.5, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "fallSpeed",    "LABEL": "Fall Speed",    "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "tumbleAmt",    "LABEL": "Tumble",        "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "focusDepth",   "LABEL": "Focus Plane",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.38, "GROUP": "Shape / Geometry" },
    { "NAME": "dofAmt",       "LABEL": "Depth Of Field","TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "paletteShift", "LABEL": "Palette Shift", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.05, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.25, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.2,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// LA B PETALS — deep-color silk petals falling through dark space.
//   The rejected version was washed out; this one glows AGAINST a deep
//   charcoal-plum gradient. Six parallax layers, back to front. Each
//   petal is a hashed teardrop with an asymmetric width profile, rotated
//   in-plane AND foreshortened by a tumble phase so it reads as a 3D
//   sheet turning while it falls. Shading is satin: soft diffuse from a
//   curved fake normal, a tight pow-26 specular band that sweeps as the
//   petal tumbles, a crisp bright rim ring, and a dark center crease.
//   TRUE depth-of-field: circle-of-confusion grows with distance from
//   the focus plane — near petals go large, translucent and soft
//   (bokeh-like), the focus band is pixel-crisp, far petals shrink and
//   sink toward the background haze. Fine dust motes sparkle at the
//   focus plane. Audio: bass = gentle sway/size swell, mids = additive
//   flutter phase, beats = soft gleam on rims + specular, highs = motes.
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
float fbm2(vec2 p) {
    float v = 0.0, a = 0.55;
    for (int i = 0; i < 2; i++) {
        v += a * vnoise(p);
        p = p * 2.17 + vec2(5.2, 1.3);
        a *= 0.5;
    }
    return v * 1.23;
}
vec3 hueShift(vec3 c, float a) {
    vec3 k = vec3(0.57735);
    float cs = cos(a), sn = sin(a);
    return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * motionSpeed * 0.55;
    float amt = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);

    float tf = t + amt * 0.5 * midP;           // flutter phase, additive push
    float gleam = amt * beatP;                 // beat: satin gleam envelope
    float swell = 1.0 + amt * 0.08 * bassP;    // bass: gentle size swell

    // ── deep charcoal-plum gradient — petals must GLOW against this ────
    float sy = clamp(q.y + 0.5, 0.0, 1.0);
    vec3 bgBot = vec3(0.040, 0.026, 0.052);
    vec3 bgTop = vec3(0.120, 0.058, 0.115);
    vec3 col = mix(bgBot, bgTop, sy);
    // slow drifting haze, kept deep — texture without wash
    float haze = fbm2(q * 1.6 + vec2(0.0, t * 0.03));
    col += vec3(0.085, 0.038, 0.080) * haze * haze;
    // faint warm pool upper-left, like a distant store light
    col += vec3(0.10, 0.045, 0.032) * exp(-dot(q - vec2(-0.42, 0.34), q - vec2(-0.42, 0.34)) * 3.2);

    // ── petal colors: blush / wine / ivory ─────────────────────────────
    vec3 blush = vec3(0.855, 0.415, 0.50);
    vec3 wine  = vec3(0.435, 0.058, 0.155);
    vec3 ivory = vec3(0.93, 0.875, 0.79);

    vec3 L = normalize(vec3(-0.36, 0.58, 0.72));

    // ── six depth layers, far → near ───────────────────────────────────
    for (int l = 0; l < 6; l++) {
        float d01 = 1.0 - (float(l) + 0.5) / 6.0;         // 0.92 far → 0.08 near
        float gs = mix(2.3, 8.8, d01) * petalScale;       // near = few big petals
        float coc = dofAmt * abs(d01 - focusDepth);       // circle of confusion
        float crispF = 1.0 - clamp(coc * 3.2, 0.0, 1.0);  // 1 at focus plane

        vec2 pp = q * gs;
        pp.y += t * mix(1.7, 0.45, d01) * fallSpeed + float(l) * 3.7;
        pp.x += 0.22 * sin(tf * 0.23 + d01 * 7.0) * (0.4 + 0.6 * tumbleAmt);
        vec2 id = floor(pp);
        vec2 lp = fract(pp) - 0.5;

        float h1 = hash21(id + float(l) * 17.71);
        if (h1 < 0.24) continue;                          // sparse, airy field
        float h2 = hash21(id * 1.71 + 31.0 + float(l));
        float h3 = hash21(id * 2.33 + 57.0 + float(l));

        // per-petal placement + gentle flutter drift inside the cell
        lp -= (vec2(h2, h3) - 0.5) * 0.52;
        lp.x -= 0.085 * sin(tf * (0.5 + 0.5 * h2) + h3 * TAU) * (0.3 + 0.7 * tumbleAmt);
        lp.y += 0.045 * sin(tf * (0.4 + 0.4 * h3) + h2 * TAU);

        // in-plane rotation + out-of-plane tumble (foreshortening)
        float rot = tf * (0.22 + 0.42 * h2) * sign(h3 - 0.5) * (0.25 + 0.75 * tumbleAmt) + h1 * TAU;
        float ca = cos(rot), sa = sin(rot);
        vec2 rp = mat2(ca, -sa, sa, ca) * lp;
        float tum = tf * (0.30 + 0.50 * h3) + h2 * TAU;
        float fore = mix(1.0, 0.28 + 0.72 * abs(cos(tum)), tumbleAmt);

        float s = (0.165 + 0.125 * h2) * swell;
        // teardrop: asymmetric width profile along the petal axis
        float ny = clamp(rp.y / s, -1.35, 1.35);
        float wp = max(0.34 + 0.72 * (1.0 - ny * ny * 0.74) + 0.10 * ny, 0.06);
        vec2 e = vec2(rp.x / (s * 0.60 * fore * wp), rp.y / s);
        float r = length(e);

        // DOF-aware edge: crisp at focus, wide+translucent when defocused
        float aaE = 1.7 * gs / (s * R.y) + coc * 2.3;
        float mask = smoothstep(1.0 + aaE, 1.0 - aaE, r);
        if (mask < 0.004) continue;
        mask *= mix(0.42, 1.0, crispF * crispF * 0.5 + 0.5 * crispF)
              * mix(1.0, 0.62, smoothstep(0.55, 0.92, d01));   // far petals sink

        // ── satin shading on a curved sheet ────────────────────────────
        float zn = sqrt(max(1.0 - min(r * r, 1.0), 0.0));
        vec3 n = normalize(vec3(e.x * 0.92, ny * 0.50, zn + 0.34));
        float dif = 0.40 + 0.66 * max(dot(n, L), 0.0);
        float band = pow(max(dot(n, normalize(L + vec3(0.0, 0.0, 1.0))), 0.0), 26.0);
        float rim = pow(clamp(1.0 - zn, 0.0, 1.0), 2.3) * smoothstep(1.0, 0.80, r);
        float crease = exp(-abs(e.x) * 8.5) * 0.34 * zn;

        // color pick, wine biased so the field stays deep
        float hcv = fract(h1 * 7.31);
        vec3 pc = mix(mix(blush, wine, step(0.38, hcv)), ivory, step(0.78, hcv));
        vec3 rimC = mix(pc * 1.7, vec3(1.0, 0.86, 0.82), 0.45);

        vec3 shaded = pc * dif * (1.0 - crease);
        shaded += vec3(1.0, 0.93, 0.87) * band * (0.70 + 0.45 * gleam) * crispF;
        shaded += rimC * rim * (0.62 + 0.35 * gleam) * (0.35 + 0.65 * crispF);
        // atmosphere: far petals cool toward the bg plum
        shaded = mix(shaded, bgTop * 2.1, d01 * 0.30);
        // defocused petals go translucent-luminous, not muddy
        shaded = mix(shaded * 1.12, shaded, crispF);

        col = mix(col, shaded, mask);
    }

    // ── dust motes sparkling at the focus plane (highs) ────────────────
    vec2 mp = q * 22.0 + vec2(0.0, t * 0.9);
    vec2 mid_ = floor(mp);
    vec2 mlp = fract(mp) - 0.5;
    float mh = hash21(mid_ * 3.17 + 9.0);
    mlp -= (vec2(hash21(mid_ + 4.7), mh) - 0.5) * 0.6;
    float mote = exp(-dot(mlp, mlp) * 90.0) * step(0.86, mh);
    float twk = 0.5 + 0.5 * sin(t * 2.1 + mh * 41.0);
    col += vec3(1.0, 0.88, 0.80) * mote * twk * (0.10 + 0.30 * amt * highP);

    // ── finish: vignette, palette rotation, grain ──────────────────────
    col *= 1.0 - 0.34 * smoothstep(0.50, 1.12, length(q * vec2(0.85, 1.0)));
    col = hueShift(col, paletteShift * TAU);
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.012 * gr;
    return max(col, 0.0);
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
        // audio lift applied post-trail so dips stay visible
        float lvl = clamp(audioLevel, 0.0, 1.0);
        col *= brightness * mix(1.0, 0.64 + 0.36 * lvl, audioReact)
             * (1.0 + audioReact * 0.06 * clamp(audioBeatPulse, 0.0, 1.0));
        // HD finish: S-curve contrast + saturation richness
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        gl_FragColor = vec4(col, 1.0);
    }
}
