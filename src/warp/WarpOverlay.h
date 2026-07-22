#pragma once
#include <glad/glad.h>
#include "render/ShaderProgram.h"
#include <glm/glm.hpp>
#include <array>
#include <vector>

class MeshWarp;

// Draws the warp control points + edges directly into the zone's warp
// output — the texture the projector presents — while the MAPPING
// workspace is active. You align a projection standing at the wall, so
// the handles have to be visible ON the surface, not just on the editor
// canvas. Corner-pin edges are straight; mesh-warp edges are sampled
// along the same Catmull-Rom spline the warp itself renders with, so the
// drawn lattice sits exactly on the warped image.
class WarpOverlay {
public:
    bool init();

    // outW/outH: zone output resolution in pixels (used to keep line
    // widths / point radii constant in projector pixels regardless of
    // aspect or the supersample factor of the bound FBO).
    // activeIndex: control point currently being dragged (-1 = none);
    // it draws larger and white so the point you're moving is unmissable.
    void drawCornerPin(const std::array<glm::vec2, 4>& corners,
                       int activeIndex, int outW, int outH);
    void drawMeshWarp(const MeshWarp& mesh, int activeIndex,
                      int outW, int outH);

private:
    // Geometry accumulates in output pixel space, converts to NDC on push.
    void addLinePx(glm::vec2 aPx, glm::vec2 bPx, float widthPx);
    void addPolylinePx(const std::vector<glm::vec2>& ptsPx, float widthPx);
    void addCirclePx(glm::vec2 cPx, float radiusPx, int segments = 28);
    void addRectPx(glm::vec2 cPx, float halfPx);
    void flush(const glm::vec4& color);

    glm::vec2 ndcToPx(glm::vec2 ndc) const {
        return (ndc * 0.5f + 0.5f) * m_outPx;
    }

    glm::vec2 m_outPx {1.0f, 1.0f};
    std::vector<glm::vec2> m_tris;   // triangle soup, NDC
    ShaderProgram m_shader;
    GLuint m_vao = 0, m_vbo = 0;
    size_t m_vboCapacity = 0;
};
