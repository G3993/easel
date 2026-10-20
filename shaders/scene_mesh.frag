#version 330 core
// Native 3D scene — mesh material: flat-shaded (normals from derivatives,
// the loaders carry none), sun + PCF shadow + point lights, Blinn
// highlight, fresnel rim, optional self-glow. With the scene's lighting
// off a built-in header light keeps the form readable.
in vec3 vWorldPos;
in vec2 vTexCoord;
in vec4 vShadowCoord;
out vec4 FragColor;

uniform vec3 uColor = vec3(0.85);
uniform bool  uFogOn = false;
uniform vec3  uFogColor = vec3(0.05, 0.06, 0.09);
uniform float uFogDensity = 0.6;
uniform float uFogStart = 1.5;
uniform float uOpacity = 1.0;
uniform float uGloss = 0.5;
uniform float uMetal = 0.15;
uniform float uEmissive = 0.0;
uniform float uWire = 0.0;            // > 0: this draw is the wireframe overlay
uniform vec3 uCamPos = vec3(0.0, 0.0, 3.0);

uniform bool uLightOn = false;
uniform vec3 uLightDir = vec3(0.0, 0.0, 1.0);
uniform vec3 uLightColor = vec3(1.0);
uniform vec3 uAmbient = vec3(0.35);
uniform bool uShadowOn = false;
uniform sampler2D uShadowMap;
uniform float uShadowStrength = 0.8;
uniform float uShadowSoft = 1.5;
uniform float uShadowTexel = 1.0 / 2048.0;
uniform int uShadowTaps = 2;
uniform int uPointCount = 0;
uniform vec4 uPointPos[4];
uniform vec3 uPointCol[4];

float shadowFactor(vec3 n, vec3 L) {
    vec3 sc = vShadowCoord.xyz / vShadowCoord.w;
    sc = sc * 0.5 + 0.5;
    if (sc.x < 0.0 || sc.x > 1.0 || sc.y < 0.0 || sc.y > 1.0 || sc.z > 1.0) return 1.0;
    float ndl = dot(n, L);
    float bias = 0.0015 + 0.005 * (1.0 - abs(ndl));
    float lit = 0.0, cnt = 0.0;
    float r = uShadowSoft * uShadowTexel;
    for (int y = -3; y <= 3; y++) {
        for (int x = -3; x <= 3; x++) {
            if (abs(x) > uShadowTaps || abs(y) > uShadowTaps) continue;
            float d = texture(uShadowMap, sc.xy + vec2(x, y) * r).r;
            lit += (sc.z - bias <= d) ? 1.0 : 0.0;
            cnt += 1.0;
        }
    }
    return lit / max(cnt, 1.0);
}

void main() {
    vec3 n = normalize(cross(dFdx(vWorldPos), dFdy(vWorldPos)));
    vec3 v = normalize(uCamPos - vWorldPos);
    if (dot(n, v) < 0.0) n = -n;

    if (uWire > 0.0) {
        FragColor = vec4(uColor * uWire * 0.8, 0.0);   // additive overlay
        return;
    }

    vec3 L = uLightOn ? uLightDir : normalize(vec3(0.35, 0.65, 0.7));
    vec3 Lc = uLightOn ? uLightColor : vec3(1.0);
    vec3 amb = uLightOn ? uAmbient : vec3(0.38, 0.40, 0.44);
    float ndl = max(dot(n, L), 0.0);
    float sh = (uLightOn && uShadowOn) ? mix(1.0, shadowFactor(n, L), uShadowStrength) : 1.0;

    vec3 h = normalize(L + v);
    float specPow = mix(8.0, 160.0, uGloss * uGloss);
    float spec = pow(max(dot(n, h), 0.0), specPow) * mix(0.25, 1.4, uGloss);
    vec3 specCol = mix(vec3(1.0), uColor, uMetal);
    float fres = pow(1.0 - max(dot(n, v), 0.0), 4.0);

    vec3 col = uColor * (amb + Lc * ndl * sh) + specCol * Lc * spec * sh;
    for (int i = 0; i < 4; i++) {
        if (i >= uPointCount) break;
        vec3 toL = uPointPos[i].xyz - vWorldPos;
        float d = length(toL);
        float r = max(uPointPos[i].w, 0.01);
        float att = clamp(1.0 - (d * d) / (r * r), 0.0, 1.0);
        att *= att;
        vec3 Lp = toL / max(d, 1e-4);
        float pdl = max(dot(n, Lp), 0.0);
        vec3 hp = normalize(Lp + v);
        float sp = pow(max(dot(n, hp), 0.0), specPow) * mix(0.25, 1.4, uGloss);
        col += uPointCol[i] * att * (uColor * pdl + specCol * sp);
    }
    col += fres * mix(vec3(0.12), uColor, uMetal) * (0.3 + 0.3 * uGloss);
    col += uColor * uEmissive;
    // Soft-knee rolloff above 1.0 (identity below) so a strong light never
    // flattens the form into clipped white; glow above the knee still
    // feeds the bloom through the alpha-weighted bright pass.
    {
        vec3 lo = min(col, vec3(1.0));
        vec3 hi = max(col - vec3(1.0), vec3(0.0));
        col = lo + hi / (1.0 + hi);
    }

    if (uFogOn) {
        float f = max(length(vWorldPos - uCamPos) - uFogStart, 0.0) * uFogDensity;
        col = mix(col, uFogColor, 1.0 - exp(-f * f));
    }
    FragColor = vec4(col * uOpacity, uOpacity);   // premultiplied
}
