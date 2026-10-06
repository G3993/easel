/*{
  "DESCRIPTION": "Computational Fluid Dynamics — multi-buffer rotational self-advection with mouse/touch painting",
  "CATEGORIES": [
    "Generator",
    "Simulation"
  ],
  "INPUTS": [
    {
      "NAME": "inputTex",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "brushForce",
      "LABEL": "Brush Force",
      "TYPE": "float",
      "DEFAULT": 3,
      "MIN": 0.5,
      "MAX": 10
    },
    {
      "NAME": "specAmount",
      "LABEL": "Specular",
      "TYPE": "float",
      "DEFAULT": 2.5,
      "MIN": 0,
      "MAX": 8
    },
    {
      "NAME": "specPow",
      "LABEL": "Spec Sharpness",
      "TYPE": "float",
      "DEFAULT": 36,
      "MIN": 4,
      "MAX": 128
    },
    {
      "NAME": "diffMin",
      "LABEL": "Shadow",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "sourceBlend",
      "LABEL": "Source Blend",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "blendMode",
      "LABEL": "Blend Mode",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3,
        4
      ],
      "LABELS": [
        "Warp",
        "Dissolve",
        "Luma Map",
        "Edge Reveal",
        "Chromatic"
      ]
    },
    {
      "NAME": "texScale",
      "LABEL": "Tex Scale",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.1,
      "MAX": 5
    },
    {
      "NAME": "brushSize",
      "LABEL": "Brush Size",
      "TYPE": "float",
      "DEFAULT": 0.06,
      "MIN": 0.01,
      "MAX": 0.2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "fluidHeight",
      "LABEL": "Surface Height",
      "TYPE": "float",
      "DEFAULT": 150,
      "MIN": 1,
      "MAX": 500,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "fluidSpeed",
      "LABEL": "Fluid Speed",
      "TYPE": "float",
      "DEFAULT": 1.5,
      "MIN": 0.1,
      "MAX": 8,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "viscosity",
      "LABEL": "Viscosity",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "movePattern",
      "LABEL": "Move Pattern",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3,
        4
      ],
      "LABELS": [
        "Freeform",
        "Center",
        "Wave",
        "Vortex",
        "Pulse"
      ],
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "moveSpeed",
      "LABEL": "Move Speed",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0.1,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "moveSpread",
      "LABEL": "Move Spread",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "moveIntensity",
      "LABEL": "Move Intensity",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "swirl",
      "LABEL": "Global Swirl",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "pathOrbit",
      "LABEL": "Path Orbit",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "currentStrength",
      "LABEL": "Current",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "currentAngle",
      "LABEL": "Current Angle",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "colorSat",
      "LABEL": "Saturation",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Color"
    },
    {
      "NAME": "colorCycle",
      "LABEL": "Color Cycle",
      "TYPE": "float",
      "DEFAULT": 0.15,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "colorFloor",
      "LABEL": "Color Floor",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 0.5,
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
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": true,
      "GROUP": "Background"
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

// Rotational fluid simulation — optimized for ultra-wide (8000x1800)
// Pass 0: Velocity field (self-advecting rotational curl)
// Pass 1: Dye/color field (advected by velocity)
// Pass 2: Final output with specular lighting + source blending

#define PI 3.14159265
// Rotation angle for 3-way triangular stencil
#define ANG 2.0943951 // 2*PI/3

float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

// Aspect-correct texture UV -- "contain" mode (native aspect, no squeeze)
vec2 texUV(vec2 coord, float canvasAspect) {
    vec2 st = coord - 0.5;
    float texAspect = IMG_SIZE_inputTex.x / max(IMG_SIZE_inputTex.y, 1.0);
    float ratio = canvasAspect / max(texAspect, 0.001);
    // Contain: texture fits inside canvas at native aspect
    if (ratio > 1.0) st.x *= ratio;   // canvas wider than tex -- expand X
    else             st.y /= ratio;   // canvas taller than tex -- expand Y
    // Scale: 1.0 = fit, >1 = zoom in, <1 = zoom out (tiles)
    st /= texScale;
    st += 0.5;
    return fract(st);
}

vec4 sampleTex(vec2 coord, float canvasAspect) {
    return texture2D(inputTex, texUV(coord, canvasAspect));
}

// Path Orbit: the splat choreography revolves about the canvas centre.
// Returns the displacement to ADD to (uv - splatPos) so the splat sits at the
// orbited position; exactly vec2(0) at the default, so the sim is unchanged.
vec2 orbitShift(vec2 sp, float t, float aspect) {
    if (abs(pathOrbit) < 0.0001) return vec2(0.0);
    float oa = t * pathOrbit * 0.5;
    vec2 od = sp - 0.5; od.x *= aspect;
    vec2 rd = vec2(od.x * cos(oa) - od.y * sin(oa), od.x * sin(oa) + od.y * cos(oa));
    vec2 diff = od - rd; diff.x /= aspect;
    return diff;
}

void main() {
    vec2 Res = RENDERSIZE;
    vec2 pos = gl_FragCoord.xy;
    vec2 uv = isf_FragNormCoord;
    float aspect = Res.x / Res.y;
    vec2 invRes = 1.0 / Res; // precompute — avoid per-sample division

    // Soft-knee audio conditioning (playbook) — shared by all passes.
    // Audio injects force into the sim: energy floors it, bass kicks it,
    // punch adds a decaying accent. The fluid's own dynamics do the rest.
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
    float midP  = smoothstep(0.08, 0.85, audioMid);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float drive = 0.25 + 0.75 * smoothstep(0.05, 0.9, audioEnergy);
    float audioGain = 0.9 + 0.4 * (drive - 0.25) + 0.3 * bassP + 0.25 * audioPunch;
    // Fast direct-path envelopes: the fluid integrates injected force, so a
    // force-only response lags and diffuses — it correlates with slow swells
    // (ambient) but not with beats (edm/rock/jazz were deaf). These drive
    // instantaneous, screen-visible params below. Low knee so jazz's soft
    // 0.4-amp kicks read; headroom top so edm keeps breathing.
    float bassF  = pow(smoothstep(0.03, 0.70, audioBass), 1.3);
    float punchP = pow(clamp(audioPunch, 0.0, 1.0), 1.5);

    // ===== PASS 0: Velocity field =====
    if (PASSINDEX == 0) {
        // Precompute trig for the 3-way rotation (constant per frame)
        float ca = cos(ANG), sa = sin(ANG);
        float cah = cos(ANG * 0.5), sah = sin(ANG * 0.5);

        float rnd = hash21(vec2(float(FRAMEINDEX), 0.37));
        vec2 b = vec2(cos(ANG * rnd), sin(ANG * rnd));

        vec2 v = vec2(0.0);
        // At 8000x1800, cap octaves aggressively — fine detail is invisible
        float maxR2 = min(Res.y * Res.y * 0.25, 4000.0);

        // 5 octaves, 3 reads each (diagonal-only sampling)
        // Instead of 3×3=9 reads per octave, sample only d0+d0, d1+d1, d2+d2
        // This keeps the triangular curl pattern at 1/3 the texture cost
        for (int level = 0; level < 5; level++) {
            float bb = dot(b, b);
            if (bb > maxR2) break;

            float invBB = 1.0 / bb;

            // 3 rotated directions
            vec2 d0 = b;
            vec2 d1 = vec2(ca * b.x - sa * b.y, sa * b.x + ca * b.y);
            vec2 d2 = vec2(ca * b.x + sa * b.y, -sa * b.x + ca * b.y);

            // Half-step probes for curl estimation
            vec2 h0 = vec2(cah * b.x - sah * b.y, sah * b.x + cah * b.y);
            vec2 h1 = vec2(ca * h0.x - sa * h0.y, sa * h0.x + ca * h0.y);
            vec2 h2 = vec2(ca * h0.x + sa * h0.y, -sa * h0.x + ca * h0.y);

            // Diagonal samples only: 3 reads instead of 9
            vec2 s0 = texture2D(velBuf, fract((pos + d0 + d0) * invRes)).xy - 0.5;
            vec2 s1 = texture2D(velBuf, fract((pos + d1 + d1) * invRes)).xy - 0.5;
            vec2 s2 = texture2D(velBuf, fract((pos + d2 + d2) * invRes)).xy - 0.5;

            // Curl from diagonal terms — multiply by 3 to compensate for missing cross-terms
            v += d0.yx * dot(s0, h0) * invBB * 3.0;
            v += d1.yx * dot(s1, h1) * invBB * 3.0;
            v += d2.yx * dot(s2, h2) * invBB * 3.0;

            b *= 2.0;
        }

        // Self-advect velocity
        vec2 advUV = fract((pos + v * vec2(-1.0, 1.0) * fluidSpeed) * invRes);
        vec4 vel = texture2D(velBuf, advUV);

        // Viscosity damping
        vel.xy = mix(vel.xy, vec2(0.5), viscosity * 0.01);

        // Mouse/touch/hand injection
        float interacting = max(mouseDown, pinchHold);
        if (interacting > 0.3) {
            vec2 mDiff = uv - mousePos;
            mDiff.x *= aspect;
            float dist2 = dot(mDiff, mDiff);
            float r2 = brushSize * brushSize;
            if (dist2 < r2 * 12.0) {
                float falloff = exp(-dist2 / r2);
                vec2 force = mouseDelta * brushForce * interacting;
                if (dot(force, force) < 0.000001) {
                    force = normalize(mDiff + 0.001) * 0.02 * interacting;
                }
                force = clamp(force, vec2(-0.3), vec2(0.3));
                vel.xy += force * falloff;
                vel.xy = clamp(vel.xy, 0.0, 1.0);
            }
        }

        // ---- Movement patterns ----
        float t = TIME * moveSpeed;
        float spread = mix(0.05, 0.42, moveSpread);
        float intensity = moveIntensity * 0.2 * audioGain;      // audio drives injection force
        float splatR = brushSize * 2.5 * (1.0 + 0.15 * midP);   // mids widen the splats
        float splatR2 = splatR * splatR;
        float cutoff2 = splatR2 * 12.0;

        if (movePattern < 0.5) {
            // Freeform — 3 wandering splats
            for (int s = 0; s < 3; s++) {
                float fs = float(s);
                float phase = t * (0.5 + fs * 0.3) + fs * 1.257;
                vec2 splatPos = vec2(
                    0.5 + spread * sin(phase) * cos(phase * 0.7 + fs),
                    0.5 + spread * cos(phase * 0.8) * sin(phase * 0.5 + fs * 1.5)
                );
                vec2 mDiff = uv - splatPos;
                mDiff += orbitShift(splatPos, t, aspect);
                mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                if (dist2 < cutoff2) {
                    vel.xy += vec2(
                        cos(phase * 1.3 + fs),
                        sin(phase * 0.9 + fs * 2.0)
                    ) * intensity * exp(-dist2 / splatR2);
                }
            }
        } else if (movePattern < 1.5) {
            // Center — 3 radial pulses + vortex
            for (int s = 0; s < 3; s++) {
                float fs = float(s);
                float phase = t * (0.8 + fs * 0.25) + fs * 1.257;
                float r = spread * 0.5 * (0.3 + 0.7 * abs(sin(phase * 0.5)));
                float a = phase * 0.7 + fs * 1.257;
                vec2 splatPos = vec2(0.5 + cos(a) * r, 0.5 + sin(a) * r);
                vec2 mDiff = uv - splatPos;
                mDiff += orbitShift(splatPos, t, aspect);
                mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                if (dist2 < cutoff2) {
                    vec2 dir = normalize(splatPos - 0.5 + 0.001);
                    vel.xy += dir * sin(phase * 1.5) * intensity * 1.5 * exp(-dist2 / splatR2);
                }
            }
            vec2 cDiff = uv - 0.5;
            cDiff.x *= aspect;
            float centralFalloff = exp(-dot(cDiff, cDiff) / (spread * spread * 0.3));
            vel.xy += vec2(-cDiff.y, cDiff.x) * sin(t * 0.6) * intensity * 0.5 * centralFalloff;
        } else if (movePattern < 2.5) {
            // Wave — 3 sweeping splats + global field
            for (int s = 0; s < 3; s++) {
                float fs = float(s);
                float wavePhase = t * 0.8 + fs * 0.7;
                vec2 splatPos = vec2(
                    0.5 + spread * sin(wavePhase * 0.6 + fs * 0.8),
                    0.5 + spread * 0.7 * sin(wavePhase * 1.1 + fs * 1.7)
                );
                vec2 mDiff = uv - splatPos;
                mDiff += orbitShift(splatPos, t, aspect);
                mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                float sr2 = brushSize * 2.2;
                if (dist2 < sr2 * sr2 * 12.0) {
                    vel.xy += vec2(
                        cos(wavePhase * 0.6 + fs * 0.8) * intensity * 1.2,
                        sin(wavePhase * 2.2 + fs) * intensity * 0.6
                    ) * exp(-dist2 / (sr2 * sr2));
                }
            }
            vel.xy += vec2(
                sin(uv.x * 8.0 + t * 1.5) * sin(uv.y * 6.0 - t * 0.8),
                cos(uv.x * 5.0 - t)
            ) * intensity * 0.15;
        } else if (movePattern < 3.5) {
            // Vortex — 3 spinning splats + global rotation
            for (int s = 0; s < 3; s++) {
                float fs = float(s);
                float vortexPhase = t * (0.6 + fs * 0.2);
                float r = spread * (0.15 + fs * 0.15);
                vec2 center = vec2(
                    0.5 + spread * 0.3 * sin(t * 0.3 + fs),
                    0.5 + spread * 0.3 * cos(t * 0.25 + fs * 1.5)
                );
                vec2 splatPos = center + vec2(cos(vortexPhase), sin(vortexPhase)) * r;
                vec2 mDiff = uv - splatPos;
                mDiff += orbitShift(splatPos, t, aspect);
                mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                float sr2 = brushSize * 1.8;
                if (dist2 < sr2 * sr2 * 12.0) {
                    vec2 radial = splatPos - center;
                    vel.xy += vec2(-radial.y, radial.x) * intensity * 2.0 / (r + 0.05) * exp(-dist2 / (sr2 * sr2));
                }
            }
            vec2 gDiff = uv - 0.5;
            gDiff.x *= aspect;
            float rotFalloff = exp(-dot(gDiff, gDiff) / (spread * spread));
            vel.xy += vec2(-gDiff.y, gDiff.x) * (sin(t * 0.4) > 0.0 ? 1.0 : -1.0) * intensity * 0.3 * rotFalloff;
        } else {
            // Pulse — 2 centers × 3 ring splats
            for (int s = 0; s < 2; s++) {
                float fs = float(s);
                float pulse = fract(t * 2.0 * moveSpeed * (0.3 + fs * 0.2) + fs * 0.5);
                float pulseR = pulse * spread * 1.5;
                vec2 center = vec2(
                    0.5 + spread * 0.4 * sin(t * 0.2 + fs * 2.094),
                    0.5 + spread * 0.4 * cos(t * 0.15 + fs * 2.094)
                );
                float strength = (1.0 - pulse) * intensity * 1.5;
                float sr2 = brushSize * (1.5 + pulse * 2.0);
                for (int a = 0; a < 3; a++) {
                    float angle = float(a) * 2.094 + fs * 0.5;
                    vec2 splatPos = center + vec2(cos(angle), sin(angle)) * pulseR;
                    vec2 mDiff = uv - splatPos;
                    mDiff += orbitShift(splatPos, t, aspect);
                    mDiff.x *= aspect;
                    float dist2 = dot(mDiff, mDiff);
                    if (dist2 < sr2 * sr2 * 12.0) {
                        vel.xy += normalize(splatPos - center + 0.001) * strength * exp(-dist2 / (sr2 * sr2));
                    }
                }
            }
        }

        // Global Swirl: whole-canvas rotational force (force-only, no buffer changes)
        if (abs(swirl) > 0.001) {
            vec2 sD = uv - 0.5; sD.x *= aspect;
            vel.xy += vec2(-sD.y, sD.x) * swirl * 0.004 * exp(-dot(sD, sD) * 1.5);
        }
        // Current: steady directional push across the whole field
        if (currentStrength > 0.001) {
            float ca2 = currentAngle * 6.2831853;
            vel.xy += vec2(cos(ca2), sin(ca2)) * currentStrength * 0.0008;
        }

        vel.xy = clamp(vel.xy, 0.0, 1.0);

        // Seed
        if (FRAMEINDEX < 3) {
            vel = vec4(0.5, 0.5, 0.0, 1.0);
            vec2 d = uv - 0.5;
            vel.xy = vec2(0.5 - d.y * 0.3, 0.5 + d.x * 0.3);
        }

        gl_FragColor = vel;
        return;
    }

    // ===== PASS 1: Dye/color field =====
    if (PASSINDEX == 1) {
        vec2 vel = (texture2D(velBuf, uv).xy - 0.5) * 2.0;
        // Bass surges the dye advection speed — existing motion accelerates
        // on the hit, so per-frame visual change tracks the envelope
        // directly (fast path; the force injection is the slow path).
        vec2 advUV = fract(uv - vel * fluidSpeed * 0.01 * (1.0 + 0.8 * bassF));
        vec4 dye = texture2D(dyeBuf, advUV);

        // Dissipation
        dye.rgb *= mix(0.999, 0.98, sourceBlend * sourceBlend);

        // Color cycle
        if (colorCycle > 0.001) {
            float cyc = colorCycle * 0.003;
            float hue = fract(TIME * colorCycle * 0.08 + uv.x * 0.15 + uv.y * 0.1);
            dye.rgb = mix(dye.rgb, dye.rgb * hsv2rgb(vec3(hue, 0.6, 1.0)), cyc);
        }

        // Color floor
        if (colorFloor > 0.001) {
            float lum = dot(dye.rgb, vec3(0.299, 0.587, 0.114));
            float boost = smoothstep(0.0, colorFloor, colorFloor - lum);
            dye.rgb += dye.rgb * boost * 0.5 + vec3(boost * colorFloor * 0.3);
        }

        bool hasInput = IMG_SIZE_inputTex.x > 0.0;

        // Source re-injection
        if (hasInput && sourceBlend > 0.001) {
            float reinjection = sourceBlend * 0.08 * (1.0 - smoothstep(0.0, 0.8, length(vel)));
            dye.rgb = mix(dye.rgb, sampleTex(uv, aspect).rgb, reinjection);
        }

        // Mouse/touch painting
        float dyeInteracting = max(mouseDown, pinchHold);
        if (dyeInteracting > 0.3) {
            vec2 mDiff = uv - mousePos;
            mDiff.x *= aspect;
            float dist2 = dot(mDiff, mDiff);
            float r2 = brushSize * brushSize;
            if (dist2 < r2 * 12.0) {
                float falloff = exp(-dist2 / r2) * dyeInteracting;
                if (hasInput) {
                    dye.rgb = mix(dye.rgb, sampleTex(mousePos, aspect).rgb, falloff * 0.8);
                } else {
                    dye.rgb = mix(dye.rgb, hsv2rgb(vec3(fract(TIME * 0.1 + hash21(mousePos * 100.0)), 0.8, 1.0)), falloff * 0.8);
                }
            }
        }

        // Movement color injection — 2 splats
        float t = TIME * moveSpeed;
        float spread = mix(0.05, 0.42, moveSpread);
        float splatR = brushSize * 2.5 * (1.0 + 0.15 * midP);   // match velocity-pass splat width
        float cutoff2 = splatR * splatR * 12.0;

        for (int s = 0; s < 2; s++) {
            float fs = float(s);
            vec2 splatPos;

            if (movePattern < 0.5) {
                float phase = t * (0.5 + fs * 0.3) + fs * 1.257;
                splatPos = vec2(
                    0.5 + spread * sin(phase) * cos(phase * 0.7 + fs),
                    0.5 + spread * cos(phase * 0.8) * sin(phase * 0.5 + fs * 1.5)
                );
            } else if (movePattern < 1.5) {
                float phase = t * (0.8 + fs * 0.25) + fs * 1.257;
                float r = spread * 0.5 * (0.3 + 0.7 * abs(sin(phase * 0.5)));
                splatPos = vec2(0.5 + cos(phase * 0.7 + fs * 1.257) * r, 0.5 + sin(phase * 0.7 + fs * 1.257) * r);
            } else if (movePattern < 2.5) {
                float wavePhase = t * 0.8 + fs * 0.7;
                splatPos = vec2(
                    0.5 + spread * sin(wavePhase * 0.6 + fs * 0.8),
                    0.5 + spread * 0.7 * sin(wavePhase * 1.1 + fs * 1.7)
                );
            } else if (movePattern < 3.5) {
                float vortexPhase = t * (0.6 + fs * 0.2);
                float r = spread * (0.15 + fs * 0.15);
                vec2 center = vec2(0.5 + spread * 0.3 * sin(t * 0.3 + fs), 0.5 + spread * 0.3 * cos(t * 0.25 + fs * 1.5));
                splatPos = center + vec2(cos(vortexPhase), sin(vortexPhase)) * r;
            } else {
                splatPos = vec2(
                    0.5 + spread * 0.4 * sin(t * 0.2 + fs * 2.094),
                    0.5 + spread * 0.4 * cos(t * 0.15 + fs * 2.094)
                );
            }

            vec2 mDiff = uv - splatPos;
            mDiff += orbitShift(splatPos, t, aspect);
            mDiff.x *= aspect;
            float dist2 = dot(mDiff, mDiff);
            if (dist2 < cutoff2) {
                float falloff = exp(-dist2 / (splatR * splatR)) * 0.15 * moveIntensity * audioGain; // dye keeps pace with force
                vec3 splatCol = hasInput
                    ? sampleTex(splatPos, aspect).rgb
                    : hsv2rgb(vec3(fract(t * 0.08 + fs * 0.2), 0.85, 1.0));
                dye.rgb = mix(dye.rgb, splatCol, falloff);
            }
        }

        // Seed
        if (FRAMEINDEX < 3) {
            if (hasInput) {
                dye = sampleTex(uv, aspect);
            } else {
                float a = atan(uv.y - 0.5, uv.x - 0.5);
                dye = vec4(hsv2rgb(vec3(fract(a / 6.283 + length(uv - 0.5) * 2.0), 0.7, 0.9)), 1.0);
            }
        }

        dye.a = 1.0;
        gl_FragColor = dye;
        return;
    }

    // ===== PASS 2: Final output =====
    vec4 col = texture2D(dyeBuf, uv);

    // Saturation
    float gray = dot(col.rgb, vec3(0.299, 0.587, 0.114));
    col.rgb = mix(vec3(gray), col.rgb, colorSat);

    // Surface normal — wider delta at high res for visible specular
    // At 8000px, 1px = 0.000125 in UV space — too fine for surface detail
    // Use 3px minimum so lighting reads as fluid surface, not noise
    float delta = max(3.0 * invRes.x, 3.0 * invRes.y);
    float lL = dot(texture2D(dyeBuf, uv + vec2(-delta, 0.0)).rgb, vec3(0.3, 0.6, 0.1));
    float lR = dot(texture2D(dyeBuf, uv + vec2( delta, 0.0)).rgb, vec3(0.3, 0.6, 0.1));
    float lU = dot(texture2D(dyeBuf, uv + vec2(0.0,  delta)).rgb, vec3(0.3, 0.6, 0.1));
    float lD = dot(texture2D(dyeBuf, uv + vec2(0.0, -delta)).rgb, vec3(0.3, 0.6, 0.1));
    vec3 n = normalize(vec3((lR - lL) * fluidHeight, (lU - lD) * fluidHeight, 1.0));

    // Diffuse + specular
    vec3 lightDir = normalize(vec3(1.0, 1.0, 2.0));
    float diff = clamp(dot(n, lightDir), diffMin, 1.0);
    float spec = pow(clamp(dot(reflect(-lightDir, n), vec3(0.0, 0.0, 1.0)), 0.0, 1.0), specPow) * specAmount * (1.0 + 0.35 * highP + 0.6 * punchP); // highs glint the surface, transients flash it

    vec3 fluid = col.rgb * diff + vec3(spec);
    // Continuous luminance follow (fast path): paint brightens with bass
    // and mids — smooth envelopes, moderate depth, no-op in silence.
    fluid *= 1.0 + 0.28 * bassF + 0.14 * midP;

    // ---- Source blending ----
    if (sourceBlend > 0.001 && IMG_SIZE_inputTex.x > 0.0) {
        vec2 vel = (texture2D(velBuf, uv).xy - 0.5) * 2.0;
        float velMag = length(vel);

        float warpAmt = (1.0 - sourceBlend * 0.7) * fluidSpeed * 0.12;
        vec2 warpedUV = fract(uv + vel * warpAmt);

        vec3 src = sampleTex(warpedUV, aspect).rgb;
        float fluidLum = dot(fluid, vec3(0.299, 0.587, 0.114));

        vec3 blended;

        if (blendMode < 0.5) {
            // WARP
            float blend = sourceBlend * (1.0 - smoothstep(0.0, 0.6, velMag) * 0.7);
            blended = mix(fluid, src * mix(1.0, diff, 0.6) + vec3(spec * 0.3), blend);

        } else if (blendMode < 1.5) {
            // DISSOLVE
            float noise = hash21(pos * 0.01 + vel * 5.0 + TIME * 0.1);
            float threshold = sourceBlend - velMag * 0.5;
            float dissolveMask = smoothstep(threshold - 0.1, threshold + 0.1, noise);
            blended = mix(src * mix(1.0, diff, 0.4) + vec3(spec * 0.2), fluid, dissolveMask);
            blended += (1.0 - smoothstep(0.0, 0.15, abs(noise - threshold))) * mix(src, fluid, 0.5) * 0.3;

        } else if (blendMode < 2.5) {
            // LUMA MAP
            float blend = sourceBlend * (1.0 - smoothstep(0.1, 0.6, fluidLum));
            vec3 tintedSrc = mix(src, src * (col.rgb / (fluidLum + 0.1)), 0.3);
            blended = mix(fluid, tintedSrc * diff + vec3(spec * 0.2), blend);

        } else if (blendMode < 3.5) {
            // EDGE REVEAL — velocity gradient reads only here
            vec2 velL = (texture2D(velBuf, uv + vec2(-delta, 0.0)).xy - 0.5) * 2.0;
            vec2 velR = (texture2D(velBuf, uv + vec2( delta, 0.0)).xy - 0.5) * 2.0;
            vec2 velU = (texture2D(velBuf, uv + vec2(0.0,  delta)).xy - 0.5) * 2.0;
            vec2 velD = (texture2D(velBuf, uv + vec2(0.0, -delta)).xy - 0.5) * 2.0;
            float velEdge = length(velR - velL) + length(velU - velD);
            float revealMask = max(smoothstep(0.1, 0.8, velEdge * 3.0), (1.0 - smoothstep(0.0, 0.3, velMag)) * 0.5) * sourceBlend;
            vec3 litSrc = src * diff + vec3(spec * 0.15);
            blended = mix(fluid, litSrc, revealMask);
            blended += smoothstep(0.1, 0.8, velEdge * 3.0) * mix(src, fluid, 0.5) * 0.15 * sourceBlend;

        } else {
            // CHROMATIC
            float chromaSpread = (1.0 - sourceBlend * 0.5) * fluidSpeed * 0.08;
            vec3 chromaSrc = vec3(
                sampleTex(fract(uv + vel * (warpAmt + chromaSpread)), aspect).r,
                src.g,
                sampleTex(fract(uv + vel * (warpAmt - chromaSpread)), aspect).b
            );
            vec3 litSrc = chromaSrc * mix(1.0, diff, 0.5) + vec3(spec * 0.25);
            blended = mix(fluid, litSrc, sourceBlend);
            blended += (chromaSrc - src) * smoothstep(0.0, 0.5, velMag) * 0.3;
        }

        fluid = blended;
    }

    float alpha = 1.0;
    if (transparentBg) {
        alpha = smoothstep(0.02, 0.15, dot(fluid, vec3(0.299, 0.587, 0.114)));
    }

    // ---- universal color block (defaults = no-op; saturation via native colorSat) ----
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        fluid = clamp(hM * fluid, 0.0, 1.0);
    }
    // Background: the empty (dark) canvas between paint takes the chosen color.
    if (bgColor.a > 0.0005) {
        float bgMask = 1.0 - smoothstep(0.0, 0.35, dot(fluid, vec3(0.299, 0.587, 0.114)));
        fluid = mix(fluid, bgColor.rgb, bgColor.a * bgMask);
        alpha = max(alpha, bgColor.a * bgMask);
    }

    gl_FragColor = vec4(fluid, alpha);
}
