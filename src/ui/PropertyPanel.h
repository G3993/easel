#pragma once
#include "compositing/Layer.h"
#include <memory>
#include <string>
#include <vector>

class ShaderSource;
class WhisperSpeech;
class DataBus;
class LayerStack;
class BPMSync;
class SceneManager;
class MIDIManager;
struct OutputZone;

// Speech-to-text state shared between PropertyPanel and Application
struct SpeechState {
    bool available = false;
    bool listening = false;
    ShaderSource* targetSource = nullptr;
    std::string targetParam;
    DataBus* dataBus = nullptr;
    uint32_t activeLayerId = 0; // for data bus binding key
    int* recentWordCap = nullptr; // max words in the *.recent rolling feed (live-adjustable)
    MIDIManager* midi = nullptr; // for MIDI Learn on shader parameter bindings
#ifdef HAS_WHISPER
    WhisperSpeech* whisper = nullptr; // for device selection UI
#endif
};

struct MosaicAudioSource {
    std::string name;
    bool isMic = false;
};

struct MosaicAudioState {
    int* selectedDevice = nullptr;       // -1 = system loopback
    std::vector<MosaicAudioSource> devices;
    float bass = 0, lowMid = 0, highMid = 0, treble = 0;
    float beatDecay = 0;
};

class StageView;
class Timeline;
class LayerPanel;
class UIManager;

class PropertyPanel {
public:
    void render(std::shared_ptr<Layer> layer, bool& maskEditMode,
                SpeechState* speech = nullptr, MosaicAudioState* mosaicAudio = nullptr,
                float appTime = 0.0f, LayerStack* layerStack = nullptr,
                BPMSync* bpmSync = nullptr, SceneManager* sceneManager = nullptr,
                int* audioDeviceIdx = nullptr, MIDIManager* midi = nullptr,
                OutputZone* canvasZone = nullptr, float* targetFPS = nullptr);

    // Shared title + label rhythm so the OTHER right-dock parameter panels
    // (Sources/Shaders/Camera/Display, Audio, Mapping — rendered outside this
    // class) match this panel's hierarchy EXACTLY. They had drifted into
    // ad-hoc ImGui::Text labels and inconsistent top spacing, which read as
    // the params "jumping" when switching tabs.
    //   PanelSectionHeader: the uppercase H2 section title + collapse chevron.
    //                       Pass firstSection=true for the top title (no extra
    //                       top padding) so every tab's first title lands at the
    //                       same Y. Returns the open/expanded state.
    //   PanelLabel        : the dim left-gutter label that parks its control at
    //                       the shared column. Returns the control-column width.
    static bool  PanelSectionHeader(const char* label, bool firstSection = false);
    static float PanelLabel(const char* text);

    // Canonical right-dock panel style (window padding / item spacing / frame
    // padding+rounding). EVERY parameter panel must Push this before its
    // ImGui::Begin() and Pop it after — otherwise each panel Begin()s with a
    // different WindowPadding (Properties 18, Mapping 22, Sources/Audio the
    // global default) and the first title lands a few px off, which reads as a
    // small vertical "jump" when switching tabs.
    static void PushPanelStyle();   // call immediately BEFORE Begin()
    static void PopPanelStyle();    // call immediately AFTER Begin()

    // Stage hookups — when set, the panel renders a Stage Setup section
    // (displays / projectors / surfaces) at the top of the Properties
    // window when the workspace mode is Stage.
    void setStageView(StageView* sv) { m_stageView = sv; }
    void setZoneTextures(const std::vector<unsigned int>* z) { m_zoneTexs = z; }
    // Phase C: timeline pointer enables the keyframe-diamond affordance next
    // to animatable parameters. Optional — diamond is only drawn when set.
    void setTimeline(Timeline* tl) { m_timeline = tl; }

    // Layer-nav hookup — lets the top of the parameters panel surface the
    // LAYERS header, the "+ Add New Layer" action, and a current-layer
    // selector WITHOUT duplicating logic: the add-layer action just trips
    // the SAME LayerPanel signal flags Application already consumes, and
    // selection drives the SAME shared selected-layer index. Optional.
    void setLayerNav(LayerPanel* lp, int* selectedLayer) {
        m_layerPanel = lp; m_selectedLayer = selectedLayer;
    }

    // UIManager hookup — needed so the pinned source-quickbar at the top
    // of the panel can switch the Sources dock tab. Optional; without it
    // the strip falls back to a non-interactive display.
    void setUIManager(UIManager* ui) { m_uiManager = ui; }

    // Set to true when a property widget is first activated (signals Application to push undo state)
    bool undoNeeded = false;
private:
    StageView* m_stageView = nullptr;
    const std::vector<unsigned int>* m_zoneTexs = nullptr;
    Timeline* m_timeline = nullptr;
    LayerPanel* m_layerPanel = nullptr;
    int* m_selectedLayer = nullptr;
    UIManager* m_uiManager = nullptr;
};
