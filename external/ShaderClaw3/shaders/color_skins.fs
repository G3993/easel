/*{
  "DESCRIPTION": "Color Skins — living animal-print camouflage: molten orange islands on black with a teal capillary web threading the gaps, exactly the reference's three-tone organic skin. A persistent flow field keeps the blobs crawling, splitting and re-merging forever without looping — real reaction-diffusion feel across four passes. Bass swells the islands, mids stir the flow, beats send a growth wave through the skin, highs light the teal web.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "cellScale",   "LABEL": "Skin Scale",    "TYPE": "float", "MIN": 2.0, "MAX": 14.0, "DEFAULT": 9.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "webAmt",      "LABEL": "Teal Web",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "coverage",    "LABEL": "Blob Coverage", "TYPE": "float", "MIN": 0.2, "MAX": 0.8,  "DEFAULT": 0.55, "GROUP": "Shape / Geometry" },
    { "NAME": "flowSpeed",   "LABEL": "Crawl Speed",   "TYPE": "float", "MIN": 0.0, "MAX": 2.0,  "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "currentAngle", "LABEL": "Current Angle", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Motion" },
    { "NAME": "crawlDepth",   "LABEL": "Crawl Warp",    "TYPE": "float", "MIN": 0.0, "MAX": 0.6,  "DEFAULT": 0.18, "GROUP": "Motion" },
    { "NAME": "stirRate",     "LABEL": "Stir Rate",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.21, "GROUP": "Motion" },
    { "NAME": "breathDepth",  "LABEL": "Breath Depth",  "TYPE": "float", "MIN": 0.0, "MAX": 0.12, "DEFAULT": 0.02, "GROUP": "Motion" },
    { "NAME": "colorA",      "LABEL": "Island Color",  "TYPE": "color", "DEFAULT": [0.95, 0.26, 0.08, 1.0], "GROUP": "Color" },
    { "NAME": "colorB",      "LABEL": "Web Color",     "TYPE": "color", "DEFAULT": [0.24, 0.72, 0.55, 1.0], "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.05, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "csFlow", "PERSISTENT": true },
    { "TARGET": "csScene" },
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// COLOR SKINS — after the orange/black/teal organic camo reference.
//   Four passes:
//   0. csFlow (persistent): a slowly-evolving warp field. Each frame the
//      old field is decayed and re-energized with drifting curl noise —
//      the skin's "metabolism". Because it accumulates, the motion is
//      non-looping and organic (real Gray-Scott is unstable in this
//      host, so this feedback field carries the RD feel instead).
//   1. csScene: the skin itself. Two warped fbm fields are thresholded
//      into three tones: orange islands (with molten inner shading and
//      a bright lip), black channels, and a thin teal capillary web
//      that lives only where the channel narrows. Pixel-tight AA.
//   2. abTrail: house luminous trail buffer.
//   3. composite: chromatic lens + bloom + HD finish.
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
        p = p * 2.13 + vec2(7.3, 3.1);
        a *= 0.5;
    }
    return v;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float t = TIME * motionSpeed;

    if (PASSINDEX == 0) {
        // ── evolving flow field (persistent) ────────────────────────────
        vec2 old = texture2D(csFlow, uv).rg * 2.0 - 1.0;
        float tw = t * flowSpeed * 0.6 + amt * 0.5 * midP;
        // fresh curl-ish energy, drifting through the domain
        // bespoke MOTION: currentAngle steers the direction the fresh energy
        // drifts through the domain (0 = original heading)
        float cA = currentAngle * TAU;
        mat2 cRot = mat2(cos(cA), -sin(cA), sin(cA), cos(cA));
        vec2 np = q * 2.2 + cRot * vec2(tw * 0.13, -tw * 0.09);
        vec2 inject = vec2(fbm(np) - 0.5, fbm(np + vec2(11.7, 5.3)) - 0.5) * 2.0;
        // rotate injection over time so currents keep changing direction
        // (stirRate = how fast the currents swing around)
        float ra = tw * stirRate;
        inject = mat2(cos(ra), -sin(ra), sin(ra), cos(ra)) * inject;
        vec2 flow = old * 0.965 + inject * 0.05;
        flow = clamp(flow, -1.0, 1.0);
        gl_FragColor = vec4(flow * 0.5 + 0.5, 0.0, 1.0);
    } else if (PASSINDEX == 1) {
        // ── the skin ────────────────────────────────────────────────────
        vec2 flow = texture2D(csFlow, uv).rg * 2.0 - 1.0;
        float sc = cellScale;
        vec2 p = q * sc;
        p += flow * crawlDepth;                             // organic crawl (depth knob)
        // gentle global drift so the whole skin migrates (steered by currentAngle)
        float dA = currentAngle * TAU;
        mat2 dRot = mat2(cos(dA), -sin(dA), sin(dA), cos(dA));
        p += dRot * vec2(t * flowSpeed * 0.05, -t * flowSpeed * 0.03);

        // island field + independent web field
        float f1 = fbm(p * 1.25 + vec2(fbm(p * 0.9 + t * 0.02), fbm(p * 0.9 + 4.2)) * 0.6);
        float f2 = fbm(p * 2.1 + vec2(9.1, 2.7));

        // beat growth wave: threshold breathes outward from roaming spots
        float grow = amt * (0.045 * bassP + 0.03 * beatP);
        float th = 1.0 - coverage + breathDepth * sin(t * 0.3) - grow;

        float aa = 1.4 * sc / R.y;
        float island = smoothstep(th - aa, th + aa, f1);

        // channel narrowness: distance of f1 below threshold
        float gap = clamp((th - f1) / 0.14, 0.0, 1.0);
        // teal web: a crisp vein hugging the coastline, gated by second field
        float webBand = smoothstep(0.06, 0.16, gap) * smoothstep(0.72, 0.40, gap);
        float webGate = smoothstep(0.26, 0.44, f2);
        float web = webBand * webGate * webAmt;

        vec3 orange = colorA.rgb;
        vec3 teal   = colorB.rgb;
        vec3 blackC = vec3(0.03, 0.026, 0.026);

        // poster-flat islands: near-flat fill, thin bright lip only
        float lip = smoothstep(th, th + 0.03, f1) * smoothstep(th + 0.10, th + 0.045, f1);
        vec3 islandC = orange * (0.95 + 0.10 * smoothstep(th, th + 0.35, f1));
        islandC += orange * lip * 0.30;                     // bright coastline lip
        islandC += vec3(1.0, 0.8, 0.4) * lip * amt * 0.35 * beatP;
        // faint grit inside islands (HD micro-detail, kept flat)
        islandC *= 1.0 + (vnoise(p * 14.0) - 0.5) * 0.06;
        // level breathes the island glow
        islandC *= 1.0 + amt * 0.20 * clamp(audioLevel, 0.0, 1.0);

        vec3 webC = teal * (1.0 + 0.15 * vnoise(p * 6.0 + 3.0));
        webC += teal * amt * 0.6 * highP * webBand;         // highs light the web

        vec3 col = blackC;
        col = mix(col, webC, web);
        col = mix(col, islandC, island);

        col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.02;
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else if (PASSINDEX == 2) {
        vec3 col = texture2D(csScene, uv).rgb;
        vec3 prev = texture2D(abTrail, uv).rgb;
        float decay = 0.50 + 0.46 * trailAmt;
        col = max(col, prev * decay - 0.0045);
        col += (hash21(fc + fract(TIME) * 61.0) - 0.5) * 0.006;
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
