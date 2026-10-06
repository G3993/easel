/*{
  "DESCRIPTION": "Dynamic Pixel Grid with Procedural Noise Animation",
  "CREDIT": "By fluidstorm.com (ISF)",
  "CATEGORIES": [
    "Generator"
  ],
  "INPUTS": [
    {
      "NAME": "maxOpacity",
      "TYPE": "float",
      "LABEL": "Max Opacity",
      "MIN": 0.1,
      "MAX": 1,
      "DEFAULT": 1
    },
    {
      "NAME": "octaves",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 8,
      "DEFAULT": 1,
      "LABEL": "Octaves"
    },
    {
      "NAME": "persistence",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 1,
      "DEFAULT": 0.6,
      "LABEL": "Persistence"
    },
    {
      "NAME": "lacunarity",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 4,
      "DEFAULT": 2.2,
      "LABEL": "Lacunarity"
    },
    {
      "NAME": "noiseStyle",
      "TYPE": "long",
      "LABEL": "Noise Style",
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Basic",
        "Clouds",
        "Turbulent",
        "Marble"
      ],
      "DEFAULT": 3
    },
    {
      "NAME": "inputTex",
      "TYPE": "image",
      "LABEL": "Texture"
    },
    {
      "NAME": "texMix",
      "TYPE": "float",
      "LABEL": "Texture Mix",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "horizontalScale",
      "TYPE": "float",
      "LABEL": "Horizontal Scale",
      "MIN": 0.1,
      "MAX": 10,
      "DEFAULT": 1.8,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "verticalScale",
      "TYPE": "float",
      "LABEL": "Vertical Scale",
      "MIN": 0.1,
      "MAX": 10,
      "DEFAULT": 1.4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "gridSize",
      "TYPE": "float",
      "LABEL": "Grid Size",
      "MIN": 1,
      "MAX": 20,
      "DEFAULT": 12.9,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "spacing",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 5,
      "DEFAULT": 1,
      "LABEL": "Spacing",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "TYPE": "float",
      "MIN": 0.01,
      "MAX": 0.5,
      "DEFAULT": 0.1,
      "LABEL": "Speed",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorRamp0",
      "TYPE": "color",
      "LABEL": "Color Low",
      "DEFAULT": [
        0,
        0,
        0,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "colorRamp0Pos",
      "TYPE": "float",
      "LABEL": "Low Position",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "colorRamp0Alpha",
      "TYPE": "float",
      "LABEL": "Low Alpha",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "colorRamp1",
      "TYPE": "color",
      "LABEL": "Color Mid",
      "DEFAULT": [
        1,
        1,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "colorRamp1Pos",
      "TYPE": "float",
      "LABEL": "Mid Position",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Color"
    },
    {
      "NAME": "colorRamp1Alpha",
      "TYPE": "float",
      "LABEL": "Mid Alpha",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "colorRamp2",
      "TYPE": "color",
      "LABEL": "Color High",
      "DEFAULT": [
        1,
        1,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "colorRamp2Pos",
      "TYPE": "float",
      "LABEL": "High Position",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "colorRamp2Alpha",
      "TYPE": "float",
      "LABEL": "High Alpha",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "LABEL": "Hue Shift",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "TYPE": "float",
      "LABEL": "Color Boost",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "zoom",
      "TYPE": "float",
      "MIN": 0.01,
      "MAX": 0.5,
      "DEFAULT": 0.42,
      "LABEL": "Zoom",
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "backgroundColor",
      "TYPE": "color",
      "LABEL": "Background",
      "DEFAULT": [
        0,
        0,
        0,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "useBackgroundColor",
      "TYPE": "bool",
      "LABEL": "Use Background",
      "DEFAULT": true,
      "GROUP": "Background"
    }
  ]
}*/

#ifdef GL_ES
precision highp float;
#endif

vec3 mod289v3(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec4 mod289v4(vec4 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec4 permute(vec4 x) { return mod289v4(((x * 34.0) + 1.0) * x); }
vec4 taylorInvSqrt(vec4 r) { return 1.79284291400159 - 0.85373472095314 * r; }

float snoise(vec3 v) {
    const vec2 C = vec2(1.0 / 6.0, 1.0 / 3.0);
    const vec4 D = vec4(0.0, 0.5, 1.0, 2.0);

    vec3 i = floor(v + dot(v, C.yyy));
    vec3 x0 = v - i + dot(i, C.xxx);

    vec3 g = step(x0.yzx, x0.xyz);
    vec3 l = 1.0 - g;
    vec3 i1 = min(g.xyz, l.zxy);
    vec3 i2 = max(g.xyz, l.zxy);

    vec3 x1 = x0 - i1 + C.xxx;
    vec3 x2 = x0 - i2 + C.yyy;
    vec3 x3 = x0 - D.yyy;

    i = mod289v3(i);
    vec4 p = permute(permute(permute(
                i.z + vec4(0.0, i1.z, i2.z, 1.0))
                + i.y + vec4(0.0, i1.y, i2.y, 1.0))
                + i.x + vec4(0.0, i1.x, i2.x, 1.0));

    float n_ = 0.142857142857;
    vec3 ns = n_ * D.wyz - D.xzx;

    vec4 j = p - 49.0 * floor(p * ns.z * ns.z);

    vec4 x_ = floor(j * ns.z);
    vec4 y_ = floor(j - 7.0 * x_);

    vec4 x = x_ * ns.x + ns.yyyy;
    vec4 y = y_ * ns.x + ns.yyyy;
    vec4 h = 1.0 - abs(x) - abs(y);

    vec4 b0 = vec4(x.xy, y.xy);
    vec4 b1 = vec4(x.zw, y.zw);

    vec4 s0 = floor(b0) * 2.0 + 1.0;
    vec4 s1 = floor(b1) * 2.0 + 1.0;
    vec4 sh = -step(h, vec4(0.0));

    vec4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
    vec4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

    vec3 p0 = vec3(a0.xy, h.x);
    vec3 p1 = vec3(a0.zw, h.y);
    vec3 p2 = vec3(a1.xy, h.z);
    vec3 p3 = vec3(a1.zw, h.w);

    vec4 norm = taylorInvSqrt(vec4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
    p0 *= norm.x;
    p1 *= norm.y;
    p2 *= norm.z;
    p3 *= norm.w;

    vec4 m = max(0.6 - vec4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
    m = m * m;
    return 42.0 * dot(m * m, vec4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
}

float fbm(vec3 p) {
    float total = 0.0;
    float amplitude = 1.0;
    float frequency = 1.0;
    float maxValue = 0.0;

    for (int i = 0; i < 8; ++i) {
        if (float(i) >= octaves) break;
        total += snoise(p * frequency) * amplitude;
        maxValue += amplitude;
        amplitude *= persistence;
        frequency *= lacunarity;
    }
    return total / maxValue;
}

vec4 getColorFromRamp(float t) {
    t = clamp(t, 0.0, 1.0);
    vec4 color0 = vec4(colorRamp0.rgb, colorRamp0Alpha);
    vec4 color1 = vec4(colorRamp1.rgb, colorRamp1Alpha);
    vec4 color2 = vec4(colorRamp2.rgb, colorRamp2Alpha);

    if (t <= colorRamp0Pos) return color0;
    if (t >= colorRamp2Pos) return color2;
    if (t < colorRamp1Pos) {
        float mixT = (t - colorRamp0Pos) / max(colorRamp1Pos - colorRamp0Pos, 0.001);
        return mix(color0, color1, mixT);
    } else {
        float mixT = (t - colorRamp1Pos) / max(colorRamp2Pos - colorRamp1Pos, 0.001);
        return mix(color1, color2, mixT);
    }
}

void main() {
    vec2 pix = gl_FragCoord.xy;
    // Non-gating: base alive at audio=0 with subtle idle breath
    float idlePulse = 1.0 + 0.04 * sin(TIME * 0.7);
    float gs = max(gridSize * (1.0 + audioBass * 0.5) * idlePulse, 1.0);
    vec2 cell = floor(pix / gs);
    vec2 within = mod(pix, gs);
    float gap = spacing;

    vec4 bgColor = useBackgroundColor ? backgroundColor : vec4(0.0);

    // Soft AA on cell boundaries: derive a coverage mask via smoothstep+fwidth.
    // Preserves the pixel-grid aesthetic at small AA width while killing aliasing.
    float aaW = max(fwidth(pix.x) + fwidth(pix.y), 0.0001) * 0.75;
    float maskX = smoothstep(gap - aaW, gap + aaW, within.x)
                * (1.0 - smoothstep(gs - gap - aaW, gs - gap + aaW, within.x));
    float maskY = smoothstep(gap - aaW, gap + aaW, within.y)
                * (1.0 - smoothstep(gs - gap - aaW, gs - gap + aaW, within.y));
    float cellMask = maskX * maskY;

    vec2 cellUV = (cell * gs + gs * 0.5) / RENDERSIZE.xy;
    vec3 pos = vec3(cellUV.x / zoom * horizontalScale, cellUV.y / zoom * verticalScale, TIME * speed * (1.0 + audioHigh * 3.0));
    float noiseValue = fbm(pos);

    int ns = int(noiseStyle);
    if (ns == 3) {
        noiseValue = sin(noiseValue * 10.0 + TIME * 0.05 * speed);
    } else if (ns == 2) {
        noiseValue = abs(noiseValue);
    } else if (ns == 1) {
        noiseValue = smoothstep(-0.5, 0.5, noiseValue);
    }

    float intensity = (noiseValue + 1.0) * 0.5;
    vec4 color = getColorFromRamp(intensity);

    // Texture sampling: each pixel cell samples from the texture at its center
    vec4 texSample = texture2D(inputTex, cellUV);
    if (texSample.a > 0.01 && texMix > 0.001) {
        vec3 texCol = texSample.rgb;
        texCol *= (0.3 + intensity * 0.7);
        color.rgb = mix(color.rgb, texCol, texMix);
    }

    // HDR PEAKS: hot fluid cores push into 1.4-2.5 linear so bloom catches them.
    // Curve: quiet mids stay ~1.0, top quartile lifts hard. Audio adds extra heat
    // but base lift is fully alive at audio=0.
    float hot = smoothstep(0.65, 0.98, intensity);
    float peakBoost = 1.0 + hot * (1.1 + audioHigh * 0.6); // ~1.0 -> ~2.1-2.7
    color.rgb *= peakBoost;

    color.a *= maxOpacity;

    // Composite: cellMask blends fluid pixel over background with soft AA edges.
    // HDR-safe: no clamp, no tonemap — bright cores stay >1.0 for bloom.
    float coverage = cellMask * color.a;

    // ---- universal color block (defaults = no-op) ----
    // (background handled by the existing backgroundColor/useBackgroundColor inputs)
    vec3 uc = mix(bgColor.rgb, color.rgb, coverage);
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

    gl_FragColor.rgb = uc;
    gl_FragColor.a = mix(bgColor.a, color.a, cellMask);
}
