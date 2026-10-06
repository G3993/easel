/*{
  "DESCRIPTION": "Moving Cloud — a volumetric cumulus bank drifting past a low sun. A front-to-back raymarch through a 5-octave value-noise fBM slab, lit with a single directional-derivative sample toward the sun (warm rim, cool shadow), fogged into a gradient sky with sun disc and glare. Written from scratch for the house (procedural noise, no textures). Wind, cloud cover, softness, sun position, sky and cloud palettes, camera orbit and quality are all live. Bass thickens the cover, beats gust the wind, mids warm the sun, highs crisp the rims.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-11 — original implementation of the classic fBM volumetric-cloud technique",
  "CATEGORIES": ["Generator", "3D", "Atmospheric", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "windSpeed",   "LABEL": "Wind Speed",      "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Cloud" },
    { "NAME": "windDir",     "LABEL": "Wind Direction",  "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.15, "GROUP": "Cloud" },
    { "NAME": "coverage",    "LABEL": "Cloud Cover",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.42, "GROUP": "Cloud" },
    { "NAME": "cloudScale",  "LABEL": "Cloud Scale",     "TYPE": "float", "MIN": 0.4,  "MAX": 2.5,  "DEFAULT": 1.0,  "GROUP": "Cloud" },
    { "NAME": "softness",    "LABEL": "Softness",        "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Cloud" },
    { "NAME": "detail",      "LABEL": "Detail Octaves",  "TYPE": "float", "MIN": 2.0,  "MAX": 6.0,  "DEFAULT": 6.0,  "GROUP": "Cloud" },
    { "NAME": "cloudTop",    "LABEL": "Cloud Top",       "TYPE": "float", "MIN": -0.5, "MAX": 2.0,  "DEFAULT": 0.6,  "GROUP": "Cloud" },
    { "NAME": "quality",     "LABEL": "March Steps",     "TYPE": "float", "MIN": 40.0, "MAX": 160.0, "DEFAULT": 110.0, "GROUP": "Cloud" },
    { "NAME": "sunAzimuth",  "LABEL": "Sun Azimuth",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.62, "GROUP": "Light" },
    { "NAME": "sunHeight",   "LABEL": "Sun Height",      "TYPE": "float", "MIN": -0.2, "MAX": 0.8,  "DEFAULT": 0.06, "GROUP": "Light" },
    { "NAME": "sunWarmth",   "LABEL": "Sun Warmth",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Light" },
    { "NAME": "sunGlare",    "LABEL": "Sun Glare",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Light" },
    { "NAME": "skyTop",      "LABEL": "Sky Top",         "TYPE": "color", "DEFAULT": [0.30, 0.47, 0.82, 1.0], "GROUP": "Light" },
    { "NAME": "skyHorizon",  "LABEL": "Sky Horizon",     "TYPE": "color", "DEFAULT": [0.70, 0.71, 0.80, 1.0], "GROUP": "Light" },
    { "NAME": "cloudLight",  "LABEL": "Cloud Lit",       "TYPE": "color", "DEFAULT": [1.0, 0.95, 0.86, 1.0], "GROUP": "Light" },
    { "NAME": "cloudShade",  "LABEL": "Cloud Shadow",    "TYPE": "color", "DEFAULT": [0.20, 0.24, 0.36, 1.0], "GROUP": "Light" },
    { "NAME": "camOrbit",    "LABEL": "Camera Orbit",    "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": 0.05, "GROUP": "Camera" },
    { "NAME": "camPitch",    "LABEL": "Camera Pitch",    "TYPE": "float", "MIN": -0.6, "MAX": 0.8,  "DEFAULT": 0.15, "GROUP": "Camera" },
    { "NAME": "camRoll",     "LABEL": "Camera Sway",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Camera" },
    { "NAME": "lens",        "LABEL": "Lens",            "TYPE": "float", "MIN": 0.8,  "MAX": 3.0,  "DEFAULT": 1.5,  "GROUP": "Camera" },
    { "NAME": "contrast",    "LABEL": "Contrast",        "TYPE": "float", "MIN": 0.5,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Look" },
    { "NAME": "brightness",  "LABEL": "Brightness",      "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Look" },
    { "NAME": "audioReact",  "LABEL": "Audio React",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.05, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.2,  "GROUP": "Depth / Passes" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "mcScene" },
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// MOVING CLOUD — original house implementation.
//   Density: fBM of 3-D value noise (procedural hash, no textures) over a
//   horizontal slab, thresholded by coverage with a soft knee. Marched
//   front-to-back with distance-scaled steps and octave LOD by distance;
//   lighting = one extra density sample toward the sun (directional
//   derivative) → lit/shadow palette mix; distance fog into the sky.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define TAU 6.28318531

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float hash31(vec3 p) {
    p = fract(p * vec3(0.1031, 0.1030, 0.0973));
    p += dot(p, p.yxz + 33.33);
    return fract((p.x + p.y) * p.z);
}
float vnoise3(vec3 x) {
    vec3 p = floor(x), f = fract(x);
    f = f * f * (3.0 - 2.0 * f);
    float n000 = hash31(p), n100 = hash31(p + vec3(1, 0, 0));
    float n010 = hash31(p + vec3(0, 1, 0)), n110 = hash31(p + vec3(1, 1, 0));
    float n001 = hash31(p + vec3(0, 0, 1)), n101 = hash31(p + vec3(1, 0, 1));
    float n011 = hash31(p + vec3(0, 1, 1)), n111 = hash31(p + vec3(1, 1, 1));
    return mix(mix(mix(n000, n100, f.x), mix(n010, n110, f.x), f.y),
               mix(mix(n001, n101, f.x), mix(n011, n111, f.x), f.y), f.z) * 2.0 - 1.0;
}

float bassP()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.4); }
float midP()   { return pow(smoothstep(0.06, 0.85, audioMid),  1.2); }
float highP()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }

vec3 windOffset() {
    // beats gust the wind: phase-push on the integrated bass clock
    float w = TIME * windSpeed + audioReact * 0.5 * audioBassTime;
    float a = windDir * TAU;
    return vec3(cos(a), 0.08, sin(a)) * w * 0.35;
}

float density(vec3 p, int oct) {
    vec3 q = (p - windOffset()) * cloudScale;
    float f = 0.0, a = 0.5;
    float cov = coverage + audioReact * 0.18 * bassP();
    for (int i = 0; i < 6; i++) {
        if (i >= oct) break;
        f += a * vnoise3(q);
        q = q * 2.02 + vec3(1.7, 9.2, 3.1);
        a *= 0.5;
    }
    // slab profile: dense low, thinning to cloudTop
    float slab = 1.0 - smoothstep(cloudTop - 1.2, cloudTop, p.y);
    float base = 1.0 * f + (cov - 0.5) * 1.6 - (p.y - cloudTop + 1.6) * 0.42;
    float knee = 0.22 + 0.55 * softness;
    return clamp(base / knee, 0.0, 1.0) * slab;
}

vec3 sunDir() {
    float az = sunAzimuth * TAU;
    return normalize(vec3(cos(az) * (1.0 - sunHeight), sunHeight, sin(az) * (1.0 - sunHeight)));
}

vec3 sky(vec3 rd, vec3 sd) {
    float sun = clamp(dot(sd, rd), 0.0, 1.0);
    vec3 col = mix(skyHorizon.rgb, skyTop.rgb, smoothstep(-0.1, 0.6, rd.y));
    vec3 warm = mix(vec3(1.0), vec3(1.0, 0.6, 0.25), sunWarmth * (0.6 + audioReact * 0.3 * midP()));
    col += 0.18 * warm * pow(sun, 8.0);
    col += 0.9 * warm * smoothstep(0.9985, 0.9995, sun);    // sun disc
    return col;
}

vec4 march(vec3 ro, vec3 rd, vec3 bg, vec3 sd) {
    vec4 sum = vec4(0.0);
    float dither = hash21(gl_FragCoord.xy + fract(TIME) * 13.0);
    float t = 0.05 + 0.1 * dither;
    int steps = int(quality);
    int oct = int(detail);
    vec3 lit = cloudLight.rgb;
    vec3 shd = cloudShade.rgb;
    vec3 warm = mix(vec3(1.0), vec3(1.0, 0.62, 0.32), sunWarmth);
    float rim = 0.34 - 0.14 * audioReact * highP();          // highs crisp the rims
    for (int i = 0; i < 160; i++) {
        if (i >= steps) break;
        vec3 pos = ro + t * rd;
        if (pos.y < -3.5 || sum.a > 0.99 || t > 60.0) break;
        if (pos.y > cloudTop + 1.5 && rd.y > 0.0) break;        // left the slab upward
        if (pos.y > cloudTop + 0.3) { t += max(0.06, 0.05 * t); continue; }   // above: keep flying
        int lod = oct - int(log2(1.0 + t * 0.5));
        if (lod < 2) lod = 2;
        float den = density(pos, lod);
        if (den > 0.01) {
            float dif = clamp((den - density(pos + 0.3 * sd, lod)) / rim, 0.0, 1.0);
            vec3 lin = warm * 1.4 * dif + vec3(0.34, 0.40, 0.54);
            vec4 col = vec4(mix(lit, shd, smoothstep(0.0, 0.9, den)), den);
            col.rgb *= lin;
            col.rgb = mix(col.rgb, bg, 1.0 - exp(-0.0015 * t * t));
            col.a *= 0.55;
            col.rgb *= col.a;
            sum += col * (1.0 - sum.a);
        }
        t += max(0.06, 0.05 * t);
    }
    return clamp(sum, 0.0, 1.0);
}

mat3 lookAt(vec3 ro, vec3 ta, float cr) {
    vec3 cw = normalize(ta - ro);
    vec3 cp = vec3(sin(cr), cos(cr), 0.0);
    vec3 cu = normalize(cross(cw, cp));
    vec3 cv = normalize(cross(cu, cw));
    return mat3(cu, cv, cw);
}

vec3 renderScene() {
    vec2 p = (2.0 * gl_FragCoord.xy - R) / R.y;
    float orb = camOrbit * TIME * 0.12;
    vec3 ro = 4.0 * normalize(vec3(sin(orb), 0.8 * camPitch + 0.25, cos(orb))) - vec3(0.0, 0.1, 0.0);
    vec3 ta = vec3(0.0, -1.0 + camPitch * 1.5, 0.0);
    mat3 ca = lookAt(ro, ta, 0.07 * camRoll * cos(0.25 * TIME));
    vec3 rd = ca * normalize(vec3(p, lens));
    vec3 sd = sunDir();

    vec3 col = sky(rd, sd);
    vec4 res = march(ro, rd, col, sd);
    col = col * (1.0 - res.a) + res.rgb;
    float sun = clamp(dot(sd, rd), 0.0, 1.0);
    col += vec3(0.2, 0.08, 0.04) * pow(sun, 3.0) * sunGlare * 1.2;
    col = mix(vec3(0.5), col, contrast);
    col += (hash21(gl_FragCoord.xy + fract(TIME) * vec2(17.0, 29.0)) - 0.5) * 0.01;
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        gl_FragColor = vec4(clamp(renderScene(), 0.0, 1.0), 1.0);
    } else if (PASSINDEX == 1) {
        vec3 col  = texture2D(mcScene, uv).rgb;
        vec3 prev = texture2D(abTrail, uv).rgb;
        if (any(notEqual(prev, prev))) prev = vec3(0.0);
        float decay = 0.50 + 0.46 * trailAmt;
        col = max(col, prev * decay - 0.0045);
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else {
        vec2 dir = uv - 0.5;
        float ab = aberration * 0.0045;
        vec3 base;
        base.r = texture2D(abTrail, uv + dir * ab).r;
        base.g = texture2D(abTrail, uv).g;
        base.b = texture2D(abTrail, uv - dir * ab).b;
        vec3 bl = vec3(0.0);
        for (int i = 0; i < 8; i++) {
            float an = float(i) * 0.7853982;
            vec2 o = vec2(cos(an), sin(an)) * (3.5 / R.y);
            bl += texture2D(abTrail, uv + o).rgb;
            bl += texture2D(abTrail, uv + o * 2.6).rgb * 0.6;
        }
        bl /= 12.8;
        bl = max(bl - 0.6, 0.0);
        vec3 col = base + bl * bl * bloomAmt * 1.4;
        float lvl = clamp(audioLevel, 0.0, 1.0);
        col *= brightness * mix(1.0, 0.80 + 0.20 * lvl, audioReact);
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.5);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.18), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
