/*{
  "DESCRIPTION": "Chromespike — raymarched chrome barbed clusters (spiked metaball strands) floating against a blue sky gradient. Strands sway on the audio time-clocks, chrome stripe reflections shift with spectral brightness, star glints fire on drum/high hits with eased decay, and the sky breathes with band presence. Layers: strands / glints / sky / reflections. No text.",
  "CREDIT": "ShaderClaw3 A-List VFX",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Abstract",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "glintAmount",
      "LABEL": "Star Glints",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "chromeContrast",
      "LABEL": "Reflections",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1
    },
    {
      "NAME": "spikeLength",
      "LABEL": "Strand Spikes",
      "TYPE": "float",
      "MIN": 0.2,
      "MAX": 1.6,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "swayAmount",
      "LABEL": "Strand Sway",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "swayRate",
      "LABEL": "Sway Rate",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 1.0,
      "MIN": 0.0,
      "MAX": 4.0
    },
    {
      "NAME": "tumble",
      "LABEL": "Cluster Tumble",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.0,
      "MIN": -2.0,
      "MAX": 2.0
    },
    {
      "NAME": "bobAmount",
      "LABEL": "Cluster Bob",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 1.0
    },
    {
      "NAME": "depthDrift",
      "LABEL": "Depth Drift",
      "TYPE": "float",
      "GROUP": "Motion / Animation",
      "DEFAULT": 0.0,
      "MIN": 0.0,
      "MAX": 1.0
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
      "NAME": "skyGlow",
      "LABEL": "Sky",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 1.6,
      "DEFAULT": 1,
      "GROUP": "Camera / Layout"
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

float h11(float n){ return fract(sin(n * 127.1) * 43758.5453); }

mat2 rot2(float a){ float c = cos(a), s = sin(a); return mat2(c, -s, s, c); }

// one spiked chrome cluster: sphere body + sharp radial thorn lobes
float cluster(vec3 q, float r, float spk){
  float l = length(q);
  vec3 n = q / max(l, 1e-4);
  float az = atan(n.y, n.x);
  float el = acos(clamp(n.z, -1.0, 1.0));
  float S = abs(sin(az * 3.0) * sin(el * 4.0));
  float thorn = pow(S, 7.0);
  return l - r * (0.40 + spk * thorn);
}

// scene state shared between map() and main via globals
float gSway;
float gSpk;
float gBob;    // MOTION: vertical bob amplitude
float gDepth;  // MOTION: z parallax drift amplitude

float map(vec3 p){
  float d = 1e5;
  // four hand-placed clusters (depth spread for parallax)
  vec3 q;
  q = p - vec3(-0.95, 0.60 + gBob * sin(TIME * 0.70 + 0.0), 0.30 + gDepth * sin(TIME * 0.31 + 1.0));
  q.xy *= rot2(gSway * 1.00 + 0.9); q.yz *= rot2(gSway * 0.70 + 2.1);
  d = min(d, cluster(q, 0.72, gSpk));

  q = p - vec3(0.95, 0.75 + gBob * sin(TIME * 0.62 + 1.9), 1.10 + gDepth * sin(TIME * 0.27 + 3.2));
  q.xy *= rot2(-gSway * 0.80 + 4.2); q.xz *= rot2(gSway * 0.60 + 1.3);
  d = min(d, cluster(q, 0.80, gSpk * 1.1));

  q = p - vec3(0.85, -0.80 + gBob * sin(TIME * 0.77 + 3.7), 0.10 + gDepth * sin(TIME * 0.35 + 5.1));
  q.xy *= rot2(gSway * 0.90 + 5.6); q.yz *= rot2(-gSway * 0.55 + 0.4);
  d = min(d, cluster(q, 0.68, gSpk));

  q = p - vec3(-0.70, -0.85 + gBob * sin(TIME * 0.58 + 5.4), 0.90 + gDepth * sin(TIME * 0.29 + 0.4));
  q.xy *= rot2(-gSway * 0.65 + 2.8); q.xz *= rot2(gSway * 0.85 + 3.7);
  d = min(d, cluster(q, 0.74, gSpk * 0.9));

  q = p - vec3(0.05, -0.05 + gBob * sin(TIME * 0.50 + 2.6), 2.30 + gDepth * sin(TIME * 0.23 + 2.2)); // far center cluster
  q.xy *= rot2(gSway * 0.50 + 1.5); q.yz *= rot2(gSway * 0.40 + 4.9);
  d = min(d, cluster(q, 0.85, gSpk * 0.8));
  return d;
}

vec3 calcNormal(vec3 p){
  vec2 e = vec2(0.004, -0.004);
  return normalize(e.xyy * map(p + e.xyy) + e.yyx * map(p + e.yyx)
                 + e.yxy * map(p + e.yxy) + e.xxx * map(p + e.xxx));
}

vec3 sky(vec3 d, float breathe, float glowAmt){
  vec3 zen = vec3(0.10, 0.22, 0.66) * breathe;
  vec3 hor = vec3(0.72, 0.80, 0.96);
  float t = smoothstep(-0.35, 0.75, d.y);
  vec3 c = mix(hor, zen, t) * glowAmt;
  // soft high sun glow
  c += vec3(0.30, 0.32, 0.40) * pow(max(0.0, dot(d, normalize(vec3(0.35, 0.85, -0.2)))), 6.0) * glowAmt;
  return c;
}

void main(){
  vec2 uv = gl_FragCoord.xy / RENDERSIZE.xy;
  float aspect = RENDERSIZE.x / RENDERSIZE.y;
  vec2 p = (uv - 0.5) * vec2(aspect, 1.0) * 2.0;

  float react = audioReactivity;

  // ---- clocks & envelopes
  float swayT = TIME * 0.05 * swayRate + (audioTime * 0.14 + audioMidTime * 0.14) * react; // integrated sway clocks (MOTION: swayRate)
  gSway = swayAmount * (0.35 * sin(swayT) + 0.12 * sin(swayT * 2.7 + 1.0)) + swayT * 0.15
        + TIME * tumble * 0.25;          // MOTION: continuous per-cluster tumble
  gBob   = bobAmount  * 0.25;            // MOTION: clusters bob up / down
  gDepth = depthDrift * 0.60;            // MOTION: clusters drift toward / away (parallax)
  // thorns swell LINEARLY on the bass stem (fast) + presence (slow hold)
  gSpk  = spikeLength * (1.0 + react * (0.10 * stemBass + 0.14 * audioBassPresence));

  float breathe = 0.80 + react * (0.20 * audioMid + 0.30 * audioMidPresence); // sky breathes

  vec3 ro = vec3(0.0, 0.0, -3.1);
  vec3 rd = normalize(vec3(p, 1.75));

  // ---- raymarch (conservative factor: thorn field is steep)
  float t = 0.0;
  float d = 1e5;
  vec3 pos = ro;
  for (int i = 0; i < 80; i++){
    pos = ro + rd * t;
    d = map(pos);
    if (d < 0.004 || t > 8.0) break;
    t += d * 0.45;
  }

  vec3 col;
  if (d < 0.02 && t <= 8.0){
    // =================== LAYER 1 — chrome strands ============================
    vec3 nor = calcNormal(pos);
    vec3 refl = reflect(rd, nor);

    // =================== LAYER 4 — reflections ===============================
    // liquid-chrome environment: sky + hard bright/dark stripe bands.
    // audioBrightness slides the stripe phase = reflections shimmer with timbre
    vec3 env = sky(refl, breathe, skyGlow);
    // stripes slide on the integrated bass clock (derivative = bass envelope,
    // so every chrome pixel's frame-delta tracks the music) + brightness shimmer
    float stripe = sin((refl.y * 7.0 + refl.x * 2.4) + audioBassTime * 1.6 * react
                       + audioBrightness * 4.0 * react + TIME * 0.15);
    float bandHi = smoothstep(0.15, 0.85, stripe);
    vec3 chrome = mix(env * 0.22, env * 1.35 + vec3(0.42), bandHi);
    col = mix(env, chrome, clamp(chromeContrast, 0.0, 1.0));
    col += (chrome - env) * max(chromeContrast - 1.0, 0.0); // >1 pushes contrast harder

    // fresnel pink/magenta bleed (the ref's candy tint), hue eased by brightness
    float fr = pow(1.0 - max(0.0, dot(-rd, nor)), 3.0);
    vec3 tint = mix(vec3(0.95, 0.30, 0.65), vec3(0.45, 0.55, 0.98), 0.5 + 0.5 * sin(audioBrightness * 3.0 + TIME * 0.1));
    col += fr * tint * 0.55;

    // dark grazing occlusion = inked edges, keeps the metal graphic
    col *= 0.25 + 0.75 * smoothstep(0.0, 0.35, abs(dot(nor, -rd)));

    // key light spec
    vec3 L = normalize(vec3(0.4, 0.8, -0.45));
    col += vec3(1.0) * pow(max(0.0, dot(refl, L)), 24.0) * 0.8;
  } else {
    // ======================= LAYER 3 — sky ===================================
    col = sky(rd, breathe, skyGlow);
    // faint drifting cirrus shimmer so the sky is alive in silence
    col += vec3(0.05, 0.06, 0.10) * sin(p.y * 6.0 + p.x * 2.0 + TIME * 0.4) * 0.5;
  }

  // ===================== LAYER 2 — star glints ===============================
  // eased decaying hit envelopes (bus Hits already decay); sparse fixed anchors
  for (int k = 0; k < 6; k++){
    float fk = float(k);
    vec2 gp = vec2(h11(fk * 7.3 + 1.7) * 2.0 - 1.0, h11(fk * 3.1 + 9.2) * 2.0 - 1.0) * vec2(aspect * 0.9, 0.9);
    gp += 0.05 * vec2(sin(TIME * 0.21 + fk * 2.0), cos(TIME * 0.17 + fk * 1.4));
    float env = (k < 3 ? stemDrumsHit : audioHighHit) * react;
    env = env * (0.45 + 0.55 * h11(fk * 13.7)); // per-glint gain lag
    env += 0.10 * pow(max(0.0, sin(TIME * 0.6 + fk * 2.4)), 8.0); // rare idle twinkle
    vec2 v = p - gp;
    float l = length(v);
    float star = pow(max(0.0, 1.0 - abs(v.x) * 26.0), 3.0) * pow(max(0.0, 1.0 - abs(v.y) * 3.5), 2.0)
               + pow(max(0.0, 1.0 - abs(v.y) * 26.0), 3.0) * pow(max(0.0, 1.0 - abs(v.x) * 3.5), 2.0);
    star += 0.9 * smoothstep(0.05, 0.0, l);
    col += vec3(1.0, 0.98, 0.95) * star * env * 0.7 * glintAmount;
  }

  // whole-frame LINEAR follower (silence multiplies to exactly 1.0)
  col *= 1.0 + react * (0.16 * audioBass + 0.18 * audioMid + 0.10 * audioEnergy);

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
  // background = the sky miss region
  float bgMask = (d < 0.02 && t <= 8.0) ? 0.0 : 1.0;
  col = mix(col, bgColor.rgb, bgColor.a * bgMask);

  gl_FragColor = vec4(col, 1.0);
}
