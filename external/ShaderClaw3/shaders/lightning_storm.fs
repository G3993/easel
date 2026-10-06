/*{
  "CATEGORIES": [
    "Generator",
    "Atmospheric",
    "Audio Reactive"
  ],
  "DESCRIPTION": "High-fidelity branching forked lightning bolts with hot core + cyan plasma sheath, stochastic Brownian forks, parallax stormcloud layers, diagonal rain, abstract thunder forms (ribbon, plasma ball, sheet flash, ground crawl), screen flash with chromatic aberration, ground silhouette, bloom, grain, and vignette. Bass triggers strikes, mids amplify the storm, highs shimmer the clouds.",
  "INPUTS": [
    {
      "NAME": "boltProbability",
      "LABEL": "Bolt Chance",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55
    },
    {
      "NAME": "flashIntensity",
      "LABEL": "Flash Intensity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.95
    },
    {
      "NAME": "rainAngle",
      "LABEL": "Rain Angle",
      "TYPE": "float",
      "MIN": -0.6,
      "MAX": 0.6,
      "DEFAULT": 0.26
    },
    {
      "NAME": "stormDensity",
      "LABEL": "Storm Density",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.92,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "boltBranchDepth",
      "LABEL": "Branch Depth",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 4,
      "DEFAULT": 3,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "boltCoreWidth",
      "LABEL": "Core Width",
      "TYPE": "float",
      "MIN": 0.0005,
      "MAX": 0.012,
      "DEFAULT": 0.0025,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "boltSheathWidth",
      "LABEL": "Sheath Width",
      "TYPE": "float",
      "MIN": 0.005,
      "MAX": 0.05,
      "DEFAULT": 0.018,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "rainDensity",
      "LABEL": "Rain Density",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "abstractThunderMix",
      "LABEL": "Abstract Thunder",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "cloudDrift",
      "LABEL": "Cloud Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "boltLifetime",
      "LABEL": "Bolt Lifetime",
      "TYPE": "float",
      "MIN": 0.15,
      "MAX": 2.5,
      "DEFAULT": 0.95,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "boltJitter",
      "LABEL": "Bolt Jitter",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.3,
      "DEFAULT": 0.085,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "rainSpeed",
      "LABEL": "Rain Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "cloudChurn",
      "LABEL": "Cloud Churn",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 2.0
    },
    {
      "NAME": "cloudParallax",
      "LABEL": "Cloud Parallax",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "boltWrithe",
      "LABEL": "Bolt Writhe",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "rainGust",
      "LABEL": "Rain Gust",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "skyTopColor",
      "LABEL": "Sky Top",
      "TYPE": "color",
      "DEFAULT": [0.018, 0.022, 0.052, 1],
      "GROUP": "Color"
    },
    {
      "NAME": "skyBotColor",
      "LABEL": "Sky Bottom",
      "TYPE": "color",
      "DEFAULT": [0.1, 0.12, 0.18, 1],
      "GROUP": "Color"
    },
    {
      "NAME": "boltCoreColor",
      "LABEL": "Bolt Core",
      "TYPE": "color",
      "DEFAULT": [1, 1, 1, 1],
      "GROUP": "Color"
    },
    {
      "NAME": "boltSheathColor",
      "LABEL": "Bolt Sheath",
      "TYPE": "color",
      "DEFAULT": [0.55, 0.78, 1, 1],
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
      "NAME": "groundLine",
      "LABEL": "Ground Silhouette",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.3,
      "DEFAULT": 0.075,
      "GROUP": "Camera / Layout"
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

// ---------- hashing & noise ----------
float hash11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float hash12(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
vec2  hash21(float n) { return fract(sin(vec2(n * 12.9898, n * 78.233)) * 43758.5453); }

float vnoise(vec2 p) {
    vec2 ip = floor(p), fp = fract(p);
    fp = fp * fp * (3.0 - 2.0 * fp);
    float a = hash12(ip);
    float b = hash12(ip + vec2(1.0, 0.0));
    float c = hash12(ip + vec2(0.0, 1.0));
    float d = hash12(ip + vec2(1.0, 1.0));
    return mix(mix(a, b, fp.x), mix(c, d, fp.x), fp.y);
}

float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    mat2 r = mat2(0.8, -0.6, 0.6, 0.8);
    for (int i = 0; i < 6; i++) {
        v += a * vnoise(p);
        p = r * p * 2.04;
        a *= 0.5;
    }
    return v;
}

float fbm3(vec2 p) {
    float v = 0.0, a = 0.5;
    mat2 r = mat2(0.8, -0.6, 0.6, 0.8);
    for (int i = 0; i < 3; i++) {
        v += a * vnoise(p);
        p = r * p * 2.04;
        a *= 0.5;
    }
    return v;
}

// ---------- continuous polyline distance ----------
float sdSeg(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-8), 0.0, 1.0);
    return length(pa - ba * h);
}

float sdTaperedSeg(vec2 p, vec2 a, vec2 b, float wA, float wB) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-8), 0.0, 1.0);
    float w = mix(wA, wB, h);
    return length(pa - ba * h) - w;
}

// ---------- bolt construction ----------
vec3 boltSDF(vec2 uv, float seed, float depth, float jitter, float life01) {
    float topX = mix(0.18, 0.82, hash11(seed * 7.13 + 1.0));
    float botX = topX + (hash11(seed * 3.7 + 11.0) - 0.5) * 0.55;
    vec2 a = vec2(topX, 1.08);
    vec2 b = vec2(botX, -0.06);

    float strike = smoothstep(0.0, 0.18, life01);

    vec2 trunkDir = normalize(b - a);
    vec2 perp = vec2(-trunkDir.y, trunkDir.x);

    const int SEGS = 22;
    float minDC = 1e9;
    float minDS = 1e9;

    vec2 prev = a;
    float walk = 0.0;
    vec2 segPivots[6];
    float segPivotT[6];
    int pivotCount = 0;

    for (int i = 1; i <= SEGS; i++) {
        float t = float(i) / float(SEGS);
        if (t > strike + 0.05) break;

        float h = hash11(seed * 19.7 + float(i) * 3.1) - 0.5;
        float taperJ = sin(t * 3.14159);
        walk += h * jitter * 0.42;
        walk *= 0.86;
        vec2 base = mix(a, b, t);
        // MOTION: the live bolt writhes sideways while it burns
        float wr = boltWrithe * 0.045 * sin(t * 9.0 + TIME * 14.0 + seed) * taperJ;
        vec2 cur = base + perp * (walk * taperJ + wr);

        float wA = mix(0.0030, 0.0014, (t - 1.0/float(SEGS)));
        float wB = mix(0.0030, 0.0014, t);

        float dc = sdSeg(uv, prev, cur);
        float ds = sdTaperedSeg(uv, prev, cur, wA, wB);
        minDC = min(minDC, dc);
        minDS = min(minDS, ds);

        if (pivotCount < 6 && hash11(seed * 5.0 + float(i) * 2.13) > 0.55) {
            for (int s = 0; s < 6; s++) {
                if (s == pivotCount) { segPivots[s] = cur; segPivotT[s] = t; break; }
            }
            pivotCount++;
        }

        prev = cur;
    }

    int branches = int(clamp(depth, 1.0, 4.0));
    for (int k = 0; k < 4; k++) {
        if (k >= branches || k >= pivotCount) break;
        if (segPivotT[k] > strike + 0.02) continue;

        vec2 bp = segPivots[k];
        float bs = seed * 41.0 + float(k) * 17.31;

        float ang = mix(0.52, 1.05, hash11(bs + 1.7));
        ang *= (hash11(bs + 5.5) > 0.5) ? 1.0 : -1.0;
        float ca = cos(ang), sa = sin(ang);
        vec2 brDir = mat2(ca, -sa, sa, ca) * trunkDir;
        vec2 brPerp = vec2(-brDir.y, brDir.x);
        float reach = mix(0.18, 0.42, hash11(bs + 9.1));
        vec2 brEnd = bp + brDir * reach;

        const int BSEGS = 11;
        vec2 bprev = bp;
        float bwalk = 0.0;
        vec2 subPivots[3];
        int subCount = 0;

        for (int j = 1; j <= BSEGS; j++) {
            float tt = float(j) / float(BSEGS);
            float bh = hash11(bs + float(j) * 2.31) - 0.5;
            bwalk += bh * jitter * 0.35;
            bwalk *= 0.82;
            vec2 bbase = mix(bp, brEnd, tt);
            vec2 bcur = bbase + brPerp * bwalk * sin(tt * 3.14159);

            float bwA = mix(0.0019, 0.0007, (tt - 1.0/float(BSEGS)));
            float bwB = mix(0.0019, 0.0007, tt);

            float dc = sdSeg(uv, bprev, bcur);
            float ds = sdTaperedSeg(uv, bprev, bcur, bwA, bwB);
            minDC = min(minDC, dc);
            minDS = min(minDS, ds);

            if (subCount < 3 && hash11(bs + float(j) * 7.7) > 0.72) {
                for (int s = 0; s < 3; s++) {
                    if (s == subCount) { subPivots[s] = bcur; break; }
                }
                subCount++;
            }
            bprev = bcur;
        }

        for (int m = 0; m < 3; m++) {
            if (m >= subCount) break;
            if (depth < 2.5) break;
            vec2 sp = subPivots[m];
            float ss = bs * 13.7 + float(m) * 5.3;

            float sang = mix(0.5, 1.0, hash11(ss + 1.1));
            sang *= (hash11(ss + 2.2) > 0.5) ? 1.0 : -1.0;
            float sca = cos(sang), ssa = sin(sang);
            vec2 sDir = mat2(sca, -ssa, ssa, sca) * brDir;
            vec2 sPerp = vec2(-sDir.y, sDir.x);
            float sReach = mix(0.06, 0.18, hash11(ss + 3.3));
            vec2 sEnd = sp + sDir * sReach;

            const int SSEGS = 6;
            vec2 sprev2 = sp;
            float swalk = 0.0;
            for (int q = 1; q <= SSEGS; q++) {
                float qt = float(q) / float(SSEGS);
                float qh = hash11(ss + float(q) * 1.93) - 0.5;
                swalk += qh * jitter * 0.28;
                swalk *= 0.78;
                vec2 sbase = mix(sp, sEnd, qt);
                vec2 scur = sbase + sPerp * swalk * sin(qt * 3.14159);

                float swA = mix(0.0012, 0.0004, (qt - 1.0/float(SSEGS)));
                float swB = mix(0.0012, 0.0004, qt);

                float dc = sdSeg(uv, sprev2, scur);
                float ds = sdTaperedSeg(uv, sprev2, scur, swA, swB);
                minDC = min(minDC, dc);
                minDS = min(minDS, ds);
                sprev2 = scur;
            }
        }
    }

    return vec3(minDC, minDS, minDS);
}

float boltEnv(float life01, float seed) {
    float onset = smoothstep(0.0, 0.04, life01);
    float decay = exp(-life01 * 4.2);
    float flick = 0.65 + 0.35 * sin(life01 * 110.0 + seed * 7.0)
                       * sin(life01 * 47.0 + seed * 3.0);
    flick = mix(1.0, flick, smoothstep(0.18, 0.55, life01));
    return onset * decay * flick;
}

// ---------- ABSTRACT THUNDER FORMS ----------

// 1. RIBBON DISCHARGE: a sinuous horizontal plasma ribbon
float ribbonThunder(vec2 uv, float seed, float life01, float aspect) {
    float env = exp(-life01 * 5.5) * smoothstep(0.0, 0.06, life01);
    float yBase = mix(0.25, 0.85, hash11(seed * 3.71));
    float xStart = hash11(seed * 1.13) * 0.3;
    float xEnd   = 0.7 + hash11(seed * 2.31) * 0.3;
    float minD = 1e9;
    const int RSEGS = 18;
    vec2 prev = vec2(xStart, yBase);
    for (int i = 1; i <= RSEGS; i++) {
        float t = float(i) / float(RSEGS);
        if (t > smoothstep(0.0, 0.22, life01) + 0.05) break;
        float xc = mix(xStart, xEnd, t);
        float yc = yBase
            + 0.12 * sin(t * 9.31 + seed * 5.0)
            + 0.06 * sin(t * 23.7 + seed * 11.0)
            + 0.04 * (hash11(seed * 7.0 + float(i) * 1.9) - 0.5);
        vec2 cur = vec2(xc * aspect, yc);
        vec2 prevA = vec2(prev.x * aspect, prev.y);
        float d = sdSeg(uv, prevA, cur);
        minD = min(minD, d);
        prev = vec2(xc, yc);
    }
    float core = exp(-(minD * minD) / (0.002 * 0.002)) * env;
    float glow = exp(-(minD * minD) / (0.018 * 0.018)) * env * 0.5;
    return core + glow;
}

// 2. PLASMA BALL: expanding ring discharge from a point
float plasmaBall(vec2 uv, float seed, float life01, float aspect) {
    float env = exp(-life01 * 6.0) * smoothstep(0.0, 0.05, life01);
    vec2 center = vec2(
        mix(0.2, 0.8, hash11(seed * 4.11)) * aspect,
        mix(0.3, 0.9, hash11(seed * 6.77))
    );
    float r = life01 * 0.55 * aspect;
    float d = abs(length(uv - center) - r);
    float ring = exp(-(d * d) / (0.012 * 0.012)) * env;
    // Inner spokes: radial discharge lines
    float spokeAmt = 0.0;
    for (int si = 0; si < 8; si++) {
        float ang = float(si) * 0.7854 + hash11(seed + float(si) * 1.3) * 0.4;
        float ca = cos(ang), sa = sin(ang);
        vec2 dir = vec2(ca, sa);
        float spokeR = r * (0.4 + 0.6 * hash11(seed * 9.0 + float(si)));
        vec2 tip = center + dir * spokeR;
        float sd = sdSeg(uv, center, tip);
        float w = 0.003 + 0.004 * hash11(seed * 3.0 + float(si));
        spokeAmt += exp(-(sd * sd) / (w * w)) * env * 0.6;
    }
    return ring + spokeAmt;
}

// 3. SHEET LIGHTNING: broad diffuse glow behind clouds (no defined bolt path)
float sheetLightning(vec2 uv, float seed, float life01) {
    float env = exp(-life01 * 8.0) * smoothstep(0.0, 0.03, life01);
    float cx = mix(0.15, 0.85, hash11(seed * 5.3));
    float cy = mix(0.55, 0.98, hash11(seed * 2.9));
    vec2 d = uv - vec2(cx, cy);
    d.y *= 0.5; // flatten vertically
    float dist = length(d);
    float sheet = exp(-(dist * dist) / (0.18 * 0.18)) * env;
    // Layered noise structure inside the sheet
    float n = fbm3(uv * 5.0 + seed * 0.37 + TIME * 0.1);
    sheet *= 0.6 + 0.4 * n;
    return sheet;
}

// 4. GROUND CRAWL: horizontal branching discharge near ground level
float groundCrawl(vec2 uv, float seed, float life01, float aspect, float groundY) {
    float env = exp(-life01 * 7.0) * smoothstep(0.0, 0.08, life01);
    float startX = mix(0.1, 0.9, hash11(seed * 8.1)) * aspect;
    float y0 = groundY + 0.01 + 0.03 * hash11(seed * 2.0);
    float minD = 1e9;

    // Main crawl arm
    const int CSEGS = 14;
    vec2 cprev = vec2(startX, y0);
    float cwalk = 0.0;
    float reveal = smoothstep(0.0, 0.25, life01);
    for (int i = 1; i <= CSEGS; i++) {
        float t = float(i) / float(CSEGS);
        if (t > reveal + 0.06) break;
        float dir = (hash11(seed * 3.0) > 0.5) ? 1.0 : -1.0;
        float xc = startX + dir * t * 0.55 * aspect;
        cwalk += (hash11(seed * 11.0 + float(i) * 1.7) - 0.5) * 0.025;
        cwalk *= 0.88;
        float yc = y0 + cwalk + 0.008 * sin(t * 17.0 + seed);
        vec2 ccur = vec2(xc, yc);
        float d = sdSeg(uv, cprev, ccur);
        minD = min(minD, d);
        cprev = ccur;
    }

    float core = exp(-(minD * minD) / (0.0015 * 0.0015)) * env;
    float glow = exp(-(minD * minD) / (0.012 * 0.012)) * env * 0.45;
    return core + glow;
}

// ---------- RAIN FUNCTIONS ----------
// Fine misty rain: very short, dense, faint streaks
float mistyRain(vec2 uv, float speed, float angle, float density, float t) {
    vec2 ruv = uv;
    ruv.x += ruv.y * angle;
    ruv *= vec2(320.0, 55.0);
    ruv.y += t * speed * 28.0;
    ruv.x += hash11(floor(ruv.y) * 2.3) * 0.5;
    vec2 rip = floor(ruv);
    vec2 rfp = fract(ruv);
    float rh = hash12(rip + vec2(77.0, 0.0));
    float on = step(1.0 - density * 0.35, rh);
    float xProf = smoothstep(0.48, 0.44, abs(rfp.x - 0.5));
    float yProf = smoothstep(0.0, 0.2, rfp.y) * (1.0 - smoothstep(0.3, 0.7, rfp.y));
    float streak = xProf * yProf * on * (0.3 + 0.7 * hash11(rh * 3.1));
    return streak;
}

// Puddle ripple: subtle circular expanding rings on lower portion of frame
float puddleRipple(vec2 uv, float t) {
    float result = 0.0;
    for (int i = 0; i < 5; i++) {
        float fi = float(i);
        float cx = mix(0.1, 0.9, hash11(fi * 7.31 + 5.0));
        float cy = mix(0.0, 0.12, hash11(fi * 3.17 + 2.0));
        float period = 0.6 + hash11(fi * 1.9) * 0.8;
        float phase = mod(t * (0.7 + hash11(fi * 2.3) * 0.6) + fi * 0.37, period) / period;
        float r = phase * 0.06;
        float d = abs(length(uv - vec2(cx, cy)) - r);
        float fade = (1.0 - phase) * (1.0 - phase);
        result += exp(-(d * d) / (0.003 * 0.003)) * fade * 0.4;
    }
    return result;
}

// ---------- main ----------
void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    vec2 uva = vec2(uv.x * aspect, uv.y);

    float aR     = min(audioReact, 1.5);
    float bassP  = clamp(audioBass, 0.0, 1.0) * aR;
    float subP   = clamp(audioSub,  0.0, 1.0) * aR;
    float midP   = clamp(audioMid,  0.0, 1.0) * aR;
    float punchP = clamp(audioPunch, 0.0, 1.0) * aR;
    float kick = smoothstep(0.03, 0.50, max(audioBass, audioSub) * audioReact);
    float bass = clamp(audioBass * audioReact, 0.0, 2.0);
    float mid  = clamp(audioMid  * audioReact, 0.0, 2.0);
    float high = clamp(audioHigh * audioReact, 0.0, 2.0);
    float presence  = smoothstep(0.06, 0.30, audioLevel * aR);
    float kickTrace = clamp(1.35 * audioBeatPulse + 0.9 * audioPunch + 0.5 * subP, 0.0, 1.0);
    float boltGate  = mix(1.0, 0.10 + 0.90 * kickTrace, presence);
    float stormAmp = stormDensity * (1.0 + 0.45 * mid);

    // ===== STORMCLOUDS =====
    vec3 sky = mix(skyBotColor.rgb, skyTopColor.rgb, smoothstep(-0.05, 1.05, uv.y));

    vec2 farUv = uv * vec2(1.6, 0.95);
    farUv.x *= aspect;
    float farDrift = TIME * cloudDrift * 0.35;
    // MOTION: cloud churn — the deck boils in place instead of only sliding
    vec2 churn = cloudChurn * 0.35 * vec2(fbm3(farUv * 0.45 + TIME * 0.07) - 0.5,
                                          fbm3(farUv * 0.45 + 7.3 - TIME * 0.06) - 0.5);
    float farClouds = fbm(farUv + vec2(farDrift, farDrift * 0.18) + churn * 0.6);
    farClouds = pow(farClouds, mix(1.7, 0.9, stormAmp));

    vec2 nearUv = uv * vec2(3.6, 1.8);
    nearUv.x *= aspect;
    float nearDrift = TIME * cloudDrift * 1.25 * cloudParallax;   // MOTION: near/far layer speed ratio
    float nearClouds = fbm(nearUv + vec2(nearDrift, -nearDrift * 0.4) + churn);
    nearClouds = pow(nearClouds, mix(1.4, 0.7, stormAmp));

    float shimmer = vnoise(uv * 22.0 + TIME * (0.6 + high * 1.4));
    float shimmerMix = high * 0.18;

    vec3 col = sky;
    vec3 farTone  = sky * 0.6  + vec3(0.018, 0.022, 0.030);
    vec3 nearTone = sky * 0.32 + vec3(0.008, 0.010, 0.018);
    col = mix(col, farTone,  clamp(farClouds  * stormAmp * 1.05, 0.0, 1.0));
    col = mix(col, nearTone, clamp(nearClouds * stormAmp * 0.85, 0.0, 1.0));
    col += vec3(0.04, 0.05, 0.08) * shimmer * shimmerMix * stormAmp;
    col *= mix(0.55, 1.0, smoothstep(0.95, 0.10, uv.y));
    col *= 1.0 + 0.22 * bassP + 0.12 * midP + 0.12 * subP;

    // ===== BOLT SCHEDULING =====
    float slot = max(boltLifetime * 0.85, 0.28);
    float boltCore   = 0.0;
    float boltSheath = 0.0;
    float boltOuter  = 0.0;
    float flashEnv   = 0.0;
    float aliveAmt   = 0.0;

    // Abstract thunder accumulators
    float absRibbon  = 0.0;
    float absPlasma  = 0.0;
    float absSheet   = 0.0;
    float absCrawl   = 0.0;

    for (int s = -1; s <= 1; s++) {
        float slotIdx  = floor(TIME / slot) + float(s);
        float slotStart = slotIdx * slot;
        float life      = TIME - slotStart;
        float life01    = clamp(life / boltLifetime, 0.0, 1.0);
        if (life < 0.0 || life > boltLifetime) continue;

        float seed   = slotIdx * 13.37 + 1.0;
        float chance = hash11(seed * 0.917);
        float thresh = 1.0 - clamp(boltProbability + 0.35 * kick + 0.12 * mid, 0.0, 0.95);
        if (chance < thresh) continue;

        float boltCount = 1.0 + step(0.55, bass) * step(0.5, hash11(seed + 31.7));

        for (float bi = 0.0; bi < 2.0; bi += 1.0) {
            if (bi >= boltCount) break;
            float bseed = seed + bi * 91.7;

            // --- standard forked bolt ---
            vec3 sd  = boltSDF(uva, bseed, boltBranchDepth, boltJitter, life01);
            float d  = sd.x;
            float env = boltEnv(life01, bseed);
            env *= 1.0 + 0.40 * bassP + 0.35 * subP + 0.30 * punchP;
            env *= boltGate;

            float coreR  = boltCoreWidth;
            float core   = exp(-(d * d) / (coreR * coreR)) * env;
            float sheathR = boltSheathWidth;
            float sheath  = exp(-(d * d) / (sheathR * sheathR)) * env * 0.65;
            float outerR  = boltSheathWidth * 4.0;
            float outer   = exp(-(d * d) / (outerR * outerR)) * env * 0.22;

            boltCore   += core;
            boltSheath += sheath;
            boltOuter  += outer;

            float fenv = exp(-life * 9.5) * step(life, 0.30);
            flashEnv = max(flashEnv, fenv * env);
            aliveAmt = max(aliveAmt, env);

            // --- abstract thunder forms ---
            // Each slot picks one abstract form based on a hash, weighted by abstractThunderMix.
            float formHash = hash11(bseed * 0.331 + 7.0);
            float absEnv   = boltGate * (1.0 + 0.35 * bassP + 0.25 * subP);
            absEnv *= abstractThunderMix;

            if (formHash < 0.28) {
                // Ribbon discharge
                absRibbon += ribbonThunder(uva, bseed + 0.1, life01, aspect) * absEnv;
            } else if (formHash < 0.52) {
                // Plasma ball
                absPlasma += plasmaBall(uva, bseed + 0.2, life01, aspect) * absEnv;
            } else if (formHash < 0.76) {
                // Sheet lightning
                absSheet  += sheetLightning(uv, bseed + 0.3, life01) * absEnv;
            } else {
                // Ground crawl
                absCrawl  += groundCrawl(uva, bseed + 0.4, life01, aspect, groundLine) * absEnv;
            }
        }
    }

    // ===== APPLY BOLT =====
    vec3 coreCol   = boltCoreColor.rgb;
    vec3 sheathCol = boltSheathColor.rgb;
    vec3 outerCol  = mix(sheathCol, vec3(0.35, 0.45, 0.65), 0.4);

    col += outerCol  * clamp(boltOuter,  0.0, 4.0) * 0.85;
    col += sheathCol * clamp(boltSheath, 0.0, 4.0) * 1.10;
    col += coreCol   * clamp(boltCore,   0.0, 6.0) * 1.25;

    // ===== APPLY ABSTRACT THUNDER =====
    // Ribbon: warm white-blue
    col += vec3(0.75, 0.88, 1.0) * clamp(absRibbon, 0.0, 3.5) * 0.90;
    // Plasma ball: violet-cyan
    col += vec3(0.60, 0.72, 1.0) * clamp(absPlasma, 0.0, 3.5) * 0.85;
    // Sheet: soft diffuse blue-white
    col += vec3(0.65, 0.78, 1.0) * clamp(absSheet,  0.0, 2.5) * 0.70;
    // Ground crawl: orange-white ground discharge
    col += vec3(1.0, 0.82, 0.55) * clamp(absCrawl,  0.0, 3.0) * 0.80;

    // ===== WHOLE-CANVAS FLASH WASH =====
    vec3 flashTint = vec3(0.95, 0.97, 1.0);
    float flashAmt = flashEnv * flashIntensity;
    col *= 1.0 + flashAmt * 1.4;
    col += flashTint * flashAmt * 0.55;

    // ===== RAIN =====
    // MOTION: gusts swing the rain angle back and forth
    float rainAng = rainAngle + rainGust * 0.35 * sin(TIME * 0.55) * (0.7 + 0.3 * sin(TIME * 1.7));
    if (rainDensity > 0.0) {
        // Layer 1: primary heavy rain
        for (int rl = 0; rl < 2; rl++) {
            float layer = float(rl);
            float scale = mix(1.0, 0.55, layer);
            float alpha = mix(0.22, 0.10, layer);
            vec2 ruv = uv;
            ruv.x += ruv.y * rainAng;
            ruv *= vec2(180.0 * scale, 22.0 * scale);
            ruv.y += TIME * rainSpeed * (16.0 + 8.0 * layer);
            ruv.x += hash11(floor(ruv.y) * 1.7 + layer * 13.0) * 0.7;
            vec2 rip = floor(ruv);
            vec2 rfp = fract(ruv);
            float rh = hash12(rip + vec2(layer * 31.0, 0.0));
            float on = step(1.0 - rainDensity * 0.45, rh);
            float xProf = smoothstep(0.50, 0.46, abs(rfp.x - 0.5));
            float yProf = smoothstep(0.0, 0.15, rfp.y) * (1.0 - smoothstep(0.55, 1.0, rfp.y));
            float streak = xProf * yProf * on;
            streak *= 0.5 + 0.5 * hash11(rh * 7.0 + layer * 3.0);
            streak *= (1.0 - flashEnv * 0.55);
            col += vec3(0.55, 0.65, 0.82) * streak * alpha;
        }

        // Layer 2: fine mist / micro-droplets (always subtle)
        float mist = mistyRain(uv, rainSpeed, rainAng * 0.6, rainDensity, TIME);
        mist *= (1.0 - flashEnv * 0.7) * 0.06;
        col += vec3(0.62, 0.72, 0.88) * mist;

        // Layer 3: puddle ripples near ground
        if (groundLine > 0.01) {
            float rippleUVy = uv.y / max(groundLine + 0.06, 0.08);
            float rippleMask = smoothstep(0.0, 1.0, 1.0 - uv.y / max(groundLine + 0.08, 0.09));
            float ripple = puddleRipple(vec2(uv.x, rippleUVy * 0.12), TIME);
            ripple *= rippleMask * rainDensity * 0.18;
            col += vec3(0.45, 0.58, 0.78) * ripple;
        }
    }

    // ===== GROUND SILHOUETTE =====
    if (groundLine > 0.0) {
        float horizon = groundLine
            + 0.022 * fbm(vec2(uv.x * 7.0, 0.0))
            + 0.012 * vnoise(vec2(uv.x * 22.0, 0.0));
        float ground = smoothstep(horizon + 0.004, horizon - 0.004, uv.y);
        col = mix(col, vec3(0.005, 0.008, 0.014), ground);
        float rimGlow = aliveAmt * 0.35 + flashAmt * 0.55
                      + clamp(absCrawl, 0.0, 1.0) * 0.45;
        col += vec3(0.45, 0.55, 0.72) * ground * rimGlow;
        // Ground crawl orange tint on rim
        col += vec3(0.9, 0.65, 0.25) * ground * clamp(absCrawl, 0.0, 1.0) * 0.35;
    }

    // ===== POST: chromatic aberration during flash =====
    if (flashAmt > 0.02) {
        vec2 ca = (uv - 0.5) * flashAmt * 0.012;
        col.r *= 1.0 + length(ca) * 6.0;
        col.b *= 1.0 + length(ca) * 6.0 * 0.7;
    }

    // ===== WHOLE-FRAME AUDIO FOLLOW =====
    col *= 1.0 + 0.12 * bassP + 0.15 * subP + 0.08 * midP
               + 0.22 * audioBeatPulse * aR;

    // ===== POST: bloom =====
    float bright = max(max(col.r, col.g), col.b);
    float bloom = smoothstep(0.85, 1.6, bright);
    col += col * bloom * 0.35;

    // ===== POST: film grain =====
    float grain = hash12(gl_FragCoord.xy + TIME * 60.0) - 0.5;
    col += vec3(grain) * 0.022;

    // ===== POST: vignette =====
    vec2 vc = uv - 0.5;
    float vig = 1.0 - dot(vc, vc) * 0.85;
    col *= vig;

    // ===== COLOR GRADE =====
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        col = clamp(hM * col, 0.0, 1.0);
    }
    col = clamp(col, 0.0, 1.8);
    gl_FragColor = vec4(col, 1.0);
}