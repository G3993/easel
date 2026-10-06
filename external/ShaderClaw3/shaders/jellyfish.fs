/*{
  "DESCRIPTION": "Jellyfish — a raymarched medusa in deep water: a translucent pulsing bell with a frilled rim and a fine ripple skin, four glowing gonad rings inside, four oral arms and a ring of long sinuous tentacles trailing below, bioluminescent sparkle in the water and light shafts falling from the surface. Pass 0 renders the scene with the glow in alpha, pass 1 blooms it and applies the HD finisher. Bass drives the bell's contraction, beats flash the bioluminescence, mids sway the tentacles, highs add sparkle.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-30 — original",
  "CATEGORIES": ["Generator", "3D", "Organic", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "pulseRate",   "LABEL": "Pulse Rate",       "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "pulseDepth",  "LABEL": "Pulse Depth",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Motion" },
    { "NAME": "swayAmt",     "LABEL": "Tentacle Sway",    "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "drift",       "LABEL": "Drift",            "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "orbit",       "LABEL": "Camera Orbit",     "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.6,  "GROUP": "Motion" },
    { "NAME": "camDist",     "LABEL": "Camera Distance",  "TYPE": "float", "MIN": 2.0,  "MAX": 8.0,  "DEFAULT": 4.2,  "GROUP": "Camera" },
    { "NAME": "camHeight",   "LABEL": "Camera Height",    "TYPE": "float", "MIN": -2.0, "MAX": 2.0,  "DEFAULT": 0.2,  "GROUP": "Camera" },
    { "NAME": "bellSize",    "LABEL": "Bell Size",        "TYPE": "float", "MIN": 0.5,  "MAX": 1.6,  "DEFAULT": 1.0,  "GROUP": "Body" },
    { "NAME": "frill",       "LABEL": "Rim Frill",        "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Body" },
    { "NAME": "skinDetail",  "LABEL": "Skin Detail",      "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Body" },
    { "NAME": "tentacles",   "LABEL": "Tentacles",        "TYPE": "float", "MIN": 6.0,  "MAX": 48.0, "DEFAULT": 24.0, "GROUP": "Body" },
    { "NAME": "tentLen",     "LABEL": "Tentacle Length",  "TYPE": "float", "MIN": 0.5,  "MAX": 4.0,  "DEFAULT": 2.6,  "GROUP": "Body" },
    { "NAME": "armLen",      "LABEL": "Oral Arm Length",  "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,  "DEFAULT": 1.6,  "GROUP": "Body" },
    { "NAME": "hue",         "LABEL": "Body Hue",         "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.86, "GROUP": "Color" },
    { "NAME": "hue2",        "LABEL": "Glow Hue",         "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.52, "GROUP": "Color" },
    { "NAME": "glowAmt",     "LABEL": "Bioluminescence",  "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "translucency","LABEL": "Translucency",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.7,  "GROUP": "Color" },
    { "NAME": "shafts",      "LABEL": "Light Shafts",     "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.8,  "GROUP": "Water" },
    { "NAME": "plankton",    "LABEL": "Plankton",         "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Water" },
    { "NAME": "waterTint",   "LABEL": "Water Depth",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Water" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom",            "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "exposure",    "LABEL": "Exposure",         "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "saturation",  "LABEL": "Saturation",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.6,  "DEFAULT": 1.1,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",       "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Color" }
  ],
  "PASSES": [
    { "TARGET": "jfScene" },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// JELLYFISH
//   pass 0  jfScene — raymarched medusa. The bell is a thin translucent
//                     shell: the ray hits its front, takes its colour at the
//                     Fresnel-weighted amount, steps THROUGH it and keeps
//                     marching for the gonads, arms, tentacles and the back of
//                     the bell, then composites. Alpha = emissive weight.
//   pass 1  final   — bloom from the emissive weight + HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R    RENDERSIZE.xy
#define PI   3.14159265359
#define TAU  6.28318530718

float bassP()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.4); }
float midP()   { return pow(smoothstep(0.06, 0.85, audioMid),  1.2); }
float highP()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }
float beatP()  { return clamp(audioBeatPulse, 0.0, 1.0); }
float levP()   { return clamp(audioLevel, 0.0, 1.0); }

// ── hashes / noise (float only) ──────────────────────────────────────────
float hash11(float p) { p = fract(p * 0.1031); p *= p + 33.33; p *= p + p; return fract(p); }
float hash12(vec2 p)  { vec3 p3 = fract(vec3(p.xyx) * 0.1031); p3 += dot(p3, p3.yzx + 33.33); return fract((p3.x + p3.y) * p3.z); }
float hash13(vec3 p3) { p3 = fract(p3 * 0.1031); p3 += dot(p3, p3.zyx + 31.32); return fract((p3.x + p3.y) * p3.z); }
vec3  hash33(vec3 p3) { p3 = fract(p3 * vec3(0.1031, 0.1030, 0.0973)); p3 += dot(p3, p3.yxz + 33.33); return fract((p3.xxy + p3.yxx) * p3.zyx); }
float vnoise2(vec2 x) {
    vec2 p = floor(x), f = fract(x); f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash12(p), hash12(p + vec2(1, 0)), f.x), mix(hash12(p + vec2(0, 1)), hash12(p + vec2(1, 1)), f.x), f.y);
}
float vnoise3(vec3 x) {
    vec3 p = floor(x), f = fract(x); f = f * f * (3.0 - 2.0 * f);
    return mix(mix(mix(hash13(p), hash13(p + vec3(1, 0, 0)), f.x), mix(hash13(p + vec3(0, 1, 0)), hash13(p + vec3(1, 1, 0)), f.x), f.y),
               mix(mix(hash13(p + vec3(0, 0, 1)), hash13(p + vec3(1, 0, 1)), f.x), mix(hash13(p + vec3(0, 1, 1)), hash13(p + vec3(1, 1, 1)), f.x), f.y), f.z);
}
float fbm3(vec3 p) { return vnoise3(p) * 0.5 + vnoise3(p * 2.03 + 1.7) * 0.25 + vnoise3(p * 4.1 + 3.1) * 0.125 + vnoise3(p * 8.3 + 5.2) * 0.0625; }
vec3 hsv2rgb(vec3 c) { vec3 p = abs(fract(c.xxx + vec3(0.0, 2.0 / 3.0, 1.0 / 3.0)) * 6.0 - 3.0); return c.z * mix(vec3(1.0), clamp(p - 1.0, 0.0, 1.0), c.y); }
mat2 rot(float a) { float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }

// ── animation state (per pixel, cheap) ──────────────────────────────────
float g_ar, g_t, g_pulse, g_sway, g_glowK;
vec3  g_bodyCol, g_glowCol, g_tentCol;

// the bell's contraction: a sharp squeeze, slow relax — like the real thing
float pulseCurve(float t) {
    float ph = fract(t);
    float squeeze = exp(-pow((ph - 0.18) * 5.5, 2.0));          // quick contraction
    float relax   = smoothstep(0.25, 1.0, ph) * (1.0 - smoothstep(0.25, 1.0, ph)) * 0.6;
    return squeeze - relax * 0.4;
}

// ── SDF helpers ──────────────────────────────────────────────────────────
float sdSphere(vec3 p, float r) { return length(p) - r; }
float sdTorus(vec3 p, vec2 t) { vec2 q = vec2(length(p.xz) - t.x, p.y); return length(q) - t.y; }
float smin(float a, float b, float k) { float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0); return mix(b, a, h) - k * h * (1.0 - h); }

// material ids: 1 bell, 2 gonad, 3 arm, 4 tentacle
float g_mat;

// bell: an ellipsoid shell, squeezed by the pulse, frilled and rippled at the rim
float sdBell(vec3 p) {
    float sq = g_pulse;                                   // 0..1 contraction
    vec3 s = vec3(1.0 - 0.16 * sq, 0.78 + 0.22 * sq, 1.0 - 0.16 * sq) * bellSize;
    vec3 q = p / s;
    float ang = atan(q.z, q.x);
    float rimW = smoothstep(0.35, -0.25, q.y);            // 1 at the rim, 0 at the apex
    // frill: 12 lobes around the rim, each lobe rippling with the pulse
    float fr = sin(ang * 12.0 + sin(ang * 3.0 + g_t * 0.7) * 0.8) * 0.045 * frill * rimW;
    fr += sin(ang * 36.0 - g_t * 1.3) * 0.012 * frill * rimW;
    // skin: fine ripple detail travelling down the bell
    float skin = (vnoise3(q * 9.0 + vec3(0.0, g_t * 0.35, 0.0)) - 0.5) * 0.02 * skinDetail
               + (vnoise3(q * 26.0 - vec3(0.0, g_t * 0.6, 0.0)) - 0.5) * 0.006 * skinDetail;
    float outer = length(q) - 1.0 - fr - skin;
    // hollow underside: carve a slightly smaller sphere offset downward
    float inner = length(q - vec3(0.0, -0.18, 0.0)) - 0.9 - fr * 0.6 - skin;
    float shell = max(outer, -inner);
    // cut the bell off below the rim line (flat-ish opening)
    float cut = -(q.y + 0.42 - fr * 1.5);
    shell = max(shell, cut);
    return shell * min(s.x, min(s.y, s.z));
}
// four gonad rings (horseshoe tori) hanging inside the bell
float sdGonads(vec3 p) {
    vec3 q = p / bellSize;
    float ang = atan(q.z, q.x);
    float sec = TAU / 4.0;
    float a2 = mod(ang + sec * 0.5, sec) - sec * 0.5;
    float rr = length(q.xz);
    vec3 l = vec3(rr * cos(a2) - 0.36, q.y - 0.22 + 0.06 * sin(g_t * 2.0 + ang), rr * sin(a2));
    l.xz *= rot(0.6);
    float d = sdTorus(l.xzy, vec2(0.13, 0.035 + 0.008 * sin(ang * 9.0 + g_t * 3.0)));
    return d * bellSize;
}
// a wavy strand hanging from (ox, oy, oz): distance from p to the curve x(y)
float strand(vec3 p, vec3 origin, float len, float radius, float waveAmp, float seed, float taper) {
    vec3 q = p - origin;
    float yy = clamp(-q.y, 0.0, len);                     // distance down the strand
    float u = yy / len;
    float ph = g_t * 1.6 + seed * 6.28;
    vec2 off = vec2(sin(yy * 2.4 - ph + seed * 3.0), cos(yy * 1.9 - ph * 0.8 + seed * 5.0)) * waveAmp * (0.25 + u) * g_sway;
    off += vec2(sin(yy * 6.0 + ph * 1.7 + seed), cos(yy * 5.3 - ph * 1.3)) * waveAmp * 0.12 * u;   // micro wiggle
    // closest point on the strand (param clamped to its length) → a true
    // capsule-like distance, so nothing leaks above the root or past the tip
    vec3 c = vec3(off.x, -yy, off.y);
    float r = radius * mix(1.0, taper, u);
    return length(q - c) - r;
}
// four oral arms: thick frilled ribbons under the bell
float sdArms(vec3 p) {
    float best = 1e9;
    for (int i = 0; i < 4; i++) {
        float a = float(i) * TAU / 4.0 + 0.4;
        vec3 o = vec3(cos(a) * 0.14, -0.30, sin(a) * 0.14) * bellSize;
        float d = strand(p, o, armLen * bellSize, 0.085 * bellSize, 0.22, float(i) * 0.37, 0.35);
        // ruffle along the arm
        d -= (vnoise3(p * 11.0 + float(i)) - 0.5) * 0.03 * bellSize;
        best = smin(best, d, 0.06);
    }
    return best;
}
// the tentacle ring: domain-repeated in angle so N tentacles cost one
float sdTentacles(vec3 p) {
    float n = floor(tentacles + 0.5);
    float sec = TAU / n;
    float ang = atan(p.z, p.x);
    float id = floor((ang + sec * 0.5) / sec);
    float a2 = ang - id * sec;
    float rr = length(p.xz);
    vec3 q = vec3(rr * cos(a2), p.y, rr * sin(a2));
    float seed = hash11(id + 7.0);
    float rimR = (0.96 - 0.14 * g_pulse) * bellSize;
    vec3 o = vec3(rimR, -0.40 * bellSize, 0.0);
    float len = tentLen * bellSize * (0.7 + 0.5 * seed);
    float d = strand(q, o, len, 0.016 * bellSize, 0.28 + 0.2 * seed, seed, 0.25);
    // beads along the tentacle (nematocyst batteries) — only along its length
    float along = clamp(-(q.y - o.y), 0.0, len);
    float beads = sin(along * 14.0 + seed * 9.0) * 0.5 + 0.5;
    d -= beads * beads * 0.0035 * bellSize * step(0.01, along) * step(along, len - 0.01);
    return d;
}

float map(vec3 p) {
    float dBell = sdBell(p);
    float dGon  = sdGonads(p);
    float dArm  = sdArms(p);
    float dTen  = sdTentacles(p);
    float d = dBell; g_mat = 1.0;
    if (dGon < d) { d = dGon; g_mat = 2.0; }
    if (dArm < d) { d = dArm; g_mat = 3.0; }
    if (dTen < d) { d = dTen; g_mat = 4.0; }
    return d;
}
vec3 calcNormal(vec3 p, float e) {
    vec2 k = vec2(1.0, -1.0) * e;
    return normalize(k.xyy * map(p + k.xyy) + k.yyx * map(p + k.yyx) + k.yxy * map(p + k.yxy) + k.xxx * map(p + k.xxx));
}
// bell "thickness" glow: how much translucent flesh light passes through
float shellGlow(vec3 p) {
    float sq = g_pulse;
    vec3 s = vec3(1.0 - 0.16 * sq, 0.78 + 0.22 * sq, 1.0 - 0.16 * sq) * bellSize;
    vec3 q = p / s;
    return smoothstep(-0.35, 0.9, q.y);                  // brighter toward the apex
}

// ── water ────────────────────────────────────────────────────────────────
vec3 waterBg(vec3 rd, vec2 uv) {
    float up = clamp(rd.y * 0.5 + 0.5, 0.0, 1.0);
    vec3 deep = mix(vec3(0.0, 0.004, 0.012), vec3(0.0, 0.03, 0.06), waterTint);
    vec3 shallow = mix(vec3(0.0, 0.09, 0.16), vec3(0.02, 0.22, 0.30), waterTint);
    vec3 col = mix(deep, shallow, pow(up, 2.2));
    // light shafts from the surface: streaks that converge upward
    float sx = uv.x / (0.6 + up * 1.2);
    float st = vnoise2(vec2(sx * 7.0 + g_t * 0.05, up * 1.5)) * vnoise2(vec2(sx * 19.0 - g_t * 0.08, up * 0.7 + 3.0));
    float shaft = pow(st, 2.5) * pow(up, 3.0) * 1.6;
    col += vec3(0.25, 0.55, 0.6) * shaft * shafts;
    return col;
}
// plankton: tiny lit specks in the water, three parallax layers
float planktonLayer(vec2 uv, float scale, float speed, float seed) {
    vec2 g = uv * scale + vec2(seed, g_t * speed * drift);
    vec2 cell = floor(g), f = fract(g);
    float h = hash12(cell + seed);
    vec2 c = vec2(hash12(cell + 1.3 + seed), hash12(cell + 2.7 + seed));
    float d = length(f - c);
    float tw = 0.6 + 0.4 * sin(g_t * (2.0 + 3.0 * h) + h * 20.0);
    return step(0.72, h) * exp(-d * d * 900.0 / (0.5 + h)) * tw;
}

// ── scene render ─────────────────────────────────────────────────────────
struct Hit { float t; float mat; };
Hit march(vec3 ro, vec3 rd, float tStart, float tMax) {
    float t = tStart;
    float mat = 0.0;
    for (int i = 0; i < 96; i++) {
        vec3 p = ro + rd * t;
        float d = map(p);
        if (d < 0.0012 * t) { mat = g_mat; break; }
        t += d * 0.85;
        if (t > tMax) { mat = 0.0; break; }
    }
    Hit h; h.t = t; h.mat = mat; return h;
}
// shade a surface hit. returns rgb, and emissive weight in .a
vec4 shadeHit(vec3 p, vec3 n, vec3 rd, float mat, vec3 lightDir) {
    float fres = pow(1.0 - max(dot(-rd, n), 0.0), 3.0);
    float ndl  = max(dot(n, lightDir), 0.0);
    float spec = pow(max(dot(reflect(rd, n), lightDir), 0.0), 36.0);
    vec3 col; float emis;
    if (mat < 1.5) {
        // bell: translucent flesh, rim-lit, glowing from within toward the apex
        float inner = shellGlow(p);
        vec3 flesh = mix(g_bodyCol * 0.35, g_bodyCol, inner);
        col = flesh * (0.35 + 0.65 * ndl) + g_glowCol * inner * 0.6 * g_glowK;
        col += fres * mix(g_bodyCol, vec3(1.0), 0.6) * 1.4;        // rim
        col += spec * 1.2;
        emis = 0.35 * inner * g_glowK + fres * 0.5;
    } else if (mat < 2.5) {
        // gonads: hot glowing rings
        col = g_glowCol * (1.6 + 0.8 * g_glowK) + vec3(1.0) * spec * 0.6;
        emis = 1.0;
    } else if (mat < 3.5) {
        // oral arms: frilled, lit, faintly glowing edges
        col = mix(g_bodyCol, g_glowCol, 0.35) * (0.3 + 0.7 * ndl) + fres * g_glowCol * 1.2 + spec * 0.8;
        emis = 0.35 + fres * 0.5;
    } else {
        // tentacles: thin, bright beads, strong rim
        col = g_tentCol * (0.4 + 0.6 * ndl) + fres * g_tentCol * 1.5 + spec * 0.6;
        emis = 0.5 + fres * 0.6;
    }
    return vec4(col, emis * glowAmt);
}

vec4 renderScene(vec3 ro, vec3 rd, vec2 uv) {
    vec3 lightDir = normalize(vec3(0.3, 1.0, -0.2));
    vec3 bg = waterBg(rd, uv);
    // plankton behind everything (parallax with the orbit)
    float pk = planktonLayer(uv, 14.0, 0.03, 1.0) * 0.6 + planktonLayer(uv, 26.0, 0.05, 2.0) * 0.4 + planktonLayer(uv, 48.0, 0.08, 3.0) * 0.25;
    pk *= plankton * (1.0 + g_ar * 0.8 * highP());
    bg += vec3(0.5, 0.9, 1.0) * pk;

    float tMax = 14.0;
    Hit h1 = march(ro, rd, 0.5, tMax);
    if (h1.mat < 0.5) return vec4(bg, pk * 0.6);

    vec3 p1 = ro + rd * h1.t;
    vec3 n1 = calcNormal(p1, 0.0015);
    vec4 s1 = shadeHit(p1, n1, rd, h1.mat, lightDir);

    if (h1.mat < 1.5) {
        // translucent bell: look THROUGH it — step past the shell and keep going
        float fres = pow(1.0 - max(dot(-rd, n1), 0.0), 3.0);
        float alpha = clamp(mix(0.95, 0.42, translucency) + fres * 0.45, 0.0, 1.0);
        Hit h2 = march(ro, rd, h1.t + 0.12 * bellSize, tMax);
        vec4 behind;
        if (h2.mat < 0.5) behind = vec4(bg * mix(1.0, 0.75, translucency), pk * 0.3);
        else {
            vec3 p2 = ro + rd * h2.t;
            vec3 n2 = calcNormal(p2, 0.0015);
            behind = shadeHit(p2, n2, rd, h2.mat, lightDir);
            // the inside of the bell catches the glow: soft tint on what's behind
            behind.rgb = mix(behind.rgb, behind.rgb * g_bodyCol * 1.4, 0.3);
        }
        // light scattered through the flesh
        vec3 through = behind.rgb * (1.0 - alpha) + s1.rgb * alpha;
        through += g_glowCol * 0.12 * g_glowK * (1.0 - alpha);
        float em = s1.a * alpha + behind.a * (1.0 - alpha);
        // depth fog toward the water colour
        float fog = 1.0 - exp(-h1.t * 0.045);
        return vec4(mix(through, bg, fog * 0.35), em);
    }
    float fog = 1.0 - exp(-h1.t * 0.045);
    return vec4(mix(s1.rgb, bg, fog * 0.35), s1.a);
}

void main() {
    vec2 fragCoord = gl_FragCoord.xy;
    vec2 uv = (fragCoord - 0.5 * R) / R.y;
    g_ar = audioReact;
    // time: bass pushes the pulse PHASE (never its rate) so motion stays smooth
    g_t = TIME;
    float pt = TIME * 0.55 * pulseRate + g_ar * 0.25 * audioBassTime;
    g_pulse = clamp(pulseCurve(pt) * (0.35 + 0.65 * pulseDepth) * (1.0 + g_ar * 0.5 * bassP()), 0.0, 1.0);
    g_sway  = swayAmt * (1.0 + g_ar * 0.8 * midP());
    g_glowK = glowAmt * (1.0 + g_ar * 1.4 * beatP());
    g_bodyCol = hsv2rgb(vec3(hue, 0.75, 1.0));
    g_glowCol = hsv2rgb(vec3(hue2, 0.85, 1.0));
    g_tentCol = hsv2rgb(vec3(mix(hue, hue2, 0.5), 0.6, 1.0));

    if (PASSINDEX == 0) {
        // camera: slow orbit around the medusa, which drifts up and sways
        float oa = TIME * 0.12 * orbit;
        vec3 ro = vec3(sin(oa) * camDist, camHeight + 0.3 * sin(TIME * 0.3) * drift * 0.3, cos(oa) * camDist);
        vec3 ta = vec3(0.0, -0.4 + 0.15 * sin(TIME * 0.5) * drift, 0.0);
        // the jelly itself bobs with each pulse
        ro.y -= 0.12 * g_pulse;
        vec3 ww = normalize(ta - ro);
        vec3 uu = normalize(cross(vec3(0.0, 1.0, 0.0), ww));
        vec3 vv = cross(ww, uu);
        vec3 rd = normalize(uu * uv.x + vv * uv.y + ww * 1.6);
        vec4 sc = renderScene(ro, rd, uv);
        gl_FragColor = vec4(max(sc.rgb, 0.0), clamp(sc.a, 0.0, 4.0));
    } else {
        vec2 st = fragCoord / R;
        vec4 c0 = texture2D(jfScene, st);
        // bloom: 16-tap spiral around the pixel, weighted by the emissive alpha
        vec3 bl = vec3(0.0);
        float wsum = 0.0;
        float rad = 0.022 * bloomAmt;
        for (int i = 0; i < 16; i++) {
            float fi = float(i);
            float a = fi * 2.399963 + hash12(fragCoord) * 6.28;
            float rr = sqrt((fi + 0.5) / 16.0) * rad;
            vec2 o = vec2(cos(a), sin(a)) * rr * vec2(R.y / R.x, 1.0);
            vec4 s = texture2D(jfScene, st + o);
            float w = (1.0 - rr / rad) * s.a;
            bl += s.rgb * w; wsum += 1.0;
        }
        bl /= wsum;
        vec3 col = c0.rgb + bl * 0.9 * bloomAmt;
        col *= exposure;
        // tone + HD finisher
        col = col / (1.0 + col * 0.35);
        float lvl = levP();
        col *= brightness * mix(1.0, 0.82 + 0.25 * lvl, g_ar);
        float l0 = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(vec3(l0), col, saturation);
        col += (hash12(fragCoord + fract(TIME) * 61.0) - 0.5) * 0.006;
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.4);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.15), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.18, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
