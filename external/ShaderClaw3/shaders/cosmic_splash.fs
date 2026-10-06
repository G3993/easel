/*{
  "DESCRIPTION": "Cosmic Splash — a 1970s airbrush sci-fi poster: deep black void crossed diagonally by great sweeping comet streaks in cream, pink, cobalt and yellow with feathered airbrush edges, fine dot-spray speckle clusters along them, and one bold red circle accent sitting in its own dark halo. A persistent trail buffer lets onset- and beat-launched streak heads paint slowly-fading arcs across the poster. In silence the composed poster drifts slowly; onsets launch new streaks, highs shimmer the dot spray, bass swells the airbrush width. Streak Ink tints the comet family, Accent Circle recolors the red disc.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "streakWidth",
      "LABEL": "Streak Width",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "sprayAmount",
      "LABEL": "Dot Spray",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "driftSpeed",
      "LABEL": "Drift Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "trailFade",
      "LABEL": "Trail Fade",
      "TYPE": "float",
      "MIN": 0.9,
      "MAX": 0.995,
      "DEFAULT": 0.985,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorA",
      "LABEL": "Streak Ink",
      "TYPE": "color",
      "DEFAULT": [0.96, 0.92, 0.84, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "colorB",
      "LABEL": "Accent Circle",
      "TYPE": "color",
      "DEFAULT": [0.9, 0.08, 0.16, 1.0],
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
      "TARGET": "trailBuf",
      "PERSISTENT": true
    },
    {
    }
  ]
}*/

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

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float gA, gBassP, gHighP;
float gMinArc;   // proximity to the nearest painted arc (for dot-spray gating)

vec4 stAt(float ix) {
    return texture2D(trailBuf, vec2((ix + 0.5) / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
}

// Age packed hi/lo across two channels. Internal scale stays <= 255 so the
// math is safe in fp16 (mediump) AND each channel quantizes exactly to 8-bit
// for the native persistent buffers. Range 0..12s, resolution ~0.25ms.
float decodeAge(vec4 px) {
    float v = floor(px.r * 255.0 + 0.5) + floor(px.g * 255.0 + 0.5) / 255.0;
    return clamp(v / 255.0, 0.0, 1.0) * 12.0;
}
vec2 encodeAge(float age) {
    float v = clamp(age / 12.0, 0.0, 1.0) * 255.0;
    float hi = floor(v);
    float lo = floor(fract(v) * 255.0 + 0.5);
    return vec2(hi / 255.0, lo / 255.0);
}

vec3 streakBase(float kc) {
    if (kc < 0.5) return vec3(0.960, 0.930, 0.850);   // cream
    if (kc < 1.5) return vec3(0.945, 0.550, 0.740);   // pink
    if (kc < 2.5) return vec3(0.360, 0.400, 0.930);   // cobalt
    return vec3(0.950, 0.840, 0.230);                 // yellow
}

vec3 tintA() {
    return mix(vec3(1.0), clamp(colorA.rgb / vec3(0.960, 0.920, 0.840), 0.0, 4.0), colorA.a);
}

// Streak-head position on its gently curved crossing trajectory: a wide arc
// that sweeps through the poster and exits, never orbiting the center.
vec2 arcPos(float seed, float j, float age) {
    float h1 = hash11(seed * 37.1 + j * 5.3 + 1.1);
    float h2 = hash11(seed * 61.7 + j * 9.7 + 2.3);
    float h3 = hash11(seed * 17.9 + j * 3.1 + 4.7);
    float h4 = hash11(seed * 47.3 + j * 7.9 + 8.9);
    vec2 target = vec2(0.25 + 0.50 * h1, 0.25 + 0.50 * h2);
    float ang   = h3 * 6.28318;
    float cdist = 1.25 + 1.60 * h4;   // large radius = flat sweeping curvature
    vec2 C = target + cdist * vec2(cos(ang), sin(ang));
    float spd = 0.30 + 0.22 * hash11(seed * 91.3 + j * 3.7);
    float wv  = spd / cdist * (h1 > 0.5 ? 1.0 : -1.0);
    float thMid = atan(target.y - C.y, target.x - C.x);
    float th = thMid + wv * (age - 2.5);
    return C + cdist * vec2(cos(th), sin(th));
}

// ---- Pass 0: persistent trail canvas + bottom-row particle state ----
vec4 passTrail() {
    vec2 uv = isf_FragNormCoord.xy;

    if (gl_FragCoord.y < 1.0 && gl_FragCoord.x < 8.0) {
        float ix = floor(gl_FragCoord.x);
        vec4 ctl = stAt(6.0);
        float pOn = ctl.r;
        float pBt = ctl.g;
        float cd  = ctl.b;
        float rr  = floor(ctl.a * 8.0 + 0.5);
        float onNow = step(0.42, audioOnset);
        float btNow = step(0.5, audioBeat);
        float trig = 0.0;
        if (cd <= 0.001 && ((onNow > 0.5 && pOn < 0.5) || (btNow > 0.5 && pBt < 0.5))) trig = 1.0;
        trig *= step(0.05, gA);
        bool init = TIME < 0.10;

        float dtc = clamp(TIMEDELTA, 0.008, 0.06);
        if (ix > 5.5) {   // control pixel: onset/beat edges, cooldown, round-robin
            float cd2 = max(cd - dtc * 0.5, 0.0);   // ~2s between launches
            if (trig > 0.5) cd2 = 1.0;
            float rr2 = (trig > 0.5) ? mod(rr + 1.0, 5.0) : rr;
            if (init) { cd2 = 0.0; rr2 = 0.0; }
            return vec4(onNow, btNow, cd2, rr2 / 8.0);
        }
        if (ix < 4.5) {   // one streak-head slot per pixel
            float j = ix;
            vec4 px = stAt(j);
            float age  = decodeAge(px);
            float seed = clamp(px.b, 0.0, 0.996);
            age = min(age + dtc, 8.0);   // freeze well below encode range
            if (trig > 0.5 && abs(rr - j) < 0.5) {
                age = 0.0;
                seed = fract(hash11(mod(TIME, 37.0) * 5.31 + j * 13.7) + 0.31 * j);
            }
            if (init) { age = j * 1.35; seed = hash11(j * 7.7 + 2.9); }
            return vec4(encodeAge(age), seed, 1.0);
        }
        return vec4(0.0);
    }

    // trail field: decay + additive-max repaint of each head's recent stroke.
    // The stroke is a capsule over the last ~0.4s of trajectory, so it stays a
    // continuous painted line no matter the host frame step; max-blend makes
    // repainting overlap harmless.
    // Comet arcs rendered analytically from head state each frame:
    // the "trail" is the last ~3.2s of each head's trajectory, faded by age.
    vec3 col = vec3(0.0);
    vec2 asp = vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0);
    for (int j = 0; j < 5; j++) {
        float fj = float(j);
        vec4 px = stAt(fj);
        float age  = decodeAge(px);
        float seed = clamp(px.b, 0.0, 0.996);
        float life = 6.0;
        float inten = smoothstep(0.0, 0.15, age) * (1.0 - smoothstep(life * 0.55, life, age));
        if (!(inten > 0.003)) continue;
        // cheap reject: bounding test against the arc chord
        float span = min(age, 3.2);
        vec2 pHead = arcPos(seed, fj, age);
        vec2 pTail = arcPos(seed, fj, age - span);
        vec2 mid = (pHead + pTail) * 0.5;
        float rad = length((pHead - pTail) * asp) * 0.62 + 0.13;
        if (length((uv - mid) * asp) > rad) continue;
        float wid = 0.0060 * streakWidth * (1.0 + 0.35 * gA * gBassP);
        vec3 hcol; float kc = floor(hash11(seed * 23.7 + fj * 1.7 + paletteShift * 1.31) * 4.0);
        hcol = streakBase(kc) * tintA();
        vec2 pPrev = pTail;
        for (int k = 1; k <= 12; k++) {
            float tk = age - span + span * float(k) / 12.0;
            vec2 pk = arcPos(seed, fj, tk);
            vec2 pa = (uv - pPrev) * asp;
            vec2 ba = (pk - pPrev) * asp;
            float bb = max(dot(ba, ba), 1e-7);
            float h = clamp(dot(pa, ba) / bb, 0.0, 1.0);
            float d = length(pa - ba * h);
            float fade = exp(-(age - tk) * 0.85);           // older = fainter
            float wk = wid * (0.45 + 0.75 * fade);          // and thinner
            float body    = exp(-(d * d) / (wk * wk)) * 0.60;
            float feather = exp(-(d * d) / (wk * wk * 12.0)) * 0.34;
            col = max(col, hcol * (body + feather) * fade * inten);
            pPrev = pk;
        }
        // brilliant head core
        float dh = length((uv - pHead) * asp);
        col = max(col, hcol * exp(-(dh * dh) / (wid * wid * 0.6)) * inten);
    }
        return vec4(col, 1.0);
}

// One feathered airbrush comet arc; also feeds the dot-spray gate.
vec3 arcContrib(vec2 p, vec2 C, float r, float thC, float span, float w, vec3 colr) {
    vec2 rel = p - C;
    float dc = length(rel) - r;
    float th = atan(rel.y, rel.x);
    float dth = mod(th - thC + 3.14159, 6.28318) - 3.14159;
    float tt = dth / span;
    if (abs(tt) > 1.25) return vec3(0.0);
    float taper = smoothstep(-1.15, -0.55, tt) * smoothstep(1.15, -0.10, tt);
    float widG = w * (dc > 0.0 ? 2.8 : 1.15);   // asymmetric feather
    float prof = exp(-(dc * dc) / (widG * widG)) * 0.80;
    float core = exp(-(dc * dc) / (w * w * 0.10)) * 1.15;
    gMinArc = min(gMinArc, abs(dc) + (1.0 - taper) * 0.5);
    return colr * (prof + core) * taper;
}

// ---- Pass 1: composed poster + trails ----
vec4 passPresent() {
    vec2 uv = isf_FragNormCoord.xy;
    vec2 asp = vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0);
    vec2 p = (uv - 0.5) * asp + 0.5;

    vec3 col = vec3(0.016, 0.014, 0.022);
    gMinArc = 1e5;
    float drift = TIME * 0.012 * driftSpeed + audioTime * 0.02 * gA;
    float wS = streakWidth * (1.0 + 0.22 * gA * gBassP);
    vec3 tA = tintA();

    // the great diagonal sweeps
    col += arcContrib(p, vec2(1.52, -0.38), 1.36, 2.30 + drift * 0.50, 0.55, 0.055 * wS, vec3(0.940, 0.900, 0.800) * tA);
    col += arcContrib(p, vec2(1.30, -0.20), 1.18, 2.52 - drift * 0.40, 0.42, 0.030 * wS, vec3(0.300, 0.340, 0.920) * tA);
    col += arcContrib(p, vec2(-0.55, 1.45), 1.30, -0.95 + drift * 0.35, 0.38, 0.038 * wS, vec3(0.930, 0.520, 0.720) * tA);
    col += arcContrib(p, vec2(-0.40, -0.55), 1.05, 0.62 - drift * 0.30, 0.30, 0.024 * wS, vec3(0.950, 0.830, 0.250) * tA);
    // thin vermillion fringe hugging the cream sweep — the 70s airbrush rim
    col += arcContrib(p, vec2(1.52, -0.38), 1.415, 2.29 + drift * 0.50, 0.50, 0.010 * wS, vec3(0.880, 0.160, 0.180));

    // fine dot-spray speckle clusters hugging the streaks
    vec2 cell = floor(p * 150.0);
    float rnd = hash21(cell);
    vec2 jit = vec2(hash21(cell + 7.1), hash21(cell + 13.7)) - 0.5;
    vec2 cpos = (cell + 0.5 + jit * 0.9) / 150.0;
    float dd = length(p - cpos);
    float dsize = (0.10 + 0.30 * hash21(cell + 3.3)) / 150.0;
    float dotm = smoothstep(dsize, dsize * 0.5, dd);
    float cluster = step(0.35, hash21(floor(p * 9.0) + 1.7));
    float gate = smoothstep(0.09, 0.015, gMinArc) * step(0.62, rnd) * cluster;
    float shimmer = 0.75 + 0.25 * sin(TIME * 2.6 + rnd * 40.0);
    shimmer *= 1.0 + 0.9 * gA * gHighP * (0.4 + 0.6 * hash21(cell + 29.0));
    float kd = hash21(cell + 51.0);
    vec3 dcol = kd < 0.4 ? vec3(0.95, 0.90, 0.80)
              : (kd < 0.6 ? vec3(0.95, 0.55, 0.20)
              : (kd < 0.8 ? vec3(0.90, 0.20, 0.25) : vec3(0.95, 0.83, 0.25)));
    col += dcol * dotm * gate * shimmer * sprayAmount * 0.9;

    // bold red circle accent in a dark airbrushed halo
    vec2 cp = vec2(0.615, 0.660) + 0.012 * vec2(sin(TIME * 0.10), cos(TIME * 0.083));
    float dr = length(p - cp);
    float rc = 0.052;
    float halo = 1.0 - smoothstep(rc * 1.05, rc * 2.9, dr);
    col *= 1.0 - 0.80 * halo;
    col += colorB.rgb * 0.10 * exp(-pow(max(dr - rc, 0.0), 2.0) / (rc * rc * 0.5));
    float aaP = fwidth(dr) * 1.3 + 1e-5;
    float disc = smoothstep(rc, rc - aaP, dr);
    vec3 discCol = colorB.rgb * (1.0 - 0.35 * smoothstep(rc * 0.3, rc, dr));
    col = mix(col, discCol, disc);

    // fading streak trails (skip the state row)
    vec2 tuv = vec2(uv.x, max(uv.y, 2.5 / RENDERSIZE.y));
    vec3 tr = texture2D(trailBuf, tuv).rgb;
    col = 1.0 - (1.0 - clamp(col, 0.0, 1.0)) * (1.0 - tr * 0.78);

    // poster grain + vignette + level breath
    col += (hash21(gl_FragCoord.xy) - 0.5) * 0.020;
    col *= 1.0 - 0.30 * pow(length(uv - 0.5) * 1.35, 3.0);
    float lift = mix(1.0, 0.76 + 0.44 * knee(audioLevel, 0.03, 0.85), gA * 0.5);
    col *= brightness * lift;
    return vec4(max(col, 0.0), 1.0);
}

void main() {
    gA     = clamp(audioReact, 0.0, 1.0);
    gBassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    gHighP = pow(knee(audioHigh, 0.10, 0.90), 1.2);

    if (PASSINDEX == 0) gl_FragColor = passTrail();
    else                gl_FragColor = passPresent();
}
