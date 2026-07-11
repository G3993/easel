#pragma once
#include "AudioBinding.h"
#include <cstdint>
#include <map>
#include <string>
#include <vector>

// The master audio-reactivity "recipe" behind the PropertyPanel's
// Reactivity/Character/Shuffle/Off row, factored out of the panel's function
// statics so a headless caller (the OSC remote — mobile's Audio Reactivity
// controls) drives the exact same per-layer state the UI shows. One recipe
// per layer id: WHICH params react (random pick), to WHICH signal, centred
// where — plus the two knobs (intensity, character) that re-scale the live
// bindings without re-randomizing.
namespace AudioPresetEngine {

// A bindable parameter: current value + range. Callers build this list from
// their source (ShaderSource ISF float inputs, FluidSource's curated members).
struct Param { std::string name; float cur, lo, hi; };

struct RecipeEntry { int idx; AudioSignal sig; float center01; bool invert; };
struct Recipe { std::vector<RecipeEntry> e; bool has = false; };

struct State {
    Recipe recipe;
    // Off stashes the recipe here so On can restore the SAME set — On/Off is
    // a true toggle, not "Off then re-roll".
    Recipe lastRecipe;
    // Start subtle: low intensity = shallow modulation + heavy smoothing.
    // Character default -0.5 = conditioner-neutral = the classic feel.
    float intensity = 0.22f;
    float character = -0.5f;
};

// Per-layer state, created on first touch with the calm defaults above.
State& stateFor(uint32_t layerId);

// The user-facing Character knob (-1 smooth … +1 chopped, default -0.5) maps
// onto the conditioning block so the default lands on conditioner-neutral 0.
float characterKnobToConditioner(float k);

// Rebuild `bindings` from the layer's recipe at its current intensity/
// character. Returns false (bindings untouched) when there is no recipe.
bool rebuild(std::map<std::string, AudioBinding>& bindings,
             const std::vector<Param>& params, uint32_t layerId);

// Pick a fresh random recipe (~5 params, continuous signals, random centres,
// ~half inverted) and build it. Returns true when bindings changed.
bool shuffle(std::map<std::string, AudioBinding>& bindings,
             const std::vector<Param>& params, uint32_t layerId);

// After a project reload the bindings persist but the recipe is gone —
// derive one from the existing bindings so the knobs can still re-scale.
// No-op when the layer already has a recipe.
void adoptExisting(const std::map<std::string, AudioBinding>& bindings,
                   const std::vector<Param>& params, uint32_t layerId);

// Retint existing (manual) bindings with the layer's current character —
// the no-recipe path of the Character knob. Returns true if any changed.
bool retintCharacter(std::map<std::string, AudioBinding>& bindings, uint32_t layerId);

// Clear bindings + recipe (stashing the recipe for on()). Returns true when
// there was anything to clear.
bool off(std::map<std::string, AudioBinding>& bindings, uint32_t layerId);

// Turn audio reactivity ON: restore the recipe stashed by off() (same set),
// or shuffle a fresh one if this layer never had a recipe. No-op (returns
// false) when already on. The panel's On button + OSC 'on' command.
bool on(std::map<std::string, AudioBinding>& bindings,
        const std::vector<Param>& params, uint32_t layerId);

} // namespace AudioPresetEngine
