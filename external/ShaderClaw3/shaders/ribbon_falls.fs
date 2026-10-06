/*{
  "DESCRIPTION": "Ribbon Falls — dozens of stacked paper-cut wave layers cascading down the sheet in three tiers, each layer a flat curated ink (cobalt, powder blue, leaf green, vermillion, wine, pinks, cream, gold) with scalloped finger-lobes on its edge and a soft ambient-occlusion shadow under every lip. Pale grey backing paper shows at the fringes. Layers slide slowly sideways; bass deepens the wave relief, mids ripple the lobe edges, and every beat re-inks one random layer with a 100ms eased crossfade. Cool Ink and Warm Ink tint the two dominant ink families.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Geometry",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "layerCount",
      "LABEL": "Paper Layers",
      "TYPE": "float",
      "MIN": 14,
      "MAX": 34,
      "DEFAULT": 29,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "reliefAmp",
      "LABEL": "Wave Relief",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "lobeDetail",
      "LABEL": "Lobe Detail",
      "TYPE": "float",
      "MIN": 6,
      "MAX": 32,
      "DEFAULT": 18,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "slideSpeed",
      "LABEL": "Slide Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorA",
      "LABEL": "Cool Ink",
      "TYPE": "color",
      "DEFAULT": [0.135, 0.31, 0.78, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "colorB",
      "LABEL": "Warm Ink",
      "TYPE": "color",
      "DEFAULT": [0.855, 0.135, 0.135, 1.0],
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
      "TARGET": "stateBuf",
      "PERSISTENT": true
    },
    {
    }
  ]
}*/

float hash11(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

// audio + state globals
float gA, gBassP, gMidP, gClk, gCount, gEnv, gNf;

// Fixed 10-ink paper palette pulled from the reference print.
vec3 pal10(float k) {
    k = mod(k, 10.0);
    if (k < 0.5) return vec3(0.955, 0.925, 0.815);  // cream
    if (k < 1.5) return vec3(0.965, 0.755, 0.100);  // golden yellow
    if (k < 2.5) return vec3(0.935, 0.420, 0.075);  // orange
    if (k < 3.5) return vec3(0.855, 0.135, 0.135);  // vermillion
    if (k < 4.5) return vec3(0.420, 0.030, 0.100);  // wine
    if (k < 5.5) return vec3(0.945, 0.440, 0.630);  // hot pink
    if (k < 6.5) return vec3(0.965, 0.800, 0.850);  // pale pink
    if (k < 7.5) return vec3(0.135, 0.310, 0.780);  // cobalt
    if (k < 8.5) return vec3(0.720, 0.800, 0.920);  // powder blue
    return vec3(0.075, 0.500, 0.230);               // leaf green
}

// Scalloped bottom edge of paper layer fi at column x.
float layerEdge(float fi, float x) {
    float h    = hash11(fi * 0.917 + 3.7);
    float h2   = hash11(fi * 2.113 + 9.1);
    float tier = floor(fi * 3.0 / gNf);
    float base = 1.10 - (fi + 1.0) * (1.26 / gNf);
    float dir  = mod(tier, 2.0) < 0.5 ? 1.0 : -1.0;
    float slide = gClk * (0.020 + 0.030 * h) * dir;

    // big cascade waves — per-layer phase lag fans the stack into waterfalls;
    // bass swells the whole relief
    float amp = reliefAmp * (0.052 + 0.075 * h2)
              * (1.0 + 0.06 * sin(gClk * 0.19 + tier * 2.1))
              * (1.0 + 0.55 * gA * gBassP);
    float e = base
            + amp * sin(x * 6.2832 * (0.52 + 0.30 * tier) + tier * 2.63 + fi * 0.42 + slide)
            + amp * 0.55 * sin(x * 6.2832 * 1.31 + fi * 0.77 - slide * 1.35 + h * 6.2832);

    // scalloped finger lobes; mids ripple their phase and depth
    float lf   = lobeDetail * (0.80 + 0.42 * hash11(fi * 5.31));
    float lph  = h * 6.2832 + slide * 2.2 + 0.35 * gA * gMidP * sin(gClk * 0.9 + fi * 1.7);
    float lamp = 0.013 * (0.7 + 0.6 * h2) * (1.0 + 1.0 * gA * gMidP);
    e -= lamp * pow(abs(sin(3.14159 * x * lf + lph)), 0.55);
    return e;
}

// Tier-weighted ink pick — like the reference: cream/pink/red pooling in the
// top tier, cobalt/powder/green dominating the middle, warm pinks below.
float pickK(float fi, float ep) {
    float h  = hash11(fi * 13.31 + ep * 7.71 + paletteShift * 2.13);
    float h2 = hash11(fi * 3.917 + ep * 5.13 + 0.7);
    float tier = floor(fi * 3.0 / gNf);
    float k = floor(h * 10.0);
    if (tier < 0.5)      { if (h2 < 0.42) k = (h < 0.35) ? 0.0 : ((h < 0.7) ? 6.0 : 3.0); }
    else if (tier < 1.5) { if (h2 < 0.52) k = (h < 0.40) ? 7.0 : ((h < 0.72) ? 8.0 : 9.0); }
    else                 { if (h2 < 0.42) k = (h < 0.45) ? 5.0 : ((h < 0.75) ? 3.0 : 1.0); }
    return k;
}

vec3 tintInk(vec3 ink, float k) {
    vec3 coolBase = vec3(0.135, 0.310, 0.780);
    vec3 warmBase = vec3(0.855, 0.135, 0.135);
    float isCool = step(6.5, k) * (1.0 - step(8.5, k));
    float isWarm = step(2.5, k) * (1.0 - step(5.5, k));
    ink *= mix(vec3(1.0), clamp(colorA.rgb / max(coolBase, vec3(0.02)), 0.0, 4.0), isCool * colorA.a);
    ink *= mix(vec3(1.0), clamp(colorB.rgb / max(warmBase, vec3(0.02)), 0.0, 4.0), isWarm * colorB.a);
    return ink;
}

// Flat curated ink of layer fi; on each beat exactly one (hash-permuted)
// layer advances its epoch — crossfaded by the 100ms ease envelope.
vec3 layerInk(float fi) {
    float perm = floor(hash11(fi * 0.677 + 11.0) * gNf);
    float ep   = floor((gCount + perm) / gNf);
    float kN = pickK(fi, ep);
    float kO = pickK(fi, ep - 1.0);
    float changing = 1.0 - step(0.5, mod(gCount + perm, gNf));
    return mix(tintInk(pal10(kN), kN), tintInk(pal10(kO), kO), changing * gEnv);
}

vec4 renderArt() {
    vec2 uv = isf_FragNormCoord.xy;
    float x = uv.x;
    float y = uv.y;

    // winner = frontmost layer whose scalloped edge is below this pixel
    float w = -1.0;
    float prevE = 10.0;
    for (int i = 0; i < 36; i++) {
        float fi = float(i);
        if (fi >= gNf) break;
        float e = layerEdge(fi, x);
        if (y > e) { w = fi; break; }
        prevE = e;
    }
    if (w < -0.5) w = gNf;   // below every edge: one virtual deepest sheet

    float aa = 2.2 / RENDERSIZE.y;
    float d = prevE - y;     // depth under the lip of the layer in front

    vec3 inkW = layerInk(w);
    inkW *= 1.0 + (hash21(vec2(w * 7.13, floor(y * 320.0))) - 0.5) * 0.045;  // paper streaks

    // ambient-occlusion shadow under every layer lip — sells the paper depth
    float shade = 1.0 - 0.40 * exp(-d * 150.0) - 0.20 * exp(-d * 38.0);
    vec3 colW = inkW * shade;

    vec3 col;
    if (w > 0.5) {
        vec3 inkA = layerInk(w - 1.0);
        col = mix(inkA, colW, smoothstep(0.0, aa, d));
    } else {
        col = colW;
    }

    // pale grey backing paper at the fringes, organic wavy inset
    vec3 bg = vec3(0.800, 0.806, 0.815);
    vec2 p = uv - 0.5;
    float wob = 0.014 * sin(uv.y * 19.0 + 1.7)
              + 0.009 * sin(uv.x * 23.0 - 0.4 + gClk * 0.03)
              + 0.007 * sin((uv.x + uv.y) * 37.0 + 2.2);
    float md = max(abs(p.x) / 0.452, abs(p.y) / 0.462) + wob;
    bg *= 1.0 - 0.28 * exp(-max(md - 1.0, 0.0) * 55.0);   // stack shadow on backing
    float inside = smoothstep(1.004, 0.998, md);
    col = mix(bg, col, inside);

    // paper tooth + level breath (can dip below 1)
    float g = hash21(uv * RENDERSIZE.xy);
    col += (g - 0.5) * 0.028;
    float lift = mix(1.0, 0.80 + 0.36 * knee(audioLevel, 0.03, 0.85), gA * 0.6);
    col *= brightness * lift;
    return vec4(max(col, 0.0), 1.0);
}

void main() {
    gA     = clamp(audioReact, 0.0, 1.0);
    gBassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    gMidP  = pow(knee(audioMid,  0.08, 0.85), 1.3);
    gClk   = TIME * slideSpeed + audioTime * 0.35 * gA * slideSpeed;
    gNf    = floor(layerCount + 0.5);

    if (PASSINDEX == 0) {
        // beat-counter state lives in one bottom-corner pixel
        if (gl_FragCoord.x > 1.0 || gl_FragCoord.y > 1.0) { gl_FragColor = vec4(0.0); return; }
        vec4 st = texture2D(stateBuf, vec2(0.5 / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
        float count = floor(st.r * 255.0 + 0.5);
        float env   = st.g;
        float pb    = st.b;
        float beatNow = max(audioBeat, step(0.6, audioBeatPulse));
        if (beatNow > 0.5 && pb < 0.5) { count = mod(count + 1.0, 250.0); env = 1.0; }
        env *= 0.76;   // ~100ms ease
        gl_FragColor = vec4(count / 255.0, env, step(0.5, beatNow), 1.0);
        return;
    }

    vec4 st = texture2D(stateBuf, vec2(0.5 / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
    gCount = floor(st.r * 255.0 + 0.5);
    gEnv   = st.g * gA;
    gl_FragColor = renderArt();
}
