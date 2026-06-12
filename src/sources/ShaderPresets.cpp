#include "sources/ShaderPresets.h"
#include "sources/ShaderSource.h"

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

int ShaderPresets::capture(const std::string& shaderFile, const ShaderSource& src) {
    json snap = json::object();
    int n = 0;
    for (const auto& in : src.inputs()) {
        if (in.type == "float" || in.type == "long") {
            if (std::holds_alternative<float>(in.value)) {
                snap[in.name] = std::get<float>(in.value);
                n++;
            }
        } else if (in.type == "color") {
            if (std::holds_alternative<glm::vec4>(in.value)) {
                const auto& v = std::get<glm::vec4>(in.value);
                snap[in.name] = { v.x, v.y, v.z, v.w };
                n++;
            }
        } else if (in.type == "bool" || in.type == "event") {
            if (std::holds_alternative<bool>(in.value)) {
                snap[in.name] = std::get<bool>(in.value);
                n++;
            }
        } else if (in.type == "point2D") {
            if (std::holds_alternative<glm::vec2>(in.value)) {
                const auto& v = std::get<glm::vec2>(in.value);
                snap[in.name] = { v.x, v.y };
                n++;
            }
        } else if (in.type == "text") {
            if (std::holds_alternative<std::string>(in.value)) {
                snap[in.name] = std::get<std::string>(in.value);
                n++;
            }
        }
    }
    m_presets[shaderFile] = snap;
    save();
    return n;
}

int ShaderPresets::apply(const std::string& shaderFile, ShaderSource& src) const {
    auto it = m_presets.find(shaderFile);
    if (it == m_presets.end()) return 0;
    const json& snap = it->second;
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

void ShaderPresets::clear(const std::string& shaderFile) {
    if (m_presets.erase(shaderFile) > 0) save();
}
