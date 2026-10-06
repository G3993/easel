/*{
  "DESCRIPTION": "Element Air — the invisible made visible: ribbons of wind comb across a high pale sky in long silver streamlines, dandelion seeds and tiny leaves tumble through the currents, and soft cloud shoals dissolve at the edges of the gusts. The whole sky leans with the wind direction as it slowly veers. Mids gust the streamlines brighter and faster, bass leans the whole wind, beats loose a scatter of seeds, level lifts the light.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "windStreaks",  "LABEL": "Streamlines",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "seedAmt",      "LABEL": "Seeds & Leaves","TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "cloudAmt",     "LABEL": "Cloud Shoals",  "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.5,  "GROUP": "Shape / Geometry" },
    { "NAME": "skyHue",       "LABEL": "Sky Hue",       "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",   "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0, "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "windSpeed",    "LABEL": "Wind Speed",    "TYPE": "float", "MIN": 0.0, "MAX": 3.0, "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "audioReact",   "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.4,  "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",   "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.45, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",   "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.55, "GROUP": "Depth / Passes" },
    { "NAME": "aberration", "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0, "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// ELEMENT AIR — fourth element. Wind as flow-field streamlines: the sky
//   is sheared along a slowly veering wind direction; in sheared space,
//   thin bright filaments live on narrow fbm iso-bands advected fast
//   along-stream and slow across-stream, which reads as true streamlines
//   without any particle history buffer (analytic-trail lesson from the
//   A-List batches). Three streak scales layer for depth. Dandelion
//   seeds = tiny radial star puffs tumbling on Lissajous gusts; leaves =
//   small swaying ellipses. Cloud shoals dissolve along the same shear so
//   everything obeys one wind. Silence keeps a steady serene breeze.
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
    for (int i = 0; i < 3; i++) {
        v += a * vnoise(p);
        p = p * 2.13 + vec2(7.3, 3.1);
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
    vec2 q = (fc - 0.5 * R) / R.y;

    float t   = TIME * windSpeed * 0.5;
    float amt = audioReact;
    float bassP  = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP   = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float levelP = clamp(audioLevel, 0.0, 1.0);
    float beatP  = clamp(audioBeatPulse, 0.0, 1.0);

    // wind direction slowly veers; bass leans it harder
    float wa = 0.18 * sin(t * 0.07) + 0.10 * sin(t * 0.031 + 2.0) + amt * 0.12 * bassP;
    float ca = cos(wa), sa = sin(wa);
    vec2 w = vec2(ca * q.x - sa * q.y, sa * q.x + ca * q.y);     // sheared wind space

    // ── high pale sky ───────────────────────────────────────────────────
    vec3 col = mix(vec3(0.88, 0.93, 0.97), vec3(0.62, 0.74, 0.90),
                   smoothstep(-0.5, 0.55, q.y));
    col += vec3(0.10, 0.08, 0.04) * exp(-length(q - vec2(0.4, 0.45)) * 1.8)
         * (1.0 + amt * 0.3 * levelP);

    // ── cloud shoals stretched along the wind ───────────────────────────
    float shoal = fbm(vec2(w.x * 1.1 - t * 0.12, w.y * 4.5));
    col = mix(col, vec3(0.97, 0.98, 1.0),
              cloudAmt * 0.55 * smoothstep(0.52, 0.78, shoal));

    // ── streamlines: three scales of advected filaments ─────────────────
    float gust = 1.0 + amt * 1.1 * midP;
    for (int i = 0; i < 3; i++) {
        float fi = float(i);
        float sc = 3.0 + fi * 2.6;
        // along-stream coordinate advected fast; across-stream nearly static
        float band = fbm(vec2(w.x * 0.7 * sc - t * (0.9 + 0.4 * fi) * gust,
                              w.y * sc * 2.6 + fi * 11.0));
        float fil = smoothstep(0.50, 0.535, band) * smoothstep(0.59, 0.555, band);
        float depthFade = 1.0 - fi * 0.26;
        col += vec3(0.97, 0.98, 1.0) * fil * windStreaks * (0.20 + 0.60 * gust) * depthFade * 0.8;
        // faint cool shadow filaments give the wind body
        float fil2 = smoothstep(0.30, 0.36, band) * smoothstep(0.42, 0.36, band);
        col -= vec3(0.05, 0.04, 0.02) * fil2 * windStreaks * depthFade;
    }

    // ── dandelion seeds + leaves tumbling on gusts ──────────────────────
    for (int i = 0; i < 12; i++) {
        float fi = float(i);
        float h1 = hash11(fi * 12.99 + 1.0);
        float h2 = hash11(fi * 4.41 + 7.0);
        float h3 = hash11(fi * 7.77 + 3.0);
        float scatter = 1.0 + amt * 1.0 * beatP * step(0.5, h2);
        float lifeT = t * (0.10 + 0.06 * h1) * scatter;
        vec2 sp = vec2(mix(-0.95, 0.95, fract(h1 + lifeT * 0.7)),
                       (h2 * 2.0 - 1.0) * 0.42 + 0.10 * sin(t * (0.6 + h3) + fi * 2.0));
        // lean the travel along the wind
        sp = vec2(ca * sp.x + sa * sp.y, -sa * sp.x + ca * sp.y);
        vec2 d = q - sp;
        float r = length(d);
        if (r > 0.06) continue;
        if (h3 < 0.6) {
            // dandelion puff: tiny radial star
            float th = atan(d.y, d.x);
            float puff = smoothstep(0.020 * (0.6 + h1), 0.0, r)
                       * (0.55 + 0.45 * pow(abs(cos(th * 6.0 + fi)), 2.0));
            col += vec3(1.0, 1.0, 0.98) * puff * seedAmt * 0.8;
        } else {
            // little leaf: swaying ellipse
            float rot = t * (1.0 + h1) + fi;
            float cb = cos(rot), sb = sin(rot);
            vec2 lp = vec2(cb * d.x - sb * d.y, sb * d.x + cb * d.y);
            float e = length(lp / vec2(0.020, 0.008));
            col = mix(col, vec3(0.45, 0.60, 0.30), smoothstep(1.0, 0.7, e) * seedAmt);
        }
    }

    // ── finish ──────────────────────────────────────────────────────────
    float gr = hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5;
    col += 0.018 * gr;
    if (skyHue > 0.001) col = hueRotate(col, skyHue * TAU);
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
