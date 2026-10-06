/*{
  "DESCRIPTION": "Meadow Dream — a dimensional dusk landscape the camera drifts through sideways: five parallax mountain ridgelines dissolve into violet haze under a slowly shifting gradient sky with drifting cloud decks and a haloed low sun, while a real blade-by-blade grass foreground sways in layered wind, tips backlit gold, wildflower heads (poppy, cornflower, daisy, buttercup, rose) riding the tallest stems. Bass breathes the sun halo, mids push wind through the grass, beats send a golden shimmer traveling across the blade tips.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "sunHeight",     "LABEL": "Sun Height",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Shape / Geometry" },
    { "NAME": "parallaxSpeed", "LABEL": "Parallax Drift", "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "grassHeight",   "LABEL": "Grass Height",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "flowerDensity", "LABEL": "Flower Density", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "skyShift",      "LABEL": "Sky Shift",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",    "LABEL": "Brightness",     "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "motionSpeed",   "LABEL": "Wind Speed",     "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",    "LABEL": "Audio React",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.25, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// MEADOW DREAM v2 — a genuinely dimensional landscape, composed like a
//   camera drifting sideways at golden hour:
//   SKY    two-deck drifting cloud bands over a slowly color-shifting dusk
//          gradient; low sun with a crisp disc, layered halo, haze slices.
//   RANGE  five silhouette ridgelines (ridged fbm far → soft fbm hills
//          near), each scrolling at its own parallax rate, dissolving into
//          violet valley fog, sun-warm rim light on every crest.
//   FIELD  a real screen-space blade field: three parallax grass layers,
//          every blade individually hashed (height, curvature, width,
//          wind phase), quadratic bend, backlit rim on the tip edges,
//          petaled wildflower heads riding the stems. NO stipple dots.
//   Audio: bass breathes the halo, mids push the wind (additive phase),
//   beats travel a golden shimmer across the tips. Composite dim-tracks.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash11(float n) { return fract(sin(n) * 43758.5453123); }
float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash21(i), hash21(i + vec2(1.0, 0.0)), f.x),
               mix(hash21(i + vec2(0.0, 1.0)), hash21(i + vec2(1.0, 1.0)), f.x), f.y);
}
float fbm(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 4; i++) {
        v += a * vnoise(p);
        p = p * 2.07 + vec2(7.3, 3.1);
        a *= 0.52;
    }
    return v;
}
// ridged fbm: sharp mountain crests
float ridged(vec2 p) {
    float v = 0.0, a = 0.55;
    for (int i = 0; i < 4; i++) {
        float n = vnoise(p);
        n = 1.0 - abs(2.0 * n - 1.0);
        v += a * n * n;
        p = p * 2.15 + vec2(11.7, 5.3);
        a *= 0.5;
    }
    return v;
}
vec3 hueRotate(vec3 c, float a) {
    float cs = cos(a), sn = sin(a);
    mat3 m = mat3(
        0.299 + 0.701*cs + 0.168*sn, 0.587 - 0.587*cs + 0.330*sn, 0.114 - 0.114*cs - 0.497*sn,
        0.299 - 0.299*cs - 0.328*sn, 0.587 + 0.413*cs + 0.035*sn, 0.114 - 0.114*cs + 0.292*sn,
        0.299 - 0.300*cs + 1.250*sn, 0.587 - 0.588*cs - 1.050*sn, 0.114 + 0.886*cs - 0.203*sn);
    return clamp(c * m, 0.0, 2.0);
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;          // y in [-0.5, 0.5]
    float aa = 1.5 / R.y;                    // pixel-true AA width

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);

    float camX = TIME * 0.060 * parallaxSpeed;   // sideways camera drift
    float tw   = TIME * motionSpeed;             // wind clock

    float horizon = -0.04;

    // ── dusk sky: slow color drift between two twilight palettes ────────
    float ds = 0.5 + 0.5 * sin(TIME * 0.013);
    vec3 zen  = mix(vec3(0.20, 0.23, 0.50), vec3(0.28, 0.20, 0.47), ds);
    vec3 midc = mix(vec3(0.56, 0.46, 0.76), vec3(0.64, 0.42, 0.70), ds);
    vec3 horc = mix(vec3(0.99, 0.64, 0.42), vec3(0.94, 0.52, 0.50), ds);
    vec3 col = mix(horc, midc, smoothstep(horizon, horizon + 0.30, q.y));
    col = mix(col, zen, smoothstep(horizon + 0.22, 0.55, q.y));
    // horizon glow slowly breathing
    col += horc * 0.10 * exp(-max(q.y - horizon, 0.0) * 6.0) * (0.85 + 0.15 * sin(TIME * 0.07));

    // ── the sun: crisp disc + breathing layered halo + haze slices ──────
    vec2 sunC = vec2(-0.16 + 0.015 * sin(TIME * 0.021), horizon + mix(0.07, 0.40, sunHeight));
    float sd = length(q - sunC);
    vec3 sunCol = vec3(1.00, 0.55, 0.26);
    float breathe = 1.0 + amt * 0.45 * bassP + 0.04 * sin(TIME * 0.11);
    col += sunCol * breathe * (0.34 * exp(-sd * 6.5) + 0.08 * exp(-sd * 2.4));
    float sr = 0.052;
    float disc = smoothstep(sr + aa, sr - aa, sd);
    vec3 discCol = mix(sunCol, vec3(1.0, 0.88, 0.62), exp(-sd * 22.0));
    col = mix(col, discCol, disc * 0.97);
    float hb = smoothstep(0.35, 0.9, fbm(vec2(q.x * 1.6 + camX * 0.06 + tw * 0.010, q.y * 40.0)));
    col = mix(col, horc * 0.92, disc * hb * 0.35);   // dusk haze slicing the low sun

    // ── two drifting cloud decks ────────────────────────────────────────
    float cd1 = fbm(vec2(q.x * 0.85 + camX * 0.10 + tw * 0.028, q.y * 5.5 + 2.0));
    float z1 = smoothstep(0.02, 0.10, q.y - horizon) * smoothstep(0.34, 0.16, q.y - horizon);
    vec3 cl1 = mix(vec3(0.96, 0.58, 0.52), vec3(0.55, 0.44, 0.74), smoothstep(0.1, 0.7, length(q - sunC)));
    col = mix(col, cl1, smoothstep(0.46, 0.74, cd1) * z1 * 0.75);
    float cd2 = fbm(vec2(q.x * 0.6 - camX * 0.05 - tw * 0.018 + 7.0, q.y * 3.2 + 9.0));
    float z2 = smoothstep(0.24, 0.34, q.y - horizon) * smoothstep(0.62, 0.40, q.y - horizon);
    vec3 cl2 = mix(vec3(0.36, 0.32, 0.68), vec3(0.86, 0.50, 0.60), exp(-abs(q.x - sunC.x) * 1.2));
    col = mix(col, cl2, smoothstep(0.50, 0.78, cd2) * z2 * 0.55);
    // thin teal dusk band just above the ridgelines — real twilight color
    float teal = smoothstep(0.26, 0.10, q.y - horizon) * smoothstep(0.02, 0.09, q.y - horizon);
    col = mix(col, vec3(0.60, 0.78, 0.72), teal * 0.16);

    vec3 hazeCol = mix(midc, vec3(0.78, 0.55, 0.62), 0.40);   // lilac-rose air

    // ── five parallax mountain / hill silhouette layers ─────────────────
    for (int i = 0; i < 5; i++) {
        float fi = float(i);
        float depth = fi / 4.0;                            // 0 far → 1 near
        float par = mix(0.10, 0.60, depth * depth);
        float xr = (q.x + camX * par) * mix(1.1, 2.4, depth) + fi * 17.3;
        float rg = ridged(vec2(xr, fi * 7.7));
        float sm = fbm(vec2(xr * 0.8, fi * 3.1 + 40.0));
        float prof = mix(rg, sm, smoothstep(0.25, 0.85, depth));  // peaks far, hills near
        float h = horizon + mix(0.17, 0.015, pow(depth, 0.85))
                + mix(0.10, 0.05, depth) * prof - 0.05 * depth;
        // scrub/tree fringe on the near hills — crisp vegetated ridge edge
        h += 0.014 * depth * depth * (vnoise(vec2(xr * 26.0, fi * 11.0)) - 0.5);
        float m = smoothstep(aa, -aa, q.y - h);
        if (m < 0.001) continue;
        vec3 lc = mix(hazeCol, vec3(0.07, 0.11, 0.13), 0.30 + 0.65 * pow(depth, 1.0));
        // valley fog pooling between the ridgelines
        lc = mix(lc, hazeCol, (1.0 - depth) * 0.35 * smoothstep(0.16, -0.02, q.y - horizon));
        // sun-warm rim light hugging each crest
        float rim = exp(-max(h - q.y, 0.0) * 34.0);
        lc += sunCol * rim * exp(-abs(q.x - sunC.x) * 1.1) * (0.28 + amt * 0.22 * bassP) * (1.0 - 0.55 * depth);
        // crisp 1.5px crest line — pixel-true ridge definition
        lc += sunCol * smoothstep(2.0 * aa, 0.0, abs(q.y - h)) * 0.16 * (1.0 - 0.6 * depth);
        col = mix(col, lc, m);
    }

    // ── meadow ground plane (parallax with the front grass) ─────────────
    float groundTop = -0.30 + 0.035 * fbm(vec2(q.x * 3.0 + camX * 1.7, 7.0))
                    + 0.008 * (vnoise(vec2(q.x * 55.0 + camX * 90.0, 13.0)) - 0.5);
    float gm = smoothstep(aa, -aa, q.y - groundTop);
    vec3 gcolG = mix(vec3(0.085, 0.14, 0.09), vec3(0.020, 0.045, 0.030),
                     clamp((groundTop - q.y) * 3.5, 0.0, 1.0));
    gcolG *= 0.85 + 0.30 * vnoise(vec2(q.x * 38.0 + camX * 80.0, q.y * 55.0));
    col = mix(col, gcolG, gm);

    // ── real grass: three parallax blade layers, per-blade wind ─────────
    for (int l = 0; l < 3; l++) {
        float fl = float(l);
        float w = 0.011 * pow(1.85, fl);                   // column width
        float pf = 0.9 + 0.65 * fl;                        // parallax factor
        float hScale = 0.55 + 0.30 * fl;
        float rootY = -0.505 - 0.030 * fl;
        float xl = q.x + camX * pf;                        // layer-scrolled x
        float cid = floor(xl / w);
        for (int k = -1; k <= 1; k++) {
            float id = cid + float(k);
            float h1 = hash11(id * 12.99 + fl * 31.7 + 1.0);
            float h2 = hash11(id * 4.41  + fl * 17.3 + 7.0);
            float h3 = hash11(id * 7.77  + fl * 11.1 + 3.0);
            float h4 = hash11(id * 2.31  + fl * 23.9 + 9.0);
            float rootX = (id + 0.5 + (h1 - 0.5) * 0.6) * w;
            float bladeH = mix(0.18, 0.40, h2) * (0.45 + 0.95 * grassHeight) * hScale;
            float yn = clamp((q.y - rootY) / bladeH, 0.0, 1.0);
            // layered wind; audio is an ADDITIVE phase push (never time-scaling)
            float wPh = tw * (0.9 + 0.8 * h3) + rootX * 1.4 + h4 * TAU + amt * 2.4 * midP;
            float sway = (0.13 + 0.18 * h3) * sin(wPh)
                       + 0.05 * sin(tw * 2.6 + rootX * 3.3 + h1 * 9.0);
            float lat = ((h4 - 0.5) * 0.55 + sway) * bladeH;
            float xc = rootX + lat * yn * yn;              // quadratic bend
            float bw = w * (0.22 + 0.16 * h1) * (1.0 - yn * 0.92) + 0.0010;
            float dxc = abs(xl - xc);
            float bm = smoothstep(bw + aa, bw - aa, dxc) * smoothstep(1.0, 0.985, yn);
            // dusk-lit → silhouette by layer; backlit gold rim on tip edges
            vec3 gA = mix(vec3(0.16, 0.30, 0.17), vec3(0.33, 0.45, 0.21), h1);
            vec3 gB = mix(vec3(0.045, 0.095, 0.065), vec3(0.10, 0.16, 0.095), h1);
            vec3 bcol = mix(gA, gB, fl * 0.5) * (0.62 + 0.55 * yn);
            float edge = smoothstep(0.25, 0.9, dxc / max(bw, 1e-4));
            float shimmer = amt * beatP * (0.5 + 0.5 * sin(xl * 3.0 - TIME * 1.6));
            bcol += vec3(0.98, 0.60, 0.32) * edge * smoothstep(0.40, 0.95, yn)
                    * (0.42 + 0.40 * shimmer) * exp(-abs(rootX - camX * pf - sunC.x) * 0.8);
            col = mix(col, bcol, bm);
            // wildflower head riding the blade tip — proper petaled shape
            float fOn = step(1.0 - flowerDensity * (0.18 + 0.20 * fl), hash11(id * 9.13 + fl * 5.5 + 2.0));
            if (fOn > 0.5) {
                vec2 hp = vec2(rootX + lat, rootY + bladeH);
                vec2 fd = vec2(xl, q.y) - hp;
                float hr = length(fd);
                float headR = w * (0.75 + 0.45 * h3);
                if (hr < headR * 2.4) {
                    float ang = atan(fd.y, fd.x);
                    float prad = headR * (0.62 + 0.38 * cos(5.0 * ang + h1 * TAU));
                    float pm = smoothstep(prad + aa, prad - aa, hr);
                    float h6 = hash11(id * 3.77 + fl * 13.0 + 5.0);
                    vec3 pcol = (h6 < 0.22) ? vec3(0.93, 0.28, 0.26)   // poppy
                              : (h6 < 0.44) ? vec3(0.55, 0.45, 0.92)   // cornflower
                              : (h6 < 0.62) ? vec3(0.98, 0.93, 0.80)   // daisy
                              : (h6 < 0.84) ? vec3(0.99, 0.78, 0.28)   // buttercup
                              : vec3(0.96, 0.55, 0.68);                // rose
                    pcol *= (0.75 + 0.45 * smoothstep(headR, 0.0, hr)) * (1.0 + amt * 0.5 * highP);
                    col = mix(col, pcol, pm);
                    float cm = smoothstep(headR * 0.32 + aa, headR * 0.32 - aa, hr);
                    col = mix(col, vec3(0.30, 0.17, 0.08), cm);        // dark amber heart
                    col += pcol * 0.18 * exp(-hr / max(headR, 1e-4) * 3.5); // backlit glow
                }
            }
        }
    }

    // ── golden-hour grade ───────────────────────────────────────────────
    col = mix(col, col * vec3(1.05, 0.97, 0.90), 0.45);
    col *= 1.0 - 0.10 * smoothstep(0.55, 1.05, length(q * vec2(0.75, 1.0)));
    col += 0.016 * (hash21(fc + fract(TIME) * vec2(17.0, 29.0)) - 0.5);

    if (skyShift > 0.001) col = hueRotate(col, skyShift * TAU);
    col *= brightness;
    return col;
}

// ─── multipass: trail accumulation → bloom + lens-depth composite ────────
void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        vec3 col = renderScene();
        // luminous motion trails persist in the buffer (8-bit-safe decay floor)
        vec3 prev = texture2D(abTrail, uv).rgb;
        float decay = 0.50 + 0.46 * trailAmt;
        col = max(col, prev * decay - 0.0045);
        // write-dither: keeps soft gradients band-free in 8-bit buffer hosts
        col += (hash21(gl_FragCoord.xy + fract(TIME) * 61.0) - 0.5) * 0.006;
        gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
    } else {
        // chromatic lens depth: R/B pulled apart along the radial axis
        vec2 dir = uv - 0.5;
        float ab = aberration * 0.0045;
        vec3 base;
        base.r = texture2D(abTrail, uv + dir * ab).r;
        base.g = texture2D(abTrail, uv).g;
        base.b = texture2D(abTrail, uv - dir * ab).b;
        // wide two-ring bloom of the bright field — real glow depth
        vec3 bl = vec3(0.0);
        for (int i = 0; i < 8; i++) {
            float a = float(i) * 0.7853982;
            vec2 o = vec2(cos(a), sin(a)) * (3.5 / R.y);
            bl += texture2D(abTrail, uv + o).rgb;
            bl += texture2D(abTrail, uv + o * 2.6).rgb * 0.6;
        }
        bl /= 12.8;
        bl = max(bl - 0.52, 0.0);
        vec3 col = base + bl * bl * bloomAmt * 1.8;
        // dim-tracking audio lift (bright scene → stays <= 1, post-trail)
        col *= mix(1.0, 0.64 + 0.36 * clamp(audioLevel, 0.0, 1.0), audioReact);
        // HD finish: S-curve contrast + saturation richness
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        gl_FragColor = vec4(col, 1.0);
    }
}
