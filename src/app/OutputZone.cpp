#include "app/OutputZone.h"

// Phase Q v2: 2× supersample. The composite chain renders at twice the
// canvas dimensions; the warp pass samples that 16F texture with linear
// filtering and writes it to the 1× warpFBO — implicit downsample, ~4×
// effective MSAA on every shader without changing a line of shader source.
// Cost: 4× fragment work in the composite chain. M-series GPUs eat this.
static constexpr int kSupersample = 2;

bool OutputZone::init() {
    if (!compositor.init(width * kSupersample, height * kSupersample)) return false;
    // warpFBO is the post-composite, pre-readback target at LOGICAL size —
    // the implicit downsample happens when warp samples the supersampled
    // composite output and writes here at 1×. 16F so the chain keeps headroom.
    if (!warpFBO.createHalfFloat(width, height)) return false;
    if (!readbackFBO.create(width, height, false)) return false;
    return true;
}

void OutputZone::resize(int w, int h) {
    if (w == width && h == height) return;
    width = w;
    height = h;
    compositor.resize(w * kSupersample, h * kSupersample);
    warpFBO.resize(w, h);
    readbackFBO.resize(w, h);
}
