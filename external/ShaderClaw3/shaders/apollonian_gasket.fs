/*{
  "DESCRIPTION": "Apollonian Gasket — 2D circle-packing fractal via the classic apollonian fold. Thin neon circle rims on deep black, cosine-palette colored, live audio breathes the fold radius / glows the rims / drifts the palette. Optional user image rides the circle packing.",
  "CREDIT": "ShaderClaw3",
  "CATEGORIES": ["Generator", "Fractal", "Audio Reactive"],
  "INPUTS": [
    { "NAME": "sBase",       "LABEL": "Fold Radius",      "TYPE": "float", "MIN": 0.8,  "MAX": 1.4,  "DEFAULT": 1.05 },
    { "NAME": "zoom",        "LABEL": "Zoom",             "TYPE": "float", "MIN": 0.4,  "MAX": 2.5,  "DEFAULT": 1.0 },
    { "NAME": "rimGlow",     "LABEL": "Rim Glow",         "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 0.8 },
    { "NAME": "paletteShift","LABEL": "Palette Shift",    "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0 },
    { "NAME": "rotSpeed",    "LABEL": "Spin Speed",       "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.06 },
    { "NAME": "audioReact",  "LABEL": "Sound Reactivity", "TYPE": "float", "MIN": 0.0,  "MAX": 2.0,  "DEFAULT": 1.0 },
    { "NAME": "inputImage",  "LABEL": "Your Image",       "TYPE": "image" },
    { "NAME": "texMix",      "LABEL": "Image Amount",     "TYPE": "float", "MIN": 0.0,  "MAX": 1.0,  "DEFAULT": 0.0 }
  ]
}*/

// curated cosine palette (neon, not full rainbow noise)
vec3 pal(float t){
  return 0.5 + 0.5*cos(6.28318*(t + vec3(0.0, 0.33, 0.67)));
}

// classic Apollonian fold: returns a thin distance-ish value to the nearest
// circle boundary, plus the accumulated inversion scale (for coloring) and the
// folded local coordinate (for texturing the packing).
float apollo(vec2 p, float s, out float oScale, out vec2 oFold){
  float scale = 1.0;
  for(int i = 0; i < 8; i++){
    p = -1.0 + 2.0*fract(0.5*p + 0.5);   // wrap into [-1,1]
    float r2 = dot(p, p);
    float k = s / max(r2, 1e-4);          // inversion (guard against /0)
    p *= k;
    scale *= k;
  }
  oScale = scale;
  oFold  = p;                              // local folded coord (pre final-scale)
  return abs(p.x) / scale;                 // distance-ish to nearest circle
}

void main() {
  // live audio globals are auto-provided by the runtime (mic/FFT); one master knob.
  float bass   = audioBass * audioReact;
  float mid    = audioMid  * audioReact;
  float treble = audioHigh * audioReact;

  vec2 uv = (gl_FragCoord.xy - 0.5*RENDERSIZE) / min(RENDERSIZE.x, RENDERSIZE.y);

  // calm zoom + gentle breathing pan so it stays alive at audio=0
  uv /= max(zoom, 0.01);
  uv += 0.20*vec2(sin(TIME*0.13), cos(TIME*0.11));   // slow drift

  // slow rotation, faintly faster with mid
  float ang = TIME * rotSpeed * (1.0 + 0.6*mid);
  float ca = cos(ang), sa = sin(ang);
  uv = mat2(ca, -sa, sa, ca) * uv;

  // fold radius breathes with bass (K well under 1.5)
  float s = sBase + 0.12*bass;

  float scale;
  vec2 fold;
  float d = apollo(uv, s, scale, fold);

  // thin neon rim: bright right at the circle boundary, deep black interior
  float rim = exp(-180.0*d);

  // palette indexed by accumulated scale + slow TIME rotation + a little mid
  float t = log(abs(scale) + 1.0)*0.18
          + TIME*0.03
          + paletteShift
          + 0.08*mid;
  vec3 base = pal(t);

  // optional: texture the circle rims with the user's image, sampled along the
  // folded gasket coordinate so the image colors ride the circle packing.
  // Unuploaded image samples as black, so guard on texMix (default 0 = unchanged).
  if (texMix > 0.0) {
    vec2 iuv = fract(fold*0.5 + 0.5);                // folded coord -> 0..1
    vec3 img = texture2D(inputImage, iuv).rgb;
    base = mix(base, base*img + img*0.5, texMix);    // image rides the rim color
  }

  // additive glow on the rims; treble brightens (positional/intensity K<=0.6)
  float glow = rimGlow * (1.0 + 0.6*treble);
  vec3 col = base * rim * glow;

  // a secondary soft halo so rims read as glowing rings, not hairlines
  col += base * 0.35 * exp(-26.0*d) * (0.6 + 0.4*treble);

  // deep near-black background floor
  col += vec3(0.015, 0.02, 0.035) * (0.5 + 0.5*rim);

  // tonemap so it never blows out
  col = col / (1.0 + col);
  col = pow(col, vec3(0.4545));

  gl_FragColor = vec4(col, 1.0);
}
