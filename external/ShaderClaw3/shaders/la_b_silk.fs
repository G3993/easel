/*{
  "DESCRIPTION": "La B Silk — liquid silk with real dimension: a deeply folded fabric height-field poured in slow continuous motion, lit by a dramatic directional key so fold valleys sink into bordeaux shadow while satin ridges catch champagne highlights, with midnight-blue accents pooling in select folds and a fine woven-thread micro-texture glinting in the light. Bass deepens the folds, mids push the pour, beats sweep a soft gleam across the ridges, highs shimmer the threads.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "foldScale",    "LABEL": "Fold Scale",    "TYPE": "float", "MIN": 0.5, "MAX": 2.5, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "foldDepth",    "LABEL": "Fold Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "sheenAmt",     "LABEL": "Satin Sheen",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.75, "GROUP": "Shape / Geometry" },
    { "NAME": "threadAmt",    "LABEL": "Thread Weave",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Shape / Geometry" },
    { "NAME": "accentAmt",    "LABEL": "Midnight Accent","TYPE": "float","MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.45, "GROUP": "Color" },
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
// LA B SILK — liquid silk with dimension, for La Bloom.
//   The rejected version was a flat washed drape; this one is SCULPTED.
//   A domain-warped fbm + ridged-fold height field is read as fabric
//   relief and lit with finite-difference normals under a hard key
//   light: diffuse floor 0.16 so valleys really sink, a cavity term from
//   the local laplacian carves the fold roots darker still, and two
//   specular lobes (a broad pow-13 satin band + a tight pow-44 hot line)
//   ride the ridge crests in champagne. Color is mapped by relief:
//   bordeaux deep in the folds rising through rose-champagne to ivory on
//   the crests, with midnight-blue pooling in the valleys of selected
//   regions. A fine two-axis thread weave perturbs the normals so the
//   highlight breaks into woven glints. The whole field pours downward
//   continuously — slow, layered drift, never a loop. Audio: bass
//   deepens fold amplitude, mids push the pour phase additively, beats
//   gleam the speculars (envelope-gated), highs shimmer the weave.
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
float fbm4(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.09 + vec2(3.7, 7.1);
        a *= 0.52;
    }
    return v * 1.08;
}
vec3 hueShift(vec3 c, float a) {
    vec3 k = vec3(0.57735);
    float cs = cos(a), sn = sin(a);
    return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
}

// silk relief: domain-warped rolling folds + ridged creases
float hgt(vec2 sp, float tw) {
    vec2 w = sp;
    w += 1.15 * vec2(fbm4(sp * 0.55 + vec2(0.0, tw * 0.12)) - 0.5,
                     fbm4(sp * 0.55 + vec2(5.1, -tw * 0.09)) - 0.5) * 2.0;
    float h = fbm4(w);
    float rdg = 1.0 - abs(2.0 * fbm4(w * 1.9 + vec2(2.2, 8.8)) - 1.0);
    return h * 0.70 + rdg * rdg * 0.30;
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * motionSpeed * 0.17;
    float amt = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);

    float tw = t + amt * 0.5 * midP;                 // pour phase, additive
    float gleam = amt * beatP;                       // beat: ridge gleam

    // ── the pour: field coordinates flow downward continuously ─────────
    vec2 fp = q * (1.55 * foldScale);
    fp.y += tw * 0.34 + amt * 0.10 * midP;           // pour + mid phase push
    fp.x += 0.10 * sin(tw * 0.21);                   // slow lateral sway

    // ── relief + finite-difference normals ─────────────────────────────
    float eps = 0.045;
    float h0 = hgt(fp, tw);
    float hx = hgt(fp + vec2(eps, 0.0), tw);
    float hy = hgt(fp + vec2(0.0, eps), tw);
    float ampl = (0.55 + 1.85 * foldDepth) * (1.0 + amt * 0.28 * bassP);
    vec3 n = normalize(vec3(-(hx - h0) / eps * ampl * 0.42,
                            -(hy - h0) / eps * ampl * 0.42, 1.0));

    // cavity term: fold roots (negative laplacian) darken further
    float cav = clamp((h0 - 0.5 * (hx + hy)) * 9.0, -1.0, 1.0);

    // ── fine woven thread micro-texture perturbing the normal ──────────
    vec2 tvp = fp + vec2(hx - h0, hy - h0) * 6.0;    // threads follow the folds
    float thrU = sin(tvp.x * 210.0 + tvp.y * 14.0);
    float thrV = sin(tvp.y * 210.0 - tvp.x * 14.0 + 1.3);
    float weave = thrU * 0.6 + thrV * 0.4;
    n = normalize(n + vec3(thrU, thrV, 0.0) * 0.045 * threadAmt);

    // ── dramatic directional key + fill ────────────────────────────────
    vec3 L = normalize(vec3(-0.56, 0.60, 0.50));
    vec3 V = vec3(0.0, 0.0, 1.0);
    float ndl = max(dot(n, L), 0.0);
    float dif = 0.16 + 0.92 * ndl;                   // deep shadow valleys
    dif *= 1.0 - 0.30 * clamp(-cav, 0.0, 1.0);       // carve the fold roots
    // bass breathes the key light — smooth kneed envelope, no time-warp
    dif *= 1.0 + amt * 0.30 * bassP;
    vec3 Hv = normalize(L + V);
    float ndh = max(dot(n, Hv), 0.0);
    float satin = pow(ndh, 13.0);                    // broad satin band
    float hot   = pow(ndh, 44.0);                    // tight ridge line
    float sheen = pow(1.0 - max(dot(n, V), 0.0), 3.0);

    // ── jewel palette mapped by relief ─────────────────────────────────
    vec3 bordeaux  = vec3(0.335, 0.045, 0.115);
    vec3 roseGold  = vec3(0.72, 0.38, 0.30);
    vec3 ivory     = vec3(0.935, 0.875, 0.775);
    vec3 champagne = vec3(1.0, 0.83, 0.55);
    vec3 midnight  = vec3(0.075, 0.115, 0.30);

    float rel = clamp(h0 * 0.7 + ndl * 0.45, 0.0, 1.0);
    vec3 base = mix(bordeaux, roseGold, smoothstep(0.12, 0.55, rel));
    base = mix(base, ivory, smoothstep(0.50, 0.92, rel));
    // midnight-blue pooling in valleys of selected regions
    float accMask = smoothstep(0.52, 0.80, fbm4(fp * 0.42 + vec2(9.7, 3.1)))
                  * smoothstep(0.62, 0.20, rel) * accentAmt;
    base = mix(base, midnight, accMask);

    vec3 col = base * dif;
    // speculars in champagne — bass swells them, beats sweep a soft gleam
    // (localized highlights, not a whole-frame lift, so no chop)
    float shn = sheenAmt * (1.0 + amt * (0.70 * bassP + 0.35 * highP) + 0.70 * gleam);
    col += champagne * satin * 0.42 * shn;
    col += vec3(1.0, 0.93, 0.80) * hot * 0.85 * shn;
    col += champagne * sheen * 0.16 * shn;
    // beat: a soft light sweep travels diagonally across the silk —
    // spatially moving + envelope-gated, so it reads as light, not a strobe
    float swPos = q.x * 0.8 + q.y * 0.6 - (fract(t * 0.55) * 2.6 - 1.3);
    float sweep = exp(-swPos * swPos * 7.0);
    col += champagne * sweep * (0.25 + 0.75 * ndl) * (0.40 * gleam + 0.10 * amt * bassP) * sheenAmt;
    // woven glints where the highlight crosses the weave (highs shimmer)
    float glint = max(weave, 0.0) * satin;
    col += vec3(1.0, 0.90, 0.72) * glint * threadAmt * (0.20 + 0.45 * amt * highP);
    // thread shadow texture in the diffuse
    col *= 1.0 - 0.05 * threadAmt * (0.5 - 0.5 * weave) * dif;

    // ── depth cue: soft ambient falloff into the frame corners ─────────
    col *= 1.0 - 0.36 * smoothstep(0.48, 1.15, length(q * vec2(0.85, 1.0)));
    col = hueShift(col, paletteShift * TAU);
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.013 * gr;
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
