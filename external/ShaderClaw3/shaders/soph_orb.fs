/*{
  "CATEGORIES": [
    "Generator",
    "3D",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Sophic Orb — a single contemplative central orb rendered with curatorial conviction. Four surface treatments (Iridescent Pearl, Living Wood, Plasma Core, Marble + Gold Kintsugi) each carry their own material logic: thin-film interference, slow Voronoi morph, volumetric inner turbulence, or veined classical stone. Bass breathes the orb scale, mids drive the surface morph rate, treble ignites micro-flickers across the silhouette. Drifting mote field surrounds; complementary deep gradient backdrop. Returns LINEAR HDR.",
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
      "NAME": "mood",
      "LABEL": "Surface Treatment",
      "TYPE": "long",
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Iridescent Pearl",
        "Living Wood",
        "Plasma Core",
        "Marble + Gold"
      ],
      "DEFAULT": 0
    },
    {
      "NAME": "highlightStrength",
      "LABEL": "Specular Key",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "orbScale",
      "LABEL": "Orb Size",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 1.4,
      "DEFAULT": 0.95,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "moteDensity",
      "LABEL": "Mote Field",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "morphSpeed",
      "LABEL": "Surface Morph",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.6,
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
      "NAME": "exposure",
      "LABEL": "Exposure",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "kintsugiGold",
      "LABEL": "Kintsugi Gold",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2.5,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "pearlIridescence",
      "LABEL": "Pearl Iridescence",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
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
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
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
      "NAME": "camOrbitSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.18,
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
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ---- universal color block (defaults = no-op) ----
vec3 ucApply(vec3 uc) {
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                      // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    return uc;
}


#define MAX_STEPS 80
#define MAX_DIST  20.0
#define EPS       0.0009
#define PI        3.14159265

float hash11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float hash21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
float hash31(vec3 p)  { return fract(sin(dot(p, vec3(127.1, 311.7, 74.7))) * 43758.5453); }

float vnoise(vec3 p) {
    vec3 i = floor(p), f = fract(p);
    f = f*f*(3.0 - 2.0*f);
    float n000 = hash31(i + vec3(0,0,0));
    float n100 = hash31(i + vec3(1,0,0));
    float n010 = hash31(i + vec3(0,1,0));
    float n110 = hash31(i + vec3(1,1,0));
    float n001 = hash31(i + vec3(0,0,1));
    float n101 = hash31(i + vec3(1,0,1));
    float n011 = hash31(i + vec3(0,1,1));
    float n111 = hash31(i + vec3(1,1,1));
    return mix(
        mix(mix(n000, n100, f.x), mix(n010, n110, f.x), f.y),
        mix(mix(n001, n101, f.x), mix(n011, n111, f.x), f.y),
        f.z);
}

float fbm(vec3 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 5; i++) { v += a * vnoise(p); p *= 2.02; a *= 0.5; }
    return v;
}

float voronoi3(vec3 p) {
    vec3 i = floor(p), f = fract(p);
    float md = 1.0;
    for (int x = -1; x <= 1; x++)
    for (int y = -1; y <= 1; y++)
    for (int z = -1; z <= 1; z++) {
        vec3 g = vec3(x,y,z);
        vec3 o = vec3(hash31(i + g + vec3(1.0,0.0,0.0)),
                      hash31(i + g + vec3(0.0,1.0,0.0)),
                      hash31(i + g + vec3(0.0,0.0,1.0)));
        vec3 r = g + o - f;
        md = min(md, dot(r, r));
    }
    return sqrt(md);
}

float bassEnv()  { return clamp(audioBass, 0.0, 1.0); }
float midEnv()   { return clamp(audioMid,  0.0, 1.0); }
float trebleEnv(){ return clamp(audioHigh, 0.0, 1.0); }

float sdOrb(vec3 p, float r) { return length(p) - r; }

vec3 orbNormal(vec3 p, float r) {
    const vec2 e = vec2(EPS, 0.0);
    return normalize(vec3(
        sdOrb(p + e.xyy, r) - sdOrb(p - e.xyy, r),
        sdOrb(p + e.yxy, r) - sdOrb(p - e.yxy, r),
        sdOrb(p + e.yyx, r) - sdOrb(p - e.yyx, r)));
}

vec3 thinFilm(float ndv, float thickness, float irid) {
    float phase = (1.0 - ndv) * thickness * 18.0 * irid;
    vec3 col;
    col.r = 0.5 + 0.5 * cos(phase + 0.0);
    col.g = 0.5 + 0.5 * cos(phase + 2.094);
    col.b = 0.5 + 0.5 * cos(phase + 4.188);
    vec3 base = vec3(0.92, 0.88, 0.82);
    return mix(base, col, clamp(0.40 + 0.45 * irid, 0.0, 1.0));
}

vec3 surfacePearl(vec3 p, vec3 n, vec3 v, float t) {
    float ndv = clamp(dot(n, v), 0.0, 1.0);
    float swirl = fbm(p * 2.4 + vec3(0.0, t * 0.18, 0.0));
    float thick = 0.65 + 0.55 * swirl;
    vec3 ir = thinFilm(ndv, thick, pearlIridescence);
    ir *= mix(vec3(0.85, 0.92, 1.05), vec3(1.10, 0.90, 0.95), swirl);
    return ir;
}

vec3 surfaceWood(vec3 p, vec3 n, float t) {
    vec3 q = p * 5.5 + vec3(0.0, 0.0, t * 0.12);
    q += 0.35 * vec3(fbm(p * 2.0 + t * 0.07),
                     fbm(p * 2.0 + 7.3 + t * 0.07),
                     fbm(p * 2.0 + 13.1));
    float v1 = voronoi3(q);
    float v2 = voronoi3(q * 1.7 + 4.1);
    float grain = smoothstep(0.0, 0.18, v1) * smoothstep(0.0, 0.30, v2);
    vec3 walnut = vec3(0.32, 0.18, 0.10);
    vec3 umber  = vec3(0.55, 0.32, 0.16);
    vec3 dark   = vec3(0.10, 0.06, 0.04);
    vec3 col    = mix(dark, walnut, grain);
    col = mix(col, umber, smoothstep(0.45, 0.85, fbm(p * 4.0 + t * 0.05)));
    return col;
}

vec3 surfacePlasma(vec3 p, vec3 n, vec3 v, float t) {
    vec3 inner = p - n * 0.18;
    float turb = fbm(inner * 3.8 + vec3(0.0, t * 0.6, 0.0));
    turb = pow(turb, 1.4);
    vec3 core   = vec3(2.40, 1.20, 0.35);
    vec3 mid    = vec3(1.50, 0.45, 0.10);
    vec3 shell  = vec3(0.45, 0.10, 0.04);
    vec3 col    = mix(shell, mid, turb);
    col         = mix(col, core, smoothstep(0.55, 0.85, turb));
    float ndv = clamp(dot(n, v), 0.0, 1.0);
    col += core * pow(1.0 - ndv, 4.0) * 0.6;
    return col;
}

vec3 surfaceMarble(vec3 p, vec3 n, float t) {
    float base   = fbm(p * 3.0 + vec3(0.0, t * 0.04, 0.0));
    float veinF  = fbm(p * 8.0 + vec3(t * 0.03));
    float ridge  = 1.0 - abs(2.0 * veinF - 1.0);
    float vein   = pow(smoothstep(0.55, 0.95, ridge), 6.0);
    vec3 stone   = mix(vec3(0.92, 0.90, 0.86), vec3(0.78, 0.76, 0.72), base * 0.55);
    vec3 gold    = vec3(1.45, 1.05, 0.45) * kintsugiGold;
    return mix(stone, gold, vein * clamp(kintsugiGold, 0.0, 1.0));
}

vec3 surface(vec3 p, vec3 n, vec3 v, int moodID, float t) {
    if (moodID == 0) return surfacePearl(p, n, v, t);
    if (moodID == 1) return surfaceWood(p, n, t);
    if (moodID == 2) return surfacePlasma(p, n, v, t);
    return surfaceMarble(p, n, t);
}

vec3 backdrop(vec2 uv, int moodID) {
    float vy = uv.y;
    vec3 top, bot;
    if (moodID == 0)      { top = vec3(0.025, 0.018, 0.045); bot = vec3(0.10, 0.05, 0.12); }
    else if (moodID == 1) { top = vec3(0.020, 0.040, 0.060); bot = vec3(0.05, 0.10, 0.14); }
    else if (moodID == 2) { top = vec3(0.020, 0.030, 0.060); bot = vec3(0.04, 0.05, 0.09); }
    else                  { top = vec3(0.030, 0.020, 0.050); bot = vec3(0.10, 0.07, 0.05); }
    return mix(bot, top, smoothstep(0.0, 1.0, vy));
}

vec3 moteField(vec2 ndc, float t, float density, int moodID, float treble) {
    if (density <= 0.001) return vec3(0.0);
    vec3 acc = vec3(0.0);
    vec3 tint = (moodID == 2) ? vec3(1.30, 0.85, 0.55)
              : (moodID == 1) ? vec3(0.85, 0.78, 0.55)
              : (moodID == 3) ? vec3(1.20, 1.00, 0.70)
                              : vec3(0.85, 0.92, 1.10);
    for (int layer = 0; layer < 3; layer++) {
        float fl = float(layer);
        float scale = mix(40.0, 110.0, fl / 2.0);
        vec2 q = ndc * scale + vec2(t * (0.05 + 0.04 * fl), -t * (0.03 + 0.02 * fl));
        vec2 i = floor(q), f = fract(q);
        float h = hash21(i + fl * 17.3);
        if (h > 0.96) {
            vec2 c = vec2(0.5) + 0.4 * vec2(hash21(i + fl * 3.7) - 0.5,
                                            hash21(i + fl * 9.1) - 0.5);
            float d = length(f - c);
            float r = mix(0.06, 0.14, hash21(i + fl * 23.5));
            float twinkle = 0.5 + 0.5 * sin(t * (1.5 + 3.0 * h) + h * 30.0);
            float lum = smoothstep(r, 0.0, d) * twinkle;
            acc += tint * lum * (0.20 + 0.5 * fl * 0.4);
        }
    }
    return acc * density * (1.0 + 0.6 * treble);
}

void main() {
    vec2 uv  = isf_FragNormCoord.xy;
    float aspect = RENDERSIZE.x / RENDERSIZE.y;
    vec2 ndc = (uv * 2.0 - 1.0) * vec2(aspect, 1.0);

    int   moodID = int(mood + 0.5);
    float t      = TIME;
    float ar     = clamp(audioReact, 0.0, 2.0);
    float bass   = bassEnv()   * ar;
    float mid    = midEnv()    * ar;
    float treble = trebleEnv() * ar;

    float baseRadius = 0.92 * orbScale;
    float breath     = 1.0 + 0.06 * sin(t * 0.7) + 0.10 * bass;
    float r          = baseRadius * breath;

    // Universal orbiting camera: azimuth + height look at origin.
    float az = camAzimuth + t * camOrbitSpeed;
    vec3 ro  = vec3(sin(az) * camDist, camHeight, cos(az) * camDist);
    vec3 fwd = normalize(-ro);
    vec3 right = normalize(cross(fwd, vec3(0.0, 1.0, 0.0)));
    vec3 up    = cross(right, fwd);
    vec3 rd    = normalize(ndc.x * right + ndc.y * up + 1.7 * fwd);

    float morphT = t * (morphSpeed * (0.6 + 1.4 * mid) + 0.10);

    float dist = 0.0;
    bool hit = false;
    for (int i = 0; i < MAX_STEPS; i++) {
        float d = sdOrb(ro + rd * dist, r);
        if (d < EPS) { hit = true; break; }
        dist += d;
        if (dist > MAX_DIST) break;
    }

    vec3 col = backdrop(uv, moodID);
    col = mix(col, bgColor.rgb, bgColor.a);   // universal background (a=0 -> no-op)

    if (hit) {
        vec3 p = ro + rd * dist;
        vec3 n = orbNormal(p, r);
        vec3 v = -rd;

        vec3 albedo = surface(p, n, v, moodID, morphT);

        // Universal lighting from azimuth/elevation angles.
        float ce = cos(keyElevation);
        vec3 keyDir = normalize(vec3(cos(keyAngle) * ce, sin(keyElevation), sin(keyAngle) * ce));
        vec3 fillDir = normalize(vec3(-keyDir.x, max(0.1, keyDir.y * 0.4), -keyDir.z));

        float ndl  = max(dot(n, keyDir), 0.0);
        float ndlF = max(dot(n, fillDir), 0.0);
        float ndv  = clamp(dot(n, v), 0.0, 1.0);

        float rough = (moodID == 0) ? 0.22
                     : (moodID == 1) ? 0.55
                     : (moodID == 2) ? 0.70
                                     : 0.30;
        vec3  H   = normalize(keyDir + v);
        float ndh = max(dot(n, H), 0.0);
        float a2  = rough * rough;
        float denom = (ndh*ndh) * (a2 - 1.0) + 1.0;
        float D   = a2 / (PI * denom * denom + 1e-5);
        float fres = pow(1.0 - ndv, 5.0);
        float F   = 0.04 + 0.96 * fres;
        float spec = D * F * highlightStrength;

        float rim = pow(1.0 - ndv, 3.0);
        vec3  rimTint = (moodID == 2) ? vec3(2.20, 0.80, 0.30)
                       : (moodID == 0) ? vec3(0.85, 0.95, 1.15)
                       : (moodID == 3) ? vec3(1.30, 1.10, 0.70)
                                        : vec3(0.85, 0.65, 0.40);

        float sparkleSeed = hash31(floor(p * 90.0));
        float flick = step(0.985, sparkleSeed) * (0.5 + 0.5 * sin(t * 22.0 + sparkleSeed * 31.0));
        flick *= treble * 1.6;

        vec3 lit = albedo * ambient
                 + albedo * ndl  * keyColor.rgb
                 + albedo * 0.25 * ndlF * fillColor.rgb;
        lit += keyColor.rgb * spec * 1.4;
        lit += rimTint * rim * rimStrength;
        lit += vec3(1.30, 1.20, 1.00) * flick;

        if (moodID == 2) lit += albedo * 0.45;

        col = lit;
    }

    if (!hit) {
        col += moteField(ndc, t, moteDensity, moodID, treble);
    }

    float vig = 1.0 - 0.30 * dot(ndc * 0.55, ndc * 0.55);
    col *= vig;

    col *= exposure;
    col = max(col, 0.0);
    col = ucApply(col);
    gl_FragColor = vec4(col, 1.0);
}
