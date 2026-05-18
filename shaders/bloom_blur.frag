// Phase Q v4 — separable Gaussian blur (9-tap, σ ≈ 1.5 in texels).
// Run once horizontal, once vertical. The texel direction is supplied
// by the caller via uDirection.
#version 330 core
in  vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uTexture;
uniform vec2      uDirection;    // (1/w, 0) for H, (0, 1/h) for V

void main() {
    // Symmetric 9-tap Gaussian with weights from
    //   exp(-x²/(2σ²)) / Σ, σ ≈ 1.5
    const float w0 = 0.227027;
    const float w1 = 0.1945946;
    const float w2 = 0.1216216;
    const float w3 = 0.054054;
    const float w4 = 0.016216;

    vec3 sum = texture(uTexture, vTexCoord).rgb * w0;
    sum += texture(uTexture, vTexCoord + uDirection * 1.0).rgb * w1;
    sum += texture(uTexture, vTexCoord - uDirection * 1.0).rgb * w1;
    sum += texture(uTexture, vTexCoord + uDirection * 2.0).rgb * w2;
    sum += texture(uTexture, vTexCoord - uDirection * 2.0).rgb * w2;
    sum += texture(uTexture, vTexCoord + uDirection * 3.0).rgb * w3;
    sum += texture(uTexture, vTexCoord - uDirection * 3.0).rgb * w3;
    sum += texture(uTexture, vTexCoord + uDirection * 4.0).rgb * w4;
    sum += texture(uTexture, vTexCoord - uDirection * 4.0).rgb * w4;

    FragColor = vec4(sum, 1.0);
}
