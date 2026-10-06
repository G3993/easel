/*{
  "DESCRIPTION": "Tube Loops — the retro supergraphic: a fat rainbow-striped tube snaking through two circular hubs with sun-disc cores, pink-outlined, over a split field of black-and-white stripes and a polka-dot wall. The rainbow flows along the tube, hub cores spin, the dots pulse random candy colors, and the stripe field scrolls. Bass pumps the tube width, mids pour the rainbow, beats pop dots, highs sparkle the outline.",
  "CATEGORIES": ["Generator", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "tubeWidth",   "LABEL": "Tube Width",    "TYPE": "float", "MIN": 0.5, "MAX": 1.4,  "DEFAULT": 1.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "stripeN",     "LABEL": "Tube Stripes",  "TYPE": "float", "MIN": 5.0, "MAX": 11.0, "DEFAULT": 8.0,  "GROUP": "Shape / Geometry" },
    { "NAME": "dotScale",    "LABEL": "Dot Density",   "TYPE": "float", "MIN": 6.0, "MAX": 18.0, "DEFAULT": 11.0, "GROUP": "Shape / Geometry" },
    { "NAME": "flowSpeed",   "LABEL": "Rainbow Flow",  "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.5,  "GROUP": "Motion / Animation" },
    { "NAME": "scrollSpeed", "LABEL": "Field Scroll",  "TYPE": "float", "MIN": -2.0, "MAX": 2.0, "DEFAULT": 0.3,  "GROUP": "Motion / Animation" },
    { "NAME": "motionSpeed", "LABEL": "Motion Speed",  "TYPE": "float", "MIN": 0.0, "MAX": 3.0,  "DEFAULT": 1.0,  "GROUP": "Motion / Animation" },
    { "NAME": "hueShift",    "LABEL": "Rainbow Shift", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.0,  "GROUP": "Color" },
    { "NAME": "brightness",  "LABEL": "Brightness",    "TYPE": "float", "MIN": 0.3, "MAX": 2.0,  "DEFAULT": 1.0,  "GROUP": "Color" },
    { "NAME": "audioReact",  "LABEL": "Audio React",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.45, "GROUP": "Audio Reactivity" },
    { "NAME": "trailAmt",    "LABEL": "Motion Trails", "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.06, "GROUP": "Depth / Passes" },
    { "NAME": "bloomAmt",    "LABEL": "Bloom Depth",   "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" },
    { "NAME": "aberration",  "LABEL": "Lens Depth",    "TYPE": "float", "MIN": 0.0, "MAX": 1.0,  "DEFAULT": 0.3,  "GROUP": "Depth / Passes" }
  ],
  "PASSES": [
    { "TARGET": "abTrail", "PERSISTENT": true },
    { }
  ]
}*/

// ─────────────────────────────────────────────────────────────────────────
// TUBE LOOPS — after the pop 'qb' supergraphic reference.
//   Background split like the poster: left field = horizontal b/w
//   stripes, right field = a wall of white dots on black where hashed
//   dots light up in candy colors. The tube: distance to a path made of
//   two arcs (each wrapping one hub) joined by a straight diagonal run,
//   built from smooth-min of arc/segment SDFs. Signed distance ACROSS
//   the tube indexes the rainbow stripe bands (black-edged, pink halo
//   outline), and the ALONG-path coordinate flows so the rainbow pours
//   through the loops. Hubs: half-and-half sun discs that spin inside
//   pink rings.
// ─────────────────────────────────────────────────────────────────────────

#define R RENDERSIZE.xy
#define TAU 6.2831853

float hash21(vec2 p) {
    p = fract(p * vec2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return fract(p.x * p.y);
}

vec3 rainbow6(float k, float hs) {
    float s = fract(k);
    vec3 pinkO = vec3(0.95, 0.62, 0.85);
    vec3 red   = vec3(0.90, 0.12, 0.15);
    vec3 orng  = vec3(0.98, 0.55, 0.10);
    vec3 yell  = vec3(0.99, 0.85, 0.10);
    vec3 grn   = vec3(0.10, 0.65, 0.35);
    vec3 blu   = vec3(0.15, 0.45, 0.85);
    vec3 blk   = vec3(0.08, 0.07, 0.08);
    float v = s * 7.0;
    vec3 c = pinkO;
    c = v < 1.0 ? pinkO : (v < 2.0 ? red : (v < 3.0 ? orng : (v < 4.0 ? yell : (v < 5.0 ? grn : (v < 6.0 ? blu : blk)))));
    // hue shift rotates the color stripes subtly
    float an = hs * TAU;
    vec3 kk = vec3(0.57735);
    return c * cos(an) + cross(kk, c) * sin(an) + kk * dot(kk, c) * (1.0 - cos(an));
}

// distance + along-coordinate to an arc of circle (center c, radius rad,
// from angle a0 spanning aspan)
vec2 arcDist(vec2 p, vec2 c, float rad, float a0, float aspan) {
    vec2 d = p - c;
    float an = atan(d.y, d.x);
    float rel = mod(an - a0, TAU);
    if (rel > aspan) {
        // clamp to nearest endpoint
        float relC = rel - aspan > (TAU - rel) ? 0.0 : aspan;
        vec2 ep = c + rad * vec2(cos(a0 + relC), sin(a0 + relC));
        return vec2(length(p - ep), relC * rad);
    }
    return vec2(abs(length(d) - rad), rel * rad);
}

vec2 segDist(vec2 p, vec2 a, vec2 b) {
    vec2 ab = b - a, ap = p - a;
    float h = clamp(dot(ap, ab) / dot(ab, ab), 0.0, 1.0);
    return vec2(length(ap - ab * h), h * length(ab));
}

vec3 renderScene() {
    vec2 fc = gl_FragCoord.xy;
    vec2 q = (fc - 0.5 * R) / R.y;
    vec2 uv = fc / R;

    float amt   = audioReact;
    float bassP = pow(smoothstep(0.05, 0.85, audioBass), 1.5);
    float midP  = pow(smoothstep(0.06, 0.85, audioMid), 1.2);
    float highP = pow(smoothstep(0.10, 0.90, audioHigh), 1.2);
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float levP  = clamp(audioLevel, 0.0, 1.0);
    float t = TIME * motionSpeed;

    float aa = 1.6 / R.y;

    // ── split background ────────────────────────────────────────────────
    float split = 0.12 + 0.03 * sin(t * 0.1);
    // left: horizontal b/w stripes, scrolling
    float sy = uv.y * 13.0 + t * scrollSpeed * 0.8;
    float stripe = step(0.5, fract(sy));
    vec3 bgL = mix(vec3(0.05, 0.05, 0.06), vec3(0.96, 0.94, 0.90), stripe);
    // right: dot wall — white dots, hashed candy pops
    vec2 g = q * dotScale;
    g.y -= t * scrollSpeed * 0.4;
    vec2 id = floor(g);
    vec2 lp = fract(g) - 0.5;
    float dd = length(lp) - 0.36;
    float dotM = smoothstep(aa * dotScale, -aa * dotScale, dd);
    float pop = hash21(id + floor(t * 0.8));
    vec3 dotC = vec3(0.97, 0.95, 0.92);
    if (pop > 0.86) {
        float pk = fract(pop * 13.7);
        dotC = pk < 0.25 ? vec3(0.95, 0.75, 0.10) : (pk < 0.5 ? vec3(0.9, 0.15, 0.2) : (pk < 0.75 ? vec3(0.2, 0.6, 0.9) : vec3(0.55, 0.3, 0.75)));
        dotC *= 1.0 + amt * 0.5 * beatP;
    }
    vec3 bgR = mix(vec3(0.04, 0.04, 0.05), dotC, dotM);
    vec3 col = mix(bgL, bgR, step(split, q.x));

    // ── the tube path: arc(hub A, top-left) + diagonal + arc(hub B) ─────
    vec2 hubA = vec2(-0.28, 0.22);
    vec2 hubB = vec2(0.24, -0.26);
    float loopR = 0.21;
    // breathing width (bass)
    float halfW = 0.105 * tubeWidth * (1.0 + amt * 0.12 * bassP);

    vec2 dA = arcDist(q, hubA, loopR, -1.2, 4.9);
    vec2 dB = arcDist(q, hubB, loopR, 1.9, 4.9);
    vec2 pA = hubA + loopR * vec2(cos(-1.2 + 4.9), sin(-1.2 + 4.9));
    vec2 pB = hubB + loopR * vec2(cos(1.9), sin(1.9));
    vec2 dS = segDist(q, pA, pB);

    float lenA = 4.9 * loopR, lenS = length(pB - pA);
    float d; float along;
    if (dA.x <= dB.x && dA.x <= dS.x) { d = dA.x; along = dA.y; }
    else if (dS.x <= dB.x)            { d = dS.x; along = lenA + dS.y; }
    else                              { d = dB.x; along = lenA + lenS + dB.y; }

    float inTube = smoothstep(halfW + aa, halfW - aa, d);
    if (inTube > 0.001) {
        // stripe index across the tube (outermost = pink → inward rainbow)
        float nS = floor(stripeN + 0.5);
        float v = 1.0 - clamp(d / halfW, 0.0, 1.0);   // 0 at edge → 1 center
        float band = floor(v * nS) / nS;
        float flow = along * 1.6 - t * flowSpeed * 0.5 - amt * 0.4 * midP;
        vec3 tc = rainbow6(band * 0.99 + floor(flow) * 0.0, hueShift);
        // thin black separators between stripes (the printed keylines)
        float sep = abs(fract(v * nS) - 0.5);
        tc = mix(vec3(0.07, 0.06, 0.07), tc, smoothstep(0.40, 0.34, sep));
        // subtle tube rounding shade
        tc *= 0.90 + 0.14 * cos((1.0 - v) * 1.5707);
        tc *= 1.0 + amt * 0.12 * levP;
        col = mix(col, tc, inTube);
    }
    // pink halo outline around the tube
    float halo = smoothstep(halfW + 0.035, halfW + 0.012, d) * (1.0 - inTube);
    vec3 haloC = vec3(0.95, 0.62, 0.85) * (1.0 + amt * 0.4 * highP * 0.3);
    col = mix(col, haloC, halo * 0.95);

    // ── hub cores: spinning half-and-half sun discs in pink rings ───────
    for (int i = 0; i < 2; i++) {
        vec2 hc = i == 0 ? hubA : hubB;
        float hr = loopR - 0.105 * tubeWidth - 0.012;
        vec2 hd = q - hc;
        float hdist = length(hd);
        // solid pink hub plate (ring + interior fill, like the print)
        float plate = smoothstep(hr + 0.03 + aa, hr + 0.03 - aa, hdist);
        col = mix(col, vec3(0.95, 0.62, 0.85), plate);
        float core = smoothstep(hr * 0.62 + aa, hr * 0.62 - aa, hdist);
        float spin = t * (i == 0 ? 0.4 : -0.33) + amt * 0.4 * midP;
        float half_ = step(0.0, sin(atan(hd.y, hd.x) + spin) * 0.5 + hd.y * 0.0);
        vec3 sun = mix(vec3(0.99, 0.83, 0.10), vec3(0.92, 0.12, 0.14), half_);
        sun *= 1.0 + amt * 0.25 * beatP;
        col = mix(col, sun, core);
    }

    col += (hash21(fc + fract(t) * vec2(17.0, 29.0)) - 0.5) * 0.015;
    return col;
}

void main() {
    vec2 uv = gl_FragCoord.xy / R;
    if (PASSINDEX == 0) {
        vec3 col = renderScene();
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
