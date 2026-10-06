/*{
  "CATEGORIES": [
    "Generator",
    "Cymatics",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Chladni Figures with depth lighting, soft shadows, and slow drift. Sand on a vibrating plate (Ernst Chladni, 1787). Nodal-line equation Z = sin(nπx)sin(mπy) − sin(mπx)sin(nπy) solved per-pixel; sand collects along zero-crossings. Deferred-style normal map gives the sand ridges 3-D depth via a directional light with diffuse + specular. A slow Lissajous drift gently rocks the plate. Bass triggers mode jumps; mid/treble drive brightness and grain.",
  "INPUTS": [
    {
      "NAME": "mood",
      "LABEL": "Mood",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [0,1,2],
      "LABELS": ["Sand on Plate","Water Cymatics","Iron Filings + Magnet"]
    },
    {
      "NAME": "lightAngle",
      "LABEL": "Light Angle",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6.283,
      "DEFAULT": 0.785,
      "GROUP": "Lighting"
    },
    {
      "NAME": "lightElevation",
      "LABEL": "Light Elevation",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 1,
      "DEFAULT": 0.55,
      "GROUP": "Lighting"
    },
    {
      "NAME": "lightIntensity",
      "LABEL": "Light Intensity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Lighting"
    },
    {
      "NAME": "glossiness",
      "LABEL": "Glossiness",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 180,
      "DEFAULT": 48,
      "GROUP": "Lighting"
    },
    {
      "NAME": "ambientStrength",
      "LABEL": "Ambient Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.25,
      "GROUP": "Lighting"
    },
    {
      "NAME": "lineWidth",
      "LABEL": "Sand Line Width",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 3.5,
      "DEFAULT": 1.4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "grainDensity",
      "LABEL": "Grain Density",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "morphPeriod",
      "LABEL": "Mode Period (s)",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 14,
      "DEFAULT": 8,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "settle",
      "LABEL": "Settle vs Agitate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "driftAmount",
      "LABEL": "Drift Amount",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "driftSpeed",
      "LABEL": "Drift Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "warmth",
      "LABEL": "Warmth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.7,
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
      "LABEL": "Color Boost / Saturation",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "contrast",
      "LABEL": "Contrast",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "sandColor",
      "LABEL": "Sand / Line Color",
      "TYPE": "color",
      "DEFAULT": [0.94, 0.88, 0.72, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "sandColorCool",
      "LABEL": "Sand Cool Tone",
      "TYPE": "color",
      "DEFAULT": [0.86, 0.86, 0.84, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "specularColor",
      "LABEL": "Specular / Highlight Color",
      "TYPE": "color",
      "DEFAULT": [1.0, 0.97, 0.88, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "grainColor",
      "LABEL": "Grain / Sparkle Color",
      "TYPE": "color",
      "DEFAULT": [1.0, 0.95, 0.82, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "haloStrength",
      "LABEL": "Halo / Glow Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "vignetteAmount",
      "LABEL": "Vignette Amount",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.22,
      "GROUP": "Color"
    },
    {
      "NAME": "filmGrain",
      "LABEL": "Film Grain",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.008,
      "GROUP": "Color"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [0,0,0,0],
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

// ════════════════════════════════════════════════════════════════════════
//  Chladni Figures — Ernst Chladni 1787
//  Full per-pixel normal map lighting, Lissajous drift, soft shadow,
//  and a comprehensive color control suite.
// ════════════════════════════════════════════════════════════════════════

#define PI 3.14159265359

// ─── hashes ──────────────────────────────────────────────────────────
float hash11(float n)  { return fract(sin(n * 12.9898) * 43758.5453); }
float hash21(vec2 p)   { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// ─── 12 curated (n,m) mode pairs ─────────────────────────────────────
vec2 modePair(int i) {
    if (i ==  0) return vec2( 3.0,  5.0);
    if (i ==  1) return vec2( 2.0,  7.0);
    if (i ==  2) return vec2( 4.0,  6.0);
    if (i ==  3) return vec2( 5.0,  7.0);
    if (i ==  4) return vec2( 3.0,  8.0);
    if (i ==  5) return vec2( 6.0,  9.0);
    if (i ==  6) return vec2( 4.0, 11.0);
    if (i ==  7) return vec2( 7.0, 10.0);
    if (i ==  8) return vec2( 5.0, 12.0);
    if (i ==  9) return vec2( 8.0, 11.0);
    if (i == 10) return vec2( 2.0,  9.0);
    return             vec2( 6.0, 13.0);
}

// ─── Chladni scalar field ─────────────────────────────────────────────
float chladni(vec2 p, vec2 nm) {
    float a = sin(nm.x * PI * p.x) * sin(nm.y * PI * p.y);
    float b = sin(nm.y * PI * p.x) * sin(nm.x * PI * p.y);
    return a - b;
}

// ─── Analytic gradient ───────────────────────────────────────────────
vec2 chladniGrad(vec2 p, vec2 nm) {
    float nx = nm.x * PI;
    float ny = nm.y * PI;
    float dzdx = nx * cos(nx * p.x) * sin(ny * p.y)
               - ny * cos(ny * p.x) * sin(nx * p.y);
    float dzdy = ny * sin(nx * p.x) * cos(ny * p.y)
               - nx * sin(ny * p.x) * cos(nx * p.y);
    return vec2(dzdx, dzdy);
}

// ─── Nodal-line coverage (AA'd) ───────────────────────────────────────
float nodalLine(vec2 p, vec2 nm, float widthPx) {
    float Z   = chladni(p, nm);
    float aaw = max(fwidth(Z), 1e-5);
    float d   = abs(Z) / aaw;
    return 1.0 - smoothstep(widthPx * 0.5, widthPx * 0.5 + 1.2, d);
}

// ─── Height field for the sand ridge ─────────────────────────────────
float ridgeHeight(float Z, float aaw, float widthPx, float ridgeAmp) {
    float sigma = max(widthPx * 0.5 * aaw, 1e-4);
    return exp(-0.5 * (Z / sigma) * (Z / sigma)) * ridgeAmp;
}

// ─── Iron-filings dipole field (mood 2) ──────────────────────────────
float dipoleStream(vec2 p, float t) {
    vec2 a = vec2(0.30, 0.50) + 0.04 * vec2(sin(t * 0.21), cos(t * 0.17));
    vec2 b = vec2(0.70, 0.50) + 0.04 * vec2(cos(t * 0.19), sin(t * 0.23));
    vec2 da = p - a, db = p - b;
    vec2 Bv = da / (dot(da, da) + 1e-3) - db / (dot(db, db) + 1e-3);
    float ang = atan(Bv.y, Bv.x);
    return 0.5 + 0.5 * cos(ang * 7.0 + 12.0 * (p.x + p.y));
}

// ─── Slow Lissajous plate drift ───────────────────────────────────────
vec2 plateDrift(float t, float amount, float speed) {
    float s  = speed * 0.08;
    float dx = sin(t * s * 1.000 + 0.0) * 0.018
             + sin(t * s * 0.618 + 1.1) * 0.011;
    float dy = sin(t * s * 0.857 + 2.4) * 0.015
             + sin(t * s * 1.272 + 0.7) * 0.009;
    return vec2(dx, dy) * amount;
}

// ─── cheap YIQ hue rotation ──────────────────────────────────────────
vec3 hueRotate(vec3 c, float angle) {
    float hC = cos(angle), hS = sin(angle);
    mat3 hM = mat3(
        0.299,  0.587,  0.114,
        0.299,  0.587,  0.114,
        0.299,  0.587,  0.114)
      + hC * mat3(
         0.701, -0.587, -0.114,
        -0.299,  0.413, -0.114,
        -0.300, -0.588,  0.886)
      + hS * mat3(
         0.168,  0.330, -0.497,
        -0.328,  0.035,  0.292,
         1.250, -1.050, -0.203);
    return clamp(hM * c, 0.0, 1.0);
}

void main() {
    vec2 uv  = isf_FragNormCoord.xy;
    vec2 ndc = uv * 2.0 - 1.0;

    float t      = TIME;
    float audio  = clamp(audioReact, 0.0, 2.0);
    float bass   = clamp(audioBass  * audio, 0.0, 1.5);
    float mid    = clamp(audioMid   * audio, 0.0, 1.5);
    float treb   = clamp(audioHigh  * audio, 0.0, 1.5);
    float level  = clamp(audioLevel * audio, 0.0, 1.5);

    // ── mode-pair morph ───────────────────────────────────────────────
    float period   = max(4.0, morphPeriod);
    float baseIdx  = t / period;
    float bassKick = smoothstep(0.55, 0.95, bass);
    float idxF     = baseIdx + 1.5 * bassKick + 0.05 * sin(t * 0.07);

    int   ia = int(mod(floor(idxF),       12.0));
    int   ib = int(mod(floor(idxF) + 1.0, 12.0));
    float u  = smoothstep(0.0, 1.0, fract(idxF));

    vec2 nmA = modePair(ia);
    vec2 nmB = modePair(ib);
    vec2 nm  = mix(nmA, nmB, u);

    // ── aspect-corrected plate coords ─────────────────────────────────
    float ar = RENDERSIZE.x / RENDERSIZE.y;
    vec2  p  = uv;
    if (ar > 1.0) p.x = (uv.x - 0.5) * ar + 0.5;
    else          p.y = (uv.y - 0.5) / ar + 0.5;

    // ── slow plate drift ──────────────────────────────────────────────
    p += plateDrift(t, driftAmount * 0.06, driftSpeed);

    // ── agitation jitter ──────────────────────────────────────────────
    float agitate = clamp((level - settle * 0.6) * 1.3, 0.0, 1.0);
    vec2 jit = (vec2(hash21(p * RENDERSIZE.xy),
                     hash21(p * RENDERSIZE.xy + 17.3)) - 0.5);
    p += jit * (0.0015 + 0.010 * agitate);

    // ── field evaluation ──────────────────────────────────────────────
    float widthPx = lineWidth * (0.85 + 0.6 * mid);
    float Z       = chladni(p, nm);
    float aaw     = max(fwidth(Z), 1e-5);
    float d       = abs(Z) / aaw;
    float line    = 1.0 - smoothstep(widthPx * 0.5, widthPx * 0.5 + 1.2, d);
    float halo    = (1.0 - smoothstep(widthPx * 0.5 * 3.5,
                                      widthPx * 0.5 * 3.5 + 1.2, d)) - line;
    halo = clamp(halo, 0.0, 1.0);

    // ── surface normal from ridge height field ─────────────────────────
    float ridgeAmp = 0.35;
    float h0       = ridgeHeight(Z, aaw, widthPx, ridgeAmp);
    float sigma    = max(widthPx * 0.5 * aaw, 1e-4);
    float dHdZ     = -(Z / (sigma * sigma)) * h0;
    vec2  gradZ    = chladniGrad(p, nm);
    float pixScale = 1.0 / max(RENDERSIZE.x, RENDERSIZE.y);
    vec2  dHdp     = dHdZ * gradZ * pixScale;
    vec3  N        = normalize(vec3(-dHdp.x, -dHdp.y, 1.0));

    // ── directional light ─────────────────────────────────────────────
    float el   = clamp(lightElevation, 0.05, 1.0);
    vec3  Ldir = normalize(vec3(cos(lightAngle) * cos(el * PI * 0.5),
                                sin(lightAngle) * cos(el * PI * 0.5),
                                sin(el * PI * 0.5)));
    float diff = clamp(dot(N, Ldir), 0.0, 1.0);

    vec3  V    = vec3(0.0, 0.0, 1.0);
    vec3  H    = normalize(Ldir + V);
    float spec = pow(clamp(dot(N, H), 0.0, 1.0), glossiness);

    // ── soft self-shadow ──────────────────────────────────────────────
    float shadowStep = 0.003 + 0.004 * (1.0 - el);
    vec2  pShadow    = p - vec2(Ldir.x, Ldir.y) * shadowStep;
    float Zshadow    = chladni(pShadow, nm);
    float hShadow    = ridgeHeight(Zshadow, aaw, widthPx, ridgeAmp);
    float shadow     = 1.0 - clamp((hShadow - h0) * 6.0, 0.0, 0.65);

    // Ambient term respects user control
    float ambient  = ambientStrength;
    float diffLit  = (ambient + (1.0 - ambient) * diff * shadow) * lightIntensity;
    float specLit  = spec * shadow * lightIntensity;

    // ── per-mood base colour ──────────────────────────────────────────
    int moodI = int(mood + 0.5);
    vec3 col;

    // User-supplied color inputs
    vec3 userSand     = sandColor.rgb;
    vec3 userSandCool = sandColorCool.rgb;
    vec3 userSpec     = specularColor.rgb;
    vec3 userGrain    = grainColor.rgb;

    if (moodI == 1) {
        // ─ Water cymatics ─────────────────────────────────────────────
        // Blend user sand color into the caustic tones so it's controllable
        vec3 water1 = mix(vec3(0.04, 0.10, 0.16), userSandCool, 0.25);
        vec3 water2 = mix(vec3(0.02, 0.05, 0.10), userSandCool, 0.15);
        float ripple = 0.5 + 0.5 * sin(40.0 * (p.x + p.y) + t * 1.4);
        vec3 base    = mix(water2, water1, 0.5 + 0.4 * ripple);
        base = mix(base, bgColor.rgb, bgColor.a);

        // Caustic crest color driven by sandColor
        vec3 cau = mix(vec3(0.85, 0.95, 1.05), userSand, 0.4);

        col  = base;
        col += cau * line  * (0.9 + 1.2 * mid) * diffLit;
        col += cau * 0.35  * halo * haloStrength * diffLit;
        col += userSand * line * bassKick * 0.5;

        // Specular: blend user specular color with a cool water tint
        vec3 waterSpec = mix(vec3(0.7, 0.9, 1.1), userSpec, 0.5);
        col += waterSpec * specLit * line * 0.9;

    } else if (moodI == 2) {
        // ─ Iron filings + dipole ──────────────────────────────────────
        // Plate color: blends bgColor as before; also tinted by sandColorCool
        vec3 plate = mix(vec3(0.10, 0.09, 0.08), userSandCool, 0.18);
        plate = mix(plate, bgColor.rgb, bgColor.a);
        float dip    = dipoleStream(p, t);
        float dipL   = smoothstep(0.55, 0.92, dip);
        float combined = max(line * 0.85, dipL * (0.55 + 0.35 * line));

        // Iron color tinted by sandColor
        vec3 iron = mix(vec3(0.55, 0.50, 0.46), userSand, 0.35);

        col  = mix(plate, iron * diffLit, combined * (0.7 + 0.5 * mid));
        col *= mix(1.0, 0.72, (1.0 - shadow) * combined);
        col += userSand * combined * bassKick * 0.4;

        // Metal specular: blend user spec color
        vec3 metalSpec = mix(vec3(1.0, 0.95, 0.88), userSpec, 0.6);
        col += metalSpec * specLit * combined * 0.7;

    } else {
        // ─ Sand on plate (default) ────────────────────────────────────
        vec3 plate    = mix(vec3(0.18, 0.16, 0.14), userSandCool * 0.6, 0.2);
        plate = mix(plate, bgColor.rgb, bgColor.a);

        // User controls both warm and cool sand endpoints
        vec3 sand = mix(userSandCool, userSand, clamp(warmth, 0.0, 1.0));

        // Brushed plate grain
        float brushed = 0.5 + 0.5 * sin(p.y * 540.0 + hash21(p) * 6.28);
        plate += vec3(0.018) * (brushed - 0.5);

        // Self-shadow on plate under sand ridges
        float plateShad = mix(1.0, 0.60, (1.0 - shadow) * line);
        plate *= plateShad;

        float br     = 0.85 + 1.45 * mid + 0.25 * level;
        vec3 sandLit = sand * br * diffLit;

        col  = plate;
        col  = mix(col, sandLit, line);
        col += sand * halo * (0.18 + 0.35 * mid) * diffLit * haloStrength;

        // Specular: blend between a warmth-derived color and user specular
        vec3 warmSpec = mix(vec3(1.0, 0.97, 0.88), vec3(1.0, 0.90, 0.70), clamp(warmth, 0.0, 1.0));
        vec3 specColorFinal = mix(warmSpec, userSpec, 0.5);
        col += specColorFinal * specLit * line * 0.55;
    }

    // ── treble grains ─────────────────────────────────────────────────
    {
        vec2  gp      = floor(uv * RENDERSIZE.xy / 1.6);
        float h1      = hash21(gp);
        float h2      = hash21(gp + 7.7);
        float h3      = hash21(gp + 13.3);
        float gateLine = nodalLine(p, nm, widthPx * 2.2);
        float gateRand = step(1.0 - 0.55 * grainDensity * (0.4 + treb), h1);
        float twinkle  = step(0.55, fract(h2 + t * (0.7 + 1.2 * treb) + h3));
        float grain    = gateLine * gateRand * twinkle;
        col += userGrain * grain * (0.6 + 1.4 * treb) * (0.6 + 0.4 * diff);
    }

    // ── standing-wave shimmer ─────────────────────────────────────────
    float shimmer = 0.5 + 0.5 * sin(t * 0.6 + (nm.x + nm.y) * 0.3);
    col *= 0.96 + 0.06 * shimmer;

    // ── soft vignette ─────────────────────────────────────────────────
    col *= 1.0 - vignetteAmount * dot(ndc * 0.5, ndc * 0.5) * 4.0;

    // ── film grain ────────────────────────────────────────────────────
    col += (hash21(uv * RENDERSIZE.xy + t) - 0.5) * filmGrain * 2.0;

    // ── mild HDR boost on bright areas ───────────────────────────────
    float lum = dot(col, vec3(0.2126, 0.7152, 0.0722));
    col += col * smoothstep(0.85, 1.6, lum) * 0.6;

    // ── universal color controls ──────────────────────────────────────
    // Brightness
    col *= brightness;

    // Contrast (pivot at 0.5)
    col = (col - 0.5) * contrast + 0.5;

    // Saturation / color boost
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);

    // Hue shift (YIQ rotation)
    if (hueShift > 0.0005) {
        col = hueRotate(col, hueShift * 6.2831853);
    }

    col = clamp(col, 0.0, 1.0);

    gl_FragColor = vec4(col, 1.0);
}