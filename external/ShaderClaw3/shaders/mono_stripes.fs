/*{
  "CATEGORIES": ["Generator", "Audio Reactive", "Minimal"],
  "DESCRIPTION": "Riley Current — op-art monochrome stripes. Crisp parallel black/white bands bend through a slow travelling lens; bass bulges the lens, beats flip stripe phase in an eased snap, mids raise a moiré shimmer where two stripe fields interfere. Hard grayscale, no color ever.",
  "INPUTS": [
    {"NAME": "stripeFreq", "LABEL": "Stripe Count",  "TYPE": "float", "MIN": 6.0,  "MAX": 60.0, "DEFAULT": 22.0},
    {"NAME": "bendAmount", "LABEL": "Bend",          "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.55},
    {"NAME": "rotation",   "LABEL": "Angle",         "TYPE": "float", "MIN": 0.0,  "MAX": 3.1416, "DEFAULT": 1.15},
    {"NAME": "moire",      "LABEL": "Moire Shimmer", "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.35},
    {"NAME": "softness",   "LABEL": "Edge Softness", "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.25},
    {"NAME": "driftSpeed", "LABEL": "Drift Speed",   "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.5},
    {"NAME": "invert",     "LABEL": "Invert",        "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0}
  ]
}*/

float fftLog(float t) { return texture2D(audioFFT, vec2(pow(clamp(t, 0.0, 1.0), 2.2) * 0.5, 0.5)).r; }

float hash21(vec2 p) {
    p = fract(p * vec2(234.34, 435.345));
    p += dot(p, p + 34.23);
    return fract(p.x * p.y);
}

float knee(float x, float lo, float hi) { return clamp(smoothstep(lo, hi, x), 0.0, 1.0); }

void main() {
    vec2 p = (gl_FragCoord.xy - 0.5 * RENDERSIZE) / min(RENDERSIZE.x, RENDERSIZE.y);

    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP  = pow(knee(audioMid,  0.08, 0.88), 1.3);
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float drive = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9);
    float levelP = knee(audioLevel, 0.03, 0.8);
    float beat  = clamp(audioBeatPulse, 0.0, 1.0);

    // Bounded phase offset, never TIME*drive (whole-field teleport hazard).
    float mt = TIME * driftSpeed * 0.85 + drive * 0.5;

    float cs = cos(rotation), sn = sin(rotation);
    vec2 rp = mat2(cs, -sn, sn, cs) * p;

    // Travelling lens: a slow orbiting center that bulges the stripe field.
    // Bass deepens the bulge — big/central/global belongs to bass (law 3).
    vec2 lens = 0.45 * vec2(sin(mt * 0.27 + 1.3), cos(mt * 0.21));
    vec2 dl = p - lens;
    float r2 = dot(dl, dl);
    float bulge = exp(-r2 * 3.2) * bendAmount * (0.85 + 0.10 * bassP);

    // The lens displaces the stripe coordinate radially, like glass over print.
    vec2 disp = dl * bulge * 0.85;
    vec2 sp = rp - mat2(cs, -sn, sn, cs) * disp;

    // Beat flips the stripes' phase — structure on beats — but the flip lives
    // inside the lens and dies off with distance, a ripple instead of a
    // whole-frame strobe (the full-field flip read as choppy in eval).
    float flipZone = exp(-r2 * 2.2);
    float phaseFlip = 0.55 * beat * beat * beat * flipZone;

    // A second, breath-slow wave keeps silence alive.
    float baseWave = 0.10 * sin(sp.y * 2.4 + mt * 0.5);

    float stripes = sin(sp.x * stripeFreq + baseWave * stripeFreq + phaseFlip + mt * 0.4);

    // Crisp bands with resolution-aware anti-aliasing. Loudness fattens the
    // white bands (duty cycle) — a smooth, whole-field coverage response
    // that tracks the music without any single-frame step.
    float aa = stripeFreq / min(RENDERSIZE.x, RENDERSIZE.y) * 2.5 + softness * 0.35;
    float duty = -0.11 + 0.08 * levelP;
    float band = smoothstep(-aa, aa, stripes + duty);

    // Moiré layer: a second stripe field at a hair's rotation; mids fade it in.
    float ma = 0.06 + 0.03 * sin(mt * 0.4);
    vec2 mp2 = mat2(cos(ma), -sin(ma), sin(ma), cos(ma)) * sp;
    float stripes2 = sin(mp2.x * stripeFreq * 1.03 + phaseFlip);
    float band2 = smoothstep(-aa, aa, stripes2 + duty);
    float interference = abs(band - band2);
    band = mix(band, band * (1.0 - interference * 0.85), moire * (0.35 + 0.25 * sin(mt * 0.31)));

    // Each white stripe listens to its own slice of the spectrum: only a few
    // stripes brighten at a time, each a local tonal change — sparse response
    // instead of a whole-field step (per-band cells, playbook technique #2).
    float stripeArg = sp.x * stripeFreq + baseWave * stripeFreq + mt * 0.4;
    float stripeIdx = floor(stripeArg / 3.14159);
    float sh = hash21(vec2(stripeIdx, 4.2));
    float bandP = knee(fftLog(mix(0.18, 0.95, sh)), 0.2, 0.9);
    band *= 0.84 + 0.18 * bandP;

    // Highs: a knife-thin bright line at each stripe edge, sparse and quick.
    float edge = 1.0 - smoothstep(0.0, aa * 2.0, abs(stripes));
    band += edge * highP * 0.35;

    // Grayscale finish; the field lifts with loudness so slow music reads too.
    float v = clamp(band, 0.0, 1.0) * (0.58 + 0.08 * drive + 0.34 * levelP);

    v = mix(v, 1.0 - v, clamp(invert, 0.0, 1.0));
    gl_FragColor = vec4(vec3(v), 1.0);
}
