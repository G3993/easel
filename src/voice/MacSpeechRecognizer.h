#pragma once
#include <string>
#include <functional>

// Push-to-talk wrapper around macOS SFSpeechRecognizer + AVAudioEngine.
//
// Lifecycle: ::available()? → start() while user holds the mic key/button →
// stop() on release. Final transcript fires via the onFinal callback.
// Partial results stream via onPartial as the user speaks (used for the
// floating pill so they see what's being heard before they release).
//
// First call triggers two TCC permission prompts (mic + speech recognition).
// Implementation lives in MacSpeechRecognizer.mm.
//
// Failure modes are silent: if permissions are denied or the system
// recognizer is unavailable, available() returns false, start()/stop() are
// no-ops, and the user can still type into the command bar.
class MacSpeechRecognizer {
public:
    MacSpeechRecognizer();
    ~MacSpeechRecognizer();

    // Returns true once both authorizations (mic + speech) are granted.
    bool available() const;

    // Begin a recognition session. Audio engine starts capturing immediately;
    // partial results stream via onPartial. Idempotent — calling start() while
    // already recording is a no-op.
    void start();
    // End the current session. The final transcript (if any) fires via
    // onFinal. After stop(), start() can be called again.
    void stop();

    bool isRecording() const;

    std::function<void(const std::string&)> onPartial;
    std::function<void(const std::string&)> onFinal;

private:
    struct Impl;
    Impl* m_impl = nullptr;
};
