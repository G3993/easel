// Default "nothing loaded" test pattern — the user's own broadcast-style
// test card image as the background, now rendered into a 16:9 canvas so it
// fills the frame edge-to-edge (no letterboxing), with a duck sprite
// (chroma-keyed out of its own reference photo) bouncing around the four
// walls on top as a simple "tester", per user request. Both are real
// images (assets/testcard.png, assets/duck.png), not procedural.
#version 330 core
in  vec2 vTexCoord;
out vec4 FragColor;

uniform float uTime;
uniform sampler2D uTestCardTex;
uniform sampler2D uDuckTex;

float triWave(float t) {
    float f = fract(t);
    return abs(f * 2.0 - 1.0);
}

void main() {
    vec2 uv = vTexCoord;

    // Canvas is now 16:9 itself (matches the test card's own ~16:9 aspect),
    // so it just samples straight across — no letterbox fit needed.
    vec3 col = texture(uTestCardTex, uv).rgb;

    // Bouncing duck — a cutout of the reference duck photo (chroma-keyed
    // off its turquoise background), 4x bigger than last round, per user
    // request. duckSize.x is aspect-corrected against the 16:9 canvas so
    // the (square) duck sprite doesn't stretch wide. Position bounces
    // independently per axis (deterministic from uTime via a triangle
    // wave), which reads as bouncing off all four walls.
    const float kCanvasAspect = 16.0 / 9.0;
    vec2 duckSize = vec2(0.1 / kCanvasAspect, 0.1);
    vec2 margin = duckSize * 0.6 + vec2(0.02);
    vec2 duckPos = margin + (1.0 - 2.0 * margin) *
                   vec2(triWave(uTime * 0.13), triWave(uTime * 0.081));
    vec2 duv = (uv - duckPos) / duckSize + 0.5;
    if (duv.x >= 0.0 && duv.x <= 1.0 && duv.y >= 0.0 && duv.y <= 1.0) {
        vec3 dcol = texture(uDuckTex, duv).rgb;
        const vec3 bgRef = vec3(0.27, 0.82, 0.85);   // the photo's turquoise backdrop
        float distBg = length(dcol - bgRef);
        float duckMask = smoothstep(0.12, 0.30, distBg);
        col = mix(col, dcol, duckMask);
    }

    FragColor = vec4(col, 1.0);
}
