/*{
  "DESCRIPTION": "Mirror Shards — four thin mirror-finish planes intersect and tumble slowly in a clean off-white void. Each face carries flowing acid-marble paint: domain-warped ink in saturated magenta, cyan, yellow, orange and violet with white veins, so the marbling itself is the color while the void stays empty. Sharp anti-aliased edges catch a soft studio light. Mids swirl the marble flow, bass gently breathes the whole cluster, and each beat eases a nudge into the tumble axis — no snapping, everything glides.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Geometry",
    "3D",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "inkA",
      "LABEL": "Ink A",
      "TYPE": "color",
      "DEFAULT": [0.93, 0.08, 0.52, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "inkB",
      "LABEL": "Ink B",
      "TYPE": "color",
      "DEFAULT": [0.0, 0.60, 0.95, 1.0],
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
      "NAME": "shardCount",
      "LABEL": "Shards",
      "TYPE": "float",
      "MIN": 3,
      "MAX": 5,
      "DEFAULT": 4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "marbleScale",
      "LABEL": "Marble Scale",
      "TYPE": "float",
      "MIN": 0.6,
      "MAX": 2.5,
      "DEFAULT": 1.2,
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
      "NAME": "flowSpeed",
      "LABEL": "Marble Flow",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
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

vec3 hueRot(vec3 c, float a) {
    vec3 k = vec3(0.57735);
    float s = sin(a), co = cos(a);
    return c * co + cross(k, c) * s + k * dot(k, c) * (1.0 - co);
}

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.03 + vec2(11.7, 5.3);
        a *= 0.5;
    }
    return v;
}

// audio / motion globals
float gA, gBassP, gMidP, gHighP, gT, gFlow, gScale, gWarp;
vec3  gInkA, gInkB;

vec3 rotY(vec3 p, float a) { float s = sin(a), c = cos(a); return vec3(c * p.x + s * p.z, p.y, -s * p.x + c * p.z); }
vec3 rotX(vec3 p, float a) { float s = sin(a), c = cos(a); return vec3(p.x, c * p.y - s * p.z, s * p.y + c * p.z); }
vec3 rotZ(vec3 p, float a) { float s = sin(a), c = cos(a); return vec3(c * p.x - s * p.y, s * p.x + c * p.y, p.z); }

float sdRoundBox(vec3 p, vec3 b, float r) {
    vec3 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, max(q.y, q.z)), 0.0) - r;
}

// shard i local coords (formula-of-i, no arrays)
vec3 shardLocal(vec3 p, float i) {
    vec3 o = vec3(sin(i * 2.4 + 1.0), cos(i * 1.8) * 0.7, sin(i * 3.7)) * 0.24;
    float a1 = i * 1.91 + 0.55 + gT * 0.045 * (1.0 + 0.35 * sin(i * 3.0));
    float a2 = i * 1.37 - 0.80 + gT * 0.030 * (1.0 - 0.3 * cos(i * 2.2));
    vec3 q = p - o;
    q = rotY(q, a1);
    q = rotX(q, a2);
    return q;
}

vec3 shardSize(float i) {
    return vec3(0.92 - 0.10 * sin(i * 2.9), 0.60 + 0.11 * cos(i * 2.2), 0.011);
}

float mapRaw(vec3 p) {
    float n = clamp(floor(shardCount + 0.5), 3.0, 5.0);
    float d = 1e9;
    for (int j = 0; j < 5; j++) {
        float i = float(j);
        if (i < n) d = min(d, sdRoundBox(shardLocal(p, i), shardSize(i), 0.009));
    }
    return d;
}

float map(vec3 p) { return mapRaw(p / gScale) * gScale; }

float shardID(vec3 p) {
    float n = clamp(floor(shardCount + 0.5), 3.0, 5.0);
    float d = 1e9, id = 0.0;
    for (int j = 0; j < 5; j++) {
        float i = float(j);
        if (i < n) {
            float di = sdRoundBox(shardLocal(p, i), shardSize(i), 0.009);
            if (di < d) { d = di; id = i; }
        }
    }
    return id;
}

vec3 calcNormal(vec3 p) {
    vec2 e = vec2(0.0012, -0.0012);
    return normalize(e.xyy * map(p + e.xyy) + e.yyx * map(p + e.yyx)
                   + e.yxy * map(p + e.yxy) + e.xxx * map(p + e.xxx));
}

// six saturated inks (index by if-chain — GLSL ES 1.0 safe)
vec3 ink(float k) {
    float m = mod(k, 6.0);
    if (m < 0.5) return gInkA;                       // magenta
    if (m < 1.5) return vec3(0.98, 0.45, 0.05);      // orange
    if (m < 2.5) return vec3(1.00, 0.86, 0.05);      // yellow
    if (m < 3.5) return vec3(0.12, 0.75, 0.35);      // green
    if (m < 4.5) return gInkB;                       // cyan
    return vec3(0.42, 0.16, 0.85);                   // violet
}

// flowing acid-marble paint (only evaluated on hits)
vec3 marble(vec2 uv, float seed) {
    vec2 w = uv * (1.02 * marbleScale) + seed * 3.71;
    float t = gFlow;
    vec2 q1 = vec2(fbm(w + vec2(t * 0.30, 0.0)),
                   fbm(w + vec2(5.2, 1.3) - vec2(0.0, t * 0.24)));
    vec2 q2 = vec2(fbm(w + gWarp * q1 + vec2(1.7, 9.2) + t * 0.08),
                   fbm(w + gWarp * q1 + vec2(8.3, 2.8)));
    float f = fbm(w + 3.2 * q2);

    float band = fract(f * 1.5 + q1.x * 0.8 + paletteShift * 0.1 + seed * 0.17);
    float seg = floor(band * 6.0);
    float fr  = smoothstep(0.30, 0.70, fract(band * 6.0));
    vec3 col = mix(ink(seg), ink(seg + 1.0), fr);

    // saturate & deepen the pools
    col = mix(col, col * col * 1.75, 0.60);
    col *= 0.70 + 0.55 * f;

    // white veins where the warp folds
    float vein = smoothstep(0.55, 0.72, q2.y) * smoothstep(0.92, 0.62, q2.y + q1.x * 0.3);
    col = mix(col, vec3(0.97, 0.96, 0.94), vein * 0.85);
    // near-black ink threads + navy pools where the field collapses
    float dark = smoothstep(0.40, 0.28, q2.x) * smoothstep(0.16, 0.30, f);
    col = mix(col, vec3(0.04, 0.03, 0.08), dark * 0.88);
    float navy = smoothstep(0.30, 0.16, f) * smoothstep(0.30, 0.48, q1.y);
    col = mix(col, vec3(0.05, 0.09, 0.26), navy * 0.8);
    // acid punch: saturation boost + slight gamma deepening
    col = clamp(mix(vec3(dot(col, vec3(0.3333))), col, 1.45), 0.0, 1.0);
    col = pow(col, vec3(1.12));
    return col;
}

// soft white studio environment for the mirror finish
vec3 env(vec3 d) {
    vec3 e = mix(vec3(0.72, 0.72, 0.74), vec3(1.06, 1.05, 1.03), smoothstep(-0.5, 0.75, d.y));
    e += vec3(0.35) * pow(max(1.0 - abs(d.x + 0.3), 0.0), 3.0) * smoothstep(0.0, 0.6, d.y); // window streak
    return e;
}

vec3 shade(vec3 hp, vec3 rd) {
    vec3 n = calcNormal(hp);
    vec3 pl = hp / gScale;
    float id = shardID(pl);
    vec3 q = shardLocal(pl, id);

    // local normal to find face vs thin edge
    vec3 nl = normalize(vec3(
        mapRaw(pl + vec3(0.0015, 0.0, 0.0)) - mapRaw(pl - vec3(0.0015, 0.0, 0.0)),
        mapRaw(pl + vec3(0.0, 0.0015, 0.0)) - mapRaw(pl - vec3(0.0, 0.0015, 0.0)),
        mapRaw(pl + vec3(0.0, 0.0, 0.0015)) - mapRaw(pl - vec3(0.0, 0.0, 0.0015))));

    vec3 paint = marble(q.xy, id + 1.0);

    vec3 refl = reflect(rd, n);
    float fre = pow(1.0 - max(dot(n, -rd), 0.0), 5.0);
    vec3 e = env(refl);

    // mirror-lacquer over the paint
    vec3 col = paint * (0.82 + 0.18 * abs(nl.z));
    col = mix(col, e * mix(vec3(1.0), paint, 0.35), 0.05 + 0.42 * fre);

    // sharp studio specular
    vec3 L = normalize(vec3(0.4, 0.85, -0.45));
    col += vec3(1.0) * pow(max(dot(refl, L), 0.0), 90.0) * (0.55 + 0.45 * gHighP * gA);

    // thin edges catch the light — bright metallic rim
    float faceAlign = abs(normalize(nl).z);
    float edgeHi = 1.0 - smoothstep(0.35, 0.92, faceAlign);
    col = mix(col, vec3(1.02, 1.01, 0.99), edgeHi * 0.55);
    return col;
}

void main() {
    gA     = clamp(audioReact, 0.0, 1.0);
    gBassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    gMidP  = pow(knee(audioMid,  0.08, 0.85), 1.3);
    gHighP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float levelP = knee(audioLevel, 0.04, 0.85);

    gT     = TIME * 0.55 * tumbleSpeed + audioTime * 0.60 * gA * tumbleSpeed;
    gFlow  = TIME * 0.06 * flowSpeed + audioTime * 0.50 * gA * flowSpeed;
    gWarp  = 2.7 * (1.0 + 0.85 * gA * gMidP);     // mids swirl the marble
    gScale = 1.0 + 0.15 * gA * gBassP;            // bass breathes the cluster

    float hs = paletteShift * 0.6283;
    gInkA = clamp(hueRot(inkA.rgb, hs), 0.0, 1.0);
    gInkB = clamp(hueRot(inkB.rgb, hs), 0.0, 1.0);

    vec2 uv = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y;

    // cluster tumble + eased beat nudge on the axis (no snap)
    float pulse = clamp(audioBeatPulse, 0.0, 1.0);
    float pe = pulse * pulse * (3.0 - 2.0 * pulse);
    float nudge = 0.30 * pe * gA;

    float ty = gT * 0.10 + nudge * 0.6;
    float tx = 0.30 * sin(gT * 0.073) - 0.15 + nudge;
    float tz = 0.10 * sin(gT * 0.051) + nudge * 0.4;

    vec3 ro = vec3(0.0, 0.0, -4.7);
    vec3 rd = normalize(vec3(uv, 2.05));
    ro = rotZ(rotY(rotX(ro, tx), ty), tz);
    rd = rotZ(rotY(rotX(rd, tx), ty), tz);

    // clean off-white void with the gentlest vignette
    vec3 bg = vec3(0.962, 0.956, 0.944) * (1.0 - 0.07 * dot(uv, uv));

    // raymarch with closest-approach silhouette AA
    float t = 1.0, minRel = 1e9, tMin = 1.0;
    bool hit = false;
    for (int i = 0; i < 64; i++) {
        vec3 p = ro + rd * t;
        float d = map(p);
        float rel = d / t;
        if (rel < minRel) { minRel = rel; tMin = t; }
        if (d < 0.0011) { hit = true; break; }
        t += d * 0.95;
        if (t > 8.5) break;
    }

    vec3 col = bg;
    float pix = 1.5 / RENDERSIZE.y;
    if (hit) {
        col = shade(ro + rd * t, rd);
    } else if (minRel < pix * 2.0) {
        float cov = smoothstep(pix * 1.5, 0.0, minRel);
        col = mix(bg, shade(ro + rd * tMin, rd), cov);
    }

    // fine grain + brightness lift that can dip below one
    col += (hash21(gl_FragCoord.xy + fract(TIME) * 5.71) - 0.5) * 0.014;
    float lift = mix(1.0, 0.74 + 0.46 * levelP, gA * 0.8);
    col *= brightness * lift;

    gl_FragColor = vec4(clamp(col, 0.0, 1.6), 1.0);
}
