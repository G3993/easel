/*{
  "CATEGORIES": [
    "Generator",
    "Physical",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Wave Interference Field — Multiple point-source ripples on a water surface — concentric wavefronts radiating from N sources, summing constructively + destructively into shimmering moiré patterns. Bass kicks add new sources at hashed positions; treble shortens wavelength; the grid is a real PHYSICAL standing-wave field.",
  "INPUTS": [
    {
      "NAME": "sourceCount",
      "LABEL": "Sources",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 12,
      "DEFAULT": 5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "waveFrequency",
      "LABEL": "Frequency",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 80,
      "DEFAULT": 28,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "waveSpeed",
      "LABEL": "Wave Speed",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 8,
      "DEFAULT": 2.4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "drift",
      "LABEL": "Source Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.35,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "peakColor",
      "LABEL": "Peak (Crest)",
      "TYPE": "color",
      "DEFAULT": [
        1,
        1,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "midColor",
      "LABEL": "Mid (Cyan)",
      "TYPE": "color",
      "DEFAULT": [
        0.18,
        0.78,
        0.92,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "deepColor",
      "LABEL": "Deep (Indigo)",
      "TYPE": "color",
      "DEFAULT": [
        0.04,
        0.08,
        0.28,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "troughColor",
      "LABEL": "Trough (Magenta)",
      "TYPE": "color",
      "DEFAULT": [
        0.62,
        0.1,
        0.55,
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
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "LABEL": "Background",
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "falloff",
      "LABEL": "Energy Falloff",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "sourceMarkers",
      "LABEL": "Show Sources",
      "TYPE": "bool",
      "DEFAULT": true
    }
  ]
}*/

// Wave Interference Field — physical 2D linear wave superposition.
// Each source contributes A_i = sin(k * r_i - omega * t - phi_i) / sqrt(r_i)
// where r_i is the distance from pixel to source i. The total amplitude
// is mapped through a four-stop palette (deep / mid / peak / trough).

const int MAX_SOURCES = 12;

float hash11(float p) {
    return fract(sin(p * 12.9898 + 78.233) * 43758.5453);
}

vec2 hash22(float p) {
    return vec2(hash11(p), hash11(p + 17.31));
}

// Source position: hashed seed point, slowly drifting via slow sin/cos so
// the interference geometry breathes even on TIME alone.
vec2 sourcePos(int idx, float t, float driftAmt) {
    float fi = float(idx);
    vec2 seed = hash22(fi * 7.13 + 1.7);
    // Map seed into [0.15, 0.85] so sources stay inside the visible canvas.
    vec2 base = 0.15 + 0.70 * seed;
    // Slow Lissajous drift, unique per source.
    float phx = hash11(fi * 3.91) * 6.2831853;
    float phy = hash11(fi * 5.77) * 6.2831853;
    float sx = sin(t * 0.11 + phx) + 0.5 * sin(t * 0.27 + phx * 1.3);
    float sy = cos(t * 0.09 + phy) + 0.5 * cos(t * 0.31 + phy * 1.7);
    return base + driftAmt * 0.18 * vec2(sx, sy);
}

// Per-source phase offset so sources are not synchronized.
float sourcePhase(int idx) {
    return hash11(float(idx) * 11.71) * 6.2831853;
}

// Per-source amplitude weight; bass kicks boost transient sources.
// idx 0..baseN-1 are stable; the upper slots fade in with bass.
float sourceWeight(int idx, float baseN, float bass) {
    float fi = float(idx);
    if (fi < baseN) return 1.0;
    // Transient slots — pulse with bass, with a unique threshold so they
    // don't all activate at once.
    float thresh = 0.35 + 0.5 * hash11(fi * 2.13);
    float k = smoothstep(thresh, thresh + 0.15, bass);
    // Slight breathing so even active transients shimmer.
    float breath = 0.6 + 0.4 * sin(TIME * (1.7 + hash11(fi * 4.4)) + fi);
    return k * breath;
}

// Four-stop palette: trough -> deep -> mid -> peak.
// amp is in roughly [-1, 1]; we remap to [0, 1].
vec3 palette(float amp, vec3 trough, vec3 deep, vec3 mid, vec3 peak) {
    float t = clamp(0.5 + 0.5 * amp, 0.0, 1.0);
    vec3 c;
    if (t < 0.25) {
        c = mix(trough, deep, t / 0.25);
    } else if (t < 0.55) {
        c = mix(deep, mid, (t - 0.25) / 0.30);
    } else {
        c = mix(mid, peak, (t - 0.55) / 0.45);
    }
    return c;
}

void main() {
    // Aspect-correct canvas coordinates centered around the working area.
    vec2 uv = isf_FragNormCoord;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    vec2 p = vec2(uv.x * aspect, uv.y);

    // Audio drives: bass spawns transient sources; treble shortens lambda.
    float bass = audioBass * audioReact;
    float treb = audioHigh * audioReact;
    float mids = audioMid * audioReact;

    float baseN = clamp(sourceCount, 1.0, float(MAX_SOURCES));
    float freq = waveFrequency * (1.0 + 1.7 * treb);
    float speed = waveSpeed * (1.0 + 0.6 * mids);

    // Sum waves from all potential sources.
    float amp = 0.0;
    float energy = 0.0;
    for (int i = 0; i < MAX_SOURCES; i++) {
        float w = sourceWeight(i, baseN, bass);
        if (w <= 0.0001) continue;
        vec2 sp = sourcePos(i, TIME, drift);
        // Aspect-correct the source too.
        sp.x *= aspect;
        float r = distance(p, sp);
        // Avoid singularity at the source itself.
        float rs = max(r, 0.0035);
        float phi = sourcePhase(i);
        float phase = rs * freq - TIME * speed - phi;
        // 1/sqrt(r) energy-conserving radial falloff (exponent user-tunable).
        float atten = 1.0 / pow(rs, 0.5 * falloff);
        amp += w * sin(phase) * atten;
        energy += w * atten;
    }
    // Normalize so the field stays roughly in [-1, 1] regardless of N.
    if (energy > 1e-4) amp /= max(energy * 0.6, 0.5);

    // Bass punch: an audible kick swells the whole field's crest/trough
    // contrast so the surface visibly "breathes" with the beat, on top of
    // the transient-source spawning above.
    amp += 2.4 * bass * sin(TIME * 3.0 + p.x * 6.0);
    amp *= 1.0 + 1.4 * treb;

    // Tonemap and palette.
    vec3 col = palette(amp, troughColor.rgb, deepColor.rgb, midColor.rgb, peakColor.rgb);

    // Subtle vignette so peaks read on bright displays.
    vec2 vc = uv - 0.5;
    float vig = 1.0 - 0.35 * dot(vc, vc);
    col *= vig;

    // Direct energy punch: loud bass brightens the whole field so the
    // audio response is felt, not just seen in source spawning.
    col *= 1.0 + 1.4 * bass;

    // Optional source markers — bright dots at each active emitter.
    if (sourceMarkers) {
        for (int i = 0; i < MAX_SOURCES; i++) {
            float w = sourceWeight(i, baseN, bass);
            if (w <= 0.0001) continue;
            vec2 sp = sourcePos(i, TIME, drift);
            sp.x *= aspect;
            float d = distance(p, sp);
            float core = smoothstep(0.012, 0.0, d);
            float halo = smoothstep(0.045, 0.012, d) * 0.35;
            float pulse = 0.7 + 0.3 * sin(TIME * 4.0 + float(i));
            col += (core + halo) * pulse * w * peakColor.rgb;
        }
    }

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = col;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                     // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    // background: tint the darkest end (deep-water troughs) toward bgColor
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    col = uc;

    gl_FragColor = vec4(col, 1.0);
}
