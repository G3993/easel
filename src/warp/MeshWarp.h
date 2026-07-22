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

    // Sample the warped surface at continuous lattice coords
    // (gx in [0, cols-1], gy in [0, rows-1]) — bicubic Catmull-Rom through
    // the control points, falling back to linear across "corner" points.
    // Used by render()'s tessellation AND the projector-output overlay so
    // the drawn edges follow the exact same curve as the warped surface.
    glm::vec2 samplePoint(float gx, float gy) const;

    // Tessellation substeps per control cell for the smooth Catmull-Rom warp
    // (higher = smoother diagonals between control points). 48 → a 4×4 grid
    // renders as 144×144 smoothly-curved quads, so steep diagonals stay
    // clean instead of faceting at cell boundaries.
    int m_renderSubsteps = 48;

private:
    int m_cols = 4, m_rows = 4;
    std::vector<glm::vec2> m_controlPoints;
    std::vector<uint8_t>   m_corner;   // parallel to m_controlPoints; 1 = straight
    Mesh m_mesh;
    ShaderProgram m_shader;

    // Tessellation cache: the inputs that produced the GPU mesh currently in
    // m_mesh. render() re-tessellates (and re-uploads) only when these change
    // — it used to rebuild the whole fine mesh and destroy/recreate the
    // VAO/VBO/EBO every frame it rendered. Control points are compared by
    // value because points() hands out a mutable reference (drag UI edits
    // the vector directly).
    std::vector<glm::vec2> m_meshedPoints;
    std::vector<uint8_t>   m_meshedCorner;
    int m_meshedCols = 0, m_meshedRows = 0, m_meshedSubsteps = 0;

    void rebuildMesh();
    glm::vec2 defaultPoint(int col, int row) const;
};
