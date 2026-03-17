#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include "speech/EthereaTranscript.h"
#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <fstream>
#include <algorithm>
#include <cctype>
#include <sstream>

#pragma comment(lib, "ws2_32.lib")

static void etLog(const std::string& msg) {
    std::ofstream f("etherea_debug.log", std::ios::app);
    f << msg << std::endl;
    f.flush();
}

// Minimal JSON string value extractor
static std::string jsonStr(const std::string& json, const std::string& key) {
    std::string needle = "\"" + key + "\"";
    size_t pos = json.find(needle);
    if (pos == std::string::npos) return "";
    pos = json.find(':', pos + needle.size());
    if (pos == std::string::npos) return "";
    // Skip whitespace after colon
    pos++;
    while (pos < json.size() && (json[pos] == ' ' || json[pos] == '\t')) pos++;
    if (pos >= json.size() || json[pos] != '"') return "";
    pos++; // skip opening quote
    std::string result;
    while (pos < json.size() && json[pos] != '"') {
        if (json[pos] == '\\' && pos + 1 < json.size()) {
            pos++;
            if (json[pos] == 'n') result += '\n';
            else if (json[pos] == 't') result += '\t';
            else if (json[pos] == 'u') {
                // Skip unicode escapes like \u23f8
                pos += 4;
            } else {
                result += json[pos];
            }
        } else {
            result += json[pos];
        }
        pos++;
    }
    return result;
}

// Helper: do a blocking HTTP GET and return body
static std::string httpGet(const std::string& host, int port, const std::string& path) {
    WSADATA wsaData;
    WSAStartup(MAKEWORD(2, 2), &wsaData);

    struct addrinfo hints = {}, *result = nullptr;
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    std::string portStr = std::to_string(port);
    if (getaddrinfo(host.c_str(), portStr.c_str(), &hints, &result) != 0) {
        WSACleanup();
        return "";
    }

    SOCKET sock = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
    if (sock == INVALID_SOCKET) { freeaddrinfo(result); WSACleanup(); return ""; }
    if (::connect(sock, result->ai_addr, (int)result->ai_addrlen) == SOCKET_ERROR) {
        closesocket(sock); freeaddrinfo(result); WSACleanup(); return "";
    }
    freeaddrinfo(result);

    std::string req = "GET " + path + " HTTP/1.1\r\nHost: " + host + "\r\nConnection: close\r\n\r\n";
    send(sock, req.c_str(), (int)req.size(), 0);

    std::string response;
    char buf[4096];
    int n;
    while ((n = recv(sock, buf, sizeof(buf) - 1, 0)) > 0) {
        buf[n] = '\0';
        response += buf;
    }
    closesocket(sock);
    WSACleanup();

    // Strip HTTP headers
    size_t headerEnd = response.find("\r\n\r\n");
    if (headerEnd != std::string::npos) return response.substr(headerEnd + 4);
    return response;
}

std::vector<EthereaSession> EthereaTranscript::fetchSessions(const std::string& baseUrl) {
    std::vector<EthereaSession> sessions;

    std::string host = "localhost";
    int port = 7860;
    {
        std::string u = baseUrl;
        if (u.substr(0, 7) == "http://") u = u.substr(7);
        size_t slashPos = u.find('/');
        std::string hostPort = (slashPos != std::string::npos) ? u.substr(0, slashPos) : u;
        size_t colonPos = hostPort.find(':');
        host = (colonPos != std::string::npos) ? hostPort.substr(0, colonPos) : hostPort;
        if (colonPos != std::string::npos) port = std::stoi(hostPort.substr(colonPos + 1));
    }

    std::string body = httpGet(host, port, "/api/sessions");
    if (body.empty()) return sessions;

    // Parse JSON array of sessions — find each {...} object
    size_t pos = 0;
    while ((pos = body.find('{', pos)) != std::string::npos) {
        // Find matching closing brace (no nesting in session objects)
        size_t end = body.find('}', pos);
        if (end == std::string::npos) break;

        std::string obj = body.substr(pos, end - pos + 1);
        pos = end + 1;

        // Must have session_id
        std::string sid = jsonStr(obj, "session_id");
        if (sid.empty()) continue;

        EthereaSession s;
        s.id = sid;

        // Parse numeric: idle_seconds
        auto parseNum = [&](const std::string& key) -> double {
            size_t kp = obj.find("\"" + key + "\"");
            if (kp == std::string::npos) return 0;
            kp = obj.find(':', kp) + 1;
            while (kp < obj.size() && obj[kp] == ' ') kp++;
            return std::atof(obj.c_str() + kp);
        };

        s.idleSeconds = (float)parseNum("idle_seconds");
        s.transcriptLength = (int)parseNum("transcript_length");

        // Parse boolean: is_paused
        size_t pp = obj.find("\"is_paused\"");
        if (pp != std::string::npos) {
            pp = obj.find(':', pp) + 1;
            while (pp < obj.size() && obj[pp] == ' ') pp++;
            s.isPaused = (obj[pp] == 't');
        }

        etLog("EthereaTranscript: session " + s.id.substr(0, 8) + " transcript=" + std::to_string(s.transcriptLength) + " paused=" + std::to_string(s.isPaused));
        sessions.push_back(s);
    }

    return sessions;
}

bool EthereaTranscript::connect(const std::string& baseUrl, const std::string& sessionId) {
    if (m_running.load()) disconnect();
    m_baseUrl = baseUrl;
    m_sessionId = sessionId;
    {
        std::lock_guard<std::mutex> lock(m_resultMutex);
        m_pendingText.clear();
        m_lastTranscript.clear();
        m_hasPending = false;
    }

    m_running.store(true);
    m_thread = std::thread(&EthereaTranscript::sseLoop, this);
    etLog("EthereaTranscript: connecting to " + baseUrl + " session=" + (sessionId.empty() ? "(default)" : sessionId));
    return true;
}

void EthereaTranscript::disconnect() {
    m_running.store(false);
    m_connected.store(false);
    if (m_thread.joinable()) {
        m_thread.join();
    }
}

void EthereaTranscript::poll() {
    std::lock_guard<std::mutex> lock(m_resultMutex);
    if (m_hasPending) {
        m_lastTranscript = m_pendingText;
        if (m_callback) m_callback(m_pendingText, false);
        m_hasPending = false;
    }
}

void EthereaTranscript::sseLoop() {
    std::string host = "localhost";
    int port = 7860;

    {
        std::string u = m_baseUrl;
        if (u.substr(0, 7) == "http://") u = u.substr(7);
        // Strip any trailing path
        size_t slashPos = u.find('/');
        std::string hostPort = (slashPos != std::string::npos) ? u.substr(0, slashPos) : u;

        size_t colonPos = hostPort.find(':');
        host = (colonPos != std::string::npos) ? hostPort.substr(0, colonPos) : hostPort;
        if (colonPos != std::string::npos) port = std::stoi(hostPort.substr(colonPos + 1));
    }

    std::string path = "/stream";
    if (!m_sessionId.empty()) {
        path += "?session_id=" + m_sessionId;
    }

    // Init winsock
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
        etLog("EthereaTranscript: WSAStartup failed");
        m_running.store(false);
        return;
    }

    while (m_running.load()) {
        // Resolve host
        struct addrinfo hints = {}, *result = nullptr;
        hints.ai_family = AF_INET;
        hints.ai_socktype = SOCK_STREAM;
        hints.ai_protocol = IPPROTO_TCP;

        std::string portStr = std::to_string(port);
        if (getaddrinfo(host.c_str(), portStr.c_str(), &hints, &result) != 0) {
            etLog("EthereaTranscript: DNS resolve failed for " + host);
            std::this_thread::sleep_for(std::chrono::seconds(3));
            continue;
        }

        SOCKET sock = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
        if (sock == INVALID_SOCKET) {
            etLog("EthereaTranscript: socket() failed");
            freeaddrinfo(result);
            std::this_thread::sleep_for(std::chrono::seconds(3));
            continue;
        }

        if (::connect(sock, result->ai_addr, (int)result->ai_addrlen) == SOCKET_ERROR) {
            etLog("EthereaTranscript: connect failed (is Etherea running on port " + std::to_string(port) + "?)");
            closesocket(sock);
            freeaddrinfo(result);
            std::this_thread::sleep_for(std::chrono::seconds(3));
            continue;
        }
        freeaddrinfo(result);

        // Send HTTP GET request
        std::string request = "GET " + path + " HTTP/1.1\r\n"
            "Host: " + host + ":" + std::to_string(port) + "\r\n"
            "Accept: text/event-stream\r\n"
            "Connection: keep-alive\r\n"
            "\r\n";
        send(sock, request.c_str(), (int)request.size(), 0);

        // Read and skip HTTP headers
        std::string buffer;
        char chunk[4096];
        bool headersSkipped = false;

        m_connected.store(true);
        etLog("EthereaTranscript: connected via TCP");

        std::string lastDisplay;

        while (m_running.load()) {
            // Use select() with timeout so we can check m_running
            fd_set readSet;
            FD_ZERO(&readSet);
            FD_SET(sock, &readSet);
            struct timeval tv;
            tv.tv_sec = 1;
            tv.tv_usec = 0;

            int sel = select(0, &readSet, nullptr, nullptr, &tv);
            if (sel == 0) continue; // timeout, check m_running
            if (sel == SOCKET_ERROR) {
                etLog("EthereaTranscript: select() error");
                break;
            }

            int bytesRead = recv(sock, chunk, sizeof(chunk) - 1, 0);
            if (bytesRead <= 0) {
                etLog("EthereaTranscript: connection closed");
                break;
            }

            chunk[bytesRead] = '\0';
            buffer += chunk;

            // Skip HTTP response headers
            if (!headersSkipped) {
                size_t headerEnd = buffer.find("\r\n\r\n");
                if (headerEnd == std::string::npos) continue;
                buffer = buffer.substr(headerEnd + 4);
                headersSkipped = true;
                etLog("EthereaTranscript: headers skipped, streaming SSE data");
            }

            // Process complete SSE messages (double newline delimited)
            size_t pos;
            while ((pos = buffer.find("\n\n")) != std::string::npos) {
                std::string message = buffer.substr(0, pos);
                buffer = buffer.substr(pos + 2);

                // Parse SSE data lines
                std::string data;
                std::istringstream stream(message);
                std::string line;
                while (std::getline(stream, line)) {
                    if (!line.empty() && line.back() == '\r') line.pop_back();
                    if (line.size() >= 5 && line.substr(0, 5) == "data:") {
                        std::string payload = line.substr(5);
                        if (!payload.empty() && payload[0] == ' ') payload = payload.substr(1);
                        data += payload;
                    }
                }

                if (data.empty() || data[0] != '{') continue;

                std::string transcript = jsonStr(data, "full_transcript");
                if (transcript.empty()) continue;

                // Skip if unchanged
                if (transcript == lastDisplay) continue;
                lastDisplay = transcript;

                // Queue for main thread — send full transcript, let Application buffer it
                {
                    std::lock_guard<std::mutex> lock(m_resultMutex);
                    m_pendingText = transcript;
                    m_hasPending = true;
                }
            }
        }

        m_connected.store(false);
        closesocket(sock);

        if (m_running.load()) {
            etLog("EthereaTranscript: reconnecting in 3s...");
            std::this_thread::sleep_for(std::chrono::seconds(3));
        }
    }

    WSACleanup();
}
