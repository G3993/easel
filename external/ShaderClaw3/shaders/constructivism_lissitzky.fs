/*{
  "CATEGORIES": [
    "Generator",
    "Art Movement",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Constructivism (El Lissitzky / Rodchenko) — a bold animated Suprematist composition: a red disc, heavy black diagonal beams and a thrusting wedge, fine line accents and a small jewel-accent square on a warm paper field. Crisp anti-aliased edges, drop-shadow depth, paper grain and vignette for a premium flat-graphic look. Bass pulses the disc; mid drives the diagonal thrust; the whole composition breathes with a slow rotation. Single-pass, LINEAR-ish out.",
  "INPUTS": [
    {
      "NAME": "grainAmt",
      "LABEL": "Grain",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.2,
      "DEFAULT": 0.06
    },
    {
      "NAME": "discScale",
      "LABEL": "Disc Size",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 0.8,
      "DEFAULT": 0.46,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "tempo",
      "LABEL": "Tempo",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.7,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "compRotate",
      "LABEL": "Composition Tilt",
      "TYPE": "float",
      "MIN": -0.6,
      "MAX": 0.6,
      "DEFAULT": 0.12,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "redCol",
      "LABEL": "Red",
      "TYPE": "color",
      "DEFAULT": [
        0.86,
        0.1,
        0.09,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "inkCol",
      "LABEL": "Ink",
      "TYPE": "color",
      "DEFAULT": [
        0.06,
        0.06,
        0.07,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "accentCol",
      "LABEL": "Accent",
      "TYPE": "color",
      "DEFAULT": [
        0.1,
        0.32,
        0.78,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "LABEL": "Hue Shift",
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
      "NAME": "paperCol",
      "LABEL": "Paper",
      "TYPE": "color",
      "DEFAULT": [
        0.93,
        0.9,
        0.83,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// Constructivism — flat-graphic Suprematist composition, fully 2D, crisp.

mat2 rot(float a){ float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }

float hash21(vec2 p){ p = fract(p * vec2(127.1, 311.7)); p += dot(p, p + 34.5); return fract(p.x * p.y); }

// 2D SDFs.
float sdBox(vec2 p, vec2 b){ vec2 d = abs(p) - b; return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0); }
float sdCircle(vec2 p, float r){ return length(p) - r; }
float sdSegment(vec2 p, vec2 a, vec2 b, float r){
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h) - r;
}
float sdTriangle(vec2 p, vec2 p0, vec2 p1, vec2 p2){
    vec2 e0 = p1 - p0, e1 = p2 - p1, e2 = p0 - p2;
    vec2 v0 = p - p0, v1 = p - p1, v2 = p - p2;
    vec2 pq0 = v0 - e0 * clamp(dot(v0, e0) / dot(e0, e0), 0.0, 1.0);
    vec2 pq1 = v1 - e1 * clamp(dot(v1, e1) / dot(e1, e1), 0.0, 1.0);
    vec2 pq2 = v2 - e2 * clamp(dot(v2, e2) / dot(e2, e2), 0.0, 1.0);
    float s = sign(e0.x * e2.y - e0.y * e2.x);
    vec2 d = min(min(vec2(dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
                     vec2(dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
                     vec2(dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)));
    return -sqrt(d.x) * sign(d.y);
}

// Crisp anti-aliased fill of an SDF over `under`.
vec3 fillSDF(vec3 under, float sd, vec3 c){
    float aa = max(fwidth(sd), 1e-4);
    return mix(under, c, 1.0 - smoothstep(-aa, aa, sd));
}
// Soft offset drop-shadow contribution (premium depth).
vec3 shadowSDF(vec3 under, float sd, float amt){
    float m = exp(-max(sd, 0.0) * 26.0) * amt;   // soft falloff outside the shape
    return mix(under, under * 0.55, clamp(m, 0.0, 1.0));
}

void main(){
    vec2 uv = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y;

    float bass = clamp(audioBass * audioReact, 0.0, 1.5);
    float mid  = clamp(audioMid  * audioReact, 0.0, 1.5);
    float t    = TIME * (0.3 + tempo);

    // Whole composition breathes with a slow tilt.
    vec2 p = rot(compRotate + 0.04 * sin(t * 0.4)) * uv;

    // ── Paper field: warm base + faint diagonal tone split + grain. ──
    vec3 paper = paperCol.rgb;
    float diag = dot(uv, normalize(vec2(0.9, 0.6)));
    vec3 col = mix(paper, paper * 0.93, smoothstep(-0.1, 0.5, diag));
    col = mix(col, paper * 1.03, smoothstep(0.2, -0.4, diag) * 0.5);

    vec3 ink = inkCol.rgb, red = redCol.rgb, acc = accentCol.rgb;

    // ── Drop shadows first (so shapes sit above them) ──
    float discR = discScale * (1.0 + 0.16 * bass);
    vec2  discC = vec2(-0.28, 0.10) + 0.02 * vec2(sin(t * 0.5), cos(t * 0.43));
    col = shadowSDF(col, sdCircle(p - discC - vec2(0.03, -0.03), discR), 0.9);

    // Heavy diagonal beam (slides along its axis).
    float beamAng = 0.9 + 0.12 * sin(t * 0.5 + mid) + 0.10 * mid;
    vec2  bp = rot(-beamAng) * (p - vec2(0.18, -0.05));
    float slide = 0.16 * sin(t * 0.6);
    float beam = sdBox(bp - vec2(slide, 0.0), vec2(0.95, 0.075));
    col = shadowSDF(col, sdBox(rot(-beamAng) * (p - vec2(0.18, -0.05)) - vec2(slide + 0.03, -0.03), vec2(0.95, 0.075)), 0.85);

    // Thrusting black wedge (swings with mid).
    float sw = 0.18 * sin(t * 0.7) + 0.30 * mid;
    vec2 wc = vec2(0.34, 0.30);
    vec2 t0 = wc + rot(sw) * vec2(-0.05, -0.34);
    vec2 t1 = wc + rot(sw) * vec2(-0.05,  0.34);
    vec2 t2 = wc + rot(sw) * vec2( 0.66,  0.0);
    col = shadowSDF(col, sdTriangle(p - vec2(0.03, -0.03), t0, t1, t2), 0.8);

    // ── Shapes (crisp, hard-edged) ──
    // Red disc.
    col = fillSDF(col, sdCircle(p - discC, discR), red);
    // A thin ink ring riding the disc edge (graphic detail).
    float ring = abs(sdCircle(p - discC, discR * 1.06)) - 0.006;
    col = fillSDF(col, ring, ink);

    // Black diagonal beam.
    col = fillSDF(col, beam, ink);
    // A second, thinner crossing beam.
    vec2 bp2 = rot(0.35) * (p - vec2(-0.1, 0.32));
    col = fillSDF(col, sdBox(bp2 - vec2(0.1 * sin(t * 0.5 + 1.7), 0.0), vec2(0.7, 0.022)), ink);

    // Black wedge.
    col = fillSDF(col, sdTriangle(p, t0, t1, t2), ink);

    // Fine parallel line accents (Rodchenko ruling).
    for (int i = 0; i < 4; i++){
        float fi = float(i);
        vec2 a = rot(-0.5) * vec2(-1.2, -0.36 + 0.12 * fi);
        vec2 b = rot(-0.5) * vec2( 1.2, -0.36 + 0.12 * fi);
        col = fillSDF(col, sdSegment(p + vec2(0.0, 0.02 * sin(t * 0.4 + fi)), a, b, 0.004), ink * 1.4);
    }

    // Jewel accent square (small, off in a corner, gentle spin).
    vec2 ac = vec2(0.46, -0.30);
    col = shadowSDF(col, sdBox(rot(0.6) * (p - ac - vec2(0.02, -0.02)), vec2(0.07)), 0.7);
    col = fillSDF(col, sdBox(rot(0.6 + 0.3 * sin(t * 0.5)) * (p - ac), vec2(0.07)), acc);

    // White circle outline accent (Suprematist counterpoint).
    float wring = abs(sdCircle(p - vec2(0.04, -0.32), 0.12)) - 0.007;
    col = fillSDF(col, wring, vec3(0.97));

    // ── Finish: paper grain + soft vignette. ──
    float g = hash21(gl_FragCoord.xy + floor(TIME * 24.0));
    col += (g - 0.5) * grainAmt;
    float vig = 1.0 - smoothstep(0.55, 1.25, length(uv));
    col *= mix(0.82, 1.0, vig);

    // ---- universal color block (defaults = no-op; bg via native Paper color) ----
    vec3 uc = max(col, 0.0);
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                     // saturation
    if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        uc = clamp(hM * uc, 0.0, 1.0);
    }
    col = uc;

    gl_FragColor = vec4(max(col, 0.0), 1.0);
}
