#pragma once

#include <functional>
#include <imgui.h>

struct GLFWwindow;
struct ImFont;

// Awesome-design tokens — single named source of truth used by rails,
// pills, panels, and selection overlays. Anyone touching chrome surface
// or active-state styling should reference these so the whole UI stays
// in lockstep.
namespace UITokens {
    static constexpr ImU32 kChromeSurface = IM_COL32(11, 13, 17, 255);
    static constexpr ImU32 kHairline      = IM_COL32(255, 255, 255, 30);
    static constexpr ImU32 kHairlineSoft  = IM_COL32(255, 255, 255, 22);
    static constexpr ImU32 kButtonHover   = IM_COL32(255, 255, 255, 22);
    static constexpr ImU32 kButtonActive  = IM_COL32(255, 255, 255, 36);
    // Single chromatic anchor — reference's cyan/blue accent. Used only
    // for active states (selection handles, ring on the play button,
    // active rail icon, active properties tab). Never as a plain fill
    // color outside of the play-button glow.
    static constexpr ImU32 kAccent        = IM_COL32(74, 140, 255, 255);
    static constexpr ImU32 kAccentSoft    = IM_COL32(74, 140, 255, 100);
    static constexpr ImU32 kAccentGlow    = IM_COL32(74, 140, 255, 60);
}
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

    // Current main-viewport workspace. Four modes — only one is rendered
    // per frame, the pill switcher in the secondary nav flips this enum:
    //   Canvas  → 2D compositor + layers/properties/sources/timeline
    //   Mapping → 2D output + warp/mask params (projection calibration)
    //   Stage   → 3D stage view (projector preview)
    //   Show    → live-performance focus: timeline + MIDI + audio
    // NOTE: order matters — the nav switcher lists them left→right as
    // Canvas / Mapping / Stage / Play, so Mapping sits between Canvas and Stage.
    enum class WorkspaceMode { Canvas, Mapping, Stage, Show };
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

    // Application reports the canvas image's screen-space vertical bounds
    // each frame (after ViewportPanel renders). setupDockspace's NEXT
    // frame uses these to align the right Control Panel float top/bottom
    // with the canvas top/bottom — equal margins, vertically centered.
    void setCanvasBoundsY(float topY, float bottomY) {
        m_canvasTopY    = topY;
        m_canvasBottomY = bottomY;
    }

    // Application reports the live (animated) timeline top edge in screen
    // space each frame, BEFORE setupDockspace/renderLeftRail. The right
    // params float and the left-rail thumbnail column both clamp their
    // bottom to this Y so the timeline never covers them (Fix 1 + Fix 3).
    // 0 = not set this frame (treat as "no timeline").
    void setTimelineTopY(float y) { m_timelineTopY = y; }

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
    const char* m_dockSelectQueue = nullptr; // window to set as dock SelectedTabId at end of frame
    // Sticky visible-window for the right dock. Updated by pill clicks
    // (focusPanel / focusSourcesTab) and re-applied to dockNode->VisibleWindow
    // every frame so ImGui's own dock-update can't override our choice.
    const char* m_currentRightPanel = "        ###Properties";
    int m_pendingFocusFramesLeft = 0;
    // Seed Begin/End calls in the desired tab order on the next rebuild frame.
    // ImGui locks tab order to the FIRST Begin() call per docked window after
    // a layout rebuild, so we run no-op Begin/End in our intended order before
    // any panel renders. Without this, Application.cpp's Begin call order
    // (Layers→Mapping→Properties→Sources) wins and breaks Layers→Sources→
    // Mapping in the Control Panel header.
    bool m_seedRightDockTabs = false;
    // Canvas image vertical bounds in screen coords (set by Application
    // after each ViewportPanel render). Used by setupDockspace next frame
    // to align the right float panel with the canvas top/bottom.
    float m_canvasTopY    = 0.0f;
    float m_canvasBottomY = 0.0f;
    // Live animated timeline top edge (screen Y). Set every frame by
    // Application::renderUI via setTimelineTopY before setupDockspace.
    float m_timelineTopY  = 0.0f;

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

    // Source-tab quick switcher — exposed so the Properties panel can pin
    // the 4-icon strip (Shader / Mic / Cam / Win) at its top instead of it
    // only living on the Sources panel. focusSourcesTab() programmatically
    // activates the matching tab in the Sources dock; activeSourcesTab()
    // returns which tab is currently selected so callers can highlight.
    enum class SourceTab { None = 0, Shader, Mic, Cam, Win };
    void      focusSourcesTab(SourceTab t);
    SourceTab activeSourcesTab() const;

    // Programmatic tab/window focus — sets a pending-focus string that the
    // next few frames will apply via SetWindowFocus, the same mechanism the
    // left rail uses. Pass the full ImGui window name including any padding
    // (e.g. "        ###Properties" / "        ###Mapping").
    void focusPanel(const char* windowName);

    // Right-dock pinned nav bar (Properties / Shader / Mic / Cam / Win /
    // Mapping). Render at the TOP of each right-dock panel's ImGui::Begin
    // block — `active` controls which pill highlights. With this bar
    // present in every panel we hide the outer ImGui tab bar.
    enum class QuickNavTab { None = 0, Properties, Shader, Mic, Music, Cam, Win, Mapping };
    void renderRightDockNavBar(QuickNavTab active);

    // Singleton accessor — set during init() so panels without a UIManager
    // member (LayerPanel, WarpEditor) can call renderRightDockNavBar.
    static UIManager* current() { return s_instance; }
private:
    static UIManager* s_instance;
public:

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

    // Right transform-tool rail (Phase 3) — pinned to the right edge of
    // the viewport. 5 tool buttons (Move / Rotate / Scale / Flip / Center)
    // plus a vertical zoom slider. Properties dock host shrinks by this
    // width so the rail and Properties don't overlap.
    static constexpr float kRightToolRailW = 56.0f;
    enum class RightTool { Move = 0, Rotate, Scale, Flip, Center };
    // The rail itself is purely chrome — Application owns layer state, so
    // it provides callbacks that the rail invokes on click. zoomGet/Set
    // bind the vertical slider to whichever zoom owner the host wants
    // (typically ViewportPanel::zoom / setZoom).
    void renderRightToolRail(
        const std::function<void(RightTool)>& onTool,
        const std::function<float()>& zoomGet,
        const std::function<void(float)>& zoomSet);

private:
    // Reference B vibe: rail thumbnails are the primary layer affordance,
    // so the larger Layers/Sources/Mapping flyout starts CLOSED. Click a
    // rail icon to open the panel; click again to collapse. Avoids the
    // duplication where both the rail thumb AND a wide layers card show
    // the same layer.
    LeftPanel m_activeLeftPanel = LeftPanel::None;
    unsigned int m_tabIconTex[4] = {0, 0, 0, 0};
    int          m_tabIconW[4]   = {0, 0, 0, 0};
    int          m_tabIconH[4]   = {0, 0, 0, 0};
};
