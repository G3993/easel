# Easel Timeline Integration Map

## 1. Timeline → LayerStack (applyToLayers)
**File:** `src/timeline/Timeline.cpp:194–274`

### What applyToLayers() mutates on layers:
- **`layer->visible`** — set to `true` on clip enter, fades out on clip exit
- **`layer->source`** — swapped to new ContentSource from clip.sourcePath (line 255)
- **`layer->nextSource`** — queued when shader transition is active (line 140, Layer::startShaderTransition)
- **`layer->transitionProgress`** — reset to 1.0f on fresh clip enter (line 230)
- **`layer->transitionActive`** — triggered for fade-out at clip boundary (line 265)
- **`layer->shaderTransitionActive`** — set true on clip-enter if shader transition configured (line 239, then Layer::startShaderTransition line 142)
- **`layer->transitionDirection`** — set false for exit fade (line 264)

### Layers not in any track:
**Orphaned completely** — applyToLayers() only iterates `m_tracks`, never touches layers without a track. Orphan layers stay invisible (src/app/Application.cpp:3524 shows UI creates track on demand via `ensureTrack()`).

---

## 2. Timeline → ContentSource loading (loadSourceForPath)
**File:** `src/timeline/Timeline.cpp:173–192`

### Source types created:
- **ShaderSource** — extensions: `.fs`, `.frag`, `.isf` (line 180–183)
- **VideoSource** — all other extensions when `HAS_FFMPEG=ON` (line 186–188)
- **Failure** — returns nullptr if load fails or HAS_FFMPEG disabled

### Caching behavior:
**No caching** — called once per clip-enter (line 234). Each source reload is a fresh parse + decode initiation. VideoSource has per-instance WASAPI thread.

### Cost:
- ShaderSource: file parse + ISF compilation (can be ~100ms for complex shaders)
- VideoSource: FFmpeg codec setup + audio thread spawn (can be ~500ms for hardware decode init)

---

## 3. Timeline ↔ Application render loop
**File:** `src/app/Application.cpp:462–468`

### Call order (CRITICAL):
```
frame_start:
  → updateSources()              [audio analysis, shader uniforms]
  → m_timeline.advance(dt)       [playhead += dt, loop check]
  → m_timeline.applyToLayers()   [clip → layer mutations]
  → compositeAndWarp()           [render layer stack to FBO]
  → presentOutputs()             [VideoRecorder pulls texture]
  → UI render
```

### Ordering assumptions:
- **MUST**: applyToLayers called AFTER advance (playhead must be current)
- **MUST**: applyToLayers called BEFORE compositeAndWarp (layer state must be live)
- **DANGER**: If you defer applyToLayers to next frame, transitions will stutter

---

## 4. VideoRecorder trigger surface
**File:** `src/app/Application.cpp:4056, 4068, 1244–1246`

### Start/stop invocation:
- **Start** (line 4056): `m_recorder.start(fname, zone.warpFBO.width(), zone.warpFBO.height(), 30)`
- **Stop** (line 4068): `m_recorder.stop()`
- Called from UI timeline panel via ImGui controls (user action)

### Texture source:
**`active.readbackFBO.textureId()`** (line 1245) — NOT warpFBO, but readbackFBO (post-color-management, pre-UI)

### FBO assumption:
Hard-coded to `readbackFBO` of active zone. Must exist and have valid texture.

### Framerate:
**Fixed 30 fps** (hardcoded in line 4056). Dynamic framerate not supported; encoder thread operates on swapchain time (av_gettime_relative, VideoRecorder.cpp:44, 68).

---

## 5. Audio path
**File:** `src/sources/VideoSource.h:59–85` + `src/sources/VideoSource.cpp:20–127`

### Architecture:
Each VideoSource instance with `hasAudio()=true` spawns **its own WASAPI thread** that:
1. Decodes audio frames from FFmpeg (m_audioCodecCtx)
2. Writes to ring buffer (m_audioRing)
3. Outputs directly to Windows audio device (m_audioClient/IAudioRenderClient)

### Multi-clip audio:
**NO mixing** — if two video clips overlap, **both play audio independently**, resulting in sum at speaker (potential clipping, no gain compensation). No AudioMixer exists for timeline audio.

**Recorder audio** (separate): VideoRecorder.cpp captures from WASAPI device (line 79–83), not from layers. Recorder must have a capture device selected (m_selectedAudioDevice).

---

## 6. Layer lifecycle
**File:** `src/compositing/LayerStack.cpp:4–27` + `src/app/Application.cpp:1637, 4201, 4286`

### Layer add/remove → Timeline notification:
**NONE** — LayerStack.cpp has no callback to Timeline. 

### Orphan tracks:
**Accumulate in memory** — When a layer is deleted (Application.cpp:1637, 4201, 4286), code explicitly calls `m_timeline.removeTrackForLayer(rid)` to clean up. **If you remove a layer without calling removeTrackForLayer, the track persists**.

### Risk:
- Undo/redo layer deletion that doesn't sync Timeline → orphan tracks in save file
- Batch layer delete without loop calling removeTrackForLayer → lost references

---

## 7. Shader transitions
**File:** `src/timeline/Timeline.cpp:236–239` + `src/compositing/Layer.h:138–143`

### Trigger:
Clip-enter edge (line 226) triggers Timeline::applyToLayers → Layer::startShaderTransition() if:
- `transitionType == Shader` ✓
- `transitionShaderPath` not empty ✓
- prevActiveId != 0 (not first-ever clip) ✓

### Race condition potential:
**SAFE** — startShaderTransition sets flags synchronously; shader render happens in compositeZone (after applyToLayers). Timeline does not spawn threads.

### State collision:
If clip-enter fires while previous transition still active (transitionProgress < 1), Timeline checks `!layer->shaderTransitionActive` (line 229) — won't re-trigger. Old transition completes before new clip can transition.

---

## 8. Persistence
**File:** `src/app/Application.cpp:4885, 5314` + `src/timeline/Timeline.cpp:110–169`

### Save entry point:
**Application::saveProject()** line 4885:
```cpp
j["timeline"] = m_timeline.toJson();
```
Called on user File→Save or auto-save every 30 sec (line 475).

### Load entry point:
**Application::loadProject()** line 5314:
```cpp
m_timeline.fromJson(j["timeline"]);
```
Called on File→Open or startup (default.easel).

### Serialization:
- **toJson()** (line 110–138): duration, playhead, loop, nextClipId, tracks[] with clips[], muted/solo
- **fromJson()** (line 140–169): reverse + clip.id collision handling (line 157)

---

## 9. External code touching Timeline internals

### UI queries (Application.cpp):
- Line 3493: `m_timeline.playhead()` — UI progress bar
- Line 3495: `m_timeline.duration()` — UI duration field
- Line 3483: `m_timeline.isPlaying()` — play button state
- Line 3649: `m_timeline.tracks()` — clip grid rendering

### Recorder (Application.cpp):
- Uses m_timeline state to drive START/STOP UI only (user-facing, not data-driven)

### Layer deletion cascade (Application.cpp):
- Line 1637, 4201, 4286: `m_timeline.removeTrackForLayer(rid)` — **critical cleanup**

### No external readers:
- No code queries Timeline::findClip outside Timeline
- No code directly accesses TimelineTrack beyond Application UI rendering

---

## TOP 5 "DANGER — DON'T BREAK THESE"

### 🔴 1. applyToLayers() call order (src/app/Application.cpp:463–464)
**Why:** If you move applyToLayers before compositeAndWarp or after presentOutputs, clips will render one frame delayed or skip transitions entirely. Recorder will see stale layer state.
**Touch point:** `m_timeline.advance(dt)` MUST precede `m_timeline.applyToLayers()` MUST precede `compositeAndWarp()`.

### 🔴 2. Layer→Track lifetime management (src/timeline/Timeline.cpp + Application.cpp:1637, 4201, 4286)
**Why:** Orphan tracks silently accumulate. Save file balloons. If you redesign Track storage, orphan cleanup must still happen on layer deletion.
**Touch point:** Every `LayerStack::removeLayer()` or layer-delete UI action must call `Timeline::removeTrackForLayer()` explicitly.

### 🔴 3. VideoSource per-instance WASAPI threads (src/sources/VideoSource.cpp:20–127)
**Why:** Two overlapping video clips = two audio threads writing to same output device → unpredictable mix. If you centralize audio, you must drain all VideoSource rings or replace with AudioMixer.
**Touch point:** Any Timeline change that allows overlapping clips must address AudioMixer integration.

### 🔴 4. Recorder FBO binding (src/app/Application.cpp:1244–1246)
**Why:** Recorder pulls from `active.readbackFBO.textureId()`, not warpFBO. If you change which FBO is active or refactor readbackFBO, recorder will capture wrong content.
**Touch point:** Verify `readbackFBO` state before/after any zone or composite refactor.

### 🔴 5. Shader transition state machine (src/timeline/Timeline.cpp:236–239 + src/compositing/Layer.h:138–142)
**Why:** Three overlapping flags: transitionActive (fade), shaderTransitionActive (ISF blend), nextSource (queued). If you redesign transitions, clip-enter + shader transition + overlap must remain atomic and non-blocking.
**Touch point:** applyToLayers() must remain synchronous; startShaderTransition() must not spawn threads.

