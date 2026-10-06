/*{
  "DESCRIPTION": "Open Flame — a living bed of three flame tongues built from layered turbulent noise advected upward: blue-white combustion core, orange body, red licking tips over a dim hearth glow. Bass and energy make the flames leap taller and more vigorous, mids roughen the turbulence, highs brighten the core.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "flameHeight",
      "LABEL": "Flame Height",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.35,
      "MAX": 1.8
    },
    {
      "NAME": "turbulence",
      "LABEL": "Turbulence",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 2.2
    },
    {
      "NAME": "flickerSpeed",
      "LABEL": "Flicker Speed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 2.5
    },
    {
      "NAME": "coreIntensity",
      "LABEL": "Core Intensity",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 2.0
    },
    {
      "NAME": "glowAmount",
      "LABEL": "Hearth Glow",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 2.0
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "GROUP": "Audio Reactivity",
      "DEFAULT": 0.6,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "tintColor",
      "LABEL": "Tint",
      "TYPE": "color",
      "GROUP": "Color",
      "DEFAULT": [1.0, 1.0, 1.0, 1.0]
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 3.0
    }
  ]
}*/

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float hash1(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash1(i), hash1(i + vec2(1, 0)), u.x),
               mix(hash1(i + vec2(0, 1)), hash1(i + vec2(1, 1)), u.x), u.y);
}
float fbm(vec2 p) {
    float a = 0.5, r = 0.0;
    mat2 m = mat2(0.8, -0.6, 0.6, 0.8);
    for (int i = 0; i < 5; i++) {
        r += a * vnoise(p);
        p = m * p * 2.03 + 11.7;
        a *= 0.5;
    }
    return r;
}

// blackbody-ish flame ramp: transparent -> deep red -> orange -> yellow-white
vec3 fireRamp(float h) {
    vec3 c = mix(vec3(0.0), vec3(0.55, 0.03, 0.0), smoothstep(0.02, 0.22, h));
    c = mix(c, vec3(1.05, 0.34, 0.02), smoothstep(0.22, 0.52, h));
    c = mix(c, vec3(1.25, 0.95, 0.42), smoothstep(0.52, 0.85, h));
    return c;
}

// one flame tongue; p is relative to the flame base, w = width, h = height
// returns rgb (premultiplied additive) — turbulence is sampled in coordinates
// that rise over time, so features advect upward instead of scrolling
vec3 flameTongue(vec2 p, float w, float h, float seed, float T, float turb,
                 float coreGain) {
    float yn = p.y / h;
    if (yn < -0.08 || yn > 1.7) return vec3(0.0);

    // two turbulence layers rising at different speeds (parallax = depth)
    vec2 q1 = vec2(p.x * 2.6 / w, p.y * 2.4 - T * 1.55) + seed * 17.0;
    vec2 q2 = vec2(p.x * 5.6 / w, p.y * 5.0 - T * 2.85) - seed * 9.0;
    float n1 = fbm(q1);
    float n2 = fbm(q2);

    // sway displaces x more with height — tongues lick sideways at the tip
    float x = p.x / w + (n1 - 0.5) * turb * (0.25 + 1.7 * clamp(yn, 0.0, 1.4));

    // teardrop profile: fast rise from the base, tapering tip
    float ynp = clamp(yn, 0.0, 1.5);
    float prof = 1.55 * pow(max(1.0 - ynp * 0.72, 0.0), 1.1)
               * pow(clamp(ynp * 4.0 + 0.22, 0.0, 1.0), 0.5);
    float heat = prof - x * x * 2.3
               - (n2 - 0.28) * turb * (0.30 + 0.85 * ynp)
               - ynp * 0.30;
    heat = clamp(heat, 0.0, 1.35);

    vec3 col = fireRamp(heat) * smoothstep(0.0, 0.06, heat);

    // blue-white combustion core hugging the lower center — mixed, not added,
    // so the blue survives the white-hot body instead of saturating out
    float core = smoothstep(0.68, 1.0, heat)
               * (1.0 - smoothstep(0.10, 0.50, ynp));
    col = mix(col, vec3(0.40, 0.62, 1.35) * 1.45,
              clamp(core * 0.85 * coreGain, 0.0, 1.0));

    // fade smoothly through the base instead of a hard bed line
    col *= smoothstep(-0.10, 0.03, yn);
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 p = (uv - vec2(0.5, 0.0)) * 2.0;
    p.x *= RENDERSIZE.x / RENDERSIZE.y;
    p.y -= 0.14; // flame bed sits just above the bottom edge

    float ar = audioReact;
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP   = pow(knee(audioMid, 0.08, 0.90), 1.3);
    float highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float levelP = knee(audioLevel, 0.05, 0.90);

    float T = TIME * flickerSpeed;
    // idle breathing so the fire lives in silence; audio adds vigor on top
    float breath = 0.5 + 0.5 * sin(T * 0.53 + 1.7)
                 * (0.6 + 0.4 * sin(T * 0.219));
    float vigor = 0.86 + 0.14 * breath
                + ar * (0.42 * bassP + 0.22 * levelP);
    float turb = turbulence * (1.0 + ar * 0.35 * (midP - 0.25));
    float coreGain = coreIntensity * (1.0 + ar * 0.55 * highP);

    // bed of three tongues, each with its own phase lag and size
    float h0 = flameHeight * vigor;
    vec3 fl = vec3(0.0);
    fl += flameTongue(p - vec2(0.0, 0.0), 0.46, h0 * 1.18, 0.31, T, turb, coreGain);
    fl += flameTongue(p - vec2(-0.52, 0.02), 0.34, h0 * 0.82
                      * (0.9 + 0.1 * sin(T * 0.61 + 2.0)), 0.77, T * 1.13, turb,
                      coreGain * 0.8);
    fl += flameTongue(p - vec2(0.55, 0.03), 0.30, h0 * 0.72
                      * (0.9 + 0.1 * sin(T * 0.47 + 4.1)), 0.53, T * 0.91, turb,
                      coreGain * 0.7);

    // glowing coal bed under the flames, spanning the base
    float coalN = fbm(vec2(p.x * 5.5, p.y * 9.0 + TIME * 0.05));
    float coalBand = (1.0 - smoothstep(-0.02, 0.16, p.y))
                   * smoothstep(-0.35, -0.10, p.y - 0.0)
                   * (1.0 - smoothstep(0.9, 1.5, abs(p.x)));
    vec3 coals = vec3(0.95, 0.28, 0.05)
               * (0.25 + 0.75 * smoothstep(0.35, 0.75, coalN))
               * coalBand * (0.55 + 0.25 * breath + ar * 0.5 * bassP);

    // hearth: warm dark gradient + ambient glow around the bed — never black
    float d = length(p * vec2(0.55, 1.0));
    vec3 bg = coals + vec3(0.030, 0.014, 0.008) * (1.15 - uv.y * 0.75);
    float glowPulse = 0.75 + 0.25 * breath + ar * 0.45 * bassP;
    bg += vec3(0.42, 0.13, 0.03) * glowAmount * glowPulse
        * exp(-d * d * 2.4) * (1.0 - smoothstep(0.0, 1.4, p.y)) * 0.55;
    // faint heat shimmer band above the flames
    float shim = fbm(vec2(p.x * 3.0, p.y * 4.0 - T * 1.2));
    bg += vec3(0.10, 0.05, 0.02) * shim * glowAmount
        * smoothstep(0.2, 0.9, p.y) * (1.0 - smoothstep(0.9, 1.8, p.y)) * 0.6;

    vec3 col = bg + fl;
    col = 1.0 - exp(-col * 1.45); // soft filmic rolloff
    col = max(col, vec3(0.012));
    gl_FragColor = vec4(col * tintColor.rgb * brightness, 1.0);
}
