#pragma once
#include <string>
#include <functional>
#include <vector>
#include <mutex>
#include <thread>
#include <atomic>

struct EthereaSession {
    std::string id;
    float idleSeconds = 0;
    int transcriptLength = 0;
    bool isPaused = false;
};

class EthereaTranscript {
public:
    // Connect to SSE stream, optionally with a specific session
    bool connect(const std::string& baseUrl = "http://localhost:7860", const std::string& sessionId = "");
    void disconnect();
    bool isConnected() const { return m_connected.load(); }
    bool isRunning() const { return m_running.load(); }

    void setCallback(std::function<void(const std::string&, bool)> cb) { m_callback = std::move(cb); }

    // Call from main thread each frame to dispatch queued results
    void poll();

    // Get latest transcript for UI display
    std::string lastTranscript() const { std::lock_guard<std::mutex> lock(m_resultMutex); return m_lastTranscript; }

    // Fetch available sessions from Etherea (blocking HTTP call — call from background or sparingly)
    static std::vector<EthereaSession> fetchSessions(const std::string& baseUrl = "http://localhost:7860");

private:
    std::function<void(const std::string&, bool)> m_callback;
    std::atomic<bool> m_connected{false};
    std::atomic<bool> m_running{false};
    std::thread m_thread;

    // Thread-safe result queue
    mutable std::mutex m_resultMutex;
    std::string m_pendingText;
    std::string m_lastTranscript;
    bool m_hasPending = false;

    std::string m_baseUrl;
    std::string m_sessionId;

    void sseLoop();
};
