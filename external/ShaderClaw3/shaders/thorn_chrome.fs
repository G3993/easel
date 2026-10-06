/*{
  "DESCRIPTION": "Thorn Chrome — raymarched liquid-chrome thorn clusters: spiky star metaball nodes with long sharp spikes drifting on a clean blue-sky gradient. Mirror-chrome shading (sky reflection, warm ground bounce, pink iridescent accents) with hard star-glint sparkles at spike tips. Bass extends and sharpens the spikes, mids rotate the clusters on an envelope-rate clock, highs fire the star glints, beats flash a brief lens sparkle. Y2K chrome jewelry.",
  "CREDIT": "ShaderClaw — A-List batch 2",
  "CATEGORIES": ["Generator", "Audio", "3D"],
  "INPUTS": [
    { "NAME": "spikeLen",    "LABEL": "Spike Length",  "TYPE": "float", "MIN": 0.6,  "MAX": 1.5,  "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "spikeSharp",  "LABEL": "Spike Sharpness","TYPE": "float","MIN": 0.6,  "MAX": 1.6,  "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "clusters",    "LABEL": "Clusters",      "TYPE": "float", "MIN": 2.0,  "MAX": 3.0,  "DEFAULT": 3.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "driftSpeed",  "LABEL": "Drift Speed",   "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "spinSpeed",   "LABEL": "Spin Speed",    "TYPE": "float", "MIN": 0.0,  "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "glintAmount", "LABEL": "Glint Amount",  "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "skyTop",      "LABEL": "Sky Top",       "TYPE": "color", "DEFAULT": [0.10, 0.23, 0.76, 1.0], "GROUP": "Color" },
    { "NAME": "skyBottom",   "LABEL": "Sky Horizon",   "TYPE": "color", "DEFAULT": [0.78, 0.87, 0.99, 1.0], "GROUP": "Color" },
    { "NAME": "accentColor", "LABEL": "Iridescent Pink","TYPE": "color","DEFAULT": [1.0, 0.55, 0.80, 1.0],  "GROUP": "Color" },
    { "NAME": "paletteShift","LABEL": "Palette Shift", "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3,  "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Audio Reactivity" }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// THORN CHROME — matched to "classicbarbwire_3d_skybg" + "liquid_metal_3d".
//   Needle-spike SDF thorn stars (twin metaball cores + 9 capsule-cone
//   spikes each), 2-3 clusters smin-fused, fixed 48-step raymarch.
//   Screen-space 4-point star sprites at projected spike tips (depth-checked
//   against the march) — highs fire them, beats add diagonal lens arms.
// Single-pass, memoryless audio response (a persistent phase accumulator was
// tried and measurably polluted the silence baseline in the harness):
//   bass extends/sharpens spikes, mids rock the clusters + parallax-sway the
//   camera with level-proportional amplitude, highs fire glints, beats flash.
// Idle floor: silence = authored slow drift/spin + quiet glint twinkle.
// ─────────────────────────────────────────────────────────────────────────

#define R   RENDERSIZE.xy
#define TAU 6.2831853

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
float hash21(vec2 p)  { return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453); }

vec3 hueRot(vec3 c, float a) {
    if (a < 0.0005) return c;
    float hC = cos(a), hS = sin(a);
    mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
            + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
            + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
    return clamp(c * hM, 0.0, 1.0);
}

mat3 rotXY(float ax, float ay) {
    float cx = cos(ax), sx = sin(ax), cy = cos(ay), sy = sin(ay);
    return mat3(cy, 0.0, sy, 0.0, 1.0, 0.0, -sy, 0.0, cy) *
           mat3(1.0, 0.0, 0.0, 0.0, cx, -sx, 0.0, sx, cx);
}

// cluster transforms (set once in main, read by map/glints)
mat3  gR0, gR1, gR2;
vec3  gC0, gC1, gC2;
float gLen, gThin, gCount;

vec3 spikeDir(float j, float seed) {
    float y = 1.0 - 2.0 * (j + 0.5) / 9.0;
    float rr = sqrt(max(1.0 - y * y, 0.0));
    float a = j * 2.39996 + seed * TAU;
    return vec3(rr * cos(a), y, rr * sin(a));
}
float spikeLenJ(float j, float seed) {
    return (0.55 + 0.60 * hash11(seed * 37.0 + j * 7.1)) * gLen;
}

float smin(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

// twin liquid-metal cores + 9 long sharp needle spikes (exact capsule-cones)
float sdCluster(vec3 p, mat3 Rm, vec3 C, float scl, float seed) {
    vec3 q = Rm * (p - C) / scl;
    // cheap bounding reject keeps the march fast between clusters
    float bound = length(q) - (1.15 * gLen + 0.45);
    if (bound > 0.35) return bound * scl;
    vec3 off = normalize(vec3(hash11(seed * 5.1) - 0.5,
                              hash11(seed * 9.7) - 0.5,
                              hash11(seed * 3.3) - 0.5)) * 0.17;
    float d = length(q - off) - 0.21;
    d = smin(d, length(q + off) - 0.17, 0.20);
    for (int j = 0; j < 9; j++) {
        float fj = float(j);
        vec3 D = spikeDir(fj, seed);
        float L = spikeLenJ(fj, seed);
        float h = clamp(dot(q, D), 0.0, L);
        float rad = mix(0.080, 0.004, pow(h / L, 0.55)) * gThin;
        float ds = length(q - D * h) - rad;
        d = smin(d, ds, 0.05);
    }
    return d * scl;
}

float map(vec3 p) {
    float d = sdCluster(p, gR0, gC0, 1.45, 0.13);
    d = smin(d, sdCluster(p, gR1, gC1, 1.10, 0.57), 0.15);
    if (gCount > 2.5) d = smin(d, sdCluster(p, gR2, gC2, 0.80, 0.91), 0.15);
    return d;
}

vec3 calcNormal(vec3 p) {
    const vec2 e = vec2(0.005, -0.005);
    return normalize(e.xyy * map(p + e.xyy) + e.yyx * map(p + e.yyx) +
                     e.yxy * map(p + e.yxy) + e.xxx * map(p + e.xxx));
}

vec3 envSky(vec3 d, vec3 top, vec3 bot) {
    vec3 sky = mix(bot, top, smoothstep(-0.05, 0.75, d.y));
    // warm ground tone below, falling to deep navy — chrome's dark bands
    sky = mix(sky, vec3(0.48, 0.36, 0.27), smoothstep(0.06, 0.35, -d.y));
    sky = mix(sky, vec3(0.10, 0.09, 0.14), smoothstep(0.40, 0.85, -d.y));
    // bright silver horizon band — the liquid-chrome signature
    sky += vec3(1.0, 0.98, 0.94) * exp(-(d.y - 0.03) * (d.y - 0.03) * 60.0) * 0.75;
    return sky;
}

void main() {
    float amt = audioReact;
    // Soft-knee band conditioning (playbook standard).
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.6);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);

    vec2 uv = (gl_FragCoord.xy * 2.0 - R) / R.y;
    float t = TIME;
    float td = t * driftSpeed;

    // camera parallax sway: sinusoidal carrier, AMPLITUDE ∝ mid level —
    // whole-frame motion tracks the envelope, exactly zero in silence
    uv += amt * midP * vec2(0.055 * sin(t * 1.3), -0.036 * cos(t * 1.1));

    // cluster transforms: slow drift at different depths, per-cluster spin
    gC0 = vec3(-0.70,  0.85, 0.40) + 0.22 * vec3(sin(td*0.21),      cos(td*0.17),      0.5*sin(td*0.11));
    gC1 = vec3( 1.10, -0.40, 1.20) + 0.24 * vec3(sin(td*0.16+2.3),  cos(td*0.21+1.7),  0.5*sin(td*0.13+3.0));
    gC2 = vec3(-0.80, -1.40, 2.10) + 0.26 * vec3(sin(td*0.18+4.1),  cos(td*0.15+3.3),  0.5*sin(td*0.10+1.2));
    // mids ROCK the clusters: sway amplitude is the mid level (memoryless)
    float spin = 0.14 * spinSpeed;
    float sw0 = amt * 0.85 * midP * sin(t * 1.5 + 0.4);
    float sw1 = amt * 0.85 * midP * sin(t * 1.7 + 2.5);
    float sw2 = amt * 0.85 * midP * sin(t * 1.4 + 4.6);
    gR0 = rotXY(t*spin*0.9 + sw0 + 0.7,  t*spin*1.1 + sw0*0.6);
    gR1 = rotXY(t*spin*1.2 + sw1 + 2.1, -t*spin*0.8 - sw1*0.6 + 1.3);
    gR2 = rotXY(-t*spin*0.7 - sw2 + 4.0, t*spin*1.3 + sw2*0.6 + 2.6);

    // bass EXTENDS and SHARPENS the spikes (the thorns lunge)
    gLen  = spikeLen * (1.0 + amt * 0.65 * bassP);
    gThin = (1.0 / spikeSharp) * (1.0 - amt * 0.22 * bassP);
    gCount = clusters;

    // clean vertical blue-sky gradient background + grain (matched to ref)
    float grain = hash21(gl_FragCoord.xy) - 0.5;
    vec3 topC = hueRot(skyTop.rgb, paletteShift * TAU);
    vec3 botC = hueRot(skyBottom.rgb, paletteShift * TAU);
    vec3 accC = hueRot(accentColor.rgb, paletteShift * TAU);
    // bass slides the sky gradient: carrier sinusoid, amplitude ∝ bass level
    // (whole-frame level-proportional response; zero in silence)
    float skySlide = amt * 0.42 * bassP * sin(t * 1.5 + 0.8);
    vec3 col = mix(botC, topC, smoothstep(-1.05, 1.05, uv.y - skySlide));

    // ---- fixed 48-step raymarch ----
    vec3 ro = vec3(0.0, 0.0, -3.8);
    vec3 rd = normalize(vec3(uv, 1.6));
    vec3 sunD = normalize(vec3(-0.45, 0.65, -0.62));
    float hitT = -1.0;
    float tm = 0.0;
    for (int i = 0; i < 48; i++) {
        vec3 pos = ro + rd * tm;
        float sd = map(pos);
        if (sd < 0.0012 * tm + 0.0008) { hitT = tm; break; }
        tm += sd * 0.7;
        if (tm > 10.0) break;
    }

    if (hitT > 0.0) {
        vec3 pos = ro + rd * hitT;
        vec3 n = calcNormal(pos);
        vec3 refl = reflect(rd, n);
        // mirror chrome: sky reflection + fresnel edge + warm ground bounce
        vec3 env = envSky(refl, topC, botC);
        float fres = pow(1.0 - max(dot(n, -rd), 0.0), 3.0);
        vec3 chrome = env * (0.78 + 0.50 * fres);
        chrome += vec3(1.0) * pow(max(dot(refl, sunD), 0.0), 60.0) * 1.7;
        chrome += vec3(0.30, 0.20, 0.12) * max(-n.y, 0.0) * 0.25;
        // pink iridescent accent riding the fresnel bands
        float ir = 0.5 + 0.5 * sin(7.0 * fres + 3.0 * dot(n, vec3(0.6, 0.2, 0.7)) + t * 0.15);
        chrome += accC * ir * (0.10 + fres * 0.55);
        // crevice AO keeps the metal reading solid, not gray goo
        float ao = clamp(map(pos + n * 0.18) / 0.18, 0.0, 1.0);
        chrome *= 0.55 + 0.45 * ao;
        // level-proportional shine: the metal itself brightens with the music
        chrome *= 1.0 + amt * 0.45 * clamp(audioLevel, 0.0, 1.0);
        // soft depth haze into the sky keeps far clusters airy
        col = mix(chrome, col, smoothstep(4.5, 10.5, hitT) * 0.55);
    } else {
        hitT = 1e3;
    }

    // ---- hard star glints at spike tips (screen-space, depth-checked) ----
    float glintDrive = 0.28 + amt * (2.0 * highP + 0.7 * beatP);
    float diagAmt = amt * beatP;                      // beat lens sparkle
    vec3 glintCol = mix(vec3(1.0), accC, 0.22);
    for (int ci = 0; ci < 3; ci++) {
        if (float(ci) >= gCount) break;
        mat3 Rm = ci == 0 ? gR0 : (ci == 1 ? gR1 : gR2);
        vec3 C  = ci == 0 ? gC0 : (ci == 1 ? gC1 : gC2);
        float scl  = ci == 0 ? 1.45 : (ci == 1 ? 1.10 : 0.80);
        float seed = ci == 0 ? 0.13 : (ci == 1 ? 0.57 : 0.91);
        for (int j = 0; j < 9; j++) {
            float fj = float(j);
            vec3 tipL = spikeDir(fj, seed) * spikeLenJ(fj, seed);
            vec3 tipW = C + (tipL * Rm) * scl;        // v*M = R^T v (inverse rot)
            vec3 rel = tipW - ro;
            if (rel.z < 1.0) continue;
            vec2 sp = rel.xy * (1.6 / rel.z);
            vec2 v = uv - sp;
            if (abs(v.x) > 0.30 || abs(v.y) > 0.30) continue;
            float hj = hash11(seed * 91.0 + fj * 3.3);
            // twinkle phase per tip; occlusion: hide tips behind the metal
            float twk = 0.35 + 0.65 * pow(0.5 + 0.5 * sin(t * (1.5 + 2.5 * hj) + hj * 40.0), 4.0);
            float vis = smoothstep(-0.35, -0.05, hitT - length(rel)) * 0.85 + 0.15;
            float armL = 0.055 * (1.0 + 0.9 * diagAmt) * (0.7 + 0.6 * hj);
            float star = exp(-dot(v, v) * 5200.0) * 1.4;
            star += pow(max(0.0, 1.0 - abs(v.x) / armL), 2.0) * exp(-abs(v.y) * 260.0) * 0.85;
            star += pow(max(0.0, 1.0 - abs(v.y) / armL), 2.0) * exp(-abs(v.x) * 260.0) * 0.85;
            if (diagAmt > 0.02) {
                vec2 vd = vec2(v.x + v.y, v.x - v.y) * 0.7071;
                star += diagAmt * 0.8 *
                        (pow(max(0.0, 1.0 - abs(vd.x) / armL), 2.0) * exp(-abs(vd.y) * 300.0) +
                         pow(max(0.0, 1.0 - abs(vd.y) / armL), 2.0) * exp(-abs(vd.x) * 300.0));
            }
            col += glintCol * star * glintAmount * glintDrive * twk * vis;
        }
    }

    col += grain * 0.032;
    // soft compress then gamma-style brightness: no white-out, no black-out
    col = col / (1.0 + 0.22 * max(col - 1.0, 0.0));
    col = clamp(col, 0.0, 1.0);
    col = pow(col, vec3(1.0 / max(brightness, 0.3)));
    gl_FragColor = vec4(col, 1.0);
}
