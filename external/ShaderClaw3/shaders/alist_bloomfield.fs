/*{
  "DESCRIPTION": "Bloomfield — a cluster of layered rippling rosettes (concentric scalloped rings, teal/blue/pink on cream) blooming radially. Each rosette breathes on its own band (bass = biggest), ring detail follows the mids, cores pulse-dip on hits, and the whole cluster slow-orbits on the audio time-clock. Layers: rosettes / cores / backdrop disc / grain. No text.",
  "CREDIT": "ShaderClaw3 A-List VFX",
  "CATEGORIES": [
    "Generator",
    "Abstract",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "coreGlow",
      "LABEL": "Cores",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "grainAmount",
      "LABEL": "Grain",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0.35
    },
    {
      "NAME": "rosetteAmount",
      "LABEL": "Rosettes",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "discAmount",
      "LABEL": "Backdrop Disc",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "bloomSpeed",
      "LABEL": "Bloom Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "clusterOrbit",
      "LABEL": "Cluster Orbit",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 4.0
    },
    {
      "NAME": "rosetteDrift",
      "LABEL": "Rosette Drift",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 4.0
    },
    {
      "NAME": "petalWaver",
      "LABEL": "Petal Waver",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 3.0
    },
    {
      "NAME": "ringFlow",
      "LABEL": "Ring Flow",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 1.0,
      "MIN": -3.0,
      "MAX": 3.0
    },
    {
      "NAME": "hueShift",
      "LABEL": "Hue Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "LABEL": "Color Boost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
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
      "NAME": "audioReactivity",
      "LABEL": "Sound Reactivity",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

float h21(vec2 p){ p = fract(p * vec2(123.34, 456.21)); p += dot(p, p + 45.32); return fract(p.x * p.y); }

void main(){
  vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
  float aspect = RENDERSIZE.x / RENDERSIZE.y;
  vec2 p = (uv - 0.5) * vec2(aspect, 1.0);

  float react = audioReactivity;

  // slow cluster orbit on the audio time-clock (integrated, never raw position)
  float orbit = TIME * 0.03 * clusterOrbit + audioTime * 0.05 * react; // MOTION: idle orbit rate
  float co = cos(orbit), so = sin(orbit);
  mat2 R = mat2(co, -so, so, co);
  vec2 pr = R * p;

  float bloomT = TIME * 0.12 * bloomSpeed + audioTime * 0.10 * react;

  // ======================= LAYER 3 — backdrop disc =============================
  vec3 cream = vec3(0.91, 0.86, 0.75);
  vec3 col = cream;
  float r0 = length(p);
  float discR = 0.46 + 0.02 * sin(TIME * 0.3);
  vec3 discBlue = vec3(0.13, 0.24, 0.86);
  float disc = smoothstep(discR + 0.10, discR - 0.22, r0);
  float halo = smoothstep(discR + 0.16, discR - 0.05, r0) - disc; // soft blue halo edge
  col = mix(col, discBlue, discAmount * disc);
  col = mix(col, mix(cream, discBlue, 0.45), discAmount * clamp(halo, 0.0, 1.0));

  // ======================= LAYER 1+2 — rosettes + cores ========================
  // seven rosettes, back-to-front; each owns a band (frequency -> space: bass = biggest)
  vec3 teal    = vec3(0.05, 0.42, 0.40);
  vec3 tealHi  = vec3(0.55, 0.78, 0.74);
  vec3 pinkHi  = vec3(0.93, 0.72, 0.68);
  vec3 coreBlu = vec3(0.16, 0.28, 0.90);
  vec3 coral   = vec3(0.97, 0.58, 0.48);

  float ringN = 7.0 + 3.0 * react * audioMid; // mids add scallop/ring detail
  float covered = 0.0; // rosette coverage, for honest bgColor masking

  for (int i = 0; i < 7; i++){
    float fi = float(i);
    float h = h21(vec2(fi * 3.7, 11.0));
    // hand-laid cluster (back = biggest first, front = small last)
    vec2 c;
    float R0;
    float band; float hit;
    if      (i == 0){ c = vec2(-0.02,  0.04); R0 = 0.335; band = stemBass;         hit = stemDrumsHit; }
    else if (i == 1){ c = vec2( 0.26,  0.22); R0 = 0.255; band = audioMid;         hit = audioMidHit; }
    else if (i == 2){ c = vec2(-0.28,  0.24); R0 = 0.215; band = stemMelody;       hit = stemMelodyHit; }
    else if (i == 3){ c = vec2( 0.30, -0.18); R0 = 0.205; band = audioHigh;        hit = audioHighHit; }
    else if (i == 4){ c = vec2(-0.26, -0.22); R0 = 0.185; band = stemAir;          hit = stemAirHit; }
    else if (i == 5){ c = vec2( 0.02, -0.30); R0 = 0.205; band = stemDrums;        hit = stemDrumsHit; }
    else            { c = vec2( 0.01,  0.31); R0 = 0.165; band = stemVocal;        hit = audioHighHit; }

    // gentle independent drift so nothing moves in lockstep
    c += 0.020 * rosetteDrift * vec2(sin(audioTime * 0.23 * react + TIME * 0.11 + fi * 2.1), // MOTION: drift reach
                      cos(audioTime * 0.19 * react + TIME * 0.09 + fi * 1.7));

    vec2 q = pr - c;
    float r = length(q);
    float a = atan(q.y, q.x);

    // per-band radius breathing — LINEAR follower, per-rosette phase lag
    float rad = R0 * (1.0 + 0.11 * react * band);

    // scalloped edge: two petal frequencies + slow waver
    float nPet = 6.0 + fi;
    float wob = 1.0 + petalWaver * (0.075 * sin(a * nPet + bloomT * (0.7 + 0.13 * fi) + fi * 2.3)
                                  + 0.045 * sin(a * (nPet * 2.0 + 3.0) - bloomT * 1.3 + h * 6.28)); // MOTION: scallop waver
    float s = r / (rad * wob);

    if (s < 1.0){
      // concentric rippled rings blooming outward
      float ring = fract(s * ringN - bloomT * (0.9 + 0.2 * h) * ringFlow - fi * 0.37); // MOTION: ring bloom rate / direction
      float tri = abs(ring - 0.5) * 2.0;
      float bandMix = smoothstep(0.25, 0.75, tri);
      vec3 rc = mix(teal, mix(tealHi, pinkHi, smoothstep(0.25, 0.95, s + 0.3 * h)), bandMix);
      // scallop relief lighting: ring edges shade darker, crests catch light (3D feel)
      float lit = 0.78 + 0.30 * smoothstep(0.0, 0.6, tri) - 0.22 * smoothstep(0.85, 1.0, tri);
      lit *= 1.06 - 0.28 * s * s; // dome falloff
      rc *= lit;

      // core: blue ring around a coral heart, pulse-DIP on hits (chop-safe darken)
      float coreS = smoothstep(0.30, 0.10, s);
      vec3 core = mix(coreBlu, coral, smoothstep(0.16, 0.05, s));
      core *= 1.0 - 0.34 * react * hit;
      rc = mix(rc, core * (0.6 + 0.4 * coreGlow), coreS * min(coreGlow, 1.0));
      rc += coral * 0.25 * coreGlow * smoothstep(0.09, 0.0, s); // hot center

      float edge = smoothstep(1.0, 0.955, s);
      col = mix(col, rc, rosetteAmount * edge);
      covered = max(covered, rosetteAmount * edge);
    }
  }

  // ============================ LAYER 4 — grain ================================
  float gr = h21(gl_FragCoord.xy + fract(TIME) * 61.7) - 0.5;
  col += gr * 0.05 * grainAmount;

  // whole-frame LINEAR darken-dip follower (bright scene: dips can't clip)
  col *= 1.0 - react * (0.13 * audioBass + 0.09 * audioMid);

  // ---- universal color block (defaults = no-op)
  float ucL = dot(col, vec3(0.299, 0.587, 0.114));
  col = mix(vec3(ucL), col, colorBoost);
  if (hueShift > 0.0005){
    float hA = hueShift * 6.2831853;
    float hC = cos(hA), hS = sin(hA);
    mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
            + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
            + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
    col = clamp(hM * col, 0.0, 1.5);
  }
  // background = the cream field outside the disc, not covered by rosettes
  float bgMask = smoothstep(discR - 0.02, discR + 0.14, r0) * (1.0 - covered);
  col = mix(col, bgColor.rgb, bgColor.a * clamp(bgMask, 0.0, 1.0));

  gl_FragColor = vec4(col, 1.0);
}
