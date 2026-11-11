// Name: NY Split-Flap
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// A station departures board. The picture is cut into flaps; row by row from the top, each
// flap's upper half falls over its hinge (showing B on its back) with a little bounce, the way
// a Solari board clacks over. steps quantises the rows onto the beat.
uniform float rows; // = 9.0
uniform float cols; // = 14.0
uniform float steps; // = 0.0

float sfBounce(float x) { return 1.0 - abs(cos(x * 3.14159 * 1.5)) * (1.0 - x) * (1.0 - x) * 0.9 - (1.0 - x) * 0.1 * (1.0 - x); }

vec4 transition(vec2 uv) {
  float R = max(rows, 1.0), C = max(cols, 1.0);
  vec2 cell = vec2(floor(uv.x * C), floor((1.0 - uv.y) * R));          // row 0 = top
  vec2 lc = vec2(fract(uv.x * C), 1.0 - fract((1.0 - uv.y) * R));       // 0..1 in the flap, y up
  float prog = progress;
  if (steps > 0.5) prog = (floor(progress * steps) + smoothstep(0.0, 0.25, fract(progress * steps))) / steps;
  float st = (cell.y / R) * 0.7 + (cell.x / C) * 0.08;
  float f = clamp((prog - st) / 0.22, 0.0, 1.0);
  vec4 A = getFromColor(uv), B = getToColor(uv);
  vec3 gap = vec3(0.02);
  float seam = smoothstep(0.0, 0.02, abs(lc.y - 0.5)) * smoothstep(0.0, 0.03, lc.x) * smoothstep(1.0, 0.97, lc.x);
  if (f <= 0.0) return vec4(A.rgb * mix(0.7, 1.0, seam), 1.0);
  if (f >= 1.0) return vec4(B.rgb * mix(0.7, 1.0, seam), 1.0);
  // static halves: top shows B (revealed), bottom shows A until the flap lands
  float ang = sfBounce(f) * 3.14159;                                  // 0 → π: the upper flap falls
  float c = cos(ang);
  vec3 col;
  if (lc.y >= 0.5) {
    col = B.rgb * 0.85;                                                // revealed top behind the flap
    if (c > 0.0) {                                                     // flap still above the hinge
      float h = 0.5 + (lc.y - 0.5) / max(c, 0.001);
      if (h <= 1.0) {
        vec2 suv = vec2(uv.x, (cell.y < 0.0 ? 0.0 : (R - cell.y - 1.0 + h) / R));
        col = getFromColor(suv).rgb * (0.6 + 0.4 * c);
      }
    }
  } else {
    col = A.rgb;
    if (c < 0.0) {                                                     // flap below the hinge: shows B's lower half
      float h = 0.5 - (0.5 - lc.y) / max(-c, 0.001);
      if (h >= 0.0) {
        vec2 suv = vec2(uv.x, (R - cell.y - 1.0 + h) / R);
        col = getToColor(suv).rgb * (0.65 + 0.35 * -c);
      }
    }
    col *= 1.0 - 0.35 * (1.0 - abs(c)) * step(0.0, c);                  // the falling flap's shadow
  }
  return vec4(col * mix(0.7, 1.0, seam), 1.0);
}
