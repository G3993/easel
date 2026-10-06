/*{
  "DESCRIPTION": "Squiggle Loop — a thick round tube meandering and self-crossing over pale ice-blue paper, changing ink at every bend: solid, striped and speckled segments in curated saturated inks (cobalt, orange-red, teal, yellow, violet, pink, green, black). Proper over/under crossings with painterly contact shadows and a soft drop shadow on the paper. The whole loop breathes and wanders slowly; mids wiggle the path locally, bass thickens the tube ~15%, and every beat advances the segment-ink pattern one step with an eased slide. Ink A and Ink B re-skin the two dominant ink families.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Geometry",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "tubeWidth",
      "LABEL": "Tube Width",
      "TYPE": "float",
      "MIN": 0.025,
      "MAX": 0.085,
      "DEFAULT": 0.052,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "inkSegments",
      "LABEL": "Ink Segments",
      "TYPE": "float",
      "MIN": 6,
      "MAX": 24,
      "DEFAULT": 13,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "wanderSpeed",
      "LABEL": "Wander Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "wiggleAmt",
      "LABEL": "Wiggle Amount",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "colorA",
      "LABEL": "Ink A",
      "TYPE": "color",
      "DEFAULT": [0.11, 0.2, 0.76, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "colorB",
      "LABEL": "Ink B",
      "TYPE": "color",
      "DEFAULT": [0.93, 0.275, 0.06, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "paletteShift",
      "LABEL": "Palette Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 10,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.6,
      "GROUP": "Audio Reactivity"
    }
  ],
  "PASSES": [
    {
      "TARGET": "stateBuf",
      "PERSISTENT": true
    },
    {
    }
  ]
}*/

float hash11(float p) {
    p = fract(p * 0.1031);
    p *= p + 33.33;
    p *= p + p;
    return fract(p);
}

float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

float gA, gBassP, gMidP, gHighP, gCount, gEnv, gSmMid;

// Curated saturated collage inks.
vec3 pal8(float k) {
    k = mod(k, 8.0);
    if (k < 0.5) return vec3(0.110, 0.200, 0.760);  // cobalt
    if (k < 1.5) return vec3(0.930, 0.275, 0.060);  // orange-red
    if (k < 2.5) return vec3(0.045, 0.410, 0.390);  // teal
    if (k < 3.5) return vec3(0.975, 0.860, 0.120);  // yellow
    if (k < 4.5) return vec3(0.395, 0.240, 0.700);  // violet
    if (k < 5.5) return vec3(0.950, 0.530, 0.590);  // pink
    if (k < 6.5) return vec3(0.135, 0.520, 0.205);  // green
    return vec3(0.090, 0.095, 0.105);               // black
}

// Closed sum-of-sines loop; integer harmonics keep it seamless, phases drift
// so the loop slowly wanders and re-knots itself.
vec2 pathPt(float s) {
    float a = s * 6.28318;
    float t = TIME * 0.045 * wanderSpeed;
    vec2 p;
    p.x = 0.54 * sin(a + 1.90 + 0.35 * sin(t * 0.63))
        + 0.30 * sin(2.0 * a + 0.60 + t)
        + 0.17 * sin(3.0 * a + 2.20 - t * 0.80)
        + 0.07 * sin(5.0 * a + 4.00 + t * 1.31);
    p.y = 0.52 * cos(a + 0.20 - 0.30 * sin(t * 0.71))
        + 0.31 * cos(2.0 * a + 2.90 - t * 0.87)
        + 0.17 * cos(3.0 * a + 5.10 + t * 0.66)
        + 0.06 * cos(5.0 * a + 1.10 - t * 1.13);
    p *= vec2(0.44, 0.475);
    return p * (1.0 + 0.035 * sin(TIME * 0.21));
}

float dsWrap(float a, float b) { float d = abs(a - b); return min(d, 1.0 - d); }

// deterministic over/under ordering along the loop
float ov(float s) { return sin(s * 12.566 + 1.3) + 0.6 * sin(s * 31.416 + 4.1); }

// Ink of the tube at parameter s — segment index slides one step per beat.
vec3 inkAt(float s, vec2 q) {
    float segN = floor(inkSegments + 0.5);
    float u = s * segN + (gCount - gEnv);
    float seg = mod(floor(u), 64.0);
    float h1 = hash11(seg * 7.31 + paletteShift * 2.13);
    float k = floor(h1 * 8.0);
    vec3 ink = pal8(k);
    // family re-skin: cobalt family -> Ink A, orange-red family -> Ink B
    ink *= mix(vec3(1.0), clamp(colorA.rgb / vec3(0.110, 0.200, 0.760), 0.0, 5.0),
               (1.0 - step(0.5, k)) * colorA.a);
    ink *= mix(vec3(1.0), clamp(colorB.rgb / vec3(0.930, 0.275, 0.060), 0.0, 5.0),
               step(0.5, k) * (1.0 - step(1.5, k)) * colorB.a);

    float sh = hash11(seg * 3.77 + 13.0);
    float style = sh < 0.52 ? 0.0 : (sh < 0.78 ? 1.0 : 2.0);
    if (style > 1.5) {
        // speckled — highs sprinkle extra specks
        float thr = 0.87 - 0.05 * gA * gHighP;
        float sp = step(thr, hash21(floor(q * 170.0) + vec2(seg * 13.1, seg * 5.7)));
        vec3 spc = (dot(ink, vec3(0.333)) > 0.45) ? vec3(0.10, 0.09, 0.10)
                                                  : vec3(0.965, 0.720, 0.760);
        ink = mix(ink, spc, sp * 0.85);
    } else if (style > 0.5) {
        // striped (phase from fract(u) keeps trig args small for mediump)
        float str = smoothstep(0.35, 0.65, 0.5 + 0.5 * sin(fract(u) * 28.0));
        vec3 second = pal8(k + 2.0 + floor(hash11(seg * 9.1) * 3.0));
        ink = mix(ink, second, str * 0.85);
    }
    return ink;
}

vec3 shadeTube(vec3 ink, float d, float R, float s) {
    float t = clamp(d / max(R, 1e-5), 0.0, 1.0);
    ink *= 0.90 + 0.10 * (1.0 - t * t);   // gentle painted rounding
    ink *= 1.0 + (hash21(vec2(floor(s * 380.0), floor(t * 5.0))) - 0.5) * 0.05;  // brush
    return ink;
}

vec4 renderArt() {
    vec2 res = RENDERSIZE.xy;
    vec2 q = (gl_FragCoord.xy - 0.5 * res) / min(res.x, res.y);
    // mids wiggle the path locally — amplitude soft-kneed so the top of the
    // slider never overshoots into scribble, and driven by the temporally
    // smoothed mid follower (gSmMid) so squiggle glides instead of jumping
    float wEff = 1.55 * (1.0 - exp(-wiggleAmt * 0.85));
    q += wEff * gA * gSmMid * 0.011 * vec2(sin(q.y * 30.0 + TIME * 2.3),
                                           sin(q.x * 27.0 - TIME * 1.9));

    float R = tubeWidth * (1.0 + 0.15 * gA * gBassP);
    vec2 shOff = vec2(0.016, -0.022);

    // distance to polyline, tracking the two nearest disjoint branches
    float d1 = 1e5; float s1 = 0.0;
    float d2 = 1e5; float s2 = 0.37;
    float dS = 1e5;
    vec2 pp = pathPt(0.0);
    for (int i = 1; i <= 48; i++) {
        float sc = float(i) / 48.0;
        vec2 pc = pathPt(sc);
        vec2 ba = pc - pp;
        float bb = max(dot(ba, ba), 1e-7);
        vec2 pa = q - pp;
        float h = clamp(dot(pa, ba) / bb, 0.0, 1.0);
        float d = length(pa - ba * h);
        float s = (float(i) - 1.0 + h) / 48.0;
        vec2 pa2 = q - shOff - pp;
        float h2 = clamp(dot(pa2, ba) / bb, 0.0, 1.0);
        dS = min(dS, length(pa2 - ba * h2));
        if (dsWrap(s, s1) < 0.075) { if (d < d1) { d1 = d; s1 = s; } }
        else if (d < d1) { d2 = d1; s2 = s1; d1 = d; s1 = s; }
        else if (dsWrap(s, s2) < 0.075) { if (d < d2) { d2 = d; s2 = s; } }
        else if (d < d2) { d2 = d; s2 = s; }
        pp = pc;
    }

    // pale ice-blue paper
    vec3 col = vec3(0.855, 0.895, 0.925);
    col *= 1.0 - 0.06 * length(q);
    col += (hash21(gl_FragCoord.xy) - 0.5) * 0.022;

    float aa = fwidth(d1) * 1.4 + 1e-4;

    // soft drop shadow of the whole loop on the paper
    float shm = (1.0 - smoothstep(R * 0.75, R * 1.9, dS)) * 0.30;
    col *= 1.0 - shm * vec3(0.30, 0.28, 0.22);

    // over/under: higher order value paints on top
    bool two = d2 < R * 1.9;
    float dT; float sT; float dB; float sB;
    if (two && ov(s2) > ov(s1)) { dT = d2; sT = s2; dB = d1; sB = s1; }
    else { dT = d1; sT = s1; dB = d2; sB = s2; }

    if (two) {
        vec3 inkB = shadeTube(inkAt(sB, q), dB, R, sB);
        float mB = smoothstep(R, R - aa, dB);
        float csh = (1.0 - smoothstep(R * 1.02, R * 1.55, dT)) * 0.42;  // contact shadow
        col = mix(col, inkB * (1.0 - csh), mB);
    }
    vec3 inkT = shadeTube(inkAt(sT, q), dT, R, sT);
    col = mix(col, inkT, smoothstep(R, R - aa, dT));

    float lift = mix(1.0, 0.82 + 0.34 * knee(audioLevel, 0.03, 0.85), gA * 0.55);
    col *= brightness * lift;
    return vec4(max(col, 0.0), 1.0);
}

void main() {
    gA     = clamp(audioReact, 0.0, 1.0);
    gBassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    gMidP  = pow(knee(audioMid,  0.08, 0.85), 1.3);
    gHighP = pow(knee(audioHigh, 0.10, 0.90), 1.2);

    if (PASSINDEX == 0) {
        // beat-step state in one bottom-corner pixel
        if (gl_FragCoord.x > 1.0 || gl_FragCoord.y > 1.0) { gl_FragColor = vec4(0.0); return; }
        vec4 st = texture2D(stateBuf, vec2(0.5 / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
        float count = floor(st.r * 255.0 + 0.5);
        float env   = st.g;
        float pb    = st.b;
        float beatNow = max(audioBeat, step(0.6, audioBeatPulse));
        if (beatNow > 0.5 && pb < 0.5) { count = mod(count + 1.0, 192.0); env = 1.0; }
        env *= 0.76;
        // temporally smoothed mid follower for the wiggle (chop-free glide)
        float smMid = (FRAMEINDEX < 2) ? gMidP : mix(st.a, gMidP, 0.10);
        gl_FragColor = vec4(count / 255.0, env, step(0.5, beatNow), smMid);
        return;
    }

    vec4 st = texture2D(stateBuf, vec2(0.5 / RENDERSIZE.x, 0.5 / RENDERSIZE.y));
    gCount = floor(st.r * 255.0 + 0.5);
    gEnv   = st.g * gA;
    gSmMid = st.a;
    gl_FragColor = renderArt();
}
