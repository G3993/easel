/*{
  "DESCRIPTION": "Gravity Streams — orbiting particles with glowing trails, deferred lighting, and texture-mapped streams",
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
      "NAME": "glossiness",
      "LABEL": "Glossiness",
      "TYPE": "float",
      "DEFAULT": 120,
      "MIN": 4,
      "MAX": 256
    },
    {
      "NAME": "specular",
      "LABEL": "Specular",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "glowAmount",
      "LABEL": "Glow",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3
    },
    {
      "NAME": "particleSize",
      "LABEL": "Particle Size",
      "TYPE": "float",
      "DEFAULT": 12,
      "MIN": 2,
      "MAX": 64,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "surfaceHeight",
      "LABEL": "Surface Depth",
      "TYPE": "float",
      "DEFAULT": 384,
      "MIN": 0,
      "MAX": 800,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "texScale",
      "LABEL": "Tex Scale",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.1,
      "MAX": 5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "orbitSpeed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "orbitChaos",
      "LABEL": "Chaos",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "fadeRate",
      "LABEL": "Trail Fade",
      "TYPE": "float",
      "DEFAULT": 0.002,
      "MIN": 0,
      "MAX": 0.05,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "swirlAmt",
      "LABEL": "Orbit Swirl",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "kaleido",
      "LABEL": "Kaleidoscope",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 8,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "shimmer",
      "LABEL": "Iridescence",
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
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Color"
    },
    {
      "NAME": "palette",
      "LABEL": "Palette",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3,
        4
      ],
      "LABELS": [
        "Source",
        "Aurora",
        "Magma",
        "Cyberpunk",
        "Bioluminescence"
      ],
      "DEFAULT": 0,
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
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Color"
    },
    {
      "NAME": "vignette",
      "LABEL": "Vignette",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "scrollSpeed",
      "LABEL": "Camera Scroll",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": true,
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
      "TARGET": "albedoBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "normalBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

// Gravity Streams — analytical orbits (no persistent particle buffer needed)
// Particle positions computed from TIME using multi-frequency Lissajous orbits
// with gravitational clustering. Trails + deferred lighting in persistent buffers.

#define N_PARTICLES 12
#define PI 3.14159265

float hash(vec2 co) {
    return fract(sin(dot(co, vec2(12.9898, 78.233))) * 43758.5453);
}

// Curated palettes — sampled by particle "phase" (0..1) along the gradient.
// Returns linear-HDR RGB; some peaks intentionally exceed 1.0 so bloom catches them.
vec3 paletteSample(int p, float k) {
    k = clamp(k, 0.0, 1.0);
    if (p == 1) {
        // Aurora — deep indigo -> teal -> mint -> hot magenta
        vec3 c0 = vec3(0.05, 0.02, 0.35);
        vec3 c1 = vec3(0.05, 0.55, 0.85);
        vec3 c2 = vec3(0.35, 1.10, 0.75);
        vec3 c3 = vec3(1.40, 0.25, 0.95);
        if (k < 0.33) return mix(c0, c1, k / 0.33);
        if (k < 0.66) return mix(c1, c2, (k - 0.33) / 0.33);
        return mix(c2, c3, (k - 0.66) / 0.34);
    } else if (p == 2) {
        // Magma — black -> blood -> orange -> white-hot
        vec3 c0 = vec3(0.02, 0.0, 0.05);
        vec3 c1 = vec3(0.85, 0.05, 0.10);
        vec3 c2 = vec3(1.40, 0.55, 0.05);
        vec3 c3 = vec3(1.80, 1.60, 0.95);
        if (k < 0.33) return mix(c0, c1, k / 0.33);
        if (k < 0.66) return mix(c1, c2, (k - 0.33) / 0.33);
        return mix(c2, c3, (k - 0.66) / 0.34);
    } else if (p == 3) {
        // Cyberpunk — neon cyan / magenta / yellow
        vec3 c0 = vec3(0.0, 0.95, 1.30);
        vec3 c1 = vec3(1.30, 0.10, 0.95);
        vec3 c2 = vec3(1.40, 1.20, 0.10);
        vec3 c3 = vec3(0.10, 0.30, 1.30);
        if (k < 0.33) return mix(c0, c1, k / 0.33);
        if (k < 0.66) return mix(c1, c2, (k - 0.33) / 0.33);
        return mix(c2, c3, (k - 0.66) / 0.34);
    } else if (p == 4) {
        // Bioluminescence — abyss blue -> jellyfish cyan -> chartreuse pop
        vec3 c0 = vec3(0.0, 0.05, 0.12);
        vec3 c1 = vec3(0.05, 0.45, 0.80);
        vec3 c2 = vec3(0.20, 1.20, 1.00);
        vec3 c3 = vec3(0.95, 1.40, 0.30);
        if (k < 0.33) return mix(c0, c1, k / 0.33);
        if (k < 0.66) return mix(c1, c2, (k - 0.33) / 0.33);
        return mix(c2, c3, (k - 0.66) / 0.34);
    }
    return vec3(-1.0); // sentinel: "use source"
}

// Compute particle position analytically from time
// Uses layered sinusoidal orbits that simulate gravitational interaction
vec2 particlePos(int id, float t) {
    float fi = float(id);
    float seed1 = hash(vec2(fi * 7.13, 1.0));
    float seed2 = hash(vec2(fi * 3.71, 2.0));
    float seed3 = hash(vec2(fi * 11.37, 3.0));
    float seed4 = hash(vec2(fi * 5.91, 4.0));

    // Base orbit — each particle has unique frequency + phase
    float baseFreq = 0.3 + seed1 * 0.4;
    float basePhase = seed2 * PI * 2.0;
    float baseRadius = 0.12 + seed3 * 0.25;

    // Secondary wobble (simulates gravitational perturbation)
    float wobbleFreq = 0.7 + seed4 * 1.3;
    float wobbleAmt = 0.03 + seed1 * 0.08;

    // Gravitational clustering — particles share common attractor points
    // Two attractors orbiting slowly
    float a1x = sin(t * 0.13) * 0.15;
    float a1y = cos(t * 0.17) * 0.12;
    float a2x = sin(t * 0.11 + PI) * 0.12;
    float a2y = cos(t * 0.14 + PI) * 0.15;

    // Each particle orbits around one of the attractors (alternating)
    float attract = mod(fi, 2.0) < 0.5 ? 1.0 : 0.0;
    float ax = mix(a1x, a2x, attract);
    float ay = mix(a1y, a2y, attract);

    // Occasionally swap attractor allegiance (creates stream crossings)
    float swapPhase = sin(t * 0.08 + fi * 0.7);
    if (swapPhase > 0.3) {
        ax = mix(ax, a2x, smoothstep(0.3, 0.8, swapPhase));
        ay = mix(ay, a2y, smoothstep(0.3, 0.8, swapPhase));
    }

    float wobbleScale = wobbleAmt * (1.0 + orbitChaos * 2.0);

    float x = 0.5 + ax
        + cos(t * baseFreq + basePhase) * baseRadius
        + sin(t * wobbleFreq + seed3 * 5.0) * wobbleScale;

    float y = 0.5 + ay
        + sin(t * baseFreq * 0.8 + basePhase + 1.57) * baseRadius * 0.7
        + cos(t * wobbleFreq * 0.9 + seed1 * 5.0) * wobbleScale;

    // Keep in bounds
    x = clamp(x, 0.02, 0.98);
    y = clamp(y, 0.02, 0.98);

    // Orbit swirl: the whole constellation slowly revolves, with a radius-
    // dependent twist so outer particles shear past the inner ones. Applied
    // in analytic space so trails, normals and orbs all agree.
    if (swirlAmt > 0.001) {
        vec2 c = vec2(x, y) - 0.5;
        float rr = length(c);
        float ang = swirlAmt * (t * 0.12 + rr * 1.8 * sin(t * 0.07));
        float ca = cos(ang), sa = sin(ang);
        c = vec2(ca * c.x - sa * c.y, sa * c.x + ca * c.y);
        x = clamp(0.5 + c.x, 0.02, 0.98);
        y = clamp(0.5 + c.y, 0.02, 0.98);
    }

    return vec2(x, y) * RENDERSIZE;
}

vec2 texUV(vec2 px) {
    vec2 st = px / RENDERSIZE;
    float ar = RENDERSIZE.x / RENDERSIZE.y;
    float tar = IMG_SIZE_inputTex.x / max(IMG_SIZE_inputTex.y, 1.0);
    vec2 c = st - 0.5;
    float r = ar / max(tar, 0.001);
    if (r > 1.0) c.x *= r; else c.y /= r;
    c /= texScale;
    return fract(c + 0.5);
}

void main() {
    vec2 Res = RENDERSIZE;
    vec2 pos = gl_FragCoord.xy;
    vec2 uv = isf_FragNormCoord;
    // ── Audio conditioning — soft knees, gated on audioReact so silence
    //    renders the exact baseline look (idle floor = multiplier 1.0).
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
    float midP  = smoothstep(0.08, 0.90, audioMid);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float lvlP  = smoothstep(0.05, 0.90, audioLevel);
    float aKick = audioBeatPulse * audioBeatPulse; // decaying beat accent

    // Bass swells the stream weight — the dominant structure of the piece.
    float pSize = particleSize * (1.0 + audioReact * 0.45 * bassP);
    float t = TIME * orbitSpeed;

    // ===== PASS 0: Albedo (trails) =====
    // Interpolate between previous and current position to draw continuous trails
    // Mids lean on the streams — an accumulated sideways drift of the whole
    // trail field (impulse in, motion with memory out). Zero in silence.
    float aScroll = audioReact * 3.2 * midP;

    if (PASSINDEX == 0) {
        vec2 scrollUV = (pos + vec2(scrollSpeed + aScroll, 0.0) + 0.5) / Res;
        vec4 col = texture2D(albedoBuf, scrollUV);
        col.a *= (1.0 - fadeRate);

        bool hasTex = IMG_SIZE_inputTex.x > 0.0;
        // Use real frame delta with slight overshoot to prevent gaps
        float dt = TIMEDELTA * orbitSpeed * 1.2;

        for (int i = 0; i < N_PARTICLES; i++) {
            vec2 ppCur = particlePos(i, t);
            vec2 ppPrev = particlePos(i, t - dt);

            vec3 pc;
            int pal = int(palette);
            if (pal > 0) {
                // Palette mode — slowly cycle each particle along the gradient
                float fi = float(i);
                float k = fract(fi / float(N_PARTICLES) + TIME * 0.05 + hash(vec2(fi, 9.1)) * 0.3);
                pc = paletteSample(pal, k);
            } else if (hasTex) {
                // Sample texture at midpoint for stable color along trail
                vec2 ppMid = (ppCur + ppPrev) * 0.5;
                pc = texture2D(inputTex, texUV(ppMid)).rgb;
            } else {
                float fi = float(i);
                pc = normalize(vec3(0.1) +
                    vec3(hash(vec2(fi, 0.1) + TIME * 0.01),
                         hash(vec2(fi, 1.7) + TIME * 0.01),
                         hash(vec2(fi, 3.1) + TIME * 0.01)));
            }

            // Capsule SDF: distance from pixel to line segment (prev → cur)
            vec2 seg = ppCur - ppPrev;
            float segLen = length(seg);
            float dist;
            if (segLen < 0.5) {
                dist = distance(pos, ppCur);
            } else {
                vec2 toPos = pos - ppPrev;
                float proj = clamp(dot(toPos, seg) / (segLen * segLen), 0.0, 1.0);
                dist = distance(pos, ppPrev + seg * proj);
            }

            // Soft AA: use fwidth to anti-alias the capsule edge in screen space
            float aaw = max(fwidth(dist), 1.0);
            float a = smoothstep(pSize + aaw, pSize * 0.4 - aaw, dist);
            col = mix(col, vec4(pc, 1.0), a);
        }

        gl_FragColor = col;
        return;
    }

    // ===== PASS 1: Normals (trails) =====
    if (PASSINDEX == 1) {
        vec2 scrollUV = (pos + vec2(scrollSpeed + aScroll, 0.0) + 0.5) / Res;
        vec4 nrm = texture2D(normalBuf, scrollUV);
        float dt = TIMEDELTA * orbitSpeed * 1.2;

        for (int i = 0; i < N_PARTICLES; i++) {
            vec2 ppCur = particlePos(i, t);
            vec2 ppPrev = particlePos(i, t - dt);

            vec2 seg = ppCur - ppPrev;
            float segLen = length(seg);
            vec2 closest;
            if (segLen < 0.5) {
                closest = ppCur;
            } else {
                vec2 toPos = pos - ppPrev;
                float proj = clamp(dot(toPos, seg) / (segLen * segLen), 0.0, 1.0);
                closest = ppPrev + seg * proj;
            }

            vec2 v = pos - closest;
            float l = length(v);
            // Soft AA on normal capsule edge
            float aaw = max(fwidth(l), 1.0);
            float a = smoothstep(pSize + aaw, pSize * 0.4 - aaw, l);
            float z = sqrt(abs(pSize * pSize - l * l));
            nrm = mix(nrm, vec4(normalize(vec3(v, z)), 1.0), a);
        }

        gl_FragColor = nrm;
        return;
    }

    // ===== PASS 2: Compositing =====
    // Kaleidoscope fold: mirror the composite into N wedges (2+ = on).
    if (kaleido >= 1.5) {
        float segs = floor(kaleido + 0.5);
        float ar = Res.x / Res.y;
        vec2 kc = (uv - 0.5) * vec2(ar, 1.0);
        float kAng = atan(kc.y, kc.x);
        float kRad = length(kc);
        float sector = 6.2831853 / segs;
        kAng = mod(kAng, sector);
        kAng = abs(kAng - sector * 0.5);
        kc = kRad * vec2(cos(kAng), sin(kAng)) / vec2(ar, 1.0);
        uv = clamp(kc + 0.5, 0.001, 0.999);
        pos = uv * Res;
    }

    vec4 albedo = texture2D(albedoBuf, uv);
    vec3 rawN = texture2D(normalBuf, uv).xyz;
    vec3 normal = length(rawN) > 0.01 ? normalize(rawN) : vec3(0.0, 0.0, 1.0);

    // Simple directional light on the trail surface
    vec3 lDir = normalize(vec3(1.0, 2.0, 1.5));
    float nDot = clamp(dot(normal, lDir), 0.0, 1.0);
    float diff = mix(0.3, 1.0, nDot);
    float spec = pow(clamp(dot(reflect(-lDir, normal), vec3(0,0,1)), 0.0, 1.0), glossiness) * specular;
    // Highs glint off the trail surface — fine sparkle on the lit ridges.
    spec *= 1.0 + audioReact * 0.8 * highP;

    // Lit trail color — specular peak lifted to HDR (~1.8x at the gloss centroid)
    vec3 result = albedo.rgb * diff + vec3(spec * 0.3) + vec3(pow(spec, 1.5) * 1.5);

    // Mix with background based on trail alpha. Palette mode injects a
    // matching deep tint so the whole frame commits to the look.
    vec3 bg = bgColor.rgb;
    int palMode = int(palette);
    if (palMode == 1) bg = mix(bg, vec3(0.015, 0.005, 0.06), 0.85); // Aurora
    else if (palMode == 2) bg = mix(bg, vec3(0.05, 0.005, 0.01), 0.85); // Magma
    else if (palMode == 3) bg = mix(bg, vec3(0.02, 0.0, 0.04), 0.85); // Cyberpunk
    else if (palMode == 4) bg = mix(bg, vec3(0.0, 0.02, 0.05), 0.85); // Biolum
    result = mix(bg, result, min(albedo.a * 1.2, 1.0));

    // Particle glow — drawn along the SAME capsule that the trail pass
    // uses, so the orb sits exactly on the rendered ball, not ahead of it.
    float dt = TIMEDELTA * orbitSpeed * 1.2;
    for (int i = 0; i < N_PARTICLES; i++) {
        vec2 ppCur = particlePos(i, t);
        vec2 ppPrev = particlePos(i, t - dt);
        // Sample the trail color at the midpoint so the orb tints from
        // the same source as the trail core.
        vec2 ppMid = (ppCur + ppPrev) * 0.5;
        vec3 pc = texture2D(albedoBuf, ppMid / Res).rgb;
        if (dot(pc, pc) < 0.01) pc = vec3(0.5);

        // Capsule distance — same math as the trail render.
        vec2 seg = ppCur - ppPrev;
        float segLen = length(seg);
        float dist;
        if (segLen < 0.5) {
            dist = distance(pos, ppCur);
        } else {
            vec2 toPos = pos - ppPrev;
            float proj = clamp(dot(toPos, seg) / (segLen * segLen), 0.0, 1.0);
            dist = distance(pos, ppPrev + seg * proj);
        }

        // Tight core matched to the trail pSize so orb == ball
        // Soft AA on orb core/halo edges
        float aaw = max(fwidth(dist), 1.0);
        float core = smoothstep(pSize * 0.8 + aaw, -aaw, dist);
        float halo = smoothstep(pSize * 2.0 + aaw, pSize * 0.5 - aaw, dist) * 0.25;
        // HDR PEAKS: tight cores blow past 1.0 so the bloom catches them
        float hdrCore = pow(core, 3.0) * 2.2; // up to ~2.2x linear at center
        // Highs sparkle a sparse subset of orbs; beats accent every orb briefly.
        float sparkle = step(0.62, hash(vec2(float(i), 4.2))) * highP;
        float aGlow = 1.0 + audioReact * (0.8 * aKick + 0.6 * sparkle);
        result += pc * (core + halo) * glowAmount * 0.4 * aGlow;
        result += pc * hdrCore * glowAmount * 0.9 * aGlow;
    }

    // Iridescence: thin-film shimmer bands across the lit tube ridges —
    // fine high-frequency color interference that follows the surface normal.
    if (shimmer > 0.001) {
        float film = sin(normal.x * 9.0 + normal.y * 7.0 + normal.z * 5.0 + TIME * 1.1);
        vec3 irid = 0.5 + 0.5 * cos(vec3(0.0, 2.094, 4.188) + film * 2.6);
        result += irid * albedo.rgb * albedo.a * shimmer * 0.9 * (0.3 + 0.7 * nDot);
    }

    // Level breathes overall trail luminance (~11% at default depth);
    // mids shimmer the hue of the whole field — turbulence in color space.
    result *= 1.0 + audioReact * 0.38 * lvlP;
    result = mix(result, result.gbr, audioReact * 0.22 * midP);

    // Vignette
    if (vignette > 0.001) {
        vec2 co = (uv - 0.5) * (Res.x / Res.y) * 2.0;
        // Bass opens the vignette — the whole frame breathes with the low end.
        float rf = length(co) * 0.25 * vignette * (1.0 - audioReact * 0.35 * bassP);
        float rf2 = rf * rf + 1.0;
        result *= pow(1.0 / (rf2 * rf2), 2.24);
    }

    // Linear HDR out — host applies ACES tonemap. Clamp negatives only.
    result = max(result, vec3(0.0));

    float alpha = 1.0;
    if (transparentBg) alpha = smoothstep(0.01, 0.1, albedo.a);

    // Surprise: every ~26s the flow direction reverses for ~1.5s,
    // then snaps back. Tide pulled out and pushed in.
    {
        float _ph = fract(TIME / 26.0);
        float _f  = smoothstep(0.0, 0.06, _ph) * smoothstep(0.30, 0.18, _ph);
        result = mix(result, result.bgr, _f * 0.4);
    }

    // ---- universal color block (defaults = no-op) ----
    // (background handled by the existing bgColor/transparentBg inputs)
    vec3 uc = result;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                   // saturation
    // Color Drift: the whole palette slowly cycles around the hue wheel.
    float hueTot = hueShift + fract(TIME * 0.022 * colorDrift);
    if (hueTot > 0.0005) {                                 // cheap hue rotate (YIQ)
        float hA = hueTot * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }

    gl_FragColor = vec4(uc, alpha);
}
