/*{
  "DESCRIPTION": "Bubble Ascent — parallax streams of wobbling bubbles rise toward a glowing surface, each one a little lens with rim light and twin specular glints; soft-focus far and foreground layers give a depth-of-field feel, the abyss darkens below and slow god-ray beams comb the water. Bass releases extra bubble plumes, mids sway the wobble, highs flare the glints.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Particles",
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
      "NAME": "bubbleRate",
      "LABEL": "Bubble Rate",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.3,
      "MAX": 2.0
    },
    {
      "NAME": "bubbleSize",
      "LABEL": "Bubble Size",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.5,
      "MAX": 2.0
    },
    {
      "NAME": "wobble",
      "LABEL": "Wobble",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 2.0
    },
    {
      "NAME": "abyssDepth",
      "LABEL": "Abyss Depth",
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
  ]
}*/

#define TAU 6.2831853

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

vec4 hash42(vec2 src) {
    vec4 p4 = fract(vec4(src.xyxy) * vec4(0.1031, 0.1030, 0.0973, 0.1099));
    p4 += dot(p4, p4.wzxy + 33.33);
    return fract((p4.xxyz + p4.yzzw) * p4.zywx);
}

vec3 hash32(vec2 src) {
    vec3 p3 = fract(src.xyx * vec3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yxz + 33.33);
    return fract((p3.xxy + p3.yzz) * p3.zyx);
}

// water column: bright glowing surface above, abyss below
vec3 bgWater(vec2 uv) {
    float g = clamp(uv.y * 0.55 + 0.55, 0.0, 1.0);
    g = pow(g, mix(1.1, 2.8, abyssDepth));
    vec3 col = mix(vec3(0.004, 0.012, 0.026), vec3(0.10, 0.36, 0.52), g);
    col += vec3(0.20, 0.45, 0.55)
         * exp(-length((uv - vec2(0.0, 0.95)) * vec2(1.1, 1.7)) * 1.6) * 0.9;
    return col;
}

// slow slanted god-ray beams combing down from the surface
float shafts(vec2 uv, float t) {
    float x = uv.x + (0.9 - uv.y) * 0.28;
    float s = 0.5 + 0.5 * sin(x * 8.0 + t * 0.30);
    s *= 0.5 + 0.5 * sin(x * 5.1 - t * 0.19 + 1.7);
    s *= 0.5 + 0.5 * sin(x * 2.3 + t * 0.11 + 4.2);
    return pow(s, 2.0) * smoothstep(-0.75, 0.85, uv.y);
}

float motes(vec2 uv, float scale, float t, float seed) {
    vec2 gv = uv * scale + vec2(t * 0.03, -t * 0.10);
    vec2 id = floor(gv);
    vec2 f = fract(gv) - 0.5;
    vec3 h = hash32(id + seed);
    float d = length(f - (h.xy - 0.5) * 0.7);
    float r = 0.03 + 0.05 * h.z;
    return smoothstep(r, r * 0.25, d) * (0.3 + 0.7 * h.z);
}

// one parallax layer of rising bubbles (3x5 neighbourhood so wobble never clips)
void bubbleLayer(inout vec3 col, vec2 uv, float scale, float rise, float blur,
                 float dens, float seed, float opac, float t,
                 float wobAmp, float glintAmp) {
    vec2 p = uv * scale;
    p.y -= t * rise;
    vec2 id0 = floor(p);
    for (int yy = -1; yy <= 1; yy++)
    for (int xx = -2; xx <= 2; xx++) {
        vec2 id = id0 + vec2(float(xx), float(yy));
        vec4 h = hash42(id * 1.13 + seed);
        if (h.w > dens) continue;
        // wobbling ascent path — phase hashed per bubble so nothing moves in lockstep
        float wob = sin(uv.y * (2.5 + 3.0 * h.y) + t * (0.55 + 0.5 * h.x) + h.z * TAU)
                  * 0.22 * wobAmp;
        vec2 bp = id + 0.5 + vec2((h.x - 0.5) * 0.45 + wob, (h.y - 0.5) * 0.45);
        float r = (0.11 + 0.14 * h.z) * bubbleSize;
        vec2 q = p - bp;
        float d = length(q);
        if (d > r * (1.0 + blur) + 0.05) continue;
        float edge = r * (0.10 + 0.90 * blur);
        float body = 1.0 - smoothstep(r - edge, r + edge, d);
        // the lens: flipped, magnified view of the water gradient behind it
        vec3 lens = bgWater(uv - (q / scale) * 2.4) * 1.25;
        float rim = smoothstep(r * 0.45, r, d);
        vec3 bub = lens * (0.55 + 0.45 * rim);
        bub += vec3(0.75, 0.92, 1.00) * rim * rim * 0.55;
        // twin specular glints (key light + fill)
        float s1 = length(q - vec2(-0.34, 0.36) * r);
        float s2 = length(q - vec2( 0.22, -0.40) * r);
        float gl = exp(-s1 * s1 / (r * r * 0.015)) * 1.2
                 + exp(-s2 * s2 / (r * r * 0.035)) * 0.35;
        bub += vec3(0.90, 1.00, 1.00) * gl * glintAmp;
        col = mix(col, bub, clamp(body * opac, 0.0, 1.0));
    }
}

void main() {
    vec2 uv = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y;
    float t = TIME * speed;
    float ar = audioReact;
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP   = pow(knee(audioMid,  0.08, 0.90), 1.3);
    float highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float levelP = knee(audioLevel, 0.05, 0.90);
    float beatP  = audioBeatPulse * audioBeatPulse; // host-side decaying envelope

    vec3 col = bgWater(uv);

    // god rays + drifting micro-particles behind everything
    float sh = shafts(uv, t);
    col += vec3(0.18, 0.42, 0.55) * sh * (0.55 + ar * 0.45 * levelP) * 0.9;
    float m = motes(uv, 18.0, t, 5.1) + motes(uv, 30.0, t * 1.25, 11.7) * 0.5;
    col += vec3(0.45, 0.70, 0.80) * m * 0.15 * (1.0 + ar * 0.5 * highP);

    float glint = 0.80 + ar * 1.40 * highP;
    float wobA  = wobble * (0.80 + ar * 0.50 * midP);
    float dens  = clamp(0.32 * bubbleRate, 0.05, 0.80);
    // bass plume: fine dense bubbles whose visibility rides the low end
    // (their motion never changes — only how present they are, so no popping)
    float burst = ar * (0.60 * bassP + 0.35 * beatP);

    //                       scale  rise  blur  dens        seed   opacity
    bubbleLayer(col, uv,     20.0,  1.15, 0.85, dens * 0.9,  3.1,  0.30, t, wobA,        glint * 0.4);
    bubbleLayer(col, uv,     26.0,  3.40, 0.35, 0.75,       23.7,  0.10 + 0.50 * burst,
                t, wobA * 1.3, glint * 0.6);
    bubbleLayer(col, uv,     13.0,  1.75, 0.45, dens,        7.7,  0.55, t, wobA,        glint * 0.7);
    bubbleLayer(col, uv,      8.0,  2.60, 0.18, dens,       13.3,  0.85, t, wobA,        glint);
    // foreground bokeh — big, very soft, sparse
    bubbleLayer(col, uv,      4.2,  3.60, 1.40, dens * 0.5, 31.9,  0.28, t, wobA * 0.7,  glint * 0.5);

    // gentle exposure breath with the mix
    col *= 1.0 + 0.12 * ar * levelP;

    col *= 1.0 - 0.28 * dot(uv * vec2(1.0, 0.8), uv * vec2(1.0, 0.8)); // vignette
    col = col / (1.0 + col * 0.45);   // soft shoulder
    col *= tintColor.rgb * brightness;
    gl_FragColor = vec4(col, 1.0);
}
