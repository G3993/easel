#include "speech/CueClient.h"

#ifdef _WIN32
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#pragma comment(lib, "ws2_32.lib")
#else
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <unistd.h>
#define SOCKET int
#define INVALID_SOCKET -1
#define SOCKET_ERROR -1
#define closesocket close
#define MAKEWORD(a,b) 0
inline int WSAStartup(int, void*) { return 0; }
inline void WSACleanup() {}
struct WSADATA { int dummy; };
#endif

#include <fstream>
#include <algorithm>
#include <cstdlib>
#include <cstring>
#include <ctime>

// ─── Logging (mirrors EthereaClient: cue_debug.log) ────────────────────────

static void cueLog(const std::string& msg) {
    // Single ws thread today, but guard the static state anyway (mirrors etLog).
    static std::mutex logMutex;
    std::lock_guard<std::mutex> lock(logMutex);
    static int suppressCount = 0;
    static std::string lastMsg;
    if (msg == lastMsg) {
        suppressCount++;
        if (suppressCount % 10 != 0) return;
    } else {
        suppressCount = 0;
        lastMsg = msg;
    }
    // Write to /tmp/ — relative paths land inside the .app bundle's Resources
    // dir on macOS, which invalidates the code signature and causes Gatekeeper
    // to refuse to launch the app on the next run.
    std::ofstream f("/tmp/cue_debug.log", std::ios::app);
    f << msg;
    if (suppressCount > 0) f << " (x" << suppressCount << ")";
    f << std::endl;
}

// ─── Minimal JSON helpers (string/bool fields and nested object slicing) ───
//
// These are deliberately permissive: Cue events nest objects (action.payload,
// observation.payload). For nested fields we slice the substring containing
// the inner object and run the same key-extractor over it.

static std::string jsonStr(const std::string& json, const std::string& key) {
    std::string needle = "\"" + key + "\"";
    size_t pos = json.find(needle);
    if (pos == std::string::npos) return "";
    pos = json.find(':', pos + needle.size());
    if (pos == std::string::npos) return "";
    pos++;
    while (pos < json.size() && (json[pos] == ' ' || json[pos] == '\t')) pos++;
    if (pos >= json.size() || json[pos] != '"') return "";
    pos++;
    std::string out;
    while (pos < json.size() && json[pos] != '"') {
        if (json[pos] == '\\' && pos + 1 < json.size()) {
            pos++;
            if (json[pos] == 'n') out += '\n';
            else if (json[pos] == 't') out += '\t';
            else if (json[pos] == 'u') pos += 4;
            else out += json[pos];
        } else {
            out += json[pos];
        }
        pos++;
    }
    return out;
}

static bool jsonBool(const std::string& json, const std::string& key) {
    std::string needle = "\"" + key + "\"";
    size_t pos = json.find(needle);
    if (pos == std::string::npos) return false;
    pos = json.find(':', pos + needle.size());
    if (pos == std::string::npos) return false;
    pos++;
    while (pos < json.size() && json[pos] == ' ') pos++;
    return (pos < json.size() && json[pos] == 't');
}

// Extract the substring of the object value for `key`. Returns "" if missing.
// Handles nested braces and quoted strings (skips embedded { and " inside strings).
static std::string jsonObj(const std::string& json, const std::string& key) {
    std::string needle = "\"" + key + "\"";
    size_t pos = json.find(needle);
    if (pos == std::string::npos) return "";
    pos = json.find(':', pos + needle.size());
    if (pos == std::string::npos) return "";
    pos++;
    while (pos < json.size() && (json[pos] == ' ' || json[pos] == '\t')) pos++;
    if (pos >= json.size() || json[pos] != '{') return "";
    size_t start = pos;
    int depth = 0;
    bool inStr = false;
    bool esc = false;
    for (; pos < json.size(); pos++) {
        char c = json[pos];
        if (inStr) {
            if (esc) esc = false;
            else if (c == '\\') esc = true;
            else if (c == '"') inStr = false;
            continue;
        }
        if (c == '"') inStr = true;
        else if (c == '{') depth++;
        else if (c == '}') {
            depth--;
            if (depth == 0) return json.substr(start, pos - start + 1);
        }
    }
    return "";
}

// ─── Base64 (WebSocket key) ─────────────────────────────────────────────────

static const char b64chars[] = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

static std::string base64Encode(const uint8_t* data, size_t len) {
    std::string out;
    for (size_t i = 0; i < len; i += 3) {
        uint32_t n = (data[i] << 16);
        if (i + 1 < len) n |= (data[i + 1] << 8);
        if (i + 2 < len) n |= data[i + 2];
        out += b64chars[(n >> 18) & 63];
        out += b64chars[(n >> 12) & 63];
        out += (i + 1 < len) ? b64chars[(n >> 6) & 63] : '=';
        out += (i + 2 < len) ? b64chars[n & 63] : '=';
    }
    return out;
}

// ─── Buffered WebSocket reader ──────────────────────────────────────────────

struct WSReader {
    SOCKET sock = INVALID_SOCKET;
    std::string buf;

    bool readExact(char* out, size_t len, int timeoutMs = 10000) {
        size_t have = 0;
        if (!buf.empty()) {
            size_t take = std::min(len, buf.size());
            memcpy(out, buf.data(), take);
            buf.erase(0, take);
            have = take;
        }
        while (have < len) {
            fd_set readSet;
            FD_ZERO(&readSet);
            FD_SET(sock, &readSet);
            struct timeval tv;
            tv.tv_sec = timeoutMs / 1000;
            tv.tv_usec = (timeoutMs % 1000) * 1000;
#ifdef _WIN32
            int sel = select(0, &readSet, nullptr, nullptr, &tv);
#else
            int sel = select(sock + 1, &readSet, nullptr, nullptr, &tv);
#endif
            if (sel <= 0) return false;
            // Chunked so the recv length always fits an int on every platform
            int want = (int)std::min<size_t>(len - have, 65536);
            int n = recv(sock, out + have, want, 0);
            if (n <= 0) return false;
            have += (size_t)n;
        }
        return true;
    }
};

struct WSFrame {
    uint8_t opcode;
    std::string payload;
    bool fin;
};

static bool wsReadFrame(WSReader& rd, WSFrame& frame) {
    uint8_t header[2];
    if (!rd.readExact((char*)header, 2)) return false;
    frame.fin = (header[0] & 0x80) != 0;
    frame.opcode = header[0] & 0x0F;
    bool masked = (header[1] & 0x80) != 0;
    uint64_t len = header[1] & 0x7F;
    if (len == 126) {
        uint8_t ext[2];
        if (!rd.readExact((char*)ext, 2)) return false;
        len = ((uint64_t)ext[0] << 8) | ext[1];
    } else if (len == 127) {
        uint8_t ext[8];
        if (!rd.readExact((char*)ext, 8)) return false;
        len = 0;
        for (int i = 0; i < 8; i++) len = (len << 8) | ext[i];
    }
    // len comes off the wire — a hostile/corrupt peer can claim up to 2^63
    // bytes and resize() would throw and kill the thread. Treat as dead link.
    constexpr uint64_t kMaxFramePayload = 4 * 1024 * 1024;
    if (len > kMaxFramePayload) return false;
    uint8_t mask[4] = {};
    if (masked && !rd.readExact((char*)mask, 4)) return false;
    frame.payload.resize((size_t)len);
    if (len > 0) {
        if (!rd.readExact(&frame.payload[0], (size_t)len)) return false;
        if (masked) {
            for (size_t i = 0; i < len; i++) frame.payload[i] ^= mask[i % 4];
        }
    }
    return true;
}

static bool wsSendFrame(SOCKET sock, uint8_t opcode, const std::string& data) {
    std::vector<uint8_t> frame;
    frame.push_back(0x80 | opcode);
    uint8_t mask[4];
    for (int i = 0; i < 4; i++) mask[i] = (uint8_t)(rand() & 0xFF);
    if (data.size() < 126) {
        frame.push_back(0x80 | (uint8_t)data.size());
    } else if (data.size() < 65536) {
        frame.push_back(0x80 | 126);
        frame.push_back((uint8_t)((data.size() >> 8) & 0xFF));
        frame.push_back((uint8_t)(data.size() & 0xFF));
    } else {
        frame.push_back(0x80 | 127);
        uint64_t len = data.size();
        for (int i = 7; i >= 0; i--) frame.push_back((uint8_t)((len >> (i * 8)) & 0xFF));
    }
    frame.insert(frame.end(), mask, mask + 4);
    for (size_t i = 0; i < data.size(); i++) frame.push_back((uint8_t)data[i] ^ mask[i % 4]);
    return send(sock, (const char*)frame.data(), (int)frame.size(), 0) == (int)frame.size();
}

// ─── CueClient ──────────────────────────────────────────────────────────────

CueClient::~CueClient() { disconnect(); }

void CueClient::parseUrl() {
    m_host = "localhost";
    m_port = 8791;
    std::string u = m_baseUrl;
    if (u.substr(0, 7) == "http://") u = u.substr(7);
    else if (u.substr(0, 5) == "ws://") u = u.substr(5);
    size_t slashPos = u.find('/');
    std::string hostPort = (slashPos != std::string::npos) ? u.substr(0, slashPos) : u;
    size_t colonPos = hostPort.find(':');
    m_host = (colonPos != std::string::npos) ? hostPort.substr(0, colonPos) : hostPort;
    if (colonPos != std::string::npos) {
        // strtol, not stoi — "host:" or "host:abc" must not throw on the main thread
        std::string portPart = hostPort.substr(colonPos + 1);
        char* end = nullptr;
        long p = strtol(portPart.c_str(), &end, 10);
        if (end != portPart.c_str() && p > 0 && p <= 65535) m_port = (int)p;
        else cueLog("CueClient: bad port in \"" + hostPort + "\", using " + std::to_string(m_port));
    }
}

bool CueClient::connect(const std::string& baseUrl, const std::string& sessionId) {
    if (m_running.load()) disconnect();
    m_baseUrl = baseUrl;
    m_sessionId = sessionId.empty() ? "easel" : sessionId;
    parseUrl();

    {
        std::lock_guard<std::mutex> lk(m_dataMutex);
        m_fullTranscript.clear();
        m_latestWords.clear();
        m_prompt.clear();
        m_lastActionType.clear();
        m_lastActionPayload.clear();
    }
    {
        std::lock_guard<std::mutex> lk(m_eventMutex);
        m_pendingTranscripts.clear();
        m_pendingActions.clear();
        m_pendingPrompts.clear();
    }

    m_running.store(true);
    m_wsThread = std::thread(&CueClient::wsLoop, this);
    cueLog("CueClient: connecting to " + baseUrl + " session=" + m_sessionId);
    return true;
}

void CueClient::disconnect() {
    m_running.store(false);
    m_wsConnected.store(false);
    // Close the live socket so select()/recv() on the ws thread unblocks
    // immediately instead of waiting for the next timeout to fire.
    uintptr_t ws = m_wsSock.exchange((uintptr_t)INVALID_SOCKET);
    if (ws != (uintptr_t)INVALID_SOCKET) {
#ifndef _WIN32
        shutdown((SOCKET)ws, SHUT_RDWR); // close() alone may not wake select() on POSIX
#endif
        closesocket((SOCKET)ws);
    }
    if (m_wsThread.joinable()) m_wsThread.join();
}

void CueClient::poll() {
    std::vector<PendingTranscript> ts;
    std::vector<PendingAction>     as;
    std::vector<PendingPrompt>     ps;
    {
        std::lock_guard<std::mutex> lk(m_eventMutex);
        ts.swap(m_pendingTranscripts);
        as.swap(m_pendingActions);
        ps.swap(m_pendingPrompts);
    }
    if (m_transcriptCb) {
        for (auto& t : ts) m_transcriptCb(t.text, t.isFinal, t.speaker);
    }
    if (m_actionCb) {
        for (auto& a : as) {
            CueAction ca{a.type, a.payload};
            m_actionCb(ca);
        }
    }
    if (m_promptCb) {
        for (auto& p : ps) m_promptCb(p.prompt, p.reset);
    }
}

void CueClient::wsLoop() {
    srand((unsigned)time(nullptr));

    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
        cueLog("CueClient WS: WSAStartup failed");
        return;
    }

    auto iSleep = [&](int ms) {
        for (int t = 0; t < ms && m_running.load(); t += 50)
            std::this_thread::sleep_for(std::chrono::milliseconds(50));
    };

    int backoff = 3;
    while (m_running.load()) {
        // Nothing thrown in here may kill the app — catch, log, reconnect.
        try {
            struct addrinfo hints = {}, *result = nullptr;
            hints.ai_family = AF_INET;
            hints.ai_socktype = SOCK_STREAM;
            std::string portStr = std::to_string(m_port);
            if (getaddrinfo(m_host.c_str(), portStr.c_str(), &hints, &result) != 0) {
                cueLog("CueClient WS: DNS failed");
                iSleep(backoff * 1000);
                backoff = std::min(backoff * 2, 60);
                continue;
            }
            SOCKET sock = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
            if (sock == INVALID_SOCKET) {
                freeaddrinfo(result);
                iSleep(backoff * 1000);
                backoff = std::min(backoff * 2, 60);
                continue;
            }
            if (::connect(sock, result->ai_addr, (int)result->ai_addrlen) == SOCKET_ERROR) {
                cueLog("CueClient WS: connect failed (is Cue server running?)");
                closesocket(sock); freeaddrinfo(result);
                iSleep(backoff * 1000);
                backoff = std::min(backoff * 2, 60);
                continue;
            }
            freeaddrinfo(result);
            backoff = 3;
            m_wsSock.store((uintptr_t)sock);

            uint8_t keyBytes[16];
            for (int i = 0; i < 16; i++) keyBytes[i] = (uint8_t)(rand() & 0xFF);
            std::string wsKey = base64Encode(keyBytes, 16);

            std::string path = "/sessions/" + m_sessionId + "/events";
            std::string request =
                "GET " + path + " HTTP/1.1\r\n"
                "Host: " + m_host + ":" + std::to_string(m_port) + "\r\n"
                "Upgrade: websocket\r\n"
                "Connection: Upgrade\r\n"
                "Sec-WebSocket-Key: " + wsKey + "\r\n"
                "Sec-WebSocket-Version: 13\r\n"
                "\r\n";
            send(sock, request.c_str(), (int)request.size(), 0);

            std::string response;
            char buf[4096];
            while (m_running.load()) {
                fd_set readSet;
                FD_ZERO(&readSet);
                FD_SET(sock, &readSet);
                struct timeval tv = { 0, 50000 }; // 50ms
#ifdef _WIN32
                int sel = select(0, &readSet, nullptr, nullptr, &tv);
#else
                int sel = select(sock + 1, &readSet, nullptr, nullptr, &tv);
#endif
                if (sel <= 0) continue;
                int n = recv(sock, buf, sizeof(buf) - 1, 0);
                if (n <= 0) break;
                buf[n] = '\0';
                response += buf;
                if (response.size() > 65536) break; // runaway pre-upgrade stream
                if (response.find("\r\n\r\n") != std::string::npos) break;
            }

            if (response.find("101") == std::string::npos) {
                cueLog("CueClient WS: upgrade failed — " + response.substr(0, 80));
                // exchange so disconnect() and this path can't both close the fd
                if (m_wsSock.exchange((uintptr_t)INVALID_SOCKET) != (uintptr_t)INVALID_SOCKET)
                    closesocket(sock);
                iSleep(3000);
                continue;
            }

            WSReader rd;
            rd.sock = sock;
            size_t headerEnd = response.find("\r\n\r\n");
            if (headerEnd != std::string::npos && headerEnd + 4 < response.size()) {
                rd.buf = response.substr(headerEnd + 4);
            }

            m_wsConnected.store(true);
            cueLog("CueClient WS: connected (" + path + ")");

            while (m_running.load()) {
                WSFrame frame;
                if (!wsReadFrame(rd, frame)) {
                    cueLog("CueClient WS: read failed");
                    break;
                }
                if (frame.opcode == 0x8) { cueLog("CueClient WS: server closed"); break; }
                if (frame.opcode == 0x9) { wsSendFrame(sock, 0xA, frame.payload); continue; }
                if (frame.opcode != 0x1) continue;

                const std::string& msg = frame.payload;
                std::string type = jsonStr(msg, "type");

                if (type == "transcript") {
                    std::string text    = jsonStr(msg, "text");
                    bool isFinal        = jsonBool(msg, "isFinal");
                    std::string speaker = jsonStr(msg, "speaker");
                    std::string fullT   = jsonStr(msg, "fullTranscript");
                    {
                        std::lock_guard<std::mutex> lk(m_dataMutex);
                        m_latestWords = text;
                        if (!fullT.empty()) m_fullTranscript = fullT;
                    }
                    {
                        std::lock_guard<std::mutex> lk(m_eventMutex);
                        m_pendingTranscripts.push_back({text, isFinal, speaker});
                    }
                } else if (type == "action") {
                    // action: { sessionId, action: { type, payload, ... } }
                    std::string actionObj = jsonObj(msg, "action");
                    std::string actType   = jsonStr(actionObj, "type");
                    std::string payload   = jsonObj(actionObj, "payload");
                    {
                        std::lock_guard<std::mutex> lk(m_dataMutex);
                        m_lastActionType    = actType;
                        m_lastActionPayload = payload;
                    }
                    {
                        std::lock_guard<std::mutex> lk(m_eventMutex);
                        m_pendingActions.push_back({actType, payload});
                    }
                } else if (type == "prompt") {
                    // prompt: { sessionId, actionType, prompt, reset, payload }
                    std::string p = jsonStr(msg, "prompt");
                    bool reset = jsonBool(msg, "reset");
                    if (!p.empty()) {
                        {
                            std::lock_guard<std::mutex> lk(m_dataMutex);
                            m_prompt = p;
                        }
                        std::lock_guard<std::mutex> lk(m_eventMutex);
                        m_pendingPrompts.push_back({p, reset});
                    }
                }
                // ready/state.snapshot/observation/actions/vision.description/signal
                // /output.available/source.available/output.status — pass-through (ignored).
            }

            m_wsConnected.store(false);
            // exchange so disconnect() and this path can't both close the fd
            if (m_wsSock.exchange((uintptr_t)INVALID_SOCKET) != (uintptr_t)INVALID_SOCKET)
                closesocket(sock);
            if (m_running.load()) {
                cueLog("CueClient WS: disconnected, reconnecting...");
                iSleep(backoff * 1000);
                backoff = std::min(backoff * 2, 60);
            }
        } catch (const std::exception& e) {
            cueLog(std::string("CueClient WS: exception — ") + e.what());
        } catch (...) {
            cueLog("CueClient WS: unknown exception");
        }
        // The exception path skips the normal teardown — make sure the socket
        // is closed and a pause happens before retrying (no-op on clean exit).
        uintptr_t leaked = m_wsSock.exchange((uintptr_t)INVALID_SOCKET);
        if (leaked != (uintptr_t)INVALID_SOCKET) {
            m_wsConnected.store(false);
            closesocket((SOCKET)leaked);
            iSleep(3000);
        }
    }

    WSACleanup();
}
