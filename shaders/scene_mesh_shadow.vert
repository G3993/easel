#version 330 core
layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aTexCoord;
uniform mat4 uModel = mat4(1.0);
uniform mat4 uLightVP = mat4(1.0);
void main() { gl_Position = uLightVP * uModel * vec4(aPos, 1.0); }
