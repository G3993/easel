/*{
  "DESCRIPTION": "Element Aether — the fifth element, the space the other four float in: a violet-teal nebula slowly folding through the void around a radiant quintessence orb, ringed by a tilted orbit of glowing sigil-stars, with a deep starfield twinkling behind at three parallax depths. The orb breathes, the ring precesses, the nebula never stops folding. Bass breathes the orb and nebula, beats flare a sigil around the ring in sequence, mids fold the nebula faster, highs twinkle the deep field.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "orbSize",      "LABEL": "Orb Size",      "TYPE": "float", "MIN": 0.4, "MAX": 1.6, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "nebulaAmt",    "LABEL": "Nebula",        "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.65, "GROUP": "Shape / Geometry" },
    { "NAME": "ringTilt",     "LABEL": "Ring Tilt",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Shape / Geometry" },
    { "NAME": "starAmt",      "LABEL": "Star Field",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "hueShift",     "LABEL": "Hue Shift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// ELEMENT AETHER — fifth element, closing the set. Nebula = domain-warped
//   fbm in violet↔teal folded by its own field (the la_b_silk engine gone
//   cosmic); starfield = three hash-cell parallax layers drifting at
//   different rates with per-star twinkle phase; the quintessence orb is
//   orby's airbrushed glow distilled — layered gaussians with a white
//   heart and a violet corona; the sigil ring is a tilted ellipse orbit
//   of 8 four-point star glints that flare ONE AT A TIME on beats,
//   stepping around the ring like a clock of light. Sound-off leaves a
//   slow cosmic breath, never black.
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

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * motionSpeed * 0.35;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ── void base ───────────────────────────────────────────────────────
    vec3 col = vec3(0.010, 0.008, 0.030);

    // ── three-depth twinkling starfield ─────────────────────────────────
    for (int i = 0; i < 3; i++) {
        float fi = float(i);
        float sc = 40.0 + fi * 45.0;
        vec2 sp = q * sc + vec2(t * (0.5 + fi * 0.7), fi * 31.0);
        vec2 cell = floor(sp);
        float hs = hash21(cell);
        vec2 off = vec2(hash21(cell + 3.1), hash21(cell + 7.7)) - 0.5;
        float sd = length(fract(sp) - 0.5 - off * 0.6);
        float tw = 0.5 + 0.5 * sin(t * (2.0 + hs * 5.0) + hs * 50.0);
        tw = mix(tw, 1.0, amt * 0.6 * highP);
        float star = step(0.93 - 0.04 * starAmt, hs) * smoothstep(0.10, 0.0, sd);
        col += vec3(0.75, 0.80, 1.0) * star * tw * starAmt * (0.7 - fi * 0.18);
    }

    // ── folding nebula ──────────────────────────────────────────────────
    float foldSpd = 1.0 + amt * 0.7 * midP;
    vec2 p = q * 1.5 + vec2(t * 0.06 * foldSpd, -t * 0.04 * foldSpd);
    float n1 = fbm(p + vec2(t * 0.05, 0.0));
    float n2 = fbm(p * 1.4 + vec2(4.2, 1.7) + n1 * 1.6);
    float neb = fbm(p + 1.8 * vec2(n1, n2));
    float nebGain = nebulaAmt * (1.0 + amt * 0.25 * bassP);
    vec3 violet = vec3(0.38, 0.16, 0.62);
    vec3 teal   = vec3(0.10, 0.48, 0.55);
    vec3 nebCol = mix(violet, teal, smoothstep(0.30, 0.70, n2));
    col += nebCol * nebGain * smoothstep(0.42, 0.72, neb) * 0.95;
    col += vec3(0.85, 0.55, 0.90) * nebGain * smoothstep(0.68, 0.85, neb) * 0.45;

    // ── quintessence orb ────────────────────────────────────────────────
    float s = 0.16 * orbSize * (1.0 + amt * 0.16 * bassP + 0.03 * sin(t * 0.8));
    float r = length(q);
    col += vec3(0.55, 0.35, 0.95) * 0.55 * exp(-(r * r) / (2.6 * s * s));   // corona
    col += vec3(0.45, 0.75, 0.95) * 0.75 * exp(-(r * r) / (0.85 * s * s));
    col = mix(col, vec3(0.98, 0.96, 1.0),
              clamp(exp(-(r * r) / (0.16 * s * s)) * (1.0 + amt * 0.5 * beatP), 0.0, 1.0));

    // ── sigil ring: 8 glints, beat-flared in sequence ───────────────────
    float tiltY = mix(0.85, 0.25, ringTilt);
    float ringR = 0.34 * orbSize;
    float litIdx = mod(floor(TIME * 1.5), 8.0);                  // the clock of light
    for (int k = 0; k < 8; k++) {
        float fk = float(k);
        float a = fk * TAU / 8.0 + t * 0.4;
        vec2 gp = vec2(cos(a), sin(a) * tiltY) * ringR;
        // behind-the-orb dimming for the ring's far side
        float behind = smoothstep(0.0, 0.15, sin(a)) * 0.55;
        vec2 d = q - gp;
        float rr = length(d);
        float flare = 1.0 + amt * 2.2 * beatP * step(abs(fk - litIdx), 0.5);
        float cross = exp(-abs(d.x) * 90.0) * exp(-abs(d.y) * 18.0)
                    + exp(-abs(d.y) * 90.0) * exp(-abs(d.x) * 18.0);
        col += vec3(0.85, 0.75, 1.0) * (1.0 - behind)
             * (cross * 0.35 + exp(-rr * rr * 900.0)) * flare * 0.5;
    }
    // faint elliptical orbit path
    float od = abs(length(q / vec2(1.0, tiltY)) - ringR);
    col += vec3(0.45, 0.35, 0.75) * exp(-od * 60.0) * 0.25;

    // ── finish ──────────────────────────────────────────────────────────
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.02 * gr;
    if (hueShift > 0.001) col = hueRotate(col, hueShift * TAU);
    col *= brightness * mix(1.0, 0.78 + 0.34 * levelP + 0.26 * clamp(audioBass, 0.0, 1.0)
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
