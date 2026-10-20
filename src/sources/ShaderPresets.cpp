#include "sources/ShaderPresets.h"
#include "sources/ShaderSource.h"
#include "sources/AudioBinding.h"

#include <fstream>
#include <filesystem>
#include <cstdlib>

namespace fs = std::filesystem;
using nlohmann::json;

std::string ShaderPresets::filePath() const {
    const char* home = std::getenv("HOME");
    if (!home) home = ".";
    fs::path dir = fs::path(home) / ".easel";
    std::error_code ec;
    fs::create_directories(dir, ec);
    return (dir / "shader_presets.json").string();
}

void ShaderPresets::load() {
    std::ifstream f(filePath());
    if (!f) return;
    try {
        auto j = json::parse(f);
        m_presets.clear();
        if (j.is_object()) {
            for (auto it = j.begin(); it != j.end(); ++it) {
                if (it.value().is_object()) m_presets[it.key()] = it.value();
            }
        }
    } catch (...) {
        m_presets.clear();
    }
}

void ShaderPresets::save() const {
    json j = json::object();
    for (const auto& kv : m_presets) j[kv.first] = kv.second;
    std::ofstream f(filePath());
    if (f) f << j.dump(2);
}

bool ShaderPresets::has(const std::string& shaderFile) const {
    return m_presets.find(shaderFile) != m_presets.end();
}

json ShaderPresets::captureJson(const ShaderSource& src) {
    json snap = json::object();
    for (const auto& in : src.inputs()) {
        if (in.type == "float" || in.type == "long") {
            if (std::holds_alternative<float>(in.value)) {
                snap[in.name] = std::get<float>(in.value);
            }
        } else if (in.type == "color") {
            if (std::holds_alternative<glm::vec4>(in.value)) {
                const auto& v = std::get<glm::vec4>(in.value);
                snap[in.name] = { v.x, v.y, v.z, v.w };
            }
        } else if (in.type == "bool" || in.type == "event") {
            if (std::holds_alternative<bool>(in.value)) {
                snap[in.name] = std::get<bool>(in.value);
            }
        } else if (in.type == "point2D") {
            if (std::holds_alternative<glm::vec2>(in.value)) {
                const auto& v = std::get<glm::vec2>(in.value);
                snap[in.name] = { v.x, v.y };
            }
        } else if (in.type == "text") {
            if (std::holds_alternative<std::string>(in.value)) {
                snap[in.name] = std::get<std::string>(in.value);
            }
        }
    }
    return snap;
}

int ShaderPresets::capture(const std::string& shaderFile, const ShaderSource& src) {
    json snap = captureMemory(src);
    m_presets[shaderFile] = snap;
    m_dirty = false;
    save();
    return (int)snap.size();
}

static const char* kBindingsKey = "__bindings";

json ShaderPresets::captureMemory(const ShaderSource& src) {
    json snap = captureJson(src);
    const auto& binds = src.audioBindings();
    // Audio-swept floats: the engine overwrites input.value every frame, so
    // the number is whatever the music was doing — drop it, keep the binding.
    for (const auto& kv : binds) {
        if (kv.second.signal == AudioSignal::None || kv.second.signal == AudioSignal::MidiCC) continue;
        snap.erase(kv.first);
    }
    // The voice-native text input is fed by the data bus (cue.latest), not
    // typed — remembering it would churn the file for every spoken word.
    snap.erase("msg");
    json bj = json::array();
    for (const auto& [name, ab] : binds) {
        if (ab.signal == AudioSignal::None) continue;
        // The XL knob map is surface state, not the shader's: it follows
        // whichever deck holds the knobs and is rebuilt on every deck take.
        // Remembering it would make a shader arriving on deck B answer the
        // same CCs as deck A. The knob VALUES above are remembered.
        if (ab.signal == AudioSignal::MidiCC) continue;
        json b;
        b["param"]     = name;
        b["signal"]    = (int)ab.signal;
        b["rangeMin"]  = ab.rangeMin;
        b["rangeMax"]  = ab.rangeMax;
        b["smoothing"] = ab.smoothing;
        b["character"] = ab.character;
        b["rampTime"]  = ab.rampTime;
        if (ab.gain != 1.0f) b["gain"] = ab.gain;
        if (ab.fall != 1.0f) b["fall"] = ab.fall;
        if (ab.curveOn) {
            b["curve"] = {ab.curveP1x, ab.curveP1y, ab.curveP2x, ab.curveP2y};
        }
        bj.push_back(b);
    }
    snap[kBindingsKey] = bj;
    return snap;
}

int ShaderPresets::applyMemory(const json& snap, ShaderSource& src) {
    int n = applyJson(snap, src);   // ignores "__bindings": no input has that name
    if (!snap.contains(kBindingsKey) || !snap.at(kBindingsKey).is_array()) return n;
    auto& binds = src.audioBindings();
    // Replace the SOUND bindings with the remembered ones; a live knob map
    // (MidiCC) on this source stays — it is not part of the memory.
    for (auto it = binds.begin(); it != binds.end();)
        if (it->second.signal != AudioSignal::MidiCC) it = binds.erase(it); else ++it;
    for (const auto& b : snap.at(kBindingsKey)) {
        std::string param = b.value("param", std::string());
        if (param.empty()) continue;
        AudioBinding ab;
        ab.signal      = (AudioSignal)b.value("signal", 0);
        ab.rangeMin    = b.value("rangeMin", 0.0f);
        ab.rangeMax    = b.value("rangeMax", 1.0f);
        ab.smoothing   = b.value("smoothing", ab.smoothing);
        ab.character   = b.value("character", ab.character);
        ab.rampTime    = b.value("rampTime", ab.rampTime);
        ab.gain        = b.value("gain", 1.0f);
        ab.fall        = b.value("fall", 1.0f);
        if (b.contains("curve") && b["curve"].is_array() && b["curve"].size() == 4) {
            ab.curveOn = true;
            ab.curveP1x = b["curve"][0].get<float>(); ab.curveP1y = b["curve"][1].get<float>();
            ab.curveP2x = b["curve"][2].get<float>(); ab.curveP2y = b["curve"][3].get<float>();
        }
        if (ab.signal == AudioSignal::MidiCC) continue;   // old files: knob maps are not memory
        binds[param] = ab;
        n++;
    }
    return n;
}

bool ShaderPresets::remember(const std::string& shaderFile, const ShaderSource& src) {
    if (shaderFile.empty()) return false;
    json snap = captureMemory(src);
    auto it = m_presets.find(shaderFile);
    if (it != m_presets.end() && it->second == snap) return false;
    m_presets[shaderFile] = std::move(snap);
    if (!m_dirty) { m_dirty = true; m_dirtyT = -1.0; }
    return true;
}

bool ShaderPresets::rememberJson(const std::string& key, json snap) {
    if (key.empty() || !snap.is_object()) return false;
    auto it = m_presets.find(key);
    if (it != m_presets.end() && it->second == snap) return false;
    m_presets[key] = std::move(snap);
    if (!m_dirty) { m_dirty = true; m_dirtyT = -1.0; }
    return true;
}

void ShaderPresets::tick(double now, double settle) {
    if (!m_dirty) return;
    if (m_dirtyT < 0.0) { m_dirtyT = now; return; }
    if (now - m_dirtyT >= settle) flush();
}

void ShaderPresets::flush() {
    if (!m_dirty) return;
    save();
    m_dirty = false;
}

bool ShaderPresets::hasBindings(const std::string& shaderFile) const {
    auto it = m_presets.find(shaderFile);
    return it != m_presets.end() && it->second.is_object() && it->second.contains(kBindingsKey);
}

int ShaderPresets::applyJson(const json& snap, ShaderSource& src) {
    int n = 0;
    for (auto& in : src.inputs()) {
        if (!snap.contains(in.name)) continue;
        const json& v = snap.at(in.name);
        try {
            if ((in.type == "float" || in.type == "long") && v.is_number()) {
                in.value = (float)v.get<double>();
                n++;
            } else if (in.type == "color" && v.is_array() && v.size() == 4) {
                in.value = glm::vec4(v[0].get<float>(), v[1].get<float>(),
                                     v[2].get<float>(), v[3].get<float>());
                n++;
            } else if ((in.type == "bool" || in.type == "event") && v.is_boolean()) {
                in.value = v.get<bool>();
                n++;
            } else if (in.type == "point2D" && v.is_array() && v.size() == 2) {
                in.value = glm::vec2(v[0].get<float>(), v[1].get<float>());
                n++;
            } else if (in.type == "text" && v.is_string()) {
                in.value = v.get<std::string>();
                n++;
            }
        } catch (...) {
            // mismatched type — leave default, skip.
        }
    }
    return n;
}

int ShaderPresets::apply(const std::string& shaderFile, ShaderSource& src) const {
    auto it = m_presets.find(shaderFile);
    if (it == m_presets.end()) return 0;
    return applyMemory(it->second, src);
}

const json* ShaderPresets::findSnapshot(const std::string& shaderFile) const {
    auto it = m_presets.find(shaderFile);
    return it == m_presets.end() ? nullptr : &it->second;
}

void ShaderPresets::clear(const std::string& shaderFile) {
    if (m_presets.erase(shaderFile) > 0) { m_dirty = false; save(); }
}
