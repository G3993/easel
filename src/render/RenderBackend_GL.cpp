// RenderBackend_GL.cpp — Phase M.0 OpenGL stub.
//
// This is the seam — a thin wrapper around the GL primitives that
// already exist in Texture.cpp / Framebuffer.cpp / ShaderProgram.cpp.
// In M.0 we satisfy the RenderBackend interface enough that the
// abstraction compiles and the GL backend reports the correct API +
// caps. Real wrapping (turning every render call into a backend
// dispatch) is a Phase M.1 / Phase M.5 concern.

#include "render/RenderBackend.h"
#include <glad/glad.h>
#include <unordered_map>
#include <string>

namespace easel {

namespace {

class GLBackend : public RenderBackend {
public:
    GLBackend() {
        m_caps.supportsCompute      = false;   // GL 4.1 ceiling on macOS
        m_caps.supportsRaytracing   = false;
        m_caps.supportsMeshShaders  = false;
        m_caps.supportsHalfFloatRT  = true;
        m_caps.supportsAsyncCompute = false;
        // Query real values lazily; safe defaults work for now.
    }
    ~GLBackend() override = default;

    GraphicsAPI api() const override { return GraphicsAPI::OpenGL; }
    const BackendCaps& caps() const override { return m_caps; }
    std::string name() const override {
        // Avoid glGetString here — Phase M.0 backends may be constructed
        // before a real GL context is current (test harness, capability
        // probing). The real version string is queried on first
        // beginFrame() if needed.
        return std::string("OpenGL 4.1 Core (Easel backend)");
    }

    void beginFrame(int w, int h) override {
        glViewport(0, 0, w, h);
    }
    void endFrame() override {}

    // Phase M.0: handles are GL object IDs cast to BackendHandle. The
    // tracking maps below let us swap to ref-counted wrappers later
    // without touching call-sites.
    BackendHandle createTexture(int w, int h, TextureFormat fmt) override {
        GLuint id = 0;
        glGenTextures(1, &id);
        glBindTexture(GL_TEXTURE_2D, id);
        GLint internal = GL_RGBA8;
        GLenum format  = GL_RGBA;
        GLenum type    = GL_UNSIGNED_BYTE;
        switch (fmt) {
            case TextureFormat::RGBA16F: internal = GL_RGBA16F; type = GL_HALF_FLOAT;       break;
            case TextureFormat::RGBA32F: internal = GL_RGBA32F; type = GL_FLOAT;            break;
            case TextureFormat::R8:      internal = GL_R8;      format = GL_RED;            break;
            case TextureFormat::R16F:    internal = GL_R16F;    format = GL_RED;
                                          type = GL_HALF_FLOAT;                              break;
            case TextureFormat::Depth32F:internal = GL_DEPTH_COMPONENT32F;
                                          format = GL_DEPTH_COMPONENT;
                                          type = GL_FLOAT;                                   break;
            case TextureFormat::RGBA8:   default: break;
        }
        glTexImage2D(GL_TEXTURE_2D, 0, internal, w, h, 0, format, type, nullptr);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S,     GL_CLAMP_TO_EDGE);
        glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T,     GL_CLAMP_TO_EDGE);
        glBindTexture(GL_TEXTURE_2D, 0);
        return static_cast<BackendHandle>(id);
    }

    void uploadTexture(BackendHandle tex, const void* pixels,
                       int w, int h, TextureFormat /*srcFmt*/) override {
        if (tex == kInvalidHandle || !pixels) return;
        glBindTexture(GL_TEXTURE_2D, static_cast<GLuint>(tex));
        glTexSubImage2D(GL_TEXTURE_2D, 0, 0, 0, w, h, GL_RGBA, GL_UNSIGNED_BYTE, pixels);
        glBindTexture(GL_TEXTURE_2D, 0);
    }

    void destroyTexture(BackendHandle tex) override {
        if (tex == kInvalidHandle) return;
        GLuint id = static_cast<GLuint>(tex);
        glDeleteTextures(1, &id);
    }

    BackendHandle createRenderTarget(BackendHandle colorTex, BackendHandle /*depthTex*/) override {
        if (colorTex == kInvalidHandle) return kInvalidHandle;
        GLuint fbo = 0;
        glGenFramebuffers(1, &fbo);
        glBindFramebuffer(GL_FRAMEBUFFER, fbo);
        glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0,
                               GL_TEXTURE_2D, static_cast<GLuint>(colorTex), 0);
        glBindFramebuffer(GL_FRAMEBUFFER, 0);
        return static_cast<BackendHandle>(fbo);
    }
    void destroyRenderTarget(BackendHandle rt) override {
        if (rt == kInvalidHandle) return;
        GLuint id = static_cast<GLuint>(rt);
        glDeleteFramebuffers(1, &id);
    }
    void bindRenderTarget(BackendHandle rt) override {
        glBindFramebuffer(GL_FRAMEBUFFER, static_cast<GLuint>(rt));
    }
    void clearRenderTarget(float r, float g, float b, float a, bool clearDepth) override {
        glClearColor(r, g, b, a);
        glClear(GL_COLOR_BUFFER_BIT | (clearDepth ? GL_DEPTH_BUFFER_BIT : 0));
    }

    BackendHandle createShader(const std::string& vert, const std::string& frag) override {
        auto compile = [](GLenum type, const std::string& src) -> GLuint {
            GLuint sh = glCreateShader(type);
            const char* p = src.c_str();
            glShaderSource(sh, 1, &p, nullptr);
            glCompileShader(sh);
            GLint ok = GL_FALSE;
            glGetShaderiv(sh, GL_COMPILE_STATUS, &ok);
            if (!ok) { glDeleteShader(sh); return 0; }
            return sh;
        };
        GLuint v = compile(GL_VERTEX_SHADER,   vert);
        GLuint f = compile(GL_FRAGMENT_SHADER, frag);
        if (!v || !f) {
            if (v) glDeleteShader(v);
            if (f) glDeleteShader(f);
            return kInvalidHandle;
        }
        GLuint prog = glCreateProgram();
        glAttachShader(prog, v);
        glAttachShader(prog, f);
        glLinkProgram(prog);
        glDeleteShader(v);
        glDeleteShader(f);
        GLint ok = GL_FALSE;
        glGetProgramiv(prog, GL_LINK_STATUS, &ok);
        if (!ok) { glDeleteProgram(prog); return kInvalidHandle; }
        return static_cast<BackendHandle>(prog);
    }
    void destroyShader(BackendHandle shader) override {
        if (shader == kInvalidHandle) return;
        glDeleteProgram(static_cast<GLuint>(shader));
    }
    void bindShader(BackendHandle shader) override {
        glUseProgram(static_cast<GLuint>(shader));
    }

    GLint loc(BackendHandle shader, const char* name) {
        return glGetUniformLocation(static_cast<GLuint>(shader), name);
    }

    void setUniformFloat (BackendHandle s, const char* n, float v) override {
        glUniform1f(loc(s, n), v);
    }
    void setUniformVec2  (BackendHandle s, const char* n, float x, float y) override {
        glUniform2f(loc(s, n), x, y);
    }
    void setUniformVec3  (BackendHandle s, const char* n, float x, float y, float z) override {
        glUniform3f(loc(s, n), x, y, z);
    }
    void setUniformVec4  (BackendHandle s, const char* n, float x, float y, float z, float w) override {
        glUniform4f(loc(s, n), x, y, z, w);
    }
    void setUniformMat4  (BackendHandle s, const char* n, const float* m) override {
        glUniformMatrix4fv(loc(s, n), 1, GL_FALSE, m);
    }
    void setUniformInt   (BackendHandle s, const char* n, int v) override {
        glUniform1i(loc(s, n), v);
    }
    void setUniformTexture(BackendHandle s, const char* n, int slot, BackendHandle tex) override {
        glActiveTexture(GL_TEXTURE0 + slot);
        glBindTexture(GL_TEXTURE_2D, static_cast<GLuint>(tex));
        glUniform1i(loc(s, n), slot);
    }

    void drawFullscreenQuad() override {
        // No VAO bind here — assumes caller has a fullscreen quad set up.
        // Phase M.5 will move quad ownership into the backend.
        glDrawArrays(GL_TRIANGLE_STRIP, 0, 4);
    }

private:
    BackendCaps m_caps;
};

} // namespace

// ── Factory implementations live next to the GL backend so they can
//    construct it without exposing GLBackend's class. The Metal entry
//    point is in RenderBackend_Metal.mm (Apple-only TU).

std::unique_ptr<RenderBackend> createGLBackend() {
    return std::make_unique<GLBackend>();
}

#if !defined(__APPLE__)
// On non-Apple platforms there is no Metal stub TU; provide the symbol
// here so the factory below links.
std::unique_ptr<RenderBackend> createMetalBackend() { return nullptr; }
bool metalIsAvailable() { return false; }
#endif

// Forward decls — defined in this TU on non-Apple, in
// RenderBackend_Metal.mm on Apple.
extern std::unique_ptr<RenderBackend> createMetalBackend();
extern bool metalIsAvailable();

std::unique_ptr<RenderBackend> RenderBackend::create(GraphicsAPI api) {
    if (api == GraphicsAPI::Metal) {
        auto m = createMetalBackend();
        if (m) return m;
        // Phase M.0/M.1 fallback: Metal not yet wired → fall through to GL.
        return createGLBackend();
    }
    return createGLBackend();
}

bool RenderBackend::available(GraphicsAPI api) {
    if (api == GraphicsAPI::Metal) return metalIsAvailable();
    return true;  // GL is always assumed available at this point in the boot
}

} // namespace easel
