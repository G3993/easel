/*{
  "DESCRIPTION": "Chromatic dispersion — refractive color separation driven by animated flow noise",
  "CREDIT": "ShaderClaw (dispersion model inspired by Shadertoy)",
  "CATEGORIES": [
    "Effect"
  ],
  "INPUTS": [
    {
      "NAME": "inputImage",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "noiseScale",
      "LABEL": "Noise Scale",
      "TYPE": "float",
      "DEFAULT": 3,
      "MIN": 0.5,
      "MAX": 12,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "flowSpeed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "dispScale",
      "LABEL": "Dispersion",
      "TYPE": "float",
      "DEFAULT": 0.08,
      "MIN": 0,
      "MAX": 0.3,
      "GROUP": "Color"
    },
    {
      "NAME": "contrastAmt",
      "LABEL": "Contrast",
      "TYPE": "float",
      "DEFAULT": 12,
      "MIN": 1,
      "MAX": 30,
      "GROUP": "Color"
    },
    {
      "NAME": "causticStrength",
      "LABEL": "Caustics",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 2.5,
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
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": 1,
      "GROUP": "Background"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "GROUP": "Background"
    }
  ],
  "PASSES": [
    {
      "TARGET": "dispBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

// ---- Simplex 2D noise (Ashima Arts) ----
vec3 mod289(vec3 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec2 mod289(vec2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
vec3 permute(vec3 x) { return mod289(((x * 34.0) + 1.0) * x); }

float snoise(vec2 v) {
    const vec4 C = vec4(0.211324865405187, 0.366025403784439,
                       -0.577350269189626, 0.024390243902439);
    vec2 i  = floor(v + dot(v, C.yy));
    vec2 x0 = v - i + dot(i, C.xx);
    vec2 i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
    vec4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;
    i = mod289(i);
    vec3 p = permute(permute(i.y + vec3(0.0, i1.y, 1.0))
                             + i.x + vec3(0.0, i1.x, 1.0));
    vec3 m = max(0.5 - vec3(dot(x0, x0), dot(x12.xy, x12.xy),
                             dot(x12.zw, x12.zw)), 0.0);
    m = m * m;
    m = m * m;
    vec3 x = 2.0 * fract(p * C.www) - 1.0;
    vec3 h = abs(x) - 0.5;
    vec3 ox = floor(x + 0.5);
    vec3 a0 = x - ox;
    m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);
    vec3 g;
    g.x = a0.x * x0.x + h.x * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}

// Curl noise — divergence-free 2D flow from scalar noise
vec2 curlNoise(vec2 p) {
    float eps = 0.001;
    float n1 = snoise(vec2(p.x, p.y + eps));
    float n2 = snoise(vec2(p.x, p.y - eps));
    float a = (n1 - n2) / (2.0 * eps);
    n1 = snoise(vec2(p.x + eps, p.y));
    n2 = snoise(vec2(p.x - eps, p.y));
    float b = (n1 - n2) / (2.0 * eps);
    return vec2(a, -b);
}

// ---- Dispersion helpers ----

vec3 sigmoidContrast(vec3 x, float k) {
    return 1.0 / (1.0 + exp(-k * (x - 0.5)));
}

vec2 normz(vec2 x) {
    return length(x) < 0.0001 ? vec2(0.0) : normalize(x);
}

// Spectral weights — models human cone response curves
vec3 sampleWeights(float i) {
    return vec3(i * i, 46.6666 * pow((1.0 - i) * i, 3.0), (1.0 - i) * (1.0 - i));
}

#define SAMPLES 16

// Procedural starter texture — used when no inputImage is bound. Gives
// the dispersion effect something rich to bend so the canvas isn't black.
vec3 starterTex(vec2 uv) {
    // High-contrast colorful pattern: rotating cosine palette with bands
    vec2 c = uv - 0.5;
    float r = length(c);
    float a = atan(c.y, c.x);
    float bands = sin(r * 18.0 - TIME * 0.4) * 0.5 + 0.5;
    float sweep = sin(a * 6.0 + TIME * 0.3) * 0.5 + 0.5;
    vec3 grad = 0.5 + 0.5 * cos(6.28318 *
                (uv.x * 2.0 + uv.y * 1.3 + TIME * 0.05) +
                vec3(0.0, 2.094, 4.188));
    return mix(grad, vec3(1.0, 0.95, 0.85), bands * sweep * 0.4);
}

vec3 sampleDisp(vec2 uv, vec2 dispNorm, float disp) {
    bool hasInput = IMG_SIZE_inputImage.x > 0.0;
    vec3 col = vec3(0.0);
    vec3 denom = vec3(0.0);
    float sd = 1.0 / float(SAMPLES);
    float wl = 0.0;
    for (int i = 0; i < SAMPLES; i++) {
        vec3 sw = sampleWeights(wl);
        denom += sw;
        vec2 sUV = uv + dispNorm * disp * wl;
        vec3 src = hasInput ? texture2D(inputImage, sUV).rgb : starterTex(sUV);
        col += sw * src;
        wl += sd;
    }
    return col / denom;
}

void main() {
    vec2 uv = isf_FragNormCoord;
    vec2 texel = 1.0 / RENDERSIZE;

    // ---- Soft-knee audio conditioning (playbook) — shared by both passes ----
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
    float midP  = smoothstep(0.08, 0.85, audioMid);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float drive = 0.25 + 0.75 * smoothstep(0.05, 0.9, audioEnergy);

    // ---- Pass 0: generate animated displacement field ----
    if (PASSINDEX == 0) {
        float t = TIME * flowSpeed * (0.9 + 0.4 * (drive - 0.25)); // flow rides the energy
        vec2 p = uv * noiseScale;
        // Multi-octave curl noise for organic flow
        // Bass swells the big flow; mids stir the mid-octave turbulence
        vec2 d = curlNoise(p + t) * 0.6 * (1.0 + 0.25 * bassP)
               + curlNoise(p * 2.1 - t * 0.7) * 0.3 * (1.0 + 0.30 * midP)
               + curlNoise(p * 4.3 + t * 0.4) * 0.1;
        gl_FragColor = vec4(d, 0.0, 1.0);
        return;
    }

    // ---- Pass 1: chromatic dispersion ----
    vec2 n = vec2(0.0, texel.y);
    vec2 e = vec2(texel.x, 0.0);
    vec2 s = vec2(0.0, -texel.y);
    vec2 w = vec2(-texel.x, 0.0);

    vec2 d   = texture2D(dispBuf, uv).xy;
    vec2 d_n = texture2D(dispBuf, fract(uv + n)).xy;
    vec2 d_e = texture2D(dispBuf, fract(uv + e)).xy;
    vec2 d_s = texture2D(dispBuf, fract(uv + s)).xy;
    vec2 d_w = texture2D(dispBuf, fract(uv + w)).xy;

    // Antialias the vector field
    vec2 db = 0.4 * d + 0.15 * (d_n + d_e + d_s + d_w);

    float ld = length(db);
    vec2 ln = normz(db);

    vec3 col = sampleDisp(uv, ln, dispScale * ld * (1.0 + 0.22 * bassP)); // bass widens the spectral split
    col = sigmoidContrast(col, contrastAmt);

    // ---- Caustics: bright focal lines where the dispersion field converges ----
    // Negative divergence of the flow field = light rays focusing to a point.
    // Computed from the same 4 neighbor samples we already fetched. Produces
    // crisp animated rainbow filaments — the "light through water" look.
    if (causticStrength > 0.0) {
        float divX = d_e.x - d_w.x;
        float divY = d_n.y - d_s.y;
        float divergence = divX + divY;
        // Caustic intensity = focusing (negative divergence), sharpened.
        float focus = max(-divergence, 0.0);
        float caustic = pow(focus, 3.0) * 18.0;
        // Tint by the local dispersion direction so caustics inherit hue
        // from the spectrum they're focusing — different colors converge
        // at different points.
        vec3 causticHue = 0.5 + 0.5 * cos(6.28318 *
                          (ld * 4.0 + vec3(0.0, 0.33, 0.67)));
        // Highs make the filaments glitter; punch adds a decaying flare
        col += causticHue * caustic * causticStrength * (1.0 + 0.40 * highP + 0.30 * audioPunch);
    }

    // ---- HDR PEAKS: lift brightest dispersion strips into bloom range ----
    // Spec edges = high gradient of luminance in the field, where the
    // sigmoid contrast clips into pure spectrum. Audio peaks = high |db|
    // ridges. Both get pushed into linear 1.6–2.5 so the upstream bloom
    // bright-pass (>~0.85) rims them without changing the look at base.
    {
        // Spec edge: how steeply does the dispersion vector field change
        // around this pixel? Length-of-gradient via the 4 neighbors we
        // already sampled. Soft-AA via fwidth of the field length.
        float ldn = length(d_n);
        float lde = length(d_e);
        float lds = length(d_s);
        float ldw = length(d_w);
        float gx = lde - ldw;
        float gy = ldn - lds;
        float specEdge = sqrt(gx * gx + gy * gy);

        // Audio peak: ridge of the field itself. Smoothstep with fwidth
        // for soft, resolution-independent edges (no audio gating: alive
        // at audio=0 because |db| is purely the noise field).
        float aw = max(fwidth(ld), 1e-4);
        float audioPeak = smoothstep(0.55 - aw, 0.85 + aw, ld);

        // Saturation of the dispersed color — most rainbow strips live
        // where one channel dominates the other two.
        float mxc = max(max(col.r, col.g), col.b);
        float mnc = min(min(col.r, col.g), col.b);
        float sat = (mxc - mnc) / max(mxc, 1e-4);

        // Brightness in linear-ish luminance (the bloom expects linear).
        float lum = dot(col, vec3(0.2126, 0.7152, 0.0722));

        // Combine: highlight rims need both a spectrum-clipped pixel AND
        // either a spec edge or audio ridge. Soft-AA all transitions.
        float ew = max(fwidth(specEdge), 1e-4);
        float specMask = smoothstep(0.05 - ew, 0.25 + ew, specEdge);
        float satMask  = smoothstep(0.55, 0.95, sat);
        float lumMask  = smoothstep(0.65, 0.98, lum);

        float peakMask = clamp(max(specMask, audioPeak) * satMask * lumMask, 0.0, 1.0);

        // Lift peaks into HDR — directional along spectrum so the rim
        // keeps its hue. Push ~1.6× at edge of peak, up to ~2.5× at core.
        float lift = mix(1.0, mix(1.6, 2.5, lumMask), peakMask);
        col *= lift;
    }

    float alpha = 1.0;
    if (transparentBg) {
        alpha = dot(col, vec3(0.299, 0.587, 0.114));
    }

    // Surprise: every ~22s a complete spectrum splits dramatically —
    // for ~0.5s the dispersion blows wide and rejoins. Newton's prism.
    {
        float _ph = fract(TIME / 22.0);
        float _f  = smoothstep(0.0, 0.04, _ph) * smoothstep(0.18, 0.10, _ph);
        // Push channels apart based on luminance
        float _l = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(col, vec3(col.r * 1.4, col.g, col.b * 0.8) * (0.5 + _l), _f * 0.4);
    }

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = col;
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
    float ucA = alpha;
    if (bgColor.a > 0.0) {                                 // fill low-alpha bg region
        uc = mix(uc, bgColor.rgb, (1.0 - clamp(ucA, 0.0, 1.0)) * bgColor.a);
        ucA = ucA + (1.0 - clamp(ucA, 0.0, 1.0)) * bgColor.a;
    }

    // NO TONEMAP — pass HDR through to bloom pipeline.
    gl_FragColor = vec4(uc, ucA);
}
