// Name: NY Beat Slices
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// Horizontal bands slide across in alternating directions, each band moving in clean jumps
// (steps = beats in the transition), so the cut lands with the music. B rides in behind A.
uniform float bands; // = 8.0
uniform float steps; // = 8.0

vec4 transition(vec2 uv) {
  float n = max(bands, 1.0);
  float row = floor(uv.y * n);
  float dir = mod(row, 2.0) < 0.5 ? 1.0 : -1.0;
  float st = max(steps, 1.0);
  float q = (floor(progress * st) + smoothstep(0.0, 0.3, fract(progress * st))) / st;   // jump on each beat
  float stagger = (row / n) * 0.3;
  float t = clamp((q - stagger) / 0.7, 0.0, 1.0);
  float off = t * dir;
  vec2 ua = vec2(uv.x - off, uv.y);
  vec2 ub = vec2(uv.x - off + dir, uv.y);
  vec3 col;
  if (ua.x >= 0.0 && ua.x <= 1.0) col = getFromColor(ua).rgb;
  else col = getToColor(clamp(ub, 0.0, 1.0)).rgb;
  // a bright seam where the two pictures meet, a dark hairline between bands
  float seamX = dir > 0.0 ? off : 1.0 + off;
  col += vec3(1.0) * exp(-pow((uv.x - seamX) * 120.0, 2.0)) * step(0.01, t) * step(t, 0.99) * (0.4 + 0.6 * audioBeat);
  col *= 1.0 - 0.6 * step(fract(uv.y * n), 0.008);
  return vec4(col, 1.0);
}
