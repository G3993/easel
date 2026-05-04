#include "sources/ShaderRatings.h"

#include <nlohmann/json.hpp>
#include <fstream>
#include <sstream>
#include <filesystem>
#include <cstdlib>

namespace fs = std::filesystem;

std::string ShaderRatings::filePath() const {
    const char* home = std::getenv("HOME");
    if (!home) home = ".";
    fs::path dir = fs::path(home) / ".easel";
    std::error_code ec;
    fs::create_directories(dir, ec);
    return (dir / "shader_ratings.json").string();
}

void ShaderRatings::load() {
    std::ifstream f(filePath());
    if (!f) return;
    try {
        auto j = nlohmann::json::parse(f);
        m_ratings.clear();
        if (j.is_object()) {
            for (auto it = j.begin(); it != j.end(); ++it) {
                if (it.value().is_number_integer()) {
                    int v = (int)it.value();
                    if (v >= 0 && v <= 5) m_ratings[it.key()] = v;
                }
            }
        }
    } catch (...) {
        // Corrupt file — start fresh rather than crash.
        m_ratings.clear();
    }
}

void ShaderRatings::save() const {
    nlohmann::json j = nlohmann::json::object();
    for (const auto& kv : m_ratings) j[kv.first] = kv.second;
    std::ofstream f(filePath());
    if (f) f << j.dump(2);
}

int ShaderRatings::get(const std::string& shaderFile) const {
    auto it = m_ratings.find(shaderFile);
    return it == m_ratings.end() ? 0 : it->second;
}

void ShaderRatings::set(const std::string& shaderFile, int stars) {
    if (stars < 0) stars = 0;
    if (stars > 5) stars = 5;
    if (stars == 0) m_ratings.erase(shaderFile);
    else            m_ratings[shaderFile] = stars;
    save();
}

int ShaderRatings::needsImprovementCount() const {
    int n = 0;
    for (const auto& kv : m_ratings) {
        if (kv.second > 0 && kv.second < 5) n++;
    }
    return n;
}
