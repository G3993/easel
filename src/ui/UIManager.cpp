#include "ui/UIManager.h"
#include <algorithm>
#include <cmath>
#include <cstring>
#include <initializer_list>
#include <imgui.h>
#include <imgui_internal.h>
#include <imgui_impl_glfw.h>
#include <imgui_impl_opengl3.h>
#include <glad/glad.h>
#include <GLFW/glfw3.h>
#include "stb_image.h"
#include "ui/ImGuizmo.h"

UIManager::WorkspaceMode UIManager::sMode = UIManager::WorkspaceMode::Canvas;
UIManager::WorkspaceMode UIManager::sPrevMode = UIManager::WorkspaceMode::Canvas;
double UIManager::sModeTransitionStart = -1.0;

void UIManager::setMode(WorkspaceMode m) {
    if (m == sMode) return;
    sPrevMode = sMode;
    sMode = m;
    sModeTransitionStart = glfwGetTime();
}

float UIManager::modeAlpha(WorkspaceMode m) {
    if (sModeTransitionStart < 0.0) return (m == sMode) ? 1.0f : 0.0f;
    double t = (glfwGetTime() - sModeTransitionStart) / kModeTransitionSec;
    if (t >= 1.0) {
        sModeTransitionStart = -1.0;  // transition complete
        return (m == sMode) ? 1.0f : 0.0f;
    }
    if (t < 0.0) t = 0.0;
    // Ease-out cubic — fast start, settles softly
    float u = (float)(1.0 - std::pow(1.0 - t, 3.0));
    if (m == sMode)     return u;        // incoming
    if (m == sPrevMode) return 1.0f - u; // outgoing (rarely actually rendered;
                                         // dock nodes hide it, but kept for
                                         // any standalone overlays)
    return 0.0f;
}

static ImFont* addFirstAvailableFont(ImGuiIO& io, const char* const* paths,
                                     float size, const ImFontConfig* cfg,
                                     const ImWchar* glyphRanges) {
    for (const char* const* path = paths; *path; ++path) {
        if (ImFont* font = io.Fonts->AddFontFromFileTTF(*path, size, cfg, glyphRanges)) {
            return font;
        }
    }
    return nullptr;
}

bool UIManager::init(GLFWwindow* window) {
    m_window = window;

    IMGUI_CHECKVERSION();
    ImGui::CreateContext();

    ImGuiIO& io = ImGui::GetIO();
    io.ConfigFlags |= ImGuiConfigFlags_DockingEnable;
    io.ConfigFlags |= ImGuiConfigFlags_NavEnableKeyboard;
    // Any Drag/Slider widget in the app becomes click-to-type — matches the
    // user expectation that double-clicking a value lets you edit it directly.
    io.ConfigDragClickToInputText = true;

    // Get DPI scale from the monitor
    float xscale = 1.0f, yscale = 1.0f;
    GLFWmonitor* monitor = glfwGetPrimaryMonitor();
    if (monitor) {
        glfwGetMonitorContentScale(monitor, &xscale, &yscale);
    }
    float dpiScale = xscale;
    // On macOS Retina, fonts are loaded at 2x for crispness, then
    // FontGlobalScale is set to 1/dpiScale so they render at logical size.
    // On Windows/Linux, fonts and UI are both scaled by dpiScale.
    float fontScale = dpiScale;   // font texture resolution
#ifdef __APPLE__
    float uiScale = 1.0f;        // widget sizes — already in logical coords on mac
    m_baseFontGlobalScale = 1.0f / dpiScale;
#elif defined(__linux__)
    fontScale = std::max(fontScale, 1.15f);
    float uiScale = std::max(dpiScale, 1.10f);
    m_baseFontGlobalScale = 1.0f;
#else
    float uiScale = dpiScale;
    m_baseFontGlobalScale = 1.0f;
#endif

    // Extended glyph ranges for unicode support (accented chars, etc.)
    static const ImWchar glyphRanges[] = {
        0x0020, 0x00FF, // Basic Latin + Latin Supplement (covers umlauts, accents)
        0x0100, 0x017F, // Latin Extended-A
        0x2000, 0x206F, // General Punctuation
        0x2190, 0x21FF, // Arrows
        0x25A0, 0x25FF, // Geometric Shapes
        0,
    };

    // Font loading — prefer Segoe UI Variable (Windows 11, Inter-like) on
    // Windows with Segoe UI static fallback; Apple system fonts on macOS.
    // Variable font gives the "510 weight" feel Linear's design system leans on.
    const char* primaryFontPath = "C:/Windows/Fonts/SegUIVar.ttf";
    const char* primaryFallback = "C:/Windows/Fonts/segoeui.ttf";
    const char* boldFontPath    = "C:/Windows/Fonts/seguisb.ttf";       // SemiBold
    const char* boldFallback    = "C:/Windows/Fonts/segoeuib.ttf";      // Bold
    const char* monoFontPath    = "C:/Windows/Fonts/CascadiaMono.ttf";
    // Apple's actual UI typeface (SF Pro). Variable file shipped at this
    // path on macOS 11+. Falls back to Helvetica/Arial if missing. SF Pro
    // gives Easel the authentic native-Mac feel that Helvetica.ttc (1957)
    // can't — same family Apple uses across Finder, Xcode, and Sonoma.
    const char* macPrimaryPath  = "/System/Library/Fonts/SFNS.ttf";
    const char* macPrimaryFB    = "/System/Library/Fonts/Helvetica.ttc";
    const char* macBoldPath     = "/System/Library/Fonts/SFNS.ttf";
    const char* macMonoPath     = "/System/Library/Fonts/SFNSMono.ttf";
    const char* linuxPrimaryPaths[] = {
        "/usr/share/fonts/truetype/ubuntu/Ubuntu-R.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
        "/usr/share/fonts/truetype/liberation2/LiberationSans-Regular.ttf",
        "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc",
        nullptr,
    };
    const char* linuxBoldPaths[] = {
        "/usr/share/fonts/truetype/ubuntu/Ubuntu-B.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
        "/usr/share/fonts/truetype/liberation2/LiberationSans-Bold.ttf",
        nullptr,
    };
    const char* linuxMonoPaths[] = {
        "/usr/share/fonts/truetype/ubuntu/UbuntuMono-R.ttf",
        "/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf",
        "/usr/share/fonts/truetype/liberation2/LiberationMono-Regular.ttf",
        nullptr,
    };

    float fontSize = 15.0f * fontScale;  // denser feel; scaled for DPI
    ImFontConfig fontCfg;
    fontCfg.OversampleH = 3;
    fontCfg.OversampleV = 1;
    fontCfg.PixelSnapH = false;
    ImFont* mainFont = nullptr;
#ifdef _WIN32
    mainFont = io.Fonts->AddFontFromFileTTF(primaryFontPath, fontSize, &fontCfg, glyphRanges);
    if (!mainFont) mainFont = io.Fonts->AddFontFromFileTTF(primaryFallback, fontSize, &fontCfg, glyphRanges);
#elif defined(__APPLE__)
    mainFont = io.Fonts->AddFontFromFileTTF(macPrimaryPath, fontSize, &fontCfg, glyphRanges);
    if (!mainFont) mainFont = io.Fonts->AddFontFromFileTTF(macPrimaryFB, fontSize, &fontCfg, glyphRanges);
#elif defined(__linux__)
    mainFont = addFirstAvailableFont(io, linuxPrimaryPaths, fontSize, &fontCfg, glyphRanges);
#endif
    if (!mainFont) {
        ImFontConfig defCfg;
        defCfg.SizePixels = fontSize;
        io.Fonts->AddFontDefault(&defCfg);
    }

    // Small font — for captions, metadata, tertiary labels
    float smallSize = 13.0f * fontScale;
    ImFontConfig smallCfg;
    smallCfg.OversampleH = 3;
    smallCfg.PixelSnapH = false;
    m_smallFont = nullptr;
#ifdef _WIN32
    m_smallFont = io.Fonts->AddFontFromFileTTF(primaryFontPath, smallSize, &smallCfg, glyphRanges);
    if (!m_smallFont) m_smallFont = io.Fonts->AddFontFromFileTTF(primaryFallback, smallSize, &smallCfg, glyphRanges);
#elif defined(__APPLE__)
    m_smallFont = io.Fonts->AddFontFromFileTTF(macPrimaryPath, smallSize, &smallCfg, glyphRanges);
    if (!m_smallFont) m_smallFont = io.Fonts->AddFontFromFileTTF(macPrimaryFB, smallSize, &smallCfg, glyphRanges);
#elif defined(__linux__)
    m_smallFont = addFirstAvailableFont(io, linuxPrimaryPaths, smallSize, &smallCfg, glyphRanges);
#endif
    if (!m_smallFont) {
        ImFontConfig defCfg;
        defCfg.SizePixels = smallSize;
        m_smallFont = io.Fonts->AddFontDefault(&defCfg);
    }

    // Semibold for headers / emphasis (Linear's ~590 weight)
    ImFontConfig boldCfg;
    boldCfg.OversampleH = 3;
    boldCfg.PixelSnapH = false;
    m_boldFont = nullptr;
#ifdef _WIN32
    m_boldFont = io.Fonts->AddFontFromFileTTF(boldFontPath, fontSize, &boldCfg, glyphRanges);
    if (!m_boldFont) m_boldFont = io.Fonts->AddFontFromFileTTF(boldFallback, fontSize, &boldCfg, glyphRanges);
#elif defined(__APPLE__)
    m_boldFont = io.Fonts->AddFontFromFileTTF(macBoldPath, fontSize, &boldCfg, glyphRanges);
#elif defined(__linux__)
    m_boldFont = addFirstAvailableFont(io, linuxBoldPaths, fontSize, &boldCfg, glyphRanges);
#endif
    if (!m_boldFont) m_boldFont = mainFont ? mainFont : io.Fonts->Fonts[0];

    // Mono font for uppercase section labels / technical metadata
    float monoSize = 11.0f * fontScale;
    ImFontConfig monoCfg;
    monoCfg.OversampleH = 3;
    monoCfg.PixelSnapH = false;
    m_monoFont = nullptr;
#ifdef _WIN32
    m_monoFont = io.Fonts->AddFontFromFileTTF(monoFontPath, monoSize, &monoCfg, glyphRanges);
#elif defined(__APPLE__)
    m_monoFont = io.Fonts->AddFontFromFileTTF(macMonoPath, monoSize, &monoCfg, glyphRanges);
#elif defined(__linux__)
    m_monoFont = addFirstAvailableFont(io, linuxMonoPaths, monoSize, &monoCfg, glyphRanges);
#endif
    if (!m_monoFont) m_monoFont = m_smallFont;

    applyTheme(uiScale);

    // Set font global scale (on Retina: 0.5 to counteract 2x font texture)
    io.FontGlobalScale = m_baseFontGlobalScale * m_uiZoom;

    ImGui_ImplGlfw_InitForOpenGL(window, true);
#ifdef __APPLE__
    ImGui_ImplOpenGL3_Init("#version 150");
#else
    ImGui_ImplOpenGL3_Init("#version 430");
#endif

    // Load inspector tab icons. qlmanage rasterises SVGs onto an opaque
    // white background, so the raw PNG is "black icon on white sheet".
    // We convert it into a proper alpha mask: RGB becomes white, alpha
    // becomes (255 - luminance). That way the ImGui tint parameter in
    // drawInspectorTabIcons() cleanly recolours the silhouette to any
    // hue we pass in.
    {
        const char* paths[4] = {
            "assets/icons/tab_properties.png",
            "assets/icons/tab_mapping.png",
            "assets/icons/tab_audio.png",
            "assets/icons/tab_midi.png",
        };
        for (int i = 0; i < 4; i++) {
            int ch = 0;
            stbi_set_flip_vertically_on_load(false);
            unsigned char* data = stbi_load(paths[i], &m_tabIconW[i], &m_tabIconH[i], &ch, 4);
            if (!data) continue;
            int N = m_tabIconW[i] * m_tabIconH[i];
            for (int p = 0; p < N; p++) {
                unsigned char* px = &data[p * 4];
                // Luma from raw RGB (before we overwrite it). ITU-R BT.601.
                int luma = (px[0] * 299 + px[1] * 587 + px[2] * 114) / 1000;
                // Premultiplied by original alpha so transparent bg stays
                // transparent instead of suddenly going white-opaque.
                int alpha = ((255 - luma) * px[3]) / 255;
                px[0] = 255; px[1] = 255; px[2] = 255;
                px[3] = (unsigned char)alpha;
            }
            GLuint tex = 0;
            glGenTextures(1, &tex);
            glBindTexture(GL_TEXTURE_2D, tex);
            glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8,
                         m_tabIconW[i], m_tabIconH[i], 0,
                         GL_RGBA, GL_UNSIGNED_BYTE, data);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_CLAMP_TO_EDGE);
            glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_CLAMP_TO_EDGE);
            stbi_image_free(data);
            m_tabIconTex[i] = tex;
        }
    }

    return true;
}

unsigned int UIManager::tabIconTex(TabIcon w) const {
    int i = (int)w;
    if (i < 0 || i >= 4) return 0;
    return m_tabIconTex[i];
}

// Draws Layers tab icon — two offset rounded squares overlapping at the
// corner (Figma/Sketch convention). Reads as "layered things" more
// clearly than horizontal-band stacks (which look like a menu).
static void drawLayersGlyph(ImDrawList* dl, ImVec2 mn, ImVec2 mx, ImU32 col) {
    float w = mx.x - mn.x;
    float h = mx.y - mn.y;
    float side = (w < h ? w : h) * 0.62f;
    float ofs  = side * 0.42f;
    float cx = (mn.x + mx.x) * 0.5f;
    float cy = (mn.y + mx.y) * 0.5f;

    // Back square (dimmer, top-left).
    ImU32 backCol = (col & 0x00FFFFFF) | (((col >> 24) * 110 / 255) << 24);
    ImVec2 ba(cx - side * 0.5f - ofs * 0.5f, cy - side * 0.5f - ofs * 0.5f);
    ImVec2 bb(ba.x + side, ba.y + side);
    dl->AddRectFilled(ba, bb, backCol, 2.5f);
    // Front square (full alpha, bottom-right).
    ImVec2 fa(cx - side * 0.5f + ofs * 0.5f, cy - side * 0.5f + ofs * 0.5f);
    ImVec2 fb(fa.x + side, fa.y + side);
    dl->AddRectFilled(fa, fb, col, 2.5f);
    // Outline the front square so the overlap reads cleanly.
    dl->AddRect(fa, fb, IM_COL32(0, 0, 0, 60), 2.5f, 0, 1.0f);
}

// Draws Sources tab icon procedurally — 2x2 grid of small squares (gallery).
static void drawSourcesGlyph(ImDrawList* dl, ImVec2 mn, ImVec2 mx, ImU32 col) {
    float w = mx.x - mn.x;
    float h = mx.y - mn.y;
    float cellW = (w - 2.0f) * 0.5f;
    float cellH = (h - 2.0f) * 0.5f;
    for (int j = 0; j < 2; j++) {
        for (int i = 0; i < 2; i++) {
            ImVec2 a(mn.x + i * (cellW + 2.0f),
                     mn.y + j * (cellH + 2.0f));
            ImVec2 b(a.x + cellW, a.y + cellH);
            dl->AddRectFilled(a, b, col, 1.5f);
        }
    }
}

// Helper: paint the icon for one tab item. Splits texture vs procedural
// glyphs so future tabs can land on either path without duplicating the
// fill/center boilerplate.
static void drawOneTabIcon(ImDrawList* fg, ImGuiTabBar* tabBar,
                           ImGuiTabItem& tab, const char* tabName,
                           unsigned int* iconTex /* size 4 */) {
    if (!tabName) return;

    // Layers/Sources: procedural (no PNG texture).
    // Mapping/Properties/Audio/MIDI: texture from m_tabIconTex.
    enum class Kind { None, Tex, Layers, Sources };
    Kind kind = Kind::None;
    int  texIdx = -1;
    if      (std::strstr(tabName, "###Layers"))     kind = Kind::Layers;
    else if (std::strstr(tabName, "###Sources"))    kind = Kind::Sources;
    else if (std::strstr(tabName, "###Properties")) { kind = Kind::Tex; texIdx = 0; }
    else if (std::strstr(tabName, "###Mapping"))    { kind = Kind::Tex; texIdx = 1; }
    else if (std::strstr(tabName, "###Audio"))      { kind = Kind::Tex; texIdx = 2; }
    else if (std::strstr(tabName, "###MIDI"))       { kind = Kind::Tex; texIdx = 3; }
    if (kind == Kind::None) return;
    if (kind == Kind::Tex && (texIdx < 0 || iconTex[texIdx] == 0)) return;

    float tabX0 = tabBar->BarRect.Min.x + tab.Offset;
    float tabX1 = tabX0 + tab.Width;
    float tabY0 = tabBar->BarRect.Min.y;
    float tabY1 = tabBar->BarRect.Max.y;

    // Fill over the text with the matching tab background colour so
    // the space-padded label vanishes visually; the icon renders on top.
    ImGuiCol bgCol = (tabBar->SelectedTabId == tab.ID)
                      ? ImGuiCol_TabActive : ImGuiCol_Tab;
    ImU32 fillCol = ImGui::GetColorU32(bgCol);
    fg->AddRectFilled(ImVec2(tabX0 + 1, tabY0 + 1),
                       ImVec2(tabX1 - 1, tabY1),
                       fillCol, 4.0f);

    // Icon box — centred, scaled to fit the tab with a small inset.
    float availH = (tabY1 - tabY0) - 8.0f;
    if (availH < 10.0f) availH = 10.0f;
    if (availH > 20.0f) availH = 20.0f;
    float availW = (tabX1 - tabX0) - 8.0f;
    float iconSize = availH < availW ? availH : availW;
    float cx = (tabX0 + tabX1) * 0.5f;
    float cy = (tabY0 + tabY1) * 0.5f;
    ImVec2 imin(cx - iconSize * 0.5f, cy - iconSize * 0.5f);
    ImVec2 imax(cx + iconSize * 0.5f, cy + iconSize * 0.5f);
    ImU32 tint = (tabBar->SelectedTabId == tab.ID)
                 ? IM_COL32(235, 240, 250, 245)
                 : IM_COL32(170, 180, 200, 200);

    if (kind == Kind::Tex) {
        fg->AddImage((ImTextureID)(intptr_t)iconTex[texIdx],
                      imin, imax, ImVec2(0, 0), ImVec2(1, 1), tint);
    } else if (kind == Kind::Layers) {
        drawLayersGlyph(fg, imin, imax, tint);
    } else if (kind == Kind::Sources) {
        drawSourcesGlyph(fg, imin, imax, tint);
    }
}

// ─── Sources-tab procedural glyphs ──────────────────────────────────
// Three lightning bolts — ShaderClaw signature.
static void drawShaderClawGlyph(ImDrawList* dl, ImVec2 mn, ImVec2 mx, ImU32 col) {
    float w = mx.x - mn.x, h = mx.y - mn.y;
    float boltW = w * 0.22f;
    for (int i = 0; i < 3; i++) {
        float x = mn.x + w * (0.18f + 0.32f * float(i));
        ImVec2 p1(x,           mn.y + h * 0.05f);
        ImVec2 p2(x + boltW * 0.55f, mn.y + h * 0.45f);
        ImVec2 p3(x - boltW * 0.10f, mn.y + h * 0.45f);
        ImVec2 p4(x + boltW * 0.45f, mn.y + h * 0.95f);
        dl->AddTriangleFilled(p1, p2, p3, col);
        dl->AddTriangleFilled(p2, p3, p4, col);
    }
}
// Microphone — capsule head + stem.
static void drawMicGlyph(ImDrawList* dl, ImVec2 mn, ImVec2 mx, ImU32 col) {
    float w = mx.x - mn.x, h = mx.y - mn.y;
    float cx = (mn.x + mx.x) * 0.5f;
    float headTopY = mn.y + h * 0.10f;
    float headBotY = mn.y + h * 0.55f;
    float headHalfW = w * 0.22f;
    dl->AddRectFilled(ImVec2(cx - headHalfW, headTopY),
                      ImVec2(cx + headHalfW, headBotY),
                      col, headHalfW);
    // Stem
    dl->AddLine(ImVec2(cx, headBotY + 2.0f),
                ImVec2(cx, mn.y + h * 0.78f),
                col, 1.6f);
    // Base
    dl->AddRectFilled(ImVec2(cx - w * 0.20f, mn.y + h * 0.78f),
                      ImVec2(cx + w * 0.20f, mn.y + h * 0.86f),
                      col, 1.0f);
    dl->AddLine(ImVec2(cx - w * 0.30f, mn.y + h * 0.94f),
                ImVec2(cx + w * 0.30f, mn.y + h * 0.94f),
                col, 1.4f);
}
// Camera — body box + circular lens.
static void drawCameraGlyph(ImDrawList* dl, ImVec2 mn, ImVec2 mx, ImU32 col) {
    float w = mx.x - mn.x, h = mx.y - mn.y;
    // Viewfinder bump
    dl->AddRectFilled(ImVec2(mn.x + w * 0.30f, mn.y + h * 0.10f),
                      ImVec2(mn.x + w * 0.55f, mn.y + h * 0.25f),
                      col, 1.0f);
    // Body
    dl->AddRectFilled(ImVec2(mn.x + w * 0.05f, mn.y + h * 0.25f),
                      ImVec2(mx.x - w * 0.05f, mn.y + h * 0.85f),
                      col, 2.0f);
    // Lens (cut out as background-coloured circle)
    ImU32 hole = IM_COL32(0, 0, 0, 0);
    float cx = (mn.x + mx.x) * 0.5f;
    float cy = mn.y + h * 0.55f;
    float r  = h * 0.18f;
    dl->AddCircleFilled(ImVec2(cx, cy), r, IM_COL32(0, 0, 0, 110), 14);
    // Lens highlight ring
    dl->AddCircle(ImVec2(cx, cy), r * 0.55f,
                  IM_COL32(((col >> 0) & 0xff),
                           ((col >> 8) & 0xff),
                           ((col >> 16) & 0xff), 200), 12, 1.2f);
    (void)hole;
}
// Window display — rectangle with title-bar strip + dot.
static void drawWindowGlyph(ImDrawList* dl, ImVec2 mn, ImVec2 mx, ImU32 col) {
    float w = mx.x - mn.x, h = mx.y - mn.y;
    ImVec2 a(mn.x + w * 0.05f, mn.y + h * 0.18f);
    ImVec2 b(mx.x - w * 0.05f, mx.y - h * 0.18f);
    dl->AddRect(a, b, col, 2.0f, 0, 1.6f);
    // Title bar (filled top strip)
    dl->AddRectFilled(a, ImVec2(b.x, a.y + h * 0.18f), col, 2.0f);
    // Traffic-light dot
    dl->AddCircleFilled(ImVec2(a.x + w * 0.10f, a.y + h * 0.09f),
                        h * 0.04f, IM_COL32(0, 0, 0, 130), 8);
}

void UIManager::drawSourcesTabIcons() {
    // The Sources tab bar lives inside the Sources window. Find its
    // node by the docked-window name and walk its tabs.
    ImGuiWindow* win = ImGui::FindWindowByName("        ###Sources");
    if (!win || !win->DockNode) return;
    ImGuiTabBar* tabBar = nullptr;
    // The internal TabBar is searched by the storage ID that
    // BeginTabBar("##SourcesTabs") generates. Easier path: scan all
    // TabBars in the current ImGui context for one matching the name.
    ImGuiContext& g = *ImGui::GetCurrentContext();
    for (int i = 0; i < g.TabBars.GetMapSize(); i++) {
        ImGuiTabBar* tb = g.TabBars.TryGetMapData(i);
        if (!tb) continue;
        // Match by checking if any of its tabs has a name with
        // "###ShaderClaw" — that's only true for our Sources bar.
        for (int t = 0; t < tb->Tabs.Size; t++) {
            const char* nm = ImGui::TabBarGetTabName(tb, &tb->Tabs[t]);
            if (nm && std::strstr(nm, "###ShaderClaw")) {
                tabBar = tb; break;
            }
        }
        if (tabBar) break;
    }
    if (!tabBar) return;

    ImDrawList* fg = ImGui::GetForegroundDrawList();
    for (int t = 0; t < tabBar->Tabs.Size; t++) {
        ImGuiTabItem& tab = tabBar->Tabs[t];
        const char* tabName = ImGui::TabBarGetTabName(tabBar, &tab);
        if (!tabName) continue;
        enum Kind { K_None, K_Shader, K_Mic, K_Cam, K_Win };
        Kind kind = K_None;
        if      (std::strstr(tabName, "###ShaderClaw")) kind = K_Shader;
        else if (std::strstr(tabName, "###Etherea"))    kind = K_Mic;
        else if (std::strstr(tabName, "###Camera"))     kind = K_Cam;
        else if (std::strstr(tabName, "###Capture"))    kind = K_Win;
        if (kind == K_None) continue;

        float tabX0 = tabBar->BarRect.Min.x + tab.Offset;
        float tabY0 = tabBar->BarRect.Min.y;
        float tabY1 = tabBar->BarRect.Max.y;

        // Icon sits in the leading 4-space pad — about 18px wide.
        float iconSize = (tabY1 - tabY0) - 8.0f;
        if (iconSize < 12.0f) iconSize = 12.0f;
        if (iconSize > 18.0f) iconSize = 18.0f;
        float ix = tabX0 + 6.0f;
        float iy = (tabY0 + tabY1) * 0.5f - iconSize * 0.5f;
        ImVec2 imin(ix, iy), imax(ix + iconSize, iy + iconSize);
        ImU32 tint = (tabBar->SelectedTabId == tab.ID)
                     ? IM_COL32(235, 240, 250, 245)
                     : IM_COL32(170, 180, 200, 200);

        if      (kind == K_Shader) drawShaderClawGlyph(fg, imin, imax, tint);
        else if (kind == K_Mic)    drawMicGlyph(fg, imin, imax, tint);
        else if (kind == K_Cam)    drawCameraGlyph(fg, imin, imax, tint);
        else if (kind == K_Win)    drawWindowGlyph(fg, imin, imax, tint);
    }
}

void UIManager::drawInspectorTabIcons() {
    ImDrawList* fg = ImGui::GetForegroundDrawList();
    // Walk both float-panel TabBars. Mapping moved into the LEFT float
    // (next to Layers + Sources); Properties/Audio/MIDI still on the right.
    ImGuiID floatIds[2] = { m_leftFloatId, m_rightFloatId };
    for (int f = 0; f < 2; f++) {
        if (floatIds[f] == 0) continue;
        ImGuiDockNode* node = ImGui::DockBuilderGetNode(floatIds[f]);
        if (!node || !node->TabBar) continue;
        ImGuiTabBar* tabBar = node->TabBar;
        for (int t = 0; t < tabBar->Tabs.Size; t++) {
            ImGuiTabItem& tab = tabBar->Tabs[t];
            const char* tabName = ImGui::TabBarGetTabName(tabBar, &tab);
            drawOneTabIcon(fg, tabBar, tab, tabName, m_tabIconTex);
        }
    }
}

void UIManager::applyTheme(float dpiScale) {
    ImGuiStyle& s = ImGui::GetStyle();

    // Geometry — canonical design tokens. One block, one set of values
    // applied app-wide so spacing/padding rhythm is consistent. The 10x6
    // frame padding fits "Masks"/"Mapping" tabs without truncation at any
    // reasonable dock width.
    s.WindowPadding     = ImVec2(18, 16);
    s.FramePadding      = ImVec2(10, 6);
    s.CellPadding       = ImVec2(8, 5);
    s.ItemSpacing       = ImVec2(8, 6);
    s.ItemInnerSpacing  = ImVec2(8, 4);
    s.IndentSpacing     = 16.0f;
    s.ScrollbarSize     = 10.0f;
    s.GrabMinSize       = 16.0f;
    s.SeparatorTextBorderSize = 1.0f;
    s.SeparatorTextPadding    = ImVec2(12, 6);

    // Rounding — uniform 8px tactile radius across all interactive frames,
    // 10px for containers. This eliminates the rounded/sharp inconsistency
    // the user called out (Tiling sliders, dropdowns, etc.). No more pill
    // (100px) shapes mixed with rounded rectangles — every interactive
    // surface reads as the same "interactive" affordance.
    s.WindowRounding    = 10.0f;
    s.ChildRounding     = 8.0f;
    s.FrameRounding     = 8.0f;
    s.PopupRounding     = 10.0f;
    s.ScrollbarRounding = 8.0f;
    s.GrabRounding      = 8.0f;
    s.TabRounding       = 8.0f;

    // Border layout preserved (WindowBorderSize / ChildBorderSize stay
    // at 1) so panel content rects don't shift; Border color alpha is
    // zeroed below in the palette so the line is invisible. This kills
    // the stray edge-stroke without breaking dock geometry.
    // Borders off by default — outlines on every window were bleeding into
    // the viewport between docked panels. Only Layers and Parameters panels
    // opt in (via PushStyleVar). The top nav and timeline-transport edges
    // are drawn explicitly with AddLine, not via Window/Child borders.
    s.WindowBorderSize  = 0.0f;
    s.ChildBorderSize   = 0.0f;
    s.FrameBorderSize   = 0.0f;
    s.PopupBorderSize   = 1.0f;
    s.TabBorderSize     = 0.0f;

    // Alignment
    s.WindowTitleAlign  = ImVec2(0.0f, 0.5f);    // left-align titles (Linear pattern)
    s.SeparatorTextAlign = ImVec2(0.0f, 0.5f);

    // Disabled alpha for "on dark" contexts
    s.DisabledAlpha     = 0.45f;

    // Anti-aliasing
    s.AntiAliasedLines  = true;
    s.AntiAliasedFill   = true;

    // --- Color Palette — strict monochrome ramp ---
    // Background: near-black canvas with micro luminance steps. NO blue,
    // NO cyan, NO teal. White-with-alpha for every accent.
    ImVec4 bgVoid       = ImVec4(0.05f, 0.06f, 0.07f, 1.00f);  // darkest
    ImVec4 bgDeep       = ImVec4(0.07f, 0.08f, 0.09f, 1.00f);  // marketing black
    ImVec4 bgPanel      = ImVec4(0.09f, 0.10f, 0.12f, 1.00f);  // panel bg — opaque so docked panels fully occlude the canvas behind
    ImVec4 bgWidget     = ImVec4(0.13f, 0.14f, 0.16f, 1.00f);  // frame bg
    ImVec4 bgWidgetHov  = ImVec4(0.18f, 0.19f, 0.22f, 1.00f);  // frame bg hovered
    ImVec4 bgWidgetAct  = ImVec4(0.24f, 0.26f, 0.30f, 1.00f);  // frame bg active

    // Borders — semi-transparent white, Linear's signature "moonlight" edges
    ImVec4 borderSubtle = ImVec4(1.0f, 1.0f, 1.0f, 0.05f);
    ImVec4 border       = ImVec4(1.0f, 1.0f, 1.0f, 0.08f);
    ImVec4 borderStrong = ImVec4(1.0f, 1.0f, 1.0f, 0.12f);

    // Monochrome "accent" — no chromatic color anywhere. Interactions use
    // white-alpha tiers on the dark canvas, giving a dark/gray/light-gray palette.
    ImVec4 brand        = ImVec4(1.0f, 1.0f, 1.0f, 1.00f); // solid white for key interactive fills
    ImVec4 accent       = ImVec4(1.0f, 1.0f, 1.0f, 0.90f); // slider grabs, checkmarks
    ImVec4 accentHover  = ImVec4(1.0f, 1.0f, 1.0f, 1.00f);
    ImVec4 accentSoft   = ImVec4(1.0f, 1.0f, 1.0f, 0.07f); // selection / header bg (light gray tint)
    ImVec4 accentMed    = ImVec4(1.0f, 1.0f, 1.0f, 0.12f); // hover-header (slightly stronger)

    // Text tiers — neutral grays, no warm/cool bias
    ImVec4 textPrimary  = ImVec4(0.94f, 0.95f, 0.97f, 1.00f);
    ImVec4 textSecondary= ImVec4(0.78f, 0.80f, 0.84f, 1.00f);
    ImVec4 textTertiary = ImVec4(0.55f, 0.58f, 0.62f, 1.00f);
    ImVec4 textDisabled = ImVec4(0.45f, 0.48f, 0.55f, 1.00f);

    // Semantic — muted, used sparingly
    ImVec4 success      = ImVec4(0.063f, 0.725f, 0.506f, 1.00f); // #10b981
    ImVec4 warning      = ImVec4(1.000f, 0.720f, 0.240f, 1.00f);
    ImVec4 error        = ImVec4(0.950f, 0.320f, 0.380f, 1.00f);
    (void)bgVoid; (void)borderStrong; (void)textTertiary; (void)error;
    (void)bgWidget; (void)bgWidgetHov; (void)bgWidgetAct; (void)textSecondary; (void)success;

    auto* c = s.Colors;

    // Window
    c[ImGuiCol_WindowBg]            = bgPanel;
    c[ImGuiCol_ChildBg]             = ImVec4(1.0f, 1.0f, 1.0f, 0.02f);
    c[ImGuiCol_PopupBg]             = ImVec4(bgWidget.x, bgWidget.y, bgWidget.z, 0.96f);
    c[ImGuiCol_Border]              = border;
    c[ImGuiCol_BorderShadow]        = ImVec4(0, 0, 0, 0);

    // Text
    c[ImGuiCol_Text]                = textPrimary;
    c[ImGuiCol_TextDisabled]        = textDisabled;

    // Title bar
    c[ImGuiCol_TitleBg]             = ImVec4(bgPanel.x, bgPanel.y, bgPanel.z, 0.92f);
    c[ImGuiCol_TitleBgActive]       = ImVec4(bgWidget.x, bgWidget.y, bgWidget.z, 0.95f);
    c[ImGuiCol_TitleBgCollapsed]    = ImVec4(bgDeep.x, bgDeep.y, bgDeep.z, 0.75f);

    // Menu bar
    c[ImGuiCol_MenuBarBg]           = bgDeep;

    // Frames (inputs, sliders) — neutral gray track
    c[ImGuiCol_FrameBg]             = bgWidget;
    c[ImGuiCol_FrameBgHovered]      = bgWidgetHov;
    c[ImGuiCol_FrameBgActive]       = bgWidgetAct;

    // Tabs — neutral gray, white at low alpha for active
    c[ImGuiCol_Tab]                 = ImVec4(1.0f, 1.0f, 1.0f, 0.02f);
    c[ImGuiCol_TabHovered]          = ImVec4(1.0f, 1.0f, 1.0f, 0.06f);
    c[ImGuiCol_TabActive]           = ImVec4(1.0f, 1.0f, 1.0f, 0.18f);
    c[ImGuiCol_TabUnfocused]        = ImVec4(0, 0, 0, 0);
    c[ImGuiCol_TabUnfocusedActive]  = ImVec4(1.0f, 1.0f, 1.0f, 0.04f);

    // Headers (CollapsingHeader, Selectable)
    c[ImGuiCol_Header]              = accentSoft;
    c[ImGuiCol_HeaderHovered]       = ImVec4(1.0f, 1.0f, 1.0f, 0.04f);
    c[ImGuiCol_HeaderActive]        = ImVec4(1.0f, 1.0f, 1.0f, 0.18f);

    // Buttons — ghost
    c[ImGuiCol_Button]              = ImVec4(1.0f, 1.0f, 1.0f, 0.03f);
    c[ImGuiCol_ButtonHovered]       = ImVec4(1.0f, 1.0f, 1.0f, 0.07f);
    c[ImGuiCol_ButtonActive]        = accentMed;

    // Checkmarks / slider grabs — STRICT WHITE-ALPHA, no chromatic accent
    c[ImGuiCol_CheckMark]           = ImVec4(1.0f, 1.0f, 1.0f, 0.95f);
    c[ImGuiCol_SliderGrab]          = ImVec4(1.0f, 1.0f, 1.0f, 0.55f);
    c[ImGuiCol_SliderGrabActive]    = ImVec4(1.0f, 1.0f, 1.0f, 0.85f);

    // Scrollbar
    c[ImGuiCol_ScrollbarBg]         = ImVec4(0, 0, 0, 0);
    c[ImGuiCol_ScrollbarGrab]       = ImVec4(1.0f, 1.0f, 1.0f, 0.06f);
    c[ImGuiCol_ScrollbarGrabHovered]= ImVec4(1.0f, 1.0f, 1.0f, 0.14f);
    c[ImGuiCol_ScrollbarGrabActive] = ImVec4(1.0f, 1.0f, 1.0f, 0.22f);

    // Separator — white-alpha tiers (no chromatic)
    c[ImGuiCol_Separator]           = ImVec4(1.0f, 1.0f, 1.0f, 0.10f);
    c[ImGuiCol_SeparatorHovered]    = ImVec4(1.0f, 1.0f, 1.0f, 0.20f);
    c[ImGuiCol_SeparatorActive]     = ImVec4(1.0f, 1.0f, 1.0f, 0.35f);

    // Resize grip — white-alpha tiers
    c[ImGuiCol_ResizeGrip]          = ImVec4(1.0f, 1.0f, 1.0f, 0.05f);
    c[ImGuiCol_ResizeGripHovered]   = ImVec4(1.0f, 1.0f, 1.0f, 0.15f);
    c[ImGuiCol_ResizeGripActive]    = ImVec4(1.0f, 1.0f, 1.0f, 0.30f);

    // Plot — was potentially blue/teal in some places; force white-alpha
    c[ImGuiCol_PlotLines]           = ImVec4(1.0f, 1.0f, 1.0f, 0.55f);
    c[ImGuiCol_PlotLinesHovered]    = ImVec4(1.0f, 1.0f, 1.0f, 0.85f);
    c[ImGuiCol_PlotHistogram]       = ImVec4(1.0f, 1.0f, 1.0f, 0.40f);
    c[ImGuiCol_PlotHistogramHovered]= ImVec4(1.0f, 1.0f, 1.0f, 0.65f);

    // Docking
    c[ImGuiCol_DockingPreview]      = ImVec4(1.0f, 1.0f, 1.0f, 0.40f);
    c[ImGuiCol_DockingEmptyBg]      = bgDeep;

    // Nav focus — white-alpha
    c[ImGuiCol_NavHighlight]        = ImVec4(1.0f, 1.0f, 1.0f, 0.45f);

    // Text selection
    c[ImGuiCol_TextSelectedBg]      = ImVec4(1.0f, 1.0f, 1.0f, 0.18f);

    // Table
    c[ImGuiCol_TableHeaderBg]       = ImVec4(1.0f, 1.0f, 1.0f, 0.02f);
    c[ImGuiCol_TableBorderStrong]   = border;
    c[ImGuiCol_TableBorderLight]    = borderSubtle;
    c[ImGuiCol_TableRowBg]          = ImVec4(0, 0, 0, 0);
    c[ImGuiCol_TableRowBgAlt]       = ImVec4(1.0f, 1.0f, 1.0f, 0.015f);

    // Drag/drop — white-alpha (was warning amber)
    c[ImGuiCol_DragDropTarget]      = ImVec4(1.0f, 1.0f, 1.0f, 0.6f);

    // Modal dim — deep, near-opaque (Linear uses 0.85)
    c[ImGuiCol_ModalWindowDimBg]    = ImVec4(0.0f, 0.0f, 0.0f, 0.85f);

    // Scale for DPI
    s.ScaleAllSizes(dpiScale);
}

void UIManager::handleZoom() {
    ImGuiIO& io = ImGui::GetIO();

    // Cmd+= / Cmd+- on macOS, Ctrl+= / Ctrl+- on Windows/Linux
#ifdef __APPLE__
    bool mod = io.KeySuper;
#else
    bool mod = io.KeyCtrl;
#endif

    if (mod && ImGui::IsKeyPressed(ImGuiKey_Equal)) {   // + / =
        m_uiZoom = std::min(m_uiZoom + 0.1f, 3.0f);
        io.FontGlobalScale = m_baseFontGlobalScale * m_uiZoom;
    }
    if (mod && ImGui::IsKeyPressed(ImGuiKey_Minus)) {   // -
        m_uiZoom = std::max(m_uiZoom - 0.1f, 0.4f);
        io.FontGlobalScale = m_baseFontGlobalScale * m_uiZoom;
    }
    if (mod && ImGui::IsKeyPressed(ImGuiKey_0)) {       // reset
        m_uiZoom = 1.0f;
        io.FontGlobalScale = m_baseFontGlobalScale * m_uiZoom;
    }
}

void UIManager::shutdown() {
    ImGui_ImplOpenGL3_Shutdown();
    ImGui_ImplGlfw_Shutdown();
    ImGui::DestroyContext();
}

void UIManager::beginFrame() {
    ImGui_ImplOpenGL3_NewFrame();
    ImGui_ImplGlfw_NewFrame();
    ImGui::NewFrame();
    // ImGuizmo wants its frame state reset right after ImGui::NewFrame().
    // Without this the 3D Stage manipulator handles never render and
    // IsOver()/IsUsing() return stale values, so select/move/rotate/scale
    // silently no-op when you click them.
    ImGuizmo::BeginFrame();
    handleZoom();
}

void UIManager::endFrame() {
    ImGui::Render();
    ImGui_ImplOpenGL3_RenderDrawData(ImGui::GetDrawData());
}

void UIManager::setupDockspace(float bottomBarHeight) {
    ImGuiViewport* viewport = ImGui::GetMainViewport();
    ImVec2 dockPos = viewport->WorkPos;
    dockPos.y += m_workspaceBarHeight;           // shift below the primary nav bar
    ImGui::SetNextWindowPos(dockPos);
    ImVec2 dockSize = viewport->WorkSize;
    dockSize.y -= (bottomBarHeight + m_workspaceBarHeight);
    ImGui::SetNextWindowSize(dockSize);
    ImGui::SetNextWindowViewport(viewport->ID);

    ImGuiWindowFlags flags =
        ImGuiWindowFlags_NoDocking |
        ImGuiWindowFlags_NoTitleBar |
        ImGuiWindowFlags_NoCollapse |
        ImGuiWindowFlags_NoResize |
        ImGuiWindowFlags_NoMove |
        ImGuiWindowFlags_NoBringToFrontOnFocus |
        ImGuiWindowFlags_NoNavFocus |
        ImGuiWindowFlags_NoBackground;

    ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding, 0.0f);
    ImGui::PushStyleVar(ImGuiStyleVar_WindowBorderSize, 0.0f);
    ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(0, 0));

    ImGui::Begin("DockSpace", nullptr, flags);
    ImGui::PopStyleVar(3);

    // 1px hairline along the dockspace top edge — this is the underline
    // beneath the main nav. Drawn explicitly because WindowBorderSize is now
    // 0 globally (so panels stop bleeding outlines into the viewport).
    {
        ImDrawList* fg = ImGui::GetForegroundDrawList();
        fg->AddLine(ImVec2(dockPos.x, dockPos.y),
                    ImVec2(dockPos.x + dockSize.x, dockPos.y),
                    IM_COL32(255, 255, 255, 25), 1.0f);
    }

    ImGuiID dockspaceId = ImGui::GetID("EaselDockSpace");
    // Push a taller frame padding for the DockSpace only — ImGui derives
    // dock tab height from style.FramePadding.y, so bumping the Y here
    // gives the inspector tabs more vertical breathing room while
    // leaving regular buttons/sliders elsewhere untouched.
    ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(10.0f, 12.0f));
    ImGui::DockSpace(dockspaceId, ImVec2(0, 0), ImGuiDockNodeFlags_PassthruCentralNode);
    ImGui::PopStyleVar();

    // Only one of Canvas/Stage submits Begin() per frame (guarded by
    // sShowStage), so the main dock has a single-window tab bar. Still
    // hide it: NoTabBar keeps the strip out and the pill is the sole
    // switcher.
    if (ImGuiWindow* cw = ImGui::FindWindowByName("Canvas")) {
        if (cw->DockNode) {
            cw->DockNode->LocalFlags |= ImGuiDockNodeFlags_NoTabBar
                                       | ImGuiDockNodeFlags_NoWindowMenuButton;
        }
    }
    if (ImGuiWindow* sw = ImGui::FindWindowByName("Stage")) {
        if (sw->DockNode) {
            sw->DockNode->LocalFlags |= ImGuiDockNodeFlags_NoTabBar
                                       | ImGuiDockNodeFlags_NoWindowMenuButton;
        }
    }

    // Rebuild layout on first frame or when window size changes significantly
    bool sizeChanged = false;
    if (!m_firstFrame) {
        float dw = fabsf(dockSize.x - m_lastDockW);
        float dh = fabsf(dockSize.y - m_lastDockH);
        // Trigger rebuild if size changed by more than 100px in either dimension
        // (catches maximize, restore, resolution change)
        if (dw > 100.0f || dh > 100.0f) {
            sizeChanged = true;
        }
    }

    // Trigger a full dock-layout rebuild whenever the workspace mode
    // changes — Canvas / Stage / Show each have a different right-rail
    // composition (Properties+Sources vs Mapping vs MIDI+Audio), and we
    // want hidden panels to vacate their dock slot so visible ones grow
    // into the freed space rather than leaving an empty rectangle.
    static WorkspaceMode s_lastMode = sMode;
    bool modeChanged = (s_lastMode != sMode);
    s_lastMode = sMode;

    if (m_firstFrame || sizeChanged || modeChanged) {
        m_firstFrame = false;
        m_lastDockW = dockSize.x;
        m_lastDockH = dockSize.y;

        // Always rebuild layout to ensure clean state
        ImGui::DockBuilderRemoveNode(dockspaceId);
        ImGui::DockBuilderAddNode(dockspaceId, ImGuiDockNodeFlags_DockSpace);
        ImGui::DockBuilderSetNodeSize(dockspaceId, dockSize);

        // Floating workspace layout:
        //   - Main dockspace holds only Canvas/Stage/Mapping tabs (full width)
        //     and the Timeline strip at the bottom.
        //   - Layers + Sources live in a FLOATING dock group pinned to the
        //     left edge of the viewport.
        //   - Properties + Audio + MIDI live in a FLOATING dock group pinned
        //     to the right edge.
        // The canvas shows through behind both floating groups, matching the
        // "floating overlay" concept.
        // Timeline defaults to minimised (transport row only — see
        // m_timelineMinimized = true in Application), so the initial dock
        // split targets that collapsed height directly. This keeps the floating
        // panels from reserving space for a tall timeline that never appears.
        // Layout: full-bleed central dockspace (Canvas/Stage/Show fill
        // the whole viewport) + Timeline at the bottom + two FLOATING
        // overlay nodes (Layers/Sources on the left, Properties/Mapping/
        // Audio/MIDI on the right). The floating nodes are positioned
        // every frame via DockBuilderSetNodePos in the per-frame reflow
        // block below — that's required because standalone floating dock
        // roots don't otherwise persist their position.
        ImGuiID mainId, timelineDockId;
        float timelineSplit = (dockSize.y > 0) ? (60.0f / dockSize.y) : 0.05f;
        if (timelineSplit < 0.03f) timelineSplit = 0.03f;
        if (timelineSplit > 0.20f) timelineSplit = 0.20f;
        ImGui::DockBuilderSplitNode(dockspaceId, ImGuiDir_Down, timelineSplit,
                                    &timelineDockId, &mainId);

        auto dockAlways = [](const char* name, ImGuiID node) {
            ImGui::DockBuilderDockWindow(name, node);
        };

        // Center peer tabs — one submits Begin() per frame (gated by sMode).
        dockAlways("Canvas", mainId);
        dockAlways("Stage",  mainId);
        dockAlways("Show",   mainId);
        if (ImGuiDockNode* mn = ImGui::DockBuilderGetNode(mainId)) {
            mn->LocalFlags |= ImGuiDockNodeFlags_HiddenTabBar
                            | ImGuiDockNodeFlags_NoWindowMenuButton;
        }
        dockAlways("Timeline", timelineDockId);
        if (ImGuiDockNode* tln = ImGui::DockBuilderGetNode(timelineDockId)) {
            tln->LocalFlags |= ImGuiDockNodeFlags_NoWindowMenuButton
                             | ImGuiDockNodeFlags_NoTabBar;
        }

        // Floating overlay panels — docked into TWO host windows
        // rendered below in renderFloatPanelHosts() with positions
        // forced via SetNextWindowPos every frame. The DockSpace IDs
        // are hardcoded constants so the IDs we dock into here match
        // the IDs used in the host-window DockSpace() calls — using
        // ImGui::GetID() would salt by current window stack and the
        // IDs wouldn't match.
        const ImGuiID leftFloatId  = 0xE45E1FF7;
        const ImGuiID rightFloatId = 0xE45E2008;

        // All three left-float panels use the icon-pad pattern so
        // drawInspectorTabIcons() can render their tab icons consistently.
        dockAlways("        ###Layers",     leftFloatId);
        dockAlways("        ###Sources",    leftFloatId);
        dockAlways("        ###Mapping",    leftFloatId);
        dockAlways("        ###Properties", rightFloatId);
        dockAlways("        ###Audio",      rightFloatId);
        dockAlways("        ###MIDI",       rightFloatId);
        ImGui::DockBuilderDockWindow("Scene Scanner", rightFloatId);

        // Strip the dock chrome (tab bar + chevron) from the left float
        // so the rail's icons are the sole way to switch panels — no
        // duplicate tab strip + collapse arrow inside the panel header.
        if (ImGuiDockNode* ln = ImGui::DockBuilderGetNode(leftFloatId)) {
            ln->LocalFlags |= ImGuiDockNodeFlags_NoTabBar
                            | ImGuiDockNodeFlags_NoWindowMenuButton;
        }

        ImGui::DockBuilderFinish(dockspaceId);

        m_timelineDockId = timelineDockId;
        m_leftFloatId    = leftFloatId;
        m_rightFloatId   = rightFloatId;
        m_leftFloatW     = 320.0f;
        m_rightFloatW    = 320.0f;
        m_lastTimelineH  = dockSize.y * timelineSplit;

        // Two deferred focus passes: Canvas in the big left slot, Layers as
        // the active tab in the top-right. SetWindowFocus is called every
        // frame while m_pendingFocusFramesLeft > 0 so both settle correctly.
        // In Stage mode, surface Mapping (not Properties) as the active
        // right-rail tab — when the user goes Canvas→Stage they expect to
        // see the mapping controls front and center for projector calibration.
        if (sMode == WorkspaceMode::Stage) {
            m_pendingFocus = "        ###Mapping";
        } else {
            m_pendingFocus = "Layers";
        }
        m_pendingFocusFramesLeft = 3;
    } else {
        // Track size for change detection even when not rebuilding
        m_lastDockW = dockSize.x;
        m_lastDockH = dockSize.y;
    }

    ImGui::End();

    // ── Floating overlay host windows ──
    // Two regular ImGui windows (LeftFloat, RightFloat) with positions
    // forced via SetNextWindowPos every frame. Each contains a DockSpace
    // with a hardcoded ID — Layers/Sources are docked into the left
    // dockspace, Properties/Mapping/Audio/MIDI into the right. The
    // hardcoded IDs MUST match the ones used in the layout-rebuild
    // dockAlways() calls above; since GetID() is salted by the current
    // window stack, we can't compute the same hash from two different
    // stack contexts.
    {
        const ImGuiID kLeftFloatId  = 0xE45E1FF7;
        const ImGuiID kRightFloatId = 0xE45E2008;
        const float kFloatTopReserve = 6.0f;
        const float kFloatMargin     = 12.0f;

        // Vertical band: from below the menu/secondary-nav row, to just
        // above the timeline. Use the timeline dock node's actual height
        // (so the float panels follow the timeline splitter live).
        float headerReserve = ImGui::GetFrameHeight() + 22.0f;
        float timelineH = 60.0f;
        if (m_timelineDockId != 0) {
            if (ImGuiDockNode* tln = ImGui::DockBuilderGetNode(m_timelineDockId))
                timelineH = tln->Size.y;
        }
        float floatY = viewport->WorkPos.y + headerReserve + kFloatTopReserve;
        float floatH = std::max(120.0f,
            dockSize.y - headerReserve - timelineH - kFloatTopReserve - kFloatMargin);

        float leftW  = std::min(360.0f, dockSize.x * 0.22f);
        // In Stage mode the user wants a noticeably wider, anchored right
        // column (Mapping & Properties) that reads as a fixed dock — bump
        // it to ~26% of the work area, no upper clamp short of the dock
        // width itself, so it stops feeling like a thin floating overlay.
        float rightW = (sMode == WorkspaceMode::Stage)
                         ? std::max(320.0f, dockSize.x * 0.26f)
                         : std::min(340.0f, dockSize.x * 0.22f);
        // Persist for other UI code that needs to reserve space (e.g. Stage
        // panel reserves the right column out of its central content width).
        m_rightFloatW = rightW;
        m_leftFloatW  = leftW;

        ImGuiWindowFlags hostFlags =
            ImGuiWindowFlags_NoTitleBar |
            ImGuiWindowFlags_NoResize |
            ImGuiWindowFlags_NoMove |
            ImGuiWindowFlags_NoCollapse |
            ImGuiWindowFlags_NoDocking |
            ImGuiWindowFlags_NoSavedSettings;

        // Skip the entire host window when no panel docked into it is
        // visible in the current mode — otherwise an empty dock chrome
        // sits over the canvas (e.g. blank Layers/Sources strip in
        // Stage mode where both panels are hidden).
        bool leftHasContent  = isPanelVisible("Layers")  || isPanelVisible("Sources");
        bool rightHasContent = isPanelVisible("Properties") || isPanelVisible("Mapping")
                            || isPanelVisible("Audio")      || isPanelVisible("MIDI");

        // Left host — shifted right of the activity rail.
        if (leftHasContent) {
            ImGui::SetNextWindowPos (ImVec2(viewport->WorkPos.x + kLeftRailW + kFloatMargin, floatY),
                                     ImGuiCond_Always);
            ImGui::SetNextWindowSize(ImVec2(leftW, floatH), ImGuiCond_Always);
            ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(0, 0));
            if (ImGui::Begin("##LeftFloatHost", nullptr, hostFlags)) {
                ImGui::DockSpace(kLeftFloatId, ImVec2(0, 0),
                                 ImGuiDockNodeFlags_NoDockingSplit |
                                 ImGuiDockNodeFlags_NoUndocking);
            }
            ImGui::End();
            ImGui::PopStyleVar();
        }

        // Right host
        if (rightHasContent) {
            ImGui::SetNextWindowPos (
                ImVec2(viewport->WorkPos.x + dockSize.x - rightW - kFloatMargin, floatY),
                ImGuiCond_Always);
            ImGui::SetNextWindowSize(ImVec2(rightW, floatH), ImGuiCond_Always);
            ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(0, 0));
            if (ImGui::Begin("##RightFloatHost", nullptr, hostFlags)) {
                ImGui::DockSpace(kRightFloatId, ImVec2(0, 0),
                                 ImGuiDockNodeFlags_NoDockingSplit |
                                 ImGuiDockNodeFlags_NoUndocking);
            }
            ImGui::End();
            ImGui::PopStyleVar();
        }
    }

    // Apply deferred focus. SetWindowFocus only works if the named window
    // has been Begin()-ed at least once in a previous frame, so we may need
    // to retry across a couple frames after a dock rebuild.
    // We now focus BOTH Layers (left group) and Properties (right group) —
    // without the second call, Mapping's early Begin (from the warp editor's
    // preamble render) steals focus in the right tab group.
    if (m_pendingFocus) {
        // In Stage mode the primary focus IS the right-rail Mapping tab —
        // calling it last means Mapping wins selection in its dock group.
        // In other modes, Layers takes the left, then Properties wins the
        // right group (originally to beat Mapping's early Begin).
        if (sMode == WorkspaceMode::Stage) {
            ImGui::SetWindowFocus(m_pendingFocus);  // "        ###Mapping"
        } else {
            ImGui::SetWindowFocus(m_pendingFocus);  // typically "Layers"
            // Also raise Properties so it leads the right floating group's tabs.
            // Calling it AFTER the primary focus means Properties is the
            // most-recently-focused window in ITS dock; its dock updates its
            // selected tab independently of the left group's selection.
            ImGui::SetWindowFocus("        ###Properties");
        }
        m_pendingFocusFramesLeft--;
        if (m_pendingFocusFramesLeft <= 0) m_pendingFocus = nullptr;
    }
}

void UIManager::setWorkspace(Workspace w) {
    if (w == m_workspace) return;
    m_workspace = w;
    // Force dock layout rebuild on next setupDockspace call
    m_firstFrame = true;
}

bool UIManager::isPanelVisible(const char* title) const {
    // Per-mode panel curation. Three workspaces, three different right-
    // rail compositions. Bottom dock (Timeline) stays in every mode.
    if (!title) return true;
    auto eq = [&](const char* x) { return std::strcmp(title, x) == 0; };

    switch (sMode) {
    case WorkspaceMode::Canvas:
        // Editing the comp: only ONE of {Layers, Sources, Mapping} is
        // visible at a time, gated by the activity rail's selection.
        // Properties stays docked on the right rail. Masks ride along
        // with Mapping.
        if (eq("Layers"))     return m_activeLeftPanel == LeftPanel::Layers;
        if (eq("Sources"))    return m_activeLeftPanel == LeftPanel::Sources;
        if (eq("Mapping"))    return m_activeLeftPanel == LeftPanel::Mapping;
        if (eq("Properties")) return true;
        if (eq("Timeline"))   return true;
        if (eq("Canvas"))     return true;
        return false;

    case WorkspaceMode::Stage:
        // Stage is the 3D pre-viz: physical surfaces, projector frusta,
        // environment. Mapping + Masks moved out to Canvas — Stage now
        // focuses on the spatial setup, not warp calibration.
        if (eq("Stage"))      return true;
        if (eq("Properties")) return true;
        if (eq("Timeline"))   return true;
        return false;

    case WorkspaceMode::Show:
        // Live performance: MIDI + Audio on the right, Timeline at the
        // bottom. No layer editing surfaces.
        if (eq("Show"))       return true;
        if (eq("MIDI"))       return true;
        if (eq("Audio"))      return true;
        if (eq("Timeline"))   return true;
        return false;
    }
    return true;
}

void UIManager::renderLeftRail(const std::function<void(float innerW)>& drawExtra) {
    // Activity rail — fixed-width vertical strip on the left edge with
    // icon buttons that toggle which left panel is active. Reuses the
    // procedural glyphs already defined for the float-panel tabs.
    if (sMode != WorkspaceMode::Canvas) return;  // Stage/Show have no left panels

    ImGuiViewport* vp = ImGui::GetMainViewport();
    float topReserve = m_workspaceBarHeight + ImGui::GetFrameHeight() + 6.0f;
    float h = vp->WorkSize.y - topReserve - 12.0f;
    ImGui::SetNextWindowPos (ImVec2(vp->WorkPos.x,
                                     vp->WorkPos.y + topReserve),
                              ImGuiCond_Always);
    ImGui::SetNextWindowSize(ImVec2(kLeftRailW, h), ImGuiCond_Always);

    ImGuiWindowFlags flags =
        ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoResize  |
        ImGuiWindowFlags_NoMove     | ImGuiWindowFlags_NoCollapse|
        ImGuiWindowFlags_NoDocking  | ImGuiWindowFlags_NoSavedSettings;

    ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(6, 12));
    ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding, 0.0f);
    ImGui::PushStyleColor(ImGuiCol_WindowBg, IM_COL32(10, 11, 14, 255));

    if (ImGui::Begin("##LeftRail", nullptr, flags)) {
        // Hairline divider at top, just below where the workspace bar ends.
        {
            ImDrawList* dlTop = ImGui::GetWindowDrawList();
            ImVec2 wp = ImGui::GetWindowPos();
            float wW = ImGui::GetWindowSize().x;
            dlTop->AddLine(ImVec2(wp.x, wp.y + 0.5f),
                           ImVec2(wp.x + wW, wp.y + 0.5f),
                           IM_COL32(255, 255, 255, 22), 1.0f);
        }
        struct Item { LeftPanel which; const char* lbl; };
        const Item items[] = {
            { LeftPanel::Layers,  "##rail_layers"  },
            { LeftPanel::Sources, "##rail_sources" },
            { LeftPanel::Mapping, "##rail_mapping" },
        };
        ImDrawList* dl = ImGui::GetWindowDrawList();
        for (int i = 0; i < 3; i++) {
            const Item& it = items[i];
            bool active = (m_activeLeftPanel == it.which);
            ImVec2 cursor = ImGui::GetCursorScreenPos();
            ImVec2 sz(kLeftRailW - 12.0f, 44.0f);
            // Background pill
            ImU32 bgCol = active
                ? IM_COL32(255, 255, 255, 26)
                : IM_COL32(255, 255, 255, 0);
            dl->AddRectFilled(cursor,
                              ImVec2(cursor.x + sz.x, cursor.y + sz.y),
                              bgCol, 8.0f);
            // Glyph
            ImVec2 gMin(cursor.x + (sz.x - 26.0f) * 0.5f,
                        cursor.y + (sz.y - 26.0f) * 0.5f);
            ImVec2 gMax(gMin.x + 26.0f, gMin.y + 26.0f);
            ImU32 tint = active ? IM_COL32(240, 245, 255, 255)
                                : IM_COL32(160, 170, 190, 220);
            if      (it.which == LeftPanel::Layers)  drawLayersGlyph (dl, gMin, gMax, tint);
            else if (it.which == LeftPanel::Sources) drawSourcesGlyph(dl, gMin, gMax, tint);
            else if (it.which == LeftPanel::Mapping) {
                if (m_tabIconTex[1]) {
                    dl->AddImage((ImTextureID)(intptr_t)m_tabIconTex[1],
                                 gMin, gMax, ImVec2(0,0), ImVec2(1,1), tint);
                }
            }
            // Invisible button captures the click.
            if (ImGui::InvisibleButton(it.lbl, sz)) {
                m_activeLeftPanel = active ? LeftPanel::None : it.which;
            }
            ImGui::Dummy(ImVec2(0, 4));
        }
        // Phase 2 — divider + per-layer thumbnails callback. Application
        // owns the layer stack so it draws thumbs itself; we just provide
        // a hairline divider and the inner width so it can size correctly.
        if (drawExtra) {
            ImGui::Dummy(ImVec2(0, 6));
            ImVec2 dPos = ImGui::GetCursorScreenPos();
            float divW = kLeftRailW - 16.0f;
            ImGui::GetWindowDrawList()->AddLine(
                ImVec2(dPos.x + 8.0f, dPos.y),
                ImVec2(dPos.x + 8.0f + divW, dPos.y),
                IM_COL32(255, 255, 255, 30), 1.0f);
            ImGui::Dummy(ImVec2(0, 10));
            drawExtra(kLeftRailW - 12.0f);
        }
    }
    ImGui::End();
    ImGui::PopStyleColor();
    ImGui::PopStyleVar(2);
}

void UIManager::renderWorkspaceBar() {
    // No-op. Workspace switcher now lives inside the main menu bar
    // (see Application::renderMenuBar). Kept as a method so the API
    // doesn't break callers and future redesigns can slot back in.
    m_workspaceBarHeight = 0.0f;
}
