#ifdef HAS_WHISPER

#define MINIAUDIO_IMPLEMENTATION
#include "miniaudio.h"

#include "speech/WhisperSpeech.h"
#include "whisper.h"
#include <iostream>
#include <fstream>
#include <algorithm>
#include <cctype>
#include <chrono>
#include <cmath>

static void speechLog(const std::string& msg) {
    std::ofstream f("whisper_debug.log", std::ios::app);
    f << msg << std::endl;
    f.flush();
}

void WhisperSpeech::audioCaptureCallback(void* device, void* output, const void* input, unsigned int frameCount) {
    ma_device* dev = static_cast<ma_device*>(device);
    WhisperSpeech* self = static_cast<WhisperSpeech*>(dev->pUserData);
    if (!self || !input) return;

    const float* samples = static_cast<const float*>(input);
    std::lock_guard<std::mutex> lock(self->m_audioMutex);
    self->m_audioBuffer.insert(self->m_audioBuffer.end(), samples, samples + frameCount);

    // Cap buffer size
    size_t maxSamples = static_cast<size_t>(kSampleRate * kMaxBufferSec);
    if (self->m_audioBuffer.size() > maxSamples) {
        size_t trim = self->m_audioBuffer.size() - maxSamples;
        self->m_audioBuffer.erase(self->m_audioBuffer.begin(),
                                   self->m_audioBuffer.begin() + trim);
        if (self->m_consumedSamples > trim)
            self->m_consumedSamples -= trim;
        else
            self->m_consumedSamples = 0;
    }
}

void WhisperSpeech::enumerateDevices() {
    m_captureDevices.clear();
    ma_context* ctx = static_cast<ma_context*>(m_maContext);
    if (!ctx) return;

    ma_device_info* pCaptureInfos;
    ma_uint32 captureCount;
    if (ma_context_get_devices(ctx, nullptr, nullptr, &pCaptureInfos, &captureCount) != MA_SUCCESS) {
        speechLog("WhisperSpeech: failed to enumerate devices");
        return;
    }

    for (ma_uint32 i = 0; i < captureCount; i++) {
        AudioDeviceInfo info;
        info.name = pCaptureInfos[i].name;
        info.index = (int)i;
        info.isDefault = pCaptureInfos[i].isDefault != 0;
        m_captureDevices.push_back(info);
        speechLog("WhisperSpeech: capture device [" + std::to_string(i) + "] " + info.name +
                  (info.isDefault ? " (DEFAULT)" : ""));
    }
}

bool WhisperSpeech::initCaptureDevice(int deviceIndex) {
    deinitCaptureDevice();

    ma_context* ctx = static_cast<ma_context*>(m_maContext);
    if (!ctx) return false;

    // Get device list to find the ID
    ma_device_info* pCaptureInfos;
    ma_uint32 captureCount;
    if (ma_context_get_devices(ctx, nullptr, nullptr, &pCaptureInfos, &captureCount) != MA_SUCCESS) {
        return false;
    }

    ma_device_config config = ma_device_config_init(ma_device_type_capture);
    config.capture.format = ma_format_f32;
    config.capture.channels = 1;
    config.sampleRate = kSampleRate;
    config.dataCallback = reinterpret_cast<ma_device_data_proc>(audioCaptureCallback);
    config.pUserData = this;
    config.periodSizeInFrames = 512;

    // Select specific device if not default
    if (deviceIndex >= 0 && deviceIndex < (int)captureCount) {
        config.capture.pDeviceID = &pCaptureInfos[deviceIndex].id;
        speechLog("WhisperSpeech: selecting device [" + std::to_string(deviceIndex) + "] " +
                  pCaptureInfos[deviceIndex].name);
    } else {
        speechLog("WhisperSpeech: using default capture device");
    }

    m_captureDevice = new ma_device;
    if (ma_device_init(ctx, &config, static_cast<ma_device*>(m_captureDevice)) != MA_SUCCESS) {
        speechLog("WhisperSpeech: failed to init capture device");
        delete static_cast<ma_device*>(m_captureDevice);
        m_captureDevice = nullptr;
        return false;
    }

    m_deviceInit = true;
    m_selectedDevice = deviceIndex;

    // Log the actual device name
    ma_device* dev = static_cast<ma_device*>(m_captureDevice);
    speechLog("WhisperSpeech: capture device ready: " + std::string(dev->capture.name));
    return true;
}

void WhisperSpeech::deinitCaptureDevice() {
    if (m_deviceInit && m_captureDevice) {
        ma_device_uninit(static_cast<ma_device*>(m_captureDevice));
        delete static_cast<ma_device*>(m_captureDevice);
        m_captureDevice = nullptr;
        m_deviceInit = false;
    }
}

bool WhisperSpeech::selectDevice(int index) {
    if (m_listening.load()) return false; // can't switch while listening
    return initCaptureDevice(index);
}

bool WhisperSpeech::init(const std::string& modelPath) {
    struct whisper_context_params cparams = whisper_context_default_params();
    cparams.use_gpu = true;
    cparams.flash_attn = true;
    m_ctx = whisper_init_from_file_with_params(modelPath.c_str(), cparams);
    if (!m_ctx) {
        speechLog("WhisperSpeech: failed to load model: " + modelPath);
        return false;
    }
    speechLog("WhisperSpeech: model loaded (" + modelPath + ")");

    // Init miniaudio context (shared for enumeration + device creation)
    m_maContext = new ma_context;
    if (ma_context_init(nullptr, 0, nullptr, static_cast<ma_context*>(m_maContext)) != MA_SUCCESS) {
        speechLog("WhisperSpeech: failed to init audio context");
        delete static_cast<ma_context*>(m_maContext);
        m_maContext = nullptr;
        return false;
    }

    // Enumerate available devices
    enumerateDevices();

    // Init default capture device
    if (!initCaptureDevice(-1)) {
        speechLog("WhisperSpeech: failed to init default capture device");
        return false;
    }

    return true;
}

void WhisperSpeech::shutdown() {
    stopListening();
    deinitCaptureDevice();

    if (m_maContext) {
        ma_context_uninit(static_cast<ma_context*>(m_maContext));
        delete static_cast<ma_context*>(m_maContext);
        m_maContext = nullptr;
    }

    if (m_ctx) {
        whisper_free(m_ctx);
        m_ctx = nullptr;
    }
}

void WhisperSpeech::startListening() {
    if (!m_ctx || !m_deviceInit || m_listening.load()) return;

    {
        std::lock_guard<std::mutex> lock(m_audioMutex);
        m_audioBuffer.clear();
        m_consumedSamples = 0;
    }
    m_accumulated.clear();
    {
        std::lock_guard<std::mutex> lock(m_resultMutex);
        m_pendingText.clear();
        m_hasPending = false;
    }

    if (ma_device_start(static_cast<ma_device*>(m_captureDevice)) != MA_SUCCESS) {
        speechLog("WhisperSpeech: failed to start audio capture");
        return;
    }

    m_listening.store(true);
    m_running.store(true);
    m_thread = std::thread(&WhisperSpeech::processLoop, this);
    speechLog("WhisperSpeech: listening started");
}

void WhisperSpeech::stopListening() {
    m_listening.store(false);
    m_running.store(false);

    if (m_thread.joinable()) {
        m_thread.join();
    }

    if (m_deviceInit && m_captureDevice) {
        ma_device_stop(static_cast<ma_device*>(m_captureDevice));
    }
}

void WhisperSpeech::poll() {
    std::lock_guard<std::mutex> lock(m_resultMutex);
    if (m_hasPending && m_callback) {
        m_callback(m_pendingText, false);
        m_hasPending = false;
    }
}

void WhisperSpeech::processLoop() {
    std::string prevTranscript; // last sliding-window transcription for diff

    while (m_running.load()) {
        std::this_thread::sleep_for(std::chrono::milliseconds(kProcessIntervalMs));
        if (!m_running.load()) break;

        // Sliding window: use last kWindowSec of audio for context,
        // but only emit NEW words by diffing against previous transcription
        std::vector<float> audio;
        {
            std::lock_guard<std::mutex> lock(m_audioMutex);
            size_t totalSamples = m_audioBuffer.size();

            // Need enough new audio since last check
            size_t newSamples = (totalSamples > m_consumedSamples)
                ? totalSamples - m_consumedSamples : 0;
            if (newSamples < static_cast<size_t>(kSampleRate * kMinNewAudioSec))
                continue;

            // Take a window of the most recent audio for whisper context
            size_t windowSamples = static_cast<size_t>(kSampleRate * kWindowSec);
            if (totalSamples > windowSamples) {
                audio.assign(m_audioBuffer.end() - windowSamples, m_audioBuffer.end());
            } else {
                audio = m_audioBuffer;
            }
            m_consumedSamples = totalSamples;
        }

        // Whisper requires at least 1 second of audio — pad with silence if needed
        if (audio.size() < static_cast<size_t>(kSampleRate)) {
            audio.insert(audio.begin(), kSampleRate - audio.size(), 0.0f);
        }

        // VAD: compute RMS energy, skip silence
        float sumSq = 0.0f;
        for (size_t i = 0; i < audio.size(); i++) {
            sumSq += audio[i] * audio[i];
        }
        float rms = std::sqrt(sumSq / audio.size());
        if (rms < 0.00005f) continue;

        auto t0 = std::chrono::high_resolution_clock::now();

        whisper_full_params wparams = whisper_full_default_params(WHISPER_SAMPLING_GREEDY);
        wparams.print_progress = false;
        wparams.print_special = false;
        wparams.print_realtime = false;
        wparams.print_timestamps = false;
        wparams.single_segment = true;
        wparams.no_timestamps = true;
        wparams.n_threads = 4;
        wparams.language = "en";
        wparams.suppress_blank = true;
        wparams.suppress_nst = true;
        wparams.no_context = true;
        wparams.max_tokens = 32;
        wparams.temperature = 0.0f;
        wparams.temperature_inc = 0.0f;
        wparams.audio_ctx = (int)(audio.size() * 1500 / (kSampleRate * 30));
        if (wparams.audio_ctx < 64) wparams.audio_ctx = 64;
        if (wparams.audio_ctx > 1500) wparams.audio_ctx = 1500;

        if (whisper_full(m_ctx, wparams, audio.data(), (int)audio.size()) != 0) {
            speechLog("WhisperSpeech: inference failed");
            continue;
        }

        auto t1 = std::chrono::high_resolution_clock::now();
        float inferMs = std::chrono::duration<float, std::milli>(t1 - t0).count();

        // Collect text
        std::string result;
        int nSegments = whisper_full_n_segments(m_ctx);
        for (int i = 0; i < nSegments; i++) {
            const char* text = whisper_full_get_segment_text(m_ctx, i);
            if (text) result += text;
        }

        // Trim whitespace
        size_t start = result.find_first_not_of(" \t\n\r");
        size_t end = result.find_last_not_of(" \t\n\r");
        if (start != std::string::npos) {
            result = result.substr(start, end - start + 1);
        } else {
            result.clear();
        }

        if (result.empty()) continue;

        // Filter common whisper hallucinations
        {
            std::string lower = result;
            std::transform(lower.begin(), lower.end(), lower.begin(),
                           [](unsigned char c) { return (char)std::tolower(c); });
            if (lower.find("thank") != std::string::npos && result.size() < 40) continue;
            if (lower.find("subscribe") != std::string::npos) continue;
            if (lower.find("bye") != std::string::npos && result.size() < 10) continue;
            if (lower == "you" || lower == "." || lower == "..") continue;
            if (lower.find("[") != std::string::npos) continue;
            if (lower.find("(") != std::string::npos && result.size() < 20) continue;
            // Filter music note hallucinations
            bool allJunk = true;
            for (char c : lower) {
                if (std::isalpha((unsigned char)c)) { allJunk = false; break; }
            }
            if (allJunk) continue;
        }

        // Uppercase for shader encoding
        std::transform(result.begin(), result.end(), result.begin(),
                       [](unsigned char c) { return (char)std::toupper(c); });

        // Diff against previous window transcription to find new words
        // If the new result starts with the previous result, extract suffix as new words
        // Otherwise treat the whole result as new (speaker changed topic / window shifted)
        std::string newWords;
        if (!prevTranscript.empty() && result.size() > prevTranscript.size() &&
            result.substr(0, prevTranscript.size()) == prevTranscript) {
            // New words are the suffix
            newWords = result.substr(prevTranscript.size());
            // Trim leading spaces
            size_t s = newWords.find_first_not_of(" ");
            if (s != std::string::npos) newWords = newWords.substr(s);
            else newWords.clear();
        } else if (result != prevTranscript) {
            // Window shifted significantly — take the whole new result
            // but only if it's actually different from last time
            newWords = result;
        }
        prevTranscript = result;

        if (newWords.empty()) continue;

        // Append to accumulated transcript
        if (!m_accumulated.empty()) m_accumulated += " ";
        m_accumulated += newWords;

        speechLog("WhisperSpeech: [" + std::to_string((int)inferMs) + "ms] +" + newWords + " => " + m_accumulated);

        // Queue accumulated text for main thread
        {
            std::lock_guard<std::mutex> lock(m_resultMutex);
            m_pendingText = m_accumulated;
            m_hasPending = true;
        }
    }
}

#endif // HAS_WHISPER
