// Name: NY Train Pass
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// A train blows through the frame: lit cars with windows and gaps between them rush past
// left to right, the air around them smears the picture, and when the last car clears, the
// next scene is standing there. Bass shakes the cars.
uniform float cars; // = 3.0
uniform vec4 bodyTint; // = vec4(0.55, 0.57, 0.6, 1.0)
uniform vec4 lineColor; // = vec4(0.93, 0.21, 0.18, 1.0)

vec4 transition(vec2 uv) {
  float len = max(cars, 1.0) * 1.15;                                     // train length in screen widths
  float head = mix(-0.05, 1.0 + len, progress * progress * (3.0 - 2.0 * progress));
  float tail = head - len;
  float x = uv.x;
  // the picture behind: A ahead of the train, B behind it, smeared by the wind near the cars
  float near = smoothstep(0.25, 0.0, min(abs(x - head), abs(x - tail)));
  vec2 blur = vec2(0.03 * near, 0.0);
  vec3 A = (getFromColor(uv).rgb + getFromColor(uv - blur).rgb + getFromColor(uv - 2.0 * blur).rgb) / 3.0;
  vec3 B = (getToColor(uv).rgb + getToColor(uv + blur).rgb + getToColor(uv + 2.0 * blur).rgb) / 3.0;
  vec3 scene = x > head ? A : B;
  float shake = (audioBass * 0.01 + 0.003) * sin(progress * 180.0);
  float y = uv.y + shake;
  float onTrain = step(tail, x) * step(x, head) * step(0.12, y) * step(y, 0.88);
  if (onTrain < 0.5) return vec4(scene, 1.0);
  float cx = (x - tail) / len * max(cars, 1.0);
  float carI = floor(cx), cf = fract(cx);
  float gap = step(cf, 0.03) + step(0.97, cf);
  if (gap > 0.5) return vec4(scene * 0.3, 1.0);
  // car body: ribbed steel, a line-colour stripe, a row of lit windows with passengers
  float rib = 0.9 + 0.1 * sin(y * 420.0);
  vec3 body = bodyTint.rgb * rib * (0.85 + 0.25 * smoothstep(0.12, 0.88, y));
  float stripe = step(0.3, y) * step(y, 0.34);
  body = mix(body, lineColor.rgb, stripe);
  float wx = fract(cf * 6.0);
  float win = step(0.15, wx) * step(wx, 0.85) * step(0.45, y) * step(y, 0.72);
  vec3 inside = vec3(1.0, 0.95, 0.82) * 0.9;
  float person = step(0.5, fract(sin((carI * 6.0 + floor(cf * 6.0)) * 12.9898) * 43758.5)) * smoothstep(0.08, 0.0, abs(wx - 0.5) - 0.1) * step(y, 0.66);
  inside = mix(inside, vec3(0.15, 0.13, 0.12), person * 0.8);
  vec3 col = mix(body, inside, win);
  // doors, motion streaks, and the headlight glare on the leading car
  col *= 1.0 - 0.4 * step(abs(fract(cf * 2.0) - 0.5), 0.004);
  col += vec3(0.2) * pow(0.5 + 0.5 * sin(y * 90.0 + progress * 60.0), 40.0);
  col += vec3(1.0, 0.95, 0.8) * smoothstep(0.02, 0.0, head - x) * 1.5;
  return vec4(col, 1.0);
}
