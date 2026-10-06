/*{
  "DESCRIPTION": "Linear — a barebones boids flocking sim. Buffer A tracks 280 boids (position + velocity) in the first texel row, steering each toward the average heading and position of neighbors within sight while separating from those too close. Buffer B renders the boids as glowing dots with a ghosting trail; the image pass tone-maps it. Ported to Easel ISF: the Shadertoy 'common' tab folded inline, iChannels mapped to named persistent buffers, mouse/keyboard dropped, iDate seed replaced by TIME.",
  "CREDIT": "Cole Peterson — ISF port for Easel.",
  "CATEGORIES": [
    "Generator",
    "Simulation",
    "Particles"
  ],
  "INPUTS": [
    {
      "NAME": "trail",
      "LABEL": "Trail",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.995,
      "DEFAULT": 0.97
    },
    {
      "NAME": "exposure",
      "LABEL": "Exposure",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2.5,
      "DEFAULT": 1
    },
    {
      "NAME": "inputTex",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "texMix",
      "LABEL": "Texture Mix",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "sight",
      "LABEL": "Sight Radius",
      "TYPE": "float",
      "MIN": 10,
      "MAX": 120,
      "DEFAULT": 33,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 6,
      "DEFAULT": 2.2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "dt",
      "LABEL": "Sim Speed",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 2,
      "DEFAULT": 0.7,
      "GROUP": "Motion / Animation"
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
    {}
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  LINEAR — boids flocking (ISF port).
//    bufA: boid state, texel (i,0) = (pos.xy, vel.zw), i in [0,nParticles).
//    bufB: rendered dots + ghost trail (persistent).
//    A(p) -> texelFetch(bufA);  iChannel0/1 -> bufA / bufB by pass.
//  PASSINDEX 0 = bufA sim, 1 = bufB render, 2 = image.
// ════════════════════════════════════════════════════════════════════════

#define R   RENDERSIZE.xy
#define ss(a, b, t) smoothstep(a, b, t)
// WebGL1: texelFetch unavailable — sample texel centers via texture2D.
#define A(p) texture2D(bufA, (vec2(p) + 0.5) / R)

const int   nParticles = 200;
const float minSep     = 33.0;
const float rad        = 0.0075;
const float obRad      = 45.0;

// ── Audio conditioning (playbook: soft knees + floors, never linear) ─────
float aKnee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
float aBassP() { return pow(aKnee(audioBass, 0.05, 0.85), 1.6); }  // structural weight (flock speed)
float aHighP() { return pow(aKnee(audioHigh, 0.10, 0.90), 1.2); }  // sparkle (dot glints)
float aBeatP() { return audioBeatPulse * audioBeatPulse; }         // decaying accent (flash)

vec2 hash22(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * vec3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.xx + p3.yz) * p3.zy);
}

void main() {
    // ───────── Pass 0 — boid simulation ─────────
    if (PASSINDEX == 0) {
        ivec2 ip = ivec2(gl_FragCoord.xy);
        if (ip.x < nParticles && ip.y == 0) {
            vec4 bA = A(ip);
            bA.xy += bA.zw * dt;

            vec2 avgDir = vec2(0);
            vec2 avgPos = vec2(0);
            float nb = 0.0;

            for (int i = 0; i < nParticles; i++) {
                vec4 p = A(ivec2(i, 0));
                float d = length(bA.xy - p.xy);
                if (d <= sight) { avgDir += p.zw; avgPos += p.xy; nb++; }
                if (d <= minSep) {
                    vec2 dir = normalize(p.xy - bA.xy);
                    bA.xy -= dir * minSep * .008;
                }
            }

            // Bass gives the whole flock more urgency (the dominant
            // structure — everyone speeds up together), a beat kicks a
            // brief extra surge. Idle floor: audio 0 -> exactly authored
            // speed.
            float aSpeedMul = 1.0 + audioReact * (0.7 * aBassP() + 0.5 * aBeatP());
            if (nb > 0.0) {
                avgPos /= nb;
                bA.zw = normalize(avgDir) * speed * aSpeedMul;
                vec2 dir = normalize(avgPos - bA.xy);
                bA.zw += dir * 0.1;
            }

            bA.z += .22 * cos(TIME + gl_FragCoord.x * 555.0);
            bA.w += .22 * sin(TIME * 1.3 + gl_FragCoord.x * 355.0);

            bA.xy = mod(bA.xy, R);

            if (FRAMEINDEX < 4) {
                bA.xy = hash22(gl_FragCoord.xy * 999.0 + 522.2 + TIME) * R * 0.7 + R * 0.15;
                bA.zw = (2.0 * hash22(gl_FragCoord.xy * 999.0 + 322.2) - 1.0) * 0.4;
            }
            gl_FragColor = bA;
        } else {
            gl_FragColor = vec4(0.0);
        }
        return;
    }

    // ───────── Pass 1 — render boids + trail ─────────
    if (PASSINDEX == 1) {
        // Highs add a sparse glint radius around each dot (sparkle on a
        // sparse subset — only reads on strong high end). Idle floor:
        // audio 0 -> exactly the authored dot size.
        float glintR = rad * R.y * (1.0 + 0.5 * audioReact * aHighP());
        vec4 bB = texture(bufB, gl_FragCoord.xy / R);
        for (int i = 0; i < nParticles; i++) {
            vec2 p = A(ivec2(i, 0)).xy;
            float d = length(p - gl_FragCoord.xy);
            vec3 c = 0.5 + 0.5 * cos(vec3(4., 1., 2.) * float(i) * 53.);
            bB.xyz = mix(bB.xyz, c, ss(rad * R.y, rad * R.y - 1.0, d));
            float sd2 = ss(.4 * rad * R.y, .4 * rad * R.y - 1.0, d);
            bB.w = mix(bB.w, 1.0, sd2);
            // Highs: a soft outer glint ring per dot, only visible once
            // audio pushes past the knee — clean flock in silence.
            float glint = ss(glintR, glintR - 1.5, d) - ss(rad * R.y, rad * R.y - 1.0, d);
            bB.xyz += c * max(glint, 0.0) * audioReact * 0.8 * aHighP();
        }
        bB.xyz *= 0.8;
        bB.w   *= trail;
        gl_FragColor = bB;
        return;
    }

    // ───────── Pass 2 — image ─────────
    vec4 bB = texture(bufB, gl_FragCoord.xy / R);
    vec4 f = vec4(0.44 * vec3(bB.w), 1.0);
    f.xyz += 1.0 - exp(-bB.xyz);

    // Bass/beat lift a faint ambient wash through the dark field behind
    // the flock — the one part of the frame with real headroom (the dots
    // and trail already run bright). Bass = steady glow, beat = a brief
    // extra flare. Idle floor: audio 0 -> the field stays clean black.
    float aB = audioReact * aBassP();
    float aBeat = audioReact * aBeatP();
    vec2 fuv = gl_FragCoord.xy / R;
    float vign = 1.0 - length(fuv - 0.5) * 0.9;
    // Beat flare depth capped: 1.3 lit the whole dark field in a single
    // frame (choppy); 0.5 still reads, and beatPulse gives the 300-600ms
    // glide back down.
    vec3 wash = vec3(0.32, 0.48, 0.75) * (0.6 * aB + 0.5 * aBeat) * clamp(vign, 0.0, 1.0);
    f.rgb += wash * (1.0 - clamp(bB.w, 0.0, 1.0));

    if (texMix > 0.001) {
        // Boids fly in front of the image: the texture shows through as a
        // dim backdrop only where the trail hasn't claimed the pixel, so the
        // flock reads as flying over it rather than a flat crossfade.
        vec2 tuv = gl_FragCoord.xy / R;
        vec3 texCol = texture2D(inputTex, tuv).rgb;
        vec3 backdrop = texCol * (1.0 - clamp(bB.w, 0.0, 1.0)) * 0.6;
        f.rgb = mix(f.rgb, f.rgb + backdrop, texMix);
    }

    vec3 ucCol = f.rgb * exposure;
    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(ucCol, vec3(0.299, 0.587, 0.114));
    ucCol = mix(vec3(ucL), ucCol, colorBoost);
    if (hueShift > 0.0005) {
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        ucCol = clamp(hM * ucCol, 0.0, 1.0);
    }
    ucCol = mix(ucCol, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    gl_FragColor = vec4(ucCol, 1.0);
}
