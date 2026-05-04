#pragma once

#include <glad/glad.h>
#include <string>
#include <unordered_map>
#include <vector>

// Live-preview grid of GL transition shaders.
//
// Each tile is a small offscreen FBO that runs the actual gl-transitions
// shader against two source textures (the user's current "from" and "to"
// content). Progress loops 0→1 over kLoopSeconds with a hold at each end so
// the eye can register which transition is which. Only kTilesPerFrame tiles
// are rendered per frame, round-robin, so a 33-shader catalog stays cheap.
//
// Usage:
//   inside an ImGui popup / child window:
//     std::string picked = m_transitionCatalog.drawAndPick(
//                              fromTex, toTex, srcW, srcH, currentName);
//     if (!picked.empty()) {
//         /* user clicked a tile — apply the pick */
//     }
class TransitionCatalog {
public:
    TransitionCatalog() = default;
    ~TransitionCatalog();
    TransitionCatalog(const TransitionCatalog&) = delete;
    TransitionCatalog& operator=(const TransitionCatalog&) = delete;

    // Draws the grid inside the current ImGui window/popup. Pass the user's
    // current adjacent clip textures as fromTex/toTex; the tiles render the
    // transition shader between them so previews look like the real seam.
    // Returns the picked transition name (clicked this frame), or "" if none.
    std::string drawAndPick(GLuint fromTex, GLuint toTex,
                            int sourceW, int sourceH,
                            const std::string& currentlyPicked);

    // Free GL resources. Safe to call repeatedly.
    void release();

private:
    struct Tile { GLuint fbo = 0; GLuint tex = 0; int w = 0; int h = 0; };
    Tile& ensureTile(const std::string& name, int w, int h);
    void  renderTile(Tile& tile, const std::string& name,
                     GLuint fromTex, GLuint toTex,
                     int sourceW, int sourceH, float progress);

    std::unordered_map<std::string, Tile> m_tiles;
    std::vector<std::string> m_visibleOrder; // names currently in viewport, refreshed per frame
    int  m_renderCursor = 0;                 // round-robin head into m_visibleOrder
    char m_filter[64]   = "";
    double m_t0 = -1.0;                      // wallclock anchor for progress loop
};
