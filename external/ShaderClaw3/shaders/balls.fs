/*{
  "DESCRIPTION": "Balls — metallic spheres with selective Unreal-style bloom",
  "CREDIT": "ShaderClaw (inspired by three.js selective bloom example)",
  "CATEGORIES": [
    "Generator",
    "3D"
  ],
  "INPUTS": [
    {
      "NAME": "noiseTexture",
      "LABEL": "Surface Noise",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "rimLight",
      "LABEL": "Rim Light",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "shadowSoftness",
      "LABEL": "Shadow Soft",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "bloomStr",
      "LABEL": "Bloom",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3
    },
    {
      "NAME": "bloomRadius",
      "LABEL": "Blur Size",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.1,
      "MAX": 3
    },
    {
      "NAME": "bloomRatio",
      "LABEL": "Glow Ratio",
      "TYPE": "float",
      "DEFAULT": 0.4,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "metallic",
      "LABEL": "Metallic",
      "TYPE": "float",
      "DEFAULT": 0.8,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "inputImage",
      "LABEL": "Texture",
      "TYPE": "image"
    },
    {
      "NAME": "texMix",
      "LABEL": "Texture Mix",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "ballCount",
      "LABEL": "Count",
      "TYPE": "float",
      "DEFAULT": 25,
      "MIN": 5,
      "MAX": 50,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "ballSize",
      "LABEL": "Size",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0.05,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "sizeVariance",
      "LABEL": "Size Variance",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "movementMode",
      "LABEL": "Movement",
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
        "Orbit",
        "Heart Pump",
        "Morph Center",
        "Dance Around",
        "Swarm"
      ],
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "rotSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "accentColor",
      "LABEL": "Color",
      "TYPE": "color",
      "DEFAULT": [
        1,
        1,
        1,
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
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0.02,
        0.02,
        0.04,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "transparentBg",
      "LABEL": "Transparent",
      "TYPE": "bool",
      "DEFAULT": 1,
      "GROUP": "Background"
    },
    {
      "NAME": "audioDrive",
      "LABEL": "Audio Drive",
      "TYPE": "float",
      "DEFAULT": 1.8,
      "MIN": 0,
      "MAX": 5,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "scene",
      "PERSISTENT": true
    },
    {
      "TARGET": "blurH",
      "PERSISTENT": true,
      "WIDTH": "$WIDTH/3",
      "HEIGHT": "$HEIGHT/3"
    },
    {
      "TARGET": "blurV",
      "PERSISTENT": true,
      "WIDTH": "$WIDTH/3",
      "HEIGHT": "$HEIGHT/3"
    },
    {}
  ]
}*/

// ==========================================
// Hash functions for deterministic sphere placement
// ==========================================

float hash1(float n) { return fract(sin(n) * 43758.5453); }
vec3 hash3(float n) {
    return vec3(hash1(n), hash1(n + 127.1), hash1(n + 269.5));
}

// ==========================================
// Sphere SDF scene
// ==========================================

// Returns vec2(distance, sphereID) — ID encodes which sphere was hit
vec2 mapScene(vec3 p, float count) {
    vec2 res = vec2(1e10, -1.0);
    for (float i = 0.0; i < 50.0; i++) {
        if (i >= count) break;
        // Deterministic position on a sphere shell, radius 2-6
        vec3 h = hash3(i * 7.13) * 2.0 - 1.0;
        float r = 2.0 + hash1(i * 3.91) * 4.0;
        vec3 center = normalize(h) * r;

        // ── Movement modes ────────────────────────────────────────────
        float orbitAngle = TIME * rotSpeed * (0.5 + hash1(i * 5.17) * 0.5);
        float sa = sin(orbitAngle), ca = cos(orbitAngle);

        if (movementMode < 0.5) {
            // 0: ORBIT — original behavior
            center.xz = mat2(ca, -sa, sa, ca) * center.xz;
        } else if (movementMode < 1.5) {
            // 1: HEART PUMP — radius pulses with bass like a heartbeat
            float beat = pow(audioBass, 0.6) * audioDrive;
            float bpm  = 0.5 + 0.5 * sin(TIME * 4.0 + hash1(i * 5.17) * 6.28);
            float pump = 1.0 + (beat * 0.30 + bpm * 0.10) * (0.5 + hash1(i * 7.7));
            center *= pump;
            // Slow rotation around y
            center.xz = mat2(ca * 0.5, -sa * 0.5, sa * 0.5, ca * 0.5) * center.xz;
        } else if (movementMode < 2.5) {
            // 2: MORPH CENTER — balls migrate slowly toward origin then back
            float morph = 0.5 + 0.5 * sin(TIME * rotSpeed * 0.7 + hash1(i * 3.1) * 6.28);
            center = mix(center, vec3(0.0), morph * 0.7);
            center.xz = mat2(ca, -sa, sa, ca) * center.xz;
        } else if (movementMode < 3.5) {
            // 3: DANCE AROUND — Lissajous in three planes per ball
            float a1 = TIME * rotSpeed * (0.6 + hash1(i * 7.13));
            float a2 = TIME * rotSpeed * (0.4 + hash1(i * 11.7));
            float a3 = TIME * rotSpeed * (0.5 + hash1(i * 13.3));
            center += vec3(sin(a1) * 1.0, sin(a2) * 0.6, sin(a3) * 0.8) * 0.5;
            center.xz = mat2(ca, -sa, sa, ca) * center.xz;
        } else {
            // 4: SWARM — all balls drift toward a moving target
            vec3 swarmTarget = vec3(sin(TIME * 0.3) * 2.5,
                                    cos(TIME * 0.4) * 1.5,
                                    sin(TIME * 0.5) * 2.5);
            center = mix(center, swarmTarget + (h * 1.2), 0.4);
            center.xz = mat2(ca, -sa, sa, ca) * center.xz;
        }

        // Audio pulse: expand radius on beat
        float pulse = 1.0 + audioBass * audioDrive * 0.55 * hash1(i * 11.3);

        // Wider size variance per user feedback
        float szJit = mix(1.0, 0.20 + hash1(i * 2.37) * 1.6, sizeVariance);
        float sz = ballSize * szJit * pulse;
        float d = length(p - center) - sz;

        // Optional surface noise — bumps the SDF so balls can read as
        // textured rocks, gnarled fruit, etc.
        if (noiseTexture > 0.0) {
            float bump = sin(p.x * 12.0 + i) * sin(p.y * 11.0 + i * 1.7) * sin(p.z * 13.0 + i * 2.3);
            d -= bump * noiseTexture * sz * 0.05;
        }

        if (d < res.x) {
            res = vec2(d, i);
        }
    }
    return res;
}

// ==========================================
// Raymarching
// ==========================================

vec2 raymarch(vec3 ro, vec3 rd, float count) {
    float t = 0.0;
    vec2 res = vec2(-1.0);
    for (int i = 0; i < 80; i++) {
        vec3 p = ro + rd * t;
        vec2 h = mapScene(p, count);
        if (h.x < 0.001) {
            res = vec2(t, h.y);
            break;
        }
        t += h.x;
        if (t > 30.0) break;
    }
    return res;
}

vec3 calcNormal(vec3 p, float count) {
    vec2 e = vec2(0.001, 0.0);
    return normalize(vec3(
        mapScene(p + e.xyy, count).x - mapScene(p - e.xyy, count).x,
        mapScene(p + e.yxy, count).x - mapScene(p - e.yxy, count).x,
        mapScene(p + e.yyx, count).x - mapScene(p - e.yyx, count).x
    ));
}

// ==========================================
// Shading — metallic PBR-ish
// ==========================================

vec3 shadeSphere(vec3 p, vec3 n, vec3 rd, float id) {
    // Per-sphere color: accent color with per-ball hue/brightness variation
    vec3 h = hash3(id * 7.13);
    float hueShift = (h.x - 0.5) * 0.3; // subtle hue spread around accent
    vec3 baseCol = accentColor.rgb;
    // Rotate hue slightly per sphere via RGB approximation
    float angle = hueShift * 6.2832;
    float cs = cos(angle), sn = sin(angle);
    baseCol = vec3(
        baseCol.r * (0.667 + cs * 0.333) + baseCol.g * (0.333 - cs * 0.333 + sn * 0.577) + baseCol.b * (0.333 - cs * 0.333 - sn * 0.577),
        baseCol.r * (0.333 - cs * 0.333 - sn * 0.577) + baseCol.g * (0.667 + cs * 0.333) + baseCol.b * (0.333 - cs * 0.333 + sn * 0.577),
        baseCol.r * (0.333 - cs * 0.333 + sn * 0.577) + baseCol.g * (0.333 - cs * 0.333 - sn * 0.577) + baseCol.b * (0.667 + cs * 0.333)
    );
    baseCol *= 0.5 + 0.5 * h.y; // brightness variation

    // Simple metallic shading
    vec3 lightDir = normalize(vec3(1.0, 1.5, 0.8));
    vec3 lightDir2 = normalize(vec3(-0.5, -0.3, 1.0));
    vec3 halfVec = normalize(lightDir - rd);
    vec3 halfVec2 = normalize(lightDir2 - rd);

    float diff = max(dot(n, lightDir), 0.0) * 0.7 + max(dot(n, lightDir2), 0.0) * 0.3;
    float spec = pow(max(dot(n, halfVec), 0.0), 32.0 + metallic * 64.0);
    float spec2 = pow(max(dot(n, halfVec2), 0.0), 32.0 + metallic * 64.0);

    // Fresnel
    float fresnel = pow(1.0 - max(dot(n, -rd), 0.0), 3.0);

    vec3 col = baseCol * diff * (1.0 - metallic * 0.5);
    // HDR PEAKS: specular highlights blow past 1.0 so bloom catches them.
    // Tight mirror lobe → punchy linear-HDR peaks (~1.8–2.2 on bright metals).
    vec3 specTint = baseCol * metallic + vec3(1.0) * (1.0 - metallic);
    col += specTint * (spec * 1.8 + spec2 * 0.6);
    // Fresnel rim — push slightly hot at grazing angles for bloom.
    col += fresnel * 0.45 * (baseCol * metallic + vec3(0.6));

    // Ambient
    col += baseCol * 0.08;

    return col;
}

// ==========================================
// Bloom selection — which spheres glow
// ==========================================

bool shouldBloom(float id) {
    // Deterministic: ~bloomRatio of spheres glow
    return hash1(id * 13.37 + 0.5) < bloomRatio;
}

// ==========================================
// Pass 0: Render spheres to scene buffer
// ==========================================

vec4 passScene(vec2 uv) {
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    vec2 p = (uv - 0.5) * vec2(aspect, 1.0);

    // Camera: orbit via mouse
    float camAngle = mousePos.x * 6.2832 * 0.5;
    float camPitch = 0.3 + mousePos.y * 0.5;
    float camDist = 12.0;
    vec3 ro = vec3(
        sin(camAngle) * cos(camPitch) * camDist,
        sin(camPitch) * camDist,
        cos(camAngle) * cos(camPitch) * camDist
    );
    vec3 target = vec3(0.0);
    vec3 fwd = normalize(target - ro);
    vec3 right = normalize(cross(fwd, vec3(0.0, 1.0, 0.0)));
    vec3 up = cross(right, fwd);
    vec3 rd = normalize(fwd + right * p.x + up * p.y);

    float count = floor(ballCount);
    vec2 hit = raymarch(ro, rd, count);

    if (hit.x > 0.0) {
        vec3 pos = ro + rd * hit.x;
        vec3 n = calcNormal(pos, count);
        vec3 col = shadeSphere(pos, n, rd, hit.y);

        // Triplanar texture mapping — seamless, no wrap seam
        if (texMix > 0.0) {
            vec3 blend = abs(n);
            blend = pow(blend, vec3(4.0));
            blend /= blend.x + blend.y + blend.z;
            vec3 texCol = texture2D(inputImage, pos.yz * 0.15 + 0.5).rgb * blend.x
                        + texture2D(inputImage, pos.xz * 0.15 + 0.5).rgb * blend.y
                        + texture2D(inputImage, pos.xy * 0.15 + 0.5).rgb * blend.z;
            col = mix(col, texCol, texMix);
        }

        // Audio brightness boost for blooming spheres.
        // Baseline TIME pulse keeps the shader alive in silence (audio non-gating).
        float beatAll = audioLevel * audioDrive;
        if (shouldBloom(hit.y)) {
            float idle = 0.5 + 0.5 * sin(TIME * 1.7 + hit.y * 1.3);
            float beat = beatAll;
            // HDR core boost: hot blooming spheres punch into linear HDR (~1.4–2.2x).
            col *= 1.0 + beat * 1.6 + idle * 0.25;
            // Audio/idle flash: sharp linear-HDR spike on transients pushes bloom hard.
            // Tinted by the sphere's own color so flashes feel considered, not white.
            vec3 hotTint = normalize(col + vec3(1e-4)) * 1.4 + vec3(0.2);
            col += hotTint * (beat * 0.9 + idle * 0.18);
        } else {
            // Non-bloom spheres still get a gentle audio-driven brightness
            // ripple so the reactivity reads across the whole scene, not
            // just the ~40% of spheres flagged for bloom.
            col *= 1.0 + beatAll * 0.35;
        }

        // Encode bloom flag in alpha: 1.0 = bloom, 0.5 = no bloom
        float alpha = shouldBloom(hit.y) ? 1.0 : 0.5;
        return vec4(col, alpha);
    }

    // Background
    return vec4(bgColor.rgb, 0.0);
}

// ==========================================
// Pass 1 & 2: Separable Gaussian blur (bloom-only)
// ==========================================

vec4 passBlur(vec2 uv, bool horizontal) {
    // 9-tap Gaussian weights
    float w[5];
    w[0] = 0.227027;
    w[1] = 0.1945946;
    w[2] = 0.1216216;
    w[3] = 0.054054;
    w[4] = 0.016216;

    vec2 texelSize = bloomRadius / RENDERSIZE;
    vec2 dir = horizontal ? vec2(texelSize.x, 0.0) : vec2(0.0, texelSize.y);

    // Source: scene buffer for horizontal, blurH for vertical
    vec4 center;
    if (horizontal) {
        center = texture2D(scene, uv);
    } else {
        center = texture2D(blurH, uv);
    }

    // For horizontal pass, only blur bloom-tagged pixels (alpha > 0.7)
    if (horizontal) {
        // Extract bloom-only: keep pixels with alpha == 1.0 (bloom flagged)
        vec4 result = vec4(0.0);
        for (int j = 0; j < 5; j++) {
            float fj = float(j);
            vec4 s1 = texture2D(scene, uv + dir * fj);
            vec4 s2 = texture2D(scene, uv - dir * fj);
            // Only include bloom-flagged pixels
            float b1 = step(0.7, s1.a);
            float b2 = step(0.7, s2.a);
            result.rgb += s1.rgb * b1 * w[j];
            result.rgb += s2.rgb * b2 * w[j];
        }
        // Subtract center double-counted
        float bc = step(0.7, center.a);
        result.rgb -= center.rgb * bc * w[0];
        return vec4(result.rgb, 1.0);
    } else {
        // Vertical pass: blur everything in blurH
        vec3 result = center.rgb * w[0];
        for (int j = 1; j < 5; j++) {
            float fj = float(j);
            result += texture2D(blurH, uv + dir * fj).rgb * w[j];
            result += texture2D(blurH, uv - dir * fj).rgb * w[j];
        }
        return vec4(result, 1.0);
    }
}

// ==========================================
// Pass 3: Final composite — scene + bloom
// ==========================================

vec4 passFinal(vec2 uv) {
    vec4 sceneCol = texture2D(scene, uv);
    vec3 bloomCol = texture2D(blurV, uv).rgb;

    vec3 col = sceneCol.rgb + bloomCol * bloomStr;

    // Background where nothing was hit (alpha == 0)
    if (sceneCol.a < 0.01) {
        col = bgColor.rgb + bloomCol * bloomStr;
    }

    float alpha = 1.0;
    if (transparentBg) {
        float lum = dot(col, vec3(0.299, 0.587, 0.114));
        alpha = clamp(max(sceneCol.a, lum * 2.0), 0.0, 1.0);
        if (sceneCol.a < 0.01 && length(bloomCol) < 0.01) alpha = 0.0;
    }

    // ---- universal color block (defaults = no-op) ----
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
    col = uc;

    return vec4(col, alpha);
}

// ==========================================
// Dispatch
// ==========================================

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE;
    if      (PASSINDEX == 0) gl_FragColor = passScene(uv);
    else if (PASSINDEX == 1) gl_FragColor = passBlur(uv, true);
    else if (PASSINDEX == 2) gl_FragColor = passBlur(uv, false);
    else                     gl_FragColor = passFinal(uv);
}
