/*{
  "DESCRIPTION": "Holo Rock — the gallery specimen: a grey stone monolith floating on white, tumbling slowly while holographic paint strokes crawl across its faces — iridescent ribbons, candy dashes and chrome drips that shift hue as the rock turns. Real speckled granite shading, contact shadow on the paper. Bass makes the rock breathe, mids tumble it, beats splash a fresh stroke glow, highs flare the iridescence.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "rockSize",    "LABEL": "Rock Size",     "TYPE": "float", "MIN": 0.5, "MAX": 1.6,  "DEFAULT": 1.2,  "GROUP": "Shape / Geometry" },
    { "NAME": "strokeAmt",   "LABEL": "Holo Strokes",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.75, "GROUP": "Shape / Geometry" },
    { "NAME": "grit",        "LABEL": "Stone Grit",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "tumbleSpeed", "LABEL": "Tumble Speed",  "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.3,  "GROUP": "Motion / Animation" },
    { "NAME": "crawlSpeed",  "LABEL": "Stroke Crawl",  "TYPE": "float", "MIN": 0.0, "MAX": 2.0,  "DEFAULT": 0.4,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "hueShift",    "LABEL": "Holo Shift",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.05, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "hrScene" },
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// HOLO ROCK — after the holographic-paint rock reference.
//   Screen-space sculpted rock: silhouette = warped circle whose radius
//   is fbm-modulated per angle (re-hashed as the tumble clock advances,
//   crossfaded so the form morphs like slow rotation). Interior gets a
//   fake 3D normal from the silhouette SDF gradient + a coarse facet
//   noise, lit as speckled granite (two greys, salt-and-pepper grit,
//   crevice darkening). Holo strokes: worm dashes living in the rock's
//   surface space, each an iridescent ramp keyed by position + normal
//   (so hue slides as the rock tumbles), plus tiny candy confetti
//   dashes. White paper, soft contact shadow. 3-pass.
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

vec3 iri(float k) {
    return vec3(0.55 + 0.45 * cos(TAU * (k + 0.00)),
                0.55 + 0.45 * cos(TAU * (k + 0.30)),
                0.55 + 0.45 * cos(TAU * (k + 0.62)));
}

// rock radius at angle a for tumble frame f
float rockR(float a, float f) {
    return 0.30 * (1.0
        + 0.16 * (vnoise(vec2(a * 1.1, f * 3.1)) - 0.5) * 2.0
        + 0.07 * (vnoise(vec2(a * 2.7 + 9.0, f * 3.1 + 4.0)) - 0.5) * 2.0);
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - vec2(0.5, 0.54) * R) / (R.y * rockSize);

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float levP  = clamp(audioLevel, 0.0, 1.0);
    float t = TIME * motionSpeed;

    // paper
    vec3 col = vec3(0.965, 0.960, 0.950);
    col *= 1.0 - 0.05 * length(q);
    col += (vnoise(fc * 0.06) - 0.5) * 0.015;

    // ── tumbling silhouette: crossfade between hashed forms ─────────────
    float tum = t * tumbleSpeed * 0.25 + amt * 0.3 * midP;
    float a = atan(q.y, q.x) + tum * 0.6;             // slow visible rotation
    float f0 = floor(tum), ff = smoothstep(0.15, 0.85, fract(tum));
    float rr = mix(rockR(a, f0), rockR(a, f0 + 1.0), ff);
    rr *= 1.0 + amt * 0.05 * bassP;                    // breathe

    float r = length(q * vec2(1.0, 1.12));             // slightly tall stone
    float d = r - rr;
    float aa = 1.6 / (R.y * rockSize);

    // contact shadow under the rock
    vec2 shq = q - vec2(0.02, -0.34);
    float shd = length(shq * vec2(1.0, 2.6)) - rr * 0.85;
    col = mix(col, col * 0.78, smoothstep(0.16, 0.0, shd) * 0.7);

    float mask = smoothstep(aa, -aa, d);
    if (mask > 0.001) {
        // ── fake 3D normal: SDF gradient + facet bump ───────────────────
        float e = 0.012;
        float dx = (length((q + vec2(e, 0.0)) * vec2(1.0, 1.12)) - rr)
                 - (length((q - vec2(e, 0.0)) * vec2(1.0, 1.12)) - rr);
        float dy = (length((q + vec2(0.0, e)) * vec2(1.0, 1.12)) - rr)
                 - (length((q - vec2(0.0, e)) * vec2(1.0, 1.12)) - rr);
        float z = sqrt(max(1.0 - clamp(r / max(rr, 1e-3), 0.0, 1.0), 0.0));
        vec3 nrm = normalize(vec3(dx, dy, 0.9 * max(z, 0.15)));
        // coarse facets + fine bump
        vec2 sp = q * 4.0 + f0 * 3.0;
        nrm.xy += (vec2(fbm(sp) - 0.5, fbm(sp + 7.0) - 0.5)) * 1.1;
        nrm.xy += (vec2(vnoise(q * 30.0) - 0.5, vnoise(q * 30.0 + 3.0) - 0.5)) * 0.35 * grit;
        nrm = normalize(nrm);

        // ── granite shading ─────────────────────────────────────────────
        vec3 L = normalize(vec3(0.4, 0.6, 0.7));
        float dif = clamp(dot(nrm, L), 0.0, 1.0);
        float g1 = vnoise(q * 55.0 + f0);
        float g2 = vnoise(q * 140.0 + 9.0);
        vec3 stone = mix(vec3(0.22, 0.215, 0.21), vec3(0.55, 0.54, 0.53), fbm(q * 6.0 + f0));
        stone = mix(stone, vec3(0.10, 0.095, 0.09), step(0.80, g1) * grit * 0.9);   // pepper
        stone = mix(stone, vec3(0.70, 0.69, 0.68), step(0.90, g2) * grit * 0.7);    // salt
        stone *= 0.30 + 0.95 * dif;
        stone += vec3(1.0) * pow(clamp(dot(reflect(-L, nrm), vec3(0.0, 0.0, 1.0)), 0.0, 1.0), 18.0) * 0.14;
        // crevice darkening near strong facet slopes (deep cuts)
        stone *= 1.0 - 0.45 * clamp(length(nrm.xy) - 0.35, 0.0, 1.0);

        // ── holographic strokes in surface space ────────────────────────
        vec3 rock = stone;
        for (int i = 0; i < 9; i++) {
            float fi = float(i);
            float h1 = hash11(fi * 5.13 + 1.0);
            float h2 = hash11(fi * 8.77 + 4.0);
            if (h2 > strokeAmt + 0.25) continue;
            // stroke home on the rock, drifting slowly (crawl)
            vec2 sc = (vec2(h1, h2) - 0.5) * 0.52;
            sc += 0.05 * vec2(sin(t * crawlSpeed * 0.3 + h1 * TAU),
                              cos(t * crawlSpeed * 0.24 + h2 * TAU));
            float ra = h1 * TAU + tum * 0.4;
            vec2 rp = mat2(cos(ra), -sin(ra), sin(ra), cos(ra)) * (q - sc);
            // worm dash: sine spine, tapered width
            float Lh = 0.10 + 0.10 * h2;
            float xx = clamp(rp.x, -Lh, Lh);
            float yy = 0.03 * sin(xx * 22.0 + h1 * 9.0 + t * crawlSpeed * 0.8);
            float sd = length(rp - vec2(xx, yy));
            float w = 0.016 * (0.6 + 0.4 * h1) * (1.0 - 0.5 * abs(xx) / Lh);
            float sm = smoothstep(w + aa, w - aa, sd) * mask;
            if (sm > 0.001) {
                // iridescent ramp keyed by normal + position → hue slides
                float key = hueShift + h1 + nrm.x * 0.6 + nrm.y * 0.3 + xx * 2.0
                          + amt * 0.15 * highP;
                vec3 holo = iri(key);
                holo += vec3(1.0) * pow(clamp(dot(nrm, L), 0.0, 1.0), 8.0) * 0.4;
                holo *= 1.0 + amt * 0.5 * beatP * step(0.6, h1);
                rock = mix(rock, holo, sm * 0.95);
            }
        }
        rock *= 1.0 + amt * 0.12 * levP;
        col = mix(col, rock, mask);
    }

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.012;
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        gl_FragColor = vec4(clamp(renderScene(), 0.0, 1.0), 1.0);
    } else if (PASSINDEX == 1) {
        vec3 col = texture2D(hrScene, uv).rgb;
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
