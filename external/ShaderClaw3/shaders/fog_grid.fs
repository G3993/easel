/*{
  "DESCRIPTION": "Fog Grid — a dark technical plate: hairline blueprint grid panel floating on near-black, draped in a thick frosted gel fog lit from within by neon gradients (hot pink through molten orange to acid yellow-green on a diagonal). The gel is a domain-warped fbm volume with milky subsurface shading, white frost on its thinnest edges, and billowing lobes that spill past the panel border. A persistent buffer advects the fog so it flows slowly like syrup. Bass surges the inner neon light and thickens the gel, mids churn the billow, highs frost sparse sparkles along the thin rims.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "colorA",
      "LABEL": "Neon Core A",
      "TYPE": "color",
      "DEFAULT": [1.0, 0.10, 0.42, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "colorB",
      "LABEL": "Neon Core B",
      "TYPE": "color",
      "DEFAULT": [0.68, 0.96, 0.05, 1.0],
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
      "NAME": "fogAmount",
      "LABEL": "Gel Thickness",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 1,
      "DEFAULT": 0.66,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "billow",
      "LABEL": "Edge Billow",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "gridDetail",
      "LABEL": "Grid Fineness",
      "TYPE": "float",
      "MIN": 18,
      "MAX": 84,
      "DEFAULT": 42,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "flowSpeed",
      "LABEL": "Fog Flow Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 10,
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
  ],
  "PASSES": [
    {
      "TARGET": "fogBuf",
      "PERSISTENT": true
    },
    {
    }
  ]
}*/

float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1, 0)), u.x),
               mix(hash21(i + vec2(0, 1)), hash21(i + vec2(1, 1)), u.x), u.y);
}

float fbm(vec2 p) {
    float v = 0.0, a = 0.55;
    for (int k = 0; k < 5; k++) {
        v += a * vnoise(p);
        p = p * 2.07 + 13.7;
        a *= 0.53;
    }
    return v;
}

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

// hue rotation for paletteShift re-skinning
vec3 hueRot(vec3 c, float a) {
    const vec3 W = vec3(0.299, 0.587, 0.114);
    float ca = cos(a), sa = sin(a);
    vec3 g = vec3(dot(c, W));
    vec3 d = c - g;
    vec3 cr = cross(vec3(0.57735), c);
    return max(g + d * ca + cr * sa, 0.0);
}

// audio conditioning
float gA, gBassP, gMidP, gHighP, gClk;

// panel margins (uv space)
const float MX = 0.135;
const float MY = 0.095;

// ── Pass 0: gel fog volume, lit from inside, advected in a persistent buffer ──
vec4 passFog() {
    vec2 uv = isf_FragNormCoord.xy;
    float asp = RENDERSIZE.x / RENDERSIZE.y;
    vec2 p = vec2((uv.x - 0.5) * asp, uv.y - 0.5);
    float t = gClk * 0.11;

    // domain-warped fbm gel body — anisotropic so it drapes vertically
    vec2 q = p * vec2(1.9, 1.30);
    // extra flowing warp layer: a large slow curl that carries the whole gel
    // in winding ribbons, so high flow speeds read as flowing, not sliding
    vec2 curl = vec2(fbm(q * 0.62 + vec2(t * 0.50, 3.1)) - 0.5,
                     fbm(q * 0.62 + vec2(9.4, t * 0.44)) - 0.5);
    q += curl * (0.62 + 0.22 * sin(t * 0.17));
    float w1 = fbm(q * 1.5 + vec2(0.0, t * 0.42));
    float w2 = fbm(q * 1.5 + vec2(5.2, -t * 0.33));
    float churn = 1.05 + 0.85 * gA * gMidP;                    // mids churn the billow
    vec2 wp = q + churn * vec2(w1 - 0.5, w2 - 0.5) * 1.5;
    float body = fbm(wp * 1.1 + vec2(0.0, t * 0.26));

    // vertical drape folds like poured gel
    float fold = fbm(vec2(p.x * 6.0 + (w1 - 0.5) * 2.4, p.y * 1.7 - t * 0.18));
    float d = body * 0.68 + fold * 0.46;

    // billowing silhouette: rounded panel footprint, edge eaten by noise lobes
    vec2 e = abs(uv - 0.5) - vec2(0.5 - MX, 0.5 - MY) * 0.98;
    float rd = max(e.x, e.y);
    float lobes = (fbm(vec2(uv.x * 4.2, uv.y * 3.1) + vec2(t * 0.2, 7.7)) - 0.5);
    float mask = smoothstep(0.055 + 0.075 * billow, -0.10,
                            rd - lobes * (0.05 + 0.13 * billow));

    // gel always drapes the panel: thin spots go translucent grey, never punch out
    float density = clamp(0.20 + (d - 0.34) * 1.6, 0.0, 1.0) * mask;
    density *= (0.45 + 0.85 * fogAmount) * (1.0 + 0.25 * gA * gBassP); // bass thickens
    density = clamp(density, 0.0, 1.0);

    // ── lit-from-inside subsurface color ──
    float ps = paletteShift * 0.628;
    vec3 cA = hueRot(colorA.rgb, ps);
    vec3 cB = hueRot(colorB.rgb, ps);
    vec3 mid = mix(cA, cB, 0.5);
    mid = mid / max(max(mid.r, max(mid.g, mid.b)), 0.001);     // saturated hot midpoint

    // diagonal gradient coordinate, wobbled by the warp field
    float g = clamp(0.42 + 0.85 * (p.x * 0.72 - p.y * 0.42) + (w1 - 0.5) * 0.85, 0.0, 1.0);
    vec3 neon = mix(cA, mid, smoothstep(0.04, 0.52, g));
    neon = mix(neon, cB, smoothstep(0.52, 0.96, g));

    // the light lives in the central band; flanks stay milky
    float litMask = smoothstep(0.52, 0.10, abs(uv.x - 0.47) + 0.20 * abs(uv.y - 0.52));
    litMask *= 0.40 + 0.80 * body;
    float lit = pow(clamp(density * 1.25, 0.0, 1.0), 1.15) * litMask;

    // milky gel, grey-blue shadows pooling between the drape folds
    float foldSh = 0.40 + 0.60 * smoothstep(0.18, 0.85, fold);
    float shade = 0.45 + 0.55 * smoothstep(0.05, 0.95, d);
    vec3 milk = vec3(0.60, 0.66, 0.74) * shade * foldSh;

    // inner neon light: saturated color, brightness rides density + bass surge
    float surge = 0.92 + 0.55 * gA * gBassP + 0.06 * sin(gClk * 0.23);
    vec3 neonLit = neon * surge * (0.62 + 0.48 * lit) * (0.62 + 0.38 * foldSh);
    // keep the core saturated, never clipped to white
    float mc = max(neonLit.r, max(neonLit.g, neonLit.b));
    neonLit *= 1.0 / max(1.0, mc / 1.08);
    vec3 fogCol = mix(milk, neonLit, clamp(lit * 2.4, 0.0, 0.95));
    // white frost only where the gel thins to a rim
    float frost = smoothstep(0.01, 0.06, density) * smoothstep(0.22, 0.06, density);
    fogCol += vec3(0.88, 0.93, 1.0) * frost * (0.16 + 0.20 * fold);

    // ── gentle feedback advection: the gel oozes downward and swirls ──
    vec2 flow = vec2(vnoise(p * 2.6 + vec2(t * 0.5, 0.0)) - 0.5,
                     vnoise(p * 2.6 + vec2(7.7, -t * 0.4)) - 0.5) * 0.0026;
    flow *= (1.0 + 0.9 * gA * gMidP);
    // faster flow settings advect the buffer harder so the memory flows too
    flow *= (0.7 + 0.3 * clamp(flowSpeed, 0.0, 4.0));
    flow.y += 0.00045;                                          // sampling above = flowing down
    vec4 prev = texture2D(fogBuf, clamp(uv + flow, 0.001, 0.999));
    vec4 fresh = vec4(fogCol, density);
    // color memory stays light so advection can't average the hues to white;
    // density carries the longer memory (the gel visibly flows)
    float keepC = (FRAMEINDEX < 2) ? 0.0 : 0.45;
    float keepA = (FRAMEINDEX < 2) ? 0.0 : 0.78;
    return vec4(mix(fresh.rgb, prev.rgb, keepC), mix(fresh.a, prev.a, keepA));
}

// ── Pass 1: blueprint panel + gel composite ──
vec4 passPresent() {
    vec2 uv = isf_FragNormCoord.xy;
    vec3 col = vec3(0.008, 0.009, 0.013);

    // panel footprint
    float inx = smoothstep(MX - 0.002, MX + 0.002, uv.x)
              * smoothstep(1.0 - MX + 0.002, 1.0 - MX - 0.002, uv.x);
    float iny = smoothstep(MY - 0.002, MY + 0.002, uv.y)
              * smoothstep(1.0 - MY + 0.002, 1.0 - MY - 0.002, uv.y);
    float inside = inx * iny;
    col += vec3(0.010, 0.013, 0.020) * inside;                 // faint smoked-glass fill

    // hairline blueprint grid (pixel-accurate AA)
    vec3 gridInk = vec3(0.34, 0.47, 0.68);
    float cells = floor(gridDetail);
    vec2 gc = vec2((uv.x - MX) / (1.0 - 2.0 * MX), (uv.y - MY) / (1.0 - 2.0 * MY)) * cells;
    vec2 gf = abs(fract(gc) - 0.5);
    vec2 fw = fwidth(gc);
    float lnx = smoothstep(0.0, 1.0, (0.5 - gf.x) / max(fw.x, 1e-5));
    float lny = smoothstep(0.0, 1.0, (0.5 - gf.y) / max(fw.y, 1e-5));
    float minor = max(1.0 - lnx, 1.0 - lny);
    vec2 gM = abs(fract(gc / 6.0) - 0.5);
    vec2 fwM = fw / 6.0;
    float major = max(smoothstep(1.4, 0.4, (0.5 - gM.x) / max(fwM.x, 1e-5)),
                      smoothstep(1.4, 0.4, (0.5 - gM.y) / max(fwM.y, 1e-5)));
    float grid = minor * 0.16 + major * 0.26;

    // panel border
    vec2 b = min(vec2(uv.x - MX, uv.y - MY), vec2(1.0 - MX - uv.x, 1.0 - MY - uv.y));
    float bd = min(b.x, b.y) * RENDERSIZE.y;
    float border = smoothstep(2.2, 0.6, abs(bd)) * 0.55;

    // sparse technical sub-divisions in the left column (fixed, drawing-like)
    float sub = 0.0;
    for (int i = 0; i < 4; i++) {
        float fi = float(i);
        float yy = MY + (0.14 + 0.20 * fi + 0.05 * hash21(vec2(fi, 3.7))) * (1.0 - 2.0 * MY);
        float xx = MX + (0.06 + 0.11 * hash21(vec2(fi, 9.1))) * (1.0 - 2.0 * MX);
        float hpx = abs(uv.y - yy) * RENDERSIZE.y;
        float seg = smoothstep(1.4, 0.4, hpx) * step(MX, uv.x) * step(uv.x, xx);
        float vpx = abs(uv.x - xx) * RENDERSIZE.y;
        float seg2 = smoothstep(1.4, 0.4, vpx) * step(yy - 0.09, uv.y) * step(uv.y, yy + 0.09);
        sub += (seg + seg2) * 0.17;
    }

    col += gridInk * (grid * inside + border * (inx + iny) * 0.5 + sub * iny);

    // ── the gel ──
    vec4 fog = texture2D(fogBuf, uv);
    float alpha = smoothstep(0.0, 0.50, fog.a) * 0.96;
    col = mix(col, fog.rgb, alpha);
    // grid ghosting faintly through the thin gel
    col += gridInk * grid * inside * alpha * smoothstep(0.6, 0.15, fog.a) * 0.25;

    // highs frost sparse sparkles along the thin rims
    float rimZone = smoothstep(0.02, 0.10, fog.a) * smoothstep(0.42, 0.14, fog.a);
    float tw = hash21(floor(uv * RENDERSIZE.xy / 2.0) + floor(TIME * 7.0));
    float spark = step(0.9982, tw) * rimZone * gA * gHighP;
    col += vec3(0.92, 0.96, 1.0) * spark * 1.4;

    // fine grain
    float grain = hash21(uv * RENDERSIZE.xy + fract(TIME) * 61.7);
    col += (grain - 0.5) * 0.016;

    // breathing lift that can dip below 1
    float lift = mix(1.0, 0.76 + 0.42 * knee(audioLevel, 0.03, 0.8), gA * 0.7);
    col *= brightness * lift;
    return vec4(max(col, 0.0), 1.0);
}

void main() {
    gA     = clamp(audioReact, 0.0, 1.0);
    gBassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    gMidP  = pow(knee(audioMid,  0.08, 0.85), 1.3);
    gHighP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    gClk   = TIME * flowSpeed + audioTime * 0.35 * gA * flowSpeed;

    if (PASSINDEX == 0) gl_FragColor = passFog();
    else                gl_FragColor = passPresent();
}
