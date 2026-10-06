/*{
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Soundcurrent — EaselAudio flagship. An analytic curl-noise fluid (no feedback buffer): dye advected by an audioBassTime-driven flow, viscosity breathing with band presence, stemAir raking satin shimmer across the surface, and kicks (audioBassHit) injecting vortex rings that push the dye as they expand and fade. Silky in silence, a living current with music.",
  "CREDIT": "ShaderClaw3 EaselAudio flagship",
  "INPUTS": [
    {
      "NAME": "shimmerAmount",
      "LABEL": "Air Shimmer",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "flowScale",
      "LABEL": "Current Scale",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "flowAmount",
      "LABEL": "Flow Field",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "ringAmount",
      "LABEL": "Vortex Rings",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "dyeRichness",
      "LABEL": "Dye Palette",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
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
//  SOUNDCURRENT (id 1202) — EaselAudio engine flagship, fluid edition.
//
//  Layers (each independently controllable):
//    • FLOW FIELD — iterated curl-noise advection. The advection clock is
//                   audioBassTime (integrates bass: silky drift in silence,
//                   surging current when the low end plays). Viscosity
//                   breathes with audioBassPresence — sparse mixes read
//                   thin and glassy, full mixes get thick and rolling.
//    • DYE        — two-scale fbm dye through a cosine palette; hue drifts
//                   with audioBrightness (color on spectral character) and
//                   a soft bar-phase tide re-seasons the palette per bar.
//    • RINGS      — kicks (audioBassHit) inject vortex rings: born tight
//                   and bright, they expand and fade as the hit envelope
//                   decays, physically displacing the dye they cross.
//    • SHIMMER    — stemAir rakes fine satin glints across the dye's
//                   luminance ridges (highs = fine peripheral sparkle).
//
//  Playbook compliance: Time clocks (never raw audio→position), soft
//  knees + idle floor, events with finite life, beautiful in silence.
// ════════════════════════════════════════════════════════════════════════

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

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
        p = p * 2.07 + vec2(11.3, 7.9);
        a *= 0.5;
    }
    return v;
}

// divergence-free curl of the fbm potential
vec2 curl2(vec2 p) {
    float e = 0.13;
    float n1 = fbm(p + vec2(0.0, e));
    float n2 = fbm(p - vec2(0.0, e));
    float n3 = fbm(p + vec2(e, 0.0));
    float n4 = fbm(p - vec2(e, 0.0));
    return vec2(n1 - n2, n4 - n3) / (2.0 * e);
}

// iq cosine palette — deep-water body with warm accents
vec3 pal(float t) {
    return vec3(0.50, 0.48, 0.52)
         + vec3(0.46, 0.44, 0.48) * cos(6.28318 * (vec3(0.90, 1.00, 1.10) * t
         + vec3(0.05, 0.32, 0.62)));
}

vec3 hueRot(vec3 c, float a) {
    const vec3 k = vec3(0.57735);
    float ca = cos(a), sa = sin(a);
    return c * ca + cross(k, c) * sa + k * dot(k, c) * (1.0 - ca);
}

void main() {
    vec2 p = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y;
    float px = 1.0 / RENDERSIZE.y;   // one pixel in p-units — edges in PIXEL widths

    // ── audio conditioning ──
    float react = audioReactivity;
    float rA    = min(react, 1.5);
    float bassP = pow(knee(stemBass, 0.06, 0.85), 1.4);
    float airP  = pow(knee(stemAir,  0.08, 0.90), 1.2);
    float drive = 0.25 + 0.75 * knee(audioEnergy, 0.05, 0.9);
    // viscosity breathes with presence (slow macro envelope — section-level)
    float visc  = 0.55 + 0.45 * clamp(audioBassPresence * 1.25, 0.0, 1.0);

    // ── Time clocks: the current only surges when bass actually plays ──
    float tFlow = TIME * 0.20 + audioBassTime * 0.62 * rA;
    float tAir  = TIME * 0.25 + audioHighTime * 0.55 * rA;
    float barTide = sin(audioPhase4 * 6.28318) * 0.20 * rA;

    // ═══ LAYER 3 first — vortex rings (they displace the dye below) ═══
    // Kick shockwave: age runs 0→1 as the AD hit envelope decays; the ring
    // is born tight and bright, expands outward, brightness dies with it.
    float hitB = clamp(audioBassHit, 0.0, 1.0);
    float hitM = clamp(audioMidHit,  0.0, 1.0);
    float ageB = 1.0 - hitB;
    vec2 rc = 0.34 * vec2(cos(tFlow * 0.55), sin(tFlow * 0.80));   // slow orbit
    vec2 rel = p - rc;
    float rl = length(rel);
    float rrB = mix(0.10, 0.95, smoothstep(0.0, 1.0, ageB));
    float bandB = exp(-pow((rl - rrB) * 11.0, 2.0));
    // soft-knee the birth frame: the hit uniform attacks instantly, so ramp
    // the ring in over the first ~20% of the envelope to avoid a strobe pop
    float easeB = smoothstep(1.0, 0.72, hitB);
    float ringB = bandB * pow(hitB, 1.35) * (0.35 + 0.65 * easeB);
    // counter-ring from the snare side, smaller and cooler
    vec2 rel2 = p + rc;                                            // mirrored center
    float rl2 = length(rel2);
    float rrM = mix(0.08, 0.55, smoothstep(0.0, 1.0, 1.0 - hitM));
    float bandM = exp(-pow((rl2 - rrM) * 14.0, 2.0));
    float easeM = smoothstep(1.0, 0.72, hitM);
    float ringM = bandM * pow(hitM, 1.4) * 0.6 * (0.35 + 0.65 * easeM);
    float ringPush = (ringB + ringM) * rA * ringAmount;

    // rings physically shove the dye they pass through (audio → impulse →
    // the flow's own dynamics carry it; law 5) — displacement kept gentle so
    // a kick reads as a wave, not a full-frame jump
    vec2 pd = p + normalize(rel + 1e-4) * ringB * 0.07 * rA * ringAmount
                + normalize(rel2 + 1e-4) * ringM * 0.05 * rA * ringAmount;

    // ═══ LAYER 1 — the current: iterated curl-noise advection ═══
    vec2 q = pd * (2.1 * flowScale);
    q.y += tFlow * 0.16;                                           // base drift
    float amp = flowAmount * (0.22 + 0.26 * visc) * (0.85 + 0.28 * rA * bassP);
    for (int i = 0; i < 3; i++) {
        vec2 cu = curl2(q * 0.82 + vec2(0.0, tFlow * 0.22) + float(i) * 4.7);
        q += cu * amp;
        amp *= 0.62;
    }

    // ═══ LAYER 2 — dye through the palette ═══
    float d1 = fbm(q * 1.15 + vec2(tFlow * 0.10, 0.0));
    float d2 = fbm(q * 2.60 - vec2(0.0, tFlow * 0.14) + d1 * 1.8);
    float hueDrift = 0.16 * audioBrightness + 0.05 * barTide;
    float t = d1 * 0.95 + d2 * 0.50 + hueDrift + hueShift;
    vec3 dye = pal(t);
    // dyeRichness: contrast + saturation of the dye body
    float dLum = dot(dye, vec3(0.299, 0.587, 0.114));
    dye = mix(vec3(dLum), dye, clamp(0.4 + 0.6 * dyeRichness, 0.0, 2.0));
    dye = pow(max(dye, 0.0), vec3(mix(1.25, 0.85, clamp(dyeRichness * 0.5, 0.0, 1.0))));

    // pseudo-depth lighting: shade by the fine dye layer's gradient
    float shade = 0.62 + 0.55 * d2;
    vec3 col = dye * shade * (0.50 + 0.50 * drive);

    // fine current filaments: thin bright threads along the dye's ridge
    // lines — a CRISP tight-threshold core inside the soft glow band, so the
    // threads read razor-sharp full-screen while keeping the silky halo
    float ridgeT = 1.0 - abs(fract(d2 * 3.0 + d1) * 2.0 - 1.0);
    float ridgeGlow = pow(clamp(ridgeT, 0.0, 1.0), 6.0);
    float ridgeCore = smoothstep(0.90, 0.965, ridgeT);
    float melW = (0.45 + 0.55 * clamp(stemMelody * 1.3, 0.0, 1.0) * rA)
               * clamp(dyeRichness, 0.0, 2.0);
    col += dye * (ridgeGlow * 0.45 + ridgeCore * 0.90) * melW;

    // flow contour threads: thin iso-lines of the dye field, advected by the
    // same bass clock — topographic detail across the whole current
    float isoPh = d2 * 6.0 + d1 * 2.0 - tFlow * 0.06;
    float isoT = 1.0 - abs(fract(isoPh) * 2.0 - 1.0);
    float isoLine = smoothstep(0.86, 0.96, isoT);
    col += (dye * 0.8 + 0.2) * isoLine * 0.55 * clamp(dyeRichness, 0.0, 2.0)
         * smoothstep(0.10, 0.45, d1 * 0.7 + d2 * 0.45);
    // hairline shadow on the line's dark side — engraved relief, extra edge
    float isoDk = smoothstep(0.86, 0.96, 1.0 - abs(fract(isoPh + 0.5) * 2.0 - 1.0));
    col *= 1.0 - isoDk * 0.22 * clamp(dyeRichness, 0.0, 2.0) * 0.5;

    // native background floor (deep water), replaceable via bgColor
    vec3 nativeBg = mix(vec3(0.020, 0.030, 0.065), vec3(0.050, 0.020, 0.075),
                        0.5 + 0.5 * sin(tFlow * 0.20 + p.y));
    vec3 base = (bgColor.a > 0.001) ? mix(nativeBg, bgColor.rgb, bgColor.a) : nativeBg;
    col = base + col * smoothstep(0.02, 0.55, d1 * 0.7 + d2 * 0.45);

    // ring light: warm bloom where the vortex ring crosses the dye
    col += vec3(1.00, 0.80, 0.52) * ringB * rA * ringAmount * 0.36;
    col += vec3(0.55, 0.85, 1.00) * ringM * rA * ringAmount * 0.30;
    // ring wake brightens dye it just pushed (memory-flavored accent)
    col += dye * ringPush * 0.20;

    // whole-current breathing: continuous follower with a compressed top
    // end (soft ratio) — beatless swells read clearly, sharp kick attacks
    // don't slam the whole frame in a single step
    float breathe = 0.34 * (audioBass / (0.55 + audioBass))
                  + 0.16 * (audioMid  / (0.60 + audioMid))
                  + 0.08 * audioHigh;
    col *= 1.0 + rA * breathe;

    // ═══ LAYER 4 — stemAir satin shimmer on luminance ridges ═══
    if (shimmerAmount > 0.001) {
        float ridge = pow(clamp(1.0 - abs(d2 * 2.0 - 1.0), 0.0, 1.0), 3.0);
        float gfld = fbm(q * 4.6 + vec2(tAir * 0.8, -tAir * 0.5));
        float glint = pow(clamp(gfld, 0.0, 1.0), 5.0);
        // crisp satin specks: tight threshold on the glint field carves
        // sharp-edged islands out of the soft shimmer
        float speck = smoothstep(0.72, 0.79, gfld);
        col += vec3(0.85, 0.93, 1.00) * ridge * (glint * 0.7 + speck * 0.85)
             * shimmerAmount * (0.20 + 1.30 * rA * airP);
    }

    // ── finish: gentle vignette, hue, boost, filmic knee ──
    col *= 1.0 - 0.26 * smoothstep(0.60, 1.30, length(p));
    col = hueRot(col, hueShift * 6.28318);
    float l = dot(col, vec3(0.299, 0.587, 0.114));
    col = max(mix(vec3(l), col, clamp(colorBoost, 0.0, 2.0)), 0.0);
    col = 1.0 - exp(-col * 1.45);

    // whisper of STATIC water-grain — fine edge energy at zero motion cost
    float gr = hash21(gl_FragCoord.xy) - 0.5;
    col += gr * 0.044;

    gl_FragColor = vec4(col, 1.0);
}
