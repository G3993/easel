/*{
  "DESCRIPTION": "Candy Contours — a hypnotic topographic map of candy: dozens of concentric multicolor stripe rings wrapped around a tilted, elongated, slowly wobbling core, every ring its own hashed vivid ink, all of them flowing endlessly toward the center like taffy being pulled. Bass squeezes the whole form, mids wobble its silhouette, beats hue-shift the ring deck, and the stripes ride the level.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "ringWidth",    "LABEL": "Ring Width",    "TYPE": "float", "MIN": 0.01, "MAX": 0.08, "DEFAULT": 0.028, "GROUP": "Shape / Geometry" },
    { "NAME": "elongation",   "LABEL": "Elongation",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,   "GROUP": "Shape / Geometry" },
    { "NAME": "tilt",         "LABEL": "Tilt",          "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.35,  "GROUP": "Shape / Geometry" },
    { "NAME": "wobble",       "LABEL": "Wobble",        "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,   "GROUP": "Shape / Geometry" },
    { "NAME": "paletteShift", "LABEL": "Palette Shift", "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,   "GROUP": "Color" },
    { "NAME": "vivid",        "LABEL": "Vividness",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.8,   "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,   "GROUP": "Color" },
    { "NAME": "flowSpeed",    "LABEL": "Flow Speed",    "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,   "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.45,  "GROUP": "Audio Reactivity" },
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
// CANDY CONTOURS — from the striped-loops poster: an anisotropic distance
//   field (rotated, elongated metric with a rounded-capsule core) whose
//   iso-lines become fat candy stripes. Each ring index hashes to its own
//   ink from a two-cosine palette family so neighbors clash joyfully like
//   the reference; rings march steadily inward (fract flow), the whole
//   silhouette breathes with low-frequency fbm wobble, and the outermost
//   ring melts into warm gallery-paper gray. Fuses: contour-line topo
//   fields, the tangle poster's flat-ink discipline, and the airbrush
//   grain finish of shapes_and_gradients.
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
vec3 pal(float h) { return 0.5 + 0.5 * cos(TAU * h + vec3(0.0, 2.094, 4.188)); }

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * flowSpeed * 0.4;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ── anisotropic capsule metric: tilted, elongated, wobbling ─────────
    float ang = tilt * 0.9 + 0.05 * sin(t * 0.21);
    float ca = cos(ang), sa = sin(ang);
    vec2 p = vec2(ca * q.x - sa * q.y, sa * q.x + ca * q.y);
    float squeeze = 1.0;                                        // bass rescale jittered every edge → chop
    p.x /= max(1.0 + 1.6 * elongation, 1e-3) / squeeze;         // elongate along tilt
    p.y *= squeeze;
    // capsule core: distance to a short segment instead of a point
    float capLen = 0.10 + 0.25 * elongation;
    p.x -= clamp(p.x, -capLen, capLen);
    float d = length(p);
    // low-frequency silhouette wobble (angle-domain noise, seam-free)
    float th = atan(q.y, q.x);
    vec2 circ = vec2(cos(th), sin(th));
    d *= 1.0 + wobble * (0.18 * vnoise(circ * 1.8 + t * 0.10) - 0.09)
             + amt * 0.05 * midP * sin(3.0 * th + t * 2.0);

    // ── stripe deck flowing inward ──────────────────────────────────────
    float w = ringWidth;
    float flow = t * 0.5;
    float idx = floor(d / w + flow);
    float frac = fract(d / w + flow);
    float hueKick = amt * 0.05 * beatP;
    float h = hash11(idx * 7.31);
    // two clashing palette families interleaved, like the reference
    vec3 ink = (mod(idx, 2.0) < 1.0)
             ? pal(paletteShift + h * 0.9 + hueKick)
             : pal(paletteShift + 0.45 + h * 0.5 + hueKick);
    ink = mix(vec3(dot(ink, vec3(0.333))), ink, 0.45 + 0.55 * vivid);
    ink *= 0.75 + 0.25 * hash11(idx * 3.17);
    // soft anti-aliased stripe borders + a slight rounded-emboss shade
    float edge = smoothstep(0.0, 0.10, frac) * smoothstep(1.0, 0.90, frac);
    ink *= 0.82 + 0.18 * edge + 0.06 * sin(frac * 3.14159);

    // ── melt to paper outside the form ──────────────────────────────────
    vec3 paper = vec3(0.90, 0.89, 0.87);
    vec3 col = mix(ink, paper, smoothstep(0.62, 0.80, d));
    col *= 1.0 - 0.08 * smoothstep(0.55, 1.0, length(q));       // gallery vignette

    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
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
