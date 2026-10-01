// ShowDeck "Blue" blend mode — same per-pixel wipe as alphafade.glsl,
// keyed on the incoming (to) layer's blue channel.
vec4 transition (vec2 uv) {
  vec4 from = getFromColor(uv);
  vec4 to = getToColor(uv);
  float key = clamp(to.b, 0.0001, 1.0);
  float amt = clamp((progress - (1.0 - key)) / key, 0.0, 1.0);
  return mix(from, to, amt);
}
