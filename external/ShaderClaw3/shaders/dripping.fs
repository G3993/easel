/*{
  "DESCRIPTION": "Dripping — the input image melts: a compact energy-exchange fluid drags a location field downward so the picture drips and smears like wet paint, relit with a moving specular sheen and a diagonal lens-flair bloom. Bass pulls the drips harder, highs flare the bloom; the melt re-forms on a loop.",
  "CREDIT": "Energy-exchange fluid + location-field warp technique (wyatt-style, Shadertoy), ShaderClaw audio port",
  "CATEGORIES": [
    "Effect"
  ],
  "INPUTS": [
    {
      "NAME": "inputTex",
      "LABEL": "Source",
      "TYPE": "image"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Audio React",
      "TYPE": "float",
      "GROUP": "Audio Reactivity",
      "DEFAULT": 0.5,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "loopTime",
      "LABEL": "Melt Loop (s)",
      "TYPE": "float",
      "DEFAULT": 14.0,
      "MIN": 4.0,
      "MAX": 40.0
    },
    {
      "NAME": "flareAmt",
      "LABEL": "Flare",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "tintColor",
      "LABEL": "Tint",
      "TYPE": "color",
      "GROUP": "Color",
      "DEFAULT": [1.0, 1.0, 1.0, 1.0]
    },
    {
      "NAME": "brightness",
      "LABEL": "Brightness",
      "TYPE": "float",
      "GROUP": "Color",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 3.0
    },
    {
      "NAME": "meltAmt",
      "LABEL": "Melt Strength",
      "TYPE": "float",
      "DEFAULT": 1.0,
      "MIN": 0.2,
      "MAX": 3.0
    }
  ],
  "PASSES": [
    {
      "TARGET": "fluidA",
      "PERSISTENT": true
    },
    {
      "TARGET": "locBuf",
      "PERSISTENT": true
    },
    {
      "TARGET": "litBuf"
    },
    {}
  ]
}*/

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }
float hashD(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }

// fluid state encode: signed values round-trip the 8-bit buffer.
// Write-dither is REQUIRED: forcing per frame is below the 4/255 encode
// quantum, so without dither the fluid never accumulates and nothing melts.
vec4 encA(vec4 v) {
    vec4 e = clamp(v / 4.0 + 0.5, 0.0, 1.0);
    float d = hashD(gl_FragCoord.xy + fract(TIME) * 61.7) - 0.5;
    return clamp(e + d * (1.0 / 255.0), 0.0, 1.0);
}
vec4 decA(vec4 e) { return (e - 0.5) * 4.0; }
// location field: normalized coords packed 16-bit per axis
vec4 encLoc(vec2 p) {
    p = clamp(p, 0.0, 1.0);
    vec2 e = p * 255.0;
    return vec4(floor(e.x) / 255.0, fract(e.x), floor(e.y) / 255.0, fract(e.y));
}
vec2 decLoc(vec4 t) { return vec2(t.r + t.g / 255.0, t.b + t.a / 255.0); }

// source with a procedural fallback so the effect is alive with no input bound:
// a vivid sunset poster — deep color fields, a hot sun disc, thin dark
// mullions — built to smear beautifully when it melts.
vec4 srcAt(vec2 loc) {
    if (IMG_SIZE(inputTex).x > 0.5) return IMG_NORM_PIXEL(inputTex, loc);
    vec2 p = clamp(loc, 0.0, 1.0);
    vec3 gold  = vec3(0.99, 0.80, 0.28);
    vec3 coral = vec3(0.93, 0.30, 0.22);
    vec3 indigo= vec3(0.16, 0.24, 0.62);
    vec3 nightC= vec3(0.05, 0.045, 0.10);
    vec3 c = mix(coral, gold, smoothstep(0.18, 0.62, p.y));
    c = mix(c, indigo, smoothstep(0.60, 0.96, p.y) * 0.9);
    // hot sun disc with a soft halo
    float sd = length((p - vec2(0.60, 0.58)) * vec2(1.25, 1.0));
    c = mix(c, vec3(1.0, 0.95, 0.80), smoothstep(0.155, 0.145, sd));
    c += vec3(0.9, 0.6, 0.25) * exp(-sd * sd * 22.0) * 0.35;
    // dark ground band + skyline teeth
    float sky = smoothstep(0.30, 0.10, p.y);
    float teeth = step(fract(p.x * 9.0), 0.55) * 0.10;
    c = mix(c, nightC, clamp(sky + smoothstep(0.30 + teeth, 0.29 + teeth, p.y) * 0.85, 0.0, 1.0) * 0.95);
    // faint vertical mullions for the drips to shear apart
    c *= 1.0 - 0.16 * smoothstep(0.035, 0.0, abs(fract(p.x * 6.0) - 0.5) * 0.1667)
             * smoothstep(0.25, 0.45, p.y);
    return vec4(c, 1.0);
}

vec4 TA(vec2 U) { return decA(texture2D(fluidA, U / RENDERSIZE.xy)); }
vec2 vel(vec4 b) { return vec2(b.x - b.y, b.z - b.w); }
float pres(vec4 b) { return 0.25 * (b.x + b.y + b.z + b.w); }
vec4 advA(vec2 U) {
    U -= 0.5 * vel(TA(U));
    U -= 0.5 * vel(TA(U));
    return TA(U);
}
vec2 locAt(vec2 U) { return decLoc(texture2D(locBuf, U / RENDERSIZE.xy)); }
vec2 advLoc(vec2 U) {
    U -= 0.5 * vel(TA(U));
    U -= 0.5 * vel(TA(U));
    return locAt(U);
}

bool resetPulse() {
    return FRAMEINDEX < 2 || mod(TIME, max(loopTime, 4.0)) < max(TIMEDELTA, 0.034);
}

// pass 0 — energy-exchange fluid
vec4 passFluid() {
    vec2 U = gl_FragCoord.xy;
    vec2 R = RENDERSIZE.xy;
    float bassP = pow(knee(audioBass, 0.05, 0.85), 1.6);

    vec4 Q = advA(U);
    vec4 n = advA(U + vec2(0, 1)), e = advA(U + vec2(1, 0)),
         s = advA(U - vec2(0, 1)), w = advA(U - vec2(1, 0));
    float px = 0.25 * (pres(e) - pres(w));
    float py = 0.25 * (pres(n) - pres(s));
    Q += 0.25 * (n.w + e.y + s.z + w.x) - pres(Q) - vec4(px, -px, py, -py);

    // darker picture areas melt faster; z stays positive so drips always
    // fall the same way (a signed z churned instead of dripping).
    // lum comes straight from the warped source (litBuf is non-persistent
    // and read-before-write here, so it was never a reliable tap).
    float lum = dot(srcAt(locAt(U)).rgb, vec3(0.333));
    float z = clamp(0.9 - lum * 0.8, 0.12, 1.0);
    Q = mix(mix(Q, 0.25 * (n + e + s + w), 0.01), vec4(pres(Q)), 0.01 * (1.0 - z));
    // per-column streak weights make distinct rivulets instead of a sheet
    float streak = 0.45 + 0.55 * hashD(vec2(floor(U.x / 5.0), 3.7));
    Q.zw -= 0.011 * meltAmt * z * streak * vec2(1, -1)
          * (1.0 + 2.4 * audioReact * bassP);
    Q = clamp(Q, -1.9, 1.9);

    if (resetPulse()) Q = vec4(0.2);
    if (U.x < 3.0 || R.x - U.x < 3.0 || U.y < 3.0 || R.y - U.y < 3.0) Q = vec4(pres(Q));
    return encA(Q);
}

// pass 1 — location field advected by the fluid
vec4 passLoc() {
    vec2 U = gl_FragCoord.xy;
    vec2 Q = advLoc(U);

    vec4 q = TA(U), n = TA(U + vec2(0, 1)), e = TA(U + vec2(1, 0)),
         s = TA(U - vec2(0, 1)), w = TA(U - vec2(1, 0));
    vec2 N = advLoc(U + vec2(0, 1)), E = advLoc(U + vec2(1, 0)),
         S = advLoc(U - vec2(0, 1)), W = advLoc(U - vec2(1, 0));
    Q += 0.25 * ((n.w - q.z) * (N - Q) + (e.y - q.x) * (E - Q)
               + (s.z - q.w) * (S - Q) + (w.x - q.y) * (W - Q));

    // guaranteed melt: the lookup location creeps upward (content slides
    // down) at a per-rivulet rate, darker paint dripping faster — this
    // keeps the picture dripping even while the fluid is still waking up.
    float lum2 = dot(srcAt(Q).rgb, vec3(0.333));
    float z2 = clamp(0.95 - lum2 * 0.75, 0.15, 1.0);
    float riv = 0.30 + 0.70 * hashD(vec2(floor(U.x / 4.0), 9.1));
    float wob = 0.75 + 0.25 * sin(U.x * 0.05 + TIME * 0.6);
    float bassP2 = pow(knee(audioBass, 0.05, 0.85), 1.6);
    Q.y += 0.00042 * meltAmt * z2 * riv * wob
         * (1.0 + 2.0 * audioReact * bassP2);
    // a whisper of sideways shear so drips waver as they fall
    Q.x += 0.00006 * meltAmt * sin(U.y * 0.045 + TIME * 0.35 + riv * 6.28);

    if (resetPulse()) Q = U / RENDERSIZE.xy;
    return encLoc(Q);
}

// pass 2 — look the picture up through the warped locations, then relight
vec4 passLit() {
    vec2 U = gl_FragCoord.xy;
    vec2 R = RENDERSIZE.xy;
    vec4 img = srcAt(locAt(U));
    float n = length(srcAt(locAt(U + vec2(0, 1))));
    float e = length(srcAt(locAt(U + vec2(1, 0))));
    float s = length(srcAt(locAt(U - vec2(0, 1))));
    float w = length(srcAt(locAt(U - vec2(1, 0))));
    vec3 no = normalize(vec3(e - w, n - s, 0.35));
    // light direction leans with the mids so sustained music visibly
    // re-lights the wet paint (rock/jazz correlation, display-side only)
    float midP = pow(knee(audioMid, 0.06, 0.88), 1.2);
    float levP = knee(audioLevel, 0.04, 0.9);
    vec3 ld = normalize(vec3(1.0, 1.0 + 0.9 * audioReact * midP,
                             1.0 - 0.35 * audioReact * levP));
    float d = dot(reflect(no, vec3(0, 0, 1)), ld);
    float sheen = exp(-2.5 * d * d);
    float sheenGain = 1.0 + audioReact * (0.9 * midP + 0.5 * levP);
    vec3 col = img.rgb * (0.45 + 0.55 * sheen * sheenGain)
             + vec3(0.20) * pow(sheen, 3.0) * sheenGain;
    return vec4(col, 1.0);
}

// pass 3 — diagonal lens-flair bloom composite
vec4 flareTap(vec2 U, vec2 r) {
    vec4 t = texture2D(litBuf, (U + r) / RENDERSIZE.xy);
    return exp(-0.01 * dot(r, r)) * (exp(2.0 * t) - 1.0);
}

vec4 passFinal() {
    vec2 U = gl_FragCoord.xy;
    float highP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    vec4 Q = vec4(0.0);
    for (float i = 0.0; i < 7.0; i += 1.1) {
        Q += flareTap(U, vec2(-i, i));
        Q += flareTap(U, vec2(i, i));
        Q += flareTap(U, -vec2(-i, i));
        Q += flareTap(U, -vec2(i, i));
    }
    float beatP = clamp(audioBeatPulse, 0.0, 1.0);
    float flareGain = flareAmt * (1.0 + audioReact * (2.2 * highP + 2.6 * beatP));
    // 1e-5 made the flare invisible; 0.0028 puts the diagonal bloom back
    Q = texture2D(litBuf, U / RENDERSIZE.xy) * 1.0 + 0.0028 * flareGain * Q;
    Q.rgb *= tintColor.rgb * brightness;
    // audio lift, dim-tracking so bright frames never blow past 1
    float levP = knee(audioLevel, 0.04, 0.9);
    Q.rgb *= mix(1.0, 0.64 + 0.24 * levP + 0.12 * beatP,
                 clamp(audioReact, 0.0, 1.0) * 0.7);
    Q = atan(Q * 1.25);
    Q.a = 1.0;
    return Q;
}

void main() {
    if      (PASSINDEX == 0) gl_FragColor = passFluid();
    else if (PASSINDEX == 1) gl_FragColor = passLoc();
    else if (PASSINDEX == 2) gl_FragColor = passLit();
    else                     gl_FragColor = passFinal();
}
