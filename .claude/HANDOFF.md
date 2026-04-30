# Easel UX session handoff — 2026-04-24

## Shipped this session (live in source tree)
- **Canvas/Stage switcher** is a segmented pill on the second row (left of zone tabs). Works via `UIManager::sShowStage` flag — only one of Canvas/Stage submits its `ImGui::Begin()` per frame. No dock tab bar shown.
- **MAPPING combo removed** from Canvas toolbar.
- **COMPOSITION moved to a zone-chip** (`1920 x 1080` button, opens preset popover). See `ViewportPanel.cpp` right after the zone-tabs loop.
- **Properties panel**: "Parameters" heading removed. Transition section moved to bottom of Parameters. Crop moved under Transform.
- **Layer panel**: full-width rounded row container. Opacity % is draggable (InvisibleButton + drag delta — doesn't move the panel anymore). Zone dots inline with type caption. Thumbnail rounding 6px.
- **Audio panel**: gain slider labels (Input/Bass/Low/High/Treble/Gate) now in a 64px left column — previously clipped off-panel.
- **Mapping panel**: Masks-section hairline dividers removed. Corner-Pin label column widened to 56px.
- **Inspector tabs**: bundled icons (Properties=filter, Mapping=edit-tools, Audio=audio, MIDI=controller). Loaded as alpha-mask textures from `assets/icons/*.png`; `drawInspectorTabIcons()` overlays them on top of the padded-space tab labels.
- **Timeline tab bar hidden** (`ImGuiDockNodeFlags_NoTabBar`).
- **`inputTex` → `Texture`** for generic image-input names in Properties.

## Still TODO (top 3 UX recommendations partially done)
| # | Task | Effort | Where |
|---|---|---|---|
| 1 | **OUTPUT combo → Mapping inspector tab** | Medium | `ViewportPanel.cpp` lines ~274–449 hold the combo. Needs to move into `WarpEditor::render` (or a new section rendered above it in Application's Mapping Begin block). `WarpEditor::render` needs the active `OutputZone&`, the `monitors` vector, `ndiAvailable` flag, and `editorMonitor` — pass them the same way Canvas already receives them. |
| 4 | **Delete Layer + Zone top-level menus** | Small | `Application.cpp` line 6247 is `if (ImGui::BeginMenu("Layer"))` block; line 6282 is `if (ImGui::BeginMenu("Zone"))` block. Delete both. Every item is already on the right-click context menus for layers / zone tabs. |
| 5 | **GO LIVE → next to REC in Timeline transport** | Medium | Remove from `Application::renderMenuBar()` around line 6300. Add as a pill in the timeline transport row — see `Application::renderTimelinePanel()` around `Application.cpp:3904` where REC is drawn via `pillBtn`. Clone that pattern. |
| 7 | **Parameters density pass** | Medium | `PropertyPanel.cpp` around line 1392 (shader input render loop). Goal: ~22–24px per row; label inside slider track (left-aligned muted); value right-aligned; tiny dot on track when audio/MIDI-bound (instead of amber fill); bolt/bind icon only on hover. For `point2D` put X/Y on one row with shared label. For `bool` right-align toggle on label line. For `color` inline swatch with label. Drop `Dummy(0,6)` spacers between consecutive floats. |

(#6 — moving the Canvas/Stage pill to the menu bar row — was rejected by the user.)

## Skip list (UX reviewer's "don't do")
- Don't move OUTPUT into Stage panel (Stage is 3D scene, not routing).
- Don't add a global Workspace enum (Design/Live/Mapping).
- Don't fold GO LIVE into File menu.
- Don't nest Properties/Mapping/Audio/MIDI under a parent "Inspector".
- Don't remove per-zone color dot — ambient routing signal.

## Known quirks
- Timeline dock starts a little high by default — user wants it nudged down. Exact target height TBD (user said they'd send a screenshot). Control is `timelineSplit` in `UIManager.cpp:538` (currently `90.0f / dockSize.y`, clamped to `[0.06, 0.20]`). Bumping the numerator or raising the clamp floor is the one-line fix.
- `UIManager::sShowStage` is a static. Works but is a global; if you ever need per-window state migrate it to a member.
- `drawInspectorTabIcons` tints alpha-mask PNGs — if you swap icons, keep them black-on-white qlmanage thumbnails; the loader inverts luma to alpha.

## Build / run
```
cd ~/Easel
cmake --build build --target Easel
./build/Easel.app/Contents/MacOS/Easel
```
