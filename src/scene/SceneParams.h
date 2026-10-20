#pragma once
// ─────────────────────────────────────────────────────────────────────────
// Native 3D scene — the per-zone scene state.
//
// When a zone's 3D toggle is on, the zone's compositor stops stacking flat
// layers and instead stands every layer up as a CARD in a world with one
// camera and one sun. The camera starts orthographic so the first 3D frame
// is pixel-identical to the 2D composite; tilt it, or switch it to
// perspective, and the cards gain depth. Lighting is off by default (every
// card lit flat = identity); on, the sun lights the cards and casts a soft
// shadow map across them.
//
// This header is plain data with JSON in/out: no GL, no app dependency, so
// Palette can link it as well as Easel.
// ─────────────────────────────────────────────────────────────────────────
#include "sources/AudioBinding.h"
#include <glm/glm.hpp>
#include <nlohmann/json.hpp>
#include <string>
#include <vector>
#include <map>
#include <functional>
#include <algorithm>

struct SceneCameraParams {
    bool  perspective = false;   // false = orthographic (the 2D view), true = perspective
    float azimuth     = 0.0f;    // degrees, orbit around the target's Y axis (0 = straight on)
    float elevation   = 0.0f;    // degrees, orbit above (+) / below (-) the target
    float distance    = 3.0f;    // world units from the target (the canvas is 2 units tall)
    float fovDeg      = 0.0f;    // 0 = auto: the fov that frames the canvas exactly at `distance`
    glm::vec3 target  = glm::vec3(0.0f);   // orbit centre (world units; canvas centre = 0)
    float roll        = 0.0f;    // degrees, screen roll
    float dolly       = 0.0f;    // 0 = frame the canvas exactly; +/- = zoom in/out (ortho AND perspective)
};

struct SceneLightParams {
    bool  enabled   = false;     // lighting off = flat cards (identity with 2D)
    float azimuth   = -30.0f;    // degrees, sun direction around Y
    float elevation = 50.0f;     // degrees above the canvas plane
    float intensity = 1.0f;
    glm::vec3 color = glm::vec3(1.0f);
    float ambient   = 0.35f;     // flat fill so unlit faces never go black
    glm::vec3 ambientColor = glm::vec3(1.0f);
    bool  shadows   = true;
    float shadowStrength = 0.8f; // 0 = no darkening, 1 = full
    float shadowSoft = 1.5f;     // PCF radius in shadow-map texels
    int   shadowRes = 2048;      // shadow map size (Preview 1024 / HD 2048 / Ultra 4096)
};

// Distance fog over the whole world (cards, meshes, volumes, particles):
// amount = 1 - exp(-((d - start) * density)^2), d = distance from the eye.
struct SceneFogParams {
    bool  enabled = false;
    glm::vec3 color = glm::vec3(0.05f, 0.06f, 0.09f);
    float density = 0.25f;   // 0 = clear .. 1.5 = soup (the canvas sits ~3 units from the default eye)
    float start   = 2.0f;    // world units before the fog begins
};

// Screen-space ambient occlusion from the shared scene depth (half res).
struct SceneAOParams {
    bool  enabled  = false;
    float strength = 1.0f;    // 0 = off .. 2 = heavy
    float radius   = 0.25f;   // world units (the canvas is 2 tall)
};

// A point light placed in the world (world units: the canvas is 2 tall).
struct ScenePointLight {
    bool  enabled = false;
    glm::vec3 pos = glm::vec3(0.0f, 0.6f, 1.2f);
    glm::vec3 color = glm::vec3(1.0f);
    float intensity = 1.0f;
    float radius = 3.0f;         // falloff radius
};

// One bindable scene parameter (audio / MIDI / Show knobs / OSC).
struct SceneParamDef {
    const char* id;      // stable key: bindings, project file, OSC
    const char* label;
    float* ptr;
    float lo, hi;
};

struct SceneParams {
    // Timeline keyframes address "L<id>.<param>"; a zone's scene borrows an
    // id block no layer can reach ('S' << 24 + zone index) so camera and
    // light params ride the same Automation lanes as layer opacity.
    static uint32_t laneId(int zoneIndex) { return 0x53000000u + (uint32_t)(zoneIndex < 0 ? 0 : zoneIndex); }
    static bool isSceneLane(uint32_t id) { return (id & 0xFF000000u) == 0x53000000u; }
    static int  laneZone(uint32_t id) { return (int)(id & 0x00FFFFFFu); }
    static constexpr int kMaxPointLights = 4;
    bool enabled = false;        // the zone's 3D toggle
    int  tier = 1;               // 0 Preview, 1 HD, 2 Ultra — scales shadow res / PCF taps
    SceneCameraParams camera;
    SceneLightParams  light;
    SceneFogParams    fog;
    SceneAOParams     ao;
    ScenePointLight   pointLights[kMaxPointLights];
    // Per-parameter audio/MIDI bindings, keyed by SceneParamDef::id — the
    // exact binding model shaders and fluids use (sparkle popover, ranges,
    // smoothing). A bound param is owned by the binding every frame.
    std::map<std::string, AudioBinding> audioBindings;

    // The bindable float table. Pointers into this object: rebuild after a copy.
    std::vector<SceneParamDef> params() {
        std::vector<SceneParamDef> t = {
            {"cam.azimuth",   "Orbit",          &camera.azimuth,   -180.0f, 180.0f},
            {"cam.elevation", "Elevation",      &camera.elevation,  -89.0f,  89.0f},
            {"cam.distance",  "Distance",       &camera.distance,     0.5f,  20.0f},
            {"cam.fov",       "Field of view",  &camera.fovDeg,       0.0f, 120.0f},
            {"cam.roll",      "Roll",           &camera.roll,      -180.0f, 180.0f},
            {"cam.dolly",     "Zoom",           &camera.dolly,       -2.0f,   2.0f},
            {"cam.targetX",   "Target X",       &camera.target.x,    -3.0f,   3.0f},
            {"cam.targetY",   "Target Y",       &camera.target.y,    -3.0f,   3.0f},
            {"cam.targetZ",   "Target Z",       &camera.target.z,    -3.0f,   3.0f},
            {"sun.azimuth",   "Sun azimuth",    &light.azimuth,    -180.0f, 180.0f},
            {"sun.elevation", "Sun elevation",  &light.elevation,   -89.0f,  89.0f},
            {"sun.intensity", "Sun intensity",  &light.intensity,     0.0f,   3.0f},
            {"sun.ambient",   "Ambient",        &light.ambient,       0.0f,   1.5f},
            {"sun.shadowStrength", "Shadow strength", &light.shadowStrength, 0.0f, 1.0f},
            {"sun.shadowSoft", "Shadow softness", &light.shadowSoft,  0.0f,   6.0f},
            {"fog.density",   "Fog density",    &fog.density,         0.0f,   1.5f},
            {"fog.start",     "Fog start",      &fog.start,           0.0f,  10.0f},
            {"ao.strength",   "AO strength",    &ao.strength,         0.0f,   2.0f},
            {"ao.radius",     "AO radius",      &ao.radius,           0.05f,  1.0f},
        };
        static const char* kIds[kMaxPointLights][5] = {
            {"pl1.x", "pl1.y", "pl1.z", "pl1.intensity", "pl1.radius"},
            {"pl2.x", "pl2.y", "pl2.z", "pl2.intensity", "pl2.radius"},
            {"pl3.x", "pl3.y", "pl3.z", "pl3.intensity", "pl3.radius"},
            {"pl4.x", "pl4.y", "pl4.z", "pl4.intensity", "pl4.radius"},
        };
        for (int i = 0; i < kMaxPointLights; i++) {
            auto& L = pointLights[i];
            t.push_back({kIds[i][0], "X",         &L.pos.x,     -4.0f, 4.0f});
            t.push_back({kIds[i][1], "Y",         &L.pos.y,     -4.0f, 4.0f});
            t.push_back({kIds[i][2], "Z",         &L.pos.z,     -4.0f, 4.0f});
            t.push_back({kIds[i][3], "Intensity", &L.intensity,  0.0f, 6.0f});
            t.push_back({kIds[i][4], "Radius",    &L.radius,     0.1f, 10.0f});
        }
        return t;
    }

    // Drive every bound parameter from the current audio frame. `midiCC`
    // returns the CC value in 0..1 or a negative number when unknown.
    void applyAudioBindings(float level, float bass, float mid, float high, float beat, float dt,
                            float energy, float build, float drop, float silence, float momentum,
                            const std::function<float(int channel, int cc)>& midiCC) {
        if (audioBindings.empty()) return;
        auto table = params();
        for (auto& [name, b] : audioBindings) {
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
                    float v = (midiCC && b.midiCC >= 0) ? midiCC(b.midiChannel, b.midiCC) : -1.0f;
                    if (v < 0.0f) have = false; else raw = v;
                    break;
                }
                default: have = false; break;
            }
            if (!have) continue;
            float mapped = b.follow(raw, dt);
            for (const auto& d : table) {
                if (name != d.id) continue;
                *d.ptr = std::max(d.lo, std::min(d.hi, mapped));
            }
        }
    }
    // Backdrop drawn behind the cards when 3D is on (transparent by default
    // so a 3D zone composites exactly like a 2D one).
    glm::vec4 background = glm::vec4(0.0f);

    nlohmann::json toJson() const {
        nlohmann::json j;
        j["enabled"] = enabled;
        j["tier"] = tier;
        j["background"] = {background.r, background.g, background.b, background.a};
        nlohmann::json c;
        c["perspective"] = camera.perspective;
        c["azimuth"] = camera.azimuth;
        c["elevation"] = camera.elevation;
        c["distance"] = camera.distance;
        c["fovDeg"] = camera.fovDeg;
        c["target"] = {camera.target.x, camera.target.y, camera.target.z};
        c["roll"] = camera.roll;
        c["dolly"] = camera.dolly;
        j["camera"] = c;
        nlohmann::json l;
        l["enabled"] = light.enabled;
        l["azimuth"] = light.azimuth;
        l["elevation"] = light.elevation;
        l["intensity"] = light.intensity;
        l["color"] = {light.color.r, light.color.g, light.color.b};
        l["ambient"] = light.ambient;
        l["ambientColor"] = {light.ambientColor.r, light.ambientColor.g, light.ambientColor.b};
        l["shadows"] = light.shadows;
        l["shadowStrength"] = light.shadowStrength;
        l["shadowSoft"] = light.shadowSoft;
        l["shadowRes"] = light.shadowRes;
        j["light"] = l;
        nlohmann::json pls = nlohmann::json::array();
        for (int i = 0; i < kMaxPointLights; i++) {
            const auto& L = pointLights[i];
            nlohmann::json pj;
            pj["enabled"] = L.enabled;
            pj["pos"] = {L.pos.x, L.pos.y, L.pos.z};
            pj["color"] = {L.color.r, L.color.g, L.color.b};
            pj["intensity"] = L.intensity;
            pj["radius"] = L.radius;
            pls.push_back(pj);
        }
        j["pointLights"] = pls;
        nlohmann::json abs = nlohmann::json::array();
        for (const auto& [name, ab] : audioBindings) {
            if (ab.signal == AudioSignal::None) continue;
            nlohmann::json abj;
            abj["param"] = name;
            abj["signal"] = (int)ab.signal;
            abj["rangeMin"] = ab.rangeMin;
            abj["rangeMax"] = ab.rangeMax;
            abj["smoothing"] = ab.smoothing;
            if (ab.character != 0.0f) abj["character"] = ab.character;
            if (ab.signal == AudioSignal::MidiCC) { abj["midiCC"] = ab.midiCC; abj["midiChannel"] = ab.midiChannel; }
            abs.push_back(abj);
        }
        if (!abs.empty()) j["audioBindings"] = abs;
        {
            nlohmann::json f;
            f["enabled"] = fog.enabled;
            f["color"] = {fog.color.r, fog.color.g, fog.color.b};
            f["density"] = fog.density;
            f["start"] = fog.start;
            j["fog"] = f;
        }
        {
            nlohmann::json a;
            a["enabled"] = ao.enabled;
            a["strength"] = ao.strength;
            a["radius"] = ao.radius;
            j["ao"] = a;
        }
        return j;
    }
    static SceneParams fromJson(const nlohmann::json& j) {
        SceneParams s;
        if (!j.is_object()) return s;
        s.enabled = j.value("enabled", false);
        s.tier = j.value("tier", 1);
        if (j.contains("background") && j["background"].is_array() && j["background"].size() == 4)
            s.background = glm::vec4(j["background"][0].get<float>(), j["background"][1].get<float>(),
                                     j["background"][2].get<float>(), j["background"][3].get<float>());
        if (j.contains("camera") && j["camera"].is_object()) {
            const auto& c = j["camera"];
            s.camera.perspective = c.value("perspective", false);
            s.camera.azimuth = c.value("azimuth", 0.0f);
            s.camera.elevation = c.value("elevation", 0.0f);
            s.camera.distance = c.value("distance", 3.0f);
            s.camera.fovDeg = c.value("fovDeg", 0.0f);
            if (c.contains("target") && c["target"].is_array() && c["target"].size() == 3)
                s.camera.target = glm::vec3(c["target"][0].get<float>(), c["target"][1].get<float>(), c["target"][2].get<float>());
            s.camera.roll = c.value("roll", 0.0f);
            s.camera.dolly = c.value("dolly", 0.0f);
        }
        if (j.contains("light") && j["light"].is_object()) {
            const auto& l = j["light"];
            s.light.enabled = l.value("enabled", false);
            s.light.azimuth = l.value("azimuth", -30.0f);
            s.light.elevation = l.value("elevation", 50.0f);
            s.light.intensity = l.value("intensity", 1.0f);
            if (l.contains("color") && l["color"].is_array() && l["color"].size() == 3)
                s.light.color = glm::vec3(l["color"][0].get<float>(), l["color"][1].get<float>(), l["color"][2].get<float>());
            s.light.ambient = l.value("ambient", 0.35f);
            if (l.contains("ambientColor") && l["ambientColor"].is_array() && l["ambientColor"].size() == 3)
                s.light.ambientColor = glm::vec3(l["ambientColor"][0].get<float>(), l["ambientColor"][1].get<float>(), l["ambientColor"][2].get<float>());
            s.light.shadows = l.value("shadows", true);
            s.light.shadowStrength = l.value("shadowStrength", 0.8f);
            s.light.shadowSoft = l.value("shadowSoft", 1.5f);
            s.light.shadowRes = l.value("shadowRes", 2048);
        }
        if (j.contains("fog") && j["fog"].is_object()) {
            const auto& f = j["fog"];
            s.fog.enabled = f.value("enabled", false);
            if (f.contains("color") && f["color"].is_array() && f["color"].size() >= 3)
                s.fog.color = glm::vec3(f["color"][0].get<float>(), f["color"][1].get<float>(), f["color"][2].get<float>());
            s.fog.density = f.value("density", 0.25f);
            s.fog.start = f.value("start", 2.0f);
        }
        if (j.contains("ao") && j["ao"].is_object()) {
            const auto& a = j["ao"];
            s.ao.enabled = a.value("enabled", false);
            s.ao.strength = a.value("strength", 1.0f);
            s.ao.radius = a.value("radius", 0.25f);
        }
        if (j.contains("pointLights") && j["pointLights"].is_array()) {
            int i = 0;
            for (const auto& pj : j["pointLights"]) {
                if (i >= kMaxPointLights) break;
                auto& L = s.pointLights[i++];
                L.enabled = pj.value("enabled", false);
                if (pj.contains("pos") && pj["pos"].is_array() && pj["pos"].size() == 3)
                    L.pos = glm::vec3(pj["pos"][0].get<float>(), pj["pos"][1].get<float>(), pj["pos"][2].get<float>());
                if (pj.contains("color") && pj["color"].is_array() && pj["color"].size() == 3)
                    L.color = glm::vec3(pj["color"][0].get<float>(), pj["color"][1].get<float>(), pj["color"][2].get<float>());
                L.intensity = pj.value("intensity", 1.0f);
                L.radius = pj.value("radius", 3.0f);
            }
        }
        if (j.contains("audioBindings") && j["audioBindings"].is_array()) {
            for (const auto& abj : j["audioBindings"]) {
                std::string name = abj.value("param", std::string());
                if (name.empty()) continue;
                AudioBinding ab;
                ab.signal = (AudioSignal)abj.value("signal", 0);
                ab.rangeMin = abj.value("rangeMin", 0.0f);
                ab.rangeMax = abj.value("rangeMax", 1.0f);
                ab.smoothing = abj.value("smoothing", 0.8f);
                ab.character = abj.value("character", 0.0f);
                ab.midiCC = abj.value("midiCC", -1);
                ab.midiChannel = abj.value("midiChannel", -1);
                s.audioBindings[name] = ab;
            }
        }
        return s;
    }
};
