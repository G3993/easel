/*{
  "DESCRIPTION": "Mandelbox Dive — raymarched 3D Mandelbox with the camera flying into the folds. Audio-reactive: bass pushes velocity, mid breathes the fold scale, treble ignites edge glow. Calm constant forward drift at silence.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "Fractal",
    "3D",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "glow",
      "TYPE": "float",
      "DEFAULT": 0.5,
      "MIN": 0,
      "MAX": 1.5,
      "LABEL": "Glow Intensity"
    },
    {
      "NAME": "fogDensity",
      "TYPE": "float",
      "DEFAULT": 0.12,
      "MIN": 0,
      "MAX": 0.6,
      "LABEL": "Fog Density"
    },
    {
      "NAME": "inputImage",
      "LABEL": "Your Image",
      "TYPE": "image"
    },
    {
      "NAME": "texMix",
      "LABEL": "Image Amount",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1
    },
    {
      "NAME": "scaleParam",
      "TYPE": "float",
      "DEFAULT": 2.4,
      "MIN": 1.8,
      "MAX": 3,
      "LABEL": "Fold Scale",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "camSpeed",
      "LABEL": "Dive Speed",
      "TYPE": "float",
      "DEFAULT": 0.1,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "paletteShift",
      "TYPE": "float",
      "DEFAULT": 0,
      "MIN": 0,
      "MAX": 1,
      "LABEL": "Palette Shift",
      "GROUP": "Color"
    },
    {
      "NAME": "colorBoost",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "LABEL": "Color Boost",
      "GROUP": "Color"
    },
    {
      "NAME": "bgColor",
      "TYPE": "color",
      "DEFAULT": [
        0,
        0,
        0,
        0
      ],
      "LABEL": "Background",
      "GROUP": "Background"
    },
    {
      "NAME": "audioReact",
      "LABEL": "Sound Reactivity",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 2,
      "GROUP": "Audio Reactivity"
    }
  ]
}*/

// ---- curated cosine palette (house style) ----
vec3 pal(float t){ return 0.5 + 0.5*cos(6.28318*(t + vec3(0.0,0.33,0.67))); }

// fold scale modulated slightly by mid (K <= 1.5)
float gScale;

// ---- Mandelbox distance estimator ----
// orbit-trap radius returned via global for coloring
float gTrap;
float mandelboxDE(vec3 p){
  vec3 z = p; float dr = 1.0; float scale = gScale;
  float trap = 1e9;
  for(int n=0;n<12;n++){
    z = clamp(z,-1.0,1.0)*2.0 - z;            // box fold
    float r2 = dot(z,z);
    if(r2<0.25){ z*=4.0; dr*=4.0; }            // sphere fold (inner)
    else if(r2<1.0){ float t=1.0/r2; z*=t; dr*=t; }
    z = z*scale + p; dr = dr*abs(scale) + 1.0;
    trap = min(trap, length(z));
  }
  gTrap = trap;
  return length(z)/abs(dr);
}

// DE without trap bookkeeping (for normals)
float mapDE(vec3 p){
  vec3 z = p; float dr = 1.0; float scale = gScale;
  for(int n=0;n<12;n++){
    z = clamp(z,-1.0,1.0)*2.0 - z;
    float r2 = dot(z,z);
    if(r2<0.25){ z*=4.0; dr*=4.0; }
    else if(r2<1.0){ float t=1.0/r2; z*=t; dr*=t; }
    z = z*scale + p; dr = dr*abs(scale) + 1.0;
  }
  return length(z)/abs(dr);
}

vec3 calcNormal(vec3 p){
  vec2 e = vec2(0.0006, 0.0);
  return normalize(vec3(
    mapDE(p+e.xyy) - mapDE(p-e.xyy),
    mapDE(p+e.yxy) - mapDE(p-e.yxy),
    mapDE(p+e.yyx) - mapDE(p-e.yyx)
  ));
}

mat2 rot(float a){ float c=cos(a), s=sin(a); return mat2(c,-s,s,c); }

void main() {
  // live audio: runtime auto-drives audioBass/audioMid/audioHigh (mic FFT);
  // one master knob scales them. K caps preserved downstream.
  float bass   = audioBass * audioReact;
  float mid    = audioMid  * audioReact;
  float treble = audioHigh * audioReact;

  vec2 uv = (gl_FragCoord.xy - 0.5*RENDERSIZE) / min(RENDERSIZE.x, RENDERSIZE.y);

  // mid breathes the fold scale (small, K<=1.5)
  gScale = scaleParam * (1.0 + 0.22*mid);

  // forward velocity: constant drift, bass pushes (K<=0.8)
  float spd = camSpeed * (1.0 + bass*2.6);
  vec3 ro = vec3(0.0, 0.0, -3.0 + TIME*spd);

  vec3 rd = normalize(vec3(uv, 1.4));

  // slow yaw/pitch sway of the view (~0.05)
  float yaw   = sin(TIME*0.05) * 0.35;
  float pitch = cos(TIME*0.037) * 0.22;
  rd.xz = rot(yaw)   * rd.xz;
  rd.yz = rot(pitch) * rd.yz;

  // ---- sphere-tracing raymarch ----
  float t = 0.0;
  float dmin = 1e9;       // min trap accumulated along ray for glow
  float steps = 0.0;
  float hit = 0.0;
  vec3 pos = ro;
  for(int i=0;i<90;i++){
    pos = ro + rd*t;
    float d = mandelboxDE(pos);
    dmin = min(dmin, gTrap);
    steps += 1.0;
    if(d < 0.001){ hit = 1.0; break; }
    t += d;
    if(t > 40.0) break;
  }

  vec3 col = vec3(0.0);

  // glow term from orbit trap, colored by palette
  float trapGlow = exp(-dmin*1.6);
  float stepGlow = steps/90.0;
  float pt = paletteShift + dmin*0.35 + TIME*0.02;
  vec3 glowCol = pal(pt) * (trapGlow*0.8 + stepGlow*0.6);
  // treble boosts edge glow (additive, K<=0.6)
  glowCol *= glow * (1.0 + treble*1.8);

  if(hit > 0.5){
    vec3 n = calcNormal(pos);
    vec3 lightDir = normalize(vec3(0.5, 0.6, -0.4));
    float lam = max(dot(n, lightDir), 0.0);
    float rim = pow(1.0 - max(dot(n, -rd), 0.0), 2.5);

    vec3 base = pal(paletteShift + dmin*0.4 + 0.15);

    // user image as triplanar surface albedo (guarded; black when unuploaded)
    if(texMix > 0.0){
      vec3 an = abs(normalize(n));
      an /= (an.x + an.y + an.z + 1e-5);
      vec3 tex = texture2D(inputImage, pos.yz*0.2+0.5).rgb*an.x
               + texture2D(inputImage, pos.xz*0.2+0.5).rgb*an.y
               + texture2D(inputImage, pos.xy*0.2+0.5).rgb*an.z;
      base = mix(base, tex, texMix);   // wrap the fractal BEFORE lighting
    }

    col  = base * (0.12 + 0.7*lam);
    col += rim * pal(pt + 0.2) * (0.6 + treble*1.5);
    col += glowCol * 0.6;

    // fog into black with distance
    float fog = exp(-t*fogDensity);
    col *= fog;
  } else {
    // miss: only the volumetric glow survives, fading into deep near-black
    col = glowCol * 0.5;
    col = mix(col, bgColor.rgb, bgColor.a);  // universal background
  }

  // tonemap + gamma (house style)
  col = col/(1.0+col);
  col = pow(col, vec3(0.4545));

  // ---- universal color block (defaults = no-op) ----
  float ucL = dot(col, vec3(0.299, 0.587, 0.114));
  col = mix(vec3(ucL), col, colorBoost);
  gl_FragColor = vec4(col, 1.0);
}
