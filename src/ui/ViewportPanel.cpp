#include "ui/ViewportPanel.h"
#include "ui/UIManager.h"
#include "ui/ParamRow.h"
#include "app/OutputZone.h"
#include "app/MappingProfile.h"
#include "app/ProjectorOutput.h"
#include "warp/CornerPinWarp.h"
#include "warp/MeshWarp.h"
#include "warp/ObjMeshWarp.h"
#include "compositing/MaskPath.h"
#include <imgui.h>
#include <imgui_internal.h>
#include <algorithm>
#include <cmath>
#include <cstring>

// Zone color palette — currently monochrome (white/gray) per request.
// The `idx` parameter is preserved so per-zone tinting can be re-enabled
// later by restoring distinct hues here without touching call sites.
struct ZoneRGB { int r, g, b; };
static const ZoneRGB kZoneColors[] = {
    {235, 238, 244},  // near-white (zone 1)
    {235, 238, 244},
    {235, 238, 244},
    {235, 238, 244},
    {235, 238, 244},
    {235, 238, 244},
    {235, 238, 244},
    {235, 238, 244},
};
static ZoneRGB zoneRGB(int idx) { return kZoneColors[idx % 8]; }

// Zone-colored accent helpers
static ImU32 zAccent(int zi)      { auto c = zoneRGB(zi); return IM_COL32(c.r, c.g, c.b, 255); }
static ImU32 zAccentDim(int zi)   { auto c = zoneRGB(zi); return IM_COL32(c.r*7/10, c.g*7/10, c.b*7/10, 255); }
static ImU32 zAccentSoft(int zi)  { auto c = zoneRGB(zi); return IM_COL32(c.r, c.g, c.b, 80); }
static ImU32 zAccentGlow(int zi)  { auto c = zoneRGB(zi); return IM_COL32(c.r, c.g, c.b, 30); }
static ImU32 zMaskFill(int zi)    { auto c = zoneRGB(zi); return IM_COL32(c.r, c.g, c.b, 30); }
static ImU32 zMaskCurve(int zi)   { auto c = zoneRGB(zi); return IM_COL32(c.r, c.g, c.b, 200); }
static ImU32 zMaskGlow(int zi)    { auto c = zoneRGB(zi); return IM_COL32(c.r, c.g, c.b, 50); }
static ImU32 zHandleRing(int zi)  { auto c = zoneRGB(zi); return IM_COL32(c.r, c.g, c.b, 200); }
static ImU32 zSelectedFill(int zi){ auto c = zoneRGB(zi); return IM_COL32(c.r, c.g, c.b, 255); }
static ImU32 zPointFill(int zi)   { auto c = zoneRGB(zi); return IM_COL32(c.r*9/10, c.g*9/10, c.b*9/10, 255); }

// Fallback static colors (zone-independent) — monochrome white/gray
// per the "no chromatic accent for now" directive.
static const ImU32 kAccent        = IM_COL32(255, 255, 255, 255);
static const ImU32 kAccentDim     = IM_COL32(180, 180, 180, 255);
static const ImU32 kAccentSoft    = IM_COL32(255, 255, 255, 80);
static const ImU32 kAccentGlow    = IM_COL32(255, 255, 255, 30);
static const ImU32 kWhiteSoft     = IM_COL32(255, 255, 255, 140);
static const ImU32 kHandleOuter   = IM_COL32(255, 255, 255, 220);
static const ImU32 kMaskFill      = IM_COL32(255, 255, 255, 30);
static const ImU32 kMaskCurve     = IM_COL32(255, 255, 255, 200);
static const ImU32 kMaskCurveGlow = IM_COL32(255, 255, 255, 50);
static const ImU32 kHandleLine    = IM_COL32(255, 255, 255, 70);
static const ImU32 kHandleDot     = IM_COL32(255, 255, 255, 200);
static const ImU32 kHandleRing    = IM_COL32(255, 255, 255, 200);
static const ImU32 kSelectedFill  = IM_COL32(255, 255, 255, 255);
static const ImU32 kSelectedRing  = IM_COL32(255, 255, 255, 255);
static const ImU32 kPointFill     = IM_COL32(220, 220, 220, 255);
static const ImU32 kPointRing     = IM_COL32(255, 255, 255, 180);
static const ImU32 kBorderColor   = IM_COL32(255, 255, 255, 20);
static const ImU32 kBorderGlow    = IM_COL32(0, 0, 0, 0);
// Awesome-design: layer selection bbox + handles use the global blue
// accent so the selected layer reads with the same chromatic anchor as
// the active rail icon and the play button. Handle = white fill +
// blue ring, matching the reference's corner-dot appearance.
static const ImU32 kBBoxLine      = IM_COL32(74, 140, 255, 235);
static const ImU32 kBBoxGlow      = IM_COL32(74, 140, 255, 60);
static const ImU32 kBBoxDim       = IM_COL32(255, 255, 255, 30);
static const ImU32 kLHandleFill   = IM_COL32(255, 255, 255, 255);
static const ImU32 kLHandleStroke = IM_COL32(74, 140, 255, 255);
static const ImU32 kLHandleActive = IM_COL32(74, 140, 255, 255);
// Mask-edit handle vocabulary: a single clear gold reused from the mask
// banner / layer-mask gold (255,200,60) so the whole handle set reads
// consistently while editing a mask. Only used in mask-edit mode — normal
// (transform/warp) handles keep their zone/blue colors unchanged.
static const ImU32 kMaskEditFill  = IM_COL32(255, 200, 60, 255);   // handle fill
static const ImU32 kMaskEditRing  = IM_COL32(150, 110, 25, 255);   // darker same-hue outline

glm::vec2 ViewportPanel::screenToUV(glm::vec2 screen) const {
    return glm::vec2(
        (screen.x - m_imageOrigin.x) / m_imageSize.x,
        1.0f - (screen.y - m_imageOrigin.y) / m_imageSize.y);
}

glm::vec2 ViewportPanel::uvToScreenVec(glm::vec2 uv) const {
    return glm::vec2(
        m_imageOrigin.x + uv.x * m_imageSize.x,
        m_imageOrigin.y + (1.0f - uv.y) * m_imageSize.y);
}

glm::vec2 ViewportPanel::screenToNDC(glm::vec2 screen) const {
    return glm::vec2(
        ((screen.x - m_imageOrigin.x) / m_imageSize.x) * 2.0f - 1.0f,
        1.0f - ((screen.y - m_imageOrigin.y) / m_imageSize.y) * 2.0f);
}

glm::vec2 ViewportPanel::ndcToScreen(glm::vec2 ndc) const {
    return glm::vec2(
        m_imageOrigin.x + (ndc.x * 0.5f + 0.5f) * m_imageSize.x,
        m_imageOrigin.y + (1.0f - (ndc.y * 0.5f + 0.5f)) * m_imageSize.y);
}

static ImVec2 toImVec2(glm::vec2 v) { return ImVec2(v.x, v.y); }

#include <functional>

void ViewportPanel::render(GLuint texture, MappingProfile* mapping,
                           float projectorAspect,
                           std::vector<std::unique_ptr<OutputZone>>* zones,
                           int* activeZone,
                           const std::vector<MonitorInfo>* monitors,
                           bool ndiAvailable,
                           int editorMonitor,
                           const std::vector<std::unique_ptr<MappingProfile>>* allMappings,
                           std::function<void()> inlineSetupSection,
                           std::function<void()> navPrefix) {
    // First-render reset — guarantee the canvas opens at the default
    // out-of-the-box state (zoom=1, pan=0,0) regardless of what was in
    // the loaded project or any stale runtime state. Without this, a
    // fresh launch could land with the canvas dragged off-screen and
    // the user with no obvious recovery path.
    static bool s_firstRender = true;
    if (s_firstRender) {
        s_firstRender = false;
        m_zoom = 1.0f;
        m_pan = {0, 0};
    }
    // Unpack mapping for warp overlay
    WarpMode warpMode = mapping ? mapping->warpMode : WarpMode::CornerPin;
    CornerPinWarp* cornerPinPtr = mapping ? &mapping->cornerPin : nullptr;
    MeshWarp* meshWarpPtr = mapping ? &mapping->meshWarp : nullptr;
    ObjMeshWarp* objMeshWarp = mapping ? &mapping->objMeshWarp : nullptr;
    // Skip the whole Canvas render when the workspace is flipped to
    // Stage or Show. Mapping mode REUSES this 2D output viewport (so the
    // corner-pin / mesh-warp / mask handles draw on the live composite),
    // so it renders here too — only Stage and Show suppress it. Only one
    // of the central windows submits per frame, so the dock never shows a
    // tab bar.
    if (UIManager::sMode != UIManager::WorkspaceMode::Canvas &&
        UIManager::sMode != UIManager::WorkspaceMode::Mapping) { return; }
    // (Mode-transition Alpha fade removed — it made the panel translucent
    // during the cross-fade which exposed the GL backbuffer underneath as a
    // white flash. Re-introduce only with a non-translucent technique
    // such as snapshotting the outgoing panel into a texture overlay.)
    // Top padding gives the nav row breathing room from the macOS title
    // bar / menu bar / fullscreen icon above. Previously WindowPadding=0
    // meant the workspace pill was clipped by anything overlaying the
    // top edge. (8,4) keeps the canvas image close to the rails on the
    // sides while reserving 8px headroom for the pill.
    // WindowPadding.y dropped 8 → 0 so the nav row sits flush at the top of
    // the window — same row as the macOS traffic-light buttons. Matches
    // Figma's single thin chrome row instead of a stacked title-bar + nav.
    ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(0, 0));
    // Pure black canvas background — true #000 so the comp reads with real
    // blacks behind it (was #141414 grey, which made the stage feel washed).
    ImGui::PushStyleColor(ImGuiCol_WindowBg, ImVec4(0.0f, 0.0f, 0.0f, 1.0f));
    ImGui::PushStyleColor(ImGuiCol_ChildBg,  ImVec4(0.0f, 0.0f, 0.0f, 1.0f));
    ImGui::Begin("Canvas", nullptr,
                 ImGuiWindowFlags_NoScrollbar | ImGuiWindowFlags_NoScrollWithMouse);
    ImGui::PopStyleVar();
    ImGui::PopStyleColor(2);

    // Track panel visibility and bounds for overlay clipping
    // Check if this window is actually the visible/selected tab in its dock node
    ImGuiWindow* win = ImGui::GetCurrentWindow();
    bool isDockTabVisible = true;
    if (win->DockNode && win->DockNode->VisibleWindow != win)
        isDockTabVisible = false;
    m_panelVisible = isDockTabVisible && !ImGui::IsWindowCollapsed();
    ImVec2 wMin = ImGui::GetWindowContentRegionMin();
    ImVec2 wMax = ImGui::GetWindowContentRegionMax();
    ImVec2 wPos = ImGui::GetWindowPos();
    m_panelMin = {wPos.x + wMin.x, wPos.y + wMin.y};
    m_panelMax = {wPos.x + wMax.x, wPos.y + wMax.y};

    // Zone tab bar + output routing — always visible. Delegated to the
    // shared renderNavBar so the Stage panel can render an identical row
    // and switching workspaces doesn't shift element positions.
    if (zones && activeZone) {
        renderNavBar(false, zones, activeZone, monitors, ndiAvailable, editorMonitor,
                     navPrefix);
        // Capture the nav row's bottom Y in screen space. We use this to
        // (a) seed m_panelMin.y for the layer overlay clip rect AND
        // (b) expose m_navRowBottomY so the overlay can additionally
        //     skip any draw whose top half would punch above the nav.
        // Without (b), a layer whose bbox top sits flush with the canvas
        // image edge produced corner handles whose top halves rendered
        // ON TOP of the nav row.
        // Capture the nav row's bottom edge for overlay clipping. No hairline
        // separator and no dummy spacer band below it — the canvas should
        // sit flush against the bottom of the nav row so there's no visible
        // strip between them.
        m_navRowBottomY = ImGui::GetCursorScreenPos().y;
        m_panelMin.y    = m_navRowBottomY;
        if (inlineSetupSection) {
            ImGui::Indent(6);
            inlineSetupSection();
            ImGui::Unindent(6);
        }
    }

    // --- legacy inline nav (kept disabled) ---
#if 0
    if (zones && activeZone) {
        ImGui::Dummy(ImVec2(0, 2));
        ImGui::Indent(6);

        ImDrawList* tabDraw = ImGui::GetWindowDrawList();

        // --- Canvas / Stage pill — reads/writes UIManager::sShowStage
        //     to swap between workspaces. Only one of Canvas/Stage has
        //     its Begin() submitted per frame (guarded in Application),
        //     so the dock never shows a tab bar and the button is
        //     guaranteed to work.
        {
            ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(16, 7));
            ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing,  ImVec2(0, 0));
            ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 999.0f);
            auto pill = [&](const char* label, bool active) {
                if (active) {
                    ImGui::PushStyleColor(ImGuiCol_Button,        ImVec4(0.96f, 0.97f, 1.00f, 1.00f));
                    ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.00f, 1.00f, 1.00f, 1.00f));
                    ImGui::PushStyleColor(ImGuiCol_ButtonActive,  ImVec4(0.92f, 0.94f, 0.98f, 1.00f));
                    ImGui::PushStyleColor(ImGuiCol_Text,          ImVec4(0.05f, 0.07f, 0.10f, 1.00f));
                    ImGui::PushStyleColor(ImGuiCol_Border,        ImVec4(0.96f, 0.97f, 1.00f, 1.00f));
                } else {
                    // Inactive = grey outline, no fill.
                    ImGui::PushStyleColor(ImGuiCol_Button,        ImVec4(0, 0, 0, 0));
                    ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1, 1, 1, 0.06f));
                    ImGui::PushStyleColor(ImGuiCol_ButtonActive,  ImVec4(1, 1, 1, 0.10f));
                    ImGui::PushStyleColor(ImGuiCol_Text,          ImVec4(0.80f, 0.82f, 0.88f, 1.00f));
                    ImGui::PushStyleColor(ImGuiCol_Border,        ImVec4(0.45f, 0.48f, 0.54f, 0.85f));
                }
                ImGui::PushStyleVar(ImGuiStyleVar_FrameBorderSize, 1.0f);
                bool clicked = ImGui::Button(label);
                ImGui::PopStyleVar();
                ImGui::PopStyleColor(5);
                return clicked;
            };
            // We're inside Canvas::render, so Canvas is the active pill.
            // Clicking Stage flips the workspace flag for next frame.
            pill("CANVAS", true);
            ImGui::SameLine(0, 6);
            if (pill("STAGE", false)) UIManager::sShowStage = true;
            ImGui::PopStyleVar(3);
        }

        // --- Right-align the rest of the nav row (zones → + → OUTPUT → combo
        //     → composition chip). Compute the cluster's total width and jump
        //     the cursor over the gap so only CANVAS/STAGE stay on the left.
        {
            const float kPadX        = 14.0f * 2.0f;  // matches FramePadding
            const float kZoneSpacing = 4.0f;
            const float kSectionGap  = 14.0f;
            const float kInnerGap    = 8.0f;
            const float kComboW      = 150.0f;
            const float kDotW        = 12.0f;
            float rightW = 0.0f;
            if (zones) {
                for (auto& zp : *zones)
                    rightW += ImGui::CalcTextSize(zp->name.c_str()).x + kPadX + kZoneSpacing;
            }
            rightW += ImGui::CalcTextSize("+").x + kPadX;
            rightW += kSectionGap + kDotW + kInnerGap;
            rightW += ImGui::CalcTextSize("OUTPUT").x + kInnerGap;
            rightW += kComboW + kInnerGap;
            int aiLookup = *activeZone;
            if (aiLookup >= 0 && aiLookup < (int)zones->size()) {
                char compLabel[48];
                snprintf(compLabel, sizeof(compLabel), "%d x %d",
                         (*zones)[aiLookup]->width, (*zones)[aiLookup]->height);
                rightW += ImGui::CalcTextSize(compLabel).x + kPadX;
            } else {
                rightW += 120.0f;
            }
            rightW += kInnerGap;
            // Fullscreen button width — worst case is "Fullscreen"
            rightW += ImGui::CalcTextSize("Fullscreen").x + kPadX;
            // Right-edge breathing room — mirrors the ~12px CANVAS pill has
            // on the left, plus slack for scrollbar / sub-pixel / combo arrow.
            rightW += 90.0f;

            ImGui::SameLine();
            float targetX = ImGui::GetContentRegionMax().x - rightW;
            float curX    = ImGui::GetCursorPosX();
            if (targetX > curX) ImGui::SetCursorPosX(targetX);
        }

        // --- Zone tabs ---
        // Awesome-design: zone tabs only render when there are MULTIPLE
        // zones. Single-zone projects (the common case) get a clean
        // top bar with just the workspace pill + Preview/Resolution.
        // Renaming + adding zones still works via the right-click menu
        // and the "+" affordance below.
        bool showZoneTabs = ((int)zones->size() > 1);
        ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing, ImVec2(4, 0));
        ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(14, 7));
        for (int i = 0; showZoneTabs && i < (int)zones->size(); i++) {
            ImGui::PushID(9000 + i);
            bool isActive = (i == *activeZone);
            auto& z = *(*zones)[i];

            // Per-zone color from shared palette (matches layer panel dots)
            static const float zoneColors[][3] = {
                // Monochrome zone tabs — lightness ramp only
                {0.96f, 0.96f, 0.96f}, {0.86f, 0.86f, 0.86f}, {0.76f, 0.76f, 0.76f}, {0.66f, 0.66f, 0.66f},
                {0.56f, 0.56f, 0.56f}, {0.80f, 0.80f, 0.80f}, {0.70f, 0.70f, 0.70f}, {0.60f, 0.60f, 0.60f},
            };
            const float* zc = zoneColors[i % 8];
            if (isActive) {
                ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(zc[0], zc[1], zc[2], 0.25f));
                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(zc[0], zc[1], zc[2], 0.35f));
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(zc[0], zc[1], zc[2], 1.0f));
            } else {
                ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(zc[0] * 0.15f, zc[1] * 0.15f, zc[2] * 0.15f, 0.9f));
                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(zc[0] * 0.25f, zc[1] * 0.25f, zc[2] * 0.25f, 1.0f));
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(zc[0] * 0.6f, zc[1] * 0.6f, zc[2] * 0.6f, 1.0f));
            }
            if (ImGui::Button(z.name.c_str())) {
                *activeZone = i;
            }
            ImGui::PopStyleColor(3);

            // Double-click to rename
            if (ImGui::IsItemHovered() && ImGui::IsMouseDoubleClicked(0)) {
                m_renaming = true;
                m_renameIndex = i;
                strncpy(m_renameBuf, z.name.c_str(), sizeof(m_renameBuf) - 1);
                m_renameBuf[sizeof(m_renameBuf) - 1] = '\0';
            }

            // Right-click context menu
            if (ImGui::BeginPopupContextItem("ZoneTabCtx")) {
                if (ImGui::MenuItem("Rename")) {
                    m_renaming = true;
                    m_renameIndex = i;
                    strncpy(m_renameBuf, z.name.c_str(), sizeof(m_renameBuf) - 1);
                    m_renameBuf[sizeof(m_renameBuf) - 1] = '\0';
                }
                if (ImGui::MenuItem("Duplicate")) {
                    *activeZone = -(200 + i); // signal: duplicate zone i
                }
                if ((int)zones->size() > 1) {
                    if (ImGui::MenuItem("Remove")) {
                        *activeZone = -(300 + i); // signal: remove zone i
                    }
                }
                ImGui::EndPopup();
            }

            // Output type dot on tab (top-left corner) — uses zone color
            ImVec2 btnMin = ImGui::GetItemRectMin();
            if (z.outputDest == OutputDest::Fullscreen || z.outputDest == OutputDest::NDI) {
                ImU32 dotCol = IM_COL32((int)(zc[0]*255), (int)(zc[1]*255), (int)(zc[2]*255), 255);
                tabDraw->AddCircleFilled(ImVec2(btnMin.x + 5, btnMin.y + 5), 3.0f, dotCol);
            }
            ImGui::SameLine();
            ImGui::PopID();
        }

        // Rename popup
        if (m_renaming) {
            ImGui::OpenPopup("##RenameZone");
        }
        if (ImGui::BeginPopup("##RenameZone")) {
            ImGui::Text("Rename Zone");
            ImGui::SetNextItemWidth(200);
            bool enter = ImGui::InputText("##RenameInput", m_renameBuf, sizeof(m_renameBuf),
                                          ImGuiInputTextFlags_EnterReturnsTrue | ImGuiInputTextFlags_AutoSelectAll);
            if (m_renaming) {
                ImGui::SetKeyboardFocusHere(-1);
                m_renaming = false; // only set focus once
            }
            if (enter || ImGui::IsKeyPressed(ImGuiKey_Escape)) {
                if (enter && m_renameIndex >= 0 && m_renameIndex < (int)zones->size() && m_renameBuf[0]) {
                    (*zones)[m_renameIndex]->name = m_renameBuf;
                }
                m_renameIndex = -1;
                ImGui::CloseCurrentPopup();
            }
            ImGui::EndPopup();
        }

        // "+" button to add zone — only when zone tabs are shown.
        // Single-zone projects skip the affordance to keep the bar clean
        // (still reachable via Zone menu in the "···" dropdown).
        if (showZoneTabs) {
            ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(1.0f, 1.0f, 1.0f, 0.08f));
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.25f));
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(1.0f, 1.0f, 1.0f, 0.85f));
            if (ImGui::Button("+")) {
                *activeZone = -(100 + (int)zones->size());
            }
            if (ImGui::IsItemHovered()) {
                ParamRow::Tooltip("Add output zone");
            }
            ImGui::PopStyleColor(3);
        }
        ImGui::PopStyleVar(2);

        // --- Output + Mapping routing (same row as zone tabs) ---
        int ai = *activeZone;
        if (ai >= 0 && ai < (int)zones->size()) {
            auto& az = *(*zones)[ai];

            // Match the pill / zone-tab vertical metrics so the whole
            // nav row baselines cleanly instead of stair-stepping.
            // Right-alignment of the whole cluster (zones + OUTPUT + chip)
            // is handled earlier — see the SameLine(pushX) jump above.
            ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(14, 7));
            ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing, ImVec2(8, 2));
            ImGui::SameLine(0, 14);

            // Build display label
            static char destBuf[128] = {};
            const char* destLabel = "Preview Only";
            if (az.outputDest == OutputDest::Fullscreen && monitors) {
                int mi = az.outputMonitor;
                if (mi >= 0 && mi < (int)monitors->size()) {
                    snprintf(destBuf, sizeof(destBuf), "Fullscreen: %s", (*monitors)[mi].name.c_str());
                    destLabel = destBuf;
                }
            } else if (az.outputDest == OutputDest::NDI) {
                snprintf(destBuf, sizeof(destBuf), "NDI: \"%s\"",
                         az.ndiStreamName.empty() ? az.name.c_str() : az.ndiStreamName.c_str());
                destLabel = destBuf;
            }

            // "Output" label with status dot. Dot Y aligns to the framed
            // widget centre (not text baseline) so it sits on the same line
            // as the combo and zone pills.
            bool live = (az.outputDest != OutputDest::None);
            ImVec2 dotPos = ImGui::GetCursorScreenPos();
            float frameH = ImGui::GetFrameHeight();
            float dotCY = dotPos.y + frameH * 0.5f;
            if (live) {
                tabDraw->AddCircleFilled(ImVec2(dotPos.x + 4, dotCY),
                                         3.5f, IM_COL32(34, 210, 130, 255));
                tabDraw->AddCircle(ImVec2(dotPos.x + 4, dotCY),
                                   5.5f, IM_COL32(34, 210, 130, 40));
            } else {
                tabDraw->AddCircle(ImVec2(dotPos.x + 4, dotCY),
                                   3.0f, IM_COL32(100, 110, 130, 140), 0, 1.2f);
            }
            ImGui::Dummy(ImVec2(12, frameH));
            ImGui::SameLine();

            ImGui::AlignTextToFramePadding();
            if (live) {
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.22f, 0.82f, 0.52f, 1.0f));
            } else {
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 1.0f));
            }
            // Compact reference-style: drop the literal "OUTPUT" word
            // and let the "Preview Only" combo carry the meaning. The
            // combo's chevron + label is enough affordance, and the
            // omission tightens the cluster to the 3-pill rhythm in
            // the reference (Preview ⌄ / 1920×1080 ⌄ / 🟢).
            ImGui::TextUnformatted("");
            ImGui::PopStyleColor();
            ImGui::SameLine();

            // Combo inherits accent color when live
            if (live) {
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.22f, 0.82f, 0.52f, 1.0f));
            } else {
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.70f, 0.73f, 0.78f, 1.0f));
            }

            ImGui::SetNextItemWidth(150.0f);
            if (ImGui::BeginCombo("##ZoneOutput", destLabel, ImGuiComboFlags_HeightLarge)) {
                if (ImGui::Selectable("Preview Only", az.outputDest == OutputDest::None)) {
                    az.outputDest = OutputDest::None;
                    az.outputMonitor = -1;
                }

                if (monitors && !monitors->empty()) {
                    ImGui::Separator();
                    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 0.7f));
                    ImGui::Text("  Fullscreen");
                    ImGui::PopStyleColor();
                    for (int mi = 0; mi < (int)monitors->size(); mi++) {
                        ImGui::PushID(mi);

                        // Skip the editor's own monitor
                        bool isEditorMonitor = (mi == editorMonitor);
                        if (isEditorMonitor) {
                            ImGui::PopID();
                            continue;
                        }

                        // Check if another zone claims this monitor
                        std::string claimedBy;
                        for (int zi = 0; zi < (int)zones->size(); zi++) {
                            if (zi == ai) continue;
                            auto& oz = *(*zones)[zi];
                            if (oz.outputDest == OutputDest::Fullscreen && oz.outputMonitor == mi) {
                                claimedBy = oz.name;
                                break;
                            }
                        }

                        char label[256];
                        if (!claimedBy.empty()) {
                            snprintf(label, sizeof(label), "%s  %dx%d  (-> %s)",
                                     (*monitors)[mi].name.c_str(),
                                     (*monitors)[mi].width, (*monitors)[mi].height,
                                     claimedBy.c_str());
                        } else {
                            snprintf(label, sizeof(label), "%s  %dx%d",
                                     (*monitors)[mi].name.c_str(),
                                     (*monitors)[mi].width, (*monitors)[mi].height);
                        }

                        // Dim text for claimed monitors
                        if (!claimedBy.empty()) {
                            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 0.6f));
                        }
                        bool sel = (az.outputDest == OutputDest::Fullscreen && az.outputMonitor == mi);
                        if (ImGui::Selectable(label, sel)) {
                            // Steal: set the other zone to None
                            if (!claimedBy.empty()) {
                                for (int zi = 0; zi < (int)zones->size(); zi++) {
                                    if (zi == ai) continue;
                                    auto& oz = *(*zones)[zi];
                                    if (oz.outputDest == OutputDest::Fullscreen && oz.outputMonitor == mi) {
                                        oz.outputDest = OutputDest::None;
                                        oz.outputMonitor = -1;
                                        break;
                                    }
                                }
                            }
                            az.outputDest = OutputDest::Fullscreen;
                            az.outputMonitor = mi;
                        }
                        if (!claimedBy.empty()) {
                            ImGui::PopStyleColor();
                        }
                        ImGui::PopID();
                    }
                }

                if (ndiAvailable) {
                    ImGui::Separator();
                    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 0.7f));
                    ImGui::Text("  NDI");
                    ImGui::PopStyleColor();
                    char ndiLabel[256];
                    std::string streamName = az.ndiStreamName.empty() ? az.name : az.ndiStreamName;
                    snprintf(ndiLabel, sizeof(ndiLabel), "Easel - %s", streamName.c_str());
                    bool sel = (az.outputDest == OutputDest::NDI);
                    if (ImGui::Selectable(ndiLabel, sel)) {
                        az.outputDest = OutputDest::NDI;
                        if (az.ndiStreamName.empty()) az.ndiStreamName = az.name;
                    }
                }

                ImGui::EndCombo();
            }
            ImGui::PopStyleColor(); // combo text color

            // --- Per-zone mic (push-to-talk) ------------------------------
            // Multi-floor/multi-room installs: each zone can listen to its
            // own mic instead of the shared global one. Enable configures
            // + persists; the hold-button is the actual push-to-talk gate
            // (same transient pushToTalkActive the mobile app / SDK drive
            // remotely over /easel/zone/mic/ptt).
            {
                // Extra left margin (vs. the usual 6-12px between controls)
                // so this reads as its own labeled cluster, not one more
                // button tacked onto the OUTPUT row.
                ImGui::SameLine(0, 20);
                ImGui::AlignTextToFramePadding();
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 0.85f));
                ImGui::TextUnformatted("ZONE MIC");
                ImGui::PopStyleColor();

                ImGui::SameLine(0, 8);
                bool micOn = az.micEnabled;
                ImGui::PushStyleColor(ImGuiCol_Button, micOn ? ImVec4(0.20f, 0.45f, 0.85f, 0.55f) : ImVec4(1, 1, 1, 0.06f));
                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, micOn ? ImVec4(0.24f, 0.50f, 0.90f, 0.65f) : ImVec4(1, 1, 1, 0.14f));
                ImGui::PushStyleColor(ImGuiCol_Text, micOn ? ImVec4(0.85f, 0.92f, 1.0f, 1.0f) : ImVec4(0.65f, 0.68f, 0.74f, 1.0f));
                if (ImGui::Button(micOn ? "Mic: On" : "Mic: Off")) {
                    az.micEnabled = !az.micEnabled;
                    if (!az.micEnabled) {
                        az.pushToTalkActive = false;
                        az.micAnalyzer.stopCapture();
                    }
                }
                ImGui::PopStyleColor(3);
                if (ImGui::IsItemHovered()) {
                    ImGui::SetTooltip("Give THIS zone its own independent mic input\n"
                                      "(instead of the shared global one below).\n"
                                      "Enable it here, then hold PTT to talk —\n"
                                      "same control the mobile app/SDK drive remotely.");
                }

                if (az.micEnabled) {
                    ImGui::SameLine(0, 6);
                    ImGui::SetNextItemWidth(140.0f);
                    char micIdBuf[128];
                    snprintf(micIdBuf, sizeof(micIdBuf), "%s", az.micDeviceId.c_str());
                    ImGui::PushStyleColor(ImGuiCol_FrameBg, ImVec4(1, 1, 1, 0.06f));
                    if (ImGui::InputTextWithHint("##ZoneMicDevice", "device id (blank=default)",
                                                  micIdBuf, sizeof(micIdBuf))) {
                        az.micDeviceId = micIdBuf;
                    }
                    ImGui::PopStyleColor();
                    if (ImGui::IsItemHovered()) {
                        ImGui::SetTooltip("Capture device id for this zone's mic.\n"
                                          "Leave blank to use the system default input.");
                    }

                    ImGui::SameLine(0, 6);
                    bool held = az.pushToTalkActive;
                    ImGui::PushStyleColor(ImGuiCol_Button, held ? ImVec4(0.85f, 0.25f, 0.30f, 0.85f) : ImVec4(1, 1, 1, 0.10f));
                    ImGui::PushStyleColor(ImGuiCol_ButtonHovered, held ? ImVec4(0.90f, 0.30f, 0.35f, 0.9f) : ImVec4(1, 1, 1, 0.18f));
                    ImGui::PushStyleColor(ImGuiCol_ButtonActive, ImVec4(0.90f, 0.30f, 0.35f, 0.9f));
                    ImGui::Button(held ? "\xE2\x97\x8F PTT" : "PTT Hold");
                    // Hold-to-talk: only touch pushToTalkActive on the actual
                    // press/release transition (not every idle frame) so this
                    // local button doesn't fight remote OSC/mobile-app control
                    // of the same flag when the user isn't touching it here.
                    if (ImGui::IsItemActivated()) az.pushToTalkActive = true;
                    if (ImGui::IsItemDeactivated()) az.pushToTalkActive = false;
                    ImGui::PopStyleColor(3);
                    if (ImGui::IsItemHovered() && !ImGui::IsItemActive()) {
                        ImGui::SetTooltip("Hold to talk on this zone's mic.\n"
                                          "Release to stop — momentary, like a walkie-talkie.");
                    }
                }
            }

            // MAPPING combo removed — the Mapping inspector tab has its
            // own profile picker; duplicating it here was noise.
            // COMPOSITION moved to a popover on the zone tab (see below).

            // --- Composition size as a clickable chip, right-anchored on
            //     the same row so the size of the active zone is always
            //     visible. Clicking opens a preset picker.
            {
                static const char* presetLabels[] = {
                    "1920x1080 (1080p)", "3840x2160 (4K)", "1280x720 (720p)",
                    "2560x1440 (1440p)", "8000x2000 (Ultra-wide)", "1024x768", "Custom"
                };
                static const int presetW[] = { 1920, 3840, 1280, 2560, 8000, 1024, 0 };
                static const int presetH[] = { 1080, 2160, 720, 1440, 2000, 768, 0 };
                const int presetCount = 7;

                ImGui::SameLine(0, 12);
                char compLabel[48];
                snprintf(compLabel, sizeof(compLabel), "%d x %d", az.width, az.height);
                ImGui::PushStyleColor(ImGuiCol_Button,        ImVec4(1, 1, 1, 0.06f));
                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1, 1, 1, 0.14f));
                ImGui::PushStyleColor(ImGuiCol_Text,          ImVec4(0.65f, 0.68f, 0.74f, 1.0f));
                if (ImGui::Button(compLabel)) {
                    // Clicking the aspect/composition chip also re-centers the
                    // canvas in the editor viewport. After heavy panning the
                    // canvas can drift off-screen with no obvious way back —
                    // making the resolution chip double as a "reset view" hit
                    // gives users a single, discoverable recovery action.
                    resetZoom();
                    ImGui::OpenPopup("##CompPreset");
                }
                ImGui::PopStyleColor(3);
                // Fullscreen toggle — sits inline with the composition chip
                // so "display setup" actions (pick output size, go fullscreen)
                // cluster together instead of living in a separate menu row.
                ImGui::SameLine();
                ImGui::PushStyleColor(ImGuiCol_Button,        ImVec4(1, 1, 1, 0.06f));
                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1, 1, 1, 0.14f));
                ImGui::PushStyleColor(ImGuiCol_Text,          ImVec4(0.75f, 0.78f, 0.84f, 1.0f));
                const char* fsLbl = m_editorFullscreenHint ? "Exit FS" : "Fullscreen";
                if (ImGui::Button(fsLbl)) m_wantsFullscreenToggle = true;
                ImGui::PopStyleColor(3);

                if (ImGui::BeginPopup("##CompPreset")) {
                    for (int p = 0; p < presetCount - 1; p++) {
                        bool sel = (az.width == presetW[p] && az.height == presetH[p]);
                        if (ImGui::Selectable(presetLabels[p], sel)) {
                            az.resize(presetW[p], presetH[p]);
                            az.compPreset = p;
                            resetZoom();
                        }
                    }
                    ImGui::Separator();
                    // Custom... — inline W x H inputs with Apply so arbitrary
                    // composition sizes are actually reachable (previously
                    // the popup just set a preset index and closed).
                    static int s_customCompW = 1920, s_customCompH = 1080;
                    static int s_customCompZone = -1;
                    if (s_customCompZone != ai) {
                        s_customCompW = az.width > 0 ? az.width : 1920;
                        s_customCompH = az.height > 0 ? az.height : 1080;
                        s_customCompZone = ai;
                    }
                    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.55f, 0.60f, 0.68f, 1.0f));
                    ImGui::TextUnformatted("Custom");
                    ImGui::PopStyleColor();
                    ImGui::SetNextItemWidth(80);
                    ImGui::InputInt("##compCW", &s_customCompW, 0);
                    ImGui::SameLine();
                    ImGui::TextDisabled("x");
                    ImGui::SameLine();
                    ImGui::SetNextItemWidth(80);
                    ImGui::InputInt("##compCH", &s_customCompH, 0);
                    ImGui::SameLine();
                    bool valid = s_customCompW >= 64 && s_customCompH >= 64
                              && s_customCompW <= 16384 && s_customCompH <= 16384;
                    if (!valid) ImGui::BeginDisabled();
                    if (ImGui::SmallButton("Apply")) {
                        az.resize(s_customCompW, s_customCompH);
                        az.compPreset = presetCount - 1;
                        resetZoom();
                        ImGui::CloseCurrentPopup();
                    }
                    if (!valid) ImGui::EndDisabled();
                    ImGui::EndPopup();
                }
            }

            ImGui::PopStyleVar(2);
        }

        ImGui::Unindent(6);
    }
#endif

    ImVec2 avail = ImGui::GetContentRegionAvail();
    ImVec2 availOrigin = ImGui::GetCursorScreenPos();

    // The Properties/Sources/Audio/MIDI float panel on the right is rendered
    // ON TOP of the Canvas window (it's a separate floating ImGui window with
    // ImGuiWindowFlags_NoBringToFrontOnFocus). GetContentRegionAvail returns
    // the FULL canvas width including the area behind the floating panel, so
    // without this adjustment the canvas image gets sized to span the whole
    // window and the right side is clipped past the window edge / hidden
    // under the panel. Find the right-float host via the always-docked
    // Properties tab and trim avail.x by the overlap so the image fits in
    // the actually-visible viewport.
    // Panel-overlap trims removed. The previous version computed avail.x
    // from floating-panel positions and could collapse the canvas to a
    // tiny strip at the wrong edge if the side panel positions weren't
    // what the trim code assumed. Floating panels now simply render on
    // top of the canvas image — z-order handles the overlap, the canvas
    // itself always uses the full content region.
    (void)availOrigin;
    m_size = {avail.x, avail.y};
    // Push the cursor to availOrigin so the image (and its clip rect, both
    // read from GetCursorScreenPos below) start at the post-trim left edge,
    // not at the original full-window left edge that would put the image
    // behind the Layers float.
    ImGui::SetCursorScreenPos(availOrigin);

    if (texture && avail.x > 1 && avail.y > 1) {
        // Base image size (fit to panel)
        float panelAspect = avail.x / avail.y;
        float baseW, baseH;
        if (projectorAspect > panelAspect) {
            baseW = avail.x; baseH = avail.x / projectorAspect;
        } else {
            baseH = avail.y; baseW = avail.y * projectorAspect;
        }

        // Apply zoom
        float imgW = baseW * m_zoom;
        float imgH = baseH * m_zoom;
        float offsetX = (avail.x - imgW) * 0.5f + m_pan.x;
        float offsetY = (avail.y - imgH) * 0.5f + m_pan.y;

        // Clip the canvas image to the panel's content rect — without
        // this, zoom > 1 spills the image over the workspace nav above
        // and the timeline below. Hit-testing keeps the unclipped image
        // bounds so panning past the edge still works.
        ImVec2 clipMin = ImGui::GetCursorScreenPos();
        ImVec2 clipMax = ImVec2(clipMin.x + avail.x, clipMin.y + avail.y);
        ImGui::PushClipRect(clipMin, clipMax, true);

        ImVec2 cursor = ImGui::GetCursorPos();
        ImGui::SetCursorPos(ImVec2(cursor.x + offsetX, cursor.y + offsetY));
        ImGui::Image((ImTextureID)(intptr_t)texture, ImVec2(imgW, imgH),
                     ImVec2(0, 1), ImVec2(1, 0));

        ImVec2 imgMin = ImGui::GetItemRectMin();
        ImVec2 imgMax = ImGui::GetItemRectMax();
        m_imageOrigin = {imgMin.x, imgMin.y};
        m_imageSize = {imgMax.x - imgMin.x, imgMax.y - imgMin.y};
        // Base bounds: where the canvas would sit at zoom=1, pan=0.
        // The image's parent (panel content rect) has its top-left at the
        // current cursor screen-pos; the base centred offset is just half
        // the panel-vs-base difference. Stored separately so chrome that
        // wants a stable canvas anchor doesn't follow the zoom.
        float baseOffX = (avail.x - baseW) * 0.5f;
        float baseOffY = (avail.y - baseH) * 0.5f;
        m_baseImageOrigin = {clipMin.x + baseOffX, clipMin.y + baseOffY};
        m_baseImageSize   = {baseW, baseH};

        ImGui::PopClipRect();

        // Canvas image previously had a 2-stroke glow + hairline ring
        // around it; removed because it read as an unwanted "work area"
        // border on top of the already-clear darker letterbox. This is a
        // deliberately much lighter treatment than that — a single subtle
        // 1px gray line so the canvas edge stays legible against a black or
        // near-black frame, without reintroducing an obtrusive border.
        ImGui::GetWindowDrawList()->AddRect(
            imgMin, imgMax, IM_COL32(255, 255, 255, 28), 0.0f, 0, 1.0f);

        ImDrawList* draw = ImGui::GetWindowDrawList();

        // Awesome-design: regular dot grid across the entire viewport
        // letterbox, matching the reference's compositional grid. Spacing
        // scales with zoom so the lattice stays visually steady. Drawn
        // BEFORE the layer image overlays so dots sit in the negative
        // space, not over the layer content. (We drew after Image() so
        // we layer over canvas; the dot alpha is low enough that it
        // doesn't muddy the layer.)
        {
            const float gridStep = 28.0f;          // px between dots
            const ImU32 dotCol   = IM_COL32(255, 255, 255, 22);
            float startX = clipMin.x +
                fmodf(clipMax.x - clipMin.x, gridStep) * 0.5f;
            float startY = clipMin.y +
                fmodf(clipMax.y - clipMin.y, gridStep) * 0.5f;
            for (float gy = startY; gy < clipMax.y; gy += gridStep) {
                for (float gx = startX; gx < clipMax.x; gx += gridStep) {
                    draw->AddCircleFilled(ImVec2(gx, gy), 0.9f, dotCol, 8);
                }
            }
        }
    }

    m_hovered = ImGui::IsWindowHovered();

    // Safety net: if NO mouse buttons are down and we still think we're mid-drag,
    // clear the flags. This prevents the canvas from getting stuck unresponsive
    // after the user releases the mouse over a different window (e.g. clicking a
    // zone tab mid-drag, or dragging off the viewport onto another panel).
    if (!ImGui::IsMouseDown(ImGuiMouseButton_Left) &&
        !ImGui::IsMouseDown(ImGuiMouseButton_Middle) &&
        !ImGui::IsMouseDown(ImGuiMouseButton_Right)) {
        if (m_panDragging)   m_panDragging = false;
        if (m_layerDragging) { m_layerDragging = false; m_handleDrag = HandleType::None; }
        if (m_warpDragging)  { m_warpDragging = false;  m_warpDragIndex = -1; }
        if (m_orbitDragging) m_orbitDragging = false;
        // Clear BOTH the mask-point drag and the proximity flag so toggling
        // edit mode / mask-edit on or off can never leave a stuck mask drag
        // nor a stale "mask under cursor" reading that would wrongly suppress
        // warp-corner drags on the next frame.
        if (m_maskDragType > 0) { m_maskDragType = 0; m_maskDragIndex = -1; }
        m_maskHitUnderCursor = false;
    }

    // --- Canvas zoom (scroll wheel) and pan (middle-mouse drag) ---
    if (m_hovered && warpMode != WarpMode::ObjMesh) {
        float scroll = ImGui::GetIO().MouseWheel;
        if (scroll != 0.0f) {
            float oldZoom = m_zoom;
            m_zoom = std::max(0.25f, std::min(8.0f, m_zoom + scroll * 0.15f * m_zoom));
            // Scale pan to keep view centered at same point
            if (oldZoom > 0.0f) {
                float ratio = m_zoom / oldZoom;
                m_pan.x *= ratio;
                m_pan.y *= ratio;
            }
        }
    }

    // Middle-mouse pan, OR Space-held + Left-mouse drag (Photoshop / Figma idiom).
    bool spaceHeld = ImGui::IsKeyDown(ImGuiKey_Space);
    bool spacePanStart = m_hovered && spaceHeld &&
                         ImGui::IsMouseClicked(ImGuiMouseButton_Left);
    // Hand cursor whenever space is held over the canvas (or already
    // panning). Matches the Photoshop / Figma affordance.
    if ((m_hovered && spaceHeld) || (m_panDragging && spaceHeld)) {
        ImGui::SetMouseCursor(ImGuiMouseCursor_Hand);
    }
    if (m_hovered && (ImGui::IsMouseClicked(ImGuiMouseButton_Middle) || spacePanStart)) {
        m_panDragging = true;
        ImVec2 mp = ImGui::GetMousePos();
        m_panDragStart = {mp.x, mp.y};
        m_panStart = m_pan;
    }
    bool spacePanActive = m_panDragging && spaceHeld &&
                          ImGui::IsMouseDown(ImGuiMouseButton_Left);
    if (m_panDragging && (ImGui::IsMouseDown(ImGuiMouseButton_Middle) || spacePanActive)) {
        ImVec2 mp = ImGui::GetMousePos();
        m_pan.x = m_panStart.x + (mp.x - m_panDragStart.x);
        m_pan.y = m_panStart.y + (mp.y - m_panDragStart.y);
    }
    if (ImGui::IsMouseReleased(ImGuiMouseButton_Middle) ||
        (m_panDragging && !spaceHeld && !ImGui::IsMouseDown(ImGuiMouseButton_Middle))) {
        m_panDragging = false;
    }

    // (Pan clamp removed — it was interacting badly with initial layout
    // when avail dimensions weren't yet stable. Reset shortcuts below
    // are the recovery path instead.)
    if (!std::isfinite(m_pan.x) || !std::isfinite(m_pan.y)) m_pan = {0, 0};
    if (!std::isfinite(m_zoom)  || m_zoom <= 0.0f)         m_zoom = 1.0f;

    // Reset zoom/pan — three ways:
    //  • double-click middle mouse (mouse-only)
    //  • `0` while hovering the canvas (no modifier needed)
    //  • Cmd/Ctrl + 0  (works regardless of focus — escape hatch when the
    //    canvas has been panned entirely off-screen and there's nothing
    //    left to hover over).
    bool wantReset = false;
    if (m_hovered && ImGui::IsMouseDoubleClicked(ImGuiMouseButton_Middle))
        wantReset = true;
    if (m_hovered && ImGui::IsKeyPressed(ImGuiKey_0, false))
        wantReset = true;
    bool cmdOrCtrl = ImGui::GetIO().KeyCtrl || ImGui::GetIO().KeySuper;
    if (cmdOrCtrl && ImGui::IsKeyPressed(ImGuiKey_0, false))
        wantReset = true;
    if (wantReset) {
        m_zoom = 1.0f;
        m_pan = {0, 0};
    }

    // --- Draw warp handles ---
    // During MASK edit the white alignment grid is shown so the user can
    // line the mapping up to it — which means the corner-pin / mesh-warp
    // handles MUST stay visible and draggable on top of the grid (they
    // used to vanish entirely in mask mode). The mask point/curve gizmos
    // render on the FOREGROUND draw list (renderMaskOverlay) so they still
    // sit above these window-draw-list warp handles. ObjMesh orbit is a
    // camera, not a gizmo to align, so it stays gated out of mask mode.
    const bool inMaskMode = (m_editMode == EditMode::Mask);
    // UNSELECTED STATE: with nothing selected and no edit/mask context the
    // viewport must be clean — no corner-pin / mesh-warp handles drawn, and
    // nothing invisible to grab. Handles return when a layer/zone mapping
    // surface is selected (m_layerSelected, fed from m_selectedLayer in
    // Application) OR mask-edit mode is active (canvas mask mode deselects
    // the layer but keeps inMaskMode true, so it still shows the gold
    // handles). Gating the whole block also suppresses hit-test/drag.
    // MAPPING workspace is the dedicated projection-calibration mode — the
    // corner-pin / mesh-warp handles are its WHOLE point, so they're always
    // live there regardless of layer selection or mask mode.
    const bool inMappingMode = (UIManager::sMode == UIManager::WorkspaceMode::Mapping);
    const bool warpEditContext = m_layerSelected || inMaskMode || inMappingMode;
    const bool warpDrawOK = warpEditContext &&
                            (!inMaskMode || warpMode != WarpMode::ObjMesh);
    if (warpDrawOK && m_imageSize.x > 0 && m_imageSize.y > 0) {
        if (warpMode == WarpMode::ObjMesh && objMeshWarp) {
            // --- Orbit camera interaction ---
            auto& cam = objMeshWarp->camera();

            if (ImGui::IsMouseClicked(ImGuiMouseButton_Left) && m_hovered && !m_layerDragging) {
                m_orbitDragging = true;
                ImVec2 mp = ImGui::GetMousePos();
                m_orbitDragStart = {mp.x, mp.y};
                m_orbitStartAzimuth = cam.azimuth;
                m_orbitStartElevation = cam.elevation;
            }

            if (m_orbitDragging && ImGui::IsMouseDown(ImGuiMouseButton_Left)) {
                ImVec2 mp = ImGui::GetMousePos();
                float dx = mp.x - m_orbitDragStart.x;
                float dy = mp.y - m_orbitDragStart.y;
                cam.azimuth = m_orbitStartAzimuth - dx * 0.005f;
                cam.elevation = std::max(-1.5f, std::min(1.5f,
                    m_orbitStartElevation + dy * 0.005f));
            }

            if (ImGui::IsMouseReleased(ImGuiMouseButton_Left)) {
                m_orbitDragging = false;
            }

            // Scroll zoom
            if (m_hovered) {
                float scroll = ImGui::GetIO().MouseWheel;
                if (scroll != 0.0f) {
                    cam.distance = std::max(0.5f, std::min(20.0f,
                        cam.distance - scroll * 0.3f));
                }
            }

            // Overlay label
            ImDrawList* draw = ImGui::GetWindowDrawList();
            ImVec2 labelPos(m_imageOrigin.x + 8, m_imageOrigin.y + m_imageSize.y - 20);
            draw->AddText(labelPos, kWhiteSoft, "Orbit | drag to rotate | scroll to zoom");
        } else {
            // --- CornerPin / MeshWarp handle interaction ---
            ImVec2 mousePos = ImGui::GetMousePos();
            glm::vec2 mouseNDC = screenToNDC({mousePos.x, mousePos.y});

            // Warp handles get priority over layer handles (smaller hit
            // targets). In mask mode the corner-pin / mesh-warp handles are
            // ALSO draggable so the mapping can be aligned to the white grid
            // — but a click must never ambiguously move both a mask point and
            // a mapping corner. DISAMBIGUATION by cursor proximity (no hidden
            // modifier): renderMaskOverlay ran last frame and set
            // m_maskHitUnderCursor when a mask point / bezier handle /
            // closeable first-point sits under the cursor (using the SAME
            // hitTestPoint(0.03) / hitTestHandle*(0.025) radii it drags
            // with). If a mask point is under the cursor, the mask-point drag
            // wins and we skip starting a warp drag entirely; the mask pass
            // (which runs after this one) then begins the mask-point gesture.
            // Otherwise the warp-corner drag starts here. Whichever drag
            // starts OWNS the gesture until mouse-up — m_warpDragging /
            // m_maskDragType are sticky and never reassigned mid-drag. The
            // 1-frame staleness of m_maskHitUnderCursor is negligible: at
            // frame rate the cursor barely moves between the mask pass and
            // the next frame's drag-start. ObjMesh orbit stays gated out of
            // mask mode (camera, not an alignment gizmo) via warpDrawOK.
            bool warpStartAllowed = !inMaskMode || !m_maskHitUnderCursor;
            if (warpStartAllowed && cornerPinPtr && meshWarpPtr && ImGui::IsMouseClicked(ImGuiMouseButton_Left) && m_hovered) {
                int hit = -1;
                if (warpMode == WarpMode::CornerPin) {
                    hit = cornerPinPtr->hitTest(mouseNDC);
                } else {
                    hit = meshWarpPtr->hitTest(mouseNDC);
                }
                if (hit >= 0) {
                    m_warpDragIndex = hit;
                    m_warpDragging = true;
                }
            }

            // Right-click a mesh point toggles it between curve (smooth) and
            // straight (corner): segments touching a corner point render
            // linear, so you can pin hard edges without losing the curve
            // elsewhere. Corner points draw as squares, smooth ones as circles.
            if (warpStartAllowed && meshWarpPtr && warpMode == WarpMode::MeshWarp &&
                ImGui::IsMouseClicked(ImGuiMouseButton_Right) && m_hovered) {
                int hit = meshWarpPtr->hitTest(mouseNDC);
                if (hit >= 0) meshWarpPtr->toggleCorner(hit);
            }

            if (m_warpDragging && cornerPinPtr && meshWarpPtr && ImGui::IsMouseDown(ImGuiMouseButton_Left)) {
                glm::vec2 clamped(std::max(-1.5f, std::min(1.5f, mouseNDC.x)),
                                  std::max(-1.5f, std::min(1.5f, mouseNDC.y)));
                if (warpMode == WarpMode::CornerPin) {
                    cornerPinPtr->corners()[m_warpDragIndex] = clamped;
                } else {
                    meshWarpPtr->points()[m_warpDragIndex] = clamped;
                }
            }

            if (ImGui::IsMouseReleased(ImGuiMouseButton_Left)) {
                m_warpDragging = false;
                m_warpDragIndex = -1;
            }

            // Draw warp handles (zone-colored). Same clip-below-nav rule
            // as renderLayerOverlay so a corner-pin handle dragged up to
            // the canvas top edge can't paint over the workspace pill.
            ImDrawList* draw = ImGui::GetWindowDrawList();
            float warpClipTop = std::max(m_panelMin.y, m_navRowBottomY);
            draw->PushClipRect(ImVec2(m_panelMin.x, warpClipTop),
                               ImVec2(m_panelMax.x, m_panelMax.y), true);
            auto ndc2scr = [&](glm::vec2 ndc) -> ImVec2 { return toImVec2(ndcToScreen(ndc)); };
            int zi = activeZone ? *activeZone : 0;

            if (warpMode == WarpMode::CornerPin && cornerPinPtr) {
                const auto& corners = cornerPinPtr->corners();

                for (int i = 0; i < 4; i++) {
                    draw->AddLine(ndc2scr(corners[i]), ndc2scr(corners[(i + 1) % 4]), zAccentGlow(zi), 6.0f);
                    draw->AddLine(ndc2scr(corners[i]), ndc2scr(corners[(i + 1) % 4]), zAccent(zi), 1.5f);
                }
                for (int i = 0; i < 4; i++) {
                    ImVec2 p = ndc2scr(corners[i]);
                    bool active = (m_warpDragging && m_warpDragIndex == i);
                    // Mask-edit mode: gold handles so the warp corners read
                    // consistently with the mask point/handle gizmos. Normal
                    // mode keeps the zone accent colors unchanged.
                    ImU32 cpFill = inMaskMode ? kMaskEditFill
                                              : (active ? zAccent(zi) : zAccentDim(zi));
                    ImU32 cpRing = inMaskMode ? kMaskEditRing : kHandleOuter;
                    draw->AddCircleFilled(p, active ? 14.0f : 10.0f, zAccentGlow(zi), 32);
                    draw->AddCircleFilled(p, active ? 8.0f : 6.0f, cpFill, 32);
                    draw->AddCircle(p, active ? 8.0f : 6.0f, cpRing, 32, 1.5f);
                }
            } else if (meshWarpPtr) {
                const auto& points = meshWarpPtr->points();
                int cols = meshWarpPtr->cols(), rows = meshWarpPtr->rows();
                for (int r = 0; r < rows; r++)
                    for (int c = 0; c < cols - 1; c++)
                        draw->AddLine(ndc2scr(points[r*cols+c]), ndc2scr(points[r*cols+c+1]), zAccentSoft(zi), 1.0f);
                for (int c = 0; c < cols; c++)
                    for (int r = 0; r < rows - 1; r++)
                        draw->AddLine(ndc2scr(points[r*cols+c]), ndc2scr(points[(r+1)*cols+c]), zAccentSoft(zi), 1.0f);
                for (int i = 0; i < (int)points.size(); i++) {
                    ImVec2 p = ndc2scr(points[i]);
                    bool active = (m_warpDragging && m_warpDragIndex == i);
                    // Mask-edit mode: gold to match the mask gizmos; normal
                    // mode keeps the zone accent colors unchanged.
                    ImU32 mwFill = inMaskMode ? kMaskEditFill
                                              : (active ? zAccent(zi) : zPointFill(zi));
                    ImU32 mwRing = inMaskMode ? kMaskEditRing : kPointRing;
                    float rad = active ? 6.0f : 4.0f;
                    // Straight/corner points draw as squares; smooth as circles.
                    if (meshWarpPtr->isCorner(i)) {
                        draw->AddRectFilled(ImVec2(p.x - rad, p.y - rad),
                                            ImVec2(p.x + rad, p.y + rad), mwFill, 1.0f);
                        draw->AddRect(ImVec2(p.x - rad, p.y - rad),
                                      ImVec2(p.x + rad, p.y + rad), mwRing, 1.0f, 0, 1.2f);
                    } else {
                        draw->AddCircleFilled(p, rad, mwFill, 32);
                        draw->AddCircle(p, rad, mwRing, 32, 1.2f);
                    }
                }
            }
            draw->PopClipRect();  // matches the PushClipRect at the start
                                  // of the warp-handle block.
        }
    }

    ImGui::End();
}

// ======== LAYER TRANSFORM OVERLAY ========

// Distance from point to line segment
static float pointToSegmentDist(ImVec2 p, ImVec2 a, ImVec2 b) {
    float abx = b.x-a.x, aby = b.y-a.y;
    float apx = p.x-a.x, apy = p.y-a.y;
    float t = (abx*apx + aby*apy) / (abx*abx + aby*aby + 1e-8f);
    t = std::max(0.0f, std::min(1.0f, t));
    float cx = a.x + t*abx, cy = a.y + t*aby;
    float dx = p.x-cx, dy = p.y-cy;
    return sqrtf(dx*dx + dy*dy);
}

void ViewportPanel::renderLayerOverlay(LayerStack& stack, int& selectedLayer, int canvasW, int canvasH) {
    if (m_imageSize.x <= 0 || m_imageSize.y <= 0) return;
    if (!m_panelVisible) return;
    if (m_editMode != EditMode::Normal) return;
    if (stack.count() == 0) {
        m_layerDragging = false;
        m_handleDrag = HandleType::None;
        selectedLayer = -1;
        return;
    }
    if (selectedLayer >= stack.count()) {
        selectedLayer = stack.count() - 1;
        m_layerDragging = false;
        m_handleDrag = HandleType::None;
    }

    bool warpBusy = m_warpDragging;
    // Use the Canvas window's own draw list — BackgroundDrawList sits
    // BEHIND windows and gets occluded by the opaque canvas BG, which
    // hid all the warp / mapping / layer handles. WindowDrawList draws
    // on top of the Image() call so gizmo overlays are actually visible.
    // Manual clip below keeps overlays from spilling onto the workspace
    // nav / timeline; the floating dock panels render on top regardless.
    ImDrawList* draw = ImGui::GetWindowDrawList();

    // Clip overlay drawing to viewport panel bounds. Force the top of
    // the clip down to m_navRowBottomY so corner handles sitting at the
    // canvas image's top edge never punch above the nav row. Without
    // this, the bbox + handles z-order on top of the workspace pill
    // and the OUTPUT cluster (visible in the user's screenshot).
    float clipTop = std::max(m_panelMin.y, m_navRowBottomY);
    draw->PushClipRect(ImVec2(m_panelMin.x, clipTop),
                       ImVec2(m_panelMax.x, m_panelMax.y), true);
    ImVec2 mouse = ImGui::GetMousePos();
    glm::vec2 mouseNDC = screenToNDC({mouse.x, mouse.y});
    const float handleR = 5.0f;
    const float hitR = 10.0f;
    const float edgeHitDist = 6.0f;

    // --- Helpers ---
    auto getCorners = [&](const std::shared_ptr<Layer>& layer, ImVec2 out[4]) {
        float sx = layer->scale.x * (layer->flipH ? -1.0f : 1.0f);
        float sy = layer->scale.y * (layer->flipV ? -1.0f : 1.0f);
        // Apply same aspect ratio correction as CompositeEngine::nativeScale
        // so bounding box matches rendered content
        bool mosaicFill = (layer->tileX > 1.0f || layer->tileY > 1.0f ||
                           layer->mosaicMode != MosaicMode::Mirror);
        if (!mosaicFill && layer->source && canvasW > 0 && canvasH > 0) {
            int lw = layer->width(), lh = layer->height();
            if (lw > 0 && lh > 0) {
                float srcAspect = (float)lw / lh;
                float canvasAspect = (float)canvasW / canvasH;
                sx *= srcAspect / canvasAspect;
            }
        }
        float rad = glm::radians(layer->rotation);
        float c = cosf(rad), s = sinf(rad);
        float px = layer->position.x, py = layer->position.y;
        glm::vec2 local[4] = {{-1,-1},{1,-1},{1,1},{-1,1}};
        for (int i = 0; i < 4; i++) {
            float lx = local[i].x * sx, ly = local[i].y * sy;
            float rx = lx*c - ly*s, ry = lx*s + ly*c;
            out[i] = toImVec2(ndcToScreen({rx + px, ry + py}));
        }
    };
    auto midPt = [](ImVec2 a, ImVec2 b) -> ImVec2 { return ImVec2((a.x+b.x)*0.5f, (a.y+b.y)*0.5f); };
    auto distPt = [](ImVec2 a, ImVec2 b) -> float { float dx=a.x-b.x, dy=a.y-b.y; return sqrtf(dx*dx+dy*dy); };
    auto cross2d = [](ImVec2 o, ImVec2 a, ImVec2 b) -> float { return (a.x-o.x)*(b.y-o.y)-(a.y-o.y)*(b.x-o.x); };
    auto inQuad = [&](ImVec2 c[4], ImVec2 pt) -> bool {
        bool pos=true, neg=true;
        for (int j=0; j<4; j++) { float cp=cross2d(c[j],c[(j+1)%4],pt); if(cp<0)pos=false; if(cp>0)neg=false; }
        return pos||neg;
    };
    HandleType handleTypes[8] = {
        HandleType::TopLeft, HandleType::Top, HandleType::TopRight, HandleType::Right,
        HandleType::BottomRight, HandleType::Bottom, HandleType::BottomLeft, HandleType::Left
    };
    auto getHandles = [&](ImVec2 c[4], ImVec2 out[8]) {
        out[0]=c[0]; out[1]=midPt(c[0],c[1]); out[2]=c[1]; out[3]=midPt(c[1],c[2]);
        out[4]=c[2]; out[5]=midPt(c[2],c[3]); out[6]=c[3]; out[7]=midPt(c[3],c[0]);
    };

    // --- Draw dim bboxes for non-selected layers ---
    for (int i = 0; i < stack.count(); i++) {
        if (!stack[i]->visible || !stack[i]->source || i == selectedLayer) continue;
        ImVec2 c[4]; getCorners(stack[i], c);
        // Subtle hover highlight when mouse is near
        bool hovering = inQuad(c, mouse) && m_hovered;
        ImU32 col = hovering ? IM_COL32(255, 255, 255, 80) : kBBoxDim;
        for (int j = 0; j < 4; j++)
            draw->AddLine(c[j], c[(j+1)%4], col, hovering ? 1.5f : 1.0f);
    }

    // --- Handle mouse-down for selection ---
    if (ImGui::IsMouseClicked(ImGuiMouseButton_Left) && m_hovered && !warpBusy && !m_layerDragging) {
        bool hitSomething = false;

        // If there's a selected layer, check its handles first
        if (selectedLayer >= 0 && selectedLayer < stack.count() && stack[selectedLayer]->source) {
            ImVec2 selCorners[4], selHandles[8];
            getCorners(stack[selectedLayer], selCorners);
            getHandles(selCorners, selHandles);

            for (int h = 0; h < 8; h++) {
                if (distPt(mouse, selHandles[h]) < hitR) {
                    m_handleDrag = handleTypes[h];
                    m_layerDragging = true;
                    m_dragStartMouse = mouseNDC;
                    m_dragStartPos = stack[selectedLayer]->position;
                    m_dragStartScale = stack[selectedLayer]->scale;
                    m_dragStartRotation = stack[selectedLayer]->rotation;
                    m_dragStartRatio = stack[selectedLayer]->scale.x / std::max(0.001f, stack[selectedLayer]->scale.y);
                    hitSomething = true;
                    break;
                }
            }

            // Check body for move
            if (!hitSomething && inQuad(selCorners, mouse)) {
                m_handleDrag = HandleType::Move;
                m_layerDragging = true;
                m_dragStartMouse = mouseNDC;
                m_dragStartPos = stack[selectedLayer]->position;
                m_dragStartScale = stack[selectedLayer]->scale;
                m_dragStartRotation = stack[selectedLayer]->rotation;
                hitSomething = true;
            }
        }

        // Try selecting a different layer (top-to-bottom)
        if (!hitSomething) {
            bool found = false;
            for (int idx = stack.count()-1; idx >= 0; idx--) {
                if (!stack[idx]->visible || !stack[idx]->source) continue;
                ImVec2 c2[4]; getCorners(stack[idx], c2);
                if (inQuad(c2, mouse)) {
                    selectedLayer = idx;
                    m_handleDrag = HandleType::Move;
                    m_layerDragging = true;
                    m_dragStartMouse = mouseNDC;
                    m_dragStartPos = stack[idx]->position;
                    m_dragStartScale = stack[idx]->scale;
                    m_dragStartRotation = stack[idx]->rotation;
                    found = true;
                    break;
                }
            }
            // Clicked empty space: deselect
            if (!found) {
                selectedLayer = -1;
                m_layerDragging = false;
                m_handleDrag = HandleType::None;
            }
        }
    }

    // --- Double-click corner: enter mask edit mode ---
    if (ImGui::IsMouseDoubleClicked(ImGuiMouseButton_Left) && m_hovered && !warpBusy) {
        if (selectedLayer >= 0 && selectedLayer < stack.count() && stack[selectedLayer]->source) {
            ImVec2 selCorners[4];
            getCorners(stack[selectedLayer], selCorners);
            for (int j = 0; j < 4; j++) {
                if (distPt(mouse, selCorners[j]) < hitR * 1.5f) {
                    m_wantsMaskEdit = true;
                    m_layerDragging = false;
                    m_handleDrag = HandleType::None;
                    break;
                }
            }
            // Double-click on edge: enter mask and add point there
            if (!m_wantsMaskEdit) {
                for (int j = 0; j < 4; j++) {
                    float d = pointToSegmentDist(mouse, selCorners[j], selCorners[(j+1)%4]);
                    if (d < edgeHitDist * 2.0f) {
                        m_wantsMaskEdit = true;
                        // Convert click position to UV for mask point
                        m_maskEditClickUV = screenToUV({mouse.x, mouse.y});
                        m_layerDragging = false;
                        m_handleDrag = HandleType::None;
                        break;
                    }
                }
            }
        }
    }

    // --- Dragging ---
    if (m_layerDragging && ImGui::IsMouseDown(ImGuiMouseButton_Left) &&
        selectedLayer >= 0 && selectedLayer < stack.count()) {
        auto& dl = stack[selectedLayer];
        glm::vec2 delta = mouseNDC - m_dragStartMouse;
        bool shift = ImGui::GetIO().KeyShift;

        if (m_handleDrag == HandleType::Move) {
            dl->position = m_dragStartPos + delta;
        } else {
            // Rotate mouse delta into the layer's local coordinate space
            // so handle dragging works correctly at any rotation angle.
            // Use drag-start rotation so projection is stable if rotation
            // changes during the drag (e.g. audio bindings).
            float rad = glm::radians(m_dragStartRotation);
            float c = cosf(rad), s = sinf(rad);
            float dx = delta.x * c + delta.y * s;   // local X (along layer's right)
            float dy = -delta.x * s + delta.y * c;   // local Y (along layer's up)
            glm::vec2 ns = m_dragStartScale;

            switch (m_handleDrag) {
            case HandleType::TopLeft:     ns.x = std::max(0.05f, ns.x - dx*0.5f); ns.y = std::max(0.05f, ns.y - dy*0.5f); break;
            case HandleType::TopRight:    ns.x = std::max(0.05f, ns.x + dx*0.5f); ns.y = std::max(0.05f, ns.y - dy*0.5f); break;
            case HandleType::BottomLeft:  ns.x = std::max(0.05f, ns.x - dx*0.5f); ns.y = std::max(0.05f, ns.y + dy*0.5f); break;
            case HandleType::BottomRight: ns.x = std::max(0.05f, ns.x + dx*0.5f); ns.y = std::max(0.05f, ns.y + dy*0.5f); break;
            case HandleType::Top:    ns.y = std::max(0.05f, ns.y - dy*0.5f); break;
            case HandleType::Bottom: ns.y = std::max(0.05f, ns.y + dy*0.5f); break;
            case HandleType::Left:   ns.x = std::max(0.05f, ns.x - dx*0.5f); break;
            case HandleType::Right:  ns.x = std::max(0.05f, ns.x + dx*0.5f); break;
            default: break;
            }

            // Shift: constrain aspect ratio (lock to drag-start ratio)
            if (shift && m_dragStartRatio > 0.001f) {
                bool isCornerHandle = (m_handleDrag == HandleType::TopLeft || m_handleDrag == HandleType::TopRight ||
                                       m_handleDrag == HandleType::BottomLeft || m_handleDrag == HandleType::BottomRight);
                if (isCornerHandle) {
                    // Use the axis with the larger delta to drive both
                    float adx = fabsf(ns.x - m_dragStartScale.x);
                    float ady = fabsf(ns.y - m_dragStartScale.y);
                    if (adx > ady) {
                        ns.y = std::max(0.05f, ns.x / m_dragStartRatio);
                    } else {
                        ns.x = std::max(0.05f, ns.y * m_dragStartRatio);
                    }
                }
            }

            dl->scale = ns;
        }
    }

    if (ImGui::IsMouseReleased(ImGuiMouseButton_Left)) {
        m_layerDragging = false;
        m_handleDrag = HandleType::None;
    }

    // --- Draw selected layer ---
    if (selectedLayer >= 0 && selectedLayer < stack.count()) {
        auto& layer = stack[selectedLayer];
        if (layer->source) {
            ImVec2 corners[4];
            getCorners(layer, corners);

            // Bbox glow + line
            for (int j = 0; j < 4; j++)
                draw->AddLine(corners[j], corners[(j+1)%4], kBBoxGlow, 4.0f);
            for (int j = 0; j < 4; j++)
                draw->AddLine(corners[j], corners[(j+1)%4], kBBoxLine, 1.5f);

            // Edge hover highlight
            if (!m_layerDragging && m_hovered) {
                for (int j = 0; j < 4; j++) {
                    float d = pointToSegmentDist(mouse, corners[j], corners[(j+1)%4]);
                    if (d < edgeHitDist) {
                        draw->AddLine(corners[j], corners[(j+1)%4], IM_COL32(255, 255, 255, 200), 3.0f);
                    }
                }
            }

            // Handles
            ImVec2 handles[8];
            getHandles(corners, handles);

            for (int h = 0; h < 8; h++) {
                bool isCorner = (h % 2 == 0);
                float r = isCorner ? handleR : (handleR - 1.5f);
                bool active = (m_layerDragging && m_handleDrag == handleTypes[h]);
                bool hovered = (!m_layerDragging && distPt(mouse, handles[h]) < hitR && m_hovered);

                ImU32 fillCol = (active || hovered) ? IM_COL32(255, 255, 255, 255) : kLHandleFill;
                ImU32 strokeCol = active ? kLHandleActive : (hovered ? IM_COL32(255, 255, 255, 255) : kLHandleStroke);
                float drawR = (active || hovered) ? r + 1.5f : r;

                if (isCorner) {
                    ImVec2 hMin(handles[h].x-drawR, handles[h].y-drawR);
                    ImVec2 hMax(handles[h].x+drawR, handles[h].y+drawR);
                    draw->AddRectFilled(hMin, hMax, fillCol, 1.5f);
                    draw->AddRect(hMin, hMax, strokeCol, 1.5f, 0, 1.5f);
                } else {
                    // num_segments=32 → smooth, non-pixelated edges
                    draw->AddCircleFilled(handles[h], drawR, fillCol, 32);
                    draw->AddCircle(handles[h], drawR, strokeCol, 32, 1.5f);
                }
            }

            // Cursor hints
            if (!m_layerDragging && !warpBusy && m_hovered) {
                bool setCursor = false;
                for (int h = 0; h < 8; h++) {
                    if (distPt(mouse, handles[h]) < hitR) {
                        if (h==0||h==4) ImGui::SetMouseCursor(ImGuiMouseCursor_ResizeNWSE);
                        else if (h==2||h==6) ImGui::SetMouseCursor(ImGuiMouseCursor_ResizeNESW);
                        else if (h==1||h==5) ImGui::SetMouseCursor(ImGuiMouseCursor_ResizeNS);
                        else if (h==3||h==7) ImGui::SetMouseCursor(ImGuiMouseCursor_ResizeEW);
                        setCursor = true;
                        break;
                    }
                }
                // Edge hover cursor (crosshair for "add mask point")
                if (!setCursor) {
                    for (int j = 0; j < 4; j++) {
                        if (pointToSegmentDist(mouse, corners[j], corners[(j+1)%4]) < edgeHitDist) {
                            ImGui::SetMouseCursor(ImGuiMouseCursor_Hand);
                            break;
                        }
                    }
                }
            }

            // Arrow key nudge (1 canvas pixel)
            if (!ImGui::GetIO().WantTextInput) {
                float nudgeX = 2.0f / (float)canvasW;
                float nudgeY = 2.0f / (float)canvasH;
                if (ImGui::IsKeyPressed(ImGuiKey_LeftArrow))  layer->position.x -= nudgeX;
                if (ImGui::IsKeyPressed(ImGuiKey_RightArrow)) layer->position.x += nudgeX;
                if (ImGui::IsKeyPressed(ImGuiKey_UpArrow))    layer->position.y += nudgeY;
                if (ImGui::IsKeyPressed(ImGuiKey_DownArrow))  layer->position.y -= nudgeY;
            }
        }
    }

    draw->PopClipRect();
}

// ======== MASK OVERLAY ========

void ViewportPanel::renderMaskOverlay(MaskPath& mask, const glm::mat3& layerTransform, int zoneIndex) {
    if (m_imageSize.x <= 0 || m_imageSize.y <= 0) return;
    if (!m_panelVisible) return;
    if (m_editMode != EditMode::Mask) return;

    // Convert between canvas UV (what's displayed) and layer UV (what the mask stores).
    // Mask points are in layer UV; the viewport shows canvas UV.
    glm::mat3 invXform = glm::inverse(layerTransform);
    auto canvasToLayerUV = [&](glm::vec2 cuv) -> glm::vec2 {
        glm::vec2 ndc = cuv * 2.0f - 1.0f;
        glm::vec3 lndc = invXform * glm::vec3(ndc, 1.0f);
        return glm::vec2(lndc.x, lndc.y) * 0.5f + 0.5f;
    };
    auto layerToCanvasUV = [&](glm::vec2 luv) -> glm::vec2 {
        glm::vec2 ndc = luv * 2.0f - 1.0f;
        glm::vec3 cndc = layerTransform * glm::vec3(ndc, 1.0f);
        return glm::vec2(cndc.x, cndc.y) * 0.5f + 0.5f;
    };
    // Screen-to-layer and layer-to-screen convenience
    auto screenToLayerUV = [&](glm::vec2 screen) -> glm::vec2 {
        return canvasToLayerUV(screenToUV(screen));
    };
    auto layerUVToScreen = [&](glm::vec2 luv) -> glm::vec2 {
        return uvToScreenVec(layerToCanvasUV(luv));
    };


    // Foreground draw list — handles must render ABOVE the Canvas window's
    // opaque background fill. BackgroundDrawList rendered FIRST (before any
    // window) and the Canvas's near-black bg obscured the mask handles
    // entirely; switching to foreground guarantees they sit on top.
    ImDrawList* draw = ImGui::GetForegroundDrawList();
    // Clip mask overlay drawing to viewport panel bounds (below the nav row).
    draw->PushClipRect(ImVec2(m_panelMin.x, m_panelMin.y),
                       ImVec2(m_panelMax.x, m_panelMax.y), true);
    ImVec2 mousePos = ImGui::GetMousePos();
    glm::vec2 mouseUV = screenToLayerUV({mousePos.x, mousePos.y});

    // --- Compute banner rect early so clicks on the banner/Save button
    // don't leak through to the mask-point-add handler below. Centered
    // horizontally near the top of the canvas — the previous top-right
    // pin pushed Save off-screen on narrow layouts and made it hard to
    // find. Centered placement also reads as a focused "you're in mask
    // edit mode" affordance.
    ImVec2 bannerMin, bannerMax, saveBtnMin, saveBtnMax;
    {
        const char* banner = mask.closed()
            ? "MASK EDIT  |  Shift+Click: multi-select  •  Drag: move  •  R-click: del"
            : "MASK EDIT  |  Click: add points  •  Click first point to close  •  R-click: del";
        ImVec2 ts = ImGui::CalcTextSize(banner);
        const char* saveLbl = "Save";
        ImVec2 sts = ImGui::CalcTextSize(saveLbl);
        float pad = 10.0f;
        float saveBtnW = sts.x + 22.0f;
        float totalW = ts.x + pad * 2 + saveBtnW + 8.0f;
        float totalH = ts.y + pad * 1.4f;
        // Center horizontally, sit 16px below the canvas content top.
        float panelW = m_panelMax.x - m_panelMin.x;
        float bannerX = m_panelMin.x + (panelW - totalW) * 0.5f;
        bannerMin = ImVec2(bannerX, m_panelMin.y + 16);
        bannerMax = ImVec2(bannerMin.x + totalW, bannerMin.y + totalH);
        saveBtnMin = ImVec2(bannerMin.x + pad + ts.x + pad, bannerMin.y);
        saveBtnMax = ImVec2(saveBtnMin.x + saveBtnW, bannerMin.y + totalH);
    }
    auto ptInRect = [](ImVec2 p, ImVec2 a, ImVec2 b) {
        return p.x >= a.x && p.x <= b.x && p.y >= a.y && p.y <= b.y;
    };
    bool mouseOverBanner = ptInRect(mousePos, bannerMin, bannerMax);

    bool shiftHeld = ImGui::GetIO().KeyShift;
    auto isSelected = [&](int idx) -> bool {
        for (int s : m_maskSelectedPoints) if (s == idx) return true;
        return false;
    };

    // Disambiguation source-of-truth: is a mask point / bezier handle under
    // the cursor RIGHT NOW? render() (which runs before this pass) reads this
    // on the next frame to decide whether a warp-corner drag may start —
    // mask points win on overlap. Uses the EXACT same hit radii the
    // mask-point drag-start below uses (0.025 handles, 0.03 points) so the
    // priority decision and the actual drag agree. Cleared when the cursor
    // is over the Save banner so a banner click can't block warp handles.
    m_maskHitUnderCursor =
        m_hovered && !mouseOverBanner &&
        (mask.hitTestHandleIn(mouseUV, 0.025f) >= 0 ||
         mask.hitTestHandleOut(mouseUV, 0.025f) >= 0 ||
         mask.hitTestPoint(mouseUV, 0.03f) >= 0);

    if (ImGui::IsMouseClicked(ImGuiMouseButton_Left) && m_hovered && !mouseOverBanner) {
        int hi = mask.hitTestHandleIn(mouseUV, 0.025f);
        int ho = mask.hitTestHandleOut(mouseUV, 0.025f);
        int pt = mask.hitTestPoint(mouseUV, 0.03f);

        if (hi >= 0) {
            m_maskDragIndex = hi; m_maskDragType = 2; m_maskSelectedPoint = hi;
        } else if (ho >= 0) {
            m_maskDragIndex = ho; m_maskDragType = 3; m_maskSelectedPoint = ho;
        } else if (pt >= 0) {
            // Click on point 0 with 3+ points and open path → close the mask
            if (pt == 0 && !mask.closed() && mask.count() >= 3) {
                mask.setClosed(true);
                mask.markDirty();
                m_maskSelectedPoint = 0;
            } else {
                // Shift-click: toggle selection
                if (shiftHeld) {
                    if (isSelected(pt)) {
                        m_maskSelectedPoints.erase(
                            std::remove(m_maskSelectedPoints.begin(), m_maskSelectedPoints.end(), pt),
                            m_maskSelectedPoints.end());
                    } else {
                        m_maskSelectedPoints.push_back(pt);
                    }
                } else {
                    if (!isSelected(pt)) {
                        m_maskSelectedPoints.clear();
                        m_maskSelectedPoints.push_back(pt);
                    }
                }
                m_maskSelectedPoint = pt;
                m_maskDragIndex = pt; m_maskDragType = 1;
            }
        } else {
            float t; int edge = mask.hitTestEdge(mouseUV, 0.02f, t);
            if (edge >= 0 && mask.count() >= 2 && mask.closed()) {
                mask.insertPoint(edge, t);
                m_maskSelectedPoints.clear();
                m_maskSelectedPoints.push_back(edge + 1);
                m_maskSelectedPoint = edge + 1;
                m_maskDragIndex = m_maskSelectedPoint; m_maskDragType = 1;
            } else {
                // Add new point at end (open path building)
                mask.addPoint(mouseUV);
                m_maskSelectedPoints.clear();
                m_maskSelectedPoints.push_back(mask.count() - 1);
                m_maskSelectedPoint = mask.count() - 1;
                m_maskDragIndex = m_maskSelectedPoint; m_maskDragType = 1;
            }
        }
    }

    if (m_maskDragType > 0 && ImGui::IsMouseDown(ImGuiMouseButton_Left)) {
        auto& pts = mask.points();
        if (m_maskDragIndex >= 0 && m_maskDragIndex < (int)pts.size()) {
            glm::vec2 clamped = glm::clamp(mouseUV, glm::vec2(0.0f), glm::vec2(1.0f));
            if (m_maskDragType == 1) {
                // Move all selected points together
                glm::vec2 delta = clamped - pts[m_maskDragIndex].position;
                if (m_maskSelectedPoints.size() > 1) {
                    for (int si : m_maskSelectedPoints) {
                        if (si >= 0 && si < (int)pts.size()) {
                            pts[si].position = glm::clamp(pts[si].position + delta, glm::vec2(0.0f), glm::vec2(1.0f));
                        }
                    }
                } else {
                    pts[m_maskDragIndex].position = clamped;
                }
            } else if (m_maskDragType == 2) {
                auto& pt = pts[m_maskDragIndex];
                pt.handleIn = clamped - pt.position;
                if (pt.smooth) { float l = glm::length(pt.handleOut); if (l<0.001f) l=glm::length(pt.handleIn); pt.handleOut = glm::normalize(-pt.handleIn)*l; }
            } else if (m_maskDragType == 3) {
                auto& pt = pts[m_maskDragIndex];
                pt.handleOut = clamped - pt.position;
                if (pt.smooth) { float l = glm::length(pt.handleIn); if (l<0.001f) l=glm::length(pt.handleOut); pt.handleIn = glm::normalize(-pt.handleOut)*l; }
            }
            mask.markDirty();
        }
    }
    if (ImGui::IsMouseReleased(ImGuiMouseButton_Left)) { m_maskDragType = 0; m_maskDragIndex = -1; }

    if (ImGui::IsMouseClicked(ImGuiMouseButton_Right) && m_hovered) {
        int pt = mask.hitTestPoint(mouseUV, 0.03f); if (pt >= 0) { mask.removePoint(pt); m_maskSelectedPoint = -1; }
    }
    if (m_maskSelectedPoint >= 0 && m_maskSelectedPoint < mask.count()) {
        if (ImGui::IsKeyPressed(ImGuiKey_Delete) || ImGui::IsKeyPressed(ImGuiKey_Backspace)) { mask.removePoint(m_maskSelectedPoint); m_maskSelectedPoint = -1; }

        // Arrow key nudge selected mask point (1 pixel in layer UV space)
        if (!ImGui::GetIO().WantTextInput && m_maskSelectedPoint >= 0 && m_maskSelectedPoint < mask.count()) {
            auto& pts = mask.points();
            // 1 pixel nudge: approximate as 1/canvas_size in UV, use image pixel size
            float nudgeX = 1.0f / std::max(1.0f, m_imageSize.x);
            float nudgeY = 1.0f / std::max(1.0f, m_imageSize.y);
            bool nudged = false;
            if (ImGui::IsKeyPressed(ImGuiKey_LeftArrow))  { pts[m_maskSelectedPoint].position.x -= nudgeX; nudged = true; }
            if (ImGui::IsKeyPressed(ImGuiKey_RightArrow)) { pts[m_maskSelectedPoint].position.x += nudgeX; nudged = true; }
            if (ImGui::IsKeyPressed(ImGuiKey_UpArrow))    { pts[m_maskSelectedPoint].position.y += nudgeY; nudged = true; }
            if (ImGui::IsKeyPressed(ImGuiKey_DownArrow))  { pts[m_maskSelectedPoint].position.y -= nudgeY; nudged = true; }
            if (nudged) {
                pts[m_maskSelectedPoint].position = glm::clamp(pts[m_maskSelectedPoint].position, glm::vec2(0.0f), glm::vec2(1.0f));
                mask.markDirty();
            }
        }
    }
    if (ImGui::IsKeyPressed(ImGuiKey_Escape) && !ImGui::IsAnyItemActive()) {
        if (m_maskSelectedPoint >= 0) {
            // First Esc: clear mask-point selection
            m_maskSelectedPoint = -1;
        } else {
            // Second Esc (or no point selected): request exit from mask mode
            m_wantsExitMaskMode = true;
        }
    }

    const auto& pts = mask.points();

    // Mask edit mode visual vocabulary: amber/yellow distinct from cyan transform handles
    // Mask overlay colors: zone-colored for canvas masks, gold for layer masks
    ImU32 kMFill, kMCurveGlow, kMCurve, kMHandleLine, kMHandleDot, kMHandleRing;
    ImU32 kMPointFill, kMPointRing, kMSelFill, kMSelRing, kMSelGlow;
    if (zoneIndex >= 0) {
        auto c = kZoneColors[zoneIndex % 8];
        // Inside of mask = NO color overlay so the user sees the canvas
        // exactly as it will look after Save. The outside is dimmed to
        // 25% via a separate pass below — that's the "this will be cut"
        // affordance, not the inside fill.
        kMFill       = IM_COL32(0, 0, 0, 0);
        kMCurveGlow  = IM_COL32(c.r, c.g, c.b, 55);
        kMCurve      = IM_COL32(c.r, c.g, c.b, 230);
        kMHandleLine = IM_COL32(c.r, c.g, c.b, 110);
        // Handle/point dots themselves are gold in mask-edit mode so the
        // whole handle set reads consistently (curve/glow stay zone-colored).
        kMHandleDot  = kMaskEditFill;
        kMHandleRing = kMaskEditRing;
        kMPointFill  = kMaskEditFill;
        kMPointRing  = kMaskEditRing;
        kMSelFill    = IM_COL32(std::min(c.r+40,255), std::min(c.g+40,255), std::min(c.b+40,255), 255);
        kMSelRing    = IM_COL32(255, 255, 255, 255);
        kMSelGlow    = IM_COL32(c.r, c.g, c.b, 60);
    } else {
        kMFill       = IM_COL32(0, 0, 0, 0); // no inside fill — see canvas-mask branch comment
        kMCurveGlow  = IM_COL32(255, 200, 60, 55);
        kMCurve      = IM_COL32(255, 210, 90, 230);
        kMHandleLine = IM_COL32(255, 200, 60, 110);
        kMHandleDot  = kMaskEditFill;
        kMHandleRing = kMaskEditRing;
        kMPointFill  = kMaskEditFill;
        kMPointRing  = kMaskEditRing;
        kMSelFill    = IM_COL32(255, 240, 120, 255);
        kMSelRing    = IM_COL32(255, 255, 255, 255);
        kMSelGlow    = IM_COL32(255, 200, 60, 60);
    }

    // Edit mode banner (top-right of viewport) + Save button.
    {
        const char* banner = mask.closed()
            ? "MASK EDIT  |  Shift+Click: multi-select  •  Drag: move  •  R-click: del"
            : "MASK EDIT  |  Click: add points  •  Click first point to close  •  R-click: del";
        const char* saveLbl = "Save";
        ImVec2 ts = ImGui::CalcTextSize(banner);
        ImVec2 sts = ImGui::CalcTextSize(saveLbl);
        float pad = 8.0f;
        draw->AddRectFilled(bannerMin, bannerMax, IM_COL32(40, 30, 10, 200), 3.0f);
        draw->AddRect(bannerMin, bannerMax, IM_COL32(255, 200, 60, 180), 3.0f, 0, 1.5f);
        draw->AddText(ImVec2(bannerMin.x + pad, bannerMin.y + pad * 0.5f),
                      IM_COL32(255, 220, 120, 255), banner);

        bool saveHov = ptInRect(mousePos, saveBtnMin, saveBtnMax);
        ImU32 btnBg = saveHov ? IM_COL32(255, 210, 110, 230) : IM_COL32(255, 200, 60, 180);
        ImU32 btnTx = saveHov ? IM_COL32(40, 25, 0, 255)      : IM_COL32(60, 40, 10, 255);
        draw->AddRectFilled(saveBtnMin, saveBtnMax, btnBg, 3.0f);
        float saveBtnW = saveBtnMax.x - saveBtnMin.x;
        float saveBtnH = saveBtnMax.y - saveBtnMin.y;
        draw->AddText(ImVec2(saveBtnMin.x + (saveBtnW - sts.x) * 0.5f,
                             saveBtnMin.y + (saveBtnH - sts.y) * 0.5f), btnTx, saveLbl);
        if (saveHov && ImGui::IsMouseClicked(ImGuiMouseButton_Left)) {
            m_wantsExitMaskMode = true;
        }
    }

    if (pts.empty()) { draw->PopClipRect(); return; }

    // Closed mask: dim the OUTSIDE area to ~25% black so the user can
    // preview "this is what gets cut after Save" without coloring the
    // inside. Implemented as 4 axis-aligned quads bounding the polygon's
    // bbox — covers the usual rectangular use cases. Concave-polygon
    // gaps in the bbox→polygon corners are acceptable for a preview.
    if (pts.size() >= 3 && mask.closed()) {
        // Bbox of the polygon in screen space.
        float minX = FLT_MAX, minY = FLT_MAX, maxX = -FLT_MAX, maxY = -FLT_MAX;
        for (const auto& p : pts) {
            ImVec2 sp = toImVec2(layerUVToScreen(p.position));
            if (sp.x < minX) minX = sp.x;
            if (sp.y < minY) minY = sp.y;
            if (sp.x > maxX) maxX = sp.x;
            if (sp.y > maxY) maxY = sp.y;
        }
        const ImU32 kDim = IM_COL32(0, 0, 0, 64);  // ~25% black
        // Top band, bottom band, left band, right band — together they
        // cover the canvas area outside the polygon's bounding rect.
        draw->AddRectFilled(ImVec2(m_panelMin.x, m_panelMin.y),
                            ImVec2(m_panelMax.x, minY), kDim);
        draw->AddRectFilled(ImVec2(m_panelMin.x, maxY),
                            ImVec2(m_panelMax.x, m_panelMax.y), kDim);
        draw->AddRectFilled(ImVec2(m_panelMin.x, minY),
                            ImVec2(minX,         maxY), kDim);
        draw->AddRectFilled(ImVec2(maxX,         minY),
                            ImVec2(m_panelMax.x, maxY), kDim);
        // Inside fill (kMFill is now alpha=0, so this is a no-op — left
        // in place for future re-introduction with a different vocab).
        auto tv = mask.tessellate(24);
        if (tv.size() >= 3) {
            ImVec2 cs = toImVec2(layerUVToScreen(mask.centroid()));
            for (int i = 0; i < (int)tv.size(); i++) {
                int j = (i+1) % (int)tv.size();
                draw->AddTriangleFilled(cs, toImVec2(layerUVToScreen(tv[i])),
                                        toImVec2(layerUVToScreen(tv[j])), kMFill);
            }
        }
    }

    // Open path with 1+ points: show "close" hint on first point
    if (!mask.closed() && pts.size() >= 3) {
        ImVec2 firstPt = toImVec2(layerUVToScreen(pts[0].position));
        draw->AddCircle(firstPt, 12.0f, IM_COL32(255, 200, 60, 180), 0, 2.0f);
        draw->AddCircleFilled(firstPt, 12.0f, IM_COL32(255, 200, 60, 40));
        // "Close" label
        draw->AddText(ImVec2(firstPt.x + 14, firstPt.y - 8), IM_COL32(255, 220, 120, 255), "Close");
    }
    if (pts.size() >= 2) {
        int n = (int)pts.size(), edges = mask.closed() ? n : (n-1);
        for (int i = 0; i < edges; i++) {
            int j = (i+1)%n;
            ImVec2 p0=toImVec2(layerUVToScreen(pts[i].position)), c0=toImVec2(layerUVToScreen(pts[i].position+pts[i].handleOut));
            ImVec2 c1=toImVec2(layerUVToScreen(pts[j].position+pts[j].handleIn)), p1=toImVec2(layerUVToScreen(pts[j].position));
            draw->AddBezierCubic(p0,c0,c1,p1,kMCurveGlow,5.0f,32);
            draw->AddBezierCubic(p0,c0,c1,p1,kMCurve,1.8f,32);
        }

        // Hover ghost "+" on edge midpoints for discoverable point insertion
        float tParam; int hoverEdge = mask.hitTestEdge(mouseUV, 0.04f, tParam);
        if (hoverEdge >= 0 && m_maskDragType == 0) {
            // Find the midpoint on the edge at t=0.5 for visual hint
            int i = hoverEdge, j = (hoverEdge + 1) % n;
            glm::vec2 mid = MaskPath::evalBezier(
                pts[i].position,
                pts[i].position + pts[i].handleOut,
                pts[j].position + pts[j].handleIn,
                pts[j].position,
                0.5f);
            ImVec2 mp = toImVec2(layerUVToScreen(mid));
            draw->AddCircleFilled(mp, 8.0f, IM_COL32(255, 220, 120, 80));
            draw->AddCircle(mp, 8.0f, IM_COL32(255, 230, 160, 200), 0, 1.5f);
            // Plus icon
            draw->AddLine(ImVec2(mp.x - 4, mp.y), ImVec2(mp.x + 4, mp.y), IM_COL32(255, 250, 200, 255), 1.8f);
            draw->AddLine(ImVec2(mp.x, mp.y - 4), ImVec2(mp.x, mp.y + 4), IM_COL32(255, 250, 200, 255), 1.8f);
        }
    }
    for (int i = 0; i < (int)pts.size(); i++) {
        ImVec2 anch = toImVec2(layerUVToScreen(pts[i].position));
        bool isSel = (i == m_maskSelectedPoint) || isSelected(i);
        if (glm::length(pts[i].handleIn) > 0.001f) {
            ImVec2 h = toImVec2(layerUVToScreen(pts[i].position+pts[i].handleIn));
            draw->AddLine(anch,h,kMHandleLine,1.0f); draw->AddCircleFilled(h,3.5f,kMHandleDot); draw->AddCircle(h,3.5f,kMHandleRing,0,1.2f);
        }
        if (glm::length(pts[i].handleOut) > 0.001f) {
            ImVec2 h = toImVec2(layerUVToScreen(pts[i].position+pts[i].handleOut));
            draw->AddLine(anch,h,kMHandleLine,1.0f); draw->AddCircleFilled(h,3.5f,kMHandleDot); draw->AddCircle(h,3.5f,kMHandleRing,0,1.2f);
        }
        if (isSel) {
            float s=5.5f;
            draw->AddQuadFilled(ImVec2(anch.x,anch.y-s-3),ImVec2(anch.x+s+3,anch.y),ImVec2(anch.x,anch.y+s+3),ImVec2(anch.x-s-3,anch.y),kMSelGlow);
            draw->AddQuadFilled(ImVec2(anch.x,anch.y-s),ImVec2(anch.x+s,anch.y),ImVec2(anch.x,anch.y+s),ImVec2(anch.x-s,anch.y),kMSelFill);
            draw->AddQuad(ImVec2(anch.x,anch.y-s),ImVec2(anch.x+s,anch.y),ImVec2(anch.x,anch.y+s),ImVec2(anch.x-s,anch.y),kMSelRing,1.5f);
        } else {
            draw->AddCircleFilled(anch,4.5f,kMPointFill); draw->AddCircle(anch,4.5f,kMPointRing,0,1.2f);
        }
    }

    draw->PopClipRect();
}

// --- Shared secondary nav bar — CANVAS/STAGE pills + zones + OUTPUT + comp
//     chip + Fullscreen. Called from both Canvas (stageActive=false) and
//     Stage (stageActive=true) so switching workspaces never shifts the
//     element geometry.
void ViewportPanel::renderNavBar(bool stageActive,
                                 std::vector<std::unique_ptr<OutputZone>>* zones,
                                 int* activeZone,
                                 const std::vector<MonitorInfo>* monitors,
                                 bool ndiAvailable,
                                 int editorMonitor,
                                 std::function<void()> prefixContent) {
    if (!zones || !activeZone) return;

    // Nav row height = 28 so its 28-tall items (gear, System Audio pill,
    // workspace pill, etc.) center exactly on y=14 — the same vertical line
    // the macOS traffic-light buttons live on (titlebar = 28, lights centered
    // → y=14). No dummy spacer, no separate strip color: the row sits directly
    // on the Canvas window's dark-gray background so there's no black band
    // visible above the canvas content on launch.
    const float kNavRowH = 28.0f;
    // Capture the actual screen-Y of the nav row's first drawable position
    // BEFORE any item advances the cursor. This is the canonical center-line
    // for every element in the row: the hamburger glyph (drawn via
    // GetCursorScreenPos in renderNavBarPrefix), the Canvas/Stage/Show tabs
    // (drawn at rowPos.y + kPillH * 0.5), and the floating right-cluster
    // window (anchored explicitly to this Y below). Using viewport->Pos.y
    // can drift if ImGui adds an internal title-bar reserve to the docked
    // window, so we use the live cursor instead.
    const float navRowScreenY = ImGui::GetCursorScreenPos().y;

    // The top nav must visually win against EVERY other surface — no
    // floating panel, no docked window, no overlay should ever paint over
    // it. Routing the band AND every nav glyph through the viewport
    // FOREGROUND draw list (rendered last, after all windows) gives it
    // an absolute z-priority. The geometric clamp in UIManager still
    // keeps panels out of the nav-row Y range, but z-priority is the
    // belt-and-suspenders guarantee.
    ImGuiViewport* vpFG = ImGui::GetMainViewport();
    ImDrawList* navFG = ImGui::GetForegroundDrawList(vpFG);
    {
        navFG->AddRectFilled(
            ImVec2(vpFG->Pos.x, navRowScreenY),
            ImVec2(vpFG->Pos.x + vpFG->Size.x, navRowScreenY + kNavRowH),
            IM_COL32(0x20, 0x24, 0x28, 255));
        navFG->AddLine(
            ImVec2(vpFG->Pos.x,                  navRowScreenY + kNavRowH),
            ImVec2(vpFG->Pos.x + vpFG->Size.x,   navRowScreenY + kNavRowH),
            IM_COL32(255, 255, 255, 28), 1.0f);
    }

    // Traffic-light clearance is owned by Application::renderNavBarPrefix
    // (it knows about macOS fullscreen state, where AppKit hides the lights
    // and the inset should collapse). Don't double-stack an Indent here —
    // earlier the 78px Indent + 70px inset put the gear at x=148, leaving a
    // wide dead band between the green light and the gear that read as a
    // dark "shape". Keep ViewportPanel agnostic; only inset 12px so non-
    // prefix nav items get the same edge breathing room as the timeline.
    ImGui::Indent(12);
    // Prefix cluster — Application injects the brand mark + ellipsis/overflow
    // menu here so the row carries the chrome (no separate ImGui menu bar
    // above the viewport). Keep it on the same line as Canvas/Stage/Show
    // and let the pill below center using the FULL row width, not the
    // remaining-after-prefix width — so the pill stays visually centered
    // regardless of prefix width.
    if (prefixContent) {
        prefixContent();
        ImGui::SameLine(0, 0);
    }
    ImDrawList* tabDraw = ImGui::GetWindowDrawList();

    // CANVAS / STAGE / SHOW — Figma-style text tabs with a thin underline
    // under the active label. No track, no pill, no segmented background.
    // Active = full-saturated text + 2px underline; inactive = muted text;
    // hover lifts the muted text toward white.
    {
        using Mode = UIManager::WorkspaceMode;
        Mode mode = UIManager::sMode;

        // Slightly smaller workspace labels so they share the same visual
        // weight as the right cluster (Main / OUTPUT / 1920x1080 chip) and
        // sit one notch lighter than the hamburger / traffic lights.
        ImGui::SetWindowFontScale(0.93f);

        const float kPillH       = 28.0f;
        const float kSegGap      = 26.0f;   // breathing room between segments
        // Play hidden from the workspace switcher per Lu's request — the
        // mode/panel code is untouched (still reachable over OSC), just not
        // exposed as a tab here.
        const int   kNumTabs     = 3;
        const char* labels[kNumTabs] = {"CANVAS", "MAPPING", "ZONES"};
        Mode      modes[kNumTabs]    = {Mode::Canvas, Mode::Mapping, Mode::Zones};

        // Each segment = label width. Icons removed — the workspace tabs
        // now read as text-only, mirroring the rest of the nav row's
        // borderless typographic style.
        float segW[kNumTabs];
        float totalW = 0.0f;
        for (int i = 0; i < kNumTabs; i++) {
            segW[i] = ImGui::CalcTextSize(labels[i]).x;
            totalW += segW[i];
            if (i < kNumTabs - 1) totalW += kSegGap;
        }

        // Left-align the workspace tabs — they sit a comfortable distance
        // after the prefix cluster (settings gear / System Audio) so the
        // two groups read as separate clusters, not one mashed strip.
        ImGui::Dummy(ImVec2(24.0f, 0));
        ImGui::SameLine(0, 0);

        ImVec2 rowPos = ImGui::GetCursorScreenPos();
        // Workspace tab labels paint into the foreground draw list so they
        // sit above any panel that might be near the nav row in z-order.
        ImDrawList* dl = navFG;
        int activeIdx = 0;
        for (int i = 0; i < kNumTabs; i++) {
            if (modes[i] == mode) { activeIdx = i; break; }
        }

        // Pre-allocate the entire row's footprint with a single Dummy.
        // ImGui asserts at frame end if SetCursorScreenPos pushes the cursor
        // past content bounds without a subsequent item that grows them, so
        // we declare the bounds upfront, then drop the per-tab InvisibleButtons
        // inside the now-already-claimed rect with AllowOverlap.
        const float kRowPadRight = 8.0f;
        ImGui::Dummy(ImVec2(totalW + kRowPadRight, kPillH));

        // Vertical centerline for the row. Label baselines hang off this
        // so the segment reads as a single horizontal run.
        float yCenter = rowPos.y + kPillH * 0.5f;

        float x = rowPos.x;
        for (int i = 0; i < kNumTabs; i++) {
            ImGui::SetCursorScreenPos(ImVec2(x, rowPos.y));
            ImGui::PushID(i);
            ImGui::SetNextItemAllowOverlap();
            bool clicked = ImGui::InvisibleButton("##wsTab", ImVec2(segW[i], kPillH));
            bool hov     = ImGui::IsItemHovered();
            ImGui::PopID();
            if (clicked) UIManager::setMode(modes[i]);

            ImVec2 ts = ImGui::CalcTextSize(labels[i]);
            bool isActive = (i == activeIdx);
            ImU32 fg = isActive
                ? IM_COL32(255, 255, 255, 255)
                : (hov ? IM_COL32(200, 208, 220, 230)
                       : IM_COL32(135, 142, 158, 200));

            // ── Label (vertically centered on yCenter) ─────────────
            float lblX = x;
            float lblY = yCenter - ts.y * 0.5f;
            dl->AddText(ImVec2(lblX, lblY), fg, labels[i]);
            // Faux-bold for the active label
            if (isActive) {
                dl->AddText(ImVec2(lblX + 0.6f, lblY), fg, labels[i]);
            }

            x += segW[i];
            if (i < kNumTabs - 1) x += kSegGap;
        }

        // Reset cursor to the right edge of the row's pre-allocated rect.
        // SetCursorPos (window-relative) avoids the SetCursorScreenPos
        // boundary-extension assertion entirely.
        float windowRelX = (rowPos.x + totalW + kRowPadRight)
                         - ImGui::GetWindowPos().x;
        float windowRelY = rowPos.y - ImGui::GetWindowPos().y;
        ImGui::SetCursorPos(ImVec2(windowRelX, windowRelY));
        ImGui::SameLine(0, 0);
        // Reset font scale so the rest of the canvas window renders at
        // its normal size — the smaller scale only applies to the
        // CANVAS/STAGE/SHOW row.
        ImGui::SetWindowFontScale(1.0f);
    }

    // Stage workspace: skip the entire zones / OUTPUT cluster on the right.
    // Stage is a 3D pre-viz surface — zone routing and OUTPUT destination
    // are Canvas concerns. The only right-side affordance Stage needs is
    // System Audio (the source the workspace listens to). The settings
    // gear is already drawn on the left via prefixContent.
    if (stageActive) {
        // Right-align a single "System Audio" pill identical to the one
        // in the floating transport pill, opening the same picker popup.
        ImGui::Unindent(12);
        // Reuse the workspace-pill's window-wide centering math so the
        // System Audio pill lines up against the right edge.
        float winW = ImGui::GetWindowContentRegionMax().x
                   - ImGui::GetWindowContentRegionMin().x;
        const float kPillH = 30.0f;
        const float kPadX  = 14.0f;
        const char* label  = "System Audio";
        float labelW = ImGui::CalcTextSize(label).x;
        float pillW  = labelW + kPadX * 2.0f + 18.0f; // 18 = chevron column
        // Set cursor to right edge inset by 12px so it visually mirrors
        // the workspace nav's left inset.
        ImGui::SetCursorPosX(ImGui::GetWindowContentRegionMin().x + winW - pillW - 12.0f);
        ImVec2 cur = ImGui::GetCursorScreenPos();
        ImDrawList* dl = ImGui::GetWindowDrawList();
        bool clicked = ImGui::InvisibleButton("##stage_audio", ImVec2(pillW, kPillH));
        bool hov     = ImGui::IsItemHovered();
        ImU32 bg = hov ? IM_COL32(255, 255, 255, 22)
                       : IM_COL32(255, 255, 255, 10);
        dl->AddRectFilled(cur, ImVec2(cur.x + pillW, cur.y + kPillH),
                          bg, kPillH * 0.5f);
        dl->AddRect(cur, ImVec2(cur.x + pillW, cur.y + kPillH),
                    IM_COL32(255, 255, 255, 28),
                    kPillH * 0.5f, 0, 1.0f);
        ImVec2 ts = ImGui::CalcTextSize(label);
        float yMid = cur.y + kPillH * 0.5f;
        dl->AddText(ImVec2(cur.x + kPadX, yMid - ts.y * 0.5f),
                    IM_COL32(232, 238, 250, 240), label);
        // Chevron-down on the right
        float cxv = cur.x + pillW - kPadX - 6.0f;
        dl->AddTriangleFilled(
            ImVec2(cxv - 4.0f, yMid - 2.0f),
            ImVec2(cxv + 4.0f, yMid - 2.0f),
            ImVec2(cxv,        yMid + 3.0f),
            IM_COL32(170, 178, 195, 220));
        if (clicked) ImGui::OpenPopup("##stage_sysaudio_popup");
        // (Popup contents are wired separately in Application.cpp where
        // the audio device list lives — this nav row only owns the
        // visible pill. Click-through opens the same popup name.)
        ImGui::Indent(12);
        return;
    }

    // (void) silences unused warnings for params that are only consumed by
    // the floating zone+output dock — kept on the signature so the existing
    // call sites don't need refactoring.
    (void)tabDraw;
    (void)monitors;
    (void)ndiAvailable;
    (void)editorMonitor;

    // Right cluster — zones (Main / + ) + OUTPUT combo + composition chip
    // + fullscreen icon. AlwaysAutoResize + top-right pivot anchor lets
    // the window grow leftward as the active zone name / OUTPUT label
    // change length, without us having to compute width by hand.
    int ai = *activeZone;
    if (ai >= 0 && ai < (int)zones->size()) {
        auto& az = *(*zones)[ai];

        char compLabel[48];
        snprintf(compLabel, sizeof(compLabel), "%d x %d", az.width, az.height);
        ImVec2 lblSz = ImGui::CalcTextSize(compLabel);
        const float fsBtnSize = 28.0f;
        const float gap = 8.0f;
        const float caretW = 10.0f;        // chevron + breathing space
        const float caretGap = 4.0f;       // gap between label and chevron
        float chipW = lblSz.x + caretGap + caretW;

        ImGuiViewport* vp = ImGui::GetMainViewport();
        const float kRightInset = 8.0f;
        const float kBtn = 28.0f;
        // Pivot (1, 0) anchors the window's TOP-RIGHT to the given point so
        // the cluster grows leftward as content gets wider. The cluster is
        // composed entirely of InvisibleButton(fsBtnSize=28) + manually
        // centred text/glyphs, so its content already lives in a 28px band
        // identical to the workspace tabs and the macOS traffic-light row.
        // Anchor at navRowScreenY (no Y offset) so all three groups —
        // traffic lights, CANVAS/STAGE/SHOW, and the right cluster — share
        // exactly one vertical centerline.
        float anchorX = vp->Pos.x + vp->Size.x - kRightInset;
        float anchorY = navRowScreenY;

        ImGui::SetNextWindowPos(ImVec2(anchorX, anchorY),
                                ImGuiCond_Always, ImVec2(1.0f, 0.0f));
        ImGui::SetNextWindowViewport(vp->ID);
        ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding,    ImVec2(0, 0));
        ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding,   0.0f);
        ImGui::PushStyleVar(ImGuiStyleVar_WindowBorderSize, 0.0f);
        ImGui::PushStyleColor(ImGuiCol_WindowBg, IM_COL32(0, 0, 0, 0));
        ImGuiWindowFlags fr = ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoResize
            | ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoCollapse | ImGuiWindowFlags_NoDocking
            | ImGuiWindowFlags_NoSavedSettings | ImGuiWindowFlags_NoScrollbar
            | ImGuiWindowFlags_AlwaysAutoResize;
        if (ImGui::Begin("##NavRightCluster", nullptr, fr)) {
        // Slightly smaller text across the entire right cluster so the
        // labels (Main / OUTPUT / Preview Only / 1920x1080) read as a
        // compact secondary row instead of competing with body content.
        ImGui::SetWindowFontScale(0.93f);
        ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(12, 3));
        // Bumped 8 → 16 so MAIN / + / OUTPUT / PREVIEW ONLY / 1920x1080 /
        // fullscreen all have breathing room between them.
        ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing, ImVec2(16.0f, 2));

        // ── Zone tabs (MAIN / ZONE 2 / + ) — borderless text-only style ──
        // Active zone reads as full-saturated white text; inactive zones as
        // muted gray. No pill background, no chip, no rounded fill — the
        // active state is carried by text weight/colour alone, mirroring
        // CANVAS/STAGE/SHOW. The "+" is a thin 12px glyph, no disc.
        // Right-cluster glyphs route to the foreground draw list so they
        // can never be obscured by a floating panel that brushes the
        // nav-row Y range.
        ImDrawList* tabDraw = navFG;
        for (int zi = 0; zi < (int)zones->size(); zi++) {
            ImGui::PushID(9000 + zi);
            bool isActive = (zi == *activeZone);
            auto& z = *(*zones)[zi];

            std::string zNameUpper = z.name;
            for (char& ch : zNameUpper) ch = (char)std::toupper((unsigned char)ch);
            ImVec2 lblSz = ImGui::CalcTextSize(zNameUpper.c_str());

            ImVec2 cur = ImGui::GetCursorScreenPos();
            // Hit area = label width + 6px breathing room on each side; row
            // height matches fsBtnSize so the centerline aligns with OUTPUT,
            // PREVIEW ONLY, the resolution chip, and the fullscreen icon.
            float hitW = lblSz.x + 12.0f;
            bool clicked = ImGui::InvisibleButton("##zoneTab", ImVec2(hitW, fsBtnSize));
            bool hov     = ImGui::IsItemHovered();

            ImU32 fg = isActive
                ? IM_COL32(245, 247, 252, 255)
                : (hov ? IM_COL32(220, 225, 235, 230)
                       : IM_COL32(150, 158, 175, 200));
            float yMid = cur.y + fsBtnSize * 0.5f;
            tabDraw->AddText(ImVec2(cur.x + 6.0f, yMid - lblSz.y * 0.5f),
                             fg, zNameUpper.c_str());

            // Routing indicator — small dot to the left of the label when
            // this zone is sending output to a destination.
            if (z.outputDest == OutputDest::Fullscreen || z.outputDest == OutputDest::NDI) {
                ImU32 dotCol = IM_COL32(85, 210, 130, 245);
                tabDraw->AddCircleFilled(ImVec2(cur.x + 2.0f, yMid), 2.0f, dotCol, 12);
            }

            if (clicked) *activeZone = zi;
            if (ImGui::IsItemHovered() && ImGui::IsMouseDoubleClicked(0)) {
                m_renaming = true;
                m_renameIndex = zi;
                strncpy(m_renameBuf, z.name.c_str(), sizeof(m_renameBuf) - 1);
                m_renameBuf[sizeof(m_renameBuf) - 1] = '\0';
            }
            if (ImGui::BeginPopupContextItem("ZoneTabCtx")) {
                if (ImGui::MenuItem("Rename")) {
                    m_renaming = true;
                    m_renameIndex = zi;
                    strncpy(m_renameBuf, z.name.c_str(), sizeof(m_renameBuf) - 1);
                    m_renameBuf[sizeof(m_renameBuf) - 1] = '\0';
                }
                if (ImGui::MenuItem("Duplicate")) *activeZone = -(200 + zi);
                if ((int)zones->size() > 1) {
                    if (ImGui::MenuItem("Remove")) *activeZone = -(300 + zi);
                }
                ImGui::EndPopup();
            }
            ImGui::SameLine(0, 0);
            ImGui::PopID();
        }
        if (m_renaming) ImGui::OpenPopup("##NavRenameZone");
        if (ImGui::BeginPopup("##NavRenameZone")) {
            ImGui::Text("Rename Zone");
            ImGui::SetNextItemWidth(200);
            bool enter = ImGui::InputText("##NavRenameInput", m_renameBuf, sizeof(m_renameBuf),
                ImGuiInputTextFlags_EnterReturnsTrue | ImGuiInputTextFlags_AutoSelectAll);
            if (m_renaming) { ImGui::SetKeyboardFocusHere(-1); m_renaming = false; }
            if (enter || ImGui::IsKeyPressed(ImGuiKey_Escape)) {
                if (enter && m_renameIndex >= 0 && m_renameIndex < (int)zones->size() && m_renameBuf[0]) {
                    (*zones)[m_renameIndex]->name = m_renameBuf;
                }
                m_renameIndex = -1;
                ImGui::CloseCurrentPopup();
            }
            ImGui::EndPopup();
        }
        // ── + (add zone) — borderless thin plus glyph, no disc ─────────
        {
            const float kAddW = fsBtnSize;        // matches the row band height
            ImVec2 cur = ImGui::GetCursorScreenPos();
            bool clicked = ImGui::InvisibleButton("##addZone", ImVec2(kAddW, fsBtnSize));
            bool hov     = ImGui::IsItemHovered();
            ImDrawList* dAdd = navFG;
            float cx = cur.x + kAddW * 0.5f;
            float cy = cur.y + fsBtnSize * 0.5f;
            ImU32 col = hov ? IM_COL32(235, 240, 250, 240)
                            : IM_COL32(150, 158, 175, 220);
            float a = 5.0f;        // half-length of each plus arm
            float th = 1.4f;       // stroke thickness, matches the chevrons
            dAdd->AddLine(ImVec2(cx - a, cy), ImVec2(cx + a, cy), col, th);
            dAdd->AddLine(ImVec2(cx, cy - a), ImVec2(cx, cy + a), col, th);
            if (clicked) *activeZone = -(100 + (int)zones->size());
            if (ImGui::IsItemHovered()) ParamRow::Tooltip("Add output zone");
        }

        // ── OUTPUT label + destination combo ────────────────────────
        // Bumped 14 → 24 to give the zone/+ cluster more breathing room
        // before the OUTPUT cluster — they read as separate groups now.
        ImGui::SameLine(0, 24);
        {
            static char destBuf[128] = {};
            const char* destLabel = "PREVIEW ONLY";
            if (az.outputDest == OutputDest::Fullscreen && monitors) {
                int mi = az.outputMonitor;
                if (mi >= 0 && mi < (int)monitors->size()) {
                    snprintf(destBuf, sizeof(destBuf), "FULLSCREEN: %s",
                             (*monitors)[mi].name.c_str());
                    destLabel = destBuf;
                }
            } else if (az.outputDest == OutputDest::NDI) {
                snprintf(destBuf, sizeof(destBuf), "NDI: \"%s\"",
                         az.ndiStreamName.empty() ? az.name.c_str()
                                                  : az.ndiStreamName.c_str());
                destLabel = destBuf;
            }
            bool live = (az.outputDest != OutputDest::None);

            // OUTPUT label — drawn manually, vertically centered inside the
            // same fsBtnSize-tall band the composition chip + fullscreen
            // icon use. This puts the OUTPUT label, the PREVIEW ONLY chip,
            // the 1920x1080 chip and the fullscreen icon on a single
            // shared centerline.
            {
                const char* outLbl = "OUTPUT";
                ImVec2 outSz = ImGui::CalcTextSize(outLbl);
                ImVec2 cur = ImGui::GetCursorScreenPos();
                ImGui::Dummy(ImVec2(outSz.x, fsBtnSize));
                ImU32 outCol = live ? IM_COL32(56, 209, 132, 245)
                                    : IM_COL32(115, 128, 149, 245);
                float yMid = cur.y + fsBtnSize * 0.5f;
                navFG->AddText(
                    ImVec2(cur.x, yMid - outSz.y * 0.5f), outCol, outLbl);
            }
            ImGui::SameLine(0, 6);

            // Chip-style picker (no frame, no background) — same family as
            // the aspect-ratio chip beside it: label + tiny chevron, click
            // opens a popup. The chip uses fsBtnSize so it shares the exact
            // vertical centerline of the composition chip and fullscreen
            // icon to its right.
            ImVec2 outChipBR;  // bottom-right of trigger — anchors popup
            {
                ImVec2 lblSz = ImGui::CalcTextSize(destLabel);
                const float chevW   = 7.0f;
                const float chevGap = 4.0f;
                float chipW = lblSz.x + chevGap + chevW;
                // Match the composition chip + fullscreen icon height so all
                // three controls share the same vertical centerline.
                float chipH = fsBtnSize;

                ImVec2 cur = ImGui::GetCursorScreenPos();
                bool clicked = ImGui::InvisibleButton("##NavOutputChip",
                                                     ImVec2(chipW, chipH));
                bool hov = ImGui::IsItemHovered();
                ImDrawList* dOut = navFG;

                ImU32 textCol;
                if (live)      textCol = IM_COL32(85, 210, 130, 245);
                else if (hov)  textCol = IM_COL32(235, 240, 250, 240);
                else           textCol = IM_COL32(170, 175, 185, 230);

                float yMid = cur.y + chipH * 0.5f;
                dOut->AddText(ImVec2(cur.x, yMid - lblSz.y * 0.5f),
                              textCol, destLabel);
                // tiny chevron-down — three lines forming a triangle
                float cxv = cur.x + lblSz.x + chevGap + chevW * 0.5f;
                float chevY = yMid - 0.5f;
                dOut->AddTriangleFilled(
                    ImVec2(cxv - chevW * 0.5f, chevY - 1.5f),
                    ImVec2(cxv + chevW * 0.5f, chevY - 1.5f),
                    ImVec2(cxv,                chevY + 2.5f),
                    textCol);

                outChipBR = ImVec2(cur.x + chipW, cur.y + chipH);
                if (clicked) ImGui::OpenPopup("##NavOutputPopup");
            }
            // Anchor the popup's TOP-RIGHT to the trigger's bottom-right
            // corner (pivot 1,0) so it grows LEFTWARD — keeps the popup
            // fully on-screen even when the trigger sits flush against
            // the viewport's right edge. Without this, a 320px popup
            // anchored to a near-right-edge trigger gets clipped.
            ImGui::SetNextWindowPos(outChipBR, ImGuiCond_Always, ImVec2(1.0f, 0.0f));
            ImGui::SetNextWindowSize(ImVec2(320, 0), ImGuiCond_Always);
            ImGui::SetNextWindowViewport(ImGui::GetMainViewport()->ID);
            ImGui::SetNextWindowFocus();
            ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding,    ImVec2(14, 12));
            ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing,      ImVec2(10, 9));
            ImGui::PushStyleVar(ImGuiStyleVar_FramePadding,     ImVec2(8, 5));
            ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding,   8.0f);
            ImGui::PushStyleColor(ImGuiCol_PopupBg, IM_COL32(8, 9, 12, 255));
            ImGui::PushStyleColor(ImGuiCol_Border,  IM_COL32(255, 255, 255, 22));
            bool navOutputPopupOpen = ImGui::BeginPopup("##NavOutputPopup");
            ImGui::PopStyleColor(2);
            ImGui::PopStyleVar(4);
            if (navOutputPopupOpen) {
                if (ImGui::Selectable("Preview Only", az.outputDest == OutputDest::None)) {
                    az.outputDest = OutputDest::None;
                    az.outputMonitor = -1;
                }
                if (monitors && !monitors->empty()) {
                    ImGui::Separator();
                    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 0.7f));
                    ImGui::Text("  Fullscreen");
                    ImGui::PopStyleColor();
                    for (int mi = 0; mi < (int)monitors->size(); mi++) {
                        ImGui::PushID(mi);
                        if (mi == editorMonitor) { ImGui::PopID(); continue; }
                        std::string claimedBy;
                        for (int zi = 0; zi < (int)zones->size(); zi++) {
                            if (zi == ai) continue;
                            auto& oz = *(*zones)[zi];
                            if (oz.outputDest == OutputDest::Fullscreen && oz.outputMonitor == mi) {
                                claimedBy = oz.name; break;
                            }
                        }
                        char label[256];
                        if (!claimedBy.empty()) {
                            snprintf(label, sizeof(label), "%s  %dx%d  (-> %s)",
                                     (*monitors)[mi].name.c_str(),
                                     (*monitors)[mi].width, (*monitors)[mi].height,
                                     claimedBy.c_str());
                        } else {
                            snprintf(label, sizeof(label), "%s  %dx%d",
                                     (*monitors)[mi].name.c_str(),
                                     (*monitors)[mi].width, (*monitors)[mi].height);
                        }
                        if (!claimedBy.empty())
                            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 0.6f));
                        bool sel = (az.outputDest == OutputDest::Fullscreen && az.outputMonitor == mi);
                        if (ImGui::Selectable(label, sel)) {
                            if (!claimedBy.empty()) {
                                for (int zi = 0; zi < (int)zones->size(); zi++) {
                                    if (zi == ai) continue;
                                    auto& oz = *(*zones)[zi];
                                    if (oz.outputDest == OutputDest::Fullscreen && oz.outputMonitor == mi) {
                                        oz.outputDest = OutputDest::None;
                                        oz.outputMonitor = -1;
                                        break;
                                    }
                                }
                            }
                            az.outputDest = OutputDest::Fullscreen;
                            az.outputMonitor = mi;
                        }
                        if (!claimedBy.empty()) ImGui::PopStyleColor();
                        ImGui::PopID();
                    }
                }
                if (ndiAvailable) {
                    ImGui::Separator();
                    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 0.7f));
                    ImGui::Text("  NDI");
                    ImGui::PopStyleColor();
                    char ndiLabel[256];
                    std::string streamName = az.ndiStreamName.empty() ? az.name : az.ndiStreamName;
                    snprintf(ndiLabel, sizeof(ndiLabel), "Easel - %s", streamName.c_str());
                    bool sel = (az.outputDest == OutputDest::NDI);
                    if (ImGui::Selectable(ndiLabel, sel)) {
                        az.outputDest = OutputDest::NDI;
                        if (az.ndiStreamName.empty()) az.ndiStreamName = az.name;
                    }
                }
                ImGui::EndPopup();
            }
        }

        // Bumped 14 → 24 so the OUTPUT cluster has space before the
        // resolution chip + fullscreen icon.
        ImGui::SameLine(0, 24);

        // Composition chip + Fullscreen
        {
            static const char* presetLabels[] = {
                "1920x1080 (1080p)", "3840x2160 (4K)", "1280x720 (720p)",
                "2560x1440 (1440p)", "8000x2000 (Ultra-wide)", "1024x768", "Custom"
            };
            static const int presetW[] = { 1920, 3840, 1280, 2560, 8000, 1024, 0 };
            static const int presetH[] = { 1080, 2160, 720, 1440, 2000, 768, 0 };
            const int presetCount = 7;

            // Borderless text + chevron — the chip itself doesn't carry a
            // visible background; affordance comes from the small ▾ glyph
            // and a subtle hover tint on text only.
            ImVec2 compChipBR;  // bottom-right of chip — anchors popup
            {
                ImVec2 cur = ImGui::GetCursorScreenPos();
                bool clicked = ImGui::InvisibleButton("##compChip",
                                                      ImVec2(chipW, fsBtnSize));
                bool hov = ImGui::IsItemHovered();
                ImDrawList* dl = navFG;

                ImU32 textCol = hov ? IM_COL32(235, 240, 250, 240)
                                    : IM_COL32(170, 175, 185, 230);
                ImVec2 lblPos(cur.x,
                              cur.y + (fsBtnSize - lblSz.y) * 0.5f);
                dl->AddText(lblPos, textCol, compLabel);

                // Tiny chevron — 6px wide, 3px tall, 1.4px thick.
                float chx = cur.x + lblSz.x + caretGap + 1.0f;
                float chy = cur.y + fsBtnSize * 0.5f + 1.0f;
                float chr = 3.0f;
                dl->AddLine(ImVec2(chx,             chy - 1.5f),
                            ImVec2(chx + chr,       chy + 1.5f),
                            textCol, 1.4f);
                dl->AddLine(ImVec2(chx + chr,       chy + 1.5f),
                            ImVec2(chx + chr * 2.f, chy - 1.5f),
                            textCol, 1.4f);

                compChipBR = ImVec2(cur.x + chipW, cur.y + fsBtnSize);
                if (clicked) {
                    // Chip doubles as "reset view" — see comment in the
                    // legacy nav-row chip above for rationale.
                    resetZoom();
                    ImGui::OpenPopup("##CompPreset");
                }
            }

            // Fullscreen icon — 4 corner brackets, drawn directly so the
            // affordance is iconographic rather than a wide text button.
            // Click toggles editor fullscreen.
            {
                ImGui::SameLine(0, 6);
                const float fsBtnSize = 28.0f;
                ImVec2 cur = ImGui::GetCursorScreenPos();
                bool clicked = ImGui::InvisibleButton("##fs_icon", ImVec2(fsBtnSize, fsBtnSize));
                bool hov = ImGui::IsItemHovered();
                ImDrawList* dlfs = navFG;
                float cx = cur.x + fsBtnSize * 0.5f;
                float cy = cur.y + fsBtnSize * 0.5f;
                if (hov) {
                    dlfs->AddRectFilled(cur,
                                        ImVec2(cur.x + fsBtnSize, cur.y + fsBtnSize),
                                        IM_COL32(255, 255, 255, 24), 6.0f);
                }
                float r  = 7.0f;
                float a  = 4.0f;
                float th = 1.6f;
                ImU32 c  = hov ? IM_COL32(235, 240, 250, 240)
                               : IM_COL32(190, 195, 205, 230);
                if (m_editorFullscreenHint) {
                    // Inward-pointing brackets — "exit fullscreen" glyph.
                    dlfs->AddLine(ImVec2(cx - r,     cy - r + a), ImVec2(cx - r + a, cy - r + a), c, th);
                    dlfs->AddLine(ImVec2(cx - r + a, cy - r + a), ImVec2(cx - r + a, cy - r),     c, th);
                    dlfs->AddLine(ImVec2(cx + r,     cy - r + a), ImVec2(cx + r - a, cy - r + a), c, th);
                    dlfs->AddLine(ImVec2(cx + r - a, cy - r + a), ImVec2(cx + r - a, cy - r),     c, th);
                    dlfs->AddLine(ImVec2(cx - r,     cy + r - a), ImVec2(cx - r + a, cy + r - a), c, th);
                    dlfs->AddLine(ImVec2(cx - r + a, cy + r - a), ImVec2(cx - r + a, cy + r),     c, th);
                    dlfs->AddLine(ImVec2(cx + r,     cy + r - a), ImVec2(cx + r - a, cy + r - a), c, th);
                    dlfs->AddLine(ImVec2(cx + r - a, cy + r - a), ImVec2(cx + r - a, cy + r),     c, th);
                } else {
                    // Outward-pointing brackets — "enter fullscreen" glyph.
                    dlfs->AddLine(ImVec2(cx - r, cy - r), ImVec2(cx - r + a, cy - r),     c, th);
                    dlfs->AddLine(ImVec2(cx - r, cy - r), ImVec2(cx - r,     cy - r + a), c, th);
                    dlfs->AddLine(ImVec2(cx + r, cy - r), ImVec2(cx + r - a, cy - r),     c, th);
                    dlfs->AddLine(ImVec2(cx + r, cy - r), ImVec2(cx + r,     cy - r + a), c, th);
                    dlfs->AddLine(ImVec2(cx - r, cy + r), ImVec2(cx - r + a, cy + r),     c, th);
                    dlfs->AddLine(ImVec2(cx - r, cy + r), ImVec2(cx - r,     cy + r - a), c, th);
                    dlfs->AddLine(ImVec2(cx + r, cy + r), ImVec2(cx + r - a, cy + r),     c, th);
                    dlfs->AddLine(ImVec2(cx + r, cy + r), ImVec2(cx + r,     cy + r - a), c, th);
                }
                if (clicked) m_wantsFullscreenToggle = true;
                if (ImGui::IsItemHovered())
                    ImGui::SetTooltip(m_editorFullscreenHint ? "Exit fullscreen"
                                                             : "Enter fullscreen");
            }

            // Wider, darker, with generous vertical rhythm so the resolution
            // list breathes and the Custom row (W × H inputs + Apply) fits
            // comfortably without truncation. Anchor top-right of popup at
            // chip's bottom-right (pivot 1,0) so it grows leftward —
            // keeps the popup fully on-screen even when the chip sits flush
            // against the viewport's right edge.
            ImGui::SetNextWindowPos(compChipBR, ImGuiCond_Always, ImVec2(1.0f, 0.0f));
            ImGui::SetNextWindowSize(ImVec2(320, 0), ImGuiCond_Always);
            // Pull the popup viewport to the same logical viewport as the
            // chip and force focus so it renders above the floating
            // Properties / Layers control-panel hosts that would otherwise
            // submit later in the frame and z-order over the popup.
            ImGui::SetNextWindowViewport(ImGui::GetMainViewport()->ID);
            ImGui::SetNextWindowFocus();
            ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding,    ImVec2(14, 12));
            ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing,      ImVec2(10, 9));
            ImGui::PushStyleVar(ImGuiStyleVar_FramePadding,     ImVec2(8, 5));
            ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding,   8.0f);
            ImGui::PushStyleColor(ImGuiCol_PopupBg, IM_COL32(8, 9, 12, 255));
            ImGui::PushStyleColor(ImGuiCol_Border,  IM_COL32(255, 255, 255, 22));
            bool compPresetOpen = ImGui::BeginPopup("##CompPreset");
            ImGui::PopStyleColor(2);
            ImGui::PopStyleVar(4);
            if (compPresetOpen) {
                for (int p = 0; p < presetCount - 1; p++) {
                    bool sel = (az.width == presetW[p] && az.height == presetH[p]);
                    if (ImGui::Selectable(presetLabels[p], sel)) {
                        az.resize(presetW[p], presetH[p]);
                        az.compPreset = p;
                        resetZoom();
                    }
                }
                ImGui::Separator();
                static int s_customCompW = 1920, s_customCompH = 1080;
                static int s_customCompZone = -1;
                if (s_customCompZone != ai) {
                    s_customCompW = az.width > 0 ? az.width : 1920;
                    s_customCompH = az.height > 0 ? az.height : 1080;
                    s_customCompZone = ai;
                }
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.55f, 0.60f, 0.68f, 1.0f));
                ImGui::TextUnformatted("Custom");
                ImGui::PopStyleColor();
                ImGui::SetNextItemWidth(80);
                ImGui::InputInt("##compCW", &s_customCompW, 0);
                ImGui::SameLine();
                ImGui::TextDisabled("x");
                ImGui::SameLine();
                ImGui::SetNextItemWidth(80);
                ImGui::InputInt("##compCH", &s_customCompH, 0);
                ImGui::SameLine();
                bool valid = s_customCompW >= 64 && s_customCompH >= 64
                          && s_customCompW <= 16384 && s_customCompH <= 16384;
                if (!valid) ImGui::BeginDisabled();
                if (ImGui::SmallButton("Apply")) {
                    az.resize(s_customCompW, s_customCompH);
                    az.compPreset = presetCount - 1;
                    resetZoom();
                    ImGui::CloseCurrentPopup();
                }
                if (!valid) ImGui::EndDisabled();
                ImGui::EndPopup();
            }
        }

        ImGui::PopStyleVar(2);
        }
        ImGui::End();
        ImGui::PopStyleColor();
        ImGui::PopStyleVar(3);
    }

    ImGui::Unindent(12);
}

// ─────────────────────────────────────────────────────────────────────
// Floating zone + OUTPUT secondary nav, anchored at the canvas
// top-left. Holds the zone tabs (Main / + ) and the OUTPUT destination
// combo so the main nav row can stay focused on workspace switching
// and display setup (composition + fullscreen).
// ─────────────────────────────────────────────────────────────────────
void ViewportPanel::renderZoneOutputDock(
        std::vector<std::unique_ptr<OutputZone>>* zones,
        int* activeZone,
        const std::vector<MonitorInfo>* monitors,
        bool ndiAvailable,
        int editorMonitor) {
    // Floating dock disabled — zones + OUTPUT moved into the top-nav
    // right cluster (next to the aspect-ratio chip and Fullscreen icon).
    // Function body kept for diff history.
    (void)zones; (void)activeZone; (void)monitors; (void)ndiAvailable; (void)editorMonitor;
    return;

#if 0
    ImGuiViewport* vp = ImGui::GetMainViewport();
    // 20px from the GLFW window's left edge — symmetric with the 20px
    // right inset on the top-nav fullscreen cluster. Y offset clears the
    // workspace nav row plus a small breathing margin.
    float dockX = vp->Pos.x + 20.0f;
    float dockY = vp->Pos.y + ImGui::GetFrameHeight() + 24.0f;

    ImGui::SetNextWindowPos(ImVec2(dockX, dockY), ImGuiCond_Always);

    ImGuiWindowFlags fl = ImGuiWindowFlags_NoTitleBar | ImGuiWindowFlags_NoResize
        | ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoCollapse | ImGuiWindowFlags_NoDocking
        | ImGuiWindowFlags_NoSavedSettings | ImGuiWindowFlags_AlwaysAutoResize;

    ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding,    ImVec2(10, 6));
    ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding,   8.0f);
    ImGui::PushStyleVar(ImGuiStyleVar_WindowBorderSize, 1.0f);
    ImGui::PushStyleColor(ImGuiCol_WindowBg, IM_COL32(28, 28, 32, 240));
    ImGui::PushStyleColor(ImGuiCol_Border,   IM_COL32(255, 255, 255, 24));

    if (ImGui::Begin("##ZoneOutputDock", nullptr, fl)) {
        ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing, ImVec2(4, 0));
        ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(14, 6));

        ImDrawList* tabDraw = ImGui::GetWindowDrawList();

        // ── Zone tabs ───────────────────────────────────────────────
        for (int i = 0; i < (int)zones->size(); i++) {
            ImGui::PushID(9000 + i);
            bool isActive = (i == *activeZone);
            auto& z = *(*zones)[i];
            static const float zoneColors[][3] = {
                {0.96f, 0.96f, 0.96f}, {0.86f, 0.86f, 0.86f}, {0.76f, 0.76f, 0.76f}, {0.66f, 0.66f, 0.66f},
                {0.56f, 0.56f, 0.56f}, {0.80f, 0.80f, 0.80f}, {0.70f, 0.70f, 0.70f}, {0.60f, 0.60f, 0.60f},
            };
            const float* zc = zoneColors[i % 8];
            if (isActive) {
                ImGui::PushStyleColor(ImGuiCol_Button,        ImVec4(zc[0], zc[1], zc[2], 0.25f));
                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(zc[0], zc[1], zc[2], 0.35f));
                ImGui::PushStyleColor(ImGuiCol_Text,          ImVec4(zc[0], zc[1], zc[2], 1.0f));
            } else {
                ImGui::PushStyleColor(ImGuiCol_Button,        ImVec4(zc[0]*0.15f, zc[1]*0.15f, zc[2]*0.15f, 0.9f));
                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(zc[0]*0.25f, zc[1]*0.25f, zc[2]*0.25f, 1.0f));
                ImGui::PushStyleColor(ImGuiCol_Text,          ImVec4(zc[0]*0.6f,  zc[1]*0.6f,  zc[2]*0.6f,  1.0f));
            }
            if (ImGui::Button(z.name.c_str())) *activeZone = i;
            ImGui::PopStyleColor(3);
            if (ImGui::IsItemHovered() && ImGui::IsMouseDoubleClicked(0)) {
                m_renaming = true;
                m_renameIndex = i;
                strncpy(m_renameBuf, z.name.c_str(), sizeof(m_renameBuf) - 1);
                m_renameBuf[sizeof(m_renameBuf) - 1] = '\0';
            }
            if (ImGui::BeginPopupContextItem("ZoneTabCtx")) {
                if (ImGui::MenuItem("Rename")) {
                    m_renaming = true;
                    m_renameIndex = i;
                    strncpy(m_renameBuf, z.name.c_str(), sizeof(m_renameBuf) - 1);
                    m_renameBuf[sizeof(m_renameBuf) - 1] = '\0';
                }
                if (ImGui::MenuItem("Duplicate")) *activeZone = -(200 + i);
                if ((int)zones->size() > 1) {
                    if (ImGui::MenuItem("Remove")) *activeZone = -(300 + i);
                }
                ImGui::EndPopup();
            }
            ImVec2 btnMin = ImGui::GetItemRectMin();
            if (z.outputDest == OutputDest::Fullscreen || z.outputDest == OutputDest::NDI) {
                ImU32 dotCol = IM_COL32((int)(zc[0]*255), (int)(zc[1]*255), (int)(zc[2]*255), 255);
                tabDraw->AddCircleFilled(ImVec2(btnMin.x + 5, btnMin.y + 5), 3.0f, dotCol);
            }
            ImGui::SameLine();
            ImGui::PopID();
        }
        if (m_renaming) ImGui::OpenPopup("##RenameZone");
        if (ImGui::BeginPopup("##RenameZone")) {
            ImGui::Text("Rename Zone");
            ImGui::SetNextItemWidth(200);
            bool enter = ImGui::InputText("##RenameInput", m_renameBuf, sizeof(m_renameBuf),
                ImGuiInputTextFlags_EnterReturnsTrue | ImGuiInputTextFlags_AutoSelectAll);
            if (m_renaming) { ImGui::SetKeyboardFocusHere(-1); m_renaming = false; }
            if (enter || ImGui::IsKeyPressed(ImGuiKey_Escape)) {
                if (enter && m_renameIndex >= 0 && m_renameIndex < (int)zones->size() && m_renameBuf[0]) {
                    (*zones)[m_renameIndex]->name = m_renameBuf;
                }
                m_renameIndex = -1;
                ImGui::CloseCurrentPopup();
            }
            ImGui::EndPopup();
        }

        // ── + button (add zone) ─────────────────────────────────────
        ImGui::PushStyleColor(ImGuiCol_Button,        ImVec4(1.0f, 1.0f, 1.0f, 0.08f));
        ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.25f));
        ImGui::PushStyleColor(ImGuiCol_Text,          ImVec4(1.0f, 1.0f, 1.0f, 0.85f));
        if (ImGui::Button("+")) *activeZone = -(100 + (int)zones->size());
        if (ImGui::IsItemHovered()) ParamRow::Tooltip("Add output zone");
        ImGui::PopStyleColor(3);

        // ── OUTPUT label + destination combo ────────────────────────
        int ai = *activeZone;
        if (ai >= 0 && ai < (int)zones->size()) {
            auto& az = *(*zones)[ai];
            ImGui::SameLine(0, 14);

            static char destBuf[128] = {};
            const char* destLabel = "PREVIEW ONLY";
            if (az.outputDest == OutputDest::Fullscreen && monitors) {
                int mi = az.outputMonitor;
                if (mi >= 0 && mi < (int)monitors->size()) {
                    snprintf(destBuf, sizeof(destBuf), "FULLSCREEN: %s",
                             (*monitors)[mi].name.c_str());
                    destLabel = destBuf;
                }
            } else if (az.outputDest == OutputDest::NDI) {
                snprintf(destBuf, sizeof(destBuf), "NDI: \"%s\"",
                         az.ndiStreamName.empty() ? az.name.c_str()
                                                  : az.ndiStreamName.c_str());
                destLabel = destBuf;
            }

            bool live = (az.outputDest != OutputDest::None);
            ImGui::AlignTextToFramePadding();
            if (live) ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.22f, 0.82f, 0.52f, 1.0f));
            else      ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 1.0f));
            ImGui::TextUnformatted("OUTPUT");
            ImGui::PopStyleColor();
            ImGui::SameLine();

            if (live) ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.22f, 0.82f, 0.52f, 1.0f));
            else      ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.70f, 0.73f, 0.78f, 1.0f));
            ImGui::SetNextItemWidth(160.0f);
            if (ImGui::BeginCombo("##ZoneOutputDockCombo", destLabel, ImGuiComboFlags_HeightLarge)) {
                if (ImGui::Selectable("Preview Only", az.outputDest == OutputDest::None)) {
                    az.outputDest = OutputDest::None;
                    az.outputMonitor = -1;
                }
                if (monitors && !monitors->empty()) {
                    ImGui::Separator();
                    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 0.7f));
                    ImGui::Text("  Fullscreen");
                    ImGui::PopStyleColor();
                    for (int mi = 0; mi < (int)monitors->size(); mi++) {
                        ImGui::PushID(mi);
                        if (mi == editorMonitor) { ImGui::PopID(); continue; }
                        std::string claimedBy;
                        for (int zi = 0; zi < (int)zones->size(); zi++) {
                            if (zi == ai) continue;
                            auto& oz = *(*zones)[zi];
                            if (oz.outputDest == OutputDest::Fullscreen && oz.outputMonitor == mi) {
                                claimedBy = oz.name; break;
                            }
                        }
                        char label[256];
                        if (!claimedBy.empty()) {
                            snprintf(label, sizeof(label), "%s  %dx%d  (-> %s)",
                                     (*monitors)[mi].name.c_str(),
                                     (*monitors)[mi].width, (*monitors)[mi].height,
                                     claimedBy.c_str());
                        } else {
                            snprintf(label, sizeof(label), "%s  %dx%d",
                                     (*monitors)[mi].name.c_str(),
                                     (*monitors)[mi].width, (*monitors)[mi].height);
                        }
                        if (!claimedBy.empty())
                            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 0.6f));
                        bool sel = (az.outputDest == OutputDest::Fullscreen && az.outputMonitor == mi);
                        if (ImGui::Selectable(label, sel)) {
                            if (!claimedBy.empty()) {
                                for (int zi = 0; zi < (int)zones->size(); zi++) {
                                    if (zi == ai) continue;
                                    auto& oz = *(*zones)[zi];
                                    if (oz.outputDest == OutputDest::Fullscreen && oz.outputMonitor == mi) {
                                        oz.outputDest = OutputDest::None;
                                        oz.outputMonitor = -1;
                                        break;
                                    }
                                }
                            }
                            az.outputDest = OutputDest::Fullscreen;
                            az.outputMonitor = mi;
                        }
                        if (!claimedBy.empty()) ImGui::PopStyleColor();
                        ImGui::PopID();
                    }
                }
                if (ndiAvailable) {
                    ImGui::Separator();
                    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 0.7f));
                    ImGui::Text("  NDI");
                    ImGui::PopStyleColor();
                    char ndiLabel[256];
                    std::string streamName = az.ndiStreamName.empty() ? az.name : az.ndiStreamName;
                    snprintf(ndiLabel, sizeof(ndiLabel), "Easel - %s", streamName.c_str());
                    bool sel = (az.outputDest == OutputDest::NDI);
                    if (ImGui::Selectable(ndiLabel, sel)) {
                        az.outputDest = OutputDest::NDI;
                        if (az.ndiStreamName.empty()) az.ndiStreamName = az.name;
                    }
                }
                ImGui::EndCombo();
            }
            ImGui::PopStyleColor();
        }

        ImGui::PopStyleVar(2);
    }
    ImGui::End();
    ImGui::PopStyleColor(2);
    ImGui::PopStyleVar(3);
#endif
}
