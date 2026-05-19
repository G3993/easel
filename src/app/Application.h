#pragma once
#include "app/OutputZone.h"
#include "app/MappingProfile.h"
#include "app/ProjectorOutput.h"
#include "app/UndoStack.h"
#include "compositing/CompositeEngine.h"
#include "compositing/LayerStack.h"
#include "compositing/MaskRenderer.h"
#include "warp/CornerPinWarp.h"
#include "warp/MeshWarp.h"
#include "render/Framebuffer.h"
#include "render/ShaderProgram.h"
#include "render/Mesh.h"
#include "render/Texture.h"
#include "ui/UIManager.h"
#include "ui/ViewportPanel.h"
#include "ui/LayerPanel.h"
#include "ui/PropertyPanel.h"
#include "ui/WarpEditor.h"
#include "ui/TransitionCatalog.h"
#include "voice/VoiceCommand.h"
#ifdef __APPLE__
#include "voice/MacSpeechRecognizer.h"
#endif
#include <unordered_map>
#include <deque>

#ifdef _WIN32
#include "sources/WindowCaptureSource.h"
#elif defined(__APPLE__)
#include "sources/WindowCaptureSource_mac.h"
#endif
#include "sources/ShaderSource.h"
#include "sources/ShaderClawBridge.h"
#include "sources/ShaderRatings.h"
#include "sources/ParticleSource.h"

#ifdef HAS_OPENCV
#include "scanning/SceneScanner.h"
#include "scanning/WebcamSource.h"
#include "scanning/ScanPanel.h"
#endif

#ifdef HAS_WHISPER
#include "speech/WhisperSpeech.h"
#endif

#include "speech/EthereaClient.h"
#include "speech/CueClient.h"
#include "app/AudioAnalyzer.h"
#include "app/AudioMixer.h"
#include "app/BPMSync.h"
#include "app/SceneManager.h"
#include "app/OSCManager.h"
#include "timeline/Timeline.h"
#include "app/MIDIManager.h"
#include "app/DataBus.h"
#include "stage/StageView.h"

#ifdef HAS_NDI
#include "sources/NDISource.h"
#include "app/NDIOutput.h"
#endif

#ifdef HAS_SPOUT
#include "app/SpoutIO.h"
#endif

#ifdef HAS_WHEP
#include "sources/WHEPSource.h"
#endif

#ifdef HAS_FFMPEG
#include "app/RTMPOutput.h"
#include "app/VideoRecorder.h"
#endif

#include <GLFW/glfw3.h>
#include <string>
#include <vector>
#include <atomic>
#include <thread>

class Application {
public:
    bool init();
    void run();
    void shutdown();

private:
    GLFWwindow* m_window = nullptr;
    int m_windowWidth = 1280, m_windowHeight = 720;

    UIManager m_ui;
    TransitionCatalog m_transitionCatalog;
    std::unordered_map<std::string, float> m_animatedParams; // Phase C lane runtime cache

    // V1 voice-driven timeline. Push-to-talk via UI for now; SFSpeech wrapper
    // arrives in V1.1. handleVoiceIntent() dispatches recognized phrases to
    // the existing timeline / layer / shader APIs so anyone can drive the
    // tool with their voice instead of clicks.
    void handleVoiceIntent(const easel::voice::VoiceIntent& intent);
    void renderVoiceCommandBar();
    void startVoiceRecording();
    void stopVoiceRecording();
    char m_voiceTextInput[256] = "";
    std::deque<std::string> m_voiceLog;        // last N transcripts (recent first)
    std::string m_voiceLastEcho;               // formatted intent for the pill
    double m_voiceLastEchoTime = 0.0;
    bool m_voiceBarOpen = true;                // command bar visibility
    std::string m_voicePartial;                // streaming partial transcript while listening
    bool m_voiceListening = false;             // mic gate (push-to-talk)
    bool m_voiceContinuous = true;             // when true, mic stays on; auto-restart after every final
    bool m_voiceRestartPending = false;        // set in onFinal; main loop tears down + restarts
#ifdef __APPLE__
    MacSpeechRecognizer m_voiceRecognizer;
#endif
    ViewportPanel m_viewportPanel;
    LayerPanel m_layerPanel;
    PropertyPanel m_propertyPanel;
    WarpEditor m_warpEditor;

    // Mapping profiles (warp + edge blend, independent of zones)
    std::vector<std::unique_ptr<MappingProfile>> m_mappings;
    MappingProfile* mappingForZone(OutputZone& z);


    // Output zones (replaces singular compositor/warp/FBO)
    std::vector<std::unique_ptr<OutputZone>> m_zones;
    int m_activeZone = 0;
    int m_prevActiveZone = 0;
    uint32_t m_nextLayerId = 1;
    OutputZone& activeZone() {
        if (m_activeZone < 0 || m_activeZone >= (int)m_zones.size())
            m_activeZone = 0;
        return *m_zones[m_activeZone];
    }

    LayerStack m_layerStack;
    std::unordered_map<int, std::unique_ptr<ProjectorOutput>> m_projectors;
    MaskRenderer m_maskRenderer;
    UndoStack m_undoStack;

    Mesh m_quad;
    ShaderProgram m_passthroughShader;
    ShaderProgram m_edgeBlendShader;
    Framebuffer m_edgeBlendFBO;
    Framebuffer m_maskPingPongFBO; // second FBO for multi-mask ping-pong
    Texture m_testPattern;

    // Phase Q v4 — bloom pipeline. Half-res FBOs ping-pong for the
    // separable Gaussian; uCompositeFBO holds the screen-blended
    // result before it's copied back to warpFBO.
    ShaderProgram m_bloomBrightShader;
    ShaderProgram m_bloomBlurShader;
    ShaderProgram m_bloomCompositeShader;
    ShaderProgram m_linearCopyShader;     // bloom copy-back, no ACES
    ShaderProgram m_warpDownsampleShader; // 4-tap explicit-offset SS → 1× downsample
    Framebuffer   m_bloomBrightFBO;       // half-res, 16F
    Framebuffer   m_bloomPingPongFBO[2];  // half-res, 16F
    Framebuffer   m_bloomCompositeFBO;    // full-res, 16F
    bool          m_bloomEnabled   = true;
    float         m_bloomThreshold = 0.85f;
    float         m_bloomKnee      = 0.30f;
    float         m_bloomStrength  = 0.55f;
    float         m_bloomTint      = 0.40f;
    int           m_bloomBlurPasses = 2;  // 1..6 — more = wider, softer halo

    int m_selectedLayer = -1;
    AudioAnalyzer m_audioAnalyzer;
    AudioMixer m_audioMixer;
    bool m_mixerEnabled = false;        // false = legacy single-device mode
    int m_mixerOutputDevice = -1;       // -1 = default output
    BPMSync m_bpmSync;
    SceneManager m_sceneManager;
    Timeline m_timeline;
    OSCManager m_oscManager;
    MIDIManager m_midiManager;
    // Latest normalized MIDI CC values, indexed [channel][cc]. Updated each frame from polled events.
    float m_midiCCValues[16][128] = {};
    StageView m_stageView;
    float m_audioRMS = 0; // backward compat: smoothed audio level
    int m_mosaicAudioDevice = -1; // -1 = system loopback, >=0 = index into device list
    bool m_projectorAutoConnect = false;
    int m_lastMonitorCount = 0;
    bool m_maskEditMode = false;

    // Show-workspace preview zoom. Multiplier applied to the centered
    // live-output preview's height (then width follows aspect). 1.0 = fit
    // (default). Driven by `+`/`-`/Fit toolbar buttons and Cmd+=, Cmd+-,
    // Cmd+0 keyboard shortcuts (handled in renderUI before the Show panel).
    float m_showZoom = 1.0f;

    // Editor fullscreen toggle (F11)
    bool m_editorFullscreen = false;
    bool m_pendingExitFullscreen = false;
    // Async close — blocking cleanup runs on a background thread while
    // the main thread renders a spinner so the window stays responsive.
    std::atomic<bool> m_closing{false};
    std::atomic<bool> m_closingDone{false};
    std::atomic<int>  m_closingStep{0};
    std::thread       m_closingThread;
    int m_savedWindowX = 0, m_savedWindowY = 0;
    int m_savedWindowW = 1280, m_savedWindowH = 720;

    // Splash screen shown immediately on launch
    bool   m_showSplash = true;
    double m_splashStartTime = 0.0;
    void   renderSplash();

    // Landing page shown after splash dismisses
    bool m_showLanding = false;
    int  m_landingSubView = 0; // 0=main 1=templates 2=recent
    std::vector<std::string> m_recentProjects;
    void renderLandingPage();
    void loadRecentProjectsList();
    void addRecentProject(const std::string& path);

    void updateSources();
    void compositeAndWarp();
    void compositeZone(OutputZone& zone);
    void presentOutputs();
    void renderReadbackFBO(OutputZone& zone);
    void renderUI();
    void renderMenuBar();
    // Inline brand mark + overflow menu drawn at the start of the workspace
    // nav row. Replaces the standalone main menu bar so we have ONE chrome
    // row instead of two stacked ones.
    void renderNavBarPrefix();
    void toggleEditorFullscreen();
#ifdef HAS_FFMPEG
    void renderGoLiveButton();
#endif

    // Inline (no Begin/End) section renderers — called from inside their host
    // tab so each control lives with the workflow step it belongs to.
    void renderStageInlineSetup(OutputZone& zone);   // Composition + Output, rendered in Stage tab
    void renderEdgeBlendInline(OutputZone& zone);    // Edge blend, rendered in Masks tab
    void addZone();
    void removeZone(int index);
    void duplicateZone(int index);
    void setupMultiGPUProjection(const std::vector<std::string>& ndiSourceNames);
    void loadImage(const std::string& path);
    void loadVideo(const std::string& path);
#if defined(_WIN32) || defined(__APPLE__)
    void addScreenCapture(int monitorIndex);
#endif
    void addParticles();
#ifdef HAS_OPENCV
    void addWebcam(int cameraIndex);
#endif
#ifdef _WIN32
    void addWindowCapture(HWND hwnd, const std::string& title);
#elif defined(__APPLE__)
    void addWindowCapture(uint32_t windowID, const std::string& title);
#endif
    void loadShader(const std::string& path);
    void registerLayerWithZones(uint32_t layerId);

#if defined(_WIN32) || defined(__APPLE__)
    std::vector<WindowInfo> m_windowList;
#endif
    ShaderClawBridge m_shaderClaw;
    ShaderRatings    m_shaderRatings;

    // ShaderClaw thumbnail preview (animated on hover)
    std::shared_ptr<ShaderSource> m_scPreview;
    std::string m_scPreviewPath;
    int m_scPreviewFrame = 0;

    // ShaderClaw static thumbnail cache (one per shader)
    struct SCThumbEntry {
        std::shared_ptr<Texture> texture;
        bool ready = false;
    };
    std::unordered_map<std::string, SCThumbEntry> m_scThumbnails;
    std::shared_ptr<ShaderSource> m_scThumbRenderer; // temp shader for generating thumbnails
    std::string m_scThumbRenderPath; // currently rendering thumbnail for this shader
    int m_scThumbRenderFrame = 0;

#ifdef HAS_OPENCV
    SceneScanner m_scanner;
    WebcamSource m_webcam;
    ScanPanel m_scanPanel;
#endif

    SpeechState m_speechState;
    EthereaClient m_ethereaClient;
    CueClient m_cueClient;
    DataBus m_dataBus;
    std::string m_prevTranscript;       // Last full_transcript for diffing

    // Voice decay: fade text layers after speech stops (matches Shader-Claw 3)
    double m_voiceLastInputTime = 0.0;  // glfwGetTime() of last transcript update
    float m_voiceDecayHold = 2.0f;      // seconds at full opacity after last input
    float m_voiceDecayDuration = 3.0f;  // seconds to fade out after hold
    bool m_voiceDecayEnabled = true;
#ifdef HAS_WHISPER
    WhisperSpeech m_whisperSpeech;
#endif

#ifdef HAS_NDI
    NDIOutput m_ndiOutput;
    NDIFinder m_ndiFinder;
    std::vector<NDISenderInfo> m_ndiSources;
    bool m_ndiOutputEnabled = true;
    void addNDISource(const std::string& senderName);
#endif

#ifdef HAS_SPOUT
    EaselSpoutSender m_spoutOutput;
    bool m_spoutOutputEnabled = false;
#endif

#ifdef HAS_WHEP
    char m_whepUrlBuf[512] = {};
    std::shared_ptr<WHEPSource> m_whepConnecting; // tracks in-progress connection
    std::string m_whepStatus;
    void addWHEPSource(const std::string& whepUrl);
    void addScopeRTMP();
#endif

    int m_selectedAudioDevice = -1; // -1 = default loopback

    // Timeline state — always present so the render loop and UI code can
    // reference these unconditionally regardless of FFmpeg availability.
    void renderTimelinePanel();
    void renderFloatingTransportPill();
    void renderFloatingActionPills();
    void startTimelineExport();
    bool   m_timelineExporting = false;
    double m_timelineExportEnd = 0.0;
    std::string m_timelineExportPath;
    bool   m_timelineOpen = true;
    bool   m_timelineMinimized = false;

    // ── Single source of truth for the animated timeline geometry ──────────
    // The timeline + bottom transport bar slide up/down as one unit. These
    // are recomputed once per frame in updateTimelineAnim() (called at the
    // top of renderUI, before any panel renders) so Fixes 1/2/3 all read the
    // SAME numbers in the same frame:
    //   m_timelineAnimT  : eased open factor [0..1] (0 = hidden, 1 = open)
    //   m_timelineCurH   : current animated timeline height in px (0 when
    //                      minimized). Bottom nav slides up by this much.
    //   m_timelineTopY   : screen-space Y of the timeline's top edge — the
    //                      hard bottom limit for the params panel + the
    //                      left-rail layer thumbnails.
    float  m_timelineAnimT = 1.0f;
    float  m_timelineCurH  = 0.0f;
    float  m_timelineTopY  = 0.0f;
    // Target (fully-open) timeline content height, measured by
    // renderTimelinePanel() and consumed next frame by updateTimelineAnim().
    float  m_timelineTargetH = 220.0f;
    void   updateTimelineAnim();

#ifdef HAS_FFMPEG
    RTMPOutput m_rtmpOutput;
    char m_streamKeyBuf[128] = {};
    int m_streamAspect = 0; // 0=16:9, 1=4:3, 2=16:10, 3=Source
    VideoRecorder m_recorder;
    std::vector<RecAudioDevice> m_audioDevices;
    std::vector<RecAudioDevice> m_outputDevices; // render devices for mixer output
    void renderTransportBar();

    // Audio level meter (WASAPI IAudioMeterInformation)
    void* m_audioMeterInfo = nullptr;
    void* m_audioMeterDevice = nullptr;
    int m_meterDeviceIdx = -2; // which device the meter is tracking (-2 = uninitialized)
    float m_audioLevelPeak = 0.0f;
    float m_audioLevelL = 0.0f, m_audioLevelR = 0.0f;
    float m_audioLevelSmooth = 0.0f, m_audioLevelSmoothL = 0.0f, m_audioLevelSmoothR = 0.0f;
    void updateAudioMeter();
    void cleanupAudioMeter();
#endif

    // File drop handling
    std::vector<std::string> m_pendingDrops;
    void handleDroppedFiles();
    static void dropCallback(GLFWwindow* window, int count, const char** paths);

    // JSON save/load
    void saveProject(const std::string& path);
    void loadProject(const std::string& path);

    // Screenshot
    void captureScreenshot(const std::string& path);
    void captureWindow(const std::string& path);
    void pollScreenshotTrigger();
};
