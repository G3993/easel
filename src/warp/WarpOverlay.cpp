#include "warp/WarpOverlay.h"
#include "warp/MeshWarp.h"
#include <cmath>

// Overlay palette — saturated fills over black underlays so the gizmos
// read on both the white and black regions of the calibration patterns.
static const glm::vec4 kUnderlay   (0.0f,  0.0f,  0.0f,  0.70f);
static const glm::vec4 kEdge       (0.24f, 0.85f, 1.0f,  0.95f);   // cyan
static const glm::vec4 kPointFill  (1.0f,  0.80f, 0.24f, 1.0f);    // gold
static const glm::vec4 kPointRing  (1.0f,  1.0f,  1.0f,  0.95f);
static const glm::vec4 kActiveFill (1.0f,  1.0f,  1.0f,  1.0f);    // white

bool WarpOverlay::init() {
    // mask.vert takes positions in UV (0-1) space and converts to clip;
    // mask.frag is a flat uColor fill — exactly what a gizmo overlay needs.
    if (!m_shader.loadFromFiles("shaders/mask.vert", "shaders/mask.frag")) {
        return false;
    }
    glGenVertexArrays(1, &m_vao);
    glGenBuffers(1, &m_vbo);
    glBindVertexArray(m_vao);
    glBindBuffer(GL_ARRAY_BUFFER, m_vbo);
    glEnableVertexAttribArray(0);
    glVertexAttribPointer(0, 2, GL_FLOAT, GL_FALSE, sizeof(glm::vec2), (void*)0);
    glBindVertexArray(0);
    return true;
}

void WarpOverlay::addLinePx(glm::vec2 aPx, glm::vec2 bPx, float widthPx) {
    glm::vec2 d = bPx - aPx;
    float len = std::sqrt(d.x * d.x + d.y * d.y);
    if (len < 1e-4f) return;
    glm::vec2 n = glm::vec2(-d.y, d.x) * (widthPx * 0.5f / len);
    // Convert the four quad corners px → UV (mask.vert expects 0-1)
    glm::vec2 q[4] = { (aPx + n) / m_outPx, (bPx + n) / m_outPx,
                       (bPx - n) / m_outPx, (aPx - n) / m_outPx };
    m_tris.push_back(q[0]); m_tris.push_back(q[1]); m_tris.push_back(q[2]);
    m_tris.push_back(q[0]); m_tris.push_back(q[2]); m_tris.push_back(q[3]);
}

void WarpOverlay::addPolylinePx(const std::vector<glm::vec2>& ptsPx, float widthPx) {
    for (size_t i = 0; i + 1 < ptsPx.size(); i++) {
        addLinePx(ptsPx[i], ptsPx[i + 1], widthPx);
    }
}

void WarpOverlay::addCirclePx(glm::vec2 cPx, float radiusPx, int segments) {
    glm::vec2 cUV = cPx / m_outPx;
    float prevA = 0.0f;
    for (int s = 1; s <= segments; s++) {
        float a = (float)s / segments * 6.2831853f;
        glm::vec2 p0 = (cPx + radiusPx * glm::vec2(std::cos(prevA), std::sin(prevA))) / m_outPx;
        glm::vec2 p1 = (cPx + radiusPx * glm::vec2(std::cos(a),     std::sin(a)))     / m_outPx;
        m_tris.push_back(cUV); m_tris.push_back(p0); m_tris.push_back(p1);
        prevA = a;
    }
}

void WarpOverlay::addRectPx(glm::vec2 cPx, float halfPx) {
    glm::vec2 q[4] = { (cPx + glm::vec2(-halfPx, -halfPx)) / m_outPx,
                       (cPx + glm::vec2( halfPx, -halfPx)) / m_outPx,
                       (cPx + glm::vec2( halfPx,  halfPx)) / m_outPx,
                       (cPx + glm::vec2(-halfPx,  halfPx)) / m_outPx };
    m_tris.push_back(q[0]); m_tris.push_back(q[1]); m_tris.push_back(q[2]);
    m_tris.push_back(q[0]); m_tris.push_back(q[2]); m_tris.push_back(q[3]);
}

void WarpOverlay::flush(const glm::vec4& color) {
    if (m_tris.empty() || m_shader.id() == 0) { m_tris.clear(); return; }
    m_shader.use();
    m_shader.setVec4("uColor", color);
    glBindVertexArray(m_vao);
    glBindBuffer(GL_ARRAY_BUFFER, m_vbo);
    size_t bytes = m_tris.size() * sizeof(glm::vec2);
    if (m_tris.size() > m_vboCapacity) {
        glBufferData(GL_ARRAY_BUFFER, bytes, m_tris.data(), GL_DYNAMIC_DRAW);
        m_vboCapacity = m_tris.size();
    } else {
        glBufferSubData(GL_ARRAY_BUFFER, 0, bytes, m_tris.data());
    }
    glDrawArrays(GL_TRIANGLES, 0, (GLsizei)m_tris.size());
    glBindVertexArray(0);
    m_tris.clear();
}

void WarpOverlay::drawCornerPin(const std::array<glm::vec2, 4>& corners,
                                int activeIndex, int outW, int outH) {
    if (m_shader.id() == 0) return;
    m_outPx = glm::vec2((float)outW, (float)outH);
    std::array<glm::vec2, 4> px;
    for (int i = 0; i < 4; i++) px[i] = ndcToPx(corners[i]);

    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

    // Edges: dark underlay, then bright line on top.
    for (int i = 0; i < 4; i++) addLinePx(px[i], px[(i + 1) % 4], 9.0f);
    flush(kUnderlay);
    for (int i = 0; i < 4; i++) addLinePx(px[i], px[(i + 1) % 4], 3.5f);
    flush(kEdge);

    // Corner handles: black halo → white ring → fill (white + bigger while
    // the point is being dragged so it stays visible under your cursor).
    for (int i = 0; i < 4; i++) {
        bool active = (i == activeIndex);
        float r = active ? 15.0f : 11.0f;
        addCirclePx(px[i], r + 4.0f);
    }
    flush(kUnderlay);
    for (int i = 0; i < 4; i++) {
        bool active = (i == activeIndex);
        addCirclePx(px[i], (active ? 15.0f : 11.0f));
    }
    flush(kPointRing);
    for (int i = 0; i < 4; i++) {
        if (i == activeIndex) continue;
        addCirclePx(px[i], 8.0f);
    }
    flush(kPointFill);
    if (activeIndex >= 0 && activeIndex < 4) {
        addCirclePx(px[activeIndex], 11.5f);
        flush(kActiveFill);
    }

    glDisable(GL_BLEND);
}

void WarpOverlay::drawMeshWarp(const MeshWarp& mesh, int activeIndex,
                               int outW, int outH) {
    if (m_shader.id() == 0) return;
    m_outPx = glm::vec2((float)outW, (float)outH);
    const int cols = mesh.cols(), rows = mesh.rows();
    const auto& points = mesh.points();

    // Trace the lattice edges along the SAME spline the warp renders with,
    // so the drawn lines hug the curved surface instead of cutting chords
    // between control points.
    const int kSamplesPerCell = 16;
    std::vector<std::vector<glm::vec2>> lines;
    lines.reserve(rows + cols);
    for (int r = 0; r < rows; r++) {
        std::vector<glm::vec2> line;
        line.reserve((cols - 1) * kSamplesPerCell + 1);
        for (int i = 0; i <= (cols - 1) * kSamplesPerCell; i++) {
            float gx = (float)i / kSamplesPerCell;
            line.push_back(ndcToPx(mesh.samplePoint(gx, (float)r)));
        }
        lines.push_back(std::move(line));
    }
    for (int c = 0; c < cols; c++) {
        std::vector<glm::vec2> line;
        line.reserve((rows - 1) * kSamplesPerCell + 1);
        for (int j = 0; j <= (rows - 1) * kSamplesPerCell; j++) {
            float gy = (float)j / kSamplesPerCell;
            line.push_back(ndcToPx(mesh.samplePoint((float)c, gy)));
        }
        lines.push_back(std::move(line));
    }

    glEnable(GL_BLEND);
    glBlendFunc(GL_SRC_ALPHA, GL_ONE_MINUS_SRC_ALPHA);

    for (const auto& l : lines) addPolylinePx(l, 7.0f);
    flush(kUnderlay);
    for (const auto& l : lines) addPolylinePx(l, 2.5f);
    flush(kEdge);

    // Control points: squares for straight/corner points, circles for
    // smooth ones — same vocabulary as the editor canvas.
    for (int i = 0; i < (int)points.size(); i++) {
        bool active = (i == activeIndex);
        glm::vec2 p = ndcToPx(points[i]);
        float r = active ? 13.0f : 9.0f;
        if (mesh.isCorner(i)) addRectPx(p, r + 3.0f);
        else                  addCirclePx(p, r + 3.0f);
    }
    flush(kUnderlay);
    for (int i = 0; i < (int)points.size(); i++) {
        bool active = (i == activeIndex);
        glm::vec2 p = ndcToPx(points[i]);
        float r = active ? 13.0f : 9.0f;
        if (mesh.isCorner(i)) addRectPx(p, r);
        else                  addCirclePx(p, r);
    }
    flush(kPointRing);
    for (int i = 0; i < (int)points.size(); i++) {
        if (i == activeIndex) continue;
        glm::vec2 p = ndcToPx(points[i]);
        if (mesh.isCorner(i)) addRectPx(p, 6.5f);
        else                  addCirclePx(p, 6.5f);
    }
    flush(kPointFill);
    if (activeIndex >= 0 && activeIndex < (int)points.size()) {
        glm::vec2 p = ndcToPx(points[activeIndex]);
        if (mesh.isCorner(activeIndex)) addRectPx(p, 10.0f);
        else                            addCirclePx(p, 10.0f);
        flush(kActiveFill);
    }

    glDisable(GL_BLEND);
}
