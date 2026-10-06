/*{
  "DESCRIPTION": "Neon Growth — electric blue / red / hot pink / yellow neon colonies growing over a glowing blue ink skeleton on near-black, high contrast. stemMelody (+ its Presence) feeds continuous colony growth via a domain-warped field advected on the audioMidTime clock; stemDrums spawns spore pops that ride audioPhase2 ramps; treble makes the electric-blue outline glow breathe. Ink skeleton moves freely with music. Colonies are lit as puffy 3D matter (gradient lighting + cast shadow). No text.",
  "CREDIT": "ShaderClaw",
  "CATEGORIES": [
    "Generator",
    "Audio"
  ],
  "INPUTS": [
    {
      "NAME": "glowAmount",
      "LABEL": "Outline Glow",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2
    },
    {
      "NAME": "inkAmount",
      "LABEL": "Ink Skeleton",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "inkMotion",
      "LABEL": "Ink Motion Amount",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "colonyGrowth",
      "LABEL": "Colony Growth",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "sporeAmount",
      "LABEL": "Spore Pops",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Motion / Animation"
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
        0.012,
        0.012,
        0.03,
        1
      ],
      "GROUP": "Background"
    },
    {
      "NAME": "audioReactivity",
      "LABEL": "Audio Reactivity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ============================================================================
// NEON GROWTH — A-List VFX #2
//   Ink skeleton now warps/shifts/breathes freely with all audio bands.
// ============================================================================

#define TAU 6.2831853

float hash21(vec2 p){
  p = fract(p * vec2(123.34, 456.21));
  p += dot(p, p + 45.32);
  return fract(p.x * p.y);
}

float vnoise(vec2 p){
  vec2 i = floor(p), f = fract(p);
  vec2 u = f * f * (3.0 - 2.0 * f);
  float a = hash21(i);
  float b = hash21(i + vec2(1.0, 0.0));
  float c = hash21(i + vec2(0.0, 1.0));
  float d = hash21(i + vec2(1.0, 1.0));
  return mix(mix(a, b, u.x), mix(c, d, u.x), u.y);
}

float fbm3(vec2 p){
  float v = 0.0, a = 0.5;
  for (int i = 0; i < 3; i++){
    v += a * vnoise(p);
    p = p * 2.03 + vec2(17.3, 9.1);
    a *= 0.5;
  }
  return v * 1.1428;
}

float fbm5(vec2 p){
  float v = 0.0, a = 0.5;
  for (int i = 0; i < 5; i++){
    v += a * vnoise(p);
    p = p * 2.03 + vec2(17.3, 9.1);
    a *= 0.5;
  }
  return v * (1.0 / (1.0 - pow(0.5, 5.0)));
}

// domain-warped colony field
float colonyField(vec2 p, float t1){
  vec2 q = p + 1.0 * vec2(fbm3(p * 1.9 + vec2(0.0, t1)) - 0.5,
                          fbm3(p * 1.9 + vec2(5.2, -t1 * 0.8)) - 0.5);
  return fbm3(q * 2.6 + vec2(t1 * 0.35, 1.7));
}

vec3 hueRotate(vec3 c, float a){
  float an = a * TAU;
  vec3 k = vec3(0.57735);
  float cs = cos(an), sn = sin(an);
  return c * cs + cross(k, c) * sn + k * dot(k, c) * (1.0 - cs);
}

// Smooth 2D rotation
vec2 rot2(vec2 v, float a){
  float cs = cos(a), sn = sin(a);
  return vec2(v.x * cs - v.y * sn, v.x * sn + v.y * cs);
}

void main(){
  vec2 uv = isf_FragNormCoord;
  vec2 p = uv - 0.5;
  p.x *= RENDERSIZE.x / RENDERSIZE.y;
  float react = audioReactivity;
  float px = 1.0 / RENDERSIZE.y;
  float pw = px * 0.75;

  // --- conditioned audio drivers ---
  float melL  = clamp(0.50 * stemMelody + 0.60 * stemMelodyPresence + 0.30 * audioMid, 0.0, 1.3) * react;
  float highL = clamp(0.55 * audioHigh + 0.55 * stemAir, 0.0, 1.3) * react;
  float bassL = clamp(0.70 * audioBass + 0.50 * stemBass, 0.0, 1.3) * react;
  float lvlL  = clamp(audioLevel, 0.0, 1.0) * react;
  float drumK = smoothstep(0.04, 0.75, stemDrums) * react;
  float midL  = clamp(audioMid, 0.0, 1.0) * react;

  // integrated growth clocks
  float t1     = TIME * 0.055 + audioMidTime * 0.10;
  float tCrawl = TIME * 0.7   + audioMidTime * 2.4;

  // --- INK SKELETON MOTION DRIVERS ---
  // Build a continuously-moving warp for the ink coordinate system.
  // Multiple independent oscillations on each audio band so the skeleton
  // writhes, breathes, shifts, and rotates with the music.

  float inkT = TIME * 0.18 + audioMidTime * 0.55;  // integrated ink clock

  // Global translation that sways with bass and melody
  float txBass = 0.032 * bassL * sin(TIME * 1.30 + 0.0) * inkMotion;
  float tyBass = 0.028 * bassL * cos(TIME * 1.07 + 1.1) * inkMotion;
  float txMel  = 0.018 * melL  * sin(TIME * 0.73 + 2.3) * inkMotion;
  float tyMel  = 0.022 * melL  * cos(TIME * 0.59 + 0.8) * inkMotion;
  // Treble adds a fast jitter
  float txHi   = 0.009 * highL * sin(TIME * 4.10 + 0.5) * inkMotion;
  float tyHi   = 0.009 * highL * cos(TIME * 3.80 + 1.7) * inkMotion;

  vec2 inkShift = vec2(txBass + txMel + txHi, tyBass + tyMel + tyHi);

  // Global rotation breathing with mid + bass
  float inkRot = 0.12 * bassL * sin(TIME * 0.44 + 0.3) * inkMotion
               + 0.06 * midL  * cos(TIME * 0.67 + 1.5) * inkMotion
               + 0.03 * highL * sin(TIME * 2.10 + 0.9) * inkMotion;

  // Scale breath: bass pumps, melody stretches
  float inkScale = 1.0 + 0.08 * bassL * sin(TIME * 1.15) * inkMotion
                       + 0.04 * melL  * cos(TIME * 0.82 + 2.0) * inkMotion;

  // Domain warp of the ink coordinate: a slow low-freq fbm warp that
  // SPEEDS UP with audio energy (integrated clock, so speed ~ energy)
  float warpAmp = inkMotion * (0.040 + 0.070 * bassL + 0.035 * melL + 0.020 * highL);
  float wx = warpAmp * (fbm3(p * 1.4 + vec2(0.0,   inkT * 0.9)) - 0.5);
  float wy = warpAmp * (fbm3(p * 1.4 + vec2(8.3,  -inkT * 0.7)) - 0.5);
  vec2 inkWarp = vec2(wx, wy);

  // Assemble final ink coordinate
  vec2 pi = p + inkShift + inkWarp;         // translate + warp
  pi = rot2(pi, inkRot);                    // rotate
  pi *= inkScale;                           // scale

  // Diagonal coordinate for rotated strokes (uses pi, not p)
  vec2 piR = rot2(pi, 0.7854); // 45 deg — same for all diagonal strokes

  // --- layer 0: near-black ground ---
  vec3 paper = (bgColor.a > 0.004) ? bgColor.rgb : vec3(0.012, 0.012, 0.03);
  paper *= 0.895 + 0.21 * smoothstep(0.5 - 10.0 * px, 0.5 + 10.0 * px, vnoise(p * 90.0));
  paper *= 1.0 - 0.60 * dot(uv - 0.5, uv - 0.5);

  // --- layer 1: ink skeleton (uses animated pi / piR) ---
  float mask = smoothstep(0.40, 0.60, fbm3(pi * 1.25 + vec2(41.3, 17.9)));
  float l1 = 1.0 - smoothstep(26.0 * pw * 0.6, 26.0 * pw * 1.5, abs(vnoise(pi * vec2(26.0, 6.0) + 3.1) - 0.5));
  float l2 = 1.0 - smoothstep(28.0 * pw * 0.6, 28.0 * pw * 1.5, abs(vnoise(pi * vec2(6.0, 28.0) + 9.7) - 0.5));
  float l3 = 1.0 - smoothstep(20.0 * pw * 0.6, 20.0 * pw * 1.5, abs(vnoise(piR * vec2(20.0, 7.0) + 6.3) - 0.5));
  float l4 = 1.0 - smoothstep(44.0 * pw * 0.5, 44.0 * pw * 1.3, abs(vnoise(pi * vec2(44.0, 12.0) + 14.9) - 0.5));
  float ink = mask * clamp(l1 + 0.9 * l2 + 0.85 * l3 + 0.7 * l4, 0.0, 1.0);
  ink = clamp(ink * inkAmount, 0.0, 1.0);
  vec3 col = mix(paper, vec3(0.04, 0.14, 0.45), ink * 0.94);   // electric-blue skeleton

  // pale speckle dots (static — on paper, not warped)
  vec2 spCell = floor(p * 70.0);
  float spDot = step(0.945, hash21(spCell + 0.7))
              * smoothstep(0.30, 0.30 - 1.6 * 70.0 * px, length(fract(p * 70.0) - 0.5));
  col = mix(col, vec3(0.92, 0.90, 1.00), spDot * 0.8);

  // pencil under-hatch (warped with ink)
  float l5 = 1.0 - smoothstep(36.0 * pw * 0.5, 36.0 * pw * 1.3,
                              abs(vnoise(piR * vec2(36.0, 9.0) + 21.7) - 0.5));
  col = mix(col, vec3(0.08, 0.16, 0.38), (1.0 - mask) * l5 * clamp(inkAmount, 0.0, 1.0) * 0.64);

  // --- layer 2: neon colonies (uses original p) ---
  float F = colonyField(p, t1);
  float thr = 0.560 - 0.105 * colonyGrowth * melL - 0.075 * lvlL
            - 0.010 * sin(TIME * 0.31) - 0.008 * sin(TIME * 0.53 + 1.7)
            + 0.014 * sin(p.x * 21.0 + p.y * 17.0 + tCrawl);
  float s = F - thr;
  float body = smoothstep(0.0, 0.010, s);

  // cast shadow
  float Fs = colonyField(p + vec2(-0.035, 0.05), t1);
  float shad = smoothstep(thr, thr + 0.05, Fs) * (1.0 - body) * 0.25;
  col *= 1.0 - shad;

  // contact rim — electric blue on the black ground
  float rim = exp(-abs(s + 0.020) / (2.7 * px)) * (1.0 - body);
  col = mix(col, vec3(0.05, 0.30, 0.95), rim * 0.85);

  // satellite neon-yellow colonies
  float g = fbm3(p * 2.8 + vec2(13.1, 7.7) + t1 * 0.25);
  float thrG = 0.660 - 0.045 * colonyGrowth * melL;
  float gBody = smoothstep(thrG, thrG + 4.0 * px, g) * (1.0 - body);
  vec3 limeC = vec3(1.00, 0.86, 0.05) * (1.0 + 0.25 * melL);
  col = mix(col, vec3(0.10, 0.08, 0.02), exp(-abs(g - thrG) / (3.0 * px)) * (1.0 - body) * (1.0 - gBody) * 0.55);
  col = mix(col, limeC, gBody * 0.95);
  col = mix(col, vec3(0.40, 0.60, 1.00),
            exp(-abs(g - thrG) / (4.5 * px)) * (1.0 - body) * glowAmount * (0.12 + 0.30 * highL));

  if (s > -0.20){
    float tt = smoothstep(0.0, 0.24, s);
    vec3 cc = mix(vec3(1.00, 0.04, 0.10), vec3(1.00, 0.12, 0.58), tt);   // red -> hot pink
    cc = mix(cc, vec3(1.00, 0.88, 0.10), smoothstep(0.75, 1.0, tt));     // yellow crowns
    cc = mix(cc, vec3(0.10, 0.40, 1.00), smoothstep(0.60, 0.72, g) * 0.9); // electric-blue islands
    float pores = smoothstep(0.54, 0.60, vnoise(p * 34.0 + 3.7));
    cc = mix(cc, vec3(0.30, 0.01, 0.12), pores * (0.45 + 0.55 * tt));
    float e = 0.016;
    float Fx = colonyField(p + vec2(e, 0.0), t1) - F;
    float Fy = colonyField(p + vec2(0.0, e), t1) - F;
    float lit = clamp((Fx * (-0.6) + Fy * 0.8) / e, -1.0, 1.0);
    cc *= 1.0 + 0.28 * lit;
    cc += vec3(1.0, 0.9, 0.7) * pow(max(lit, 0.0), 3.0) * 0.25;
    cc *= 1.16 + 0.18 * melL + 0.14 * bassL;   // hotter neon on the black ground
    col = mix(col, cc, body);
  }

  // --- layer 4: electric-blue outline glow ---
  float halo = exp(-abs(s) * (38.0 - 16.0 * clamp(highL, 0.0, 1.0)));
  col = mix(col, vec3(0.30, 0.52, 1.00), halo * glowAmount * (0.34 + 0.62 * highL));
  float tubeCore = smoothstep(5.2 * px, 1.6 * px, abs(s));
  col = mix(col, vec3(0.88, 0.94, 1.00), tubeCore * min(glowAmount, 1.0) * (0.85 + 0.15 * highL));
  float tubeCoreG = smoothstep(5.2 * px, 1.6 * px, abs(g - thrG)) * (1.0 - body);
  col = mix(col, vec3(1.00, 0.92, 0.50), tubeCoreG * min(glowAmount, 1.0) * (0.50 + 0.30 * highL));

  // --- layer 3: spore pops ---
  vec2 sp = p * 6.5;
  vec2 cell = floor(sp);
  float rnd = hash21(cell);
  if (rnd > 0.45){
    vec2 ctr = cell + 0.5 + (vec2(hash21(cell + 11.1), hash21(cell + 27.7)) - 0.5) * 0.7;
    float r = length(sp - ctr);
    float age = fract(audioPhase2 + TIME * 0.13 + rnd * 7.0);
    float R = mix(0.06, 0.38, age);
    float alpha = pow(1.0 - age, 1.8);
    float pxs = 6.5 * px;
    float dsk = smoothstep(R, R - 1.6 * pxs, r);
    float ring = smoothstep(1.4 * pxs, 0.2 * pxs, abs(r - R)) * 0.75
               + smoothstep(0.16, 0.0, abs(r - R)) * 0.18;
    float hue = hash21(cell + 5.5);
    vec3 scol = hue < 0.4 ? vec3(1.00, 0.14, 0.60)
              : (hue < 0.75 ? vec3(0.25, 0.55, 1.00) : vec3(1.00, 0.90, 0.28));
    float ringD = smoothstep(1.6 * pxs, 0.4 * pxs, abs(r - R - 1.2 * pxs));
    col = mix(col, vec3(0.08, 0.06, 0.08),
              ringD * alpha * (0.62 + 0.30 * drumK) * sporeAmount);
    float sInt = (dsk + ring) * alpha * (0.50 + 0.50 * drumK) * sporeAmount;
    col = mix(col, scol, clamp(sInt, 0.0, 1.0) * 0.85);
  }

  col *= 1.0 - clamp(react * (0.14 * audioBass + 0.07 * audioMid), 0.0, 0.26);

  col = hueRotate(col, hueShift);
  float lum = dot(col, vec3(0.299, 0.587, 0.114));
  col = mix(vec3(lum), col, colorBoost);

  gl_FragColor = vec4(col, 1.0);
}