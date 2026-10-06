/*{
  "DESCRIPTION": "Rainbow Around — 15 invisible orbiting charges push a 2D vector field; the field's direction paints the hue and its strength the brightness, swirling rainbow rivers around the movers. Bass brightens the field, mids swell the charges, highs rotate the rainbow. Optional charge-dot overlay.",
  "CREDIT": "Vector-field charge visualizer from Shadertoy, ShaderClaw audio port",
  "CATEGORIES": [
    "Generator",
    "Pattern"
  ],
  "INPUTS": [
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.1,
      "MAX": 3.0
    },
    {
      "NAME": "orbitReach",
      "LABEL": "Orbit Reach",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 2.5
    },
    {
      "NAME": "orbitSwirl",
      "LABEL": "Orbit Swirl",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 0.0,
      "MIN": -1.0,
      "MAX": 1.0
    },
    {
      "NAME": "centerWander",
      "LABEL": "Centre Wander",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "chargePulse",
      "LABEL": "Charge Pulse",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "GROUP": "Audio Reactivity",
      "DEFAULT": 0.5,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "saturation",
      "LABEL": "Saturation",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "showPoints",
      "LABEL": "Show Charges",
      "TYPE": "bool",
      "DEFAULT": false
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

#define OBJECTS 15
#define TAU 6.283185307179586

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float rand(vec2 co) {
    return fract(sin(mod(dot(co.xy, vec2(12.9898, 78.233)), 3.14)) * 43758.5453);
}

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    vec3 p = abs(fract(c.x + K.xyz) * 6.0 - K.w);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void main() {
    vec2 uv = (gl_FragCoord.xy * 2.0 - RENDERSIZE.xy) / RENDERSIZE.y * 3.0;
    float T = TIME * speed;
    float ar = audioReact;

    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP  = pow(knee(audioMid,  0.08, 0.90), 1.3);
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);

    vec4 fragColor = vec4(0.0);
    vec2 f = vec2(0.0);
    float chargeGain = mix(1.0, 0.4 + 1.9 * midP, ar);

    for (int i = 0; i < OBJECTS; i++) {
        vec2 c = vec2(float(i) + 0.5, 0.5);
        // procedural per-object params (was the keyboard-editable state buffer):
        // v = orbit amplitude + center, v2 = orbit freqs + charge
        vec4 v  = vec4(rand(c + 0.2) * 5.0 * orbitReach, rand(c + 0.3) * 5.0 * orbitReach, 0.0, 0.0);
        vec3 v2 = vec3(rand(c) * 1.5, rand(c + 0.1) * 1.5, rand(c + 0.4) * 6.0 - 3.0);
        // MOTION: orbit centres wander on their own slow lissajous paths
        v.zw = centerWander * vec2(sin(T * 0.31 * (0.5 + rand(c + 0.6)) + rand(c + 0.7) * TAU),
                                   cos(T * 0.27 * (0.5 + rand(c + 0.8)) + rand(c + 0.9) * TAU));
        // MOTION: charges wax and wane so the rivers re-route
        v2.z *= 1.0 + chargePulse * sin(T * (0.8 + 1.2 * rand(c + 0.5)) + rand(c + 0.45) * TAU);

        vec2 pos = v.xy * vec2(sin(T * v2.x), cos(T * v2.y));
        // MOTION: alternate charges swirl their orbit paths CW / CCW (never a rigid frame spin)
        float sw = T * orbitSwirl * 0.5 * (mod(float(i), 2.0) < 0.5 ? 1.0 : -1.0);
        float cs = cos(sw), sn = sin(sw);
        pos = mat2(cs, -sn, sn, cs) * pos + v.zw;
        vec2 d = uv - pos;
        float w = dot(d, d);

        if (showPoints) {
            fragColor += 0.01 / w;
            float g = 4.0 * length(d) / (w * w + 3.0);
            g *= g; g *= g; g *= g; g *= g; g *= g;
            fragColor += g * vec4(1, 0, 0, 1) * 0.2;
        } else {
            f += normalize(d) / max(w, 1e-5) * v2.z * chargeGain;
        }
    }

    float hue = atan(f.x, f.y) / TAU + ar * 0.12 * highP;
    // soft exponential response never clamps flat, so audio gain stays visible
    float levelP = knee(audioLevel, 0.05, 0.90);
    float gain = mix(1.0, 0.25 + 1.6 * levelP + 1.1 * bassP, ar);
    float val = 1.0 - exp(-length(f.xy) * gain);
    vec3 col = hsv2rgb(vec3(hue, saturation, val));
    fragColor += vec4(col, 1.0);

    gl_FragColor = vec4(fragColor.rgb * tintColor.rgb * brightness, 1.0);
}
