#version 330 core
layout(location = 0) in vec2 aPos;
layout(location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;

uniform mat3 uHomography = mat3(1.0);

void main() {
    vec3 warped = uHomography * vec3(aPos, 1.0);
    // Safe perspective divide — clamp w to avoid NaN/Inf when near zero
    float w = warped.z;
    w = sign(w) * max(abs(w), 0.0001);
    vec2 pos = clamp(warped.xy / w, vec2(-4.0), vec2(4.0));
    gl_Position = vec4(pos, 0.0, 1.0);
    vTexCoord = aTexCoord;
}
