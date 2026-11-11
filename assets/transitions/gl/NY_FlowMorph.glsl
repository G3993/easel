// Name: NY Flow Morph
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// A morph rather than a cross-fade. Each picture is read at three softened scales, broad
// shapes first and finer forms after, and pushed along the difference between them: A's
// pixels flow toward where their brightness sits in B, and B flows back from where it was in
// A. The hand-off happens in the middle under a soft bloom, so forms melt and re-form.
uniform float strength; // = 1.0

float fmL(vec3 c) { return dot(c, vec3(0.299, 0.587, 0.114)); }
float fmA(vec2 uv, float r) { return (fmL(getFromColor(uv + vec2(r, r)).rgb) + fmL(getFromColor(uv + vec2(-r, r)).rgb) + fmL(getFromColor(uv + vec2(r, -r)).rgb) + fmL(getFromColor(uv - vec2(r, r)).rgb)) * 0.25; }
float fmB(vec2 uv, float r) { return (fmL(getToColor(uv + vec2(r, r)).rgb) + fmL(getToColor(uv + vec2(-r, r)).rgb) + fmL(getToColor(uv + vec2(r, -r)).rgb) + fmL(getToColor(uv - vec2(r, r)).rgb)) * 0.25; }

vec4 transition(vec2 uv) {
  float t = progress;
  float bell = sin(t * 3.14159);
  vec2 flowV = vec2(0.0);
  float w = 1.0;
  for (int i = 0; i < 3; i++) {
    float s = 0.05 / pow(2.0, float(i));                       // 0.05, 0.025, 0.0125 — broad first
    float diff = fmB(uv, s) - fmA(uv, s);
    vec2 g = vec2(fmB(uv + vec2(s, 0.0), s) - fmB(uv - vec2(s, 0.0), s) + fmA(uv + vec2(s, 0.0), s) - fmA(uv - vec2(s, 0.0), s),
                  fmB(uv + vec2(0.0, s), s) - fmB(uv - vec2(0.0, s), s) + fmA(uv + vec2(0.0, s), s) - fmA(uv - vec2(0.0, s), s));
    flowV += g * diff * w * s * 3.0;
    w *= 0.55;
  }
  flowV *= strength * (1.0 + 0.6 * audioBass);
  float lf = length(flowV);
  flowV *= min(1.0, 0.07 / max(lf, 1e-5));                      // never tear the picture apart
  vec3 A = getFromColor(uv + flowV * t * 4.0).rgb;
  vec3 B = getToColor(uv - flowV * (1.0 - t) * 4.0).rgb;
  float m = smoothstep(0.32, 0.68, t);
  vec3 col = mix(A, B, m);
  vec3 soft = (getFromColor(uv + vec2(0.012, 0.0)).rgb + getToColor(uv - vec2(0.012, 0.0)).rgb + getFromColor(uv + vec2(0.0, 0.012)).rgb + getToColor(uv - vec2(0.0, 0.012)).rgb) * 0.25;
  col += soft * soft * bell * 0.3;
  return vec4(col, 1.0);
}
