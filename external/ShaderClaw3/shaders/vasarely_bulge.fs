/*{
  "DESCRIPTION": "Vega Bulge — Vasarely op-art alive: a red/green/blue circle-grid wall where a roaming 3D sphere swells out of the lattice, cells inflating as it passes and melting flat behind it, with real sphere lighting. The tilt knob leans the whole wall into the second reference's perspective sweep. Bass inflates the bulge, mids roam the lens, beats send a lattice shockwave, highs glint the cell rims.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "gridN",       "LABEL": "Grid Cells",    "TYPE": "float", "MIN": 8.0, "MAX": 28.0, "DEFAULT": 17.0, "GROUP": "Shape / Geometry" },
    { "NAME": "bulgeSize",   "LABEL": "Bulge Size",    "TYPE": "float", "MIN": 0.15, "MAX": 0.6, "DEFAULT": 0.38, "GROUP": "Shape / Geometry" },
    { "NAME": "bulgeAmt",    "LABEL": "Bulge Power",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.8,  "GROUP": "Shape / Geometry" },
    { "NAME": "tiltAmt",     "LABEL": "Wall Tilt",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "roamSpeed",   "LABEL": "Roam Speed",    "TYPE": "float", "MIN": 0.0, "MAX": 2.0,  "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "hueShift",    "LABEL": "Palette Spin",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.08, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "vbScene" },
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// VEGA BULGE — Vasarely's Vega sphere paintings, both references fused:
//   the frontal bulging wall AND the tilted perspective sweep (tilt
//   knob). The classic construction: a square lattice where a lens
//   remaps radius r → r' so cells near the lens center project LARGER
//   (sphere surface seen head-on) and compress at the lens rim (surface
//   turning away). Cell contents alternate Vasarely-style: blue squares
//   carrying green rings with red cores, green squares carrying red
//   discs — with the checker phase and oval squash following the local
//   lattice distortion, so cells at the bulge rim stretch into the
//   ellipses of the original paintings. Sphere lighting on the bulge:
//   lambert + rim darkening + a moving specular, so it reads as a real
//   ball pushing through the canvas. Beat = a ring shockwave of cell
//   growth radiating from the lens. 3 passes: scene → trail → composite.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

vec3 hueRotate(vec3 c, float a) {
    float an = a * TAU;
    vec3 k = vec3(0.57735);
    return c * cos(an) + cross(k, c) * sin(an) + k * dot(k, c) * (1.0 - cos(an));
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float t = TIME * motionSpeed;

    // ── optional perspective tilt (the second reference's sweep) ────────
    // shear + scale so the right side balloons toward the viewer
    if (tiltAmt > 0.001) {
        float persp = 1.0 / (1.0 + tiltAmt * 0.9 * (0.6 - q.x));
        q *= persp;
        q.y *= 1.0 + tiltAmt * 0.25 * q.x;
    }

    // ── roaming bulge lens ──────────────────────────────────────────────
    float roam = t * roamSpeed * 0.35 + amt * 0.4 * midP;
    vec2 lensC = 0.30 * vec2(sin(roam * 0.7), cos(roam * 0.53) * 0.8) * min(roamSpeed * 4.0, 1.0);
    float lensR = bulgeSize * (1.0 + amt * 0.35 * bassP + 0.04 * sin(t * 0.8));
    float pw = bulgeAmt * 0.85;

    vec2 d = q - lensC;
    float r = length(d);
    float rn = clamp(r / lensR, 0.0, 1.0);
    // sphere projection: inside the lens, sample the lattice at
    // asin-compressed radius → center cells huge, rim cells crushed
    float rmap = mix(rn, asin(clamp(rn, 0.0, 1.0)) / 1.5707963, pw);
    // extra center magnification
    float mag = mix(1.0, 0.55, pw * (1.0 - rn * rn));
    vec2 ql = lensC + normalize(d + 1e-6) * rmap * lensR * mag;
    float inLens = smoothstep(lensR, lensR * 0.97, r);
    vec2 qq = mix(q, ql, inLens);

    // local scale factor (how much this cell is magnified) for shading
    float cellMag = mix(1.0, 1.0 / max(mag + (rmap - rn) * 2.0, 0.3), inLens);

    // ── the lattice ─────────────────────────────────────────────────────
    float N = gridN;
    vec2 g = qq * N + vec2(0.5);
    // slow lattice drift keeps the wall alive even with the lens parked
    g += vec2(t * 0.03, t * 0.021);
    vec2 id = floor(g);
    vec2 lp = fract(g) - 0.5;
    float checker = mod(id.x + id.y, 2.0);

    // beat shockwave: a ring of cell growth radiating from the lens
    float ringR = fract(t * 0.4) * 1.5;
    float shock = exp(-pow((r - ringR) * 7.0, 2.0)) * amt * beatP;

    // oval squash: cells at the bulge rim stretch tangentially
    float ang = atan(d.y, d.x);
    float squash = inLens * pw * smoothstep(0.2, 1.0, rn) * 0.45;
    vec2 sq = vec2(cos(ang), sin(ang));
    // project lp onto radial/tangential axes and squash radial
    vec2 lpr = vec2(dot(lp, sq), dot(lp, vec2(-sq.y, sq.x)));
    lpr.x /= (1.0 - squash);
    vec2 lpe = lpr;

    float aa = 1.8 * N / R.y * cellMag;

    // ── Vasarely palette (spinnable) ────────────────────────────────────
    vec3 vRed   = hueRotate(vec3(0.90, 0.16, 0.10), hueShift);
    vec3 vGreen = hueRotate(vec3(0.15, 0.62, 0.20), hueShift);
    vec3 vBlue  = hueRotate(vec3(0.52, 0.62, 0.88), hueShift);
    vec3 vDkGrn = hueRotate(vec3(0.10, 0.35, 0.14), hueShift);

    // plate color alternates; toward the wall edge the plates darken
    vec3 plate = mix(vBlue, vGreen, checker);
    float wallFade = smoothstep(1.15, 0.45, length(q));
    plate = mix(mix(vDkGrn, plate, 0.55), plate, wallFade);

    // disc sizes grow inside the lens + on the shockwave
    float dsc = 0.34 * (1.0 + 0.25 * inLens * pw * (1.0 - rn) + shock * 0.5);
    float ringOuter = dsc * 1.22;

    float dd = length(lpe);
    vec3 col = plate;
    if (checker > 0.5) {
        // green-ring red-core cell
        float ringM = smoothstep(ringOuter + aa, ringOuter - aa, dd);
        col = mix(col, vGreen * 1.05, ringM);
        float coreM = smoothstep(dsc * 0.82 + aa, dsc * 0.82 - aa, dd);
        col = mix(col, vRed, coreM);
    } else {
        // red disc with thin green halo on blue plate
        float haloM = smoothstep(ringOuter + aa, ringOuter - aa, dd)
                    - smoothstep(dsc + aa, dsc - aa, dd);
        col = mix(col, vDkGrn, clamp(haloM, 0.0, 1.0) * 0.8);
        float discM = smoothstep(dsc + aa, dsc - aa, dd);
        col = mix(col, vRed, discM);
    }

    // cell rim glint on highs (tight)
    float glint = exp(-abs(dd - dsc) * 60.0 / cellMag) * amt * 0.3 * highP;
    col += vec3(1.0, 0.95, 0.8) * glint;

    // ── sphere lighting over the bulge ──────────────────────────────────
    if (inLens > 0.001) {
        float z = sqrt(max(1.0 - rn * rn, 0.0));
        vec3 nrm = normalize(vec3(d / lensR, z + 0.35));
        vec3 L = normalize(vec3(0.45 * sin(t * 0.2) + 0.3, 0.55, 0.75));
        float dif = clamp(dot(nrm, L), 0.0, 1.0);
        float spe = pow(clamp(dot(reflect(-L, nrm), vec3(0.0, 0.0, 1.0)), 0.0, 1.0), 28.0);
        float shade = (0.55 + 0.55 * dif) * mix(1.0, 0.55 + 0.45 * z, pw);
        col = mix(col, col * shade + vec3(1.0) * spe * 0.35 * pw, inLens * bulgeAmt);
        // contact shadow ring just outside the lens
        float contact = exp(-pow((r - lensR * 1.04) * 24.0, 2.0)) * pw;
        col *= 1.0 - contact * 0.35;
    }

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.015;
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        gl_FragColor = vec4(clamp(renderScene(), 0.0, 1.0), 1.0);
    } else if (PASSINDEX == 1) {
        vec3 col = texture2D(vbScene, uv).rgb;
        vec3 prev = texture2D(abTrail, uv).rgb;
        float decay = 0.50 + 0.46 * trailAmt;
        col = max(col, prev * decay - 0.0045);
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else {
        vec2 dir = uv - 0.5;
        float ab = aberration * 0.0045;
        vec3 base;
        base.r = texture2D(abTrail, uv + dir * ab).r;
        base.g = texture2D(abTrail, uv).g;
        base.b = texture2D(abTrail, uv - dir * ab).b;
        vec3 bl = vec3(0.0);
        for (int i = 0; i < 8; i++) {
            float an = float(i) * 0.7853982;
            vec2 o = vec2(cos(an), sin(an)) * (3.5 / R.y);
            bl += texture2D(abTrail, uv + o).rgb;
            bl += texture2D(abTrail, uv + o * 2.6).rgb * 0.6;
        }
        bl /= 12.8;
        bl = max(bl - 0.52, 0.0);
        vec3 col = base + bl * bl * bloomAmt * 1.8;
        float lvl = clamp(audioLevel, 0.0, 1.0);
        col *= brightness * mix(1.0, 0.64 + 0.36 * lvl, audioReact)
             * (1.0 + audioReact * 0.06 * clamp(audioBeatPulse, 0.0, 1.0));
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        gl_FragColor = vec4(col, 1.0);
    }
}
