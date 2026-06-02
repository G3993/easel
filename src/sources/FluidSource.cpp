#include "sources/FluidSource.h"
#include "app/MIDIManager.h"
#include <GLFW/glfw3.h>
#include <iostream>
#include <cmath>

// ── GLSL 330 core shaders (ported from ShaderClaw3 ES2 sources) ──────
// ES2 → desktop core mapping: attribute→in, varying→out/in,
// texture2D→texture, gl_FragColor→`out vec4 fragColor`.

static const char* VS_BASE = R"(#version 330 core
layout(location=0) in vec2 position;
out vec2 vUv; out vec2 vL, vR, vT, vB;
uniform vec2 texelSize;
void main(){
    vUv = position * 0.5 + 0.5;
    vL = vUv - vec2(texelSize.x, 0.0);
    vR = vUv + vec2(texelSize.x, 0.0);
    vT = vUv + vec2(0.0, texelSize.y);
    vB = vUv - vec2(0.0, texelSize.y);
    gl_Position = vec4(position, 0.0, 1.0);
})";

static const char* VS_SIMPLE = R"(#version 330 core
layout(location=0) in vec2 position;
out vec2 vUv;
void main(){ vUv = position*0.5+0.5; gl_Position = vec4(position,0.0,1.0); })";

// Pavel Dobryakov's exact splat shader — pure additive for BOTH velocity
// and dye. (The earlier screen-blend dye variant was a ShaderClaw tweak
// that changed the canonical look.)
static const char* FS_SPLAT = R"(#version 330 core
in vec2 vUv; out vec4 fragColor;
uniform sampler2D uTarget; uniform float aspectRatio;
uniform vec3 color; uniform vec2 point; uniform float radius;
void main(){
    vec2 p = vUv - point.xy; p.x *= aspectRatio;
    vec3 splat = exp(-dot(p,p)/radius) * color;
    vec3 base = texture(uTarget, vUv).xyz;
    fragColor = vec4(base + splat, 1.0);
})";

// FS_INJECT — additive dye contribution from an external texture. Runs
// once per frame against the dye field BEFORE the sim step so the
// advection pass smears the fresh pixels through the velocity field
// (the classic "fluid carries the picture" look). uFlipY handles
// top-down sources (NDI/video) without an explicit CPU flip.
static const char* FS_INJECT = R"(#version 330 core
in vec2 vUv; out vec4 fragColor;
uniform sampler2D uTarget;    // current dye
uniform sampler2D uImage;     // bound source texture
uniform float     uIntensity; // 0..1 per-frame strength
uniform float     uFlipY;     // 1.0 = top-down source, 0.0 = bottom-up
void main(){
    vec2  iv  = vec2(vUv.x, mix(vUv.y, 1.0 - vUv.y, uFlipY));
    vec3  base = texture(uTarget, vUv).rgb;
    vec3  add  = texture(uImage,  iv ).rgb;
    fragColor  = vec4(base + add * uIntensity, 1.0);
})";

static const char* FS_ADVECT = R"(#version 330 core
in vec2 vUv; out vec4 fragColor;
uniform sampler2D uVelocity; uniform sampler2D uSource;
uniform vec2 texelSize; uniform float dt; uniform float dissipation;
void main(){
    vec2 coord = vUv - dt * texture(uVelocity, vUv).xy * texelSize;
    vec4 result = texture(uSource, coord);
    float decay = 1.0 + dissipation * dt;
    fragColor = result / decay;
})";

static const char* FS_DIVERGENCE = R"(#version 330 core
in vec2 vUv; in vec2 vL, vR, vT, vB; out vec4 fragColor;
uniform sampler2D uVelocity;
void main(){
    float L = texture(uVelocity, vL).x;
    float R = texture(uVelocity, vR).x;
    float T = texture(uVelocity, vT).y;
    float B = texture(uVelocity, vB).y;
    vec2 C = texture(uVelocity, vUv).xy;
    if (vL.x < 0.0) L = -C.x;
    if (vR.x > 1.0) R = -C.x;
    if (vT.y > 1.0) T = -C.y;
    if (vB.y < 0.0) B = -C.y;
    float div = 0.5 * (R - L + T - B);
    fragColor = vec4(div, 0.0, 0.0, 1.0);
})";

static const char* FS_CURL = R"(#version 330 core
in vec2 vUv; in vec2 vL, vR, vT, vB; out vec4 fragColor;
uniform sampler2D uVelocity;
void main(){
    float L = texture(uVelocity, vL).y;
    float R = texture(uVelocity, vR).y;
    float T = texture(uVelocity, vT).x;
    float B = texture(uVelocity, vB).x;
    fragColor = vec4(0.5*(R - L - T + B), 0.0, 0.0, 1.0);
})";

static const char* FS_VORTICITY = R"(#version 330 core
in vec2 vUv; in vec2 vL, vR, vT, vB; out vec4 fragColor;
uniform sampler2D uVelocity; uniform sampler2D uCurl;
uniform float curl; uniform float dt;
void main(){
    float L = texture(uCurl, vL).x;
    float R = texture(uCurl, vR).x;
    float T = texture(uCurl, vT).x;
    float B = texture(uCurl, vB).x;
    float C = texture(uCurl, vUv).x;
    vec2 force = 0.5 * vec2(abs(T)-abs(B), abs(R)-abs(L));
    force /= length(force) + 0.0001;
    force *= curl * C; force.y *= -1.0;
    vec2 velocity = texture(uVelocity, vUv).xy;
    velocity += force * dt;
    velocity = clamp(velocity, -1000.0, 1000.0);
    fragColor = vec4(velocity, 0.0, 1.0);
})";

static const char* FS_PRESSURE = R"(#version 330 core
in vec2 vUv; in vec2 vL, vR, vT, vB; out vec4 fragColor;
uniform sampler2D uPressure; uniform sampler2D uDivergence;
void main(){
    float L = texture(uPressure, vL).x;
    float R = texture(uPressure, vR).x;
    float T = texture(uPressure, vT).x;
    float B = texture(uPressure, vB).x;
    float divergence = texture(uDivergence, vUv).x;
    fragColor = vec4((L+R+B+T-divergence)*0.25, 0.0, 0.0, 1.0);
})";

static const char* FS_GRADSUB = R"(#version 330 core
in vec2 vUv; in vec2 vL, vR, vT, vB; out vec4 fragColor;
uniform sampler2D uPressure; uniform sampler2D uVelocity;
void main(){
    float L = texture(uPressure, vL).x;
    float R = texture(uPressure, vR).x;
    float T = texture(uPressure, vT).x;
    float B = texture(uPressure, vB).x;
    vec2 velocity = texture(uVelocity, vUv).xy;
    velocity -= vec2(R - L, T - B);
    fragColor = vec4(velocity, 0.0, 1.0);
})";

static const char* FS_CLEAR = R"(#version 330 core
in vec2 vUv; out vec4 fragColor;
uniform sampler2D uTexture; uniform float value;
void main(){ fragColor = value * texture(uTexture, vUv); })";

// ── Post-processing chain (Pavel Dobryakov, verbatim) ────────────────
// Bloom prefilter: threshold + soft-knee curve, isolates bright dye.
static const char* FS_BLOOM_PREFILTER = R"(#version 330 core
in vec2 vUv; out vec4 fragColor;
uniform sampler2D uTexture; uniform vec3 curve; uniform float threshold;
void main(){
    vec3 c = texture(uTexture, vUv).rgb;
    float br = max(c.r, max(c.g, c.b));
    float rq = clamp(br - curve.x, 0.0, curve.y);
    rq = curve.z * rq * rq;
    c *= max(rq, br - threshold) / max(br, 0.0001);
    fragColor = vec4(c, 0.0);
})";

// Bloom blur: 4-tap box (vL/vR/vT/vB), used for both down- and up-sample.
static const char* FS_BLOOM_BLUR = R"(#version 330 core
in vec2 vUv; in vec2 vL, vR, vT, vB; out vec4 fragColor;
uniform sampler2D uTexture;
void main(){
    vec4 sum = vec4(0.0);
    sum += texture(uTexture, vL);
    sum += texture(uTexture, vR);
    sum += texture(uTexture, vT);
    sum += texture(uTexture, vB);
    sum *= 0.25;
    fragColor = sum;
})";

// Bloom final: same 4-tap blur × intensity into the full-res bloom target.
static const char* FS_BLOOM_FINAL = R"(#version 330 core
in vec2 vUv; in vec2 vL, vR, vT, vB; out vec4 fragColor;
uniform sampler2D uTexture; uniform float intensity;
void main(){
    vec4 sum = vec4(0.0);
    sum += texture(uTexture, vL);
    sum += texture(uTexture, vR);
    sum += texture(uTexture, vT);
    sum += texture(uTexture, vB);
    sum *= 0.25;
    fragColor = sum * intensity;
})";

// Sunrays mask: bright dye → low alpha occluder for the radial blur.
static const char* FS_SUNRAYS_MASK = R"(#version 330 core
in vec2 vUv; out vec4 fragColor;
uniform sampler2D uTexture;
void main(){
    vec4 c = texture(uTexture, vUv);
    float br = max(c.r, max(c.g, c.b));
    c.a = 1.0 - min(max(br * 20.0, 0.0), 0.8);
    fragColor = c;
})";

// Sunrays: radial light-shaft accumulation toward screen center.
static const char* FS_SUNRAYS = R"(#version 330 core
in vec2 vUv; out vec4 fragColor;
uniform sampler2D uTexture; uniform float weight;
#define ITERATIONS 16
void main(){
    float Density = 0.3;
    float Decay = 0.95;
    float Exposure = 0.7;
    vec2 coord = vUv;
    vec2 dir = vUv - 0.5;
    dir *= 1.0 / float(ITERATIONS) * Density;
    float illuminationDecay = 1.0;
    float color = texture(uTexture, vUv).a;
    for (int i = 0; i < ITERATIONS; i++) {
        coord -= dir;
        float col = texture(uTexture, coord).a;
        color += col * illuminationDecay * weight;
        illuminationDecay *= Decay;
    }
    fragColor = vec4(color * Exposure, 0.0, 0.0, 1.0);
})";

// Separable Gaussian (1.33333 dual-tap), applied to the sunrays buffer.
static const char* VS_BLUR = R"(#version 330 core
layout(location=0) in vec2 position;
out vec2 vUv; out vec2 vL, vR;
uniform vec2 texelSize;
void main(){
    vUv = position * 0.5 + 0.5;
    float offset = 1.33333333;
    vL = vUv - texelSize * offset;
    vR = vUv + texelSize * offset;
    gl_Position = vec4(position, 0.0, 1.0);
})";

static const char* FS_BLUR = R"(#version 330 core
in vec2 vUv; in vec2 vL, vR; out vec4 fragColor;
uniform sampler2D uTexture;
void main(){
    vec4 sum = texture(uTexture, vUv) * 0.29411764;
    sum += texture(uTexture, vL) * 0.35294117;
    sum += texture(uTexture, vR) * 0.35294117;
    fragColor = sum;
})";

// Pavel's exact displayShaderSource body (sans #version, which is prepended
// along with #define SHADING/BLOOM/SUNRAYS by compileDisplay()). Same
// diffuse-normal shading + bloom/sunrays composite + dithering as the demo.
static const char* FS_DISPLAY_BODY = R"(
in vec2 vUv; in vec2 vL, vR, vT, vB; out vec4 fragColor;
uniform sampler2D uTexture;
uniform sampler2D uBloom;
uniform sampler2D uSunrays;
uniform sampler2D uDithering;
uniform vec2 ditherScale;
uniform vec2 texelSize;

vec3 linearToGamma(vec3 color){
    color = max(color, vec3(0));
    return max(1.055 * pow(color, vec3(0.416666667)) - 0.055, vec3(0));
}

void main(){
    vec3 c = texture(uTexture, vUv).rgb;

#ifdef SHADING
    vec3 lc = texture(uTexture, vL).rgb;
    vec3 rc = texture(uTexture, vR).rgb;
    vec3 tc = texture(uTexture, vT).rgb;
    vec3 bc = texture(uTexture, vB).rgb;

    float dx = length(rc) - length(lc);
    float dy = length(tc) - length(bc);

    vec3 n = normalize(vec3(dx, dy, length(texelSize)));
    vec3 l = vec3(0.0, 0.0, 1.0);

    float diffuse = clamp(dot(n, l) + 0.7, 0.7, 1.0);
    c *= diffuse;
#endif

#ifdef BLOOM
    vec3 bloom = texture(uBloom, vUv).rgb;
#endif

#ifdef SUNRAYS
    float sunrays = texture(uSunrays, vUv).r;
    c *= sunrays;
#ifdef BLOOM
    bloom *= sunrays;
#endif
#endif

#ifdef BLOOM
    float noise = texture(uDithering, vUv * ditherScale).r;
    noise = noise * 2.0 - 1.0;
    bloom += noise / 255.0;
    bloom = linearToGamma(bloom);
    c += bloom;
#endif

    float a = max(c.r, max(c.g, c.b));
    fragColor = vec4(c, a);
})";

// ── compile helpers ─────────────────────────────────────────────────

static GLuint compileStage(GLenum type, const char* src) {
    GLuint s = glCreateShader(type);
    glShaderSource(s, 1, &src, nullptr);
    glCompileShader(s);
    GLint ok = 0; glGetShaderiv(s, GL_COMPILE_STATUS, &ok);
    if (!ok) {
        char log[2048]; glGetShaderInfoLog(s, sizeof(log), nullptr, log);
        std::cerr << "[FluidSource] shader compile error: " << log << std::endl;
        glDeleteShader(s); return 0;
    }
    return s;
}

GLuint FluidSource::compileProgram(const char* vs, const char* fs) {
    GLuint v = compileStage(GL_VERTEX_SHADER, vs);
    GLuint f = compileStage(GL_FRAGMENT_SHADER, fs);
    if (!v || !f) return 0;
    GLuint p = glCreateProgram();
    glAttachShader(p, v); glAttachShader(p, f);
    glLinkProgram(p);
    glDeleteShader(v); glDeleteShader(f);
    GLint ok = 0; glGetProgramiv(p, GL_LINK_STATUS, &ok);
    if (!ok) {
        char log[2048]; glGetProgramInfoLog(p, sizeof(log), nullptr, log);
        std::cerr << "[FluidSource] link error: " << log << std::endl;
        glDeleteProgram(p); return 0;
    }
    return p;
}

// Compile a display program with the requested feature #defines injected
// after the #version directive (GLSL requires #version on line 1). Mirrors
// Pavel's keyword material: each combination is a distinct linked program.
GLuint FluidSource::compileDisplay(bool shading, bool bloom, bool sunrays) {
    std::string defs = "#version 330 core\n";
    if (shading) defs += "#define SHADING\n";
    if (bloom)   defs += "#define BLOOM\n";
    if (sunrays) defs += "#define SUNRAYS\n";
    std::string fs = defs + FS_DISPLAY_BODY;
    // VS_BASE already starts with its own #version; that's fine for the VS.
    return compileProgram(VS_BASE, fs.c_str());
}

GLuint FluidSource::displayProgram(bool shading, bool bloom, bool sunrays) {
    int key = (shading ? 1 : 0) | (bloom ? 2 : 0) | (sunrays ? 4 : 0);
    if (!m_displayPrograms[key])
        m_displayPrograms[key] = compileDisplay(shading, bloom, sunrays);
    return m_displayPrograms[key];
}

// Small white-noise texture standing in for Pavel's blue-noise dithering
// PNG. The display adds only ±1/255 of this, so the exact pattern is
// imperceptible — it just breaks up bloom banding on the 8-bit output.
void FluidSource::createDitherTexture() {
    m_ditherW = m_ditherH = 64;
    std::vector<unsigned char> px(m_ditherW * m_ditherH * 4);
    std::uniform_int_distribution<int> d(0, 255);
    for (size_t i = 0; i < px.size(); i += 4) {
        unsigned char n = (unsigned char)d(m_rng);
        px[i] = px[i+1] = px[i+2] = n; px[i+3] = 255;
    }
    glGenTextures(1, &m_ditherTex);
    glBindTexture(GL_TEXTURE_2D, m_ditherTex);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8, m_ditherW, m_ditherH, 0,
                 GL_RGBA, GL_UNSIGNED_BYTE, px.data());
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    glBindTexture(GL_TEXTURE_2D, 0);
}

// Pavel's getResolution(): scale a base resolution to the output aspect,
// putting the larger dimension along the longer axis.
void FluidSource::getResolution(int res, int& w, int& h) const {
    float aspect = (float)m_outW / (float)m_outH;
    if (aspect < 1.0f) aspect = 1.0f / aspect;
    int lo = (int)std::round((float)res);
    int hi = (int)std::round((float)res * aspect);
    if (m_outW > m_outH) { w = hi; h = lo; }
    else                 { w = lo; h = hi; }
}

void FluidSource::DoubleFBO::swap() { std::swap(read, write); }

FluidSource::FBO FluidSource::createFBO(int w, int h, GLint internalFmt,
                                        GLenum type, GLenum filter) {
    FBO f; f.w = w; f.h = h;
    glGenFramebuffers(1, &f.fbo);
    glGenTextures(1, &f.tex);
    glBindTexture(GL_TEXTURE_2D, f.tex);
    glTexImage2D(GL_TEXTURE_2D, 0, internalFmt, w, h, 0, GL_RGBA, type, nullptr);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, filter);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, filter);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
    glBindFramebuffer(GL_FRAMEBUFFER, f.fbo);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0,
                           GL_TEXTURE_2D, f.tex, 0);
    glClearColor(0, 0, 0, 1); glClear(GL_COLOR_BUFFER_BIT);
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    return f;
}

FluidSource::DoubleFBO FluidSource::createDoubleFBO(int w, int h,
        GLint internalFmt, GLenum type, GLenum filter) {
    DoubleFBO d; d.w = w; d.h = h;
    d.read  = createFBO(w, h, internalFmt, type, filter);
    d.write = createFBO(w, h, internalFmt, type, filter);
    return d;
}

void FluidSource::destroyFBO(FBO& f) {
    if (f.tex) glDeleteTextures(1, &f.tex);
    if (f.fbo) glDeleteFramebuffers(1, &f.fbo);
    f = FBO{};
}

bool FluidSource::init(int outW, int outH) {
    m_outW = outW; m_outH = outH;

    m_progSplat   = compileProgram(VS_SIMPLE, FS_SPLAT);
    m_progAdvect  = compileProgram(VS_BASE,   FS_ADVECT);
    m_progDiverge = compileProgram(VS_BASE,   FS_DIVERGENCE);
    m_progCurl    = compileProgram(VS_BASE,   FS_CURL);
    m_progVort    = compileProgram(VS_BASE,   FS_VORTICITY);
    m_progPressure= compileProgram(VS_BASE,   FS_PRESSURE);
    m_progGradSub = compileProgram(VS_BASE,   FS_GRADSUB);
    m_progClear   = compileProgram(VS_SIMPLE, FS_CLEAR);
    // Post-processing chain.
    m_progBloomPrefilter = compileProgram(VS_BASE, FS_BLOOM_PREFILTER);
    m_progBloomBlur      = compileProgram(VS_BASE, FS_BLOOM_BLUR);
    m_progBloomFinal     = compileProgram(VS_BASE, FS_BLOOM_FINAL);
    m_progSunraysMask    = compileProgram(VS_SIMPLE, FS_SUNRAYS_MASK);
    m_progSunrays        = compileProgram(VS_SIMPLE, FS_SUNRAYS);
    m_progBlur           = compileProgram(VS_BLUR, FS_BLUR);
    m_progInject         = compileProgram(VS_SIMPLE, FS_INJECT);
    // Display programs compiled lazily per #define combo (displayProgram()).
    if (!m_progSplat || !m_progAdvect || !m_progDiverge || !m_progCurl ||
        !m_progVort || !m_progPressure || !m_progGradSub || !m_progClear ||
        !m_progBloomPrefilter || !m_progBloomBlur || !m_progBloomFinal ||
        !m_progSunraysMask || !m_progSunrays || !m_progBlur) {
        std::cerr << "[FluidSource] program compile failed" << std::endl;
        return false;
    }

    // Fullscreen triangle.
    float verts[] = { -1.f,-1.f,  3.f,-1.f,  -1.f,3.f };
    glGenVertexArrays(1, &m_quadVAO);
    glGenBuffers(1, &m_quadVBO);
    glBindVertexArray(m_quadVAO);
    glBindBuffer(GL_ARRAY_BUFFER, m_quadVBO);
    glBufferData(GL_ARRAY_BUFFER, sizeof(verts), verts, GL_STATIC_DRAW);
    glEnableVertexAttribArray(0);
    glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, 0, (void*)0);
    glBindVertexArray(0);

    // Sim res scaled to output aspect.
    int sw = m_simRes, sh = m_simRes;
    int dw = m_dyeRes, dh = m_dyeRes;
    if (m_outW >= m_outH) {
        sh = (int)std::round(m_simRes * (float)m_outH / m_outW);
        dh = (int)std::round(m_dyeRes * (float)m_outH / m_outW);
    } else {
        sw = (int)std::round(m_simRes * (float)m_outW / m_outH);
        dw = (int)std::round(m_dyeRes * (float)m_outW / m_outH);
    }
    sw = std::max(2, sw); sh = std::max(2, sh);
    dw = std::max(2, dw); dh = std::max(2, dh);

    // Sim fields need signed values → RGBA16F. Dye/output → RGBA8.
    m_velocity   = createDoubleFBO(sw, sh, GL_RGBA16F, GL_HALF_FLOAT, GL_LINEAR);
    m_pressure   = createDoubleFBO(sw, sh, GL_RGBA16F, GL_HALF_FLOAT, GL_LINEAR);
    m_divergence = createFBO(sw, sh, GL_RGBA16F, GL_HALF_FLOAT, GL_NEAREST);
    m_curl       = createFBO(sw, sh, GL_RGBA16F, GL_HALF_FLOAT, GL_NEAREST);
    // Native quality: float (RGBA16F) dye gives smooth HDR gradients with
    // no 8-bit banding — the browser path falls back to UNSIGNED_BYTE.
    if (m_hdrDye)
        m_dye    = createDoubleFBO(dw, dh, GL_RGBA16F, GL_HALF_FLOAT, GL_LINEAR);
    else
        m_dye    = createDoubleFBO(dw, dh, GL_RGBA8, GL_UNSIGNED_BYTE, GL_LINEAR);
    m_output     = createFBO(m_outW, m_outH, GL_RGBA8, GL_UNSIGNED_BYTE, GL_LINEAR);

    // ── Bloom: full-res target + halving downsample mip chain ─────────
    {
        int bw, bh; getResolution(m_bloomRes, bw, bh);
        m_bloomTarget = createFBO(bw, bh, GL_RGBA16F, GL_HALF_FLOAT, GL_LINEAR);
        m_bloomMips.clear();
        for (int i = 0; i < m_bloomIters; i++) {
            int w = bw >> (i + 1), h = bh >> (i + 1);
            if (w < 2 || h < 2) break;
            m_bloomMips.push_back(
                createFBO(w, h, GL_RGBA16F, GL_HALF_FLOAT, GL_LINEAR));
        }
    }
    // ── Sunrays: result + separable-blur scratch ──────────────────────
    {
        int sw2, sh2; getResolution(m_sunraysRes, sw2, sh2);
        m_sunraysFbo  = createFBO(sw2, sh2, GL_RGBA16F, GL_HALF_FLOAT, GL_LINEAR);
        m_sunraysTemp = createFBO(sw2, sh2, GL_RGBA16F, GL_HALF_FLOAT, GL_LINEAR);
    }
    createDitherTexture();

    m_lastTime = glfwGetTime();
    m_ready = true;

    // Seed with a few splats so it isn't empty on first frame.
    for (int i = 0; i < 8; i++) autoSplats(0.016f);
    std::cout << "[FluidSource] init " << sw << "x" << sh << " sim, "
              << dw << "x" << dh << " dye" << std::endl;
    return true;
}

void FluidSource::blit(const FBO& target) {
    glBindFramebuffer(GL_FRAMEBUFFER, target.fbo);
    glViewport(0, 0, target.w, target.h);
    glBindVertexArray(m_quadVAO);
    glDrawArrays(GL_TRIANGLES, 0, 3);
}

static void setTex(GLuint prog, const char* name, GLuint tex, int unit) {
    glActiveTexture(GL_TEXTURE0 + unit);
    glBindTexture(GL_TEXTURE_2D, tex);
    glUniform1i(glGetUniformLocation(prog, name), unit);
}

void FluidSource::step(float dt) {
    glDisable(GL_BLEND);
    if (dt > 0.016667f) dt = 0.016667f;

    float simTexel[2] = { 1.0f / m_velocity.w, 1.0f / m_velocity.h };
    float dyeTexel[2] = { 1.0f / m_dye.w, 1.0f / m_dye.h };

    // 1. Curl
    glUseProgram(m_progCurl);
    glUniform2fv(glGetUniformLocation(m_progCurl, "texelSize"), 1, simTexel);
    setTex(m_progCurl, "uVelocity", m_velocity.read.tex, 0);
    blit(m_curl);

    // 2. Vorticity
    glUseProgram(m_progVort);
    glUniform2fv(glGetUniformLocation(m_progVort, "texelSize"), 1, simTexel);
    setTex(m_progVort, "uVelocity", m_velocity.read.tex, 0);
    setTex(m_progVort, "uCurl", m_curl.tex, 1);
    glUniform1f(glGetUniformLocation(m_progVort, "curl"), m_curlAmount);
    glUniform1f(glGetUniformLocation(m_progVort, "dt"), dt);
    blit(m_velocity.write); m_velocity.swap();

    // 3. Divergence
    glUseProgram(m_progDiverge);
    glUniform2fv(glGetUniformLocation(m_progDiverge, "texelSize"), 1, simTexel);
    setTex(m_progDiverge, "uVelocity", m_velocity.read.tex, 0);
    blit(m_divergence);

    // 4. Clear pressure
    glUseProgram(m_progClear);
    setTex(m_progClear, "uTexture", m_pressure.read.tex, 0);
    glUniform1f(glGetUniformLocation(m_progClear, "value"), m_pressureValue);
    blit(m_pressure.write); m_pressure.swap();

    // 5. Pressure Jacobi
    glUseProgram(m_progPressure);
    glUniform2fv(glGetUniformLocation(m_progPressure, "texelSize"), 1, simTexel);
    setTex(m_progPressure, "uDivergence", m_divergence.tex, 1);
    for (int i = 0; i < m_pressureIters; i++) {
        setTex(m_progPressure, "uPressure", m_pressure.read.tex, 0);
        blit(m_pressure.write); m_pressure.swap();
    }

    // 6. Gradient subtract
    glUseProgram(m_progGradSub);
    glUniform2fv(glGetUniformLocation(m_progGradSub, "texelSize"), 1, simTexel);
    setTex(m_progGradSub, "uPressure", m_pressure.read.tex, 0);
    setTex(m_progGradSub, "uVelocity", m_velocity.read.tex, 1);
    blit(m_velocity.write); m_velocity.swap();

    // 7. Advect velocity
    glUseProgram(m_progAdvect);
    glUniform2fv(glGetUniformLocation(m_progAdvect, "texelSize"), 1, simTexel);
    setTex(m_progAdvect, "uVelocity", m_velocity.read.tex, 0);
    setTex(m_progAdvect, "uSource", m_velocity.read.tex, 1);
    glUniform1f(glGetUniformLocation(m_progAdvect, "dt"), dt);
    glUniform1f(glGetUniformLocation(m_progAdvect, "dissipation"), m_velocityDissipation);
    blit(m_velocity.write); m_velocity.swap();

    // 8. Advect dye
    glUseProgram(m_progAdvect);
    glUniform2fv(glGetUniformLocation(m_progAdvect, "texelSize"), 1, dyeTexel);
    setTex(m_progAdvect, "uVelocity", m_velocity.read.tex, 0);
    setTex(m_progAdvect, "uSource", m_dye.read.tex, 1);
    glUniform1f(glGetUniformLocation(m_progAdvect, "dt"), dt);
    glUniform1f(glGetUniformLocation(m_progAdvect, "dissipation"), m_densityDissipation);
    blit(m_dye.write); m_dye.swap();
}

void FluidSource::splat(float x, float y, float dx, float dy,
                        float r, float g, float b) {
    if (!m_ready) return;
    float aspect = (float)m_outW / m_outH;
    float radius = m_splatRadius / 100.0f;
    if (aspect > 1.0f) radius *= aspect;

    glDisable(GL_BLEND);
    // velocity
    glUseProgram(m_progSplat);
    setTex(m_progSplat, "uTarget", m_velocity.read.tex, 0);
    glUniform1f(glGetUniformLocation(m_progSplat, "aspectRatio"), aspect);
    glUniform2f(glGetUniformLocation(m_progSplat, "point"), x, y);
    glUniform1f(glGetUniformLocation(m_progSplat, "radius"), radius);
    glUniform3f(glGetUniformLocation(m_progSplat, "color"), dx, dy, 0.0f);
    blit(m_velocity.write); m_velocity.swap();
    // dye (pure additive — Pavel's canonical splat)
    setTex(m_progSplat, "uTarget", m_dye.read.tex, 0);
    glUniform3f(glGetUniformLocation(m_progSplat, "color"), r, g, b);
    blit(m_dye.write); m_dye.swap();
}

// Inject an external texture into the dye field. Sized to the dye FBO so
// the resolution is independent of the source image's pixel grid (texture
// filtering interpolates). Caller guards the m_image.textureId != 0 case.
void FluidSource::applyImageInject() {
    if (!m_ready || m_image.textureId == 0 || m_imageIntensity <= 0.0f) return;
    glDisable(GL_BLEND);
    glUseProgram(m_progInject);
    setTex(m_progInject, "uTarget", m_dye.read.tex,       0);
    setTex(m_progInject, "uImage",  m_image.textureId,    1);
    glUniform1f(glGetUniformLocation(m_progInject, "uIntensity"),
                m_imageIntensity);
    glUniform1f(glGetUniformLocation(m_progInject, "uFlipY"),
                m_image.flippedV ? 1.0f : 0.0f);
    blit(m_dye.write); m_dye.swap();
    // Leave texture unit 0 active so callers don't see a leaked unit 1 binding.
    glActiveTexture(GL_TEXTURE0);
}

void FluidSource::hsv(float h, float s, float v, float& r, float& g, float& b) {
    float i = std::floor(h * 6.0f);
    float f = h * 6.0f - i;
    float p = v * (1.0f - s);
    float q = v * (1.0f - f * s);
    float t = v * (1.0f - (1.0f - f) * s);
    switch ((int)i % 6) {
        case 0: r=v; g=t; b=p; break;
        case 1: r=q; g=v; b=p; break;
        case 2: r=p; g=v; b=t; break;
        case 3: r=p; g=q; b=v; break;
        case 4: r=t; g=p; b=v; break;
        default:r=v; g=p; b=q; break;
    }
}

void FluidSource::autoSplats(float dt) {
    // Continuous motion-driven splatting — a virtual cursor traces one of
    // several selectable patterns (m_autoPattern) and lays down dye along
    // its path. Mirrors the JS freeform default (Wander) plus orbit / figure-
    // 8 / spiral path shapes and pulse / rain emitter shapes. This is what
    // makes the canvas read as dense flowing fluid rather than blinking dots.
    if (!m_autoMovement) return;
    std::uniform_real_distribution<float> u(0.0f, 1.0f);
    m_autoPhase += dt * std::max(0.0f, m_autoSpeed);

    const float aspect = (m_outH > 0) ? (float)m_outW / (float)m_outH : 1.0f;
    const float kForce = 6000.0f;   // SPLAT_FORCE-ish base velocity

    // Wander keeps its legacy steering integrator (re-aimed cursor, edge
    // bounce). The other patterns are evaluated per-splat below.
    if (m_autoPattern == Wander) {
        m_autoTimer += dt;
        if (m_autoTimer > 1.1f) {
            m_autoTimer = 0.0f;
            float tx = 0.15f + 0.7f * u(m_rng);
            float ty = 0.15f + 0.7f * u(m_rng);
            m_cvx = (tx - m_cx) * 1.6f * std::max(0.05f, m_autoSpeed);
            m_cvy = (ty - m_cy) * 1.6f * std::max(0.05f, m_autoSpeed);
        }
        m_cx += m_cvx * dt;
        m_cy += m_cvy * dt;
        if (m_cx < 0.05f || m_cx > 0.95f) { m_cvx = -m_cvx; m_cx = std::min(0.95f, std::max(0.05f, m_cx)); }
        if (m_cy < 0.05f || m_cy > 0.95f) { m_cvy = -m_cvy; m_cy = std::min(0.95f, std::max(0.05f, m_cy)); }
    }

    // Emit roughly m_autoRate splats/sec, distributed across this frame.
    float interval = (m_autoRate > 0.1f) ? (1.0f / m_autoRate) : 1e9f;
    m_splatAccum += dt;
    int guard = 0;
    while (m_splatAccum >= interval && guard++ < 8) {
        m_splatAccum -= interval;
        m_hue = std::fmod(m_hue + 0.013f, 1.0f);
        float r, g, b; hsv(m_hue, 1.0f, 1.0f, r, g, b);
        r *= m_splatIntensity; g *= m_splatIntensity; b *= m_splatIntensity;

        // Position (uv [0,1]) + velocity for this splat, per pattern. The /
        // aspect on x keeps circular paths visually round on a wide canvas.
        float px, py, dx, dy;
        switch (m_autoPattern) {
            case Orbit: {
                float a = m_autoPhase * 2.0f;
                px = 0.5f + (m_autoScale / aspect) * std::cos(a);
                py = 0.5f +  m_autoScale          * std::sin(a);
                dx = -std::sin(a) * kForce * m_autoSpeed;
                dy =  std::cos(a) * kForce * m_autoSpeed;
                break;
            }
            case Figure8: {
                float a = m_autoPhase * 2.0f;
                px = 0.5f + (m_autoScale / aspect) * std::sin(a);
                py = 0.5f +  m_autoScale * 0.6f    * std::sin(2.0f * a);
                dx = std::cos(a)        * kForce * m_autoSpeed;
                dy = std::cos(2.0f * a) * kForce * m_autoSpeed * 1.2f;
                break;
            }
            case Spiral: {
                float a   = m_autoPhase * 3.0f;
                float rad = m_autoScale * (0.25f + 0.75f *
                            (0.5f + 0.5f * std::sin(m_autoPhase * 0.5f)));
                px = 0.5f + (rad / aspect) * std::cos(a);
                py = 0.5f +  rad           * std::sin(a);
                dx = -std::sin(a) * kForce * m_autoSpeed;
                dy =  std::cos(a) * kForce * m_autoSpeed;
                break;
            }
            case Pulse: {
                // Center fountain — each splat fires outward at a rotating
                // angle, so dye erupts radially from the middle.
                float a = m_autoPhase * 3.0f + u(m_rng) * 0.6f;
                px = 0.5f; py = 0.5f;
                float mag = kForce * (0.5f + m_autoScale * 2.0f);
                dx = std::cos(a) * mag;
                dy = std::sin(a) * mag;
                break;
            }
            case Rain: {
                // Streams entering the top edge, driven downward.
                px = u(m_rng);
                py = 0.96f;
                dx = (u(m_rng) - 0.5f) * 600.0f;
                dy = -kForce * (0.5f + m_autoScale * 2.0f);
                break;
            }
            default: { // Wander
                float jx = (u(m_rng) - 0.5f) * 0.04f;
                float jy = (u(m_rng) - 0.5f) * 0.04f;
                px = m_cx + jx; py = m_cy + jy;
                dx = m_cvx * kForce + (u(m_rng) - 0.5f) * 300.0f;
                dy = m_cvy * kForce + (u(m_rng) - 0.5f) * 300.0f;
                break;
            }
        }
        splat(px, py, dx, dy, r, g, b);
    }
}

void FluidSource::queuePointerSplat(float x, float y, float dx, float dy) {
    if (!m_ready) return;
    m_pointerSplats.push_back({x, y, dx, dy});
}

void FluidSource::applyAudioBindings(float level, float bass, float mid,
                                     float high, float beat, float dt,
                                     MIDIManager* midi,
                                     float energy, float build, float drop,
                                     float silence, float momentum) {
    if (m_audioBindings.empty()) return;
    for (auto& [name, b] : m_audioBindings) {
        if (b.signal == AudioSignal::None) continue;

        float raw = 0.0f; bool have = true;
        switch (b.signal) {
            case AudioSignal::Level: raw = level; break;
            case AudioSignal::Bass:  raw = bass;  break;
            case AudioSignal::Mid:   raw = mid;   break;
            case AudioSignal::High:  raw = high;  break;
            case AudioSignal::Beat:  raw = beat;  break;
            case AudioSignal::Energy:   raw = energy;   break;
            case AudioSignal::Build:    raw = build;    break;
            case AudioSignal::Drop:     raw = drop;     break;
            case AudioSignal::Silence:  raw = silence;  break;
            case AudioSignal::Momentum: raw = momentum; break;
            case AudioSignal::MidiCC: {
                if (midi && b.midiCC >= 0) {
                    float v = midi->getCCValue(b.midiChannel, b.midiCC);
                    if (v < 0.0f) have = false; else raw = v;
                } else have = false;
                break;
            }
            default: have = false; break;
        }
        if (!have) continue;

        float mapped = b.follow(raw, dt);
        // param id → member pointer + clamp range (mirrors the PropertyPanel
        // slider ranges so a bound value can't exceed the manual slider).
        float* dst = nullptr; float lo = 0.0f, hi = 1.0f;
        if      (name == "curl")                { dst = &m_curlAmount;          lo = 0.0f;  hi = 60.0f; }
        else if (name == "densityDissipation")  { dst = &m_densityDissipation;  lo = 0.0f;  hi = 4.0f;  }
        else if (name == "velocityDissipation") { dst = &m_velocityDissipation; lo = 0.0f;  hi = 4.0f;  }
        else if (name == "pressure")            { dst = &m_pressureValue;       lo = 0.0f;  hi = 1.0f;  }
        else if (name == "splatRadius")         { dst = &m_splatRadius;         lo = 0.05f; hi = 1.5f;  }
        else if (name == "splatIntensity")      { dst = &m_splatIntensity;      lo = 0.1f;  hi = 4.0f;  }
        else if (name == "autoRate")            { dst = &m_autoRate;            lo = 0.0f;  hi = 60.0f; }
        else if (name == "autoSpeed")           { dst = &m_autoSpeed;           lo = 0.0f;  hi = 4.0f;  }
        else if (name == "autoScale")           { dst = &m_autoScale;           lo = 0.0f;  hi = 0.5f;  }
        else if (name == "bloomIntensity")      { dst = &m_bloomIntensity;      lo = 0.0f;  hi = 2.0f;  }
        else if (name == "bloomThreshold")      { dst = &m_bloomThreshold;      lo = 0.0f;  hi = 1.5f;  }
        else if (name == "sunraysWeight")       { dst = &m_sunraysWeight;       lo = 0.0f;  hi = 2.0f;  }
        else if (name == "imageIntensity")      { dst = &m_imageIntensity;      lo = 0.0f;  hi = 1.0f;  }
        if (dst) { if (mapped < lo) mapped = lo; if (mapped > hi) mapped = hi; *dst = mapped; }
    }
}

// Pavel's applyBloom(): prefilter → downsample mip chain → additive
// up-sample/accumulate → final blur×intensity, all into m_bloomTarget.
void FluidSource::applyBloom(const FBO& source) {
    if (m_bloomMips.size() < 2) return;
    const FBO* last = &m_bloomTarget;

    glDisable(GL_BLEND);
    // Prefilter: threshold + soft-knee curve.
    glUseProgram(m_progBloomPrefilter);
    float knee = m_bloomThreshold * m_bloomSoftKnee + 0.0001f;
    float c0 = m_bloomThreshold - knee, c1 = knee * 2.0f, c2 = 0.25f / knee;
    glUniform3f(glGetUniformLocation(m_progBloomPrefilter, "curve"), c0, c1, c2);
    glUniform1f(glGetUniformLocation(m_progBloomPrefilter, "threshold"),
                m_bloomThreshold);
    setTex(m_progBloomPrefilter, "uTexture", source.tex, 0);
    blit(*last);

    // Downsample chain.
    glUseProgram(m_progBloomBlur);
    for (size_t i = 0; i < m_bloomMips.size(); i++) {
        const FBO& dest = m_bloomMips[i];
        float ts[2] = { 1.0f / last->w, 1.0f / last->h };
        glUniform2fv(glGetUniformLocation(m_progBloomBlur, "texelSize"), 1, ts);
        setTex(m_progBloomBlur, "uTexture", last->tex, 0);
        blit(dest);
        last = &dest;
    }

    // Up-sample, additively accumulating onto each coarser-down level.
    glBlendFunc(GL_ONE, GL_ONE);
    glEnable(GL_BLEND);
    for (int i = (int)m_bloomMips.size() - 2; i >= 0; i--) {
        const FBO& base = m_bloomMips[i];
        float ts[2] = { 1.0f / last->w, 1.0f / last->h };
        glUniform2fv(glGetUniformLocation(m_progBloomBlur, "texelSize"), 1, ts);
        setTex(m_progBloomBlur, "uTexture", last->tex, 0);
        blit(base);
        last = &base;
    }
    glDisable(GL_BLEND);

    // Final blur × intensity back into the full-res bloom target.
    glUseProgram(m_progBloomFinal);
    float ts[2] = { 1.0f / last->w, 1.0f / last->h };
    glUniform2fv(glGetUniformLocation(m_progBloomFinal, "texelSize"), 1, ts);
    glUniform1f(glGetUniformLocation(m_progBloomFinal, "intensity"),
                m_bloomIntensity);
    setTex(m_progBloomFinal, "uTexture", last->tex, 0);
    blit(m_bloomTarget);
}

// Pavel's applySunrays(): bright-dye occlusion mask → radial light shafts.
// Reuses m_dye.write as the mask scratch (advect overwrites it next frame).
void FluidSource::applySunrays(const FBO& source) {
    glDisable(GL_BLEND);
    glUseProgram(m_progSunraysMask);
    setTex(m_progSunraysMask, "uTexture", source.tex, 0);
    blit(m_dye.write);

    glUseProgram(m_progSunrays);
    glUniform1f(glGetUniformLocation(m_progSunrays, "weight"), m_sunraysWeight);
    setTex(m_progSunrays, "uTexture", m_dye.write.tex, 0);
    blit(m_sunraysFbo);
}

// Separable 1.33333-tap Gaussian, ping-ponging target↔temp.
void FluidSource::blurFBO(FBO& target, FBO& temp, int iterations) {
    glDisable(GL_BLEND);
    glUseProgram(m_progBlur);
    for (int i = 0; i < iterations; i++) {
        float h[2] = { 1.0f / target.w, 0.0f };
        glUniform2fv(glGetUniformLocation(m_progBlur, "texelSize"), 1, h);
        setTex(m_progBlur, "uTexture", target.tex, 0);
        blit(temp);
        float v[2] = { 0.0f, 1.0f / target.h };
        glUniform2fv(glGetUniformLocation(m_progBlur, "texelSize"), 1, v);
        setTex(m_progBlur, "uTexture", temp.tex, 0);
        blit(target);
    }
}

void FluidSource::renderToOutput() {
    glDisable(GL_BLEND);

    if (m_bloom)   applyBloom(m_dye.read);
    if (m_sunrays) { applySunrays(m_dye.read); blurFBO(m_sunraysFbo, m_sunraysTemp, 1); }

    // Display program for the active SHADING/BLOOM/SUNRAYS combination.
    GLuint prog = displayProgram(m_shading, m_bloom, m_sunrays);
    glUseProgram(prog);
    // SHADING samples the dye at ±1 *output* texel (Pavel uses target size).
    float texel[2] = { 1.0f / (float)m_outW, 1.0f / (float)m_outH };
    glUniform2fv(glGetUniformLocation(prog, "texelSize"), 1, texel);
    setTex(prog, "uTexture", m_dye.read.tex, 0);
    if (m_bloom) {
        setTex(prog, "uBloom", m_bloomTarget.tex, 1);
        setTex(prog, "uDithering", m_ditherTex, 2);
        float ds[2] = { (float)m_outW / (float)m_ditherW,
                        (float)m_outH / (float)m_ditherH };
        glUniform2fv(glGetUniformLocation(prog, "ditherScale"), 1, ds);
    }
    if (m_sunrays)
        setTex(prog, "uSunrays", m_sunraysFbo.tex, 3);
    blit(m_output);
}

void FluidSource::update() {
    if (!m_ready) return;
    // Save GL state the rest of Easel relies on, since we rebind FBO/VAO.
    GLint prevFBO = 0; glGetIntegerv(GL_FRAMEBUFFER_BINDING, &prevFBO);
    GLint prevVP[4]; glGetIntegerv(GL_VIEWPORT, prevVP);
    GLboolean prevBlend = glIsEnabled(GL_BLEND);

    double now = glfwGetTime();
    float dt = (float)(now - m_lastTime);
    m_lastTime = now;
    if (dt <= 0.0f) dt = 0.016f;

    autoSplats(dt);
    // Drain queued interactive pointer splats (mouse / vision hand). Done
    // here, inside the GL-state-saved region, with a cycling dye color.
    if (!m_pointerSplats.empty()) {
        for (const auto& p : m_pointerSplats) {
            m_hue = std::fmod(m_hue + 0.0021f, 1.0f);
            float r, g, b; hsv(m_hue, 1.0f, 1.0f, r, g, b);
            float k = m_splatIntensity;
            splat(p.x, p.y, p.dx, p.dy, r * k, g * k, b * k);
        }
        m_pointerSplats.clear();
    }
    // Optional image inject — adds the bound texture to the dye field
    // before the sim step, so this frame's advect+dissipate already
    // begins smearing it through the velocity field.
    if (m_imageEnabled) applyImageInject();
    step(dt);
    renderToOutput();

    glBindFramebuffer(GL_FRAMEBUFFER, prevFBO);
    glViewport(prevVP[0], prevVP[1], prevVP[2], prevVP[3]);
    glBindVertexArray(0);
    glUseProgram(0);
    glActiveTexture(GL_TEXTURE0);      // step() left it on a higher unit
    if (prevBlend) glEnable(GL_BLEND); // restore compositor's blend state
    else           glDisable(GL_BLEND);
}

FluidSource::~FluidSource() {
    if (m_quadVBO) glDeleteBuffers(1, &m_quadVBO);
    if (m_quadVAO) glDeleteVertexArrays(1, &m_quadVAO);
    destroyFBO(m_velocity.read);  destroyFBO(m_velocity.write);
    destroyFBO(m_dye.read);       destroyFBO(m_dye.write);
    destroyFBO(m_pressure.read);  destroyFBO(m_pressure.write);
    destroyFBO(m_divergence);     destroyFBO(m_curl);
    destroyFBO(m_output);
    destroyFBO(m_bloomTarget);
    for (FBO& f : m_bloomMips) destroyFBO(f);
    destroyFBO(m_sunraysFbo);      destroyFBO(m_sunraysTemp);
    if (m_ditherTex) glDeleteTextures(1, &m_ditherTex);
    GLuint progs[] = { m_progSplat, m_progAdvect, m_progDiverge, m_progCurl,
                       m_progVort, m_progPressure, m_progGradSub, m_progClear,
                       m_progBloomPrefilter, m_progBloomBlur, m_progBloomFinal,
                       m_progSunraysMask, m_progSunrays, m_progBlur };
    for (GLuint p : progs) if (p) glDeleteProgram(p);
    for (GLuint p : m_displayPrograms) if (p) glDeleteProgram(p);
}
