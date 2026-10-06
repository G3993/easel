/*{
  "CATEGORIES": [
    "Generator",
    "Fluid",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Drops of colored ink dispersing through clear water — curl-noise advected blooms with branching tendrils, slow rotational vortex, and pigment density decreasing as it spreads. Audio bass triggers new ink drops at hashed positions; treble drives fine turbulence detail. The Sunday-afternoon kitchen experiment as music visualizer.",
  "INPUTS": [
    {
      "NAME": "fadeRate",
      "LABEL": "Fade Rate",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 1.5,
      "DEFAULT": 0.35
    },
    {
      "NAME": "dropCount",
      "LABEL": "Drop Count",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 12,
      "DEFAULT": 7,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dropSize",
      "LABEL": "Drop Size",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 0.8,
      "DEFAULT": 0.27,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dropSpread",
      "LABEL": "Spread Rate",
      "TYPE": "float",
      "MIN": 0.02,
      "MAX": 0.4,
      "DEFAULT": 0.14,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dropDispersion",
      "LABEL": "Dispersion",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 1.8,
      "DEFAULT": 0.7,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "curlScale",
      "LABEL": "Curl Scale",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 8,
      "DEFAULT": 3.2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "curlStrength",
      "LABEL": "Curl Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.8,
      "DEFAULT": 0.3,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "vortexSpeed",
      "LABEL": "Vortex Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.12,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "tendrilRate",
      "LABEL": "Tendril Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.18,
      "GROUP": "Motion"
    },
    {
      "NAME": "patternDrift",
      "LABEL": "Pattern Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0.07,
      "GROUP": "Motion"
    },
    {
      "NAME": "sinkRate",
      "LABEL": "Sink Rate",
      "TYPE": "float",
      "MIN": -0.06,
      "MAX": 0.06,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "dropOrbit",
      "LABEL": "Drop Orbit",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "waterTop",
      "LABEL": "Water Top",
      "TYPE": "color",
      "DEFAULT": [
        0.92,
        0.96,
        0.98,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "waterBottom",
      "LABEL": "Water Bottom",
      "TYPE": "color",
      "DEFAULT": [
        0.78,
        0.86,
        0.92,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "inkA",
      "LABEL": "Ink A",
      "TYPE": "color",
      "DEFAULT": [
        0.05,
        0.18,
        0.55,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "inkB",
      "LABEL": "Ink B",
      "TYPE": "color",
      "DEFAULT": [
        0.85,
        0.1,
        0.35,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "inkC",
      "LABEL": "Ink C",
      "TYPE": "color",
      "DEFAULT": [
        0.95,
        0.65,
        0.1,
        1
      ],
      "GROUP": "Color"
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
vec2  hash22(vec2 p)  {
    return fract(sin(vec2(dot(p, vec2(127.1, 311.7)),
                          dot(p, vec2(269.5,  183.3)))) * 43758.5453);
}

float vnoise(vec2 p) {
    vec2 ip = floor(p), fp = fract(p);
    fp = fp * fp * (3.0 - 2.0 * fp);
    float a = hash21(ip);
    float b = hash21(ip + vec2(1.0, 0.0));
    float c = hash21(ip + vec2(0.0, 1.0));
    float d = hash21(ip + vec2(1.0, 1.0));
    return mix(mix(a, b, fp.x), mix(c, d, fp.x), fp.y);
}

float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    mat2 r = mat2(0.8, -0.6, 0.6, 0.8);
    for (int i = 0; i < 5; i++) {
        v += a * vnoise(p);
        p = r * p * 2.03;
        a *= 0.5;
    }
    return v;
}

// Curl of an fbm scalar potential — divergence-free flow used to advect ink.
vec2 curlNoise(vec2 p) {
    float e = 0.08;
    float n1 = fbm(p + vec2(0.0,  e));
    float n2 = fbm(p - vec2(0.0,  e));
    float n3 = fbm(p + vec2(e,   0.0));
    float n4 = fbm(p - vec2(e,   0.0));
    return vec2((n1 - n2), -(n3 - n4)) / (2.0 * e);
}

vec3 inkPalette(float h) {
    h = fract(h);
    vec3 a = inkA.rgb, b = inkB.rgb, c = inkC.rgb;
    vec3 col;
    if (h < 0.3333) col = mix(a, b, h * 3.0);
    else if (h < 0.6666) col = mix(b, c, (h - 0.3333) * 3.0);
    else col = mix(c, a, (h - 0.6666) * 3.0);
    // Saturation boost — push the inks brighter and more vivid.
    float L = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(L), col, 1.35);
    return clamp(col, 0.0, 1.0);
}

float dispersionFor(float life) {
    return dropDispersion * (0.6 + 0.4 * sin(life * 0.7));
}

void main() {
    vec2 res = RENDERSIZE.xy;
    vec2 uv  = gl_FragCoord.xy / res;
    float aspect = res.x / max(res.y, 1.0);
    vec2 p = vec2((uv.x - 0.5) * aspect, uv.y - 0.5);

    float t = TIME;

    // Audio inputs — Easel binds these as ISF uniforms; default to zero otherwise.
    float bass   = clamp(audioBass * audioReact, 0.0, 2.0);
    float treble = clamp(audioHigh * audioReact, 0.0, 2.0);

    // Slow rotational vortex of the whole bath.
    float ang = vortexSpeed * t * 0.25;
    float ca = cos(ang), sa = sin(ang);
    vec2 pr = mat2(ca, -sa, sa, ca) * p;

    // Water background: vertical gradient + faint shimmer.
    vec3 col = mix(waterBottom.rgb, waterTop.rgb, smoothstep(0.0, 1.0, uv.y));
    float shimmer = fbm(pr * 1.2 + vec2(0.0, t * 0.05));
    col += (shimmer - 0.5) * 0.025;

    // Subtractive ink mixing: start at full transmission, absorb per drop.
    vec3 transmission = vec3(1.0);

    int N = int(clamp(dropCount, 1.0, 12.0));

    // Beat-driven drop swell: audioBeatPulse already decays smoothly.
    // (The old sawtooth exp(-fract(t*2.0)) envelope snapped radii every
    // 0.5 s, and bass-modulated periods re-hashed drop positions — chop.)
    float explodeEnv = pow(max(audioBeatPulse, 0.8 * audioPunch), 1.3);
    // Accumulator for bloom halo — sums pigment so we can add a glow afterward.
    vec3 bloomAccum = vec3(0.0);

    for (int i = 0; i < 12; i++) {
        if (i >= N) break;
        float fi = float(i);

        // Each drop relaunches every `period` seconds (TIME-bucket).
        // Period must stay CONSTANT: bass-shortened periods moved bucket
        // boundaries, instantly re-hashing drop positions (teleports).
        float period = mix(9.0, 4.0, hash11(fi * 1.37));
        float bucket = floor(t / period + hash11(fi * 7.13));
        float seed   = fi * 17.3 + bucket * 53.1;

        // Hashed launch position in aspect-corrected centred space.
        vec2 h = hash22(vec2(seed, seed + 3.7));
        vec2 dropPos = vec2((h.x - 0.5) * aspect * 0.85,
                            (h.y - 0.5) * 0.85);

        // Drop hue and life timer.
        float hue   = hash11(seed + 1.9);
        float birth = bucket * period - hash11(fi * 4.2) * period * 0.5;
        float life  = max(0.0, t - birth);

        // bespoke MOTION: pigment slowly sinks (or rises) through the bath,
        // and can orbit its launch point like a stirred drop. Both 0 = still.
        dropPos.y -= sinkRate * life;
        float orbA = life * 1.2 + seed;
        dropPos += dropOrbit * 0.15 * vec2(cos(orbA), sin(orbA));

        // Diffusion radius grows with sqrt(life); pigment density decays.
        // Bass explosion: massive size jump for ~0.5s after impact.
        float baseR    = dropSize * mix(0.55, 1.25, hash11(seed + 5.5));
        // Continuous radius breathing (smoothed bass) + decaying beat swell.
        float bassKick = (1.0 + 0.35 * bass)
                       * (1.0 + explodeEnv * 0.5 * smoothstep(0.0, 0.4, life)
                              * exp(-life * 1.5));
        float radius   = (baseR + sqrt(life) * dropSpread) * bassKick;
        float density  = 1.0 / (1.0 + life * dispersionFor(life));
        density       *= smoothstep(0.0, 0.6, life) * exp(-life * fadeRate * 0.18);

        if (density < 0.005) continue;

        // Curl-noise warp of the interior pattern; treble adds fine detail.
        // Stronger trail effect: curl warp magnitude × 1.5 for branching tendrils.
        vec2 q = pr - dropPos;
        float cs = curlScale * mix(1.0, 1.6, treble * 0.5);
        // tendrilRate = how fast the curl field itself evolves (tendrils writhe)
        vec2 flow = curlNoise(q * cs + vec2(seed, t * tendrilRate));
        // Add a second curl octave for branching tendril detail.
        vec2 flow2 = curlNoise(q * cs * 2.3 + vec2(t * tendrilRate * 1.5, seed * 0.5));
        vec2 warp = q + (flow + flow2 * 0.45) * curlStrength * 1.5
                       * (1.0 + treble * 0.6) * (0.4 + radius);

        // Branching tendrils: layered fbm in warped space.
        // patternDrift = how fast the interior pigment texture crawls
        float pattern = fbm(warp * 4.2 + vec2(seed * 0.31, t * patternDrift));
        pattern += 0.5 * fbm(warp * 9.0 - vec2(t * patternDrift * (0.11 / 0.07), seed));
        pattern  = smoothstep(0.35, 1.05, pattern);

        // Soft radial mask with curl-driven boundary wobble.
        float dist = length(q);
        float edgeWobble = 0.08 * (fbm(q * 6.0 + seed) - 0.5);
        float mask = 1.0 - smoothstep(radius * 0.25,
                                      radius * (1.0 + edgeWobble * 4.0),
                                      dist);

        float pigment = mask * pattern * density;
        // Concentrated core when freshly dropped.
        float r2 = max(radius * radius * 0.15, 1e-4);
        float core = exp(-dist * dist / r2);
        pigment += core * density * 0.55;

        // Per-channel absorption: ink colour subtracts its complement.
        vec3 inkCol = inkPalette(hue);
        vec3 absorb = (1.0 - inkCol) * pigment;
        transmission *= exp(-absorb * 1.8);
        // Accumulate pigment for bloom halo.
        bloomAccum += inkCol * pigment;
    }

    col *= transmission;

    // Bloom halo around bright pigment — soft additive glow so colours feel rich.
    col += bloomAccum * 0.18 * (1.0 + bass * 0.4);

    // Subtle bass-driven brightness lift on the water.
    col += vec3(0.04, 0.05, 0.06) * bass * 0.4;

    // Vignette to keep the eye on the experiment.
    float vig = smoothstep(1.15, 0.35, length(p));
    col *= mix(0.85, 1.0, vig);

    // Whole-frame follower as DARKEN-dips: the water sits at 0.85-0.96
    // luma, so up-gains clip almost immediately and the response saturates;
    // dips never clip and read on every pixel. Beats deepen the dip (ink
    // "absorbs" on the hit). All terms exactly 1.0 in silence.
    col *= 1.0 - 0.15 * audioBass - 0.20 * audioMid;
    col *= 1.0 - 0.04 * pow(max(audioBeatPulse, 0.8 * audioPunch), 1.3);

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
    gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
}
