// Name: NY Scan Roll
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// An old station CRT losing hold: the picture rolls, splits into red / green / blue, tears
// on the scanlines and snows, then locks back onto B.
vec4 transition(vec2 uv) {
  float bell = sin(progress * 3.14159);
  float roll = progress * 2.0;
  vec2 u = vec2(uv.x + 0.02 * bell * sin(uv.y * 80.0 + progress * 40.0), fract(uv.y + roll * bell));
  float split = 0.012 * bell * (1.0 + audioBeat);
  vec3 A = vec3(getFromColor(u + vec2(split, 0.0)).r, getFromColor(u).g, getFromColor(u - vec2(split, 0.0)).b);
  vec3 B = vec3(getToColor(u + vec2(split, 0.0)).r, getToColor(u).g, getToColor(u - vec2(split, 0.0)).b);
  vec3 col = progress < 0.5 ? A : B;
  float snow = fract(sin(dot(floor(uv * vec2(320.0, 240.0)) + floor(progress * 60.0), vec2(12.9898, 78.233))) * 43758.5453);
  col = mix(col, vec3(snow), bell * bell * 0.35);
  col *= 0.85 + 0.15 * sin(uv.y * 900.0);
  col += vec3(1.0) * exp(-pow((fract(uv.y - progress * 3.0) - 0.5) / 0.01, 2.0)) * bell * 0.3;
  return vec4(col, 1.0);
}
