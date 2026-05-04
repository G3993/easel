#pragma once
#include <string>
#include <unordered_map>

// Per-shader 1–5 star ratings. Persisted to ~/.easel/shader_ratings.json so
// they survive ShaderClaw3 pulls (the manifest can be overwritten by upstream
// without losing the user's grades).
//
// Goal: anything below 5 stars is a candidate for improvement. Use the
// `belowFive()` view to drive an "improve next" worklist.
class ShaderRatings {
public:
    void load();
    void save() const;

    // Rating in [0..5]. 0 = unrated. set() persists immediately.
    int  get(const std::string& shaderFile) const;
    void set(const std::string& shaderFile, int stars);

    // Number of shaders rated < 5 (and > 0). Excludes unrated.
    int needsImprovementCount() const;

private:
    std::string filePath() const;
    std::unordered_map<std::string, int> m_ratings;
};
