/*{
  "DESCRIPTION": "Electricity — the Earth–ionosphere cavity as a living instrument. Five Schumann resonance shells (7.83 Hz and its harmonics, each fed by its own FFT band) undulate as hairline standing waves around the planet's curve. Thunder filaments strike from the ionosphere to the ground on a hashed schedule — beats call down extra strikes — and every strike rings the earth: expanding surface ripples that persist for seconds. Bass swells the ground waves, mids turbulate the filament wander, highs crackle sparks at the strike points. Idle: calm resonance shells breathing over a quiet dark earth.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "boltColor",
      "LABEL": "Bolt Core",
      "TYPE": "color",
      "DEFAULT": [0.88, 0.94, 1.0, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "energyColor",
      "LABEL": "Energy Field",
      "TYPE": "color",
      "DEFAULT": [0.30, 0.52, 1.0, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "earthColor",
      "LABEL": "Earth Ripple",
      "TYPE": "color",
      "DEFAULT": [0.22, 0.80, 0.62, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "shellDetail",
      "LABEL": "Resonance Detail",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 1.8,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "strikeRate",
      "LABEL": "Strike Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2.5,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "rippleAmount",
      "LABEL": "Earth Ripples",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.35,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ELECTRICITY — Schumann cavity, all analytic, no persistence.
// Geometry: planet center below the frame; r-R = height above the surface,
// arc angle * R = distance along the ground. Five resonance shells sit at
// fixed heights in the cavity; shell n carries mode-n azimuthal standing
// waves at the real Schumann frequency ratios (7.83/14.3/20.8/27.3/33.8 Hz,
// scaled to visual time) with amplitude fed by its own log-FFT band.
// Strikes: 4 hashed slots; each cycle = one bolt (fast flash, eased decay)
// whose surface ripple then lives for the REST of the cycle — a beat stays
// visible for seconds (law 4). Beat pulse advances the strike clock, so loud
// music genuinely strikes more often; silence keeps a slow idle schedule.

#define TAU 6.2831853

float hash11(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}
float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}
float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

// 1D value noise, 3 octaves — the bolt's wander line
float vnoise(float x, float seed) {
    float i = floor(x), f = fract(x);
    float u = f * f * (3.0 - 2.0 * f);
    return mix(hash11(i + seed), hash11(i + 1.0 + seed), u);
}
float wander(float x, float seed) {
    float w = 0.0;
    w += (vnoise(x * 3.0,  seed) - 0.5) * 0.60;
    w += (vnoise(x * 7.0,  seed + 19.7) - 0.5) * 0.28;
    w += (vnoise(x * 17.0, seed + 47.3) - 0.5) * 0.12;
    return w;
}

// log-frequency FFT lookup — musical energy lives in the low bins
float fftLog(float t) { return texture2D(audioFFT, vec2(pow(t, 2.2) * 0.5, 0.5)).r; }

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    float asp = RENDERSIZE.x / RENDERSIZE.y;
    vec2 p = vec2(uv.x * asp, uv.y);
    float px = 1.0 / RENDERSIZE.y;                 // one pixel, p-space

    // ---- audio conditioning (smooth, floored — never raw) ----
    float amt   = clamp(audioReact, 0.0, 1.0);
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midL  = clamp(audioMid, 0.0, 1.0);
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float pulse = clamp(audioBeatPulse, 0.0, 1.0);
    float drive = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9);

    // ---- cavity geometry ----
    vec2 C = vec2(0.5 * asp, -0.62);
    float R = 0.80;
    vec2 d = p - C;
    float r = length(d);
    float h = r - R;                                // height above surface
    float ang = atan(d.x, d.y);                     // 0 = straight up
    float s = ang * R;                              // ground arc-length
    float hTop = 0.92;                              // ionosphere height

    // ---- night sky + dark earth ----
    vec3 sky = mix(vec3(0.010, 0.014, 0.030), vec3(0.028, 0.030, 0.075),
                   knee(h, 0.0, 1.1));
    vec3 ground = vec3(0.015, 0.022, 0.026)
                * (1.0 - 0.65 * knee(-h, 0.0, 0.30));
    vec3 col = (h > 0.0) ? sky : ground;
    // faint horizon limb light
    col += energyColor.rgb * 0.055 * exp(-h * h / 0.004) * drive;

    // ---- Schumann resonance shells (frequency-based standing waves) ----
    // heights and mode ratios vs the 7.83 Hz fundamental: 1.83/2.66/3.49/4.32
    float shellGlow = 0.0;
    float wLine = max(0.0016 * shellDetail, px * 1.2);
    for (int n = 0; n < 5; n++) {
        float fn = float(n);
        float rat = 1.0 + fn * 0.83;                // ~Schumann harmonic ratios
        float hs = 0.14 + 0.165 * fn;               // shell height in cavity
        // this shell's band energy from the log FFT (low modes = low bins)
        float band = fftLog(0.06 + 0.17 * fn);
        float bandP = pow(knee(band, 0.04, 0.85), 1.3);
        // azimuthal standing wave: (n+2) lobes around the arc, breathing at
        // the mode's frequency ratio (slow visual clock, phase-wrapped)
        float phase = fract(TIME * 0.028 * rat) * TAU;
        float wave = cos(ang * (fn + 2.0) * 3.0 - phase)
                   * cos(phase * 0.5 + fn * 1.7);
        // second harmonic shimmer; mids deepen the motion
        wave += 0.35 * cos(ang * (fn + 2.0) * 6.0 + phase * 1.7)
              * (0.4 + amt * 0.6 * midL);
        float ampl = 0.012 + 0.030 * (0.30 * drive + amt * 0.70 * bandP);
        float dist = abs(h - (hs + wave * ampl));
        float line = exp(-dist * dist / (wLine * wLine));
        // soft aura under each line so the cavity reads as a field
        line += 0.22 * exp(-dist * dist / (wLine * wLine * 90.0));
        // low modes strongest; each shell swells with its own band
        float wgt = (1.0 - 0.13 * fn) * (0.35 + 0.65 * (0.4 * drive + amt * 0.6 * bandP));
        shellGlow += line * wgt;
    }
    col += energyColor.rgb * shellGlow * 0.44;

    // ---- strikes: 4 slots, hashed schedule, beat-advanced clock ----
    float strikeClock = TIME * 0.28 * strikeRate + amt * 0.55 * audioTime;
    float visArc = 0.62;                            // visible half-angle
    vec3 boltAcc = vec3(0.0);
    for (int i = 0; i < 4; i++) {
        float fi = float(i);
        float period = 1.6 + 1.9 * hash11(fi * 7.7 + 1.3);  // slot cadence
        float c = strikeClock / period + hash11(fi * 3.1) * 7.0;
        float cyc = floor(c);
        float ph = fract(c);
        float age = ph * period * 3.2;              // approx seconds since strike

        // strike ground point for this cycle
        float hh = hash21(vec2(cyc, fi * 13.7));
        float a0 = (hh - 0.5) * 2.0 * visArc * 0.85;
        vec2 strikePos = C + vec2(sin(a0), cos(a0)) * R;
        float s0 = a0 * R;

        // ---- bolt filament (alive only at the start of the cycle) ----
        float env = exp(-age * 6.0) * knee(ph, 0.0, 0.012);
        env *= 0.75 + 0.25 * cos(age * 34.0 + hh * TAU);     // gentle flutter
        if (env > 0.004 && h > -0.02 && h < hTop + 0.1) {
            float u = clamp(h / hTop, 0.0, 1.0);             // 0 ground, 1 top
            float seed = cyc * 9.17 + fi * 31.7;
            // wander grows with altitude; mids add live turbulence
            float wob = wander(u * 3.0 + age * 0.35, seed);
            float spread = (0.05 + 0.30 * u) * (1.0 + amt * 0.45 * midL);
            float pathS = s0 + wob * spread;
            float dd = abs(s - pathS);
            float wCore = max(0.0018, px * 1.3);
            float core = exp(-dd * dd / (wCore * wCore));
            float halo = exp(-dd * dd / (wCore * wCore * 160.0));
            // one branch, forking above mid-height
            float wob2 = wander(u * 4.8 + 4.4, seed + 77.7);
            float dB = abs(s - (s0 + wob * spread * 0.4 + wob2 * spread * (u - 0.35)));
            float branch = exp(-dB * dB / (wCore * wCore)) * knee(u, 0.35, 0.55) * 0.5;
            float beatKick = 1.0 + amt * 1.6 * pulse * pulse;
            boltAcc += (boltColor.rgb * (core + branch)
                      + energyColor.rgb * halo * 0.30) * env * beatKick;
            // impact flash + sparse high-frequency sparks at the strike point
            float rimp = length(p - strikePos);
            boltAcc += boltColor.rgb * exp(-rimp * rimp / 0.0012) * env * 0.8;
            float spark = hash21(floor(p * 240.0) + cyc + floor(age * 24.0));
            spark = step(0.985, spark) * exp(-rimp * rimp / 0.010);
            boltAcc += boltColor.rgb * spark * env * (0.25 + amt * 1.2 * highP);
        }

        // ---- earth ripple: lives for the whole rest of the cycle ----
        float rd = length(p - strikePos);
        float rippleAge = age + 0.06;
        float front = rippleAge * 0.42;                       // wavefront radius
        float ring = cos((rd - front) * 46.0);
        ring *= ring; ring *= ring;                           // crisp crest lines
        float envelope = exp(-(rd - front) * (rd - front) / 0.028)
                       * exp(-rippleAge * 0.85)
                       * exp(-rd * 1.15);
        // ripples belong to the earth: strongest just below the surface
        float groundMask = knee(-h, -0.015, 0.02) * (1.0 - knee(-h, 0.10, 0.42));
        float swell = 1.0 + amt * (0.9 * bassP + 0.5 * pulse);
        boltAcc += earthColor.rgb * ring * envelope * groundMask
                 * rippleAmount * swell * 0.55;
        // the surface itself lights up as the wave passes (limb shimmer)
        float limb = exp(-h * h / 0.0016) * exp(-(rd - front) * (rd - front) / 0.02);
        boltAcc += mix(earthColor.rgb, energyColor.rgb, 0.5) * limb
                 * exp(-rippleAge * 0.9) * rippleAmount * swell * 0.30;
    }
    col += boltAcc;

    // ---- global breath: bass gently lifts the whole cavity field ----
    col *= 1.0 + amt * 0.18 * bassP;

    // vignette + fine static grain (static seed keeps idle frames quiet)
    vec2 vd = (uv - 0.5) * vec2(1.0, 1.15);
    col *= 1.0 - 0.52 * dot(vd, vd);
    float grain = hash21(uv * RENDERSIZE.xy + 7.1);
    col += (grain - 0.5) * 0.010;

    col = col * brightness / (1.0 + 0.65 * max(brightness - 1.0, 0.0) * col);
    gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
}
