/*{
  "DESCRIPTION": "Max Orb — the psychedelic-poster cosmos: a giant golden orb inside a purple ring, split by a blue cloud band woven with red-and-orange checkerwork, teal thought-clouds at the shoulders, all floating on a shimmering halftone-dot wall that fades green to plum. The checker band waves, clouds drift, the orb breathes, dots shimmer in print-registration drift. Bass breathes the orb, mids wave the band, beats blink checker cells, highs shimmer the dot wall.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "orbSize",     "LABEL": "Orb Size",      "TYPE": "float", "MIN": 0.6, "MAX": 1.3,  "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "bandAmt",     "LABEL": "Cloud Band",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.8,  "GROUP": "Shape / Geometry" },
    { "NAME": "dotScale",    "LABEL": "Halftone Dots", "TYPE": "float", "MIN": 30.0, "MAX": 110.0,"DEFAULT": 64.0, "GROUP": "Shape / Geometry" },
    { "NAME": "waveSpeed",   "LABEL": "Band Wave",     "TYPE": "float", "MIN": 0.0, "MAX": 2.0,  "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "driftSpeed",  "LABEL": "Cloud Drift",   "TYPE": "float", "MIN": 0.0, "MAX": 2.0,  "DEFAULT": 0.4,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "hueShift",    "LABEL": "Poster Shift",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.05, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// MAX ORB — after the Peter Max "Infinity" poster reference.
//   Layers back to front: (1) the dot wall — a halftone grid whose dot
//   radius follows a green→plum diagonal ramp, with slow registration
//   drift and a high-frequency shimmer; (2) the purple ring and the
//   big golden orb, its orange airbrush core breathing; (3) the blue
//   cloud band across the orb's waist: scalloped top and bottom edges
//   (three bumps like the poster's cloud), filled with a red/orange
//   checker inside blue webbing that WAVES like a flag; (4) teal
//   thought-clouds at the shoulders with red teardrop eyes. Everything
//   drawn poster-flat with tight AA, animated gently — a living print.
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

vec3 hueRotate(vec3 c, float a) {
    float an = a * TAU;
    vec3 k = vec3(0.57735);
    return c * cos(an) + cross(k, c) * sin(an) + k * dot(k, c) * (1.0 - cos(an));
}

// scalloped cloud edge: base y + three bumps across x
float cloudEdge(float x, float y0, float amp, float ph) {
    return y0 + amp * (0.6 * sin(x * 9.0 + ph) + 0.4 * sin(x * 17.0 - ph * 0.7));
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float levP  = clamp(audioLevel, 0.0, 1.0);
    float t = TIME * motionSpeed;
    float aa = 1.6 / R.y;

    // poster inks
    vec3 plum   = hueRotate(vec3(0.45, 0.12, 0.38), hueShift);
    vec3 grass  = hueRotate(vec3(0.42, 0.55, 0.15), hueShift);
    vec3 purple = hueRotate(vec3(0.55, 0.25, 0.62), hueShift);
    vec3 gold   = hueRotate(vec3(0.98, 0.72, 0.10), hueShift);
    vec3 orange = hueRotate(vec3(0.96, 0.45, 0.08), hueShift);
    vec3 blue   = hueRotate(vec3(0.38, 0.55, 0.85), hueShift);
    vec3 teal   = hueRotate(vec3(0.18, 0.52, 0.45), hueShift);
    vec3 red    = hueRotate(vec3(0.88, 0.10, 0.12), hueShift);

    // ── 1. halftone dot wall ────────────────────────────────────────────
    float ramp = clamp(0.5 + q.x * 0.5 - q.y * 0.55, 0.0, 1.0);
    vec3 wallA = mix(grass, plum, ramp);
    // registration drift + shimmer
    vec2 dg = q * dotScale + vec2(t * 0.10, -t * 0.07);
    vec2 did = floor(dg);
    vec2 dlp = fract(dg) - 0.5;
    float dr = 0.24 + 0.14 * sin(ramp * 5.0 + t * 0.2)
             + 0.05 * vnoise(did * 0.5) * (1.0 + amt * 1.2 * highP);
    float dm = smoothstep(dr + aa * dotScale, dr - aa * dotScale, length(dlp));
    vec3 col = mix(wallA * 0.72, wallA * 1.18, dm);

    // ── 2. purple ring + golden orb ─────────────────────────────────────
    float orbR = 0.42 * orbSize * (1.0 + amt * 0.04 * bassP + 0.008 * sin(t * 0.5));
    float r = length(q * vec2(1.0, 1.04));
    float ring = smoothstep(orbR + 0.045 + aa, orbR + 0.045 - aa, r);
    col = mix(col, purple, ring);
    float orb = smoothstep(orbR + aa, orbR - aa, r);
    // golden orb with airbrushed orange poles (top & bottom like the poster)
    vec3 orbC = gold;
    float pole = exp(-pow((abs(q.y) - orbR * 0.55) * 4.5, 2.0));
    orbC = mix(orbC, orange, clamp(pole + 0.35 * smoothstep(0.15, orbR, abs(q.y)), 0.0, 1.0) * 0.8);
    orbC *= 1.0 + amt * 0.10 * levP;
    col = mix(col, orbC, orb);

    // ── 3. the blue cloud band with waving checkerwork ──────────────────
    if (bandAmt > 0.001) {
        float ph = t * waveSpeed * 0.7 + amt * 0.5 * midP;
        float topE = cloudEdge(q.x, 0.16, 0.035 * bandAmt, ph);
        float botE = cloudEdge(q.x, -0.16, 0.035 * bandAmt, -ph * 0.8);
        float inBand = smoothstep(topE + aa * 3.0, topE - aa * 3.0, q.y)
                     * smoothstep(botE - aa * 3.0, botE + aa * 3.0, q.y)
                     * smoothstep(orbR * 1.14, orbR * 0.82, abs(q.x)) * bandAmt;
        if (inBand > 0.001) {
            // blue webbing base
            vec3 band = blue;
            // checker inside, waving like a flag
            vec2 cg = vec2(q.x * 9.0, (q.y + 0.05 * sin(q.x * 6.0 + ph)) * 9.0);
            vec2 cid = floor(cg);
            float chk = mod(cid.x + cid.y, 2.0);
            float inChk = smoothstep(0.30, 0.28, abs(fract(cg.x) - 0.5))
                        * smoothstep(0.30, 0.28, abs(fract(cg.y) - 0.5));
            // checker cells blink on beats
            float blink = step(0.93, hash21(cid + floor(t * 2.0))) * amt * beatP;
            vec3 cell = mix(red, orange, chk);
            cell *= 1.0 + blink * 0.6;
            band = mix(band, cell, inChk * smoothstep(0.14, 0.10, abs(q.y)) * 0.95);
            col = mix(col, band, inBand);
        }
        // red teardrop eyes in blue pods at the band's outer wings
        for (int i = 0; i < 2; i++) {
            float sx = i == 0 ? -1.0 : 1.0;
            vec2 pc = vec2(sx * orbR * 1.18, 0.0);
            vec2 pd = (q - pc) * vec2(1.0, 1.8);
            float pod = smoothstep(0.085 + aa, 0.085 - aa, length(pd));
            col = mix(col, blue, pod * bandAmt);
            float eye = smoothstep(0.045 + aa, 0.045 - aa, length(pd * vec2(1.3, 1.0)));
            col = mix(col, red, eye * bandAmt);
        }
    }

    // ── 4. teal thought-clouds at the shoulders ─────────────────────────
    for (int i = 0; i < 2; i++) {
        float sx = i == 0 ? -1.0 : 1.0;
        vec2 cc = vec2(sx * orbR * 1.02, 0.16);
        cc.x += sx * 0.02 * sin(t * driftSpeed * 0.5 + float(i) * 2.0);
        vec2 cd = q - cc;
        // blobby cloud: union of 3 discs, breathing
        float dcl = length(cd * vec2(1.1, 1.5)) - 0.09;
        dcl = min(dcl, length((cd - vec2(sx * 0.07, 0.03)) * vec2(1.1, 1.6)) - 0.065);
        dcl = min(dcl, length((cd + vec2(sx * 0.02, 0.06)) * vec2(1.2, 1.5)) - 0.055);
        float cm = smoothstep(aa * 2.0, -aa * 2.0, dcl);
        vec3 cloudC = mix(teal, grass, 0.3 + 0.2 * sin(t * 0.3 + float(i)));
        col = mix(col, cloudC, cm * 0.95);
    }

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.016;
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
