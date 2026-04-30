# Scenes, Presets, and Stage UI Audit

## Findings

### 1. What is a Scene?

**File**: `src/app/SceneManager.h:21–24`

A **Scene** is a *named snapshot of all layer states*:
```cpp
struct Scene {
    std::string name = "Scene";
    std::vector<SceneLayerState> layers;  // Per-layer snapshots
};
```

Each `SceneLayerState` (lines 9–18) captures:
- **Layer visibility, opacity, blend mode**
- **Transform**: position, scale, rotation
- **Effects** (filters/compositing effects)

**What it saves**: Layer composition state only — NOT shader parameters, NOT 3D stage setup, NOT timing/duration.

**What it does NOT save**: 
- Shader uniforms (ISF parameters)
- 3D projector/display setup
- Timeline/animation state
- Audio settings

---

### 2. What is a Preset?

**File**: `src/app/OutputZone.h:20` + `src/ui/PropertyPanel.cpp:959–961`

A **Preset** refers to *resolution presets* for output zones, not layer snapshots. The UI tab is misleadingly named "Presets" but actually shows **"Save Scene"** functionality.

**Actual usage in code**:
- `compPreset = src.compPreset;` — indices into a list of preset resolutions (4K, 1080p, 720p, etc.)
- Not related to the Scenes system at all

**Conclusion**: The word "Presets" in the Scenes tab is confusing. It should be labeled **"Scenes"** or **"Snapshots"**.

---

### 3. Where is the Scenes Tab Rendered?

**File**: `src/app/Application.cpp:2118–2161`

```cpp
if (m_ui.isPanelVisible("Scenes")) {
    ImGui::Begin("Scenes");
    if (ImGui::BeginTabBar("##ScenesTabs")) {
        if (ImGui::BeginTabItem("Presets")) {
            // MAIN CONTENT: Save Scene button + list of saved scenes
            ImGui::Button("Save Scene", ...);
            for (int s = 0; s < m_sceneManager.count(); s++) {
                ImGui::Button(scene.name.c_str(), ...);  // Recall scene
                ImGui::SmallButton("x");                  // Delete scene
            }
        }
        if (ImGui::BeginTabItem("Stage")) {
            m_stageView.renderSceneInspector(zoneTextures);  // 3D element inspector
        }
    }
}
```

**Line 2117**: Comment explains the intent: `"Unified "Scenes" panel: two tabs — Presets (save/recall) and Stage (3D inspector)."`

---

### 4. Where is the Stage Tab / Stage Panel?

There are **TWO separate Stage UI elements**:

1. **Standalone Stage Panel** (file:line `2102–2115`):
   - `ImGui::Begin("Stage")` NOT called; instead: `m_stageView.render(zoneTextures, nullptr)`
   - Renders the **full 3D viewport** with orbit camera, model display, projection frustums
   - **Purpose**: Interactive 3D pre-visualization + model manipulation

2. **Stage Tab inside Scenes panel** (file:line `2154–2156`):
   - `ImGui::BeginTabItem("Stage")` inside the "Scenes" `BeginTabBar`
   - Calls `m_stageView.renderSceneInspector()` (file `src/stage/StageView.cpp:775+`)
   - **Purpose**: List/edit displays, projectors, surfaces, clusters (inspector only, no 3D viewport)

---

### 5. Why Does "Stage" Appear INSIDE the Scenes Tab?

**Root Cause**: Architectural split to avoid UI clutter.

**What happened**:
- Early design: one big "Stage" panel with both 3D viewport + inspector UI
- Problem: the inspector became verbose (displays list, projector list, surface list, cluster UI, etc.)
- Solution: Move inspector to a tab inside "Scenes" to consolidate 3D config with scene snapshots
- Result: Two "Stage" references now exist

**File evidence** (comments at line 2103–2104):
```cpp
// No inline top section — Composition + Output are in the Canvas
// zone bar now, so the Stage panel is pure 3D viewport.
```

**Mental model**:
- **"Stage" panel** = 3D preview (render-only)
- **"Scenes" panel with Stage tab** = 3D inspector (edit displays/projectors/surfaces)

This is **confusing** because they have the same name but different functions.

---

### 6. Canvas vs Stage vs Scenes — Conceptual Roles

| Term | What it is | Where rendered | What it does |
|------|-----------|-----------------|-------------|
| **Canvas** | 2D composition layer editor | Tabs in main viewport | Edit layers (add/remove/reorder), inline zoom, pan |
| **Stage** | 3D pre-visualization space | Standalone "Stage" panel | Display 3D model + projection frustums + displays, orbit/pan camera, import models |
| **Scenes** | Layer state snapshots | "Scenes" panel, "Presets" tab | Save/recall layer composition (visibility, opacity, transforms, effects) |
| **Stage Inspector** | 3D config editor | "Scenes" panel, "Stage" tab | Edit displays (pos/rot/size/zone), edit projectors (pos/target/FOV), edit surfaces/clusters |

**Redundancy detected**:
- The word "Stage" refers to both the 3D viewport AND the inspector
- No clear separation between "edit 3D config" (Stage tab in Scenes) and "view 3D result" (Stage panel)

---

### 7. Recommended Fix

**Problem Summary**:
- User sees "Scenes" tab → clicks it → finds "Presets" and "Stage" tabs inside
- "Stage" is confused with the standalone "Stage" panel
- "Presets" is mislabeled (it's actually "Save/Recall Scenes")

**Recommended Changes** (minimal code churn):

1. **Rename the "Presets" tab to "Snapshots"** (or "Scenes Snapshots")
   - File: `src/app/Application.cpp:2121`
   - Change: `ImGui::BeginTabItem("Presets")` → `ImGui::BeginTabItem("Snapshots")`
   - Reason: Clarifies that this is scene capture, not resolution presets

2. **Rename the nested "Stage" tab to "Inspector"** or "Setup"
   - File: `src/app/Application.cpp:2154`
   - Change: `ImGui::BeginTabItem("Stage")` → `ImGui::BeginTabItem("Inspector")`
   - Reason: Disambiguates from the standalone "Stage" 3D viewport panel

3. **Update the comment** at line 2117:
   - Current: `"Unified "Scenes" panel: two tabs — Presets (save/recall) and Stage (3D inspector)."`
   - New: `"Unified "Scenes" panel: two tabs — Snapshots (layer state) and Inspector (3D display/projector config)."`

4. **Consider** adding a warning/tooltip in the "Stage" panel render (line 2102):
   - "This is the 3D preview. Configure displays and projectors in the Scenes > Inspector tab."

**Alternative (larger refactor)**: Separate "Scenes" and "3D Config" into two panels entirely. But the above renames are lower-friction and solve 80% of the confusion.

---

## Summary

- **Scenes** = Layer snapshots (visibility, opacity, transform, effects)
- **Presets** (mislabeled) = Actually layer scene snapshots (not resolution presets)
- **Stage** (dual use) = Both the 3D viewport AND the 3D config inspector
- **Confusion**: Nested Stage tab inside Scenes + same "Stage" name for separate panel
- **Fix**: Rename "Presets" → "Snapshots", "Stage tab" → "Inspector"
