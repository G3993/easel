/*{
  "DESCRIPTION": "Shooting Stars — 500 stars spiral outward from the center in alternating colors, leaving glowing motion trails in a feedback buffer. Bass and loudness flare the stars bright, mids twist the spiral, trail length rides the music. Both star colors are user-editable.",
  "CREDIT": "Audio-reactive star spiral from Shadertoy, ShaderClaw audio port",
  "CATEGORIES": [
    "Generator",
    "Particles"
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
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "GROUP": "Audio Reactivity",
      "DEFAULT": 0.6,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "colorA",
      "LABEL": "Star Color A",
      "TYPE": "color",
      "DEFAULT": [0.3, 0.2, 1.0, 1.0]
    },
    {
      "NAME": "colorB",
      "LABEL": "Star Color B",
      "TYPE": "color",
      "DEFAULT": [1.0, 0.2, 0.3, 1.0]
    },
    {
      "NAME": "trailAmt",
      "LABEL": "Trails",
      "TYPE": "float",
      "DEFAULT": 0.5,
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
  ],
  "PASSES": [
    {
      "TARGET": "starBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

vec4 passStars() {
    vec2 uv = (gl_FragCoord.xy * 2.0 - RENDERSIZE.xy) / RENDERSIZE.xx;
    float T = TIME * speed;
    float ar = audioReact;

    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP   = pow(knee(audioMid,  0.08, 0.90), 1.3);
    float levelP = knee(audioLevel, 0.05, 0.90);

    vec3 col = vec3(0.07) + 0.04 * cos(vec3(0.0, 1.5, 3.0) + T * 0.3 + uv.xyx * 3.0);

    // flare energy (was summed FFT bins): floor keeps stars alive in silence
    float flare = mix(12.0, 6.0 + 26.0 * (0.6 * bassP + 0.4 * levelP), ar);
    // spiral twist (was summed low FFT bins on the spin angle)
    float twist = ar * 0.05 * midP;

    for (float i = 0.0; i < 500.0; i += 1.0) {
        float t = T + i * 0.05 * i;
        float a = smoothstep(0.0, 1.0, fract(t)) + floor(t);
        a *= 0.5 + twist;

        vec2 p = uv - fract(i * 0.01) * vec2(cos(a), sin(a));

        float att = (0.00004 + flare * 0.000005 * (1.0 - i / 250.0)) / (dot(p, p) + 0.00015);

        vec3 starCol = mix(colorA.rgb, colorB.rgb, mod(i, 2.0));
        starCol.g += ar * 0.06 * bassP;
        col += abs(att * starCol) * 0.5;
    }

    vec3 oldColor = texture2D(starBuf, gl_FragCoord.xy / RENDERSIZE.xy).rgb;
    vec3 color = max(vec3(0.0), col);

    // trail persistence: knob + music both stretch the trails
    float trailK = mix(trailAmt, min(trailAmt + 0.3, 1.0), ar * levelP);
    float fadeRate = mix(20.0, 4.0, trailK);
    float dt = clamp(TIMEDELTA, 0.001, 0.1);
    float keep = pow(0.1, fadeRate * dt);
    if (FRAMEINDEX < 2) keep = 0.0;

    return vec4(mix(color, oldColor, keep), 1.0);
}

void main() {
    if (PASSINDEX == 0) gl_FragColor = passStars();
    else gl_FragColor = vec4(texture2D(starBuf, gl_FragCoord.xy / RENDERSIZE.xy).rgb * tintColor.rgb * brightness, 1.0);
}
