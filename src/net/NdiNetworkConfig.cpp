#ifdef HAS_NDI
#include "net/NdiNetworkConfig.h"
#include "app/NetAdapters.h"   // SameSubnet

#include <nlohmann/json.hpp>
#include <fstream>
#include <filesystem>
#include <cstdlib>
#include <cstring>
#include <mutex>

#ifdef _WIN32
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <winsock2.h>
#include <ws2tcpip.h>
#pragma comment(lib, "ws2_32.lib")
#else
#include <sys/types.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <netdb.h>
#include <unistd.h>
#include <fcntl.h>
#include <errno.h>
#endif

using json = nlohmann::json;

namespace {

// Split "host:port" -> {host, port}. If no ':' present, port = -1.
std::pair<std::string, int> splitHostPort(const std::string& s) {
    auto pos = s.rfind(':');
    if (pos == std::string::npos) return {s, -1};
    std::string host = s.substr(0, pos);
    int port = -1;
    try { port = std::stoi(s.substr(pos + 1)); } catch (...) { port = -1; }
    return {host, port};
}

#ifdef _WIN32
void ensureWinsock() {
    static std::once_flag once;
    std::call_once(once, []() {
        WSADATA wsa{};
        WSAStartup(MAKEWORD(2, 2), &wsa);
    });
}
#endif

}  // namespace

namespace NdiNetworkConfig {

std::string configDir() {
    namespace fs = std::filesystem;
    fs::path base;
#ifdef _WIN32
    if (const char* la = std::getenv("LOCALAPPDATA")) {
        base = fs::path(la) / "Easel" / "ndi";
    } else if (const char* up = std::getenv("USERPROFILE")) {
        base = fs::path(up) / "AppData" / "Local" / "Easel" / "ndi";
    } else {
        base = fs::temp_directory_path() / "Easel" / "ndi";
    }
#elif defined(__APPLE__)
    if (const char* home = std::getenv("HOME")) {
        base = fs::path(home) / "Library" / "Application Support" / "Easel" / "ndi";
    } else {
        base = fs::temp_directory_path() / "Easel" / "ndi";
    }
#else
    if (const char* xdg = std::getenv("XDG_CONFIG_HOME")) {
        base = fs::path(xdg) / "easel" / "ndi";
    } else if (const char* home = std::getenv("HOME")) {
        base = fs::path(home) / ".config" / "easel" / "ndi";
    } else {
        base = fs::temp_directory_path() / "easel" / "ndi";
    }
#endif
    std::error_code ec;
    fs::create_directories(base, ec);
    return base.string();
}

bool applyToEnv(const NdiNetworkSettings& s) {
    const std::string dir = configDir();

    json j;
    if (s.enabled && !s.interfaceIp.empty()) {
        j["ndi"]["adapters"]["allowed"] = json::array({s.interfaceIp});
    }
    // Manual cross-subnet peer discovery + optional discovery server registry.
    j["ndi"]["networks"]["ips"] = s.extraIps;
    j["ndi"]["networks"]["discovery"] = s.useDiscoveryServer ? s.discoveryServer : std::string();

    std::filesystem::path file = std::filesystem::path(dir) / "ndi-config.v1.json";
    {
        std::ofstream out(file);
        if (!out.is_open()) return false;
        out << j.dump(2);
    }

    // NDI_CONFIG_DIR points at the FOLDER (not the file) per NDI docs. Affects
    // only this process — exactly what we want.
#ifdef _WIN32
    _putenv_s("NDI_CONFIG_DIR", dir.c_str());
#else
    setenv("NDI_CONFIG_DIR", dir.c_str(), 1);
#endif
    return true;
}

bool needsReinit(const NdiNetworkSettings& a, const NdiNetworkSettings& b) {
    if (a.enabled != b.enabled) return true;
    if (a.interfaceIp != b.interfaceIp) return true;
    if (a.extraIps != b.extraIps) return true;
    if (a.useDiscoveryServer != b.useDiscoveryServer) return true;
    if (a.discoveryServer != b.discoveryServer) return true;
    return false;
}

std::string findExtraIps(const NdiNetworkSettings& s) {
    return s.extraIps;
}

bool tcpProbe(const std::string& host, int port, int timeoutMs) {
    auto hp = splitHostPort(host);
    std::string h = hp.first;
    int p = (hp.second > 0) ? hp.second : port;
    if (h.empty() || p <= 0) return false;

#ifdef _WIN32
    ensureWinsock();
#endif

    struct addrinfo hints{};
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;
    char portStr[16];
    std::snprintf(portStr, sizeof(portStr), "%d", p);

    struct addrinfo* res = nullptr;
    if (getaddrinfo(h.c_str(), portStr, &hints, &res) != 0 || !res) return false;

    bool ok = false;
#ifdef _WIN32
    SOCKET sock = socket(res->ai_family, res->ai_socktype, res->ai_protocol);
    if (sock != INVALID_SOCKET) {
        u_long nb = 1;
        ioctlsocket(sock, FIONBIO, &nb);
        int rc = connect(sock, res->ai_addr, (int)res->ai_addrlen);
        if (rc == 0) {
            ok = true;
        } else if (WSAGetLastError() == WSAEWOULDBLOCK) {
            fd_set wfds;
            FD_ZERO(&wfds);
            FD_SET(sock, &wfds);
            timeval tv{};
            tv.tv_sec = timeoutMs / 1000;
            tv.tv_usec = (timeoutMs % 1000) * 1000;
            if (select(0, nullptr, &wfds, nullptr, &tv) > 0 && FD_ISSET(sock, &wfds)) {
                int soErr = 0;
                int len = sizeof(soErr);
                if (getsockopt(sock, SOL_SOCKET, SO_ERROR, (char*)&soErr, &len) == 0 && soErr == 0) {
                    ok = true;
                }
            }
        }
        closesocket(sock);
    }
#else
    int sock = socket(res->ai_family, res->ai_socktype, res->ai_protocol);
    if (sock >= 0) {
        int flags = fcntl(sock, F_GETFL, 0);
        fcntl(sock, F_SETFL, flags | O_NONBLOCK);
        int rc = connect(sock, res->ai_addr, res->ai_addrlen);
        if (rc == 0) {
            ok = true;
        } else if (errno == EINPROGRESS) {
            fd_set wfds;
            FD_ZERO(&wfds);
            FD_SET(sock, &wfds);
            timeval tv{};
            tv.tv_sec = timeoutMs / 1000;
            tv.tv_usec = (timeoutMs % 1000) * 1000;
            if (select(sock + 1, nullptr, &wfds, nullptr, &tv) > 0 && FD_ISSET(sock, &wfds)) {
                int soErr = 0;
                socklen_t len = sizeof(soErr);
                if (getsockopt(sock, SOL_SOCKET, SO_ERROR, &soErr, &len) == 0 && soErr == 0) {
                    ok = true;
                }
            }
        }
        close(sock);
    }
#endif
    freeaddrinfo(res);
    return ok;
}

std::vector<NdiPeerStatus> classifyPeers(
    const std::vector<std::pair<std::string, std::string>>& nameUrlPairs,
    const std::string& activeIp, const std::string& activeMask) {
    std::vector<NdiPeerStatus> out;
    out.reserve(nameUrlPairs.size());
    for (const auto& nu : nameUrlPairs) {
        NdiPeerStatus ps;
        ps.name = nu.first;
        ps.url = nu.second;
        auto hp = splitHostPort(nu.second);
        ps.ip = hp.first;
        if (ps.ip.empty()) { out.push_back(std::move(ps)); continue; }
        ps.sameSubnet = SameSubnet(ps.ip, activeIp, activeMask);
        ps.reachable = tcpProbe(ps.ip, 5960);
        out.push_back(std::move(ps));
    }
    return out;
}

}  // namespace NdiNetworkConfig

#endif // HAS_NDI
