/*{
  "DESCRIPTION": "Infinite Mandelbrot set zoom on a curated path of interesting points (sea-horse valley, elephant valley, mini-Mandelbrot near 0.286+0.011i). Smooth iteration count for continuous colour, audio-driven palette rotation, and zoom speed responds to bass. Pure mathematics as light show",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Fractal"
  ],
  "INPUTS": [
    {
      "NAME": "interiorGlow",
      "TYPE": "float",
      "DEFAULT": 0.25,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Interior Glow"
    },
    {
      "NAME": "maxIterations",
      "TYPE": "float",
      "DEFAULT": 160,
      "MIN": 50,
      "MAX": 200,
      "LABEL": "Max Iterations",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "palettePhase",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Palette Phase",
      "GROUP": "Color"
    },
    {
      "NAME": "paletteFreq",
      "TYPE": "float",
      "DEFAULT": 0.06,
      "MIN": 0.005,
      "MAX": 0.25,
      "LABEL": "Palette Frequency",
      "GROUP": "Color"
    },
    {
      "NAME": "fringeBoost",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1.5,
      "LABEL": "Fringe Boost",
      "GROUP": "Color"
    },
    {
      "NAME": "insideColor",
      "TYPE": "color",
      "DEFAULT": [
        0.02,
        0.02,
        0.08,
        1
      ],
      "LABEL": "Inside Color",
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "LABEL": "Color Boost",
      "GROUP": "Color"
    },
    {
      "NAME": "zoomSpeed",
      "TYPE": "float",
      "DEFAULT": 0.18,
      "MIN": 0.02,
      "MAX": 0.6,
      "LABEL": "Zoom Speed",
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "zoomDepth",
      "TYPE": "float",
      "DEFAULT": 14,
      "MIN": 4,
      "MAX": 22,
      "LABEL": "Zoom Depth",
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "audioReact",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "bass",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Bass",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "mid",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Mid",
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "treble",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Treble",
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ---------------------------------------------------------------------------
// Mandelbrot Zoom — pure-math fractal flight, audio-reactive colour & speed.
// ---------------------------------------------------------------------------

const float PI = 3.14159265359;

// Curated zoom targets — classic deep-zoom destinations.
const vec2 LOC0 = vec2(-0.7269,    0.1889);     // Seahorse Valley
const vec2 LOC1 = vec2(-0.74364,   0.13182);    // Tendril spirals
const vec2 LOC2 = vec2(-1.7497,    0.00001);    // Needle / mini-Mandelbrot
const vec2 LOC3 = vec2( 0.2965,    0.4836);     // Elephant valley edge
const vec2 LOC4 = vec2( 0.286,     0.011);      // Mini-Mandelbrot island

vec2 locationAt(int idx) {
    if (idx == 0) return LOC0;
    if (idx == 1) return LOC1;
    if (idx == 2) return LOC2;
    if (idx == 3) return LOC3;
    return LOC4;
}

// 4-stop cosine palette (Inigo Quilez style) — full hue circle.
vec3 palette(float t) {
    vec3 a = vec3(0.50, 0.50, 0.55);
    vec3 b = vec3(0.50, 0.45, 0.50);
    vec3 c = vec3(1.00, 1.00, 1.00);
    vec3 d = vec3(0.00, 0.33, 0.67);
    return a + b * cos(2.0 * PI * (c * t + d));
}

// Smooth easing for zoom transitions.
float easeInOut(float x) {
    return x * x * (3.0 - 2.0 * x);
}

void main() {
    vec2 res = RENDERSIZE;
    vec2 uv  = (gl_FragCoord.xy - 0.5 * res) / min(res.x, res.y);

    // ------------------------------------------------------------------ //
    // Zoom path — cycle through 5 locations.                             //
    // ------------------------------------------------------------------ //
    // Engine audio bus drives reactivity by default; the bass/mid/treble
    // INPUT knobs remain as manual overrides (max-merged).
    float bassPunch  = max(bass,   audioBass) * audioReact;
    float midPunch   = max(mid,    audioMid)  * audioReact;
    float treblePunch= max(treble, audioHigh) * audioReact;

    float speed = zoomSpeed * (1.0 + 0.6 * bassPunch);
    float t     = TIME * speed;

    // Each location segment: hold ~10s + transition ~3s @ unit speed.
    // Total ~13s per segment in scaled time.
    float segLen = 13.0;
    float seg    = mod(t, segLen * 5.0) / segLen;
    int   idxA   = int(floor(seg));
    int   idxB   = int(mod(float(idxA) + 1.0, 5.0));
    float local  = fract(seg);

    // Hold for first 10/13 of segment, then ease to next.
    float holdRatio = 10.0 / 13.0;
    float blend = local <= holdRatio
        ? 0.0
        : easeInOut((local - holdRatio) / (1.0 - holdRatio));

    vec2 cA = locationAt(idxA);
    vec2 cB = locationAt(idxB);
    vec2 center = mix(cA, cB, blend);

    // Log-zoom: oscillate depth between 1.0 and zoomDepth so we breathe.
    float zoomCycle = 0.5 - 0.5 * cos(t * 0.18);   // 0..1
    float logZoom   = mix(1.0, zoomDepth, zoomCycle);
    float scale     = exp(-logZoom);                // tiny window

    vec2 c = center + uv * scale * 3.0;

    // ------------------------------------------------------------------ //
    // Mandelbrot iteration with smooth-iteration colouring.              //
    // ------------------------------------------------------------------ //
    float maxIt = clamp(maxIterations, 50.0, 200.0);
    int   maxI  = int(maxIt);

    vec2  z   = vec2(0.0);
    float i   = 0.0;
    float esc = 0.0;
    float r2  = 0.0;

    for (int n = 0; n < 200; n++) {
        if (n >= maxI) break;
        z = vec2(z.x * z.x - z.y * z.y, 2.0 * z.x * z.y) + c;
        r2 = dot(z, z);
        if (r2 > 256.0) {        // larger bailout → smoother nu
            esc = 1.0;
            break;
        }
        i += 1.0;
    }

    vec3 col;

    if (esc < 0.5) {
        // ----- Inside the set: deep colour with subtle interior structure.
        float interior = 0.5 + 0.5 * sin(8.0 * (z.x + z.y) + TIME * 0.5);
        vec3 inside = insideColor.rgb + interior * interiorGlow * vec3(0.05, 0.08, 0.15);
        col = inside;
    } else {
        // ----- Outside: smooth iteration count.
        float logZn = log(r2) * 0.5;
        float nu    = log(logZn / log(2.0)) / log(2.0);
        float sm    = i + 1.0 - nu;

        // Palette index — audio mid rotates hue, palettePhase user-controlled.
        float paletteIdx = sm * paletteFreq + palettePhase + midPunch * 0.5
                          + TIME * 0.02;
        col = palette(paletteIdx);

        // Treble brightens the fringe (low iteration counts near edge get extra punch).
        float fringe = exp(-sm * 0.04);
        col += fringe * fringeBoost * (0.3 + 0.7 * treblePunch);

        // Soft vignette of brightness with iteration depth.
        float depth = clamp(sm / maxIt, 0.0, 1.0);
        col *= mix(0.7, 1.15, 1.0 - depth);
    }

    // Light global tonemap.
    col = col / (1.0 + col);
    col = pow(col, vec3(0.85));

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);
    gl_FragColor = vec4(col, 1.0);
}
