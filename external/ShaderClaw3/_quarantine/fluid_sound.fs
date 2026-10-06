/*{
  "DESCRIPTION": "Fluid Sound — sound is the brush. A self-advecting fluid field (Fluid Sim) where audio energy births blobs from the center, shaded as a lit volumetric metaball gel (3D Fluid). Quiet = dim + settle to middle + fade; loud = blobs erupt and flow. Core audio bus only, so it runs in both Easel and ShaderClaw3.",
  "CREDIT": "ShaderClaw",
  "CATEGORIES": [
    "Generator",
    "Simulation",
    "3D"
  ],
  "INPUTS": [
    {
      "NAME": "specAmount",
      "LABEL": "Specular",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 3
    },
    {
      "NAME": "specPow",
      "LABEL": "Spec Sharpness",
      "TYPE": "float",
      "DEFAULT": 28,
      "MIN": 4,
      "MAX": 96
    },
    {
      "NAME": "glow",
      "LABEL": "Inner Glow",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 2
    },
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
      "NAME": "emitterSpread",
      "LABEL": "Spread",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "blobRadius",
      "LABEL": "Blob Size",
      "TYPE": "float",
      "DEFAULT": 0.1,
      "MIN": 0.03,
      "MAX": 0.3,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "surfaceDepth",
      "LABEL": "Surface Depth",
      "TYPE": "float",
      "DEFAULT": 1.6,
      "MIN": 0.2,
      "MAX": 5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "birth",
      "LABEL": "Blob Birth",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "flowSpeed",
      "LABEL": "Flow Speed",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "swirl",
      "LABEL": "Swirl",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "settle",
      "LABEL": "Settle (quiet)",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "specTint",
      "LABEL": "Spec Tint",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "saturation",
      "LABEL": "Saturation",
      "TYPE": "float",
      "DEFAULT": 1.35,
      "MIN": 0,
      "MAX": 2.5,
      "GROUP": "Color"
    },
    {
      "NAME": "hueSpan",
      "LABEL": "Hue Span",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "colorDrift",
      "LABEL": "Color Drift",
      "TYPE": "float",
      "DEFAULT": 0.06,
      "MIN": 0,
      "MAX": 0.5,
      "GROUP": "Color"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": true,
      "GROUP": "Background"
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
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2.5,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "dimFloor",
      "LABEL": "Quiet Dimness",
      "TYPE": "float",
      "DEFAULT": 0.1,
      "MIN": 0,
      "MAX": 0.5,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "velBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "dyeBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

// ============================================================================
// FLUID SOUND
//   Essence fusion (not an overlay):
//     • Fluid Sim  → a real persistent, self-advecting velocity+dye field. The
//                    fluid has memory: dye smears, curls, and flows. Motion is
//                    emergent from the velocity field, not a per-frame redraw.
//     • 3D Fluid   → the accumulated dye is treated as a volumetric height
//                    field and shaded as lit metaball gel: surface normal from
//                    the density gradient, lambert + Blinn specular + fresnel
//                    rim + a back-lit volume layer, so blobs read as 3D matter.
//   The new idea that fuses them: SOUND IS THE BRUSH. In a fluid sim you inject
//   dye/force with a pointer. Here the audio feature bus is the pointer — energy
//   births blobs at center-anchored emitters, the bass "kick" gives them an
//   outward impulse, and the whole field settles inward + dims when it goes
//   quiet. Everything is derived from the CORE audio bus (audioLevel/Bass/Mid/
//   High/audioFFT) so it compiles in both Easel and ShaderClaw3's web runtime.
// ============================================================================

#define PI 3.14159265
#define NE 3                 // number of sound emitters

float hash11(float p){
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

vec3 hsv2rgb(vec3 c){
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

// Velocity is stored encoded into [0,1] (0.5 == zero) so it survives an 8-bit
// or float persistent buffer the same way Pavel/cfd_paint do it.
vec2 decodeVel(vec4 t){ return (t.xy - 0.5); }
vec4 encodeVel(vec2 v){ return vec4(clamp(v, -0.49, 0.49) + 0.5, 0.5, 1.0); }

// One FFT bin (core sampler — bound in both runtimes). Safe if the texture is
// flat: it just contributes nothing and the band scalars drive everything.
float fftBand(float f){ return texture2D(audioFFT, vec2(clamp(f, 0.0, 1.0), 0.5)).r; }

// Center-anchored emitter. Always lives near the middle; its orbit radius grows
// with energy (Spread), and it drifts slowly so activity wanders "a little bit"
// without flying around the canvas. Mirrors the tuned native Sound movement.
vec2 emitterPos(int i, float energy, float t){
    float fi = float(i);
    float ph = fi * 2.39963;                         // golden angular spacing
    float r  = (0.015 + 0.13 * emitterSpread * (0.30 + 1.5 * energy))
             * (0.55 + 0.45 * sin(t * 0.50 + ph));
    float a  = t * 0.33 + ph * 2.0 + 0.8 * sin(t * 0.23 + ph * 1.7);
    return vec2(0.5) + vec2(cos(a), sin(a)) * r;
}

// Spectral palette built only from core bands — low/warm → high/cool, with a
// slow hue drift. No dependency on the Easel-only audioPalette().
vec3 palette(float t, float warmth, float driftT){
    t = clamp(t, 0.0, 1.0);
    // Wider hue sweep (hueSpan) so the three emitters span a real spread of
    // color instead of washing into one tone. Stays rich at the top end.
    float hue = fract(0.60 + hueShift + (0.30 + 0.7 * hueSpan) * t
                      - 0.16 * warmth + driftT);
    float sat = mix(0.95, 0.78, t);
    float val = 1.0;
    return hsv2rgb(vec3(hue, sat, val));
}

// Push chroma without changing brightness (s>1 = more saturated).
vec3 boostSat(vec3 c, float s){
    float l = dot(c, vec3(0.2126, 0.7152, 0.0722));
    return max(mix(vec3(l), c, s), 0.0);
}

void main(){
    vec2  R      = RENDERSIZE;
    vec2  uv     = isf_FragNormCoord;
    float aspect = R.x / max(R.y, 1.0);
    vec2  texel  = 1.0 / R;

    // ---- Audio features (CORE bus only → cross-runtime) --------------------
    float react  = audioReact;
    float lvl    = clamp(react * audioLevel, 0.0, 2.0);
    float bass   = clamp(react * audioBass,  0.0, 2.0);
    float mid    = clamp(react * audioMid,   0.0, 2.0);
    float high   = clamp(react * audioHigh,  0.0, 2.0);
    // Energy = weighted loudness. The persistent buffers integrate this over
    // time, so density/brightness SWELL smoothly even though the drive is
    // instantaneous — no per-beat throb.
    float energy = clamp(0.55 * bass + 0.30 * mid + 0.20 * high + 0.12 * lvl, 0.0, 1.4);
    float quiet  = 1.0 - smoothstep(0.03, 0.30, max(lvl, energy));
    float beat   = smoothstep(0.40, 0.95, bass);     // bass transient "kick"
    float warmth = clamp(bass - high, -1.0, 1.0);    // spectral tilt for color
    float t      = TIME;
    float driftT = TIME * colorDrift;

    // ========================================================================
    // PASS 0 — VELOCITY FIELD (self-advecting, audio-forced)
    // ========================================================================
    if (PASSINDEX == 0){
        if (FRAMEINDEX < 1){ gl_FragColor = encodeVel(vec2(0.0)); return; }

        // Semi-Lagrangian self-advection: pull velocity from where this parcel
        // came from. Clamp (don't wrap) so the edges stay calm.
        vec2 v0  = decodeVel(texture2D(velBuf, uv));
        vec2 src = clamp(uv - v0 * (flowSpeed * 0.030), 0.001, 0.999);
        vec2 v   = decodeVel(texture2D(velBuf, src));

        // Light vorticity confinement — sample curl of neighbours and nudge
        // perpendicular, which keeps the fluid swirling instead of going radial
        // and dead. This is the "fluid feel".
        vec2 e = texel;
        float cL = decodeVel(texture2D(velBuf, uv - vec2(e.x, 0.0))).y;
        float cR = decodeVel(texture2D(velBuf, uv + vec2(e.x, 0.0))).y;
        float cD = decodeVel(texture2D(velBuf, uv - vec2(0.0, e.y))).x;
        float cU = decodeVel(texture2D(velBuf, uv + vec2(0.0, e.y))).x;
        float curl = (cR - cL) - (cU - cD);
        v += vec2(-(cU - cD), (cR - cL)) * curl * swirl * 0.25;

        // Sound emitters inject force: a gentle outward push (blobs are born and
        // breathe outward) plus a tangential swirl. Strength tracks energy, with
        // a small bass-kick accent. Kept moderate on purpose (less movement).
        for (int i = 0; i < NE; i++){
            vec2  ep = emitterPos(i, energy, t);
            vec2  d  = uv - ep; d.x *= aspect;
            float rr = blobRadius * (0.8 + 0.5 * hash11(float(i) + 3.0));
            float g  = exp(-dot(d, d) / (rr * rr));
            vec2  dir = normalize(uv - ep + 1e-5);
            float push = (0.010 + 0.075 * energy + 0.055 * beat) * flowSpeed;
            v += dir * push * g;
            v += vec2(-dir.y, dir.x) * (0.045 * swirl * (0.4 + energy)) * g;
        }

        // When it goes quiet, gravitate the whole field back toward the middle
        // and damp harder so motion eases out instead of drifting forever.
        v += (vec2(0.5) - uv) * (settle * 0.045 * quiet);
        v *= mix(0.985, 0.930, quiet);

        gl_FragColor = encodeVel(v);
        return;
    }

    // ========================================================================
    // PASS 1 — DYE / DENSITY (advected by velocity, born from sound)
    // ========================================================================
    if (PASSINDEX == 1){
        if (FRAMEINDEX < 1){ gl_FragColor = vec4(0.0); return; }

        vec2 v   = decodeVel(texture2D(velBuf, uv));
        vec2 src = clamp(uv - v * (flowSpeed * 0.030), 0.001, 0.999);
        vec4 dye = texture2D(dyeBuf, src);

        // Dissipate — faster when quiet so the blobs fade away to dim/nothing,
        // slower when loud so dense matter persists and flows.
        dye *= mix(0.992, 0.952, quiet);

        // Birth blobs at the emitters. Amount is a small idle baseline (so the
        // field always feels alive) + energy-driven swell + a bass-kick accent.
        for (int i = 0; i < NE; i++){
            vec2  ep = emitterPos(i, energy, t);
            float band = (i == 0) ? bass : (i == 1) ? mid : high;  // each emitter owns a band
            float fb  = fftBand(0.06 + 0.40 * float(i));           // spectral spice (safe if flat)
            vec2  d   = uv - ep; d.x *= aspect;
            float rr  = blobRadius * (0.7 + 0.55 * band + 0.4 * fb);
            float g   = exp(-dot(d, d) / max(rr * rr, 1e-4));

            float tcol = clamp(0.10 + 0.30 * float(i) + 0.55 * (high - bass) + 0.20 * band, 0.0, 1.0);
            vec3  col  = palette(tcol, warmth, driftT);
            float amt  = (0.006 + birth * (0.16 * energy + 0.10 * band) + 0.6 * beat * 0.04) * g;

            dye.rgb += col * amt;
            dye.a   += amt;
        }

        gl_FragColor = clamp(dye, 0.0, 8.0);
        return;
    }

    // ========================================================================
    // PASS 2 — VOLUMETRIC SHADING (3D gel from the density field)
    // ========================================================================
    vec2  e   = texel * 1.5;
    vec4  c0  = texture2D(dyeBuf, uv);
    float d   = c0.a;

    // Surface normal from the density gradient — the dye field is the height
    // field of a thick liquid. surfaceDepth controls how "raised" blobs read.
    float hL = texture2D(dyeBuf, uv - vec2(e.x, 0.0)).a;
    float hR = texture2D(dyeBuf, uv + vec2(e.x, 0.0)).a;
    float hD = texture2D(dyeBuf, uv - vec2(0.0, e.y)).a;
    float hU = texture2D(dyeBuf, uv + vec2(0.0, e.y)).a;
    vec3  N  = normalize(vec3((hL - hR) * surfaceDepth, (hD - hU) * surfaceDepth, 1.0));

    vec3  L = normalize(vec3(0.45, 0.60, 0.75));
    vec3  V = vec3(0.0, 0.0, 1.0);
    vec3  H = normalize(L + V);
    float diff = max(dot(N, L), 0.0);
    float spec = pow(max(dot(N, H), 0.0), specPow) * specAmount;
    float fres = pow(1.0 - max(N.z, 0.0), 3.0);            // rim / edge light

    // Base colour, un-premultiplied so dense regions aren't blown out.
    vec3 base = c0.rgb / max(d, 1e-3);
    vec3 rim  = palette(0.85, warmth, driftT);

    vec3 col = base * (0.22 + 0.85 * diff);
    // Tinted specular — a colored highlight keeps the blob's hue instead of
    // blowing out to pure white. specTint=1 → fully colored, 0 → white.
    vec3 specCol = mix(vec3(1.0), base, specTint);
    col += specCol * spec;
    col += rim * fres * (0.35 + 0.65 * high);

    // Back-lit volume layer: sample the field offset along the normal so light
    // appears to pass through the gel — the depth cue that sells "3D fluid".
    float dBack = texture2D(dyeBuf, uv + N.xy * 0.02).a;
    col += c0.rgb * pow(max(dBack, 0.0), 1.3) * glow;

    // Coverage / shape, and a denser-interior darkening for thickness.
    float cov = smoothstep(0.02, 0.55, d);
    col *= mix(1.0, 0.82, smoothstep(0.6, 2.2, d));
    col *= cov;

    // Master brightness — dim toward near-black when quiet, full when loud.
    float bright = mix(dimFloor, 1.0, smoothstep(0.0, 0.45, max(lvl, energy)));
    col *= bright;

    // Hue-preserving highlight rolloff: compress only the LUMINANCE so bright
    // overlaps stop clipping to white and keep their color, then push chroma.
    float Lum = dot(col, vec3(0.2126, 0.7152, 0.0722));
    float Lc  = Lum / (1.0 + Lum);
    col *= Lc / max(Lum, 1e-4);
    col = boostSat(col, saturation);

    // Soft vignette so the volume sits in space.
    vec2  q = uv - 0.5;
    col *= 1.0 - 0.35 * dot(q, q);

    if (texMix > 0.001) {
        // Pour the texture into the gel: refract the lookup through the same
        // surface normal used for the back-lit volume sample, then modulate
        // (not crossfade) so it reads as etched into the fluid's density,
        // strongest where the blobs actually have coverage.
        vec2 texUV = clamp(uv + N.xy * 0.06, 0.0, 1.0);
        vec3 texCol = texture2D(inputTex, texUV).rgb;
        vec3 modCol = col * mix(vec3(1.0), texCol * 1.6, cov);
        col = mix(col, modCol, texMix);
    }

    // ---- universal background (defaults = no-op; hue/saturation already
    // ---- covered by the existing hueShift/saturation inputs) ----
    if (transparentBg){
        float a = cov * mix(0.6, 1.0, bright);
        if (bgColor.a > 0.0) {                    // fill transparent bg region
            col = mix(col, bgColor.rgb, (1.0 - a) * bgColor.a);
            a   = a + (1.0 - a) * bgColor.a;
        }
        gl_FragColor = vec4(col, a);
    } else {
        vec3 bg = vec3(0.015, 0.018, 0.026) * bright;
        bg = mix(bg, bgColor.rgb, bgColor.a);     // blend the void toward bgColor
        gl_FragColor = vec4(col + bg * (1.0 - cov), 1.0);
    }
}
