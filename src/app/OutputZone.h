#pragma once
#include "compositing/CompositeEngine.h"
#include "render/Framebuffer.h"
#include "app/AudioAnalyzer.h"
#ifdef HAS_NDI
#include "app/NDIOutput.h"
#endif
#ifdef HAS_SPOUT
#include "app/SpoutIO.h"
#endif
#include <string>
#include <set>
#include <cstdint>
#include <vector>
#include <memory>

enum class OutputDest { None, Fullscreen, NDI, Spout };

struct OutputZone {
    std::string name = "Main";
    int width = 1920;
    int height = 1080;
    int compPreset = 0; // resolution preset index

    CompositeEngine compositor;
    Framebuffer warpFBO;
    Framebuffer readbackFBO; // vertically-flipped copy of warpFBO for CPU readback
    GLuint canvasTexture = 0; // post-mask, pre-warp texture for canvas preview

    // Per-zone scratch FBOs for the composite/warp post chain. These used to
    // be Application members shared across zones — with two zones of
    // different resolutions composited per frame, every size check failed
    // every frame and each scratch FBO was destroyed + reallocated (tens of
    // MB of GL_RGBA16F churn per frame, plus the implicit sync of each
    // realloc). Per-zone they allocate once and stay stable.
    Framebuffer warpSSFBO;                       // supersampled warp target
    Framebuffer bloomBrightFBO;                  // half-res, 16F
    Framebuffer bloomPingFBO[2];                 // half-res, 16F
    Framebuffer bloomCompositeFBO;               // full-res, 16F
    Framebuffer postFBO;                         // edge-blend / mask-clip scratch
    Framebuffer maskUnionFBO;                    // multi-mask union scratch
    // Frame stamps per scratch group (0=warpSS 1=bloom 2=post 3=maskUnion);
    // groups idle long enough are freed by releaseIdleScratch().
    uint64_t scratchUsedFrame[4] = {0, 0, 0, 0};
    void releaseIdleScratch(uint64_t frame);

    // GPU-side allocation is deferred until the zone is first composited so
    // zones that are never consumed (unrouted, never previewed) hold no VRAM.
    bool gpuReady = false;
    bool ensureGpu();

    // Mapping profile index (-1 = none/passthrough, >=0 = index into Application::m_mappings)
    int mappingIndex = 0;

    // Layer visibility
    bool showAllLayers = true;
    std::set<uint32_t> visibleLayerIds;

    // Output routing
    OutputDest outputDest = OutputDest::None;
    int outputMonitor = -1;         // for Fullscreen: which monitor index
    std::string ndiStreamName;      // for NDI: stream name (defaults to zone name)
    // When true, the NDI sender broadcasts ndiStreamName verbatim (no "Easel - "
    // prefix) so agent-driven composite feeds get a caller-chosen name.
    bool rawNdiName = false;
    // Name the live NDI sender was created with — compared each frame so a
    // rename (e.g. agent ensureZoneNdi on an already-live zone) recreates
    // the sender instead of silently keeping the old wire name.
    std::string ndiActiveName;

    // --- Per-zone microphone (push-to-talk) ---------------------------------
    // Lets multi-floor/multi-room installs give each zone its own independent
    // live mic input instead of sharing the single global AudioAnalyzer.
    // micEnabled = the zone is configured to use its own mic (persisted).
    // pushToTalkActive = the mic is currently gated open (transient runtime
    // state, driven locally or remotely via OSC/SDK/mobile app — not saved).
    std::string micDeviceId;        // capture device endpoint id; empty = system default
    bool micEnabled = false;
    bool pushToTalkActive = false;
    AudioAnalyzer micAnalyzer;      // lazily initialized when first enabled

#ifdef HAS_NDI
    NDIOutput ndiOutput;            // per-zone NDI sender
#endif
#ifdef HAS_SPOUT
    EaselSpoutSender spoutOutput;   // per-zone Spout sender
    std::string spoutStreamName;    // custom Spout sender name
#endif

    bool init();
    void resize(int w, int h);
};

// "One look everywhere" helpers — shared by the layer zone menus (LayerPanel
// card menu, the viewport thumbnail rail) and the /easel/layer/allzones OSC
// handler, so one layer can be pushed to the whole house in a single gesture.
//
// showLayerInAllZones: the layer joins every zone's composite. Zones already
// showing all layers include it implicitly; zones with an explicit set gain it.
inline void showLayerInAllZones(std::vector<std::unique_ptr<OutputZone>>& zones,
                                uint32_t layerId) {
    for (auto& z : zones) {
        if (!z) continue;
        if (!z->showAllLayers) z->visibleLayerIds.insert(layerId);
    }
}

// soloLayerAcrossZones: every zone renders ONLY this layer — all outputs in
// the house show the same content.
inline void soloLayerAcrossZones(std::vector<std::unique_ptr<OutputZone>>& zones,
                                 uint32_t layerId) {
    for (auto& z : zones) {
        if (!z) continue;
        z->showAllLayers = false;
        z->visibleLayerIds.clear();
        z->visibleLayerIds.insert(layerId);
    }
}
