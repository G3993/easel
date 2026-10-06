/*{
  "DESCRIPTION": "Candy Flames — Y2K glossy pink flames licking up over a pastel rainbow haze, every tongue outlined in liquid chrome with lens-flare sparkles twinkling on the tips. Teardrop flames flick and sway, drips fall from the row, and star glints rotate as they breathe. Flame color is yours. Bass makes the flames rear taller, mids sway them, beats pop fresh sparkles, highs spin the glints.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "flameCount",  "LABEL": "Flame Tongues", "TYPE": "float", "MIN": 4.0, "MAX": 14.0, "DEFAULT": 10.0, "GROUP": "Shape / Geometry" },
    { "NAME": "flameTall",   "LABEL": "Flame Height",  "TYPE": "float", "MIN": 0.4, "MAX": 1.4,  "DEFAULT": 0.9,  "GROUP": "Shape / Geometry" },
    { "NAME": "sparkleAmt",  "LABEL": "Sparkles",      "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.7,  "GROUP": "Shape / Geometry" },
    { "NAME": "lickSpeed",   "LABEL": "Flicker Speed", "TYPE": "float", "MIN": 0.0, "MAX": 2.0,  "DEFAULT": 0.7,  "GROUP": "Motion / Animation" },
    { "NAME": "swayAmt",     "LABEL": "Sway",          "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "flameColor",  "LABEL": "Flame Color",   "TYPE": "color", "DEFAULT": [0.98, 0.25, 0.62, 1.0], "GROUP": "Color" },
    { "NAME": "hueShift",    "LABEL": "Rainbow Shift", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.12, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}
float hash11(float n) { return fract(sin(n) * 43758.5453123); }

vec3 pastel(float h) {
    return vec3(0.72 + 0.28 * cos(TAU * (h + 0.00)),
                0.72 + 0.28 * cos(TAU * (h + 0.33)),
                0.72 + 0.28 * cos(TAU * (h + 0.66)));
}

float smin(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

// Signed distance to a single flame tongue with a pointed tip.
// The tongue is modeled as a tapered lozenge whose top is pinched
// to a sharp cusp (cone-like) rather than a blunt cap.
float flameTongueSDF(vec2 p, float x0, float hgt, float phase,
                     float swayS, float lick, float hx) {
    // Normalised height along the tongue spine: 0=base, 1=tip
    float yy = clamp((p.y + 0.55) / hgt, 0.0, 1.0);

    // Spine x with S-curve sway
    float sx = x0
             + swayS * 0.10 * sin(yy * 5.0 + phase) * yy
             + swayS * 0.05 * sin(yy * 11.0 - phase * 1.7) * yy * yy;

    // Width profile: fat teardrop base tapering aggressively toward tip.
    // Using a higher power (2.5) makes the upper part narrow much faster
    // so the silhouette comes to a real point instead of a blunt end.
    float wid = 0.10 * (0.55 + 0.45 * hx) * pow(1.0 - yy, 2.5) + 0.004;
    // Teardrop bulge at the very base
    wid += 0.085 * exp(-yy * 5.0) * (0.65 + 0.35 * sin(phase * 1.3));

    // Horizontal distance from the spine (signed, positive = outside)
    float dx = abs(p.x - sx) - wid;

    // --- Pointy tip cap ---
    // Instead of a flat horizontal cap we build a cone: the "cap" distance
    // is the distance to a V-shaped wedge that meets at the tip position.
    // tipY is the actual tip height; above it we measure distance to the
    // spine point at the tip (pure radial), weighted to form a sharp cone.
    float tipY = -0.55 + hgt * (1.0 + 0.06 * sin(lick * 2.0 + phase));

    // The half-angle of the cone is derived from the width at yy=0.98 so
    // the cone is tangent to the tapering body — seamless join.
    // cone: d = len(p - tip) scaled to match taper slope
    vec2 toTip = p - vec2(sx, tipY);
    // project onto spine axis (vertical) and lateral
    float axial   = -toTip.y;          // positive above tip
    float lateral = abs(toTip.x);

    // Cone half-slope derived from width profile derivative at tip
    // w(yy)=0.10*(0.55+0.45*hx)*(1-yy)^2.5, dw/dyy at yy=1 → 0
    // We use the width just below the tip for a shallow cone angle.
    float nearTipWid = 0.10 * (0.55 + 0.45 * hx) * pow(0.04, 2.5) + 0.004;
    float coneSlope = nearTipWid / (hgt * 0.04 + 0.001); // rise/run

    // SDF of cone above the tip: lateral - coneSlope*axial (positive outside)
    float dCone = lateral - coneSlope * axial;

    // Below tip level: normal body taper handles it; cap only fires above tip
    float dTop;
    if (p.y > tipY) {
        // Above tip: use cone distance (gives sharp pointed termination)
        dTop = max(dCone, axial * 0.0); // just cone, axial>=0 already guaranteed
        // Also blend in a soft radial distance so the very apex is smooth
        float radial = length(toTip) - 0.006;
        dTop = smin(dTop, radial, 0.005);
    } else {
        // Below tip: uncapped — only the width profile controls the shape
        dTop = -1e3; // never the max
    }

    return max(dx, dTop);
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;
    vec2 uv = fc / R;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid),  1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float t = TIME * motionSpeed;

    // ── background ───────────────────────────────────────────────────────
    vec3 hotPink = flameColor.rgb * vec3(1.02, 0.55, 0.95) + vec3(0.0, 0.08, 0.12);
    vec3 bg = pastel(hueShift + uv.y * 0.55 + 0.05 * sin(t * 0.1)) * 0.85;
    for (int i = 0; i < 4; i++) {
        float fi = float(i);
        vec2 c = vec2(sin(t * 0.07 + fi * 2.1) * 0.4, -0.35 + 0.2 * sin(t * 0.05 + fi * 1.3));
        float d = length(q - c);
        vec3 blobC = pastel(hueShift + fi * 0.21 + 0.1 * t * 0.02) * vec3(1.05, 0.95, 0.9);
        bg = mix(bg, blobC, 0.5 * exp(-d * d * 7.0));
    }
    bg = mix(bg, hotPink, smoothstep(-0.05, 0.28, q.y + 0.05 * sin(q.x * 3.0 + t * 0.2)));

    // ── merged flame tongue field ─────────────────────────────────────────
    float N    = floor(flameCount + 0.5);
    float tall = flameTall * (1.0 + amt * 0.30 * bassP);
    float lick = t * lickSpeed;
    float sway = swayAmt * (0.6 + amt * 0.5 * midP);

    float dmin   = 1e3;
    float tipGlow = 0.0;

    for (int i = 0; i < 14; i++) {
        if (float(i) >= N) break;
        float fi  = float(i);
        float hx  = hash11(fi * 7.31);
        float x0  = (fi + 0.5) / N * 2.0 - 1.0;
        x0 *= 0.5 * R.x / R.y * 1.9 / max(R.x / R.y, 1.0);
        float hgt   = tall * (0.72 + 0.38 * hx) * 0.95;
        float phase = hx * TAU + lick * (0.7 + 0.5 * hx);

        float d = flameTongueSDF(q, x0, hgt, phase, sway, lick, hx);
        dmin = smin(dmin, d, 0.035);

        // tip position for sparkles
        float tipY = -0.55 + hgt;
        float syTip = x0 + sway * 0.10 * sin(5.0 + phase);
        float td = length(q - vec2(syTip, tipY));
        tipGlow += exp(-td * td * 400.0);
    }

    float aa    = 1.6 / R.y;
    float flame = smoothstep(aa, -aa, dmin);

    // ── candy flame body + chrome edges ──────────────────────────────────
    vec3 fC    = flameColor.rgb;
    float yGrad = clamp((q.y + 0.55) / 1.1, 0.0, 1.0);
    vec3 body  = fC * (0.78 + 0.22 * yGrad);
    body = mix(body, fC * vec3(0.92, 0.6, 0.95), yGrad * 0.4);
    float sheen = exp(-abs(dmin + 0.018) * 220.0);
    body += vec3(1.0, 0.85, 0.97) * sheen * 0.22;
    float rimIn = exp(-abs(dmin + 0.005) * 420.0);
    body += vec3(1.0) * rimIn * 0.45;
    body *= 1.0 + amt * 0.18 * clamp(audioLevel, 0.0, 1.0);

    vec3 col = mix(bg, body, flame);

    float stroke = exp(-abs(dmin) * 520.0);
    col += vec3(1.0, 0.96, 1.0) * stroke * 0.55;
    col += fC * exp(-max(dmin, 0.0) * 22.0) * 0.10;

    // ── star sparkles ─────────────────────────────────────────────────────
    float spark = 0.0;
    for (int i = 0; i < 10; i++) {
        float fi = float(i);
        float h1 = hash11(fi * 3.77);
        float h2 = hash11(fi * 9.13);
        float tw = fract(t * (0.15 + 0.2 * h1) + h2);
        float env = smoothstep(0.0, 0.25, tw) * smoothstep(1.0, 0.55, tw);
        env *= 1.0 + amt * 1.2 * beatP;
        vec2 sp = vec2(h1 * 2.0 - 1.0, h2 * 0.9 - 0.35);
        sp.x *= 0.5 * R.x / R.y * 1.8 / max(R.x / R.y, 1.0);
        vec2 dv = q - sp;
        float ra = t * (0.3 + amt * 0.5 * highP) + fi;
        dv = mat2(cos(ra), -sin(ra), sin(ra), cos(ra)) * dv;
        float core = exp(-dot(dv, dv) * 2600.0);
        float rays = exp(-abs(dv.x) * 300.0) * exp(-abs(dv.y) * 26.0)
                   + exp(-abs(dv.y) * 300.0) * exp(-abs(dv.x) * 26.0);
        spark += (core * 0.8 + rays * 0.3) * env;
    }
    col += vec3(1.0, 0.98, 1.0) * spark * sparkleAmt * 0.7;
    col += vec3(1.0, 0.9, 1.0) * tipGlow * sparkleAmt * (0.03 + amt * 0.10 * highP);

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.016;
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        vec3 col  = renderScene();
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