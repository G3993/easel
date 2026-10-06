/*{
  "DESCRIPTION": "Gradient Bars",
  "CREDIT": "Joshua Batty",
  "ISFVSN": "2",
  "CATEGORIES": [
    "Generator"
  ],
  "INPUTS": [
    {
      "NAME": "balance",
      "TYPE": "float",
      "DEFAULT": 0.15,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Balance"
    },
    {
      "NAME": "offset",
      "TYPE": "float",
      "DEFAULT": 4,
      "MIN": 0,
      "MAX": 16,
      "LABEL": "Offset"
    },
    {
      "NAME": "use_odd_dirs",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Use Odd Directions"
    },
    {
      "NAME": "x_iter",
      "TYPE": "float",
      "DEFAULT": 2,
      "MIN": 1,
      "MAX": 2,
      "LABEL": "X Iterations"
    },
    {
      "NAME": "inputTex",
      "TYPE": "image",
      "LABEL": "Texture"
    },
    {
      "NAME": "texMix",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Texture Mix"
    },
    {
      "NAME": "num_columns",
      "TYPE": "float",
      "DEFAULT": 8,
      "MIN": 1,
      "MAX": 32,
      "LABEL": "Number of Columns",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "use_columns",
      "TYPE": "bool",
      "DEFAULT": 1,
      "LABEL": "Columns / Rows",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "easing_type",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3,
        4,
        5,
        6,
        7,
        8,
        9,
        10,
        11,
        12,
        13,
        14,
        15,
        16,
        17,
        18,
        19,
        20,
        21,
        22,
        23,
        24,
        25,
        26,
        27,
        28,
        29
      ],
      "LABELS": [
        "Ease In Sine",
        "Ease Out Sine",
        "Ease InOut Sine",
        "Ease In Quad",
        "Ease Out Quad",
        "Ease InOut Quad",
        "Ease In Cubic",
        "Ease Out Cubic",
        "Ease InOut Cubic",
        "Ease In Quart",
        "Ease Out Quart",
        "Ease InOut Quart",
        "Ease In Quint",
        "Ease Out Quint",
        "Ease InOut Quint",
        "Ease In Expo",
        "Ease Out Expo",
        "Ease InOut Expo",
        "Ease In Circ",
        "Ease Out Circ",
        "Ease InOut Circ",
        "Ease In Back",
        "Ease Out Back",
        "Ease InOut Back",
        "Ease In Elastic",
        "Ease Out Elastic",
        "Ease InOut Elastic",
        "Ease In Bounce",
        "Ease Out Bounce",
        "Ease InOut Bounce"
      ],
      "LABEL": "Easing Type",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "speed",
      "TYPE": "float",
      "DEFAULT": 0.03,
      "MIN": 0,
      "MAX": 0.5,
      "LABEL": "Speed",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "invert_speed",
      "TYPE": "float",
      "DEFAULT": 0.05,
      "MIN": 0,
      "MAX": 0.5,
      "LABEL": "Invert Speed",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "phase_iter",
      "TYPE": "float",
      "DEFAULT": 2,
      "MIN": 1,
      "MAX": 16,
      "LABEL": "Phase Iterations",
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "sweep_dir",
      "LABEL": "Cascade Direction",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "wave_scroll",
      "LABEL": "Wave Scroll",
      "TYPE": "float",
      "MIN": -0.5,
      "MAX": 0.5,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "bar_wobble",
      "LABEL": "Bar Wobble",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "burst_period",
      "LABEL": "Burst Period",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 40,
      "DEFAULT": 17,
      "GROUP": "Motion"
    },
    {
      "NAME": "gradient_pow",
      "TYPE": "float",
      "DEFAULT": 0.2,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Gradient Power",
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
      "NAME": "audio_influence",
      "TYPE": "float",
      "DEFAULT": 0.2,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Audio Influence",
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

const float PI = 3.14159265358979;

float easeInSine(float x) {
    return 1.0 - cos((x * PI) / 2.0);
}

float easeOutSine(float x) {
    return sin((x * PI) / 2.0);
}

float easeInOutSine(float x) {
    return -(cos(PI * x) - 1.0) / 2.0;
}

float easeInCubic(float x) {
    return x * x * x;
}

float easeOutCubic(float x) {
    return 1.0 - pow(1.0 - x, 3.0);
}

float easeInOutCubic(float x) {
    return x < 0.5 ? 4.0 * x * x * x : 1.0 - pow(-2.0 * x + 2.0, 3.0) / 2.0;
}

float easeInQuint(float x) {
    return x * x * x * x * x;
}

float easeOutQuint(float x) {
    return 1.0 - pow(1.0 - x, 5.0);
}

float easeInOutQuint(float x) {
    return x < 0.5 ? 16.0 * x * x * x * x * x : 1.0 - pow(-2.0 * x + 2.0, 5.0) / 2.0;
}

float easeInCirc(float x) {
    return 1.0 - sqrt(abs(1.0 - pow(x, 2.0)));
}

float easeOutCirc(float x) {
    return sqrt(abs(1.0 - pow(x - 1.0, 2.0)));
}

float easeInOutCirc(float x) {
    return x < 0.5
      ? (1.0 - sqrt(1.0 - pow(2.0 * x, 2.0))) / 2.0
      : (sqrt(1.0 - pow(-2.0 * x + 2.0, 2.0)) + 1.0) / 2.0;
}

float easeInElastic(float x) {
    float c4 = (2.0 * PI) / 3.0;
    return x == 0.0
      ? 0.0
      : x == 1.0
      ? 1.0
      : -pow(2.0, 10.0 * x - 10.0) * sin((x * 10.0 - 10.75) * c4);
}

float easeOutElastic(float x) {
    float c4 = (2.0 * PI) / 3.0;
    return x == 0.0
      ? 0.0
      : x == 1.0
      ? 1.0
      : pow(2.0, -10.0 * x) * sin((x * 10.0 - 0.75) * c4) + 1.0;
}

float easeInOutElastic(float x) {
    float c5 = (2.0 * PI) / 4.5;
    return x == 0.0
      ? 0.0
      : x == 1.0
      ? 1.0
      : x < 0.5
      ? -(pow(2.0, 20.0 * x - 10.0) * sin((20.0 * x - 11.125) * c5)) / 2.0
      : (pow(2.0, -20.0 * x + 10.0) * sin((20.0 * x - 11.125) * c5)) / 2.0 + 1.0;
}

float easeInQuad(float x) {
    return x * x;
}

float easeOutQuad(float x) {
    return 1.0 - (1.0 - x) * (1.0 - x);
}

float easeInOutQuad(float x) {
    return x < 0.5 ? 2.0 * x * x : 1.0 - pow(-2.0 * x + 2.0, 2.0) / 2.0;
}

float easeInQuart(float x) {
    return x * x * x * x;
}

float easeOutQuart(float x) {
    return 1.0 - pow(1.0 - x, 4.0);
}

float easeInOutQuart(float x) {
    return x < 0.5 ? 8.0 * x * x * x * x : 1.0 - pow(-2.0 * x + 2.0, 4.0) / 2.0;
}

float easeInExpo(float x) {
    return x == 0.0 ? 0.0 : pow(2.0, 10.0 * x - 10.0);
}

float easeOutExpo(float x) {
    return x == 1.0 ? 1.0 : 1.0 - pow(2.0, -10.0 * x);
}

float easeInOutExpo(float x) {
    return x == 0.0
      ? 0.0
      : x == 1.0
      ? 1.0
      : x < 0.5 ? pow(2.0, 20.0 * x - 10.0) / 2.0
      : (2.0 - pow(2.0, -20.0 * x + 10.0)) / 2.0;
}

float easeInBack(float x) {
    float c1 = 1.70158;
    float c3 = c1 + 1.0;
    return c3 * x * x * x - c1 * x * x;
}

float easeOutBack(float x) {
    float c1 = 1.70158;
    float c3 = c1 + 1.0;
    return 1.0 + c3 * pow(x - 1.0, 3.0) + c1 * pow(x - 1.0, 2.0);
}

float easeInOutBack(float x) {
    float c1 = 1.70158;
    float c2 = c1 * 1.525;
    return x < 0.5
      ? (pow(2.0 * x, 2.0) * ((c2 + 1.0) * 2.0 * x - c2)) / 2.0
      : (pow(2.0 * x - 2.0, 2.0) * ((c2 + 1.0) * (x * 2.0 - 2.0) + c2) + 2.0) / 2.0;
}

float easeOutBounce(float x) {
    float n1 = 7.5625;
    float d1 = 2.75;
    if (x < 1.0 / d1) {
        return n1 * x * x;
    } else if (x < 2.0 / d1) {
        return n1 * (x -= 1.5 / d1) * x + 0.75;
    } else if (x < 2.5 / d1) {
        return n1 * (x -= 2.25 / d1) * x + 0.9375;
    } else {
        return n1 * (x -= 2.625 / d1) * x + 0.984375;
    }
}

float easeInBounce(float x) {
    return 1.0 - easeOutBounce(1.0 - x);
}

float easeInOutBounce(float x) {
    return x < 0.5
      ? (1.0 - easeOutBounce(1.0 - 2.0 * x)) / 2.0
      : (1.0 + easeOutBounce(2.0 * x - 1.0)) / 2.0;
}

float fun(float phase, float id) {
    int i = int(id);
    if (i == 0) return easeInSine(phase);
    else if (i == 1) return easeOutSine(phase);
    else if (i == 2) return easeInOutSine(phase);
    else if (i == 3) return easeInQuad(phase);
    else if (i == 4) return easeOutQuad(phase);
    else if (i == 5) return easeInOutQuad(phase);
    else if (i == 6) return easeInCubic(phase);
    else if (i == 7) return easeOutCubic(phase);
    else if (i == 8) return easeInOutCubic(phase);
    else if (i == 9) return easeInQuart(phase);
    else if (i == 10) return easeOutQuart(phase);
    else if (i == 11) return easeInOutQuart(phase);
    else if (i == 12) return easeInQuint(phase);
    else if (i == 13) return easeOutQuint(phase);
    else if (i == 14) return easeInOutQuint(phase);
    else if (i == 15) return easeInExpo(phase);
    else if (i == 16) return easeOutExpo(phase);
    else if (i == 17) return easeInOutExpo(phase);
    else if (i == 18) return easeInCirc(phase);
    else if (i == 19) return easeOutCirc(phase);
    else if (i == 20) return easeInOutCirc(phase);
    else if (i == 21) return easeInBack(phase);
    else if (i == 22) return easeOutBack(phase);
    else if (i == 23) return easeInOutBack(phase);
    else if (i == 24) return easeInElastic(phase);
    else if (i == 25) return easeOutElastic(phase);
    else if (i == 26) return easeInOutElastic(phase);
    else if (i == 27) return easeInBounce(phase);
    else if (i == 28) return easeOutBounce(phase);
    else if (i == 29) return easeInOutBounce(phase);
    else return 0.0;
}

float adjust_balance(float value, float b) {
    float gamma = exp(mix(-2.0, 2.0, b));
    return pow(value, gamma);
}

float stripe_mask_for(float stripe_index, float secondary_in, float secondary_orig) {
    float phase_offset = stripe_index * (1.0 / (num_columns * max(offset, 0.001))) * sweep_dir;
    // R3 chop fix: multiplying TIME by an audio value made the stripe phase
    // jump by TIME*speed*0.1*Δbass — grows unbounded with TIME, and on EDM
    // it was the single biggest frame-step source (p95 0.109). The bounded
    // additive band-follow on animated_coord below already carries the
    // correlated response; keep the clock constant.
    float phase = fract(TIME * speed + phase_offset);
    float lfo = fun(phase, easing_type) * phase_iter - (phase_iter / 2.0);

    bool is_even_stripe = mod(stripe_index, 2.0) < 1.0;
    float secondary_adjusted = is_even_stripe ? secondary_orig : 1.0 - secondary_orig;
    float sec = mix(secondary_in, secondary_adjusted, use_odd_dirs);

    float gradient = pow(sec, gradient_pow * (1.0 - audioLevel * audio_influence * 0.3));
    // Slow, smooth invert oscillation
    gradient = mix(gradient, 1.0 - gradient, 0.5 + sin(TIME * invert_speed) * 0.5);

    // Continuous band-follow (ambient fix r2): LINEAR bass/mid shift the
    // stripe phase itself — geometry breathing that reads even where the
    // HDR bar brightness clips at white or sits at black. Bands are
    // pre-smoothed; silence = exact authored pattern.
    float animated_coord = fract(lfo + gradient
        + TIME * wave_scroll
        + bar_wobble * sin(TIME * 0.7 + stripe_index * 1.3)
        + 0.10 * clamp(audioBass, 0.0, 1.0)
        + 0.06 * clamp(audioMid,  0.0, 1.0));
    float col = 0.5 + 0.5 * sin(animated_coord * 6.28318530718);
    col = adjust_balance(col, balance);
    return 1.0 - col;
}

void main() {
    vec2 uv = isf_FragNormCoord.xy;
    float primary = use_columns ? uv.x : uv.y;
    float secondary = use_columns ? uv.y : uv.x;

    float mirrored_primary = abs(primary - 0.5) * x_iter;
    float stripe_pos = mirrored_primary * num_columns;
    float stripe_index = ceil(stripe_pos);

    float aa = fwidth(stripe_pos);
    float frac_in_stripe = stripe_pos - floor(stripe_pos);
    float edge_t = smoothstep(1.0 - aa, 1.0, frac_in_stripe);

    float mask = stripe_mask_for(stripe_index, secondary, secondary);
    if (edge_t > 0.0) {
        float mask_neighbor = stripe_mask_for(stripe_index + 1.0, secondary, secondary);
        mask = mix(mask, mask_neighbor, edge_t);
    }

    float _ph = fract(TIME / max(burst_period, 1.0));
    float _f  = smoothstep(0.0, 0.04, _ph) * smoothstep(0.18, 0.10, _ph);
    {
        float _which = floor(fract(TIME / max(burst_period, 1.0) + 0.5) * 32.0);
        float _bx = (_which + 0.5) / 32.0;
        float _bandX = exp(-pow((uv.x - _bx) * 80.0, 2.0));
        mask += _bandX * _f * 0.9;
    }

    float peakShape = smoothstep(0.55, 1.0, mask);
    float baseGain  = 1.0 + 0.55 * peakShape;
    // Audio influence is gated by the user control
    float audioGain = (0.85 * audioBass + 0.45 * audioLevel) * audio_influence;
    float burstGain = 0.9 * _f * peakShape;
    float hdrGain   = baseGain + audioGain * peakShape + burstGain;
    hdrGain = min(hdrGain, 2.5);
    float mask_hdr = mask * hdrGain;

    // Continuous smooth band-following (ambient fix r2): LINEAR bands at
    // full calibrated depth, NOT scaled by audio_influence (default 0.2
    // diluted the round-1 depth to ~0.19 effective and the knee crushed
    // ambient swells). The clipping-proof follower is the stripe-phase
    // shift above; this brightness term covers the midtone pixels.
    float _fb = clamp(audioBass, 0.0, 1.0);
    float _fm = clamp(audioMid,  0.0, 1.0);
    mask_hdr *= 1.0 + 0.20 * _fb + 0.12 * _fm;

    // IMG_SIZE() on this engine reports the canvas resolution, not whether an
    // image is actually connected — it's always > 0, so it can't gate the
    // texture branch (that made the bars vanish behind an empty sampler
    // whenever no image was wired up). Always render the procedural bars;
    // blend in the texture only when the user explicitly dials texMix up,
    // matching this library's house convention for optional image inputs.
    vec3 base = mix(vec3(mask_hdr), vec3(1.0, 0.2, 0.8) * mask_hdr, _f * 0.5);
    vec3 outCol = base;
    if (texMix > 0.001) {
        vec4 tex = IMG_NORM_PIXEL(inputTex, uv);
        vec3 texd = tex.rgb * mask_hdr;
        outCol = mix(base, texd, texMix);
    }

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = outCol;
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
    // Full-field bars: tint the darkest end (gaps between bars) toward bg.
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    outCol = uc;

    gl_FragColor = vec4(outCol, 1.0);
}