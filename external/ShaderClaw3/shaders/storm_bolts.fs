/*{
  "DESCRIPTION": "Storm Bolts — branching lightning strikes lash down from a churning storm sky, each bolt forking as it descends, flashing the clouds and leaving a fading afterglow. Kicks trigger fresh strikes, bass flares the flash, silence keeps a slow rolling storm alive.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "boltRate",
      "LABEL": "Bolt Rate",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 3.0
    },
    {
      "NAME": "forkiness",
      "LABEL": "Forkiness",
      "TYPE": "float",
      "DEFAULT": 1.6,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "glowAmt",
      "LABEL": "Afterglow",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "stormy",
      "LABEL": "Storm Density",
      "TYPE": "float",
      "DEFAULT": 0.6,
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
      "TARGET": "stormState",
      "PERSISTENT": true
    },
    {
      "TARGET": "stormGlow",
      "PERSISTENT": true
    },
    {}
  ]
}*/

#define N_BOLTS 6
#define TAU 6.283185307179586
#define AGE_SPAN 8.0
#define GROUND_Y 0.07

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float hash11(float x) { return fract(sin(x * 127.1 + 311.7) * 43758.5453); }
float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 443.897);
    p3 += dot(p3, p3.yzx + 19.19);
    return fract((p3.x + p3.y) * p3.z);
}
float vnoise(float x) {
    float i = floor(x), f = fract(x);
    float u = f * f * (3.0 - 2.0 * f);
    return mix(hash11(i), hash11(i + 1.0), u) * 2.0 - 1.0;
}
float vnoise2(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1, 0)), u.x),
               mix(hash21(i + vec2(0, 1)), hash21(i + vec2(1, 1)), u.x), u.y);
}
float fbm2(vec2 p) {
    float v = 0.0, a = 0.55;
    for (int k = 0; k < 5; k++) {
        v += a * vnoise2(p);
        p = p * 2.13 + 17.7;
        a *= 0.52;
    }
    return v;
}

// ---- bolt state: bottom row, texel i = (age16 hi, age16 lo, seed8, xpos8) ----
vec2 enc16(float v) {
    float e = clamp(v, 0.0, 1.0) * 255.0;
    return vec2(floor(e) / 255.0, fract(e));
}
float dec16(vec2 t) { return t.x + t.y / 255.0; }

vec4 fetchBolt(int i) {
    return texture2D(stormState, vec2((float(i) + 0.5) / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
}

float cycleFor(int i) {
    return mix(2.6, 6.5, hash11(float(i) * 7.31)) / clamp(boltRate, 0.2, 3.0);
}

// strike envelope: fast eased ramp-in, main flash + two restrikes, all smooth
float boltEnv(float age) {
    if (age < 0.0 || age > 0.85) return 0.0;
    float ramp = smoothstep(0.0, 0.055, age);
    float e = exp(-age * 6.0)
            + 0.60 * exp(-pow((age - 0.17) * 11.0, 2.0))
            + 0.32 * exp(-pow((age - 0.34) * 10.0, 2.0));
    return ramp * e;
}

// fractal descent path: anchored at the cloud base, wanders toward the ground
float boltPath(float y, float seed, float xpos) {
    float d = 0.0, amp = 1.0, fr = 2.7;
    for (int k = 0; k < 4; k++) {
        d += vnoise(y * fr + seed * 173.3 + float(k) * 37.71) * amp;
        amp *= 0.5;
        fr *= 2.35;
    }
    return xpos + d * 0.115 * mix(0.12, 1.0, 1.0 - y);
}

float lineGlow(float d, float w) { return w / (d * d + w); }

// ---- pass 0: strike scheduler (bottom-row state) ----
vec4 passState() {
    if (gl_FragCoord.y > 1.0) return vec4(0.0);
    int i = int(gl_FragCoord.x);
    if (i >= N_BOLTS) return vec4(0.0);

    vec4 st = fetchBolt(i);
    float age = dec16(st.rg) * AGE_SPAN;
    float seed = st.b;
    float xpos = st.a;
    float cyc = cycleFor(i);
    float dt = clamp(TIMEDELTA, 0.001, 0.1);
    age += dt;

    if (FRAMEINDEX < 2) {
        // stagger so the storm is alive immediately but not all-at-once
        age = 0.2 + hash11(float(i) * 3.17) * cyc;
        seed = hash11(float(i) * 9.71);
        xpos = 0.15 + 0.7 * hash11(float(i) * 5.37);
    }

    bool spawn = age > cyc;
    // beats call down extra strikes: per-slot refractory + rolling hash gate
    float ar = audioReact;
    if (audioBeatPulse > mix(1.2, 0.42, ar) && age > 0.9
        && hash21(vec2(float(i) * 3.7, floor(TIME * 11.0))) < 0.4) spawn = true;

    if (spawn) {
        age = 0.0;
        seed = hash21(vec2(TIME * 3.71, float(i) * 2.13));
        xpos = 0.12 + 0.76 * hash21(vec2(float(i) * 7.7, TIME * 1.93));
    }

    vec2 a16 = enc16(min(age, AGE_SPAN - 0.01) / AGE_SPAN);
    return vec4(a16, seed, xpos);
}

// per-pixel strike light for one bolt (main channel + forks + ground impact)
float strikeLight(vec2 uv, float aspect, float age, float seed, float xpos) {
    float env = boltEnv(age);
    if (env <= 0.001) return 0.0;

    // stepped leader: the tip races down in ~60ms, visible from spawn frame
    float tipY = mix(1.0, GROUND_Y, smoothstep(0.0, 0.06, age));
    float I = 0.0;

    if (uv.y >= tipY - 0.02) {
        float px = boltPath(uv.y, seed, xpos);
        float dx = (uv.x - px) * aspect;
        // white-hot channel + wide halo, gentle crawl in the crackle
        float crackle = 0.82 + 0.18 * vnoise(uv.y * 34.0 + TIME * 21.0 + seed * 50.0);
        I += lineGlow(dx, 3.5e-5) * crackle;
        I += lineGlow(dx, 2.6e-3) * 0.22;
    }

    // forks peel off the channel and fade as they descend
    for (int b = 0; b < 3; b++) {
        float alpha = clamp(forkiness - float(b), 0.0, 1.0);
        if (alpha <= 0.0) continue;
        float hb = hash11(seed * 91.7 + float(b) * 17.31);
        float yb = mix(0.30, 0.78, hb);
        if (uv.y < yb && uv.y >= tipY - 0.02) {
            float slope = (hash11(hb * 57.13) - 0.5) * 2.7;
            float bx = boltPath(yb, seed, xpos) + (yb - uv.y) * slope
                     + vnoise(uv.y * 21.0 + hb * 99.0) * 0.022;
            float dbx = (uv.x - bx) * aspect;
            float fadeB = exp(-(yb - uv.y) * 5.5) * alpha;
            I += (lineGlow(dbx, 2.2e-5) + lineGlow(dbx, 1.1e-3) * 0.18) * fadeB * 0.75;
        }
    }

    // ground impact bloom once the leader lands
    if (age > 0.06) {
        vec2 gp = vec2((uv.x - boltPath(GROUND_Y, seed, xpos)) * aspect, uv.y - GROUND_Y);
        I += exp(-length(gp) * 9.0) * 0.55;
    }

    return I * env;
}

// ---- pass 1: strike light + persistent afterglow ----
vec4 passGlow() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    float ar = audioReact;
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);

    float cur = 0.0;
    for (int i = 0; i < N_BOLTS; i++) {
        vec4 st = fetchBolt(i);
        float age = dec16(st.rg) * AGE_SPAN;
        cur += strikeLight(uv, aspect, age, st.b, st.a);
    }
    // bass flares the strike brightness (smoothed band, not an event)
    cur *= mix(1.0, 0.72 + 0.9 * bassP, ar);
    cur = min(cur, 2.6) * 0.35; // headroom for 8-bit storage

    float keep = exp(-clamp(TIMEDELTA, 0.001, 0.1) * mix(5.5, 1.15, glowAmt));
    float prev = texture2D(stormGlow, uv).r;
    float stored = max(prev * keep, cur);
    if (FRAMEINDEX < 2) stored = cur;
    return vec4(vec3(stored), 1.0);
}

// ---- pass 2: storm backdrop + composite ----
vec4 passImage() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    float ar = audioReact;
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP = pow(knee(audioMid, 0.08, 0.90), 1.3);
    float T = TIME;

    // churning cloud deck: domain-warped fbm, mids stir the turbulence gently
    vec2 cq = uv * vec2(2.6 * aspect, 2.1);
    float stir = mix(1.0, 0.85 + 0.5 * midP, ar);
    vec2 warp = vec2(fbm2(cq * 1.4 + vec2(T * 0.030 * stir, 0.0)),
                     fbm2(cq * 1.4 + vec2(0.0, T * 0.022 * stir) + 5.2));
    float cloud = fbm2(cq + warp * 1.15 + vec2(T * 0.045 * stir, T * 0.012));
    cloud = clamp(cloud * mix(0.75, 1.35, stormy), 0.0, 1.2);

    // sheet-lightning ambience: sum of strike envelopes lights the whole deck
    float flashE = 0.0;
    for (int i = 0; i < N_BOLTS; i++) {
        vec4 st = fetchBolt(i);
        flashE += boltEnv(dec16(st.rg) * AGE_SPAN);
    }
    flashE = min(flashE, 1.6);

    float skyLight = 0.30 + 0.38 * flashE + ar * 0.16 * bassP;
    // heavier sky at the top, faint glow band at the horizon
    vec3 cloudCol = mix(vec3(0.050, 0.060, 0.100), vec3(0.155, 0.185, 0.275), cloud);
    cloudCol *= mix(1.15, 0.62, uv.y);
    vec3 col = cloudCol * skyLight;
    col += vec3(0.06, 0.07, 0.11) * exp(-abs(uv.y - GROUND_Y) * 9.0) * (0.5 + flashE);

    // bolt channel: blue halo, white-hot core
    float g = texture2D(stormGlow, uv).r * 2.9;
    vec3 boltCol = g * mix(vec3(0.42, 0.58, 1.0), vec3(1.05, 1.03, 1.0),
                           clamp(g * 0.85 - 0.12, 0.0, 1.0));
    col += boltCol;

    // clouds catch the strike light (cheap 4-tap spread of the glow buffer)
    float gi = texture2D(stormGlow, uv + vec2(0.020, 0.012)).r
             + texture2D(stormGlow, uv + vec2(-0.020, 0.012)).r
             + texture2D(stormGlow, uv + vec2(0.012, -0.020)).r
             + texture2D(stormGlow, uv + vec2(-0.012, -0.020)).r;
    col += cloud * gi * 0.25 * vec3(0.40, 0.52, 0.92);

    // low ground silhouette
    col *= mix(0.13, 1.0, smoothstep(GROUND_Y - 0.025, GROUND_Y + 0.035, uv.y));

    // vignette, never letting the frame die to black
    vec2 vp = uv - 0.5;
    col *= 1.0 - dot(vp, vp) * 0.55;
    col = max(col, vec3(0.008, 0.009, 0.014));

    return vec4(col * tintColor.rgb * brightness, 1.0);
}

void main() {
    if      (PASSINDEX == 0) gl_FragColor = passState();
    else if (PASSINDEX == 1) gl_FragColor = passGlow();
    else                     gl_FragColor = passImage();
}
