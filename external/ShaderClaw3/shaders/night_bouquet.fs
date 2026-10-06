/*{
  "DESCRIPTION": "Night Bouquet — abstract luminous flower-forms unfurling out of deep black: three domain-warped spiral blooms that only SUGGEST petals, painted as smooth silk-ink gradients flowing rose into coral into cream into leaf green into iris violet, with crisp mid filaments and fine luminous threads layered over soft back-plumes, everything slowly breathing and unfurling like ink in water. Bass pushes the unfurl, mids sway the forms, beats breathe a luminous ring out of each bloom, highs light the fine threads.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "formScale",    "LABEL": "Form Scale",    "TYPE": "float", "MIN": 0.5, "MAX": 1.6, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "flowAmt",      "LABEL": "Silk Flow",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "filamentAmt",  "LABEL": "Filaments",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "hueDrift",     "LABEL": "Hue Drift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "glowAmt",      "LABEL": "Bloom Glow",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.4,  "GROUP": "Audio Reactivity" },
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
// NIGHT BOUQUET v2 — Georgia O'Keeffe × silk ink in water, not clip-art:
//   the whole canvas rides one domain-warp flow field, and three spiral
//   bloom-forms live inside it. Each form is a polar field on an
//   unfurling spiral coordinate u = θ + k·log(r) — never a literal daisy:
//     · big soft back-plume  (huge smooth gradient body)
//     · crisp silk ribbons   (pow-shaped petal bands, sharp silk edges)
//     · luminous crest lines (exp filaments riding the ribbon crests)
//     · fine detail threads  (high-order threads, high-lit)
//   Color is a cyclic NATURE palette — rose → coral → cream → leaf →
//   iris violet — swept seamlessly through angle, radius and time so the
//   hues flow INTO each other. Audio: bass = additive unfurl push,
//   mids = additive sway, beat = a luminous ring breathing outward,
//   highs = thread light. All phase-additive, chop-free.
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
    for (int i = 0; i < 3; i++) {
        v += a * vnoise(p);
        p = p * 2.13 + vec2(7.3, 3.1);
        a *= 0.5;
    }
    return v;
}
// cyclic nature palette: rose → coral → cream → leaf green → iris violet
vec3 naturePal(float u) {
    u = fract(u) * 5.0;
    vec3 c = mix(vec3(0.93, 0.38, 0.52), vec3(0.98, 0.55, 0.36), smoothstep(0.0, 1.0, u));
    c = mix(c, vec3(0.99, 0.92, 0.74), smoothstep(1.0, 2.0, u));
    c = mix(c, vec3(0.38, 0.68, 0.40), smoothstep(2.0, 3.0, u));
    c = mix(c, vec3(0.52, 0.38, 0.90), smoothstep(3.0, 4.0, u));
    c = mix(c, vec3(0.93, 0.38, 0.52), smoothstep(4.0, 5.0, u));
    return c;
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;
    float t = TIME * motionSpeed * 0.22;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    // smooth luminous swell — brightness follows the music, never the clock
    float swell = 1.0 + amt * (0.20 * bassP + 0.30 * levelP);

    // silk-ink flow: the whole canvas rides one slow domain warp
    vec2 wrp = vec2(fbm(q * 1.5 + vec2(0.0, t * 0.10)),
                    fbm(q * 1.5 + vec2(3.7, -t * 0.08)));
    vec2 qw = q + (wrp - 0.5) * (0.30 + 0.55 * flowAmt);

    // deep black, kept alive by the faintest drifting ink wisps
    vec3 col = vec3(0.004, 0.004, 0.008);
    float wisp = fbm(qw * 2.1 + vec2(t * 0.05, -t * 0.03));
    col += naturePal(wisp * 0.35 + 0.55 + t * 0.012) * pow(wisp, 3.0) * 0.085;

    // ── three unfurling bloom-forms ─────────────────────────────────────
    for (int i = 0; i < 3; i++) {
        float fi = float(i);
        float h2 = hash11(fi * 4.41 + 7.0);
        vec2 c = 0.30 * vec2(cos(fi * 2.094 + 0.7), sin(fi * 2.094 + 0.7));
        c += 0.05 * vec2(sin(t * 0.13 + fi * 2.4), cos(t * 0.11 + fi * 1.7));
        float s = formScale * (0.62 + 0.30 * h2);
        vec2 d = (qw - c) / s;
        float r = max(length(d), 1e-4);
        float th = atan(d.y, d.x);
        float dir = (fi < 1.5) ? 1.0 : -1.0;               // alternate unfurl
        // unfurling spiral coordinate; audio is ADDITIVE phase, never time-scale
        float u = th + dir * (1.55 * log(r + 0.16) + t * (0.16 + 0.05 * fi))
                + amt * (0.45 * bassP + 0.22 * midP * sin(t * 0.4 + fi * 2.0));
        float n = 3.0 + fi;                                 // 3/4/5-fold suggestion
        float det = vnoise(d * 2.6 + vec2(t * 0.07, fi * 5.0));
        float g = sin(n * u + det * 1.8);

        // seamless hue sweep (only full-period angular terms — no seam line)
        float hue = hueDrift + fi * 0.27 + r * 0.25
                  + 0.12 * sin(th + r * 2.0 - t * 0.2) + 0.08 * det + t * 0.015;
        vec3 p1 = naturePal(hue);

        // big soft back-plume: huge smooth gradient body, swelling with bass
        float soft = exp(-r * r * 1.5) * (0.50 + 0.34 * g);
        col += p1 * max(soft, 0.0) * (0.18 + 0.34 * glowAmt) * swell;
        // crisp silk ribbons — pow-shaped petal bands with sharp silk edges
        float band = 0.5 + 0.5 * sin(n * u + r * 3.2);
        float rib = pow(band, 10.0);
        float env = exp(-r * 1.7) * smoothstep(0.03, 0.22, r);
        col += naturePal(hue + 0.06) * rib * env * (0.55 + 0.75 * filamentAmt) * swell;
        // luminous crest filament riding each ribbon edge — pixel-crisp
        float crest = abs(sin(n * u + r * 3.2));
        float filig = exp(-crest * crest * 60.0) * exp(-r * 2.1) * smoothstep(0.02, 0.12, r);
        col += vec3(1.0, 0.92, 0.78) * filig * 0.42 * (0.4 + 0.6 * filamentAmt);
        // fine detail threads, high-lit — two crossing families
        float th3 = u * 2.0 + vnoise(d * 5.0 + vec2(t * 0.09, 3.0)) * 4.5;
        float fine = pow(0.5 + 0.5 * sin(th3 * 7.0 - r * 13.0 + t * 0.5), 24.0)
                   * exp(-r * 2.5) * smoothstep(0.03, 0.15, r);
        fine += pow(0.5 + 0.5 * sin(th3 * 4.0 + r * 17.0 - t * 0.4), 24.0)
                * exp(-r * 2.2) * smoothstep(0.04, 0.18, r) * 0.7;
        col += naturePal(hue + 0.13) * fine * 0.34 * (0.5 + 0.5 * filamentAmt)
             * (1.0 + amt * 0.6 * highP);
        // beat glint: a luminous ring breathing outward from the bloom heart
        float gr_ = fract(t * 0.35 + fi * 0.37) * 1.5;
        float ring = exp(-abs(r - gr_) * 8.0) * smoothstep(1.5, 0.4, r);
        col += naturePal(hue + 0.2) * ring * amt * beatP * 0.34;
    }

    // film grain keeps the black velvety, not flat
    col += 0.014 * (hash21(fc + fract(TIME) * vec2(17.0, 29.0)) - 0.5);
    col *= brightness;
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
        // dim-tracking audio lift (post-trail, chop-safe, <= 1)
        col *= mix(1.0, 0.70 + 0.30 * clamp(audioLevel, 0.0, 1.0), audioReact);
        // HD finish: S-curve contrast + saturation richness
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        gl_FragColor = vec4(col, 1.0);
    }
}
