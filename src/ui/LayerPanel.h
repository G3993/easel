#pragma once
#include "app/OutputZone.h"
#include "compositing/LayerStack.h"
#include <string>
#include <vector>
#include <memory>

class FluidSource;
class FluidSource3D;

class LayerPanel {
public:
    void render(LayerStack& stack, int& selectedLayer,
                std::vector<std::unique_ptr<OutputZone>>* zones = nullptr,
                int activeZone = 0);

    // Signals: checked by Application after render
    bool wantsAddImage = false;
    bool wantsAddVideo = false;
    bool wantsAddShader = false;

    // Post-create binder for the Fluid image-inject quick-add buttons.
    // When set alongside wantsAddImage/Video/Shader, Application binds the
    // newly-created layer's id to this Fluid's image source (and clears).
    // Lets the Fluid PropertyPanel offer inline "+ Add Image / Video /
    // Shader" actions that create AND auto-bind in one click.
    FluidSource* postCreateBindFluidImage = nullptr;
    // Same mechanism for the 3D fluid's image-source quick-add buttons.
    FluidSource3D* postCreateBindFluid3DImage = nullptr;

    // Layer IDs removed during the last render() call — Application consumes these
    // to notify the Timeline (so orphaned tracks can be cleaned up).
    std::vector<uint32_t> removedLayerIds;

private:
    // Manual drag state
    bool m_dragActive = false;
    int m_dragIndex = -1;         // Stack index being dragged
    int m_insertIndex = -1;       // Where to insert on drop (display-space)
    float m_dragStartY = 0;       // Mouse Y when drag began
    float m_dragOffsetY = 0;      // Offset from row top to mouse

    // Inline rename
    bool m_renaming = false;
    int m_renameIndex = -1;
    char m_renameBuf[256] = {};
    bool m_renameJustStarted = false; // auto-focus the input on first frame

    // Inline opacity drag — drag the "100%" readout left/right to scrub the
    // layer's opacity. Active while the mouse is down inside the readout's
    // hit rect.
    int   m_opacityDragIdx   = -1;
    float m_opacityDragStart = 1.0f;
    float m_opacityDragAnchorX = 0.0f;
};
