// Name: NY Spray Over
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// A writer buffs the wall: a fat-cap spray front sweeps across A laying down paint with soft
// overspray and flying specks, then a second pass sprays B in through the fresh paint, its
// edge a ragged spray stencil. Highs add specks, beats puff the cloud.
uniform vec4 paint; // = vec4(0.92, 0.93, 0.95, 1.0)

float spHash(vec2 p) { return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453); }
float spNoise(vec2 x) { vec2 i = floor(x), f = fract(x); f = f * f * (3.0 - 2.0 * f);
  return mix(mix(spHash(i), spHash(i + vec2(1, 0)), f.x), mix(spHash(i + vec2(0, 1)), spHash(i + vec2(1, 1)), f.x), f.y); }
float spFbm(vec2 p) { return spNoise(p) * 0.55 + spNoise(p * 2.1 + 3.1) * 0.3 + spNoise(p * 4.7 + 7.0) * 0.15; }

vec4 transition(vec2 uv) {
  vec2 p = uv * vec2(ratio, 1.0);
  float wob = (spFbm(p * 3.0) - 0.5) * 0.35 + 0.08 * sin(uv.y * 9.0);
  // pass 1: paint covers A; pass 2: B sprayed in through the paint
  float f1 = smoothstep(0.0, 0.55, progress) * 1.5 - 0.25;
  float f2 = smoothstep(0.4, 1.0, progress) * 1.5 - 0.25;
  float x = uv.x + wob;
  float paintM = smoothstep(f1 + 0.08, f1 - 0.04, x);
  float bM = smoothstep(f2 + 0.06, f2 - 0.06, x + (spFbm(p * 9.0) - 0.5) * 0.12);
  // overspray: a soft cloud ahead of each front, and flying specks
  float cloud1 = exp(-pow((x - f1) / 0.07, 2.0)) * 0.6 * (1.0 + audioBeat);
  float specks = step(0.985 - 0.02 * audioHigh, spHash(floor(uv * vec2(ratio, 1.0) * 400.0) + floor(progress * 30.0))) * exp(-pow((x - f1) / 0.15, 2.0));
  vec3 A = getFromColor(uv).rgb, B = getToColor(uv).rgb;
  vec3 col = mix(A, paint.rgb, clamp(paintM + cloud1 * 0.6 + specks, 0.0, 1.0));
  col = mix(col, B, bM);
  // a wet sheen on fresh paint
  col += vec3(0.08) * paintM * (1.0 - bM) * smoothstep(0.4, 0.9, spFbm(p * 6.0 + progress));
  return vec4(col, 1.0);
}
