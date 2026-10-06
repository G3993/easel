/*{
  "DESCRIPTION": "Unveiled — a keyed silhouette (camera, layer, or a procedural walking figure when nothing is bound) dissolves into a fluid: every frame the image is advected along its own brightness gradient plus a layered sine turbulence, so the figure smears, veils and re-emerges as the source burns back through. Luminance is mapped to a hot ember → dark blood → black palette with film texture. Bass drives the smear, beats kick the turbulence, mids shift the hue, highs add grain. Every parameter is exposed.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-23 — after a Shadertoy keyed-fluid-smear sketch pasted by Lu",
  "CATEGORIES": ["Effect", "Feedback", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "inputTex",    "LABEL": "Source",           "TYPE": "image" },
    { "NAME": "keyColor",    "LABEL": "Key Colour",       "TYPE": "color", "DEFAULT": [0.051, 0.639, 0.145, 1.0], "GROUP": "Source" },
    { "NAME": "keyAmt",      "LABEL": "Keying",           "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Source" },
    { "NAME": "invert",      "LABEL": "Invert Subject",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Source" },
    { "NAME": "lift",        "LABEL": "Source Lift",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.8,  "GROUP": "Source" },
    { "NAME": "flow",        "LABEL": "Smear Direction",  "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Fluid" },
    { "NAME": "diffusion",   "LABEL": "Diffusion",        "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Fluid" },
    { "NAME": "sampleDist",  "LABEL": "Sample Distance",  "TYPE": "float", "MIN": 2.0,  "MAX": 80.0, "DEFAULT": 30.0, "GROUP": "Fluid" },
    { "NAME": "turbAmt",     "LABEL": "Turbulence",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.2,  "GROUP": "Fluid" },
    { "NAME": "turbDetail",  "LABEL": "Turbulence Detail","TYPE": "float", "MIN": 5.0,  "MAX": 60.0, "DEFAULT": 30.0, "GROUP": "Fluid" },
    { "NAME": "turbSpeed",   "LABEL": "Turbulence Speed", "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Fluid" },
    { "NAME": "fluidify",    "LABEL": "Fluidify",         "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.1,  "GROUP": "Fluid" },
    { "NAME": "attenuate",   "LABEL": "Re-emerge",        "TYPE": "float", "MIN": 0.0,  "MAX": 0.05, "DEFAULT": 0.005,"GROUP": "Fluid" },
    { "NAME": "color1",      "LABEL": "Ember",            "TYPE": "color", "DEFAULT": [1.0, 0.25, 0.15, 1.0], "GROUP": "Color" },
    { "NAME": "color2",      "LABEL": "Blood",            "TYPE": "color", "DEFAULT": [0.5, 0.1, 0.1, 1.0],   "GROUP": "Color" },
    { "NAME": "color3",      "LABEL": "Void",             "TYPE": "color", "DEFAULT": [0.0, 0.0, 0.0, 1.0],   "GROUP": "Color" },
    { "NAME": "multiplier",  "LABEL": "Level",            "TYPE": "float", "MIN": 0.2,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "midPosition", "LABEL": "Mid Point",        "TYPE": "float", "MIN": 0.05, "MAX": 0.95, "DEFAULT": 0.5,  "GROUP": "Color" },
    { "NAME": "grainAmt",    "LABEL": "Film Texture",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Color" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",        "TYPE": "float", "MIN": -0.5, "MAX": 0.5,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",       "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Color" }
  ],
  "PASSES": [
    { "TARGET": "uvBase" },
    { "TARGET": "uvFluid", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// UNVEILED
//   pass 0  uvBase  — keyed + lifted source (procedural walking figure
//                     when no texture is bound).
//   pass 1  uvFluid — self-advection: previous frame sampled along the
//                     8-tap brightness gradient + layered sine turbulence,
//                     blended back toward the source by alpha.
//   pass 2  final   — luminance → ember/blood/void palette, film texture,
//                     HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy

float hash11(float n) { return fract(sin(dot(vec2(n, n), vec2(12.9898, 78.233))) * 43758.5453); }
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float vnoise2(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1, 0)), f.x), mix(hash21(i + vec2(0, 1)), hash21(i + vec2(1, 1)), f.x), f.y);
}
float bassP()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.4); }
float midP()   { return pow(smoothstep(0.06, 0.85, audioMid),  1.2); }
float highP()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }
float beatP()  { return clamp(audioBeatPulse, 0.0, 1.0); }
float levP()   { return clamp(audioLevel, 0.0, 1.0); }
vec3 hueRotate(vec3 c, float h) {
    float a = h * 6.28318531; vec3 k = vec3(0.57735);
    float cs = cos(a), sn = sin(a);
    return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
}

// procedural fallback: a soft walking figure, keyed on a green field
vec4 fallbackSource(vec2 uv) {
    float t = TIME * 0.5;
    vec2 p = (uv - 0.5) * vec2(R.x / R.y, 1.0);
    p.x -= 0.35 * sin(t * 0.6);
    float bob = 0.02 * sin(t * 6.0);
    float head = length((p - vec2(0.0, 0.28 + bob)) * vec2(1.0, 1.15)) - 0.075;
    float torso = length(max(abs(p - vec2(0.0, 0.05 + bob)) - vec2(0.07, 0.13), 0.0)) - 0.04;
    float sw = sin(t * 6.0);
    float legA = length(max(abs((p - vec2(-0.045 + 0.05 * sw, -0.2)) * vec2(1.0, 0.6)) - vec2(0.02, 0.09), 0.0)) - 0.02;
    float legB = length(max(abs((p - vec2( 0.045 - 0.05 * sw, -0.2)) * vec2(1.0, 0.6)) - vec2(0.02, 0.09), 0.0)) - 0.02;
    float armA = length(max(abs((p - vec2(-0.11, 0.05 - 0.04 * sw)) * vec2(1.0, 0.7)) - vec2(0.015, 0.09), 0.0)) - 0.015;
    float armB = length(max(abs((p - vec2( 0.11, 0.05 + 0.04 * sw)) * vec2(1.0, 0.7)) - vec2(0.015, 0.09), 0.0)) - 0.015;
    float d = min(min(head, torso), min(min(legA, legB), min(armA, armB)));
    float fig = smoothstep(0.004, -0.004, d);
    vec3 skin = mix(vec3(0.55, 0.5, 0.5), vec3(0.75, 0.7, 0.68), vnoise2(p * 30.0 + t));
    vec3 col = mix(keyColor.rgb, skin, fig);
    return vec4(col, 1.0);
}

bool hasSource() {
    if (IMG_SIZE(inputTex).x < 0.5) return false;
    // an unbound / empty layer reads pure black: treat as no signal
    float m = 0.0;
    m = max(m, dot(IMG_NORM_PIXEL(inputTex, vec2(0.25, 0.25)).rgb, vec3(1.0)));
    m = max(m, dot(IMG_NORM_PIXEL(inputTex, vec2(0.75, 0.25)).rgb, vec3(1.0)));
    m = max(m, dot(IMG_NORM_PIXEL(inputTex, vec2(0.5, 0.5)).rgb, vec3(1.0)));
    m = max(m, dot(IMG_NORM_PIXEL(inputTex, vec2(0.25, 0.75)).rgb, vec3(1.0)));
    m = max(m, dot(IMG_NORM_PIXEL(inputTex, vec2(0.75, 0.75)).rgb, vec3(1.0)));
    return m > 0.002;
}
vec4 source(vec2 uv) {
    if (hasSource()) return IMG_NORM_PIXEL(inputTex, uv);
    return fallbackSource(uv);
}

vec2 turbulence(vec2 uv, float t) {
    vec2 turb = vec2(sin(uv.x), cos(uv.y));
    vec2 spd = vec2(1.0, 2.0) * turbSpeed;
    for (int i = 0; i < 10; i++) {
        float fi = 1.0 + float(i);
        float r1 = hash11(fi + 1.0) * 2.0 - 1.0;
        float r2 = hash11(fi + 100.0) * 2.0 - 1.0;
        vec2 t2;
        t2.x = sin(uv.x * (1.0 + r1 * turbDetail) + turb.y * 0.3 * fi + t * spd.x * r2);
        t2.y = cos(uv.y * (1.0 + r1 * turbDetail) + turb.x * 0.3 * fi + t * spd.y * r2);
        turb = mix(turb, t2, 0.5);
    }
    return turb;
}

float lum3(vec4 c) { return (c.x + c.y + c.z) / 3.0; }

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    float ar = audioReact;
    if (PASSINDEX == 0) {
        vec4 cam = source(uv);
        // key: colour key when Keying is up (or on the procedural figure),
        // otherwise a luminance key so dark backgrounds fall to the void
        float ka = hasSource() ? keyAmt : 1.0;
        float lumKey = smoothstep(0.04, 0.35, dot(cam.rgb, vec3(0.299, 0.587, 0.114)));
        float keying = mix(lumKey, smoothstep(0.0, 1.0, distance(cam.xyz, keyColor.rgb)), ka);
        vec3 c = clamp(cam.xyz + vec3(lift), 0.0, 1.0) * keying;
        gl_FragColor = vec4(c, keying);
    } else if (PASSINDEX == 1) {
        vec4 baseColor = texture2D(uvBase, uv);
        vec2 sDist = sampleDist / R;
        vec2 aspectUv = uv * vec2(R.x / R.y, 1.0);
        float tt = TIME * (1.0 + ar * 0.3 * beatP());
        vec2 turb = turbulence(aspectUv, tt);
        vec2 t = vec2(0.0);
        t += lum3(texture2D(uvFluid, uv + vec2( 1.0, 0.0) * sDist)) * vec2( 1.0, 0.0);
        t += lum3(texture2D(uvFluid, uv + vec2(-1.0, 0.0) * sDist)) * vec2(-1.0, 0.0);
        t += lum3(texture2D(uvFluid, uv + vec2( 0.0, 1.0) * sDist)) * vec2( 0.0, 1.0);
        t += lum3(texture2D(uvFluid, uv + vec2( 0.0,-1.0) * sDist)) * vec2( 0.0,-1.0);
        t += lum3(texture2D(uvFluid, uv + vec2( 1.0, 1.0) * sDist)) * vec2( 1.0, 1.0);
        t += lum3(texture2D(uvFluid, uv + vec2(-1.0, 1.0) * sDist)) * vec2(-1.0, 1.0);
        t += lum3(texture2D(uvFluid, uv + vec2( 1.0,-1.0) * sDist)) * vec2( 1.0,-1.0);
        t += lum3(texture2D(uvFluid, uv + vec2(-1.0,-1.0) * sDist)) * vec2(-1.0,-1.0);
        t /= 8.0;
        float dt = TIMEDELTA > 0.0001 ? min(TIMEDELTA, 0.05) : 1.0 / 60.0;
        float dif = diffusion * (1.0 + ar * 0.6 * bassP());
        float ta  = turbAmt * (1.0 + ar * 0.8 * beatP());
        vec2 dir = (t + turb * ta) * dt * dif * flow;
        vec4 res = texture2D(uvFluid, uv + dir);
        if (any(notEqual(res, res))) res = baseColor;
        if (FRAMEINDEX < 10) gl_FragColor = baseColor;
        else gl_FragColor = mix(res, baseColor, clamp(baseColor.a * fluidify + attenuate, 0.0, 1.0));
    } else {
        vec4 c = texture2D(uvFluid, uv);
        float l = clamp(lum3(c) * multiplier, 0.0, 1.0);
        l = mix(l, 1.0 - l, invert);
        vec4 res1 = mix(color1, color2, smoothstep(0.0, midPosition, l));
        vec4 res2 = mix(res1, color3, smoothstep(midPosition, 1.0, l));
        vec2 auv = uv * vec2(R.x / R.y, 1.0);
        float tex = vnoise2(auv * 90.0 + fract(TIME) * 13.0) * 0.6 + vnoise2(auv * 400.0 + fract(TIME * 7.0) * 31.0) * 0.4;
        tex = mix(0.5, tex, grainAmt * (1.0 + ar * 0.6 * highP()));
        vec3 col = res2.rgb * (0.9 + tex * 0.2);
        col = hueRotate(col, hueShift + ar * 0.04 * midP());
        float lvl = levP();
        col *= brightness * mix(1.0, 0.75 + 0.30 * lvl + 0.15 * bassP(), ar)
             * (1.0 + ar * 0.05 * beatP());
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
