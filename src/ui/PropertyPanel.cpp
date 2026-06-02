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
#include "app/OutputZone.h"
#include "ui/LayerPanel.h"
#include "sources/ShaderSource.h"
#include "sources/VideoSource.h"
#include "sources/ParticleSource.h"
#include "sources/FluidSource.h"
#include "sources/FluidSource3D.h"
#include "sources/HologramModelSource.h"
#include "sources/MovingCompanySource.h"
#include "app/DataBus.h"
#include "app/MIDIManager.h"
#ifdef HAS_WHISPER
#include "speech/WhisperSpeech.h"
#endif
#include <imgui.h>
#include <imgui_internal.h>
#include <cstdio>
#include <cmath>
#include <cctype>
#include <algorithm>
#include <utility>
#include <unordered_set>
#include <random>
#include <unordered_map>

// Uppercase a string in place — used for the editorial ALL-CAPS shader
// parameter labels. ImGui IDs use a separate `##paramname` token so the
// uppercased visible label doesn't disturb widget identity.
static std::string upperLabel(const std::string& s) {
    std::string out = s;
    for (auto& ch : out) ch = (char)toupper((unsigned char)ch);
    return out;
}

// --- Semantic palette ------------------------------------------------------
// ONE source of truth for every chrome color in the layer parameters panel.
// The screenshot showed too many slightly-different tones serving the SAME
// role (three dim-label shades, four control-bg grays, mismatched borders).
// They are collapsed here into a few intentional roles. Pick the cleanest
// existing values as canonical; do NOT redesign the vibe — only remove drift.
//
//   kColHeader      section-header text (LAYERS / TRANSFORM / …)
//   kColLabel       ONE dim tone for every sub-label (OPACITY, BLEND,
//                   TEXTURE, param names, chevron, "ADD EFFECT", crop, etc.)
//   kColValue       readout / primary control text (1.00, "Normal", names)
//   kColCtrlBg      fill of EVERY pill / box / field / slider track
//   kColCtrlBgHover one subtle hover variant derived from the fill
//   kColCtrlBgActive one subtle active variant derived from the fill
//   kColCtrlBorder  ONE hairline for boxed controls
//   kColAccent      THE accent (audio amber / selection / slider fill)
//   kColAccentDim   same accent at a lower alpha (slider fills / spans)
//
// Distinct-by-role stays distinct: accent/selection still stand out, and
// disabled/hidden states are an ALPHA of kColLabel — never a new hue.
static constexpr ImU32 kColHeader       = IM_COL32(247, 249, 254, 255);
static constexpr ImU32 kColLabel        = IM_COL32(150, 158, 172, 230);
static constexpr ImU32 kColLabelDim     = IM_COL32(150, 158, 172, 120); // alpha-derived: hidden eye / disabled
static constexpr ImU32 kColValue        = IM_COL32(235, 240, 250, 245);
static constexpr ImU32 kColCtrlBg       = IM_COL32(255, 255, 255, 16);
static constexpr ImU32 kColCtrlBgHover  = IM_COL32(255, 255, 255, 32);
static constexpr ImU32 kColCtrlBgActive = IM_COL32(255, 255, 255, 48);
static constexpr ImU32 kColCtrlBorder   = IM_COL32(255, 255, 255, 22);
static constexpr ImU32 kColAccent       = IM_COL32(232, 150,  70, 255);
static constexpr ImU32 kColAccentDim    = IM_COL32(232, 150,  70, 200);
static constexpr ImU32 kColTrackBg      = IM_COL32(255, 255, 255, 14); // slider/track recess (subtle inset of ctrl bg)

// ImVec4 mirrors for the PushStyleColor() call sites. Same values as the
// ImU32 constants above — one palette, two encodings, no new numbers.
static const ImVec4 kColLabelV    = ImVec4(0.588f, 0.620f, 0.675f, 0.902f); // == kColLabel
static const ImVec4 kColValueV    = ImVec4(0.922f, 0.941f, 0.980f, 0.961f); // == kColValue
static const ImVec4 kColHeaderV   = ImVec4(0.969f, 0.976f, 0.996f, 1.0f);   // == kColHeader
static const ImVec4 kColCtrlBgV       = ImVec4(1.0f, 1.0f, 1.0f, 0.063f);  // == kColCtrlBg
static const ImVec4 kColCtrlBgHoverV  = ImVec4(1.0f, 1.0f, 1.0f, 0.125f);  // == kColCtrlBgHover
static const ImVec4 kColCtrlBgActiveV = ImVec4(1.0f, 1.0f, 1.0f, 0.188f);  // == kColCtrlBgActive
static const ImVec4 kColAccentV       = ImVec4(0.910f, 0.588f, 0.275f, 1.0f); // == kColAccent
// Destructive/danger stays its own role (red) — semantically distinct from
// the neutral chrome, intentionally NOT collapsed into the palette.
static const ImVec4 kColDanger    = ImVec4(0.85f, 0.30f, 0.32f, 1.0f);

// Back-compat aliases — the panel body still references these names in many
// places; they now all resolve to the ONE canonical dim-label tone (no more
// kDimText / kMuted / kRowLabel drift). Kept as aliases so call sites read
// clearly without a mass rename, but there is only one underlying value.
static const ImVec4& kDimText  = kColLabelV;
static const ImVec4& kMuted    = kColLabelV;
static const ImVec4& kRowLabel = kColLabelV;
static constexpr ImU32  kSepColor  = IM_COL32(255, 255, 255, 12);

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
// Inter-element vertical gaps trimmed ~37% (single source of truth) so the
// whole panel tightens uniformly into a denser grid. kStepY (base unit) and
// kLabelGapY (label→track legibility) are intentionally NOT reduced — only
// the empty padding BETWEEN rows/sections shrinks, never control content.
static constexpr float kRowGapY    = kStepY * 1.25f;  // 5  — above a row (was 8)
static constexpr float kRowPadY    = kStepY * 1.25f;  // 5  — below a row (was 8)
static constexpr float kLabelGapY  = kStepY * 2.5f;   // 10 — label→track (unchanged: legibility)
static constexpr float kSectionGap = kStepY * 2.5f;   // 10 — sub-group gap (was 16)
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
    ImGui::PushStyleColor(ImGuiCol_Button,        kColCtrlBgV);
    ImGui::PushStyleColor(ImGuiCol_ButtonHovered, kColCtrlBgHoverV);
    ImGui::PushStyleColor(ImGuiCol_ButtonActive,  kColCtrlBgActiveV);
    ImGui::PushStyleColor(ImGuiCol_Text,          kColValueV);
    bool c = ImGui::Button(label, ImVec2(w, 0));
    ImGui::PopStyleColor(4);
    return c;
}

// ===========================================================================
// UNIFIED VALUE SLIDER — the single canonical slider used by EVERY value
// control in the parameters panel (opacity, shader params, audio amount /
// smoothing, particle params, etc.). Style is copied EXACTLY from the
// OPACITY slider the user likes: 6px rounded pill track, a real solid
// circular thumb (radius 7) with a thin dark outline, dim label top-left,
// bright value top-right, all on the shared kRowGapY / kLabelGapY / kRowPadY
// rhythm. Routing every helper through this guarantees identical look + size.
//
// Resolution: the drag maps mouse-x → value as a CONTINUOUS float across the
// full row width (many pixels ⇒ fine steps — the old narrow gutter-width
// opacity track was the source of the "too few steps" feel). NO quantization
// of the underlying value; Shift still snaps to a coarse 0.05 grid as a
// deliberate convenience only. Callers keep their own min/max/value/binding;
// `accent` tints the fill (amber) to signal an active binding — that is
// state, not style, so it is a parameter rather than a hardcode.
//
// `outActivated` (optional) is set true on the frame the drag begins so
// callers can snapshot for undo. Returns true on any value change this frame.
static bool unifiedSlider(const char* idSuffix, const char* label,
                          float* v, float lo, float hi,
                          const char* fmt, bool accent = false,
                          bool* outActivated = nullptr) {
    ImGui::PushID(idSuffix);
    // Leading gap — identical to every other row helper.
    ImGui::Dummy(ImVec2(0, kRowGapY));
    float w = ImGui::GetContentRegionAvail().x;
    ImVec2 rowStart = ImGui::GetCursorScreenPos();
    float labelH  = ImGui::GetFontSize();
    // Canonical OPACITY-slider geometry: 6px pill track, r=7 circle thumb.
    const float trackH  = 6.0f;
    const float handleR = 7.0f;
    float rowH = labelH + kLabelGapY + trackH;
    ImDrawList* dl = ImGui::GetWindowDrawList();

    // Dim label (left) + bright value (right) on the label row.
    char valbuf[32]; snprintf(valbuf, sizeof(valbuf), fmt, *v);
    ImVec2 valSize = ImGui::CalcTextSize(valbuf);
    dl->AddText(rowStart, kColLabel, label);
    dl->AddText(ImVec2(rowStart.x + w - valSize.x, rowStart.y),
                kColValue, valbuf);

    // Full-width track hit zone — wide ⇒ many sub-steps ⇒ fine resolution.
    float trackY = rowStart.y + labelH + kLabelGapY;
    ImGui::SetCursorScreenPos(ImVec2(rowStart.x, trackY - 7.0f));
    bool pressed = ImGui::InvisibleButton("##uslider_track",
                                          ImVec2(w, trackH + 14.0f));
    bool active  = ImGui::IsItemActive();
    bool hovered = ImGui::IsItemHovered();
    if (ImGui::IsItemActivated() && outActivated) *outActivated = true;
    bool changed = false;
    if (active || pressed) {
        // Continuous map across the full pixel width — no rounding of the
        // underlying float (Shift snaps to a coarse 0.05 grid by choice).
        float mx = ImGui::GetIO().MousePos.x - rowStart.x;
        float t = (w > 0.0f) ? mx / w : 0.0f;
        if (t < 0.0f) t = 0.0f; if (t > 1.0f) t = 1.0f;
        float newV = lo + t * (hi - lo);
        if (ImGui::GetIO().KeyShift) newV = std::round(newV / 0.05f) * 0.05f;
        if (newV != *v) { *v = newV; changed = true; }
    }
    float norm = (hi > lo) ? (*v - lo) / (hi - lo) : 0.0f;
    if (norm < 0.0f) norm = 0.0f; if (norm > 1.0f) norm = 1.0f;

    // Track bg + fill — EXACT opacity-slider colors (amber fill when accent).
    ImU32 fillCol = accent ? kColAccentDim
                           : kColCtrlBgActive;
    dl->AddRectFilled(ImVec2(rowStart.x, trackY),
                      ImVec2(rowStart.x + w, trackY + trackH),
                      kColTrackBg, trackH * 0.5f);
    dl->AddRectFilled(ImVec2(rowStart.x, trackY),
                      ImVec2(rowStart.x + w * norm + 0.5f, trackY + trackH),
                      fillCol, trackH * 0.5f);
    // Real solid circular thumb + thin dark outline — the opacity handle.
    float hx = rowStart.x + w * norm;
    float hy = trackY + trackH * 0.5f;
    ImU32 handleCol = (active || hovered) ? IM_COL32(255, 255, 255, 255)
                                          : kColValue;
    dl->AddCircleFilled(ImVec2(hx, hy), handleR, handleCol);
    dl->AddCircle      (ImVec2(hx, hy), handleR, IM_COL32(0, 0, 0, 110), 0, 1.2f);

    // Trailing pad = kRowPadY — shared rhythm.
    ImVec2 curScreen = ImGui::GetCursorScreenPos();
    float targetY = rowStart.y + rowH + kRowPadY;
    float advanceY = targetY - curScreen.y;
    if (advanceY > 0.0f) ImGui::Dummy(ImVec2(w, advanceY));
    ImGui::PopID();
    return changed;
}

// Section header with chevron; click anywhere in the row to toggle.
// Returns true when the section is OPEN (content should be drawn).
//
// Laws-of-UX notes:
//  - Proximity: tight (2px) top margin + 4px bottom margin so the header
//    visually groups with its content, not the previous section.
//  - Aesthetic-usability: chevron + label stay calm; hover brightens label.
//  - Fitts: full-row hit target (InvisibleButton spans the panel width).
// Real collapsible section header. Clicking the chevron/label toggles a
// PERSISTENT open/closed state and the return value reflects it, so the
// existing `if (sectionHeader(...)) { body }` callers actually collapse the
// body. Look/spacing/firstSection rhythm are unchanged from the prior
// version — only the chevron and the collapse behaviour are new.
//
//  - `open`     : caller-owned persistent flag. When non-null it is the
//                 source of truth (toggled here). When null we fall back to
//                 an internal per-label persistent map so the nullptr
//                 callers (Setup/Audio/Scenes) collapse too. Default state
//                 is EXPANDED (matches the old always-true behaviour).
//  - `reserveRight` : width (px) at the right edge the header hit-target
//                 must NOT cover, so a caller-drawn trailing control (the
//                 LAYERS visibility toggle) keeps its own click and ID and
//                 doesn't fight the collapse hit-target.
static bool sectionHeader(const char* label, bool* open,
                          bool firstSection = false,
                          float reserveRight = 0.0f) {
    // Persistent fallback state for nullptr callers — keyed by label so each
    // section keeps its own open/closed across frames. Default = expanded.
    static std::unordered_map<std::string, bool> sFallbackOpen;
    bool* state = open;
    if (!state) {
        auto it = sFallbackOpen.find(label);
        if (it == sFallbackOpen.end())
            it = sFallbackOpen.emplace(label, true).first;
        state = &it->second;
    }

    if (!firstSection)
        ImGui::Dummy(ImVec2(0, 10));           // top breathing room (32 → 20 → 10; tight but still grouped; firstSection still gets none)
    ImVec2 rowStart = ImGui::GetCursorScreenPos();
    float rowW  = ImGui::GetContentRegionAvail().x;
    float fontSize = ImGui::GetFontSize();
    float headlineSize = fontSize * 1.30f;     // 1.85 → 1.30: compact H2 (smaller but still a clear header)

    ImDrawList* dl = ImGui::GetWindowDrawList();

    // Chevron — ▾ when open, ▸ when collapsed. Drawn as a small triangle so
    // it matches the calm vector idiom (no glyph font dependency).
    // RIGHT-ALIGNED: parked at the row's right edge, minus its own size,
    // minus `reserveRight` so it sits just LEFT of (clear of) a caller-
    // owned trailing widget (the LAYERS visibility toggle). When there is
    // a reserved region, an extra 8px keeps the chevron from kissing it.
    float chevR = headlineSize * 0.26f;
    float chevCy = rowStart.y + headlineSize * 0.5f;
    float rightPad = (reserveRight > 0.0f) ? (reserveRight + 8.0f) : 0.0f;
    float chevCx = rowStart.x + rowW - rightPad - chevR;
    ImU32 chevCol = kColLabel;
    if (*state) {
        ImVec2 a(chevCx - chevR, chevCy - chevR * 0.55f);
        ImVec2 b(chevCx + chevR, chevCy - chevR * 0.55f);
        ImVec2 c(chevCx, chevCy + chevR * 0.75f);
        dl->AddTriangleFilled(a, b, c, chevCol);     // ▾ open
    } else {
        ImVec2 a(chevCx - chevR * 0.55f, chevCy - chevR);
        ImVec2 b(chevCx - chevR * 0.55f, chevCy + chevR);
        ImVec2 c(chevCx + chevR * 0.75f, chevCy);
        dl->AddTriangleFilled(a, b, c, chevCol);     // ▸ collapsed
    }
    // Label stays LEFT-aligned at the row origin (chevron is now on the
    // right, so no leading chevron gutter is needed). DISPLAY-ONLY uppercase:
    // every section header renders in ALL CAPS. The original `label` is kept
    // verbatim for ImGui IDs / PushID / the persistent collapse-state map and
    // the hit-test below — only the drawn glyphs are upper-cased.
    std::string display(label);
    for (char& ch : display)
        ch = static_cast<char>(std::toupper(static_cast<unsigned char>(ch)));
    float labelX = rowStart.x;
    dl->AddText(ImGui::GetFont(), headlineSize,
                ImVec2(labelX, rowStart.y),
                kColHeader, display.c_str());

    // Full-row click target (chevron + label) toggles collapse. Stop short
    // of `reserveRight` so a caller-owned trailing widget keeps its click.
    float hitW = rowW - reserveRight;
    if (hitW < 1.0f) hitW = 1.0f;
    ImGui::SetCursorScreenPos(rowStart);
    ImGui::PushID(label);
    if (ImGui::InvisibleButton("##secHdr",
                               ImVec2(hitW, headlineSize)))
        *state = !*state;
    ImGui::PopID();

    // Reserve vertical space + bottom padding (20 → 12 → 6). The
    // headline + 6px gap below is the breathing rhythm — no divider,
    // no chrome, the air does the hierarchy work. 6px clears the
    // headline glyph + chevron (chevron half-height ≈ headlineSize*0.26)
    // without clipping; a collapsed header no longer carries a big
    // empty band, while an expanded section's first row still gets a
    // small clean gap below the header.
    ImGui::SetCursorScreenPos(rowStart);
    ImGui::Dummy(ImVec2(rowW, headlineSize + 6.0f));
    return *state;
}

// Public wrappers so panels rendered OUTSIDE this class (Sources / Audio /
// Mapping, which live in Application.cpp + WarpEditor.cpp) share the exact
// same section-title and label rhythm — single source of truth, no drift.
bool PropertyPanel::PanelSectionHeader(const char* label, bool firstSection) {
    return sectionHeader(label, nullptr, firstSection);
}
float PropertyPanel::PanelLabel(const char* text) {
    return labelGutter(text, kDimText);
}
void PropertyPanel::PushPanelStyle() {
    ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(kStepY * 4.5f, kStepY * 4.5f)); // 18,18 symmetric inset
    ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing,   ImVec2(kColGap, kStepY * 0.5f));       // 12,2
    ImGui::PushStyleVar(ImGuiStyleVar_FramePadding,  ImVec2(kInnerPad, kStepY * 2.5f));     // 12,10
    ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 6.0f);
    ImGui::PushStyleVar(ImGuiStyleVar_GrabRounding,  6.0f);
}
void PropertyPanel::PopPanelStyle() {
    ImGui::PopStyleVar(5);
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
            // Selected pill — the accent, so the selection stands out
            // (one accent value, used everywhere selection means "active").
            ImGui::PushStyleColor(ImGuiCol_Button,        kColAccentV);
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, kColAccentV);
            ImGui::PushStyleColor(ImGuiCol_ButtonActive,  kColAccentV);
            ImGui::PushStyleColor(ImGuiCol_Text,          ImVec4(0.06f, 0.07f, 0.10f, 1.0f));
        } else {
            ImGui::PushStyleColor(ImGuiCol_Button,        kColCtrlBgV);
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, kColCtrlBgHoverV);
            ImGui::PushStyleColor(ImGuiCol_ButtonActive,  kColCtrlBgActiveV);
            ImGui::PushStyleColor(ImGuiCol_Text,          kColValueV);
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
    // Thin wrapper — routes straight through the one canonical slider so it
    // looks/sizes IDENTICALLY to the OPACITY slider (same track, same real
    // circular thumb, same colors, same fine continuous resolution).
    return unifiedSlider(label, label, v, lo, hi, fmt);
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
    ImVec2 boltPos    = ImVec2(0, 0); // screen pos of the sparkle (popup anchor)
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
    r.boltPos = ImVec2(rowStart.x - 2.0f, rowStart.y); // anchor for bind popup
    float labelH  = ImGui::GetFontSize();
    // CANONICAL OPACITY-slider geometry — identical to unifiedSlider /
    // pillSlider / the inline opacity track (6px pill, r=7 circle thumb).
    const float trackH  = 6.0f;
    const float handleR = 7.0f;
    float rowH    = labelH + kLabelGapY + trackH;
    float boltBox = 18.0f; // hit-target square around the sparkle glyph

    ImDrawList* dl = ImGui::GetWindowDrawList();

    // ⚡ Sparkle bind button — leftmost affordance (UNCHANGED behavior).
    // Clicking opens the bind menu; the row right-click does too.
    ImGui::SetCursorScreenPos(ImVec2(rowStart.x - 2.0f,
                                     rowStart.y + (labelH - boltBox) * 0.5f));
    bool boltClicked = ImGui::InvisibleButton("##bolt", ImVec2(boltBox, boltBox));
    bool boltHovered = ImGui::IsItemHovered();
    if (boltClicked) r.openBindMenu = true;

    ImVec2 bMin(rowStart.x - 2.0f, rowStart.y + (labelH - boltBox) * 0.5f);
    ImVec2 bMax(bMin.x + boltBox, bMin.y + boltBox);
    ImU32 bgCol = boltHovered ? kColCtrlBgHover
                              : kColCtrlBg;
    dl->AddRectFilled(bMin, bMax, bgCol, 4.0f);

    ImU32 boltCol = bound       ? kColAccent
                  : boltHovered ? kColValue
                                : kColLabel;
    lucide::sparkles(dl,
                     rowStart.x - 2.0f + boltBox * 0.5f,
                     rowStart.y + (labelH - boltBox) * 0.5f + boltBox * 0.5f,
                     14.0f, boltCol);

    float labelX = rowStart.x + boltBox + 4.0f;

    // ---- Canonical slider drawing (EXACTLY the opacity-slider style) ----
    // Dim label (offset past the sparkle) + bright right-aligned value.
    char valbuf[32]; snprintf(valbuf, sizeof(valbuf), fmt, *v);
    ImVec2 valSize = ImGui::CalcTextSize(valbuf);
    dl->AddText(ImVec2(labelX, rowStart.y), kColLabel, label);
    dl->AddText(ImVec2(rowStart.x + w - valSize.x, rowStart.y),
                kColValue, valbuf);

    // Right-click anywhere on the label row also opens the bind menu.
    float trackY = rowStart.y + labelH + kLabelGapY;
    ImGui::SetCursorScreenPos(ImVec2(labelX, rowStart.y));
    ImGui::InvisibleButton("##row", ImVec2(w - (labelX - rowStart.x), labelH + 4));
    if (ImGui::IsItemClicked(ImGuiMouseButton_Right)) r.openBindMenu = true;

    // Full-width continuous track — many pixels ⇒ fine resolution, no
    // quantization of the underlying float (Shift snaps to 0.05 by choice).
    ImGui::SetCursorScreenPos(ImVec2(rowStart.x, trackY - 7.0f));
    ImGui::InvisibleButton("##track", ImVec2(w, trackH + 14.0f));
    bool tActive  = ImGui::IsItemActive();
    bool hovered  = ImGui::IsItemHovered();
    if (ImGui::IsItemActivated()) r.activated = true;
    if (ImGui::IsItemClicked(ImGuiMouseButton_Right)) r.openBindMenu = true;
    if (tActive) {
        float mx = ImGui::GetIO().MousePos.x - rowStart.x;
        float t = (w > 0.0f) ? mx / w : 0.0f;
        if (t < 0.0f) t = 0.0f; if (t > 1.0f) t = 1.0f;
        float newV = lo + t * (hi - lo);
        if (ImGui::GetIO().KeyShift) newV = std::round(newV / 0.05f) * 0.05f;
        if (newV != *v) { *v = newV; r.changed = true; }
    }

    float norm = (hi > lo) ? (*v - lo) / (hi - lo) : 0.0f;
    if (norm < 0.0f) norm = 0.0f; if (norm > 1.0f) norm = 1.0f;

    // Track bg + fill — EXACT opacity colors (amber fill signals a binding).
    ImU32 fillCol = bound ? kColAccentDim
                          : kColCtrlBgActive;
    dl->AddRectFilled(ImVec2(rowStart.x, trackY),
                      ImVec2(rowStart.x + w, trackY + trackH),
                      kColTrackBg, trackH * 0.5f);
    dl->AddRectFilled(ImVec2(rowStart.x, trackY),
                      ImVec2(rowStart.x + w * norm + 0.5f, trackY + trackH),
                      fillCol, trackH * 0.5f);
    // Real solid circular thumb + thin dark outline — the opacity handle.
    float hx = rowStart.x + w * norm;
    float hy = trackY + trackH * 0.5f;
    ImU32 handleCol = (tActive || hovered) ? IM_COL32(255, 255, 255, 255)
                                           : kColValue;
    dl->AddCircleFilled(ImVec2(hx, hy), handleR, handleCol);
    dl->AddCircle      (ImVec2(hx, hy), handleR, IM_COL32(0, 0, 0, 110), 0, 1.2f);

    // Bottom breathing room = kRowPadY — identical to every other row helper.
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
                kColLabel, label);

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
    dl->AddCircle(ImVec2(cx, cy), swatchR, kColCtrlBorder, 0, 1.0f);

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
                kColLabel, label);

    float sx = rowStart.x + w - switchW - 2.0f;
    float sy = rowStart.y + (rowH - switchH) * 0.5f;

    ImGui::SetCursorScreenPos(ImVec2(sx, sy));
    bool clicked = ImGui::InvisibleButton("##sw", ImVec2(switchW, switchH));
    if (clicked) { *b = !*b; }

    // White when ON, faint white track when OFF — matches the rest of the
    // chrome and removes the lone orange accent that was reading as warning.
    ImU32 trackCol = *b ? kColValue : kColCtrlBg;
    dl->AddRectFilled(ImVec2(sx, sy), ImVec2(sx + switchW, sy + switchH),
                      trackCol, switchH * 0.5f);
    float knobR = switchH * 0.5f - 2.0f;
    float knobX = *b ? sx + switchW - knobR - 2.0f : sx + knobR + 2.0f;
    float knobY = sy + switchH * 0.5f;
    dl->AddCircleFilled(ImVec2(knobX, knobY), knobR,
                        *b ? IM_COL32(13, 18, 26, 255) : kColValue);

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
    // CANONICAL OPACITY-slider geometry — same 6px pill track + r=7 thumbs
    // as unifiedSlider / pillSlider / paramSlider / the inline opacity track.
    const float trackH = 6.0f, handleR = 7.0f;
    float rowH   = labelH + kLabelGapY + trackH;
    float span   = (absHi > absLo) ? (absHi - absLo) : 1.0f;
    ImDrawList* dl = ImGui::GetWindowDrawList();

    // Dim label (left) + bright right-aligned "min – max" readout — same
    // text idiom/colors as every other unified slider.
    char val[48];
    snprintf(val, sizeof(val), "%.2f  -  %.2f", *lo, *hi);
    ImVec2 vs = ImGui::CalcTextSize(val);
    dl->AddText(ImVec2(rowStart.x, rowStart.y), kColLabel, label);
    dl->AddText(ImVec2(rowStart.x + w - vs.x, rowStart.y),
                kColValue, val);

    float trackY = rowStart.y + labelH + kLabelGapY;
    auto toX = [&](float v) { return rowStart.x + (v - absLo) / span * w; };
    auto toV = [&](float x) {
        float t = (x - rowStart.x) / w;
        if (t < 0) t = 0; if (t > 1) t = 1;
        return absLo + t * span;
    };

    ImGui::SetCursorScreenPos(ImVec2(rowStart.x, trackY - 7.0f));
    ImGui::InvisibleButton("##rng", ImVec2(w, trackH + 14.0f));
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

    // Track, selected span, then the two solid thumbs — EXACT canonical
    // opacity colors (track bg 14; amber span = the meaningful range fill;
    // each thumb a real filled circle + the same thin dark outline).
    dl->AddRectFilled(ImVec2(rowStart.x, trackY),
                      ImVec2(rowStart.x + w, trackY + trackH),
                      kColTrackBg, trackH * 0.5f);
    dl->AddRectFilled(ImVec2(toX(*lo), trackY), ImVec2(toX(*hi), trackY + trackH),
                      kColAccentDim, trackH * 0.5f);
    for (float hv : { *lo, *hi }) {
        float hx = toX(hv), hy = trackY + trackH * 0.5f;
        dl->AddCircleFilled(ImVec2(hx, hy), handleR,
                            active ? IM_COL32(255, 255, 255, 255)
                                   : kColValue);
        dl->AddCircle(ImVec2(hx, hy), handleR, IM_COL32(0, 0, 0, 110), 0, 1.2f);
    }

    // Live driven-value marker — thin bright caret + triangle, on top of the
    // round thumbs, using the same amber accent as the selected-span fill.
    if (liveVal) {
        float lv = *liveVal;
        if (lv < absLo) lv = absLo; else if (lv > absHi) lv = absHi;
        float lx = toX(lv);
        dl->AddLine(ImVec2(lx, trackY - 3.0f), ImVec2(lx, trackY + trackH + 3.0f),
                    kColAccent, 1.75f);
        float ty = trackY - 3.0f;
        dl->AddTriangleFilled(ImVec2(lx - 4.0f, ty - 5.0f), ImVec2(lx + 4.0f, ty - 5.0f),
                              ImVec2(lx, ty), kColAccent);
    }

    ImGui::SetCursorScreenPos(ImVec2(rowStart.x, rowStart.y + rowH + kRowPadY));

    // Tiny live numeric readout under the track (dim label + bright value),
    // same text idiom / spacing as the rest of the panel.
    if (liveVal) {
        char lvbuf[48];
        snprintf(lvbuf, sizeof(lvbuf), "live  %.3f", *liveVal);
        ImVec2 lr = ImGui::GetCursorScreenPos();
        dl->AddText(ImVec2(lr.x, lr.y), kColLabel, lvbuf);
        ImGui::Dummy(ImVec2(0, ImGui::GetFontSize() + kRowPadY));
    }

    ImGui::PopID();
    return changed;
}

// Shared audio/MIDI "modulation" popover — used by every bind-capable
// paramSlider (shader `##audiobind` + fluid `##fbind`). Two craft goals:
//   1. Never slide under the bottom transport nav. It opens UPWARD from the
//      sparkle (pivot bottom-left), with the anchor clamped above the nav
//      band, so the whole popover always stays on-screen.
//   2. Read as an integrated panel element, not a default ImGui box: fixed-
//      width opaque tinted container, hairline border, generous padding, a
//      titled header (sparkle + the param name), dim uppercase section labels,
//      and the same row rhythm as the inspector.
// Caller opens it with ImGui::OpenPopup(popupId) from the slider's bolt; this
// lazily creates bindings[key] only while the popover is open.
static void audioBindPopup(const char* popupId, const char* paramLabel,
                           std::map<std::string, AudioBinding>& bindings,
                           const std::string& key, float lo, float hi,
                           MIDIManager* midi, ImVec2 boltPos) {
    ImGuiViewport* vp = ImGui::GetMainViewport();
    const float kNavReserve = 78.0f;   // transport-nav band + breathing margin
    float maxBottom = vp->WorkPos.y + vp->WorkSize.y - kNavReserve;
    float anchorY = boltPos.y;
    if (anchorY > maxBottom) anchorY = maxBottom;
    // pivot (0,1): the popover's BOTTOM-left sits at the anchor and it grows
    // upward — guaranteeing it never extends down into the nav.
    ImGui::SetNextWindowPos(ImVec2(boltPos.x, anchorY),
                            ImGuiCond_Appearing, ImVec2(0.0f, 1.0f));
    ImGui::SetNextWindowSizeConstraints(
        ImVec2(254.0f, 0.0f), ImVec2(254.0f, vp->WorkSize.y * 0.82f));

    ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding, ImVec2(14, 13));
    ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding, 11.0f);
    ImGui::PushStyleVar(ImGuiStyleVar_WindowBorderSize, 1.0f);
    ImGui::PushStyleColor(ImGuiCol_PopupBg, ImVec4(0.069f, 0.077f, 0.099f, 0.985f));
    ImGui::PushStyleColor(ImGuiCol_Border,  ImVec4(1.0f, 1.0f, 1.0f, 0.10f));

    if (ImGui::BeginPopup(popupId)) {
        ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing, ImVec2(8, 7));
        ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 6.0f);
        ImGui::PushStyleColor(ImGuiCol_FrameBg,        kColCtrlBgV);
        ImGui::PushStyleColor(ImGuiCol_FrameBgHovered, kColCtrlBgHoverV);
        ImGui::PushStyleColor(ImGuiCol_FrameBgActive,  kColCtrlBgActiveV);

        bool isNew = (bindings.find(key) == bindings.end());
        AudioBinding& ab = bindings[key];
        if (isNew) { ab.rangeMin = lo; ab.rangeMax = hi; }

        // ── Header: sparkle + the parameter being modulated, then a hairline.
        {
            ImDrawList* dl = ImGui::GetWindowDrawList();
            ImVec2 p = ImGui::GetCursorScreenPos();
            lucide::sparkles(dl, p.x + 7.0f, p.y + ImGui::GetFontSize() * 0.5f,
                             13.0f, kColAccent);
            ImGui::SetCursorScreenPos(ImVec2(p.x + 20.0f, p.y));
            ImGui::PushStyleColor(ImGuiCol_Text, kColHeaderV);
            ImGui::TextUnformatted(paramLabel);
            ImGui::PopStyleColor();
            ImGui::Dummy(ImVec2(0, 5));
            ImVec2 a = ImGui::GetCursorScreenPos();
            dl->AddLine(a, ImVec2(a.x + ImGui::GetContentRegionAvail().x, a.y),
                        kColCtrlBorder, 1.0f);
            ImGui::Dummy(ImVec2(0, 4));
        }

        static const char* signalNames[] = {
            "None", "Level", "Bass", "Mid", "High", "Beat", "MIDI" };
        dimLabel("SOURCE", kRowLabel, false);
        int sigIdx = (int)ab.signal;
        ImGui::SetNextItemWidth(-1);
        if (ImGui::Combo("##sig", &sigIdx, signalNames, IM_ARRAYSIZE(signalNames)))
            ab.signal = (AudioSignal)sigIdx;

        if (ab.signal == AudioSignal::MidiCC) {
            ImGui::Dummy(ImVec2(0, 5));
            dimLabel("MIDI CC", kRowLabel, false);
            ImGui::SetNextItemWidth(70);
            ImGui::InputInt("##cc", &ab.midiCC, 1, 1);
            if (ab.midiCC < -1)  ab.midiCC = -1;
            if (ab.midiCC > 127) ab.midiCC = 127;
            ImGui::SameLine();
            dimLabel("Ch", kRowLabel, true);
            ImGui::SetNextItemWidth(70);
            int ch1 = ab.midiChannel + 1;   // display 1-16 (0 = any)
            if (ImGui::InputInt("##chan", &ch1, 1, 1)) {
                if (ch1 < 0)  ch1 = 0;
                if (ch1 > 16) ch1 = 16;
                ab.midiChannel = ch1 - 1;
            }
            if (midi) {
                ImGui::Dummy(ImVec2(0, 3));
                if (midi->isLearning()) {
                    ImGui::PushStyleColor(ImGuiCol_Button,
                        ImVec4(0.85f, 0.30f, 0.32f, 0.45f));
                    if (ImGui::Button("Learning... (move a knob)", ImVec2(-1, 0)))
                        midi->stopLearn();
                    ImGui::PopStyleColor();
                    if (midi->hasLearnEvent()) {
                        auto evt = midi->lastLearnEvent();
                        ab.midiCC = evt.number;
                        ab.midiChannel = evt.channel;
                        midi->stopLearn();
                    }
                } else if (ImGui::Button("MIDI Learn", ImVec2(-1, 0))) {
                    midi->startLearn();
                }
                if (!midi->isOpen()) {
                    ImGui::PushStyleColor(ImGuiCol_Text, kColAccentV);
                    ImGui::TextWrapped("No MIDI device open");
                    ImGui::PopStyleColor();
                }
            }
        }

        if (ab.signal != AudioSignal::None) {
            ImGui::Dummy(ImVec2(0, 6));
            dimLabel("OUTPUT RANGE", kRowLabel, false);
            float dragSpd = (hi - lo) * 0.005f;
            if (dragSpd <= 0.0f) dragSpd = 0.01f;
            float liveDriven = ab.rangeMin +
                ab.smoothedValue * (ab.rangeMax - ab.rangeMin);
            rangeSlider("##arng", "Min / Max", &ab.rangeMin, &ab.rangeMax,
                        lo, hi, &liveDriven);
            if (dragPair("##armin", "Min", &ab.rangeMin,
                         "##armax", "Max", &ab.rangeMax, dragSpd, lo, hi)) {
                if (ab.rangeMin > ab.rangeMax)
                    std::swap(ab.rangeMin, ab.rangeMax);
            }
            ImGui::Dummy(ImVec2(0, 6));
            dimLabel("SMOOTHING", kRowLabel, false);
            pillSlider("Amount", &ab.smoothing, 0.0f, 1.0f, "%.2f");
        }

        ImGui::PopStyleColor(3);
        ImGui::PopStyleVar(2);
        ImGui::EndPopup();
    }
    ImGui::PopStyleColor(2);
    ImGui::PopStyleVar(3);
}

// One bindable parameter for the audio-reactivity preset row: its id, current
// value, and range. Shader params and FluidSource members both reduce to this.
struct PresetParam { std::string name; float cur, lo, hi; };

// Audio-reactivity intensity presets. depthLo/Hi = modulation depth as a
// fraction of each param's range; smoothLo/Hi = follower smoothing range;
// useBeat allows the punchy Beat signal. All tuned to stay smooth.
struct FxPreset { const char* name; float depthLo, depthHi, smoothLo, smoothHi; bool useBeat; };
static const FxPreset kAudioPresets[3] = {
    { "Subtle",  0.07f, 0.15f, 0.84f, 0.93f, false },
    { "Medium",  0.15f, 0.28f, 0.72f, 0.85f, false },
    { "Intense", 0.26f, 0.45f, 0.60f, 0.76f, false },
};

// Renders the [Subtle][Medium][Intense][re-roll] audio-reactivity row, shared
// by the shader "Effects" and fluid "Fluid" sections. Clicking a preset binds
// ~5 random params from `params` into `bindings` at that intensity (anchored to
// each param's current value so silence keeps the look); the active preset
// highlights and toggles off; the re-roll icon picks a fresh random set at the
// same intensity. `stateKey` (the layer id) tracks the active preset. Returns
// true when bindings changed.
static bool audioPresetRow(std::map<std::string, AudioBinding>& bindings,
                           const std::vector<PresetParam>& params,
                           uint32_t stateKey) {
    bool changed = false;
    int activeCount = 0;
    for (auto& kv : bindings)
        if (kv.second.signal != AudioSignal::None) activeCount++;
    bool on = activeCount > 0;

    static std::unordered_map<uint32_t, int> sActivePreset;
    int activeIdx = -1;
    if (on) {
        auto it = sActivePreset.find(stateKey);
        if (it != sActivePreset.end()) activeIdx = it->second;
    }

    auto applyPreset = [&](int p) {
        const FxPreset& pr = kAudioPresets[p];
        std::vector<int> idx;
        for (int i = 0; i < (int)params.size(); i++) idx.push_back(i);
        static std::mt19937 rng{std::random_device{}()};
        std::uniform_real_distribution<float> u01(0.0f, 1.0f);
        std::shuffle(idx.begin(), idx.end(), rng);
        const AudioSignal cs[] = { AudioSignal::Level, AudioSignal::Bass,
                                   AudioSignal::Mid, AudioSignal::High };
        bindings.clear();
        int n = std::min(5, (int)idx.size());
        for (int j = 0; j < n; j++) {
            const PresetParam& pp = params[idx[j]];
            float span = pp.hi - pp.lo;
            float depth = span * (pr.depthLo +
                          (pr.depthHi - pr.depthLo) * u01(rng));
            AudioBinding ab;
            ab.signal = (pr.useBeat && rng() % 4 == 0) ? AudioSignal::Beat
                                                       : cs[rng() % 4];
            ab.rangeMin = pp.cur;
            ab.rangeMax = std::min(pp.hi, pp.cur + depth);
            if (ab.rangeMax - ab.rangeMin < span * 0.04f) {
                ab.rangeMin = std::max(pp.lo, pp.cur - depth);
                ab.rangeMax = pp.cur;
            }
            ab.smoothing = pr.smoothLo + (pr.smoothHi - pr.smoothLo) * u01(rng);
            bindings[pp.name] = ab;
        }
        sActivePreset[stateKey] = p;
        changed = true;
    };

    float avail = ImGui::GetContentRegionAvail().x;
    float gap   = ImGui::GetStyle().ItemSpacing.x;
    float refW  = 30.0f;
    float btnW  = (avail - refW - gap * 3.0f) / 3.0f;
    if (btnW < 36.0f) btnW = 36.0f;

    ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 7.0f);
    for (int p = 0; p < 3; p++) {
        bool sel = (activeIdx == p);
        if (sel) {
            ImGui::PushStyleColor(ImGuiCol_Button,        kColAccentV);
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, kColAccentV);
            ImGui::PushStyleColor(ImGuiCol_ButtonActive,  kColAccentV);
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.08f, 0.06f, 0.03f, 1.0f));
        } else {
            ImGui::PushStyleColor(ImGuiCol_Button,        kColCtrlBgV);
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, kColCtrlBgHoverV);
            ImGui::PushStyleColor(ImGuiCol_ButtonActive,  kColCtrlBgActiveV);
            ImGui::PushStyleColor(ImGuiCol_Text, kColValueV);
        }
        if (p > 0) ImGui::SameLine();
        if (ImGui::Button(kAudioPresets[p].name, ImVec2(btnW, 28))) {
            if (sel) { bindings.clear(); sActivePreset.erase(stateKey); changed = true; }
            else applyPreset(p);
        }
        ImGui::PopStyleColor(4);
        if (ImGui::IsItemHovered())
            ImGui::SetTooltip(sel
                ? "Audio reactivity on (click to turn off)."
                : "Bind ~5 random params to audio at this intensity.");
    }

    ImGui::SameLine();
    ImGui::PushStyleColor(ImGuiCol_Button,        kColCtrlBgV);
    ImGui::PushStyleColor(ImGuiCol_ButtonHovered, kColCtrlBgHoverV);
    ImGui::PushStyleColor(ImGuiCol_ButtonActive,  kColCtrlBgActiveV);
    ImVec2 rcur = ImGui::GetCursorScreenPos();
    bool reroll = ImGui::Button("##fxReroll", ImVec2(refW, 28));
    {
        ImU32 ic = ImGui::IsItemHovered() ? kColValue : kColLabel;
        lucide::repeat(ImGui::GetWindowDrawList(),
                       rcur.x + refW * 0.5f, rcur.y + 14.0f, 15.0f, ic);
    }
    ImGui::PopStyleColor(3);
    if (ImGui::IsItemHovered())
        ImGui::SetTooltip("Re-roll a new random set at the current intensity.");
    if (reroll) applyPreset(activeIdx >= 0 ? activeIdx : 0);

    ImGui::PopStyleVar();
    ImGui::Dummy(ImVec2(0, 4));
    return changed;
}

void PropertyPanel::render(std::shared_ptr<Layer> layer, bool& maskEditMode,
                           SpeechState* speech, MosaicAudioState* mosaicAudio,
                           float appTime, LayerStack* layerStack,
                           BPMSync* bpmSync, SceneManager* sceneManager,
                           int* audioDeviceIdx, MIDIManager* midi,
                           OutputZone* canvasZone, float* targetFPS) {
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
    // Horizontal inset trimmed 28→18 (kStepY * 4.5) so every row is slightly
    // wider — single source of truth: all rows derive width from
    // GetContentRegionAvail()/GetCursorStartPos() which already respect this
    // WindowPadding, so the trim applies uniformly and the grid stays aligned.
    // Vertical padding (Y) now equals horizontal (X) — same kStepY * 4.5
    // multiplier — so the content margin is provably symmetric: the empty
    // space ABOVE the first section title ("LAYERS", firstSection=true, no
    // top Dummy) equals the left/right inset (and the bottom inset). The
    // shared multiplier guarantees x == y with no separate magic number.
    PushPanelStyle();   // canonical inset shared by ALL right-dock panels
    ImGui::Begin("        ###Properties");
    PopPanelStyle();

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
            mn, mx, kColCtrlBorder,
            ImGui::GetStyle().WindowRounding, 0, 1.0f);
    }

    // 6-pill quick-nav is rendered ONCE at the right-dock host level (above
    // the auto tab bar) in UIManager::renderFloatPanelHosts. Per-panel
    // call removed to avoid duplicate bars + empty tab-strip space.
#if 0  // legacy inline implementation kept for reference, replaced by helper
    {
        const float kBarH = 50.0f;
        ImGui::PushStyleColor(ImGuiCol_ChildBg, IM_COL32(0, 0, 0, 0));
        ImGui::BeginChild("##propQuickBar", ImVec2(0, kBarH), false,
                          ImGuiWindowFlags_NoScrollbar |
                          ImGuiWindowFlags_NoScrollWithMouse);
        const float pillR = 18.0f;
        const float pillW = pillR * 2.0f;
        const float gap   = 14.0f;
        const float groupW = 6.0f * pillW + 5.0f * gap;
        float availW = ImGui::GetContentRegionAvail().x;
        float startX = (availW - groupW) * 0.5f;
        ImGui::SetCursorPosY((kBarH - pillW) * 0.5f);
        if (startX > 0.0f) ImGui::SetCursorPosX(ImGui::GetCursorPosX() + startX);

        // Lucide icons all share the 6-arg form (last is stroke width).
        using LucideFn = void (*)(ImDrawList*, float, float, float, ImU32, float);
        auto pill = [&](const char* id, UIManager::SourceTab kind,
                        LucideFn icon, const char* tip) {
            bool active = (m_uiManager && m_uiManager->activeSourcesTab() == kind);
            ImVec2 cur  = ImGui::GetCursorScreenPos();
            bool   clicked = ImGui::InvisibleButton(id, ImVec2(pillW, pillW));
            bool   hov     = ImGui::IsItemHovered();
            if (hov && tip) ImGui::SetTooltip("%s", tip);
            ImDrawList* dl = ImGui::GetWindowDrawList();
            ImU32 fill = active ? IM_COL32(255, 255, 255, 38)
                       : hov    ? IM_COL32(255, 255, 255, 22)
                                : IM_COL32(255, 255, 255, 12);
            float cx = cur.x + pillR, cy = cur.y + pillR;
            dl->AddCircleFilled(ImVec2(cx, cy), pillR, fill, 32);
            ImU32 tint = active ? IM_COL32(235, 240, 250, 245)
                                : IM_COL32(170, 180, 200, 200);
            icon(dl, cx, cy, 18.0f, tint, 1.6f);
            if (clicked && m_uiManager) m_uiManager->focusSourcesTab(kind);
        };

        // navPill — variant of pill() for tabs outside the Sources dock
        // (Properties/Mapping). Click routes through focusPanel() instead
        // of focusSourcesTab(); active flag is supplied by the caller.
        auto navPill = [&](const char* id, bool active, LucideFn icon,
                           const char* tip, const char* focusName) {
            ImVec2 cur = ImGui::GetCursorScreenPos();
            bool clicked = ImGui::InvisibleButton(id, ImVec2(pillW, pillW));
            bool hov     = ImGui::IsItemHovered();
            if (hov && tip) ImGui::SetTooltip("%s", tip);
            ImDrawList* dl = ImGui::GetWindowDrawList();
            ImU32 fill = active ? IM_COL32(255, 255, 255, 38)
                       : hov    ? IM_COL32(255, 255, 255, 22)
                                : IM_COL32(255, 255, 255, 12);
            float cx = cur.x + pillR, cy = cur.y + pillR;
            dl->AddCircleFilled(ImVec2(cx, cy), pillR, fill, 32);
            ImU32 tint = active ? IM_COL32(235, 240, 250, 245)
                                : IM_COL32(170, 180, 200, 200);
            icon(dl, cx, cy, 18.0f, tint, 1.6f);
            if (clicked && m_uiManager) m_uiManager->focusPanel(focusName);
        };

        // Always-visible top bar: Properties → Shader → Mic → Cam → Win → Mapping.
        // First pill is the Parameters panel itself (always active here);
        // Mapping (last) jumps to the Mapping tab.
        navPill("##qsProps", true, &lucide::sliders, "Parameters",
                "        ###Properties");
        ImGui::SameLine(0, gap);
        pill("##qsShader", UIManager::SourceTab::Shader, &lucide::zap,
             "Shaders");
        ImGui::SameLine(0, gap);
        pill("##qsMic",    UIManager::SourceTab::Mic,    &lucide::mic,
             "Voice / Etherea");
        ImGui::SameLine(0, gap);
        pill("##qsCam",    UIManager::SourceTab::Cam,    &lucide::camera,
             "Camera");
        ImGui::SameLine(0, gap);
        pill("##qsWin",    UIManager::SourceTab::Win,    &lucide::monitor,
             "Display / Capture");
        ImGui::SameLine(0, gap);
        navPill("##qsMap",  false, &lucide::vectorSquare, "Mapping",
                "        ###Mapping");

        // hairline divider so the bar reads as separated chrome
        ImDrawList* dl = ImGui::GetWindowDrawList();
        ImVec2 cmn = ImGui::GetWindowPos();
        ImVec2 cmx(cmn.x + ImGui::GetWindowSize().x,
                   cmn.y + ImGui::GetWindowSize().y);
        dl->AddLine(ImVec2(cmn.x, cmx.y - 0.5f),
                    ImVec2(cmx.x, cmx.y - 0.5f),
                    IM_COL32(255, 255, 255, 22), 1.0f);
        ImGui::EndChild();
        ImGui::PopStyleColor();
    }
#endif  // end legacy inline propQuickBar reference

    // Open the scrollable content region — everything below renders into
    // this child so the quick-bar above stays pinned during scroll. Both
    // End() paths (the empty-state early-return and the normal end of
    // render) close this child before closing the outer window.
    // Transparent child bg so the scroll region matches the panel exactly —
    // the global theme ChildBg carries a 2% white tint that otherwise reads
    // as a lighter "container" rectangle inside the parameters panel.
    ImGui::PushStyleColor(ImGuiCol_ChildBg, IM_COL32(0, 0, 0, 0));
    ImGui::BeginChild("##propContent", ImVec2(0, 0), false, 0);
    ImGui::PopStyleColor();

    // ── CANVAS ─────────────────────────────────────────────────────────
    // Canvas resolution / aspect ratio + frame rate. This is a Canvas-mode
    // composition concern, so it's pinned at the top of the inspector ONLY
    // in Canvas mode — in Stage mode the parameters panel is the spatial
    // Setup inspector and the resolution/FPS block would just be noise.
    if (canvasZone && UIManager::sMode == UIManager::WorkspaceMode::Canvas) {
        if (sectionHeader("Canvas", nullptr, /*firstSection=*/true)) {
            OutputZone& cz = *canvasZone;
            int cw = cz.width, ch = cz.height;

            // Aspect ratio, gcd-reduced (3840x2160 -> 16:9).
            auto igcd = [](int a, int b){ while (b){ int t=b; b=a%b; a=t; } return a<1?1:a; };
            int gg = igcd(cw, ch);
            char aspectBuf[24];
            snprintf(aspectBuf, sizeof(aspectBuf), "%d:%d", cw/std::max(gg,1), ch/std::max(gg,1));

            struct CRes { const char* label; int w; int h; };
            static const CRes cpres[] = {
                {"1080p · 16:9",    1920, 1080}, {"4K · 16:9",       3840, 2160},
                {"720p · 16:9",     1280, 720},  {"1440p · 16:9",    2560, 1440},
                {"Vertical · 9:16", 1080, 1920}, {"Square · 1:1",    1080, 1080},
                {"Ultrawide · 21:9",2560, 1080},
            };
            char resLabel[56];
            snprintf(resLabel, sizeof(resLabel), "%d x %d   (%s)", cw, ch, aspectBuf);

            labelGutter("RESOLUTION", kDimText);
            ImGui::SetNextItemWidth(-1);
            if (ImGui::BeginCombo("##canvasRes", resLabel)) {
                for (auto& p : cpres) {
                    bool sel = (cw == p.w && ch == p.h);
                    char it[56];
                    snprintf(it, sizeof(it), "%-18s %d x %d", p.label, p.w, p.h);
                    if (ImGui::Selectable(it, sel)) cz.resize(p.w, p.h);
                    if (sel) ImGui::SetItemDefaultFocus();
                }
                ImGui::EndCombo();
            }

            // Custom W x H (commit on Apply, not per keystroke).
            static int s_cw = 0, s_ch = 0;
            static OutputZone* s_lastZone = nullptr;
            if (s_lastZone != &cz) { s_cw = cw; s_ch = ch; s_lastZone = &cz; }
            ImGui::SetNextItemWidth(74); ImGui::InputInt("##canvasCW", &s_cw, 0);
            ImGui::SameLine(0, 6); ImGui::TextDisabled("x"); ImGui::SameLine(0, 6);
            ImGui::SetNextItemWidth(74); ImGui::InputInt("##canvasCH", &s_ch, 0);
            ImGui::SameLine(0, 8);
            bool okCustom = s_cw >= 64 && s_ch >= 64 && s_cw <= 16384 && s_ch <= 16384;
            if (!okCustom) ImGui::BeginDisabled();
            if (ImGui::SmallButton("Apply") && okCustom) cz.resize(s_cw, s_ch);
            if (!okCustom) ImGui::EndDisabled();

            // ── Frame rate ── live readout (color-coded) + target cap.
            ImGui::Dummy(ImVec2(0, 4));
            float liveFPS = ImGui::GetIO().Framerate;
            ImU32 fpsCol = liveFPS >= 50.0f ? IM_COL32(120, 220, 140, 255)
                         : liveFPS >= 30.0f ? IM_COL32(235, 205, 90, 255)
                                            : IM_COL32(235, 110, 110, 255);
            labelGutter("FRAME RATE", kDimText);
            ImGui::PushStyleColor(ImGuiCol_Text, fpsCol);
            ImGui::Text("%.0f fps  ", liveFPS);
            ImGui::PopStyleColor();
            ImGui::SameLine(0, 2);
            // live pulse dot
            {
                ImVec2 p = ImGui::GetCursorScreenPos();
                float r = 3.0f, cy = p.y + ImGui::GetTextLineHeight() * 0.5f;
                float pulse = 0.5f + 0.5f * sinf(appTime * 3.0f);
                ImGui::GetWindowDrawList()->AddCircleFilled(
                    ImVec2(p.x + r, cy), r, fpsCol, 12);
                (void)pulse;
                ImGui::Dummy(ImVec2(r * 2 + 4, ImGui::GetTextLineHeight()));
            }

            if (targetFPS) {
                struct FPSOpt { const char* label; float v; };
                static const FPSOpt fopts[] = {
                    {"Uncapped (vsync)", 0.0f}, {"24", 24.0f}, {"30", 30.0f},
                    {"48", 48.0f}, {"60", 60.0f}, {"120", 120.0f},
                };
                const char* cur = "Uncapped (vsync)";
                for (auto& o : fopts) if (*targetFPS == o.v) { cur = o.label; break; }
                labelGutter("TARGET", kDimText);
                ImGui::SetNextItemWidth(-1);
                if (ImGui::BeginCombo("##canvasFPS", cur)) {
                    for (auto& o : fopts) {
                        bool sel = (*targetFPS == o.v);
                        if (ImGui::Selectable(o.label, sel)) *targetFPS = o.v;
                        if (sel) ImGui::SetItemDefaultFocus();
                    }
                    ImGui::EndCombo();
                }
            }
            ImGui::Dummy(ImVec2(0, 6));
        }
    }

    // (Single LAYERS section — the full stacked list — lives below, in the
    // "LAYERS block" that also carries "+ Add New Layer".)

    // Stage Setup section — only when the workspace is Stage mode and
    // we have a StageView reference. Tool selection (Move/Rotate/Scale)
    // lives on the floating left toolbar; this panel is the displays
    // inspector ONLY so the user has a single source of truth.
    if (m_stageView && UIManager::sMode == UIManager::WorkspaceMode::Stage) {
        ImGui::PushStyleColor(ImGuiCol_Header,        kColCtrlBgV);
        ImGui::PushStyleColor(ImGuiCol_HeaderHovered, kColCtrlBgHoverV);
        ImGui::PushStyleColor(ImGuiCol_HeaderActive,  kColCtrlBgActiveV);
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
        ImGui::EndChild();   // ##propContent
        ImGui::End();
        return;
    }

    undoNeeded = false;

    // ── LAYERS block ──────────────────────────────────────────────────
    // Brings the LAYERS header, "+ Add New Layer", and a current-layer
    // selector to the TOP of the parameters panel (per the wireframe).
    // NOTHING here reimplements layer logic: Add trips the SAME
    // LayerPanel signal flags Application already consumes, and the nav
    // arrows drive the SAME shared selected-layer index, so the separate
    // Layers panel stays the source of truth and is left untouched.
    {
        ImGui::Dummy(ImVec2(0, 4));

        // Row: collapsible "LAYERS" header. The chevron/label toggles
        // collapse (persistent, default expanded). The visibility toggle
        // formerly drawn in this header's right gutter now lives on the
        // Layer Nav row as an eye button (see below), so the header reserves
        // no right gutter and its chevron sits flush at the right edge.
        static bool layersOpen = true;
        bool layersBodyOpen =
            sectionHeader("LAYERS", &layersOpen, /*firstSection=*/true,
                          /*reserveRight=*/0.0f);

        // "+ Add New Layer" — full-width pill. Opens the SAME popup the
        // Layers panel uses and sets the SAME wantsAdd* flags Application
        // already consumes (no duplicated load logic).
        if (layersBodyOpen && m_layerPanel) {
            ImGui::Dummy(ImVec2(0, kRowGapY));
            ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 100.0f);
            ImGui::PushStyleColor(ImGuiCol_Button,        kColCtrlBgV);
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, kColCtrlBgHoverV);
            ImGui::PushStyleColor(ImGuiCol_ButtonActive,  kColCtrlBgActiveV);
            ImGui::PushStyleColor(ImGuiCol_Text,          kColValueV);
            if (ImGui::Button("+ Add New Layer##paramsAddLayer",
                              ImVec2(-1, 0)))
                ImGui::OpenPopup("##AddLayerFromParams");
            ImGui::PopStyleColor(4);
            ImGui::PopStyleVar();
            if (ImGui::BeginPopup("##AddLayerFromParams")) {
                if (ImGui::MenuItem("Image..."))  m_layerPanel->wantsAddImage  = true;
                if (ImGui::MenuItem("Video..."))  m_layerPanel->wantsAddVideo  = true;
                if (ImGui::MenuItem("Shader...")) m_layerPanel->wantsAddShader = true;
                ImGui::EndPopup();
            }
        }

        // Layer list: EVERY layer as a stacked container (top of stack first,
        // matching composite order). Per row: eye visibility toggle, name
        // (".fs" stripped), click-to-select, ▲▼ reorder. Selection + add +
        // reorder all drive the shared state the Layers panel already owns.
        if (layersBodyOpen && m_selectedLayer && layerStack && layerStack->count() > 0) {
            int n = layerStack->count();
            ImDrawList* dl = ImGui::GetWindowDrawList();
            ImVec2 mp = ImGui::GetMousePos();
            float frameH = ImGui::GetFrameHeight();
            int reorderFrom = -1, reorderTo = -1;
            for (int i = n - 1; i >= 0; i--) {
                auto& L = (*layerStack)[i];
                if (!L) continue;
                ImGui::PushID(i);
                ImGui::Dummy(ImVec2(0, kRowGapY));
                float rowW = ImGui::GetContentRegionAvail().x;
                ImVec2 rs = ImGui::GetCursorScreenPos();
                float midY = rs.y + frameH * 0.5f;
                bool sel = (*m_selectedLayer == i);

                // Full-row select hit first; sub-zones (eye / chevrons) below
                // override on click via manual hit-test.
                ImGui::InvisibleButton("##lrowSel", ImVec2(rowW, frameH));
                bool rowHov = ImGui::IsItemHovered();

                ImU32 bg = sel    ? IM_COL32(255, 255, 255, 26)
                         : rowHov ? IM_COL32(255, 255, 255, 12)
                                  : IM_COL32(255, 255, 255, 5);
                dl->AddRectFilled(rs, ImVec2(rs.x + rowW, rs.y + frameH), bg, 6.0f);
                dl->AddRect(rs, ImVec2(rs.x + rowW, rs.y + frameH),
                            sel ? IM_COL32(255, 255, 255, 90) : kColCtrlBorder,
                            6.0f, 0, 1.0f);

                bool eOn = L->visible && !L->userHidden;

                // Eye toggle (manual hit-test).
                float eX = rs.x + 14.0f;
                bool overEye = fabsf(mp.x - eX) < 11 && fabsf(mp.y - midY) < 11;
                ImU32 eCol = eOn ? kColValue : kColLabelDim;
                if (overEye && rowHov) eCol = IM_COL32(255, 255, 255, 255);
                if (eOn) lucide::eye   (dl, eX, midY, 16.0f, eCol);
                else     lucide::eyeOff(dl, eX, midY, 16.0f, eCol);

                // Name (".fs" stripped for shader layers).
                std::string nm = L->name;
                if (nm.size() >= 3) {
                    std::string tl = nm.substr(nm.size() - 3);
                    for (auto& c : tl) c = (char)tolower((unsigned char)c);
                    if (tl == ".fs") nm.erase(nm.size() - 3);
                }
                dl->AddText(ImVec2(rs.x + 30.0f, midY - ImGui::GetFontSize() * 0.5f),
                            eOn ? kColValue : kColLabelDim, nm.c_str());

                // ▲▼ reorder chevrons (right). ▲ = toward top (on top).
                float chX = rs.x + rowW - 16.0f;
                bool overUp = (i < n - 1) && fabsf(mp.x - chX) < 10 &&
                              mp.y > rs.y && mp.y < midY;
                bool overDn = (i > 0) && fabsf(mp.x - chX) < 10 &&
                              mp.y > midY && mp.y < rs.y + frameH;
                ImU32 upCol = (i < n - 1) ? (overUp ? IM_COL32(255,255,255,255) : kColLabelDim)
                                          : IM_COL32(80, 84, 94, 110);
                ImU32 dnCol = (i > 0) ? (overDn ? IM_COL32(255,255,255,255) : kColLabelDim)
                                      : IM_COL32(80, 84, 94, 110);
                dl->AddTriangleFilled(ImVec2(chX-4, midY-2), ImVec2(chX+4, midY-2),
                                      ImVec2(chX, midY-7), upCol);
                dl->AddTriangleFilled(ImVec2(chX-4, midY+2), ImVec2(chX+4, midY+2),
                                      ImVec2(chX, midY+7), dnCol);

                if (ImGui::IsMouseClicked(0) && rowHov) {
                    if (overUp)       { reorderFrom = i; reorderTo = i + 1; }
                    else if (overDn)  { reorderFrom = i; reorderTo = i - 1; }
                    else if (overEye) { L->userHidden = eOn; L->visible = !L->userHidden; undoNeeded = true; }
                    else              { *m_selectedLayer = i; }
                }

                ImGui::SetCursorScreenPos(ImVec2(rs.x, rs.y));
                ImGui::Dummy(ImVec2(rowW, frameH));
                ImGui::PopID();
            }
            if (reorderFrom >= 0 && reorderTo >= 0 && reorderTo < n) {
                layerStack->moveLayer(reorderFrom, reorderTo);
                if (*m_selectedLayer == reorderFrom) *m_selectedLayer = reorderTo;
                undoNeeded = true;
            }
            ImGui::Dummy(ImVec2(0, kRowPadY));
        }
    }
    // (No trailing spacer here — Transform's own 10px header lead provides the
    // gap, keeping LAYERS→Transform on the same grid as the other sections.)

    // Section order (top→bottom): Transform → Blend+Opacity → shader
    // Parameters → … → Tiling (last child). The Blend+Opacity row is a
    // lambda so it can be emitted right after Transform without moving
    // its internals; Tiling is likewise emitted as the panel's last child.
    //
    // emitBlendOpacityRow used to publish a half-width here so the caller
    // could append an inline "+ Add Effect" button on the blend row. That
    // standalone button is gone (effects are added from the blend gallery's
    // effect preview grid), but the lambda still resets this — kept for the
    // lambda contract and harmlessly unused.
    float gMergedAddFxW = 0.0f;
    // Declared here so the blend gallery's in-popup effect cells — emitted
    // from inside emitBlendOpacityRow — can auto-expand the Effects chain
    // when an effect is added.
    static bool effectsOpen = false;
    // OPACITY now lives at the very TOP of the Parameters panel (above the
    // Transform header) — user feedback: it's the canonical look-knob and
    // wanted it persistent regardless of which sections are collapsed. BLEND
    // stays inside Transform (collapses with it). Each got its own emitter
    // so the call sites can place them independently while the internals
    // stay shared (same unifiedSlider / same gallery popup as before).
    auto emitOpacityRow = [&]() {
        // CANONICAL slider — same shared helper as every other value slider:
        // identical look, size, label/value placement, full-width track,
        // Shift-snap to 0.05. No behaviour change vs. the previous
        // emitBlendOpacityRow Row-1 block.
        float op = layer->opacity;
        if (unifiedSlider("##OpacityInline", "OPACITY",
                          &op, 0.0f, 1.0f, "%.2f")) {
            layer->opacity = op;
            undoNeeded = true;
        }
    };
    auto emitBlendRow = [&]() {
    // --- Blend --- previously rode the same row as Opacity inside Transform.
    // Opacity has been hoisted to the top of the panel; this emitter is now
    // BLEND-only. The dropdown gallery (including the in-popup "+ Add Effect"
    // grid) is unchanged — same aligned control column as RESOLUTION /
    // COLORMODE so the inspector still reads as one consistent grid.
    {
        // (BLEND, now ONE clean dropdown row — no more side-by-side
        // blend-trigger | + Add Effect split). Left of the shared label
        // gutter: "BLEND". The whole control column is a single full-width
        // trigger button showing the current blend-mode name; clicking it
        // opens ONE popup (##BlendGallery) — a grid gallery where every
        // cell is a LIVE thumbnail of the actual layer content (same
        // layer->source->textureId() V-flipped AddImageRounded idiom as
        // the track / left-rail thumbnails — NO new render pass / FBO),
        // plus a "+ Add Effect" entry at the top. Selecting a cell sets the
        // SAME layer->blendMode state as before (no behaviour change).
        ImGui::Dummy(ImVec2(0, kRowGapY));
        float blendRowCtrlW = labelGutter("BLEND", kDimText);
        const char* currentBlend = blendModeName(layer->blendMode);
        {
            // Single full-width trigger, styled like the other compact row
            // controls (same idiom as before, just no width split).
            ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 6.0f);
            ImGui::PushStyleColor(ImGuiCol_Button,        kColCtrlBgV);
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, kColCtrlBgHoverV);
            ImGui::PushStyleColor(ImGuiCol_ButtonActive,  kColCtrlBgActiveV);
            ImGui::PushStyleColor(ImGuiCol_Text,          kColValueV);
            char trig[48];
            snprintf(trig, sizeof(trig), "%s##BlendTrigger", currentBlend);
            ImVec2 trigPos = ImGui::GetCursorScreenPos();
            float trigH = ImGui::GetFrameHeight();
            if (ImGui::Button(trig, ImVec2(blendRowCtrlW, 0)))
                ImGui::OpenPopup("##BlendGallery");
            ImGui::PopStyleColor(4);
            ImGui::PopStyleVar();

            // Gallery popup — reuse the sound-capture / nav-output popup
            // styling idiom (same WindowPadding/ItemSpacing/rounding/colors,
            // no new magic numbers or palette). Anchored under the trigger,
            // sized sensibly, scrollable if it ever overflows.
            const float kGalW = 264.0f;
            ImGui::SetNextWindowPos(ImVec2(trigPos.x, trigPos.y + trigH + 4.0f),
                                    ImGuiCond_Always);
            ImGui::SetNextWindowSizeConstraints(ImVec2(kGalW, 0),
                                                ImVec2(kGalW, 360));
            ImGui::PushStyleVar(ImGuiStyleVar_WindowPadding,  ImVec2(14, 12));
            ImGui::PushStyleVar(ImGuiStyleVar_ItemSpacing,    ImVec2(kStepY * 2.0f, kStepY * 2.0f));
            ImGui::PushStyleVar(ImGuiStyleVar_WindowRounding, 8.0f);
            ImGui::PushStyleColor(ImGuiCol_PopupBg, IM_COL32(8, 9, 12, 255));
            ImGui::PushStyleColor(ImGuiCol_Border,  kColCtrlBorder);
            bool galleryOpen = ImGui::BeginPopup("##BlendGallery");
            ImGui::PopStyleColor(2);
            ImGui::PopStyleVar(3);
            if (galleryOpen) {
                ImDrawList* dl = ImGui::GetWindowDrawList();

                // Shared grid geometry — ONE source of truth so the blend
                // grid and the effect grid below are pixel-for-pixel
                // identical (cell size / rounding / spacing / columns /
                // label placement / selection+hover treatment).
                const int   kCols   = 3;
                const float kCellRd = 4.0f;           // cell rounding
                const float kLblPad = 18.0f;          // label strip height
                const float kCellW  = (kGalW - 28.0f - kStepY * 2.0f * (kCols - 1)) / kCols;
                const float kCellH  = kCellW * 0.78f;
                GLuint texId = layer->source ? layer->source->textureId() : 0;

                // Shared cell drawer — paints the live-content thumbnail
                // (SAME textureId() V-flipped AddImageRounded idiom, NO new
                // render pass), an optional decorative per-effect hint drawn
                // ON TOP (drawlist-only, cheap — purely signals which effect
                // it is, NOT an accurate render), the selection/hover frame,
                // and the centred name label. Used by BOTH grids so they are
                // visually consistent. `fallbackIdx`/`fallbackN` drive the
                // no-texture two-tone swatch; `effHint`<0 ⇒ no hint overlay.
                auto drawGridCell = [&](const ImVec2& cp, bool selected,
                                        bool hov, const char* nm,
                                        int fallbackIdx, int fallbackN,
                                        int effHint) {
                    ImVec2 tMax(cp.x + kCellW, cp.y + kCellH - kLblPad);
                    if (texId != 0) {
                        // Reuse the EXACT thumbnail idiom: V-flipped uv
                        // (0,1)-(1,0) AddImageRounded — no new render pass.
                        dl->AddRectFilled(cp, tMax, IM_COL32(14,15,19,255), kCellRd);
                        dl->AddImageRounded((ImTextureID)(intptr_t)texId,
                                            cp, tMax, ImVec2(0, 1), ImVec2(1, 0),
                                            IM_COL32(255, 255, 255, 255), kCellRd);
                    } else {
                        // Fallback: original two-tone swatch so a layer with
                        // no content texture never breaks the gallery.
                        float t = (float)fallbackIdx / (float)(fallbackN - 1);
                        ImU32 cA = IM_COL32((int)(60 + 150 * t),
                                            (int)(70 + 120 * (1 - t)),
                                            (int)(120 + 90 * t), 255);
                        ImU32 cB = IM_COL32((int)(200 - 120 * t),
                                            (int)(150 + 70 * t),
                                            (int)(90 + 130 * (1 - t)), 255);
                        dl->AddRectFilled(cp, ImVec2(cp.x + kCellW * 0.5f, tMax.y),
                                          cA, kCellRd, ImDrawFlags_RoundCornersLeft);
                        dl->AddRectFilled(ImVec2(cp.x + kCellW * 0.5f, cp.y),
                                          tMax, cB, kCellRd, ImDrawFlags_RoundCornersRight);
                    }
                    // Lightweight, decorative per-effect hint overlay (no
                    // render pass / FBO / math — just drawlist primitives).
                    if (effHint >= 0) {
                        dl->PushClipRect(cp, tMax, true);
                        switch ((EffectType)effHint) {
                            case EffectType::Invert: // negative tint wash
                                dl->AddRectFilled(cp, tMax,
                                    IM_COL32(255,255,255,90), kCellRd);
                                break;
                            case EffectType::Pixelate: { // coarse blocks
                                float bw = (tMax.x - cp.x) / 4.0f;
                                float bh = (tMax.y - cp.y) / 3.0f;
                                for (int by = 0; by < 3; by++)
                                  for (int bx = 0; bx < 4; bx++)
                                    if (((bx + by) & 1) == 0)
                                      dl->AddRectFilled(
                                        ImVec2(cp.x + bx*bw, cp.y + by*bh),
                                        ImVec2(cp.x + (bx+1)*bw, cp.y + (by+1)*bh),
                                        IM_COL32(0,0,0,55));
                                break; }
                            case EffectType::Blur: // soft translucent veil
                                dl->AddRectFilled(cp, tMax,
                                    IM_COL32(210,216,228,60), kCellRd);
                                break;
                            case EffectType::Glow: { // bright corner glow
                                ImVec2 gc(tMax.x, cp.y);
                                for (int g = 4; g >= 1; g--)
                                  dl->AddCircleFilled(gc, g * 8.0f,
                                    IM_COL32(255,244,200, 18));
                                break; }
                            case EffectType::Feedback: { // offset ghost rect
                                ImVec2 o(8.0f, 6.0f);
                                dl->AddRect(ImVec2(cp.x+o.x, cp.y+o.y),
                                            ImVec2(tMax.x+o.x, tMax.y+o.y),
                                            IM_COL32(255,255,255,70), kCellRd);
                                dl->AddRect(ImVec2(cp.x-o.x, cp.y-o.y),
                                            ImVec2(tMax.x-o.x, tMax.y-o.y),
                                            IM_COL32(255,255,255,45), kCellRd);
                                break; }
                            case EffectType::ColorAdjust: { // hue tint band
                                float h = (tMax.y - cp.y) * 0.34f;
                                dl->AddRectFilledMultiColor(cp,
                                    ImVec2(tMax.x, cp.y + h),
                                    IM_COL32(255,90,140,70), IM_COL32(120,170,255,70),
                                    IM_COL32(120,170,255,70), IM_COL32(255,90,140,70));
                                break; }
                            case EffectType::Sharpen: { // crisp edge ticks
                                ImVec2 ctr((cp.x+tMax.x)*0.5f, (cp.y+tMax.y)*0.5f);
                                float r = (tMax.y - cp.y) * 0.30f;
                                dl->AddLine(ImVec2(ctr.x-r, ctr.y), ImVec2(ctr.x+r, ctr.y),
                                            IM_COL32(255,255,255,150), 1.5f);
                                dl->AddLine(ImVec2(ctr.x, ctr.y-r), ImVec2(ctr.x, ctr.y+r),
                                            IM_COL32(255,255,255,150), 1.5f);
                                break; }
                            default: break;
                        }
                        dl->PopClipRect();
                    }
                    // Cell chrome: selection = the accent (stands out),
                    // hover/idle = the ONE palette border at hover/idle alpha.
                    dl->AddRect(cp, tMax,
                                selected ? kColAccent
                                         : hov ? kColCtrlBgActive
                                               : kColCtrlBorder,
                                kCellRd, 0, selected ? 2.0f : 1.0f);
                    ImVec2 ts = ImGui::CalcTextSize(nm);
                    float lblScale = (ts.x > kCellW) ? kCellW / ts.x : 1.0f;
                    dl->AddText(ImGui::GetFont(),
                                ImGui::GetFontSize() * std::min(1.0f, lblScale),
                                ImVec2(cp.x + (kCellW - ts.x * std::min(1.0f, lblScale)) * 0.5f,
                                       cp.y + kCellH - kLblPad + 3.0f),
                                selected ? kColValue : kColLabel, nm);
                };

                // Live-content blend gallery — each cell previews the ACTUAL
                // layer content via the SAME textureId() V-flip idiom used
                // by the track / left-rail thumbnails.
                for (int i = 0; i < (int)BlendMode::COUNT; i++) {
                    BlendMode mode = (BlendMode)i;
                    bool selected = (layer->blendMode == mode);
                    if (i % kCols != 0) ImGui::SameLine();
                    ImGui::PushID(i);  // unique ID per blend cell
                    ImVec2 cp = ImGui::GetCursorScreenPos();
                    if (ImGui::InvisibleButton("##cell",
                                               ImVec2(kCellW, kCellH))) {
                        undoNeeded = true;
                        layer->blendMode = mode;       // SAME blend state/handler
                        ImGui::CloseCurrentPopup();
                    }
                    drawGridCell(cp, selected, ImGui::IsItemHovered(),
                                 blendModeName(mode), i,
                                 (int)BlendMode::COUNT, -1);
                    ImGui::PopID();
                }

                // --- ADD EFFECT section ------------------------------------
                // Separator + ALL-CAPS header dividing it from the blend
                // grid, then a thumbnail GRID styled IDENTICALLY to the
                // blend cells above (same drawGridCell). Each cell shows the
                // live layer thumbnail + a cheap decorative per-effect hint
                // and is labelled with effectTypeName(). Clicking a cell
                // runs the EXACT same add action as the ##AddEffect menu
                // (LayerEffect{type}; effects.push_back; undo; auto-expand)
                // then closes the gallery.
                ImGui::Dummy(ImVec2(0, 4));
                {
                    ImVec2 sp = ImGui::GetCursorScreenPos();
                    dl->AddLine(sp, ImVec2(sp.x + ImGui::GetContentRegionAvail().x, sp.y),
                                kColCtrlBorder);
                }
                ImGui::Dummy(ImVec2(0, 4));
                {
                    ImVec2 hp = ImGui::GetCursorScreenPos();
                    dl->AddText(ImVec2(hp.x, hp.y),
                                kColLabel, "ADD EFFECT");
                    ImGui::Dummy(ImVec2(0, ImGui::GetTextLineHeight() + 2.0f));
                }
                for (int t = 0; t < (int)EffectType::COUNT; t++) {
                    if (t % kCols != 0) ImGui::SameLine();
                    ImGui::PushID(7000 + t);  // unique per-effect, no clash
                    ImVec2 cp = ImGui::GetCursorScreenPos();
                    if (ImGui::InvisibleButton("##fxcell",
                                               ImVec2(kCellW, kCellH))) {
                        LayerEffect fx;                 // SAME add action
                        fx.type = (EffectType)t;
                        layer->effects.push_back(fx);
                        undoNeeded = true;
                        effectsOpen = true;             // auto-expand chain
                        ImGui::CloseCurrentPopup();
                    }
                    drawGridCell(cp, false, ImGui::IsItemHovered(),
                                 effectTypeName((EffectType)t), t,
                                 (int)EffectType::COUNT, t);
                    ImGui::PopID();
                }
                ImGui::EndPopup();
            }
        }
        // The "+ Add Effect" button no longer rides this row — it lives
        // inside the gallery popup. No trailing control to append.
        gMergedAddFxW = 0.0f;
    }
    }; // end emitBlendRow

    // --- Drop Shadow ---
    // Inline header row: label + Enable checkbox on the same line. If the
    // checkbox is off, there's nothing to tweak so we skip the controls
    // entirely — saving a dropdown click for the common "just want it on"
    // case. When on, controls appear directly below.
    //
    // Relocated: now emitted at the BOTTOM of the Transform collapsible
    // (per user request), so it collapses with Transform. Internals and
    // bindings unchanged — only the call site moved. Lambda is hoisted up
    // here (above Transform) so the in-Transform call site can see it.
    auto emitDropShadowSection = [&]() {
    {
        ImGui::Dummy(ImVec2(0, 4));
        ImGui::AlignTextToFramePadding();
        ImGui::PushStyleColor(ImGuiCol_Text, kColValueV);
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
    }; // end emitDropShadowSection

    // --- Shader RESOLUTION (relocated) ---
    // Body moved verbatim out of the mid-panel "Shader (ISF) controls"
    // block; now emitted at the BOTTOM of the Transform collapsible (per
    // user request) so it collapses with Transform. The lambda re-checks
    // layer->source->isShader() so it stays gated exactly as before.
    // Internals/bindings/IDs unchanged.
    auto emitResolutionSection = [&]() {
        if (!(layer->source && layer->source->isShader())) return;
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
    }; // end emitResolutionSection

    // (Transition section moved to the BOTTOM of the Effects dropdown —
    // see the matching block inside the shader-inputs wrapper.)

    // OPACITY lives INSIDE Transform (next to BLEND) — moved back per user
    // request. Emitted from the Transform body below; no longer pinned to
    // the top of the panel.

    // --- Transform (collapsible, default closed) — secondary controls go
    // under a header so the main event (shader parameters / video) isn't
    // buried under a wall of position/scale/rotation.
    static bool transformOpen = false;
    // Not firstSection: Transform always follows LAYERS, so it gets the same
    // standard 10px header lead as Effects / Tiling — keeps the collapsed
    // section headers on a uniform vertical grid.
    if (sectionHeader("Transform", &transformOpen, /*firstSection=*/false)) {
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
            dl->AddCircleFilled(center, padR, kColCtrlBg, 36);
            dl->AddCircle(center, padR,        kColCtrlBorder, 36, 1.2f);
            dl->AddCircle(center, padR * 0.5f, kColCtrlBorder, 36, 1.0f);
            // Crosshair through center.
            dl->AddLine(ImVec2(center.x - padR, center.y),
                        ImVec2(center.x + padR, center.y),
                        kColCtrlBorder, 1.0f);
            dl->AddLine(ImVec2(center.x, center.y - padR),
                        ImVec2(center.x, center.y + padR),
                        kColCtrlBorder, 1.0f);
            // Position dot — clamped position.x/y from [-1..1] map to ring.
            float px = std::max(-1.0f, std::min(1.0f, layer->position.x));
            float py = std::max(-1.0f, std::min(1.0f, layer->position.y));
            ImVec2 dot(center.x + px * padR,
                       center.y - py * padR);  // y inverted (up = +y in NDC)
            dl->AddCircleFilled(dot, 5.0f, kColValue, 16);
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

        // Sketch layout: Reset on the LEFT, Flip H / Flip V toggles on the
        // RIGHT, one row. Reset gets the left third; the two flip pills are
        // right-aligned and sit on the shared kStepY gutter rhythm.
        bool doReset = false;
        {
            ImGui::Dummy(ImVec2(0, kRowGapY));
            float rowW   = ImGui::GetContentRegionAvail().x;
            float resetW = (rowW - kColGap) * 0.34f;
            doReset = accentBtn("Reset", resetW);
            ImGui::SameLine(0, kColGap);
            float fhW = ImGui::CalcTextSize("Flip H").x
                      + ImGui::GetStyle().FramePadding.x * 2.0f;
            float fvW = ImGui::CalcTextSize("Flip V").x
                      + ImGui::GetStyle().FramePadding.x * 2.0f;
            float pairW = fhW + fvW + kStepY;
            float startX = ImGui::GetCursorPosX() + (rowW - resetW - kColGap)
                         - pairW;
            if (startX > ImGui::GetCursorPosX()) ImGui::SetCursorPosX(startX);
            if (ImGui::Checkbox("Flip H", &layer->flipH)) undoNeeded = true;
            ImGui::SameLine(0, kStepY);
            if (ImGui::Checkbox("Flip V", &layer->flipV)) undoNeeded = true;
        }
        if (doReset) {
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

        // OPACITY + BLEND both live INSIDE Transform (so they collapse with
        // it). OPACITY rides just above the BLEND gallery trigger (which
        // itself hosts the in-popup "+ Add Effect" grid). Separated from the
        // Crop rows by the shared kSectionGap idiom to match the existing
        // intra-section rhythm.
        sectionBreak();
        emitOpacityRow();
        emitBlendRow();

        // Drop Shadow + Resolution — also live INSIDE Transform now (per
        // user request). They collapse with Transform and stop occupying
        // their own top-level real estate at the bottom of the panel.
        sectionBreak();
        emitDropShadowSection();
        emitResolutionSection();
    }

    // --- Effects machinery ---
    // (The ##AddEffect popup + openAddEffectPopup helper were removed: the
    // BLEND dropdown gallery's effect preview grid adds effects via a direct
    // layer->effects.push_back, and the standalone "+ Add Effect" buttons
    // that opened the popup are gone. No remaining caller, so the popup is
    // fully dead and dropped. effectsOpen is declared above
    // emitBlendOpacityRow so the in-gallery add path can auto-expand the
    // Effects chain.)

    // Blend Mode + Opacity used to render here as their own top-level rows
    // below the Transform header. They now live INSIDE the Transform body
    // (emitted just above, after Crop) so collapsing Transform also hides
    // them — the intentional behaviour now that they are spatial/look
    // attributes of the layer rather than a separate panel concern.
    // (void gMergedAddFxW: kept for the emitBlendOpacityRow contract; the
    // inline trailing button it used to publish for is gone.)
    (void)gMergedAddFxW;

    // --- Effect Stack --- once effects exist, the collapsible section
    // holds the per-effect rows below the merged Blend/Effects row.
    // Renamed from "Effects" → "Effect Stack" so the shader-INPUTS section
    // below (now titled "Effects") doesn't collide. ID/state unchanged.
    if (!layer->effects.empty())
    if (sectionHeader("Effect Stack", &effectsOpen)) {
    {

    // --- Layer Effects Chain ---
    {
        // (The standalone "+ Add Effect" button was removed — effects are
        // added from the BLEND dropdown gallery's effect preview grid. This
        // section now shows ONLY the populated chain so the user can
        // toggle/tweak/remove already-added effects.)

        // Render each effect
        int removeIdx = -1;
        for (int e = 0; e < (int)layer->effects.size(); e++) {
            auto& fx = layer->effects[e];
            ImGui::PushID(20000 + e);

            // Effect header row: checkbox + name + remove
            ImGui::Checkbox("##en", &fx.enabled);
            ImGui::SameLine();
            ImGui::PushStyleColor(ImGuiCol_Text, fx.enabled ? kColValueV : kColLabelV);
            ImGui::Text("%s", effectTypeName(fx.type));
            ImGui::PopStyleColor();
            ImGui::SameLine();
            {
                float rightX = ImGui::GetWindowContentRegionMax().x - 20.0f;
                if (ImGui::GetCursorPosX() < rightX) ImGui::SetCursorPosX(rightX);
            }
            ImGui::PushStyleColor(ImGuiCol_Text, kColDanger);
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
                case EffectType::Sharpen: {
                    float half = (w - ImGui::GetStyle().ItemSpacing.x) * 0.5f;
                    ImGui::SetNextItemWidth(half);
                    ImGui::SliderFloat("##shAmt", &fx.sharpenAmount, 0.0f, 3.0f, "Amt %.2f");
                    ImGui::SameLine();
                    ImGui::SetNextItemWidth(half);
                    ImGui::SliderFloat("##shRad", &fx.sharpenRadius, 0.5f, 4.0f, "Rad %.2f");
                    break;
                }
                default: break;
                }

                // --- Per-effect mic modulator (band + amount) ---
                // Only for effects whose primary param is meant to pulse with
                // the music; existing color/utility effects stay static.
                if (effectSupportsAudio(fx.type)) {
                    const char* bands[] = { "Mic Off", "Bass", "Mid", "Treble", "Beat" };
                    int sel = fx.audioSignal + 1;  // -1..3 -> 0..4
                    float half = (w - ImGui::GetStyle().ItemSpacing.x) * 0.5f;
                    bool on = fx.audioSignal >= 0;
                    if (on) ImGui::PushStyleColor(ImGuiCol_Text, kColAccentV);
                    ImGui::SetNextItemWidth(half);
                    if (ImGui::Combo("##fxAudBand", &sel, bands, IM_ARRAYSIZE(bands))) {
                        fx.audioSignal = sel - 1;
                        undoNeeded = true;
                    }
                    if (on) ImGui::PopStyleColor();
                    if (fx.audioSignal >= 0) {
                        ImGui::SameLine();
                        ImGui::SetNextItemWidth(half);
                        ImGui::SliderFloat("##fxAudAmt", &fx.audioAmount, 0.0f, 1.0f, "Mic %.2f");
                        if (ImGui::IsItemActivated()) undoNeeded = true;
                    }
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
    } // end sectionHeader("Effect Stack")

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
            // Audio-reactive ON — uses THE accent (same amber semantics as
            // the bound-slider fill / live caret), so "audio is driving this"
            // reads consistently everywhere.
            ImGui::PushStyleColor(ImGuiCol_Button,        kColAccentV);
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, kColAccentV);
            ImGui::PushStyleColor(ImGuiCol_ButtonActive,  kColAccentV);
            ImGui::PushStyleColor(ImGuiCol_Text, ImVec4(0.06f, 0.07f, 0.10f, 1.0f));
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
                // Same shared control column as BLEND / RESOLUTION /
                // COLORMODE via labelGutter() — every standalone parameter
                // dropdown lines its trigger up on the one grid line.
                labelGutter("AUDIO SRC", kDimText);
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

                ImGui::PushStyleColor(ImGuiCol_Text, kColLabelV);
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
                            // Selected tiles = the accent (consistent
                            // "this is chosen" signal across the panel).
                            draw->AddRectFilled(cMin, cMax, kColAccentDim, 2.0f);
                            draw->AddRect(cMin, cMax, kColAccent, 2.0f);
                        } else if (hovered) {
                            draw->AddRectFilled(cMin, cMax, kColCtrlBgActive, 2.0f);
                            draw->AddRect(cMin, cMax, kColCtrlBorder, 2.0f);
                        } else {
                            draw->AddRectFilled(cMin, cMax, kColCtrlBg, 2.0f);
                            draw->AddRect(cMin, cMax, kColCtrlBorder, 2.0f);
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

    // (emitDropShadowSection + emitResolutionSection lambdas were hoisted
    // ABOVE the Transform section so they can be called from inside the
    // Transform collapsible body. See their definitions above. Nothing else
    // changed about their internals.)

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
            ImGui::PushStyleColor(ImGuiCol_Button, muted ? ImVec4(0.85f, 0.30f, 0.32f, 0.25f) : kColCtrlBgV);
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, muted ? ImVec4(0.85f, 0.30f, 0.32f, 0.45f) : kColCtrlBgHoverV);
            ImGui::PushStyleColor(ImGuiCol_Text, muted ? kColDanger : kColValueV);
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

    // --- Moving Company (F-117) controls ---------------------------
    // Flying-jet mesh source: flight maneuver, chase camera, jet shading,
    // and the warp-speed space backdrop. Edits apply live.
    if (layer->source && layer->source->typeName() == "Moving Company") {
        auto* mc = static_cast<MovingCompanySource*>(layer->source.get());
        auto& P = mc->params();

        sectionBreak();
        static bool flightOpen = true;
        if (sectionHeader("Flight", &flightOpen)) {
            ImGui::Dummy(ImVec2(0, 4));
            pillSlider("Turn Speed",  &P.yawRate,    -1.0f, 1.0f, "%.2f");
            pillSlider("Bank",        &P.bankAmount,  0.0f, 1.2f, "%.2f");
            pillSlider("Pitch",       &P.pitchAmount, 0.0f, 1.0f, "%.2f");
            pillSlider("Orientation", &P.baseYawDeg, -180.0f, 180.0f, "%.0f");
        }

        sectionBreak();
        static bool camOpen = true;
        if (sectionHeader("Camera", &camOpen)) {
            ImGui::Dummy(ImVec2(0, 4));
            pillSlider("Distance",    &P.camDistance, 1.0f, 6.0f, "%.2f");
            pillSlider("Height",      &P.camHeight,  -2.0f, 3.0f, "%.2f");
            pillSlider("Orbit Speed", &P.orbitSpeed, -0.6f, 0.6f, "%.3f");
            pillSlider("FOV",         &P.fov,        15.0f, 90.0f, "%.0f");
        }

        sectionBreak();
        static bool lookOpen = true;
        if (sectionHeader("Jet Look", &lookOpen)) {
            ImGui::Dummy(ImVec2(0, 4));
            pillSlider("Brightness", &P.jetBrightness, 0.0f, 2.0f, "%.2f");
            pillSlider("Rim Glow",   &P.rimIntensity,  0.0f, 3.0f, "%.2f");
            labelGutter("BODY COLOR", kDimText);
            ImGui::ColorEdit3("##MCBody", &P.jetColor.x);
            labelGutter("RIM COLOR", kDimText);
            ImGui::ColorEdit3("##MCRim", &P.rimColor.x);
        }

        sectionBreak();
        static bool spaceOpen = true;
        if (sectionHeader("Space", &spaceOpen)) {
            ImGui::Dummy(ImVec2(0, 4));
            pillSlider("Warp Speed",   &P.warpSpeed,     0.0f, 4.0f, "%.2f");
            pillSlider("Warp Streaks", &P.warpIntensity, 0.0f, 6.0f, "%.2f");
            pillSlider("Star Density", &P.starDensity,   0.0f, 4.0f, "%.2f");
            pillSlider("Nebula",       &P.nebulaAmount,  0.0f, 2.0f, "%.2f");
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

            // Shared control column (labelGutter) — aligns with RESOLUTION /
            // BLEND / enum combos.
            labelGutter("SPAWN SHAPE", kDimText);
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

            const char* modes[] = {"Soft Sprite", "Textured", "Ring"};
            int rm = em.renderMode; if (rm < 0 || rm > 2) rm = 0;
            // Shared control column (labelGutter).
            labelGutter("RENDER MODE", kDimText);
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
                    mod.enabled ? kColValueV : kColLabelV);
                ImGui::Text("%s", particleModuleTypeName(mod.type));
                ImGui::PopStyleColor();

                // Row 2: small action buttons — safe SameLine chain.
                if (ImGui::SmallButton("up")) { toMoveIdx = i; toMoveDir = -1; }
                ImGui::SameLine();
                if (ImGui::SmallButton("dn")) { toMoveIdx = i; toMoveDir = +1; }
                ImGui::SameLine();
                ImGui::PushStyleColor(ImGuiCol_Text, kColDanger);
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

    // --- Fluid simulation controls ---
    if (layer->source && layer->source->typeName() == "Fluid") {
        auto* fsrc = static_cast<FluidSource*>(layer->source.get());

        sectionBreak();
        static bool fluidOpen = true;
        if (sectionHeader("Fluid", &fluidOpen)) {
            ImGui::Dummy(ImVec2(0, 4));

            // Bind-capable fluid param: the canonical paramSlider (sparkle
            // bind affordance) + the SAME audio/MIDI bind popup as ShaderSource
            // params. Keyed by a stable id string that
            // FluidSource::applyAudioBindings() maps back to a config member.
            auto fluidParam = [&](const char* pid, const char* label, float* v,
                                  float lo, float hi, const char* fmt) {
                auto& bindings = fsrc->audioBindings();
                auto bit = bindings.find(pid);
                bool isBound = (bit != bindings.end() &&
                                bit->second.signal != AudioSignal::None);
                ImGui::PushID(pid);
                ParamSliderResult ps = paramSlider("##fp", label, v, lo, hi,
                                                   isBound, fmt);
                if (ps.activated) undoNeeded = true;
                if (ps.openBindMenu) ImGui::OpenPopup("##fbind");
                audioBindPopup("##fbind", label, bindings, pid, lo, hi,
                               midi, ps.boltPos);
                // Bound → inline draggable min/max range (matches shaders).
                if (isBound) {
                    AudioBinding& abr = bindings[pid];
                    float liveDriven = abr.rangeMin +
                        abr.smoothedValue * (abr.rangeMax - abr.rangeMin);
                    ImGui::Indent(14.0f);
                    if (rangeSlider("##finrng", "range",
                                    &abr.rangeMin, &abr.rangeMax, lo, hi,
                                    &liveDriven))
                        undoNeeded = true;
                    ImGui::Unindent(14.0f);
                }
                ImGui::PopID();
            };

            // Audio-reactivity presets — same control as the shader Effects
            // section, operating on the fluid's bindable members (curated to the
            // visually pleasing ones; sim-stability params are left manual).
            {
                std::vector<PresetParam> pp = {
                    { "curl",               fsrc->m_curlAmount,         0.0f,  60.0f },
                    { "splatRadius",        fsrc->m_splatRadius,        0.05f, 1.5f  },
                    { "splatIntensity",     fsrc->m_splatIntensity,     0.1f,  4.0f  },
                    { "densityDissipation", fsrc->m_densityDissipation, 0.0f,  4.0f  },
                    { "autoSpeed",          fsrc->m_autoSpeed,          0.0f,  4.0f  },
                    { "autoScale",          fsrc->m_autoScale,          0.0f,  0.5f  },
                    { "bloomIntensity",     fsrc->m_bloomIntensity,     0.0f,  2.0f  },
                    { "sunraysWeight",      fsrc->m_sunraysWeight,      0.0f,  2.0f  },
                };
                if (audioPresetRow(fsrc->audioBindings(), pp, layer->id))
                    undoNeeded = true;
            }

            // Dynamics (every value param has the sparkle → audio/MIDI bindable).
            fluidParam("curl",                "Swirl (Curl)", &fsrc->m_curlAmount,          0.0f, 60.0f, "%.0f");
            fluidParam("densityDissipation",  "Dye Life",     &fsrc->m_densityDissipation,  0.0f, 4.0f,  "%.2f");
            fluidParam("velocityDissipation", "Motion Damp",  &fsrc->m_velocityDissipation, 0.0f, 4.0f,  "%.2f");
            fluidParam("pressure",            "Pressure",     &fsrc->m_pressureValue,       0.0f, 1.0f,  "%.2f");
            {
                float it = (float)fsrc->m_pressureIters;
                if (pillSlider("Quality (iters)", &it, 1.0f, 60.0f, "%.0f"))
                    fsrc->m_pressureIters = (int)it;
            }
            sectionBreak();

            // Injection
            fluidParam("splatRadius",    "Splat Size",      &fsrc->m_splatRadius,    0.05f, 1.5f, "%.2f");
            fluidParam("splatIntensity", "Splat Intensity", &fsrc->m_splatIntensity, 0.1f,  4.0f, "%.2f");
            sectionBreak();

            // ── Image inject ─────────────────────────────────────────────
            // Pick another layer (image / video / NDI / shader / webcam)
            // and the fluid additively pulls its pixels into the dye field
            // every frame; the velocity field then smears them — the
            // classic "fluid carries the picture" look. Strength is
            // audio-bindable so the picture can pulse on a beat.
            ImGui::Checkbox("Image Inject", &fsrc->m_imageEnabled);
            if (fsrc->m_imageEnabled) {
                auto& img = fsrc->imageSource();
                // Current selection label
                const char* curName = "(none)";
                if (layerStack) {
                    for (int i = 0; i < layerStack->count(); i++) {
                        auto& L = (*layerStack)[i];
                        if (L && L->id == img.sourceLayerId) {
                            curName = L->name.empty() ? "(unnamed)" : L->name.c_str();
                            break;
                        }
                    }
                }
                dimLabel("SOURCE", kRowLabel, false);
                ImGui::SetNextItemWidth(-1);
                if (ImGui::BeginCombo("##fluidImgSrc", curName)) {
                    if (ImGui::Selectable("(none)", img.sourceLayerId == 0)) {
                        img.sourceLayerId = 0;
                        img.textureId     = 0;
                        undoNeeded = true;
                    }
                    if (layerStack) {
                        for (int i = 0; i < layerStack->count(); i++) {
                            auto& L = (*layerStack)[i];
                            if (!L || !L->source) continue;
                            if (L->id == layer->id) continue;   // skip self
                            const char* nm = L->name.empty()
                                ? "(unnamed)" : L->name.c_str();
                            bool sel = (img.sourceLayerId == L->id);
                            ImGui::PushID((int)L->id);
                            if (ImGui::Selectable(nm, sel)) {
                                img.sourceLayerId = L->id;
                                undoNeeded = true;
                            }
                            ImGui::PopID();
                        }
                    }
                    ImGui::EndCombo();
                }
                fluidParam("imageIntensity", "Inject Strength",
                           &fsrc->m_imageIntensity, 0.0f, 1.0f, "%.2f");

                // Inline create-and-bind shortcut — when no usable layer is
                // available (or you just want a fresh one), these buttons
                // trigger the same Add Image/Video/Shader flow Application
                // already runs from the Layers panel, then auto-bind the
                // new layer's id back to this Fluid's image source via
                // LayerPanel::postCreateBindFluidImage.
                if (m_layerPanel) {
                    ImGui::Dummy(ImVec2(0, 2));
                    dimLabel("ADD AS SOURCE", kRowLabel, false);
                    float avail = ImGui::GetContentRegionAvail().x;
                    float bw = (avail - 12.0f) / 3.0f;
                    auto addBtn = [&](const char* label, bool* flag) {
                        if (ImGui::Button(label, ImVec2(bw, 0))) {
                            *flag = true;
                            m_layerPanel->postCreateBindFluidImage = fsrc;
                            undoNeeded = true;
                        }
                    };
                    addBtn("+ Image",  &m_layerPanel->wantsAddImage);
                    ImGui::SameLine(0, 6);
                    addBtn("+ Video",  &m_layerPanel->wantsAddVideo);
                    ImGui::SameLine(0, 6);
                    addBtn("+ Shader", &m_layerPanel->wantsAddShader);
                }
            }
            sectionBreak();

            // Auto-movement presets — choose the cursor's motion pattern, then
            // tweak density / tempo / spread (all bindable).
            ImGui::Checkbox("Auto Movement", &fsrc->m_autoMovement);
            if (fsrc->m_autoMovement) {
                static const char* patternNames[] = {
                    "Wander", "Orbit", "Figure 8", "Pulse", "Rain", "Spiral" };
                dimLabel("MOVEMENT", kRowLabel, false);
                ImGui::SetNextItemWidth(-1);
                ImGui::Combo("##fluidpattern", &fsrc->m_autoPattern,
                             patternNames, IM_ARRAYSIZE(patternNames));
                fluidParam("autoRate",  "Density", &fsrc->m_autoRate,  0.0f, 60.0f, "%.0f/s");
                fluidParam("autoSpeed", "Speed",   &fsrc->m_autoSpeed, 0.0f, 4.0f,  "%.2f");
                fluidParam("autoScale", "Spread",  &fsrc->m_autoScale, 0.0f, 0.5f,  "%.2f");
            }
            ImGui::Checkbox("Shading", &fsrc->m_shading);
            sectionBreak();

            // Native-only quality (RGBA16F HDR dye — not feasible on the
            // browser's UNSIGNED_BYTE fallback path).
            ImGui::Checkbox("HDR Dye (16F)", &fsrc->m_hdrDye);
            if (ImGui::IsItemHovered())
                ImGui::SetTooltip("Float dye buffer — smoother gradients,\n"
                                  "no 8-bit banding. Desktop GL only.");
            sectionBreak();

            // Bloom pyramid (Pavel-exact multi-pass glow).
            ImGui::Checkbox("Bloom", &fsrc->m_bloom);
            if (fsrc->m_bloom) {
                fluidParam("bloomIntensity", "Bloom Strength",  &fsrc->m_bloomIntensity, 0.0f, 2.0f, "%.2f");
                fluidParam("bloomThreshold", "Bloom Threshold", &fsrc->m_bloomThreshold, 0.0f, 1.5f, "%.2f");
                pillSlider("Bloom Knee",      &fsrc->m_bloomSoftKnee,  0.0f, 1.0f, "%.2f");
            }
            // Sunrays (radial light shafts).
            ImGui::Checkbox("Sunrays", &fsrc->m_sunrays);
            if (fsrc->m_sunrays)
                fluidParam("sunraysWeight", "Sunray Weight", &fsrc->m_sunraysWeight, 0.0f, 2.0f, "%.2f");
        }
    }

    // --- 3D Fluid (SPH) controls ---
    if (layer->source && layer->source->typeName() == "Fluid3D") {
        auto* f3 = static_cast<FluidSource3D*>(layer->source.get());

        sectionBreak();
        static bool fluid3dOpen = true;
        if (sectionHeader("3D Fluid", &fluid3dOpen)) {
            ImGui::Dummy(ImVec2(0, 4));

            // Bind-capable param: paramSlider + audio/MIDI bind popup + inline
            // range, keyed by an id FluidSource3D::applyAudioBindings() maps back.
            auto f3Param = [&](const char* pid, const char* label, float* v,
                               float lo, float hi, const char* fmt) {
                auto& bindings = f3->audioBindings();
                auto bit = bindings.find(pid);
                bool isBound = (bit != bindings.end() &&
                                bit->second.signal != AudioSignal::None);
                ImGui::PushID(pid);
                ParamSliderResult ps = paramSlider("##f3", label, v, lo, hi,
                                                   isBound, fmt);
                if (ps.activated) undoNeeded = true;
                if (ps.openBindMenu) ImGui::OpenPopup("##f3bind");
                audioBindPopup("##f3bind", label, bindings, pid, lo, hi,
                               midi, ps.boltPos);
                if (isBound) {
                    AudioBinding& abr = bindings[pid];
                    float liveDriven = abr.rangeMin +
                        abr.smoothedValue * (abr.rangeMax - abr.rangeMin);
                    ImGui::Indent(14.0f);
                    if (rangeSlider("##f3rng", "range",
                                    &abr.rangeMin, &abr.rangeMax, lo, hi,
                                    &liveDriven))
                        undoNeeded = true;
                    ImGui::Unindent(14.0f);
                }
                ImGui::PopID();
            };

            {
                std::vector<PresetParam> pp = {
                    { "brightness",  f3->m_brightness,  0.0f,  6.0f },
                    { "rotateSpeed", f3->m_rotateSpeed, 0.0f,  2.0f },
                    { "tilt",        f3->m_tilt,       -1.57f, 1.57f },
                };
                if (audioPresetRow(f3->audioBindings(), pp, layer->id))
                    undoNeeded = true;
            }

            f3Param("brightness",  "Brightness", &f3->m_brightness,  0.0f,  6.0f,  "%.2f");
            ImGui::Checkbox("Auto-rotate", &f3->m_autoRotate);
            if (f3->m_autoRotate)
                f3Param("rotateSpeed", "Spin Speed", &f3->m_rotateSpeed, 0.0f, 2.0f, "%.2f");
            f3Param("tilt", "Tilt", &f3->m_tilt, -1.57f, 1.57f, "%.2f");
            f3Param("zoom", "Zoom", &f3->m_zoom, 0.5f, 4.0f, "%.2f");
            f3Param("audioIntensity", "Audio Motion", &f3->m_audioIntensity, 0.0f, 1.0f, "%.2f");

            ImGui::ColorEdit3("Liquid",  f3->m_deepColor, ImGuiColorEditFlags_NoInputs);
            ImGui::ColorEdit3("Glow",    f3->m_glowColor, ImGuiColorEditFlags_NoInputs);
            ImGui::ColorEdit3("Rim",     f3->m_shallowColor, ImGuiColorEditFlags_NoInputs);
            f3Param("rim",        "Rim Amount",  &f3->m_rim,        0.0f, 1.0f, "%.2f");
            f3Param("saturation", "Saturation",  &f3->m_saturation, 0.0f, 2.0f, "%.2f");
            ImGui::ColorEdit3("BG Top",    f3->m_bgTop,    ImGuiColorEditFlags_NoInputs);
            ImGui::ColorEdit3("BG Bottom", f3->m_bgBottom, ImGuiColorEditFlags_NoInputs);
            f3Param("bgAlpha",    "BG Opacity",  &f3->m_bgAlpha,    0.0f, 1.0f, "%.2f");
            f3Param("sphereScale","Blob Size",   &f3->m_sphereScale,0.05f,1.5f, "%.2f");

            sectionBreak();
            ImGui::TextDisabled("Lighting");
            ImGui::SliderFloat3("Light Dir",   f3->m_lightDir, -1.0f, 1.0f);
            f3Param("lightIntensity", "Light Intensity", &f3->m_lightIntensity, 0.0f, 3.0f, "%.2f");
            f3Param("ambient",        "Ambient",         &f3->m_ambient,        0.0f, 1.0f, "%.2f");
            f3Param("specular",       "Specular",        &f3->m_specular,       0.0f, 3.0f, "%.2f");

            sectionBreak();
            // Quality / performance.
            {
                float rs = f3->m_renderScale * 100.0f;
                if (pillSlider("Render Sharpness", &rs, 25.0f, 100.0f, "%.0f%%"))
                    f3->m_renderScale = rs / 100.0f;
                if (ImGui::IsItemHovered())
                    ImGui::SetTooltip("Render resolution as %% of the output zone.\n"
                                      "100%% = crispest; lower = faster.");
            }
            {
                float sr = (float)f3->m_simRes;
                if (pillSlider("Sim Detail (grid)", &sr, 32.0f, 80.0f, "%.0f"))
                    f3->m_simRes = (int)sr;
                if (ImGui::IsItemHovered())
                    ImGui::SetTooltip("3D grid edge — higher = finer fluid detail,\n"
                                      "much higher GPU cost. Reseeds on change.");
            }
            {
                float ss = (float)f3->m_substeps;
                if (pillSlider("Sim Substeps", &ss, 1.0f, 4.0f, "%.0f"))
                    f3->m_substeps = (int)ss;
            }

            sectionBreak();
            // ── Image — color the fluid surface with another layer ──────
            ImGui::Checkbox("Image", &f3->m_imageEnabled);
            if (f3->m_imageEnabled) {
                auto& img = f3->imageSource();
                const char* curName = "(none)";
                if (layerStack) {
                    for (int i = 0; i < layerStack->count(); i++) {
                        auto& L = (*layerStack)[i];
                        if (L && L->id == img.sourceLayerId) {
                            curName = L->name.empty() ? "(unnamed)" : L->name.c_str();
                            break;
                        }
                    }
                }
                dimLabel("SOURCE", kRowLabel, false);
                ImGui::SetNextItemWidth(-1);
                if (ImGui::BeginCombo("##f3ImgSrc", curName)) {
                    if (ImGui::Selectable("(none)", img.sourceLayerId == 0)) {
                        img.sourceLayerId = 0; img.textureId = 0; undoNeeded = true;
                    }
                    if (layerStack) {
                        for (int i = 0; i < layerStack->count(); i++) {
                            auto& L = (*layerStack)[i];
                            if (!L || !L->source) continue;
                            if (L->id == layer->id) continue;   // skip self
                            const char* nm = L->name.empty() ? "(unnamed)" : L->name.c_str();
                            bool sel = (img.sourceLayerId == L->id);
                            ImGui::PushID((int)L->id);
                            if (ImGui::Selectable(nm, sel)) { img.sourceLayerId = L->id; undoNeeded = true; }
                            ImGui::PopID();
                        }
                    }
                    ImGui::EndCombo();
                }
                f3Param("imageMix", "Image Amount", &f3->m_imageMix, 0.0f, 1.0f, "%.2f");
                if (m_layerPanel) {
                    ImGui::Dummy(ImVec2(0, 2));
                    dimLabel("ADD AS SOURCE", kRowLabel, false);
                    float avail = ImGui::GetContentRegionAvail().x;
                    float bw = (avail - 12.0f) / 3.0f;
                    auto addBtn = [&](const char* label, bool* flag) {
                        if (ImGui::Button(label, ImVec2(bw, 0))) {
                            *flag = true;
                            m_layerPanel->postCreateBindFluid3DImage = f3;
                            undoNeeded = true;
                        }
                    };
                    addBtn("+ Image",  &m_layerPanel->wantsAddImage);
                    ImGui::SameLine(0, 6);
                    addBtn("+ Video",  &m_layerPanel->wantsAddVideo);
                    ImGui::SameLine(0, 6);
                    addBtn("+ Shader", &m_layerPanel->wantsAddShader);
                }
            }
        }
    }

    // --- Hologram Model controls (uploaded 3D model + glitch) ---
    if (layer->source && layer->source->typeName() == "Hologram Model") {
        auto* hm = static_cast<HologramModelSource*>(layer->source.get());
        auto& P = hm->params();

        sectionBreak();
        static bool holoOpen = true;
        if (sectionHeader("Hologram Model", &holoOpen)) {
            ImGui::Dummy(ImVec2(0, 4));
            // Model upload — routes through Application's native picker via a
            // request flag (PropertyPanel can't open the dialog itself).
            ImGui::PushStyleColor(ImGuiCol_Button,        kColCtrlBgV);
            ImGui::PushStyleColor(ImGuiCol_ButtonHovered, kColCtrlBgHoverV);
            ImGui::PushStyleColor(ImGuiCol_ButtonActive,  kColCtrlBgActiveV);
            ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 6.0f);
            if (ImGui::Button(hm->hasModel() ? "Change Model..." : "Upload Model...",
                              ImVec2(-1, 0)))
                hm->m_requestModelDialog = true;
            ImGui::PopStyleVar();
            ImGui::PopStyleColor(3);
            if (!hm->hasModel()) {
                ImGui::PushStyleColor(ImGuiCol_Text, kColLabelV);
                ImGui::TextWrapped("No model loaded — upload an .obj / .glb.");
                ImGui::PopStyleColor();
            }
            sectionBreak();
            // Model presentation
            pillSlider("Rotation",     &P.rotateSpeed, 0.0f, 1.5f, "%.2f");
            pillSlider("Model Size",   &P.modelScale,  0.2f, 3.0f, "%.2f");
            pillSlider("Wire Glow",    &P.wireBright,  0.0f, 2.0f, "%.2f");
            pillSlider("Surface Fill", &P.surfaceFill, 0.0f, 1.0f, "%.2f");
            sectionBreak();
            // Glitch (mirrors the hologram_glitch shader)
            pillSlider("Scan Speed",     &P.scanSpeed,    0.0f, 2.0f,  "%.2f");
            pillSlider("Interference",   &P.interference, 0.0f, 1.0f,  "%.2f");
            pillSlider("Chromatic Split",&P.chromaShift,  0.0f, 0.04f, "%.3f");
            pillSlider("Beam Haze",      &P.beamHaze,     0.0f, 1.5f,  "%.2f");
            pillSlider("Audio React",    &P.audioReact,   0.0f, 2.0f,  "%.2f");
        }
    }

    // --- Shader (ISF) controls ---
    if (layer->source && layer->source->isShader()) {
        auto* shaderSrc = static_cast<ShaderSource*>(layer->source.get());
        auto& inputs = shaderSrc->inputs();

        // Shader resolution override — RELOCATED to the bottom of the
        // panel (just above Tiling). The body now lives in
        // emitResolutionSection(), a function-scope lambda called near
        // ImGui::End(). It re-checks layer->source->isShader() itself so
        // the relocated call stays gated exactly as before. Internals,
        // bindings and unique ImGui IDs (##ShaderRes/##cw/##ch) unchanged.

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
                        kColCtrlBg, 4.0f);
                    ImGui::SetCursorScreenPos(ImVec2(cursor.x + 8.0f,
                                                     cursor.y + 4.0f));
                    ImGui::PushStyleColor(ImGuiCol_Text, kColHeaderV);
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

        // Per-layer "voice-control edit mode" flag — lives on the Layer so
        // the Layer-panel right-click menu ("Edit voice control") can toggle
        // it too. When on, every float param shows an inline voice/audio
        // source combo so users can wire bindings without hunting for the
        // right-click popover.
        bool paramEdit = layer->voiceControlEdit;

        if (!inputs.empty()) {
            // No leading sectionBreak: the "Effects" header below relies on
            // its own standard 10px lead so it sits on the same vertical grid
            // as Transform / Tiling (uniform collapsed-header spacing).

            // Voice-control edit mode is entered from the Layer panel's
            // right-click menu ("Edit voice control"); "Delete shader" lives
            // there too (its "Delete" item). Only the in-mode "DONE" exit
            // button shows here, so the Parameters section stays uncluttered.
            if (paramEdit) {
                float availW = ImGui::GetContentRegionAvail().x;
                ImGui::SetCursorPosX(ImGui::GetCursorPosX() + availW - 44.0f);
                if (ImGui::SmallButton("DONE##voiceEditDone")) {
                    layer->voiceControlEdit = false;
                    paramEdit = false;
                }
            }

            if (paramEdit) {
                ImGui::PushStyleColor(ImGuiCol_Text, kColAccentV);
                ImGui::TextWrapped("Voice-control edit mode — pick a source per parameter.");
                ImGui::PopStyleColor();
                ImGui::Dummy(ImVec2(0, 4));
            }
            // (params follow)

            // Per-shader parameter rows are wrapped in a NEW collapsible
            // section titled "Effects" — every input unique to this shader
            // (CHARSIZE, COLORMODE, NODECOUNT, energyA/B/C, palette, etc.)
            // lives inside this dropdown. Defaults OPEN since this is the
            // primary place users edit shader params. Distinct from the
            // chain section above (now "Effect Stack"); its own static
            // collapse state so the two never share toggles.
            static bool shaderEffectsOpen = true;
            if (sectionHeader("Effects", &shaderEffectsOpen)) {

            // == Audio Reactivity presets — shared row (see audioPresetRow) ==
            // Builds the bindable-param list from this shader's float inputs
            // (minus audio-plumbing) and renders the [Subtle][Medium][Intense]
            // [re-roll] row. Same control the Fluid section uses.
            {
                std::vector<PresetParam> pp;
                for (const auto& in : inputs) {
                    if (in.type != "float") continue;
                    if (in.name.find("audio") != std::string::npos ||
                        in.name.find("Audio") != std::string::npos) continue;
                    pp.push_back({ in.name, std::get<float>(in.value),
                                   in.minVal, in.maxVal });
                }
                if (audioPresetRow(shaderSrc->audioBindings(), pp, layer->id))
                    undoNeeded = true;
            }

            // EASING_TYPE is relocated to the BOTTOM of the parameter rows
            // (still inside the Parameters area, before Transition/Drop
            // Shadow/Resolution/Tiling). We build an ordered index list that
            // pushes the EASING_TYPE input's index last, then iterate that
            // list through the unchanged loop body — so the param renders
            // exactly once, via its normal enum-combo render path, just in a
            // different slot. No binding/value/persistence change.
            std::vector<int> paramOrder;
            paramOrder.reserve(inputs.size());
            int easingIdx = -1;
            // Pass 1: image-type inputs first — they're the "Add your own
            // texture" selector and should always sit at the top so users
            // see it before scrolling through sliders/dropdowns.
            for (int i = 0; i < (int)inputs.size(); i++) {
                if (inputs[i].type == "image") paramOrder.push_back(i);
            }
            // Pass 2: everything else (except EASING_TYPE which goes last).
            for (int i = 0; i < (int)inputs.size(); i++) {
                if (inputs[i].type == "image") continue;
                if (inputs[i].name == "EASING_TYPE") { easingIdx = i; continue; }
                paramOrder.push_back(i);
            }
            if (easingIdx >= 0) paramOrder.push_back(easingIdx);

            for (int oi = 0; oi < (int)paramOrder.size(); oi++) {
                int i = paramOrder[oi];
                auto& input = inputs[i];
                if (consumedInputs.count(input.name)) continue;
                // Baked-in audio reactivity removed — hide the now-inert
                // `audioReact` slider (shaders no longer receive live audio
                // uniforms; use the "Audio Reactivity On" button instead).
                if (input.name == "audioReact") continue;
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

                    // Shared, nav-clearing modulation popover (see audioBindPopup).
                    audioBindPopup("##audiobind", lblUp.c_str(), bindings,
                                   input.name, input.minVal, input.maxVal,
                                   midi, ps.boltPos);

                    // When bound, expose the audio range INLINE — a dual-handle
                    // min/max slider right under the param so the reactive
                    // range can be seen and dragged here without opening the
                    // popup. The slider above shows the live driven value.
                    if (isBound) {
                        AudioBinding& abr = bindings[input.name];
                        float liveDriven = abr.rangeMin +
                            abr.smoothedValue * (abr.rangeMax - abr.rangeMin);
                        ImGui::Indent(14.0f);
                        if (rangeSlider("##inrng", "range",
                                        &abr.rangeMin, &abr.rangeMax,
                                        input.minVal, input.maxVal, &liveDriven))
                            undoNeeded = true;
                        ImGui::Unindent(14.0f);
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
                } else if (input.type == "event") {
                    // Hit-button style: bold magenta-red accent pill, full
                    // width, ~42 px tall. Two flavors driven by ISF JSON:
                    //   - MOMENTARY:true → uniform mirrors the held state
                    //     (release returns to normal). Used for GLITCH HIT.
                    //   - default tap   → on click, randomize a TARGET float
                    //     param. Used for NEW GLITCH to cycle through seeds.
                    std::string lblUp = upperLabel(input.name);
                    ImGui::Dummy(ImVec2(0, kRowGapY));
                    ImVec4 accent = input.momentary
                        ? ImVec4(0.96f, 0.42f, 0.18f, 1.0f)
                        : ImVec4(0.96f, 0.18f, 0.32f, 1.0f);
                    ImVec4 hover  = ImVec4(accent.x + 0.06f, accent.y + 0.10f,
                                           accent.z + 0.10f, 1.0f);
                    ImVec4 active = ImVec4(accent.x + 0.10f, accent.y + 0.18f,
                                           accent.z + 0.18f, 1.0f);
                    ImGui::PushStyleColor(ImGuiCol_Button,        accent);
                    ImGui::PushStyleColor(ImGuiCol_ButtonHovered, hover);
                    ImGui::PushStyleColor(ImGuiCol_ButtonActive,  active);
                    ImGui::PushStyleColor(ImGuiCol_Text,
                                          ImVec4(1.0f, 1.0f, 1.0f, 1.0f));
                    ImGui::PushStyleVar(ImGuiStyleVar_FrameRounding, 10.0f);
                    ImGui::PushStyleVar(ImGuiStyleVar_FramePadding,
                                        ImVec2(10.0f, 12.0f));
                    bool clicked = ImGui::Button(lblUp.c_str(), ImVec2(-1, 0));
                    bool held    = ImGui::IsItemActive();
                    ImGui::PopStyleVar(2);
                    ImGui::PopStyleColor(4);

                    if (input.momentary) {
                        // Press-and-hold: keep uniform synced with held state.
                        bool prev = std::get<bool>(input.value);
                        if (prev != held) input.value = held;
                    } else if (clicked) {
                        // Tap/trigger: random target value if a TARGET param
                        // is named in the ISF JSON.
                        if (!input.eventTarget.empty()) {
                            for (auto& sib : inputs) {
                                if (sib.name == input.eventTarget
                                    && sib.type == "float") {
                                    float lo = sib.minVal;
                                    float hi = sib.maxVal;
                                    float r = (float)(std::rand() % 100000) / 99999.0f;
                                    sib.value = lo + r * (hi - lo);
                                    undoNeeded = true;
                                    break;
                                }
                            }
                        }
                        input.value = true; // pulse for one frame
                    } else {
                        bool prev = std::get<bool>(input.value);
                        if (prev) input.value = false;
                    }
                    ImGui::Dummy(ImVec2(0, kRowPadY));
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
                        // Match the panel bg so the combo doesn't read as a
                        // visible "container box", and force full remaining
                        // width so the preview text never has to scroll.
                        ImGui::PushStyleColor(ImGuiCol_FrameBg,        IM_COL32(0, 0, 0, 255));
                        ImGui::PushStyleColor(ImGuiCol_FrameBgHovered, IM_COL32(255, 255, 255, 14));
                        ImGui::PushStyleColor(ImGuiCol_FrameBgActive,  IM_COL32(255, 255, 255, 22));
                        ImGui::PushStyleColor(ImGuiCol_Button,         IM_COL32(0, 0, 0, 255));
                        ImGui::PushStyleColor(ImGuiCol_Border,         IM_COL32(255, 255, 255, 18));
                        ImGui::PushStyleVar(ImGuiStyleVar_FrameBorderSize, 1.0f);
                        ImGui::SetNextItemWidth(-1);
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
                        ImGui::PopStyleVar();      // FrameBorderSize
                        ImGui::PopStyleColor(5);   // FrameBg + Hovered + Active + Button + Border
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
                        ImGui::PushStyleColor(ImGuiCol_Text, kColValueV);
                        ImGui::TextWrapped("%s", val.empty() ? "..." : val.c_str());
                        ImGui::PopStyleColor();
                    } else {
                        // Manual mode: tall single-line text input + MIC button.
                        // Input takes full row minus mic + spacing.
                        char textBuf[256] = {};
                        // buf_size MUST match the real capacity of textBuf,
                        // never a shader-declared maxLen (could exceed 256 or
                        // be shorter than the existing text, tripping
                        // InputTextEx's "buffer properly zero-terminated"
                        // assert). Clamp the seed copy to maxLen *and* the
                        // buffer so the field stays correctly terminated.
                        size_t seedLen = text.size();
                        if (seedLen > (size_t)maxLen) seedLen = (size_t)maxLen;
                        if (seedLen > sizeof(textBuf) - 1) seedLen = sizeof(textBuf) - 1;
                        text.copy(textBuf, seedLen);
                        textBuf[seedLen] = '\0';

                        float micW = (speech && speech->available) ? 44.0f : 0.0f;
                        float spacing = (micW > 0.0f) ? ImGui::GetStyle().ItemSpacing.x : 0.0f;
                        // Larger input height — defaults to ~22 px, bump to 32 px
                        // by pushing FramePadding before the InputText.
                        ImGui::PushStyleVar(ImGuiStyleVar_FramePadding, ImVec2(8.0f, 8.0f));
                        ImGui::SetNextItemWidth(-(spacing + micW + 1.0f));
                        if (ImGui::InputText("##val", textBuf, sizeof(textBuf),
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
                                ImGui::PushStyleColor(ImGuiCol_Text, kColDanger);
                                if (ImGui::Button("STOP", micSize)) {
                                    speech->listening = false;
                                    speech->targetSource = nullptr;
                                    speech->targetParam.clear();
                                }
                                ImGui::PopStyleColor(4);
                            } else {
                                ImGui::PushStyleColor(ImGuiCol_Button,        kColCtrlBgV);
                                ImGui::PushStyleColor(ImGuiCol_ButtonHovered, kColCtrlBgHoverV);
                                ImGui::PushStyleColor(ImGuiCol_ButtonActive,  kColCtrlBgActiveV);
                                ImGui::PushStyleColor(ImGuiCol_Text,          kColValueV);
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

                    // Same shared control column as RESOLUTION / enum combos
                    // so TEXTURE lines up with every other dropdown.
                    labelGutter(displayLabel, kDimText);
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

            // (The standalone "+ Add Effect" trailer was removed — adding
            // effects now lives entirely in the BLEND dropdown gallery's
            // effect preview grid. Parameters ends here and flows straight
            // into the next section.)
            ImGui::Dummy(ImVec2(0, 4));

            // --- Transition (LAST block INSIDE the Effects dropdown) ---
            // Transition type + duration + optional shader-transition block.
            // Sits at the bottom of Effects so it surfaces as the natural
            // "next step" after editing parameters AND collapses along with
            // Effects when the section is closed.
            {
                static const char* transLabels[(int)TransitionType::COUNT] = {};
                for (int i = 0; i < (int)TransitionType::COUNT; i++)
                    transLabels[i] = transitionTypeName((TransitionType)i);
                int curT = (int)layer->transitionType;
                ImGui::Dummy(ImVec2(0, 4));
                // Shared control column (labelGutter).
                labelGutter("TRANSITION", kDimText);
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
            } // end sectionHeader("Effects") — wraps params + Transition
        }

#ifdef HAS_WHISPER
        if (speech && speech->available && speech->whisper) {
            auto& devices = speech->whisper->captureDevices();
            if (!devices.empty()) {
                int sel = speech->whisper->selectedDevice();
                std::string preview = (sel < 0) ? "Default" :
                    (sel < (int)devices.size() ? devices[sel].name : "Unknown");
                // Shared control column (labelGutter).
                labelGutter("MIC", kDimText);
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

        // (Shader description removed from the Parameters panel per user
        // request — the ISF JSON `DESCRIPTION` field is still parsed/stored
        // on shaderSrc and used elsewhere; we just no longer render it as
        // a wrapped paragraph below the param rows.)

    }

    // Drop Shadow + shader RESOLUTION used to render here at the BOTTOM of
    // the panel; they now live INSIDE the Transform collapsible body (per
    // user request) so they collapse with Transform. No top-level emission
    // remains.

    // Tiling — emitted as the panel's LAST child so the bulky Mosaic /
    // Feather controls sit at the very bottom and never push Transform,
    // Blend, or the shader Parameters below the fold.
    emitTilingSection();

    ImGui::EndChild();   // ##propContent
    ImGui::End();
}
