// Canvas-mask edit overlay burned into the projected output: the path's
// outline plus a round handle on every anchor, so the mask can be lined up
// against the real surface from the projector, not just the editor.
// Drawn over the final warped frame with alpha blending. Mask points are in
// the same 0-1 output UV space as the frame (origin bottom-left).
#version 330 core
in  vec2 vTexCoord;
out vec4 FragColor;

uniform vec2  uRes;
uniform vec2  uOutline[160];   // tessellated path (UV)
uniform int   uOutlineCount;
uniform bool  uClosed;
uniform vec2  uAnchors[48];    // anchor points (UV)
uniform int   uAnchorCount;
uniform int   uSelected;       // selected anchor (-1 = none)
uniform float uTime;

float segDist(vec2 p, vec2 a, vec2 b) {
    vec2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / max(dot(ba, ba), 1e-6), 0.0, 1.0);
    return length(pa - ba * h);
}

void main() {
    vec2 p = vTexCoord * uRes;
    float s = min(uRes.x, uRes.y);
    float lineW = max(1.5, s * 0.0018);

    float dl = 1e9;
    int segs = uClosed ? uOutlineCount : uOutlineCount - 1;
    for (int i = 0; i < segs; i++) {
        vec2 a = uOutline[i] * uRes;
        vec2 b = uOutline[(i + 1) % uOutlineCount] * uRes;
        dl = min(dl, segDist(p, a, b));
    }
    // dashed marching outline: white with a dark edge so it reads on any content
    float along = p.x + p.y;
    float dash = step(0.5, fract(along / (s * 0.02) - uTime * 0.5));
    float line = 1.0 - smoothstep(lineW - 0.75, lineW + 0.75, dl);
    float lineHalo = 1.0 - smoothstep(lineW * 2.5 - 0.75, lineW * 2.5 + 0.75, dl);

    vec4 o = vec4(0.0);
    o = mix(o, vec4(0.0, 0.0, 0.0, 0.7), lineHalo);
    o = mix(o, vec4(mix(vec3(1.0), vec3(0.0, 0.9, 1.0), dash), 1.0), line);

    float R = s * 0.016, w = max(1.5, s * 0.003);
    for (int i = 0; i < uAnchorCount; i++) {
        float r = length(p - uAnchors[i] * uRes);
        bool sel = (i == uSelected);
        float RR = sel ? R * 1.35 : R;
        float ring = 1.0 - smoothstep(w - 0.75, w + 0.75, abs(r - RR));
        float halo = 1.0 - smoothstep(w * 2.2 - 0.75, w * 2.2 + 0.75, abs(r - RR));
        float dot_ = 1.0 - smoothstep(w * 1.3 - 0.75, w * 1.3 + 0.75, r);
        vec3 c = sel ? vec3(1.0, 0.85, 0.1) : vec3(1.0, 40.0 / 255.0, 220.0 / 255.0);
        o = mix(o, vec4(0.0, 0.0, 0.0, 0.85), halo);
        o = mix(o, vec4(c, 1.0), max(ring, dot_));
    }
    FragColor = o;
}
