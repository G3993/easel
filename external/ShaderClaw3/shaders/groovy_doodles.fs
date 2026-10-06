/*{
  "DESCRIPTION": "Groovy Doodles — a Miró playground come alive: bold black brush squiggles, primary-color blobs (the blue eye-stone, green leaf, yellow eggs, red bean), scattered dots and dashes on warm gallery cream. Every element floats on its own orbit, squiggles undulate along their length, dashes spin, and each shape hovers over its own soft shadow. Bass bounces the blobs, mids wave the squiggles, beats pop a color accent, highs jitter the confetti.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "density",     "LABEL": "Doodle Density","TYPE": "float", "MIN": 0.4, "MAX": 1.0,  "DEFAULT": 0.85, "GROUP": "Shape / Geometry" },
    { "NAME": "doodleSize",  "LABEL": "Doodle Size",   "TYPE": "float", "MIN": 0.6, "MAX": 1.6,  "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "shadowAmt",   "LABEL": "Hover Shadow",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.55, "GROUP": "Shape / Geometry" },
    { "NAME": "wiggleSpeed", "LABEL": "Wiggle Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 2.0,  "DEFAULT": 0.6,  "GROUP": "Motion / Animation" },
    { "NAME": "driftAmt",    "LABEL": "Float Drift",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "hueShift",    "LABEL": "Palette Spin",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.05, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.25, "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.25, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// GROOVY DOODLES — after the Miró/Matisse doodle-field reference.
//   The canvas is a hashed scatter of elements, each with a home cell in
//   a jittered grid, a slow personal float orbit, and a soft hover
//   shadow (the 3D lift). Element types by hash: big color blob (warped
//   circle with fbm boundary wobble, some with a donut hole + black
//   pupil), black brush squiggle (distance to a wiggling sine worm with
//   thickness variation along its length, round caps), little dots, and
//   short dash strokes at hashed angles. Brush edges get a dry-brush
//   roughen from high-frequency noise so ink feels painted, not vector.
//   Palette: Miró primaries (cobalt, red, chrome yellow, green) spun by
//   the palette knob.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float hash11(float n) { return fract(sin(n) * 43758.5453123); }
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

// distance to a wiggly worm segment centered at origin, length L
float wormD(vec2 p, float L, float amp, float freq, float phase) {
    float x = clamp(p.x, -L, L);
    float y = amp * sin(x * freq + phase);
    // thickness varies along the stroke (brush pressure)
    return length(p - vec2(x, y));
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float t = TIME * motionSpeed;

    // warm gallery cream with paper tooth
    vec3 col = vec3(0.945, 0.92, 0.87);
    col += (vnoise(fc * 0.08) - 0.5) * 0.03;

    vec3 inkK = vec3(0.07, 0.06, 0.06);
    float aa = 1.8 / R.y;

    // Miró primaries
    vec3 pal0 = vec3(0.16, 0.32, 0.78);   // cobalt
    vec3 pal1 = vec3(0.88, 0.12, 0.10);   // red
    vec3 pal2 = vec3(0.99, 0.78, 0.10);   // chrome yellow
    vec3 pal3 = vec3(0.05, 0.55, 0.25);   // green
    vec3 pal4 = vec3(0.95, 0.45, 0.10);   // orange accent

    // scatter grid: 5x6 jittered home cells
    for (int gy = 0; gy < 6; gy++) {
        for (int gx = 0; gx < 5; gx++) {
            vec2 id = vec2(float(gx), float(gy));
            float h  = hash21(id * 3.17 + 5.0);
            float h2 = hash21(id * 7.91 + 2.0);
            float h3 = hash21(id * 1.37 + 9.0);
            if (h3 > density) continue;

            // home + personal float orbit
            vec2 home = (id + 0.5) / vec2(5.0, 6.0) - 0.5;
            home *= vec2(R.x / R.y, 1.0) * 0.98;
            home += (vec2(h, h2) - 0.5) * 0.14;
            vec2 orbit = driftAmt * 0.035 * vec2(sin(t * (0.25 + 0.3 * h) + h * TAU),
                                                 cos(t * (0.2 + 0.25 * h2) + h2 * TAU));
            // bass bounce on the big blobs
            orbit.y += amt * 0.02 * bassP * sin(h * TAU + t * 2.0);
            vec2 p = q - home - orbit;

            float sz = doodleSize * (0.7 + 0.6 * h2);
            float kind = h * 4.0;

            // hover shadow first (under everything this element draws)
            vec2 shp = p - vec2(0.012, -0.018);

            if (kind < 1.6) {
                // ── big color blob, some with donut hole + pupil ────────
                float rad = 0.115 * sz;
                float wob = (vnoise(p * 14.0 + id * 7.0 + t * wiggleSpeed * 0.15) - 0.5) * 0.35;
                float d = length(p) - rad * (1.0 + wob);
                float ds = length(shp) - rad;
                col = mix(col, col * (1.0 - shadowAmt * 0.35), smoothstep(0.02, -0.02, ds) * smoothstep(-0.02, 0.02, d));
                float m = smoothstep(aa, -aa, d);
                float pi5 = floor(h2 * 5.0);
                vec3 bc = pi5 < 1.0 ? pal0 : (pi5 < 2.0 ? pal1 : (pi5 < 3.0 ? pal2 : (pi5 < 4.0 ? pal3 : pal4)));
                bc = hueRotate(bc, hueShift);
                // beat accent + level warmth keep the blobs musical
                bc *= 1.0 + amt * (0.18 * beatP + 0.14 * clamp(audioLevel, 0.0, 1.0));
                col = mix(col, bc, m);
                if (h2 > 0.6) {
                    // donut hole + black pupil (the eye-stone)
                    float hole = smoothstep(aa, -aa, length(p - vec2(rad * 0.15, 0.0)) - rad * 0.45);
                    col = mix(col, vec3(0.955, 0.935, 0.895), hole * m);
                    float pup = smoothstep(aa, -aa, length(p - vec2(rad * 0.15, 0.0)) - rad * 0.16);
                    col = mix(col, inkK, pup * m);
                }
            } else if (kind < 2.7) {
                // ── black brush squiggle (real S-waves along its length) ─
                float ra = h2 * TAU + 0.15 * sin(t * 0.2 + h * 5.0);
                vec2 rp = mat2(cos(ra), -sin(ra), sin(ra), cos(ra)) * p;
                float ph = t * wiggleSpeed * (0.5 + h) + h * TAU + amt * 0.8 * midP;
                float L = 0.11 * sz;
                float freq = (46.0 + 26.0 * h) / sz;
                float d = wormD(rp, L, 0.030 * sz, freq, ph);
                float press = 0.011 * sz * (0.75 + 0.35 * sin(rp.x * 9.0 + h * 7.0));
                // dry-brush roughen (subtle)
                press *= 1.0 + (vnoise(rp * 90.0 + id * 3.0) - 0.5) * 0.25;
                float ds2 = wormD(rp - vec2(0.012, -0.018), L, 0.030 * sz, freq, ph);
                col = mix(col, col * (1.0 - shadowAmt * 0.22), smoothstep(press, press * 0.4, ds2) * smoothstep(press * 0.4, press, d));
                float m = smoothstep(press + aa, press - aa, d);
                col = mix(col, inkK, m);
            } else if (kind < 3.2) {
                // ── little ink dots (confetti) ──────────────────────────
                float jit = amt * 0.006 * highP;
                vec2 jp = p + jit * vec2(hash21(id + floor(t * 8.0)) - 0.5,
                                         hash21(id * 2.0 + floor(t * 8.0)) - 0.5);
                float d = length(jp) - 0.014 * sz;
                float m = smoothstep(aa, -aa, d);
                col = mix(col, inkK, m);
            } else {
                // ── short dash strokes at hashed angles ─────────────────
                float ra = h2 * TAU + t * wiggleSpeed * 0.3 * (h - 0.5);
                vec2 rp = mat2(cos(ra), -sin(ra), sin(ra), cos(ra)) * p;
                float dw = 0.010 * sz;
                float dl = 0.045 * sz;
                vec2 dd = abs(rp) - vec2(dl, dw);
                float d = length(max(dd, 0.0)) + min(max(dd.x, dd.y), 0.0) - dw * 0.6;
                float m = smoothstep(aa, -aa, d);
                // some dashes are colored
                vec3 dc = h2 > 0.75 ? hueRotate(pal1, hueShift) : inkK;
                col = mix(col, dc, m);
            }
        }
    }

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.015;
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
