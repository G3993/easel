#pragma once
#include "compositing/CompositeEngine.h"
#include "warp/CornerPinWarp.h"
#include "warp/MeshWarp.h"
#include "warp/ObjMeshWarp.h"
#include "render/Framebuffer.h"
#include "ui/ViewportPanel.h" // for WarpMode enum
#ifdef HAS_NDI
#include "app/NDIOutput.h"
#endif
#include <string>
#include <set>
#include <cstdint>

enum class OutputDest { None, Fullscreen, NDI };

struct OutputZone {
    std::string name = "Main";
    int width = 1920;
    int height = 1080;
    int compPreset = 0; // resolution preset index

    CompositeEngine compositor;
    CornerPinWarp cornerPin;
    MeshWarp meshWarp;
    ObjMeshWarp objMeshWarp;
    Framebuffer warpFBO;
    ViewportPanel::WarpMode warpMode = ViewportPanel::WarpMode::CornerPin;

    // Layer visibility
    bool showAllLayers = true;
    std::set<uint32_t> visibleLayerIds;

    // Output routing
    OutputDest outputDest = OutputDest::None;
    int outputMonitor = -1;         // for Fullscreen: which monitor index
    std::string ndiStreamName;      // for NDI: stream name (defaults to zone name)

#ifdef HAS_NDI
    NDIOutput ndiOutput;            // per-zone NDI sender
#endif

    bool init();
    void resize(int w, int h);
};
