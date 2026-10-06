/*{
  "CATEGORIES": ["Generator", "Audio Reactive", "Minimal"],
  "DESCRIPTION": "Halo — ethereal monochrome light. Two soft shafts of pale light lean through slow fog around a breathing orb halo; bass swells the beams, the orb inhales with the music's energy, highs wake drifting dust motes inside the light. Church-light grayscale, nothing hard-edged.",
  "INPUTS": [
    {"NAME": "beamAngle",  "LABEL": "Beam Angle",   "TYPE": "float", "MIN": -0.8, "MAX": 0.8, "DEFAULT": 0.35},
    {"NAME": "beamWidth",  "LABEL": "Beam Width",   "TYPE": "float", "MIN": 0.05, "MAX": 0.5, "DEFAULT": 0.18},
    {"NAME": "orbSize",    "LABEL": "Halo Size",    "TYPE": "float", "MIN": 0.1,  "MAX": 0.8, "DEFAULT": 0.34},
    {"NAME": "fogAmount",  "LABEL": "Fog",          "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.5},
    {"NAME": "motes",      "LABEL": "Dust Motes",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.55},
    {"NAME": "breathSpeed","LABEL": "Breath Speed", "TYPE": "float", "MIN": 0.1,  "MAX": 2.0, "DEFAULT": 0.6},
    {"NAME": "invert",     "LABEL": "Invert",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.0}
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

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1.0, 0.0)), u.x),
               mix(hash21(i + vec2(0.0, 1.0)), hash21(i + vec2(1.0, 1.0)), u.x), u.y);
}

float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    mat2 r = mat2(0.8, 0.6, -0.6, 0.8);
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = r * p * 2.03 + vec2(11.7, 5.1);
        a *= 0.5;
    }
    return v;
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

    // Bounded phase offset, never TIME*drive (breath phase teleport hazard).
    float mt = TIME * breathSpeed * 0.7 + drive * 1.2;

    // The room breathes: one long inhale/exhale, and the music's loudness IS
    // most of the lung — pads and swells visibly fill the light.
    float breath = 0.5 + 0.5 * sin(mt * 0.7);
    breath = mix(breath, 1.0, 0.20 * drive + 0.42 * levelP);

    // Fog, two octave-warped layers sliding against each other — kept moving
    // briskly enough that the scene always has baseline motion (a too-still
    // frame makes any beat response read as a spike).
    float fog = fbm(p * 1.8 + vec2(mt * 0.11, -mt * 0.07));
    fog = fog * 0.65 + 0.35 * fbm(p * 3.4 - vec2(mt * 0.07, mt * 0.05) + fog);

    float v = 0.0;

    // Two light shafts leaning through the fog. Bass swells their throw.
    for (int i = 0; i < 2; i++) {
        float fi = float(i);
        float ang = beamAngle + (fi - 0.5) * 0.5 + 0.06 * sin(mt * 0.23 + fi * 2.1);
        vec2 dir = vec2(sin(ang), cos(ang));
        // Perpendicular distance from a beam anchored above the frame.
        vec2 origin = vec2((fi - 0.5) * 0.9, 1.2);
        vec2 rel = p - origin;
        float along = dot(rel, -dir);
        float side  = abs(dot(rel, vec2(-dir.y, dir.x)));
        float width = beamWidth * (1.0 + 0.35 * bassP) * (0.8 + 0.4 * breath);
        float shaft = exp(-pow(side / max(width, 1e-3), 2.0));
        shaft *= smoothstep(-0.2, 0.6, along);                    // fades in from source
        shaft *= 0.55 + 0.45 * fog * fogAmount * 2.0;             // fog textures the beam
        shaft *= 0.24 + 0.15 * bassP + 0.28 * levelP + 0.05 * beat * beat;
        v += shaft * 0.5;
    }

    // The orb halo: a soft core with a wide breathing corona. Mids ripple
    // its rim so the circle never reads as a hard shape.
    vec2 op = p - vec2(0.0, -0.05 + 0.03 * sin(mt * 0.4));
    float ripple = 1.0 + 0.05 * midP * sin(atan(op.y, op.x) * 6.0 + mt * 1.3);
    float r = length(op) / max(orbSize * ripple, 1e-3);
    float core   = exp(-r * r * 6.0) * (0.45 + 0.40 * breath);
    float corona = exp(-r * 1.8) * (0.18 + 0.30 * breath + 0.10 * bassP);
    v += core + corona;

    // Dust motes: sparse points that only exist inside the light, waking
    // with the highs — sparkle stays sparse and peripheral.
    float lightMask = clamp(v, 0.0, 1.0);
    vec2 mq = p * 9.0 + vec2(mt * 0.35, -mt * 0.5);
    vec2 mid_ = floor(mq);
    vec2 mrnd = hash22(mid_);
    if (mrnd.x > 0.55) {
        vec2 mpos = mid_ + 0.2 + 0.6 * hash22(mid_ + 7.7);
        float md = length(mq - mpos);
        float mote = smoothstep(0.09, 0.02, md);
        float wake = 0.25 + 0.75 * highP;
        float twinkle = 0.6 + 0.4 * sin(mt * 6.0 + mrnd.y * 6.2831);
        v += mote * motes * wake * twinkle * lightMask * 0.8;
    }

    // Ambient fog floor so silence glows faintly instead of dying to black.
    v += fog * fogAmount * 0.10 * (0.5 + 0.5 * drive);

    // Soft-knee tonemap keeps everything in the pale, airy range.
    v = 1.0 - exp(-max(v, 0.0) * 1.6);

    v = mix(v, 1.0 - v, clamp(invert, 0.0, 1.0));
    gl_FragColor = vec4(vec3(clamp(v, 0.0, 1.0)), 1.0);
}
