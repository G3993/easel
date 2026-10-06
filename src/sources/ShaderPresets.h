#pragma once
#include <string>
#include <unordered_map>
#include <vector>
#include <nlohmann/json.hpp>

class ShaderSource;

// Per-shader memory. Persisted to ~/.easel/shader_presets.json, one
// snapshot per shader file: every ISF input the user can set, plus the
// shader's reactivity bindings (SOUND / MIDI, with their ranges and
// smoothing) under the reserved "__bindings" key. Lu (2026-10-04): "any
// change I do on a shader, remember — as if every shader had its own
// memory", so Application::memoryTick captures continuously (remember())
// and every load path re-applies the snapshot (apply()), so a shader comes
// back exactly as it was last left, in the Canvas, a SHOW deck, or a
// timeline clip. Writes are debounced (tick()) and flushed on exit.
//
// Audio-driven float params are NOT captured by value: the engine writes
// the modulated value straight into the input every frame, so the
// binding itself (range = the sweep) is the intent, and the raw value is
// noise. MIDI-knob params ARE captured by value (the knob position is the
// choice) but the MidiCC bindings themselves are not: the knob map is
// surface state that follows the targeted deck.
class ShaderPresets {
public:
    void load();
    void save() const;

    // Automatic memory: snapshot `src`; if it differs from what is stored
    // for `shaderFile`, store it and mark the file dirty. Returns true when
    // something changed. Cheap enough to call a few times a second per
    // shader layer.
    bool remember(const std::string& shaderFile, const ShaderSource& src);
    // Write the file when it has been dirty for `settle` seconds.
    void tick(double now, double settle = 2.0);
    // Write now if dirty (exit, project close).
    void flush();
    // True when the stored snapshot carries a bindings list (even an empty
    // one): the user's reactivity choice for this shader is known, so a
    // fresh load must not invent a random recipe over it.
    bool hasBindings(const std::string& shaderFile) const;

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
    // Full memory shape: captureJson's values (minus audio-swept floats and
    // the voice-fed "msg" text) + "__bindings". applyMemory restores both.
    static nlohmann::json captureMemory(const ShaderSource& src);
    static int            applyMemory(const nlohmann::json& snap, ShaderSource& src);

    // Stored snapshot lookup (nullptr when none) — read-only.
    const nlohmann::json* findSnapshot(const std::string& shaderFile) const;

private:
    std::string filePath() const;
    // Per-shader → param-name → JSON value (float, [r,g,b,a], bool, [x,y], string)
    std::unordered_map<std::string, nlohmann::json> m_presets;
    bool   m_dirty  = false;
    double m_dirtyT = 0.0;
};
