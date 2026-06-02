#pragma once
#include "sources/ContentSource.h"
#include "sources/AudioBinding.h"
#include <glad/glad.h>
#include <cstdint>
#include <map>
#include <string>

// Native 3D smoothed-particle-hydrodynamics fluid, ported from Wyatt's
// shadertoy "Particle cluster grid SPH 3D" (shadertoy.com/view/mstfzS).
//
// Each grid cell holds up to 2 particles (mass, pos, vel, force, density)
// bit-packed into a single RGBA32UI voxel. The sim runs as 4 fragment
// passes per frame over real GL_TEXTURE_3D volumes (NOT the shadertoy
// 2D-tiling hack), using layered rendering (one FBO, glFramebufferTextureLayer
// per Z slice):
//   A: advect neighbors + clusterize into this cell's 2 particles
//   B: SPH density (gaussian kernel over ±2 neighbors)
//   C: apply forces + integrate; seed an initial block on the first frames
//   D: shadow march along the light direction (optical density)
// A final raymarch (Image) traces the particle spheres into the RGBA8
// output texture.
//
// Buffer dependency graph (no ping-pong needed — each pass writes a
// distinct volume, C feeds A next frame by pass ordering):
//   A.in = C(prev)        A.out = volA
//   B.in = volA           B.out = volB
//   C.in = volA + volB    C.out = volC
//   D.in = volB           D.out = volD
//   Image.in = volC + volB + volD
//
// Particle volumes (volA/volC): RGBA32UI + NEAREST (exact bit-packed,
// texelFetch only). Density/shadow (volB/volD): RGBA16F + LINEAR (trilinear).
class FluidSource3D : public ContentSource {
public:
    FluidSource3D() = default;
    ~FluidSource3D() override;

    bool init(int outW = 1280, int outH = 720);

    // ContentSource
    void update() override;
    GLuint textureId() const override { return m_output.tex; }
    int width() const override { return m_outW; }
    int height() const override { return m_outH; }
    std::string typeName() const override { return "Fluid3D"; }
    std::string sourcePath() const override { return "__fluid3d__"; }

    // ── Audio/MIDI-reactive parameter bindings ────────────────────────
    std::map<std::string, AudioBinding>& audioBindings() { return m_audioBindings; }
    const std::map<std::string, AudioBinding>& audioBindings() const { return m_audioBindings; }
    void applyAudioBindings(float level, float bass, float mid, float high,
                            float beat, float dt, class MIDIManager* midi = nullptr);

    // ── Image injection ──────────────────────────────────────────────
    // Tint the fluid surface with a bound layer's texture, mapped across
    // the fluid's world x/y extent. Host refreshes textureId each frame
    // from the bound layer (mirrors FluidSource's image pattern).
    struct ImageRef {
        GLuint   textureId     = 0;
        int      width         = 0;
        int      height        = 0;
        uint32_t sourceLayerId = 0;
        bool     flippedV      = false;
    };
    ImageRef&       imageSource()       { return m_image; }
    const ImageRef& imageSource() const { return m_image; }
    bool  m_imageEnabled = false;
    float m_imageMix     = 1.0f;    // 0 = palette only, 1 = full image albedo (video colors)

    // ── Live render config (PropertyPanel "Fluid3D" section, M5) ──────
    // The SPH constants live as GLSL #defines for now (faithful to the
    // shadertoy); M5 promotes the tunable ones to uniforms.
    float m_deepColor[3]  = {0.220f, 0.349f, 1.000f}; // albedo
    float m_glowColor[3]  = {0.420f, 0.302f, 0.996f}; // velocity glow
    float m_brightness    = 2.5f;     // diffuse multiplier

    // ── Color + lighting options (defaults reproduce the original look) ──
    float m_lightDir[3]    = {0.820f, 1.000f, 0.702f}; // key-light direction
    float m_lightIntensity = 1.0f;    // diffuse key intensity
    float m_ambient        = 0.10f;   // ambient fill (shadow floor)
    float m_specular       = 1.0f;    // reflection/specular strength
    float m_shallowColor[3]= {0.60f, 0.85f, 1.00f};    // rim / grazing-edge tint
    float m_rim            = 0.0f;    // rim amount (0 = off)
    float m_saturation     = 1.0f;    // output saturation
    // Background (also reflection env) + blob sphere size. Defaults = original.
    float m_bgTop[3]       = {0.12f, 0.16f, 0.28f};
    float m_bgBottom[3]    = {0.02f, 0.03f, 0.06f};
    float m_bgAlpha        = 1.0f;    // 0 = transparent bg (composites under)
    float m_sphereScale    = 1.0f;    // particle sphere size (lower = smaller blobs)
    float m_zoom           = 1.0f;    // camera zoom (1 = original; >1 zooms in)
    // Audio-driven motion: 0 = off (original constant motion); higher = the
    // fluid is calm when quiet and churns as the music gets loud.
    float m_audioIntensity = 0.0f;
    float m_audioEnergy    = 0.0f;    // smoothed loudness (updated from audio)
    // Force fields (layered on top of the base SPH; defaults = original look).
    float m_gravity        = 1.0f;    // gravity strength (0 = zero-g, 1 = original)
    float m_vortex         = 0.0f;    // coherent swirl around vertical axis (0 = off)
    float m_turbulence     = 0.0f;    // chaotic turbulent churn (0 = off)
    float m_forceScale     = 1.0f;    // SPH cohesion force (1 = original)

    bool  m_autoRotate    = true;
    float m_rotateSpeed   = 0.2f;     // mstfzS: angles = vec2(0.2*iTime, -0.5)
    float m_tilt          = -0.5f;

    int   m_simRes        = 64;       // size3d cube edge (volume realloc on change)
    int   m_substeps      = 1;        // sim iterations per frame
    // Render resolution as a fraction of the zone size. 1.0 = full/crisp;
    // lower trades sharpness for speed. Applied live (output realloc).
    float m_renderScale   = 1.0f;

private:
    struct Volume { GLuint tex = 0; int size = 0; GLint internalFmt = 0; };
    struct FBO    { GLuint fbo = 0, tex = 0; int w = 0, h = 0; };

    bool   m_ready = false;
    int    m_outW = 1280, m_outH = 720;   // actual render-target dims (scaled)
    int    m_zoneW = 1280, m_zoneH = 720; // full zone res (pre-scale)
    int    m_size = 64;          // active size3d
    int    m_frame = 0;          // iFrame analogue (drives the seed block)
    double m_lastTime = 0.0;
    float  m_time = 0.0f;        // iTime analogue (drives gravity wobble + camera)

    std::map<std::string, AudioBinding> m_audioBindings;
    ImageRef m_image;

    Volume m_volA, m_volB, m_volC, m_volD;
    FBO    m_output;
    GLuint m_volFBO = 0;         // layered-render FBO, retargeted per Z slice

    GLuint m_progA = 0, m_progB = 0, m_progC = 0, m_progD = 0, m_progRender = 0;
    GLuint m_quadVAO = 0, m_quadVBO = 0;

    GLuint compileProgram(const char* vs, const char* fs);
    GLuint compileProgram(const char* vs, const char* gs, const char* fs);
    Volume createVolume(int size, GLint internalFmt, GLenum format,
                        GLenum type, GLenum filter);
    void   destroyVolume(Volume& v);
    void   clearVolume(const Volume& v, bool isInt);
    FBO    createFBO(int w, int h, GLint internalFmt, GLenum type, GLenum filter);
    void   destroyFBO(FBO& f);

    // Run a fragment pass over every Z slice of `dst` (uSlice uniform).
    // Caller binds input samplers + sets non-slice uniforms first.
    void runVolumePass(GLuint prog, const Volume& dst);
    void bindVolume(GLuint prog, const char* name, const Volume& v, int unit);
    void renderToOutput();

    // Live realloc when m_simRes / m_renderScale change (called from update()
    // inside the GL-state-saved region).
    void reallocVolumes();
    void reallocOutput();
};
