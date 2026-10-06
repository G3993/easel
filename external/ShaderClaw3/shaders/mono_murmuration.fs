/*{
  "CATEGORIES": ["Generator", "Audio Reactive", "Minimal"],
  "DESCRIPTION": "Murmuration — a starling flock in monochrome, run like a particle system: every bird is a crisp anti-aliased chevron glyph that flies along its own analytic path (shared wandering swarm center + per-bird orbit), pointing where it actually moves, wings beating. Three depth shells, motion trails, cohesion/scatter/speed/density all on sliders down to dust-mote scale. Bass tightens and swells the flock, beats bank the whole formation, highs flicker sparse wingbeats. Black sky, white birds, nothing else.",
  "INPUTS": [
    {"NAME": "flockSize",  "LABEL": "Flock Size",    "TYPE": "float", "MIN": 0.2,  "MAX": 1.2, "DEFAULT": 0.55},
    {"NAME": "birdCount",  "LABEL": "Bird Density",  "TYPE": "float", "MIN": 0.3,  "MAX": 1.6, "DEFAULT": 0.85},
    {"NAME": "windSpeed",  "LABEL": "Flight Speed",  "TYPE": "float", "MIN": 0.1,  "MAX": 2.0, "DEFAULT": 0.7},
    {"NAME": "birdScale",  "LABEL": "Bird Scale",    "TYPE": "float", "MIN": 0.05, "MAX": 2.0, "DEFAULT": 0.8},
    {"NAME": "cohesion",   "LABEL": "Cohesion",      "TYPE": "float", "MIN": 0.3,  "MAX": 2.5, "DEFAULT": 1.0},
    {"NAME": "scatter",    "LABEL": "Scatter",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.40},
    {"NAME": "flapSpeed",  "LABEL": "Wingbeat",      "TYPE": "float", "MIN": 0.0,  "MAX": 2.0, "DEFAULT": 1.0},
    {"NAME": "ghosting",   "LABEL": "Wing Trails",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.45},
    {"NAME": "bankAmt",    "LABEL": "Beat Bank",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.6},
    {"NAME": "windDetail", "LABEL": "Wind Detail",   "TYPE": "float", "MIN": 0.5,  "MAX": 3.0, "DEFAULT": 1.3},
    {"NAME": "skyGlow",    "LABEL": "Sky Glow",      "TYPE": "float", "MIN": 0.0,  "MAX": 0.3, "DEFAULT": 0.07},
    {"NAME": "invert",     "LABEL": "Invert",        "TYPE": "float", "MIN": 0.0,  "MAX": 1.0, "DEFAULT": 0.0}
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// MURMURATION rework — particle-system flocking, crisp glyphs.
//   • Bird position is ANALYTIC: swarm-center Lissajous (whole flock
//     travels) + per-bird elliptical orbit; velocity is the analytic
//     derivative, so every bird POINTS where it MOVES (boid feel without
//     state buffers).
//   • Glyph = chevron of two wing capsules + body, drawn with pixel-true
//     AA (smoothstep width ≈1.5px scaled by resolution). Sub-pixel birds
//     clamp stroke width to ~0.7px and dim by coverage — tiny particle
//     flocks stay crisp, never pixelated shimmer.
//   • Audio: bass adds cohesion + swell, beats bank heading (eased,
//     additive), highs flicker sparse wing widths, level lifts the sky.
// ─────────────────────────────────────────────────────────────────────────

float hash21(vec2 p) {
    p = fract(p * vec2(234.34, 435.345));
    p += dot(p, p + 34.23);
    return fract(p.x * p.y);
}

vec2 hash22(vec2 p) {
    float n = hash21(p);
    return vec2(n, hash21(p + n + 17.31));
}

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1.0, 0.0)), u.x),
               mix(hash21(i + vec2(0.0, 1.0)), hash21(i + vec2(1.0, 1.0)), u.x), u.y);
}

float knee(float x, float lo, float hi) { return clamp(smoothstep(lo, hi, x), 0.0, 1.0); }

// segment distance (scene units)
float sdSeg(vec2 pt, vec2 a, vec2 b) {
    vec2 pa = pt - a, ba = b - a;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-8), 0.0, 1.0);
    return length(pa - ba * h);
}

// one bird glyph: chevron wings + short body, oriented along hd.
// Returns coverage with pixel-true AA; tiny birds dim instead of shimmer.
float birdGlyph(vec2 d, vec2 hd, float len, float wid, float flap, float aa) {
    vec2 sd2 = vec2(-hd.y, hd.x);
    // wings sweep back ~110° off the heading; flap rocks them up/down
    float wa = 1.95 - 0.55 * flap;
    vec2 w1 = hd * cos(wa) + sd2 * sin(wa);
    vec2 w2 = hd * cos(wa) - sd2 * sin(wa);
    float dist = min(sdSeg(d, vec2(0.0), w1 * len),
                     sdSeg(d, vec2(0.0), w2 * len));
    dist = min(dist, sdSeg(d, -hd * len * 0.10, hd * len * 0.42));
    // crispness guard: stroke never thinner than ~0.7px; energy conserved
    float wEff = max(wid, aa * 0.45);
    float dimC = clamp(wid / wEff, 0.25, 1.0);
    return smoothstep(aa, -aa, dist - wEff) * dimC;
}

void main() {
    vec2 R = RENDERSIZE.xy;
    vec2 p = (gl_FragCoord.xy - 0.5 * R) / min(R.x, R.y);
    float aa = 1.5 / min(R.x, R.y);          // pixel-true AA in scene units

    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP  = pow(knee(audioMid,  0.08, 0.88), 1.3);
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float drive = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9);
    float levelP = knee(audioLevel, 0.03, 0.8);
    float beat  = clamp(audioBeatPulse, 0.0, 1.0);

    // Bounded phase offset, never TIME*drive (whole-flock teleport hazard).
    float mt = TIME * windSpeed * 0.8 + drive * 2.5;

    // Swarm center wanders a lissajous sky path; velocity is its analytic
    // derivative so the flock heads where it travels.
    vec2 center = vec2(0.42 * sin(mt * 0.23) + 0.18 * sin(mt * 0.071 + 2.1),
                       0.28 * cos(mt * 0.181) + 0.14 * cos(mt * 0.053));
    vec2 centerV = vec2(0.42 * 0.23 * cos(mt * 0.23) + 0.18 * 0.071 * cos(mt * 0.071 + 2.1),
                       -0.28 * 0.181 * sin(mt * 0.181) - 0.14 * 0.053 * sin(mt * 0.053));

    // Beat banks the formation: an eased momentary rotation of every heading.
    float bank = 0.55 * bankAmt * beat * beat
               * (hash21(vec2(floor(TIME * 0.5), 7.0)) > 0.5 ? 1.0 : -1.0);

    // Flock radius: bass gathers the birds (murmurations contract on impact);
    // sheer loudness swells the cloud so quiet passages visibly thin it out.
    float spread = flockSize * (0.68 - 0.26 * bassP + 0.08 * midP + 0.45 * levelP);
    float tight = (1.2 * cohesion + 1.6 * bassP);

    float v = 0.0;

    // Three interleaved grids = three depth shells of birds.
    // Grid space is anchored to the swarm center: the whole cloud of
    // particles physically crosses the sky with it.
    for (int layer = 0; layer < 3; layer++) {
        float fl   = float(layer);
        float cell = (0.052 + 0.026 * fl) / max(birdCount, 0.05);
        float depth = 1.0 - 0.26 * fl;             // far shells dimmer, smaller

        vec2 pw = p - center;                       // flock-frame coords
        vec2 gid = floor(pw / cell);

        // 3x3 neighborhood so glyphs cross cell borders cleanly.
        for (int oy = -1; oy <= 1; oy++)
        for (int ox = -1; ox <= 1; ox++) {
            vec2 id = gid + vec2(float(ox), float(oy));
            vec2 rnd = hash22(id + fl * 91.7);
            vec2 rnd2 = hash22(id * 1.37 + 19.3 + fl * 7.7);

            // Not every cell holds a bird — density falls off from the center.
            vec2 anchor = (id + rnd) * cell;
            float dc = length(anchor) / max(spread, 1e-3);
            float present = step(rnd.x, exp(-dc * dc * tight) * 0.92 + 0.02);
            if (present < 0.5) continue;

            // Per-bird orbit (the "particle"): elliptical wander inside the
            // cell neighborhood, analytic velocity, per-bird rates/phases.
            float phA = rnd.y * 6.2831;
            float phB = rnd2.x * 6.2831;
            float ra = mt * (0.8 + 0.65 * rnd2.y) + phA;
            float rb = mt * (0.55 + 0.45 * rnd.x) + phB;
            float amp = cell * (0.55 + 1.1 * scatter);
            vec2 orb = amp * vec2(sin(ra), 0.72 * cos(rb));
            vec2 orbV = amp * vec2(cos(ra) * (0.8 + 0.65 * rnd2.y),
                                   -0.72 * sin(rb) * (0.55 + 0.45 * rnd.x));

            vec2 bpos = anchor + orb;
            vec2 d = p - (center + bpos);

            // Heading = actual velocity (swarm drift + own orbit + wind shear)
            float wind = (vnoise(anchor * 2.3 * windDetail
                                 + vec2(mt * 0.9, mt * 0.63)) - 0.5) * 2.4;
            vec2 vel = centerV * 2.2 + orbV * 0.9;
            float hAng = atan(vel.y, vel.x) + 0.45 * wind + bank
                       + 0.20 * sin(mt * 1.7 + phA);
            vec2 hd = vec2(cos(hAng), sin(hAng));

            float len = cell * 0.42 * birdScale * depth;
            float wid = cell * 0.055 * birdScale * (1.4 - 0.5 * depth + 0.5);

            // Wingbeat: every bird flaps; highs flicker a sparse subset harder.
            float flap = sin(mt * (7.0 + 3.0 * rnd2.x) * flapSpeed + phB)
                       * (0.55 + 0.45 * levelP);
            float flick = 1.0 + highP * 0.8 * step(0.80, rnd.y)
                        * sin(TIME * 14.0 + phA * 3.0);
            wid *= clamp(flick, 0.4, 1.9);

            float b = birdGlyph(d, hd, len, wid, flap, aa) * depth;

            // Motion trail — two fading ghosts along the flight line.
            vec2 vn = vel / max(length(vel), 1e-4);
            b = max(b, ghosting * 0.40 * birdGlyph(d + vn * len * 1.7, hd, len * 0.85,
                                                   wid * 0.8, flap * 0.6, aa) * depth);
            b = max(b, ghosting * 0.18 * birdGlyph(d + vn * len * 3.2, hd, len * 0.70,
                                                   wid * 0.6, flap * 0.3, aa) * depth);

            v = max(v, b);
        }
    }

    // Quiet sky: a barely-there vertical glow so silence isn't a dead frame;
    // it also swells with loudness so the whole frame tracks the music.
    float sky = skyGlow * (1.0 - length(p) * 0.7) * (0.40 + 0.30 * drive + 1.2 * levelP);
    v = clamp(v * (0.56 + 0.14 * drive + 0.55 * levelP + 0.10 * beat * beat) + sky, 0.0, 1.0);

    v = mix(v, 1.0 - v, clamp(invert, 0.0, 1.0));
    gl_FragColor = vec4(vec3(v), 1.0);
}
