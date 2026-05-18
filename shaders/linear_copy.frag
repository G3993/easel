// Phase Q v4 — straight linear-HDR copy. NO tonemap, NO opacity, NO mask,
// NO mosaic. Used by the bloom pipeline to write its post-composite
// result back into warpFBO without re-applying the ACES rolloff that
// `passthrough.frag` does as the final present step.
#version 330 core
in  vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uTexture;

void main() {
    FragColor = vec4(texture(uTexture, vTexCoord).rgb, 1.0);
}
