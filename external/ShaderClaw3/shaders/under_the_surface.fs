/*{
  "DESCRIPTION": "Under the Surface — first-person from below: the rippling surface opens a shimmering Snell's window with a wandering sun, god rays refract down through the wave field into a deep blue gradient, and parallax layers of drifting particulates catch the shafts. Bass swells the light shafts, mids stir the surface waves, highs ignite sun glints and particle sparkle.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Nature",
    "Audio Reactive"
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
      "NAME": "depth",
      "LABEL": "Depth",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "rayIntensity",
      "LABEL": "Ray Intensity",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 2.0
    },
    {
      "NAME": "waveAmount",
      "LABEL": "Wave Amount",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 2.0
    },
    {
      "NAME": "particleAmount",
      "LABEL": "Particulates",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 2.0
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
  ]
}*/

#define TAU 6.2831853

float gAmp; // live wave amplitude (knob + smoothed mids)

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

vec3 hash32(vec2 src) {
    vec3 p3 = fract(src.xyx * vec3(0.1031, 0.1030, 0.0973));
    p3 += dot(p3, p3.yxz + 33.33);
    return fract((p3.xxy + p3.yzz) * p3.zyx);
}

// four directional wave trains — the one surface everything derives from
float waveH(vec2 p, float t) {
    float h = 0.0;
    h += 0.50 * sin(dot(p, vec2( 0.66,  0.31)) * 1.10 + t * 1.00);
    h += 0.28 * sin(dot(p, vec2(-0.41,  0.74)) * 2.30 + t * 1.37 + 1.7);
    h += 0.15 * sin(dot(p, vec2( 0.92, -0.55)) * 4.10 + t * 1.93 + 4.1);
    h += 0.09 * sin(dot(p, vec2(-0.17, -0.89)) * 7.70 + t * 2.41 + 2.3);
    return h;
}

// extra fine chop, only for the surface close-up normal
float waveHD(vec2 p, float t) {
    float h = waveH(p, t);
    h += 0.050 * sin(dot(p, vec2( 3.1,  2.3)) * 4.0 + t * 3.7);
    h += 0.030 * sin(dot(p, vec2(-2.2,  4.1)) * 3.4 + t * 4.3 + 1.3);
    return h;
}

// negative laplacian of the wave field = where the surface focuses sunlight
float causticFocus(vec2 sp, float t) {
    float e = 0.45;
    float lap = waveH(sp + vec2(e, 0.0), t) + waveH(sp - vec2(e, 0.0), t)
              + waveH(sp + vec2(0.0, e), t) + waveH(sp - vec2(0.0, e), t)
              - 4.0 * waveH(sp, t);
    float c = max(-lap * 1.35, 0.0);
    return c * c;
}

// soft drifting particulate layer
float motes(vec2 uv, float scale, float t, float seed) {
    vec2 gv = uv * scale + vec2(t * 0.05, -t * 0.03);
    vec2 id = floor(gv);
    vec2 f = fract(gv) - 0.5;
    vec3 h = hash32(id + seed);
    vec2 pp = (h.xy - 0.5) * 0.7;
    pp += 0.12 * vec2(sin(t * 0.6 + h.z * TAU), cos(t * 0.45 + h.z * TAU));
    float d = length(f - pp);
    float r = 0.03 + 0.06 * h.z;
    return smoothstep(r, r * 0.25, d) * (0.3 + 0.7 * h.z);
}

void main() {
    vec2 uv = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y;
    float t = TIME * speed * 0.55;
    float ar = audioReact;
    float bassP  = pow(knee(audioBass, 0.05, 0.85), 1.6);
    float midP   = pow(knee(audioMid,  0.08, 0.90), 1.3);
    float highP  = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float levelP = knee(audioLevel, 0.05, 0.90);

    gAmp = 0.45 * waveAmount * (1.0 + ar * 0.35 * midP);

    vec3 ro = vec3(0.0, 0.0, 0.0);
    vec3 rd = normalize(vec3(uv.x, uv.y + 0.40, 0.86));
    // slow head sway — the floating, weightless feel
    float sway = sin(t * 0.22) * 0.05;
    rd.xz = mat2(cos(sway), -sin(sway), sin(sway), cos(sway)) * rd.xz;

    float surfY = mix(1.8, 4.6, depth);
    vec3 sunDir = normalize(vec3(sin(t * 0.07) * 0.30, 1.0, 0.42));

    // deep-water base gradient (never black)
    float dg = clamp(rd.y * 0.5 + 0.5, 0.0, 1.0);
    vec3 deepLo = vec3(0.012, 0.030, 0.052);
    vec3 deepHi = vec3(0.070, 0.255, 0.380);
    vec3 col = mix(deepLo, deepHi, pow(dg, 1.4)) * mix(1.15, 0.60, depth);

    // ---- the surface, seen from underneath (Snell's window) ----
    float ts = 1000.0;
    if (rd.y > 0.035) {
        ts = surfY / rd.y;
        vec3 pos = ro + rd * ts;
        vec2 sxz = pos.xz * 1.5;      // finer apparent chop on the ceiling
        float e = 0.10;
        float h0 = waveHD(sxz, t);
        vec3 n = normalize(vec3(-(waveHD(sxz + vec2(e, 0.0), t) - h0) / e * gAmp,
                                 1.0,
                                -(waveHD(sxz + vec2(0.0, e), t) - h0) / e * gAmp));
        vec3 rr = refract(rd, -n, 1.335);
        vec3 surfCol;
        if (dot(rr, rr) > 1.0e-4) {
            // through the window: sky + wandering sun + glitter
            rr = normalize(rr);
            float sunAmt = max(dot(rr, sunDir), 0.0);
            vec3 sky = mix(vec3(0.10, 0.28, 0.42), vec3(0.55, 0.80, 0.95),
                           pow(max(rr.y, 0.0), 0.55));
            float glint = pow(sunAmt, 180.0) * (2.5 + ar * 3.5 * highP);
            sky += vec3(1.0, 0.93, 0.78)
                 * (pow(sunAmt, 6.0) * 0.7 + pow(sunAmt, 20.0) * 1.3 + glint);
            surfCol = sky;
        } else {
            // total internal reflection: a dark mirror of the deep
            vec3 rf = reflect(rd, -n);
            float rg = clamp(-rf.y * 0.5 + 0.5, 0.0, 1.0);
            surfCol = mix(deepHi * 0.8, deepLo, rg) * 0.9;
        }
        // shimmering focus web crawling on the underside of the surface
        surfCol += vec3(0.55, 0.85, 0.95) * causticFocus(pos.xz, t) * 0.45;
        float atten = exp(-ts * mix(0.05, 0.16, depth));
        col = mix(col, surfCol, atten * smoothstep(0.035, 0.09, rd.y));
    }

    // ---- god rays: march the column, project the focus web down along the sun ----
    float maxT = min(ts, 13.0);
    float dith = hash32(gl_FragCoord.xy).x;
    float shaft = 0.0;
    for (int i = 0; i < 14; i++) {
        float ft = (float(i) + dith) / 14.0;
        float d = ft * ft * maxT;
        vec3 ps = ro + rd * d;
        float rise = surfY - ps.y;
        vec2 sp = ps.xz + sunDir.xz / max(sunDir.y, 0.4) * rise;
        shaft += causticFocus(sp, t) * exp(-rise * mix(0.16, 0.34, depth));
    }
    shaft /= 14.0;
    float align = 0.35 + 0.65 * pow(max(dot(rd, sunDir), 0.0), 2.0);
    float drive = 0.60 + ar * (0.55 * bassP + 0.25 * levelP); // idle floor: rays never die
    shaft *= align * rayIntensity * drive;
    col += vec3(0.30, 0.68, 0.85) * shaft * 3.2;

    // ---- particulates in parallax, lit where the shafts are ----
    float local = clamp(shaft * 3.0, 0.0, 1.0);
    float mAmt = particleAmount * (0.75 + ar * 0.6 * highP);
    float m = 0.0;
    m += motes(uv, 9.0,  t, 3.1);
    m += motes(uv, 16.0, t * 1.2, 7.7) * 0.55;
    m += motes(uv, 26.0, t * 1.45, 13.3) * 0.30;
    col += vec3(0.55, 0.80, 0.90) * m * mAmt * (0.25 + 0.75 * local) * 0.5;

    // gentle exposure breath with the mix level
    col *= 1.0 + 0.12 * ar * levelP;

    col *= 1.0 - 0.35 * dot(uv, uv);          // soft vignette
    col = col / (1.0 + col * 0.55);           // soft shoulder so the sun blooms, not clips
    col *= tintColor.rgb * brightness;
    gl_FragColor = vec4(col, 1.0);
}
