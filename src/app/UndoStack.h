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
    // Identity — restored verbatim so OutputZone::visibleLayerIds, DataBus
    // bindings, and the agent-SDK managed-slot lookup keep resolving across
    // undo/redo. A restored id of 0 would be handed a NEW id by Application's
    // per-frame fixup, silently orphaning every reference to the layer.
    uint32_t id = 0;
    std::string managedKey;
    uint32_t groupId = 0;

    std::string name;
    bool visible = true;
    bool userHidden = false;
    bool muted = false;
    bool soloed = false;
    float opacity = 1.0f;
    BlendMode blendMode = BlendMode::Normal;
    glm::vec2 position = {0.0f, 0.0f};
    glm::vec2 scale = {1.0f, 1.0f};
    float rotation = 0.0f;
    glm::vec2 anchor = {0.0f, 0.0f};
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
    bool autoCrop = false;
    bool autoCropDone = false;

    // Transitions — captured as-is, including in-flight progress, so a
    // mid-animation transition simply resumes after restore (simpler than
    // force-completing it, and the compositor drives it to completion the
    // same way either path ends). transitionShaderInst is NOT captured —
    // the compositor lazy-loads it from transitionShaderPath.
    TransitionType transitionType = TransitionType::Fade;
    float transitionDuration = 0.5f;
    float transitionProgress = 1.0f;
    bool transitionActive = false;
    bool transitionDirection = true;
    std::string transitionShaderPath;
    std::shared_ptr<ContentSource> nextSource; // shared, like source
    bool shaderTransitionActive = false;
    std::string glTransitionName;
    bool glTransitionActive = false;

    float feather = 0.0f;

    // Drop shadow
    bool dropShadowEnabled = false;
    float dropShadowOffsetX = 0.05f;
    float dropShadowOffsetY = 0.05f;
    float dropShadowBlur = 8.0f;
    float dropShadowOpacity = 0.7f;
    float dropShadowColorR = 0.0f;
    float dropShadowColorG = 0.0f;
    float dropShadowColorB = 0.0f;
    float dropShadowSpread = 1.0f;

    // Masks / effects / audio bindings — plain value copies. LayerMask's
    // texture shared_ptr rides along so a restored mask isn't blank while
    // MaskRenderer re-renders a dirty path.
    std::vector<Layer::LayerMask> masks;
    int activeMaskIndex = -1;
    std::vector<LayerEffect> effects;
    std::vector<Layer::AudioBinding> audioBindings;

    // Shader resolution override
    int shaderWidth = 0;
    int shaderHeight = 0;

#ifdef HAS_NDI
    // ndiSender itself is per-Layer runtime state (recreated fresh); the
    // name + enabled flag are what the user set.
    std::string ndiName;
    bool ndiEnabled = false;
#endif

    // Deliberately NOT captured (transient per-frame / editor-session state):
    // mosaicModeFrom/mosaicTransitionStart (ephemeral animation), the
    // betweenRow* fields (rewritten every frame by Timeline::applyToLayers),
    // and voiceControlEdit (panel edit mode).

    // Shader param values (empty if not a shader source)
    std::vector<std::variant<float, glm::vec4, bool, glm::vec2, std::string>> shaderParamValues;

    // Content source pointer (shared — not deep-copied)
    std::shared_ptr<ContentSource> source;
};

struct SceneSnapshot {
    std::vector<LayerSnapshot> layers;
    // Layer groups restored alongside the layers so every snapshotted
    // groupId resolves. Group ids are minted monotonically and never
    // reused, so an old map can't collide with groups created later.
    std::unordered_map<uint32_t, LayerGroup> groups;
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

    // Drop every snapshot. Should be called when the project context changes
    // wholesale (loadProject): loadProject resets m_nextLayerId to the NEW
    // project's max(id)+1, so a stale snapshot could restore an id ≥ that
    // counter and later collide with a freshly minted layer id. Within one
    // project session ids are monotonic and never reused, so preserved
    // snapshot ids can never collide there.
    void clear() { m_undoStack.clear(); m_redoStack.clear(); }

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
                    // Snapshots capture the queued transition "B" source too
                    // — park it the same way when nothing live holds it.
                    if (ls.nextSource && !liveSet.count(ls.nextSource.get())) {
                        ls.nextSource->suspend();
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
        snap.groups = stack.groups();
        for (int i = 0; i < stack.count(); i++) {
            const auto& layer = stack[i];
            LayerSnapshot ls;
            ls.id = layer->id;
            ls.managedKey = layer->managedKey;
            ls.groupId = layer->groupId;
            ls.name = layer->name;
            ls.visible = layer->visible;
            ls.userHidden = layer->userHidden;
            ls.muted = layer->muted;
            ls.soloed = layer->soloed;
            ls.opacity = layer->opacity;
            ls.blendMode = layer->blendMode;
            ls.position = layer->position;
            ls.scale = layer->scale;
            ls.rotation = layer->rotation;
            ls.anchor = layer->anchor;
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
            ls.autoCrop = layer->autoCrop;
            ls.autoCropDone = layer->autoCropDone;
            ls.transitionType = layer->transitionType;
            ls.transitionDuration = layer->transitionDuration;
            ls.transitionProgress = layer->transitionProgress;
            ls.transitionActive = layer->transitionActive;
            ls.transitionDirection = layer->transitionDirection;
            ls.transitionShaderPath = layer->transitionShaderPath;
            ls.nextSource = layer->nextSource;
            ls.shaderTransitionActive = layer->shaderTransitionActive;
            ls.glTransitionName = layer->glTransitionName;
            ls.glTransitionActive = layer->glTransitionActive;
            ls.feather = layer->feather;
            ls.dropShadowEnabled = layer->dropShadowEnabled;
            ls.dropShadowOffsetX = layer->dropShadowOffsetX;
            ls.dropShadowOffsetY = layer->dropShadowOffsetY;
            ls.dropShadowBlur = layer->dropShadowBlur;
            ls.dropShadowOpacity = layer->dropShadowOpacity;
            ls.dropShadowColorR = layer->dropShadowColorR;
            ls.dropShadowColorG = layer->dropShadowColorG;
            ls.dropShadowColorB = layer->dropShadowColorB;
            ls.dropShadowSpread = layer->dropShadowSpread;
            ls.masks = layer->masks;
            ls.activeMaskIndex = layer->activeMaskIndex;
            ls.effects = layer->effects;
            ls.audioBindings = layer->audioBindings;
            ls.shaderWidth = layer->shaderWidth;
            ls.shaderHeight = layer->shaderHeight;
#ifdef HAS_NDI
            ls.ndiName = layer->ndiName;
            ls.ndiEnabled = layer->ndiEnabled;
#endif
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

        stack.groups() = snap.groups;

        for (const auto& ls : snap.layers) {
            auto layer = std::make_shared<Layer>();
            // Identity first — a preserved nonzero id sails through
            // Application's per-frame `id == 0` fixup untouched, so zone
            // visibility sets and DataBus bindings keep pointing at it.
            layer->id = ls.id;
            layer->managedKey = ls.managedKey;
            layer->groupId = ls.groupId;
            layer->name = ls.name;
            layer->visible = ls.visible;
            layer->userHidden = ls.userHidden;
            layer->muted = ls.muted;
            layer->soloed = ls.soloed;
            layer->opacity = ls.opacity;
            layer->blendMode = ls.blendMode;
            layer->position = ls.position;
            layer->scale = ls.scale;
            layer->rotation = ls.rotation;
            layer->anchor = ls.anchor;
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
            layer->autoCrop = ls.autoCrop;
            layer->autoCropDone = ls.autoCropDone;
            layer->transitionType = ls.transitionType;
            layer->transitionDuration = ls.transitionDuration;
            layer->transitionProgress = ls.transitionProgress;
            layer->transitionActive = ls.transitionActive;
            layer->transitionDirection = ls.transitionDirection;
            layer->transitionShaderPath = ls.transitionShaderPath;
            layer->nextSource = ls.nextSource;
            layer->shaderTransitionActive = ls.shaderTransitionActive;
            layer->glTransitionName = ls.glTransitionName;
            layer->glTransitionActive = ls.glTransitionActive;
            layer->feather = ls.feather;
            layer->dropShadowEnabled = ls.dropShadowEnabled;
            layer->dropShadowOffsetX = ls.dropShadowOffsetX;
            layer->dropShadowOffsetY = ls.dropShadowOffsetY;
            layer->dropShadowBlur = ls.dropShadowBlur;
            layer->dropShadowOpacity = ls.dropShadowOpacity;
            layer->dropShadowColorR = ls.dropShadowColorR;
            layer->dropShadowColorG = ls.dropShadowColorG;
            layer->dropShadowColorB = ls.dropShadowColorB;
            layer->dropShadowSpread = ls.dropShadowSpread;
            layer->masks = ls.masks;
            layer->activeMaskIndex = ls.activeMaskIndex;
            layer->effects = ls.effects;
            layer->audioBindings = ls.audioBindings;
            layer->shaderWidth = ls.shaderWidth;
            layer->shaderHeight = ls.shaderHeight;
#ifdef HAS_NDI
            layer->ndiName = ls.ndiName;
            layer->ndiEnabled = ls.ndiEnabled;
#endif
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
