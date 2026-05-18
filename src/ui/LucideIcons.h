#pragma once
//
// Lucide-style procedural icons for ImGui.
//
// Mirrors the visual language of https://lucide.dev/ — stroke-only outlines,
// 2px strokes, round caps via ImGui::PathStroke, simple geometric primitives.
// Each function draws a single icon centered at (cx, cy) inside a `size`-pixel
// bounding box, in the given color.
//
// Conventions:
//   - "size" is the visual diameter / box edge for the icon (typical: 16–22).
//   - "stroke" is line thickness (typical: 1.6–2.0).
//   - All filled icons use the same `col` for fill; stroke-only icons use it
//     for stroke. Some (mic, circle-dot) mix fill and stroke for parity with
//     Lucide.
//
// Usage:
//   lucide::play(dl, cx, cy, 16.f, IM_COL32_WHITE);
//   lucide::mic (dl, cx, cy, 18.f, IM_COL32(232,238,250,240));
//
// All paths are local to (cx, cy); no ImGui state is mutated.

#include <imgui.h>
#include <cmath>

namespace lucide {

// Right-pointing triangle. Lucide's `play` is filled with rounded corners;
// AddTriangleFilled is the closest in ImGui's primitive set.
inline void play(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                 float /*stroke*/ = 2.0f) {
    float r = size * 0.40f;
    dl->AddTriangleFilled(
        ImVec2(cx - r * 0.55f, cy - r),
        ImVec2(cx - r * 0.55f, cy + r),
        ImVec2(cx + r * 0.95f, cy), col);
}

// Two vertical bars.
inline void pause(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                  float /*stroke*/ = 2.0f) {
    float r = size * 0.36f;
    float w = r * 0.45f;
    dl->AddRectFilled(ImVec2(cx - r * 0.55f, cy - r),
                      ImVec2(cx - r * 0.55f + w, cy + r), col, 1.0f);
    dl->AddRectFilled(ImVec2(cx + r * 0.55f - w, cy - r),
                      ImVec2(cx + r * 0.55f, cy + r), col, 1.0f);
}

// Rounded square (Lucide `square`). Used for "stop".
inline void square(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                   float stroke = 2.0f) {
    float r = size * 0.34f;
    dl->AddRect(ImVec2(cx - r, cy - r), ImVec2(cx + r, cy + r),
                col, 2.5f, 0, stroke);
}

// Filled square (used as "stop" when filled style preferred).
inline void squareFilled(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                         float /*stroke*/ = 2.0f) {
    float r = size * 0.34f;
    dl->AddRectFilled(ImVec2(cx - r, cy - r), ImVec2(cx + r, cy + r), col, 2.0f);
}

// Lucide `repeat-2` — two parallel arrows curving back on each other,
// forming a stylized loop. Implemented as two horizontal lines with curved
// caps + arrowheads pointing in opposite directions.
inline void repeat(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                   float stroke = 1.6f) {
    float r = size * 0.42f;
    float arc = r * 0.45f;
    // Top line: ─→ (right end has arrow pointing right)
    dl->AddLine(ImVec2(cx - r * 0.6f, cy - r * 0.45f),
                ImVec2(cx + r * 0.55f, cy - r * 0.45f), col, stroke);
    // Right curve down
    dl->PathArcTo(ImVec2(cx + r * 0.55f, cy - r * 0.45f + arc), arc,
                  -1.57f, 0.0f, 10);
    dl->PathStroke(col, 0, stroke);
    // Bottom line: ←─ (left end has arrow pointing left)
    dl->AddLine(ImVec2(cx + r * 0.55f + arc, cy + r * 0.45f),
                ImVec2(cx - r * 0.55f, cy + r * 0.45f), col, stroke);
    // Left curve up (mirror)
    dl->PathArcTo(ImVec2(cx - r * 0.55f, cy + r * 0.45f - arc), arc,
                  1.57f, 3.14f, 10);
    dl->PathStroke(col, 0, stroke);
    // Right arrow head (top line pointing right)
    dl->AddTriangleFilled(
        ImVec2(cx + r * 0.55f - 0.3f, cy - r * 0.45f - r * 0.28f),
        ImVec2(cx + r * 0.55f + r * 0.30f, cy - r * 0.45f),
        ImVec2(cx + r * 0.55f - 0.3f, cy - r * 0.45f + r * 0.28f), col);
    // Left arrow head (bottom line pointing left)
    dl->AddTriangleFilled(
        ImVec2(cx - r * 0.55f + 0.3f, cy + r * 0.45f - r * 0.28f),
        ImVec2(cx - r * 0.55f - r * 0.30f, cy + r * 0.45f),
        ImVec2(cx - r * 0.55f + 0.3f, cy + r * 0.45f + r * 0.28f), col);
}

// Lucide `infinity` (∞) — two interlocked loops that meet at the icon
// centre. Reads as "endless loop" much more directly than the Spotify-
// style two-arrows-in-a-box `repeat` glyph.
inline void infinity(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                     float stroke = 1.6f) {
    float r = size * 0.22f;
    // Centres slightly inside r * 2 so the two strokes overlap at the centre
    // and form the figure-eight X-cross instead of two unrelated circles.
    float dx = r * 0.92f;
    dl->AddCircle(ImVec2(cx - dx, cy), r, col, 28, stroke);
    dl->AddCircle(ImVec2(cx + dx, cy), r, col, 28, stroke);
}

// Lucide `mic` — pill-shaped capsule, U-shaped cradle, stem, horizontal base.
inline void mic(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                float stroke = 1.6f) {
    // Canonical Lucide `mic` mapped from its 24-unit viewBox:
    //   rect 9,2 6x13 rx=3   → capsule (-3,-10)..(3,3) rx=3
    //   M19 10v2 a7 7 0 0 1 -14 0 v-2 → U-cradle (radius 7) at y=0..7
    //   M12 19v3            → stand (0,7)..(0,10)
    float u = size / 24.0f;
    // Capsule (stroked, not filled — matches Lucide's outline style)
    dl->AddRect(ImVec2(cx - 3.0f*u, cy - 10.0f*u),
                ImVec2(cx + 3.0f*u, cy +  3.0f*u),
                col, 3.0f*u, 0, stroke);
    // Cradle: short verticals on both sides, arc below the capsule
    dl->AddLine(ImVec2(cx + 7.0f*u, cy - 2.0f*u),
                ImVec2(cx + 7.0f*u, cy),           col, stroke);
    dl->AddLine(ImVec2(cx - 7.0f*u, cy - 2.0f*u),
                ImVec2(cx - 7.0f*u, cy),           col, stroke);
    dl->PathArcTo(ImVec2(cx, cy), 7.0f*u, 0.0f, 3.14159f, 24);
    dl->PathStroke(col, 0, stroke);
    // Stand
    dl->AddLine(ImVec2(cx, cy + 7.0f*u),
                ImVec2(cx, cy + 10.0f*u), col, stroke);
}

// Lucide `mic-off` — mic + diagonal slash.
inline void micOff(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                   float stroke = 1.6f) {
    mic(dl, cx, cy, size, col, stroke);
    // Diagonal slash (top-left to bottom-right)
    dl->AddLine(ImVec2(cx - size * 0.45f, cy - size * 0.45f),
                ImVec2(cx + size * 0.45f, cy + size * 0.45f), col, stroke + 0.4f);
}

// Lucide `circle-dot` — outer ring + small filled dot. Used for REC.
inline void circleDot(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                      float stroke = 1.6f) {
    float r = size * 0.45f;
    dl->AddCircle(ImVec2(cx, cy), r, col, 28, stroke);
    dl->AddCircleFilled(ImVec2(cx, cy), r * 0.45f, col, 16);
}

// Lucide `radio` — broadcast antenna with concentric arcs. Used for STREAM.
inline void radio(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                  float stroke = 1.6f) {
    float r = size * 0.48f;
    // Center dot
    dl->AddCircleFilled(ImVec2(cx, cy), size * 0.07f, col, 12);
    // Inner left/right arcs
    dl->PathArcTo(ImVec2(cx, cy), r * 0.55f, 0.65f * 3.14159f, 1.35f * 3.14159f, 14);
    dl->PathStroke(col, 0, stroke);
    dl->PathArcTo(ImVec2(cx, cy), r * 0.55f, -0.35f * 3.14159f, 0.35f * 3.14159f, 14);
    dl->PathStroke(col, 0, stroke);
    // Outer left/right arcs
    dl->PathArcTo(ImVec2(cx, cy), r, 0.70f * 3.14159f, 1.30f * 3.14159f, 16);
    dl->PathStroke(col, 0, stroke);
    dl->PathArcTo(ImVec2(cx, cy), r, -0.30f * 3.14159f, 0.30f * 3.14159f, 16);
    dl->PathStroke(col, 0, stroke);
}

// Lucide `volume-2` — speaker + two outward arcs. Used for audio output.
inline void volume(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                   float stroke = 1.6f) {
    float w = size * 0.42f;
    float h = size * 0.42f;
    // Speaker body (small rect on left)
    float bx = cx - w * 0.5f;
    dl->AddRectFilled(ImVec2(bx - w * 0.18f, cy - h * 0.22f),
                      ImVec2(bx,             cy + h * 0.22f), col);
    // Cone (triangle going right from speaker body)
    dl->AddTriangleFilled(
        ImVec2(bx,             cy - h * 0.55f),
        ImVec2(bx,             cy + h * 0.55f),
        ImVec2(bx + w * 0.55f, cy + h * 0.55f), col);
    dl->AddTriangleFilled(
        ImVec2(bx,             cy - h * 0.55f),
        ImVec2(bx + w * 0.55f, cy - h * 0.55f),
        ImVec2(bx,             cy + h * 0.55f), col);
    // Two right-radiating arcs
    float arcCenterX = cx + w * 0.05f;
    dl->PathArcTo(ImVec2(arcCenterX, cy), w * 0.45f, -0.85f, 0.85f, 14);
    dl->PathStroke(col, 0, stroke);
    dl->PathArcTo(ImVec2(arcCenterX, cy), w * 0.78f, -0.85f, 0.85f, 16);
    dl->PathStroke(col, 0, stroke);
}

// Lucide `chevron-up`.
inline void chevronUp(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                      float stroke = 1.8f) {
    float w = size * 0.36f;
    float h = size * 0.18f;
    dl->AddLine(ImVec2(cx - w, cy + h),
                ImVec2(cx,     cy - h), col, stroke);
    dl->AddLine(ImVec2(cx,     cy - h),
                ImVec2(cx + w, cy + h), col, stroke);
}

// Lucide `chevron-down`.
inline void chevronDown(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                        float stroke = 1.8f) {
    float w = size * 0.36f;
    float h = size * 0.18f;
    dl->AddLine(ImVec2(cx - w, cy - h),
                ImVec2(cx,     cy + h), col, stroke);
    dl->AddLine(ImVec2(cx,     cy + h),
                ImVec2(cx + w, cy - h), col, stroke);
}

// Lucide `more-horizontal` — three filled dots in a row. Used for the
// "more / overflow menu" affordance. Filled (not stroked) because dots
// at this size read more clearly as solid pixels.
inline void moreHorizontal(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                           float /*stroke*/ = 1.8f) {
    float r       = size * 0.10f;
    float spacing = size * 0.30f;
    dl->AddCircleFilled(ImVec2(cx - spacing, cy), r, col, 12);
    dl->AddCircleFilled(ImVec2(cx,           cy), r, col, 12);
    dl->AddCircleFilled(ImVec2(cx + spacing, cy), r, col, 12);
}

// Easel brand mark — open inverted-V triangle inside a thin circle, drawn
// in the same Lucide stroke language so the app icon reads as part of the
// icon family in headers and toolbars.
inline void easelMark(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                      float stroke = 1.4f) {
    float r  = size * 0.5f;
    dl->AddCircle(ImVec2(cx, cy), r, col, 24, stroke);
    float tw = r * 0.55f;
    float th = r * 0.50f;
    dl->AddLine(ImVec2(cx - tw, cy + th * 0.5f),
                ImVec2(cx,        cy - th * 0.6f), col, stroke);
    dl->AddLine(ImVec2(cx,        cy - th * 0.6f),
                ImVec2(cx + tw, cy + th * 0.5f), col, stroke);
}

// Lucide `layers` — three stacked diamonds (sheets). Stroke style.
inline void layers(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                   float stroke = 1.6f) {
    float r = size * 0.42f;
    // Bottom diamond
    dl->AddQuad(
        ImVec2(cx - r,  cy + r * 0.3f),
        ImVec2(cx,      cy + r * 0.7f),
        ImVec2(cx + r,  cy + r * 0.3f),
        ImVec2(cx,      cy - r * 0.1f), col, stroke);
    // Middle diamond
    dl->AddQuad(
        ImVec2(cx - r,  cy + r * 0.0f),
        ImVec2(cx,      cy + r * 0.4f),
        ImVec2(cx + r,  cy + r * 0.0f),
        ImVec2(cx,      cy - r * 0.4f), col, stroke);
    // Top diamond
    dl->AddQuad(
        ImVec2(cx - r,  cy - r * 0.3f),
        ImVec2(cx,      cy + r * 0.1f),
        ImVec2(cx + r,  cy - r * 0.3f),
        ImVec2(cx,      cy - r * 0.7f), col, stroke);
}

// Lucide `folder-open` (simple rectangle with tab) — used for Sources.
inline void folder(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                   float stroke = 1.6f) {
    float w = size * 0.45f;
    float h = size * 0.32f;
    // Body
    dl->AddRect(ImVec2(cx - w, cy - h * 0.5f),
                ImVec2(cx + w, cy + h),
                col, 1.5f, 0, stroke);
    // Tab on top-left
    dl->AddLine(ImVec2(cx - w, cy - h * 0.5f),
                ImVec2(cx - w * 0.4f, cy - h * 0.5f), col, stroke);
    dl->AddLine(ImVec2(cx - w * 0.4f, cy - h * 0.5f),
                ImVec2(cx - w * 0.2f, cy - h * 0.85f), col, stroke);
    dl->AddLine(ImVec2(cx - w * 0.2f, cy - h * 0.85f),
                ImVec2(cx + w,        cy - h * 0.85f), col, stroke);
    dl->AddLine(ImVec2(cx + w,        cy - h * 0.85f),
                ImVec2(cx + w,        cy - h * 0.5f), col, stroke);
}

// Lucide `frame` — rounded rect with cross-hair tick marks. Used for Mapping.
inline void frame(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                  float stroke = 1.6f) {
    float r = size * 0.40f;
    // Outer rectangle (no rounding for "frame" feel)
    dl->AddRect(ImVec2(cx - r, cy - r), ImVec2(cx + r, cy + r),
                col, 1.5f, 0, stroke);
    // Plus-mark in the center
    dl->AddLine(ImVec2(cx - r * 0.45f, cy),
                ImVec2(cx + r * 0.45f, cy), col, stroke);
    dl->AddLine(ImVec2(cx, cy - r * 0.45f),
                ImVec2(cx, cy + r * 0.45f), col, stroke);
}

// Lucide `volume-2` — speaker silhouette + two outward-curving sound waves.
// Path 1: speaker — trapezoid-ish silhouette traced from canonical Lucide
//   coords (M11 4.7 ... .705 .705 0 0 0 1.197.498), simplified to a
//   polygon since the corner blips are sub-pixel at icon sizes.
// Path 2: inner wave (M16 9 a5 5 0 0 1 0 6) — small arc on right.
// Path 3: outer wave (M19.364 18.364 a9 9 0 0 0 0-12.728) — bigger arc.
inline void volume2(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                    float stroke = 1.6f) {
    float u = size / 24.0f;
    auto P = [&](float x, float y) { return ImVec2(cx + x*u, cy + y*u); };
    // Speaker silhouette — 6-vertex polygon centred on viewBox (12,12).
    // Lucide coords:
    //   (11, 4.7) top of speaker
    //   (6.4, 7.6) top-right of cone meets baffle
    //   (3, 8) top-left of baffle (just inside body rect)
    //   (3, 16) bottom-left of baffle
    //   (6.4, 16.4) bottom-right of cone meets baffle
    //   (11, 19.3) bottom of speaker
    ImVec2 spk[6] = {
        P(-1.0f, -7.3f),
        P(-5.6f, -4.4f),
        P(-9.0f, -4.0f),
        P(-9.0f,  4.0f),
        P(-5.6f,  4.4f),
        P(-1.0f,  7.3f),
    };
    dl->AddPolyline(spk, 6, col, stroke, ImDrawFlags_Closed);
    // Inner wave: 5-radius arc from (16,9)→(16,15) bulging right (sweep=1).
    // Centre is at (16-5, 12) = (11, 12) → centred coord (-1, 0). Wait —
    // arc rx=ry=5, endpoints (16,9) & (16,15) on the same circle whose
    // centre lies between them on the perpendicular bisector. Both endpoints
    // are 4u right of viewBox-centre, vertical span 6u, so the centre lies
    // at x=16-? — easier to draw directly: arc on circle r=5 centred at
    // approx (11, 12) (i.e. centred coord (-1, 0)) sweeping the right half.
    dl->PathArcTo(P(-1.0f, 0.0f), 5.0f * u,
                  -3.14159f * 0.295f,  // ~-53° (top endpoint)
                   3.14159f * 0.295f,  // ~+53°  (bottom endpoint)
                  20);
    dl->PathStroke(col, 0, stroke);
    // Outer wave: 9-radius arc from (19.364, 5.636)→(19.364, 18.364)
    // bulging right. Endpoints are ~7.36u right of viewBox-centre,
    // vertical span 12.728u → arc on circle r=9 centred near (10.36, 12)
    // i.e. centred coord (-1.64, 0).
    dl->PathArcTo(P(-1.64f, 0.0f), 9.0f * u,
                  -3.14159f * 0.295f,
                   3.14159f * 0.295f,
                  28);
    dl->PathStroke(col, 0, stroke);
}

// Lucide `palette` — kidney-shaped body with a thumb notch and 4 paint
// dots. The canonical path also has a tiny bottom thumb-tab; that detail
// disappears at icon sizes so we ship the recognizable silhouette only.
inline void palette(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                    float stroke = 1.6f) {
    float u = size / 24.0f;

    // Body: ~300° of a radius-10 circle, leaving a gap on the right for
    // the notch. PathArcTo angles: 0 = +x, π/2 = +y (screen-down).
    float r = 10.0f * u;
    dl->PathArcTo(ImVec2(cx, cy), r,
                   0.30f * 3.14159f,
                   2.0f * 3.14159f - 0.30f * 3.14159f,
                   32);
    // Notch: small inward arc centered outside the body on the +x side,
    // curving back through the body interior and rejoining the outline.
    dl->PathArcTo(ImVec2(cx + 6.5f * u, cy), 4.5f * u,
                   3.14159f - 0.55f,
                   3.14159f + 0.55f,
                   16);
    dl->PathStroke(col, ImDrawFlags_Closed, stroke);

    // Paint dots — Lucide coords (13.5,6.5)/(17.5,10.5)/(6.5,12.5)/(8.5,7.5)
    // relative to viewBox center (12,12).
    float dr = 1.0f * u;
    dl->AddCircleFilled(ImVec2(cx + 1.5f*u, cy - 5.5f*u), dr, col, 10);
    dl->AddCircleFilled(ImVec2(cx + 5.5f*u, cy - 1.5f*u), dr, col, 10);
    dl->AddCircleFilled(ImVec2(cx - 5.5f*u, cy + 0.5f*u), dr, col, 10);
    dl->AddCircleFilled(ImVec2(cx - 3.5f*u, cy - 4.5f*u), dr, col, 10);
}

// Lucide `vector-square` — four corner handles connected by gentle arcs.
// Used for the Mapping tab (vector / warp / shape editing affordance).
//   4× rect 5x5 rx=1 at corners (±10..±5, ±10..±5)
//   4× gentle arcs (R=24, sagitta < 1u over chord 10u) — flattened to lines
inline void vectorSquare(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                         float stroke = 1.6f) {
    float u = size / 24.0f;
    float r = 1.0f * u;
    dl->AddRect(ImVec2(cx - 10.0f*u, cy - 10.0f*u),
                ImVec2(cx -  5.0f*u, cy -  5.0f*u), col, r, 0, stroke);
    dl->AddRect(ImVec2(cx +  5.0f*u, cy - 10.0f*u),
                ImVec2(cx + 10.0f*u, cy -  5.0f*u), col, r, 0, stroke);
    dl->AddRect(ImVec2(cx - 10.0f*u, cy +  5.0f*u),
                ImVec2(cx -  5.0f*u, cy + 10.0f*u), col, r, 0, stroke);
    dl->AddRect(ImVec2(cx +  5.0f*u, cy +  5.0f*u),
                ImVec2(cx + 10.0f*u, cy + 10.0f*u), col, r, 0, stroke);
    dl->AddLine(ImVec2(cx - 5.0f*u, cy - 7.5f*u),
                ImVec2(cx + 5.0f*u, cy - 7.5f*u), col, stroke);
    dl->AddLine(ImVec2(cx - 5.0f*u, cy + 7.5f*u),
                ImVec2(cx + 5.0f*u, cy + 7.5f*u), col, stroke);
    dl->AddLine(ImVec2(cx - 7.5f*u, cy - 5.0f*u),
                ImVec2(cx - 7.5f*u, cy + 5.0f*u), col, stroke);
    dl->AddLine(ImVec2(cx + 7.5f*u, cy - 5.0f*u),
                ImVec2(cx + 7.5f*u, cy + 5.0f*u), col, stroke);
}

// Lucide `monitor` — screen on a stand. For screen-capture / Display tab.
//   rect  2,3 20x14 rx=2  → (-10,-9)..(10,5) rx=2
//   M12 17v4              → stand neck (0,5)..(0,9)
//   M8 21h8               → stand base (-4,9)..(4,9)
inline void monitor(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                    float stroke = 1.6f) {
    float u = size / 24.0f;
    dl->AddRect(ImVec2(cx - 10.0f*u, cy - 9.0f*u),
                ImVec2(cx + 10.0f*u, cy + 5.0f*u),
                col, 2.0f*u, 0, stroke);
    dl->AddLine(ImVec2(cx, cy + 5.0f*u),
                ImVec2(cx, cy + 9.0f*u), col, stroke);
    dl->AddLine(ImVec2(cx - 4.0f*u, cy + 9.0f*u),
                ImVec2(cx + 4.0f*u, cy + 9.0f*u), col, stroke);
}

// Lucide `zap` — lightning bolt. ShaderClaw glyph.
// 6-vertex polygon traced from the canonical Lucide path; corner-radius
// blips are flattened.
inline void zap(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                float stroke = 1.6f) {
    float u = size / 24.0f;
    ImVec2 pts[6] = {
        ImVec2(cx -  8.0f*u, cy +  2.0f*u),  // bottom-left of crossbar
        ImVec2(cx +  1.0f*u, cy - 10.0f*u),  // top tip
        ImVec2(cx +  1.0f*u, cy -  2.0f*u),  // top-right of crossbar (top side)
        ImVec2(cx +  8.0f*u, cy -  2.0f*u),  // top-right of crossbar (right side)
        ImVec2(cx -  1.0f*u, cy + 10.0f*u),  // bottom tip
        ImVec2(cx -  1.0f*u, cy +  2.0f*u),  // bottom-left of crossbar (right side)
    };
    dl->AddPolyline(pts, 6, col, stroke, ImDrawFlags_Closed);
}

// Lucide `camera`.
inline void camera(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                   float stroke = 1.6f) {
    float w = size * 0.45f;
    float h = size * 0.30f;
    // Body
    dl->AddRect(ImVec2(cx - w, cy - h * 0.5f),
                ImVec2(cx + w, cy + h), col, 2.0f, 0, stroke);
    // Top notch
    dl->AddRect(ImVec2(cx - w * 0.30f, cy - h * 0.95f),
                ImVec2(cx + w * 0.30f, cy - h * 0.5f), col, 1.0f, 0, stroke);
    // Lens
    dl->AddCircle(ImVec2(cx, cy + h * 0.1f), size * 0.16f, col, 18, stroke);
}

// Lucide `database`.
inline void database(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                     float stroke = 1.6f) {
    float w = size * 0.32f;
    float h = size * 0.10f;
    // Top ellipse (drawn as flattened circle approximation via two arcs)
    auto drawEllipse = [&](float yc) {
        const int n = 14;
        ImVec2 prev;
        for (int i = 0; i <= n; i++) {
            float t = (float)i / (float)n * 6.28318f;
            ImVec2 p(cx + cosf(t) * w, yc + sinf(t) * h);
            if (i > 0) dl->AddLine(prev, p, col, stroke);
            prev = p;
        }
    };
    drawEllipse(cy - size * 0.32f);
    // Connecting sides + middle/bottom curves (just bottom halves of ellipses)
    dl->AddLine(ImVec2(cx - w, cy - size * 0.32f),
                ImVec2(cx - w, cy + size * 0.20f), col, stroke);
    dl->AddLine(ImVec2(cx + w, cy - size * 0.32f),
                ImVec2(cx + w, cy + size * 0.20f), col, stroke);
    // Middle dividing curve
    auto drawHalfEllipse = [&](float yc) {
        const int n = 10;
        ImVec2 prev;
        for (int i = 0; i <= n; i++) {
            float t = 3.14159f + (float)i / (float)n * 3.14159f;
            ImVec2 p(cx + cosf(t) * w, yc + sinf(t) * h);
            if (i > 0) dl->AddLine(prev, p, col, stroke);
            prev = p;
        }
    };
    drawHalfEllipse(cy - size * 0.05f);
    drawHalfEllipse(cy + size * 0.20f);
}

// Lucide `film`.
// Lucide `list` — three horizontal rows, each with a bullet dot on the
// leading edge. Reads as "ordered list / layers stack".
inline void list(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                 float stroke = 1.6f) {
    float w        = size * 0.42f;
    float bulletR  = size * 0.06f;
    float rowGap   = size * 0.22f;
    float bulletX  = cx - w + bulletR;
    float lineX0   = bulletX + bulletR + size * 0.10f;
    float lineX1   = cx + w;
    for (int i = -1; i <= 1; i++) {
        float y = cy + (float)i * rowGap;
        dl->AddCircleFilled(ImVec2(bulletX, y), bulletR, col, 10);
        dl->AddLine(ImVec2(lineX0, y), ImVec2(lineX1, y), col, stroke);
    }
}

// Lucide `clapperboard` — slate body with a hinged top bar carrying three
// diagonal stripes. Reads as "timeline / scene / take" and is a richer
// alternative to `film` for the timeline toggle.
inline void clapperboard(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                         float stroke = 1.6f) {
    // Slate body — slightly taller than wide, rounded rect.
    float w = size * 0.44f;
    float h = size * 0.36f;
    float bodyTopY = cy - h * 0.30f;
    float bodyBotY = cy + h;
    dl->AddRect(ImVec2(cx - w, bodyTopY),
                ImVec2(cx + w, bodyBotY),
                col, 2.0f, 0, stroke);

    // Hinged top bar — sits above the slate, slanting up to the right
    // very slightly so it reads as "open clapper". A horizontal bar is
    // close enough at icon scale and stays crisp.
    float barH    = size * 0.18f;
    float barTopY = bodyTopY - barH;
    float barBotY = bodyTopY;
    dl->AddRect(ImVec2(cx - w, barTopY),
                ImVec2(cx + w, barBotY),
                col, 1.0f, 0, stroke);

    // Three diagonal stripes inside the top bar.
    float stripeStep = (w * 2.0f) / 3.0f;
    for (int i = 0; i < 3; i++) {
        float x0 = cx - w + stripeStep * (float)i + stripeStep * 0.20f;
        float x1 = x0 + stripeStep * 0.55f;
        dl->AddLine(ImVec2(x0, barBotY),
                    ImVec2(x1, barTopY),
                    col, stroke - 0.2f);
    }
}

inline void film(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                 float stroke = 1.6f) {
    float w = size * 0.42f;
    float h = size * 0.42f;
    dl->AddRect(ImVec2(cx - w, cy - h),
                ImVec2(cx + w, cy + h), col, 2.0f, 0, stroke);
    // Side perforation rectangles (4 small per side)
    float sw = w * 0.18f, sh = h * 0.18f;
    for (int i = 0; i < 4; i++) {
        float yc = cy - h + h * 0.5f * (i + 0.5f);
        dl->AddRect(ImVec2(cx - w + 1.5f, yc - sh * 0.5f),
                    ImVec2(cx - w + 1.5f + sw, yc + sh * 0.5f),
                    col, 1.0f, 0, stroke - 0.3f);
        dl->AddRect(ImVec2(cx + w - 1.5f - sw, yc - sh * 0.5f),
                    ImVec2(cx + w - 1.5f, yc + sh * 0.5f),
                    col, 1.0f, 0, stroke - 0.3f);
    }
    // Horizontal middle divider
    dl->AddLine(ImVec2(cx - w + sw + 3.0f, cy),
                ImVec2(cx + w - sw - 3.0f, cy), col, stroke);
}

// Lucide `sliders-horizontal` — three horizontal lines, each with a knob
// at a different position. Reads as "controls / properties / settings".
inline void sliders(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                    float stroke = 1.6f) {
    float w = size * 0.42f;
    float spacing = size * 0.22f;
    float ys[3] = { cy - spacing, cy, cy + spacing };
    float knobOff[3] = { -w * 0.30f, w * 0.35f, -w * 0.05f };
    float knobR = size * 0.10f;
    for (int i = 0; i < 3; i++) {
        dl->AddLine(ImVec2(cx - w, ys[i]),
                    ImVec2(cx + w, ys[i]), col, stroke);
        dl->AddCircleFilled(ImVec2(cx + knobOff[i], ys[i]), knobR + 0.5f,
                            IM_COL32(20, 22, 28, 255), 12);
        dl->AddCircle(ImVec2(cx + knobOff[i], ys[i]), knobR, col, 12, stroke);
    }
}

// Lucide `audio-lines` — four vertical bars of varying heights, like a VU
// meter. Reads as "audio".
inline void audioLines(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                       float stroke = 1.8f) {
    float spacing = size * 0.18f;
    float heights[4] = { 0.45f, 0.85f, 0.55f, 0.75f };
    for (int i = 0; i < 4; i++) {
        float x = cx + (i - 1.5f) * spacing;
        float h = size * heights[i] * 0.5f;
        dl->AddLine(ImVec2(x, cy - h), ImVec2(x, cy + h), col, stroke);
    }
}

// Lucide `music` (simplified) — single eighth-note shape. Used for MIDI.
inline void music(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                  float stroke = 1.6f) {
    float r = size * 0.16f;
    // Note head (bottom-left)
    dl->AddCircle(ImVec2(cx - size * 0.20f, cy + size * 0.22f), r, col, 14, stroke);
    // Stem (vertical line up from head to top-right)
    dl->AddLine(ImVec2(cx - size * 0.20f + r * 0.8f, cy + size * 0.22f),
                ImVec2(cx + size * 0.30f,             cy - size * 0.32f),
                col, stroke);
    // Flag at the top
    dl->AddLine(ImVec2(cx + size * 0.30f, cy - size * 0.32f),
                ImVec2(cx + size * 0.40f, cy - size * 0.10f),
                col, stroke);
}

// Lucide `activity` — heartbeat / pulse line. A flat segment, then a
// down-spike, an up-spike, another down-spike, then flat. Reads as
// "reactive to live signal" — used for parameter binding affordance
// (audio / MIDI / data driven).
inline void activity(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                     float stroke = 1.6f) {
    float w = size * 0.45f;
    // Five points across the width — flat-down-up-down-flat profile.
    ImVec2 p[5] = {
        ImVec2(cx - w,         cy),
        ImVec2(cx - w * 0.45f, cy),
        ImVec2(cx - w * 0.20f, cy - w * 0.55f),  // up spike
        ImVec2(cx + w * 0.05f, cy + w * 0.55f),  // down spike
        ImVec2(cx + w * 0.30f, cy),
    };
    for (int i = 0; i < 4; i++) dl->AddLine(p[i], p[i+1], col, stroke);
    // Tail to the right — the flat segment that closes the trace.
    dl->AddLine(p[4], ImVec2(cx + w, cy), col, stroke);
}

// Lucide `sparkles` — 4 filled triangles forming a star, plus a small
// + sparkle and a small filled dot. AddConvexPolyFilled with non-convex
// vertices was crashing on first frame, so this version uses 4 separate
// triangle fills (each is a convex kite-half) which stays safe.
inline void sparkles(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                     float /*stroke*/ = 1.6f) {
    float mx = cx - size * 0.05f;
    float my = cy + size * 0.05f;
    float r  = size * 0.42f;
    float w  = r * 0.18f;
    // Top spike: triangle (top, upper-right concave, upper-left concave).
    dl->AddTriangleFilled(ImVec2(mx, my - r),
                          ImVec2(mx + w, my - w),
                          ImVec2(mx - w, my - w), col);
    // Right spike.
    dl->AddTriangleFilled(ImVec2(mx + r, my),
                          ImVec2(mx + w, my + w),
                          ImVec2(mx + w, my - w), col);
    // Bottom spike.
    dl->AddTriangleFilled(ImVec2(mx, my + r),
                          ImVec2(mx - w, my + w),
                          ImVec2(mx + w, my + w), col);
    // Left spike.
    dl->AddTriangleFilled(ImVec2(mx - r, my),
                          ImVec2(mx - w, my - w),
                          ImVec2(mx - w, my + w), col);
    // Center diamond (fills the gap between the four spikes).
    dl->AddQuadFilled(ImVec2(mx, my - w),
                      ImVec2(mx + w, my),
                      ImVec2(mx, my + w),
                      ImVec2(mx - w, my), col);
    // Small + sparkle at top-right.
    float sx = cx + size * 0.36f;
    float sy = cy - size * 0.36f;
    float sl = size * 0.10f;
    dl->AddLine(ImVec2(sx, sy - sl), ImVec2(sx, sy + sl), col, 1.4f);
    dl->AddLine(ImVec2(sx - sl, sy), ImVec2(sx + sl, sy), col, 1.4f);
    // Tiny filled dot at bottom-left.
    dl->AddCircleFilled(ImVec2(cx - size * 0.36f, cy + size * 0.36f),
                        size * 0.07f, col, 10);
}

// Lucide `settings` — gear icon. Eight-tooth gear with a centered
// circular hub. Stroke-only to match the rest of the family.
inline void settings(ImDrawList* dl, float cx, float cy, float size, ImU32 col,
                     float stroke = 1.6f) {
    const int teeth = 8;
    float rOuter = size * 0.46f;
    float rInner = size * 0.34f;
    float rHub   = size * 0.16f;
    float halfTooth = (3.14159f / teeth) * 0.45f;  // tooth angular half-width
    // Gear outline — alternate between rOuter (tooth tip) and rInner (gap)
    // around the circle. 8 teeth × 2 segments each = 16 vertices.
    dl->PathClear();
    for (int i = 0; i < teeth * 2; i++) {
        float a = (3.14159f * 2.0f) * (float(i) / (teeth * 2));
        float r = (i % 2 == 0) ? rOuter : rInner;
        // Round each tooth slightly by inserting two close-by vertices
        // separated by halfTooth — gives the gear a true notched profile.
        float a1 = a - halfTooth * 0.5f;
        float a2 = a + halfTooth * 0.5f;
        dl->PathLineTo(ImVec2(cx + cosf(a1) * r, cy + sinf(a1) * r));
        dl->PathLineTo(ImVec2(cx + cosf(a2) * r, cy + sinf(a2) * r));
    }
    dl->PathStroke(col, ImDrawFlags_Closed, stroke);
    // Center hub circle.
    dl->AddCircle(ImVec2(cx, cy), rHub, col, 16, stroke);
}

} // namespace lucide
