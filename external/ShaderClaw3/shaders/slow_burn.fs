/*{
  "DESCRIPTION": "Slow Burn — a smolder creeps across dark parchment: a ragged glowing burn front eats the surface, leaving charred coal veined with cooling embers, popping sparks at the edge and wisps of persistent rising smoke. The front literally advances with the music's energy; bass breathes the embers, highs pop the sparks.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Simulation",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "burnSpeed",
      "LABEL": "Burn Speed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.25,
      "MAX": 3.0
    },
    {
      "NAME": "edgeGlow",
      "LABEL": "Edge Glow",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 2.5
    },
    {
      "NAME": "sparkAmount",
      "LABEL": "Sparks",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 2.0
    },
    {
      "NAME": "smokeAmount",
      "LABEL": "Smoke",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 2.0
    },
    {
      "NAME": "paperScale",
      "LABEL": "Surface Detail",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.4,
      "MAX": 2.5
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
      "TARGET": "stateBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "smokeBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

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
    for (int i = 0; i < 5; i++) {
        r += a * vnoise(p);
        p = m * p * 2.03 + 11.7;
        a *= 0.5;
    }
    return r;
}

// ===== burn clock state: single texel at (0,0), 8-bit packed =====
// r,g = cycle progress c in [0,1) 16-bit; b = cycle index / 255
vec4 readState() {
    return texture2D(stateBuf, vec2(0.5 / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
}
float stClock(vec4 st) { return st.r + st.g / 255.0; }
float stCycle(vec4 st) { return floor(st.b * 255.0 + 0.5); }

// where this cycle's fire started
vec2 seedPos(float idx) {
    float A = RENDERSIZE.x / RENDERSIZE.y;
    vec2 h = hash2(vec2(idx * 0.373 + 2.13, idx * 0.911 + 5.7));
    return vec2((h.x * 2.0 - 1.0) * 0.72 * A, (h.y * 2.0 - 1.0) * 0.55);
}

// warped travel distance from the seed — the fbm warp is what makes the
// front ragged and creeping instead of a clean expanding circle
float travelD(vec2 p, float idx) {
    return length(p - seedPos(idx))
         + (fbm(p * 2.1 + idx * 5.31) - 0.5) * 0.80
         + (fbm(p * 6.4 - idx * 3.17) - 0.5) * 0.26;
}

// burn field: <0 unburnt, ~0 the glowing front, >0 char age behind the front
float burnA(vec2 p, float c, float idx) {
    return c * 5.2 - 0.30 - travelD(p, idx);
}

// pass 0 — the burn clock accumulates energy: the front advances with music
vec4 passState() {
    if (gl_FragCoord.x > 1.0 || gl_FragCoord.y > 1.0) return vec4(0.0);
    vec4 st = readState();
    float c = stClock(st);
    float idx = stCycle(st);

    float dt = clamp(TIMEDELTA, 0.0, 0.05);
    float energyP = knee(audioEnergy, 0.05, 0.90);
    float levelP = knee(audioLevel, 0.05, 0.90);
    // idle floor keeps it smoldering in silence; energy stokes the creep
    float adv = (0.62 + 0.13 * sin(TIME * 0.29))
              * mix(1.0, 0.45 + 1.30 * energyP + 0.35 * levelP, audioReact);
    c += dt * burnSpeed * adv / 24.0;

    if (FRAMEINDEX < 2) {
        c = 0.035; // start already smoldering, front visible from frame one
        idx = floor(mod(TIME * 7.3, 97.0));
    }
    if (c >= 1.0) {
        c -= 1.0;
        idx = mod(idx + 1.0, 251.0);
    }
    float e = c * 255.0;
    return vec4(floor(e) / 255.0, fract(e), idx / 255.0, 1.0);
}

// pass 1 — persistent smoke: rises with a curl wobble, injected at the front
vec4 passSmoke() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 p = (uv - 0.5) * 2.0;
    p.x *= RENDERSIZE.x / RENDERSIZE.y;

    vec4 st = readState();
    float c = stClock(st);
    float idx = stCycle(st);
    float dt = clamp(TIMEDELTA, 0.008, 0.05);
    float ar = audioReact;
    float midP = pow(knee(audioMid, 0.08, 0.90), 1.3);

    float wob = (fbm(vec2(uv.x * 4.5, uv.y * 3.0 - TIME * 0.23)) - 0.5) * 0.0045;
    vec2 from = uv - vec2(wob, (0.30 + 0.14 * fbm(vec2(uv.x * 2.0, TIME * 0.2))) * dt);
    float prev = texture2D(smokeBuf, from).r;
    prev = max(prev * 0.950 - 0.0030, 0.0); // linear leak beats 8-bit stall

    float A = burnA(p, c, idx);
    float band = exp(-A * A / 0.0028);
    // gusty, gapped injection so the smoke reads as wisps, not a wash
    float gusts = smoothstep(0.35, 0.75, fbm(p * 5.5 + vec2(0.0, -TIME * 0.6)));
    float inj = band * gusts * (0.25 + 0.30 * fbm(p * 6.0 + TIME * 0.5))
              * smokeAmount * dt * 2.4 * (1.0 + ar * 0.45 * midP);

    float s = clamp(prev + inj, 0.0, 1.0);
    if (FRAMEINDEX < 2) s = 0.0;
    return vec4(s, s, s, 1.0);
}

// pass 2 — parchment, scorch, glowing front, char + cooling embers, smoke
vec4 passImage() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 p = (uv - 0.5) * 2.0;
    p.x *= RENDERSIZE.x / RENDERSIZE.y;

    vec4 st = readState();
    float c = stClock(st);
    float idx = stCycle(st);

    float ar = audioReact;
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP = pow(knee(audioMid, 0.08, 0.90), 1.3);
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);

    float A = burnA(p, c, idx);

    // parchment with fibers and grain, dim but clearly alive
    float fib = fbm(p * 3.6 * paperScale + 7.0);
    float grain = vnoise(p * 70.0 * paperScale) * 0.14;
    vec3 paperBase = vec3(0.235, 0.185, 0.135) * (0.55 + 0.62 * fib) + grain * 0.10;
    // firelight spills onto the paper ahead of the front
    float spill = exp(-max(-A, 0.0) * 2.6);
    paperBase += vec3(0.45, 0.16, 0.04) * spill * 0.30
               * (0.8 + 0.2 * sin(TIME * 1.3 + fib * 9.0));

    // scorch browning just ahead of the front
    float scorch = smoothstep(-0.40, -0.02, A);
    vec3 paper = mix(paperBase, vec3(0.16, 0.085, 0.038), scorch * 0.8);

    // char behind the front: coal sheen veined with cooling embers
    vec3 charc = vec3(0.040, 0.036, 0.040) * (0.55 + 0.45 * fbm(p * 7.5 + 2.0));
    float veins = smoothstep(0.52, 0.86, fbm(p * 8.5 * paperScale - 4.2));
    float cool = exp(-max(A, 0.0) * 2.1);
    float breathe = 0.62 + 0.38 * sin(TIME * 1.15 + fib * 11.0 + p.y * 2.0);
    charc += vec3(1.0, 0.30, 0.045) * veins * cool * breathe
           * (0.42 + ar * 0.55 * bassP) * edgeGlow;

    float charm = smoothstep(0.0, 0.09, A);
    vec3 col = mix(paper, charc, charm);

    // the glowing burn front itself: orange rim with a white-hot filament
    float flick = 0.72 + 0.28 * fbm(vec2(p.x * 6.5 + p.y * 3.0, TIME * 1.6));
    float rim = exp(-A * A / 0.0035);
    vec3 rimCol = mix(vec3(1.05, 0.40, 0.05), vec3(1.25, 0.95, 0.55),
                      exp(-A * A / 0.0005));
    col += rimCol * rim * flick * edgeGlow * (0.95 + ar * 0.45 * midP);

    // sparks popping at the burn front — jittered cells, smooth flash envelopes
    vec2 cell = floor(p * 42.0);
    vec2 hc = hash2(cell + idx * 13.0);
    vec2 sPos = (cell + 0.15 + hc * 0.7) / 42.0;
    float Ac = burnA(sPos, c, idx);
    if (abs(Ac) < 0.13 && hc.x < 0.55 * sparkAmount) {
        float ph = fract(TIME * (0.55 + hc.y * 0.85) + hc.x * 7.0);
        float env = smoothstep(0.0, 0.18, ph) * (1.0 - smoothstep(0.30, 0.62, ph));
        vec2 dp = p - sPos;
        float d2 = dot(dp, dp);
        col += vec3(1.2, 0.75, 0.30) * env * exp(-d2 * 26000.0)
             * (0.8 + ar * 0.9 * highP) * sparkAmount * 1.4;
    }

    // smoke wisps: persistent buffer broken up by moving fbm, lit warm near front
    float sm = texture2D(smokeBuf, uv).r;
    sm *= 0.40 + 0.60 * fbm(p * 4.2 + vec2(0.0, -TIME * 0.30));
    vec3 smokeCol = mix(vec3(0.32, 0.31, 0.32), vec3(0.55, 0.38, 0.24),
                        exp(-A * A / 0.06) * 0.7);
    col = mix(col, smokeCol, clamp(sm * 0.65, 0.0, 0.55));

    // near the cycle's end the ash cools and fresh paper is laid — hides the loop
    float renew = smoothstep(0.955, 1.0, c);
    col = mix(col, paperBase, renew);

    col = 1.0 - exp(-col * 1.6);
    col = max(col, vec3(0.012));
    return vec4(col * tintColor.rgb * brightness, 1.0);
}

void main() {
    if      (PASSINDEX == 0) gl_FragColor = passState();
    else if (PASSINDEX == 1) gl_FragColor = passSmoke();
    else                     gl_FragColor = passImage();
}
