/*{
  "DESCRIPTION": "Lava Scribble — a warm grey canvas where neon coral lava flows organically as a slow domain-warped metaball field with chartreuse patches, a crisp pale halo outline, and dense black scribble-hatch clusters (layered rotated stripes broken by hash noise) filling the negative space and hugging the lava's edges. Tiny paint speckles dust the whole surface. Mids agitate the scribble density and direction, bass grows the lava field a few percent, and each beat splashes a small new lava pocket that swells in visibly and slowly melts back into the flow.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "lavaHot",
      "LABEL": "Lava Hot Color",
      "TYPE": "color",
      "DEFAULT": [1.0, 0.30, 0.16, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "accentCol",
      "LABEL": "Accent (Chartreuse)",
      "TYPE": "color",
      "DEFAULT": [0.70, 0.88, 0.08, 1.0],
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
      "NAME": "lavaAmount",
      "LABEL": "Lava Coverage",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 1,
      "DEFAULT": 0.52,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "scribbleAmt",
      "LABEL": "Scribble Density",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.62,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "flowSpeed",
      "LABEL": "Flow Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 8,
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
      "TARGET": "lavaState",
      "PERSISTENT": true
    },
    {
    }
  ]
}*/

#define N_POCK 4
#define AGE_SPAN 9.0

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
    for (int k = 0; k < 5; k++) {
        v += a * vnoise(p);
        p = p * 2.11 + 17.3;
        a *= 0.52;
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

// state: bottom row texel i = (age16 hi, age16 lo, posx8, posy8)
vec2 enc16(float v) {
    float e = clamp(v, 0.0, 1.0) * 255.0;
    return vec2(floor(e) / 255.0, fract(e));
}
float dec16(vec2 t) { return t.x + t.y / 255.0; }
vec4 fetchPock(int i) {
    return texture2D(lavaState, vec2((float(i) + 0.5) / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
}

float gA, gBassP, gMidP, gHighP, gClk;

// ── pass 0: beat-splash pocket scheduler ──
vec4 passState() {
    if (gl_FragCoord.y > 1.0) return vec4(0.0);
    int i = int(gl_FragCoord.x);
    if (i >= N_POCK) return vec4(0.0);

    vec4 st = fetchPock(i);
    float age = dec16(st.rg) * AGE_SPAN;
    vec2 pos = st.ba;
    float dt = clamp(TIMEDELTA, 0.001, 0.1);
    age += dt;

    if (FRAMEINDEX < 2) {
        // one pocket visibly mid-splash from the very first frame
        age = 0.35 + 2.4 * float(i);
        pos = vec2(0.25 + 0.5 * hash11(float(i) * 5.31),
                   0.25 + 0.5 * hash11(float(i) * 9.17));
    }

    // beats splash a new pocket: per-slot refractory + rolling gate
    if (audioBeatPulse > mix(1.25, 0.45, gA) && age > 2.2
        && hash21(vec2(float(i) * 3.7, floor(TIME * 9.0))) < 0.38) {
        age = 0.0;
        pos = vec2(0.18 + 0.64 * hash21(vec2(TIME * 2.71, float(i) * 1.93)),
                   0.18 + 0.64 * hash21(vec2(float(i) * 7.13, TIME * 3.37)));
    }

    return vec4(enc16(min(age, AGE_SPAN - 0.01) / AGE_SPAN), pos);
}

// splash envelope: quick swell in, long organic melt-back
float pockEnv(float age) {
    if (age >= AGE_SPAN - 0.05) return 0.0;
    return smoothstep(0.0, 0.45, age) * exp(-age * 0.55);
}

// ── pass 1: the painting ──
vec4 passImage() {
    vec2 uv = isf_FragNormCoord.xy;
    float asp = RENDERSIZE.x / RENDERSIZE.y;
    vec2 p = vec2((uv.x - 0.5) * asp, uv.y - 0.5);
    float t = gClk * 0.05;

    float ps = paletteShift * 0.628;
    vec3 hot = hueRot(lavaHot.rgb, ps);
    vec3 acc = hueRot(accentCol.rgb, ps);

    // ── lava field: slow domain-warped metaball-ish flow ──
    float w1 = fbm(p * 1.35 + vec2(t * 0.7, 0.0));
    float w2 = fbm(p * 1.35 + vec2(4.7, -t * 0.55));
    vec2 wp = p * 2.05 + 1.45 * vec2(w1 - 0.5, w2 - 0.5);
    float F = fbm(wp + vec2(0.0, t * 0.4));
    // central pooling like the reference composition; corners breathe out
    F += 0.11 * exp(-dot(p - vec2(0.0, -0.06), p - vec2(0.0, -0.06)) * 2.6);
    F -= 0.06 * smoothstep(0.55, 0.95, length(p));

    // beat pockets swell the field locally (spawn visible, slow melt)
    for (int i = 0; i < N_POCK; i++) {
        vec4 st = fetchPock(i);
        float age = dec16(st.rg) * AGE_SPAN;
        vec2 pp = vec2((st.b - 0.5) * asp, st.a - 0.5);
        float env = pockEnv(age);
        float rad = 0.05 + 0.10 * smoothstep(0.0, 2.5, age);
        vec2 dv = p - pp;
        F += 0.30 * env * exp(-dot(dv, dv) / (rad * rad));
    }

    // threshold: coverage knob + bass grows the field a few percent
    float th = mix(0.82, 0.58, lavaAmount) - 0.022 * gA * gBassP;

    // pixel-space signed distance to the silhouette
    float e = max(fwidth(F), 1e-5);
    float sdPx = (F - th) / e;                    // + inside, − outside, ~pixels
    float lava = clamp(sdPx * 0.5 + 0.5, 0.0, 1.0);

    // ── warm grey canvas with tooth ──
    vec3 canvas = vec3(0.735, 0.715, 0.672);
    canvas *= 0.97 + 0.06 * fbm(p * 3.1 + 7.7);
    canvas *= 1.0 - 0.10 * smoothstep(0.35, 0.95, length(p));

    vec3 col = canvas;

    // ── scribble-hatch clusters (dense black ink) ──
    float cluster = smoothstep(0.46, 0.56,
        fbm(p * 1.5 + vec2(23.1, 8.8) + 0.06 * sin(t * 2.0)));
    // hug the lava edge: broken band just outside the halo
    float hug = smoothstep(-30.0, -14.0, sdPx) * smoothstep(-3.0, -8.0, sdPx);
    hug *= smoothstep(0.30, 0.55, fbm(p * 2.3 + 51.0));
    float region = max(cluster, hug * 0.95) * smoothstep(0.0, -2.5, sdPx);
    region = smoothstep(0.22, 0.62, region);   // near-binary clusters, solid black ink
    float densGate = scribbleAmt * (0.85 + 0.55 * gA * gMidP);  // mids densify

    float ink = 0.0;
    for (int j = 0; j < 4; j++) {
        float fj = float(j);
        // angular cells: each picks its own stroke direction
        vec2 cellP = p * (3.4 + fj * 1.15) + fj * 13.7;
        vec2 cid = floor(cellP);
        float h = hash21(cid + fj * 31.0);
        float ang = h * 6.2831 + fj * 2.3
                  + 0.55 * gA * gMidP * sin(t * 6.0 + h * 6.2831); // mids swing direction
        vec2 dir = vec2(cos(ang), sin(ang));
        float s = dot(p - cid * 0.13, dir) * (150.0 + 70.0 * h);
        float tri = abs(fract(s) - 0.5);
        float fws = max(fwidth(s), 1e-4);
        float stripe = smoothstep(0.21 + fws, 0.21 - fws, tri);
        // hash breaks along the stroke: short angular segments, not clean stripes
        float along = dot(p, vec2(-dir.y, dir.x));
        float brk = step(0.30 + 0.25 * (1.0 - densGate),
                         hash21(vec2(floor(s * 0.37), floor(along * 23.0)) + cid * 1.7 + fj * 5.3));
        // organic participation gate — continuous noise, no square cell blocks
        float cg = smoothstep(0.40 + 0.22 * (1.0 - clamp(densGate * 1.5, 0.0, 1.0)),
                              0.52, fbm(p * (2.0 + fj * 0.7) + fj * 19.3));
        ink = max(ink, stripe * brk * cg);
    }
    ink *= region * clamp(densGate * 2.0, 0.0, 1.0);
    col = mix(col, vec3(0.045, 0.042, 0.048), ink * 0.95);

    // ── tiny paint speckles everywhere ──
    for (int j = 0; j < 2; j++) {
        float fj = float(j);
        float cells = 60.0 + fj * 55.0;
        vec2 sp = vec2(uv.x * asp, uv.y) * cells;
        vec2 sc = floor(sp);
        float h = hash21(sc + fj * 91.0);
        if (h > 0.955) {
            vec2 dotp = vec2(hash21(sc + 3.1), hash21(sc + 5.7));
            float dpx = length(fract(sp) - dotp) * (RENDERSIZE.y / cells);
            float rr = 1.0 + 2.4 * hash21(sc + 8.3);
            float dm = smoothstep(rr + 0.8, rr - 0.8, dpx);
            vec3 spc = (hash21(sc + 12.9) > 0.5) ? vec3(0.86, 0.84, 0.93)
                                                 : mix(hot, acc, hash21(sc + 17.7)) * 0.9;
            // highs make a sparse subset of speckles twinkle
            float twk = 1.0 + 1.6 * gA * gHighP * step(0.85, hash21(sc + 44.0));
            col = mix(col, spc * min(twk, 1.6), dm * 0.85);
        }
    }

    // ── pale halo outline (2px), then the lava body ──
    float halo = smoothstep(-4.8, -2.2, sdPx) * smoothstep(1.2, -0.6, sdPx);
    vec3 haloCol = vec3(0.82, 0.81, 0.93);
    col = mix(col, haloCol, halo * 0.95);

    // interior shading: bright rim → deep core, chartreuse patches, red hotspots
    float depth = clamp((F - th) * 6.5, 0.0, 1.0);
    vec3 rimCol  = mix(hot, vec3(1.0, 0.66, 0.06), 0.48);       // orange edge
    vec3 coreCol = mix(hot, vec3(0.86, 0.06, 0.06), 0.42);      // red-orange heart
    vec3 body = mix(rimCol, coreCol, smoothstep(0.18, 0.85, depth) * 0.9);
    // crisp chartreuse islands
    float pf = fbm(p * 3.1 + vec2(41.0, 3.0) + 0.15 * vec2(w1, w2));
    float epf = max(fwidth(pf), 1e-4) * 1.5;
    float patch = smoothstep(0.60 - epf, 0.60 + epf, pf);
    body = mix(body, acc, patch * 0.94 * smoothstep(0.05, 0.20, depth));
    // gloss: pale in-lava droplets like wet paint
    float drop = smoothstep(0.75, 0.9, fbm(p * 6.5 + 77.0)) * smoothstep(0.3, 0.6, depth);
    body = mix(body, vec3(0.95, 0.9, 0.95), drop * 0.35);
    body *= 0.97 + 0.06 * vnoise(p * 40.0);

    col = mix(col, body, lava);

    // fine paper grain
    float grain = hash21(uv * RENDERSIZE.xy + fract(TIME) * 43.7);
    col += (grain - 0.5) * 0.022;

    float lift = mix(1.0, 0.78 + 0.40 * knee(audioLevel, 0.03, 0.8), gA * 0.6);
    col *= brightness * lift;
    return vec4(clamp(col, 0.0, 1.0), 1.0);
}

void main() {
    gA     = clamp(audioReact, 0.0, 1.0);
    gBassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    gMidP  = pow(knee(audioMid,  0.08, 0.85), 1.3);
    gHighP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    gClk   = TIME * flowSpeed + audioTime * 0.4 * gA * flowSpeed;

    if (PASSINDEX == 0) gl_FragColor = passState();
    else                gl_FragColor = passImage();
}
