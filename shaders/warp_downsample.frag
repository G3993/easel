// 4-tap box-filter downsample. Each tap is itself a bilinear sample of
// the 4× supersampled warp output, giving an effective 16-texel average
// per 1× output pixel — proper supersample antialiasing on every warp
// silhouette pixel, not the bare bilinear minimum that linear_copy gives.
//
// Offsets are ±0.25 in the 1× pixel space (which becomes ±0.5 in 4×
// texels), placing each tap at the center of one 2×2 quad of source
// texels — so 4 taps × 4 texels per bilinear sample = 16 texels read.
#version 330 core
in  vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uTexture;
uniform vec2      uTexelSize;   // 1.0 / sourceSize (the 4× SS FBO)

void main() {
    vec2 o = uTexelSize * 1.0;  // 1 source-texel offset → 0.25 of a 1× pixel at 4× SS
    vec3 c =
        texture(uTexture, vTexCoord + vec2(-o.x, -o.y)).rgb +
        texture(uTexture, vTexCoord + vec2( o.x, -o.y)).rgb +
        texture(uTexture, vTexCoord + vec2(-o.x,  o.y)).rgb +
        texture(uTexture, vTexCoord + vec2( o.x,  o.y)).rgb;
    FragColor = vec4(c * 0.25, 1.0);
}
