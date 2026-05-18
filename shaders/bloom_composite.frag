// Phase Q v4 — bloom composite. Screen-blends a blurred bright-pass on
// top of the original linear-HDR composite. Caller supplies bloom
// strength + dirt-mask (optional, set 0 to skip).
#version 330 core
in  vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uBase;        // the post-warp composite (linear HDR)
uniform sampler2D uBloom;       // blurred bright-pass
uniform float     uStrength;    // 0..2
uniform float     uTint;        // 0..1  warm tint mix (0 = neutral, 1 = ember)

void main() {
    vec3 base  = texture(uBase, vTexCoord).rgb;
    vec3 bloom = texture(uBloom, vTexCoord).rgb * uStrength;

    // Optional warm-tint pass — pulls the bloom slightly toward
    // amber so highlights feel filmic, not LED.
    vec3 warm  = bloom * vec3(1.05, 0.96, 0.84);
    bloom      = mix(bloom, warm, uTint);

    // Pure additive bloom. Preserves base HDR fully (the previous
    // screen-blend formula `1 - (1-base)(1-bloom)` clamped base to
    // [0,1] and crushed everything above 1.0 down to white, which
    // then ACES'd as off-white — pure-white shaders looked gray).
    // Additive lets the host ACES tonemap roll off real HDR values.
    vec3 outc = base + bloom;

    FragColor = vec4(outc, 1.0);
}
