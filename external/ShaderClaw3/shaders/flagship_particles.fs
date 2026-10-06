/*{
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Stem Swarm — EaselAudio flagship. Three interleaved particle populations bound to stems: heavy bass motes that swell and brighten with stemBass, drum sparks born on stemDrumsHit that fly apart as the hit decays, and melody streamers riding a curl-noise flow advected by audioMidTime. Time clocks keep every motion smooth; phase ramps re-choreograph the swarm per bar. Alive in silence, swarming with music.",
  "CREDIT": "ShaderClaw3 EaselAudio flagship",
  "INPUTS": [
    {
      "NAME": "glowAmount",
      "LABEL": "Glow / Haze",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "moteAmount",
      "LABEL": "Bass Motes",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "sparkAmount",
      "LABEL": "Drum Sparks",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "streamerAmount",
      "LABEL": "Melody Streamers",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "swarmScale",
      "LABEL": "Swarm Scale",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReactivity",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  STEM SWARM (id 1201) — EaselAudio engine flagship, particle edition.
//
//  Populations (each an independently controllable layer):
//    • MOTES     — few, huge, slow. Orbits driven by audioBassTime (a clock
//                  that only advances when bass plays), radius+ember glow
//                  breathe with stemBass. Frequency→space: bass = big.
//    • SPARKS    — many, tiny, fast. Born bright on stemDrumsHit (an
//                  AD-enveloped bus signal), they fly outward from their
//                  nest as the envelope decays (event → finite life).
//    • STREAMERS — curl-noise ribbons advected by audioMidTime; stemMelody
//                  tightens and brightens the flow. audioPhase4 gently
//                  re-phases the ribbons each bar (structure on beats).
//    • GLOW      — self-bloom + stemAir shimmer haze (highs = sparkle).
//
//  Playbook compliance: no raw audio→position (Time clocks integrate),
//  soft knees everywhere, idle floor keeps the swarm alive in silence.
// ════════════════════════════════════════════════════════════════════════

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }

vec2 hash22(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.xx + p3.yz) * p3.zy);
}

float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
    return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 3; i++) {
        v += a * vnoise(p);
        p = p * 2.03 + vec2(17.7, 9.2);
        a *= 0.5;
    }
    return v;
}

// curl of the fbm field — divergence-free flow for the streamers
vec2 curl2(vec2 p) {
    float e = 0.14;
    float n1 = fbm(p + vec2(0.0, e));
    float n2 = fbm(p - vec2(0.0, e));
    float n3 = fbm(p + vec2(e, 0.0));
    float n4 = fbm(p - vec2(e, 0.0));
    return vec2(n1 - n2, n4 - n3) / (2.0 * e);
}

// axis-angle hue rotation (cheap, keeps luma roughly stable)
vec3 hueRot(vec3 c, float a) {
    const vec3 k = vec3(0.57735);
    float ca = cos(a), sa = sin(a);
    return c * ca + cross(k, c) * sa + k * dot(k, c) * (1.0 - ca);
}

void main() {
    vec2 p = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y;
    float px = 1.0 / RENDERSIZE.y;   // one pixel in p-units — all edges in PIXEL widths

    // ── audio conditioning (soft knees + idle floor; law 6/7) ──
    float react  = audioReactivity;
    float rA     = min(react, 1.5);
    float bassP  = pow(knee(stemBass,   0.06, 0.85), 1.4);   // structural weight
    float melP   = pow(knee(stemMelody, 0.06, 0.85), 1.2);   // ribbon body
    float airP   = pow(knee(stemAir,    0.08, 0.90), 1.2);   // sparkle haze
    float drumE  = pow(clamp(stemDrumsHit, 0.0, 1.0), 1.2);  // AD-enveloped hit
    float drive  = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9); // never zero

    // ── Time clocks: THE smooth motion drive. Advance with the music,
    //    idle at a gentle TIME base so silence still drifts. ──
    float tBass = TIME * 0.16 + audioBassTime * 0.55 * rA;
    float tMel  = TIME * 0.12 + audioMidTime  * 0.60 * rA;
    float tAir  = TIME * 0.22 + audioHighTime * 0.50 * rA;

    // bar-phase choreography: a soft cyclic sway, never a ramp snap
    float barSway = sin(audioPhase4 * 6.28318) * 0.35 * rA;

    // ── background: deep nebula wash (native), replaceable via bgColor ──
    float bgn = fbm(p * 1.5 + vec2(tBass * 0.10, -tMel * 0.06));
    vec3 nativeBg = mix(vec3(0.045, 0.075, 0.155), vec3(0.150, 0.060, 0.190), bgn);
    nativeBg += vec3(0.015, 0.045, 0.085) * smoothstep(0.95, -0.35, length(p));
    nativeBg *= 0.85 + 0.35 * drive;
    vec3 base = (bgColor.a > 0.001) ? mix(nativeBg, bgColor.rgb, bgColor.a) : nativeBg;
    vec3 col = base;

    // ═══ LAYER 0 — pinpoint starfield (crisp 1-2px dust behind the swarm) ═══
    // Static anchors, slow smooth twinkle: fine detail that makes full-screen
    // rendering read high-res without touching the silence noise floor.
    {
        float stScale = 24.0;
        vec2 sgrid = p * stScale;
        vec2 sid = floor(sgrid);
        vec2 sh2 = hash22(sid);
        float sgate = step(0.50, hash21(sid + 4.7));
        vec2 sc = sid + 0.15 + 0.70 * sh2;
        float sr = (0.9 + 0.9 * sh2.y) * px * stScale;        // 0.9-1.8 px radius
        float sd = length(sgrid - sc);
        float star = smoothstep(sr + 1.5 * px * stScale, sr - 0.5 * px * stScale, sd);
        float tw = 0.74 + 0.26 * sin(TIME * (0.4 + 0.6 * sh2.x) + sh2.y * 6.28318);
        col += vec3(0.82, 0.88, 1.00) * star * sgate * tw * (0.55 + 0.35 * drive);
        // finer second dust octave — half-pixel pinpricks
        vec2 sgrid2 = p * 44.0 + 17.3;
        vec2 sid2 = floor(sgrid2);
        float sgate2 = step(0.80, hash21(sid2 + 9.1));
        vec2 sc2 = sid2 + 0.2 + 0.6 * hash22(sid2);
        float star2 = smoothstep(2.0 * px * 44.0, 0.6 * px * 44.0, length(sgrid2 - sc2));
        col += vec3(0.70, 0.78, 0.95) * star2 * sgate2 * 0.40 * (0.5 + 0.5 * drive);
    }

    // ═══ LAYER 1 — bass motes (big, slow, breathing) ═══
    if (moteAmount > 0.001) {
        float mScale = 2.6 / swarmScale;
        vec2 mg = p * mScale + vec2(tBass * 0.14, tBass * 0.045);
        vec2 mBase = floor(mg);
        vec3 moteCol = vec3(0.0);
        for (int i = -1; i <= 1; i++)
        for (int j = -1; j <= 1; j++) {
            vec2 id = mBase + vec2(float(i), float(j));
            vec2 h = hash22(id);
            float lag = h.x * 6.28318;                 // per-element phase lag (law 3)
            vec2 c = id + 0.5 + (0.30 + 0.06 * h.y) * vec2(
                sin(tBass * (0.55 + 0.45 * h.x) + lag + barSway),
                cos(tBass * (0.65 + 0.35 * h.y) + lag * 1.7));
            float rad = (0.15 + 0.15 * h.y) * (1.0 + 0.45 * rA * bassP);
            float d2 = dot(mg - c, mg - c);
            float g = exp(-d2 / (rad * rad + 1e-5));
            vec3 ember = mix(vec3(1.00, 0.42, 0.14), vec3(0.88, 0.20, 0.56), h.x);
            float lum = 0.34 + 0.26 * drive
                      + 0.75 * rA * bassP * (0.65 + 0.35 * sin(TIME * 1.1 + lag));
            // crisp core + thin rim (pixel-width AA) UNDER the soft glow:
            // the ember reads sharp full-screen, the halo keeps the atmosphere
            float rr  = sqrt(d2);
            float pxm = px * mScale;                       // one pixel in grid units
            float core = smoothstep(rad * 0.40 + 1.5 * pxm, rad * 0.40 - 1.5 * pxm, rr);
            float rim  = smoothstep(1.4 * pxm, 0.0, abs(rr - rad * 0.60));
            moteCol += ember * (g * 0.72 + core * 1.05 + rim * 0.80) * lum;
        }
        col += moteCol * moteAmount * 0.85;
    }

    // ═══ LAYER 2 — drum sparks (born on stemDrumsHit, fly as it decays) ═══
    if (sparkAmount > 0.001) {
        float sScale = 8.5 / swarmScale;
        vec2 sg = p * sScale + vec2(0.0, -tAir * 0.35);
        vec2 sBase = floor(sg);
        // nest re-roll clock: advances with the mix (audioTime pauses in silence)
        float seedIdx = floor(audioTime * 1.6 + TIME * 0.10);
        float fly = (1.0 - drumE) * 0.42 * (0.4 + 0.6 * rA);  // event → outward travel
        vec3 sparkCol = vec3(0.0);
        for (int i = -1; i <= 1; i++)
        for (int j = -1; j <= 1; j++) {
            vec2 id = sBase + vec2(float(i), float(j));
            vec2 h = hash22(id + seedIdx * 13.13);
            float gateH = hash21(id * 1.618 + seedIdx);
            // spark density grows with drum presence; sparse in quiet passages
            float gate = step(0.62 - 0.22 * clamp(stemDrumsPresence, 0.0, 1.0), gateH);
            vec2 nest = id + 0.18 + 0.64 * h;
            vec2 dir = normalize(h - 0.5 + 1e-4);
            vec2 c = nest + dir * fly;
            float d2 = dot(sg - c, sg - c);
            float core = exp(-d2 * 70.0);
            // pixel-sharp pinpoint at the spark heart (crisp core + soft halo)
            float pxs = px * sScale;
            float pin = smoothstep(1.6 * pxs, 0.4 * pxs, sqrt(d2));
            float tw = 0.055 + 0.05 * sin(TIME * (2.0 + 3.0 * gateH) + h.x * 6.28318);
            float amp = tw + 1.55 * rA * drumE;
            sparkCol += mix(vec3(1.00, 0.92, 0.62), vec3(0.68, 0.88, 1.00), gateH)
                      * (core * 0.55 + pin * 0.95) * amp * gate;
        }
        col += sparkCol * sparkAmount;
    }

    // ═══ LAYER 3 — melody streamers (curl-flow ribbons on audioMidTime) ═══
    if (streamerAmount > 0.001) {
        vec2 q = p * (1.7 * swarmScale);
        q.y += tMel * 0.20;
        vec2 cu = curl2(q * 0.85 + vec2(0.0, tMel * 0.10));
        q += cu * (0.34 + 0.26 * rA * melP);
        float ph = q.x * 4.2 + fbm(q * 2.2) * 5.2 + tMel * 1.5 + barSway;
        float band = sin(ph);
        float streakA = pow(clamp(0.5 + 0.5 * band, 0.0, 1.0), 7.0);
        float streakB = pow(clamp(0.5 - 0.5 * band, 0.0, 1.0), 13.0);
        // crisp ribbon centerlines: tight value-threshold cores riding the
        // soft streaks (sharp thread + wide glow = high-res AND atmospheric)
        float lineA = smoothstep(0.955, 0.990, 0.5 + 0.5 * band);
        float lineB = smoothstep(0.955, 0.990, 0.5 - 0.5 * band);
        // fine secondary striations inside the streak body (detail octave,
        // crisp value-threshold ridging)
        float striae = smoothstep(0.72, 0.90, 0.5 + 0.5 * sin(ph * 6.0 + fbm(q * 5.0) * 3.0));
        vec3 streamCol = vec3(0.14, 0.72, 0.85) * streakA
                       + vec3(0.36, 0.42, 1.00) * streakB * 1.3;
        streamCol += vec3(0.30, 0.92, 1.00) * lineA * 1.10
                   + vec3(0.55, 0.60, 1.00) * lineB * 1.10;
        streamCol += vec3(0.20, 0.62, 0.90) * striae * (streakA + streakB) * 0.80;
        col += streamCol * streamerAmount * (0.34 + 0.42 * drive + 0.80 * rA * melP);
    }

    // ═══ LAYER 4 — glow: self-bloom + stemAir shimmer haze ═══
    if (glowAmount > 0.001) {
        float lum = dot(col - base, vec3(0.30, 0.50, 0.20));
        col += col * 0.30 * glowAmount * smoothstep(0.05, 1.1, lum);   // soft self-glow
        float haze = fbm(p * 6.5 + vec2(tAir * 0.7, -tAir * 0.4));
        col += vec3(0.82, 0.92, 1.00) * haze * haze * glowAmount
             * (0.035 + 0.30 * rA * airP);                              // highs = sparkle
    }

    // ── finish: vignette, hue, boost, filmic knee ──
    col *= 1.0 - 0.32 * smoothstep(0.55, 1.25, length(p));
    col = hueRot(col, hueShift * 6.28318);
    float l = dot(col, vec3(0.299, 0.587, 0.114));
    col = max(mix(vec3(l), col, clamp(colorBoost, 0.0, 2.0)), 0.0);
    col = 1.0 - exp(-col * 1.5);

    // whisper of STATIC micro-grain — fine edge energy at zero motion cost
    // (per-frame grain flicker would flood the silence noise floor)
    float gr = hash21(gl_FragCoord.xy) - 0.5;
    col += gr * 0.046;

    gl_FragColor = vec4(col, 1.0);
}
