// Name: NY Iris
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// A camera aperture: polygonal iris blades close over A (rotating as they go, with a lens
// fringe and a glow on the edge), and reopen onto B. The blades snap shut on the beat.
uniform float blades; // = 7.0

vec4 transition(vec2 uv) {
  vec2 p = (uv - 0.5) * vec2(ratio, 1.0);
  float n = max(blades, 3.0);
  float r = length(p);
  float closing = progress < 0.5 ? 1.0 - progress * 2.0 : progress * 2.0 - 1.0;   // 1 open → 0 shut → 1 open
  float ease = closing * closing * (3.0 - 2.0 * closing);
  float rot = (1.0 - ease) * 1.2 * (progress < 0.5 ? 1.0 : -1.0);
  float a = atan(p.y, p.x) + rot;
  float seg = 6.2831853 / n;
  float polyR = r * cos(mod(a, seg) - seg * 0.5);                      // distance to a regular polygon edge
  float R = ease * length(vec2(ratio, 1.0)) * 0.6 * (1.0 + 0.04 * audioBeat);
  vec4 scene = progress < 0.5 ? getFromColor(uv) : getToColor(uv);
  float inside = smoothstep(R + 0.004, R - 0.004, polyR);
  // blades: dark brushed metal with a lit rim
  float rim = exp(-pow((polyR - R) / 0.01, 2.0));
  vec3 blade = vec3(0.05, 0.05, 0.06) + vec3(0.12) * fract(a / seg + r * 3.0) * 0.4;
  // lens fringe inside the aperture edge
  vec2 dir = normalize(p + 1e-5);
  float fr = smoothstep(R, R * 0.85, polyR) * (1.0 - smoothstep(R * 0.85, R * 0.6, polyR));
  vec3 fringe = vec3((progress < 0.5 ? getFromColor(uv + dir * 0.006) : getToColor(uv + dir * 0.006)).r, scene.g,
                     (progress < 0.5 ? getFromColor(uv - dir * 0.006) : getToColor(uv - dir * 0.006)).b);
  vec3 col = mix(scene.rgb, fringe, fr * 0.8);
  col = mix(blade, col, inside) + vec3(1.0, 0.9, 0.75) * rim * 0.6;
  return vec4(col, 1.0);
}
