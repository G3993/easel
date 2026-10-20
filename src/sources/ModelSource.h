#pragma once
#include "sources/ContentSource.h"
#include "sources/AudioBinding.h"
#include "render/Framebuffer.h"
#include "render/ShaderProgram.h"
#include "warp/ObjMeshWarp.h"
#include <glm/glm.hpp>
#include <map>
#include <string>
#include <vector>

// "3D Model" — the native scene's first real object kind. Loads a .obj /
// .gltf / .glb (ObjMeshWarp is the loader) and:
//   - in a 2D zone: renders a lit preview of the model into its own
//     texture, which composites like any other layer;
//   - in a 3D zone: the compositor draws the mesh itself into the zone's
//     world (scene camera, sun + shadow map, point lights), so it sits IN
//     the scene with the cards instead of being a picture of a model.
// The layer's position / scale / rotation / depth / tilt place it; the
// params below give it a material and a slow spin. Every float is bindable
// through the same AudioBinding model shaders and fluids use.
class ModelSource : public ContentSource {
public:
    struct ParamDef {
        const char* id;
        const char* label;
        float*      ptr;
        float       lo, hi;
    };

    ModelSource() = default;
    ~ModelSource() override = default;

    bool init(int previewSize = 1024);
    bool loadModel(const std::string& path);

    void update() override;
    void suspend() override;
    GLuint textureId() const override { return m_output.textureId(); }
    int width() const override  { return m_output.width(); }
    int height() const override { return m_output.height(); }
    std::string typeName() const override { return "3D Model"; }
    bool isShader() const override { return false; }
    std::string sourcePath() const override { return m_modelPath; }

    bool hasModel() const { return m_hasModel; }
    std::vector<MaterialGroup>& materials() { return m_model.materials(); }

    // Model-space -> unit box at the origin (what the scene places).
    glm::mat4 normalizeMatrix() const;
    // The accumulated auto-spin (about Y), applied after normalisation.
    glm::mat4 spinMatrix() const;

    // ── material / motion ─────────────────────────────────────────────
    glm::vec3 m_color   = glm::vec3(0.85f, 0.87f, 0.90f);
    float m_gloss       = 0.55f;   // 0 matte .. 1 mirror-tight highlight
    float m_metal       = 0.15f;   // tints the highlight with the colour
    float m_emissive    = 0.0f;    // self-glow (bloom picks it up)
    float m_spin        = 12.0f;   // degrees per second about Y
    float m_wire        = 0.0f;    // 0 solid .. 1 wireframe overlay
    float m_bob         = 0.0f;    // gentle floating bob amplitude

    std::vector<ParamDef> params() {
        return {
            {"spin",     "Spin",      &m_spin,     -90.0f, 90.0f},
            {"gloss",    "Gloss",     &m_gloss,      0.0f,  1.0f},
            {"metal",    "Metal",     &m_metal,      0.0f,  1.0f},
            {"emissive", "Glow",      &m_emissive,   0.0f,  3.0f},
            {"wire",     "Wireframe", &m_wire,       0.0f,  1.0f},
            {"bob",      "Float",     &m_bob,        0.0f,  1.0f},
        };
    }
    std::map<std::string, AudioBinding>& audioBindings() { return m_audioBindings; }
    const std::map<std::string, AudioBinding>& audioBindings() const { return m_audioBindings; }
    void applyAudioBindings(float level, float bass, float mid, float high, float beat, float dt,
                            class MIDIManager* midi, float energy, float build, float drop,
                            float silence, float momentum);

    // PropertyPanel sets this to request a "change model" file dialog; the
    // Application loop (which owns the native picker) consumes + clears it.
    bool m_requestModelDialog = false;

    // The mesh program the compositor uses to draw this object in a 3D
    // zone (shared with the preview). Loaded lazily in init().
    ShaderProgram& meshShader() { return m_meshShader; }
    ShaderProgram& meshShadowShader() { return m_meshShadowShader; }
    bool shadersOk() const { return m_shadersOk; }

private:
    ObjMeshWarp   m_model;
    Framebuffer   m_output;        // preview (RGBA8 + depth)
    ShaderProgram m_meshShader;
    ShaderProgram m_meshShadowShader;
    bool          m_shadersOk = false;
    std::string   m_modelPath;
    glm::vec3     m_center{0.0f};
    float         m_extent = 1.0f;
    bool          m_hasModel = false;
    bool          m_suspended = false;
    int           m_size = 1024;
    double        m_lastTime = 0.0;
    float         m_spinAngle = 0.0f;   // degrees
    float         m_bobPhase = 0.0f;
    std::map<std::string, AudioBinding> m_audioBindings;

    void renderPreview();
};
