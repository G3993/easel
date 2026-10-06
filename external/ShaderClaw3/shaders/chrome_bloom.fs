/*{
  "DESCRIPTION": "Chrome Bloom — a floating liquid-metal splash sculpture. Raymarched metaballs stretch into thin spiky tendrils with droplet tips, every surface a perfect mirror of a procedural sky: editable zenith blue overhead, pale horizon light, and a warm gold sheen pooling only in the downward reflections. The sculpture tumbles slowly against open sky. Bass wobbles the surface tension of the chrome, each beat births a small droplet that flies out and merges back into the mass, and highs strike sparse specular sparks off the highlights. Beautiful and calm in silence.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Geometry",
    "3D",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "skyZenith",
      "LABEL": "Sky Zenith",
      "TYPE": "color",
      "DEFAULT": [0.13, 0.27, 0.62, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "skyHorizon",
      "LABEL": "Sky Horizon",
      "TYPE": "color",
      "DEFAULT": [0.76, 0.84, 0.94, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "paletteShift",
      "LABEL": "Palette Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 10,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "spread",
      "LABEL": "Splash Spread",
      "TYPE": "float",
      "MIN": 0.6,
      "MAX": 1.6,
      "DEFAULT": 1.0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "spikes",
      "LABEL": "Tendril Spikiness",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.7,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "tumbleSpeed",
      "LABEL": "Tumble Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "wobbleAmt",
      "LABEL": "Surface Wobble",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

// hue rotation about the grey axis
vec3 hueRot(vec3 c, float a) {
    vec3 k = vec3(0.57735);
    float s = sin(a), co = cos(a);
    return c * co + cross(k, c) * s + k * dot(k, c) * (1.0 - co);
}

float smin(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

// audio globals
float gA, gBassP, gMidP, gHighP, gT, gWob, gDropT;
vec3  gDropDir, gZen, gHor;
vec3  SUN;

vec3 rotY(vec3 p, float a) { float s = sin(a), c = cos(a); return vec3(c * p.x + s * p.z, p.y, -s * p.x + c * p.z); }
vec3 rotX(vec3 p, float a) { float s = sin(a), c = cos(a); return vec3(p.x, c * p.y - s * p.z, s * p.y + c * p.z); }

// approximate ellipsoid SDF — good enough for smooth blending
float sdEll(vec3 p, vec3 r) {
    float k = length(p / r);
    return (k - 1.0) * min(min(r.x, r.y), r.z);
}

// splash arms: wide, mostly planar arrangement — a frozen splatter
vec3 ballPos(float i) {
    float a  = i * 2.399 + gT * 0.16 + sin(i * 5.1) * 0.7;
    float el = 0.85 * sin(i * 1.71 + gT * 0.055);
    float r  = spread * (0.92 + 0.38 * sin(i * 2.3 + gT * 0.07)) * (1.0 + 0.09 * gA * gMidP);
    return vec3(cos(a) * cos(el), 0.52 * sin(el), sin(a) * cos(el)) * r;
}

float map(vec3 p) {
    // core: overlapping flattened liquid sheets — a pooled splash, visible from frame zero
    vec3 p1 = rotY(p, 0.7);
    float d = sdEll(p1, vec3(0.62, 0.155, 0.40));
    vec3 p2 = rotY(rotX(p, 0.5), -0.9);
    d = smin(d, sdEll(p2 + vec3(0.15, 0.06, -0.10), vec3(0.46, 0.135, 0.30)), 0.14);
    d = smin(d, length(p - vec3(-0.06, 0.15, 0.08)) - 0.15, 0.15);

    float kBlend = mix(0.18, 0.07, spikes);
    float tipR   = mix(0.035, 0.008, spikes);
    for (int j = 0; j < 7; j++) {
        float i  = float(j);
        vec3  bp = ballPos(i);
        float br = 0.042 + 0.020 * sin(i * 2.1 + gT * 0.17);
        float db = length(p - bp) - br;
        // curved thin tendril arcing from the core out to the droplet tip
        vec3 perp = normalize(cross(bp, vec3(0.0, 1.0, 0.12)));
        float h  = clamp(dot(p, bp) / dot(bp, bp), 0.0, 1.0);
        vec3 axis = bp * h + perp * (0.20 * sin(h * 3.14159) * sin(i * 2.7 + gT * 0.11));
        float dt = length(p - axis) - mix(0.095, tipR, pow(h, 0.55));
        db = min(db, dt);
        // detached-looking satellite droplet just past the tip
        float ds = length(p - bp * 1.26) - 0.030;
        db = min(db, smin(ds, dt, 0.05));
        d = smin(d, db, kBlend);
    }

    // beat droplet: born at full pulse away from the mass, eases back and merges
    vec3  dp = gDropDir * (0.40 + 1.15 * gDropT);
    float dd = length(p - dp) - (0.065 + 0.065 * (1.0 - gDropT));
    d = smin(d, dd, 0.13);

    // bass surface-tension wobble (small, march-safe)
    d += gWob * sin(p.x * 6.0 + gT * 1.6) * sin(p.y * 7.0 - gT * 1.2) * sin(p.z * 5.0 + gT * 0.8);
    return d * 0.75;
}

vec3 calcNormal(vec3 p) {
    vec2 e = vec2(0.0011, -0.0011);
    return normalize(e.xyy * map(p + e.xyy) + e.yyx * map(p + e.yyx)
                   + e.yxy * map(p + e.yxy) + e.xxx * map(p + e.xxx));
}

// procedural sky: zenith / horizon inputs, warm gold pooling only well below
vec3 skyCol(vec3 d) {
    float h = d.y;
    vec3 c = mix(gHor, gZen, pow(clamp(h * 0.95 + 0.20, 0.0, 1.0), 1.3));
    // below the horizon: dark hazy slate with only a warm bronze hint — never mud
    vec3 low = mix(vec3(0.27, 0.29, 0.37), vec3(0.43, 0.35, 0.23), 0.35 + 0.35 * clamp(-h * 2.0 - 0.5, 0.0, 1.0));
    c = mix(low, c, smoothstep(-0.55, -0.10, h));
    // faint cloud shelves so the mirror has something to grab
    float band = sin(h * 16.0 + sin(d.x * 3.1 + d.z * 2.2) * 1.4);
    c += vec3(0.045, 0.048, 0.055) * band * smoothstep(0.42, 0.06, abs(h - 0.10));
    float s = max(dot(d, SUN), 0.0);
    c += vec3(1.0, 0.87, 0.62) * (pow(s, 9.0) * 0.20 + pow(s, 80.0) * 0.9);
    c += vec3(0.09, 0.075, 0.05) * exp(-abs(h + 0.08) * 6.0); // warm horizon haze
    return c;
}

vec3 shade(vec3 hp, vec3 rd) {
    vec3 n = calcNormal(hp);
    vec3 refl = reflect(rd, n);
    float fre = pow(1.0 - max(dot(n, -rd), 0.0), 5.0);
    vec3 env = skyCol(refl);

    // chrome: nearly pure mirror, slight edge lift, punchy contrast
    vec3 col = env * (0.55 + 0.45 * fre) + env * env * 0.30;

    // cheap ambient occlusion in the crevices
    float ao = clamp(map(hp + n * 0.20) / 0.20, 0.0, 1.0);
    col *= 0.55 + 0.45 * ao;

    // sun glint + sparse high-frequency sparks on the highs
    float sg = pow(max(dot(refl, SUN), 0.0), 160.0);
    float cell = hash21(floor(n.xy * 26.0 + 40.0) + floor(TIME * 6.0) * 0.373);
    float spark = step(0.955, cell) * gHighP * gA;
    col += vec3(1.05, 0.95, 0.78) * sg * (1.2 + 5.0 * spark);
    col += vec3(1.0, 0.97, 0.9) * spark * fre * 0.35;
    return col;
}

void main() {
    gA     = clamp(audioReact, 0.0, 1.0);
    gBassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    gMidP  = pow(knee(audioMid,  0.08, 0.85), 1.3);
    gHighP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float levelP = knee(audioLevel, 0.04, 0.85);

    gT   = TIME * tumbleSpeed + audioTime * 0.35 * gA * tumbleSpeed;
    gWob = wobbleAmt * (0.005 + 0.016 * gA * gBassP + 0.009 * gA * gMidP);

    // beat droplet event (eased, decaying — merged & invisible in silence)
    float pulse = clamp(audioBeatPulse, 0.0, 1.0);
    float pe = pulse * pulse * (3.0 - 2.0 * pulse);
    gDropT = pe * gA;
    gDropDir = normalize(vec3(sin(gT * 0.41 + 2.0), 0.45 + 0.42 * sin(gT * 0.29), cos(gT * 0.34)));

    // palette-shifted sky colors
    float hs = paletteShift * 0.6283;
    gZen = clamp(hueRot(skyZenith.rgb, hs), 0.0, 1.0);
    gHor = clamp(hueRot(skyHorizon.rgb, hs), 0.0, 1.0);
    SUN  = normalize(vec3(0.55, 0.42, -0.38));

    vec2 uv = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y;

    // camera with slow tumble — stays near the horizon so the sky reads blue
    float ty = gT * 0.11;
    float tx = 0.16 * sin(gT * 0.09) + 0.04;
    // beatless swells (pads, ambient) ease the camera in — smooth, no snapping
    float eDrive = knee(audioLevel, 0.04, 0.80);
    vec3 ro = vec3(0.0, 0.10, -4.3 + 0.55 * gA * eDrive);
    vec3 rd = normalize(vec3(uv, 1.9));
    ro = rotY(rotX(ro, tx), ty);
    rd = rotY(rotX(rd, tx), ty);
    SUN = rotY(rotX(SUN, tx * 0.2), ty * 0.25); // sun mostly world-fixed

    // raymarch, 64 steps, early out, closest-approach tracked for silhouette AA
    float t = 0.8, minRel = 1e9, tMin = 0.8;
    bool hit = false;
    for (int i = 0; i < 64; i++) {
        vec3 p = ro + rd * t;
        float d = map(p);
        float rel = d / t;
        if (rel < minRel) { minRel = rel; tMin = t; }
        if (d < 0.0011) { hit = true; break; }
        t += d * 0.9;
        if (t > 9.5) break;
    }

    vec3 bg = skyCol(rd);
    vec3 col = bg;
    float pix = 1.6 / RENDERSIZE.y;
    if (hit) {
        col = shade(ro + rd * t, rd);
    } else if (minRel < pix * 2.0) {
        // anti-aliased silhouette: shade at closest approach, blend by coverage
        float cov = smoothstep(pix * 1.6, 0.0, minRel);
        col = mix(bg, shade(ro + rd * tMin, rd), cov);
    }

    // film grain + brightness (audio lift that can dip below 1)
    col += (hash21(gl_FragCoord.xy + fract(TIME) * 7.13) - 0.5) * 0.022;
    float lift = mix(1.0, 0.78 + 0.36 * levelP, gA * 0.7);
    col *= brightness * lift;

    gl_FragColor = vec4(max(col, 0.0), 1.0);
}
