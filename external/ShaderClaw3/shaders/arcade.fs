/*{
  "DESCRIPTION": "Arcade — glowing agents with per-particle personalities roam a diffusing 'sugar' energy field, sensing it with steering sensors, leaving iridescent trails and clustering into emergent flows. Bass swells the energy sources and excites the swarm, mids drive the roaming feeders, highs shimmer the palette and spotlight.",
  "CREDIT": "Based on 'Party Lights' by Hiromune Kubayashi (agents-from-nearest-particle lecture example, shadertoy 7fl3zH), ShaderClaw audio port",
  "CATEGORIES": [
    "Generator",
    "Particles"
  ],
  "INPUTS": [
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.1,
      "MAX": 3.0
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "GROUP": "Audio Reactivity",
      "DEFAULT": 0.5,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "trailFade",
      "LABEL": "Trail Fade",
      "TYPE": "float",
      "DEFAULT": 0.94,
      "MIN": 0.80,
      "MAX": 0.98
    },
    {
      "NAME": "tintColor",
      "LABEL": "Tint",
      "TYPE": "color",
      "GROUP": "Color",
      "DEFAULT": [1.0, 1.0, 1.0, 1.0]
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 3.0
    }
  ],
  "PASSES": [
    {
      "TARGET": "agentBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "trailBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "fieldBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

#define N_PARTICLES 48
#define TWOPI 6.283185307179586

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

mat2 rotate2d(float angle) {
    float s = sin(angle);
    float c = cos(angle);
    return mat2(c, -s, s, c);
}

#define RANDOM_SCALE vec4(0.1031, 0.1030, 0.0973, 0.1099)
vec4 random4(vec3 p) {
    vec4 p4 = fract(p.xyzx * RANDOM_SCALE);
    p4 += dot(p4, p4.wzxy + 19.19);
    return fract((p4.xxyz + p4.yzzw) * p4.zywx);
}

// ---- agent-buffer encode / decode -------------------------------------------
// Positions live in 0..1 normalized space, packed 16-bit per axis (hi/lo bytes)
// so the persistent 8-bit buffer round-trips them smoothly.
vec4 encodePos(vec2 p) {
    p = clamp(p, 0.0, 1.0);
    vec2 e = p * 255.0;
    return vec4(floor(e.x)/255.0, fract(e.x), floor(e.y)/255.0, fract(e.y));
}
vec2 decodePos(vec4 t) {
    return vec2(t.r + t.g/255.0, t.b + t.a/255.0);
}
// agent data lives in the bottom pixel row of a full-size buffer
vec4 fetchAgent(int texel) {
    return texture2D(agentBuf, vec2((float(texel) + 0.5) / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
}
vec2 particlePos(int i)  { return decodePos(fetchAgent(2*i)); }

// audio envelopes
float aBass()  { return pow(knee(audioBass, 0.05, 0.85), 1.6); }
float aMid()   { return pow(knee(audioMid,  0.08, 0.90), 1.3); }
float aHigh()  { return pow(knee(audioHigh, 0.10, 0.90), 1.2); }
float aLevel() { return knee(audioLevel, 0.05, 0.90); }

// ---- pass 0: particle update -------------------------------------------------
// Each particle owns two texels: 2i = packed position, 2i+1 = (dir, mood).
// Both texels recompute the same update from last frame's buffer, so the
// state stays consistent without needing float targets.
vec4 passAgents() {
    int texel = int(gl_FragCoord.x);
    int i = texel / 2;
    bool isPosTexel = (texel - 2*i) == 0;
    if (i >= N_PARTICLES) return vec4(0.0);

    vec4 posT   = fetchAgent(2*i);
    vec4 stateT = fetchAgent(2*i + 1);
    vec2 pos   = decodePos(posT);
    float dir  = stateT.r * TWOPI;
    float mood = stateT.g;

    float T  = TIME * speed;
    float dt = clamp(TIMEDELTA, 0.0005, 0.06) * speed;
    float ar = audioReact;

    // per-frame noise + fixed per-particle personality
    vec4 noise   = random4(vec3(float(i) * 0.123 + pos.x, TIME * 0.31, 3.3));
    vec4 idNoise = random4(vec3(float(i), 1.234, 7.7));

    float baseSpeed    = mix(0.04, 0.15, idNoise.x);
    float wander       = mix(0.05, 1.2,  idNoise.y);
    float turnfactor   = mix(0.08, 0.7,  idNoise.z);
    float sensorLength = mix(0.09, 0.33, idNoise.w);

    // three forward sensors (narrow spread, like the original 12:0.7 ratio)
    mat2 rot = rotate2d(dir);
    vec2 s0 = pos + rot * (vec2(1.0,  0.0)    * sensorLength);
    vec2 s1 = pos + rot * (vec2(1.0,  0.0583) * sensorLength);
    vec2 s2 = pos + rot * (vec2(1.0, -0.0583) * sensorLength);

    vec4 F  = texture2D(fieldBuf, s0);
    vec4 FL = texture2D(fieldBuf, s1);
    vec4 FR = texture2D(fieldBuf, s2);
    float f  = F.x;
    float fl = FL.x;
    float fr = FR.x;
    float energyAhead = dot(F.rgb, vec3(0.333));
    float asym = fl - fr;

    // mood: excited by strong fields, random twitches, loud music; calms slowly
    mood += energyAhead * 0.03;
    mood += smoothstep(0.96, 1.0, noise.x) * 0.08;
    mood += ar * 0.06 * aBass();
    mood *= 0.985;
    mood = clamp(mood, 0.0, 1.0);

    float chaos = mix(0.0, 1.5, mood);
    if (f > fl && f > fr) {
        // forward strongest: keep going
    } else if (f < fl && f < fr) {
        dir += wander * (noise.z - 0.5) * (1.0 + chaos);
    } else if (fl < fr) {
        dir += turnfactor * (1.0 + 0.7 * chaos);
    } else if (fr < fl) {
        dir -= turnfactor * (1.0 + 0.7 * chaos);
    }
    dir += asym * mix(-0.12, 0.15, mood);
    dir += 0.07 * sin(T * 0.9 + idNoise.x * TWOPI);

    float spd = baseSpeed * (1.0 + 1.8 * mood)
              * mix(0.8, 1.25, smoothstep(0.02, 0.25, energyAhead))
              * mix(1.0, 0.7 + 0.9 * aLevel(), ar);

    rot = rotate2d(dir);
    pos += rot * vec2(spd, 0.0) * dt;

    // soft edge avoidance + reflection
    float margin = 0.03;
    if (pos.x < margin)       dir += 0.08 + 0.12 * noise.y;
    if (pos.x > 1.0 - margin) dir -= 0.08 + 0.12 * noise.y;
    if (pos.y < margin)       dir += 0.08 + 0.12 * noise.x;
    if (pos.y > 1.0 - margin) dir -= 0.08 + 0.12 * noise.x;
    vec2 b = clamp(pos, vec2(0.0), vec2(1.0));
    if (pos.x != b.x) dir = TWOPI * 0.5 - dir;
    if (pos.y != b.y) dir = TWOPI - dir;
    pos = b;
    dir = mod(dir, TWOPI);

    // init / occasional respawn keeps behaviors varied
    if (FRAMEINDEX < 2 || noise.x < 0.005 * noise.y) {
        vec4 initNoise = random4(vec3(float(i), 0.0, TIME * 0.01));
        pos  = mix(vec2(0.1), vec2(0.9), initNoise.xy);
        dir  = initNoise.z * TWOPI;
        mood = initNoise.x * 0.2;
    }

    if (isPosTexel) return encodePos(pos);
    return vec4(dir / TWOPI, mood, 0.0, 1.0);
}

// nearest-particle distance in pixels of the current pass
float nearestDist(vec2 fragPx, vec2 sizePx) {
    float dm = 1e6;
    for (int k = 0; k < N_PARTICLES; k++) {
        float d = distance(fragPx, particlePos(k) * sizePx);
        dm = min(dm, d);
    }
    return dm;
}

// ---- pass 1: trails ----------------------------------------------------------
// rgb = decaying trail, a = nearest-particle distance (for the image pass core)
vec4 passTrails() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec4 B = texture2D(trailBuf, uv);
    float decay = mix(trailFade, min(trailFade + 0.04, 0.985),
                      audioReact * aLevel());
    B.rgb *= decay;
    float dm = nearestDist(gl_FragCoord.xy, RENDERSIZE.xy);
    // hits inject brighter deposits — injection stays audio-locked through persistence
    B.rgb += vec3(smoothstep(4.0, 0.0, dm))
           * (1.0 + audioReact * (1.5 * aBass() + 0.8 * aLevel()));
    B.rgb = clamp(B.rgb, 0.0, 1.0);
    B.a = clamp(dm / 64.0, 0.0, 1.0);
    return B;
}

// ---- pass 2: sugar field -----------------------------------------------------
vec4 passField() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec4 prev = texture2D(fieldBuf, uv);
    vec4 C = prev * 0.95;

    float ar = audioReact;
    float T = TIME * speed;
    float dm = nearestDist(gl_FragCoord.xy, RENDERSIZE.xy);

    // particles deposit sugar + a ring structure around themselves
    C += exp(-dm * dm);
    C += exp(-0.02 * (dm - 6.0) * (dm - 6.0));
    C = clamp(C, 0.0, 1.0);

    // sweeping circular energy source (bass swells it)
    float a = T * 0.2;
    float r = RENDERSIZE.y / 2.0;
    vec2 p = RENDERSIZE.xy / 2.0 + r * vec2(cos(a), sin(a));
    float d = distance(gl_FragCoord.xy, p * 5.0);
    float circleRadius = (50.0 + 75.0 * sin(T * 2.0)) * mix(1.0, 0.4 + 1.8 * aBass(), ar);
    float circle = smoothstep(circleRadius, circleRadius - 10.0, d);
    C += vec4(circle) * 0.65;

    // two roaming energy sources (mids feed them)
    vec2 p1 = RENDERSIZE.xy * vec2(0.3 + 0.3 * cos(T), 0.5 + 0.2 * sin(T));
    vec2 p2 = RENDERSIZE.xy * vec2(0.7 + 0.2 * cos(T * 1.3), 0.5 + 0.2 * sin(T * 0.8));
    float c1 = smoothstep(60.0, 5.0,  distance(gl_FragCoord.xy, p1));
    float c2 = smoothstep(50.0, 35.0, distance(gl_FragCoord.xy, p2));
    C += vec4(c1 + c2) * 0.65 * mix(1.0, 0.3 + 1.9 * aMid(), ar);

    C = clamp(C, 0.0, 1.0);
    return mix(C, prev, 0.25);
}

// ---- pass 3: composite -------------------------------------------------------
vec4 passImage() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    float ar = audioReact;
    float T = TIME * speed;

    vec4 B = texture2D(trailBuf, uv);
    vec4 C = texture2D(fieldBuf, uv);
    float dm = B.a * 64.0;

    float core = smoothstep(4.0, 0.0, dm);
    float trail = length(B.rgb);
    float sugar = length(C.rgb);

    // animated palette, highs shimmer the phase
    vec3 palette = 0.5 + 0.5 * cos(
        vec3(0.0, 2.0, 4.0) + T * 9.0 + sugar * 7.2 + trail * 1.0
        + ar * 2.0 * aHigh());

    // slight screen warp from the sugar field
    vec2 warp = (C.xy - 0.5) * 0.15;
    vec3 warpedTrail = texture2D(trailBuf, uv + warp).rgb;

    vec3 bg = 0.08 + 0.05 * cos(vec3(0.0, 1.5, 3.0) + TIME + uv.xyx * 8.0);

    vec3 col = bg;
    col += 0.15 * sugar * palette;      // sugar field glows through
    col += warpedTrail * palette * 2.4;
    col += vec3(0.0, 1.95, 0.9) * core * 6.8;

    // roaming spotlight (was mouse-driven), highs make it breathe
    vec2 m = 0.5 + 0.35 * vec2(cos(TIME * 0.4), sin(TIME * 0.53));
    float md = distance(uv, m);
    col += vec3(0.2, 0.4, 1.0) * exp(-20.0 * md) * 0.4
         * mix(1.0, 0.3 + 2.0 * aHigh(), ar);

    // sustained loudness lift (mid-range colors, survives the diff metric)
    col *= 1.0 + ar * (0.5 * knee(audioLevel, 0.05, 0.9) + 0.25 * aBass());
    col *= tintColor.rgb * brightness;
    return vec4(col, 1.0);
}

void main() {
    if      (PASSINDEX == 0) gl_FragColor = passAgents();
    else if (PASSINDEX == 1) gl_FragColor = passTrails();
    else if (PASSINDEX == 2) gl_FragColor = passField();
    else                     gl_FragColor = passImage();
}
