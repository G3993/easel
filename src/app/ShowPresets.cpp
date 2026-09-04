#include "app/ShowPresets.h"

#include <fstream>
#include <filesystem>
#include <cstdlib>

namespace fs = std::filesystem;
using nlohmann::json;

std::string ShowPresets::filePath() const {
    const char* home = std::getenv("HOME");
    if (!home) home = ".";
    fs::path dir = fs::path(home) / ".easel";
    std::error_code ec;
    fs::create_directories(dir, ec);
    return (dir / "show_presets.json").string();
}

void ShowPresets::load() {
    m_shows.clear();
    std::ifstream f(filePath());
    if (!f) return;
    try {
        auto j = json::parse(f);
        if (!j.is_array()) return;
        for (const auto& sj : j) {
            ShowPreset p;
            p.name    = sj.value("name", std::string{});
            p.minutes = sj.value("minutes", 10.0f);
            if (p.name.empty()) continue;
            if (sj.contains("entries") && sj["entries"].is_array()) {
                for (const auto& ej : sj["entries"]) {
                    ShowPresetEntry e;
                    e.path = ej.value("path", std::string{});
                    if (e.path.empty()) continue;
                    if (ej.contains("params") && ej["params"].is_object())
                        e.params = ej["params"];
                    else
                        e.params = json::object();
                    p.entries.push_back(std::move(e));
                }
            }
            if (!p.entries.empty()) m_shows.push_back(std::move(p));
        }
    } catch (...) {
        m_shows.clear();
    }
}

void ShowPresets::save() const {
    json j = json::array();
    for (const auto& p : m_shows) {
        json sj;
        sj["name"]    = p.name;
        sj["minutes"] = p.minutes;
        json ea = json::array();
        for (const auto& e : p.entries) {
            json ej;
            ej["path"]   = e.path;
            ej["params"] = e.params;
            ea.push_back(ej);
        }
        sj["entries"] = ea;
        j.push_back(sj);
    }
    std::ofstream f(filePath());
    if (f) f << j.dump(2);
}

const ShowPreset* ShowPresets::find(const std::string& name) const {
    for (const auto& p : m_shows)
        if (p.name == name) return &p;
    return nullptr;
}

void ShowPresets::put(const ShowPreset& p) {
    for (auto& q : m_shows) {
        if (q.name == p.name) { q = p; save(); return; }
    }
    m_shows.push_back(p);
    save();
}

void ShowPresets::remove(const std::string& name) {
    for (size_t i = 0; i < m_shows.size(); i++) {
        if (m_shows[i].name == name) {
            m_shows.erase(m_shows.begin() + i);
            save();
            return;
        }
    }
}
