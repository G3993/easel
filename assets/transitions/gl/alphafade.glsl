// ShowDeck "Alpha" blend mode — a per-pixel wipe keyed on the incoming
// (to) layer's alpha channel instead of a flat crossfade. High-alpha
// pixels of the incoming layer arrive early as progress rises; low-alpha
// pixels hold back until progress nears 1. Endpoints are always exactly
// the from/to image regardless of key content (key clamped away from 0).
vec4 transition (vec2 uv) {
  vec4 from = getFromColor(uv);
  vec4 to = getToColor(uv);
  float key = clamp(to.a, 0.0001, 1.0);
  float amt = clamp((progress - (1.0 - key)) / key, 0.0, 1.0);
  return mix(from, to, amt);
}
