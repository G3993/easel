/*{
  "DESCRIPTION": "UV-advection fluid sim — warp any image/video with fluid dynamics, image stays crisp and returns when fluid settles",
  "CREDIT": "Based on Paketa12/Bruno Imbrizi UV-advection technique, ported to ISF by ShaderClaw",
  "CATEGORIES": [
    "VFX",
    "Simulation"
  ],
  "INPUTS": [
    {
      "NAME": "inputTex",
      "LABEL": "Image/Video",
      "TYPE": "image"
    },
    {
      "NAME": "specAmount",
      "LABEL": "Specular",
      "TYPE": "float",
      "DEFAULT": 1.5,
      "MIN": 0,
      "MAX": 5
    },
    {
      "NAME": "specPow",
      "LABEL": "Spec Power",
      "TYPE": "float",
      "DEFAULT": 36,
      "MIN": 4,
      "MAX": 128
    },
    {
      "NAME": "showUV",
      "LABEL": "Show UV",
      "TYPE": "bool",
      "DEFAULT": false
    },
    {
      "NAME": "splatForce",
      "LABEL": "Splat Force",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.1,
      "MAX": 10,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "splatRadius",
      "LABEL": "Splat Radius",
      "TYPE": "float",
      "DEFAULT": 0.05,
      "MIN": 0.01,
      "MAX": 0.2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "bumpHeight",
      "LABEL": "Surface Depth",
      "TYPE": "float",
      "DEFAULT": 80,
      "MIN": 0,
      "MAX": 300,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "moveSpread",
      "LABEL": "Move Spread",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "poseForce",
      "LABEL": "Body Force",
      "TYPE": "float",
      "DEFAULT": 2,
      "MIN": 0,
      "MAX": 6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "poseSpawn",
      "LABEL": "Body Spawn",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "poseRadius",
      "LABEL": "Body Radius",
      "TYPE": "float",
      "DEFAULT": 0.08,
      "MIN": 0.02,
      "MAX": 0.25,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "fluidSpeed",
      "LABEL": "Fluid Speed",
      "TYPE": "float",
      "DEFAULT": 5,
      "MIN": 0.5,
      "MAX": 20,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "advectSpeed",
      "LABEL": "Advect Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.1,
      "MAX": 5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "returnRate",
      "LABEL": "Return Rate",
      "TYPE": "float",
      "DEFAULT": 0.005,
      "MIN": 0,
      "MAX": 0.05,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "vorticity",
      "LABEL": "Vorticity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "viscosity",
      "LABEL": "Viscosity",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "moveMode",
      "LABEL": "Movement",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3,
        4
      ],
      "LABELS": [
        "None",
        "Freeform",
        "Center",
        "Wave",
        "Vortex"
      ],
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "moveSpeed",
      "LABEL": "Move Speed",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0.05,
      "MAX": 2,
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
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
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
    }
  ],
  "PASSES": [
    {
      "TARGET": "velBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "uvBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "prevPoseBuf",
      "PERSISTENT": true,
      "WIDTH": "33",
      "HEIGHT": "1"
    },
    {}
  ]
}*/

// Fluid Image — UV-advection fluid simulation
// Pass 0: Velocity field (rotational self-advection + mouse/movement/audio/pose splats)
// Pass 1: UV advection buffer (stores displaced UV coordinates)
// Pass 2: prevPoseBuf (33x1 snapshot of mpPoseLandmarks — written AFTER velBuf so
//         next frame's velBuf reads last frame's pose as the "previous" sample)
// Pass 3: Final render (sample original image at warped UVs + surface lighting)

#define PI2 6.283185
#define RotNum 5

// ---- Helpers ----
float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

// ---- Rotation ----
float _ang = PI2 / float(RotNum);

vec2 rot2(vec2 v, float a) {
    float c = cos(a), s = sin(a);
    return vec2(c * v.x - s * v.y, s * v.x + c * v.y);
}

// ---- Pose landmark sampling ----
// mpPoseLandmarks is a 33x1 RGBA texture. Each landmark is stored as:
//   R = x in [0..1], G = 1-y in [0..1] (already inverted in JS), B = z+0.5, A = visibility.
// Sample at texel centers (i+0.5)/33 to avoid any LINEAR interpolation issues.
// X is mirrored here to match the selfie-view flipped webcam, same convention as hand splats.
vec2 poseJoint(int idx) {
    vec4 lm = texture2D(mpPoseLandmarks, vec2((float(idx) + 0.5) / 33.0, 0.5));
    return vec2(1.0 - lm.r, lm.g);
}
float poseJointVis(int idx) {
    return texture2D(mpPoseLandmarks, vec2((float(idx) + 0.5) / 33.0, 0.5)).a;
}
vec2 prevPoseJoint(int idx) {
    vec4 lm = texture2D(prevPoseBuf, vec2((float(idx) + 0.5) / 33.0, 0.5));
    return vec2(1.0 - lm.r, lm.g);
}
// Pose-active probe — peek at the alpha (visibility) of a few core joints. If the
// landmark texture is unbound or empty, all visibilities read zero and pose is "off".
// This replaces a host-supplied mpPoseActive flag with a self-contained derivation.
float poseActive() {
    float v = 0.0;
    v = max(v, poseJointVis(0));   // nose
    v = max(v, poseJointVis(11));  // L shoulder
    v = max(v, poseJointVis(12));  // R shoulder
    v = max(v, poseJointVis(23));  // L hip
    v = max(v, poseJointVis(24));  // R hip
    return v;
}

// Apply one joint's splat to the velocity color. Force direction = joint movement since
// last frame (fluid "follows the body"), plus a weaker radial push outward from the joint
// center (fluid "spawns from the skeleton"). Unrolled via literal int calls to avoid
// GLSL ES 1.00 dynamic indexing restrictions.
void poseSplat(int idx, vec2 uv, float aspect, float r2, float cutoff2,
               float followForce, float spawnForce, inout vec4 col) {
    float vis = poseJointVis(idx);
    if (vis < 0.3) return;

    vec2 cur  = poseJoint(idx);
    vec2 prev = prevPoseJoint(idx);

    // Reject the first-frame case where prevPoseBuf is still zero — gives a huge bogus delta.
    // If the prev sample reads as (1.0, 0.0) (the mirror of an unpopulated 0,0 texel) or
    // matches current within epsilon, treat as zero motion.
    vec2 delta = cur - prev;
    if (dot(prev, prev) < 0.001 || abs(prev.x - 1.0) < 0.0001) delta = vec2(0.0);
    // Clamp per-frame travel so a lost-then-found landmark doesn't teleport.
    delta = clamp(delta, vec2(-0.15), vec2(0.15));

    vec2 diff = uv - cur;
    diff.x *= aspect;
    float dist2 = dot(diff, diff);
    if (dist2 >= cutoff2) return;

    float falloff = exp(-dist2 / r2);

    // Follow: push fluid in the direction the joint moved.
    vec2 followVec = delta * followForce;
    // Spawn: radial outward from the joint, scaled by visibility so low-confidence
    // joints contribute less. Never zero — that's the "spawning from skeleton" feel.
    vec2 radial = normalize(diff + vec2(0.0001)) * spawnForce * 0.02 * vis;

    col.xy += (followVec + radial) * falloff;
    col.xy = clamp(col.xy, 0.0, 1.0);
}

// ---- Multi-scale rotational curl measurement ----
float getRot(vec2 pos, vec2 b, vec2 Res) {
    vec2 p = b;
    float rotSum = 0.0;
    for (int i = 0; i < RotNum; i++) {
        vec2 samp = texture2D(velBuf, fract((pos + p) / Res)).xy - vec2(0.5);
        rotSum += dot(samp, p.yx * vec2(1.0, -1.0));
        p = rot2(p, _ang);
    }
    return rotSum / float(RotNum) / dot(b, b);
}

void main() {
    vec2 Res = RENDERSIZE;
    vec2 pos = gl_FragCoord.xy;
    vec2 uv = isf_FragNormCoord;
    float aspect = Res.x / Res.y;
    int mMode = int(moveMode);

    // ===== PASS 0: Velocity Field =====
    if (PASSINDEX == 0) {
        // Stochastic rotation offset per frame
        vec2 b = cos(float(FRAMEINDEX) * 0.3 - vec2(0.0, 1.57));

        vec2 v = vec2(0.0);
        float bbMax = 0.5 * Res.y;
        bbMax *= bbMax;

        // Multi-scale curl computation (up to 20 octaves)
        for (int l = 0; l < 20; l++) {
            if (dot(b, b) > bbMax) break;
            vec2 p = b;
            for (int i = 0; i < RotNum; i++) {
                v += p.yx * getRot(pos + p, -rot2(b, _ang * 0.5), Res);
                p = rot2(p, _ang);
            }
            b *= 2.0;
        }

        // Vorticity amplification — scale the curl-derived velocity
        v *= mix(0.5, 2.0, vorticity / 5.0);

        // Self-advection
        float speedScale = fluidSpeed * sqrt(Res.x / 600.0);
        vec2 advUV = fract((pos - v * vec2(-1.0, 1.0) * speedScale) / Res);
        vec4 col = texture2D(velBuf, advUV);

        // Viscosity damping — blend stored velocity toward neutral
        col.xy = mix(col.xy, vec2(0.5), viscosity * 0.02);

        // Boundary collision — reflect velocity near edges to create spirals
        float edgeDist = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
        float edgeForce = smoothstep(0.05, 0.0, edgeDist);
        if (edgeForce > 0.0) {
            vec2 edgeNormal = vec2(0.0);
            if (uv.x < 0.05) edgeNormal.x = 1.0;
            if (uv.x > 0.95) edgeNormal.x = -1.0;
            if (uv.y < 0.05) edgeNormal.y = 1.0;
            if (uv.y > 0.95) edgeNormal.y = -1.0;
            edgeNormal = normalize(edgeNormal + 0.001);
            // Reflect velocity off boundary
            vec2 vel = (col.xy - 0.5) * 2.0;
            float into = dot(vel, -edgeNormal);
            if (into > 0.0) {
                vel += edgeNormal * into * 2.0 * edgeForce;
                col.xy = vel * 0.5 + 0.5;
            }
        }

        // ---- Mouse interaction ----
        float interacting = max(mouseDown, pinchHold);
        if (interacting > 0.3) {
            vec2 mDiff = uv - mousePos;
            mDiff.x *= aspect;
            float dist2 = dot(mDiff, mDiff);
            float r2 = splatRadius * splatRadius;
            if (dist2 < r2 * 12.0) {
                float falloff = exp(-dist2 / r2);
                vec2 force = mouseDelta * Res * splatForce * 0.0003 * interacting;
                if (dot(force, force) < 0.000001) {
                    // No delta — push radially
                    force = normalize(mDiff + 0.001) * 0.02 * interacting * splatForce;
                }
                force = clamp(force, vec2(-0.3), vec2(0.3));
                col.xy += force * falloff;
                col.xy = clamp(col.xy, 0.0, 1.0);
            }
        }

        // ---- Movement patterns (matched to CFD Paint) ----
        float t = TIME * moveSpeed;
        float spread = mix(0.05, 0.42, moveSpread);
        float intensity = moveIntensity * 0.15;
        float splatR = splatRadius * 2.5;
        float splatR2 = splatR * splatR;
        float cutoff2 = splatR2 * 12.0;

        if (mMode == 1) {
            // Freeform — 3 wandering splats
            for (int s = 0; s < 3; s++) {
                float fs = float(s);
                float phase = t * (0.5 + fs * 0.3) + fs * 1.257;
                vec2 splatPos = vec2(
                    0.5 + spread * sin(phase) * cos(phase * 0.7 + fs),
                    0.5 + spread * cos(phase * 0.8) * sin(phase * 0.5 + fs * 1.5)
                );
                vec2 mDiff = uv - splatPos;
                mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                if (dist2 < cutoff2) {
                    col.xy += vec2(
                        cos(phase * 1.3 + fs),
                        sin(phase * 0.9 + fs * 2.0)
                    ) * intensity * exp(-dist2 / splatR2);
                }
            }
        } else if (mMode == 2) {
            // Center — 3 radial pulses + vortex
            for (int s = 0; s < 3; s++) {
                float fs = float(s);
                float phase = t * (0.8 + fs * 0.25) + fs * 1.257;
                float r = spread * 0.5 * (0.3 + 0.7 * abs(sin(phase * 0.5)));
                float a = phase * 0.7 + fs * 1.257;
                vec2 splatPos = vec2(0.5 + cos(a) * r, 0.5 + sin(a) * r);
                vec2 mDiff = uv - splatPos;
                mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                if (dist2 < cutoff2) {
                    vec2 dir = normalize(splatPos - 0.5 + 0.001);
                    col.xy += dir * sin(phase * 1.5) * intensity * 1.5 * exp(-dist2 / splatR2);
                }
            }
            vec2 cDiff = uv - 0.5;
            cDiff.x *= aspect;
            float centralFalloff = exp(-dot(cDiff, cDiff) / (spread * spread * 0.3));
            col.xy += vec2(-cDiff.y, cDiff.x) * sin(t * 0.6) * intensity * 0.5 * centralFalloff;
        } else if (mMode == 3) {
            // Wave — 3 sweeping splats + global wave field
            for (int s = 0; s < 3; s++) {
                float fs = float(s);
                float wavePhase = t * 0.8 + fs * 0.7;
                vec2 splatPos = vec2(
                    0.5 + spread * sin(wavePhase * 0.6 + fs * 0.8),
                    0.5 + spread * 0.7 * sin(wavePhase * 1.1 + fs * 1.7)
                );
                vec2 mDiff = uv - splatPos;
                mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                if (dist2 < cutoff2) {
                    col.xy += vec2(
                        cos(wavePhase * 0.6 + fs * 0.8) * intensity * 1.2,
                        sin(wavePhase * 2.2 + fs) * intensity * 0.6
                    ) * exp(-dist2 / splatR2);
                }
            }
            col.xy += vec2(
                sin(uv.x * 8.0 + t * 1.5) * sin(uv.y * 6.0 - t * 0.8),
                cos(uv.x * 5.0 - t)
            ) * intensity * 0.1;
        } else if (mMode == 4) {
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
                mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                if (dist2 < cutoff2) {
                    vec2 radial = splatPos - center;
                    col.xy += vec2(-radial.y, radial.x) * intensity * 2.0 / (r + 0.05) * exp(-dist2 / splatR2);
                }
            }
            vec2 gDiff = uv - 0.5;
            gDiff.x *= aspect;
            float rotFalloff = exp(-dot(gDiff, gDiff) / (spread * spread));
            col.xy += vec2(-gDiff.y, gDiff.x) * (sin(t * 0.4) > 0.0 ? 1.0 : -1.0) * intensity * 0.3 * rotFalloff;
        }

        // ---- Body tracking: splats at every visible skeleton joint ----
        // When pose is active, inject fluid at 13 key body joints. Each splat combines a
        // movement-following force (fluid tracks body motion) with a radial outward spawn
        // force (fluid emanates from the skeleton even when still).
        if (poseActive() > 0.3) {
            float poseR  = poseRadius;
            float poseR2 = poseR * poseR;
            float poseCutoff2 = poseR2 * 12.0;
            // Unrolled joint list (literal ints required for GLSL ES 1.00 texture2D indexing
            // inside helper calls). Key points: nose + shoulders + elbows + wrists +
            // hips + knees + ankles.
            poseSplat( 0, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // nose
            poseSplat(11, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // L shoulder
            poseSplat(12, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // R shoulder
            poseSplat(13, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // L elbow
            poseSplat(14, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // R elbow
            poseSplat(15, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // L wrist
            poseSplat(16, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // R wrist
            poseSplat(23, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // L hip
            poseSplat(24, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // R hip
            poseSplat(25, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // L knee
            poseSplat(26, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // R knee
            poseSplat(27, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // L ankle
            poseSplat(28, uv, aspect, poseR2, poseCutoff2, poseForce, poseSpawn, col); // R ankle
        }

        // Audio — bass creates force splats. Knee'd so a quiet track still
        // nudges the fluid but a driving bassline reads clearly against the
        // always-on self-advection swirl (house law: never silent-dead).
        {
            float bassP = smoothstep(0.05, 0.85, audioBass);
            if (bassP > 0.01) {
                float at = float(FRAMEINDEX) * 0.1;
                vec2 splatPos = vec2(hash21(vec2(at, 1.0)), hash21(vec2(at, 2.0)));
                vec2 mDiff = uv - splatPos;
                mDiff.x *= aspect;
                float dist2 = dot(mDiff, mDiff);
                float splatR = splatRadius * (1.0 + bassP * 8.0);
                if (dist2 < splatR * splatR * 12.0) {
                    float falloff = exp(-dist2 / (splatR * splatR));
                    float splatAngle = hash21(vec2(at, 3.0)) * PI2;
                    col.xy += vec2(cos(splatAngle), sin(splatAngle)) * bassP * 1.6 * splatForce * falloff;
                    col.xy = clamp(col.xy, 0.0, 1.0);
                }
            }
        }

        // Initialization
        if (FRAMEINDEX < 4) {
            col = vec4(0.5, 0.5, 0.0, 1.0);
            // Seed with gentle initial swirl
            vec2 d = uv - 0.5;
            col.xy = vec2(0.5 - d.y * 0.2, 0.5 + d.x * 0.2);
        }

        gl_FragColor = col;
        return;
    }

    // ===== PASS 1: UV Advection =====
    if (PASSINDEX == 1) {
        // Initialize UV buffer to identity on first frames
        if (FRAMEINDEX < 4) {
            gl_FragColor = vec4(uv, 0.0, 1.0);
            return;
        }

        // Read velocity at this pixel (stored as 0-1, decode to -1..1)
        vec2 vel = (texture2D(velBuf, uv).xy - 0.5) * 2.0;

        // Advect: sample the UV buffer from where the fluid came from
        // This is the core of the Paketa12 technique — pull UVs along velocity
        vec2 advUV = fract(uv - vel * advectSpeed * 0.003);
        vec2 storedUV = texture2D(uvBuf, advUV).rg;

        // Pull back toward identity (the "return home" force)
        // When returnRate=0, UVs never return; when high, image snaps back quickly
        storedUV = mix(storedUV, uv, returnRate);

        gl_FragColor = vec4(storedUV, 0.0, 1.0);
        return;
    }

    // ===== PASS 2: prevPoseBuf snapshot (33x1) =====
    // Runs AFTER velBuf/uvBuf so next frame's velocity pass reads this as the "previous"
    // pose sample for per-joint delta computation.
    if (PASSINDEX == 2) {
        if (poseActive() > 0.3) {
            gl_FragColor = texture2D(mpPoseLandmarks, uv);
        } else {
            // Pose off — write zeros so stale landmarks don't resurrect when re-enabled.
            gl_FragColor = vec4(0.0);
        }
        return;
    }

    // ===== PASS 3: Final Render =====

    // Read warped UVs from the advection buffer
    vec2 warpedUV = texture2D(uvBuf, uv).rg;

    // UV displacement for lighting computation
    vec2 uvDisp = warpedUV - uv;

    bool hasInput = IMG_SIZE_inputTex.x > 0.0;

    // Sample the original image at warped UVs — this is why it stays crisp
    vec3 col;
    if (hasInput && !showUV) {
        col = texture2D(inputTex, warpedUV).rgb;
    } else {
        // No texture or showUV mode: visualize the UV field as colors
        // Red = X displacement, Green = Y displacement (the "UV aquarium")
        col = vec3(warpedUV.x, warpedUV.y, 0.3 + 0.3 * sin(TIME * 0.5));
    }

    // ---- Surface lighting from UV displacement gradient ----
    if (bumpHeight > 0.0) {
        float delta = max(1.0 / Res.x, 1.0 / Res.y);

        // Compute gradient of UV displacement for surface normal
        vec2 uvL = texture2D(uvBuf, uv + vec2(-delta, 0.0)).rg;
        vec2 uvR = texture2D(uvBuf, uv + vec2( delta, 0.0)).rg;
        vec2 uvU = texture2D(uvBuf, uv + vec2(0.0,  delta)).rg;
        vec2 uvD = texture2D(uvBuf, uv + vec2(0.0, -delta)).rg;

        // Use the magnitude of UV displacement as the height field
        float hL = length(uvL - (uv + vec2(-delta, 0.0)));
        float hR = length(uvR - (uv + vec2( delta, 0.0)));
        float hU = length(uvU - (uv + vec2(0.0,  delta)));
        float hD = length(uvD - (uv + vec2(0.0, -delta)));

        vec3 n = normalize(vec3(
            (hR - hL) * bumpHeight,
            (hU - hD) * bumpHeight,
            1.0
        ));

        // Diffuse lighting
        vec3 lightDir = normalize(vec3(0.5, 0.8, 1.0));
        float diff = clamp(dot(n, lightDir), 0.4, 1.0);

        // Specular highlight — Blinn-Phong
        vec2 sc = (gl_FragCoord.xy - Res * 0.5) / Res.x;
        vec3 viewDir = normalize(vec3(sc, -1.0));
        vec3 halfVec = normalize(lightDir - viewDir);
        float specRaw = pow(max(dot(n, halfVec), 0.0), specPow);

        // Phase Q v4: fwidth() AA on the specular ridge so caustic glints don't
        // flicker at sub-pixel scale on high-DPI displays.
        float ridgeMag = length(vec2(hR - hL, hU - hD)) * bumpHeight;
        float aaW = max(fwidth(ridgeMag), 0.0001);
        float ridgeMask = smoothstep(0.0, aaW * 4.0, ridgeMag);
        float spec = specRaw * specAmount * ridgeMask;

        // Phase Q v4: HDR specular peaks (1.4–2.0 linear) on bright wet caustic
        // ridges only — gated by ridge strength and source brightness so flat
        // regions stay matte. Bloom downstream picks these peaks up cleanly.
        float srcBright = max(max(col.r, col.g), col.b);
        float hdrGate = smoothstep(0.45, 0.85, srcBright)
                      * smoothstep(0.04, 0.20, ridgeMag)
                      * specRaw;
        // Warm-white wet glint, peaks ~1.7–2.0 linear on the brightest pixels.
        vec3 hdrPeak = vec3(1.0, 0.97, 0.90) * hdrGate * 1.6;

        col = col * diff + vec3(spec) + hdrPeak;
    }

    // No internal tonemap — return LINEAR HDR. Downstream Phase Q bloom expects
    // values >1.0 on highlights and clamps at composite time.

    // Audio-reactive brightness pulse — direct hook in the final render pass
    // so a driving bassline reads immediately, independent of advection lag/
    // saturation dynamics in the velocity buffer (non-gating: 0 at silence).
    {
        float bassP = smoothstep(0.05, 0.85, audioBass);
        col += vec3(0.14, 0.11, 0.07) * bassP;
    }

    // Transparent background
    float alpha = 1.0;
    if (transparentBg) {
        float lum = dot(col, vec3(0.299, 0.587, 0.114));
        alpha = smoothstep(0.02, 0.15, lum);
    }

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = col;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                   // saturation
    if (hueShift > 0.0005) {                               // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    float ucA = alpha;
    if (bgColor.a > 0.0) {                                 // fill low-alpha bg region
        uc = mix(uc, bgColor.rgb, (1.0 - ucA) * bgColor.a);
        ucA = ucA + (1.0 - ucA) * bgColor.a;
    }

    gl_FragColor = vec4(uc, ucA);
}
