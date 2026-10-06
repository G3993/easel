/*{
  "DESCRIPTION": "Paint Cosmos — an ultra-elegant blurred nebula: big out-of-focus fog patches of teal, crimson, violet and amber with a dark vignette pooling center-right and fine film grain. Floating razor-sharp above it: a few small glossy 3D-shaded spheres with specular dots and soft shadows, thin tube-shaded wire curves, one large hairline circle, and tiny scattered specks. The tension between the blurry field and the precise elements is the whole look. Everything drifts imperceptibly; each beat drops a new glossy sphere that floats up and fades over eight seconds, mids sway the wire curves, bass warms and brightens the fog gently.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "fogWarm",
      "LABEL": "Fog Warm Anchor",
      "TYPE": "color",
      "DEFAULT": [0.80, 0.18, 0.16, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "fogCool",
      "LABEL": "Fog Cool Anchor",
      "TYPE": "color",
      "DEFAULT": [0.22, 0.72, 0.78, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "paletteShift",
      "LABEL": "Palette Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 10,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "elementScale",
      "LABEL": "Element Scale",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 1.8,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "fogDepth",
      "LABEL": "Fog Depth",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 1,
      "DEFAULT": 0.62,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "driftSpeed",
      "LABEL": "Drift Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2.5,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "cosmoState",
      "PERSISTENT": true
    },
    {
    }
  ]
}*/

#define N_EVT 4
#define AGE_SPAN 10.0

float hash11(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}
float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1, 0)), u.x),
               mix(hash21(i + vec2(0, 1)), hash21(i + vec2(1, 1)), u.x), u.y);
}
float fbm(vec2 p) {
    float v = 0.0, a = 0.55;
    for (int k = 0; k < 4; k++) {
        v += a * vnoise(p);
        p = p * 2.13 + 9.7;
        a *= 0.5;
    }
    return v;
}
float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

vec3 hueRot(vec3 c, float a) {
    const vec3 W = vec3(0.299, 0.587, 0.114);
    float ca = cos(a), sa = sin(a);
    vec3 g = vec3(dot(c, W));
    vec3 d = c - g;
    vec3 cr = cross(vec3(0.57735), c);
    return max(g + d * ca + cr * sa, 0.0);
}

vec2 enc16(float v) {
    float e = clamp(v, 0.0, 1.0) * 255.0;
    return vec2(floor(e) / 255.0, fract(e));
}
float dec16(vec2 t) { return t.x + t.y / 255.0; }
vec4 fetchEvt(int i) {
    return texture2D(cosmoState, vec2((float(i) + 0.5) / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
}

float gA, gBassP, gMidP, gHighP, gClk;

// ── pass 0: beat-sphere event queue ──
vec4 passState() {
    if (gl_FragCoord.y > 1.0) return vec4(0.0);
    int i = int(gl_FragCoord.x);
    if (i >= N_EVT) return vec4(0.0);

    vec4 st = fetchEvt(i);
    float age = dec16(st.rg) * AGE_SPAN;
    vec2 pos = st.ba;
    float dt = clamp(TIMEDELTA, 0.001, 0.1);
    age += dt;

    if (FRAMEINDEX < 2) {
        // one drop-sphere already mid-float on the first frame
        age = 1.2 + 3.1 * float(i);
        pos = vec2(0.22 + 0.56 * hash11(float(i) * 6.13),
                   0.25 + 0.45 * hash11(float(i) * 2.97));
    }

    if (audioBeatPulse > mix(1.25, 0.45, gA) && age > 2.6
        && hash21(vec2(float(i) * 5.1, floor(TIME * 8.0))) < 0.34) {
        age = 0.0;
        pos = vec2(0.16 + 0.68 * hash21(vec2(TIME * 3.31, float(i) * 2.77)),
                   0.20 + 0.50 * hash21(vec2(float(i) * 8.39, TIME * 2.11)));
    }

    return vec4(enc16(min(age, AGE_SPAN - 0.01) / AGE_SPAN), pos);
}

// soft out-of-focus color blob
vec3 blob(vec2 p, vec2 c, float sig, vec3 col, float w) {
    vec2 d = p - c;
    return col * w * exp(-dot(d, d) / (sig * sig));
}

// glossy 3D-shaded sphere with specular dot; returns rgb + coverage
vec4 sphere(vec2 p, vec2 c, float r, vec3 base) {
    vec2 q = (p - c) / r;
    float rr = dot(q, q);
    float px = r * RENDERSIZE.y;
    float cov = smoothstep(1.0, 1.0 - 2.4 / max(px, 3.0), sqrt(max(rr, 1e-6)));
    if (cov <= 0.0) return vec4(0.0);
    float z = sqrt(max(1.0 - rr, 0.0));
    vec3 n = vec3(q, z);
    vec3 L = normalize(vec3(-0.42, 0.55, 0.72));
    float dif = clamp(dot(n, L), 0.0, 1.0);
    vec3 col = base * (0.22 + 0.85 * dif);
    // bottom bounce tint + dark occluded underside
    col += base * 0.15 * clamp(-n.y, 0.0, 1.0);
    col *= 0.75 + 0.25 * z;
    // crisp specular dot upper-left
    vec3 R = reflect(vec3(0.0, 0.0, -1.0), n);
    float spec = pow(clamp(dot(R, L), 0.0, 1.0), 90.0);
    col += vec3(1.0) * spec * 1.1;
    return vec4(col, cov);
}

// thin tube-shaded wire: returns rgb + coverage
vec4 wire(vec2 p, vec2 org, float ang, float len, float amp, vec3 cA, vec3 cB,
          float ph, float w) {
    vec2 dU = vec2(cos(ang), sin(ang));
    vec2 dV = vec2(-dU.y, dU.x);
    vec2 rp = p - org;
    float u = dot(rp, dU);
    float v = dot(rp, dV);
    float uc = clamp(u, 0.0, len);
    float f = amp * (sin(uc * 6.8 + ph) * 0.6
                   + sin(uc * 14.3 + ph * 1.7 + 1.3) * 0.22
                   + sin(uc * 3.2 + ph * 0.6 + 4.1) * 0.85);
    // taper the wave near the ends so tips exit clean
    f *= smoothstep(0.0, 0.10, uc) * smoothstep(len, len - 0.10, uc);
    vec2 dv = vec2(u - uc, v - f);
    float d = length(dv);
    float px = RENDERSIZE.y;
    float cov = smoothstep(w + 1.4 / px, w - 1.4 / px, d);
    if (cov <= 0.0) return vec4(0.0);
    float s = clamp((v - f) / max(w, 1e-5), -1.0, 1.0);
    float zn = sqrt(max(1.0 - s * s, 0.0));
    vec3 base = mix(cA, cB, smoothstep(0.0, len, uc));
    vec3 col = base * (0.30 + 0.70 * clamp(0.6 - 0.55 * s, 0.0, 1.0)) * (0.55 + 0.45 * zn);
    col += vec3(1.0) * pow(clamp(1.0 - abs(s + 0.38) * 2.0, 0.0, 1.0), 6.0) * 0.45;
    return vec4(col, cov);
}

// ── pass 1: the picture ──
vec4 passImage() {
    vec2 uv = isf_FragNormCoord.xy;
    float asp = RENDERSIZE.x / RENDERSIZE.y;
    vec2 p = vec2(uv.x * asp, uv.y);          // aspect-true space, y in 0..1
    vec2 ctr = vec2(0.5 * asp, 0.5);
    float t = gClk * 0.03;                    // imperceptible hue/position drift
    float tm = gClk * 0.30;                   // element clock: slow but alive

    float ps = paletteShift * 0.628;
    vec3 warm = hueRot(fogWarm.rgb, ps);
    vec3 cool = hueRot(fogCool.rgb, ps);
    vec3 viol = hueRot(cool, 2.35);          // saturated violet from the cool anchor
    vec3 ambr = hueRot(warm, 0.55);
    vec3 milkC = mix(cool, vec3(0.9), 0.55);

    // ── blurred nebula field ──
    vec3 col = vec3(0.075, 0.068, 0.095);
    vec2 dr1 = 0.045 * vec2(sin(tm * 0.43), cos(tm * 0.37));
    vec2 dr2 = 0.045 * vec2(sin(tm * 0.31 + 2.0), sin(tm * 0.49 + 4.0));
    col += blob(p, vec2(0.10 * asp, 0.72) + dr1, 0.32, milkC, 0.68);
    col += blob(p, vec2(0.24 * asp, 0.94) + dr2, 0.30, cool * 0.9, 0.72);
    col += blob(p, vec2(0.88 * asp, 0.88) - dr1, 0.32, warm, 0.62);
    col += blob(p, vec2(0.97 * asp, 0.34) + dr2.yx, 0.28, warm * vec3(1.0, 0.55, 0.4), 0.55);
    col += blob(p, vec2(0.10 * asp, 0.22) - dr2, 0.30, ambr, 0.52);
    col += blob(p, vec2(0.45 * asp, 0.04) + dr1.yx, 0.34, ambr * vec3(1.0, 0.8, 0.55), 0.45);
    col += blob(p, vec2(0.38 * asp, 0.55) + dr1, 0.28, viol, 0.48);
    // huge soft mottling — still out-of-focus, never gritty
    col *= 0.88 + 0.24 * fbm(p * 1.5 + t * 0.4);
    // dark vignette pooling center-right
    vec2 vd = p - vec2(0.63 * asp, 0.47);
    col *= 1.0 - (0.78 * fogDepth) * exp(-dot(vd, vd) / 0.20);
    vd = p - vec2(0.70 * asp, 0.72);
    col *= 1.0 - (0.35 * fogDepth) * exp(-dot(vd, vd) / 0.05);
    // bass warms and brightens the fog gently
    col = mix(col, col * (vec3(1.10, 0.96, 0.90) + warm * 0.12), gA * gBassP * 0.6);
    col *= 1.0 + 0.10 * gA * gBassP;

    // ── one large hairline circle ──
    vec2 cc = p - (vec2(0.55 * asp, 0.47) + 0.008 * vec2(sin(t), cos(t * 0.8)));
    float cd = abs(length(cc) - 0.375 * elementScale) * RENDERSIZE.y;
    float arcFade = 0.55 + 0.45 * vnoise(vec2(atan(cc.y, cc.x) * 1.6 + 3.0, 0.5));
    col = mix(col, mix(col, vec3(0.78, 0.62, 0.42), 0.85),
              smoothstep(1.6, 0.4, cd) * 0.55 * arcFade);

    // ── wire curves (mids sway them) ──
    float sway = 0.35 * gA * gMidP;
    float es = elementScale;
    vec4 w0 = wire(p, vec2(0.60 * asp, 0.28), 1.25, 0.55 * es, 0.045 + 0.02 * sway,
                   mix(cool, vec3(0.15, 0.75, 0.55), 0.6), vec3(0.55, 0.85, 0.72),
                   tm * 0.9 + sway * 3.0, 0.0042 * es);
    col = mix(col, w0.rgb, w0.a);
    vec4 w1 = wire(p, vec2(0.30 * asp, 0.44), -0.35, 0.42 * es, 0.055 + 0.025 * sway,
                   vec3(0.92, 0.78, 0.74), mix(warm, vec3(0.1), 0.5),
                   -tm * 0.7 + 2.0 + sway * 2.5, 0.0050 * es);
    col = mix(col, w1.rgb, w1.a);
    vec4 w2 = wire(p, vec2(0.10 * asp, 0.80), 0.15, 0.30 * es, 0.035 + 0.02 * sway,
                   vec3(0.94, 0.95, 0.97), milkC * 0.9,
                   tm * 1.1 + 5.0 + sway * 2.0, 0.0036 * es);
    col = mix(col, w2.rgb, w2.a);

    // ── glossy spheres: 3 anchors + beat drops ──
    for (int i = 0; i < 3; i++) {
        float fi = float(i);
        vec2 sc = vec2((0.34 + 0.36 * fi - 0.24 * fi * fi) * asp,
                       0.42 + 0.31 * sin(fi * 2.4 + 0.8));
        sc += 0.022 * vec2(sin(tm * (0.30 + 0.08 * fi) + fi * 2.1),
                           cos(tm * (0.38 + 0.06 * fi) + fi * 4.2));
        float r = (0.020 - 0.004 * fi) * es;
        vec3 base = (i == 0) ? mix(cool, vec3(0.1, 0.9, 0.7), 0.5)
                  : (i == 1) ? warm
                             : viol;
        // soft drop shadow into the fog
        vec2 shd = p - (sc + vec2(0.55, -1.1) * r);
        col *= 1.0 - 0.42 * exp(-dot(shd, shd) / (r * r * 3.2));
        vec4 s = sphere(p, sc, r, base);
        col = mix(col, s.rgb, s.a);
    }
    for (int i = 0; i < N_EVT; i++) {
        vec4 st = fetchEvt(i);
        float age = dec16(st.rg) * AGE_SPAN;
        float env = smoothstep(0.0, 0.30, age) * (1.0 - smoothstep(5.5, 8.2, age));
        if (env <= 0.001) continue;
        float h = hash21(st.ba * 91.7);
        vec2 sc = vec2(st.b * asp, st.a + age * 0.011);       // floats upward
        sc.x += 0.008 * sin(t * 3.0 + h * 6.28 + age * 0.5);
        float r = (0.013 + 0.007 * h) * es * smoothstep(0.0, 0.30, age);
        vec3 base = (h < 0.34) ? warm : (h < 0.67) ? cool : viol;
        base = mix(base, vec3(0.95, 0.9, 0.85), 0.12 * hash21(st.ba * 37.1));
        vec2 shd = p - (sc + vec2(0.55, -1.1) * r);
        col *= 1.0 - 0.38 * env * exp(-dot(shd, shd) / (r * r * 3.2));
        vec4 s = sphere(p, sc, r, base);
        col = mix(col, mix(col, s.rgb, env), s.a);
    }

    // ── tiny scattered specks ──
    for (int j = 0; j < 2; j++) {
        float fj = float(j);
        float cells = 34.0 + fj * 46.0;
        vec2 sp = p * cells;
        vec2 scd = floor(sp);
        float h = hash21(scd + fj * 71.0);
        if (h > 0.935) {
            vec2 dotp = vec2(hash21(scd + 3.3), hash21(scd + 6.1));
            float dpx = length(fract(sp) - dotp) * (RENDERSIZE.y / cells);
            float rr = 0.7 + 1.3 * hash21(scd + 9.7);
            float dm = smoothstep(rr + 0.7, rr - 0.7, dpx);
            vec3 spc = (hash21(scd + 13.1) > 0.6)
                     ? vec3(0.95)
                     : mix(warm, cool, hash21(scd + 21.3)) * 1.1;
            float twk = 0.55 + 0.45 * sin(tm * 2.4 + h * 40.0)
                      + 0.9 * gA * gHighP * step(0.8, hash21(scd + 51.0));
            col = mix(col, spc, dm * clamp(twk, 0.0, 1.0) * 0.8);
        }
    }

    // fine film grain
    float grain = hash21(uv * RENDERSIZE.xy + fract(TIME) * 37.1);
    col += (grain - 0.5) * 0.030;

    float lift = mix(1.0, 0.78 + 0.40 * knee(audioLevel, 0.03, 0.8), gA * 0.55);
    col *= brightness * lift;
    return vec4(clamp(col, 0.0, 1.0), 1.0);
}

void main() {
    gA     = clamp(audioReact, 0.0, 1.0);
    gBassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    gMidP  = pow(knee(audioMid,  0.08, 0.85), 1.3);
    gHighP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    gClk   = TIME * driftSpeed + audioTime * 0.3 * gA * driftSpeed;

    if (PASSINDEX == 0) gl_FragColor = passState();
    else                gl_FragColor = passImage();
}
