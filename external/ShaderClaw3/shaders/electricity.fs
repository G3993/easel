/*{
  "DESCRIPTION": "Electric arc — simplex-noise plasma discharge lines woven into a full electric grid: horizontal AND vertical arcs (gridMix blends them in), crackling energy packets traveling along every line, hot white cores, branch forks and micro-tendrils. Bass shakes the arcs, beats surge the crackle.",
  "CREDIT": "Port of Humus Electro demo, simplex noise by Nikita Miropolskiy",
  "CATEGORIES": [
    "Generator"
  ],
  "INPUTS": [
    {
      "NAME": "midSize1",
      "LABEL": "Mid Size 1",
      "TYPE": "float",
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "midSize2",
      "LABEL": "Mid Size 2",
      "TYPE": "float",
      "DEFAULT": 0.15,
      "MIN": 0,
      "MAX": 0.4,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "arcCount",
      "LABEL": "Arc Count",
      "TYPE": "float",
      "DEFAULT": 3,
      "MIN": 1,
      "MAX": 8,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "branching",
      "LABEL": "Branch Forks",
      "TYPE": "float",
      "DEFAULT": 0.4,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "gridMix",
      "LABEL": "Grid Mix",
      "TYPE": "float",
      "DEFAULT": 0.7,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "wiggleAmp",
      "LABEL": "Wiggle Amp",
      "TYPE": "float",
      "DEFAULT": 45,
      "MIN": 0,
      "MAX": 100,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "freak",
      "LABEL": "Freak",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0.5,
      "MAX": 10,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "freak2",
      "LABEL": "Freak 2",
      "TYPE": "float",
      "DEFAULT": 0.55,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "flicker",
      "LABEL": "Flicker",
      "TYPE": "float",
      "DEFAULT": 0.3,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "burn",
      "LABEL": "Burn",
      "TYPE": "float",
      "DEFAULT": 0.35,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "arcColor",
      "LABEL": "Color",
      "TYPE": "color",
      "DEFAULT": [
        0.95,
        0.95,
        0.95,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
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
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

vec3 random3(vec3 c) {
    float j = 4096.0 * sin(dot(c, vec3(17.0, 59.4, 15.0)));
    vec3 r;
    r.z = fract(512.0 * j);
    j *= 0.125;
    r.x = fract(512.0 * j);
    j *= 0.125;
    r.y = fract(512.0 * j);
    return r - freak;
}

float simplex3d(vec3 p) {
    float F3 = 0.3333333;
    float G3 = 0.1666667;
    vec3 s = floor(p + dot(p, vec3(F3)));
    vec3 x = p - s + dot(s, vec3(G3));
    vec3 e = step(vec3(0.0), x - x.yzx);
    vec3 i1 = e * (1.0 - e.zxy);
    vec3 i2 = 1.0 - e.zxy * (1.0 - e);
    vec3 x1 = x - i1 + G3;
    vec3 x2 = x - i2 + 2.0 * G3;
    vec3 x3 = x - 1.0 + 3.0 * G3;
    vec4 w, d;
    w.x = dot(x, x);
    w.y = dot(x1, x1);
    w.z = dot(x2, x2);
    w.w = dot(x3, x3);
    w = max(freak2 - w, 0.0);
    d.x = dot(random3(s), x);
    d.y = dot(random3(s + i1), x1);
    d.z = dot(random3(s + i2), x2);
    d.w = dot(random3(s + 1.0), x3);
    w *= w;
    w *= w;
    d *= w;
    return dot(d, vec4(wiggleAmp));
}

float fbmNoise(vec3 m) {
    return 0.5333333 * simplex3d(m)
         + 0.2666667 * simplex3d(2.0 * m)
         + 0.1333333 * simplex3d(4.0 * m)
         + 0.0666667 * simplex3d(8.0 * m);
}

// One discharge line. ax = coordinate ALONG the arc (-1..1), ay = ACROSS,
// pn = noise-domain coords, axisSeed decorrelates the vertical family.
vec3 arcLayer(float ax, float ay, vec2 pn, float fai, float axisSeed,
              float ampBoost, float aHit) {
    float off    = (fract(sin((fai + axisSeed) * 7.13) * 43758.5453) - 0.5) * 0.7;
    float phase  = fai * 1.7 + axisSeed * 3.1;
    float ampJit = 0.6 + 0.6 * fract(sin((fai + axisSeed) * 11.7) * 43758.5453);

    vec3 p3 = vec3(pn, TIME * 0.4 + phase);
    float intensity = fbmNoise(p3 * 12.0 + 12.0);

    float tw = clamp(ax * -ax * midSize1 + midSize2, 0.0, 1.0);
    float yc = abs(intensity * -tw * ampBoost * ampJit + (ay - off));
    float g  = pow(yc, burn * (1.0 - audioLevel * audioReact * 0.4));

    // Hue shift per arc — purple/cyan/white gradient via cosine palette
    float hue = fract(hueShift + fai * 0.18 + axisSeed * 0.07 + TIME * 0.05);
    vec3 arcCol = mix(arcColor.rgb,
                      0.5 + 0.5 * cos(6.28318 * hue + vec3(0.0, 2.094, 4.188)),
                      hueShift);
    vec3 acc = arcCol;
    acc = acc * -g + acc;
    acc = acc * acc;
    acc = acc * acc;

    // Crackling energy packets travel along the line: hot knots that slide,
    // fattening the white core as they pass. Beats surge every packet.
    float dirSign = mod(fai + axisSeed, 2.0) < 1.0 ? 1.0 : -1.0;
    float trav = 0.5 + 0.5 * sin(ax * 5.5 * dirSign - TIME * (2.4 + fai * 0.55)
                                 + phase * 2.0);
    float packet = pow(trav, 9.0);
    float core = exp(-yc * (34.0 + 22.0 * packet))
               * (0.42 + 1.0 * packet) * (1.0 + 1.5 * aHit);
    acc += (arcCol * 0.6 + vec3(0.4)) * core;

    // Branch forks — sparse perpendicular bolts shooting off the arc
    if (branching > 0.001) {
        float branchPhase = fract(TIME * 1.5 + fai * 0.7 + axisSeed * 0.21);
        float branchTrig = step(0.94, fract(sin(floor(TIME * 6.0 + fai + axisSeed) * 17.3) * 43758.5453));
        float fork = smoothstep(0.05, 0.0, abs(ax - (branchPhase * 2.0 - 1.0)))
                   * smoothstep(0.5, 0.0, abs(ay - off));
        acc += arcCol * fork * branching * branchTrig * 1.5;

        // Micro-tendrils — thinner, faster, twitchier filaments
        float tPos = fract(sin(floor(TIME * 9.0 + fai * 3.0 + axisSeed) * 23.7) * 43758.5453);
        float tTrig = step(0.55, fract(sin(floor(TIME * 7.0 + fai * 1.3 + axisSeed * 2.0) * 41.9) * 43758.5453));
        float wobble = 0.05 * sin(ay * 34.0 + TIME * 11.0 + phase);
        float tend = smoothstep(0.016, 0.0, abs(ax - (tPos * 1.8 - 0.9) - wobble))
                   * smoothstep(0.26, 0.0, abs(ay - off))
                   * smoothstep(0.02, 0.09, abs(ay - off));
        acc += (arcCol * 0.7 + vec3(0.3)) * tend
             * (0.35 + 0.65 * branching) * tTrig * (1.0 + 1.2 * aHit);
    }
    return acc;
}

void main() {
    vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec2 centered = uv * 2.0 - 1.0;

    // Guitar-string pluck: mouse crossing center amplifies wiggle
    float mouseDist = abs(mousePos.y - 0.5) * 2.0; // 0 at center, 1 at edges
    float pluck = 1.0 - smoothstep(0.0, 0.4, mouseDist); // strong near center, fades out
    // Soft-kneed bass (sub coupled for sparse hiphop kicks): low 0.03 floor so
    // jazz's soft accents shake the arc too; 0.95 ceiling keeps EDM headroom.
    float bassC = pow(smoothstep(0.03, 0.95, max(audioBass, 0.8 * audioSub)), 1.3);
    float ampBoost = 1.0 + pluck * 3.0 + bassC * 4.5; // mouse pluck + bass shakes the arc

    vec2 p = gl_FragCoord.xy / RENDERSIZE.x;

    // hit envelope drives the crackle surges (computed once, shared)
    float aHit = clamp(audioReact, 0.0, 2.0)
               * max(smoothstep(0.02, 0.80, audioBeatPulse),
                     smoothstep(0.03, 0.85, audioPunch));

    // ── The electric grid: horizontal arcs + gridMix vertical arcs ───
    int AC = int(clamp(arcCount, 1.0, 8.0));
    vec3 col = vec3(0.0);
    for (int ai = 0; ai < 8; ai++) {
        if (ai >= AC) break;
        float fai = float(ai);
        col += arcLayer(centered.x, centered.y, p, fai, 0.0, ampBoost, aHit);
        if (gridMix > 0.001)
            col += gridMix * arcLayer(centered.y, centered.x, p.yx, fai, 5.0,
                                      ampBoost, aHit);
    }

    // Stochastic flicker
    float fl = 1.0 + (fract(sin(floor(TIME * 30.0) * 91.7) * 43758.5453) - 0.5) * flicker * 0.6;
    col *= fl;

    // Continuous band-follow on the whole discharge glow + decaying hit
    // traces. The arc's own fbm churn drowns subtle geometry shifts, so the
    // music must also ride the luminance directly: rock's sustained mids and
    // jazz's walking mids lift it continuously, each kick/snare/accent lands
    // a trace that eases out (beatPulse/punch decay — never a strobe).
    // Silence multiplies by exactly 1.0.
    {
        // r2 jazz fix: LINEAR mid/bass followers (jazz walks its mids), and
        // events with a low 0.02-0.03 floor, never squared — squaring turned
        // jazz's soft 0.4-0.5 accents into ~0.2 and they vanished. The kneed
        // bassC stays on the arc geometry above. Decaying envelopes, never a
        // strobe; silence multiplies by exactly 1.0.
        float aMidC  = smoothstep(0.02, 0.98, audioMid);
        float aBassL = smoothstep(0.02, 0.98, max(audioBass, 0.8 * audioSub));
        float aHitT  = max(smoothstep(0.02, 0.80, audioBeatPulse),
                           smoothstep(0.03, 0.85, audioPunch));
        col *= 1.0 + clamp(audioReact, 0.0, 2.0)
                   * (0.26 * aMidC + 0.22 * aBassL + 0.38 * aHitT);
    }

    // Surprise: every ~11s a brief power-out — the entire field cuts to
    // black for ~80ms then resumes. Distant thunder behind the bolt.
    {
        float _ph = fract(TIME / 11.0);
        float _cut = step(_ph, 0.04);
        col = mix(col, vec3(0.0), _cut * 0.85);
    }

    // ---- universal color block (defaults = no-op) ----
    // (hue handled by the existing `hueShift` arc-palette input)
    vec3 uc = col;
    float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
    uc = mix(vec3(ucL), uc, colorBoost);                   // saturation
    // background = darkest end of the field (the void around the arcs)
    uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

    gl_FragColor = vec4(uc, 1.0);
}
