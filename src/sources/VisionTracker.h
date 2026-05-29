#pragma once
#include <atomic>
#include <mutex>

// Apple Vision-based body / hand / face tracking. Owns its own
// AVCaptureSession (independent of the OpenCV webcam + scene scanner) so
// it can run alongside whatever else has a camera open. Pushes normalized
// [0,1] signals that map 1:1 onto the DataBus `vision.*` keys, so shaders
// bound to "Pinch", "Head X", etc. react to the live camera.
//
// Modeled on ShaderClaw3's MediaPipe "Body Tracking" toggle: a master
// on/off plus per-mode toggles (Hand / Face / Pose). macOS-native via the
// Vision framework — no MediaPipe/Bazel dependency. The header is pure
// C++ so Application.cpp (compiled as C++) can include it; the impl lives
// in VisionTracker.mm (Objective-C++).
class VisionTracker {
public:
    // Normalized signal snapshot. All positions are [0,1] with origin at
    // the TOP-LEFT (Y flipped from Vision's bottom-left convention to
    // match shader UV expectations / ShaderClaw3 parity).
    struct Signals {
        // Hands
        float handCount  = 0.0f;
        float leftHandX  = 0.5f, leftHandY  = 0.5f;
        float rightHandX = 0.5f, rightHandY = 0.5f;
        float pinch      = 0.0f;  // 1 = thumb+index touching, 0 = far apart
        // Pose
        float poseConfidence = 0.0f;
        float headX = 0.5f, headY = 0.5f;
        // Face
        float faceDetected = 0.0f;
        float smile        = 0.0f;
    };

    VisionTracker() = default;
    ~VisionTracker();

    // Start/stop the capture session. start() requests camera access if
    // not yet granted; returns false if the device/permission is missing.
    bool start();
    void stop();
    bool isRunning() const { return m_running.load(); }

    // Per-mode toggles — mirror ShaderClaw3's Hand / Face / Pose buttons.
    void setHandEnabled(bool e) { m_handEnabled.store(e); }
    void setFaceEnabled(bool e) { m_faceEnabled.store(e); }
    void setPoseEnabled(bool e) { m_poseEnabled.store(e); }
    bool handEnabled() const { return m_handEnabled.load(); }
    bool faceEnabled() const { return m_faceEnabled.load(); }
    bool poseEnabled() const { return m_poseEnabled.load(); }

    // Thread-safe snapshot of the latest analyzed frame's signals.
    Signals signals() const;

    // Called by the capture delegate (impl side) to publish results.
    void publish(const Signals& s);

private:
    std::atomic<bool> m_running{false};
    std::atomic<bool> m_handEnabled{true};
    std::atomic<bool> m_faceEnabled{false};
    std::atomic<bool> m_poseEnabled{false};

    mutable std::mutex m_mutex;
    Signals m_signals;

    // Opaque pointer to the Objective-C capture session holder. Defined
    // in VisionTracker.mm; nullptr when stopped.
    void* m_impl = nullptr;
};
