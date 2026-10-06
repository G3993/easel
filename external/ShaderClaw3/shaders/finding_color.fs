/*{
  "DESCRIPTION": "Finding Color — a self-eroding feedback landscape keeps rewriting itself while dancing orbs, each tied to its own frequency band, bob across the field injecting fresh terrain. Extended controls for chaos, movement, warp, color and orb behavior.",
  "CREDIT": "Erosion feedback sim + shading from shadertoy Xsd3DB lineage, ShaderClaw audio port with dancing orbs",
  "CATEGORIES": [
    "Generator"
  ],
  "INPUTS": [
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 1.0,
      "MIN": 0.1,
      "MAX": 3.0
    },
    {
      "NAME": "warpStrength",
      "LABEL": "Warp Strength",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 0.4,
      "MIN": 0.0,
      "MAX": 2.0
    },
    {
      "NAME": "warpFreq",
      "LABEL": "Warp Frequency",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 1.0,
      "MIN": 0.1,
      "MAX": 5.0
    },
    {
      "NAME": "erosionRate",
      "LABEL": "Erosion Rate",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 0.08,
      "MIN": 0.0,
      "MAX": 0.5
    },
    {
      "NAME": "terrainScale",
      "LABEL": "Terrain Scale",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 1.0,
      "MIN": 0.1,
      "MAX": 4.0
    },
    {
      "NAME": "flowAngle",
      "LABEL": "Flow Angle",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 6.2832
    },
    {
      "NAME": "flowSpeed",
      "LABEL": "Flow Speed",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 0.0,
      "MIN": -2.0,
      "MAX": 2.0
    },
    {
      "NAME": "orbWander",
      "LABEL": "Orb Wander",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "orbitPrecess",
      "LABEL": "Orbit Precess",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 0.0,
      "MIN": -2.0,
      "MAX": 2.0
    },
    {
      "NAME": "warpSwirl",
      "LABEL": "Warp Swirl",
      "TYPE": "float",
      "GROUP": "Motion",
      "DEFAULT": 0.0,
      "MIN": -2.0,
      "MAX": 2.0
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "GROUP": "Audio",
      "DEFAULT": 0.6,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "bassInfluence",
      "LABEL": "Bass → Orbs",
      "TYPE": "float",
      "GROUP": "Audio",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "midInfluence",
      "LABEL": "Mid → Erosion",
      "TYPE": "float",
      "GROUP": "Audio",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "highInfluence",
      "LABEL": "High → Color Spin",
      "TYPE": "float",
      "GROUP": "Audio",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "colorA",
      "LABEL": "Color A",
      "TYPE": "color",
      "GROUP": "Color",
      "DEFAULT": [0.9, 0.3, 0.5, 1.0]
    },
    {
      "NAME": "colorB",
      "LABEL": "Color B",
      "TYPE": "color",
      "GROUP": "Color",
      "DEFAULT": [0.2, 0.7, 1.0, 1.0]
    },
    {
      "NAME": "colorC",
      "LABEL": "Color C",
      "TYPE": "color",
      "GROUP": "Color",
      "DEFAULT": [1.0, 0.85, 0.3, 1.0]
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
    },
    {
      "NAME": "colorPhaseSpeed",
      "LABEL": "Color Phase Speed",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 0.2,
      "MIN": 0.0,
      "MAX": 2.0
    },
    {
      "NAME": "colorPhaseScale",
      "LABEL": "Color Phase Scale",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 1.7,
      "MIN": 0.1,
      "MAX": 8.0
    },
    {
      "NAME": "saturation",
      "LABEL": "Saturation",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "lightDir",
      "LABEL": "Light Angle",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 6.2832
    },
    {
      "NAME": "specPower",
      "LABEL": "Specular Power",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 32.0,
      "MIN": 1.0,
      "MAX": 128.0
    },
    {
      "NAME": "specStrength",
      "LABEL": "Specular Strength",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 4.0
    },
    {
      "NAME": "orbCount",
      "LABEL": "Orb Count",
      "TYPE": "float",
      "GROUP": "Orbs",
      "DEFAULT": 5.0,
      "MIN": 1.0,
      "MAX": 8.0
    },
    {
      "NAME": "orbSize",
      "LABEL": "Orb Size",
      "TYPE": "float",
      "GROUP": "Orbs",
      "DEFAULT": 1.0,
      "MIN": 0.1,
      "MAX": 4.0
    },
    {
      "NAME": "orbBrightness",
      "LABEL": "Orb Brightness",
      "TYPE": "float",
      "GROUP": "Orbs",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 4.0
    },
    {
      "NAME": "orbOrbitRadius",
      "LABEL": "Orbit Radius",
      "TYPE": "float",
      "GROUP": "Orbs",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "orbOrbitSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "GROUP": "Orbs",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 4.0
    },
    {
      "NAME": "orbBobAmount",
      "LABEL": "Orb Bob",
      "TYPE": "float",
      "GROUP": "Orbs",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "orbInjectStrength",
      "LABEL": "Orb Inject Strength",
      "TYPE": "float",
      "GROUP": "Orbs",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 4.0
    },
    {
      "NAME": "chaosAmp",
      "LABEL": "Chaos Amplitude",
      "TYPE": "float",
      "GROUP": "Chaos",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 4.0
    },
    {
      "NAME": "chaosTwist",
      "LABEL": "Chaos Twist",
      "TYPE": "float",
      "GROUP": "Chaos",
      "DEFAULT": 0.0,
      "MIN": -3.14159,
      "MAX": 3.14159
    },
    {
      "NAME": "noiseAmp",
      "LABEL": "Noise Amplitude",
      "TYPE": "float",
      "GROUP": "Chaos",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 4.0
    },
    {
      "NAME": "feedbackDecay",
      "LABEL": "Feedback Decay",
      "TYPE": "float",
      "GROUP": "Chaos",
      "DEFAULT": 1.0001,
      "MIN": 0.990,
      "MAX": 1.020
    }
  ],
  "PASSES": [
    {
      "TARGET": "valBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

#define PI 3.1415927
#define N_ORBS_MAX 8
#define VAL_MAX 40.0

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
float fftLog(float t) { return texture2D(audioFFT, vec2(pow(t, 2.2) * 0.5, 0.5)).r; }

vec4 encV(float v) {
    float e = clamp(v / VAL_MAX, 0.0, 1.0) * 255.0;
    return vec4(floor(e) / 255.0, fract(e), 0.0, 1.0);
}
float decV(vec4 t) { return (t.r + t.g / 255.0) * VAL_MAX; }

float rnd(vec2 co) {
    return fract(sin(dot(co, vec2(12.9898, 78.233)) + fract(TIME) * 7.13) * 43758.5453);
}

float getVal(vec2 offsetPx) {
    vec2 uv = (gl_FragCoord.xy + offsetPx) / RENDERSIZE.xy;
    return decV(texture2D(valBuf, uv));
}

vec2 orbPos(int i, float T) {
    float fi = float(i);
    float nOrbs = clamp(orbCount, 1.0, float(N_ORBS_MAX));
    float band = pow(knee(fftLog(fi / nOrbs), 0.03, 0.7), 1.3);
    float a = T * orbOrbitSpeed * (0.3 + 0.13 * fi) + fi * 2.4;
    float baseRad = (0.22 + 0.13 * fi * 0.25) * orbOrbitRadius;
    float rad = baseRad + 0.18 * band * audioReact * bassInfluence * orbOrbitRadius;
    vec2 orb = rad * vec2(cos(a), sin(a * (1.0 + 0.21 * fi)));
    // MOTION: the orbit path itself slowly precesses (per-orb alternating sense)
    float pr = T * orbitPrecess * 0.3 * (mod(fi, 2.0) < 0.5 ? 1.0 : -1.0);
    orb = vec2(cos(pr) * orb.x - sin(pr) * orb.y, sin(pr) * orb.x + cos(pr) * orb.y);
    vec2 c = vec2(0.5) + orb;
    // MOTION: independent lissajous wander layered on the orbit
    c += orbWander * 0.08 * vec2(sin(T * 0.53 + fi * 1.9), cos(T * 0.41 + fi * 2.7));
    c.y += 0.06 * band * sin(T * 6.0 + fi) * orbBobAmount;
    return c * RENDERSIZE.xy;
}

// Rotate a 2D vector
vec2 rot2(vec2 v, float angle) {
    float c = cos(angle);
    float s = sin(angle);
    return vec2(c * v.x - s * v.y, s * v.x + c * v.y);
}

vec3 saturateRGB(vec3 col, float sat) {
    float lum = dot(col, vec3(0.2126, 0.7152, 0.0722));
    return mix(vec3(lum), col, sat);
}

vec4 passSim() {
    float T = TIME * speed;
    float ar = audioReact;
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP  = pow(knee(audioMid,  0.08, 0.90), 1.3);

    // Optional global UV flow / scroll
    vec2 flowDir = vec2(cos(flowAngle), sin(flowAngle)) * flowSpeed * TIMEDELTA * RENDERSIZE.y * 0.1;
    vec2 sampUV = (gl_FragCoord.xy - flowDir) / RENDERSIZE.xy;
    float val = decV(texture2D(valBuf, sampUV));

    // Erosion — mids stir it
    float erosionAmount = erosionRate * mix(1.0, 0.5 + 1.6 * midP * midInfluence, ar);
    val += rnd(gl_FragCoord.xy) * noiseAmp * val * erosionAmount;

    // Warp lookup with twist and scale
    float warpW = warpStrength * warpFreq * terrainScale;
    float twistT = chaosTwist + T * warpSwirl * 0.1;   // MOTION: warp direction slowly revolves
    float wx = sin(getVal(vec2(val, 0.0)) - getVal(vec2(-val, 0.0)) + PI + twistT) * val * warpW;
    float wy = cos(getVal(vec2(0.0, -val)) - getVal(vec2(0.0, val)) - PI * 0.5 + twistT) * val * warpW;
    vec2 warpOffset = rot2(vec2(wx, wy), twistT) * chaosAmp;
    val = getVal(warpOffset);

    val *= feedbackDecay;

    // Dancing orbs inject terrain; bass feeds them
    int nOrbs = int(clamp(orbCount, 1.0, float(N_ORBS_MAX)));
    for (int i = 0; i < N_ORBS_MAX; i++) {
        if (i >= nOrbs) break;
        float d = length(orbPos(i, T) - gl_FragCoord.xy);
        float orbRadius = RENDERSIZE.y / (10.0 / max(orbSize, 0.01));
        val += smoothstep(orbRadius, 0.5, d)
             * (0.35 + ar * 2.4 * bassP * bassInfluence) * orbInjectStrength;
    }

    if (FRAMEINDEX < 2) {
        val = rnd(gl_FragCoord.xy) * length(RENDERSIZE.xy) / 100.0
            + smoothstep(length(RENDERSIZE.xy) / 2.0, 0.5,
                         length(RENDERSIZE.xy * 0.5 - gl_FragCoord.xy)) * 25.0;
    }
    return encV(val);
}

vec4 passImage() {
    float T = TIME * speed;
    float ar = audioReact;
    float highP  = pow(knee(audioHigh,  0.10, 0.90), 1.2) * highInfluence;
    float levelP = knee(audioLevel, 0.05, 0.90);

    vec2 q = gl_FragCoord.xy / RENDERSIZE.xy;
    float val = decV(texture2D(valBuf, q));

    // Three editable colors mixed by terrain phase (highs spin the mix)
    float ph = val * colorPhaseScale + T * colorPhaseSpeed + ar * 1.5 * highP;
    float w1 = 0.5 + 0.5 * sin(ph);
    float w2 = 0.5 + 0.5 * sin(ph + 2.094);
    float w3 = 0.5 + 0.5 * sin(ph + 4.188);
    vec3 tint = (colorA.rgb * w1 + colorB.rgb * w2 + colorC.rgb * w3) / max(w1 + w2 + w3, 1e-3);
    tint = saturateRGB(tint, saturation);

    vec4 color = vec4(tint, 1.0) * pow(clamp(vec4(cos(val), 0.8, sin(val), 1.0) * 0.5 + 0.5, 0.0, 1.0), vec4(0.5));

    // Normal / lighting
    vec3 e = vec3(1.0 / RENDERSIZE.xy, 0.0);
    float p10 = decV(texture2D(valBuf, q - e.zy));
    float p01 = decV(texture2D(valBuf, q - e.xz));
    float p21 = decV(texture2D(valBuf, q + e.xz));
    float p12 = decV(texture2D(valBuf, q + e.zy));
    vec3 grad = normalize(vec3(p21 - p01, p12 - p10, 1.0));

    // Rotatable light direction
    vec3 light = normalize(vec3(cos(lightDir) * 0.2, sin(lightDir) * 0.25, 0.7));
    float diffuse = dot(grad, light);
    float spec = pow(max(0.0, -reflect(light, grad).z), specPower) * specStrength;

    vec4 col = (color * diffuse) + spec;

    // Glowing dancing orbs
    int nOrbs = int(clamp(orbCount, 1.0, float(N_ORBS_MAX)));
    for (int i = 0; i < N_ORBS_MAX; i++) {
        if (i >= nOrbs) break;
        float band = pow(knee(fftLog(float(i) / clamp(orbCount, 1.0, float(N_ORBS_MAX))), 0.03, 0.7), 1.3);
        float d = length(orbPos(i, T) - gl_FragCoord.xy) / RENDERSIZE.y;
        vec3 oc = (i == 0 || i == 3) ? colorA.rgb : ((i == 1 || i == 4) ? colorB.rgb : colorC.rgb);
        float sharp = orbSize * orbSize * 900.0;
        col.rgb += oc * exp(-d * d * sharp) * (0.5 + 1.6 * band * ar) * orbBrightness;
        col.rgb += oc * exp(-d * 14.0 / orbSize) * 0.10 * (0.5 + band) * orbBrightness;
    }

    col.rgb *= 1.0 + ar * 0.3 * levelP;
    col.rgb = saturateRGB(col.rgb, saturation);
    col.rgb *= tintColor.rgb * brightness;
    return vec4(col.rgb, 1.0);
}

void main() {
    if (PASSINDEX == 0) gl_FragColor = passSim();
    else                gl_FragColor = passImage();
}