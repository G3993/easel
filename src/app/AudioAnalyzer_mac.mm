#ifdef __APPLE__
#include "app/AudioAnalyzer.h"
#include <ScreenCaptureKit/ScreenCaptureKit.h>
#include <CoreMedia/CoreMedia.h>
#include <AudioToolbox/AudioToolbox.h>
#include <iostream>
#include <mutex>
#include <vector>

// ─── ScreenCaptureKit audio delegate ──────────────────────────────

@interface EaselAudioCaptureDelegate : NSObject <SCStreamOutput>
@property (nonatomic) std::mutex* bufferMutex;
@property (nonatomic) std::vector<float>* sampleBuffer;
@end

@implementation EaselAudioCaptureDelegate
- (void)stream:(SCStream *)stream didOutputSampleBuffer:(CMSampleBufferRef)sampleBuffer ofType:(SCStreamOutputType)type {
    if (type != SCStreamOutputTypeAudio) return;

    CMBlockBufferRef blockBuffer = CMSampleBufferGetDataBuffer(sampleBuffer);
    if (!blockBuffer) return;

    // Get audio format
    CMFormatDescriptionRef formatDesc = CMSampleBufferGetFormatDescription(sampleBuffer);
    if (!formatDesc) return;

    const AudioStreamBasicDescription* asbd = CMAudioFormatDescriptionGetStreamBasicDescription(formatDesc);
    if (!asbd) return;

    size_t totalBytes = 0;
    char* dataPointer = nullptr;
    OSStatus status = CMBlockBufferGetDataPointer(blockBuffer, 0, nullptr, &totalBytes, &dataPointer);
    if (status != noErr || !dataPointer || totalBytes == 0) return;

    int channels = (int)asbd->mChannelsPerFrame;
    if (channels < 1) channels = 1;

    // ScreenCaptureKit delivers 32-bit float PCM
    const float* floatData = (const float*)dataPointer;
    int totalSamples = (int)(totalBytes / sizeof(float));
    int frames = totalSamples / channels;

    // Mix down to mono
    std::lock_guard<std::mutex> lock(*self.bufferMutex);
    for (int i = 0; i < frames; i++) {
        float mono = 0.0f;
        for (int ch = 0; ch < channels; ch++) {
            mono += floatData[i * channels + ch];
        }
        mono /= channels;
        self.sampleBuffer->push_back(mono);
    }
}
@end

// ─── Internal state ──────────────────────────────────────────────

struct MacAudioState {
    SCStream* stream = nil;
    EaselAudioCaptureDelegate* delegate = nil;
    dispatch_queue_t queue = nil;
    std::mutex bufferMutex;
    std::vector<float> sampleBuffer;
};

// ─── AudioAnalyzer macOS implementation ─────────────────────────

void AudioAnalyzer::initCapture() {
    cleanupCapture();

    auto* state = new MacAudioState();
    m_macAudioImpl = state;

    dispatch_semaphore_t sem = dispatch_semaphore_create(0);
    __block bool success = false;

    [SCShareableContent getShareableContentWithCompletionHandler:^(SCShareableContent* content, NSError* error) {
        if (error || !content) {
            std::cerr << "[AudioAnalyzer] Failed to get shareable content: "
                      << (error ? error.localizedDescription.UTF8String : "unknown") << std::endl;
            dispatch_semaphore_signal(sem);
            return;
        }

        // We need a display to create a content filter, but we only want audio
        SCDisplay* display = content.displays.firstObject;
        if (!display) {
            std::cerr << "[AudioAnalyzer] No displays found for audio capture" << std::endl;
            dispatch_semaphore_signal(sem);
            return;
        }

        // Configure for audio-only capture
        SCStreamConfiguration* config = [[SCStreamConfiguration alloc] init];
        config.capturesAudio = YES;
        config.excludesCurrentProcessAudio = NO;
        config.sampleRate = 48000;
        config.channelCount = 2;

        // Minimize video overhead — we only want audio but SCStream requires a display filter
        config.width = 2;
        config.height = 2;
        config.minimumFrameInterval = CMTimeMake(1, 1); // 1 fps minimum video

        // Capture all system audio from this display
        SCContentFilter* filter = [[SCContentFilter alloc] initWithDisplay:display excludingWindows:@[]];

        state->stream = [[SCStream alloc] initWithFilter:filter configuration:config delegate:nil];
        state->delegate = [[EaselAudioCaptureDelegate alloc] init];
        state->delegate.bufferMutex = &state->bufferMutex;
        state->delegate.sampleBuffer = &state->sampleBuffer;

        state->queue = dispatch_queue_create("com.easel.audiocapture", DISPATCH_QUEUE_SERIAL);

        NSError* addError = nil;
        [state->stream addStreamOutput:state->delegate type:SCStreamOutputTypeAudio sampleHandlerQueue:state->queue error:&addError];
        if (addError) {
            std::cerr << "[AudioAnalyzer] Failed to add audio output: " << addError.localizedDescription.UTF8String << std::endl;
            dispatch_semaphore_signal(sem);
            return;
        }

        [state->stream startCaptureWithCompletionHandler:^(NSError* startError) {
            if (startError) {
                std::cerr << "[AudioAnalyzer] Failed to start audio capture: " << startError.localizedDescription.UTF8String << std::endl;
            } else {
                success = true;
                std::cout << "[AudioAnalyzer] macOS system audio capture started (48kHz stereo -> mono)" << std::endl;
            }
            dispatch_semaphore_signal(sem);
        }];
    }];

    dispatch_semaphore_wait(sem, dispatch_time(DISPATCH_TIME_NOW, 5 * NSEC_PER_SEC));

    if (success) {
        m_initialized = true;
        m_sampleRate = 48000;
        m_channels = 2;
    } else {
        std::cerr << "[AudioAnalyzer] macOS audio capture failed — FFT will not update" << std::endl;
        cleanupCapture();
    }
}

void AudioAnalyzer::cleanupCapture() {
    if (m_macAudioImpl) {
        auto* state = (MacAudioState*)m_macAudioImpl;
        if (state->stream) {
            dispatch_semaphore_t sem = dispatch_semaphore_create(0);
            [state->stream stopCaptureWithCompletionHandler:^(NSError* error) {
                dispatch_semaphore_signal(sem);
            }];
            dispatch_semaphore_wait(sem, dispatch_time(DISPATCH_TIME_NOW, 2 * NSEC_PER_SEC));
        }
        delete state;
        m_macAudioImpl = nullptr;
    }
    m_initialized = false;
    m_samplesAccumulated = 0;
}

void AudioAnalyzer::drainPackets() {
    if (!m_macAudioImpl) return;
    auto* state = (MacAudioState*)m_macAudioImpl;

    std::vector<float> samples;
    {
        std::lock_guard<std::mutex> lock(state->bufferMutex);
        samples.swap(state->sampleBuffer);
    }

    for (float s : samples) {
        m_ringBuf[m_ringPos] = s;
        m_ringPos = (m_ringPos + 1) % kFFTSize;
        m_samplesAccumulated++;
    }
}
#endif
