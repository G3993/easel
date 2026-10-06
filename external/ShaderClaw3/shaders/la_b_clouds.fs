/*{
  "DESCRIPTION": "La B Clouds — a golden-hour dusk cloudscape with real volume: self-shadowed cloud banks in deep rose, terracotta and plum rolling slowly across a saturated gradient sky, gold-lined edges facing a low sun, and warm god-ray shafts breaking through the gaps. Bass swells the cloud mass, mids pour the drift, beats send a soft gleam through the sun and shafts.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "cloudScale",   "LABEL": "Cloud Scale",   "TYPE": "float", "MIN": 0.5, "MAX": 2.4, "DEFAULT": 1.15, "GROUP": "Shape / Geometry" },
    { "NAME": "cloudDense",   "LABEL": "Cloud Cover",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.62, "GROUP": "Shape / Geometry" },
    { "NAME": "sunHeight",    "LABEL": "Sun Height",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.38, "GROUP": "Shape / Geometry" },
    { "NAME": "rayAmt",       "LABEL": "Light Shafts",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "paletteShift", "LABEL": "Palette Shift", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "motionSpeed",  "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.05, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.25, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.2,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// LA B CLOUDS — premium dusk cloudscape backdrop for La Bloom.
//   NOT a pastel wash: golden-hour saturation. Deep plum zenith falling
//   through rich rose to a terracotta horizon around a low gold sun.
//   Two cloud strata (far bank + hero bank) with REAL volume: density is
//   re-sampled toward the sun for self-shadowing (lit faces go gold, the
//   shadow cores go deep plum-brown), and edges facing the sun get a hot
//   gold lining. Warm god-ray shafts are marched from each pixel toward
//   the sun through a cheap occluder field, striated by angle so beams
//   read as beams. Slow, majestic, layered drift — never a loop.
//   Audio: bass swells cloud mass (density amplitude), mids push the
//   drift phase (additive, chop-free), beats bloom a soft gleam through
//   sun + shafts + linings; the global level lift lives in the composite.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

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
float fbm2(vec2 p) {
    float v = 0.0, a = 0.55;
    for (int i = 0; i < 2; i++) {
        v += a * vnoise(p);
        p = p * 2.17 + vec2(5.2, 1.3);
        a *= 0.5;
    }
    return v * 1.23;
}
float fbm5(vec2 p) {
    float v = 0.0, a = 0.5;
    for (int i = 0; i < 5; i++) {
        v += a * vnoise(p);
        p = p * 2.07 + vec2(3.7, 7.1);
        a *= 0.52;
    }
    return v * 1.05;
}
vec3 hueShift(vec3 c, float a) {
    vec3 k = vec3(0.57735);
    float cs = cos(a), sn = sin(a);
    return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
}

// full 5-octave cloud density with coverage shaping (heavier low in frame)
float cloudField(vec2 p, float tw, float densAmp) {
    vec2 w = p - vec2(tw * 0.055, 0.0);
    w += 0.30 * vec2(fbm2(p * 0.8 + vec2(tw * 0.030, 1.7)) - 0.5,
                     fbm2(p * 0.8 + vec2(6.1, -tw * 0.022)) - 0.5) * 2.0;
    float d = fbm5(w * (1.35 * cloudScale));
    float cov = clamp(cloudDense * densAmp, 0.0, 1.2);
    float bias = mix(0.66, 0.36, cov * 0.83);
    return smoothstep(bias, bias + 0.30, d + 0.13 * (0.15 - p.y));
}
// cheap 2-octave occluder for the god-ray march (same drift transform)
float cloudOcc(vec2 p, float tw, float densAmp) {
    vec2 w = p - vec2(tw * 0.055, 0.0);
    float d = fbm2(w * (1.35 * cloudScale) + vec2(1.9, 0.4));
    float cov = clamp(cloudDense * densAmp, 0.0, 1.2);
    float bias = mix(0.62, 0.34, cov * 0.83);
    return smoothstep(bias, bias + 0.38, d + 0.13 * (0.15 - p.y));
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * motionSpeed * 0.5;
    float amt = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);

    float densAmp = 1.0 + amt * 0.26 * bassP;      // bass: slow mass swell
    float tw = t + amt * 0.6 * midP;               // mid: additive drift push
    float gleam = amt * beatP;                     // beat: soft light gleam

    vec2 sunP = vec2(0.30, mix(-0.10, 0.30, sunHeight));
    vec2 toSun = sunP - q;
    float dSun = length(toSun);
    vec2 sd = toSun / max(dSun, 1e-4);

    // ── golden-hour gradient sky: plum → rose → terracotta ─────────────
    float sy = clamp(q.y + 0.5, 0.0, 1.3);
    vec3 zen = vec3(0.14, 0.055, 0.20);            // deep plum zenith
    vec3 ros = vec3(0.47, 0.13, 0.245);            // rich rose mid
    vec3 hor = vec3(0.80, 0.31, 0.155);            // terracotta horizon
    vec3 sky = mix(hor, ros, smoothstep(0.02, 0.48, sy));
    sky = mix(sky, zen, smoothstep(0.42, 1.0, sy));
    // sun: broad gold ambience + tight hot core
    vec3 sunGold = vec3(1.0, 0.64, 0.26);
    sky += sunGold * exp(-dSun * dSun * 8.0) * (0.80 + 0.28 * gleam);
    sky += vec3(1.0, 0.87, 0.58) * exp(-dSun * 46.0) * 1.5;
    // subtle high-alt shimmer (highs)
    sky += vec3(0.30, 0.12, 0.20) * fbm2(q * 6.0 + vec2(0.0, tw * 0.03))
         * smoothstep(0.35, 1.0, sy) * (0.16 + 0.10 * amt * highP);

    // ── far cloud bank: dimmer, smaller structures, behind the rays ────
    vec2 fp = q * 1.85 + vec2(3.3, 1.05);
    float dF  = cloudField(fp, tw * 0.62, densAmp * 0.92);
    float dFs = cloudField(fp + sd * 0.10, tw * 0.62, densAmp * 0.92);
    float litF = exp(-dFs * 2.1);
    vec3 farShad = vec3(0.255, 0.10, 0.21);
    vec3 farLit  = vec3(0.93, 0.47, 0.325);
    vec3 farC = mix(farShad, farLit, clamp(litF * 0.95, 0.0, 1.0));
    farC += vec3(1.0, 0.74, 0.44) * clamp((dF - dFs) * 2.6, 0.0, 1.0) * 0.45;
    float aF = smoothstep(0.02, 0.55, dF) * 0.78;
    vec3 col = mix(sky, farC, aF);

    // ── god rays: 16-tap march toward the sun through the occluder ─────
    if (rayAmt > 0.002) {
        float trans = 0.0;
        vec2 dv = toSun / 16.0;
        for (int i = 0; i < 16; i++) {
            vec2 sp = q + dv * (float(i) + 0.5);
            trans += 1.0 - cloudOcc(sp, tw, densAmp);
        }
        trans /= 16.0;
        float ang = atan(toSun.y, toSun.x);
        float striate = 0.42 + 0.58 * vnoise(vec2(ang * 9.0 + 4.0, 2.0 + tw * 0.07));
        float ray = pow(clamp(trans, 0.0, 1.0), 2.2) * exp(-dSun * 1.35) * striate;
        col += sunGold * ray * rayAmt * (0.60 + 0.40 * gleam);
    }

    // ── hero cloud bank: big volumes, 2-tap self-shadow, gold lining ───
    vec2 hp = q * 1.02 + vec2(0.0, -0.10);
    float dH  = cloudField(hp, tw, densAmp);
    float dH1 = cloudField(hp + sd * 0.062, tw, densAmp);
    float dH2 = cloudField(hp + sd * 0.150, tw, densAmp);
    float litH = exp(-(dH1 * 1.55 + dH2 * 0.85));
    float micro = fbm2(hp * 9.0 - vec2(tw * 0.11, 0.0));      // volume grit
    vec3 hShad = vec3(0.205, 0.075, 0.165);                   // deep plum core
    vec3 hMid  = vec3(0.615, 0.225, 0.245);                   // rose body
    vec3 hLit  = vec3(1.0, 0.685, 0.42);                      // golden lit face
    vec3 hc = mix(hShad, hMid, clamp(litH * 1.45, 0.0, 1.0));
    hc = mix(hc, hLit, clamp(litH * litH * 1.25, 0.0, 1.0) * (0.72 + 0.28 * micro));
    // hot gold lining on sun-facing edges — beats make it bloom softly
    hc += vec3(1.0, 0.815, 0.52) * clamp((dH - dH1) * 3.2, 0.0, 1.0)
        * (0.55 + 0.45 * gleam);
    hc *= 0.90 + 0.18 * micro;
    float aH = smoothstep(0.025, 0.55, dH);
    col = mix(col, hc, aH);

    // ── foreground wisps: thin stretched veils crossing the bottom ─────
    vec2 wq = vec2(q.x * 0.7 - tw * 0.085, q.y * 3.4 + 1.35);
    float wisp = fbm2(wq * vec2(2.1, 1.0));
    wisp = smoothstep(0.62, 0.95, wisp) * smoothstep(0.05, -0.32, q.y);
    col = mix(col, vec3(0.30, 0.10, 0.185), wisp * 0.55);
    col += sunGold * wisp * exp(-dSun * 2.0) * 0.22;

    // ── finish: vignette, palette rotation, grain ──────────────────────
    col *= 1.0 - 0.30 * smoothstep(0.55, 1.15, length(q * vec2(0.82, 1.0)));
    col = hueShift(col, paletteShift * TAU);
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.014 * gr;
    return max(col, 0.0);
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
        // audio lift applied post-trail so dips stay visible
        float lvl = clamp(audioLevel, 0.0, 1.0);
        col *= brightness * mix(1.0, 0.64 + 0.36 * lvl, audioReact)
             * (1.0 + audioReact * 0.06 * clamp(audioBeatPulse, 0.0, 1.0));
        // HD finish: S-curve contrast + saturation richness
        col = clamp(col, 0.0, 1.0);
        col = mix(col, col * col * (3.0 - 2.0 * col), 0.55);
        float lumHD = dot(col, vec3(0.299, 0.587, 0.114));
        col = clamp(mix(vec3(lumHD), col, 1.22), 0.0, 1.0);
        gl_FragColor = vec4(col, 1.0);
    }
}
