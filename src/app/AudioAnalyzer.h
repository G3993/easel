#pragma once
#include "render/Texture.h"
#include "app/EaselAudio.h"
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

    // Explicitly release the capture device (e.g. push-to-talk released) so
    // the OS input isn't held open between talk-spurts. The next update()
    // call after this reopens capture from scratch if a device is set.
    void stopCapture();

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

    // --- Audio Feature Bus: Tier 1 extras + Tier 2 spectral character ---
    // All normalized 0-1 unless noted; pre-smoothed, ready as GLSL uniforms.
    float sub() const        { return m_smoothSub; }       // sub-bass (coarse, ~bin1)
    float punch() const      { return m_punch; }           // transient crest (peak-hold)
    float brightness() const { return m_brightness; }      // spectral centroid, log-mapped
    float spread() const     { return m_spread; }          // focused(0) vs washy(1)
    float rolloff() const    { return m_rolloff; }          // body(0) vs air(1)
    float flatness() const   { return m_flatness; }        // tonal(0) vs noise(1)
    float flux() const       { return m_flux; }            // spectral change / movement
    float onset() const      { return m_onset; }           // discrete attack pulse
    float onsetRate() const  { return m_onsetRate; }       // rhythmic busyness
    float tilt() const       { return m_tilt; }            // warm(-1) vs harsh(+1)
    float zcr() const        { return m_zcr; }             // sibilance / cheap noisiness
    float texture() const    { return m_texture; }         // crispy(1) vs smooth(0)

    // --- Tier 3: affect / mood (0-1) ---
    float valence() const   { return m_valence; }
    float arousal() const   { return m_arousal; }
    float tension() const   { return m_tension; }
    float warmth() const    { return m_warmth; }
    float softness() const  { return m_softness; }
    float roughness() const { return m_roughness; }
    float charm() const     { return m_charm; }
    // --- Tier 4: structure / build-up ---
    float energy() const      { return m_energy; }
    float energyVel() const   { return m_energyVel; }     // -1..1
    float energyAcc() const   { return m_energyAcc; }     // -1..1
    float buildup() const     { return m_buildup; }
    float buildupRate() const { return m_buildupRate; }   // -1..1
    float drop() const        { return m_drop; }
    float novelty() const     { return m_novelty; }
    float sectionPhase() const{ return m_sectionPhase; }
    float sectionAge() const  { return m_sectionAge; }
    float layers() const      { return m_layers; }
    float density() const     { return m_density; }
    const float* presence() const { return m_presence; }  // [4] bass/lowMid/highMid/treble
    const float* flow() const     { return m_flow; }      // [2] drift vector -1..1
    // --- Tier 5: palette (linear RGB anchors) + harmony ---
    const float* palShadow() const { return m_palShadow; }
    const float* palMid() const    { return m_palMid; }
    const float* palHigh() const   { return m_palHigh; }
    const float* palAccent() const { return m_palAccent; }
    float palTemp() const          { return m_palTemp; }
    float palSat() const           { return m_palSat; }
    const float* chroma() const    { return m_chroma; }   // [12]
    float dominantPitch() const    { return m_dominantPitch; }
    float majorMinor() const       { return m_majorMinor; }

    // True RMS from sample buffer (0-1)
    float rms() const { return m_smoothRMS; }

    // Backward compat: smoothed RMS matching old IAudioMeterInformation behavior
    float smoothedRMS() const { return m_smoothRMS; }

    // Beat detection
    float beatDecay() const { return m_beatDecay; }
    bool beatDetected() const { return m_beatThisFrame; }

    // --- EaselAudio v1: temperament matrix (Synesthesia taxonomy) ---------
    // Hit = dual-follower onset (AD 10ms/350ms), Presence = slow macro
    // envelope (1.5s/4s), Time = monotonic clock += dt*band (UNCLAMPED —
    // use as a time source; pauses in silence, can never jitter).
    float bassHit() const  { return m_hitOut[0]; }
    float midHit() const   { return m_hitOut[1]; }
    float highHit() const  { return m_hitOut[2]; }
    float bassPresence() const  { return m_presOut[0]; }
    float midPresence() const   { return m_presOut[1]; }
    float highPresence() const  { return m_presOut[2]; }
    float levelPresence() const { return m_presOut[3]; }   // full-mix presence
    float bassTime() const  { return m_timeClock[0]; }
    float midTime() const   { return m_timeClock[1]; }
    float highTime() const  { return m_timeClock[2]; }
    float levelTime() const { return m_timeClock[3]; }

    // --- EaselAudio v1: detected tempo (autocorrelation, 50-220 BPM) ------
    // Phase-locks the app BPMSync clock via Application; tap/OSC override.
    float detectedBPM() const           { return m_tempo.bpm; }
    float detectedBPMConfidence() const { return m_tempo.confidence; }

    // --- EaselAudio v1: tier-1 pseudo-stems (LR band split + causal median
    // HPSS — see computeStems). AGC'd + stem-conditioned (extra release). ---
    enum Stem { StemBass = 0, StemDrums, StemMelody, StemAir, StemVocal, StemCount };
    float stem(int s) const         { return m_stemOut[s]; }
    float stemHit(int s) const      { return m_stemHitOut[s]; }
    float stemPresence(int s) const { return m_stemPresOut[s]; }

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
    bool  m_specDirty = false;   // set by runFFT, consumed by computeStems

    // Cached kiss_fft config — allocated once, reused every frame
    // (was alloc/freed per runFFT call). Opaque to avoid kiss_fft.h here.
    void* m_fftCfg = nullptr;

    // Pre-AGC band energies (linear, manual gains applied) — the
    // self-normalizing detectors (beat, Hits) feed on these so they stay
    // level-independent; the AGC'd 0-1 values below are what shaders see.
    float m_bandE[4] = {0, 0, 0, 0};   // bass/lowMid/highMid/treble
    float m_rmsLin = 0;                // linear RMS (pre-AGC, post-inputGain)

    // Raw band energies (post-AGC + gate + curves, before temporal smoothing)
    float m_rawBass = 0, m_rawLowMid = 0, m_rawHighMid = 0, m_rawTreble = 0;
    float m_rawRMS = 0;

    // dB-domain AGC per band (EaselAudio §3) — replaces the old fixed magic
    // gains 60/100/200/400x and the 4x mic seed. Default ON; the user Gain
    // sliders remain as a defeatable manual trim applied before the AGC.
    easelaudio::DbAGC m_bandAgc[4];
    easelaudio::DbAGC m_rmsAgc;
    easelaudio::DbAGC m_subAgc;

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
    // Gentler defaults so audio reactivity glides instead of strobing on every
    // beat — was 8/3, which read as "way too fast" when working with shaders.
    float m_smoothAttackRate  = 4.5f;
    float m_smoothReleaseRate = 1.8f;

    // Smoothed values
    float m_smoothBass = 0, m_smoothLowMid = 0, m_smoothHighMid = 0, m_smoothTreble = 0;
    float m_smoothRMS = 0;

    // Beat detection — dual fast/slow follower crossing on the PRE-AGC
    // bass-weighted energy (self-normalizing; replaced the naive
    // energy-vs-1.4x-rolling-average trigger). Cooldown + decay semantics
    // of beatDecay()/beatDetected() are unchanged.
    float m_beatDecay = 0;
    bool m_beatThisFrame = false;
    float m_beatFast = 0, m_beatSlow = 0;
    float m_beatCooldown = 0; // seconds remaining

    // --- EaselAudio v1 state ------------------------------------------------
    // Temperament matrix: [0]=bass [1]=mid [2]=high ([3]=mix for pres/time)
    easelaudio::HitDetector m_hitDet[3];
    easelaudio::PresenceEnv m_presEnv[4];
    float m_hitOut[3]  = {0, 0, 0};
    float m_presOut[4] = {0, 0, 0, 0};
    float m_timeClock[4] = {0, 0, 0, 0};   // unclamped integrated clocks

    // Tempo
    easelaudio::TempoTracker m_tempo;
    float m_fluxNorm = 0;   // unsmoothed AGC-normalized flux (onset strength)

    // Pseudo-stems: causal median HPSS over a 31-frame magnitude-spectrum
    // history + soft band split. Energies are pre-AGC; outputs conditioned
    // with the stem role preset (+50% release vs mix bands).
    static constexpr int kHPSSFrames = 31;
    static constexpr int kHPSSKernel = 31;   // frequency-median kernel
    float m_specHist[kHPSSFrames][kBins] = {};
    int   m_specHistPos = 0;
    int   m_specHistCount = 0;
    float m_stemE[StemCount] = {0, 0, 0, 0, 0};      // pre-AGC energies
    float m_vocalFluxGate = 0;
    float m_vocalFluxAvg = 1e-4f;                    // running normalizer
    float m_lastVocalFlux = 0;                       // 2-6kHz positive flux
    easelaudio::DbAGC       m_stemAgc[StemCount];
    easelaudio::Conditioner m_stemCond[StemCount];
    easelaudio::HitDetector m_stemHitDet[StemCount];
    easelaudio::PresenceEnv m_stemPresEnv[StemCount];
    float m_stemOut[StemCount]     = {0, 0, 0, 0, 0};
    float m_stemHitOut[StemCount]  = {0, 0, 0, 0, 0};
    float m_stemPresOut[StemCount] = {0, 0, 0, 0, 0};
    bool  m_stemInit = false;

    // FFT texture
    Texture m_fftTex;
    uint8_t m_fftTexData[128] = {};

    // --- Audio Feature Bus state (Tier 1 extras + Tier 2 spectral) ---
    // Adaptive gain control: tracks a slow running [lo,hi] envelope and maps
    // the raw value into 0-1, so "shape" features stay well-scaled across very
    // different material without a fixed magic constant.
    struct AGC {
        float lo = 0.0f, hi = 0.05f;
        float norm(float x, float dt) {
            // hi rises fast to peaks, falls slowly; lo the mirror.
            float upHi = 1.0f - std::exp(-6.0f * dt);    // fast catch
            float dnHi = 1.0f - std::exp(-0.20f * dt);   // slow release
            hi += (x - hi) * (x > hi ? upHi : dnHi);
            float upLo = 1.0f - std::exp(-0.20f * dt);
            float dnLo = 1.0f - std::exp(-6.0f * dt);
            lo += (x - lo) * (x < lo ? dnLo : upLo);
            float d = std::max(1e-4f, hi - lo);
            float y = (x - lo) / d;
            return y < 0.0f ? 0.0f : (y > 1.0f ? 1.0f : y);
        }
    };
    // Instant attack, hold, then exponential decay — for transient channels.
    struct PeakHold {
        float v = 0.0f, hold = 0.0f;
        float update(float x, float dt, float holdT, float decayTau) {
            if (x >= v) { v = x; hold = holdT; }
            else if (hold > 0.0f) { hold -= dt; }
            else { v *= std::exp(-dt / std::max(1e-3f, decayTau)); }
            return v;
        }
    };

    float m_prevSpec[kBins] = {};
    float m_smoothSub = 0;
    float m_brightness = 0, m_spread = 0, m_rolloff = 0, m_flatness = 0;
    float m_flux = 0, m_onset = 0, m_onsetRate = 0, m_tilt = 0, m_zcr = 0, m_texture = 0.5f;
    float m_punch = 0;
    float m_fluxFloor = 1e-3f;          // slow running floor for flux normalization
    float m_onsetMedian = 0;            // adaptive onset threshold
    float m_beatCooldownOnset = 0;      // refractory for onset peak-pick
    AGC m_agcBright, m_agcFlat, m_agcFlux, m_agcTilt;
    PeakHold m_punchPH, m_onsetPH, m_dropPH;

    // Tier 3 — affect (neutral init)
    float m_valence = 0.5f, m_arousal = 0, m_tension = 0, m_warmth = 0.5f;
    float m_softness = 0.5f, m_roughness = 0, m_charm = 0.05f;
    // Tier 4 — structure (dual-timescale)
    float m_energyFast = 0, m_energySlow = 0, m_energyRunMax = 1e-3f;
    float m_energy = 0, m_energyVel = 0, m_energyAcc = 0, m_prevEnergy = 0, m_prevEnergyVel = 0;
    float m_buildup = 0, m_buildupRate = 0, m_prevBuildup = 0, m_drop = 0;
    float m_novelty = 0, m_sectionPhase = 0, m_sectionAge = 0; int m_sectionCount = 0;
    float m_sectionCooldown = 0;
    float m_layers = 0, m_density = 0;
    float m_presence[4] = {0, 0, 0, 0};
    float m_band12[12] = {0}, m_band12Slow[12] = {0};
    float m_flow[2] = {0, 0};
    // Tier 5 — palette + harmony
    float m_chroma[12] = {0};
    int   m_binToPC[kBins] = {0};
    bool  m_binToPCInit = false;
    float m_dominantPitch = 0, m_majorMinor = 0.5f;
    float m_hueSin = 0, m_hueCos = 1, m_accHueSin = 0, m_accHueCos = 1; // circular hue smoothing
    float m_palShadow[3] = {0.05f,0.05f,0.06f};
    float m_palMid[3]    = {0.30f,0.30f,0.33f};
    float m_palHigh[3]   = {0.80f,0.80f,0.84f};
    float m_palAccent[3] = {0.90f,0.85f,0.70f};
    float m_palTemp = 0, m_palSat = 0.3f;

    void runFFT();
    void computeBands();
    void conditionBands(float dt);          // dB AGC + gate + curves (every frame)
    void computeTemperaments(float dt);     // Hit/Presence/Time matrix + tempo feed
    void computeStems(float dt);            // tier-1 pseudo-stems (HPSS + bands)
    void computeSpectralFeatures(float dt); // Tier 2 + sub/punch over m_spectrum
    void computeAffect(float dt);           // Tier 3 valence/arousal/tension/...
    void computeStructure(float dt);        // Tier 4 energy/buildup/drop/section/layers
    void computeFlow(float dt);             // drift vector from low-freq envelopes
    void computeChroma(float dt);           // 12-bin pitch class, dominant, major/minor
    void computePalette(float dt);          // Tier 5 Oklch ramp + accent
    void detectBeat(float dt);
    void smoothBands(float dt);
    void updateFFTTexture();

    // dt-based exponential smoothing helper
    static float expSmooth(float current, float target, float rate, float dt) {
        return current + (target - current) * (1.0f - std::exp(-rate * dt));
    }
};
