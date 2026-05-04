#include "stage/StageView.h"
#include <imgui.h>
#include "ui/ImGuizmo.h"
#include "ui/ParamRow.h"
#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/type_ptr.hpp>
#include <iostream>
#include <cmath>
#include <algorithm>
#include <functional>
#include <initializer_list>

#define TINYOBJLOADER_IMPLEMENTATION
// Already defined in ObjMeshWarp.cpp, so we use extern
#undef TINYOBJLOADER_IMPLEMENTATION

// --- VirtualProjector ---

glm::mat4 VirtualProjector::viewMatrix() const {
    return glm::lookAt(position, target, glm::vec3(0, 1, 0));
}

glm::mat4 VirtualProjector::projMatrix() const {
    return glm::perspective(glm::radians(fovDeg), aspectRatio, nearPlane, farPlane);
}

glm::mat4 VirtualProjector::vpMatrix() const {
    return projMatrix() * viewMatrix();
}

// --- StageView ---

bool StageView::init() {
    if (!m_shader.loadFromFiles("shaders/objmesh.vert", "shaders/objmesh.frag")) {
        std::cerr << "[Stage] Failed to load shader" << std::endl;
        return false;
    }
    m_quad.createQuad();
    m_fbo.create(m_fboWidth, m_fboHeight, true); // with depth

    // Friendly empty state — drop a default 1920x1080 display into the scene
    // so there's always something showing the test pattern. Users can delete
    // it, import a model over it, or just use it as their screen if the show
    // is that simple. Defaults in StageDisplay are already 1.92m x 1.08m and
    // zoneIndex = 0, which means it will pick up zone 0's live content.
    if (m_displays.empty()) {
        addDisplay("Default Screen", StageDisplay::Type::Monitor);
    }

    // Default camera: orbit slightly right + above the display so the user can
    // tell they're looking at a plane in 3D space (not a flat preview). A pure
    // head-on camera reads like a Canvas tab clone and hides the spatial
    // context Stage is meant to communicate.
    m_camera.azimuth   = 0.35f;
    m_camera.elevation = 0.22f;
    m_camera.distance  = 3.2f;
    m_camera.target    = {0.0f, 1.0f, 0.0f}; // display center (y=1 per StageDisplay default)
    return true;
}

bool StageView::loadModel(const std::string& path) {
    // Reuse ObjMeshWarp's loading logic
    ObjMeshWarp loader;
    if (!loader.loadModel(path)) return false;

    m_materials = std::move(loader.materials());
    m_modelPath = path;
    m_modelScale = loader.modelScale();
    m_bboxCenter = {0, 0, 0};

    // ObjMeshWarp normalises with `modelScale = 1 / maxAxisExtent`, so
    // `1/modelScale` is the model's largest world dimension after import.
    // Without a clamp this can go to hundreds of metres for tiny scales,
    // which sends the camera 750m+ away and makes F-to-frame useless.
    float worldExtent = (loader.modelScale() > 1e-4f) ? (1.0f / loader.modelScale()) : 3.0f;
    m_bboxExtent = std::max(0.5f, std::min(30.0f, worldExtent));

    // Reset camera to fit model
    m_camera.distance = m_bboxExtent * 2.5f;
    m_camera.target = m_bboxCenter;
    m_camera.azimuth = 0.4f;
    m_camera.elevation = 0.3f;

    std::cout << "[Stage] Loaded model: " << path << " (" << m_materials.size() << " materials)" << std::endl;
    return true;
}

bool StageView::hasModel() const {
    return !m_materials.empty();
}

int StageView::addProjector(const std::string& name) {
    VirtualProjector proj;
    proj.name = name;
    proj.position = {0, 2, 4};
    proj.target = {0, 0, 0};
    proj.zoneIndex = (int)m_projectors.size();
    m_projectors.push_back(proj);
    return (int)m_projectors.size() - 1;
}

void StageView::removeProjector(int index) {
    if (index >= 0 && index < (int)m_projectors.size()) {
        m_projectors.erase(m_projectors.begin() + index);
        // Keep selection pointing at the SAME projector across the erase.
        if (m_selectedProjector == index)       m_selectedProjector = -1;
        else if (m_selectedProjector > index)   m_selectedProjector--;
        if (m_selectedProjector >= (int)m_projectors.size())
            m_selectedProjector = -1;
        // Surfaces lose / shift their projector reference too.
        for (auto& s : m_surfaces) {
            if (s.projectorIndex == index)      s.projectorIndex = -1;
            else if (s.projectorIndex > index)  s.projectorIndex--;
        }
    }
}

int StageView::addSurface(const std::string& name, int materialIdx, int projIdx) {
    ProjectionSurface surf;
    surf.name = name;
    surf.materialIndex = materialIdx;
    surf.projectorIndex = projIdx;
    m_surfaces.push_back(surf);
    m_selectedSurface = (int)m_surfaces.size() - 1;
    return m_selectedSurface;
}

void StageView::removeSurface(int index) {
    if (index >= 0 && index < (int)m_surfaces.size()) {
        m_surfaces.erase(m_surfaces.begin() + index);
        if (m_selectedSurface == index)       m_selectedSurface = -1;
        else if (m_selectedSurface > index)   m_selectedSurface--;
        if (m_selectedSurface >= (int)m_surfaces.size()) m_selectedSurface = -1;
    }
}

// --- ScreenCluster ---

glm::mat4 ScreenCluster::getTransform() const {
    glm::mat4 t = glm::translate(glm::mat4(1.0f), position);
    t = glm::rotate(t, glm::radians(rotation.y), glm::vec3(0, 1, 0));
    t = glm::rotate(t, glm::radians(rotation.x), glm::vec3(1, 0, 0));
    t = glm::rotate(t, glm::radians(rotation.z), glm::vec3(0, 0, 1));
    t = glm::scale(t, glm::vec3(scale));
    return t;
}

int StageView::addCluster(const std::string& name) {
    ScreenCluster cluster;
    cluster.name = name;
    m_clusters.push_back(cluster);
    return (int)m_clusters.size() - 1;
}

void StageView::removeCluster(int index) {
    if (index >= 0 && index < (int)m_clusters.size()) {
        // Unlink any surfaces belonging to this cluster
        for (auto& surf : m_surfaces) {
            if (surf.clusterIndex == index) surf.clusterIndex = -1;
            else if (surf.clusterIndex > index) surf.clusterIndex--;
        }
        m_clusters.erase(m_clusters.begin() + index);
        if (m_selectedCluster == index)       m_selectedCluster = -1;
        else if (m_selectedCluster > index)   m_selectedCluster--;
        if (m_selectedCluster >= (int)m_clusters.size()) m_selectedCluster = -1;
    }
}

void StageView::populateClusterGrid(int clusterIdx, int startZone) {
    if (clusterIdx < 0 || clusterIdx >= (int)m_clusters.size()) return;
    auto& cl = m_clusters[clusterIdx];

    // Remove existing surfaces in this cluster
    for (int i = (int)m_surfaces.size() - 1; i >= 0; i--) {
        if (m_surfaces[i].clusterIndex == clusterIdx)
            m_surfaces.erase(m_surfaces.begin() + i);
    }

    // Create grid surfaces
    int zone = startZone;
    for (int r = 0; r < cl.gridRows; r++) {
        for (int c = 0; c < cl.gridCols; c++) {
            ProjectionSurface surf;
            surf.name = cl.name + " " + std::to_string(r * cl.gridCols + c + 1);
            surf.clusterIndex = clusterIdx;
            surf.projectorIndex = std::min(zone, (int)m_projectors.size() - 1);
            if (surf.projectorIndex < 0) surf.projectorIndex = 0;
            m_surfaces.push_back(surf);
            zone++;
        }
    }
}

// --- StageDisplay ---

glm::mat4 StageDisplay::getTransform() const {
    glm::mat4 t = glm::translate(glm::mat4(1.0f), position);
    t = glm::rotate(t, glm::radians(rotation.y), glm::vec3(0, 1, 0));
    t = glm::rotate(t, glm::radians(rotation.x), glm::vec3(1, 0, 0));
    t = glm::rotate(t, glm::radians(rotation.z), glm::vec3(0, 0, 1));
    // Scale the unit quad (-1..1) to actual display dimensions (half-extents)
    t = glm::scale(t, glm::vec3(width * 0.5f, height * 0.5f, 1.0f));
    return t;
}

int StageView::addDisplay(const std::string& name, StageDisplay::Type type) {
    StageDisplay d;
    d.name = name;
    d.type = type;
    d.zoneIndex = 0;
    // Spread displays out along X so they don't overlap
    d.position.x = (float)m_displays.size() * 2.5f;
    m_displays.push_back(d);
    m_selectedDisplay = (int)m_displays.size() - 1;
    return m_selectedDisplay;
}

void StageView::removeDisplay(int index) {
    if (index >= 0 && index < (int)m_displays.size()) {
        m_displays.erase(m_displays.begin() + index);
        if (m_selectedDisplay == index)       m_selectedDisplay = -1;
        else if (m_selectedDisplay > index)   m_selectedDisplay--;
        if (m_selectedDisplay >= (int)m_displays.size()) m_selectedDisplay = -1;
    }
}

void StageView::renderDisplays(const std::vector<GLuint>& zoneTextures, const glm::mat4& viewProj) {
    if (m_displays.empty()) return;

    // Load display shader on first use
    if (!m_displayShaderReady) {
        if (m_displayShader.loadFromFiles("shaders/display3d.vert", "shaders/display3d.frag")) {
            m_displayShaderReady = true;
            m_displayQuad.createQuad(); // unit quad -1..1
        }
    }
    if (!m_displayShaderReady) return;

    glEnable(GL_DEPTH_TEST);

    for (int i = 0; i < (int)m_displays.size(); i++) {
        auto& d = m_displays[i];
        if (!d.visible) continue;

        glm::mat4 model = d.getTransform();
        glm::mat4 mvp = viewProj * model;

        m_displayShader.use();
        m_displayShader.setMat4("uMVP", mvp);
        m_displayShader.setFloat("uOpacity", 1.0f);
        m_displayShader.setInt("uTexture", 0);

        // Border color: cyan normally, yellow when selected
        bool sel = (i == m_selectedDisplay);
        if (sel) {
            m_displayShader.setVec4("uBorderColor", glm::vec4(1.0f, 0.85f, 0.2f, 1.0f));
            m_displayShader.setFloat("uBorderWidth", 0.015f);
        } else {
            m_displayShader.setVec4("uBorderColor", glm::vec4(1.0f, 1.0f, 1.0f, 0.6f));
            m_displayShader.setFloat("uBorderWidth", 0.008f);
        }

        // Bind zone texture
        glActiveTexture(GL_TEXTURE0);
        if (d.zoneIndex >= 0 && d.zoneIndex < (int)zoneTextures.size() && zoneTextures[d.zoneIndex]) {
            glBindTexture(GL_TEXTURE_2D, zoneTextures[d.zoneIndex]);
        } else {
            glBindTexture(GL_TEXTURE_2D, 0);
        }

        m_displayQuad.draw();
    }
}

// Floor + back wall. Lazily builds two large quads and draws them with the
// objmesh shader in solid-color mode. Keeps the 3D viewport from feeling
// like a void so users can read depth and orientation at a glance.
void StageView::renderEnvironment(const glm::mat4& viewProj) {
    if (!m_envReady) {
        // Floor: 60m × 60m so the camera never sees the edge. Smaller
        // floors left a visible square outline floating in space when
        // the user orbited around — the user reported this as a stray
        // viewport outline.
        const float fHalf = 30.0f;
        const float fY    = -0.55f;
        std::vector<Vertex3D> fv = {
            {-fHalf, fY, -fHalf, 0.0f, 0.0f},
            { fHalf, fY, -fHalf, 1.0f, 0.0f},
            { fHalf, fY,  fHalf, 1.0f, 1.0f},
            {-fHalf, fY,  fHalf, 0.0f, 1.0f},
        };
        std::vector<unsigned int> fi = { 0, 1, 2, 0, 2, 3 };
        m_floorMesh.upload(fv, fi);

        // Back wall: very wide and tall, at z = -2 behind the origin.
        const float wHalfX = 30.0f;
        const float wYBot  = -0.55f;
        const float wYTop  =  18.0f;
        const float wZ     = -2.0f;
        std::vector<Vertex3D> wv = {
            {-wHalfX, wYBot, wZ, 0.0f, 0.0f},
            { wHalfX, wYBot, wZ, 1.0f, 0.0f},
            { wHalfX, wYTop, wZ, 1.0f, 1.0f},
            {-wHalfX, wYTop, wZ, 0.0f, 1.0f},
        };
        std::vector<unsigned int> wi = { 0, 1, 2, 0, 2, 3 };
        m_wallMesh.upload(wv, wi);

        m_envReady = true;
    }

    if (!m_envVisible) return;

    m_shader.use();
    m_shader.setMat4("uMVP", viewProj);  // identity model — vertices are in world space
    m_shader.setBool("uTextured", false);

    // Floor — slightly lighter to catch the eye as "ground".
    m_shader.setVec4("uSolidColor", glm::vec4(0.16f, 0.17f, 0.20f, 1.0f));
    m_floorMesh.draw();

    // Back wall — a touch darker so it reads as behind the floor.
    m_shader.setVec4("uSolidColor", glm::vec4(0.12f, 0.13f, 0.16f, 1.0f));
    m_wallMesh.draw();
}

void StageView::renderScene(const std::vector<GLuint>& zoneTextures, float aspect) {
    glm::mat4 mdl = glm::scale(glm::mat4(1.0f), glm::vec3(m_modelScale));
    glm::mat4 view = m_camera.viewMatrix();
    glm::mat4 proj = m_camera.projMatrix(aspect);
    glm::mat4 mvp = proj * view * mdl;
    glm::mat4 viewProjStage = proj * view;

    glEnable(GL_DEPTH_TEST);
    glClear(GL_DEPTH_BUFFER_BIT);

    // Floor + back wall first, so displays and frustums render on top of them.
    renderEnvironment(viewProjStage);

    m_shader.use();
    m_shader.setMat4("uMVP", mvp);
    m_shader.setInt("uTexture", 0);

    // Determine which materials have projection surfaces assigned
    std::vector<int> matToZone(m_materials.size(), -1);
    for (const auto& surf : m_surfaces) {
        if (surf.active && surf.materialIndex >= 0 && surf.materialIndex < (int)m_materials.size()) {
            if (surf.projectorIndex >= 0 && surf.projectorIndex < (int)m_projectors.size()) {
                matToZone[surf.materialIndex] = m_projectors[surf.projectorIndex].zoneIndex;
            }
        }
    }

    for (int i = 0; i < (int)m_materials.size(); i++) {
        auto& mg = m_materials[i];
        if (!mg.mesh.isLoaded()) continue;

        int zoneIdx = matToZone[i];
        if (zoneIdx >= 0 && zoneIdx < (int)zoneTextures.size() && zoneTextures[zoneIdx]) {
            // This material has a projection surface - show the zone's content
            m_shader.setBool("uTextured", true);
            glActiveTexture(GL_TEXTURE0);
            glBindTexture(GL_TEXTURE_2D, zoneTextures[zoneIdx]);
        } else {
            // Untextured - dark solid
            m_shader.setBool("uTextured", false);
            m_shader.setVec4("uSolidColor", glm::vec4(0.06f, 0.07f, 0.09f, 1.0f));
        }
        mg.mesh.draw();
    }

    // Wireframe overlay for structure
    glPolygonMode(GL_FRONT_AND_BACK, GL_LINE);
    glEnable(GL_POLYGON_OFFSET_LINE);
    glPolygonOffset(-1.0f, -1.0f);
    m_shader.setBool("uTextured", false);
    m_shader.setVec4("uSolidColor", glm::vec4(0.2f, 0.22f, 0.28f, 1.0f));
    for (auto& mg : m_materials) {
        if (mg.mesh.isLoaded()) mg.mesh.draw();
    }
    glDisable(GL_POLYGON_OFFSET_LINE);
    glPolygonMode(GL_FRONT_AND_BACK, GL_FILL);

    // Draw projector frustums
    for (int i = 0; i < (int)m_projectors.size(); i++) {
        if (!m_projectors[i].visible) continue;
        renderFrustum(m_projectors[i], viewProjStage, i == m_selectedProjector);
    }

    // Draw display planes (textured quads in 3D space)
    renderDisplays(zoneTextures, viewProjStage);

    glDisable(GL_DEPTH_TEST);
}

void StageView::renderFrustum(const VirtualProjector& proj, const glm::mat4& stageVP, bool selected) {
    // Compute frustum corners in world space
    glm::mat4 invVP = glm::inverse(proj.vpMatrix());
    glm::vec3 corners[8];
    int idx = 0;
    for (float z : {0.0f, 1.0f}) {
        for (float y : {-1.0f, 1.0f}) {
            for (float x : {-1.0f, 1.0f}) {
                glm::vec4 ndc(x, y, z * 2.0f - 1.0f, 1.0f);
                glm::vec4 world = invVP * ndc;
                corners[idx++] = glm::vec3(world) / world.w;
            }
        }
    }

    // Draw lines via ImGui's foreground draw list. Anchor the screen
    // mapping to the viewport image rect (set by renderUI), not the
    // whole window — otherwise toolbars / scroll regions push the
    // overlay off-image.
    ImDrawList* draw = ImGui::GetForegroundDrawList();
    ImVec2 wMin (m_viewportScreenPos.x,  m_viewportScreenPos.y);
    ImVec2 wSize(m_viewportScreenSize.x, m_viewportScreenSize.y);

    auto project = [&](glm::vec3 worldPos) -> ImVec2 {
        glm::vec4 clip = stageVP * glm::vec4(worldPos, 1.0f);
        if (clip.w <= 0.001f) return ImVec2(-999, -999);
        glm::vec3 ndc = glm::vec3(clip) / clip.w;
        float sx = wMin.x + (ndc.x * 0.5f + 0.5f) * wSize.x;
        float sy = wMin.y + (1.0f - (ndc.y * 0.5f + 0.5f)) * wSize.y;
        return ImVec2(sx, sy);
    };

    ImU32 col = selected ? IM_COL32(255, 200, 50, 200) : IM_COL32(255, 255, 255, 120);
    ImU32 colDim = selected ? IM_COL32(255, 200, 50, 60) : IM_COL32(255, 255, 255, 40);

    // Near plane
    for (int i = 0; i < 4; i++) {
        ImVec2 a = project(corners[i]);
        ImVec2 b = project(corners[(i + 1) % 4]);
        draw->AddLine(a, b, col, 1.5f);
    }
    // Far plane
    for (int i = 4; i < 8; i++) {
        ImVec2 a = project(corners[i]);
        ImVec2 b = project(corners[4 + (i - 4 + 1) % 4]);
        draw->AddLine(a, b, colDim, 1.0f);
    }
    // Edges connecting near to far
    for (int i = 0; i < 4; i++) {
        draw->AddLine(project(corners[i]), project(corners[i + 4]), colDim, 1.0f);
    }

    // Projector position dot
    ImVec2 posScreen = project(proj.position);
    draw->AddCircleFilled(posScreen, selected ? 6.0f : 4.0f, col);
    draw->AddText(ImVec2(posScreen.x + 10, posScreen.y - 8), col, proj.name.c_str());
}

void StageView::render(const std::vector<GLuint>& zoneTextures,
                       std::function<void()> inlineTopSection) {
    ImGui::SetNextWindowSizeConstraints(ImVec2(300, 200), ImVec2(FLT_MAX, FLT_MAX));
    ImGui::Begin("Stage");

    // Composition/output inline sections from Application live at the top
    // of the Stage tab so that canvas size and output target are set where
    // you're physically staging the projection.
    if (inlineTopSection) {
        inlineTopSection();
        ImGui::Dummy(ImVec2(0, 2));
        ImDrawList* dl = ImGui::GetWindowDrawList();
        ImVec2 p = ImGui::GetCursorScreenPos();
        float w = ImGui::GetContentRegionAvail().x;
        dl->AddLine(ImVec2(p.x, p.y), ImVec2(p.x + w, p.y), IM_COL32(255, 255, 255, 25));
        ImGui::Dummy(ImVec2(0, 4));
    }

    renderUI(zoneTextures);

    ImGui::End();
}

void StageView::renderToolbar() {
    // Floating pill toolbar — long rounded container with circular icon
    // buttons inside. Sits at the top of the Stage workspace as an
    // overlay above the 3D viewport. Tools, left-to-right:
    //   Floor (toggle) | Wall (toggle) | + Display | + Surface ▾ | Import
    // The Projector button is hidden until that workflow is real.
    // Mapping/Masks live in the right-rail Properties panel now,
    // not in this toolbar.
    const float kBtnSize  = 36.0f;        // diameter of each circular button
    const float kBtnGap   = 6.0f;         // gap between buttons
    const float kPillPad  = 6.0f;         // inner padding inside the pill
    const float kPillH    = kBtnSize + kPillPad * 2.0f;

    struct ToolBtn {
        const char* id;
        const char* tip;
        char glyph;          // 0..3 = floor/wall/display/surface, 4 = import
        int  kind;           // matches glyph
        bool toggleOn;       // for stateful toggles (Floor/Wall)
        bool isToggle;
    };

    bool floorOn = m_envVisible; // current Floor/Wall flag covers both
    bool wallOn  = m_envVisible;
    ToolBtn buttons[] = {
        {"##Floor",   "Show floor",       0, 0, floorOn, true},
        {"##Wall",    "Show wall",        0, 1, wallOn,  true},
        {"##AddDisp", "Add display",      0, 2, false,   false},
        {"##AddSurf", "Add surface",      0, 3, false,   false},
        {"##Import",  "Import...",        0, 4, false,   false},
    };
    const int kBtnCount = (int)(sizeof(buttons) / sizeof(buttons[0]));

    float pillW = kPillPad * 2.0f + kBtnSize * kBtnCount + kBtnGap * (kBtnCount - 1);

    // Center the pill horizontally inside the Stage panel.
    float availX = ImGui::GetContentRegionAvail().x;
    float startX = ImGui::GetCursorPosX() + std::max(0.0f, (availX - pillW) * 0.5f);
    ImVec2 pillPos = ImVec2(ImGui::GetWindowPos().x + startX,
                            ImGui::GetCursorScreenPos().y);

    ImDrawList* dl = ImGui::GetWindowDrawList();

    // Pill background — same dark widget tint as other floating chrome
    // (matches the CANVAS/STAGE/SHOW segmented control track).
    dl->AddRectFilled(
        pillPos,
        ImVec2(pillPos.x + pillW, pillPos.y + kPillH),
        IM_COL32(28, 30, 34, 235),
        kPillH * 0.5f);
    dl->AddRect(
        pillPos,
        ImVec2(pillPos.x + pillW, pillPos.y + kPillH),
        IM_COL32(255, 255, 255, 24),
        kPillH * 0.5f, 0, 1.0f);

    // Per-button glyph drawing — FILLED silhouettes, scale with kBtnSize.
    auto drawGlyph = [&](ImVec2 c, int kind, ImU32 col) {
        const float r = kBtnSize * 0.32f;
        switch (kind) {
        case 0: { // Floor — filled trapezoid (perspective slab)
            ImVec2 pts[4] = {
                ImVec2(c.x - r,           c.y + r * 0.35f),
                ImVec2(c.x + r,           c.y + r * 0.35f),
                ImVec2(c.x + r * 0.55f,   c.y - r * 0.15f),
                ImVec2(c.x - r * 0.55f,   c.y - r * 0.15f),
            };
            dl->AddConvexPolyFilled(pts, 4, col);
            break; }
        case 1: { // Wall — filled rounded rectangle
            dl->AddRectFilled(ImVec2(c.x - r * 0.75f, c.y - r),
                              ImVec2(c.x + r * 0.75f, c.y + r), col, 2.0f);
            break; }
        case 2: { // Display/Monitor — filled rect with stand triangle below
            dl->AddRectFilled(ImVec2(c.x - r, c.y - r * 0.75f),
                              ImVec2(c.x + r, c.y + r * 0.30f), col, 2.0f);
            ImVec2 stand[3] = {
                ImVec2(c.x - r * 0.45f, c.y + r * 0.85f),
                ImVec2(c.x + r * 0.45f, c.y + r * 0.85f),
                ImVec2(c.x,             c.y + r * 0.30f),
            };
            dl->AddConvexPolyFilled(stand, 3, col);
            break; }
        case 3: { // Surface — filled diamond
            ImVec2 pts[4] = {
                ImVec2(c.x,         c.y - r),
                ImVec2(c.x + r,     c.y),
                ImVec2(c.x,         c.y + r),
                ImVec2(c.x - r,     c.y),
            };
            dl->AddConvexPolyFilled(pts, 4, col);
            break; }
        case 4: { // Import — filled down-arrow into filled tray
            // Decompose concave arrow into two convex pieces:
            //   shaft (rectangle) + arrowhead (triangle)
            dl->AddRectFilled(ImVec2(c.x - r * 0.22f, c.y - r * 0.85f),
                              ImVec2(c.x + r * 0.22f, c.y - r * 0.05f), col, 1.0f);
            ImVec2 head[3] = {
                ImVec2(c.x - r * 0.55f, c.y - r * 0.10f),
                ImVec2(c.x + r * 0.55f, c.y - r * 0.10f),
                ImVec2(c.x,             c.y + r * 0.50f),
            };
            dl->AddConvexPolyFilled(head, 3, col);
            // Tray bar
            dl->AddRectFilled(ImVec2(c.x - r,        c.y + r * 0.65f),
                              ImVec2(c.x + r,        c.y + r * 0.90f), col, 1.5f);
            break; }
        }
    };

    // Hit zones + circular fills.
    float bx = pillPos.x + kPillPad;
    float by = pillPos.y + kPillPad;
    int clickedIdx = -1;

    for (int i = 0; i < kBtnCount; i++) {
        ImGui::SetCursorScreenPos(ImVec2(bx, by));
        ImGui::PushID(buttons[i].id);
        bool clicked = ImGui::InvisibleButton("##btn",
                                              ImVec2(kBtnSize, kBtnSize));
        bool hov     = ImGui::IsItemHovered();
        ImGui::PopID();

        bool active = buttons[i].isToggle && buttons[i].toggleOn;

        ImVec2 center(bx + kBtnSize * 0.5f, by + kBtnSize * 0.5f);
        ImU32 fill = active
            ? IM_COL32(247, 248, 248, 255)             // active = white
            : (hov
                ? IM_COL32(255, 255, 255, 28)
                : IM_COL32(255, 255, 255, 12));
        dl->AddCircleFilled(center, kBtnSize * 0.5f - 1.0f, fill, 64);

        ImU32 fg = active
            ? IM_COL32(13, 18, 26, 255)
            : IM_COL32(220, 226, 235, 230);
        drawGlyph(center, buttons[i].kind, fg);

        if (hov) ParamRow::Tooltip(buttons[i].tip);
        if (clicked) clickedIdx = i;

        bx += kBtnSize + kBtnGap;
    }

    // Advance the cursor past the pill so subsequent rows lay out below.
    ImGui::SetCursorPosY(ImGui::GetCursorPosY() + kPillH + 6.0f);

    // Action dispatch.
    if (clickedIdx == 0 || clickedIdx == 1) {
        // Floor / Wall toggle — currently both share m_envVisible.
        // (Independent toggles need a backend split that's pending.)
        m_envVisible = !m_envVisible;
    } else if (clickedIdx == 2) {
        char name[32];
        snprintf(name, sizeof(name), "Display %d", (int)m_displays.size() + 1);
        addDisplay(name, StageDisplay::Type::LED);
    } else if (clickedIdx == 3) {
        // + Surface opens a shape picker popup so the user can choose
        // a primitive (rect / circle / triangle / octagon).
        ImGui::OpenPopup("##SurfaceShapePicker");
    } else if (clickedIdx == 4) {
        m_wantsImport = true;
    }

    if (ImGui::BeginPopup("##SurfaceShapePicker")) {
        ImGui::TextDisabled("Add surface");
        ImGui::Separator();
        const char* shapes[] = {"Rectangle", "Circle", "Triangle", "Octagon"};
        for (int s = 0; s < (int)(sizeof(shapes) / sizeof(shapes[0])); s++) {
            if (ImGui::MenuItem(shapes[s])) {
                char nm[32];
                snprintf(nm, sizeof(nm), "%s %d", shapes[s], (int)m_surfaces.size() + 1);
                int matIdx = m_materials.empty() ? -1 : 0;
                int prjIdx = m_projectors.empty() ? -1 : 0;
                addSurface(nm, matIdx, prjIdx);
            }
        }
        ImGui::EndPopup();
    }
}

void StageView::renderFloatingToolbar() {
    // Vertical pill at the left edge of the viewport, where the Layers
    // panel sits in Canvas mode. Four circular icon buttons stacked
    // top-to-bottom: Room (env preset dropdown), Add Display,
    // Add Surface (shape picker), Import. Bigger buttons + more inner
    // padding so each glyph reads cleanly with breathing room.
    const float kBtnSize = 44.0f;
    const float kBtnGap  = 8.0f;
    const float kPillPad = 10.0f;
    const float kPillW   = kBtnSize + kPillPad * 2.0f;
    // 4 add/import + 3 gizmo tools (select / rotate / scale) so users can
    // switch transform mode without leaving the canvas.
    const int   kBtnCount = 7;
    const float kPillH   = kPillPad * 2.0f
                         + kBtnSize * kBtnCount
                         + kBtnGap  * (kBtnCount - 1);

    // Position: left edge, vertically CENTRED in the available canvas
    // height (so the rounded ends never get clipped against the
    // secondary nav above or the timeline below).
    ImGuiViewport* vp = ImGui::GetMainViewport();
    float headerReserve = ImGui::GetFrameHeight() + 22.0f;
    float bandTop    = vp->WorkPos.y + headerReserve + 18.0f;
    float bandHeight = vp->WorkSize.y - headerReserve - 18.0f - 80.0f; // -timeline strip
    float topY       = bandTop + std::max(0.0f, (bandHeight - kPillH) * 0.5f);

    ImGuiWindowFlags flags =
        ImGuiWindowFlags_NoTitleBar |
        ImGuiWindowFlags_NoResize |
        ImGuiWindowFlags_NoMove |
        ImGuiWindowFlags_NoCollapse |
        ImGuiWindowFlags_NoDocking |
        ImGuiWindowFlags_NoSavedSettings |
        ImGuiWindowFlags_NoBackground |
        ImGuiWindowFlags_NoScrollbar;

    // Pad the host window so the pill's rounded ends fit inside its
    // content rect without being clipped by the window's own rounding.
    const float kHostExtra = 8.0f;
    ImGui::SetNextWindowPos (ImVec2(vp->WorkPos.x + 12.0f, topY - kHostExtra),
                             ImGuiCond_Always);
    ImGui::SetNextWindowSize(ImVec2(kPillW + kHostExtra * 2.0f,
                                    kPillH + kHostExtra * 2.0f),
                             ImGuiCond_Always);
    ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding,  ImVec2(0, 0));
    ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding, 0.0f);
    ImGui::PushStyleVar(ImGuiStyleVar_WindowBorderSize, 0.0f);
    if (!ImGui::Begin("##StageToolbarFloat", nullptr, flags)) {
        ImGui::End();
        ImGui::PopStyleVar(3);
        return;
    }
    ImGui::PopStyleVar(3);

    ImDrawList* dl = ImGui::GetWindowDrawList();
    // Inset the pill from the host window so its rounded ends sit
    // inside the host's content rect without being clipped.
    ImVec2 pillPos(ImGui::GetWindowPos().x + kHostExtra,
                   ImGui::GetWindowPos().y + kHostExtra);
    dl->AddRectFilled(
        pillPos,
        ImVec2(pillPos.x + kPillW, pillPos.y + kPillH),
        IM_COL32(28, 30, 34, 235),
        kPillW * 0.5f);
    dl->AddRect(
        pillPos,
        ImVec2(pillPos.x + kPillW, pillPos.y + kPillH),
        IM_COL32(255, 255, 255, 24),
        kPillW * 0.5f, 0, 1.0f);

    auto drawGlyph = [&](ImVec2 c, int kind, ImU32 col) {
        const float r = kBtnSize * 0.32f;
        switch (kind) {
        case 0: { // Room — filled isometric cube (top + left + right faces)
            // Top face (rhombus, lighter)
            ImU32 colTop  = (col & 0x00FFFFFF) | (((col >> 24) & 0xFF) << 24);
            // Use alpha modulation for a flat-but-readable cube: top brightest,
            // left/right dimmer. Simulate by mixing col with two tones.
            // Compose two extra tones from `col` alpha.
            int  a   = (col >> 24) & 0xFF;
            ImU32 colMid = (col & 0x00FFFFFF) | ((unsigned)((a * 70) / 100) << 24);
            ImU32 colDim = (col & 0x00FFFFFF) | ((unsigned)((a * 45) / 100) << 24);
            (void)colTop;
            // Top rhombus
            ImVec2 top[4] = {
                ImVec2(c.x,             c.y - r),
                ImVec2(c.x + r,         c.y - r * 0.5f),
                ImVec2(c.x,             c.y),
                ImVec2(c.x - r,         c.y - r * 0.5f),
            };
            dl->AddConvexPolyFilled(top, 4, col);
            // Left face
            ImVec2 left[4] = {
                ImVec2(c.x - r,         c.y - r * 0.5f),
                ImVec2(c.x,             c.y),
                ImVec2(c.x,             c.y + r),
                ImVec2(c.x - r,         c.y + r * 0.5f),
            };
            dl->AddConvexPolyFilled(left, 4, colMid);
            // Right face
            ImVec2 right[4] = {
                ImVec2(c.x,             c.y),
                ImVec2(c.x + r,         c.y - r * 0.5f),
                ImVec2(c.x + r,         c.y + r * 0.5f),
                ImVec2(c.x,             c.y + r),
            };
            dl->AddConvexPolyFilled(right, 4, colDim);
            break; }
        case 1: { // Display — filled rect with stand triangle below
            dl->AddRectFilled(ImVec2(c.x - r,        c.y - r * 0.75f),
                              ImVec2(c.x + r,        c.y + r * 0.30f), col, 2.0f);
            ImVec2 stand[3] = {
                ImVec2(c.x - r * 0.45f, c.y + r * 0.85f),
                ImVec2(c.x + r * 0.45f, c.y + r * 0.85f),
                ImVec2(c.x,             c.y + r * 0.30f),
            };
            dl->AddConvexPolyFilled(stand, 3, col);
            break; }
        case 2: { // Surface — filled diamond
            ImVec2 pts[4] = {
                ImVec2(c.x,         c.y - r),
                ImVec2(c.x + r,     c.y),
                ImVec2(c.x,         c.y + r),
                ImVec2(c.x - r,     c.y),
            };
            dl->AddConvexPolyFilled(pts, 4, col);
            break; }
        case 3: { // Import — filled down-arrow into filled tray
            dl->AddRectFilled(ImVec2(c.x - r * 0.22f, c.y - r * 0.85f),
                              ImVec2(c.x + r * 0.22f, c.y - r * 0.05f), col, 1.0f);
            ImVec2 head[3] = {
                ImVec2(c.x - r * 0.55f, c.y - r * 0.10f),
                ImVec2(c.x + r * 0.55f, c.y - r * 0.10f),
                ImVec2(c.x,             c.y + r * 0.50f),
            };
            dl->AddConvexPolyFilled(head, 3, col);
            dl->AddRectFilled(ImVec2(c.x - r,        c.y + r * 0.65f),
                              ImVec2(c.x + r,        c.y + r * 0.90f), col, 1.5f);
            break; }
        case 4: { // Select / move — filled cursor arrow
            ImVec2 ptr[7] = {
                ImVec2(c.x - r * 0.55f, c.y - r * 0.75f),
                ImVec2(c.x + r * 0.20f, c.y),
                ImVec2(c.x - r * 0.10f, c.y + r * 0.10f),
                ImVec2(c.x + r * 0.18f, c.y + r * 0.65f),
                ImVec2(c.x - r * 0.05f, c.y + r * 0.78f),
                ImVec2(c.x - r * 0.32f, c.y + r * 0.20f),
                ImVec2(c.x - r * 0.55f, c.y + r * 0.40f),
            };
            dl->AddConvexPolyFilled(ptr, 7, col);
            break; }
        case 5: { // Rotate — filled arc with arrowhead
            // Arc is non-convex; build it from triangle fan strips between
            // outer/inner radii.
            const int seg = 22;
            const float aStart = -1.4f;
            const float aSpan  = 5.0f;
            const float rOut   = r * 0.95f;
            const float rIn    = r * 0.62f;
            for (int s = 0; s < seg; s++) {
                float a0 = aStart + (s    / (float)seg) * aSpan;
                float a1 = aStart + ((s+1)/ (float)seg) * aSpan;
                ImVec2 q[4] = {
                    ImVec2(c.x + cosf(a0) * rIn,  c.y + sinf(a0) * rIn),
                    ImVec2(c.x + cosf(a0) * rOut, c.y + sinf(a0) * rOut),
                    ImVec2(c.x + cosf(a1) * rOut, c.y + sinf(a1) * rOut),
                    ImVec2(c.x + cosf(a1) * rIn,  c.y + sinf(a1) * rIn),
                };
                dl->AddConvexPolyFilled(q, 4, col);
            }
            // Arrowhead at the end
            float ae = aStart + aSpan;
            ImVec2 tip(c.x + cosf(ae) * (rOut + r * 0.30f),
                       c.y + sinf(ae) * (rOut + r * 0.30f));
            float perpx = -sinf(ae);
            float perpy =  cosf(ae);
            ImVec2 baseA(c.x + cosf(ae) * rOut + perpx * r * 0.20f,
                         c.y + sinf(ae) * rOut + perpy * r * 0.20f);
            ImVec2 baseB(c.x + cosf(ae) * rIn  - perpx * r * 0.20f,
                         c.y + sinf(ae) * rIn  - perpy * r * 0.20f);
            ImVec2 head[3] = { tip, baseA, baseB };
            dl->AddConvexPolyFilled(head, 3, col);
            break; }
        case 6: { // Scale — filled diagonal arrow with corner triangles
            // Diagonal shaft as rotated rectangle (BL → TR)
            float t = r * 0.10f; // half-thickness
            ImVec2 shaft[4] = {
                ImVec2(c.x - r * 0.55f - t, c.y + r * 0.55f - t),
                ImVec2(c.x - r * 0.55f + t, c.y + r * 0.55f + t),
                ImVec2(c.x + r * 0.55f + t, c.y - r * 0.55f + t),
                ImVec2(c.x + r * 0.55f - t, c.y - r * 0.55f - t),
            };
            dl->AddConvexPolyFilled(shaft, 4, col);
            // Bottom-left corner triangle
            ImVec2 blt[3] = {
                ImVec2(c.x - r * 0.85f, c.y + r * 0.85f),
                ImVec2(c.x - r * 0.10f, c.y + r * 0.85f),
                ImVec2(c.x - r * 0.85f, c.y + r * 0.10f),
            };
            dl->AddConvexPolyFilled(blt, 3, col);
            // Top-right corner triangle
            ImVec2 trt[3] = {
                ImVec2(c.x + r * 0.85f, c.y - r * 0.85f),
                ImVec2(c.x + r * 0.10f, c.y - r * 0.85f),
                ImVec2(c.x + r * 0.85f, c.y - r * 0.10f),
            };
            dl->AddConvexPolyFilled(trt, 3, col);
            break; }
        }
    };

    const char* tips[]     = {"Environment", "Add display", "Add surface", "Import...",
                              "Select / Move (V)", "Rotate (R)", "Scale (S)"};
    const bool  isToggle[] = {false,         false,         false,         false,
                              true,                  true,         true};

    float bx = pillPos.x + kPillPad;
    float by = pillPos.y + kPillPad;
    int clickedIdx = -1;
    for (int i = 0; i < kBtnCount; i++) {
        ImGui::SetCursorScreenPos(ImVec2(bx, by));
        ImGui::PushID(i + 100);
        bool clicked = ImGui::InvisibleButton("##fb", ImVec2(kBtnSize, kBtnSize));
        bool hov     = ImGui::IsItemHovered();
        ImGui::PopID();
        // Active state: tools 4-6 highlight when current gizmo op matches
        // (i-4 maps to translate=0 / rotate=1 / scale=2). Tool 0 (env)
        // would highlight when m_envVisible — but it's not currently
        // wired as a toggle, so the existing behaviour (no-active env
        // button) is preserved by leaving it false.
        bool active = false;
        if (isToggle[i] && i >= 4 && i <= 6) {
            active = (m_gizmoOp == (i - 4));
        }

        ImVec2 center(bx + kBtnSize * 0.5f, by + kBtnSize * 0.5f);
        ImU32 fill = active
            ? IM_COL32(247, 248, 248, 255)
            : (hov ? IM_COL32(255, 255, 255, 28) : IM_COL32(255, 255, 255, 12));
        dl->AddCircleFilled(center, kBtnSize * 0.5f - 1.0f, fill, 64);

        ImU32 fg = active ? IM_COL32(13, 18, 26, 255)
                          : IM_COL32(220, 226, 235, 230);
        drawGlyph(center, i, fg);

        if (hov) ParamRow::Tooltip(tips[i]);
        if (clicked) clickedIdx = i;

        by += kBtnSize + kBtnGap;
    }

    // Toggle sub-menus on click. Mutually exclusive — opening one
    // closes the other so we never stack two pickers on top of each
    // other next to the toolbar.
    if (clickedIdx == 0) {
        m_envMenuOpen     = !m_envMenuOpen;
        m_surfaceMenuOpen = false;
    } else if (clickedIdx == 1) {
        char name[32];
        snprintf(name, sizeof(name), "Display %d", (int)m_displays.size() + 1);
        addDisplay(name, StageDisplay::Type::LED);
        m_envMenuOpen = m_surfaceMenuOpen = false;
    } else if (clickedIdx == 2) {
        m_surfaceMenuOpen = !m_surfaceMenuOpen;
        m_envMenuOpen     = false;
    } else if (clickedIdx == 3) {
        m_wantsImport = true;
        m_envMenuOpen = m_surfaceMenuOpen = false;
    } else if (clickedIdx >= 4 && clickedIdx <= 6) {
        // Gizmo tool selection: 4=translate, 5=rotate, 6=scale.
        m_gizmoOp = clickedIdx - 4;
        m_envMenuOpen = m_surfaceMenuOpen = false;
    }

    ImGui::End();

    // ── Sub-menus rendered as separate peer floating windows ──
    // Anchored to the right of the toolbar's button row at the
    // appropriate Y for the button that opened them. Plain Begin
    // windows (not ImGui popups) so they don't fight with the host
    // window's borderless flags.
    auto subMenu = [&](const char* id, int btnIndex,
                       const char* title,
                       const std::initializer_list<const char*>& items,
                       bool& openFlag,
                       std::function<void(int idx)> onPick)
    {
        if (!openFlag) return;
        const float kMenuW = 200.0f;
        float menuY = pillPos.y + kPillPad + btnIndex * (kBtnSize + kBtnGap);
        float menuX = pillPos.x + kPillW + 8.0f;
        ImGui::SetNextWindowPos (ImVec2(menuX, menuY), ImGuiCond_Always);
        ImGui::SetNextWindowSize(ImVec2(kMenuW, 0),    ImGuiCond_Always);
        ImGuiWindowFlags mf = ImGuiWindowFlags_NoTitleBar
                            | ImGuiWindowFlags_NoResize
                            | ImGuiWindowFlags_NoMove
                            | ImGuiWindowFlags_NoCollapse
                            | ImGuiWindowFlags_NoDocking
                            | ImGuiWindowFlags_NoSavedSettings
                            | ImGuiWindowFlags_AlwaysAutoResize;
        ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding,  ImVec2(8, 8));
        ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding, 10.0f);
        if (ImGui::Begin(id, &openFlag, mf)) {
            ImGui::TextDisabled("%s", title);
            ImGui::Separator();
            int i = 0;
            for (const char* it : items) {
                if (ImGui::Selectable(it)) {
                    onPick(i);
                    openFlag = false;
                }
                i++;
            }
        }
        ImGui::End();
        ImGui::PopStyleVar(2);
    };

    subMenu("##StageEnvMenu", 0, "Environment",
            {"Floor", "Wall", "3D Room", "Seamless backdrop", "Frame"},
            m_envMenuOpen,
            [this](int idx) {
                // Every item toggles visibility for now so the click is
                // visibly responsive. Floor/Wall/Room/Seamless/Frame
                // presets will get distinct geometry when authoring lands.
                (void)idx;
                m_envVisible = !m_envVisible;
            });

    subMenu("##StageSurfaceMenu", 2, "Add surface",
            {"Rectangle", "Circle", "Triangle", "Octagon"},
            m_surfaceMenuOpen,
            [this](int idx) {
                static const char* names[] = {"Rectangle","Circle","Triangle","Octagon"};
                char nm[32];
                snprintf(nm, sizeof(nm), "%s %d", names[idx], (int)m_surfaces.size() + 1);
                int matIdx = m_materials.empty() ? -1 : 0;
                int prjIdx = m_projectors.empty() ? -1 : 0;
                addSurface(nm, matIdx, prjIdx);
            });
}

void StageView::renderUI(const std::vector<GLuint>& zoneTextures) {
    ImDrawList* draw = ImGui::GetWindowDrawList();
    float panelW = ImGui::GetContentRegionAvail().x;
    float panelH = ImGui::GetContentRegionAvail().y;

    // Empty state only if BOTH materials and displays are empty.
    // A default display is added during init() so the 3D view shows a plane
    // with the live canvas content out of the box.
    if (m_materials.empty() && m_displays.empty()) {
        ImGui::Dummy(ImVec2(0, panelH * 0.3f));
        float btnW = 200.0f;
        ImGui::SetCursorPosX((panelW - btnW) * 0.5f);
        ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(1.0f, 1.0f, 1.0f, 0.10f));
        ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.25f));
        ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(1.0f, 1.0f, 1.0f, 1.0f));
        if (ImGui::Button("Import 3D Model", ImVec2(btnW, 40))) {
            // Open file dialog - handled by signal
            m_wantsImport = true;
        }
        ImGui::PopStyleColor(3);

        ImGui::Dummy(ImVec2(0, 10));
        float tw = ImGui::CalcTextSize("OBJ / GLTF / GLB").x;
        ImGui::SetCursorPosX((panelW - tw) * 0.5f);
        ImGui::TextDisabled("OBJ / GLTF / GLB");
        return;
    }

    // Toolbar moved out of renderUI and into renderToolbar() — host pins it
    // above the scrollable viewport child so it stays visible while Setup /
    // Scenes are open below.

    // 3D viewport area fills the full child now that the toolbar is external.
    float viewH = panelH;
    if (viewH < 100) viewH = 100;

    ImVec2 viewStart = ImGui::GetCursorScreenPos();
    ImVec2 viewSize(panelW, viewH);

    // SSAA: render FBO at 3× viewport size so the GL_LINEAR filter on the
    // display texture downsamples — cheap anti-aliasing on the 3D edges,
    // no MSAA FBO plumbing needed. 3× → 9 samples per final pixel.
    const int ssaa = 3;
    int needW = (int)viewSize.x * ssaa, needH = (int)viewSize.y * ssaa;
    if (needW < 1) needW = 1;
    if (needH < 1) needH = 1;
    if (needW != m_fboWidth || needH != m_fboHeight) {
        m_fboWidth = needW;
        m_fboHeight = needH;
        m_fbo.create(m_fboWidth, m_fboHeight, true);
    }

    // Snapshot the on-screen viewport rect for renderFrustum's overlay
    // mapping — must be set BEFORE renderScene since renderFrustum is
    // invoked from inside the FBO draw pass.
    m_viewportScreenPos  = {viewStart.x, viewStart.y};
    m_viewportScreenSize = {viewSize.x,  viewSize.y};

    // Render 3D scene to FBO
    m_fbo.bind();
    glViewport(0, 0, m_fboWidth, m_fboHeight);
    glClearColor(0.04f, 0.045f, 0.06f, 1.0f);
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    float aspect = viewSize.x / std::max(1.0f, viewSize.y);
    renderScene(zoneTextures, aspect);

    Framebuffer::unbind();

    // Display FBO as ImGui image
    ImGui::Image((ImTextureID)(intptr_t)m_fbo.textureId(), ImVec2(viewSize.x, viewSize.y),
                 ImVec2(0, 1), ImVec2(1, 0));
    // Capture the viewport-image hover state NOW, before subsequent toolbar buttons
    // overwrite ImGui's "last item" — without this, zoom/pan/orbit only register
    // when the mouse happens to be over the "Scale" toolbar button.
    bool viewportHovered = ImGui::IsItemHovered();

    // (Move / Rotate / Scale buttons moved to the Properties panel —
    // see PropertyPanel::render Stage section. Keyboard shortcuts
    // V/R/S still toggle m_gizmoOp from anywhere in the app.)
    if (!ImGui::GetIO().WantTextInput) {
        if (ImGui::IsKeyPressed(ImGuiKey_V)) m_gizmoOp = 0;
        if (ImGui::IsKeyPressed(ImGuiKey_R)) m_gizmoOp = 1;
        if (ImGui::IsKeyPressed(ImGuiKey_S)) m_gizmoOp = 2;
    }

    // --- 3D Gizmo overlay ---
    {
        glm::mat4 view = m_camera.viewMatrix();
        glm::mat4 proj = m_camera.projMatrix(aspect);

        ImGuizmo::SetDrawlist();
        ImGuizmo::SetRect(viewStart.x, viewStart.y, viewSize.x, viewSize.y);

        // Determine gizmo operation
        ImGuizmo::OPERATION op = ImGuizmo::TRANSLATE;
        if (m_gizmoOp == 1) op = ImGuizmo::ROTATE;
        else if (m_gizmoOp == 2) op = ImGuizmo::SCALE;

        // Apply gizmo to selected Display (skipped when pinned so the
        // user can lock a screen and click around it without nudging it).
        if (m_selectedDisplay >= 0 && m_selectedDisplay < (int)m_displays.size()
            && !m_displays[m_selectedDisplay].pinned) {
            auto& d = m_displays[m_selectedDisplay];
            glm::mat4 model = d.getTransform();
            float* vPtr = glm::value_ptr(view);
            float* pPtr = glm::value_ptr(proj);
            float* mPtr = glm::value_ptr(model);

            ImGuizmo::Manipulate(vPtr, pPtr, op, ImGuizmo::WORLD, mPtr);
            if (ImGuizmo::IsUsing()) {
                // Decompose back to position/rotation/scale
                float matDecompTranslation[3], matDecompRotation[3], matDecompScale[3];
                ImGuizmo::DecomposeMatrixToComponents(mPtr, matDecompTranslation, matDecompRotation, matDecompScale);
                d.position = {matDecompTranslation[0], matDecompTranslation[1], matDecompTranslation[2]};
                d.rotation = {matDecompRotation[0], matDecompRotation[1], matDecompRotation[2]};
                if (op == ImGuizmo::SCALE) {
                    d.width = std::abs(matDecompScale[0]) * 2.0f;
                    d.height = std::abs(matDecompScale[1]) * 2.0f;
                }
                m_orbitDragging = false;
            }
        }
        // Apply gizmo to selected Projector
        else if (m_selectedProjector >= 0 && m_selectedProjector < (int)m_projectors.size()) {
            auto& p = m_projectors[m_selectedProjector];
            glm::mat4 model = glm::translate(glm::mat4(1.0f), p.position);
            float* vPtr = glm::value_ptr(view);
            float* pPtr = glm::value_ptr(proj);
            float* mPtr = glm::value_ptr(model);

            ImGuizmo::Manipulate(vPtr, pPtr, ImGuizmo::TRANSLATE, ImGuizmo::WORLD, mPtr);
            if (ImGuizmo::IsUsing()) {
                float t[3], r[3], s[3];
                ImGuizmo::DecomposeMatrixToComponents(mPtr, t, r, s);
                p.position = {t[0], t[1], t[2]};
                m_orbitDragging = false;
            }
        }
    }

    // --- Viewport interaction (UE5/Spline hybrid) ---
    // Right-drag = orbit, Middle-drag/Space+Left = pan, Scroll = distance-proportional zoom
    // Left-click = select object, Left-drag on gizmo = manipulate
    bool hovered = viewportHovered && !ImGuizmo::IsOver();
    bool gizmoUsing = ImGuizmo::IsUsing();
    ImVec2 mouse = ImGui::GetMousePos();
    float dt = ImGui::GetIO().DeltaTime;

    // --- Smooth camera animation (F to frame) ---
    if (m_cameraAnimating) {
        m_cameraAnimTime += dt;
        float t = std::min(1.0f, m_cameraAnimTime / m_cameraAnimDuration);
        // Ease-out cubic: 1 - (1-t)^3
        float ease = 1.0f - (1.0f - t) * (1.0f - t) * (1.0f - t);
        m_camera.target = m_cameraAnimStartTarget + (m_cameraAnimEndTarget - m_cameraAnimStartTarget) * ease;
        m_camera.distance = m_cameraAnimStartDist + (m_cameraAnimEndDist - m_cameraAnimStartDist) * ease;
        if (t >= 1.0f) m_cameraAnimating = false;
    }

    // Right-drag: Orbit camera (UE5 style - direct 1:1 mapping, no inertia)
    if (hovered && ImGui::IsMouseClicked(ImGuiMouseButton_Right)) {
        m_orbitDragging = true;
        m_orbitDragStart = {mouse.x, mouse.y};
        m_orbitStartAzimuth = m_camera.azimuth;
        m_orbitStartElevation = m_camera.elevation;
        m_cameraAnimating = false; // cancel any animation
    }
    if (m_orbitDragging && ImGui::IsMouseDown(ImGuiMouseButton_Right)) {
        float dx = mouse.x - m_orbitDragStart.x;
        float dy = mouse.y - m_orbitDragStart.y;
        m_camera.azimuth = m_orbitStartAzimuth - dx * 0.005f;
        m_camera.elevation = std::max(-1.5f, std::min(1.5f, m_orbitStartElevation + dy * 0.005f));
    }
    if (ImGui::IsMouseReleased(ImGuiMouseButton_Right)) m_orbitDragging = false;

    // Middle-drag OR Space+Left-drag: Pan camera
    // Pan speed proportional to distance (feels natural at any zoom level)
    bool spaceHeld = ImGui::IsKeyDown(ImGuiKey_Space);
    bool panStart = (hovered && ImGui::IsMouseClicked(ImGuiMouseButton_Middle)) ||
                    (hovered && spaceHeld && ImGui::IsMouseClicked(ImGuiMouseButton_Left));
    bool panHold = (m_panDragging && ImGui::IsMouseDown(ImGuiMouseButton_Middle)) ||
                   (m_panDragging && spaceHeld && ImGui::IsMouseDown(ImGuiMouseButton_Left));
    if (panStart) {
        m_panDragging = true;
        m_panDragStart = {mouse.x, mouse.y};
        m_panStartTarget = m_camera.target;
        m_cameraAnimating = false;
    }
    if (m_panDragging && panHold) {
        // Pan speed scales with distance (infinite canvas feel)
        float panScale = m_camera.distance * 0.002f;
        float dx = (mouse.x - m_panDragStart.x) * panScale;
        float dy = (mouse.y - m_panDragStart.y) * panScale;
        // Pan in camera-local right/up axes
        float ca = cosf(m_camera.azimuth), sa = sinf(m_camera.azimuth);
        glm::vec3 right = {ca, 0.0f, sa};
        glm::vec3 up = {0.0f, 1.0f, 0.0f};
        m_camera.target = m_panStartTarget - right * dx + up * dy;
    }
    if (ImGui::IsMouseReleased(ImGuiMouseButton_Middle) ||
        (m_panDragging && !spaceHeld && !ImGui::IsMouseDown(ImGuiMouseButton_Middle))) {
        m_panDragging = false;
    }

    // Scroll: Distance-proportional zoom (THE key to infinite canvas feel)
    // Zoom speed = 10% of current distance per scroll notch
    if (hovered) {
        float scroll = ImGui::GetIO().MouseWheel;
        if (scroll != 0.0f) {
            float zoomFactor = 1.0f - scroll * 0.1f; // 10% per notch
            float newDist = m_camera.distance * zoomFactor;
            // Soft clamp: no hard limits, just very gentle resistance at extremes
            newDist = std::max(0.01f, std::min(1000.0f, newDist));
            m_camera.distance = newDist;
            m_cameraAnimating = false;
        }
    }

    // ── Spline-style left interaction ──
    // Press in empty viewport space starts a "potential drag". If the
    // cursor moves past kClickDragPx the press promotes to a left-drag
    // orbit (mirroring right-drag). If the press releases without
    // moving, we run the select/deselect raycast — so a selected
    // object only ever gets manipulated via its gizmo handles, never
    // by a stray drag on empty space.
    const float kClickDragPx = 4.0f;

    // Cancel any in-flight orbit if the user starts using the gizmo.
    if (gizmoUsing) {
        m_leftPressActive = false;
        m_leftPressOrbiting = false;
    }

    if (hovered && ImGui::IsMouseClicked(ImGuiMouseButton_Left) && !gizmoUsing && !spaceHeld) {
        // Hit test displays / projectors on PRESS, not on release. If the
        // press lands on a display, just switch selection — never start
        // an orbit. That kills the bug where clicking the rendered plane
        // both moved the camera AND let the gizmo distort the surface.
        glm::mat4 vp = m_camera.projMatrix(aspect) * m_camera.viewMatrix();
        float bestDist = 40.0f;
        int  bestIdx   = -1;
        for (int i = 0; i < (int)m_displays.size(); i++) {
            if (!m_displays[i].visible) continue;
            glm::vec4 clip = vp * glm::vec4(m_displays[i].position, 1.0f);
            if (clip.w <= 0) continue;
            float sx = viewStart.x + (clip.x / clip.w * 0.5f + 0.5f) * viewSize.x;
            float sy = viewStart.y + (1.0f - (clip.y / clip.w * 0.5f + 0.5f)) * viewSize.y;
            float d  = sqrtf((mouse.x-sx)*(mouse.x-sx) + (mouse.y-sy)*(mouse.y-sy));
            if (d < bestDist) { bestDist = d; bestIdx = i; }
        }
        for (int i = 0; i < (int)m_projectors.size(); i++) {
            if (!m_projectors[i].visible) continue;
            glm::vec4 clip = vp * glm::vec4(m_projectors[i].position, 1.0f);
            if (clip.w <= 0) continue;
            float sx = viewStart.x + (clip.x / clip.w * 0.5f + 0.5f) * viewSize.x;
            float sy = viewStart.y + (1.0f - (clip.y / clip.w * 0.5f + 0.5f)) * viewSize.y;
            float d  = sqrtf((mouse.x-sx)*(mouse.x-sx) + (mouse.y-sy)*(mouse.y-sy));
            if (d < bestDist) { bestDist = d; bestIdx = -(i + 100); }
        }
        if (bestIdx >= 0) {
            m_selectedDisplay = bestIdx; m_selectedProjector = -1;
            m_leftPressActive = false;     // suppress orbit promotion
        } else if (bestIdx <= -100) {
            m_selectedProjector = -(bestIdx + 100); m_selectedDisplay = -1;
            m_leftPressActive = false;
        } else {
            // Empty space — eligible for orbit-on-drag and deselect-on-click.
            m_leftPressActive   = true;
            m_leftPressOrbiting = false;
            m_leftPressStart    = {mouse.x, mouse.y};
        }
    }

    // While the left button is held with a press in flight: promote to
    // orbit drag once we cross the threshold.
    if (m_leftPressActive && ImGui::IsMouseDown(ImGuiMouseButton_Left)) {
        float dx = mouse.x - m_leftPressStart.x;
        float dy = mouse.y - m_leftPressStart.y;
        if (!m_leftPressOrbiting && (dx * dx + dy * dy) > kClickDragPx * kClickDragPx) {
            m_leftPressOrbiting   = true;
            m_orbitDragging       = true;
            m_orbitDragStart      = m_leftPressStart;
            m_orbitStartAzimuth   = m_camera.azimuth;
            m_orbitStartElevation = m_camera.elevation;
            m_cameraAnimating     = false;
        }
        if (m_leftPressOrbiting) {
            float ox = mouse.x - m_orbitDragStart.x;
            float oy = mouse.y - m_orbitDragStart.y;
            m_camera.azimuth   = m_orbitStartAzimuth   - ox * 0.005f;
            m_camera.elevation = std::max(-1.5f, std::min(1.5f,
                                  m_orbitStartElevation + oy * 0.005f));
        }
    }

    // Release in empty space without a drag → deselect.
    if (m_leftPressActive && ImGui::IsMouseReleased(ImGuiMouseButton_Left)) {
        if (!m_leftPressOrbiting) {
            m_selectedDisplay = -1;
            m_selectedProjector = -1;
        }
        m_leftPressActive   = false;
        m_leftPressOrbiting = false;
        m_orbitDragging     = false;
    }

    // F: Frame selected (smooth animated fly-to)
    if (hovered && !ImGui::GetIO().WantTextInput && ImGui::IsKeyPressed(ImGuiKey_F)) {
        glm::vec3 targetPos = {0, 0, 0};
        float targetDist = 5.0f;
        bool found = false;
        if (m_selectedDisplay >= 0 && m_selectedDisplay < (int)m_displays.size()) {
            targetPos = m_displays[m_selectedDisplay].position;
            targetDist = std::max(m_displays[m_selectedDisplay].width, m_displays[m_selectedDisplay].height) * 2.5f;
            found = true;
        } else if (m_selectedProjector >= 0 && m_selectedProjector < (int)m_projectors.size()) {
            targetPos = m_projectors[m_selectedProjector].position;
            targetDist = 5.0f;
            found = true;
        }
        if (found) {
            m_cameraAnimating = true;
            m_cameraAnimTime = 0.0f;
            m_cameraAnimStartTarget = m_camera.target;
            m_cameraAnimEndTarget = targetPos;
            m_cameraAnimStartDist = m_camera.distance;
            m_cameraAnimEndDist = targetDist;
        }
    }

    // Esc: Deselect any active display/projector. Without a selection
    // ImGuizmo's translate/rotate/scale gizmo doesn't render, which
    // means left-drag in the viewport falls through to camera pan
    // instead of accidentally translating the previously-selected
    // display when the user just wants to navigate.
    if (!ImGui::GetIO().WantTextInput && ImGui::IsKeyPressed(ImGuiKey_Escape)) {
        m_selectedDisplay   = -1;
        m_selectedProjector = -1;
    }

    // Delete/Backspace: Remove selected object
    if (!ImGui::GetIO().WantTextInput && (ImGui::IsKeyPressed(ImGuiKey_Delete) || ImGui::IsKeyPressed(ImGuiKey_Backspace))) {
        if (m_selectedDisplay >= 0 && m_selectedDisplay < (int)m_displays.size()) {
            removeDisplay(m_selectedDisplay);
        } else if (m_selectedProjector >= 0 && m_selectedProjector < (int)m_projectors.size()) {
            removeProjector(m_selectedProjector);
            m_selectedProjector = -1;
        }
    }

    // Cmd/Ctrl+D: duplicate the selected display, offset slightly so it's
    // visible as its own object. Selection follows the new copy.
    {
        ImGuiIO& io = ImGui::GetIO();
        bool modDown = io.KeySuper || io.KeyCtrl;
        if (!io.WantTextInput && modDown && ImGui::IsKeyPressed(ImGuiKey_D)
            && m_selectedDisplay >= 0 && m_selectedDisplay < (int)m_displays.size()) {
            StageDisplay copy = m_displays[m_selectedDisplay];
            copy.position.x += 0.30f;
            copy.name      += " copy";
            m_displays.push_back(copy);
            m_selectedDisplay = (int)m_displays.size() - 1;
        }
    }
}

// Stage inspector — Displays only. Projector/Surface/Cluster machinery
// remains in the data model for future use but is hidden from the UI
// until those features are wired into the renderer.
void StageView::renderSceneInspector(const std::vector<GLuint>& zoneTextures) {
    // Section header is drawn by the caller as "Setup". This panel is
    // the per-display inspector + add/delete row.

    int dispRemove = -1;
    auto labelLeft = [](const char* lbl) {
        // Left label, vertically centered with the control on its right.
        // Uses a fixed 26% of the row for the label so dropdowns and
        // numeric fields land on the same gridline.
        float rowW = ImGui::GetContentRegionAvail().x;
        float lblW = std::max(64.0f, rowW * 0.26f);
        ImGui::AlignTextToFramePadding();
        ImGui::TextUnformatted(lbl);
        ImGui::SameLine(lblW);
        ImGui::SetNextItemWidth(rowW - lblW);
    };

    for (int i = 0; i < (int)m_displays.size(); i++) {
        ImGui::PushID(53000 + i);
        auto& d = m_displays[i];
        bool sel = (i == m_selectedDisplay);

        ImGui::PushStyleColor(ImGuiCol_Header, ImVec4(1.0f, 1.0f, 1.0f, sel ? 0.15f : 0.0f));
        char dlabel[128];
        snprintf(dlabel, sizeof(dlabel), "%s  [%s]", d.name.c_str(), StageDisplay::typeName(d.type));
        if (ImGui::Selectable(dlabel, sel)) {
            m_selectedDisplay = i;
        }
        ImGui::PopStyleColor();

        if (sel) {
            static const char* typeNames[] = { "Projector", "LED Screen", "TV", "Monitor", "Custom" };
            int typeIdx = (int)d.type;
            labelLeft("Type");
            if (ImGui::Combo("##type", &typeIdx, typeNames, 5)) {
                d.type = (StageDisplay::Type)typeIdx;
            }

            // Zone
            labelLeft("Zone");
            char zoneLabel[32];
            snprintf(zoneLabel, sizeof(zoneLabel), "Zone %d", d.zoneIndex);
            if (ImGui::BeginCombo("##zone", zoneLabel)) {
                for (int zi = 0; zi < std::max(1, (int)zoneTextures.size()); zi++) {
                    char zl[32]; snprintf(zl, sizeof(zl), "Zone %d", zi);
                    if (ImGui::Selectable(zl, d.zoneIndex == zi)) d.zoneIndex = zi;
                }
                ImGui::EndCombo();
            }

            labelLeft("Position");
            ImGui::DragFloat3("##pos", &d.position[0], 0.05f, -50.0f, 50.0f, "%.2f");
            labelLeft("Rotation");
            ImGui::DragFloat3("##rot", &d.rotation[0], 0.5f, -180.0f, 180.0f, "%.1f");
            labelLeft("Size (m)");
            float sz[2] = { d.width, d.height };
            if (ImGui::DragFloat2("##size", sz, 0.01f, 0.1f, 20.0f, "%.2f")) {
                d.width = sz[0]; d.height = sz[1];
            }

            ImGui::Dummy(ImVec2(0, 4));
            // Row of three equal-height pills: Visible / Pinned / Delete.
            float btnH = ImGui::GetFrameHeight();
            float rowW = ImGui::GetContentRegionAvail().x;
            float spacing = ImGui::GetStyle().ItemSpacing.x;
            float btnW = (rowW - spacing * 2) / 3.0f;
            auto pill = [&](const char* label, bool on, ImU32 fill, ImU32 fillHov, ImU32 textCol) -> bool {
                ImVec2 pos = ImGui::GetCursorScreenPos();
                bool clicked = ImGui::InvisibleButton(label, ImVec2(btnW, btnH));
                bool hov = ImGui::IsItemHovered();
                ImDrawList* dl = ImGui::GetWindowDrawList();
                ImU32 bg = on ? IM_COL32(255,255,255,240)
                              : (hov ? fillHov : fill);
                ImU32 tx = on ? IM_COL32(13,18,26,255) : textCol;
                dl->AddRectFilled(pos, ImVec2(pos.x + btnW, pos.y + btnH), bg, btnH * 0.5f);
                ImVec2 ts = ImGui::CalcTextSize(label);
                dl->AddText(ImVec2(pos.x + (btnW - ts.x) * 0.5f,
                                   pos.y + (btnH - ts.y) * 0.5f),
                            tx, label);
                return clicked;
            };
            if (pill("Visible", d.visible,
                     IM_COL32(255,255,255,18), IM_COL32(255,255,255,32),
                     IM_COL32(220,226,235,230))) d.visible = !d.visible;
            ImGui::SameLine();
            if (pill("Pinned", d.pinned,
                     IM_COL32(255,255,255,18), IM_COL32(255,255,255,32),
                     IM_COL32(220,226,235,230))) d.pinned = !d.pinned;
            ImGui::SameLine();
            if (pill("Delete", false,
                     IM_COL32(255,255,255,12), IM_COL32(255,255,255,28),
                     IM_COL32(220,226,235,200))) dispRemove = i;
        }
        ImGui::PopID();
    }
    if (dispRemove >= 0) removeDisplay(dispRemove);

    ImGui::Dummy(ImVec2(0, 4));

    // Inspector ends here. Projectors / Surfaces / Screen Clusters
    // are kept as data structures but the UI is removed until they're
    // wired into the renderer (currently they're a stub).
    return;

    // ── unreachable legacy projector inspector (kept compiled out) ──
    int projRemove = -1;
    for (int i = 0; i < (int)m_projectors.size(); i++) {
        ImGui::PushID(50000 + i);
        auto& p = m_projectors[i];
        bool sel = (i == m_selectedProjector);

        ImGui::PushStyleColor(ImGuiCol_Header, ImVec4(1.0f, 1.0f, 1.0f, sel ? 0.15f : 0.0f));
        if (ImGui::Selectable(p.name.c_str(), sel)) {
            m_selectedProjector = i;
        }
        ImGui::PopStyleColor();

        if (sel) {
            ImGui::DragFloat3("Pos", &p.position.x, 0.1f);
            ImGui::DragFloat3("Target", &p.target.x, 0.1f);
            ImGui::DragFloat("FOV", &p.fovDeg, 0.5f, 10.0f, 170.0f);
            ImGui::SameLine();
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.6f, 0.3f, 0.3f, 0.7f));
            if (ImGui::SmallButton("Del")) projRemove = i;
            ImGui::PopStyleColor();
        }
        ImGui::PopID();
    }
    if (projRemove >= 0) removeProjector(projRemove);

    // --- Surface list ---
    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.5f, 0.55f, 0.65f, 1.0f));
    ImGui::Text("Surfaces");
    ImGui::PopStyleColor();

    int surfRemove = -1;
    for (int i = 0; i < (int)m_surfaces.size(); i++) {
        ImGui::PushID(51000 + i);
        auto& s = m_surfaces[i];
        bool sel = (i == m_selectedSurface);

        ImGui::PushStyleColor(ImGuiCol_Header, ImVec4(1.0f, 1.0f, 1.0f, sel ? 0.15f : 0.0f));
        if (ImGui::Selectable(s.name.c_str(), sel)) {
            m_selectedSurface = i;
        }
        ImGui::PopStyleColor();

        if (sel) {
            // Material selector — guard both bounds: -1 (unassigned) would index out.
            const char* matLabel = (s.materialIndex >= 0 && s.materialIndex < (int)m_materials.size())
                ? m_materials[s.materialIndex].name.c_str() : "None";
            if (ImGui::BeginCombo("Material", matLabel)) {
                for (int m = 0; m < (int)m_materials.size(); m++) {
                    if (ImGui::Selectable(m_materials[m].name.c_str(), s.materialIndex == m)) {
                        s.materialIndex = m;
                    }
                }
                ImGui::EndCombo();
            }
            // Projector selector — guard both bounds: addSurface sets projectorIndex = -1
            // when no projectors exist; the old `< size` check allowed -1 through.
            const char* projLabel = (s.projectorIndex >= 0 && s.projectorIndex < (int)m_projectors.size())
                ? m_projectors[s.projectorIndex].name.c_str() : "None";
            if (ImGui::BeginCombo("Projector", projLabel)) {
                for (int p = 0; p < (int)m_projectors.size(); p++) {
                    if (ImGui::Selectable(m_projectors[p].name.c_str(), s.projectorIndex == p)) {
                        s.projectorIndex = p;
                    }
                }
                ImGui::EndCombo();
            }
            ImGui::SameLine();
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.6f, 0.3f, 0.3f, 0.7f));
            if (ImGui::SmallButton("Del")) surfRemove = i;
            ImGui::PopStyleColor();
        }
        ImGui::PopID();
    }
    if (surfRemove >= 0) removeSurface(surfRemove);

    ImGui::Dummy(ImVec2(0, 6));

    // --- Screen Clusters ---
    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.5f, 0.55f, 0.65f, 1.0f));
    ImGui::Text("Screen Clusters");
    ImGui::PopStyleColor();

    ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(1.0f, 1.0f, 1.0f, 0.15f));
    ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.30f));
    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(1.0f, 1.0f, 1.0f, 1.0f));
    if (ImGui::SmallButton("+Cluster")) {
        int idx = addCluster("Cluster " + std::to_string(m_clusters.size() + 1));
        m_selectedCluster = idx;
    }
    ImGui::PopStyleColor(3);

    int clRemove = -1;
    for (int ci = 0; ci < (int)m_clusters.size(); ci++) {
        ImGui::PushID(52000 + ci);
        auto& cl = m_clusters[ci];
        bool sel = (ci == m_selectedCluster);

        ImGui::PushStyleColor(ImGuiCol_Header, ImVec4(1.0f, 1.0f, 1.0f, sel ? 0.15f : 0.0f));
        if (ImGui::Selectable(cl.name.c_str(), sel)) {
            m_selectedCluster = ci;
        }
        ImGui::PopStyleColor();

        if (sel) {
            // Position
            ImGui::SetNextItemWidth(-1);
            ImGui::DragFloat3("Pos##cl", &cl.position[0], 0.05f, -50.0f, 50.0f, "%.2f");
            // Rotation
            ImGui::SetNextItemWidth(-1);
            ImGui::DragFloat3("Rot##cl", &cl.rotation[0], 0.5f, -180.0f, 180.0f, "%.1f");
            // Scale
            ImGui::SetNextItemWidth(-1);
            ImGui::DragFloat("Scale##cl", &cl.scale, 0.01f, 0.01f, 10.0f, "%.2f");

            ImGui::Dummy(ImVec2(0, 3));
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.5f, 0.55f, 0.65f, 1.0f));
            ImGui::Text("Grid");
            ImGui::PopStyleColor();

            // Grid layout
            int prevCols = cl.gridCols, prevRows = cl.gridRows;
            ImGui::SetNextItemWidth(60);
            ImGui::InputInt("Cols##cl", &cl.gridCols, 1, 1);
            ImGui::SameLine();
            ImGui::SetNextItemWidth(60);
            ImGui::InputInt("Rows##cl", &cl.gridRows, 1, 1);
            cl.gridCols = std::max(1, std::min(8, cl.gridCols));
            cl.gridRows = std::max(1, std::min(8, cl.gridRows));

            // Screen dimensions
            ImGui::SetNextItemWidth(60);
            ImGui::DragFloat("W##clsw", &cl.screenW, 0.01f, 0.1f, 20.0f, "%.2f");
            ImGui::SameLine();
            ImGui::SetNextItemWidth(60);
            ImGui::DragFloat("H##clsh", &cl.screenH, 0.01f, 0.1f, 20.0f, "%.2f");

            // Gap
            ImGui::SetNextItemWidth(60);
            ImGui::DragFloat("Gap X##clg", &cl.gapX, 0.005f, 0.0f, 1.0f, "%.3f");
            ImGui::SameLine();
            ImGui::SetNextItemWidth(60);
            ImGui::DragFloat("Gap Y##clg", &cl.gapY, 0.005f, 0.0f, 1.0f, "%.3f");

            // Populate grid button
            ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(1.0f, 1.0f, 1.0f, 0.15f));
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.30f));
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(1.0f, 1.0f, 1.0f, 1.0f));
            char gridLabel[64];
            snprintf(gridLabel, sizeof(gridLabel), "Generate %dx%d Grid", cl.gridCols, cl.gridRows);
            if (ImGui::Button(gridLabel, ImVec2(-1, 0))) {
                populateClusterGrid(ci, 0);
            }
            ImGui::PopStyleColor(3);

            // Count surfaces in this cluster
            int surfCount = 0;
            for (auto& s : m_surfaces) if (s.clusterIndex == ci) surfCount++;
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 0.7f));
            ImGui::Text("%d screens", surfCount);
            ImGui::PopStyleColor();

            ImGui::SameLine();
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.6f, 0.3f, 0.3f, 0.7f));
            if (ImGui::SmallButton("Del##cl")) clRemove = ci;
            ImGui::PopStyleColor();
        }
        ImGui::PopID();
    }
    if (clRemove >= 0) removeCluster(clRemove);
}
