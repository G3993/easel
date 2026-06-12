#pragma once
#include <string>
#include <atomic>
#include <mutex>
#include <thread>

// AI shader "push further" — sends a shader's source + a text instruction
// (and optionally a second shader to combine ideas from) to the Claude API,
// gets back an improved ISF .fs, and writes it next to the original. The
// network call runs on a background thread; the result is compile-tested on
// the GL thread by the caller (ShaderSource needs a current GL context).
//
// Key: reads ANTHROPIC_API_KEY from the environment, falling back to a
// `ANTHROPIC_API_KEY=...` line in <repoRoot>/.env.
class ShaderImprover {
public:
    enum class Status { Idle, Working, ReadyToCompile, Done, Error };

    ~ShaderImprover() { if (m_thread.joinable()) m_thread.join(); }

    // Kick off a generation. `shadersDir` is where the new .fs is written.
    // `combineWithPath` may be empty. No-op if already Working.
    void request(const std::string& shaderPath,
                 const std::string& instruction,
                 const std::string& combineWithPath,
                 const std::string& shadersDir);

    Status status() const { return m_status.load(); }

    // Thread-safe snapshots for the UI / finalize step.
    std::string resultPath();   // temp candidate .fs the worker wrote
    std::string origPath();     // the original .fs to overwrite (build on top)
    std::string message();      // status / error text for display

    // Caller (GL thread) sets the terminal state after compile-testing.
    void markDone(const std::string& finalPath);
    void markError(const std::string& err);
    void reset();               // back to Idle (clears state)

    // Caller (GL thread) reports that the candidate failed to compile, passing
    // the GLSL error log. If retries remain, this feeds the error back to the
    // model for a self-correcting re-generation and returns true (status →
    // Working). Returns false when attempts are exhausted (caller markError).
    bool retryWithError(const std::string& compileError);

    bool busy() const { return m_status.load() == Status::Working; }

private:
    // Unified generation: initial request when fixError is empty, else a
    // "fix the compile error" pass that re-sends the failed candidate + error.
    void generate(std::string fixError);

    std::atomic<Status> m_status{Status::Idle};
    std::mutex          m_mtx;
    std::string         m_resultPath;
    std::string         m_origPath;
    std::string         m_message;
    std::thread         m_thread;

    // Request context (set by request(), reused by retries).
    std::string m_shaderPath, m_instruction, m_combinePath, m_shadersDir;
    std::string m_lastCandidate;     // the most recent generated .fs text
    int         m_attempt = 0;       // 0 = initial; retries increment
    static constexpr int kMaxRetries = 2;
};
