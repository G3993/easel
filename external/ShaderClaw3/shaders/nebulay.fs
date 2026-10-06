/*{
  "DESCRIPTION": "Nebulay — a self-evolving nebula. Spectral feedback blobs are advected by a rotational 'flockaroo' fluid sim, lit by a gradient-normal surface, then finished with chromatic aberration, film grain and a tweaked ACES tonemap. Fully generative (no input needed); drag the mouse to nudge the feedback flow.",
  "CREDIT": "Port/fusion: flockaroo CFD (CC-BY-NC-SA), shader-web-background feedback blobs (xemantic), spectral_zucconi6 by Alan Zucconi, transverse chromatic aberration after pali6/flexmonkey. Assembled for Easel/ShaderClaw3.",
  "CATEGORIES": [
    "Generator",
    "Simulation",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "inputTex",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "texInject",
      "LABEL": "Image Feed",
      "TYPE": "float",
      "DEFAULT": 0.05,
      "MIN": 0,
      "MAX": 0.5
    },
    {
      "NAME": "feedbackFade",
      "LABEL": "Feedback Fade",
      "TYPE": "float",
      "DEFAULT": 0.9985,
      "MIN": 0.985,
      "MAX": 1
    },
    {
      "NAME": "drawIntensity",
      "LABEL": "Blob Intensity",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "lightHeight",
      "LABEL": "Surface Relief",
      "TYPE": "float",
      "DEFAULT": 250,
      "MIN": 1,
      "MAX": 500
    },
    {
      "NAME": "spec",
      "LABEL": "Specular",
      "TYPE": "float",
      "DEFAULT": 2.5,
      "MIN": 0,
      "MAX": 8
    },
    {
      "NAME": "aberration",
      "LABEL": "Chromatic Ab.",
      "TYPE": "float",
      "DEFAULT": 0.0075,
      "MIN": 0,
      "MAX": 0.05
    },
    {
      "NAME": "grain",
      "LABEL": "Film Grain",
      "TYPE": "float",
      "DEFAULT": 0.15,
      "MIN": 0,
      "MAX": 0.6
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "fluidSpeed",
      "LABEL": "Flow Speed",
      "TYPE": "float",
      "DEFAULT": 2,
      "MIN": 0,
      "MAX": 6,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorInject",
      "LABEL": "Color Feed",
      "TYPE": "float",
      "DEFAULT": 0.025,
      "MIN": 0,
      "MAX": 0.2,
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
      "NAME": "texScale",
      "LABEL": "Image Zoom",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.25,
      "MAX": 4,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "margins",
      "LABEL": "Letterbox",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 0.45,
      "GROUP": "Camera / Layout"
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
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "genBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "fluidBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "litBuf"
    },
    {}
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  NEBULAY — multi-buffer feedback nebula
//  Pass 0 (genBuf)   : shader-web-background feedback blobs — the colour seed
//  Pass 1 (fluidBuf) : flockaroo single-pass rotational CFD — advects genBuf
//  Pass 2 (litBuf)   : gradient-normal lighting for nebula depth
//  Pass 3 (Image)    : chromatic aberration + film grain + tweaked ACES
//
//  Channel wiring (the Shadertoy original's bindings weren't in the paste, so
//  resolved coherently): genBuf feeds itself (self-feedback); fluidBuf reads
//  itself for the velocity field and injects genBuf's colour each frame;
//  litBuf reads fluidBuf; Image reads litBuf + fluidBuf + genBuf.
// ════════════════════════════════════════════════════════════════════════

#define ROTNUM 5

// ── blob / feedback constants (from the original common + buffer c) ──
#define iBlobEdgeSmoothing        0.12
#define iBlob1Radius              0.33
#define iBlob1PowFactor           20.0
#define iBlob2Radius              0.69
#define iBlob2PowFactor           20.0
#define iBlob2ColorPulseShift     3.0
#define iColorShiftOfRadius       (-0.5)
#define iFeedbackZoomRate         0.001
#define iFeedbackColorShiftImpact 0.00051
#define iFeedbackMouseShiftFactor 0.003
#define iFeedbackColorShiftZoom   0.05
#define iBlob1ColorPulseSpeed     0.03456
#define iBlob2ColorPulseSpeed     (-0.02345)

// ── audio conditioning (soft knees + floors; shader stays alive in silence) ──
float aKnee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
float aBassP() { return pow(aKnee(audioBass, 0.05, 0.85), 1.6); } // structural weight
float aMidP()  { return pow(aKnee(audioMid,  0.05, 0.85), 1.2); } // continuous body
float aHighP() { return pow(aKnee(audioHigh, 0.10, 0.90), 1.2); } // sparse sparkle
float aBeatP() { return audioBeatPulse * audioBeatPulse; }        // decaying accent

// ── grain (mutable per-fragment global; valid GLSL) ──
float NoiseSeed;
float randomFloat() {
    NoiseSeed = sin(NoiseSeed) * 84522.13219145687;
    return fract(NoiseSeed);
}

// Tweaked ACES-ish tonemap — keeps the original author's non-standard look.
vec3 ACESFilm(vec3 x) {
    float a = 3.51, b = 1.03, c = 1.43, d = 1.59, e = 2.14;
    return (x * (a * x + b)) / (x * (c * x + d) + e);
}

float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

// ── Spectral Zucconi 6 (visible-spectrum → RGB), used by the blobs ──
float ssat(float x)  { return min(1.0, max(0.0, x)); }
vec3  ssat3(vec3 x)  { return min(vec3(1.0), max(vec3(0.0), x)); }
vec3 bump3y(vec3 x, vec3 yoffset) {
    vec3 y = vec3(1.0) - x * x;
    return ssat3(y - yoffset);
}
vec3 spectral_zucconi6(float x) {
    const vec3 c1 = vec3(3.54585104, 2.93225262, 2.41593945);
    const vec3 x1 = vec3(0.69549072, 0.49228336, 0.27699880);
    const vec3 y1 = vec3(0.02312639, 0.15225084, 0.52607955);
    const vec3 c2 = vec3(3.90307140, 3.21182957, 3.96587128);
    const vec3 x2 = vec3(0.11748627, 0.86755042, 0.66077860);
    const vec3 y2 = vec3(0.84897130, 0.88445281, 0.73949448);
    return bump3y(c1 * (x - x1), y1) + bump3y(c2 * (x - x2), y2);
}

// Repeat-sample (Mac texture-repeat workaround from the original).
vec4 repeatedTexture(sampler2D ch, vec2 uv) { return texture2D(ch, mod(uv, 1.0)); }

// ── optional image texture (bound layer) — aspect-corrected contain fit ──
// Sampled in pass 1 and continuously mixed into the fluid, so the picture
// becomes dye the CFD swirls into the nebula. IMG_SIZE_inputTex.x == 0
// means nothing is bound and the shader stays fully generative.
vec2 texUV(vec2 coord) {
    vec2 st = coord - 0.5;
    float canvasAspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    float texAspect = IMG_SIZE_inputTex.x / max(IMG_SIZE_inputTex.y, 1.0);
    float ratio = canvasAspect / max(texAspect, 0.001);
    if (ratio > 1.0) st.x *= ratio;   // canvas wider than tex — expand X
    else             st.y /= ratio;   // canvas taller than tex — expand Y
    st /= texScale;                   // 1.0 = fit, >1 zoom in, <1 tiles
    st += 0.5;
    return fract(st);
}
vec4 sampleTex(vec2 coord) { return texture2D(inputTex, texUV(coord)); }

float drawBlob(vec2 st, vec2 center, float radius, float edge) {
    float dist = length((st - center) / radius);
    return dist * smoothstep(1.0, 1.0 - (edge - (0.05 * sin(TIME * speed / 5.1))), dist);
}

// ── flockaroo rotational CFD helpers (read the fluid field = fluidBuf) ──
float getRot(vec2 pos, vec2 b, mat2 mu, vec2 Res) {
    vec2 p = b;
    float rot = 0.0;
    for (int i = 0; i < ROTNUM; i++) {
        rot += dot(texture2D(fluidBuf, fract((pos + p) / Res)).xy - vec2(0.5),
                   p.yx * vec2(1.0, -1.0));
        p = mu * p;
    }
    return rot / float(ROTNUM) / dot(b, b);
}

// ── gradient-normal lighting helpers (read fluidBuf) ──
float getVal(vec2 uv) { return length(texture2D(fluidBuf, uv).xyz); }
vec2 getGrad(vec2 uv, float delta) {
    vec2 d = vec2(delta, 0.0);
    return vec2(getVal(uv + d.xy) - getVal(uv - d.xy),
                getVal(uv + d.yx) - getVal(uv - d.yx)) / delta;
}

void main() {
    vec2 uv = isf_FragNormCoord;

    // ═══ PASS 0 — genBuf: spectral feedback blobs ═══
    if (PASSINDEX == 0) {
        float iMinDimension = min(RENDERSIZE.x, RENDERSIZE.y);
        vec2  iScreenRatioHalf = (RENDERSIZE.x >= RENDERSIZE.y)
            ? vec2(RENDERSIZE.y / RENDERSIZE.x * 0.5, 0.5)
            : vec2(0.5, RENDERSIZE.x / RENDERSIZE.y);

        float bT = TIME * speed;
        vec3 iBlob1Color = spectral_zucconi6(mod(bT * iBlob1ColorPulseSpeed, 1.0));
        vec3 iBlob2Color = spectral_zucconi6(mod(bT * iBlob2ColorPulseSpeed + iBlob2ColorPulseShift, 1.0));

        vec2 mPix = (mouseDown > 0.5) ? mousePos * RENDERSIZE : vec2(0.0);
        vec2 iFeedbackShiftVector = (mPix.x > 0.0 && mPix.y > 0.0)
            ? (mPix * 2.0 - RENDERSIZE) / iMinDimension * iFeedbackMouseShiftFactor
            : vec2(0.0);

        vec2 st = (gl_FragCoord.xy * 2.0 - RENDERSIZE) / iMinDimension;

        vec3 feedbk = repeatedTexture(genBuf, uv - st).rgb;
        vec3 colorShift = repeatedTexture(
            genBuf, uv - st * (iFeedbackColorShiftZoom * (1.5 * sin(TIME * speed / 2.81))) * iScreenRatioHalf
        ).rgb;

        vec2 stShift = vec2(0.0);
        stShift += iFeedbackZoomRate * st;
        // +epsilon so the all-black first frame can't produce a NaN lock.
        stShift += (feedbk.bg / (colorShift.br + 1e-4) - 0.5) * iFeedbackColorShiftImpact;
        stShift += iFeedbackShiftVector;
        stShift *= iScreenRatioHalf;

        vec3 prevColor = repeatedTexture(genBuf, uv - stShift).rgb;
        prevColor *= feedbackFade;

        float radius = 1.0 + (colorShift.r + colorShift.g + colorShift.b) * iColorShiftOfRadius;
        // bass swells the dominant blob structure; beats inject a brighter
        // pulse that the fluid then advects for seconds (audio with memory)
        radius *= 1.0 + 0.16 * audioReact * aBassP();
        vec3 drawColor = vec3(0.0);
        drawColor += pow(drawBlob(st, vec2(0.0), radius * iBlob1Radius, iBlobEdgeSmoothing), iBlob1PowFactor) * iBlob1Color;
        drawColor += pow(drawBlob(st, vec2(0.0), radius * iBlob2Radius, iBlobEdgeSmoothing), iBlob2PowFactor) * iBlob2Color;
        drawColor *= drawIntensity * (1.0 + audioReact * (0.20 * aBassP() + 0.45 * aBeatP()));

        vec3 color = clamp(prevColor + drawColor, 0.0, 1.0);
        gl_FragColor = vec4(color, 1.0);
        return;
    }

    // ═══ PASS 1 — fluidBuf: flockaroo rotational self-advection ═══
    if (PASSINDEX == 1) {
        vec2 Res = RENDERSIZE;
        vec2 pos = gl_FragCoord.xy;
        float ang = 6.28318530718 / float(ROTNUM);
        mat2 mu = mat2(cos(ang), sin(ang), -sin(ang), cos(ang));

        float rnd = hash21(vec2(float(FRAMEINDEX) * 0.01 + 0.13, 0.57)) - 0.5;
        vec2 b = vec2(cos(ang * rnd), sin(ang * rnd));

        vec2 v = vec2(0.0);
        float bbMax = 0.7 * Res.y; bbMax *= bbMax;
        for (int l = 0; l < 12; l++) {
            if (dot(b, b) > bbMax) break;
            vec2 p = b;
            for (int i = 0; i < ROTNUM; i++) {
                v += p.yx * getRot(pos + p, b, mu, Res);  // odd-ROTNUM fast path
                p = mu * p;
            }
            b *= 2.0;
        }

        // Audio injects energy into the fluid's own momentum rather than raw
        // color (law 5): bass swells the swirl speed, a beat adds a short
        // decaying kick — the CFD's inertia then carries it for seconds.
        // (mids keep the flow following beatless swells; the beat kick is kept
        // small — a large step here shifts the whole advected image in one
        // frame, which reads as a jump on a near-static nebula.)
        float fluidAudioMod = 1.0 + audioReact * (0.30 * aBassP() + 0.18 * aMidP() + 0.25 * aBeatP());
        vec2 advUV = fract((pos + v * vec2(-1.0, 1.0) * fluidSpeed * fluidAudioMod) / Res);
        vec4 col  = texture2D(fluidBuf, advUV);
        vec4 col2 = texture2D(genBuf,   advUV);
        vec4 blend = mix(col, col2, colorInject);

        // Image dye — keep pulling the bound texture into the fluid so the
        // advection swirls it together with the blob colors. Sampled at the
        // un-advected uv: the injection is sharp, the history is what warps.
        if (texInject > 0.001 && IMG_SIZE_inputTex.x > 0.0)
            blend = mix(blend, sampleTex(uv), texInject);

        // Seed the fluid from the blob field for the first frames.
        if (FRAMEINDEX < 4) blend = texture2D(genBuf, uv);

        // Stability bound: this is a self-advecting float accumulator with
        // no other ceiling, so tiny per-frame gain compounds over minutes of
        // playback into an unbounded runaway (pre-existing, not audio-
        // related). Cap it generously — far above anything the visible look
        // needs in normal operation — so it can't diverge to infinity while
        // leaving the intended "blooms toward white" character untouched.
        blend = clamp(blend, -2.5, 2.5);

        gl_FragColor = blend;
        return;
    }

    // ═══ PASS 2 — litBuf: gradient-normal surface lighting ═══
    if (PASSINDEX == 2) {
        // Continuous band-follow on the lighting (visible on beatless
        // material): mids flatten the relief slightly, highs lift the
        // specular. Smooth envelopes only — no gates.
        float relief = lightHeight * abs(sin(TIME / 13.0))
                     * (1.0 - 0.30 * min(audioReact, 1.5) * aMidP());
        vec3 n = vec3(getGrad(uv, 1.0 / RENDERSIZE.y), relief);
        n = normalize(n);
        vec3 light = normalize(vec3(1.0, 1.0, 2.0));
        float diff = clamp(dot(n, light), 0.05, 1.0);
        float sp = clamp(dot(reflect(light, n), vec3(0.0, 0.0, -1.0)), 0.0, 1.0);
        sp = pow(sp, 36.0) * spec * (1.0 + 0.5 * min(audioReact, 1.5) * aHighP());
        gl_FragColor = texture2D(fluidBuf, uv) * vec4(diff) + vec4(sp);
        return;
    }

    // ═══ PASS 3 — Image: chromatic aberration + grain + ACES ═══
    if (uv.y < margins || uv.y > 1.0 - margins) {
        gl_FragColor = vec4(ACESFilm(vec3(0.0)), 1.0);
        return;
    }

    NoiseSeed = float(FRAMEINDEX) * 0.003186154 + gl_FragCoord.y * 17.2986546543 + gl_FragCoord.x;

    // Highs add fine sparkle: a touch more chromatic split + grain, sparse
    // and never gating the base image (idle floor stays intact at audio=0).
    float aberrMod = aberration * (1.0 + audioReact * 0.5 * aHighP());
    vec2 d = (uv - 0.5) * aberrMod;

    vec3 color = vec3(texture2D(litBuf, uv + 0.5 * d).r,
                      texture2D(litBuf, uv - 1.0 * d).g,
                      texture2D(litBuf, uv - 2.0 * d).b);

    vec3 col  = vec3(texture2D(fluidBuf, uv).r,
                     texture2D(fluidBuf, uv - 1.0 * d).g,
                     texture2D(fluidBuf, uv - 2.0 * d).b);

    vec3 col2 = vec3(texture2D(genBuf, uv).r,
                     texture2D(genBuf, uv - 1.0 * d).g,
                     texture2D(genBuf, uv - 2.0 * d).b);

    col = max(col, col2);

    // fluidBuf is an unbounded float accumulator (self-advecting feedback) —
    // over time it drifts well past 1.0 and the final blend clips to flat
    // white, at which point ordinary brightness modulation is invisible.
    // Bass pulls the glow back toward the bounded genBuf/litBuf floor
    // (color/3 below), so the nebula visibly "breathes" open on a bass hit
    // and lets its spectral structure show — a soft knee, silent-safe (no
    // change at audioReact=0 or audioBass=0), that reads as a tasteful
    // contraction rather than a raw brightness cut.
    // Depth capped at 0.45 and eased through a smoothstep knee: the old
    // 0.9-deep hard clamp yanked full-frame luminance down in a single frame
    // on every kick — on a near-static nebula that read as strobing
    // (choppiness ratio ~70). Mids join bass so beatless swells (ambient)
    // move the pull continuously too.
    float bassDrive = audioReact * (1.4 * aBassP() + 0.7 * aMidP() + 0.5 * aBeatP());
    float bloomPull = 1.0 - 0.45 * smoothstep(0.05, 1.1, bassDrive);
    col *= bloomPull;

    float grainMod = grain * (1.0 + audioReact * 0.35 * aHighP());
    float noise = 0.9 + randomFloat() * grainMod;
    vec3 preTone = ((color * 0.5) + max(col, color / 3.0)) * noise;

    // R3: the accumulator rides so far past the ACES knee (this ACES hits 1.0
    // at x≈1.16 and the buffer sits near 2+) that ANY post-tonemap gain or dip
    // stays clipped flat white — the round-2 duck was invisible. Respond
    // PRE-tonemap instead, deep enough to pull peaks under the clip point,
    // and stir a music-driven dark dye blob through the frame so the
    // near-frozen nebula has actual moving structure to correlate (its orbit
    // advances with audioBassTime — frozen in silence). Silence = exactly 1.0.
    float depR3  = 0.55 + 0.45 * min(audioReact, 1.0);
    float duckR3 = depR3 * (0.50 * audioBass + 0.28 * audioMid);
    preTone *= 1.0 - duckR3;

    float aT3  = audioBassTime * 2.4;
    vec2  sw3  = (uv - 0.5) + 0.28 * vec2(cos(aT3), sin(aT3 * 1.37));
    float dye3 = exp(-dot(sw3, sw3) * 28.0)
               * (0.55 * audioBass + 0.30 * audioMid + 0.50 * audioBeatPulse);

    vec3 outCol = ACESFilm(preTone);

    // Guaranteed-visible response: this ACES output exceeds 1.0 over most of
    // the frame, so a plain multiply stays clipped. Fold to the display
    // ceiling FIRST (stored value is identical — the buffer clamps anyway,
    // so silence renders the exact same image), then darken-dip from the
    // ceiling: whole-frame breathing linear in bass+mid, plus the moving dye
    // carve. Dips can't clip, so they always read.
    outCol = min(outCol, vec3(1.0));
    outCol *= (1.0 - depR3 * (0.28 * audioBass + 0.16 * audioMid))
            * (1.0 - min(dye3, 0.8));

    float haloR3 = smoothstep(1.2, 0.15, length(uv - 0.5));
    outCol *= 1.0 + 0.30 * depR3 * audioBeatPulse * haloR3;

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(outCol, vec3(0.299, 0.587, 0.114));
    vec3 uc = mix(vec3(ucL), outCol, colorBoost);
    if (hueShift > 0.0005) {
        float hueA = hueShift * 6.2831853;
        float hueC = cos(hueA), hueS = sin(hueA);
        mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                  + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                  + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hueM * uc, 0.0, 1.0);
    }
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

    gl_FragColor = vec4(uc, 1.0);
}
