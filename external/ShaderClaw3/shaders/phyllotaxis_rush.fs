/*{
  "DESCRIPTION": "Phyllotaxis Rush — a 3D sunflower-seed spiral of glowing dots streaming toward the viewer along the golden angle. Calm starfield flight, live mic/audio-reactive rush + sparkle. Optionally reconstructs your image out of the flying dots.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": [
    "Generator",
    "3D",
    "Particles",
    "Audio Reactive"
  ],
  "INPUTS": [
    {
      "NAME": "brightness",
      "TYPE": "float",
      "DEFAULT": 1,
      "MIN": 0,
      "MAX": 3,
      "LABEL": "Brightness"
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
      "NAME": "spread",
      "TYPE": "float",
      "DEFAULT": 2.2,
      "MIN": 0.5,
      "MAX": 5,
      "LABEL": "Spread",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "dotSize",
      "TYPE": "float",
      "DEFAULT": 0.012,
      "MIN": 0.002,
      "MAX": 0.05,
      "LABEL": "Dot Size",
      "GROUP": "Shape / Geometry"
    },
    {
      "NAME": "rushSpeed",
      "LABEL": "Rush Speed",
      "TYPE": "float",
      "DEFAULT": 0.16,
      "MIN": 0,
      "MAX": 1,
      "GROUP": "Motion / Animation"
    },
    {
      "NAME": "rotSpeed",
      "TYPE": "float",
      "DEFAULT": 0.06,
      "MIN": 0,
      "MAX": 0.5,
      "LABEL": "Rotation Speed",
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
      "NAME": "nearZ",
      "TYPE": "float",
      "DEFAULT": 0.25,
      "MIN": 0.05,
      "MAX": 1,
      "LABEL": "Near Z",
      "GROUP": "Camera / Layout"
    },
    {
      "NAME": "farZ",
      "TYPE": "float",
      "DEFAULT": 4,
      "MIN": 1,
      "MAX": 10,
      "LABEL": "Far Z",
      "GROUP": "Camera / Layout"
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

vec3 pal(float t){return 0.5+0.5*cos(6.28318*(t+vec3(0.0,0.33,0.67)));}

const int N = 180;

void main() {
  // live audio globals (audioBass/audioMid/audioHigh) are auto-declared by the runtime
  float bass   = max(audioBass, audioSub) * audioReact; // sub-coupled: hiphop kicks live below audioBass
  float mid    = audioMid  * audioReact;
  float treble = audioHigh * audioReact;
  float hit    = audioBeatPulse * audioReact;           // host-side decaying hit trace (300-600ms)
  float bassK  = smoothstep(0.03, 0.85, bass);          // low sensitivity floor for sparse/soft kicks

  vec2 uv = (gl_FragCoord.xy - 0.5*RENDERSIZE) / min(RENDERSIZE.x, RENDERSIZE.y);

  vec3 col = vec3(0.0);

  // global slow rotation of the whole spiral (mid loosens the drift a touch)
  float baseRot = TIME * rotSpeed * (1.0 + mid*0.4);

  float Nf = float(N);

  for (int i = 0; i < 180; i++) {
    float fi = float(i);

    float ang = fi * 2.39996323 + baseRot;          // golden angle + drift
    float rad = sqrt(fi / Nf) * spread;             // phyllotaxis radius (XY plane)

    // travel toward camera; bass speeds the rush (K = 0.6)
    float z = fract(fi / Nf - TIME * rushSpeed * (1.0 + bass*0.6)); // 0 far .. 1 near
    float depth = mix(farZ, nearZ, z);              // near = small depth

    vec2 proj = vec2(cos(ang), sin(ang)) * rad / depth; // perspective: closer = spread out
    // each hit briefly swells every dot (decaying trace), bass follows under it
    float size = (dotSize * (1.0 + 0.30*hit + 0.20*bassK)) / depth; // closer = bigger

    float d = length(uv - proj);
    float px = 1.5 / RENDERSIZE.y;                  // ~1.5px AA in UV units
    float disc = smoothstep(size + px, size - px, d);            // pixel-sharp silhouette
    float glow = disc * (0.55 + 0.45 * smoothstep(size, 0.0, d)); // shaded interior, crisp rim
    glow += 0.4 * smoothstep(size*3.0, 0.0, d);     // soft halo (additive accent)

    // fade in while far, fade out as it sweeps past the camera
    float fade = smoothstep(0.0, 0.15, z) * (1.0 - smoothstep(0.85, 1.0, z));

    // treble sparkles the nearest dots (positional pulse, K <= 0.6)
    float sparkle = 1.0 + treble * 0.6 * smoothstep(0.6, 1.0, z)
                        * (0.5 + 0.5*sin(fi*12.9898 + TIME*9.0));

    vec3 tint = pal(fi*0.012 + paletteShift + TIME*0.02);

    // reconstruct the user's image from the flat phyllotaxis seed layout:
    // sample inputImage at each dot's base position, tint that dot by it.
    // (unuploaded image = black, so guard on texMix; default texMix 0)
    if (texMix > 0.0) {
      vec2 seed = vec2(cos(ang), sin(ang)) * sqrt(fi / Nf) * 0.5 + 0.5;
      vec3 img = texture2D(inputImage, seed).rgb;
      tint = mix(tint, img, texMix);
    }

    col += tint * glow * fade * sparkle * brightness;
  }

  // brightness rides each hit's decaying trace + smooth bass under it,
  // so sparse kicks stay visible between hits; silence = exactly 1.0
  col *= 1.0 + 0.35*hit + 0.25*bassK;

  // tonemap + gamma
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
  uc = mix(uc, bgColor.rgb, bgColor.a * (1.0 - smoothstep(0.0, 0.35, ucL)));

  gl_FragColor = vec4(uc, 1.0);
}
