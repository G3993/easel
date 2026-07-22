#include "warp/MeshWarp.h"
#include <imgui.h>
#include <vector>
#include <algorithm>
#include <cmath>

// Uniform Catmull-Rom — interpolates through p1,p2 with smooth tangents from
// p0,p3. Used to tessellate the warp lattice into a smooth surface.
static glm::vec2 catmull(const glm::vec2& p0, const glm::vec2& p1,
                         const glm::vec2& p2, const glm::vec2& p3, float t) {
    float t2 = t * t, t3 = t2 * t;
    return 0.5f * ((2.0f * p1)
                 + (-p0 + p2) * t
                 + (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2
                 + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t3);
}

bool MeshWarp::init(int cols, int rows) {
    if (!m_shader.loadFromFiles("shaders/meshwarp.vert", "shaders/passthrough.frag")) {
        return false;
    }
    setGridSize(cols, rows);
    return true;
}

glm::vec2 MeshWarp::defaultPoint(int col, int row) const {
    float u = (float)col / (m_cols - 1);
    float v = (float)row / (m_rows - 1);
    return glm::vec2(u * 2.0f - 1.0f, v * 2.0f - 1.0f);
}

void MeshWarp::setGridSize(int cols, int rows) {
    m_cols = std::max(2, cols);
    m_rows = std::max(2, rows);
    resetGrid();
}

void MeshWarp::resetGrid() {
    m_controlPoints.resize(m_cols * m_rows);
    m_corner.assign(m_cols * m_rows, 0);   // all points smooth by default
    for (int r = 0; r < m_rows; r++) {
        for (int c = 0; c < m_cols; c++) {
            m_controlPoints[r * m_cols + c] = defaultPoint(c, r);
        }
    }
    rebuildMesh();
}

glm::vec2& MeshWarp::controlPoint(int col, int row) {
    return m_controlPoints[row * m_cols + col];
}

const glm::vec2& MeshWarp::controlPoint(int col, int row) const {
    return m_controlPoints[row * m_cols + col];
}

void MeshWarp::rebuildMesh() {
    // The GPU mesh is owned by render()'s tessellation cache; grid mutations
    // just invalidate it so the next render re-tessellates and re-uploads.
    m_meshedPoints.clear();
    m_meshedCols = m_meshedRows = m_meshedSubsteps = 0;
}

glm::vec2 MeshWarp::samplePoint(float gx, float gy) const {
    auto cp = [&](int c, int r) -> glm::vec2 {
        c = std::min(std::max(c, 0), m_cols - 1);
        r = std::min(std::max(r, 0), m_rows - 1);
        return m_controlPoints[r * m_cols + c];
    };
    auto cornerAt = [&](int c, int r) -> bool {
        c = std::min(std::max(c, 0), m_cols - 1);
        r = std::min(std::max(r, 0), m_rows - 1);
        int idx = r * m_cols + c;
        return idx < (int)m_corner.size() && m_corner[idx] != 0;
    };
    // Bicubic Catmull-Rom sample at continuous lattice coords
    // (gx in [0,cols-1], gy in [0,rows-1]). A segment whose either
    // endpoint is flagged "corner" falls back to straight (linear)
    // interpolation, so hard edges stay straight while the rest of the
    // lattice curves smoothly.
    int ix = (int)std::floor(gx); float fx = gx - (float)ix;
    int iy = (int)std::floor(gy); float fy = gy - (float)iy;
    glm::vec2 row[4];
    for (int k = 0; k < 4; k++) {
        int rr = iy - 1 + k;
        row[k] = (cornerAt(ix, rr) || cornerAt(ix + 1, rr))
                   ? cp(ix, rr) * (1.0f - fx) + cp(ix + 1, rr) * fx   // straight
                   : catmull(cp(ix - 1, rr), cp(ix, rr),
                             cp(ix + 1, rr), cp(ix + 2, rr), fx);
    }
    int cc = (fx < 0.5f) ? ix : ix + 1;   // nearest column for the vertical decision
    return (cornerAt(cc, iy) || cornerAt(cc, iy + 1))
             ? row[1] * (1.0f - fy) + row[2] * fy                     // straight
             : catmull(row[0], row[1], row[2], row[3], fy);
}

void MeshWarp::render(GLuint sourceTexture) {
    // Smoothly tessellate the control lattice with Catmull-Rom interpolation:
    // each control cell is split into S×S sub-quads whose positions follow a
    // smooth spline through the control points, so the warped surface curves
    // instead of faceting linearly at each cell boundary — straight and
    // diagonal lines stay smooth, not jagged. A regular (unmoved) grid reduces
    // exactly to the identity mapping, so flat output is unchanged.
    const int S = std::max(1, m_renderSubsteps);

    // Re-tessellate only when the lattice actually changed (drag, grid
    // add/remove, substep change). The common steady-state frame skips
    // straight to the draw — no CPU spline work, no buffer re-upload.
    const bool dirty = m_meshedCols != m_cols || m_meshedRows != m_rows ||
                       m_meshedSubsteps != S || m_meshedPoints != m_controlPoints ||
                       m_meshedCorner != m_corner;
    if (dirty) {
        const int FX = (m_cols - 1) * S;   // fine quads across / down
        const int FY = (m_rows - 1) * S;
        const int stride = FX + 1;
        std::vector<Vertex> verts;
        verts.reserve(stride * (FY + 1));
        for (int j = 0; j <= FY; j++) {
            float gy = (float)j / (float)S;
            float v  = gy / (float)(m_rows - 1);
            for (int i = 0; i <= FX; i++) {
                float gx = (float)i / (float)S;
                float u  = gx / (float)(m_cols - 1);
                glm::vec2 p = samplePoint(gx, gy);
                verts.push_back({p.x, p.y, u, v});
            }
        }

        // Same topology as the current GPU mesh (only positions moved, the
        // usual case while dragging a handle) → glBufferSubData in place.
        // Topology changed (grid size / substeps) → full re-upload.
        if (m_meshedCols == m_cols && m_meshedRows == m_rows &&
            m_meshedSubsteps == S && m_mesh.indexCount() == FX * FY * 6) {
            m_mesh.updateVertices(verts);
        } else {
            std::vector<unsigned int> indices;
            indices.reserve(FX * FY * 6);
            for (int j = 0; j < FY; j++) {
                for (int i = 0; i < FX; i++) {
                    unsigned int tl = j * stride + i, tr = tl + 1;
                    unsigned int bl = (j + 1) * stride + i, br = bl + 1;
                    indices.push_back(tl); indices.push_back(tr); indices.push_back(br);
                    indices.push_back(tl); indices.push_back(br); indices.push_back(bl);
                }
            }
            m_mesh.upload(verts, indices);
        }
        m_meshedPoints   = m_controlPoints;
        m_meshedCorner   = m_corner;
        m_meshedCols     = m_cols;
        m_meshedRows     = m_rows;
        m_meshedSubsteps = S;
    }

    m_shader.use();
    m_shader.setInt("uTexture", 0);
    m_shader.setFloat("uOpacity", 1.0f);
    m_shader.setBool("uHasMask", false);
    m_shader.setFloat("uFeather", 0.0f);
    m_shader.setFloat("uTileX", 1.0f);
    m_shader.setFloat("uTileY", 1.0f);
    m_shader.setInt("uMosaicMode", 0);
    m_shader.setFloat("uMosaicTransition", 1.0f);
    m_shader.setVec4("uCrop", glm::vec4(0.0f));

    glActiveTexture(GL_TEXTURE0);
    glBindTexture(GL_TEXTURE_2D, sourceTexture);

    m_mesh.draw();
}

int MeshWarp::hitTest(glm::vec2 pos, float radius) const {
    for (int i = 0; i < (int)m_controlPoints.size(); i++) {
        float dx = pos.x - m_controlPoints[i].x;
        float dy = pos.y - m_controlPoints[i].y;
        if (dx * dx + dy * dy < radius * radius) {
            return i;
        }
    }
    return -1;
}

void MeshWarp::drawHandles(const glm::vec2& viewportSize) const {
    ImDrawList* draw = ImGui::GetWindowDrawList();
    ImVec2 winPos = ImGui::GetWindowPos();
    ImVec2 contentMin = ImGui::GetWindowContentRegionMin();
    ImVec2 contentMax = ImGui::GetWindowContentRegionMax();
    float contentW = contentMax.x - contentMin.x;
    float contentH = contentMax.y - contentMin.y;

    auto ndcToScreen = [&](glm::vec2 ndc) -> ImVec2 {
        float sx = winPos.x + contentMin.x + (ndc.x * 0.5f + 0.5f) * contentW;
        float sy = winPos.y + contentMin.y + (1.0f - (ndc.y * 0.5f + 0.5f)) * contentH;
        return ImVec2(sx, sy);
    };

    // Draw grid lines
    for (int r = 0; r < m_rows; r++) {
        for (int c = 0; c < m_cols - 1; c++) {
            ImVec2 a = ndcToScreen(m_controlPoints[r * m_cols + c]);
            ImVec2 b = ndcToScreen(m_controlPoints[r * m_cols + c + 1]);
            draw->AddLine(a, b, IM_COL32(100, 200, 255, 150), 1.0f);
        }
    }
    for (int c = 0; c < m_cols; c++) {
        for (int r = 0; r < m_rows - 1; r++) {
            ImVec2 a = ndcToScreen(m_controlPoints[r * m_cols + c]);
            ImVec2 b = ndcToScreen(m_controlPoints[(r + 1) * m_cols + c]);
            draw->AddLine(a, b, IM_COL32(100, 200, 255, 150), 1.0f);
        }
    }

    // Draw control points — squares for straight/corner points, circles for
    // smooth (curve) points, so every point's type is visible at a glance.
    for (int i = 0; i < (int)m_controlPoints.size(); i++) {
        ImVec2 p = ndcToScreen(m_controlPoints[i]);
        if (isCorner(i)) {
            draw->AddRectFilled(ImVec2(p.x - 5, p.y - 5), ImVec2(p.x + 5, p.y + 5),
                                IM_COL32(255, 210, 80, 255), 1.0f);
            draw->AddRect(ImVec2(p.x - 5, p.y - 5), ImVec2(p.x + 5, p.y + 5),
                          IM_COL32(255, 255, 255, 220), 1.0f, 0, 1.5f);
        } else {
            draw->AddCircleFilled(p, 5.0f, IM_COL32(100, 200, 255, 255));
            draw->AddCircle(p, 5.0f, IM_COL32(255, 255, 255, 200), 0, 1.5f);
        }
    }
}

void MeshWarp::addColumn() {
    int newCols = m_cols + 1;
    std::vector<glm::vec2> newPoints(newCols * m_rows);
    std::vector<uint8_t>   newCorner(newCols * m_rows, 0);

    for (int r = 0; r < m_rows; r++) {
        for (int c = 0; c < m_cols; c++) {
            newPoints[r * newCols + c] = m_controlPoints[r * m_cols + c];
            newCorner[r * newCols + c] = m_corner[r * m_cols + c];
        }
        // New column: interpolate between last column and edge
        newPoints[r * newCols + m_cols] = defaultPoint(m_cols, r);
    }

    m_cols = newCols;
    m_controlPoints = newPoints;
    m_corner = newCorner;
    rebuildMesh();
}

void MeshWarp::removeColumn() {
    if (m_cols <= 2) return;
    int newCols = m_cols - 1;
    std::vector<glm::vec2> newPoints(newCols * m_rows);
    std::vector<uint8_t>   newCorner(newCols * m_rows, 0);

    for (int r = 0; r < m_rows; r++) {
        for (int c = 0; c < newCols; c++) {
            newPoints[r * newCols + c] = m_controlPoints[r * m_cols + c];
            newCorner[r * newCols + c] = m_corner[r * m_cols + c];
        }
    }

    m_cols = newCols;
    m_controlPoints = newPoints;
    m_corner = newCorner;
    rebuildMesh();
}

void MeshWarp::addRow() {
    int newRows = m_rows + 1;
    std::vector<glm::vec2> newPoints(m_cols * newRows);
    std::vector<uint8_t>   newCorner(m_cols * newRows, 0);

    for (int r = 0; r < m_rows; r++) {
        for (int c = 0; c < m_cols; c++) {
            newPoints[r * m_cols + c] = m_controlPoints[r * m_cols + c];
            newCorner[r * m_cols + c] = m_corner[r * m_cols + c];
        }
    }
    for (int c = 0; c < m_cols; c++) {
        newPoints[m_rows * m_cols + c] = defaultPoint(c, m_rows);
    }

    m_rows = newRows;
    m_controlPoints = newPoints;
    m_corner = newCorner;
    rebuildMesh();
}

void MeshWarp::removeRow() {
    if (m_rows <= 2) return;
    int newRows = m_rows - 1;
    std::vector<glm::vec2> newPoints(m_cols * newRows);
    std::vector<uint8_t>   newCorner(m_cols * newRows, 0);

    for (int r = 0; r < newRows; r++) {
        for (int c = 0; c < m_cols; c++) {
            newPoints[r * m_cols + c] = m_controlPoints[r * m_cols + c];
            newCorner[r * m_cols + c] = m_corner[r * m_cols + c];
        }
    }

    m_rows = newRows;
    m_controlPoints = newPoints;
    m_corner = newCorner;
    rebuildMesh();
}
