/*{
  "CATEGORIES": [
    "Generator",
    "Atmospheric",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Volumetric fog machine — domain-warped fbm rolling forward, with an optional input texture sampled through the warp so the image itself becomes the fog. Audio-reactive density pulse on bass.",
  "INPUTS": [
    {
      "NAME": "density",
      "LABEL": "Density",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1.15
    },
    {
      "NAME": "glow",
      "LABEL": "Glow",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.7
    },
    {
      "NAME": "textureMix",
      "LABEL": "Texture as Fog",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "textureWarp",
      "LABEL": "Texture Warp",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.35
    },
    {
      "NAME": "textureZoom",
      "LABEL": "Texture Zoom",
      "TYPE": "float",
      "MIN": 0.25,
      "MAX": 4,
      "DEFAULT": 1
    },
    {
      "NAME": "inputTex",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "scale",
      "LABEL": "Fog Scale",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 8,
      "DEFAULT": 2.4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "depth",
      "LABEL": "Depth Layers",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Drift Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.35,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "billow",
      "LABEL": "Billowing",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.85,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "fogColor",
      "LABEL": "Fog Color",
      "TYPE": "color",
      "DEFAULT": [
        0.85,
        0.88,
        0.95,
        1
      ],
      "GROUP": "Color"
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
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "backColor",
      "LABEL": "Back Color",
      "TYPE": "color",
      "DEFAULT": [
        0.04,
        0.05,
        0.09,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

float hash21(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

float vnoise(vec2 p) {
    vec2 ip = floor(p), fp = fract(p);
    fp = fp * fp * (3.0 - 2.0 * fp);
    float a = hash21(ip);
    float b = hash21(ip + vec2(1.0, 0.0));
    float c = hash21(ip + vec2(0.0, 1.0));
    float d = hash21(ip + vec2(1.0, 1.0));
    return mix(mix(a, b, fp.x), mix(c, d, fp.x), fp.y);
}

float fbm(vec2 p) {
    float v = 0.0, amp = 0.5;
    mat2 rot = mat2(0.8, -0.6, 0.6, 0.8);
    for (int i = 0; i < 5; i++) {
        v += amp * vnoise(p);
        p = rot * p * 2.02;
        amp *= 0.5;
    }
    return v;
}

void main() {
    vec2 res = RENDERSIZE.xy;
    vec2 uv = gl_FragCoord.xy / res;
    float aspect = res.x / max(res.y, 1.0);
    vec2 p = (uv - 0.5) * vec2(aspect, 1.0);

    float t = TIME * speed;
    float bass = clamp(audioBass, 0.0, 1.0) * audioReact;

    // ---- domain warp: two layers drifting against each other ----
    vec2 q = vec2(
        fbm(p * scale + vec2(0.0, t)),
        fbm(p * scale + vec2(5.2, -t * 0.7) + 1.7)
    );

    vec2 r = vec2(
        fbm(p * scale + billow * q + vec2(1.7 - t, 9.2)),
        fbm(p * scale + billow * q + vec2(8.3, 2.8 + t * 0.55))
    );

    float fog = fbm(p * scale + billow * r);

    // back layer drifting opposite direction at half scale — adds depth
    float back = fbm(p * scale * 0.5 + vec2(-t * 0.4, t * 0.25) + r * 0.5);
    fog = mix(fog, max(fog, back * 0.85), depth);

    // bass-pumped density + baseline TIME breath so silent runs still move
    float breath = 0.06 * sin(TIME * 0.7);
    float d = clamp((fog - 0.35 + breath + 0.18 * bass) * density, 0.0, 1.0);
    d = smoothstep(0.0, 1.0, d);

    // ---- color: procedural fog vs. input texture sampled through the warp ----
    vec3 fogTint = fogColor.rgb;

    // highlight catches in dense ridges — fake light scatter
    float ridge = smoothstep(0.55, 0.95, fog);
    fogTint += glow * ridge * vec3(0.45, 0.55, 0.7);

    vec3 procColor = mix(backColor.rgb, fogTint, d);

    if (textureMix > 0.001 && IMG_SIZE_inputTex.x > 0.0) {
        // sample the texture at uv distorted by the same warp that built the fog
        vec2 warp = (r - 0.5) * textureWarp;
        vec2 tuv = (uv - 0.5) / max(textureZoom, 0.0001) + 0.5 + warp;
        tuv = clamp(tuv, vec2(0.0), vec2(1.0));
        vec3 texColor = texture(inputTex, tuv).rgb;

        // texture replaces fog tint where fog is present, so the image
        // shows up *as* the fog rather than behind it
        vec3 texFog = mix(backColor.rgb, texColor, d);
        procColor = mix(procColor, texFog, clamp(textureMix, 0.0, 1.0));
    }

    // subtle vertical light-shaft falloff (fog machines feel bottom-heavy)
    float shaft = mix(0.92, 1.05, smoothstep(0.0, 1.0, 1.0 - uv.y));
    procColor *= shaft;

    // ---- universal color block (defaults = no-op) ----
    // (background handled by the existing backColor input)
    vec3 uc = procColor;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                   // saturation
    if (hueShift > 0.0005) {                               // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }

    gl_FragColor = vec4(uc, 1.0);
}
