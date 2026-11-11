// Name: NY Tile Flip
// Author: Easel / ShaderClaw — Shades of New York
// License: MIT
// The screen is a wall of square tiles. Rings of tiles flip from the centre outward, each
// turning in perspective over its vertical axis: A on the front, B on the back. Tiles lift
// and shade as they turn, glint when they face the light, show their glazed edge, and land
// with a small overshoot. steps > 0 quantises the wave into that many beats.
uniform float tiles; // = 10.0
uniform float flipShare; // = 0.35
uniform float steps; // = 0.0
uniform vec4 groutColor; // = vec4(0.06, 0.055, 0.05, 1.0)

float tfHash(vec2 p) { return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453); }
float tfBack(float x) { float c1 = 1.7, c3 = c1 + 1.0, y = x - 1.0; return 1.0 + c3 * y * y * y + c1 * y * y; }

vec4 transition(vec2 uv) {
  vec2 asp = vec2(ratio, 1.0);
  float n = max(tiles, 2.0);
  vec2 g = uv * asp * n;
  vec2 id = floor(g);
  vec2 lc = fract(g) - 0.5;
  vec2 cen = (id + 0.5) / n / asp;                          // tile centre in uv
  // choreography: rings of tiles from the centre, on the beat grid
  vec2 ringV = (id + 0.5) - 0.5 * n * asp;
  float ring = max(abs(ringV.x) / asp.x, abs(ringV.y));
  float sn = clamp(ring / (0.5 * n) , 0.0, 1.0);
  float prog = progress;
  if (steps > 0.5) prog = (floor(progress * steps) + smoothstep(0.0, 0.35, fract(progress * steps))) / steps;
  float f = clamp((prog - sn * (1.0 - flipShare)) / flipShare, 0.0, 1.0);
  float ang = tfBack(f) * 3.14159265;
  float c = cos(ang), s = sin(ang);
  float sx = max(abs(c), 0.001);
  // perspective: the receding edge shrinks vertically
  float persp = 1.0 + 0.25 * s * lc.x * sign(c);
  vec2 q = vec2(lc.x / sx, lc.y * persp);
  float kick = audioBeat * 0.04 * s;
  q *= 1.0 + kick;
  float inFace = step(abs(q.x), 0.47) * step(abs(q.y), 0.47);
  vec2 suv = cen + q / n / asp;
  vec4 colA = getFromColor(clamp(suv, 0.0, 1.0));
  vec4 colB = getToColor(clamp(suv, 0.0, 1.0));
  vec4 face = f < 0.5 ? colA : colB;
  // light: darker as the tile turns away, a glint as it passes the light, edges brighter
  float shade = 0.55 + 0.45 * sx;
  float glint = exp(-pow((abs(c) - 0.6) / 0.1, 2.0)) * step(0.02, f) * step(f, 0.98);
  float bevel = smoothstep(0.47, 0.40, max(abs(q.x), abs(q.y)));
  vec3 col = face.rgb * shade * mix(0.82, 1.0, bevel) + vec3(1.0, 0.97, 0.92) * glint * 0.35;
  // the glazed edge on the leading side, and the dark bed behind a turning tile
  float edgeW = 0.06 * s;
  float edge = step(0.47 * sx, abs(lc.x)) * step(abs(lc.x), 0.47 * sx + edgeW) * step(abs(lc.y), 0.47);
  vec3 bed = groutColor.rgb * (1.0 + 2.0 * s * (1.0 - f) * f);
  vec3 outCol = mix(bed, col, inFace);
  outCol = mix(outCol, vec3(0.86, 0.82, 0.74) * shade, edge * (1.0 - inFace));
  // untouched tiles show the plain image with hairline grout; settled tiles too
  float grout = step(0.485, max(abs(lc.x), abs(lc.y)));
  if (f <= 0.0) return vec4(mix(colA.rgb, groutColor.rgb, grout * 0.7 * step(0.001, progress)), 1.0);
  if (f >= 1.0) return vec4(mix(colB.rgb, groutColor.rgb, grout * 0.7 * step(progress, 0.999)), 1.0);
  return vec4(outCol, 1.0);
}
