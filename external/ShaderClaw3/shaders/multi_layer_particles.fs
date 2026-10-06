/*{
  "DESCRIPTION": "Multi-GPU Tiled Particles — spatial tiling for multi-GPU projection. Each GPU renders a tile of a larger virtual canvas. All particles exist in the full space; each GPU sees its slice. 3 GPUs at 832x480 = 2496x480 seamless output.",
  "CREDIT": "Etherea / ShaderClaw",
  "CATEGORIES": [
    "Generator",
    "Projection"
  ],
  "INPUTS": [
    {
      "NAME": "syncTime",
      "TYPE": "float",
      "DEFAULT": -1,
      "MIN": -1,
      "MAX": 99999,
      "LABEL": "Sync Time"
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
      "NAME": "particleCount",
      "LABEL": "Particles",
      "TYPE": "float",
      "DEFAULT": 12,
      "MIN": 1,
      "MAX": 100,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "particleSize",
      "LABEL": "Size",
      "TYPE": "float",
      "DEFAULT": 0.04,
      "MIN": 0.005,
      "MAX": 0.2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "trailLength",
      "LABEL": "Trail",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorMode",
      "LABEL": "Color Mode",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Color"
    },
    {
      "NAME": "accentColor",
      "LABEL": "Color",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.7,
        0.3,
        1
      ],
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
      "NAME": "tileIndex",
      "LABEL": "Tile Index",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 7,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "tileCount",
      "LABEL": "Tile Count",
      "TYPE": "float",
      "DEFAULT": 3,
      "MIN": 1,
      "MAX": 8,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "tileAxis",
      "LABEL": "Tile Axis",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "overlap",
      "LABEL": "Overlap",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 0.2,
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
      "NAME": "audioDrive",
      "LABEL": "Audio Drive",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 5,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "trail",
      "PERSISTENT": true
    },
    {}
  ]
}*/

// ============================================================
// Deterministic hash — identical output on every GPU/instance
// ============================================================

float hash1(float n) { return fract(sin(n) * 43758.5453); }

vec2 hash2(float n) {
    return vec2(hash1(n), hash1(n + 127.1));
}

vec3 hash3(float n) {
    return vec3(hash1(n), hash1(n + 127.1), hash1(n + 269.5));
}

// ============================================================
// Particle position in GLOBAL virtual canvas space (0-1)
// All GPUs compute the same positions — each GPU just views a slice
// ============================================================

vec2 particlePos(float id, float t) {
    vec2 h = hash2(id * 7.13);
    float fx = 0.2 + h.x * 0.8;
    float fy = 0.3 + h.y * 0.7;
    float px = hash1(id * 3.91);
    float py = hash1(id * 5.17);

    // Full-range orbits across the entire virtual canvas
    float ax = 0.3 + hash1(id * 11.3) * 0.15;
    float ay = 0.25 + hash1(id * 13.7) * 0.2;

    return vec2(
        0.5 + ax * sin(t * fx + px * 6.2832),
        0.5 + ay * sin(t * fy + py * 6.2832)
    );
}

// ============================================================
// Map pixel UV to global virtual canvas coordinates
// ============================================================

vec2 uvToGlobal(vec2 uv) {
    float tiles = max(floor(tileCount + 0.5), 1.0);
    float tile = floor(tileIndex + 0.5);
    float axis = floor(tileAxis + 0.5); // 0 = horizontal, 1 = vertical

    // Each tile covers 1/N of the virtual canvas plus overlap on edges
    float tileSize = 1.0 / tiles;
    float tileStart = tile * tileSize - overlap;
    float tileEnd = (tile + 1.0) * tileSize + overlap;
    float tileSpan = tileEnd - tileStart;

    if (axis < 0.5) {
        // Horizontal tiling: tiles are side by side
        return vec2(tileStart + uv.x * tileSpan, uv.y);
    } else {
        // Vertical tiling: tiles are stacked
        return vec2(uv.x, tileStart + uv.y * tileSpan);
    }
}

// ============================================================
// Particle color
// ============================================================

vec3 particleColor(float id) {
    float mode = floor(colorMode + 0.5);

    if (mode < 0.5) {
        // Mode 0: accent color with per-particle hue shift
        vec3 base = accentColor.rgb;
        float hueJit = (hash1(id * 13.37) - 0.5) * 0.4;
        float angle = hueJit * 6.2832;
        float cs = cos(angle), sn = sin(angle);
        return vec3(
            base.r * (0.667 + cs * 0.333) + base.g * (0.333 - cs * 0.333 + sn * 0.577) + base.b * (0.333 - cs * 0.333 - sn * 0.577),
            base.r * (0.333 - cs * 0.333 - sn * 0.577) + base.g * (0.667 + cs * 0.333) + base.b * (0.333 - cs * 0.333 + sn * 0.577),
            base.r * (0.333 - cs * 0.333 + sn * 0.577) + base.g * (0.333 - cs * 0.333 - sn * 0.577) + base.b * (0.667 + cs * 0.333)
        ) * (0.7 + 0.3 * hash1(id * 2.37));
    } else if (mode < 1.5) {
        // Mode 1: each particle gets a unique hue
        float hue = hash1(id * 17.31);
        vec3 rgb = clamp(abs(mod(hue * 6.0 + vec3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
        return rgb * (0.7 + 0.3 * hash1(id * 2.37));
    } else {
        // Mode 2: warm palette (amber → crimson → gold)
        float t = hash1(id * 17.31);
        vec3 a = vec3(1.0, 0.6, 0.1);  // amber
        vec3 b = vec3(0.9, 0.15, 0.2); // crimson
        return mix(a, b, t) * (0.7 + 0.3 * hash1(id * 2.37));
    }
}

// ============================================================
// Pass 0: Trail buffer (persistent feedback)
// ============================================================

vec4 passTrail(vec2 uv) {
    vec4 prev = texture2D(trail, uv);
    float fade = 0.9 + trailLength * 0.09;

    vec3 col = prev.rgb * fade;
    float alpha = prev.a * fade;

    // Use syncTime if set (>= 0), otherwise fall back to built-in TIME
    float baseTime = syncTime >= 0.0 ? syncTime : TIME;
    float t = baseTime * speed;
    float count = floor(particleCount);

    float audioPulse = 1.0 + audioBass * audioDrive * 0.9;
    float depositGain = 1.0 + audioBass * audioDrive * 1.6;

    // Map this pixel to global virtual canvas coordinates
    vec2 globalUV = uvToGlobal(uv);

    // Virtual canvas aspect ratio (tileCount-wide if horizontal)
    float tiles = max(floor(tileCount + 0.5), 1.0);
    float axis = floor(tileAxis + 0.5);
    float virtualAspect;
    if (axis < 0.5) {
        virtualAspect = (RENDERSIZE.x * tiles) / RENDERSIZE.y;
    } else {
        virtualAspect = RENDERSIZE.x / (RENDERSIZE.y * tiles);
    }

    vec2 gAspect = vec2(globalUV.x * virtualAspect, globalUV.y);

    // Render ALL particles — they exist in the global space
    // This GPU only "sees" particles that fall within its tile region
    for (float i = 0.0; i < 100.0; i++) {
        if (i >= count) break;

        vec2 pos = particlePos(i, t);
        vec2 posAspect = vec2(pos.x * virtualAspect, pos.y);

        float dist = length(gAspect - posAspect);
        float sz = particleSize * audioPulse * (0.7 + hash1(i * 2.37) * 0.6);

        // Skip particles too far away (optimization — outside this tile)
        if (dist > sz * 20.0) continue;

        // Soft glow falloff
        float glow = sz / (dist + 0.001);
        glow = pow(glow, 2.5) * 0.015;

        // Hard core
        float core = smoothstep(sz, sz * 0.3, dist);

        vec3 pCol = particleColor(i);

        col += pCol * (core + glow) * depositGain;
        alpha += core + glow * 0.5;
    }

    return vec4(col, clamp(alpha, 0.0, 1.0));
}

// ============================================================
// Pass 1: Final output
// ============================================================

vec4 passFinal(vec2 uv) {
    vec4 trailCol = texture2D(trail, uv);
    vec3 col = trailCol.rgb / (1.0 + trailCol.rgb);
    // TRANSPARENT BACKGROUND: alpha 0 where there are no particles/trails;
    // coverage comes from the trail buffer (soft alpha edges on the glow).
    float lum0 = max(max(col.r, col.g), col.b);
    // Audio swell rides the particles; the ambient glow lift now lives only
    // where particles are (scaled by coverage) so the empty canvas stays clear.
    // The soft alpha fringe itself breathes with the level (smooth envelope),
    // so the glow visibly swells over the host on loud passages.
    float drive = audioLevel * audioDrive;
    float cov = clamp(trailCol.a * (1.15 + 0.55 * drive)
                      + smoothstep(0.015, 0.30, lum0) * (0.8 + 0.4 * drive), 0.0, 1.0);
    col = col * (1.0 + drive * 2.2) + drive * 0.55 * cov;

    // Optional texture — particles reveal/tint it, they don't flatly crossfade over it
    if (texMix > 0.001) {
        vec3 texCol = texture2D(inputTex, uv).rgb;
        float glow = clamp(max(max(col.r, col.g), col.b), 0.0, 1.0);
        vec3 texBlend = texCol * (0.15 + glow * 1.85) + col * glow * 0.4;
        col = mix(col, texBlend, texMix);
    }

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    vec3 uc = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hueA = hueShift * 6.2831853;
        float hueC = cos(hueA), hueS = sin(hueA);
        mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                  + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                  + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hueM * uc, 0.0, 1.0);
    }
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

    // Premultiplied output: background transparent unless the user paints one
    // back in via bgColor's alpha; particles keep their soft alpha edges.
    float alphaOut = max(cov, bgColor.a);
    return vec4(uc * alphaOut, alphaOut);
}

// ============================================================
// Dispatch
// ============================================================

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE;
    if (PASSINDEX == 0) {
        gl_FragColor = passTrail(uv);
    } else {
        gl_FragColor = passFinal(uv);
    }
}
