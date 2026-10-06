/*{
  "DESCRIPTION": "Grainz — a bit-plane shear automaton. A 6-bit-per-channel image lives in a persistent buffer; every frame each of its 18 bit-planes (3 channels × 6 bits) is sheared along rows and columns by its own golden-ratio drift, so the planes slide apart and interfere. The display shows how many bits are set per channel — a living halftone grain that tears, weaves and re-crystallises. Seed pattern, drift, plane count, colour mode and reseed are all editable; beats can re-seed, bass widens the drift, mids push the clock.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-11 — after the bit-plane shear buffer",
  "CATEGORIES": ["Generator", "Simulation", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "seedPattern", "LABEL": "Seed Pattern",    "TYPE": "float", "MIN": 0.0,  "MAX": 4.0,   "DEFAULT": 0.0,   "GROUP": "Seed" },
    { "NAME": "seedScale",   "LABEL": "Seed Scale",      "TYPE": "float", "MIN": 8.0,  "MAX": 200.0, "DEFAULT": 50.0,  "GROUP": "Seed" },
    { "NAME": "seedValue",   "LABEL": "Seed Value",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,   "DEFAULT": 1.0,   "GROUP": "Seed" },
    { "NAME": "reseed",      "LABEL": "Reseed (hold)",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,   "DEFAULT": 0.0,   "GROUP": "Seed" },
    { "NAME": "reseedBeat",  "LABEL": "Reseed On Beat",  "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,   "DEFAULT": 0.0,   "GROUP": "Seed" },
    { "NAME": "driftAmp",    "LABEL": "Drift Amount",    "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,   "DEFAULT": 1.0,   "GROUP": "Drift" },
    { "NAME": "driftSpeed",  "LABEL": "Drift Speed",     "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,   "DEFAULT": 1.0,   "GROUP": "Drift" },
    { "NAME": "driftFreq",   "LABEL": "Drift Frequency", "TYPE": "float", "MIN": 1.0,  "MAX": 4.0,   "DEFAULT": 1.0,   "GROUP": "Drift" },
    { "NAME": "driftOct",    "LABEL": "Drift Octaves",   "TYPE": "float", "MIN": 1.0,  "MAX": 7.0,   "DEFAULT": 7.0,   "GROUP": "Drift" },
    { "NAME": "shearSteps",  "LABEL": "Shear Steps",     "TYPE": "float", "MIN": 1.0,  "MAX": 8.0,   "DEFAULT": 6.0,   "GROUP": "Drift" },
    { "NAME": "planeSpread", "LABEL": "Plane Spread",    "TYPE": "float", "MIN": 0.0,  "MAX": 0.05,  "DEFAULT": 0.01,  "GROUP": "Drift" },
    { "NAME": "planes",      "LABEL": "Bit Planes",      "TYPE": "float", "MIN": 1.0,  "MAX": 6.0,   "DEFAULT": 6.0,   "GROUP": "Look" },
    { "NAME": "colorMode",   "LABEL": "Colour Mode",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,   "DEFAULT": 0.0,   "GROUP": "Look" },
    { "NAME": "contrast",    "LABEL": "Contrast",        "TYPE": "float", "MIN": 0.5,  "MAX": 2.5,   "DEFAULT": 1.0,   "GROUP": "Look" },
    { "NAME": "tintA",       "LABEL": "Tint Dark",       "TYPE": "color", "DEFAULT": [0.0, 0.0, 0.0, 1.0],   "GROUP": "Look" },
    { "NAME": "tintB",       "LABEL": "Tint Light",      "TYPE": "color", "DEFAULT": [1.0, 1.0, 1.0, 1.0],   "GROUP": "Look" },
    { "NAME": "tintMix",     "LABEL": "Tint Mix",        "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,   "DEFAULT": 0.0,   "GROUP": "Look" },
    { "NAME": "invert",      "LABEL": "Invert",          "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,   "DEFAULT": 0.0,   "GROUP": "Look" },
    { "NAME": "brightness",  "LABEL": "Brightness",      "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,   "DEFAULT": 1.0,   "GROUP": "Look" },
    { "NAME": "audioReact",  "LABEL": "Audio React",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,   "DEFAULT": 0.5,   "GROUP": "Audio Reactivity" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "gzState", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// GRAINZ
//   pass 0  gzState — persistent 6-bit-per-channel image, stored
//                     normalised (v/63). 6 bits (not 8) so the value
//                     survives imprecise half-float LINEAR readback with
//                     margin (0.5/63 ≫ sampler error) on every host. Each channel c, bit b: walk
//                     `shearSteps` alternating row/col shifts by an integer
//                     drift D(...) and copy that bit from the shifted
//                     source. All bit maths is done in float (floor/mod)
//                     so it runs on WebGL1 and GL 3.3 alike.
//   pass 1  display — popcount per channel (colorMode 0) or raw value
//                     (colorMode 1), tinted, contrast, invert.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define T   6.28318531
#define NBITS 6.0
#define VMAX  63.0

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

// bit b (0..7) of an integer-valued float v

float D(float c, vec2 t, int i) {
    float P = float(i) * (0.5 * sqrt(5.0) - 0.5);      // golden ratio / Weyl
    vec2 r = t * 9.0 * sin(P * T);                      // drift, golden angle
    t += P;
    vec2 C = vec2(c * T);                               // continuous for toroidal wrap
    C += 0.5 * sin(C + t * 0.1);
    float fq = floor(driftFreq + 0.5);                  // freq scale must be integer
    vec3 v = vec3(320.0 * driftAmp, fq, 0.14);
    int oct = int(driftOct);
    for (int k = 0; k < 7; k++) {
        if (k >= oct) break;
        float j = 0.1 + float(k) * 0.02;
        r += v.x * cos(v.y * C + v.z * t + j);
        v *= vec3(0.45, 2.0, -1.1);
    }
    r = floor(r + 0.5);
    return r.x - r.y;
}

float seedAt(vec2 q) {
    float sel = floor(seedPattern + 0.5);
    vec2 u = q / R;
    float s = seedScale;
    float m;
    if (sel < 0.5)      m = step(0.0, sin(q.x / s));                                  // stripes (original)
    else if (sel < 1.5) m = step(0.0, sin(q.x / s) * sin(q.y / s));                   // checker
    else if (sel < 2.5) m = step(0.0, sin(length(q - 0.5 * R) / s));                  // rings
    else if (sel < 3.5) m = step(0.5, hash21(floor(q / max(s * 0.15, 1.0))));         // block noise
    else                m = step(0.0, sin(q.x / s + 2.0 * sin(q.y / s)));             // waves
    return floor(m * VMAX * seedValue + 0.5);
}

float readVal(vec2 p, int c) {
    vec4 s = texture2D(gzState, (floor(p) + 0.5) / R);
    float v = (c == 0) ? s.r : (c == 1) ? s.g : s.b;
    return floor(v * VMAX + 0.5);
}

void passState() {
    vec2 q = gl_FragCoord.xy;
    float amt = audioReact;
    float clock = TIME * driftSpeed + amt * 0.5 * audioMidTime;
    vec2 tt = vec2(clock, clock - 1.0 / 60.0);
    float aBass = 1.0 + amt * 0.6 * pow(smoothstep(0.05, 0.85, audioBass), 1.4);

    bool doSeed = (FRAMEINDEX < 9) || (reseed > 0.5)
               || (reseedBeat > 0.5 && audioReact > 0.0 && clamp(audioBeatPulse, 0.0, 1.0) > 0.92);
    if (doSeed) {
        float m = seedAt(q);
        gl_FragColor = vec4(vec3(m / VMAX), 1.0);
        return;
    }

    int nsteps = int(shearSteps);
    vec3 result = vec3(0.0);
    for (int c = 0; c < 3; c++) {
        float acc = 0.0;
        float pw = 1.0;
        for (int b = 0; b < 6; b++) {
            vec2 p = q;
            float phase = (float(c) * NBITS + float(b)) * planeSpread;
            for (int i = 0; i < 8; i++) {
                if (i >= nsteps) break;
                if (i - (i / 2) * 2 == 0) {
                    // shift x by a function of normalised y
                    p.x = mod(p.x - D(p.y / R.y, tt + phase, i) * aBass, R.x);
                } else {
                    p.y = mod(p.y - D(p.x / R.x, tt + phase, i) * aBass, R.y);
                }
            }
            float src = readVal(p, c);
            float bit = mod(floor(src / pw), 2.0);
            acc += bit * pw;
            pw *= 2.0;
        }
        if (c == 0) result.r = acc; else if (c == 1) result.g = acc; else result.b = acc;
    }
    gl_FragColor = vec4(result / VMAX, 1.0);
}

void passDisplay() {
    vec2 q = gl_FragCoord.xy;
    vec4 s = texture2D(gzState, (floor(q) + 0.5) / R);
    vec3 v = floor(s.rgb * VMAX + 0.5);
    float np = floor(planes + 0.5);
    vec3 pop = vec3(0.0);
    float pw = 1.0;
    for (int b = 0; b < 6; b++) {
        if (float(b) < np) pop += mod(floor(v / pw), 2.0);
        pw *= 2.0;
    }
    pop /= np;
    vec3 raw = v / VMAX;
    // highs pull the display toward raw bit values (colour separation),
    // level pushes contrast — both soft-kneed, idle floor = plain popcount
    float hi = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float lv = clamp(audioLevel, 0.0, 1.0);
    vec3 col = mix(pop, raw, clamp(colorMode + audioReact * 0.7 * hi, 0.0, 1.0));
    col = mix(vec3(0.5), col, contrast * (1.0 + audioReact * 0.6 * lv));
    col = clamp(col, 0.0, 1.0);
    col = mix(col, 1.0 - col, invert);
    float l = dot(col, vec3(0.299, 0.587, 0.114));
    vec3 tinted = mix(tintA.rgb, tintB.rgb, l);
    col = mix(col, tinted, tintMix);
    col *= brightness * (1.0 + audioReact * 0.08 * clamp(audioLevel, 0.0, 1.0));
    col = clamp(col, 0.0, 1.0);
    col = mix(col, col * col * (3.0 - 2.0 * col), 0.35);
    float alphaOut = transparentBg ? smoothstep(0.02, 0.15, dot(col, vec3(0.299, 0.587, 0.114))) : 1.0;
    gl_FragColor = vec4(col, alphaOut);
}

void main() {
    if (PASSINDEX == 0) passState();
    else passDisplay();
}
