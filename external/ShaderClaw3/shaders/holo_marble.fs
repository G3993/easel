/*{
  "DESCRIPTION": "Holo Marble — holographic ink marbling on warm cream: saturated pastel rivers of pink, mint, lemon and violet stream diagonally like wet marbling ink, every band engraved with fine topographic micro-contours, while deep navy tendrils cut through the flow and soft white gleams ride the currents. Mids deepen the marbling warp, beats send a gleam sweeping across the sheet, bass slows and swells the rivers, highs sharpen the engraving.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "warpAmt",      "LABEL": "Marble Warp",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.65, "GROUP": "Shape / Geometry" },
    { "NAME": "contourFreq",  "LABEL": "Micro Contours","TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.55, "GROUP": "Shape / Geometry" },
    { "NAME": "inkAmt",       "LABEL": "Dark Ink",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Color" },
    { "NAME": "hueBase",      "LABEL": "Hue Base",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.85, "GROUP": "Color" },
    { "NAME": "hueSpread",    "LABEL": "Hue Spread",    "TYPE": "float", "MIN": 0.2,  "MAX": 1.5,  "DEFAULT": 0.8,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "driftSpeed",   "LABEL": "Drift Speed",   "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.4,  "GROUP": "Audio Reactivity" },
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
// HOLO MARBLE — from the psychedelic holographic-marbling reference:
//   la_b_silk's double domain-warp engine pushed saturated, plus the
//   engraved micro-contour lines that give printed marbling its tooth
//   (thin sin-of-field darkening at high frequency), plus glitch_god's
//   love of a dark ink phase: where the field crests, the flow turns
//   deep navy-black tendril, exactly like the reference's black river.
//   A traveling white gleam crosses the sheet on beats. The whole sheet
//   drifts diagonally so the marbling reads as slow pouring, not static
//   noise.
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
float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.07 + vec2(7.3, 3.1);
        a *= 0.52;
    }
    return v;
}
vec3 pal(float h) { return 0.5 + 0.5 * cos(TAU * h + vec3(0.0, 2.094, 4.188)); }

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * driftSpeed * 0.25;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ── double-warped marbling field, drifting diagonally ───────────────
    float warp = (1.2 + 1.6 * warpAmt) * (1.0 + amt * 0.55 * midP);
    float swell = 1.0 + amt * 0.15 * bassP;
    vec2 p = q * 1.6 * swell + vec2(t * 0.5, -t * 0.35);
    float q1 = fbm(p + vec2(t * 0.2, 0.0));
    float q2 = fbm(p + vec2(4.7, 2.1) - vec2(0.0, t * 0.15));
    float f  = fbm(p + warp * vec2(q1, q2));

    // ── pastel-saturated ink bands ──────────────────────────────────────
    float hue = hueBase + hueSpread * (f * 1.1 + 0.25 * q1) + t * 0.02;
    vec3 col = mix(vec3(1.0), pal(hue), 0.72);                   // milk-lifted saturation
    // cream sheet shows through in the low field
    col = mix(vec3(0.95, 0.93, 0.87), col, smoothstep(0.18, 0.42, f));
    // deep navy tendrils where the field crests
    vec3 navy = vec3(0.05, 0.05, 0.18);
    // bass grows the ink territory — a structural response, not just a lift
    float inkTh = 0.66 - amt * 0.10 * bassP;
    col = mix(col, navy, inkAmt * smoothstep(inkTh, inkTh + 0.14, f));
    // bright pooled cores inside the tendrils (the reference's glowing eyes)
    col = mix(col, pal(hue + 0.45) * 1.1, smoothstep(0.80, 0.90, f) * inkAmt);

    // ── engraved micro-contours ─────────────────────────────────────────
    float eng = sin(f * (60.0 + 200.0 * contourFreq) + q1 * 8.0);
    float engStr = 0.16 * contourFreq * (1.0 + amt * 0.8 * highP);
    col *= 1.0 - engStr * smoothstep(0.2, 0.9, abs(eng)) * smoothstep(0.15, 0.4, f);

    // ── beat gleam sweeping the sheet ───────────────────────────────────
    float sweep = fract(t * 0.7);
    float gl = exp(-pow((dot(q, normalize(vec2(0.8, 0.6))) + 0.9 - sweep * 1.8) * 6.0, 2.0));
    col += vec3(1.0, 0.98, 0.92) * gl * amt * (0.8 * beatP + 0.3 * midP);
    // slow ambient gleam so silence still shimmers
    col += vec3(0.06) * exp(-pow((q.x + q.y - sin(t * 0.3)) * 3.0, 2.0));

    float gr = hash21(fc + fract(t * 2.0) * vec2(17.0, 29.0)) - 0.5;
    col += 0.02 * gr;
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
