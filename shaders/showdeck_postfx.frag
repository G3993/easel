// ShowDeck PROGRAM monitor post-fx — the 7 vertical effect faders (Show
// mode) run through this single pass rather than 7 separate FBO bounces.
// Every uniform is 0..1, 0 = no effect. Geometric warps (mirror/
// quad-mirror/kaleidoscope) reshape the sample UV first; brightness/
// saturation/contrast/hue-rotate then grade the sampled color. Purely a
// preview effect on the deck's own PROGRAM box — not wired into the real
// projector/Zone output.
#version 330 core
in  vec2 vTexCoord;
out vec4 FragColor;

uniform sampler2D uTexture;
uniform float uBrightness;
uniform float uSaturation;
uniform float uContrast;
uniform float uHueRotate;
uniform float uVMirror;
uniform float uQMirror;
uniform float uKaleido;

// Standard hue-rotation matrix (same coefficients as the CSS/SVG
// hue-rotate() filter) — rotates RGB around the luma axis by `angle`
// radians without needing an HSL round-trip.
mat3 hueRotateMatrix(float angle) {
    float c = cos(angle);
    float s = sin(angle);
    return mat3(
        0.213 + c * 0.787 - s * 0.213, 0.213 - c * 0.213 + s * 0.143, 0.213 - c * 0.213 - s * 0.787,
        0.715 - c * 0.715 - s * 0.715, 0.715 + c * 0.285 + s * 0.140, 0.715 - c * 0.715 + s * 0.715,
        0.072 - c * 0.072 + s * 0.928, 0.072 - c * 0.072 - s * 0.283, 0.072 + c * 0.928 + s * 0.072
    );
}

void main() {
    vec2 uv = vTexCoord;

    // Vertical mirror — folds the right half onto the left (a single
    // vertical line of symmetry), Resolume-style.
    if (uVMirror > 0.001) {
        vec2 folded = uv;
        folded.x = uv.x < 0.5 ? uv.x : 1.0 - uv.x;
        uv = mix(uv, folded, uVMirror);
    }

    // Quad mirror — folds both axes for 4-way radial symmetry.
    if (uQMirror > 0.001) {
        vec2 folded = uv;
        folded.x = uv.x < 0.5 ? uv.x : 1.0 - uv.x;
        folded.y = uv.y < 0.5 ? uv.y : 1.0 - uv.y;
        uv = mix(uv, folded, uQMirror);
    }

    // Kaleidoscope — 8-slice radial mirror fold.
    if (uKaleido > 0.001) {
        vec2 cc = uv - 0.5;
        float r = length(cc);
        float a = atan(cc.y, cc.x);
        const float segments = 8.0;
        const float segAngle = 6.2831853 / segments;
        a = mod(a, segAngle);
        a = abs(a - segAngle * 0.5);
        vec2 kUV = 0.5 + r * vec2(cos(a), sin(a));
        uv = mix(uv, kUV, uKaleido);
    }

    uv = clamp(uv, 0.0, 1.0);
    vec4 color = texture(uTexture, uv);

    // Hue rotate / brightness / saturation / contrast grade, in that order.
    // Hue rotate: 0 = normal (no rotation). Turning it up sweeps a ROYGBIV
    // gradient from reddish up to violet — a 270° sweep (not the full 360,
    // which would wrap back past magenta toward red again).
    if (uHueRotate > 0.001) {
        color.rgb = clamp(hueRotateMatrix(uHueRotate * 4.712389) * color.rgb, 0.0, 1.0);
    }
    // 0 = black, 1.0 = unity (the image as shot) — per user request, no
    // boost past unity (was 0=black/0.5=unity/1.0=2x boost).
    color.rgb *= uBrightness;
    float gray = dot(color.rgb, vec3(0.299, 0.587, 0.114));
    // Saturation: 0 = grayscale, 0.5 = unity (unchanged), 1.0 = 10x
    // saturation.
    float satFactor = uSaturation < 0.5
        ? mix(0.0, 1.0, uSaturation * 2.0)
        : mix(1.0, 10.0, (uSaturation - 0.5) * 2.0);
    color.rgb = mix(vec3(gray), color.rgb, satFactor);
    // Contrast: 0 = flat/low contrast, 0.5 = unity (unchanged), 1.0 = super
    // high contrast.
    float contrastFactor = uContrast < 0.5
        ? mix(0.0, 1.0, uContrast * 2.0)
        : mix(1.0, 12.0, (uContrast - 0.5) * 2.0);
    color.rgb = (color.rgb - 0.5) * contrastFactor + 0.5;

    FragColor = vec4(clamp(color.rgb, 0.0, 1.0), color.a);
}
