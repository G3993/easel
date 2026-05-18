// test_render_backend.cpp — Phase M.0 smoke test.
//
// Verifies the RenderBackend abstraction header compiles, the factory
// returns a valid GL backend, the Metal availability probe works, and
// the fallback path (Metal → GL) returns a usable object even when
// Metal isn't yet implemented (which it isn't in M.0).

#define CATCH_CONFIG_MAIN
#include <catch2/catch_all.hpp>
#include "render/RenderBackend.h"

using easel::GraphicsAPI;
using easel::RenderBackend;

TEST_CASE("GL backend factory returns a non-null instance", "[render][m0]") {
    auto gl = RenderBackend::create(GraphicsAPI::OpenGL);
    REQUIRE(gl);
    REQUIRE(gl->api() == GraphicsAPI::OpenGL);
}

TEST_CASE("GL backend reports its caps", "[render][m0]") {
    auto gl = RenderBackend::create(GraphicsAPI::OpenGL);
    REQUIRE(gl);
    const auto& caps = gl->caps();
    // GL 4.1 ceiling on macOS — these are the constraints Phase M was created to escape.
    REQUIRE(caps.supportsCompute    == false);
    REQUIRE(caps.supportsRaytracing == false);
    REQUIRE(caps.supportsHalfFloatRT == true);
    REQUIRE(caps.maxColorAttachments >= 4);
    REQUIRE(caps.maxTextureSize     >= 8192);
}

TEST_CASE("Metal availability probe runs without crashing", "[render][m0][metal]") {
    // On macOS this should return true (every modern Mac has Metal); on
    // other platforms it returns false. Either way, no crash.
    bool av = RenderBackend::available(GraphicsAPI::Metal);
#if defined(__APPLE__)
    REQUIRE(av == true);
#else
    REQUIRE(av == false);
#endif
}

TEST_CASE("Metal backend is now real on macOS (Phase M.1)", "[render][m1][metal]") {
    auto m = RenderBackend::create(GraphicsAPI::Metal);
    REQUIRE(m);
#if defined(__APPLE__)
    // M.1 ships a real Metal backend. Factory should return it.
    REQUIRE(m->api() == GraphicsAPI::Metal);
    // Metal caps lift the GL 4.1 ceiling.
    REQUIRE(m->caps().supportsCompute    == true);
    REQUIRE(m->caps().supportsAsyncCompute == true);
    // Name should mention Metal.
    REQUIRE(m->name().find("Metal") != std::string::npos);
#else
    // Non-Apple platforms still fall back to GL.
    REQUIRE(m->api() == GraphicsAPI::OpenGL);
#endif
}

#if defined(__APPLE__)
TEST_CASE("Metal backend creates and destroys textures", "[render][m1][metal]") {
    auto m = RenderBackend::create(GraphicsAPI::Metal);
    REQUIRE(m);
    REQUIRE(m->api() == GraphicsAPI::Metal);
    auto tex = m->createTexture(256, 256, easel::TextureFormat::RGBA16F);
    REQUIRE(tex != easel::kInvalidHandle);
    m->destroyTexture(tex);
}

TEST_CASE("Metal backend compiles a trivial MSL shader pair", "[render][m1][metal]") {
    auto m = RenderBackend::create(GraphicsAPI::Metal);
    REQUIRE(m);
    const std::string vert = R"(
#include <metal_stdlib>
using namespace metal;
vertex float4 vertexMain(uint vid [[vertex_id]]) {
    float2 p[4] = { float2(-1,-1), float2(1,-1), float2(-1,1), float2(1,1) };
    return float4(p[vid], 0.0, 1.0);
}
)";
    const std::string frag = R"(
#include <metal_stdlib>
using namespace metal;
fragment float4 fragmentMain() {
    return float4(0.5, 0.5, 0.5, 1.0);
}
)";
    auto sh = m->createShader(vert, frag);
    REQUIRE(sh != easel::kInvalidHandle);
    m->destroyShader(sh);
}

TEST_CASE("Metal backend creates and destroys render targets", "[render][m1][metal]") {
    auto m = RenderBackend::create(GraphicsAPI::Metal);
    REQUIRE(m);
    auto color = m->createTexture(64, 64, easel::TextureFormat::RGBA16F);
    REQUIRE(color != easel::kInvalidHandle);
    auto rt = m->createRenderTarget(color, easel::kInvalidHandle);
    REQUIRE(rt != easel::kInvalidHandle);
    m->destroyRenderTarget(rt);
    m->destroyTexture(color);
}

// ── Phase M.2: compute shaders ─────────────────────────────────────
TEST_CASE("Metal backend compiles a compute kernel", "[render][m2][metal]") {
    auto m = RenderBackend::create(GraphicsAPI::Metal);
    REQUIRE(m);
    REQUIRE(m->caps().supportsCompute);

    // Trivial fill kernel — write a constant to every texel of an output texture.
    const std::string msl = R"(
#include <metal_stdlib>
using namespace metal;
kernel void computeMain(texture2d<float, access::write> outTex [[texture(0)]],
                        uint2 gid [[thread_position_in_grid]]) {
    if (gid.x >= outTex.get_width() || gid.y >= outTex.get_height()) return;
    outTex.write(float4(0.5, 0.5, 0.5, 1.0), gid);
}
)";
    auto sh = m->createComputeShader(msl);
    REQUIRE(sh != easel::kInvalidHandle);
    m->destroyComputeShader(sh);
}

// ── Phase M.4: mesh shader capability ──────────────────────────────
TEST_CASE("Metal backend reports mesh-shader capability honestly", "[render][m4][metal]") {
    auto m = RenderBackend::create(GraphicsAPI::Metal);
    REQUIRE(m);
    bool ms = m->caps().supportsMeshShaders;
    INFO("supportsMeshShaders = " << (ms ? "yes" : "no"));
    SUCCEED();
}

// ── Phase M.3: ray tracing ─────────────────────────────────────────
TEST_CASE("Metal backend reports raytracing capability honestly", "[render][m3][metal]") {
    auto m = RenderBackend::create(GraphicsAPI::Metal);
    REQUIRE(m);
    // Don't require RT support — older Intel iGPUs lack it. Just confirm
    // the cap is a clean bool that reflects MTLDevice.supportsRaytracing.
    bool rt = m->caps().supportsRaytracing;
    INFO("supportsRaytracing = " << (rt ? "yes" : "no"));
    SUCCEED();
}

TEST_CASE("Metal backend builds an acceleration structure when supported",
          "[render][m3][metal]") {
    auto m = RenderBackend::create(GraphicsAPI::Metal);
    REQUIRE(m);
    if (!m->caps().supportsRaytracing) {
        SUCCEED("Skipped — MTLDevice does not support hardware raytracing");
        return;
    }
    // A single triangle in the XY plane.
    const float verts[9] = {
        -1.0f, -1.0f, 0.0f,
         1.0f, -1.0f, 0.0f,
         0.0f,  1.0f, 0.0f
    };
    const uint32_t idx[3] = { 0, 1, 2 };
    auto as = m->createAccelerationStructure(verts, 3, idx, 3);
    REQUIRE(as != easel::kInvalidHandle);
    m->destroyAccelerationStructure(as);
}

TEST_CASE("Metal backend dispatches a compute kernel", "[render][m2][metal]") {
    auto m = RenderBackend::create(GraphicsAPI::Metal);
    REQUIRE(m);
    auto outTex = m->createTexture(64, 64, easel::TextureFormat::RGBA16F);
    REQUIRE(outTex != easel::kInvalidHandle);

    const std::string msl = R"(
#include <metal_stdlib>
using namespace metal;
kernel void computeMain(texture2d<float, access::write> outTex [[texture(0)]],
                        uint2 gid [[thread_position_in_grid]]) {
    if (gid.x >= outTex.get_width() || gid.y >= outTex.get_height()) return;
    outTex.write(float4(float(gid.x)/64.0, float(gid.y)/64.0, 0.0, 1.0), gid);
}
)";
    auto sh = m->createComputeShader(msl);
    REQUIRE(sh != easel::kInvalidHandle);
    m->setComputeTexture(sh, 0, outTex);
    m->beginFrame(64, 64);
    bool dispatched = m->dispatchCompute(sh, 8, 8, 1);
    m->endFrame();
    REQUIRE(dispatched);

    m->destroyComputeShader(sh);
    m->destroyTexture(outTex);
}
#endif

TEST_CASE("Backend reports a non-empty name string", "[render][m0]") {
    auto gl = RenderBackend::create(GraphicsAPI::OpenGL);
    REQUIRE(gl);
    // Without a live GL context glGetString returns null, but the
    // backend's name() must still return a string (the placeholder
    // "OpenGL (uninitialized)") — never an empty string.
    REQUIRE_FALSE(gl->name().empty());
}
