#pragma once
#include <string>
#include <vector>
#include <nlohmann/json.hpp>

// Saved show presets: an ordered shader playlist + each shader's full ISF
// parameter snapshot + the show length. Persisted app-wide to
// ~/.easel/show_presets.json so a dialed-in show can be recalled in any
// project with one click from the gallery's SHOWS menu.
struct ShowPresetEntry {
    std::string    path;    // full shader path (ShaderClaw .fs)
    nlohmann::json params;  // param-name → value snapshot (ShaderPresets shape)
};

struct ShowPreset {
    std::string name;
    float       minutes = 10.0f;
    std::vector<ShowPresetEntry> entries;
};

class ShowPresets {
public:
    void load();
    void save() const;

    const std::vector<ShowPreset>& all() const { return m_shows; }
    const ShowPreset* find(const std::string& name) const;

    // Insert or replace by name. Persists immediately.
    void put(const ShowPreset& p);
    void remove(const std::string& name);

private:
    std::string filePath() const;
    std::vector<ShowPreset> m_shows;
};
