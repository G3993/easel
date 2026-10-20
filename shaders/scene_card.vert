#version 330 core
// Native 3D scene — a layer standing as a card in the zone's world.
layout(location = 0) in vec2 aPos;       // unit quad, -1..1
layout(location = 1) in vec2 aTexCoord;

out vec2 vTexCoord;
out vec3 vWorldPos;
out vec3 vNormal;
out vec4 vShadowCoord;

uniform mat4 uModel = mat4(1.0);
uniform mat4 uViewProj = mat4(1.0);
uniform mat4 uLightVP = mat4(1.0);
uniform bool uFlipV = false;

void main() {
    vec4 world = uModel * vec4(aPos, 0.0, 1.0);
    gl_Position = uViewProj * world;
    vWorldPos = world.xyz;
    vNormal = normalize(mat3(uModel) * vec3(0.0, 0.0, 1.0));
    vShadowCoord = uLightVP * world;
    vTexCoord = aTexCoord;
    if (uFlipV) vTexCoord.y = 1.0 - vTexCoord.y;
}
