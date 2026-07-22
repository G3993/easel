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
    // Session generation — bumped by every stop(). Each recognition task's
    // resultHandler stamps the generation it was created under and refuses
    // to deliver once it changes: Apple can hold a task open past stop(),
    // and continuous mode restarts the session every utterance, so a stale
    // task must not inject partials/finals into the next session.
    std::atomic<uint64_t>                     generation{0};
    std::function<void(const std::string&)>   onPartial;
    std::function<void(const std::string&)>   onFinal;

    ~Impl() {
        // Runs when the LAST shared owner drops — possibly a late
        // resultHandler block, after ~MacSpeechRecognizer already returned.
        // Project compiles without ARC — `nil` doesn't release. Manual.
        [recognizer release];
        recognizer = nil;
        // Releasing a queue from a block running on it is fine: GCD holds
        // its own reference for the duration of the block.
        if (q) dispatch_release(q);
    }
};

MacSpeechRecognizer::MacSpeechRecognizer() : m_impl(std::make_shared<Impl>()) {
    m_impl->q = dispatch_queue_create("easel.voice.engine", DISPATCH_QUEUE_SERIAL);
    NSLocale* loc = [NSLocale localeWithLocaleIdentifier:@"en-US"];
    m_impl->recognizer = [[SFSpeechRecognizer alloc] initWithLocale:loc];
    if (!m_impl->recognizer || !m_impl->recognizer.isAvailable) {
        NSLog(@"[Voice] SFSpeechRecognizer unavailable for en-US");
        return;
    }
    // Blocks copy the shared_ptr (Block_copy runs C++ copy ctors), so every
    // escaped block below co-owns Impl — see the header comment on m_impl.
    std::shared_ptr<Impl> impl = m_impl;
    [SFSpeechRecognizer requestAuthorization:^(SFSpeechRecognizerAuthorizationStatus s) {
        bool ok = (s == SFSpeechRecognizerAuthorizationStatusAuthorized);
        if (!ok) NSLog(@"[Voice] Speech auth denied (%ld)", (long)s);
        impl->authorized.store(ok, std::memory_order_relaxed);
    }];
}

MacSpeechRecognizer::~MacSpeechRecognizer() {
    if (m_impl) {
        if (m_impl->recording.load()) stop();
        // Drain the engine queue so the queued teardown (engine/request/task
        // cancel) lands before we drop our reference. Apple may still fire
        // one last resultHandler after the cancel — that's safe: the handler
        // holds its own shared_ptr to Impl (so Impl and the queue outlive
        // us exactly as long as needed) and its delivery is rejected by the
        // generation check. Recognizer/queue release happen in ~Impl, with
        // the last owner.
        dispatch_sync(m_impl->q, ^{});
        m_impl.reset();
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

    // Callbacks are captured here, synchronously, so callers see start()-
    // time values — but the write into impl happens ON the engine queue,
    // because marshaled resultHandlers read impl->onPartial/onFinal on
    // that same queue (a stale handler could otherwise race this write).
    std::function<void(const std::string&)> partialCb = onPartial;
    std::function<void(const std::string&)> finalCb   = onFinal;

    std::shared_ptr<Impl> impl = m_impl;
    dispatch_async(impl->q, ^{
        // A stop() may have landed between the request and this block.
        if (!impl->recording.load()) return;
        if (impl->engine) return; // already running
        impl->onPartial = partialCb;
        impl->onFinal   = finalCb;

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

        // Stamp this session's generation (see Impl::generation). Read on
        // the serial queue, after any preceding stop()'s bump is visible.
        const uint64_t gen = impl->generation.load(std::memory_order_relaxed);
        impl->task = [impl->recognizer
            recognitionTaskWithRequest:impl->request
                         resultHandler:^(SFSpeechRecognitionResult* result, NSError* taskErr) {
            if (taskErr) {
                // Cancellation (every stop() cancels the task) lands here —
                // quiet return so it's not noisy.
                return;
            }
            if (!result) return;
            NSString* str = result.bestTranscription.formattedString;
            if (!str) return;
            std::string s = std::string([str UTF8String]);
            bool isFinal = result.isFinal;
            // Apple invokes this on its own internal queue, racing stop().
            // Marshal onto the engine queue (serialized with start/stop) and
            // re-check the generation there: once stop() has bumped it, a
            // result from the old task is dropped instead of delivered into
            // whatever session is live now.
            dispatch_async(impl->q, ^{
                if (impl->generation.load(std::memory_order_relaxed) != gen) return;
                if (isFinal) { if (impl->onFinal)   impl->onFinal(s); }
                else         { if (impl->onPartial) impl->onPartial(s); }
            });
        }];
    });
}

void MacSpeechRecognizer::stop() {
    if (!m_impl) return;
    if (!m_impl->recording.exchange(false)) return;

    // Invalidate the live session's results immediately: anything Apple
    // delivers from here on fails the generation check in the
    // resultHandler and is dropped, so a stop→final race (or a stale
    // continuous-mode task) can't fire callbacks into the next session.
    // Both product paths consume onFinal BEFORE calling stop (continuous
    // mode restarts off onFinal; manual stop discards the tail), so
    // nothing wanted is lost.
    m_impl->generation.fetch_add(1, std::memory_order_relaxed);

    // Teardown rides the same serial queue as start, so a queued start
    // always fully precedes its matching teardown and vice versa.
    std::shared_ptr<Impl> impl = m_impl;
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
        // Actively cancel — endAudio alone leaves the task running until
        // Apple decides it's done, and a lingering task keeps delivering
        // (and holding Speech.framework state) long after stop. The task is
        // owned by the recognizer; don't release it ourselves.
        if (impl->task) [impl->task cancel];
        impl->task = nil;
    });
}
