#pragma once
#include "render/Texture.h"
#include <vector>
#include <string>
#include <cmath>
#include <algorithm>

#ifdef _WIN32
// Forward declare WASAPI types to avoid Windows.h in header
struct IAudioClient;
struct IAudioCaptureClient;
struct IMMDevice;
#endif

struct AudioBands {
    float bass = 0;
    float lowMid = 0;
    float highMid = 0;
    float treble = 0;
};

// Per-band response curve — reshapes the raw 0..1 band energy into the value
// shaders actually react to. The chain (in order) is:
//   1. Floor/Ceil — remap [floor,ceil] → [0,1] (a smooth, soft noise gate +
//      headroom limiter; replaces needing a hard gate per band).
//   2. Curve      — pow(x, exponent). >1 = ease-in (suppress quiet, punchier
//      peaks); <1 = ease-out (lift quiet, more constantly active).
//   3. Contrast   — blend toward a smoothstep S-curve for soft toes/shoulders.
// Defaults are identity, so an untouched curve is a no-op.
struct AudioCurve {
    float floor    = 0.0f;   // input below this → 0
    float ceil     = 1.0f;   // input at/above this → 1
    float exponent = 1.0f;   // gamma; 1 = linear
    float contrast = 0.0f;   // 0 = off, 1 = full smoothstep S-curve
};

// Single source of truth for the transfer function — used by the analyzer to
// shape the live signal AND by the Audio panel to draw the curve graph, so the
// graph is always exactly what you hear.
inline float applyAudioCurve(float x, const AudioCurve& c) {
    float d = std::max(1e-4f, c.ceil - c.floor);
    float y = std::min(std::max((x - c.floor) / d, 0.0f), 1.0f);
    y = std::pow(y, std::max(0.01f, c.exponent));
    if (c.contrast > 0.0f) {
        float s = y * y * (3.0f - 2.0f * y);   // smoothstep
        y += (s - y) * std::min(c.contrast, 1.0f);
    }
    return y;
}

// One-click easing presets for the Response curve. Each preset bundles a full
// AudioCurve (floor/ceil/exponent/contrast) plus the GLOBAL temporal smoothing
// rates so "smoother" presets actually soften the envelope too. Smoothing is
// global (shared by all bands), not per-band — see smoothAttack()/smoothRelease().
// attack <= 0 means "leave the current smoothing untouched".
struct AudioCurvePreset {
    const char* name;
    const char* desc;     // one-line tooltip describing the feel
    AudioCurve  curve;    // floor / ceil / exponent / contrast
    float       attack;   // global smoothing to apply, 1/s (<=0 = leave unchanged)
    float       release;
};

// Curated table. Values are tuned so each entry FEELS distinct and musical.
// Order: neutral first, then progressively smoother → snappier → specialty.
// AudioCurve fields are {floor, ceil, exponent, contrast}.
static const AudioCurvePreset kAudioCurvePresets[] = {
    { "Linear",      "Neutral 1:1 — the raw signal, no shaping.",
      { 0.00f, 1.00f, 1.00f, 0.00f },  8.0f,  3.0f },
    { "Smooth",      "Gentle S-curve + slow release — syrupy, never strobes.",
      { 0.00f, 1.00f, 1.10f, 0.50f },  5.0f,  1.5f },
    { "Ambient",     "Very slow, floaty glide that lifts quiet detail.",
      { 0.00f, 1.00f, 0.85f, 0.20f },  3.0f,  0.8f },
    { "Punchy",      "Ease-in with a fast attack — peaks pop, noise cut.",
      { 0.05f, 1.00f, 2.00f, 0.10f }, 16.0f,  4.0f },
    { "Snappy",      "Fast and tight — highly reactive, minimal glide.",
      { 0.02f, 1.00f, 1.30f, 0.00f }, 22.0f, 12.0f },
    { "Gentle",      "Lifts quiet detail with light smoothing — sensitive.",
      { 0.00f, 1.00f, 0.60f, 0.15f },  7.0f,  3.0f },
    { "Gate",        "High floor + steep — only strong hits register.",
      { 0.25f, 1.00f, 1.80f, 0.00f }, 18.0f,  6.0f },
    { "Ease In-Out", "Classic full smoothstep S — soft toe and shoulder.",
      { 0.00f, 1.00f, 1.00f, 1.00f },  8.0f,  3.0f },
    { "Exponential", "Very peaky — suppresses all but the loudest moments.",
      { 0.00f, 1.00f, 3.00f, 0.00f }, 12.0f,  4.0f },
};
static constexpr int kAudioCurvePresetCount =
    (int)(sizeof(kAudioCurvePresets) / sizeof(kAudioCurvePresets[0]));

// Curve targets: a global Master curve applied on top of each of the 4 bands.
enum CurveBand { CurveMaster = 0, CurveBass, CurveLowMid, CurveHighMid, CurveTreble, CurveCount };

class AudioAnalyzer {
public:
    static constexpr int kFFTSize = 512;
    static constexpr int kBins = kFFTSize / 2;

    AudioAnalyzer() = default;
    ~AudioAnalyzer();

    // Call once per frame with delta time
    void update(float dt);

    // Change audio device (-1 = system loopback, >=0 = device index)
    void setDevice(int deviceIdx);
    void setDeviceId(const std::string& id, bool isCapture);

    // Opt-in gate for macOS ScreenCaptureKit system-audio capture. macOS
    // triggers a "Screen Recording" TCC prompt the first time SCShareableContent
    // is called; for self-signed dev builds the grant is keyed to the binary's
    // cdhash and evaporates on every rebuild, so the prompt fires repeatedly.
    // Gating the call behind an explicit opt-in means the prompt only appears
    // when the user actually picks System Audio / starts recording, not on
    // every app launch.
    void setWantsSystemAudio(bool v) { m_wantsSystemAudio = v; }
    bool wantsSystemAudio() const    { return m_wantsSystemAudio; }

    // Smoothed frequency bands (0-1)
    float bass() const { return m_smoothBass; }
    float lowMid() const { return m_smoothLowMid; }
    float highMid() const { return m_smoothHighMid; }
    float treble() const { return m_smoothTreble; }

    // True RMS from sample buffer (0-1)
    float rms() const { return m_smoothRMS; }

    // Backward compat: smoothed RMS matching old IAudioMeterInformation behavior
    float smoothedRMS() const { return m_smoothRMS; }

    // Beat detection
    float beatDecay() const { return m_beatDecay; }
    bool beatDetected() const { return m_beatThisFrame; }

    // FFT texture (128x1 GL_R8, power spectrum normalized 0-255)
    GLuint fftTexture() const { return m_fftTex.id(); }

    // External feed mode: when true, skip internal WASAPI capture and
    // rely on feedSamples() from an external source (e.g., AudioMixer)
    void setExternalFeed(bool enabled) { m_externalFeed = enabled; }
    bool externalFeed() const { return m_externalFeed; }

    // Feed samples externally (bypasses WASAPI) — used by AudioMixer and tests
    void feedSamples(const float* mono, int count);

    // Expose raw values for testing
    float rawBass() const { return m_rawBass; }
    float rawLowMid() const { return m_rawLowMid; }
    float rawHighMid() const { return m_rawHighMid; }
    float rawTreble() const { return m_rawTreble; }
    float rawRMS() const { return m_rawRMS; }

    // User-adjustable gains (applied on top of default band scaling)
    float& inputGain() { return m_inputGain; }
    float& bassGain() { return m_bassGain; }
    float& lowMidGain() { return m_lowMidGain; }
    float& highMidGain() { return m_highMidGain; }
    float& trebleGain() { return m_trebleGain; }
    float& noiseGate() { return m_noiseGate; }

    // Per-frame envelope smoothing rates (1/s). The smoother always uses
    // the asymmetric attack-vs-release model: when the raw band energy is
    // RISING toward a new target, current → target advances at the attack
    // rate; when it's FALLING, the release rate is used instead. Higher
    // rate = faster response = less smooth. Lower rate = slower response
    // = smoother / more glide. Reasonable musical range is roughly 0.5
    // (very smooth, ~2s glide) to 30 (snappy, ~30ms glide).
    float& smoothAttack()  { return m_smoothAttackRate; }
    float& smoothRelease() { return m_smoothReleaseRate; }

    // Response curves — index with CurveBand (0=Master, 1..4 = bands).
    AudioCurve& curve(int band) { return m_curves[band]; }
    const AudioCurve& curve(int band) const { return m_curves[band]; }
    // Current pre-curve input level feeding a given CurveBand this frame, so
    // the UI can draw a live dot on the transfer graph. Master uses the
    // loudest band as its proxy.
    float curveInput(int band) const { return m_curveInput[band]; }

private:
#ifdef _WIN32
    // WASAPI capture
    IAudioClient* m_audioClient = nullptr;
    IAudioCaptureClient* m_captureClient = nullptr;
    IMMDevice* m_device = nullptr;
#elif defined(__APPLE__)
    void* m_macAudioImpl = nullptr; // Opaque pointer to macOS audio capture
#endif
    int m_deviceIdx = -2; // -2 = uninitialized
    int m_requestedDevice = -1;
    std::string m_deviceId;         // Windows endpoint ID string
    std::string m_requestedDeviceId;
    bool m_requestedIsCapture = false;
    int m_sampleRate = 48000;
    int m_channels = 2;
    bool m_initialized = false;
    bool m_externalFeed = false;
    bool m_captureFailed = false;  // true after permission denied — don't retry
    bool m_wantsSystemAudio = false; // opt-in gate for ScreenCaptureKit (see header)

    void initCapture();
    void cleanupCapture();
    void drainPackets();

    // Ring buffer (mono, 512 samples)
    float m_ringBuf[kFFTSize] = {};
    int m_ringPos = 0;
    int m_samplesAccumulated = 0;

    // FFT output (power spectrum, 256 bins)
    float m_spectrum[kBins] = {};

    // Raw band energies (before smoothing)
    float m_rawBass = 0, m_rawLowMid = 0, m_rawHighMid = 0, m_rawTreble = 0;
    float m_rawRMS = 0;

    // User gains
    float m_inputGain = 1.0f;   // master input multiplier (applied to RMS + bands)
    float m_bassGain = 1.0f;
    float m_lowMidGain = 1.0f;
    float m_highMidGain = 1.0f;
    float m_trebleGain = 1.0f;
    float m_noiseGate = 0.0f;   // values below this threshold are squashed to 0

    // Response curves [CurveMaster, Bass, LowMid, HighMid, Treble] and the
    // captured pre-curve input per target (for the live graph dot).
    AudioCurve m_curves[CurveCount];
    float      m_curveInput[CurveCount] = {0, 0, 0, 0, 0};

    // Default smoothing rates — chosen so a typical reactive shader gets
    // a punchy attack but a soft release that doesn't strobe on every
    // beat. Mutable via smoothAttack() / smoothRelease() accessors.
    float m_smoothAttackRate  = 8.0f;
    float m_smoothReleaseRate = 3.0f;

    // Smoothed values
    float m_smoothBass = 0, m_smoothLowMid = 0, m_smoothHighMid = 0, m_smoothTreble = 0;
    float m_smoothRMS = 0;

    // Beat detection
    float m_beatDecay = 0;
    bool m_beatThisFrame = false;
    float m_energyHistory[32] = {};
    int m_energyHistoryPos = 0;
    float m_beatCooldown = 0; // seconds remaining

    // FFT texture
    Texture m_fftTex;
    uint8_t m_fftTexData[128] = {};

    void runFFT();
    void computeBands();
    void detectBeat(float dt);
    void smoothBands(float dt);
    void updateFFTTexture();

    // dt-based exponential smoothing helper
    static float expSmooth(float current, float target, float rate, float dt) {
        return current + (target - current) * (1.0f - std::exp(-rate * dt));
    }
};
