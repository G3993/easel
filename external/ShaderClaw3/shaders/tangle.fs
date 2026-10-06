/*{
  "DESCRIPTION": "Tangle — a living poster of six fat colored tubes hopelessly knotted over warm paper: each rope wanders its own looping Lissajous path in classic poster colors (black, vermilion, cobalt, yellow, green, pink), endlessly re-tangling in slow motion. Beats snap a kink through the ropes, bass thickens them, mids stir the wander, and the whole knot drifts like it's being redrawn forever.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "ropeCount",    "LABEL": "Rope Count",    "TYPE": "float", "MIN": 2.0, "MAX": 6.0, "DEFAULT": 6.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "ropeWidth",    "LABEL": "Rope Width",    "TYPE": "float", "MIN": 0.4, "MAX": 1.8, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "wiggle",       "LABEL": "Wander",        "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "paletteShift", "LABEL": "Palette Shift", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "paperTone",    "LABEL": "Paper Tone",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.9,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.4,  "GROUP": "Audio Reactivity" },
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
// TANGLE — from the POSTERLAD "Tangled" poster. Six ropes, each a closed
//   multi-harmonic Lissajous path sampled at 26 points per pixel; the
//   min point-distance over dense samples approximates tube distance
//   (widths are fat enough that sampling artifacts vanish). Ropes are
//   painted flat, later ropes over earlier — real over/under crossings
//   emerge from draw order like layered screen prints. A faint offset
//   shadow under every rope lifts the knot off the paper. Paths morph
//   continuously (phase drift per harmonic) so the tangle never repeats;
//   beat injects a fast kink harmonic, bass fattens, mids speed the
//   morph. Palette = 6 poster inks, hue-rotatable.
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

// closed wandering path for rope k at parameter s
vec2 ropePath(float s, float k, float t, float wig, float kink) {
    float a = s * TAU;
    vec2 p = vec2(
        0.62 * sin(a + k * 1.7 + 0.4 * sin(t * 0.11 + k)),
        0.50 * sin(2.0 * a + k * 2.9 + t * 0.07));
    p += wig * vec2(0.24 * sin(3.0 * a + t * 0.13 + k * 4.2),
                    0.22 * sin(4.0 * a - t * 0.09 + k * 1.3));
    p += wig * 0.13 * vec2(sin(5.0 * a - t * 0.05 + k * 7.0),
                           sin(6.0 * a + t * 0.06 + k * 3.7));
    p += kink * 0.015 * vec2(sin(6.0 * a + t * 2.0 + k * 5.0),
                             sin(7.0 * a - t * 1.7 + k * 2.0));
    return p;
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * motionSpeed * 0.5;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // warm poster paper with a whisper of pulp texture
    vec3 col = mix(vec3(0.80, 0.79, 0.76), vec3(0.94, 0.93, 0.90), paperTone);
    col *= 1.0 - 0.05 * hash21(floor(q * 240.0));

    // six poster inks
    vec3 ink0 = vec3(0.10, 0.10, 0.10);   // black
    vec3 ink1 = vec3(0.85, 0.25, 0.12);   // vermilion
    vec3 ink2 = vec3(0.16, 0.38, 0.78);   // cobalt
    vec3 ink3 = vec3(0.98, 0.78, 0.10);   // yellow
    vec3 ink4 = vec3(0.22, 0.62, 0.32);   // green
    vec3 ink5 = vec3(0.95, 0.66, 0.76);   // pink

    float tm   = t + amt * 0.35 * midP;               // phase push, not time-scale (chop-free)
    float kink = amt * beatP;
    float wid  = 0.042 * ropeWidth * (1.0 + amt * 0.22 * bassP);
    float rc   = floor(ropeCount + 0.5);

    for (int k = 0; k < 6; k++) {
        float fk = float(k);
        if (fk >= rc) continue;
        // dense sampling of the path → approximate tube distance
        float d = 1e3;
        for (int i = 0; i < 26; i++) {
            float s = (float(i) + 0.5) / 26.0;
            vec2 pp = ropePath(s, fk, tm, wiggle, kink);
            d = min(d, length(q - pp));
        }
        // soft offset shadow, then flat ink with a crisp edge
        float sh = smoothstep(wid + 0.020, wid * 0.6, d - 0.012);
        col *= 1.0 - 0.16 * sh;
        vec3 ink = (k == 0) ? ink0 : (k == 1) ? ink1 : (k == 2) ? ink2
                 : (k == 3) ? ink3 : (k == 4) ? ink4 : ink5;
        float m = smoothstep(wid, wid - 0.006, d);
        col = mix(col, ink, m);
        // subtle round-tube shading: lighter along the middle
        col += ink * 0.18 * smoothstep(wid * 0.6, 0.0, d) * m;
    }

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
