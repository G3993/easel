#include "ui/PropertyPanel.h"
#include "ui/UIManager.h"
#include "ui/ParamRow.h"
#include "ui/LucideIcons.h"
#include "timeline/Timeline.h"
#include "stage/StageView.h"
#include "app/BPMSync.h"
#include "app/SceneManager.h"
#include "app/MIDIManager.h"
#include "compositing/BlendMode.h"
#include "compositing/LayerStack.h"
#include "sources/ShaderSource.h"
#include "sources/VideoSource.h"
#include "sources/ParticleSource.h"
#include "app/DataBus.h"
#include "app/MIDIManager.h"
#ifdef HAS_WHISPER
#include "speech/WhisperSpeech.h"
#endif
#include <imgui.h>
#include <imgui_internal.h>
#include <cstdio>
#include <cmath>
#include <algorithm>
#include <utility>
#include <unordered_set>

// Uppercase a string in place — used for the editorial ALL-CAPS shader
// parameter labels. ImGui IDs use a separate `##paramname` token so the
// uppercased visible label doesn't disturb widget identity.
static std::string upperLabel(const std::string& s) {
    std::string out = s;
    for (auto& ch : out) ch = (char)toupper((unsigned char)ch);
    return out;
}

// --- Theme ---
static const ImVec4 kDimText   = ImVec4(0.45f, 0.50f, 0.58f, 1.0f);
static const ImVec4 kMuted     = ImVec4(0.35f, 0.40f, 0.48f, 1.0f);
static const ImVec4 kRowLabel  = ImVec4(0.59f, 0.62f, 0.68f, 0.90f);
static const ImU32  kSepColor  = IM_COL32(255, 255, 255, 12);

// --- Spacing scale ---------------------------------------------------------
// ONE shared 4px-based rhythm for the whole inspector. Every control row,
// every gap, every label gutter resolves to one of these constants — no
// per-row magic numbers. This is what makes the panel read as a deliberate
// grid instead of "child-developer" nudges. If a value here changes, the
// entire panel re-flows consistently.
//
//   kStepY      base vertical unit (everything is a multiple of this)
//   kRowGapY    breathing room ABOVE each control row (leading Dummy)
//   kRowPadY    breathing room BELOW each control row (trailing Dummy)
//   kLabelGapY  gap between a control's label and its track/widget
//   kSectionGap gap between logical sub-groups inside a section
//   kColGap     horizontal gap between the two columns of a value pair
//   kLabelColW  fixed label-gutter width for "label : control" rows — the
//               control column ALWAYS starts at GetCursorStartPos().x +
//               kLabelColW so RESOLUTION / COLORMODE / blend etc. line up
//   kInnerPad   inset between a label's text and the control that follows
//   kFieldH     nominal control row height (label + gap + track)
static constexpr float kStepY      = 4.0f;
static constexpr float kRowGapY    = kStepY * 2.0f;   // 8  — above a row
static constexpr float kRowPadY    = kStepY * 2.0f;   // 8  — below a row
static constexpr float kLabelGapY  = kStepY * 2.5f;   // 10 — label→track
static constexpr float kSectionGap = kStepY * 4.0f;   // 16 — sub-group gap
static constexpr float kColGap     = kStepY * 3.0f;   // 12 — 2-col gutter
static constexpr float kLabelColW  = 96.0f;           // label gutter width
static constexpr float kInnerPad   = kStepY * 3.0f;   // 12 — text→control
static constexpr float kFieldH     = 22.0f;           // nominal row height

// Dim label + optional inline follow-up. Use `sameLine=false` when the
// follow-up control lives on the next row; default keeps the legacy
// label-then-widget flow.
static void dimLabel(const char* text, const ImVec4& col = kRowLabel, bool sameLine = true) {
    ImGui::PushStyleColor(ImGuiCol_Text, col);
    ImGui::TextUnformatted(text);
    ImGui::PopStyleColor();
    if (sameLine) ImGui::SameLine();
}

// Label-then-control row primer. Draws `text` left-aligned, then parks the
// next ImGui widget at a CONSISTENT control-column X so every "RESOLUTION /
// COLORMODE / Normal-blend / Size" row lines its control up on the same
// grid line. Uses GetCursorStartPos()-relative cursor math (window-relative
// SetCursorPosX) — NOT SameLine(offset), which is window-raw and drifts when
// the panel is scrolled or docked. Call immediately before the widget.
//
// Returns the control-column width so callers can size the widget (or split
// it into a 2-col pair) against the same grid.
static float labelGutter(const char* text,
                         const ImVec4& col = kRowLabel) {
    float startX = ImGui::GetCursorStartPos().x;     // window-relative origin
    float rowW   = ImGui::GetContentRegionAvail().x;
    // Gutter grows to fit a long label so e.g. "AUDIO SRC" never collides
    // with its control, but never shrinks below the shared grid column.
    float textW  = ImGui::CalcTextSize(text).x + kInnerPad;
    float gutter = std::max(kLabelColW, textW);
    ImGui::AlignTextToFramePadding();
    ImGui::PushStyleColor(ImGuiCol_Text, col);
    ImGui::TextUnformatted(text);
    ImGui::PopStyleColor();
    ImGui::SameLine();
    ImGui::SetCursorPosX(startX + gutter);            // fixed control column
    float ctrlW = rowW - gutter;
    ImGui::SetNextItemWidth(ctrlW);
    return ctrlW;
}

static void thinSep() {
    ImGui::Dummy(ImVec2(0, 4));
    ImDrawList* draw = ImGui::GetWindowDrawList();
    ImVec2 p = ImGui::GetCursorScreenPos();
    draw->AddLine(p, ImVec2(p.x + ImGui::GetContentRegionAvail().x, p.y), kSepColor);
    ImGui::Dummy(ImVec2(0, 4));
}

static bool accentBtn(const char* label, float w = 0) {
    ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(1.0f, 1.0f, 1.0f, 0.10f));
    ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.22f));
    ImGui::PushStyleColor(ImGuiCol_ButtonActive, ImVec4(1.0f, 1.0f, 1.0f, 0.40f));
    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(1.0f, 1.0f, 1.0f, 1.0f));
    bool c = ImGui::Button(label, ImVec2(w, 0));
    ImGui::PopStyleColor(4);
    return c;
}

// Section header with chevron; click anywhere in the row to toggle.
// Returns true when the section is OPEN (content should be drawn).
//
// Laws-of-UX notes:
//  - Proximity: tight (2px) top margin + 4px bottom margin so the header
//    visually groups with its content, not the previous section.
//  - Aesthetic-usability: chevron + label stay calm; hover brightens label.
//  - Fitts: full-row hit target (InvisibleButton spans the panel width).
static bool sectionHeader(const char* label, bool* open,
                          bool firstSection = false) {
    // Calm-editor section header. Inspired by the reference's grass.visu
    // panel: bold H2-scale label, NO underline (rhythm comes from
    // generous whitespace, not hairlines), and lots of breathing room
    // above + below so each section reads as its own quiet block.
    //
    // The 32px leading Dummy is the inter-section gap idiom — it separates
    // one section from the previous one. The FIRST section has no previous
    // section, so on it that 32px stacks on top of WindowPadding + the
    // panel's leading Dummy(0,4) and reads as a large dead band above the
    // title (the section reorder made Transform first, exposing this).
    // Skip the leading gap for the first section; the header still keeps
    // its identical headline + 20px bottom reservation, so the rhythm
    // between sections is unchanged.
    (void)open;
    if (!firstSection)
        ImGui::Dummy(ImVec2(0, 32));           // top breathing room (24 → 32)
    ImVec2 rowStart = ImGui::GetCursorScreenPos();
    float rowW  = ImGui::GetContentRegionAvail().x;
    float fontSize = ImGui::GetFontSize();
    float headlineSize = fontSize * 1.85f;     // 1.65 → 1.85: more confident H2 scale

    ImDrawList* dl = ImGui::GetWindowDrawList();
    dl->AddText(ImGui::GetFont(), headlineSize,
                ImVec2(rowStart.x, rowStart.y),
                IM_COL32(247, 249, 254, 255), label);

    // Reserve vertical space + bottom padding (was 14 → 20). The
    // headline + 20px gap below is the breathing rhythm — no divider,
    // no chrome, the air does the hierarchy work.
    ImGui::Dummy(ImVec2(rowW, headlineSize + 20.0f));
    return true;
}

// Horizontal pill-group selector: active pill is filled, others are outlined.
// Returns the newly-selected index, or `current` if nothing changed.
static int pillGroup(const char* id, const char* const* labels, int count, int current) {
    ImGui::PushID(id);
    int result = current;
    ImGui::Dummy(ImVec2(0, 4)); // breathing room above the row
    ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 100.0f);
    ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(12, 5));
    ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing, ImVec2(6, 8));
    float avail = ImGui::GetContentRegionAvail().x;
    float rowX  = 0.0f;
    for (int i = 0; i < count; i++) {
        bool active = (i == current);
        ImVec2 sz = ImGui::CalcTextSize(labels[i]);
        float w = sz.x + 24.0f;
        if (rowX > 0 && rowX + w > avail) {
            ImGui::NewLine();
            rowX = 0;
        } else if (i > 0) {
            ImGui::SameLine();
        }
        if (active) {
            ImGui::PushStyleColor(ImGuiCol_Button,        ImVec4(1.0f, 1.0f, 1.0f, 0.95f));
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 1.0f, 1.0f, 1.00f));
            ImGui::PushStyleColor(ImGuiCol_ButtonActive,  ImVec4(1.0f, 1.0f, 1.0f, 0.85f));
            ImGui::PushStyleColor(ImGuiCol_Text,          ImVec4(0.06f, 0.07f, 0.10f, 1.0f));
        } else {
            ImGui::PushStyleColor(ImGuiCol_Button,        ImVec4(1.0f, 1.0f, 1.0f, 0.06f));
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.14f));
            ImGui::PushStyleColor(ImGuiCol_ButtonActive,  ImVec4(1.0f, 1.0f, 1.0f, 0.22f));
            ImGui::PushStyleColor(ImGuiCol_Text,          ImVec4(0.82f, 0.85f, 0.90f, 1.0f));
        }
        if (ImGui::Button(labels[i])) result = i;
        ImGui::PopStyleColor(4);
        rowX += w + 6.0f;
    }
    ImGui::PopStyleVar(3);
    ImGui::Dummy(ImVec2(0, 6)); // breathing room below the row
    ImGui::PopID();
    return result;
}

// Soft pill slider: label-left, value-right, thin pill track, circular handle.
// Draws on its own row, full width. Shift to snap to 0.05.
static bool pillSlider(const char* label, float* v, float lo, float hi,
                       const char* fmt = "%.2f") {
    ImGui::PushID(label);
    // Shared rhythm: leading gap = kRowGapY (same as every other row helper).
    ImGui::Dummy(ImVec2(0, kRowGapY));
    float w = ImGui::GetContentRegionAvail().x;
    ImVec2 rowStart = ImGui::GetCursorScreenPos();
    float labelH = ImGui::GetFontSize();
    float trackH = 6.0f;
    float handleR = 8.0f;
    float rowH = labelH + kLabelGapY; // label→track gap on the shared scale

    // Label + value row (drawn via drawlist so we control exact positions)
    ImDrawList* dl = ImGui::GetWindowDrawList();
    char valbuf[32]; snprintf(valbuf, sizeof(valbuf), fmt, *v);
    ImVec2 valSize = ImGui::CalcTextSize(valbuf);
    dl->AddText(rowStart, IM_COL32(170, 175, 185, 220), label);
    dl->AddText(ImVec2(rowStart.x + w - valSize.x, rowStart.y),
                IM_COL32(235, 240, 250, 245), valbuf);

    // Track interaction (invisible button under the track region)
    float trackY = rowStart.y + labelH + kLabelGapY;
    ImGui::SetCursorScreenPos(ImVec2(rowStart.x, trackY - 6.0f));
    bool pressed = ImGui::InvisibleButton("##track", ImVec2(w, trackH + 12.0f));
    bool active  = ImGui::IsItemActive();
    bool hovered = ImGui::IsItemHovered();
    bool changed = false;
    if (active || pressed) {
        float mx = ImGui::GetIO().MousePos.x - rowStart.x;
        float t = mx / w; if (t < 0) t = 0; if (t > 1) t = 1;
        float newV = lo + t * (hi - lo);
        if (ImGui::GetIO().KeyShift) newV = std::round(newV / 0.05f) * 0.05f;
        if (newV != *v) { *v = newV; changed = true; }
    }
    float norm = (hi > lo) ? (*v - lo) / (hi - lo) : 0.0f;
    if (norm < 0) norm = 0; if (norm > 1) norm = 1;

    // Track background (dim) + fill (subtle)
    dl->AddRectFilled(ImVec2(rowStart.x, trackY),
                      ImVec2(rowStart.x + w, trackY + trackH),
                      IM_COL32(255, 255, 255, 14), trackH * 0.5f);
    dl->AddRectFilled(ImVec2(rowStart.x, trackY),
                      ImVec2(rowStart.x + w * norm + 0.5f, trackY + trackH),
                      IM_COL32(255, 255, 255, 44), trackH * 0.5f);
    // Handle
    float hx = rowStart.x + w * norm;
    float hy = trackY + trackH * 0.5f;
    ImU32 handleCol = active ? IM_COL32(255, 255, 255, 255)
                    : hovered ? IM_COL32(240, 244, 252, 255)
                              : IM_COL32(220, 225, 235, 255);
    dl->AddCircleFilled(ImVec2(hx, hy), handleR, handleCol);
    dl->AddCircle(ImVec2(hx, hy), handleR, IM_COL32(0, 0, 0, 110), 0, 1.2f);

    // Advance cursor past the row via Dummy (SetCursorScreenPos without a
    // follow-up item trips ImGui's bounds-check at window End). Trailing
    // pad = kRowPadY — identical to every other row helper.
    ImVec2 curScreen = ImGui::GetCursorScreenPos();
    float targetY = rowStart.y + rowH + kRowPadY;
    float advanceY = targetY - curScreen.y;
    if (advanceY > 0.0f) ImGui::Dummy(ImVec2(w, advanceY));
    ImGui::PopID();
    return changed;
}

// Draw a solid filled lightning-bolt glyph inside a square at (cx, cy) with
// size s. Uses a single 7-vertex convex-ish polygon that reads as one clean
// shape rather than two overlapping triangles. AddConvexPolyFilled gives us
// anti-aliased edges so the bolt no longer looks pixelated.
static void drawBolt(ImDrawList* dl, float cx, float cy, float s, ImU32 col) {
    float h = s * 0.5f;
    // 7-vertex bolt outline, traced clockwise starting from the top spike.
    // Coordinates chosen so the silhouette reads as the classic ⚡ glyph
    // with slightly thicker arms than the previous two-triangle version.
    ImVec2 pts[7] = {
        ImVec2(cx + h * 0.10f, cy - h * 1.00f),  // top spike
        ImVec2(cx - h * 0.55f, cy + h * 0.10f),  // upper-left notch (inside)
        ImVec2(cx - h * 0.05f, cy + h * 0.10f),  // mid-left waist
        ImVec2(cx - h * 0.40f, cy + h * 1.00f),  // bottom spike
        ImVec2(cx + h * 0.55f, cy - h * 0.10f),  // mid-right waist (top edge of lower arm)
        ImVec2(cx + h * 0.05f, cy - h * 0.10f),  // upper-right notch
        ImVec2(cx + h * 0.45f, cy - h * 1.00f),  // top-right shoulder back to spike line
    };
    dl->AddConvexPolyFilled(pts, 7, col);
}

// Parameter row styled after the reference UI: muted label (top-left) + right-aligned
// value on the label row, a full-width pill track with circular handle below. A small
// lightning-bolt icon sits at the very left — click it to open the binding menu so the
// parameter can be driven by audio, MIDI, body tracking, etc. When bound the bolt fills
// amber and the slider's fill tints to match, echoing the "interactivity" state.
struct ParamSliderResult {
    bool changed      = false;
    bool openBindMenu = false; // bolt clicked OR row right-clicked
    bool activated    = false; // drag just started — for undo snapshots
};
static ParamSliderResult paramSlider(const char* id, const char* label, float* v,
                                     float lo, float hi, bool bound,
                                     const char* fmt = "%.2f") {
    ParamSliderResult r;
    ImGui::PushID(id);
    // Shared rhythm: leading gap = kRowGapY (identical to pillSlider /
    // color / toggle rows). Every slider sits on the same vertical grid.
    ImGui::Dummy(ImVec2(0, kRowGapY));

    float w = ImGui::GetContentRegionAvail().x;
    ImVec2 rowStart = ImGui::GetCursorScreenPos();
    float labelH  = ImGui::GetFontSize();
    // Thicker pill track + larger circular thumb to match the reference
    // style: track reads as a rounded pill (not a hairline) and the
    // thumb is a confident solid dot the eye latches onto.
    float trackH  = 10.0f;
    float handleR = 11.0f;
    // Row height = label + shared label→track gap + the pill track itself,
    // so the trailing kRowPadY lands the same distance below every slider.
    float rowH    = labelH + kLabelGapY + trackH;
    float boltBox = 18.0f; // hit-target square around the bolt glyph

    ImDrawList* dl = ImGui::GetWindowDrawList();

    // ⚡ Bolt button — leftmost affordance. Clicking opens the bind menu.
    // Wrapped in a subtle dark-rounded square (~18px, 4px radius) so it
    // reads as a tactile button rather than a floating glyph.
    ImGui::SetCursorScreenPos(ImVec2(rowStart.x - 2.0f,
                                     rowStart.y + (labelH - boltBox) * 0.5f));
    bool boltClicked = ImGui::InvisibleButton("##bolt", ImVec2(boltBox, boltBox));
    bool boltHovered = ImGui::IsItemHovered();
    if (boltClicked) r.openBindMenu = true;

    // Rounded background — brightens on hover so the user sees the tap target.
    ImVec2 bMin(rowStart.x - 2.0f, rowStart.y + (labelH - boltBox) * 0.5f);
    ImVec2 bMax(bMin.x + boltBox, bMin.y + boltBox);
    ImU32 bgCol = boltHovered ? IM_COL32(255, 255, 255, 36)
                              : IM_COL32(255, 255, 255, 16);
    dl->AddRectFilled(bMin, bMax, bgCol, 4.0f);

    // Bolt fill: bound → amber, active (white when bound treated as "on") —
    // when off, fill is a muted gray; when on (bound), fill is the bright
    // amber accent. This keeps active/inactive state differentiated.
    ImU32 boltCol = bound       ? IM_COL32(232, 150,  70, 255)
                  : boltHovered ? IM_COL32(215, 225, 240, 230)
                                : IM_COL32(120, 128, 142, 200);
    // Lucide `sparkles` — replaces the lightning bolt. Reads as "magic /
    // reactive": click to bind this parameter to live data (audio, MIDI, voice).
    lucide::sparkles(dl,
                     rowStart.x - 2.0f + boltBox * 0.5f,
                     rowStart.y + (labelH - boltBox) * 0.5f + boltBox * 0.5f,
                     14.0f, boltCol);

    float labelX = rowStart.x + boltBox + 4.0f;

    // Label (muted) + right-aligned value (bright).
    char valbuf[32]; snprintf(valbuf, sizeof(valbuf), fmt, *v);
    ImVec2 valSize = ImGui::CalcTextSize(valbuf);
    dl->AddText(ImVec2(labelX, rowStart.y),
                IM_COL32(150, 158, 172, 230), label);
    dl->AddText(ImVec2(rowStart.x + w - valSize.x, rowStart.y),
                IM_COL32(235, 240, 250, 245), valbuf);

    // Right-click anywhere on the row also opens the bind menu. Label→track
    // gap = kLabelGapY (the shared scale) so every control's label sits the
    // same distance above its track.
    float trackY = rowStart.y + labelH + kLabelGapY;
    ImGui::SetCursorScreenPos(ImVec2(labelX, rowStart.y));
    ImGui::InvisibleButton("##row", ImVec2(w - (labelX - rowStart.x), labelH + 4));
    if (ImGui::IsItemClicked(ImGuiMouseButton_Right)) r.openBindMenu = true;

    // Track drag hit-target — slightly oversized vertically so the user
    // can grab the slider with their thumb-circle, not just the hairline.
    ImGui::SetCursorScreenPos(ImVec2(rowStart.x, trackY - 8.0f));
    ImGui::InvisibleButton("##track", ImVec2(w, trackH + 16.0f));
    bool tActive  = ImGui::IsItemActive();
    bool hovered  = ImGui::IsItemHovered();
    if (ImGui::IsItemActivated()) r.activated = true;
    if (ImGui::IsItemClicked(ImGuiMouseButton_Right)) r.openBindMenu = true;
    if (tActive) {
        float mx = ImGui::GetIO().MousePos.x - rowStart.x;
        float t = mx / w; if (t < 0) t = 0; if (t > 1) t = 1;
        float newV = lo + t * (hi - lo);
        if (ImGui::GetIO().KeyShift) newV = std::round(newV / 0.05f) * 0.05f;
        if (newV != *v) { *v = newV; r.changed = true; }
    }

    float norm = (hi > lo) ? (*v - lo) / (hi - lo) : 0.0f;
    if (norm < 0) norm = 0; if (norm > 1) norm = 1;

    // Track background + fill — pill track. Background opacity bumped
    // (14 → 28) so the rounded pill reads as a clear container rather
    // than a faint hairline. Fill tint shifts to amber when bound.
    ImU32 fillCol = bound ? IM_COL32(232, 150, 70, 200)
                          : IM_COL32(255, 255, 255, 60);
    dl->AddRectFilled(ImVec2(rowStart.x, trackY),
                      ImVec2(rowStart.x + w, trackY + trackH),
                      IM_COL32(255, 255, 255, 28), trackH * 0.5f);
    dl->AddRectFilled(ImVec2(rowStart.x, trackY),
                      ImVec2(rowStart.x + w * norm + 0.5f, trackY + trackH),
                      fillCol, trackH * 0.5f);
    // Solid circular thumb — bigger and brighter than the previous
    // version so the eye latches onto it as the primary affordance.
    // Subtle inner highlight at the top of the dot adds a polished feel.
    float hx = rowStart.x + w * norm;
    float hy = trackY + trackH * 0.5f;
    ImU32 handleCol = tActive  ? IM_COL32(255, 255, 255, 255)
                    : hovered  ? IM_COL32(245, 248, 254, 255)
                               : IM_COL32(232, 236, 244, 255);
    // Soft drop shadow under the thumb for elevation
    dl->AddCircleFilled(ImVec2(hx, hy + 1.5f), handleR + 1.5f,
                        IM_COL32(0, 0, 0, 60), 24);
    dl->AddCircleFilled(ImVec2(hx, hy), handleR, handleCol, 24);
    // Inner dark accent dot for the "solid black thumb" reference look
    // when not interacting (handle reads as a confident filled circle
    // with weight, not a flat disc).
    if (!tActive && !hovered) {
        dl->AddCircleFilled(ImVec2(hx, hy), handleR * 0.55f,
                            IM_COL32(40, 44, 56, 220), 20);
    }

    // Bottom breathing room = kRowPadY — identical to every other row
    // helper so successive controls share one vertical rhythm.
    ImGui::SetCursorScreenPos(ImVec2(rowStart.x, rowStart.y + rowH + kRowPadY));
    ImGui::PopID();
    return r;
}

// Color row — muted label on the left, circular swatch on the right that opens
// the native color picker. One row per color input, matching the reference's
// quiet layout (no cramped dropdown + tiny chip).
static bool paramColorRow(const char* id, const char* label, glm::vec4* c) {
    ImGui::PushID(id);
    ImGui::Dummy(ImVec2(0, kRowGapY));
    float w = ImGui::GetContentRegionAvail().x;
    ImVec2 rowStart = ImGui::GetCursorScreenPos();
    float labelH = ImGui::GetFontSize();
    float swatchR = 10.0f;
    // Single-line control: clamp to the shared field height so it matches
    // toggle rows and reads on the same rhythm.
    float rowH = std::max({labelH, swatchR * 2.0f, kFieldH});

    ImDrawList* dl = ImGui::GetWindowDrawList();
    dl->AddText(ImVec2(rowStart.x, rowStart.y + (rowH - labelH) * 0.5f),
                IM_COL32(150, 158, 172, 230), label);

    // Reserve the row's vertical space WITHOUT swallowing clicks (Dummy
    // doesn't capture input). The previous InvisibleButton here was
    // intercepting clicks meant for the swatch hit-target below, so the
    // swatch never received the click event needed to open the picker.
    ImGui::Dummy(ImVec2(w, rowH));

    // Circular swatch, right-aligned.
    float cx = rowStart.x + w - swatchR - 2.0f;
    float cy = rowStart.y + rowH * 0.5f;
    ImU32 fill = IM_COL32((int)(c->r * 255), (int)(c->g * 255),
                          (int)(c->b * 255), (int)(c->a * 255));
    dl->AddCircleFilled(ImVec2(cx, cy), swatchR, fill);
    dl->AddCircle(ImVec2(cx, cy), swatchR, IM_COL32(255, 255, 255, 60), 0, 1.0f);

    // A small invisible hit-target around the swatch so clicking it opens the picker.
    ImVec2 hitMin(cx - swatchR - 4, cy - swatchR - 4);
    ImGui::SetCursorScreenPos(hitMin);
    ImGui::InvisibleButton("##swatch", ImVec2(swatchR * 2 + 8, swatchR * 2 + 8));
    if (ImGui::IsItemClicked()) ImGui::OpenPopup("##picker");

    bool changed = false;
    if (ImGui::BeginPopup("##picker")) {
        if (ImGui::ColorPicker4("##cp", &(*c)[0],
                                ImGuiColorEditFlags_NoLabel |
                                ImGuiColorEditFlags_AlphaBar)) {
            changed = true;
        }
        ImGui::EndPopup();
    }

    ImGui::SetCursorScreenPos(ImVec2(rowStart.x, rowStart.y + rowH + kRowPadY));
    ImGui::PopID();
    return changed;
}

// Toggle row — label on the left, pill-style switch on the right. One row per bool.
static bool paramToggleRow(const char* id, const char* label, bool* b) {
    ImGui::PushID(id);
    ImGui::Dummy(ImVec2(0, kRowGapY));
    float w = ImGui::GetContentRegionAvail().x;
    ImVec2 rowStart = ImGui::GetCursorScreenPos();
    float labelH = ImGui::GetFontSize();
    float switchW = 30.0f, switchH = 16.0f;
    // Single-line control row clamped to the shared field height so toggle
    // rows, color rows and combo rows all share one row box.
    float rowH = std::max({labelH, switchH, kFieldH});

    ImDrawList* dl = ImGui::GetWindowDrawList();
    dl->AddText(ImVec2(rowStart.x, rowStart.y + (rowH - labelH) * 0.5f),
                IM_COL32(150, 158, 172, 230), label);

    float sx = rowStart.x + w - switchW - 2.0f;
    float sy = rowStart.y + (rowH - switchH) * 0.5f;

    ImGui::SetCursorScreenPos(ImVec2(sx, sy));
    bool clicked = ImGui::InvisibleButton("##sw", ImVec2(switchW, switchH));
    if (clicked) { *b = !*b; }

    // White when ON, faint white track when OFF — matches the rest of the
    // chrome and removes the lone orange accent that was reading as warning.
    ImU32 trackCol = *b ? IM_COL32(232, 238, 250, 220) : IM_COL32(255, 255, 255, 28);
    dl->AddRectFilled(ImVec2(sx, sy), ImVec2(sx + switchW, sy + switchH),
                      trackCol, switchH * 0.5f);
    float knobR = switchH * 0.5f - 2.0f;
    float knobX = *b ? sx + switchW - knobR - 2.0f : sx + knobR + 2.0f;
    float knobY = sy + switchH * 0.5f;
    dl->AddCircleFilled(ImVec2(knobX, knobY), knobR, IM_COL32(240, 244, 250, 255));

    ImGui::SetCursorScreenPos(ImVec2(rowStart.x, rowStart.y + rowH + kRowPadY));
    ImGui::PopID();
    return clicked;
}

// Inter-group gap — use between logical groups of controls. Resolves to
// the shared kSectionGap so every sub-group break in the panel is identical
// (no "tight cluster next to huge empty gap").
static void sectionBreak() {
    ImGui::Dummy(ImVec2(0, kSectionGap));
}

// --- Two-column labeled drag helpers ---
// Draws: [Label: value] using format string as the label inside the widget

// Single drag with embedded label: "X  0.01"
static bool namedDrag(const char* id, const char* prefix, float* v, float speed, float lo, float hi, const char* fmt = "%.2f") {
    char fullFmt[64];
    snprintf(fullFmt, sizeof(fullFmt), "%s  %s", prefix, fmt);
    return ImGui::DragFloat(id, v, speed, lo, hi, fullFmt);
}

// Two drags side by side with embedded labels
static bool dragPair(const char* idA, const char* labelA, float* a,
                     const char* idB, const char* labelB, float* b,
                     float speed, float lo, float hi, const char* fmt = "%.2f") {
    // Clean 2-col grid: equal column widths, fixed kColGap gutter. Using the
    // same constant for the width math AND the SameLine spacing guarantees
    // every paired row (X/Y, Size/Rot, W/H, Top/Btm, Left/Right) aligns to
    // the identical two-column grid regardless of window ItemSpacing.
    float w = (ImGui::GetContentRegionAvail().x - kColGap) * 0.5f;
    bool changed = false;
    ImGui::SetNextItemWidth(w);
    if (namedDrag(idA, labelA, a, speed, lo, hi, fmt)) changed = true;
    ImGui::SameLine(0, kColGap);
    ImGui::SetNextItemWidth(w);
    if (namedDrag(idB, labelB, b, speed, lo, hi, fmt)) changed = true;
    return changed;
}

// Two drags side by side with independent speed/range/format per slot.
// Use when the neighboring values aren't homogeneous (e.g. Size + Rot).
struct DragCfg { float speed, lo, hi; const char* fmt; };
struct DragPairResult { bool changedA, changedB, activated; };
static DragPairResult dragPair2(const char* idA, const char* labelA, float* a, DragCfg ca,
                                const char* idB, const char* labelB, float* b, DragCfg cb) {
    // Same 2-col grid as dragPair — equal widths, fixed kColGap gutter.
    float w = (ImGui::GetContentRegionAvail().x - kColGap) * 0.5f;
    DragPairResult r{false, false, false};
    ImGui::SetNextItemWidth(w);
    r.changedA = namedDrag(idA, labelA, a, ca.speed, ca.lo, ca.hi, ca.fmt);
    if (ImGui::IsItemActivated()) r.activated = true;
    ImGui::SameLine(0, kColGap);
    ImGui::SetNextItemWidth(w);
    r.changedB = namedDrag(idB, labelB, b, cb.speed, cb.lo, cb.hi, cb.fmt);
    if (ImGui::IsItemActivated()) r.activated = true;
    return r;
}

// Dual-handle range slider — picks [*lo, *hi] inside the absolute [absLo,
// absHi] domain. Same visual language as pillSlider/paramSlider (rounded
// pill track, solid circular thumbs) and the same kRowGapY / kLabelGapY /
// kRowPadY vertical rhythm so it drops into the panel without new spacing.
// Returns true while either handle is being dragged.
// `liveVal` (optional, nullptr to disable): a value in [absLo, absHi] that is
// rendered as a thin bright vertical caret + triangle on the SAME track as the
// Min/Max thumbs — used to show an audio binding's current driven value moving
// in real time. Read-only; never mutated.
static bool rangeSlider(const char* id, const char* label,
                        float* lo, float* hi, float absLo, float absHi,
                        const float* liveVal = nullptr) {
    ImGui::PushID(id);
    ImGui::Dummy(ImVec2(0, kRowGapY));
    float w = ImGui::GetContentRegionAvail().x;
    ImVec2 rowStart = ImGui::GetCursorScreenPos();
    float labelH = ImGui::GetFontSize();
    float trackH = 10.0f, handleR = 9.0f;
    float rowH   = labelH + kLabelGapY + trackH;
    float span   = (absHi > absLo) ? (absHi - absLo) : 1.0f;
    ImDrawList* dl = ImGui::GetWindowDrawList();

    // Label (muted) + right-aligned "min – max" readout (bright).
    char val[48];
    snprintf(val, sizeof(val), "%.2f  -  %.2f", *lo, *hi);
    ImVec2 vs = ImGui::CalcTextSize(val);
    dl->AddText(ImVec2(rowStart.x, rowStart.y), IM_COL32(150, 158, 172, 230), label);
    dl->AddText(ImVec2(rowStart.x + w - vs.x, rowStart.y),
                IM_COL32(235, 240, 250, 245), val);

    float trackY = rowStart.y + labelH + kLabelGapY;
    auto toX = [&](float v) { return rowStart.x + (v - absLo) / span * w; };
    auto toV = [&](float x) {
        float t = (x - rowStart.x) / w;
        if (t < 0) t = 0; if (t > 1) t = 1;
        return absLo + t * span;
    };

    ImGui::SetCursorScreenPos(ImVec2(rowStart.x, trackY - 8.0f));
    ImGui::InvisibleButton("##rng", ImVec2(w, trackH + 16.0f));
    bool active = ImGui::IsItemActive();
    bool changed = false;
    if (active) {
        float mx = ImGui::GetIO().MousePos.x;
        // Grab whichever handle the press started nearest to.
        static int grab = -1;
        if (ImGui::IsItemActivated())
            grab = (std::abs(mx - toX(*lo)) <= std::abs(mx - toX(*hi))) ? 0 : 1;
        float nv = toV(mx);
        if (grab == 0) { *lo = std::min(nv, *hi); } else { *hi = std::max(nv, *lo); }
        changed = true;
    }

    // Track, selected span, then the two solid thumbs (pill idiom).
    dl->AddRectFilled(ImVec2(rowStart.x, trackY),
                      ImVec2(rowStart.x + w, trackY + trackH),
                      IM_COL32(255, 255, 255, 28), trackH * 0.5f);
    dl->AddRectFilled(ImVec2(toX(*lo), trackY), ImVec2(toX(*hi), trackY + trackH),
                      IM_COL32(232, 150, 70, 200), trackH * 0.5f);
    for (float hv : { *lo, *hi }) {
        float hx = toX(hv), hy = trackY + trackH * 0.5f;
        dl->AddCircleFilled(ImVec2(hx, hy + 1.5f), handleR + 1.5f, IM_COL32(0, 0, 0, 60), 20);
        dl->AddCircleFilled(ImVec2(hx, hy), handleR, IM_COL32(232, 236, 244, 255), 20);
    }

    // Live driven-value marker — thin bright caret + triangle, on top of the
    // round thumbs, using the same amber accent as the selected-span fill.
    if (liveVal) {
        float lv = *liveVal;
        if (lv < absLo) lv = absLo; else if (lv > absHi) lv = absHi;
        float lx = toX(lv);
        dl->AddLine(ImVec2(lx, trackY - 3.0f), ImVec2(lx, trackY + trackH + 3.0f),
                    IM_COL32(232, 150, 70, 255), 1.75f);
        float ty = trackY - 3.0f;
        dl->AddTriangleFilled(ImVec2(lx - 4.0f, ty - 5.0f), ImVec2(lx + 4.0f, ty - 5.0f),
                              ImVec2(lx, ty), IM_COL32(232, 150, 70, 255));
    }

    ImGui::SetCursorScreenPos(ImVec2(rowStart.x, rowStart.y + rowH + kRowPadY));

    // Tiny live numeric readout under the track (dim label + bright value),
    // same text idiom / spacing as the rest of the panel.
    if (liveVal) {
        char lvbuf[48];
        snprintf(lvbuf, sizeof(lvbuf), "live  %.3f", *liveVal);
        ImVec2 lr = ImGui::GetCursorScreenPos();
        dl->AddText(ImVec2(lr.x, lr.y), IM_COL32(150, 158, 172, 230), lvbuf);
        ImGui::Dummy(ImVec2(0, ImGui::GetFontSize() + kRowPadY));
    }

    ImGui::PopID();
    return changed;
}

void PropertyPanel::render(std::shared_ptr<Layer> layer, bool& maskEditMode,
                           SpeechState* speech, MosaicAudioState* mosaicAudio,
                           float appTime, LayerStack* layerStack,
                           BPMSync* bpmSync, SceneManager* sceneManager,
                           int* audioDeviceIdx, MIDIManager* midi) {
    ImGui::SetNextWindowSizeConstraints(ImVec2(250, 200), ImVec2(FLT_MAX, FLT_MAX));
    // Window name uses the "display###ID" form so the tab shows a minimal
    // "    ###Properties" — a few spaces reserve the tab's visible width
    // without rendering any readable label text. UIManager::drawMyTabIcon
    // then paints the filter icon over the tab rect so the tab reads as
    // an icon. The "###ID" half keeps the internal window name stable
    // for dock/focus lookups.
    //
    // Generous padding + vertical spacing — the parameter panel is meant to
    // read as airy and minimal, more breathing room than the default tight
    // ImGui rhythm. Mirrors the calm-editor reference (grass.visu) where
    // section headers stand alone and rows have ample vertical air between
    // them. Also bump frame padding so individual fields breathe.
    // Spacing rhythm: ItemSpacing.y bumped down 16 → 6 so successive rows
    // sit on a single visual rhythm rather than feeling like every row is
    // its own paragraph. Each row helper (paramSlider / paramToggleRow /
    // pillSlider) already adds its own internal breathing — stacking 16px
    // ItemSpacing on top of those caused the panel to read as half-empty.
    // All derived from the shared spacing scale so the chrome breathes on
    // the same rhythm as the content. ItemSpacing stays tight (kStepY) —
    // each row helper already injects kRowGapY/kRowPadY, so a large
    // ItemSpacing.y here would double the gaps and read as half-empty.
    // Slightly more generous frame padding + consistent rounded fields give
    // the pro-audio/video-inspector "premium" feel without wasting space.
    ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(kStepY * 7.0f, kStepY * 6.0f)); // 28,24
    ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing,   ImVec2(kColGap, kStepY));              // 12,4
    ImGui::PushStyleVar(ImGuiStyleVar_FramePadding,  ImVec2(kInnerPad, kStepY * 2.5f));     // 12,10
    ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 6.0f);
    ImGui::PushStyleVar(ImGuiStyleVar_GrabRounding,  6.0f);
    ImGui::Begin("        ###Properties");
    ImGui::PopStyleVar(5);

#if 0
    // ── SOURCES tab strip ─────────────────────────────────────────────
    // Top of the inspector: 4 big circular icon tabs (VOICE / CAMERA /
    // DATA / CONTENT). Active tab glows red and reveals its config block
    // (transcript area + mic toggle + audio device + decay for VOICE).
    // Modeled on ShaderClaw3's controls panel — the inspector is "what's
    // driving the shader" first, "shader params" second.
    {
        static int s_sourceTab = 0;  // 0=Voice 1=Camera 2=Data 3=Content
        struct Src { const char* label; ImU32 accent; };
        const Src srcs[] = {
            { "VOICE",   IM_COL32(255, 90, 110, 255) },
            { "CAMERA",  IM_COL32(120, 180, 240, 255) },
            { "DATA",    IM_COL32(150, 200, 140, 255) },
            { "CONTENT", IM_COL32(220, 175, 110, 255) },
        };
        const float iconR = 24.0f;          // circle radius
        const float colW  = 78.0f;          // per-column width
        const float ICON_TOP_PAD = 8.0f;
        ImDrawList* dl = ImGui::GetWindowDrawList();
        ImGui::Dummy(ImVec2(0, ICON_TOP_PAD));
        ImVec2 stripStart = ImGui::GetCursorScreenPos();
        float availW = ImGui::GetContentRegionAvail().x;
        float groupW = colW * 4.0f;
        float baseX  = stripStart.x + (availW - groupW) * 0.5f;

        for (int i = 0; i < 4; i++) {
            float cx = baseX + i * colW + colW * 0.5f;
            float cy = stripStart.y + iconR + 2.0f;
            bool active = (s_sourceTab == i);

            // Hit area (square covering circle + label)
            ImGui::SetCursorScreenPos(ImVec2(baseX + i * colW,
                                              stripStart.y));
            ImGui::PushID(i);
            bool clicked = ImGui::InvisibleButton("##src", ImVec2(colW, iconR * 2.0f + 22.0f));
            bool hov     = ImGui::IsItemHovered();
            ImGui::PopID();

            // Active glow halo
            if (active) {
                for (int k = 0; k < 3; k++) {
                    dl->AddCircle(ImVec2(cx, cy), iconR + 2.0f + k * 1.5f,
                                  (srcs[i].accent & 0x00FFFFFF) | (((28 - k * 8) << 24)),
                                  32, 1.6f);
                }
            }
            ImU32 bgFill = active ? IM_COL32(255, 90, 110, 80)
                                  : (hov ? IM_COL32(255, 255, 255, 22)
                                         : IM_COL32(20, 22, 28, 220));
            dl->AddCircleFilled(ImVec2(cx, cy), iconR, bgFill, 32);
            dl->AddCircle(ImVec2(cx, cy), iconR,
                          IM_COL32(255, 255, 255, active ? 60 : 40), 32, 1.0f);

            // Glyph
            ImU32 glyph = active ? IM_COL32(255, 255, 255, 255)
                                 : IM_COL32(180, 188, 200, 230);
            if (i == 0) {
                // Mic
                float bw = 4.5f;
                dl->AddRectFilled(ImVec2(cx - bw, cy - 8.0f),
                                  ImVec2(cx + bw, cy + 1.0f), glyph, bw);
                dl->PathArcTo(ImVec2(cx, cy - 1.0f), 8.0f,
                              0.10f * 3.14159f, 0.90f * 3.14159f, 14);
                dl->PathStroke(glyph, 0, 1.6f);
                dl->AddLine(ImVec2(cx, cy + 7.0f), ImVec2(cx, cy + 11.0f), glyph, 1.6f);
                dl->AddLine(ImVec2(cx - 4.0f, cy + 11.0f),
                            ImVec2(cx + 4.0f, cy + 11.0f), glyph, 1.6f);
            } else if (i == 1) {
                // Camera body + lens
                dl->AddRectFilled(ImVec2(cx - 10.0f, cy - 6.0f),
                                  ImVec2(cx + 10.0f, cy + 6.0f), glyph, 2.0f);
                dl->AddCircleFilled(ImVec2(cx, cy), 3.5f, IM_COL32(0, 0, 0, 200), 16);
                dl->AddCircle(ImVec2(cx, cy), 3.5f, glyph, 16, 1.4f);
                // Top notch
                dl->AddRectFilled(ImVec2(cx - 4.0f, cy - 8.5f),
                                  ImVec2(cx + 4.0f, cy - 6.0f), glyph, 1.0f);
            } else if (i == 2) {
                // Database cylinders (3 stacked ellipses)
                for (int k = 0; k < 3; k++) {
                    float yk = cy - 6.0f + k * 5.0f;
                    dl->AddCircle(ImVec2(cx, yk), 8.0f, glyph, 18, 1.4f);
                }
            } else {
                // Content: play triangle inside a box
                dl->AddRect(ImVec2(cx - 9.0f, cy - 7.0f),
                            ImVec2(cx + 9.0f, cy + 7.0f), glyph, 2.0f, 0, 1.4f);
                dl->AddTriangleFilled(
                    ImVec2(cx - 2.0f, cy - 4.0f),
                    ImVec2(cx - 2.0f, cy + 4.0f),
                    ImVec2(cx + 4.0f, cy), glyph);
            }

            // Label below
            ImVec2 ts = ImGui::CalcTextSize(srcs[i].label);
            dl->AddText(ImVec2(cx - ts.x * 0.5f, cy + iconR + 4.0f),
                        active ? IM_COL32(232, 238, 250, 240)
                               : IM_COL32(150, 160, 175, 220),
                        srcs[i].label);

            if (clicked) s_sourceTab = i;
        }
        ImGui::Dummy(ImVec2(0, iconR * 2.0f + 28.0f));

        // ── Tab content ────────────────────────────────────────────────
        if (s_sourceTab == 0) {
            // VOICE: transcript display + mic toggle + audio device + decay
            const char* transcript = (speech && speech->dataBus) ?
                "" : "";
            // Read latest words from DataBus.
            std::string words;
            if (speech && speech->dataBus) {
                words = speech->dataBus->get("cue.latest");
                if (words.empty()) words = speech->dataBus->get("etherea.latest");
            }
            ImVec2 fp = ImGui::GetCursorScreenPos();
            float fw = ImGui::GetContentRegionAvail().x;
            float fh = 48.0f;
            dl->AddRectFilled(fp, ImVec2(fp.x + fw, fp.y + fh),
                              IM_COL32(20, 22, 28, 220), 8.0f);
            dl->AddRect(fp, ImVec2(fp.x + fw, fp.y + fh),
                        IM_COL32(255, 255, 255, 28), 8.0f, 0, 1.0f);
            const char* placeholder = "START TALKING..";
            const char* shown = words.empty() ? placeholder : words.c_str();
            ImU32 textCol = words.empty() ? IM_COL32(110, 118, 130, 220)
                                          : IM_COL32(232, 238, 250, 240);
            ImVec2 tts = ImGui::CalcTextSize(shown);
            dl->AddText(ImVec2(fp.x + 14.0f, fp.y + (fh - tts.y) * 0.5f),
                        textCol, shown);
            ImGui::Dummy(ImVec2(0, fh + 8.0f));

            // (mic toggle / audio device / decay come from Application —
            //  this tab will gain those rows once we pass the data through.)
        } else {
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.45f, 0.50f, 0.58f, 0.9f));
            ImGui::TextWrapped("(%s controls coming soon)", srcs[s_sourceTab].label);
            ImGui::PopStyleColor();
        }

        // Section divider before layer params
        ImVec2 dp = ImGui::GetCursorScreenPos();
        float dw = ImGui::GetContentRegionAvail().x;
        dl->AddLine(dp, ImVec2(dp.x + dw, dp.y),
                    IM_COL32(255, 255, 255, 30), 1.0f);
        ImGui::Dummy(ImVec2(0, 8));
    }
#endif // SOURCES tab strip — moved to Sources panel per design
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

    // Stage Setup section — only when the workspace is Stage mode and
    // we have a StageView reference. Tool selection (Move/Rotate/Scale)
    // lives on the floating left toolbar; this panel is the displays
    // inspector ONLY so the user has a single source of truth.
    if (m_stageView && UIManager::sMode == UIManager::WorkspaceMode::Stage) {
        ImGui::PushStyleColor(ImGuiCol_Header,        ImVec4(1.0f, 1.0f, 1.0f, 0.08f));
        ImGui::PushStyleColor(ImGuiCol_HeaderHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.15f));
        ImGui::PushStyleColor(ImGuiCol_HeaderActive,  ImVec4(1.0f, 1.0f, 1.0f, 0.22f));
        bool setupOpen = sectionHeader("Setup", nullptr);
        ImGui::PopStyleColor(3);
        if (setupOpen) {
            static const std::vector<unsigned int> kEmpty;
            const std::vector<unsigned int>& zts = m_zoneTexs ? *m_zoneTexs : kEmpty;
            m_stageView->renderSceneInspector(zts);
        }
    }

    // BPM/Audio controls now live in the Audio panel.
    // Scene management now lives in the Scenes panel.
    // Properties is empty unless a layer is selected.
#if 0
    // --- Audio (BPM + bindings, always visible) ---
    if (bpmSync) {
        ImGui::PushStyleColor(ImGuiCol_Header, ImVec4(1.0f, 1.0f, 1.0f, 0.08f));
        ImGui::PushStyleColor(ImGuiCol_HeaderHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.15f));
        ImGui::PushStyleColor(ImGuiCol_HeaderActive, ImVec4(1.0f, 1.0f, 1.0f, 0.22f));
        bool audioSectionOpen = sectionHeader("Audio", nullptr);
        ImGui::PopStyleColor(3);

        if (audioSectionOpen) {
            float currentBPM = bpmSync->bpm();
            float w = ImGui::GetContentRegionAvail().x;

            // Beat indicator dots + BPM text
            {
                ImDrawList* dl = ImGui::GetWindowDrawList();
                ImVec2 p = ImGui::GetCursorScreenPos();
                float dotY = p.y + 8;
                for (int b = 0; b < 4; b++) {
                    float dotCX = p.x + b * 16.0f;
                    int beatInBar = bpmSync->beatCount() % 4;
                    bool isCurrent = (b == beatInBar) && currentBPM > 0;
                    float pulse = isCurrent ? bpmSync->beatPulse() : 0.0f;
                    float r = 4.0f + pulse * 2.0f;
                    dl->AddCircleFilled(ImVec2(dotCX + 6, dotY), r,
                                        isCurrent ? IM_COL32(255, 255, 255, (int)(140 + pulse * 115))
                                                  : IM_COL32(255, 255, 255, 30));
                }
                char bpmBuf[16];
                if (currentBPM > 0) snprintf(bpmBuf, sizeof(bpmBuf), "%.1f BPM", currentBPM);
                else snprintf(bpmBuf, sizeof(bpmBuf), "--- BPM");
                dl->AddText(ImVec2(p.x + 74, p.y + 2),
                            currentBPM > 0 ? IM_COL32(255, 255, 255, 255) : IM_COL32(100, 115, 140, 180),
                            bpmBuf);
                ImGui::Dummy(ImVec2(w, 18));
            }

            // TAP + BPM input + Reset
            {
                float btnW = (w - ImGui::GetStyle().ItemSpacing.x * 2) / 3.0f;
                ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(1.0f, 1.0f, 1.0f, 0.10f));
                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.25f));
                ImGui::PushStyleColor(ImGuiCol_ButtonActive, ImVec4(1.0f, 1.0f, 1.0f, 0.40f));
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(1.0f, 1.0f, 1.0f, 1.0f));
                if (ImGui::Button("TAP", ImVec2(btnW, 0))) bpmSync->tap();
                ImGui::PopStyleColor(4);
                ImGui::SameLine();
                ImGui::SetNextItemWidth(btnW);
                float bpmVal = currentBPM;
                if (ImGui::DragFloat("##BPMVal", &bpmVal, 0.5f, 0.0f, 300.0f, "%.0f BPM"))
                    bpmSync->setBPM(bpmVal);
                ImGui::SameLine();
                ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(0.85f, 0.30f, 0.32f, 0.10f));
                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(0.85f, 0.30f, 0.32f, 0.30f));
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.85f, 0.30f, 0.32f, 0.85f));
                if (ImGui::Button("Reset", ImVec2(btnW, 0))) {
                    bpmSync->setBPM(0);
                    bpmSync->resetPhase();
                }
                ImGui::PopStyleColor(3);
            }

            // Audio reactive bindings (only when a layer is selected)
            if (layer) {
                ImGui::Dummy(ImVec2(0, 4));
                ImGui::PushStyleColor(ImGuiCol_Text, kDimText);
                ImGui::Text("Bindings");
                ImGui::PopStyleColor();

                static const char* targetNames[] = { "None", "Opacity", "Pos X", "Pos Y", "Scale", "Rotation" };
                static const char* signalNames[] = { "Bass", "Mid", "High", "Beat" };

                if (accentBtn("+ Add Binding", -1)) {
                    Layer::AudioBinding ab;
                    ab.target = Layer::AudioTarget::Scale;
                    ab.signal = 0;
                    ab.strength = 0.3f;
                    layer->audioBindings.push_back(ab);
                }

                int removeIdx = -1;
                for (int b = 0; b < (int)layer->audioBindings.size(); b++) {
                    auto& ab = layer->audioBindings[b];
                    ImGui::PushID(40000 + b);
                    float bw = ImGui::GetContentRegionAvail().x;
                    float third = (bw - ImGui::GetStyle().ItemSpacing.x * 2 - 20) / 3.0f;
                    ImGui::SetNextItemWidth(third);
                    int tgt = (int)ab.target;
                    if (ImGui::Combo("##tgt", &tgt, targetNames, (int)Layer::AudioTarget::COUNT))
                        ab.target = (Layer::AudioTarget)tgt;
                    ImGui::SameLine();
                    ImGui::SetNextItemWidth(third);
                    ImGui::Combo("##sig", &ab.signal, signalNames, 4);
                    ImGui::SameLine();
                    ImGui::SetNextItemWidth(third);
                    ImGui::SliderFloat("##str", &ab.strength, 0.0f, 2.0f, "%.2f");
                    ImGui::SameLine();
                    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.6f, 0.3f, 0.3f, 0.7f));
                    if (ImGui::SmallButton("x")) removeIdx = b;
                    ImGui::PopStyleColor();
                    ImGui::PopID();
                }
                if (removeIdx >= 0) layer->audioBindings.erase(layer->audioBindings.begin() + removeIdx);
            }
        }

        thinSep();
    }

    // --- Scenes (always visible) ---
    if (sceneManager && layerStack) {
        ImGui::PushStyleColor(ImGuiCol_Header, ImVec4(1.0f, 1.0f, 1.0f, 0.08f));
        ImGui::PushStyleColor(ImGuiCol_HeaderHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.15f));
        ImGui::PushStyleColor(ImGuiCol_HeaderActive, ImVec4(1.0f, 1.0f, 1.0f, 0.22f));
        bool scenesOpen = sectionHeader("Scenes", nullptr);
        ImGui::PopStyleColor(3);

        if (scenesOpen) {
            // Save current state as scene
            if (accentBtn("Save Scene", -1)) {
                char name[32];
                snprintf(name, sizeof(name), "Scene %d", sceneManager->count() + 1);
                sceneManager->saveScene(name, *layerStack);
            }

            // Scene list
            int removeIdx = -1;
            for (int s = 0; s < sceneManager->count(); s++) {
                ImGui::PushID(30000 + s);
                auto& scene = (*sceneManager)[s];

                // Recall button (full width minus delete button)
                float w = ImGui::GetContentRegionAvail().x - 24;
                ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(0.06f, 0.07f, 0.10f, 0.9f));
                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.20f));
                ImGui::PushStyleColor(ImGuiCol_ButtonActive, ImVec4(1.0f, 1.0f, 1.0f, 0.35f));
                if (ImGui::Button(scene.name.c_str(), ImVec2(w, 0))) {
                    sceneManager->recallScene(s, *layerStack);
                }
                ImGui::PopStyleColor(3);

                ImGui::SameLine();
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.6f, 0.3f, 0.3f, 0.7f));
                if (ImGui::SmallButton("x")) removeIdx = s;
                ImGui::PopStyleColor();

                ImGui::PopID();
            }
            if (removeIdx >= 0) sceneManager->removeScene(removeIdx);
        }

        thinSep();
    }
#endif
    // Silence unused-parameter warnings for sections now routed elsewhere.
    (void)bpmSync; (void)sceneManager;

    if (!layer) {
        // Empty Properties — content appears only when a layer/shader/etc. is selected.
        ImGui::End();
        return;
    }

    undoNeeded = false;

    // (Layer-name rename field removed — the Layer panel already shows and
    //  edits the name; duplicating it here just added scroll distance before
    //  the user could reach the shader parameters.)
    ImGui::Dummy(ImVec2(0, 4));

    // Section order (top→bottom): Transform → Blend+Opacity → shader
    // Parameters → … → Tiling (last child). The Blend+Opacity row is a
    // lambda so it can be emitted right after Transform without moving
    // its internals; Tiling is likewise emitted as the panel's last child.
    auto emitBlendOpacityRow = [&]() {
    // --- Blend + Opacity --- previously crammed onto one 50/50 line with a
    // tight 10px gutter (combo had no label, opacity label+track+value were
    // all squeezed into one half). Now two clean rows, each aligned to the
    // SAME shared control column as RESOLUTION / COLORMODE so the inspector
    // reads as one consistent grid with real breathing room.
    {
        // Row 1 — BLEND combo at the shared control column.
        ImGui::Dummy(ImVec2(0, kRowGapY));
        float blendCtrlW = labelGutter("BLEND", kDimText);
        const char* currentBlend = blendModeName(layer->blendMode);
        ImGui::SetNextItemWidth(blendCtrlW);
        if (ImGui::BeginCombo("##Blend", currentBlend)) {
            for (int i = 0; i < (int)BlendMode::COUNT; i++) {
                BlendMode mode = (BlendMode)i;
                bool selected = (layer->blendMode == mode);
                if (ImGui::Selectable(blendModeName(mode), selected)) {
                    undoNeeded = true;
                    layer->blendMode = mode;
                }
                if (selected) ImGui::SetItemDefaultFocus();
            }
            ImGui::EndCombo();
        }

        // Row 2 — OPACITY: inline track spanning the full control column,
        // value right-aligned. Same gutter as BLEND so the two rows stack
        // into a tidy two-row block instead of a cramped single line.
        {
            ImGui::Dummy(ImVec2(0, kRowGapY));
            ImGui::PushID("##OpacityInline");
            float frameH = ImGui::GetFrameHeight();
            float opCtrlW = labelGutter("OPACITY", kDimText);
            // labelGutter() leaves the cursor at the control column; capture
            // that as the row origin for the custom-drawn slider.
            ImVec2 rowStart = ImGui::GetCursorScreenPos();
            ImDrawList* dl = ImGui::GetWindowDrawList();

            char valbuf[16]; snprintf(valbuf, sizeof(valbuf), "%.2f", layer->opacity);
            ImVec2 valSize = ImGui::CalcTextSize(valbuf);

            const float valPad = kInnerPad;
            float trackX0 = rowStart.x;
            float trackX1 = rowStart.x + opCtrlW - valSize.x - valPad;
            float trackW = trackX1 - trackX0;
            if (trackW < 24.0f) trackW = 24.0f;
            float rowMidY = rowStart.y + frameH * 0.5f;

            // Hit zone covers ONLY the track strip, not the label/value text.
            ImGui::SetCursorScreenPos(ImVec2(trackX0, rowStart.y));
            bool pressed = ImGui::InvisibleButton("##opacity_track",
                                                  ImVec2(trackW, frameH));
            bool active  = ImGui::IsItemActive();
            bool hovered = ImGui::IsItemHovered();
            if (active || pressed) {
                float mx = ImGui::GetIO().MousePos.x - trackX0;
                float t = mx / trackW; if (t < 0) t = 0; if (t > 1) t = 1;
                float newV = t;
                if (ImGui::GetIO().KeyShift) newV = std::round(newV / 0.05f) * 0.05f;
                if (newV != layer->opacity) {
                    layer->opacity = newV;
                    undoNeeded = true;
                }
            }
            float norm = layer->opacity;
            if (norm < 0) norm = 0; if (norm > 1) norm = 1;

            // (Label is drawn by labelGutter("OPACITY") above — no inline
            // duplicate here.)
            // Track + fill.
            const float trackH = 6.0f;
            ImVec2 trackA(trackX0, rowMidY - trackH * 0.5f);
            ImVec2 trackB(trackX1, rowMidY + trackH * 0.5f);
            dl->AddRectFilled(trackA, trackB,
                              IM_COL32(255, 255, 255, 14), trackH * 0.5f);
            dl->AddRectFilled(trackA,
                              ImVec2(trackX0 + trackW * norm + 0.5f, trackB.y),
                              IM_COL32(255, 255, 255, 44), trackH * 0.5f);
            // Value (right), vertically centered.
            dl->AddText(ImVec2(trackX1 + valPad, rowMidY - valSize.y * 0.5f),
                        IM_COL32(235, 240, 250, 245), valbuf);
            // Handle on the track
            float hx = trackX0 + trackW * norm;
            ImU32 handleCol = active ? IM_COL32(255, 255, 255, 255)
                            : hovered ? IM_COL32(240, 244, 252, 255)
                                      : IM_COL32(220, 225, 235, 255);
            dl->AddCircleFilled(ImVec2(hx, rowMidY), 7.0f, handleCol);
            dl->AddCircle      (ImVec2(hx, rowMidY), 7.0f,
                                IM_COL32(0, 0, 0, 110), 0, 1.2f);
            // Advance the cursor past the row (the hit-zone InvisibleButton
            // left it at the track origin). Land kRowPadY below the row on
            // the shared rhythm so the next section spaces consistently.
            ImGui::SetCursorScreenPos(ImVec2(rowStart.x, rowStart.y));
            ImGui::Dummy(ImVec2(opCtrlW, frameH + kRowPadY));
            ImGui::PopID();
        }
    }
    }; // end emitBlendOpacityRow

    // (Transition section moved to the BOTTOM of the Parameters panel —
    // see the matching block below the shader-inputs loop.)

    // --- Transform (collapsible, default closed) — secondary controls go
    // under a header so the main event (shader parameters / video) isn't
    // buried under a wall of position/scale/rotation.
    static bool transformOpen = false;
    if (sectionHeader("Transform", &transformOpen, /*firstSection=*/true)) {
        // Phase 6 — circular xy-pad. Drag the dot to translate the layer
        // along X/Y in NDC (-1..1). Doubles as the visual anchor that
        // reference A leads with at the top of its Transform section.
        // Sits left of the X/Y drag fields so the numeric and graphical
        // inputs read as one combined transform widget.
        // padR 36 → 46: pad height (92) now comfortably exceeds the
        // ~84px stack of three drag rows (X/Y + Size/Rot + W/H), so the
        // bottom W/H row no longer collides with the pad's lower edge.
        const float padR = 46.0f;          // outer radius
        const float padPad = 8.0f;
        ImVec2 padTopLeft = ImGui::GetCursorScreenPos();
        float pillsX = padTopLeft.x + padR * 2.0f + padPad * 3.0f;
        // Convert screen-x to window-relative cursor X (the value SetCursorPosX expects).
        float windowX = ImGui::GetWindowPos().x;
        float scrollX = ImGui::GetScrollX();
        float pillsCursorX = pillsX - windowX + scrollX;
        float padBottomY  = padTopLeft.y + padR * 2.0f;
        {
            ImVec2 cur = padTopLeft;
            ImVec2 center(cur.x + padR + padPad, cur.y + padR);
            ImDrawList* dl = ImGui::GetWindowDrawList();
            // Outer ring (track) and inner softer ring (mid-detent at 0,0).
            dl->AddCircleFilled(center, padR, IM_COL32(255, 255, 255, 12), 36);
            dl->AddCircle(center, padR,        IM_COL32(255, 255, 255, 60), 36, 1.2f);
            dl->AddCircle(center, padR * 0.5f, IM_COL32(255, 255, 255, 22), 36, 1.0f);
            // Crosshair through center.
            dl->AddLine(ImVec2(center.x - padR, center.y),
                        ImVec2(center.x + padR, center.y),
                        IM_COL32(255, 255, 255, 18), 1.0f);
            dl->AddLine(ImVec2(center.x, center.y - padR),
                        ImVec2(center.x, center.y + padR),
                        IM_COL32(255, 255, 255, 18), 1.0f);
            // Position dot — clamped position.x/y from [-1..1] map to ring.
            float px = std::max(-1.0f, std::min(1.0f, layer->position.x));
            float py = std::max(-1.0f, std::min(1.0f, layer->position.y));
            ImVec2 dot(center.x + px * padR,
                       center.y - py * padR);  // y inverted (up = +y in NDC)
            dl->AddCircleFilled(dot, 5.0f, IM_COL32(247, 248, 248, 240), 16);
            dl->AddCircle      (dot, 5.0f, IM_COL32(0,   0,   0, 90 ), 16, 1.0f);
            // Hit area
            ImGui::SetCursorScreenPos(ImVec2(center.x - padR, center.y - padR));
            ImGui::InvisibleButton("##XYPad", ImVec2(padR * 2.0f, padR * 2.0f));
            if (ImGui::IsItemActivated()) undoNeeded = true;
            if (ImGui::IsItemActive()) {
                ImVec2 mp = ImGui::GetIO().MousePos;
                float nx = (mp.x - center.x) / padR;
                float ny = -(mp.y - center.y) / padR;
                // Soft clamp to unit disk.
                float r = sqrtf(nx * nx + ny * ny);
                if (r > 1.0f) { nx /= r; ny /= r; }
                layer->position.x = nx;
                layer->position.y = ny;
            }
            if (ImGui::IsItemHovered()) {
                ImGui::SetTooltip("Drag to translate (X %.2f, Y %.2f)\nDouble-click → 0,0",
                                  layer->position.x, layer->position.y);
            }
            if (ImGui::IsMouseDoubleClicked(0) && ImGui::IsItemHovered()) {
                undoNeeded = true;
                layer->position = {0.0f, 0.0f};
            }
            // Layout: numeric fields ride right of the pad on the same row.
            ImGui::SetCursorScreenPos(ImVec2(cur.x + padR * 2.0f + padPad * 3.0f,
                                             cur.y));
        }
        if (dragPair("##PosX", "X", &layer->position.x, "##PosY", "Y", &layer->position.y,
                     0.01f, -2.0f, 2.0f))
        {}
        if (ImGui::IsItemActivated()) undoNeeded = true;

        // Pin Size/Rot and W/H to the same pillsCursorX so they ride right
        // of the xy-pad instead of falling back under it.
        ImGui::SetCursorPosX(pillsCursorX);
        {
            float uniformScale = (layer->scale.x + layer->scale.y) * 0.5f;
            auto sr = dragPair2(
                "##Size", "Size", &uniformScale, {0.01f, 0.01f, 10.0f, "%.2f"},
                "##Rot",  "Rot",  &layer->rotation, {1.0f, -360.0f, 360.0f, "%.1f"});
            if (sr.changedA) {
                float ratio = (layer->scale.x > 0.001f) ? layer->scale.y / layer->scale.x : 1.0f;
                layer->scale.x = uniformScale;
                layer->scale.y = uniformScale * ratio;
            }
            if (sr.activated) undoNeeded = true;
        }

        ImGui::SetCursorPosX(pillsCursorX);
        if (dragPair("##ScaleX", "W", &layer->scale.x, "##ScaleY", "H", &layer->scale.y,
                     0.01f, 0.01f, 10.0f))
        {}
        if (ImGui::IsItemActivated()) undoNeeded = true;

        // After the three pill rows, ensure the cursor is below the pad's
        // bottom edge before Flip H / Flip V / Reset row.
        {
            ImVec2 nowPos = ImGui::GetCursorScreenPos();
            if (nowPos.y < padBottomY + 6.0f) {
                ImGui::SetCursorScreenPos(ImVec2(padTopLeft.x, padBottomY + 6.0f));
            } else {
                ImGui::SetCursorPosX(ImGui::GetCursorPosX() -
                    (pillsCursorX - (padTopLeft.x - ImGui::GetWindowPos().x + ImGui::GetScrollX())));
                ImGui::SetCursorScreenPos(ImVec2(padTopLeft.x, nowPos.y));
            }
        }

        if (ImGui::Checkbox("Flip H", &layer->flipH)) undoNeeded = true;
        ImGui::SameLine();
        if (ImGui::Checkbox("Flip V", &layer->flipV)) undoNeeded = true;
        ImGui::SameLine();
        if (accentBtn("Reset")) {
            undoNeeded = true;
            layer->position = {0.0f, 0.0f};
            layer->scale = {1.0f, 1.0f};
            layer->rotation = 0.0f;
            layer->flipH = false;
            layer->flipV = false;
            layer->mosaicModeFrom = layer->mosaicMode;
            layer->mosaicTransitionStart = appTime;
            layer->mosaicMode = MosaicMode::Mirror;
            layer->tileX = layer->tileY = 1.0f;
            layer->mosaicDensity = 4.0f;
            layer->mosaicSpin = 0.0f;
            layer->audioReactive = false;
            layer->audioStrength = 0.15f;
            layer->cropTop = layer->cropBottom = layer->cropLeft = layer->cropRight = 0.0f;
        }

        // Crop sits inside Transform — cropping is a spatial adjustment,
        // so it belongs with position/scale/rotation rather than as its
        // own top-level section.
        ImGui::Dummy(ImVec2(0, 8));
        dimLabel("Crop", kRowLabel, false);
        if (ImGui::Checkbox("Auto-trim black borders", &layer->autoCrop)) {
            if (layer->autoCrop) {
                layer->autoCropDone = false;
            } else {
                layer->cropTop = layer->cropBottom = layer->cropLeft = layer->cropRight = 0.0f;
            }
            undoNeeded = true;
        }
        if (dragPair("##CropT", "Top", &layer->cropTop, "##CropB", "Btm", &layer->cropBottom,
                     0.005f, 0.0f, 0.49f, "%.3f"))
            undoNeeded = true;
        if (dragPair("##CropL", "Left", &layer->cropLeft, "##CropR", "Right", &layer->cropRight,
                     0.005f, 0.0f, 0.49f, "%.3f"))
            undoNeeded = true;
    }

    // Blend Mode + Opacity — emitted directly under Transform so the user
    // sees transform + blend + the shader effect params without scrolling.
    emitBlendOpacityRow();

    // --- Effects ---
    // When no effects exist yet, show a single inline row: "Effects" label on
    // the left + "+ Add Effect" button on the right. Skips the wasteful
    // collapsible header + full-width button of the empty state.
    // Once effects exist, falls back to the normal collapsible section so the
    // per-effect rows can stack below.
    static bool effectsOpen = false;
    auto openAddEffectPopup = [&]() {
        if (ImGui::BeginPopup("##AddEffect")) {
            for (int t = 0; t < (int)EffectType::COUNT; t++) {
                if (ImGui::MenuItem(effectTypeName((EffectType)t))) {
                    LayerEffect fx;
                    fx.type = (EffectType)t;
                    layer->effects.push_back(fx);
                    undoNeeded = true;
                    effectsOpen = true; // auto-expand once something's been added
                }
            }
            ImGui::EndPopup();
        }
    };
    if (layer->effects.empty()) {
        ImGui::Dummy(ImVec2(0, 4));
        ImGui::AlignTextToFramePadding();
        ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.93f, 0.95f, 0.98f, 1.0f));
        ImGui::Text("Effects");
        ImGui::PopStyleColor();
        ImGui::SameLine();
        const char* btn = "+ Add Effect";
        float btnW = ImGui::CalcTextSize(btn).x
                   + ImGui::GetStyle().FramePadding.x * 2.0f + 12.0f;
        float rightX = ImGui::GetWindowContentRegionMax().x - btnW;
        if (rightX > ImGui::GetCursorPosX()) ImGui::SetCursorPosX(rightX);
        if (accentBtn(btn)) ImGui::OpenPopup("##AddEffect");
        openAddEffectPopup();
        ImGui::Dummy(ImVec2(0, 2));
    } else
    if (sectionHeader("Effects", &effectsOpen)) {
    {

    // --- Layer Effects Chain ---
    {
        // Add effect button (full-width once the section holds effect rows)
        if (accentBtn("+ Add Effect", -1)) {
            ImGui::OpenPopup("##AddEffect");
        }
        openAddEffectPopup();

        // Render each effect
        int removeIdx = -1;
        for (int e = 0; e < (int)layer->effects.size(); e++) {
            auto& fx = layer->effects[e];
            ImGui::PushID(20000 + e);

            // Effect header row: checkbox + name + remove
            ImGui::Checkbox("##en", &fx.enabled);
            ImGui::SameLine();
            ImGui::PushStyleColor(ImGuiCol_Text, fx.enabled ? ImVec4(0.85f, 0.90f, 0.95f, 1.0f) : ImVec4(0.45f, 0.50f, 0.58f, 1.0f));
            ImGui::Text("%s", effectTypeName(fx.type));
            ImGui::PopStyleColor();
            ImGui::SameLine();
            {
                float rightX = ImGui::GetWindowContentRegionMax().x - 20.0f;
                if (ImGui::GetCursorPosX() < rightX) ImGui::SetCursorPosX(rightX);
            }
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.6f, 0.3f, 0.3f, 0.7f));
            if (ImGui::SmallButton("x")) removeIdx = e;
            ImGui::PopStyleColor();

            if (fx.enabled) {
                float w = ImGui::GetContentRegionAvail().x;
                switch (fx.type) {
                case EffectType::Blur:
                    ParamRow::Begin("BLUR");
                    ImGui::SliderFloat("##blur", &fx.blurRadius, 0.0f, 20.0f, "%.1f");
                    break;
                case EffectType::ColorAdjust: {
                    float half = (w - ImGui::GetStyle().ItemSpacing.x) * 0.5f;
                    ImGui::SetNextItemWidth(half);
                    ImGui::SliderFloat("##brt", &fx.brightness, -1.0f, 1.0f, "Brt %.2f");
                    ImGui::SameLine();
                    ImGui::SetNextItemWidth(half);
                    ImGui::SliderFloat("##ctr", &fx.contrast, -1.0f, 1.0f, "Ctr %.2f");
                    ImGui::SetNextItemWidth(half);
                    ImGui::SliderFloat("##sat", &fx.saturation, -1.0f, 1.0f, "Sat %.2f");
                    ImGui::SameLine();
                    ImGui::SetNextItemWidth(half);
                    ImGui::SliderFloat("##hue", &fx.hueShift, 0.0f, 360.0f, "Hue %.0f");
                    break;
                }
                case EffectType::Invert:
                    // No params
                    break;
                case EffectType::Pixelate:
                    ParamRow::Begin("SIZE");
                    ImGui::SliderFloat("##pix", &fx.pixelSize, 1.0f, 64.0f, "%.0f");
                    break;
                case EffectType::Feedback: {
                    float half = (w - ImGui::GetStyle().ItemSpacing.x) * 0.5f;
                    ImGui::SetNextItemWidth(half);
                    ImGui::SliderFloat("##fbmix", &fx.feedbackMix, 0.0f, 0.99f, "Mix %.2f");
                    ImGui::SameLine();
                    ImGui::SetNextItemWidth(half);
                    ImGui::SliderFloat("##fbzm", &fx.feedbackZoom, 0.95f, 1.1f, "Zoom %.3f");
                    break;
                }
                case EffectType::Glow: {
                    ParamRow::Begin("THRESHOLD");
                    ImGui::SliderFloat("##glowT", &fx.glowThreshold, 0.0f, 1.0f, "%.2f");
                    float half = (w - ImGui::GetStyle().ItemSpacing.x) * 0.5f;
                    ImGui::SetNextItemWidth(half);
                    ImGui::SliderFloat("##glowR", &fx.glowRadius, 1.0f, 40.0f, "Rad %.1f");
                    ImGui::SameLine();
                    ImGui::SetNextItemWidth(half);
                    ImGui::SliderFloat("##glowI", &fx.glowIntensity, 0.0f, 3.0f, "Int %.2f");
                    break;
                }
                default: break;
                }
            }

            ImGui::PopID();
        }

        if (removeIdx >= 0) {
            layer->effects.erase(layer->effects.begin() + removeIdx);
            undoNeeded = true;
        }

        if (!layer->effects.empty()) {
            ImGui::Dummy(ImVec2(0, 2));
        }
    }

    } // end Effects section (layer effects chain only)
    } // end sectionHeader("Effects")

    // --- Mosaic + Feather (collapsible, default closed) ---
    // Emitted as the panel's LAST child (see call below, just before
    // ImGui::End()) so the bulky Tiling section never pushes Transform /
    // Blend / shader Parameters below the fold.
    static bool tilingOpen = false;
    auto emitTilingSection = [&]() {
    if (sectionHeader("Tiling", &tilingOpen)) {
    // --- Mosaic mode ---
    {
        float halfW = (ImGui::GetContentRegionAvail().x - ImGui::GetStyle().ItemSpacing.x) * 0.5f;

        // Mode dropdown
        ImGui::SetNextItemWidth(halfW);
        const char* currentMode = mosaicModeName(layer->mosaicMode);
        if (ImGui::BeginCombo("##MosaicMode", currentMode)) {
            for (int i = 0; i < (int)MosaicMode::COUNT; i++) {
                MosaicMode mode = (MosaicMode)i;
                bool selected = (layer->mosaicMode == mode);
                if (ImGui::Selectable(mosaicModeName(mode), selected)) {
                    undoNeeded = true;
                    layer->mosaicModeFrom = layer->mosaicMode;
                    layer->mosaicTransitionStart = appTime;
                    layer->mosaicMode = mode;
                }
                if (selected) ImGui::SetItemDefaultFocus();
            }
            ImGui::EndCombo();
        }

        // Audio toggle
        ImGui::SameLine();
        if (layer->audioReactive) {
            ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(1.0f, 1.0f, 1.0f, 0.25f));
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.40f));
            ImGui::PushStyleColor(ImGuiCol_ButtonActive, ImVec4(1.0f, 1.0f, 1.0f, 0.55f));
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(1.0f, 1.0f, 1.0f, 1.0f));
            if (ImGui::Button("~ Audio", ImVec2(halfW, 0))) {
                layer->audioReactive = false;
                undoNeeded = true;
            }
            ImGui::PopStyleColor(4);
        } else {
            if (ImGui::Button("~ Audio", ImVec2(halfW, 0))) {
                layer->audioReactive = true;
                undoNeeded = true;
            }
        }

        // Audio strength slider + source selector (only when active)
        if (layer->audioReactive) {
            ImGui::SetNextItemWidth(-1);
            if (namedDrag("##AudioStr", "Strength", &layer->audioStrength, 0.005f, 0.0f, 1.0f)) {}
            if (ImGui::IsItemActivated()) undoNeeded = true;

            // Mini spectrum bars (bass=red, lowMid=orange, highMid=green, treble=cyan)
            if (mosaicAudio) {
                float barH = 24.0f;
                float avail = ImGui::GetContentRegionAvail().x;
                ImVec2 origin = ImGui::GetCursorScreenPos();
                ImDrawList* draw = ImGui::GetWindowDrawList();

                struct BandInfo { float level; ImU32 color; };
                BandInfo bands[4] = {
                    { mosaicAudio->bass,    IM_COL32(220, 50, 50, 200) },
                    { mosaicAudio->lowMid,  IM_COL32(230, 150, 30, 200) },
                    { mosaicAudio->highMid, IM_COL32(50, 200, 80, 200) },
                    { mosaicAudio->treble,  IM_COL32(30, 200, 220, 200) },
                };

                float bandW = avail / 4.0f;
                for (int b = 0; b < 4; b++) {
                    float h = bands[b].level * barH;
                    ImVec2 bMin(origin.x + b * bandW + 1, origin.y + barH - h);
                    ImVec2 bMax(origin.x + (b + 1) * bandW - 1, origin.y + barH);
                    draw->AddRectFilled(bMin, bMax, bands[b].color, 2.0f);
                }

                // Beat flash overlay
                if (mosaicAudio->beatDecay > 0.05f) {
                    ImU32 flashCol = IM_COL32(255, 255, 255, (int)(mosaicAudio->beatDecay * 60));
                    draw->AddRectFilled(origin, ImVec2(origin.x + avail, origin.y + barH), flashCol, 2.0f);
                }

                ImGui::Dummy(ImVec2(avail, barH + 2));
            }

            // Audio source dropdown
            if (mosaicAudio && mosaicAudio->selectedDevice) {
                const char* srcLabel = "System Audio";
                int sel = *mosaicAudio->selectedDevice;
                if (sel >= 0 && sel < (int)mosaicAudio->devices.size()) {
                    srcLabel = mosaicAudio->devices[sel].name.c_str();
                }
                ParamRow::Begin("AUDIO SRC");
                if (ImGui::BeginCombo("##AudioSrc", srcLabel)) {
                    if (ImGui::Selectable("System Audio", sel == -1)) {
                        *mosaicAudio->selectedDevice = -1;
                    }
                    for (int i = 0; i < (int)mosaicAudio->devices.size(); i++) {
                        auto& d = mosaicAudio->devices[i];
                        char label[256];
                        snprintf(label, sizeof(label), "%s%s", d.name.c_str(),
                                 d.isMic ? "  (mic)" : "");
                        if (ImGui::Selectable(label, sel == i)) {
                            *mosaicAudio->selectedDevice = i;
                        }
                    }
                    ImGui::EndCombo();
                }
            }
        }

        // Mode-specific controls
        switch (layer->mosaicMode) {
            case MosaicMode::Mirror: {
                // 8x8 clickable tile grid
                const int maxTile = 8;
                float avail = ImGui::GetContentRegionAvail().x;
                float cellSize = avail / (float)maxTile;
                if (cellSize > 24.0f) cellSize = 24.0f;
                float gridW = cellSize * maxTile;
                float gridH = cellSize * maxTile;
                int itx = (int)(layer->tileX + 0.5f);
                int ity = (int)(layer->tileY + 0.5f);

                ImGui::PushStyleColor(ImGuiCol_Text, kMuted);
                ImGui::Text("Tile  %dx%d", itx, ity);
                ImGui::PopStyleColor();

                ImVec2 origin = ImGui::GetCursorScreenPos();
                float indent = (avail - gridW) * 0.5f;
                if (indent > 0) origin.x += indent;

                ImDrawList* draw = ImGui::GetWindowDrawList();
                ImGui::SetCursorScreenPos(origin);
                ImGui::InvisibleButton("##TileGrid", ImVec2(gridW, gridH));

                for (int gy = 0; gy < maxTile; gy++) {
                    for (int gx = 0; gx < maxTile; gx++) {
                        ImVec2 cMin(origin.x + gx * cellSize, origin.y + gy * cellSize);
                        ImVec2 cMax(cMin.x + cellSize - 1.0f, cMin.y + cellSize - 1.0f);
                        bool active = (gx < itx && gy < ity);
                        bool hovered = false;
                        ImVec2 mouse = ImGui::GetIO().MousePos;
                        if (mouse.x >= cMin.x && mouse.x < cMax.x &&
                            mouse.y >= cMin.y && mouse.y < cMax.y) {
                            hovered = true;
                        }
                        if (active) {
                            draw->AddRectFilled(cMin, cMax, IM_COL32(255, 255, 255, 90), 2.0f);
                            draw->AddRect(cMin, cMax, IM_COL32(255, 255, 255, 60), 2.0f);
                        } else if (hovered) {
                            draw->AddRectFilled(cMin, cMax, IM_COL32(255, 255, 255, 35), 2.0f);
                            draw->AddRect(cMin, cMax, IM_COL32(255, 255, 255, 15), 2.0f);
                        } else {
                            draw->AddRectFilled(cMin, cMax, IM_COL32(255, 255, 255, 6), 2.0f);
                            draw->AddRect(cMin, cMax, IM_COL32(255, 255, 255, 10), 2.0f);
                        }
                    }
                }

                if (ImGui::IsItemActive() && ImGui::IsMouseDown(0)) {
                    ImVec2 mouse = ImGui::GetIO().MousePos;
                    int clickX = (int)((mouse.x - origin.x) / cellSize) + 1;
                    int clickY = (int)((mouse.y - origin.y) / cellSize) + 1;
                    if (clickX >= 1 && clickX <= maxTile && clickY >= 1 && clickY <= maxTile) {
                        if (clickX != itx || clickY != ity) {
                            layer->tileX = (float)clickX;
                            layer->tileY = (float)clickY;
                            undoNeeded = true;
                        }
                    }
                }
                break;
            }
            case MosaicMode::Hex: {
                float w = (ImGui::GetContentRegionAvail().x - ImGui::GetStyle().ItemSpacing.x) * 0.5f;
                ImGui::SetNextItemWidth(w);
                if (namedDrag("##Density", "Cells", &layer->mosaicDensity, 0.1f, 1.0f, 20.0f, "%.1f")) {}
                if (ImGui::IsItemActivated()) undoNeeded = true;
                ImGui::SameLine();
                ImGui::SetNextItemWidth(w);
                if (namedDrag("##Spin", "Spin", &layer->mosaicSpin, 1.0f, -360.0f, 360.0f, "%.1f")) {}
                if (ImGui::IsItemActivated()) undoNeeded = true;
                break;
            }
            default: break;
        }
    }

    // --- Feather (inside Tiling collapsible) ---
    pillSlider("Feather", &layer->feather, 0.0f, 0.5f, "%.3f");
    } // end sectionHeader("Tiling")
    }; // end emitTilingSection

    // --- Drop Shadow ---
    // Inline header row: label + Enable checkbox on the same line. If the
    // checkbox is off, there's nothing to tweak so we skip the controls
    // entirely — saving a dropdown click for the common "just want it on"
    // case. When on, controls appear directly below.
    {
        ImGui::Dummy(ImVec2(0, 4));
        ImGui::AlignTextToFramePadding();
        ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.93f, 0.95f, 0.98f, 1.0f));
        ImGui::Text("Drop Shadow");
        ImGui::PopStyleColor();

        // Right-anchored cluster: "Enable" label + checkbox (label on the
        // LEFT of the square). Use the "##" hidden-label trick so ImGui draws
        // just the square, then place our own Text before it.
        const float squareW = ImGui::GetFrameHeight();
        const float gap     = ImGui::GetStyle().ItemInnerSpacing.x;
        const float textW   = ImGui::CalcTextSize("Enable").x;
        const float clusterW = textW + gap + squareW + 4.0f;
        ImGui::SameLine();
        float rightX = ImGui::GetWindowContentRegionMax().x - clusterW;
        if (rightX > ImGui::GetCursorPosX()) ImGui::SetCursorPosX(rightX);

        ImGui::AlignTextToFramePadding();
        ImGui::TextUnformatted("Enable");
        ImGui::SameLine(0, gap);
        if (ImGui::Checkbox("##dshadow", &layer->dropShadowEnabled)) undoNeeded = true;
    }
    if (layer->dropShadowEnabled) {
        {
            if (dragPair("##DsOx", "X", &layer->dropShadowOffsetX,
                         "##DsOy", "Y", &layer->dropShadowOffsetY,
                         0.002f, -1.0f, 1.0f, "%.3f"))
                undoNeeded = true;
            ImGui::SetNextItemWidth(-1);
            if (namedDrag("##DsBlur", "Blur", &layer->dropShadowBlur, 0.25f, 0.0f, 80.0f, "%.1f"))
                undoNeeded = true;
            ImGui::SetNextItemWidth(-1);
            if (namedDrag("##DsOpac", "Opacity", &layer->dropShadowOpacity, 0.01f, 0.0f, 1.0f, "%.2f"))
                undoNeeded = true;
            ImGui::SetNextItemWidth(-1);
            if (namedDrag("##DsSpread", "Spread", &layer->dropShadowSpread, 0.02f, 0.5f, 8.0f, "%.2f"))
                undoNeeded = true;
            // Color picker
            float col[3] = { layer->dropShadowColorR, layer->dropShadowColorG, layer->dropShadowColorB };
            if (ImGui::ColorEdit3("Color##dsCol", col, ImGuiColorEditFlags_NoInputs)) {
                layer->dropShadowColorR = col[0];
                layer->dropShadowColorG = col[1];
                layer->dropShadowColorB = col[2];
                undoNeeded = true;
            }
        }
    }

    // --- Video controls ---
    if (layer->source && layer->source->isVideo()) {
        if (layer->source->isPlaying()) {
            if (accentBtn("Pause", -1)) layer->source->pause();
        } else {
            if (accentBtn("Play", -1)) layer->source->play();
        }
        ImGui::SetNextItemWidth(-1);
        float t = (float)layer->source->currentTime();
        float dur = (float)layer->source->duration();
        if (ImGui::SliderFloat("##Time", &t, 0.0f, dur, "%.1fs")) {
            layer->source->seek(t);
        }
        auto* vidSrc = static_cast<VideoSource*>(layer->source.get());
        if (vidSrc->hasAudio()) {
            float vol = vidSrc->volume();
            bool muted = (vol == 0.0f);

            // Mute toggle button
            ImGui::PushStyleColor(ImGuiCol_Button, muted ? ImVec4(0.85f, 0.30f, 0.32f, 0.25f) : ImVec4(1.0f, 1.0f, 1.0f, 0.10f));
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, muted ? ImVec4(0.85f, 0.30f, 0.32f, 0.45f) : ImVec4(1.0f, 1.0f, 1.0f, 0.22f));
            ImGui::PushStyleColor(ImGuiCol_Text, muted ? ImVec4(0.85f, 0.30f, 0.32f, 1.0f) : ImVec4(1.0f, 1.0f, 1.0f, 1.0f));
            if (ImGui::Button(muted ? "Unmute" : "Mute", ImVec2(54, 0))) {
                static float s_preMuteVol = 1.0f;
                if (muted) {
                    vidSrc->setVolume(s_preMuteVol > 0.01f ? s_preMuteVol : 1.0f);
                } else {
                    s_preMuteVol = vol;
                    vidSrc->setVolume(0.0f);
                }
                vol = vidSrc->volume();
            }
            ImGui::PopStyleColor(3);

            ImGui::SameLine();
            ImGui::SetNextItemWidth(-1);
            if (ImGui::SliderFloat("##Volume", &vol, 0.0f, 1.0f, "Vol %.0f%%")) {
                vidSrc->setVolume(vol);
            }
        }
    }

    // --- Particle System controls ----------------------------------
    // Niagara-style emitter inspector: spawn config + module stack. Each
    // module has its own header so users can collapse/reorder them.
    if (layer->source && layer->source->typeName() == "Particles") {
        auto* psrc = static_cast<ParticleSource*>(layer->source.get());
        auto& em = psrc->emitter();

        sectionBreak();
        static bool emitterOpen = true;
        if (sectionHeader("Emitter", &emitterOpen)) {
            // All-vertical layout — no inline SameLine with absolute column
            // positions. Each control gets its own row so ImGui never has to
            // compute a negative cursor offset on narrow panels.
            ImGui::Dummy(ImVec2(0, 4));

            ParamRow::Begin("SPAWN SHAPE");
            if (ImGui::BeginCombo("##PSShape", particleSpawnShapeName(em.spawnShape))) {
                for (int i = 0; i < (int)ParticleSpawnShape::COUNT; i++) {
                    bool sel = ((int)em.spawnShape == i);
                    if (ImGui::Selectable(particleSpawnShapeName((ParticleSpawnShape)i), sel))
                        em.spawnShape = (ParticleSpawnShape)i;
                    if (sel) ImGui::SetItemDefaultFocus();
                }
                ImGui::EndCombo();
            }

            pillSlider("Spawn Rate", &em.spawnRate, 0.0f, 2000.0f, "%.0f/s");
            {
                float mp = (float)em.maxParticles;
                if (pillSlider("Max Particles", &mp, 100.0f, 20000.0f, "%.0f"))
                    em.maxParticles = (int)mp;
            }
            pillSlider("Lifetime Min", &em.lifetimeMin, 0.1f, 10.0f, "%.2fs");
            pillSlider("Lifetime Max", &em.lifetimeMax, 0.1f, 10.0f, "%.2fs");
            pillSlider("Initial Size", &em.initialSize, 0.001f, 0.5f, "%.3f");
            pillSlider("Size Jitter",  &em.sizeJitter,  0.0f, 1.0f, "%.2f");
            pillSlider("Velocity Jitter", &em.velocityJitter, 0.0f, 3.0f, "%.2f");

            if (ImGui::Checkbox("Additive Blend", &em.additive)) {}

            ParamRow::Begin("RENDER MODE");
            const char* modes[] = {"Soft Sprite", "Textured", "Ring"};
            int rm = em.renderMode; if (rm < 0 || rm > 2) rm = 0;
            if (ImGui::BeginCombo("##PSRender", modes[rm])) {
                for (int i = 0; i < 3; i++) {
                    bool sel = (rm == i);
                    if (ImGui::Selectable(modes[i], sel)) em.renderMode = i;
                    if (sel) ImGui::SetItemDefaultFocus();
                }
                ImGui::EndCombo();
            }

            ImGui::TextDisabled("%d live particles", psrc->liveParticleCount());
        }

        sectionBreak();
        static bool modulesOpen = true;
        if (sectionHeader("Modules", &modulesOpen)) {
            // + Add Module dropdown
            if (accentBtn("+ Add Module", -1)) {
                ImGui::OpenPopup("##AddParticleModule");
            }
            if (ImGui::BeginPopup("##AddParticleModule")) {
                for (int t = 0; t < (int)ParticleModuleType::COUNT; t++) {
                    if (ImGui::MenuItem(particleModuleTypeName((ParticleModuleType)t))) {
                        psrc->addModule((ParticleModuleType)t);
                    }
                }
                ImGui::EndPopup();
            }

            int toRemove = -1;
            int toMoveIdx = -1, toMoveDir = 0;
            auto& mods = em.modules;
            for (int i = 0; i < (int)mods.size(); i++) {
                auto& mod = mods[i];
                ImGui::PushID(50000 + i);
                ImGui::Dummy(ImVec2(0, 4));

                // Row 1: [X] Module Name — no right-aligned cluster (that
                // pattern kept tripping ImGui's cursor-bounds assertion in
                // narrow panels). Action buttons get their own row below.
                if (ImGui::Checkbox("##en", &mod.enabled)) {}
                ImGui::SameLine();
                ImGui::PushStyleColor(ImGuiCol_Text,
                    mod.enabled ? ImVec4(0.93f, 0.95f, 0.98f, 1.0f)
                                : ImVec4(0.55f, 0.58f, 0.64f, 1.0f));
                ImGui::Text("%s", particleModuleTypeName(mod.type));
                ImGui::PopStyleColor();

                // Row 2: small action buttons — safe SameLine chain.
                if (ImGui::SmallButton("up")) { toMoveIdx = i; toMoveDir = -1; }
                ImGui::SameLine();
                if (ImGui::SmallButton("dn")) { toMoveIdx = i; toMoveDir = +1; }
                ImGui::SameLine();
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.85f, 0.30f, 0.32f, 1.0f));
                if (ImGui::SmallButton("remove")) toRemove = i;
                ImGui::PopStyleColor();

                // Per-module parameters
                if (mod.enabled) {
                    ImGui::Indent(8.0f);
                    switch (mod.type) {
                        case ParticleModuleType::InitialVelocity:
                            pillSlider("Vel X", &mod.vec3A.x, -5.0f, 5.0f, "%.2f");
                            pillSlider("Vel Y", &mod.vec3A.y, -5.0f, 5.0f, "%.2f");
                            pillSlider("Vel Z", &mod.vec3A.z, -5.0f, 5.0f, "%.2f");
                            pillSlider("Randomness", &mod.randomness, 0.0f, 1.0f, "%.2f");
                            break;
                        case ParticleModuleType::Gravity:
                            pillSlider("G X", &mod.vec3A.x, -5.0f, 5.0f, "%.2f");
                            pillSlider("G Y", &mod.vec3A.y, -5.0f, 5.0f, "%.2f");
                            pillSlider("G Z", &mod.vec3A.z, -5.0f, 5.0f, "%.2f");
                            break;
                        case ParticleModuleType::Drag:
                            pillSlider("Drag", &mod.floatA, 0.0f, 5.0f, "%.2f");
                            break;
                        case ParticleModuleType::Orbital:
                            pillSlider("Axis X", &mod.vec3A.x, -1.0f, 1.0f, "%.2f");
                            pillSlider("Axis Y", &mod.vec3A.y, -1.0f, 1.0f, "%.2f");
                            pillSlider("Axis Z", &mod.vec3A.z, -1.0f, 1.0f, "%.2f");
                            pillSlider("Speed",  &mod.floatA, -8.0f, 8.0f, "%.2f");
                            break;
                        case ParticleModuleType::Turbulence:
                            pillSlider("Strength",  &mod.floatA, 0.0f, 4.0f, "%.2f");
                            pillSlider("Frequency", &mod.floatB, 0.1f, 8.0f, "%.2f");
                            break;
                        case ParticleModuleType::SizeOverLife:
                            pillSlider("Size × (start)", &mod.floatA, 0.0f, 5.0f, "%.2f");
                            pillSlider("Size × (end)",   &mod.floatB, 0.0f, 5.0f, "%.2f");
                            break;
                        case ParticleModuleType::ColorOverLife:
                            if (ImGui::ColorEdit4("Start##cA", &mod.colorA.r,
                                                  ImGuiColorEditFlags_NoInputs)) {}
                            if (ImGui::ColorEdit4("End##cB",   &mod.colorB.r,
                                                  ImGuiColorEditFlags_NoInputs)) {}
                            break;
                        case ParticleModuleType::RotationOverLife:
                            pillSlider("Start (rad)", &mod.floatA, -6.28f, 6.28f, "%.2f");
                            pillSlider("End (rad)",   &mod.floatB, -6.28f, 6.28f, "%.2f");
                            break;
                        case ParticleModuleType::TextureSampleColor:
                            ImGui::TextDisabled("Color is sampled from the bound\n"
                                                "image/video layer at spawn.");
                            break;
                        default: break;
                    }
                    ImGui::Unindent(8.0f);
                }
                ImGui::PopID();
            }
            if (toMoveIdx >= 0) psrc->moveModule(toMoveIdx, toMoveDir);
            if (toRemove >= 0)  psrc->removeModule(toRemove);
        }
    }

    // --- Shader (ISF) controls ---
    if (layer->source && layer->source->isShader()) {
        auto* shaderSrc = static_cast<ShaderSource*>(layer->source.get());
        auto& inputs = shaderSrc->inputs();

        // Shader resolution override
        {
            sectionBreak();

            struct ResPreset { const char* label; int w; int h; };
            ResPreset presets[] = {
                {"Canvas", 0, 0},
                {"720p",  1280, 720},
                {"1080p", 1920, 1080},
                {"1440p", 2560, 1440},
                {"4K",    3840, 2160},
            };
            // Per-layer "custom mode" flag — once the user picks Custom...,
            // stay in custom mode even if the width/height happen to coincide
            // with a preset. Keyed by layer id so selecting a different layer
            // starts fresh. Without this the combo bounces straight back to
            // "1080p" (etc.) and the Size input row never appears.
            static std::unordered_set<uint32_t> s_customMode;
            bool customMode = s_customMode.count(layer->id) > 0;
            bool matchesPreset = false;
            const char* currentLabel = "Custom";
            for (auto& p : presets) {
                if (layer->shaderWidth == p.w && layer->shaderHeight == p.h) {
                    currentLabel = p.label;
                    matchesPreset = true;
                    break;
                }
            }
            if (customMode) matchesPreset = false;
            // "Custom" label shows the current dimensions inline so users
            // can see the exact value at a glance without opening the combo.
            static char customLabel[64];
            if (!matchesPreset) {
                snprintf(customLabel, sizeof(customLabel),
                         "Custom  (%d x %d)",
                         layer->shaderWidth, layer->shaderHeight);
                currentLabel = customLabel;
            }

            // RESOLUTION used to butt straight against the Canvas dropdown
            // (ParamRow::Begin's window-raw SameLine left ~0px gutter).
            // labelGutter() parks the combo at the shared control column
            // (GetCursorStartPos()+kLabelColW) with clear space before it.
            labelGutter("RESOLUTION", kDimText);
            if (ImGui::BeginCombo("##ShaderRes", currentLabel)) {
                for (auto& p : presets) {
                    bool sel = !customMode
                             && layer->shaderWidth == p.w
                             && layer->shaderHeight == p.h;
                    if (ImGui::Selectable(p.label, sel)) {
                        layer->shaderWidth = p.w;
                        layer->shaderHeight = p.h;
                        s_customMode.erase(layer->id);
                    }
                }
                // Dedicated "Custom..." entry — flips the layer into custom
                // mode so the Size input row appears. If the layer was on a
                // preset, seed the custom fields with that preset's size
                // (or 1920x1080 for Canvas) so editing starts from a sane
                // default instead of 0x0.
                ImGui::Separator();
                if (ImGui::Selectable("Custom...", customMode)) {
                    if (layer->shaderWidth == 0 && layer->shaderHeight == 0) {
                        layer->shaderWidth  = 1920;
                        layer->shaderHeight = 1080;
                    }
                    s_customMode.insert(layer->id);
                }
                ImGui::EndCombo();
            }

            // Inline custom W x H inputs — shown beneath the dropdown when
            // the layer is in custom mode. Full-width, labeled, with Apply
            // so the value isn't committed on every keystroke.
            if (!matchesPreset) {
                static int   customW = 1920, customH = 1080;
                static Layer* lastLayer = nullptr;
                if (lastLayer != layer.get()) {
                    customW = layer->shaderWidth  > 0 ? layer->shaderWidth  : 1920;
                    customH = layer->shaderHeight > 0 ? layer->shaderHeight : 1080;
                    lastLayer = layer.get();
                }
                ImGui::Dummy(ImVec2(0, kRowGapY));
                // "Size" label parks its W×H pair at the shared control
                // column (same grid as RESOLUTION above). The two int
                // fields form a clean 2-col grid with the kColGap gutter
                // and an "x" glyph centred between them.
                float sizeCtrlW = labelGutter("Size", kDimText);
                float xGlyphW = ImGui::CalcTextSize("x").x;
                float inputW  = (sizeCtrlW - kColGap * 2.0f - xGlyphW) * 0.5f;
                if (inputW < 48.0f) inputW = 48.0f;
                ImGui::SetNextItemWidth(inputW);
                ImGui::InputInt("##cw", &customW, 0);
                ImGui::SameLine(0, kColGap);
                ImGui::AlignTextToFramePadding();
                ImGui::TextDisabled("x");
                ImGui::SameLine(0, kColGap);
                ImGui::SetNextItemWidth(inputW);
                ImGui::InputInt("##ch", &customH, 0);
                // Apply moves to its own row at the control column — it was
                // crammed flush against the H field with zero breathing room.
                bool changed = (customW != layer->shaderWidth || customH != layer->shaderHeight);
                ImGui::Dummy(ImVec2(0, kRowPadY));
                ImGui::SetCursorPosX(ImGui::GetCursorStartPos().x + kLabelColW);
                if (!changed) ImGui::BeginDisabled();
                if (ImGui::SmallButton("Apply")) {
                    if (customW >= 64 && customH >= 64
                        && customW <= 7680 && customH <= 4320) {
                        layer->shaderWidth  = customW;
                        layer->shaderHeight = customH;
                        undoNeeded = true;
                    }
                }
                if (!changed) ImGui::EndDisabled();
            }
        }

        // Universal Typography section. Detect text shaders by the
        // presence of a `msg` text input — every text shader in the
        // catalog (la_bloom, chat, stream, james, matrix, ascii, etc.)
        // exposes one. Promoted controls (Font, Size, Kerning) render
        // here in a Figma-style block; the generic input loop below
        // skips any input whose name is in `consumedInputs` so the
        // same control isn't drawn twice.
        std::unordered_set<std::string> consumedInputs;
        {
            int  msgIdx        = -1;
            int  fontIdx       = -1;
            int  sizeIdx       = -1;
            int  kerningIdx    = -1;
            for (int i = 0; i < (int)inputs.size(); i++) {
                const auto& in = inputs[i];
                if (in.name == "msg" && in.type == "text") msgIdx = i;
                else if (in.name == "fontFamily")          fontIdx = i;
                else if (in.name == "textScale")           sizeIdx = i;
                else if (in.name == "kerning")             kerningIdx = i;
            }
            bool isTextShader = (msgIdx >= 0);
            if (isTextShader) {
                sectionBreak();
                // Bolder section header — full-width dim band + bright
                // label so the typography group reads at a glance from
                // the top of the panel.
                {
                    ImVec2 cursor = ImGui::GetCursorScreenPos();
                    float  bandH  = 22.0f;
                    float  bandW  = ImGui::GetContentRegionAvail().x;
                    ImGui::GetWindowDrawList()->AddRectFilled(
                        cursor,
                        ImVec2(cursor.x + bandW, cursor.y + bandH),
                        IM_COL32(255, 255, 255, 12), 4.0f);
                    ImGui::SetCursorScreenPos(ImVec2(cursor.x + 8.0f,
                                                     cursor.y + 4.0f));
                    ImGui::PushStyleColor(ImGuiCol_Text,
                        ImVec4(0.95f, 0.96f, 1.0f, 1.0f));
                    ImGui::Text("TYPOGRAPHY");
                    ImGui::PopStyleColor();
                    ImGui::Dummy(ImVec2(0, 4));
                }
                ImGui::Dummy(ImVec2(0, 4));

                // 1) Font family — full-width dropdown.
                if (fontIdx >= 0) {
                    auto& in = inputs[fontIdx];
                    int  cur = (int)std::get<float>(in.value);
                    int  selectedIdx = 0;
                    for (int v = 0; v < (int)in.longValues.size(); v++) {
                        if (in.longValues[v] == cur) { selectedIdx = v; break; }
                    }
                    const char* preview = (!in.longLabels.empty()
                        && selectedIdx < (int)in.longLabels.size())
                        ? in.longLabels[selectedIdx].c_str()
                        : "Font";
                    ImGui::SetNextItemWidth(-1);
                    if (ImGui::BeginCombo("##typoFont", preview)) {
                        for (int v = 0; v < (int)in.longValues.size(); v++) {
                            const char* lbl = (v < (int)in.longLabels.size())
                                ? in.longLabels[v].c_str()
                                : "?";
                            bool sel = (v == selectedIdx);
                            if (ImGui::Selectable(lbl, sel)) {
                                in.value = (float)in.longValues[v];
                                undoNeeded = true;
                            }
                        }
                        ImGui::EndCombo();
                    }
                    consumedInputs.insert("fontFamily");
                    ImGui::Dummy(ImVec2(0, 4));
                }

                // 2) Size + Kerning row — paired sliders, half-width each.
                if (sizeIdx >= 0 || kerningIdx >= 0) {
                    float avail = ImGui::GetContentRegionAvail().x;
                    float halfW = (avail - ImGui::GetStyle().ItemSpacing.x) * 0.5f;
                    if (halfW < 60.0f) halfW = 60.0f;

                    if (sizeIdx >= 0) {
                        auto& in = inputs[sizeIdx];
                        float v = std::get<float>(in.value);
                        ImGui::SetNextItemWidth(halfW);
                        if (ImGui::DragFloat("##typoSize", &v, 0.001f,
                                             in.minVal, in.maxVal, "Size %.3f")) {
                            in.value = v;
                        }
                        if (ImGui::IsItemActivated()) undoNeeded = true;
                        consumedInputs.insert("textScale");
                    }
                    if (sizeIdx >= 0 && kerningIdx >= 0) ImGui::SameLine();
                    if (kerningIdx >= 0) {
                        auto& in = inputs[kerningIdx];
                        float v = std::get<float>(in.value);
                        ImGui::SetNextItemWidth(halfW);
                        if (ImGui::DragFloat("##typoKern", &v, 0.005f,
                                             in.minVal, in.maxVal, "Kern %.2f")) {
                            in.value = v;
                        }
                        if (ImGui::IsItemActivated()) undoNeeded = true;
                        consumedInputs.insert("kerning");
                    }
                    ImGui::Dummy(ImVec2(0, 4));
                }
            }
        }

        // Per-layer "voice-control edit mode" flag — toggled from the
        // options menu below. When on, every float param shows an inline
        // voice/audio source combo so users can wire bindings without
        // hunting for the right-click popover.
        static std::unordered_set<uint32_t> s_paramEditMode;
        bool paramEdit = s_paramEditMode.count(layer->id) > 0;
        // Deferred layer-delete from the options menu — applied after
        // we've finished rendering this frame so the layer isn't yanked
        // out from under the rest of the panel.
        int pendingDelete = -1;

        if (!inputs.empty()) {
            sectionBreak();

            // ── Options menu (⋯): Delete shader / Edit voice control ──
            // Sits at the top of the parameters section. Edit mode is
            // per-layer so toggling it on one shader doesn't affect
            // others.
            {
                float availW = ImGui::GetContentRegionAvail().x;
                ImGui::SetCursorPosX(ImGui::GetCursorPosX() + availW - 28.0f);
                if (ImGui::SmallButton(paramEdit ? "DONE##optsBtn" : "···##optsBtn")) {
                    if (paramEdit) {
                        s_paramEditMode.erase(layer->id);
                    } else {
                        ImGui::OpenPopup("##shaderOptsMenu");
                    }
                }
                if (ImGui::BeginPopup("##shaderOptsMenu")) {
                    if (ImGui::MenuItem("Edit voice control")) {
                        s_paramEditMode.insert(layer->id);
                    }
                    ImGui::Separator();
                    if (ImGui::MenuItem("Delete shader")) {
                        if (layerStack) {
                            for (int li = 0; li < layerStack->count(); li++) {
                                if ((*layerStack)[li]->id == layer->id) {
                                    pendingDelete = li;
                                    break;
                                }
                            }
                        }
                    }
                    ImGui::EndPopup();
                }
            }

            if (paramEdit) {
                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.95f, 0.78f, 0.32f, 0.95f));
                ImGui::TextWrapped("Voice-control edit mode — pick a source per parameter.");
                ImGui::PopStyleColor();
                ImGui::Dummy(ImVec2(0, 4));
            }

            // "Parameters" header removed — shader inputs render directly
            // after the composition section. Each input has its own label
            // row so the group heading was redundant noise.
            ImGui::Dummy(ImVec2(0, 4));
            {
            for (int i = 0; i < (int)inputs.size(); i++) {
                auto& input = inputs[i];
                if (consumedInputs.count(input.name)) continue;
                ImGui::PushID(i + 10000);

                if (input.type == "float") {
                    auto& bindings = shaderSrc->audioBindings();
                    auto bit = bindings.find(input.name);
                    bool isBound = (bit != bindings.end() && bit->second.signal != AudioSignal::None);

                    // Pick a format that fits the range.
                    const char* fmt = "%.2f";
                    float range = input.maxVal - input.minVal;
                    if (range > 100.0f)      fmt = "%.0f";
                    else if (range > 10.0f)  fmt = "%.1f";
                    else if (range < 1.0f)   fmt = "%.3f";

                    float v = std::get<float>(input.value);
                    std::string lblUp = upperLabel(input.name);
                    ParamSliderResult ps = paramSlider("##val", lblUp.c_str(),
                                                      &v, input.minVal, input.maxVal,
                                                      isBound, fmt);
                    if (ps.changed) input.value = v;
                    if (ps.activated) undoNeeded = true;
                    if (ps.openBindMenu) ImGui::OpenPopup("##audiobind");

                    if (ImGui::BeginPopup("##audiobind")) {
                        static const char* signalNames[] = { "None", "Level", "Bass", "Mid", "High", "Beat", "MIDI" };
                        bool isNew = (bindings.find(input.name) == bindings.end());
                        AudioBinding& ab = bindings[input.name];
                        if (isNew) { ab.rangeMin = input.minVal; ab.rangeMax = input.maxVal; }
                        int sigIdx = (int)ab.signal;
                        ImGui::Text("Source");
                        ImGui::SetNextItemWidth(120);
                        if (ImGui::Combo("##sig", &sigIdx, signalNames, IM_ARRAYSIZE(signalNames))) {
                            ab.signal = (AudioSignal)sigIdx;
                        }
                        if (ab.signal == AudioSignal::MidiCC) {
                            ImGui::Text("MIDI CC");
                            ImGui::SetNextItemWidth(55);
                            ImGui::InputInt("##cc", &ab.midiCC, 1, 1);
                            if (ab.midiCC < -1) ab.midiCC = -1;
                            if (ab.midiCC > 127) ab.midiCC = 127;
                            ImGui::SameLine();
                            ImGui::Text("Ch");
                            ImGui::SameLine();
                            ImGui::SetNextItemWidth(55);
                            int ch1 = ab.midiChannel + 1; // display 1-16 (0 = any)
                            if (ImGui::InputInt("##chan", &ch1, 1, 1)) {
                                if (ch1 < 0) ch1 = 0;
                                if (ch1 > 16) ch1 = 16;
                                ab.midiChannel = ch1 - 1; // -1 = any
                            }
                            if (midi) {
                                bool learning = midi->isLearning();
                                if (learning) {
                                    // Active/destructive: monochrome-friendly red.
                                    ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(0.85f, 0.30f, 0.32f, 0.45f));
                                    if (ImGui::Button("Learning... (move a knob)", ImVec2(-1, 0))) {
                                        midi->stopLearn();
                                    }
                                    ImGui::PopStyleColor();
                                    if (midi->hasLearnEvent()) {
                                        auto evt = midi->lastLearnEvent();
                                        ab.midiCC = evt.number;
                                        ab.midiChannel = evt.channel;
                                        midi->stopLearn();
                                    }
                                } else {
                                    if (ImGui::Button("MIDI Learn", ImVec2(-1, 0))) {
                                        midi->startLearn();
                                    }
                                }
                                if (!midi->isOpen()) {
                                    ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(1.0f, 0.6f, 0.2f, 1.0f));
                                    ImGui::TextWrapped("No MIDI device open");
                                    ImGui::PopStyleColor();
                                }
                            }
                        }
                        if (ab.signal != AudioSignal::None) {
                            // Give the audio-reactive controls room to use the
                            // panel's row idioms (full-width pill tracks).
                            ImGui::Dummy(ImVec2(220.0f, 0));

                            // Output range — dual-handle slider + the two
                            // scrubbable numeric fields share ab.rangeMin/Max
                            // storage, so they stay in sync for free. Drag
                            // speed scales with the param's own span.
                            dimLabel("OUTPUT RANGE", kRowLabel, false);
                            float dragSpd = (input.maxVal - input.minVal) * 0.005f;
                            if (dragSpd <= 0.0f) dragSpd = 0.01f;
                            // Live driven value = the same map applyAudioBindings
                            // applies each frame: rangeMin + smoothed*(span).
                            // ab is a reference into m_audioBindings, so this
                            // reads the value the apply path just wrote.
                            float liveDriven = ab.rangeMin +
                                ab.smoothedValue * (ab.rangeMax - ab.rangeMin);
                            rangeSlider("##arng", "Min / Max",
                                        &ab.rangeMin, &ab.rangeMax,
                                        input.minVal, input.maxVal,
                                        &liveDriven);
                            if (dragPair("##armin", "Min", &ab.rangeMin,
                                         "##armax", "Max", &ab.rangeMax,
                                         dragSpd, input.minVal, input.maxVal)) {
                                // Keep min<=max regardless of which field moved.
                                if (ab.rangeMin > ab.rangeMax)
                                    std::swap(ab.rangeMin, ab.rangeMax);
                            }

                            // Smoothing — higher = gentler/slower follower.
                            dimLabel("SMOOTHING", kRowLabel, false);
                            pillSlider("Amount",
                                       &ab.smoothing, 0.0f, 1.0f, "%.2f");
                        }
                        ImGui::EndPopup();
                    }

                    // (The bound/unbound bar below has been replaced by paramSlider
                    // above, which draws a unified pill track and tints its fill amber
                    // when a binding is active.)

                    // Voice-control edit mode: inline source combo. Same
                    // AudioBindings the right-click popup writes — just
                    // discoverable without hidden gestures.
                    if (paramEdit) {
                        static const char* sigLabels[] = {
                            "Manual", "Audio Level", "Bass", "Mid",
                            "High", "Beat", "MIDI" };
                        bool isNew = (bindings.find(input.name) == bindings.end());
                        AudioBinding& ab = bindings[input.name];
                        if (isNew) {
                            ab.rangeMin = input.minVal;
                            ab.rangeMax = input.maxVal;
                        }
                        int sigIdx = (int)ab.signal;
                        ImGui::PushStyleColor(ImGuiCol_Text, kDimText);
                        ImGui::Text("VOICE / AUDIO");
                        ImGui::PopStyleColor();
                        ImGui::SetNextItemWidth(-1);
                        if (ImGui::Combo("##voicebind", &sigIdx,
                                         sigLabels, IM_ARRAYSIZE(sigLabels))) {
                            ab.signal = (AudioSignal)sigIdx;
                        }
                        ImGui::Dummy(ImVec2(0, 6));
                    }
                } else if (input.type == "color") {
                    glm::vec4 c = std::get<glm::vec4>(input.value);
                    std::string lblUp = upperLabel(input.name);
                    if (paramColorRow("##col", lblUp.c_str(), &c)) {
                        input.value = c;
                        undoNeeded = true;
                    }
                } else if (input.type == "bool") {
                    bool b = std::get<bool>(input.value);
                    std::string lblUp = upperLabel(input.name);
                    if (paramToggleRow("##bool", lblUp.c_str(), &b)) {
                        input.value = b;
                        undoNeeded = true;
                    }
                } else if (input.type == "point2D") {
                    // Two stacked paramSliders (X / Y) keeps the clean
                    // label-top, pill-track-below rhythm.
                    glm::vec2 p = std::get<glm::vec2>(input.value);
                    std::string lblUp = upperLabel(input.name);
                    std::string labelX = lblUp + "  X";
                    std::string labelY = lblUp + "  Y";
                    auto rx = paramSlider("##px", labelX.c_str(), &p.x,
                                          input.minVec.x, input.maxVec.x, false);
                    auto ry = paramSlider("##py", labelY.c_str(), &p.y,
                                          input.minVec.y, input.maxVec.y, false);
                    if (rx.changed || ry.changed) input.value = p;
                    if (rx.activated || ry.activated) undoNeeded = true;
                } else if (input.type == "long") {
                    // Named enum → dropdown combo; numeric-only → paramSlider
                    // with integer format. Dropdowns keep long option lists
                    // (ease-in variants, blend modes, etc.) from taking up
                    // a wall of horizontal pills.
                    float v = std::get<float>(input.value);
                    int iv = (int)v;
                    if (!input.longLabels.empty()) {
                        ImGui::Dummy(ImVec2(0, kRowGapY));
                        std::string lblUp = upperLabel(input.name);

                        int cur = iv;
                        if (cur < 0) cur = 0;
                        if (cur > (int)input.longLabels.size() - 1)
                            cur = (int)input.longLabels.size() - 1;
                        const char* preview = input.longLabels[cur].c_str();
                        ImGui::PushID(input.name.c_str());
                        // Enum combo (e.g. COLORMODE) parks at the SAME shared
                        // control column as RESOLUTION via labelGutter() —
                        // no more per-row right-edge math that left every
                        // enum row's label-to-combo gap a different width.
                        labelGutter(lblUp.c_str(), kDimText);
                        if (ImGui::BeginCombo("##longCombo", preview)) {
                            for (int i = 0; i < (int)input.longLabels.size(); i++) {
                                bool sel = (i == cur);
                                if (ImGui::Selectable(input.longLabels[i].c_str(), sel)) {
                                    input.value = (float)i;
                                    undoNeeded = true;
                                }
                                if (sel) ImGui::SetItemDefaultFocus();
                            }
                            ImGui::EndCombo();
                        }
                        ImGui::PopID();
                        ImGui::Dummy(ImVec2(0, kRowPadY));
                    } else {
                        float fv = (float)iv;
                        std::string lblUp = upperLabel(input.name);
                        auto r = paramSlider("##val", lblUp.c_str(),
                                             &fv, input.minVal, input.maxVal,
                                             false, "%.0f");
                        if (r.changed) input.value = (float)(int)(fv + 0.5f);
                        if (r.activated) undoNeeded = true;
                    }
                } else if (input.type == "text") {
                    std::string text = std::get<std::string>(input.value);
                    int maxLen = (int)input.maxVal;
                    if (maxLen <= 0) maxLen = 12;

                    std::string lblUp = upperLabel(input.name);
                    ImGui::PushStyleColor(ImGuiCol_Text, kDimText);
                    ImGui::Text("%s", lblUp.c_str());
                    ImGui::PopStyleColor();

                    // Data bus binding dropdown
                    DataBus* bus = (speech) ? speech->dataBus : nullptr;
                    uint32_t layerId = (speech) ? speech->activeLayerId : 0;
                    std::string currentBinding = bus ? bus->binding(layerId, input.name) : "";
                    bool isBound = !currentBinding.empty();

                    // Source-binding combo on its own row so the text field
                    // below can take the panel's full width. Previously the
                    // combo + input + MIC were jammed onto one line and the
                    // input was crushed to whatever remained after a 100 px
                    // combo and a 30 px MIC button.
                    if (bus) {
                        auto keys = DataBus::availableKeys();
                        std::string currentLabel = "Manual";
                        for (auto& k : keys) {
                            if (k.key == currentBinding) { currentLabel = k.label; break; }
                        }
                        ImGui::SetNextItemWidth(-1);
                        if (ImGui::BeginCombo("##bind", currentLabel.c_str(), ImGuiComboFlags_NoArrowButton)) {
                            for (auto& k : keys) {
                                bool sel = (k.key == currentBinding);
                                if (ImGui::Selectable(k.label.c_str(), sel)) {
                                    bus->bind(layerId, input.name, k.key);
                                }
                            }
                            ImGui::EndCombo();
                        }
                        ImGui::Dummy(ImVec2(0, 2));
                    }

                    if (isBound) {
                        // Bound value renders as a read-only block taking the
                        // full row — no manual edit / MIC needed.
                        std::string val = bus ? bus->get(currentBinding) : "";
                        if (val.size() > (size_t)maxLen) val = val.substr(val.size() - maxLen);
                        ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.85f, 0.87f, 0.92f, 0.85f));
                        ImGui::TextWrapped("%s", val.empty() ? "..." : val.c_str());
                        ImGui::PopStyleColor();
                    } else {
                        // Manual mode: tall single-line text input + MIC button.
                        // Input takes full row minus mic + spacing.
                        char textBuf[256] = {};
                        strncpy(textBuf, text.c_str(), sizeof(textBuf) - 1);

                        float micW = (speech && speech->available) ? 44.0f : 0.0f;
                        float spacing = (micW > 0.0f) ? ImGui::GetStyle().ItemSpacing.x : 0.0f;
                        // Larger input height — defaults to ~22 px, bump to 32 px
                        // by pushing FramePadding before the InputText.
                        ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(8.0f, 8.0f));
                        ImGui::SetNextItemWidth(-(spacing + micW + 1.0f));
                        if (ImGui::InputText("##val", textBuf, (size_t)maxLen + 1,
                                             ImGuiInputTextFlags_CharsUppercase)) {
                            input.value = std::string(textBuf);
                        }
                        bool inputActivated = ImGui::IsItemActivated();
                        ImGui::PopStyleVar();
                        if (inputActivated) undoNeeded = true;

                        if (speech && speech->available) {
                            ImGui::SameLine();
                            bool isTarget = speech->listening &&
                                            speech->targetSource == shaderSrc &&
                                            speech->targetParam == input.name;
                            // Match input height (32 px) for visual alignment.
                            ImVec2 micSize(micW, ImGui::GetFrameHeight());
                            if (isTarget) {
                                ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(0.85f, 0.30f, 0.32f, 0.30f));
                                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(0.85f, 0.30f, 0.32f, 0.55f));
                                ImGui::PushStyleColor(ImGuiCol_ButtonActive, ImVec4(0.85f, 0.30f, 0.32f, 0.75f));
                                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.85f, 0.30f, 0.32f, 1.0f));
                                if (ImGui::Button("STOP", micSize)) {
                                    speech->listening = false;
                                    speech->targetSource = nullptr;
                                    speech->targetParam.clear();
                                }
                                ImGui::PopStyleColor(4);
                            } else {
                                ImGui::PushStyleColor(ImGuiCol_Button, ImVec4(1.0f, 1.0f, 1.0f, 0.15f));
                                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, ImVec4(1.0f, 1.0f, 1.0f, 0.30f));
                                ImGui::PushStyleColor(ImGuiCol_ButtonActive, ImVec4(1.0f, 1.0f, 1.0f, 0.50f));
                                ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(1.0f, 1.0f, 1.0f, 1.0f));
                                if (ImGui::Button("MIC", micSize)) {
                                    speech->listening = true;
                                    speech->targetSource = shaderSrc;
                                    speech->targetParam = input.name;
                                }
                                ImGui::PopStyleColor(4);
                            }
                        }
                    }
                } else if (input.type == "image" && layerStack) {
                    // Image input — dropdown to pick a layer as texture source.
                    // Common ShaderClaw placeholder names ("inputTex",
                    // "inputImage", "input", "tex", "sourceTex", "iChannel0")
                    // all render as a friendlier "Texture" label so every
                    // shader that accepts one reads consistently. Non-generic
                    // names (e.g. "from"/"to" on transitions, or descriptive
                    // per-shader names) keep the original label.
                    auto isGenericImageName = [](const std::string& n) {
                        static const char* generics[] = {
                            "inputTex", "inputtex", "inputImage", "inputimage",
                            "input", "tex", "texture",
                            "sourceTex", "sourcetex", "source",
                            "iChannel0", "ichannel0",
                            "image",
                        };
                        for (const char* g : generics) if (n == g) return true;
                        return false;
                    };
                    std::string imgLabelStorage = isGenericImageName(input.name)
                                                  ? std::string("TEXTURE")
                                                  : upperLabel(input.name);
                    const char* displayLabel = imgLabelStorage.c_str();

                    auto& bindings = shaderSrc->imageBindings();
                    auto it = bindings.find(input.name);
                    uint32_t currentSrcId = (it != bindings.end()) ? it->second.sourceLayerId : 0;

                    // Build label for current selection
                    std::string preview = "None";
                    for (int li = 0; li < layerStack->count(); li++) {
                        auto& other = (*layerStack)[li];
                        if (other->id == currentSrcId && other->source) {
                            preview = other->name + " (" + other->source->typeName() + ")";
                            break;
                        }
                    }

                    ParamRow::Begin(displayLabel);
                    if (ImGui::BeginCombo("##imgsrc", preview.c_str())) {
                        // "None" option
                        if (ImGui::Selectable("None", currentSrcId == 0)) {
                            shaderSrc->unbindImageInput(input.name);
                        }
                        // List all other layers that have a texture
                        for (int li = 0; li < layerStack->count(); li++) {
                            auto& other = (*layerStack)[li];
                            if (other->id == layer->id) continue; // skip self
                            if (!other->source || other->source->textureId() == 0) continue;
                            std::string label = other->name + " (" + other->source->typeName() + ")";
                            bool selected = (other->id == currentSrcId);
                            if (ImGui::Selectable(label.c_str(), selected)) {
                                shaderSrc->bindImageInput(input.name,
                                    other->source->textureId(),
                                    other->source->width(),
                                    other->source->height(),
                                    other->id,
                                    other->source->isFlippedV());
                            }
                        }
                        ImGui::EndCombo();
                    }
                }

                ImGui::PopID();
            }

            // "+ Add Effect" trailer at the end of the per-shader
            // parameters. Wires into the same OpenPopup target as the
            // top-of-panel Effects section, so clicking surfaces the
            // EffectType menu and pushing it lands in layer->effects.
            // Mirrors the Figma pattern of "Fill / Stroke / Effects"
            // with an Add control at the section tail.
            ImGui::Dummy(ImVec2(0, 2));
            if (accentBtn("+ Add Effect", -1)) {
                ImGui::OpenPopup("##AddEffect");
            }
            ImGui::Dummy(ImVec2(0, 2));
            } // end sectionHeader("Parameters")

            // --- Transition (moved to the BOTTOM of Parameters) ---
            // Transition type + duration + optional shader-transition block.
            // Rendered after all shader parameters so scrolling reveals the
            // transition controls as a "next step" rather than a header.
            {
                static const char* transLabels[(int)TransitionType::COUNT] = {};
                for (int i = 0; i < (int)TransitionType::COUNT; i++)
                    transLabels[i] = transitionTypeName((TransitionType)i);
                int curT = (int)layer->transitionType;
                ImGui::Dummy(ImVec2(0, 4));
                ParamRow::Begin("TRANSITION");
                if (ImGui::BeginCombo("##TransType", transLabels[curT])) {
                    for (int i = 0; i < (int)TransitionType::COUNT; i++) {
                        bool sel = (i == curT);
                        if (ImGui::Selectable(transLabels[i], sel)) {
                            layer->transitionType = (TransitionType)i;
                        }
                        if (sel) ImGui::SetItemDefaultFocus();
                    }
                    ImGui::EndCombo();
                }

                pillSlider("Duration", &layer->transitionDuration, 0.0f, 5.0f, "%.2fs");

                if (layer->transitionType == TransitionType::Shader) {
                    static char pathBuf[512];
                    static const Layer* lastLayer = nullptr;
                    static std::string lastPath;
                    if (lastLayer != layer.get() || lastPath != layer->transitionShaderPath) {
                        std::snprintf(pathBuf, sizeof(pathBuf), "%s", layer->transitionShaderPath.c_str());
                        lastLayer = layer.get();
                        lastPath = layer->transitionShaderPath;
                    }
                    ImGui::SetNextItemWidth(-FLT_MIN);
                    if (ImGui::InputText("##TransShader", pathBuf, sizeof(pathBuf))) {
                        layer->transitionShaderPath = pathBuf;
                        layer->transitionShaderInst.reset();
                    }
                    if (ImGui::SmallButton("Dissolve")) {
                        layer->transitionShaderPath = "shaders/transitions/dissolve_noise.fs";
                        layer->transitionShaderInst.reset();
                    }
                    ImGui::SameLine();
                    if (ImGui::SmallButton("Wet Paint")) {
                        layer->transitionShaderPath = "shaders/transitions/wet_paint.fs";
                        layer->transitionShaderInst.reset();
                    }

                    if (layerStack) {
                        static int triggerTargetIdx = -1;
                        const char* preview = "<pick source layer>";
                        if (triggerTargetIdx >= 0 && triggerTargetIdx < layerStack->count() &&
                            (*layerStack)[triggerTargetIdx].get() != layer.get()) {
                            preview = (*layerStack)[triggerTargetIdx]->name.c_str();
                        }
                        ImGui::SetNextItemWidth(-60.0f);
                        if (ImGui::BeginCombo("##TransB", preview)) {
                            for (int li = 0; li < layerStack->count(); li++) {
                                auto other = (*layerStack)[li];
                                if (!other || other.get() == layer.get() || !other->source) continue;
                                bool sel = (triggerTargetIdx == li);
                                if (ImGui::Selectable(other->name.c_str(), sel)) triggerTargetIdx = li;
                            }
                            ImGui::EndCombo();
                        }
                        ImGui::SameLine();
                        bool canTrigger = triggerTargetIdx >= 0 && triggerTargetIdx < layerStack->count()
                                          && (*layerStack)[triggerTargetIdx].get() != layer.get()
                                          && (*layerStack)[triggerTargetIdx]->source
                                          && !layer->transitionShaderPath.empty()
                                          && !layer->shaderTransitionActive;
                        if (!canTrigger) ImGui::BeginDisabled();
                        if (ImGui::Button("Trigger", ImVec2(-FLT_MIN, 0))) {
                            layer->startShaderTransition((*layerStack)[triggerTargetIdx]->source);
                        }
                        if (!canTrigger) ImGui::EndDisabled();
                    }
                }
            }
        }

#ifdef HAS_WHISPER
        if (speech && speech->available && speech->whisper) {
            auto& devices = speech->whisper->captureDevices();
            if (!devices.empty()) {
                int sel = speech->whisper->selectedDevice();
                std::string preview = (sel < 0) ? "Default" :
                    (sel < (int)devices.size() ? devices[sel].name : "Unknown");
                ParamRow::Begin("MIC");
                if (ImGui::BeginCombo("##mic_device", preview.c_str())) {
                    if (ImGui::Selectable("Default", sel < 0)) {
                        if (!speech->listening) speech->whisper->selectDevice(-1);
                    }
                    for (auto& d : devices) {
                        bool isSel = (d.index == sel);
                        std::string lbl = d.name + (d.isDefault ? " *" : "");
                        if (ImGui::Selectable(lbl.c_str(), isSel)) {
                            if (!speech->listening) speech->whisper->selectDevice(d.index);
                        }
                    }
                    ImGui::EndCombo();
                }
            }
        }
#endif

        if (!shaderSrc->description().empty()) {
            ImGui::PushStyleColor(ImGuiCol_Text, kDimText);
            ImGui::TextWrapped("%s", shaderSrc->description().c_str());
            ImGui::PopStyleColor();
        }

        // Apply deferred delete from the options menu — done last so the
        // layer pointer stays valid through the panel render. Edit-mode
        // flag is cleared so a future layer reusing this id starts fresh.
        if (pendingDelete >= 0 && layerStack) {
            s_paramEditMode.erase(layer->id);
            layerStack->removeLayer(pendingDelete);
        }
    }

    // Tiling — emitted as the panel's LAST child so the bulky Mosaic /
    // Feather controls sit at the very bottom and never push Transform,
    // Blend, or the shader Parameters below the fold.
    emitTilingSection();

    ImGui::End();
}
