/*{
  "DESCRIPTION": "Static Field — a charged particle field: thousands of micro-sparks slowly build charge and shimmer, then discharge in bright chain-flashes as circular energy waves sweep the field; wandering electric sprites stitch glowing trails between them. Music energy charges the field faster, kicks launch discharge waves, highs make the sparks twinkle.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive",
    "Particles"
  ],
  "INPUTS": [
    {
      "NAME": "fieldDensity",
      "LABEL": "Field Density",
      "TYPE": "float",
      "DEFAULT": 56.0,
      "MIN": 20.0,
      "MAX": 110.0
    },
    {
      "NAME": "chargeRate",
      "LABEL": "Charge Rate",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 3.0
    },
    {
      "NAME": "waveSpeed",
      "LABEL": "Wave Speed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 2.5
    },
    {
      "NAME": "sparkle",
      "LABEL": "Sparkle",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "hazeAmt",
      "LABEL": "Haze",
      "TYPE": "float",
      "DEFAULT": 0.4,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "GROUP": "Audio Reactivity",
      "DEFAULT": 0.6,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "tintColor",
      "LABEL": "Tint",
      "TYPE": "color",
      "GROUP": "Color",
      "DEFAULT": [1.0, 1.0, 1.0, 1.0]
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 3.0
    }
  ],
  "PASSES": [
    {
      "TARGET": "fieldBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "sparkGlow",
      "PERSISTENT": true
    },
    {}
  ]
}*/

#define N_WAVES 3
#define N_SPRITES 6
#define TAU 6.283185307179586
#define AGE_SPAN 16.0

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float hash11(float x) { return fract(sin(x * 127.1 + 311.7) * 43758.5453); }
float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 443.897);
    p3 += dot(p3, p3.yzx + 19.19);
    return fract((p3.x + p3.y) * p3.z);
}
float vnoise2(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1, 0)), u.x),
               mix(hash21(i + vec2(0, 1)), hash21(i + vec2(1, 1)), u.x), u.y);
}
float fbm2(vec2 p) {
    float v = 0.0, a = 0.55;
    for (int k = 0; k < 4; k++) {
        v += a * vnoise2(p);
        p = p * 2.13 + 17.7;
        a *= 0.52;
    }
    return v;
}

vec2 enc16(float v) {
    float e = clamp(v, 0.0, 1.0) * 255.0;
    return vec2(floor(e) / 255.0, fract(e));
}
float dec16(vec2 t) { return t.x + t.y / 255.0; }

// wave state lives in bottom-row texels 0..N_WAVES-1 of fieldBuf:
// (age16 hi, age16 lo, centerX8, centerY8)
vec4 fetchWave(int w) {
    return texture2D(fieldBuf, vec2((float(w) + 0.5) / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
}

float waveVel() { return 0.30 * clamp(waveSpeed, 0.2, 2.5); }
float waveCycle(int w) { return 1.45 / waveVel() + mix(1.2, 3.2, hash11(float(w) * 5.31)); }

vec2 gridDims() {
    float D = clamp(fieldDensity, 20.0, 110.0);
    return vec2(D, max(D * RENDERSIZE.y / RENDERSIZE.x, 4.0));
}

// ---- pass 0: charge field + wave scheduler ----
vec4 passField() {
    float dt = clamp(TIMEDELTA, 0.001, 0.1);
    float ar = audioReact;
    float levelP = knee(audioLevel, 0.05, 0.90);
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;

    // --- bottom row: discharge-wave slots ---
    if (gl_FragCoord.y <= 1.0) {
        int w = int(gl_FragCoord.x);
        if (w >= N_WAVES) return vec4(0.0);
        vec4 st = fetchWave(w);
        float age = dec16(st.rg) * AGE_SPAN + dt;
        float cx = st.b, cy = st.a;
        float cyc = waveCycle(w);

        if (FRAMEINDEX < 2) {
            age = hash11(float(w) * 3.77) * cyc;
            cx = 0.2 + 0.6 * hash11(float(w) * 7.13);
            cy = 0.2 + 0.6 * hash11(float(w) * 11.9);
        }

        bool re = age > cyc;
        // kicks launch fresh discharge waves (per-slot travel refractory + gate)
        if (audioBeatPulse > mix(1.2, 0.45, ar) && age * waveVel() > 0.55
            && hash21(vec2(float(w) * 3.3, floor(TIME * 9.0))) < 0.45) re = true;

        if (re) {
            age = 0.0;
            cx = 0.15 + 0.7 * hash21(vec2(TIME * 5.11, float(w) * 2.7));
            cy = 0.15 + 0.7 * hash21(vec2(float(w) * 9.1, TIME * 3.37));
        }
        return vec4(enc16(min(age, AGE_SPAN - 0.01) / AGE_SPAN), cx, cy);
    }

    // --- everywhere else: per-cell charge accumulator ---
    vec2 grid = gridDims();
    vec2 cell = floor(uv * grid);
    vec2 cellPos = ((cell + 0.5) / grid - 0.5) * vec2(aspect, 1.0);
    float h = hash21(cell * 0.0173 + 0.71);

    vec4 prev = texture2D(fieldBuf, uv);
    float charge = dec16(prev.rg);
    float spark = prev.b;
    if (FRAMEINDEX < 2) {
        charge = h * 0.8;
        spark = 0.0;
    }

    // build-up: each cell charges at its own pace; music energy charges faster
    float drive = mix(0.45, 0.20 + 1.15 * levelP, ar);
    charge += dt * chargeRate * (0.25 + 0.75 * h) * 0.16 * drive;
    spark *= exp(-dt * 6.5);

    // discharge waves sweeping the field release charged cells
    for (int w = 0; w < N_WAVES; w++) {
        vec4 st = fetchWave(w);
        float age = dec16(st.rg) * AGE_SPAN;
        float r = age * waveVel();
        if (r > 1.5) continue;
        vec2 c = (vec2(st.b, st.a) - 0.5) * vec2(aspect, 1.0);
        if (abs(distance(cellPos, c) - r) < 0.05 && charge > 0.30) {
            spark = max(spark, charge);
            charge = 0.02;
        }
    }

    // fully-charged cells pop on their own — idle crackle in silence
    if (charge >= 1.0) {
        spark = max(spark, 1.0);
        charge = 0.03;
    }

    return vec4(enc16(charge), clamp(spark, 0.0, 1.0), 1.0);
}

// ---- pass 1: discharge flashes + sprites, with afterglow ----
vec4 passSparks() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    float ar = audioReact;
    float levelP = knee(audioLevel, 0.05, 0.90);
    float rowMask = smoothstep(0.0, 3.0, gl_FragCoord.y); // keep state row dark

    vec2 grid = gridDims();
    vec2 cell = floor(uv * grid);
    float h = hash21(cell * 0.0173 + 0.71);
    vec2 jit = (vec2(hash21(cell + 4.7), hash21(cell + 9.2)) - 0.5) * 0.42;
    vec2 local = fract(uv * grid) - 0.5 - jit;

    // discharge flash of this pixel's own cell (written THIS frame in pass 0)
    float spark = texture2D(fieldBuf, uv).b * rowMask;
    float r2 = dot(local, local);
    float cur = spark * spark * (exp(-r2 * 34.0) * 1.4 + exp(-r2 * 7.0) * 0.35);

    // wandering electric sprites with glowing trails (pure idle life)
    float spd = mix(1.0, 0.55 + 1.1 * levelP, ar);
    float spriteI = 0.0;
    for (int s = 0; s < N_SPRITES; s++) {
        float hs = hash11(float(s) * 9.73);
        vec2 sp = 0.5 + 0.40 * vec2(sin(TIME * (0.31 + 0.23 * hs) + hs * 17.0),
                                    sin(TIME * (0.43 + 0.19 * hs) + hs * 31.0));
        float d = length((uv - sp) * vec2(aspect, 1.0));
        spriteI += 0.0016 / (d + 0.0045) * (0.55 + 0.45 * sin(TIME * 2.3 + hs * TAU));
    }
    cur += min(spriteI, 2.0) * spd * 0.55 * rowMask;

    cur = min(cur, 2.2) * 0.42; // 8-bit headroom
    float keep = exp(-clamp(TIMEDELTA, 0.001, 0.1) * 3.1); // afterglow tail
    float prev = texture2D(sparkGlow, uv).r;
    float stored = max(prev * keep, cur);
    if (FRAMEINDEX < 2) stored = cur;
    return vec4(vec3(stored), 1.0);
}

// ---- pass 2: composite ----
vec4 passImage() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    vec2 p = (uv - 0.5) * vec2(aspect, 1.0);
    float ar = audioReact;
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float rowMask = smoothstep(0.0, 3.0, gl_FragCoord.y);

    // deep charged haze
    vec3 col = vec3(0.016, 0.019, 0.038);
    col += hazeAmt * fbm2(uv * vec2(3.0 * aspect, 3.0) + vec2(TIME * 0.020, -TIME * 0.013))
           * vec3(0.035, 0.055, 0.105);

    // the charge field itself: micro-dots shimmering as they fill
    vec2 grid = gridDims();
    vec2 cell = floor(uv * grid);
    float h = hash21(cell * 0.0173 + 0.71);
    vec2 jit = (vec2(hash21(cell + 4.7), hash21(cell + 9.2)) - 0.5) * 0.42;
    vec2 local = fract(uv * grid) - 0.5 - jit;
    float charge = dec16(texture2D(fieldBuf, uv).rg) * rowMask;

    // smooth per-cell twinkle; highs raise the twinkle depth (sparkle knob)
    float twAmp = 0.35 + 0.65 * clamp(sparkle * (0.4 + mix(0.3, 1.6 * highP, ar)), 0.0, 1.0);
    float tw = 1.0 - twAmp + twAmp * (0.5 + 0.5 * sin(TIME * (1.3 + h * 3.1) + h * TAU));
    float dotI = exp(-dot(local, local) * 30.0) * charge * charge * tw;
    col += dotI * vec3(0.38, 0.66, 1.0) * 1.05;

    // faint traveling wavefronts
    for (int w = 0; w < N_WAVES; w++) {
        vec4 st = fetchWave(w);
        float age = dec16(st.rg) * AGE_SPAN;
        float r = age * waveVel();
        if (r > 1.5) continue;
        vec2 c = (vec2(st.b, st.a) - 0.5) * vec2(aspect, 1.0);
        float ring = exp(-pow((distance(p, c) - r) * 42.0, 2.0));
        col += ring * smoothstep(0.0, 0.12, age) * (1.0 - r / 1.5) * vec3(0.10, 0.17, 0.34);
    }

    // discharge flashes, sprites and their afterglow
    float g = texture2D(sparkGlow, uv).r * 2.5;
    col += g * (vec3(0.45, 0.65, 1.0) + g * vec3(0.55, 0.5, 0.4));

    vec2 vp = uv - 0.5;
    col *= 1.0 - dot(vp, vp) * 0.5;
    col = max(col, vec3(0.007, 0.008, 0.014));

    return vec4(col * tintColor.rgb * brightness, 1.0);
}

void main() {
    if      (PASSINDEX == 0) gl_FragColor = passField();
    else if (PASSINDEX == 1) gl_FragColor = passSparks();
    else                     gl_FragColor = passImage();
}
