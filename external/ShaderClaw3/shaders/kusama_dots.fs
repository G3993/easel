/*{
  "DESCRIPTION": "Kusama Dots — an infinity polka-dot field in Yayoi Kusama's exact palettes: crisp red dots breathing across warm white, or her black-on-yellow and yellow-on-black nets. The whole field warps like fabric in slow waves, dot sizes flow in traveling bands, scattered dots jitter off the grid — and inside a roaming obliteration lens the dots inflate into glossy 3D spheres that cast real shadows before melting flat again. Bass swells the dot waves, mids pour the warp, beats grow the 3D lens and send a growth ripple across the net, highs sparkle the micro-dots.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "dotScale",     "LABEL": "Dot Scale",       "TYPE": "float", "MIN": 8.0, "MAX": 36.0, "DEFAULT": 16.0, "GROUP": "Shape / Geometry" },
    { "NAME": "warpAmt",      "LABEL": "Field Warp",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "sizeWave",     "LABEL": "Size Waves",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "jitterAmt",    "LABEL": "Dot Jitter",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Shape / Geometry" },
    { "NAME": "sphereAmt",    "LABEL": "3D Obliteration", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "rippleAmt",    "LABEL": "Ripples",         "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Motion / Animation" },
    { "NAME": "dotMotion",    "LABEL": "Dot Motion",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "paletteMode",  "LABEL": "Palette R/W-B/Y-Y/B", "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.0, "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",      "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",    "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.1, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.45, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.3, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// KUSAMA DOTS — after Yayoi Kusama's infinity nets and dot obsessions.
//   Three reference behaviors fused into one living field:
//   1. The red-on-white room: a hex dot lattice with hashed per-dot
//      jitter and size variety, on warm gallery white with paper grain.
//   2. The black-on-yellow paintings: dot RADIUS flows in traveling fbm
//      bands (big dots in the wave crests, dense micro-dots filling the
//      valleys), and the whole domain is double-warped so the lattice
//      bends like fabric — her hand-painted drift, not a grid.
//   3. The sculptures: a roaming circular "obliteration lens" inside
//      which dots inflate into lit 3D spheres — diffuse + specular +
//      fresnel + a real contact shadow on the wall — then flatten again
//      as the lens moves on. Beats grow the lens and launch a smooth
//      growth ripple across the net; a barrel warp curves the whole
//      room infinity-mirror style.
//   Palette knob sweeps her three canonical schemes: red/white →
//   black/yellow → yellow/black. Edges are pixel-accurate AA at any
//   resolution. Audio: bass→wave amplitude, mids→warp phase push
//   (chop-free), beat→lens + ripple, highs→micro-dot sparkle; the
//   white-background lift dims below 1 in the composite pass.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
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

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * motionSpeed * 0.4;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ── Kusama palettes: red/white → black/yellow → yellow/black ────────
    float pm = clamp(paletteMode, 0.0, 2.0);
    vec3 red    = vec3(0.91, 0.07, 0.05);
    vec3 nearBk = vec3(0.10, 0.08, 0.05);
    vec3 yellow = vec3(0.96, 0.69, 0.05);
    vec3 white  = vec3(0.965, 0.955, 0.935);
    vec3 dotC = mix(mix(red,   nearBk, clamp(pm, 0.0, 1.0)), yellow, clamp(pm - 1.0, 0.0, 1.0));
    vec3 bgC  = mix(mix(white, yellow, clamp(pm, 0.0, 1.0)), nearBk, clamp(pm - 1.0, 0.0, 1.0));

    // ── infinity-room barrel curvature + fabric warp ────────────────────
    q *= 1.0 + 0.14 * warpAmt * dot(q, q);                       // room curves away
    float tw = t + amt * 0.5 * midP;                             // phase push, chop-free
    vec2 w = q;
    w += warpAmt * 0.10 * vec2(fbm(q * 1.6 + vec2(tw * 0.10, 0.0)) - 0.5,
                               fbm(q * 1.6 + vec2(3.7, -tw * 0.08)) - 0.5) * 2.0;
    w += warpAmt * 0.035 * vec2(sin(q.y * 3.0 + tw * 0.35), sin(q.x * 3.0 - tw * 0.28));

    // ── traveling size-wave bands (the black-on-yellow paintings) ───────
    float wave = fbm(w * 0.9 + vec2(0.0, -tw * 0.10));
    wave = 0.5 + (wave - 0.5) * 2.2;                             // widen contrast
    float waveAmp = sizeWave * (1.0 + amt * 0.45 * bassP);
    // beat growth ripple sweeping out from center, envelope-gated (smooth)
    float rad = length(q);
    float ripple = exp(-pow((rad - fract(t * 0.35) * 1.3) * 5.0, 2.0)) * amt * beatP;

    // ── continuous ripple field: three sources ringing through the net ──
    // (center pool + two roaming drops). Height swells dot radius, the
    // radial derivative physically DISPLACES dots, so rings visibly push
    // through the lattice like water. Bass drives amplitude wide open,
    // level feeds the roamers, so the audio range is much bigger.
    vec2 rs1 = 0.48 * vec2(sin(t * 0.21), cos(t * 0.17) * 0.8);
    vec2 rs2 = 0.42 * vec2(sin(t * 0.13 + 3.1), cos(t * 0.24 + 1.2) * 0.85);
    float rippleGain = rippleAmt * (0.30 + amt * (1.1 * bassP + 0.35 * levelP));

    // ── the roaming 3D obliteration lens ────────────────────────────────
    vec2 lensC = 0.34 * vec2(sin(t * 0.13), cos(t * 0.09) * 0.8);
    float lensR = sphereAmt * (0.26 + 0.05 * sin(t * 0.5) + amt * 0.09 * beatP);
    float f3d = smoothstep(lensR, lensR * 0.45, length(q - lensC)) * sphereAmt;

    // ── hex dot lattice in warped space ─────────────────────────────────
    float aa = 1.8 * dotScale / R.y;                             // pixel-true AA width
    vec2 g = w * dotScale * 0.5;
    float row = floor(g.y);
    g.x += 0.5 * mod(row, 2.0);
    vec2 id = vec2(floor(g.x), row);
    vec2 lp = fract(g) - 0.5;
    float h1 = hash21(id * 1.31 + 7.0);
    float h2 = hash21(id * 2.71 + 3.0);
    // hashed jitter off the grid (the hand-placed room dots)
    lp -= (vec2(h1, h2) - 0.5) * 0.42 * jitterAmt;

    // cell center back in field space (for the ripple field)
    vec2 cc = (id + 0.5 - vec2(0.5 * mod(row, 2.0), 0.0)) / (dotScale * 0.5);

    float rippleH = 0.0;
    vec2  rippleD = vec2(0.0);
    for (int ri = 0; ri < 3; ri++) {
        vec2 src = (ri == 0) ? vec2(0.0) : (ri == 1) ? rs1 : rs2;
        float freq = (ri == 0) ? 18.0 : (ri == 1) ? 24.0 : 21.0;
        float spd  = (ri == 0) ? 2.6  : (ri == 1) ? 3.4  : 3.0;
        vec2 dv = cc - src;
        float dd = length(dv) + 1e-4;
        float phw = dd * freq - tw * spd + float(ri) * 2.1;
        float env = exp(-dd * (1.6 + 0.5 * float(ri)));
        rippleH += sin(phw) * env;
        rippleD += (dv / dd) * cos(phw) * env;
    }
    rippleH *= rippleGain;
    // rings push the dots radially as they pass (rigid per-dot shift = crisp)
    lp -= rippleD * rippleGain * 0.13;

    // per-dot orbital drift: every dot circles its home on its own clock;
    // mids push the phase (chop-free), level + beat widen the orbit
    float om = dotMotion * (0.10 + 0.09 * amt * (0.8 * levelP + 0.6 * beatP));
    float oph = tw * (1.1 + 1.6 * h2) + h1 * TAU;
    lp -= om * vec2(cos(oph), sin(oph));

    // radius: size variety × flowing wave × ripple × 3D inflation
    float rr = (0.16 + 0.26 * h1) * mix(1.0 - 0.45 * waveAmp, 1.0 + 0.30 * waveAmp, wave);
    rr *= 1.0 + 0.35 * ripple + 0.30 * f3d + 0.55 * rippleH;
    rr = min(rr, 0.46);
    float e = length(lp) / max(rr, 1e-4);

    // ── background: gallery wall + paper grain + lens ambience ──────────
    vec3 col = bgC;
    col *= 0.97 + 0.05 * fbm(q * 7.0);                           // wall texture
    col *= 1.0 - 0.10 * warpAmt * smoothstep(0.5, 1.1, rad);     // room falloff
    col *= 1.0 - 0.08 * f3d;                                     // lens dims the wall

    // contact shadow on the wall under 3D spheres (offset toward lower-left)
    float eSh = length(lp - rr * vec2(-0.30, 0.34)) / max(rr, 1e-4);
    col *= 1.0 - 0.38 * f3d * smoothstep(1.35, 0.75, eSh) * step(1.0, e);

    // ── micro-dots filling the wave valleys (her dense tiny nets) ───────
    vec2 g2 = w * dotScale * 1.7;
    float row2 = floor(g2.y);
    g2.x += 0.5 * mod(row2, 2.0);
    vec2 lp2 = fract(g2) - 0.5;
    float h3 = hash21(vec2(floor(g2.x), row2) * 3.7 + 11.0);
    float rr2 = 0.16 * (0.6 + 0.4 * h3) * (1.0 - wave) * sizeWave;
    rr2 *= 1.0 + amt * 0.7 * highP * step(0.75, h3);             // highs sparkle a subset
    float e2 = length(lp2) / max(rr2, 1e-4);
    float aa2 = aa * 3.4 / max(rr2 * 6.0, 1.0);
    col = mix(col, dotC, smoothstep(1.0 + aa2, 1.0 - aa2, e2) * step(0.02, rr2));

    // ── the main dot: flat ink → glossy 3D sphere inside the lens ───────
    float mask = smoothstep(1.0 + aa / max(rr, 0.05), 1.0 - aa / max(rr, 0.05), e);
    if (mask > 0.001) {
        vec3 flat_ = dotC * (0.94 + 0.08 * h2);                  // slight ink variety
        // sphere shading
        float zs = sqrt(max(1.0 - min(e * e, 1.0), 0.0));
        vec3 n = normalize(vec3(lp / max(rr, 1e-4), zs + 0.10));
        vec3 L = normalize(vec3(-0.42, 0.55, 0.72));
        float dif = 0.45 + 0.62 * max(dot(n, L), 0.0);
        float spe = pow(max(dot(n, normalize(L + vec3(0.0, 0.0, 1.0))), 0.0), 48.0);
        float fre = pow(1.0 - max(n.z, 0.0), 2.5);
        vec3 sphere = dotC * dif + vec3(1.0) * spe * 0.85 + bgC * fre * 0.25;
        col = mix(col, mix(flat_, sphere, f3d), mask);
    }

    // ── finish ──────────────────────────────────────────────────────────
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.016 * gr;
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
