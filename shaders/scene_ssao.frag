#version 330 core
// Native 3D scene — screen-space ambient occlusion from the shared scene
// depth. Half resolution; hemisphere kernel rotated per pixel; range-checked.
in vec2 vTexCoord;
out vec4 FragColor;
uniform sampler2D uDepth;        // GL_DEPTH_COMPONENT, 0..1
uniform mat4  uProj;
uniform mat4  uInvProj;
uniform float uRadius = 0.25;    // world units
uniform float uBias = 0.01;
uniform vec2  uTexel;            // 1 / depth texture size
uniform int   uTaps = 12;

vec3 viewPos(vec2 uv) {
    float z = texture(uDepth, uv).r;
    vec4 c = uInvProj * vec4(uv * 2.0 - 1.0, z * 2.0 - 1.0, 1.0);
    return c.xyz / c.w;
}
float ign(vec2 p) { return fract(52.9829189 * fract(0.06711056 * p.x + 0.00583715 * p.y)); }

void main() {
    float z0 = texture(uDepth, vTexCoord).r;
    if (z0 >= 0.9999) { FragColor = vec4(1.0); return; }
    vec3 P = viewPos(vTexCoord);
    vec3 Px = viewPos(vTexCoord + vec2(uTexel.x, 0.0)) - P;
    vec3 Py = viewPos(vTexCoord + vec2(0.0, uTexel.y)) - P;
    vec3 N = normalize(cross(Px, Py));
    if (dot(N, -P) < 0.0) N = -N;          // face the eye
    // tangent frame with a per-pixel rotation
    float a = ign(gl_FragCoord.xy) * 6.2831853;
    vec3 rnd = vec3(cos(a), sin(a), 0.0);
    vec3 T = normalize(rnd - N * dot(rnd, N));
    vec3 B = cross(N, T);
    float occ = 0.0;
    for (int i = 0; i < 16; i++) {
        if (i >= uTaps) break;
        float fi = float(i) + 0.5;
        float r = sqrt(fi / float(uTaps));             // even area
        float ph = fi * 2.399963;                       // golden angle
        vec3 k = vec3(cos(ph) * r, sin(ph) * r, 0.0);
        k.z = sqrt(max(0.0, 1.0 - r * r)) * 0.9 + 0.1;  // hemisphere, lifted off the surface
        float scale = mix(0.2, 1.0, (fi / float(uTaps)) * (fi / float(uTaps)));
        vec3 S = P + (T * k.x + B * k.y + N * k.z) * uRadius * scale;
        vec4 c = uProj * vec4(S, 1.0);
        vec2 suv = c.xy / c.w * 0.5 + 0.5;
        if (suv.x < 0.0 || suv.x > 1.0 || suv.y < 0.0 || suv.y > 1.0) continue;
        float sz = viewPos(suv).z;                      // scene depth at the sample's pixel (view z, negative forward)
        float rangeCheck = smoothstep(0.0, 1.0, uRadius / max(abs(P.z - sz), 1e-4));
        occ += (sz >= S.z + uBias ? 1.0 : 0.0) * rangeCheck;
    }
    float ao = 1.0 - occ / float(uTaps);
    FragColor = vec4(vec3(ao), 1.0);
}
