#include "AudioPresetEngine.h"
#include <algorithm>
#include <random>
#include <unordered_map>

namespace AudioPresetEngine {

static std::unordered_map<uint32_t, State> sState;
static std::mt19937 sRng{std::random_device{}()};

State& stateFor(uint32_t layerId) { return sState[layerId]; }

float characterKnobToConditioner(float k) {
    if (k <= -0.5f) return (k + 0.5f) * 2.0f;   // -1 … -0.5  →  -1 … 0
    return (k + 0.5f) / 1.5f;                    // -0.5 … +1  →   0 … +1
}

bool rebuild(std::map<std::string, AudioBinding>& bindings,
             const std::vector<Param>& params, uint32_t layerId) {
    State& st = sState[layerId];
    if (!st.recipe.has) return false;
    bindings.clear();
    for (auto& e : st.recipe.e) {
        if (e.idx < 0 || e.idx >= (int)params.size()) continue;
        const Param& pp = params[e.idx];
        float span = pp.hi - pp.lo; if (span <= 0.0f) continue;
        float half   = span * (0.08f + 0.42f * st.intensity) * 0.5f; // subtle..intense
        float center = pp.lo + e.center01 * span;
        float rmin = center - half, rmax = center + half;
        if (rmin < pp.lo) rmin = pp.lo;
        if (rmax > pp.hi) rmax = pp.hi;
        if (e.invert) { float t = rmin; rmin = rmax; rmax = t; } // loud -> down
        AudioBinding ab;
        ab.signal    = e.sig;
        ab.rangeMin  = rmin;
        ab.rangeMax  = rmax;
        ab.smoothing = 0.96f - 0.24f * st.intensity;   // syrupy..less syrupy (never strobey)
        ab.character = characterKnobToConditioner(st.character); // bus-wide 2nd knob
        bindings[pp.name] = ab;
    }
    return true;
}

bool shuffle(std::map<std::string, AudioBinding>& bindings,
             const std::vector<Param>& params, uint32_t layerId) {
    Recipe r; r.has = true;
    std::vector<int> idx;
    for (int i = 0; i < (int)params.size(); i++) idx.push_back(i);
    std::shuffle(idx.begin(), idx.end(), sRng);
    std::uniform_real_distribution<float> u01(0.0f, 1.0f);
    // Continuous drivers (skip the impulse/inverted Drop/Silence/Momentum).
    const AudioSignal cs[] = { AudioSignal::Level, AudioSignal::Bass,
                               AudioSignal::Mid,   AudioSignal::High,
                               AudioSignal::Energy, AudioSignal::Build };
    int n = std::min(5, (int)idx.size());
    for (int j = 0; j < n; j++) {
        RecipeEntry e;
        e.idx      = idx[j];
        e.sig      = cs[sRng() % 6];
        e.center01 = 0.25f + 0.50f * u01(sRng);   // random centre, room both ways
        e.invert   = (sRng() % 2 == 0);           // ~half fall as energy rises
        r.e.push_back(e);
    }
    sState[layerId].recipe = r;
    return rebuild(bindings, params, layerId);
}

void adoptExisting(const std::map<std::string, AudioBinding>& bindings,
                   const std::vector<Param>& params, uint32_t layerId) {
    State& st = sState[layerId];
    if (st.recipe.has) return;
    Recipe r;
    for (int i = 0; i < (int)params.size(); i++) {
        auto it = bindings.find(params[i].name);
        if (it == bindings.end() || it->second.signal == AudioSignal::None) continue;
        float span = params[i].hi - params[i].lo; if (span <= 0.0f) continue;
        float center = 0.5f * (it->second.rangeMin + it->second.rangeMax);
        RecipeEntry e; e.idx = i; e.sig = it->second.signal;
        e.center01 = (center - params[i].lo) / span;
        e.invert   = (it->second.rangeMin > it->second.rangeMax);
        r.e.push_back(e); r.has = true;
    }
    if (r.has) st.recipe = r;
}

bool retintCharacter(std::map<std::string, AudioBinding>& bindings, uint32_t layerId) {
    float c = characterKnobToConditioner(sState[layerId].character);
    for (auto& [n, ab] : bindings) ab.character = c;
    return !bindings.empty();
}

bool off(std::map<std::string, AudioBinding>& bindings, uint32_t layerId) {
    State& st = sState[layerId];
    bool had = st.recipe.has || !bindings.empty();
    bindings.clear();
    st.recipe = Recipe{};
    return had;
}

} // namespace AudioPresetEngine
