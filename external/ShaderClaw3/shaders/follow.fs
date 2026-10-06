/*{
  "DESCRIPTION": "Follow — a self-organizing particle flow. Each cell carries mass + velocity; mass is pushed along its velocity toward the local centre-of-mass gradient while a 1/r^2 gravity clumps the streams into travelling points. Movement drivers (like the fluid sim): a left-edge Feed, a rotational Swirl, a wandering Turbulence, and a 'Dual Cursors' dance — two invisible cursors that orbit, twirl around each other and weave in and out, dragging the particles as if two hands were playing with the field. Palette-coloured with an additive glow. Ported from Cole Peterson's Shadertoy.",
  "CREDIT": "Cole Peterson (Plento) — ISF port + movement drivers for Easel",
  "CATEGORIES": [
    "Simulation",
    "Generator",
    "Abstract"
  ],
  "INPUTS": [
    {
      "NAME": "inputImage",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "textureMix",
      "LABEL": "Tex Color",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.7
    },
    {
      "NAME": "textureFeed",
      "LABEL": "Tex Feed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0
    },
    {
      "NAME": "glowAmt",
      "LABEL": "Glow",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.4,
      "DEFAULT": 0.08
    },
    {
      "NAME": "movement",
      "LABEL": "Movement",
      "TYPE": "long",
      "DEFAULT": 2,
      "VALUES": [
        0,
        1,
        2
      ],
      "LABELS": [
        "Side Feed",
        "Dual Cursors",
        "Both"
      ],
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "sideForce",
      "LABEL": "Feed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "danceStrength",
      "LABEL": "Dance",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1.2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "danceSpeed",
      "LABEL": "Dance Speed",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "danceScale",
      "LABEL": "Dance Range",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "swirl",
      "LABEL": "Swirl",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "turb",
      "LABEL": "Turbulence",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "gravity",
      "LABEL": "Clump",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "tintSlow",
      "LABEL": "Tint Slow Mass",
      "TYPE": "color",
      "DEFAULT": [0.12, 0.45, 1.0, 0.0],
      "GROUP": "Color"
    },
    {
      "NAME": "tintFast",
      "LABEL": "Tint Fast Mass",
      "TYPE": "color",
      "DEFAULT": [1.0, 0.22, 0.72, 0.0],
      "GROUP": "Color"
    },
    {
      "NAME": "filigree",
      "LABEL": "Filigree Layer",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.45,
      "GROUP": "Color"
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
    {}
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  FOLLOW — Easel ISF port of Cole Peterson's particle-flow Shadertoy, with
//  fluid-style movement drivers added.
//    iChannel0 Buffer A self-feedback -> bufA;  texelFetch -> texture(bufA,p/R)
//    Mouse drag -> autonomous "Dual Cursors": two invisible cursors orbit and
//      twirl around each other (and pulse in/out), each dragging the field the
//      same way the original mouse did (push along motion + deposit mass).
//    Plus Swirl (rotational), Turbulence (wandering), Speed, Feed (side force).
// ════════════════════════════════════════════════════════════════════════

#define R RENDERSIZE.xy
#define A(p) texture(bufA, (p) / RENDERSIZE.xy)
#define ss(a, b, t) smoothstep(a, b, t)

const vec2 UP    = vec2(0.,  1.);
const vec2 DOWN  = vec2(0., -1.);
const vec2 LEFT  = vec2(-1., 0.);
const vec2 RIGHT = vec2(1.,  0.);
const vec2 UPL   = vec2(-1.,  1.);
const vec2 DOWNL = vec2(-1., -1.);
const vec2 UPR   = vec2(1.,   1.);
const vec2 DOWNR = vec2(1.,  -1.);

vec2 hash22(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * vec3(.1031, .1030, .0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.xx + p3.yz) * p3.zy);
}
float hash12(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * .1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}
vec3 pal(float t) {
    return 0.5 + 0.44 * cos(vec3(1.4, 1.1, 1.4) * t + vec3(1.1, 6.1, 4.4) + .5);
}

vec4 Grad(vec2 u) {
    vec4 up = A(u + UP), down = A(u + DOWN), left = A(u + LEFT), right = A(u + RIGHT);
    vec4 upl = A(u + UPL), downl = A(u + DOWNL), upr = A(u + UPR), downr = A(u + DOWNR);
    return (up + down + left + right + upr + downr + upl + downl) / 8.;
}
vec2 massGrad(vec2 u) {
    vec2 cm = vec2(0.);
    cm += A(u + UP).x    * UP;    cm += A(u + DOWN).x  * DOWN;
    cm += A(u + LEFT).x  * LEFT;  cm += A(u + RIGHT).x * RIGHT;
    cm += A(u + UPL).x   * UPL;   cm += A(u + DOWNL).x * DOWNL;
    cm += A(u + UPR).x   * UPR;   cm += A(u + DOWNR).x * DOWNR;
    return cm;
}

// ── Dual-cursor "dance": two invisible cursors that orbit a wandering centre,
//    sit 180° apart so they twirl AROUND each other, and pulse in/out. ───────
vec2 cursorPos(float t, float sgn) {
    float mn = min(R.x, R.y);
    vec2 ctr = 0.5 * R + vec2(0.22 * R.x * sin(t * 0.31 + 1.7),
                              0.22 * R.y * cos(t * 0.27));
    float orbit = (0.10 + 0.085 * sin(t * 0.6)) * mn * danceScale; // in/out
    float ang = t * 1.3 + 0.6 * sin(t * 0.9);                      // twirl wobble
    ang += (sgn > 0.0) ? 0.0 : 3.14159265;                         // opposite sides
    return ctr + orbit * vec2(cos(ang), sin(ang));
}
void applyCursor(inout vec4 bA, vec2 u, float t, float sgn) {
    float dtv = 1.0 / 60.0;
    vec2 c    = cursorPos(t, sgn);
    vec2 cvel = c - cursorPos(t - dtv * max(danceSpeed, 0.001), sgn); // per-frame motion
    float mn  = min(R.x, R.y);
    float d   = length(c - u);
    float frc = danceStrength * 0.065 * exp2(-d * (28.0 / mn));      // res-relative falloff
    bA.zw += frc * cvel * 60.0;   // push particles in the cursor's direction
    bA.x  += 0.08 * frc;          // deposit mass under the cursor
}

vec3 color(vec4 bA) {
    float tt = abs(bA.z * 2.) + abs(bA.w * 4.) + 3.6 * length(bA.zw);
    vec3 p = pal(tt * .2 + .3);
    // user tints: slow (drifting) mass leans to tintSlow, fast streams to
    // tintFast; each color's alpha is its strength (0 = original palette)
    float spd = smoothstep(2.0, 40.0, length(bA.zw));
    p = mix(p, tintSlow.rgb * (0.35 + 0.65 * p), tintSlow.a * (1.0 - spd));
    p = mix(p, tintFast.rgb * (0.35 + 0.65 * p), tintFast.a * spd);
    vec3 col = vec3(clamp(bA.x, 0., 1.)) * p;
    return col * 3.6;
}
float glow(vec2 u) {
    vec2 uv = u / R;
    float blur = 0.; const float N = 3.;
    for (float i = 0.; i < N; i++) {
        blur += texture(bufA, uv + vec2(i * .001, 0.)).x;
        blur += texture(bufA, uv - vec2(i * .001, 0.)).x;
        blur += texture(bufA, uv + vec2(0., i * .001)).x;
        blur += texture(bufA, uv - vec2(0., i * .001)).x;
    }
    return blur / N * 4.;
}

void main() {
    vec2 u = gl_FragCoord.xy;

    // Non-gating audio: alive at audio=0; audioReact only adds on top.
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = audioBeatPulse * audioBeatPulse;

    if (PASSINDEX == 0) {
        vec4 bA = A(u);
        vec4 grad = Grad(u);
        vec2 acc = clamp(grad.zw - bA.zw, -1., 1.);
        bA = mix(bA, grad, .02);

        vec2 mg = massGrad(u);
        float massAvg = grad.x;
        float massDif = massAvg - bA.x;
        float dp = dot(mg, bA.zw);
        bA.x -= dp * massAvg;
        bA.x += .999 * dp * massDif;
        bA.zw += acc;

        float r = max(length(mg), 1.);
        // Bass thickens the clump — the dominant structural force in this sim.
        float grav = -(3.5 * gravity * (1.0 + audioReact * 0.6 * bassP) * massAvg * bA.x) / (r * r);
        bA.zw -= mg * grav;

        // Beat: a brief outward shockwave from centre that shoves + seeds mass,
        // decaying naturally as audioBeatPulse falls.
        if (beatP > 0.0) {
            vec2 rc = u - 0.5 * R;
            float rl = max(length(rc), 1.);
            bA.zw += audioReact * 5.0 * beatP * rc / rl;
            bA.x  += audioReact * 0.03 * beatP;
        }

        bool useFeed    = (movement < 0.5) || (movement > 1.5);   // Side Feed or Both
        bool useCursors = (movement > 0.5);                       // Cursors or Both

        // Side feed (force + mass from the left edge).
        if (useFeed) {
            float sf = sideForce * .002 * ss(R.x, 0., u.x * 0.9);
            bA.zw += sf * vec2(1., 0.);
            bA.x  += .06 * sf;
        }

        // Texture feed: deposit mass where the bound image is bright, so the
        // particles gather into the picture (then the movement animates it).
        if (textureFeed > 0.0 && IMG_SIZE_inputImage.x > 0.0) {
            vec3 img = texture(inputImage, u / R).rgb;
            float b = dot(img, vec3(0.299, 0.587, 0.114));
            bA.x += textureFeed * 0.03 * b;
        }

        // Dual invisible cursors dancing around each other.
        if (useCursors && danceStrength > 0.0) {
            float t = TIME * danceSpeed;
            applyCursor(bA, u, t,  1.0);
            applyCursor(bA, u, t, -1.0);
        }

        // Swirl — a rotational push around screen centre (fluid-style vortex).
        if (swirl > 0.0) {
            vec2 rc = u - 0.5 * R;
            float rl = max(length(rc), 1.);
            bA.zw += swirl * 0.5 * vec2(-rc.y, rc.x) / rl;
        }
        // Turbulence — wandering divergence-free-ish jitter.
        if (turb > 0.0) {
            float mn = min(R.x, R.y);
            vec2 q = u / mn * 6.0;
            bA.zw += turb * 0.5 * vec2(sin(q.y + TIME * 0.7), sin(q.x - TIME * 0.6));
        }

        // Overall speed: scale velocity around 1.0 (Speed=1 neutral, >1 livelier,
        // <1 calmer). Bounded near 1 so it can't blow the sim up.
        bA.zw *= 0.97 + 0.03 * speed;

        // Seed random specks so there's motion from frame 0.
        if (FRAMEINDEX < 8) {
            bA = vec4(0.);
            if (hash12(u * 343. + 232.) < .1) {
                bA.zw = 12. * (2. * hash22(u * 543. + 332.) - 1.);
                bA.x = hash22(u * 543. + 332.).x;
            }
        }

        bA.zw = clamp(bA.zw, -120., 120.);
        bA.x = clamp(bA.x, 0., 1.);
        gl_FragColor = bA;
        return;
    }

    vec4 bA = A(u);
    vec3 col = color(bA);
    col += glowAmt * glow(u) * pal(1.5 * length(bA.zw));

    // Filigree overlay: a fine high-frequency striation layer riding the main
    // motion — thin ridges perpendicular to the local velocity, phase pushed
    // by speed, only visible where the streams carry mass.
    if (filigree > 0.001) {
        float vl = length(bA.zw);
        vec2 vd = bA.zw / max(vl, 1e-3);
        float ph = dot(u, vec2(-vd.y, vd.x)) * 0.55 + vl * 0.30;
        float fil = pow(0.5 + 0.5 * sin(ph), 6.0);
        vec3 filC = pal(vl * 0.7 + 1.9);
        filC = mix(filC, tintFast.rgb, tintFast.a * 0.6);
        col += filC * fil * clamp(bA.x, 0.0, 1.0) * filigree
             * (0.45 + 0.35 * smoothstep(2.0, 30.0, vl));
    }

    // Highs: sparse sparkle riding the brighter particle streaks only.
    float sparkleGate = step(0.88, hash12(u * 0.37 + 5.1));
    col += vec3(audioReact * 0.7 * highP) * sparkleGate * col;

    // Beat: a brief flash weighted by local mass, decays with the pulse.
    col += vec3(audioReact * 0.35 * beatP) * bA.x;

    // Texture: tint the particles with the bound image — the image appears
    // "made of" the moving particles. Mass/velocity still drive brightness.
    if (textureMix > 0.0 && IMG_SIZE_inputImage.x > 0.0) {
        vec3 img = texture(inputImage, u / R).rgb;
        float lum = dot(col, vec3(0.299, 0.587, 0.114));   // particle brightness
        vec3 imgCol = img * (0.35 + 2.4 * lum);            // image lit by the particles
        col = mix(col, imgCol, textureMix);
    }

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = sqrt(clamp(col, 0.0, 1.0));
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
    // background = darkest end of the field (the void between particle streams)
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

    gl_FragColor = vec4(uc, 1.0);
}
