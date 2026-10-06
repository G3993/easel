/*{
  "DESCRIPTION": "Caustic Floor — an HD chromatic caustic web dances over a rippled sandy sea floor scattered with half-buried stones; the sand sways gently in the current, a broad beam of passing light sweeps by, and blue depth-haze swallows the distance. Bass swells the caustic glow, mids stir the current, highs sharpen the web's filaments.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Nature",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.1,
      "MAX": 3.0
    },
    {
      "NAME": "causticScale",
      "LABEL": "Caustic Scale",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.5,
      "MAX": 3.0
    },
    {
      "NAME": "causticSharp",
      "LABEL": "Caustic Sharpness",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.4,
      "MAX": 2.0
    },
    {
      "NAME": "swayAmount",
      "LABEL": "Current Sway",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 2.0
    },
    {
      "NAME": "depthHaze",
      "LABEL": "Depth Haze",
      "TYPE": "float",
      "DEFAULT": 0.5,
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
  ]
}*/

#define TAU 6.2831853

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float h21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

vec3 hash32(vec2 src) {
    vec3 p3 = fract(src.xyx * vec3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yxz + 33.33);
    return fract((p3.xxy + p3.yzz) * p3.zyx);
}

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(h21(i), h21(i + vec2(1.0, 0.0)), f.x),
               mix(h21(i + vec2(0.0, 1.0)), h21(i + vec2(1.0, 1.0)), f.x), f.y);
}

float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.03 + vec2(1.7, 9.2);
        a *= 0.5;
    }
    return v;
}

// voronoi F1 + cell hash — half-buried stones
vec2 stones(vec2 p) {
    vec2 id = floor(p), f = fract(p);
    float best = 8.0;
    float bh = 0.0;
    for (int y = -1; y <= 1; y++)
    for (int x = -1; x <= 1; x++) {
        vec2 o = vec2(float(x), float(y));
        vec2 h = vec2(h21(id + o), h21(id + o + 17.7));
        vec2 c = o + 0.5 + (h - 0.5) * 0.7 - f;
        float d = length(c);
        if (d < best) { best = d; bh = h.x; }
    }
    return vec2(best, bh);
}

float sandH(vec2 p) {
    // wandering dune ripples (grain lives in the albedo, not the relief —
    // high-frequency height noise turns normals to mush)
    float warp = fbm(p * 0.7) * 2.6;
    float rip = sin(p.x * 3.2 + warp + p.y * 0.5);
    rip = pow(rip * 0.5 + 0.5, 1.6) * 0.45;
    // stones as low rounded domes in ~25% of cells
    vec2 st = stones(p * 0.85 + 4.7);
    float dome = smoothstep(0.42, 0.08, st.x);
    float stone = dome * dome * (3.0 - 2.0 * dome) * step(0.75, st.y);
    return rip + stone * 0.38;
}

// the classic layered refraction caustic, parametric sharpness
float caustic(vec2 uv, float t, float sharpExp) {
    vec2 p = mod(uv * TAU, TAU) - 250.0;
    vec2 i = p;
    float c = 1.0;
    float inten = 0.005;
    for (int n = 0; n < 5; n++) {
        float tt = t * (1.0 - 3.5 / float(n + 1));
        i = p + vec2(cos(tt - i.x) + sin(tt + i.y), sin(tt - i.y) + cos(tt + i.x));
        c += 1.0 / length(vec2(p.x / (sin(i.x + tt) / inten),
                               p.y / (cos(i.y + tt) / inten)));
    }
    c /= 5.0;
    c = 1.17 - pow(c, 1.4);
    return pow(abs(c), sharpExp);
}

// specks riding the current
float motes(vec2 uv, float scale, float t, float seed) {
    vec2 gv = uv * scale + vec2(t * 0.12, sin(t * 0.3 + seed) * 0.05);
    vec2 id = floor(gv);
    vec2 f = fract(gv) - 0.5;
    vec3 h = hash32(id + seed);
    vec2 pp = (h.xy - 0.5) * 0.7;
    float d = length(f - pp);
    float r = 0.03 + 0.05 * h.z;
    return smoothstep(r, r * 0.25, d) * (0.3 + 0.7 * h.z);
}

void main() {
    vec2 uv = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y;
    float t = TIME * speed;
    float ar = audioReact;
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP   = pow(knee(audioMid,  0.08, 0.90), 1.3);
    float highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float levelP = knee(audioLevel, 0.05, 0.90);

    float far01 = clamp(uv.y * 0.85 + 0.5, 0.0, 1.0);   // top of frame = farther away

    // floor coordinates with a hint of perspective
    vec2 wp = uv * vec2(1.0 + far01 * 0.6, 1.35) * 3.2;

    // gentle current: the light web sways more than the sand itself
    vec2 sway = swayAmount * vec2(sin(t * 0.31 + uv.y * 1.8), cos(t * 0.23 + uv.x * 1.3));
    vec2 sandUV = wp + sway * 0.02 * (1.0 + ar * 0.35 * midP);

    // sand relief + normal
    float e = 0.08;
    float h0 = sandH(sandUV);
    float hx = sandH(sandUV + vec2(e, 0.0));
    float hy = sandH(sandUV + vec2(0.0, e));
    vec3 n = normalize(vec3(-(hx - h0) / e, -(hy - h0) / e, 1.6));

    // stone mask again for albedo
    vec2 st = stones(sandUV * 0.85 + 4.7);
    float stoneM = smoothstep(0.38, 0.16, st.x) * step(0.75, st.y);
    // grain as fine albedo speckle
    float grain = vnoise(sandUV * 38.0) * 0.6 + vnoise(sandUV * 79.0) * 0.4;
    vec3 albedo = vec3(0.55, 0.50, 0.38) * (0.82 + 0.36 * grain);
    albedo = mix(albedo, vec3(0.30, 0.33, 0.32) * (0.75 + 0.5 * st.y), stoneM);

    // key light slowly precessing overhead
    vec3 ld = normalize(vec3(sin(t * 0.05) * 0.4, 0.55, 0.72));
    float ndl = clamp(dot(n, ld), 0.0, 1.0);
    vec3 col = albedo * (vec3(0.09, 0.17, 0.23) + vec3(0.55, 0.72, 0.72) * ndl * 0.9);

    // large-scale dappled cloud-light drifting over the floor
    float lp = fbm(wp * 0.35 + vec2(t * 0.05, -t * 0.03));
    col *= 0.80 + 0.40 * lp;

    // ---- the caustic web: chromatic triple-sample, parallax over the relief ----
    float sharpE = 3.5 + 3.5 * causticSharp + ar * 2.5 * highP;
    vec2 cp = wp * 0.22 * causticScale
            + sway * 0.05 * (1.0 + ar * 0.4 * midP)
            + n.xy * 0.06 - h0 * 0.05;
    float t2 = t * 0.55;
    float chrom = 0.008;
    vec3 cau = vec3(caustic(cp, t2, sharpE),
                    caustic(cp + vec2(chrom, chrom * 0.6), t2, sharpE),
                    caustic(cp - vec2(chrom * 0.7, chrom), t2, sharpE));
    float cauAmp = 0.60 + ar * (0.50 * bassP + 0.20 * levelP);
    col += cau * vec3(0.50, 0.74, 0.82) * cauAmp * (0.35 + 0.65 * ndl);

    // a broad beam of passing light sweeping the floor
    float beamX = sin(t * 0.09) * 0.9;
    float beam = exp(-pow((uv.x - beamX) * 1.6, 2.0)) * (0.35 + 0.25 * sin(t * 0.13 + 2.0));
    col *= 1.0 + beam * (0.35 + ar * 0.4 * levelP);

    // blue depth-haze toward the far edge
    col = mix(col, vec3(0.06, 0.20, 0.28), smoothstep(0.15, 1.0, far01) * depthHaze);

    // specks drifting with the current
    float m = motes(uv, 14.0, t, 3.1) + motes(uv, 24.0, t * 1.3, 9.7) * 0.5;
    col += vec3(0.50, 0.75, 0.80) * m * 0.20 * (1.0 + ar * 0.6 * highP);

    // gentle exposure breath with the mix
    col *= 1.0 + 0.10 * ar * levelP;

    col *= 1.0 - 0.30 * dot(uv, uv);        // vignette
    col = col / (1.0 + col * 0.45);         // soft shoulder
    col *= tintColor.rgb * brightness;
    gl_FragColor = vec4(col, 1.0);
}
