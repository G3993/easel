#version 330 core
// Native 3D scene — card fragment. Same per-pixel maths as composite.frag
// (crop, mosaic, feather, mask, blend modes) so a flat card with the
// orthographic camera matches the 2D composite exactly; plus sun lighting
// and a PCF shadow map, both identity when lighting is off.
in vec2 vTexCoord;
in vec3 vWorldPos;
in vec3 vNormal;
in vec4 vShadowCoord;
out vec4 FragColor;

uniform vec2 uBaseSize = vec2(1.0);   // size of the accumulated base (= viewport)
#ifdef SCENE_DEPTH_TEX
// Volume objects (Fluid 3D): the layer texture is a view already rendered
// through the zone camera; its per-pixel scene depth sits in uDepthTex.z.
uniform sampler2D uDepthTex;
uniform mat4 uInvViewProj = mat4(1.0);   // to rebuild the world position for fog
#endif
uniform bool uFirst = false;          // first layer: passthrough-path maths (rolloff, no base)
uniform bool uLightOn = false;
uniform vec3 uLightDir = vec3(0.0, 0.0, 1.0);
uniform vec3 uLightColor = vec3(1.0);
uniform vec3 uAmbient = vec3(0.35);
uniform bool uShadowOn = false;
uniform sampler2D uShadowMap;
uniform float uShadowStrength = 0.8;
uniform float uShadowSoft = 1.5;
uniform float uShadowTexel = 1.0 / 2048.0;
uniform vec3 uCamPos = vec3(0.0, 0.0, 3.0);
uniform int uShadowTaps = 2;          // PCF half-kernel: 1 = 3x3, 2 = 5x5, 3 = 7x7
uniform int uPointCount = 0;
uniform vec4 uPointPos[4];            // xyz = world position, w = radius
uniform vec3 uPointCol[4];            // rgb * intensity
// Distance fog (scene-wide): 1 - exp(-((d - start) * density)^2)
uniform bool  uFogOn = false;
uniform vec3  uFogColor = vec3(0.05, 0.06, 0.09);
uniform float uFogDensity = 0.6;
uniform float uFogStart = 1.5;
float fogAmount(vec3 worldPos) {
    float f = max(length(worldPos - uCamPos) - uFogStart, 0.0) * uFogDensity;
    return 1.0 - exp(-f * f);
}

uniform sampler2D uBase;
uniform sampler2D uLayer;
uniform float uOpacity = 1.0;
uniform int uBlendMode = 0;
uniform sampler2D uMask;
uniform bool uHasMask = false;
uniform float uMaskFeather = 0.0; // UV units
uniform bool uMaskInvert = false;
uniform mat3 uInvTransform = mat3(1.0);
uniform float uTileX = 1.0;
uniform float uTileY = 1.0;
uniform vec4 uCrop = vec4(0);          // left, right, top, bottom
uniform int uMosaicMode = 0;           // 0=Mirror, 1=Hex
uniform float uMosaicDensity = 4.0;
uniform float uMosaicSpin = 0.0;
uniform float uTime = 0.0;
uniform float uAudioRMS = 0.0;
uniform float uAudioStrength = 0.0;
uniform int uMosaicModeFrom = 0;
uniform float uMosaicTransition = 1.0; // 0=old mode, 1=new mode

// Frequency band uniforms
uniform float uAudioBass = 0.0;
uniform float uAudioLowMid = 0.0;
uniform float uAudioHighMid = 0.0;
uniform float uAudioTreble = 0.0;
uniform float uAudioBeatDecay = 0.0;
uniform float uFeather = 0.0;
uniform bool uFlipV = false;

// BPM sync uniforms
uniform float uBeatPhase = 0.0;
uniform float uBeatPulse = 0.0;
uniform float uBarPhase = 0.0;
uniform float uBPM = 0.0;

vec3 blendNormal(vec3 base, vec3 blend)    { return blend; }
vec3 blendMultiply(vec3 base, vec3 blend)  { return base * blend; }
vec3 blendScreen(vec3 base, vec3 blend)    { return 1.0 - (1.0 - base) * (1.0 - blend); }

vec3 blendOverlay(vec3 base, vec3 blend) {
    return vec3(
        base.r < 0.5 ? 2.0 * base.r * blend.r : 1.0 - 2.0 * (1.0 - base.r) * (1.0 - blend.r),
        base.g < 0.5 ? 2.0 * base.g * blend.g : 1.0 - 2.0 * (1.0 - base.g) * (1.0 - blend.g),
        base.b < 0.5 ? 2.0 * base.b * blend.b : 1.0 - 2.0 * (1.0 - base.b) * (1.0 - blend.b)
    );
}

vec3 blendAdd(vec3 base, vec3 blend)       { return min(base + blend, 1.0); }
vec3 blendSubtract(vec3 base, vec3 blend)  { return max(base - blend, 0.0); }
vec3 blendDifference(vec3 base, vec3 blend){ return abs(base - blend); }

float softLightCh(float b, float s) {
    return (s <= 0.5) ? b - (1.0 - 2.0*s) * b * (1.0 - b)
                      : b + (2.0*s - 1.0) * (sqrt(b) - b);
}
vec3 blendSoftLight(vec3 base, vec3 blend) {
    return vec3(softLightCh(base.r, blend.r), softLightCh(base.g, blend.g), softLightCh(base.b, blend.b));
}

vec3 blendHardLight(vec3 base, vec3 blend) {
    return vec3(
        blend.r < 0.5 ? 2.0*base.r*blend.r : 1.0-2.0*(1.0-base.r)*(1.0-blend.r),
        blend.g < 0.5 ? 2.0*base.g*blend.g : 1.0-2.0*(1.0-base.g)*(1.0-blend.g),
        blend.b < 0.5 ? 2.0*base.b*blend.b : 1.0-2.0*(1.0-base.b)*(1.0-blend.b)
    );
}

vec3 blendColorDodge(vec3 base, vec3 blend) {
    return vec3(
        blend.r >= 1.0 ? 1.0 : min(1.0, base.r / (1.0 - blend.r)),
        blend.g >= 1.0 ? 1.0 : min(1.0, base.g / (1.0 - blend.g)),
        blend.b >= 1.0 ? 1.0 : min(1.0, base.b / (1.0 - blend.b))
    );
}

vec3 blendColorBurn(vec3 base, vec3 blend) {
    return vec3(
        blend.r <= 0.0 ? 0.0 : max(0.0, 1.0 - (1.0 - base.r) / blend.r),
        blend.g <= 0.0 ? 0.0 : max(0.0, 1.0 - (1.0 - base.g) / blend.g),
        blend.b <= 0.0 ? 0.0 : max(0.0, 1.0 - (1.0 - base.b) / blend.b)
    );
}

vec3 blendExclusion(vec3 base, vec3 blend) { return base + blend - 2.0 * base * blend; }

float cellPop(vec2 cell) {
    float h1 = fract(sin(dot(cell, vec2(127.1, 311.7))) * 43758.5);
    float h2 = fract(sin(dot(cell, vec2(269.5, 183.3))) * 28461.3);

    // Original prime-ratio wave interference — each cell has a unique phase
    // so they light up one-at-a-time in a non-repeating pattern
    float w1 = smoothstep(0.12, 0.0, min(abs(h1 - fract(uTime * 0.7)), 1.0 - abs(h1 - fract(uTime * 0.7))));
    float w2 = smoothstep(0.15, 0.0, min(abs(h2 - fract(uTime * 1.1)), 1.0 - abs(h2 - fract(uTime * 1.1))));
    float w3 = smoothstep(0.18, 0.0, min(abs(h1 - fract(uTime * 0.3 + 0.5)), 1.0 - abs(h1 - fract(uTime * 0.3 + 0.5))));

    // Radial pulse rippling outward from center
    float dist = length(cell) * 0.15;
    float ripple = smoothstep(0.2, 0.0, abs(fract(uTime * 0.8 - dist) - 0.5) * 2.0);

    float pop = max(max(w1, w2 * 0.7), w3 * 0.5) + ripple * 0.4;

    // Drive by beat energy instead of raw RMS:
    // beatDecay provides the envelope (1.0 on hit, decays smoothly)
    // bass provides the intensity scaling
    float beat = uAudioBeatDecay * (0.4 + uAudioBass * 0.6);
    pop *= beat * uAudioStrength * 4.0;
    return clamp(pop, 0.0, 1.0);
}

vec2 mosaicMirror(vec2 uv, out float pop) {
    uv *= vec2(uTileX, uTileY);
    vec2 cell = floor(uv);
    uv = fract(uv);
    if (int(cell.x) % 2 == 1) uv.x = 1.0 - uv.x;
    if (int(cell.y) % 2 == 1) uv.y = 1.0 - uv.y;

    pop = cellPop(cell);
    uv = mix(uv, vec2(0.5), pop * 0.2);
    return uv;
}

vec2 mosaicHex(vec2 uv, out float pop) {
    float spin = radians(uMosaicSpin);
    vec2 c = uv - 0.5;
    c = vec2(c.x * cos(spin) - c.y * sin(spin),
             c.x * sin(spin) + c.y * cos(spin));
    vec2 p = c * uMosaicDensity;

    const vec2 s = vec2(1.0, 1.7320508);
    vec2 halfS = s * 0.5;
    vec2 a = mod(p + halfS, s) - halfS;
    vec2 b = mod(p, s) - halfS;

    vec2 gv;
    vec2 cellCenter;
    if (dot(a, a) < dot(b, b)) {
        gv = a;
        cellCenter = p - a;
    } else {
        gv = b;
        cellCenter = p - b;
    }

    pop = (length(cellCenter) < 0.5) ? 0.0 : cellPop(cellCenter);
    vec2 result = gv / s + 0.5;
    result = mix(result, vec2(0.5), pop * 0.2);
    return clamp(result, 0.0, 1.0);
}

// Sample layer texture through a given mosaic mode, applying crop
vec4 sampleLayerWithMode(vec2 layerUV, int mode, vec2 cropMin, vec2 cropMax) {
    float pop = 0.0;
    vec2 uv;

    if (mode == 0) {
        // Mirror: crop first, then tile
        uv = (layerUV - cropMin) / (cropMax - cropMin);
        if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0) {
            return vec4(0);
        }
        uv = mosaicMirror(uv, pop);
    } else {
        uv = mosaicHex(layerUV, pop);
    }

    // Map back to texture space (apply crop)
    uv = cropMin + uv * (cropMax - cropMin);

    vec4 color = texture(uLayer, uv);
    color.rgb *= 1.0 + pop * 0.6;
    return color;
}


float shadowFactor() {
    vec3 sc = vShadowCoord.xyz / vShadowCoord.w;
    sc = sc * 0.5 + 0.5;
    if (sc.x < 0.0 || sc.x > 1.0 || sc.y < 0.0 || sc.y > 1.0 || sc.z > 1.0) return 1.0;
    float ndl = dot(normalize(vNormal), uLightDir);
    float bias = 0.0012 + 0.004 * (1.0 - abs(ndl));
    float lit = 0.0, n = 0.0;
    float r = uShadowSoft * uShadowTexel;
    for (int y = -3; y <= 3; y++) {
        for (int x = -3; x <= 3; x++) {
            if (abs(x) > uShadowTaps || abs(y) > uShadowTaps) continue;
            float d = texture(uShadowMap, sc.xy + vec2(x, y) * r).r;
            lit += (sc.z - bias <= d) ? 1.0 : 0.0;
            n += 1.0;
        }
    }
    return lit / max(n, 1.0);
}

void main() {
    vec2 layerUV = vTexCoord;   // the card's own UVs (uFlipV already applied in the vertex stage)
#ifdef SCENE_DEPTH_TEX
    gl_FragDepth = texture(uDepthTex, layerUV).z;
#endif
    vec4 base = uFirst ? vec4(0.0) : texture(uBase, gl_FragCoord.xy / uBaseSize);
    vec2 maskUV = layerUV;

    vec2 cropMin = vec2(uCrop.x, uCrop.w);
    vec2 cropMax = vec2(1.0 - uCrop.y, 1.0 - uCrop.z);

    vec4 layer;
    if (uMosaicTransition >= 1.0) {
        layer = sampleLayerWithMode(layerUV, uMosaicMode, cropMin, cropMax);
    } else {
        vec4 colorFrom = sampleLayerWithMode(layerUV, uMosaicModeFrom, cropMin, cropMax);
        vec4 colorTo   = sampleLayerWithMode(layerUV, uMosaicMode, cropMin, cropMax);
        float t = smoothstep(0.0, 1.0, uMosaicTransition);
        float dist = length(layerUV - 0.5) * 2.0;
        float radialT = smoothstep(0.0, 1.0, t * 2.5 - dist * 0.7);
        layer = mix(colorFrom, colorTo, radialT);
        layer.rgb *= 1.0 + sin(t * 3.14159) * 0.15;
    }

    // Edge feather (based on layer UV, not mosaic UV)
    float featherAlpha = 1.0;
    if (uFeather > 0.0) {
        float edgeDist = min(min(layerUV.x, 1.0 - layerUV.x), min(layerUV.y, 1.0 - layerUV.y));
        featherAlpha = smoothstep(0.0, uFeather, edgeDist);
    }

    if (uFirst) {
        // passthrough.frag's soft-knee HDR rolloff: identity in [0,1],
        // Reinhard shoulder on the overflow only.
        vec3 c  = max(layer.rgb, vec3(0.0));
        vec3 lo = min(c, vec3(1.0));
        vec3 hi = max(c - vec3(1.0), vec3(0.0));
        layer.rgb = clamp(lo + hi / (1.0 + hi), 0.0, 1.0);
    }

    // Sun + shadow (identity when lighting is off)
    if (uLightOn) {
        vec3 n = normalize(vNormal);
        if (dot(n, uCamPos - vWorldPos) < 0.0) n = -n;   // cards are two-sided
        float ndl = max(dot(n, uLightDir), 0.0);
        float sh = uShadowOn ? mix(1.0, shadowFactor(), uShadowStrength) : 1.0;
        vec3 lit = uAmbient + uLightColor * ndl * sh;
        for (int i = 0; i < 4; i++) {
            if (i >= uPointCount) break;
            vec3 toL = uPointPos[i].xyz - vWorldPos;
            float d = length(toL);
            float r = max(uPointPos[i].w, 0.01);
            float att = clamp(1.0 - (d * d) / (r * r), 0.0, 1.0);
            att *= att;
            float pdl = max(dot(n, toL / max(d, 1e-4)), 0.0);
            lit += uPointCol[i] * pdl * att;
        }
        layer.rgb *= lit;
    }

    float alpha = layer.a * uOpacity * featherAlpha;

    if (uHasMask) {
        float maskVal;
        if (uMaskFeather > 0.001) {
            float f = uMaskFeather * 0.5;
            const vec2 offs[9] = vec2[](
                vec2(-1,-1), vec2( 0,-1), vec2( 1,-1),
                vec2(-1, 0), vec2( 0, 0), vec2( 1, 0),
                vec2(-1, 1), vec2( 0, 1), vec2( 1, 1)
            );
            const float wts[9] = float[](
                1.0, 2.0, 1.0, 2.0, 4.0, 2.0, 1.0, 2.0, 1.0
            );
            float sum = 0.0;
            for (int i = 0; i < 9; i++) {
                sum += texture(uMask, maskUV + offs[i] * f).r * wts[i];
            }
            maskVal = sum * (1.0 / 16.0);
        } else {
            maskVal = texture(uMask, maskUV).r;
        }
        if (uMaskInvert) maskVal = 1.0 - maskVal;
        alpha *= maskVal;
    }

    if (uFogOn) {
#ifdef SCENE_DEPTH_TEX
        float dz = texture(uDepthTex, layerUV).z;
        vec4 cp = uInvViewProj * vec4(layerUV * 2.0 - 1.0, dz * 2.0 - 1.0, 1.0);
        vec3 fogPos = cp.xyz / cp.w;
#else
        vec3 fogPos = vWorldPos;
#endif
        layer.rgb = mix(layer.rgb, uFogColor, fogAmount(fogPos));
    }

    if (uFirst) {
        // First-layer path: drawn with SRC_ALPHA blending onto a cleared
        // target in 2D, i.e. stored premultiplied.
        FragColor = vec4(layer.rgb * alpha, alpha);
        return;
    }

    vec3 blended;
    switch (uBlendMode) {
        case 0:  blended = blendNormal(base.rgb, layer.rgb); break;
        case 1:  blended = blendMultiply(base.rgb, layer.rgb); break;
        case 2:  blended = blendScreen(base.rgb, layer.rgb); break;
        case 3:  blended = blendOverlay(base.rgb, layer.rgb); break;
        case 4:  blended = blendAdd(base.rgb, layer.rgb); break;
        case 5:  blended = blendSubtract(base.rgb, layer.rgb); break;
        case 6:  blended = blendDifference(base.rgb, layer.rgb); break;
        case 7:  blended = blendSoftLight(base.rgb, layer.rgb); break;
        case 8:  blended = blendHardLight(base.rgb, layer.rgb); break;
        case 9:  blended = blendColorDodge(base.rgb, layer.rgb); break;
        case 10: blended = blendColorBurn(base.rgb, layer.rgb); break;
        case 11: blended = blendExclusion(base.rgb, layer.rgb); break;
        default: blended = blendNormal(base.rgb, layer.rgb); break;
    }

    vec3 result = mix(base.rgb, blended, alpha);
    float resultAlpha = alpha + base.a * (1.0 - alpha);
    // composite.frag is drawn with SRC_ALPHA blending onto a cleared target:
    // the stored value is result * resultAlpha. Reproduce that here (the
    // card is drawn with blending OFF over a copy of the base).
    FragColor = vec4(result * resultAlpha, resultAlpha);
}
