/*{
  "DESCRIPTION": "Painted Orbit — a surrealist airbrushed still life. One large glossy amber orb hangs mid-canvas while sweeping black, cream and sage-green blades arc over and around it, small satellite pebbles drift nearby and a thin wire line loops through the arrangement — all over a soft sky-blue to cream vertical gradient with fine canvas grain. Every shape is airbrush-shaded with soft drop shadows, like a 1970s spray-gun painting, never flat vector. Ultra-slow orbital drift; bass swells the orb 10-15 percent, mids sway the blades, and each beat breathes a soft rim light around the orb. Serene in silence.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Geometry",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "orbInk",
      "LABEL": "Orb Ink",
      "TYPE": "color",
      "DEFAULT": [0.94, 0.63, 0.09, 1.0],
      "GROUP": "Color"
    },
    {
      "NAME": "skyInk",
      "LABEL": "Sky Ink",
      "TYPE": "color",
      "DEFAULT": [0.44, 0.50, 0.79, 1.0],
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
      "NAME": "orbSize",
      "LABEL": "Orb Size",
      "TYPE": "float",
      "MIN": 0.6,
      "MAX": 1.4,
      "DEFAULT": 1.0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "bladeSweep",
      "LABEL": "Blade Sweep",
      "TYPE": "float",
      "MIN": 0.6,
      "MAX": 1.4,
      "DEFAULT": 1.0,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "grainAmt",
      "LABEL": "Canvas Grain",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.55,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "driftSpeed",
      "LABEL": "Drift Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
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
  ]
}*/

float hash21(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float knee(float x, float lo, float hi) { return smoothstep(lo, hi, x); }

vec3 hueRot(vec3 c, float a) {
    vec3 k = vec3(0.57735);
    float s = sin(a), co = cos(a);
    return c * co + cross(k, c) * s + k * dot(k, c) * (1.0 - co);
}

float vnoise(vec2 p) {
    vec2 i = floor(p), f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash21(i);
    float b = hash21(i + vec2(1.0, 0.0));
    float c = hash21(i + vec2(0.0, 1.0));
    float d = hash21(i + vec2(1.0, 1.0));
    return mix(mix(a, b, f.x), mix(c, d, f.x), f.y);
}

vec2 rot2(vec2 p, float a) { float s = sin(a), c = cos(a); return vec2(c * p.x - s * p.y, s * p.x + c * p.y); }

// tapered sweeping blade: arc of radius R, half-angle ha, max half-width w.
// Width follows a sine bell so both tips come to airbrushed points.
float gBladeT;
float sdBlade(vec2 p, float R, float ha, float w) {
    float a = atan(p.y, p.x);
    float ca = clamp(a, -ha, ha);
    vec2 q = vec2(cos(ca), sin(ca)) * R;
    float t = clamp(0.5 + 0.5 * a / ha, 0.0, 1.0);
    gBladeT = t;
    float wt = w * (0.06 + 0.94 * sin(t * 3.14159265));
    return length(p - q) - wt;
}

float sdCircle(vec2 p, float r) { return length(p) - r; }

// approximate ellipse SDF — fine for thin AA'd shapes
float sdEllipse(vec2 p, vec2 r) {
    float k = length(p / r);
    return (k - 1.0) * min(r.x, r.y);
}

// audio / motion globals
float gA, gBassP, gMidP, gHighP, gT, gSway, gRim, gGap, gOrbBr;
vec3  gOrb, gSky;

// paint one shape with AA + a soft airbrushed inner-edge density; d in canvas units
void lay(inout vec3 col, vec3 ink, float d, float px) {
    float edge = smoothstep(px * 3.5, 0.0, abs(d));
    vec3 inked = ink * (1.0 - 0.16 * edge);
    col = mix(col, inked, smoothstep(px, -px, d));
}

// soft airbrushed drop shadow cast by sdf d evaluated at the offset position
void dropShadow(inout vec3 col, float dOfs, float soft, float amt) {
    col *= 1.0 - amt * smoothstep(soft, -soft * 0.4, dOfs);
}

void main() {
    gA     = clamp(audioReact, 0.0, 1.0);
    gBassP = pow(knee(audioBass, 0.05, 0.85), 1.6);
    gMidP  = pow(knee(audioMid,  0.08, 0.85), 1.3);
    gHighP = pow(knee(audioHigh, 0.10, 0.90), 1.2);
    float levelP = knee(audioLevel, 0.04, 0.85);

    gT = TIME * driftSpeed + audioTime * 0.25 * gA * driftSpeed;

    // beat rim light (eased pulse, decays host-side)
    float pulse = clamp(audioBeatPulse, 0.0, 1.0);
    gRim = pulse * pulse * (3.0 - 2.0 * pulse) * gA;

    // mids sway the blades — amplitude modulation, phase stays continuous
    gSway = (0.055 + 0.075 * gA * gMidP);

    // eased breathing cycle: a slow inhale-exhale that pumps the gap between
    // the orb and everything orbiting it — the arrangement drifts out, dwells,
    // and eases back in. Audio deepens the breath via the kneed pre-smoothed
    // bass follower (amplitude only — chop-free).
    float bw = 0.5 + 0.5 * sin(gT * 0.24);
    float bE = bw * bw * (3.0 - 2.0 * bw);
    bE = bE * bE * (3.0 - 2.0 * bE);              // dwell at full inhale/exhale
    float bSgn = bE * 2.0 - 1.0;
    float bDepth = 0.07 + 0.075 * gA * gBassP;
    gGap   = 1.0 + bDepth * bSgn;                 // satellites breathe out / in
    gOrbBr = 1.0 - 0.5 * bDepth * bSgn;           // orb counter-breathes (pump)

    float hs = paletteShift * 0.6283;
    gOrb = clamp(hueRot(orbInk.rgb, hs), 0.0, 1.0);
    gSky = clamp(hueRot(skyInk.rgb, hs), 0.0, 1.0);

    // zoomed out so the arrangement floats with generous margins
    vec2 uv = (gl_FragCoord.xy - 0.5 * RENDERSIZE.xy) / RENDERSIZE.y * 1.34;
    float px = 1.9 / RENDERSIZE.y;

    // ── background: airbrushed sky-blue → cream vertical gradient ──
    vec3 cream = vec3(0.93, 0.89, 0.82);
    float gy = clamp(uv.y * 0.72 + 0.50, 0.0, 1.0);
    vec3 col = mix(cream, gSky, pow(gy, 1.4));
    // gentle atmospheric blotches, like thinned spray coats
    col += (vnoise(uv * 3.1 + 7.0 + 0.15 * gT * 0.2) - 0.5) * 0.040;
    col += (vnoise(uv * 9.7 + 2.0 - 0.10 * gT * 0.2) - 0.5) * 0.020;

    // ── layout ──
    float drift = gT * 0.09; // slow orbital drift — the whole arrangement breathes
    vec2 orbC = vec2(0.01, -0.055) + 0.019 * vec2(sin(gT * 0.42), cos(gT * 0.31));
    float orbR = 0.315 * orbSize * gOrbBr * (1.0 + 0.010 * sin(gT * 0.26) + 0.13 * gA * gBassP);

    // ── thin looping wire line (behind the orb — loops off to the right) ──
    {
        vec2 wp = rot2(uv - (orbC + gGap * vec2(0.26, -0.03)), -0.40 + 0.06 * sin(gT * 0.37));
        float dw = abs(sdEllipse(wp, vec2(0.50, 0.29))) - 0.0038;
        vec3 wcol = mix(vec3(0.92, 0.90, 0.87), vec3(0.36, 0.39, 0.56), smoothstep(-0.1, 0.25, wp.y));
        lay(col, wcol, dw, px);
    }

    // ── the orb: glossy amber sphere, airbrushed radial shading ──
    {
        vec2 op = uv - orbC;
        float d = sdCircle(op, orbR);
        // soft shadow the orb throws on the sky
        dropShadow(col, sdCircle(op - vec2(-0.04, -0.055), orbR), 0.15, 0.15);

        float r = length(op) / orbR;
        // highlight sits in the upper-center, like sprayed gouache
        float rh = length((op - vec2(-0.035, 0.105)) / orbR);
        vec3 hot  = vec3(0.99, 0.91, 0.68);
        vec3 deep = gOrb * vec3(0.70, 0.50, 0.52) + vec3(0.07, 0.01, 0.0);
        vec3 orb = mix(hot, gOrb, smoothstep(0.02, 0.80, rh));
        orb = mix(orb, deep, smoothstep(0.76, 1.03, r));
        // beat rim light: soft warm halo hugging the edge, breathing on beats
        float rim = smoothstep(0.78, 0.99, r) * smoothstep(1.02, 0.97, r);
        orb += vec3(0.95, 0.74, 0.40) * rim * (0.09 + 0.55 * gRim);
        // airbrush speckle inside the orb
        orb += (vnoise(uv * 26.0) - 0.5) * 0.045;
        lay(col, orb, d, px);
    }

    // ── sage-green blade, left side, apex pointing left ──
    {
        float sway = gSway * sin(gT * 0.31 + 1.7);
        vec2 bp = rot2(uv - (orbC + gGap * vec2(-0.40, 0.05)), 2.75 + 0.05 * sin(drift * 0.9) + sway);
        float d = sdBlade(bp, 0.30 * bladeSweep, 1.5, 0.082);
        dropShadow(col, sdBlade(bp - rot2(vec2(-0.028, -0.042), 3.05), 0.30 * bladeSweep, 1.5, 0.082), 0.08, 0.20);
        float t = gBladeT;
        vec3 sage = mix(vec3(0.35, 0.47, 0.30), vec3(0.63, 0.72, 0.47), t);
        sage = mix(sage, vec3(0.24, 0.44, 0.40), smoothstep(0.6, 1.0, t) * 0.5);
        lay(col, sage, d, px);
    }

    // ── second small sage sliver, lower left ──
    {
        float sway = gSway * sin(gT * 0.27 + 4.0);
        vec2 bp = rot2(uv - (orbC + gGap * vec2(-0.31, -0.20)), 2.25 + sway * 0.7);
        float d = sdBlade(bp, 0.21 * bladeSweep, 1.15, 0.042);
        float t = gBladeT;
        vec3 sage2 = mix(vec3(0.46, 0.56, 0.35), vec3(0.73, 0.78, 0.55), t);
        lay(col, sage2, d, px);
    }

    // ── cream blade sweeping over the orb's crown, tilted toward the right ──
    {
        float sway = gSway * sin(gT * 0.36);
        vec2 bp = rot2(uv - (orbC + gGap * vec2(-0.01, -0.02)), -1.28 + 0.045 * sin(drift * 1.1) + sway);
        float d = sdBlade(bp, 0.46 * bladeSweep, 1.45, 0.098);
        dropShadow(col, sdBlade(bp - rot2(vec2(-0.026, -0.048), -1.28), 0.46 * bladeSweep, 1.45, 0.098), 0.09, 0.26);
        float t = gBladeT;
        vec3 cr = mix(vec3(0.99, 0.97, 0.92), vec3(0.86, 0.82, 0.73), smoothstep(0.15, 0.95, t));
        cr = mix(cr, vec3(0.79, 0.71, 0.77), smoothstep(0.7, 1.0, t) * 0.4);
        lay(col, cr, d, px);
    }

    // ── big black blade: asymmetric sweep, apex up-left, long tail to the right ──
    {
        float sway = gSway * sin(gT * 0.33 + 0.8);
        vec2 bp = rot2(uv - (orbC + gGap * vec2(0.06, -0.045)), -1.98 + 0.04 * sin(drift) + sway);
        float d = sdBlade(bp, 0.55 * bladeSweep, 1.58, 0.078);
        dropShadow(col, sdBlade(bp - rot2(vec2(-0.028, -0.05), -1.98), 0.55 * bladeSweep, 1.58, 0.078), 0.10, 0.32);
        float t = gBladeT;
        vec3 blk = mix(vec3(0.05, 0.045, 0.05), vec3(0.15, 0.13, 0.12), pow(t, 2.0));
        blk += vec3(0.09, 0.08, 0.075) * pow(max(sin(t * 3.14159265), 0.0), 6.0) * 0.5; // faint sheen
        lay(col, blk, d, px);
    }

    // ── second black hook, right side, diving down ──
    {
        float sway = gSway * sin(gT * 0.29 + 2.6);
        vec2 bp = rot2(uv - (orbC + gGap * vec2(0.34, 0.03)), 0.95 + 0.05 * sin(drift * 1.3) + sway);
        float d = sdBlade(bp, 0.27 * bladeSweep, 1.05, 0.046);
        dropShadow(col, sdBlade(bp - rot2(vec2(-0.022, -0.04), 0.95), 0.27 * bladeSweep, 1.05, 0.046), 0.07, 0.24);
        float t = gBladeT;
        vec3 blk = mix(vec3(0.14, 0.12, 0.11), vec3(0.04, 0.04, 0.05), t);
        lay(col, blk, d, px);
    }

    // ── satellite pebbles ──
    {
        // teal-green glossy ellipse, right of the orb (slow orbit)
        vec2 pc = orbC + gGap * vec2(0.40 + 0.012 * sin(drift * 2.1), 0.185 + 0.010 * cos(drift * 1.6));
        vec2 pp = rot2(uv - pc, -0.35);
        float d = sdEllipse(pp, vec2(0.088, 0.046));
        dropShadow(col, sdEllipse(pp - vec2(-0.018, -0.028), vec2(0.088, 0.046)), 0.055, 0.20);
        float rr = length((pp - vec2(-0.018, 0.013)) / vec2(0.088, 0.046));
        vec3 teal = mix(vec3(0.62, 0.86, 0.74), vec3(0.16, 0.42, 0.47), smoothstep(0.1, 1.05, rr));
        teal = mix(teal, vec3(0.30, 0.62, 0.78), smoothstep(0.55, 1.0, rr) * 0.5);
        lay(col, teal, d, px);
    }
    {
        // small crimson pebble upper-left
        vec2 pc = orbC + gGap * vec2(-0.345 + 0.010 * sin(drift * 1.8), 0.315);
        float d = sdCircle(uv - pc, 0.035);
        dropShadow(col, sdCircle(uv - pc - vec2(-0.011, -0.017), 0.035), 0.038, 0.18);
        float rr = length(uv - pc - vec2(-0.010, 0.010)) / 0.035;
        vec3 red = mix(vec3(0.88, 0.48, 0.38), vec3(0.46, 0.12, 0.14), smoothstep(0.1, 1.1, rr));
        lay(col, red, d, px);
    }
    {
        // tiny slate pebble near the top
        vec2 pc = orbC + gGap * vec2(0.155 + 0.008 * cos(drift * 2.4), 0.415);
        float d = sdCircle(uv - pc, 0.019);
        float rr = length(uv - pc - vec2(-0.006, 0.006)) / 0.019;
        vec3 sl = mix(vec3(0.62, 0.60, 0.78), vec3(0.24, 0.22, 0.38), smoothstep(0.1, 1.1, rr));
        lay(col, sl, d, px);
    }

    // ── front segment of the wire crossing over the lower right ──
    {
        vec2 wp = rot2(uv - (orbC + gGap * vec2(0.24, -0.185)), 0.30 + 0.05 * sin(gT * 0.37 + 1.1));
        float dw = abs(sdEllipse(wp, vec2(0.36, 0.16))) - 0.0034;
        // only the lower-front sweep is drawn on top
        float gate = smoothstep(0.02, -0.06, wp.y);
        vec3 wcol = vec3(0.965, 0.945, 0.90);
        col = mix(col, wcol, smoothstep(px, -px, dw) * gate);
    }

    // ── canvas grain: fine tooth + subtle weave ──
    float tooth = hash21(gl_FragCoord.xy * 0.5);
    float weave = vnoise(uv * vec2(210.0, 190.0)) * 0.6 + vnoise(uv * vec2(90.0, 240.0)) * 0.4;
    col *= 1.0 + ((tooth - 0.5) * 0.030 + (weave - 0.5) * 0.050) * grainAmt;

    // highs: the faintest airbrush shimmer on the bright zones (sparse, gentle)
    col += vec3(0.03, 0.028, 0.022) * gHighP * gA * smoothstep(0.6, 0.9, dot(col, vec3(0.333)));

    // soft vignette like sprayed canvas corners
    col *= 1.0 - 0.085 * dot(uv, uv);

    // brightness with an audio lift that can dip below one
    float lift = mix(1.0, 0.82 + 0.30 * levelP, gA * 0.55);
    col *= brightness * lift;

    gl_FragColor = vec4(clamp(col, 0.0, 1.0), 1.0);
}
