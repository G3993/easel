// Phase Q v4 — bloom bright-pass.
// Extracts pixels with luminance above a soft threshold. The "soft knee"
// gives a smooth roll-on instead of a hard binary cutoff so highlights
// don't pop in and out.
#version 330 core
in  vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uTexture;
uniform float     uThreshold;   // 0..2  cutoff luminance (linear HDR)
uniform float     uKnee;        // 0..1  soft-knee width

void main() {
    vec3  c = texture(uTexture, vTexCoord).rgb;
    float lum = dot(c, vec3(0.2126, 0.7152, 0.0722));

    // Soft-knee threshold from Karis / Kawase reference.
    float knee  = max(uKnee, 1e-4) * uThreshold;
    float soft  = clamp(lum - uThreshold + knee, 0.0, 2.0 * knee);
    soft        = soft * soft / (4.0 * knee + 1e-4);
    float hard  = max(lum - uThreshold, 0.0);
    float w     = max(soft, hard) / max(lum, 1e-4);

    FragColor = vec4(c * w, 1.0);
}
