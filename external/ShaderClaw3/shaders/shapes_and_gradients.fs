/*{
  "DESCRIPTION": "Shapes and Gradients — an airbrushed retro poster brought to life: a grainy sunset sky from slate blue down to hot yellow, two pink twelve-point starbursts with glowing orange cores over violet halos, a tall olive cone rising through the middle, and a bottom band of pinstripe panels, a lavender strip with yellow lights, a radiant orange blob, and wandering liquid-chrome squiggles. Beats flash the star cores and stretch the spikes, bass swells the orange blob and halos, mids ripple the chrome, highs sparkle the grain.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "starSize",     "LABEL": "Star Size",        "TYPE": "float", "MIN": 0.5, "MAX": 1.6, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "coneWidth",    "LABEL": "Cone Width",       "TYPE": "float", "MIN": 0.4, "MAX": 1.8, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "squiggleAmt",  "LABEL": "Chrome Squiggles", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "grainAmt",     "LABEL": "Grain",            "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6,  "GROUP": "Color" },
    { "NAME": "paletteShift", "LABEL": "Palette Shift",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",       "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",     "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Audio Reactivity" },
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
// SHAPES AND GRADIENTS — from a Y2K-airbrush poster reference:
//   grainy 4-stop sunset sky, two pink 12-point starbursts (violet-gray
//   halo + orange→white radial core, hashed irregular spike lengths),
//   a tall olive cone with cylindrical shading, and a bottom band:
//   blue-gray pinstripe panels left/right, a center panel with a lavender
//   strip (two glowing yellow ellipse "lights"), an orange gradient blob
//   panel, wobbling chrome loops on the stripes, and thin chrome strands
//   dripping from the lights out toward the loops.
//   Everything is composed opaque back-to-front like the print, then
//   dusted with animated film grain + fbm airbrush mottle.
//   Silence keeps an authored slow dream: spike lengths shimmer, cores
//   breathe, chrome wobbles, grain crawls. Audio is linear/additive:
//   beat flashes cores + stretches spikes, bass swells blob/halos, mids
//   grow the chrome wiggle, highs sparkle grain; whole-frame lift dips
//   below 1 so quiet passages visibly settle.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define PI 3.14159265
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
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
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

// hue rotation (YIQ-ish) for paletteShift
vec3 hueRotate(vec3 c, float a) {
    float cs = cos(a), sn = sin(a);
    mat3 m = mat3(
        0.299 + 0.701*cs + 0.168*sn, 0.587 - 0.587*cs + 0.330*sn, 0.114 - 0.114*cs - 0.497*sn,
        0.299 - 0.299*cs - 0.328*sn, 0.587 + 0.413*cs + 0.035*sn, 0.114 - 0.114*cs + 0.292*sn,
        0.299 - 0.300*cs + 1.250*sn, 0.587 - 0.588*cs - 1.050*sn, 0.114 + 0.886*cs - 0.203*sn);
    return clamp(c * m, 0.0, 2.0);
}

// liquid-chrome tube shading from a distance + width
void chrome(inout vec3 col, float d, float w) {
    float m    = smoothstep(w, w - 0.0035, d);
    float core = smoothstep(w * 0.85, w * 0.15, d);
    vec3 tube  = mix(vec3(0.55, 0.58, 0.72), vec3(0.97, 0.97, 1.0), core);
    col *= 1.0 - 0.22 * smoothstep(w + 0.02, w, d) * (1.0 - m);   // soft drop shadow
    col = mix(col, tube, m);
}

// 12-point starburst: spikes → violet halo → orange/white core
void drawStar(inout vec3 col, vec2 q, vec2 c, float s, float t, float seed,
              float beatP, float bassP, float amt) {
    vec2 d = q - c;
    float r = length(d);
    if (r > 1.6 * s) return;
    float ang = atan(d.y, d.x);
    float k = ang / TAU * 12.0;
    float sector = floor(k + 0.5);
    float fa = (k - sector) * TAU / 12.0;
    float h = hash11(sector * 3.71 + seed);
    float len = s * (0.72 + 0.58 * h)
              * (1.0 + 0.06 * sin(t * 0.7 + sector * 2.1 + seed)
                     + amt * 0.15 * beatP);
    float across = abs(fa) * r;
    float hw = s * 0.085 * clamp(1.0 - r / len, 0.0, 1.0) + 0.0015;
    float spike = smoothstep(hw, hw * 0.80, across)
                * smoothstep(len, len * 0.90, r)
                * smoothstep(0.02 * s, 0.10 * s, r);
    vec3 pink     = vec3(0.97, 0.52, 0.82);
    vec3 pinkLite = vec3(1.00, 0.79, 0.93);
    vec3 spikeCol = mix(pink, pinkLite, smoothstep(0.15, 0.95, r / len));
    // white spine highlight running up each spike
    float spine = smoothstep(hw * 0.40, 0.0, across) * smoothstep(len * 0.95, len * 0.25, r);
    spikeCol = mix(spikeCol, vec3(1.0, 0.93, 0.98), 0.50 * spine);
    col = mix(col, spikeCol, spike);
    // violet-gray halo over the spike bases
    float halo = exp(-(r * r) / (0.20 * s * 0.20 * s));
    col = mix(col, vec3(0.61, 0.59, 0.72), 0.85 * halo);
    // glowing core
    float coreGain = 1.0 + amt * (0.90 * beatP + 0.35 * bassP)
                   + 0.10 * sin(t * 1.3 + seed);
    col += vec3(1.00, 0.52, 0.12) * 0.85 * coreGain * exp(-(r * r) / (0.115 * s * 0.115 * s));
    col += vec3(1.00, 0.85, 0.32) * coreGain * exp(-(r * r) / (0.055 * s * 0.055 * s));
    col = mix(col, vec3(1.0, 0.97, 0.86),
              clamp(coreGain * exp(-(r * r) / (0.028 * s * 0.028 * s)), 0.0, 1.0));
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;      // x centered, y in [-0.5, 0.5]
    q.y += 0.5;                          // y in [0, 1], bottom → top
    float A = 0.5 * R.x / R.y;           // half-width in units

    float t   = TIME * motionSpeed * 0.5;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ── sky: 4-stop sunset gradient ─────────────────────────────────────
    float st = clamp((q.y - 0.302) / 0.698, 0.0, 1.0);
    vec3 sky = mix(vec3(1.00, 0.87, 0.24), vec3(1.00, 0.62, 0.13), smoothstep(0.00, 0.30, st));
    sky = mix(sky, vec3(0.78, 0.56, 0.50), smoothstep(0.32, 0.66, st));
    sky = mix(sky, vec3(0.40, 0.43, 0.51), smoothstep(0.60, 0.96, st));
    // slow drifting warmth in the low sky
    sky += vec3(0.05, 0.02, 0.0) * sin(t * 0.23) * (1.0 - st);
    vec3 col = sky;

    // ── starbursts (behind the cone, like the print) ────────────────────
    float s = 0.30 * starSize;
    float sx = max(0.295, A * 0.52);
    drawStar(col, q, vec2(-sx, 0.655), s, t, 1.7, beatP, bassP, amt);
    drawStar(col, q, vec2( sx, 0.655), s, t + 2.6, 9.3, beatP, bassP, amt);

    // ── olive cone ──────────────────────────────────────────────────────
    float apexY = 1.03, baseY = 0.302;
    float coneT = clamp((apexY - q.y) / (apexY - baseY), 0.0, 1.0);
    float chw = 0.158 * coneWidth * coneT;
    float coneMask = smoothstep(chw + 0.004, chw - 0.002, abs(q.x)) * step(q.y, apexY);
    if (coneMask > 0.001) {
        float u = q.x / max(chw, 1e-4);           // -1..1 across the cone
        float lgt = 0.38 + 0.62 * smoothstep(-0.15, 1.0, u);     // lit right flank
        lgt += 0.45 * exp(-pow((u + 0.85) / 0.14, 2.0));          // left rim light
        lgt *= mix(0.80, 1.05, coneT);                            // darker at the tip
        vec3 coneCol = mix(vec3(0.36, 0.37, 0.20), vec3(0.80, 0.80, 0.60), clamp(lgt, 0.0, 1.3) * 0.77);
        col = mix(col, coneCol, coneMask);
    }

    // ── bottom band ─────────────────────────────────────────────────────
    float pw = 0.235;                    // center-panel half width
    if (q.y < 0.302) {
        // pinstripe side panels
        vec3 band = vec3(0.92, 0.92, 0.94);
        float ln = smoothstep(0.32, 0.18, abs(fract(q.y * 74.0) - 0.5))
                 * smoothstep(0.010, 0.020, q.y) * smoothstep(0.295, 0.285, q.y);
        band = mix(band, vec3(0.44, 0.49, 0.62), ln * 0.85);
        col = band;

        if (abs(q.x) < pw) {
            if (q.y > 0.252) {
                // lavender strip with two yellow ellipse lights
                col = mix(vec3(0.63, 0.60, 0.80), vec3(0.77, 0.75, 0.88),
                          (q.y - 0.252) / 0.05);
                float liteGain = 1.0 + amt * (0.55 * highP + 0.45 * beatP)
                               + 0.06 * sin(t * 1.7);
                for (int i = 0; i < 2; i++) {
                    float m = (i == 0) ? -1.0 : 1.0;
                    vec2 ec = vec2(m * 0.105, 0.281);
                    float e = length((q - ec) / vec2(0.050, 0.0135));
                    col += vec3(1.0, 0.55, 0.10) * 0.8 * liteGain * exp(-e * e * 0.55);
                    col = mix(col, vec3(1.0, 0.83, 0.18) * liteGain,
                              smoothstep(1.0, 0.72, e));
                }
            } else {
                // orange gradient panel with radiant blob
                col = mix(vec3(1.00, 0.72, 0.18), vec3(1.00, 0.56, 0.07),
                          q.y / 0.252);
                float blobR = 1.0 + amt * 0.22 * bassP + 0.03 * sin(t * 0.9);
                vec2 bc = vec2(0.0, 0.112);
                float e = length((q - bc) / (vec2(0.195, 0.088) * blobR));
                col = mix(col, vec3(1.00, 0.86, 0.42), smoothstep(1.15, 0.25, e));
                col = mix(col, vec3(1.00, 0.94, 0.62), smoothstep(0.55, 0.05, e));
            }
            // soft inner shadow at the panel edges
            col *= 1.0 - 0.18 * smoothstep(0.030, 0.0, pw - abs(q.x));
        }

        // ── liquid-chrome squiggles ─────────────────────────────────────
        if (squiggleAmt > 0.001) {
            float wig = 1.0 + amt * 1.2 * midP;
            float lx = pw + 0.5 * (A - pw);      // loop centers on the stripes
            float dmin = 1e3;
            for (int i = 0; i < 2; i++) {
                float m = (i == 0) ? -1.0 : 1.0;
                // wobbling chrome loop
                vec2 lc = vec2(m * lx, 0.145);
                vec2 dl = q - lc;
                float rr = length(dl);
                float th = atan(dl.y, dl.x);
                float ringR = 0.072 * (1.0 + 0.30 * sin(3.0 * th + m * t * 0.6 + m * 2.0)
                                           + 0.16 * sin(5.0 * th - t * 0.4) * wig);
                dmin = min(dmin, abs(rr - ringR));
                // droplet resting inside each loop
                vec2 dc = lc + vec2(m * 0.012, -0.020 + 0.006 * sin(t * 0.8 + m));
                dmin = min(dmin, length((q - dc) * vec2(1.0, 1.35)) - 0.013);
                // thin strand dripping from the yellow light toward the loop
                float u = clamp((0.281 - q.y) / 0.145, 0.0, 1.0);
                float xc = mix(m * 0.105, m * lx, u * u)
                         + 0.030 * squiggleAmt * wig * sin(26.0 * q.y + t * 1.6 + m * 1.7) * u;
                float inY = smoothstep(0.281, 0.270, q.y) * smoothstep(0.120, 0.150, q.y);
                dmin = min(dmin, abs(q.x - xc) + (1.0 - inY) * 1.0);
            }
            chrome(col, dmin, 0.0085 * squiggleAmt);
        }
    }

    // ── grain + airbrush mottle ─────────────────────────────────────────
    float skyMask = smoothstep(0.29, 0.34, q.y);
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    float mottle = fbm(q * vec2(9.0, 26.0) + t * 0.02) - 0.5;
    col += grainAmt * (0.075 * gr * (1.0 + amt * 0.8 * highP)
                     + (0.05 + 0.07 * skyMask) * mottle);

    if (paletteShift > 0.001) col = hueRotate(col, paletteShift * TAU);

    // whole-frame lift that dips below 1 in quiet passages
    col *= brightness * mix(1.0, 0.74 + 0.46 * levelP + 0.20 * beatP, amt * 0.7);

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
