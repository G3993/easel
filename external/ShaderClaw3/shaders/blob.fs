/*{
  "DESCRIPTION": "Blob — flockaroo's 'mud planet' with expanded warp, scale, flow, texture and lighting controls. Buffer A advects a velocity/concentration field packed as two hemispheres; the image pass raymarches the sphere, displaces it by the field, and shades it.",
  "CREDIT": "Florian Berger (flockaroo) 2017 — CC BY-NC-SA 3.0. ISF port + extensions for Easel.",
  "CATEGORIES": [
    "3D",
    "Generator",
    "Fluid",
    "Simulation"
  ],
  "INPUTS": [
    {
      "NAME": "sunInfluence",
      "LABEL": "Sun Drive Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "surfaceRoughness",
      "LABEL": "Surface Roughness",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5
    },
    {
      "NAME": "specPower",
      "LABEL": "Specular Power",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 2,
      "DEFAULT": 0.5
    },
    {
      "NAME": "fresnelStr",
      "LABEL": "Fresnel Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.9
    },
    {
      "NAME": "aoStrength",
      "LABEL": "AO Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "exposure",
      "LABEL": "Exposure",
      "TYPE": "float",
      "MIN": 0.1,
      "MAX": 4,
      "DEFAULT": 1
    },
    {
      "NAME": "gamma",
      "LABEL": "Gamma",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 2.5,
      "DEFAULT": 1
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
      "NAME": "blobScale",
      "LABEL": "Blob Scale",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "displacement",
      "LABEL": "Displacement",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.75,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "warpTwist",
      "LABEL": "Warp Twist",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "warpPinch",
      "LABEL": "Warp Pinch/Bulge",
      "TYPE": "float",
      "MIN": -2,
      "MAX": 2,
      "DEFAULT": 0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "warpNoise",
      "LABEL": "Warp Noise Amount",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "warpNoiseScale",
      "LABEL": "Warp Noise Scale",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 8,
      "DEFAULT": 2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "orbitSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "flowSpeed",
      "LABEL": "Flow Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 5,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "simSpeed",
      "LABEL": "Sim Step Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "flowIntensity",
      "LABEL": "Flow Intensity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "flowCurl",
      "LABEL": "Flow Curl Bias",
      "TYPE": "float",
      "MIN": -2,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "deepColor",
      "LABEL": "Deep Color",
      "TYPE": "color",
      "DEFAULT": [
        0.03,
        0.13,
        0.28,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "midColor",
      "LABEL": "Mid Color",
      "TYPE": "color",
      "DEFAULT": [
        0.1,
        0.35,
        0.55,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "highColor",
      "LABEL": "Highlight Color",
      "TYPE": "color",
      "DEFAULT": [
        0.9,
        0.95,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "sunColor",
      "LABEL": "Sun Color",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.85,
        0.6,
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
      "NAME": "vignette",
      "LABEL": "Vignette",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.3,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "cameraDistance",
      "LABEL": "Camera Distance",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 30,
      "DEFAULT": 13.5,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "cameraFOV",
      "LABEL": "Camera FOV",
      "TYPE": "float",
      "MIN": 2,
      "MAX": 14,
      "DEFAULT": 8,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "cameraOrbitX",
      "LABEL": "Camera Tilt",
      "TYPE": "float",
      "MIN": -1,
      "MAX": 1,
      "DEFAULT": 0.27,
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
//  BLOB / mud planet — flockaroo 2017, expanded ISF port.
//  PASSINDEX 0 = Buffer A (CFD step), 1 = image (raymarch render).
// ════════════════════════════════════════════════════════════════════════

#define MAX_RADIUS 400.0
#define PI 3.14159265359
#define RotNum 5

#define Res  RENDERSIZE
#define Res1 RENDERSIZE

const float ang = 2.0 * 3.14159265359 / float(RotNum);

// ── Audio conditioning (playbook: soft knees + floors, never linear) ────────
float aKnee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
float aBassP() { return pow(aKnee(audioBass, 0.05, 0.85), 1.6); }  // dominant-structure weight
float aHighP() { return pow(aKnee(audioHigh, 0.10, 0.90), 1.2); }  // sparkle
float aBeatP() { return audioBeatPulse * audioBeatPulse; }         // decaying accent

// Audio "zoom": bass/beat shrink the effective blob scale used by the SDF,
// which reads as the planet swelling toward camera (verified numerically
// safe for this raymarcher, unlike perturbing camera distance directly).
// Set once per-fragment in main() before dist()/march() are called.
float gZoomDiv = 1.0;

// ── Procedural hash ──────────────────────────────────────────────────────────
vec4 hash4(vec2 p) {
    vec4 q = vec4(dot(p, vec2(127.1, 311.7)),
                  dot(p, vec2(269.5, 183.3)),
                  dot(p, vec2(113.5, 271.9)),
                  dot(p, vec2(246.1, 124.6)));
    return fract(sin(q) * 43758.5453);
}
vec4 randS(vec2 uv) { return hash4(uv * Res.xy + 3.1) - vec4(0.5); }

// ── Smooth value noise ───────────────────────────────────────────────────────
float vnoise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash4(i).x;
    float b = hash4(i + vec2(1.0, 0.0)).x;
    float c = hash4(i + vec2(0.0, 1.0)).x;
    float d = hash4(i + vec2(1.0, 1.0)).x;
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

// ── Sphere <-> fragment packing ──────────────────────────────────────────────
vec2 sph2frag(vec3 p) {
    float r = min(RENDERSIZE.x / 4.0, MAX_RADIUS);
    vec2 center = vec2(r, r);
    p = normalize(p);
    if (p.z < 0.0) center.x += 2.0 * r;
    return center + sqrt(1.0 - abs(p.z)) * normalize(p.xy) * r;
}
vec3 frag2sph(vec2 f) {
    float r = min(RENDERSIZE.x / 4.0, MAX_RADIUS);
    vec2 center = vec2(r, r);
    float sz = 1.0;
    if (f.x > 2.0 * r) { center.x += 2.0 * r; sz = -1.0; }
    vec2 R = (f - center) / r / sqrt(2.0);
    if (dot(R, R) > 0.55) return vec3(100.0);
    vec3 n = vec3(R.xy, sqrt(max(0.0, 1.0 - dot(R, R))) * sz);
    vec3 pp = reflect(vec3(0.0, 0.0, -1.0 * sz), n);
    return pp;
}

// ── Quaternion rotation ──────────────────────────────────────────────────────
vec4 multQuat(vec4 a, vec4 b) {
    return vec4(cross(a.xyz, b.xyz) + a.xyz * b.w + b.xyz * a.w,
                a.w * b.w - dot(a.xyz, b.xyz));
}
vec4 rotateQuatbyAngle(vec4 quat, vec3 angle) {
    float s = length(angle);
    if (s < 0.00001) return quat;
    return multQuat(quat, vec4(angle * (sin(s * 0.5) / s), cos(s * 0.5)));
}
vec3 rotAx(vec3 p, vec3 a) {
    vec4 q = rotateQuatbyAngle(vec4(0.0, 0.0, 0.0, 1.0), a);
    return p + 2.0 * cross(q.xyz, cross(q.xyz, p) + q.w * p);
}

// ── Matrix helpers ───────────────────────────────────────────────────────────
mat2 mrot2(float a) { float c = cos(a), s = sin(a); return mat2(c, s, -s, c); }
mat3 rotX(float a)  { float c = cos(a), s = sin(a); return mat3(1.0,0.0,0.0, 0.0,c,s, 0.0,-s,c); }
mat3 rotZ(float a)  { float c = cos(a), s = sin(a); return mat3(c,s,0.0, -s,c,0.0, 0.0,0.0,1.0); }
mat3 rotY(float a)  { float c = cos(a), s = sin(a); return mat3(c,0.0,-s, 0.0,1.0,0.0, s,0.0,c); }

// ── Sun position ─────────────────────────────────────────────────────────────
vec3 getSun() {
    float phi  = 0.2 * TIME * orbitSpeed * 2.0;
    float phi2 = 0.7 * cos(phi);
    return vec3(vec2(cos(phi), sin(phi)) * cos(phi2), sin(phi2));
}

// ── Warp a sphere position (twist + pinch + noise) ───────────────────────────
vec3 warpPos(vec3 p) {
    vec3 w = p;
    // Twist around Y
    if (abs(warpTwist) > 0.001) {
        float twist = warpTwist * w.y;
        w.xz = mrot2(twist) * w.xz;
    }
    // Pinch/Bulge: scale xz by radial factor
    if (abs(warpPinch) > 0.001) {
        float lat = asin(clamp(w.y / (length(w) + 0.0001), -1.0, 1.0));
        float pf  = 1.0 + warpPinch * cos(lat * 2.0) * 0.5;
        w.xz *= pf;
    }
    // Noise warp
    if (warpNoise > 0.001) {
        vec2 uvn = vec2(atan(w.z, w.x) / (2.0 * PI), asin(clamp(w.y / (length(w) + 0.0001), -1.0, 1.0)) / PI + 0.5);
        float nx = vnoise(uvn * warpNoiseScale + vec2(0.3, 0.7));
        float ny = vnoise(uvn * warpNoiseScale + vec2(1.7, 2.3));
        float nz = vnoise(uvn * warpNoiseScale + vec2(3.1, 0.9));
        w += vec3(nx, ny, nz) * warpNoise * 0.35;
    }
    return w;
}

// ── CFD helpers ──────────────────────────────────────────────────────────────
vec3 getRot(vec3 pos, vec3 b) {
    vec3 n  = normalize(pos);
    vec3 p  = normalize(b - dot(b, n) * n);
    vec3 rot = vec3(0.0);
    for (int i = 0; i < RotNum; i++) {
        vec2 fr = sph2frag(pos + p);
        vec3 v  = texture(bufA, fr / Res.xy).xyz - 0.5;
        rot    += cross(v * flowCurl, p);
        p = rotAx(p, n * ang);
    }
    return rot / float(RotNum) / dot(b, b);
}

// ── SDF & gradient ───────────────────────────────────────────────────────────
float dist(vec3 pos) {
    vec3 wp = warpPos(pos);
    vec3 disp = (texture(bufA, sph2frag(pos) / RENDERSIZE.xy).xyz - 0.5) * displacement;
    return length(wp + disp) / (blobScale / gZoomDiv) - 1.0;
}
vec3 getGrad(vec3 pos, float eps) {
    vec2 d = vec2(eps, 0.0);
    float d0 = dist(pos);
    return vec3(dist(pos + d.xyy) - d0,
                dist(pos + d.yxy) - d0,
                dist(pos + d.yyx) - d0) / eps;
}

// ── Raymarch ─────────────────────────────────────────────────────────────────
vec4 march(inout vec3 pos, vec3 dir) {
    if (length(pos - dir * dot(dir, pos)) > 1.4 * (blobScale / gZoomDiv)) return vec4(0.0, 0.0, 1.0, 1.0);
    float eps = 0.001, bg = 1.0;
    for (int cnt = 0; cnt < 120; cnt++) {
        float d = dist(pos);
        pos += d * dir * 0.65;
        if (d < eps) { bg = 0.0; break; }
    }
    vec3 gradEps = vec3(mix(0.001, 0.05, surfaceRoughness));
    vec3 n = getGrad(pos, gradEps.x);
    if (dot(n, n) < 0.0001) n = vec3(0.0, 0.0, 1.0);
    return vec4(n, bg);
}

// ════════════════════════════════════════════════════════════════════════
void main() {

    // ─────────────────────────────────────────────────────────────────────
    // PASS 0 — Buffer A: CFD advection step
    // ─────────────────────────────────────────────────────────────────────
    if (PASSINDEX == 0) {
        vec3 pos = frag2sph(gl_FragCoord.xy);
        if (pos.x > 10.0) discard;
        vec3 n = normalize(pos);

        float t = TIME * simSpeed;
        vec3 b = randS(vec2(float(FRAMEINDEX) / Res.x + t * 0.0001, 0.5 / Res1.y)).xyz;
        b = normalize(b - dot(b, n) * n) * 0.01;

        vec3 v = vec3(0.0);
        float bbMax = 5.0; bbMax *= bbMax;
        for (int l = 0; l < 20; l++) {
            if (dot(b, b) > bbMax) break;
            vec3 p2 = b;
            for (int i = 0; i < RotNum; i++) {
                float str = 0.01 * flowSpeed * flowIntensity * simSpeed;
                // Continuous advection-speed follow: mids quicken the mud's
                // flow (smooth band, no beat gating; silence = authored speed).
                str *= 1.0 + audioReact * 1.1 * smoothstep(0.04, 0.85, audioMid);
                str *= pow(dot(b, b), 0.25) * 5.0;
                v += cross(getRot(pos + p2, b), p2) * str;
                p2 = rotAx(p2, n * ang);
            }
            b *= 2.0;
        }

        vec4 fragColor = texture(bufA, sph2frag(pos + v) / RENDERSIZE.xy);
        fragColor.xyz = mix(fragColor.xyz, v * 5.0 + 0.5, 0.01 * simSpeed);

        vec3 sun = getSun();
        vec3 vel = vec3(sun.yx * vec2(1.0, -1.0), 0.0) * 0.5 * sunInfluence;
        fragColor.xyz = mix(fragColor.xyz, vel + 0.5,
            sunInfluence * 0.01 / (dot(pos - sun, pos - sun) / 0.2 + 0.2));

        if (FRAMEINDEX <= 4)
            fragColor = mix(vec4(0.5), hash4(gl_FragCoord.xy * 0.5), 0.2);

        gl_FragColor = fragColor;
        return;
    }

    // ─────────────────────────────────────────────────────────────────────
    // PASS 1 — Image: raymarch + shade
    // ─────────────────────────────────────────────────────────────────────
    vec2 sc = (gl_FragCoord.xy / RENDERSIZE.xy) * 2.0 - 1.0;

    // Bass swells the planet toward camera (dominant-structure zoom,
    // Milkdrop-style breathing); a beat adds a short extra shove. Idle
    // floor: audio 0 -> gZoomDiv 1.0 -> exactly the authored scale.
    float aB = audioReact * aBassP();
    float aBeat = audioReact * aBeatP();
    gZoomDiv = 1.0 + 0.55 * aB + 0.4 * aBeat;

    vec3 pos = vec3(0.0, -cameraDistance, 0.0);
    vec3 dir = normalize(cameraFOV * normalize(-pos) +
                         vec3(sc.x, 0.0, sc.y * RENDERSIZE.y / RENDERSIZE.x));

    float phi = TIME * 0.5 * orbitSpeed;
    float th  = cameraOrbitX * 0.5 * TIME * orbitSpeed;
    mat3 rx = rotX(th);
    mat3 rz = rotZ(phi);
    pos = rz * (rx * pos);
    dir = rz * (rx * dir);
    vec3 eye = pos;

    vec3 sun = getSun();

    vec4 nm = march(pos, dir);
    float bg = nm.w;

    vec3 poss = pos + sun * 0.01;
    vec4 n2 = march(poss, sun);
    float shadow = n2.w;

    // AO
    float ao = 1.0;
    if (bg < 0.5) {
        float aoDist = dist(pos + normalize(getGrad(pos, 0.1)) * 0.1) / 0.1;
        ao = mix(1.0, clamp(aoDist, 0.0, 1.0), aoStrength);
    }

    // Reflections
    vec3 R = pos - 2.0 * dot(pos, nm.xyz) * nm.xyz;
    R = -((R * rz) * rx).yzx;

    // Diffuse
    float diff = clamp(clamp(dot(sun, nm.xyz) * 2.0, 0.0, 1.0) * shadow + 0.3, 0.0, 1.5);

    // Fresnel
    float fres = 1.0 - dot(normalize(eye - pos), nm.xyz);
    fres = mix(0.1, 1.0, clamp(fres, 0.0, 1.0));
    fres = mix(0.1, 0.9 * fres, fresnelStr);

    // Specular
    float spec = pow(max(0.0, dot(R, sun)), mix(0.2, 2.0, specPower));

    // Color composite
    vec3 c = vec3(1.0);
    c += normalize(nm.xyz) * 0.1 + 0.1;
    c *= 0.8;
    c += clamp(spec * fres, 0.0, 1.0) * highColor.rgb;

    // Highs: sparse specular glints on the fresnel rim only — silence keeps
    // the clean surface, sound peppers a few extra glints across it.
    c += clamp(spec * fres, 0.0, 1.0) * highColor.rgb * audioReact * 1.6 * aHighP();

    if (bg > 0.5) { diff = 0.0; ao = 1.0; }

    // Tone by diffuse: lerp deep -> mid -> highlight
    float t2 = clamp(diff * 0.8, 0.0, 1.0);
    vec3 baseCol = mix(deepColor.rgb, midColor.rgb, t2);
    c = mix(c * 0.6, baseCol * diff, 1.0 - diff * 0.4);
    c *= ao;

    if (texMix > 0.001 && bg < 0.5) {
        // Map the texture onto the planet as albedo via spherical (lat/long)
        // projection of the surface normal, lit by the same diffuse/AO the
        // rest of the surface uses — reads as a textured planet, not a decal.
        vec3 nn = normalize(nm.xyz);
        vec2 texUV = vec2(atan(nn.z, nn.x) / (2.0 * PI) + 0.5, acos(clamp(nn.y, -1.0, 1.0)) / PI);
        vec3 texCol = texture2D(inputTex, texUV).rgb;
        vec3 texShaded = texCol * (diff * 0.7 + 0.3) * ao;
        c = mix(c, texShaded, texMix);
    }

    // Sun glow
    float sdot = clamp(dot(sun, dir), 0.0, 1.0);
    float sunmix = pow(sdot, 600.0) * 2.0 + 0.3 * sdot;
    c = c * (0.8 - 2.0 * sunmix) + sunColor.rgb * bg * sunmix;

    // User background: blend the space/void region toward the chosen color
    // (corona + audio rim still composite on top below).
    if (bg > 0.5) c = mix(c, bgColor.rgb, bgColor.a);

    // Audio corona: a soft energy rim breathing around the planet's
    // silhouette against the black — bass swells it, a beat kicks a bright
    // pulse. Only lives on background pixels; silent = no ring at all.
    if (bg > 0.5) {
        float rad = length(sc);
        float edge = 0.62 / (1.0 + 0.55 * aB + 0.35 * aBeat);
        float ring = exp(-pow(max(rad - edge, 0.0) * 5.5, 2.0));
        c += midColor.rgb * ring * (0.9 * aB + 1.4 * aBeat);
    }

    // Continuous band-follow (ambient fix): smooth bass lifts the planet's
    // luminance, mids add a smaller second phase — no beat gating, so
    // beatless swells read as breathing light. Silence: exactly 1.0.
    float aFolB = audioReact * smoothstep(0.02, 0.85, audioBass);
    float aFolM = audioReact * smoothstep(0.04, 0.85, audioMid);
    c *= 1.0 + 0.7 * aFolB + 0.35 * aFolM;

    // Vignette
    float vign = mix(1.0, 1.06 - length(sc.xy) * 0.5, vignette);

    // Exposure + gamma
    c = c * vign * exposure;
    c = pow(max(c, vec3(0.0)), vec3(1.0 / max(gamma, 0.01)));

    // ---- universal color block (defaults = no-op) ----
    vec3 uc = c;
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
    c = uc;

    gl_FragColor = vec4(c, 1.0);
}