/*{
  "DESCRIPTION": "Plasma Arc — a tesla-coil discharge: writhing electric filaments crawl between glowing electrode poles, bright plasma nodes racing along the arcs inside a corona haze and animated equipotential field rings. Bass fattens the arc cores, mids drive the writhing, highs send sparkle racing down the filaments.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "arcCount",
      "LABEL": "Filaments",
      "TYPE": "float",
      "DEFAULT": 4.0,
      "MIN": 1.0,
      "MAX": 7.0
    },
    {
      "NAME": "poleCount",
      "LABEL": "Poles",
      "TYPE": "float",
      "DEFAULT": 2.0,
      "MIN": 2.0,
      "MAX": 5.0
    },
    {
      "NAME": "writhe",
      "LABEL": "Writhe",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 2.2
    },
    {
      "NAME": "coronaGlow",
      "LABEL": "Corona",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "arcSpeed",
      "LABEL": "Arc Speed",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 3.0
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "GROUP": "Audio Reactivity",
      "DEFAULT": 0.6,
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
      "TARGET": "arcGlow",
      "PERSISTENT": true
    },
    {}
  ]
}*/

#define TAU 6.283185307179586

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float hash11(float x) { return fract(sin(x * 127.1 + 311.7) * 43758.5453); }
float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 443.897);
    p3 += dot(p3, p3.yzx + 19.19);
    return fract((p3.x + p3.y) * p3.z);
}
float vnoise2(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1, 0)), u.x),
               mix(hash21(i + vec2(0, 1)), hash21(i + vec2(1, 1)), u.x), u.y) * 2.0 - 1.0;
}

void polePos(int j, float n, float T, out vec2 P) {
    float ang = TAU * float(j) / n + T * 0.045; // slow electrode drift
    P = vec2(cos(ang), sin(ang)) * vec2(0.60, 0.44);
}

// plasma scene energy (scalar field) — arcs + poles + corona
float sceneEnergy(vec2 p) {
    float ar = audioReact;
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP   = pow(knee(audioMid,  0.08, 0.90), 1.3);
    float highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float levelP = knee(audioLevel, 0.05, 0.90);

    float n = clamp(floor(poleCount + 0.5), 2.0, 5.0);
    int ni = int(n);
    float Tm = TIME * arcSpeed;

    // writhing amplitude: mids stir it, idle floor keeps it alive in silence
    float wob = writhe * 0.14 * mix(0.75, 0.55 + 1.15 * midP, ar);
    // filament core: bass fattens and brightens the channel
    float coreK = 0.0021 * mix(1.0, 0.72 + 0.85 * bassP, ar);
    float sparkleAmt = mix(0.18, 0.15 + 1.6 * highP, ar);

    float I = 0.0;
    int nPairs = (ni == 2) ? 1 : ni;

    for (int j = 0; j < 5; j++) {
        if (j >= nPairs) break;
        vec2 A, B;
        polePos(j, n, TIME, A);
        int j2 = (j + 1 >= ni) ? 0 : j + 1;
        polePos(j2, n, TIME, B);

        vec2 BA = B - A;
        float L = max(length(BA), 1e-4);
        vec2 dir = BA / L;
        vec2 nrm = vec2(-dir.y, dir.x);
        float t = clamp(dot(p - A, dir) / L, 0.0, 1.0);
        float perp = dot(p - (A + dir * t * L), nrm);
        float envl = pow(4.0 * t * (1.0 - t), 0.65); // pinned at the electrodes

        for (int f = 0; f < 7; f++) {
            if (float(f) >= floor(arcCount + 0.5)) break;
            float s = hash21(vec2(float(j) * 13.7 + 3.1, float(f) * 7.19));
            // two-octave writhing displacement, each filament on its own clock
            float disp = (vnoise2(vec2(t * 4.0 + s * 61.0, Tm * (0.55 + s * 0.5) + s * 43.0)) * 0.7
                        + vnoise2(vec2(t * 11.0 + s * 23.0, Tm * (1.15 + s * 0.7) + s * 9.0)) * 0.3)
                       * envl * wob * L;
            float d = abs(perp - disp);
            float fil = coreK / (d + 0.0055);
            // slow independent flicker per filament (never a strobe)
            fil *= 0.62 + 0.38 * sin(Tm * (2.1 + s * 2.9) + s * TAU);
            // plasma node racing along the arc; highs send more of them
            float tn = fract(Tm * (0.19 + 0.27 * s) + s);
            fil *= 1.0 + 2.1 * sparkleAmt * exp(-pow((t - tn) * 13.0, 2.0));
            I += min(fil, 2.6);
        }
    }

    // electrodes: hot cores + noisy corona flare
    float breathe = mix(1.0, 0.78 + 0.65 * bassP, ar);
    for (int j = 0; j < 5; j++) {
        if (j >= ni) break;
        vec2 P;
        polePos(j, n, TIME, P);
        vec2 rp = p - P;
        float r = length(rp);
        I += 0.0032 / (r * r + 0.004) * breathe;
        float flare = 0.68 + 0.32 * vnoise2(rp * 4.5 / max(r, 1e-3) + TIME * 0.9);
        I += coronaGlow * 0.045 / (r + 0.10) * flare;
    }

    // overall drive breathes with loudness but never dies
    I *= mix(0.85, 0.55 + 0.85 * levelP, ar);
    return I;
}

vec4 passGlow() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 p = (uv - 0.5) * vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0);

    float cur = min(sceneEnergy(p), 3.0) * 0.32; // headroom for 8-bit buffer
    float keep = exp(-clamp(TIMEDELTA, 0.001, 0.1) * 5.0); // short phosphor tail
    float prev = texture2D(arcGlow, uv).r;
    float stored = max(prev * keep, cur);
    if (FRAMEINDEX < 2) stored = cur;
    return vec4(vec3(stored), 1.0);
}

vec4 passImage() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 p = (uv - 0.5) * vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0);
    float n = clamp(floor(poleCount + 0.5), 2.0, 5.0);
    int ni = int(n);

    // dark chamber + animated equipotential rings around the electrodes
    float pot = 0.0;
    float rmin = 10.0;
    for (int j = 0; j < 5; j++) {
        if (j >= ni) break;
        vec2 P;
        polePos(j, n, TIME, P);
        float r = length(p - P);
        pot += 0.1 / (r + 0.06);
        rmin = min(rmin, r);
    }
    float rings = 0.5 + 0.5 * sin(pot * 26.0 - TIME * 0.9);
    vec3 col = vec3(0.014, 0.017, 0.030);
    col += rings * rings * vec3(0.035, 0.055, 0.105) * (0.35 + 0.65 * exp(-rmin * 1.4));
    // faint chamber haze
    col += vec3(0.020, 0.028, 0.055) * exp(-dot(p, p) * 1.6);

    // plasma: blue body, white-hot core
    float I = texture2D(arcGlow, uv).r * 3.1;
    col += I * vec3(0.30, 0.50, 1.0) * 0.85;
    col += pow(I, 2.4) * vec3(0.95, 0.98, 1.0) * 0.75;

    vec2 vp = uv - 0.5;
    col *= 1.0 - dot(vp, vp) * 0.6;
    col = max(col, vec3(0.006, 0.007, 0.012));

    return vec4(col * tintColor.rgb * brightness, 1.0);
}

void main() {
    if (PASSINDEX == 0) gl_FragColor = passGlow();
    else                gl_FragColor = passImage();
}
