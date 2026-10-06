/*{
  "CREDIT": "by mojovideotech",
  "CATEGORIES": [
    "generator",
    "rainbow",
    "circular",
    "rotation"
  ],
  "DESCRIPTION": "Twisting rainbow ring wrapped around a spinning six-petal flower centre — layered petal rings with fine veins and a warm glowing pistil, all drawn from the same cosine palette.",
  "ISFVSN" : "2",
    "INPUTS": [
    {
        "NAME" :    "scale",
        "TYPE" :    "float",
        "DEFAULT" : 1.0,
        "MIN" :     0.1,
        "MAX" :     2.0
    },
    {
        "NAME" :    "thickness",
        "TYPE" :    "float",
        "DEFAULT" : 1.75,
        "MIN" :     0.5,
        "MAX" :     2.0
    },
    {
        "NAME" :    "twists",
        "TYPE" :    "float",
        "DEFAULT" : 1.0,
        "MIN" :     1.0,
        "MAX" :     5.0
    },
    {
        "NAME" :    "rate",
        "TYPE" :    "float",
        "DEFAULT" : 1.5,
        "MIN" :     -2.0,
        "MAX" :     2.0
    },
    {
        "NAME" :    "gamma",
        "TYPE" :    "float",
        "DEFAULT" : 0.454545,
        "MIN" :     0.25,
        "MAX" :     1.0
    },
    {
        "NAME" :    "audioReact",
        "LABEL" :   "Audio React",
        "TYPE" :    "float",
        "DEFAULT" : 1.0,
        "MIN" :     0.0,
        "MAX" :     2.0,
        "GROUP" :   "Audio Reactivity"
    }
    ]
}

*/

////////////////////////////////////////////////////////////
// RainbowRingCubicTwist  by mojovideotech
//
// based on :
// glslsandbox/e#58416.0
//
// Creative Commons Attribution-NonCommercial-ShareAlike 3.0
////////////////////////////////////////////////////////////


#ifdef GL_ES
precision highp float;
#endif


void main()
{
    float T = TIME * rate;
    // Audio: bass fattens the ring and nudges the twist phase, highs brighten.
    // (Couplings kept small + additive so the frame never jumps — anti-chop.)
    float aB = pow(smoothstep(0.05, 0.85, audioBass), 1.4) * audioReact;
    float aH = smoothstep(0.05, 0.85, audioHigh) * audioReact;
    T += 0.30 * aB;
    vec2 R = RENDERSIZE;
    vec2 P0 = (gl_FragCoord.xy - 0.5*R)*(2.1 - scale);
    float rr = length(P0) / R.y;
    float an = atan(P0.y, P0.x);
    vec2 P = vec2(rr - 0.333, an);
    vec4 S, E, F;
    P *= vec2(2.6 - thickness - 0.16*aB, floor(twists));
    S = 0.08*cos(1.5*vec4(0.0, 1.0, 2.0, 3.0) + T + P.y + sin(P.y)*cos(T));
    E = S.yzwx;
    F = max(P.x - S, E - P.x);
    vec4 ring = pow(dot(clamp(F*R.y, 0.0, 1.0), 72.0*(S - E))*(S - 0.1), vec4(gamma * (1.0 - 0.10*aH)));

    // ── flower centre: two layered petal rings + a glowing pistil, so the
    //    rainbow ring wraps around a bloom instead of an empty hole ──
    float px = 1.6 / R.y;
    float spin = T * 0.10;
    vec3 flower = vec3(0.0);
    for (int i = 0; i < 2; i++) {
        float fi = float(i);
        float a2 = an + spin * (1.0 - 0.55*fi) + fi * 0.5236;
        float petal = pow(abs(cos(a2 * 3.0)), 0.65);          // 6 rounded petals
        float petR = (0.245 - 0.088*fi) * (0.34 + 0.66*petal);
        float m = smoothstep(px*1.6, -px*1.6, rr - petR);      // crisp AA edge
        // same cos-palette family as the ring so the bloom belongs to it
        vec3 pc = 0.5 + 0.5*cos(T*0.6 + fi*2.1 + rr*9.0 + an*0.5 + vec3(0.0, 2.094, 4.188));
        float shade = 0.50 + 0.50*smoothstep(petR, petR*0.12, rr); // lit toward centre
        float vein = 0.92 + 0.08*cos(a2*18.0);                 // fine petal veins
        flower = mix(flower, pc * shade * vein, m);
    }
    // glowing pistil
    float coreR = 0.055 * (1.0 + 0.06*aB);
    float core = smoothstep(coreR + px*2.0, coreR - px*2.0, rr);
    vec3 coreC = clamp(vec3(1.0, 0.9, 0.55) + 0.30*cos(T*0.8 + vec3(0.0, 1.0, 2.0)), 0.0, 1.0);
    flower = mix(flower, coreC, core);
    flower += vec3(1.0, 0.85, 0.5) * exp(-rr*26.0) * 0.30;     // warm centre glow

    // bloom lives inside the ring's inner edge only
    float inner = smoothstep(0.31, 0.25, rr);
    vec3 col = max(ring.rgb, 0.0) + flower * inner;
    // smooth-envelope luminance breath on the dark frame — audio reads as a
    // glow swell, never a spatial jump
    col *= 1.0 + 0.35 * aB + 0.18 * aH;
    gl_FragColor = vec4(col, 1.0);
}