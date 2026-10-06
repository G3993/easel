/*{
  "DESCRIPTION": "Nature — a flight through a sunlit rock-and-grass canyon under drifting clouds: layered plateau terrain carved by a tunnel along the camera path, warm sun with soft shadows and occlusion, then volumetric god rays through valley mist and a lens flare rendered from the depth buffer. Pass 0 marches the landscape (depth in alpha), pass 1 adds the rays, mist, flare and the HD finisher. Bass swells the rays and mist, beats flash the sun, mids warm the grass, highs sharpen the flare.",
  "CREDIT": "Easel / ShaderClaw house port, 2026-09-30 — of 'Sun Rays over landscape' by David Hoskins (Shadertoy, CC BY-NC-SA 3.0). Noise textures and the data buffer replaced by procedural code.",
  "CATEGORIES": ["Generator", "3D", "Landscape", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "flightSpeed", "LABEL": "Flight Speed",    "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "camBob",      "LABEL": "Camera Bob",      "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "camRoll",     "LABEL": "Camera Roll",     "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Motion" },
    { "NAME": "sunHeight",   "LABEL": "Sun Height",      "TYPE": "float", "MIN": 0.05, "MAX": 1.5,  "DEFAULT": 0.75, "GROUP": "Sun" },
    { "NAME": "sunAzimuth",  "LABEL": "Sun Azimuth",     "TYPE": "float", "MIN": -3.14,"MAX": 3.14, "DEFAULT": 0.64, "GROUP": "Sun" },
    { "NAME": "sunWarmth",   "LABEL": "Sun Warmth",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Sun" },
    { "NAME": "rayAmt",      "LABEL": "God Rays",        "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Sun" },
    { "NAME": "flareAmt",    "LABEL": "Lens Flare",      "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Sun" },
    { "NAME": "mistAmt",     "LABEL": "Valley Mist",     "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Air" },
    { "NAME": "cloudAmt",    "LABEL": "Clouds",          "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Air" },
    { "NAME": "skyBlue",     "LABEL": "Sky Blue",        "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Air" },
    { "NAME": "grassAmt",    "LABEL": "Grass",           "TYPE": "float", "MIN": 0.0,  "MAX": 1.5,  "DEFAULT": 1.0,  "GROUP": "Land" },
    { "NAME": "rockWarmth",  "LABEL": "Rock Warmth",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.5,  "DEFAULT": 0.75, "GROUP": "Land" },
    { "NAME": "plateau",     "LABEL": "Plateau Steps",   "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Land" },
    { "NAME": "detail",      "LABEL": "March Detail",    "TYPE": "float", "MIN": 50.0, "MAX": 220.0,"DEFAULT": 130.0,"GROUP": "Land" },
    { "NAME": "exposure",    "LABEL": "Exposure",        "TYPE": "float", "MIN": 0.3,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "saturation",  "LABEL": "Saturation",      "TYPE": "float", "MIN": 0.0,  "MAX": 1.6,  "DEFAULT": 1.05, "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",      "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "vignette",    "LABEL": "Vignette",        "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.15, "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Audio Reactivity" },
    { "NAME": "transparentBg", "LABEL": "Transparent BG", "TYPE": "bool", "DEFAULT": false, "GROUP": "Color" }
  ],
  "PASSES": [
    { "TARGET": "ntScene" },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// NATURE
//   pass 0  ntScene — terrain march (plateaus + wobble + camera tunnel),
//                     rock/grass albedo, sun + shadow + occlusion, sky with
//                     lit clouds. RGB = scene, A = hit distance (sky = FAR+).
//   pass 1  final   — god rays through valley mist from the depth buffer,
//                     sun-occlusion lens flare, grade + HD finisher.
//   The original's data buffer (camera / sun per frame) is a handful of
//   sin/cos, so every pixel just recomputes it. Its three noise textures
//   are hash-based value noise here.
// ─────────────────────────────────────────────────────────────────────────

#define R    RENDERSIZE.xy
#define FAR  1100.0
#define TAU  6.28318530718

float bassP()  { return pow(smoothstep(0.05, 0.85, audioBass), 1.4); }
float midP()   { return pow(smoothstep(0.06, 0.85, audioMid),  1.2); }
float highP()  { return pow(smoothstep(0.10, 0.90, audioHigh), 1.2); }
float beatP()  { return clamp(audioBeatPulse, 0.0, 1.0); }
float levP()   { return clamp(audioLevel, 0.0, 1.0); }

// ── globals (set per pixel in main) ─────────────────────────────────────
vec3  sunLight, camPos, sunCol, fogCol;
mat3  camMat;
float zProj, specular, g_ar;

// ── hashes / noise (float-only: the web host has no uint / bit ops) ────
float hash11(float p) { p = fract(p * 0.1031); p *= p + 33.33; p *= p + p; return fract(p); }
float hash12(vec2 p)  { vec3 p3 = fract(vec3(p.xyx) * 0.1031); p3 += dot(p3, p3.yzx + 33.33); return fract((p3.x + p3.y) * p3.z); }
vec2  hash22(vec2 p)  { vec3 p3 = fract(vec3(p.xyx) * vec3(0.1031, 0.1030, 0.0973)); p3 += dot(p3, p3.yzx + 33.33); return fract((p3.xx + p3.yz) * p3.zy); }
float hash13(vec3 p3) { p3 = fract(p3 * 0.1031); p3 += dot(p3, p3.zyx + 31.32); return fract((p3.x + p3.y) * p3.z); }

// value noise, 2D — stands in for the grey 256² noise texture
float vnoise2(vec2 x) {
    vec2 p = floor(x), f = fract(x);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash12(p),               hash12(p + vec2(1.0, 0.0)), f.x),
               mix(hash12(p + vec2(0.0, 1.0)), hash12(p + vec2(1.0, 1.0)), f.x), f.y);
}
// value noise, 3D — stands in for the RGBA noise texture's xy-slice trick
float vnoise3(vec3 x) {
    vec3 p = floor(x), f = fract(x);
    f = f * f * (3.0 - 2.0 * f);
    float n000 = hash13(p), n100 = hash13(p + vec3(1, 0, 0));
    float n010 = hash13(p + vec3(0, 1, 0)), n110 = hash13(p + vec3(1, 1, 0));
    float n001 = hash13(p + vec3(0, 0, 1)), n101 = hash13(p + vec3(1, 0, 1));
    float n011 = hash13(p + vec3(0, 1, 1)), n111 = hash13(p + vec3(1, 1, 1));
    return mix(mix(mix(n000, n100, f.x), mix(n010, n110, f.x), f.y),
               mix(mix(n001, n101, f.x), mix(n011, n111, f.x), f.y), f.z);
}
// "mipmapped" terrain texture: the original reads a texture at a lod that
// grows with ray distance, so far terrain is smoother. Fade the fine
// octave out the same way.
float terrainTex(vec2 uv, float lod) {
    float n = vnoise2(uv) * 0.65 + vnoise2(uv * 2.03 + 7.1) * 0.25;
    float fine = vnoise2(uv * 4.1 + 3.3) * 0.10;
    return n + fine * clamp(1.0 - lod * 0.2, 0.0, 1.0) + 0.5 * (0.10 * clamp(lod * 0.2, 0.0, 1.0));
}
// rock grain — stands in for the rock photo texture (vec3)
vec3 rockTex(vec2 uv) {
    float a = vnoise2(uv * 8.0) * 0.5 + vnoise2(uv * 17.0 + 2.0) * 0.3 + vnoise2(uv * 41.0 + 9.0) * 0.2;
    float b = vnoise2(uv * 6.0 + 5.0);
    float grit = vnoise2(uv * 90.0 + 1.0) * 0.12;   // micro detail
    return vec3(0.55 + 0.45 * a + grit, 0.50 + 0.40 * b, 0.42 + 0.40 * a * b) * 0.85;
}

// ── camera path (verbatim) ───────────────────────────────────────────────
vec3 cameraPath(float z) {
    return vec3(200.0 * sin(z * 0.0045) + 190.0 * cos(z * 0.001),
                43.0 * (cos(z * 0.0047) + sin(z * 0.0013)) + 53.0 * (sin(z * 0.003)),
                z);
}
mat3 setCamMat(vec3 ro, vec3 ta, float cr) {
    vec3 cw = normalize(ta - ro);
    vec3 cp = vec3(sin(cr), cos(cr), 0.0);
    vec3 cu = normalize(cross(cw, cp));
    vec3 cv = normalize(cross(cu, cw));
    return mat3(cu, cv, cw);
}
float sMax(float a, float b, float s) {
    float h = clamp(0.5 + 0.5 * (a - b) / s, 0.0, 1.0);
    return mix(b, a, h) + h * (1.0 - h) * s;
}

// ── terrain ──────────────────────────────────────────────────────────────
float map(vec3 p, float di) {
    float te = terrainTex(p.xz * 0.0017 + p.xy * 0.0019 - p.zy * 0.0017, di) * 80.0;
    float h  = dot(sin(p * 0.019), cos(p.zxy * 0.017)) * 100.0;
    // rock plateaus: quantised height with a steep lip
    float g = p.y * 0.33 + vnoise2(p.xz * 0.0003 * 16.0) * 40.0;
    float c = 60.0;
    g /= c;
    float s = fract(g);
    g = mix(g * c, floor(g) * c + pow(s, 20.0) * c, clamp(plateau, 0.0, 1.0)) * (plateau > 1.0 ? plateau : 1.0);
    float d = h + te + g;
    // carve the camera tunnel so the flight never hits rock
    vec2 o = cameraPath(p.z).xy;
    p.xy -= o;
    float tunnel = 40.0 - length(p.xy);
    return sMax(d, tunnel, 140.0);
}
vec3 getNormal(vec3 p, float e) {
    return normalize(vec3(map(p + vec3(e, 0.0, 0.0), e) - map(p - vec3(e, 0.0, 0.0), e),
                          map(p + vec3(0.0, e, 0.0), e) - map(p - vec3(0.0, e, 0.0), e),
                          map(p + vec3(0.0, 0.0, e), e) - map(p - vec3(0.0, 0.0, e), e)));
}
float binarySubdivision(vec3 rO, vec3 rD, vec2 t) {
    float halfwayT = 0.0;
    for (int i = 0; i < 7; i++) {
        halfwayT = dot(t, vec2(0.5));
        float d = map(rO + halfwayT * rD, halfwayT * 0.002);
        t = mix(vec2(t.x, halfwayT), vec2(halfwayT, t.y), step(0.01, d));
    }
    return halfwayT;
}
float marchScene(vec3 rO, vec3 rD, vec2 co) {
    float t = 5.0 + 10.0 * hash12(co);
    float oldT = 0.0;
    vec2 dist = vec2(1000.0);
    int steps = int(detail + 0.5);
    for (int j = 0; j < 220; j++) {
        if (j >= steps || t >= FAR) break;
        vec3 p = rO + t * rD;
        float h = map(p, t * 0.002);
        if (h < 0.01) { dist = vec2(oldT, t); break; }
        oldT = t;
        t += h * 0.35 + t * 0.001;
    }
    if (t < FAR) t = binarySubdivision(rO, rD, dist);
    return t;
}

// ── sky + clouds ─────────────────────────────────────────────────────────
vec3 getSky(vec3 dir) {
    vec3 blue = mix(vec3(0.05, 0.14, 0.5), vec3(0.12, 0.30, 0.75), skyBlue);
    return mix(fogCol, blue, abs(dir.y));
}
float findClouds2D(vec2 p) {
    float a = 1.0, r = 0.0;
    p *= 0.0015;
    for (int i = 0; i < 5; i++) { r += vnoise2(p *= 2.2) * a; a *= 0.5; }
    return max(r - 1.0, 0.0);
}
vec4 getClouds(vec3 pos, vec3 dir) {
    if (dir.y < 0.0) return vec4(0.0);
    float d = 1600.0 / dir.y;
    vec2 p = pos.xz + dir.xz * d;
    float r = findClouds2D(p);
    float t = findClouds2D(p + normalize(sunLight.xz) * 15.0);
    t = sqrt(max((r - t) * 20.0, 0.2)) * 0.8;
    return vec4(vec3(t) * sunCol, r * cloudAmt);
}

// ── surface ──────────────────────────────────────────────────────────────
vec3 texCube(vec3 p, vec3 n) {
    vec3 x = rockTex(p.yz), y = rockTex(p.zx), z = rockTex(p.xy);
    return (x * abs(n.x) + y * abs(n.y) + z * abs(n.z)) / (1e-20 + abs(n.x) + abs(n.y) + abs(n.z));
}
vec3 albedo(vec3 pos, vec3 nor) {
    specular = 0.8;
    vec3 alb = texCube(pos * 0.017, nor).yxz;
    float f = vnoise3(pos * 0.01);
    alb *= vec3(0.75 + f * rockWarmth / 0.75, 1.0, 0.9);
    float grass = smoothstep(0.1, 0.8, nor.y) * (vnoise3(pos * 0.07) + 0.1) * grassAmt;
    float v = (vnoise3(pos * 0.05) + vnoise3(pos * 0.1) * 0.5) * 0.5;
    vec3 col = rockTex(pos.xz * 0.01);
    col += vnoise2(pos.xz * 0.01 * 16.0) - 0.3;
    float warm = g_ar * 0.25 * midP();                       // mids warm the grass
    alb = mix(alb, col * vec3(0.1 + v + warm, 0.8, 0.1), clamp(grass, 0.0, 1.0));
    alb = clamp(alb, 0.0, 1.0);
    specular = max(specular - grass, 0.0);
    return pow(alb, vec3(1.3));
}
float shadow(vec3 ro, vec3 rd) {
    float res = 1.0, t = 0.1;
    for (int i = 0; i < 8; i++) {
        float h = map(ro + rd * t, 1.0);
        res = min(res, 4.0 * h / t);
        t += h + t * 0.01;
        if (res < 0.3) break;
    }
    return clamp(res, 0.3, 1.0);
}
float calcOcc(vec3 pos, vec3 nor) {
    float occ = 0.0, sca = 1.0;
    for (int i = 0; i < 5; i++) {
        float h = 0.1 + 1.0 * float(i);
        float d = map(pos + h * nor, 0.0);
        occ += (h - d) * sca;
        sca *= 0.5;
    }
    return clamp(1.0 - occ, 0.0, 1.0);
}
vec3 lighting(vec3 mat, vec3 pos, vec3 normal, vec3 eyeDir) {
    float sh = shadow(pos + normal * 0.2, sunLight);
    vec3 col = mat * sunCol * max(dot(sunLight, normal), 0.0) * sh;
    float occ = calcOcc(pos, normal);
    col += mat * sunCol * abs(-(normal.y * 0.14)) * occ;
    normal = reflect(eyeDir, normal);
    col += pow(max(dot(sunLight, normal), 0.0), 12.0) * sunCol * sh * specular * occ;
    return min(col, 1.0);
}

// ── pass 1 helpers ───────────────────────────────────────────────────────
float getMist(vec3 dir, vec3 pos) {
    vec3 clou = dir * 1.5 + pos * 0.02;
    float t = vnoise3(clou);
    t += vnoise3(clou * 2.1) * 0.4;
    t += vnoise3(clou * 4.3) * 0.2;
    t += vnoise3(clou * 7.9) * 0.1;
    return t;
}
float sceneDepth(vec2 uv) { return texture2D(ntScene, clamp(uv, 0.0, 1.0)).w; }
float obscurePartsOfSun(vec2 p) {
    float a = 0.0, e = 0.08;
    vec2 asp = vec2(R.y / R.x, 1.0);
    if (sceneDepth(0.5 + 0.5 * p * asp) >= FAR) a += 0.5;
    if (sceneDepth(0.5 + 0.5 * (p + vec2( e,  e)) * asp) >= FAR) a += 0.125;
    if (sceneDepth(0.5 + 0.5 * (p + vec2( e, -e)) * asp) >= FAR) a += 0.125;
    if (sceneDepth(0.5 + 0.5 * (p + vec2(-e, -e)) * asp) >= FAR) a += 0.125;
    if (sceneDepth(0.5 + 0.5 * (p + vec2(-e,  e)) * asp) >= FAR) a += 0.125;
    return a;
}
float godRays(vec2 uv) {
    float ra = 0.0;
    vec2 sunP = vec2(dot(sunLight, camMat[0]), dot(sunLight, camMat[1])) - vec2(0.05, -0.15);
    vec2 p = uv - sunP;
    float add = hash12(uv * 4000.0) * 0.02;
    for (float x = 0.1; x < 1.0; x += 0.02) {
        float z = max(sceneDepth((sunP + (p * (x + add)) + 1.0) * 0.5), 300.0) - 300.0;
        ra += z * x;
    }
    return ra * 0.00001;
}
vec3 lensFlare(vec2 uv, vec3 dir) {
    vec3 col = vec3(0.0);
    vec3 sunPos = sunLight * 20000.0;
    mat3 inv = transpose(camMat);
    vec3 cp = inv * -sunPos;
    if (cp.z < 0.0) {
        vec2 sun2d = zProj * cp.xy / cp.z;
        if (sun2d.x < -2.0 || sun2d.x > 2.0 || sun2d.y < -2.0 || sun2d.y > 2.0) return col;
        float z = obscurePartsOfSun(sun2d);
        if (z > 0.0) {
            float bri = max(dot(dir, sunLight) * 0.5, 0.0);
            bri = pow(bri, 3.0) * 5.0 * z;
            vec2 uvT = uv - sun2d;
            float glare1 = max(dot(dir, sunLight), 0.0);
            uvT = mix(uvT, uv, -2.3);
            float glare2 = max(1.7 - length(uvT + sun2d * 3.0) * 4.0, 0.0);
            float glare3 = max(1.7 - pow(length(uvT + sun2d * 3.5) * 14.0, 200.0), 0.0) * 0.7;
            col += bri * vec3(1.0, 0.0, 0.0)  * pow(glare1, 10.5) * 2.0;
            col += bri * vec3(0.5, 0.05, 0.0) * pow(glare2, 3.0);
            col += bri * vec3(0.1, 0.1, 0.6)  * pow(glare3, 3.0) * 3.0;
        }
    }
    return col * 0.8;
}

// ── per-pixel "data buffer" ──────────────────────────────────────────────
void setupFrame() {
    g_ar = audioReact;
    // the original: gTime = (iTime + 410) * 32 — flight speed scales it,
    // bass pushes the phase (never the rate) so motion stays chop-free
    float gTime = (TIME * flightSpeed + 410.0) * 32.0 + g_ar * 12.0 * audioBassTime;
    float r = gTime / 63.0;
    camPos = cameraPath(gTime) + vec3(sin(r * 0.4) * 24.0, cos(r * 0.3) * 24.0, 0.0) * camBob;
    vec3 camTar = cameraPath(gTime + 30.0);
    camMat = setCamMat(camPos, camTar, (camTar.x - camPos.x) * 0.02 * camRoll);
    float ca = cos(sunAzimuth), sa = sin(sunAzimuth);
    sunLight = normalize(vec3(0.5 * sa, sunHeight, 0.5 * ca));
    sunCol = mix(vec3(1.0, 0.95, 0.9), vec3(1.0, 0.72, 0.55), sunWarmth);
    fogCol = vec3(0.4);
    zProj = 0.6;
}

void main() {
    vec2 fragCoord = gl_FragCoord.xy;
    setupFrame();

    if (PASSINDEX == 0) {
        // ── landscape ──
        vec2 uv = (-R + 2.0 * fragCoord) / R.y;
        specular = 0.0;
        vec3 dir = camMat * normalize(vec3(uv, zProj));
        vec3 sky = getSky(dir);
        float dhit = marchScene(camPos, dir, fragCoord);
        vec3 col;
        if (dhit < FAR) {
            vec3 p = camPos + dhit * dir;
            vec3 nor = getNormal(p, dhit / R.y);
            vec3 mat = albedo(p, nor);
            col = lighting(mat, p, nor, dir);
        } else {
            col = sky;
            vec4 cc = getClouds(camPos, dir);
            col = mix(col, cc.xyz, clamp(cc.w, 0.0, 1.0));
            col += pow(max(dot(sunLight, dir), 0.0), 200.0) * sunCol * (1.0 + g_ar * 0.6 * beatP());
            col = min(col, 1.0);
            dhit = FAR + 100.0;           // a clean "sky" depth for pass 1
        }
        col = clamp(col, 0.0, 1.0);
        col = col * 0.6 + col * col * (3.0 - 2.0 * col);
        gl_FragColor = vec4(col, dhit);
    } else {
        // ── rays, mist, flare, grade ──
        vec2 xy = (-R + 2.0 * fragCoord) / R;
        vec2 uv = xy * vec2(R.x / R.y, 1.0);
        vec4 scene = texture2D(ntScene, fragCoord / R);
        vec3 col = scene.xyz;
        vec3 dir = camMat * normalize(vec3(uv, zProj));

        float t = getMist(dir, camPos);
        t = mix(1.0, t * mistAmt, exp(-0.00005 * scene.w));
        float gr = godRays(xy) * rayAmt * (1.0 + g_ar * 0.6 * bassP());
        col += gr * t * sunCol;
        col += lensFlare(uv, dir) * flareAmt * (1.0 + g_ar * 0.4 * highP());

        float vig = smoothstep(4.2, 0.5, dot(uv, uv));
        col *= mix(1.0, vig, vignette);
        col = min(col * vec3(1.1, 1.0, 0.8), 1.0);
        col = sqrt(col) * exposure;

        // HD finisher
        float lvl = levP();
        col *= brightness * mix(1.0, 0.8 + 0.25 * lvl, g_ar);
        float l0 = dot(col, vec3(0.299, 0.587, 0.114));
        col = mix(vec3(l0), col, saturation);
        col += (hash12(fragCoord + fract(TIME) * 61.0) - 0.5) * 0.006;
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.35);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.12), 0.0, 1.0);
        float alphaOut = transparentBg ? smoothstep(0.02, 0.15, lumHD) : 1.0;
        gl_FragColor = vec4(col, alphaOut);
    }
}
