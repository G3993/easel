#pragma once
#ifdef HAS_WHISPER

#include <string>
#include <functional>
#include <vector>
#include <mutex>
#include <thread>
#include <atomic>

struct whisper_context;

struct AudioDeviceInfo {
    std::string name;
    int index = -1;
    bool isDefault = false;
};

class WhisperSpeech {
public:
    bool init(const std::string& modelPath);
    void shutdown();

    void startListening();
    void stopListening();
    bool isListening() const { return m_listening.load(); }

    void setCallback(std::function<void(const std::string&, bool)> cb) { m_callback = std::move(cb); }

    // Call from main thread each frame to dispatch queued results
    void poll();

    // Device enumeration and selection
    const std::vector<AudioDeviceInfo>& captureDevices() const { return m_captureDevices; }
    int selectedDevice() const { return m_selectedDevice; }
    bool selectDevice(int index); // reinitializes capture device; call while not listening

private:
    whisper_context* m_ctx = nullptr;
    std::function<void(const std::string&, bool)> m_callback;

    std::atomic<bool> m_listening{false};
    std::atomic<bool> m_running{false};
    std::thread m_thread;

    // Audio capture (miniaudio)
    void* m_captureDevice = nullptr;
    void* m_maContext = nullptr;
    bool m_deviceInit = false;
    int m_selectedDevice = -1; // -1 = default

    // Device list
    std::vector<AudioDeviceInfo> m_captureDevices;
    void enumerateDevices();
    bool initCaptureDevice(int deviceIndex);
    void deinitCaptureDevice();

    // Audio ring buffer - protected by mutex
    std::mutex m_audioMutex;
    std::vector<float> m_audioBuffer;
    size_t m_consumedSamples = 0; // how far we've consumed for final transcription

    // Thread-safe result queue (whisper thread writes, main thread reads)
    std::mutex m_resultMutex;
    std::string m_pendingText;
    bool m_hasPending = false;

    // Accumulated transcript (only grows, never re-transcribes old audio)
    std::string m_accumulated;

    static constexpr int kSampleRate = 16000;
    static constexpr int kProcessIntervalMs = 500;
    static constexpr float kWindowSec = 4.0f;
    static constexpr float kMaxBufferSec = 20.0f;
    static constexpr float kMinNewAudioSec = 0.5f;

    static void audioCaptureCallback(void* device, void* output, const void* input, unsigned int frameCount);
    void processLoop();
};

#endif // HAS_WHISPER
