// Hard switch, no blending — ShowDeck "Cut" blend mode.
vec4 transition (vec2 uv) {
  return progress < 0.5 ? getFromColor(uv) : getToColor(uv);
}
