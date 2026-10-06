/*{
  "CATEGORIES": [
    "Generator"
  ],
  "DESCRIPTION": "Rainbow Flower — layered petal rings with simplex noise distortion, hue cycling, depth, and audio reactivity",
  "INPUTS": [
    {
      "NAME": "bloom",
      "LABEL": "Bloom",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5
    },
    {
      "NAME": "shadowStrength",
      "LABEL": "Shadow Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.6
    },
    {
      "NAME": "shadowSoftness",
      "LABEL": "Shadow Softness",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 6,
      "DEFAULT": 2
    },
    {
      "NAME": "innerShadow",
      "LABEL": "Inner Shadow",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.45
    },
    {
      "NAME": "rimLight",
      "LABEL": "Rim Light",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.35
    },
    {
      "NAME": "ambientOcclusion",
      "LABEL": "Ambient Occlusion",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.4
    },
    {
      "NAME": "petalCount",
      "LABEL": "Petals",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 12,
      "DEFAULT": 5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "layers",
      "LABEL": "Layers",
      "TYPE": "float",
      "MIN": 5,
      "MAX": 60,
      "DEFAULT": 40,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "depth",
      "LABEL": "Depth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "depthFalloff",
      "LABEL": "Depth Falloff",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.7,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "scale",
      "LABEL": "Scale",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "noiseAmount",
      "LABEL": "Noise",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "spinSpeed",
      "LABEL": "Spin",
      "TYPE": "float",
      "MIN": -3,
      "MAX": 3,
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "pulseSpeed",
      "LABEL": "Pulse",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1.25,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "hueRange",
      "LABEL": "Hue Range",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "saturation",
      "LABEL": "Saturation",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.42,
      "GROUP": "Color"
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.6,
      "GROUP": "Color"
    },
    {
      "NAME": "petalContrast",
      "LABEL": "Petal Contrast",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 2.5,
      "DEFAULT": 1.2,
      "GROUP": "Color"
    },
    {
      "NAME": "centerX",
      "LABEL": "Center X",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "centerY",
      "LABEL": "Center Y",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": true,
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ═══════════════════════════════════════════════════════════════════════
// RAINBOW FLOWER — Layered petal rings with simplex noise
// ═══════════════════════════════════════════════════════════════════════

const float PI = 3.1415926;
const float PI2 = 6.2831852;

#define SF 1.0 / min(RENDERSIZE.x, RENDERSIZE.y)
#define SS(l, s) smoothstep(SF, -SF, l - s)

// Soft-AA edge using fwidth so petal silhouettes stay crisp at any scale.
float aaEdge(float x) {
    float w = max(fwidth(x), 1e-5);
    return smoothstep(w, -w, x);
}

vec4 hueToColor(float v) {
    return saturation + brightness * cos(6.3 * v + vec4(0.0, 23.0, 21.0, 0.0));
}

const vec3 MOD3 = vec3(0.1031, 0.11369, 0.13787);

float hash11(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

vec3 hash33(vec3 p3) {
    p3 = fract(p3 * MOD3);
    p3 += dot(p3, p3.yxz + 19.19);
    return -1.0 + 2.0 * fract(vec3(
        (p3.x + p3.y) * p3.z,
        (p3.x + p3.z) * p3.y,
        (p3.y + p3.z) * p3.x
    ));
}

float snoise(vec3 p) {
    const float K1 = 0.333333333;
    const float K2 = 0.166666667;

    vec3 i = floor(p + (p.x + p.y + p.z) * K1);
    vec3 d0 = p - (i - (i.x + i.y + i.z) * K2);

    vec3 e = step(vec3(0.0), d0 - d0.yzx);
    vec3 i1 = e * (1.0 - e.zxy);
    vec3 i2 = 1.0 - e.zxy * (1.0 - e);

    vec3 d1 = d0 - (i1 - K2);
    vec3 d2 = d0 - (i2 - 2.0 * K2);
    vec3 d3 = d0 - (1.0 - 3.0 * K2);

    vec4 h = max(0.6 - vec4(dot(d0, d0), dot(d1, d1), dot(d2, d2), dot(d3, d3)), 0.0);
    vec4 n = h * h * h * h * vec4(
        dot(d0, hash33(i)),
        dot(d1, hash33(i + i1)),
        dot(d2, hash33(i + i2)),
        dot(d3, hash33(i + 1.0))
    );

    return dot(vec4(31.316), n);
}

void main() {
    vec2 uv = (gl_FragCoord.xy - RENDERSIZE.xy * 0.5) / RENDERSIZE.y;

    // Center offset
    uv -= vec2(centerX, centerY);

    // Scale
    uv /= max(scale, 0.01);

    // Audio (non-gating: floor keeps shader alive at audio=0).
    float aBass = audioBass * audioReact;
    float aMid = audioMid * audioReact;
    float aHigh = audioHigh * audioReact;
    float aLevel = audioLevel * audioReact;

    // Idle ambient pulse so motion + glow exist without audio.
    float idleP = 0.5 + 0.5 * sin(TIME * 0.7);
    float idleQ = 0.5 + 0.5 * sin(TIME * 1.3 + 1.7);

    // Audio-reactive scale pulse (idle breath when silent).
    uv /= 1.0 + aBass * 0.15 + idleP * 0.02;

    // Spin rotation
    float spinAngle = TIME * spinSpeed + aBass * 0.3;
    float cs = cos(spinAngle), sn = sin(spinAngle);
    uv = vec2(uv.x * cs - uv.y * sn, uv.x * sn + uv.y * cs);

    float l = length(uv);

    vec3 result = bgColor.rgb;

    float numLayers = layers + aBass * 10.0;

    for (float i = 60.0; i > 0.0; i -= 1.0) {
        if (i > numLayers) continue;

        // Layer parameter (0..1)
        float t = i / numLayers;

        // Petal angle modulation — audio can warp the petal shape
        float angle = atan(uv.y, uv.x) + i * 0.1;
        // Add extra petal lobes based on petalCount
        float petalMod = sin(angle * petalCount + i * 0.05 + aMid * 2.0);

        float a = sin(angle);
        float am = abs(a - 0.5) / 4.0;

        // Noise-driven radius with depth control
        float noiseVal = snoise(vec3(a, a, 10.0 * i * 0.005 + TIME * pulseSpeed - i * 0.11));
        float zn = 0.0125
                  + noiseVal * i * 0.0025 * noiseAmount
                  + i * 0.01 * depth;

        // Petal shape modulation
        zn += petalMod * 0.005 * bloom * i / numLayers;

        // Audio bloom: bass expands outer layers, highs add shimmer to inner
        zn += aBass * 0.008 * t;
        zn += aHigh * 0.003 * (1.0 - t) * sin(angle * 8.0 + TIME * 5.0);

        // Soft-AA petal edge — fwidth-based so it stays clean at any zoom.
        float edge = l - zn;
        float d = aaEdge(edge);

        // Hue: shifted + range-scaled per layer
        float rn = hash11(i);
        float hueVal = rn * hueRange + hueShift + aMid * 0.1 + idleQ * 0.02;
        vec3 col = hueToColor(hueVal).rgb;

        // Layered depth shading — inner layers progressively darker
        // by depthFalloff^layer-distance, giving real recession.
        float depthShade = pow(t, depthFalloff);
        col *= mix(0.30, 1.05, depthShade);

        // Petal contrast — exaggerate the bright/dark range per layer.
        col = mix(vec3(0.5), col, petalContrast);

        // Inner shadow — petal interior darkens toward the centre,
        // suggesting curved petal undersides catching less light.
        float interior = 1.0 - smoothstep(0.0, 0.15, l);
        col *= 1.0 - interior * innerShadow * (1.0 - t);

        // Ring-light removed — was blowing out the petals into a whitewashed
        // halo. Specular tip retained for crest definition only.
        float spec = exp(-pow((l - zn) * 220.0 / max(shadowSoftness, 0.5), 2.0));
        col += vec3(1.4, 1.3, 1.1) * spec * (0.7 + 0.3 * petalMod) * t * rimLight;

        // Petal-face HDR boost: lift the bright side of each petal so the
        // colourful highlights punch above 1.0 and bloom can grab them.
        // Toned down peak so the bloom pipeline catches it gentler.
        float petalFace = max(petalMod, 0.0);
        col += col * petalFace * (0.25 + idleP * 0.10) * t;

        result = mix(result, col, d);

        // Dropped shadow under each petal — outer ring of darker pixels
        // beyond the petal edge so layers cast onto the layer below.
        float shadowRing = SS(l - SF * shadowSoftness * (1.0 + i * 0.04), zn)
                         - SS(l, zn);
        result = mix(result, vec3(0.0),
                     shadowRing * shadowStrength * t * 0.6);

        // Dark edge outline between layers — kept from original.
        float dd = SS(l, zn) * SS(zn - SF, l);
        result = mix(result, vec3(0.0), dd * (0.6 + 0.4 * depth));

        // Ambient occlusion — every petal layer slightly darkens the
        // accumulated colour so deep stacks feel recessed.
        result *= 1.0 - d * ambientOcclusion * 0.05;
    }

    // Center glow — HDR core so bloom flares the heart of the flower.
    float centerCore = exp(-l * l * 80.0);
    float centerHalo = exp(-l * l * 20.0);
    float centerGlow = (centerHalo * 0.5 + centerCore * 1.6) * (0.5 + bloom * 0.5);
    result += vec3(1.0, 0.95, 0.9) * centerGlow
            * (1.7 + aBass * 0.6 + idleP * 0.25);

    // Pollen sparks — tiny HDR specks near the centre (alive at audio=0).
    float sparkA = exp(-pow(l * 9.0 - 1.2, 2.0) * 6.0);
    float sparkMod = 0.5 + 0.5 * sin(atan(uv.y, uv.x) * 9.0 + TIME * 2.3);
    result += vec3(1.4, 1.2, 0.7) * sparkA * sparkMod
            * (0.35 + aHigh * 0.6 + idleQ * 0.1);

    // Vignette
    float vig = 1.0 - smoothstep(0.4, 1.2, l * scale);
    result *= 0.7 + 0.3 * vig;

    // No tonemap, no clamp at top end — let Phase Q v4 bloom see HDR peaks.
    result = max(result, 0.0);

    // Bee/fly easter egg removed — the drifting dot was distracting.

    if (transparentBg) {
        float lum = dot(result, vec3(0.299, 0.587, 0.114));
        float a = clamp(max(lum, 1.0 - smoothstep(0.0, 0.02, l - 0.01)), 0.0, 1.0);
        gl_FragColor = vec4(result, a);
    } else {
        gl_FragColor = vec4(result, 1.0);
    }
}
