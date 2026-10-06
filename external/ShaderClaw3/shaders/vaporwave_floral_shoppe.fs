/*{
  "CATEGORIES": [
    "Generator",
    "Vaporwave",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Macintosh Plus — Floral Shoppe (2011). Hot-pink/magenta/indigo sunset with twin sun-discs (one slumped into its plaza-tile reflection), Tron-era cyan/magenta perspective grid scrolling toward camera, and a swarm of drifting Y2K junk: chrome stars, wireframe cubes, holo-CDs, sparkle dust, dolphin silhouettes, floppy disks, globe outlines, lens flares. Optional fake-katakana glyphs. VHS scanlines, RGB chromatic aberration, treble-triggered tear-glitches. Stays alive in silence. Single-pass, LINEAR HDR.",
  "INPUTS": [
    {
      "NAME": "y2kCount",
      "LABEL": "Y2K Junk Count",
      "TYPE": "long",
      "DEFAULT": 16,
      "VALUES": [
        8,
        12,
        16,
        20,
        24
      ],
      "LABELS": [
        "8",
        "12",
        "16",
        "20",
        "24"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "gridDensity",
      "LABEL": "Grid Density",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 24,
      "DEFAULT": 10,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "sunSize",
      "LABEL": "Sun Size",
      "TYPE": "float",
      "MIN": 0.06,
      "MAX": 0.3,
      "DEFAULT": 0.16,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "katakanaCount",
      "LABEL": "Katakana Glyphs",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        4,
        6,
        9,
        12,
        16
      ],
      "LABELS": [
        "Off",
        "4",
        "6",
        "9",
        "12",
        "16"
      ],
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "gridSpeed",
      "LABEL": "Grid Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.45,
      "GROUP": "Motion / Animation"
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
      "NAME": "horizonY",
      "LABEL": "Horizon",
      "TYPE": "float",
      "MIN": 0.4,
      "MAX": 0.7,
      "DEFAULT": 0.55,
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "LABEL": "Background",
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
    },
    {
      "NAME": "scanlineAmp",
      "LABEL": "Scanlines",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.5,
      "DEFAULT": 0.22
    },
    {
      "NAME": "chromaShift",
      "LABEL": "Chroma Aberration",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.02,
      "DEFAULT": 0.006
    },
    {
      "NAME": "tearAmount",
      "LABEL": "VHS Tear",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55
    }
  ]
}*/

// ════════════════════════════════════════════════════════════════════════
//  Macintosh Plus — Floral Shoppe. Six-colour discipline: hot pink,
//  magenta, indigo, cyan, marble cream, black. LINEAR HDR.
// ════════════════════════════════════════════════════════════════════════

const vec3 PINK   = vec3(1.00, 0.40, 0.78);
const vec3 MAGEN  = vec3(0.78, 0.20, 0.78);
const vec3 INDIGO = vec3(0.18, 0.10, 0.55);
const vec3 CYAN   = vec3(0.20, 0.92, 0.98);
const vec3 CREAM  = vec3(0.92, 0.88, 0.82);
const vec3 BLACK  = vec3(0.00, 0.00, 0.00);

float hash11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float hash21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

float sdBox(vec2 p, vec2 b) {
    vec2 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}
float sdSeg(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h);
}

mat2 rot2(float a) { float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }

// 5-pointed star SDF (signed-ish; negative inside, positive outside).
float sdStar5(vec2 p, float r) {
    const float k1x = 0.809016994; // cos(pi/5)
    const float k1y = 0.587785252; // sin(pi/5)
    const float k2x = 0.309016994; // cos(2pi/5)
    const float k2y = 0.951056516; // sin(2pi/5)
    p.x = abs(p.x);
    p -= 2.0 * max(dot(vec2(-k1x, k1y), p), 0.0) * vec2(-k1x, k1y);
    p -= 2.0 * max(dot(vec2( k2x, k2y), p), 0.0) * vec2( k2x, k2y);
    p.x = abs(p.x);
    p.y -= r;
    vec2 ba = vec2(k2y, -k2x) * 0.5; // inner radius factor 0.5
    float h = clamp(dot(p, ba) / dot(ba, ba), 0.0, r);
    return length(p - ba * h) * sign(p.y * ba.x - p.x * ba.y);
}

// Wireframe cube outline (axonometric). Returns line-mask 0..1.
float cubeWire(vec2 p, float r, float t) {
    p /= r;
    // Build 8 vertices of a unit cube, project with simple iso rotation.
    float ca = cos(t * 0.4), sa = sin(t * 0.4);
    float cb = cos(0.6), sb = sin(0.6);
    float lw = 0.06;
    float m = 1e9;
    // Edges: pairs of corner indices (0..7) along x/y/z directions.
    for (int i = 0; i < 12; i++) {
        // Encode cube edges procedurally.
        int a, b;
        if (i < 4) { a = i; b = i + 4; }                              // vertical (z)
        else if (i < 8) { int j = i - 4; a = j * 2; b = j * 2 + 1; }  // along x within faces
        else { int j = i - 8;
               int base = (j < 2) ? 0 : 4;
               a = base + (j - (j / 2) * 2); b = a + 2; }             // along y
        // Bit extraction via float mod (GLSL ES 1.0: no bitwise ops)
        float fa = float(a), fb = float(b);
        vec3 va = vec3(mod(fa, 2.0) - 0.5,
                       mod(floor(fa / 2.0), 2.0) - 0.5,
                       mod(floor(fa / 4.0), 2.0) - 0.5);
        vec3 vb = vec3(mod(fb, 2.0) - 0.5,
                       mod(floor(fb / 2.0), 2.0) - 0.5,
                       mod(floor(fb / 4.0), 2.0) - 0.5);
        // Rotate around Y then X, project orthographic.
        va.xz = mat2(ca, -sa, sa, ca) * va.xz;
        vb.xz = mat2(ca, -sa, sa, ca) * vb.xz;
        va.yz = mat2(cb, -sb, sb, cb) * va.yz;
        vb.yz = mat2(cb, -sb, sb, cb) * vb.yz;
        m = min(m, sdSeg(p, va.xy, vb.xy));
    }
    return smoothstep(lw, lw * 0.4, m);
}

// CD / disc: ring with hole, plus rainbow-sheen via angle.
vec3 cdDisc(vec2 p, float r, float t, out float alpha) {
    alpha = 0.0;
    float d = length(p);
    if (d > r * 1.05) return vec3(0.0);
    float outer = smoothstep(r, r * 0.96, d);
    float inner = smoothstep(r * 0.18, r * 0.22, d);
    float ring  = outer * inner;
    if (ring <= 0.0) {
        alpha = smoothstep(r * 0.10, r * 0.14, d) * (1.0 - inner) * 0.6;
        return CREAM * 0.4 * alpha;
    }
    float ang = atan(p.y, p.x) + t * 0.7;
    // 3-band rainbow refraction: cycle PINK→CYAN→CREAM by angle.
    float w = fract(ang / 6.28318 * 3.0);
    vec3 sheen = (w < 0.333) ? mix(PINK, CYAN,  w * 3.0)
              : (w < 0.666) ? mix(CYAN, CREAM, (w - 0.333) * 3.0)
                            : mix(CREAM, PINK, (w - 0.666) * 3.0);
    float spec = pow(0.5 + 0.5 * cos(ang * 5.0), 4.0);
    vec3 col = mix(sheen * 0.8, CREAM, spec * 0.7);
    alpha = ring;
    return col * ring;
}

// Sparkle cluster: 5 small star-points.
float sparkleCluster(vec2 p, float r, float seed, float t) {
    float a = 0.0;
    for (int i = 0; i < 5; i++) {
        float fi = float(i);
        vec2 c = (vec2(hash11(seed + fi * 1.3), hash11(seed + fi * 2.7)) - 0.5) * r * 1.4;
        vec2 q = p - c;
        float pulse = 0.5 + 0.5 * sin(t * 4.0 + fi * 1.7 + seed * 6.28);
        float sz = r * (0.10 + 0.10 * pulse);
        // Cross-spike sparkle: two thin rotated boxes.
        float d1 = sdBox(rot2(0.785) * q, vec2(sz, sz * 0.10));
        float d2 = sdBox(rot2(-0.785) * q, vec2(sz, sz * 0.10));
        float d  = min(d1, d2);
        a = max(a, smoothstep(sz * 0.4, 0.0, d) * pulse);
    }
    return a;
}

// Holographic dolphin silhouette: leaping arc + body + tail-fluke.
float sdDolphin(vec2 p) {
    // Body: tilted ellipse (squashed along arc).
    vec2 q = rot2(-0.35) * p;
    float body = length(q * vec2(0.55, 1.4)) - 0.35;
    // Snout taper.
    float snout = sdSeg(p, vec2(0.30, 0.05), vec2(0.50, -0.05)) - 0.06;
    // Tail fluke.
    float tail  = sdSeg(p, vec2(-0.30, -0.05), vec2(-0.55, 0.18)) - 0.05;
    float flukeL = sdSeg(p, vec2(-0.55, 0.18), vec2(-0.62, 0.32)) - 0.03;
    float flukeR = sdSeg(p, vec2(-0.55, 0.18), vec2(-0.40, 0.30)) - 0.03;
    // Dorsal fin.
    float dors  = sdSeg(p, vec2(0.02, 0.18), vec2(-0.10, 0.36)) - 0.03;
    float d = body;
    d = min(d, snout);
    d = min(d, tail);
    d = min(d, flukeL);
    d = min(d, flukeR);
    d = min(d, dors);
    return d;
}

// Floppy disk: rounded rect + metal shutter slot + label.
float sdFloppy(vec2 p) {
    float body = sdBox(p, vec2(0.42, 0.42)) - 0.04;
    // Notch at top-right corner.
    float notch = sdBox(p - vec2(0.30, 0.36), vec2(0.10, 0.08));
    body = max(body, -notch);
    // Shutter (metal slider) at top.
    float shutter = sdBox(p - vec2(-0.05, 0.28), vec2(0.20, 0.10));
    // Label rectangle (lower).
    float label = sdBox(p - vec2(0.0, -0.10), vec2(0.30, 0.18));
    return min(min(body, shutter), label);
}

// Globe: circle outline + 3 latitude arcs + 1 meridian.
float globeWire(vec2 p, float r, float t) {
    float lw = r * 0.05;
    float d = abs(length(p) - r);                 // equator-circle outline
    // Latitudes: horizontal ellipses of varying widths.
    for (int i = 0; i < 3; i++) {
        float fi = float(i) - 1.0;
        float yy = fi * r * 0.45;
        float ww = sqrt(max(r * r - yy * yy, 1e-4));
        float dd = abs(length(vec2(p.x, (p.y - yy) * 6.0)) - ww);
        d = min(d, dd);
    }
    // Spinning meridian: vertical ellipse whose width oscillates.
    float wm = abs(cos(t * 0.6)) * r;
    if (wm > 0.02) {
        float dm = abs(length(vec2(p.x / max(wm / r, 0.05), p.y)) - r);
        d = min(d, dm);
    }
    return smoothstep(lw, lw * 0.3, d);
}

// Lens flare: bright core + soft halo + 4 thin spokes.
vec3 lensFlare(vec2 p, float r, float t) {
    float d  = length(p);
    float core = smoothstep(r * 0.20, 0.0, d);
    float halo = smoothstep(r * 1.4, r * 0.3, d) * 0.5;
    // 4 spokes (rotated 0/45/90/135).
    float spokes = 0.0;
    for (int i = 0; i < 4; i++) {
        float a = float(i) * 0.7853981 + t * 0.3;
        vec2 q = rot2(a) * p;
        spokes = max(spokes, smoothstep(r * 1.6, 0.0, abs(q.y)) *
                              smoothstep(r * 1.6, 0.0, abs(q.x) - r * 0.1) * 0.4);
    }
    vec3 col = CREAM * (core + halo) + PINK * spokes * 0.6 + CYAN * spokes * 0.3;
    return col;
}

// Y2K junk swarm: kinds 0..7 dispatched per element.
//   0: chrome star    1: wire cube   2: CD disc   3: sparkle cluster
//   4: dolphin        5: floppy      6: globe     7: lens flare
vec3 y2kSwarm(vec2 uv, float aspect, int count, float audio, float t) {
    if (count <= 0) return vec3(0.0);
    vec3 acc = vec3(0.0);
    for (int i = 0; i < 24; i++) {
        if (i >= count) break;
        float fi = float(i);

        // Per-element parameters.
        float seed = hash11(fi * 9.31 + 0.17);
        float kindF = hash11(fi * 13.7);
        int   kind  = int(kindF * 8.0);
        if (kind > 7) kind = 7;

        // Drift across canvas. Mostly horizontal, gentle vertical bob.
        float dir   = (hash11(fi * 3.7) > 0.5) ? 1.0 : -1.0;
        float spd   = 0.020 + hash11(fi * 17.1) * 0.060;
        float yBase = 0.10 + hash11(fi * 5.3) * 0.78;
        float bob   = 0.04 * sin(t * (0.4 + hash11(fi * 23.0) * 0.6) + seed * 6.28);
        float xPos  = fract(hash11(fi * 11.7) + dir * t * spd);
        vec2  ctr   = vec2(xPos, yBase + bob);

        // Scale and per-frame spin.
        float sz   = 0.035 + hash11(fi * 23.1) * 0.080;
        sz *= (1.0 + audio * 0.10);
        float spin = (hash11(fi * 31.7) - 0.5) * 4.0 * t + seed * 6.28;

        // Local frame (aspect-corrected).
        vec2 d = (uv - ctr) * vec2(aspect, 1.0);
        // Cheap reject.
        if (max(abs(d.x), abs(d.y)) > sz * 1.3) continue;

        vec2 q = rot2(spin) * d;

        if (kind == 0) {
            // Chrome star
            float dd = sdStar5(q, sz);
            float a  = smoothstep(sz * 0.04, -sz * 0.04, dd);
            float rim = smoothstep(sz * 0.08, sz * 0.0, abs(dd));
            vec3 chrome = mix(CREAM, CYAN, 0.5 + 0.5 * sin(spin * 2.0));
            chrome = mix(chrome, PINK, 0.3 + 0.3 * sin(spin * 3.0 + 1.7));
            acc = max(acc, chrome * a + CREAM * rim * 0.5);
        } else if (kind == 1) {
            // Wireframe cube
            float w = cubeWire(q, sz, t + seed * 6.28);
            vec3 wc = mix(CYAN, PINK, 0.5 + 0.5 * sin(t + seed * 6.28));
            acc = max(acc, wc * w);
        } else if (kind == 2) {
            // CD disc
            float a;
            vec3 cdC = cdDisc(q, sz, t + seed * 6.28, a);
            acc = max(acc, cdC);
        } else if (kind == 3) {
            // Sparkle cluster
            float a = sparkleCluster(d, sz, seed * 17.0, t);
            acc = max(acc, CREAM * a + PINK * a * 0.3);
        } else if (kind == 4) {
            // Holographic dolphin
            vec2 dq = q / sz;
            // Random horizontal flip per element.
            if (hash11(fi * 41.3) > 0.5) dq.x = -dq.x;
            float dd = sdDolphin(dq);
            float a  = smoothstep(0.04, -0.02, dd);
            // Holographic gradient by local Y.
            vec3 holo = mix(CYAN, PINK, 0.5 + 0.5 * sin(dq.x * 4.0 + t * 2.0 + seed * 6.28));
            holo = mix(holo, CREAM, 0.3);
            acc = max(acc, holo * a);
        } else if (kind == 5) {
            // Floppy disk
            vec2 dq = q / sz;
            float dd = sdFloppy(dq);
            float a  = smoothstep(0.03, -0.02, dd);
            // Body magenta-ish, label cream — pick by local position.
            vec3 fc = (dq.y < -0.04 && abs(dq.x) < 0.30) ? CREAM
                    : (abs(dq.y - 0.28) < 0.10 && abs(dq.x + 0.05) < 0.20) ? CYAN * 0.85
                    : MAGEN * 0.95;
            acc = max(acc, fc * a);
        } else if (kind == 6) {
            // Globe wireframe
            float g = globeWire(q, sz, t + seed * 6.28);
            vec3 gc = mix(CYAN, CREAM, 0.4);
            acc = max(acc, gc * g);
        } else {
            // Lens flare
            vec3 lf = lensFlare(d, sz, t + seed * 6.28);
            acc += lf * 0.6;
        }
    }
    return acc;
}

// Twin suns: top sun + squashed reflection with horizontal bars.
vec3 twinSun(vec2 uv, float aspect, float horizon, float sz, float audio, float t) {
    vec3 acc = vec3(0.0);
    float r = sz * (1.0 + audio * 0.12 + 0.04 * sin(t * 1.7));

    vec2 dT = (uv - vec2(0.5, horizon + sz * 0.55)) * vec2(aspect, 1.0);
    float lT = length(dT);
    if (lT < r * 1.6) {
        vec3 sunC = mix(PINK * 1.05, MAGEN, clamp(dT.y / r * 0.5 + 0.5, 0.0, 1.0));
        acc += sunC * smoothstep(r, r * 0.92, lT)
             + PINK * smoothstep(r * 1.6, r, lT) * 0.35;
    }

    vec2 dR = (uv - vec2(0.5, horizon - sz * 0.40)) * vec2(aspect, 2.2);
    float lR = length(dR);
    if (lR < r * 1.4) {
        vec3 sunC = mix(MAGEN, PINK * 0.9, clamp(dR.y / r * 0.5 + 0.5, 0.0, 1.0));
        float bar = step(0.0, sin((uv.y - (horizon - sz * 0.40)) * 80.0 + t * 0.6));
        sunC = mix(BLACK, sunC, bar);
        acc += sunC * smoothstep(r, r * 0.92, lR) * 0.85;
    }
    return acc;
}

// Tron-era perspective floor — inverse-perspective by distance below
// horizon, scrolls toward camera. Cyan grid + magenta boost lines.
vec3 gridFloor(vec2 uv, float horizon, float density, float speed, float audio, float t) {
    float dh    = max(horizon - uv.y, 1e-4);
    float depth = 1.0 / dh;
    vec2  g     = vec2((uv.x - 0.5) * depth * 0.6,
                       depth - t * speed * (1.0 + audio * 0.5));
    float gx    = abs(fract(g.x * density) - 0.5);
    float gy    = abs(fract(g.y) - 0.5);
    float lw    = 0.020 * dh * 6.0;
    float line  = max(smoothstep(lw, 0.0, gx), smoothstep(lw, 0.0, gy));

    vec3 base = mix(INDIGO * 0.5, MAGEN * 0.5, clamp(uv.y / horizon, 0.0, 1.0));
    vec3 col  = mix(base, CYAN, line * 0.95);
    float boost = step(0.5, fract(g.y * 0.25)) * smoothstep(lw, 0.0, gy);
    col = mix(col, PINK, boost * 0.45);
    col = mix(col, MAGEN * 0.6, smoothstep(horizon - 0.04, horizon, uv.y) * 0.7);
    return col;
}

// Fake katakana glyph — composite of 4 random horizontal/vertical bars.
float sdKatakana(vec2 p, float seed) {
    float d = 1e9;
    for (int i = 0; i < 4; i++) {
        float fi = float(i);
        float h1 = hash11(seed * 31.0 + fi * 7.0);
        float h2 = hash11(seed * 53.0 + fi * 11.0);
        float h3 = hash11(seed * 71.0 + fi * 13.0);
        float h4 = hash11(seed * 91.0 + fi * 17.0);
        vec2  c  = (vec2(h1, h2) - 0.5) * 0.8;
        vec2  b  = (h3 > 0.5) ? vec2(0.18 + h4 * 0.14, 0.04)
                              : vec2(0.04, 0.18 + h4 * 0.14);
        d = min(d, sdBox(p - c, b));
    }
    return d;
}

vec3 katakanaLayer(vec2 uv, float aspect, int count, float audio, float t) {
    if (count <= 0) return vec3(0.0);
    vec3 acc = vec3(0.0);
    for (int i = 0; i < 16; i++) {
        if (i >= count) break;
        float fi   = float(i);
        float seed = hash11(fi * 9.31);
        float dir  = (hash11(fi * 3.7) > 0.5) ? 1.0 : -1.0;
        float spd  = 0.05 + hash11(fi * 17.1) * 0.12;
        float yPos = 0.06 + hash11(fi * 5.3) * 0.88;
        if (abs(yPos - 0.55) < 0.05) yPos += 0.08;
        vec2  ctr  = vec2(fract(hash11(fi * 11.7) + dir * t * spd), yPos);
        float sz   = 0.020 + hash11(fi * 23.1) * 0.018;
        vec2  d    = (uv - ctr) * vec2(aspect, 1.0) / sz;
        if (max(abs(d.x), abs(d.y)) > 0.7) continue;
        float pick = hash11(fi * 41.7);
        vec3  gC   = (pick < 0.33) ? CYAN : (pick < 0.66) ? PINK : CREAM;
        acc = max(acc, gC * smoothstep(0.02, 0.0, sdKatakana(d, seed))
                          * (0.7 + audio * 0.2));
    }
    return acc;
}

// Treble-gated tear: 3 horizontal bands sliding top-to-bottom; each
// fires as treble crosses threshold and offsets sample x.
float tearOffset(float uvY, float t, float treble, float amount) {
    if (amount <= 0.0) return 0.0;
    float ofs = 0.0;
    for (int i = 0; i < 3; i++) {
        float fi = float(i);
        float yC = 1.0 - fract(t * (0.13 + fi * 0.07) + hash11(fi * 7.7));
        float band = smoothstep(0.04, 0.0, abs(uvY - yC));
        float gate = smoothstep(0.25, 0.55, treble) * amount;
        ofs += band * gate * (hash11(fi * 13.1 + floor(t * 0.3)) - 0.5) * 0.06;
    }
    return ofs;
}

vec3 composeScene(vec2 uv, float aspect, float horizon, float density,
                  float speed, float sunSz, int kCount, int y2kN,
                  float bass, float mid, float audio, float t) {
    // Sky: hot pink at horizon → magenta → indigo at zenith
    float sy = clamp((uv.y - horizon) / (1.0 - horizon), 0.0, 1.0);
    vec3  col = mix(PINK,  MAGEN,  smoothstep(0.0,  0.55, sy));
          col = mix(col,   INDIGO, smoothstep(0.55, 1.0,  sy));
    // universal background: blend the sky backdrop toward bgColor
    col = mix(col, bgColor.rgb, bgColor.a);

    // Twin suns above grid
    col += twinSun(uv, aspect, horizon, sunSz, mid * audio, t);

    // Grid floor blends in below horizon haze
    if (uv.y < horizon) {
        vec3 floorC = gridFloor(uv, horizon, density, speed, bass * audio, t);
        col = mix(floorC, col, smoothstep(horizon - 0.06, horizon - 0.02, uv.y));
    }

    // Y2K junk swarm (replaces foreground silhouette).
    col += y2kSwarm(uv, aspect, y2kN, audio, t);

    // Drifting katakana on top of everything
    col += katakanaLayer(uv, aspect, kCount, audio, t);
    return col;
}

void main() {
    vec2  uv     = isf_FragNormCoord.xy;
    float aspect = RENDERSIZE.x / max(RENDERSIZE.y, 1.0);
    float t      = TIME;
    float audio  = clamp(audioReact, 0.0, 2.0);
    float bass   = audioBass;
    float mid    = audioMid;
    float treble = audioHigh;
    int   kCount = int(katakanaCount + 0.5);
    int   y2kN   = int(y2kCount + 0.5);

    // VHS tear horizontal offset
    float tear = tearOffset(uv.y, t, treble * audio, tearAmount);

    // Chromatic aberration grows toward edges and with treble
    float ab = chromaShift * (1.0 + length(uv - 0.5) * 2.0 + treble * audio * 1.5);

    vec2 uvR = uv + vec2( ab + tear, 0.0);
    vec2 uvG = uv + vec2(      tear, 0.0);
    vec2 uvB = uv + vec2(-ab + tear, 0.0);

    vec3 cR = composeScene(uvR, aspect, horizonY, gridDensity, gridSpeed,
                           sunSize, kCount, y2kN, bass, mid, audio, t);
    vec3 cG = composeScene(uvG, aspect, horizonY, gridDensity, gridSpeed,
                           sunSize, kCount, y2kN, bass, mid, audio, t);
    vec3 cB = composeScene(uvB, aspect, horizonY, gridDensity, gridSpeed,
                           sunSize, kCount, y2kN, bass, mid, audio, t);
    vec3 col = vec3(cR.r, cG.g, cB.b);

    // CRT scanlines
    float scan = 0.5 + 0.5 * sin(uv.y * RENDERSIZE.y * 1.6);
    col *= 1.0 - scanlineAmp * (1.0 - scan);

    // Tear-band cyan tint
    if (tearAmount > 0.0) {
        col = mix(col, mix(col, CYAN, 0.25), abs(tear) * 200.0 * tearAmount);
    }

    // CRT bezel vignette + tape grain (bass-modulated)
    float vig = smoothstep(1.05, 0.45, length((uv - 0.5) * vec2(aspect, 1.0)));
    col *= mix(0.78, 1.0, vig);
    col += (hash21(uv * RENDERSIZE.xy + t) - 0.5) * 0.03 * (0.6 + bass * audio * 0.4);

    // ---- universal color block (defaults = no-op) ----
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
