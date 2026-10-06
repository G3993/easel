/*{
  "DESCRIPTION": "Turbulence Flight — a glowing volumetric ride through a rotating turbulent field (after a Xor commented raymarcher). 100 raymarch steps accumulate neon glow off a sine-warped plane; nested frequencies fold the space and a slow rotation tumbles it. Bass swells the warp + glow, mid spins it faster, treble shimmers the detail. Optional image tints the glow in screen space.",
  "CREDIT": "ShaderClaw3 (after Xor, shadertoy tXlXDX)",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Abstract",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "detail",
      "LABEL": "Detail",
      "TYPE": "float",
      "MIN": 6,
      "MAX": 28,
      "DEFAULT": 16,
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "speed",
      "LABEL": "Speed",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 3,
      "DEFAULT": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "glow",
      "LABEL": "Glow",
      "TYPE": "float",
      "MIN": 0.3,
      "MAX": 2.5,
      "DEFAULT": 1,
      "GROUP": "Color"
    },
    {
      "NAME": "colorShift",
      "LABEL": "Color Shift",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 6.2832,
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
      "NAME": "warp",
      "LABEL": "Warp",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 0.03,
      "DEFAULT": 0.01
    },
    {
      "NAME": "inputImage",
      "LABEL": "Tint Image",
      "TYPE": "image"
    },
    {
      "NAME": "texMix",
      "LABEL": "Image Tint",
      "TYPE": "float",
      "MIN": 0,
      "MAX": 1,
      "DEFAULT": 0
    }
  ]
}*/

// tanh isn't in GLSL ES 1.0 — polyfill (args here are >= 0, so this is stable)
vec4 tanh4(vec4 x){
  vec4 e = exp(-2.0 * x);
  return (1.0 - e) / (1.0 + e);
}

void main(){
  vec2 u = gl_FragCoord.xy;
  vec4 o = vec4(0.0);

  // live audio (runtime globals) under one master knob
  float bass   = audioBass * audioReact;
  float mid    = audioMid  * audioReact;
  float treble = audioHigh * audioReact;

  float t       = TIME * speed * (1.0 + 0.4*mid);     // rotation animation
  float freq    = detail * (1.0 + 0.2*treble);        // fold frequency
  float warpAmt = warp   * (1.0 + 0.8*bass);          // sine warp strength
  float expo    = 6e3 / (glow * (1.0 + 0.5*treble));  // tone divisor (more glow -> smaller)

  // ray direction: vec3(2u, 0) - (resx, resy, resx), normalized. camera at origin.
  vec3 res3 = vec3(RENDERSIZE.xy, RENDERSIZE.x);
  vec3 dir  = normalize(vec3(u + u, 0.0) - res3);

  float d = 0.0;
  float s;

  for(int i = 0; i < 100; i++){
    vec3 p = d * dir;

    // nested turbulence: 4 doubling octaves (s = .1 .2 .4 .8), each warps + rotates
    s = 0.1;
    for(int k = 0; k < 4; k++){
      p -= dot(sin(p * s * freq), vec3(warpAmt)) / s;
      vec4 c = cos(0.3 * t + vec4(0.0, 33.0, 11.0, 0.0));
      p.xz *= mat2(c.x, c.y, c.z, c.w);
      s += s;
    }

    // distance to the warped plane (y = 0), step forward
    d += s = 0.01 + abs(p.y);

    // accumulate neon glow, palette cycles by depth
    o += (1.0 + cos(d + colorShift + vec4(4.0, 2.0, 1.0, 0.0))) / s;
  }

  o = tanh4(o / expo);

  // optional: tint the glow with the user's image (screen-space). black if none, so guard.
  if(texMix > 0.0){
    vec2 iuv = gl_FragCoord.xy / RENDERSIZE.xy;
    vec3 img = texture2D(inputImage, iuv).rgb;
    o.rgb = mix(o.rgb, o.rgb * img * 2.0, texMix);
  }

  // ---- universal color block (defaults = no-op) ----
  vec3 uc = o.rgb;
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
  // background: tint the darkest end (the void between glow) toward bgColor
  uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));
  o.rgb = uc;

  gl_FragColor = vec4(o.rgb, 1.0);
}
