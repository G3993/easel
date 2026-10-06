/*{
  "DESCRIPTION": "Resonance Rings — a struck membrane simulated with a real 2D wave equation: impacts press the surface down and concentric rings propagate outward, decay, and reflect off the frame edges. The membrane keeps a slow idle heartbeat of strikes; musical hits land new impacts whose rings ripple on for seconds.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Physical",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "waveSpeed",
      "LABEL": "Propagation",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "damping",
      "LABEL": "Damping",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "impactRate",
      "LABEL": "Impact Rate",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 3,
      "DEFAULT": 1.0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "impactSize",
      "LABEL": "Impact Size",
      "TYPE": "float",
      "MIN": 0.015,
      "MAX": 0.09,
      "DEFAULT": 0.035,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "ringContrast",
      "LABEL": "Ring Contrast",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 4,
      "DEFAULT": 1.8,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "GROUP": "Audio Reactivity",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6
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
      "MIN": 0.2,
      "MAX": 3.0,
      "DEFAULT": 1.0
    }
  ],
  "PASSES": [
    {
      "TARGET": "simBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

// ════════════════════════════════════════════════════════════════════
//  Resonance Rings — genuine damped 2D wave equation, leapfrog scheme:
//      h(t+1) = (2·h(t) − h(t−1) + c²·∇²h) · damp  +  impacts
//  State lives in a full-frame persistent 8-bit buffer (full-size passes
//  are what this host renders anyway): height packed 16-bit in rg,
//  previous height 16-bit in ba — the same hi/lo packing angel_hair uses.
//  Neighbor taps are clamped at the frame border (Neumann boundary), so
//  rings physically REFLECT off the edges and fold back across the field.
//  Impacts: 4 recurring idle strikes keep the membrane alive in silence;
//  2 beat slots hash new positions from accumulated bass time and press
//  the membrane on musical hits (visible the frame they land, since the
//  display pass reads the same buffer).
// ════════════════════════════════════════════════════════════════════

#define TAU 6.28318530718

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
float hash11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
vec2  hash22(float n) { return vec2(hash11(n), hash11(n + 17.31)); }

// ── 16-bit hi/lo packing in two 8-bit channels, range [-1, 1] ─────────
vec2 enc16(float v) {
    float e = clamp(v * 0.5 + 0.5, 0.0, 1.0) * 255.0;
    return vec2(floor(e) / 255.0, fract(e));
}
float dec16(vec2 t) { return (t.x + t.y / 255.0) * 2.0 - 1.0; }

float height(vec2 uv) { return dec16(texture2D(simBuf, uv).rg); }

// ── pass 0: wave-equation step + impact injection ─────────────────────
vec4 passSim() {
    vec2 res = RENDERSIZE.xy;
    vec2 uv  = gl_FragCoord.xy / res;
    vec2 px  = 1.0 / res;
    vec2 lo  = px * 0.5;
    vec2 hi  = 1.0 - px * 0.5;

    vec4 C = texture2D(simBuf, uv);
    float h  = dec16(C.rg);
    float hp = dec16(C.ba);

    // clamped neighbor taps → reflective (Neumann) frame edges
    float hL = height(clamp(uv + vec2(-px.x, 0.0), lo, hi));
    float hR = height(clamp(uv + vec2( px.x, 0.0), lo, hi));
    float hD = height(clamp(uv + vec2(0.0, -px.y), lo, hi));
    float hU = height(clamp(uv + vec2(0.0,  px.y), lo, hi));
    float lap = hL + hR + hD + hU - 4.0 * h;

    float c2   = 0.06 + 0.40 * clamp(waveSpeed, 0.0, 1.0);   // CFL-stable (< 0.5)
    float damp = mix(0.9992, 0.9930, clamp(damping, 0.0, 1.0));

    float hn = (2.0 * h - hp + c2 * lap) * damp;

    // ── impacts ──────────────────────────────────────────────────────
    float ar     = clamp(audioReact, 0.0, 1.0);
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float levelP = knee(audioLevel, 0.05, 0.90);
    float beatE  = pow(clamp(audioBeatPulse, 0.0, 1.0), 2.2);

    float aspect = res.x / res.y;
    vec2 pa = vec2(uv.x * aspect, uv.y);
    float sz = impactSize;
    float inj = 0.0;

    // 4 recurring strikes — the idle heartbeat of the membrane
    for (int s = 0; s < 4; s++) {
        float fs  = float(s);
        float per = (2.2 + 1.9 * hash11(fs * 3.31 + 1.7)) / max(impactRate, 0.2);
        float tt  = TIME / per + hash11(fs * 7.77);
        float cyc = floor(tt);
        float phs = (tt - cyc) * per;                 // seconds since this strike
        vec2 ip = vec2(0.16) + 0.68 * hash22(cyc * 13.73 + fs * 31.17);
        ip.x *= aspect;
        float envT = exp(-pow((phs - 0.05) / 0.055, 2.0));   // ~100 ms press
        float g = exp(-dot(pa - ip, pa - ip) / (sz * sz));
        float ampI = 0.16 * (0.55 + mix(0.0, 1.0 * bassP + 0.45 * levelP, ar));
        inj -= ampI * envT * g;
    }

    // 2 beat slots — new impacts on musical hits, position steps with
    // accumulated bass time so every hit lands somewhere fresh
    for (int s = 0; s < 2; s++) {
        float fs = float(s);
        float id = floor(audioBassTime * 0.9 + fs * 0.5);
        vec2 ip = vec2(0.16) + 0.68 * hash22(id * 17.39 + fs * 57.73);
        ip.x *= aspect;
        float g = exp(-dot(pa - ip, pa - ip) / (sz * sz));
        inj -= ar * 0.12 * beatE * (0.35 + 0.65 * bassP) * g;
    }

    hn += inj;

    // init: a standing center bump so rings are visible from frame 0
    if (FRAMEINDEX < 2) {
        vec2 c0 = vec2(0.5 * aspect, 0.5);
        hn = -0.55 * exp(-dot(pa - c0, pa - c0) / 0.004);
        h  = hn;
    }

    hn = clamp(hn, -0.999, 0.999);
    return vec4(enc16(hn), enc16(h));
}

// ── pass 1: render the membrane ───────────────────────────────────────
vec4 passImage() {
    vec2 res = RENDERSIZE.xy;
    vec2 uv  = gl_FragCoord.xy / res;
    vec2 px  = 1.0 / res;

    float ar     = clamp(audioReact, 0.0, 1.0);
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float levelP = knee(audioLevel, 0.05, 0.90);

    float h  = height(uv);
    float hR = height(clamp(uv + vec2(px.x, 0.0), px * 0.5, 1.0 - px * 0.5));
    float hU = height(clamp(uv + vec2(0.0, px.y), px * 0.5, 1.0 - px * 0.5));

    // sustained loudness swells ring contrast; dips below 1 in silence
    float gain = ringContrast * 3.2 * mix(1.0, 0.74 + 0.55 * levelP + 0.30 * bassP, ar);
    float v = h * gain;
    v = v / (1.0 + 0.45 * abs(v));   // soft limit

    // membrane lighting from the height gradient
    float gx = (hR - h) * res.x * 0.020 * ringContrast;
    float gy = (hU - h) * res.y * 0.020 * ringContrast;
    vec3 N = normalize(vec3(-gx, -gy, 1.0));
    vec3 L = normalize(vec3(0.50, 0.62, 0.72));
    float diff = max(dot(N, L), 0.0);
    float spec = pow(max(dot(N, normalize(L + vec3(0.0, 0.0, 1.0))), 0.0), 48.0);

    // base membrane — dark blue skin with a slow breathing sheen
    vec3 base = mix(vec3(0.020, 0.026, 0.048), vec3(0.034, 0.048, 0.082), uv.y);
    base *= 1.0 + 0.10 * sin(TIME * 0.23 + uv.x * 2.1);

    vec3 crest  = vec3(0.50, 0.80, 1.00);
    vec3 trough = vec3(0.85, 0.35, 0.95);
    vec3 col = base;
    col += crest  * max(v, 0.0) * 0.90;
    col += trough * max(-v, 0.0) * 0.80;
    col *= 0.55 + 0.55 * diff;
    col += vec3(0.90, 0.96, 1.00) * spec * 0.35 * min(abs(v) * 2.0 + 0.15, 1.0);

    // ring-edge glow — travelling fronts trace bright hairlines
    float edge = clamp(length(vec2(gx, gy)) * 0.9, 0.0, 1.0);
    col += vec3(0.22, 0.48, 0.80) * edge * 0.40;

    // vignette + floor (never fully black)
    vec2 q = uv - 0.5;
    col *= 1.0 - 0.28 * dot(q, q);
    col = max(col, vec3(0.012, 0.014, 0.022));

    return vec4(col * tintColor.rgb * brightness, 1.0);
}

void main() {
    if (PASSINDEX == 0) gl_FragColor = passSim();
    else                gl_FragColor = passImage();
}
