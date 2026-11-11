// Name: NY Zoom Punch
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// A drop transition: A rushes into the centre with a radial blur, a white flash on the hit,
// and B punches out from the centre, overshooting slightly before it settles. Bass deepens
// the punch.
uniform float amount; // = 0.6

vec4 transition(vec2 uv) {
  vec2 c = vec2(0.5);
  float t = progress;
  float inA = smoothstep(0.0, 0.5, t), outB = smoothstep(0.5, 1.0, t);
  float zA = 1.0 + inA * inA * 3.0 * amount;
  float over = 1.0 - (1.0 - outB) * (1.0 - outB);
  float zB = mix(0.25, 1.0, over) + 0.06 * sin(outB * 9.0) * (1.0 - outB);
  float punch = 1.0 + audioBass * 0.3;
  vec3 col = vec3(0.0);
  float blurA = inA * 0.08 * punch, blurB = (1.0 - outB) * 0.08 * punch;
  for (int i = 0; i < 8; i++) {
    float k = float(i) / 7.0;
    if (t < 0.5) col += getFromColor(c + (uv - c) / (zA * (1.0 + blurA * k))).rgb;
    else col += getToColor(c + (uv - c) / (zB * (1.0 - blurB * k)) ).rgb;
  }
  col /= 8.0;
  col += vec3(1.0) * exp(-pow((t - 0.5) / 0.06, 2.0)) * 0.9;
  return vec4(col, 1.0);
}
