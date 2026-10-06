/*{
  "DESCRIPTION": "Ember Storm — a swirling column of embers and sparks rises off an unseen fire below the frame: curl-noise gusts, glowing advected trails, heat-haze shimmer over a dim coal glow. Bass gusts fling the embers sideways and upward, energy lengthens the trails, highs twinkle the hottest sparks.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Particles",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "emberRate",
      "LABEL": "Ember Density",
      "TYPE": "float",
      "DEFAULT": 0.75,
      "MIN": 0.15,
      "MAX": 1.0
    },
    {
      "NAME": "riseSpeed",
      "LABEL": "Rise Speed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.3,
      "MAX": 2.5
    },
    {
      "NAME": "swirl",
      "LABEL": "Swirl",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 2.2
    },
    {
      "NAME": "heatHaze",
      "LABEL": "Heat Haze",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 2.5
    },
    {
      "NAME": "trailGlow",
      "LABEL": "Trail Glow",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 2.0
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
      "TARGET": "agentBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "trailBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

#define N_EMBERS 220
#define TAU 6.283185307179586

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float hash1(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
vec2 hash2(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * vec3(443.897, 441.423, 437.195));
    p3 += dot(p3.zxy, p3.yxz + 19.19);
    return fract(vec2(p3.x * p3.y, p3.z * p3.x));
}
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash1(i), hash1(i + vec2(1, 0)), u.x),
               mix(hash1(i + vec2(0, 1)), hash1(i + vec2(1, 1)), u.x), u.y);
}
float fbm(vec2 p) {
    float a = 0.5, r = 0.0;
    mat2 m = mat2(0.8, -0.6, 0.6, 0.8);
    for (int i = 0; i < 4; i++) {
        r += a * vnoise(p);
        p = m * p * 2.03 + 11.7;
        a *= 0.5;
    }
    return r;
}
// divergence-free swirl field from noise (finite-difference curl)
vec2 curl(vec2 p) {
    float e = 0.12;
    float nx = fbm(p + vec2(0.0, e)) - fbm(p - vec2(0.0, e));
    float ny = fbm(p + vec2(e, 0.0)) - fbm(p - vec2(e, 0.0));
    return vec2(nx, -ny) / (2.0 * e);
}

// ===== ember state: 3 texels each on the bottom pixel row, 8-bit packed =====
// texel 3i   : position, [-2.2, 2.2] per axis, 16-bit
// texel 3i+1 : velocity angle 16-bit + speed/3 8-bit
// texel 3i+2 : age 16-bit (0..1 of life)
vec4 encPos(vec2 p) {
    p = clamp((p + 2.2) / 4.4, 0.0, 1.0) * 255.0;
    return vec4(floor(p.x) / 255.0, fract(p.x), floor(p.y) / 255.0, fract(p.y));
}
vec2 decPos(vec4 t) { return vec2(t.r + t.g / 255.0, t.b + t.a / 255.0) * 4.4 - 2.2; }
vec4 encVel(vec2 v) {
    float e = fract(atan(v.y, v.x) / TAU) * 255.0;
    return vec4(floor(e) / 255.0, fract(e), clamp(length(v) / 3.0, 0.0, 1.0), 1.0);
}
vec4 encAge(float a) {
    float e = clamp(a, 0.0, 1.0) * 255.0;
    return vec4(floor(e) / 255.0, fract(e), 0.0, 1.0);
}
vec4 fetchT(int texel) {
    return texture2D(agentBuf,
                     vec2((float(texel) + 0.5) / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
}
vec2 emberPos(int i) { return decPos(fetchT(3 * i)); }
vec2 emberVel(int i) {
    vec4 s = fetchT(3 * i + 1);
    float ang = (s.r + s.g / 255.0) * TAU;
    return vec2(cos(ang), sin(ang)) * s.b * 3.0;
}
float emberAge(int i) {
    vec4 s = fetchT(3 * i + 2);
    return s.r + s.g / 255.0;
}

// white-yellow when hot and young -> orange -> dull red as it cools
vec3 emberRamp(float heat) {
    vec3 c = mix(vec3(0.30, 0.03, 0.005), vec3(1.05, 0.34, 0.03),
                 smoothstep(0.15, 0.62, heat));
    return mix(c, vec3(1.25, 0.98, 0.62), smoothstep(0.72, 1.0, heat));
}

// pass 0 — ember flight: buoyancy + curl swirl + bass gusts, with inertia
vec4 passAgents() {
    if (gl_FragCoord.y > 1.0) return vec4(0.0);
    int texel = int(gl_FragCoord.x);
    int i = texel / 3;
    int slot = texel - 3 * i;
    if (i >= N_EMBERS) return vec4(0.0);

    float ar = audioReact;
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float dt = clamp(TIMEDELTA, 0.008, 0.05);
    float A = RENDERSIZE.x / RENDERSIZE.y;

    float fi = float(i);
    vec2 h = hash2(vec2(fi * 0.713, 3.7));
    vec2 h2 = hash2(vec2(fi * 1.291, 8.3));
    float life = mix(3.0, 6.5, h.x);

    vec2 pos = emberPos(i);
    vec2 vel = emberVel(i);
    float age = emberAge(i);
    float heat = 1.0 - age;

    // flow field: rising storm column with swirl; gusts ride the bass with a
    // per-ember phase lag so the storm leans, never snaps
    vec2 flow = curl(pos * 2.6 + vec2(0.0, -TIME * 0.33) + h.y * 4.0)
              * swirl * 0.45;
    flow.y += riseSpeed * (0.42 + 0.65 * heat);
    float gust = ar * bassP;
    flow.x += sin(TIME * 0.6 + h2.x * TAU + pos.y * 1.4) * gust * 1.15;
    flow.y += gust * 0.85;

    vel = mix(vel, flow, 0.075); // inertia: impulse in, physics out
    pos += vel * dt * 0.62;
    age += dt / life;

    if (age >= 1.0 || pos.y > 1.35 || abs(pos.x) > A * 1.35) {
        pos = vec2((h2.y * 2.0 - 1.0) * A * 1.05, -1.08 - h.y * 0.18);
        vel = vec2(0.0, 0.4);
        age = fract(age) * 0.05;
    }
    if (FRAMEINDEX < 12) {
        // scatter across the canvas so the storm is visible from frame one
        vec2 s = hash2(vec2(fi * 0.437, 11.9));
        pos = vec2((s.x * 2.0 - 1.0) * A * 1.1, s.y * 2.4 - 1.15);
        vel = vec2(0.0, 0.4);
        age = hash1(vec2(fi, 5.2));
    }

    if (slot == 0) return encPos(pos);
    if (slot == 1) return encVel(vel);
    return encAge(age);
}

// pass 1 — glowing trails: advect the buffer upward, stamp the embers in
vec4 passTrail() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 p = (uv - 0.5) * 2.0;
    p.x *= RENDERSIZE.x / RENDERSIZE.y;

    float ar = audioReact;
    float energyP = knee(audioEnergy, 0.05, 0.90);
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float dt = clamp(TIMEDELTA, 0.008, 0.05);

    // trails rise and waver like the heat column that carries them
    float wob = (fbm(vec2(uv.x * 5.0, uv.y * 3.0 - TIME * 0.4)) - 0.5) * 0.0022;
    vec2 from = uv - vec2(wob, riseSpeed * dt * 0.075);
    vec3 prev = texture2D(trailBuf, from).rgb;
    float decay = mix(0.885, 0.925, ar * energyP); // energy lengthens trails
    prev = max(prev * decay - 0.0035, 0.0);        // linear term kills 8-bit ghosting

    vec3 ink = vec3(0.0);
    for (int i = 0; i < N_EMBERS; i++) {
        if (hash1(vec2(float(i) * 0.911, 2.3)) > emberRate) continue;
        vec2 dp = p - emberPos(i);
        float d2 = dot(dp, dp);
        if (d2 > 0.0009) continue; // far embers can't touch this pixel
        float age = emberAge(i);
        float heat = 1.0 - age;
        float hh = hash1(vec2(float(i), 7.7));
        float size = mix(0.5, 1.4, hh) * (0.55 + 0.45 * heat);
        float b = size * 4.5e-7 / (d2 + 7e-6);
        // smooth twinkle on the hottest sparks, highs lift it gently
        float tw = 0.8 + 0.2 * sin(TIME * (2.5 + 3.5 * hh) + hh * 31.0);
        b *= mix(1.0, tw, heat) * (1.0 + ar * 0.5 * highP * heat);
        b *= 1.0 - smoothstep(0.82, 1.0, age); // gutter out at end of life
        ink += emberRamp(heat) * b;
    }

    vec3 stored = clamp(prev + ink, 0.0, 1.0);
    if (FRAMEINDEX < 2) stored = vec3(0.0);
    return vec4(stored, 1.0);
}

// pass 2 — composite through heat haze over the coal-glow dark
vec4 passImage() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float ar = audioReact;
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float levelP = knee(audioLevel, 0.05, 0.90);

    // heat-haze refraction: strongest near the unseen fire at the bottom
    float hz = heatHaze * (1.0 + ar * 0.5 * bassP);
    float n1 = fbm(vec2(uv.x * 6.0, uv.y * 9.0 - TIME * 1.35));
    float n2 = fbm(vec2(uv.x * 7.0 + 4.7, uv.y * 8.0 - TIME * 1.1));
    vec2 off = (vec2(n1, n2) - 0.5) * hz * (0.0035 + 0.010 * exp(-uv.y * 2.4));

    vec3 glow = texture2D(trailBuf, uv + off).rgb;
    // cheap cross-blur bloom halo around the trails
    vec2 px = 2.5 / RENDERSIZE.xy;
    vec3 halo = texture2D(trailBuf, uv + off + vec2(px.x, 0.0)).rgb
              + texture2D(trailBuf, uv + off - vec2(px.x, 0.0)).rgb
              + texture2D(trailBuf, uv + off + vec2(0.0, px.y)).rgb
              + texture2D(trailBuf, uv + off - vec2(0.0, px.y)).rgb;

    // the unseen fire: deep coal glow breathing at the bottom edge, never black
    float flick = 0.7 + 0.3 * fbm(vec2(uv.x * 3.5 + off.x * 60.0, TIME * 0.55));
    vec3 bg = vec3(0.020, 0.012, 0.017) * (1.1 - uv.y * 0.55);
    bg += vec3(0.50, 0.135, 0.032) * exp(-uv.y * 3.4) * flick
        * (0.55 + ar * (0.5 * bassP + 0.2 * levelP));
    // faint drifting smoke lit from below
    float sm = fbm(vec2(uv.x * 2.6 + off.x * 30.0, uv.y * 3.4 - TIME * 0.22));
    bg += vec3(0.085, 0.055, 0.050) * sm * sm * (0.35 + 0.65 * (1.0 - uv.y));

    vec3 col = bg + glow * 2.0 * trailGlow + halo * 0.22 * trailGlow;
    col = 1.0 - exp(-col * 1.55);
    col = max(col, vec3(0.012));
    return vec4(col * tintColor.rgb * brightness, 1.0);
}

void main() {
    if      (PASSINDEX == 0) gl_FragColor = passAgents();
    else if (PASSINDEX == 1) gl_FragColor = passTrail();
    else                     gl_FragColor = passImage();
}
