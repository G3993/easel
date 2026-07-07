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
