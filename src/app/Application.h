#pragma once
#include "app/OutputZone.h"
#include "app/MappingProfile.h"
#include "app/ProjectorOutput.h"
#include "app/UndoStack.h"
#include "app/CameraEnumerator.h"
#include "compositing/CompositeEngine.h"
#include "compositing/LayerStack.h"
#include "compositing/MaskRenderer.h"
#include "warp/CornerPinWarp.h"
#include "warp/MeshWarp.h"
#include "warp/WarpOverlay.h"
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
#include <future>

#ifdef _WIN32
#include "sources/WindowCaptureSource.h"
#elif defined(__APPLE__)
#include "sources/WindowCaptureSource_mac.h"
#endif
#include "sources/ShaderSource.h"
#include "sources/ShaderClawBridge.h"
#include "sources/ShaderRatings.h"
#include "sources/ShaderPresets.h"
#include "app/ShowPresets.h"
#include "sources/ShaderImprover.h"
#include "sources/ParticleSource.h"
#include "sources/MovingCompanySource.h"
#include "sources/FluidSource.h"
#include "sources/FluidSource3D.h"
#include "sources/HologramModelSource.h"
#ifdef __APPLE__
#include "sources/VisionTracker.h"
#endif

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
#include "app/AudioInputDevices.h"
#include "app/DataBus.h"
#include "stage/StageView.h"
#include "app/ProDJLink.h"
#include "ui/TimecodePanel.h"

#include "app/NetAdapters.h"

#ifdef HAS_NDI
#include "sources/NDISource.h"
#include "app/NDIOutput.h"
#include "net/NdiNetworkConfig.h"
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
#include <mutex>
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
    // Per-source rolling transcript state, for the *.recent FIFO + *.words delta.
    struct TranscriptFeed {
        std::string prevSegment;              // baseline for the per-event delta
        std::vector<std::string> finalWords;  // committed words (FIFO history)
    };
    // Route a live transcript segment to three DataBus channels under `prefix`
    // ("cue" or "etherea"):
    //   <prefix>.latest — full current segment (unchanged contract; the whole
    //                     growing utterance, for typewriter/reveal text shaders).
    //   <prefix>.words  — only the words newly appended since the previous
    //                     segment (per-event delta; mirrors etherea-ai's
    //                     unprocessed-tail concept).
    //   <prefix>.recent — a running FIFO of the last m_recentWordCap words that
    //                     slides as you speak (committed words + current segment,
    //                     windowed). This is the live "ticker" feed.
    // `feed` holds per-source state; `isFinal` commits the current segment.
    void pushTranscript(const std::string& prefix, TranscriptFeed& feed,
                        const std::string& text, bool isFinal);
    // Thin Cue-channel wrapper.
    void pushCueWords(const std::string& text, bool isFinal) {
        pushTranscript("cue", m_cueFeed, text, isFinal);
    }
    char m_voiceTextInput[256] = "";
    std::deque<std::string> m_voiceLog;        // last N transcripts (recent first)
    std::string m_voiceLastEcho;               // formatted intent for the pill
    double m_voiceLastEchoTime = 0.0;
    bool m_voiceBarOpen = true;                // command bar visibility
    std::string m_voicePartial;                // streaming partial transcript while listening
    bool m_voiceListening = false;             // mic gate (push-to-talk)
    // Continuous mic: when true the recognizer stays on and auto-restarts
    // after every final. OFF by default and remembered in
    // ~/.easel/show_settings.json ("voiceMic"): an open mic makes macOS draw
    // its orange privacy dot on EVERY display — projectors included, above
    // everything — so the mic is opt-in, not something every launch opens.
    bool m_voiceContinuous = false;
    bool m_voicePrefLoaded = false;
    void setVoiceContinuous(bool on);          // toggle + start/stop + persist
    bool m_voiceRestartPending = false;        // set in onFinal; main loop tears down + restarts
    double m_voiceStartRetryAt = 0.0;          // continuous-mic start backoff (async engine start)
#ifdef __APPLE__
    MacSpeechRecognizer m_voiceRecognizer;
    // Speech.framework delivers onPartial/onFinal on its own dispatch queue.
    // Those callbacks must NOT touch app state — the main thread iterates
    // m_cueFeed / m_layerStack / m_timeline / m_voiceLog every frame with no
    // locking — so they only append here, and drainPendingSpeech() replays
    // the events on the main thread once per frame, in arrival order.
    struct PendingSpeech { std::string text; bool isFinal; };
    std::vector<PendingSpeech> m_pendingSpeech;  // guarded by m_pendingSpeechMutex
    std::mutex m_pendingSpeechMutex;
    void drainPendingSpeech();
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
    // Monotonic composite-pass counter — drives idle release of per-zone
    // scratch FBOs (OutputZone::releaseIdleScratch).
    uint64_t m_compositeFrame = 0;
    int m_prevActiveZone = 0;

    // ── Multi-screen output mode ─────────────────────────────────────
    // Independent: each zone drives its own monitor (the existing path).
    // Spanned:     one wide "span" canvas is sliced left-to-right across
    //              several monitors, so a single visual stretches/moves
    //              across screens. The two are alternatives, toggled live.
    enum class OutputMode { Independent, Spanned };
    OutputMode m_outputMode = OutputMode::Independent;

    // The span canvas: its own compositor / warp / layer-visibility at a
    // user-defined resolution (default 3840x1080 = two side-by-side 1080p
    // screens). Lazily created the first time spanned mode is used.
    std::unique_ptr<OutputZone> m_spanZone;
    int m_spanWidth  = 3840;
    int m_spanHeight = 1080;

    // Horizontal slices of the span canvas, ordered left-to-right. Each slice
    // crops the [u0,u1] sub-rect of the (flat) span canvas, warps it through
    // its OWN mapping profile (independent per-projector corner-pin), then
    // presents fullscreen on one monitor.
    struct SpanSlice {
        int monitor = -1;
        float u0 = 0.0f;
        float u1 = 1.0f;
        int mappingIndex = -1;   // own MappingProfile in m_mappings (-1 until created)
    };
    std::vector<SpanSlice> m_spanSlices;
    // Per-slice GPU targets (parallel to m_spanSlices): cropFBO holds the
    // projector's flat half, warpFBO the corner-pinned result. Kept out of
    // SpanSlice so the struct stays copyable.
    std::vector<Framebuffer> m_spanCropFBO;
    std::vector<Framebuffer> m_spanWarpFBO;
    // Per-projector mapping profiles, kept SEPARATE from m_mappings so the
    // normal Mapping workspace (which enumerates m_mappings) never sees or
    // touches them. SpanSlice.mappingIndex indexes into this vector.
    std::vector<std::unique_ptr<MappingProfile>> m_spanMappings;

    OutputZone& ensureSpanZone();   // create + size the span canvas on demand
    void layoutSpanSlices();        // even left->right split across the slices
    void ensureSpanSliceResources(); // size per-slice FBOs + mapping profiles
    // Shared projector create/retry/backoff helper (used by both the
    // independent per-zone path and the spanned slice path). Returns an
    // active projector for the monitor, or nullptr if it isn't ready yet.
    ProjectorOutput* ensureProjector(int monitorIndex);
    // Target frame-rate cap shown/edited in the Canvas section. 0 = vsync
    // (loop runs at the editor display's refresh, which on a high-refresh
    // monitor is well above 60 and just burns extra GPU/heat for no visual
    // gain). >0 disables vsync and busy-waits after swap to hold the frame
    // to ~1/target sec. Defaults to a real 60fps cap regardless of the
    // editor display's own refresh rate.
    float m_targetFPS = 60.0f;
    // Swap interval currently applied to the main context (-1 = unset). Driven
    // each frame from m_targetFPS so toggling the Canvas Target flips vsync.
    int m_appliedSwapInterval = -1;
    // glfwGetTime() instant until which the PLAY workspace's PUBLISH
    // button shows the "Published" confirmation caption. 0 = no flash.
    double m_publishFlashUntil = 0.0;
    // M3 — live re-publish. Stable per-process show id (so byte-comparison
    // of consecutive publishes detects actual content changes, not just a
    // bumped timestamp). Cached last-sent JSON for the dirty check, and
    // the next time the dirty check should run.
    std::string m_showIdStr;
    std::string m_lastPublishedJson;
    double      m_nextPublishCheckAt = 0.0;
    // M7-11 — After an explicit /cue/clear, suppress any background writes
    // (mic recognizer, cue WS client) to cue.latest for a brief window so
    // an intentional blank doesn't get clobbered by stray room audio.
    // 0 = no active suppression.
    double      m_cueLatestSuppressUntil = 0.0;
    // Per-source rolling transcript state (delta baseline + FIFO history).
    // Touched only from the transcript write path.
    TranscriptFeed m_cueFeed;
    TranscriptFeed m_ethereaFeed;
    // Max words shown in the *.recent rolling feed; user-adjustable (>=1).
    int m_recentWordCap = 7;
    uint32_t m_nextLayerId = 1;
    OutputZone& activeZone() {
        if (m_activeZone < 0 || m_activeZone >= (int)m_zones.size())
            m_activeZone = 0;
        return *m_zones[m_activeZone];
    }

    LayerStack m_layerStack;
    std::unordered_map<int, std::unique_ptr<ProjectorOutput>> m_projectors;
    MaskRenderer m_maskRenderer;
    // MAPPING-workspace gizmo overlay drawn into the warp output itself so
    // control points + edges are visible on the projector while aligning.
    WarpOverlay m_warpOverlay;
    UndoStack m_undoStack;

    Mesh m_quad;
    ShaderProgram m_passthroughShader;
    ShaderProgram m_edgeBlendShader;
    // Scratch FBOs for the post chain (edge blend, mask union, bloom, warp
    // supersample) live per-zone on OutputZone — sharing them across
    // mixed-resolution zones caused a destroy/realloc cycle every frame.
    // Default "nothing loaded" test pattern, rendered live each frame from
    // two real images the user supplied — a broadcast-style test card
    // background (assets/testcard.png) and a duck sprite chroma-keyed out
    // of its own photo (assets/duck.png), which bounces around the frame.
    ShaderProgram m_testPatternShader;
    Framebuffer   m_testPatternFBO;
    Texture       m_testCardTex;
    Texture       m_duckTex;
    Texture m_maskGrid; // white alignment grid shown while a mask is being added/edited

    // MAPPING-workspace calibration patterns (black & white). In Mapping mode
    // the warp source is replaced by one of these so the user aligns geometry
    // to crisp lines instead of live content / the color test pattern. The
    // active one is chosen via the Mapping panel's Test Pattern dropdown
    // (WarpEditor::testPatternIndex). Order MUST match kPatternNames there:
    // 0 Grid, 1 Checkerboard, 2 Crosshair, 3 Concentric Circles, 4 Dots,
    // 5 Solid White.
    static constexpr int kMapPatternCount = 6;
    Texture m_mapPatterns[kMapPatternCount];

    // Phase Q v4 — bloom pipeline. Half-res FBOs ping-pong for the
    // separable Gaussian; uCompositeFBO holds the screen-blended
    // result before it's copied back to warpFBO.
    ShaderProgram m_bloomBrightShader;
    ShaderProgram m_bloomBlurShader;
    ShaderProgram m_bloomCompositeShader;
    ShaderProgram m_linearCopyShader;     // bloom copy-back, no ACES
    ShaderProgram m_warpDownsampleShader; // 4-tap explicit-offset SS → 1× downsample
    // Bloom FBOs are per-zone (OutputZone::bloom*) — see scratch FBO note above.
    // Global post bloom OFF by default — it was glowing every layer/shader and
    // reading as a soft haze. Crisp output is the default; flip on (or raise
    // strength) only when a deliberately glowy finish is wanted.
    bool          m_bloomEnabled   = false;
    float         m_bloomThreshold = 0.85f;
    float         m_bloomKnee      = 0.30f;
    float         m_bloomStrength  = 0.70f;
    float         m_bloomTint      = 0.40f;
    int           m_bloomBlurPasses = 2;  // 1..6 — more = wider, softer halo
    // Global finish (saturation grade + film grain + dither) applied in the
    // bloom composite pass — lifts every layer toward an engine/post-stack look.
    float         m_finishAmount   = 1.0f;

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

    // Canvas-mode knob/fader bank under the main viewport — 3 rows of 8
    // knobs + 8 faders, mirroring a Launch Control XL. Each mirrors its
    // assigned CC's live value and mouse-drags as a fallback until that CC
    // is touched. Pushed into DataBus every frame as "midi.knob1".."midi.knob24"
    // / "midi.fader1".."midi.fader8" so any shader text param can bind to
    // one. The dot next to each "Effect N" label opens a picker that wires
    // the knob straight into a global target (layer opacity/position/scale/
    // rotation/visibility, scene recall, BPM) via MIDIManager's mapping
    // system instead.
    static const int kKnobBankCount = 24;
    float m_knobValue[kKnobBankCount] = {};
    MIDIMapping::Target m_knobTarget[kKnobBankCount];
    int m_knobTargetLayer[kKnobBankCount] = {};
    // Shader-parameter binding (mutually exclusive with m_knobTarget's
    // global MIDIMapping targets) — e.g. a knob bound straight to a
    // shader's "ribbonRadius" ISF input via DataBus. Empty param = none;
    // the actual binding lives in DataBus (m_dataBus.bind), these just
    // remember which layer/param so the picker can show the current
    // selection and clear the old binding on reassignment.
    uint32_t    m_knobBoundLayerId[kKnobBankCount] = {};
    std::string m_knobBoundParam[kKnobBankCount];
    static const int kFaderBankCount = 8;
    float m_faderValue[kFaderBankCount] = {};

    // SHOW-mode deck (Application.cpp: renderShowDeck()) — a self-contained
    // shader mixer docked under the Program/Preview monitors in Show mode.
    // Two slots — A (green) and B (red) — each run their own live ISF
    // shader. m_deckSlider[0] is the A/B crossfader (blends slot A and slot
    // B via gl-transitions "fade" into m_deckMixFBO); the blended, post-fx'd
    // result is PROGRAM (see m_showProgramTex below), which replaces the
    // active zone's normal layer-stack composite — Show's decks are the
    // actual live output, not a page-local-only preview. Left/right
    // thumbnail rails single-click into m_deckPreviewL/R for a preview,
    // double-click to commit into slot B/A.
    static const int kDeckKnobCount   = 12;
    static const int kDeckButtonCount = 4;
    // 7 vertical faders, each a fixed PROGRAM-monitor post-fx (brightness/
    // saturation/contrast/hue-rotate/vmirror/qmirror/kaleidoscope — see
    // showdeck_postfx.frag) rather than a free shader-param binding. Live-
    // mirrored from MIDI faders 2-8 of whatever controller is connected,
    // same convention as the horizontal crossfader below and the Canvas
    // knob bank's kFaderCC.
    static const int kDeckSliderCount = 7;
    std::string m_deckShaderPathA, m_deckShaderPathB;
    std::shared_ptr<ShaderSource> m_deckShaderA, m_deckShaderB;
    float m_deckKnobA[kDeckKnobCount]     = {};
    float m_deckKnobB[kDeckKnobCount]     = {};
    // Memory banks — the 2 button rows x (B+A) are save/recall slots for a
    // named snapshot of the 12 knob positions, per user request. A memory
    // is just a name + 12 floats, kept in one shared list so any memory can
    // be recalled into either side's knob bank; each slot below is just an
    // index into that list (-1 = empty, shows a "+" to save the current
    // knobs as a new memory there).
    struct DeckKnobMemory {
        std::string name;
        float knobs[kDeckKnobCount] = {};
    };
    std::vector<DeckKnobMemory> m_deckKnobMemories;
    int m_deckMemSlotB[kDeckButtonCount]  = { -1, -1, -1, -1 };
    int m_deckMemSlotA[kDeckButtonCount]  = { -1, -1, -1, -1 };
    int m_deckMemSlotB2[kDeckButtonCount] = { -1, -1, -1, -1 };
    int m_deckMemSlotA2[kDeckButtonCount] = { -1, -1, -1, -1 };
    // Transient state for the two memory-bank popups (name-new and
    // pick-existing) — only one of either can be open at a time, so this is
    // shared across all 16 slots rather than per-slot.
    int*  m_deckMemPendingSlots = nullptr;
    int   m_deckMemPendingIdx   = -1;
    float m_deckMemPendingKnobs[kDeckKnobCount] = {};
    char  m_deckMemNameBuf[64] = {};
    int*  m_deckMemPickSlots = nullptr;
    int   m_deckMemPickIdx   = -1;
    // 0.5 default for Brightness/Saturation/Contrast (identity slots 0-2)
    // matches their neutral midpoint on the 3-point curves (extreme low ..
    // unity .. extreme high); the rest default to 0.
    float m_deckSlider[kDeckSliderCount]  = { 0.5f, 0.5f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f };
    // Which global effect (index into kGlobalEffectNames, Application.cpp)
    // each physical fader drives — identity by default, i.e. today's fixed
    // Bright/Sat/Contrast/... layout. Reassignable via the small cog next
    // to each fader's caption.
    int m_deckFaderEffect[kDeckSliderCount] = { 0, 1, 2, 3, 4, 5, 6 };
    // Per-knob shader-parameter binding, keyed by ISFInput::name — empty =
    // unassigned (the knob has no effect). Knobs are split into full A/B
    // arrays above, so each gets its own bound-name array.
    std::string m_deckKnobBoundParamA[kDeckKnobCount];
    std::string m_deckKnobBoundParamB[kDeckKnobCount];
    // PARAMETERS panel's Auto-Map switch, per slot — knobs no longer
    // auto-map the instant a shader loads (that kept clobbering hand-set
    // assignments); flipping this on re-derives all 12 from scratch instead.
    bool m_deckAutoMapOnA = false, m_deckAutoMapOnB = false;
    // Per-knob physical MIDI CC override, set via "Learn MIDI CC" in the
    // knob's dot popup — -1 (the init() default) means "use the
    // position-based Launch Control XL factory CC" (kShowKnobCC in
    // renderShowDeck); >=0 means the user explicitly mapped a physical
    // control onto this knob, for a controller that isn't on the factory
    // template (or isn't a Launch Control XL at all) and so doesn't match
    // the assumed layout.
    int m_deckKnobCCOverrideA[kDeckKnobCount];
    int m_deckKnobCCOverrideB[kDeckKnobCount];
    // Which knob (if any) is currently waiting to capture the next MIDI CC
    // via Learn MIDI CC — -1/-1 = not learning. side: 0 = B, 1 = A.
    int m_deckKnobLearnSide = -1, m_deckKnobLearnIdx = -1;
    // Horizontal A/B crossfader — separate from the effect sliders above.
    // 0 = full B (left), 1 = full A (right). Defaults all the way to B
    // (left), per user request, rather than centered. Live-mirrored from
    // MIDI fader 1. m_deckBlendMode indexes kShowDeckBlendModes
    // (fade/cut/channel-key).
    float m_deckCrossfade = 0.0f;
    int   m_deckBlendMode = 0;
    // "Fade" duration shown/edited next to the blend-mode dropdown, per the
    // reference mockup. Drives the animated auto-crossfade double-clicking
    // the crossfader track triggers (animates from the current position to
    // the opposite end over this many seconds) — see m_deckCrossfadeAnim*.
    float m_deckCrossfadeDuration = 2.0f;
    bool  m_deckCrossfadeAnimating = false;
    float m_deckCrossfadeAnimFrom = 0.0f, m_deckCrossfadeAnimTo = 1.0f, m_deckCrossfadeAnimT = 0.0f;
    Framebuffer m_deckMixFBO;
    // PROGRAM-monitor post-fx pass (showdeck_postfx.frag) driven by the 7
    // effect sliders above.
    ShaderProgram m_deckPostFxShader;
    Framebuffer   m_deckProgramFxFBO;
    // Kaleidoscope trails — a simple feedback loop (ping-pong FBOs, blended
    // via the same "fade" gl-transition used by the A/B crossfader) that
    // gives the kaleidoscope effect motion-trail echoes, Resolume-style.
    // On by default; toggled off via the "Trails" checkbox in the
    // Kaleidoscope fader's reassign popup.
    bool        m_deckKaleidoTrails = true;
    Framebuffer m_deckTrailFBO[2];
    int         m_deckTrailIdx = 0;
    bool        m_deckKaleidoWasActive = false;
    // What PROGRAM is showing right now, published by renderShowDeck() each
    // frame it runs and read back by compositeZone() to replace the active
    // zone's normal layer-stack composite with it — this is what actually
    // routes Show's B/A mix out to the viewport, present/fullscreen output,
    // and any NDI/Spout/RTMP consumers of the active zone. Zero when
    // neither deck slot has a shader loaded, which lets compositeZone fall
    // straight back to the normal layer-stack composite.
    GLuint m_showProgramTex  = 0;
    int    m_showProgramTexW = 0, m_showProgramTexH = 0;
    std::string m_deckPreviewPathL, m_deckPreviewPathR;
    std::shared_ptr<ShaderSource> m_deckPreviewL, m_deckPreviewR;
    // Named subsets of the shader catalog, filterable per thumbnail rail —
    // lets a user narrow "all shaders" down to a curated set for a show.
    // Persisted to ~/.easel/show_deck_playlists.json (shared across
    // projects, like the shader-thumbnail cache).
    struct DeckPlaylist {
        std::string name;
        std::vector<std::string> shaderPaths;   // ShaderEntry::fullPath values
    };
    std::vector<DeckPlaylist> m_deckPlaylists;
    bool m_deckPlaylistsLoaded = false;
    // -1 = "All Shaders" (unfiltered); >=0 indexes m_deckPlaylists. Per rail
    // (0 = left/B, 1 = right/A) so each side can browse independently.
    int  m_deckPlaylistSel[2] = { -1, -1 };
    // Free-text filter typed above the playlist dropdown, per rail — narrows
    // the thumbnail grid to shaders whose name contains the query.
    char m_deckSearchQuery[2][64] = { { 0 }, { 0 } };
    void loadDeckPlaylists();
    void saveDeckPlaylists();
    void renderShowDeck(float width, float height, GLuint programTex,
                        int programTexW, int programTexH);

    StageView m_stageView;
    ProDJLink m_prodjlink;
    TimecodePanel m_timecodePanel;
#ifdef __APPLE__
    // MediaPipe-style body/hand/face tracking via Apple Vision. Toggled
    // from the Camera tab; feeds the DataBus vision.* keys each frame.
    VisionTracker m_visionTracker;
#endif
    float m_audioRMS = 0; // backward compat: smoothed audio level
    // Global "Audio -> Shaders" switch. When true, the full AudioFeatures bus is
    // fed to every shader's GLSL uniforms; when false, shaders get a neutral
    // (zeroed) bus — the panic off-switch that the old "feed zeros" decision
    // provided. The explicit per-param audio-binding system is independent.
    bool m_audioToShaders = true;
    // Auto-connect the first MIDI controller so it "just works" without opening
    // it in the MIDI panel. Set true once the user explicitly picks "None",
    // so we stop forcing a reconnect.
    bool m_midiUserDisconnected = false;
    int m_mosaicAudioDevice = -1; // -1 = system loopback, >=0 = index into device list
    bool m_projectorAutoConnect = false;
    int m_lastMonitorCount = 0;
    bool m_maskEditMode = false;
    bool m_showTimecodeWindow = false; // Show-mode: Timecode window visible

    // Show-workspace preview zoom. Multiplier applied to the centered
    // live-output preview's height (then width follows aspect). 1.0 = fit
    // (default). Driven by `+`/`-`/Fit toolbar buttons and Cmd+=, Cmd+-,
    // Cmd+0 keyboard shortcuts (handled in renderUI before the Show panel).
    float m_showZoom = 1.0f;

    // App fullscreen toggle (F11) — borderless, editor UI STAYS visible.
    bool m_editorFullscreen = false;
    // Presentation mode (Shift+F11) — draws the active zone's OUTPUT
    // fullscreen with NO editor UI. Kept independent of m_editorFullscreen so
    // "app fullscreen" (UI visible) and "present output" (UI hidden) are
    // separate states. Esc steps out of present first, then out of fullscreen.
    bool m_presentMode = false;
    // Set to true when Esc is pressed in fullscreen — actual exit runs
    // at the TOP of the next frame so AppKit has time to settle the
    // window-style transition without racing the GL/ImGui dock setup.
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

    // Landing page shown after splash dismisses — skipped when the default
    // project auto-loaded at startup (live/agent workflow restores the show
    // across restarts; the landing page must not cover it).
    bool m_showLanding = false;
    bool m_autoLoadedProject = false;
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
    void renderReadbackFBO(OutputZone& zone, Framebuffer& target, int width, int height);
    void renderUI();
    void renderMenuBar();
    // Inline brand mark + overflow menu drawn at the start of the workspace
    // nav row. Replaces the standalone main menu bar so we have ONE chrome
    // row instead of two stacked ones.
    void renderAudioSettingsMenu();   // hamburger-menu "Audio Settings" submenu (mic input picker)
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
    // Decodes an add/duplicate/remove request a zone-tab strip encoded into
    // m_activeZone as a negative "signal" (see ViewportPanel's zone tabs).
    // Must run before ANYTHING else touches m_activeZone this frame —
    // activeZone() clamps a negative value to 0 on read, which silently
    // destroys an unconsumed signal. Called at the very top of run()'s loop
    // (before compositeAndWarp()) so a signal set by any zone-tab strip,
    // in any workspace mode, survives to be consumed here; also called
    // again inside renderUI() so Canvas's own tabs (rendered later in the
    // same frame) still act immediately rather than waiting a frame.
    void consumeZoneTabSignal();
    void setupMultiGPUProjection(const std::vector<std::string>& ndiSourceNames);
    void loadImage(const std::string& path);
    void loadVideo(const std::string& path);
#if defined(_WIN32) || defined(__APPLE__)
    void addScreenCapture(int monitorIndex);
#endif
    void addParticles();
    void addFluid();
    void addFluid3D();
    void addHologramModel(const std::string& path = ""); // upload 3D model → glitchy hologram; empty path opens the picker
    void addMovingCompany();   // F-117 flying through space (mesh source)
#ifdef HAS_OPENCV
    void addWebcam(int cameraIndex, const std::string& deviceName = std::string());
#endif
#if defined(__APPLE__) || (defined(_WIN32) && defined(HAS_OPENCV))
    // Cached device list (AVFoundation on Mac, Media Foundation on Windows)
    // for the camera pickers; index order matches OpenCV VideoCapture
    // indices. Rescans at most every 2s (so hot-plugged capture cards
    // appear) unless force is set.
    void refreshCameraDevices(bool force = false);
    std::vector<CameraDeviceInfo> m_cameraDevices;
    double m_cameraDevicesLastScan = -1.0;
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
    ShaderPresets    m_shaderPresets;
    ShaderImprover   m_shaderImprover;

    // Saved show playlists (shader list + per-shader param snapshots),
    // persisted app-wide. m_showParamOverrides holds the ACTIVE show's param
    // snapshots keyed by full shader path — applied (after per-shader
    // presets) whenever the timeline hot-loads a clip source, and to the
    // first shader startShow() loads directly.
    ShowPresets m_showPresets;
    std::unordered_map<std::string, nlohmann::json> m_showParamOverrides;

    // Build + start a show: one layer PER picked shader, each on its own
    // timeline track, clips staggered so consecutive shaders overlap by the
    // fade window, with a cross-layer "fade" transition on every seam. Each
    // shader stays independently editable (its own layer, its own params).
    // Shared by the gallery's START SHOW button and the SHOWS preset menu.
    void startShow(const std::vector<std::string>& sel, float minutes);
    // Remove every show layer/track/transition built by startShow.
    void teardownShow();

    // Push Further (AI shader improve) — styled top-level panel state.
    bool        m_pushOpen = false;
    std::string m_pushPath, m_pushTitle, m_pushFile;
    char        m_pushInstr[512] = {};
    int         m_pushCombine = 0;   // 0 = none; else index+1 into shaders()
    void        renderPushFurtherPanel();

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
    // Background thumbnail generation, independent of the Sources panel —
    // that panel's own generator above only runs while it's visible, so a
    // shader with no cached thumb yet (e.g. a freshly added one) never got
    // one otherwise, per user request. Same render-to-PNG approach, just
    // driven every frame in updateSources() regardless of what's open.
    std::shared_ptr<ShaderSource> m_bgThumbRenderer;
    std::string m_bgThumbRenderPath;
    int m_bgThumbRenderFrame = 0;

    // GALLERY — fullscreen shader gallery overlay: every shader as a big
    // thumbnail tile in one scrollable page. Opened from the top-nav
    // GALLERY tab (UIManager::sGalleryOpen); Esc closes. Click adds a
    // shader; right-click = Duplicate / Push Further / Delete Permanently;
    // BUILD SHOW turns a selection into a rotating shader playlist.
    void renderShaderGallery();

    // SHOW — fullscreen performance page (top-nav SHOW tab): the live output
    // preview above a Launch Control XL surface whose knobs/faders animate
    // with the hardware. AUTO-MAP binds the whole surface to the target
    // shader's float params through the standard AudioBinding MidiCC path.
    // The XL's bottom button rows (and the drawn buttons) pick which shader
    // layer the surface drives; a fresh target auto-maps on arrival.
    void renderShowPanel();
    void renderSettingsWindow();
    std::string m_recError;
    std::vector<Layer*> showShaderSlots();
    // A SHOW-bindable source: shaders, 2D fluid, 3D fluid, particles.
    struct ShowParam { std::string name, label; float cur, lo, hi; };
    std::map<std::string, AudioBinding>* showBindingsFor(ContentSource* src);
    std::vector<ShowParam> showParamsFor(ContentSource* src);
    void autoMapShaderToLCXL(ContentSource* src);
    void selectShowTarget(int slot);
    Layer* resolveShowTarget();
    // bank 0 = faders, 1-3 = knob rows top to bottom, in the active layout.
    const int* lcxlBank(int idx);
    // ── SHOW decks: two managed shader layers (A/B) under a crossfader ──
    float       m_deckBlend = 0.0f;         // 0 = A on screen, 1 = B
    int         m_deckTarget = 0;           // which deck the XL knobs drive
    BlendMode   m_deckBlendModes[2] = {BlendMode::Normal, BlendMode::Normal};   // per deck: how it sits over the other when on top
    int         m_showLibraryDeck = -1;
    bool        m_deckGLLive = false;      // a gl-transition is live on the decks this frame (GO or the hand fader)
    int         m_deckGLIn = 1;            // the deck riding IN through it
    float       m_deckGLProgress = 0.0f;     // SHOW page: library open in the centre card, cueing into this deck (-1 = closed)
    std::string m_deckTransition = "fade";  // "cut" | "fade" | gl-transition name
    float       m_deckTransSecs = 1.5f;
    bool  m_deckAnim = false, m_deckAnimGL = false;
    float m_deckAnimFrom = 0.0f, m_deckAnimTo = 0.0f, m_deckAnimT = 0.0f, m_deckAnimDur = 1.0f;
    float m_deckFaderPrev = -1.0f;          // XL fader 8 = crossfader; last seen value
    Layer* deckLayer(int deck);
    int    deckLayerIndex(int deck);
    void   deckLoad(int deck, const std::string& path, const std::string& title);
    void   deckEnsureRouted(int deck);     // a deck must reach the active zone's output
    void   deckGo();
    void   takeOverFromShowGlobal();
    void   deckTick(float dt);
    void   deckTakeSurface(int deck, bool pin);
    bool   m_deckTargetPinned = false;   // false = knobs follow whatever is on air
    // ── show extras: bindable layers that are NOT decks (3D Fluid, a shader
    // added from the panel, particles…). They play alongside the decks, get a
    // track in a running show, and can take the knobs like a deck can.
    uint32_t m_showExtraTargetId = 0;          // an extra currently holding the knobs
    std::vector<uint32_t> m_showExtraTrackIds; // tracks a show added for extras (layers stay)
    std::vector<Layer*> showExtraLayers();
    void showTakeExtra(Layer* l);
    void showReconcileExtras();
    void resetShowParams(ContentSource* src);
    void layerToFront(Layer* l);   // the layer becomes the top of the stack (the output)
    void decksToFront();           // both decks on top, order kept — the crossfade is the output
    int    m_showTab = 0;                // 0 KNOBS · 1 TRANSITION · 2 TIMELINE
    int    m_deckXfadeCC = -1;           // learned crossfader CC (-1 = XL fader 8)
    bool   m_deckXfadeLearn = false;     // next CC that moves becomes the crossfader
    bool   m_deckXfadeLoaded = false;
    int    deckXfadeCC();
    void   deckXfadeSave();
    void   deckShowSync();               // a running show drives the crossfader from its fades
    bool   m_deckHand = false;           // the fader was grabbed mid-show: hand mix until the next fade
    uint32_t m_showMixTrId = 0;          // the fade currently steering the crossfader
    float  m_showMixFrom = 0.0f;         // where the fader stood when that fade began
    ContentSource* m_deckSrcSeen[2] = {nullptr, nullptr};  // re-map the knobs when a deck swaps shader

    // ── shader usage history (~/.easel/shader_usage.json) ──
    struct ShaderUsage { int loads = 0; double seconds = 0.0; std::string last; };
    std::map<std::string, ShaderUsage> m_shaderUsage;
    bool   m_usageLoaded = false;
    double m_usageDirtyT = -1.0;   // wall time of the first unflushed change
    void usageNoteLoad(const std::string& path);
    void usageTick(float dt);
    void usageFlush(bool force);

    uint32_t m_showTargetLayerId = 0;   // 0 = automatic (selected / topmost)
    int m_showMapPage = 0;              // which page of 32 params is on the XL
    bool m_showLCXLMk3      = false;    // which XL generation the surface speaks
    bool m_showLCXLDetected = false;    // auto-detect ran (from device names)

    // Gallery shader show: built as REAL timeline clips (one layer + one
    // track per picked shader, cross-layer fades between) — the timeline
    // renders and drives it; these remember the show layers for STOP/rebuild.
    uint32_t m_showLayerId = 0;                 // legacy single-layer shows
    std::vector<uint32_t> m_showLayerIds;       // stacked per-shader layers
    bool     m_showRunning = false;

#ifdef HAS_OPENCV
    SceneScanner m_scanner;
    WebcamSource m_webcam;
    ScanPanel m_scanPanel;

    // Dedicated webcam capture for Show-deck shader Image inputs (see
    // renderShowDeck's PARAMETERS panel). Deliberately separate from
    // m_webcam above (the Scene Scanner's own camera handle, which the
    // scanner opens/closes/reads on its own schedule) so Show works without
    // an active scan and doesn't fight it for the same capture session.
    // Shared by both A and B deck slots — one physical camera either deck
    // can bind an Image input to, same as one Canvas webcam layer can feed
    // multiple shaders there.
    std::shared_ptr<WebcamSource> m_deckWebcam;
    // Which cv::VideoCapture index m_deckWebcam currently has open (-1 =
    // none yet). Shown/picked by name via m_cameraDevices (see below) —
    // important when virtual capture devices are installed alongside a real
    // camera (e.g. NDI Tools registers an "NDI Webcam Video" device that
    // silently produces black frames until something feeds it over NDI).
    int m_deckWebcamIndex = -1;
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

    // Remove all agent-managed layers (those with a non-empty Layer::managedKey).
    // Backs /easel/layer/clear-managed; not NDI-specific so it links in builds
    // compiled without HAS_NDI.
    void clearManagedLayers();
    // Idempotent managed shader overlay layer keyed by slot (composited above a
    // base source). Backs /easel/layer/ensure/shader.
    void ensureManagedShaderLayer(const std::string& slot, const std::string& shaderPath);
    // Idempotent managed Fluid Simulation layer keyed by slot. The built-in fluid
    // generator (addFluid) as an agent-managed, zone-assignable layer. Backs
    // /easel/layer/ensure/fluid.
    void ensureManagedFluidLayer(const std::string& slot, bool threeD = false);
    // Remove one managed layer by its exact key (drops an overlay/base layer).
    // Backs /easel/layer/remove-managed.
    void removeManagedLayer(const std::string& slot);
    // Set an ISF param on a managed shader layer (by key). Backs /easel/layer/param.
    void setManagedLayerParam(const std::string& key, const std::string& name, const OSCMessage& msg);
    // Drive the master audio-reactivity recipe on a managed layer (by key) —
    // the remote face of the PropertyPanel Reactivity/Character/Shuffle/Off
    // row, via AudioPresetEngine. Backs /easel/layer/audiopreset.
    void setManagedLayerAudioPreset(const std::string& key, const std::string& command, const OSCMessage& msg);
    // Bind one param of a managed shader layer to an audio signal (or "off").
    // Backs /easel/layer/audiobind — the remote face of the per-slider bolt.
    void setManagedLayerAudioBind(const std::string& key, const std::string& param,
                                  const std::string& signalName, const OSCMessage& msg);
    // Bind another layer as a shader image input's texture source — the
    // local face of the PropertyPanel TEXTURE dropdown. (The OSC verb rides
    // upstream's bindManagedLayerImage below.)
    void setManagedLayerBindImage(const std::string& key, const std::string& input, const std::string& sourceName);
    void bindManagedLayerImage(const std::string& key, const std::string& name, const std::string& sourceRef);
    // Agent composite/bus: find-or-create an Easel output zone, publish it as a
    // named NDI feed, and assign managed layers to it. Backs /easel/zone/ensure
    // and /easel/zone/layer.
    OutputZone* ensureZoneByName(const std::string& name);
    void ensureZoneNdi(const std::string& zoneName, const std::string& feedName);
    // Set a zone's physical output at runtime: None (preview) / Fullscreen+monitor /
    // NDI[+feed]. Backs /easel/zone/output; the render loop applies it next frame.
    void setZoneOutput(const std::string& zoneName, const std::string& dest,
                       int monitor, const std::string& ndiFeedName = std::string());
    void addZoneLayerByKey(const std::string& zoneName, const std::string& managedKey);
    // Tear down a composite zone (stop its NDI feed). Backs /easel/zone/remove.
    void removeZoneByName(const std::string& zoneName);

#ifdef HAS_NDI
    NDIOutput m_ndiOutput;
    Framebuffer m_ndiFluxInputFBO;
    NDIFinder m_ndiFinder;
    std::vector<NDISenderInfo> m_ndiSources;
    bool m_ndiOutputEnabled = true;
    // Wire-rate cap applied to ALL NDI senders (global + per-zone) each
    // frame; <= 0 = uncapped. Live-settable via OSC /easel/ndi/fps.
    float m_ndiTargetFps = 60.0f;
    void addNDISource(const std::string& senderName, const std::string& senderUrl = "");
    // Idempotent agent-driven NDI layer keyed by a stable "slot" (managedKey).
    // Re-ensuring the same slot reuses the layer and only reconnects when the
    // sender name changes, so /easel/layer/ensure/ndi never stacks duplicates.
    void ensureManagedNDILayer(const std::string& slot, const std::string& senderName);

    // Wi-Fi / Ethernet NIC pin + cross-device reachability (see NdiNetworkConfig).
    NdiNetworkSettings m_ndiNetwork;             // persisted machine-wide selection
    std::vector<NetAdapterInfo> m_netAdapters;   // cached; refreshed on panel open / Refresh
    std::vector<NdiPeerStatus> m_ndiPeerStatus;  // cached peer reachability
    double m_ndiPeerStatusLastRefresh = 0.0;
    bool m_ndiServerUp = false;                  // cached discovery-server :5959 probe
    double m_ndiServerUpLastRefresh = 0.0;
    void applyNdiNetworkSettings(bool reinit);   // write config (+ optional NDI re-init)
    void refreshNdiPeerStatus();                 // re-enumerate adapters + classify peers
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
    // FFmpeg-free mic picker state (used when HAS_FFMPEG is off — see
    // AudioInputDevices.h). Empty id = System Audio (loopback).
    std::vector<AudioInputDevice> m_micDevices;
    std::string m_selectedMicId;

    // Sentinel for m_selectedAudioDevice: no capture at all (-1 = system loopback)

    static constexpr int kAudioInputOff = -2;

    // Top-edge screen pos of the transport-pill mic button, captured each
    // frame so ##fp_sound_popup can anchor itself ABOVE the pill (the pill
    // is flush to the viewport bottom; a downward popup would be clipped).
    float m_fpSoundBtnTopX = 0.0f;
    float m_fpSoundBtnTopY = 0.0f;

#ifdef HAS_FFMPEG
    RTMPOutput m_rtmpOutput;
    char m_streamKeyBuf[128] = {};
    int m_streamAspect = 0; // 0=16:9, 1=4:3, 2=16:10, 3=Source
    VideoRecorder m_recorder;
    std::vector<RecAudioDevice> m_audioDevices;
    std::vector<RecAudioDevice> m_outputDevices; // render devices for mixer output
    // Audio hot-plug: set by a CoreAudio listener thread whenever the device
    // topology changes; the render loop re-enumerates and remaps the current
    // selection by UID (indexes shift when devices come and go).
    std::atomic<bool> m_audioDevicesChanged{false};
    // UID of a selected capture device that vanished (interface unplugged).
    // Capture falls back to System Audio, and this remembers what to restore
    // the moment the device reappears.
    std::string m_pendingAudioDeviceUID;
    void refreshAudioDevicesIfChanged();
#endif

    void renderTransportBar();
    void renderTimelinePanel();
    // Always-rendered collapse/expand tab straddling the seam between the
    // transport pill and the timeline's top edge. Independent of
    // renderTimelinePanel() itself, which stops being called at all once
    // the timeline is fully minimized — without this, there would be no
    // way to bring it back.
    void renderTimelineCollapseTab();
    // Always-rendered collapse/expand tab straddling the LEFT edge of the
    // right Properties/Audio/MIDI dock, vertically centered. Same
    // standalone-floating-window approach as renderTimelineCollapseTab —
    // the nav-bar pill version (renderRightDockNavBar) wasn't visible
    // enough, so this always sits right on the panel's edge instead.
    void renderRightPanelCollapseTab();
    // Knob/fader bank docked directly under the Canvas-mode viewport
    // ("Canvas" window) — see m_knobValue / m_faderValue above.
    void renderCanvasKnobBank();
    // Phase 5 — floating transport pill at viewport bottom-center. Draws
    // play/stop/loop + timecode in a single rounded pill that overlays the
    // canvas, matching reference B's minimal control surface. Independent
    // of the docked timeline panel which still hosts tracks/audio lane.
    void renderFloatingTransportPill();
    // Show-mode's own flush-bottom bar — just the MIDI device picker +
    // connection status, moved here out of the ShowDeck panel itself per
    // user request (that grey strip across the bottom of the whole window
    // was sitting blank in Show mode, unlike Canvas's transport pill above).
    void renderShowMidiBar();
    // Floating REC + LIVE pills at viewport bottom-right — two separate
    // rounded surfaces matching the reference's split record/broadcast
    // affordances. Independent of the docked timeline.
    void renderFloatingActionPills();

    // Recording is indefinite/live: the REC button records the active zone's
    // output continuously until the user stops it (no Work-Area auto-stop).
    bool   m_timelineExporting = false;   // retained (always false) for legacy UI checks
    double m_timelineExportEnd = 0.0;
    std::string m_timelineExportPath;
    // overridePath: empty = timestamped recordings/ default (UI button path);
    // non-empty = explicit output file (the /easel/record/start OSC trigger).
    void   startRecording(const std::string& overridePath = "");

    // Timeline panel visibility. Toggled via the T key or the transport-bar
    // show/hide button. Defaults to visible so the panel is discoverable.
    bool   m_timelineOpen = true;

    // When true, the timeline renders only its transport row and hides the
    // ruler / tracks / audio lane / clip inspector. The panel itself remains
    // docked — drag the dock splitter to shrink to just the transport.
    // Default: minimized. The transport row is enough for a quick show of
    // hands during setup, and the tracks pop up on demand (click the "+" on
    // the timeline tab or press T). Keeps the canvas area maximized on launch.
    bool   m_timelineMinimized = true;

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
    float  m_timelineAnimT = 0.0f;
    float  m_timelineCurH  = 0.0f;
    float  m_timelineTopY  = 0.0f;
    // Target (fully-open) timeline content height, measured by
    // renderTimelinePanel() and consumed next frame by updateTimelineAnim().
    float  m_timelineTargetH = 220.0f;
    void   updateTimelineAnim();

#ifdef HAS_FFMPEG
    // Audio level meter (WASAPI IAudioMeterInformation)
    void* m_audioMeterInfo = nullptr;
    void* m_audioMeterDevice = nullptr;
    int m_meterDeviceIdx = -2; // which device the meter is tracking (-2 = uninitialized)
    float m_audioLevelPeak = 0.0f;
    float m_audioLevelL = 0.0f, m_audioLevelR = 0.0f;
    float m_audioLevelSmooth = 0.0f, m_audioLevelSmoothL = 0.0f, m_audioLevelSmoothR = 0.0f;
    void updateAudioMeter();
    void cleanupAudioMeter();

    // Old mosaic meter removed — replaced by AudioAnalyzer
#endif

    // File drop handling
    std::vector<std::string> m_pendingDrops;
    void handleDroppedFiles();
    static void dropCallback(GLFWwindow* window, int count, const char** paths);

    // JSON save/load
    void saveProject(const std::string& path);
    void loadProject(const std::string& path);
    // The throwing part of loadProject — dozens of typed .get<T>() reads on
    // nested arrays. Split out so loadProject can wrap it in one try/catch
    // and recover to a clean session when a file is truncated/mis-typed.
    void loadProjectBody(const nlohmann::json& j);
    // Fresh-session state: no layers, one default zone + one default mapping.
    // Shared by File > New Project and loadProject's corrupt-file recovery.
    void resetToEmptyProject();
    // Set when loadProject() threw mid-load on this file: the app is showing
    // an empty fallback session, so the periodic / quit-time / OSC autosaves
    // must not overwrite the (still intact) file on disk with the empty
    // state. Cleared by a later successful load, or by an explicit
    // Save Project… over the same path (deliberate user overwrite).
    std::string m_projectLoadFailedPath;

    // Publish the current Show (layers + timeline + markers + BPM) to the
    // etherea-agent over OSC at /agent/play/publish. The agent caches it and
    // includes it in every state.snapshot, so paired iPhones mirror the
    // timeline. Triggered by the File > Publish to Mobile menu item and by
    // an inbound /easel/play/publish OSC message.
    void publishPlayToAgent();

    // M3 — Internal helpers for the live re-publish loop. buildPlayJson()
    // produces the wire payload as a string (no side effects); the per-frame
    // dirty check sends it only when it differs from the last send.
    std::string buildPlayJson();
    void        publishPlayIfChanged();

    // Screenshot. The flip + PNG encode runs on a worker thread (one job at
    // a time) so an externally-triggered capture doesn't freeze the frame.
    void captureScreenshot(const std::string& path);
    void captureWindow(const std::string& path);
    void writeScreenshotAsync(const std::string& path,
                              std::vector<uint8_t> pixels, int w, int h);
    void pollScreenshotTrigger();
    std::future<void> m_screenshotJob;
};
