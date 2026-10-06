/*{
  "DESCRIPTION": "Dibujito — a little drawing machine: an orbiting emitter squirts dye into a real Navier-Stokes fluid (RK4 backward advection, divergence solve, iterative pressure, gradient subtraction) rendered as embossed shaded ink. Bass squirts harder, mids kick the swirl, loudness lifts the page.",
  "CREDIT": "NS fluid by Robert Schuetze (trirop) / Ulysse Vimont (2017) CC BY-NC-SA 3.0, ShaderClaw audio port",
  "CATEGORIES": [
    "Generator"
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
      "NAME": "inkColor",
      "LABEL": "Ink Color",
      "TYPE": "color",
      "DEFAULT": [0.85, 0.9, 1.0, 1.0]
    },
    {
      "NAME": "wander",
      "LABEL": "Wander",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.5,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "chaos",
      "LABEL": "Chaos",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.35,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "jitter",
      "LABEL": "Jitter",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.2,
      "MIN": 0.0,
      "MAX": 1.0
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
      "TARGET": "advBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "divBuf"
    },
    {
      "TARGET": "presBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "gradBuf",
      "PERSISTENT": true
    },
    {}
  ]
}*/

#define RK_H 2.0

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

// velocity ±2 packed to 8-bit; concentration direct
vec4 encVel(vec2 v, float d) { return vec4(clamp(v / 4.0 + 0.5, 0.0, 1.0), clamp(d, 0.0, 1.0), 1.0); }
vec2 decVel(vec4 t) { return (t.xy - 0.5) * 4.0; }
float encS(float s) { return clamp(s / 4.0 + 0.5, 0.0, 1.0); }
float decS(float e) { return (e - 0.5) * 4.0; }

vec4 gradAt(vec2 C) { return texture2D(gradBuf, C / RENDERSIZE.xy); }
float presAt(vec2 C) { return decS(texture2D(presBuf, C / RENDERSIZE.xy).x); }
float divAt(vec2 C) { return decS(texture2D(divBuf, C / RENDERSIZE.xy).x); }
vec4 advAt(vec2 C) { return texture2D(advBuf, C / RENDERSIZE.xy); }

vec2 hash2e(float n) {
    return fract(sin(vec2(n * 12.9898, n * 78.233 + 4.7)) * 43758.5453);
}

// the animated emitter (replaces the mouse) — motion-kit style:
// base figure sweep + smooth wander + darting jumps + fine jitter
vec2 emitterPos(float T) {
    vec2 p = vec2(pow(abs(sin(T)), 2.0) - 0.5, 0.2 * sin(T * 4.0) + 0.2);
    // wander: layered incommensurate sines = smooth random walk
    vec2 w = vec2(sin(T * 0.73 + 1.7) + 0.5 * sin(T * 1.31 + 0.4) + 0.25 * sin(T * 2.17),
                  cos(T * 0.61)       + 0.5 * cos(T * 1.13 + 2.1) + 0.25 * cos(T * 1.87 + 0.9));
    p = mix(p, w * 0.22, wander);
    // chaos: eased darts to random spots, holding briefly between jumps
    float seg = floor(T * 0.55);
    float ft = smoothstep(0.25, 0.75, fract(T * 0.55));
    p += mix(hash2e(seg) - 0.5, hash2e(seg + 1.0) - 0.5, ft) * 0.55 * chaos;
    // jitter: fast small trembles
    p += vec2(sin(T * 13.7), cos(T * 17.3)) * 0.012 * jitter;
    return p;
}

// pass 0 — RK4 backward advection of the divergence-free field
vec4 passAdvect() {
    vec2 C = gl_FragCoord.xy;
    vec2 r = RENDERSIZE.xy;
    vec2 uv = (C - r * 0.5) / r.y;
    float T = TIME * speed;
    float ar = audioReact;
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP  = pow(knee(audioMid,  0.08, 0.90), 1.3);

    // iterative backward trace through the last divergence-free field
    vec2 p = C;
    for (int i = 0; i < 10; i++) {
        p = C - RK_H * decVel(gradAt(p));
    }
    vec4 buf = gradAt(p);
    vec2 v = decVel(buf);
    float d = buf.z;

    if (C.x < 1.0 || C.x > r.x - 1.0) { v.x = 0.0; v.y *= 0.5; }
    if (r.y - 1.0 < C.y || C.y < 1.0) { v.y = 0.0; v.x *= 0.5; }

    // emitter squirt: dye with bass, velocity kick with mids
    vec2 m = emitterPos(T);
    float dtq = max(TIMEDELTA, 0.008);
    vec2 mv = (m - emitterPos(T - dtq * speed)) / dtq * 0.016;
    if (length(uv - m) < 0.025) {
        float rr = length(uv - m) / 0.025;
        rr = sqrt(max(0.0, 1.0 - rr * rr));
        d += rr * (0.6 + ar * (1.8 * bassP + 0.8 * knee(audioLevel, 0.05, 0.9)));
        v += mv * 100.0 * rr * (1.0 + ar * 1.2 * midP);
    }
    d = clamp(d, 0.0, 1.0);
    d *= 0.998; // slow dye fade so the page never saturates

    if (FRAMEINDEX < 2) { v = vec2(0.0); d = 0.0; }
    return encVel(v, d);
}

// pass 1 — divergence
vec4 passDiv() {
    vec2 C = gl_FragCoord.xy;
    float vxl = decVel(advAt(C + vec2(1, 0))).x;
    float vxr = decVel(advAt(C - vec2(1, 0))).x;
    float vyt = decVel(advAt(C + vec2(0, 1))).y;
    float vyb = decVel(advAt(C - vec2(0, 1))).y;
    float div = (vxl - vxr + vyt - vyb) / 2.0;
    return vec4(encS(div), 0.0, 0.0, 1.0);
}

// pass 2 — pressure (Jacobi, converging across frames)
vec4 passPressure() {
    vec2 C = gl_FragCoord.xy;
    float pl = presAt(C + vec2(1, 0));
    float pr = presAt(C - vec2(1, 0));
    float pt = presAt(C + vec2(0, 1));
    float pb = presAt(C - vec2(0, 1));
    float p = (pl + pr + pt + pb - divAt(C)) * 0.25;
    if (FRAMEINDEX < 2) p = 0.0;
    return vec4(encS(p), 0.0, 0.0, 1.0);
}

// pass 3 — subtract pressure gradient (+ dye gravity)
vec4 passGrad() {
    vec2 C = gl_FragCoord.xy;
    vec2 r = RENDERSIZE.xy;
    float pl = presAt(C - vec2(-1, 0));
    float pr = presAt(C - vec2(1, 0));
    float pt = presAt(C - vec2(0, -1));
    float pb = presAt(C - vec2(0, 1));
    vec2 grad = vec2(pr - pl, pb - pt) / 2.0;

    vec4 bufOld = advAt(C);
    float d = bufOld.z;
    vec2 v = decVel(bufOld);
    v = v - grad - vec2(0.0, d) * 0.01;

    if (C.x < 1.0 || C.x > r.x - 1.0) v.x = 0.0;
    if (r.y - 1.0 < C.y || C.y < 1.0) v.y = 0.0;
    if (FRAMEINDEX < 2) { v = vec2(0.0); d = 0.0; }
    return encVel(v, d);
}

// final — embossed shaded concentration
vec4 passImage() {
    vec2 C = gl_FragCoord.xy;
    vec2 r = RENDERSIZE.xy;
    float ar = audioReact;
    float levelP = knee(audioLevel, 0.05, 0.90);

    float concentration = advAt(C).z;
    float pl = advAt(C - vec2(-1, 0)).z;
    float pr = advAt(C - vec2(1, 0)).z;
    float pt = advAt(C - vec2(0, -1)).z;
    float pb = advAt(C - vec2(0, 1)).z;
    vec2 grad = vec2(pr - pl, pb - pt);
    float lit = 0.2 + 0.8 * max(dot(normalize(vec3(0.0, 1.0, 1.0)),
                                    normalize(vec3(grad.x, 0.05, grad.y))), 0.0);
    vec3 col = clamp(concentration, 0.0, 1.0) * lit * inkColor.rgb * 1.6;

    // velocity tint underneath
    vec2 v = decVel(advAt(C));
    col += 0.3 * concentration * vec3(0.5 + 0.5 * v.x, 0.4, 0.5 + 0.5 * v.y);

    col *= 1.0 + ar * 0.35 * levelP;
    col *= tintColor.rgb * brightness;
    return vec4(col, 1.0);
}

void main() {
    if      (PASSINDEX == 0) gl_FragColor = passAdvect();
    else if (PASSINDEX == 1) gl_FragColor = passDiv();
    else if (PASSINDEX == 2) gl_FragColor = passPressure();
    else if (PASSINDEX == 3) gl_FragColor = passGrad();
    else                     gl_FragColor = passImage();
}
