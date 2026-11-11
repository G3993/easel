// Name: NY Luma Morph
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// A morph led by the pictures themselves: B's brightest forms surface first and spread into
// its shadows while A's dark areas sink away. Brightness is read from a softened copy of each
// picture, so whole shapes lead (not pixel noise), and both images drift along their own broad
// brightness slopes so forms melt into each other. A glow rides the moving boundary.
uniform float flow; // = 0.035
uniform float softness; // = 0.24

float lmL(vec3 c) { return dot(c, vec3(0.299, 0.587, 0.114)); }
float lmSoftA(vec2 uv, float r) {
  return (lmL(getFromColor(uv).rgb) * 2.0 + lmL(getFromColor(uv + vec2(r, 0.0)).rgb) + lmL(getFromColor(uv - vec2(r, 0.0)).rgb)
        + lmL(getFromColor(uv + vec2(0.0, r)).rgb) + lmL(getFromColor(uv - vec2(0.0, r)).rgb)) / 6.0;
}
float lmSoftB(vec2 uv, float r) {
  return (lmL(getToColor(uv).rgb) * 2.0 + lmL(getToColor(uv + vec2(r, 0.0)).rgb) + lmL(getToColor(uv - vec2(r, 0.0)).rgb)
        + lmL(getToColor(uv + vec2(0.0, r)).rgb) + lmL(getToColor(uv - vec2(0.0, r)).rgb)) / 6.0;
}

vec4 transition(vec2 uv) {
  float r = 0.03, e = 0.035;
  float bell = sin(progress * 3.14159);
  vec2 gA = vec2(lmSoftA(uv + vec2(e, 0.0), r) - lmSoftA(uv - vec2(e, 0.0), r), lmSoftA(uv + vec2(0.0, e), r) - lmSoftA(uv - vec2(0.0, e), r));
  vec2 gB = vec2(lmSoftB(uv + vec2(e, 0.0), r) - lmSoftB(uv - vec2(e, 0.0), r), lmSoftB(uv + vec2(0.0, e), r) - lmSoftB(uv - vec2(0.0, e), r));
  float fl = flow * (1.0 + 0.8 * audioBass) * bell;
  vec3 A = getFromColor(uv - gB * fl * 6.0).rgb;
  vec3 B = getToColor(uv + gA * fl * 6.0).rgb;
  float key = mix(lmSoftB(uv, r * 1.5), 1.0 - lmSoftA(uv, r * 1.5), 0.3);
  float t = progress * (1.0 + 2.0 * softness) - softness;
  float m = smoothstep(1.0 - t - softness, 1.0 - t + softness, key);
  float edge = exp(-pow((key - (1.0 - t)) / max(softness * 0.3, 0.01), 2.0)) * bell;
  vec3 col = mix(A, B, m) + mix(A, B, 0.5) * edge * 0.45;
  return vec4(col, 1.0);
}
