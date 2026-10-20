#version 330 core
// Native 3D scene — a mesh object in the zone's world.
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aTexCoord;
out vec3 vWorldPos;
out vec2 vTexCoord;
out vec4 vShadowCoord;
uniform mat4 uModel = mat4(1.0);
uniform mat4 uViewProj = mat4(1.0);
uniform mat4 uLightVP = mat4(1.0);
void main() {
    vec4 world = uModel * vec4(aPos, 1.0);
    gl_Position = uViewProj * world;
    vWorldPos = world.xyz;
    vTexCoord = aTexCoord;
    vShadowCoord = uLightVP * world;
}
