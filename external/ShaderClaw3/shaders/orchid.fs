/*{
  "DESCRIPTION": "Orchid — a raymarched 3D orchid bloom (Sphere Pack knob re-expresses it as packed breathing spheres that scatter and re-fuse) (profiled from orchid_flower.glb: broad twin petals, three back sepals, ruffled lip, tiny column) floating in a dreamy sky of drifting gradient clouds. The petals carry real interior color — throat gradients, magenta veining, a golden lip ridge, translucent backlight — and the bloom slowly sways, breathes open, and turns. Two soft sister blooms drift in the clouds behind. Bass breathes the bloom open, mids sway the petals, beats sweep a gleam along them, level warms the sky.",
  "CATEGORIES": ["Generator", "3D", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "bloomSize",    "LABEL": "Bloom Size",    "TYPE": "float", "MIN": 0.5, "MAX": 1.6, "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "bloomOpen",    "LABEL": "Bloom Open",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.65, "GROUP": "Shape / Geometry" },
    { "NAME": "swayAmt",      "LABEL": "Sway",          "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.65, "GROUP": "Motion / Animation" },
    { "NAME": "orbitSpeed",   "LABEL": "Camera Orbit",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "petalColor",   "LABEL": "Petal Color",   "TYPE": "color", "DEFAULT": [0.98, 0.93, 0.97, 1.0], "GROUP": "Color" },
    { "NAME": "throatColor",  "LABEL": "Throat Color",  "TYPE": "color", "DEFAULT": [0.85, 0.10, 0.45, 1.0], "GROUP": "Color" },
    { "NAME": "cloudAmt",     "LABEL": "Clouds",        "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.65, "GROUP": "Color" },
    { "NAME": "skyShift",     "LABEL": "Sky Shift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.4,  "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.15, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Depth / Passes" },
    { "NAME": "packAmt",    "LABEL": "Sphere Pack",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Sphere Pack" },
    { "NAME": "packCell",   "LABEL": "Sphere Size",   "TYPE": "float", "MIN": 0.05,"MAX": 0.25,"DEFAULT": 0.11, "GROUP": "Sphere Pack" },
    { "NAME": "packFill",   "LABEL": "Sphere Fill",   "TYPE": "float", "MIN": 0.5, "MAX": 1.6, "DEFAULT": 1.1,  "GROUP": "Sphere Pack" },
    { "NAME": "packBlend",  "LABEL": "Sphere Fusion", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Sphere Pack" },
    { "NAME": "packBreath", "LABEL": "Sphere Breath", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Sphere Pack" },
    { "NAME": "packScatter","LABEL": "Sphere Scatter","TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Sphere Pack" },
    { "NAME": "packSpeed",  "LABEL": "Sphere Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Sphere Pack" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// ORCHID — ~/Downloads/orchid_flower.glb profiled offline (Sketchfab
//   "Orchid_Highpoly": bloom diameter ≈ half plant height, wide flat
//   spread) and rebuilt as an analytic Phalaenopsis SDF, the pp.glb
//   pipeline: no mesh at runtime, pure math.
//   Anatomy (6 parts, each a curved tapered petal sheet via nearest-point
//   projection): 2 broad lateral petals nearly horizontal, 3 narrower
//   sepals behind at 120°, a ruffled forward-cupped lip below center,
//   and a small column. Parts remember u (base→tip) and v (across) so
//   the SHADING paints real interior color: throat→edge gradients,
//   magenta vein streaks, ruffle-edge tint, a golden spotted lip ridge,
//   and a translucent backlight so petals glow like real ones.
//   Behind: layered gradient sky + two self-shaded fbm cloud decks +
//   two soft 2D sister blooms drifting in the haze (depth without a
//   second raymarch). Bass opens the bloom, mids sway (phase push),
//   beat sweeps a gleam base→tip, level warms the sky. Bright sky →
//   capped dim lift in the composite.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define PI 3.14159265
#define TAU 6.2831853

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1.0, 0.0)), f.x),
               mix(hash21(i + vec2(0.0, 1.0)), hash21(i + vec2(1.0, 1.0)), f.x), f.y);
}
float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.07 + vec2(7.3, 3.1);
        a *= 0.52;
    }
    return v;
}

// petal-frame rotation helpers
vec3 rotZ(vec3 p, float a) { float c = cos(a), s = sin(a); return vec3(c*p.x - s*p.y, s*p.x + c*p.y, p.z); }
vec3 rotX(vec3 p, float a) { float c = cos(a), s = sin(a); return vec3(p.x, c*p.y - s*p.z, s*p.y + c*p.z); }
vec3 rotY(vec3 p, float a) { float c = cos(a), s = sin(a); return vec3(c*p.x + s*p.z, p.y, -s*p.x + c*p.z); }

// petal-material globals written by the nearest part during map()
float gU, gV, gPart;
float gOpen, gSway1, gSway2, gRufPhase;   // animation params fed from renderScene

// curved tapered petal sheet: grows along +y, faces +z; ruffle = edge wave
float sdPetal(vec3 p, float len, float wid, float curl, float ruffle, float part) {
    float yc = clamp(p.y, 0.0, len);
    float tt = yc / max(len, 1e-4);
    float w = wid * sin(PI * pow(max(tt, 0.001), 0.70)) ;
    float xc = clamp(p.x, -w, w);
    float vv = (w > 1e-4) ? xc / w : 0.0;
    // sheet height: backward curl along length, cup across, ruffled edge
    float z = curl * (yc * yc * 0.55 + xc * xc * 0.75)
            + ruffle * 0.022 * sin(vv * 9.0 + tt * 5.0 + gRufPhase * (1.0 + 0.6 * part)) * tt;
    vec3 s = vec3(xc, yc, z);
    float d = length(p - s) - 0.022 * (1.0 - 0.55 * tt);
    if (d < 0.06) { gU = tt; gV = vv; gPart = part; }   // material coords
    return d;
}

// one full bloom SDF (local space, facing +z)
float sdBloom(vec3 p) {
    float d = 1e3;
    // 3 sepals behind, at 120° (top, lower-left, lower-right), tilted back
    for (int i = 0; i < 3; i++) {
        float ang = PI * 0.5 + float(i) * TAU / 3.0;
        vec3 q = rotZ(p - vec3(0.0, 0.0, -0.055), -ang + PI * 0.5);
        q = rotX(q, -(0.55 - 0.45 * gOpen) + 0.15 * gSway1 * sin(float(i) * 2.1 + gRufPhase * 0.4));
        d = min(d, sdPetal(q, 0.52, 0.15, 0.34, 0.3, 0.0));
    }
    // 2 broad petals, nearly horizontal
    for (int i = 0; i < 2; i++) {
        float m = (i == 0) ? 1.0 : -1.0;
        float ang = PI * 0.5 + m * (PI * 0.42);
        vec3 q = rotZ(p, -ang + PI * 0.5);
        q = rotX(q, -(0.42 - 0.40 * gOpen) + 0.13 * gSway2 * m);
        d = min(d, sdPetal(q, 0.46, 0.33, 0.26, 0.5, 1.0));
    }
    // ruffled lip below center, cupped forward, twin lobes via |x| pinch
    {
        vec3 q = rotZ(p - vec3(0.0, 0.0, 0.05), PI);      // grows downward
        q.x = abs(q.x) - 0.035;                            // twin lobes
        q = rotX(q, 0.55 - 0.25 * gOpen + 0.09 * gSway1);
        d = min(d, sdPetal(q, 0.30, 0.16, -0.55, 1.0, 2.0));
    }
    // column: small rounded nub at the heart
    {
        vec3 q = p - vec3(0.0, 0.03, 0.10);
        float dc = length(q * vec3(1.0, 0.85, 1.15)) - 0.062;
        if (dc < 0.06) { gU = 0.0; gV = 0.0; gPart = 3.0; }
        d = min(d, dc);
    }
    return d;
}

// ── SPHERE PACK: the bloom re-expressed as packed spheres (after the
//    Shadertoy medial-axis sphere-packing sketch Lu pasted, rebuilt as a
//    jittered lattice of inscribed spheres: each cell centre samples the true
//    SDF, its inscribed radius becomes a ball, balls smin together, breathe,
//    and scatter away from the surface on a slow cycle / on beats).
float gPackT, gPackK, gPackFun, gPackScatter, gPackBeat;
float hash13(vec3 p3) {
    p3 = fract(p3 * 0.1031);
    p3 += dot(p3, p3.zyx + 31.32);
    return fract((p3.x + p3.y) * p3.z);
}
float sminPack(float a, float b, float k) {
    float h = max(k - abs(a - b), 0.0) / max(k, 1e-5);
    return min(a, b) - h * h * k * 0.25;
}
float sdPacked(vec3 p) {
    float d0 = sdBloom(p);
    float h = packCell;
    if (d0 > 2.0 * h) return d0 - 0.6 * h;             // beads reach 0.55h outside the sheet
    float uS = gU, vS = gV, pS = gPart;                // material of the true surface
    float bestU = uS, bestV = vS, bestP = pS;
    vec3 base = floor(p / h - 0.5);
    float d = 1e3, bestD = 1e3;
    for (int i = 0; i < 8; i++) {
        int ix = i - (i / 2) * 2, iy = (i / 2) - (i / 4) * 2, iz = i / 4;   // no bit ops (GLSL ES 1.0 host)
        vec3 cell = base + vec3(float(ix), float(iy), float(iz));
        float hh = hash13(cell + 11.3);
        vec3 jit = vec3(hash13(cell + 1.7), hash13(cell + 5.9), hh) - 0.5;
        vec3 c = (cell + 0.5) * h + jit * h * 0.55;
        // animation: balls wobble off the surface (buffer C of the reference)
        vec3 wob = vec3(sin(gPackT + hh * 12.0), cos(gPackT + hh * 19.0), cos(gPackT * 2.0 + hh * 25.0));
        c += wob * gPackScatter * h * 1.5;
        // petals are thin sheets: beads straddle the surface instead of
        // sitting inside it — radius shrinks with distance from the sheet
        float sc = sdBloom(c);
        float r = (h * 0.55 * packFill - abs(sc)) ;
        if (r < h * 0.06) continue;
        r *= mix(1.1, 0.55, gPackFun) * (1.0 + 0.25 * gPackBeat);
        float nd = length(p - c) - r;
        if (nd < bestD) { bestD = nd; bestU = gU; bestV = gV; bestP = gPart; }
        d = sminPack(nd, d, gPackK * h);
    }
    gU = bestU; gV = bestV; gPart = bestP;
    if (d > 9e2) return d0 + h;                        // no balls here: stay outside
    return d;
}
float mapBloom(vec3 p) {
    if (packAmt < 0.001) return sdBloom(p);
    if (packAmt > 0.999) return sdPacked(p);
    float a = sdBloom(p);
    float uA = gU, vA = gV, pA = gPart;
    float b = sdPacked(p);
    if (b > a) { gU = uA; gV = vA; gPart = pA; }
    return mix(a, b, packAmt);
}
vec3 calcN(vec3 p) {
    vec2 e = vec2(0.0018, 0.0);
    return normalize(vec3(
        mapBloom(p + e.xyy) - mapBloom(p - e.xyy),
        mapBloom(p + e.yxy) - mapBloom(p - e.yxy),
        mapBloom(p + e.yyx) - mapBloom(p - e.yyx)));
}

// soft 2D sister bloom for the background haze
float bloom2D(vec2 d, float s, float rot) {
    float th = atan(d.y, d.x) + rot;
    float r = length(d);
    float lobe = pow(abs(cos(2.5 * th)), 0.6);
    float rad = s * (0.55 + 0.45 * lobe);
    return smoothstep(1.0, 0.55, r / max(rad, 1e-4));
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 uv = (fc - 0.5 * R) / R.y;

    float t   = TIME * motionSpeed * 0.4;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP  = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // ── gradient sky: dawn colors, slowly drifting ──────────────────────
    float sh = skyShift * TAU;
    vec3 skyA = vec3(0.62, 0.58, 0.86) + 0.05 * vec3(sin(sh), sin(sh + 2.1), sin(sh + 4.2));
    vec3 skyB = vec3(0.98, 0.76, 0.62) + 0.05 * vec3(sin(sh + 1.0), sin(sh + 3.1), sin(sh + 5.2));
    vec3 skyC = vec3(0.99, 0.93, 0.82);
    float g = smoothstep(-0.55, 0.55, uv.y + 0.06 * sin(t * 0.11));
    vec3 col = mix(skyC, skyB, smoothstep(0.0, 0.45, g));
    col = mix(col, skyA, smoothstep(0.40, 1.0, g));
    // sun glow, warmed by level
    vec2 sunP = vec2(0.38, 0.30);
    col += vec3(0.30, 0.20, 0.10) * (0.8 + amt * 0.5 * levelP) * exp(-length(uv - sunP) * 2.2);

    // ── two self-shaded cloud decks ─────────────────────────────────────
    float tw = t + amt * 0.4 * midP;                 // phase push, chop-free
    for (int i = 0; i < 2; i++) {
        float fi = float(i);
        vec2 p = uv * vec2(1.15 - 0.3 * fi, 1.9 - 0.4 * fi)
               + vec2(tw * (0.030 + 0.022 * fi) + 0.06 * sin(t * 0.32 * orbitSpeed) * (1.0 + fi), fi * 11.0);
        float f = fbm(p);
        float fDet = f + 0.10 * (vnoise(p * 5.0 + fi * 3.0) - 0.5);
        float dens = cloudAmt * smoothstep(0.50 - 0.06 * fi, 0.68, fDet);
        float f2 = fbm(p + normalize(sunP - uv + vec2(0.0, 0.3)) * 0.09);
        float shade = clamp(0.5 + 2.6 * (f2 - f), 0.0, 1.0);
        vec3 cloudCol = mix(mix(vec3(0.72, 0.62, 0.80), vec3(1.00, 0.94, 0.88), fi * 0.5),
                            mix(vec3(0.55, 0.48, 0.68), vec3(0.99, 0.80, 0.70), fi * 0.5),
                            shade);
        col = mix(col, cloudCol, dens * (0.40 + 0.15 * fi));
    }

    // ── sister blooms drifting in the haze ──────────────────────────────
    vec3 pc = petalColor.rgb, tc = throatColor.rgb;
    {
        float par = sin(t * 0.32 * orbitSpeed);          // orbit parallax cue
        vec2 b1 = uv - vec2(-0.52 + 0.10 * par + 0.03 * sin(t * 0.19), 0.24 + 0.03 * sin(t * 0.13));
        vec2 b2 = uv - vec2(0.55 - 0.13 * par + 0.03 * sin(t * 0.16), -0.20 + 0.03 * cos(t * 0.11));
        float m1 = bloom2D(b1, 0.14, t * 0.05);
        float m2 = bloom2D(b2, 0.10, -t * 0.04 + 1.3);
        vec3 haze1 = mix(mix(pc, col, 0.45), mix(tc, col, 0.55), smoothstep(0.10, 0.0, length(b1)));
        vec3 haze2 = mix(mix(pc, col, 0.55), mix(tc, col, 0.62), smoothstep(0.08, 0.0, length(b2)));
        col = mix(col, haze1, m1 * 0.75);
        col = mix(col, haze2, m2 * 0.65);
    }

    // ── the hero orchid: raymarched ─────────────────────────────────────
    gOpen  = clamp(bloomOpen + amt * 0.30 * bassP + 0.05 * sin(t * 0.45), 0.0, 1.15);
    gSway1 = swayAmt * sin(tw * 0.7);
    gSway2 = swayAmt * sin(tw * 0.55 + 1.4);
    gRufPhase = tw * 1.6;                            // living ruffle edges
    // sphere-pack animation: slow fun cycle (smoothstep(sin)) as in the reference
    gPackT   = TIME * packSpeed * 0.8 + amt * 0.4 * audioBassTime;
    gPackFun = packBreath * smoothstep(0.0, 1.0, sin((TIME * packSpeed + 1.0) * 0.5) * 0.5 + 0.5);
    gPackK   = mix(0.12, 1.6, packBlend) * (1.0 - 0.6 * gPackFun);
    gPackScatter = packScatter * gPackFun + amt * 0.3 * beatP;
    gPackBeat = amt * bassP;

    float scale = 0.62 * bloomSize;
    // the bloom floats on a slow breeze
    vec3 drift = vec3(0.05 * sin(t * 0.31), 0.06 * sin(t * 0.24 + 1.0), 0.03 * sin(t * 0.19));
    // ── ORBITING camera: circles the flower, breathing in and out ──────
    float oa = t * 0.32 * orbitSpeed + amt * 0.10 * midP;
    float el = 0.16 + 0.15 * sin(t * 0.21);
    float orad = 2.30 + 0.25 * sin(t * 0.14) - amt * 0.20 * bassP;   // bass pulls in close
    vec3 ro = drift + orad * vec3(cos(oa) * cos(el), sin(el), sin(oa) * cos(el));
    vec3 ww = normalize(drift - ro);
    vec3 uu = normalize(cross(ww, vec3(0.0, 1.0, 0.0)));
    vec3 vv2 = cross(uu, ww);
    vec3 rd = normalize(uv.x * uu + uv.y * vv2 + 1.55 * ww);
    // the flower itself also turns slowly against the orbit
    float yaw = -t * 0.12 + 0.22 * sin(t * 0.16) + amt * 0.05 * midP;
    float pit = 0.12 * sin(t * 0.12 + 1.0);

    float dist = 0.0; float hit = -1.0;
    float hU = 0.0, hV = 0.0, hPart = 0.0;
    for (int i = 0; i < 96; i++) {
        if (i >= 64 && packAmt < 0.001) break;
        vec3 p = ro + rd * dist;
        vec3 lp = rotX(rotY((p - drift) / scale, yaw), pit);
        float d = mapBloom(lp) * scale;
        if (d < 0.004) { hit = 1.0; hU = gU; hV = gV; hPart = gPart; break; }
        dist += d * (packAmt > 0.001 ? 0.7 : 0.62);   // conservative (approx SDF)
        if (dist > 5.0) break;
    }

    if (hit > 0.0) {
        vec3 p = ro + rd * dist;
        vec3 lp = rotX(rotY((p - drift) / scale, yaw), pit);
        vec3 n = calcN(lp);
        n = rotY(rotX(n, -pit), -yaw);          // back to world

        // ── interior color: throat gradient + veins + part identity ─────
        vec3 base;
        if (hPart < 0.5) {                       // sepals: cooler, veined
            base = mix(tc * 0.8, mix(pc, vec3(0.90, 0.86, 0.95), 0.4),
                       smoothstep(0.10, 0.70, hU));
            base *= 0.88 + 0.12 * sin(hV * 11.0 + hU * 3.0);
        } else if (hPart < 1.5) {                // petals: bright, magenta veins
            base = mix(tc, pc, smoothstep(0.08, 0.65, hU));
            float vein = smoothstep(0.75, 1.0, sin(hV * 16.0 + hU * 2.0) * 0.5 + 0.5)
                       * smoothstep(0.85, 0.25, hU);
            base = mix(base, tc * 0.85, vein * 0.55);
        } else if (hPart < 2.5) {                // lip: deep + golden ridge + spots
            base = mix(tc * 1.05, tc * 0.55, hU);
            base = mix(base, vec3(1.00, 0.75, 0.15),
                       smoothstep(0.30, 0.05, abs(hV)) * smoothstep(0.55, 0.15, hU));
            base *= 0.85 + 0.15 * step(0.5, hash21(vec2(floor(hU * 14.0), floor(hV * 7.0))));
        } else {                                 // column: cream
            base = mix(pc, vec3(1.0, 0.96, 0.88), 0.6);
        }

        vec3 L = normalize(vec3(-0.5, 0.65, 0.55));
        float dif = 0.35 + 0.65 * max(dot(n, L), 0.0);
        float spe = pow(max(dot(normalize(L - rd), n), 0.0), 36.0);
        // translucent backlight: petals glow when lit from behind
        float sss = pow(max(dot(rd, L), 0.0), 2.0) * max(-dot(n, L), 0.0);
        float fre = pow(1.0 - max(dot(n, -rd), 0.0), 3.0);
        // beat gleam sweeping base → tip
        float sweep = fract(t * 0.6);
        float gleam = exp(-pow((hU - sweep) * 5.0, 2.0)) * amt * beatP * 0.5;

        col = base * dif
            + vec3(1.0) * spe * (0.5 + amt * 0.4 * highP)
            + mix(tc, vec3(1.0, 0.6, 0.8), 0.4) * sss * 0.85
            + mix(pc, skyB, 0.5) * fre * 0.35
            + vec3(1.0, 0.95, 0.9) * gleam;
        // soft contact of bloom into the haze at its silhouette
        col = mix(col, col * 1.06, fre * 0.5);
    } else {
        // faint god-light through the clouds where the bloom isn't
        col += vec3(0.08, 0.06, 0.04) * exp(-abs(uv.x - 0.2) * 3.0) * cloudAmt;
    }

    // ── finish ──────────────────────────────────────────────────────────
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.018 * gr;
    return col;
}

// ─── multipass: trail accumulation → bloom + lens-depth composite ────────
void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        vec3 col = renderScene();
        // luminous motion trails persist in the buffer (8-bit-safe decay floor)
        vec3 prev = texture2D(abTrail, uv).rgb;
        float decay = 0.50 + 0.46 * trailAmt;
        col = max(col, prev * decay - 0.0045);
        // write-dither: keeps soft gradients band-free in 8-bit buffer hosts
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else {
        // chromatic lens depth: R/B pulled apart along the radial axis
        vec2 dir = uv - 0.5;
        float ab = aberration * 0.0045;
        vec3 base;
        base.r = texture2D(abTrail, uv + dir * ab).r;
        base.g = texture2D(abTrail, uv).g;
        base.b = texture2D(abTrail, uv - dir * ab).b;
        // wide two-ring bloom of the bright field — real glow depth
        vec3 bl = vec3(0.0);
        for (int i = 0; i < 8; i++) {
            float a = float(i) * 0.7853982;
            vec2 o = vec2(cos(a), sin(a)) * (3.5 / R.y);
            bl += texture2D(abTrail, uv + o).rgb;
            bl += texture2D(abTrail, uv + o * 2.6).rgb * 0.6;
        }
        bl /= 12.8;
        bl = max(bl - 0.52, 0.0);
        vec3 col = base + bl * bl * bloomAmt * 1.8;
        // audio lift applied post-trail so dips stay visible (bright sky)
        float lvl = clamp(audioLevel, 0.0, 1.0);
        col *= brightness * mix(1.0, 0.64 + 0.36 * lvl, audioReact)
             * (1.0 + audioReact * 0.06 * clamp(audioBeatPulse, 0.0, 1.0));
        // HD finish: S-curve contrast + saturation richness
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        gl_FragColor = vec4(col, 1.0);
    }
}
