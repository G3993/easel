#include "ui/LayerPanel.h"
#include "ui/UIManager.h"
#include "ui/LucideIcons.h"
#include "sources/FluidSource3D.h"
#include <imgui.h>
#include <imgui_internal.h>
#include <algorithm>
#include <cstring>
#include <cstdio>
#include <cmath>
#include <map>
#include <set>

// Theme — every colour is a UITokens grey or white-at-alpha. No hue.
static const ImU32 kTextPrimary   = UITokens::kTextPrimary;
static const ImU32 kTextSecondary = UITokens::kTextSecondary;
static const ImU32 kTextTertiary  = UITokens::kTextTertiary;
static const ImU32 kTextMuted     = UITokens::kTextTertiary;   // group header count
static const ImU32 kRowHoverFill  = IM_COL32(255, 255, 255, 12);
static const ImU32 kRowSelFill    = IM_COL32(255, 255, 255, 30);
static const ImU32 kInsertLine    = IM_COL32(255, 255, 255, 200);
static const ImU32 kInsertGlow    = IM_COL32(255, 255, 255, 50);
static const ImU32 kThumbBg       = UITokens::kSurfaceRaised;
static const ImU32 kDragShadow    = IM_COL32(0, 0, 0, 120);

// Mirror of Application.cpp's shaderDisplayName: strip trailing ".fs" and
// uppercase. Used to present shader layer names in the panel's editorial
// all-caps style without mutating the underlying layer->name (which keeps
// its original filename/mixed-case form for project files and elsewhere).
static std::string lpShaderDisplayName(const std::string& s) {
    std::string out = s;
    if (out.size() >= 3) {
        std::string tail = out.substr(out.size() - 3);
        for (auto& ch : tail) ch = (char)tolower((unsigned char)ch);
        if (tail == ".fs") out.erase(out.size() - 3);
    }
    for (auto& ch : out) ch = (char)toupper((unsigned char)ch);
    return out;
}

// ─── Layout constants ───────────────────────────────────────────────────
//
//   Monochrome redesign (2026-09-27). The Layers panel is the app's left
//   column: a "LAYERS" header row with a small "+ Add" on the right, then
//   52px rows on a 56px pitch. Each row = 36px rounded thumbnail · name +
//   type caption · [eye] [S] [lock] opacity% · zone dots. Everything is a
//   UITokens grey or white-at-alpha; selection = the brightest thing.
//
namespace LP {
    static constexpr float kRowPitch    = 56.0f;  // 52px row + 4px gap
    static constexpr float kRowGap      = 4.0f;
    static constexpr float kRowR        = UITokens::kRadiusControl;  // 6px
    static constexpr float kPadX        = 10.0f;  // inner horizontal padding
    static constexpr float kThumbW      = 36.0f;
    static constexpr float kThumbR      = UITokens::kRadiusControl;
    static constexpr float kThumbToText = 10.0f;
    static constexpr float kCtlW        = 18.0f;  // eye / S / lock column width
    static constexpr float kCtlGap      = 4.0f;
    static constexpr float kCtlToPct    = 8.0f;
    static constexpr float kPctToDots   = 10.0f;
    static constexpr float kZoneDotD    = 5.0f;   // zone dot diameter
    static constexpr float kZoneGap     = 9.0f;   // centre-to-centre
    static constexpr float kCaptionSize = 11.0f;  // type caption, %, header
    static constexpr float kNameToCtl   = 10.0f;
    static constexpr float kHeaderH     = 32.0f;
}

// Lock is a panel-side visual toggle: Layer has no lock field and adding
// one would ripple through serialization + every Layer TU, so the state
// lives here, keyed by the layer's stable id. It affects nothing but the
// glyph.
static std::map<uint32_t, bool> s_lockedLayers;
static bool lpIsLocked(uint32_t id) {
    auto it = s_lockedLayers.find(id);
    return it != s_lockedLayers.end() && it->second;
}

// Per-row geometry, shared by the drawer and the hit-testing in render()
// so the glyphs and their InvisibleButtons can never drift apart.
struct RowGeom {
    ImVec2 rowMin, rowMax;      // the 52px card rect
    float  cy;                  // vertical centre
    ImVec2 thumbMin, thumbMax;  // 36x36
    float  nameX, nameRight;    // name/caption clip span
    float  eyeCx, soloCx, lockCx;
    float  pctLeft, pctRight;   // opacity % box (right-aligned text)
    float  dotsRight;           // right edge of the zone-dot cluster
    int    zoneCount;
};

static RowGeom computeRowGeom(float x, float y, float width, int zoneCount) {
    RowGeom g;
    g.rowMin = ImVec2(x, y + LP::kRowGap * 0.5f);
    g.rowMax = ImVec2(x + width, y + LP::kRowPitch - LP::kRowGap * 0.5f);
    g.cy     = (g.rowMin.y + g.rowMax.y) * 0.5f;
    g.thumbMin = ImVec2(g.rowMin.x + LP::kPadX, g.cy - LP::kThumbW * 0.5f);
    g.thumbMax = ImVec2(g.thumbMin.x + LP::kThumbW, g.thumbMin.y + LP::kThumbW);
    g.nameX    = g.thumbMax.x + LP::kThumbToText;
    g.zoneCount = zoneCount;

    // Walk leftward from the inner right edge: dots → % → lock → S → eye.
    float rx = g.rowMax.x - LP::kPadX;
    g.dotsRight = rx;
    if (zoneCount > 0) {
        rx -= LP::kZoneDotD + (float)(zoneCount - 1) * LP::kZoneGap;
        rx -= LP::kPctToDots;
    }
    g.pctRight = rx;
    // Reserve the width of "100%" so the cluster never jitters as the
    // value changes.
    float pctW = ImGui::GetFont()->CalcTextSizeA(LP::kCaptionSize, FLT_MAX, 0.0f, "100%").x;
    g.pctLeft  = g.pctRight - pctW;
    rx = g.pctLeft - LP::kCtlToPct;
    g.lockCx = rx - LP::kCtlW * 0.5f;  rx -= LP::kCtlW + LP::kCtlGap;
    g.soloCx = rx - LP::kCtlW * 0.5f;  rx -= LP::kCtlW + LP::kCtlGap;
    g.eyeCx  = rx - LP::kCtlW * 0.5f;  rx -= LP::kCtlW;
    g.nameRight = std::max(g.nameX + 40.0f, rx - LP::kNameToCtl);
    return g;
}

// Letter-spaced text (ImGui has no tracking): draw glyph by glyph.
static void lpTrackedText(ImDrawList* dl, ImFont* font, float size, ImVec2 pos,
                          ImU32 col, const char* text, float tracking) {
    float x = pos.x;
    char buf[2] = {0, 0};
    for (const char* p = text; *p; ++p) {
        buf[0] = *p;
        dl->AddText(font, size, ImVec2(x, pos.y), col, buf);
        x += font->CalcTextSizeA(size, FLT_MAX, 0.0f, buf).x + tracking;
    }
}

// Simple padlock: rounded body + shackle arc. Locked = filled body, closed
// shackle. Unlocked = outlined body, shackle lifted and open on the right.
static void drawPadlock(ImDrawList* dl, float cx, float cy, bool locked, ImU32 col) {
    const float bw = 10.0f, bh = 7.5f;
    ImVec2 bMin(cx - bw * 0.5f, cy - 0.5f);
    ImVec2 bMax(cx + bw * 0.5f, cy - 0.5f + bh);
    if (locked) dl->AddRectFilled(bMin, bMax, col, 2.0f);
    else        dl->AddRect(bMin, bMax, col, 2.0f, 0, 1.4f);

    const float sr = 3.2f;   // shackle radius
    if (locked) {
        // Upper semicircle (ImGui angles: 0 = +x, increasing clockwise on
        // screen, so PI..2PI sweeps over the top).
        dl->PathArcTo(ImVec2(cx, bMin.y), sr, 3.14159265f, 6.28318531f, 12);
        dl->PathStroke(col, 0, 1.4f);
    } else {
        float sy = bMin.y - 2.5f;
        dl->PathArcTo(ImVec2(cx, sy), sr, 3.14159265f, 6.28318531f, 12);
        dl->PathStroke(col, 0, 1.4f);
        // Left leg reaches the body; right leg hangs open.
        dl->AddLine(ImVec2(cx - sr, sy), ImVec2(cx - sr, bMin.y), col, 1.4f);
    }
}

// "S" solo toggle. Soloed = filled white disc with dark S; otherwise a
// tertiary S (hairline ring on hover).
static void drawSoloBadge(ImDrawList* dl, ImFont* font, float cx, float cy,
                          bool on, bool hovered) {
    const float r = 8.0f;
    if (on) {
        dl->AddCircleFilled(ImVec2(cx, cy), r, UITokens::kAccent, 24);
    } else if (hovered) {
        dl->AddCircle(ImVec2(cx, cy), r, UITokens::kHairline, 24, 1.0f);
    }
    ImVec2 ts = font->CalcTextSizeA(LP::kCaptionSize, FLT_MAX, 0.0f, "S");
    ImU32 tc = on      ? UITokens::kNavSurface
             : hovered ? UITokens::kTextPrimary
                       : UITokens::kTextTertiary;
    dl->AddText(font, LP::kCaptionSize,
                ImVec2(cx - ts.x * 0.5f, cy - ts.y * 0.5f), tc, "S");
}

// Draw a single layer row using precomputed geometry. Selected = 30-alpha
// white fill + 2px white bar on the left edge; hovered = 12-alpha fill.
// `mouse` is used only for per-control hover tinting.
static void drawLayerRow(ImDrawList* draw, const std::shared_ptr<Layer>& layer,
                         const RowGeom& g, bool selected, bool hovered,
                         ImVec2 mouse) {
    ImFont* font = ImGui::GetFont();
    bool effectivelyVisible = layer->visible && !layer->userHidden;
    bool locked = lpIsLocked(layer->id);

    // ── Row fill
    if (selected) {
        draw->AddRectFilled(g.rowMin, g.rowMax, kRowSelFill, LP::kRowR);
        // 2px accent bar, inset vertically so it stays inside the rounding.
        draw->AddRectFilled(ImVec2(g.rowMin.x, g.rowMin.y + 8.0f),
                            ImVec2(g.rowMin.x + 2.0f, g.rowMax.y - 8.0f),
                            UITokens::kAccent, 1.0f);
    } else if (hovered) {
        draw->AddRectFilled(g.rowMin, g.rowMax, kRowHoverFill, LP::kRowR);
    }

    // ── Thumbnail (36x36, 6px radius)
    draw->AddRectFilled(g.thumbMin, g.thumbMax, kThumbBg, LP::kThumbR);
    GLuint texId = layer->source ? layer->source->textureId() : 0;
    if (texId != 0) {
        draw->AddImageRounded((ImTextureID)(intptr_t)texId, g.thumbMin, g.thumbMax,
                              ImVec2(0, 1), ImVec2(1, 0),
                              IM_COL32(255, 255, 255, effectivelyVisible ? 255 : 80),
                              LP::kThumbR);
    }
    draw->AddRect(g.thumbMin, g.thumbMax, UITokens::kHairline, LP::kThumbR, 0, 1.0f);

    // ── Name + type caption
    const float nameSize = ImGui::GetFontSize();
    const float gapY     = 3.0f;
    const float pairH    = nameSize + gapY + LP::kCaptionSize;
    const float nameY    = g.cy - pairH * 0.5f;
    const float capY     = nameY + nameSize + gapY;

    ImU32 nameCol = effectivelyVisible ? kTextPrimary : kTextSecondary;
    draw->PushClipRect(ImVec2(g.nameX, g.rowMin.y), ImVec2(g.nameRight, g.rowMax.y), true);
    // Shader layers display in the editorial form (".fs" stripped, caps)
    // while layer->name stays untouched for project files.
    std::string displayName = layer->name;
    if (layer->source && layer->source->typeName() == "Shader") {
        displayName = lpShaderDisplayName(displayName);
    }
    draw->AddText(font, nameSize, ImVec2(g.nameX, nameY), nameCol, displayName.c_str());
    if (layer->source) {
        std::string type = layer->source->typeName();
        if (type == "WindowCapture") type = "Window";
        if (layer->muted) type += "  Muted";
        draw->AddText(font, LP::kCaptionSize, ImVec2(g.nameX, capY),
                      kTextTertiary, type.c_str());
    }
    draw->PopClipRect();

    // ── Controls: eye · S · lock
    auto hot = [&](float cx) {
        return hovered && std::fabs(mouse.x - cx) <= LP::kCtlW * 0.5f + 2.0f
                       && mouse.y >= g.rowMin.y && mouse.y < g.rowMax.y;
    };
    {
        bool h = hot(g.eyeCx);
        ImU32 c = effectivelyVisible ? (h ? kTextPrimary  : kTextSecondary)
                                     : (h ? kTextSecondary : kTextTertiary);
        if (effectivelyVisible) lucide::eye   (draw, g.eyeCx, g.cy, 15.0f, c, 1.4f);
        else                    lucide::eyeOff(draw, g.eyeCx, g.cy, 15.0f, c, 1.4f);
    }
    drawSoloBadge(draw, font, g.soloCx, g.cy, layer->soloed, hot(g.soloCx));
    {
        ImU32 c = locked ? kTextPrimary : (hot(g.lockCx) ? kTextSecondary : kTextTertiary);
        drawPadlock(draw, g.lockCx, g.cy, locked, c);
    }

    // ── Opacity % — right-aligned in its reserved box. Draggable via the
    //    InvisibleButton in render().
    char opBuf[8];
    snprintf(opBuf, sizeof(opBuf), "%d%%", (int)(layer->opacity * 100.0f + 0.5f));
    ImVec2 opSz = font->CalcTextSizeA(LP::kCaptionSize, FLT_MAX, 0.0f, opBuf);
    draw->AddText(font, LP::kCaptionSize,
                  ImVec2(g.pctRight - opSz.x, g.cy - opSz.y * 0.5f),
                  kTextSecondary, opBuf);
}

// Zone dot fill — a short greyscale ramp so neighbouring zones still read
// as distinct dots without introducing any hue.
static ImU32 zoneColor(int idx) {
    static const ImU32 kRamp[] = {
        IM_COL32(245, 246, 248, 235),
        IM_COL32(212, 214, 218, 235),
        IM_COL32(180, 183, 189, 235),
        IM_COL32(150, 154, 161, 235),
    };
    return kRamp[idx % (int)(sizeof(kRamp) / sizeof(kRamp[0]))];
}

void LayerPanel::render(LayerStack& stack, int& selectedLayer,
                        std::vector<std::unique_ptr<OutputZone>>* zones,
                        int activeZone) {
    ImGui::SetNextWindowSizeConstraints(ImVec2(250, 150), ImVec2(FLT_MAX, FLT_MAX));
    // 8-space pad + ###ID suffix matches Mapping/Properties/Audio/MIDI:
    // the empty space gives drawInspectorTabIcons() room to paint the
    // Layers icon over the tab (otherwise the text would show through).
    // Body bg = kNavSurface so the panel matches the host + top nav band.
    ImGui::PushStyleColor(ImGuiCol_WindowBg, IM_COL32(0, 0, 0, 0));  // host paints the card
    ImGui::Begin("        ###Layers", nullptr, ImGuiWindowFlags_NoCollapse);
    ImGui::PopStyleColor();
    // 6-pill nav is rendered at the right-dock host level (one bar total).
    // 1px outline only when floating — when docked, dock-node edges already
    // separate the panel from its neighbours and a window border just adds
    // a visual seam.
    if (!ImGui::IsWindowDocked()) {
        ImVec2 mn = ImGui::GetWindowPos();
        ImVec2 mx(mn.x + ImGui::GetWindowSize().x,
                  mn.y + ImGui::GetWindowSize().y);
        ImGui::GetWindowDrawList()->AddRect(
            mn, mx, IM_COL32(255, 255, 255, 50),
            ImGui::GetStyle().WindowRounding, 0, 1.0f);
    }

    wantsAddImage = wantsAddVideo = wantsAddShader = false;
    removedLayerIds.clear();

    ImDrawList* draw = ImGui::GetWindowDrawList();
    ImDrawList* fg = ImGui::GetForegroundDrawList();
    float panelWidth = ImGui::GetContentRegionAvail().x;
    if (panelWidth < 10.0f) { ImGui::End(); return; }

    // ── Header: "LAYERS" title (left) + "+ Add" (right) ─────────────────
    // The small "+ Add" opens the same ##AddLayer popup the old full-width
    // "+ Add Layer" button did.
    {
        ImFont* font = ImGui::GetFont();
        ImVec2 hp = ImGui::GetCursorScreenPos();
        ImGui::Dummy(ImVec2(panelWidth, LP::kHeaderH));
        ImVec2 afterHeader = ImGui::GetCursorScreenPos();

        // Title — 11px, uppercase, tracked.
        lpTrackedText(draw, font, LP::kCaptionSize,
                      ImVec2(hp.x + LP::kPadX,
                             hp.y + (LP::kHeaderH - LP::kCaptionSize) * 0.5f),
                      UITokens::kTextTertiary, "LAYERS", 1.6f);

        // "+ Add" — small rounded button, right-aligned.
        {
            const char* lbl = "+ Add";
            ImVec2 ls = font->CalcTextSizeA(LP::kCaptionSize, FLT_MAX, 0.0f, lbl);
            const float bh = 22.0f;
            const float bw = ls.x + 18.0f;
            ImVec2 bMin(hp.x + panelWidth - bw, hp.y + (LP::kHeaderH - bh) * 0.5f);
            ImVec2 bMax(bMin.x + bw, bMin.y + bh);
            ImGui::SetCursorScreenPos(bMin);
            ImGui::InvisibleButton("##addLayerBtn", ImVec2(bw, bh));
            bool hov = ImGui::IsItemHovered();
            bool act = ImGui::IsItemActive();
            ImU32 fill = act ? UITokens::kButtonActive
                       : hov ? UITokens::kButtonHover
                             : IM_COL32(255, 255, 255, 10);
            draw->AddRectFilled(bMin, bMax, fill, UITokens::kRadiusControl);
            draw->AddRect(bMin, bMax, UITokens::kHairline, UITokens::kRadiusControl, 0, 1.0f);
            draw->AddText(font, LP::kCaptionSize,
                          ImVec2(bMin.x + (bw - ls.x) * 0.5f, bMin.y + (bh - ls.y) * 0.5f),
                          hov ? UITokens::kTextPrimary : UITokens::kTextSecondary, lbl);
            if (hov) ImGui::SetMouseCursor(ImGuiMouseCursor_Hand);
            if (ImGui::IsItemClicked()) ImGui::OpenPopup("##AddLayer");
        }

        if (ImGui::BeginPopup("##AddLayer")) {
            if (ImGui::MenuItem("Image...")) wantsAddImage = true;
            if (ImGui::MenuItem("Video...")) wantsAddVideo = true;
            if (ImGui::MenuItem("Shader...")) wantsAddShader = true;
            ImGui::EndPopup();
        }
        ImGui::SetCursorScreenPos(afterHeader);
    }

    // Row rhythm — 52px rows on a 56px pitch (4px vertical gap).
    float rowHeight = LP::kRowPitch;
    int layerCount = stack.count();

    // Empty state
    if (layerCount == 0) {
        ImGui::Dummy(ImVec2(0, 30));
        float tw = ImGui::CalcTextSize("No layers").x;
        ImGui::SetCursorPosX((panelWidth - tw) * 0.5f);
        ImGui::TextDisabled("No layers");
        ImGui::Dummy(ImVec2(0, 6));
        float hw = ImGui::CalcTextSize("Drop files or click +").x;
        ImGui::SetCursorPosX((panelWidth - hw) * 0.5f);
        ImGui::TextDisabled("Drop files or click +");
        ImGui::End();
        return;
    }

    ImVec2 mousePos = ImGui::GetMousePos();
    ImVec2 listStart = ImGui::GetCursorScreenPos();
    bool windowHovered = ImGui::IsWindowHovered(ImGuiHoveredFlags_AllowWhenBlockedByActiveItem);
    // TEMP DIAG: print on ANY left double-click while the LayerPanel
    // render fn is alive, regardless of gates. If this never fires while
    // you double-click a layer thumb, the event is being eaten upstream
    // — likely by InvisibleButton items in drawLayerRow (eye/lock/op-scrub)
    // consuming the press, so ImGui flags it as "owned" and IsMouseDouble-
    // Clicked never reports it at the panel scope.
    if (ImGui::IsMouseDoubleClicked(ImGuiMouseButton_Left)) {
        fprintf(stderr, "[LayerPanel] L double-click DETECTED  "
                        "mouse=(%.0f,%.0f) listStart=(%.0f,%.0f) "
                        "winHov=%d wantCap=%d anyActive=%d activeID=%u\n",
                mousePos.x, mousePos.y, listStart.x, listStart.y,
                (int)windowHovered, (int)ImGui::GetIO().WantCaptureMouse,
                (int)ImGui::IsAnyItemActive(),
                (unsigned)ImGui::GetActiveID());
    }

    // Reserve space for all rows so scrolling works
    ImGui::Dummy(ImVec2(panelWidth, rowHeight * layerCount));

    // --- MOUSE INTERACTION ---

    // Detect mouse-down on a row to start potential drag.
    // NOTE: the opacity-scrub drag is NOT handled here — it's driven by the
    // ImGui::InvisibleButton over each row's % text region, which claims
    // the mouse press itself so the window-drag gesture never triggers.
    // We only run this block if no opacity drag is in progress.
    if (m_opacityDragIdx < 0
        && ImGui::IsMouseClicked(ImGuiMouseButton_Left)
        && windowHovered && !m_renaming)
    {
        float relY = mousePos.y - listStart.y;
        int displayIdx = (int)(relY / rowHeight);
        if (displayIdx >= 0 && displayIdx < layerCount && relY >= 0) {
            int stackIdx = layerCount - 1 - displayIdx;

            // A single left click anywhere on the row — INCLUDING the
            // thumbnail — selects the layer and arms a potential reorder
            // drag. Visibility toggling moved entirely to (a) the dedicated
            // eye icon and (b) the double-click handler above, because the
            // old "single-click thumb = toggle visibility" path was
            // silently fighting the double-click: click-1 hid, click-2
            // showed again, net = no change, and double-click "didn't
            // work." Opacity scrub is still handled by its own
            // InvisibleButton below, not here.
            selectedLayer = stackIdx;
            m_dragIndex   = stackIdx;
            m_dragStartY  = mousePos.y;
            m_dragOffsetY = mousePos.y - (listStart.y + displayIdx * rowHeight);
            m_dragActive  = false; // not yet — need to move first
        } else {
            // Clicked outside any layer row — deselect
            selectedLayer = -1;
        }
    }

    // Activate drag once mouse moves enough (3px threshold)
    if (m_dragIndex >= 0 && !m_dragActive && ImGui::IsMouseDown(ImGuiMouseButton_Left)) {
        if (fabsf(mousePos.y - m_dragStartY) > 3.0f) {
            m_dragActive = true;
        }
    }

    // (Opacity scrub is driven by IsItemActivated / IsItemActive on the
    // per-row InvisibleButton below, not here. ImGui claims the mouse
    // press so the window never starts a drag.)

    // Track insertion position during drag
    if (m_dragActive) {
        float relY = mousePos.y - listStart.y;
        int hoverDisplay = (int)(relY / rowHeight);
        // Snap to nearest edge
        float frac = (relY / rowHeight) - hoverDisplay;
        if (frac > 0.5f) hoverDisplay++;
        m_insertIndex = std::max(0, std::min(hoverDisplay, layerCount));
    }

    // Drop on release
    if (ImGui::IsMouseReleased(ImGuiMouseButton_Left)) {
        if (m_dragActive && m_dragIndex >= 0) {
            int dragDisplay = layerCount - 1 - m_dragIndex;
            int insertDisplay = m_insertIndex;

            // Convert display insert position to stack index
            // display 0 = stack top (layerCount-1), display N = stack bottom (0)
            // Insert at display position D means the layer goes to stack index (layerCount - D)
            // but we need to account for the removal of the dragged layer
            if (insertDisplay != dragDisplay && insertDisplay != dragDisplay + 1) {
                int toStack;
                if (insertDisplay < dragDisplay) {
                    // Moving up in display = moving to higher stack index
                    toStack = layerCount - 1 - insertDisplay;
                } else {
                    // Moving down in display = moving to lower stack index
                    toStack = layerCount - insertDisplay;
                }
                toStack = std::max(0, std::min(toStack, layerCount - 1));
                if (toStack != m_dragIndex) {
                    stack.moveLayer(m_dragIndex, toStack);
                    selectedLayer = toStack;
                }
            }
        }
        m_dragActive = false;
        m_dragIndex = -1;
        m_insertIndex = -1;
    }

    // Double-click ANYWHERE on a layer row → toggle visibility. Previous
    // version had an `x >= listStart.x + 26` gate (intended to skip an
    // "icon column" on the left), but the eye icon actually lives on the
    // RIGHT side of the row (see eyeX = pctX - kEyeGap - kEyeW, computed
    // in drawLayerRow). The gate was protecting nothing and silently
    // blocked the LEFT half of the 48px thumbnail — which is exactly
    // where most users aim. Now the whole row body is hot.
    //
    // Eye-button overlap: clicking on the eye fires its own toggle via
    // IsItemActivated on the InvisibleButton. A *double*-click on the
    // eye therefore toggles twice via the eye + once via this handler =
    // an odd number of flips = visible state changes by one — same as
    // a single click. So no inconsistency.
    if (ImGui::IsMouseDoubleClicked(ImGuiMouseButton_Left) && windowHovered && !m_dragActive) {
        float relY = mousePos.y - listStart.y;
        int displayIdx = (int)(relY / rowHeight);
        // TEMP DIAG (remove once confirmed working): tells us whether the
        // event is even reaching this handler. If you double-click and
        // see nothing in /tmp/easel_audio.log, the event is being eaten
        // upstream — possibly by another window or an InvisibleButton.
        fprintf(stderr, "[LayerDblClick] mouseX=%.0f mouseY=%.0f listStartX=%.0f "
                        "relY=%.0f rowHeight=%.0f displayIdx=%d layerCount=%d "
                        "windowHovered=%d dragActive=%d\n",
                mousePos.x, mousePos.y, listStart.x, relY, rowHeight,
                displayIdx, layerCount, (int)windowHovered, (int)m_dragActive);
        if (displayIdx >= 0 && displayIdx < layerCount) {
            int stackIdx = layerCount - 1 - displayIdx;
            auto& L = stack[stackIdx];
            L->userHidden = !L->userHidden;
            L->visible    = !L->userHidden;
            fprintf(stderr, "[LayerDblClick]   → toggled layer %d (\"%s\") to "
                            "userHidden=%d visible=%d\n",
                    stackIdx, L->name.c_str(),
                    (int)L->userHidden, (int)L->visible);
        }
    }

    // Right-click context menu
    if (ImGui::IsMouseClicked(ImGuiMouseButton_Right) && windowHovered) {
        float relY = mousePos.y - listStart.y;
        int displayIdx = (int)(relY / rowHeight);
        if (displayIdx >= 0 && displayIdx < layerCount) {
            int stackIdx = layerCount - 1 - displayIdx;
            selectedLayer = stackIdx;
            ImGui::OpenPopup("LayerMenu");
        }
    }

    // --- DRAW ROWS ---

    int dragDisplay = (m_dragIndex >= 0) ? (layerCount - 1 - m_dragIndex) : -1;
    int zoneCount = (zones && zones->size() > 1) ? (int)zones->size() : 0;

    for (int displayIdx = 0; displayIdx < layerCount; displayIdx++) {
        int stackIdx = layerCount - 1 - displayIdx;
        auto& layer = stack[stackIdx];
        bool selected = (stackIdx == selectedLayer);

        float rowY = listStart.y + displayIdx * rowHeight;
        RowGeom g = computeRowGeom(listStart.x, rowY, panelWidth, zoneCount);

        // Skip drawing the dragged row in-place (we draw it floating)
        if (m_dragActive && displayIdx == dragDisplay) {
            // Dim placeholder — a faint outlined slot where the row came from
            draw->AddRectFilled(g.rowMin, g.rowMax, IM_COL32(255, 255, 255, 6), LP::kRowR);
            draw->AddRect(g.rowMin, g.rowMax, UITokens::kHairlineSoft, LP::kRowR, 0, 1.0f);
            continue;
        }

        // Hover (only when not dragging) — the whole pitch band is hot so
        // the 4px gap between rows never flickers the hover off.
        bool rowHovered = !m_dragActive &&
                          mousePos.x >= listStart.x && mousePos.x < listStart.x + panelWidth &&
                          mousePos.y >= rowY && mousePos.y < rowY + rowHeight && windowHovered;

        drawLayerRow(draw, layer, g, selected, rowHovered, mousePos);

        // Control hit boxes — eye / S (solo) / lock. Each is an
        // InvisibleButton spanning the full row height so the user can hit
        // the glyph anywhere in its column. IsItemActivated fires on press,
        // more reliable than IsItemClicked which needs hover at press AND
        // release. Because ImGui owns the press, the panel never treats
        // these clicks as a window-drag gesture.
        auto ctlButton = [&](const char* id, float cx, uint32_t salt) -> bool {
            ImGui::SetCursorScreenPos(ImVec2(cx - LP::kCtlW * 0.5f - 2.0f, g.rowMin.y));
            ImGui::PushID((int)(layer->id + salt));
            ImGui::InvisibleButton(id, ImVec2(LP::kCtlW + 4.0f, g.rowMax.y - g.rowMin.y));
            bool fired = ImGui::IsItemActivated();
            if (ImGui::IsItemHovered()) ImGui::SetMouseCursor(ImGuiMouseCursor_Hand);
            ImGui::PopID();
            return fired;
        };
        // Eye — visibility toggle (same semantics as the double-click path).
        if (ctlButton("##eyeBtn", g.eyeCx, 0xB0000000u)) {
            layer->userHidden = !layer->userHidden;
            layer->visible    = !layer->userHidden;
        }
        // S — solo. Same action as the context menu's Solo / Unsolo item.
        if (ctlButton("##soloBtn", g.soloCx, 0xC0000000u)) {
            layer->soloed = !layer->soloed;
        }
        // Lock — panel-side visual toggle (see s_lockedLayers).
        if (ctlButton("##lockBtn", g.lockCx, 0xD0000000u)) {
            s_lockedLayers[layer->id] = !lpIsLocked(layer->id);
        }

        // Opacity scrub — per-row InvisibleButton over the % text. Because
        // ImGui's item system owns the press, ImGui NEVER treats this
        // click as a "window drag" gesture on the Layers panel. We drive
        // the scrub off IsItemActivated / IsItemActive + GetMouseDragDelta.
        {
            float hitH = 20.0f;
            float hitY = g.cy - hitH * 0.5f;
            float hitX = g.pctLeft - 6.0f;
            float hitW = (g.pctRight + 6.0f) - hitX;
            ImGui::SetCursorScreenPos(ImVec2(hitX, hitY));
            ImGui::PushID((int)(layer->id + 0xA0000000));
            ImGui::InvisibleButton("##opDrag", ImVec2(hitW, hitH));
            if (ImGui::IsItemActivated()) {
                m_opacityDragIdx    = stackIdx;
                m_opacityDragStart  = layer->opacity;
            }
            if (ImGui::IsItemActive()) {
                ImVec2 d = ImGui::GetMouseDragDelta(ImGuiMouseButton_Left, 0.0f);
                float newOp = m_opacityDragStart + d.x * 0.005f;
                if (newOp < 0.0f) newOp = 0.0f;
                if (newOp > 1.0f) newOp = 1.0f;
                layer->opacity = newOp;
                ImGui::SetMouseCursor(ImGuiMouseCursor_ResizeEW);
                selectedLayer = stackIdx;
            } else if (ImGui::IsItemDeactivated()) {
                m_opacityDragIdx = -1;
            } else if (ImGui::IsItemHovered()) {
                ImGui::SetMouseCursor(ImGuiMouseCursor_ResizeEW);
            }
            ImGui::PopID();
        }

        // Zone visibility dots — far right, after the opacity %, 5px,
        // greyscale. Filled = layer shows in that zone; outline = hidden
        // there. Click a dot to toggle.
        if (zoneCount > 0) {
            const float r = LP::kZoneDotD * 0.5f;
            float dotX = g.dotsRight - r;
            for (int zi = zoneCount - 1; zi >= 0; zi--) {
                auto& z = *(*zones)[zi];
                bool inZone = z.showAllLayers || z.visibleLayerIds.count(layer->id);
                ImVec2 center(dotX, g.cy);
                if (inZone) draw->AddCircleFilled(center, r, zoneColor(zi), 12);
                else        draw->AddCircle     (center, r, IM_COL32(255, 255, 255, 70), 12, 1.0f);

                if (!m_dragActive && ImGui::IsMouseClicked(ImGuiMouseButton_Left) && windowHovered) {
                    float dx = mousePos.x - center.x, dy = mousePos.y - center.y;
                    if (dx*dx + dy*dy < (r + 4) * (r + 4)) {
                        if (z.showAllLayers) {
                            z.showAllLayers = false;
                            for (int li = 0; li < stack.count(); li++) {
                                z.visibleLayerIds.insert(stack[li]->id);
                            }
                            z.visibleLayerIds.erase(layer->id);
                        } else {
                            if (inZone) z.visibleLayerIds.erase(layer->id);
                            else        z.visibleLayerIds.insert(layer->id);
                        }
                    }
                }
                dotX -= LP::kZoneGap;
            }
        }
    }

    // --- DRAW INSERTION LINE ---
    if (m_dragActive && m_insertIndex >= 0) {
        float lineY = listStart.y + m_insertIndex * rowHeight;
        float lx0 = listStart.x + 4;
        float lx1 = listStart.x + panelWidth - 4;
        fg->AddLine(ImVec2(lx0, lineY), ImVec2(lx1, lineY), kInsertGlow, 6.0f);
        fg->AddLine(ImVec2(lx0, lineY), ImVec2(lx1, lineY), kInsertLine, 2.0f);
        fg->AddCircleFilled(ImVec2(lx0, lineY), 4.0f, kInsertLine);
        fg->AddCircleFilled(ImVec2(lx1, lineY), 4.0f, kInsertLine);
    }

    // --- DRAW FLOATING DRAG ROW ---
    if (m_dragActive && m_dragIndex >= 0 && m_dragIndex < layerCount) {
        float floatY = mousePos.y - m_dragOffsetY;
        auto& dragLayer = stack[m_dragIndex];
        RowGeom fgG = computeRowGeom(listStart.x, floatY, panelWidth, 0);

        // Shadow
        fg->AddRectFilled(ImVec2(fgG.rowMin.x + 2, fgG.rowMin.y + 2),
                          ImVec2(fgG.rowMax.x + 2, fgG.rowMax.y + 2),
                          kDragShadow, LP::kRowR);
        // Background — raised surface so the row reads as lifted off the list
        fg->AddRectFilled(fgG.rowMin, fgG.rowMax, UITokens::kSurfaceRaised, LP::kRowR);
        // Border glow
        fg->AddRect(fgG.rowMin, fgG.rowMax, IM_COL32(255, 255, 255, 80), LP::kRowR, 0, 1.5f);

        drawLayerRow(fg, dragLayer, fgG, true, false, ImVec2(-1e9f, -1e9f));
    }

    // --- INLINE RENAME ---
    if (m_renaming && m_renameIndex >= 0 && m_renameIndex < layerCount) {
        int rDisplay = layerCount - 1 - m_renameIndex;
        float rY = listStart.y + rDisplay * rowHeight
                 + (rowHeight - ImGui::GetFrameHeight()) * 0.5f;
        // Align the rename InputText with the name column from drawLayerRow
        // (row padding + thumbnail + gap).
        float nameX = listStart.x + LP::kPadX + LP::kThumbW + LP::kThumbToText;
        float inputW = panelWidth - (nameX - listStart.x) - 10;
        if (inputW < 40.0f) inputW = 40.0f;

        ImGui::SetCursorScreenPos(ImVec2(nameX, rY));
        ImGui::SetNextItemWidth(inputW);
        // On the first frame of rename, auto-focus the input so typing works
        // immediately without clicking it first.
        if (m_renameJustStarted) {
            ImGui::SetKeyboardFocusHere();
            m_renameJustStarted = false;
        }
        ImGui::PushStyleColor(ImGuiCol_FrameBg,
                              ImGui::ColorConvertU32ToFloat4(UITokens::kSurfaceRaised));
        ImGui::PushStyleColor(ImGuiCol_FrameBgActive,
                              ImGui::ColorConvertU32ToFloat4(UITokens::kSurfaceHover));
        bool committed = ImGui::InputText("##rename", m_renameBuf, sizeof(m_renameBuf),
                             ImGuiInputTextFlags_EnterReturnsTrue | ImGuiInputTextFlags_AutoSelectAll);
        // Commit on click-away: IsItemDeactivatedAfterEdit fires when the
        // input loses focus after the user typed something. Also close (without
        // changes) on plain deactivation so the field doesn't stick around.
        bool deactivatedAfterEdit = ImGui::IsItemDeactivatedAfterEdit();
        bool deactivated          = ImGui::IsItemDeactivated();
        ImGui::PopStyleColor(2);
        if (committed || deactivatedAfterEdit) {
            stack[m_renameIndex]->name = m_renameBuf;
            m_renaming = false;
            m_renameIndex = -1;
        } else if (deactivated || ImGui::IsKeyPressed(ImGuiKey_Escape)) {
            m_renaming = false;
            m_renameIndex = -1;
        }
    }

    // --- GROUP HEADERS ---
    // Draw group headers above their first member (visual only, lightweight)
    {
        std::set<uint32_t> drawnGroups;
        for (int displayIdx = 0; displayIdx < layerCount; displayIdx++) {
            int stackIdx = layerCount - 1 - displayIdx;
            auto& layer = stack[stackIdx];
            if (layer->groupId == 0) continue;
            if (drawnGroups.count(layer->groupId)) continue;
            drawnGroups.insert(layer->groupId);

            auto* grp = stack.group(layer->groupId);
            if (!grp) continue;

            float rowY = listStart.y + displayIdx * rowHeight;
            // Draw a subtle group bar above the first grouped layer
            float barY = rowY - 2;
            ImU32 grpCol = IM_COL32(255, 255, 255, 100);
            draw->AddRectFilled(ImVec2(listStart.x, barY - 14),
                                ImVec2(listStart.x + panelWidth, barY),
                                IM_COL32(255, 255, 255, 20), 2.0f);
            draw->AddLine(ImVec2(listStart.x + 4, barY), ImVec2(listStart.x + panelWidth - 4, barY), grpCol, 1.0f);

            // Group name
            ImVec2 textPos(listStart.x + 8, barY - 13);
            draw->AddText(textPos, grpCol, grp->name.c_str());

            // Count members
            int memberCount = 0;
            for (int i = 0; i < layerCount; i++) {
                if (stack[i]->groupId == layer->groupId) memberCount++;
            }
            char countBuf[16];
            snprintf(countBuf, sizeof(countBuf), "(%d)", memberCount);
            ImVec2 countSize = ImGui::CalcTextSize(countBuf);
            ImVec2 nameSize = ImGui::CalcTextSize(grp->name.c_str());
            draw->AddText(ImVec2(textPos.x + nameSize.x + 6, barY - 13), kTextMuted, countBuf);
        }
    }

    // Draw group bracket on left edge for grouped layers
    {
        for (int displayIdx = 0; displayIdx < layerCount; displayIdx++) {
            int stackIdx = layerCount - 1 - displayIdx;
            auto& layer = stack[stackIdx];
            if (layer->groupId == 0) continue;

            float rowY = listStart.y + displayIdx * rowHeight;
            // Left accent line for group membership
            draw->AddRectFilled(ImVec2(listStart.x, rowY),
                                ImVec2(listStart.x + 2, rowY + rowHeight),
                                IM_COL32(255, 255, 255, 60));
        }
    }

    // --- CONTEXT MENU ---
    if (ImGui::BeginPopup("LayerMenu")) {
        int ci = selectedLayer;
        if (ci >= 0 && ci < layerCount) {
            if (ImGui::MenuItem("Duplicate", "Ctrl+D")) {
                auto& orig = stack[ci];
                auto dupe = std::make_shared<Layer>();
                dupe->name = orig->name + " copy";
                dupe->visible = orig->visible;
                dupe->opacity = orig->opacity;
                dupe->blendMode = orig->blendMode;
                dupe->position = orig->position;
                dupe->scale = orig->scale;
                dupe->rotation = orig->rotation;
                dupe->flipH = orig->flipH;
                dupe->flipV = orig->flipV;
                dupe->tileX = orig->tileX;
                dupe->tileY = orig->tileY;
                dupe->cropTop = orig->cropTop;
                dupe->cropBottom = orig->cropBottom;
                dupe->cropLeft = orig->cropLeft;
                dupe->cropRight = orig->cropRight;
                dupe->source = orig->source;
                // A 3D Fluid gets its OWN simulation (same settings) so the
                // copy can be colored, blended and driven independently —
                // sharing the source would just draw the same sim twice.
                if (orig->source && orig->source->typeName() == "Fluid3D") {
                    auto* of = static_cast<FluidSource3D*>(orig->source.get());
                    auto nf = std::make_shared<FluidSource3D>();
                    nf->copyConfigFrom(*of);
                    if (nf->init(of->zoneWidth(), of->zoneHeight())) dupe->source = nf;
                }
                stack.insertLayer(ci + 1, dupe);
                selectedLayer = ci + 1;
            }
            if (ImGui::MenuItem("Rename")) {
                m_renaming = true;
                m_renameJustStarted = true;
                m_renameIndex = ci;
                strncpy(m_renameBuf, stack[ci]->name.c_str(), sizeof(m_renameBuf) - 1);
            }
            // Per-zone visibility — checkbox per zone for THIS layer. The
            // popup draws above the dock layout, so this works even when
            // the panel is collapsed/occluded down to the thumbnail rail.
            if (zones && zones->size() > 1) {
                ImGui::Separator();
                if (ImGui::BeginMenu("Visible in Zones")) {
                    auto& L = stack[ci];
                    for (int zi = 0; zi < (int)zones->size(); zi++) {
                        auto& z = *(*zones)[zi];
                        bool inZone = z.showAllLayers ||
                                      z.visibleLayerIds.count(L->id);
                        if (ImGui::MenuItem(z.name.c_str(), nullptr, inZone)) {
                            if (z.showAllLayers) {
                                // Freeze the implicit "all layers" set, then
                                // toggle this one off.
                                z.showAllLayers = false;
                                for (int li = 0; li < stack.count(); li++)
                                    z.visibleLayerIds.insert(stack[li]->id);
                                z.visibleLayerIds.erase(L->id);
                            } else if (inZone) {
                                z.visibleLayerIds.erase(L->id);
                            } else {
                                z.visibleLayerIds.insert(L->id);
                            }
                        }
                    }
                    ImGui::Separator();
                    if (ImGui::MenuItem("Show in All Zones"))
                        showLayerInAllZones(*zones, L->id);
                    if (ImGui::MenuItem("Solo Across All Zones"))
                        soloLayerAcrossZones(*zones, L->id);
                    if (ImGui::IsItemHovered())
                        ImGui::SetTooltip("One look everywhere — every zone renders\nonly this layer, so the whole house matches.");
                    ImGui::EndMenu();
                }
            }
            ImGui::Separator();
            if (ImGui::MenuItem("Move to Top", nullptr, false, ci < layerCount - 1)) {
                stack.moveLayer(ci, layerCount - 1);
                selectedLayer = layerCount - 1;
            }
            if (ImGui::MenuItem("Move Up", nullptr, false, ci < layerCount - 1)) {
                stack.moveLayer(ci, ci + 1); selectedLayer = ci + 1;
            }
            if (ImGui::MenuItem("Move Down", nullptr, false, ci > 0)) {
                stack.moveLayer(ci, ci - 1); selectedLayer = ci - 1;
            }
            if (ImGui::MenuItem("Move to Bottom", nullptr, false, ci > 0)) {
                stack.moveLayer(ci, 0); selectedLayer = 0;
            }
            ImGui::Separator();
            if (ImGui::MenuItem(stack[ci]->visible ? "Hide" : "Show")) {
                stack[ci]->visible = !stack[ci]->visible;
            }
            if (ImGui::MenuItem(stack[ci]->muted ? "Unmute" : "Mute")) {
                stack[ci]->muted = !stack[ci]->muted;
            }
            if (ImGui::MenuItem(stack[ci]->soloed ? "Unsolo" : "Solo")) {
                stack[ci]->soloed = !stack[ci]->soloed;
            }
            ImGui::Separator();
            // Group operations
            if (stack[ci]->groupId == 0) {
                if (ImGui::MenuItem("Create Group", "Ctrl+G")) {
                    uint32_t gid = stack.createGroup("Group");
                    stack[ci]->groupId = gid;
                }
            } else {
                if (ImGui::MenuItem("Ungroup")) {
                    uint32_t gid = stack[ci]->groupId;
                    stack.removeGroup(gid);
                }
                if (ImGui::BeginMenu("Add to Group")) {
                    for (auto& [gid, grp] : stack.groups()) {
                        if (ImGui::MenuItem(grp.name.c_str())) {
                            stack[ci]->groupId = gid;
                        }
                    }
                    ImGui::EndMenu();
                }
            }
            // Show available groups to join
            if (stack[ci]->groupId == 0 && !stack.groups().empty()) {
                if (ImGui::BeginMenu("Add to Group")) {
                    for (auto& [gid, grp] : stack.groups()) {
                        if (ImGui::MenuItem(grp.name.c_str())) {
                            stack[ci]->groupId = gid;
                        }
                    }
                    ImGui::EndMenu();
                }
            }
            ImGui::Separator();
            // Voice-control edit mode — toggles the Property panel's inline
            // per-parameter voice/audio source pickers for this layer.
            // (Was the PropertyPanel "···" overflow item; consolidated here.)
            if (ImGui::MenuItem("Edit voice control", nullptr,
                                stack[ci]->voiceControlEdit)) {
                stack[ci]->voiceControlEdit = !stack[ci]->voiceControlEdit;
            }
            ImGui::Separator();
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.831f, 0.839f, 0.855f, 1.0f));
            if (ImGui::MenuItem("Delete")) {
                if (ci >= 0 && ci < stack.count() && stack[ci])
                    removedLayerIds.push_back(stack[ci]->id);
                stack.removeLayer(ci);
                selectedLayer = std::min(selectedLayer, stack.count() - 1);
            }
            ImGui::PopStyleColor();
        }
        ImGui::EndPopup();
    }

    // Keyboard shortcuts
    if (!m_renaming && selectedLayer >= 0 && selectedLayer < stack.count() && !ImGui::IsAnyItemActive()) {
        if (ImGui::IsKeyPressed(ImGuiKey_Delete)) {
            if (selectedLayer >= 0 && selectedLayer < stack.count() && stack[selectedLayer])
                removedLayerIds.push_back(stack[selectedLayer]->id);
            stack.removeLayer(selectedLayer);
            selectedLayer = std::min(selectedLayer, stack.count() - 1);
        }
        if (ImGui::GetIO().KeyCtrl && ImGui::IsKeyPressed(ImGuiKey_D)) {
            int ci = selectedLayer;
            auto& orig = stack[ci];
            auto dupe = std::make_shared<Layer>();
            dupe->name = orig->name + " copy";
            dupe->visible = orig->visible;
            dupe->opacity = orig->opacity;
            dupe->blendMode = orig->blendMode;
            dupe->position = orig->position;
            dupe->scale = orig->scale;
            dupe->rotation = orig->rotation;
            dupe->flipH = orig->flipH;
            dupe->flipV = orig->flipV;
            dupe->tileX = orig->tileX;
            dupe->tileY = orig->tileY;
            dupe->cropTop = orig->cropTop;
            dupe->cropBottom = orig->cropBottom;
            dupe->cropLeft = orig->cropLeft;
            dupe->cropRight = orig->cropRight;
            dupe->source = orig->source;
            stack.insertLayer(ci + 1, dupe);
            selectedLayer = ci + 1;
        }
    }

    ImGui::End();
}
