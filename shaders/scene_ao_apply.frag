#version 330 core
// Native 3D scene — multiply the composite by the blurred AO (4x4 box over
// the half-res AO map, depth-unaware but cheap; the bilinear fetch hides it).
in vec2 vTexCoord;
out vec4 FragColor;
uniform sampler2D uColor;
uniform sampler2D uAO;
uniform vec2  uAOTexel;
uniform float uStrength = 1.0;
void main() {
    vec4 c = texture(uColor, vTexCoord);
    float ao = 0.0;
    for (int y = -1; y <= 2; y++)
        for (int x = -1; x <= 2; x++)
            ao += texture(uAO, vTexCoord + (vec2(x, y) - 0.5) * uAOTexel).r;
    ao /= 16.0;
    float k = clamp(1.0 - uStrength * (1.0 - ao), 0.0, 1.0);
    FragColor = vec4(c.rgb * k, c.a);   // premultiplied: scale colour only
}
