/*{
  "DESCRIPTION": "Ring Of Light — two nested light tunnels flying through each other: a wide outer vortex (blue ↔ ember, supersampled 4×) and a tight inner ring tunnel (magenta ↔ mint) composited over it, every wall carved by folded sine turbulence and twisted along the flight path. Bass widens the outer tunnel, beats push you faster down the bore, mids twist the walls, highs brighten the inner ring. Speed, twist, radius, wobble, turbulence, palette and mix are all yours.",
  "CREDIT": "Easel / ShaderClaw house shader, 2026-09-11 — after the twin-tunnel glow march (buffer A + image)",
  "CATEGORIES": ["Generator", "3D", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "flySpeed",    "LABEL": "Fly Speed",        "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "twistAmt",    "LABEL": "Twist",            "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "wobble",      "LABEL": "Wall Wobble",      "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "lens",        "LABEL": "Lens",             "TYPE": "float", "MIN": 0.6,  "MAX": 2.4,  "DEFAULT": 1.2,  "GROUP": "Motion" },
    { "NAME": "outerRadius", "LABEL": "Outer Radius",     "TYPE": "float", "MIN": 1.5,  "MAX": 6.0,  "DEFAULT": 3.5,  "GROUP": "Outer Tunnel" },
    { "NAME": "outerTurb",   "LABEL": "Outer Turbulence", "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Outer Tunnel" },
    { "NAME": "outerGlow",   "LABEL": "Outer Glow",       "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Outer Tunnel" },
    { "NAME": "outerSteps",  "LABEL": "Outer Steps",      "TYPE": "float", "MIN": 40.0, "MAX": 160.0, "DEFAULT": 90.0,  "GROUP": "Outer Tunnel" },
    { "NAME": "innerRadius", "LABEL": "Inner Radius",     "TYPE": "float", "MIN": 0.6,  "MAX": 4.0,  "DEFAULT": 1.8,  "GROUP": "Inner Ring" },
    { "NAME": "innerTurb",   "LABEL": "Inner Turbulence", "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Inner Ring" },
    { "NAME": "innerMix",    "LABEL": "Inner Mix",        "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.75, "GROUP": "Inner Ring" },
    { "NAME": "innerSteps",  "LABEL": "Inner Steps",      "TYPE": "float", "MIN": 30.0, "MAX": 120.0, "DEFAULT": 64.0, "GROUP": "Inner Ring" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",        "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "colorCycle",  "LABEL": "Colour Cycle",     "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "saturation",  "LABEL": "Saturation",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.6,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "exposure",    "LABEL": "Exposure",         "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",       "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "marchRes",    "LABEL": "March Resolution", "TYPE": "float", "MIN": 0.3,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Depth / Passes" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.1,  "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.4,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Depth / Passes" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "rlOuter", "WIDTH": "floor($WIDTH*$marchRes)", "HEIGHT": "floor($HEIGHT*$marchRes)" },
    { "TARGET": "rlScene", "WIDTH": "floor($WIDTH*$marchRes)", "HEIGHT": "floor($HEIGHT*$marchRes)" },
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// RING OF LIGHT
//   pass 0  rlOuter — outer vortex tunnel (buffer A), rendered at marchRes
//                     (the bilinear upsample replaces the old 4-tap supersample,
//                     which cost 4× for a glow field — 13 fps at 1080p).
//   pass 1  rlScene — inner ring tunnel + composite over outer, tanh tone, at marchRes.
//   pass 2  abTrail — house motion trail.
//   pass 3  final   — house bloom + aberration + HD finisher.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define TAU 6.28318531

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
vec3 tanh3(vec3 x) {
    x = clamp(x, -12.0, 12.0);
    vec3 e = exp(2.0 * x);
    return (e - 1.0) / (e + 1.0);
}
vec3 hueRotate(vec3 c, float h) {
    float a = h * TAU;
    vec3 k = vec3(0.57735);
    float cs = cos(a), sn = sin(a);
    return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
}
float bassP()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.4); }
float midP()   { return pow(smoothstep(0.06, 0.85, audioMid),  1.2); }
float highP()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }
float beatP()  { return clamp(audioBeatPulse, 0.0, 1.0); }

// shared clocks: beats push you down the bore (phase-push, never time-scale)
float flyClock() { return TIME * flySpeed + audioReact * 0.6 * audioBassTime; }
float twistClock() { return TIME * flySpeed * 0.8 + audioReact * 0.4 * audioMidTime; }

vec3 outerTunnel(vec2 uv) {
    float t  = flyClock();
    float tt = twistClock();
    float amt = audioReact;
    float rad = outerRadius * (1.0 + amt * 0.25 * bassP());
    float cyc = colorCycle;
    vec3 o = vec3(0.0);
    float z = 0.0, d = 0.0;
    int steps = int(outerSteps);
    vec3 dir = normalize(vec3((uv * 2.0 - R) / R.y, lens));
    for (int i = 0; i < 160; i++) {
        if (i >= steps || float(i) + z >= 300.0) break;
        vec3 q = z * dir;
        q.z += t * 3.0;
        float a = (sin(q.z * 0.1) * 0.5 + tt * 0.2) * twistAmt;
        q.xy = mat2(cos(a), -sin(a), sin(a), cos(a)) * q.xy;
        vec3 p = q;
        float s = 0.2;
        for (int k = 0; k < 9; k++) {
            if (s >= 24.0) break;
            p += (abs(sin(p.yzx * s + vec3(tt * 0.8, tt * 0.4, 0.0))) - 0.6) / s * outerTurb;
            s /= 0.55;
        }
        float tunnel = abs(length(q.xy) - rad + sin(q.z * 0.4 + sin(tt)) * 1.2 * wobble) / 10.0;
        d = 0.002 + tunnel + abs(p.x * 0.015);
        z += d;
        vec3 col = mix(vec3(0.05, 0.25, 0.9), vec3(0.95, 0.35, 0.05), 0.5 + 0.5 * sin(q.z * 0.15 + t * 0.5 * cyc));
        col += vec3(0.1, 0.4, 0.8) * cos(q.z * 0.3 - t * cyc);
        float glow = exp(-z * 0.06) / (d * 60.0 + 0.005);
        o += col * glow;
    }
    return o * outerGlow;
}

vec3 innerTunnel(vec2 uv) {
    float t  = flyClock() * 1.5;
    float tt = twistClock() * 1.5;
    float amt = audioReact;
    float cyc = colorCycle;
    vec3 o = vec3(0.0);
    float z = 0.0, d = 0.0;
    int steps = int(innerSteps);
    vec3 dir = normalize(vec3((uv * 2.0 - R) / R.y, lens));
    for (int i = 0; i < 120; i++) {
        if (i >= steps || float(i) + z >= 200.0) break;
        vec3 q = z * dir;
        q.z += t * 4.5;
        float a = (-sin(q.z * 0.15) * 0.7 - tt * 0.3) * twistAmt;
        q.xy = mat2(cos(a), -sin(a), sin(a), cos(a)) * q.xy;
        vec3 p = q;
        float s = 0.3;
        for (int k = 0; k < 9; k++) {
            if (s >= 18.0) break;
            p += (abs(sin(p.zxy * s + vec3(0.0, tt * 0.6, tt * 0.9))) - 0.55) / s * innerTurb;
            s /= 0.6;
        }
        float tunnel = abs(length(q.xy) - innerRadius + cos(q.z * 0.6 - t) * 0.8 * wobble) / 8.0;
        d = 0.003 + tunnel + abs(p.y * 0.02);
        z += d;
        vec3 col = mix(vec3(0.8, 0.1, 0.6), vec3(0.1, 0.9, 0.7), 0.5 + 0.5 * cos(q.z * 0.2 - t * cyc));
        float glow = exp(-z * 0.09) / (d * 80.0 + 0.008);
        o += col * glow;
    }
    return o * (1.0 + amt * 0.5 * highP());
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    vec2 FD = gl_FragCoord.xy;
    if (PASSINDEX == 0) {
        vec3 col = outerTunnel(FD);
        // store pre-tonemap HDR scaled down (half-float safe)
        gl_FragColor = vec4(col * 0.01, 1.0);
    } else if (PASSINDEX == 1) {
        vec3 bg = texture2D(rlOuter, uv).rgb * 100.0;
        vec3 fg = innerTunnel(FD);
        vec3 col = bg + fg * innerMix;
        col = tanh3(col * 0.025 * exposure);
        col = hueRotate(col, hueShift);
        float l = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(vec3(l), col, saturation);
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else if (PASSINDEX == 2) {
        vec3 col  = texture2D(rlScene, uv).rgb;
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
        bl = max(bl - 0.5, 0.0);
        vec3 col = base + bl * bl * bloomAmt * 1.8;
        float lvl = clamp(audioLevel, 0.0, 1.0);
        col *= brightness * mix(1.0, 0.70 + 0.30 * lvl, audioReact)
             * (1.0 + audioReact * 0.05 * clamp(audioBeatPulse, 0.0, 1.0));
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
