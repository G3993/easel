/*{
  "CATEGORIES": [
    "Generator",
    "Atmospheric",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Perseid-style radiant meteor shower — bright streaks of varying intensity radiating outward from a single radiant point in the sky, with persistent twinkling stars, milky-way band, and audio-bass triggering bright fireball bolides. Each meteor has a hot leading edge and a glowing trail that fades, plus occasional rare sub-branching",
  "INPUTS": [
    {
      "NAME": "milkyWayBrightness",
      "LABEL": "Milky Way",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.4
    },
    {
      "NAME": "boliderProb",
      "LABEL": "Bolide Chance",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.18
    },
    {
      "NAME": "meteorCount",
      "LABEL": "Meteors",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 15,
      "DEFAULT": 12,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "trailLength",
      "LABEL": "Trail Length",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 0.8,
      "DEFAULT": 0.42,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "starDensity",
      "LABEL": "Star Density",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "meteorSpeed",
      "LABEL": "Meteor Speed",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "skyTop",
      "LABEL": "Sky Top",
      "TYPE": "color",
      "DEFAULT": [
        0.01,
        0.02,
        0.06,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "skyHorizon",
      "LABEL": "Sky Horizon",
      "TYPE": "color",
      "DEFAULT": [
        0.04,
        0.05,
        0.12,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "nebulaTint",
      "LABEL": "Nebula Tint",
      "TYPE": "color",
      "DEFAULT": [
        0.18,
        0.1,
        0.3,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "LABEL": "Hue Shift",
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "LABEL": "Color Boost",
      "GROUP": "Color"
    },
    {
      "NAME": "radiantX",
      "LABEL": "Radiant X",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.78,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "radiantY",
      "LABEL": "Radiant Y",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.82,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

float hash11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float hash21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

vec2 hash22(float n) {
    return fract(sin(vec2(n * 12.9898, n * 78.233)) * vec2(43758.5453, 22578.1459));
}

// Capsule SDF from a to b at point p.
float sdCapsule(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a;
    vec2 ba = b - a;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-6), 0.0, 1.0);
    return length(pa - ba * h);
}

// Returns t in [0,1] along ba projection (clamped).
float projT(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a;
    vec2 ba = b - a;
    return clamp(dot(pa, ba) / max(dot(ba, ba), 1e-6), 0.0, 1.0);
}

float starLayer(vec2 uv, float cell, float threshold, float twinkleSpeed, float baseTime) {
    vec2 g = uv * cell;
    vec2 ip = floor(g);
    vec2 fp = fract(g) - 0.5;
    float h = hash21(ip);
    if (h < threshold) return 0.0;
    vec2 jitter = vec2(hash21(ip + 17.0), hash21(ip + 91.0)) - 0.5;
    float d = length(fp - jitter * 0.6);
    float size = 0.04 + 0.06 * hash21(ip + 3.7);
    float core = smoothstep(size, 0.0, d);
    float tw = 0.55 + 0.45 * sin(baseTime * (0.5 + h * twinkleSpeed) + h * 30.0);
    return core * tw;
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    vec2 auv = vec2(uv.x * aspect, uv.y);

    // Audio bands — LINEAR (r2 ambient fix): the envelopes arrive pre-smoothed,
    // and the round-1 pow/knee crushed ambient's 0.1-0.8 swells into tiny
    // variance. Continuous envelopes, never gated.
    float bass   = smoothstep(0.02, 0.97, audioBass) * audioReact;
    float midK   = smoothstep(0.02, 0.95, audioMid)  * audioReact;
    float treble = smoothstep(0.02, 0.95, audioHigh) * audioReact;

    // ---- TRANSPARENT BACKGROUND: the sky plate (gradient, nebula, milky-way
    // wash) is no longer rendered — alpha 0 there. Only the shower shows:
    // meteors/trails opaque with soft alpha edges, stars as tiny alpha points.
    // Output is premultiplied (rgb * alpha) so host blending is clean. ----
    vec3 col = vec3(0.0);
    float cover = 0.0;

    // Milky-way band survives only as a star-brightness stripe (no wash)
    float band = 0.0;
    if (milkyWayBrightness > 0.0) {
        vec2 c = uv - 0.5;
        float ang = 0.55;
        float ca = cos(ang), sa = sin(ang);
        vec2 r = vec2(ca * c.x - sa * c.y, sa * c.x + ca * c.y);
        float clumps = 0.5 + 0.5 * sin(r.x * 18.0) * sin(r.x * 7.3 + 1.4);
        band = exp(-pow(r.y * 5.5, 2.0)) * clumps * milkyWayBrightness;
    }

    // ---- Star fields (two layers) ----
    float twkSpeed = 4.0 + treble * 8.0;
    float s1 = starLayer(auv, 70.0,  0.985, twkSpeed, TIME);
    float s2 = starLayer(auv, 130.0, 0.992, twkSpeed * 1.4, TIME);
    float starBoost = 1.0 + treble * 1.4 + midK * 0.7 + bass * 0.5 + band * 1.6;
    col += vec3(1.0, 0.96, 0.88) * s1 * starDensity * 1.4 * starBoost;
    col += vec3(0.85, 0.90, 1.00) * s2 * starDensity * 0.9 * starBoost;
    cover += clamp((s1 * 1.1 + s2 * 0.7) * starDensity * (1.0 + band), 0.0, 1.0) * 0.85;

    // ---- Meteors radiating from radiant ----
    vec2 radiant = vec2(radiantX * aspect, radiantY);

    int N = int(clamp(meteorCount, 1.0, 15.0));
    vec3 meteorAccum = vec3(0.0);
    float meteorA = 0.0;

    for (int i = 0; i < 15; i++) {
        if (i >= N) break;
        float fi = float(i);

        // Per-meteor cycle period (varied so they don't sync).
        float period = 2.4 + hash11(fi * 1.13) * 4.5;
        float phase  = hash11(fi * 7.31) * period;
        float localT = mod(TIME + phase, period);
        float bucket = floor((TIME + phase) / period);

        // Per-launch random seed (changes every cycle so direction re-rolls).
        float seed = fi * 13.0 + bucket * 91.7;

        // Hashed direction biased outward from radiant — cone-ish away from radiant center.
        float ang = hash11(seed) * 6.2831853;
        // Bias direction so meteors mostly go down/left if radiant is upper-right:
        vec2 baseOut = normalize(vec2(0.5, 0.5) - vec2(radiantX, radiantY) + 1e-4);
        vec2 randDir = vec2(cos(ang), sin(ang));
        vec2 dir = normalize(mix(randDir, baseOut, 0.45));

        // Speed varies; some sluggish/long, some fast/short.
        float spd = (0.25 + hash11(seed + 2.3) * 1.4) * meteorSpeed;
        float trailLen = trailLength * (0.5 + hash11(seed + 5.1) * 1.4) / max(spd, 0.2);

        // Bolide chance: rarer big bright ones, bass-modulated.
        float bolideRoll = hash11(seed + 11.7);
        float isBolide = step(1.0 - clamp(boliderProb + bass * 0.35, 0.0, 0.95), bolideRoll);
        float intensity = mix(0.6 + hash11(seed + 3.1) * 0.6, 2.4, isBolide);

        // Active window: meteor is "alive" for first chunk of its cycle.
        float life = 0.9 + hash11(seed + 9.0) * 0.6;
        if (localT > life) continue;

        // Head & tail positions in aspect-corrected space.
        float headDist = spd * localT;
        vec2 head = radiant + dir * headDist;
        vec2 tail = head - dir * trailLen;

        // Capsule distance from fragment.
        float d = sdCapsule(auv, tail, head);

        // Tapered glow along trail (t=0 at tail, t=1 at head).
        float tProj = projT(auv, tail, head);
        float taper = pow(tProj, 1.6);          // brighter near head
        float coreW = mix(0.0020, 0.0050, isBolide);
        float glowW = mix(0.028, 0.058, isBolide) * (1.0 + 0.35 * bass);
        float core = smoothstep(coreW, 0.0, d) * (0.5 + taper * 1.6);
        float glow = smoothstep(glowW, 0.0, d) * (0.20 + taper * 0.75);

        // Fade in/out across life.
        float fade = smoothstep(0.0, 0.08, localT) * smoothstep(life, life - 0.25, localT);

        // Color: hot white head, warm orange trail, blue-white for bolides.
        vec3 hotCol  = mix(vec3(1.0, 0.95, 0.85), vec3(0.9, 0.95, 1.15), isBolide);
        vec3 trailCol= mix(vec3(1.0, 0.55, 0.25), vec3(0.7, 0.85, 1.0), isBolide);
        vec3 mc = mix(trailCol, hotCol, taper);

        meteorAccum += mc * (core * 1.6 + glow * 0.8) * intensity * fade;
        // coverage: opaque core, soft alpha falling off through the glow
        meteorA += (core * 1.3 + glow * 0.55) * fade * min(intensity, 1.5);

        // Rare sub-branch: a faint shorter capsule diverging from mid-trail.
        if (hash11(seed + 17.0) > 0.86) {
            float branchAng = (hash11(seed + 23.1) - 0.5) * 0.7;
            float bca = cos(branchAng), bsa = sin(branchAng);
            vec2 bdir = vec2(bca * dir.x - bsa * dir.y, bsa * dir.x + bca * dir.y);
            vec2 bStart = mix(tail, head, 0.4);
            vec2 bEnd = bStart + bdir * trailLen * 0.45;
            float bd = sdCapsule(auv, bStart, bEnd);
            float bglow = smoothstep(0.018, 0.0, bd);
            meteorAccum += trailCol * bglow * 0.35 * fade;
            meteorA += bglow * 0.30 * fade;
        }
    }

    // Decaying beat flash on the streaks (audioBeatPulse decays 300ms+) —
    // restores per-kick variation on EDM where smoothed bass rides high.
    col += meteorAccum * (1.0 + bass * 0.9 + 0.6 * audioBeatPulse * audioReact);
    cover += meteorA;

    // whole-shower linear follower — every visible pixel breathes with music.
    col *= 1.0 + 0.35 * bass + 0.22 * midK;

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        col = clamp(hM * col, 0.0, 1.0);
    }

    // Premultiplied output: background alpha 0, shower carries the coverage.
    float alphaOut = clamp(cover, 0.0, 1.0);
    gl_FragColor = vec4(col * alphaOut, alphaOut);
}
