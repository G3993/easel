// RenderBackend_Metal.mm — Phase M.1 Metal renderer (render-to-texture).
//
// What's wired in M.1:
//   • MTLDevice + MTLCommandQueue lifecycle
//   • MTLTexture creation / upload / destroy
//   • Render-target = wrap an MTLTexture in an MTLRenderPassDescriptor
//   • Shader compile from MSL source (caller-supplied)
//   • MTLRenderPipelineState built from compiled shader library
//   • Uniform buffer (one MTLBuffer per shader, slot 0)
//   • Per-frame command-buffer / encoder lifecycle
//   • Fullscreen-quad draw via 4-vertex hardcoded triangle strip
//
// What's deferred (Phase M.1.1+):
//   • SPIRV-Cross GLSL → MSL transpile (callers must hand us MSL today)
//   • CAMetalLayer drawable presentation (we render to off-screen
//     texture and let GL composite the final present)
//   • Textured uniform binding chains (multiple texture slots OK,
//     but no sampler-state cross-referencing yet)
//   • Compute shaders (Phase M.2 — separable Gaussian for bloom)
//
// Project compiles WITHOUT ARC (project-wide), so all `id<>` types are
// retained manually with [retain] / [release].

#include "render/RenderBackend.h"

#if defined(__APPLE__)
#import <Metal/Metal.h>
#import <Foundation/Foundation.h>
#include <unordered_map>
#include <string>
#include <vector>

namespace easel {

namespace {

// ── Internal handle wrappers ─────────────────────────────────────────
// Each backend handle is the address of a heap-allocated struct that
// owns the underlying Objective-C object plus any helper state.

struct MetalTextureSlot {
    id<MTLTexture> tex = nil;
    int  w = 0, h = 0;
    bool isDepth = false;
};

struct MetalRTSlot {
    BackendHandle colorTex = kInvalidHandle;
    BackendHandle depthTex = kInvalidHandle;
    int           w = 0, h = 0;
};

// One shader = vertex + fragment functions, a compiled pipeline state,
// and a uniform-buffer scratch area. Uniforms are written by name into
// a CPU-side blob and uploaded each draw — same pattern as our GL
// glUniform path, slow but correct.
struct MetalShaderSlot {
    id<MTLLibrary>             vertLib = nil;
    id<MTLLibrary>             fragLib = nil;
    id<MTLRenderPipelineState> pipeline = nil;
    id<MTLBuffer>              uniBuf  = nil;       // slot 0 fragment
    std::unordered_map<std::string, size_t> uniOffsets;  // name → byte offset
    size_t                     uniSize = 0;
    bool                       built   = false;
};

struct MetalComputeSlot {
    id<MTLLibrary>              lib      = nil;
    id<MTLComputePipelineState> pipeline = nil;
    BackendHandle               boundTextures[8] = { kInvalidHandle, kInvalidHandle,
                                                     kInvalidHandle, kInvalidHandle,
                                                     kInvalidHandle, kInvalidHandle,
                                                     kInvalidHandle, kInvalidHandle };
    BackendHandle               boundAS[2] = { kInvalidHandle, kInvalidHandle };
};

struct MetalASSlot {
    id<MTLAccelerationStructure> as     = nil;
    id<MTLBuffer>                vBuf   = nil;
    id<MTLBuffer>                iBuf   = nil;
    int                          tris   = 0;
};

class MetalBackend : public RenderBackend {
public:
    MetalBackend()
        : m_device(MTLCreateSystemDefaultDevice())  // +1 retained, we own it
    {
        if (m_device) {
            m_queue = [m_device newCommandQueue];   // +1 retained, we own it
            m_caps.supportsCompute      = true;
            m_caps.supportsRaytracing   = false;
            if (@available(macOS 11.0, *)) {
                // M-series + AMD RDNA GPUs ship hardware-accelerated
                // ray tracing on macOS 12.4+. Older Intel iGPUs return
                // false here and we fall back to no-RT mode.
                if ([m_device supportsRaytracing]) {
                    m_caps.supportsRaytracing = true;
                }
            }
            // Phase M.4 — mesh-shader capability detection.
            // Apple GPU family 9 = M3 series (and the M2 Max/Ultra in
            // newer macOS releases). Older Apple Silicon and any Intel
            // iGPU return false here.
            m_caps.supportsMeshShaders = false;
            if (@available(macOS 13.0, *)) {
                if ([m_device supportsFamily:MTLGPUFamilyApple9]) {
                    m_caps.supportsMeshShaders = true;
                }
            }
            m_caps.supportsHalfFloatRT  = true;
            m_caps.supportsAsyncCompute = true;
            m_caps.maxColorAttachments  = 8;
            m_caps.maxTextureSize       = 16384;
        }
    }

    ~MetalBackend() override {
        // Tear down all shader pipelines first (they hold lib refs).
        for (auto& kv : m_shaders) {
            auto* s = kv.second;
            if (s->pipeline) [s->pipeline release];
            if (s->vertLib)  [s->vertLib release];
            if (s->fragLib)  [s->fragLib release];
            if (s->uniBuf)   [s->uniBuf release];
            delete s;
        }
        for (auto& kv : m_compute) {
            if (kv.second->pipeline) [kv.second->pipeline release];
            if (kv.second->lib)      [kv.second->lib release];
            delete kv.second;
        }
        for (auto& kv : m_as) {
            if (kv.second->as)   [kv.second->as release];
            if (kv.second->vBuf) [kv.second->vBuf release];
            if (kv.second->iBuf) [kv.second->iBuf release];
            delete kv.second;
        }
        for (auto& kv : m_textures) {
            if (kv.second->tex) [kv.second->tex release];
            delete kv.second;
        }
        for (auto& kv : m_rts) delete kv.second;

        if (m_currentEncoder) [m_currentEncoder release];
        if (m_currentCommandBuffer) [m_currentCommandBuffer release];
        if (m_queue)  [m_queue release];
        if (m_device) [m_device release];
    }

    bool isValid() const { return m_device != nil; }

    // ── Identity ────────────────────────────────────────────────
    GraphicsAPI api() const override { return GraphicsAPI::Metal; }
    const BackendCaps& caps() const override { return m_caps; }
    std::string name() const override {
        if (!m_device) return std::string("Metal (no device)");
        NSString* n = [m_device name];
        return std::string("Metal: ") + std::string([n UTF8String]);
    }

    // ── Frame lifecycle ─────────────────────────────────────────
    // For render-to-texture flow, beginFrame is a no-op; draws happen
    // between bindRenderTarget + endFrame. endFrame commits the
    // currently-recording command buffer.
    void beginFrame(int /*w*/, int /*h*/) override {
        if (!m_currentCommandBuffer) {
            m_currentCommandBuffer = [[m_queue commandBuffer] retain];
        }
    }
    void endFrame() override {
        if (m_currentEncoder) {
            [m_currentEncoder endEncoding];
            [m_currentEncoder release];
            m_currentEncoder = nil;
            m_currentRT = kInvalidHandle;
        }
        if (m_currentCommandBuffer) {
            [m_currentCommandBuffer commit];
            [m_currentCommandBuffer waitUntilCompleted];
            [m_currentCommandBuffer release];
            m_currentCommandBuffer = nil;
        }
    }

    // ── Texture API ─────────────────────────────────────────────
    BackendHandle createTexture(int w, int h, TextureFormat fmt) override {
        if (!m_device) return kInvalidHandle;
        MTLPixelFormat pf = MTLPixelFormatRGBA8Unorm;
        bool isDepth = false;
        switch (fmt) {
            case TextureFormat::RGBA16F:  pf = MTLPixelFormatRGBA16Float;  break;
            case TextureFormat::RGBA32F:  pf = MTLPixelFormatRGBA32Float;  break;
            case TextureFormat::R8:       pf = MTLPixelFormatR8Unorm;      break;
            case TextureFormat::R16F:     pf = MTLPixelFormatR16Float;     break;
            case TextureFormat::Depth32F: pf = MTLPixelFormatDepth32Float; isDepth = true; break;
            case TextureFormat::RGBA8: default: break;
        }
        MTLTextureDescriptor* d = [MTLTextureDescriptor
            texture2DDescriptorWithPixelFormat:pf
                                         width:(NSUInteger)w
                                        height:(NSUInteger)h
                                     mipmapped:NO];
        d.usage = isDepth
            ? (MTLTextureUsageRenderTarget | MTLTextureUsageShaderRead)
            : (MTLTextureUsageRenderTarget | MTLTextureUsageShaderRead | MTLTextureUsageShaderWrite);
        d.storageMode = MTLStorageModePrivate;
        id<MTLTexture> tex = [m_device newTextureWithDescriptor:d];
        if (!tex) return kInvalidHandle;
        auto* slot = new MetalTextureSlot{ tex, w, h, isDepth };
        BackendHandle h2 = nextHandle();
        m_textures[h2] = slot;
        return h2;
    }

    void uploadTexture(BackendHandle h, const void* pixels,
                       int w, int h2, TextureFormat /*srcFmt*/) override {
        auto it = m_textures.find(h);
        if (it == m_textures.end() || !pixels) return;
        // Storage mode is private, so route through a shared blit buffer.
        // For M.1 we keep this simple: replaceRegion needs shared-mode tex.
        // Promote to a shared texture if upload requested. (Cheap re-create.)
        MTLTextureDescriptor* d = [MTLTextureDescriptor
            texture2DDescriptorWithPixelFormat:[it->second->tex pixelFormat]
                                         width:(NSUInteger)w
                                        height:(NSUInteger)h2
                                     mipmapped:NO];
        d.usage       = [it->second->tex usage];
        d.storageMode = MTLStorageModeShared;
        id<MTLTexture> shared = [m_device newTextureWithDescriptor:d];
        if (shared) {
            MTLRegion r = MTLRegionMake2D(0, 0, w, h2);
            [shared replaceRegion:r mipmapLevel:0
                        withBytes:pixels bytesPerRow:(NSUInteger)(w * 4)];
            [it->second->tex release];
            it->second->tex = shared;
            it->second->w = w;
            it->second->h = h2;
        }
    }

    void destroyTexture(BackendHandle h) override {
        auto it = m_textures.find(h);
        if (it == m_textures.end()) return;
        if (it->second->tex) [it->second->tex release];
        delete it->second;
        m_textures.erase(it);
    }

    // ── Render targets ─────────────────────────────────────────
    BackendHandle createRenderTarget(BackendHandle color, BackendHandle depth) override {
        auto* slot = new MetalRTSlot{ color, depth, 0, 0 };
        auto cit = m_textures.find(color);
        if (cit != m_textures.end()) {
            slot->w = cit->second->w;
            slot->h = cit->second->h;
        }
        BackendHandle h = nextHandle();
        m_rts[h] = slot;
        return h;
    }
    void destroyRenderTarget(BackendHandle h) override {
        auto it = m_rts.find(h);
        if (it == m_rts.end()) return;
        delete it->second;
        m_rts.erase(it);
    }
    void bindRenderTarget(BackendHandle h) override {
        // Close any prior encoder.
        if (m_currentEncoder) {
            [m_currentEncoder endEncoding];
            [m_currentEncoder release];
            m_currentEncoder = nil;
        }
        if (h == kInvalidHandle) {
            // No-op: there is no default drawable in render-to-texture mode.
            m_currentRT = kInvalidHandle;
            return;
        }
        auto it = m_rts.find(h);
        if (it == m_rts.end()) return;
        if (!m_currentCommandBuffer) {
            m_currentCommandBuffer = [[m_queue commandBuffer] retain];
        }
        MTLRenderPassDescriptor* rpd = [MTLRenderPassDescriptor renderPassDescriptor];
        auto cit = m_textures.find(it->second->colorTex);
        if (cit != m_textures.end()) {
            rpd.colorAttachments[0].texture     = cit->second->tex;
            rpd.colorAttachments[0].loadAction  = MTLLoadActionLoad;
            rpd.colorAttachments[0].storeAction = MTLStoreActionStore;
        }
        auto dit = m_textures.find(it->second->depthTex);
        if (dit != m_textures.end() && dit->second->isDepth) {
            rpd.depthAttachment.texture     = dit->second->tex;
            rpd.depthAttachment.loadAction  = MTLLoadActionLoad;
            rpd.depthAttachment.storeAction = MTLStoreActionStore;
        }
        m_currentEncoder =
            [[m_currentCommandBuffer renderCommandEncoderWithDescriptor:rpd] retain];
        m_currentRT = h;
    }
    void clearRenderTarget(float r, float g, float b, float a, bool clearDepth) override {
        // Reopen the encoder with a Clear load action. Cheap form: end
        // current encoder, build a new pass descriptor, replace.
        if (m_currentRT == kInvalidHandle) return;
        if (m_currentEncoder) {
            [m_currentEncoder endEncoding];
            [m_currentEncoder release];
            m_currentEncoder = nil;
        }
        auto it = m_rts.find(m_currentRT);
        if (it == m_rts.end()) return;
        MTLRenderPassDescriptor* rpd = [MTLRenderPassDescriptor renderPassDescriptor];
        auto cit = m_textures.find(it->second->colorTex);
        if (cit != m_textures.end()) {
            rpd.colorAttachments[0].texture     = cit->second->tex;
            rpd.colorAttachments[0].loadAction  = MTLLoadActionClear;
            rpd.colorAttachments[0].storeAction = MTLStoreActionStore;
            rpd.colorAttachments[0].clearColor  = MTLClearColorMake(r, g, b, a);
        }
        auto dit = m_textures.find(it->second->depthTex);
        if (clearDepth && dit != m_textures.end() && dit->second->isDepth) {
            rpd.depthAttachment.texture     = dit->second->tex;
            rpd.depthAttachment.loadAction  = MTLLoadActionClear;
            rpd.depthAttachment.storeAction = MTLStoreActionStore;
            rpd.depthAttachment.clearDepth  = 1.0;
        }
        m_currentEncoder =
            [[m_currentCommandBuffer renderCommandEncoderWithDescriptor:rpd] retain];
    }

    // ── Shaders ─────────────────────────────────────────────────
    // Caller passes MSL source for both vertex and fragment. The legacy
    // GL path passes GLSL — Phase M.1.1 will SPIRV-Cross transpile when
    // a GLSL string arrives. For M.1 we treat the input as MSL.
    BackendHandle createShader(const std::string& vertSrc, const std::string& fragSrc) override {
        if (!m_device) return kInvalidHandle;
        NSError* err = nil;
        NSString* vs = [NSString stringWithUTF8String:vertSrc.c_str()];
        NSString* fs = [NSString stringWithUTF8String:fragSrc.c_str()];
        MTLCompileOptions* opts = [[[MTLCompileOptions alloc] init] autorelease];
        id<MTLLibrary> vl = [m_device newLibraryWithSource:vs options:opts error:&err];
        if (!vl || err) return kInvalidHandle;
        id<MTLLibrary> fl = [m_device newLibraryWithSource:fs options:opts error:&err];
        if (!fl || err) { [vl release]; return kInvalidHandle; }
        id<MTLFunction> vfn = [vl newFunctionWithName:@"vertexMain"];
        id<MTLFunction> ffn = [fl newFunctionWithName:@"fragmentMain"];
        if (!vfn || !ffn) {
            [vl release]; [fl release];
            if (vfn) [vfn release];
            if (ffn) [ffn release];
            return kInvalidHandle;
        }
        MTLRenderPipelineDescriptor* pd = [[[MTLRenderPipelineDescriptor alloc] init] autorelease];
        pd.vertexFunction   = vfn;
        pd.fragmentFunction = ffn;
        pd.colorAttachments[0].pixelFormat = MTLPixelFormatRGBA16Float;  // matches our 16F composite chain
        id<MTLRenderPipelineState> pso =
            [m_device newRenderPipelineStateWithDescriptor:pd error:&err];
        [vfn release];
        [ffn release];
        if (!pso || err) {
            [vl release]; [fl release];
            return kInvalidHandle;
        }
        // Pre-allocate a 4 KB uniform buffer. Real reflection of MSL
        // struct layout comes in M.1.2; for now setUniformX writes into
        // hashed offsets the caller registers.
        id<MTLBuffer> uni = [m_device newBufferWithLength:4096 options:MTLResourceStorageModeShared];
        auto* slot = new MetalShaderSlot();
        slot->vertLib  = vl;
        slot->fragLib  = fl;
        slot->pipeline = pso;
        slot->uniBuf   = uni;
        slot->built    = true;
        BackendHandle h = nextHandle();
        m_shaders[h] = slot;
        return h;
    }

    void destroyShader(BackendHandle h) override {
        auto it = m_shaders.find(h);
        if (it == m_shaders.end()) return;
        if (it->second->pipeline) [it->second->pipeline release];
        if (it->second->vertLib)  [it->second->vertLib  release];
        if (it->second->fragLib)  [it->second->fragLib  release];
        if (it->second->uniBuf)   [it->second->uniBuf   release];
        delete it->second;
        m_shaders.erase(it);
    }

    void bindShader(BackendHandle h) override {
        auto it = m_shaders.find(h);
        if (it == m_shaders.end() || !m_currentEncoder) return;
        [m_currentEncoder setRenderPipelineState:it->second->pipeline];
        [m_currentEncoder setFragmentBuffer:it->second->uniBuf offset:0 atIndex:0];
        [m_currentEncoder setVertexBuffer:it->second->uniBuf offset:0 atIndex:0];
        m_boundShader = h;
    }

    // Uniforms — written into the per-shader scratch buffer at the
    // offset registered for `name`. Caller registers offsets up front
    // by setting any value once. Subsequent writes hit the same slot.
    size_t locOrAlloc(MetalShaderSlot* s, const char* name, size_t bytes) {
        std::string n(name);
        auto it = s->uniOffsets.find(n);
        if (it != s->uniOffsets.end()) return it->second;
        size_t off = (s->uniSize + 15u) & ~15u;  // 16-byte align
        s->uniOffsets[n] = off;
        s->uniSize = off + bytes;
        return off;
    }
    void writeUniform(BackendHandle sh, const char* name, const void* src, size_t bytes) {
        auto it = m_shaders.find(sh);
        if (it == m_shaders.end()) return;
        size_t off = locOrAlloc(it->second, name, bytes);
        if (off + bytes > 4096) return;  // grow path is M.1.3
        memcpy(((uint8_t*)[it->second->uniBuf contents]) + off, src, bytes);
    }

    void setUniformFloat(BackendHandle s, const char* n, float v) override {
        writeUniform(s, n, &v, sizeof(float));
    }
    void setUniformVec2(BackendHandle s, const char* n, float x, float y) override {
        float v[2] = { x, y }; writeUniform(s, n, v, sizeof(v));
    }
    void setUniformVec3(BackendHandle s, const char* n, float x, float y, float z) override {
        // pad to 16 bytes (Metal vec3 alignment)
        float v[4] = { x, y, z, 0.0f }; writeUniform(s, n, v, sizeof(v));
    }
    void setUniformVec4(BackendHandle s, const char* n, float x, float y, float z, float w) override {
        float v[4] = { x, y, z, w }; writeUniform(s, n, v, sizeof(v));
    }
    void setUniformMat4(BackendHandle s, const char* n, const float* m) override {
        writeUniform(s, n, m, sizeof(float) * 16);
    }
    void setUniformInt(BackendHandle s, const char* n, int v) override {
        writeUniform(s, n, &v, sizeof(int));
    }
    void setUniformTexture(BackendHandle /*s*/, const char* /*name*/, int slot, BackendHandle tex) override {
        if (!m_currentEncoder) return;
        auto it = m_textures.find(tex);
        if (it == m_textures.end()) return;
        [m_currentEncoder setFragmentTexture:it->second->tex atIndex:(NSUInteger)slot];
    }

    // ── Draw ─────────────────────────────────────────────────────
    void drawFullscreenQuad() override {
        if (!m_currentEncoder) return;
        [m_currentEncoder drawPrimitives:MTLPrimitiveTypeTriangleStrip
                             vertexStart:0
                             vertexCount:4];
    }

    // ── Phase M.2 — compute shaders ─────────────────────────────
    BackendHandle createComputeShader(const std::string& mslSrc) override {
        if (!m_device) return kInvalidHandle;
        NSError* err = nil;
        NSString* src = [NSString stringWithUTF8String:mslSrc.c_str()];
        MTLCompileOptions* opts = [[[MTLCompileOptions alloc] init] autorelease];
        id<MTLLibrary> lib = [m_device newLibraryWithSource:src options:opts error:&err];
        if (!lib || err) return kInvalidHandle;
        id<MTLFunction> fn = [lib newFunctionWithName:@"computeMain"];
        if (!fn) { [lib release]; return kInvalidHandle; }
        id<MTLComputePipelineState> pso =
            [m_device newComputePipelineStateWithFunction:fn error:&err];
        [fn release];
        if (!pso || err) { [lib release]; return kInvalidHandle; }
        auto* slot = new MetalComputeSlot();
        slot->lib      = lib;
        slot->pipeline = pso;
        BackendHandle h = nextHandle();
        m_compute[h] = slot;
        return h;
    }

    void destroyComputeShader(BackendHandle h) override {
        auto it = m_compute.find(h);
        if (it == m_compute.end()) return;
        if (it->second->pipeline) [it->second->pipeline release];
        if (it->second->lib)      [it->second->lib release];
        delete it->second;
        m_compute.erase(it);
    }

    void setComputeTexture(BackendHandle sh, int slot, BackendHandle tex) override {
        auto cit = m_compute.find(sh);
        if (cit == m_compute.end()) return;
        if (slot < 0 || slot >= 8) return;
        cit->second->boundTextures[slot] = tex;
    }

    bool dispatchCompute(BackendHandle sh, int gx, int gy, int gz) override {
        auto cit = m_compute.find(sh);
        if (cit == m_compute.end()) return false;
        // Close any prior render encoder — compute uses its own encoder type.
        if (m_currentEncoder) {
            [m_currentEncoder endEncoding];
            [m_currentEncoder release];
            m_currentEncoder = nil;
        }
        if (!m_currentCommandBuffer) {
            m_currentCommandBuffer = [[m_queue commandBuffer] retain];
        }
        id<MTLComputeCommandEncoder> ce =
            [m_currentCommandBuffer computeCommandEncoder];
        [ce setComputePipelineState:cit->second->pipeline];
        for (int i = 0; i < 8; i++) {
            BackendHandle th = cit->second->boundTextures[i];
            if (th == kInvalidHandle) continue;
            auto tit = m_textures.find(th);
            if (tit == m_textures.end()) continue;
            [ce setTexture:tit->second->tex atIndex:(NSUInteger)i];
        }
        // Bind any acceleration structures the kernel needs to ray-trace
        // against. Slot 0 = primary AS, slot 1 = optional secondary
        // (e.g. instance AS). Mapped to argument-table buffer slots
        // 16..17 by convention to keep them out of the uniform area.
        if (@available(macOS 11.0, *)) {
            for (int i = 0; i < 2; i++) {
                BackendHandle ah = cit->second->boundAS[i];
                if (ah == kInvalidHandle) continue;
                auto ait = m_as.find(ah);
                if (ait == m_as.end()) continue;
                [ce setAccelerationStructure:ait->second->as atBufferIndex:(NSUInteger)(16 + i)];
            }
        }
        // Use the pipeline's own threadExecutionWidth × maxTotalThreadsPerThreadgroup
        // partitioning. For an 8×8×1 group this is a sensible default.
        NSUInteger w = [cit->second->pipeline threadExecutionWidth];
        NSUInteger h = [cit->second->pipeline maxTotalThreadsPerThreadgroup] / w;
        if (h < 1) h = 1;
        MTLSize tgSize = MTLSizeMake(w, h, 1);
        MTLSize tgCount = MTLSizeMake((NSUInteger)gx, (NSUInteger)gy, (NSUInteger)gz);
        [ce dispatchThreadgroups:tgCount threadsPerThreadgroup:tgSize];
        [ce endEncoding];
        return true;
    }

    // ── Phase M.3 — ray tracing ─────────────────────────────────
    BackendHandle createAccelerationStructure(const float* positions,
                                              int vertCount,
                                              const uint32_t* indices,
                                              int indexCount) override {
        if (!m_caps.supportsRaytracing || !m_device || !positions || vertCount <= 0)
            return kInvalidHandle;
        if (@available(macOS 11.0, *)) {
            // Stage vertex + (optional) index buffers in shared memory
            // first so the AS builder can read them.
            id<MTLBuffer> vb = [m_device
                newBufferWithBytes:positions
                            length:(NSUInteger)(vertCount * 3 * sizeof(float))
                           options:MTLResourceStorageModeShared];
            id<MTLBuffer> ib = nil;
            if (indices && indexCount > 0) {
                ib = [m_device
                    newBufferWithBytes:indices
                                length:(NSUInteger)(indexCount * sizeof(uint32_t))
                               options:MTLResourceStorageModeShared];
            }
            int triCount = ib ? (indexCount / 3) : (vertCount / 3);
            MTLAccelerationStructureTriangleGeometryDescriptor* tg =
                [MTLAccelerationStructureTriangleGeometryDescriptor descriptor];
            tg.vertexBuffer       = vb;
            tg.vertexStride       = 3 * sizeof(float);
            tg.triangleCount      = (NSUInteger)triCount;
            if (ib) {
                tg.indexBuffer    = ib;
                tg.indexType      = MTLIndexTypeUInt32;
            }
            MTLPrimitiveAccelerationStructureDescriptor* pd =
                [MTLPrimitiveAccelerationStructureDescriptor descriptor];
            pd.geometryDescriptors = @[ tg ];
            MTLAccelerationStructureSizes sizes =
                [m_device accelerationStructureSizesWithDescriptor:pd];
            id<MTLAccelerationStructure> as =
                [m_device newAccelerationStructureWithSize:sizes.accelerationStructureSize];
            id<MTLBuffer> scratch = [m_device
                newBufferWithLength:sizes.buildScratchBufferSize
                            options:MTLResourceStorageModePrivate];
            id<MTLCommandBuffer>                          cb =
                [m_queue commandBuffer];
            id<MTLAccelerationStructureCommandEncoder>    ce =
                [cb accelerationStructureCommandEncoder];
            [ce buildAccelerationStructure:as
                                descriptor:pd
                             scratchBuffer:scratch
                       scratchBufferOffset:0];
            [ce endEncoding];
            [cb commit];
            [cb waitUntilCompleted];
            [scratch release];
            auto* slot = new MetalASSlot();
            slot->as   = as;       // already +1 from newAccelerationStructure
            slot->vBuf = vb;
            slot->iBuf = ib;
            slot->tris = triCount;
            BackendHandle h = nextHandle();
            m_as[h] = slot;
            return h;
        }
        return kInvalidHandle;
    }

    void destroyAccelerationStructure(BackendHandle h) override {
        auto it = m_as.find(h);
        if (it == m_as.end()) return;
        if (it->second->as)   [it->second->as release];
        if (it->second->vBuf) [it->second->vBuf release];
        if (it->second->iBuf) [it->second->iBuf release];
        delete it->second;
        m_as.erase(it);
    }

    void setComputeAccelerationStructure(BackendHandle sh, int slot, BackendHandle as) override {
        auto cit = m_compute.find(sh);
        if (cit == m_compute.end()) return;
        if (slot < 0 || slot >= 2) return;
        cit->second->boundAS[slot] = as;
    }

private:
    BackendHandle nextHandle() { return ++m_handleCounter; }

    id<MTLDevice>              m_device  = nil;
    id<MTLCommandQueue>        m_queue   = nil;
    id<MTLCommandBuffer>       m_currentCommandBuffer = nil;
    id<MTLRenderCommandEncoder> m_currentEncoder = nil;
    BackendHandle              m_currentRT = kInvalidHandle;
    BackendHandle              m_boundShader = kInvalidHandle;

    std::unordered_map<BackendHandle, MetalTextureSlot*> m_textures;
    std::unordered_map<BackendHandle, MetalRTSlot*>      m_rts;
    std::unordered_map<BackendHandle, MetalShaderSlot*>  m_shaders;
    std::unordered_map<BackendHandle, MetalComputeSlot*> m_compute;
    std::unordered_map<BackendHandle, MetalASSlot*>      m_as;
    BackendHandle m_handleCounter = 100;
    BackendCaps   m_caps;
};

} // anonymous

// ── Factory entry points ─────────────────────────────────────────
bool metalIsAvailable() {
    @autoreleasepool {
        id<MTLDevice> dev = MTLCreateSystemDefaultDevice();
        return dev != nil;
    }
}

std::unique_ptr<RenderBackend> createMetalBackend() {
    auto b = std::make_unique<MetalBackend>();
    if (!b->isValid()) return nullptr;
    return b;
}

} // namespace easel
#endif
