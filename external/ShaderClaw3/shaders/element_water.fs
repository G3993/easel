/*{
  "DESCRIPTION": "Element Water — real living water: slow heavy swells of superposed wandering waves, fluid caustic light webs with fine detail octaves, streaky foam lacing along the crests with drifting foam speckle, lazy vortex swirls turning the whole sea around drifting centers, and god rays slanting down through deep teal into abyssal navy. Bass swells the deep current, mids ripple the caustics, beats fizz the foam, highs glint the crests — everything slow and fluid.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "causticAmt",   "LABEL": "Caustics",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "rayAmt",       "LABEL": "Light Shafts",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Shape / Geometry" },
    { "NAME": "foamAmt",      "LABEL": "Foam",          "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "swirlAmt",     "LABEL": "Vortex Swirl",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Shape / Geometry" },
    { "NAME": "depthHue",     "LABEL": "Water Hue",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.45, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// ELEMENT WATER — complete redesign as REAL living water (Lu's notes:
//   fluidity, foam, foam particles, vortex randomness, moving waves;
//   much slower). Base clock HALVED (0.30 vs old 0.60). The domain first
//   passes through 3 drifting VORTEX centers (gaussian-falloff rotational
//   warp, strength slowly breathing) so the whole sea turns lazily and
//   nothing repeats. On top of that, a domain-warped fbm pre-warp makes
//   every wave crest wander. The height field = 3 superposed directional
//   gerstner-style sine trains + broadband fbm swell; crests are lit,
//   troughs fall to abyssal navy through a vertical depth gradient.
//   Caustic webs: 3 octaves of min-of-warped-sines (coarse web + midweb
//   + fine detail octave). FOAM: streaky advected anisotropic noise laced
//   along crests, plus round advected foam-speckle particles (cell dots —
//   no square pixels anywhere: every particle is a distance-field disc).
//   God-ray light shafts slant from the surface. Audio: bass swells the
//   current (additive phase), mids push caustic phase (additive — never
//   time×envelope), beats fizz foam speckle, highs glint crests.
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
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.07 + vec2(7.3, 3.1);
        a *= 0.52;
    }
    return v;
}
vec3 hueRotate(vec3 c, float a) {
    float cs = cos(a), sn = sin(a);
    mat3 m = mat3(
        0.299 + 0.701*cs + 0.168*sn, 0.587 - 0.587*cs + 0.330*sn, 0.114 - 0.114*cs - 0.497*sn,
        0.299 - 0.299*cs - 0.328*sn, 0.587 + 0.413*cs + 0.035*sn, 0.114 - 0.114*cs + 0.292*sn,
        0.299 - 0.300*cs + 1.250*sn, 0.587 - 0.588*cs - 1.050*sn, 0.114 + 0.886*cs - 0.203*sn);
    return clamp(c * m, 0.0, 2.0);
}

// caustic web: layered warped sines, min-combined and sharpened
float caustic(vec2 p, float t) {
    vec2 w1 = p + 0.35 * vec2(vnoise(p * 1.5 + t * 0.3), vnoise(p * 1.5 + 4.0 - t * 0.25));
    vec2 w2 = p * 1.3 + 0.30 * vec2(vnoise(p * 2.1 - t * 0.2 + 9.0), vnoise(p * 2.1 + t * 0.35 + 2.0));
    float a = 0.5 + 0.5 * sin(w1.x * 6.0 + t) * sin(w1.y * 6.0 - t * 0.8);
    float b = 0.5 + 0.5 * sin(w2.x * 7.0 - t * 1.1) * sin(w2.y * 7.0 + t * 0.9);
    float c = min(a, b);
    return pow(1.0 - c, 11.0) * 1.55;
}

// round advected foam speckle — distance-field discs in a drifting grid
float speckle(vec2 p, float t, float scale) {
    vec2 g  = p * scale;
    vec2 id = floor(g);
    vec2 fr = fract(g) - 0.5;
    vec2 off = (vec2(hash21(id), hash21(id + 19.7)) - 0.5) * 0.72;
    float pd = length(fr - off);
    float gate = step(0.88, hash21(id + 5.3));
    float tw = 0.55 + 0.45 * sin(t * 2.4 + hash21(id + 2.1) * TAU);
    return smoothstep(0.30, 0.06, pd) * gate * tw;
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * motionSpeed * 0.30;                       // HALVED — slow, heavy water
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ── vortex swirls: 3 drifting centers turn the whole sea lazily ─────
    vec2 p = q + vec2(t * 0.02 + amt * 0.10 * bassP, 0.0);       // slow current, bass push (additive)
    for (int i = 0; i < 3; i++) {
        float fi = float(i);
        vec2 c = vec2(sin(t * 0.11 + fi * 2.4) * 0.45,
                      cos(t * 0.083 + fi * 1.7) * 0.30);
        vec2 d = p - c;
        float rr = dot(d, d);
        float sw = swirlAmt * 1.7 * exp(-rr * 6.5)
                 * (0.55 + 0.45 * sin(t * 0.19 + fi * 2.9));     // strength breathes slowly
        float cs = cos(sw), sn = sin(sw);
        p = c + vec2(cs * d.x - sn * d.y, sn * d.x + cs * d.y);
    }

    // ── domain-warped wave field: crests wander, never repeat ───────────
    vec2 wrp = 0.24 * vec2(fbm(p * 1.7 + vec2(t * 0.16, 2.0)),
                           fbm(p * 1.7 + vec2(4.9, -t * 0.13)));
    vec2 wv = p + wrp;
    float swell = amt * 0.6 * bassP;                             // additive swell phase
    float h = 0.0;
    h += 0.42 * sin(dot(wv, vec2(1.0,  0.35)) * 6.2  + t * 1.05 + swell);
    h += 0.30 * sin(dot(wv, vec2(-0.62, 1.0)) * 8.6  - t * 0.80 + 2.1 + swell * 0.6);
    h += 0.20 * sin(dot(wv, vec2(0.88, -0.80)) * 12.5 + t * 1.35 + 4.0);
    h += 0.50 * (fbm(wv * 2.9 + vec2(t * 0.22, -t * 0.16)) - 0.5);
    // smooth envelope-scaled swell amplitude (never time×audio — chop-safe)
    h *= 1.0 + amt * (0.45 * bassP + 0.35 * levelP);
    float hgt = clamp(h * 0.42 + 0.5, 0.0, 1.0);                 // 0 trough → 1 crest

    // ── depth-shaded water body: crest light over abyssal navy ──────────
    float depth = smoothstep(0.55, -0.55, q.y);                  // 0 top → 1 bottom
    vec3 shallow = vec3(0.09, 0.52, 0.58);
    vec3 midw    = vec3(0.025, 0.26, 0.44);
    vec3 abyss   = vec3(0.004, 0.032, 0.115);
    vec3 col = mix(midw, abyss, smoothstep(0.15, 1.0, depth));
    col = mix(col, shallow, hgt * hgt * mix(0.95, 0.35, depth)); // wave height lights the water
    col += vec3(0.03, 0.15, 0.19) * smoothstep(0.35, 0.9, hgt);  // crest sheen

    // ── caustic webs — coarse + mid + fine detail octave ────────────────
    float cph = t * 0.75 + amt * 2.2 * midP;                     // mids push phase (additive)
    float ca_ = caustic(wv * 2.0, cph);
    float cb_ = caustic(wv * 3.9 + 7.0, cph * 1.35 + 3.0) * 0.55;
    float cc_ = caustic(wv * 7.4 + 3.0, cph * 1.7  + 6.0) * 0.28;
    float cw = (ca_ + cb_ + cc_) * causticAmt * mix(1.15, 0.30, depth) * (0.55 + 0.55 * hgt);
    col += vec3(0.50, 0.88, 0.92) * cw * (1.0 + amt * (0.6 * highP + 0.55 * levelP));

    // ── god-ray light shafts from the surface ───────────────────────────
    vec2 sun = vec2(0.35, 0.62);
    vec2 sd = q - sun;
    float ang = atan(sd.x, -sd.y);
    float rays = 0.0;
    rays += pow(0.5 + 0.5 * sin(ang * 14.0 + t * 0.30), 3.0);
    rays += pow(0.5 + 0.5 * sin(ang * 23.0 - t * 0.19 + 2.0), 4.0) * 0.6;
    rays *= smoothstep(1.5, 0.1, length(sd)) * smoothstep(-0.2, 0.6, q.y + 0.4);
    float throb = 1.0 + amt * (0.4 * bassP + 0.25 * levelP);
    col += vec3(0.42, 0.78, 0.84) * rays * rayAmt * 0.20 * throb;
    col += vec3(0.80, 0.96, 0.94) * exp(-length(sd) * 3.2) * 0.45 * rayAmt;

    // ── FOAM: streaky lace along crests + round speckle particles ───────
    float crest = smoothstep(0.58, 0.92, hgt);
    // anisotropic streak noise advected along the dominant wave direction
    vec2 fdir = normalize(vec2(1.0, 0.35));
    vec2 fuv  = vec2(dot(wv, fdir), dot(wv, vec2(-fdir.y, fdir.x)));
    float streak = fbm(vec2(fuv.x * 3.2 - t * 0.85, fuv.y * 15.0));
    float lace   = fbm(wv * 9.5 + vec2(t * 0.5, -t * 0.35));
    float foam = crest * smoothstep(0.42, 0.85, streak * 0.62 + lace * 0.62);
    foam = pow(clamp(foam, 0.0, 1.0), 1.15) * foamAmt * (1.0 + amt * 0.4 * levelP);
    col = mix(col, vec3(0.82, 0.96, 0.97), clamp(foam * 1.25, 0.0, 0.85));
    // foam speckle: fine round bubbles drifting with the water, fizzing on beats
    float fizz = 1.0 + amt * 1.1 * beatP;
    float spA = speckle(wv + vec2(t * 0.34, -t * 0.20), t, 48.0);
    float spB = speckle(wv * 1.6 + vec2(-t * 0.22, t * 0.28) + 31.0, t + 9.0, 62.0) * 0.7;
    col += vec3(0.78, 0.95, 0.97) * (spA + spB) * foamAmt
         * (0.22 + 0.78 * crest) * fizz * 0.75;
    // crisp white glints riding the crests — tight specular sparkle
    float spC = speckle(wv * 2.2 + vec2(t * 0.5, t * 0.3) + 57.0, t + 3.0, 88.0);
    col += vec3(0.95, 1.0, 1.0) * spC * crest * foamAmt * (0.5 + amt * 0.7 * highP);

    // ── suspended round motes sinking through the shafts ────────────────
    float mote = speckle(wv * 0.5 + vec2(t * 0.05, t * 0.10), t * 0.7, 26.0);
    col += vec3(0.26, 0.50, 0.56) * mote * 0.35 * (1.0 - depth * 0.7) * rayAmt;

    // ── finish ──────────────────────────────────────────────────────────
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.02 * gr;
    if (depthHue > 0.001) col = hueRotate(col, depthHue * TAU);
    col *= brightness * mix(1.0, 0.80 + 0.32 * levelP + 0.26 * clamp(audioBass, 0.0, 1.0)
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
