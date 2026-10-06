/*{
  "CATEGORIES": ["Generator", "Audio", "3D"],
  "DESCRIPTION": "Prismatic 3D fluid — a full-spectrum particle cloud glowing on black, wandering randomly, tuned for audio reactivity",
  "INPUTS": [
    { "NAME": "scale",    "LABEL": "Scale",         "TYPE": "float", "MIN": 0.5, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Shape / Geometry" },
    { "NAME": "size",     "LABEL": "Particle Size", "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Shape / Geometry" },
    { "NAME": "density",  "LABEL": "Density",       "TYPE": "float", "MIN": 0.2, "MAX": 1.0, "DEFAULT": 0.85,"GROUP": "Shape / Geometry" },
    { "NAME": "speed",    "LABEL": "Speed",         "TYPE": "float", "MIN": 0.1, "MAX": 3.0, "DEFAULT": 1.0, "GROUP": "Motion / Animation" },
    { "NAME": "wander",   "LABEL": "Wander",        "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6, "GROUP": "Motion / Animation" },
    { "NAME": "hueShift", "LABEL": "Hue Shift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0, "GROUP": "Color" },
    { "NAME": "glow",     "LABEL": "Glow",          "TYPE": "float", "MIN": 0.2, "MAX": 2.0, "DEFAULT": 1.0, "GROUP": "Color" }
  ]
}*/

// FLUID3D / PRISM — the all-colorful take on the 3D Fluid: every particle
// carries its own hue, additive glow accumulation, black background always.
// Bass pumps and bursts, mids swirl the camera and cycle hue, highs sparkle
// the brightest particles. CORE audio bus only.

const int   N     = 90;
const float SEG_T = 5.0;

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
vec3  hash31(float n) {
    return fract(sin(vec3(n, n + 1.7, n + 2.9)) *
                 vec3(43758.5453, 22578.145, 19642.318));
}

vec3 hue2rgb(float h) {
    vec3 k = mod(vec3(5.0, 3.0, 1.0) + h * 6.0, 6.0);
    return 1.0 - clamp(min(k, 4.0 - k), 0.0, 1.0);
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
    float burst = 1.0 + 0.25 * bassP;

    // Slow hue drift always; mids push the wheel around faster.
    float hueBase = hueShift + t * 0.015 + 0.10 * midP;

    vec3 col = vec3(0.0);   // black background, always

    for (int i = 0; i < N; i++) {
        float id = float(i);
        if (hash11(id * 91.7) > density) continue;

        vec3 q = cam * (particlePos(id, t, wander) * burst) / scale;
        float zc = q.z + 3.2;
        if (zc < 0.4) continue;
        float persp = 1.9 / zc;
        vec2  sp    = q.xy * persp;

        float rad = (0.030 + 0.055 * hash11(id * 3.77)) * size * persp;
        rad *= 1.0 + 0.4 * bassP;

        float d  = length(uv - sp);
        float g  = exp(-(d * d) / (rad * rad));          // soft core
        float halo = exp(-(d * d) / (rad * rad * 9.0));  // wide faint halo
        if (g + halo < 0.002) continue;

        float hp  = hash11(id * 5.31);
        vec3  pc  = hue2rgb(fract(hueBase + hp));
        pc = mix(pc, vec3(1.0), 0.15 + 0.5 * highP * step(0.8, hp)); // sparkle whites
        float depthFade = clamp(1.6 / zc, 0.25, 1.0);

        col += pc * (g * 1.0 + halo * 0.22) * glow * depthFade * (0.7 + 0.6 * drive);
    }

    // Gentle tonemap keeps stacked glows out of clip while staying punchy.
    col = 1.0 - exp(-col * 1.4);
    gl_FragColor = vec4(col, 1.0);
}
