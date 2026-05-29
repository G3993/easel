// Phase Q v4 — bloom composite + global finish. Screen-blends a blurred
// bright-pass on top of the original linear-HDR composite, then applies a
// light global "finish" (saturation + warm/cool grade + film grain + dither)
// so every layer reads closer to an engine/post-stack look in one place.
#version 330 core
in  vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uBase;        // the post-warp composite (linear HDR)
uniform sampler2D uBloom;       // blurred bright-pass
uniform float     uStrength;    // 0..2
uniform float     uTint;        // 0..1  warm tint mix (0 = neutral, 1 = ember)
uniform float     uTime = 0.0;            // animated grain/dither
uniform vec2      uResolution = vec2(1920.0, 1080.0);
uniform float     uFinish = 1.0;          // 0..1 global finish amount

float h12(vec2 p){ return fract(sin(dot(p, vec2(12.9898, 78.233))) * 43758.5453); }

void main() {
    vec3 base  = texture(uBase, vTexCoord).rgb;
    vec3 bloom = texture(uBloom, vTexCoord).rgb * uStrength;

    // Optional warm-tint pass — pulls the bloom slightly toward amber so
    // highlights feel filmic, not LED.
    vec3 warm  = bloom * vec3(1.05, 0.96, 0.84);
    bloom      = mix(bloom, warm, uTint);

    // Pure additive bloom (preserves base HDR; the host tonemap rolls off
    // real HDR values downstream).
    vec3 c = max(base + bloom, 0.0);

    // ── Global finish — HDR-safe (no contrast curve on values >1). ──
    float l = dot(c, vec3(0.2126, 0.7152, 0.0722));
    // Subtle saturation lift.
    c = mix(c, mix(vec3(l), c, 1.12), uFinish);
    // Cool shadows → warm highlights split-tone.
    vec3 grade = mix(vec3(0.96, 0.98, 1.05), vec3(1.05, 1.00, 0.95), clamp(l, 0.0, 1.0));
    c *= mix(vec3(1.0), grade, uFinish);
    // Animated film grain (a touch stronger in the shadows).
    float gn = h12(vTexCoord * uResolution + uTime * 97.0);
    c += (gn - 0.5) * 0.020 * uFinish * (0.7 + 0.5 * (1.0 - clamp(l, 0.0, 1.0)));
    // Ordered-ish dither — kills 8-bit gradient banding across the whole frame.
    float dn = h12(vTexCoord * uResolution + uTime);
    c += (dn - 0.5) / 255.0 * uFinish;

    FragColor = vec4(max(c, 0.0), 1.0);
}
