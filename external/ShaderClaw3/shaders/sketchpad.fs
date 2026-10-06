/*{
  "DESCRIPTION": "SketchPad — animated sketch on paper: rough circles, drifting lines/rectangles, scribble hatching, with outline or filled-color modes and halftone screen-print overlap blending",
  "CREDIT": "ShaderClaw",
  "CATEGORIES": [
    "Generator"
  ],
  "INPUTS": [
    {
      "NAME": "renderMode",
      "LABEL": "Render Mode",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2
      ],
      "LABELS": [
        "Outline (B&W)",
        "Filled Color",
        "Mixed (Outline + Fill)"
      ]
    },
    {
      "NAME": "fadeAmount",
      "LABEL": "Stroke Fade",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "screenAmount",
      "LABEL": "Halftone Blend",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "screenAngle",
      "LABEL": "Halftone Angle",
      "TYPE": "float",
      "DEFAULT": 0.52,
      "MIN": 0,
      "MAX": 3.14
    },
    {
      "NAME": "hatchAngle",
      "LABEL": "Hatch Angle",
      "TYPE": "float",
      "DEFAULT": 0.78,
      "MIN": 0,
      "MAX": 3.14
    },
    {
      "NAME": "tickMarks",
      "LABEL": "Tick Marks",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1.5
    },
    {
      "NAME": "vignette",
      "LABEL": "Vignette",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "texInfluence",
      "LABEL": "Texture Influence",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "inputTex",
      "LABEL": "Reference",
      "TYPE": "image"
    },
    {
      "NAME": "gridSize",
      "LABEL": "Grid Size",
      "TYPE": "float",
      "DEFAULT": 40,
      "MIN": 8,
      "MAX": 120,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "shapeType",
      "LABEL": "Line Shape",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2
      ],
      "LABELS": [
        "Lines",
        "Rectangles",
        "Mixed"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "strokeWidth",
      "LABEL": "Stroke Width",
      "TYPE": "float",
      "DEFAULT": 1.6,
      "MIN": 0.3,
      "MAX": 6,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "roughness",
      "LABEL": "Roughness",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "doubleStroke",
      "LABEL": "Double Stroke",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "circleCount",
      "LABEL": "Circles",
      "TYPE": "float",
      "DEFAULT": 4,
      "MIN": 0,
      "MAX": 10,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "lineCount",
      "LABEL": "Lines/Rects",
      "TYPE": "float",
      "DEFAULT": 6,
      "MIN": 0,
      "MAX": 14,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "sizeVariance",
      "LABEL": "Size Variance",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "screenSize",
      "LABEL": "Halftone Size",
      "TYPE": "float",
      "DEFAULT": 7,
      "MIN": 2,
      "MAX": 30,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "scribbleDensity",
      "LABEL": "Scribble",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "sketchSpeed",
      "LABEL": "Sketch Speed",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 2.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "decayAmount",
      "LABEL": "Line Decay",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "decayPeriod",
      "LABEL": "Decay Period",
      "TYPE": "float",
      "DEFAULT": 6,
      "MIN": 1,
      "MAX": 20,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "decayCoverage",
      "LABEL": "Decay Coverage",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "sizePulse",
      "LABEL": "Size Pulse",
      "TYPE": "float",
      "DEFAULT": 0.4,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorSpeed",
      "LABEL": "Color Shift Speed",
      "TYPE": "float",
      "DEFAULT": 0.2,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "inkColor",
      "LABEL": "Ink Color",
      "TYPE": "color",
      "DEFAULT": [
        0.95,
        0.97,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "inkIntensity",
      "LABEL": "Ink Intensity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0.2,
      "MAX": 2.5,
      "GROUP": "Color"
    },
    {
      "NAME": "hueBase",
      "LABEL": "Hue Base",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "hueSpread",
      "LABEL": "Hue Spread",
      "TYPE": "float",
      "DEFAULT": 0.75,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Color"
    },
    {
      "NAME": "fillSaturation",
      "LABEL": "Fill Saturation",
      "TYPE": "float",
      "DEFAULT": 0.85,
      "MIN": 0,
      "MAX": 1.2,
      "GROUP": "Color"
    },
    {
      "NAME": "fillBrightness",
      "LABEL": "Fill Brightness",
      "TYPE": "float",
      "DEFAULT": 0.95,
      "MIN": 0.1,
      "MAX": 1.6,
      "GROUP": "Color"
    },
    {
      "NAME": "screenContrast",
      "LABEL": "Screen Contrast",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Color"
    },
    {
      "NAME": "paperType",
      "LABEL": "Paper",
      "TYPE": "long",
      "DEFAULT": 2,
      "VALUES": [
        0,
        1,
        2,
        3
      ],
      "LABELS": [
        "Plain",
        "Dot Grid",
        "Graph Paper",
        "Lined Paper"
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "paperColor",
      "LABEL": "Paper Color",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "gridLineStrength",
      "LABEL": "Grid Strength",
      "TYPE": "float",
      "DEFAULT": 0.25,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Background"
    },
    {
      "NAME": "gridSubdiv",
      "LABEL": "Major Every",
      "TYPE": "float",
      "DEFAULT": 5,
      "MIN": 1,
      "MAX": 12,
      "GROUP": "Background"
    },
    {
      "NAME": "paperGrain",
      "LABEL": "Paper Grain",
      "TYPE": "float",
      "DEFAULT": 0.12,
      "MIN": 0,
      "MAX": 0.5,
      "GROUP": "Background"
    },
    {
      "NAME": "audioBassPress",
      "LABEL": "Bass Pressure",
      "TYPE": "float",
      "DEFAULT": 0.8,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "audioMidDraw",
      "LABEL": "Mid Draw",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "audioHighScribble",
      "LABEL": "High Scribble",
      "TYPE": "float",
      "DEFAULT": 0.9,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "audioSizePulse",
      "LABEL": "Audio Size Pulse",
      "TYPE": "float",
      "DEFAULT": 0.8,
      "MIN": 0,
      "MAX": 3,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

#ifdef GL_ES
precision highp float;
#endif

#define PI 3.14159265358979
#define TAU 6.28318530718

// ---------- hashing ----------
float hash11(float n) { return fract(sin(n * 127.1) * 43758.5453); }
float hash12(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float vnoise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash12(i);
    float b = hash12(i + vec2(1.0, 0.0));
    float c = hash12(i + vec2(0.0, 1.0));
    float d = hash12(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

// ---------- palette ----------
// cosine palette: smooth hue-like band parameterized by t
vec3 palette(float t) {
    vec3 a = vec3(0.55);
    vec3 b = vec3(0.45);
    vec3 c = vec3(1.0, 1.0, 1.0);
    vec3 d = vec3(0.00, 0.33, 0.67);
    return a + b * cos(TAU * (c * t + d));
}

vec3 shapeColor(float id, float timeOffset) {
    float t = hueBase + id * hueSpread * 0.17 + timeOffset * colorSpeed * 0.1;
    vec3 c = palette(t);
    // desaturate toward gray by fillSaturation
    float lum = dot(c, vec3(0.299, 0.587, 0.114));
    c = mix(vec3(lum), c, fillSaturation);
    c *= fillBrightness;
    return c;
}

// ---------- ink helpers ----------
float inkStroke(float d, float width) {
    float hw = width * 0.5;
    return 1.0 - smoothstep(hw - 1.0, hw + 1.0, d);
}
float fillMask(float d) {
    return 1.0 - smoothstep(-1.0, 1.0, d);
}

float sdSegment(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a;
    vec2 ba = b - a;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 0.0001), 0.0, 1.0);
    return length(pa - ba * h);
}

float sdRoughCircle(vec2 p, vec2 c, float r, float seed, float rough, float wobTime) {
    vec2 d = p - c;
    float ang = atan(d.y, d.x);
    float wob = 0.0;
    wob += sin(ang * 3.0 + seed * 7.0 + wobTime * 0.6) * 0.35;
    wob += sin(ang * 7.0 + seed * 3.0 + wobTime * 1.1) * 0.18;
    wob += vnoise(vec2(ang * 2.0, seed + wobTime * 0.2)) * 0.5 - 0.25;
    float rr = r + wob * rough * r * 0.08;
    return length(d) - rr;  // signed: negative inside
}

float sdRoughLine(vec2 p, vec2 a, vec2 b, float seed, float rough, float wobTime) {
    vec2 ba = b - a;
    float L = length(ba);
    vec2 dir = ba / max(L, 0.0001);
    vec2 nrm = vec2(-dir.y, dir.x);
    vec2 pa = p - a;
    float t = clamp(dot(pa, dir), 0.0, L);
    float perp = dot(pa, nrm);
    float wob = sin(t * 0.03 + seed * 5.0 + wobTime * 0.7) * 1.5
              + sin(t * 0.11 + seed * 1.7 + wobTime * 1.3) * 0.7;
    wob += (vnoise(vec2(t * 0.02, seed)) - 0.5) * 3.0;
    float d = abs(perp - wob * rough);
    float outside = max(max(-dot(pa, dir), dot(pa, dir) - L), 0.0);
    return sqrt(d * d + outside * outside);
}

// Signed distance to an oriented rectangle defined by center c, half-extents e, rotation angle
float sdRoughRect(vec2 p, vec2 c, vec2 e, float ang, float seed, float rough, float wobTime) {
    float ca = cos(ang), sa = sin(ang);
    vec2 q = p - c;
    q = vec2(ca * q.x + sa * q.y, -sa * q.x + ca * q.y);
    // Wobble corners by adding noise to half-extents per quadrant
    float wx = (vnoise(vec2(q.y * 0.05, seed + wobTime * 0.3)) - 0.5) * 2.0;
    float wy = (vnoise(vec2(q.x * 0.05, seed + wobTime * 0.4 + 5.1)) - 0.5) * 2.0;
    vec2 ew = e + vec2(wx, wy) * rough * 1.5;
    vec2 d = abs(q) - ew;
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

// Scribble hatching field
float scribbleField(vec2 p, float angle, float density, float wobTime) {
    float ca = cos(angle), sa = sin(angle);
    vec2 r = vec2(ca * p.x + sa * p.y, -sa * p.x + ca * p.y);
    float freq = mix(6.0, 20.0, density);
    float stripe = sin(r.x * freq + vnoise(r * 0.04 + wobTime * 0.2) * 6.0);
    float amp = 0.5 + 0.5 * stripe;
    float gate = vnoise(vec2(r.y * 0.15, wobTime * 0.1));
    return amp * smoothstep(0.35, 0.65, gate);
}

// Paper pattern
float paperPattern(vec2 p, float cellPx, float subdiv) {
    if (paperType < 0.5) return 0.0;
    vec2 g = p / max(cellPx, 1.0);
    if (paperType < 1.5) {
        vec2 ij = floor(g) + 0.5;
        vec2 center = ij * cellPx;
        float d = length(p - center);
        float dot = 1.0 - smoothstep(1.2, 2.2, d);
        float jit = (hash12(ij) - 0.5) * 0.4;
        dot *= 0.9 + jit;
        return clamp(dot, 0.0, 1.0);
    }
    if (paperType < 2.5) {
        vec2 fp = fract(g);
        vec2 dEdge = min(fp, 1.0 - fp) * cellPx;
        float minor = 1.0 - smoothstep(0.0, 1.2, min(dEdge.x, dEdge.y));
        vec2 major = min(fract(g / subdiv), 1.0 - fract(g / subdiv)) * cellPx * subdiv;
        float maj = 1.0 - smoothstep(0.0, 1.8, min(major.x, major.y));
        return clamp(minor * 0.4 + maj * 0.9, 0.0, 1.0);
    }
    float fy = fract(g.y);
    float dEdge = min(fy, 1.0 - fy) * cellPx;
    float line = 1.0 - smoothstep(0.0, 1.4, dEdge);
    float margin = smoothstep(0.5, 0.9, g.y / 2.0);
    return clamp(line * margin, 0.0, 1.0);
}

// ---------- halftone ----------
// Returns 0..1 "dot coverage" at pixel p for a given rotated screen
// threshold: how filled the cell is (0 empty, 1 full)
float halftone(vec2 p, float cellPx, float ang, float threshold) {
    float ca = cos(ang), sa = sin(ang);
    vec2 r = vec2(ca * p.x + sa * p.y, -sa * p.x + ca * p.y);
    vec2 g = r / max(cellPx, 1.0);
    vec2 f = fract(g) - 0.5;
    float d = length(f);
    // d in [0, ~0.707]. Full dot when threshold=1.
    float radius = threshold * 0.707;
    return 1.0 - smoothstep(radius - 0.05, radius + 0.05, d);
}

// ---------- animated shape parameters ----------
vec2 circlePos(float i, float t, vec2 Res) {
    float s1 = hash11(i * 3.31 + 1.0);
    float s2 = hash11(i * 7.11 + 2.0);
    float s3 = hash11(i * 11.7 + 3.0);
    float fx = 0.25 + s1 * 1.5;
    float fy = 0.3 + s2 * 1.2;
    return vec2(
        0.5 + sin(t * fx + s3 * TAU) * 0.32,
        0.5 + cos(t * fy + s1 * TAU) * 0.32
    ) * Res;
}

float circleRadius(float i, float t, float bassPress, float audioPulse, float Rmin) {
    float s = hash11(i * 13.1 + 5.0);
    float base = Rmin * (0.4 + s * 1.1 * sizeVariance);
    float breathe = 0.5 + 0.5 * sin(t * (0.5 + s * 1.5) + s * 10.0);
    float sizeMod = 1.0 + breathe * sizePulse * 0.7 + audioPulse * audioSizePulse * 0.5;
    return base * sizeMod * (1.0 + bassPress * 0.4);
}

void linePos(float i, float t, vec2 Res, out vec2 a, out vec2 b) {
    float s1 = hash11(i * 5.37 + 11.0);
    float s2 = hash11(i * 2.91 + 17.0);
    float s3 = hash11(i * 9.13 + 23.0);
    float s4 = hash11(i * 6.77 + 29.0);
    a = vec2(
        0.2 + sin(t * (0.3 + s1 * 0.7) + s2 * TAU) * 0.35,
        0.2 + cos(t * (0.4 + s2 * 0.6) + s3 * TAU) * 0.35
    ) * Res;
    b = vec2(
        0.5 + sin(t * (0.35 + s3 * 0.8) + s4 * TAU) * 0.4,
        0.5 + cos(t * (0.45 + s4 * 0.7) + s1 * TAU) * 0.4
    ) * Res;
}

// Rectangle params for line index i
void rectParams(float i, float t, float audioPulse, vec2 Res, out vec2 c, out vec2 e, out float ang) {
    float s1 = hash11(i * 4.19 + 31.0);
    float s2 = hash11(i * 8.33 + 37.0);
    float s3 = hash11(i * 1.91 + 41.0);
    float s4 = hash11(i * 6.47 + 43.0);
    c = vec2(
        0.5 + sin(t * (0.25 + s1 * 0.6) + s2 * TAU) * 0.35,
        0.5 + cos(t * (0.3 + s2 * 0.5) + s3 * TAU) * 0.35
    ) * Res;
    float rmin = min(Res.x, Res.y);
    float breathe = 0.5 + 0.5 * sin(t * (0.7 + s3 * 1.3) + s4 * TAU);
    float sizeMod = 1.0 + breathe * sizePulse * 0.9 + audioPulse * audioSizePulse * 0.6;
    e = vec2(
        rmin * (0.05 + s1 * 0.18 * sizeVariance) * sizeMod,
        rmin * (0.03 + s3 * 0.15 * sizeVariance) * sizeMod
    );
    ang = s4 * TAU + t * (0.1 + s2 * 0.4);
}

// Choose whether index i uses rectangle (returns true) or line (false)
bool isRect(float i) {
    if (shapeType < 0.5) return false;           // all lines
    if (shapeType < 1.5) return true;            // all rects
    return hash11(i * 17.3 + 0.7) > 0.5;         // mixed
}

// Decay envelope for line/rect index i — quick ramp in, slow fade out
// Returns alpha in [0, 1]. Only shapes in decayCoverage fraction decay.
float decayEnvelope(float i, float t) {
    float selector = hash11(i * 31.7 + 0.91);
    bool decays = selector < decayCoverage;
    if (!decays || decayAmount < 0.001) return 1.0;

    float offset = hash11(i * 13.1 + 5.3) * decayPeriod;
    float phase = fract((t + offset) / max(decayPeriod, 0.1));
    // Quick "drawn" phase (0..0.08), hold (0.08..0.35), long fade (0.35..1.0)
    float rampIn = smoothstep(0.0, 0.08, phase);
    float rampOut = 1.0 - smoothstep(0.35, 1.0, phase);
    float env = rampIn * rampOut;
    return mix(1.0, env, decayAmount);
}

// ---------- fill layer accumulator ----------
// Track top 2 colors by "recency" (iteration order = drawing order),
// plus their coverage. When two layers overlap, halftone interleaves them.
struct FillLayer {
    vec3 color;
    float cover;  // 0..1
    float id;     // shape id for screen-angle offset
};

void pushFill(inout FillLayer top, inout FillLayer mid, vec3 c, float cover, float id) {
    if (cover < 0.01) return;
    // slide: mid <- top, top <- new
    mid = top;
    top.color = c;
    top.cover = cover;
    top.id = id;
}

void main() {
    vec2 Res = RENDERSIZE;
    vec2 p = gl_FragCoord.xy;
    vec2 uv = isf_FragNormCoord.xy;

    float bass = audioBass;
    float mid = audioMid;
    float high = audioHigh;
    float level = audioLevel;
    float audioPulse = bass + level * 0.3;

    float tt = TIME * sketchSpeed * (1.0 + mid * audioMidDraw);
    float Rmin = min(Res.x, Res.y);

    // ---- Paper background ----
    vec3 paper = paperColor.rgb;
    float grain = vnoise(p * 0.35) * 2.0 - 1.0;
    paper += grain * paperGrain * 0.3;
    float paperInk = paperPattern(p, gridSize, gridSubdiv) * gridLineStrength;

    // ---- Reference texture hatch (darker → more ink) ----
    bool hasTex = IMG_SIZE_inputTex.x > 0.0;
    float texHatch = 0.0;
    if (hasTex && texInfluence > 0.001) {
        vec4 tx = IMG_NORM_PIXEL(inputTex, uv);
        float brightness = dot(tx.rgb, vec3(0.299, 0.587, 0.114));
        float hatch = scribbleField(p, hatchAngle + tt * 0.1, scribbleDensity, tt);
        texHatch = (1.0 - brightness) * hatch * texInfluence * 0.8;
    }

    float pressScale = 1.0 + bass * audioBassPress;
    float sw = strokeWidth * pressScale;
    float rough = roughness * (1.0 + high * 0.3);

    // Fill accumulator (top 2)
    FillLayer top = FillLayer(vec3(0.0), 0.0, 0.0);
    FillLayer mid2 = FillLayer(vec3(0.0), 0.0, 0.0);

    float ink = 0.0;

    // ===== Circles =====
    for (int i = 0; i < 10; i++) {
        if (float(i) >= circleCount) break;
        float fi = float(i);
        vec2 c = circlePos(fi, tt, Res);
        float r = circleRadius(fi, tt, bass * audioBassPress, audioPulse, Rmin * 0.22);

        float dSigned = sdRoughCircle(p, c, r, fi * 3.7, rough, tt);
        float absD = abs(dSigned);

        // Outline
        float s = inkStroke(absD, sw);
        if (doubleStroke > 0.001) {
            float dSigned2 = sdRoughCircle(p + vec2(1.5, -1.0), c, r, fi * 3.7 + 1.3, rough, tt);
            s = max(s, inkStroke(abs(dSigned2), sw * 0.7) * doubleStroke);
        }
        if (fadeAmount > 0.001) {
            vec2 dv = p - c;
            float ang = atan(dv.y, dv.x);
            float fade = 0.5 + 0.5 * sin(ang * 3.0 + fi * 4.1 + tt * 0.5);
            s *= mix(1.0, fade, fadeAmount);
        }

        // Fill (inside of shape)
        float fill = fillMask(dSigned);
        if (renderMode > 0.5 && fill > 0.01) {
            vec3 col = shapeColor(fi + 0.13, tt);
            pushFill(top, mid2, col, fill, fi);
        }

        ink = max(ink, s);
    }

    // ===== Lines / Rectangles =====
    for (int i = 0; i < 14; i++) {
        if (float(i) >= lineCount) break;
        float fi = float(i);
        bool asRect = isRect(fi);
        float env = decayEnvelope(fi, TIME);

        if (asRect) {
            vec2 rc, re; float rang;
            rectParams(fi, tt + fi * 0.17, audioPulse, Res, rc, re, rang);
            float dSigned = sdRoughRect(p, rc, re, rang, fi * 4.3, rough * 2.5, tt);
            float absD = abs(dSigned);

            float s = inkStroke(absD, sw);
            if (doubleStroke > 0.001) {
                float dSigned2 = sdRoughRect(p + vec2(1.0, -1.2), rc, re, rang, fi * 4.3 + 0.9, rough * 2.5, tt);
                s = max(s, inkStroke(abs(dSigned2), sw * 0.6) * doubleStroke * 0.8);
            }
            s *= env;

            float fill = fillMask(dSigned) * env;
            if (renderMode > 0.5 && fill > 0.01) {
                vec3 col = shapeColor(fi * 1.7 + 5.7, tt);
                pushFill(top, mid2, col, fill, fi + 100.0);
            }

            ink = max(ink, s);
        } else {
            vec2 a, b;
            linePos(fi, tt + fi * 0.17, Res, a, b);
            float d = sdRoughLine(p, a, b, fi * 5.3, rough * 3.0, tt);
            float s = inkStroke(d, sw);
            if (doubleStroke > 0.001) {
                float d2 = sdRoughLine(p + vec2(0.8, 1.2), a, b, fi * 5.3 + 0.9, rough * 3.0, tt);
                s = max(s, inkStroke(d2, sw * 0.6) * doubleStroke * 0.8);
            }
            if (fadeAmount > 0.001) {
                float tparam = clamp(dot(p - a, normalize(b - a)) / max(length(b - a), 0.001), 0.0, 1.0);
                float fade = smoothstep(0.0, 0.15, tparam) * (1.0 - smoothstep(0.85, 1.0, tparam));
                s *= mix(1.0, fade, fadeAmount);
            }
            s *= env;
            ink = max(ink, s);
        }
    }

    // ===== Scribble blobs =====
    if (scribbleDensity > 0.001) {
        float angle = hatchAngle + sin(tt * 0.4) * 0.2;
        float hatch = scribbleField(p * (0.6 + high * 0.8), angle, scribbleDensity, tt);
        float blob = 0.0;
        for (int k = 0; k < 4; k++) {
            float fk = float(k);
            vec2 bc = vec2(
                0.5 + sin(tt * (0.2 + fk * 0.15) + fk * 1.7) * 0.3,
                0.5 + cos(tt * (0.17 + fk * 0.19) + fk * 2.3) * 0.3
            ) * Res;
            float br = Rmin * (0.08 + fk * 0.03);
            blob = max(blob, 1.0 - smoothstep(0.3 * br, br, length(p - bc)));
        }
        ink = max(ink, hatch * blob * scribbleDensity * 0.75);
    }

    // ===== Tick marks =====
    if (tickMarks > 0.001) {
        vec2 tp = p * 0.07;
        vec2 tic = floor(tp);
        float has = step(hash12(tic + floor(tt * 0.5)), tickMarks * 0.12);
        float tickLen = 4.0 + hash12(tic) * 6.0;
        vec2 dir = vec2(cos(hash12(tic + 7.0) * TAU), sin(hash12(tic + 7.0) * TAU));
        vec2 tcenter = (tic + 0.5) / 0.07;
        vec2 ta = tcenter - dir * tickLen;
        vec2 tb = tcenter + dir * tickLen;
        float d = sdSegment(p, ta, tb);
        ink = max(ink, inkStroke(d, sw * 0.9) * has * 0.9);
    }

    // ===== High-freq sparkle =====
    if (audioHighScribble > 0.001 && high > 0.01) {
        float sp = hash12(p * 0.2 + vec2(TIME * 13.0, TIME * 9.0));
        float dashGate = smoothstep(0.93 - high * 0.2, 1.0, sp);
        float dashLine = sin(p.x * 0.2 + p.y * 0.15 + TIME * 6.0) * 0.5 + 0.5;
        ink = max(ink, dashGate * dashLine * audioHighScribble * 0.4);
    }

    ink += texHatch;
    ink *= inkIntensity;
    ink = clamp(ink, 0.0, 1.3);

    // ===== Compose =====
    vec3 col = paper;
    col = mix(col, inkColor.rgb, paperInk * 0.55);

    // Fill layers first (so outline overwrites)
    if (renderMode > 0.5 && top.cover > 0.01) {
        // Choose top vs mid using halftone interleave when both overlap
        float overlap = min(top.cover, mid2.cover);
        // Use a screen-print interleave when overlap is significant
        float angTop = screenAngle + top.id * 0.3;
        float angMid = screenAngle + mid2.id * 0.3 + PI * 0.25;

        // Halftone dot coverage for top color at this pixel
        // Threshold drawn from top.cover^contrast
        float thrTop = pow(clamp(top.cover, 0.0, 1.0), 1.0 / max(screenContrast, 0.1));
        float dotTop = halftone(p, screenSize, angTop, thrTop);

        float thrMid = pow(clamp(mid2.cover, 0.0, 1.0), 1.0 / max(screenContrast, 0.1));
        float dotMid = halftone(p, screenSize, angMid, thrMid);

        // Base fill: solid top color scaled by coverage
        vec3 baseFill = mix(paper, top.color, top.cover);

        // When overlap: interleave by halftone — top dots show top color, mid dots show mid color
        vec3 overlapFill = baseFill;
        if (overlap > 0.05) {
            // use halftone alternation driven by screenAmount
            vec3 alt = mix(mid2.color, top.color, dotTop);
            // bring in mid color on its own screen pattern at the overlap region
            alt = mix(alt, mid2.color, dotMid * (1.0 - dotTop) * 0.8);
            overlapFill = mix(baseFill, alt, screenAmount);
        }

        vec3 fillComposite = mix(baseFill, overlapFill, clamp(overlap * 4.0, 0.0, 1.0));
        col = mix(col, fillComposite, top.cover);
    }

    // Outline ink on top
    col = mix(col, inkColor.rgb, clamp(ink, 0.0, 1.0));

    // Plain-paper wet glow around ink
    col += (inkColor.rgb - paper) * ink * 0.15;

    // Vignette
    float v = length(uv - 0.5);
    col *= mix(1.0, smoothstep(0.85, 0.15, v), vignette);

    // Overall audio gain bump
    col *= (1.0 + level * 0.15);

    gl_FragColor = vec4(col, 1.0);
}
