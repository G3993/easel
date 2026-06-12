#pragma once
#include "sources/ContentSource.h"
#include "render/Framebuffer.h"
#include "render/ShaderProgram.h"
#include "render/Mesh.h"
#include "warp/ObjMeshWarp.h"
#include <glm/glm.hpp>
#include <string>

// Live-tunable knobs surfaced in the Properties panel. Defaults reproduce
// the shipped hero look; everything reads back into update()/shader uniforms
// each frame so edits are instant.
struct MovingCompanyParams {
    // ── Camera ──
    float camDistance = 2.55f;   // how far the chase cam sits
    float camHeight   = 0.65f;   // cam vertical offset
    float orbitSpeed  = 0.10f;   // how fast the cam circles the jet
    float fov         = 42.0f;   // field of view, degrees
    // ── Flight maneuver ──
    float yawRate     = 0.18f;   // continuous turn rate
    float bankAmount  = 0.38f;   // roll amplitude
    float pitchAmount = 0.13f;   // pitch bob amplitude
    float baseYawDeg  = -90.0f;  // resting orientation of the jet
    // ── Jet look ──
    float jetBrightness = 0.85f;            // key-light strength
    float rimIntensity  = 1.05f;            // cyan rim pop
    glm::vec3 jetColor = {0.045f, 0.05f, 0.065f}; // matte body
    glm::vec3 rimColor = {0.30f, 0.62f, 1.0f};    // rim / accent
    // ── Space ──
    float warpSpeed     = 1.0f;  // streak travel speed
    float warpIntensity = 2.3f;  // streak brightness
    float starDensity   = 1.0f;  // static star field amount
    float nebulaAmount  = 1.0f;  // nebula cloud strength
};

// "Moving Company" — a hero shot of the F-117 Nighthawk flying through deep
// space. Unlike the ISF shader sources, this renders an *actual* glTF mesh:
// it composes an ObjMeshWarp purely as a GLB loader (so we don't duplicate
// the one-and-only TINYGLTF_IMPLEMENTATION), then draws the loaded meshes
// with its own lit/faceted shader over a procedural warp-speed starfield.
//
// The jet's geometry is normalized to a unit sphere at the origin; a chase
// camera drifts around it while the model banks/pitches/yaws and the
// background streaks past — together selling "flying through space".
//
// Output is an offscreen FBO (with depth); textureId() hands the composited
// frame to the layer compositor exactly like ParticleSource does.
class MovingCompanySource : public ContentSource {
public:
    MovingCompanySource() = default;
    ~MovingCompanySource() override = default;

    bool init(int resolutionW = 1920, int resolutionH = 1080);

    void update() override;
    GLuint textureId() const override { return m_output.textureId(); }
    int width() const override  { return m_output.width(); }
    int height() const override { return m_output.height(); }
    std::string typeName() const override { return "Moving Company"; }
    bool isShader() const override { return false; }
    std::string sourcePath() const override { return m_modelPath; }

    bool isReady() const { return m_ready; }

    // Live parameter bag — edited by the Properties panel, read every frame.
    MovingCompanyParams& params() { return m_params; }
    const MovingCompanyParams& params() const { return m_params; }

private:
    bool loadModel();

    MovingCompanyParams m_params;

    Framebuffer   m_output;        // color + depth render target
    ShaderProgram m_planeShader;   // lit faceted mesh
    ShaderProgram m_bgShader;      // fullscreen warp-speed space backdrop
    Mesh          m_quad;          // fullscreen quad for the backdrop pass
    ObjMeshWarp   m_model;         // GLB loader (we draw its materials() meshes)

    std::string m_modelPath;
    glm::vec3   m_center{0.0f};    // mesh bbox center (for normalization)
    float       m_extent = 1.0f;   // mesh bbox half-extent
    double      m_startTime = 0.0;
    bool        m_ready = false;
    int         m_w = 1920, m_h = 1080;
};
