#pragma once
#include "warp/CornerPinWarp.h"
#include "warp/MeshWarp.h"
#include "warp/ObjMeshWarp.h"
#include "compositing/MaskPath.h"
#include "render/Texture.h"
#include "ui/ViewportPanel.h" // for WarpMode enum
#include <string>
#include <memory>
#include <vector>

// A named mask within a mapping profile
struct MappingMask {
    std::string name = "Mask";
    MaskPath path;
    std::shared_ptr<Texture> texture; // rendered by MaskRenderer
};

struct MappingProfile {
    std::string name = "Default";
    ViewportPanel::WarpMode warpMode = ViewportPanel::WarpMode::CornerPin;

    CornerPinWarp cornerPin;
    MeshWarp meshWarp;
    ObjMeshWarp objMeshWarp;

    // Edge blending (pixels of overlap on each edge)
    float edgeBlendLeft = 0.0f;
    float edgeBlendRight = 0.0f;
    float edgeBlendTop = 0.0f;
    float edgeBlendBottom = 0.0f;
    float edgeBlendGamma = 2.2f;

    // Masks (applied to the zone output after compositing, before warp)
    std::vector<MappingMask> masks;
    int activeMaskIndex = -1; // which mask is being edited (-1 = none)

    bool init();
};
