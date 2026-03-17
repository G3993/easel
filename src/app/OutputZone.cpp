#include "app/OutputZone.h"

bool OutputZone::init() {
    if (!compositor.init(width, height)) return false;
    if (!cornerPin.init()) return false;
    if (!meshWarp.init(5, 5)) return false;
    if (!objMeshWarp.init()) return false;
    bool needsDepth = (warpMode == ViewportPanel::WarpMode::ObjMesh);
    if (!warpFBO.create(width, height, needsDepth)) return false;
    return true;
}

void OutputZone::resize(int w, int h) {
    if (w == width && h == height) return;
    width = w;
    height = h;
    compositor.resize(w, h);
    warpFBO.resize(w, h);
}
