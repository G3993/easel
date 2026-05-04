#include "voice/MacSpeechRecognizer.h"

#import <Speech/Speech.h>
#import <AVFoundation/AVFoundation.h>

#include <atomic>

struct MacSpeechRecognizer::Impl {
    SFSpeechRecognizer*                       recognizer = nil;
    AVAudioEngine*                            engine     = nil;
    SFSpeechAudioBufferRecognitionRequest*    request    = nil;
    SFSpeechRecognitionTask*                  task       = nil;
    std::atomic<bool>                         authorized{false};
    std::atomic<bool>                         recording {false};
    std::function<void(const std::string&)>   onPartial;
    std::function<void(const std::string&)>   onFinal;
};

MacSpeechRecognizer::MacSpeechRecognizer() : m_impl(new Impl()) {
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
    if (m_impl->recording.load()) return;

    m_impl->onPartial = onPartial;
    m_impl->onFinal   = onFinal;

    NSError* err = nil;
    m_impl->engine  = [[AVAudioEngine alloc] init];
    m_impl->request = [[SFSpeechAudioBufferRecognitionRequest alloc] init];
    m_impl->request.shouldReportPartialResults = YES;

    AVAudioInputNode* input = m_impl->engine.inputNode;
    AVAudioFormat* fmt = [input outputFormatForBus:0];
    [input installTapOnBus:0 bufferSize:1024 format:fmt
                     block:^(AVAudioPCMBuffer* buf, AVAudioTime* /*when*/) {
        if (m_impl->request) [m_impl->request appendAudioPCMBuffer:buf];
    }];

    [m_impl->engine prepare];
    [m_impl->engine startAndReturnError:&err];
    if (err) {
        NSLog(@"[Voice] AVAudioEngine start failed: %@", err.localizedDescription);
        [input removeTapOnBus:0];
        // Manual release — both were +1 retained by alloc/init above and
        // never make it into the recording state where stop() releases them.
        [m_impl->engine release];   m_impl->engine  = nil;
        [m_impl->request release];  m_impl->request = nil;
        return;
    }

    Impl* impl = m_impl;
    m_impl->task = [m_impl->recognizer
        recognitionTaskWithRequest:m_impl->request
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

    m_impl->recording.store(true, std::memory_order_relaxed);
}

void MacSpeechRecognizer::stop() {
    if (!m_impl || !m_impl->recording.load()) return;

    // No ARC — every nil-assignment without a matching release leaks.
    // AVAudioEngine + SFSpeechAudioBufferRecognitionRequest are heavy
    // (Core Audio buffers + Speech.framework state); a leak per tap of
    // the mic adds up in single-megabyte chunks.
    if (m_impl->engine) {
        [m_impl->engine stop];
        [m_impl->engine.inputNode removeTapOnBus:0];
        [m_impl->engine release];
        m_impl->engine = nil;
    }
    if (m_impl->request) {
        [m_impl->request endAudio];
        [m_impl->request release];
        m_impl->request = nil;
    }
    // The task is owned by the recognizer; don't release it ourselves.
    // It self-releases after isFinal fires.
    m_impl->task = nil;
    m_impl->recording.store(false, std::memory_order_relaxed);
}
