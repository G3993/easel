/*{
  "CATEGORIES": ["Generator", "Audio Reactive", "Minimal"],
  "DESCRIPTION": "Orbits — abstract monochrome line-work. Hairline concentric rings and orbiting arc fragments around a quiet center; every kick births a shockwave ring that travels outward and fades, each arc spins at the speed of its own spectral band, highs spark the arc tips. White lines on black, drafting-table clean.",
  "INPUTS": [
    {"NAME": "ringCount",  "LABEL": "Rings",        "TYPE": "float", "MIN": 3.0,  "MAX": 12.0, "DEFAULT": 6.0},
    {"NAME": "arcCount",   "LABEL": "Arcs",         "TYPE": "float", "MIN": 2.0,  "MAX": 8.0,  "DEFAULT": 5.0},
    {"NAME": "lineWeight", "LABEL": "Line Weight",  "TYPE": "float", "MIN": 0.4,  "MAX": 3.0,  "DEFAULT": 1.0},
    {"NAME": "spinSpeed",  "LABEL": "Spin Speed",   "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.6},
    {"NAME": "wavePower",  "LABEL": "Kick Wave",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.75},
    {"NAME": "centerGlow", "LABEL": "Center Glow",  "TYPE": "float", "MIN": 0.0,  "MAX": 0.5,  "DEFAULT": 0.12},
    {"NAME": "invert",     "LABEL": "Invert",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0}
  ]
}*/

float hash21(vec2 p) {
    p = fract(p * vec2(234.34, 435.345));
    p += dot(p, p + 34.23);
    return fract(p.x * p.y);
}

float knee(float x, float lo, float hi) { return clamp(smoothstep(lo, hi, x), 0.0, 1.0); }

float fftLog(float t) { return texture2D(audioFFT, vec2(pow(clamp(t, 0.0, 1.0), 2.2) * 0.5, 0.5)).r; }

void main() {
    vec2 p = (gl_FragCoord.xy - 0.5 * RENDERSIZE) / min(RENDERSIZE.x, RENDERSIZE.y);

    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float drive = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9);
    float levelP = knee(audioLevel, 0.03, 0.8);
    float beat  = clamp(audioBeatPulse, 0.0, 1.0);
    float punch = pow(clamp(audioPunch, 0.0, 1.0), 1.5);

    // Bounded phase offset, never TIME*drive — mt spins the arcs, so a
    // drive-scaled clock teleports every arc on each energy change.
    float mt = TIME * 0.6 + drive * 1.5;

    float r = length(p);
    float ang = atan(p.y, p.x);
    float px = 1.5 / min(RENDERSIZE.x, RENDERSIZE.y);   // one-ish pixel
    float lw = px * (2.0 + 3.0 * lineWeight);

    float v = 0.0;

    // Concentric hairline rings; each breathes a few percent against its
    // neighbors (per-ring phase lag), bass adds one shared, gentle swell.
    float N = floor(ringCount);
    for (int i = 0; i < 12; i++) {
        if (float(i) >= N) break;
        float fi = float(i);
        float h = hash21(vec2(fi, 3.7));
        float base = mix(0.12, 0.85, (fi + 0.5) / N);
        float breatheR = base * (1.0 + 0.020 * sin(mt * 0.8 + h * 6.2831)
                                + 0.030 * bassP);
        float d = abs(r - breatheR);
        v += smoothstep(lw, lw * 0.25, d) * mix(0.18, 0.42, h);
    }

    // Orbiting arc fragments: each one listens to its own spectral band and
    // spins at that band's speed — frequency mapped to space (law 3).
    float M = floor(arcCount);
    for (int i = 0; i < 8; i++) {
        if (float(i) >= M) break;
        float fi = float(i);
        float h  = hash21(vec2(fi, 9.1));
        float h2 = hash21(vec2(fi, 27.3));
        float band = fftLog(mix(0.1, 0.9, h));
        float bandP = knee(band, 0.15, 0.85);

        float orbitR = mix(0.18, 0.80, h);
        float speed = spinSpeed * (0.15 + 0.85 * h2) * (0.4 + 1.1 * bandP)
                    * (h2 > 0.5 ? 1.0 : -1.0);
        float a0 = h * 6.2831 + mt * speed;
        float span = mix(0.5, 1.8, h2) * (0.8 + 0.35 * bandP);

        // Angular distance into the arc.
        float rel = mod(ang - a0 + 3.14159, 6.2831) - 3.14159;
        float inArc = smoothstep(span * 0.5, span * 0.5 - 0.05, abs(rel));
        float d = abs(r - orbitR);
        float arc = smoothstep(lw * 1.4, lw * 0.3, d) * inArc;
        v += arc * (0.35 + 0.45 * bandP);

        // Arc tip spark: highs light a point at the leading edge only.
        vec2 tip = vec2(cos(a0 + span * 0.5 * sign(speed)),
                        sin(a0 + span * 0.5 * sign(speed))) * orbitR;
        float td = length(p - tip);
        v += smoothstep(0.025, 0.004, td) * highP * 0.9;
    }

    // Kick shockwave (golden technique #1): age comes off the beat envelope,
    // ring travels out as the pulse decays, brightness = pulse² × punch.
    float waveAge = 1.0 - beat;
    float waveR = mix(0.05, 1.25, waveAge);
    float wave = exp(-pow((r - waveR) * 26.0, 2.0));
    // Fade the ring in over the first slice of its life so its birth is a
    // bloom, not a single-frame step (the instant birth read as choppy).
    float birth = smoothstep(0.0, 0.22, waveAge);
    v += wave * beat * beat * birth * (0.35 + 0.65 * punch) * wavePower;

    // Quiet center: a soft anchor glow, breathing with the track's energy.
    v += exp(-r * r * 18.0) * centerGlow * (0.6 + 0.4 * drive);

    // Whole drawing lifts with loudness — never fully dark, never loud.
    v = clamp(v * (0.72 + 0.12 * drive + 0.16 * levelP), 0.0, 1.0);

    v = mix(v, 1.0 - v, clamp(invert, 0.0, 1.0));
    gl_FragColor = vec4(vec3(v), 1.0);
}
