#pragma once
//
// RenderBackend — Phase M.0 abstraction layer.
//
// The long-term plan is to ship a Metal-API renderer alongside the
// existing OpenGL one so we can stop hitting the GL 4.1 ceiling on
// macOS (no compute shaders, no real ray tracing, no proper async
// compute). This header is the seam: every render-side caller goes
// through this interface; backends implement it.
//
// Phase M.0 (this commit) — interface only. The GL backend is a thin
// wrapper around Texture/Framebuffer/ShaderProgram that already exist.
// The Metal backend is a stub returning false from `available()` until
// Phase M.1 ships SPIRV-Cross → MSL transpilation.
//
// Phase ladder:
//   M.0 — abstraction (this header) + GL wrapper that compiles
//   M.1 — Metal renderer for ShaderSource via SPIRV-Cross
//   M.2 — Metal compute shaders (separable Gaussian, parallel reductions)
//   M.3 — Metal ray-tracing via MTLAccelerationStructure
//   M.4 — Metal mesh shaders
//
// Don't refactor existing GL call sites yet — they still call Texture /
// Framebuffer / ShaderProgram directly. Only NEW renderers that need
// Metal-portable code should target this interface.

#include <cstdint>
#include <memory>
#include <string>

namespace easel {

enum class GraphicsAPI {
    OpenGL,
    Metal
};

enum class TextureFormat {
    RGBA8,
    RGBA16F,
    RGBA32F,
    R8,
    R16F,
    Depth32F
};

// Opaque handles. Backends interpret these however they like; callers
// pass them around as cookies. 0 / kInvalidHandle = null/empty.
using BackendHandle = uint64_t;
constexpr BackendHandle kInvalidHandle = 0;

struct BackendCaps {
    bool supportsCompute        = false;
    bool supportsRaytracing     = false;
    bool supportsMeshShaders    = false;
    bool supportsHalfFloatRT    = true;   // GL 4.1 has this via ARB_color_buffer_float
    bool supportsAsyncCompute   = false;
    int  maxColorAttachments    = 8;      // GL min, Metal supports 8 too
    int  maxTextureSize         = 16384;
};

class RenderBackend {
public:
    virtual ~RenderBackend() = default;

    // ── Backend identity ───────────────────────────────────────────
    virtual GraphicsAPI api() const = 0;
    virtual const BackendCaps& caps() const = 0;
    virtual std::string name() const = 0;          // e.g. "OpenGL 4.1 Core" / "Metal 3.2"

    // ── Frame lifecycle ────────────────────────────────────────────
    // Wraps glViewport / present-drawable equivalents. Drivers may or
    // may not need an explicit beginFrame; backends decide.
    virtual void beginFrame(int viewportW, int viewportH) = 0;
    virtual void endFrame() = 0;

    // ── Texture API ────────────────────────────────────────────────
    virtual BackendHandle createTexture(int w, int h, TextureFormat fmt) = 0;
    virtual void uploadTexture(BackendHandle tex,
                               const void* pixels,
                               int w, int h,
                               TextureFormat srcFmt) = 0;
    virtual void destroyTexture(BackendHandle tex) = 0;

    // ── Render target API ─────────────────────────────────────────
    virtual BackendHandle createRenderTarget(BackendHandle colorTex,
                                             BackendHandle depthTex /*may be 0*/) = 0;
    virtual void destroyRenderTarget(BackendHandle rt) = 0;
    virtual void bindRenderTarget(BackendHandle rt /*0 = default framebuffer*/) = 0;
    virtual void clearRenderTarget(float r, float g, float b, float a,
                                   bool clearDepth = true) = 0;

    // ── Shader API ─────────────────────────────────────────────────
    // For Metal these are MSL strings (or SPIRV-Cross-transpiled). For
    // GL they're GLSL. Vertex/fragment pair.
    virtual BackendHandle createShader(const std::string& vertSrc,
                                       const std::string& fragSrc) = 0;
    virtual void destroyShader(BackendHandle shader) = 0;
    virtual void bindShader(BackendHandle shader) = 0;

    virtual void setUniformFloat (BackendHandle shader, const char* name, float v) = 0;
    virtual void setUniformVec2  (BackendHandle shader, const char* name, float x, float y) = 0;
    virtual void setUniformVec3  (BackendHandle shader, const char* name, float x, float y, float z) = 0;
    virtual void setUniformVec4  (BackendHandle shader, const char* name, float x, float y, float z, float w) = 0;
    virtual void setUniformMat4  (BackendHandle shader, const char* name, const float* mat16) = 0;
    virtual void setUniformInt   (BackendHandle shader, const char* name, int v) = 0;
    virtual void setUniformTexture(BackendHandle shader, const char* name, int slot, BackendHandle tex) = 0;

    // ── Draw ───────────────────────────────────────────────────────
    virtual void drawFullscreenQuad() = 0;

    // ── Phase M.2 — compute shaders ────────────────────────────────
    // Compile an MSL compute kernel (entry point `computeMain`). Returns
    // kInvalidHandle if compilation failed or the backend doesn't
    // support compute (GL 4.1 returns kInvalidHandle for everything).
    virtual BackendHandle createComputeShader(const std::string& /*mslSrc*/) { return kInvalidHandle; }
    virtual void destroyComputeShader(BackendHandle /*shader*/) {}

    // Dispatch the compute kernel with `groupsX*Y*Z` threadgroups,
    // each containing the kernel's declared threadgroup size. Returns
    // false if the backend can't dispatch (no compute support, no
    // bound shader, etc.).
    virtual bool dispatchCompute(BackendHandle /*computeShader*/,
                                 int /*groupsX*/, int /*groupsY*/, int /*groupsZ*/) {
        return false;
    }

    // Bind a texture for read or write to the compute kernel's
    // `texture(slot)` argument table. RW textures must be created
    // with shader-write usage (the Metal backend already does this).
    virtual void setComputeTexture(BackendHandle /*shader*/, int /*slot*/, BackendHandle /*tex*/) {}

    // ── Phase M.3 — ray tracing ────────────────────────────────────
    // Build an acceleration structure from a vertex buffer (interleaved
    // float3 positions, `vertCount` entries) plus an index buffer
    // (uint32, `indexCount` entries — typically 3 × triangle count).
    // Pass nullptr/0 for the index buffer to use sequential indices
    // (vert 0,1,2 = tri 0; 3,4,5 = tri 1; ...).
    //
    // Returns kInvalidHandle if the backend can't build it (no RT
    // support, allocation failure, etc.). Always check
    // `caps().supportsRaytracing` first.
    virtual BackendHandle createAccelerationStructure(const float* /*positions*/,
                                                      int /*vertCount*/,
                                                      const uint32_t* /*indices*/,
                                                      int /*indexCount*/) { return kInvalidHandle; }
    virtual void destroyAccelerationStructure(BackendHandle /*as*/) {}
    virtual void setComputeAccelerationStructure(BackendHandle /*shader*/,
                                                 int /*slot*/,
                                                 BackendHandle /*as*/) {}

    // ── Factory ────────────────────────────────────────────────────
    // Pass GraphicsAPI::Metal to attempt Metal; falls back / returns
    // nullptr if the platform doesn't support it (Phase M.0/M.1
    // always returns nullptr for Metal). Callers should always check
    // for nullptr and fall back to GraphicsAPI::OpenGL.
    static std::unique_ptr<RenderBackend> create(GraphicsAPI api);
    static bool available(GraphicsAPI api);
};

} // namespace easel
