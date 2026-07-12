#include "voice/MacSpeechRecognizer.h"

#import <Speech/Speech.h>
#import <AVFoundation/AVFoundation.h>

#include <atomic>

struct MacSpeechRecognizer::Impl {
    SFSpeechRecognizer*                       recognizer = nil;
    AVAudioEngine*                            engine     = nil;
    SFSpeechAudioBufferRecognitionRequest*    request    = nil;
    SFSpeechRecognitionTask*                  task       = nil;
    // All engine/request/task mutation happens on this serial queue.
    // AVAudioEngine start can block for SECONDS inside CoreAudio's HAL
    // (device wake, contention); doing that on the render thread froze
    // the whole app at launch and on every continuous-mic restart.
    dispatch_queue_t                          q          = nullptr;
    std::atomic<bool>                         authorized{false};
    // "recording" = intent: set the moment start() is requested, cleared
    // on stop() or when the async start fails. Callers keep the same
    // synchronous view they had before the engine work went async.
    std::atomic<bool>                         recording {false};
    std::function<void(const std::string&)>   onPartial;
    std::function<void(const std::string&)>   onFinal;
};

MacSpeechRecognizer::MacSpeechRecognizer() : m_impl(new Impl()) {
    m_impl->q = dispatch_queue_create("easel.voice.engine", DISPATCH_QUEUE_SERIAL);
    NSLocale* loc = [NSLocale localeWithLocaleIdentifier:@"en-US"];
    m_impl->recognizer = [[SFSpeechRecognizer alloc] initWithLocale:loc];
    if (!m_impl->recognizer || !m_impl->recognizer.isAvailable) {
        NSLog(@"[Voice] SFSpeechRecognizer unavailable for en-US");
        return;
    }
    Impl* impl = m_impl;
    [SFSpeechRecognizer requestAuthorization:^(SFSpeechRecognizerAuthorizationStatus s) {
        bool ok = (s == SFSpeechRecognizerAuthorizationStatusAuthorized);
        if (!ok) NSLog(@"[Voice] Speech auth denied (%ld)", (long)s);
        impl->authorized.store(ok, std::memory_order_relaxed);
    }];
}

MacSpeechRecognizer::~MacSpeechRecognizer() {
    if (m_impl) {
        if (m_impl->recording.load()) stop();
        // Drain the engine queue so no async start/stop block can touch
        // m_impl after we delete it.
        dispatch_sync(m_impl->q, ^{});
        dispatch_release(m_impl->q);
        // Project compiles without ARC — `nil` doesn't release. Manual.
        [m_impl->recognizer release];
        m_impl->recognizer = nil;
        delete m_impl;
        m_impl = nullptr;
    }
}

bool MacSpeechRecognizer::available() const {
    return m_impl && m_impl->authorized.load(std::memory_order_relaxed) &&
           m_impl->recognizer && m_impl->recognizer.isAvailable;
}

bool MacSpeechRecognizer::isRecording() const {
    return m_impl && m_impl->recording.load(std::memory_order_relaxed);
}

void MacSpeechRecognizer::start() {
    if (!available()) return;
    // exchange() doubles as the re-entry guard while a start is inflight
    // on the engine queue.
    if (m_impl->recording.exchange(true)) return;

    // Callbacks are copied here, synchronously, before the async block —
    // the resultHandler only ever reads them, so there is no race.
    m_impl->onPartial = onPartial;
    m_impl->onFinal   = onFinal;

    Impl* impl = m_impl;
    dispatch_async(impl->q, ^{
        // A stop() may have landed between the request and this block.
        if (!impl->recording.load()) return;
        if (impl->engine) return; // already running

        NSError* err = nil;
        impl->engine  = [[AVAudioEngine alloc] init];
        impl->request = [[SFSpeechAudioBufferRecognitionRequest alloc] init];
        impl->request.shouldReportPartialResults = YES;

        AVAudioInputNode* input = impl->engine.inputNode;
        AVAudioFormat* fmt = [input outputFormatForBus:0];
        [input installTapOnBus:0 bufferSize:1024 format:fmt
                         block:^(AVAudioPCMBuffer* buf, AVAudioTime* /*when*/) {
            if (impl->request) [impl->request appendAudioPCMBuffer:buf];
        }];

        [impl->engine prepare];
        [impl->engine startAndReturnError:&err];
        if (err) {
            NSLog(@"[Voice] AVAudioEngine start failed: %@", err.localizedDescription);
            [input removeTapOnBus:0];
            // Manual release — both were +1 retained by alloc/init above and
            // never make it into the recording state where stop() releases them.
            [impl->engine release];   impl->engine  = nil;
            [impl->request release];  impl->request = nil;
            impl->recording.store(false, std::memory_order_relaxed);
            return;
        }

        impl->task = [impl->recognizer
            recognitionTaskWithRequest:impl->request
                         resultHandler:^(SFSpeechRecognitionResult* result, NSError* taskErr) {
            if (taskErr) {
                // Cancellation while idle fires here — quiet log so it's not noisy.
                return;
            }
            if (!result) return;
            NSString* str = result.bestTranscription.formattedString;
            if (!str) return;
            std::string s = std::string([str UTF8String]);
            if (result.isFinal) { if (impl->onFinal)   impl->onFinal(s); }
            else                { if (impl->onPartial) impl->onPartial(s); }
        }];
    });
}

void MacSpeechRecognizer::stop() {
    if (!m_impl) return;
    if (!m_impl->recording.exchange(false)) return;

    // Teardown rides the same serial queue as start, so a queued start
    // always fully precedes its matching teardown and vice versa.
    Impl* impl = m_impl;
    dispatch_async(impl->q, ^{
        // No ARC — every nil-assignment without a matching release leaks.
        // AVAudioEngine + SFSpeechAudioBufferRecognitionRequest are heavy
        // (Core Audio buffers + Speech.framework state); a leak per tap of
        // the mic adds up in single-megabyte chunks.
        if (impl->engine) {
            [impl->engine stop];
            [impl->engine.inputNode removeTapOnBus:0];
            [impl->engine release];
            impl->engine = nil;
        }
        if (impl->request) {
            [impl->request endAudio];
            [impl->request release];
            impl->request = nil;
        }
        // The task is owned by the recognizer; don't release it ourselves.
        // It self-releases after isFinal fires.
        impl->task = nil;
    });
}
