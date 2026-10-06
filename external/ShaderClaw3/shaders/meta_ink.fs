/*{
  "DESCRIPTION": "Meta Ink — the black-and-white metaball poster: a wall of thin-outlined circles on ink black, where roaming white blob-chains flow through the grid, swallowing circles into smooth liquid links and releasing them again. Pure two-tone, razor-crisp merges — the calendar poster brought to life. Bass fattens the chains, mids steer the roamers, beats grab a new circle, highs tick the outlines.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "gridN",       "LABEL": "Circle Grid",   "TYPE": "float", "MIN": 4.0, "MAX": 9.0,  "DEFAULT": 5.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "blobAmt",     "LABEL": "Blob Coverage", "TYPE": "float", "MIN": 0.2, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "linkWidth",   "LABEL": "Link Fatness",  "TYPE": "float", "MIN": 0.4, "MAX": 1.2,  "DEFAULT": 0.8,  "GROUP": "Shape / Geometry" },
    { "NAME": "flowSpeed",   "LABEL": "Blob Flow",     "TYPE": "float", "MIN": 0.0, "MAX": 2.0,  "DEFAULT": 0.4,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "inkColor",    "LABEL": "Ink Color",     "TYPE": "color", "DEFAULT": [0.04, 0.04, 0.05, 1.0], "GROUP": "Color" },
    { "NAME": "paperColor",  "LABEL": "Blob Color",    "TYPE": "color", "DEFAULT": [0.93, 0.93, 0.90, 1.0], "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.06, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// META INK — after the CALENDAR1O metaball-grid reference.
//   A regular circle grid where every node contributes a metaball
//   kernel whose weight is a slow personal on/off clock (plus roaming
//   attractor gates): nodes near a roamer switch ON, pouring into the
//   white liquid; the summed field is thresholded ONCE, so on-nodes
//   fuse into the reference's smooth pill-chains and off-nodes stand
//   as thin outlined circles (outline = ring where their own kernel
//   would be, drawn in white stroke on black). Diagonal capsule links
//   between grid neighbors switch on the same gates, giving the
//   poster's connected runs. Two-tone, pixel AA, grain only.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
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

    float N = floor(gridN + 0.5);
    float cell = 1.0 / N;                       // grid pitch in q units
    float rad = cell * 0.42;                    // circle radius
    float aa = 1.6 / R.y;

    // roaming attractors that decide which nodes are "liquid"
    float rt = t * flowSpeed * 0.5 + amt * 0.5 * midP;
    vec2 roam1 = 0.34 * vec2(sin(rt * 0.7), cos(rt * 0.53));
    vec2 roam2 = 0.30 * vec2(sin(rt * 0.41 + 2.6), cos(rt * 0.67 + 1.1));

    // field accumulation over the 4x4 neighborhood
    vec2 gpos = q * N;
    vec2 base = floor(gpos - 1.0);
    float field = 0.0;
    float ringInk = 0.0;

    float kw = 0.62 * linkWidth * (1.0 + amt * 0.18 * bassP);

    for (int gy = 0; gy < 4; gy++) {
        for (int gx = 0; gx < 4; gx++) {
            vec2 id = base + vec2(float(gx), float(gy));
            vec2 c = (id + 0.5) * cell;
            float h = hash21(id * 3.7 + 11.0);

            // node gate: personal slow clock + roamer proximity + beat grab
            float gate = 0.5 + 0.5 * sin(t * 0.20 * (0.5 + h) + h * TAU);
            gate = max(gate * 0.7,
                       smoothstep(0.34, 0.10, min(length(c - roam1), length(c - roam2))));
            gate = smoothstep(1.0 - blobAmt, 1.0 - blobAmt + 0.35, gate);
            // beat grabs one hashed node hard
            gate = max(gate, step(0.985, hash21(id + floor(t * 1.5))) * amt * beatP);

            vec2 d = (q - c) / cell;
            float r2 = dot(d, d);
            // smooth metaball kernel with FINITE support so the 4x4
            // neighborhood window never truncates it (no blocky seams)
            float k = kw / (r2 * 3.2 + 0.35);
            k *= smoothstep(2.4, 1.1, r2);
            field += k * gate;

            // off-node outline ring contribution
            float rr = length(q - c);
            float ring = smoothstep(aa, 0.0, abs(rr - rad) - 0.0035);
            ringInk = max(ringInk, ring * (1.0 - gate));
        }
    }

    // threshold ONCE → smooth fused chains
    float th = 1.0;
    float m = smoothstep(th - fwidth(field) * 1.2, th + fwidth(field) * 1.2, field);

    vec3 ink = inkColor.rgb;
    vec3 pap = paperColor.rgb;
    // level warms the paper slightly so silence still breathes
    pap *= 1.0 + amt * 0.08 * levP;

    vec3 col = ink;
    // outlined idle circles (thin white stroke)
    float tick = 1.0 + amt * 0.4 * highP;
    col = mix(col, pap * 0.9, clamp(ringInk, 0.0, 1.0) * 0.9 * tick * (1.0 - m));
    // the liquid
    col = mix(col, pap, m);

    // paper grain
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
