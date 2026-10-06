/*{
  "DESCRIPTION": "Element Fire — a radial fire orb: a slowly-morphing sphere of living flame floating in darkness, turbulent fire wrapped around a blinding white heart like a miniature sun, licking radial tendrils breathing in and out, embers spiraling off the surface into the dark, heat shimmer bending the air around it. Bass roars the orb bigger, beats burst embers off the surface, mids flicker the tendrils, level stokes the whole star.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "flameHeight",  "LABEL": "Orb Size",      "TYPE": "float", "MIN": 0.4, "MAX": 1.6, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "flameWidth",   "LABEL": "Tendril Reach", "TYPE": "float", "MIN": 0.4, "MAX": 1.6, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "emberAmt",     "LABEL": "Embers",        "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "heatShimmer",  "LABEL": "Heat Shimmer",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Shape / Geometry" },
    { "NAME": "hueShift",     "LABEL": "Hue Shift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "orbMorph",     "LABEL": "Orb Morph",     "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "flameFlow",    "LABEL": "Flame Outflow", "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "emberSpin",    "LABEL": "Ember Spin",    "TYPE": "float", "MIN": -2.0,"MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "orbSway",      "LABEL": "Orb Sway",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// ELEMENT FIRE — redesigned as a RADIAL FIRE ORB (Lu: "fire in the center,
//   like an orb... a sphere that's slowly morphing"). A miniature sun:
//   the orb boundary is displaced by slow seam-free angular fbm (sampled
//   on the unit circle so there is no atan seam) so the sphere breathes
//   and morphs; the fire body is two octaves of fbm turbulence advected
//   RADIALLY OUTWARD (domain offset along -dirOut·t) so flame streams off
//   the core in every direction; slowly-morphing angular lobes give the
//   silhouette licking radial tendrils. Black-body ramp: near-black red →
//   ember → orange → gold → white heart that saturates at the core.
//   Embers spiral off the orb surface on rising helical paths; heat
//   shimmer refracts the darkness around the sphere. Bass feeds orb
//   ENERGY (radius + heart), mids push turbulence phase (additive — no
//   time multiplication), beats flare embers, level stokes the light.
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

    float t   = TIME * motionSpeed;
    float tm  = t * orbMorph;                                   // MOTION: sphere morph clock
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ── heat shimmer: refract the darkness AROUND the orb ───────────────
    // MOTION: the whole star sways on a slow figure-8 through the dark
    q -= orbSway * 0.07 * vec2(sin(t * 0.43), sin(t * 0.86 + 1.2) * 0.6);
    float r0 = length(q);
    float shimZone = smoothstep(0.02, 0.30, r0 - 0.16 * flameHeight);
    q += heatShimmer * 0.010 * shimZone * vec2(
        sin(q.y * 26.0 + t * 5.0 + fbm(q * 3.0) * 4.0),
        sin(q.x * 23.0 - t * 4.3 + fbm(q * 3.0 + 5.0) * 4.0));

    // ── the orb: slowly-morphing sphere of fire ─────────────────────────
    float roar = 1.0 + amt * (0.45 * bassP + 0.30 * levelP);   // bass + level = energy
    float tt   = t + amt * 0.9 * midP;                          // additive flicker phase
    float r    = length(q);
    vec2  dirO = q / max(r, 0.015);                             // outward flow direction
    vec2  ca   = dirO;                                          // point on unit circle → seam-free angular noise

    // morphing radius: two slow seam-free angular fbm layers displace the sphere
    float wob  = fbm(ca * 1.8 + vec2(tm * 0.21, -tm * 0.17));
    float wob2 = fbm(ca * 3.6 + vec2(-tm * 0.13, tm * 0.19));
    float orbR = 0.16 * flameHeight * roar * (1.0 + 0.30 * (wob - 0.5) + 0.16 * (wob2 - 0.5));

    // licking radial tendrils: slowly-morphing angular lobes set the reach
    float lob   = fbm(ca * 2.6 + vec2(tt * 0.11, -tt * 0.09) * orbMorph);
    float reach = flameWidth * (0.09 + 0.34 * pow(lob, 1.7)) * (1.0 + amt * 0.25 * bassP);

    // fire turbulence streaming radially outward (domain advected along dirO)
    float n1 = fbm(q * 4.6 - dirO * tt * 0.55 * flameFlow + vec2(3.1, 7.2)); // MOTION: outward advection rate
    float n2 = fbm(q * 8.8 - dirO * tt * 1.05 * flameFlow + vec2(9.4, 1.7));
    float shell = r - orbR;
    float body  = smoothstep(reach, -0.02, shell);
    float fire  = body * (0.55 + 0.80 * n1) * (0.50 + 0.72 * n2);
    fire = pow(clamp(fire * 2.0, 0.0, 1.0), 1.7);
    // fine licking detail crawling outward across the surface
    fire += 0.10 * body * (vnoise(q * 15.0 - dirO * tt * 2.2 * flameFlow) - 0.5);
    // hot white heart — saturates the ramp at the core, swells with bass
    fire += smoothstep(orbR * 0.95, orbR * 0.18, r) * (1.1 + amt * 0.5 * bassP);

    // ── black-body ramp ─────────────────────────────────────────────────
    vec3 col = vec3(0.015, 0.005, 0.004);                        // near-black warmth
    col = mix(col, vec3(0.42, 0.03, 0.01), smoothstep(0.02, 0.22, fire));
    col = mix(col, vec3(0.90, 0.26, 0.02), smoothstep(0.20, 0.48, fire));
    col = mix(col, vec3(1.00, 0.66, 0.08), smoothstep(0.45, 0.72, fire));
    col = mix(col, vec3(1.00, 0.94, 0.70), smoothstep(0.70, 0.95, fire));
    // hottest heart tips toward blue-white (true black-body top end)
    col = mix(col, vec3(0.82, 0.90, 1.00), smoothstep(1.35, 1.9, fire) * 0.40);
    // crisp limb line: a tight bright rim where the morphing sphere meets the dark
    col += vec3(1.0, 0.60, 0.15) * exp(-abs(shell) * 120.0) * 0.45;
    // cold deep-space haze far from the star — hue depth against the fire
    col += vec3(0.014, 0.020, 0.048) * smoothstep(0.20, 0.60, r);
    // dim smoke veil curling off the star — slow, colored, keeps the dark alive
    float smoke = fbm(q * 2.6 - dirO * t * 0.10 + vec2(0.0, t * 0.05));
    vec3 smokeCol = mix(vec3(0.060, 0.014, 0.010), vec3(0.026, 0.020, 0.066),
                        fbm(q * 1.4 + 3.0 + vec2(t * 0.03, -t * 0.02)));
    col += smokeCol * smoke * smoothstep(0.02, 0.28, shell) * 2.0;
    // corona glow bleeding off the sphere into the dark, breathing with level
    float stoke = 1.0 + amt * 0.6 * levelP;
    col += vec3(0.32, 0.08, 0.02) * exp(-max(shell, 0.0) * 7.5) * roar * 0.55 * stoke;
    col += vec3(0.10, 0.02, 0.005) * exp(-r * 2.0) * roar * 0.5 * stoke;
    // faint drifting spark dust in the darkness (micro-detail, keeps silence alive)
    vec2 dg = q * 34.0 + vec2(t * 0.25, -t * 0.15);
    vec2 did = floor(dg); vec2 dfr = fract(dg) - 0.5;
    vec2 doff = (vec2(hash21(did), hash21(did + 11.3)) - 0.5) * 0.7;
    float dd = length(dfr - doff);
    float dust = smoothstep(0.16, 0.04, dd) * step(0.90, hash21(did + 4.7))
               * (0.5 + 0.5 * sin(t * 1.8 + hash21(did) * TAU));
    col += vec3(0.60, 0.22, 0.06) * dust * 0.42 * smoothstep(0.05, 0.25, shell) * stoke;

    // ── embers spiraling off the orb surface ────────────────────────────
    for (int i = 0; i < 14; i++) {
        float fi = float(i);
        float h1 = hash11(fi * 12.99 + 1.0);
        float h2 = hash11(fi * 4.41 + 7.0);
        float h3 = hash11(fi * 7.77 + 3.0);
        float life = fract(h1 + t * (0.10 + 0.08 * h2));
        float spin = mix(-1.0, 1.0, step(0.5, h2));              // both spiral directions
        float th   = h1 * TAU + (t * spin * (0.35 + 0.45 * h3) + life * 2.2 * spin) * emberSpin; // MOTION: spiral rate / direction
        float er_  = 0.16 * flameHeight + life * (0.24 + 0.24 * flameWidth);
        vec2  ep   = er_ * vec2(cos(th), sin(th));
        float es   = 0.0035 + 0.006 * h3 * (1.0 - life);
        float ed   = length(q - ep);
        float flare = 1.0 + amt * (1.4 * beatP + 0.6 * highP) * step(0.5, h1);
        float fade  = (1.0 - life) * smoothstep(0.0, 0.10, life) * emberAmt;
        col += vec3(1.0, 0.55, 0.12) * flare * fade
             * (smoothstep(es, es * 0.3, ed) + 0.6 * exp(-ed * 70.0));
    }

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
