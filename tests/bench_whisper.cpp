#include "whisper.h"
#include <vector>
#include <chrono>
#include <iostream>
#include <cmath>
#include <string>
#include <numeric>
#include <algorithm>

static const int SAMPLE_RATE = 16000;

// Generate a simple tone burst to simulate speech-like audio
static std::vector<float> generateTestAudio(float durationSec) {
    int n = static_cast<int>(SAMPLE_RATE * durationSec);
    std::vector<float> audio(n);
    for (int i = 0; i < n; i++) {
        float t = static_cast<float>(i) / SAMPLE_RATE;
        // Mix of frequencies to simulate speech spectrum
        audio[i] = 0.1f * (
            std::sin(2.0f * 3.14159f * 200.0f * t) +
            0.5f * std::sin(2.0f * 3.14159f * 500.0f * t) +
            0.3f * std::sin(2.0f * 3.14159f * 1200.0f * t) +
            0.2f * std::sin(2.0f * 3.14159f * 2400.0f * t)
        );
    }
    return audio;
}

int main(int argc, char** argv) {
    std::string modelPath = "models/ggml-tiny.en.bin";
    if (argc > 1) modelPath = argv[1];

    std::cout << "=== Whisper Benchmark ===" << std::endl;
    std::cout << "Model: " << modelPath << std::endl;

    // Load model with GPU + flash attention
    struct whisper_context_params cparams = whisper_context_default_params();
    cparams.use_gpu = true;
    cparams.flash_attn = true;

    auto t0 = std::chrono::high_resolution_clock::now();
    whisper_context* ctx = whisper_init_from_file_with_params(modelPath.c_str(), cparams);
    auto t1 = std::chrono::high_resolution_clock::now();

    if (!ctx) {
        std::cerr << "Failed to load model" << std::endl;
        return 1;
    }

    float loadMs = std::chrono::duration<float, std::milli>(t1 - t0).count();
    std::cout << "Model load: " << (int)loadMs << " ms" << std::endl;
    std::cout << std::endl;

    // Test different audio durations (simulating the sliding window)
    float durations[] = { 1.0f, 2.0f, 4.0f, 6.0f, 8.0f };
    int warmup = 2;
    int runs = 5;

    for (float dur : durations) {
        auto audio = generateTestAudio(dur);

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
        wparams.audio_ctx = (int)(audio.size() * 1500 / (SAMPLE_RATE * 30));
        if (wparams.audio_ctx < 64) wparams.audio_ctx = 64;
        if (wparams.audio_ctx > 1500) wparams.audio_ctx = 1500;

        // Warmup
        for (int i = 0; i < warmup; i++) {
            whisper_full(ctx, wparams, audio.data(), (int)audio.size());
        }

        // Timed runs
        std::vector<float> times;
        for (int i = 0; i < runs; i++) {
            auto a = std::chrono::high_resolution_clock::now();
            whisper_full(ctx, wparams, audio.data(), (int)audio.size());
            auto b = std::chrono::high_resolution_clock::now();
            times.push_back(std::chrono::duration<float, std::milli>(b - a).count());
        }

        float avg = std::accumulate(times.begin(), times.end(), 0.0f) / times.size();
        float minT = *std::min_element(times.begin(), times.end());
        float maxT = *std::max_element(times.begin(), times.end());
        float rtf = (avg / 1000.0f) / dur; // real-time factor

        std::cout << dur << "s audio | avg " << (int)avg << " ms"
                  << " | min " << (int)minT << " ms"
                  << " | max " << (int)maxT << " ms"
                  << " | RTF " << rtf
                  << " | audio_ctx " << wparams.audio_ctx
                  << std::endl;
    }

    std::cout << std::endl;
    std::cout << "RTF < 1.0 = faster than real-time" << std::endl;

    whisper_free(ctx);
    return 0;
}
