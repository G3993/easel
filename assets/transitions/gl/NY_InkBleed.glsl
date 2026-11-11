// Name: NY Ink Bleed
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// B spreads into A like ink into wet paper: it blooms out of a few fixed points (centre first,
// then the corners), the edge feathers through the paper fibres and leaves a dark tide line.
uniform vec4 inkRim; // = vec4(0.05, 0.04, 0.06, 1.0)

float ibHash(vec2 p) { return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453); }
float ibNoise(vec2 x) { vec2 i = floor(x), f = fract(x); f = f * f * (3.0 - 2.0 * f);
  return mix(mix(ibHash(i), ibHash(i + vec2(1, 0)), f.x), mix(ibHash(i + vec2(0, 1)), ibHash(i + vec2(1, 1)), f.x), f.y); }

vec4 transition(vec2 uv) {
  vec2 p = uv * vec2(ratio, 1.0);
  float fib = ibNoise(p * 6.0) * 0.5 + ibNoise(p * 17.0) * 0.3 + ibNoise(p * 50.0) * 0.2;
  // distance to the nearest bloom point, each starting a little later than the last
  vec2 c0 = vec2(0.5 * ratio, 0.5);
  float d = length(p - c0);
  d = min(d, length(p - vec2(0.12 * ratio, 0.85)) + 0.25);
  d = min(d, length(p - vec2(0.88 * ratio, 0.15)) + 0.25);
  d = min(d, length(p - vec2(0.85 * ratio, 0.88)) + 0.4);
  d = min(d, length(p - vec2(0.15 * ratio, 0.12)) + 0.4);
  float front = progress * (length(vec2(ratio, 1.0)) * 0.7 + 0.5);
  float v = d + (fib - 0.5) * 0.18 - front;
  float m = smoothstep(0.02, -0.02, v);
  float rim = exp(-pow(v / 0.012, 2.0)) * (1.0 - progress * 0.5);
  vec3 col = mix(getFromColor(uv).rgb, getToColor(uv).rgb, m);
  col = mix(col, inkRim.rgb, rim * 0.75);
  col *= 1.0 - 0.15 * smoothstep(0.08, 0.0, abs(v)) * (1.0 - m);     // wet halo darkens the paper ahead
  return vec4(col, 1.0);
}
