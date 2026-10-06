/*{
  "DESCRIPTION": "Bent Planes — four glossy gradient membranes twisted through black space, each an infinity-ribbon that folds over itself showing front and back faces in different color worlds, exactly the reference poster. The ribbons undulate, the twist travels along them, and their silk edges catch white rim light. Bass deepens the fold, mids pour the twist along the ribbons, beats flash the rims, highs sharpen the gloss.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "ribbonCount", "LABEL": "Ribbons",       "TYPE": "float", "MIN": 2.0, "MAX": 5.0,  "DEFAULT": 4.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "ribbonSize",  "LABEL": "Ribbon Size",   "TYPE": "float", "MIN": 0.5, "MAX": 1.6,  "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "foldAmt",     "LABEL": "Fold Depth",    "TYPE": "float", "MIN": 0.2, "MAX": 1.0,  "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "twistSpeed",  "LABEL": "Twist Speed",   "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.4,  "GROUP": "Motion / Animation" },
    { "NAME": "waveSpeed",   "LABEL": "Wave Speed",    "TYPE": "float", "MIN": 0.0, "MAX": 2.0,  "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.12, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.55, "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.4,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "bpScene" },
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// BENT PLANES — after the black-field twisted-membrane poster.
//   Each ribbon is an analytically-projected twisted band: its long axis
//   runs across the screen on an S-curve spine c(x); about that axis the
//   band twists with angle θ(x), so the projected half-height is
//   H·|cos θ| — pinching to a silk edge where cos θ crosses zero, which
//   is exactly the reference's folded waists. Front face (cos θ > 0)
//   and back face carry different gradient worlds; the position along
//   the band (v) and along the screen (x) index a 2D gradient so color
//   pours diagonally like airbrushed silk. Lighting: face normal from
//   (dθ/dx, v) drives a broad sheen + a pow-28 specular streak, plus a
//   white fresnel rim at |v|→1 and at the pinch points. Ribbons render
//   back-to-front (upper = further). 3 passes: scene → trail → bloom.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float hash11(float n) { return fract(sin(n) * 43758.5453123); }

vec3 vivid(float h) {
    return clamp(vec3(0.5 + 0.62 * cos(TAU * (h + 0.00)),
                      0.5 + 0.62 * cos(TAU * (h + 0.33)),
                      0.5 + 0.62 * cos(TAU * (h + 0.66))), 0.0, 1.0);
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

    vec3 col = vec3(0.008, 0.006, 0.010);          // deep black field
    float N = floor(ribbonCount + 0.5);
    float fold = foldAmt * (0.8 + amt * 0.35 * bassP);

    // render back-to-front: ribbon 0 = top/back … N-1 = bottom/front
    for (int i = 0; i < 5; i++) {
        if (float(i) >= N) break;
        float fi = float(i);
        float hz = hash11(fi * 5.17);

        // vertical slot for this ribbon
        float y0 = (0.5 - (fi + 0.5) / N) * 1.12;
        float x = q.x;

        // spine: S-curve, undulating
        float wph = t * waveSpeed * 0.5 + fi * 1.9;
        float c = y0
                + 0.16 * ribbonSize * sin(x * 3.1 + wph + hz * TAU)
                + 0.05 * ribbonSize * sin(x * 6.4 - wph * 0.7);

        // twist angle traveling along the band (mid = phase push)
        float tph = t * twistSpeed * 0.6 + amt * 0.7 * midP;
        float th = x * (2.2 + 0.8 * hz) * fold * 2.0 + tph + fi * 2.3;
        float ct = cos(th);
        float H = 0.23 * ribbonSize * (0.75 + 0.25 * sin(x * 1.3 + fi));
        float Hp = max(H * abs(ct), 0.012);         // projected half-height

        float v = (q.y - c) / Hp;                   // -1..1 across the band
        if (abs(v) > 1.0) continue;

        float aa = 1.6 / (R.y * Hp);
        float mask = smoothstep(1.0, 1.0 - aa * 2.0, abs(v));

        // which face is showing
        float face = step(0.0, ct);

        // ── silk gradient worlds ────────────────────────────────────────
        float hueBase = hueShift + hz + fi * 0.13;
        float u = x * 0.55 + v * 0.25;              // diagonal pour coord
        vec3 cFront = mix(vivid(hueBase),        vivid(hueBase + 0.18), clamp(u + 0.5, 0.0, 1.0));
        vec3 cBack  = mix(vivid(hueBase + 0.45), vivid(hueBase + 0.62), clamp(0.5 - u, 0.0, 1.0));
        vec3 base = mix(cBack, cFront, face);

        // ── lighting: normal from twist slope + band coordinate ─────────
        // approximate surface normal z from |cosθ| (flat-on = 1, edge = 0)
        float nz = abs(ct);
        float sheen = 0.55 + 0.55 * nz;
        base *= sheen;
        // specular streak running along the band where the face turns
        float spec = pow(clamp(1.0 - abs(v + 0.35 * sin(th)), 0.0, 1.0), 6.0)
                   * pow(nz, 1.5);
        base += vec3(1.0, 0.97, 0.94) * spec * (0.35 + amt * 0.3 * highP);
        // fresnel rim: white silk edge at the band border and pinch waists
        float rim = pow(1.0 - abs(v) * abs(v), 8.0) * 0.0
                  + pow(clamp(1.0 - nz, 0.0, 1.0), 2.5) * 0.9
                  + pow(clamp(abs(v), 0.0, 1.0), 14.0) * 0.9;
        rim *= 0.55 + amt * 0.8 * beatP;
        base += vec3(1.0) * rim * 0.55;

        // gentle ambient occlusion where band overlaps itself (dim backs)
        base *= mix(0.72, 1.0, face * 0.5 + 0.5 * nz);

        // shadow the field just beneath the ribbon (depth against black)
        float under = smoothstep(1.6, 1.0, abs(v));
        col *= mix(1.0, 0.5, under * 0.4 * mask);

        col = mix(col, base, mask);
    }

    // level breathes the whole silk stack (dark field → safe additive)
    col *= 1.0 + amt * 0.22 * clamp(audioLevel, 0.0, 1.0);

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.014;
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        gl_FragColor = vec4(clamp(renderScene(), 0.0, 1.0), 1.0);
    } else if (PASSINDEX == 1) {
        vec3 col = texture2D(bpScene, uv).rgb;
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
