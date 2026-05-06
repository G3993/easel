#pragma once

#include <functional>

struct GLFWwindow;
struct ImFont;
// Mirrors ImGui's `typedef unsigned int ImGuiID;` so we can store dock node
// ids here without forcing imgui.h into every translation unit that includes
// this header. Kept consistent with imgui.h — duplicate same-underlying-type
// typedefs are legal in C++11.
typedef unsigned int ImGuiID;

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

    // Width of the right floating dock host (Mapping/Properties column),
    // measured in the same coordinate space as ImGui::GetContentRegionAvail.
    // Other panels (e.g. the Stage 3D viewport) read this to reserve space
    // so the right column reads as a fixed anchored rail rather than an
    // overlay covering the central content.
    float rightRailWidth() const { return m_rightFloatW; }

    Workspace workspace() const { return m_workspace; }
    void setWorkspace(Workspace w);

    // Current main-viewport workspace. Three modes — only one is rendered
    // per frame, the pill switcher in the secondary nav flips this enum:
    //   Canvas → 2D compositor + layers/properties/sources/timeline
    //   Stage  → 3D stage view (projector preview)
    //   Show   → live-performance focus: timeline + MIDI + audio
    enum class WorkspaceMode { Canvas, Stage, Show };
    static WorkspaceMode sMode;
    static WorkspaceMode sPrevMode;
    // Glassy 2D→3D handoff: incoming mode fades in over kModeTransitionSec
    // with an ease-out cubic. Set via setMode() — never write sMode directly
    // from new code or you'll skip the animation.
    static double sModeTransitionStart;
    static constexpr double kModeTransitionSec = 0.28;
    static void  setMode(WorkspaceMode m);
    // 0..1 alpha to render `m` at this frame. 1.0 when no transition; ramps
    // 0→1 for the incoming mode during transition; 0 for any other mode.
    static float modeAlpha(WorkspaceMode m);

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

    // Cached dock node ids + sizing used to keep the left/right floating
    // panel groups aligned with the current timeline height every frame.
    // Populated during setupDockspace rebuilds and read in the per-frame
    // reflow block.
    ImGuiID m_timelineDockId = 0;
    ImGuiID m_leftFloatId = 0;
    ImGuiID m_rightFloatId = 0;
    float m_leftFloatW = 0.0f;
    float m_rightFloatW = 0.0f;
    float m_lastTimelineH = 0.0f;

public:
    // Icon textures for the inspector tabs (Properties/Mapping/Audio/MIDI).
    // Loaded from assets/icons/ PNGs at startup.
    enum class TabIcon { Properties = 0, Mapping, Audio, MIDI, COUNT };
    unsigned int tabIconTex(TabIcon w) const;
    // Walks the right-side dock node's tab bar once per frame and paints
    // the matching icon over each inspector tab's title text. Called by
    // Application::renderUI() after all panels have rendered.
    void drawInspectorTabIcons();
    // Walks the Sources panel's internal TabBar and paints procedural
    // icon glyphs over the leading-space pad of each tab label.
    // Tabs: ShaderClaw (3 thunderbolts), Etherea (mic), Camera (lens),
    // Capture (window). NDI/Spout keep plain text.
    void drawSourcesTabIcons();

    // Left activity rail — pinned to the left edge of the viewport.
    // Icons toggle which panel (Layers / Sources / Mapping) is the
    // active flyout. Only one is visible at a time. None means the
    // rail is collapsed and the canvas reaches the rail's right edge.
    enum class LeftPanel { None = 0, Layers, Sources, Mapping };
    LeftPanel activeLeftPanel() const   { return m_activeLeftPanel; }
    void setActiveLeftPanel(LeftPanel p) { m_activeLeftPanel = p; }
    // Rail width — used by the float-host code to shift the flyout
    // position rightward by this much. Constant for now.
    static constexpr float kLeftRailW = 64.0f;
    // Renders the rail itself; called once per frame from Application.
    // The optional `drawExtra` runs inside the rail window after the nav
    // buttons, with `kLeftRailW - 12` of horizontal space available. Use it
    // to draw a hairline divider + per-layer thumbnails (Phase 2) without
    // coupling UIManager to LayerStack.
    void renderLeftRail(const std::function<void(float innerW)>& drawExtra = {});

private:
    LeftPanel m_activeLeftPanel = LeftPanel::Layers;
    unsigned int m_tabIconTex[4] = {0, 0, 0, 0};
    int          m_tabIconW[4]   = {0, 0, 0, 0};
    int          m_tabIconH[4]   = {0, 0, 0, 0};
};
