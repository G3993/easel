// Name: NY Shatter
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// A breaks like a pane of glass: Voronoi shards crack from the impact point (the centre, hit
// on the beat), then fall away under gravity, spinning and catching the light on their edges,
// revealing B behind.
uniform float shards; // = 9.0

vec2 shH(vec2 p) { p = vec2(dot(p, vec2(127.1, 311.7)), dot(p, vec2(269.5, 183.3))); return fract(sin(p) * 43758.5453); }

vec4 transition(vec2 uv) {
  vec2 asp = vec2(ratio, 1.0);
  vec2 p = uv * asp * max(shards, 2.0);
  // nearest shard seed (3×3 search)
  vec2 ip = floor(p), fp = fract(p);
  float d1 = 9.0, d2 = 9.0; vec2 best = vec2(0.0);
  for (int j = -1; j <= 1; j++) for (int i = -1; i <= 1; i++) {
    vec2 o = vec2(float(i), float(j));
    vec2 pt = o + shH(ip + o);
    float d = length(pt - fp);
    if (d < d1) { d2 = d1; d1 = d; best = ip + o; } else if (d < d2) d2 = d;
  }
  vec2 cen = (best + shH(best)) / max(shards, 2.0) / asp;              // shard centre (uv)
  float dist = length((cen - 0.5) * asp);
  float crack = smoothstep(0.0, 0.25, progress);
  float fall = clamp((progress - 0.15 - dist * 0.35) / 0.55, 0.0, 1.0);
  fall = fall * fall;
  float spin = (shH(best).x - 0.5) * 6.0 * fall;
  vec2 drop = vec2((shH(best).y - 0.5) * 0.3, -1.6) * fall;
  // move this pixel back into the shard's rest frame
  vec2 rel = (uv - cen - drop) * asp;
  float c = cos(-spin), s = sin(-spin);
  rel = mat2(c, -s, s, c) * rel;
  vec2 src = cen + rel / asp;
  // is the source point still inside the same shard?
  vec2 sp = src * asp * max(shards, 2.0);
  vec2 sip = floor(sp), sfp = fract(sp);
  float sd1 = 9.0; vec2 sbest = vec2(0.0);
  for (int j = -1; j <= 1; j++) for (int i = -1; i <= 1; i++) {
    vec2 o = vec2(float(i), float(j));
    float d = length(o + shH(sip + o) - sfp);
    if (d < sd1) { sd1 = d; sbest = sip + o; }
  }
  vec3 B = getToColor(uv).rgb;
  if (fall > 0.0 && (length(sbest - best) > 0.1 || src.x < 0.0 || src.x > 1.0 || src.y < 0.0 || src.y > 1.0)) return vec4(B, 1.0);
  vec3 A = getFromColor(src).rgb;
  float edge = smoothstep(0.06, 0.0, d2 - d1);
  vec3 col = A * (1.0 - 0.25 * fall) + vec3(0.9, 0.95, 1.0) * edge * crack * (0.4 + 0.6 * abs(sin(spin)) + audioBeat * 0.4);
  return vec4(col, 1.0);
}
