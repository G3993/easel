#pragma once
#include <string>
#include <unordered_map>
#include <vector>
#include <nlohmann/json.hpp>

class ShaderSource;

// Per-shader saved parameter snapshots. Persisted to
// ~/.easel/shader_presets.json. One snapshot per shader file (the latest
// "Save Params" wins). On loadShader, the snapshot is re-applied so the
// user's tweaked controls survive switching away and back.
class ShaderPresets {
public:
    void load();
    void save() const;

    // True if a preset exists for this shader file.
    bool has(const std::string& shaderFile) const;

    // Capture current ISF input values from `src` and store under
    // `shaderFile`. Persists immediately. Returns number of params captured.
    int  capture(const std::string& shaderFile, const ShaderSource& src);

    // Apply stored preset values to `src` (matching by input name).
    // No-op if no preset exists. Returns number of params applied.
    int  apply(const std::string& shaderFile, ShaderSource& src) const;

    // Drop any saved preset for `shaderFile` (e.g. on delete).
    void clear(const std::string& shaderFile);

    // Raw snapshot helpers — the same param-name → value shape the store
    // uses, but standalone so other systems (show presets, timeline clip
    // hot-swaps) can capture/apply without touching the persisted store.
    static nlohmann::json captureJson(const ShaderSource& src);
    static int            applyJson(const nlohmann::json& snap, ShaderSource& src);

    // Stored snapshot lookup (nullptr when none) — read-only.
    const nlohmann::json* findSnapshot(const std::string& shaderFile) const;

private:
    std::string filePath() const;
    // Per-shader → param-name → JSON value (float, [r,g,b,a], bool, [x,y], string)
    std::unordered_map<std::string, nlohmann::json> m_presets;
};
