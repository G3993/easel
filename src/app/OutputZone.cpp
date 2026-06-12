#include "app/OutputZone.h"

// Phase Q v2: 2× supersample. The composite chain renders at twice the
// canvas dimensions; the warp pass samples that 16F texture with linear
// filtering and writes it to the 1× warpFBO — implicit downsample, ~4×
// effective MSAA on every shader without changing a line of shader source.
// Cost: 4× fragment work in the composite chain. M-series GPUs eat this.
static constexpr int kSupersample = 2;

bool OutputZone::init() {
    // GL allocation is deferred to ensureGpu(): a zone that is never
    // composited (unrouted, never shown in a preview) must not hold a
    // supersampled composite chain in VRAM.
    return true;
}

bool OutputZone::ensureGpu() {
    if (gpuReady) return true;
    if (!compositor.init(width * kSupersample, height * kSupersample)) return false;
    // warpFBO is the post-composite, pre-readback target at LOGICAL size —
    // the implicit downsample happens when warp samples the supersampled
    // composite output and writes here at 1×. 16F so the chain keeps headroom.
    if (!warpFBO.createHalfFloat(width, height)) return false;
    // readbackFBO is only needed by NDI/RTMP/recorder readback and is
    // created lazily by renderReadbackFBO().
    gpuReady = true;
    return true;
}

void OutputZone::resize(int w, int h) {
    if (w == width && h == height) return;
    width = w;
    height = h;
    if (!gpuReady) return;
    compositor.resize(w * kSupersample, h * kSupersample);
    warpFBO.resize(w, h);
    if (readbackFBO.fboId()) readbackFBO.resize(w, h);
    // Scratch FBOs recreate themselves on next use via their size checks.
}

void OutputZone::releaseIdleScratch(uint64_t frame) {
    // ~10s at 60fps. Scratch sets for features that turned off (bloom,
    // edge blend, masks) or zones that stopped compositing are returned to
    // the driver instead of staying resident until zone deletion.
    constexpr uint64_t kIdleFrames = 600;
    auto idle = [&](int g) {
        return scratchUsedFrame[g] != 0 && frame - scratchUsedFrame[g] > kIdleFrames;
    };
    if (idle(0)) {
        warpSSFBO.destroy();
        scratchUsedFrame[0] = 0;
    }
    if (idle(1)) {
        bloomBrightFBO.destroy();
        bloomPingFBO[0].destroy();
        bloomPingFBO[1].destroy();
        bloomCompositeFBO.destroy();
        scratchUsedFrame[1] = 0;
    }
    if (idle(2)) {
        postFBO.destroy();
        scratchUsedFrame[2] = 0;
    }
    if (idle(3)) {
        maskUnionFBO.destroy();
        scratchUsedFrame[3] = 0;
    }
}
