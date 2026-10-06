/*{
  "DESCRIPTION": "Fiery — a volumetric bonfire: a twisting column of flame raymarched through upward-advected, domain-warped noise, front-to-back composited with a heat palette from white-gold core through orange and ember red into dark smoke. Embers rise and twinkle out of the tongues, a warm glow pools on the ground. Slow camera orbit. Bass raises and heats the flame, beats gust the turbulence, mids warm the hue, highs spark the embers. Every parameter is exposed.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-23 — written from scratch (generic volumetric-fire technique)",
  "CATEGORIES": ["Generator", "3D", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "speed",       "LABEL": "Flame Speed",      "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "orbitSpeed",  "LABEL": "Orbit Speed",      "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": 0.15, "GROUP": "Motion" },
    { "NAME": "twist",       "LABEL": "Twist",            "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "flicker",     "LABEL": "Flicker",          "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.4,  "GROUP": "Motion" },
    { "NAME": "flameHeight", "LABEL": "Flame Height",     "TYPE": "float", "MIN": 1.0,  "MAX": 6.0,  "DEFAULT": 3.4,  "GROUP": "Flame" },
    { "NAME": "flameWidth",  "LABEL": "Flame Width",      "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,  "DEFAULT": 1.4,  "GROUP": "Flame" },
    { "NAME": "turbulence",  "LABEL": "Turbulence",       "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.25,  "GROUP": "Flame" },
    { "NAME": "noiseScale",  "LABEL": "Detail Scale",     "TYPE": "float", "MIN": 0.5,  "MAX": 5.0,  "DEFAULT": 2.0,  "GROUP": "Flame" },
    { "NAME": "density",     "LABEL": "Density",          "TYPE": "float", "MIN": 0.2,  "MAX": 3.0,  "DEFAULT": 1.3,  "GROUP": "Flame" },
    { "NAME": "sharpness",   "LABEL": "Edge Sharpness",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.7,  "GROUP": "Flame" },
    { "NAME": "heat",        "LABEL": "Heat",             "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.15,  "GROUP": "Flame" },
    { "NAME": "smokeAmt",    "LABEL": "Smoke",            "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Flame" },
    { "NAME": "steps",       "LABEL": "March Steps",      "TYPE": "float", "MIN": 32.0, "MAX": 128.0,"DEFAULT": 72.0, "GROUP": "Flame" },
    { "NAME": "camDist",     "LABEL": "Camera Distance",  "TYPE": "float", "MIN": 1.5,  "MAX": 10.0, "DEFAULT": 3.6,  "GROUP": "Camera" },
    { "NAME": "camHeight",   "LABEL": "Camera Height",    "TYPE": "float", "MIN": -1.0, "MAX": 4.0,  "DEFAULT": 1.2,  "GROUP": "Camera" },
    { "NAME": "camRoll",     "LABEL": "Camera Roll",      "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Camera" },
    { "NAME": "fov",         "LABEL": "Field Of View",    "TYPE": "float", "MIN": 0.6,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Camera" },
    { "NAME": "emberAmt",    "LABEL": "Embers",           "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Embers" },
    { "NAME": "emberRise",   "LABEL": "Ember Rise",       "TYPE": "float", "MIN": 0.2,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Embers" },
    { "NAME": "emberSize",   "LABEL": "Ember Size",       "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Embers" },
    { "NAME": "groundGlow",  "LABEL": "Ground Glow",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Embers" },
    { "NAME": "colCore",     "LABEL": "Core",             "TYPE": "color", "DEFAULT": [1.0, 0.95, 0.65, 1.0], "GROUP": "Color" },
    { "NAME": "colFlame",    "LABEL": "Flame",            "TYPE": "color", "DEFAULT": [1.0, 0.45, 0.04, 1.0], "GROUP": "Color" },
    { "NAME": "colEmber",    "LABEL": "Ember",            "TYPE": "color", "DEFAULT": [0.75, 0.08, 0.01, 1.0], "GROUP": "Color" },
    { "NAME": "colSmoke",    "LABEL": "Smoke",            "TYPE": "color", "DEFAULT": [0.16, 0.12, 0.1, 1.0],  "GROUP": "Color" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",        "TYPE": "float", "MIN": -0.5, "MAX": 0.5,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "exposure",    "LABEL": "Exposure",         "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom",            "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.2, "GROUP": "Color" },
    { "NAME": "saturation",  "LABEL": "Saturation",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.6,  "DEFAULT": 1.05, "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",       "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Color" }
  ],
  "PASSES": [
    { "TARGET": "fyScene" },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// FIERY
//   pass 0  fyScene — volumetric flame column (bounded cylinder march,
//                     advected + warped value-noise fbm, heat palette,
//                     front-to-back compositing) + projected 3D embers
//                     + ground glow.
//   pass 1  final   — tight bloom + HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define TAU 6.28318531

float hash11(float n) { return fract(sin(n * 91.3458) * 47453.5453); }
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float hash31(vec3 p3) {
    p3 = fract(p3 * 0.1031);
    p3 += dot(p3, p3.zyx + 31.32);
    return fract((p3.x + p3.y) * p3.z);
}
// value noise (named vnoise3: noise3 is reserved in GLSL 330)
float vnoise3(vec3 x) {
    vec3 p = floor(x), f = fract(x);
    f = f * f * (3.0 - 2.0 * f);
    float n000 = hash31(p), n100 = hash31(p + vec3(1, 0, 0));
    float n010 = hash31(p + vec3(0, 1, 0)), n110 = hash31(p + vec3(1, 1, 0));
    float n001 = hash31(p + vec3(0, 0, 1)), n101 = hash31(p + vec3(1, 0, 1));
    float n011 = hash31(p + vec3(0, 1, 1)), n111 = hash31(p + vec3(1, 1, 1));
    return mix(mix(mix(n000, n100, f.x), mix(n010, n110, f.x), f.y),
               mix(mix(n001, n101, f.x), mix(n011, n111, f.x), f.y), f.z);
}
mat2 rot(float a) { return mat2(cos(a), -sin(a), sin(a), cos(a)); }
vec3 hueRotate(vec3 c, float h) {
    float a = h * TAU; vec3 k = vec3(0.57735);
    float cs = cos(a), sn = sin(a);
    return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
}

float bassP()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.4); }
float midP()   { return pow(smoothstep(0.06, 0.85, audioMid),  1.2); }
float highP()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }
float beatP()  { return clamp(audioBeatPulse, 0.0, 1.0); }
float levP()   { return clamp(audioLevel, 0.0, 1.0); }

// ── flame field ──────────────────────────────────────────────────────────
float g_t;      // flame clock
float g_h;      // effective height
float g_turb;

// >0 inside the flame envelope: narrow foot, belly, tapering tongue
float envelope(vec3 p) {
    float y = p.y / g_h;
    float w = flameWidth * (0.55 + 0.45 * smoothstep(0.0, 0.25, y)) * (1.0 - smoothstep(0.25, 1.05, y));
    // flicker: the envelope leans and pulses
    float lean = flicker * 0.25 * sin(g_t * 2.1 + y * 3.0) * y;
    float r = length(p.xz - vec2(lean, 0.0));
    return 1.0 - r / max(w, 0.02);
}

// returns density (>0 burns) and heat (0 smoke .. 1 core)
vec2 flame(vec3 p) {
    float env = envelope(p);
    if (env < -0.6) return vec2(-1.0, 0.0);
    float y = clamp(p.y / g_h, 0.0, 1.2);
    // twist around the axis, stronger with height
    p.xz *= rot(p.y * twist * 0.5 - g_t * 0.35);
    // advect upward, warp the domain with a second noise
    vec3 q = p * noiseScale - vec3(0.0, g_t * 1.6, 0.0);
    vec3 warp = vec3(vnoise3(q * 1.3 + 5.2), vnoise3(q * 1.3 + 11.7), vnoise3(q * 1.3 + 23.1)) - 0.5;
    q += warp * 0.9 * g_turb;
    float f = 0.0, a = 0.5;
    vec3 qq = q;
    for (int i = 0; i < 6; i++) {
        f += a * vnoise3(qq);
        qq = qq * 2.07 - vec3(0.0, g_t * 0.9, 0.0);
        a *= 0.52;
    }
    f -= 0.52;
    // ridged micro-detail: sharp filaments inside the tongues
    f += (abs(vnoise3(q * 4.1 - vec3(0.0, g_t * 2.4, 0.0)) - 0.5) - 0.25) * 0.35;
    // noise carves tongues out of the envelope; more carving higher up
    float den = env * 1.1 + f * (2.4 + 1.8 * y) * g_turb - 0.10 - 0.30 * y * y;
    float ht = clamp(env * 0.7 + 0.5 - y * 1.1 + f * 0.9, 0.0, 1.0);
    return vec2(den, ht);
}

vec3 heatColor(float h, float y) {
    vec3 c = h < 0.35 ? mix(colSmoke.rgb * 0.6, colEmber.rgb, h / 0.35)
           : h < 0.7  ? mix(colEmber.rgb, colFlame.rgb, (h - 0.35) / 0.35)
                      : mix(colFlame.rgb, colCore.rgb, (h - 0.7) / 0.3);
    // smoke greys out the top
    c = mix(c, colSmoke.rgb, smoothstep(0.55, 1.1, y) * smokeAmt * (1.0 - h));
    return c;
}

// camera
vec3 g_ro, g_uu, g_vv, g_ww;
void buildCam() {
    float yaw = TIME * orbitSpeed * 0.5;
    g_ro = vec3(sin(yaw) * camDist, camHeight, cos(yaw) * camDist);
    vec3 ta = vec3(0.0, g_h * 0.45, 0.0);
    g_ww = normalize(ta - g_ro);
    float cr = camRoll * 0.6;
    g_uu = normalize(cross(vec3(sin(cr), cos(cr), 0.0), g_ww));
    g_vv = normalize(cross(g_ww, g_uu));
}
vec2 project(vec3 w, out float depth) {
    vec3 v = w - g_ro;
    depth = dot(v, g_ww);
    vec2 s = vec2(dot(v, g_uu), dot(v, g_vv)) / max(depth, 0.01) * (2.0 / fov);
    return s;   // in "p" space: (2*frag - R)/R.y
}

vec3 embers(vec2 p) {
    if (emberAmt <= 0.001) return vec3(0.0);
    float ar = audioReact;
    vec3 acc = vec3(0.0);
    float t = TIME * emberRise;
    for (int i = 0; i < 48; i++) {
        float fi = float(i);
        float h1 = hash11(fi * 1.37 + 0.5), h2 = hash11(fi * 3.11 + 1.7), h3 = hash11(fi * 5.03 + 2.9);
        float life = fract(t * (0.25 + 0.35 * h1) + h2);
        float ang = h3 * TAU + t * 0.6 * (h1 - 0.5);
        float rad = flameWidth * (0.15 + 0.5 * h2) * (1.0 + life * 0.8);
        vec3 w = vec3(cos(ang) * rad + 0.25 * sin(t * 1.7 + fi), life * g_h * 1.5 + 0.1,
                      sin(ang) * rad + 0.25 * cos(t * 1.3 + fi * 1.3));
        float depth;
        vec2 s = project(w, depth);
        if (depth < 0.3) continue;
        float px = emberSize * 0.012 * (5.0 / depth);
        float dd = length(p - s) / px;
        float glow = exp(-dd * dd * 2.5) + 0.12 * exp(-dd * 0.8);
        float fade = smoothstep(0.0, 0.12, life) * (1.0 - smoothstep(0.55, 1.0, life));
        float twinkle = 0.6 + 0.4 * sin(t * (8.0 + h3 * 9.0) + fi);
        twinkle *= 1.0 + ar * 0.8 * highP();
        vec3 ec = mix(colFlame.rgb, colCore.rgb, h1 * 0.6) * (1.0 - life * 0.5) + colEmber.rgb * life;
        acc += ec * glow * fade * twinkle;
    }
    return acc * emberAmt * 0.9;
}

vec3 scene(vec2 frag) {
    float ar = audioReact;
    g_t    = TIME * speed + ar * 0.6 * audioBassTime;
    g_h    = flameHeight * (1.0 + ar * 0.30 * bassP());
    g_turb = turbulence * (1.0 + ar * 0.25 * beatP());
    buildCam();

    vec2 p = (2.0 * frag - R) / R.y;
    vec3 rd = normalize(p.x * g_uu + p.y * g_vv + (2.0 / fov) * g_ww);

    // bound the march: infinite cylinder of radius rc, y in [0, top]
    float rc = flameWidth * 1.35;
    float top = g_h * 1.15;
    vec2 ro2 = g_ro.xz, rd2 = rd.xz;
    float A = dot(rd2, rd2), B = 2.0 * dot(ro2, rd2), C = dot(ro2, ro2) - rc * rc;
    float disc = B * B - 4.0 * A * C;
    vec4 sum = vec4(0.0);
    if (disc > 0.0) {
        float sq = sqrt(disc);
        float t0 = (-B - sq) / (2.0 * A), t1 = (-B + sq) / (2.0 * A);
        // y slab
        float ty0 = (0.0 - g_ro.y) / rd.y, ty1 = (top - g_ro.y) / rd.y;
        float ya = min(ty0, ty1), yb = max(ty0, ty1);
        t0 = max(max(t0, ya), 0.0); t1 = min(t1, yb);
        if (t1 > t0) {
            int n = int(steps + 0.5);
            float dt = (t1 - t0) / float(n);
            float dth = hash21(frag + fract(TIME) * 37.0);
            float t = t0 + dt * dth;
            float ht = heat * (1.0 + ar * 0.25 * bassP());
            for (int i = 0; i < 128; i++) {
                if (i >= n) break;
                vec3 pos = g_ro + rd * t;
                vec2 fd = flame(pos);
                if (fd.x > 0.0) {
                    float y = pos.y / g_h;
                    float d = mix(clamp(fd.x, 0.0, 1.0), smoothstep(0.0, 0.22, fd.x), sharpness);
                    float alpha = 1.0 - exp(-d * density * dt * 4.5);
                    float hh = clamp(fd.y * ht, 0.0, 1.0);
                    vec3 col = heatColor(hh, y) * (0.3 + 2.0 * hh * hh) * exposure;
                    sum.rgb += col * alpha * (1.0 - sum.a);
                    sum.a   += alpha * (1.0 - sum.a);
                    if (sum.a > 0.985) break;
                }
                t += dt;
            }
        }
    }
    vec3 col = sum.rgb;

    // ground glow: warm pool on y=0 lit by the fire
    if (rd.y < 0.0 && groundGlow > 0.001) {
        float tg = -g_ro.y / rd.y;
        vec3 gp = g_ro + rd * tg;
        float r = length(gp.xz);
        float glow = exp(-r * r * 0.9) * (0.7 + 0.3 * sin(g_t * 3.0 + r * 4.0));
        float tex = 0.85 + 0.3 * (vnoise3(vec3(gp.xz * 6.0, 1.0)) - 0.5);
        vec3 gcol = mix(colEmber.rgb, colFlame.rgb, glow) * glow * tex * groundGlow * 0.6
                  * (1.0 + ar * 0.3 * bassP());
        col += gcol * (1.0 - sum.a);
    }

    col += embers(p);
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        vec3 col = scene(gl_FragCoord.xy);
        gl_FragColor = vec4(max(col, 0.0), 1.0);
    } else {
        vec3 col = texture2D(fyScene, uv).rgb;
        // tight bloom
        vec3 bl = vec3(0.0);
        for (int i = 0; i < 8; i++) {
            float an = float(i) * 0.7853982;
            vec2 o = vec2(cos(an), sin(an)) * (3.0 / R.y);
            bl += texture2D(fyScene, uv + o).rgb;
            bl += texture2D(fyScene, uv + o * 2.4).rgb * 0.6;
        }
        bl /= 12.8;
        bl = max(bl - 0.6, 0.0);
        col += bl * bl * bloomAmt * 1.6;

        col = hueRotate(col, hueShift + audioReact * 0.03 * midP());
        float lvl = levP();
        col *= brightness * mix(1.0, 0.70 + 0.32 * lvl + 0.18 * bassP(), audioReact)
             * (1.0 + audioReact * 0.05 * beatP());
        // filmic tone
        col = col / (1.0 + col * 0.35);
        float l0 = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(vec3(l0), col, saturation);
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;
        // HD finisher
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
