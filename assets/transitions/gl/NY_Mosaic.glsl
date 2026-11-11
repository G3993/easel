// Name: NY Mosaic
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// A sets into mosaic: the picture coarsens into glazed tesserae with grout (the tiles bevel
// and catch the light), the tesserae turn over to B's colours ring by ring from the centre,
// then the mosaic refines back into the full picture of B.
uniform float coarse; // = 26.0
uniform float steps; // = 0.0

vec4 transition(vec2 uv) {
  float prog = progress;
  if (steps > 0.5) prog = (floor(progress * steps) + smoothstep(0.0, 0.3, fract(progress * steps))) / steps;
  // resolution: full → coarse → full (in powers that read as deliberate steps)
  float k = sin(prog * 3.14159);
  float n = mix(400.0, max(coarse, 4.0), pow(k, 0.6));
  vec2 asp = vec2(ratio, 1.0);
  vec2 g = uv * asp * n;
  vec2 id = floor(g), lc = fract(g) - 0.5;
  vec2 cuv = (id + 0.5) / n / asp;
  vec2 rv = cuv - 0.5;
  float ring = length(rv * asp) / (0.5 * length(asp));
  float flip = smoothstep(ring * 0.6 + 0.2, ring * 0.6 + 0.32, prog);   // centre first
  vec3 a = getFromColor(cuv).rgb, b = getToColor(cuv).rgb;
  vec3 tile = mix(a, b, flip);
  float tileK = smoothstep(80.0, 30.0, n);                                // tile look only when coarse
  float bev = smoothstep(0.5, 0.35, max(abs(lc.x), abs(lc.y)));
  vec2 nrm = lc * (1.0 - bev) * 2.0;
  float light = 1.0 + dot(nrm, vec2(-0.5, 0.6)) * 0.6;
  float grout = smoothstep(0.42, 0.48, max(abs(lc.x), abs(lc.y)));
  vec3 mosaic = mix(tile * light, vec3(0.08, 0.075, 0.07), grout);
  mosaic += vec3(1.0) * pow(max(0.0, 1.0 - length(lc - vec2(-0.2, 0.22)) * 4.0), 3.0) * 0.25;   // glaze glint
  vec3 full = prog < 0.5 ? getFromColor(uv).rgb : getToColor(uv).rgb;
  vec3 col = mix(n > 300.0 ? full : tile, mosaic, tileK);
  col *= 1.0 + 0.15 * audioBeat * tileK;
  return vec4(col, 1.0);
}
