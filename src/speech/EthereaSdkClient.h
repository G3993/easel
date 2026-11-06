#pragma once
#include <atomic>
#include <condition_variable>
#include <cstdint>
#include <memory>
#include <mutex>
#include <string>
#include <thread>

// Control connection to the Easel agent SDK (the same /api/actions surface
// Paseo and the phone use) for steering Tulip's Etherea show from Easel:
//
//   prompt       -> cue.transcript.send {text, source:"easel", isFinal, newScene}
//   abstraction  -> model.primary.status (read) / model.primary.strength.set (write)
//
// This is separate from EthereaClient (the transcript websocket/SSE feed).
// All HTTP happens on worker threads; the UI reads snapshot() and calls the
// gesture / prompt methods, none of which block.
//
// Nothing is ever written on connect or panel open: status is polled only
// while the panel is visible (heartbeat()), and strength writes happen only
// inside a user drag gesture. Writes carry X-Easel-House-Act: 1; polls don't.
class EthereaSdkClient {
public:
    struct Snapshot {
        // connection
        std::string baseUrl;
        bool hasKey = false;
        bool statusLoaded = false;   // at least one good status since (re)connect
        bool reachable = false;
        bool authFailed = false;     // 401 — polling stopped until key/url change
        std::string connError;
        double statusAgeSec = -1;

        // model.primary.status
        std::string provider;
        std::string providerLabel;
        bool supportsStrength = false;
        bool available = false;
        std::string observedStatus;
        std::string mode;            // capabilities.strengthMode / mode, informational
        double uiMax = 1.0;          // capabilities.strengthUiMax
        double uiExp = 1.0;          // capabilities.strengthUiExponent
        bool hasTarget = false, hasValue = false, hasSettled = false;
        double target = 0, value = 0;
        long long revision = -1;
        bool settled = false;
        std::string operationSummary; // last operation history, NOT live state

        // strength gesture
        bool inputEnabled = false;
        bool gestureLocal = false;   // UI should show localValue, not readback
        double localValue = 0;       // wire value 0..1
        std::string writeMsg;
        bool writeError = false;

        // prompt
        bool promptInFlight = false;
        uint64_t promptSeq = 0;      // bumps when a send finishes
        std::string promptKind;      // changed | accepted | heard | refused | error | timeout
        std::string promptText;      // human-readable outcome
        std::string appliedPrompt;   // prompt Cue actually generated (video.* tool)
        bool clearDraft = false;     // delivered: safe to clear the draft
    };

    EthereaSdkClient();
    ~EthereaSdkClient();

    void start();
    void stop();

    // (Re)configure. Clears queued writes and status; nothing is sent until
    // the panel's next heartbeat triggers a read.
    void setConnection(const std::string& baseUrl, const std::string& key);
    void heartbeat();          // call each frame the control panel is visible
    void requestRefresh();     // one status read now (also clears authFailed)

    // Abstraction slider (wire values 0..1).
    bool beginGesture();       // false = input not enabled right now
    void setGestureValue(double wire);
    void endGesture();

    // Prompt. Returns false if a send is already in flight or text is empty.
    bool sendPrompt(const std::string& text, bool newScene);

    Snapshot snapshot() const;

    // Key resolution: EASEL_AGENT_API_KEY env, else ~/.easel/etherea_sdk_key.
    static std::string loadKey();
    static bool saveKey(const std::string& key);   // writes ~/.easel/etherea_sdk_key (0600)

private:
    struct PromptState;
    void workerLoop();
    void doPoll();
    void doWrite(double value, const std::string& provider, uint64_t gestureId);
    void cancelGestureLocked(const std::string& why, bool error);

    mutable std::mutex m_mx;
    std::condition_variable m_cv;
    std::thread m_worker;
    std::atomic<bool> m_running{false};

    std::string m_baseUrl, m_key;
    uint64_t m_connGen = 0;    // bumps on setConnection; stale results are dropped

    Snapshot m_s;
    double m_lastHeartbeat = -1e9, m_lastPollDone = -1e9, m_statusTime = -1e9;
    bool m_refreshRequested = false;
    bool m_needFreshStatus = true;   // input stays disabled until a new read lands
    uint64_t m_pollSeqStarted = 0, m_pollSeqDone = 0;

    // gesture
    uint64_t m_gestureId = 0;
    std::string m_gestureProvider;
    bool m_dragging = false, m_hasPending = false, m_inFlight = false, m_canceled = false;
    double m_pendingValue = 0;
    double m_lastSend = -1e9;
    uint64_t m_holdUntilPollSeq = 0;  // show local value until this poll lands

    std::shared_ptr<PromptState> m_prompt;
};
