#pragma once
#include "compositing/LayerStack.h"
#include "compositing/MaskPath.h"
#include "sources/ShaderSource.h"
#include "timeline/Timeline.h"
#include <vector>
#include <deque>
#include <unordered_set>
#include <nlohmann/json.hpp>

struct LayerSnapshot {
    std::string name;
    bool visible = true;
    float opacity = 1.0f;
    BlendMode blendMode = BlendMode::Normal;
    glm::vec2 position = {0.0f, 0.0f};
    glm::vec2 scale = {1.0f, 1.0f};
    float rotation = 0.0f;
    bool flipH = false;
    bool flipV = false;
    MosaicMode mosaicMode = MosaicMode::Mirror;
    float tileX = 1.0f, tileY = 1.0f;
    float mosaicDensity = 4.0f;
    float mosaicSpin = 0.0f;
    bool audioReactive = false;
    float audioStrength = 0.15f;
    float cropTop = 0.0f, cropBottom = 0.0f;
    float cropLeft = 0.0f, cropRight = 0.0f;

    // Shader param values (empty if not a shader source)
    std::vector<std::variant<float, glm::vec4, bool, glm::vec2, std::string>> shaderParamValues;

    // Content source pointer (shared — not deep-copied)
    std::shared_ptr<ContentSource> source;
};

struct SceneSnapshot {
    std::vector<LayerSnapshot> layers;
    int selectedLayer = -1;
    // Optional — captured when an undo is pushed alongside a Timeline ref.
    // Restored via Timeline::fromJson on undo/redo so clip/transition edits
    // (move, trim, add, delete) are reversible.
    nlohmann::json timelineJson;
    bool           hasTimeline = false;
};

class UndoStack {
public:
    void pushState(const LayerStack& stack, int selectedLayer) {
        SceneSnapshot snap = capture(stack, selectedLayer);
        m_undoStack.push_back(std::move(snap));
        if ((int)m_undoStack.size() > m_maxEntries) {
            m_undoStack.pop_front();
        }
        m_redoStack.clear();
    }

    // Timeline-aware push — snapshots both the layer stack AND the current
    // timeline state so clip/transition drags, adds, and deletes are undoable.
    void pushState(const LayerStack& stack, int selectedLayer, const Timeline& tl) {
        SceneSnapshot snap = capture(stack, selectedLayer);
        snap.timelineJson = tl.toJson();
        snap.hasTimeline  = true;
        m_undoStack.push_back(std::move(snap));
        if ((int)m_undoStack.size() > m_maxEntries) {
            m_undoStack.pop_front();
        }
        m_redoStack.clear();
    }

    // Push a pre-captured snapshot (used when we need to capture before edits happen)
    void pushSnapshot(SceneSnapshot snap) {
        m_undoStack.push_back(std::move(snap));
        if ((int)m_undoStack.size() > m_maxEntries) {
            m_undoStack.pop_front();
        }
        m_redoStack.clear();
    }

    // Capture a snapshot without pushing it (caller decides whether to push)
    static SceneSnapshot captureSnapshot(const LayerStack& stack, int selectedLayer) {
        return capture(stack, selectedLayer);
    }

    bool canUndo() const { return !m_undoStack.empty(); }
    bool canRedo() const { return !m_redoStack.empty(); }

    void undo(LayerStack& stack, int& selectedLayer) {
        if (m_undoStack.empty()) return;
        m_redoStack.push_back(capture(stack, selectedLayer));
        restore(m_undoStack.back(), stack, selectedLayer);
        m_undoStack.pop_back();
    }

    // Timeline-aware undo — restores both layer stack and timeline state.
    // Safe to call when the snapshot has no timeline data (falls back to
    // layer-only undo).
    void undo(LayerStack& stack, int& selectedLayer, Timeline& tl) {
        if (m_undoStack.empty()) return;
        SceneSnapshot cur = capture(stack, selectedLayer);
        cur.timelineJson = tl.toJson();
        cur.hasTimeline  = true;
        m_redoStack.push_back(std::move(cur));
        const auto& prev = m_undoStack.back();
        restore(prev, stack, selectedLayer);
        if (prev.hasTimeline) tl.fromJson(prev.timelineJson);
        m_undoStack.pop_back();
    }

    void redo(LayerStack& stack, int& selectedLayer) {
        if (m_redoStack.empty()) return;
        m_undoStack.push_back(capture(stack, selectedLayer));
        restore(m_redoStack.back(), stack, selectedLayer);
        m_redoStack.pop_back();
    }

    void redo(LayerStack& stack, int& selectedLayer, Timeline& tl) {
        if (m_redoStack.empty()) return;
        SceneSnapshot cur = capture(stack, selectedLayer);
        cur.timelineJson = tl.toJson();
        cur.hasTimeline  = true;
        m_undoStack.push_back(std::move(cur));
        const auto& next = m_redoStack.back();
        restore(next, stack, selectedLayer);
        if (next.hasTimeline) tl.fromJson(next.timelineJson);
        m_redoStack.pop_back();
    }

    // Snapshots share live source objects (shared_ptr, not deep-copied), so
    // a replaced/deleted layer's source used to stay FULLY ALIVE — running
    // video decode threads, open capture devices, NDI receivers — until 50
    // newer pushes rolled it off. Suspend any source that is reachable only
    // from undo/redo snapshots; sources lazily revive inside their own
    // update() if anything (undo restore, scene switch) puts them back in
    // the live stack. Called once per frame — suspend() on an
    // already-suspended source is a guarded no-op.
    void suspendOrphanedSources(const LayerStack& live) {
        if (m_undoStack.empty() && m_redoStack.empty()) return;
        std::unordered_set<const ContentSource*> liveSet;
        for (int i = 0; i < live.count(); i++) {
            if (!live[i]) continue;
            if (live[i]->source) liveSet.insert(live[i]->source.get());
            // A mid-transition layer renders nextSource every frame (the B
            // side of the blend) — it's just as live as source, and a
            // transition started from another layer's source can be the
            // only live holder after that donor layer is deleted.
            if (live[i]->nextSource) liveSet.insert(live[i]->nextSource.get());
        }
        auto sweep = [&](std::deque<SceneSnapshot>& dq) {
            for (auto& snap : dq) {
                for (auto& ls : snap.layers) {
                    if (ls.source && !liveSet.count(ls.source.get())) {
                        ls.source->suspend();
                    }
                }
            }
        };
        sweep(m_undoStack);
        sweep(m_redoStack);
    }

private:
    static constexpr int m_maxEntries = 50;
    std::deque<SceneSnapshot> m_undoStack;
    std::deque<SceneSnapshot> m_redoStack;

    static SceneSnapshot capture(const LayerStack& stack, int selectedLayer) {
        SceneSnapshot snap;
        snap.selectedLayer = selectedLayer;
        for (int i = 0; i < stack.count(); i++) {
            const auto& layer = stack[i];
            LayerSnapshot ls;
            ls.name = layer->name;
            ls.visible = layer->visible;
            ls.opacity = layer->opacity;
            ls.blendMode = layer->blendMode;
            ls.position = layer->position;
            ls.scale = layer->scale;
            ls.rotation = layer->rotation;
            ls.flipH = layer->flipH;
            ls.flipV = layer->flipV;
            ls.mosaicMode = layer->mosaicMode;
            ls.tileX = layer->tileX;
            ls.tileY = layer->tileY;
            ls.mosaicDensity = layer->mosaicDensity;
            ls.mosaicSpin = layer->mosaicSpin;
            ls.audioReactive = layer->audioReactive;
            ls.audioStrength = layer->audioStrength;
            ls.cropTop = layer->cropTop;
            ls.cropBottom = layer->cropBottom;
            ls.cropLeft = layer->cropLeft;
            ls.cropRight = layer->cropRight;
            ls.source = layer->source;

            // Snapshot shader param values
            if (layer->source && layer->source->isShader()) {
                auto* ss = static_cast<ShaderSource*>(layer->source.get());
                for (const auto& input : ss->inputs()) {
                    ls.shaderParamValues.push_back(input.value);
                }
            }

            snap.layers.push_back(std::move(ls));
        }
        return snap;
    }

    static void restore(const SceneSnapshot& snap, LayerStack& stack, int& selectedLayer) {
        // Rebuild layer stack from snapshot
        while (stack.count() > 0) {
            stack.removeLayer(0);
        }

        for (const auto& ls : snap.layers) {
            auto layer = std::make_shared<Layer>();
            layer->name = ls.name;
            layer->visible = ls.visible;
            layer->opacity = ls.opacity;
            layer->blendMode = ls.blendMode;
            layer->position = ls.position;
            layer->scale = ls.scale;
            layer->rotation = ls.rotation;
            layer->flipH = ls.flipH;
            layer->flipV = ls.flipV;
            layer->mosaicMode = ls.mosaicMode;
            layer->tileX = ls.tileX;
            layer->tileY = ls.tileY;
            layer->mosaicDensity = ls.mosaicDensity;
            layer->mosaicSpin = ls.mosaicSpin;
            layer->audioReactive = ls.audioReactive;
            layer->audioStrength = ls.audioStrength;
            layer->cropTop = ls.cropTop;
            layer->cropBottom = ls.cropBottom;
            layer->cropLeft = ls.cropLeft;
            layer->cropRight = ls.cropRight;
            layer->source = ls.source;

            // Restore shader param values
            if (layer->source && layer->source->isShader() && !ls.shaderParamValues.empty()) {
                auto* ss = static_cast<ShaderSource*>(layer->source.get());
                auto& inputs = ss->inputs();
                for (int i = 0; i < (int)ls.shaderParamValues.size() && i < (int)inputs.size(); i++) {
                    inputs[i].value = ls.shaderParamValues[i];
                }
            }

            stack.addLayer(layer);
        }

        selectedLayer = snap.selectedLayer;
    }
};
