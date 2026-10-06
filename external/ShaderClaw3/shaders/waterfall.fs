/*{
  "DESCRIPTION": "Waterfall — a 2D wave-propagation fluid: velocity and pressure live in a persistent field that advects itself along its own flow, two opposed emitters pump pressure in and out so ripples, eddies and standing fronts chase each other across the surface. The height field refracts a procedural stone-and-caustic bed and is lit with a directional diffuse and a tight specular. Bass pumps the emitters, beats pulse them, mids shift the bed colour, highs sharpen the specular. Every parameter is exposed.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-23 — after the Shadertoy 'Wave Propagation Effect' fork (tomkh) pasted by Lu",
  "CATEGORIES": ["Generator", "Feedback", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "emitStrength","LABEL": "Emitter Strength", "TYPE": "float", "MIN": 0.0,  "MAX": 120.0,"DEFAULT": 50.0, "GROUP": "Fluid" },
    { "NAME": "emitRadius",  "LABEL": "Emitter Radius",   "TYPE": "float", "MIN": 2.0,  "MAX": 30.0, "DEFAULT": 5.0,  "GROUP": "Fluid" },
    { "NAME": "emitSpread",  "LABEL": "Emitter Spread",   "TYPE": "float", "MIN": 0.0,  "MAX": 0.5,  "DEFAULT": 0.45, "GROUP": "Fluid" },
    { "NAME": "emitY",       "LABEL": "Emitter Height",   "TYPE": "float", "MIN": -0.5, "MAX": 0.5,  "DEFAULT": 0.0,  "GROUP": "Fluid" },
    { "NAME": "emitOrbit",   "LABEL": "Emitter Orbit",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Fluid" },
    { "NAME": "advect",      "LABEL": "Self-Advection",   "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Fluid" },
    { "NAME": "damping",     "LABEL": "Damping",          "TYPE": "float", "MIN": 0.9,  "MAX": 1.0,  "DEFAULT": 0.995,"GROUP": "Fluid" },
    { "NAME": "reliefAmt",   "LABEL": "Relief",           "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Surface" },
    { "NAME": "refractAmt",  "LABEL": "Refraction",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.5,  "DEFAULT": 0.35, "GROUP": "Surface" },
    { "NAME": "bedScale",    "LABEL": "Bed Scale",        "TYPE": "float", "MIN": 0.5,  "MAX": 6.0,  "DEFAULT": 2.0,  "GROUP": "Surface" },
    { "NAME": "specAmt",     "LABEL": "Specular",         "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Surface" },
    { "NAME": "lightX",      "LABEL": "Light X",          "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": 0.2,  "GROUP": "Surface" },
    { "NAME": "lightY",      "LABEL": "Light Y",          "TYPE": "float", "MIN": -1.0, "MAX": 1.0,  "DEFAULT": -0.5, "GROUP": "Surface" },
    { "NAME": "bedA",        "LABEL": "Bed Colour A",     "TYPE": "color", "DEFAULT": [0.05, 0.25, 0.35, 1.0], "GROUP": "Color" },
    { "NAME": "bedB",        "LABEL": "Bed Colour B",     "TYPE": "color", "DEFAULT": [0.35, 0.75, 0.85, 1.0], "GROUP": "Color" },
    { "NAME": "waterTint",   "LABEL": "Water Tint",       "TYPE": "color", "DEFAULT": [0.7, 0.8, 1.0, 1.0],    "GROUP": "Color" },
    { "NAME": "tintAmt",     "LABEL": "Tint Amount",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.25, "GROUP": "Color" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",        "TYPE": "float", "MIN": -0.5, "MAX": 0.5,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "saturation",  "LABEL": "Saturation",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.6,  "DEFAULT": 1.1,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",       "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Color" }
  ],
  "PASSES": [
    { "TARGET": "wfState", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// WATERFALL
//   pass 0  wfState — .xy velocity, .z pressure, .w emitter flag; stored
//                     offset-encoded (v*KV+0.5, p*KP+0.5) so signed values
//                     survive any buffer format. Wrap-around sampling.
//   pass 1  final   — pressure gradient → fake normal, refracted procedural
//                     bed, diffuse + specular, HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R  RENDERSIZE.xy
#define KV 0.008
#define KP 0.002

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

vec4 decode(vec4 e) { return vec4((e.xy - 0.5) / KV, (e.z - 0.5) / KP, e.w); }
vec4 encode(vec4 s) { return vec4(s.xy * KV + 0.5, s.z * KP + 0.5, s.w); }

// wrap-around read in pixel space, decoded
vec4 tex(vec2 g, vec2 p) {
    vec2 gp = fract((g + p) / R);
    vec4 e = texture2D(wfState, gp);
    if (any(notEqual(e, e))) e = vec4(0.5, 0.5, 0.5, 0.0);
    return decode(e);
}

// follow the velocity back n times (semi-Lagrangian self-advection)
vec4 getState(vec2 g, int n) {
    vec4 p = vec4(0.0);
    for (int i = 0; i < 4; i++) {
        if (i >= n) break;
        p = tex(g, -p.xy * advect);
    }
    return p;
}

void main() {
    vec2 g = gl_FragCoord.xy;
    vec2 uv = g / R;
    float ar = audioReact;
    if (PASSINDEX == 0) {
        if (FRAMEINDEX < 2) {
            // spawn visible: seed a low-frequency pressure field so waves exist at once
            float sd = (vnoise2(uv * 6.0 + 3.1) - 0.5) * 2.0 + (vnoise2(uv * 17.0 + 9.7) - 0.5) * 0.6;
            gl_FragColor = encode(vec4(0.0, 0.0, sd * 60.0, 0.0)); return;
        }
        vec4 r = tex(g, vec2( 1.0, 0.0));
        vec4 t = tex(g, vec2( 0.0, 1.0));
        vec4 l = tex(g, vec2(-1.0, 0.0));
        vec4 b = tex(g, vec2( 0.0,-1.0));
        vec2 c = sin(uv * 6.28318) * 0.5 + 0.5;
        float cc = c.x + c.y;
        vec4 f = getState(g, int(cc * 2.0 + 1.0));
        f.xy += vec2(r.z - l.z, t.z - b.z);
        vec4 dp = (r + t + l + b) / 4.0;
        float div = ((l - r).x + (b - t).y) / 20.0;
        f.z = dp.z - div;
        f.xyz *= damping;

        // emitters: opposed pair, optionally orbiting; bass pumps, beats pulse
        float k = emitStrength * (1.0 + ar * 0.5 * bassP() + ar * 0.5 * beatP());
        float orb = emitOrbit * TIME * 0.4;
        vec2 e1 = vec2(-emitSpread, emitY);
        vec2 e2 = vec2( emitSpread, emitY);
        mat2 m = mat2(cos(orb), -sin(orb), sin(orb), cos(orb));
        e1 = m * e1; e2 = m * e2;
        if (length(g - R * (0.5 + e1)) < emitRadius) { f.x =  k; f.w = 1.0; }
        if (length(g - R * (0.5 + e2)) < emitRadius) { f.x = -k; f.w = 1.0; }
        // keep the encoded range safe
        f.xy = clamp(f.xy, -60.0, 60.0);
        f.z  = clamp(f.z, -240.0, 240.0);
        gl_FragColor = encode(f);
    } else {
        vec3 e = vec3(vec2(1.0) / R, 0.0);
        float p10 = tex(g, vec2(0.0, -1.0)).z;
        float p01 = tex(g, vec2(-1.0, 0.0)).z;
        float p21 = tex(g, vec2( 1.0, 0.0)).z;
        float p12 = tex(g, vec2(0.0,  1.0)).z;
        vec4 w = tex(g, vec2(0.0));
        vec3 grad = normalize(vec3((p21 - p01) * reliefAmt, (p12 - p10) * reliefAmt, 0.5));

        // procedural bed: stone plates + caustic web, refracted by the surface
        vec2 buv = (g / R.y) * bedScale + grad.xy * refractAmt;
        float plates = vnoise2(buv * 3.0) * 0.6 + vnoise2(buv * 9.0) * 0.3 + vnoise2(buv * 27.0) * 0.1;
        float ca = 0.0;
        for (int i = 0; i < 3; i++) {
            float fi = float(i);
            vec2 q = buv * (2.0 + fi) + vec2(0.3 * fi, 0.17 * fi) + TIME * 0.05 * (fi + 1.0);
            ca += abs(sin(q.x * 3.1 + sin(q.y * 2.7)) * sin(q.y * 2.3 + sin(q.x * 3.3)));
        }
        ca = pow(clamp(1.0 - ca / 3.0, 0.0, 1.0), 6.0);
        vec3 bed = mix(bedA.rgb, bedB.rgb, plates);
        bed = hueRotate(bed, ar * 0.05 * midP());
        vec4 c = vec4(bed + ca * 0.9 * bedB.rgb, 1.0);
        c += c * 0.5;
        c += c * clamp(abs(w.z) * 0.02, 0.0, 1.5) * (0.5 - distance(uv, vec2(0.5)));

        vec3 light = normalize(vec3(lightX, lightY, 0.7));
        float diffuse = dot(grad, light);
        float spec = pow(max(0.0, -reflect(light, grad).z), 32.0) * specAmt * (1.0 + ar * 0.6 * highP());
        vec3 col = mix(c.rgb, waterTint.rgb, tintAmt) * max(diffuse, 0.0) + spec;

        col = hueRotate(col, hueShift);
        float lvl = levP();
        col *= brightness * mix(1.0, 0.75 + 0.30 * lvl + 0.15 * bassP(), ar)
             * (1.0 + ar * 0.05 * beatP());
        col = col / (1.0 + col * 0.2);
        float l0 = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(vec3(l0), col, saturation);
        col += (hash21(g + fract(TIME) * 61.0) - 0.5) * 0.006;
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
