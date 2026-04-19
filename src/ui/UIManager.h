#pragma once

struct GLFWwindow;
struct ImFont;

// Two top-level modes:
//   Canvas — setup + authoring (mapping, masks, stage 3D, layers, sources,
//            output config). The bulk of the work happens here.
//   Show   — live ops (preview + triggers + monitoring). Minimal chrome.
enum class Workspace { Canvas, Show };

class UIManager {
public:
    bool init(GLFWwindow* window);
    void shutdown();

    void beginFrame();
    void endFrame();

    void setupDockspace(float bottomBarHeight = 0);
    // Renders the primary nav (Stage / Canvas / Show) as a prominent bar
    // directly under the menu bar. Call BEFORE setupDockspace each frame.
    void renderWorkspaceBar();
    float workspaceBarHeight() const { return m_workspaceBarHeight; }

    ImFont* smallFont() const { return m_smallFont; }
    ImFont* boldFont() const { return m_boldFont; }
    ImFont* monoFont() const { return m_monoFont; }

    void handleZoom();  // call each frame to handle Cmd+/- zoom
    float uiScale() const { return m_uiZoom; }

    Workspace workspace() const { return m_workspace; }
    void setWorkspace(Workspace w);

    // True if a panel with this title should render in the current workspace.
    // Call sites that render panels (inline or via class.render()) should
    // wrap with `if (m_ui.isPanelVisible("Title")) { ... }` so hidden panels
    // don't appear as floating windows. Names must match ImGui::Begin() titles.
    bool isPanelVisible(const char* title) const;

private:
    void applyTheme(float dpiScale);

    GLFWwindow* m_window = nullptr;
    bool m_firstFrame = true;
    float m_lastDockW = 0;
    float m_lastDockH = 0;
    ImFont* m_smallFont = nullptr;
    ImFont* m_boldFont = nullptr;
    ImFont* m_monoFont = nullptr;
    float m_uiZoom = 1.0f;
    float m_baseFontGlobalScale = 1.0f;
    Workspace m_workspace = Workspace::Canvas;
    float m_workspaceBarHeight = 0.0f;
    const char* m_pendingFocus = nullptr;  // window name to focus next frame
    int m_pendingFocusFramesLeft = 0;
};
