# Easel UI Redesign — Self-Driving Execution Plan

This plan is designed to run **autonomously across multiple sessions**. Each phase has explicit pass criteria, a verification screenshot, and a self-correction loop.

---

## How to use this plan

In any fresh session, run:
```
"Continue UI redesign from .planning/UI-REDESIGN-PLAN.md. Start with the first phase whose checkbox is unchecked. Run the auto-test loop until pass criteria are met, then check the box and continue to the next phase."
```

The agent should:
1. Read this file
2. Find the first `[ ]` unchecked phase
3. Execute its `Tasks`
4. Run the `Auto-test loop` at the end of each task
5. Compare screenshot to `Pass criteria`
6. Iterate (max 5 attempts per phase) until pass
7. Check the box `[x]`, commit, continue

---

## Reference images

Three target screenshots (saved on user's disk, also embedded in past chat):
- **Adobe Fresco iPad** — left rail tools + flyout, right thumbnail stack
- **Easel mockup A** — Canvas chip top, left rail w/ layer thumbnails, right Properties popover w/ mini-tabs + xy-pad widget, floating bottom timeline panel
- **Easel mockup B** — minimal: Canvas pill top-center, top-right Preview/Resolution/Live pills, left rail (5 nav icons + 2 layer thumbnails below divider), right rail (5 transform tools + vertical zoom slider), centered canvas with crosshair grid + corner handles, floating bottom transport pill

---

## Auto-test loop (use after every code change)

```bash
cmake --build /Users/lu/easel/build --target Easel 2>&1 | tail -3
pkill -f "Easel.app/Contents/MacOS/Easel" 2>&1; sleep 1
rm -f /Users/lu/easel/build/Easel.app/Contents/Resources/imgui.ini \
      ~/Library/Application\ Support/Easel/imgui.ini 2>/dev/null
open /Users/lu/easel/build/Easel.app
sleep 4
osascript -e 'tell application "Easel" to activate'
sleep 1
screencapture -x /tmp/easel-test.png
# then in agent: Read /tmp/easel-test.png and compare to reference
```

After each loop the agent **MUST**:
- Read `/tmp/easel-test.png` via the image tool
- Compare visually to the phase's pass criteria
- If any criterion fails → write/edit code to fix → loop again
- Cap at 5 iterations per phase. If still failing, write findings to `.planning/UI-REDESIGN-NOTES.md` and skip phase with a note.

---

## Current status
- ✅ Phase 1a-1c: Activity rail wired (`UIManager::renderLeftRail`), gated by `isPanelVisible`, left float shifted by rail width.
- ⚠️ Phase 1d: Rail visible but visually thin — needs polish.

---

## Phases

### [x] Phase 1d — Rail polish
**Goal:** rail icons match reference B in size, padding, contrast.

**Tasks:**
1. Bump `kLeftRailW` 52 → 64 in `UIManager.h`
2. In `renderLeftRail`: increase button height 38 → 44, glyph size 22 → 26
3. Add 1px hairline divider at top below the workspace bar
4. Push `WindowBg` darker: `IM_COL32(10, 11, 14, 255)`

**Pass criteria (compare to reference B):**
- [ ] Rail visible on far-left edge, 60-70px wide
- [ ] At least 3 stacked rounded-pill icon buttons
- [ ] Active button has visible background highlight
- [ ] Rail does NOT overlap canvas viewport

**Files:** `src/ui/UIManager.h`, `src/ui/UIManager.cpp::renderLeftRail`

---

### [x] Phase 4 — Minimal top bar
**Goal:** remove EDIT/FILE/LAYER/ZONE menu strip; replace with center Canvas pill + right-edge Preview/Resolution/Live pills.

**Tasks:**
1. Find `renderMenuBar()` in `Application.cpp`, replace EDIT/FILE/LAYER/ZONE with a thin button that opens a "..." menu
2. Center the workspace switcher pill (Canvas/Stage/Show) horizontally
3. Right-align: Preview pill, Resolution pill ("1920×1200"), Live pill (green dot)

**Pass criteria:**
- [ ] No EDIT/FILE/LAYER/ZONE strip
- [ ] Canvas pill visually centered in top bar
- [ ] 3 pills tight to right edge

**Files:** `src/app/Application.cpp::renderMenuBar`

---

### [~] Phase 5 — Floating transport pill (skipped — see UI-REDESIGN-NOTES.md, blocked on Phase 7)
**Goal:** replace docked bottom transport with a floating pill at bottom-center.

**Tasks:**
1. Identify current transport rendering (search `play\|pause\|REC\|GO LIVE`)
2. Wrap in a fixed-position `ImGui::Begin("##TransportPill")` window with `NoDocking|NoTitleBar|NoBackground`
3. Position via `SetNextWindowPos` to viewport bottom-center (vp.WorkPos.y + WorkSize.y - 80)
4. Width auto-fit; rounded pill background (FrameBg dark + 16px rounding)
5. Time display below pill, plain text

**Pass criteria:**
- [ ] Transport buttons in a single horizontal pill
- [ ] Pill floats over canvas (canvas visible below it)
- [ ] No bottom dock bar

**Files:** `src/app/Application.cpp` (transport bar code, lines 4900-5200)

---

### [x] Phase 2 — Rail layer thumbnails
**Goal:** layer stack thumbnails appear at the bottom of the rail, below a divider.

**Tasks:**
1. In `Layer.h`, add `GLuint thumbTex; int thumbFrame;` members
2. In `Application::compositeZone`, every 30 frames per layer: blit layer's `canvasTexture` into a 64×64 thumb FBO
3. In `renderLeftRail`: after navigation icons, draw a hairline divider, then loop over `m_layerStack` and draw 56×56 rounded thumbnails. Click → set `m_selectedLayer`.

**Pass criteria:**
- [ ] Hairline divider visible mid-rail
- [ ] Thumbnails show actual rendered layer content
- [ ] Active layer thumbnail has bright border
- [ ] Click thumbnail switches selected layer (Properties panel updates)

**Files:** `src/compositing/Layer.h`, `src/app/Application.cpp::compositeZone`, `src/ui/UIManager.cpp::renderLeftRail`

---

### [x] Phase 3 — Right transform tool rail
**Goal:** add a thin vertical rail on the right edge with 5 transform tool icons + zoom slider.

**Tasks:**
1. Add `kRightToolRailW = 56` constant
2. New `UIManager::renderRightToolRail()` mirroring renderLeftRail
3. 5 buttons: Move (4-arrow glyph), Rotate (curved arrow), Scale (corner brackets), Flip (mirror), Center (target)
4. Below: vertical `ImGui::VSliderFloat` for canvas zoom 0.25-4.0
5. Properties panel host shifts left by `kRightToolRailW`

**Pass criteria:**
- [ ] Right rail visible, 5 stacked icon buttons + slider
- [ ] Properties panel still readable, positioned left of the tool rail
- [ ] Tool icons render as procedural glyphs, no missing assets

**Files:** `src/ui/UIManager.h`, `src/ui/UIManager.cpp`, `src/app/Application.cpp` (call renderRightToolRail in same place as renderLeftRail)

---

### [x] Phase 8 — Universal styling pass
**Goal:** unify color/spacing/rounding across all panels.

**Tasks:** in `UIManager::applyTheme`:
- `WindowBg` → `IM_COL32(15, 17, 22, 245)`
- Panel chrome: 10px rounding, no border
- All buttons: 8px rounding, FrameBg `IM_COL32(255, 255, 255, 18)`, active `IM_COL32(255, 255, 255, 36)`
- Section headers: small caps, 11pt, dim text color
- Add a thin shadow under floating panels via `ImGui::GetForegroundDrawList()` AddRectFilled

**Pass criteria:**
- [ ] All panels consistent rounding (10 outer, 8 inner)
- [ ] Color hierarchy: bg darkest → panel mid → control highlight brightest
- [ ] No duplicated color tokens (use named constants)

**Files:** `src/ui/UIManager.cpp::applyTheme` only.

---

### [ ] Phase 6 — Properties as contextual flyout
**Goal:** PropertyPanel matches reference A/B style — mini-tabs, collapsible sections, xy-pad widget.

**Tasks:**
1. Add 3 small circular tabs at top of PropertyPanel: Layers / Move / Effects (icon-only)
2. Each tab swaps which section group renders
3. Replace existing flat list with collapsible sections: Transform / Crop / Effects / Tiling
4. Transform: add a circular xy-pad on the left (drag → translate); fields x/y/scale/rotate/w/h on right
5. Tiling: 3×3 dot picker (current code already has tile UI — restyle)
6. Effects: each effect row shows expand arrow + enabled toggle + name + remove (existing code)

**Pass criteria:**
- [ ] PropertyPanel shows 3 mini-tabs at top
- [ ] Sections collapse/expand on click
- [ ] Transform shows circular xy-pad widget
- [ ] All existing inputs still wired (audio binding, MIDI learn intact)

**Files:** `src/ui/PropertyPanel.cpp` (major refactor)

---

### [ ] Phase 7 — Timeline redesign
**Goal:** Timeline matches reference A bottom panel — track list on left, timeline grid on right, per-track lane affordance.

**Tasks:**
1. Floating panel below the transport pill (or replace if Phase 5 lands first)
2. Header strip: time scrub bar with playhead
3. Tracks list (left ~200px): dot color + layer name + "+ Lane" small button
4. Lanes (right): bar showing the layer's active range, click to scrub
5. Audio track at bottom with waveform
6. "+" at bottom to add new lane

**Pass criteria:**
- [ ] Tracks render with name + range bar
- [ ] Audio waveform visible if audio source is connected
- [ ] Click on bar moves playhead to that time
- [ ] Lane add affordance present per track

**Files:** `src/timeline/Timeline.cpp` (major refactor — single biggest file change)

---

## Iteration discipline

- After each phase commit, run `git diff --stat` to verify no unrelated files changed
- Compile suite: `/Users/lu/easel/build/test_shaders /Users/lu/ShaderClaw3/shaders` should still pass 145+ shaders
- Workspace switching (Canvas/Stage/Show) must not regress
- Per-mode panel curation in `isPanelVisible` must stay correct

## Failure escape hatch

If any phase fails 5 auto-test iterations, append to `.planning/UI-REDESIGN-NOTES.md`:
- Phase name
- What was tried
- Final screenshot path
- What looked wrong (one paragraph)

Then skip to next phase. Do NOT block on a stuck phase.

## Total budget

~14h focused work distributed across phases. Recommended: 1 phase per session for clean commit history + visual review at each step.
