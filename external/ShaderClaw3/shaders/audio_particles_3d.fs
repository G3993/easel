/*{
  "CATEGORIES": [
    "3D",
    "Generator",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Audio Particles 3D — particle-as-subject in four curated moods. Anadol Cloud (electric haze of ~2000 implicit points), Gursky Repetition (rigid 3D lattice with subtle per-cell variance, after '99 Cent'), Memo Form (humanoid silhouette gestural morph, after 'Forms' 2012), Constellation (sparse HDR stars + connecting lines, after Onformative). Bass aggregates density; mids drive orbit speed; treble shimmers. Single-pass per-pixel ray-vs-particle accumulation; key/fill/ambient/rim lighting on diffuse moods, pure emissive on the constellation. Outputs LINEAR HDR multiplied by exposure — host applies tonemap.",
  "INPUTS": [
    {
      "NAME": "mood",
      "LABEL": "Mood",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Anadol Cloud",
        "Gursky Repetition",
        "Memo Form",
        "Constellation"
      ]
    },
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
      "NAME": "particleCount",
      "LABEL": "Particle Density",
      "TYPE": "long",
      "DEFAULT": 2,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Sparse",
        "Med",
        "Dense",
        "Storm"
      ],
      "GROUP": "Shape / Geometry"
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
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// AUDIO PARTICLES 3D — Anadol/Gursky/Memo/Onformative references. Single-
// pass ray-vs-particle field; universal camera + key/fill/amb/rim. LINEAR HDR.
// Mobile-safe loop caps (were 2000/512); point size + weight compensated below.
#define MAX_CLOUD 240
#define MAX_FORM  256
#define MAX_STARS 180

int   cloudCount(int t) { return t == 0 ? 90 : t == 1 ? 140 : t == 2 ? 190 : MAX_CLOUD; }
ivec3 gridDims  (int t) { return t == 0 ? ivec3(7,4,4) : t == 1 ? ivec3(8,5,5) : t == 2 ? ivec3(9,6,6) : ivec3(10,7,7); }
int   formCount (int t) { return t == 0 ? 140 : t == 1 ? 180 : t == 2 ? 220 : MAX_FORM; }
int   starCount (int t) { return t == 0 ?  80 : t == 1 ? 120 : t == 2 ? 150 : MAX_STARS; }

float h11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
vec3  h31(float n) {
    return vec3(fract(sin(n * 12.9898) * 43758.5453),
                fract(sin(n * 78.2330) * 43758.5453),
                fract(sin(n * 51.4419) * 43758.5453));
}

// ray-to-point squared distance; tca = signed parametric distance along ray.
float rpd2(vec3 ro, vec3 rd, vec3 p, out float tca) {
    vec3 oc = p - ro;
    tca = dot(oc, rd);
    vec3 c = ro + rd * tca;
    return dot(c - p, c - p);
}

// implicit-particle "normal" — direction from particle to ray's closest pt.
vec3 pNormal(vec3 ro, vec3 rd, vec3 p, float tca) {
    vec3 d = ro + rd * tca - p;
    float l = length(d);
    return l < 1e-5 ? -rd : d / l;
}
// PALETTES (LINEAR space, HDR-friendly) ─────────────────────────────────
vec3 palMix4(float t, vec3 c0, vec3 c1, vec3 c2, vec3 c3, vec3 c4) {
    float s = fract(t) * 4.0; int i = int(floor(s)); float f = fract(s);
    if (i == 0) return mix(c0, c1, f);
    if (i == 1) return mix(c1, c2, f);
    if (i == 2) return mix(c2, c3, f);
    return mix(c3, c4, f);
}
vec3 palMix3(float t, vec3 c0, vec3 c1, vec3 c2, vec3 c3) {
    float s = fract(t) * 3.0; int i = int(floor(s)); float f = fract(s);
    if (i == 0) return mix(c0, c1, f);
    if (i == 1) return mix(c1, c2, f);
    return mix(c2, c3, f);
}
vec3 anadol(float t)  { return palMix4(t,
    vec3(0.10,0.32,1.10), vec3(1.00,0.18,0.85), vec3(0.95,0.95,1.10),
    vec3(0.28,0.05,0.55), vec3(0.10,0.85,1.05)); }
vec3 gursky(float t)  { return palMix3(t,
    vec3(0.86,0.92,1.00), vec3(0.45,0.62,0.92),
    vec3(0.95,0.96,0.98), vec3(0.32,0.50,0.78)); }
vec3 memoP (float t)  { return palMix3(t,
    vec3(0.92,0.84,0.68), vec3(0.55,0.42,0.34),
    vec3(0.18,0.10,0.07), vec3(0.85,0.62,0.40)); }
vec3 starP (float t)  {
    float s = fract(t) * 2.0; int i = int(floor(s)); float f = fract(s);
    vec3 c0 = vec3(1.05,0.96,0.86), c1 = vec3(1.10,0.78,0.48), c2 = vec3(0.78,0.86,1.10);
    return i == 0 ? mix(c0, c1, f) : mix(c1, c2, f);
}

// MOOD POSITIONERS ──────────────────────────────────────────────────────
vec3 cloudPos(float i, float t, float bass) {
    vec3 r = h31(i + 1.7) * 2.0 - 1.0;
    float orbR = 0.5 + h11(i * 7.13) * 1.6;
    float spd  = 0.18 + h11(i * 11.7) * 0.55;
    float ph   = i * 0.731 + t * spd;
    vec3  ax = normalize(r + vec3(0.0, 0.001, 0.0));
    vec3  b  = normalize(cross(ax, vec3(0.0, 1.0, 0.0) + r * 0.15));
    vec3  c  = normalize(cross(ax, b));
    vec3  p  = ax * cos(ph) * orbR
             + b  * sin(ph * 1.21) * orbR * 0.78
             + c  * sin(ph * 0.67 + i * 0.13) * orbR * 0.62;
    float a = t * 0.12, ca = cos(a), sa = sin(a);
    p.xz = mat2(ca, -sa, sa, ca) * p.xz;
    return p * (1.0 - 0.85 * bass);   // bass aggregation
}
vec3 gridPos(int ix, int iy, int iz, ivec3 dims, float t, float aAll) {
    float fi = float(ix) + float(iy) * 17.0 + float(iz) * 113.0;
    vec3 base = vec3(float(ix) - float(dims.x - 1) * 0.5,
                     float(iy) - float(dims.y - 1) * 0.5,
                     float(iz) - float(dims.z - 1) * 0.5) * 0.42;
    base.y += (h11(fi * 0.71) - 0.5) * 0.25;     // per-cell height variance
    vec3 jit = (h31(fi) - 0.5) * 0.05 * (1.0 + aAll * 1.4);
    jit += 0.014 * vec3(sin(t * 0.7 + fi),
                        sin(t * 0.9 + fi * 1.3),
                        sin(t * 1.1 + fi * 0.7));
    return base + jit;
}
vec3 memoPos(float i, float t, float aAll) {
    float gT = t * 0.18, armS = sin(gT) * 0.55, legS = sin(gT + 1.5708) * 0.45, lean = sin(gT * 0.7) * 0.18;
    float fi = i, bk = h11(fi);
    vec3 anch; float spread; float h;
    if (bk < 0.10)      { anch = vec3(lean * 0.7, 0.95, 0.0); spread = 0.16; }
    else if (bk < 0.45) { h = h11(fi * 1.7) - 0.25; anch = vec3(lean * h, h, 0.0); spread = 0.20 + 0.07 * (1.0 - abs(h - 0.25)); }
    else if (bk < 0.65) { h = h11(fi * 2.3); anch = mix(vec3(-0.32, 0.55, 0.0), vec3(-0.55 + armS * 0.6, 0.55 - armS * 0.7, armS * 0.4), h); spread = 0.07 + 0.05 * h; }
    else if (bk < 0.85) { h = h11(fi * 3.1); anch = mix(vec3( 0.32, 0.55, 0.0), vec3( 0.55 - armS * 0.6, 0.55 + armS * 0.7, -armS * 0.4), h); spread = 0.07 + 0.05 * h; }
    else if (bk < 0.93) { h = h11(fi * 5.7); anch = mix(vec3(-0.14, -0.25, 0.0), vec3(-0.20, -1.10, legS * 0.45), h); spread = 0.09 + 0.04 * h; }
    else                { h = h11(fi * 6.9); anch = mix(vec3( 0.14, -0.25, 0.0), vec3( 0.20, -1.10, -legS * 0.45), h); spread = 0.09 + 0.04 * h; }
    vec3 jit = (h31(fi * 0.91) * 2.0 - 1.0) * spread * (1.0 + 0.25 * sin(t * 0.6 + fi * 0.31));
    jit += 0.05 * aAll * (h31(fi * 4.4) * 2.0 - 1.0);
    vec3 p = (anch + jit) * 0.95;
    float a = t * 0.08, ca = cos(a), sa = sin(a);
    p.xz = mat2(ca, -sa, sa, ca) * p.xz;
    return p;
}
vec3 starPosition(float i, float t) {
    vec3 d = normalize(h31(i * 1.31) * 2.0 - 1.0 + vec3(0.0001));
    float r = 1.6 + 0.30 * h11(i * 9.7);
    float a = t * 0.025 + i * 0.07;
    vec3 ax = normalize(h31(i * 17.0) * 2.0 - 1.0);
    float ca = cos(a), sa = sin(a);
    return (d * ca + cross(ax, d) * sa + ax * dot(ax, d) * (1.0 - ca)) * r;
}

// LIT SHADING (key + fill + ambient + rim) ──────────────────────────────
vec3 shade(vec3 base, vec3 n, vec3 v, vec3 kDir, vec3 kC, vec3 fC, float amb, float rimK) {
    float ndlK = max(dot(n, kDir), 0.0);
    vec3  fDir = normalize(vec3(-kDir.x, max(0.2, kDir.y * 0.5), -kDir.z));
    float ndlF = max(dot(n, fDir), 0.0);
    float rim  = pow(1.0 - max(dot(n, v), 0.0), 3.0);
    return base * (kC * ndlK + fC * ndlF * 0.55 + amb) + fC * rim * rimK * 0.35;
}

// EVALUATORS — return LINEAR HDR ────────────────────────────────────────
vec3 evalCloud(vec3 ro, vec3 rd, float t, vec3 a, int N,
               vec3 kDir, vec3 kC, vec3 fC, float amb, float rimK) {
    vec3 emit = vec3(0.0);
    float ps = 0.052, ps2 = ps * ps, cut = ps2 * 24.0;   // larger points compensate reduced count
    float sh = 1.0 + a.z * 3.0 * (0.5 + 0.5 * sin(t * 11.0 + ro.x * 2.0));
    for (int i = 0; i < MAX_CLOUD; i++) {
        if (i >= N) break;
        float fi = float(i);
        vec3 pp = cloudPos(fi, t, a.x);
        float tca; float d2 = rpd2(ro, rd, pp, tca);
        if (tca < 0.0 || d2 > cut) continue;
        float w  = exp(-d2 / (ps2 * 4.5)) * exp(-tca * 0.07);
        vec3 ct  = anadol(h11(fi * 3.7) + t * 0.012) * (0.7 + 0.6 * h11(fi * 1.91));
        vec3 lit = shade(ct * 0.6, pNormal(ro, rd, pp, tca), -rd, kDir, kC, fC, amb, rimK);
        emit += (ct * 1.4 + lit) * w * sh * 1.6;   // brightness compensation for lower count
    }
    return emit;
}
vec3 evalGrid(vec3 ro, vec3 rd, float t, vec3 a, ivec3 dims,
              vec3 kDir, vec3 kC, vec3 fC, float amb, float rimK) {
    vec3 emit = vec3(0.0);
    float ps = 0.030, ps2 = ps * ps, cut = ps2 * 18.0;
    float aAll = (a.x + a.y + a.z) * 0.33;
    for (int ix = 0; ix < 10; ix++) { if (ix >= dims.x) break;
    for (int iy = 0; iy <  7; iy++) { if (iy >= dims.y) break;
    for (int iz = 0; iz <  7; iz++) { if (iz >= dims.z) break;
        float fi = float(ix) + float(iy) * 17.0 + float(iz) * 113.0;
        vec3 pp = gridPos(ix, iy, iz, dims, t, aAll);
        float tca; float d2 = rpd2(ro, rd, pp, tca);
        if (tca < 0.0 || d2 > cut) continue;
        float w   = exp(-d2 / (ps2 * 3.2)) * exp(-tca * 0.06);
        vec3  ct  = gursky(h11(fi * 0.71));
        float fl  = 1.0 + a.z * 0.4 * (h11(fi + floor(t * 4.0)) - 0.5);
        vec3  lit = shade(ct, pNormal(ro, rd, pp, tca), -rd, kDir, kC, fC, amb, rimK);
        emit += lit * w * fl;
    }}}
    return emit;
}
vec3 evalMemo(vec3 ro, vec3 rd, float t, vec3 a, int N,
              vec3 kDir, vec3 kC, vec3 fC, float amb, float rimK) {
    vec3 emit = vec3(0.0);
    float ps = 0.036, ps2 = ps * ps, cut = ps2 * 22.0;   // compensate reduced form count
    float aAll = (a.x + a.y + a.z) * 0.33;
    for (int i = 0; i < MAX_FORM; i++) {
        if (i >= N) break;
        float fi = float(i);
        vec3 pp = memoPos(fi, t, aAll);
        float tca; float d2 = rpd2(ro, rd, pp, tca);
        if (tca < 0.0 || d2 > cut) continue;
        float w  = exp(-d2 / (ps2 * 4.0)) * exp(-tca * 0.08);
        float h  = clamp(pp.y * 0.45 + 0.5, 0.0, 1.0);
        vec3 ct  = memoP(h * 0.75 + 0.05);
        vec3 lit = shade(ct, pNormal(ro, rd, pp, tca), -rd, kDir, kC, fC, amb, rimK);
        emit += lit * w * (1.0 + a.z * 0.25);
    }
    return emit;
}
vec3 evalConstellation(vec3 ro, vec3 rd, float t, vec3 a, int N) {
    vec3 emit = vec3(0.0);
    float ps2 = 0.018 * 0.018, cut = ps2 * 14.0;
    vec3  pos[MAX_STARS]; vec3 tnt[MAX_STARS]; float mg[MAX_STARS];
    for (int i = 0; i < MAX_STARS; i++) {
        if (i >= N) break;
        float fi = float(i);
        vec3 p = starPosition(fi, t);
        pos[i] = p;
        float mag = pow(h11(fi * 13.7), 4.0) * 5.5 + 0.5;
        mg[i] = mag;
        tnt[i] = starP(h11(fi * 0.83));
        float tca; float d2 = rpd2(ro, rd, p, tca);
        if (tca < 0.0 || d2 > cut) continue;
        float w = exp(-d2 / (ps2 * 1.5)) * exp(-tca * 0.05);
        float tw = 1.0 + a.z * mag * 0.20 * sin(t * 7.0 + fi * 2.31);
        emit += tnt[i] * w * mag * tw;
    }
    // connecting lines: each star → its nearest neighbour with j > i
    float lb = 0.3 + a.y * 1.4 + a.x * 0.4;
    for (int i = 0; i < MAX_STARS; i++) {
        if (i >= N) break;
        float bestD = 1e9; bool found = false;
        vec3 bestP = vec3(0.0); vec3 bestT = vec3(0.0);
        for (int j = 0; j < MAX_STARS; j++) {
            if (j >= N) break;
            if (j <= i) continue;
            vec3 dp = pos[j] - pos[i]; float dd = dot(dp, dp);
            if (dd < bestD) { bestD = dd; found = true; bestP = pos[j]; bestT = tnt[j]; }
        }
        if (!found || bestD > 0.55 * 0.55) continue;
        vec3 p0 = pos[i]; vec3 bv = bestP - p0;
        vec3 ao = p0 - ro; vec3 cr = cross(rd, bv);
        float den = dot(cr, cr);
        if (den < 1e-6) continue;
        float s = dot(cross(ao, bv), cr) / den;
        float u = clamp(dot(cross(ao, rd), cr) / den, 0.0, 1.0);
        if (s < 0.0) continue;
        vec3 sp = p0 + bv * u; vec3 rp = ro + rd * s;
        float dseg2 = dot(sp - rp, sp - rp);
        float lw = 0.0035;
        if (dseg2 > lw * 14.0) continue;
        float w = exp(-dseg2 / (lw * 1.4)) * exp(-s * 0.05);
        emit += mix(tnt[i], bestT, 0.5) * w * 0.42 * lb;
    }
    return emit;
}

void main() {
    vec2 uv = (isf_FragNormCoord.xy * 2.0 - 1.0)
            * vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0);
    float aR    = clamp(audioReact, 0.0, 2.0);
    vec3  audio = vec3(audioBass, audioMid, audioHigh) * aR;

    // Universal camera: azimuth + orbit, mid-band overrides orbit speed.
    float orbT = camAzimuth + TIME * (camOrbitSpeed + audio.y * 0.6);
    vec3  ro   = vec3(cos(orbT) * camDist, camHeight, sin(orbT) * camDist);
    vec3  ta   = vec3(0.0, 0.05, 0.0);
    vec3  fw   = normalize(ta - ro);
    vec3  ri   = normalize(cross(vec3(0.0, 1.0, 0.0), fw));
    vec3  up   = cross(fw, ri);
    vec3  rd   = normalize(fw + uv.x * ri + uv.y * up);

    // Universal lighting basis from keyAngle/keyElevation.
    float ce = cos(keyElevation);
    vec3  kDir = normalize(vec3(cos(keyAngle) * ce, sin(keyElevation), sin(keyAngle) * ce));

    int m = int(mood), tier = int(particleCount);

    // Mood-specific background — disciplined, photographic.
    vec3 bgT, bgB;
    if (m == 0)      { bgT = vec3(0.020, 0.012, 0.040); bgB = vec3(0.005, 0.008, 0.020); }
    else if (m == 1) { bgT = vec3(0.050, 0.054, 0.060); bgB = vec3(0.024, 0.027, 0.032); }
    else if (m == 2) { bgT = vec3(0.030, 0.022, 0.018); bgB = vec3(0.010, 0.008, 0.006); }
    else             { bgT = vec3(0.004, 0.005, 0.012); bgB = vec3(0.001, 0.001, 0.003); }
    vec3 bg = mix(bgB, bgT, clamp(rd.y * 0.5 + 0.5, 0.0, 1.0));
    // User background: blend the mood backdrop toward the chosen color.
    bg = mix(bg, bgColor.rgb, bgColor.a);

    vec3 emit;
    if      (m == 0) emit = evalCloud(ro, rd, TIME, audio, cloudCount(tier),
                                      kDir, keyColor.rgb, fillColor.rgb, ambient, rimStrength);
    else if (m == 1) emit = evalGrid (ro, rd, TIME, audio, gridDims(tier),
                                      kDir, keyColor.rgb, fillColor.rgb, ambient, rimStrength);
    else if (m == 2) emit = evalMemo (ro, rd, TIME, audio, formCount(tier),
                                      kDir, keyColor.rgb, fillColor.rgb, ambient, rimStrength);
    else             emit = evalConstellation(ro, rd, TIME, audio, starCount(tier));

    // Global emissive audio pulse — bass/high are 0 in silence so this is a
    // no-op sound-off; gives the particle field a musical brightness lift
    // on top of the per-mood spatial/shimmer response computed above.
    emit *= (1.0 + audio.x * 2.2 + audio.z * 1.2);

    vec3 col = bg + emit;

    // Bass kick wash — soft full-frame HDR lift so even the near-black
    // negative space breathes on the beat (dead silent when audio.x==0).
    col += vec3(0.06, 0.035, 0.10) * (audio.x * audio.x) * 1.8;

    if (m == 1) col *= 0.92 + 0.16 * (1.0 - abs(uv.y));   // overhead fluorescent feel
    if (m == 3) col  = max(col, bg);                       // protect true black

    col *= 1.0 - 0.20 * length(uv * 0.55);                 // light vignette
    col *= exposure;                                        // LINEAR HDR out

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
        uc = max(hM * uc, 0.0);
    }
    col = uc;

    gl_FragColor = vec4(col, 1.0);
}
