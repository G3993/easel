/*{
  "CATEGORIES": [
    "3D",
    "Generator",
    "Audio Reactive"
  ],
  "DESCRIPTION": "PBR Glass Dispersion 3D — refractive crystal sculpture with per-channel index of refraction producing prism-style rainbow dispersion. SDF glass (twisted torus + sphere smooth-union) over a colored grid floor; Fresnel mix between reflected sky and refracted floor; specular highlight + rim halo on the glass surface. Audio modulates twist and dispersion strength. Returns LINEAR HDR — host applies ACES.",
  "INPUTS": [
    {
      "NAME": "keyAngle",
      "LABEL": "Key Light Angle",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6.2832,
      "DEFAULT": 0.785
    },
    {
      "NAME": "keyElevation",
      "LABEL": "Key Elevation",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5708,
      "DEFAULT": 0.7
    },
    {
      "NAME": "ambient",
      "LABEL": "Ambient",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0.08
    },
    {
      "NAME": "rimStrength",
      "LABEL": "Rim Strength",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.5
    },
    {
      "NAME": "exposure",
      "LABEL": "Exposure",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 3,
      "DEFAULT": 1
    },
    {
      "NAME": "iorBase",
      "LABEL": "IOR (Green)",
      "TYPE": "float",
      "MIN": 1.1,
      "MAX": 1.8,
      "DEFAULT": 1.45
    },
    {
      "NAME": "internalAbsorption",
      "LABEL": "Internal Absorption",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 0.4
    },
    {
      "NAME": "causticBoost",
      "LABEL": "Caustic Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2.5,
      "DEFAULT": 0
    },
    {
      "NAME": "twist",
      "LABEL": "Glass Twist",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2.5,
      "DEFAULT": 0.6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "camOrbitSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.18,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "keyColor",
      "LABEL": "Key Light",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.94,
        0.82,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "fillColor",
      "LABEL": "Fill Light",
      "TYPE": "color",
      "DEFAULT": [
        0.55,
        0.7,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "dispersion",
      "LABEL": "Dispersion",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.15,
      "DEFAULT": 0.045,
      "GROUP": "Color"
    },
    {
      "NAME": "gridA",
      "LABEL": "Grid Color A",
      "TYPE": "color",
      "DEFAULT": [
        1,
        0.42,
        0.28,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "gridB",
      "LABEL": "Grid Color B",
      "TYPE": "color",
      "DEFAULT": [
        0.28,
        0.55,
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
      "NAME": "camDist",
      "LABEL": "Camera Distance",
      "TYPE": "float",
      "MIN": 1.5,
      "MAX": 12,
      "DEFAULT": 4.5,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "camHeight",
      "LABEL": "Camera Height",
      "TYPE": "float",
      "MIN": -3,
      "MAX": 4,
      "DEFAULT": 1.2,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "camAzimuth",
      "LABEL": "Camera Azimuth",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6.2832,
      "DEFAULT": 0,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0.03,
        0.04,
        0.07,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  PBR Glass Dispersion — chromatic-aberration glass prism reference.
//  Per-RGB IOR triple-refracts through SDF glass into a colored grid
//  floor + sky gradient. Fresnel-mixed with reflected environment.
//  Returns LINEAR HDR.
// ════════════════════════════════════════════════════════════════════════

#define MAX_STEPS 80
#define MAX_DIST  30.0
#define EPS       0.0008

// ─── SDF primitives ────────────────────────────────────────────────────
float sdSphere(vec3 p, float r) { return length(p) - r; }
float sdTorus (vec3 p, vec2 t)  { vec2 q = vec2(length(p.xz) - t.x, p.y); return length(q) - t.y; }

// ─── ops ───────────────────────────────────────────────────────────────
float opSmoothUnion(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

vec3 twistY(vec3 p, float k) {
    float c = cos(k * p.y), s = sin(k * p.y);
    p.xz = mat2(c, -s, s, c) * p.xz;
    return p;
}

// ─── Glass SDF: twisted torus smooth-unioned with a bobbing sphere ─────
float sdfGlass(vec3 p, float twistK, float audioPulse) {
    vec3  q = twistY(p, twistK);
    float t = sdTorus(q, vec2(0.95, 0.32 + 0.06 * audioPulse));
    float s = sdSphere(p - vec3(0.0, sin(TIME * 0.6) * 0.08, 0.0), 0.62);
    return opSmoothUnion(t, s, 0.4);
}

// ─── Background sampler (floor + sky), used by primary rays AND
// per-channel refracted rays. The world without the glass.
vec3 floorColor(vec3 p, vec3 cA, vec3 cB) {
    vec2  g  = p.xz * 0.5;
    vec2  dv = fwidth(g);
    vec2  qq = smoothstep(vec2(0.0), dv, fract(g))
             - smoothstep(vec2(1.0) - dv, vec2(1.0), fract(g));
    float chk    = qq.x * qq.y + (1.0 - qq.x) * (1.0 - qq.y);
    float radial = exp(-length(p.xz) * 0.18);
    return mix(cA, cB, chk) * (0.45 + 0.55 * radial);
}

vec3 sky(vec3 rd, vec3 base) {
    float h = clamp(rd.y * 0.5 + 0.5, 0.0, 1.0);
    return mix(base * 0.4, base * 1.6, h)
         + vec3(0.06, 0.05, 0.04) * pow(max(rd.y, 0.0), 4.0);
}

vec3 sampleBackground(vec3 ro, vec3 rd, vec3 cA, vec3 cB, vec3 bg) {
    if (rd.y < -1e-4) {
        float t = (-1.6 - ro.y) / rd.y;
        if (t > 0.0) {
            vec3  hp   = ro + rd * t;
            float dist = length(hp - ro);
            float fog  = 1.0 - exp(-dist * 0.045);
            return mix(floorColor(hp, cA, cB), bg, fog);
        }
    }
    return sky(rd, bg);
}

// ─── Normal via 4-tap tetrahedron ─────────────────────────────────────
vec3 calcNormal(vec3 p, float twistK, float audioPulse) {
    const vec2 e = vec2(0.0009, -0.0009);
    return normalize(
        e.xyy * sdfGlass(p + e.xyy, twistK, audioPulse) +
        e.yyx * sdfGlass(p + e.yyx, twistK, audioPulse) +
        e.yxy * sdfGlass(p + e.yxy, twistK, audioPulse) +
        e.xxx * sdfGlass(p + e.xxx, twistK, audioPulse));
}

// ─── March the glass SDF (slight under-step for smoother surfaces) ────
float marchGlass(vec3 ro, vec3 rd, float twistK, float audioPulse) {
    float d = 0.0;
    for (int i = 0; i < MAX_STEPS; i++) {
        vec3  p    = ro + rd * d;
        float dist = sdfGlass(p, twistK, audioPulse);
        if (abs(dist) < EPS) return d;
        if (d > MAX_DIST)    break;
        d += dist * 0.85;
    }
    return -1.0;
}

// ─── Per-channel refraction sampler (the dispersion happens here) ─────
vec3 refractedRGB(vec3 entry, vec3 rd, vec3 n,
                  float iorR, float iorG, float iorB,
                  vec3 cA, vec3 cB, vec3 bg) {
    vec3 rdR = refract(rd, n, 1.0 / iorR);
    vec3 rdG = refract(rd, n, 1.0 / iorG);
    vec3 rdB = refract(rd, n, 1.0 / iorB);
    // Fall back to the incoming direction if total internal reflection ever
    // returns a zero vector (shouldn't on entry, but harmless guard).
    rdR = (dot(rdR, rdR) < 0.001) ? rd : rdR;
    rdG = (dot(rdG, rdG) < 0.001) ? rd : rdG;
    rdB = (dot(rdB, rdB) < 0.001) ? rd : rdB;
    return vec3(
        sampleBackground(entry, rdR, cA, cB, bg).r,
        sampleBackground(entry, rdG, cA, cB, bg).g,
        sampleBackground(entry, rdB, cA, cB, bg).b);
}

// ─── main ─────────────────────────────────────────────────────────────
void main() {
    vec2 uv = (isf_FragNormCoord.xy * 2.0 - 1.0)
            * vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0);

    // Wire real audio band into the reactivity knob — previously this only
    // read the static audioReact param, never the live audioBass signal, so
    // the shader was completely dead to actual sound regardless of level.
    float audioLvl   = clamp(audioBass, 0.0, 1.0);
    float audio      = clamp(audioReact, 0.0, 2.0) * (0.35 + 0.65 * audioLvl);
    float audioPulse = sin(TIME * 1.4) * 0.5 * audio;
    float twistK     = twist + 0.2 * audio;

    // Camera orbit
    float ang = camAzimuth + TIME * camOrbitSpeed;
    vec3  ro  = vec3(cos(ang) * camDist, camHeight, sin(ang) * camDist);
    vec3  ta  = vec3(0.0, 0.1, 0.0);
    vec3  fw  = normalize(ta - ro);
    vec3  ri  = normalize(cross(vec3(0.0, 1.0, 0.0), fw));
    vec3  up  = cross(fw, ri);
    vec3  rd  = normalize(fw + uv.x * ri + uv.y * up);

    // Default: see right through to the world
    vec3 col = sampleBackground(ro, rd, gridA.rgb, gridB.rgb, bgColor.rgb);

    float gd = marchGlass(ro, rd, twistK, audioPulse);
    if (gd > 0.0) {
        vec3 hp = ro + rd * gd;
        vec3 n  = calcNormal(hp, twistK, audioPulse);

        float cosTh = clamp(-dot(rd, n), 0.0, 1.0);
        float F0    = pow((1.0 - iorBase) / (1.0 + iorBase), 2.0);
        float fr    = F0 + (1.0 - F0) * pow(1.0 - cosTh, 5.0);

        // Reflected lobe — environment specular
        vec3 reflRD  = reflect(rd, n);
        vec3 reflCol = sampleBackground(hp + n * 0.01, reflRD,
                                        gridA.rgb, gridB.rgb, bgColor.rgb);

        // Refracted lobe — per-channel IOR triple
        float disp = dispersion * (0.7 + 0.6 * audio);
        vec3 refrCol = refractedRGB(
            hp - n * 0.005, rd, n,
            iorBase - disp, iorBase, iorBase + disp,
            gridA.rgb, gridB.rgb, bgColor.rgb);

        // Beer-Lambert internal absorption — tint glass body by mean grid
        // color, depth-modulated by dispersion strength as a thickness proxy.
        vec3  tintCol  = mix(gridA.rgb, gridB.rgb, 0.5);
        float thickness = 0.6 + 4.0 * dispersion;
        vec3  absorb   = exp(-internalAbsorption * thickness * (1.0 - tintCol));
        refrCol *= absorb;

        col = mix(refrCol, reflCol, fr);

        // Caustic boost: extra brightness on refracted lobe near grazing
        col += refrCol * causticBoost * pow(1.0 - cosTh, 2.0) * 0.5;

        // Hard specular highlight (key light) — driven by keyAngle/keyElevation
        float ce = cos(keyElevation);
        vec3  keyDir = normalize(vec3(cos(keyAngle) * ce,
                                      sin(keyElevation),
                                      sin(keyAngle) * ce));
        float spec   = pow(max(dot(reflect(-keyDir, n), -rd), 0.0), 80.0);
        col += keyColor.rgb * spec * 0.65;

        // Diffuse key + ambient term (subtle on glass)
        float ndl = max(dot(n, keyDir), 0.0);
        col += keyColor.rgb * ndl * ambient * 0.3;
        col += fillColor.rgb * ambient;

        // Rim halo — fill-tinted thin glass edge brightening
        float rim = pow(1.0 - cosTh, 3.0);
        col += fillColor.rgb * rim * rimStrength * 0.36;
    }

    // Subtle vignette
    col *= 1.0 - 0.18 * length(uv * 0.6);

    // Final exposure — bass adds a touch of extra punch on top of the
    // caustic/dispersion response above so the whole piece breathes with it.
    col *= exposure * (1.0 + 0.35 * audioLvl);

    // ---- universal color block (defaults = no-op; bgColor already native) ----
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

    gl_FragColor = vec4(uc, 1.0);
}
