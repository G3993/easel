/*{
  "CATEGORIES": ["Generator", "Audio Reactive", "Minimal", "Simulation"],
  "DESCRIPTION": "Sumi-e ink in water — monochrome fluid. White wisps of ink bloom and advect through black water via domain-warped flow; bass drops fresh ink at a wandering brush point, mids stir the turbulence, highs raise faint paper-fiber shimmer. Pure grayscale.",
  "INPUTS": [
    {"NAME": "inkScale",  "LABEL": "Ink Scale",   "TYPE": "float", "MIN": 1.2, "MAX": 6.0, "DEFAULT": 2.6},
    {"NAME": "flowSpeed", "LABEL": "Flow Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.55},
    {"NAME": "inkAmount", "LABEL": "Ink Amount",  "TYPE": "float", "MIN": 0.2, "MAX": 1.0, "DEFAULT": 0.62},
    {"NAME": "contrast",  "LABEL": "Contrast",    "TYPE": "float", "MIN": 0.6, "MAX": 3.0, "DEFAULT": 1.45},
    {"NAME": "wispDetail","LABEL": "Wisp Detail", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6},
    {"NAME": "invert",    "LABEL": "Invert",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0},
    {"NAME": "grain",     "LABEL": "Paper Grain", "TYPE": "float", "MIN": 0.0, "MAX": 0.06, "DEFAULT": 0.009}
  ]
}*/

float hash21(vec2 p) {
    p = fract(p * vec2(234.34, 435.345));
    p += dot(p, p + 34.23);
    return fract(p.x * p.y);
}

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    mat2 r = mat2(0.8, 0.6, -0.6, 0.8);
    for (int i = 0; i < 5; i++) {
        v += a * vnoise(p);
        p = r * p * 2.03 + vec2(11.7, 5.1);
        a *= 0.5;
    }
    return v;
}

float knee(float x, float lo, float hi) { return clamp(smoothstep(lo, hi, x), 0.0, 1.0); }

void main() {
    vec2 uv = isf_FragNormCoord;
    vec2 p  = (gl_FragCoord.xy - 0.5 * RENDERSIZE) / min(RENDERSIZE.x, RENDERSIZE.y);

    // Soft-knee audio conditioning (playbook standard snippet).
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP  = pow(knee(audioMid,  0.08, 0.88), 1.3);
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float drive = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9);
    float levelP = knee(audioLevel, 0.03, 0.8);
    float beat  = clamp(audioBeatPulse, 0.0, 1.0);

    // Water keeps drifting in silence; music leans the current forward as a
    // BOUNDED phase offset — never TIME*drive, which teleports the whole
    // field whenever energy moves (reads as choppy on pumping music).
    float mt = TIME * flowSpeed * 0.85;

    // Audio NEVER touches the fbm arguments: domain-warped noise re-rolls the
    // entire texture on any parametric jitter, which reads as a full-frame
    // strobe. Audio lives in tone (brightness/threshold/rim) and the local
    // additive splat below instead.
    vec2 pw = p;

    // Domain-warped flow: q advects r advects the ink density (iq-style).
    // Mids stir the second warp stage — turbulence, not position (law 2/3).
    vec2 q = vec2(fbm(pw * inkScale + vec2(0.0, mt * 0.62)),
                  fbm(pw * inkScale + vec2(5.2, 1.3) - vec2(mt * 0.44, 0.0)));
    float stir = 1.0 + 0.35 * wispDetail;
    vec2 r = vec2(fbm(pw * inkScale + 2.2 * q * stir + vec2(1.7, 9.2) + vec2(0.0, mt * 0.26)),
                  fbm(pw * inkScale + 2.2 * q * stir + vec2(8.3, 2.8) - vec2(mt * 0.19, 0.0)));

    float ink = fbm(pw * inkScale + 3.1 * r);

    // Wandering brush: a slow lissajous point where bass drops fresh ink.
    // Impulse in, physics out — the splat joins the same advected field.
    vec2 brush = 0.42 * vec2(sin(mt * 0.31 + 1.7) + 0.4 * sin(mt * 0.113),
                             cos(mt * 0.24) + 0.4 * cos(mt * 0.147 + 0.6));
    // The splat is a TONAL bloom added after the thresholds — injected into
    // the ink field it crossed the binarizing smoothstep and flipped a big
    // region in one frame. A soft gray glow carries the same correlation.
    float splat = exp(-dot(p - brush, p - brush) * 22.0);

    // Ink body: mid-tones carved into wisps; edges pool darker like real sumi.
    // Loudness widens the wash a touch — slow tracks breathe their coverage.
    float body = smoothstep(0.42 - 0.16 * inkAmount - 0.07 * levelP, 0.74, ink);
    float veil = smoothstep(0.30, 0.95, ink) * (0.26 + 0.24 * levelP); // thin wash breathes with loudness
    float rim  = body * (1.0 - smoothstep(0.0, 0.22, abs(ink - 0.60))) * (0.35 + 0.30 * midP);

    float v = clamp(veil + body * 0.85 + rim * wispDetail, 0.0, 1.0);

    // Highs: faint paper-fiber shimmer riding only on the lit wash.
    float fiber = vnoise(pw * 90.0 + vec2(0.0, mt * 3.0));
    v += highP * 0.18 * fiber * veil;

    // Bass blooms the brush point as light, not as field structure.
    v += splat * (0.50 * bassP + 0.06 * beat * beat) * (0.5 + 0.5 * veil);

    // Grayscale finishing: contrast around mid-gray, gentle energy lift.
    v = pow(clamp(v, 0.0, 1.0), 1.0 / max(contrast, 0.001));
    v *= 0.72 + 0.08 * drive + 0.22 * levelP;

    float g = hash21(gl_FragCoord.xy + vec2(mod(float(FRAMEINDEX), 731.0)));
    v += (g - 0.5) * grain;

    v = clamp(v, 0.0, 1.0);
    v = mix(v, 1.0 - v, clamp(invert, 0.0, 1.0));
    gl_FragColor = vec4(vec3(v), 1.0);
}
