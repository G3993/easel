/*{
  "CATEGORIES": [
    "Generator",
    "Sci-Fi",
    "Audio Reactive"
  ],
  "DESCRIPTION": "Unstable projected hologram — wireframe SDF subject (head/cube/sphere/cylinder/custom) scanned by a sweeping line, corrupted by interference bands, smeared by chromatic aberration, hazed by volumetric beam dust. Drop any image into Custom Image to project your own logo or silhouette as a hologram. References: Princess Leia recording (1977), Blade Runner 2049 Joi, JARVIS UI, HoloLens, Cyberpunk brain-dance. Bass triggers signal-loss dropouts, mid drives scan speed, treble shimmers the RGB split. Single-pass, linear HDR.",
  "INPUTS": [
    {
      "NAME": "subject",
      "LABEL": "Subject",
      "TYPE": "long",
      "DEFAULT": 0,
      "VALUES": [
        0,
        1,
        2,
        3,
        4
      ],
      "LABELS": [
        "Head",
        "Cube",
        "Sphere",
        "Cylinder",
        "Custom"
      ]
    },
    {
      "NAME": "inputTex",
      "LABEL": "Custom Image",
      "TYPE": "image"
    },
    {
      "NAME": "interference",
      "LABEL": "Interference",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55
    },
    {
      "NAME": "beamHaze",
      "LABEL": "Beam Haze",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.65
    },
    {
      "NAME": "wireDensity",
      "LABEL": "Wire Density",
      "TYPE": "float",
      "MIN": 4,
      "MAX": 28,
      "DEFAULT": 14,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "scanSpeed",
      "LABEL": "Scan Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 0.55,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "rotateSpeed",
      "LABEL": "Rotation",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1.5,
      "DEFAULT": 0.35,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "chromaShift",
      "LABEL": "Chromatic Split",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.04,
      "DEFAULT": 0.01,
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "bgColor",
      "LABEL": "Background",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
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

// Hologram/Glitch — SDF subject re-rendered as triplanar wireframe, then
// degraded in screen space: scan line sweep, interference bands, chromatic
// aberration, bass-triggered signal-loss dropouts, volumetric beam haze.
// Five-color palette, pure black bg, linear HDR (host tonemaps).

#define MAX_STEPS 80
#define MAX_DIST  18.0
#define EPS       0.0015

// ─── palette (locked) ────────────────────────────────────────────────────
const vec3 PAL_PRIMARY = vec3(0.25, 0.85, 1.00);
const vec3 PAL_SHADOW  = vec3(0.25, 0.45, 1.00);
const vec3 PAL_HIGH    = vec3(0.96, 0.98, 1.00);
const vec3 PAL_ERROR   = vec3(0.95, 0.20, 0.75);

// ─── hash / noise ────────────────────────────────────────────────────────
float h11(float n) { return fract(sin(n * 12.9898) * 43758.5453); }
float h21(vec2 p)  { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
float h31(vec3 p)  { return fract(sin(dot(p, vec3(127.1, 311.7, 74.7))) * 43758.5453); }

// ─── primitive SDFs ──────────────────────────────────────────────────────
float sdSphere (vec3 p, float r) { return length(p) - r; }
float sdBox    (vec3 p, vec3 b)  { vec3 q = abs(p) - b; return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0); }
float sdCyl    (vec3 p, float r, float h) { vec2 d = vec2(length(p.xz)-r, abs(p.y)-h); return min(max(d.x,d.y),0.0) + length(max(d,0.0)); }
float sdCapsule(vec3 p, vec3 a, vec3 b, float r) {
    vec3 pa = p - a, ba = b - a;
    float t = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * t) - r;
}
float opSU(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

// ─── stylized humanoid head silhouette (Leia hologram reference) ────────
float sdHead(vec3 p) {
    // Skull — slightly squashed sphere
    float skull = sdSphere(p * vec3(1.0, 0.92, 1.05), 0.55);
    // Jaw — rounded box pulled forward and down
    float jaw   = sdBox(p - vec3(0.0, -0.32, 0.10), vec3(0.32, 0.20, 0.30)) - 0.12;
    // Neck — capsule descending
    float neck  = sdCapsule(p, vec3(0.0, -0.55, 0.0), vec3(0.0, -1.10, 0.0), 0.18);
    float head  = opSU(skull, jaw, 0.15);
    head        = opSU(head,  neck, 0.10);
    // Two side "buns" — Leia silhouette tell
    float bunL  = sdSphere(p - vec3(-0.55, -0.05,  0.0), 0.22);
    float bunR  = sdSphere(p - vec3( 0.55, -0.05,  0.0), 0.22);
    head        = opSU(head, bunL, 0.05);
    head        = opSU(head, bunR, 0.05);
    return head;
}

// ─── subject dispatch ────────────────────────────────────────────────────
float sdSubject(vec3 p, int which) {
    if (which == 1) return sdBox(p, vec3(0.55));
    if (which == 2) return sdSphere(p, 0.70);
    if (which == 3) return sdCyl(p, 0.55, 0.75);
    return sdHead(p);
}

// ─── triplanar wireframe carve — gives the SDF its grid skin ────────────
// Returns the line intensity at p (0 = empty, 1 = on a wire).
float wireGrid(vec3 p, float density) {
    vec3 g = abs(fract(p * density) - 0.5);
    float lx = smoothstep(0.45, 0.50, max(g.y, g.z));
    float ly = smoothstep(0.45, 0.50, max(g.x, g.z));
    float lz = smoothstep(0.45, 0.50, max(g.x, g.y));
    return clamp(lx + ly + lz, 0.0, 1.5);
}

// ─── normal via tetrahedron ─────────────────────────────────────────────
vec3 sdNormal(vec3 p, int which) {
    const vec2 e = vec2(0.0015, -0.0015);
    return normalize(
        e.xyy * sdSubject(p + e.xyy, which) +
        e.yyx * sdSubject(p + e.yyx, which) +
        e.yxy * sdSubject(p + e.yxy, which) +
        e.xxx * sdSubject(p + e.xxx, which));
}

// ─── interference: per-row band with shifted/inverted/noisy slices ──────
// Returns: x = horizontal shift, y = invert flag, z = noise replace amount
vec3 interferenceBand(float ny, float t, float amount) {
    // Time-quantized so bands hold for a frame slice (~12Hz)
    float slab = floor(ny * 24.0) + floor(t * 12.0) * 47.0;
    float r1   = h11(slab);
    float r2   = h11(slab + 7.7);
    float r3   = h11(slab + 13.3);
    // Probability of corruption rises with `amount`
    float trig = step(1.0 - 0.30 * amount, r1);
    float shift = (r2 - 0.5) * 0.18 * amount * trig;
    float invert = step(0.85, r2) * trig;
    float noiseR = step(0.92, r3) * trig;
    return vec3(shift, invert, noiseR);
}

// ─── 2D silhouette sample from input texture (Custom mode) ──────────────
// Maps screen UV onto a centered, aspect-preserving square that holds the
// image. Returns alpha-bright regions of the texture as the silhouette mask.
float sampleCustomMask(vec2 uv, float t, float rotSpeed, float audioM) {
    // Center, square the sample plane (aspect-corrected NDC)
    vec2 ndc = uv * 2.0 - 1.0;
    ndc.x   *= RENDERSIZE.x / RENDERSIZE.y;
    // Rotate the silhouette around screen center for parity with SDF subjects
    float ang = t * (rotSpeed + 0.10) + audioM * 0.4;
    float ca = cos(ang), sa = sin(ang);
    // 2D rotation (fake Y-spin → x-squash for that hologram shimmy)
    vec2 r2 = vec2(ndc.x * ca, ndc.y);
    // Fit ~1.4 unit square into 0..1 image coords, centered
    vec2 iuv = r2 / 1.4 * 0.5 + 0.5;
    if (iuv.x < 0.0 || iuv.x > 1.0 || iuv.y < 0.0 || iuv.y > 1.0) return 0.0;
    vec4 tex = IMG_NORM_PIXEL(inputTex, iuv);
    // Treat both alpha and luminance as silhouette signal so PNGs with alpha
    // and JPEGs without alpha both work.
    float lum = dot(tex.rgb, vec3(0.299, 0.587, 0.114));
    float mask = max(tex.a, lum);
    return clamp(mask, 0.0, 1.0);
}

// ─── render the hologram subject at one screen-space sample ─────────────
// Returns linear color contribution + alpha-like density in .a
vec4 sampleHologram(vec2 uv, float t, int which, float density,
                    float rotSpeed, float audioM) {
    // ─── Custom 2D image mode ──────────────────────────────────────────
    if (which == 4) {
        float m = sampleCustomMask(uv, t, rotSpeed, audioM);
        if (m < 0.02) return vec4(0.0);
        // Fake wireGrid in screen space so chromatic split + scanlines still
        // see structure inside the silhouette.
        vec2 ndc = uv * 2.0 - 1.0;
        ndc.x   *= RENDERSIZE.x / RENDERSIZE.y;
        vec3 g = abs(fract(vec3(ndc.x, ndc.y, ndc.x + ndc.y) * density * 0.5) - 0.5);
        float wire = smoothstep(0.45, 0.50, max(g.x, g.y));
        // Edge detection on the mask = silhouette emission (stand-in for fresnel)
        float e = 0.004;
        float mL = sampleCustomMask(uv + vec2(-e, 0.0), t, rotSpeed, audioM);
        float mR = sampleCustomMask(uv + vec2( e, 0.0), t, rotSpeed, audioM);
        float mD = sampleCustomMask(uv + vec2(0.0,-e), t, rotSpeed, audioM);
        float mU = sampleCustomMask(uv + vec2(0.0, e), t, rotSpeed, audioM);
        float edge = clamp(abs(mL - mR) + abs(mD - mU), 0.0, 1.0) * 4.0;
        edge = clamp(edge, 0.0, 1.0);
        vec3 base = mix(PAL_SHADOW, PAL_PRIMARY, m);
        vec3 col  = mix(base * 0.25, PAL_HIGH, wire * m);
        col += PAL_PRIMARY * edge * 0.9;
        float a = clamp(m * (0.55 + 0.45 * wire) + edge * 0.6, 0.0, 1.0);
        return vec4(col, a);
    }

    // ─── SDF mode (Head/Cube/Sphere/Cylinder) ──────────────────────────
    // NDC with aspect correction
    vec2 ndc = (uv * 2.0 - 1.0);
    ndc.x   *= RENDERSIZE.x / RENDERSIZE.y;

    // Camera — fixed, the subject rotates
    vec3 ro = vec3(0.0, 0.0, 3.2);
    vec3 rd = normalize(vec3(ndc.x, ndc.y, -1.6));

    // Subject rotation around Y, gentle bob
    float ang = t * (rotSpeed + 0.10) + audioM * 0.4;
    float ca = cos(ang), sa = sin(ang);
    mat3 R = mat3( ca, 0.0,  sa,
                  0.0, 1.0, 0.0,
                  -sa, 0.0,  ca);

    float d = 0.0;
    float hitDist = -1.0;
    vec3  hitP    = vec3(0.0);
    for (int i = 0; i < MAX_STEPS; i++) {
        vec3 p = ro + rd * d;
        vec3 pr = R * p;
        float ds = sdSubject(pr, which);
        if (ds < EPS) { hitDist = d; hitP = pr; break; }
        if (d > MAX_DIST) break;
        d += ds * 0.90;
    }

    if (hitDist < 0.0) return vec4(0.0);

    vec3 n   = sdNormal(hitP, which);
    // Carve the wireframe into the surface using triplanar lines
    float w  = wireGrid(hitP, density);
    // Edge highlight via fresnel — accentuates silhouette
    float fres = pow(1.0 - max(dot(n, normalize(-rd)), 0.0), 2.2);

    // Shading from primary→shadow along normal y (top lit, bottom shaded)
    float lit = 0.5 + 0.5 * dot(n, normalize(vec3(0.3, 0.8, 0.5)));
    vec3  base = mix(PAL_SHADOW, PAL_PRIMARY, lit);
    // Wires on the surface get the highlight color
    vec3  col  = mix(base * 0.25, PAL_HIGH, clamp(w, 0.0, 1.0));
    // Silhouette emission
    col += PAL_PRIMARY * fres * 0.8;

    return vec4(col, clamp(w * 0.85 + fres * 0.6, 0.0, 1.0));
}

// ─── main ───────────────────────────────────────────────────────────────
void main() {
    vec2 uv = isf_FragNormCoord.xy;
    float t = TIME;

    float aR    = clamp(audioReact, 0.0, 2.0);
    float aBass = audioBass * aR;
    float aMid  = audioMid  * aR;
    float aHigh = audioHigh * aR;

    int   which   = int(clamp(float(subject), 0.0, 4.0) + 0.5);
    // Fallback: if Custom is selected but no image is bound (host returns a
    // 1x1 placeholder), drop back to the head silhouette.
    if (which == 4 && IMG_SIZE_inputTex.x < 2.0) which = 0;
    float scanS   = scanSpeed * (1.0 + 4.0 * aMid);
    float chrAmt  = chromaShift * (1.0 + 6.0 * aHigh);
    float intAmt  = clamp(interference + 1.2 * aBass, 0.0, 1.0);
    float density = wireDensity;

    // Major signal-loss event — bass thump triggers a short dropout
    // "_drop" rises sharply, decays over ~0.4s. Latched by bass envelope.
    float dropPhase = fract(t * 0.31 + h11(floor(t * 0.31)) * 0.5);
    float dropTrig  = step(0.75, h11(floor(t * 4.0))) * step(0.20, aBass);
    float dropEnv   = exp(-dropPhase * 6.0) * dropTrig;
    dropEnv = max(dropEnv, step(0.997, h11(floor(t * 13.0))) * 0.7);

    // Scan line — bright thin sweep climbing the frame, with a fading trail
    float scanY = fract(t * scanS * 0.35);
    float scanD = uv.y - scanY;
    // Bright leading line
    float scanLine = exp(-pow(scanD * 180.0, 2.0)) * 1.4;
    // Fading downward trail (only below the line as it sweeps up)
    float trail = exp(-max(-scanD, 0.0) * 9.0) * 0.18;
    float scanFx = scanLine + trail;

    // Per-row interference band data
    vec3 band = interferenceBand(uv.y, t, intAmt);
    float rowShift = band.x;
    float rowInv   = band.y;
    float rowNoise = band.z;

    // Screen-space jitter on big drops — whole frame trembles
    vec2 quake = vec2(
        (h21(vec2(floor(t * 30.0), 0.0)) - 0.5),
        (h21(vec2(0.0, floor(t * 30.0))) - 0.5)) * dropEnv * 0.09;

    vec2 sUV = uv + vec2(rowShift, 0.0) + quake;

    // Chromatic aberration — sample three offset copies of the hologram
    vec2 caOff = vec2(chrAmt, 0.0);
    vec4 sR = sampleHologram(sUV + caOff, t, which, density, rotateSpeed, aMid);
    vec4 sG = sampleHologram(sUV,         t, which, density, rotateSpeed, aMid);
    vec4 sB = sampleHologram(sUV - caOff, t, which, density, rotateSpeed, aMid);

    vec3 holo = vec3(sR.r, sG.g, sB.b);
    float alpha = max(sG.a, max(sR.a, sB.a));

    // Color invert on corrupted bands
    holo = mix(holo, PAL_HIGH - holo, rowInv);
    // Replace with magenta-tinted noise on worst bands
    if (rowNoise > 0.5) {
        float n = h21(uv * RENDERSIZE.xy + floor(t * 24.0));
        vec3 noiseCol = mix(PAL_ERROR, PAL_HIGH, n);
        holo  = mix(holo,  noiseCol, 0.85);
        alpha = mix(alpha, 0.9,      0.7);
    }

    // Bass drop: dissolve the figure into pure noise + magenta error flashes
    if (dropEnv > 0.01) {
        float n = h21(uv * RENDERSIZE.xy * 0.5 + floor(t * 30.0));
        vec3  errCol = mix(vec3(0.0), PAL_ERROR, step(0.6, n));
        errCol = mix(errCol, PAL_HIGH, step(0.93, n));
        holo  = mix(holo, errCol, dropEnv * 0.92);
        alpha = mix(alpha, n, dropEnv * 0.6);
    }

    // Apply scan line — additive bright sweep over the figure & the haze
    holo += PAL_PRIMARY * scanFx * (alpha * 0.6 + 0.15);

    // ─── volumetric beam haze ────────────────────────────────────────────
    // Cone of dust around vertical axis through the figure.
    vec2 ndc = uv * 2.0 - 1.0;
    ndc.x   *= RENDERSIZE.x / RENDERSIZE.y;
    float radial = length(ndc.xy * vec2(0.9, 0.55));
    float beam   = exp(-radial * 2.4) * (0.35 + 0.65 * smoothstep(-1.0, 0.4, ndc.y));
    // Drifting dust motes — slow vertical particles
    float dust = 0.0;
    for (int i = 0; i < 5; i++) {
        float fi = float(i);
        vec2  c  = vec2(0.5 + 0.42 * (h11(fi * 3.7) - 0.5),
                        fract(h11(fi * 1.3) - t * (0.05 + 0.04 * fi)));
        float r  = 0.004 + 0.010 * h11(fi * 9.1);
        float dm = length((uv - c) * vec2(RENDERSIZE.x / RENDERSIZE.y, 1.0));
        dust    += smoothstep(r, 0.0, dm) * (0.3 + 0.5 * h11(fi * 7.7));
    }

    vec3 hazeCol = PAL_PRIMARY * beam * beamHaze * 0.35
                 + PAL_HIGH    * dust * 0.55;

    // Composite hologram over black bg + additive haze + floor gleam
    vec3 col = mix(vec3(0.0), holo, clamp(alpha, 0.0, 1.0)) + hazeCol;
    float floorGleam = smoothstep(0.0, 0.08, uv.y) * (1.0 - smoothstep(0.0, 0.18, uv.y));
    col += PAL_PRIMARY * floorGleam * 0.18 * (alpha + 0.4);

    // Broadcast scanlines + carrier breathing + treble sparkle
    col *= 0.92 + 0.08 * sin(uv.y * RENDERSIZE.y * 1.7);
    col *= 0.88 + 0.12 * sin(t * 1.3) + 0.06 * sin(t * 7.1);
    float sparkle = step(0.997 - aHigh * 0.06, h21(uv * RENDERSIZE.xy + t));
    col += PAL_HIGH * sparkle * 0.6 * (0.3 + aHigh);

    // Linear HDR boost on the brightest emissive bits (host tonemaps)
    float lum = dot(col, vec3(0.299, 0.587, 0.114));
    col += PAL_PRIMARY * pow(max(lum - 0.6, 0.0), 1.6) * 0.8;

    // ---- universal color block (defaults = no-op) ----
    float ucL = dot(col, vec3(0.299, 0.587, 0.114));
    col = mix(vec3(ucL), col, colorBoost);
    if (hueShift > 0.0005) {
        float hA = hueShift * 6.2831853;
        float hC = cos(hA), hS = sin(hA);
        mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
                + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
                + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
        col = clamp(hM * col, 0.0, 1.0);
    }
    col = mix(col, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
    gl_FragColor = vec4(col, 1.0);
}
