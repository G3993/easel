/*{
  "CATEGORIES": ["Generator", "Audio Reactive", "Minimal"],
  "DESCRIPTION": "Dust — monochrome particle field with depth. Four parallax layers of drifting motes on black: near ones are soft white bokeh discs, far ones faint gray specks. Each kick sends a shockwave ring that lights particles as it passes; highs twinkle a sparse subset. Grayscale only.",
  "INPUTS": [
    {"NAME": "density",    "LABEL": "Density",      "TYPE": "float", "MIN": 0.3, "MAX": 1.0, "DEFAULT": 0.65},
    {"NAME": "driftSpeed", "LABEL": "Drift Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 2.0, "DEFAULT": 0.5},
    {"NAME": "bokehSize",  "LABEL": "Bokeh Size",   "TYPE": "float", "MIN": 0.5, "MAX": 2.0, "DEFAULT": 1.0},
    {"NAME": "ringPower",  "LABEL": "Kick Ring",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7},
    {"NAME": "twinkle",    "LABEL": "Twinkle",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6},
    {"NAME": "haze",       "LABEL": "Haze",         "TYPE": "float", "MIN": 0.0, "MAX": 0.3, "DEFAULT": 0.06},
    {"NAME": "invert",     "LABEL": "Invert",       "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0}
  ]
}*/

float hash21(vec2 p) {
    p = fract(p * vec2(234.34, 435.345));
    p += dot(p, p + 34.23);
    return fract(p.x * p.y);
}

vec2 hash22(vec2 p) {
    float n = hash21(p);
    return vec2(n, hash21(p + n + 17.31));
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
    float punch = pow(clamp(audioPunch, 0.0, 1.0), 1.5);

    // Bounded phase offset, never TIME*drive (whole-field teleport hazard).
    float mt = TIME * driftSpeed * 0.8 + drive * 2.0;

    // Kick shockwave: the ring's age comes straight off the beat envelope,
    // so it births at the hit and travels outward as the pulse decays.
    float ringAge = 1.0 - beat;
    float ringR   = mix(0.06, 1.25, ringAge);
    float ringGain = beat * beat * (0.4 + 0.6 * punch) * ringPower;

    float v = 0.0;

    // Four parallax shells, far to near.
    for (int layer = 0; layer < 4; layer++) {
        float fl = float(layer);
        float depth = (fl + 1.0) / 4.0;                 // 0.25 far .. 1.0 near
        float cell = mix(0.10, 0.26, depth) / max(density, 0.05);

        // Each shell drifts at its own speed and heading — parallax.
        vec2 off = vec2(mt * (0.02 + 0.06 * depth), mt * (0.013 + 0.03 * depth) * (fl > 1.5 ? -1.0 : 1.0));
        vec2 q = p * (1.0 + 0.15 * fl) + off + fl * 3.7;

        vec2 gid = floor(q / cell);
        vec2 rnd = hash22(gid + fl * 57.3);
        if (rnd.x > density * 0.9 + 0.1) continue;

        // Mote floats inside its cell; mids add gentle turbulence to the sway.
        float lag = rnd.y * 6.2831;
        vec2 sway = vec2(sin(mt * 1.3 + lag), cos(mt * 1.07 + lag * 1.7))
                  * cell * (0.18 + 0.10 * midP);
        vec2 mpos = (gid + 0.25 + 0.5 * hash22(gid + fl * 13.1)) * cell + sway;
        float d = length(q - mpos);

        // Bokeh disc: soft edge scales with depth; near = big + bright.
        float rad = cell * mix(0.06, 0.20, depth) * bokehSize;
        float disc = smoothstep(rad, rad * 0.25, d);
        float lum = mix(0.18, 0.95, depth * depth);

        // Twinkle: sparse subset only (law: sparkle stays sparse).
        float tw = 1.0;
        if (rnd.y > 0.86) {
            tw += twinkle * highP * 1.4 * (0.5 + 0.5 * sin(TIME * 9.0 + lag * 5.0));
        }

        // The shockwave lights motes as it sweeps their radius (world space).
        vec2 wpos = mpos - off - fl * 3.7;               // back to screen space
        float ringHit = exp(-pow((length(wpos) - ringR) * 9.0, 2.0)) * ringGain;

        v += disc * lum * tw * (0.55 + 0.15 * drive + 0.35 * levelP) + disc * ringHit * 1.6;
    }

    // The ring itself: a whisper of a line so the wave reads between motes.
    float ringLine = exp(-pow((length(p) - ringR) * 22.0, 2.0)) * ringGain * 0.30;

    // Depth haze: faint gradient so black isn't ever a dead void.
    float bg = haze * (1.0 - length(p) * 0.8) * (0.6 + 0.4 * drive) + haze * 0.4 * bassP;

    v = clamp(v + ringLine + bg, 0.0, 1.0);
    v = mix(v, 1.0 - v, clamp(invert, 0.0, 1.0));
    gl_FragColor = vec4(vec3(v), 1.0);
}
