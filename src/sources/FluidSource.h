#pragma once
#include "sources/ContentSource.h"
#include "sources/AudioBinding.h"
#include <glad/glad.h>
#include <array>
#include <vector>
#include <random>
#include <map>
#include <string>

// GPU stable-fluids simulation (Stam / Pavel Dobryakov WebGL-Fluid style),
// ported from ShaderClaw3's js/fluid-renderer.js to native desktop GL
// (OpenGL 4.1 core, GLSL 330). Self-contained: owns its own ping-pong
// FBOs and shader programs, renders the dye field into an RGBA8 output
// texture each frame, and keeps itself alive with an auto-splat movement
// pattern (no mouse required) so it works as a drop-in generator layer.
class FluidSource : public ContentSource {
public:
    FluidSource() = default;
    ~FluidSource() override;

    bool init(int outW = 1280, int outH = 720);

    // ContentSource
    void update() override;
    GLuint textureId() const override { return m_output.tex; }
    int width() const override { return m_outW; }
    int height() const override { return m_outH; }
    std::string typeName() const override { return "Fluid"; }
    std::string sourcePath() const override { return "__fluid__"; }

    // Inject a directional splat in normalized [0,1] coords (e.g. from
    // mouse/MIDI/vision later). dx/dy are velocity, color is dye RGB.
    void splat(float x, float y, float dx, float dy,
               float r, float g, float b);

    // ── Audio/MIDI-reactive parameter bindings ────────────────────────
    // Same "sparkle" model as ShaderSource — keyed by a stable param id
    // string (see applyAudioBindings() for the id → member mapping). The
    // PropertyPanel FLUID section opens the bind popup against this map.
    std::map<std::string, AudioBinding>& audioBindings() { return m_audioBindings; }
    const std::map<std::string, AudioBinding>& audioBindings() const { return m_audioBindings; }
    void applyAudioBindings(float level, float bass, float mid, float high,
                            float beat, float dt, class MIDIManager* midi = nullptr,
                            float energy = 0.0f, float build = 0.0f, float drop = 0.0f,
                            float silence = 0.0f, float momentum = 0.5f);

    // Interactive pointer splat (mouse drag / vision hand). x,y in normalized
    // [0,1] GL uv (origin bottom-left); dx,dy are velocity. Enqueued here and
    // applied inside update() so callers can splat from the UI thread without
    // disturbing the compositor's GL state. Color is generated internally.
    void queuePointerSplat(float x, float y, float dx, float dy);

    // ── Image-as-dye injection (optional) ─────────────────────────────
    // When enabled, the bound texture is additively painted into the dye
    // field every frame; the standard advection then smears it through the
    // velocity field, giving the classic "fluid carries the picture" look.
    // Source is supplied by the host each frame (mirrors ShaderSource's
    // image-binding pattern — see Application's per-frame refresh).
    struct ImageRef {
        GLuint   textureId    = 0;     // live GL texture (refreshed per frame)
        int      width        = 0;
        int      height       = 0;
        uint32_t sourceLayerId = 0;    // 0 = no layer bound, picker is empty
        bool     flippedV     = false; // source uses top-down v (NDI, etc.)
    };
    ImageRef&       imageSource()       { return m_image; }
    const ImageRef& imageSource() const { return m_image; }

    // ── Live-tunable config (exposed in the PropertyPanel "Fluid"
    // section). Public so the UI can bind pillSlider(&...) directly,
    // mirroring how ParticleSource exposes its emitter struct. ──────
    // Defaults match Pavel Dobryakov's WebGL-Fluid-Simulation exactly.
    float m_curlAmount = 30.0f;          // CURL
    float m_densityDissipation = 1.0f;   // DENSITY_DISSIPATION
    float m_velocityDissipation = 0.2f;  // VELOCITY_DISSIPATION
    float m_pressureValue = 0.8f;        // PRESSURE
    int   m_pressureIters = 20;          // PRESSURE_ITERATIONS
    float m_splatRadius = 0.25f;         // SPLAT_RADIUS
    float m_splatIntensity = 0.15f;      // generateColor() ×0.15
    float m_autoRate = 24.0f;            // auto-splats per second (Easel-native)
    bool  m_autoMovement = true;         // self-animate without input
    // ── Auto-movement presets — the virtual cursor's motion pattern ───
    // Sound: audio-reactive — a single orb lives at center, ventures out as
    // the sound's energy rises (random wander, brighter/denser splats), and
    // springs back to the middle + dims + fades when it goes quiet. Feels
    // alive: fast attack on loudness, slow graceful release into stillness.
    enum AutoPattern { Wander = 0, Orbit, Figure8, Pulse, Rain, Spiral, Sound };
    int   m_autoPattern = Wander;
    // ── Color palettes — dye color theme the splat hue cycles through ─
    // Rainbow keeps the legacy full-spectrum HSV sweep; the rest loop a
    // 4-stop gradient. Custom uses the user-editable stops below.
    enum ColorPalette { Rainbow = 0, Ocean, Sunset, Neon, Fire, Pastel,
                        Forest, Mono, Custom };
    int   m_palette = Rainbow;
    float m_customStops[4][3] = { {1.0f, 0.2f, 0.5f}, {0.4f, 0.2f, 1.0f},
                                  {0.1f, 0.8f, 1.0f}, {0.9f, 0.9f, 0.4f} };
    float m_autoSpeed = 1.0f;            // pattern tempo (×base speed)
    float m_autoScale = 0.30f;           // radius / amplitude / spread
    // ── Sound movement controls (spawn/unspawn wandering orbs) ────────
    // A few knobs to dial the audio reactivity in smoothly. All only affect
    // the Sound pattern; ignored by the geometric presets.
    float m_sndSensitivity = 1.0f;   // master audio gain          (0..3)
    float m_sndThreshold   = 0.08f;  // loudness floor to spawn    (0..0.6)
    float m_sndSmoothing   = 0.5f;   // attack/release smoothing   (0..1)
    float m_sndBeatKick    = 0.6f;   // beat → spawn burst + kick  (0..2)
    float m_sndWander      = 0.6f;   // how far/fast orbs roam     (0..1.5)
    int   m_sndMaxOrbs     = 10;     // live population cap         (1..24)
    bool  m_shading = true;              // velocity-based shading highlight
    // ── Native-only quality (browser falls back to 8-bit / no bloom) ──
    bool  m_hdrDye = true;               // RGBA16F float dye (read at init)
    // ── Bloom pyramid (Pavel-exact multi-pass) ────────────────────────
    bool  m_bloom = true;                // BLOOM
    float m_bloomIntensity = 0.8f;       // BLOOM_INTENSITY
    float m_bloomThreshold = 0.6f;       // BLOOM_THRESHOLD
    float m_bloomSoftKnee = 0.7f;        // BLOOM_SOFT_KNEE
    // ── Sunrays (Pavel-exact radial blur) ─────────────────────────────
    bool  m_sunrays = true;              // SUNRAYS
    float m_sunraysWeight = 1.0f;        // SUNRAYS_WEIGHT
    // ── Image inject ──────────────────────────────────────────────────
    bool  m_imageEnabled   = false;      // gate (off until host binds a layer)
    float m_imageIntensity = 0.10f;      // per-frame dye "over" strength (0..1)
    // Reform mode: advect a UV-coord field + remap the image through it so the
    // picture distorts without blurring and melts back to the sharp original.
    bool  m_imageReform    = false;      // off = paint/reveal; on = UV-remap
    float m_reformRate     = 0.40f;      // 0 = stays distorted; higher = reforms

private:
    struct FBO { GLuint fbo = 0, tex = 0; int w = 0, h = 0; };
    struct DoubleFBO { FBO read, write; int w = 0, h = 0; void swap(); };

    bool m_ready = false;
    int  m_outW = 1280, m_outH = 720;
    double m_lastTime = 0.0;
    float  m_hue = 0.0f;
    float  m_splatAccum = 0.0f;   // per-instance splat-rate accumulator
    float  m_autoPhase = 0.0f;    // pattern clock (orbit/figure-8/spiral/rain)
    // Drifting virtual cursors ("orbs") for continuous motion-driven splats —
    // mirrors the JS freeform movement pattern (the website default). Wander
    // runs two of them, staggered so they re-aim at different moments and
    // read as two distinct moving orbs.
    static constexpr int kWanderOrbs = 2;
    struct WanderOrb { float x, y, vx, vy, timer; };
    WanderOrb m_orbs[kWanderOrbs] = { {0.35f, 0.5f, 0.0f, 0.0f, 0.0f},
                                      {0.65f, 0.5f, 0.0f, 0.0f, 0.55f} };
    unsigned m_orbSplat = 0;      // alternates Wander splats between orbs
    // ── Sound movement state ──────────────────────────────────────────
    // Latest audio captured each frame in applyAudioBindings() (independent
    // of whether any param bindings exist), consumed by the Sound pattern.
    float m_sndLevel = 0.0f, m_sndEnergy = 0.0f,
          m_sndSilence = 0.0f, m_sndMomentum = 0.5f;
    float m_sndDrive = 0.0f;          // smoothed 0..1 loudness drive
    float m_sndBeat = 0.0f;           // raw per-frame beat (0..1)
    float m_sndBeatEnv = 0.0f;        // fast-decaying beat envelope
    float m_sndBeatEnvPrev = 0.0f;    // last frame's env (rising-edge detect)
    // Spawn/unspawn wandering-orb pool. Orbs are BORN from audio energy + beats,
    // roam with momentum, and DIE (fade out) when the sound drops — population
    // tracks the smoothed drive. Each orb lays a distance-stamped dye trail.
    struct SoundOrb {
        float x = 0.5f, y = 0.5f, vx = 0.0f, vy = 0.0f;
        float tx = 0.5f, ty = 0.5f;     // current wander target
        float life = 0.0f;              // 0..1 (0 = dead / free slot)
        float reaim = 0.0f;             // countdown to next wander target
        float px = 0.5f, py = 0.5f, stamp = 0.0f;   // trail-stamp state
        bool  active = false;
    };
    static constexpr int kSoundOrbs = 24;   // hard ceiling (m_sndMaxOrbs gates live)
    SoundOrb m_sndOrbs[kSoundOrbs];
    float m_sndSpawnAccum = 0.0f;     // fractional spawn-budget accumulator
    std::mt19937 m_rng{12345};
    std::map<std::string, AudioBinding> m_audioBindings;
    // Interactive pointer splats queued from the UI, drained in update().
    struct PointerSplat { float x, y, dx, dy; };
    std::vector<PointerSplat> m_pointerSplats;
    // Optional image input. Host refreshes textureId each frame from the
    // bound layer (see Application's Fluid branch). All zero = no source.
    ImageRef m_image;

    // Programs (keyed by enum order)
    GLuint m_progSplat = 0, m_progAdvect = 0, m_progDiverge = 0,
           m_progCurl = 0, m_progVort = 0, m_progPressure = 0,
           m_progGradSub = 0, m_progClear = 0;
    GLuint m_progSplatImage = 0;  // dye splat coloured from the bound image
    GLuint m_progCoordInit = 0, m_progCoordAdvect = 0, m_progRemap = 0; // reform
    bool   m_suppressDyeSplat = false;  // runtime: skip dye writes in reform
    bool   m_coordNeedsInit   = true;   // re-seed coord field on next reform
    // Post-processing programs (Pavel-exact bloom/sunrays/display chain).
    GLuint m_progBloomPrefilter = 0, m_progBloomBlur = 0, m_progBloomFinal = 0,
           m_progSunraysMask = 0, m_progSunrays = 0, m_progBlur = 0;
    // Image-inject pass — samples the bound texture and adds it to the dye
    // each frame; the advect pass then smears it through the velocity field.
    GLuint m_progInject = 0;
    // Display program is compiled per #define combination (SHADING/BLOOM/
    // SUNRAYS), mirroring Pavel's keyword material; cached by 3-bit mask.
    GLuint m_displayPrograms[8] = {0};

    GLuint m_quadVAO = 0, m_quadVBO = 0;

    DoubleFBO m_velocity, m_dye, m_pressure, m_coord;
    FBO m_divergence, m_curl, m_output;
    // Bloom: destination at BLOOM_RESOLUTION + a downsample mip chain.
    FBO m_bloomTarget;
    std::vector<FBO> m_bloomMips;
    // Sunrays mask/result + separable-blur scratch.
    FBO m_sunraysFbo, m_sunraysTemp;
    // Blue-noise-ish dithering texture for the display pass.
    GLuint m_ditherTex = 0; int m_ditherW = 1, m_ditherH = 1;

    // Fixed resolution config (not live-tunable — needs FBO realloc).
    int   m_simRes = 256, m_dyeRes = 1024;
    int   m_bloomRes = 256, m_sunraysRes = 196;  // Pavel BLOOM/SUNRAYS_RESOLUTION
    int   m_bloomIters = 8;                       // BLOOM_ITERATIONS

    GLuint compileProgram(const char* vs, const char* fs);
    GLuint compileDisplay(bool shading, bool bloom, bool sunrays);
    GLuint displayProgram(bool shading, bool bloom, bool sunrays);
    FBO       createFBO(int w, int h, GLint internalFmt, GLenum type, GLenum filter);
    DoubleFBO createDoubleFBO(int w, int h, GLint internalFmt, GLenum type, GLenum filter);
    void destroyFBO(FBO& f);
    void createDitherTexture();
    void getResolution(int res, int& w, int& h) const;
    void blit(const FBO& target);
    void step(float dt);
    void applyBloom(const FBO& source);          // writes m_bloomTarget
    void applySunrays(const FBO& source);         // writes m_sunraysFbo
    void applyImageInject();                      // adds m_image into m_dye
    void initCoordField();                        // seed coord field = identity
    void advectCoord(float dt);                   // carry coords by velocity
    void resolveRemap();                          // image through coords -> dye
    void blurFBO(FBO& target, FBO& temp, int iterations);
    void renderToOutput();
    void autoSplats(float dt);
    void hsv(float h, float s, float v, float& r, float& g, float& b);
    // Map the cycling phase t [0,1) through the active palette (m_palette).
    void paletteColor(float t, float& r, float& g, float& b);
};
