/*{
  "DESCRIPTION": "Ripple Pool — grainy interference rings on risograph paper: three slow-orbiting drop points ring the pool with expanding waves that collide into moiré blooms, tinted tangerine, ultraviolet and cerulean where each source dominates, all of it dissolved into heavy print grain. Beats drop a fresh splash whose rings race outward, bass stretches the wavelength, mids swirl the sources, highs boil the grain.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "rippleFreq",   "LABEL": "Ring Density",  "TYPE": "float", "MIN": 6.0, "MAX": 40.0, "DEFAULT": 20.0, "GROUP": "Shape / Geometry" },
    { "NAME": "waveAmp",      "LABEL": "Wave Strength", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "grainAmt",     "LABEL": "Print Grain",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.7,  "GROUP": "Color" },
    { "NAME": "paletteShift", "LABEL": "Palette Shift", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "inkVivid",     "LABEL": "Ink Vividness", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.7,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "waveSpeed",    "LABEL": "Wave Speed",    "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
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
// RIPPLE POOL — from the grainy concentric-ripple print: wave_interference
//   and water_drop fused with a risograph finish. Three sources orbit
//   slowly; each emits sin rings with a soft radial envelope. Ink color is
//   chosen by WHICH source dominates locally (tangerine / ultraviolet /
//   cerulean, hue-rotatable), so collisions tint exactly like overlapping
//   riso plates; wave crests darken, troughs lighten, and everything is
//   pushed through heavy hash grain so the surface reads as sprayed ink,
//   not vector rings. Beat = a fourth splash source born at the swirl
//   center, its envelope decaying with beat energy.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

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

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * waveSpeed * 0.5;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ── three orbiting drop points ──────────────────────────────────────
    float orb = 1.0 + amt * 0.5 * midP;
    vec2 s0 = 0.30 * vec2(cos(t * 0.13 * orb), sin(t * 0.11 * orb));
    vec2 s1 = 0.38 * vec2(cos(t * 0.09 * orb + 2.4), sin(t * 0.15 * orb + 1.1));
    vec2 s2 = 0.26 * vec2(cos(t * 0.17 * orb + 4.4), sin(t * 0.07 * orb + 3.0));

    float k = rippleFreq * (1.0 - amt * 0.25 * bassP);          // bass stretches wavelength
    float d0 = length(q - s0), d1 = length(q - s1), d2 = length(q - s2);
    float w0 = sin(d0 * k - t * 2.2) * exp(-d0 * 1.5);
    float w1 = sin(d1 * k * 1.13 - t * 1.8 + 1.0) * exp(-d1 * 1.4);
    float w2 = sin(d2 * k * 0.87 - t * 2.6 + 3.0) * exp(-d2 * 1.7);
    // beat splash from the pool center
    float db = length(q - 0.12 * vec2(sin(t * 0.3), cos(t * 0.23)));
    float wb = sin(db * k * 1.4 - t * 5.0) * exp(-db * 2.0) * amt * (0.4 * beatP + 0.2 * levelP);
    float wave = (w0 + w1 + w2) * waveAmp + wb;

    // ── riso paper + per-source ink tint ────────────────────────────────
    vec3 paper = vec3(0.82, 0.76, 0.78);                        // dusty lavender-gray
    vec3 inkA = vec3(0.95, 0.42, 0.10);                         // tangerine
    vec3 inkB = vec3(0.48, 0.28, 0.95);                         // ultraviolet
    vec3 inkC = vec3(0.20, 0.55, 0.95);                        // cerulean
    float e0 = abs(w0), e1 = abs(w1), e2 = abs(w2);
    float sum = e0 + e1 + e2 + 1e-4;
    vec3 ink = (inkA * e0 + inkB * e1 + inkC * e2) / sum;
    ink = mix(vec3(dot(ink, vec3(0.333))), ink, 0.4 + 0.6 * inkVivid);

    // crests print ink, troughs bleach paper
    float crest = smoothstep(0.05, 0.65, wave);
    float trough = smoothstep(-0.05, -0.65, wave);
    vec3 col = paper;
    col = mix(col, ink, crest * 0.85);
    col = mix(col, vec3(0.95, 0.93, 0.92), trough * 0.6);
    // moiré bloom where two sources collide hard
    float clash = e0 * e1 + e1 * e2 + e0 * e2;
    col = mix(col, ink * 1.25, smoothstep(0.25, 0.6, clash) * 0.5);

    // ── heavy print grain (the reference is mostly grain) ───────────────
    float g1 = hash21(fc * 0.7 + fract(t) * vec2(17.0, 29.0)) - 0.5;
    float g2 = hash21(fc * 0.31 + 7.7) - 0.5;
    float boil = 1.0 + amt * 0.8 * highP;
    col += grainAmt * (0.16 * g1 * boil + 0.10 * g2);
    col = mix(col, paper, grainAmt * 0.15 * step(0.75, hash21(fc * 0.5 + 3.3)));

    // paper border like the print
    vec2 aq = abs(q); float A = 0.5 * R.x / R.y;
    float border = smoothstep(0.0, 0.015, min(A - 0.02 - aq.x, 0.48 - aq.y));
    col = mix(vec3(0.93, 0.91, 0.88), col, border);

    if (paletteShift > 0.001) col = hueRotate(col, paletteShift * TAU);
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
