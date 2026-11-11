// Name: NY Subway Doors
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// Stainless-steel train doors slide shut over A, hold for a beat, then open onto B. Brushed
// steel with a vertical grain, a window in each leaf showing the scene behind, rubber edge
// seals that meet in the middle, and a bump when they close (the bass kicks the bump).
uniform float hold; // = 0.14
uniform vec4 steelTint; // = vec4(0.72, 0.74, 0.78, 1.0)

float sdHash(vec2 p) { return fract(sin(dot(p, vec2(41.3, 289.1))) * 43758.5453); }
float sdNoise(float x) { float i = floor(x), f = fract(x); return mix(sdHash(vec2(i, 0.0)), sdHash(vec2(i + 1.0, 0.0)), f * f * (3.0 - 2.0 * f)); }

vec4 transition(vec2 uv) {
  float a = (1.0 - hold) * 0.5;
  float close = smoothstep(0.0, a, progress);
  float open = smoothstep(a + hold, 1.0, progress);
  // closing eases in hard and bumps; opening glides
  float shut = close * close * (3.0 - 2.0 * close);
  float bump = exp(-pow((progress - a) / 0.03, 2.0)) * (0.012 + 0.02 * audioBass);
  float cover = shut * (1.0 - open * open * (3.0 - 2.0 * open));       // 0 open … 1 closed
  vec4 scene = progress < a + hold * 0.5 ? getFromColor(uv) : getToColor(uv);
  float half_ = 0.5 * cover;
  float x = uv.x - 0.5;
  float onLeft = step(-0.5, x) * step(x, -0.5 + half_ + bump);
  float onRight = step(0.5 - half_ - bump, x) * step(x, 0.5);
  if (onLeft + onRight < 0.5) return scene;
  // door-local coordinates: 0 at the leading (rubber) edge
  float lead = onLeft > 0.5 ? (-0.5 + half_ + bump) - x : x - (0.5 - half_ - bump);
  vec2 dp = vec2(lead * ratio, uv.y);
  // brushed steel: vertical grain + soft reflections
  float grain = sdNoise(uv.y * 900.0 + floor(uv.x * 400.0) * 13.0) * 0.06 + sdNoise(uv.y * 60.0 + 3.0) * 0.05;
  float sheen = 0.18 * smoothstep(0.0, 0.5, uv.y) * smoothstep(1.0, 0.6, uv.y);
  vec3 steel = steelTint.rgb * (0.78 + grain + sheen) * (0.92 + 0.08 * uv.y);
  // the window: a rounded rectangle showing the blurred scene behind
  vec2 wc = vec2(0.17 * ratio, 0.66);
  vec2 wq = abs(dp - wc) - vec2(0.09 * ratio, 0.17);
  float wd = length(max(wq, 0.0)) + min(max(wq.x, wq.y), 0.0) - 0.02;
  float win = smoothstep(0.003, -0.003, wd);
  vec3 behind = (getFromColor(uv + vec2(0.004, 0.0)).rgb + getFromColor(uv - vec2(0.004, 0.0)).rgb) * 0.5;
  behind = progress > a + hold * 0.5 ? (getToColor(uv + vec2(0.004, 0.0)).rgb + getToColor(uv - vec2(0.004, 0.0)).rgb) * 0.5 : behind;
  vec3 glass = behind * 0.7 + vec3(0.05, 0.06, 0.07) + vec3(0.25) * smoothstep(0.3, 0.0, abs(dp.x - wc.x + dp.y * 0.3 - 0.2));
  vec3 col = mix(steel, glass, win);
  col *= 1.0 - 0.5 * smoothstep(0.012, 0.0, abs(wd));                  // window gasket
  // rubber seal at the leading edge, and the dark recess shadow where the leaves meet
  float seal = smoothstep(0.012, 0.006, lead);
  col = mix(col, vec3(0.04), seal);
  col *= 1.0 - 0.3 * smoothstep(0.06, 0.0, lead) * (1.0 - seal);
  return vec4(col, 1.0);
}
