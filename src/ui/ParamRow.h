#pragma once
#include <imgui.h>
#include <imgui_internal.h>
#include <algorithm>

namespace ParamRow {

// Begin a parameter row: draws the label on the left in `kLabelFrac` of
// the row, primes the next ImGui widget to consume the rest. Use BEFORE
// the widget call (e.g. ImGui::DragFloat("##xyz", ...)). Label width
// auto-grows to fit the actual text so longer labels (TRANSITION,
// RESETFIELD, SPLATTERDENSITY) don't get clipped by the trailing widget.
// kRowLabelColW mirrors PropertyPanel's kLabelColW so every "label : control"
// row across the inspector parks its control at the SAME grid column. The
// control X is set via GetCursorStartPos()-relative SetCursorPosX (reliable
// when scrolled/docked) instead of the window-raw SameLine(offset) form,
// which used to leave RESOLUTION-style labels colliding with their control.
inline constexpr float kRowLabelColW = 96.0f;
inline void Begin(const char* label, float labelFrac = 0.34f) {
    float startX = ImGui::GetCursorStartPos().x;
    float rowW   = ImGui::GetContentRegionAvail().x;
    float textW  = ImGui::CalcTextSize(label).x + 12.0f; // 12px inner pad
    float lblW   = std::max({kRowLabelColW, rowW * labelFrac, textW});
    ImGui::AlignTextToFramePadding();
    ImGui::TextUnformatted(label);
    ImGui::SameLine();
    ImGui::SetCursorPosX(startX + lblW);
    ImGui::SetNextItemWidth(rowW - lblW);
}

// Two equal-width controls on a single row, label on the left. Useful
// for Width/Height, X/Y, Min/Max pairs.
inline void BeginPair(const char* label, float labelFrac = 0.34f,
                      float gap = 6.0f) {
    float rowW = ImGui::GetContentRegionAvail().x;
    float lblW = std::max(70.0f, rowW * labelFrac);
    ImGui::AlignTextToFramePadding();
    ImGui::TextUnformatted(label);
    ImGui::SameLine(lblW);
    float ctrlW = (rowW - lblW - gap) * 0.5f;
    ImGui::SetNextItemWidth(ctrlW);
}
inline float PairSecondWidth(float labelFrac = 0.34f, float gap = 6.0f) {
    float rowW = ImGui::GetContentRegionAvail().x;
    float lblW = std::max(70.0f, rowW * labelFrac);
    return (rowW - lblW - gap) * 0.5f;
}

// Pill-style toggle replacement for ImGui::Checkbox. Returns true if
// clicked. Renders an equal-height capsule that flips state on click.
inline bool TogglePill(const char* label, bool* value) {
    ImVec2 pos = ImGui::GetCursorScreenPos();
    float h    = ImGui::GetFrameHeight();
    float pad  = 14.0f;
    float w    = ImGui::CalcTextSize(label).x + pad * 2.0f;
    bool clicked = ImGui::InvisibleButton(label, ImVec2(w, h));
    bool hov     = ImGui::IsItemHovered();
    ImDrawList* dl = ImGui::GetWindowDrawList();
    bool on = value && *value;
    ImU32 fill = on ? IM_COL32(255,255,255,235)
                    : (hov ? IM_COL32(255,255,255,28) : IM_COL32(255,255,255,14));
    ImU32 tx   = on ? IM_COL32(13,18,26,255)
                    : IM_COL32(220,226,235,230);
    dl->AddRectFilled(pos, ImVec2(pos.x + w, pos.y + h), fill, h * 0.5f);
    ImVec2 ts = ImGui::CalcTextSize(label);
    dl->AddText(ImVec2(pos.x + (w - ts.x) * 0.5f,
                       pos.y + (h - ts.y) * 0.5f), tx, label);
    if (clicked && value) *value = !*value;
    return clicked;
}

// Section header: uppercase + hairline under-rule, monochrome.
// Matches the look of `sectionHeader`/`flatSection` but is callable
// from any panel without duplicating draw-list code.
inline void SectionHeader(const char* name) {
    ImGui::Dummy(ImVec2(0, 14));
    ImVec2 pos = ImGui::GetCursorScreenPos();
    float headlineSize = ImGui::GetFontSize() * 1.4f;
    float w = ImGui::GetContentRegionAvail().x;
    ImDrawList* dl = ImGui::GetWindowDrawList();
    dl->AddText(ImGui::GetFont(), headlineSize, pos,
                IM_COL32(245, 248, 254, 255), name);
    ImGui::Dummy(ImVec2(w, headlineSize + 4.0f));
    float lineY = ImGui::GetCursorScreenPos().y + 2.0f;
    dl->AddLine(ImVec2(pos.x, lineY), ImVec2(pos.x + w, lineY),
                IM_COL32(255, 255, 255, 18), 1.0f);
    ImGui::Dummy(ImVec2(0, 6));
}

// Uniform tooltip — fixed wrap width so every hovered icon / button shows
// a same-shape pill instead of letting the OS auto-size around the text
// length. Call as a drop-in replacement for ImGui::SetTooltip("...").
inline void Tooltip(const char* text, float wrapWidth = 240.0f) {
    if (!text || !*text) return;
    ImGui::SetNextWindowSize(ImVec2(wrapWidth + 24.0f, 0.0f),
                             ImGuiCond_Always);
    ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding,  ImVec2(12, 8));
    ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding, 8.0f);
    if (ImGui::BeginTooltip()) {
        ImGui::PushTextWrapPos(wrapWidth);
        ImGui::TextUnformatted(text);
        ImGui::PopTextWrapPos();
        ImGui::EndTooltip();
    }
    ImGui::PopStyleVar(2);
}

} // namespace ParamRow
