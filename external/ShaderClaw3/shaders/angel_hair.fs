/*{
  "DESCRIPTION": "Angel Hair — sinuous ink-strand particles ride a divergence-free noise flow on a paper canvas, combined with an energy-field sensing swarm. Hair strands are bold and visible; the palettes, audio reactivity, and visual effects are deeply parametric. Bass presses ink darker and swells energy sources, mids quicken strands and feed roaming sources, highs shimmer colors. 10x variety in effects via new controls.",
  "CREDIT": "nimitz (stormoid, shadertoy 4sGSDw) CC BY-NC-SA 3.0, Kubayashi party-lights concept, ShaderClaw audio port",
  "CATEGORIES": ["Generator", "Particles"],
  "INPUTS": [
    { "NAME": "speed",          "LABEL": "Speed",           "TYPE": "float",  "DEFAULT": 1.0,  "MIN": 0.1,  "MAX": 4.0 },
    { "NAME": "hairCount",      "LABEL": "Hair Count",      "TYPE": "float",  "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 1.0 },
    { "NAME": "hairThickness",  "LABEL": "Hair Thickness",  "TYPE": "float",  "DEFAULT": 1.0,  "MIN": 0.1,  "MAX": 6.0 },
    { "NAME": "hairBrightness", "LABEL": "Hair Brightness", "TYPE": "float",  "DEFAULT": 1.0,  "MIN": 0.1,  "MAX": 5.0 },
    { "NAME": "hairSaturation", "LABEL": "Hair Saturation", "TYPE": "float",  "DEFAULT": 0.5,  "MIN": 0.0,  "MAX": 1.0 },
    { "NAME": "flowScale",      "LABEL": "Flow Scale",      "TYPE": "float",  "DEFAULT": 1.0,  "MIN": 0.1,  "MAX": 4.0 },
    { "NAME": "flowTwist",      "LABEL": "Flow Twist",      "TYPE": "float",  "DEFAULT": 1.0,  "MIN": 0.0,  "MAX": 4.0 },
    { "NAME": "trailDecay",     "LABEL": "Trail Decay",     "TYPE": "float",  "DEFAULT": 0.993,"MIN": 0.96, "MAX": 0.9998 },
    { "NAME": "loopTime",       "LABEL": "Loop Time",       "TYPE": "float",  "DEFAULT": 26.7, "MIN": 5.0,  "MAX": 120.0 },
    { "NAME": "audioReact",     "LABEL": "Audio React",     "TYPE": "float",  "GROUP": "Audio Reactivity", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "bassResponse",   "LABEL": "Bass → Ink Dark", "TYPE": "float",  "GROUP": "Audio Reactivity", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 3.0 },
    { "NAME": "midResponse",    "LABEL": "Mid → Speed",     "TYPE": "float",  "GROUP": "Audio Reactivity", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 3.0 },
    { "NAME": "highResponse",   "LABEL": "High → Color",    "TYPE": "float",  "GROUP": "Audio Reactivity", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 3.0 },
    { "NAME": "bassThick",      "LABEL": "Bass → Thickness","TYPE": "float",  "GROUP": "Audio Reactivity", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "audioSpawnBoost","LABEL": "Audio Spawn Burst","TYPE": "float", "GROUP": "Audio Reactivity", "DEFAULT": 0.5, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "paletteMode",    "LABEL": "Palette Mode",    "TYPE": "float",  "GROUP": "Color", "DEFAULT": 0.0, "MIN": 0.0, "MAX": 4.0 },
    { "NAME": "paletteSpeed",   "LABEL": "Palette Speed",   "TYPE": "float",  "GROUP": "Color", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 8.0 },
    { "NAME": "paperTone",      "LABEL": "Paper Tone",      "TYPE": "float",  "GROUP": "Color", "DEFAULT": 1.0, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "glowAmount",     "LABEL": "Glow Amount",     "TYPE": "float",  "GROUP": "Color", "DEFAULT": 0.3, "MIN": 0.0, "MAX": 2.0 },
    { "NAME": "fieldInfluence", "LABEL": "Field Influence", "TYPE": "float",  "GROUP": "Color", "DEFAULT": 0.4, "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "tintColor",      "LABEL": "Tint",            "TYPE": "color",  "GROUP": "Color", "DEFAULT": [1.0, 1.0, 1.0, 1.0] },
    { "NAME": "brightness",     "LABEL": "Brightness",      "TYPE": "float",  "GROUP": "Color", "DEFAULT": 1.0, "MIN": 0.2, "MAX": 3.0 },
    { "NAME": "warpAmount",     "LABEL": "Warp Amount",     "TYPE": "float",  "DEFAULT": 0.08, "MIN": 0.0, "MAX": 0.5 },
    { "NAME": "spotlightOn",    "LABEL": "Spotlight",       "TYPE": "float",  "DEFAULT": 0.5,  "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "chromaticAb",    "LABEL": "Chromatic Aberr", "TYPE": "float",  "DEFAULT": 0.2,  "MIN": 0.0, "MAX": 1.0 },
    { "NAME": "vignetteStr",    "LABEL": "Vignette",        "TYPE": "float",  "DEFAULT": 0.4,  "MIN": 0.0, "MAX": 1.0 }
  ],
  "PASSES": [
    { "TARGET": "agentBuf",  "PERSISTENT": true },
    { "TARGET": "trailBuf",  "PERSISTENT": true },
    { "TARGET": "fieldBuf",  "PERSISTENT": true },
    {}
  ]
}*/

#define TAU 6.283185307179586
#define N_HAIR_MAX 400
#define N_SWARM 32

// ---- helpers ----------------------------------------------------------------
float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float aBass()  { return pow(knee(audioBass,  0.05, 0.85), 1.6) * bassResponse; }
float aMid()   { return pow(knee(audioMid,   0.08, 0.90), 1.3) * midResponse; }
float aHigh()  { return pow(knee(audioHigh,  0.10, 0.90), 1.2) * highResponse; }
float aLevel() { return knee(audioLevel, 0.05, 0.90); }

mat2 rot2(float a) { float s=sin(a),c=cos(a); return mat2(c,-s,s,c); }

// Dave Hoskins hash
vec2 hash2(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * vec3(443.897, 441.423, 437.195));
    p3 += dot(p3.zxy, p3.yxz + 19.19);
    return fract(vec2(p3.x*p3.y, p3.z*p3.x)) * 2.0 - 1.0;
}
vec4 hash4(vec3 p) {
    vec4 p4 = fract(p.xyzx * vec4(0.1031,0.1030,0.0973,0.1099));
    p4 += dot(p4, p4.wzxy + 19.19);
    return fract((p4.xxyz + p4.yzzw) * p4.zywx);
}

// gradient noise + fbm
float gnoise(vec2 p) {
    vec2 i=floor(p), f=fract(p);
    vec2 u=f*f*(3.0-2.0*f);
    return mix(mix(dot(hash2(i),         f),
                   dot(hash2(i+vec2(1.0,0.0)),f-vec2(1.0,0.0)),u.x),
               mix(dot(hash2(i+vec2(0.0,1.0)),f-vec2(0.0,1.0)),
                   dot(hash2(i+vec2(1.0,1.0)),f-vec2(1.0,1.0)),u.x),u.y);
}
float fbm2(vec2 p, float tm) {
    p *= 2.0 * flowScale;
    p -= tm;
    float z=2.0, rz=0.0;
    p += TIME*0.001 + 0.1;
    const mat2 m2 = mat2(0.80,-0.60,0.60,0.80);
    for (int i=1;i<7;i++){
        rz += abs((gnoise(p)-0.5)*2.0)/z;
        z *= 1.93;
        p = m2*p*2.0;
    }
    return rz;
}

// ---- 16-bit encode / decode -------------------------------------------------
vec4 encPos(vec2 p) {
    p = clamp((p+1.1)/2.2, 0.0, 1.0)*255.0;
    return vec4(floor(p.x)/255.0, fract(p.x), floor(p.y)/255.0, fract(p.y));
}
vec2 decPos(vec4 t) { return vec2(t.r+t.g/255.0, t.b+t.a/255.0)*2.2-1.1; }
vec4 encState(float ang, float spd) {
    float e = fract(ang/TAU)*255.0;
    return vec4(floor(e)/255.0, fract(e), clamp(spd/2.0,0.0,1.0), 1.0);
}

// encode for swarm (0-1 space)
vec4 encPosN(vec2 p) {
    p = clamp(p,0.0,1.0)*255.0;
    return vec4(floor(p.x)/255.0, fract(p.x), floor(p.y)/255.0, fract(p.y));
}
vec2 decPosN(vec4 t) { return vec2(t.r+t.g/255.0, t.b+t.a/255.0); }

// agent buffer layout: hair particles in row 0, swarm in row 1
// Hair:  texel 2i   = pos,   texel 2i+1 = state
// Swarm: texel 2*N_HAIR_MAX + 2j = pos,  +1 = state

int hairN() { return int(clamp(hairCount, 0.0, 1.0) * float(N_HAIR_MAX-1)) + 1; }

vec4 fetchHairAgent(int texel) {
    float fx = (float(texel) + 0.5) / RENDERSIZE.x;
    float fy = 0.5 / RENDERSIZE.y;
    return texture2D(agentBuf, vec2(fx, fy));
}
vec4 fetchSwarmAgent(int texel) {
    float fx = (float(texel) + 0.5) / RENDERSIZE.x;
    float fy = 1.5 / RENDERSIZE.y;
    return texture2D(agentBuf, vec2(fx, fy));
}

vec2 hairPos(int i) { return decPos(fetchHairAgent(2*i)); }
vec2 swarmPos(int j) { return decPosN(fetchSwarmAgent(2*j)); }

bool resetPulse() {
    return FRAMEINDEX < 12 || mod(TIME * speed, loopTime) < max(TIMEDELTA * speed, 0.034);
}

// ---- palette ----------------------------------------------------------------
vec3 pickPalette(float t, float pMode) {
    int pm = int(clamp(pMode, 0.0, 3.99));
    if (pm == 0) {
        return 0.5 + 0.5*cos(vec3(0.0,0.8,1.6) + t);
    } else if (pm == 1) {
        return 0.5 + 0.5*cos(vec3(2.0,3.0,4.5) + t);
    } else if (pm == 2) {
        return clamp(vec3(t*2.0, t*1.2-0.3, t*0.4-0.5), 0.0, 1.0);
    } else {
        return 0.5 + 0.5*sin(vec3(0.0,2.094,4.188) + t*3.0);
    }
}

// ---- PASS 0: particle/swarm update ------------------------------------------
vec4 passAgents() {
    int hn = hairN();
    int texel = int(gl_FragCoord.x);
    int row   = int(gl_FragCoord.y);

    // Row 0: hair agents
    if (row == 0) {
        int i = texel / 2;
        bool isPos = (texel - 2*i) == 0;
        if (i >= hn) return vec4(0.0);

        float ar   = audioReact;
        float midP = aMid();

        vec2 pos = hairPos(i);
        vec4 stT = fetchHairAgent(2*i+1);
        float ang = (stT.r + stT.g/255.0)*TAU;
        float spdVal = stT.b*2.0;
        vec2 vel = vec2(cos(ang), sin(ang))*spdVal;

        float n1a = fbm2(pos, 0.0);
        float n1b = fbm2(pos.yx, 0.0);
        float nn  = fbm2(vec2(n1a,n1b), 0.0)*5.8*flowTwist + 0.5;
        vec2  dir = vec2(cos(nn), sin(nn));
        vel = mix(vel, dir*1.5, 0.05);

        float spdMult = mix(1.0, 0.6+1.4*midP, ar);
        float bassMod = mix(1.0, 1.0+0.5*aBass(), ar*bassThick);
        pos += vel * 0.006 * speed * spdMult * bassMod;

        // wrap / reset at edge
        if (pos.x > 1.05 || abs(pos.y) > 1.05) {
            vec2 h = hash2(vec2(float(i)*0.713, 4.7));
            pos = vec2(-0.99, h.x*0.45);
            vel = vec2(1.5,0.0)*0.1;
        }
        float spawnThresh = mix(0.005, 0.002, audioSpawnBoost * ar * aBass());
        vec4 rng = hash4(vec3(float(i)*0.217, TIME*0.07, 1.1));
        if (rng.x < spawnThresh) {
            vec2 h = hash2(vec2(float(i)*1.13, TIME));
            pos = h*0.9;
            vel = vec2(1.5,0.0)*0.12;
        }
        if (resetPulse()) {
            vec2 h = hash2(vec2(float(i)*0.713, 9.1));
            pos = vec2(h.y*0.99, h.x*0.5);
            vel = vec2(1.5,0.0)*0.1;
        }

        if (isPos) return encPos(pos);
        return encState(atan(vel.y,vel.x), length(vel));
    }

    // Row 1: swarm agents (0-1 space)
    if (row == 1) {
        int j = texel / 2;
        bool isPosT = (texel - 2*j) == 0;
        if (j >= N_SWARM) return vec4(0.0);

        vec4 posT   = fetchSwarmAgent(2*j);
        vec4 stateT = fetchSwarmAgent(2*j+1);
        vec2 pos    = decPosN(posT);
        float dir   = stateT.r * TAU;
        float mood  = stateT.g;

        float ar = audioReact;
        float T  = TIME * speed;
        float dt = clamp(TIMEDELTA, 0.0005, 0.06) * speed;

        vec4 noise   = hash4(vec3(float(j)*0.123 + pos.x, TIME*0.31, 3.3));
        vec4 idNoise = hash4(vec3(float(j), 1.234, 7.7));

        float baseSpeed    = mix(0.04, 0.15, idNoise.x);
        float wander       = mix(0.05, 1.2,  idNoise.y);
        float turnfactor   = mix(0.08, 0.7,  idNoise.z);
        float sensorLength = mix(0.09, 0.33, idNoise.w);

        mat2 r2 = rot2(dir);
        vec2 s0 = pos + r2*(vec2(1.0,  0.0)    *sensorLength);
        vec2 s1 = pos + r2*(vec2(1.0,  0.0583) *sensorLength);
        vec2 s2 = pos + r2*(vec2(1.0, -0.0583) *sensorLength);
        float f  = texture2D(fieldBuf, s0).x;
        float fl = texture2D(fieldBuf, s1).x;
        float fr = texture2D(fieldBuf, s2).x;
        float energyAhead = (f+fl+fr)/3.0;
        float asym = fl - fr;

        mood += energyAhead*0.03 + smoothstep(0.96,1.0,noise.x)*0.08 + ar*0.06*aBass();
        mood *= 0.985;
        mood = clamp(mood, 0.0, 1.0);
        float chaos = mix(0.0, 1.5, mood);

        if (f>fl && f>fr) {
        } else if (f<fl && f<fr) {
            dir += wander*(noise.z-0.5)*(1.0+chaos);
        } else if (fl < fr) {
            dir += turnfactor*(1.0+0.7*chaos);
        } else {
            dir -= turnfactor*(1.0+0.7*chaos);
        }
        dir += asym * mix(-0.12, 0.15, mood);
        dir += 0.07*sin(T*0.9 + idNoise.x*TAU);

        float spd = baseSpeed*(1.0+1.8*mood)
                  * mix(0.8,1.25,smoothstep(0.02,0.25,energyAhead))
                  * mix(1.0, 0.7+0.9*aLevel(), ar);
        pos += rot2(dir)*vec2(spd,0.0)*dt;

        float margin = 0.03;
        if (pos.x < margin)       dir += 0.08 + 0.12*noise.y;
        if (pos.x > 1.0-margin)   dir -= 0.08 + 0.12*noise.y;
        if (pos.y < margin)       dir += 0.08 + 0.12*noise.x;
        if (pos.y > 1.0-margin)   dir -= 0.08 + 0.12*noise.x;
        vec2 bv = clamp(pos, vec2(0.0), vec2(1.0));
        if (pos.x != bv.x) dir = TAU*0.5 - dir;
        if (pos.y != bv.y) dir = TAU - dir;
        pos = bv;
        dir = mod(dir, TAU);

        if (FRAMEINDEX < 2 || noise.x < 0.005*noise.y) {
            vec4 iN = hash4(vec3(float(j), 0.0, TIME*0.01));
            pos  = mix(vec2(0.1), vec2(0.9), iN.xy);
            dir  = iN.z * TAU;
            mood = iN.x * 0.2;
        }
        if (isPosT) return encPosN(pos);
        return vec4(dir/TAU, mood, 0.0, 1.0);
    }

    return vec4(0.0);
}

// ---- PASS 1: trail/ink accumulation -----------------------------------------
vec4 passTrail() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 p  = (uv - 0.5);
    p.x *= RENDERSIZE.x / RENDERSIZE.y;
    p *= 1.1;

    float ar     = audioReact;
    float bassP  = aBass();
    float highP  = aHigh();
    float levelP = aLevel();
    float midP   = aMid();

    float T = TIME * speed;
    float phaseBase = T * 0.07 * paletteSpeed + 2.5 + ar*0.3*highP;

    // Hair strand accumulation
    int hn = hairN();
    float thickK = hairThickness * mix(1.0, 1.0 + bassThick*bassP, ar);
    float K = 350.0 / (thickK*thickK);
    vec3 inkHair = vec3(0.0);
    for (int i = 0; i < N_HAIR_MAX; i++) {
        if (i >= hn) break;
        vec2 pos = hairPos(i);
        float d2 = dot(p - pos, p - pos) * K;
        if (d2 > 18.0) continue;
        float d = 0.035 / (d2 + 0.0008);

        vec3 pCol = abs(sin(vec3(2.0, 3.4, 1.2)
                    * (phaseBase + float(i)*0.0031)
                    + vec3(0.8, 0.0, 1.2))
                    * 0.7 + 0.3);

        float palT = phaseBase + float(i)*0.007 + ar*highP*2.0;
        vec3 palCol = pickPalette(palT, paletteMode);
        pCol = mix(pCol, palCol, 0.6);

        float lum = dot(pCol, vec3(0.333));
        pCol = mix(vec3(lum), pCol, hairSaturation);

        inkHair += d * pCol * hairBrightness;
    }
    float inkScale = mix(1.0, 0.3 + 1.9*bassP + 0.7*levelP, ar) * bassResponse;
    inkHair *= inkScale * 0.12;

    // Swarm particle deposits
    vec3 inkSwarm = vec3(0.0);
    for (int j = 0; j < N_SWARM; j++) {
        vec2 spos = swarmPos(j);
        vec2 sc = (spos - 0.5);
        sc.x *= RENDERSIZE.x / RENDERSIZE.y;
        sc *= 1.1;
        float dm = distance(p, sc);
        float sw = smoothstep(0.04, 0.0, dm);
        float palT2 = phaseBase + float(j)*0.05 + midP;
        inkSwarm += sw * pickPalette(palT2, paletteMode) * 0.6
                  * mix(1.0, 0.5+1.5*midP, ar);
    }

    // combine with decay
    vec3 prev = texture2D(trailBuf, uv).rgb;
    float decay = trailDecay + ar * 0.001 * levelP;
    decay = clamp(decay, 0.96, 0.9998);
    vec3 stored = clamp(max(prev * decay, inkHair + inkSwarm), 0.0, 1.0);
    if (FRAMEINDEX < 2) stored = vec3(0.0);
    return vec4(stored, 1.0);
}

// ---- PASS 2: energy field (swarm sensors) -----------------------------------
vec4 passField() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec4 prev = texture2D(fieldBuf, uv);
    vec4 C = prev * 0.94;

    float ar = audioReact;
    float T = TIME * speed;

    float dm = 1e6;
    for (int j = 0; j < N_SWARM; j++) {
        float d = distance(gl_FragCoord.xy, swarmPos(j)*RENDERSIZE.xy);
        dm = min(dm, d);
    }
    C += exp(-dm*dm * 0.001);
    C += exp(-0.02*(dm-6.0)*(dm-6.0));
    C = clamp(C, 0.0, 1.0);

    float a = T * 0.2;
    float r = RENDERSIZE.y * 0.5;
    vec2 ep = RENDERSIZE.xy*0.5 + r*vec2(cos(a), sin(a));
    float ed = distance(gl_FragCoord.xy, ep);
    float cR = (50.0 + 75.0*sin(T*2.0)) * mix(1.0, 0.4+1.8*aBass(), ar);
    C += vec4(smoothstep(cR, cR-10.0, ed)) * 0.6;

    vec2 p1 = RENDERSIZE.xy * vec2(0.3+0.3*cos(T), 0.5+0.2*sin(T));
    vec2 p2 = RENDERSIZE.xy * vec2(0.7+0.2*cos(T*1.3), 0.5+0.2*sin(T*0.8));
    float c1 = smoothstep(60.0,  5.0, distance(gl_FragCoord.xy, p1));
    float c2 = smoothstep(50.0, 35.0, distance(gl_FragCoord.xy, p2));
    C += vec4(c1+c2) * 0.6 * mix(1.0, 0.3+1.9*aMid(), ar);
    C = clamp(C, 0.0, 1.0);

    return mix(C, prev, 0.22);
}

// ---- PASS 3: composite -------------------------------------------------------
vec4 passImage() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 q  = uv - 0.5;
    q.x *= RENDERSIZE.x / RENDERSIZE.y;

    float ar  = audioReact;
    float T   = TIME * speed;
    float bassP  = aBass();
    float highP  = aHigh();
    float levelP = aLevel();
    float midP   = aMid();

    // chromatic aberration
    float caOff = chromaticAb * 0.008 * mix(1.0, 1.0+highP, ar);
    vec2 uvR = uv + vec2(caOff, 0.0);
    vec2 uvB = uv - vec2(caOff, 0.0);
    vec3 trailR = texture2D(trailBuf, uvR).rgb;
    vec3 trailG = texture2D(trailBuf, uv).rgb;
    vec3 trailB = texture2D(trailBuf, uvB).rgb;
    vec3 trail = vec3(trailR.r, trailG.g, trailB.b);

    // field warp
    vec4 fieldC = texture2D(fieldBuf, uv);
    vec2 warp   = (fieldC.xy - 0.5) * warpAmount * mix(1.0, 1.0+0.5*bassP, ar);
    vec3 trailW = texture2D(trailBuf, uv + warp).rgb;
    trail = mix(trail, trailW, fieldInfluence);

    float trailLen = length(trail);
    float sugarLen = length(fieldC.rgb);

    // animated palette
    float palT  = T * paletteSpeed * 0.4 + sugarLen*7.2 + trailLen + ar*2.0*highP;
    vec3 palette = pickPalette(palT, paletteMode);

    // paper background
    float vd = dot(q, q);
    vec3 paper = mix(
        vec3(0.92, 0.88, 0.78),
        vec3(0.98, 0.96, 0.90),
        paperTone
    ) * (1.0 - vd*0.25);

    // inked strands — settle in over the first seconds instead of
    // opening at full chaos (the sim needs a moment to organize)
    float settle = mix(0.35, 1.0, smoothstep(0.3, 4.5, TIME));
    vec3 inkColor = trail * palette * (1.5 + glowAmount) * settle;

    // paper composite: paper minus ink = print effect
    vec3 printed = clamp(paper - inkColor, 0.0, 1.0);

    // field glow on top
    vec3 glow = sugarLen * palette * glowAmount * 0.6 * settle;
    vec3 col = printed + glow;

    // swarm core dots
    float swarmCore = 0.0;
    for (int j = 0; j < N_SWARM; j++) {
        vec2 spos = swarmPos(j);
        float dm = distance(uv, spos);
        swarmCore += smoothstep(0.012, 0.0, dm);
    }
    col += swarmCore * palette * 0.3 * mix(1.0, 0.5+1.5*midP, ar);

    // spotlight
    vec2 sm = 0.5 + 0.35*vec2(cos(T*0.4), sin(T*0.53));
    float md = distance(uv, sm);
    col += spotlightOn * vec3(0.2,0.4,1.0) * exp(-18.0*md) * 0.4
         * mix(1.0, 0.3+2.0*highP, ar);

    // vignette
    col *= 1.0 - vignetteStr * smoothstep(0.3, 0.85, length(q*0.85));

    // audio level lift
    col *= 1.0 + ar*(0.4*levelP + 0.2*bassP);

    col *= tintColor.rgb * brightness;
    return vec4(clamp(col, 0.0, 1.0), 1.0);
}

void main() {
    if      (PASSINDEX == 0) gl_FragColor = passAgents();
    else if (PASSINDEX == 1) gl_FragColor = passTrail();
    else if (PASSINDEX == 2) gl_FragColor = passField();
    else                     gl_FragColor = passImage();
}