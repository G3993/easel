/*{
  "DESCRIPTION": "Ripples — multi-scale reaction-diffusion fluid on a Gaussian pyramid. Two RD fields advected through an auto-driven vortex stir, blurred across a packed mip pyramid (Buffer C horizontal, Buffer D vertical) and composited with gradient-lit highlights into a flowing liquid surface. Ported from Shadertoy multi-buffer (trirop-style RD pyramid) to Easel ISF: iChannels mapped to named persistent buffers, mouse replaced by an autonomous stir, noise made procedural.",
  "CREDIT": "Shadertoy RD-pyramid (original author) — ISF port for Easel",
  "CATEGORIES": [
    "Generator",
    "Fluid",
    "Simulation"
  ],
  "INPUTS": [
    {
      "NAME": "inputTex",
      "TYPE": "image",
      "LABEL": "Texture"
    },
    {
      "NAME": "texMix",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "LABEL": "Texture Mix"
    },
    {
      "NAME": "stirSpeed",
      "LABEL": "Stir Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.25,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "stirStrength",
      "LABEL": "Stir Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1.4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "tintLow",
      "LABEL": "Tint Low",
      "TYPE": "color",
      "DEFAULT": [
        0.1,
        0,
        0.4,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "tintHigh",
      "LABEL": "Tint High",
      "TYPE": "color",
      "DEFAULT": [
        0.25,
        0.75,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "exposure",
      "LABEL": "Exposure",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2.5,
      "DEFAULT": 1,
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
      "DEFAULT": 0.35,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "bufA",
      "PERSISTENT": true
    },
    {
      "TARGET": "bufB",
      "PERSISTENT": true
    },
    {
      "TARGET": "bufC",
      "PERSISTENT": true
    },
    {
      "TARGET": "bufD",
      "PERSISTENT": true
    },
    {}
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  RIPPLES — Gaussian-pyramid reaction-diffusion fluid (Easel ISF port).
//
//  Shadertoy → Easel mapping:
//    iChannel0 -> bufA   (RD field A, self-feedback)
//    iChannel1 -> bufB   (RD field B, self-feedback)
//    iChannel3 -> bufD   (vertical-blur pyramid)
//    iChannel2 -> bufC   (only inside Buffer D's vertical blur)
//    iMouse    -> autonomous circular "stir"
//    iFrame    -> FRAMEINDEX,  iTime -> TIME,  iResolution -> RENDERSIZE
//  Pass order: bufA, bufB, bufC, bufD, image  (PASSINDEX 0..4).
// ════════════════════════════════════════════════════════════════════════

#define pi2_inv 0.159154943091895335768883763372

// ── Pyramid quadrant addressing ────────────────────────────────────────────
vec2 lower_left (vec2 uv) { return fract(uv * 0.5); }
vec2 lower_right(vec2 uv) { return fract((uv - vec2(1., 0.)) * 0.5); }
vec2 upper_left (vec2 uv) { return fract((uv - vec2(0., 1.)) * 0.5); }
vec2 upper_right(vec2 uv) { return fract((uv - 1.) * 0.5); }

// ── Procedural noise (replaces the Shadertoy noise texture) ────────────────
float h21(vec2 p) {
    p = fract(p * vec2(123.34, 345.45));
    p += dot(p, p + 34.345);
    return fract(p.x * p.y);
}
vec4 noise4(vec2 c) {
    vec2 o = fract(vec2(42.0, 56.0) * TIME) * 64.0;
    vec2 p = c * 0.0173 + o;
    return vec4(h21(p), h21(p + 11.3), h21(p + 27.7), h21(p + 51.1));
}

// ── Pyramid samplers ───────────────────────────────────────────────────────
vec4 BlurA(vec2 uv, int level) {
    if (level <= 0) return texture(bufA, fract(uv));
    uv = upper_left(uv);
    for (int depth = 1; depth < 8; depth++) {
        if (depth >= level) break;
        uv = lower_right(uv);
    }
    return texture(bufD, uv);
}
vec4 BlurB(vec2 uv, int level) {
    if (level <= 0) return texture(bufB, fract(uv));
    uv = lower_left(uv);
    for (int depth = 1; depth < 8; depth++) {
        if (depth >= level) break;
        uv = lower_right(uv);
    }
    return texture(bufD, uv);
}
vec2 GradientA(vec2 uv, vec2 d, vec4 selector, int level) {
    vec4 dX = 0.5 * BlurA(uv + vec2(1., 0.) * d, level) - 0.5 * BlurA(uv - vec2(1., 0.) * d, level);
    vec4 dY = 0.5 * BlurA(uv + vec2(0., 1.) * d, level) - 0.5 * BlurA(uv - vec2(0., 1.) * d, level);
    return vec2(dot(dX, selector), dot(dY, selector));
}

// ── Complex math + autonomous vortex stir (replaces mouse) ─────────────────
vec2 complex_mul(vec2 a, vec2 b) { return vec2(a.x*b.x - a.y*b.y, a.x*b.y + a.y*b.x); }
float sigmoid(float x) { return 2./(1. + exp2(-x)) - 1.; }

vec2 stirPos() {                                   // normalized 0..1
    float t = TIME * stirSpeed;
    return vec2(0.5) + 0.30 * vec2(cos(t), sin(t * 1.43));
}
vec2 stirVel() {                                   // normalized velocity
    float t = TIME * stirSpeed;
    return 0.30 * stirSpeed * vec2(-sin(t), 1.43 * cos(t * 1.43));
}

float conetip(vec2 uv, vec2 pos, float size, float mn) {
    vec2 aspect = vec2(1., RENDERSIZE.y / RENDERSIZE.x);
    return max(mn, 1. - length((uv - pos) * aspect / size));
}
float warpFilter(vec2 uv, vec2 pos, float size, float ramp) {
    return 0.5 + sigmoid(conetip(uv, pos, size, -16.) * ramp) * 0.5;
}
vec2 vortex_warp(vec2 uv, vec2 pos, float size, float ramp, vec2 rot) {
    vec2 aspect = vec2(1., RENDERSIZE.y / RENDERSIZE.x);
    vec2 rot_uv = pos + complex_mul((uv - pos) * aspect, rot) / aspect;
    return mix(uv, rot_uv, warpFilter(uv, pos, size, ramp));
}
vec2 vortex_pair_warp(vec2 uv, vec2 pos, vec2 vel) {
    vec2 aspect = vec2(1., RENDERSIZE.y / RENDERSIZE.x);
    float ramp = 4.0, d = 0.125;
    float l = length(vel);
    vec2 p1 = pos, p2 = pos;
    if (l > 0.) {
        vec2 normal = normalize(vel.yx * vec2(-1., 1.)) / aspect;
        p1 = pos - normal * d / 2.;
        p2 = pos + normal * d / 2.;
    }
    float w = l / d * 2.;
    vec2 c1 = vortex_warp(uv, p1, d, ramp, vec2(cos(w),  sin(w)));
    vec2 c2 = vortex_warp(uv, p2, d, ramp, vec2(cos(-w), sin(-w)));
    return (c1 + c2) / 2.;
}

// ── Buffer C: resolution reduction + horizontal blur ───────────────────────
vec4 blur_horizontal(sampler2D channel, vec2 uv, float scale) {
    float h = scale / RENDERSIZE.x;
    vec4 s = vec4(0.0);
    s += texture(channel, fract(vec2(uv.x - 4.*h, uv.y))) * 0.05;
    s += texture(channel, fract(vec2(uv.x - 3.*h, uv.y))) * 0.09;
    s += texture(channel, fract(vec2(uv.x - 2.*h, uv.y))) * 0.12;
    s += texture(channel, fract(vec2(uv.x - 1.*h, uv.y))) * 0.15;
    s += texture(channel, fract(vec2(uv.x + 0.*h, uv.y))) * 0.16;
    s += texture(channel, fract(vec2(uv.x + 1.*h, uv.y))) * 0.15;
    s += texture(channel, fract(vec2(uv.x + 2.*h, uv.y))) * 0.12;
    s += texture(channel, fract(vec2(uv.x + 3.*h, uv.y))) * 0.09;
    s += texture(channel, fract(vec2(uv.x + 4.*h, uv.y))) * 0.05;
    return s / 0.98;
}
vec4 blur_horizontal_left_column(vec2 uv, int depth) {
    float h = pow(2., float(depth)) / RENDERSIZE.x;
    vec2 a[9];
    for (int i = 0; i < 9; i++) a[i] = fract(vec2(uv.x + float(i - 4) * h, uv.y) * 2.);
    if (uv.y > 0.5) { for (int i = 0; i < 9; i++) a[i] = upper_left(a[i]); }
    else            { for (int i = 0; i < 9; i++) a[i] = lower_left(a[i]); }
    for (int level = 0; level < 8; level++) {
        if (level >= depth) break;
        for (int i = 0; i < 9; i++) a[i] = lower_right(a[i]);
    }
    float w[9]; w[0]=0.05; w[1]=0.09; w[2]=0.12; w[3]=0.15; w[4]=0.16; w[5]=0.15; w[6]=0.12; w[7]=0.09; w[8]=0.05;
    vec4 s = vec4(0.0);
    for (int i = 0; i < 9; i++) s += texture(bufD, a[i]) * w[i];
    return s / 0.98;
}

// ── Buffer D: vertical blur ────────────────────────────────────────────────
vec4 blur_vertical(vec2 uvq, bool upper) {
    float v = 1. / RENDERSIZE.y;
    float w[9]; w[0]=0.05; w[1]=0.09; w[2]=0.12; w[3]=0.15; w[4]=0.16; w[5]=0.15; w[6]=0.12; w[7]=0.09; w[8]=0.05;
    vec4 s = vec4(0.0);
    for (int i = 0; i < 9; i++) {
        vec2 c = vec2(uvq.x, uvq.y + float(i - 4) * v);
        c = upper ? upper_left(c) : lower_left(c);
        s += texture(bufC, c) * w[i];
    }
    return s / 0.98;
}
vec4 blur_vertical_left_column(vec2 uv, int depth) {
    float v = pow(2., float(depth)) / RENDERSIZE.y;
    vec2 a[9];
    for (int i = 0; i < 9; i++) a[i] = fract(vec2(uv.x, uv.y + float(i - 4) * v) * 2.);
    if (uv.y > 0.5) { for (int i = 0; i < 9; i++) a[i] = upper_left(a[i]); }
    else            { for (int i = 0; i < 9; i++) a[i] = lower_left(a[i]); }
    for (int level = 0; level < 8; level++) {
        if (level > depth) break;
        for (int i = 0; i < 9; i++) a[i] = lower_right(a[i]);
    }
    float w[9]; w[0]=0.05; w[1]=0.09; w[2]=0.12; w[3]=0.15; w[4]=0.16; w[5]=0.15; w[6]=0.12; w[7]=0.09; w[8]=0.05;
    vec4 s = vec4(0.0);
    for (int i = 0; i < 9; i++) s += texture(bufC, a[i]) * w[i];
    return s / 0.98;
}

// ── Native audio bus (soft-kneed, idle-floor safe) ─────────────────────────
// Bass nudges the vortex stir strength — the dominant structural driver of
// this sim — so the fluid visibly surges with the low end. Kept modest and
// applied identically every frame (deterministic, no feedback-loop shock).
float audioStirMod() {
    float aReact = clamp(audioReact, 0.0, 2.0);
    // Low floor + sub coupling: hiphop's sparse sub-heavy kicks and jazz's
    // soft accents must move the stir too, not just loud full-band bass.
    float aBassP = pow(smoothstep(0.03, 0.85, max(audioBass, 0.9 * audioSub)), 1.3);
    return 1.0 + 0.55 * aReact * aBassP;
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 pixelSize = 1. / RENDERSIZE.xy;
    vec2 aspect = vec2(1., RENDERSIZE.y / RENDERSIZE.x);
    vec4 fragColor = vec4(0.0);
    float stirMod = audioStirMod();

    // ───────────────────────── Buffer A — reaction-diffusion ───────────────
    if (PASSINDEX == 0) {
        vec4 noise = (noise4(gl_FragCoord.xy) - 0.5) * 2.;
        if (FRAMEINDEX < 10) { gl_FragColor = noise; return; }

        vec2 mouseV = stirVel();
        uv = vortex_pair_warp(uv, stirPos(), mouseV * aspect * stirStrength * stirMod);

        vec2 gLook = pixelSize * 3.;
        float expansion = 1.0;
        float differentialFactor = 12. / 256.;
        float increment = -3. / 256.;
        float noiseFactor = 12. / 256.;
        float feedBack = 6. / 256.;
        float feedForward = 6. / 256.;

        fragColor.r = BlurA(uv + GradientA(uv, gLook, vec4( 4., 0.,-2., 0.), 1) * pixelSize * expansion, 0).r;
        fragColor.g = BlurA(uv + GradientA(uv, gLook, vec4( 0., 4., 0.,-2.), 1) * pixelSize * expansion, 0).g;
        fragColor.b = BlurA(uv + GradientA(uv, gLook, vec4(-2., 0., 4., 0.), 1) * pixelSize * expansion, 0).b;
        fragColor.a = BlurA(uv + GradientA(uv, gLook, vec4( 0.,-2., 0., 4.), 1) * pixelSize * expansion, 0).a;

        fragColor += (BlurA(uv, 1) - BlurA(uv, 2)) * differentialFactor;
        fragColor += increment + noise * noiseFactor;
        fragColor -= fragColor.argb * feedBack;
        fragColor += fragColor.gbar * feedForward;
        gl_FragColor = clamp(fragColor, 0., 1.);
        return;
    }

    // ───────────────────────── Buffer B — diffusion + drift ────────────────
    if (PASSINDEX == 1) {
        vec4 noise = noise4(gl_FragCoord.xy);
        if (FRAMEINDEX < 10) { gl_FragColor = noise; return; }

        uv = 0.5 + (uv - 0.5) * 0.99;
        vec2 mouseV = stirVel();
        uv = vortex_pair_warp(uv, stirPos(), mouseV * aspect * stirStrength * stirMod);

        float time = float(FRAMEINDEX) / 60.;
        uv += vec2(sin(time*0.1 + uv.x*2. + 1.) - sin(time*0.214 + uv.y*2. + 1.),
                   sin(time*0.168 + uv.x*2. + 1.) - sin(time*0.115 + uv.y*2. + 1.)) * pixelSize * 1.5;

        fragColor  = BlurB(uv, 0);
        fragColor += (BlurB(uv, 1) - BlurB(uv, 2)) * 0.5 + (noise - 0.5) * 0.004;
        gl_FragColor = clamp(fragColor, 0., 1.);
        return;
    }

    // ───────────────────────── Buffer C — horizontal blur / pyramid ────────
    if (PASSINDEX == 2) {
        if (uv.x < 0.5) {
            vec2 uvh = fract(uv * 2.);
            fragColor = (uv.y > 0.5) ? blur_horizontal(bufA, uvh, 1.)
                                     : blur_horizontal(bufB, uvh, 1.);
        } else {
            for (int level = 0; level < 8; level++) {
                if ((uv.x > 0.5 && uv.y > 0.5) || (uv.x <= 0.5)) break;
                vec2 uvh = fract(uv * 2.);
                fragColor = blur_horizontal_left_column(uvh, level);
                uv = uvh;
            }
        }
        gl_FragColor = fragColor;
        return;
    }

    // ───────────────────────── Buffer D — vertical blur / pyramid ──────────
    if (PASSINDEX == 3) {
        if (uv.x < 0.5) {
            vec2 uvh = fract(uv * 2.);
            fragColor = blur_vertical(uvh, uv.y > 0.5);
        } else {
            for (int level = 0; level < 8; level++) {
                if ((uv.x > 0.5 && uv.y >= 0.5) || (uv.x < 0.5)) break;
                vec2 uvh = fract(uv * 2.);
                fragColor = blur_vertical_left_column(uvh, level);
                uv = uvh;
            }
        }
        gl_FragColor = fragColor;
        return;
    }

    // ───────────────────────── Image — composite ───────────────────────────
    vec2 d = pixelSize * 2.;
    vec4 dx = (BlurA(uv + vec2(1,0)*d, 1) - BlurA(uv - vec2(1,0)*d, 1)) * 0.5;
    vec4 dy = (BlurA(uv + vec2(0,1)*d, 1) - BlurA(uv - vec2(0,1)*d, 1)) * 0.5;
    d = pixelSize;
    dx += BlurA(uv + vec2(1,0)*d, 0) - BlurA(uv - vec2(1,0)*d, 0);
    dy += BlurA(uv + vec2(0,1)*d, 0) - BlurA(uv - vec2(0,1)*d, 0);

    fragColor = BlurA(uv + vec2(dx.x,dy.x)*pixelSize*8., 0).x * vec4(0.7,1.66,2.0,1.0) - vec4(0.3,1.0,1.0,1.0);
    fragColor = mix(fragColor, vec4(8.0,6.,2.,1.),
                    BlurA(uv + vec2(dx.x,dy.x)*vec2(0.5), 3).y*0.4*0.75*vec4(1.-BlurA(uv+vec2(dx.x,dy.x)*pixelSize*8.,0).x));
    fragColor = mix(fragColor, tintLow,
                    BlurA(uv, 1).a*length(GradientA(uv, pixelSize*2., vec4(0.,0.,0.,1.), 0))*5.);
    fragColor = mix(fragColor, vec4(1.25,1.35,1.4,0.),
                    BlurA(uv, 0).x*BlurA(uv + GradientA(uv, pixelSize*2.5, vec4(-256.,32.,-128.,32.), 1)*pixelSize, 2).y);
    fragColor = mix(fragColor, tintHigh,
                    BlurA(uv, 1).x*length(GradientA(uv+GradientA(uv, pixelSize*2., vec4(0.,0.,128.,0.), 1)*pixelSize, pixelSize*2., vec4(0.,0.,0.,1.), 0))*5.);
    fragColor = mix(fragColor, vec4(1.,1.25,1.5,0.),
                    0.5*(1.-BlurA(uv, 0)*1.).a*length(GradientA(uv+GradientA(uv, pixelSize*2., vec4(0.,128.,0.,0.), 1)*pixelSize, pixelSize*1.5, vec4(0.,0.,16.,0.), 0)));

    // ─── Native audio bus (soft-kneed, idle-floor safe) ───────────────────
    float aReact = clamp(audioReact, 0.0, 2.0);
    // Low knee floor (0.03) + sub coupling so sparse hiphop subs and soft
    // jazz kicks register; gentler pow keeps small hits visible.
    float aBassP = pow(smoothstep(0.03, 0.85, max(audioBass, 0.9 * audioSub)), 1.3);
    float aMidP  = pow(smoothstep(0.04, 0.85, audioMid), 1.2);
    float aHighP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float aBeat  = audioBeatPulse * audioBeatPulse;
    // Decaying hit trace (beat pulse OR punch) — sparse hits leave a mark.
    float aHit   = max(aBeat, pow(clamp(audioPunch, 0.0, 1.0), 1.5));

    vec3 outCol = fragColor.rgb * exposure;
    // The surface runs highlight-hot by design (many channels already near
    // full white), so a brightening push has nowhere to go — instead bass
    // pulls the surface into a cooler, deeper "inhale" that reads clearly
    // even against the blown-out highlights, then releases on the exhale.
    // Hits join the same pull-down channel as a decaying darkening trace,
    // which actually reads on a clipped-bright surface (additive flash can't).
    outCol *= mix(vec3(1.0), vec3(0.10, 0.13, 0.22), clamp(aReact * (aBassP * 1.4 + 0.8 * aHit), 0.0, 1.0));
    // Jazz's walking mids: continuous cool pull following the mid band.
    outCol = mix(outCol, outCol * vec3(0.72, 0.70, 0.85), 0.45 * aReact * aMidP);
    // Highs -> fine surface sparkle: a cool desaturating pull on a thin band,
    // same reasoning — pulls DOWN off the clipped ceiling so it reads.
    outCol = mix(outCol, outCol * vec3(0.60, 0.70, 0.88), 0.6 * aReact * aHighP);
    // Beat -> a decaying warm flash across the whole surface, never a strobe.
    outCol += vec3(1.0, 0.85, 0.65) * aBeat * aReact * 1.6;

    // Optional texture — refracted through the same RD surface gradient that
    // drives the highlight streaks, so it reads as liquid over the artwork
    // rather than a flat overlay.
    if (texMix > 0.001) {
        vec2 texUV = fract(uv + vec2(dx.x, dy.x) * pixelSize * 24.0);
        vec3 texCol = texture2D(inputTex, texUV).rgb;
        vec3 refracted = mix(texCol * (0.5 + 0.5 * outCol), outCol * texCol * 1.3, 0.5);
        outCol = mix(outCol, refracted, texMix);
    }

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
