/*{
  "DESCRIPTION": "Color Frames — a bento poster of rounded panels on warm paper, each filled with a domain-warped iridescent liquid marble (candy pink/cyan/blue/green/yellow/orange). Solid accent panels carry black cut-out glyphs; a blue crosshair plate and a grainy dot round out the grid. Soft drop shadows, hairline ink outlines. Audio swells the flow and shifts the palette.",
  "CREDIT": "ShaderClaw — original liquid-bento composition",
  "CATEGORIES": [
    "Generator",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "ink",
      "LABEL": "Ink Pooling",
      "TYPE": "float",
      "DEFAULT": 0.45,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "outlineAmt",
      "LABEL": "Outline",
      "TYPE": "float",
      "DEFAULT": 0.9,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "shadowAmt",
      "LABEL": "Drop Shadow",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "grain",
      "LABEL": "Paper Grain",
      "TYPE": "float",
      "DEFAULT": 0.25,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "fieldAmount",
      "LABEL": "Surreal Field",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 1.5
    },
    {
      "NAME": "speckDensity",
      "LABEL": "Speck Swarm",
      "TYPE": "float",
      "DEFAULT": 0.65,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "inkBlobs",
      "LABEL": "Ink Blobs",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Flow Speed",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "warpAmount",
      "LABEL": "Marble Warp",
      "TYPE": "float",
      "DEFAULT": 0.65,
      "MIN": 0,
      "MAX": 1.5,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "marbleRate",
      "LABEL": "Marble Rate",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "dotOrbit",
      "LABEL": "Dot Orbit",
      "TYPE": "float",
      "MIN": -4,
      "MAX": 4,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "squiggleSpeed",
      "LABEL": "Squiggle Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "haloDrift",
      "LABEL": "Halo Drift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 8,
      "DEFAULT": 1,
      "GROUP": "Motion"
    },
    {
      "NAME": "paletteShift",
      "LABEL": "Palette Shift",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "accentColor",
      "LABEL": "Accent Orange",
      "TYPE": "color",
      "DEFAULT": [
        0.97,
        0.42,
        0.13,
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
      "NAME": "showPanels",
      "LABEL": "Bento Panels",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Paper Color",
      "TYPE": "color",
      "DEFAULT": [
        0.905,
        0.902,
        0.892,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ====================================================================
// Color Frames — liquid-marble bento poster.
//
// A virtual portrait canvas holds 13 hand-placed rounded panels:
//   • style 0 — iridescent liquid marble (domain-warped fbm → palette)
//   • style 1 — solid accent-orange plate with black cut-out shapes
//   • style 2 — flat blue plate with a crosshair + orbiting dot
//   • style 3 — grainy blue disc
//   • style 4 — tiny solid accent dot
//
// Panels cast soft down-right shadows on warm paper and carry a
// hairline ink outline. Audio swells the marble flow and nudges hue.
// ====================================================================

#define PI 3.14159265
#define NPANEL 13

float h12(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = h12(i);
    float b = h12(i + vec2(1.0, 0.0));
    float c = h12(i + vec2(0.0, 1.0));
    float d = h12(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.02 + vec2(11.3, 7.7);
        a *= 0.5;
    }
    return v;
}

// Signed distance to a rounded box centred at origin.
float sdRB(vec2 p, vec2 b, float r) {
    vec2 d = abs(p) - b + r;
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - r;
}

// Eight-stop cyclic "high-end" palette — luxe jewel tones with a thin-
// film iridescent shimmer ride on top. This is the heart of the marble.
vec3 PAL(float t) {
    vec3 S[8];
    S[0] = vec3(0.98, 0.42, 0.66);   // rose magenta
    S[1] = vec3(0.99, 0.62, 0.36);   // warm coral
    S[2] = vec3(0.99, 0.84, 0.42);   // champagne gold
    S[3] = vec3(0.36, 0.82, 0.66);   // emerald aqua
    S[4] = vec3(0.20, 0.66, 0.86);   // lagoon teal
    S[5] = vec3(0.34, 0.40, 0.92);   // electric indigo
    S[6] = vec3(0.62, 0.42, 0.95);   // violet
    S[7] = vec3(0.93, 0.50, 0.84);   // orchid

    float x = fract(t) * 8.0;
    int i = int(x);
    float f = x - float(i);
    f = f * f * f * (f * (f * 6.0 - 15.0) + 10.0);   // quintic — silkier ramps
    int j = i + 1;
    if (j > 7) j = 0;                                 // (i + 1) & 7 without bitwise ops
    vec3 Sa = S[0], Sb = S[0];
    for (int k = 0; k < 8; k++) {                     // constant-loop lookup (ES 1.0 safe)
        if (k == i) Sa = S[k];
        if (k == j) Sb = S[k];
    }
    vec3 col = mix(Sa, Sb, f);

    // Thin-film iridescence: a low-amplitude spectral shimmer that keeps
    // the gradients reading as a premium oil-slick rather than flat candy.
    vec3 irid = 0.5 + 0.5 * cos(6.2831853 * (t * vec3(1.0, 1.07, 1.13)
                                              + vec3(0.0, 0.33, 0.67)));
    col = mix(col, col * (0.78 + 0.5 * irid), 0.32);
    return clamp(col, 0.0, 1.0);
}

float hash21(vec2 p) { return fract(sin(dot(p, vec2(41.3, 289.1))) * 43758.5453); }
vec2  hash22(vec2 p) {
    return fract(sin(vec2(dot(p, vec2(127.1, 311.7)),
                          dot(p, vec2(269.5, 183.3)))) * 43758.5453);
}
float segDist(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h);
}

// ── Surreal scattered field ────────────────────────────────────────
// Painted onto the warm paper *behind* the bento panels: a glowing
// sun, drifting colour halos, gradient puddles, glossy black ink
// blobs, a twinkling speck swarm, wavy squiggles, lollipop antennas
// and little cones. fp.x runs 0..aspect, fp.y runs 0..1 (y up).
void applyField(inout vec3 col, vec2 p, float aspect, float t, float au) {
    float FA = fieldAmount;
    if (FA <= 0.001) return;

    // Sun — soft hot core with a warm bloom.
    {
        vec2 c = vec2(aspect * 0.46, 0.80) + 0.012 * vec2(sin(t * 0.3), cos(t * 0.2));
        float d = length(p - c);
        vec3 sun = mix(vec3(0.99, 0.86, 0.26), vec3(0.98, 0.52, 0.55),
                       smoothstep(0.0, 0.20, d));
        col += sun * smoothstep(0.20, 0.0, d) * (0.55 + 0.30 * au) * FA;
        col += vec3(0.99, 0.90, 0.55) * exp(-d * 7.0) * 0.40 * FA;
    }

    // Drifting colour halos.
    for (int i = 0; i < 5; i++) {
        float fi = float(i);
        vec2 c = vec2(hash21(vec2(fi, 1.0)) * aspect, hash21(vec2(fi, 2.0)));
        c += 0.018 * haloDrift * vec2(sin(t * 0.4 + fi), cos(t * 0.5 + fi));
        float d = length(p - c);
        float r = mix(0.06, 0.17, hash21(vec2(fi, 3.0)));
        col += PAL(hash21(vec2(fi, 4.0))) * exp(-d * d / (r * r)) * 0.32 * FA;
    }

    // Gradient puddles — big soft flat lakes (blue pool, green land).
    for (int i = 0; i < 2; i++) {
        vec2  c   = (i == 0) ? vec2(aspect * 0.52, 0.15) : vec2(aspect * 0.74, 0.56);
        vec2  rad = (i == 0) ? vec2(0.32, 0.10)          : vec2(0.21, 0.085);
        // Flowing marbled interior — domain-warped so the lakes churn.
        vec2 fp2 = p + 0.10 * vec2(fbm(p * 4.0 + t * 0.25),
                                   fbm(p.yx * 4.0 - t * 0.22));
        float d   = length((p - c) / rad);
        float m   = smoothstep(1.0, 0.55, d) * 0.82 * FA;
        float mb  = fbm(fp2 * 5.0 + (i == 0 ? t * 0.18 : -t * 0.16));
        vec3  pc  = (i == 0)
            ? PAL(0.55 + mb * 0.45 + 0.05 * sin(t * 0.3))
            : PAL(0.18 + mb * 0.40 - 0.05 * cos(t * 0.3));
        pc = mix(pc, vec3(1.0), smoothstep(0.72, 0.95, mb) * 0.35);
        pc += (vnoise(p * 120.0) - 0.5) * 0.12;
        col = mix(col, pc, m);
    }

    // Glossy black ink blobs — warped, anisotropic, specular dab.
    for (int i = 0; i < 8; i++) {
        float fi = float(i);
        vec2  c  = vec2(hash21(vec2(fi, 11.0)) * aspect, hash21(vec2(fi, 12.0)));
        float ang = hash21(vec2(fi, 13.0)) * 6.2831;
        float ca = cos(ang), sa = sin(ang);
        mat2  rot = mat2(ca, -sa, sa, ca);
        vec2  q  = rot * (p - c);
        vec2  scl = vec2(1.0, mix(0.5, 2.4, hash21(vec2(fi, 14.0))));
        float rb = mix(0.018, 0.055, hash21(vec2(fi, 15.0)));
        float warp = 0.20 * fbm(q * 9.0 + fi);
        float d = length(q / scl) - rb * (1.0 + warp);
        float fill = smoothstep(0.004, -0.004, d) * inkBlobs * FA;
        vec3 ink = vec3(0.04, 0.045, 0.06);
        vec2 hp = c + mat2(ca, sa, -sa, ca) * vec2(-rb * 0.35, rb * 0.5 * scl.y);
        ink += vec3(0.92) * smoothstep(rb * 0.5, 0.0, length(p - hp)) * 0.6;
        col = mix(col, ink, fill);
    }

    // Twinkling speck swarm — two jittered grid layers.
    float SD = speckDensity * FA;
    for (int L = 0; L < 2; L++) {
        float cell = (L == 0) ? 0.045 : 0.085;
        vec2 g = p / cell, id = floor(g), f = fract(g) - 0.5;
        vec2 rnd = hash22(id + float(L) * 7.0);
        float on = step(0.42, hash21(id + float(L) * 3.7 + 0.5));
        float d  = length(f - (rnd - 0.5) * 0.6) * cell;
        float sz = mix(0.0025, 0.0085, hash21(id + 1.7));
        float tw = clamp(0.55 + 0.45 * sin(t * 3.5 + hash21(id) * 40.0) * au, 0.0, 1.0);
        float m  = smoothstep(sz, sz * 0.35, d) * on * SD * tw;
        vec3 spc = (hash21(id + 9.1) < 0.22) ? vec3(0.05, 0.05, 0.07)
                                             : PAL(hash21(id + 0.3));
        col = mix(col, spc, m);
    }

    // Wavy squiggle filaments.
    for (int k = 0; k < 3; k++) {
        float fk = float(k);
        float cy = 0.22 + 0.27 * fk + 0.02 * sin(t * 0.3 + fk);
        float yl = cy + 0.035 * sin(p.x * (7.0 + fk * 3.0) + t * 0.8 * squiggleSpeed + fk * 2.0)
                      + 0.018 * fbm(p * 4.0 + fk * 5.0);
        float win = smoothstep(0.0, 0.12, p.x) * smoothstep(aspect, aspect - 0.12, p.x);
        float m = smoothstep(0.004, 0.0015, abs(p.y - yl)) * win * 0.9 * FA;
        col = mix(col, PAL(0.08 + 0.27 * fk + t * 0.02), m);
    }

    // Lollipop antennas — thin stalk, bright cap.
    for (int i = 0; i < 4; i++) {
        float fi = float(i);
        float x  = mix(0.06, aspect - 0.06, hash21(vec2(fi, 21.0)));
        float y0 = 0.10 + 0.5 * hash21(vec2(fi, 22.0));
        float y1 = y0 + mix(0.10, 0.26, hash21(vec2(fi, 23.0)));
        float ds = segDist(p, vec2(x, y0), vec2(x, y1));
        col = mix(col, vec3(0.10, 0.10, 0.12),
                  smoothstep(0.0018, 0.0006, ds) * FA);
        col = mix(col, PAL(hash21(vec2(fi, 24.0))),
                  smoothstep(0.012, 0.006, length(p - vec2(x, y1))) * FA);
    }

    // Little cones near the ground.
    for (int i = 0; i < 3; i++) {
        float fi = float(i);
        vec2 b = vec2(mix(0.10, aspect - 0.10, hash21(vec2(fi, 31.0))),
                      0.10 + 0.15 * hash21(vec2(fi, 32.0)));
        float w = mix(0.03, 0.06, hash21(vec2(fi, 33.0)));
        float h = mix(0.06, 0.13, hash21(vec2(fi, 34.0)));
        vec2  d = p - b;
        float tt = clamp(d.y / h, 0.0, 1.0);
        float inside = smoothstep(0.003, 0.0, abs(d.x) - w * (1.0 - tt))
                       * step(0.0, d.y) * step(d.y, h);
        col = mix(col, mix(PAL(hash21(vec2(fi, 35.0))), vec3(0.98, 0.9, 0.4), tt),
                  inside * 0.9 * FA);
    }
}

// Per-panel layout. luv-space x runs 0..0.80, y runs 0..1 (y down).
void panelData(int idx, out vec2 c, out vec2 b, out float r,
                out int style, out float seed) {
    if (idx == 0)  { c = vec2(0.165, 0.190); b = vec2(0.135, 0.140); r = 0.055; style = 0; seed = 1.0; }
    else if (idx == 1)  { c = vec2(0.420, 0.220); b = vec2(0.100, 0.180); r = 0.030; style = 2; seed = 2.0; }
    else if (idx == 2)  { c = vec2(0.665, 0.230); b = vec2(0.115, 0.170); r = 0.055; style = 0; seed = 3.0; }
    else if (idx == 3)  { c = vec2(0.155, 0.470); b = vec2(0.065, 0.135); r = 0.028; style = 1; seed = 10.0; }
    else if (idx == 4)  { c = vec2(0.278, 0.460); b = vec2(0.055, 0.135); r = 0.028; style = 1; seed = 11.0; }
    else if (idx == 5)  { c = vec2(0.430, 0.500); b = vec2(0.110, 0.170); r = 0.060; style = 0; seed = 4.0; }
    else if (idx == 6)  { c = vec2(0.675, 0.530); b = vec2(0.105, 0.135); r = 0.050; style = 0; seed = 5.0; }
    else if (idx == 7)  { c = vec2(0.205, 0.610); b = vec2(0.125, 0.055); r = 0.055; style = 0; seed = 6.0; }
    else if (idx == 8)  { c = vec2(0.175, 0.825); b = vec2(0.125, 0.125); r = 0.075; style = 0; seed = 7.0; }
    else if (idx == 9)  { c = vec2(0.420, 0.730); b = vec2(0.070, 0.070); r = 0.070; style = 3; seed = 8.0; }
    else if (idx == 10) { c = vec2(0.360, 0.865); b = vec2(0.018, 0.018); r = 0.018; style = 4; seed = 12.0; }
    else if (idx == 11) { c = vec2(0.480, 0.890); b = vec2(0.080, 0.045); r = 0.045; style = 0; seed = 9.0; }
    else                { c = vec2(0.665, 0.820); b = vec2(0.115, 0.140); r = 0.030; style = 1; seed = 13.0; }
}

// Iridescent liquid marble inside one panel. luv ∈ [0,1]², y down.
vec3 liquid(vec2 luv, float seed, float t, float warp, float pshift, float inkAmt) {
    t *= marbleRate;   // Marble Rate: churn clock of the liquid panels only
    vec2 p = luv * vec2(2.4, 3.0) + seed * 7.13;

    // Iterative curl-style flow advection — each pass folds the field
    // back through itself, so the marble keeps churning fluidly rather
    // than sliding rigidly. More iterations = deeper, glossier swirl.
    for (int k = 0; k < 4; k++) {
        float fk = float(k);
        vec2 flow = vec2(fbm(p * 1.30 + t * (0.22 + 0.05 * fk) + seed + fk),
                         fbm(p.yx * 1.30 - t * (0.18 + 0.04 * fk) + seed + 4.0 + fk));
        flow -= 0.5;
        p += warp * (0.65 + 0.35 * fk * 0.5) * flow;
        p += 0.06 * vec2(sin(p.y * 2.0 + t * 0.6), cos(p.x * 2.0 - t * 0.5));
    }

    float n = fbm(p + t * 0.07);
    float m = fbm(p * 1.9 - n * 1.6 + seed);
    float s = fbm(p * 3.3 + m * 2.0 - t * 0.12);   // fine ripple detail

    float h = n * 1.30 + m * 0.55 + s * 0.18 + pshift + luv.y * 0.22 + seed * 0.07;
    vec3 col = PAL(h);
    // Veined colour banding — second palette tap blended on the ridges.
    col = mix(col, PAL(h * 1.7 + 0.35), smoothstep(0.45, 0.85, s) * 0.35);

    // Specular pearl sheen where the secondary field crests.
    float sheen = smoothstep(0.60, 0.95, m) * (0.55 + 0.35 * s);
    col = mix(col, vec3(1.0, 0.99, 0.97), sheen * 0.70);
    col += vec3(0.30, 0.22, 0.40) * pow(sheen, 3.0) * 0.5;   // iridescent rim
    // Inky pooling where it sinks — the deep glossy bleed in the source art.
    col = mix(col, vec3(0.025, 0.03, 0.05), smoothstep(0.26, 0.03, m) * inkAmt);
    return clamp(col, 0.0, 1.2);
}

// Solid accent plate carrying black cut-out shapes. luv y down.
vec3 glyphPlate(vec2 luv, float seed, vec3 accent) {
    vec3 col = accent;
    vec3 K = vec3(0.04, 0.045, 0.06);
    if (seed > 12.5) {
        // "Device" — big black inset window with little detail dashes.
        float win = sdRB(luv - vec2(0.50, 0.56), vec2(0.34, 0.34), 0.05);
        col = mix(col, K, smoothstep(0.012, 0.0, win));
        for (int i = 0; i < 3; i++) {
            float fy = 0.34 + float(i) * 0.07;
            float ln = abs(luv.x - 0.34) + abs(luv.y - fy) * 6.0;
            col = mix(col, accent, smoothstep(0.05, 0.0, ln - 0.02) * step(luv.y, 0.52) * step(0.30, luv.y));
        }
        float knob = length((luv - vec2(0.50, 0.84)) * vec2(1.0, 1.4));
        col = mix(col, accent, smoothstep(0.05, 0.04, knob));
    } else if (seed > 10.5) {
        // Numeral-ish black mark.
        float stem = sdRB(luv - vec2(0.56, 0.52), vec2(0.10, 0.34), 0.04);
        float foot = sdRB(luv - vec2(0.50, 0.84), vec2(0.26, 0.07), 0.03);
        float serif = sdRB((luv - vec2(0.40, 0.22)) * mat2(0.92, -0.39, 0.39, 0.92),
                           vec2(0.05, 0.16), 0.03);
        float g = min(min(stem, foot), serif);
        col = mix(col, K, smoothstep(0.012, 0.0, g));
    } else {
        // Two vertical slot bars near the top.
        float s1 = sdRB(luv - vec2(0.38, 0.26), vec2(0.07, 0.16), 0.025);
        float s2 = sdRB(luv - vec2(0.62, 0.26), vec2(0.07, 0.16), 0.025);
        float hook = sdRB((luv - vec2(0.50, 0.74)) * mat2(0.87, -0.5, 0.5, 0.87),
                          vec2(0.30, 0.10), 0.06);
        float g = min(min(s1, s2), hook);
        col = mix(col, K, smoothstep(0.012, 0.0, g));
    }
    return col;
}

void main() {
    vec2 R = RENDERSIZE;
    float audio = clamp(audioReact, 0.0, 2.0);
    float t = TIME * (speed + audioBass * audio * 0.30) + audioLevel * audio * 0.4;
    float warp = warpAmount + audioMid * audio * 0.40;
    float pshift = paletteShift + audioHigh * audio * 0.15;

    // Centred, y-down coords. Virtual canvas: x∈[0,0.80], y∈[0,1].
    vec2 q = (gl_FragCoord.xy - 0.5 * R) / R.y;
    q.y = -q.y;
    vec2 v = q / 1.02 + vec2(0.40, 0.5);

    // ── Warm paper with faint grain ──
    vec3 col = bgColor.rgb;
    float g = vnoise(gl_FragCoord.xy * 1.7) - 0.5;
    col += g * grain * 0.05;

    // ── Surreal scattered field (behind the panels) ──
    float aspect = R.x / R.y;
    vec2 fp = gl_FragCoord.xy / R.y;          // x: 0..aspect, y: 0..1
    applyField(col, fp, aspect, t, clamp(audioLevel * audio, 0.0, 1.0));

    // ── Soft drop shadows (one pass, behind every panel) ──
    vec2 shOff = vec2(0.007, 0.012);
    float shadow = 0.0;
    for (int i = 0; i < NPANEL; i++) {
        vec2 c, b; float r, seed; int style;
        panelData(i, c, b, r, style, seed);
        float sd = sdRB((v - shOff) - c, b, r);
        shadow = max(shadow, smoothstep(0.028, 0.0, sd));
    }
    col = mix(col, col * 0.72, shadow * shadowAmt * showPanels);

    // ── Panels (front-most wins; the layout barely overlaps) ──
    float aa = max(fwidth(v.x + v.y), 1e-4) * 1.4;
    for (int i = 0; i < NPANEL; i++) {
        vec2 c, b; float r, seed; int style;
        panelData(i, c, b, r, style, seed);
        vec2 lp = v - c;
        float sd = sdRB(lp, b, r);
        float cov = smoothstep(aa, -aa, sd) * showPanels;
        if (cov <= 0.0) continue;

        vec2 luv = (lp + b) / (2.0 * b);   // 0..1 within panel, y down
        vec3 pc;
        if (style == 0) {
            pc = liquid(luv, seed, t, warp, pshift, ink);
        } else if (style == 1) {
            pc = glyphPlate(luv, seed, accentColor.rgb);
        } else if (style == 2) {
            pc = vec3(0.15, 0.42, 0.92);
            // crosshair
            float cx = smoothstep(0.010, 0.004, abs(luv.x - 0.5));
            float cy = smoothstep(0.010, 0.004, abs(luv.y - 0.5));
            float ring = abs(length(luv - 0.5) - 0.30);
            pc = mix(pc, vec3(0.06, 0.16, 0.45), max(cx, cy) * smoothstep(0.42, 0.0, length(luv - 0.5)));
            pc = mix(pc, vec3(0.06, 0.16, 0.45), smoothstep(0.012, 0.0, ring) * 0.4);
            // orbiting accent dot + a tilted paddle
            vec2 dotP = vec2(0.5 + 0.30 * cos(t * 0.6 * dotOrbit), 0.5 + 0.22 * sin(t * 0.6 * dotOrbit));
            pc = mix(pc, accentColor.rgb, smoothstep(0.045, 0.030, length((luv - dotP) * vec2(1.0, 1.0))));
            float pad = sdRB((luv - vec2(0.5)) * mat2(0.7, -0.7, 0.7, 0.7), vec2(0.035, 0.20), 0.035);
            pc = mix(pc, vec3(0.92, 0.94, 0.98), smoothstep(0.012, 0.0, pad) * 0.85);
        } else if (style == 3) {
            float gg = vnoise(luv * 90.0);
            pc = vec3(0.18, 0.44, 0.88) + (gg - 0.5) * 0.22;
            pc = mix(pc, vec3(0.10, 0.28, 0.62), smoothstep(0.30, 0.5, length(luv - 0.5)));
        } else {
            pc = accentColor.rgb;
        }

        // Hairline ink outline.
        float ol = smoothstep(aa, -aa, abs(sd) - 0.0024);
        pc = mix(pc, vec3(0.05, 0.055, 0.07), ol * outlineAmt);

        col = mix(col, pc, cov);
    }

    col = clamp(col, 0.0, 1.0);

    // ---- universal color block (defaults = no-op; bg via native Paper Color) ----
    vec3 uc = col;
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

    gl_FragColor = vec4(col, 1.0);
}
