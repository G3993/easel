#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

uniform mat3 uTransform = mat3(1.0);
uniform bool uFlipV = false;
// Sub-rect sampling: remap the 0..1 quad UVs into [offset, offset+scale].
// Defaults are identity, so every existing caller is unaffected. Used by
// the projector present path to show one horizontal slice of a wide
// "span" canvas (e.g. left half on one screen, right half on another).
uniform vec2 uUVOffset = vec2(0.0);
uniform vec2 uUVScale  = vec2(1.0);

void main() {
    vec3 pos = uTransform * vec3(aPos, 1.0);
    gl_Position = vec4(pos.xy, 0.0, 1.0);
    vTexCoord = aTexCoord * uUVScale + uUVOffset;
    if (uFlipV) vTexCoord.y = 1.0 - vTexCoord.y;
}
