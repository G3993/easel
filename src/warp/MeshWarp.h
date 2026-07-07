#pragma once
#include "render/Mesh.h"
#include "render/ShaderProgram.h"
#include <glm/glm.hpp>
#include <vector>

class MeshWarp {
public:
    bool init(int cols = 4, int rows = 4);

    void setGridSize(int cols, int rows);
    int cols() const { return m_cols; }
    int rows() const { return m_rows; }

    // Get/set control point positions (in NDC)
    glm::vec2& controlPoint(int col, int row);
    const glm::vec2& controlPoint(int col, int row) const;

    // Reset all control points to default grid positions
    void resetGrid();

    void render(GLuint sourceTexture);

    // Hit test: returns flat index or -1
    int hitTest(glm::vec2 pos, float radius = 0.04f) const;

    // Draw grid lines and control points
    void drawHandles(const glm::vec2& viewportSize) const;

    // Add/remove rows and columns
    void addColumn();
    void removeColumn();
    void addRow();
    void removeRow();

    std::vector<glm::vec2>& points() { return m_controlPoints; }
    const std::vector<glm::vec2>& points() const { return m_controlPoints; }

    // Per-control-point curve/straight flag. 0 = smooth (Catmull-Rom curve),
    // 1 = corner: every grid segment touching this point renders straight.
    // Lets you pin hard architectural edges while the rest of the mesh stays
    // smoothly curved. Right-click a handle in the viewport to toggle.
    std::vector<uint8_t>& corners() { return m_corner; }
    const std::vector<uint8_t>& corners() const { return m_corner; }
    bool isCorner(int i) const { return i >= 0 && i < (int)m_corner.size() && m_corner[i] != 0; }
    void toggleCorner(int i) { if (i >= 0 && i < (int)m_corner.size()) m_corner[i] = m_corner[i] ? 0 : 1; }
    void setAllCorners(bool on) { for (auto& c : m_corner) c = on ? 1 : 0; }

    // Tessellation substeps per control cell for the smooth Catmull-Rom warp
    // (higher = smoother diagonals between control points). 12 → a 4×4 grid
    // renders as 36×36 smoothly-curved quads.
    int m_renderSubsteps = 12;

private:
    int m_cols = 4, m_rows = 4;
    std::vector<glm::vec2> m_controlPoints;
    std::vector<uint8_t>   m_corner;   // parallel to m_controlPoints; 1 = straight
    Mesh m_mesh;
    ShaderProgram m_shader;

    void rebuildMesh();
    glm::vec2 defaultPoint(int col, int row) const;
};
