/*{
  "DESCRIPTION": "Water Drop — a single droplet bobs and merges into a calm pool, sending soft concentric ripples across a raymarched water surface that reflects and refracts the sky. Grayscale-elegant by default; tint to color it. Live audio drives it: bass punches the drop impact + ripple amplitude, mid agitates the surface, treble shimmers the specular. Optional image becomes the equirectangular sky that the water reflects/refracts.",
  "CREDIT": "ShaderClaw3 (after a Shadertoy droplet study)",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Nature",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "rippleScale",
      "LABEL": "Ripple Density",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 7,
      "DEFAULT": 3,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dropSize",
      "LABEL": "Drop Size",
      "TYPE": "float",
      "MIN": 0.5,
      "MAX": 1.8,
      "DEFAULT": 1,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "rippleAmount",
      "LABEL": "Ripple Amount",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.9,
      "DEFAULT": 0.4,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "tint",
      "LABEL": "Water Tint",
      "TYPE": "color",
      "DEFAULT": [
        0.55,
        0.78,
        1,
        1
      ],
      "GROUP": "Color"
    },
    {
      "NAME": "tintMix",
      "LABEL": "Tint Amount",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0,
      "GROUP": "Color"
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
      "MIN": 0,
      "MAX": 2,
      "DEFAULT": 1,
      "GROUP": "Audio Reactivity"
    },
    {
      "NAME": "inputImage",
      "LABEL": "Sky / Reflection",
      "TYPE": "image"
    },
    {
      "NAME": "envMix",
      "LABEL": "Reflect Image",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    },
    {
      "NAME": "quality",
      "LABEL": "Anti-Alias",
      "TYPE": "float",
      "MIN": 1,
      "MAX": 3,
      "DEFAULT": 2
    },
    {
      "NAME": "rimLight",
      "LABEL": "Specular",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.8,
      "DEFAULT": 0.3
    }
  ]
}*/

// ---- live state (set once in main from audio + knobs) -----------------------
float gT;          // animated time
float gRippleAmp;  // ripple height
float gRippleScl;  // ripple density
float gDropSize;   // drop radius scale
const vec3  gLig = vec3(0.5477, 0.7071, 0.4472);   // sqrt(.3,.5,.2), the key light

// smooth minimum (exp form, after IQ)
float smin(float d1, float d2){
  const float e = -6.0;
  return log(exp(d1*e) + exp(d2*e)) / e;
}

// ripple pool + bobbing drop, merged
float dist(vec3 p){
  float l = pow(dot(p.xz, p.xz), 0.8);
  float ripple = p.y + 0.8 + gRippleAmp * sin(l*gRippleScl - gT + 0.5) / (1.0 + l);

  float h1 = -sin(gT);
  float h2 =  cos(gT + 0.1);
  float drop = length(p + vec3(0.0, 1.2, 0.0)*h1) - 0.40*gDropSize;
  drop = smin(drop, length(p + vec3(0.1, 0.8, 0.0)*h2) - 0.20*gDropSize);

  return smin(ripple, drop);
}

// robust tetrahedron normal (IQ)
vec3 normalAt(vec3 p){
  vec2 e = vec2(1.0, -1.0) * 0.0015;
  return normalize(
    e.xyy*dist(p + e.xyy) + e.yyx*dist(p + e.yyx) +
    e.yxy*dist(p + e.yxy) + e.xxx*dist(p + e.xxx));
}

// basic sphere-tracer
vec4 march(vec3 p, vec3 d){
  vec4 m = vec4(p, 0.0);
  for(int i = 0; i < 99; i++){
    float s = dist(m.xyz);
    m += vec4(d, 1.0) * s;
    if(s < 0.01 || m.w > 20.0) break;
  }
  return m;
}

// sky / environment: grayscale gradient, optionally tinted, optionally the image
vec3 sky(vec3 d){
  float light = dot(d, gLig);
  vec3 base = vec3(max(light*0.5 + 0.5, 0.0));

  if(tintMix > 0.0){
    base = mix(base, base * tint.rgb * 1.8, tintMix);
  }
  if(envMix > 0.0){
    // equirectangular wrap so reflections pick up the whole image
    vec2 uv = vec2(atan(d.z, d.x) / 6.28318 + 0.5,
                   acos(clamp(d.y, -1.0, 1.0)) / 3.14159);
    vec3 img = texture2D(inputImage, uv).rgb;
    base = mix(base, img, envMix);
  }
  return base;
}

void main(){
  // ---- audio (runtime-provided globals; one master knob) --------------------
  float bass   = audioBass * audioReact;
  float mid    = audioMid  * audioReact;
  float treble = audioHigh * audioReact;
  // Low-floor expansion (pow < 1 lifts soft levels, ~preserves loud ones) so
  // jazz's soft swung kicks and rock's backbeat still move the pool; sub is
  // coupled in for sparse sub-heavy hits. audioBeatPulse is a host-side
  // decaying event envelope — each hit leaves a few-hundred-ms ripple trace.
  float bassE  = pow(clamp(max(bass, audioSub * audioReact), 0.0, 1.0), 0.6);
  float midE   = pow(clamp(mid, 0.0, 1.0), 0.7);
  float hit    = audioBeatPulse * audioReact;

  // bass impact swells the drop + ripple, mid quickens the surface
  gT         = TIME * speed * (1.0 + 0.25*mid + 0.25*midE);
  gRippleAmp = rippleAmount * (1.0 + 0.6*bass + 0.30*bassE + 0.35*hit);
  gRippleScl = rippleScale * (1.0 + 0.20*midE);
  gDropSize  = dropSize * (1.0 + 0.30*bass + 0.12*hit);

  vec2 res = RENDERSIZE.xy;
  vec3 col = vec3(0.0);

  vec3 pos = vec3(0.05*cos(gT), 0.1*sin(gT), -4.0);

  int aaN = int(quality + 0.5);          // 1..3, constant-bounded loop below
  float aaF = float(aaN);

  for(int yi = 0; yi < 3; yi++){
    if(yi >= aaN) break;
    for(int xi = 0; xi < 3; xi++){
      if(xi >= aaN) break;

      vec2 off = vec2(float(xi), float(yi)) / aaF;
      vec3 ray = normalize(vec3(gl_FragCoord.xy - res*0.5 + off, res.y));

      vec4 mar = march(pos, ray);
      vec3 nor = normalAt(mar.xyz);
      vec3 ref = refract(ray, nor, 0.75);

      float r = smoothstep(0.8, 1.0, dot(reflect(ray, nor), gLig));
      float fres = 1.0 - dot(ray, nor);

      // refracted look-through + specular glint (treble shimmers it)
      vec3 wat = sky(ref) + rimLight * r * fres * fres * (1.0 + 0.8*treble);
      vec3 bac = sky(ray) * 0.5 + 0.5;
      // universal background: blend the far-field sky fade toward bgColor
      bac = mix(bac, bgColor.rgb, bgColor.a);

      float fade = pow(min(mar.w / 20.0, 1.0), 0.3);
      col += mix(wat, bac, fade);
    }
  }
  col /= aaF * aaF;

  // Continuous level-follow: whole-frame luminance breathes with the mix
  // (helps beatless/sustained material read; silence = exactly 1.0).
  col *= 1.0 + 0.18 * smoothstep(0.04, 0.9, audioLevel * audioReact);

  // ---- universal color block (defaults = no-op) ----
  vec3 uc = col;
  float ucL = dot(uc, vec3(0.299, 0.587, 0.114));
  uc = mix(vec3(ucL), uc, colorBoost);                     // saturation
  if (hueShift > 0.0005) {                                  // cheap hue rotate (YIQ)
    float hA = hueShift * 6.2831853;
    float hC = cos(hA), hS = sin(hA);
    mat3 hM = mat3(0.299,0.587,0.114, 0.299,0.587,0.114, 0.299,0.587,0.114)
            + hC * mat3(0.701,-0.587,-0.114, -0.299,0.413,-0.114, -0.300,-0.588,0.886)
            + hS * mat3(0.168,0.330,-0.497, -0.328,0.035,0.292, 1.250,-1.050,-0.203);
    uc = clamp(hM * uc, 0.0, 1.0);
  }
  col = uc;

  gl_FragColor = vec4(col * col, 1.0);   // mild gamma, faithful to the original
}
