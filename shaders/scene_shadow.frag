#version 330 core
// Depth-only shadow pass for cards: the layer's alpha cuts the silhouette.
in vec2 vTexCoord;
uniform sampler2D uTexture;
uniform vec4 uCrop = vec4(0);   // left, right, top, bottom
uniform float uOpacity = 1.0;
void main() {
    vec2 uv = vTexCoord;
    if (uv.x < uCrop.x || uv.x > 1.0 - uCrop.y || uv.y < uCrop.w || uv.y > 1.0 - uCrop.z) discard;
    float a = texture(uTexture, uv).a * uOpacity;
    if (a < 0.35) discard;
}
