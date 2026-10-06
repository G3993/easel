/*{
  "DESCRIPTION": "Arch Fade Gradient — nested rainbow arches born at the base and blooming outward, each band filled with smooth dipole gradient orbs drifting across the tunnel like aura light through paper layers. Bass swells bands, mids drift hues, beats birth bright arches, highs shimmer grain. Gradient field replaces flat band fills for a Turrell-meets-paper-tunnel look.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "bandCount",      "LABEL": "Band Density",    "TYPE": "float", "MIN": 4.0,  "MAX": 14.0, "DEFAULT": 8.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "archSize",       "LABEL": "Arch Size",       "TYPE": "float", "MIN": 0.5,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "shadowAmt",      "LABEL": "Paper Shadow",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Shape / Geometry" },
    { "NAME": "growSpeed",      "LABEL": "Bloom Speed",     "TYPE": "float", "MIN": -2.0, "MAX": 2.0,  "DEFAULT": 0.4,  "GROUP": "Motion / Animation" },
    { "NAME": "hueSpeed",       "LABEL": "Hue Drift",       "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed",    "LABEL": "Motion Speed",    "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "driftSpeed",     "LABEL": "Orb Drift",       "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.7,  "GROUP": "Motion / Animation" },
    { "NAME": "hueShift",       "LABEL": "Hue Shift",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "colorBoost",     "LABEL": "Color Boost",     "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.1,  "GROUP": "Color" },
    { "NAME": "grainAmt",       "LABEL": "Film Grain",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Color" },
    { "NAME": "brightness",     "LABEL": "Brightness",      "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "gradientSoft",   "LABEL": "Gradient Soft",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Color" },
    { "NAME": "audioReact",     "LABEL": "Audio React",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",       "LABEL": "Motion Trails",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.06, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",       "LABEL": "Bloom Depth",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Depth / Passes" },
    { "NAME": "aberration",     "LABEL": "Lens Depth",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

#define R RENDERSIZE.xy
#define TAU 6.2831853
#define PI  3.1415927

// ── Utilities ──────────────────────────────────────────────────────────────
float hash11(float n) { return fract(sin(n) * 43758.5453123); }
float hash21(vec2 p)  {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0/3.0, 2.0/3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0*d + e)), d / (q.x + e), q.x);
}
vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

// Rich pastel hue from angle — warm/cool sweep
vec3 pastelHue(float h) {
    return vec3(0.66 + 0.33 * cos(TAU * (h + 0.00)),
                0.64 + 0.33 * cos(TAU * (h + 0.33)),
                0.66 + 0.33 * cos(TAU * (h + 0.66)));
}

// Orb palette pairs (same as flagship, adapted for arch context)
vec3 orbColA(int i) {
    if (i == 0) return vec3(1.00, 0.60, 0.40);
    if (i == 1) return vec3(0.96, 0.42, 0.56);
    if (i == 2) return vec3(0.72, 0.62, 0.98);
    if (i == 3) return vec3(0.40, 0.82, 0.86);
    if (i == 4) return vec3(0.99, 0.88, 0.58);
    return vec3(0.98, 0.62, 0.52);
}
vec3 orbColB(int i) {
    if (i == 0) return vec3(0.92, 0.36, 0.58);
    if (i == 1) return vec3(0.58, 0.40, 0.88);
    if (i == 2) return vec3(0.42, 0.66, 0.98);
    if (i == 3) return vec3(0.52, 0.92, 0.70);
    if (i == 4) return vec3(0.98, 0.96, 0.88);
    return vec3(0.70, 0.62, 0.94);
}

// ── Gradient field underlying the arches (dipole orb wash) ────────────────
// Six slow orbs drifting across a dark-to-pastel field — their light bleeds
// through every arch band as a smooth gradient, replacing flat band fills.
vec3 gradientField(vec2 fc, float t) {
    // normalised screen coords, aspect-corrected
    vec2 uv = fc / R;
    vec2 p  = uv - 0.5;
    p.x *= R.x / max(R.y, 1.0);

    float bass = clamp(audioBass, 0.0, 1.0);
    float mid  = clamp(audioMid,  0.0, 1.0);
    float high = clamp(audioHigh, 0.0, 1.0);
    float aR   = audioReact;
    float kick = clamp(audioBeatPulse, 0.0, 1.0);

    // Deep dusk base — the arches sit on a rich dark field
    float gradY = clamp(uv.y * 0.9 + 0.1, 0.0, 1.0);
    vec3 field = mix(vec3(0.10, 0.07, 0.14), vec3(0.38, 0.28, 0.42), gradY);

    // Six drifting dipole aura orbs
    for (int i = 0; i < 6; i++) {
        float fi = float(i);
        float h  = hash11(fi * 7.31 + 1.7);

        float bnd;
        if (i < 2)      { bnd = bass; }
        else if (i < 4) { bnd = mid;  }
        else            { bnd = high; }

        float tt = t * (0.08 + 0.05 * h) * max(driftSpeed, 0.0) + fi * 2.399;

        // Anchor positions around the arch tunnel center
        vec2 anchor = vec2((hash11(fi * 3.13 + 0.7) - 0.5) * 0.80,
                           (hash11(fi * 5.71 + 2.3) - 0.5) * 0.60);
        vec2 c = anchor + vec2(sin(tt + h * TAU), cos(tt * 0.77 + h * 3.0))
                        * vec2(0.22, 0.16);

        // Radius: larger for bass orbs, breathes with audio
        float r0 = 0.55 / (1.0 + fi * 0.28);
        float r  = r0 * (1.0 + 0.08 * sin(t * (0.3 + 0.2*h) + fi * 1.7)
                             + 0.30 * bnd * aR);

        float d    = length(p - c) / max(r, 1e-3);
        float core = exp(-d * d * 2.2);
        float halo = exp(-d * 1.6);

        // Dipole gradient across each orb
        vec2 axis = vec2(cos(fi * 2.1 + t * 0.04), sin(fi * 2.1 + t * 0.04));
        float side = clamp(0.5 + 0.5 * dot((p - c) / max(r, 1e-3), axis), 0.0, 1.0);
        vec3 orbCol = mix(orbColA(i), orbColB(i), side);

        field = mix(field, orbCol, clamp(core * 0.90, 0.0, 1.0));
        field += orbCol * halo * 0.06 * (1.0 + 0.9 * kick * aR);
    }

    return field;
}

// ── Main scene: arch bands carved from gradient field ─────────────────────
vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q  = (fc - vec2(0.5, 0.16) * R) / (R.y * archSize);

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float t     = TIME * motionSpeed;

    // Slight squash: arches are a touch wider than tall
    float r = length(q * vec2(0.92, 1.0));
    float a = atan(q.x, q.y);

    // ── Log-radius band clock ─────────────────────────────────────────────
    float grow = t * growSpeed * 0.30 + amt * 0.35 * midP;
    float nb   = bandCount;
    float lr   = log(max(r, 1e-4)) * nb * 0.55 - grow * nb * 0.25;
    float k    = floor(lr);
    float f    = fract(lr);   // 0 = inner lip → 1 = outer lip

    float hk  = hash11(k * 13.7);
    float hk2 = hash11(k * 7.1 + 3.0);

    // ── Sample gradient field at THIS pixel ───────────────────────────────
    vec3 gField = gradientField(fc, TIME);

    // ── Arch-angle gradient: blend the gradient field with per-band hues ──
    float hueWalk = t * hueSpeed * 0.03 + hueShift;
    float h0      = hk + hueWalk;
    float u       = 1.0 - abs(a) / PI;          // 1 at crown, 0 at feet
    float sideMix = 0.5 + 0.5 * sin(a) * (1.0 - u);

    vec3 cFoot  = mix(pastelHue(h0), pastelHue(h0 + 0.45), sideMix);
    vec3 cCrown = pastelHue(h0 + 0.24 + 0.1 * hk2);
    vec3 archHue = mix(cFoot, cCrown, smoothstep(0.12, 0.85, u));

    // Core blend: gradient field is the majority color; arch hue tints it
    // gradientSoft controls how much the smooth orb field shows vs arch hue
    float gs   = clamp(gradientSoft, 0.0, 1.0);
    vec3 band  = mix(archHue, gField, 0.40 + 0.55 * gs);

    // Radial airbrush: inner lip is lighter (reveals gradient highlight)
    float innerLift = smoothstep(1.0, 0.0, f);
    band = mix(band, band * 1.28 + vec3(0.06), innerLift * 0.55);

    // Outer edge dims subtly, letting shadow read cleanly
    band *= 0.84 + 0.18 * innerLift;

    // Bass breathing wave (gentle, per-band)
    band *= 1.0 + amt * 0.07 * bassP * sin(k * 1.3 - t * 0.8);

    // Beat: warm inner bloom on newest band
    float innerFlash = exp(-max(lr, 0.0) * 1.2) * amt * beatP;
    band += vec3(1.0, 0.92, 0.80) * innerFlash * 0.12;

    // ── Paper-stack shadow + lip ──────────────────────────────────────────
    float px    = nb * 0.55 * R.y * archSize;
    float edgeAA = 2.2 / max(px * max(r, 0.05), 1.0);
    float shadow = exp(-f * 5.5) * shadowAmt * 0.32;
    band *= 1.0 - shadow;
    float lip   = exp(-f * 28.0) * 0.20;
    band += vec3(1.0) * lip * 0.55;

    // ── Fade outer / seed inner ───────────────────────────────────────────
    vec3 page = mix(vec3(0.88, 0.84, 0.90), gField, 0.5);  // page picks up field color
    vec3 col  = band;
    col = mix(col, page, smoothstep(1.30, 1.80, r));
    col = mix(vec3(0.08, 0.05, 0.10), col, smoothstep(0.0, 0.06, r));

    col *= 1.0 + amt * 0.12 * clamp(audioLevel, 0.0, 1.0);

    // ── Universal hue shift ───────────────────────────────────────────────
    if (hueShift > 0.001) {
        vec3 hsv = rgb2hsv(clamp(col, 0.0, 1.5));
        hsv.x = fract(hsv.x + hueShift);
        col = hsv2rgb(hsv);
    }

    // Color boost
    float luma = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(luma), col, clamp(colorBoost, 0.0, 2.0));

    // ── Film grain (shadow-weighted, fine tooth) ──────────────────────────
    float l2       = dot(col, vec3(0.299, 0.587, 0.114));
    float shadowW  = 1.0 - smoothstep(0.0, 0.7, l2);
    float g1 = hash21(fc * 0.9 + fract(TIME * 0.7) * vec2(31.0, 17.0)) - 0.5;
    float g2 = hash21(fc * 0.5 + 7.0) - 0.5;
    col += (g1 * 0.022 + g2 * 0.013) * grainAmt
         * mix(0.70, 1.0, shadowW)
         * (1.0 + amt * 0.22 * highP);

    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;

    if (PASSINDEX == 0) {
        vec3 col  = renderScene();
        vec3 prev = texture2D(abTrail, uv).rgb;
        float decay = 0.50 + 0.46 * trailAmt;
        col = max(col, prev * decay - 0.004);
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.005;
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);

    } else {
        vec2 dir = uv - 0.5;
        float ab = aberration * 0.005;
        vec3 base;
        base.r = texture2D(abTrail, uv + dir * ab).r;
        base.g = texture2D(abTrail, uv).g;
        base.b = texture2D(abTrail, uv - dir * ab).b;

        // Bloom pass
        vec3 bl = vec3(0.0);
        for (int i = 0; i < 8; i++) {
            float an = float(i) * 0.7853982;
            vec2 o = vec2(cos(an), sin(an)) * (3.5 / R.y);
            bl += texture2D(abTrail, uv + o).rgb;
            bl += texture2D(abTrail, uv + o * 2.6).rgb * 0.6;
        }
        bl /= 12.8;
        bl = max(bl - 0.50, 0.0);

        vec3 col = base + bl * bl * bloomAmt * 1.9;

        float lvl = clamp(audioLevel, 0.0, 1.0);
        col *= brightness
             * mix(1.0, 0.64 + 0.36 * lvl, audioReact)
             * (1.0 + audioReact * 0.06 * clamp(audioBeatPulse, 0.0, 1.0));
        col = clamp(col, 0.0, 1.0);

        // Subtle S-curve
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.50);

        // Saturation bump
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.20), 0.0, 1.0);

        gl_FragColor = vec4(col, 1.0);
    }
}