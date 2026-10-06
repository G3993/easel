/*{
  "DESCRIPTION": "Mirror — a glass mandelbulb: rays refract into the fractal, bounce and refract back out with Beer-Lambert absorption, reflecting a glowing floor slab and a sky column so every lobe reads as polished mirror glass. FXAA final pass keeps the fractal edges clean. The bulb slowly tumbles and its phase term animates. Bass swells the glow, beats push the tumble, mids drift the hue, highs brighten the sky reflections. Every parameter is exposed.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-23 — port of 'Inside the mandelbulb II' by mrange (CC0), FXAA by XorDev, mandelbulb by EvilRyu",
  "CATEGORIES": ["Generator", "3D", "Fractal", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "tumble",      "LABEL": "Tumble Speed",     "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "phaseSpeed",  "LABEL": "Phase Animate",    "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "power",       "LABEL": "Power",            "TYPE": "float", "MIN": 2.0,  "MAX": 12.0, "DEFAULT": 8.0,  "GROUP": "Fractal" },
    { "NAME": "loops",       "LABEL": "Iterations",       "TYPE": "float", "MIN": 2.0,  "MAX": 7.0,  "DEFAULT": 5.0,  "GROUP": "Fractal" },
    { "NAME": "scale",       "LABEL": "Scale",            "TYPE": "float", "MIN": 1.0,  "MAX": 3.5,  "DEFAULT": 2.0,  "GROUP": "Fractal" },
    { "NAME": "bounces",     "LABEL": "Bounces",          "TYPE": "float", "MIN": 1.0,  "MAX": 6.0,  "DEFAULT": 5.0,  "GROUP": "Glass" },
    { "NAME": "marches",     "LABEL": "March Steps",      "TYPE": "float", "MIN": 6.0,  "MAX": 24.0, "DEFAULT": 10.0, "GROUP": "Glass" },
    { "NAME": "ior",         "LABEL": "Refraction",       "TYPE": "float", "MIN": 1.0,  "MAX": 1.3,  "DEFAULT": 1.05, "GROUP": "Glass" },
    { "NAME": "absorb",      "LABEL": "Absorption",       "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Glass" },
    { "NAME": "reflectAmt",  "LABEL": "Reflectivity",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Glass" },
    { "NAME": "transmit",    "LABEL": "Transmission",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.8,  "GROUP": "Glass" },
    { "NAME": "camDist",     "LABEL": "Camera Distance",  "TYPE": "float", "MIN": 0.3,  "MAX": 1.5,  "DEFAULT": 0.6,  "GROUP": "Camera" },
    { "NAME": "camHeight",   "LABEL": "Camera Height",    "TYPE": "float", "MIN": 0.0,  "MAX": 4.0,  "DEFAULT": 2.0,  "GROUP": "Camera" },
    { "NAME": "hueOffset",   "LABEL": "Hue",              "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "skyAmt",      "LABEL": "Sky Brightness",   "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "glowAmt",     "LABEL": "Glass Glow",       "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "exposure",    "LABEL": "Exposure",         "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "saturation",  "LABEL": "Saturation",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.6,  "DEFAULT": 1.05, "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",       "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "fxaaAmt",     "LABEL": "FXAA",             "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Color" }
  ],
  "PASSES": [
    { "TARGET": "mrScene" },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// MIRROR
//   pass 0  mrScene — glass mandelbulb: multi-bounce reflect/refract march
//                     with Beer absorption, floor slab + sky column env.
//   pass 1  final   — FXAA (XorDev) + HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define PI  3.141592654
#define TAU 6.283185307
#define TOLERANCE 0.0001
#define MAX_RAY_LENGTH 5.0
#define NORM_OFF 0.005
#define initt 0.1

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float bassP()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.4); }
float midP()   { return pow(smoothstep(0.06, 0.85, audioMid),  1.2); }
float highP()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }
float beatP()  { return clamp(audioBeatPulse, 0.0, 1.0); }
float levP()   { return clamp(audioLevel, 0.0, 1.0); }

// sam hocevar hsv2rgb (WTFPL)
vec3 hsv2rgb(vec3 c) {
    const vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

mat3 g_rot = mat3(1.0);
vec3 g_skyCol, g_diffuseCol, g_beer;
float g_phase;
int g_loops, g_marches;

vec3 aces_approx(vec3 v) {
    v = max(v, 0.0);
    v *= 0.6;
    float a = 2.51, b = 0.03, c = 2.43, d = 0.59, e = 0.14;
    return clamp((v * (a * v + b)) / (v * (c * v + d) + e), 0.0, 1.0);
}
vec3 sRGB(vec3 t) { return mix(1.055 * pow(max(t, 0.0), vec3(1.0 / 2.4)) - 0.055, 12.92 * t, step(t, vec3(0.0031308))); }

float box(vec2 p, vec2 b) { vec2 d = abs(p) - b; return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0); }
float rayPlane(vec3 ro, vec3 rd, vec4 p) { return -(dot(ro, p.xyz) + p.w) / dot(rd, p.xyz); }

// EvilRyu mandelbulb
float mandelBulb(vec3 p) {
    vec3 z = p;
    float r = 0.0, theta, phi;
    float dr = 1.0;
    for (int i = 0; i < 7; i++) {
        if (i >= g_loops) break;
        r = length(z);
        if (r > 2.0) continue;
        theta = atan(z.y, z.x);
        phi = asin(clamp(z.z / r, -1.0, 1.0)) + g_phase;
        dr = pow(r, power - 1.0) * dr * power + 1.0;
        r = pow(r, power);
        theta *= power;
        phi *= power;
        z = r * vec3(cos(theta) * cos(phi), sin(theta) * cos(phi), sin(phi)) + p;
    }
    return 0.5 * log(max(r, 1e-6)) * r / dr;
}

mat3 rot_x(float a) { float c = cos(a), s = sin(a); return mat3(1, 0, 0, 0, c, s, 0, -s, c); }
mat3 rot_y(float a) { float c = cos(a), s = sin(a); return mat3(c, 0, s, 0, 1, 0, -s, 0, c); }

vec3 skyColor(vec3 ro, vec3 rd) {
    vec3 col = clamp(vec3(0.0025 / abs(rd.y)) * g_skyCol, 0.0, 1.0);
    float tp0 = rayPlane(ro, rd, vec4(vec3(0.0, 1.0, 0.0), 4.0));
    float tp1 = rayPlane(ro, rd, vec4(vec3(0.0, -1.0, 0.0), 6.0));
    if (tp1 > 0.0) {
        vec3 pos = ro + tp1 * rd;
        float db = box(pos.xz, vec2(6.0, 9.0)) - 1.0;
        col += vec3(4.0) * g_skyCol * rd.y * rd.y * smoothstep(0.25, 0.0, db);
        col += vec3(0.8) * g_skyCol * exp(-0.5 * max(db, 0.0));
    }
    if (tp0 > 0.0) {
        vec3 pos = ro + tp0 * rd;
        float ds = length(pos.xz) - 0.5;
        col += vec3(0.25) * g_skyCol * exp(-0.5 * max(ds, 0.0));
    }
    return clamp(col * skyAmt, 0.0, 10.0);
}

float df(vec3 p) {
    p *= g_rot;
    return mandelBulb(p / scale) * scale;
}

vec3 normal(vec3 pos) {
    vec2 eps = vec2(NORM_OFF, 0.0);
    vec3 nor;
    nor.x = df(pos + eps.xyy) - df(pos - eps.xyy);
    nor.y = df(pos + eps.yxy) - df(pos - eps.yxy);
    nor.z = df(pos + eps.yyx) - df(pos - eps.yyx);
    return normalize(nor);
}

float rayMarch(vec3 ro, vec3 rd, float dfactor) {
    float t = 0.0;
    for (int i = 0; i < 24; i++) {
        if (i >= g_marches) break;
        if (t > MAX_RAY_LENGTH) { t = MAX_RAY_LENGTH; break; }
        float d = dfactor * df(ro + rd * t);
        if (d < TOLERANCE) break;
        t += d;
    }
    return t;
}

vec3 render(vec3 ro, vec3 rd) {
    vec3 agg = vec3(0.0);
    vec3 ragg = vec3(1.0);
    bool isInside = df(ro) < 0.0;
    int nb = int(bounces + 0.5);
    for (int bounce = 0; bounce < 6; bounce++) {
        if (bounce >= nb) break;
        float dfactor = isInside ? -1.0 : 1.0;
        float mragg = min(min(ragg.x, ragg.y), ragg.z);
        if (mragg < 0.025) break;
        float st = rayMarch(ro, rd, dfactor);
        if (st >= MAX_RAY_LENGTH) { agg += ragg * skyColor(ro, rd); break; }
        vec3 sp = ro + rd * st;
        vec3 sn = dfactor * normal(sp);
        float fre = 1.0 + dot(rd, sn);
        fre *= fre;
        fre = mix(0.1, 1.0, fre);
        vec3 ld = normalize(vec3(0.0, 10.0, 0.0) - sp);
        float dif = max(dot(ld, sn), 0.0);
        vec3 ref = reflect(rd, sn);
        float re = ior;
        vec3 refr = refract(rd, sn, !isInside ? re : 1.0 / re);
        vec3 rsky = skyColor(sp, ref);
        vec3 col = vec3(0.0);
        col += g_diffuseCol * dif * dif * (1.0 - transmit);
        float edge = smoothstep(1.0, 0.9, fre);
        col += rsky * reflectAmt * fre * edge;
        if (isInside) ragg *= exp(-st * g_beer);
        agg += ragg * col;
        if (refr == vec3(0.0)) {
            rd = ref;
        } else {
            ragg *= transmit;
            isInside = !isInside;
            rd = refr;
        }
        ro = sp + initt * rd;
    }
    return agg;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    float ar = audioReact;
    if (PASSINDEX == 0) {
        float t = TIME * tumble + ar * 0.4 * audioBassTime;
        g_rot = rot_x(0.2 * t) * rot_y(0.3 * t);
        g_phase = TIME * 0.2 * phaseSpeed;
        g_loops = int(loops + 0.5);
        g_marches = int(marches + 0.5);
        float hoff = hueOffset + ar * 0.05 * midP();
        g_skyCol     = hsv2rgb(vec3(hoff + 0.6, 0.50, 1.0)) * (1.0 + ar * 0.5 * highP());
        g_diffuseCol = hsv2rgb(vec3(hoff + 0.6, 0.85, 1.0));
        vec3 gcol    = hsv2rgb(vec3(hoff + 0.05, 0.95, 2.0)) * glowAmt * (1.0 + ar * 0.5 * bassP());
        g_beer = -gcol * absorb;

        vec2 q = uv;
        vec2 p = -1.0 + 2.0 * q;
        p.x *= R.x / R.y;
        vec3 ro = camDist * vec3(0.0, camHeight, 5.0);
        vec3 la = vec3(0.0);
        vec3 up = vec3(0.0, 1.0, 0.0);
        vec3 ww = normalize(la - ro);
        vec3 uu = normalize(cross(up, ww));
        vec3 vv = normalize(cross(ww, uu));
        float fov = tan(TAU / 6.0);
        vec3 rd = normalize(-p.x * uu + p.y * vv + fov * ww);
        vec3 col = render(ro, rd) * exposure;
        col = aces_approx(col);
        col = sRGB(col);
        gl_FragColor = vec4(col, 1.0);
    } else {
        // FXAA (XorDev, GM_FXAA)
        vec2 texelSz = sqrt(2.0) / R;
        const float span_max = 8.0, reduce_min = 1.0 / 128.0, reduce_mul = 1.0 / 32.0;
        const vec3 luma = vec3(0.299, 0.587, 0.114);
        vec3 rgbCC = texture2D(mrScene, uv).rgb;
        vec3 rgb00 = texture2D(mrScene, uv + vec2(-0.5, -0.5) * texelSz).rgb;
        vec3 rgb10 = texture2D(mrScene, uv + vec2( 0.5, -0.5) * texelSz).rgb;
        vec3 rgb01 = texture2D(mrScene, uv + vec2(-0.5,  0.5) * texelSz).rgb;
        vec3 rgb11 = texture2D(mrScene, uv + vec2( 0.5,  0.5) * texelSz).rgb;
        float lumaCC = dot(rgbCC, luma), luma00 = dot(rgb00, luma), luma10 = dot(rgb10, luma);
        float luma01 = dot(rgb01, luma), luma11 = dot(rgb11, luma);
        vec2 dir = vec2((luma01 + luma11) - (luma00 + luma10), (luma00 + luma01) - (luma10 + luma11));
        float dirReduce = max((luma00 + luma10 + luma01 + luma11) * reduce_mul, reduce_min);
        float rcpDir = 1.0 / (min(abs(dir.x), abs(dir.y)) + dirReduce);
        dir = clamp(dir * rcpDir, -span_max, span_max) * texelSz;
        vec3 A = 0.5 * (texture2D(mrScene, uv - dir * (1.0 / 6.0)).rgb + texture2D(mrScene, uv + dir * (1.0 / 6.0)).rgb);
        vec3 B = A * 0.5 + 0.25 * (texture2D(mrScene, uv - dir * 0.3).rgb + texture2D(mrScene, uv + dir * 0.3).rgb);
        float lumaMin = min(lumaCC, min(min(luma00, luma10), min(luma01, luma11)));
        float lumaMax = max(lumaCC, max(max(luma00, luma10), max(luma01, luma11)));
        float lumaB = dot(B, luma);
        vec3 aa = ((lumaB < lumaMin) || (lumaB > lumaMax)) ? A : B;
        vec3 col = mix(rgbCC, aa, fxaaAmt);

        float lvl = levP();
        col *= brightness * mix(1.0, 0.74 + 0.30 * lvl + 0.14 * bassP(), ar)
             * (1.0 + ar * 0.05 * beatP());
        float l0 = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(vec3(l0), col, saturation);
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.45);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.18), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
