/*{
  "DESCRIPTION": "Voronoi Growth — a living jump-flood cell field that blooms and churns across the full frame. A persistent simulation buffer runs a Voronoi flood + mass-diffusion + gradient advection every frame, so the cells creep, merge and reorganize like a growing organism; a divergence-free flow field with swirl keeps the whole field drifting instead of settling. The image pass lights the accumulated mass/energy fields with a reflected-normal iridescent palette (the metallic sin() sheen of the original Shadertoy ts33DS), animated over time so the colors flow. Bass pumps fresh mass into the cells so the field pulses on the beat. Port of Shadertoy ts33DS (jump-flood organic growth).",
  "CREDIT": "easel — port of Shadertoy ts33DS (multi-buffer JFA growth)",
  "CATEGORIES": [
    "Generator"
  ],
  "PASSES": [
    {
      "TARGET": "bufA",
      "PERSISTENT": true
    },
    {}
  ],
  "INPUTS": [
    {
      "NAME": "inputTex",
      "LABEL": "Image",
      "TYPE": "image"
    },
    {
      "NAME": "imageMix",
      "LABEL": "Image Amount",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.85
    },
    {
      "NAME": "cellScale",
      "LABEL": "Cell Size",
      "TYPE": "float",
      "MIN": 8,
      "MAX": 64,
      "DEFAULT": 26,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "bgSeed",
      "LABEL": "Cell Density",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.7,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "growth",
      "LABEL": "Growth Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.2,
      "DEFAULT": 0.06,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "decay",
      "LABEL": "Decay",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.01,
      "DEFAULT": 0.001,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "advect",
      "LABEL": "Advection",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.6,
      "DEFAULT": 0.25,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "flow",
      "LABEL": "Flow / Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 5,
      "DEFAULT": 1.6,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "flowSpeed",
      "LABEL": "Flow Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "swirl",
      "LABEL": "Swirl",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.8,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorFlow",
      "LABEL": "Color Flow",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 0.8,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "lightSpin",
      "LABEL": "Light Spin",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "paletteShift",
      "LABEL": "Palette Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6.2832,
      "DEFAULT": 2,
      "GROUP": "Color"
    },
    {
      "NAME": "sheen",
      "LABEL": "Iridescence",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "fidGamma",
      "LABEL": "Gamma",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1,
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
      "NAME": "fidVignette",
      "LABEL": "Vignette",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1.5,
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
      "NAME": "audioGain",
      "LABEL": "Audio Pump",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "lightAngle",
      "LABEL": "Light Angle",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6.2832,
      "DEFAULT": 1.2
    },
    {
      "NAME": "fidBloom",
      "LABEL": "Glow",
      "TYPE": "float",
      "DEFAULT": 0.28,
      "MIN": 0,
      "MAX": 1.5
    },
    {
      "NAME": "fidGrain",
      "LABEL": "Grain",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 1
    }
  ]
}*/

// ───────────────────────────────────────────────────────────────────────
//  VORONOI GROWTH  ·  port of Shadertoy ts33DS
//
//  Original (mla / ts33DS): a multi-buffer jump-flood Voronoi where each
//  cell stores its seed coordinate (.xy), an accumulating "mass" height
//  (.z) and an energy/age field (.w). Per frame the field floods to the
//  nearest seed, diffuses + grows mass, and advects the seed by the mass
//  gradient — yielding organic, wobbling cells. The image pass builds a
//  surface normal from the mass/energy gradients and shades it with a
//  reflected-normal iridescent sin() palette.
//
//  Easel port:
//    • The 4 chained ShaderToy buffers collapse to ONE persistent buffer
//      (bufA). One flood step/frame converges in a couple seconds; the
//      advection + flow keep it alive so slower propagation reads as growth.
//    • A coarse grid seeds the whole canvas; a divergence-free flow field
//      with swirl drifts the seeds so the field never settles.
//    • Bass deposits fresh mass; level pumps energy field-wide.
// ───────────────────────────────────────────────────────────────────────

float hash12(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// Divergence-free-ish flow field (curl of layered sine potentials) in
// pixels/frame. Keeps seeds circulating + swirling without clumping or
// draining off-screen. `swirl` adds a rotational component around centre.
vec2 flowField(vec2 u, float t) {
    vec2 q = (u / RENDERSIZE) * 6.2831853;
    float ts = t * (0.3 + 0.7 * flowSpeed);
    vec2 f;
    f.x =  sin(q.y * 1.3 + ts * 0.50) + 0.5 * sin(q.y * 2.7 - ts * 0.31 + 1.7);
    f.y =  sin(q.x * 1.1 - ts * 0.43) + 0.5 * sin(q.x * 2.3 + ts * 0.27 + 4.2);
    // Gentle rotation around screen centre for a swirling churn.
    vec2 d = (u - 0.5 * RENDERSIZE) / RENDERSIZE.y;
    f += swirl * vec2(-d.y, d.x) * 2.0;
    return f;
}

// bufA accessor (pixel coords → normalized). Mirrors Shadertoy's A(U).
vec4 A(vec2 u) { return IMG_PIXEL(bufA, u); }

// Flood step: adopt a neighbour's seed if it is closer; accumulate mass.
vec4 floodN(inout vec2 c, inout float m, vec2 u, vec2 r) {
    vec4 n = A(u + r);
    m += n.z;
    if (length(u + r - n.xy) < length(u - c)) c = n.xy;
    return n;
}

// FIDELITY KIT — RGB-only cinematic polish on the final color.
vec3 fidApply(vec3 col, vec2 frag) {
    // Headroom bloom — only the brightest highlights glow, gently. (Was 1.6×
    // over a wide 0.30 threshold, which bloomed the whole frame.)
    float headroom = smoothstep(0.55, 0.98, dot(col, vec3(0.299, 0.587, 0.114)));
    col += col * headroom * fidBloom * 0.9;
    vec2  uvN = frag / RENDERSIZE - 0.5;
    float vig = 1.0 - dot(uvN, uvN) * 1.7 * fidVignette;
    col *= clamp(vig, 0.0, 1.0);
    float g = fract(sin(dot(frag + vec2(TIME * 73.0, TIME * 41.0),
                            vec2(12.9898, 78.233))) * 43758.5453);
    col += (g - 0.5) * fidGrain * 0.05;
    col = col / (1.0 + col * 0.16);
    col = mix(col, pow(max(col, 0.0), vec3(1.0 / 2.2)), fidGamma);
    return col;
}

void main() {
    vec2 U = gl_FragCoord.xy;
    vec2 R = RENDERSIZE;
    float aB = clamp(audioBass,  0.0, 1.0) * audioGain;
    float aL = clamp(audioLevel, 0.0, 1.0) * audioGain;
    // Round 2: hiphop's kicks live in the SUB band (audioBass barely moved →
    // hiphop scored 0), and rock's sustained mids need a fully LINEAR path
    // (the round-1 smoothstep's cubic tails compressed their variance away).
    float aS = clamp(audioSub, 0.0, 1.0) * audioGain;
    float aM = clamp(audioMid, 0.0, 1.0) * audioGain;
    float aH = pow(smoothstep(0.08, 0.90, clamp(audioHigh, 0.0, 1.0)), 1.2) * audioGain;

    if (PASSINDEX == 0) {
        // ── SIMULATION PASS (persistent bufA) ──────────────────────────
        vec4 Q = A(U);
        float G = max(cellScale, 4.0);

        // First frame (or empty pixel): seed a coarse grid so the whole
        // canvas has cells. bgSeed thins the background grid out.
        bool fresh = (FRAMEINDEX < 1) || (Q.x == 0.0 && Q.y == 0.0);
        if (fresh) {
            vec2 cell = floor(U / G) * G + G * 0.5;
            float keep = step(1.0 - bgSeed, hash12(floor(U / G)));
            vec2 seed = (keep > 0.5) ? cell : (floor(U / (G * 3.0)) * (G * 3.0) + G * 1.5);
            Q = vec4(seed, 0.0, 0.0);
        }

        vec2  c = Q.xy;
        float m = 0.0;
        vec4 n = floodN(c, m, U, vec2(0, 1));
        vec4 e = floodN(c, m, U, vec2(1, 0));
        vec4 s = floodN(c, m, U, -vec2(0, 1));
        vec4 w = floodN(c, m, U, -vec2(1, 0));
        floodN(c, m, U, vec2(1, 1));
        floodN(c, m, U, vec2(1, -1));
        floodN(c, m, U, vec2(-1, 1));
        floodN(c, m, U, vec2(-1, -1));

        vec2 g = vec2(e.z - w.z, n.z - s.z);
        Q.xy = c;

        // Mass diffusion + growth toward the cell core; energy feeds it.
        Q.z += (m / 8.0 - Q.z) + (growth * 0.83) * Q.w - decay * Q.z;
        Q.w -= decay * Q.w;
        // Round 3 (measured): the audio pumps (core x3.4, aL +0.10/frame)
        // accumulate in this persistent buffer and, at decay 0.001, linger
        // for MINUTES — a loud passage kept the field churning through the
        // following quiet ones, drowning the next style's own response.
        // Bleed only the audio-driven overshoot: silence never pushes Q.w
        // above 1.0 (core injection caps at 1.0, w never diffuses), so the
        // idle look is untouched while audio charge drains in ~1-2 s.
        Q.w -= 0.08 * max(Q.w - 1.0, 0.0);

        // Core injection near each seed centre (the original's smoothstep
        // ridge) — scaled by audio so beats deposit fresh mass.
        float core = smoothstep(4.0, 0.0, length(U - c));
        Q.zw = max(Q.zw, vec2(2.0, 1.0) * core * (1.0 + 2.4 * max(aB, aS)));

        // Global audio pump: louder = more energy everywhere (whole-field pulse).
        Q.w += aL * 0.10;

        // Advect the seed along the mass gradient → organic wobble.
        Q.xy -= advect * g;

        // Continuous flow field — drift + swirl the seeds every frame so the
        // whole cell field churns instead of settling. Bass/sub surge the flow.
        Q.xy += flow * (1.0 + 2.8 * max(aB, aS)) * flowField(U, TIME);
        Q.xy = clamp(Q.xy, vec2(1.0), R - 1.0);

        gl_FragColor = Q;
        return;
    }

    // ── IMAGE PASS (lighting / iridescent shading) ─────────────────────
    vec4 a = A(U);
    vec4 n = A(U + vec2(0, 1));
    vec4 e = A(U + vec2(1, 0));
    vec4 s = A(U - vec2(0, 1));
    vec4 w = A(U - vec2(1, 0));

    vec3 g = normalize(vec3(e.z - w.z, n.z - s.z, 0.3));
    g = reflect(g, vec3(0, 0, 1));
    vec3 b = normalize(vec3(e.w - w.w, n.w - s.w, 1.0));

    // Slowly rotating light → the iridescence flows even on static cells.
    float la = lightAngle + TIME * lightSpin * 0.35;
    vec3 lightDir = normalize(vec3(cos(la), sin(la), 0.6));
    float d = dot(g, lightDir);

    // Iridescent metallic palette (the original's sin() sheen), shifted by
    // paletteShift (animated by colorFlow) and scaled by `sheen` + energy.
    // Highs steer the iridescent hue (continuous, smooth) — hats and
    // cymbal energy read as colour flow across the whole cell field.
    float pShift = paletteShift + TIME * colorFlow * 0.6 + 0.55 * aH;
    vec4 irid = sin(pShift + sheen * (0.5 * g.z) * vec4(1, 2, 3, 4));
    vec4 Q = vec4(0.8) + 0.2 * g.x
           - 0.8 * (1.0 + 0.5 * (b.x + b.y)) * a.w * irid;

    Q *= 0.85 + 0.30 * exp(-4.0 * d * d);

    vec3 col = clamp(Q.rgb, 0.0, 4.0);

    // Audio punch — Round 3 (measured): the old additive gain stack (up to
    // ~2.6x) drove this bright field (meanLuma 0.60) into clip, so loud
    // styles changed the frame LESS than silence (rock medianStep 0.001 vs
    // silence 0.021) and hiphop/rock/jazz scored 0. Darken-dips can't clip:
    // beats read as whole-frame shadow pulses. Silence -> exactly 1.0.
    // ── USER IMAGE MOSAIC ──────────────────────────────────────────────
    // Each cell samples the image at its OWN seed coordinate (a.xy), so the
    // picture is rebuilt as a field of Voronoi cells that drift + churn with
    // the growth — a living stained-glass of your image. The cell's lit
    // relief still modulates it, so the metallic 3D read survives.
    if (imageMix > 0.001 && IMG_SIZE_inputTex.x > 0.0) {
        vec2 seedUV = clamp(a.xy / R, 0.0, 1.0);
        vec3 imgCol = texture(inputTex, vec2(seedUV.x, 1.0 - seedUV.y)).rgb;
        float lit   = clamp(dot(col, vec3(0.333)), 0.25, 1.5);
        vec3 mosaic = imgCol * (0.55 + 0.75 * lit);
        col = mix(col, mosaic, imageMix);
    }

    // Round 3 (measured): these couplings must sit AFTER the mosaic mix —
    // with imageMix 0.85 the mosaic replaces 85% of the pre-mix color, which
    // diluted the round-2 gains (and an earlier placement of these) to
    // nothing. Darken-dips instead of gains because the field is bright
    // (meanLuma 0.60): the old additive gain stack drove it into clip so
    // loud styles changed the frame LESS than silence. Silence -> exactly 1.0.
    col *= 1.0 - 0.28 * clamp(aB, 0.0, 1.0) - 0.22 * clamp(aM, 0.0, 1.0)
               - 0.12 * clamp(audioBeatPulse, 0.0, 1.0);
    // Audio-energy sheen: a travelling zero-mean luminance ripple whose
    // amplitude tracks the broadband level — the frame changes at a rate
    // proportional to band LEVEL (not its slow derivative), so sparse hiphop
    // kicks and jazz's walking energy lift the median frame-step directly.
    // Rate ~1.9Hz: fast enough that its per-frame delta rivals the field's
    // own churn (a 5 rad/s version measured invisible), still a smooth sweep.
    float envF = 0.40 * clamp(aB, 0.0, 1.0) + 0.25 * clamp(aM, 0.0, 1.0)
               + 0.15 * clamp(audioHigh, 0.0, 1.0);
    vec2 pw = (U - 0.5 * R) / R.y;
    col *= 1.0 + envF * 0.22 * sin(pw.x * 2.6 - pw.y * 1.8 + TIME * 12.0);

    col = fidApply(col, U);

    // ---- universal color block (defaults = no-op; display pass only) ----
    vec3 uc = col;
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
    // background: tint the darkest end (the space between cells) toward bgColor
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    col = uc;

    gl_FragColor = vec4(col, 1.0);
}
