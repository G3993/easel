/*{
  "DESCRIPTION": "Sphere Rows — the wall of iridescent orbs: overlapping rows of glossy 3D spheres on black, every one carrying its own drifting multicolor gradient skin, lit with real diffuse, specular and fresnel. Rows bob softly, gradient skins rotate, and the wall breathes. Bass swells the spheres, mids spin the skins, beats flash a roaming orb, highs glint the speculars.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "rowCount",    "LABEL": "Rows",          "TYPE": "float", "MIN": 4.0, "MAX": 12.0, "DEFAULT": 8.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "sphereSize",  "LABEL": "Sphere Size",   "TYPE": "float", "MIN": 0.7, "MAX": 1.4,  "DEFAULT": 1.05, "GROUP": "Shape / Geometry" },
    { "NAME": "overlapAmt",  "LABEL": "Row Overlap",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.6,  "GROUP": "Shape / Geometry" },
    { "NAME": "bobSpeed",    "LABEL": "Row Bob",       "TYPE": "float", "MIN": 0.0, "MAX": 2.0,  "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "skinSpin",    "LABEL": "Gradient Spin", "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "hueShift",    "LABEL": "Hue Shift",     "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.1,  "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.35, "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "srScene" },
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// SPHERE ROWS — after the iridescent orb-grid reference.
//   Rows of spheres stacked with vertical overlap (later rows sit in
//   front, like the reference where each row eclipses the one above).
//   Each sphere: normal from its circle footprint, a gradient SKIN
//   built from 3 hashed hue anchors mixed by a rotated surface
//   coordinate (so every orb has its own color weather, like the wall
//   of dyed marbles), lit by fixed key light: lambert + pow-30 spec +
//   fresnel picking up neighbor glow. Rows bob on sine phases; a
//   roaming highlight orb flashes on beats. 3-pass.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

vec3 vivid(float h) {
    return clamp(vec3(0.5 + 0.6 * cos(TAU * (h + 0.00)),
                      0.5 + 0.6 * cos(TAU * (h + 0.33)),
                      0.5 + 0.6 * cos(TAU * (h + 0.66))), 0.0, 1.0);
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = fc / R.y;                       // y in [0, 1], x in [0, aspect]
    float asp = R.x / R.y;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float levP  = clamp(audioLevel, 0.0, 1.0);
    float t = TIME * motionSpeed;

    vec3 col = vec3(0.012, 0.010, 0.016);

    float rows = floor(rowCount + 0.5);
    float rowH = 1.0 / (rows - (rows - 1.0) * 0.28 * overlapAmt);   // overlap squeeze
    float rad = 0.5 * rowH * sphereSize * (1.0 + amt * 0.06 * bassP);
    float nx = floor(asp / rowH + 1.0);      // spheres per row

    // walk rows front-most LAST (bottom rows in front, like the wall)
    for (int ry = 0; ry < 12; ry++) {
        if (float(ry) >= rows) break;
        float fy = float(ry);
        float cy = 1.0 - (fy + 0.5) * rowH * (1.0 - 0.28 * overlapAmt * step(0.5, fy));
        // actually: uniform stacking with overlap
        cy = 1.0 - rowH * 0.5 - fy * rowH * (1.0 - 0.28 * overlapAmt);
        cy += 0.012 * sin(t * bobSpeed * 0.8 + fy * 1.7) ;

        for (int rx = 0; rx < 14; rx++) {
            if (float(rx) >= nx) break;
            float fx = float(rx);
            float cx = (fx + 0.5) * rowH + 0.014 * sin(t * bobSpeed * 0.6 + fx * 2.1 + fy);
            vec2 c = vec2(cx, cy);
            vec2 d = q - c;
            float r = length(d);
            if (r > rad) continue;

            vec2 id = vec2(fx, fy);
            float h1 = hash21(id * 3.7 + 1.0);
            float h2 = hash21(id * 7.1 + 5.0);

            // sphere normal
            float z = sqrt(max(rad * rad - r * r, 0.0)) / rad;
            vec3 nrm = normalize(vec3(d / rad, z));

            // gradient skin: rotated surface coordinate mixes 3 hues
            float spin = t * skinSpin * (0.3 + 0.4 * h1) + h1 * TAU + amt * 0.5 * midP;
            vec2 sd = mat2(cos(spin), -sin(spin), sin(spin), cos(spin)) * (d / rad);
            float u1 = sd.x * 0.5 + 0.5;
            float u2 = sd.y * 0.5 + 0.5;
            vec3 skin = mix(vivid(hueShift + h1),       vivid(hueShift + h1 + 0.31), u1);
            skin = mix(skin, vivid(hueShift + h2 + 0.6), u2 * 0.6);

            // lighting
            vec3 L = normalize(vec3(0.35, 0.5, 0.78));
            float dif = clamp(dot(nrm, L), 0.0, 1.0);
            float spe = pow(clamp(dot(reflect(-L, nrm), vec3(0.0, 0.0, 1.0)), 0.0, 1.0), 30.0);
            float fre = pow(1.0 - z, 2.2);
            vec3 sc = skin * (0.30 + 0.85 * dif);
            sc += vec3(1.0) * spe * (0.5 + amt * 0.5 * highP);
            sc += skin.bgr * fre * 0.35;                      // neighbor-glow rim
            // beat: roaming flash orb
            float pick = hash21(id + floor(t * 1.2));
            sc *= 1.0 + step(0.985, pick) * amt * beatP * 0.8;
            // level breathes the whole wall
            sc *= 1.0 + amt * 0.20 * levP;

            // AA edge against what's behind
            float aa = 1.5 / R.y;
            float m = smoothstep(rad, rad - aa * 2.0, r);
            col = mix(col, sc, m);
        }
    }

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.015;
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        gl_FragColor = vec4(clamp(renderScene(), 0.0, 1.0), 1.0);
    } else if (PASSINDEX == 1) {
        vec3 col = texture2D(srScene, uv).rgb;
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
