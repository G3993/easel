/*{
  "CATEGORIES": ["Generator", "Audio", "3D"],
  "DESCRIPTION": "Ivory 3D fluid — an all-white particle cloud with soft depth shading on black, wandering randomly, tuned for audio reactivity",
  "INPUTS": [
    { "NAME": "scale",   "LABEL": "Scale",         "TYPE": "float", "MIN": 0.5, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Shape / Geometry" },
    { "NAME": "size",    "LABEL": "Particle Size", "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Shape / Geometry" },
    { "NAME": "density", "LABEL": "Density",       "TYPE": "float", "MIN": 0.2, "MAX": 1.0, "DEFAULT": 0.8, "GROUP": "Shape / Geometry" },
    { "NAME": "speed",   "LABEL": "Speed",         "TYPE": "float", "MIN": 0.1, "MAX": 3.0, "DEFAULT": 1.0, "GROUP": "Motion / Animation" },
    { "NAME": "wander",  "LABEL": "Wander",        "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6, "GROUP": "Motion / Animation" },
    { "NAME": "warmth",  "LABEL": "Warmth",        "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.25,"GROUP": "Color" },
    { "NAME": "glow",    "LABEL": "Glow",          "TYPE": "float", "MIN": 0.2, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Color" }
  ]
}*/

// FLUID3D / IVORY — the mostly-white take on the 3D Fluid: soft ivory glow
// blobs, depth does the shading (near = bright white, far = quiet gray),
// black background always. Bass pumps and bursts, mids swirl, highs put a
// crystalline sparkle on the nearest particles. CORE audio bus only.

const int   N     = 90;
const float SEG_T = 5.0;

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
vec3  hash31(float n) {
    return fract(sin(vec3(n, n + 1.7, n + 2.9)) *
                 vec3(43758.5453, 22578.145, 19642.318));
}

vec3 particlePos(float id, float t, float wanderAmt) {
    vec3 h1 = hash31(id * 7.13);
    vec3 h2 = hash31(id * 13.7 + 5.0);
    vec3 h3 = hash31(id * 29.3 + 11.0);
    vec3 p = vec3(sin(t * (0.31 + h1.x * 0.9) + h2.x * 6.2832),
                  sin(t * (0.27 + h1.y * 0.8) + h2.y * 6.2832),
                  sin(t * (0.23 + h1.z * 0.7) + h2.z * 6.2832));
    p += 0.35 * vec3(sin(t * (0.9 + h3.x) + p.y * 2.0),
                     sin(t * (0.8 + h3.y) + p.z * 2.0),
                     sin(t * (0.7 + h3.z) + p.x * 2.0));
    p *= 0.45 + 0.55 * h1.y;
    float seg = floor(t / SEG_T);
    float f   = smoothstep(0.15, 0.85, fract(t / SEG_T));
    vec3 tgtA = (hash31(id * 3.1 + seg * 17.0) - 0.5) * 2.0;
    vec3 tgtB = (hash31(id * 3.1 + (seg + 1.0) * 17.0) - 0.5) * 2.0;
    return mix(p, mix(tgtA, tgtB, f), 0.5 * wanderAmt);
}

mat3 rotY(float a) {
    float c = cos(a), s = sin(a);
    return mat3(c, 0.0, s, 0.0, 1.0, 0.0, -s, 0.0, c);
}
mat3 rotX(float a) {
    float c = cos(a), s = sin(a);
    return mat3(1.0, 0.0, 0.0, 0.0, c, -s, 0.0, s, c);
}

void main() {
    vec2 uv = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y;

    // Soft-knee audio conditioning (playbook standard snippet).
    float bassP = pow(clamp(smoothstep(0.05, 0.85, audioBass), 0.0, 1.0), 1.6);
    float midP  = clamp(smoothstep(0.08, 0.85, audioMid), 0.0, 1.0);
    float highP = pow(clamp(smoothstep(0.10, 0.90, audioHigh), 0.0, 1.0), 1.2);
    float drive = 0.25 + 0.75 * clamp(smoothstep(0.05, 0.9, audioLevel), 0.0, 1.0);

    float t = TIME * speed * (0.55 + 0.9 * drive);

    mat3 cam = rotX(0.35 + 0.15 * sin(t * 0.11)) *
               rotY(t * (0.12 + 0.10 * midP));
    float burst = 1.0 + 0.22 * bassP;

    // Ivory palette: white with a whisper of warmth; cool gray in the depth.
    vec3 ivory = mix(vec3(1.0), vec3(1.0, 0.93, 0.82), warmth);
    vec3 deep  = vec3(0.42, 0.44, 0.50);

    vec3 col = vec3(0.0);   // black background, always

    for (int i = 0; i < N; i++) {
        float id = float(i);
        if (hash11(id * 91.7) > density) continue;

        vec3 q = cam * (particlePos(id, t, wander) * burst) / scale;
        float zc = q.z + 3.2;
        if (zc < 0.4) continue;
        float persp = 1.9 / zc;
        vec2  sp    = q.xy * persp;

        float rad = (0.030 + 0.05 * hash11(id * 3.77)) * size * persp;
        rad *= 1.0 + 0.32 * bassP;

        float d    = length(uv - sp);
        float core = exp(-(d * d) / (rad * rad * 0.55));   // tight bright core
        float soft = exp(-(d * d) / (rad * rad * 5.0));    // soft body
        if (core + soft < 0.002) continue;

        float nearT = clamp((3.2 - zc) * 0.55 + 0.5, 0.0, 1.0);
        vec3  pc    = mix(deep, ivory, nearT);
        // Crystalline sparkle: highs flash only the nearest, brightest few.
        pc += vec3(0.35) * highP * step(0.8, hash11(id * 5.31)) * nearT;

        col += pc * (core * 1.1 + soft * 0.16) * glow * (0.7 + 0.6 * drive);
    }

    col = 1.0 - exp(-col * 1.5);
    gl_FragColor = vec4(col, 1.0);
}
