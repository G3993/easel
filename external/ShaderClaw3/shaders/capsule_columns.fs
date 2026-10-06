/*{
  "DESCRIPTION": "Capsule Columns — the gradient pill wall: tall rounded capsules on cornflower blue, each pouring red-orange through deep blue back to gold, sliding vertically at their own speeds like slot reels. Rounded 3D cylinder shading and soft shadows lift them off the page. Bass pumps the pour, mids slide the reels, beats bounce a column, highs shimmer the edges.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "columnCount", "LABEL": "Columns",       "TYPE": "float", "MIN": 4.0, "MAX": 12.0, "DEFAULT": 7.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "capWidth",    "LABEL": "Capsule Width", "TYPE": "float", "MIN": 0.5, "MAX": 1.0,  "DEFAULT": 0.82, "GROUP": "Shape / Geometry" },
    { "NAME": "splitAmt",    "LABEL": "Column Splits", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "slideSpeed",  "LABEL": "Reel Slide",    "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "pourSpeed",   "LABEL": "Gradient Pour", "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.4,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "fieldColor",     "LABEL": "Field Color",   "TYPE": "color", "DEFAULT": [0.42, 0.53, 0.89, 1.0], "GROUP": "Color" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.08, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// CAPSULE COLUMNS — after the gradient-pill poster reference.
//   Column lattice; each column holds one long capsule, or splits into
//   two stacked capsules with a gap (hash + splitAmt), like the
//   reference's broken columns. Each capsule's gradient: the signature
//   red-orange → deep blue → red-orange → gold ramp, phase-offset per
//   capsule and pouring vertically. Reels: every column's content
//   slides at its own speed. Cylinder shading: cos profile across the
//   width + a vertical sheen line; soft drop shadow on the blue field.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float hash11(float n) { return fract(sin(n) * 43758.5453123); }

// the reference ramp: gold → red-orange → deep blue → teal-green, cyclic
vec3 pillRamp(float v) {
    v = fract(v);
    vec3 gold  = vec3(1.00, 0.72, 0.15);
    vec3 orng  = vec3(0.96, 0.30, 0.08);
    vec3 dred  = vec3(0.72, 0.10, 0.12);
    vec3 dblu  = vec3(0.10, 0.28, 0.75);
    vec3 teal  = vec3(0.35, 0.78, 0.62);
    vec3 c = mix(gold, orng, smoothstep(0.00, 0.20, v));
    c = mix(c, dred, smoothstep(0.20, 0.38, v));
    c = mix(c, dblu, smoothstep(0.38, 0.58, v));
    c = mix(c, teal, smoothstep(0.62, 0.80, v));
    c = mix(c, gold, smoothstep(0.82, 1.00, v));
    return c;
}

vec3 hueRotate(vec3 c, float a) {
    float an = a * TAU;
    vec3 k = vec3(0.57735);
    return c * cos(an) + cross(k, c) * sin(an) + k * dot(k, c) * (1.0 - cos(an));
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 uv = fc / R;
    float asp = R.x / R.y;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float levP  = clamp(audioLevel, 0.0, 1.0);
    float t = TIME * motionSpeed;

    vec3 field = fieldColor.rgb;
    field *= 0.96 + 0.06 * uv.y;                 // subtle page light
    vec3 col = field;

    float N = floor(columnCount + 0.5);
    // margins like the poster
    float mx = 0.09;
    float colW = (1.0 - 2.0 * mx) / N;
    float x = (uv.x - mx) / colW;                // column space
    float ci = floor(x);
    float lx = fract(x) - 0.5;                   // -0.5..0.5 across column

    if (ci >= 0.0 && ci < N) {
        float hc = hash11(ci * 7.31 + 2.0);
        float hw = capWidth * 0.5 * 0.92;

        // reel slide per column (mid pushes phase — chop-free)
        float slide = t * slideSpeed * (0.25 + 0.35 * hc) + amt * 0.4 * midP;
        float y = uv.y + slide * 0.3;

        // split pattern: this column is either one capsule or two,
        // with gap edges at hashed heights, cycling slowly
        float seg = y * 1.0;
        float gapC = hash11(ci * 3.1 + floor(seg) * 5.7);
        float segY = fract(seg);

        // capsule vertical extent inside this segment cycle
        float topPad = 0.06 + 0.30 * splitAmt * step(0.45, gapC) * hash11(ci * 9.7 + floor(seg));
        float botPad = 0.06;
        // rounded-end distance: inside if segY in [botPad+r, 1-topPad-r] band
        float capR = hw * colW / 1.0;            // end radius in y units approx
        float capRy = capR * asp;                // aspect-correct-ish
        float y0 = botPad;
        float y1 = 1.0 - topPad;

        // distance in column-local space (x scaled to y units)
        float dxu = abs(lx) * colW * asp / 1.0;
        float dy = max(max(y0 + capRy - segY, segY - (y1 - capRy)), 0.0);
        float d = sqrt(dxu * dxu + dy * dy) - capRy * (capWidth / 0.82) * 0.0 - hw * colW * asp;

        float aa = 1.8 / R.y;
        float m = smoothstep(aa, -aa, d);

        if (m > 0.001) {
            // pouring gradient, phase per column + per segment
            float pour = y * 1.15 + t * pourSpeed * 0.22 + hc * 0.9
                       + amt * 0.10 * bassP;
            vec3 pc = pillRamp(pour);
            pc = hueRotate(pc, hueShift);
            // cylinder shading across the width + sheen line
            float prof = cos(clamp(lx / hw, -1.0, 1.0) * 1.5707);
            pc *= 0.72 + 0.34 * prof;
            pc += vec3(1.0) * pow(max(prof, 0.0), 24.0) * 0.06;
            // beat bounce: one column flashes bright
            float pick = step(abs(ci - mod(floor(t * 1.3), N)), 0.1);
            pc *= 1.0 + pick * amt * beatP * 0.25 + amt * 0.15 * levP;
            // shimmer on highs along the rounded edge
            pc += vec3(1.0, 0.95, 0.85) * exp(-abs(d) * 320.0) * amt * 0.3 * highP;
            col = mix(col, pc, m);
        }
        // soft drop shadow just right-below each capsule
        float dsh = sqrt(dxu * dxu + dy * dy) - hw * colW * asp;
        float sh = smoothstep(0.035, 0.0, dsh + 0.012) * (1.0 - m);
        col = mix(col, field * 0.82, sh * 0.5);
    }

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.014;
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
