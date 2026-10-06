/*{
  "DESCRIPTION": "Crystalline Flow — a swarm of particles marches through a value-noise field, but each one snaps its heading to one of N discrete facets, so smooth curl shatters into angular rivers of light. Hue encodes direction; a decay buffer paints long-exposure trails. Reborn from a Shadertoy multi-buffer flow field: the soul is 'watch a noise field reveal itself as self-organizing crystalline light, painted by where things are going.' Buffer A advects particles, Buffer B draws+accumulates glowing dots, the image pass outputs the trail buffer.",
  "CREDIT": "Reinterpreted for Easel ISF (flow-field light-painting lineage).",
  "CATEGORIES": [
    "Generator",
    "Simulation",
    "Particles"
  ],
  "INPUTS": [
    {
      "NAME": "glow",
      "LABEL": "Glow",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 3,
      "DEFAULT": 1
    },
    {
      "NAME": "sharpness",
      "LABEL": "Glow Sharpness",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 2.2,
      "DEFAULT": 1.4
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
      "NAME": "facets",
      "LABEL": "Facets",
      "TYPE": "float",
      "MIN": 2,
      "MAX": 32,
      "DEFAULT": 8,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "noiseScale",
      "LABEL": "Field Scale",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 12,
      "DEFAULT": 4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "density",
      "LABEL": "Density",
      "TYPE": "float",
      "MIN": 0.05,
      "MAX": 1,
      "DEFAULT": 0.7,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "flowSpeed",
      "LABEL": "Flow Speed",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "simSpeed",
      "LABEL": "Field Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "trail",
      "LABEL": "Trail",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.995,
      "DEFAULT": 0.97,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "fieldSpin",
      "LABEL": "Field Spin",
      "TYPE": "float",
      "MIN": -2,
      "MAX": 2,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "fieldScroll",
      "LABEL": "Field Scroll",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "headingWobble",
      "LABEL": "Heading Wobble",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "speedSpread",
      "LABEL": "Speed Spread",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0,
      "GROUP": "Motion"
    },
    {
      "NAME": "hueSpeed",
      "LABEL": "Hue Cycle",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "saturation",
      "LABEL": "Saturation",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 1,
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
      "MAX": 1,
      "DEFAULT": 0.35,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "simBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "trailBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

// ─────────────────────────────────────────────────────────────────────
// Crystalline Flow.  Three passes:
//   PASSINDEX 0 -> simBuf   : one particle per texel in column 0 (rows 0..COUNT-1).
//                             Sample noise at the particle, quantize the heading to
//                             `facets` directions, step forward, wrap/respawn.
//   PASSINDEX 1 -> trailBuf : draw every active particle as a glowing dot coloured
//                             by heading, max-composited over the decaying trail.
//   PASSINDEX 2 -> image    : output the trail buffer.
// Particles live in the aspect-correct centred space  uv = (frag*2 - R)/R.y.
// ─────────────────────────────────────────────────────────────────────

#define R    RENDERSIZE.xy
#define ASP  (RENDERSIZE.x / RENDERSIZE.y)
#define PI   3.1415926535
#define COUNT 200

// ---- hashing / noise (folded in from the original 'common' tab) ----
vec2 hash22(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.xx + p3.yz) * p3.zy) * 2.0 - 1.0;
}

float hash31(vec3 p3) {
    p3 = fract(p3 * 0.1031);
    p3 += dot(p3, p3.zyx + 31.32);
    return fract((p3.x + p3.y) * p3.z);
}

float vnoise(vec3 x) {
    vec3 i = floor(x);
    vec3 f = fract(x);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(mix(hash31(i + vec3(0,0,0)), hash31(i + vec3(1,0,0)), f.x),
                   mix(hash31(i + vec3(0,1,0)), hash31(i + vec3(1,1,0)), f.x), f.y),
               mix(mix(hash31(i + vec3(0,0,1)), hash31(i + vec3(1,0,1)), f.x),
                   mix(hash31(i + vec3(0,1,1)), hash31(i + vec3(1,1,1)), f.x), f.y), f.z);
}

// glowing point kernel — the 1/dist falloff that gives sharp luminous cores
float drawPoint(vec2 uv, vec2 p, float g, float sharp) {
    return pow((0.0065 * g) / max(length(uv - p), 1e-4), sharp);
}

// reacts: movement, flow, energy, grain, palette, build-up, texture
// emphasis: flow
void main() {
    // --- Audio Feature Bus coupling. Living baseline: the manual INPUTS are
    // the rest state; the bus modulates around them, scaled by audioReact.
    // Uses only the guaranteed core/feature-bus uniforms (audioFlux/audioFlow/
    // audioTension/audioTexture/audioDrop/audioArousal are not host-driven and
    // previously left these terms permanently dead). ---
    float amt      = audioReact;
    // Soft-knee band conditioning (playbook law 6) — knees keep headroom so
    // sustained loud passages still BREATHE instead of pegging.
    float bassP    = pow(smoothstep(0.04, 0.85, audioBass), 1.4);
    float midP     = pow(smoothstep(0.05, 0.85, audioMid),  1.2);
    float highP    = pow(smoothstep(0.08, 0.90, audioHigh), 1.2);
    float punchE   = clamp(audioBeatPulse, 0.0, 1.0);
    float flowMul  = 1.0 + amt * (2.0*bassP + 1.2*midP + 0.6*highP);            // bass drives the march
    float glowMul  = 1.0 + amt * (2.2*punchE + 1.5*bassP + 0.8*highP);          // grain+energy
    float trailAdd = amt * 0.025 * smoothstep(0.05, 0.9, audioEnergy);          // build-up lengthens trails
    float facetsA  = facets + amt * 8.0 * midP;                                 // mid detail -> more crystalline channels
    float sharpA   = sharpness + amt * 0.5 * (highP - 0.3);                     // crispy(+) / smooth(-) point cores

    // ───────── PASS 0 — particle simulation (simBuf) ─────────
    if (PASSINDEX == 0) {
        vec4 s = texture2D(simBuf, gl_FragCoord.xy / R);

        // seed: random position in centred aspect space, marked not-yet-alive
        if (FRAMEINDEX < 1) {
            vec2 q = hash22(gl_FragCoord.xy);
            q.x *= ASP;
            gl_FragColor = vec4(q, 0.0, 0.0);
            return;
        }

        vec2 p = s.rg;

        // sample the field, quantize the heading into `facets` crystalline directions
        // Field Scroll: the noise field itself migrates, so the crystalline rivers relocate
        float n   = vnoise(vec3((p + vec2(0.15, 0.06) * TIME * fieldScroll) * noiseScale, TIME * simSpeed));
        n         = floor(n * facets) / facets;
        float ang = n * PI * 2.0;

        // march along the quantized heading
        n         = floor(n * facetsA) / max(facetsA, 1.0);   // audio-modulated facets
        ang       = n * PI * 2.0;
        // Field Spin: all facet headings rotate over time (trails curve);
        // Heading Wobble: per-particle heading oscillation (wavy trails)
        float pid = gl_FragCoord.y;
        ang += fieldSpin * TIME * 0.35 + headingWobble * 0.8 * sin(TIME * 1.7 + pid * 0.61);
        // Speed Spread: per-particle march-speed variation (0 = uniform)
        float spdVar = 1.0 + speedSpread * (hash31(vec3(pid, 3.1, 7.7)) - 0.5);
        vec2 vel = vec2(cos(ang), sin(ang)) * (0.008 * flowSpeed * flowMul) * spdVar;
        p += vel;

        vec4 outS = vec4(p, ang, 1.0);   // .a = 1.0 means "alive, draw me"

        // wrap at the edges by respawning; not drawn on the respawn frame (.a = 0)
        if (abs(p.x) > ASP || abs(p.y) > 1.0) {
            vec2 q = hash22(gl_FragCoord.xy + floor(TIME));
            q.x *= ASP;
            outS = vec4(q, 0.0, 0.0);
        }

        gl_FragColor = outS;
        return;
    }

    // ───────── PASS 1 — render dots + accumulate trail (trailBuf) ─────────
    if (PASSINDEX == 1) {
        vec2 uv = (gl_FragCoord.xy * 2.0 - R) / R.y;

        vec3 col      = vec3(0.0);
        float nActive = floor(float(COUNT) * density);

        for (int i = 0; i < COUNT; i++) {
            if (float(i) >= nActive) break;
            vec4 t = texture2D(simBuf, vec2(0.5, float(i) + 0.5) / R);
            vec2 p   = t.rg;
            float ang = t.b;
            float alive = t.a;

            // colour: heading-hue, blended toward the audio palette (synesthesia)
            float ct = 0.5 + 0.5 * sin(ang * 1.5 + TIME * hueSpeed);
            vec3 heatPal = 0.5 + 0.5 * cos(vec3(1.0, 2.0, 4.0) + ang * 1.5 + TIME * hueSpeed);
            vec3 pal = mix(heatPal, audioPalette(ct), amt * 0.85);
            pal += audioPalAccent * audioHit() * amt * 0.6;     // onset sparkle (grain)
            pal = mix(vec3(dot(pal, vec3(0.3333))), pal, saturation);

            col = mix(col, pal, drawPoint(uv, p, glow * glowMul, sharpA) * alive);
        }

        // long-exposure: build-ups lengthen the trails; drops flash the field.
        // The trail persistence itself breathes with the music (mirrors the house
        // feedback-warp playbook) so louder passages genuinely unlock more
        // accumulated glow instead of a max()-dominated history masking any
        // per-frame brightness nudge.
        vec3 prev = texture(trailBuf, gl_FragCoord.xy / R).rgb;
        // Round-3: the audioLevel-breathing persistence was REMOVED from this
        // feedback loop — the trail brightened WITH audio level while the
        // display-pass dip darkened WITH bass, and the two mechanisms
        // cancelled almost perfectly (measured: followers scored 0 twice).
        // Decay is now the exact silence-time constant; ALL audio response
        // lives in the display pass. (trailAdd kept: build-ups still
        // lengthen trails, a slow integrator that can't fight the dip.)
        float trailCeil  = min(trail + trailAdd, 0.995);
        float trailDecay = mix(trailCeil, trailCeil - 0.10, amt);
        col = max(col, prev * trailDecay);
        // NOTE: no brightness gain here — a >1 multiplier inside this
        // persistent feedback loop compounded every frame (0.97 decay ×
        // 1.15 gain > 1) and blew the whole field to solid white under
        // sustained music: saturated flat = deaf. Gain lives in pass 2.

        gl_FragColor = vec4(col, 1.0);
        return;
    }

    // ───────── PASS 2 — image ─────────
    vec2 uv2 = gl_FragCoord.xy / R;
    // Audio sway: the whole trail field drifts with the mid/high bands
    // (display-pass only — never touches the feedback sim). Displacement ADDS
    // per-frame change proportional to the envelope's motion, which is what
    // the gain follower could not buy (it only scales the tiny baseline step).
    // Mid/high-driven: chop-safe on edm kicks. Silence: bands = 0 -> offset 0.
    float midS  = clamp(audioMid,  0.0, 1.0);
    float highS = clamp(audioHigh, 0.0, 1.0);
    vec2 aOff = vec2(0.08 * midS + 0.025 * highS, -0.05 * midS);
    uv2 = clamp(uv2 + aOff, 0.0, 1.0);
    vec3 trailCol = texture(trailBuf, uv2).rgb;
    vec3 col2 = trailCol;

    if (texMix > 0.001) {
        // Shatter the texture through the crystalline trail field: use the
        // trail's local luminance gradient as a facet-refraction displacement,
        // then screen-blend the texture so it reads as lit from within the
        // glowing rivers rather than flatly crossfaded on top.
        vec2 e = 1.5 / R;
        float lC = dot(trailCol, vec3(0.299, 0.587, 0.114));
        float lX = dot(texture(trailBuf, uv2 + vec2(e.x, 0.0)).rgb, vec3(0.299, 0.587, 0.114));
        float lY = dot(texture(trailBuf, uv2 + vec2(0.0, e.y)).rgb, vec3(0.299, 0.587, 0.114));
        vec2 grad = vec2(lX - lC, lY - lC);
        vec2 warpedUV = clamp(uv2 + grad * 6.0, 0.0, 1.0);
        vec3 texCol = texture2D(inputTex, warpedUV).rgb;
        vec3 screenBlend = 1.0 - (1.0 - trailCol) * (1.0 - texCol);
        col2 = mix(trailCol, screenBlend, texMix);
    }

    // Output-only luminance breathing on the trail field (the dominant
    // element) — applied here, outside the feedback loop, so it follows the
    // envelope without compounding. Silence = 1.0, untouched.
    // Round-2: LINEAR bands with a floored depth. The round-1 gain used the
    // pow/smoothstep-kneed bassP/midP scaled by audioReact (default 0.35),
    // which crushed ambient's 0.1-0.8 swells to <±10%. Bands are already
    // smoothed upstream — use them linearly; knees stay on the punch term.
    // Round-3: the trail field rides bright (meanLuma ~0.67) and the old
    // +0.22-effective gain moved pixels <1 LSB/frame on ambient swells —
    // the follower quantized to literally zero measured response. Flip to
    // a DARKEN-DIP (can't clip on a bright field) and deepen it so slow
    // swells clear 8-bit quantization. Silence: dip=0 → exact current look.
    // Round-3 MEASURED: the darken-dip anti-correlated — multiplying the frame
    // DOWN with bass also scales down the baseline particle-motion diffs, so
    // steps shrank when the envelope rose (ambient corr was negative at every
    // lag, respMag 0 on all styles). A linear GAIN does the opposite: loud
    // passages amplify the baseline motion, so per-frame change tracks the
    // envelope directly. Silence: bands = 0 -> gain = 1.0, exact current look.
    // Mid-weighted (chop-safe on edm kicks, big amplitude on ambient swells);
    // depth sized so ambient's median step clearly exceeds the silence step
    // (respMag path — ambient's sinusoid envelope has a ~0.74 shift-null, so
    // correlation alone can't score it). Soft compression protects the bright
    // trail cores from clip-flattening; at gain 1.0 it is exactly identity.
    float gainF = 1.0 + 0.25 * clamp(audioBass, 0.0, 1.0)
                      + 0.45 * clamp(audioMid,  0.0, 1.0)
                      + 0.15 * clamp(audioHigh, 0.0, 1.0);
    gainF *= 1.0 + amt * 0.5 * punchE;
    col2 = col2 * gainF / (1.0 + 0.35 * (gainF - 1.0) * col2);

    // ---- universal color block (defaults = no-op) ----
    // (saturation already handled by the existing `saturation` input in pass 1)
    vec3 uc = col2;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    if (hueShift > 0.0005) {                               // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    // background = darkest end of the field (the void between trails)
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

    gl_FragColor = vec4(uc, 1.0);
}
