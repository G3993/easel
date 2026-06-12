#pragma once
#include "sources/ContentSource.h"
#include "render/Framebuffer.h"
#include "render/ShaderProgram.h"
#include "render/Mesh.h"
#include "warp/ObjMeshWarp.h"
#include <glm/glm.hpp>
#include <string>

// Live-tunable knobs for the hologram-model layer, surfaced in the Properties
// panel. Defaults reproduce the hologram_glitch.fs look on an uploaded mesh.
struct HologramModelParams {
    float rotateSpeed   = 0.35f;   // auto-spin rate of the model
    float modelScale    = 1.0f;    // size multiplier (1 = fit unit sphere)
    float wireBright    = 1.0f;    // wireframe line brightness
    float surfaceFill   = 0.22f;   // faint translucent surface amount (0..1)
    // ── Glitch (mirrors hologram_glitch.fs inputs) ──
    float scanSpeed     = 0.55f;
    float interference  = 0.45f;
    float chromaShift   = 0.008f;
    float beamHaze      = 0.40f;
    float audioReact    = 1.0f;
};

// "Hologram Model" — upload your own 3D model (.obj/.gltf/.glb) and see it as
// an unstable projected hologram. Two passes into offscreen FBOs:
//   1. the loaded mesh rendered as a glowing cyan wireframe (+ faint surface),
//      spinning, into m_meshFBO (color + depth);
//   2. a fullscreen glitch pass (scan sweep, interference bands, chromatic
//      aberration, bass-triggered dropouts, volumetric beam haze) ported from
//      hologram_glitch.fs, sampling the mesh render into m_output.
// Composes an ObjMeshWarp purely as the model loader (it owns the one
// TINYGLTF_IMPLEMENTATION), then draws its materials() meshes — same idiom as
// MovingCompanySource. Output texture composits like any other layer.
class HologramModelSource : public ContentSource {
public:
    HologramModelSource() = default;
    ~HologramModelSource() override = default;

    bool init(int resolutionW = 1280, int resolutionH = 720);
    bool loadModel(const std::string& path);   // .obj/.gltf/.glb

    void update() override;
    GLuint textureId() const override { return m_output.textureId(); }
    int width() const override  { return m_output.width(); }
    int height() const override { return m_output.height(); }
    std::string typeName() const override { return "Hologram Model"; }
    bool isShader() const override { return false; }
    std::string sourcePath() const override { return m_modelPath; }

    bool isReady() const { return m_ready; }
    bool hasModel() const { return m_hasModel; }

    // Audio (bass/mid/treble 0..1) — fed each frame by Application, drives the
    // glitch the same way the ISF shader's audio uniforms do.
    void setAudioState(float bass, float mid, float treble) {
        m_audioBass = bass; m_audioMid = mid; m_audioHigh = treble;
    }

    HologramModelParams& params() { return m_params; }
    const HologramModelParams& params() const { return m_params; }

    // PropertyPanel sets this to request a "change model" file dialog; the
    // Application loop (which owns the native picker) consumes + clears it.
    bool m_requestModelDialog = false;

private:
    HologramModelParams m_params;

    Framebuffer   m_meshFBO;     // pass 1: mesh render (color + depth)
    Framebuffer   m_output;      // pass 2: glitched result
    ShaderProgram m_meshShader;  // mesh fill / wireframe (uMode)
    ShaderProgram m_glitchShader;// fullscreen glitch post
    Mesh          m_quad;        // fullscreen quad for the glitch pass
    ObjMeshWarp   m_model;       // model loader (we draw its materials() meshes)

    std::string m_modelPath;
    glm::vec3   m_center{0.0f};
    float       m_extent = 1.0f;
    double      m_startTime = 0.0;
    bool        m_ready = false;
    bool        m_hasModel = false;
    int         m_w = 1280, m_h = 720;

    float m_audioBass = 0.0f, m_audioMid = 0.0f, m_audioHigh = 0.0f;

    void renderMesh(float t, float aspect);
};
