/*{
  "DESCRIPTION": "Quaternion Bloom — a raymarched 4D Quaternion-Julia fractal that slowly blooms like an alien flower. The Julia seed morphs over time, treble ignites edge bloom, mid breathes the seed, bass pulses its magnitude. Calm orbiting camera, curated cosine palette, deep near-black.",
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
      "DEFAULT": 0.6,
      "MIN": 0,
      "MAX": 2,
      "LABEL": "Glow"
    },
    {
      "NAME": "fogDensity",
      "TYPE": "float",
      "DEFAULT": 0.18,
      "MIN": 0,
      "MAX": 1,
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
      "NAME": "iters",
      "TYPE": "float",
      "DEFAULT": 9,
      "MIN": 5,
      "MAX": 11,
      "GROUP": "Shape / Geometry",
      "LABEL": "Iterations"
    },
    {
      "NAME": "orbitSpeed",
      "LABEL": "Orbit Speed",
      "TYPE": "float",
      "DEFAULT": 0.07,
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
      "GROUP": "Color",
      "LABEL": "Palette Shift"
    },
    {
      "NAME": "hueShift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "LABEL": "Hue Shift",
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

// ---- house-style cosine palette ----
vec3 pal(float t){ return 0.5 + 0.5*cos(6.28318*(t + vec3(0.0,0.33,0.67))); }

// ---- quaternion multiply ----
vec4 qmul(vec4 a, vec4 b){
  return vec4(
    a.x*b.x - a.y*b.y - a.z*b.z - a.w*b.w,
    a.x*b.y + a.y*b.x + a.z*b.w - a.w*b.z,
    a.x*b.z - a.y*b.w + a.z*b.x + a.w*b.y,
    a.x*b.w + a.y*b.z - a.z*b.y + a.w*b.x);
}

// orbit-trap accumulator (set inside DE)
float gTrap = 1e9;

// ---- quaternion-Julia distance estimator with orbit trap ----
float quatJuliaDE(vec3 pos, vec4 c){
  vec4 z = vec4(pos, 0.0);
  float dz2 = 1.0;
  float r2 = dot(z,z);
  float trap = 1e9;
  for(int i=0;i<11;i++){
    if(float(i) >= iters) break;
    dz2 *= 4.0*r2;
    z = qmul(z,z) + c;
    r2 = dot(z,z);
    trap = min(trap, r2);
    if(r2 > 16.0) break;
  }
  gTrap = trap;
  float r = sqrt(r2);
  return 0.5 * r * log(max(r,1e-5)) / sqrt(max(dz2,1e-10));
}

// blooming seed
vec4 juliaSeed(float bass, float mid){
  vec4 juliaC = vec4(-0.45, 0.2, 0.0, 0.0);
  // bass gently pulses the seed magnitude (positional, K<=0.6)
  float mag = 1.0 + bass*0.6*0.4;
  vec4 c = juliaC*mag
         + 0.12*vec4(sin(TIME*0.1), cos(TIME*0.13), sin(TIME*0.07), 0.0)
         + mid*0.05;
  return c;
}

// DE-gradient normal
vec3 calcNormal(vec3 p, vec4 c){
  vec2 e = vec2(0.0012, 0.0);
  return normalize(vec3(
    quatJuliaDE(p+e.xyy,c) - quatJuliaDE(p-e.xyy,c),
    quatJuliaDE(p+e.yxy,c) - quatJuliaDE(p-e.yxy,c),
    quatJuliaDE(p+e.yyx,c) - quatJuliaDE(p-e.yyx,c)));
}

void main() {
  // live audio FFT globals (auto-declared by runtime), scaled by user reactivity
  float bass   = audioBass * audioReact;
  float mid    = audioMid  * audioReact;
  float treble = audioHigh * audioReact;

  vec2 uv = (gl_FragCoord.xy - 0.5*RENDERSIZE) / min(RENDERSIZE.x, RENDERSIZE.y);

  vec4 c = juliaSeed(bass, mid);

  // ---- orbiting camera ----
  float ang = TIME * orbitSpeed * (1.0 + mid*0.5);
  float rad = 2.6;
  vec3 ro = vec3(cos(ang)*rad, 0.55*sin(TIME*orbitSpeed*0.6), sin(ang)*rad);
  vec3 ta = vec3(0.0);
  vec3 fwd = normalize(ta - ro);
  vec3 rgt = normalize(cross(vec3(0.0,1.0,0.0), fwd));
  vec3 upv = cross(fwd, rgt);
  vec3 rd = normalize(uv.x*rgt + uv.y*upv + 1.5*fwd);

  // ---- sphere-trace raymarch ----
  float t = 0.0;
  float hit = -1.0;
  float edgeTrap = 1e9;
  for(int i=0;i<120;i++){
    vec3 p = ro + rd*t;
    float d = quatJuliaDE(p, c);
    if(d < 0.0008){ hit = t; edgeTrap = gTrap; break; }
    t += d * 0.9;
    if(t > 12.0) break;
  }

  vec3 col = vec3(0.0);

  // background: faint vertical bloom gradient so it's alive at audio=0
  vec3 bg = pal(paletteShift + 0.6 + uv.y*0.15) * (0.015 + 0.01*length(uv));
  col = bg;

  if(hit > 0.0){
    vec3 p = ro + rd*hit;
    vec3 n = calcNormal(p, c);

    // lighting
    vec3 lig = normalize(vec3(0.6, 0.8, 0.4));
    float lam = max(dot(n, lig), 0.0);
    float rim = pow(1.0 - max(dot(n, -rd), 0.0), 3.0);

    // orbit-trap glow colored by palette
    float tt = paletteShift + sqrt(edgeTrap)*0.9 + 0.1*TIME*0.05;
    vec3 trapCol = pal(tt);

    // ---- triplanar user image albedo (wraps the fractal, lit below) ----
    // unuploaded image reads as solid black, so guard on texMix
    if(texMix > 0.0){
      vec3 an = abs(normalize(n));
      an = an / max(an.x + an.y + an.z, 1e-4);
      vec3 tex = texture2D(inputImage, p.yz*0.3 + 0.5).rgb * an.x
               + texture2D(inputImage, p.xz*0.3 + 0.5).rgb * an.y
               + texture2D(inputImage, p.xy*0.3 + 0.5).rgb * an.z;
      // mix image into surface albedo BEFORE lighting so it wraps + is lit
      trapCol = mix(trapCol, tex, texMix);
    }

    vec3 base = trapCol * (0.25 + lam*0.85);
    base += trapCol * rim * (0.6 + glow*0.5);

    // treble ignites bloom on silhouette/edges (K<=0.6)
    float ignite = treble*0.6;
    float edge = pow(rim, 1.5);
    base += trapCol * edge * glow * (0.4 + ignite);
    base += pal(tt + 0.2) * edge * ignite * 0.8;

    // soft interior glow from trap
    base += trapCol * (1.0 - smoothstep(0.0, 0.6, edgeTrap)) * glow * 0.3;

    // distance fog to black
    float fog = exp(-hit * fogDensity * 0.55);
    col = base * fog;
  }

  // universal background override — miss region only (a=0 -> untouched)
  if (hit <= 0.0) col = mix(col, bgColor.rgb, bgColor.a);

  // gentle global bloom lift with treble
  col += bg * treble * 0.5;

  // ---- tonemap + gamma ----
  col = col / (1.0 + col);
  col = pow(col, vec3(0.4545));

  // ---- universal color block (defaults = no-op) ----
  float ucL = dot(col, vec3(0.299, 0.587, 0.114));
  vec3 uc = mix(vec3(ucL), col, colorBoost);
  if (hueShift > 0.0005) {
    float hueA = hueShift * 6.2831853;
    float hueC = cos(hueA), hueS = sin(hueA);
    mat3 hueM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
              + hueC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
              + hueS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
    uc = clamp(hueM * uc, 0.0, 1.0);
  }
  gl_FragColor = vec4(uc, 1.0);
}
